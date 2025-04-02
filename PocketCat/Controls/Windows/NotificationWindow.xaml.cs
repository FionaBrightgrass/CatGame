using System;
using System.Windows;
using System.Windows.Input;

namespace PocketCat.Controls.Windows
{
    public partial class NotificationWindow : Window
    {
        // Events to notify other classes (e.g., MainWindow) about button clicks
        public event Action ContinueClicked;
        public event Action CancelClicked;
        private MainWindow MainWindow;
        public NotificationWindow(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            InitializeComponent();
        }

        // Method to dynamically set the notification text
        public void SetNotificationText(string text)
        {
            NotificationTextBlock.Text = text;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise ContinueClicked event and close the window
            ContinueClicked?.Invoke();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise CancelClicked event and close the window
            CancelClicked?.Invoke();
            this.Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    MainWindow.DragMove();
                    this.DragMove(); // Allows the user to drag the window
                }
                catch
                {

                }
            }
        }

    }
}
