using UnityEngine;

public enum ItemCategory
{
    None = 0,
    Weapon,
    Armor,
    Potion,
    Artifact,
    Etc
}

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item", order = 0)]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public int ItemID;          // TSV: I001 -> 1로 파싱
    public string ItemName;
    public ItemCategory Category;
    public string Description;
    
    [Header("Trade Info")]
    public int SellPrice;
    public int BuyPrice;
    
    [Header("Data & Logic")]
    public int MaxStack;        // TSV: Stack
    public string PrefabName;   // TSV: PrefabName (리소스 로드용)
    public Sprite Icon;         // 아이콘 스프라이트 (런타임 로드)
    public GameObject Prefab;   // 3D 모델 프리팹 (런타임 로드)

    [Header("Stats")]
    public ItemStat Stat;       // TSV: Value를 기반으로 생성

    // TSV 데이터로 초기화하는 함수
    public void InitializeFromTSV(string idStr, string name, string categoryStr, int sellPrice, int buyPrice, int value, string desc, int stack, string prefabName)
    {
        // ID 파싱 (I001 -> 1)
        if (int.TryParse(idStr.Replace("I", ""), out int id))
        { 
            ItemID = id; 
        }
        else
        { 
            ItemID = 0;
        } 
        ItemName = name;
        
        // 카테고리 파싱
        if (System.Enum.TryParse(categoryStr, out ItemCategory category))
        {
            Category = category;
        }
        else
        { 
            Category = ItemCategory.Etc;
        }
        
        SellPrice = sellPrice;
        BuyPrice = buyPrice;
        Description = desc;
        MaxStack = stack;
        PrefabName = prefabName;

        // Stat 생성 (Value 컬럼 활용)
        Stat = new ItemStat();
        switch (Category)
        {
            case ItemCategory.Weapon:
                Stat.Attack = value;
                break;
            case ItemCategory.Armor:
                Stat.Defense = value;
                break;
            case ItemCategory.Potion:
                // 포션은 이름이나 별도 로직으로 HP/MP 구분 필요. 
                // 일단 간단하게 이름에 따라 분기
                if (ItemName.Contains("체력") || ItemName.Contains("HP")) Stat.Health = value;
                else if (ItemName.Contains("마나") || ItemName.Contains("MP")) Stat.Mana = value;
                break;
            case ItemCategory.Artifact:
                // 아티팩트는 기획 의도에 따라 복합 스탯일 수 있음 (현재는 Value를 체력/마나에 반반 할당 등 임시 처리)
                Stat.Health = value;
                Stat.Mana = value;
                break;
        }
    }
}
