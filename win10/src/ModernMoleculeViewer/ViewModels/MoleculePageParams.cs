using System.Runtime.Serialization;

namespace ModernMoleculeViewer.ViewModels
{
    [KnownType(typeof(MoleculePageParams))]
    [DataContract]
    public class MoleculePageParams
    {
        public string FileName { get; set; }
    }
} 