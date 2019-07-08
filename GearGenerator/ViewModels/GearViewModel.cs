using System.Windows;

namespace GearGenerator.ViewModels
{
    public class GearViewModel : ViewModel
    {
        public Gear Model { get; }

        public GearViewModel()
        {
            Model = new Gear
            {
                PitchDiameter = 200,
                NumberOfTeeth = 8,
                PressureAngle = 27,
            };

            CenterX = Model.OutsideRadius * 1.25;
            CenterY = Model.OutsideRadius * 1.25;
        }

        public Point CenterPoint => new Point( CenterX, CenterY );
        public double PitchRadius => Model.PitchRadius;
        public double OutsideRadius => Model.OutsideRadius;
        public double RootRadius => Model.RootRadius;
        public double BaseRadius => Model.BaseRadius;

        private double _centerX;
        public double CenterX
        {
            get => _centerX;
            set
            {
                _centerX = value;
                OnPropertyChanged();
            }
        }

        private double _centerY;
        public double CenterY
        {
            get => _centerY;
            set
            {
                _centerY = value;
                OnPropertyChanged();
            }
        }

        public int Teeth
        {
            get => Model.NumberOfTeeth;
            set
            {
                Model.NumberOfTeeth = value;
                OnPropertyChanged();
            }
        }

        public double PitchDiameter
        {
            get => Model.PitchDiameter;
            set
            {
                Model.PitchDiameter = value;
                OnPropertyChanged();
            }
        }

        public double PressureAngle
        {
            get => Model.PressureAngle;
            set
            {
                Model.PressureAngle = value;
                OnPropertyChanged();
            }
        }
    }
}