using System;
using GearGenerator.ViewModels;
using GearGenerator.Views;

namespace GearGenerator
{
    static class Startup
    {
        [STAThread]
        static void Main()
        {
            var app = new App();
            app.Startup += (sender, args) =>
            {
                var mainView = new MainView{ DataContext = new MainViewModel()};
                mainView.Show();
            };

            app.InitializeComponent();
            app.Run();
        }
    }
}
