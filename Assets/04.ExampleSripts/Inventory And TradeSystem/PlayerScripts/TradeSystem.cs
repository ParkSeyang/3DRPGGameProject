using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Jay;
using UnityEngine.Serialization;

namespace Examples.Inventory
{
    public class TradeSystem : SingletonBase<TradeSystem>
    {
        public enum InventoryType
        {
            User,
            Trader,
            Quick,
        }

        public enum TradeType 
        {
            None,
            Buy,
            Sell,
        }
        
        // 원래는 묶어쓰는게 좋음
        public GameObject BuyObject;
        public GameObject SellObject;
        public TMP_Text BuyText;
        public TMP_Text SellText;
        public Button YesButton;
        public Button NoButton;
        
        private bool isClick = false;
        private bool isPoitiveClick = false;
        
        
        public void RequestTradeEvent(InventoryType inven,TradeType type, SlotSystem start, SlotSystem end)
        {
            StartCoroutine(TriggerTradeEvent(inven, type, start, end));
        }
        
        private IEnumerator TriggerTradeEvent(InventoryType inven, TradeType type, SlotSystem start, SlotSystem end)
        {
            Debug.Log($"TradeEvent :: {type}, start ={start.gameObject.name}, end ={end.gameObject.name}");


            if (inven == InventoryType.User || TradeType.Buy == type)
            {
                BuyObject.SetActive(true);
                BuyText.SetText($"Would you like to purchase {end.Item.Name} : {end.Item.Price} ?");
            }

            if (inven == InventoryType.Trader || TradeType.Sell == type)
            {
                SellObject.SetActive(true);
                SellText.SetText($"Would you like to Sell {end.Item.Name} : {end.Item.Price / 2} ?");
            }
        // if (inven == InventoryType.Quick || TradeType.None == type)
        // {
        //     StopCoroutine(TriggerTradeEvent(inven, type, start, end));
        // }
        
            while (isClick == false)
            {
                yield return null;
            }
            
        
            BuyObject.SetActive(false);
            SellObject.SetActive(false);
            isClick = false;

            
            if (isPoitiveClick == true) 
            {
                
                // UserData currentUser = UserData.Instance;
                Item item = end.Item;
            
                switch (type)
                {
                    case TradeType.Buy:
                        // currentUser.DecreaseMoney(item.Price);
                        // Debug.Log($"[TradeEvent] {item.name}을 구매({item.Price})하였습니다. " +
                        //           $"잔액 : {currentUser.Money}");
                        break;
                    case TradeType.Sell:
                        // currentUser.IncreaseMoney(item.Price / 2);
                        // Debug.Log($"[TradeEvent] {item.name}을 판매({item.Price / 2})하였습니다. " +
                        //           $"잔액 : {currentUser.Money}");
                        break;
                    case TradeType.None:
                        Debug.Log("퀵슬롯에서 아이템을 구매할수없습니다.");
                    // start.SetItem(null);
                    // end.SetItem(null);
                        break;
                    default:
                        break;
                }
            }
            else 
            {
                start.SetItem(end.Item);
                end.SetItem(null);
            }
        }

        public void OnClickButton(bool isPositive)
        {
            isClick = true;
            isPoitiveClick = isPositive;
        }
    }
}