using System;
using Windows.UI.Xaml;
using ModernMoleculeViewer.Rendering;
using ModernMoleculeViewer.ViewModels;

namespace ModernMoleculeViewer.Views
{
    public sealed partial class MoleculePage
    {
        private TouchCameraController _touchCameraController;

        public MoleculePage()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _touchCameraController = new TouchCameraController();
            _touchCameraController.Viewport = SwapChain;
            _touchCameraController.Camera = ViewModel.Engine.Camera;


            ViewModel.Engine.Run(SwapChain);
        }

        public MoleculePageViewModel ViewModel
        {
            get { return (MoleculePageViewModel) DataContext; }
        }
    }
}
