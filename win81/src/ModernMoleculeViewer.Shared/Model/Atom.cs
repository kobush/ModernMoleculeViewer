using System;
using System.Collections.Generic;
using System.Globalization;
using SharpDX;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// Abstract base class for all types of atoms.
    /// </summary>
    /// <remarks>
    /// Handles the majority of atom-related display logic.
    /// </remarks>
    internal abstract class Atom : HoverObject
    {
        private Molecule _molecule;
        private string _atomName;
        private string _residueName;
        private string _chainIdentifier;
        private int _residueSequenceNumber;
        private Vector3 _position;
        private double _temperatureFactor;
        private Color _atomColor;
        private Color _structureColor;
        private Color _temperatureColor;
        private Dictionary<Atom, float> _bonds;
        private Residue _residue;
        private ColorScheme _colorScheme;
        private bool _isSelected;
        private bool _showAsSelected;
        private Color _color;

        /// <summary>
        /// Label used for atom tooltips.
        /// </summary>
        internal override string DisplayName
        {
            get { return _residue.DisplayName + " : " + _atomName; }
        }

        /// <summary>
        /// Abreviated type name for the atom.
        /// </summary>
        internal string AtomName { get { return _atomName; } }

        /// <summary>
        /// Abbreviated residue name for the amino acid atom belongs to.
        /// </summary>
        internal string ResidueName { get { return _residueName; } }

        /// <summary>
        /// Alphanumeric chain identifier for the chain atom belongs to.
        /// </summary>
        internal string ChainIdentifier { get { return _chainIdentifier; } }

        /// <summary>
        /// Index number for the amino acid the atom belongs to.
        /// </summary>
        internal int ResidueSequenceNumber { get { return _residueSequenceNumber; } }

        /// <summary>
        /// 3D coordinate of this atom in angstroms.
        /// </summary>
        internal Vector3 Position { get { return _position; } }

        internal Color AtomColor { get { return _atomColor; }}

        /// <summary>
        /// Covalently bonded atoms along with distance in angstroms.
        /// </summary>
        internal Dictionary<Atom, float> Bonds { get { return _bonds; } }

        /// <summary>
        /// Reference to the amino acid this atom belongs to.
        /// </summary>
        internal Residue Residue
        {
            get { return _residue; }
            set { _residue = value; }
        }

        /// <summary>
        /// Color to use for the structure coloring method.
        /// </summary>
        internal Color StructureColor
        {
            get { return _structureColor; }
            set { _structureColor = value; }
        }

        /// <summary>
        /// Currently used coloring method.
        /// </summary>
        internal ColorScheme ColorScheme
        {
            get
            {
                return _colorScheme;
            }
            set
            {
                if (_colorScheme != value)
                {
                    _colorScheme = value;
                    UpdateColorView();
                }
            }
        }

        /// <summary>
        /// Gets and sets a boolean value indicating the current selection state.
        /// </summary>
        internal bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    _showAsSelected = _isSelected;

                    _residue.UpdateForAtomSelectionChange();
                }
            }
        }

        /// <summary>
        /// Gets and sets a boolean value indicating if the atom is rendered as selected. For
        /// certain operations such as rubber-banding an atom might be rendered as though it were
        /// selected even though it's not.
        /// </summary>
        internal bool ShowAsSelected
        {
            get
            {
                return _showAsSelected;
            }
            set
            {
                if (_showAsSelected != value)
                {
                    _showAsSelected = value;

                    _residue.UpdateForAtomSelectionChange();
                }
            }
        }

        /// <summary>
        /// Static method for parsing atom entries in a pdb file and instantiating the correct
        /// <see cref="Atom" /> subclass.
        /// </summary>
        /// <param name="molecule">The molecule this atom belongs to.</param>
        /// <param name="pdbLine">An atom entry from a pdb file.</param>
        /// <returns>An instance of an <see cref="Atom" /> subclass.</returns>
        internal static Atom CreateAtom(Molecule molecule, string pdbLine)
        {
            Atom atom;

            string atomName = pdbLine.Substring(12, 4).Trim();
            string residueName = pdbLine.Substring(17, 3).Trim();

            if (Residue.IsAminoName(residueName))
            {
                if (atomName == "CA") atom = new CAlpha();
                else atom = new ChainAtom();
            }
            else
            {
                if (residueName == "HOH") atom = new Water();
                else atom = new HetAtom();
            }

            atom._molecule = molecule;

            atom._bonds = new Dictionary<Atom, float>();

            atom._atomName = pdbLine.Substring(12, 4).Trim();
            atom._residueName = pdbLine.Substring(17, 3).Trim();

            atom._residueSequenceNumber = Convert.ToInt32(pdbLine.Substring(22, 4));

            atom._chainIdentifier = pdbLine.Substring(21, 1);
            if (atom._residueName == "HOH") atom._chainIdentifier = "";
            else if (atom._chainIdentifier == " ") atom._chainIdentifier = "1";

            float x = Single.Parse(pdbLine.Substring(30, 8), CultureInfo.InvariantCulture);
            float y = Single.Parse(pdbLine.Substring(38, 8), CultureInfo.InvariantCulture);
            float z = Single.Parse(pdbLine.Substring(46, 8), CultureInfo.InvariantCulture);

            atom._position = new Vector3(x, y, z);

            atom._temperatureFactor = Single.Parse(pdbLine.Substring(60, 6), CultureInfo.InvariantCulture);

            if (atom._atomName.StartsWith("C")) atom._atomColor = Color.LightGray;
            else if (atom._atomName.StartsWith("N")) atom._atomColor = Color.Blue;
            else if (atom._atomName.StartsWith("O")) atom._atomColor = Color.Red;
            else if (atom._atomName.StartsWith("H")) atom._atomColor = Color.Purple;
            else if (atom._atomName.StartsWith("S")) atom._atomColor = Color.Yellow;
            else atom._atomColor = Color.Green;

            atom._structureColor = atom._atomColor;

            atom._colorScheme = ColorScheme.Structure;

            return atom;
        }

        /// <summary>
        /// Static method that sets colors for the temperature coloring method for a list of atoms
        /// by normalizing the temperature values across the list.
        /// </summary>
        /// <param name="atoms">The list of atoms.</param>
        internal static void SetBFactorColors(List<Atom> atoms)
        {
            if (atoms.Count == 0) return;

            double minTemperature = atoms[0]._temperatureFactor;
            double maxTemperature = atoms[0]._temperatureFactor;

            foreach (Atom atom in atoms)
            {
                minTemperature = Math.Min(minTemperature, atom._temperatureFactor);
                maxTemperature = Math.Max(maxTemperature, atom._temperatureFactor);
            }

            double temperatureRange = maxTemperature - minTemperature;

            foreach (Atom atom in atoms)
            {
                double relativeTemperature = temperatureRange == 0 ? 0 :
                    (atom._temperatureFactor - minTemperature) / temperatureRange;

                if (relativeTemperature < 0.25)
                    atom._temperatureColor = new Color(
                        (byte)0, (byte)(255 * (4 * relativeTemperature)), (byte)255);
                else if (relativeTemperature < 0.5)
                    atom._temperatureColor = new Color(
                        (byte)0, (byte)255, (byte)(255 * (1 - 4 * (relativeTemperature - 0.25))));
                else if (relativeTemperature < 0.75)
                    atom._temperatureColor = new Color(
                        (byte)(255 * (4 * (relativeTemperature - 0.5))), (byte)255, (byte)0);
                else
                    atom._temperatureColor = new Color(
                        (byte)255, (byte)(255 * (1 - 4 * (relativeTemperature - 0.75))), (byte)0);
            }
        }

        /// <summary>
        /// Used by a <see cref="Residue" /> to calculate it's temperature color.
        /// </summary>
        /// <param name="atoms">A list of atoms.</param>
        /// <returns>The average color.</returns>
        internal static Color GetAverageTemperateColor(List<Atom> atoms)
        {
            int r = 0, g = 0, b = 0;

            foreach (Atom atom in atoms)
            {
                r += atom._temperatureColor.R;
                g += atom._temperatureColor.G;
                b += atom._temperatureColor.B;
            }

            return new Color((byte)(r / atoms.Count), (byte)(g / atoms.Count),
                (byte)(b / atoms.Count));
        }

        /// <summary>
        /// Static method to calculate the 3D bounding box for a list of atoms.
        /// </summary>
        /// <param name="atoms">A list of atoms.</param>
        /// <returns>The 3D bounding box.</returns>
        internal static BoundingBox GetBounds(List<Atom> atoms)
        {
            if (atoms.Count == 0) return new BoundingBox();

            float x1 = atoms[0]._position.X;
            float x2 = atoms[0]._position.X;
            float y1 = atoms[0]._position.Y;
            float y2 = atoms[0]._position.Y;
            float z1 = atoms[0]._position.Z;
            float z2 = atoms[0]._position.Z;

            foreach (Atom atom in atoms)
            {
                x1 = Math.Min(x1, atom._position.X);
                x2 = Math.Max(x2, atom._position.X);
                y1 = Math.Min(y1, atom._position.Y);
                y2 = Math.Max(y2, atom._position.Y);
                z1 = Math.Min(z1, atom._position.Z);
                z2 = Math.Max(z2, atom._position.Z);
            }

            return new BoundingBox(new Vector3(x1, y1, z1), new Vector3(x2, y2, z2));
        }

        /// <summary>
        /// Extra initialization for this class and subclasses that can't be done in the
        /// constructor since certain properties are expected to be set.
        /// </summary>
        internal virtual void Initialize()
        {
            UpdateColorView();
        }

/*        /// <summary>
        /// Performs hit testing for this atom.
        /// </summary>
        /// <param name="rayHitTestResult">A 3D mesh hit test result from the WPF visual tree hit
        /// testing framework</param>
        /// <returns>True if the mesh hit belongs to this atom, otherwise false.</returns>
        internal virtual bool HoverHitTest(RayMeshGeometry3DHitTestResult rayHitTestResult)
        {
            return (selectionModel == rayHitTestResult.ModelHit || (atomModel != null &&
                atomModel.Children.Contains(rayHitTestResult.ModelHit)));
        }*/

        /// <summary>
        /// The molecule this atom belongs to.
        /// </summary>
        protected Molecule Molecule
        {
            get { return _molecule; }
        }

        public Color Color
        {
            get { return _color; }
        }

        /// <summary>
        /// Updates the 3D model to depict the correct hovered state.
        /// </summary>
        protected override void OnIsHoveredChanged()
        {
            UpdateColorView();
        }

        /// <summary>
        /// Updates the material color for this atom based on the coloring method and the current
        /// hover state.
        /// </summary>
        private void UpdateColorView()
        {
            Color color = _atomColor;

            if (_colorScheme == ColorScheme.Structure)
                color = _structureColor;
            else if (_colorScheme == ColorScheme.Residue && _residue != null)
                color = _residue.ResidueColor;
            else if (_colorScheme == ColorScheme.Chain && _residue != null &&
                _residue.Chain != null)
                color = _residue.Chain.ChainColor;
            else if (_colorScheme == ColorScheme.Temperature)
                color = _temperatureColor;

            if (IsHovered)
            {
                byte r = (byte)(color.R + (255 - color.R) / 2);
                byte g = (byte)(color.G + (255 - color.G) / 2);
                byte b = (byte)(color.B + (255 - color.B) / 2);

                if (r == g && g == b) r = g = b = 255;

                color = new Color(r, g, b);
            }

            _color = color;
        }
    }
}