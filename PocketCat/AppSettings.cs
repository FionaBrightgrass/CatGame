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
                if (lines.Length < 5)
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
                            case "SleepInterval":
                                _sleepInterval = int.Parse(line.Split(':')[1]);
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



        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
