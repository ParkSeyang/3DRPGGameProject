using System;
using UnityEngine;

public enum QuestType
{
    Tutorial,
    Normal,
    Repeat
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
public class Quest : ScriptableObject
{
    public string Key;
    public string Name;
    public string Description;
    public QuestType Type;
    
    public int CurrentProgress;
    public int TargetProgress;
    public string TargetID;
    
    public int RewardGold; // 보상 골드
    public int RewardExp;  // 보상 경험치

    public string TargetName
    {
        get
        {
            // [수정] 아이템 ID인 경우 ItemDataManager에서 실제 이름을 가져옴
            if (string.IsNullOrEmpty(TargetID) == false && ItemDataManager.Instance != null)
            {
                var item = ItemDataManager.Instance.GetItem(TargetID);
                if (item != null) return item.ItemName;
            }
            return TargetID;
        }
    }

    public bool IsCompleted => CurrentProgress >= TargetProgress;

    public void Initialize(QuestData data)
    {
        Key = data.Key;
        Name = data.Name;
        Description = data.Description;
        RewardGold = data.RewardGold;
        RewardExp = data.RewardExp;
        
        if (Enum.TryParse(data.Type, true, out QuestType qType)) Type = qType;
        else Type = QuestType.Normal;

        if (string.IsNullOrEmpty(data.Parameter) == false && data.Parameter.Contains(":"))
        {
            var split = data.Parameter.Split(':');
            TargetID = split[0];
            int.TryParse(split[1], out TargetProgress);
        }
        CurrentProgress = 0;
    }

    public void Reset() => CurrentProgress = 0;
}