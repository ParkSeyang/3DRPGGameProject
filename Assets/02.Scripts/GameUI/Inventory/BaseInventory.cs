using System.Collections.Generic;
using UnityEngine;

public abstract class BaseInventory : BaseUI
{
    [Header("Inventory Settings")]
    public string InventoryName;
    public int Row = 5;
    public int Column = 5;

    public SlotSystem[,] SlotsGrid;
    protected List<SlotSystem> slotList = new List<SlotSystem>();

    public override UIType UIType => UIType.Inventory;

    protected override void Awake()
    {
        base.Awake();
        InventorySystem.Instance.RegisterInventory(this);
        InitSlots();
    }

    protected virtual void InitSlots()
    {
        SlotSystem[] foundSlots = GetComponentsInChildren<SlotSystem>();
        SlotsGrid = new SlotSystem[Row, Column];

        int count = Mathf.Min(foundSlots.Length, Row * Column);

        for (int i = 0; i < count; i++)
        {
            int r = i / Column;
            int c = i % Column;
            SlotsGrid[r, c] = foundSlots[i];
            slotList.Add(foundSlots[i]);
        }
    }

    public bool IsInInventory(SlotSystem slot)
    {
        return slotList.Contains(slot);
    }

    public virtual void RefreshInventory()
    {
        foreach (var slot in slotList)
        {
            // 필요한 경우 슬롯 UI 갱신 로직
        }
    }

    public SlotSystem GetEmptySlot()
    {
        foreach (var slot in slotList)
        {
            if (slot.IsEmptySlot) return slot;
        }
        return null;
    }

    // 수정됨: 개수(count)를 받을 수 있도록 오버로딩
    public bool AddItem(Item item, int count = 1)
    {
        var emptySlot = GetEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.SetItem(item, count);
            return true;
        }
        return false;
    }
}