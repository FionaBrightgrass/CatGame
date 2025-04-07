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
using PocketCat.Controls.Windows.Inventory;
using PocketCat.StreamElements;
using System.IO;

namespace PocketCat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        NotificationWindow notification;
        public AppSettings AppSettings = new AppSettings();
        private SettingsWindow SettingsWindow;
        private FoodWindow FoodWindow;
        private ToyWindow ToyWindow;
        public Shop ShopWindow;
        private StreamElementsClient StreamElements;
        public Flags Flags = new Flags();
        private string _affectionBoxText = "0";
        private HoverBox _hoverBox;
        public string AffectionBoxText
        {
            get => _affectionBoxText;
            set
            {
                if (_affectionBoxText != value)
                {
                    _affectionBoxText = value;
                }
            }
        }

        public Affection Affection { get; private set; } = new Affection();

        private int lastRandom = -1;

        public MainWindow()
        {

            InitializeComponent();
            SharedInventory.LoadFromFile(Globals.AppWide.ApplicationData + "/items.txt");
            _hoverBox = new HoverBox();
            _hoverBox.Visibility = Visibility.Collapsed;
            DataContext = this;
            // Initialize other components
            ActionText._appSettings = AppSettings;
            StatSheet._mainWindow = this;
            StatSheet.LoadStatsFromFile();
            Time._appSettings = AppSettings;
            Time._mainWindow = this;
            Time.RunContinuousTimerAsync();
            Time.StartAffectionReadyTimer();
            Time.StartTipReadyTimer();
            Time.StartStatDecreaseTimer();
            States.ResetToIdle();
            States.OnStateChanged += HandleStateChanged;
            notification = new NotificationWindow(this);
            Flags.FlagsChanged += () =>
            {
                UpdateTransparencyBasedOnFlags(); // Call the method to adjust transparency
            };
            AffectionBoxText = Affection.DisplayAffection; // Update TextBox content
            AffectionBox.Text = AffectionBoxText;
            Affection.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Affection.DisplayAffection) || e.PropertyName == nameof(Affection.AffectionValue))
                {
                    AffectionBoxText = Affection.DisplayAffection; // Update TextBox content
                    AffectionBox.Text = AffectionBoxText;
                    AffectionBox.UpdateLayout();
                    StatSheet.SaveStatsToFile();
                }
            };

            // Configure StatSheet (no need for instantiation since it's static)
            StatSheet.SubscribeToAllStatChanges();
            StatSheet.ConfigureBlinkBars();

            ShopWindow = new Shop(this);
            ShopWindow.Hide();
            StreamElements = new StreamElementsClient(this);
            _ = Task.Run(() => StreamElements.ConnectAsync());

            FoodWindow = new FoodWindow(this);
            FoodWindow.Hide();
            ToyWindow = new ToyWindow(this);
            ToyWindow.Hide();
            // Initialize the Settings window
            SettingsWindow = new SettingsWindow(AppSettings, this, StreamElements);
            SettingsWindow.Visibility = Visibility.Hidden;
            // Assign BlinkBarCallback dynamically within each Stat
            foreach (var statKey in new[] { "Hunger", "Happiness", "Energy" })
            {
                Stat stat = StatSheet.GetStat(statKey);
                stat.BlinkBarCallback = async (key, value, isIncrement, isMaxed) =>
                {
                    int startIndex = Math.Max(value + (isIncrement ? 0 : -1), 0);
                    int endIndex = Math.Max(value, 0); // Ensure valid range

                    switch (key)
                    {
                        case "Hunger":
                            if (stat.Value <= 3 && States.CurrentState != "Sleeping")
                                States.CurrentState = "Hungry";
                            //HungerBar.UpdateBlinkStates(startIndex, endIndex, isIncrement ? 1 : -1);
                            //await HungerBar.HandleBlinkingAsync(startIndex, endIndex, !isIncrement); // Corrected method call
                            break;

                        case "Happiness":
                            if (stat.Value == 0 || endIndex == 0)
                                Affection.AddAffectionValue(-1);
                            //HappinessBar.UpdateBlinkStates(startIndex, endIndex, isIncrement ? 1 : -1);
                            //await HappinessBar.HandleBlinkingAsync(startIndex, endIndex, !isIncrement); // Corrected method call
                            break;
                        case "Energy":
                            if (stat.Value == 0)
                            {
                                States.CurrentState = "Sleeping";
                                if(!sleepLock)
                                    await HandleSleep(0,0,0,"#NAME# couldn't keep #PRONOUNTHREELOWER# eyes open any longer.");
                            }
                            else if (stat.Value <= 3)
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

            this.LocationChanged += MainWindow_LocationChanged;
            this.StateChanged += MainWindow_StateChanged;
            this.Deactivated += MainWindow_Deactivated;
            // Hide submenus and show the main control
            ButtonControl.Visibility = Visibility.Visible;

            // Associate stat bars with the main window
            HappinessBar.ParentWindow = this;
            HungerBar.ParentWindow = this;
            EnergyBar.ParentWindow = this;
            RefreshBars();
            HandleStateChanged(States.CurrentState);
            //DataContext = Money;
        }

        public async Task RefreshBars()
        {
            // Use the Stats instance to retrieve and update bars
            HappinessBar.UpdateBar(StatSheet.GetStat("Happiness").Value); // Update HappinessBar
            HungerBar.UpdateBar(StatSheet.GetStat("Hunger").Value);       // Update HungerBar
            EnergyBar.UpdateBar(StatSheet.GetStat("Energy").Value);       // Update EnergyBar
            UpdateLayout();
            Task.Delay(1000);// Ensure the layout is updated
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _hoverBox.Hide();
                this.DragMove(); // Allows the user to drag the window

            }
        }



        private void HideUI(object sender, RoutedEventArgs e)
        {
            MainBorder.BorderThickness = new Thickness(0);
            MainBorder.Background.Opacity = 0;
            // Show the NotificationWindow

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
            SleepControl.Visibility = Visibility.Collapsed;
            MainControl.Visibility = Visibility.Collapsed;
            Close_Button.Visibility = Visibility.Collapsed;
            Settings_Button.Visibility = Visibility.Collapsed;
            HideUI_Button.Visibility = Visibility.Collapsed;
            ActionText.Visibility = Visibility.Visible;
            HappinessBar.Visibility = Visibility.Visible;
            HungerBar.Visibility = Visibility.Visible;
            EnergyBar.Visibility = Visibility.Visible;

            // Make the background transparent
            this.Background = Brushes.Transparent;

            // Hide other UI components
            SettingsWindow.Visibility = Visibility.Hidden;
            ShopWindow.Visibility = Visibility.Hidden;
            FoodWindow.Visibility = Visibility.Hidden;
            ToyWindow.Visibility = Visibility.Hidden;
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
            SleepControl.Visibility = Visibility.Visible;
            MainControl.Visibility = Visibility.Visible;
            Close_Button.Visibility = Visibility.Visible;
            Settings_Button.Visibility = Visibility.Visible;
            HideUI_Button.Visibility = Visibility.Visible;
            // Keep visible the desired elements
            ActionText.Visibility = Visibility.Visible;
            HappinessBar.Visibility = Visibility.Visible;
            HungerBar.Visibility = Visibility.Visible;
            EnergyBar.Visibility = Visibility.Visible;
            // If you have headers as separate elements, ensure their visibility
            Flags.UIHidden = false;
            this.Background = AppSettings.WindowColorBrush;
            this.Background.Opacity = 0.8;
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            if (SettingsWindow != null)
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
                    FoodWindow.Visibility = Visibility.Hidden;
                    ToyWindow.Visibility = Visibility.Hidden;
                    ShopWindow.Visibility = Visibility.Hidden;
                    SettingsWindow.Show();
                    MainBorder.CornerRadius = new CornerRadius(5, 0, 0, 5);
                }
            }
        }

        private void ReturnMain(object sender, RoutedEventArgs e)
        {
            if(ButtonControl.Visibility != Visibility.Visible)
                ButtonControl.Visibility = Visibility.Visible;
            if (SleepControl.Visibility == Visibility.Visible)
                SleepControl.Visibility = Visibility.Hidden;
        }

        private void UpdateControlVisibility(UIElement visibleControl)
        {
            ButtonControl.Visibility = Visibility.Collapsed;
            SleepControl.Visibility = Visibility.Collapsed;

            visibleControl.Visibility = Visibility.Visible;
        }
        private async void MainButton_Click(object sender, RoutedEventArgs e)
        {
            if (Flags.ButtonLock)
            {
                UpdateTransparencyBasedOnFlags();
                return;
            }

            // Handle sleeping pet scenario
            if (Flags.IsSleeping)
            {
                UpdateTransparencyBasedOnFlags();
                if (Time.SleepCooldownStartTime == null)
                    Time.SleepCooldownStartTime = DateTime.Now;
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
                UpdateTransparencyBasedOnFlags();
                // Handle control visibility dynamically
                switch (key)
                {
                    case "Feed":
                        Toggle_FoodWindow();
                        break;
                    case "Play":
                        Toggle_ToyWindow();
                        break;

                    case "Pet":
                        PetButton_Click(sender, e);
                        break;
                    case "Shop":
                        Toggle_ShopWindow();
                        break;

                    default:
                        Console.WriteLine($"Unhandled button: {key}");
                        break;
                }
            }
        }
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal) // Restored from minimized
            {
                if (FoodWindow.Visibility == Visibility.Visible)
                {
                    FoodWindow.Activate();
                }
                if (ToyWindow.Visibility == Visibility.Visible)
                {
                    ToyWindow.Activate();
                }
                if (SettingsWindow.Visibility == Visibility.Visible)
                {
                    SettingsWindow.Activate();
                }
                if (ShopWindow.Visibility == Visibility.Visible)
                {
                    ShopWindow.Activate();
                }
            }
        }
        private bool _wasPreviouslyTopMost = false;
        private void MainWindow_Deactivated(object sender, EventArgs e)
        {

            // Hide all visible child windows
            if (FoodWindow.Visibility == Visibility.Visible && !FoodWindow.IsActive && !FoodWindow.IsMouseOver && !this.IsActive && !this.IsMouseOver)
            {
                FoodWindow.Visibility = Visibility.Hidden;
            }
            if (ToyWindow.Visibility == Visibility.Visible && !ToyWindow.IsActive && !ToyWindow.IsMouseOver && !this.IsActive && !this.IsMouseOver)
            {
                ToyWindow.Visibility = Visibility.Hidden;
            }
            if (SettingsWindow.Visibility == Visibility.Visible && !SettingsWindow.IsActive && !SettingsWindow.IsMouseOver && !this.IsActive && !this.IsMouseOver)
            {
                SettingsWindow.Visibility = Visibility.Hidden;
            }
            if (ShopWindow.Visibility == Visibility.Visible && !ShopWindow.IsActive && !ShopWindow.IsMouseOver && !this.IsActive && !this.IsMouseOver)
            {
                ShopWindow.Visibility = Visibility.Hidden;
            }
        }

        private bool TryParseActionData(string data, out int hunger, out int happiness, out int energy, out string actionText)
        {
            hunger = 0;
            happiness = 0;
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
                        else if (key == "Happiness") happiness = int.Parse(value);
                        else if (key == "Energy") energy = int.Parse(value);
                    }
                }
                // Handle parts that do not have a key-value format (ActionText)
                actionText += parts[3].Trim() + " ";
                Console.WriteLine(actionText);
                actionText = actionText.Trim();
                return true;
            }
            catch
            {
                return false; // Parsing failed
            }
        }


        private async Task PerformAction(string actionText, int hungerValue, int happinessValue, int energyValue)
        {
            try
            {
                Console.WriteLine($"Performing action: {actionText} | Hunger: {hungerValue}, Happiness: {happinessValue}, Energy: {energyValue}");
                await ActionText.StartTextWritingAsync(actionText, 25);
                await RefreshBars();
                Flags.BlinkLock = false;
                Flags.ButtonLock = false;
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

        public async void PlayPet(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Flags.ButtonLock || Flags.ButtonLock)
                {
                    ReturnMain(sender, e);
                    return;
                }
                // Handle sleeping pet scenario asynchronously
                if (Flags.IsSleeping)
                {
                    ReturnMain(sender, e);
                    await ActionText.StartTextWritingAsync("Best not to wake #NAME#.", 25);
                    return;
                }

                if (sender is System.Windows.Controls.Button button && button.DataContext is KeyValuePair<string, string> playData)
                {
                    // Parse action data
                    if (TryParseActionData(playData.Value, out int hungerValue, out int happinessValue, out int energyValue, out string actionText))
                    {
                        ReturnMain(sender, e);
                        Flags.ButtonLock = true;
                        States.StopStateResetTask();
                        States.CurrentState = playData.Key;
                        States.StopStateResetTask();
                        // Display action feedback text
                        Flags.BlinkLock = true;
                        List<Task> blinkTasks = HandleAllBlinkTasks(hungerValue, happinessValue, energyValue);
                        await ActionText.StartTextWritingAsync(actionText, 25);

                        // Wait for all blinking tasks to complete
                        await Task.WhenAll(blinkTasks);
                        Flags.BlinkLock = false;
                        StatSheet.UpdateStat("Hunger", hungerValue);
                        StatSheet.UpdateStat("Happiness", happinessValue);
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
                await RefreshBars();
                States.StartStateResetTask(States.CurrentState);
                ReturnMain(sender, e);
                Flags.ButtonLock = false;
            }
        }

        private async void PetButton_Click(object sender, RoutedEventArgs e)
        {
            if (await HandleSleepingPetAsync() || Flags.ButtonLock || Flags.BlinkLock)
            {
                UpdateTransparencyBasedOnFlags();
                return;
            }
            TimeSpan remainingTime = new();
            int maxHappiness = StatSheet.GetStat("Happiness").Value;
            // Check if the pet is on cooldown
            if (!Flags.WantsPets && Time.PetCooldownStartTime != null)
            {
                UpdateTransparencyBasedOnFlags();
                Flags.ButtonLock = true;
                ReturnMain(sender, e);
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
                {
                    Flags.ButtonLock = false;
                    UpdateTransparencyBasedOnFlags();
                }
                else
                    Flags.WantsPets = true;
                Console.WriteLine($"{ratio}% cooldown complete.");
                if (ratio < 20)
                {
                    string actionText = ActionDataService.GetRandomItem(ActionData.Extra, maxHappiness);
                    await ActionText.StartTextWritingAsync(actionText, 25);
                    Flags.ButtonLock = false;
                    UpdateTransparencyBasedOnFlags();
                    return;
                }
                else if (ratio < 60)
                {
                    string actionText = ActionDataService.GetRandomItem(ActionData.ExtraBored, maxHappiness);
                    await ActionText.StartTextWritingAsync(actionText, 25);
                    Flags.ButtonLock = false;
                    UpdateTransparencyBasedOnFlags();
                    return;
                }
                else if (ratio <= 100)
                {
                    string actionText = ActionDataService.GetRandomItem(ActionData.ExtraAnnoyed, maxHappiness);
                    await ActionText.StartTextWritingAsync(actionText, 25);
                    Flags.ButtonLock = false;
                    UpdateTransparencyBasedOnFlags();
                    return;
                }
            }
            else
            {
                Flags.ButtonLock = true;
                Flags.WantsPets = false;
                States.StopStateResetTask();
                Time.StartPetCooldownAsync();
                string story = ActionDataService.GetRandomItem(ActionData.PetRandom);
                ActionText.StartTextWritingAsync(story, 25);

                // Calculate stat changes
                const int TextWritingSpeed = 25;
                int happinessGain = 1 % 3;
                int hungerChange = 0;
                int energyChange = -1 * (1 % 3);
                Flags.BlinkLock = true;
                List<Task> blinkTasks = HandleAllBlinkTasks(hungerChange, happinessGain, energyChange);
                await Task.WhenAll(blinkTasks);

                // Update stats dynamically
                StatSheet.UpdateStat("Happiness", happinessGain);
                StatSheet.UpdateStat("Energy", energyChange);
                await RefreshBars();
                // Optionally display an extra story (30% chance)
                if (DefaultData.ActionData.Extra != null && DefaultData.ActionData.Extra.Count > 0)
                {
                    if (GenerateIndex(10) < 3)
                    {
                        await Task.Delay(1000);
                        string extraStory = ActionDataService.GetRandomItem(ActionData.PetRandom);
                        ActionText.StartTextWritingAsync(extraStory, 25);
                    }
                }
            }
            Flags.ButtonLock = false;
            Flags.BlinkLock = false;
            UpdateTransparencyBasedOnFlags();
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
            if (Flags.ButtonLock || Flags.BlinkLock || sleepLock)
            {
                UpdateTransparencyBasedOnFlags();
                return;
            }
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
                    UpdateTransparencyBasedOnFlags();
                    if (TryParseActionData(sleepData, out int hungerValue, out int happinessValue, out int energyValue, out string actionText))
                    {
                        States.CurrentState = "Sleeping";
                        States.StopStateResetTask();
                        ReturnMain(sender, e);
                        Flags.ButtonLock = true;
                        Console.WriteLine($"Handling sleep: {actionText}");
                        await HandleSleep(0, 0, 0, actionText);

                        await RefreshBars(); // Refresh bars after sleep action
                        ReturnMain(sender, e);
                        Flags.ButtonLock = false;
                        States.ResetToIdle(); // Safely reset to Idle state
                    }
                }
            }
            finally
            {
                // Ensure states are reset even if exceptions occur
                Flags.IsSleeping = false;
                States.CurrentState = "Idle";
                States.StartStateResetTask("Idle");
                Flags.ButtonLock = false;
                UpdateTransparencyBasedOnFlags();
            }
        }


        private bool sleepLock = false;
        private async Task HandleSleep(int hungerValue, int happinessValue, int energyValue, string actionText)
        {
            if(sleepLock)
            {
                return;
            }
            sleepLock = true;
            // Set the pet to sleeping state
            States.StopStateResetTask();
            Flags.IsSleeping = true;
            States.CurrentState = "Sleeping";
            States.StopStateResetTask();
            // Perform initial sleep action
            await PerformAction(actionText, hungerValue, happinessValue, 0);
            Console.WriteLine("The pet is now sleeping.");

            // Calculate energy restoration parameters
            Time.SleepCooldownStartTime = DateTime.Now;
            int currentEnergy = StatSheet.GetStat("Energy").Value;
            int energyToRestore = Stat.MaxValue - currentEnergy; // Amount of energy to restore
            List<Task> blinkTasks = new();
            // Restore energy dynamically while the pet is sleeping
            while (currentEnergy < Stat.MaxValue)
            {

                States.CurrentState = "Sleeping";
                States.StopStateResetTask();
                Flags.IsSleeping = true; // Ensure consistent sleeping state
                await EnergyBar.BlinkContinuouslyAsync(currentEnergy, AppSettings.SleepInterval); // Blink energy bar
                await EnergyBar.StopBlinking(currentEnergy); // Stop blinking
                await StatSheet.UpdateStat("Energy", 1); // Update energy stat dynamically
                currentEnergy = StatSheet.GetStat("Energy").Value; // Update current energy value
                Console.WriteLine($"Energy restored: {currentEnergy}/{Stat.MaxValue}");
                await RefreshBars();
            }
            sleepLock = false;
            // Finalize sleeping state
            Time.SleepCooldownStartTime = null;     
            string finalActionText = ActionDataService.GetRandomItem(ActionData.Woke);
            Flags.BlinkLock = false;
            await ActionText.StartTextWritingAsync(finalActionText, 25);
            Flags.IsSleeping = false;
            States.CurrentState = "Idle";
            await RefreshBars();
            UpdateTransparencyBasedOnFlags();
        }
        public async void Toggle_FoodWindow()
        {
            if(FoodWindow != null)
            {
                // Get the position and dimensions of the MainWindow
                var mainWindowLeft = this.Left; // X coordinate of MainWindow
                var mainWindowTop = this.Top;   // Y coordinate of MainWindow
                var mainWindowWidth = this.ActualWidth; // Width of MainWindow

                // Set the position for the SettingsWindow to the right of MainWindow
                FoodWindow.Left = mainWindowLeft + mainWindowWidth; // Position it to the right
                FoodWindow.Top = mainWindowTop; // Align the top of both windows
                if (FoodWindow.Visibility == Visibility.Visible)
                {
                    FoodWindow.Visibility = Visibility.Hidden;
                    MainBorder.CornerRadius = new CornerRadius(5);
                }
                else
                {
                    FoodWindow.Visibility = Visibility.Visible;
                    ToyWindow.Visibility = Visibility.Hidden;
                    SettingsWindow.Visibility = Visibility.Hidden;
                    ShopWindow.Visibility = Visibility.Hidden;
                    FoodWindow.Show();
                    MainBorder.CornerRadius = new CornerRadius(5, 0, 0, 5);
                }
            }
        }
        public async void Toggle_ToyWindow()
        {
            if (ToyWindow != null)
            {
                // Get the position and dimensions of the MainWindow
                var mainWindowLeft = this.Left; // X coordinate of MainWindow
                var mainWindowTop = this.Top;   // Y coordinate of MainWindow
                var mainWindowWidth = this.ActualWidth; // Width of MainWindow

                // Set the position for the SettingsWindow to the right of MainWindow
                ToyWindow.Left = mainWindowLeft + mainWindowWidth; // Position it to the right
                ToyWindow.Top = mainWindowTop; // Align the top of both windows
                if (ToyWindow.Visibility == Visibility.Visible)
                {
                    ToyWindow.Visibility = Visibility.Hidden;
                    MainBorder.CornerRadius = new CornerRadius(5);
                }
                else
                {
                    ToyWindow.Visibility = Visibility.Visible;
                    FoodWindow.Visibility = Visibility.Hidden;
                    SettingsWindow.Visibility = Visibility.Hidden;
                    ShopWindow.Visibility = Visibility.Hidden;
                    ToyWindow.Show();
                    MainBorder.CornerRadius = new CornerRadius(5, 0, 0, 5);
                }
            }
        }
        public async void Toggle_ShopWindow()
        {
            if (ShopWindow != null)
            {
                // Get the position and dimensions of the MainWindow
                var mainWindowLeft = this.Left; // X coordinate of MainWindow
                var mainWindowTop = this.Top;   // Y coordinate of MainWindow
                var mainWindowWidth = this.ActualWidth; // Width of MainWindow

                // Set the position for the SettingsWindow to the right of MainWindow
                ShopWindow.Left = mainWindowLeft + mainWindowWidth; // Position it to the right
                ShopWindow.Top = mainWindowTop; // Align the top of both windows
                if (ShopWindow.Visibility == Visibility.Visible)
                {
                    ShopWindow.Visibility = Visibility.Hidden;
                    MainBorder.CornerRadius = new CornerRadius(5);
                }
                else
                {
                    ShopWindow.Visibility = Visibility.Visible;
                    FoodWindow.Visibility = Visibility.Hidden;
                    SettingsWindow.Visibility = Visibility.Hidden;
                    ToyWindow.Visibility = Visibility.Hidden;
                    ShopWindow.Show();
                    MainBorder.CornerRadius = new CornerRadius(5, 0, 0, 5);
                }
            }
        }
        public async void FeedPet(object sender, RoutedEventArgs e)
        {
            try
            {
                if (await HandleSleepingPetAsync() || Flags.ButtonLock || Flags.BlinkLock)
                {
                    UpdateTransparencyBasedOnFlags();
                    return;
                }

                if (sender is Button button && button.DataContext is KeyValuePair<string, string> feedData)
                {
                    UpdateTransparencyBasedOnFlags();
                    if (HandleBackButton(feedData.Key))
                    {
                        ReturnMain(sender, e);
                        return;
                    }
                    if (TryParseFeedData(feedData.Value, out int hungerValue, out int happinessValue, out int energyValue, out string actionText))
                    {
                        Flags.ButtonLock = true;
                        ReturnMain(sender, e);
                        if (StatSheet.GetStat("Hunger").Value == StatSheet.MaxValue)
                        {
                            actionText = ActionDataService.GetRandomItem(ActionData.Full);
                            ActionText.StartTextWritingAsync(actionText, 25);
                        }
                        else
                        {
                            await Time.BlinkBarReady();
                            Flags.BlinkLock = true;
                            List<Task> blinkTasks = HandleAllBlinkTasks(hungerValue, happinessValue, energyValue);
                            States.CurrentState = feedData.Key;
                            ActionText.StartTextWritingAsync(actionText, 25);
                            await Task.WhenAll(blinkTasks);
                            StatSheet.UpdateStat("Hunger", hungerValue);
                            StatSheet.UpdateStat("Happiness", happinessValue);
                            StatSheet.UpdateStat("Energy", energyValue);
                        }
                    }
                }
            }
            finally
            {
                await RefreshBars();
                ReturnMain(sender, e);
                Flags.ButtonLock = false;
                Flags.BlinkLock = false;
                States.CurrentState = "Idle";
                States.StartStateResetTask("Idle");
                UpdateTransparencyBasedOnFlags();
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

        private bool TryParseFeedData(string feedValue, out int hungerValue, out int happinessValue, out int energyValue, out string actionText)
        {
            hungerValue = 0;
            happinessValue = 0;
            energyValue = 0;
            actionText = string.Empty;

            var feedParts = feedValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (feedParts.Length >= 4)
            {
                try
                {
                    hungerValue = int.Parse(feedParts[0].Split(':')[1].Trim());
                    happinessValue = int.Parse(feedParts[1].Split(':')[1].Trim());
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
            await Time.BlinkBarReady();
            Flags.BlinkLock = true;
            UpdateTransparencyBasedOnFlags();
            var blinkTasks = new List<Task>();
            int energy = StatSheet.GetStat("Energy").Value;
            int max = 0;
            Random _random = new Random();
            int selection = 0;



            // Handle "Sleeping" state exclusively
            if (Flags.IsSleeping || newState  == "Sleeping")
            {
                States.StopStateResetTask();
                States.CurrentState = "Sleeping";
                Flags.IsSleeping = true;
                Console.WriteLine("Entering Sleep Logic.");
                if (!sleepLock)
                {
                    await HandleSleep(0, 0, 0, "#NAME# falls fast asleep.");
                    Flags.BlinkLock = false;
                    Flags.ButtonLock = false;
                }
                return; // Exit after handling sleep logic
            }

            // Handle "Hungry" state or low hunger
            else  if (newState == "Hungry")
            {
                States.CurrentState = "Hungry";
                States.StopStateResetTask();
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

                int currentHappiness = StatSheet.GetStat("Happiness").Value;

                if (StatSheet.GetStat("Hunger").Value < 1 && currentHappiness > 0 && Flags.StatDecreaseReady)
                {
                    int startIndex = currentHappiness;
                    int endIndex = currentHappiness - 1;
                    HappinessBar.UpdateBlinkStates(startIndex, endIndex, -1);
                    blinkTasks.Add(HappinessBar.HandleBlinkingAsync(startIndex, endIndex, true));
                    await Task.WhenAll(blinkTasks);
                    StatSheet.UpdateStat("Happiness", -1);
                    await RefreshBars();
                    Flags.StatDecreaseReady = false;
                    if (StatSheet.GetStat("Hunger").Value < 1)
                    {
                        HandleStateChanged("Hungry");
                    }
                }
                await RefreshBars();
                Flags.BlinkLock = false;
                Flags.ButtonLock = false;

                return; // Ensure no further state changes occur
            }

            // Handle "Tired" state
            else if (newState == "Tired")
            {
                States.CurrentState = "Tired";
                States.StopStateResetTask();
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

                int currentHappiness = StatSheet.GetStat("Happiness").Value;

                if (energy < 1 && currentHappiness > 0 && Flags.StatDecreaseReady)
                {
                    if(currentHappiness > 0)
                    {
                        Flags.BlinkLock = true;
                        Flags.ButtonLock = true;
                        int startIndex = currentHappiness;
                        int endIndex = currentHappiness - 1;
                        HappinessBar.UpdateBlinkStates(startIndex, endIndex, -1);
                        blinkTasks.Add(HappinessBar.HandleBlinkingAsync(startIndex, endIndex, true));
                        await Task.WhenAll(blinkTasks);
                        StatSheet.UpdateStat("Happiness", -1);
                        await RefreshBars();
                        Flags.StatDecreaseReady = false;
                    }

                    HandleSleep(0, 0, 0, "#NAME# falls fast asleep.");
                }
                await RefreshBars();
                Flags.BlinkLock = false;
                Flags.ButtonLock = false;
                UpdateTransparencyBasedOnFlags();
                return;
            }

            // Handle "Idle" state logic
            else if (newState == "Idle")
            {
                string story = "";
                int num;

                // Select story based on Happiness level
                switch (StatSheet.GetStat("Happiness").Value)
                {
                    case > 6:
                        max = DefaultData.ActionData.HighHappinessIdle.Count;
                        num = GenerateIndex(max);
                        story = DefaultData.ActionData.HighHappinessIdle[num];
                        break;
                    case > 3:
                        max = DefaultData.ActionData.MidHappinessIdle.Count;
                        num = GenerateIndex(max);
                        story = DefaultData.ActionData.MidHappinessIdle[num];
                        break;
                    default:
                        max = DefaultData.ActionData.LowHappinessIdle.Count;
                        num = GenerateIndex(max);
                        story = DefaultData.ActionData.LowHappinessIdle[num];
                        break;
                }

                if (!String.IsNullOrEmpty(story))
                {
                    await Task.Delay(2000);
                    await ActionText.StartTextWritingAsync(story, 25);
                }

                States.CurrentState = "Idle"; // Ensure state updates cleanly
                await RefreshBars();
                Flags.BlinkLock = false;
                Flags.ButtonLock = false;
            }
        }


        public List<Task> HandleAllBlinkTasks(int hungerValue, int happinessValue, int energyValue)
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

            // Handle HappinessBar blinking
            if (happinessValue != 0) // Happiness changes
            {
                int currentHappiness = StatSheet.GetStat("Happiness").Value;
                int startIndex = happinessValue > 0
                    ? currentHappiness
                    : Math.Max(currentHappiness + happinessValue, 0); // Starting point depends on the change
                int endIndex = happinessValue > 0
                    ? Math.Min(currentHappiness + happinessValue - 1, HappinessBar.BlinkStateCount - 1)
                    : currentHappiness - 1; // Ending point depends on increase or decrease

                HappinessBar.UpdateBlinkStates(startIndex, endIndex, happinessValue > 0 ? 1 : -1); // Update blink state
                blinkTasks.Add(HappinessBar.HandleBlinkingAsync(startIndex, endIndex, happinessValue < 0)); // Trigger blinking
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
            SleepControl.Visibility = Visibility.Visible;
            MainControl.Visibility = Visibility.Visible;
            Close_Button.Visibility = Visibility.Visible;
            Settings_Button.Visibility = Visibility.Visible;
            HideUI_Button.Visibility = Visibility.Visible;
            MainBorder.Visibility = Visibility.Visible;
            Flags.UIHidden = false;
            UpdateLayout();
        }
        private void UpdateTransparencyBasedOnFlags()
        {
            // Check if either of the flags is true
            if (Flags.ButtonLock || Flags.BlinkLock)
            {
                // Set transparency to 0.6
                SetItemsControlTransparency(ButtonControl, 0.4);
                ButtonControl.IsHitTestVisible = false;
                SetItemsControlTransparency(SleepControl, 0.4);
                SleepControl.IsHitTestVisible = false;
            }
            else
            {
                // Reset transparency to 1.0
                SetItemsControlTransparency(ButtonControl, 1.0);
                ButtonControl.IsHitTestVisible = true;
                SetItemsControlTransparency(SleepControl, 1.0);
                SleepControl.IsHitTestVisible = true;
            }
        }
        private void SetItemsControlTransparency(ItemsControl itemsControl, double opacity)
        {
            if (itemsControl != null)
            {
                foreach (var item in itemsControl.Items)
                {
                    if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement container)
                    {
                        container.Opacity = opacity;
                    }
                }
            }
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            // Check if the hover box is visible
            if (_hoverBox.Visibility == Visibility.Visible)
            {
                // Hide the hover box
                _hoverBox.Hide();
            }
        }

        private async void HeartIMG_MouseEnter(object sender, MouseEventArgs e)
        {
            if(_hoverBox.Visibility != Visibility.Visible)
            {
                await Task.Delay(1000);
                _hoverBox.UpdateText("Gain 1 Heart every 30 minutes when increasing Happiness and Happiness is already full.");

                // Get the position of the HeartIMG relative to the main window
                //var imagePosition = HeartIMG.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);

                // Lock the hover box above the HeartIMG
                //_hoverBox.Left = imagePosition.X + (HeartIMG.ActualWidth / 2) - (_hoverBox.Width / 2); // Center horizontally
                //_hoverBox.Top = imagePosition.Y - _hoverBox.Height - 10; // Position above with slight padding
                _hoverBox.Left = this.Left + HeartIMG.Width + 25;
                _hoverBox.Top = this.Top - HeartIMG.Height - 25 ;
                _hoverBox.Visibility = Visibility.Visible;
                // Show the hover box
                _hoverBox.Show();
            }
            // Update the text inside the hover box

        }


        private void HeartIMG_MouseLeave(object sender, MouseEventArgs e)
        {
            _hoverBox.Hide();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            SettingsWindow.Close();
            ShopWindow.Close();
            ToyWindow.Close();
            FoodWindow.Close();
            Time.Stop();
            Application.Current.Shutdown();
            this.Close();

        }

        public async void TryAcceptTip()
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                string audioFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio", "gold.wav");
                if (System.IO.File.Exists(audioFilePath))
                {
                    Console.WriteLine("playing gold.mp3");
                    Sounds.PlaySound(audioFilePath);
                }
                // This code runs on the UI thread
                await ActionText.StartTextWritingAsync("#NAME# watches all the coins fly.    \nYou feel slightly richer!", 25);

                ShopWindow.ViewModel.Money.AddMoney(10);
            });
        }
        public async void TryAcceptPet()
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                // This code runs on the UI thread
                Flags.ButtonLock = true;
                Flags.BlinkLock = true;    
                await ActionText.StartTextWritingAsync("#NAME# enjoys your viewers pets.", 25);
                List<Task> blinkTasks = HandleAllBlinkTasks(0, 1, 0);
                await Task.WhenAll(blinkTasks);
                Flags.BlinkLock = false;
                StatSheet.UpdateStat("Happiness", 1);
                RefreshBars();
                Flags.ButtonLock = false;
            });
        }
        public async void TryAcceptFood(string foodName)
        {
            FoodActionData foodActionData = new();
            string[] keys = foodActionData.DataDict.Keys.ToArray();
            bool found = false;
            foreach (string key in keys)
            {
                if (key.Replace(" ", "") == foodName)
                {
                    foodName = key;
                    found = true;
                    break;
                }
            }
            if (!found)
                return;
            else
            {
                var food = foodActionData.DataDict[foodName];
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var button = new Button
                    {
                        DataContext = new KeyValuePair<string, string>(foodName, food)
                    };
                    // Call MainWindow's FeedPet method
                    FeedPet(button, new RoutedEventArgs());
                });
                // Call MainWindow's FeedPet method
            }
        }
        public async void TryAcceptToy(string toyName)
        {
            ItemActionData toyActionData = new();
            string[] keys = toyActionData.DataDict.Keys.ToArray();
            bool found = false;
            foreach (string key in keys)
            {
                if (key.Replace(" ", "") == toyName)
                {
                    toyName = key;
                    found = true;
                    break;
                }
            }
            if (!found)
                return;
            else
            {
                var toy = toyActionData.DataDict[toyName];
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var button = new Button
                    {
                        DataContext = new KeyValuePair<string, string>(toyName, toy)
                    };
                    // Call MainWindow's FeedPet method
                    PlayPet(button, new RoutedEventArgs());
                });
                // Call MainWindow's FeedPet method
            }
        }
    }
}