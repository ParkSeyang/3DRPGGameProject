using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserSaveData
{
    public string userName;
    
    // 플레이어 스탯 정보
    public PlayerStat playerStat;
    
    // 위치 정보
    public float posX;
    public float posY;
    public float posZ;
    public float rotY; // 회전값 추가

    public string lastSceneName; // 마지막 씬 이름 추가

    // 인벤토리 정보
    public InventorySaveData userInventoryData;
    public InventorySaveData equipInventoryData;
    public InventorySaveData quickSlotData;
    
    // 스킬 데이터
    public List<SkillSaveData> skillData;
    public int skillSlotQ;
    public int skillSlotE;

    // 퀘스트 데이터
    public QuestSaveContainer questSaveData;

    public void SetPosition(Vector3 position)    
    {
        posX = position.x;
        posY = position.y;
        posZ = position.z;
    }

    public Vector3 GetPosition() => new Vector3(posX, posY, posZ);
}

[Serializable]
public class QuestSaveContainer
{
    public List<string> completedQuestKeys = new List<string>();
    public List<ActiveQuestSaveData> activeQuests = new List<ActiveQuestSaveData>();
}

[Serializable]
public class ActiveQuestSaveData
{
    public string questKey;
    public int currentProgress;
}

[Serializable]
public class InventorySaveData
{
    public List<SlotSaveData> Slots = new List<SlotSaveData>();
}

[Serializable]
public class SlotSaveData
{
    public int SlotIndex;
    public string ItemID;
    public int Count;
}

[Serializable]
public class SkillSaveData
{
    public int SkillID;
    public int Level;
}