namespace PocketCat.DefaultData
{

    public static class ActionData
    {
        public static ActionDataModel DataModel = new ActionDataModel
        {
            Name = "Sherlock",
            PronounOne = "He",
            PronounOneLower = "he",
            PronounTwo = "Him",
            PronounTwoLower = "him",
            HungerName = "Hunger",
            AffectionName = "Affection",
            EnergyName = "Energy",
            ButtonControl = new Dictionary<string, string>
        {
            { "Play", "" },
            { "Feed", "" },
            { "Pet", "" },
            { "Sleep", "" },
        },
            FeedControl = new Dictionary<string, string>
        {
            { "Dry Food", "Hunger: 1, Affection: 0, Energy: -1, #NAME# munches eagerly on the dry food but doesn't look fully satisfied." },
            { "Wet Food", "Hunger: 2, Affection: 1, Energy: 0, #NAME# scarfs down the wet food eagerly." },
            { "Fish", "Hunger: 1, Affection: 2, Energy: 1, #NAME# sniffs the fish curiously, then scarfs it down in one bite." },
            { "Back", "Hunger: 0, Affection: 0, Energy: 0, Back." },
        },
            PlayControl = new Dictionary<string, string>
        {
            { "Toy Mouse", "Hunger: 0, Affection: 1, Energy: -1, #NAME# chases the toy mouse. #PRONOUNONE# seems to be enjoying #PRONOUNTWOLOWER#self." },
            { "Yarn", "Hunger: 0, Affection: 1, Energy: -1, #NAME# rolls around with the ball of yarn. #PRONOUNONE# seems to be enjoying #PRONOUNTWOLOWER#self." },
            { "Catnip", "Hunger: -1, Affection: 2, Energy: -1, #NAME# rolls around with the catnip. #PRONOUNONE# seems to be enjoying #PRONOUNTWOLOWER#self." },
            { "Back", "Hunger: 0, Affection: 0, Energy: 0, Back." },
        },
            SleepControl = new Dictionary<string, string>
        {
            { "Cat Bed", "Hunger: 0, Affection: 1, Energy: 1, #NAME# falls asleep in the cat bed peacefully." },
            { "Bed Pillow", "Hunger: 0, Affection: 2, Energy: 1, #NAME# seems to really enjoy your pillow." },
            { "Heated Bed", "Hunger: 0, Affection: 3, Energy: 2, #NAME# curls up warmly on the heated bed." },
            { "Back", "Hunger: 0, Affection: 0, Energy: 0, Back." },
        },
            Extra = new List<string>
        {
            "#NAME# eyes you shyly...",
            "#NAME# seems interested in your mouse cursor.",
            "#NAME# looks visibly bored.",
            "Meow?",
            "#NAME# looks like #PRONOUNONE# wants to be your friend.",
            "#NAME# catches your gaze and begins to purr.",
            "#NAME# seems to trust you.",
            "#NAME# seems happy!",
            "#NAME# doesn't want to leave your side.",
            "#NAME# wants to be by your side forever.",
        },
            PetRandom = new List<string>
        {
            "You pet #NAME#... \n...#PRONOUNONELOWER# purrs softly.",
            "You gently stroke #NAME#'s fur. \n#NAME# purrs quietly.",
            "#NAME# rubs up against your hand. \n#PRONOUNONE# can't seem to get enough.",
            "       ....So cute...                ",
            "You give #NAME# scritches under the chin.",
            "#NAME# nibbles your hand weirdly.",
            "You rub under #NAME#'s collar. \n#NAME# really seems to appreciate this.",
            "SCRITCH SCRITCH. SCRITCH SCRITCH.",
        },
            StatusMessages = new Dictionary<string, string>
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
        },
            PlayStoryIndex = 0
        };

        // Shortcut properties for bar names
        public static string HungerName => DataModel.HungerName;
        public static string AffectionName => DataModel.AffectionName;
        public static string EnergyName => DataModel.EnergyName;

        // Shortcut properties for other fields
        public static Dictionary<string, string> ButtonControl => DataModel.ButtonControl;
        public static Dictionary<string, string> FeedControl => DataModel.FeedControl;
        public static Dictionary<string, string> PlayControl => DataModel.PlayControl;
        public static Dictionary<string, string> SleepControl => DataModel.SleepControl;
        public static List<string> Extra => DataModel.Extra;
        public static List<string> PetRandom => DataModel.PetRandom;
        public static Dictionary<string, string> StatusMessages => DataModel.StatusMessages;

        // Getters for ObjectDataProvider
        public static Dictionary<string, string> GetButtonControl() => DataModel.ButtonControl;
        public static Dictionary<string, string> GetFeedControl() => DataModel.FeedControl;
        public static Dictionary<string, string> GetPlayControl() => DataModel.PlayControl;
        public static Dictionary<string, string> GetSleepControl() => DataModel.SleepControl;
    }
}