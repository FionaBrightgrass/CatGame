using System;
using System.IO;
using System.Windows.Threading;
using PocketCat.Controls;
using PocketCat.StaticGame.Stats;

namespace PocketCat.StaticGame
{
    public static class Time
    {
        public static AppSettings _appSettings;
        private static readonly Random _random = new Random(); // Random generator for 50% chance
        private static DispatcherTimer _hungerTimer; // Timer for hunger management
        public static event Action<int> HungerLevelChanged; // Event to notify when hunger level changes
        public static DateTime? PetCooldownStartTime { get; set; }
        public static TimeSpan PetCooldownDuration { get; set; }

        public static DateTime? SleepCooldownStartTime { get; set; }
        public static readonly TimeSpan SleepCooldownDuration = TimeSpan.FromHours(1);
        public static int SleepIncrement { get; set; } = 30; // Time interval for restoring energy (ms)
        public static MainWindow? _mainWindow { get;  set; }
        private static CancellationTokenSource _timerCancellationTokenSource = new CancellationTokenSource();
        static Time()
        {
            StartHungerTimer(); // Keep your existing hunger timer
            StartHungerTimer();
            LoadCooldownsFromFile();
            PetCooldownDuration = TimeSpan.FromMinutes(_appSettings?.SleepInterval ?? 15);
        }

        private static void StartHungerTimer()
        {
            // Create and configure the DispatcherTimer
            _hungerTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(_appSettings?.SleepInterval ?? 5) // Trigger every 15 minutes
            };
            _hungerTimer.Tick += HungerTimer_Tick;
            _hungerTimer.Start();
        }

        private static void HungerTimer_Tick(object sender, EventArgs e)
        {
            int hungerLevel = StatSheet.GetStat("Hunger").Value;
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
                    Flags.IsSleeping = false; // Wake up the pet if hunger level is at minimum
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
            Flags.WantsPets = false;
            await Task.Delay(PetCooldownDuration);

            PetCooldownStartTime = null;
            SaveCooldownsToFile();
            Flags.WantsPets = true;
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
            Flags.IsSleeping = true;

            await Task.Delay(SleepCooldownDuration);

            Flags.IsSleeping = false;
            SleepCooldownStartTime = null;
            SaveCooldownsToFile();
        }
        public static async Task StartSleepWithIncrementsAsync(int energyNeeded, Statbar energyBar)
        {
            // Exit if no energy needed or already sleeping
            if (energyNeeded <= 0 || Flags.IsSleeping) return;

            // Set sleeping state and record cooldown
            Flags.IsSleeping = true;
            SleepCooldownStartTime = DateTime.Now;
            SaveCooldownsToFile();

            try
            {
                // Gradually restore energy
                while (energyNeeded > 0)
                {
                    int currentEnergy = StatSheet.GetStat("Energy").Value;

                    // Only restore energy if it hasn't reached max value
                    if (currentEnergy < StatSheet.MaxValue)
                    {
                        Console.WriteLine($"Starting blinking for energy bar index {currentEnergy}...");

                        // Update blinking states in the Statbar
                        energyBar.UpdateBlinkStates(currentEnergy, currentEnergy, 1); // Set state to +1 (increasing)

                        try
                        {
                            // Trigger blinking and wait for it to complete
                            await energyBar.HandleBlinkingAsync(currentEnergy, currentEnergy + 1, false); // Incrementing=false
                            Console.WriteLine($"Incrementing energy stat for bar index {currentEnergy}...");

                            // Increment the energy stat
                            await StatSheet.UpdateStat("Energy", 1);
                            energyNeeded--; // Decrease remaining energy to restore
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error during blinking or stat update for bar index {currentEnergy}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Max energy reached. Stopping energy restoration.");
                        break;
                    }

                    // Wait for the sleep increment interval before continuing
                    await Task.Delay(SleepIncrement);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartSleepWithIncrementsAsync: {ex.Message}");
            }
            finally
            {
                Flags.IsSleeping = false;
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

        private static DispatcherTimer _tenMinuteTimer;

        // Method to start the timer
        
        public static async Task RunContinuousTimerAsync()
        {
            await Task.Delay(TimeSpan.FromMinutes(_appSettings.SleepInterval), _timerCancellationTokenSource.Token);
            while (!_timerCancellationTokenSource.IsCancellationRequested) // Loop until cancellation is requested
            {
                try
                {
                    await HandleTenMinuteTickAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in HandleTenMinuteTickAsync: {ex.Message}");
                }

                // Wait for 10 minutes unless canceled
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_appSettings.SleepInterval), _timerCancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    // Graceful exit when cancellation is triggered
                    break;
                }
            }
        }
        private static async Task HandleTenMinuteTickAsync()
        {
            if(_mainWindow == null || States.CurrentState == "Sleeping")
            {
                return;
            }
            int hungerIncrement = 0;
            var blinkTasks = new List<Task>();
            // Decrement hunger based on probabilities
            double randomValue = _random.NextDouble();
            if (randomValue <= 0.25)
            {
                hungerIncrement -= 0;
            }
            else if (randomValue <= 0.8)
            {
                hungerIncrement -= 1;
            }
            else if (randomValue <= 1.0)
            {
                hungerIncrement -= 2;
            }

            hungerIncrement = Math.Max(hungerIncrement, 0);

            HungerLevelChanged?.Invoke(hungerIncrement);
            Console.WriteLine($"Hunger updated by: {hungerIncrement}");
            int finalHunger = 0;
            int finalAffection = 0;
            int finalEnergy = 0;
            // Reduce affection if hunger is under 5
            if (_random.NextDouble() <= 0.5)
            {
                int startIndex = 0;
                int endIndex = 0;
                int hungerLevel = StatSheet.GetStat("Hunger").Value;
                if (hungerLevel > 0 && _random.NextDouble() <= 0.5)
                {
                    finalHunger = hungerIncrement;
                    startIndex = Math.Max(hungerLevel - hungerIncrement, 0); // Starting point after decrement
                    endIndex = hungerLevel - hungerIncrement; // Ending point of affected bars
                    if (startIndex <= endIndex) // Ensure valid range
                    {
                        _mainWindow.HungerBar.UpdateBlinkStates(startIndex, endIndex, -1); // State = -1 for decrement
                        blinkTasks.Add(_mainWindow.HungerBar.HandleBlinkingAsync(startIndex, endIndex, true)); // isDecrementing = true
                    }
                }

                int affectionLevel = StatSheet.GetStat("Affection").Value;
                if (affectionLevel > 0 && _random.NextDouble() <= 0.5 && (StatSheet.GetStat("Energy").Value == 0 || StatSheet.GetStat("Hunger").Value == 0))
                {
                    startIndex = Math.Max(affectionLevel - 1, 0); // Starting point after decrement
                    endIndex = affectionLevel - 1; // Ending point of affected bars
                    if (startIndex <= endIndex) // Ensure valid range
                    {
                        _mainWindow.AffectionBar.UpdateBlinkStates(startIndex, endIndex, -1); // State = -1 for decrement
                        blinkTasks.Add(_mainWindow.AffectionBar.HandleBlinkingAsync(startIndex, endIndex, true)); // isDecrementing = true
                    }
                    affectionLevel -= 1;
                    finalAffection = -1;
                    Console.WriteLine($"Affection decreased: {affectionLevel}");
                }

            }

            // 25% chance to decrement energy
            if (_random.NextDouble() <= 0.75)
            {
                int energyLevel = StatSheet.GetStat("Energy").Value;

                if (energyLevel > 0)
                {

                    int currentEnergy = StatSheet.GetStat("Energy").Value;
                    int startIndex = Math.Max(currentEnergy - 1, 0); // Starting point after decrement
                    int endIndex = currentEnergy - 1; // Ending point of affected bars
                    if (startIndex <= endIndex) // Ensure valid range
                    {
                        _mainWindow.EnergyBar.UpdateBlinkStates(startIndex, endIndex, -1); // State = -1 for decrement
                        blinkTasks.Add(_mainWindow.EnergyBar.HandleBlinkingAsync(startIndex, endIndex, true)); // isDecrementing = true
                    }
                    energyLevel -= 1;

                    Console.WriteLine($"Energy decreased: {energyLevel}");
                    finalEnergy = -1;
                }
            }
            // Wait for all blinking tasks to complete
            await Task.WhenAll(blinkTasks);
            StatSheet.UpdateStat("Energy", finalEnergy);
            StatSheet.UpdateStat("Affection", finalAffection);
            StatSheet.UpdateStat("Hunger", finalHunger); // Update asynchronously
        }
    }
}
