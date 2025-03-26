using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PocketCat.StaticGame
{
    public static class States
    {
        private static bool _isSleeping;
        public static bool IsSleeping
        {
            get => _isSleeping;
            set
            {
                if (_isSleeping != value)
                {
                    _isSleeping = value;
                    Console.WriteLine($"IsSleeping state changed to: {_isSleeping}");
                }
            }
        }

        private static bool _wantsPets;
        public static bool WantsPets
        {
            get => _wantsPets;
            set
            {
                if (_wantsPets != value)
                {
                    _wantsPets = value;
                    Console.WriteLine($"WantsPets state changed to: {_wantsPets}");
                }
            }
        }

        private static string _currentState = "Idle";
        private static CancellationTokenSource _resetTokenSource; // Token to cancel pending reset tasks
        public static string CurrentState
        {
            get => _currentState;
            set
            {
                if (SpriteSheetData.ContainsKey(value)) // Check if the key exists
                {
                    if (_currentState != value)
                    {
                        string previousState = _currentState; // Track the previous state
                        _currentState = value;
                        Console.WriteLine($"CurrentState changed to: {_currentState}");
                        OnStateChanged?.Invoke(_currentState); // Notify subscribers

                        // Start or stop the state reset task based on the new state
                        if (_currentState != "Idle" && _currentState != "Sleeping" && _currentState != "Tired" && _currentState != "Hungry")
                        {
                            StartStateResetTask(previousState);
                        }
                        else
                        {
                            StopStateResetTask();
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid state: {value}. Defaulting to 'Idle'.");
                    _currentState = "Idle"; // Set to Idle if invalid
                    OnStateChanged?.Invoke(_currentState); // Notify subscribers of the change
                }
            }
        }

        public static event Action<string> OnStateChanged;

        public static readonly Dictionary<string, int[]> SpriteSheetData = new Dictionary<string, int[]>
        {
                              //{Offset, FrameCount, FrameDisplayTimeIn:ms}
            { "Idle", new int[] { 4, 10, 150 } },          // Starts at 4px
            { "WalkLeft", new int[] { 144, 8, 150 } },    // Idle + 140px
            { "WalkRight", new int[] { 284, 8, 150 } },   // WalkLeft + 140px
            { "Dry Food", new int[] { 424, 15, 150 } },      // WalkRight + 140px
            { "Wet Food", new int[] { 424, 15, 150 } },      // WalkRight + 140px
            { "Fish", new int[] { 424, 15, 150 } },      // WalkRight + 140px
            { "Sleeping", new int[] { 564, 8, 400 } },    // Eating + 140px
            { "Toy Mouse", new int[] { 704, 9, 250 } },      // Sleeping + 140px
            { "Catnip", new int[] { 844, 4, 150 } },      // Toy Mouse + 140px
            { "Box", new int[] { 984, 8, 150 } },     // Catnip + 140px
            { "Tired", new int[] { 1124, 10, 150 }  },// Box + 140px
            { "Hungry", new int[] { 1124, 10, 150 }  }// Box + 140px
        };



        public static int[] GetSpriteSheetAnimData(string state)
        {
            if (SpriteSheetData.TryGetValue(state, out int[] data))
            {
                return data;
            }
            Console.WriteLine($"State '{state}' not found in SpriteSheetOffsets.");
            return new int[] {-1, 8}; // Return -1 for invalid states
        }

        private static async void StartStateResetTask(string initialTrackedState)
        {
            // Cancel any existing reset tasks
            StopStateResetTask();

            // Create a new cancellation token source for the current task
            _resetTokenSource = new CancellationTokenSource();
            CancellationToken token = _resetTokenSource.Token;

            try
            {
                Console.WriteLine($"State reset task started for state: {initialTrackedState}");

                // Wait asynchronously for 10 seconds (or cancel if the token is triggered)
                await Task.Delay(TimeSpan.FromSeconds(10), token);

                // Check if the state has changed independently
                if (initialTrackedState != CurrentState)
                {
                    Console.WriteLine($"State changed independently to '{CurrentState}', reset task canceled.");
                    return;
                }

                // Otherwise, reset the state to "Idle"
                ResetToIdle();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("State reset task was canceled.");
            }
        }

        private static void StopStateResetTask()
        {
            if (_resetTokenSource != null)
            {
                _resetTokenSource.Cancel(); // Cancel the current reset task
                _resetTokenSource.Dispose();
                _resetTokenSource = null;

                Console.WriteLine("State reset task stopped.");
            }
        }

        private static void ResetToIdle()
        {
            StopStateResetTask(); // Stop any ongoing reset tasks
            CurrentState = "Idle"; // Reset the state to Idle
            Console.WriteLine("State has been reset to Idle.");
        }
    }
}
