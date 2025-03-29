using System;
using System.ComponentModel;
using System.IO;

namespace PocketCat.StaticGame.Items
{
    public class Money : INotifyPropertyChanged
    {
        private readonly string ItemsFile = Globals.AppWide.ApplicationData + "/items.txt";
        private DateTime LastPayDay;
        public int Pay = 15;
        private int _amount = 10;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Amount
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
        public string FormattedAmount => $"${Amount}";
        public Money()
        {
            LoadItems(); // Initialize from file
        }

        public void AddMoney(int amount)
        {
            Amount += amount;
        }

        public void CheckAndRunDailyPayout()
        {
            DateTime today = DateTime.Now.Date;

            if (LastPayDay != today)
            {
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
                File.WriteAllLines(ItemsFile, new string[] { "Money:10", $"LastPayDate:{DateTime.Now:yyyy-MM-dd}" });
                LastPayDay = DateTime.Now.Date;
                if(!File.Exists(ItemsFile)){
                    Console.WriteLine($"File {ItemsFile} not found. Creating a new file.");
                    return;
                }
            }

            string[] lines = File.ReadAllLines(ItemsFile);

            foreach (var line in lines)
            {
                if (line.StartsWith("Money:"))
                {
                    string moneyValue = line.Substring(6).Trim();
                    if (int.TryParse(moneyValue, out int parsedMoney))
                    {
                        Amount = parsedMoney;
                    }
                }
                else if (line.StartsWith("LastPayDate:"))
                {
                    string dateValue = line.Substring(12).Trim();
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
