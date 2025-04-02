using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketCat.StaticGame
{
    public static class SharedInventory
    {
        // Centralized dictionary to manage item quantities across windows
        public static Dictionary<string, int> ItemsDictionary { get; set; } = new Dictionary<string, int>();

        // Method to update an item's quantity
        public static void UpdateItem(string itemName, int quantityChange)
        {
            if (ItemsDictionary.ContainsKey(itemName))
            {
                ItemsDictionary[itemName] += quantityChange;

                // Prevent negative quantities
                if (ItemsDictionary[itemName] < 0)
                {
                    ItemsDictionary[itemName] = 0;
                }
            }
            else
            {
                // Add new item with the specified quantity
                ItemsDictionary[itemName] = Math.Max(0, quantityChange);
            }

            Console.WriteLine($"Updated {itemName} quantity to {ItemsDictionary[itemName]}");
        }

        // Method to load inventory from a file
        public static void LoadFromFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine("Items file does not exist. Starting with an empty inventory.");
                    return;
                }

                var lines = System.IO.File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (line.Contains(":"))
                    {
                        var split = line.Split(':');
                        var itemName = split[0].Trim();
                        if (int.TryParse(split[1].Trim(), out int quantity))
                        {
                            ItemsDictionary[itemName] = quantity;
                        }
                    }
                }

                Console.WriteLine("Inventory loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load inventory: {ex.Message}");
            }
        }

        // Method to save inventory to a file
        public static void SaveToFile(string filePath)
        {
            try
            {
                var lines = ItemsDictionary.Select(kvp => $"{kvp.Key}: {kvp.Value}");
                System.IO.File.WriteAllLines(filePath, lines);
                Console.WriteLine("Inventory saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save inventory: {ex.Message}");
            }
        }
    }


    // Define ItemViewModel for binding
    public class ItemViewModel : INotifyPropertyChanged
    {
        private int _quantity;

        public string Name { get; set; }
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                }
            }
        }

        public string Description { get; set; }
        public string Category { get; set; } // New property to specify the category (e.g., "Food", "Toys")

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

