﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Bit.Core.Contracts;
using Bit.Core.Implementations;
using Bit.Core.Models.Events;
using Bit.View;
using Bit.ViewModel;
using Bit.ViewModel.Contracts;
using Bit.ViewModel.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Prism;
using Prism.Autofac;
using Prism.Events;
using Prism.Ioc;
using Prism.Logging;
using Prism.Navigation;
using Prism.Plugin.Popups;
using Prism.Services;
using Rg.Plugins.Popup.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

#if !UWP
[assembly: XmlnsDefinition("https://bitframework.com", "Bit", AssemblyName = "Bit.Client.Xamarin.Prism")]
[assembly: XmlnsDefinition("https://bitframework.com", "Bit.View", AssemblyName = "Bit.Client.Xamarin.Prism")]
#endif

namespace Bit
{
    public abstract class BitApplication : PrismApplication
    {
        private readonly Lazy<IEventAggregator> _eventAggregator = default!;

        /// <summary>
        /// To be called in shared/net-standard project.
        /// https://docs.microsoft.com/bg-bg/xamarin/xamarin-forms/xaml/custom-namespace-schemas#consuming-a-custom-namespace-schema
        /// </summary>
        public static void XamlInit()
        {

        }

        public BitApplication()
            : this(null)
        {

        }

        protected BitApplication(IPlatformInitializer? platformInitializer = null)
            : base(platformInitializer)
        {
            _eventAggregator = new Lazy<IEventAggregator>(() =>
            {
                return Container.Resolve<IEventAggregator>();
            }, isThreadSafe: true);

            if (MainPage == null)
                MainPage = new ContentPage { Title = "DefaultPage" };
        }

        protected sealed override async void OnInitialized()
        {
            try
            {
                Container.Resolve<IEnumerable<ITelemetryService>>().All().LogPreviousSessionCrashIfAny();
                var deviceService = Container.Resolve<IDeviceService>();
                DependencyDelegates.Current.StartTimer = (interval, callback) => deviceService.StartTimer(interval, () => callback());
                DependencyDelegates.Current.GetNavigationUriPath = () => Current.NavigationService.CurrentPageNavService.GetNavigationUriPath();
                await OnInitializedAsync();
                await Task.Yield();
            }
            catch (Exception exp)
            {
                BitExceptionHandler.Current.OnExceptionReceived(exp);
            }
        }

        protected virtual Task OnInitializedAsync()
        {
            Container.Resolve<IConnectivity>().ConnectivityChanged += (sender, e) =>
            {
                _eventAggregator.Value.GetEvent<ConnectivityChangedEvent>()
                    .Publish(new ConnectivityChangedEvent { IsConnected = e.NetworkAccess != Xamarin.Essentials.NetworkAccess.None });
            };

            return Task.CompletedTask;
        }

        public new INavService NavigationService => (PrismNavigationService == null ? null : Container.Resolve<INavServiceFactory>()(PrismNavigationService, Container.Resolve<IPopupNavigation>()))!;

        public static new BitApplication Current => (PrismApplicationBase.Current as BitApplication)!;

        public INavigationService PrismNavigationService => base.NavigationService;

        protected sealed override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ContainerBuilder containerBuilder = containerRegistry.GetBuilder();

            IServiceCollection services = new BitServiceCollection();

            containerBuilder.Properties[nameof(services)] = services;
            containerBuilder.Properties[nameof(containerRegistry)] = containerRegistry;

            AutofacDependencyManager dependencyManager = new AutofacDependencyManager();
            dependencyManager.UseContainerBuilder(containerBuilder);
            ((IServiceCollectionAccessor)dependencyManager).ServiceCollection = services;

            RegisterTypes(dependencyManager, containerRegistry, containerBuilder, services);

            containerBuilder.Populate(services);
        }

        protected virtual void RegisterTypes(IDependencyManager dependencyManager, IContainerRegistry containerRegistry, ContainerBuilder containerBuilder, IServiceCollection services)
        {
            dependencyManager.Register<ILoggerFacade, BitPrismLogger>();
            dependencyManager.RegisterUsing(resolver => Container, lifeCycle: DependencyLifeCycle.SingleInstance, overwriteExisting: false);
            dependencyManager.RegisterUsing(resolver => Container.GetContainer(), lifeCycle: DependencyLifeCycle.SingleInstance, overwriteExisting: false);
            BitCSharpClientControls.UseBitPopupNavigation();
            containerRegistry.RegisterPopupNavigationService();
            //containerRegistry.RegisterPopupDialogService();
        }
    }

    public class BitServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {

    }
}
