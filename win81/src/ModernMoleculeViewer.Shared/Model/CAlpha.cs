using System;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// <see cref="Atom "/> subclass for alpha carbon protein backbone atoms. 
    /// </summary>
    /// <remarks>
    /// Adds functionality to manage the atom's backbone 3D model.
    /// </remarks>
    internal class CAlpha : ChainAtom
    {
        private CAlpha _previousCAlpha;
        private CAlpha _nextCAlpha;

        /// <summary>
        /// Gets and sets a reference to the previous <see cref="CAlpha" /> in the backbone.
        /// </summary>
        internal CAlpha PreviousCAlpha
        {
            get { return _previousCAlpha; }
            set { _previousCAlpha = value; }
        }

        /// <summary>
        /// Gets and sets a reference to the next <see cref="CAlpha" /> in the backbone.
        /// </summary>
        internal CAlpha NextCAlpha
        {
            get { return _nextCAlpha; }
            set { _nextCAlpha = value; }
        }

        /// <summary>
        /// Attaches an event handedler to <see cref="Molecule.ShowBackboneChanged" />.
        /// </summary>
        internal override void Initialize()
        {
            base.Initialize();

            Molecule.ShowBackboneChanged += MoleculeShowBackboneChanged;
        }

/*        /// <summary>
        /// Performs hit testing for the backbone 3D model.
        /// </summary>
        /// <param name="rayHitTestResult">A 3D mesh hit test result from the WPF visual tree hit
        /// testing framework</param>
        /// <returns>True if the mesh hit belongs to the backbone, otherwise false.</returns>
        internal override bool HoverHitTest(RayMeshGeometry3DHitTestResult rayHitTestResult)
        {
            return (this._backboneModel != null &&
                    this._backboneModel.Children.Contains(rayHitTestResult.ModelHit)) ||
                   base.HoverHitTest(rayHitTestResult);
        }*/

        /// <summary>
        /// Toggles visibility based on the <see cref="Molecule.ShowBackbone" /> property.
        /// </summary>
        /// <param name="sender">The molecule.</param>
        /// <param name="e">Empty event args.</param>
        private void MoleculeShowBackboneChanged(object sender, EventArgs e)
        {

        }

 
    }
}