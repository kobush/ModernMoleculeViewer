using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;
using MetroLog;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SharpDX.Collections;

namespace ModernMoleculeViewer.ViewModels
{
    public class MainPageViewModel : ViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly DelegateCommand<MoleculeInfo> _openCommand;
        private readonly ILogger _log;
        
        private ObservableCollection<MoleculeInfo> _molecules;

        public MainPageViewModel(INavigationService navigationService, ILogManager logManager)
        {
            _log = logManager.GetLogger<MainPageViewModel>();
            _navigationService = navigationService;
            _openCommand = new DelegateCommand<MoleculeInfo>(OpenExecute);
        }

        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

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

            if (_log.IsInfoEnabled)
                _log.Info("Loaded {0} sample PDB files.", molecules.Count);

            Molecules = molecules;
        }


        public ObservableCollection<MoleculeInfo> Molecules
        {
            get { return _molecules; }
            private set { SetProperty(ref _molecules, value); }
        }

        public ICommand OpenCommand
        {
            get { return _openCommand; }
        }

        private async void OpenExecute(MoleculeInfo molecule)
        {
            StorageFile file;
            if (molecule != null)
            {
                file = molecule.File;
            }
            else
            {
                var filePicker = new FileOpenPicker();
                filePicker.FileTypeFilter.Add(".pdb");

                filePicker.ViewMode = PickerViewMode.List;
                filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                filePicker.SettingsIdentifier = "picker1";
                filePicker.CommitButtonText = "Open File";

                file = await filePicker.PickSingleFileAsync().AsTask();

            }
            
            if (file != null)
            {
                if (_log.IsInfoEnabled)
                    _log.Info("Opening PDB file {0}.", file.Name);

                _navigationService.Navigate("Molecule", new MoleculePageParams { File = file });
            }
        }
    }
}
