using SharpDX;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// <see cref="Structure "/> subclass for sheet structures.
    /// </summary>
    internal class Sheet : Structure
    {
        /// <summary>
        /// Returns the color to use for sheet structures.
        /// </summary>
        internal override Color Color { get { return Color.Orange; } }

        /// <summary>
        /// Returns the PDB file sheet structure record column for the chain identifier.
        /// </summary>
        protected override int ChainIdentifierColumn { get { return 21; } }

        /// <summary>
        /// Returns the PDB file sheet structure record column for the starting residue sequence
        /// number.
        /// </summary>
        protected override int StartResidueSequenceNumberColumn { get { return 22; } }

        /// <summary>
        /// Returns the PDB file sheet structure record column for the ending residue sequence
        /// number.
        /// </summary>
        protected override int EndResidueSequenceNumberColumn { get { return 33; } }
    }
}