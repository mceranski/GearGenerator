using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using GearGenerator.Helpers;

namespace GearGenerator.ViewModels
{
    public class MainViewModel : ViewModel
    {
        public ObservableCollection<GearViewModel> Gears { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand<GearViewModel> RemoveCommand { get; }
        public RelayCommand ResetZoomCommand { get; }
        public RelayCommand ExitCommand { get; }
        
        public MainViewModel()
        {
            Gears = new ObservableCollection<GearViewModel>();
            AddCommand = new RelayCommand(Add);
            RemoveCommand = new RelayCommand<GearViewModel>(Remove);
            ResetZoomCommand = new RelayCommand(() => ZoomValue = 1);
            ExitCommand = new RelayCommand(() => App.WindowManager.Shutdown());
            Add();
        }

        void Add()
        {
            var vm = new GearViewModel {
                Name = $"Gear # {Gears.Count + 1}"
            };

            var xPos = Gears.Any() 
                ? Gears.Last().CenterX + Gears.Last().PitchDiameter + 5
                : vm.OutsideRadius + 50;

            var yPos = Gears.Any() 
                ? Gears.Last().CenterY - 10 
                : vm.OutsideRadius + 50;

            vm.SweepDirection = Gears.Count >= 1 
                ? Gears[Gears.Count-1].SweepDirection == SweepDirection.Clockwise 
                ? SweepDirection.Counterclockwise : SweepDirection.Clockwise : SweepDirection.Clockwise;

            vm.CenterX = xPos;
            vm.CenterY = yPos;
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

        private double _zoomValue = 1d;
        public double ZoomValue
        {
            get => _zoomValue;
            set
            {
                _zoomValue = value;
                OnPropertyChanged();
            }
        }
    }
}
 