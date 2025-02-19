﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GearGenerator.Models;

namespace GearGenerator.Controls
{
    [TemplatePart(Name = "PART_Gear", Type = typeof(Path))]
    [TemplatePart(Name = "PART_PitchCircle", Type = typeof(Path))]
    [TemplatePart(Name = "PART_RootCircle", Type = typeof(Path))]
    [TemplatePart(Name = "PART_BaseCircle", Type = typeof(Path))]
    [TemplatePart(Name = "PART_OutsideCircle", Type = typeof(Path))]
    [TemplatePart(Name = "PART_BoreCircle", Type = typeof(Path))]
    [TemplatePart(Name = "PART_PitchGeometry", Type = typeof(EllipseGeometry))]
    [TemplatePart(Name = "PART_RootGeometry", Type = typeof(EllipseGeometry))]
    [TemplatePart(Name = "PART_BaseGeometry", Type = typeof(EllipseGeometry))]
    [TemplatePart(Name = "PART_OutsideGeometry", Type = typeof(EllipseGeometry))]
    [TemplatePart(Name = "PART_BoreGeometry", Type = typeof(EllipseGeometry))]
    [TemplatePart(Name = "PART_HorizontalLineGeometry", Type = typeof(LineGeometry))]
    [TemplatePart(Name = "PART_VerticalLineGeometry", Type = typeof(LineGeometry))]
    [TemplatePart(Name = "PART_TextOverlay", Type = typeof(TextOnPathElement))]
    public class GearControl : UserControl
    {
        public static readonly DependencyProperty NumberOfTeethProperty;
        public static readonly DependencyProperty PitchDiameterProperty;
        public static readonly DependencyProperty PressureAngleProperty;
        public static readonly DependencyProperty FillProperty;
        public static readonly DependencyProperty StrokeProperty;
        public static readonly DependencyProperty StrokeThicknessProperty;
        public static readonly DependencyProperty CenterPointProperty;
        public static readonly DependencyProperty AngleProperty;
        public static readonly DependencyProperty SweepDirectionProperty;
        public static readonly DependencyProperty AutoStartProperty;
        public static readonly DependencyProperty ShowGuidelinesProperty;
        public static readonly DependencyProperty GuidelineColorProperty;
        public static readonly DependencyProperty ShowTextOverlayProperty;
        public static readonly DependencyProperty NumberProperty;
        public static readonly DependencyProperty RevolutionsPerMinuteProperty;

        private Storyboard _storyboard;
        private AnimationState _animationState = AnimationState.Uninitialized;
        private RotateTransform _rotateTransform;
        private DoubleAnimation _doubleAnimation;

        private Path _gearPath;
        private Path _crosshairsPath;
        private Path _pitchCircle;
        private Path _rootCircle;
        private Path _baseCircle;
        private Path _outsideCircle;
        private Path _boreCircle;

        private EllipseGeometry _pitchGeometry;
        private EllipseGeometry _outsideGeometry;
        private EllipseGeometry _rootGeometry;
        private EllipseGeometry _boreGeometry;
        private EllipseGeometry _baseGeometry;
        private LineGeometry _horizontalLineGeometry;
        private LineGeometry _verticalLineGeometry;
        private TextOnPathElement _textOverlay;

        static GearControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GearControl), new FrameworkPropertyMetadata(typeof(GearControl)));

            NumberOfTeethProperty = DependencyProperty.Register(nameof(NumberOfTeeth), typeof(int), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    8,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                    (o, args) => ((GearControl)o).NumberOfTeeth = (int)args.NewValue));

            PitchDiameterProperty = DependencyProperty.Register(nameof(PitchDiameter), typeof(double), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    200d,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                    (o, args) => ((GearControl)o).PitchDiameter = (double)args.NewValue));

            PressureAngleProperty = DependencyProperty.Register(nameof(PressureAngle), typeof(double), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    27d,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (o, args) => ((GearControl)o).PressureAngle = (double)args.NewValue));

            StrokeProperty = DependencyProperty.RegisterAttached(nameof(Stroke), typeof(Brush), typeof(GearControl), 
                new FrameworkPropertyMetadata(
                    Brushes.Black,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                    (o, args) => SetStroke(o, (Stroke)args.NewValue)));

            FillProperty = DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    Brushes.LightGray,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (o, args) => ((GearControl)o).Fill = (Brush)args.NewValue));

            StrokeThicknessProperty = DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(GearControl),
                new FrameworkPropertyMetadata(1d, (o,args) => ((GearControl)o).StrokeThickness = (double)args.NewValue));

            CenterPointProperty = DependencyProperty.Register(nameof(CenterPoint), typeof(Point), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    new Point(0,0),
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (o, args) => ((GearControl)o).CenterPoint = (Point)args.NewValue));

            AngleProperty = DependencyProperty.Register(nameof(Angle), typeof(double), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    0d,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                    (o, args) => ((GearControl)o).Angle = (double)args.NewValue));

            SweepDirectionProperty = DependencyProperty.Register(nameof(SweepDirection), typeof(SweepDirection), typeof(GearControl), new PropertyMetadata(SweepDirection.Clockwise));

            AutoStartProperty = DependencyProperty.Register(nameof(AutoStart), typeof(bool), typeof(GearControl), 
                new FrameworkPropertyMetadata(false, (o, args) =>  {
                    if ((bool) args.NewValue) ((GearControl)o).Start();
                }));

            ShowGuidelinesProperty = DependencyProperty.Register(nameof(ShowGuidelines), typeof(bool), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (o, args) => ((GearControl)o).ShowGuidelines = (bool)args.NewValue));

            GuidelineColorProperty = DependencyProperty.RegisterAttached(nameof(GuidelineColor), typeof(Brush), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    Brushes.DimGray,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (o, args) => ((GearControl)o).GuidelineColor = (Brush)args.NewValue));

            NumberProperty = DependencyProperty.RegisterAttached(nameof(Number), typeof(int), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (o, args) => ((GearControl)o).Number = (int)args.NewValue));

            ShowTextOverlayProperty = DependencyProperty.Register(nameof(ShowTextOverlay), typeof(bool), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (o, args) => ((GearControl)o).ShowTextOverlay = (bool)args.NewValue));

            RevolutionsPerMinuteProperty = DependencyProperty.RegisterAttached(nameof(RevolutionsPerMinute), typeof(double), typeof(GearControl),
                new FrameworkPropertyMetadata(
                    4d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (o, args) => ((GearControl)o).RevolutionsPerMinute = (double)args.NewValue));
        }

        private Point _dragStart;
        public GearControl()
        {
            Loaded += delegate
            {
                PreviewMouseLeftButtonDown += delegate (object sender, MouseButtonEventArgs args)
                {
                    var element = (UIElement)sender;
                    _dragStart = args.GetPosition(element);
                    //element.CaptureMouse();
                };

                PreviewMouseMove += delegate (object sender, MouseEventArgs args)
                {
                    if (args.LeftButton != MouseButtonState.Pressed) return;

                    var dragEnd = args.GetPosition(this);

                    //only drag when the user moved the mouse by a reasonable amount
                    if (!IsMovementBigEnough(_dragStart, dragEnd)) return;

                    Mouse.OverrideCursor = Cursors.Hand;

                    //the difference between the start of the drag and end of the drag
                    var delta = new Point(_dragStart.X - dragEnd.X, _dragStart.Y - dragEnd.Y);
                    var target = new Point(_dragStart.X - delta.X, _dragStart.Y - delta.Y);
                    CenterPoint = target;
                };
            };
        }

        public static bool IsMovementBigEnough(Point initialMousePosition, Point currentPosition)
        {
            return (Math.Abs(currentPosition.X - initialMousePosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPosition.Y - initialMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance);
        }

        private Gear GetGear()
        {
            return new Gear { NumberOfTeeth = NumberOfTeeth, PitchDiameter = PitchDiameter, PressureAngle = PressureAngle };
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var gear = GetGear();
            return new Size(CenterPoint.X + gear.OutsideRadius, CenterPoint.Y + gear.OutsideRadius);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _gearPath = GetTemplateChild("PART_Gear") as Path;
            _crosshairsPath = GetTemplateChild("PART_CrossHairs") as Path;
            _pitchCircle = GetTemplateChild("PART_PitchCircle") as Path;
            _rootCircle = GetTemplateChild("PART_RootCircle") as Path;
            _baseCircle = GetTemplateChild("PART_BaseCircle") as Path;
            _outsideCircle = GetTemplateChild("PART_OutsideCircle") as Path;
            _boreCircle = GetTemplateChild("PART_BoreCircle") as Path;

            _pitchGeometry = GetTemplateChild("PART_PitchGeometry") as EllipseGeometry;
            _rootGeometry = GetTemplateChild("PART_RootGeometry") as EllipseGeometry;
            _baseGeometry = GetTemplateChild("PART_BaseGeometry") as EllipseGeometry;
            _outsideGeometry = GetTemplateChild("PART_OutsideGeometry") as EllipseGeometry;
            _boreGeometry = GetTemplateChild("PART_BoreGeometry") as EllipseGeometry;

            _horizontalLineGeometry = GetTemplateChild("PART_HorizontalLineGeometry") as LineGeometry;
            _verticalLineGeometry = GetTemplateChild("PART_VerticalLineGeometry") as LineGeometry;

            _textOverlay = GetTemplateChild("PART_TextOverlay") as TextOnPathElement;

            CreateStoryboard();
            Draw();
        }

        private void CreateStoryboard()
        {
            if (Resources.Contains("Storyboard")) return;

            //1 RPM per second, we multiply this using the speed ratio
            var duration = TimeSpan.FromSeconds(60d);

            _storyboard = new Storyboard {
                Duration = duration,
                RepeatBehavior = RepeatBehavior.Forever,
                SpeedRatio = RevolutionsPerMinute
            };
            
            _doubleAnimation = SweepDirection == SweepDirection.Clockwise
                ? new DoubleAnimation(0, 360, duration)
                : new DoubleAnimation(360, 0, duration);

            _doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            _rotateTransform = new RotateTransform(Angle) {CenterX = CenterPoint.X, CenterY = CenterPoint.Y};

            _gearPath.SetValue(RenderTransformProperty, _rotateTransform);
            Storyboard.SetTarget(_doubleAnimation, _gearPath);
            Storyboard.SetTargetProperty(_doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            _storyboard.Children.Add(_doubleAnimation);

            Resources.Add("Storyboard", _storyboard);
            if (AutoStart) Start();
        }

        public int NumberOfTeeth
        {
            get => (int)GetValue(NumberOfTeethProperty);
            set => SetValueEx(NumberOfTeethProperty, value);
        }

        public double PressureAngle
        {
            get => (double)GetValue(PressureAngleProperty);
            set => SetValueEx(PressureAngleProperty, value);
        }

        public double PitchDiameter
        {
            get => (double)GetValue(PitchDiameterProperty);
            set => SetValueEx(PitchDiameterProperty, value);
        }

        public Point CenterPoint
        {
            get => (Point)GetValue(CenterPointProperty);
            set
            {
                SetValueEx(CenterPointProperty, value);

                if (_rotateTransform == null) return;
                _rotateTransform.CenterX = value.X;
                _rotateTransform.CenterY = value.Y;
            }
        }

        public double Angle
        {
            get => (double)GetValue(AngleProperty);
            set => SetValueEx(AngleProperty, value);
        }

        public SweepDirection SweepDirection
        {
            get => (SweepDirection)GetValue(SweepDirectionProperty);
            set => SetValueEx(SweepDirectionProperty, value);
        }

        public bool AutoStart
        {
            get => (bool)GetValue(AutoStartProperty);
            set => SetValue(AutoStartProperty, value);
        }

        public double RevolutionsPerMinute
        {
            get => (double)GetValue(RevolutionsPerMinuteProperty);
            set
            {
                SetValueEx(RevolutionsPerMinuteProperty, value);

                if (_storyboard == null) return;
                _storyboard.SpeedRatio = RevolutionsPerMinute;

                var state = _animationState;
                Stop(true);
                if (state == AnimationState.Running)
                    Start();
            }
        }

        public bool ShowGuidelines
        {
            get => (bool)GetValue(ShowGuidelinesProperty);
            set => SetValueEx(ShowGuidelinesProperty, value);
        }

        public Brush GuidelineColor
        {
            get => (Brush)GetValue(GuidelineColorProperty);
            set => SetValueEx(GuidelineColorProperty, value);
        }

        public bool ShowTextOverlay
        {
            get => (bool)GetValue(ShowTextOverlayProperty);
            set => SetValueEx(ShowTextOverlayProperty, value);
        }

        public static void SetStroke(DependencyObject target, Stroke value) => target.SetValue(StrokeProperty, value);
        public static Brush GetStroke(DependencyObject target) => (Brush)target.GetValue(StrokeProperty);

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public int Number
        {
            get => (int)GetValue(NumberProperty);
            set => SetValueEx(NumberProperty, value);
        }

        private Geometry GetGearGeometry(Gear gear)
        {
            var teeth = new List<Tooth>();

            var toothGeometry = new StreamGeometry();
            using (var gc = toothGeometry.Open())
            {
                var angle = 0d;
                while (angle < 360)
                {
                    var tooth = CreateTooth(gear, angle);

                    if (angle == 0)
                        gc.BeginFigure(tooth.MirrorPoints.First(), true, false);

                    //connect the tooth to the previous tooth
                    var lastTooth = teeth.LastOrDefault();
                    if (lastTooth != null)
                        gc.ArcTo(lastTooth.PrimaryPoints.Last(), new Size(gear.RootRadius, gear.RootRadius), 0, false, SweepDirection.Clockwise, true, true);

                    teeth.Add(tooth);
                    gc.PolyLineTo(tooth.MirrorPoints, true, true);
                    gc.LineTo(tooth.PrimaryPoints.First(), true, true);
                    gc.PolyLineTo(tooth.PrimaryPoints, true, true);
                    angle += gear.ToothSpacingDegrees;
                }

                //connect the last tooth
                gc.ArcTo(teeth.First().MirrorPoints.First(), new Size(gear.RootRadius, gear.RootRadius), 0, false, SweepDirection.Clockwise, true, true);
            }

            return toothGeometry;
        }

        public void Draw()
        {
            void UpdateGeo(EllipseGeometry geo, double radius)
            {
                geo.Center = CenterPoint;
                geo.RadiusX = radius;
                geo.RadiusY = radius;
            }

            Visibility BoolToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

            if (_gearPath == null) return;

            var gear = GetGear();

            var guidelineVisibility = BoolToVisibility(ShowGuidelines);
            _gearPath.ToolTip = gear.ToString();
            _gearPath.Data = GetGearGeometry(gear);

            UpdateGeo(_pitchGeometry, gear.PitchRadius);
            _pitchCircle.ToolTip = $"Pitch radius {gear.PitchRadius}";
            _pitchCircle.Visibility = guidelineVisibility;

            UpdateGeo(_rootGeometry, gear.RootRadius);
            _rootCircle.ToolTip = $"Root radius {gear.RootRadius}";
            _rootCircle.Visibility = guidelineVisibility;

            UpdateGeo(_baseGeometry, gear.BaseRadius);
            _baseCircle.ToolTip = $"Base radius {gear.BaseRadius}";
            _baseCircle.Visibility = guidelineVisibility;

            UpdateGeo(_outsideGeometry, gear.OutsideRadius);
            _outsideCircle.ToolTip = $"Outside radius {gear.OutsideRadius}";
            _outsideCircle.Visibility = guidelineVisibility;

            UpdateGeo(_boreGeometry, gear.BoreRadius);
            _boreCircle.ToolTip = $"Bore radius {gear.BoreRadius}";
            _boreCircle.Visibility = guidelineVisibility;

            _verticalLineGeometry.StartPoint = new Point(CenterPoint.X, CenterPoint.Y - gear.OutsideRadius);
            _verticalLineGeometry.EndPoint = new Point(CenterPoint.X, CenterPoint.Y + gear.OutsideRadius);
            _horizontalLineGeometry.StartPoint = new Point(CenterPoint.X - gear.OutsideRadius, CenterPoint.Y);
            _horizontalLineGeometry.EndPoint = new Point(CenterPoint.X + gear.OutsideRadius, CenterPoint.Y);
            _crosshairsPath.ToolTip = $"X: {CenterPoint.X} Y: {CenterPoint.Y}";
            _crosshairsPath.Visibility = guidelineVisibility;

            _textOverlay.Text = OverlayText;
            _textOverlay.Visibility = BoolToVisibility(ShowTextOverlay);
            var textRadius = gear.PitchRadius * .5;
            var figure = new PathFigure {StartPoint = new Point(CenterPoint.X - textRadius, CenterPoint.Y )};
            figure.Segments.Add( new ArcSegment{ IsLargeArc = false, Point = new Point( CenterPoint.X + textRadius, CenterPoint.Y), Size = new Size(textRadius, textRadius), SweepDirection = SweepDirection.Clockwise });
            _textOverlay.PathFigure = figure;
        }

        public string OverlayText => $"#{Number} N={NumberOfTeeth} P={PitchDiameter} PA={PressureAngle}";
        public string Title => $"Gear #{Number}";

        public void Start()
        {
            if (_animationState == AnimationState.Running) return;

            _storyboard?.Begin(this, true);
            _animationState = AnimationState.Running;
        }

        public void Stop( bool force = false )
        {
            if (_storyboard == null) return;

            _storyboard.Stop(this);
            _animationState = AnimationState.Stopped;
        }

        /// <summary>
        /// Draw the tooth using involute curves and by finding the intersection points of the various radii
        /// </summary>
        /// <param name="startAngle">The angle that is used to start drawing the tooth</param>
        /// <returns>A tooth which is a collection of X,Y coordinates</returns>
        public Tooth CreateTooth( Gear gear, double startAngle)
        {
            var involutePoints = GetInvolutePoints(gear,startAngle).ToList();

            //find where the involute point intersects with the pitch circle
            var pitchIntersect = FindIntersectionPoint(involutePoints, gear.PitchRadius);
            involutePoints.Add(pitchIntersect);
            var outerIntersect = FindIntersectionPoint(involutePoints, gear.OutsideRadius);
            involutePoints.Add(outerIntersect);

            var rootIntersect = GetPointOnCircle(startAngle, gear.RootRadius);
            involutePoints.Add(rootIntersect);

            var xDiff = pitchIntersect.X - CenterPoint.X;
            var yDiff = pitchIntersect.Y - CenterPoint.Y;
            var intersectAngle = Math.Atan2(yDiff, xDiff) * 180d / Math.PI;
            var delta = startAngle - intersectAngle;

            var offset1Degrees = intersectAngle - (gear.ToothSpacingDegrees * .25);
            var offset2Degrees = offset1Degrees - (gear.ToothSpacingDegrees * .25);

            var mirrorPoints = GetInvolutePoints(gear, offset2Degrees - delta, true).ToList();
            var mirrorOuterIntersect = FindIntersectionPoint(mirrorPoints, gear.OutsideRadius);
            mirrorPoints.Add(mirrorOuterIntersect);
            var mirrorPitchIntersect = FindIntersectionPoint(mirrorPoints, gear.PitchRadius);
            mirrorPoints.Add(mirrorPitchIntersect);
            var mirrorRootIntersect = GetPointOnCircle(offset2Degrees - delta, gear.RootRadius);
            mirrorPoints.Add(mirrorRootIntersect);

            return new Tooth
            {
                PrimaryPoints = involutePoints.Where(x => IsPointApplicable(gear,x)).OrderByDescending(x => GetDistance(CenterPoint, x)).ToArray(),
                MirrorPoints = mirrorPoints.Where(x => IsPointApplicable(gear,x)).OrderBy(x => GetDistance(CenterPoint, x)).ToArray()
            };
        }

        /// <summary>
        /// Determine whether or not the point is within the outside radius and the root radius. A fudge factor of 1% is used. 
        /// </summary>
        /// <param name="p">An X,Y coordinate</param>
        /// <param name="gear">The gear</param>
        /// <returns>True or false</returns>
        private bool IsPointApplicable(Gear gear, Point p )
        {
            var d = GetDistance(CenterPoint, p);
            var deltaOuter = Math.Abs(d - gear.OutsideRadius);
            var deltaInner = Math.Abs(d - gear.RootRadius);

            var outerOk = d <= gear.OutsideRadius || deltaOuter <= (gear.OutsideRadius * .01);
            var innerOk = d >= gear.RootRadius || deltaInner <= (gear.RootRadius * .01);

            return innerOk && outerOk;
        }

        /// <summary>
        /// Draw lines tangent to the circle, the using a specified length find a point to establish an involute curve
        /// </summary>
        /// <param name="startAngle">The angle drawn from the center of the circle</param>
        /// <param name="reverse">Set reverse to true to get the mirror of the involute curve</param>
        /// <returns>A list of X,Y coordinates that draw the involute curve</returns>
        private IEnumerable<Point> GetInvolutePoints(Gear gear, double startAngle, bool reverse = false)
        {
            const int intervalCount = 14;
            for (var i = 0; i < intervalCount; i++)
            {
                var offsetDegrees = startAngle - (i * gear.FCB) * (reverse ? -1 : 1);
                var point = GetPointOnCircle(offsetDegrees, gear.BaseRadius);

                //find tangents points
                var v = CenterPoint - point;
                var perpendicular = reverse ? new Vector(-v.Y, v.X) : new Vector(v.Y, -v.X);
                var tangentPoint = new Point(point.X + perpendicular.X, point.Y + perpendicular.Y);

                //using a arc length, find a point a given distance from the tangent point
                var arcLength = (2 * Math.PI * gear.BaseRadius) * ((i * (gear.FCB)) / 360d);
                var pt = CalculatePoint(point, tangentPoint, arcLength);

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

        private void SetValueEx(DependencyProperty property, object value)
        {
            SetValue(property, value);
            Draw();
        }

    }
}
