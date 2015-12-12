using System;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// <see cref="Atom" /> subclass for atoms that are not waters and are not standard protein chain atoms.
    /// </summary>
    /// <remarks>
    /// Adds functionality to toggle visibility based on the <see cref="Molecule.ShowHetAtoms" /> property.
    /// </remarks>
    internal class HetAtom : Atom
    {
        /// <summary>
        /// Attaches an event handedler to <see cref="Molecule.ShowHetAtomsChanged" />.
        /// </summary>
        internal override void Initialize()
        {
            base.Initialize();

            Molecule.ShowHetAtomsChanged += MoleculeShowHetAtomsChanged;
        }

/*        /// <summary>
        /// Override to check <see cref="Molecule.ShowHetAtoms" /> when the atom's selection state
        /// is changed.
        /// </summary>
        protected override void UpdateSelectionView()
        {
            base.UpdateSelectionView();

            RenderAtomModel(Molecule.ShowHetAtoms || ShowAsSelected);
        }*/

        /// <summary>
        /// Toggles visibility based on the <see cref="Molecule.ShowHetAtoms" /> property.
        /// </summary>
        /// <param name="sender">The molecule.</param>
        /// <param name="e">Empty event args.</param>
        private void MoleculeShowHetAtomsChanged(object sender, EventArgs e)
        {
/*
            RenderAtomModel(Molecule.ShowHetAtoms || ShowAsSelected);
*/
        }
    }
}