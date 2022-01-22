﻿using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace Bytewizer.TinyCLR.Utilities
{
    /// <summary>
    /// Creates a stream that helps dividing data in fixed size chunks.
    /// </summary>
    public class ChunkedStream : Stream
    {
        private readonly MemoryStream _memoryStream = new MemoryStream();
        private readonly object _memoryStreamLock = new object();

        private readonly int _chunkSize;
        private readonly ReadyAction _chunkReadyAction;

        public delegate byte[] ReadyAction(byte[] buffer);

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkedStream"/> class.
        /// </summary>
        /// <param name="chunkSize">The chunk size.</param>
        /// <param name="chunkReadyAction">The <see cref="Action"/> to perform when a chunk is ready.</param>
        public ChunkedStream(int chunkSize, ReadyAction chunkReadyAction)
        {
            _chunkSize = chunkSize;
            _chunkReadyAction = chunkReadyAction;
        }

        /// <summary>
        /// Overrides <see cref="Stream.Flush"/> so that no action is performed.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>Calling this method will raise a <see cref="NotSupportedException"/>.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="origin">The origin.</param>
        /// <returns>Calling this method will raise a <see cref="NotSupportedException"/>.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes a block of bytes to the current stream using data read from a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_memoryStreamLock)
            {
                _memoryStream.Write(buffer, offset, count);

                if (_memoryStream.Position >= _chunkSize)
                {
                    SendChunks();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value>Always false as <see cref="ChunkedStream"/> doesn't support reading.</value>
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value>Always false as <see cref="ChunkedStream"/> doesn't support seeking.</value>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value>true if the stream supports writing; otherwise, false.</value>
        public override bool CanWrite
        {
            get
            {
                return _memoryStream.CanWrite;
            }
        }

        /// <summary>
        /// This property is not supported.
        /// </summary>
        /// <value>The length.</value>
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// This property is not supported.
        /// </summary>
        /// <value>The position.</value>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Closes the current stream and releases all resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_memoryStreamLock)
                {
                    if (_memoryStream.Position > 0)
                    {
                        SendChunks();
                    }

                    _memoryStream.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void SendChunks()
        {
            var buffer = new byte[_chunkSize];

            _memoryStream.Position = 0;

            var bytesRead = _memoryStream.Read(buffer, 0, _chunkSize);

            do
            {
                var workBuffer = new byte[bytesRead];

                Array.Copy(buffer, 0, workBuffer, 0, bytesRead);

                _chunkReadyAction(workBuffer);

                bytesRead = _memoryStream.Read(buffer, 0, _chunkSize);
            }
            while (bytesRead == _chunkSize);

            _memoryStream.Position = 0;
            _memoryStream.SetLength(0);

            if (bytesRead > 0)
            {
                _memoryStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}
