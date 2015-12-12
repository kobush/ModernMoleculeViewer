using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// Represents a molecule and is constructed from a PDB file stream.
    /// </summary>
    /// <remarks>
    /// This class contains references to the other business objects that make up 
    /// a molecule and centralizes the functionality to generate the necessary 3D meshes.
    /// </remarks>
    public class Molecule
    {
        private List<Atom> _atoms;
        private List<Residue> _residues;
        private List<Chain> _chains;
        private List<Structure> _structures;
        private List<Ribbon> _ribbons;

        private bool _showCartoon;
        private bool _showBackbone;
        private bool _showFullChain;
        private bool _showHetAtoms;
        private bool _showWaters;

        private ColorScheme _colorScheme;

        /// <summary>
        /// Parses a PDB stream and build the constituent objects.
        /// </summary>
        /// <param name="pdbStream">The PDB stream.</param>
        internal Molecule(Stream pdbStream)
        {
            CreateAtomsAndStructures(pdbStream);
            CreateBackbone();
            CreateBonds();
            CreateResidues();
            CreateChains();
            CreateMoleculeTransform();

            Atom.SetBFactorColors(_atoms);

            SetStructureInfo();
            CreateRibbons();

            foreach (Atom atom in _atoms) 
                atom.Initialize();

            ShowCartoon = true;

            CreateModel();
        }

        /// <summary>
        /// Allows part of the molecule to hide or show parts related to the cartoon view.
        /// </summary>
        internal event EventHandler ShowCartoonChanged;

        /// <summary>
        /// Allows part of the molecule to hide or show parts related to the backbone view.
        /// </summary>
        internal event EventHandler ShowBackboneChanged;

        /// <summary>
        /// Allows part of the molecule to hide or show parts related to the full chain view.
        /// </summary>
        internal event EventHandler ShowFullChainChanged;

        /// <summary>
        /// Allows part of the molecule to hide or show parts related to het atoms.
        /// </summary>
        internal event EventHandler ShowHetAtomsChanged;

        /// <summary>
        /// Allows part of the molecule to hide or show parts related to waters.
        /// </summary>
        internal event EventHandler ShowWatersChanged;
        
        internal event EventHandler ColorSchemeChanged;

        /// <summary>
        /// All of the atoms contained in the molecule.
        /// </summary>
        internal List<Atom> Atoms { get { return _atoms; } }

        /// <summary>
        /// All of the residues (amino acids) contained in the molecule.
        /// </summary>
        internal List<Residue> Residues { get { return _residues; } }

        /// <summary>
        /// All of the chains of amino acids contained in the molecule.
        /// </summary>
        internal List<Chain> Chains { get { return _chains; } }

/*        /// <summary>
        /// The current orientation of the molecule. This is used by a
        /// <see cref="StructureControl"> to translate atom positions from world space to screen
        /// space.</see>
        /// </summary>
        internal Transform3D MoleculeTransform { get { return moleculeTransformGroup; } }

        /// <summary>
        /// The Model3D for the molecule which contains all constituent parts. This can be added to
        /// a Visual3D and then diplayed in a Viewport3D.
        /// </summary>
        internal Model3DGroup Model { get { return model; } }*/

        /// <summary>
        /// Determines whether parts of the molecule related to the cartoon view are shown.
        /// </summary>
        internal bool ShowCartoon
        {
            get { return _showCartoon; }
            set
            {
                if (_showCartoon != value)
                {
                    _showCartoon = value;

                    if (ShowCartoonChanged != null)
                        ShowCartoonChanged(this, EventArgs.Empty);

                    if (_showCartoon) ShowBackbone = false;
                    if (_showCartoon) ShowFullChain = false;
                }
            }
        }

        /// <summary>
        /// Determines whether parts of the molecule related to the backbone view are shown.
        /// </summary>
        internal bool ShowBackbone
        {
            get { return _showBackbone; }
            set
            {
                if (_showBackbone != value)
                {
                    _showBackbone = value;

                    if (ShowBackboneChanged != null)
                        ShowBackboneChanged(this, EventArgs.Empty);

                    if (_showBackbone) ShowCartoon = false;
                    if (_showBackbone) ShowFullChain = false;
                }
            }
        }

        /// <summary>
        /// Determines whether parts of the molecule related to the full chain view are shown.
        /// </summary>
        internal bool ShowFullChain
        {
            get { return _showFullChain; }
            set
            {
                if (_showFullChain != value)
                {
                    _showFullChain = value;

                    if (ShowFullChainChanged != null)
                        ShowFullChainChanged(this, EventArgs.Empty);

                    if (_showFullChain) ShowBackbone = false;
                    if (_showFullChain) ShowCartoon = false;
                }
            }
        }

        /// <summary>
        /// Determines whether parts of the molecule related to het atoms are shown.
        /// </summary>
        internal bool ShowHetAtoms
        {
            get { return _showHetAtoms; }
            set
            {
                if (_showHetAtoms != value)
                {
                    _showHetAtoms = value;

                    if (ShowHetAtomsChanged != null)
                        ShowHetAtomsChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Determines whether parts of the molecule related to waters are shown.
        /// </summary>
        internal bool ShowWaters
        {
            get { return _showWaters; }
            set
            {
                if (_showWaters != value)
                {
                    _showWaters = value;

                    if (ShowWatersChanged != null)
                        ShowWatersChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The current molecule coloring method.
        /// </summary>
        internal ColorScheme ColorScheme
        {
            get { return _colorScheme; }
            set
            {
                if (_colorScheme != value)
                {
                    _colorScheme = value;

                    foreach (Residue residue in _residues)
                        residue.ColorScheme = _colorScheme;

                    foreach (Atom atom in _atoms)
                        atom.ColorScheme = _colorScheme;

                    if (ColorSchemeChanged != null)
                        ColorSchemeChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Provides a list of all currently selected atoms.
        /// </summary>
        internal List<Atom> SelectedAtoms
        {
            get
            {
                List<Atom> selectedAtoms = new List<Atom>();

                foreach (Atom atom in _atoms)
                    if (atom.IsSelected) selectedAtoms.Add(atom);

                return selectedAtoms;
            }
        }

        /*       /// <summary>
               /// Calculates a decent orientation of the molecule based on the bounding box of the
               /// currently selected atoms. Used for animating to the center of a selection.
               /// </summary>
               /// <returns>The generated view state representing the desired orientation.</returns>
               internal PdbViewState GetSelectionViewState()
               {
                   Vector3D center = new Vector3D();

                   foreach (Atom atom in SelectedAtoms)
                   {
                       center.X += atom.Position.X;
                       center.Y += atom.Position.Y;
                       center.Z += atom.Position.Z;
                   }

                   center.X /= SelectedAtoms.Count;
                   center.Y /= SelectedAtoms.Count;
                   center.Z /= SelectedAtoms.Count;

                   double radius = 16;

                   foreach (Atom atom in SelectedAtoms)
                   {
                       Vector3D vector = new Vector3D(atom.Position.X - center.X,
                           atom.Position.Y - center.Y, atom.Position.Z - center.Z);

                       radius = Math.Max(radius, vector.LengthSquared);
                   }

                   radius = Math.Sqrt(radius);

                   center.X += translateTransform.OffsetX;
                   center.Y += translateTransform.OffsetY;
                   center.Z += translateTransform.OffsetZ;

                   double moleculeRadius = 1.25 / scaleTransform.ScaleX;

                   double scale = 0.8 * moleculeRadius / radius;

                   Vector3D cameraVector = new Vector3D(0, 0, 1);

                   Vector3D axis = Vector3D.CrossProduct(center, cameraVector);
                   double angle = Vector3D.AngleBetween(center, cameraVector);

                   PdbViewState pdbViewState = new PdbViewState();

                   pdbViewState.Scale = scale;

                   pdbViewState.Translation = -center * scaleTransform.ScaleX;

                   if (axis.LengthSquared > 0)
                       pdbViewState.Rotation = new Quaternion(axis, angle);

                   return pdbViewState;
               }*/

        /// <summary>
        /// Called by the contructor to parse the portions of the PDB file related to atoms and
        /// secondary structures.
        /// </summary>
        /// <param name="pdbStream">The PDB stream.</param>
        private void CreateAtomsAndStructures(Stream pdbStream)
        {
            _atoms = new List<Atom>();
            _structures = new List<Structure>();

            using (StreamReader pdbReader = new StreamReader(pdbStream))
            {
                string pdbLine = pdbReader.ReadLine();

                while (pdbLine != null)
                {
                    if (pdbLine.StartsWith("ENDMDL")) break;

                    if (pdbLine.StartsWith("HELIX") || pdbLine.StartsWith("SHEET"))
                        _structures.Add(Structure.CreateStructure(pdbLine));

                    if (pdbLine.StartsWith("ATOM") || pdbLine.StartsWith("HETATM"))
                        _atoms.Add(Atom.CreateAtom(this, pdbLine));

                    pdbLine = pdbReader.ReadLine();
                }
            }
        }

        /// <summary>
        /// Called by the contructor after creating the <see cref="Atom"/> objects to identify the
        /// backbone atoms and connect them via referneces.
        /// </summary>
        private void CreateBackbone()
        {
            CAlpha previousCAlpha = null;

            foreach (Atom atom in _atoms)
            {
                CAlpha nextCAlpha = atom as CAlpha;

                if (nextCAlpha != null)
                {
                    if (previousCAlpha != null &&
                        nextCAlpha.ChainIdentifier == previousCAlpha.ChainIdentifier)
                    {
                        previousCAlpha.NextCAlpha = nextCAlpha;
                        nextCAlpha.PreviousCAlpha = previousCAlpha;
                    }

                    previousCAlpha = nextCAlpha;
                }
            }
        }

        /// <summary>
        /// Called by the contructor after creating the <see cref="Atom"/> objects to identify
        /// covalently bonded atoms. Uses a simple distance heuristic of six angstroms.
        /// </summary>
        private void CreateBonds()
        {
            for (int i = 0; i < _atoms.Count - 1; i++)
            {
                Atom atom1 = _atoms[i];

                if (atom1 is Water) continue;

                float x1 = atom1.Position.X;
                float y1 = atom1.Position.Y;
                float z1 = atom1.Position.Z;

                for (int j = i + 1; j < _atoms.Count; j++)
                {
                    Atom atom2 = _atoms[j];

                    if (atom2 is Water) continue;

                    //double distanceSquared = Math.Pow(x1 - atom2.Position.X, 2);
                    float distanceSquared = (x1 - atom2.Position.X) * (x1 - atom2.Position.X);
                    if (distanceSquared > 3.6f) continue;

                    //distanceSquared += Math.Pow(y1 - atom2.Position.Y, 2);
                    distanceSquared += (y1 - atom2.Position.Y)*(y1 - atom2.Position.Y);
                    if (distanceSquared > 3.6f) continue;

                    distanceSquared += (z1 - atom2.Position.Z) * (z1 - atom2.Position.Z);
                    if (distanceSquared > 3.6f) continue;

                    float distance = (float) Math.Sqrt(distanceSquared);

                    atom1.Bonds.Add(atom2, distance);
                    atom2.Bonds.Add(atom1, distance);
                }
            }

            Atom.SetBFactorColors(_atoms);
        }

        /// <summary>
        /// Called by the constructor to create <see cref="Residue"/> objects and group their
        /// constituent atoms.
        /// </summary>
        private void CreateResidues()
        {
            _residues = new List<Residue>();

            Residue residue = null;

            foreach (Atom atom in _atoms)
            {
                if (residue == null || atom.ResidueSequenceNumber != residue.ResidueSequenceNumber ||
                    atom.ChainIdentifier != residue.ChainIdentifier)
                {
                    residue = new Residue(this, atom);
                    _residues.Add(residue);
                }
                else
                {
                    residue.Atoms.Add(atom);
                }

                atom.Residue = residue;
            }
        }

        /// <summary>
        /// Called by the constructor to create <see cref="Chain"/> objects and group their
        /// constituent residues (amino acids).
        /// </summary>
        private void CreateChains()
        {
            _chains = new List<Chain>();

            Chain chain = null;
            Chain waters = null;

            foreach (Residue residue in _residues)
            {
                if (residue.ChainIdentifier == "")
                {
                    if (waters == null) waters = new Chain("");

                    waters.Residues.Add(residue);
                    residue.Chain = waters;
                }
                else
                {
                    if (chain == null || residue.ChainIdentifier != chain.ChainIdentifier)
                    {
                        chain = new Chain(residue.ChainIdentifier);
                        _chains.Add(chain);
                    }

                    chain.Residues.Add(residue);
                    residue.Chain = chain;
                }
            }

            if (waters != null) _chains.Add(waters);

            Chain.SetChainColors(_chains);
        }

        /// <summary>
        /// Called by the constructor to calculate the default scale and translation of the
        /// molecule based on the bounding box of the atoms.
        /// </summary>
        private void CreateMoleculeTransform()
        {
/*            Rect3D bounds = Atom.GetBounds(atoms);

            moleculeTransformGroup = new Transform3DGroup();

            translateTransform = new TranslateTransform3D();
            translateTransform.OffsetX = -(bounds.X + bounds.SizeX / 2);
            translateTransform.OffsetY = -(bounds.Y + bounds.SizeY / 2);
            translateTransform.OffsetZ = -(bounds.Z + bounds.SizeZ / 2);
            moleculeTransformGroup.Children.Add(translateTransform);

            double scale = 2.5 / Math.Max(bounds.SizeX, Math.Max(bounds.SizeY, bounds.SizeZ));

            scaleTransform = new ScaleTransform3D(scale, scale, scale);
            moleculeTransformGroup.Children.Add(scaleTransform);*/
        }

        /// <summary>
        /// Called by the constructor to set secondary structure related properties on the residues
        /// and atoms that compose each structure.
        /// </summary>
        private void SetStructureInfo()
        {
            foreach (Atom atom in _atoms)
                if (atom is ChainAtom) atom.StructureColor = Color.LightGray;

            foreach (Residue residue in _residues)
            {
                foreach (Structure structure in _structures)
                {
                    if (residue.ChainIdentifier == structure.ChainIdentifier &&
                        residue.ResidueSequenceNumber >= structure.StartResidueSequenceNumber &&
                        residue.ResidueSequenceNumber <= structure.EndResidueSequenceNumber)
                    {
                        if (structure is Sheet) residue.IsSheet = true;
                        else if (structure is Helix) residue.IsHelix = true;

                        residue.StructureColor = structure.Color;

                        foreach (Atom atom in residue.Atoms)
                            atom.StructureColor = structure.Color;

                        break;
                    }
                }
            }

            Residue previousResidue = null;

            foreach (Residue residue in _residues)
            {
                CAlpha cAlpha = null;
                ChainAtom carbonylOxygen = null;

                foreach (Atom atom in residue.Atoms)
                    if (atom is CAlpha)
                        cAlpha = (CAlpha)atom;

                if (cAlpha != null)
                {
                    foreach (Atom atom in residue.Atoms)
                        if (atom is ChainAtom && atom.AtomName == "O")
                            carbonylOxygen = (ChainAtom)atom;
                }

                if (cAlpha == null || carbonylOxygen == null)
                {
                    if (previousResidue != null)
                    {
                        previousResidue.IsStructureEnd = true;
                        previousResidue = null;
                    }

                    continue;
                }
                else
                {
                    residue.CAlphaPosition = cAlpha.Position;
                    residue.CarbonylOxygenPosition = carbonylOxygen.Position;
                }

                if (previousResidue != null && previousResidue.Chain != residue.Chain)
                {
                    previousResidue.IsStructureEnd = true;
                    previousResidue = null;
                }

                if (previousResidue != null)
                {
                    previousResidue.NextResidue = residue;
                    residue.PreviousResidue = previousResidue;

                    if (previousResidue.Chain != residue.Chain ||
                        previousResidue.IsSheet != residue.IsSheet ||
                        previousResidue.IsHelix != residue.IsHelix)
                    {
                        previousResidue.IsStructureEnd = true;
                        residue.IsStructureStart = true;
                    }
                }
                else
                {
                    residue.IsStructureStart = true;
                }

                previousResidue = residue;
            }

            if (previousResidue != null)
                previousResidue.IsStructureEnd = true;
        }

        /// <summary>
        /// Called by the constructor to create <see cref="Ribbon"/> objects which are used to
        /// compute the spline curves for secondary struction representations.
        /// </summary>
        private void CreateRibbons()
        {
            _ribbons = new List<Ribbon>();

            Ribbon currentRibbon = null;
            Residue previousResidue = null;

            foreach (Residue residue in _residues)
            {
                if (residue.CAlphaPosition == null)
                {
                    currentRibbon = null;
                }
                else
                {
                    if (currentRibbon == null ||
                        residue.ChainIdentifier != previousResidue.ChainIdentifier)
                    {
                        currentRibbon = new Ribbon();
                        _ribbons.Add(currentRibbon);
                    }

                    residue.Ribbon = currentRibbon;
                    currentRibbon.Residues.Add(residue);

                    previousResidue = residue;
                }
            }

            foreach (Ribbon ribbon in _ribbons)
                ribbon.CreateControlPoints();
        }

        /// <summary>
        /// Called by the constructor to create the container Model3DGroup for the molecule's 3D
        /// model.
        /// </summary>
        private void CreateModel()
        {
/*            model = new Model3DGroup();
            model.Transform = moleculeTransformGroup;

            foreach (Atom atom in atoms)
                model.Children.Add(atom.Model);

            foreach (Residue residue in residues)
                model.Children.Add(residue.Model);

            annotationMarkerModel = new Model3DGroup();
            model.Children.Add(annotationMarkerModel);*/
        }
    }
}
