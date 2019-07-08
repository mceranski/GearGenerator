using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GearGenerator.ViewModels;

namespace GearGenerator.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewModel;
        public MainView()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            _viewModel.PropertyChanged += (sender, args) => Redraw();
            _viewModel.GearChanged += (gear, args) => Redraw();
            Loaded += (s, a) => Redraw();
            Slider.ValueChanged += (s,e) => ResizeCanvas();
        }

        private void DrawGrid()
        {
            var brush = new DrawingBrush { TileMode = TileMode.Tile, Viewport = new Rect(0, 0, 10, 10), ViewportUnits = BrushMappingMode.Absolute };
            var pen = new Pen { Brush = Brushes.Gray, Thickness = .5 };
            var geo = new RectangleGeometry(new Rect(0, 0, 50, 50));
            brush.Drawing = new GeometryDrawing { Pen = pen, Geometry = geo };
            Canvas.Background = brush;
        }

        private void Redraw()
        {
            Canvas.Children.Clear();
            Draw();
            ResizeCanvas();
        }

        private void Draw()
        {
            Canvas.Children.Clear();

            if( _viewModel.ShowGrid )
                DrawGrid();

            var gearShape = new GearControl
            {
                CenterPoint = new Point(_viewModel.Gear1.CenterX, _viewModel.Gear1.CenterY),
                Teeth = _viewModel.Gear1.Teeth,
                PressureAngle = _viewModel.Gear1.PressureAngle,
                PitchDiameter = _viewModel.Gear1.PitchDiameter
            };

            var geometry = gearShape.PathGeometry;
            var path = new Path { Stroke = Brushes.Black, StrokeThickness = 1, Data = geometry };

            if (_viewModel.UseAnimation)
            {
                var animation = new DoubleAnimation(360, 0, new Duration(TimeSpan.FromSeconds(15))) {RepeatBehavior = RepeatBehavior.Forever};
                var rotateTransform = new RotateTransform {CenterX = _viewModel.Gear1.CenterX, CenterY = _viewModel.Gear1.CenterY};
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
                path.RenderTransform = rotateTransform;
            }

            Canvas.Children.Add(path);
        }

        private void ResizeCanvas()
        {
            var size = new Size{ Width = this.Width, Height = this.Height };

            foreach (var child in Canvas.Children)
            {
                if (!(child is Path element)) continue;
                if (element.Data.Bounds.Width > size.Width) size.Width = element.Data.Bounds.Width;
                if (element.Data.Bounds.Height > size.Height) size.Height = element.Data.Bounds.Height;
            }

            size.Width *= Slider.Value;
            size.Height *= Slider.Value;

            Canvas.Width = size.Width;
            Canvas.Height = size.Height;
        }

    }
}
