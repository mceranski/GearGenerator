using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace GearGenerator.Controls
{
    // Based on the control by Charles Petzold:
    // http://msdn.microsoft.com/en-us/magazine/dd263097.aspx
    public class TextOnPathElement : FrameworkElement
    {
        #region private fields

        private readonly List<FormattedText> _formattedChars;
        private readonly VisualCollection _visualChildren;

        private Rect _boundingRect;
        private Typeface _typeface;
        private double _pathLength;
        private double _textLength;

        #endregion

        #region dependency properties

        #region FontFamily

        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(TextOnPathElement), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, OnFontFamilyChanged));

        private static void OnFontFamilyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (TextOnPathElement)obj;
            element.OnFontPropertyChanged();
        }

        #endregion

        #region FontStyle

        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(TextOnPathElement), new FrameworkPropertyMetadata(OnFontStyleChanged));

        private static void OnFontStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (TextOnPathElement)obj;
            element.OnFontPropertyChanged();
        }

        #endregion

        #region FontWeight

        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(TextOnPathElement), new FrameworkPropertyMetadata(OnFontWeightChanged));

        private static void OnFontWeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (TextOnPathElement)obj;
            element.OnFontPropertyChanged();
        }

        #endregion

        #region FontStretch

        public FontStretch FontStretch
        {
            get => (FontStretch)GetValue(FontStretchProperty);
            set => SetValue(FontStretchProperty, value);
        }

        public static readonly DependencyProperty FontStretchProperty =
            DependencyProperty.Register("FontStretch", typeof(FontStretch), typeof(TextOnPathElement), new FrameworkPropertyMetadata(OnFontStretchChanged));

        private static void OnFontStretchChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (TextOnPathElement)obj;
            element.OnFontPropertyChanged();
        }

        #endregion

        #region FontSize

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(TextOnPathElement), new FrameworkPropertyMetadata(OnFontSizeChanged));

        private static void OnFontSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (TextOnPathElement)obj;
            element.OnFontPropertyChanged();
        }

        #endregion

        #region Foreground

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(TextOnPathElement), new FrameworkPropertyMetadata(OnForegroundChanged));

        private static void OnForegroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (TextOnPathElement)obj;
            element.OnTextPropertyChanged();
        }

        #endregion

        #region Text

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextOnPathElement), new FrameworkPropertyMetadata(OnTextChanged));

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (TextOnPathElement)obj;
            element.OnTextPropertyChanged();
        }

        #endregion

        #region PathFigure

        public PathFigure PathFigure
        {
            get => (PathFigure)GetValue(PathFigureProperty);
            set => SetValue(PathFigureProperty, value);
        }

        public static readonly DependencyProperty PathFigureProperty =
            DependencyProperty.Register("PathFigure", typeof(PathFigure), typeof(TextOnPathElement), new FrameworkPropertyMetadata(OnPathFigureChanged));

        private static void OnPathFigureChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (TextOnPathElement)obj;
            element._pathLength = element.GetPathFigureLength(element.PathFigure);
            element.TransformVisualChildren();
        }

        #endregion

        #region FontSize

        public HorizontalAlignment ContentAlignment
        {
            get => (HorizontalAlignment)GetValue(ContentAlignmentProperty);
            set => SetValue(ContentAlignmentProperty, value);
        }

        public static readonly DependencyProperty ContentAlignmentProperty =
            DependencyProperty.Register("ContentAlignment", typeof(HorizontalAlignment), typeof(TextOnPathElement), new FrameworkPropertyMetadata(OnContentAlignmentChanged));

        private static void OnContentAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (TextOnPathElement)obj;
            element.OnTextPropertyChanged();
        }

        #endregion

        #endregion

        #region constructor

        public TextOnPathElement()
        {

            _formattedChars = new List<FormattedText>();
            _boundingRect = new Rect();
            _typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            _visualChildren = new VisualCollection(this);
        }

        #endregion

        #region overrides

        protected override int VisualChildrenCount => _visualChildren.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _visualChildren.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _visualChildren[index];
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return (Size)_boundingRect.BottomRight;
        }

        #endregion

        #region private methods

        private void OnFontPropertyChanged()
        {
            _typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            OnTextPropertyChanged();
        }

        private void OnTextPropertyChanged()
        {
            _formattedChars.Clear();
            _textLength = 0;

            if (string.IsNullOrEmpty(Text)) return;
            foreach (var ch in Text)
            {
                var fontSize = ContentAlignment == HorizontalAlignment.Stretch ? 100 : FontSize;

                var formattedText =
                    new FormattedText(ch.ToString(), CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, _typeface, fontSize, Foreground);

                _formattedChars.Add(formattedText);
                _textLength += formattedText.WidthIncludingTrailingWhitespace;
            }

            GenerateVisualChildren();
        }

        private double GetPathFigureLength(PathFigure pathFigure)
        {
            if (pathFigure == null)
                return 0;

            var isAlreadyFlattened = true;

            foreach (var pathSegment in pathFigure.Segments)
            {
                if (pathSegment is PolyLineSegment || pathSegment is LineSegment) continue;
                isAlreadyFlattened = false;
                break;
            }

            var pathFigureFlattened = isAlreadyFlattened ? pathFigure : pathFigure.GetFlattenedPathFigure();
            double length = 0;
            var pt1 = pathFigureFlattened.StartPoint;

            foreach (var pathSegment in pathFigureFlattened.Segments)
            {
                if (pathSegment is LineSegment segment)
                {
                    var pt2 = segment.Point;
                    length += (pt2 - pt1).Length;
                    pt1 = pt2;
                }
                else if (pathSegment is PolyLineSegment lineSegment)
                {
                    var pointCollection = lineSegment.Points;
                    foreach (var pt2 in pointCollection)
                    {
                        length += (pt2 - pt1).Length;
                        pt1 = pt2;
                    }
                }
            }
            return length;
        }

        private void GenerateVisualChildren()
        {
            _visualChildren.Clear();

            foreach (var formText in _formattedChars)
            {
                var drawingVisual = new DrawingVisual();

                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform());
                transformGroup.Children.Add(new RotateTransform());
                transformGroup.Children.Add(new TranslateTransform());
                drawingVisual.Transform = transformGroup;

                var dc = drawingVisual.RenderOpen();
                dc.DrawText(formText, new Point(0, 0));
                dc.Close();

                _visualChildren.Add(drawingVisual);
            }

            TransformVisualChildren();
        }

        private void TransformVisualChildren()
        {
            _boundingRect = new Rect();

            if (_pathLength == 0 || _textLength == 0)
                return;

            if (_formattedChars.Count != _visualChildren.Count)
                return;

            var scalingFactor = ContentAlignment == HorizontalAlignment.Stretch ? _pathLength / _textLength : 1;
            double progress = 0;

            switch (ContentAlignment)
            {
                case HorizontalAlignment.Left:
                case HorizontalAlignment.Stretch:
                    progress = 0;
                    break;
                case HorizontalAlignment.Center:
                    progress = Math.Abs(_pathLength - _textLength) / 2 / _pathLength;
                    break;
                case HorizontalAlignment.Right:
                    progress = Math.Abs(_pathLength - _textLength) / _pathLength;
                    break;
            }

            var pathGeometry = new PathGeometry(new[] { PathFigure });
            _boundingRect = new Rect();

            for (var index = 0; index < _visualChildren.Count; index++)
            {
                var formText = _formattedChars[index];

                var width = scalingFactor * formText.WidthIncludingTrailingWhitespace;
                var baseline = scalingFactor * formText.Baseline;

                progress += width / 2 / _pathLength;

                pathGeometry.GetPointAtFractionLength(progress, out var point, out var tangent);

                var drawingVisual = _visualChildren[index] as DrawingVisual;
                var transformGroup = drawingVisual.Transform as TransformGroup;
                var scaleTransform = transformGroup.Children[0] as ScaleTransform;
                var rotateTransform = transformGroup.Children[1] as RotateTransform;
                var translateTransform = transformGroup.Children[2] as TranslateTransform;

                scaleTransform.ScaleX = scalingFactor;
                scaleTransform.ScaleY = scalingFactor;
                rotateTransform.Angle = Math.Atan2(tangent.Y, tangent.X) * 180 / Math.PI;
                rotateTransform.CenterX = width / 2;
                rotateTransform.CenterY = baseline;
                translateTransform.X = point.X - width / 2;
                translateTransform.Y = point.Y - baseline;

                var rect = drawingVisual.ContentBounds;
                rect.Transform(transformGroup.Value);
                _boundingRect.Union(rect);

                progress += width / 2 / _pathLength;
            }

            InvalidateMeasure();
        }

        #endregion
    }
}
