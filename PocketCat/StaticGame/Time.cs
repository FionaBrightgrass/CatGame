using System;
using System.IO;
using System.Windows.Threading;
using PocketCat.Controls;

namespace PocketCat.StaticGame
{
    public static class Time
    {
        public static AppSettings _AppSettings;
        private static readonly Random _random = new Random(); // Random generator for 50% chance
        private static DispatcherTimer _hungerTimer; // Timer for hunger management
        public static event Action<int> HungerLevelChanged; // Event to notify when hunger level changes
        public static DateTime? PetCooldownStartTime { get; set; }
        public static TimeSpan PetCooldownDuration { get; set; }

        public static DateTime? SleepCooldownStartTime { get; set; }
        public static readonly TimeSpan SleepCooldownDuration = TimeSpan.FromHours(1);
        public static int SleepIncrement { get; set; } = 30; // Time interval for restoring energy (ms)

        static Time()
        {
            StartHungerTimer();
            LoadCooldownsFromFile();
            PetCooldownDuration = TimeSpan.FromMinutes(_AppSettings?.SleepInterval ?? 15);
        }

        private static void StartHungerTimer()
        {
            // Create and configure the DispatcherTimer
            _hungerTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(_AppSettings?.SleepInterval ?? 5) // Trigger every 15 minutes
            };
            _hungerTimer.Tick += HungerTimer_Tick;
            _hungerTimer.Start();
        }

        private static void HungerTimer_Tick(object sender, EventArgs e)
        {
            int hungerLevel = Stats.GetStat("Hunger");
            // 50% chance to reduce hunger level
            if (_random.NextDouble() <= 0.5)
            {
                if (hungerLevel > 0) // Prevent hunger level from going below zero
                {
                    hungerLevel--;
                    HungerLevelChanged?.Invoke(hungerLevel); // Notify subscribers
                    Console.WriteLine($"Hunger level decreased! Current hunger: {hungerLevel}");
                }
                else
                {
                    States.IsSleeping = false; // Wake up the pet if hunger level is at minimum
                    Console.WriteLine("Hunger level is already at minimum!");
                }
            }
            else
            {
                Console.WriteLine("No hunger level change this time.");
            }
        }


        public static bool IsPetOnCooldown()
        {
            if (!PetCooldownStartTime.HasValue) return false;
            return DateTime.Now < PetCooldownStartTime.Value.Add(PetCooldownDuration);
        }

        public static async Task StartPetCooldownAsync()
        {
            PetCooldownStartTime = DateTime.Now;
            SaveCooldownsToFile();
            States.WantsPets = false;
            await Task.Delay(PetCooldownDuration);

            PetCooldownStartTime = null;
            SaveCooldownsToFile();
            States.WantsPets = true;
        }

        // Cooldown Management for Sleep
        public static bool IsSleepOnCooldown()
        {
            if (!SleepCooldownStartTime.HasValue) return false;
            return DateTime.Now < SleepCooldownStartTime.Value.Add(SleepCooldownDuration);
        }

        public static async Task StartSleepCooldownAsync()
        {
            SleepCooldownStartTime = DateTime.Now;
            SaveCooldownsToFile();
            States.IsSleeping = true;

            await Task.Delay(SleepCooldownDuration);

            States.IsSleeping = false;
            SleepCooldownStartTime = null;
            SaveCooldownsToFile();
        }

        public static async Task StartSleepWithIncrementsAsync(int energyNeeded, Statbar energyBar)
        {
            if (energyNeeded <= 0 || States.IsSleeping) return; // Exit if no energy needed or already sleeping

            States.IsSleeping = true; // Set the sleeping state
            SleepCooldownStartTime = DateTime.Now; // Record cooldown start time
            SaveCooldownsToFile();

            try
            {
                // Gradually restore energy
                while (energyNeeded > 0)
                {
                    int currentEnergy = Stats.GetStat("Energy");

                    if (currentEnergy < Stats.MaxStatValue)
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
                            await Stats.UpdateStat("Energy", 1);
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
                States.IsSleeping = false; // Reset the sleeping state
                SaveCooldownsToFile();
            }
        }

        // Save/Load Cooldowns to/from File
        private static void SaveCooldownsToFile()
        {
            using (StreamWriter sw = new StreamWriter(Globals.AppWide.ApplicationData + "/cooldowns.txt"))
            {
                sw.WriteLine(PetCooldownStartTime.HasValue ? PetCooldownStartTime.Value.ToString("o") : string.Empty);
                sw.WriteLine(SleepCooldownStartTime.HasValue ? SleepCooldownStartTime.Value.ToString("o") : string.Empty);
            }
        }

        private static void LoadCooldownsFromFile()
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


    }
}
