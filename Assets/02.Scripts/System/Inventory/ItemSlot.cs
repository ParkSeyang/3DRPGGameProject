using System;
using UnityEngine;

[Serializable]
public class ItemSlot
{
    public Item ItemData; // 슬롯에 담긴 아이템 정보 (ScriptableObject)
    public int Amount;    // 현재 수량

    public bool IsEmpty => ItemData == null;

    public ItemSlot()
    {
        Clear();
    }

    public void SetItem(Item item, int amount = 1)
    {
        ItemData = item;
        Amount = amount;
    }

    public void AddAmount(int value)
    {
        if (IsEmpty) return;
        Amount += value;
    }

    public void Clear()
    {
        ItemData = null;
        Amount = 0;
    }
}
