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

    // --- Save & Load Logic ---

    public InventorySaveData GetSaveData()
    {
        InventorySaveData data = new InventorySaveData();
        
        for (int i = 0; i < slotList.Count; i++)
        {
            var slot = slotList[i];
            if (!slot.IsEmptySlot)
            {
                SlotSaveData slotData = new SlotSaveData
                {
                    SlotIndex = i,
                    ItemID = slot.Item.ItemID,
                    Count = slot.ItemCount
                };
                data.Slots.Add(slotData);
            }
        }
        return data;
    }

    public void LoadFromSaveData(InventorySaveData data)
    {
        if (data == null || ItemDataManager.Instance == null) return;

        // 슬롯 초기화가 안 되어 있다면 강제 초기화 (비활성 상태에서 로드 시 필요)
        if (slotList == null || slotList.Count == 0)
        {
            InitSlots();
        }

        // 먼저 모든 슬롯 비우기
        foreach (var slot in slotList)
        {
            slot.SetItem(null);
        }

        foreach (var slotData in data.Slots)
        {
            // 인덱스 유효성 검사
            if (slotData.SlotIndex >= 0 && slotData.SlotIndex < slotList.Count)
            {
                Item item = ItemDataManager.Instance.GetItem(slotData.ItemID);
                if (item != null)
                {
                    // 주의: Instantiate 로직은 SetItem 내부가 아니라 GetItem이나 외부에서 처리됨을 가정
                    // 하지만 보통 SO는 그대로 쓰고 인스턴스화는 필요에 따라 다름.
                    // 현재 구조상 ItemDataManager.GetItem은 원본 SO를 줄 가능성이 높음.
                    // 만약 개별 속성(내구도 등)이 있다면 Instantiate 필요.
                    // 여기서는 단순 참조로 진행 (기존 AddItem과 동일)
                    slotList[slotData.SlotIndex].SetItem(item, slotData.Count);
                    
                    // 장비창인 경우 로드시 장착 효과 적용 필요
                    if (this is EquipInventory)
                    {
                         PlayerStatusController.Instance.EquipItem(item);
                    }
                }
            }
        }
        
        // 데이터 로드 후 UI 강제 갱신
        RefreshInventory();
    }
}