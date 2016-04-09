using SharpDX;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// <see cref="Structure "/> subclass for helix structures.
    /// </summary>
    internal class Helix : Structure
    {
        /// <summary>
        /// Returns the color to use for helix structures.
        /// </summary>
        internal override Color Color { get { return Color.Blue; } }

        /// <summary>
        /// Returns the PDB file helix structure record column for the chain identifier.
        /// </summary>
        protected override int ChainIdentifierColumn { get { return 19; } }

        /// <summary>
        /// Returns the PDB file helix structure record column for the starting residue sequence
        /// number.
        /// </summary>
        protected override int StartResidueSequenceNumberColumn { get { return 21; } }

        /// <summary>
        /// Returns the PDB file helix structure record column for the ending residue sequence
        /// number.
        /// </summary>
        protected override int EndResidueSequenceNumberColumn { get { return 33; } }
    }
}