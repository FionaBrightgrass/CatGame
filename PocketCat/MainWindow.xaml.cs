using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using PocketCat.CatStats;
using PocketCat.Controls;

namespace PocketCat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppSettings _appSettings = new AppSettings();
        private Stats stats;
        SettingsWindow settingsWindow;
        bool textActionInProgress = false;
        public MainWindow()
        {
            InitializeComponent();
            settingsWindow = new SettingsWindow(_appSettings);
            settingsWindow.Visibility = Visibility.Hidden;

            stats = new Stats
            {
                BlinkBarCallback = async (stat, index, flashOnce, isIncreasing) =>
                {
                    // Delegate to the respective Statbar
                    switch (stat)
                    {
                        case "Hunger":
                            HungerBar.UpdateBlinkStates(index, index, isIncreasing ? 1 : -1); // Update state
                            await HungerBar.HandleBlinkingAsync(stats.GetStat("Hunger")); // Handle blinking
                            break;
                        case "Affection":
                            AffectionBar.UpdateBlinkStates(index, index, isIncreasing ? 1 : -1);
                            await AffectionBar.HandleBlinkingAsync(stats.GetStat("Affection"));
                            break;
                        case "Energy":
                            EnergyBar.UpdateBlinkStates(index, index, isIncreasing ? 1 : -1);
                            await EnergyBar.HandleBlinkingAsync(stats.GetStat("Energy"));
                            break;
                        default:
                            break;
                    }
                }
            };

            // Update stat bars after Stats is initialized
            RefreshBars();

            // Hide submenus and show the main control
            FeedControl.Visibility = Visibility.Hidden;
            PlayControl.Visibility = Visibility.Hidden;
            ButtonControl.Visibility = Visibility.Visible;
            // Initialize stat bar states (now obsolete and removed if not needed)
        }

        public void RefreshBars()
        {
            AffectionBar.UpdateBar(stats.GetStat("Affection")); // Update AffectionBar
            HungerBar.UpdateBar(stats.GetStat("Hunger"));       // Update HungerBar
            EnergyBar.UpdateBar(stats.GetStat("Energy"));       // Update EnergyBar
            UpdateLayout();                                     // Ensure the layout is updated
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


        private int storyIndex = 0;
        private int lastRandom = -1;
        private bool isOnCooldown = false;
        private static readonly TimeSpan CooldownDuration = TimeSpan.FromMinutes(5);
        private DateTime? cooldownStartTime = null; // Nullable to handle cases where no cooldown has started
        private async void PetPet(object sender, RoutedEventArgs e)
        {
            if (textActionInProgress)
                return;

            // Check if the pet is on cooldown
            if (stats.IsPetOnCooldown())
            {
                // Notify the user of the remaining cooldown time
                TimeSpan remainingTime = stats.PetCooldownStartTime.Value.Add(Stats.PetCooldownDuration) - DateTime.Now;

                if (remainingTime.Minutes < 1)
                {
                    await WriteTextWithLetterFadeInAsync($"#NAME# purrs quietly.", 25);
                }
                else if (remainingTime.Minutes <= 3)
                {
                    await WriteTextWithLetterFadeInAsync($"#NAME# seems bored.", 25);
                }
                else
                {
                    await WriteTextWithLetterFadeInAsync($"You begin to pet #NAME#, but #PRONOUNONE# suddenly walks away.", 25);
                }

                return;
            }

            // Start the pet cooldown
            await stats.StartPetCooldownAsync();

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


        private async void MainButton_Click(object sender, RoutedEventArgs e)
        {

            if (textActionInProgress == true)
                return;
            if (stats.isSleeping == true)
            {
                if (textActionInProgress == false)
                {
                    TimeSpan remainingTime = stats.SleepCooldownStartTime.Value.Add(Stats.PetCooldownDuration) - DateTime.Now;
                    switch (remainingTime.Minutes)
                    {
                        case < 1:
                            await WriteTextWithLetterFadeInAsync($"#NAME# looks like #PRONOUNONE# is on the verge of waking up!", 25);
                            break;
                        case <= 10:
                            await WriteTextWithLetterFadeInAsync($"#NAME# is sleeping softly.", 25);
                            break;
                        default:
                            await WriteTextWithLetterFadeInAsync($"#NAME# is sound asleep.", 25);
                            break;
                    }
                    ReturnMain(sender, e);
                    return;
                }
                else
                {
                    return;
                }
            }

            if (sender is Button button && button.DataContext is KeyValuePair<string, string> mainData)
            {
                string key = mainData.Key;
                // Switch the ContentControl content dynamically
                switch (key)
                {
                    case "Feed":
                        ButtonControl.Visibility = Visibility.Collapsed;
                        PlayControl.Visibility = Visibility.Collapsed;
                        SleepControl.Visibility = Visibility.Collapsed;
                        FeedControl.Visibility = Visibility.Visible;
                        break;
                    case "Play":
                        ButtonControl.Visibility = Visibility.Collapsed;
                        FeedControl.Visibility = Visibility.Collapsed;
                        SleepControl.Visibility = Visibility.Collapsed;
                        PlayControl.Visibility = Visibility.Visible;
                        break;
                    case "Pet":
                        PetPet(sender, e);
                        break;
                    case "Sleep":
                        ButtonControl.Visibility = Visibility.Collapsed;
                        FeedControl.Visibility = Visibility.Collapsed;
                        PlayControl.Visibility = Visibility.Collapsed;
                        SleepControl.Visibility = Visibility.Visible;
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
                // Update stats using the unified UpdateStat method
                stats.UpdateStat("Hunger", hungerValue);
                stats.UpdateStat("Affection", affectionValue);
                stats.UpdateStat("Energy", energyValue);

                // Update UI elements
                //UpdateStatBars();

                // Display feedback text
                await WriteTextWithLetterFadeInAsync(actionText, 25);
            }
            finally
            {
                textActionInProgress = false; // Ensure this is always reset
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
            if (textActionInProgress) return;

            try
            {
                if (HandleSleepingPet())
                {
                    ReturnMain(sender, e);
                    return;
                }

                if (sender is Button button && button.DataContext is KeyValuePair<string, string> playData)
                {
                    if (HandleBackButton(playData.Key))
                    {
                        ReturnMain(sender, e);
                        return;
                    }

                    if (TryParseActionData(playData.Value, out int hungerValue, out int affectionValue, out int energyValue, out string actionText))
                    {
                        var blinkTasks = new List<Task>();

                        // Trigger blinking for HungerBar
                        if (hungerValue > 0)
                        {
                            int currentHunger = stats.GetStat("Hunger");
                            int endIndex = Math.Min(currentHunger + hungerValue - 1, HungerBar.BlinkStateCount - 1); // Ensure valid range
                            HungerBar.UpdateBlinkStates(currentHunger, endIndex, 1); // State = 1 for increment
                            blinkTasks.Add(HungerBar.HandleBlinkingAsync(currentHunger + hungerValue)); // Add task for blinking
                        }

                        // Trigger blinking for AffectionBar
                        if (affectionValue > 0)
                        {
                            int currentAffection = stats.GetStat("Affection");
                            int endIndex = Math.Min(currentAffection + affectionValue - 1, AffectionBar.BlinkStateCount - 1); // Ensure valid range
                            AffectionBar.UpdateBlinkStates(currentAffection, endIndex, 1); // State = 1 for increment
                            blinkTasks.Add(AffectionBar.HandleBlinkingAsync(currentAffection + affectionValue)); // Add task for blinking
                        }

                        // Trigger blinking for EnergyBar
                        if (energyValue < 0)
                        {
                            int currentEnergy = stats.GetStat("Energy");
                            int startIndex = Math.Max(currentEnergy + energyValue, 0); // Ensure valid range
                            int endIndex = currentEnergy - 1; // Decrementing: End at the current index - 1
                            EnergyBar.UpdateBlinkStates(startIndex, endIndex, -1); // State = -1 for decrement
                            blinkTasks.Add(EnergyBar.HandleBlinkingAsync(currentEnergy + energyValue, true)); // Add task for blinking
                        }

                        // Wait for all blinking tasks to complete
                        await Task.WhenAll(blinkTasks);

                        // Perform actions to update stats and display feedback
                        await PerformAction(actionText, hungerValue, affectionValue, energyValue);

                        // Refresh the UI after stat updates
                        //RefreshBars();
                    }
                }
            }
            finally
            {
                textActionInProgress = false; // Reset the flag
                ReturnMain(sender, e);
            }
        }



        private async void SleepButton_Click(object sender, RoutedEventArgs e)
        {
            if (textActionInProgress) return;

            try
            {
                // Extract buttonName safely
                string[] parts = sender.ToString().Split(':');
                string buttonName = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                // Handle the "Back" button
                if (HandleBackButton(buttonName, e)) return;

                // Process sleep action logic
                if (DefaultData.ActionData.DataModel.SleepControl.TryGetValue(buttonName, out string sleepData))
                {
                    if (TryParseActionData(sleepData, out int hungerValue, out int affectionValue, out int energyValue, out string actionText))
                    {
                        // Update Hunger and Affection via PerformAction
                        await PerformAction(actionText, hungerValue, affectionValue, 0);
                        Console.WriteLine("The pet is now sleeping.");

                        // Start sleeping state
                        stats.isSleeping = true;
                        stats.SleepCooldownStartTime = DateTime.Now;

                        // Calculate total sleep duration based on energy needed and interval
                        int currentEnergy = stats.GetStat("Energy");
                        int energyToRestore = 10 - currentEnergy; // Energy bars needed to fill to max
                        DateTime sleepEndTime = stats.SleepCooldownStartTime.Value.AddMinutes(_appSettings.SleepInterval * energyToRestore);

                        // Loop to increment energy bars one at a time
                        while (currentEnergy < 10 && stats.isSleeping)
                        {
                            // Recalculate remaining sleep duration dynamically
                            TimeSpan remainingTime = sleepEndTime - DateTime.Now;
                            Console.WriteLine($"Remaining sleep time: {remainingTime.Minutes} minutes, {remainingTime.Seconds} seconds.");

                            if (remainingTime <= TimeSpan.Zero)
                            {
                                break; // Exit the loop if time runs out
                            }

                            // Blink the next energy bar
                            int barIndex = currentEnergy;
                            var blinkingTask = EnergyBar.BlinkContinuouslyAsync(barIndex);

                            // Wait for the interval duration
                            await Task.Delay((int)TimeSpan.FromMinutes(_appSettings.SleepInterval).TotalMilliseconds);

                            // Stop blinking and increment energy
                            EnergyBar.StopBlinking(barIndex);
                            stats.UpdateStat("Energy", 1);
                            currentEnergy++;

                            Console.WriteLine($"Energy restored: {currentEnergy}/10");
                        }

                        // Finalize sleep
                        stats.isSleeping = false;
                        stats.SleepCooldownStartTime = null; // Reset cooldown start time
                        await WriteTextWithLetterFadeInAsync("#NAME# is awake and ready to play!", 25);
                    }
                }
            }
            finally
            {
                textActionInProgress = false; // Always reset the flag
                ReturnMain(sender, e); // Navigate back to the main menu
            }
        }









        private async void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            if (textActionInProgress) return;

            try
            {
                if (HandleSleepingPet())
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

                        // Trigger blinking for HungerBar
                        if (hungerValue > 0)
                        {
                            int currentHunger = stats.GetStat("Hunger");
                            int endIndex = Math.Min(currentHunger + hungerValue - 1, HungerBar.BlinkStateCount - 1); // Ensure valid range
                            HungerBar.UpdateBlinkStates(currentHunger, endIndex, 1); // State = 1 for increment
                            blinkTasks.Add(HungerBar.HandleBlinkingAsync(currentHunger + hungerValue)); // Add task for blinking
                        }

                        // Trigger blinking for AffectionBar
                        if (affectionValue > 0)
                        {
                            int currentAffection = stats.GetStat("Affection");
                            int endIndex = Math.Min(currentAffection + affectionValue - 1, AffectionBar.BlinkStateCount - 1); // Ensure valid range
                            AffectionBar.UpdateBlinkStates(currentAffection, endIndex, 1); // State = 1 for increment
                            blinkTasks.Add(AffectionBar.HandleBlinkingAsync(currentAffection + affectionValue)); // Add task for blinking
                        }

                        // Trigger blinking for EnergyBar
                        if (energyValue < 0)
                        {
                            int currentEnergy = stats.GetStat("Energy");
                            int startIndex = Math.Max(currentEnergy + energyValue, 0); // Ensure valid range
                            int endIndex = currentEnergy - 1; // Decrementing: End at the current index - 1
                            EnergyBar.UpdateBlinkStates(startIndex, endIndex, -1); // State = -1 for decrement
                            blinkTasks.Add(EnergyBar.HandleBlinkingAsync(currentEnergy + energyValue, true)); // Add task for blinking
                        }

                        // Wait for all blinking tasks to complete
                        await Task.WhenAll(blinkTasks);


                        // Perform actions to update stats and display feedback
                        await PerformAction(actionText, hungerValue, affectionValue, energyValue);

                        // Refresh the UI after stat updates
                        //RefreshBars();
                    }
                }
            }
            finally
            {
                textActionInProgress = false;
                ReturnMain(sender, e);
            }
        }

        private bool HandleSleepingPet()
        {
            if (stats.isSleeping)
            {
                if (DefaultData.ActionData.DataModel.StatusMessages.TryGetValue("SLEEPNOEAT", out string message))
                {
                    WriteTextWithLetterFadeInAsync(message, 25).Wait(); // Use `.Wait()` in synchronous helpers if needed
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