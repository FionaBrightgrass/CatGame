using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace PocketCat
{
    public class AppSettings : INotifyPropertyChanged
    {
        string path = Path.Combine(PocketCat.Globals.AppWide.ApplicationData, "usersettings.txt");
        public AppSettings()
        {
            LoadSettingsFromFile();
        }

        public void LoadSettingsFromFile()
        {

            if (!File.Exists(path))
            {
                CreateSettingsFile(path);
            }
            else
            {
                string[] lines = File.ReadAllLines(path);
                if (lines.Length < 10)
                    CreateSettingsFile(path);
                foreach (string line in lines)
                {
                    if (line.Contains(":"))
                    {
                        string[] data = line.Split(':');
                        string identifier = data[0];
                        if (data.Length < 2)
                            CreateSettingsFile(path);
                        switch (identifier)
                        {
                            case "WindowColor":
                                _windowColor = line.Split(':')[1];
                                WindowColorText = line.Split(':')[1];
                                break;
                            case "BorderColor":
                                _borderColor = line.Split(":")[1];
                                BorderColorText = line.Split(':')[1];
                                break;
                            case "ButtonColor":
                                _buttonColor = line.Split(':')[1];
                                ButtonColorText = line.Split(':')[1];
                                break;
                            case "TextColor":
                                _textColor = line.Split(':')[1];
                                TextColorText = line.Split(':')[1];
                                break;
                            case "PrimaryColor":
                                _primaryColor = line.Split(':')[1];
                                PrimaryColor = line.Split(':')[1];
                                break;
                            case "SecondaryColor":
                                _secondaryColor = line.Split(':')[1];
                                SecondaryColor = line.Split(':')[1];
                                break;
                            case "TertiaryColor":
                                _tertiaryColor = line.Split(':')[1];
                                TertiaryColor = line.Split(':')[1];
                                break;
                            case "DetailColor":
                                _detailColor = line.Split(':')[1];
                                DetailColor = line.Split(':')[1];
                                break;
                            case "EyeColor":
                                _eyeColor = line.Split(':')[1];
                                EyeColor = line.Split(':')[1];
                                break;
                            case "SleepInterval":
                                _sleepInterval = int.Parse(line.Split(':')[1].Trim());
                                SleepInterval = int.Parse(line.Split(':')[1].Trim());
                                break;
                        }
                    }
                }
            }
        }

        private void CreateSettingsFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            // Use a StreamWriter to create and write to the file
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine("WindowColor:" + _windowColor);
                writer.WriteLine("BorderColor:" + _borderColor);
                writer.WriteLine("ButtonColor:" + _buttonColor);
                writer.WriteLine("TextColor:" + _textColor);
                writer.WriteLine("PrimaryColor:" + _primaryColor);
                writer.WriteLine("SecondaryColor:" + _secondaryColor);
                writer.WriteLine("TertiaryColor:" + _tertiaryColor);
                writer.WriteLine("DetailColor:" + _detailColor);
                writer.WriteLine("EyeColor:" + _eyeColor);
                writer.WriteLine("SleepInterval:" + _sleepInterval.ToString());
                writer.Close();
            }
        }

        private string _windowColor; // Default hex color

        // Bound to TextBox in the UI
        public string WindowColorText
        {
            get => _windowColor;
            set
            {
                if (_windowColor != value)
                {
                    _windowColor = value;
                    try
                    {
                        // Try to convert the string to a Color
                        var color = (Color)ColorConverter.ConvertFromString(_windowColor);
                        OnPropertyChanged(nameof(WindowColorBrush)); // Notify changes for dependent properties
                        CreateSettingsFile(path);
                    }
                    catch
                    {
                        // If the conversion fails, do nothing
                    }
                    OnPropertyChanged(nameof(WindowColorText));
                }
            }
        }

        // Returns the SolidColorBrush for use in Background, etc.
        public SolidColorBrush WindowColorBrush
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(_windowColor));
                }
                catch
                {
                    return new SolidColorBrush(Colors.Black); // Fallback
                }
            }
        }


        private string _buttonColor = "#FF3060"; // Default hex color

        // Bound to TextBox in the UI
        public string ButtonColorText
        {
            get => _buttonColor;
            set
            {
                if (_buttonColor != value)
                {
                    _buttonColor = value;
                    try
                    {
                        // Try to convert the string to a Color
                        var color = (Color)ColorConverter.ConvertFromString(_buttonColor);
                        OnPropertyChanged(nameof(ButtonColorBrush)); // Notify changes for dependent properties
                        CreateSettingsFile(path);
                    }
                    catch
                    {
                        // If the conversion fails, do nothing
                    }
                    OnPropertyChanged(nameof(ButtonColorText));
                }
            }
        }

        // Returns the SolidColorBrush for use in Background, etc.
        public SolidColorBrush ButtonColorBrush
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(_buttonColor));
                }
                catch
                {
                    // Default to Black if invalid color string
                    return new SolidColorBrush(Colors.Black);
                }
            }
        }

        private string _textColor = "#FF3060"; // Default hex color

        // Bound to TextBox in the UI
        public string TextColorText
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    try
                    {
                        // Try to convert the string to a Color
                        var color = (Color)ColorConverter.ConvertFromString(_textColor);
                        OnPropertyChanged(nameof(TextColorBrush)); // Notify changes for dependent properties
                        CreateSettingsFile(path);
                    }
                    catch
                    {
                        // If the conversion fails, do nothing
                    }
                    OnPropertyChanged(nameof(TextColorText));
                }
            }
        }

        // Returns the SolidColorBrush for use in Background, etc.
        public SolidColorBrush TextColorBrush
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(_textColor));
                }
                catch
                {
                    // Default to Black if invalid color string
                    return new SolidColorBrush(Colors.Black);
                }
            }
        }

        private string _borderColor = "#FF3060"; // Default hex color

        // Bound to TextBox in the UI
        public string BorderColorText
        {
            get => _borderColor;
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    try
                    {
                        // Try to convert the string to a Color
                        var color = (Color)ColorConverter.ConvertFromString(_borderColor);
                        OnPropertyChanged(nameof(BorderColorBrush)); // Notify changes for dependent properties
                        CreateSettingsFile(path);
                    }
                    catch
                    {
                        // If the conversion fails, do nothing
                    }
                    OnPropertyChanged(nameof(BorderColorText));
                }
            }
        }

        // Returns the SolidColorBrush for use in Background, etc.
        public SolidColorBrush BorderColorBrush
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(_borderColor));
                }
                catch
                {
                    // Default to Black if invalid color string
                    return new SolidColorBrush(Colors.Black);
                }
            }
        }


        private int _sleepInterval = 3; // Default to 10 minutes
        public int SleepInterval
        {
            get => _sleepInterval;
            set
            {
                if (_sleepInterval != value && value >= 1 && value <= 60) // Validate range (1-60)
                {
                    _sleepInterval = value;

                    try
                    {
                        // Save the updated settings to file
                        OnPropertyChanged(nameof(SleepInterval));
                        CreateSettingsFile(path); // Replace 'path' with your actual settings file path
                    }
                    catch
                    {
                        // Handle any exceptions related to saving, if needed
                    }

                    OnPropertyChanged(nameof(SleepInterval)); // Notify listeners of the change
                }
            }
        }

        private string _primaryColor = "#202020";
        private string _secondaryColor = "#252525";
        private string _tertiaryColor = "#303030";
        private string _detailColor = "#C090B0";
        private string _eyeColor = "#A00090";

        public string PrimaryColor
        {
            get => _primaryColor;
            set
            {
                _primaryColor = value;
                try
                {
                    // Save the updated settings to file
                    CreateSettingsFile(path); // Replace 'path' with your actual settings file path
                }
                catch
                {
                    // Handle any exceptions related to saving, if needed
                }
                OnPropertyChanged(nameof(PrimaryColor));
            }
        }
        public string SecondaryColor
        {
            get => _secondaryColor;
            set
            {
                _secondaryColor = value;
                try
                {
                    // Save the updated settings to file
                    CreateSettingsFile(path); // Replace 'path' with your actual settings file path
                }
                catch
                {
                    // Handle any exceptions related to saving, if needed
                }
                OnPropertyChanged(nameof(SecondaryColor));
            }
        }
        public string TertiaryColor
        {
            get => _tertiaryColor;
            set
            {
                _tertiaryColor = value;
                try
                {
                    // Save the updated settings to file
                    CreateSettingsFile(path); // Replace 'path' with your actual settings file path
                }
                catch
                {
                    // Handle any exceptions related to saving, if needed
                }
                OnPropertyChanged(nameof(TertiaryColor));
            }
        }
        public string DetailColor
        {
            get => _detailColor;
            set
            {
                _detailColor = value;
                try
                {
                    // Save the updated settings to file
                    CreateSettingsFile(path); // Replace 'path' with your actual settings file path
                }
                catch
                {
                    // Handle any exceptions related to saving, if needed
                }
                OnPropertyChanged(nameof(DetailColor));
            }
        }

        public string EyeColor
        {
            get => _eyeColor;
            set
            {
                _eyeColor = value;
                try
                {
                    // Save the updated settings to file
                    CreateSettingsFile(path); // Replace 'path' with your actual settings file path
                }
                catch
                {
                    // Handle any exceptions related to saving, if needed
                }
                OnPropertyChanged(nameof(EyeColor));
            }

        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
