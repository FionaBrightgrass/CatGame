using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketCat.Controls.Windows.Inventory
{

    public class FoodActionData
    {
        public Dictionary<string, string> DataDict = new Dictionary<string, string>
        {
            { "Dry Food", "Hunger: 3; Happiness: 0; Energy: 0; #NAME# munches on the dry food but doesn't look fully satisfied.; Category: Food; Quantity: Unlimited" },
            { "Wet Food", "Hunger: 4; Happiness: 1; Energy: 0; #NAME# scarfs down the wet food eagerly.; Category: Food; Quantity: 5" },
            { "Fish", "Hunger: 5; Happiness: 2; Energy: 1; #NAME# sniffs the fish momentarily and then chows down!; Category: Food; Quantity: 2" }
        };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int quantity && quantity == int.MaxValue)
            {
                return "∞";
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

  
