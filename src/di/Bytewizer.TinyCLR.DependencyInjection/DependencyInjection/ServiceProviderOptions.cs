﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

#if NanoCLR
namespace Bytewizer.NanoCLR.DependencyInjection
#else
namespace Bytewizer.TinyCLR.DependencyInjection
#endif
{
    /// <summary>
    /// Options for configuring various behaviors of the default <see cref="IServiceProvider"/> implementation.
    /// </summary>
    public class ServiceProviderOptions
    {
        // Avoid allocating objects in the default case
        internal static readonly ServiceProviderOptions Default = new ServiceProviderOptions();

        /// <summary>
        /// <c>true</c> to perform check verifying that all services can be created during <c>BuildServiceProvider</c> call; otherwise <c>false</c>. Defaults to <c>false</c>.
        /// NOTE: this check doesn't verify open generics services.
        /// </summary>
        public bool ValidateOnBuild { get; set; }
    }
}