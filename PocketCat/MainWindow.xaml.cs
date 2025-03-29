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
using PocketCat.Controls.Windows;
using PocketCat.StaticGame.Items;
using System.Reflection.Metadata;

namespace PocketCat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppSettings AppSettings = new AppSettings();
        private SettingsWindow SettingsWindow;
        private Money Money = new Money();
        private Shop Shop;
        private int lastRandom = -1;

        public MainWindow()
        {
            InitializeComponent();
            // Initialize other components
            ActionText._appSettings = AppSettings;
            Time._appSettings = AppSettings;
            Time._mainWindow = this;
            Time.RunContinuousTimerAsync();
            Time.HungerLevelChanged += OnSleepingHungerLevelChanged;
            States.ResetToIdle();
            States.OnStateChanged += HandleStateChanged;

            // Configure StatSheet (no need for instantiation since it's static)
            StatSheet.SubscribeToAllStatChanges();
            StatSheet.ConfigureBlinkBars();

            Shop = new Shop(this);
            Shop.Show();
            // Initialize the Settings window
            SettingsWindow = new SettingsWindow(AppSettings, this);
            SettingsWindow.Visibility = Visibility.Hidden;

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
            HandleStateChanged(States.CurrentState);
            DataContext = Money;
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
            if (!Flags.IsSleeping || States.CurrentState != "Sleeping")
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
            string actionText = Flags.IsSleeping
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
            SettingsWindow.Close();
            Shop.Close();
            this.Close();
        }
        private void HideUI(object sender, RoutedEventArgs e)
        {
            MainBorder.BorderThickness = new Thickness(0);
            MainBorder.Background.Opacity = 0;
            // Show the NotificationWindow
            NotificationWindow notification = new NotificationWindow(this);
            notification.SetNotificationText("To show the UI in the future, you can simply double-click your pet.");
            notification.WindowStartupLocation = WindowStartupLocation.Manual;
            notification.Left = this.Left + (this.Width - notification.Width) / 2;
            notification.Top = this.Top + ((this.Height - notification.Height) / 2) - 20;
            // Subscribe to NotificationWindow events
            notification.ContinueClicked += ExecuteHideUI; // Call ExecuteHideUI only if Continue is clicked
            notification.CancelClicked += ShowUI; // Handle cancellation

            notification.Owner = this; // Optional, sets MainWindow as owner
            notification.ShowDialog(); // Show as modal dialog
        }
        private void ExecuteHideUI()
        {
            // Hide UI elements
            ButtonControl.Visibility = Visibility.Collapsed;
            FeedControl.Visibility = Visibility.Collapsed;
            PlayControl.Visibility = Visibility.Collapsed;
            SleepControl.Visibility = Visibility.Collapsed;
            MainControl.Visibility = Visibility.Collapsed;
            Close_Button.Visibility = Visibility.Collapsed;
            Settings_Button.Visibility = Visibility.Collapsed;
            HideUI_Button.Visibility = Visibility.Collapsed;

            ActionText.Visibility = Visibility.Visible;
            AffectionBar.Visibility = Visibility.Visible;
            HungerBar.Visibility = Visibility.Visible;
            EnergyBar.Visibility = Visibility.Visible;

            // Make the background transparent
            this.Background = Brushes.Transparent;

            // Hide other UI components
            SettingsWindow.Visibility = Visibility.Hidden;
            Shop.Visibility = Visibility.Hidden;

            // Update the UIHidden flag
            Flags.UIHidden = true;

            Console.WriteLine("UI has been hidden successfully.");
        }


        private void OnContinueClicked()
        {
            // Handle Continue button click
            Console.WriteLine("Continue button was clicked!");
            // Add logic to proceed with actions
        }

        private void OnCancelClicked()
        {
            // Handle Cancel button click
            Console.WriteLine("Cancel button was clicked!");
            // Add logic to abort actions or clean up
        }


        private void ShowUI(object sender, RoutedEventArgs e)
        {
            // Hide UI elements
            ButtonControl.Visibility = Visibility.Visible;
            FeedControl.Visibility = Visibility.Visible;
            PlayControl.Visibility = Visibility.Visible;
            SleepControl.Visibility = Visibility.Visible;
            MainControl.Visibility = Visibility.Visible;
            Close_Button.Visibility = Visibility.Visible;
            Settings_Button.Visibility = Visibility.Visible;
            HideUI_Button.Visibility = Visibility.Visible;
            // Keep visible the desired elements
            ActionText.Visibility = Visibility.Visible;
            AffectionBar.Visibility = Visibility.Visible;
            HungerBar.Visibility = Visibility.Visible;
            EnergyBar.Visibility = Visibility.Visible;
            // If you have headers as separate elements, ensure their visibility
            Flags.UIHidden = false;
            this.Background = AppSettings.WindowColorBrush;
            this.Background.Opacity = 0.8;
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            // Get the position and dimensions of the MainWindow
            var mainWindowLeft = this.Left; // X coordinate of MainWindow
            var mainWindowTop = this.Top;   // Y coordinate of MainWindow
            var mainWindowWidth = this.ActualWidth; // Width of MainWindow

            // Set the position for the SettingsWindow to the right of MainWindow
            SettingsWindow.Left = mainWindowLeft + mainWindowWidth; // Position it to the right
            SettingsWindow.Top = mainWindowTop; // Align the top of both windows
            if (SettingsWindow.Visibility == Visibility.Visible)
            {
                SettingsWindow.Visibility = Visibility.Hidden;
                MainBorder.CornerRadius = new CornerRadius(5);
            }
            else
            {
                SettingsWindow.Visibility = Visibility.Visible;
                SettingsWindow.Show();
                MainBorder.CornerRadius = new CornerRadius(5, 0, 0, 5);
            }
        }

        private void ReturnMain(object sender, RoutedEventArgs e)
        {
            if(ButtonControl.Visibility != Visibility.Visible)
                ButtonControl.Visibility = Visibility.Visible;
            if (FeedControl.Visibility == Visibility.Visible)
                FeedControl.Visibility = Visibility.Hidden;
            if (PlayControl.Visibility == Visibility.Visible)
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
            if (Flags.IsSleeping)
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
                Console.WriteLine($"Performing action: {actionText} | Hunger: {hungerValue}, Affection: {affectionValue}, Energy: {energyValue}");
                await ActionText.StartTextWritingAsync(actionText, 25);
                RefreshBars();
            }
            catch (Exception ex)
            {
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
            Console.WriteLine("Back button failed.");
            return false;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Handle sleeping pet scenario asynchronously
                if (Flags.IsSleeping)
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
                        States.StopStateResetTask();
                        States.CurrentState = playData.Key;
                        States.StopStateResetTask();
                        // Display action feedback text
                        List<Task> blinkTasks = HandleAllBlinkTasks(hungerValue, affectionValue, energyValue);
                        await ActionText.StartTextWritingAsync(actionText, 25);

                        // Wait for all blinking tasks to complete
                        await Task.WhenAll(blinkTasks);
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
            TimeSpan remainingTime = new();
            int maxAffection = StatSheet.GetStat("Affection").Value;
            // Check if the pet is on cooldown
            if (!Flags.WantsPets && Time.PetCooldownStartTime != null)
            {
                Time.PetCooldownStartTime ??= DateTime.Now;
                remainingTime = Time.PetCooldownStartTime.Value.Add(Time.PetCooldownDuration) - DateTime.Now;
                // Calculate the percentage of cooldown completion
                int passedMilliseconds = (int)(DateTime.Now - Time.PetCooldownStartTime.Value).TotalMilliseconds;
                Console.WriteLine($"Total time passed since last pet: {passedMilliseconds}ms");
                float ratio;
                if (passedMilliseconds <= 100 || remainingTime.TotalMilliseconds <= 100)
                {
                    Console.WriteLine("Invalid cooldown timing data detected.");
                    ratio = -1;
                }
                else
                {
                    float diff = (float)passedMilliseconds / (passedMilliseconds + (float)remainingTime.TotalMilliseconds);
                    ratio = diff * 100;
                }
                remainingTime = Time.PetCooldownStartTime.Value.Add(Time.PetCooldownDuration) - DateTime.Now;
                if (remainingTime.Milliseconds >= 0)
                    return;
                else
                    Flags.WantsPets = true;
                Console.WriteLine($"{ratio}% cooldown complete.");
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
            }
            else
            {

                Flags.WantsPets = true;
                States.StopStateResetTask();
                Time.StartPetCooldownAsync();
                string story = ActionDataService.GetRandomItem(ActionData.PetRandom);
                ActionText.StartTextWritingAsync(story, 25);

                // Calculate stat changes
                const int TextWritingSpeed = 25;
                int affectionGain = 1 % 3;
                int hungerChange = 0;
                int energyChange = -1 * (1 % 3);

                List<Task> blinkTasks = HandleAllBlinkTasks(hungerChange, affectionGain, energyChange);
                await Task.WhenAll(blinkTasks);

                // Update stats dynamically
                StatSheet.UpdateStat("Affection", affectionGain);
                StatSheet.UpdateStat("Energy", energyChange);
                RefreshBars();
                // Optionally display an extra story (30% chance)
                if (DefaultData.ActionData.Extra != null && DefaultData.ActionData.Extra.Count > 0)
                {
                    if (GenerateIndex(10) < 3)
                    {
                        await Task.Delay(2000);
                        string extraStory = ActionDataService.GetRandomItem(ActionData.PetRandom);
                        ActionText.StartTextWritingAsync(extraStory, 25);
                    }
                }
            }  
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
            string buttonName = sender is Button button ? button.Content?.ToString().Trim() ?? string.Empty : string.Empty;

            // Handle the "Back" button scenario
            if (string.Equals(buttonName, "Back", StringComparison.OrdinalIgnoreCase))
            {
                ReturnMain(sender, e);
                return;
            }

            try
            {
                // Process sleep action logic if button corresponds to a valid sleep action
                if (DefaultData.ActionData.DataModel.SleepControl.TryGetValue(buttonName, out string sleepData))
                {
                    if (TryParseActionData(sleepData, out int hungerValue, out int affectionValue, out int energyValue, out string actionText))
                    {
                        Console.WriteLine($"Handling sleep: {actionText}");
                        await HandleSleep(hungerValue, affectionValue, energyValue, actionText);

                        RefreshBars(); // Refresh bars after sleep action
                        ReturnMain(sender, e);
                        States.ResetToIdle(); // Safely reset to Idle state
                    }
                }
            }
            finally
            {
                // Ensure states are reset even if exceptions occur
                Flags.IsSleeping = false;
                States.CurrentState = "Idle";
            }
        }


        private async Task HandleSleep(int hungerValue, int affectionValue, int energyValue, string actionText)
        {
            // Set the pet to sleeping state
            States.StopStateResetTask();
            Flags.IsSleeping = true;
            States.CurrentState = "Sleeping";

            // Perform initial sleep action
            await PerformAction(actionText, hungerValue, affectionValue, 0);
            Console.WriteLine("The pet is now sleeping.");

            // Calculate energy restoration parameters
            Time.SleepCooldownStartTime = DateTime.Now;
            int currentEnergy = StatSheet.GetStat("Energy").Value;
            int energyToRestore = Stat.MaxValue - currentEnergy; // Amount of energy to restore
            int sleepDuration = AppSettings.SleepInterval * energyToRestore;

            DateTime sleepEndTime = Time.SleepCooldownStartTime.Value.AddMinutes(sleepDuration);
            List<Task> blinkTasks = new();
            // Restore energy dynamically while the pet is sleeping
            while (currentEnergy < Stat.MaxValue)
            {
                Flags.IsSleeping = true; // Ensure consistent sleeping state
                await EnergyBar.BlinkContinuouslyAsync(currentEnergy, AppSettings.SleepInterval); // Blink energy bar
                EnergyBar.StopBlinking(currentEnergy); // Stop blinking
                StatSheet.UpdateStat("Energy", 1); // Update energy stat dynamically
                currentEnergy++;
                Console.WriteLine($"Energy restored: {currentEnergy}/{Stat.MaxValue}");
                Random random = new Random();
                if(random.NextDouble() < 0.25)
                {
                    if (StatSheet.GetStat("Hunger").Value == 0)
                    {
                        if (StatSheet.GetStat("Affection").Value > 0)
                        {
                            blinkTasks = HandleAllBlinkTasks(0, affectionValue, 0);
                            await Task.WhenAll(blinkTasks);
                            StatSheet.UpdateStat("Affection", -1);
                        }
                    }         
                }
            }
            // Finalize sleeping state
            Time.SleepCooldownStartTime = null;
            string finalActionText = string.Empty;
            if (StatSheet.GetStat("Hunger").Value == 0)
            {
                if (StatSheet.GetStat("Affection").Value > 0)
                {
                    blinkTasks = HandleAllBlinkTasks(0, affectionValue, 0);
                    StatSheet.UpdateStat("Affection", affectionValue);
                }

                States.CurrentState = "Hungry";
                finalActionText = ActionDataService.GetRandomItem(ActionData.Hungry);
            }
            else
            {
                if (StatSheet.GetStat("Hunger").Value > 0)
                {
                    blinkTasks = HandleAllBlinkTasks(hungerValue, 0, 0);
                    StatSheet.UpdateStat("Hunger", hungerValue);
                }
                finalActionText = ActionDataService.GetRandomItem(ActionData.Woke);
            }
            await Task.WhenAll(blinkTasks);
            await ActionText.StartTextWritingAsync(finalActionText, 25);
            RefreshBars();
            // Additional delay for UI consistency
            await Task.Delay(1000);
            Flags.IsSleeping = false; // Exit sleeping state
            States.CurrentState = "Idle"; // Reset state
        }


        private async void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (await HandleSleepingPetAsync())
                {
                    ReturnMain(sender, e);
                    return;
                }

                if (sender is Button button && button.DataContext is KeyValuePair<string, string> feedData)
                {
                    if (HandleBackButton(feedData.Key))
                    {
                        ReturnMain(sender, e);
                        return;
                    }
                    if (TryParseFeedData(feedData.Value, out int hungerValue, out int affectionValue, out int energyValue, out string actionText))
                    {
                        if(StatSheet.GetStat("Hunger").Value == StatSheet.MaxValue)
                        {
                            actionText = ActionDataService.GetRandomItem(ActionData.Full);
                            ActionText.StartTextWritingAsync(actionText, 25);
                        }
                        else
                        {
                            List<Task> blinkTasks = HandleAllBlinkTasks(hungerValue, affectionValue, energyValue);
                            States.CurrentState = feedData.Key;
                            ActionText.StartTextWritingAsync(actionText, 25);
                            await Task.WhenAll(blinkTasks);
                            StatSheet.UpdateStat("Hunger", hungerValue);
                            StatSheet.UpdateStat("Affection", affectionValue);
                            StatSheet.UpdateStat("Energy", energyValue);
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


        private async Task<bool> HandleSleepingPetAsync()
        {
            if (Flags.IsSleeping)
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
            Console.WriteLine($"State changed to: {newState}");

            // Immediate safeguard: Exit if sleeping
            if (Flags.IsSleeping)
            {
                Console.WriteLine("HandleStateChanged exited: Pet is sleeping.");
                return;
            }

            // Delay for state handling
            await Task.Delay(6000);

            var blinkTasks = new List<Task>();
            int energy = StatSheet.GetStat("Energy").Value;
            int max = 0;
            Random _random = new Random();
            int selection = 0;

            // Handle "Sleeping" state exclusively
            if (newState == "Sleeping" || Flags.IsSleeping)
            {
                // Ensure further processing stops when already sleeping
                if (!Flags.IsSleeping)
                {
                    Console.WriteLine("Entering Sleep Logic.");
                    await HandleSleep(0, 0, 0, "#NAME# falls fast asleep.");
                }
                return; // Exit after handling sleep logic
            }

            // Handle "Hungry" state or low hunger
            if (newState == "Hungry" || StatSheet.GetStat("Hunger").Value < 3)
            {
                max = ActionData.Hungry.Count - 1;
                selection = _random.Next(0, max);
                if (stateRandom == selection)
                {
                    selection = _random.Next(0, max);
                }
                stateRandom = selection;
                string text = ActionData.Hungry[selection];
                Console.WriteLine(text);

                if (!String.IsNullOrEmpty(text))
                {
                    await Task.Delay(2000);
                    await ActionText.StartTextWritingAsync(text, 25);
                }

                int currentAffection = StatSheet.GetStat("Affection").Value;

                if (StatSheet.GetStat("Hunger").Value < 1 && currentAffection > 0)
                {
                    int startIndex = currentAffection;
                    int endIndex = currentAffection - 1;
                    AffectionBar.UpdateBlinkStates(startIndex, endIndex, -1);
                    blinkTasks.Add(AffectionBar.HandleBlinkingAsync(startIndex, endIndex, true));
                    await Task.WhenAll(blinkTasks);
                    StatSheet.UpdateStat("Affection", -1);
                    RefreshBars();

                    await Task.Delay(AppSettings.TimeInterval * 1000 * 60);
                    if (StatSheet.GetStat("Hunger").Value < 1)
                    {
                        HandleStateChanged("Hungry");
                    }
                }
                return; // Ensure no further state changes occur
            }

            // Handle "Tired" state
            if (newState == "Tired")
            {
                max = ActionData.Tired.Count - 1;
                selection = _random.Next(0, max);

                if (stateRandom == selection)
                {
                    selection = _random.Next(0, max);
                }
                stateRandom = selection;

                string text = ActionData.Tired[selection];

                if (!String.IsNullOrEmpty(text))
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

            // Handle "Idle" state logic
            if (newState == "Idle")
            {
                string story = "";
                int num;

                // Select story based on Affection level
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

                if (!String.IsNullOrEmpty(story))
                {
                    await Task.Delay(2000);
                    await ActionText.StartTextWritingAsync(story, 25);
                }

                States.CurrentState = "Idle"; // Ensure state updates cleanly
                RefreshBars();
            }
        }


        private List<Task> HandleAllBlinkTasks(int hungerValue, int affectionValue, int energyValue)
        {
            var blinkTasks = new List<Task>();
            if (hungerValue != 0) // Hunger changes
            {
                int currentHunger = StatSheet.GetStat("Hunger").Value;
                int startIndex = hungerValue > 0
                    ? currentHunger
                    : Math.Max(currentHunger + hungerValue, 0); // Starting point depends on the change
                int endIndex = hungerValue > 0
                    ? Math.Min(currentHunger + hungerValue - 1, HungerBar.BlinkStateCount - 1)
                    : currentHunger - 1; // Ending point depends on increase or decrease

                HungerBar.UpdateBlinkStates(startIndex, endIndex, hungerValue > 0 ? 1 : -1); // Update blink state
                blinkTasks.Add(HungerBar.HandleBlinkingAsync(startIndex, endIndex, hungerValue < 0)); // Trigger blinking
            }

            // Handle AffectionBar blinking
            if (affectionValue != 0) // Affection changes
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
            if (energyValue != 0) // Energy changes
            {
                int currentEnergy = StatSheet.GetStat("Energy").Value;
                int startIndex = energyValue > 0
                    ? currentEnergy
                    : Math.Max(currentEnergy + energyValue, 0); // Starting point depends on the change
                int endIndex = energyValue > 0
                    ? Math.Min(currentEnergy + energyValue - 1, EnergyBar.BlinkStateCount - 1)
                    : currentEnergy - 1; // Ending point depends on increase or decrease

                EnergyBar.UpdateBlinkStates(startIndex, endIndex, energyValue > 0 ? 1 : -1); // Update blink state
                blinkTasks.Add(EnergyBar.HandleBlinkingAsync(startIndex, endIndex, energyValue < 0)); // Trigger blinking
            }
            return blinkTasks;
        }

        private void SpriteControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Flags.UIHidden)
            {
                ShowUI();
            }
            else
            {
                // UI is already visible, no action needed
                return;
            }
        }
        private void ShowUI()
        {
            MainBorder.BorderThickness = new Thickness(2);
            MainBorder.Background.Opacity = 0.8;
            // Cancel the hide UI operation
            Console.WriteLine("Cancel clicked. UI restoration in progress.");
            // Restore UI elements
            ButtonControl.Visibility = Visibility.Visible;
            FeedControl.Visibility = Visibility.Visible;
            PlayControl.Visibility = Visibility.Visible;
            SleepControl.Visibility = Visibility.Visible;
            MainControl.Visibility = Visibility.Visible;
            Close_Button.Visibility = Visibility.Visible;
            Settings_Button.Visibility = Visibility.Visible;
            HideUI_Button.Visibility = Visibility.Visible;
            MainBorder.Visibility = Visibility.Visible;
            Flags.UIHidden = false;
            UpdateLayout();
        }
    }
}