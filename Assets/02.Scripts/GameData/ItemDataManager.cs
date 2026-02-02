using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance { get; private set; }

    private Dictionary<int, Item> itemDatabase = new Dictionary<int, Item>();

    // TSV 로드용 DTO
    private class ItemRawData
    {
        public string ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemCategory { get; set; }
        public int SellPrice { get; set; }
        public int BuyPrice { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
        public int Stack { get; set; }
        public string PrefabName { get; set; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadItemData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadItemData()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "TSVData/ItemData.tsv");
        List<ItemRawData> rawDataList = TSVReader.ReadTable<ItemRawData>(path);

        if (rawDataList == null)
        {
            Debug.LogError("Failed to load ItemData.tsv");
            return;
        }

        foreach (var rawData in rawDataList)
        {
            // 런타임에 ScriptableObject 인스턴스 생성
            Item newItem = ScriptableObject.CreateInstance<Item>();
            
            newItem.InitializeFromTSV(
                rawData.ItemID,
                rawData.ItemName,
                rawData.ItemCategory,
                rawData.SellPrice,
                rawData.BuyPrice,
                rawData.Value,
                rawData.Description,
                rawData.Stack,
                rawData.PrefabName
            );

            // 리소스 로드 (아이콘, 프리팹)
            // 아이콘 경로는 "Icons/{PrefabName}" 등으로 가정하거나, PrefabName과 동일한 이름의 스프라이트를 찾습니다.
            // 실제 프로젝트 경로에 맞춰 수정 필요. 여기서는 Resources 폴더 사용을 가정합니다.
            newItem.Icon = Resources.Load<Sprite>($"Icons/{rawData.PrefabName}");
            // 만약 못 찾으면 기본 아이콘
            if (newItem.Icon == null) newItem.Icon = Resources.Load<Sprite>($"Icons/DefaultIcon");

            newItem.Prefab = Resources.Load<GameObject>($"Prefabs/Items/{rawData.PrefabName}");

            if (itemDatabase.ContainsKey(newItem.ItemID))
            {
                Debug.LogWarning($"Duplicate Item ID: {newItem.ItemID}");
            }
            else
            {
                itemDatabase.Add(newItem.ItemID, newItem);
            }
        }

        Debug.Log($"ItemDataManager: Loaded {itemDatabase.Count} items.");
    }

    public Item GetItem(int id)
    {
        if (itemDatabase.ContainsKey(id))
            return itemDatabase[id];
        return null;
    }
    
    public Item GetItem(string name)
    {
        // 이름으로 검색 (느릴 수 있으므로 캐싱 권장)
        return itemDatabase.Values.FirstOrDefault(item => item.ItemName == name);
    }

    public List<int> GetAllItemIDs()
    {
        return itemDatabase.Keys.ToList();
    }
}
