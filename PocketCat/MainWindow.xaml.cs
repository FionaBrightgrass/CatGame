using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using PocketCat.StaticGame;
using PocketCat.StaticGame.Stats;
using PocketCat.Controls;
using System;
using PocketCat.DefaultData;
using System.ComponentModel;

namespace PocketCat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppSettings _appSettings = new AppSettings();
        private SettingsWindow settingsWindow;
        private int storyIndex = 0;
        private int lastRandom = -1;

        public MainWindow()
        {
            InitializeComponent();

            // Configure StatSheet (no need for instantiation since it's static)
            StatSheet.SubscribeToAllStatChanges();
            StatSheet.ConfigureBlinkBars();

            // Initialize other components
            Time._AppSettings = _appSettings;
            Time._mainWindow = this;
            Time.HungerLevelChanged += OnSleepingHungerLevelChanged;
            States.OnStateChanged += HandleStateChanged;

            // Initialize the Settings window
            settingsWindow = new SettingsWindow(_appSettings);
            settingsWindow.Visibility = Visibility.Hidden;

            // Assign BlinkBarCallback dynamically within each Stat
            foreach (var statKey in new[] { "Hunger", "Affection", "Energy" })
            {
                Stat stat = StatSheet.GetStat(statKey);
                stat.BlinkBarCallback = async (key, value, isIncrement, isMaxed) =>
                {
                    int startIndex = Math.Max(value + (isIncrement ? 0 : -1), 0);
                    int endIndex = Math.Max(value, 0); // Ensure valid range

                    switch (key)
                    {
                        case "Hunger":
                            if (stat.Value <= 3)
                                States.CurrentState = "Hungry";
                            //HungerBar.UpdateBlinkStates(startIndex, endIndex, isIncrement ? 1 : -1);
                            //await HungerBar.HandleBlinkingAsync(startIndex, endIndex, !isIncrement); // Corrected method call
                            break;

                        case "Affection":
                            //AffectionBar.UpdateBlinkStates(startIndex, endIndex, isIncrement ? 1 : -1);
                            //await AffectionBar.HandleBlinkingAsync(startIndex, endIndex, !isIncrement); // Corrected method call
                            break;

                        case "Energy":
                            if (stat.Value <= 3)
                                States.CurrentState = "Tired";
                            //EnergyBar.UpdateBlinkStates(startIndex, endIndex, isIncrement ? 1 : -1);
                            //await EnergyBar.HandleBlinkingAsync(startIndex, endIndex, !isIncrement); // Corrected method call
                            break;

                        default:
                            Console.WriteLine($"Unhandled Stat key: {key}");
                            break;
                    }
                };
            }



            // Hide submenus and show the main control
            FeedControl.Visibility = Visibility.Hidden;
            PlayControl.Visibility = Visibility.Hidden;
            ButtonControl.Visibility = Visibility.Visible;

            // Associate stat bars with the main window
            AffectionBar.ParentWindow = this;
            HungerBar.ParentWindow = this;
            EnergyBar.ParentWindow = this;
            RefreshBars();
        }

        public void RefreshBars()
        {
            // Use the Stats instance to retrieve and update bars
            AffectionBar.UpdateBar(StatSheet.GetStat("Affection").Value); // Update AffectionBar
            HungerBar.UpdateBar(StatSheet.GetStat("Hunger").Value);       // Update HungerBar
            EnergyBar.UpdateBar(StatSheet.GetStat("Energy").Value);       // Update EnergyBar
            UpdateLayout();                                           // Ensure the layout is updated
        }

        private async void OnSleepingHungerLevelChanged(int newHungerLevel)
        {
            if (!States.IsSleeping || States.CurrentState != "Sleeping")
                return;
            Console.WriteLine("Begining OnSleepHungerLevelChanged");
            // Prepare tasks for blinking
            var blinkTasks = new List<Task>();

            // Retrieve the current Hunger value and calculate the range
            int currentHunger = StatSheet.GetStat("Hunger").Value;
            int endHunger = Math.Max(currentHunger - 1, 0); // Ensure valid range

            // Update HungerBar with decrement
            if (currentHunger > endHunger)
            {
                HungerBar.UpdateBlinkStates(endHunger, currentHunger, -1); // Decrement state = -1
                blinkTasks.Add(HungerBar.HandleBlinkingAsync(endHunger, currentHunger, true)); // Adjusted to pass range and decrement flag
            }

            // Check if the pet is sleeping and display appropriate message
            string actionText = States.IsSleeping
                ? $"#NAME#'s belly rumbles quietly."
                : $"#NAME# wakes up and meows for food.";

            await ActionText.StartTextWritingAsync(actionText, 25);

            // Wait for all blinking tasks to complete
            await Task.WhenAll(blinkTasks);

            // Update Hunger stat and refresh the UI
            StatSheet.UpdateStat("Hunger", -1); // Update the Hunger stat
            RefreshBars(); // Refresh UI elements
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove(); // Allows the user to drag the window
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            settingsWindow.Close();
            this.Close();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            // Get the position and dimensions of the MainWindow
            var mainWindowLeft = this.Left; // X coordinate of MainWindow
            var mainWindowTop = this.Top;   // Y coordinate of MainWindow
            var mainWindowWidth = this.ActualWidth; // Width of MainWindow

            // Set the position for the SettingsWindow to the right of MainWindow
            settingsWindow.Left = mainWindowLeft + mainWindowWidth; // Position it to the right
            settingsWindow.Top = mainWindowTop; // Align the top of both windows
            if (settingsWindow.Visibility == Visibility.Visible)
            {
                settingsWindow.Visibility = Visibility.Hidden;
            }
            else
            {
                settingsWindow.Visibility = Visibility.Visible;
                settingsWindow.Show();
            }
        }

        private void ReturnMain(object sender, RoutedEventArgs e)
        {
            ButtonControl.Visibility = Visibility.Visible;
            FeedControl.Visibility = Visibility.Hidden;
            PlayControl.Visibility = Visibility.Hidden;
        }

        private void UpdateControlVisibility(UIElement visibleControl)
        {
            ButtonControl.Visibility = Visibility.Collapsed;
            FeedControl.Visibility = Visibility.Collapsed;
            PlayControl.Visibility = Visibility.Collapsed;
            SleepControl.Visibility = Visibility.Collapsed;

            visibleControl.Visibility = Visibility.Visible;
        }
        private async void MainButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle sleeping pet scenario
            if (States.IsSleeping)
            {
                TimeSpan remainingTime = Time.SleepCooldownStartTime.Value.Add(Time.PetCooldownDuration) - DateTime.Now;

                if (remainingTime.Minutes < 1)
                {
                    await ActionText.StartTextWritingAsync($"#NAME# looks like #PRONOUNONE# is on the verge of waking up!", 25);
                }
                else if (remainingTime.Minutes <= 10)
                {
                    await ActionText.StartTextWritingAsync($"#NAME# is sleeping softly.", 25);
                }
                else
                {
                    await ActionText.StartTextWritingAsync($"#NAME# is sound asleep.", 25);
                }

                ReturnMain(sender, e); // Navigate back to main menu
                return;
            }

            // Handle button logic based on the DataContext
            if (sender is Button button && button.DataContext is KeyValuePair<string, string> mainData)
            {
                string key = mainData.Key;

                // Handle control visibility dynamically
                switch (key)
                {
                    case "Feed":
                        UpdateControlVisibility(FeedControl);
                        break;

                    case "Play":
                        UpdateControlVisibility(PlayControl);
                        break;

                    case "Pet":
                        PetButton_Click(sender, e); // Call the Pet logic
                        break;

                    case "Sleep":
                        UpdateControlVisibility(SleepControl);
                        break;

                    default:
                        Console.WriteLine($"Unhandled button: {key}");
                        break;
                }
            }
        }

        private bool TryParseActionData(string data, out int hunger, out int affection, out int energy, out string actionText)
        {
            hunger = 0;
            affection = 0;
            energy = 0;
            actionText = string.Empty;

            try
            {
                string[] parts = data.Split(';');
                foreach (var part in parts)
                {
                    string[] keyValue = part.Split(':');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim();
                        string value = keyValue[1].Trim();

                        if (key == "Hunger") hunger = int.Parse(value);
                        else if (key == "Affection") affection = int.Parse(value);
                        else if (key == "Energy") energy = int.Parse(value);
                    }
                }
                // Handle parts that do not have a key-value format (ActionText)
                actionText += parts.Last().Trim() + " ";
                Console.WriteLine(actionText);
                actionText = actionText.Trim();
                return true;
            }
            catch
            {
                return false; // Parsing failed
            }
        }


        private async Task PerformAction(string actionText, int hungerValue, int affectionValue, int energyValue)
        {
            try
            {
                // Log the changes for debugging
                Console.WriteLine($"Performing action: {actionText} | Hunger: {hungerValue}, Affection: {affectionValue}, Energy: {energyValue}");

                // Update stats using the StatSheet object
                //StatSheet.UpdateStat("Hunger", hungerValue);
                //StatSheet.UpdateStat("Affection", affectionValue);
                //StatSheet.UpdateStat("Energy", energyValue);

                // Display feedback text with animation
                await ActionText.StartTextWritingAsync(actionText, 25);

                // Refresh UI after updating stats
                RefreshBars();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error performing action: {ex.Message}");
            }
            finally
            {
            }
        }


        private bool HandleBackButton(string buttonName, RoutedEventArgs e)
        {
            if (string.Equals(buttonName, "Back", StringComparison.OrdinalIgnoreCase))
            {
                ReturnMain(null, e);
                return true;
            }
            return false;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Handle sleeping pet scenario asynchronously
                if (States.IsSleeping)
                {
                    ReturnMain(sender, e);
                    await ActionText.StartTextWritingAsync("Best not to wake #NAME#.", 25);
                    return;
                }

                if (sender is Button button && button.DataContext is KeyValuePair<string, string> playData)
                {
                    // Handle "Back" button functionality
                    if (HandleBackButton(playData.Key))
                    {
                        ReturnMain(sender, e);
                        return;
                    }

                    // Parse action data
                    if (TryParseActionData(playData.Value, out int hungerValue, out int affectionValue, out int energyValue, out string actionText))
                    {
                        var blinkTasks = new List<Task>();
                        States.CurrentState = playData.Key;

                        // Handle HungerBar blinking
                        if (hungerValue < 0) // Hunger decreases
                        {
                            int currentHunger = StatSheet.GetStat("Hunger").Value;
                            int startIndex = Math.Max(currentHunger + hungerValue, 0); // Starting point after decrement
                            int endIndex = currentHunger - 1; // Ending point of affected bars
                            if (startIndex <= endIndex) // Ensure valid range
                            {
                                HungerBar.UpdateBlinkStates(startIndex, endIndex, -1); // State = -1 for decrement
                                blinkTasks.Add(HungerBar.HandleBlinkingAsync(startIndex, endIndex, true)); // Trigger blinking
                            }
                        }

                        // Handle AffectionBar blinking
                        if (affectionValue != 0) // Affection changes (increase or decrease)
                        {
                            int currentAffection = StatSheet.GetStat("Affection").Value;
                            int startIndex = affectionValue > 0
                                ? currentAffection
                                : Math.Max(currentAffection + affectionValue, 0); // Starting point depends on the change
                            int endIndex = affectionValue > 0
                                ? Math.Min(currentAffection + affectionValue - 1, AffectionBar.BlinkStateCount - 1)
                                : currentAffection - 1; // Ending point depends on increase or decrease

                            AffectionBar.UpdateBlinkStates(startIndex, endIndex, affectionValue > 0 ? 1 : -1); // Update blink state
                            blinkTasks.Add(AffectionBar.HandleBlinkingAsync(startIndex, endIndex, affectionValue < 0)); // Trigger blinking
                        }

                        // Handle EnergyBar blinking
                        // Handle EnergyBar blinking for both increases and decreases
                        if (energyValue != 0) // Energy changes
                        {
                            int currentEnergy = StatSheet.GetStat("Energy").Value;

                            if (energyValue > 0) // Energy increases
                            {
                                int startIndex = currentEnergy; // Starting point of increment
                                int endIndex = Math.Min(currentEnergy + energyValue - 1, EnergyBar.BlinkStateCount - 1); // Clamp to valid range
                                if (startIndex <= endIndex) // Ensure valid range
                                {
                                    EnergyBar.UpdateBlinkStates(startIndex, endIndex, 1); // State = 1 for increment
                                    blinkTasks.Add(EnergyBar.HandleBlinkingAsync(startIndex, endIndex, false)); // isDecrementing = false
                                }
                            }
                            else if (energyValue < 0) // Energy decreases
                            {
                                int decrementAmount = Math.Abs(energyValue); // Get absolute value for decrement
                                int startIndex = Math.Max(currentEnergy - decrementAmount, 0); // Starting point after decrement
                                int endIndex = currentEnergy - 1; // Ending point of affected bars
                                if (startIndex <= endIndex) // Ensure valid range
                                {
                                    EnergyBar.UpdateBlinkStates(startIndex, endIndex, -1); // State = -1 for decrement
                                    blinkTasks.Add(EnergyBar.HandleBlinkingAsync(startIndex, endIndex, true)); // isDecrementing = true
                                }
                            }
                        }


                        // Display action feedback text
                        await ActionText.StartTextWritingAsync(actionText, 25);

                        // Wait for all blinking tasks to complete
                        await Task.WhenAll(blinkTasks);

                        // Update stats dynamically
                        StatSheet.UpdateStat("Hunger", hungerValue);
                        StatSheet.UpdateStat("Affection", affectionValue);
                        StatSheet.UpdateStat("Energy", energyValue);
                        if (StatSheet.GetStat("Energy").Value <= 3)
                        {
                            States.CurrentState = "Tired";
                        }
                        else
                        {
                            // Reset state to Idle after 10 seconds
                            //SetStateToIdleAfterDelayAsync(playData.Key, 10);
                        }
                    }
                }
            }
            finally
            {
                RefreshBars();
                ReturnMain(sender, e);
            }
        }

        private async void PetButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the pet is on cooldown
            if (!States.WantsPets)
            {
                TimeSpan remainingTime;
                if (Time.PetCooldownStartTime == null)
                {
                    Time.PetCooldownStartTime = DateTime.Now;
                    remainingTime = Time.PetCooldownStartTime.Value.Add(Time.PetCooldownDuration) - DateTime.Now;
                }
                else
                {
                    remainingTime = Time.PetCooldownStartTime.Value.Add(Time.PetCooldownDuration) - DateTime.Now;
                }

                // Calculate the percentage of cooldown completion
                int passedMilliseconds = (int)(DateTime.Now - Time.PetCooldownStartTime.Value).TotalMilliseconds;
                Console.WriteLine($"Total time passed since last pet: {passedMilliseconds}ms");

                float ratio = 100; // Default to complete
                if (passedMilliseconds > 0 && remainingTime.TotalMilliseconds > 0)
                {
                    float diff = (float)passedMilliseconds / (passedMilliseconds + (float)remainingTime.TotalMilliseconds);
                    ratio = diff * 100; // Scale to percentage
                }
                else
                {
                    ratio = 0; // Default for unexpected values
                }

                Console.WriteLine($"{ratio}% cooldown complete.");

                // Handle different cooldown states
                int maxAffection = StatSheet.GetStat("Affection").Value;
                if (ratio < 20)
                {
                    string actionText = ActionDataService.GetRandomItem(ActionData.Extra, maxAffection);
                    await ActionText.StartTextWritingAsync(actionText, 25);
                    return;
                }
                else if (ratio < 60)
                {
                    string actionText = ActionDataService.GetRandomItem(ActionData.ExtraBored, maxAffection);
                    await ActionText.StartTextWritingAsync(actionText, 25);
                    return;
                }
                else if (ratio <= 100)
                {
                    string actionText = ActionDataService.GetRandomItem(ActionData.ExtraAnnoyed, maxAffection);
                    await ActionText.StartTextWritingAsync(actionText, 25);
                    return;
                }
                else
                {
                    States.WantsPets = true;
                }
            }

            // Start the pet cooldown
            Time.StartPetCooldownAsync();

            // Fetch a random petting story
            string story = ActionDataService.GetRandomItem(ActionData.PetRandom);

            // Calculate stat changes
            int affectionGain = 0 % 3; // Stat change for Affection
            int hungerChange = 0;      // No hunger change for petting
            int energyChange = -1 * (1 % 3); // Energy cost for petting

            // Perform the main action and update stats
            await PerformAction(story, hungerChange, affectionGain, energyChange);

            // Check if extra data is available
            if (DefaultData.ActionData.Extra == null || DefaultData.ActionData.Extra.Count == 0)
            {
                Console.WriteLine("Extra story data is missing or empty.");
                return;
            }

            // Optionally display an extra story (30% chance)
            if (GenerateIndex(10) < 3)
            {
                await Task.Delay(2000);
                string extraStory = ActionDataService.GetRandomItem(ActionData.PetRandom);
                await PerformAction(extraStory, 0, 0, 0); // No stat change for extra story

                // Cycle through the extra story index
                storyIndex++;
                if (storyIndex >= DefaultData.ActionData.Extra.Count)
                {
                    storyIndex = 0;
                }
            }

            // Refresh the visual bars
            RefreshBars();
        }

        private int GenerateIndex(int randomCount)
        {
            Random random = new Random();
            int num = random.Next(0, randomCount);

            // Reduce the chance to repeat the same message
            if (lastRandom == num)
            {
                num = random.Next(0, randomCount);
            }
            lastRandom = num;
            return num;
        }

        private async void SleepButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Safely extract button name
                string buttonName = sender is Button button ? button.Content?.ToString().Trim() ?? string.Empty : string.Empty;

                // Handle the "Back" button scenario
                if (HandleBackButton(buttonName, e))
                {
                    ReturnMain(sender, e);
                    return;
                }

                // Process sleep action logic if button corresponds to a valid sleep action
                if (DefaultData.ActionData.DataModel.SleepControl.TryGetValue(buttonName, out string sleepData))
                {
                    if (TryParseActionData(sleepData, out int hungerValue, out int affectionValue, out int energyValue, out string actionText))
                    {
                        // Set the pet to sleeping state
                        States.IsSleeping = true;
                        States.CurrentState = "Sleeping";

                        // Perform initial action
                        await PerformAction(actionText, hungerValue, affectionValue, 0);
                        Console.WriteLine("The pet is now sleeping.");

                        // Start cooldown and calculate sleep duration
                        Time.SleepCooldownStartTime = DateTime.Now;
                        int currentEnergy = StatSheet.GetStat("Energy").Value;
                        int energyToRestore = Stat.MaxValue - currentEnergy; // Number of energy units to restore
                        int sleepDuration = _appSettings.SleepInterval * energyToRestore;
                        DateTime sleepEndTime = Time.SleepCooldownStartTime.Value.AddMinutes(sleepDuration);

                        // Restore energy while pet is sleeping
                        while (currentEnergy < Stat.MaxValue)
                        {
                            await EnergyBar.BlinkContinuouslyAsync(currentEnergy, _appSettings.SleepInterval); // Blink bar
                            EnergyBar.StopBlinking(currentEnergy); // Stop blinking
                            StatSheet.UpdateStat("Energy", 1); // Update stat dynamically
                            currentEnergy++;
                            Console.WriteLine($"Energy restored: {currentEnergy}/{Stat.MaxValue}");
                        }
                        int currentHunger = StatSheet.GetStat("Hunger").Value;
                        int currentAffection = StatSheet.GetStat("Affection").Value;

                        // Finalize sleeping state
                        States.IsSleeping = false;
                        Time.SleepCooldownStartTime = null;

                        // Calculate remaining sleep time
                        double hours = (sleepEndTime - DateTime.Now).TotalHours;
                        int minutes = (int)(sleepEndTime - DateTime.Now).TotalMinutes;

                        if (currentHunger == 0)
                        {
                            if (currentAffection > 0)
                            {
                                // Use HandleBarBlinking for affection bar updates
                                HandleBarBlinking(currentAffection, -1, AffectionBar.BlinkStateCount, AffectionBar, new List<Task>());
                                StatSheet.UpdateStat("Affection", -1);
                            }
                            States.CurrentState = "Hungry";
                            actionText = ActionDataService.GetRandomItem(ActionData.Hungry);
                        }
                        else
                        {
                            actionText = ActionDataService.GetRandomItem(ActionData.Woke);
                        }

                        await ActionText.StartTextWritingAsync(actionText, 25);
                        // Update stats
                        StatSheet.UpdateStat("Hunger", hungerValue);
                    }
                }
            }
            finally
            {
                RefreshBars();
                ReturnMain(sender, e); // Navigate back to the main menu
                if (States.CurrentState == "Sleeping")
                    States.CurrentState = "Idle";
            }
        }



        private async void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the pet is sleeping
                if (await HandleSleepingPetAsync())
                {
                    ReturnMain(sender, e);
                    return;
                }

                if (sender is Button button && button.DataContext is KeyValuePair<string, string> feedData)
                {
                    // Handle "Back" button functionality
                    if (HandleBackButton(feedData.Key))
                    {
                        ReturnMain(sender, e);
                        return;
                    }
                    // Parse feed data
                    if (TryParseFeedData(feedData.Value, out int hungerValue, out int affectionValue, out int energyValue, out string actionText))
                    {
                        var blinkTasks = new List<Task>();
                        // States.CurrentState = feedData.Key;

                        // Handle HungerBar blinking
                        if(StatSheet.GetStat("Hunger").Value == 10)
                        {
                            actionText = ActionDataService.GetRandomItem(ActionData.Full);
                            ActionText.StartTextWritingAsync(actionText, 25);
                        }
                        else
                        {
                            States.CurrentState = feedData.Key;
                            if (hungerValue != 0) // Hunger changes
                            {
                                int currentHunger = StatSheet.GetStat("Hunger").Value;
                                HandleBarBlinking(currentHunger, hungerValue, HungerBar.BlinkStateCount, HungerBar, blinkTasks);
                            }

                            // Handle AffectionBar blinking
                            if (affectionValue != 0) // Affection changes
                            {
                                int currentAffection = StatSheet.GetStat("Affection").Value;
                                HandleBarBlinking(currentAffection, affectionValue, AffectionBar.BlinkStateCount, AffectionBar, blinkTasks);
                            }

                            // Handle EnergyBar blinking
                            if (energyValue != 0) // Energy changes
                            {
                                int currentEnergy = StatSheet.GetStat("Energy").Value;
                                HandleBarBlinking(currentEnergy, energyValue, EnergyBar.BlinkStateCount, EnergyBar, blinkTasks);
                            }
                            // Show action feedback text
                            ActionText.StartTextWritingAsync(actionText, 25);

                            // Wait for all blinking tasks to finish
                            await Task.WhenAll(blinkTasks);

                            // Update stats dynamically
                            StatSheet.UpdateStat("Hunger", hungerValue);
                            StatSheet.UpdateStat("Affection", affectionValue);
                            StatSheet.UpdateStat("Energy", energyValue);
                        }
                        // Reset state to Idle after 10 seconds
                        //SetStateToIdleAfterDelayAsync(feedData.Key, 1);
                    }
                }
            }
            finally
            {
                RefreshBars();
                ReturnMain(sender, e);
            }
        }



        private async void SetStateToIdleAfterDelayAsync(string expectedState, int seconds)
        {
            await Task.Delay(1000 * seconds); // 10-second delay

            // Validate the current state before changing to "Idle"
            if (States.CurrentState == expectedState)
            {
                States.CurrentState = "Idle"; // Set to Idle if it matches
                Console.WriteLine($"Cat's state has been set to 'Idle' after 10 seconds, as it matched the expected state: {expectedState}.");
            }
            else
            {
                Console.WriteLine($"State did not match. CurrentState: {States.CurrentState}, ExpectedState: {expectedState}. No change made.");
            }
        }

        private async Task<bool> HandleSleepingPetAsync()
        {
            if (States.IsSleeping)
            {
                if (DefaultData.ActionData.DataModel.StatusMessages.TryGetValue("SLEEPNOEAT", out string message))
                {
                    await ActionText.StartTextWritingAsync(message, 25);
                }
                return true;
            }
            return false;
        }



        private bool HandleBackButton(string buttonName)
        {
            return string.Equals(buttonName, "Back", StringComparison.OrdinalIgnoreCase);
        }

        private bool TryParseFeedData(string feedValue, out int hungerValue, out int affectionValue, out int energyValue, out string actionText)
        {
            hungerValue = 0;
            affectionValue = 0;
            energyValue = 0;
            actionText = string.Empty;

            var feedParts = feedValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (feedParts.Length >= 4)
            {
                try
                {
                    hungerValue = int.Parse(feedParts[0].Split(':')[1].Trim());
                    affectionValue = int.Parse(feedParts[1].Split(':')[1].Trim());
                    energyValue = int.Parse(feedParts[2].Split(':')[1].Trim());
                    actionText = feedParts[3].Trim();
                    return true;
                }
                catch
                {
                    // Log error or handle invalid feed data gracefully
                    return false;
                }
            }
            return false;
        }
        private int stateRandom = 0;
        private async void HandleStateChanged(string newState)
        {
            var blinkTasks = new List<Task>();
            int energy = StatSheet.GetStat("Energy").Value;
            int max = 0;
            Random _random = new Random();
            int selection = 0;
            if (newState == "Tired")
            {
                max = ActionData.Tired.Count -1;
                selection = _random.Next(0, max);
                if (stateRandom == selection)
                {
                    selection = _random.Next(0, max);
                }
                stateRandom = selection;
                string text = ActionData.Tired[selection];
                if (!String.IsNullOrEmpty(text)) // 30% chance for an extra story
                {
                    await Task.Delay(2000);
                    await ActionText.StartTextWritingAsync(text, 25);
                }
                int currentAffection = StatSheet.GetStat("Affection").Value;
                if (energy < 1 && currentAffection > 0)
                {
                    int startIndex = currentAffection;
                    int endIndex = currentAffection - 1;

                    AffectionBar.UpdateBlinkStates(startIndex, endIndex, -1);
                    blinkTasks.Add(AffectionBar.HandleBlinkingAsync(startIndex, endIndex, true));
                    await Task.WhenAll(blinkTasks);
                    StatSheet.UpdateStat("Affection", -1);
                    RefreshBars();
                }

                return;
            }
            // Check if the new state is "Idle"
            else if (newState == "Idle")
            {
                await Task.Delay(10000);
                string story = "";
                int num;
                // Select the story to display
                switch (StatSheet.GetStat("Affection").Value)
                {
                    case > 6:
                        max = DefaultData.ActionData.HighAffectionIdle.Count;
                        num = GenerateIndex(max);
                        story = DefaultData.ActionData.HighAffectionIdle[num];
                        break;
                    case > 3:
                        max = DefaultData.ActionData.MidAffectionIdle.Count;
                        num = GenerateIndex(max);
                        story = DefaultData.ActionData.MidAffectionIdle[num];
                        break;
                    default:
                        max = DefaultData.ActionData.LowAffectionIdle.Count;
                        num = GenerateIndex(max);
                        story = DefaultData.ActionData.LowAffectionIdle[num];
                        break;
                }

                // Optionally display an extra story
                if (!String.IsNullOrEmpty(story)) // 30% chance for an extra story
                {
                    await Task.Delay(10000);
                    await ActionText.StartTextWritingAsync(story, 25);
                }
                States.CurrentState = "Idle";
                RefreshBars();
            }
        }
        private void HandleBarBlinking(int currentStatValue, int valueChange, int blinkStateCount, Statbar bar, List<Task> blinkTasks)
        {
            if (valueChange != 0) // Only handle changes (non-zero values)
            {
                int startIndex = valueChange > 0
                    ? currentStatValue // Starting point for increment
                    : Math.Max(currentStatValue + valueChange, 0); // Starting point for decrement

                int endIndex = valueChange > 0
                    ? Math.Min(currentStatValue + valueChange - 1, blinkStateCount - 1) // Ending point for increment
                    : currentStatValue - 1; // Ending point for decrement

                if (startIndex <= endIndex) // Ensure valid range
                {
                    int stateChange = valueChange > 0 ? 1 : -1; // 1 for increment, -1 for decrement
                    bool isDecrementing = valueChange < 0; // True if decrementing

                    // Update the bar's blink states
                    bar.UpdateBlinkStates(startIndex, endIndex, stateChange);

                    // Trigger blinking and add it to the tasks list
                    blinkTasks.Add(bar.HandleBlinkingAsync(startIndex, endIndex, isDecrementing));
                }
            }
        }

    }
}