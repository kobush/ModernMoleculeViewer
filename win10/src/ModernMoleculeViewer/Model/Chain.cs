using System.Collections.Generic;
using SharpDX;

namespace ModernMoleculeViewer.Model
{
    /// <summary>
    /// Container object to group residues by chain and set chain-based temperature colors.
    /// </summary>
    internal class Chain
    {
        private readonly string _chainIdentifier;
        private readonly List<Residue> _residues;
        private Color _chainColor;

        /// <summary>
        /// Creates the chain container object.
        /// </summary>
        /// <param name="chainIdentifier"></param>
        internal Chain(string chainIdentifier)
        {
            _chainIdentifier = chainIdentifier;
            _residues = new List<Residue>();
        }

        /// <summary>
        /// Alphanumeric chain identifier.
        /// </summary>
        internal string ChainIdentifier { get { return _chainIdentifier; } }

        /// <summary>
        /// Color used for this chain.
        /// </summary>
        internal Color ChainColor { get { return _chainColor; } }

        /// <summary>
        /// A list of the constituent residues.
        /// </summary>
        internal List<Residue> Residues { get { return _residues; } }

        /// <summary>
        /// Assigns colors to all the chains for a molecule.
        /// </summary>
        /// <param name="chains">A list of chains.</param>
        internal static void SetChainColors(List<Chain> chains)
        {
            for (int index = 0; index < chains.Count; index++)
            {
                if (chains[index]._chainIdentifier == "")
                    chains[index]._chainColor = Color.Red;
                else
                    chains[index]._chainColor = GetChainColor(index);
            }
        }

        /// <summary>
        /// Selects one of five chain colors.
        /// </summary>
        /// <param name="index">A chain color index.</param>
        /// <returns>A color.</returns>
        private static Color GetChainColor(int index)
        {
            index = index % 5;

            switch (index)
            {
                case 0:
                    return Color.Blue;
                case 1:
                    return Color.Yellow;
                case 2:
                    return Color.Green;
                case 3:
                    return Color.Orange;
                default:
                    return Color.Purple;
            }
        }
    }
}