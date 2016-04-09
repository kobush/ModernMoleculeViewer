using System;
using System.Collections.Generic;
using ModernMoleculeViewer.Model;
using SharpDX;
using SharpDX.Toolkit;

namespace ModernMoleculeViewer.Rendering
{
    public class MoleculeViewerEngine : Game
    {
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly PerspectiveCamera _camera;

        private Color4 _backgroundColor;
        private MoleculeRenderer _moleculeRenderer;
        
        private bool _isContentLoaded;

        private Vector3Animation _positionAnimation;
        private Vector3Animation _targetAnimation;
        private Vector3Animation _upVectorAnimation;

        private readonly List<Vector3Animation> _animations = new List<Vector3Animation>();

        public MoleculeViewerEngine()
        {
            // Creates a graphics manager. This is mandatory.
            _graphicsDeviceManager = new GraphicsDeviceManager(this);

            _graphicsDeviceManager.PreferMultiSampling = true;
//            _graphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
            _graphicsDeviceManager.PreparingDeviceSettings += GraphicsDeviceManagerOnPreparingDeviceSettings;

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";

            // configure camera
            _camera = new PerspectiveCamera();
            _camera.IsRightHanded = true;
            _camera.FieldOfView = MathUtil.DegreesToRadians(45); 
            _camera.UpDirection = Vector3.UnitY;
            _camera.EyePosition = new Vector3(0,0,4);
            _camera.NearPlane = 0.125f;
            _camera.FarPlane = 100f;

            Services.AddService(_camera);

            _backgroundColor = Color.Black;
        }

        private void GraphicsDeviceManagerOnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            //e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = MSAALevel.X2;
        }

        public PerspectiveCamera Camera
        {
            get { return _camera; }
        }

        public Color4 BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }


        public void Show(Molecule molecule)
        {
            if (_moleculeRenderer != null)
                RemoveAndDispose(ref _moleculeRenderer);

            if (molecule != null)
            {
                _moleculeRenderer = ToDisposeContent(new MoleculeRenderer(this, molecule));

                if (_isContentLoaded)
                    _moleculeRenderer.LoadContent(Content);

                // zoom on model
                _camera.EyePosition = new Vector3(0,0,25);
                _camera.LookAt = new Vector3(0);
                _camera.UpDirection = Vector3.UnitY;

                AnimateCamera(new Vector3(0,0,4), _camera.LookAt, _camera.UpDirection, TimeSpan.FromSeconds(1.5));
            }
        }

        protected override void LoadContent()
        {
            _isContentLoaded = true;

            if (_moleculeRenderer != null)
                _moleculeRenderer.LoadContent(Content);

            base.LoadContent();
        }

        public void ResetCameraView()
        {
            AnimateCamera(new Vector3(0, 0, 4), new Vector3(0, 0, 0), Vector3.UnitY, TimeSpan.FromSeconds(1.5));
        }

        private void AnimateCamera(Vector3 newPosition, Vector3 newTarget, Vector3 newUpVector, TimeSpan duration)
        {
            _animations.Clear();

            _animations.Add(new Vector3Animation(_camera.EyePosition, newPosition, duration, v => _camera.EyePosition = v));
            _animations.Add(new Vector3Animation(_camera.LookAt, newTarget, duration, v => _camera.LookAt = v));
            _animations.Add(new Vector3Animation(_camera.UpDirection, newUpVector, duration, v=>_camera.UpDirection = v));
        }

        protected override void Update(GameTime gameTime)
        {
            if (_animations.Count > 0)
            {
                bool allCompleted = true;
                foreach (var animation in _animations)
                {
                    animation.Update(gameTime);
                    if (!animation.IsCompleted)
                        allCompleted = false;
                }
                if (allCompleted)
                    _animations.Clear();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clears the screen with the Color.CornflowerBlue
            //GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.Stencil | ClearOptions.DepthBuffer, BackgroundColor, 1f, 0);
            GraphicsDevice.Clear(BackgroundColor);

            //_testCube.Draw(_effect);
            //GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.AlphaBlend);

            base.Draw(gameTime);
        }
    }
}
