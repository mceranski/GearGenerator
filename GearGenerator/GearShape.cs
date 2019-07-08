using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GearGenerator.Models;

namespace GearGenerator
{
    //Drawing a gear: http://www.cartertools.com/involute.html
    //Gear Generator Online: https://geargenerator.com
    public class GearControl 
    {
        private Gear _gear;
        public int Teeth { get; set; }
        public double PitchDiameter { get; set; }
        public double PressureAngle { get; set; }
        public Point CenterPoint { get; set; }

        public GeometryGroup PathGeometry
        {
            get
            {
                _gear = new Gear { PitchDiameter = PitchDiameter, PressureAngle = PressureAngle, NumberOfTeeth = Teeth };

                var geoGroup = new GeometryGroup();
                var teeth = new List<Tooth>();
                var angle = 0d;
                while (angle < 360d)
                {
                    var toothGeometry = new StreamGeometry();
                    using (var gc = toothGeometry.Open())
                    {
                        var tooth = CreateTooth(angle);
                        teeth.Add( tooth );
                        gc.BeginFigure(tooth.PrimaryPoints.First(), false, false);
                        gc.PolyLineTo(tooth.PrimaryPoints, true, true);
                        gc.LineTo(tooth.MirrorPoints.First(), true, true);
                        gc.PolyLineTo(tooth.MirrorPoints, true, true);
                        angle += _gear.ToothSpacingDegrees;
                        geoGroup.Children.Add(toothGeometry);
                    }
                }

                //connect the bottoms of the teeth
                for (var i = 0; i < teeth.Count; i++)
                {
                    var startPt = teeth[i].PrimaryPoints.First();
                    var endPt = ( i == teeth.Count-1 ? teeth.First() : teeth[i+1] ).MirrorPoints.Last();
                    var g = new StreamGeometry();
                    using (var gc = g.Open())
                    {
                        gc.BeginFigure(startPt, false, false);
                        gc.LineTo(endPt, true, false );
                    }
                    geoGroup.Children.Add( g );
                }

                var boreGeo = new EllipseGeometry(CenterPoint, _gear.OutsideRadius * .25, _gear.OutsideRadius * .25);
                geoGroup.Children.Add( boreGeo );

                return geoGroup;
            }
        }

        private Tooth CreateTooth( double startAngle )
        {
            var involutePoints = GetInvolutePoints(startAngle).ToList();

            //find where the involute point intersects with the pitch circle
            var pitchIntersect = FindIntersectionPoint(involutePoints, _gear.PitchRadius);
            involutePoints.Add(pitchIntersect);
            var rootIntersect = FindIntersectionPoint(involutePoints, _gear.RootRadius);
            involutePoints.Add(rootIntersect);
            var outerIntersect = FindIntersectionPoint(involutePoints, _gear.OutsideRadius);
            involutePoints.Add(outerIntersect);

            var xDiff = pitchIntersect.X - CenterPoint.X;
            var yDiff = pitchIntersect.Y - CenterPoint.Y;
            var intersectAngle = Math.Atan2(yDiff, xDiff) * 180d / Math.PI;

            var delta = startAngle - intersectAngle;

            var offset1Degrees = intersectAngle - (_gear.ToothSpacingDegrees * .25);
            //var mirrorPoint = GetPointOnCircle(offset1Degrees, PitchRadius);
            //DrawLine(CenterPoint, mirrorPoint, Brushes.Orange);

            var offset2Degrees = offset1Degrees - (_gear.ToothSpacingDegrees * .25);
            //var mirrorPoint2 = GetPointOnCircle(offset2Degrees, PitchRadius);
            //DrawLine(CenterPoint, mirrorPoint2, Brushes.Blue);

            var mirrorPoints = GetInvolutePoints(offset2Degrees - delta, true).ToList();
            var mirrorOuterIntersect = FindIntersectionPoint(mirrorPoints, _gear.OutsideRadius);
            mirrorPoints.Add(mirrorOuterIntersect);
            var mirrorPitchIntersect = FindIntersectionPoint(mirrorPoints, _gear.PitchRadius);
            mirrorPoints.Add(mirrorPitchIntersect);
            var mirrorRootIntersect = FindIntersectionPoint(mirrorPoints, _gear.RootRadius);
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
            var threshold = _gear.OutsideRadius * .001;
            var deltaOuter = Math.Abs(d - _gear.OutsideRadius);
            var deltaInner = Math.Abs(d - _gear.RootRadius);

            var outerOk = d <= _gear.OutsideRadius || deltaOuter <= threshold;
            var innerOK = d >= _gear.RootRadius || deltaInner <= threshold;

            return innerOK && outerOk;
        }

        private IEnumerable<Point> GetInvolutePoints(double startAngle, bool reverse = false)
        {
            const int intervalCount = 20;
            for (var i = 0; i < intervalCount; i++)
            {
                var offsetDegrees = startAngle - (i * _gear.FCB) * (reverse ? -1 : 1);
                var point = GetPointOnCircle(offsetDegrees, _gear.BaseRadius);

                //find tangents points
                var v = CenterPoint - point;
                var perpendicular = reverse ? new Vector(-v.Y, v.X) : new Vector(v.Y, -v.X);
                var tangentPoint = new Point(point.X + perpendicular.X, point.Y + perpendicular.Y);
                //DrawLine(point, tangentPoint, Brushes.Purple, .5);

                //using a arc length, find a point a given distance from the tangent point
                var arcLength = (2 * Math.PI * _gear.BaseRadius) * ((i * (_gear.FCB)) / 360d);
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

        private static PathGeometry ToPathGeometry(IEnumerable<Point> points)
        {
            var enumerable = points as Point[] ?? points.ToArray();
            return new PathGeometry
            {
                Figures = new PathFigureCollection(enumerable.Select(x =>
                    new PathFigure(enumerable[0], new PathSegment[] { new PolyLineSegment(enumerable, true), }, false)))
            };
        }
    }
}