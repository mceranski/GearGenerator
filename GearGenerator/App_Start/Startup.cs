using System;

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
                App.WindowManager.StartUp();
            };

            app.InitializeComponent();
            app.Run();
        }
    }
}
