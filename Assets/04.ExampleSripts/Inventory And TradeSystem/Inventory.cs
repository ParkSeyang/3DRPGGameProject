using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class Inventory : MonoBehaviour
{
    public const string TRADER_INVENTORY_TAG = "Trader";
    public const string USER_INVENTORY_TAG = "User";
    
    [Header("Inventory settings")] 
    public string Name;
    public int SizeRow = 8;
    public int SizeColumn = 5;

    public Slot[,] SlotsGrid;
    
    private void Start()
    {
        InventorySystem.Instance.RegisterInventory(this);
        
        Slot[] slots = GetComponentsInChildren<Slot>();
        SlotsGrid = new Slot[SizeRow, SizeColumn];

        List<Slot> slotList = slots.ToList();

        for (int i = 0; i < slots.Length; i++)
        {
            int row = i / SizeColumn; // 나누면 행이되고
            int Column = i % SizeColumn; // 나머지 연산을하면 열이됩니다

            SlotsGrid[row, Column] = slotList[i];
        }

    }

    // 매개변수의 slot이 inventory안에 존재하는지 검사하는 함수
    public bool IsInInventory(Slot slot)
    {
        
        for (int x = 0; x < SlotsGrid.GetLength(0); x++)
        {
            for (int y = 0; y < SlotsGrid.GetLength(1); y++)
            {
                // Equals() 함수는 == 연산자와 거의 동일합니다.
                // 재정의 될경우 조금 다름
                if (SlotsGrid[x,y].Equals(slot))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    private void Update()
    {
        // 테스트용 코드
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < SlotsGrid.GetLength(0) / 2; i++)
            {
                for (int j = 0; j < SlotsGrid.GetLength(1) / 2; j++)
                {
                    int randNum = Random.Range(0, 3);
                    Item instance = ItemDataBase.Instance.CreateItem(randNum);
                    SlotsGrid[i,j].SetItem(instance);
                }
            }
        }
    }

}
