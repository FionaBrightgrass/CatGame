using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketCat.Controls.Windows.Inventory
{
    public class ItemActionData
    {
        public Dictionary<string, string> DataDict = new Dictionary<string, string>
    {
        { "Toy Mouse", "Hunger: -1; Happiness: 2; Energy: -1; #NAME# stares curiously at the toy mouse. #PRONOUNONE# doesn't seem to want to hurt it.; BreakChance: 0.25; Category: Toys" },
        { "Catnip", "Hunger: -2; Happiness: 3; Energy: 4; #NAME# stuffs #PRONOUNTHREELOWER# little face with catnip. #PRONOUNONE# seems to be enjoying #PRONOUNTWOLOWER#self.; BreakChance: 1.00; Category: Toys" },
        { "Box", "Hunger: 0; Happiness: 1; Energy: -1; #NAME# climbs into the box. #PRONOUNONE# seems to be enjoying #PRONOUNTWOLOWER#self!; BreakChance: 0.75; Category: Toys" },
    };
    }

}
