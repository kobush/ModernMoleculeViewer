using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ModernMoleculeViewer.Model;
using ModernMoleculeViewer.Mvvm;
using ModernMoleculeViewer.Rendering;

namespace ModernMoleculeViewer.ViewModels
{
    public class MoleculePageViewModel : BindableBase
    {
/*
        private readonly INavigationService _navigationService;
        private readonly ILogger _log;
*/

        private readonly MoleculeViewerEngine _engine;

        private Molecule _molecule;
        private string _title;

        private readonly ICommand _goBackCommand;
        private readonly ICommand _resetViewCommand;

        private bool _showHetAtoms;
        private bool _showWaters;
        private bool _showBackbone;
        private bool _showFullChain;
        private bool _showCartoon = true;

        private CancellationTokenSource _loadingTokenSource;

        private bool _isLoading;

        public MoleculePageViewModel()
        {
/*
            _log = logManager.GetLogger<MoleculePageViewModel>();
            _navigationService = navigationService;
*/
            _engine = new MoleculeViewerEngine();

            //_goBackCommand = new DelegateCommand<object>(o => _navigationService.GoBack());
            _resetViewCommand = new DelegateCommand<object>(o => _engine.ResetCameraView());
        }

        public MoleculeViewerEngine Engine
        {
            get { return _engine; }
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public ICommand GoBackCommand
        {
            get { return _goBackCommand; }
        }

        public ICommand ResetViewCommand
        {
            get { return _resetViewCommand; }
        }

        public bool ShowHetAtoms
        {
            get { return _showHetAtoms; }
            set
            {
                if (SetProperty(ref _showHetAtoms, value))
                    if (_molecule != null)
                        _molecule.ShowHetAtoms = value;
            }
        }

        public bool ShowWaters
        {
            get { return _showWaters; }
            set
            {
                if (SetProperty(ref _showWaters, value))
                    if (_molecule != null)
                        _molecule.ShowWaters = value;
            }
        }

        public bool ShowBackbone
        {
            get { return _showBackbone; }
            set
            {
                if (SetProperty(ref _showBackbone, value))
                {
                    if (_molecule != null)
                        _molecule.ShowBackbone = value;

                    if (value)
                    {
                        ShowCartoon = false;
                        ShowFullChain = false;
                    }
                }
            }
        }

        public bool ShowFullChain
        {
            get { return _showFullChain; }
            set
            {
                if (SetProperty(ref _showFullChain, value))
                {    
                    if (_molecule != null)
                        _molecule.ShowFullChain = value;

                    if (value)
                    {
                        ShowCartoon = false;
                        ShowBackbone = false;
                    }
                }
            }
        }

        public bool ShowCartoon
        {
            get { return _showCartoon; }
            set
            {
                if (SetProperty(ref _showCartoon, value))
                {
                    if (_molecule != null)
                        _molecule.ShowCartoon = value;
                
                    if (value)
                    {
                        ShowFullChain = false;
                        ShowBackbone = false;
                    }
                }
            }
        }

        public bool ColorStructure
        {
            get { return _molecule != null && _molecule.ColorScheme == ColorScheme.Structure; }
            set { if (value) SetColorScheme(ColorScheme.Structure); }
        }

        public bool ColorAtom
        {
            get { return _molecule != null && _molecule.ColorScheme == ColorScheme.Atom; }
            set { if (value) SetColorScheme(ColorScheme.Atom); }
        }

        public bool ColorResidue
        {
            get { return _molecule != null && _molecule.ColorScheme == ColorScheme.Residue; }
            set { if (value) SetColorScheme(ColorScheme.Residue); }
        }

        public bool ColorChain
        {
            get { return _molecule != null && _molecule.ColorScheme == ColorScheme.Chain; }
            set { if (value) SetColorScheme(ColorScheme.Chain); }
        }

        public bool ColorTemperature
        {
            get { return _molecule != null && _molecule.ColorScheme == ColorScheme.Temperature; }
            set { if (value) SetColorScheme(ColorScheme.Temperature); }
        }

        private void SetColorScheme(ColorScheme scheme)
        {
            if (_molecule != null)
            {
                _molecule.ColorScheme = scheme;
                OnPropertyChanged("ColorStructure");
                OnPropertyChanged("ColorAtom");
                OnPropertyChanged("ColorResidue");
                OnPropertyChanged("ColorChain");
                OnPropertyChanged("ColorTemperature");
            }
        }

        public void OnNavigatedTo(object navigationParameter)
        {
            var @params = (MoleculePageParams) navigationParameter;
            if (@params != null && @params.File != null)
            {
                OpenFile(@params.File);
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            private set { SetProperty(ref _isLoading, value); }
        }

        private void OpenFile(StorageFile file)
        {
            IsLoading = true;
            _loadingTokenSource = new CancellationTokenSource();

            Title = "";
            _molecule = null;
            _engine.Show(null);

/*
            if (_log.IsInfoEnabled)
                _log.Info("Loading molecule {0}", file.Name);
*/

            Task.Factory.StartNew(async () =>
            {
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    
                    var molecule = new Molecule(stream);

                    Debug.WriteLine("Molecule {0} loaded in {1} s.", file.Name, sw.Elapsed.TotalSeconds);
                    
                    return molecule;
                }
            },_loadingTokenSource.Token)
            .Unwrap()
            .ContinueWith(t =>
            {
                _loadingTokenSource = null;
                IsLoading = false;

                if (t.IsFaulted || t.IsCanceled)
                {
                    //TODO; show error
/*                    _log.Error("Error loading molecule file", t.Exception);*/
                    return;
                }

                _molecule = t.Result;
                Title = file.DisplayName;

                _molecule.ShowBackbone = ShowBackbone;
                _molecule.ShowFullChain = ShowFullChain;
                _molecule.ShowCartoon = ShowCartoon;

                _molecule.ShowWaters = ShowWaters;
                _molecule.ShowHetAtoms = ShowHetAtoms;

                _engine.Show(_molecule);
            }, TaskScheduler.FromCurrentSynchronizationContext());


        }
    }

    public class MoleculePageParams
    {
        public StorageFile File { get; set; }
    }
}