using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Resources;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PocketCat.Controls.Windows.Shop
{
    public class ShopItems
    {
        public ObservableCollection<ShopItemViewModel> Items { get; set; }

        private readonly Dictionary<string, double> FoodDictionary = new Dictionary<string, double>
        {
            { "Wet Food", 1.50 },
            { "Fish", 3.00 },
        };
        private readonly Dictionary<string, double> ItemsDictionary = new Dictionary<string, double>
        {
            { "Box", 0.75 },
            { "Toy Mouse", 1.75 },
            { "Catnip", 3.00 },
        };

        public ShopItems()
        {
            Items = new ObservableCollection<ShopItemViewModel>();

            foreach (var kvp in FoodDictionary)
            {
                Items.Add(new ShopItemViewModel
                {
                    Image = GetPNGFromResources(kvp.Key),
                    Name = kvp.Key,
                    Price = kvp.Value, // Assign price directly as double
                    BackgroundBrush = new SolidColorBrush(Colors.Transparent)
                });
            }
            foreach (var kvp in ItemsDictionary)
            {
                Items.Add(new ShopItemViewModel
                {
                    Image = GetPNGFromResources(kvp.Key),
                    Name = kvp.Key,
                    Price = kvp.Value, // Assign price directly as double
                    BackgroundBrush = new SolidColorBrush(Colors.Transparent)
                });
            }
        }


        private BitmapImage GetPNGFromResources(string resourceName)
        {
            var resourceManager = new ResourceManager("PocketCat.Properties.Resources", typeof(ShopItems).Assembly);
            var resourceCulture = System.Globalization.CultureInfo.CurrentCulture;

            var iconBytes = resourceManager.GetObject(resourceName, resourceCulture) as byte[];

            // If the resource does not exist, attempt to grab the "heart" resource
            if (iconBytes == null)
            {
                iconBytes = resourceManager.GetObject("Heart", resourceCulture) as byte[];
                if (iconBytes == null)
                {
                    throw new Exception($"Resource '{resourceName}' and fallback 'heart' not found or are not valid.");
                }
            }

            using (var stream = new MemoryStream(iconBytes))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

    }
}
