using System;
using System.Collections.Generic;
using System.IO;
using PocketCat.StaticGame.Items;

namespace PocketCat.StaticGame.Stats
{
    public static class StatSheet
    {
        // File path for saving stats
        private static readonly string _statsFilePath = Path.Combine(Globals.AppWide.ApplicationData,"stats.txt");
        public static MainWindow _mainWindow;
        public const int MaxValue = 10;
        // Dictionary to store all stats as Stat objects
        private static readonly Dictionary<string, Stat> _statDictionary = new Dictionary<string, Stat>
        {
            { "Hunger", new Stat("Hunger") },
            { "Energy", new Stat("Energy") },
            { "Happiness", new Stat("Happiness") }
        };

        // Static constructor to load stats from file at runtime
        static StatSheet()
        {
        }

        // Method to retrieve a Stat object by key
        public static Stat GetStat(string key)
        {
            if (_statDictionary.TryGetValue(key, out var stat))
            {
                return stat;
            }

            throw new KeyNotFoundException($"Stat with key '{key}' not found.");
        }

        // Method to update a stat value dynamically by key
        public static async Task UpdateStat(string key, int amount)
        {
            if (_statDictionary.TryGetValue(key, out var stat))
            {
                if( (stat.Value + amount) > 10 && Flags.AffectionReady == true)
                {
                    _mainWindow.Affection.AddAffectionValue(stat.Value + amount - 10);
                    Flags.AffectionReady = false;
                    Console.WriteLine("Attempting to increase affection!");
                    Console.WriteLine("New Affection: " + _mainWindow.Affection.AffectionValue.ToString());
                }

                int newValue = Math.Clamp(stat.Value + amount, 0, Stat.MaxValue);
                stat.Value = newValue;
                SaveStatsToFile();
            }
            else
            {
                throw new KeyNotFoundException($"Stat with key '{key}' not found.");
            }
        }

        // Save stats to a file
        public static void SaveStatsToFile()
        {
            using (StreamWriter writer = new StreamWriter(_statsFilePath))
            {
                foreach (var kvp in _statDictionary)
                {
                    writer.WriteLine($"{kvp.Key}:{kvp.Value.Value}");
                }
                writer.WriteLine("Affection:" + _mainWindow.AffectionBoxText);
                writer.Close();
            }
        }

        // Load stats from a file
        public static void LoadStatsFromFile()
        {
            if (!File.Exists(_statsFilePath)) return;

            string[] lines = File.ReadAllLines(_statsFilePath);
            foreach (var line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2 && _statDictionary.ContainsKey(parts[0]))
                {
                    if (int.TryParse(parts[1], out int value))
                    {
                        _statDictionary[parts[0]].Value = Math.Clamp(value, 0, Stat.MaxValue);
                    }
                }
                if (parts.Length == 2 && line.Contains("Affection"))
                {
                    if (int.TryParse(parts[1], out int value))
                    {
                        _mainWindow.Affection.AffectionValue = value;
                    }

                }
            }
        }

        // Example of subscribing to stat changes for all stats
        public static void SubscribeToAllStatChanges()
        {
            foreach (var stat in _statDictionary.Values)
            {
                Stat.StatChanged += (key, value) =>
                {
                    Console.WriteLine($"Stat '{key}' updated to {value}.");
                };
            }
        }

        // Configure blink bar callbacks for all stats
        public static void ConfigureBlinkBars()
        {
            foreach (var stat in _statDictionary.Values)
            {
                stat.BlinkBarCallback = async (key, value, isIncrement, isMaxed) =>
                {
                    Console.WriteLine($"BlinkBarCallback for '{key}' | Value: {value} | Increment: {isIncrement} | Maxed: {isMaxed}");
                    await stat.BlinkBarAsync(isIncrement, isMaxed);
                };
            }
        }
    }
}
