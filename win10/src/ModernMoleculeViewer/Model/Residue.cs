using System;
using System.Collections.Generic;
using SharpDX;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// Represents a residue in a molecule. 
    /// </summary>
    /// <remarks>
    /// Sometimes referred to as an amino acid. Generates WPF content to display the 
    /// residue in cartoon mode as well as in the identifier strip at the
    /// top of the screen. Aggregates all constituent atoms.
    /// </remarks>
    internal class Residue : HoverObject
    {
        /// <summary>
        /// 
        /// </summary>
        internal enum SelectionType { None, Partial, Full };

        private readonly Molecule _molecule;

        private readonly string _residueName;
        private readonly string _chainIdentifier;
        private readonly int _residueSequenceNumber;
        private readonly string _residueIdentifier;
        private readonly Color _residueColor;

        private Color _structureColor;
        private Color _color;

        private readonly List<Atom> _atoms;
        private readonly List<ResidueStripItem> _residueStripItems;

        private bool _isSheet;
        private bool _isHelix;
        private bool _isStructureStart;
        private bool _isStructureEnd;

        private Residue _previousResidue;
        private Residue _nextResidue;
        private Vector3? _cAlphaPosition;
        private Vector3? _carbonylOxygenPosition;

        private Chain _chain;
        private Ribbon _ribbon;
        private Cartoon _cartoon;

        private ColorScheme _colorScheme;
        private SelectionType _selection;
        private SelectionType _showAsSelection;
        private bool _updatingSelectionProperty;

        /// <summary>
        /// Creates a new <see cref="Residue" /> object.
        /// </summary>
        /// <param name="molecule">The molecule this residue belongs to.</param>
        /// <param name="atom">An atom in the residue. This is needed to obtain residue properties
        /// since there is no corresponding PDB file record.</param>
        internal Residue(Molecule molecule, Atom atom)
        {
            _molecule = molecule;
            _molecule.ShowCartoonChanged += MoleculeShowCartoonChanged;

            _residueName = atom.ResidueName;
            _chainIdentifier = atom.ChainIdentifier;
            _residueSequenceNumber = atom.ResidueSequenceNumber;

            _atoms = new List<Atom>();
            _atoms.Add(atom);

            _residueIdentifier = GetResidueIdentifier(_residueName);
            _residueColor = GetResidueColor(_residueName);

            _structureColor = _residueIdentifier != "O" ? Color.LightGray : Color.Red;

            _colorScheme = ColorScheme.Structure;

            _residueStripItems = new List<ResidueStripItem>();
            foreach (char character in _residueIdentifier)
            {
                ResidueStripItem residueStripItem = new ResidueStripItem(character.ToString());
                residueStripItem.Residue = this;
                _residueStripItems.Add(residueStripItem);
            }

            UpdateColorView();
        }

        /// <summary>
        /// Label used for atom tooltips.
        /// </summary>
        internal override string DisplayName
        {
            get
            {
                return "[" + _residueSequenceNumber + "] " + _residueName;
            }
        }

        /// <summary>
        /// The multi-character abbreviation for the residue. For chain-based amino acids, this is
        /// a three letter code.
        /// </summary>
        internal string ResidueName { get { return _residueName; } }

        /// <summary>
        /// Alphanumeric chain identifier for the chain residue belongs to.
        /// </summary>
        internal string ChainIdentifier { get { return _chainIdentifier; } }

        /// <summary>
        /// Index number for this amino acid.
        /// </summary>
        internal int ResidueSequenceNumber { get { return _residueSequenceNumber; } }

        /// <summary>
        /// Shortened abbreviation for the residue. For chain-based amino acids, this is a single
        /// letter.
        /// </summary>
        internal string ResidueIdentifier { get { return _residueIdentifier; } }

        /// <summary>
        /// The color used for this residue when using the residue-based coloring method.
        /// </summary>
        internal Color ResidueColor { get { return _residueColor; } }

        /// <summary>
        /// The constituent atoms.
        /// </summary>
        internal List<Atom> Atoms { get { return _atoms; } }

        /// <summary>
        /// Gets and sets a boolean value indicating if this residue is part of a sheet.
        /// </summary>
        internal bool IsSheet
        {
            get { return _isSheet; }
            set { _isSheet = value; }
        }

        /// <summary>
        /// Gets and sets a boolean value indicating if this residue is part of a helix.
        /// </summary>
        internal bool IsHelix
        {
            get { return _isHelix; }
            set { _isHelix = value; }
        }

        /// <summary>
        /// Gets and sets a boolean value indicating if this residue is the first residue in a
        /// secondary structure.
        /// </summary>
        internal bool IsStructureStart
        {
            get { return _isStructureStart; }
            set { _isStructureStart = value; }
        }

        /// <summary>
        /// Gets and sets a boolean value indicating if this residue is the last residue in a
        /// secondary structure.
        /// </summary>
        internal bool IsStructureEnd
        {
            get { return _isStructureEnd; }
            set { _isStructureEnd = value; }
        }

        /// <summary>
        /// Reference to the previous residue in the current chain.
        /// </summary>
        internal Residue PreviousResidue
        {
            get { return _previousResidue; }
            set { _previousResidue = value; }
        }

        /// <summary>
        /// Reference to the next residue in the current chain.
        /// </summary>
        internal Residue NextResidue
        {
            get { return _nextResidue; }
            set { _nextResidue = value; }
        }

        /// <summary>
        /// If residue belongs to a standard protein amino acid this will contain the 3D location
        /// of the alpha carbon atom.
        /// </summary>
        internal Vector3? CAlphaPosition
        {
            get { return _cAlphaPosition; }
            set { _cAlphaPosition = value; }
        }

        /// <summary>
        /// If residue belongs to a standard protein amino acid this will contain the 3D location
        /// of the carbonyl oxygen atom.
        /// </summary>
        internal Vector3? CarbonylOxygenPosition
        {
            get { return _carbonylOxygenPosition; }
            set { _carbonylOxygenPosition = value; }
        }

        /// <summary>
        /// The chain this residue belongs to.
        /// </summary>
        internal Chain Chain
        {
            get { return _chain; }
            set { _chain = value; }
        }

        /// <summary>
        /// Reference to the <see cref="Ribbon" /> object that calculates spline paths for this
        /// residue.
        /// </summary>
        internal Ribbon Ribbon
        {
            get { return _ribbon; }
            set { _ribbon = value; }
        }

        /// <summary>
        /// The color to use for this residue when using the structure-based coloring method.
        /// </summary>
        internal Color StructureColor
        {
            get { return _structureColor; }
            set
            {
                _structureColor = value;
                UpdateColorView();
            }
        }

        /// <summary>
        /// All of the <see cref="ResidueStripItem" /> controls for this residue. For protein
        /// amino acids, there is only one item in this list.
        /// </summary>
        internal List<ResidueStripItem> ResidueStripItems
        {
            get { return _residueStripItems; }
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

        public Color Color
        {
            get { return _color; }
        }

        /// <summary>
        /// Gets and sets a <see cref="SelectionType" /> enumeration value indicating the current
        /// selection state.
        /// </summary>
        internal SelectionType Selection
        {
            get
            {
                return _selection;
            }
            set
            {
                if (_selection != value)
                {
                    _selection = value;
                    _showAsSelection = _selection;
                    UpdateView();

                    _updatingSelectionProperty = true;

                    foreach (Atom atom in _atoms)
                    {
                        if (_selection == SelectionType.None)
                            atom.IsSelected = false;
                        else if (_selection == SelectionType.Full)
                            atom.IsSelected = true;
                    }

                    _updatingSelectionProperty = false;
                }
            }
        }

        /// <summary>
        /// Gets and sets a <see cref="SelectionType" /> enumeration value indicating if the
        /// residue is rendered as selected. For certain operations such as rubber-banding a
        /// residue might be rendered as though it were selected even though it's not.
        /// </summary>
        internal SelectionType ShowAsSelection
        {
            get
            {
                return _showAsSelection;
            }
            set
            {
                if (_showAsSelection != value)
                {
                    _showAsSelection = value;
                    UpdateView();

                    _updatingSelectionProperty = true;

                    foreach (Atom atom in _atoms)
                    {
                        if (_showAsSelection == SelectionType.None)
                            atom.ShowAsSelected = false;
                        else if (_showAsSelection == SelectionType.Partial)
                            atom.ShowAsSelected = atom.IsSelected;
                        else if (_showAsSelection == SelectionType.Full)
                            atom.ShowAsSelected = true;
                    }

                    _updatingSelectionProperty = false;
                }
            }
        }

        public Cartoon Cartoon
        {
            get { return _cartoon; }
            set { _cartoon = value; }
        }

        /// <summary>
        /// Updates the selection state based on the selection states of the constituent atoms.
        /// </summary>
        internal void UpdateForAtomSelectionChange()
        {
            if (_updatingSelectionProperty) return;

            bool dirty = false;

            bool fullSelected = true;
            bool partialSelected = false;

            foreach (Atom atom in _atoms)
            {
                if (atom.IsSelected) partialSelected = true;
                else fullSelected = false;
            }

            if (fullSelected && _selection != SelectionType.Full)
            {
                _selection = SelectionType.Full;
                dirty = true;
            }
            else if (!fullSelected && partialSelected && _selection != SelectionType.Partial)
            {
                _selection = SelectionType.Partial;
                dirty = true;
            }
            else if (!partialSelected && _selection != SelectionType.None)
            {
                _selection = SelectionType.None;
                dirty = true;
            }

            bool fullShowAsSelected = true;
            bool partialShowAsSelected = false;

            foreach (Atom atom in _atoms)
            {
                if (atom.ShowAsSelected) partialShowAsSelected = true;
                else fullShowAsSelected = false;
            }

            if (fullShowAsSelected && _showAsSelection != SelectionType.Full)
            {
                _showAsSelection = SelectionType.Full;
                dirty = true;
            }
            else if (!fullShowAsSelected && partialShowAsSelected &&
                _showAsSelection != SelectionType.Partial)
            {
                _showAsSelection = SelectionType.Partial;
                dirty = true;
            }
            else if (!partialShowAsSelected && _showAsSelection != SelectionType.None)
            {
                _showAsSelection = SelectionType.None;
                dirty = true;
            }

            if (dirty) UpdateView();
        }

/*        /// <summary>
        /// Performs hit testing for this residue.
        /// </summary>
        /// <param name="rayHitTestResult">A 3D mesh hit test result from the WPF visual tree hit
        /// testing framework</param>
        /// <returns>True if the mesh hit belongs to this residue, otherwise false.</returns>
        internal virtual bool HoverHitTest(RayMeshGeometry3DHitTestResult rayHitTestResult)
        {
            if (cartoon != null)
                return cartoon.HoverHitTest(rayHitTestResult);
            else
                return false;
        }*/

        /// <summary>
        /// Determines if a particular residue name refers to an amino acid.
        /// </summary>
        /// <param name="residueName">A multi-character residue abbreviation.</param>
        /// <returns>True if and only if the residue name refers to an amino acid.</returns>
        internal static bool IsAminoName(string residueName)
        {
            if (residueName == "ALA") return true;
            else if (residueName == "ARG") return true;
            else if (residueName == "ASP") return true;
            else if (residueName == "CYS") return true;
            else if (residueName == "GLN") return true;
            else if (residueName == "GLU") return true;
            else if (residueName == "GLY") return true;
            else if (residueName == "HIS") return true;
            else if (residueName == "ILE") return true;
            else if (residueName == "LEU") return true;
            else if (residueName == "LYS") return true;
            else if (residueName == "MET") return true;
            else if (residueName == "PHE") return true;
            else if (residueName == "PRO") return true;
            else if (residueName == "SER") return true;
            else if (residueName == "THR") return true;
            else if (residueName == "TRP") return true;
            else if (residueName == "TYR") return true;
            else if (residueName == "VAL") return true;
            else if (residueName == "ASN") return true;
            else return false;
        }

        /// <summary>
        /// Updates the 3D model to depict the correct hovered state.
        /// </summary>
        protected override void OnIsHoveredChanged()
        {
            UpdateView();
        }

        /// <summary>
        /// Static method to obtain the single character abbreviation of an amino acid.
        /// </summary>
        /// <param name="residueName">A multi-character residue abbreviation.</param>
        /// <returns>A single character abbreviation if one is available, othewise return the input
        /// abbreviation.</returns>
        private static string GetResidueIdentifier(string residueName)
        {
            if (residueName == "HOH") return "O";
            else if (residueName == "ALA") return "A";
            else if (residueName == "ARG") return "R";
            else if (residueName == "ASP") return "D";
            else if (residueName == "CYS") return "C";
            else if (residueName == "GLN") return "Q";
            else if (residueName == "GLU") return "E";
            else if (residueName == "GLY") return "G";
            else if (residueName == "HIS") return "H";
            else if (residueName == "ILE") return "I";
            else if (residueName == "LEU") return "L";
            else if (residueName == "LYS") return "K";
            else if (residueName == "MET") return "M";
            else if (residueName == "PHE") return "F";
            else if (residueName == "PRO") return "P";
            else if (residueName == "SER") return "S";
            else if (residueName == "THR") return "T";
            else if (residueName == "TRP") return "W";
            else if (residueName == "TYR") return "Y";
            else if (residueName == "VAL") return "V";
            else if (residueName == "ASN") return "N";
            else return residueName;
        }

        /// <summary>
        /// Selects a color based on the residue type.
        /// </summary>
        /// <param name="residueName">A multi-character residue abbreviation.</param>
        /// <returns>A color for the residue.</returns>
        private static Color GetResidueColor(string residueName)
        {
            if (residueName == "HOH") return Color.Red;
            else if (residueName == "ALA") return new Color(199, 199, 199);
            else if (residueName == "ARG") return new Color(229, 10, 10);
            else if (residueName == "CYS") return new Color(229, 229, 0);
            else if (residueName == "GLN") return new Color(0, 229, 229);
            else if (residueName == "GLU") return new Color(229, 10, 10);
            else if (residueName == "GLY") return new Color(234, 234, 234);
            else if (residueName == "HIS") return new Color(130, 130, 209);
            else if (residueName == "ILE") return new Color(15, 130, 15);
            else if (residueName == "LEU") return new Color(15, 130, 15);
            else if (residueName == "LYS") return new Color(20, 90, 255);
            else if (residueName == "MET") return new Color(229, 229, 0);
            else if (residueName == "PHE") return new Color(50, 50, 169);
            else if (residueName == "PRO") return new Color(219, 149, 130);
            else if (residueName == "SER") return new Color(249, 149, 0);
            else if (residueName == "THR") return new Color(249, 149, 0);
            else if (residueName == "TRP") return new Color(179, 90, 179);
            else if (residueName == "TYR") return new Color(50, 50, 169);
            else if (residueName == "VAL") return new Color(15, 130, 15);
            else if (residueName == "ASN") return new Color(0, 229, 229);
            else return Color.Green;
        }

        /// <summary>
        /// Toggles visibility of 3D model components based on the
        /// <see cref="Molecule.ShowCartoon" /> property.
        /// </summary>
        /// <param name="sender">The molecule.</param>
        /// <param name="e">Empty event args.</param>
        private void MoleculeShowCartoonChanged(object sender, EventArgs e)
        {
            /*if (_ribbon != null)
            {
                if (_molecule.ShowCartoon && _cartoon == null)
                {
                    _cartoon = new Cartoon(this, _color, TODO);
                }
            }*/
        }

        /// <summary>
        /// Selects the material color for this residue based on the coloring method.
        /// </summary>
        private void UpdateColorView()
        {
            if (_colorScheme == ColorScheme.Structure)
                _color = _structureColor;
            else if (_colorScheme == ColorScheme.Atom && _residueIdentifier == "O")
                _color = Color.Red;
            else if (_colorScheme == ColorScheme.Atom)
                _color = Color.LightGray;
            else if (_colorScheme == ColorScheme.Residue)
                _color = _residueColor;
            else if (_colorScheme == ColorScheme.Chain && _chain != null)
                _color = _chain.ChainColor;
            else if (_colorScheme == ColorScheme.Temperature)
                _color = Atom.GetAverageTemperateColor(_atoms);
            else
                _color = Color.LightGray;

            UpdateView();
        }

        /// <summary>
        /// Updates the material color for this atom based on the coloring method and the current
        /// hover state.
        /// </summary>
        private void UpdateView()
        {
/*            Color actualColor = _color;

            if (IsHovered)
            {
                byte r = (byte)(_color.R + (255 - _color.R) / 2);
                byte g = (byte)(_color.G + (255 - _color.G) / 2);
                byte b = (byte)(_color.B + (255 - _color.B) / 2);

                if (r == g && g == b) r = g = b = 255;

                actualColor = new Color(r, g, b);
            }

            SolidColorBrush foreground = new SolidColorBrush(actualColor);
            SolidColorBrush background = Brushes.Transparent;

            if (_showAsSelection == SelectionType.Partial)
            {
                foreground = new SolidColorBrush(actualColor);
                background = new SolidColorBrush(
                    Color.FromArgb(96, actualColor.R, actualColor.G, actualColor.B));
            }
            else if (_showAsSelection == SelectionType.Full)
            {
                foreground = Brushes.Black;
                background = new SolidColorBrush(actualColor);
            }

            foreach (ResidueStripItem residueStripItem in _residueStripItems)
            {
                residueStripItem.Label.Foreground = foreground;
                residueStripItem.Label.Background = background;
            }

            if (cartoon != null)
                cartoon.Color = actualColor;*/
        }
    }
}