using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PocketCat.Controls
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(AppSettings appSettings)
        {
            InitializeComponent();
            DataContext = appSettings; // Set the DataContext to the provided AppSettings instance
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove(); // Allows the user to drag the window
            }
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        public int Min { get; set; }
        public int Max { get; set; }

        public ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse(value.ToString(), out int inputValue))
            {
                if (inputValue >= Min && inputValue <= Max)
                {
                    return ValidationResult.ValidResult;
                }
            }

            return new ValidationResult(false, $"Please enter an integer between {Min} and {Max}.");
        }

    }
}
