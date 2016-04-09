using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace ModernMoleculeViewer.Rendering
{
    public struct Stick
    {
        public static GeometricPrimitiveAdv<VertexPositionNormalTexture> New(GraphicsDevice device, float radius = 0.2f,
            float capOffset = 0.2f, int divisions = 10, bool toLeftHanded = false)
        {
            short ic1 = (short) (2*divisions);
            short ic2 = (short) (ic1 + 1);

            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<short>();

            for (int division = 0; division < divisions; division++)
            {
                float theta = 2*MathUtil.Pi*division/divisions;

                float z = radius*(float) Math.Cos(theta);
                float y = radius*(float) Math.Sin(theta);

                vertices.Add(new VertexPositionNormalTexture(new Vector3(0, y, z), new Vector3(0, y, z), new Vector2(0)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(1, y, z), new Vector3(0, y, z), new Vector2(0)));

                short i1 = (short) (2*division);
                short i2 = (short) (i1 + 1);
                short i3 = (short) (2*((division + 1)%divisions));
                short i4 = (short) (i3 + 1);

                indices.Add(i1);
                indices.Add(i3);
                indices.Add(i2);

                indices.Add(i3);
                indices.Add(i4);
                indices.Add(i2);

                indices.Add(i2);
                indices.Add(i4);
                indices.Add(ic1);

                indices.Add(i3);
                indices.Add(i1);
                indices.Add(ic2);
            }

            vertices.Add(new VertexPositionNormalTexture(new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector2(0)));
            vertices.Add(new VertexPositionNormalTexture(new Vector3(-capOffset, 0, 0), new Vector3(-1, 0, 0),
                new Vector2(0)));

            return new GeometricPrimitiveAdv<VertexPositionNormalTexture>(device, vertices.ToArray(), indices.ToArray(),
                toLeftHanded);
        }
    }
}