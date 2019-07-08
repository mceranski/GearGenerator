using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GearGenerator.Models;

namespace GearGenerator.ViewModels
{
    //Drawing a gear: http://www.cartertools.com/involute.html
    //Gear Generator Online: https://geargenerator.com
    public class GearViewModel : ViewModel
    {
        public Gear Model { get; }

        public GearViewModel()
        {
            Model = new Gear
            {
                PitchDiameter = 200,
                NumberOfTeeth = 8,
                PressureAngle = 27,
            };

            CenterX = Model.OutsideRadius * 1.25;
            CenterY = Model.OutsideRadius * 1.25;
        }

        public Point CenterPoint => new Point( CenterX, CenterY );
        public double PitchRadius => Model.PitchRadius;
        public double OutsideRadius => Model.OutsideRadius;
        public double RootRadius => Model.RootRadius;
        public double BaseRadius => Model.BaseRadius;
        public double FCB => Model.FCB;

        private double _centerX;
        public double CenterX
        {
            get => _centerX;
            set
            {
                _centerX = value;
                OnPropertyChanged();
            }
        }

        private double _centerY;
        public double CenterY
        {
            get => _centerY;
            set
            {
                _centerY = value;
                OnPropertyChanged();
            }
        }

        public int Teeth
        {
            get => Model.NumberOfTeeth;
            set
            {
                Model.NumberOfTeeth = value;
                OnPropertyChanged();
            }
        }

        public double PitchDiameter
        {
            get => Model.PitchDiameter;
            set
            {
                Model.PitchDiameter = value;
                OnPropertyChanged();
            }
        }

        public double PressureAngle
        {
            get => Model.PressureAngle;
            set
            {
                Model.PressureAngle = value;
                OnPropertyChanged();
            }
        }

        public GeometryGroup GearGeometry
        {
            get
            {
                var geoGroup = new GeometryGroup();
                var teeth = new List<Tooth>();
                var angle = 0d;
                while (angle < 360d)
                {
                    var toothGeometry = new StreamGeometry();
                    using (var gc = toothGeometry.Open())
                    {
                        var tooth = CreateTooth(angle);
                        teeth.Add(tooth);
                        gc.BeginFigure(tooth.PrimaryPoints.First(), false, false);
                        gc.PolyLineTo(tooth.PrimaryPoints, true, true);
                        gc.LineTo(tooth.MirrorPoints.First(), true, true);
                        gc.PolyLineTo(tooth.MirrorPoints, true, true);
                        angle += Model.ToothSpacingDegrees;
                        geoGroup.Children.Add(toothGeometry);
                    }
                }

                //connect the bottoms of the teeth
                for (var i = 0; i < teeth.Count; i++)
                {
                    var startPt = teeth[i].PrimaryPoints.First();
                    var endPt = (i == teeth.Count - 1 ? teeth.First() : teeth[i + 1]).MirrorPoints.Last();
                    var g = new StreamGeometry();
                    using (var gc = g.Open())
                    {
                        gc.BeginFigure(startPt, false, false);
                        gc.LineTo(endPt, true, false);
                    }
                    geoGroup.Children.Add(g);
                }

                var boreGeo = new EllipseGeometry(CenterPoint, OutsideRadius * .25, OutsideRadius * .25);
                geoGroup.Children.Add(boreGeo);

                return geoGroup;
            }
        }

        private Tooth CreateTooth(double startAngle)
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

            var offset1Degrees = intersectAngle - (Model.ToothSpacingDegrees * .25);
            //var mirrorPoint = GetPointOnCircle(offset1Degrees, PitchRadius);
            //DrawLine(CenterPoint, mirrorPoint, Brushes.Orange);

            var offset2Degrees = offset1Degrees - (Model.ToothSpacingDegrees * .25);
            //var mirrorPoint2 = GetPointOnCircle(offset2Degrees, PitchRadius);
            //DrawLine(CenterPoint, mirrorPoint2, Brushes.Blue);

            var mirrorPoints = GetInvolutePoints(offset2Degrees - delta, true).ToList();
            var mirrorOuterIntersect = FindIntersectionPoint(mirrorPoints, OutsideRadius);
            mirrorPoints.Add(mirrorOuterIntersect);
            var mirrorPitchIntersect = FindIntersectionPoint(mirrorPoints, PitchRadius);
            mirrorPoints.Add(mirrorPitchIntersect);
            var mirrorRootIntersect = FindIntersectionPoint(mirrorPoints, RootRadius);
            mirrorPoints.Add(mirrorRootIntersect);

            return new Tooth
            {
                PrimaryPoints = involutePoints.Where(IsPointApplicable).OrderBy(x => GetDistance(CenterPoint, x)).ToArray(),
                MirrorPoints = mirrorPoints.Where(IsPointApplicable).OrderByDescending(x => GetDistance(CenterPoint, x)).ToArray()
            };
        }

        private bool IsPointApplicable(Point p)
        {
            var d = GetDistance(CenterPoint, p);
            var threshold = OutsideRadius * .001;
            var deltaOuter = Math.Abs(d - OutsideRadius);
            var deltaInner = Math.Abs(d - RootRadius);

            var outerOk = d <= OutsideRadius || deltaOuter <= threshold;
            var innerOK = d >= RootRadius || deltaInner <= threshold;

            return innerOK && outerOk;
        }

        private IEnumerable<Point> GetInvolutePoints(double startAngle, bool reverse = false)
        {
            const int intervalCount = 20;
            for (var i = 0; i < intervalCount; i++)
            {
                var offsetDegrees = startAngle - (i * FCB) * (reverse ? -1 : 1);
                var point = GetPointOnCircle(offsetDegrees, BaseRadius);

                //find tangents points
                var v = CenterPoint - point;
                var perpendicular = reverse ? new Vector(-v.Y, v.X) : new Vector(v.Y, -v.X);
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

        private Point GetPointOnCircle(double angle, double radius)
        {
            var radians = DegreesToRadians(angle);
            var x1 = CenterPoint.X + radius * Math.Cos(radians);
            var y1 = CenterPoint.X + radius * Math.Sin(radians);
            return new Point(x1, y1);
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

        private bool IsInsideCircle(Point point, double radius)
        {
            return Math.Sqrt(Math.Pow((CenterPoint.X - point.X), 2d) +
                             Math.Pow((CenterPoint.Y - point.Y), 2d)) < radius;
        }

        private bool IsIntersecting(Point startPoint, Point endPoint, double radius)
        {
            return IsInsideCircle(startPoint, radius) ^ IsInsideCircle(endPoint, radius);
        }

        private void FindIntersect(Point startPoint, Point endPoint, double radius, out Point intersection)
        {
            if (IsIntersecting(startPoint, endPoint, radius))
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
                return;
            }

            // Line segment does not intersect at one point.  It is either 
            // fully outside, fully inside, intersects at two points, is 
            // tangential to, or one or more points is exactly on the 
            // circle radius.
            intersection = new Point(0, 0);
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
    }
}