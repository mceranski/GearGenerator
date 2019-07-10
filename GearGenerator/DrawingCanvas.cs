using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GearGenerator
{
    public class DrawingCanvas : Canvas
    {
        static DrawingCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DrawingCanvas), new FrameworkPropertyMetadata(typeof(DrawingCanvas)));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var childHeight = 0.0;
            var childWidth = 0.0;
            var size = new Size(0, 0);

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(availableSize.Width, availableSize.Height));
                if (child.DesiredSize.Width > childWidth)
                {
                    childWidth = child.DesiredSize.Width;   //We will be stacking vertically.
                }
                childHeight += child.DesiredSize.Height;    //Total height needs to be summed up.
            }

            size.Width = double.IsPositiveInfinity(availableSize.Width) ? childWidth : availableSize.Width;
            size.Height = double.IsPositiveInfinity(availableSize.Height) ? childHeight : availableSize.Height;


            if (!(this.RenderTransform is ScaleTransform transform)) return size;
            size.Width *= transform.ScaleX;
            size.Height *= transform.ScaleY;

            return size;
        }
    }
}
