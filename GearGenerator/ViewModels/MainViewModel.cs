using System.Collections.ObjectModel;

namespace GearGenerator.ViewModels
{
    public class MainViewModel : ViewModel
    {
        public ObservableCollection<GearViewModel> Gears { get; }
        
        public MainViewModel()
        {
            Gears = new ObservableCollection<GearViewModel> {new GearViewModel()};
        }

        private bool _showGrid = true;
        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                _showGrid = value;
                OnPropertyChanged();
            }
        }

        private bool _showGuidelines = true;
        public bool ShowGuidelines
        {
            get => _showGuidelines;
            set
            {
                _showGuidelines = value;
                OnPropertyChanged();
            }
        }

        private bool _useAnimation;
        public bool UseAnimation
        {
            get => _useAnimation;
            set
            {
                _useAnimation = value;
                OnPropertyChanged();
            }
        }
    }
}
 