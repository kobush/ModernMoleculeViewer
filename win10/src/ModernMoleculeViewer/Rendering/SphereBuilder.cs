using System.Collections.Generic;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace ModernMoleculeViewer.Rendering
{
    public class SphereBuilder
    {
        public static GeometricPrimitiveAdv<VertexPositionNormalTexture> New(GraphicsDevice device, int divisions = 3, bool toLeftHanded = false)
        {
            var builder = new SphereBuilder(divisions);

            return new GeometricPrimitiveAdv<VertexPositionNormalTexture>(device,
                builder._vertices.ToArray(),
                builder._indices.ToArray(),
                !toLeftHanded); // its left-handed by default
        }

        private readonly List<VertexPositionNormalTexture> _vertices = new List<VertexPositionNormalTexture>();
        private readonly List<short> _indices = new List<short>();

        private short _nextIndex;

        private SphereBuilder(int divisions)
        {

            AddBaseTriangle(new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0));
            AddBaseTriangle(new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 1, 0));
            AddBaseTriangle(new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 1, 0));
            AddBaseTriangle(new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            AddBaseTriangle(new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, -1, 0));
            AddBaseTriangle(new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector3(0, -1, 0));
            AddBaseTriangle(new Vector3(-1, 0, 0), new Vector3(0, 0, -1), new Vector3(0, -1, 0));
            AddBaseTriangle(new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector3(0, -1, 0));

            for (int division = 1; division < divisions; division++)
                Divide();

        }

        private void AddBaseTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            _vertices.Add(new VertexPositionNormalTexture(p1, new Vector3(p1.X, p1.Y, p1.Z), new Vector2()));
            _vertices.Add(new VertexPositionNormalTexture(p2, new Vector3(p2.X, p2.Y, p2.Z), new Vector2()));
            _vertices.Add(new VertexPositionNormalTexture(p3, new Vector3(p3.X, p3.Y, p3.Z), new Vector2()));

            _indices.Add(_nextIndex++);
            _indices.Add(_nextIndex++);
            _indices.Add(_nextIndex++);
        }

        private void Divide()
        {
            int indexCount = _indices.Count;

            for (int indexOffset = 0; indexOffset < indexCount; indexOffset += 3)
                DivideTriangle(indexOffset);
        }

        /// <summary>
        /// Replaces a triange at a given index buffer offset and replaces it with four triangles
        /// that compose an equilateral subdivision.
        /// </summary>
        /// <param name="indexOffset">An offset into the index buffer.</param>
        private void DivideTriangle(int indexOffset)
        {
            short i1 = _indices[indexOffset];
            short i2 = _indices[indexOffset + 1];
            short i3 = _indices[indexOffset + 2];

            Vector3 p1 = _vertices[i1].Position;
            Vector3 p2 = _vertices[i2].Position;
            Vector3 p3 = _vertices[i3].Position;
            Vector3 p4 = GetNormalizedMidpoint(p1, p2);
            Vector3 p5 = GetNormalizedMidpoint(p2, p3);
            Vector3 p6 = GetNormalizedMidpoint(p3, p1);

            _vertices.Add(new VertexPositionNormalTexture(p4, new Vector3(p4.X, p4.Y, p4.Z), new Vector2()));
            _vertices.Add(new VertexPositionNormalTexture(p5, new Vector3(p5.X, p5.Y, p5.Z), new Vector2()));
            _vertices.Add(new VertexPositionNormalTexture(p6, new Vector3(p6.X, p6.Y, p6.Z), new Vector2()));


            short i4 = _nextIndex++;
            short i5 = _nextIndex++;
            short i6 = _nextIndex++;

            _indices[indexOffset] = i4;
            _indices[indexOffset + 1] = i5;
            _indices[indexOffset + 2] = i6;

            _indices.Add(i1);
            _indices.Add(i4);
            _indices.Add(i6);

            _indices.Add(i4);
            _indices.Add(i2);
            _indices.Add(i5);

            _indices.Add(i6);
            _indices.Add(i5);
            _indices.Add(i3);
        }

        /// <summary>
        /// Calculates the midpoint between two points on a unit sphere and projects the result
        /// back to the surface of the sphere.
        /// </summary>
        /// <param name="p1">Point 1.</param>
        /// <param name="p2">Point 2.</param>
        /// <returns>The normalized midpoint.</returns>
        private Vector3 GetNormalizedMidpoint(Vector3 p1, Vector3 p2)
        {
            var vector = new Vector3((p1.X + p2.X)/2, (p1.Y + p2.Y)/2, (p1.Z + p2.Z)/2);
            vector.Normalize();

            return vector;
        }
    }
}