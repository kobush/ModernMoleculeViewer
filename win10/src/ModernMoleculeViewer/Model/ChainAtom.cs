using System;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// <see cref="Atom" /> subclass for standard protein chain atoms. 
    /// </summary>
    /// <remarks>
    /// Adds functionality to toggle visibility based on the <see cref="Molecule.ShowFullChain" /> property.
    /// </remarks>
    internal class ChainAtom : Atom
    {
        /// <summary>
        /// Attaches an event handedler to <see cref="Molecule.ShowFullChainChanged" />.
        /// </summary>
        internal override void Initialize()
        {
            base.Initialize();

            Molecule.ShowFullChainChanged += MoleculeShowFullChainChanged;
        }

/*
        /// <summary>
        /// Override to check <see cref="Molecule.ShowFullChain" /> when the atom's selection state
        /// is changed.
        /// </summary>
        protected override void UpdateSelectionView()
        {
            base.UpdateSelectionView();

            RenderAtomModel(Molecule.ShowFullChain || ShowAsSelected);
        }*/

        /// <summary>
        /// Toggles visibility based on the <see cref="Molecule.ShowFullChain" /> property.
        /// </summary>
        /// <param name="sender">The molecule.</param>
        /// <param name="e">Empty event args.</param>
        private void MoleculeShowFullChainChanged(object sender, EventArgs e)
        {
/*
            RenderAtomModel(Molecule.ShowFullChain || ShowAsSelected);
*/
        }
    }
}