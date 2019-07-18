using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;

namespace GearGenerator.Controls
{
    public class DrawingCanvas : Canvas
    {
        public static readonly DependencyProperty ShowGridProperty;
        private Image _backgroundImage;

        static DrawingCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DrawingCanvas), new FrameworkPropertyMetadata(typeof(DrawingCanvas)));

            ShowGridProperty = DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(DrawingCanvas),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                    (o, args) => ((DrawingCanvas)o).ShowGrid = (bool)args.NewValue
                ));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            DrawGrid(ShowGrid);
        }

        public bool ShowGrid
        {
            get => (bool)GetValue(ShowGridProperty);
            set
            {
                SetValue(ShowGridProperty, value);
                DrawGrid(value);
            }
        }

        void DrawGrid(bool state)
        {
            if (_backgroundImage != null)
            {
                Children.Remove(_backgroundImage);
                _backgroundImage = null;
            }

            if (state == true) { 
                _backgroundImage = DrawGraphImage(10, 10);
                Children.Add(_backgroundImage);
            }
        }

        private Image DrawGraphImage(int xOffset, int yOffset )
        {
            var width = (int)(SystemParameters.PrimaryScreenWidth);
            var height = (int)(SystemParameters.PrimaryScreenHeight);

            //Draw the grid         
            var gridLinesVisual = new DrawingVisual();
            var drawingContext = gridLinesVisual.RenderOpen();
            var lightPen = new Pen(Brushes.LightGray, .5);
            var darkPen = new Pen(Brushes.DimGray, .75);
            lightPen.Freeze(); darkPen.Freeze();

            int x1 = yOffset,
                y1 = xOffset,
                rows = height,
                columns = width,
                j = 0;

            //Draw the horizontal lines         
            var x = new Point(0, 0.5);
            var y = new Point(width, 0.5);

            for (var i = 0; i <= rows; i++, j++)
            {
                drawingContext.DrawLine(i % 10 != 0 ? lightPen : darkPen, x, y);
                x.Offset(0, x1);
                y.Offset(0, x1);
            }
            j = 0;
            //Draw the vertical lines         
            x = new Point(0.5, 0);
            y = new Point(0.5, height);

            for (var i = 0; i <= columns; i++, j++)
            {
                drawingContext.DrawLine(j % 10 != 0 ? lightPen : darkPen, x, y);
                x.Offset(y1, 0);
                y.Offset(y1, 0);
            }

            drawingContext.Close();

            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(gridLinesVisual);
            bmp.Freeze();
            return new Image {Source = bmp, Opacity = .75};
        }
        
        protected override Size MeasureOverride(Size availableSize)
        {
            var childHeight = 0.0;
            var childWidth = 0.0;
            var size = new Size(0, 0);

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(availableSize.Width, availableSize.Height));
                if (child == _backgroundImage) continue;
                if (child.DesiredSize.Width > childWidth)
                {
                    childWidth = child.DesiredSize.Width;   //We will be stacking vertically.
                }
                childHeight += child.DesiredSize.Height;    //Total height needs to be summed up.
            }

            size.Width = double.IsPositiveInfinity(availableSize.Width) ? childWidth : availableSize.Width;
            size.Height = double.IsPositiveInfinity(availableSize.Height) ? childHeight : availableSize.Height;

            if (!(RenderTransform is ScaleTransform transform)) return size;
            size.Width *= transform.ScaleX;
            size.Height *= transform.ScaleY;

            return size;
        }
    }
}
