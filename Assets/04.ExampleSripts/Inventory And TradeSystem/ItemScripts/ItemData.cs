using System.Collections.Generic;
using UnityEngine;

namespace Examples.Inventory
{
    public class ItemData : MonoBehaviour
    {
        public static ItemData Instance { get; private set; }

        public Item[] Items;

        private Dictionary<int, Item> ItemDic = new Dictionary<int, Item>();
        
        private void Awake()
        {
            // 싱글톤화 해준다.
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            } 
            Instance = this;

            for (int i = 0; i < Items.Length; i++)
            {
                Item item = Items[i];
                if (ItemDic.ContainsKey(item.Key))
                {
                    continue;
                }

                ItemDic.Add(item.Key, item);
            }
            
            Debug.Log($"ItemData : ItemDic.Count = {ItemDic.Count}");
        }

        private const int DUMMY_ITEM_KEY = 0;
        
        public Item GetItem(int id)
        {
            if (ItemDic.ContainsKey(id) == false)
            {
                return ItemDic[DUMMY_ITEM_KEY];
            }

            return ItemDic[id];
        }

        public Item CreatItem(int id)
        {
            Item template = GetItem(id);
            return Instantiate(template);
        }

    }
}