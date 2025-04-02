using System;
using System.Drawing; // For Icon
using System.Windows;
using System.Windows.Forms; // For NotifyIcon
using Application = System.Windows.Application;

namespace CatTray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private NotifyIcon _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the system tray icon
            _notifyIcon = new NotifyIcon
            {
                Icon = GetIconFromResources("CatTray.CatIcon.ico"), // Load the embedded .ico file
                Visible = true,
                Text = "My WPF Application"
            };

            // Add a context menu to the tray icon
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Open", null, (s, args) => OpenMainWindow());
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, args) => ExitApplication());

            // Handle tray icon double-click event to open the main window
            _notifyIcon.DoubleClick += (s, args) => OpenMainWindow();
        }

        private void OpenMainWindow()
        {
            // Show or activate the main window
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
            }
            MainWindow.Show();
            MainWindow.Activate();
        }

        public void ExitApplication()
        {
            // Dispose of the tray icon and shutdown the application
            _notifyIcon?.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up the tray icon on application exit
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }

        private Icon? GetIconFromResources(string resourceName)
        {
            // Load the .ico file from embedded resources
            var assembly = typeof(App).Assembly; // Get the current assembly
            var fullResourceName = resourceName;

            using var stream = assembly.GetManifestResourceStream(fullResourceName);
            if (stream == null)
            {
                return null;
            }

            return new Icon(stream);
        }
    }

}
