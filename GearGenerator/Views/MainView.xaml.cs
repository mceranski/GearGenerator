using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GearGenerator.Utils;
using GearGenerator.ViewModels;

namespace GearGenerator.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
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

        private void Redraw()
        {
            Canvas.Children.Clear();
            Draw();
            ResizeCanvas();
        }

        private void Draw()
        {
            Canvas.Children.Clear();
            DrawGear(_viewModel.Gear1);
        }

        private void DrawGear(GearViewModel gear)
        {
            var geometry = gear.GearGeometry;
            var path = new Path { Stroke = Brushes.Black, StrokeThickness = 1, Data = geometry };

            if (_viewModel.UseAnimation)
            {
                var animation = new DoubleAnimation(360, 0, new Duration(TimeSpan.FromSeconds(15))) { RepeatBehavior = RepeatBehavior.Forever };
                var rotateTransform = new RotateTransform { CenterX = gear.CenterX, CenterY = gear.CenterY };
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
                path.RenderTransform = rotateTransform;
            }

            var centerPoint = gear.CenterPoint;

            if (_viewModel.ShowGuidelines)
            {
                var pitchCircle = Canvas.DrawCircle(centerPoint, gear.PitchRadius, Brushes.LightGray, 4d, 2d);
                pitchCircle.ToolTip = $"Pitch : r{gear.PitchRadius}";

                var baseCircle = Canvas.DrawCircle(centerPoint, gear.BaseRadius, Brushes.LightGreen, 4d, 2d);
                baseCircle.ToolTip = $"Base : r{gear.BaseRadius}";

                var outerCircle = Canvas.DrawCircle(centerPoint, gear.OutsideRadius, Brushes.LightCoral, 4d, 2d);
                outerCircle.ToolTip = $"Outer: r{ gear.OutsideRadius}";

                var rootCircle = Canvas.DrawCircle(centerPoint, gear.RootRadius, Brushes.LightBlue, 4d, 2d);
                rootCircle.ToolTip = $"Root: r{ gear.RootRadius}";

                Canvas.DrawLine(new Point(centerPoint.X, centerPoint.Y - gear.OutsideRadius * 1.1), new Point(centerPoint.X, centerPoint.Y + gear.OutsideRadius * 1.1), Brushes.Black, 5, 4, 15, 4); //vertical line
                Canvas.DrawLine(new Point(centerPoint.X - gear.OutsideRadius * 1.1, centerPoint.Y), new Point(centerPoint.X + gear.OutsideRadius * 1.1, centerPoint.Y), Brushes.Black, 5, 4, 15, 4); //horizontal line
            }

            //foreach (var point in gear.Teeth.SelectMany(x => x.AllPoints))
            //{
            //    Canvas.DrawDot(point, Brushes.Red);
            //}

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
