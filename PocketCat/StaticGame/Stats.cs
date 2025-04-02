using System.IO;
using PocketCat.Controls;

namespace PocketCat.StaticGame
{
    public static class Stats2
    {
        public static AppSettings _AppSettings;
        public static Dictionary<int, bool> BarBlinkingStates { get; set; } = new Dictionary<int, bool>();
        private static readonly string StatsFile = Globals.AppWide.ApplicationData + "/stats.txt";
        public const int MaxStatValue = 10;

        public static Func<string, int, bool, bool, Task> BlinkBarCallback { get; set; }

        private static Dictionary<string, int> StatDict = new Dictionary<string, int>
        {
            { "Affection", 2 },
            { "Hunger", 5 },
            { "Energy", 5 }
        };

        static Stats2()
        {
            SetStatsFromFile();
        }

        // Event to notify when a stat changes
        public static event Action<string, int> StatChanged;

        // Cooldown Methods for Pet
       

        // Incremental Sleep Method
        public static bool _isBlinking { get; set; } = false; // Tracks if a bar is blinking

        



        // Stat Management Methods
        public static int GetStat(string key) => StatDict.TryGetValue(key, out int value) ? value : 0;

        public static async Task UpdateStat(string key, int amount)
        {
            if (!StatDict.ContainsKey(key)) StatDict[key] = 0;
            int currentStat = StatDict[key];
            int newStat = Math.Clamp(currentStat + amount, 0, MaxStatValue);
            StatDict[key] = newStat;
            OnStatChanged(key, newStat);
            WriteStatsToFile();
        }

        private static void OnStatChanged(string key, int newValue)
        {
            StatChanged?.Invoke(key, newValue);
        }

        private static void SetStatsFromFile()
        {
            if (!File.Exists(StatsFile)) CreateStatsFile();

            string[] statFileLines = File.ReadAllLines(StatsFile);
            foreach (string line in statFileLines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2 && StatDict.ContainsKey(parts[0]))
                {
                    StatDict[parts[0]] = int.Parse(parts[1]);
                }
            }
        }

        private static void WriteStatsToFile()
        {
            using (StreamWriter sw = new StreamWriter(StatsFile))
            {
                foreach (var kvp in StatDict)
                {
                    sw.WriteLine($"{kvp.Key}:{kvp.Value}");
                }
            }
        }

        private static void CreateStatsFile()
        {
            using (StreamWriter sw = new StreamWriter(StatsFile))
            {
                foreach (var kvp in StatDict)
                {
                    sw.WriteLine($"{kvp.Key}:0");
                }
            }
        }
    }
}
