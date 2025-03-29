using System;
using System.Collections.Generic;

namespace PocketCat.DefaultData
{
    public static class ActionDataService
    {
        private static readonly Random _random = new Random();
        // Method for lists with lastIndex parameter
        public static string GetRandomItem(List<string> items, int? lastIndex = null)
        {
            if (items == null || items.Count == 0)
                return "No items available."; // Handle empty or null lists

            // If lastIndex is null, default to the maximum index (items.Count - 1)
            int maxIndex = lastIndex.HasValue ? Math.Min(lastIndex.Value, items.Count - 1) : items.Count - 1;

            // Generate a random index within the allowed range
            int randomIndex = _random.Next(0, maxIndex + 1); // Inclusive of 0 to maxIndex
            return items[randomIndex];
        }

        // Method for dictionaries with lastIndex parameter
        public static string GetRandomItem(Dictionary<string, string> items, int? lastIndex = null)
        {
            if (items == null || items.Count == 0)
                return "No items available."; // Handle empty or null dictionaries

            var keys = new List<string>(items.Keys); // Convert dictionary keys to a list

            // If lastIndex is null, default to the maximum index (keys.Count - 1)
            int maxIndex = lastIndex.HasValue ? Math.Min(lastIndex.Value, keys.Count - 1) : keys.Count - 1;

            // Generate a random index within the allowed range
            string randomKey = keys[_random.Next(0, maxIndex + 1)]; // Inclusive of 0 to maxIndex
            return items[randomKey];
        }
    }
}
