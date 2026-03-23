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
        if (startSlot == null || startSlot.IsEmptySlot == true)
        {
            startSlot = null;
            return;
        }
        
        CursorSlot.gameObject.SetActive(true);
        CursorSlot.SetPosition(eventData.position);
        CursorSlot.SetItem(startSlot.Item, startSlot.ItemCount);
        
        // 드래그 시작 시 원본 슬롯을 비움 (데이터는 CursorSlot이 들고 있음)
        // 주의: 실패 시 복구 로직이 반드시 필요함
        startSlot.SetItem(null);
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
        bool isSuccess = false;
        
        if (endSlot != null && startSlot != endSlot) 
        {
            if (endSlot.IsEmptySlot == true)
            {
                isSuccess = MoveItem(endSlot); 
            }
            else
            {
                isSuccess = SwapItem(startSlot, endSlot); 
            }
        }
        
        // 이동 실패 시 (또는 허공에 놓았을 때) 원본 슬롯으로 복구
        if (isSuccess == false)
        {
            startSlot.SetItem(CursorSlot.Item, CursorSlot.ItemCount);
        }
        
        ResetCursor();
    }
    
    private void ResetCursor()
    {
        CursorSlot.SetItem(null);
        CursorSlot.gameObject.SetActive(false);
        startSlot = null;
    }

    private bool MoveItem(SlotSystem goalSlot)
    {
        BaseInventory startInventory = InventorySystem.Instance.GetInventoryorNullBySlot(startSlot);
        BaseInventory goalInventory = InventorySystem.Instance.GetInventoryorNullBySlot(goalSlot);
        
        if (startInventory == null || goalInventory == null) return false;

        // 상점 거래 체크 (빈 슬롯으로 이동 시에도 거래 로직 발동)
        if (startInventory.InventoryName.Contains("Trader") || goalInventory.InventoryName.Contains("Trader"))
        {
            CheckTrade(startSlot, goalSlot);
            return false; // 거래는 별도 로직이므로 여기서 처리 종료
        }

        // 장비창 유효성 검사
        if (goalInventory is EquipInventory equipInven)
        {
            int slotIndex = GetSlotIndex(equipInven, goalSlot);
            if (equipInven.IsValidItemForSlot(slotIndex, CursorSlot.Item) == false)
            {
                UIManager.Instance.ShowWarning("이 슬롯에 장착할 수 없는 아이템입니다.");
                return false;
            }
        }

        // 장착 해제 시 인벤토리 가득 참 체크
        if (startInventory is EquipInventory && (goalInventory is EquipInventory == false))
        {
            if (goalInventory.IsFull == true)
            {
                UIManager.Instance.ShowWarning("인벤토리가 가득 차서 장비를 해제할 수 없습니다.");
                return false;
            }
        }

        // 스탯 처리 (이동 전 - 출발지가 장비창이면 해제)
        // ※ 주의: startSlot은 이미 비워졌으므로 CursorSlot의 데이터를 기준으로 체크
        if (startInventory is EquipInventory)
        {
            PlayerStatusController.Instance.UnequipItem(CursorSlot.Item);
        }

        // 실제 이동
        goalSlot.SetItem(CursorSlot.Item, CursorSlot.ItemCount);

        // 스탯 처리 (이동 후 - 도착지가 장비창이면 장착)
        if (goalInventory is EquipInventory)
        {
            PlayerStatusController.Instance.EquipItem(goalSlot.Item);
        }
        
        return true;
    }

    private bool SwapItem(SlotSystem firstSlot, SlotSystem secondSlot)
    {
        BaseInventory startInventory = InventorySystem.Instance.GetInventoryorNullBySlot(firstSlot);
        BaseInventory endInventory = InventorySystem.Instance.GetInventoryorNullBySlot(secondSlot);
        
        if (startInventory == null || endInventory == null) return false;

        // 상점 거래 체크
        if (startInventory.InventoryName.Contains("Trader") || endInventory.InventoryName.Contains("Trader"))
        {
            CheckTrade(firstSlot, secondSlot); 
            return false; // 상점 거래는 별도 로직이므로 여기서 처리 종료
        }

        // 장비창 유효성 검사 (서로 바꿀 수 있는지)
        if (endInventory is EquipInventory endEquipInven)
        {
            int slotIndex = GetSlotIndex(endEquipInven, secondSlot);
            if (endEquipInven.IsValidItemForSlot(slotIndex, CursorSlot.Item) == false)
            {
                UIManager.Instance.ShowWarning("대상 슬롯에 맞지 않는 아이템입니다.");
                return false;
            }
        }
        if (startInventory is EquipInventory startEquipInven)
        {
            int slotIndex = GetSlotIndex(startEquipInven, firstSlot);
            if (startEquipInven.IsValidItemForSlot(slotIndex, secondSlot.Item) == false)
            {
                UIManager.Instance.ShowWarning("현재 슬롯에 맞지 않는 아이템입니다.");
                return false;
            }
        }

        // 병합 로직 (기존 로직 유지하되 CursorSlot 데이터 활용)
        if ((startInventory is EquipInventory == false) && (endInventory is EquipInventory == false) &&
            CursorSlot.Item.ItemID == secondSlot.Item.ItemID)
        {
            int maxStack = secondSlot.Item.MaxStack;
            if (maxStack > 1)
            {
                int totalCount = CursorSlot.ItemCount + secondSlot.ItemCount;
                if (totalCount <= maxStack)
                {
                    secondSlot.SetItem(secondSlot.Item, totalCount);
                    return true; // 병합 완료
                }
                else
                {
                    int remainder = totalCount - maxStack;
                    secondSlot.SetItem(secondSlot.Item, maxStack);
                    firstSlot.SetItem(CursorSlot.Item, remainder); // 남은 건 다시 원래 자리로
                    return true;
                }
            }
        }

        // --- 스탯 처리 및 실제 스왑 ---
        
        // 1. 기존 장착 해제 (도착지가 장비창이면 장착되어 있던 것 해제)
        if (endInventory is EquipInventory) PlayerStatusController.Instance.UnequipItem(secondSlot.Item);
        // 2. 신규 장비 해제 (출발지가 장비창이면 드래그 중인 것 해제)
        if (startInventory is EquipInventory) PlayerStatusController.Instance.UnequipItem(CursorSlot.Item);

        // 3. 교체 실행
        Item targetItem = secondSlot.Item;
        int targetCount = secondSlot.ItemCount;

        secondSlot.SetItem(CursorSlot.Item, CursorSlot.ItemCount);
        firstSlot.SetItem(targetItem, targetCount);

        // 4. 새로운 상태 장착 적용
        if (endInventory is EquipInventory) PlayerStatusController.Instance.EquipItem(secondSlot.Item);
        if (startInventory is EquipInventory) PlayerStatusController.Instance.EquipItem(firstSlot.Item);

        return true;
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
            Item buyingItem = CursorSlot.Item;
            
            // [스마트 체크] 빈 슬롯이거나, 같은 아이템인데 더 쌓을 공간이 있는 경우에만 거래 요청
            bool isEmpty = end.IsEmptySlot;
            bool isSameItem = (isEmpty == false && end.Item.ItemID == buyingItem.ItemID);
            bool hasSpace = (isSameItem == true && end.ItemCount < end.Item.MaxStack);

            if (isEmpty == true || hasSpace == true)
            {
                TradeSystem.Instance.RequestTradeEvent(TradeSystem.InventoryType.User, TradeSystem.TradeType.Buy, start, end);
            }
            else
            {
                UIManager.Instance.ShowWarning("해당 슬롯에 더 이상 아이템을 담을 수 없습니다.");
            }
        }
    }
}