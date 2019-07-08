using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GearGenerator
{
    public static class CanvasExtensions
    {
        public static Path DrawDot(this Canvas canvas, Point pt, Brush stroke = null)
        {
            return DrawCircle(canvas, pt, 1, stroke);
        }

        public static Path DrawCircle(this Canvas canvas, Point center, double radius, Brush stroke = null, params double[] dashArray)
        {
            var circle = new Path
            {
                Stroke = stroke ?? Brushes.Black,
                StrokeThickness = 1,
                StrokeDashArray = dashArray == null ? null : new DoubleCollection(dashArray),
                Data = new EllipseGeometry { Center = center, RadiusX = radius, RadiusY = radius },
                ToolTip = $"X: {center.X}, Y: {center.Y}"
            };

            canvas.Children.Add(circle);
            return circle;
        }

        public static Path DrawLine(this Canvas canvas, Point start, Point end, Brush stroke = null, params double[] dashArray)
        {
            return DrawLine(canvas, start, end, stroke, 1d, dashArray);
        }

        private static PathGeometry ToPathGeometry(IEnumerable<Point> points)
        {
            var enumerable = points as Point[] ?? points.ToArray();
            return new PathGeometry
            {
                Figures = new PathFigureCollection(enumerable.Select(x =>
                    new PathFigure(enumerable[0], new PathSegment[] { new PolyLineSegment(enumerable, true), }, false)))
            };
        }

        public static Path DrawCurve(this Canvas canvas, IEnumerable<Point> points, Brush stroke = null, double thickness = 1d)
        {
            var curve = new Path
            {
                Stroke = stroke ?? Brushes.Black,
                StrokeThickness = thickness,
                Data = ToPathGeometry(points)
            };
            canvas.Children.Add(curve);
            return curve;
        }

        public static Path DrawLine(this Canvas canvas, Point start, Point end, Brush stroke = null, double thickness = 1d, double[] dashArray = null)
        {
            var line = new Path
            {
                Stroke = stroke ?? Brushes.Black,
                StrokeThickness = thickness,
                Data = new LineGeometry { StartPoint = start, EndPoint = end }
            };

            if (dashArray != null)
                line.StrokeDashArray = new DoubleCollection(dashArray);

            canvas.Children.Add(line);
            return line;
        }
    }
}