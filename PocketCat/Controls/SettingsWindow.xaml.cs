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

        private bool IsValidHexColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex) || !hex.StartsWith("#") || hex.Length != 7)
            {
                return false;
            }

            // Ensure the remaining characters are valid hex digits
            for (int i = 1; i < hex.Length; i++)
            {
                char c = hex[i];
                if (!Uri.IsHexDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        private void PrimaryColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string newColor = textBox.Text;

                if (IsValidHexColor(newColor))
                {
                    var appSettings = (AppSettings)Application.Current.Resources["AppSettings"];
                    appSettings.PrimaryColor = newColor; // Update PrimaryColor
                }
                else
                {
                    Console.WriteLine($"Invalid PrimaryColor entered: {newColor}");
                }
            }
        }

        // Add support for SecondaryColor
        private void SecondaryColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string newColor = textBox.Text;

                if (IsValidHexColor(newColor))
                {
                    var appSettings = (AppSettings)Application.Current.Resources["AppSettings"];
                    appSettings.SecondaryColor = newColor; // Update SecondaryColor
                }
                else
                {
                    Console.WriteLine($"Invalid SecondaryColor entered: {newColor}");
                }
            }
        }

        // Add support for EarColor
        private void TertiaryColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string newColor = textBox.Text;

                if (IsValidHexColor(newColor))
                {
                    var appSettings = (AppSettings)Application.Current.Resources["AppSettings"];
                    appSettings.TertiaryColor = newColor; // Update EarColor
                }
                else
                {
                    Console.WriteLine($"Invalid EarColor entered: {newColor}");
                }
            }
        }
        private void DetailColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string newColor = textBox.Text;

                if (IsValidHexColor(newColor))
                {
                    var appSettings = (AppSettings)Application.Current.Resources["AppSettings"];
                    appSettings.DetailColor = newColor; // Update DetailColor
                }
                else
                {
                    Console.WriteLine($"Invalid DetailColor entered: {newColor}");
                }
            }
        }

        private void EyeColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string newColor = textBox.Text;

                if (IsValidHexColor(newColor))
                {
                    var appSettings = (AppSettings)Application.Current.Resources["AppSettings"];
                    appSettings.EyeColor = newColor; // Update EyeColor
                }
                else
                {
                    Console.WriteLine($"Invalid EyeColor entered: {newColor}");
                }
            }
        }

    }
}
