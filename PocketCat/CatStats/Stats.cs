using System.IO;
using PocketCat.Controls;

namespace PocketCat.CatStats
{
    public class Stats
    {
        public Dictionary<int, bool> BarBlinkingStates { get; set; } = new Dictionary<int, bool>();
        private string StatsFile = Globals.AppWide.ApplicationData + "/stats.txt";
        private const int MaxStatValue = 10;
        public Func<string, int, bool, bool, Task> BlinkBarCallback { get; set; }

        private Dictionary<string, int> StatDict = new Dictionary<string, int>
        {
            { "Affection", 0 },
            { "Hunger", 0 },
            { "Energy", 0 }
        };

        public DateTime? PetCooldownStartTime { get; set; }
        public static readonly TimeSpan PetCooldownDuration = TimeSpan.FromMinutes(5);

        public DateTime? SleepCooldownStartTime { get; set; }
        public static readonly TimeSpan SleepCooldownDuration = TimeSpan.FromHours(1);

        private bool _isSleeping;
        public bool isSleeping
        {
            get => _isSleeping;
            set
            {
                if (_isSleeping != value)
                {
                    _isSleeping = value;
                    StatChanged?.Invoke("isSleeping", value ? 1 : 0);
                }
            }
        }

        private System.Timers.Timer _sleepTimer; // Timer to handle sleep increments
        public int SleepIncrement = 30; // Time interval for adding one energy bar (in ms)

        public Stats()
        {
            SetStatsFromFile();
            LoadCooldownsFromFile();
        }

        // Event to notify when a stat changes
        public event Action<string, int> StatChanged;

        // Cooldown Methods for Pet
        public bool IsPetOnCooldown()
        {
            if (!PetCooldownStartTime.HasValue) return false;
            return DateTime.Now < PetCooldownStartTime.Value.Add(PetCooldownDuration);
        }

        public async Task StartPetCooldownAsync()
        {
            PetCooldownStartTime = DateTime.Now;
            SaveCooldownsToFile();
            await Task.Delay(PetCooldownDuration);
            PetCooldownStartTime = null;
            SaveCooldownsToFile();
        }

        // Cooldown Methods for Sleep
        public bool IsSleepOnCooldown()
        {
            if (!SleepCooldownStartTime.HasValue) return false;
            return DateTime.Now < SleepCooldownStartTime.Value.Add(SleepCooldownDuration);
        }

        public async Task StartSleepCooldownAsync()
        {
            SleepCooldownStartTime = DateTime.Now;
            SaveCooldownsToFile();
            isSleeping = true;

            await Task.Delay(SleepCooldownDuration);

            isSleeping = false;
            SleepCooldownStartTime = null;
            SaveCooldownsToFile();
        }

        // Incremental Sleep Method
        public bool _isBlinking { get; set; } = false; // Tracks if a bar is blinking

        public async Task StartSleepWithIncrementsAsync(int energyNeeded, Statbar energyBar)
        {
            if (energyNeeded <= 0 || isSleeping) return; // Exit if no energy needed or already sleeping

            isSleeping = true; // Set the sleeping state
            SleepCooldownStartTime = DateTime.Now; // Record cooldown start time
            SaveCooldownsToFile();

            try
            {
                // Gradually restore energy
                while (energyNeeded > 0)
                {
                    int currentEnergy = GetStat("Energy");

                    if (currentEnergy < MaxStatValue)
                    {
                        try
                        {
                            Console.WriteLine($"Starting blinking for energy bar index {currentEnergy}...");

                            // Update blinking states in the Statbar
                            energyBar.UpdateBlinkStates(currentEnergy, currentEnergy, 1); // Set state to +1 (increasing)

                            // Trigger blinking and wait for it to complete
                            await energyBar.HandleBlinkingAsync(currentEnergy + 1); // Update up to the next bar

                            // Increment the energy stat
                            Console.WriteLine($"Incrementing energy stat for bar index {currentEnergy}...");
                            await UpdateStat("Energy", 1);
                            energyNeeded--; // Decrement remaining energy to restore
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error during blinking or stat update for bar index {currentEnergy}: {ex.Message}");
                        }
                    }
                    else
                    {
                        break; // Stop if max energy is reached
                    }

                    // Wait for the sleep interval before continuing
                    await Task.Delay(SleepIncrement);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartSleepWithIncrementsAsync: {ex.Message}");
            }
            finally
            {
                energyBar.ResetBlinkStates(); // Reset blinking states after completion
                isSleeping = false; // Reset the sleeping state
                SaveCooldownsToFile();
            }
        }







        // Save/Load Cooldowns to/from File
        private void SaveCooldownsToFile()
        {
            using (StreamWriter sw = new StreamWriter(Globals.AppWide.ApplicationData + "/cooldowns.txt"))
            {
                sw.WriteLine(PetCooldownStartTime.HasValue ? PetCooldownStartTime.Value.ToString("o") : string.Empty);
                sw.WriteLine(SleepCooldownStartTime.HasValue ? SleepCooldownStartTime.Value.ToString("o") : string.Empty);
            }
        }

        private void LoadCooldownsFromFile()
        {
            string cooldownFile = Globals.AppWide.ApplicationData + "/cooldowns.txt";
            if (File.Exists(cooldownFile))
            {
                string[] lines = File.ReadAllLines(cooldownFile);
                if (lines.Length >= 2)
                {
                    if (DateTime.TryParse(lines[0], out DateTime petTime))
                    {
                        PetCooldownStartTime = petTime;
                    }
                    if (DateTime.TryParse(lines[1], out DateTime sleepTime))
                    {
                        SleepCooldownStartTime = sleepTime;
                    }
                }
            }
        }

        // Stat Management Methods
        public int GetStat(string key) => StatDict.TryGetValue(key, out int value) ? value : 0;

        public async Task UpdateStat(string key, int amount)
        {
            if (!StatDict.ContainsKey(key)) StatDict[key] = 0;
            int currentStat = StatDict[key];
            int newStat = Math.Clamp(currentStat + amount, 0, MaxStatValue);
            StatDict[key] = newStat;
            OnStatChanged(key, newStat);
            WriteStatsToFile();
        }

        private void OnStatChanged(string key, int newValue)
        {
            StatChanged?.Invoke(key, newValue);
        }

        private void SetStatsFromFile()
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

        private void WriteStatsToFile()
        {
            using (StreamWriter sw = new StreamWriter(StatsFile))
            {
                foreach (var kvp in StatDict)
                {
                    sw.WriteLine($"{kvp.Key}:{kvp.Value}");
                }
            }
        }

        private void CreateStatsFile()
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
