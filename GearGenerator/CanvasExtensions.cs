using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GearGenerator
{
    public static class CanvasExtensions
    {
        public static Path DrawCircle(this Canvas canvas, Point center, double radius, Brush stroke = null, params double[] dashArray)
        {
            var circle = new Path
            {
                Stroke = stroke ?? Brushes.Black,
                StrokeThickness = 1,
                StrokeDashArray = dashArray == null ? null : new DoubleCollection(dashArray),
                Data = new EllipseGeometry { Center = center, RadiusX = radius, RadiusY = radius }
            };

            canvas.Children.Add(circle);
            return circle;
        }

        public static Path DrawLine(this Canvas canvas, Point start, Point end, Brush stroke = null, params double[] dashArray)
        {
            return DrawLine(canvas, start, end, stroke, 1, dashArray);
        }

        public static Path DrawCurve(this Canvas canvas, Point[] points, Brush stroke = null, double thickness = 1)
        {
            var curve = new Path
            {
                Stroke = stroke ?? Brushes.Black,
                StrokeThickness = thickness,
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection(points.Select(x => new PathFigure(points[0], new PathSegment[] { new PolyBezierSegment(points, true) }, false)))
                }
            };

            canvas.Children.Add(curve);
            return curve;

        }

        public static Path DrawLine(this Canvas canvas, Point start, Point end, Brush stroke = null, double thickness = 1, double[] dashArray = null)
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
