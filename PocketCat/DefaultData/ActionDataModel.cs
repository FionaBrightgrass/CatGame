public class ActionDataModel
{


    // Existing properties
    public string Name { get; set; }
    public string PronounOne { get; set; }
    public string PronounOneLower { get; set; }
    public string PronounTwo { get; set; }
    public string PronounTwoLower { get; set; }
    public string PronounThree { get; set; }
    public string PronounThreeLower { get; set; }
    public string HungerName { get; set; } = "Hunger";
    public string AffectionName { get; set; } = "Affection";
    public string EnergyName { get; set; } = "Energy";
    public Dictionary<string, string> ButtonControl { get; set; }
    public Dictionary<string, string> FeedControl { get; set; }
    public Dictionary<string, string> PlayControl { get; set; }
    public Dictionary<string, string> SleepControl { get; set; }
    public List<string> Extra { get; set; }
    public List<string> ExtraBored { get; set; }
    public List<string> ExtraAnnoyed { get; set; }
    public List<string> PetRandom { get; set; }
    public List<string> Hungry { get; set; }
    public List<string> Full { get; set; }
    public List<string> Tired { get; set; }
    public List<string> Woke { get; set; }
    public List<string> HighAffectionIdle { get; set; }
    public List<string> MidAffectionIdle { get; set; }
    public List<string> LowAffectionIdle { get; set; }
    public Dictionary<string, string> StatusMessages { get; set; }
    public int PlayStoryIndex { get; set; }
}
