using Newtonsoft.Json;

namespace PocketCat.DefaultData
{
    public static class CustomData
    {
        // Dictionaries
        public static Dictionary<string, string> ButtonControl { get; set; } = new Dictionary<string, string>
        {
            //Name
            { "Call", "" },
            { "Feed", "" },
            { "Pet", "" },
            { "Play", "" },
        };
        public static Dictionary<string, List<string>> FeedControl { get; set; } = new Dictionary<string, List<string>>
        {
            //Name               //<Hunger, Affection, Cost>
            { "Dry Food", new List<string> { "1", "0", "0", "The cat munches eagerly on the dry food, but does not look fully satisfied." } },
            { "Wet Food", new List<string> { "2", "1", "2", "The cat scarfs down the wet food." } },
            { "Fish", new List<string> { "1", "2", "5", "The cat sniffs the fish curiously, and then scarfs it down in one bite." } },
            { "Back", new List<string> { "0", "0", "0", "Back" } },
        };
        public static Dictionary<string, List<int>> PlayControl { get; set; } = new Dictionary<string, List<int>>
        {
            //Name               //<Hunger, Affection, Cost>
            { "Toy Mouse", new List<int> { 0, 1, 0 } },
            { "Yarn", new List<int> { 0, 1, 0 } },
            { "Catnip", new List<int> { -1, 2, 2 } },
            { "Back", new List<int> { 0, 0, 0 } },
        };
        public static Dictionary<string, List<int>> SleepControl { get; set; } = new Dictionary<string, List<int>>
        {
            //Name               //<Hunger, Affection, Cost>
            { "Cat Bed", new List<int> { 0, 1, 0 } },
            { "Bed Pillow", new List<int> { 0, 2, 0 } },
            { "Heated Bed", new List<int> { 0, 3, 1 } },
            { "Back", new List<int> { 0, 0, 0 } },
        };
        public static void RepopulateDictionaries(string JSONData)
        {
            try
            {
                // Split the combined JSON string using the delimiter
                string[] jsonParts = JSONData.Split(new[] { "||" }, StringSplitOptions.None);

                if (jsonParts.Length != 4)
                {
                    throw new ArgumentException("Decrypted combined data does not contain the expected 4 parts.");
                }

                // Deserialize the JSON strings into dictionaries
                ButtonControl = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonParts[0]);
                FeedControl = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonParts[1]);
                PlayControl = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(jsonParts[2]);
                SleepControl = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(jsonParts[3]);

                Console.WriteLine("Dictionaries successfully repopulated!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while repopulating dictionaries: {ex.Message}");
            }
        }
    }
}
