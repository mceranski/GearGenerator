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
    public class InvoluteCurve
    {
        public Point[] Points;
        public Point FurthestPoint => Points.Last();
        public Point ClosestPoint => Points.First();
    }

    public class Tooth
    {
        public InvoluteCurve Primary;
        public InvoluteCurve Mirror;
    }

    //Drawing a gear: http://www.cartertools.com/involute.html
    //https://geargenerator.com/#200,200,100,6,1,0,146371.80000067202,4,1,8,2,4,27,-90,0,0,16,4,4,27,-60,1,1,12,1,12,20,-60,2,0,60,5,12,20,0,0,0,2,-399
    public class Gear2dViewModel : INotifyPropertyChanged
    {
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
            
            _outerCircle = DrawCircle(CenterPoint, OutsideRadius, Brushes.Silver, 2d, 4d);
            _outerCircle.ToolTip = "Outer";

            var rootCircle = DrawCircle(CenterPoint, RootRadius, Brushes.Blue, 2d, 4d);
            rootCircle.ToolTip = "Root";

            var baseCircle = DrawCircle(CenterPoint, BaseRadius, Brushes.Orange, 2d, 4d);
            baseCircle.ToolTip = "Base";

            _pitchCircle = DrawCircle(CenterPoint, PitchRadius, Brushes.Green, 2d, 4d);
            _pitchCircle.ToolTip = "Pitch";
            DrawCircle(CenterPoint, OutsideRadius * .25);

            //_canvas.DrawCircle(CenterPoint, OutsideRadius * 1.25); //bore point
            DrawLine(new Point(CenterPoint.X, CenterPoint.Y - OutsideRadius * 1.1), new Point(CenterPoint.X, CenterPoint.Y + OutsideRadius * 1.1), Brushes.Black, 5, 4, 15, 4); //vertical line
            DrawLine(new Point(CenterPoint.X - OutsideRadius * 1.1, CenterPoint.Y), new Point(CenterPoint.X + OutsideRadius * 1.1, CenterPoint.Y), Brushes.Black, 5, 4, 15, 4); //horizontal line

            var angle = 0d;
            Tooth lastTooth = null;
            while (angle <= 360d)
            {
                var tooth = DrawTooth(angle);
                DrawCurve(tooth.Primary.Points);
                DrawCurve(tooth.Mirror.Points);
                DrawLine(tooth.Primary.FurthestPoint, tooth.Mirror.FurthestPoint);
                if (lastTooth != null)
                    DrawLine(lastTooth.Primary.ClosestPoint, tooth.Mirror.ClosestPoint);
                angle += ToothSpacingDegrees;
                lastTooth = tooth;
            }

            //DrawTooth(180);
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
            var involutePoints = GetInvolutePoints(startAngle).ToList();

            //find where the involute point intersects with the pitch circle
            var pitchIntersect = FindIntersectionPoint(involutePoints, PitchRadius);
            involutePoints.Add(pitchIntersect);
            var rootIntersect = FindIntersectionPoint(involutePoints, RootRadius);
            involutePoints.Add(rootIntersect);
            var outerIntersect = FindIntersectionPoint(involutePoints, OutsideRadius);
            involutePoints.Add(outerIntersect);

            var xDiff = pitchIntersect.X - CenterPoint.X;
            var yDiff = pitchIntersect.Y - CenterPoint.Y;
            var intersectAngle = Math.Atan2(yDiff, xDiff) * 180d / Math.PI;

            var delta = startAngle - intersectAngle;

            var offset1Degrees = intersectAngle - (ToothSpacingDegrees * .25);
            //var mirrorPoint = GetPointOnCircle(offset1Degrees, PitchRadius);
            //DrawLine(CenterPoint, mirrorPoint, Brushes.Orange);

            var offset2Degrees = offset1Degrees - (ToothSpacingDegrees * .25);
            //var mirrorPoint2 = GetPointOnCircle(offset2Degrees, PitchRadius);
            //DrawLine(CenterPoint, mirrorPoint2, Brushes.Blue);

            var mirrorPoints = GetInvolutePoints(offset2Degrees - delta, true).ToList();
            var mirrorOuterIntersect = FindIntersectionPoint(mirrorPoints, OutsideRadius);
            mirrorPoints.Add(mirrorOuterIntersect);
            var mirrorPitchIntersect = FindIntersectionPoint(mirrorPoints, PitchRadius);
            mirrorPoints.Add(mirrorPitchIntersect);
            var mirrorRootIntersect = FindIntersectionPoint(mirrorPoints, RootRadius);
            mirrorPoints.Add(mirrorRootIntersect);

            var tooth = new Tooth
            {
                Primary = new InvoluteCurve
                {
                    Points = involutePoints.Where(IsPointApplicable).OrderBy(x => GetDistance(CenterPoint, x)).ToArray()
                } ,
                Mirror = new InvoluteCurve
                {
                    Points = mirrorPoints.Where(IsPointApplicable).OrderBy(x => GetDistance(CenterPoint, x)).ToArray()
                }
            };

            return tooth;
        }

        private bool IsPointApplicable(Point p)
        {
            var d = GetDistance(CenterPoint, p);
            var threshold = OutsideRadius * .001;
            var deltaOuter = Math.Abs( d - OutsideRadius );
            var deltaInner = Math.Abs(d - RootRadius);

            var outerOk = d <= OutsideRadius || deltaOuter <= threshold;
            var innerOK = d >= RootRadius || deltaInner <= threshold;

            return innerOK && outerOk;
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

                //var distance = GetDistance(pt, CenterPoint);
                //if (distance > OutsideRadius) { DrawDot(pt, Brushes.Red);}

                yield return pt;
            }
        }

        private Point FindIntersectionPoint(IReadOnlyList<Point> points, double radius)
        {
            for (var i = 1; i < points.Count; i++)
            {
                var startPoint = points[i - 1];
                var endPoint = points[i];

                if (!IsIntersecting(startPoint, endPoint, radius)) continue;

                FindIntersect(startPoint, endPoint, radius, out var intersection);
                return intersection;
            }

            return CenterPoint;
        }

        private bool IsInsideCircle(Point point, double radius )
        {
            return Math.Sqrt(Math.Pow((CenterPoint.X - point.X), 2d) +
                             Math.Pow((CenterPoint.Y - point.Y), 2d)) < radius;
        }

        private bool IsIntersecting( Point startPoint, Point endPoint, double radius )
        {
            return IsInsideCircle(startPoint, radius) ^ IsInsideCircle(endPoint, radius);
        }

        private bool FindIntersect(Point startPoint, Point endPoint, double radius, out Point intersection )
        {
            if (IsIntersecting( startPoint, endPoint, radius))
            {
                //Calculate terms of the linear and quadratic equations
                var m1 = (endPoint.Y - startPoint.Y) / (endPoint.X - startPoint.X);
                var b1 = startPoint.Y - m1 * startPoint.X;
                var a = 1 + m1 * m1;
                var b = 2 * (m1 * b1 - m1 * CenterPoint.Y - CenterPoint.X);
                var c = CenterPoint.X * CenterPoint.X + b1 * b1 + CenterPoint.Y * CenterPoint.Y -
                        radius * radius - 2 * b1 * CenterPoint.Y;
                // solve quadratic equation
                var sqRtTerm = Math.Sqrt(b * b - 4 * a * c);
                var x = ((-b) + sqRtTerm) / (2 * a);
                // make sure we have the correct root for our line segment
                if ((x < Math.Min(startPoint.X, endPoint.X) ||
                     (x > Math.Max(startPoint.X, endPoint.X))))
                { x = ((-b) - sqRtTerm) / (2 * a); }
                //solve for the y-component
                var y = m1 * x + b1;
                // Intersection Calculated
                intersection = new Point(x, y);
                return true;
            }

            // Line segment does not intersect at one point.  It is either 
            // fully outside, fully inside, intersects at two points, is 
            // tangential to, or one or more points is exactly on the 
            // circle radius.
            intersection = new Point(0,0);
            return false;
        }

        private static double GetDistance(Point p1, Point p2)
        {
            var deltaY = p2.Y - p1.Y;
            var deltaX = p2.X - p1.X;
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

        private Path DrawDot(Point pt, Brush stroke = null)
        {
            return DrawCircle(pt, 1, stroke);
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
                    new PathFigure(enumerable[0], new PathSegment[] { new PolyLineSegment(enumerable, true),  }, false)))
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
 