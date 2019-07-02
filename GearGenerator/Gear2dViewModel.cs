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
    public class Tooth
    {
        public Point[] MirrorPoints;
        public Point[] InvolutePoints;
        public Path Top;
        public Point BottomLeft;
        public Point BottomRight;
        public Path InvoluteCurve;
        public Path MirroredCurve;
    }

    //Drawing a gear: http://www.cartertools.com/involute.html
    //https://geargenerator.com/#200,200,100,6,1,0,146371.80000067202,4,1,8,2,4,27,-90,0,0,16,4,4,27,-60,1,1,12,1,12,20,-60,2,0,60,5,12,20,0,0,0,2,-399
    public class Gear2dViewModel : INotifyPropertyChanged
    {
        private List<Tooth> _teeth = new List<Tooth>();
        private readonly Gear _gear;
        private readonly Canvas _canvas;

        public Point CenterPoint { get; }
        private Path _outerCircle;
        private Path _pitchCircle;

        public Gear2dViewModel( Gear gear, Canvas canvas )
        {
            _gear = gear;
            _canvas = canvas;
            CenterPoint = new Point(OutsideRadius * 1.5, OutsideRadius * 1.5);

            Draw();
        }

        private void DrawPieSlices()
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
            
            _outerCircle = DrawCircle(CenterPoint, OutsideRadius, Brushes.DimGray, 2d, 4d); 
            DrawCircle(CenterPoint, RootRadius, Brushes.Blue, 2d, 4d);

            DrawCircle(CenterPoint, BaseRadius, Brushes.Orange, 2d, 4d); 
            _pitchCircle = DrawCircle(CenterPoint, PitchRadius, Brushes.Green, 2d, 4d);
            DrawCircle(CenterPoint, OutsideRadius * .25);

            //_canvas.DrawCircle(CenterPoint, OutsideRadius * 1.25); //bore point
            DrawLine(new Point(CenterPoint.X, CenterPoint.Y - OutsideRadius * 1.1), new Point(CenterPoint.X, CenterPoint.Y + OutsideRadius * 1.1), Brushes.Black, 5, 4, 15, 4); //vertical line
            DrawLine(new Point(CenterPoint.X - OutsideRadius * 1.1, CenterPoint.Y), new Point(CenterPoint.X + OutsideRadius * 1.1, CenterPoint.Y), Brushes.Black, 5, 4, 15, 4); //horizontal line

            var angle = 0d;
            Tooth lastTooth = null;
            while (angle <= 360d)
            {
                var tooth = DrawTooth(angle);
                if (lastTooth != null)
                    DrawLine(lastTooth.BottomLeft, tooth.BottomRight);
                angle += ToothSpacingDegrees;
                lastTooth = tooth;
            }
        }

        private Point GetPointOnCircle(double angle, double radius)
        {
            var radians = DegreesToRadians(angle);
            var x1 = CenterPoint.X + radius * Math.Cos(radians);
            var y1 = CenterPoint.X + radius * Math.Sin(radians);
            return new Point(x1, y1);
        }


        private Tooth DrawTooth( double startAngle )
        {
            var tooth = new Tooth();
            tooth.InvolutePoints = GetInvolutePoints(startAngle).ToArray();
            tooth.InvoluteCurve = DrawCurve(tooth.InvolutePoints);

            //find where the involute point intersects with the pitch circle
            var intersection = GetIntersectionPoints(tooth.InvoluteCurve, _pitchCircle).FirstOrDefault();
            DrawCircle(intersection, 1, Brushes.Red);
            //DrawLine(CenterPoint, intersection, Brushes.Red);

            var xDiff = intersection.X - CenterPoint.X;
            var yDiff = intersection.Y - CenterPoint.Y;
            var intersectAngle = Math.Atan2(yDiff, xDiff) * 180d / Math.PI;

            var delta = startAngle - intersectAngle;

            var offset1Degrees = intersectAngle - (ToothSpacingDegrees * .25);
            var mirrorPoint = GetPointOnCircle(offset1Degrees, PitchRadius);
            //DrawLine(CenterPoint, mirrorPoint, Brushes.Orange);

            var offset2Degrees = offset1Degrees - (ToothSpacingDegrees * .25);
            var mirrorPoint2 = GetPointOnCircle(offset2Degrees, PitchRadius);
            //DrawLine(CenterPoint, mirrorPoint2, Brushes.Blue);

            tooth.MirrorPoints = GetInvolutePoints(offset2Degrees - delta, true).ToArray();

            DrawCircle(mirrorPoint, 1, Brushes.Lime);
            tooth.MirroredCurve = DrawCurve(tooth.MirrorPoints);

            //Draw the top of the tooth, which is a line around the outside circle between the top of each curve
            var mirrorIntersects = GetIntersectionPoints(tooth.MirroredCurve, _outerCircle);
            var outerIntersects = GetIntersectionPoints(tooth.InvoluteCurve, _outerCircle);
            if (mirrorIntersects.Any() && outerIntersects.Any())
                tooth.Top = DrawLine(mirrorIntersects[0], outerIntersects[0]);

            TrimToOutsideRadius(tooth.MirroredCurve);
            TrimToOutsideRadius(tooth.InvoluteCurve);

            tooth.BottomRight = GetPointOnCircle(offset2Degrees - delta, RootRadius);
            DrawLine(tooth.MirrorPoints.First(), tooth.BottomRight, Brushes.Black);

            tooth.BottomLeft = GetPointOnCircle(intersectAngle + delta, RootRadius);
            DrawLine(tooth.InvolutePoints.First(), tooth.BottomLeft, Brushes.Black);

            return tooth;
        }

        private void TrimToOutsideRadius(Path path)
        {
            var data = path.Data.Clone();
            var g = data.GetFlattenedPathGeometry();
            for( var i = g.Figures.Count-1; i>=0; i--)
            {
                var figure = g.Figures[i].Clone();
                g.Figures.RemoveAt(i);
                g.Figures.Insert(i, figure);

                for (var s = figure.Segments.Count- 1; s >= 0; s--)
                {
                    var segment = figure.Segments[s];
                    if (!(segment is PolyLineSegment poly)) continue;

                    var s2 = poly.Clone();
                    figure.Segments.RemoveAt(s);

                    for (var y = s2.Points.Count - 1; y >= 0; y--)
                    {
                        var pt = s2.Points[y];
                        var distance = GetDistance(pt, CenterPoint);
                        if ( distance > OutsideRadius ) s2.Points.RemoveAt(y);
                    }

                    figure.Segments.Insert(s, s2);
                }

            }

            _canvas.Children.Remove(path);
            _canvas.Children.Add(new Path{ Data = g, StrokeThickness = 1, Stroke = Brushes.Black});
        }

        private IEnumerable<Point> GetInvolutePoints( double startAngle, bool reverse = false )
        {
            const int intervalCount = 20;

            for (var i = 0; i < intervalCount; i++)
            {
                var offsetDegrees = startAngle - (i * FCB) * ( reverse ? -1 : 1 );
                var point = GetPointOnCircle( offsetDegrees, BaseRadius );

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

        private static double GetDistance(Point p1, Point p2)
        {
            var deltaY = p1.Y - p2.Y;
            var deltaX = p1.X - p2.X;
            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }

        private static double DegreesToRadians(double degrees) => degrees * 0.01745329252;

        private static Point CalculatePoint(Point a, Point b, double distance)
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

        private static Point[] GetIntersectionPoints(Path g1, Path g2)
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

        public double DiametralPitch => _gear.DiametralPitch;

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
                Data = new EllipseGeometry {Center = center, RadiusX = radius, RadiusY = radius},
                ToolTip = $"X: {center.X}, Y: {center.Y}"
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
 