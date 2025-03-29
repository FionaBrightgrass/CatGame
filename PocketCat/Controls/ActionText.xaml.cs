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
        public AppSettings _appSettings;
        private CancellationTokenSource _cancellationTokenSource;
        public ActionText() : base()
        {
        }
        public async Task StartTextWritingAsync(string customText, double lettersPerSecond, CancellationToken cancellationToken)
        {
            string Name = _appSettings.Name;
            string[] pronouns = _appSettings.Pronouns.Split('/');
            string He = "#Set Pronouns in Settings. Format: They/Them/Their";
            string he = "#Set Pronouns in Settings. Format: She/Her/Hers";
            string Him = "#Set Pronouns in Settings. Format: He/Him/His";
            string him = "#Set Pronouns in Settings. Format: They/Them/Their";
            string His = "#Set Pronouns in Settings. Format: She/Her/Hers";
            string his = "#Set Pronouns in Settings. Format: He/Him/His";
            if (pronouns.Length >=1)
            {
                He = pronouns[0];
                he = pronouns[0].ToLower();
            }
            if(pronouns.Length >= 2)
            {
                Him = pronouns[1];
                him = pronouns[1].ToLower();
            }
            if (pronouns.Length >= 3)
            {
                His = pronouns[2];
                his = pronouns[2].ToLower();
            }
            // Perform replacements in the text
            customText = customText.Replace("#NAME#", Name)
                                   .Replace("#PRONOUNONELOWER#", he)
                                   .Replace("#PRONOUNONE#", He)
                                   .Replace("#PRONOUNTWO#", Him)
                                   .Replace("#PRONOUNTWOLOWER#", him)
                                   .Replace("#PRONOUNTHREE#", His)
                                   .Replace("#PRONOUNTHREELOWER#", his);

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
