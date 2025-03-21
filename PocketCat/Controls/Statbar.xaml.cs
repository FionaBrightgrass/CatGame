using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PocketCat.Controls
{
    public partial class Statbar : UserControl
    {
        // Holds the states of all rectangles (bars) in this Statbar
        private List<(Rectangle Bar, int State)> _blinkStates = new();
        public string HeaderText
        {
            get => (string)GetValue(HeaderTextProperty);
            set
            {
                SetValue(HeaderTextProperty, value);
                if (Header != null)
                {
                    Header.Text = value; // Update the TextBlock in the UI
                }
            }
        }
        public static readonly DependencyProperty HeaderTextProperty =
                DependencyProperty.Register(
                    "HeaderText",
                    typeof(string),
                    typeof(Statbar),
                    new PropertyMetadata(string.Empty, OnHeaderTextChanged)
                );

        // Callback to handle changes to HeaderText DependencyProperty
        private static void OnHeaderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Statbar statbar && statbar.Header != null)
            {
                statbar.Header.Text = e.NewValue.ToString(); // Update the TextBlock
            }
        }

        public Statbar()
        {
            InitializeComponent();
            InitializeBlinkStates();
        }


        private void InitializeBlinkStates()
        {
            _blinkStates = StatbarPanel.Children.OfType<Rectangle>()
                .Select(bar => (Bar: bar, State: 0)) // Default state is 0 (unchanged)
                .ToList();

            Console.WriteLine($"Initialized {_blinkStates.Count} rectangles for blinking.");
        }



        public void UpdateBar(int achievedCount)
        {
            // Ensure achievedCount is within bounds
            if (achievedCount < 0) achievedCount = 0;
            if (achievedCount > StatbarPanel.Children.Count) achievedCount = StatbarPanel.Children.Count;

            // Update rectangle colors
            for (int i = 0; i < _blinkStates.Count; i++)
            {
                var (bar, state) = _blinkStates[i];

                if (i < achievedCount || state == 1) // Achieved or incrementing
                {
                    bar.Fill = new SolidColorBrush(Colors.Green);
                }
                else if (state == -1) // Decrementing
                {
                    bar.Fill = new SolidColorBrush(Colors.Black);
                }
                else // Reset everything else
                {
                    bar.Fill = new SolidColorBrush(Colors.Black);
                }
            }

        }
        public void UpdateBlinkStates(int startIndex, int endIndex, int changeState)
        {
            // Validate _blinkStates and ensure it has valid entries
            if (_blinkStates == null || _blinkStates.Count == 0)
            {
                Console.WriteLine("Error: Blink states are empty or uninitialized.");
                return;
            }

            // Clamp indices to valid range
            startIndex = Math.Max(startIndex, 0);
            endIndex = Math.Min(_blinkStates.Count - 1, endIndex);

            // Handle single-bar updates (startIndex == endIndex)
            if (startIndex == endIndex)
            {
                Console.WriteLine($"Updating single blink state: Index={startIndex}, ChangeState={changeState}");
                _blinkStates[startIndex] = (_blinkStates[startIndex].Bar, changeState);
                return; // No need for further looping
            }

            // Handle invalid range (startIndex > endIndex after clamping)
            if (startIndex > endIndex)
            {
                Console.WriteLine($"Invalid range after clamping: startIndex={startIndex}, endIndex={endIndex}");
                return;
            }

            // Handle multi-bar updates (startIndex <= endIndex)
            Console.WriteLine($"Updating blink states from Index={startIndex} to Index={endIndex} with ChangeState={changeState}");
            for (int i = startIndex; i <= endIndex; i++)
            {
                _blinkStates[i] = (_blinkStates[i].Bar, changeState);
                Console.WriteLine($"BlinkState Updated: Index={i}, ChangeState={changeState}");
            }
        }

        public async Task UpdateBlinkStatesAsync(int startIndex, int endIndex, int changeState)
        {
            // Clamp the indices to valid values
            startIndex = Math.Max(startIndex, 0);
            endIndex = Math.Min(_blinkStates.Count - 1, endIndex);

            // Handle the case where no valid range is provided
            if (startIndex > endIndex)
            {
                Console.WriteLine($"Invalid range: startIndex={startIndex}, endIndex={endIndex}");
                return;
            }

            // Handle single-bar updates (startIndex == endIndex)
            if (startIndex == endIndex)
            {
                Console.WriteLine($"Blinking single bar at index: {startIndex}");

                // Blink the single bar asynchronously before updating it
                await HandleBlinkingAsync(startIndex);

                // Update the state for the single bar after blinking
                _blinkStates[startIndex] = (_blinkStates[startIndex].Bar, changeState);
                Console.WriteLine($"BlinkState Updated: Index={startIndex}, ChangeState={changeState}");
            }
            else
            {
                // Handle multi-bar updates (range from startIndex to endIndex)
                Console.WriteLine($"Updating blink states from {startIndex} to {endIndex} with changeState={changeState}");

                for (int i = startIndex; i <= endIndex; i++)
                {
                    _blinkStates[i] = (_blinkStates[i].Bar, changeState);
                    Console.WriteLine($"BlinkState Updated: Index={i}, ChangeState={changeState}");
                }
            }
        }

        public int BlinkStateCount
        {
            get => _blinkStates.Count; // Returns the number of rectangles in the Statbar
        }


        public async Task HandleBlinkingAsync(int achievedCount, bool isDecrementing = false)
        {
            int flashInterval = 400; // Flash interval in milliseconds

            // List to hold tasks for each bar's blinking logic
            var blinkingTasks = new List<Task>();

            // Flashing Logic
            for (int i = 0; i < _blinkStates.Count; i++)
            {
                var (bar, state) = _blinkStates[i];
                Console.WriteLine($"Index: {i}, State: {state}, AchievedCount: {achievedCount}");

                if (state == 1 && i <= achievedCount) // Incrementing Bars
                {
                    blinkingTasks.Add(FlashBarAsync(bar, Colors.White, Colors.Black, flashInterval));
                }
                else if (state == -1) // Decrementing Bars
                {
                    blinkingTasks.Add(FlashBarAsync(bar, Colors.Red, Colors.Black, flashInterval));
                }
            }

            // Wait for all blinking tasks to complete
            await Task.WhenAll(blinkingTasks);

            // Finalization Logic
            for (int i = 0; i < _blinkStates.Count; i++)
            {
                var (bar, state) = _blinkStates[i];

                if (state == 1) // Finalize incrementing bars to green
                {
                    bar.Fill = new SolidColorBrush(Colors.Green);
                }
                else if (state == -1) // Finalize decrementing bars to black
                {
                    bar.Fill = new SolidColorBrush(Colors.Black);
                }
                else if (i < achievedCount) // Already achieved bars remain green
                {
                    bar.Fill = new SolidColorBrush(Colors.Green);
                }
            }

            ResetBlinkStates(); // Reset all states to default
        }

        public async Task BlinkOnceAsync(int index)
        {
            int flashInterval = 300; // 30ms interval

            if (index >= 0 && index < _blinkStates.Count)
            {
                var (bar, state) = _blinkStates[index];

                // Blink: Black -> White
                bar.Fill = new SolidColorBrush(Colors.White);
                await Task.Delay(flashInterval);
                bar.Fill = new SolidColorBrush(Colors.Black);
                await Task.Delay(flashInterval);

                // Finalize: Solid Green
                bar.Fill = new SolidColorBrush(Colors.Green);
                ResetBlinkStates(); // Reset state for this bar
            }
        }

        private async Task FlashBarAsync(Rectangle bar, Color firstColor, Color secondColor, int interval)
        {
            for (int j = 0; j < 3; j++) // 6 flashes
            {
                bar.Fill = new SolidColorBrush(firstColor);
                await Task.Delay(interval);
                bar.Fill = new SolidColorBrush(secondColor);
                await Task.Delay(interval);
            }
        }



        public void ResetBlinkStates()
        {
            for (int i = 0; i < _blinkStates.Count; i++)
            {
                _blinkStates[i] = (_blinkStates[i].Bar, 0); // Reset state to 0
            }
        }

        private Dictionary<int, bool> _blinkingBars = new Dictionary<int, bool>();

        public async Task BlinkContinuouslyAsync(int index)
        {
            if (index < 0 || index >= _blinkStates.Count) return;

            _blinkingBars[index] = true; // Start blinking for this bar
            var (bar, state) = _blinkStates[index];

            while (_blinkingBars.ContainsKey(index) && _blinkingBars[index])
            {
                bar.Fill = new SolidColorBrush(Colors.White); // Blink to white
                await Task.Delay(300); // 300ms interval for blinking
                bar.Fill = new SolidColorBrush(Colors.Black); // Blink to black
                await Task.Delay(300); // 300ms interval for blinking
            }
        }
        public void StopBlinking(int index)
        {
            if (index < 0 || index >= _blinkStates.Count) return;

            _blinkingBars[index] = false; // Stop blinking
            var (bar, state) = _blinkStates[index];
            bar.Fill = new SolidColorBrush(Colors.Green); // Finalize to solid green
        }

    }
}
