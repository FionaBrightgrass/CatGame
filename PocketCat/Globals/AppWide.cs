using System.IO;

namespace PocketCat.Globals
{
    public static class AppWide
    {
        public static string ApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PocketCat");
        public static bool CancelAction = false;
    }
}
