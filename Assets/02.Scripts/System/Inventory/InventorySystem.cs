using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Settings")]
    public int InventorySize = 20;

    // 인벤토리 슬롯 데이터 리스트
    public List<ItemSlot> Slots { get; private set; }

    // 아이템 변경 알림 이벤트 (UI 갱신용)
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInventory()
    {
        Slots = new List<ItemSlot>();
        for (int i = 0; i < InventorySize; i++)
        {
            Slots.Add(new ItemSlot());
        }
    }

    private void Update()
    {
        // --- Test Code ---
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 랜덤 아이템 1개 추가
            List<int> allIds = ItemDataManager.Instance.GetAllItemIDs();
            if (allIds.Count > 0)
            {
                int randomId = allIds[UnityEngine.Random.Range(0, allIds.Count)];
                AddItem(randomId, 1);
                Debug.Log($"Test: Added Item ID {randomId}");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // 포션류(ID 1~4 가정) 5개 추가
            int potionId = UnityEngine.Random.Range(1, 5); // 1, 2, 3, 4
            AddItem(potionId, 5);
            Debug.Log($"Test: Added 5 Potions (ID {potionId})");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // 인벤토리 초기화
            foreach (var slot in Slots)
            {
                slot.Clear();
            }
            OnInventoryChanged?.Invoke();
            Debug.Log("Test: Inventory Cleared");
        }
        // ----------------
    }

    // 아이템 획득
    public bool AddItem(int itemId, int amount = 1)
    {
        Item itemData = ItemDataManager.Instance.GetItem(itemId);
        if (itemData == null) return false;

        return AddItem(itemData, amount);
    }

    public bool AddItem(Item item, int amount = 1)
    {
        // 1. 스택 가능한 아이템이면 기존 슬롯에 합치기 시도
        if (item.MaxStack > 1)
        {
            foreach (var slot in Slots)
            {
                if (!slot.IsEmpty && slot.ItemData.ItemID == item.ItemID && slot.Amount < item.MaxStack)
                {
                    int space = item.MaxStack - slot.Amount;
                    int add = Mathf.Min(space, amount);
                    
                    slot.AddAmount(add);
                    amount -= add;

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        // 2. 남은 수량은 빈 슬롯에 추가
        while (amount > 0)
        {
            ItemSlot emptySlot = GetEmptySlot();
            if (emptySlot == null)
            {
                // 인벤토리 가득 참
                Debug.Log("Inventory is Full!");
                OnInventoryChanged?.Invoke();
                return false; // 일부만 들어갔을 수도 있음
            }

            int add = Mathf.Min(item.MaxStack, amount);
            emptySlot.SetItem(item, add);
            amount -= add;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    // 빈 슬롯 찾기
    private ItemSlot GetEmptySlot()
    {
        foreach (var slot in Slots)
        {
            if (slot.IsEmpty) return slot;
        }
        return null;
    }

    // 아이템 이동 (Swap & Merge)
    public void SwapItems(int slotIndexA, int slotIndexB)
    {
        if (slotIndexA == slotIndexB) return;
        if (slotIndexA < 0 || slotIndexA >= Slots.Count || slotIndexB < 0 || slotIndexB >= Slots.Count) return;

        ItemSlot slotA = Slots[slotIndexA];
        ItemSlot slotB = Slots[slotIndexB];

        if (slotA.IsEmpty) return;

        // 1. 합치기 (Merge): 도착지에 같은 아이템이 있고, 스택이 가능할 때
        if (!slotB.IsEmpty && slotA.ItemData.ItemID == slotB.ItemData.ItemID && slotA.ItemData.MaxStack > 1)
        {
            int space = slotB.ItemData.MaxStack - slotB.Amount;
            if (space > 0)
            {
                int moveAmount = Mathf.Min(slotA.Amount, space);
                slotB.AddAmount(moveAmount);
                slotA.Amount -= moveAmount;

                if (slotA.Amount <= 0) slotA.Clear();

                OnInventoryChanged?.Invoke();
                return;
            }
        }

        // 2. 교체 (Swap)
        Item tempItem = slotA.ItemData;
        int tempAmount = slotA.Amount;

        slotA.SetItem(slotB.ItemData, slotB.Amount);
        slotB.SetItem(tempItem, tempAmount);

        OnInventoryChanged?.Invoke();
    }
}
