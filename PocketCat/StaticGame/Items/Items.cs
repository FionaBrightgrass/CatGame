using System;
using System.Collections.Generic;
using System.IO;

namespace PocketCat.StaticGame.Items
{
    public class Items
    {
        private readonly string ItemsFile = Globals.AppWide.ApplicationData + "/items.txt";
        private readonly Dictionary<string, Item> _ownedItems;

        public Items()
        {
            _ownedItems = new Dictionary<string, Item>();
            LoadOwnedItems(); // Initialize from file
        }

        // Method to get all owned items
        public IEnumerable<Item> GetAllOwnedItems()
        {
            return _ownedItems.Values;
        }

        // Add or Update an Item
        public void AddOrUpdateItem(string itemName, int quantity)
        {
            if (_ownedItems.ContainsKey(itemName))
            {
                _ownedItems[itemName].Quantity += quantity; // Increment quantity
            }
            else
            {
                _ownedItems[itemName] = new Item { Name = itemName, Quantity = quantity }; // Add new item
            }

            SaveOwnedItems(); // Persist changes to file
        }

        // Remove an Item
        public void RemoveItem(string itemName)
        {
            if (_ownedItems.ContainsKey(itemName))
            {
                _ownedItems.Remove(itemName);
                SaveOwnedItems(); // Persist changes to file
            }
        }

        // Load Items from file
        private void LoadOwnedItems()
        {
            if (!File.Exists(ItemsFile))
            {
                File.Create(ItemsFile).Close(); // Create file if it doesn't exist
                return;
            }

            var lines = File.ReadAllLines(ItemsFile);
            foreach (var line in lines)
            {
                if (line.Contains(":"))
                {
                    var parts = line.Split(':');
                    string name = parts[0].Trim();
                    if (int.TryParse(parts[1].Trim(), out int quantity))
                    {
                        _ownedItems[name] = new Item { Name = name, Quantity = quantity };
                    }
                }
            }
        }

        // Save Items to file
        private void SaveOwnedItems()
        {
            var lines = new List<string>();
            foreach (var item in _ownedItems.Values)
            {
                lines.Add($"{item.Name}:{item.Quantity}");
            }

            File.WriteAllLines(ItemsFile, lines);
            Console.WriteLine("Owned items saved to file.");
        }
    }

    // Representation of an individual item
    public class Item
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
