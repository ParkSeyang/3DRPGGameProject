using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Examples.Inventory
{
    public class UserInput : MonoBehaviour, 
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public GraphicRaycaster RayCaster;
        public SlotSystem CursorSlot;
        
        private SlotSystem startSlot;
        private RectTransform rectTransform;
        
        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // Held 기능의 구현
        // 1. 드래그가 시작되면 시작된 위치의 Slot의 아이템의 참조를 복사해서 Cursor에 Set해준다
        // 2. 드래그가 진행되는 동안 Cursor 객체의 위치좌표를 Update를 해준다
        // 3. 드래그가 종료되면 상황에 따라 알맞은 처리를 해준다
        //  3.1 종료된 위치의 Slot이 Empty일 경우 : Cursor의 아이템을 넣어주고, 시작 Slot을 비워준다
        //  3.2 종료된 위치의 Slot에 아이템이 있을 경우 : 시작 Slot과 종료 Slot의 아이템을 바꿔준다
        //  3.3 종료된 위치에 Slot 자체가 없을경우 : Cursor를 원상복귀 시킨다.
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            //드래그를 시작할때 slot을 캐싱해준다.
            startSlot = CheckSlot(eventData);
            if (startSlot == null)
            {
                return;
            }
            
            if (startSlot.IsEmptySlot)
            {
                startSlot = null;
                return; //아이템이 없으면 반응 하지 않음
            }
            CursorSlot.SetItem(startSlot.Item);
        }

        public void OnDrag(PointerEventData eventData)
        {
            //UI 좌표계랑 입력좌표계랑 달라서 보정이 필요함
            CursorSlot.SetPosition(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (startSlot == null)
            {
                return;
            }

            //startSlot = 시작 슬롯, endsSlot = 끝 슬롯
            SlotSystem endSlot = CheckSlot(eventData);
            
            //  3.3 종료된 위치에 Slot 자체가 없을경우 : Cursor를 원상복귀 시킨다. 
            if (endSlot == null) 
            {
                ResetCursor();
            }
            else
            {
                //  3.1 종료된 위치의 Slot이 Empty일 경우 : Cursor의 아이템을 넣어주고, 시작 Slot을 비워준다
                //  3.2 종료된 위치의 Slot에 아이템이 있을 경우 : 시작 Slot과 종료 Slot의 아이템을 바꿔준다
                if (endSlot.IsEmptySlot)
                {
                    MoveItem(endSlot); 
                }
                else
                {
                    SwapItem(startSlot, endSlot); 
                }
            }
        }
        
        private void ResetCursor()
        {
            ResetSlot();
        }

        private void MoveItem(SlotSystem goalSlot)
        {
            goalSlot.SetItem(CursorSlot.Item);
            CheckTrade(startSlot, goalSlot);
                
            startSlot.SetItem(null); // 옮겨진거니까 시작슬롯의 아이템을 없애줘야함.
            ResetSlot();
        }

        /// <summary>
        /// firstSlot과 secondSlot의 내용물을 교체해줍니다.
        /// </summary>
        /// <param name="firstSlot"></param>
        /// <param name="secondSlot"></param>
        private void SwapItem(SlotSystem firstSlot, SlotSystem secondSlot)
        {
            
            //  3.2.1 다른 인벤토리일 경우 처리하지 않는다.
            Inventory startInventory = InventorySystem.Instance.GetInventoryorNullBySlot(firstSlot);
            Inventory endInventory = InventorySystem.Instance.GetInventoryorNullBySlot(secondSlot);
            
            
            //인벤토리 <-> 퀵슬롯간 움직여야 할 경우에는
            //인벤토리의 타입유형들을 정의해서 비교하는게 더 유연할듯 보임
            if (startInventory == endInventory)
            {
                firstSlot.SetItem(secondSlot.Item);
                secondSlot.SetItem(CursorSlot.Item);
            }
            
            ResetSlot();
        }
        
        private void ResetSlot()
        {
            CursorSlot.SetItem(null);
            startSlot = null;
        }
        
        private SlotSystem CheckSlot(PointerEventData eventData)
        {
            List<RaycastResult> results = new List <RaycastResult>();
            RayCaster.Raycast(eventData,results);

            foreach (RaycastResult result in results)
            {
                SlotSystem slot = result.gameObject.GetComponent<SlotSystem>();
                if (slot != null)
                {
                    return slot;
                }
            }

            return null;
        }

        private void CheckTrade(SlotSystem start, SlotSystem end)
        {
            //Slot을 기준으로 어떤 인벤토리에서 어떤 인벤토리로 이동했는지 체크해본다.
        
            //1. Slot이 어떤 인벤토리에 속해있는지를 알아야 합니다.
            //2. inventory가 어떤 인벤토리인지 알아야 겠네요 
            Inventory startInventory = InventorySystem.Instance.GetInventoryorNullBySlot(start);
            Inventory endInventory = InventorySystem.Instance.GetInventoryorNullBySlot(end);

            //시작과 끝이 같다면 아무일도 일어나지 않는다.
            if(startInventory == endInventory) return;
            if (startInventory.Name == endInventory.Name)
            {
                return;
            }
            
            if (endInventory.Name.Contains(Inventory.TRADER_INVENTORY_TAG))
            {
                //endInventory의 Name이 TRADER_INVENTORY(Trader)를 포함하고 있다면
                //상점 취급이다 = 판매 로직
                TradeSystem.Instance.RequestTradeEvent(TradeSystem.InventoryType.Trader, TradeSystem.TradeType.Sell, start, end);
            }
            else if(endInventory.Name.Contains(Inventory.USER_INVENTORY_TAG))
            {
                //endInventory의 Name이 USER_INVENTORY_TAG(User)를 포함하고 있다면
                //유저 인벤토리 취급 = 구매 로직
                TradeSystem.Instance.RequestTradeEvent(TradeSystem.InventoryType.User,TradeSystem.TradeType.Buy, start, end);
            }
            else if (startInventory.Name.Contains(Inventory.TRADER_INVENTORY_TAG) ||endInventory.Name.Contains(Inventory.USER_QUICKINVENTORY_TAG))
            {
                TradeSystem.Instance.RequestTradeEvent(TradeSystem.InventoryType.Quick, TradeSystem.TradeType.None, start, end);
            }
        }
    }
}
