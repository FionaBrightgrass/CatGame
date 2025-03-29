using System;
using System.Threading.Tasks;

namespace PocketCat.StaticGame.Stats
{
    public class Stat
    {
        // Maximum value for a stat
        public const int MaxValue = 10;

        // Delegate for handling stat changes
        public delegate void StatChangedHandler(string key, int value);

        // Event to notify when a stat changes
        public static event StatChangedHandler StatChanged;

        // Callback for blink bar functionality
        public Func<string, int, bool, bool, Task> BlinkBarCallback { get; set; }

        private string _key;
        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnStatChanged();
                }
            }
        }

        private int _value = 5;
        public int Value
        {
            get => _value;
            set
            {
                // Validate the value to ensure it stays within range (e.g., 0 to MaxValue)
                if (value >= 0 && value <= MaxValue)
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnStatChanged();
                    }
                }
            }
        }

        // Constructor initializing default values
        public Stat(string key)
        {
            Key = key;
            _value = 0; // Default starting value
        }

        // Trigger stat changes and blink bar callback
        private void OnStatChanged()
        {
            StatChanged?.Invoke(Key, Value);

            // Trigger the blink bar callback if defined
            BlinkBarCallback?.Invoke(Key, Value, Value > 0, Value == MaxValue);
        }

        // Example event subscription usage
        public static void SubscribeToStatChanges()
        {
            StatChanged += (key, value) =>
            {
                Console.WriteLine($"Stat changed: {key} = {value}");
            };
        }

        // Method to simulate blink bar logic (can be customized further)
        public async Task BlinkBarAsync(bool isIncrement, bool isMaxed)
        {
            Console.WriteLine($"Blinking bar for '{Key}' | Increment: {isIncrement} | Maxed: {isMaxed}");
            await Task.Delay(100); // Simulate blinking
        }
    }
}
