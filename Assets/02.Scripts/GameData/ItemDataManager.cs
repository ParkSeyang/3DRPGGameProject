using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ItemDataManager : SingletonBase<ItemDataManager>
{
    // ID 기반 데이터 딕셔너리 (데이터 보관용)
    private Dictionary<string, ItemInfo> itemInfoTable = new Dictionary<string, ItemInfo>();
    
    // ID 기반 리소스(SO) 딕셔너리 (실제 게임에서 사용)
    private Dictionary<string, Item> itemResourceTable = new Dictionary<string, Item>();

    public IReadOnlyDictionary<string, ItemInfo> ItemInfoTable => itemInfoTable;

    protected override void OnInitialize()
    {
        LoadItemTable();
        LoadItemResources();
    }

    private void LoadItemTable()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "TSVData", "ItemData.tsv");
        List<ItemInfo> list = TSVReader.ReadTable<ItemInfo>(path);

        if (list == null)
        {
            Debug.LogError("[ItemDataManager] TSV 파일을 로드하지 못했습니다.");
            return;
        }

        foreach (var info in list)
        {
            if (!itemInfoTable.ContainsKey(info.ItemID))
            {
                itemInfoTable.Add(info.ItemID, info);
            }
        }
        Debug.Log($"[ItemDataManager] TSV 로드 완료: {itemInfoTable.Count}개");
    }

    private void LoadItemResources()
    {
        // Resources/Items 폴더 내의 모든 Item ScriptableObject를 로드
        Item[] items = Resources.LoadAll<Item>("Items");
        
        foreach (var item in items)
        {
            // SO에 설정된 ItemID를 기준으로 TSV 데이터 매칭
            if (itemInfoTable.TryGetValue(item.ItemID, out var info))
            {
                // TSV 데이터를 SO 인스턴스에 주입 (런타임 동기화)
                item.ItemName = info.ItemName;
                item.ItemCategory = info.ItemCategory;
                item.SellPrice = info.SellPrice;
                item.BuyPrice = info.BuyPrice;
                item.Value = info.Value;
                item.Description = info.Description;
                item.MaxStack = info.Stack;

                if (!itemResourceTable.ContainsKey(item.ItemID))
                {
                    itemResourceTable.Add(item.ItemID, item);
                }
            }
            else
            {
                Debug.LogWarning($"[ItemDataManager] SO의 ItemID({item.ItemID})와 일치하는 TSV 데이터를 찾을 수 없습니다.");
            }
        }
        Debug.Log($"[ItemDataManager] 리소스 매칭 및 데이터 주입 완료: {itemResourceTable.Count}개");
    }

    public Item GetItem(string itemId)
    {
        if (itemResourceTable.TryGetValue(itemId, out var original))
        {
            // 원본을 복제하여 반환 (개별 인스턴스화)
            Item instance = Instantiate(original);
            instance.name = original.name; // 이름 뒤에 (Clone) 붙는 것 방지
            return instance;
        }
        return null;
    }
}