using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PocketCat.Controls.Windows.Shop
{
    public class ShopItemViewModel
    {
        public BitmapImage Image { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string FormattedPrice => $"${Price}";
        public Brush BackgroundBrush { get; set; }
    }

}
