using System.Windows;

namespace Open_lab.Services
{
    public class MainWindowLayoutService : IMainWindowLayoutService
    {
        public void ApplyLoginLayout()
        {
            var window = Application.Current?.MainWindow;
            if (window == null)
            {
                return;
            }

            window.Width = 400;
            window.Height = 550;
            window.ResizeMode = ResizeMode.NoResize;
            Center(window);
        }

        public void ApplyAppLayout()
        {
            var window = Application.Current?.MainWindow;
            if (window == null)
            {
                return;
            }

            window.Width = 1100;
            window.Height = 700;
            window.ResizeMode = ResizeMode.CanResize;
            Center(window);
        }

        private static void Center(Window window)
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            window.Left = (screenWidth - window.Width) / 2;
            window.Top = (screenHeight - window.Height) / 2;
        }
    }
}
