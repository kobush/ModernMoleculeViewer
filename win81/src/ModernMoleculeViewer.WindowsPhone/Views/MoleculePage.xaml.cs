using System;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ModernMoleculeViewer.Rendering;
using ModernMoleculeViewer.ViewModels;

namespace ModernMoleculeViewer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MoleculePage
    {
        private TouchCameraController _touchCameraController;

        public MoleculePage()
        {
            this.InitializeComponent();

            ViewModel = new MoleculePageViewModel();

            HardwareButtons.BackPressed += OnBackPressed;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _touchCameraController = new TouchCameraController();
            _touchCameraController.Viewport = SwapChain;
            _touchCameraController.Camera = ViewModel.Engine.Camera;

            ViewModel.Engine.Run(SwapChain);
        }

        public MoleculePageViewModel ViewModel
        {
            get { return (MoleculePageViewModel)DataContext; }
            set { DataContext = value; }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ViewModel.OnNavigatedTo(e.Parameter);

            // Set the desired bounds on the application view to use the core window, i.e., the entire screen (including app bar and status bar
            var applicationView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            applicationView.SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // restore previous size
            var applicationView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            applicationView.SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseVisible);
        }

        private void OnBackPressed(object sender, BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                e.Handled = true;
            }
        }
    }
}
