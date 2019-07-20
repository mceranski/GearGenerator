using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GearGenerator.Controls;

namespace GearGenerator.Views
{
    public partial class MainView
    {
        private readonly List<GearControl> _gears;

        public MainView()
        {
            InitializeComponent();

            _gears = new List<GearControl>();
            AnimateCheckbox.IsChecked = false;
            GuidelinesCheckbox.IsChecked = true;
            ShowGridCheckbox.IsChecked = true;
            TextOverlaysCheckbox.IsChecked = true;

            var smallGear = AddGear(8, 200, 27, new Point(150,150));
            smallGear.RevolutionsPerMinute = 60;

            var largeGear = AddGear(16, 400, 27, new Point(375,356));
            largeGear.RevolutionsPerMinute = 3;
        }

        private GearControl AddGear( int numberOfTeeth, double pitchDiameter, double pressureAngle, Point? centerPoint = null )
        {
            var control = new GearControl
            {
                NumberOfTeeth = numberOfTeeth,
                PitchDiameter = pitchDiameter,
                PressureAngle = pressureAngle,
                Fill = new LinearGradientBrush(Colors.LightGray, Colors.WhiteSmoke, new Point(0,0), new Point(1,0)),
                GuidelineColor = Brushes.DimGray
            };

            control.SetValue(Panel.ZIndexProperty, 1);

            var gears = ZoomCanvas.Children.OfType<GearControl>().ToList();

            control.Number = gears.Count+1;

            if (centerPoint == null)
            {
                var xPos = gears.Any()
                    ? gears.Last().CenterPoint.X + (gears.Last().PitchDiameter / 2) + (pitchDiameter / 2)
                    : control.PitchDiameter;

                var yPos = gears.Any()
                    ? gears.Last().CenterPoint.Y
                    : control.PitchDiameter;

                centerPoint = new Point(xPos, yPos);
            }

            control.ShowGuidelines = true;
            control.CenterPoint = centerPoint.Value;
            control.SweepDirection = gears.Count > 0
                ? gears.Last().SweepDirection == SweepDirection.Clockwise
                    ? SweepDirection.Counterclockwise
                    : SweepDirection.Clockwise
                : SweepDirection.Clockwise;

            _gears.Add(control);

            var item = new ListBoxItem {Content = control.Title, Tag = control};
            var index = GearList.Items.Add(item);
            GearList.SelectedIndex = index;
            ZoomCanvas.Children.Add(control);

            control.PreviewMouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs args)
            {
                if (!(sender is GearControl gearControl)) return;
                var selectedIndex = GearList.Items.Cast<ListBoxItem>()
                    .Select((i, x) => Equals(i.Tag, gearControl) ? x : -1)
                    .Max();

                if( selectedIndex >= 0 ) GearList.SelectedIndex = selectedIndex;
            };

            return control;
        }

        private void AddGear(object sender, EventArgs e)
        {
            AddGear(8, 200, 27);
        }

        private void DeleteGear(object sender, RoutedEventArgs e)
        {
            var item = GearList.SelectedItem as ListBoxItem;
            var control = item?.Tag as GearControl;
            _gears.Remove(control);
            GearList.Items.Remove(item);
            ZoomCanvas.Children.Remove(control);
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

        private void TextOverlays_Checked(object sender, RoutedEventArgs e)
        {
            Apply(x => x.ShowTextOverlay = TextOverlaysCheckbox.IsChecked == true);
        }

        private void ShowGrid_Checked(object sender, RoutedEventArgs e)
        {
            ZoomCanvas.ShowGrid = ShowGridCheckbox.IsChecked == true;
        }

        private void Slider_ValueChanged(object sender, RoutedEventArgs e)
        {
            ZoomCanvas.InvalidateMeasure();
        }

        private void Apply(Action<GearControl> action)
        {
            foreach (var gear in _gears) {
                action.Invoke(gear);
            }
        }

        private void Print_OnClick(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true) return;
            VisualPrinter.PrintAcrossPages(printDialog, ZoomCanvas);
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
