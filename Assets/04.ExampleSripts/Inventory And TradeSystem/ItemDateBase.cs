using System.Collections.Generic;
using UnityEngine;

public class ItemDataBase : MonoBehaviour
{
    public static ItemDataBase Instance { get; private set; }
    public Item[] Items;

    private Dictionary<int, Item> ItemDic = new Dictionary<int, Item>();
        
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        } 
        Instance = this;
            
        for (int i = 0; i < Items.Length; i++)
        {
            // 아이템 객체를 가져와서
            Item item = Items[i];
            // ItemDic에 같은 Key가 있는지 확인한뒤
            if (ItemDic.ContainsKey(item.Key))
            {
                continue;
            }
            ItemDic.Add(item.Key, item); // 없다면 추가해준다.
        }
        Debug.Log($"ItemDataBase ::: ItemDic.Count = {ItemDic.Count}");
            
    }

    private const int DUMMY_ITEM_KEY = 0;
    // Item을 가져온다. 견본을 가져올뿐, 인스턴스를 가져오는것이 아님에 주의
    public Item GetItem(int id)
    {
        // 만약 해당 id에 Item이 없다면 0번 반환
        if (ItemDic.ContainsKey(id) == false)
        {
            return ItemDic[DUMMY_ITEM_KEY];
        }
        return ItemDic[id];
    }
            
    // Item의 견본을 참고해서 새로운 인스턴스를 만들어서 반환한다.
    public Item CreateItem(int id)
    {
        Item template = GetItem(id); // 견본을 참고해서
        return Instantiate(template); // 사본을 생성하여 반환한다.
    }
    
}