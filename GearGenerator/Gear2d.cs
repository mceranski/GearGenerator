using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GearGenerator
{
    //Drawing a gear: http://www.cartertools.com/involute.html
    public class Gear2d
    {
        private readonly Gear _gear;
        private readonly Canvas _canvas;

        public Point CenterPoint { get; }
        private Path _pitchCircle;

        public Gear2d( Gear gear, Canvas canvas )
        {
            _gear = gear;
            _canvas = canvas;
            CenterPoint = new Point(OutsideRadius * 1.25, OutsideRadius * 1.25);
        }

        public double OutsideRadius => Mm2Pixel(_gear.OutsideRadius);
        public double PitchRadius => Mm2Pixel(_gear.PitchRadius);
        public double InsideRadius => Mm2Pixel(_gear.RootRadius);
        public double BaseRadius => Mm2Pixel(_gear.BaseRadius);

        private static double Mm2Pixel(double mm)
        {
            const double factor = 96 / 25.4;
            return mm * factor;
        }

        void DrawPieSlices()
        {
            //draw the lines that represent equal divisions of the circle based on the number of teeth;
            for (var i = 1; i <= _gear.Teeth; i++)
            {
                var offset = DegreesToRadians((360d / _gear.Teeth) * i);
                var x = CenterPoint.X + (BaseRadius * Math.Cos(offset));
                var y = CenterPoint.Y + (BaseRadius * Math.Sin(offset));
                _canvas.DrawLine(CenterPoint, new Point(x, y), Brushes.DimGray);
            }

        }

        public void Draw()
        {
            DrawPieSlices();
            _canvas.DrawCircle(CenterPoint, InsideRadius); //inner circle
            _pitchCircle = _canvas.DrawCircle(CenterPoint, PitchRadius, Brushes.DimGray, 2d, 4d); //pitch circle
            _canvas.DrawCircle(CenterPoint, BaseRadius, Brushes.Magenta, 2d, 4d); //base circle
            _canvas.DrawCircle(CenterPoint, OutsideRadius, Brushes.DimGray, 2d, 4d); //outside circle
            _canvas.DrawCircle(CenterPoint, OutsideRadius * .1); //center point
            _canvas.DrawLine(new Point(CenterPoint.X, CenterPoint.Y - OutsideRadius * 1.1), new Point(CenterPoint.X, CenterPoint.Y + OutsideRadius * 1.1), Brushes.Black, 5, 4, 15, 4); //vertical line
            _canvas.DrawLine(new Point(CenterPoint.X - OutsideRadius * 1.1, CenterPoint.Y), new Point(CenterPoint.X + OutsideRadius * 1.1, CenterPoint.Y), Brushes.Black, 5, 4, 15, 4); //horizontal line
            DrawTooth( 220 );
            //DrawTooth( 0 );
        }

        void DrawTooth( double startAngle )
        {
            var involutePts = new List<Point>();

            for (var i = 0; i < 14; i++)
            {
                var offset = DegreesToRadians(startAngle - (i * _gear.FCB));
                var x = CenterPoint.X + BaseRadius * Math.Cos(offset);
                var y = CenterPoint.X + BaseRadius * Math.Sin(offset);
                var point = new Point(x, y);
                _canvas.DrawLine(CenterPoint, point, Brushes.Red, .25);

                //draw a line tangent to the point
                //var tb = FindTangentPoints(point);

                var v = CenterPoint - point;
                var perpendicular = new Vector(v.Y, -v.X);
                //var tb = new Point(perpendicular.X, perpendicular.Y);
                var tangentPoint = new Point(point.X + perpendicular.X, point.Y + perpendicular.Y);
                _canvas.DrawLine(point, tangentPoint, Brushes.Purple, .25);
                

                var arcLength = (2 * Math.PI * BaseRadius) * ((i * _gear.FCB) / 360);
                var pt = CalculatePoint(point, tangentPoint, arcLength);
                involutePts.Add(pt);
            }

            var curve = _canvas.DrawCurve(involutePts.ToArray());
            //find where the involute point intersects with the pitch circle
            var intersection = GetIntersectionPoints(curve.Data, _pitchCircle.Data).FirstOrDefault();
            if (intersection.X == 0) return;

            _canvas.DrawCircle(intersection, 1, Brushes.Red);
            _canvas.DrawLine(CenterPoint, intersection, Brushes.Red, .5);
            var offset1 = DegreesToRadians(startAngle - (_gear.ToothSpacingDegrees * .25));
            var x1 = CenterPoint.X + PitchRadius * Math.Cos(offset1);
            var y1 = CenterPoint.X + PitchRadius * Math.Sin(offset1);
            var mirrorPoint = new Point(x1, y1);

            _canvas.DrawLine(CenterPoint, mirrorPoint, Brushes.Red, .5);


            var offset2 = DegreesToRadians(startAngle - (_gear.ToothSpacingDegrees * .5));
            var x2 = CenterPoint.X + PitchRadius * Math.Cos(offset2);
            var y2 = CenterPoint.X + PitchRadius * Math.Sin(offset2);
            var mirrorPoint2 = new Point(x2, y2);
            _canvas.DrawLine(CenterPoint, mirrorPoint2, Brushes.Red, .5);

            //next, need to figure out how to mirror points across mirror point
            //perhaps figure out the delta x, delta y of each point and draw them relative to the mirrorPoint2 startPoint?

            var mirrorPoints = new List<Point> { mirrorPoint2 };

            foreach (var involutePt in involutePts)
            {
                var deltaX = (intersection.X - involutePt.X) * -1;
                var deltaY = (intersection.Y - involutePt.Y);
                var m = new Point { X = mirrorPoint2.X + deltaX, Y = mirrorPoint2.Y + deltaY };
                mirrorPoints.Add(m);
            }

            //_canvas.DrawCurve(mirrorPoints.ToArray());
        }

        //http://jsfiddle.net/uq8fe/7/
        // This doesn't always work depending on the angle that is supplied
        public Point FindTangentPoints(Point point)
        {
            //distance
            var dd = Math.Sqrt(point.X * point.X + point.Y * point.Y);
            //alpha
            var a = Math.Asin(BaseRadius / dd);
            //beta
            var b = Math.Atan2(point.Y, point.X);
            //tangent angle
            //var t = b - a;
            //return new Point { X = BaseRadius * Math.Sin(t), Y = BaseRadius * -Math.Cos(t) };
            //tangent positive angle
            var t = b + a;
            return new Point { X = BaseRadius * -Math.Sin(t), Y = BaseRadius * Math.Cos(t) };
        }

        public static double DegreesToRadians(double degrees) => degrees * 0.0174533;

        public static Point CalculatePoint(Point a, Point b, double distance)
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
            return new Point((int)(a.X + vectorX), (int)(a.Y + vectorY));
        }

        public static Point[] GetIntersectionPoints(Geometry g1, Geometry g2)
        {
            Geometry og1 = g1.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));
            Geometry og2 = g2.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));
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
    }
}