using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
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
            this.DataContext = new MainViewModel();

            this.Loaded += delegate(object sender, RoutedEventArgs args)
            {
                var canvas = FindChild<DrawingCanvas>(Application.Current.MainWindow, "ZoomCanvas");
                Slider.ValueChanged += (o, eventArgs) => canvas.InvalidateMeasure();

                var scaleTransform = new ScaleTransform();
                BindingOperations.SetBinding( scaleTransform, ScaleTransform.ScaleXProperty, new Binding { Source = Slider, Path = new PropertyPath("Value") });
                BindingOperations.SetBinding( scaleTransform, ScaleTransform.ScaleYProperty, new Binding { Source = Slider, Path = new PropertyPath("Value") });
                canvas.RenderTransform = scaleTransform;
            };
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                if (!(child is T))
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    // If the child's name is set for search
                    if (!(child is FrameworkElement frameworkElement) || frameworkElement.Name != childName) continue;
                    // if the child's name is of the request name
                    foundChild = (T)child;
                    break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

    }
}
