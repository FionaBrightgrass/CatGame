using System.Windows;
using System.Windows.Media.Animation;

namespace PocketCat.Controls
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CallPopup : Window
    {
        public CallPopup()
        {
            InitializeComponent();
            SetStartLocationAboveMouse();
            this.Loaded += CallPopup_Loaded; // Start the animation when the window loads
        }

        private void CallPopup_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateWindowWithDelay();
        }
        private async void AnimateWindowWithDelay()
        {
            // Wait for 1 second (1000 milliseconds)
            await Task.Delay(1000);

            // Now run the original animation logic
            AnimateWindow();
        }
        private void AnimateWindow()
        {
            SetStartLocationAboveMouse();
            // Set animation duration (3 seconds)
            Duration animationDuration = new Duration(TimeSpan.FromSeconds(2.5));

            // Animate the Y position (move up 100 pixels)
            DoubleAnimation moveUpAnimation = new DoubleAnimation
            {
                From = this.Top,
                To = this.Top - 200, // Move up by 100 pixels
                Duration = animationDuration
            };

            // Animate the Opacity (fade out to 0%)
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = 1.0, // Fully visible
                To = 0.0, // Fully invisible
                Duration = new Duration(TimeSpan.FromSeconds(3)),
            };

            // Apply animations
            this.BeginAnimation(Window.TopProperty, moveUpAnimation); // Move the window up
            this.BeginAnimation(Window.OpacityProperty, fadeOutAnimation); // Fade out

            // Close the window after the animations
            fadeOutAnimation.Completed += (s, e) => this.Close();
        }
        private void SetStartLocationAboveMouse()
        {
            // Get the current mouse position in screen coordinates
            var mousePosition = System.Windows.Forms.Cursor.Position;

            // Set the window's position
            this.Left = mousePosition.X - (this.Width / 2); // Center the window horizontally above the mouse
            this.Top = mousePosition.Y - this.Height;       // Position it above the mouse
        }
    }
}