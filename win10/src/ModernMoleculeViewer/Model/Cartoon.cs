using System;
using System.Collections.Generic;
using ModernMoleculeViewer.Rendering;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// Creates the 3D model for a particular residue when being displayed in cartoon mode.
    /// </summary>
    internal class Cartoon 
    {
        private const int RadialSegmentCount = 10;
        private const float TurnWidth = 0.2f;
        private const float HelixWidth = 1.4f;
        private const float HelixHeight = 0.25f;
        private const float SheetWidth = 1.2f;
        private const float SheetHeight = 0.25f;
        private const float ArrowWidth = 1.6f;

        private readonly Residue _residue;
        
        private readonly List<Vector3> _ribbonPoints;
        private readonly List<Vector3> _torsionVectors;
        private readonly List<Vector3> _normalVectors;

        private readonly List<VertexPositionNormalTexture> _vertices = new List<VertexPositionNormalTexture>();
        private readonly List<int> _indices = new List<int>();
        
        private readonly GeometricPrimitiveAdv<VertexPositionNormalTexture> _mesh;

        /// <summary>
        /// Builds the 3D model for the cartoon view a the given residue.
        /// </summary>
        /// <param name="residue">A residue.</param>
        /// <param name="initialColor">The residue's current color.</param>
        /// <param name="graphicsDevice"></param>
        internal Cartoon(Residue residue, Color initialColor, GraphicsDevice graphicsDevice)
        {
            _residue = residue;
            
            _residue.Ribbon.GetResidueSpline(_residue, out _ribbonPoints, out _torsionVectors, out _normalVectors);

            if (_residue.IsHelix)
            {
                AddTube(HelixWidth, HelixHeight);

                if (_residue.IsStructureStart)
                    AddTubeCap(HelixWidth, HelixHeight);

                if (_residue.IsStructureEnd)
                    AddTubeCap(HelixWidth, HelixHeight);
            }
            else if (_residue.IsSheet)
            {
                AddSheet();

                if (_residue.IsStructureStart || _residue.IsStructureEnd)
                    AddSheetCap();
            }
            else
            {
                AddTube(TurnWidth, TurnWidth);

                if (_residue.IsStructureStart)
                    AddTubeCap(TurnWidth, TurnWidth);

                if (_residue.IsStructureEnd)
                    AddTubeCap(TurnWidth, TurnWidth);
            }

            if (_vertices.Count > 0 && _indices.Count > 0)
                _mesh = new GeometricPrimitiveAdv<VertexPositionNormalTexture>(graphicsDevice, _vertices.ToArray(), _indices.ToArray());
        }

        public GeometricPrimitiveAdv<VertexPositionNormalTexture> Mesh => _mesh;

/*        /// <summary>
        /// Gets and sets the color of the model.
        /// </summary>
        internal Color Color
        {
            get { return ((SolidColorBrush)diffuseMaterial.Brush).Color; }
            set { ((SolidColorBrush)diffuseMaterial.Brush).Color = value; }
        }*/

/*        /// <summary>
        /// Performs hit testing for this cartoon mesh.
        /// </summary>
        /// <param name="rayHitTestResult">A 3D mesh hit test result from the WPF visual tree hit
        /// testing framework</param>
        /// <returns>
        /// True if the mesh hit belongs to this residue, otherwise false.
        /// </returns>
        internal bool HoverHitTest(RayMeshGeometry3DHitTestResult rayHitTestResult)
        {
            return model.Children.Contains(rayHitTestResult.ModelHit);
        }*/

        /// <summary>
        /// Creates a cylindrical tube along the spline path.
        /// </summary>
        /// <param name="width">The x-radius of the extrusion ellipse.</param>
        /// <param name="height">The y-radius of the extrusion ellipse.</param>
        private void AddTube(float width, float height)
        {
            // store first index
            int io = _vertices.Count;

            for (int i = 0; i < _ribbonPoints.Count; i++)
            {
                for (int j = 0; j < RadialSegmentCount; j++)
                {
                    float t = 2 * (float)Math.PI * j / RadialSegmentCount;

                    Vector3 radialVector = width * (float)Math.Cos(t) * _torsionVectors[i] + height * (float)Math.Sin(t) * _normalVectors[i];
                    Vector3 normalVector = height * (float)Math.Cos(t) * _torsionVectors[i] + width * (float)Math.Sin(t) * _normalVectors[i];
                    normalVector.Normalize();

                    _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[i] + radialVector, normalVector, new Vector2()));
                }
            }

            int rsc = RadialSegmentCount;

            for (int i = 0; i < _ribbonPoints.Count - 1; i++)
            {
                for (int j = 0; j < RadialSegmentCount; j++)
                {
                    _indices.Add(io + i * rsc + j);
                    _indices.Add(io + i * rsc + (j + 1) % rsc);
                    _indices.Add(io + (i + 1) * rsc + (j + 1) % rsc);

                    _indices.Add(io + i * rsc + j);
                    _indices.Add(io + (i + 1) * rsc + (j + 1) % rsc);
                    _indices.Add(io + (i + 1) * rsc + j);
                }
            }
        }

        /// <summary>
        /// Creates an elliptical cap for a tube along the spline path.
        /// </summary>
        /// <param name="width">The x-radius of the cap ellipse.</param>
        /// <param name="height">The y-radius of the cap ellipse.</param>
        private void AddTubeCap(float width, float height)
        {
            int io = _vertices.Count;

            Vector3 normalVector = Vector3.Cross(_torsionVectors[0], _normalVectors[0]);

            if (_residue.IsStructureEnd) 
                normalVector = Vector3.Negate(normalVector);

            int offset = _residue.IsStructureStart ? 0 : _ribbonPoints.Count - 1;

            _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[offset], normalVector, new Vector2()));

            for (int i = 0; i < RadialSegmentCount; i++)
            {
                float t = 2 * (float)Math.PI * i / RadialSegmentCount;

                Vector3 radialVector = width * (float)Math.Cos(t) * _torsionVectors[offset] + 
                                       height * (float)Math.Sin(t) * _normalVectors[offset];

                _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[offset] + radialVector, normalVector, new Vector2()));

                _indices.Add(io);

                if (_residue.IsStructureStart)
                {
                    _indices.Add(io + (i + 1) % RadialSegmentCount + 1);
                    _indices.Add(io + i + 1);
                }
                else
                {
                    _indices.Add(io + i + 1);
                    _indices.Add(io + (i + 1) % RadialSegmentCount + 1);
                }
            }
        }

        /// <summary>
        /// Creates a rectangular solid sheet along the spline path.
        /// </summary>
        private void AddSheet()
        {
            int io = _vertices.Count;

            float offsetLength = 0;

            if (_residue.IsStructureEnd)
            {
                Vector3 lengthVector = _ribbonPoints[_ribbonPoints.Count - 1] - _ribbonPoints[0];
                offsetLength = ArrowWidth / lengthVector.Length();
            }

            for (int i = 0; i < _ribbonPoints.Count; i++)
            {
                float actualWidth = !_residue.IsStructureEnd ? SheetWidth : 
                    ArrowWidth * (1 - (float)i / (_ribbonPoints.Count - 1));

                Vector3 horizontalVector = actualWidth * _torsionVectors[i];
                Vector3 verticalVector = SheetHeight * _normalVectors[i];

                Vector3 normalOffset = new Vector3();

                if (_residue.IsStructureEnd)
                {
                    normalOffset = offsetLength * Vector3.Cross(_normalVectors[i], _torsionVectors[i]);
                }

                _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[i] + horizontalVector + verticalVector, _normalVectors[i], new Vector2()));
                _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[i] - horizontalVector + verticalVector, _normalVectors[i], new Vector2()));
                _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[i] - horizontalVector + verticalVector, -_torsionVectors[i] + normalOffset, new Vector2()));
                _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[i] - horizontalVector - verticalVector, -_torsionVectors[i] + normalOffset, new Vector2()));
                _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[i] - horizontalVector - verticalVector, -_normalVectors[i], new Vector2()));
                _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[i] + horizontalVector - verticalVector, -_normalVectors[i], new Vector2()));
                _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[i] + horizontalVector - verticalVector, _torsionVectors[i] + normalOffset, new Vector2()));
                _vertices.Add(new VertexPositionNormalTexture(_ribbonPoints[i] + horizontalVector + verticalVector, _torsionVectors[i] + normalOffset, new Vector2()));
            }

            for (int i = 0; i < _ribbonPoints.Count - 1; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    _indices.Add(io + i * 8 + 2 * j);
                    _indices.Add(io + i * 8 + 2 * j + 1);
                    _indices.Add(io + (i + 1) * 8 + 2 * j + 1);

                    _indices.Add(io + i * 8 + 2 * j);
                    _indices.Add(io + (i + 1) * 8 + 2 * j + 1);
                    _indices.Add(io + (i + 1) * 8 + 2 * j);
                }
            }
        }

        /// <summary>
        /// Creates a flat cap or an arrow head cap for a sheet.
        /// </summary>
        private void AddSheetCap()
        {

            Vector3 horizontalVector = SheetWidth * _torsionVectors[0];
            Vector3 verticalVector = SheetHeight * _normalVectors[0];

            Vector3 p1 = _ribbonPoints[0] + horizontalVector + verticalVector;
            Vector3 p2 = _ribbonPoints[0] - horizontalVector + verticalVector;
            Vector3 p3 = _ribbonPoints[0] - horizontalVector - verticalVector;
            Vector3 p4 = _ribbonPoints[0] + horizontalVector - verticalVector;

            if (_residue.IsStructureStart)
            {
                AddSheetCapSection(p1, p2, p3, p4);
            }
            else
            {
                Vector3 arrowHorizontalVector = ArrowWidth * _torsionVectors[0];

                Vector3 p5 = _ribbonPoints[0] + arrowHorizontalVector + verticalVector;
                Vector3 p6 = _ribbonPoints[0] - arrowHorizontalVector + verticalVector;
                Vector3 p7 = _ribbonPoints[0] - arrowHorizontalVector - verticalVector;
                Vector3 p8 = _ribbonPoints[0] + arrowHorizontalVector - verticalVector;

                AddSheetCapSection(p5, p1, p4, p8);
                AddSheetCapSection(p2, p6, p7, p3);
            }
        }

        /// <summary>
        /// Helper method to add a quadrilateral surface for a sheet cap.
        /// </summary>
        /// <param name="p1">Point 1.</param>
        /// <param name="p2">Point 2.</param>
        /// <param name="p3">Point 3.</param>
        /// <param name="p4">Point 4.</param>
        private void AddSheetCapSection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            int indexOffset = _vertices.Count;

             Vector3 normalVector = Vector3.Cross(p2 - p1, p4 - p1);
            normalVector.Normalize();

            _vertices.Add(new VertexPositionNormalTexture(p1, normalVector, new Vector2()));
            _vertices.Add(new VertexPositionNormalTexture(p2, normalVector, new Vector2()));
            _vertices.Add(new VertexPositionNormalTexture(p3, normalVector, new Vector2()));
            _vertices.Add(new VertexPositionNormalTexture(p4, normalVector, new Vector2()));

            _indices.Add(indexOffset + 0);
            _indices.Add(indexOffset + 2);
            _indices.Add(indexOffset + 1);

            _indices.Add(indexOffset + 2);
            _indices.Add(indexOffset + 0);
            _indices.Add(indexOffset + 3);
        }
    }
}