using System;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// <see cref="Atom" /> subclass for water atoms.
    /// </summary>
    /// <remarks>
    /// Adds functionality to toggle visibility based on the <see cref="Molecule.ShowWaters" /> property.
    /// </remarks>
    class Water : Atom
    {
        /// <summary>
        /// Attaches an event handedler to <see cref="Molecule.ShowWatersChanged" />.
        /// </summary>
        internal override void Initialize()
        {
            base.Initialize();

            Molecule.ShowWatersChanged += MoleculeShowWatersChanged;
        }

        /// <summary>
        /// Toggles visibility based on the <see cref="Molecule.ShowWaters" /> property.
        /// </summary>
        /// <param name="sender">The molecule.</param>
        /// <param name="e">Empty event args.</param>
        private void MoleculeShowWatersChanged(object sender, EventArgs e)
        {
            //RenderAtomModel(Molecule.ShowWaters);
        }
    }
}