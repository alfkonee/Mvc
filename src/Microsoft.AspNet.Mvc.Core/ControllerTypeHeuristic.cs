// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Core
{
    /// <summary>
    /// Provides heuristics to determine if a type is a controller.
    /// </summary>
    public static class ControllerTypeHeuristics
    {
        /// <summary>
        /// Uses heuristics to identify controller types in the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The sequence of see <see cref="Assembly"/> to scan.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <returns></returns>
        public static IEnumerable<TypeInfo> GetControllers([NotNull] IEnumerable<Assembly> assemblies,
                                                           [NotNull] ILogger logger)
        {
            var types = assemblies.SelectMany(a => a.DefinedTypes);
            return types.Where(type => IsController(type, logger));
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="typeInfo"/> is a controller. Otherwise <c>false</c>.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <returns><c>true</c> if the <paramref name="typeInfo"/> is a controller. Otherwise <c>false</c>.</returns>
        public static bool IsController([NotNull] TypeInfo typeInfo,
                                        [NotNull] ILogger logger)
        {
            var status = ControllerStatus.IsController;

            if (!typeInfo.IsClass)
            {
                status |= ControllerStatus.IsNotAClass;
            }
            if (typeInfo.IsAbstract)
            {
                status |= ControllerStatus.IsAbstract;
            }
            // We only consider public top-level classes as controllers. IsPublic returns false for nested
            // classes, regardless of visibility modifiers
            if (!typeInfo.IsPublic)
            {
                status |= ControllerStatus.IsNotPublicOrTopLevel;
            }
            if (typeInfo.ContainsGenericParameters)
            {
                status |= ControllerStatus.ContainsGenericParameters;
            }
            if (typeInfo.Name.Equals("Controller", StringComparison.OrdinalIgnoreCase))
            {
                status |= ControllerStatus.NameIsController;
            }
            if (!typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) &&
                !typeof(Controller).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                status |= ControllerStatus.DoesNotEndWithControllerAndIsNotAssignable;
            }

            if (logger.IsEnabled(LogLevel.Verbose))
            {
                logger.WriteVerbose(new IsControllerValues(
                    typeInfo.AsType(),
                    status));
            }

            return status == ControllerStatus.IsController;
        }
    }
}