using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using PocketCat.DefaultData;
using PocketCat.StaticGame;

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
                if (lines.Length < 13)
                {
                    Console.WriteLine("Invalid settings file. Creating new file with defaults.");
                    CreateSettingsFile(path); // Create the file with defaults if invalid
                }
                else
                {
                    foreach (string line in lines)
                    {
                        if (line.Contains(":"))
                        {
                            string[] data = line.Split(':');
                            if (data.Length < 2)
                            {
                                Console.WriteLine("Invalid settings file. Skipping: " + data[0]);
                                continue; // Skip invalid lines
                            }
                            string identifier = data[0];
                            string value = data[1].Trim(); // Trim whitespace around values

                            switch (identifier)
                            {
                                case "SleepInterval":
                                    if (int.TryParse(value.Trim(), out int interval) && interval >= 1 && interval <= 60)
                                    {
                                        _sleepInterval = interval; // Assign only if valid
                                        SleepInterval = interval;
                                    }
                                    break;
                                case "TimeInterval":
                                    if (int.TryParse(value.Trim(), out int ninterval) && ninterval >= 1 && ninterval <= 60)
                                    {
                                        _timeInterval = ninterval; // Assign only if valid
                                        TimeInterval = ninterval;
                                    }
                                    break;
                                case "WindowColor":
                                    _windowColor = value;
                                    WindowColorText = value;
                                    break;
                                case "BorderColor":
                                    _borderColor = value;
                                    BorderColorText = value;
                                    break;
                                case "ButtonColor":
                                    _buttonColor = value;
                                    ButtonColorText = value;
                                    break;
                                case "TextColor":
                                    _textColor = value;
                                    TextColorText = value;
                                    break;
                                case "Name":
                                    ActionData.DataModel.Name = value;
                                    break;
                                case "Pronouns":
                                    ActionData.DataModel.Pronouns = value;
                                    break;
                                case "PrimaryColor":
                                    _primaryColor = value;
                                    PrimaryColor = value;
                                    break;
                                case "SecondaryColor":
                                    _secondaryColor = value;
                                    SecondaryColor = value;
                                    break;
                                case "TertiaryColor":
                                    _tertiaryColor = value;
                                    TertiaryColor = value;
                                    break;
                                case "DetailColor":
                                    _detailColor = value;
                                    DetailColor = value;
                                    break;
                                case "EyeColor":
                                    _eyeColor = value;
                                    EyeColor = value;
                                    break;
                            }
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

            // Write current values to the file
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine("WindowColor:" + _windowColor);
                writer.WriteLine("BorderColor:" + _borderColor);
                writer.WriteLine("ButtonColor:" + _buttonColor);
                writer.WriteLine("TextColor:" + _textColor);
                writer.WriteLine("Name:" + ActionData.DataModel.Name);
                writer.WriteLine("Pronouns:" + ActionData.DataModel.Pronouns);
                writer.WriteLine("PrimaryColor:" + _primaryColor);
                writer.WriteLine("SecondaryColor:" + _secondaryColor);
                writer.WriteLine("TertiaryColor:" + _tertiaryColor);
                writer.WriteLine("DetailColor:" + _detailColor);
                writer.WriteLine("EyeColor:" + _eyeColor);
                writer.WriteLine("SleepInterval:" + _sleepInterval); // Use the current value of SleepInterval
                writer.WriteLine("TimeInterval:" + _timeInterval); // Use the current value of SleepInterval
            }
        }

        private string _name = "Sherlock"; // Default hex color

        // Bound to TextBox in the UI
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    try
                    {
                        OnPropertyChanged(nameof(Name)); // Notify changes for dependent properties
                        CreateSettingsFile(path);
                    }
                    catch
                    {
                        // If the conversion fails, do nothing
                    }
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        private string _pronouns = "He/Him/His"; // Default hex color

        // Bound to TextBox in the UI
        public string Pronouns
        {
            get => _pronouns;
            set
            {
                if (_pronouns != value)
                {
                    _pronouns = value;
                    try
                    {
                        OnPropertyChanged(nameof(Pronouns)); // Notify changes for dependent properties
                        CreateSettingsFile(path);
                    }
                    catch
                    {
                        // If the conversion fails, do nothing
                    }
                    OnPropertyChanged(nameof(Pronouns));
                }
            }
        }

        private string _windowColor = "#000000"; // Default hex color

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


        private string _buttonColor = "#0000000"; // Default hex color

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

        private string _textColor = "#FFFFFF"; // Default hex color

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

        private string _borderColor = "#FFFFFF"; // Default hex color

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
                var valueTemp = Math.Min(Math.Max(value, 1), 60);
                _sleepInterval = valueTemp;
                try
                {
                    CreateSettingsFile(path); // Save updated value
                }
                catch
                {
                    Console.WriteLine("Error saving settings to file.");
                }
                OnPropertyChanged(nameof(SleepInterval)); // Notify listeners of the change
            }
        }

        private int _timeInterval = 6; // Default to 6 minutes
        public int TimeInterval
        {
            get => _timeInterval;
            set
            {
                var valueTemp = Math.Min(Math.Max(value, 1), 60);
                _timeInterval = valueTemp;
                try
                {
                    CreateSettingsFile(path); // Save updated value
                }
                catch
                {
                    Console.WriteLine("Error saving settings to file.");
                }
                OnPropertyChanged(nameof(TimeInterval)); // Notify listeners of the change
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
