using System.Collections.ObjectModel;
using System.Linq;
using GearGenerator.Helpers;

namespace GearGenerator.ViewModels
{
    public class MainViewModel : ViewModel
    {
        public ObservableCollection<GearViewModel> Gears { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand<GearViewModel> RemoveCommand { get; }
        
        public MainViewModel()
        {
            Gears = new ObservableCollection<GearViewModel>();
            AddCommand = new RelayCommand(Add);
            RemoveCommand = new RelayCommand<GearViewModel>(Remove);
            Add();
        }

        void Add()
        {

            var vm = new GearViewModel {
                Name = $"Gear # {Gears.Count + 1}"
            };
            var distance = Gears.Sum(x => x.OutsideRadius) + vm.OutsideRadius;
            vm.CenterX = distance;
            vm.CenterY = distance;
            Gears.Add( vm );
        }

        void Remove(GearViewModel gear)
        {
            Gears.Remove(gear);
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
 