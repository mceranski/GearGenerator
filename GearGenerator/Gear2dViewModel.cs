using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GearGenerator.Annotations;

namespace GearGenerator
{
    //Drawing a gear: http://www.cartertools.com/involute.html
    public class Gear2dViewModel : INotifyPropertyChanged
    {
        private readonly Gear _gear;
        private readonly Canvas _canvas;

        public Point CenterPoint { get; }
        private Path _pitchCircle;

        public Gear2dViewModel( Gear gear, Canvas canvas )
        {
            _gear = gear;
            _canvas = canvas;
            CenterPoint = new Point(OutsideRadius * 1.5, OutsideRadius * 1.5);

            Draw();
        }

        void DrawPieSlices()
        {
            //draw the lines that represent equal divisions of the circle based on the number of teeth;
            for (var i = 1; i <= Teeth; i++)
            {
                var offset = DegreesToRadians(ToothSpacingDegrees * i);
                var x = CenterPoint.X + (BaseRadius * Math.Cos(offset));
                var y = CenterPoint.Y + (BaseRadius * Math.Sin(offset));
                DrawLine(CenterPoint, new Point(x, y), Brushes.DimGray);
            }
        }

        public void Draw()
        {
            _canvas.Children.Clear();
            //DrawPieSlices();
            
            DrawCircle(CenterPoint, OutsideRadius, Brushes.Red, 2d, 4d); 
            DrawCircle(CenterPoint, RootRadius, Brushes.Blue, 2d, 4d);

            DrawCircle(CenterPoint, BaseRadius, Brushes.Orange, 2d, 4d); 
            _pitchCircle = DrawCircle(CenterPoint, PitchRadius, Brushes.Green, 2d, 4d);
            DrawCircle(CenterPoint, OutsideRadius * .25);

            //_canvas.DrawCircle(CenterPoint, OutsideRadius * 1.25); //bore point
            DrawLine(new Point(CenterPoint.X, CenterPoint.Y - OutsideRadius * 1.1), new Point(CenterPoint.X, CenterPoint.Y + OutsideRadius * 1.1), Brushes.Black, 5, 4, 15, 4); //vertical line
            DrawLine(new Point(CenterPoint.X - OutsideRadius * 1.1, CenterPoint.Y), new Point(CenterPoint.X + OutsideRadius * 1.1, CenterPoint.Y), Brushes.Black, 5, 4, 15, 4); //horizontal line

            var angle = 0d;
            while (angle <= 360d)
            {
                DrawTooth(angle);
                angle += ToothSpacingDegrees;
            }
            //DrawTooth( 120 );
        }

        
        void DrawTooth( double startAngle )
        {
            var involutePts = GetInvolutePoints(startAngle);
            var curve = DrawCurve(involutePts);

            
            //find where the involute point intersects with the pitch circle
            var intersection = GetIntersectionPoints(curve, _pitchCircle).FirstOrDefault();
            DrawCircle(intersection, 1, Brushes.Red);
            DrawLine(CenterPoint, intersection, Brushes.Red);

            var xDiff = intersection.X - CenterPoint.X;
            var yDiff = intersection.Y - CenterPoint.Y;
            var intersectAngle = Math.Atan2(yDiff, xDiff) * 180d / Math.PI;

            var delta = startAngle - intersectAngle;

            var offset1Degrees = intersectAngle - (ToothSpacingDegrees * .25);
            var offset1 = DegreesToRadians(offset1Degrees);
            var x1 = CenterPoint.X + PitchRadius * Math.Cos(offset1);
            var y1 = CenterPoint.X + PitchRadius * Math.Sin(offset1);
            var mirrorPoint = new Point(x1, y1);
            DrawLine(CenterPoint, mirrorPoint, Brushes.Orange);

            var offset2Degrees = offset1Degrees - (ToothSpacingDegrees * .25);
            var offset2 = DegreesToRadians(offset2Degrees);
            var x2 = CenterPoint.X + PitchRadius * Math.Cos(offset2);
            var y2 = CenterPoint.X + PitchRadius * Math.Sin(offset2);
            var mirrorPoint2 = new Point(x2, y2);
            DrawLine(CenterPoint, mirrorPoint2, Brushes.Blue);


            var mirrorPts = GetInvolutePoints(offset2Degrees - delta, true).ToArray();
            DrawCircle(mirrorPoint2, 1, Brushes.Red);
            DrawCurve(mirrorPts);
        }

        IEnumerable<Point> GetInvolutePoints( double startAngle, bool reverse = false )
        {
            const int intervalCount = 20;
            for (var i = 0; i < intervalCount; i++)
            {
                var offsetDegrees = startAngle - (i * FCB) * ( reverse ? -1 : 1 );
                var offset = DegreesToRadians(offsetDegrees);
                var x = CenterPoint.X + BaseRadius * Math.Cos(offset);
                var y = CenterPoint.X + BaseRadius * Math.Sin(offset);
                var point = new Point(x, y);

                //find tangents points
                var v = CenterPoint - point;
                var perpendicular = reverse ? new Vector(-v.Y, v.X ) : new Vector(v.Y, -v.X);
                var tangentPoint = new Point(point.X + perpendicular.X, point.Y + perpendicular.Y);
                //DrawLine(point, tangentPoint, Brushes.Purple, .5);

                //using a arc length, find a point a given distance from the tangent point
                var arcLength = (2 * Math.PI * BaseRadius) * ((i * (FCB)) / 360d);
                var pt = CalculatePoint(point, tangentPoint, arcLength);

                
                yield return pt;
            }
        }

        static double DegreesToRadians(double degrees) => degrees * 0.01745329252;

        static Point CalculatePoint(Point a, Point b, double distance)
        {
            // a. calculate the vector from o to g:
            var vectorX = b.X - a.X;
            var vectorY = b.Y - a.Y;

            // b. calculate the proportion of hypotenuse
            var factor = distance / Math.Sqrt(vectorX * vectorX + vectorY * vectorY);

            // c. factor the lengths
            vectorX *= factor;
            vectorY *= factor;

            // d. calculate and Draw the new vector,
            return new Point(a.X + vectorX, a.Y + vectorY);
        }

        static Point[] GetIntersectionPoints(Path g1, Path g2)
        {
            Geometry og1 = g1.Data.GetWidenedPathGeometry(new Pen(Brushes.Black, g1.StrokeThickness));
            Geometry og2 = g2.Data.GetWidenedPathGeometry(new Pen(Brushes.Black, g2.StrokeThickness));
            var cg = new CombinedGeometry(GeometryCombineMode.Intersect, og1, og2);
            var pg = cg.GetFlattenedPathGeometry();
            var result = new Point[pg.Figures.Count];
            for (var i = 0; i < pg.Figures.Count; i++)
            {
                var fig = new PathGeometry(new[] { pg.Figures[i] }).Bounds;
                result[i] = new Point(fig.Left + fig.Width / 2.0, fig.Top + fig.Height / 2.0);
            }
            return result;
        }

        private double OutsideRadius => _gear.OutsideRadius;
        private double PitchRadius => _gear.PitchRadius;
        private double RootRadius => _gear.RootRadius;
        private double BaseRadius => _gear.BaseRadius;
        private double ToothSpacingDegrees => _gear.ToothSpacingDegrees;
        private double FCB => _gear.FCB;


        public int Teeth
        {
            get => _gear.Teeth;
            set
            {
                _gear.Teeth = value;
                OnPropertyChanged();
            }
        }

        public double PitchDiameter
        {
            get => _gear.PitchDiameter;
            set
            {
                _gear.PitchDiameter = value;
                OnPropertyChanged();
            }
        }

        public double DiametralPitch
        {
            get => _gear.DiametralPitch;
            set
            {
                _gear.DiametralPitch = value;
                OnPropertyChanged();
            }
        }

        public double PressureAngle
        {
            get => _gear.PressureAngle;
            set
            {
                _gear.PressureAngle = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Draw();
        }

        private Path DrawCircle(Point center, double radius, Brush stroke = null, params double[] dashArray)
        {
            var circle = new Path
            {
                Stroke = stroke ?? Brushes.Black,
                StrokeThickness = 1,
                StrokeDashArray = dashArray == null ? null : new DoubleCollection(dashArray),
                Data = new EllipseGeometry { Center = center, RadiusX = radius, RadiusY = radius }
            };

            _canvas.Children.Add(circle);
            return circle;
        }

        private Path DrawLine(Point start, Point end, Brush stroke = null, params double[] dashArray)
        {
            return DrawLine(start, end, stroke, 1d, dashArray);
        }

        public static PathGeometry ToPathGeometry(IEnumerable<Point> points)
        {
            var enumerable = points as Point[] ?? points.ToArray();
            return new PathGeometry
            {
                Figures = new PathFigureCollection(enumerable.Select(x =>
                    new PathFigure(enumerable[0], new PathSegment[] { new PolyBezierSegment(enumerable, true) }, false)))
            };
        }

        private Path DrawCurve(IEnumerable<Point> points, Brush stroke = null, double thickness = 1d)
        {
            var curve = new Path
            {
                Stroke = stroke ?? Brushes.Black,
                StrokeThickness = thickness,
                Data = ToPathGeometry(points)
            };

            _canvas.Children.Add(curve);
            return curve;

        }

        private Path DrawLine(Point start, Point end, Brush stroke = null, double thickness = 1d, double[] dashArray = null)
        {
            var line = new Path
            {
                Stroke = stroke ?? Brushes.Black,
                StrokeThickness = thickness,
                Data = new LineGeometry { StartPoint = start, EndPoint = end }
            };

            if (dashArray != null)
                line.StrokeDashArray = new DoubleCollection(dashArray);

            _canvas.Children.Add(line);
            return line;
        }
    }
}
 