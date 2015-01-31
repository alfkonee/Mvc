// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A <see cref="IControllerTypeProvider"/> that identifies controller types from assemblies
    /// specified by the registered <see cref="IAssemblyProvider"/>.
    /// </summary>
    public class DefaultControllerTypeProvider : IControllerTypeProvider
    {
        private readonly IAssemblyProvider _assemblyProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultControllerTypeProvider"/>.
        /// </summary>
        /// <param name="assemblyProvider"><see cref="IAssemblyProvider"/> that provides assemblies to look for
        /// controllers in.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public DefaultControllerTypeProvider(IAssemblyProvider assemblyProvider,
                                             ILoggerFactory loggerFactory)
        {
            _assemblyProvider = assemblyProvider;
            _logger = loggerFactory.Create<DefaultControllerTypeProvider>();
        }

        /// <inheritdoc />
        public IEnumerable<TypeInfo> GetControllerTypes()
        {
            var assemblies = _assemblyProvider.CandidateAssemblies;
            if (_logger.IsEnabled(LogLevel.Verbose))
            {
                foreach (var assembly in assemblies)
                {
                    _logger.WriteVerbose(new AssemblyValues(assembly));
                }
            }

            return ControllerTypeHeuristics.GetControllers(assemblies, _logger);
        }
    }
}