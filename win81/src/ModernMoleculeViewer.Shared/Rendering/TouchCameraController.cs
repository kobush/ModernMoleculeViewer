using System.Diagnostics;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using SharpDX;

namespace ModernMoleculeViewer.Rendering
{
    public class TouchCameraController
    {
        private PerspectiveCamera _camera;
        private FrameworkElement _viewport;

        private bool _isCtrlPressed;
        private int _numContacts;
        private bool _allowRotation = true;
        private bool _inertialPan;

        public PerspectiveCamera Camera
        {
            get { return _camera; }
            set
            {
                if (_camera == value) return;
                _camera = value;
            }
        }

        public bool AllowRotation
        {
            get { return _allowRotation; }
            set { _allowRotation = value; }
        }

        public FrameworkElement Viewport
        {
            get { return _viewport; }
            set
            {
                if (_viewport == value) return;

                // detach
                if (_viewport != null)
                {
                    _viewport.KeyDown -= ViewPort_KeyDown;
                    _viewport.KeyUp -= ViewPort_KeyUp;
                    
                    _viewport.PointerPressed -= ViewPort_PointerPressed;
                    _viewport.PointerReleased -= ViewPort_PointerReleased;
                    _viewport.PointerWheelChanged -= ViewPort_PointerWheel;

                    _viewport.ManipulationStarting -= Viewport_ManupulationStarting;
                    _viewport.ManipulationStarted -= Viewport_ManupulationStarted;
                    _viewport.ManipulationDelta -= Viewport_ManupulationDelta;
                    _viewport.ManipulationCompleted -= Viewport_ManupulationCompleted;
                    _viewport.ManipulationMode = ManipulationModes.None;
                }
                _viewport = value;

                // attach
                if (_viewport != null)
                {
                    _viewport.KeyDown += ViewPort_KeyDown;
                    _viewport.KeyUp += ViewPort_KeyUp;
                    
                    _viewport.PointerPressed += ViewPort_PointerPressed;
                    _viewport.PointerReleased += ViewPort_PointerReleased;
                    _viewport.PointerWheelChanged += ViewPort_PointerWheel;

                    _viewport.ManipulationStarting += Viewport_ManupulationStarting;
                    _viewport.ManipulationStarted += Viewport_ManupulationStarted;
                    _viewport.ManipulationDelta += Viewport_ManupulationDelta;
                    _viewport.ManipulationCompleted += Viewport_ManupulationCompleted;
                    _viewport.ManipulationMode = ManipulationModes.All;
                }
            }
        }

        private void ViewPort_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _numContacts++;
        }

        private void ViewPort_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _numContacts--;
            if (_numContacts < 0)
                _numContacts = 0;
        }

        private void ViewPort_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
                _isCtrlPressed = true;

        }

        private void ViewPort_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
                _isCtrlPressed = false;
        }

        private void Viewport_ManupulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            e.Mode = ManipulationModes.All;
            e.Handled = true;
        }

        private void Viewport_ManupulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void ViewPort_PointerWheel(object sender, PointerRoutedEventArgs e)
        {
            // handle mouse wheel scrolling
            var delta = e.GetCurrentPoint(Viewport).Properties.MouseWheelDelta;
            Camera.Zoom(-delta / 120f * 0.1f);
        }

        private void Viewport_ManupulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var rSize = Viewport.RenderSize;

            /*CoreVirtualKeyStates ctrl = Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Control);
            _isCtrlPressed = (ctrl == CoreVirtualKeyStates.Down);*/

            //handle pinch - zoom or z-axis pan depending on the Ctrl button state
            if (_isCtrlPressed)
            {
                Camera.Pan(new Vector2(0f), Camera.CalcZPan(e.Delta.Scale), rSize);
            }
            else
            {
                Camera.Scale(e.Delta.Scale);
            }

            var vPan = new Vector2(
                (float)(-e.Delta.Translation.X / rSize.Width),
                (float)(e.Delta.Translation.Y / rSize.Height));

            //spherical pan if 1 finger, pan if 2+ fingers
            if (_numContacts >= 2 || (e.IsInertial && _inertialPan))
            {
                Camera.Pan(vPan, 0, rSize);
                _inertialPan = true;
            }
            else
            {
                Camera.SphericalPan(vPan);
                _inertialPan = false;
            }

            if (AllowRotation)
                Camera.Rotate(MathUtil.DegreesToRadians(-e.Delta.Rotation));

            e.Handled = true;
        }

        private void Viewport_ManupulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}