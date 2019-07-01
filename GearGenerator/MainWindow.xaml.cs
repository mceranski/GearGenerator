using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using GearGenerator.Annotations;

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
            var gear = new Gear {Teeth = 8, PitchDiameter = 200, DiametralPitch = .04, PressureAngle = 27 };
            var vm = new Gear2dViewModel( gear, Canvas );
            this.DataContext = vm;
            //vm.Draw();
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
