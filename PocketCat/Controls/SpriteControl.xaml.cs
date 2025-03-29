using System;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using PocketCat.StaticGame;

namespace PocketCat.Controls
{
    public partial class SpriteControl : UserControl
    {
        private BitmapSource _originalSpritesheet; // The unaltered original spritesheet
        private BitmapSource _activeSpritesheet;  // The modified spritesheet displayed in the control
        private Int32Rect _currentFrame;   // Current frame to display
        private DispatcherTimer _animationTimer; // Timer for animation
        private int _currentFrameIndex;   // Current frame number
        private const int FrameWidth = 128;
        private const int FrameHeight = 128;
        private const int FrameSpacing = 8;
        private const int BaseFrames = 8;
        private bool _isAnimating = true;
        private int _animationDelay = 150; // Default value

        public int AnimationDelay
        {
            get { return _animationDelay; }
            set
            {
                if (value > 0) // Ensure the delay is positive
                {
                    _animationDelay = value;
                }
            }
        }

        private AppSettings _appSettings;

        public SpriteControl() : base()
        {
            _appSettings = (AppSettings)Application.Current.Resources["AppSettings"];

            // Subscribe to property changes
            _appSettings.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AppSettings.PrimaryColor))
                {
                    UpdatePrimaryColorAsync(_appSettings.PrimaryColor);
                }
                if (e.PropertyName == nameof(AppSettings.SecondaryColor))
                {
                    UpdateSecondaryColorAsync(_appSettings.SecondaryColor);
                }
                if (e.PropertyName == nameof(AppSettings.TertiaryColor))
                {
                    UpdateTertiaryColorAsync(_appSettings.TertiaryColor);
                }
                else if (e.PropertyName == nameof(AppSettings.DetailColor))
                {
                    UpdateDetailColorAsync(_appSettings.DetailColor);
                }
                else if (e.PropertyName == nameof(AppSettings.EyeColor))
                {
                    UpdateEyeColorAsync(_appSettings.EyeColor);
                }
            };

            // Load the spritesheet
            _originalSpritesheet = LoadSpritesheet(); // Load the original
            _activeSpritesheet = _originalSpritesheet; // Start with the original as active

            // Subscribe to OnStateChanged to adjust the Y-offset
            States.OnStateChanged += AdjustYOffsetForState;
            ReapplyAllColors();
            StartAnimationAsync();
        }

        private void AdjustYOffsetForState(string newState)
        {
            // Get the Y-offset for the new state
            int yOffset = States.GetSpriteSheetAnimData(newState)[0];

            if (yOffset == -1)
            {
                Console.WriteLine($"Invalid state '{newState}' received. Defaulting to Idle.");
                yOffset = States.GetSpriteSheetAnimData("Idle")[0];
            }

            // Update the visible frame with the new Y-offset
            SetFrame(_currentFrame.X, yOffset, FrameWidth, FrameHeight);

            Console.WriteLine($"Sprite Y-offset updated for state '{newState}': {yOffset}");
        }


        private async void StartAnimationAsync()
        {
            _currentFrameIndex = 0;

            while (_isAnimating)
            {
                AnimateFrame();

                // Wait asynchronously for the next frame (250ms for 4 frames/second)
                await Task.Delay(AnimationDelay);
            }
        }

        private async void UpdatePrimaryColorAsync(string colorHex)
        {
            if (!IsValidHexColor(colorHex))
            {
                Console.WriteLine($"Invalid PrimaryColor: {colorHex}");
                return;
            }

            try
            {
                // Convert the new color
                System.Windows.Media.Color newColor = ConvertHexToColor(colorHex);
                System.Windows.Media.Color oldColor = System.Windows.Media.Color.FromRgb(0, 0, 255); // Example old color

                // Update the spritesheet asynchronously
                _activeSpritesheet = await Task.Run(() => LoadAndReplaceColor(_originalSpritesheet, oldColor, newColor));

                Console.WriteLine("PrimaryColor updated.");
                ReapplyAllColors();
                InvalidateVisual(); // Trigger a redraw
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating PrimaryColor: {ex.Message}");
            }
        }

        private async void UpdateDetailColorAsync(string colorHex)
        {
            if (!IsValidHexColor(colorHex))
            {
                Console.WriteLine($"Invalid DetailColor: {colorHex}");
                return;
            }

            try
            {
                // Convert new color
                System.Windows.Media.Color newColor = ConvertHexToColor(colorHex);
                System.Windows.Media.Color oldColor = System.Windows.Media.Color.FromRgb(255, 0, 255); // Example old color

                // Update spritesheet asynchronously
                _activeSpritesheet = await Task.Run(() => LoadAndReplaceColor(_originalSpritesheet, oldColor, newColor));

                Console.WriteLine("DetailColor updated asynchronously.");
                ReapplyAllColors(); // Reapply for cumulative changes
                InvalidateVisual(); // Trigger a redraw
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating DetailColor: {ex.Message}");
            }
        }

        private async void UpdateEyeColorAsync(string colorHex)
        {
            if (!IsValidHexColor(colorHex))
            {
                Console.WriteLine($"Invalid EyeColor: {colorHex}");
                return;
            }

            try
            {
                // Convert new color
                System.Windows.Media.Color newColor = ConvertHexToColor(colorHex);
                System.Windows.Media.Color oldColor = System.Windows.Media.Color.FromRgb(255, 0, 0); // Example old color

                // Update spritesheet asynchronously
                _activeSpritesheet = await Task.Run(() => LoadAndReplaceColor(_originalSpritesheet, oldColor, newColor));

                Console.WriteLine("EyeColor updated asynchronously.");
                ReapplyAllColors(); // Reapply for cumulative changes
                InvalidateVisual(); // Trigger a redraw
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating EyeColor: {ex.Message}");
            }
        }

        private async void UpdateSecondaryColorAsync(string colorHex)
        {
            if (!IsValidHexColor(colorHex))
            {
                Console.WriteLine($"Invalid SecondaryColor: {colorHex}");
                return;
            }

            try
            {
                // Convert the new color
                System.Windows.Media.Color newColor = ConvertHexToColor(colorHex);
                System.Windows.Media.Color oldColor = System.Windows.Media.Color.FromRgb(0, 255, 255); // Example old color

                // Update the spritesheet asynchronously
                _activeSpritesheet = await Task.Run(() => LoadAndReplaceColor(_originalSpritesheet, oldColor, newColor));

                Console.WriteLine("SecondaryColor updated asynchronously.");
                ReapplyAllColors();
                InvalidateVisual(); // Trigger a redraw
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating SecondaryColor: {ex.Message}");
            }
        }

        private async void UpdateTertiaryColorAsync(string colorHex)
        {
            if (!IsValidHexColor(colorHex))
            {
                Console.WriteLine($"Invalid TertiaryColor: {colorHex}");
                return;
            }

            try
            {
                // Convert the new color
                System.Windows.Media.Color newColor = ConvertHexToColor(colorHex);
                System.Windows.Media.Color oldColor = System.Windows.Media.Color.FromRgb(0, 255, 0); // Example old color

                // Update the spritesheet asynchronously
                _activeSpritesheet = await Task.Run(() => LoadAndReplaceColor(_originalSpritesheet, oldColor, newColor));

                Console.WriteLine("TertiaryColor updated asynchronously.");
                ReapplyAllColors();
                InvalidateVisual(); // Trigger a redraw
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating TertiaryColor: {ex.Message}");
            }
        }


        private bool IsValidHexColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex) || !hex.StartsWith("#") || hex.Length != 7)
            {
                return false;
            }

            // Ensure the remaining characters are valid hex digits
            for (int i = 1; i < hex.Length; i++)
            {
                char c = hex[i];
                if (!Uri.IsHexDigit(c))
                {
                    return false;
                }
            }

            return true;
        }


        public BitmapSource Spritesheet
        {
            get => _activeSpritesheet;
            set
            {
                _activeSpritesheet = value;
                InvalidateVisual(); // Redraw when spritesheet changes
            }
        }

        public Int32Rect CurrentFrame
        {
            get => _currentFrame;
            set
            {
                _currentFrame = value;
                InvalidateVisual(); // Redraw when the frame changes
            }
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_activeSpritesheet != null)
            {
                // Set the scaling mode to NearestNeighbor
                RenderOptions.SetBitmapScalingMode(_activeSpritesheet, BitmapScalingMode.NearestNeighbor);

                // Draw the specific region (current frame) of the active spritesheet
                drawingContext.DrawImage(
                    new CroppedBitmap(_activeSpritesheet, _currentFrame),
                    new Rect(0, 0, _currentFrame.Width, _currentFrame.Height)
                );
            }
        }



        private string DetermineCurrentState()
        {
            if (States.CurrentState != null)
                return States.CurrentState;
            if (Flags.IsSleeping)
                return "Sleeping";
            return "Idle"; // Default state
        }


        private void StartAnimation()
        {
            _currentFrameIndex = 0;

            // Set up the animation timer
            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(250); // 4 frames per second = 250ms per frame
            _animationTimer.Tick += (sender, e) => AnimateFrame();
            _animationTimer.Start();
        }

        private void AnimateFrame()
        {
            // Retrieve the current cat state and its corresponding Y-offset from States
            string currentState = DetermineCurrentState(); // A method to determine the current state
            int yOffset = States.GetSpriteSheetAnimData(currentState)[0]; // Get the Y-offset for the current state
            if (States.GetSpriteSheetAnimData(currentState)[2] > 0)
            {
                int tempDelay = States.GetSpriteSheetAnimData(currentState)[2];
                AnimationDelay = tempDelay;
                if (_currentFrameIndex > States.GetSpriteSheetAnimData(currentState)[1])
                    _currentFrameIndex = 0;
            }

            if (yOffset == -1)
            {
                Console.WriteLine($"Invalid state: {currentState}. Defaulting to Idle.");
                yOffset = States.GetSpriteSheetAnimData("Idle")[0]; // Default to Idle if state is invalid
            }

            // Calculate the new X position based on the current frame index
            int startX = 4; // Horizontal offset
            int x = startX + (_currentFrameIndex * (FrameWidth + FrameSpacing));

            // Use the Y-offset determined by the current state
            int y = yOffset;
            // Update the visible region
            SetFrame(x, y, FrameWidth, FrameHeight);
            int frameCount = States.GetSpriteSheetAnimData(currentState)[1] != 0 ? States.GetSpriteSheetAnimData(currentState)[1] : BaseFrames; // Get the frame count for the current state
            // Increment the frame index and loop back after reaching the last frame
            _currentFrameIndex = (_currentFrameIndex + 1) % frameCount;
        }


        public void SetFrame(int x, int y, int width, int height)
        {
            CurrentFrame = new Int32Rect(x, y, width, height);
        }

        public BitmapSource LoadSpritesheet()
        {
            Bitmap spritesheetBitmap = GetPNGFromResources("SpriteSheet");
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                spritesheetBitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );
            return bitmapSource;
        }

        private System.Windows.Media.Color ConvertHexToColor(string hex)
        {
            if (!hex.StartsWith("#"))
            {
                throw new FormatException("Invalid hex color format. It should start with '#'.");
            }

            // Parse the hex color (e.g., #RRGGBB)
            byte r = Convert.ToByte(hex.Substring(1, 2), 16);
            byte g = Convert.ToByte(hex.Substring(3, 2), 16);
            byte b = Convert.ToByte(hex.Substring(5, 2), 16);

            return System.Windows.Media.Color.FromRgb(r, g, b);
        }

        private Bitmap GetPNGFromResources(string resourceName)
        {
            var resourceManager = new ResourceManager("PocketCat.Properties.Resources", typeof(CatTrayIcon).Assembly);
            var resourceCulture = System.Globalization.CultureInfo.CurrentCulture;

            var iconBytes = resourceManager.GetObject(resourceName, resourceCulture) as byte[];
            if (iconBytes == null)
            {
                throw new Exception($"Resource '{resourceName}' not found or is not valid.");
            }

            using (var stream = new MemoryStream(iconBytes))
            {
                return new Bitmap(stream);
            }
        }
        public BitmapSource LoadAndReplaceColor(BitmapSource source, System.Windows.Media.Color oldColor, System.Windows.Media.Color newColor)
        {
            BitmapSource writableBitmapSource = null;

            // Ensure the operation runs on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Convert the source image into a WriteableBitmap for pixel manipulation
                WriteableBitmap writableBitmap = new WriteableBitmap(source);

                // Lock the bitmap for writing
                writableBitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer
                    IntPtr pBackBuffer = writableBitmap.BackBuffer;

                    for (int y = 0; y < writableBitmap.PixelHeight; y++)
                    {
                        for (int x = 0; x < writableBitmap.PixelWidth; x++)
                        {
                            // Calculate the offset for the pixel
                            int pixelOffset = (y * writableBitmap.BackBufferStride) + (x * 4);

                            // Get the color components of the pixel
                            byte* pPixel = (byte*)pBackBuffer + pixelOffset;
                            byte blue = pPixel[0]; // Blue channel
                            byte green = pPixel[1]; // Green channel
                            byte red = pPixel[2];   // Red channel
                            byte alpha = pPixel[3]; // Alpha channel

                            // Check if the pixel matches the old color
                            if (red == oldColor.R && green == oldColor.G && blue == oldColor.B)
                            {
                                // Replace with the new color
                                pPixel[0] = newColor.B; // Blue
                                pPixel[1] = newColor.G; // Green
                                pPixel[2] = newColor.R; // Red
                            }
                        }
                    }
                }

                // Unlock the bitmap
                writableBitmap.Unlock();

                // Return the updated bitmap
                writableBitmapSource = writableBitmap;
            });

            return writableBitmapSource;
        }

        private void ReapplyAllColors()
        {
            // Start with the original spritesheet
            _activeSpritesheet = _originalSpritesheet;

            // Apply PrimaryColor if valid
            if (IsValidHexColor(_appSettings.PrimaryColor))
            {
                System.Windows.Media.Color primaryColor = ConvertHexToColor(_appSettings.PrimaryColor);
                System.Windows.Media.Color oldPrimaryColor = System.Windows.Media.Color.FromRgb(0, 0, 255); // Example old color
                _activeSpritesheet = LoadAndReplaceColor(_activeSpritesheet, oldPrimaryColor, primaryColor);
            }

            // Apply SecondaryColor if valid
            if (IsValidHexColor(_appSettings.SecondaryColor))
            {
                System.Windows.Media.Color secondaryColor = ConvertHexToColor(_appSettings.SecondaryColor);
                System.Windows.Media.Color oldSecondaryColor = System.Windows.Media.Color.FromRgb(0, 255, 255); // Example old color
                _activeSpritesheet = LoadAndReplaceColor(_activeSpritesheet, oldSecondaryColor, secondaryColor);
            }

            // Apply TertiaryColor if valid
            if (IsValidHexColor(_appSettings.TertiaryColor))
            {
                System.Windows.Media.Color tertiaryColor = ConvertHexToColor(_appSettings.TertiaryColor);
                System.Windows.Media.Color oldTertiaryColor = System.Windows.Media.Color.FromRgb(0, 255, 0); // Example old color
                _activeSpritesheet = LoadAndReplaceColor(_activeSpritesheet, oldTertiaryColor, tertiaryColor);
            }

            // Apply DetailColor if valid
            if (IsValidHexColor(_appSettings.DetailColor))
            {
                System.Windows.Media.Color detailColor = ConvertHexToColor(_appSettings.DetailColor);
                System.Windows.Media.Color oldDetailColor = System.Windows.Media.Color.FromRgb(255, 0, 255); // Example old color
                _activeSpritesheet = LoadAndReplaceColor(_activeSpritesheet, oldDetailColor, detailColor);
            }

            // Apply EyeColor if valid
            if (IsValidHexColor(_appSettings.EyeColor))
            {
                System.Windows.Media.Color eyeColor = ConvertHexToColor(_appSettings.EyeColor);
                System.Windows.Media.Color oldEyeColor = System.Windows.Media.Color.FromRgb(255, 0, 0); // Example old color
                _activeSpritesheet = LoadAndReplaceColor(_activeSpritesheet, oldEyeColor, eyeColor);
            }

            // Trigger a redraw
            InvalidateVisual();
        }


    }
}
