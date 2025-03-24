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
        public ActionText() : base()
        {
        }
        private bool textActionInProgress = false;
        public bool TextActionInProgress
        {
            get => textActionInProgress; // Getter
            set => textActionInProgress = value; // Setter
        }
        public async Task WriteTextWithLetterFadeInAsync(string customText, double lettersPerSecond, string name, string pronounOne, string pronounTwo, string pronounOneLower, string pronounTwoLower)
        {
            if (textActionInProgress)
                return;

            textActionInProgress = true;

            // Perform replacements in the text
            customText = customText.Replace("#NAME#", name)
                                   .Replace("#PRONOUNONE#", pronounOne)
                                   .Replace("#PRONOUNTWO#", pronounTwo)
                                   .Replace("#PRONOUNONELOWER#", pronounOneLower)
                                   .Replace("#PRONOUNTWOLOWER#", pronounTwoLower);

            // Clear the existing text in the TextBlock
            Inlines.Clear();

            // Determine the delay per letter
            double delay = 1000 / lettersPerSecond;

            // Iterate over each letter and add it with animation
            for (int i = 0; i < customText.Length; i++)
            {
                var letterRun = new Run(customText[i].ToString());
                Inlines.Add(letterRun); // Add the letter to the TextBlock
                FadeInLetter(letterRun); // Animate the letter's opacity

                await Task.Delay((int)delay); // Wait before adding the next letter
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
    }
}
