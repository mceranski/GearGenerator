using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GearGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Test();

            Loaded += (s, a) => ResizeCanvas();
            Slider.ValueChanged += (s,e) => ResizeCanvas();
        }

        void DrawGrid()
        {
            var brush = new DrawingBrush { TileMode = TileMode.Tile, Viewport = new Rect(0, 0, 10, 10), ViewportUnits = BrushMappingMode.Absolute };
            var pen = new Pen { Brush = Brushes.Gray, Thickness = .5 };
            var geo = new RectangleGeometry(new Rect(0, 0, 50, 50));
            brush.Drawing = new GeometryDrawing { Pen = pen, Geometry = geo };
            Canvas.Background = brush;
        }

        void Test()
        {
            DrawGrid();
            var gear = new Gear {Teeth = 20, DiametralPitch = .16, PressureAngle = 14.5 };
            var gear2d = new Gear2d(gear, Canvas);
            gear2d.Draw();
        }

        void ResizeCanvas()
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
