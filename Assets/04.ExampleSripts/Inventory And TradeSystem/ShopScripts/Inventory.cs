using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Examples.Inventory
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance;
        // string 상수 태그
        public const string TRADER_INVENTORY_TAG = "Trader";
        public const string USER_INVENTORY_TAG = "User";
        public const string USER_QUICKINVENTORY_TAG = "Quick";
        
        public Button ShopRerollButton;
        public Button InventoryButton;
        private bool isShopReRoll = false;
        private bool isIventoryReRoll = false;
        
        [Header("Inventory Settings")] 
        public string Name;
        public int Row = 5;
        public int Column = 5;
        
        public SlotSystem[,] SlotsGrid; // 2차원배열
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            InventorySystem.Instance.RegisterInventory(this);
            
            SlotSystem[] slots = GetComponentsInChildren<SlotSystem>();
            SlotsGrid = new SlotSystem[Row,Column];

            List<SlotSystem> slotList = slots.ToList();
            
            for (int i = 0; i < slots.Length; i++ )
            {
                int row = i / Column;
                int column = i % Column;
                
                SlotsGrid[row,column] = slotList[i];
            }
        }

        // 매개변수의 slot이 inventory안에 존재하는지 검사하는 함수
        public bool IsInInventory(SlotSystem slot)
        {
            for (int x = 0; x < SlotsGrid.GetLength(0); x++)
            {
                for (int y = 0; y < SlotsGrid.GetLength(1); y++)
                {
                    //.Equals() 함수는 == 연산자와 거의 동일합니다.
                    //재정의 될 경우 쪼금 다름
                    if (SlotsGrid[x, y].Equals(slot))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        // Update is called once per frame
        private void Update()
        {
            if (isShopReRoll == true)
            {
                OnShopRerollButton();
            }
            else
            {
                isShopReRoll = false;
            }
            
            if (isIventoryReRoll == true)
            {
                OnInventoryRerollButton();
            }
            else
            {
                isIventoryReRoll = false;
            }

            if (Name.Contains(USER_QUICKINVENTORY_TAG))
            {
                QuickSlotitem();
            }

            
        }

        public void OnShopRerollButton()
        {
            if (Name.Contains(TRADER_INVENTORY_TAG))
            {
                for (int i = 0; i < SlotsGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < SlotsGrid.GetLength(1); j++)
                    {
                        int randNum = Random.Range(0, 12);
                        Item instance = ItemData.Instance.CreatItem(randNum);
                        SlotsGrid[i,j].SetItem(instance);
                    }
                }
            }
        }

        public void OnInventoryRerollButton()
        {
            if (Name.Contains(USER_INVENTORY_TAG))
            {
                for (int i = 0; i < SlotsGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < SlotsGrid.GetLength(1); j++)
                    {
                        SlotsGrid[i,j].SetItem(null);
                    }
                }
            }
        }

        public void QuickSlotitem()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UseItemTagTest(SlotsGrid[0, 0]);
                SlotsGrid[0,0].SetItem(null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UseItemTagTest(SlotsGrid[0, 1]);
                SlotsGrid[0,1].SetItem(null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UseItemTagTest(SlotsGrid[0, 2]);
                SlotsGrid[0,2].SetItem(null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                UseItemTagTest(SlotsGrid[0, 3]);
                SlotsGrid[0,3].SetItem(null);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                UseItemTagTest(SlotsGrid[0, 4]);
                SlotsGrid[0,4].SetItem(null);
            }
        }
        public void UseItemTagTest(SlotSystem slot)
        {
            float Max = 100.0f;
            float Min = 0.0f;
            
            if (Name.Contains(USER_QUICKINVENTORY_TAG) && slot.Item.Tag == "HPPostion")
            {
                // UserData.Instance.Health += 35.0f;
                // Debug.Log("HPPostion으로 체력을 35 회복하였습니다!");
                // if (UserData.Instance.Health > Max || UserData.Instance.Health < Min)
                // {
                //     UserData.Instance.Health = Mathf.Clamp(UserData.Instance.Health, Min, Max);
                //     Debug.Log($"{UserData.Instance.Name} 의 체력이 가득찼으므로 회복이 불가합니다.");
                // }
                
            }

            if (Name.Contains(USER_QUICKINVENTORY_TAG) && slot.Item.Tag == "MPPostion")
            {
                // UserData.Instance.Mana += 45.0f;
                // Debug.Log("MPPostion으로 마나를 45 회복하였습니다!");
                // if (UserData.Instance.Mana > Max || UserData.Instance.Mana < Min)
                // {
                //     UserData.Instance.Mana = Mathf.Clamp(UserData.Instance.Mana, Min, Max);
                //     Debug.Log($"{UserData.Instance.Name} 의 마나가 가득찼으므로 회복이불가합니다.");
                // }
            }

            if (Name.Contains(USER_QUICKINVENTORY_TAG) && slot.Item.Tag == "DragonBlood")
            {
                // UserData.Instance.Attack += 10.0f;
                // UserData.Instance.moveSpeed += 2.0f;
                // Debug.Log("공격력 10 및 이동속도 2% 가 상승하였습니다!");
            }
            
            if (Name.Contains(USER_QUICKINVENTORY_TAG) && slot.Item.Tag == "Applefood")
            {
                // UserData.Instance.Hunger += 10.0f;
                // Debug.Log("사과를 섭취하여 배고픔 10을 회복하였습니다!");
                // if (UserData.Instance.Hunger > Max || UserData.Instance.Hunger < Min)
                // {
                //     UserData.Instance.Hunger = Mathf.Clamp(UserData.Instance.Hunger, Min, Max);
                //     Debug.Log($"{UserData.Instance.Name} 이 배부른상태에서 최대치만큼만 회복합니다.");
                // }
            
            }
            
            if (Name.Contains(USER_QUICKINVENTORY_TAG) && slot.Item.Tag == "Breadfood")
            {
                // UserData.Instance.Hunger += 20.0f;
                // Debug.Log("빵를 섭취하여 배고픔 20을 회복하였습니다!");
                // if (UserData.Instance.Hunger > Max || UserData.Instance.Hunger < Min)
                // {
                //     UserData.Instance.Hunger = Mathf.Clamp(UserData.Instance.Hunger, Min, Max);
                //     Debug.Log($"{UserData.Instance.Name} 이 배부른상태에서 최대치만큼만 회복합니다.");
                // }
            
            }
            
            if (Name.Contains(USER_QUICKINVENTORY_TAG) && slot.Item.Tag == "Meetfood")
            {
                // UserData.Instance.Hunger += 50.0f;
                // Debug.Log("바베큐 고기를 섭취하여 배고픔 50을 회복하였습니다!");
                // if (UserData.Instance.Hunger > Max || UserData.Instance.Hunger < Min)
                // {
                //     UserData.Instance.Hunger = Mathf.Clamp(UserData.Instance.Hunger, Min, Max);
                //     Debug.Log($"{UserData.Instance.Name} 이 배부른상태에서 최대치만큼만 회복합니다.");
                // }
                
            }
            
        }

    
    }
}