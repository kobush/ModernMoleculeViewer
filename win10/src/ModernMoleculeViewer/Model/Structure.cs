using System;
using SharpDX;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// Abstract base class for secondary structures.
    /// </summary>
    /// <remarks>
    /// Contains attributes about the structure that are parsed from a PDB file.
    /// </remarks>
    internal abstract class Structure
    {
        private string _chainIdentifier;
        private int _startResidueSequenceNumber;
        private int _endResidueSequenceNumber;

        /// <summary>
        /// Gets the color used when the structure-based coloring method is used.
        /// </summary>
        internal abstract Color Color { get; }

        /// <summary>
        /// Alphanumeric chain identifier for the chain this structure belongs to.
        /// </summary>
        internal string ChainIdentifier { get { return _chainIdentifier; } }

        /// <summary>
        /// The residue index number for the first amino acid in this structure.
        /// </summary>
        internal int StartResidueSequenceNumber { get { return _startResidueSequenceNumber; } }

        /// <summary>
        /// The residue index number for the last amino acid in this structure.
        /// </summary>
        internal int EndResidueSequenceNumber { get { return _endResidueSequenceNumber; } }

        /// <summary>
        /// Static method for parsing structure entries in a pdb file and instantiating the correct
        /// <see cref="Structure" /> subclass.
        /// </summary>
        /// <param name="pdbLine">A structure entry from a pdb file.</param>
        /// <returns>An instance of a <see cref="Structure" /> subclass.</returns>
        internal static Structure CreateStructure(string pdbLine)
        {
            Structure structure;

            if (pdbLine.StartsWith("HELIX"))
                structure = new Helix();
            else if (pdbLine.StartsWith("SHEET"))
                structure = new Sheet();
            else 
                return null;

            structure._chainIdentifier = pdbLine.Substring(structure.ChainIdentifierColumn, 1);

            structure._startResidueSequenceNumber =
                Convert.ToInt32(pdbLine.Substring(structure.StartResidueSequenceNumberColumn, 4));
            structure._endResidueSequenceNumber =
                Convert.ToInt32(pdbLine.Substring(structure.EndResidueSequenceNumberColumn, 4));

            return structure;
        }

        /// <summary>
        /// Abstract property that returns the PDB file structure record column for the chain
        /// identifier.
        /// </summary>
        protected abstract int ChainIdentifierColumn { get; }

        /// <summary>
        /// Abstract property that returns the PDB file structure record column for the starting
        /// residue sequence number.
        /// </summary>
        protected abstract int StartResidueSequenceNumberColumn { get; }

        /// <summary>
        /// Abstract property that returns the PDB file structure record column for the ending
        /// residue sequence number.
        /// </summary>
        protected abstract int EndResidueSequenceNumberColumn { get; }
    }
}