using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Userinput : MonoBehaviour, //IPointerClickHandler, IDropHandler
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
   // public void OnPointerClick(PointerEventData eventData)
   // {
   //     Debug.Log($"OnPointerClick {eventData.position}");
   // } 
   
    public GraphicRaycaster RayCaster;
    public Slot CursorSlot;
    
    private Slot startSlot;
    private RectTransform rectTransform;
    
    /* Held 기능의 구현
     * 1. 드래그가 시작되면 시작된 위치의 Slot의 아이템의 참조를 복사해서 Cursor에 Set해준다.
     * 2. 드래그가 진행되는 동안 Cursor 객체의 위치좌표를 Update를 해준다.
     * 3. 드래그가 종료되면 상황에 따라 알맞은 처리를 해준다.
     * - 3.1 종료된 위치의 Slot이 Empty일 경우 : Cursor의 아이템을 넣어주고, 시작Slot을 비워준다.
     * - 3.2 종료된 위치의 Slot에 아이템이 있을 경우 : 시작 Slot과 종료 Slot의 아이템을 바꿔준다.
     * - 3.3 종료된 위치에 Slot 자체가 없을 경우 : Cursor를 원상복귀 시킨다.
     */

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    
    public void OnBeginDrag(PointerEventData eventData)
    { 
        // 드래그를 시작할때 slot을 캐싱해준다.
        startSlot = CheckSlot(eventData);
      if (startSlot.IsEmptySlot)
      {
          return;  // 아이템이 없으면 반응 하지 않음
      }
      CursorSlot.SetItem(startSlot.item);
    }

    public void OnDrag(PointerEventData eventData)
    {
       // Debug.Log($"OnDrag {eventData.position}");
       
       CursorSlot.SetPosition(eventData.position);
       
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
       Slot endSlot = CheckSlot(eventData);

       if (endSlot == null)  // - 3.3 종료된 위치에 Slot 자체가 없을 경우 : Cursor를 원상복귀 시킨다.
       {
           ResetCursor();
       }
       else
       {
           if (endSlot.IsEmptySlot) // - 3.1 종료된 위치의 Slot이 Empty일 경우 : Cursor의 아이템을 넣어주고, 시작Slot을 비워준다.
           {
               MoveItem(endSlot);
           }
           else // - 3.2 종료된 위치의 Slot에 아이템이 있을 경우 : 시작 Slot과 종료 Slot의 아이템을 바꿔준다.
           {
               SwapItem(startSlot, endSlot);
               // 3.2.1 달느 인벤토리일 경우 처리하지않는다.
           
           }
       }
       
       void ResetCursor()
       {
           ResetSlot();
       }

       void MoveItem(Slot goalSlot)
       {
           goalSlot.SetItem(CursorSlot.item);
           // startSlot = 시작슬롯 / slot = endSlot
           CheckTrade(startSlot, endSlot);
           startSlot.SetItem(null);  // 옮겨진거니까 시작슬롯의 아이템을 없애 줘야 합니다.
           ResetSlot();
       }
    }

    /// <summary>
    /// firstSlot과 secondSlot의 내용물을 교체해줍니다.
    /// </summary>
    /// <param name="firstSlot"></param>
    /// <param name="secondSlot"></param>
    void SwapItem(Slot firstSlot,Slot secondSlot)
    {
        Inventory startInventory = InventorySystem.Instance.GetInventoryOrNullBySlot(firstSlot);
        Inventory endInventory = InventorySystem.Instance.GetInventoryOrNullBySlot(secondSlot);

        // 인벤토리 <-> 퀵슬롯간 움직여야 할경우에는
        // 인벤토리의 타입유형들을 정의하는게 더 유연할듯 보임
        if (startInventory == endInventory)
        {
            firstSlot.SetItem(secondSlot.item);
            secondSlot.SetItem(CursorSlot.item);
        }
        ResetSlot();

    }
    
    void ResetSlot()
    {
        CursorSlot.SetItem(null);
        startSlot = null;
    }
    
    private Slot CheckSlot(PointerEventData eventData)
    {     
        // Debug.Log($"OnBeginDrag {eventData.position}");
        
        List<RaycastResult> results = new List<RaycastResult>();
        RayCaster.Raycast(eventData, results);
        
        foreach (RaycastResult result in results)
        {
            Slot slot = result.gameObject.GetComponent<Slot>();
            if (slot != null)
            {
                return slot;
            }
        }

        return null;
    }

    private void CheckTrade(Slot start, Slot end)
    {
        // Slot을 기준으로 어떤 인벤토리에서 어떤 인벤토리로 이동했는지 체크해본다.
        // start가 UserInventory, end가 TradeInventory일 경우 판매
        // start가 TraderInventory, end가 UserInventory일 경우 구매
        
        // 1. Slot이 어떤인벤토리에 속해있는지를 알아야 합니다.
        // 2. inventory가 어떤 인벤토리인지 알아야 합니다.

        Inventory startInventory = InventorySystem.Instance.GetInventoryOrNullBySlot(start);
        Inventory endInventory = InventorySystem.Instance.GetInventoryOrNullBySlot(end);

        // 시작과 끝이 같다면 아무일도 일어나지 않는다.
        if (startInventory == endInventory)
        {
            return;
        }
        // endInventory의 Name이 TRADER_INVENTORY(Trader)를 포함하고 있다면
        // 상점 취급이다.
        
        if (endInventory.Name.Contains(Inventory.TRADER_INVENTORY_TAG))
        {
            // endInventory의 Name이 TRADER_INVENTORY_TAG를 포함하고있다면
            // 상점 인벤토리 취급 = 판매 로직
            TradeSystem.Instance.RequestTradeEvent(TradeSystem.TradeType.Sell, start, end);
        }
        else if(endInventory.Name.Contains(Inventory.USER_INVENTORY_TAG))
        {
            // endInventory의 Name이 USER_INVENTORY_TAG를 포함하고있다면
            // 유저 인벤토리 취급 = 구매 로직
            TradeSystem.Instance.RequestTradeEvent(TradeSystem.TradeType.Buy, start, end);
        }

    }

}
