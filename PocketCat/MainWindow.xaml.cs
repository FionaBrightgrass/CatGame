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

namespace PocketCat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppSettings _appSettings = new AppSettings();
        SettingsWindow settingsWindow;
        bool textActionInProgress = false;
        private int storyIndex = 0;
        private int lastRandom = -1;
        public MainWindow()
        {
            InitializeComponent();
            settingsWindow = new SettingsWindow(_appSettings);
            settingsWindow.Visibility = Visibility.Hidden;
            // Assign the BlinkBarCallback directly to the static Stats class
            Stat.BlinkBarCallback = async (stat, index, flashOnce, isIncreasing) =>
            {
                // Delegate to the respective Statbar
                switch (stat)
                {
                    case "Hunger":
                        HungerBar.UpdateBlinkStates(index, index, isIncreasing ? 1 : -1); // Update state
                        await HungerBar.HandleBlinkingAsync(Stat.GetStat("Hunger")); // Handle blinking
                        break;
                    case "Affection":
                        AffectionBar.UpdateBlinkStates(index, index, isIncreasing ? 1 : -1);
                        await AffectionBar.HandleBlinkingAsync(Stat.GetStat("Affection"));
                        break;
                    case "Energy":
                        EnergyBar.UpdateBlinkStates(index, index, isIncreasing ? 1 : -1);
                        await EnergyBar.HandleBlinkingAsync(Stat.GetStat("Energy"));
                        break;
                    default:
                        break;
                }
            };

            // Initialize other settings, such as AppSettings or events, separately
            Stat._AppSettings = _appSettings;
            Time._AppSettings = _appSettings;
            Time.HungerLevelChanged += OnSleepingHungerLevelChanged;
            // Update stat bars after Stats is initialized
            RefreshBars();

            // Hide submenus and show the main control
            FeedControl.Visibility = Visibility.Hidden;
            PlayControl.Visibility = Visibility.Hidden;
            ButtonControl.Visibility = Visibility.Visible;
            // Initialize stat bar states (now obsolete and removed if not needed)
            AffectionBar.ParentWindow = this;
            HungerBar.ParentWindow = this;
            EnergyBar.ParentWindow = this;
        }

        public void RefreshBars()
        {
            AffectionBar.UpdateBar(Stat.GetStat("Affection")); // Update AffectionBar
            HungerBar.UpdateBar(Stat.GetStat("Hunger"));       // Update HungerBar
            EnergyBar.UpdateBar(Stat.GetStat("Energy"));       // Update EnergyBar
            UpdateLayout();                                     // Ensure the layout is updated
        }

        private async void OnSleepingHungerLevelChanged(int newHungerLevel)
        {
            var blinkTasks = new List<Task>();
            int currentHunger = Stat.GetStat("Hunger");
            int endHunger = currentHunger - 1;
            HungerBar.UpdateBlinkStates(currentHunger, endHunger, -1); // Decrement state = -1
            blinkTasks.Add(EnergyBar.HandleBlinkingAsync(currentHunger + -1, true)); // Add asynchronous blinking task
            if (States.IsSleeping)
            {
                ActionText.WriteTextWithLetterFadeInAsync($"#NAME#'s belly rumbles quietly.'", 25,
                                                            DefaultData.ActionData.DataModel.Name,
                                                            DefaultData.ActionData.DataModel.PronounOne,
                                                            DefaultData.ActionData.DataModel.PronounTwo,
                                                            DefaultData.ActionData.DataModel.PronounOneLower,
                                                            DefaultData.ActionData.DataModel.PronounTwoLower);
            }

            else
            {
                ActionText.WriteTextWithLetterFadeInAsync($"#NAME# wakes up and meows for food.", 25,
                                                        DefaultData.ActionData.DataModel.Name,
                                                        DefaultData.ActionData.DataModel.PronounOne,
                                                        DefaultData.ActionData.DataModel.PronounTwo,
                                                        DefaultData.ActionData.DataModel.PronounOneLower,
                                                        DefaultData.ActionData.DataModel.PronounTwoLower);
            }
            await Task.WhenAll(blinkTasks);
            Stat.UpdateStat("Hunger", -1);
            RefreshBars();
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

        public async Task WriteTextWithLetterFadeInAsync(string customText, double lettersPerSecond)
        {
            customText = customText.Replace("#NAME#", DefaultData.ActionData.DataModel.Name);
            customText = customText.Replace("#PRONOUNONE#", DefaultData.ActionData.DataModel.PronounOne);
            customText = customText.Replace("#PRONOUNTWO#", DefaultData.ActionData.DataModel.PronounTwo);
            customText = customText.Replace("#PRONOUNONELOWER#", DefaultData.ActionData.DataModel.PronounOneLower);
            customText = customText.Replace("#PRONOUNTWOLOWER#", DefaultData.ActionData.DataModel.PronounTwoLower);
            if (textActionInProgress == true)
                return;
            textActionInProgress = true;
            await Task.Delay(100);
            double delay = 1000 / lettersPerSecond; // Delay in milliseconds per letter
            // Clear the ActionText content
            ActionText.Inlines.Clear();

            for (int i = 0; i < customText.Length; i++)
            {
                // Create a Run for the current letter
                Run letterRun = new Run(customText[i].ToString());

                // Add the Run to the TextBlock
                ActionText.Inlines.Add(letterRun);

                // Animate the opacity of the letter
                FadeInLetter(letterRun);
                // Wait before adding the next letter
                await Task.Delay((int)delay);
            }
            textActionInProgress = false;
        }

        private void FadeInLetter(Run letterRun)
        {
            // Create a DoubleAnimation to fade in the letter
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                From = 0.0, // Start fully transparent
                To = 1.0, // End fully opaque
                Duration = TimeSpan.FromMilliseconds(250) // Duration of the fade-in
            };

            // Apply the animation to the letter's opacity
            letterRun.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
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
            // Prevent overlapping operations if a text animation is already running
            if (ActionText.TextActionInProgress)
                return;

            // Handle sleeping pet scenario
            if (States.IsSleeping)
            {
                TimeSpan remainingTime = Time.SleepCooldownStartTime.Value.Add(Time.PetCooldownDuration) - DateTime.Now;

                if (remainingTime.Minutes < 1)
                {
                    await ActionText.WriteTextWithLetterFadeInAsync($"#NAME# looks like #PRONOUNONE# is on the verge of waking up!", 25,
                        DefaultData.ActionData.DataModel.Name,
                        DefaultData.ActionData.DataModel.PronounOne,
                        DefaultData.ActionData.DataModel.PronounTwo,
                        DefaultData.ActionData.DataModel.PronounOneLower,
                        DefaultData.ActionData.DataModel.PronounTwoLower);
                }
                else if (remainingTime.Minutes <= 10)
                {
                    await ActionText.WriteTextWithLetterFadeInAsync($"#NAME# is sleeping softly.", 25,
                        DefaultData.ActionData.DataModel.Name,
                        DefaultData.ActionData.DataModel.PronounOne,
                        DefaultData.ActionData.DataModel.PronounTwo,
                        DefaultData.ActionData.DataModel.PronounOneLower,
                        DefaultData.ActionData.DataModel.PronounTwoLower);
                }
                else
                {
                    await ActionText.WriteTextWithLetterFadeInAsync($"#NAME# is sound asleep.", 25,
                        DefaultData.ActionData.DataModel.Name,
                        DefaultData.ActionData.DataModel.PronounOne,
                        DefaultData.ActionData.DataModel.PronounTwo,
                        DefaultData.ActionData.DataModel.PronounOneLower,
                        DefaultData.ActionData.DataModel.PronounTwoLower);
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
                        PetPet(sender, e); // Call the Pet logic
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
                string[] parts = data.Split(',');
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
                    else
                    {
                        // Handle parts that do not have a key-value format (ActionText)
                        actionText += part.Trim() + " ";
                    }
                }

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

                // Update stats using the unified UpdateStat method
                Stat.UpdateStat("Hunger", hungerValue);
                Stat.UpdateStat("Affection", affectionValue);
                Stat.UpdateStat("Energy", energyValue);

                // Display feedback text
                await ActionText.WriteTextWithLetterFadeInAsync(actionText, 25,
                    DefaultData.ActionData.DataModel.Name,
                    DefaultData.ActionData.DataModel.PronounOne,
                    DefaultData.ActionData.DataModel.PronounTwo,
                    DefaultData.ActionData.DataModel.PronounOneLower,
                    DefaultData.ActionData.DataModel.PronounTwoLower);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error performing action: {ex.Message}");
            }
            finally
            {
                // Reset the action state
                ActionText.TextActionInProgress = false;
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
            // Prevent overlapping operations if another text action is in progress
            if (ActionText.TextActionInProgress)
                return;

            try
            {
                // Handle sleeping pet scenario asynchronously
                if (States.IsSleeping)
                {
                    ReturnMain(sender, e);
                    return;
                }

                if (sender is Button button && button.DataContext is KeyValuePair<string, string> playData)
                {
                    // Handle "Back" button functionality
                    if (HandleBackButton(playData.Key))
                    {
                        ActionText.WriteTextWithLetterFadeInAsync("Best not to wake #NAME#.", 25,
                            DefaultData.ActionData.DataModel.Name,
                            DefaultData.ActionData.DataModel.PronounOne,
                            DefaultData.ActionData.DataModel.PronounTwo,
                            DefaultData.ActionData.DataModel.PronounOneLower,
                            DefaultData.ActionData.DataModel.PronounTwoLower);
                        return;
                    }

                    if (TryParseActionData(playData.Value, out int hungerValue, out int affectionValue, out int energyValue, out string actionText))
                    {
                        var blinkTasks = new List<Task>();
                        States.CurrentState = playData.Key;

                        // Update Hunger Bar with visual feedback for decrement
                        if (hungerValue < 0)
                        {
                            int currentHunger = Stat.GetStat("Hunger");
                            int startIndex = Math.Max(currentHunger + hungerValue, 0); // Ensure no negative indices
                            int endIndex = currentHunger - 1; // Decrement ends at the current index - 1
                            HungerBar.UpdateBlinkStates(startIndex, currentHunger - 1, -1); // State = -1 for decrement
                            blinkTasks.Add(HungerBar.HandleBlinkingAsync(currentHunger + hungerValue, true));
                        }

                        // Update Affection Bar with visual feedback
                        if (affectionValue > 0)
                        {
                            int currentAffection = Stat.GetStat("Affection");
                            int endIndex = Math.Min(currentAffection + affectionValue - 1, AffectionBar.BlinkStateCount - 1);
                            AffectionBar.UpdateBlinkStates(currentAffection, endIndex, 1); // Increment state = 1
                            blinkTasks.Add(AffectionBar.HandleBlinkingAsync(currentAffection + affectionValue));
                        }


                        // Update Energy Bar with decrement visual feedback
                        if (energyValue < 0)
                        {
                            int currentEnergy = Stat.GetStat("Energy");
                            int startIndex = Math.Max(currentEnergy + energyValue, 0); // Ensure no negative indices
                            int endIndex = currentEnergy - 1; // Decrement ends at the current index - 1
                            EnergyBar.UpdateBlinkStates(startIndex, currentEnergy - 1, -1); // State = -1 for decrement
                            blinkTasks.Add(EnergyBar.HandleBlinkingAsync(currentEnergy + energyValue, true));
                        }

                        ActionText.WriteTextWithLetterFadeInAsync(actionText, 25,
                            DefaultData.ActionData.DataModel.Name,
                            DefaultData.ActionData.DataModel.PronounOne,
                            DefaultData.ActionData.DataModel.PronounTwo,
                            DefaultData.ActionData.DataModel.PronounOneLower,
                            DefaultData.ActionData.DataModel.PronounTwoLower);

                        // Await all blinking tasks to complete
                        await Task.WhenAll(blinkTasks);

                        // Update stats after visual feedback
                        Stat.UpdateStat("Hunger", hungerValue);
                        Stat.UpdateStat("Affection", affectionValue);
                        Stat.UpdateStat("Energy", energyValue);

                        // Set state to Idle after 10 seconds if the state matches
                        SetStateToIdleAfterDelayAsync(playData.Key, 10);
                    }
                }
            }
            finally
            {
                // Reset the text action progress flag and return to the main menu
                ActionText.TextActionInProgress = false;
                ReturnMain(sender, e);
            }
        }


        private async void PetPet(object sender, RoutedEventArgs e)
        {
            if (ActionText.TextActionInProgress) // Access the property directly from AnimatedTextBlock
                return;

            // Check if the pet is on cooldown
            if (!States.WantsPets)
            {
                // Notify the user of the remaining cooldown time
                TimeSpan remainingTime = Time.PetCooldownStartTime.Value.Add(Time.PetCooldownDuration) - DateTime.Now;
                int passedMilliseconds = (int)(DateTime.Now - Time.PetCooldownStartTime.Value).TotalMilliseconds;

                float ratio = 100;
                if (passedMilliseconds > 0 && remainingTime.TotalMilliseconds > 0)
                {
                    float diff = (float)passedMilliseconds / (passedMilliseconds + (float)remainingTime.TotalMilliseconds);
                    ratio = diff * 100; // Scale to percentage
                }
                else
                {
                    ratio = 0; // Default ratio for unexpected values
                }

                if (ratio > 80) //80%
                {
                    await ActionText.WriteTextWithLetterFadeInAsync($"#NAME# purrs quietly.", 25,
                        DefaultData.ActionData.DataModel.Name,
                        DefaultData.ActionData.DataModel.PronounOne,
                        DefaultData.ActionData.DataModel.PronounTwo,
                        DefaultData.ActionData.DataModel.PronounOneLower,
                        DefaultData.ActionData.DataModel.PronounTwoLower);
                    return;
                }
                else if (ratio > 20)
                {
                    await ActionText.WriteTextWithLetterFadeInAsync($"#NAME# seems bored.", 25,
                        DefaultData.ActionData.DataModel.Name,
                        DefaultData.ActionData.DataModel.PronounOne,
                        DefaultData.ActionData.DataModel.PronounTwo,
                        DefaultData.ActionData.DataModel.PronounOneLower,
                        DefaultData.ActionData.DataModel.PronounTwoLower);
                    return;
                }
                else if (ratio > 0)
                {
                    await ActionText.WriteTextWithLetterFadeInAsync($"You begin to pet #NAME#, but #PRONOUNONE# suddenly walks away.", 25,
                        DefaultData.ActionData.DataModel.Name,
                        DefaultData.ActionData.DataModel.PronounOne,
                        DefaultData.ActionData.DataModel.PronounTwo,
                        DefaultData.ActionData.DataModel.PronounOneLower,
                        DefaultData.ActionData.DataModel.PronounTwoLower);
                    return;
                }
                else
                {
                    States.WantsPets = true;
                }
            }

            // Start the pet cooldown
            Time.StartPetCooldownAsync();

            // Perform the PetPet logic
            int randomCount = DefaultData.ActionData.PetRandom.Count;
            Random random = new Random();
            int num = random.Next(0, randomCount);

            // Reduce the chance to repeat the same message
            if (lastRandom == num)
            {
                num = random.Next(0, randomCount);
            }
            lastRandom = num;

            // Select the story to display
            string story = DefaultData.ActionData.PetRandom[num];

            // Calculate the stat changes
            int affectionGain = num % 3;
            int hungerChange = 0;  // No hunger change for petting
            int energyChange = 0;  // No energy change for petting

            // Use PerformAction to handle stats update and text display
            await PerformAction(story, hungerChange, affectionGain, energyChange);
            if (DefaultData.ActionData.Extra == null || DefaultData.ActionData.Extra.Count == 0)
            {
                Console.WriteLine("Extra story data is missing or empty.");
                return;
            }
            // Optionally display an extra story
            if (random.Next(0, 10) < 3) // 30% chance for an extra story
            {
                await Task.Delay(2000);

                string extraStory = DefaultData.ActionData.Extra[storyIndex];
                await PerformAction(extraStory, 0, 0, 0); // No stat change for extra story

                // Cycle through the extra story index
                storyIndex++;
                if (storyIndex >= DefaultData.ActionData.Extra.Count)
                {
                    storyIndex = 0;
                }
            }
        }



        private async void SleepButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionText.TextActionInProgress) // Using the property for animation state
                return;

            try
            {
                // Safely extract buttonName
                string buttonName = sender is Button button ? button.Content?.ToString().Trim() ?? string.Empty : string.Empty;

                // Handle the "Back" button scenario
                if (HandleBackButton(buttonName, e))
                {
                    ReturnMain(sender, e);
                    return;
                }

                // Process sleep action logic
                if (DefaultData.ActionData.DataModel.SleepControl.TryGetValue(buttonName, out string sleepData))
                {
                    if (TryParseActionData(sleepData, out int hungerValue, out int affectionValue, out int energyValue, out string actionText))
                    {
                        // Start sleeping state
                        States.IsSleeping = true;
                        States.CurrentState = "Sleeping";
                        // Perform initial action
                        await PerformAction(actionText, hungerValue, affectionValue, 0);
                        Console.WriteLine("The pet is now sleeping.");


                        Time.SleepCooldownStartTime = DateTime.Now;
                        // Calculate total sleep duration based on energy needed and interval
                        int currentEnergy = Stat.GetStat("Energy");
                        int energyToRestore = Stat.MaxStatValue - currentEnergy; // Number of energy bars to restore
                        int sleepDuration = _appSettings.SleepInterval * energyToRestore;
                        DateTime sleepEndTime = Time.SleepCooldownStartTime.Value.AddMinutes(sleepDuration);
                        //sleepEndTime = sleepEndTime.AddSeconds(30);
                        while (currentEnergy < 10 && States.IsSleeping)
                        {
                            // Blink the current energy bar
                            await EnergyBar.BlinkContinuouslyAsync(currentEnergy, _appSettings.SleepInterval);
                            // Stop blinking, update energy, and increment
                            EnergyBar.StopBlinking(currentEnergy);
                            Stat.UpdateStat("Energy", 1);
                            currentEnergy++;
                            Console.WriteLine($"Energy restored: {currentEnergy}/10");
                        }

                        // Finalize sleep and notify user
                        States.IsSleeping = false;
                        Time.SleepCooldownStartTime = null; // Reset cooldown start time
                                                             // Calculate remaining time in hours
                        double hours = (sleepEndTime - DateTime.Now).TotalHours;
                        // Calculate remaining time in minutes
                        int minutes = (int)(sleepEndTime - DateTime.Now).TotalMinutes;
                        if (minutes >= 1)
                        {
                            if (hours >= 1)
                            {
                                actionText = $"#NAME# should be asleep for a little over {hours} hour{(hours == 1 ? "" : "s")}.";
                            }
                            else
                            {
                                actionText = $"#NAME# should be asleep for another {minutes.ToString("D2")} minute{(minutes == 1 ? "" : "s")}.";
                            }
                        }
                        else
                        {
                            actionText = $"#NAME# woke up!";
                        }
                        ActionText.WriteTextWithLetterFadeInAsync(actionText, 25,
                                                                        DefaultData.ActionData.DataModel.Name,
                                                                        DefaultData.ActionData.DataModel.PronounOne,
                                                                        DefaultData.ActionData.DataModel.PronounTwo,
                                                                        DefaultData.ActionData.DataModel.PronounOneLower,
                                                                        DefaultData.ActionData.DataModel.PronounTwoLower);
                        Stat.UpdateStat("Hunger", hungerValue);
                        Stat.UpdateStat("Affection", affectionValue);
                        Stat.UpdateStat("Energy", energyValue);
                    }
                }
            }
            finally
            {
                ActionText.TextActionInProgress = false; // Reset the progress flag
                ReturnMain(sender, e); // Navigate back to the main menu
            }
        }


        private async void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            if (textActionInProgress) return;

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
                        var blinkTasks = new List<Task>();
                        States.CurrentState = feedData.Key;

                        // Trigger blinking for HungerBar
                        if (hungerValue > 0)
                        {
                            int currentHunger = Stat.GetStat("Hunger");
                            int endIndex = Math.Min(currentHunger + hungerValue - 1, HungerBar.BlinkStateCount - 1);
                            HungerBar.UpdateBlinkStates(currentHunger, endIndex, 1);
                            blinkTasks.Add(HungerBar.HandleBlinkingAsync(currentHunger + hungerValue));
                        }

                        // Trigger blinking for AffectionBar
                        if (affectionValue > 0)
                        {
                            int currentAffection = Stat.GetStat("Affection");
                            int endIndex = Math.Min(currentAffection + affectionValue - 1, AffectionBar.BlinkStateCount - 1);
                            AffectionBar.UpdateBlinkStates(currentAffection, endIndex, 1);
                            blinkTasks.Add(AffectionBar.HandleBlinkingAsync(currentAffection + affectionValue));
                        }
                        else if (affectionValue < 0)
                        {
                            int currentAffection = Stat.GetStat("Affection");
                            int startIndex = Math.Max(currentAffection + affectionValue, 0); // Ensure valid range (no negative indices)
                            int endIndex = currentAffection - 1;
                            AffectionBar.UpdateBlinkStates(startIndex, endIndex, -1); // State = -1 for decrement
                            blinkTasks.Add(AffectionBar.HandleBlinkingAsync(currentAffection + affectionValue, true)); // Add task for blinking
                        }

                        // Trigger blinking for EnergyBar
                        if (energyValue < 0)
                        {
                            int currentEnergy = Stat.GetStat("Energy");
                            int startIndex = Math.Max(currentEnergy + energyValue, 0);
                            int endIndex = currentEnergy - 1;
                            EnergyBar.UpdateBlinkStates(startIndex, endIndex, -1);
                            blinkTasks.Add(EnergyBar.HandleBlinkingAsync(currentEnergy + energyValue, true));
                        }

                        ActionText.WriteTextWithLetterFadeInAsync(actionText, 25,
                                                                    DefaultData.ActionData.DataModel.Name,
                                                                    DefaultData.ActionData.DataModel.PronounOne,
                                                                    DefaultData.ActionData.DataModel.PronounTwo,
                                                                    DefaultData.ActionData.DataModel.PronounOneLower,
                                                                    DefaultData.ActionData.DataModel.PronounTwoLower);

                        // Wait for all blinking tasks to complete
                        await Task.WhenAll(blinkTasks);

                        Stat.UpdateStat("Hunger", hungerValue);
                        Stat.UpdateStat("Affection", affectionValue);
                        Stat.UpdateStat("Energy", energyValue);

                        // Set state to Idle after 10 seconds
                        SetStateToIdleAfterDelayAsync(feedData.Key, 10);
                    }
                }
            }
            finally
            {
                textActionInProgress = false;
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
                    await ActionText.WriteTextWithLetterFadeInAsync(message, 25,
                        DefaultData.ActionData.DataModel.Name,
                        DefaultData.ActionData.DataModel.PronounOne,
                        DefaultData.ActionData.DataModel.PronounTwo,
                        DefaultData.ActionData.DataModel.PronounOneLower,
                        DefaultData.ActionData.DataModel.PronounTwoLower);
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

            var feedParts = feedValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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
    }
}