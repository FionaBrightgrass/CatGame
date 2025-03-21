namespace PocketCat.DefaultData
{
    public static class DefaultSheet
    {
        public static string name = "Sherlock";
        public static string pronounOne = "He";
        public static string pronounOneLower = pronounOne.ToLower();
        public static string pronounTwo = "Him";
        public static string pronounTwoLower = pronounTwo.ToLower();
        // Dictionaries
        public static Dictionary<string, string> ButtonControl { get; set; } = new Dictionary<string, string>
        {
            //Name
            { "Play", "" },
            { "Feed", "" },
            { "Pet", "" },
            { "Sleep", "" },
        };
        public static Dictionary<string, List<string>> FeedControl { get; set; } = new Dictionary<string, List<string>>
        {
            //Name               //<Hunger, Affection, Cost, Text>
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

        public static int PlayStoryIndex = 0;
        public static List<string> PlayStory { get; set; } = new List<string>
        {
            { "You play with #NAME#...     \n...he rubs against your hands."},
            { "You wiggle your mouse.     \n#NAME# watches it dance..." },
            { "#NAME# is the greatest of kitty cats. \n...and the best of friends." },
            { "If only #NAME# were real.... \n#PRONOUNONE# would play with you until the end." },
            { "#NAME# is a fearless companion.    \n...and your meeting was written in the stars." },
            { "#NAME# found you for a reason.    \n...and to find you, no distance was ever too far." },
            { "You play with #Name.     \n...#PRONOUNONE# meets you with a warm gaze." },
            { "#NAME# will remain your friend...    \nUntil the very end of days."},
            { "If some day you and #Name must part,     \nI ask that you remember this work of art."},
            { "Because maybe, some day a real kitty cat,     \n...will be adopted into your heart."},
        };

        public static Dictionary<string, string> StatusMessages = new Dictionary<string, string>
        {
            { "VERYHUNGRY", "You hear a tummy rumble... \n#NAME# looks very hungry." },
            { "TOOHUNGRY", "#NAME# looks too hungry to play..." },
            { "TOOTIRED", "#NAME# looks too tired to play..." },
            { "SLEEP", "You put #NAME# to bed. \n#PRONOUNONE# falls asleep quickly." },
            { "SLEEPING", "#NAME# is sleeping soundly." },
            { "NAP", "You put #NAME# to bed. \n#PRONOUNONE# takes a cat nap." },
            { "NOTTIRED", "You put #NAME# to bed. \n#PRONOUNONE# wants to play instead." },
            { "SLEEPNOEAT", "#NAME# is sleeping soundly." },
            { "PLAY01", "You play with #NAME#. \n...#PRONOUNONE# looks happy." },
        };
    }
}
