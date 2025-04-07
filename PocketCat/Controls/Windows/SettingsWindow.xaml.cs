using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using PocketCat.StaticGame;

namespace PocketCat.Controls
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private MainWindow MainWindow;
        private PocketCat.StreamElements.StreamElementsClient _se;
        public SettingsWindow(AppSettings appSettings, MainWindow mainWindow, StreamElements.StreamElementsClient se)
        {
            MainWindow = mainWindow;
            _se = se;
            InitializeComponent();
            DataContext = appSettings; // Set the DataContext to the provided AppSettings instance
                                       // Bind the height of Shop to MainWindow
            this.Height = mainWindow.ActualHeight;

            // Set the initial position to the right of MainWindow
            this.Top = mainWindow.Top;
            this.Left = mainWindow.Left + mainWindow.ActualWidth;

            // Update height and position dynamically if MainWindow size or location changes
            mainWindow.SizeChanged += (sender, e) =>
            {
                this.Height = mainWindow.ActualHeight;
                this.Top = mainWindow.Top;
                this.Left = mainWindow.Left + mainWindow.ActualWidth;
            };

            mainWindow.LocationChanged += (sender, e) =>
            {
                this.Top = mainWindow.Top;
                this.Left = mainWindow.Left + mainWindow.ActualWidth;
            };
            _se = se;
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

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            MainWindow.MainBorder.BorderThickness = new Thickness(2,2,2,2); // Reset the border thickness
            MainWindow.MainBorder.CornerRadius = new CornerRadius(5);
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
        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string name = textBox.Text;
                var appSettings = (AppSettings)Application.Current.Resources["AppSettings"];
                appSettings.Name = name;
            }
        }
        // ...

        private void Pronouns_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text.Split('/').Length != 3)
                    textBox.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)); // Fix: Use SolidColorBrush instead of Brush
                string pronouns = textBox.Text;
                var appSettings = (AppSettings)Application.Current.Resources["AppSettings"];
                appSettings.Pronouns = pronouns;
            }
        }

        private void JWT_Changed(object sender, RoutedEventArgs e)
        {
            _se.jwtToken = JWT.Password;
            Encryption.SaveSecureFile(_se.jwtToken, _se.jwtTokenPath);
        }


    }
}
