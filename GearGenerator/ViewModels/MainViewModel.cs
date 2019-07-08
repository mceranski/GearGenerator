using System.ComponentModel;

namespace GearGenerator.ViewModels
{
    public class MainViewModel : ViewModel
    {
        public GearViewModel Gear1 { get; }
        public delegate void GearHandler(GearViewModel gear, PropertyChangedEventArgs args);
        public GearHandler GearChanged { get; set; }

        public MainViewModel()
        {
            Gear1 = new GearViewModel();
            Gear1.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)  {
                GearChanged?.Invoke(Gear1, args);
            };
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
 