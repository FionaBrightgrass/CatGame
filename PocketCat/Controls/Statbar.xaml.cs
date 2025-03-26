using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PocketCat.StaticGame;
using PocketCat.StaticGame.Stats;

namespace PocketCat.Controls
{
    public partial class Statbar : UserControl
    {
        // Holds the states of all rectangles (bars) in this Statbar
        private List<(Rectangle Bar, int State)> _blinkStates = new();
        public MainWindow ParentWindow { get; set; }
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
            if (_blinkStates == null || _blinkStates.Count == 0)
            {
                Console.WriteLine("Error: Blink states are empty or uninitialized.");
                return;
            }

            startIndex = Math.Max(startIndex, 0);
            endIndex = Math.Min(_blinkStates.Count - 1, endIndex);

            if (startIndex > endIndex)
            {
                Console.WriteLine($"Invalid range after clamping: startIndex={startIndex}, endIndex={endIndex}");
                return;
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                _blinkStates[i] = (_blinkStates[i].Bar, changeState); // Explicitly update the state
                Console.WriteLine($"BlinkState Updated: Index={i}, ChangeState={changeState}");
            }
        }

        public int BlinkStateCount
        {
            get => _blinkStates.Count; // Returns the number of rectangles in the Statbar
        }

        private HashSet<int> _blinkedIndices = new HashSet<int>(); // Track indices already blinked

        bool isBlinking = false;
        public async Task HandleBlinkingAsync(int startIndex, int endIndex, bool isDecrementing = false)
        {
            if (isBlinking)
                return;
            isBlinking = true;
            int flashInterval = 500;

            startIndex = Math.Max(startIndex, 0);
            endIndex = Math.Min(_blinkStates.Count - 1, endIndex);

            if (startIndex > endIndex) return;

            var blinkingTasks = new List<Task>();

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (_blinkedIndices.Contains(i)) continue; // Skip if already blinked
                _blinkedIndices.Add(i);

                var (bar, state) = _blinkStates[i];
                Console.WriteLine($"Blinking Bar Index: {i}, State: {state}");

                if (state == 1)
                    blinkingTasks.Add(FlashBarAsync(bar, Colors.White, Colors.Black, flashInterval));
                else if (state == -1 && isDecrementing)
                    blinkingTasks.Add(FlashBarAsync(bar, Colors.Red, Colors.Black, flashInterval));
            }
            await Task.WhenAll(blinkingTasks);
            for (int i = startIndex; i <= endIndex -1; i++)
            {
                var (bar, state) = _blinkStates[i];

                if (state == 1)
                    bar.Fill = new SolidColorBrush(Colors.Green);
                else if (state == -1 && isDecrementing)
                    bar.Fill = new SolidColorBrush(Colors.Black);
            }
            ResetBlinkStates(0, 9);
            _blinkedIndices.Clear(); // Reset blinked indices for the next operation
            isBlinking = false;
        }



        private async Task FlashBarAsync(Rectangle bar, Color firstColor, Color secondColor, int interval)
        {
            for (int j = 0; j < 6; j++) // 6 flashes
            {
                bar.Fill = new SolidColorBrush(firstColor);
                await Task.Delay(interval);
                bar.Fill = new SolidColorBrush(secondColor);
                await Task.Delay(interval);
            }
        }



        public void ResetBlinkStates(int startIndex, int endIndex)
        {
            startIndex = Math.Max(startIndex, 0);
            endIndex = Math.Min(_blinkStates.Count - 1, endIndex);

            if (startIndex > endIndex) return;

            for (int i = startIndex; i <= endIndex; i++)
            {
                _blinkStates[i] = (_blinkStates[i].Bar, 0); // Reset state to 0 for affected range
            }

            Console.WriteLine($"Blink states reset for range: startIndex={startIndex}, endIndex={endIndex}");
        }


        private Dictionary<int, bool> _blinkingBars = new Dictionary<int, bool>();

        public async Task BlinkContinuouslyAsync(int index, int durationInMinutes)
        {
            if (index < 0 || index >= _blinkStates.Count) return;

            _blinkingBars[index] = true; // Start blinking for this bar
            var (bar, state) = _blinkStates[index];

            // Calculate the time when blinking should stop
            var endTime = DateTime.Now.AddMinutes(durationInMinutes);

            while (_blinkingBars.ContainsKey(index) && _blinkingBars[index] && DateTime.Now < endTime)
            {
                bar.Fill = new SolidColorBrush(Colors.White); // Blink to white
                await Task.Delay(300); // 300ms interval for blinking
                bar.Fill = new SolidColorBrush(Colors.Black); // Blink to black
                await Task.Delay(300); // 300ms interval for blinking
            }

            // Ensure the bar stops in a final state (e.g., black)
            if (index >= 0 && index < _blinkStates.Count)
            {
                bar.Fill = new SolidColorBrush(Colors.Black);
            }

            // Stop the blinking explicitly when the time has elapsed
            _blinkingBars[index] = false;
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
