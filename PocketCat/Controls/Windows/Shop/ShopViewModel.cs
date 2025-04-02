using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PocketCat.Controls.Windows.Shop;
using PocketCat.StaticGame.Items;

namespace PocketCat.Controls.Windows.Models
{
    public class ShopViewModel
    {
        public Money Money;
        public ShopItems ShopItems;

        public ShopViewModel()
        {
            Money = new Money();
            ShopItems = new ShopItems();

        }
    }
}
