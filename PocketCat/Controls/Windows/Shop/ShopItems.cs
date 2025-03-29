using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PocketCat.Controls.Windows.Shop
{
    public class ShopItems
    {
        public ObservableCollection<ShopItemViewModel> Items { get; set; }

        public ShopItems()
        {
            Items = new ObservableCollection<ShopItemViewModel>
            {
                new ShopItemViewModel
                {
                    Image = GetPNGFromResources("Heart"),
                    Name = "Wet Food",
                    Price = 2,
                    BackgroundBrush = new SolidColorBrush(Colors.Transparent)
                },
                new ShopItemViewModel
                {
                    Image = GetPNGFromResources("Heart"),
                    Name = "Fish",
                    Price = 3,
                    BackgroundBrush = new SolidColorBrush(Colors.Transparent)
                }
            };
        }

        public void SetItems()
        {
            Items = new ObservableCollection<ShopItemViewModel>
            {
                new ShopItemViewModel
                {
                    Image = GetPNGFromResources("Heart"),
                    Name = "Health Potion",
                    Price = 100
                },
                new ShopItemViewModel
                {
                    Image = GetPNGFromResources("Heart"),
                    Name = "Mana Potion",
                    Price = 150
                }
            };
        }

        private BitmapImage GetPNGFromResources(string resourceName)
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
