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
                Teeth = 8,
                PressureAngle = 27,
            };

            CenterX = Model.OutsideRadius * 1.25;
            CenterY = Model.OutsideRadius * 1.25;
        }

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
            get => Model.Teeth;
            set
            {
                Model.Teeth = value;
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