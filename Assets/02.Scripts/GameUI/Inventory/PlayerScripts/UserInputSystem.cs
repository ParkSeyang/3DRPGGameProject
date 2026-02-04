using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UserInputSystem : SingletonBase<UserInputSystem>
{
    public SlotSystem CursorSlot;
    
    private SlotSystem startSlot;

    protected override void OnInitialize()
    {
        // CursorSlot이 Raycast를 막지 않도록 설정
        if (CursorSlot != null)
        {
            var image = CursorSlot.GetComponent<Image>();
            if (image != null) image.raycastTarget = false;
            
            var canvasGroup = CursorSlot.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = CursorSlot.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void ProcessBeginDrag(SlotSystem slot, PointerEventData eventData)
    {
        startSlot = slot;
        if (startSlot == null || startSlot.IsEmptySlot)
        {
            startSlot = null;
            return;
        }
        
        CursorSlot.gameObject.SetActive(true);
        CursorSlot.SetPosition(eventData.position);
        CursorSlot.SetItem(startSlot.Item, startSlot.ItemCount);
        
        // 드래그 시작 시 원본 슬롯 아이템을 잠시 숨김 처리하거나 흐리게 처리하는 로직 추가 가능
    }

    public void ProcessDrag(PointerEventData eventData)
    {
        if (startSlot == null) return;
        CursorSlot.SetPosition(eventData.position);
    }

    public void ProcessEndDrag(PointerEventData eventData)
    {
        if (startSlot == null) return;

        SlotSystem endSlot = CheckSlot(eventData);
        
        if (endSlot == null) 
        {
            // Debug.LogWarning("[UserInput] 놓은 곳에 슬롯이 없습니다.");
            // 허공에 놓았을 때 버리기 처리를 하고 싶다면 여기에 추가
            ResetCursor();
        }
        else
        {
            // 같은 슬롯이면 무시
            if (startSlot == endSlot)
            {
                ResetCursor();
                return;
            }

            if (endSlot.IsEmptySlot)
            {
                MoveItem(endSlot); 
            }
            else
            {
                SwapItem(startSlot, endSlot); 
            }
        }
        
        ResetCursor();
    }
    
    private void ResetCursor()
    {
        CursorSlot.SetItem(null);
        CursorSlot.gameObject.SetActive(false); // 커서 숨김
        startSlot = null;
    }

    private void MoveItem(SlotSystem goalSlot)
    {
        BaseInventory startInventory = InventorySystem.Instance.GetInventoryorNullBySlot(startSlot);
        BaseInventory goalInventory = InventorySystem.Instance.GetInventoryorNullBySlot(goalSlot);

        // 장비창 유효성 검사
        if (goalInventory is EquipInventory equipInven)
        {
            int slotIndex = GetSlotIndex(equipInven, goalSlot);
            
            if (!equipInven.IsValidItemForSlot(slotIndex, startSlot.Item))
            {
                Debug.LogWarning("[UserInput] 해당 장비 슬롯에 맞지 않는 아이템입니다.");
                return;
            }
        }

        // 스탯 처리 (이동 전 수행 - 출발지가 장비창이면 해제)
        if (startInventory is EquipInventory)
        {
            PlayerStatusController.Instance.UnequipItem(startSlot.Item);
        }

        // 실제 이동
        goalSlot.SetItem(CursorSlot.Item, CursorSlot.ItemCount);
        startSlot.SetItem(null);

        // 스탯 처리 (이동 후 수행 - 도착지가 장비창이면 장착)
        if (goalInventory is EquipInventory)
        {
            PlayerStatusController.Instance.EquipItem(goalSlot.Item);
        }
        
        CheckTrade(startSlot, goalSlot);
    }

    private void SwapItem(SlotSystem firstSlot, SlotSystem secondSlot)
    {
        BaseInventory startInventory = InventorySystem.Instance.GetInventoryorNullBySlot(firstSlot);
        BaseInventory endInventory = InventorySystem.Instance.GetInventoryorNullBySlot(secondSlot);
        
        if (startInventory == null || endInventory == null) return;

        // 상점 거래 체크
        if (startInventory.InventoryName.Contains("Trader") || endInventory.InventoryName.Contains("Trader"))
        {
            CheckTrade(firstSlot, secondSlot); 
            return; 
        }

        // 장비창 유효성 검사
        if (endInventory is EquipInventory endEquipInven)
        {
            int slotIndex = GetSlotIndex(endEquipInven, secondSlot);
            if (!endEquipInven.IsValidItemForSlot(slotIndex, firstSlot.Item)) return;
        }
        if (startInventory is EquipInventory startEquipInven)
        {
            int slotIndex = GetSlotIndex(startEquipInven, firstSlot);
            if (!startEquipInven.IsValidItemForSlot(slotIndex, secondSlot.Item)) return;
        }

        // 병합 로직 (장비창에서는 병합 안 함)
        if (!(startInventory is EquipInventory) && !(endInventory is EquipInventory) &&
            firstSlot.Item != null && secondSlot.Item != null && 
            firstSlot.Item.ItemID == secondSlot.Item.ItemID)
        {
            // ... 기존 병합 로직 유지 ...
            int maxStack = secondSlot.Item.MaxStack;
            if (maxStack > 1)
            {
                int totalCount = firstSlot.ItemCount + secondSlot.ItemCount;
                if (totalCount <= maxStack)
                {
                    secondSlot.SetItem(secondSlot.Item, totalCount);
                    firstSlot.SetItem(null);
                    return;
                }
                else
                {
                    int remainder = totalCount - maxStack;
                    secondSlot.SetItem(secondSlot.Item, maxStack);
                    firstSlot.SetItem(firstSlot.Item, remainder);
                    return;
                }
            }
        }

        // 스탯 처리 (장착 해제)
        if (startInventory is EquipInventory) PlayerStatusController.Instance.UnequipItem(firstSlot.Item);
        if (endInventory is EquipInventory) PlayerStatusController.Instance.UnequipItem(secondSlot.Item);

        // 스왑 실행
        Item tempItem = secondSlot.Item;
        int tempCount = secondSlot.ItemCount;

        secondSlot.SetItem(firstSlot.Item, firstSlot.ItemCount);
        firstSlot.SetItem(tempItem, tempCount);

        // 스탯 처리 (장착)
        if (startInventory is EquipInventory) PlayerStatusController.Instance.EquipItem(firstSlot.Item);
        if (endInventory is EquipInventory) PlayerStatusController.Instance.EquipItem(secondSlot.Item);
    }

    private int GetSlotIndex(BaseInventory inventory, SlotSystem targetSlot)
    {
        // 2차원 배열을 1차원으로 펼쳐서 인덱스 찾거나, 순회
        int index = 0;
        foreach (var slot in inventory.SlotsGrid)
        {
            if (slot == targetSlot) return index;
            index++;
        }
        return -1;
    }
    
    private SlotSystem CheckSlot(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            SlotSystem slot = result.gameObject.GetComponent<SlotSystem>();
            // CursorSlot 자신은 제외
            if (slot != null && slot != CursorSlot) 
            {
                return slot;
            }
        }
        return null;
    }

    private void CheckTrade(SlotSystem start, SlotSystem end)
    {
        BaseInventory startInventory = InventorySystem.Instance.GetInventoryorNullBySlot(start);
        BaseInventory endInventory = InventorySystem.Instance.GetInventoryorNullBySlot(end);

        if(startInventory == null || endInventory == null || startInventory == endInventory) return;
        
        if (endInventory.InventoryName.Contains("Trader"))
        {
            TradeSystem.Instance.RequestTradeEvent(TradeSystem.InventoryType.Trader, TradeSystem.TradeType.Sell, start, end);
        }
        else if(endInventory.InventoryName.Contains("User") && startInventory.InventoryName.Contains("Trader"))
        {
            TradeSystem.Instance.RequestTradeEvent(TradeSystem.InventoryType.User, TradeSystem.TradeType.Buy, start, end);
        }
    }
}