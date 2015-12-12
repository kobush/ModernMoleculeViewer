using Windows.Storage;
using Microsoft.Practices.Prism.StoreApps;

namespace ModernMoleculeViewer.ViewModels
{
    public class MoleculeInfo : BindableBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public StorageFile File { get; set; }
        public string Thumbnail { get; set; }
    }
}