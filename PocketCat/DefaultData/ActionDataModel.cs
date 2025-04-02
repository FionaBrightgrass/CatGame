public class ActionDataModel
{


    // Existing properties
    public string Name { get; set; }
    public string Pronouns { get; set; }
    public string HungerName { get; set; } = "Hunger";
    public string HappinessName { get; set; } = "Happiness";
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
    public List<string> Sleeping { get; set; }
    public List<string> Woke { get; set; }
    public List<string> HighHappinessIdle { get; set; }
    public List<string> MidHappinessIdle { get; set; }
    public List<string> LowHappinessIdle { get; set; }
    public Dictionary<string, string> StatusMessages { get; set; }
    public int PlayStoryIndex { get; set; }
}
