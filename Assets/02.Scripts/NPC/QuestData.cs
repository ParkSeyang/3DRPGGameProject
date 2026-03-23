using System;

[Serializable]
public class QuestData
{
    public string Category { get; set; }
    public string Key { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; } 
    public string Parameter { get; set; } 
    public int RewardGold { get; set; } // 보상 골드 
    public int RewardExp { get; set; }  // 보상 경험치 
}