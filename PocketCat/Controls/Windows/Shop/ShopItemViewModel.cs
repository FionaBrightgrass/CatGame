using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PocketCat.Controls.Windows.Shop
{
    public class ShopItemViewModel
    {
        public BitmapImage Image { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string FormattedPrice => $"${Price:0.00}";
        public Brush BackgroundBrush { get; set; }
    }

}
