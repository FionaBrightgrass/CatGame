using System;
using System.ComponentModel;
using System.IO;

namespace PocketCat.StaticGame.Items
{
    public class Money : INotifyPropertyChanged
    {
        private readonly string ItemsFile = Globals.AppWide.ApplicationData + "/items.txt";
        public DateTime LastPayDay;
        public int Pay = 15;
        private double _amount = 10;

        public event PropertyChangedEventHandler PropertyChanged;

        public double Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged(nameof(Amount));
                    OnPropertyChanged(nameof(FormattedAmount)); // Notify for updated formatted amount
                }
            }
        }
        public string FormattedAmount => $"Balance: {Amount:F2}";
        public Money()
        {
            LoadItems(); // Initialize from file
            Time.InitializeMoney(this);
        }

        public void AddMoney(double amount)
        {
            Amount += amount;
            UpdateMoneyInIniFile();
        }
        public void SubtractMoney(double amount)
        {
            Amount -= amount;
            UpdateMoneyInIniFile();
        }

        private void UpdateMoneyInIniFile()
        {
            string[] lines = File.ReadAllLines(ItemsFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("Money:"))
                {
                    lines[i] = $"Money:{Amount}"; // Replace with the updated Amount
                    break;
                }
            }

            File.WriteAllLines(ItemsFile, lines); // Save the updated lines back to the file
            Console.WriteLine($"Updated Money in file: {Amount}");
        }

        public void CheckAndRunDailyPayout()
        {
            DateTime today = DateTime.Now.Date;

            if (LastPayDay != today)
            {
                Console.WriteLine("Adding " + Pay.ToString() + " to current wallet");
                AddMoney(Pay); // Add daily payout
                LastPayDay = today; // Update LastPayDate
                SaveLastPayDay(); // Persist changes
            }
        }

        public string DisplayMoney()
        {
            return $"${Amount}";
        }

        private void LoadItems()
        {
            if (!File.Exists(ItemsFile))
            {
                File.Create(ItemsFile).Close();
                File.WriteAllLines(ItemsFile, new string[] { "Money:10.00", $"LastPayDate:{DateTime.Now:yyyy-MM-dd}" });
                LastPayDay = DateTime.Now.Date;
                Amount = 10.00; // Default value
                return;
            }

            string[] lines = File.ReadAllLines(ItemsFile);

            foreach (var line in lines)
            {
                if (line.StartsWith("Money:"))
                {
                    string moneyValue = line.Substring(6).Trim(); // Extract the money value
                    if (double.TryParse(moneyValue, out double parsedMoney))
                    {
                        Amount = parsedMoney; // Set the Amount to the parsed value
                    }
                }
                else if (line.StartsWith("LastPayDate:"))
                {
                    string dateValue = line.Substring(12).Trim(); // Extract the LastPayDate value
                    if (DateTime.TryParseExact(dateValue, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    {
                        LastPayDay = parsedDate;
                    }
                }
            }

            Console.WriteLine($"Loaded Money: {Amount}, LastPayDate: {LastPayDay:yyyy-MM-dd}");
        }


        public void SaveLastPayDay()
        {
            string[] lines = File.ReadAllLines(ItemsFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("LastPayDate:"))
                {
                    lines[i] = $"LastPayDate:{LastPayDay:yyyy-MM-dd}";
                    break;
                }
            }

            File.WriteAllLines(ItemsFile, lines);
            Console.WriteLine($"Updated LastPayDate in file: {LastPayDay:yyyy-MM-dd}");
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // Use 'this' instead of 'null'
        }
    }
}
