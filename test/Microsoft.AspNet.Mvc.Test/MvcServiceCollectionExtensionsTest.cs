// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class MvcServiceCollectionExtensionsTest
    {
        [Fact]
        public void WithControllersFromServiceProvider_AddsTypesToControllerTypeProviderAndServiceCollection()
        {
            // Arrange
            var collection = new ServiceCollection();
            var controllerTypes = new[] { typeof(ControllerTypeA), typeof(ControllerTypeB) };

            // Act
            MvcServiceCollectionExtensions.WithControllersFromServiceProvider(collection,
                                                                              controllerTypes);

            // Assert
            var services = collection.ToList();
            Assert.Equal(4, services.Count);
            Assert.Equal(typeof(IControllerActivator), services[0].ServiceType);
            Assert.Equal(typeof(ServiceBasedControllerActivator), services[0].ImplementationType);
            Assert.Equal(LifecycleKind.Transient, services[0].Lifecycle);

            Assert.Equal(typeof(ControllerTypeA), services[1].ServiceType);
            Assert.Equal(typeof(ControllerTypeA), services[1].ImplementationType);
            Assert.Equal(LifecycleKind.Transient, services[1].Lifecycle);

            Assert.Equal(typeof(ControllerTypeB), services[2].ServiceType);
            Assert.Equal(typeof(ControllerTypeB), services[2].ImplementationType);
            Assert.Equal(LifecycleKind.Transient, services[2].Lifecycle);

            Assert.Equal(typeof(IControllerTypeProvider), services[3].ServiceType);
            var typeProvider = Assert.IsType<StaticControllerTypeProvider>(services[3].ImplementationInstance);
            Assert.Equal(controllerTypes, typeProvider.GetControllerTypes());
            Assert.Equal(LifecycleKind.Singleton, services[3].Lifecycle);
        }

        [Fact]
        public void WithControllersFromServiceProvider_UsesConfigurationIfSpecified()
        {
            // Arrange
            var collection = new ServiceCollection();
            var controllerTypes = new[] { typeof(ControllerTypeA), typeof(ControllerTypeB) };
            var configuration = new Configuration();
            configuration.Add(new MemoryConfigurationSource());
            configuration.Set(typeof(IControllerActivator).FullName,
                              typeof(CustomActivator).AssemblyQualifiedName);
            configuration.Set(typeof(IControllerTypeProvider).FullName,
                              typeof(CustomTypeProvider).AssemblyQualifiedName);

            // Act
            MvcServiceCollectionExtensions.WithControllersFromServiceProvider(collection,
                                                                              controllerTypes,
                                                                              configuration);

            // Assert
            var services = collection.ToList();
            Assert.Equal(4, services.Count);
            Assert.Equal(typeof(IControllerActivator), services[0].ServiceType);
            Assert.Equal(typeof(CustomActivator), services[0].ImplementationType);
            Assert.Equal(LifecycleKind.Transient, services[0].Lifecycle);

            Assert.Equal(typeof(ControllerTypeA), services[1].ServiceType);
            Assert.Equal(typeof(ControllerTypeA), services[1].ImplementationType);
            Assert.Equal(LifecycleKind.Transient, services[1].Lifecycle);

            Assert.Equal(typeof(ControllerTypeB), services[2].ServiceType);
            Assert.Equal(typeof(ControllerTypeB), services[2].ImplementationType);
            Assert.Equal(LifecycleKind.Transient, services[2].Lifecycle);

            Assert.Equal(typeof(IControllerTypeProvider), services[3].ServiceType);
            Assert.Equal(typeof(CustomTypeProvider), services[3].ImplementationType);
            Assert.Equal(LifecycleKind.Singleton, services[3].Lifecycle);
        }

        private class ControllerTypeA
        {

        }

        private class ControllerTypeB
        {

        }

        private class CustomActivator : IControllerActivator
        {
            public object Create(ActionContext context, Type controllerType)
            {
                throw new NotImplementedException();
            }
        }

        public class CustomTypeProvider : IControllerTypeProvider
        {
            public IEnumerable<TypeInfo> GetControllerTypes()
            {
                throw new NotImplementedException();
            }
        }
    }
}