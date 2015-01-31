// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Logging;

namespace Microsoft.Framework.DependencyInjection
{
    public static class MvcServiceCollectionExtensions
    {
        public static IServiceCollection AddMvc([NotNull] this IServiceCollection services)
        {
            return AddMvc(services, configuration: null);
        }

        public static IServiceCollection AddMvc(
            [NotNull] this IServiceCollection services,
            IConfiguration configuration)
        {
            ConfigureDefaultServices(services, configuration);
            services.TryAdd(MvcServices.GetDefaultServices(configuration));
            return services;
        }

        /// <summary>
        /// Configures a set of <see cref="MvcOptions"/> for the application.
        /// </summary>
        /// <param name="services">The services available in the application.</param>
        /// <param name="setupAction">The <see cref="MvcOptions"/> which need to be configured.</param>
        public static void ConfigureMvcOptions(
            [NotNull] this IServiceCollection services,
            [NotNull] Action<MvcOptions> setupAction)
        {
            services.Configure(setupAction);
        }

        /// <summary>
        /// Register the specified <paramref name="controllerTypes"/> as controller types in the application
        /// and as services in the <paramref name="services"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="controllerTypes">A sequence of controller <see cref="Type"/>s to register in the <paramref name="services"/>
        /// and used for controller discovery.</param>
        public static IServiceCollection WithControllersFromServiceProvider(
           [NotNull] this IServiceCollection services,
           [NotNull] IEnumerable<Type> controllerTypes)
        {
            return WithControllersFromServiceProvider(services, controllerTypes, configuration: null);
        }

        /// <summary>
        /// Register the specified <paramref name="controllerTypes"/> as controller types in the application
        /// and as services in the <paramref name="services"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="controllerTypes">A sequence of controller <see cref="Type"/>s to register in the <paramref name="services"/>
        /// and used for controller discovery.</param>
        /// <param name="configuration">The application's <see cref="IConfiguration"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection WithControllersFromServiceProvider(
            [NotNull] this IServiceCollection services,
            [NotNull] IEnumerable<Type> controllerTypes,
            IConfiguration configuration)
        {
            var describer = new ServiceDescriber(configuration);
            services.TryAdd(describer.Transient<IControllerActivator, ServiceBasedControllerActivator>());

            var controllerTypeInfos = new List<TypeInfo>();
            foreach (var type in controllerTypes)
            {
                services.AddTransient(type);
                controllerTypeInfos.Add(type.GetTypeInfo());
            }

            var controllerTypeProvider = new StaticControllerTypeProvider(controllerTypeInfos);
            services.Add(describer.Instance<IControllerTypeProvider>(controllerTypeProvider));
            return services;
        }

        /// <summary>
        /// Registers controller types from the specified <paramref name="assemblies"/> in the application
        /// and as services in the <paramref name="services"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="controllerAssemblies">Assemblies to scan for controllers.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <returns></returns>
        public static IServiceCollection WithControllersFromServiceProvider(
            [NotNull] this IServiceCollection services,
            [NotNull] IEnumerable<Assembly> controllerAssemblies)
        {
            return WithControllersFromServiceProvider(services,
                                                      controllerAssemblies,
                                                      logger: null,
                                                      configuration: null);
        }

        /// <summary>
        /// Registers controller types from the specified <paramref name="assemblies"/> in the application
        /// and as services in the <paramref name="services"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="controllerAssemblies">Assemblies to scan for controllers.</param>
        /// <param name="configuration">The application's <see cref="IConfiguration"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <returns></returns>
        public static IServiceCollection WithControllersFromServiceProvider(
            [NotNull] this IServiceCollection services,
            [NotNull] IEnumerable<Assembly> controllerAssemblies,
            IConfiguration configuration,
            ILogger logger)
        {
            if (logger == null)
            {
                logger = NullLogger.Instance;
            }

            var controllerTypes = ControllerTypeHeuristics.GetControllers(controllerAssemblies, logger);
            return WithControllersFromServiceProvider(services, 
                                                      controllerTypes.Select(type => type.AsType()), 
                                                      configuration);
        }

        private static void ConfigureDefaultServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions(configuration);
            services.AddDataProtection(configuration);
            services.AddRouting(configuration);
            services.AddScopedInstance(configuration);
            services.AddAuthorization(configuration);
            services.Configure<RouteOptions>(routeOptions =>
                                                    routeOptions.ConstraintMap
                                                         .Add("exists",
                                                              typeof(KnownRouteValueConstraint)));

        }
    }
}
