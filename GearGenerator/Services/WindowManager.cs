using GearGenerator.ViewModels;
using GearGenerator.Views;

namespace GearGenerator.Services
{
    public class WindowManager
    {
        private MainView _mainView;

        public void StartUp()
        {
            _mainView = new MainView{ DataContext = new MainViewModel()};
            _mainView.Show();
        }

        public void Shutdown()
        {
            _mainView.Close();
        }

    }
}
