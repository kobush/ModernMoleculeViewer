using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using MetroLog;
using Microsoft.Practices.Unity;
using ModernMoleculeViewer.ViewModels;
using Prism.Logging;
using Prism.Unity.Windows;

namespace ModernMoleculeViewer
{
    sealed partial class App : PrismUnityApplication
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override ILoggerFacade CreateLogger()
        {
            return base.CreateLogger();
        }

        protected override void OnRegisterKnownTypesForSerialization()
        {
            SessionStateService.RegisterKnownType(typeof(MoleculePageParams));
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager);
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
#if DEBUG
            if (Debugger.IsAttached)
                DebugSettings.EnableFrameRateCounter = true;
#endif
            NavigationService.Navigate("Main", null);

            return Task.FromResult<object>(null);
        }
    }
}
