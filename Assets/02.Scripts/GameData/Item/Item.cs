using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "3DRPG/Item")]
public class Item : ScriptableObject
{
    [Header("Identify")]
    public string ItemID;      // TSV의 ItemID와 매칭
    public string ItemName;    // TSV의 ItemName

    [Header("Data")]
    public string ItemCategory;
    public int SellPrice;
    public int BuyPrice;
    public float Value;        // 회복량, 공격력 등 아이템의 핵심 수치
    [TextArea] 
    public string Description;
    public int MaxStack;       // TSV의 Stack 필드와 매칭

    [Header("Visual & Resources")]
    public Sprite Icon;        // 인벤토리 아이콘
    public GameObject Prefab;  // 월드 드랍 시 생성될 프리팹
}

    