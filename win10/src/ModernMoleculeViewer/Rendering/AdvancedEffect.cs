using System;
using Prism.Mvvm;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace ModernMoleculeViewer.Rendering
{
    public class AdvancedEffect : Effect, IEffectMatrices
    {

        private EffectParameter _paramWorld;
        private EffectParameter _paramView;
        private EffectParameter _paramProjection;
        private EffectParameter _paramWorldViewProjection;
        private EffectParameter _paramEyePosition;
        private EffectParameter _paramDirToLightX;
        private EffectParameter _paramDirToLightY;
        private EffectParameter _paramDirToLightZ;
        private EffectParameter _paramDirLightColorR;
        private EffectParameter _paramDirLightColorG;
        private EffectParameter _paramDirLightColorB;
        private EffectParameter _paramDirSpecColorR;
        private EffectParameter _paramDirSpecColorG;
        private EffectParameter _paramDirSpecColorB;
        private EffectParameter _paramDiffuseColor;
        private EffectParameter _paramSpecularColor;
        private EffectParameter _paramSpecularPower;
        private EffectParameter _paramAmbientDown;
        private EffectParameter _paramAmbientRange;

        private bool _perFrameChanged;
        private Matrix _world;
        private Matrix _view;
        private Matrix _projection;
        private Vector3 _eyePosition;

        private bool _lightChanged;
        private Color4 _diffuseColor;
        private Color3 _ambientDown;
        private Color3 _ambientUp;
        private Color3 _specularColor = Color3.White;
        private float _specularPower = 16;
        private float _gamma = 1f;

        private readonly DirectionalLight1 _directionalLight0;
        private readonly DirectionalLight1 _directionalLight1;
        private readonly DirectionalLight1 _directionalLight2;
        private readonly DirectionalLight1 _directionalLight3;

        private AdvancedRenderTechnique _technique;

        public AdvancedEffect(GraphicsDevice graphicsDevice, EffectData effectData)
            : base(graphicsDevice, effectData)
        {
            // apply once
            _lightChanged = true;

            _directionalLight0 = new DirectionalLight1();
            _directionalLight0.PropertyChanged += (sender, args) => _lightChanged = true;
            _directionalLight1 = new DirectionalLight1();
            _directionalLight1.PropertyChanged += (sender, args) => _lightChanged = true;
            _directionalLight2 = new DirectionalLight1();
            _directionalLight2.PropertyChanged += (sender, args) => _lightChanged = true;
            _directionalLight3 = new DirectionalLight1();
            _directionalLight3.PropertyChanged += (sender, args) => _lightChanged = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        
            _paramWorld = Parameters["World"];
            _paramView = Parameters["View"];
            _paramProjection = Parameters["Projection"];
            _paramWorldViewProjection = Parameters["WorldViewProjection"];
            _paramEyePosition = Parameters["EyePosition"];

            _paramDirToLightX = Parameters["DirToLightX"];
            _paramDirToLightY = Parameters["DirToLightY"];
            _paramDirToLightZ = Parameters["DirToLightZ"];
            _paramDirLightColorR = Parameters["DirLightColorR"];
            _paramDirLightColorG = Parameters["DirLightColorG"];
            _paramDirLightColorB = Parameters["DirLightColorB"];
            _paramDirSpecColorR = Parameters["DirSpecColorR"];
            _paramDirSpecColorG = Parameters["DirSpecColorG"];
            _paramDirSpecColorB = Parameters["DirSpecColorB"];

            _paramDiffuseColor = Parameters["DiffuseColor"];
            _paramSpecularColor = Parameters["SpecularColor"];
            _paramSpecularPower = Parameters["SpecularPower"];
            _paramAmbientDown = Parameters["AmbientDown"];
            _paramAmbientRange = Parameters["AmbientRange"];
        }

        public Matrix World
        {
            get { return _world; }
            set
            {
                _world = value;
                _perFrameChanged = true;
            }
        }

        public Matrix View
        {
            get { return _view; }
            set
            {
                _view = value;
                _perFrameChanged = true;
            }
        }

        public Matrix Projection
        {
            get { return _projection; }
            set
            {
                _projection = value;
                _perFrameChanged = true;
            }
        }

        public Vector3 EyePosition
        {
            get { return _eyePosition; }
            set
            {
                _eyePosition = value;
                _perFrameChanged = true;
            }
        }

        protected override EffectPass OnApply(EffectPass pass)
        {
            if (_perFrameChanged)
            {
                _paramWorld.SetValue(_world);
                _paramView.SetValue(_view);
                _paramProjection.SetValue(_projection);
                _paramWorldViewProjection.SetValue(_world*_view*_projection);
                _paramEyePosition.SetValue(_eyePosition);
                _perFrameChanged = false;
            }

            if (_lightChanged)
            {
                SetDirLightColorParams(
                    GammaCorrection(_directionalLight0.DiffuseColor),
                    GammaCorrection(_directionalLight1.DiffuseColor),
                    GammaCorrection(_directionalLight2.DiffuseColor),
                    GammaCorrection(_directionalLight3.DiffuseColor));

                SetDirSpecColorParams(
                    GammaCorrection(_directionalLight0.SpecularColor),
                    GammaCorrection(_directionalLight1.SpecularColor),
                    GammaCorrection(_directionalLight2.SpecularColor),
                    GammaCorrection(_directionalLight3.SpecularColor));

                SetDirLightDirectionParams(_directionalLight0.Direction, _directionalLight1.Direction,
                    _directionalLight2.Direction, _directionalLight3.Direction);

                _paramAmbientDown.SetValue(GammaCorrection(_ambientDown));
                _paramAmbientRange.SetValue(GammaCorrection(_ambientUp) - GammaCorrection(_ambientDown));
                _paramSpecularColor.SetValue(GammaCorrection(_specularColor));
                _paramSpecularPower.SetValue(_specularPower);

                // this one is squared in shader
                _paramDiffuseColor.SetValue(DiffuseColor);

                _lightChanged = false;
            }

            return base.OnApply(pass);
        }


        private void SetDirLightDirectionParams(Vector3 d0, Vector3 d1, Vector3 d2, Vector3 d3)
        {
            d0.Normalize();
            d1.Normalize();
            d3.Normalize();

            _paramDirToLightX.SetValue(new Vector4(-d0.X, -d1.X, -d2.X, -d3.X));
            _paramDirToLightY.SetValue(new Vector4(-d0.Y, -d1.Y, -d2.Y, -d3.Y));
            _paramDirToLightZ.SetValue(new Vector4(-d0.Z, -d1.Z, -d2.Z, -d3.Z));
        }

        private void SetDirLightColorParams(Vector3 c0, Vector3 c1, Vector3 c2, Vector3 c3)
        {
            _paramDirLightColorR.SetValue(new Vector4(c0.X, c1.X, c2.X, c3.X));
            _paramDirLightColorG.SetValue(new Vector4(c0.Y, c1.Y, c2.Y, c3.Y));
            _paramDirLightColorB.SetValue(new Vector4(c0.Z, c1.Z, c2.Z, c3.Z));
        }
        private void SetDirSpecColorParams(Vector3 c0, Vector3 c1, Vector3 c2, Vector3 c3)
        {
            _paramDirSpecColorR.SetValue(new Vector4(c0.X, c1.X, c2.X, c3.X));
            _paramDirSpecColorG.SetValue(new Vector4(c0.Y, c1.Y, c2.Y, c3.Y));
            _paramDirSpecColorB.SetValue(new Vector4(c0.Z, c1.Z, c2.Z, c3.Z));
        }

        private Vector3 GammaCorrection(Vector3 color)
        {
            if (Math.Abs(Gamma - 1.0) < 0.0001)
                return color;

            return new Vector3((float)Math.Pow(color.X, Gamma), (float)Math.Pow(color.Y, Gamma), (float)Math.Pow(color.Z, Gamma));
        }

        public Vector3 AmbientDown
        {
            get { return _ambientDown; }
            set
            {
                _ambientDown = value;
                _lightChanged = true;
            }
        }

        public Vector3 AmbientUp
        {
            get { return _ambientUp; }
            set
            {
                _ambientUp = value;
                _lightChanged = true;
            }
        }

        public DirectionalLight1 DirectionalLight0
        {
            get { return _directionalLight0; }
        }

        public DirectionalLight1 DirectionalLight1
        {
            get { return _directionalLight1; }
        }

        public DirectionalLight1 DirectionalLight2
        {
            get { return _directionalLight2; }
        }

        public DirectionalLight1 DirectionalLight3
        {
            get { return _directionalLight3; }
        }

        public float Gamma
        {
            get { return _gamma; }
            set
            {
                _gamma = value;
                _lightChanged = true;
            }
        }

        public Color4 DiffuseColor
        {
            get { return _diffuseColor; }
            set
            {
                _diffuseColor = value;
                _lightChanged = true;
            }
        }

        public Color3 SpecularColor
        {
            get { return _specularColor; }
            set
            {
                if (_specularColor == value) return;
                _specularColor = value;
                _lightChanged = true;
            }
        }

        public float SpecularPower
        {
            get { return _specularPower; }
            set
            {
                if (_specularPower.Equals(value)) return;
                _specularPower = value;
                _lightChanged = true;
            }
        }

        public AdvancedRenderTechnique Technique
        {
            get { return _technique; }
            set
            {
                if (_technique != value)
                {
                    _technique = value;
                    CurrentTechnique = Techniques[(int) Technique];
                }
            }
        }

        public void EnableDefaultLighting()
        {
            AmbientDown = AmbientUp = new Color3(0.05333332f, 0.09882354f, 0.1819608f);

            DirectionalLight0.Direction = new Vector3(-0.5265408f, 0.6275069f, -0.5735765f);
            DirectionalLight0.DiffuseColor = new Color3(1f, 0.9607844f, 0.8078432f);
            DirectionalLight0.SpecularColor = new Color3(1f, 0.9607844f, 0.8078432f);
            DirectionalLight0.Enabled = true;

            DirectionalLight1.Direction = new Vector3(0.7198464f, -0.6040227f, 0.3420201f);
            DirectionalLight1.DiffuseColor = new Color3(0.9647059f, 0.7607844f, 0.4078432f);
            DirectionalLight1.SpecularColor = Color3.Black;
            DirectionalLight1.Enabled = true;

            DirectionalLight2.Direction = new Vector3(0.4545195f, -0.4545195f, -0.7660444f);
            DirectionalLight2.DiffuseColor = new Color3(0.3231373f, 0.3607844f, 0.3937255f);
            DirectionalLight2.SpecularColor = new Color3(0.3231373f, 0.3607844f, 0.3937255f);
            DirectionalLight2.Enabled = true;

            DirectionalLight3.DiffuseColor = Color3.Black;
            DirectionalLight3.SpecularColor = Color3.Black;
            DirectionalLight3.Enabled = false;
        }

        public DirectionalLight1 GetLight(int i)
        {
            switch (i)
            {
                case 0:
                    return DirectionalLight0;
                case 1:
                    return DirectionalLight1;
                case 2:
                    return DirectionalLight2;
                case 3:
                    return DirectionalLight2;
                default:
                    return null;
            }
        }
    }

    public enum AdvancedRenderTechnique
    {
        NoLighting = 0,
        PerVertexLighting,
        PerPixelLighting,
        PerPixelLightingInstanced
    }

    public class DirectionalLight1 : BindableBase
    {
        private bool _enabled;
        private Vector3 _direction;
        private Color3 _diffuseColor;
        private Color3 _specularColor;

        public bool Enabled
        {
            get { return _enabled; }
            set { SetProperty(ref _enabled, value); }
        }

        public Vector3 Direction
        {
            get { return _direction; }
            set { SetProperty(ref _direction, value); }
        }

        public Color3 DiffuseColor
        {
            get { return _diffuseColor; }
            set { SetProperty(ref _diffuseColor, value); }
        }

        public Color3 SpecularColor
        {
            get { return _specularColor; }
            set { SetProperty(ref _specularColor, value); }
        }

        public void Copy(DirectionalLight directionalLight)
        {
            Enabled = directionalLight.Enabled;
            DiffuseColor = directionalLight.DiffuseColor;
            SpecularColor = directionalLight.SpecularColor;
            Direction = directionalLight.Direction;
        }
    }
}