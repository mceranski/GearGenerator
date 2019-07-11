using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GearGenerator.Models;

namespace GearGenerator.ViewModels
{
    public class GearViewModel : ViewModel
    {
        public string Name { get; set; }
        public Gear Model { get; }
        public List<Tooth> Teeth { get; } = new List<Tooth>();

        public GearViewModel()
        {
            Model = new Gear
            {
                PitchDiameter = 200,
                NumberOfTeeth = 8,
                PressureAngle = 27,
            };
        }

        public Point CenterPoint => new Point( CenterX, CenterY );
        public double PitchRadius => Model.PitchRadius;
        public double OutsideRadius => Model.OutsideRadius;
        public double RootRadius => Model.RootRadius;
        public double BaseRadius => Model.BaseRadius;
        public double FCB => Model.FCB;

        /// <summary>
        /// The X position of the center of the circle
        /// </summary>
        private double _centerX;
        public double CenterX
        {
            get => _centerX;
            set
            {
                _centerX = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CenterPoint));
            }
        }

        public SweepDirection SweepDirection { get; set; } = SweepDirection.Clockwise;
        public Point CenterTop => new Point( CenterX, CenterY - OutsideRadius );
        public Point CenterBottom => new Point(CenterX, CenterY + OutsideRadius);
        public Point CenterLeft => new Point(CenterX - OutsideRadius, CenterY);
        public Point CenterRight => new Point(CenterX + OutsideRadius, CenterY);

        /// <summary>
        /// The Y position of the center of the circle
        /// </summary>
        private double _centerY;
        public double CenterY
        {
            get => _centerY;
            set
            {
                _centerY = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CenterPoint));
            }
        }

        /// <summary>
        /// The number of teeth that the gear will have
        /// </summary>
        public int NumberOfTeeth
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

        /// <summary>
        /// Returns a GeometryGroup that can be used in a Path object to draw the gear
        /// </summary>
        public GeometryGroup GearGeometry
        {
            get
            {
                Teeth.Clear();
                var geoGroup = new GeometryGroup();
                var angle = 0d;
                while (angle < 360d)
                {
                    var toothGeometry = new StreamGeometry();
                    using (var gc = toothGeometry.Open())
                    {
                        var tooth = CreateTooth(angle);
                        Teeth.Add(tooth);
                        gc.BeginFigure(tooth.PrimaryPoints.First(), false, false);
                        gc.PolyLineTo(tooth.PrimaryPoints, true, true);
                        gc.LineTo(tooth.MirrorPoints.First(), true, true);
                        gc.PolyLineTo(tooth.MirrorPoints, true, true);
                        angle += Model.ToothSpacingDegrees;
                        geoGroup.Children.Add(toothGeometry);
                    }
                }

                //connect the bottoms of the teeth
                for (var i = 0; i < Teeth.Count; i++)
                {
                    var startPt = Teeth[i].PrimaryPoints.First();
                    var endPt = (i == Teeth.Count - 1 ? Teeth.First() : Teeth[i + 1]).MirrorPoints.Last();
                    var g = new StreamGeometry();
                    using (var gc = g.Open())
                    {
                        gc.BeginFigure(startPt, false, false);
                        gc.ArcTo(endPt, new Size(RootRadius, RootRadius), 0, false, SweepDirection.Clockwise, true, true);
                    }
                    geoGroup.Children.Add(g);
                }

                var boreGeo = new EllipseGeometry(CenterPoint, OutsideRadius * .25, OutsideRadius * .25);
                geoGroup.Children.Add(boreGeo);

                return geoGroup;
            }
        }

        /// <summary>
        /// Draw the tooth using involute curves and by finding the intersection points of the various radii
        /// </summary>
        /// <param name="startAngle">The angle that is used to start drawing the tooth</param>
        /// <returns>A tooth which is a collection of X,Y coordinates</returns>
        private Tooth CreateTooth(double startAngle)
        {
            var involutePoints = GetInvolutePoints(startAngle).ToList();

            //find where the involute point intersects with the pitch circle
            var pitchIntersect = FindIntersectionPoint(involutePoints, PitchRadius);
            involutePoints.Add(pitchIntersect);
            var outerIntersect = FindIntersectionPoint(involutePoints, OutsideRadius);
            involutePoints.Add(outerIntersect);

            var rootIntersect = GetPointOnCircle(startAngle, RootRadius);
            involutePoints.Add(rootIntersect);

            var xDiff = pitchIntersect.X - CenterPoint.X;
            var yDiff = pitchIntersect.Y - CenterPoint.Y;
            var intersectAngle = Math.Atan2(yDiff, xDiff) * 180d / Math.PI;
            var delta = startAngle - intersectAngle;

            var offset1Degrees = intersectAngle - (Model.ToothSpacingDegrees * .25);
            var offset2Degrees = offset1Degrees - (Model.ToothSpacingDegrees * .25);

            
            var mirrorPoints = GetInvolutePoints(offset2Degrees - delta, true).ToList();
            var mirrorOuterIntersect = FindIntersectionPoint(mirrorPoints, OutsideRadius);
            mirrorPoints.Add(mirrorOuterIntersect);
            var mirrorPitchIntersect = FindIntersectionPoint(mirrorPoints, PitchRadius);
            mirrorPoints.Add(mirrorPitchIntersect);
            var mirrorRootIntersect = GetPointOnCircle(offset2Degrees - delta, RootRadius);
            mirrorPoints.Add(mirrorRootIntersect);

            return new Tooth
            {
                PrimaryPoints = involutePoints.Where(IsPointApplicable).OrderBy(x => GetDistance(CenterPoint, x)).ToArray(),
                MirrorPoints = mirrorPoints.Where(IsPointApplicable).OrderByDescending(x => GetDistance(CenterPoint, x)).ToArray()
            };
        }

        /// <summary>
        /// Determine whether or not the point is within the outside radius and the root radius. A fudge factor of 1% is used. 
        /// </summary>
        /// <param name="p">An X,Y coordinate</param>
        /// <returns>True or false</returns>
        private bool IsPointApplicable(Point p)
        {
            var d = GetDistance(CenterPoint, p);
            var deltaOuter = Math.Abs(d - OutsideRadius);
            var deltaInner = Math.Abs(d - RootRadius);

            var outerOk = d <= OutsideRadius || deltaOuter <= (OutsideRadius * .01);
            var innerOk = d >= RootRadius || deltaInner <= (RootRadius * .01);

            return innerOk && outerOk;
        }

        /// <summary>
        /// Draw lines tangent to the circle, the using a specified length find a point to establish an involute curve
        /// </summary>
        /// <param name="startAngle">The angle drawn from the center of the circle</param>
        /// <param name="reverse">Set reverse to true to get the mirror of the involute curve</param>
        /// <returns>A list of X,Y coordinates that draw the involute curve</returns>
        private IEnumerable<Point> GetInvolutePoints(double startAngle, bool reverse = false)
        {
            const int intervalCount = 14;
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

        /// <summary>
        /// Given an angle, find where a line would intersect with the edge of the circle
        /// </summary>
        /// <param name="angle">The angle drawn from the center of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <returns></returns>
        private Point GetPointOnCircle(double angle, double radius)
        {
            var radians = DegreesToRadians(angle);
            var x1 = CenterPoint.X + radius * Math.Cos(radians);
            var y1 = CenterPoint.Y + radius * Math.Sin(radians);
            return new Point(x1, y1);
        }

        /// <summary>
        /// Given a bunch of points, get the first one that intersects with the edge of the circle
        /// </summary>
        /// <param name="points">A list of X,Y coordinates</param>
        /// <param name="radius">The radius of the circle</param>
        /// <returns>The intersection point</returns>
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

        /// <summary>
        /// Determines if a point is within a circle
        /// </summary>
        /// <param name="point">A X,Y coordinate</param>
        /// <param name="radius">The radius of the circle</param>
        /// <returns></returns>
        private bool IsInsideCircle(Point point, double radius)
        {
            return Math.Sqrt(Math.Pow((CenterPoint.X - point.X), 2d) +
                             Math.Pow((CenterPoint.Y - point.Y), 2d)) < radius;
        }

        /// <summary>
        /// Determine if a line intersections with the circle
        /// </summary>
        /// <param name="startPoint">The starting point of the line</param>
        /// <param name="endPoint">The ending point of the line</param>
        /// <param name="radius">The radius of a circle</param>
        /// <returns>True/false if the line intersects with the circle</returns>
        private bool IsIntersecting(Point startPoint, Point endPoint, double radius)
        {
            return IsInsideCircle(startPoint, radius) ^ IsInsideCircle(endPoint, radius);
        }

        /// <summary>
        /// Find where a line intersects along a circle with a given radius
        /// </summary>
        /// <param name="startPoint">The starting point of the line</param>
        /// <param name="endPoint">The ending point of the line</param>
        /// <param name="radius">The radius of a circle</param>
        /// <param name="intersection">The intersection point, it will return 0,0 if no intersection is found</param>
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

        /// <summary>
        /// Get the distance between two points
        /// </summary>
        /// <param name="p1">The starting point</param>
        /// <param name="p2">The ending point</param>
        /// <returns>The distance</returns>
        private static double GetDistance(Point p1, Point p2)
        {
            var deltaY = p2.Y - p1.Y;
            var deltaX = p2.X - p1.X;
            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }

        private static double DegreesToRadians(double degrees) => degrees * 0.01745329252;

        /// <summary>
        /// Given two points and a distance this method will return the X,Y location of the next point
        /// </summary>
        /// <param name="a">The starting point</param>
        /// <param name="b">The ending point</param>
        /// <param name="distance">Distance from ending point</param>
        /// <returns>The resultant X,Y point</returns>
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

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if( propertyName != nameof(GearGeometry))
                OnPropertyChanged(nameof(GearGeometry));
        }
    }
}