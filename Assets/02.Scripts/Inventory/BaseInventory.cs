using System.Collections.Generic;
using UnityEngine;

public abstract class BaseInventory : BaseUI
{
    [Header("Inventory Settings")]
    public string InventoryName;
    public int Row = 5;
    public int Column = 5;
    
    [Tooltip("슬롯들이 위치한 부모 오브젝트 (비어있으면 본인 하위 전체 검색)")]
    [SerializeField] protected Transform slotParentTransform;

    public SlotSystem[,] SlotsGrid;
    protected List<SlotSystem> slotList = new List<SlotSystem>();
    protected bool isInitialized = false;

    public override UIType UIType => UIType.Inventory;

    protected override void Awake()
    {
        base.Awake();
        InitializeInventory();
    }

    public void InitializeInventory()
    {
        if (isInitialized == true) return;
        isInitialized = true; // [중요] 루프 방지를 위해 플래그를 먼저 설정

        InventorySystem.Instance.RegisterInventory(this);
        InitSlots();
    }

    protected virtual void OnDisable()
    {
        // [수정] 보호된(DontDestroyOnLoad) 인벤토리만 캐시에 데이터를 저장합니다.
        // 타이틀 씬 재진입 시 파괴되는 중복 인벤토리들에 의한 데이터 오염을 방지합니다.
        if (gameObject.scene.name == "DontDestroyOnLoad" == false) return;

        if (InventoryDataManager.IsInitialized == true && InventoryDataManager.Instance != null)
        {
            InventoryDataManager.Instance.SaveToCache(InventoryName, GetSaveData());
        }
    }

    protected void InitSlots()
    {
        // 지정된 부모가 있다면 그 아래에서만 검색, 없다면 본인 하위 검색
        Transform searchRoot = slotParentTransform != null ? slotParentTransform : transform;
        SlotSystem[] foundSlots = searchRoot.GetComponentsInChildren<SlotSystem>(true);
        
        SlotsGrid = new SlotSystem[Row, Column];
        slotList.Clear();

        // 그리드 크기와 실제 발견된 슬롯 개수 중 작은 값을 사용
        int count = Mathf.Min(foundSlots.Length, Row * Column);

        for (int i = 0; i < count; i++)
        {
            int rowIndex = i / Column;
            int columnIndex = i % Column;
            SlotsGrid[rowIndex, columnIndex] = foundSlots[i];
            slotList.Add(foundSlots[i]);
        }
    }

    public bool IsInInventory(SlotSystem slot)
    {
        return slotList.Contains(slot);
    }

    public bool IsFull => GetEmptySlot() == null;

    public override void Refresh()
    {
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        if (isInitialized == false) InitializeInventory();

        foreach (var slot in slotList)
        {
            if (slot != null)
            {
                // 슬롯 스스로 자신의 아이템 정보를 다시 그리도록 호출
                slot.SetItem(slot.Item, slot.ItemCount);
            }
        }
    }

    public SlotSystem GetEmptySlot()
    {
        if (isInitialized == false) InitializeInventory();

        foreach (var slot in slotList)
        {
            if (slot.IsEmptySlot == true) return slot;
        }
        return null;
    }

    /// <summary>
    /// 인벤토리에서 특정 아이템을 지정된 개수만큼 제거합니다.
    /// </summary>
    public void RemoveItem(string itemID, int count)
    {
        if (isInitialized == false) InitializeInventory();

        int remainingCount = count;

        foreach (var slot in slotList)
        {
            if (slot.IsEmptySlot == false && slot.Item.ItemID == itemID)
            {
                if (slot.ItemCount > remainingCount)
                {
                    // 슬롯에 남은 개수가 제거할 개수보다 많으면 수량만 감소
                    slot.SetItem(slot.Item, slot.ItemCount - remainingCount);
                    break;
                }
                else
                {
                    // 슬롯의 모든 아이템을 제거
                    remainingCount -= slot.ItemCount;
                    slot.SetItem(null);
                }
            }

            if (remainingCount <= 0) break;
        }
        
        RefreshInventory();
    }
    
    // --- Save & Load Logic ---

    public InventorySaveData GetSaveData()
    {
        if (isInitialized == false) InitializeInventory();

        InventorySaveData data = new InventorySaveData();
        
        for (int i = 0; i < slotList.Count; i++)
        {
            var slot = slotList[i];
            if (slot.IsEmptySlot == false)
            {
                SlotSaveData slotData = new SlotSaveData
                {
                    SlotIndex = i,
                    ItemID = slot.Item.ItemID,
                    Count = slot.ItemCount
                };
                data.Slots.Add(slotData);
            }
        }
        return data;
    }

    public void LoadFromSaveData(InventorySaveData data)
    {
        if (data == null || ItemDataManager.Instance == null) return;

        // 명시적 초기화 보장
        if (isInitialized == false)
        {
            InitializeInventory();
        }

        // 먼저 모든 슬롯 비우기
        foreach (var slot in slotList)
        {
            slot.SetItem(null);
        }

        foreach (var slotData in data.Slots)
        {
            // 인덱스 유효성 검사
            if (slotData.SlotIndex >= 0 && slotData.SlotIndex < slotList.Count)
            {
                Item item = ItemDataManager.Instance.GetItem(slotData.ItemID);
                if (item != null)
                {
                    // 주의: Instantiate 로직은 SetItem 내부가 아니라 GetItem이나 외부에서 처리됨을 가정
                    // 하지만 보통 SO는 그대로 쓰고 인스턴스화는 필요에 따라 다름.
                    // 현재 구조상 ItemDataManager.GetItem은 원본 SO를 줄 가능성이 높음.
                    // 만약 개별 속성(내구도 등)이 있다면 Instantiate 필요.
                    // 여기서는 단순 참조로 진행 (기존 AddItem과 동일)
                    slotList[slotData.SlotIndex].SetItem(item, slotData.Count);
                    
                    // 장비창인 경우 로드시 장착 효과 적용 필요
                    if (this is EquipInventory)
                    {
                         PlayerStatusController.Instance.EquipItem(item);
                    }
                }
            }
        }
        
        // 데이터 로드 후 UI 강제 갱신
        RefreshInventory();
    }
}