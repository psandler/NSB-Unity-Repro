using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Unity;
using NSB.Unity.Repro.Services;
using NServiceBus;

namespace NSB.Unity.Repro.Test
{
    public static class Bootstrap
    {
        public static void StartBus(IUnityContainer container)
        {
            var configuration = new BusConfiguration();
            configuration.UseSerialization<XmlSerializer>();
            configuration.PurgeOnStartup(true);
            configuration.EndpointName("NSB.Unity.Repro");
            configuration.UseContainer<UnityBuilder>(customizations => customizations.UseExistingContainer(container));
            configuration.UseTransport<MsmqTransport>();
            configuration.UsePersistence<InMemoryPersistence>();
            configuration.EnableInstallers();

            var transactionSettings = configuration.Transactions();
            transactionSettings.Enable();
            transactionSettings.DisableDistributedTransactions();
            transactionSettings.DoNotWrapHandlersExecutionInATransactionScope();

            var conventionsBuilder = configuration.Conventions();
            conventionsBuilder.DefiningEventsAs(x => x.Namespace != null
                                                     && x.Namespace.StartsWith("NSB")
                                                     && x.Namespace.EndsWith("Events"));
            conventionsBuilder.DefiningCommandsAs(x => x.Namespace != null
                                                       && x.Namespace.StartsWith("NSB")
                                                       && x.Namespace.EndsWith("Commands"));

            var bus = Bus.Create(configuration);
            bus.Start();
        }

        public static IUnityContainer BootstrapContainer()
        {
            var container = new UnityContainer();
            AutoRegisterTypes(container);
            container.RegisterType<INamedRegService, NamedRegService1>("1");
            container.RegisterType<INamedRegService, NamedRegService2>("2");
            container.RegisterType<INamedRegService, NamedRegService3>("3");
            return container;
        }

        private static void AutoRegisterTypes(IUnityContainer container)
        {
            var types = new List<Type>();

            // Assemblies to participate in ioc registration
            // All interfaces and implementations in these assemblies ...
            // ... will be registered unless exlcuded by a filter
            var business = Assembly.Load("NSB.Unity.Repro.Services");
            types.AddRange(business.GetTypes());

            foreach (var theInterface in types.Where(t => t.IsInterface))
            {
                var @interface = theInterface;
                var assignableType = types.Where(t => @interface.IsAssignableFrom(t) && t != @interface);
                foreach (var type in assignableType.Where(type => !container.IsRegistered(type)))
                {
                    Debug.WriteLine("Auto-Registering {0} : {1} with Unity", type, @interface);
                    //unity uses transient lifetime by default
                    container.RegisterType(theInterface, type);
                }
            }
        }
    }
}