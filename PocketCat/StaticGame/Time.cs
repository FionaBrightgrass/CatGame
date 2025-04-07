using System;
using System.IO;
using System.Timers;
using System.Windows.Threading;
using PocketCat.Controls;
using PocketCat.StaticGame.Items;
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
            if (_random.NextDouble() <= 1)
            {
                if (hungerLevel > 0) // Prevent hunger level from going below zero
                {
                    //hungerLevel--;
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
                sw.Close();
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

        // Method to start the timer
        
        public static async Task RunContinuousTimerAsync()
        {
            await Task.Delay(TimeSpan.FromMinutes(_appSettings.TimeInterval), _timerCancellationTokenSource.Token);
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
                    await Task.Delay(TimeSpan.FromMinutes(_appSettings.TimeInterval), _timerCancellationTokenSource.Token);
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
            if (_mainWindow == null || States.CurrentState == "Sleeping" || Flags.IsSleeping)
            {
                Console.WriteLine("Skipping timer tick while sleeping or MainWindow is null.");
                return;
            }
            int hungerIncrement = 0;
            // Decrement hunger based on probabilities
            double randomValue = _random.NextDouble();
            hungerIncrement = Math.Max(hungerIncrement, 0);
            HungerLevelChanged?.Invoke(hungerIncrement);
            Console.WriteLine($"Hunger updated by: {hungerIncrement}");
            int finalHunger = -1;
            int finalHappiness = 0;
            int finalEnergy = -1;
            // Reduce affection if hunger is under 5
            if (StatSheet.GetStat("Hunger").Value < 1)
                finalHappiness = -1;
            int happinessLevel = StatSheet.GetStat("Happiness").Value;
            if (happinessLevel > 0 && StatSheet.GetStat("Hunger").Value == 0)
            {
                finalHappiness = -1;
                Console.WriteLine($"Happiness decreasing: -1");
            }
            else if (happinessLevel == 0)
            {
                _mainWindow.Affection.AddAffectionValue(-1);
            }
            // Wait for all blinking tasks to complete
            List<Task> blinkTasks = _mainWindow.HandleAllBlinkTasks(finalEnergy, finalHappiness, finalEnergy);
            await BlinkBarReady();
            await Task.WhenAll(blinkTasks);
            StatSheet.UpdateStat("Energy", finalEnergy);
            StatSheet.UpdateStat("Happiness", finalHappiness);
            StatSheet.UpdateStat("Hunger", finalHunger); // Update asynchronously
            _mainWindow.Flags.BlinkLock = false;
            _mainWindow.Flags.ButtonLock = false;
            await _mainWindow.RefreshBars();
        }

        public static async Task<bool> BlinkBarReady()
        {
            if (!_mainWindow.Flags.BlinkLock)
                return true;
            int i = 0;
            while (_mainWindow.Flags.BlinkLock && i < 5)
            {
                await Task.Delay(1000);
                i++;
            }
            return !_mainWindow.Flags.BlinkLock;
        }

        private static System.Timers.Timer _affectionReadyTimer;

        public static void StartAffectionReadyTimer()
        {
            // Create a timer with a 30-minute interval (in milliseconds).
            _affectionReadyTimer = new System.Timers.Timer(_appSettings.TimeInterval * 60 * 10);
            _affectionReadyTimer.Elapsed += OnAffectionReadyTimerElapsed;
            _affectionReadyTimer.AutoReset = true; // Ensures the timer repeats.
            _affectionReadyTimer.Enabled = true; // Starts the timer.
        }

        private static void OnAffectionReadyTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Flip the flag to true.
            Flags.AffectionReady = true;
        }

        public static void StopAffectionReadyTimer()
        {
            _affectionReadyTimer?.Stop();
            _affectionReadyTimer?.Dispose();
        }

        private static System.Timers.Timer _tipTimer;

        public static void StartTipReadyTimer()
        {
            // Create a timer with a 30-minute interval (in milliseconds).
            _tipTimer = new System.Timers.Timer(_appSettings.TimeInterval * 60 * 1000);
            _tipTimer.Elapsed += OnAffectionReadyTimerElapsed;
            _tipTimer.AutoReset = true; // Ensures the timer repeats.
            _tipTimer.Enabled = true; // Starts the timer.
        }

        private static void OnTipReadyTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Flip the flag to true.
            Flags.TipReady = true;
        }

        public static void StopTipReadyTimer()
        {
            _tipTimer?.Stop();
            _tipTimer?.Dispose();
        }



        private static System.Timers.Timer _statDecreaseTimer;

        public static void StartStatDecreaseTimer()
        {
            // Create a timer with a 30-minute interval (in milliseconds).
            _statDecreaseTimer = new System.Timers.Timer(10 * 60 * 1000);
            _statDecreaseTimer.Elapsed += OnStatDecreaseElapsed;
            _statDecreaseTimer.AutoReset = true; // Ensures the timer repeats.
            _statDecreaseTimer.Enabled = true; // Starts the timer.
        }

        private static void OnStatDecreaseElapsed(object sender, ElapsedEventArgs e)
        {
            // Flip the flag to true.
            Flags.StatDecreaseReady = true;
        }

        public static void StopStatDecreaseTimer()
        {
            _statDecreaseTimer?.Stop();
            _statDecreaseTimer?.Dispose();
        }

        private static System.Timers.Timer _dailyCheckTimer;
        private static Money _moneyInstance;

        // Initialize Time tracking
        public static void InitializeMoney(Money money)
        {
            _moneyInstance = money;
            // Set up the timer to check every minute (can adjust as needed)
            _dailyCheckTimer = new System.Timers.Timer(60000); // 10 minute interval
            _dailyCheckTimer.Elapsed += DailyCheck;
            _dailyCheckTimer.AutoReset = true;
            _dailyCheckTimer.Start();

            Console.WriteLine("Time tracker initialized. Watching for day changes...");
        }

        // Timer's Elapsed event handler
        private static void DailyCheck(object sender, ElapsedEventArgs e)
        {
            DateTime today = DateTime.Now.Date;
            if (today != _moneyInstance.LastPayDay)
            {
                Console.WriteLine($"Day has changed to {today:yyyy-MM-dd}. Running daily payout...");

                // Trigger Money's daily payout
                _moneyInstance.CheckAndRunDailyPayout();
            }
        }

        // Clean up resources
        public static void Stop()
        {
            _dailyCheckTimer?.Stop();
            _dailyCheckTimer?.Dispose();
            _statDecreaseTimer?.Stop();
            _statDecreaseTimer?.Dispose();
            _hungerTimer.Stop();
            Console.WriteLine("Time tracker stopped.");
        }
    }
}
