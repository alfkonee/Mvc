// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.ConfigurationModel;

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
        /// Adds services that allows controllers to be activated from the application's <see cref="System.IServiceProvider"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configuration">The applications <see cref="IConfiguration"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection WithControllersFromServiceProvider(
            [NotNull] this IServiceCollection services,
            IConfiguration configuration = null)
        {
            var describer = new ServiceDescriber(configuration);
            services.Add(describer.Transient<IControllerActivator, ServiceBasedControllerActivator>());
            return services;
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
