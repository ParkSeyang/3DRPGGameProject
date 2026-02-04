using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserSaveData
{
    public string userName;
    
    // 플레이어 스탯 정보 (PlayerStat 클래스 재활용 가능)
    public PlayerStat playerStat;
    
    // 위치 정보
    public float posX;
    public float posY;
    public float posZ;

    // 인벤토리 정보 (User, Equip, Quick)
    public InventorySaveData userInventoryData;
    public InventorySaveData equipInventoryData;
    public InventorySaveData quickSlotData;
    
    // 스킬 데이터
    public List<SkillSaveData> skillData;
    public int skillSlotQ;
    public int skillSlotE;

    public void SetPosition(Vector3 position)    
    {
        posX = position.x;
        posY = position.y;
        posZ = position.z;
    }

    public Vector3 GetPosition() => new Vector3(posX, posY, posZ);
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
