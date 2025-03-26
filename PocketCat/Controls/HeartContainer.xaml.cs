using System.Drawing; // For Bitmap
using System.IO; // For MemoryStream
using System.Windows.Media.Imaging; // For BitmapImage
using System.Resources;
using System.Windows.Controls; // For ResourceManager

namespace PocketCat.Controls
{
    public partial class HeartContainer : UserControl
    {
        public HeartContainer()
        {
            InitializeComponent();
            LoadHeartImage();
        }

        private void LoadHeartImage()
        {
            try
            {
                string resourceName = "Heart"; // Resource name for the heart image
                Bitmap heartBitmap = GetPNGFromResources(resourceName);

                // Convert Bitmap to BitmapImage
                BitmapImage bitmapImage = ConvertBitmapToBitmapImage(heartBitmap);

                // Set the Image source to display the heart image
                HeartImage.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading heart image: {ex.Message}");
            }
        }

        private Bitmap GetPNGFromResources(string resourceName)
        {
            var resourceManager = new ResourceManager("PocketCat.Properties.Resources", typeof(HeartContainer).Assembly);
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

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Save the bitmap to a memory stream
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Create a BitmapImage from the stream
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
