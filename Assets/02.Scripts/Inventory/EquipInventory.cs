using UnityEngine;

public class EquipInventory : BaseInventory
{
    public override UIType UIType => UIType.Equip;

    protected override void Awake()
    {
        InventoryName = "Equip";
        base.Awake();
    }

    /// <summary>
    /// 아이템이 해당 슬롯에 들어갈 수 있는지 검사합니다.
    /// </summary>
    public bool IsValidItemForSlot(int slotIndex, Item item)
    {
        if (item == null) return true; 

        // 슬롯 인덱스별 허용 카테고리
        switch (slotIndex)
        {
            case 0: return item.ItemCategory == "Armor";
            case 1: return item.ItemCategory == "Weapon";
            case 2: return item.ItemCategory == "Artifact";
            case 3: return item.ItemCategory == "Armor";
            case 4: return item.ItemCategory == "Armor";
            default: return false;
        }
    }
}