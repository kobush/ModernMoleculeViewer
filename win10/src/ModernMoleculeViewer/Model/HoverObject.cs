namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// Abstract base class for objects that support changing their visual state when hovered and
    /// displaying <see cref="HoverPopup" /> windows.
    /// </summary>
    internal abstract class HoverObject
    {
        private bool isHovered;

        /// <summary>
        /// Label used for atom tooltips.
        /// </summary>
        internal virtual string DisplayName
        {
            get { return ""; }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether or not the mouse pointer is currently
        /// hovering over the object.
        /// </summary>
        internal bool IsHovered
        {
            get
            {
                return this.isHovered;
            }
            set
            {
                this.isHovered = value;
                this.OnIsHoveredChanged();
            }
        }

        /// <summary>
        /// Abstract method for subclasses to update their visual appearance based on the hovered
        /// state.
        /// </summary>
        protected abstract void OnIsHoveredChanged();
    }
}