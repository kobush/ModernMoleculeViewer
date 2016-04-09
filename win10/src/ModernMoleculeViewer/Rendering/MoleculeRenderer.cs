using System;
using System.Collections.Generic;
using ModernMoleculeViewer.Model;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Content;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using Matrix = SharpDX.Matrix;

namespace ModernMoleculeViewer.Rendering
{
    public class MoleculeRenderer : GameSystem
    {
        private struct InstanceData
        {
            public InstanceData(Matrix instanceWorld, Color4 instanceColor)
            {
                World = instanceWorld;
                DiffuseColor = instanceColor;
            }

            public Color4 DiffuseColor;

            public Matrix World;

            public static VertexElement[] GetInputLayout()
            {
                return new VertexElement[]
                {
                    new VertexElement("COLOR", 0, Format.R32G32B32A32_Float, 0),
                    new VertexElement("WORLD", 0, Format.R32G32B32A32_Float, 16),
                    new VertexElement("WORLD", 1, Format.R32G32B32A32_Float, 32),
                    new VertexElement("WORLD", 2, Format.R32G32B32A32_Float, 48),
                    new VertexElement("WORLD", 3, Format.R32G32B32A32_Float, 64)
                };
            }
        }

        private readonly Molecule _molecule;
        
        private Matrix _moleculeTransform;

     //   private GeometricPrimitive _atomSphere;
        
        private GeometricPrimitiveAdv<VertexPositionNormalTexture> _atomSphereAdv;
        private GeometricPrimitiveAdv<VertexPositionNormalTexture> _shortStick;
        private GeometricPrimitiveAdv<VertexPositionNormalTexture> _longStick;

        private BasicEffect _basicEffect;
        private AdvancedEffect _advancedEffect;

        private bool _updateColorSchema;
        private VertexInputLayout _instancedVertexBufferLayout;
        private Buffer<InstanceData> _backboneInstanceBuffer;
        private Buffer<InstanceData> _fullChainBondsInstanceBuffer;
        private Buffer<InstanceData> _watersInstanceBuffer;
        
        private PerspectiveCamera _cameraService;

        public MoleculeRenderer(Game game, Molecule molecule)
            :base(game)
        {
            if (molecule == null) 
                throw new ArgumentNullException(nameof(molecule));

            Game.GameSystems.Add(this);
            Enabled = true;
            Visible = true;

            _molecule = molecule;
            _molecule.ColorSchemeChanged += Molecule_ColorSchemeChanged;

            CreateMoleculeTransform();
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            this.Game.GameSystems.Remove(this);

            base.Dispose(disposeManagedResources);
        }

        public override void Initialize()
        {
            base.Initialize();

            _cameraService = Services.GetService<PerspectiveCamera>();
        }

        private void Molecule_ColorSchemeChanged(object sender, EventArgs e)
        {
            _updateColorSchema = true;
        }

        /// <summary>
        /// Called by the constructor to calculate the default scale and translation of the
        /// molecule based on the bounding box of the atoms.
        /// </summary>
        private void CreateMoleculeTransform()
        {
            BoundingBox bounds = Atom.GetBounds(_molecule.Atoms);
            Vector3 center = (bounds.Maximum + bounds.Minimum)/2f;

            Vector3 size = bounds.Maximum - bounds.Minimum;
            float scale = 2.5f/Math.Max(size.X, Math.Max(size.Y, size.Z));

            _moleculeTransform = Matrix.Translation(-center)*Matrix.Scaling(scale);
        }

        public void LoadContent(ContentManager contentManager)
        {
            _basicEffect = ToDisposeContent(new BasicEffect(GraphicsDevice));
            _basicEffect.EnableDefaultLighting();
            
            //_advancedEffect = ToDispose(new AdvancedEffect(contentManager));
            _advancedEffect = ToDisposeContent(Content.Load<AdvancedEffect>("Advanced"));
            _advancedEffect.EnableDefaultLighting();

            //_atomSphere = GeometricPrimitive.Sphere.New(GraphicsDevice, 1, 8);
            _atomSphereAdv = SphereBuilder.New(GraphicsDevice);
            _shortStick = Stick.New(GraphicsDevice);
            _longStick = Stick.New(GraphicsDevice, capOffset: 0.1f);

            foreach (var residue in _molecule.Residues)
            {
                if (residue.Ribbon != null)
                    residue.Cartoon = new Cartoon(residue, residue.ResidueColor, GraphicsDevice);
            }

            _instancedVertexBufferLayout = VertexInputLayout.New(
                VertexBufferLayout.New(0, VertexElement.FromType<VertexPositionNormalTexture>(), 0), 
                VertexBufferLayout.New(1, InstanceData.GetInputLayout(), 1));

            UpdateBackbone();
            UpdateFullChain();
            UpdateWaterAtoms();
        }

        public override void Update(GameTime gameTime)
        {
            var view = _cameraService.GetViewMatrix();
            var projection = _cameraService.GetProjectionMatrix(GraphicsDevice.Viewport);

            _basicEffect.View = view;
            _basicEffect.Projection = projection;

            _advancedEffect.Technique = AdvancedRenderTechnique.PerVertexLighting;
            _advancedEffect.View = view;
            _advancedEffect.Projection = projection;

            if (_updateColorSchema)
            {
                UpdateBackbone();
                UpdateFullChain();
                UpdateWaterAtoms();

                _updateColorSchema = false;
            }
        }

        private void UpdateWaterAtoms()
        {
            var instanceData = new List<InstanceData>();
            foreach (var atom in _molecule.Atoms)
            {
                var water = atom as Water;
                if (water != null)
                {
                    instanceData.Add(
                        new InstanceData(
                            Matrix.Scaling(0.2f) * 
                            Matrix.Translation(water.Position) * 
                            _moleculeTransform,
                            water.Color));
                }
            }

            if (_watersInstanceBuffer == null)
            {
                _watersInstanceBuffer =
                    ToDispose(Buffer.New(GraphicsDevice, instanceData.ToArray(), BufferFlags.VertexBuffer, ResourceUsage.Dynamic));
            }
            else
            {
                _watersInstanceBuffer.SetData(instanceData.ToArray());
            }
        }

        private void UpdateBackbone()
        {
            var instanceData = new List<InstanceData>();

            foreach (var atom in _molecule.Atoms)
            {
                var cAlpha = atom as CAlpha;
                if (cAlpha != null)
                {
                    if (cAlpha.PreviousCAlpha != null)
                    {
                        var distance = (cAlpha.Position - cAlpha.PreviousCAlpha.Position).Length();
                        AddBondStick(instanceData, cAlpha, cAlpha.PreviousCAlpha, distance);
                    }
                    if (cAlpha.NextCAlpha != null)
                    {
                        var distance = (cAlpha.Position - cAlpha.NextCAlpha.Position).Length();
                        AddBondStick(instanceData, cAlpha, cAlpha.NextCAlpha, distance);
                    }
                }
            }

            if (_backboneInstanceBuffer == null)
            {
                _backboneInstanceBuffer =
                    ToDispose(Buffer.New(GraphicsDevice, instanceData.ToArray(), BufferFlags.VertexBuffer, ResourceUsage.Dynamic));
            }
            else
            {
                _backboneInstanceBuffer.SetData(instanceData.ToArray());
            }
        }

        private void UpdateFullChain()
        {
            var instanceData = new List<InstanceData>();

            foreach (var atom in _molecule.Atoms)
            {
                ChainAtom chainAtom = atom as ChainAtom;
                if (chainAtom != null && 
                    chainAtom.Bonds.Count > 0)
                {
                    foreach (KeyValuePair<Atom, float> pair in atom.Bonds)
                    {
                        var otherAtom = pair.Key;
                        var distance = pair.Value;

                        AddBondStick(instanceData, atom, otherAtom, distance);
                    }
                }
            }

            if (_fullChainBondsInstanceBuffer == null)
            {
                _fullChainBondsInstanceBuffer =
                    ToDispose(Buffer.New(GraphicsDevice, instanceData.ToArray(), BufferFlags.VertexBuffer,
                        ResourceUsage.Dynamic));
            }
            else
            {
                _fullChainBondsInstanceBuffer.SetData(instanceData.ToArray());
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            _advancedEffect.Technique = AdvancedRenderTechnique.PerPixelLighting;

            foreach (var atom in _molecule.Atoms)
            {

                if (atom is ChainAtom)
                {
                    /*if (_molecule.ShowFullChain || atom.ShowAsSelected)
                        DrawAtom(atom);*/

                    if (atom is CAlpha)
                    {
/*                        if (_molecule.ShowBackbone)
                        {
                            var cAlpha = (CAlpha) atom;
                            if (cAlpha.PreviousCAlpha != null)
                            {
                                var distance = (cAlpha.Position - cAlpha.PreviousCAlpha.Position).Length();
                                DrawBondStick(cAlpha, cAlpha.PreviousCAlpha, distance);
                            }
                            if (cAlpha.NextCAlpha != null)
                            {
                                var distance = (cAlpha.Position - cAlpha.NextCAlpha.Position).Length();
                                DrawBondStick(cAlpha, cAlpha.NextCAlpha, distance);
                            }
                        }*/
                    }
                }
                else if (atom is HetAtom)
                {
                    if (_molecule.ShowHetAtoms || atom.ShowAsSelected)
                        DrawAtom(atom);
                }
                else if (atom is Water)
                {
                    /*if (_molecule.ShowWaters)
                        DrawAtom(atom);*/
                }

            }

            if (_molecule.ShowCartoon)
            {
                DrawResidues();
            }

            _advancedEffect.Technique = AdvancedRenderTechnique.PerPixelLightingInstanced;

            if (_molecule.ShowWaters)
            {
                DrawInstanced(_atomSphereAdv, _watersInstanceBuffer, _advancedEffect);
            }

            if (_molecule.ShowFullChain)
            {
                DrawInstanced(_shortStick, _fullChainBondsInstanceBuffer, _advancedEffect);
            }

            if (_molecule.ShowBackbone)
            {
                DrawInstanced(_longStick, _backboneInstanceBuffer, _advancedEffect);
            }
        }

        private void DrawInstanced(GeometricPrimitiveAdv<VertexPositionNormalTexture> mesh, 
            Buffer<InstanceData> instanceBuffer, Effect effect)
        {
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                if (pass == null) continue;

                pass.Apply();

                GraphicsDevice.SetVertexInputLayout(_instancedVertexBufferLayout);
                GraphicsDevice.SetIndexBuffer(mesh.IndexBuffer, false);
                GraphicsDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                GraphicsDevice.SetVertexBuffer(1, instanceBuffer);
                GraphicsDevice.DrawIndexedInstanced(PrimitiveType.TriangleList, mesh.IndexBuffer.ElementCount, instanceBuffer.ElementCount);
            }
        }

        private void DrawResidues()
        {
            var orgSpecularColor = _advancedEffect.SpecularColor;
            var orgSpecularPower = _advancedEffect.SpecularPower;

            _advancedEffect.Technique = AdvancedRenderTechnique.PerPixelLighting;
            _advancedEffect.SpecularColor = (Vector3)new Color(255, 255, 255, 192);
            _advancedEffect.SpecularPower = 50;

            foreach (var residue in _molecule.Residues)
            {
                if (residue.Cartoon != null && residue.Cartoon.Mesh != null)
                {
                    _advancedEffect.World = _moleculeTransform;
                    _advancedEffect.DiffuseColor = residue.Color;

                    residue.Cartoon.Mesh.Draw(GraphicsDevice, _advancedEffect);
                }
            }
            _advancedEffect.SpecularColor = orgSpecularColor;
            _advancedEffect.SpecularPower = orgSpecularPower;
        }

        private void DrawAtom(Atom atom)
        {
            if (atom.Bonds.Count == 0)
            {
                // Draws the 3D model for a small ball that is used to represent an unbonded atom since
                // no sticks will be connected.
                _advancedEffect.DiffuseColor = atom.Color;
                _advancedEffect.World = Matrix.Scaling(0.2f) * Matrix.Translation(atom.Position) * _moleculeTransform;

                _atomSphereAdv.Draw(GraphicsDevice, _advancedEffect);
                //_atomSphere.Draw(GraphicsDevice, _advancedEffect.CurrentTechnique.Passes[0]);
            }
            else
            {
                foreach (KeyValuePair<Atom, float> pair in atom.Bonds)
                {
                    var otherAtom = pair.Key;
                    var distance = pair.Value;

                    DrawBondStick(atom, otherAtom, distance);
                }
            }

            if (atom.ShowAsSelected)
            {
                _basicEffect.DiffuseColor = (Vector4)atom.Color;
                _basicEffect.World = Matrix.Scaling(0.4f) * Matrix.Translation(atom.Position) * _moleculeTransform;

                _atomSphereAdv.Draw(GraphicsDevice, _advancedEffect);
                //_atomSphere.Draw(_basicEffect);
            }
        }


        private void AddBondStick(ICollection<InstanceData> instanceData, Atom atom, Atom otherAtom, float distance)
        {
            var instance = new InstanceData();

            var transform = GetBondStickTransform(atom, otherAtom, distance);
            instance.World = transform * _moleculeTransform;
            //instance.World = Matrix.Identity * Matrix.Scaling(20);
            instance.DiffuseColor = atom.Color;

            instanceData.Add(instance);
        }

        private void DrawBondStick(Atom atom, Atom otherAtom, float distance)
        {
            var transform = GetBondStickTransform(atom, otherAtom, distance);
            _advancedEffect.World = transform * _moleculeTransform;
            _advancedEffect.DiffuseColor = atom.Color;

            var stickModel = distance > 2 ? _longStick : _shortStick;
            stickModel.Draw(GraphicsDevice, _advancedEffect);
        }

        private static Matrix GetBondStickTransform(Atom atom, Atom otherAtom, float distance)
        {
            Matrix transform = Matrix.Identity;
            transform *= Matrix.Scaling(distance/2f, 1, 1);

            Vector3 orientationVector = new Vector3(1, 0, 0);
            Vector3 differenceVector = otherAtom.Position - atom.Position;
            differenceVector.Normalize();

            Vector3 axis = Vector3.Cross(orientationVector, differenceVector);
            if (axis.LengthSquared() > 0)
            {
                //float angle = (float)Math.Asin(axis.Length());
                float angle = Helper.AngleBetween(orientationVector, differenceVector);

                axis.Normalize();
                transform *= Matrix.RotationAxis(axis, angle);
            }

            transform *= Matrix.Translation(atom.Position);
            return transform;
        }
    }


}