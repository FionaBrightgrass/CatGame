using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PocketCat.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ActionText : TextBlock
    {
        private CancellationTokenSource _cancellationTokenSource;
        public ActionText() : base()
        {
        }
        public async Task StartTextWritingAsync(string customText, double lettersPerSecond, CancellationToken cancellationToken)
        {
            // Perform replacements in the text
            customText = customText.Replace("#NAME#", DefaultData.ActionData.DataModel.Name)
                                   .Replace("#PRONOUNONE#", DefaultData.ActionData.DataModel.PronounOne)
                                   .Replace("#PRONOUNTWO#", DefaultData.ActionData.DataModel.PronounTwo)
                                   .Replace("#PRONOUNONELOWER#", DefaultData.ActionData.DataModel.PronounOneLower)
                                   .Replace("#PRONOUNTWOLOWER#", DefaultData.ActionData.DataModel.PronounTwoLower)
                                   .Replace("#PRONOUNTHREE#", DefaultData.ActionData.DataModel.PronounThree)
                                   .Replace("#PRONOUNTHREELOWER#", DefaultData.ActionData.DataModel.PronounThreeLower);

            // Clear the existing text in the TextBlock
            Inlines.Clear();

            double delay = 1000 / lettersPerSecond;

            for (int i = 0; i < customText.Length; i++)
            {
                // Check for cancellation before processing each letter
                if (cancellationToken.IsCancellationRequested)
                {
                    return; // Exit gracefully if cancellation is requested
                }

                var letterRun = new Run(customText[i].ToString());
                Inlines.Add(letterRun);
                FadeInLetter(letterRun);

                await Task.Delay((int)delay, cancellationToken); // Pass the CancellationToken
            }
        }

        public async Task StartTextWritingAsync(string customText, double lettersPerSecond)
        {
            // Cancel any previous task if it exists
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await StartTextWritingAsync(customText, lettersPerSecond, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation gracefully (optional)
            }
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
    }
}
