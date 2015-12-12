using System;
using System.Collections.Generic;
using SharpDX;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// Calculates spline paths for all residues in a particular structure.
    /// </summary>
    internal class Ribbon
    {
        private const int LinearSegmentCount = 10;

        private readonly List<Residue> _residues = new List<Residue>();
        private readonly List<bool> _isHelixList = new List<bool>();
        private readonly List<Vector3> _caList = new List<Vector3>();
        private readonly List<Vector3> _oList = new List<Vector3>();
        private readonly List<Vector3> _pList = new List<Vector3>();
        private readonly List<Vector3> _cList = new List<Vector3>();
        private readonly List<Vector3> _dList = new List<Vector3>();
        private readonly List<Vector3> _ribbonPoints = new List<Vector3>();
        private readonly List<Vector3> _torsionVectors = new List<Vector3>();
        private readonly List<Vector3> _normalVectors = new List<Vector3>();

        /// <summary>
        /// All of the residues in the secondary strucuture associated with this
        /// <see cref="Ribbon" />.
        /// </summary>
        internal List<Residue> Residues { get { return _residues; } }

        /// <summary>
        /// Initiates the spine calculation logic for all constituent residues.
        /// </summary>
        internal void CreateControlPoints()
        {
            if (_residues.Count < 4)
            {
                foreach (Residue residue in _residues)
                    residue.Ribbon = null;

                return;
            }

            PopulateAtomLists();
            PopulateControlLists();
            PopulateSplineLists();
        }

        /// <summary>
        /// Gets all of the values that represent the spline for a particular residue.
        /// </summary>
        /// <param name="residue">A residue in the corresponding secondary structure.</param>
        /// <param name="residueRibbonPoints">A list control points for the spline.</param>
        /// <param name="residueTorsionVectors">A list of the torsion vectors for the
        /// spline.</param>
        /// <param name="residueNormalVectors">A list of the normal vectors for the spline.</param>
        internal void GetResidueSpline(Residue residue, out List<Vector3> residueRibbonPoints,
            out List<Vector3> residueTorsionVectors, out List<Vector3> residueNormalVectors)
        {
            residueRibbonPoints = new List<Vector3>();
            residueTorsionVectors = new List<Vector3>();
            residueNormalVectors = new List<Vector3>();

            int startIndex = _residues.IndexOf(residue) * LinearSegmentCount;

            for (int i = startIndex; i <= startIndex + LinearSegmentCount; i++)
            {
                residueRibbonPoints.Add(_ribbonPoints[i]);
                residueTorsionVectors.Add(_torsionVectors[i]);
                residueNormalVectors.Add(_normalVectors[i]);
            }
        }

        /// <summary>
        /// Helper function used by <see cref="CreateControlPoints" /> to populate the data
        /// stuctures which refence certain atom types.
        /// </summary>
        private void PopulateAtomLists()
        {
            foreach (Residue residue in _residues)
            {
                _isHelixList.Add(residue.IsHelix);
                _caList.Add(residue.CAlphaPosition.Value);
                _oList.Add(residue.CarbonylOxygenPosition.Value);
            }

            _isHelixList.Insert(0, _isHelixList[0]);
            _isHelixList.Insert(0, _isHelixList[1]);

            _isHelixList.Add(_isHelixList[_isHelixList.Count - 1]);
            _isHelixList.Add(_isHelixList[_isHelixList.Count - 2]);

            _caList.Insert(0, Reflect(_caList[0], _caList[1], 0.4f));
            _caList.Insert(0, Reflect(_caList[1], _caList[2], 0.6f));

            _caList.Add(Reflect(
                _caList[_caList.Count - 1], _caList[_caList.Count - 2], 0.4f));
            _caList.Add(Reflect(
                _caList[_caList.Count - 2], _caList[_caList.Count - 3], 0.6f));

            _oList.Insert(0, Reflect(_oList[0], _oList[1], 0.4f));
            _oList.Insert(0, Reflect(_oList[1], _oList[2], 0.6f));

            _oList.Add(Reflect(
                _oList[_oList.Count - 1], _oList[_oList.Count - 2], 0.4f));
            _oList.Add(Reflect(
                _oList[_oList.Count - 2], _oList[_oList.Count - 3], 0.6f));
        }

        /// <summary>
        /// Helper function used by <see cref="CreateControlPoints" /> to populate the data
        /// stuctures which hold control point data.
        /// </summary>
        private void PopulateControlLists()
        {
            Vector3 previousD = new Vector3();

            for (int i = 0; i < _caList.Count - 1; i++)
            {
                Vector3 ca1 = _caList[i];
                Vector3 o1 = _oList[i];
                Vector3 ca2 = _caList[i + 1];

                Vector3 p = new Vector3((ca1.X + ca2.X) / 2, (ca1.Y + ca2.Y) / 2,
                    (ca1.Z + ca2.Z) / 2);

                Vector3 a = ca2 - ca1;
                Vector3 b = o1 - ca1;

                Vector3 c = Vector3.Cross(a, b);
                Vector3 d = Vector3.Cross(c, a);

                c.Normalize();
                d.Normalize();

                if (_isHelixList[i] && _isHelixList[i + 1])
                    //p.Offset(1.5 * c.X, 1.5 * c.Y, 1.5 * c.Z);
                    p += new Vector3(1.5f * c.X, 1.5f * c.Y, 1.5f * c.Z);

                if (i > 0 && Helper.AngleBetween(d, previousD) > MathUtil.PiOverTwo) d = Vector3.Negate(d);
                previousD = d;

                _pList.Add(p);
                _dList.Add(p + d);
            }
        }

        /// <summary>
        /// Helper function used by <see cref="CreateControlPoints" /> to populate the data
        /// stuctures which hold the spline data.
        /// </summary>
        private void PopulateSplineLists()
        {
            Vector3 previousRibbonPoint = new Vector3();
            Vector3 ribbonPoint;
            Vector3 torsionPoint;

            for (int i = 0; i < _residues.Count; i++)
            {
                Vector3 p1 = _pList[i];
                Vector3 p2 = _pList[i + 1];
                Vector3 p3 = _pList[i + 2];
                Vector3 p4 = _pList[i + 3];

                Vector3 d1 = _dList[i];
                Vector3 d2 = _dList[i + 1];
                Vector3 d3 = _dList[i + 2];
                Vector3 d4 = _dList[i + 3];

                for (int j = 1; j <= LinearSegmentCount; j++)
                {
                    float t = (float)j / LinearSegmentCount;

                    if (t < 0.5)
                    {
                        ribbonPoint = Spline(p1, p2, p3, t + 0.5f);
                        torsionPoint = Spline(d1, d2, d3, t + 0.5f);
                    }
                    else
                    {
                        ribbonPoint = Spline(p2, p3, p4, t - 0.5f);
                        torsionPoint = Spline(d2, d3, d4, t - 0.5f);
                    }

                    if (i == 0 && j == 1)
                    {
                        previousRibbonPoint = Spline(p1, p2, p3, 0.5f);

                        Vector3 previousTorsionPoint = Spline(d1, d2, d3, 0.5f);

                        Vector3 extrapolatedRibbonPoint =
                            Reflect(previousRibbonPoint, ribbonPoint, 1);

                        AddSplineNode(extrapolatedRibbonPoint, previousRibbonPoint,
                            previousTorsionPoint);
                    }

                    AddSplineNode(previousRibbonPoint, ribbonPoint, torsionPoint);

                    previousRibbonPoint = ribbonPoint;
                }
            }
        }

        /// <summary>
        /// Helper function used by <see cref="PopulateSplineLists" /> to populate the data
        /// stuctures for a particular point along the spline.
        /// </summary>
        private void AddSplineNode(
            Vector3 previousRibbonPoint, Vector3 ribbonPoint, Vector3 torsionPoint)
        {
            _ribbonPoints.Add(ribbonPoint);

            Vector3 torsionVector = torsionPoint - ribbonPoint;
            torsionVector.Normalize();
            _torsionVectors.Add(torsionVector);

            Vector3 ribbonVector = ribbonPoint - previousRibbonPoint;
            Vector3 normalVector = Vector3.Cross(torsionVector, ribbonVector);
            normalVector.Normalize();
            _normalVectors.Add(normalVector);
        }

        /// <summary>
        /// Reflects one point across another by a specified amount.
        /// </summary>
        /// <param name="p1">Point 1.</param>
        /// <param name="p2">Point 2.</param>
        /// <param name="amount">The reflection scaling factor.</param>
        /// <returns></returns>
        private Vector3 Reflect(Vector3 p1, Vector3 p2, float amount)
        {
            float x = p1.X - amount * (p2.X - p1.X);
            float y = p1.Y - amount * (p2.Y - p1.Y);
            float z = p1.Z - amount * (p2.Z - p1.Z);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Calculates the actual spline position.
        /// </summary>
        /// <param name="p1">Point 1.</param>
        /// <param name="p2">Point 2.</param>
        /// <param name="p3">Point 3.</param>
        /// <param name="t">The parametric value along the spline section.</param>
        /// <returns></returns>
        private Vector3 Spline(Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float a = (float)Math.Pow(1 - t, 2) / 2;
            float c = (float)Math.Pow(t, 2) / 2;
            float b = 1 - a - c;

            float x = a * p1.X + b * p2.X + c * p3.X;
            float y = a * p1.Y + b * p2.Y + c * p3.Y;
            float z = a * p1.Z + b * p2.Z + c * p3.Z;

            return new Vector3(x, y, z);
        }
    }

    static class Helper
    {
        public static float AngleBetween(Vector3 vector1, Vector3 vector2)
        {
            vector1.Normalize();
            vector2.Normalize();

            return
                Vector3.Dot(vector1, vector2) >= 0.0
                    ? (float) (2.0*Math.Asin((vector1 - vector2).Length()/2.0))
                    : (float) (Math.PI - 2.0*Math.Asin((-vector1 - vector2).Length()/2.0));
        }
    }
}