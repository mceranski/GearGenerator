using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using GearGenerator.Controls;

namespace GearGenerator.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        private readonly List<GearControl> _gears;
        //private Point? dragStart = null;

        public MainView()
        {
            InitializeComponent();

            _gears = new List<GearControl>();
            AnimateCheckbox.IsChecked = false;
            GuidelinesCheckbox.IsChecked = true;
            AddGear(this, EventArgs.Empty);
        }

        private void AddGear(int numberOfTeeth, double pitchDiameter, double pressureAngle)
        {
            var control = new GearControl
            {
                NumberOfTeeth = numberOfTeeth,
                PitchDiameter = pitchDiameter,
                PressureAngle = pressureAngle,
                Opacity = .5
            };

            var gears = ZoomCanvas.Children.Cast<GearControl>().ToList();

            var xPos = gears.Any()
                ? gears.Last().CenterPoint.X + gears.Last().PitchDiameter
                : control.PitchDiameter;

            var yPos = gears.Any()
                ? gears.Last().CenterPoint.Y - 10
                : control.PitchDiameter;

            control.ShowGuidelines = true;
            control.CenterPoint = new Point(xPos, yPos);
            control.SweepDirection = gears.Count > 0
                ? gears.Last().SweepDirection == SweepDirection.Clockwise
                    ? SweepDirection.Counterclockwise
                    : SweepDirection.Clockwise
                : SweepDirection.Clockwise;

            _gears.Add(control);
            ZoomCanvas.Children.Add(control);
        }

        //private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    var element = (UIElement)sender;
        //    dragStart = e.GetPosition(element);
        //    element.CaptureMouse();
        //}

        //private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        //{
        //    if (_canvas == null) return;

        //    if (dragStart == null || e.LeftButton != MouseButtonState.Pressed) return;
        //    var element = (UIElement)sender;

        //    var p2 = e.GetPosition(_canvas);

        //    if (!(element is Grid grid)) return;
        //    var gearVm = grid.DataContext as GearViewModel;
        //    gearVm.CenterX = p2.X;
        //    gearVm.CenterY = p2.Y;
        //}

        //private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    var element = (UIElement)sender;
        //    dragStart = null;
        //    element.ReleaseMouseCapture();
        //}
        private void AddGear(object sender, EventArgs e)
        {
            AddGear(8, 200, 27);
        }

        private void Animate_Checked(object sender, RoutedEventArgs e)
        {
            if (AnimateCheckbox.IsChecked == true)
                Apply(x => x.Start());
            else
                Apply(x => x.Stop());
        }

        private void Guidelines_Checked(object sender, RoutedEventArgs e)
        {
            Apply(x => x.ShowGuidelines = GuidelinesCheckbox.IsChecked == true);
        }

        private void Slider_ValueChanged(object sender, RoutedEventArgs e)
        {
            ZoomCanvas.InvalidateMeasure();
        }

        private void Apply(Action<GearControl> action)
        {
            foreach (var gear in _gears)
            {
                action.Invoke(gear);
                gear.Draw();
            }
        }

        private void Print_OnClick(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true) return;
            var pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            ZoomCanvas.Measure(pageSize);
            ZoomCanvas.Arrange(new Rect(5, 5, pageSize.Width, pageSize.Height));

            if (printDialog.ShowDialog() != true) return;
            printDialog.PrintVisual(ZoomCanvas, "Printing Canvas");
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
