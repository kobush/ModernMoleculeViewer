using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ModernMoleculeViewer.Mvvm;
using ModernMoleculeViewer.Views;

namespace ModernMoleculeViewer.ViewModels
{
    public class MainPageViewModel : BindableBase
    {
        private readonly DelegateCommand<MoleculeInfo> _openCommand;
        private IEnumerable<MoleculeInfo> _molecules;

        public MainPageViewModel()
        {
            _openCommand = new DelegateCommand<MoleculeInfo>(OpenExecute);
        }

        public void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode)
        {
            if (Molecules == null || navigationMode != NavigationMode.Back)
                LoadSampleFiles();
        }

        private async void LoadSampleFiles()
        {
            var molecules = new ObservableCollection<MoleculeInfo>();
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Sample PDBs");
            var files = await folder.GetFilesAsync();

            foreach (var file in files)
            {
                if (file.FileType == ".txt") // .pdb was blocked
                {
                    var molecule = new MoleculeInfo();
                    molecule.File = file;
                    molecule.Name = file.DisplayName;

                    var thumbnailPath = Path.ChangeExtension(file.Path, ".png"); 
                    var thumbnail = files.FirstOrDefault(f => f.Path == thumbnailPath);
                    if (thumbnail != null)
                        molecule.Thumbnail = thumbnailPath;

                    molecules.Add(molecule);
                }
            }

            /*if (_log.IsInfoEnabled)
                _log.Info("Loaded {0} sample PDB files.", molecules.Count);*/

            Molecules = molecules;
        }

        public IEnumerable<MoleculeInfo> Molecules
        {
            get { return _molecules; }
            set { SetProperty(ref _molecules, value); }
        }

        public ICommand OpenCommand
        {
            get { return _openCommand; }
        }

        public Frame Frame { get; set; }

        private async void OpenExecute(MoleculeInfo molecule)
        {
            StorageFile file = null;
            if (molecule != null)
            {
                file = molecule.File;
            }
/*            else
            {
                var filePicker = new FileOpenPicker();
                filePicker.FileTypeFilter.Add(".pdb");

                filePicker.ViewMode = PickerViewMode.List;
                filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                filePicker.SettingsIdentifier = "picker1";
                filePicker.CommitButtonText = "Open File";

                file = await filePicker.PickSingleFileAsync().AsTask();
            }*/

            if (file != null)
            {
                /*if (_log.IsInfoEnabled)
                    _log.Info("Opening PDB file {0}.", file.Name);*/

                Frame.Navigate(typeof (MoleculePage), new MoleculePageParams {File = file});
            }
        }
    }
}
