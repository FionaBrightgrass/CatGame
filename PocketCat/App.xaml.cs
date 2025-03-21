using System.Windows;
using PocketCat.Controls; // Reference to the class library

namespace PocketCat
{
    public partial class App : Application
    {
        private CatTrayIcon SystemTrayIco = new CatTrayIcon();


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private void OpenApp()
        {
            // Open or activate the main window
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
            }

            MainWindow.Show();
            MainWindow.Activate();
        }

        private void ExitApp()
        {
            // Dispose of the tray icon and shutdown the application
            // SystemTrayIcon?.ExitApplication();
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose of the tray icon when exiting
            // SystemTrayIcon?.ExitApplication();
            base.OnExit(e);
        }
    }
}