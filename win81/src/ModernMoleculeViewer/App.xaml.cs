using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using MetroLog;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Microsoft.Practices.Unity;

namespace ModernMoleculeViewer
{
    sealed partial class App : MvvmAppBase
    {
        private UnityContainer _container;
        private EventAggregator _eventAggregator;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

#if DEBUG
            if (Debugger.IsAttached)
                DebugSettings.EnableFrameRateCounter = true;
#endif

            LogManagerFactory.DefaultConfiguration.IsEnabled = true;

            // setup the global crash handler...
            GlobalCrashHandler.Configure();
        }

        public IUnityContainer Container
        {
            get { return _container; }
        }

        public IEventAggregator EventAggregator
        {
            get { return _eventAggregator; }
        }

        protected override Task OnLaunchApplication(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate("Main", null);

            // no async work
            return Task.FromResult<object>(null);
        }

        protected override void OnInitialize(IActivatedEventArgs args)
        {
            base.OnInitialize(args);

            ConfigureContainer();
        }

        private void ConfigureContainer()
        {
            _container = new UnityContainer();
            _eventAggregator = new EventAggregator();

            _container.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager);
            _container.RegisterInstance<IUnityContainer>(Container);
            _container.RegisterInstance<ISessionStateService>(SessionStateService);
            _container.RegisterInstance<INavigationService>(NavigationService);
            _container.RegisterInstance<IEventAggregator>(EventAggregator);

            ViewModelLocator.SetDefaultViewModelFactory(Resolve);
        }

        protected override object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
