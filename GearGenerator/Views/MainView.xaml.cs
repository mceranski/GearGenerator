using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using GearGenerator.Helpers;
using GearGenerator.ViewModels;

namespace GearGenerator.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            Loaded += delegate
            {
                var canvas = VisualTreeUtils.FindChild<DrawingCanvas>(Application.Current.MainWindow, "ZoomCanvas");
                Slider.ValueChanged += (o, eventArgs) => canvas.InvalidateMeasure();

                mnuPrint.CommandParameter = canvas;

                var scaleTransform = new ScaleTransform();
                BindingOperations.SetBinding( scaleTransform, ScaleTransform.ScaleXProperty, new Binding { Source = Slider, Path = new PropertyPath("Value") });
                BindingOperations.SetBinding( scaleTransform, ScaleTransform.ScaleYProperty, new Binding { Source = Slider, Path = new PropertyPath("Value") });
                canvas.RenderTransform = scaleTransform;
            };
        }

        
    }
}
