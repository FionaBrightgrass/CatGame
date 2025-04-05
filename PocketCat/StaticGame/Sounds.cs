using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PocketCat.StaticGame
{
    public static class Sounds
    {
        private static MediaPlayer _player;
        public static void PlaySound(string audioFilePath)
        {
            try
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(audioFilePath);
                player.Play(); // Use PlaySync() to block until playback finishes
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
            }
        }
    }
}
