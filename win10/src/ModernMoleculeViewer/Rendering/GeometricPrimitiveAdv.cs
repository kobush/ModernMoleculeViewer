using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace ModernMoleculeViewer.Rendering
{
    public class GeometricPrimitiveAdv<T> : Component where T : struct
    {
        private static readonly VertexInputLayout InputLayout = VertexInputLayout.New<T>(0);
        
        protected readonly GraphicsDevice GraphicsDevice;

        private readonly Buffer _indexBuffer;
        private readonly Buffer<T> _vertexBuffer;
        private readonly bool _isIndex32Bit;

        public GeometricPrimitiveAdv(GraphicsDevice graphicsDevice, T[] vertices, int[] indices, bool toLeftHanded = false)
        {
            this.GraphicsDevice = graphicsDevice;

            if (toLeftHanded)
                ReverseWinding(vertices, indices);

            _isIndex32Bit = true;
            _indexBuffer = ToDispose<Buffer<int>>(Buffer.Index.New<int>(graphicsDevice, indices, ResourceUsage.Immutable));
            _vertexBuffer = ToDispose<Buffer<T>>(Buffer.Vertex.New<T>(graphicsDevice, vertices, ResourceUsage.Immutable));
        }
        
        public GeometricPrimitiveAdv(GraphicsDevice graphicsDevice, T[] vertices, short[] indices, bool toLeftHanded = false)
        {
            this.GraphicsDevice = graphicsDevice;

            if (toLeftHanded)
                ReverseWinding(vertices, indices);

            _indexBuffer = ToDispose<Buffer<short>>(Buffer.Index.New<short>(graphicsDevice, indices, ResourceUsage.Immutable));
            _vertexBuffer = ToDispose<Buffer<T>>(Buffer.Vertex.New<T>(graphicsDevice, vertices, ResourceUsage.Immutable));
        }

        protected void ReverseWinding<TIndex>(T[] vertices, TIndex[] indices)
        {
            int index = 0;
            while (index < indices.Length)
            {
                Utilities.Swap<TIndex>(ref indices[index], ref indices[index + 2]);
                index += 3;
            }
        }

        public Buffer IndexBuffer
        {
            get { return _indexBuffer; }
        }

        public Buffer<T> VertexBuffer
        {
            get { return _vertexBuffer; }
        }

        public void Draw(Effect effect = null)
        {
            Draw(GraphicsDevice, effect);
        }

        public void Draw(GraphicsDevice graphicsDevice, Effect effect = null)
        {
            if (effect != null)
            {
                EffectPassCollection passes = effect.CurrentTechnique.Passes;
                int count = passes.Count;
                for (int index = 0; index < count; ++index)
                    Draw(graphicsDevice, passes[index]);
            }
            else
                Draw(graphicsDevice, (EffectPass)null);
        }

        public void Draw(GraphicsDevice graphicsDevice, EffectPass pass)
        {
            Draw(graphicsDevice, PrimitiveType.TriangleList, pass);
        }

        public void Draw(GraphicsDevice graphicsDevice, PrimitiveType primitiveType, EffectPass pass)
        {
            if (pass != null)
                pass.Apply();

            graphicsDevice.SetVertexBuffer(0, _vertexBuffer, 0);
            graphicsDevice.SetVertexInputLayout(InputLayout);
            graphicsDevice.SetIndexBuffer(_indexBuffer, _isIndex32Bit, 0);
            graphicsDevice.DrawIndexed(primitiveType, _indexBuffer.ElementCount, 0, 0);
        }
    }
}