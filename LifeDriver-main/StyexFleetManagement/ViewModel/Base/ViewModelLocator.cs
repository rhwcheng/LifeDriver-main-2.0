using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using StyexFleetManagement.Services;
using StyexFleetManagement.Salus.Services;
using TinyIoC;
using Xamarin.Forms;

namespace StyexFleetManagement.ViewModel.Base
{
    public static class ViewModelLocator
    {
        private static readonly TinyIoCContainer Container;

        public static readonly BindableProperty AutoWireViewModelProperty =
            BindableProperty.CreateAttached("AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), default(bool),
                propertyChanged: OnAutoWireViewModelChanged);

        public static bool GetAutoWireViewModel(BindableObject bindable)
        {
            return (bool) bindable.GetValue(ViewModelLocator.AutoWireViewModelProperty);
        }

        public static void SetAutoWireViewModel(BindableObject bindable, bool value)
        {
            bindable.SetValue(ViewModelLocator.AutoWireViewModelProperty, value);
        }

        public static bool UseMockService { get; set; }

        static ViewModelLocator()
        {
            Container = new TinyIoCContainer();

            // View models - by default, TinyIoC will register concrete classes as multi-instance.
            RegisterSingleton<IAmberAlertViewModel, AmberAlertViewModel>();
            RegisterSingleton<IBeaconTaggingViewModel, BeaconTaggingViewModel>();
            RegisterSingleton<IGuardianAngelViewModel, GuardianAngelViewModel>();
            RegisterSingleton<ISosViewModel, SosViewModel>();

            // Services - by default, TinyIoC will register interface registrations as singletons.
            Container.Register<IAuthenticationService, AuthenticationService>();
            Container.Register<IDeviceService, DeviceService>();
            Container.Register<IEventService, EventService>();
            Container.Register<ILocationUpdateService, LocationUpdateService>();
            Container.Register<IServerDetailsService, ServerDetailsService>();
            Container.Register<IPermissionsService, PermissionsService>();
        }

        public static void UpdateDependencies()
        {
            //_container.Register<ICatalogService, CatalogService>();

            UseMockService = false;
        }

        public static void RegisterSingleton<TInterface, T>() where TInterface : class where T : class, TInterface
        {
            Container.Register<TInterface, T>().AsSingleton();
        }

        public static void Register<TInterface, T>(T instance) where TInterface : class where T : class, TInterface
        {
            Container.Register<TInterface, T>(instance);
        }

        public static T Resolve<T>() where T : class
        {
            return Container.Resolve<T>();
        }

        private static void OnAutoWireViewModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as Element;
            if (view == null)
            {
                return;
            }

            var viewType = view.GetType();
            var viewName = viewType.FullName.Replace(".Views.", ".ViewModels.");
            var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
            var viewModelName =
                string.Format(CultureInfo.InvariantCulture, "{0}Model, {1}", viewName, viewAssemblyName);

            var viewModelType = Type.GetType(viewModelName);
            if (viewModelType == null)
            {
                return;
            }

            var viewModel = Container.Resolve(viewModelType);
            view.BindingContext = viewModel;
        }
    }
}