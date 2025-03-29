namespace PocketCat.DefaultData
{
    public static class ActionData
    {
        public static ActionDataModel DataModel = new ActionDataModel
        {
            Name = "Sherlock",
            Pronouns = "He/Him/His",
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
                { "Dry Food", "Hunger: 1; Affection: 0; Energy: -1; #NAME# munches eagerly on the dry food but doesn't look fully satisfied." },
                { "Wet Food", "Hunger: 2; Affection: 1; Energy: 0; #NAME# scarfs down the wet food eagerly." },
                { "Fish", "Hunger: 1; Affection: 2; Energy: 1; #NAME# sniffs the fish momentarily and then chows down!" },
                { "Back", "Hunger: 0; Affection: 0; Energy: 0; Back." },
            },
            PlayControl = new Dictionary<string, string>
            {
                { "Toy Mouse", "Hunger: -1; Affection: 1; Energy: -1; #NAME# stares curiously at the toy mouse. #PRONOUNONE# doesn't seem to want to hurt it." },
                { "Catnip", "Hunger: -1; Affection: 2; Energy: 1; #NAME# stuffs #PRONOUNTHREELOWER# little face with catnip. #PRONOUNONE# seems to be enjoying #PRONOUNTWOLOWER#self." },
                { "Box", "Hunger: -1; Affection: 1; Energy: -1; #NAME# climbs into the box. #PRONOUNONE# seems to be enjoying #PRONOUNTWOLOWER#self!" },
                { "Back", "Hunger: 0; Affection: 0; Energy: 0; Back." },
            },
            SleepControl = new Dictionary<string, string>
            {
                { "Cat Bed", "Hunger: 0; Affection: 1; Energy: 1; #NAME# falls asleep in the cat bed peacefully." },
                { "Bed Pillow", "Hunger: 0; Affection: 2; Energy: 1; #NAME# seems to really enjoy your pillow." },
                { "Heated Bed", "Hunger: 0; Affection: 3; Energy: 2; #NAME# curls up warmly on the heated bed." },
                { "Back", "Hunger: 0; Affection: 0; Energy: 0; Back." },
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
            ExtraBored = new List<string>
            {
                "#NAME# looks visibly bored.",
                "Meow?",
                "Mrrrow?",
                ".......",
            },
            ExtraAnnoyed = new List<string>
            {
                "#NAME# flicked #PRONOUNTHREE# tail in annoyance.",
                "Maybe it's time to stop petting #NAME# and give #PRONOUNONE# some space.",
                "................             \n#NAME# looks annoyed.",
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
            Hungry = new List<string>
            {
                "#NAME# looks hungry.",
                "Snack time?",
            },
            Full = new List<string>
            {
                "#NAME# looks full.",
                "#NAME# doesn't seem hungry.",
                "#NAME# stares at the food.",
                "Maybe you should feed #NAME# later.",
            },
            Tired = new List<string>
            {
                "#NAME# looks sleepy.",
                "It appears to be time for a nap.",
                "Time to put #NAME# to bed soon!",
            },
            Sleeping = new List<string>
            {
                "#NAME# is sound asleep.",
                "ZzZzZz",
                "So cute. Best not to wake #PRONOUNONELOWER# up.",
            },
            Woke = new List<string>
            {
                "#NAME# woke up!",

            },
            HighAffectionIdle = new List<string>
            {
                "#NAME# seems to enjoy your company.",
                "#NAME# rubs up against your leg.",
                "#NAME# is happy to see you.",
                "Have you considered unlimited cuddles?",
                "#NAME# stares in your direction with love in #PRONOUNTHREELOWER# eyes.",
            },
            MidAffectionIdle = new List<string>
            {
                "#NAME# rubs up against your leg.",
                "#NAME# seems well entertained.",
                "You catch #NAME# staring at you.",
            },
            LowAffectionIdle = new List<string>
            {
                "#NAME# looks bored.",
                "#NAME# looks your way cautiously. #PRONOUNONE# seems unsure.",
                "#NAME# looks curious.",
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

        public static string Name => DataModel.Name;

        public static string Pronouns => DataModel.Pronouns;

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
        public static List<string> ExtraBored => DataModel.ExtraBored;
        public static List<string> ExtraAnnoyed => DataModel.ExtraAnnoyed;
        public static List<string> PetRandom => DataModel.PetRandom;
        public static List<string> Hungry => DataModel.Hungry;
        public static List<string> Full => DataModel.Full;
        public static List<string> Tired => DataModel.Tired;
        public static List<string> Sleeping => DataModel.Sleeping;
        public static List<string> Woke => DataModel.Woke;
        public static List<string> HighAffectionIdle => DataModel.HighAffectionIdle;
        public static List<string> MidAffectionIdle => DataModel.MidAffectionIdle;
        public static List<string> LowAffectionIdle => DataModel.LowAffectionIdle;
        public static Dictionary<string, string> StatusMessages => DataModel.StatusMessages;

        // Getters for ObjectDataProvider
        public static Dictionary<string, string> GetButtonControl() => DataModel.ButtonControl;
        public static Dictionary<string, string> GetFeedControl() => DataModel.FeedControl;
        public static Dictionary<string, string> GetPlayControl() => DataModel.PlayControl;
        public static Dictionary<string, string> GetSleepControl() => DataModel.SleepControl;
    }
}
