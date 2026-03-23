using System.Collections.Generic;
using UnityEngine;

public class InventoryDataManager : SingletonBase<InventoryDataManager>
{
    // 씬 전환 및 세이브/로드를 위한 데이터 캐시
    private Dictionary<string, InventorySaveData> dataCache = new Dictionary<string, InventorySaveData>();

    /// <summary>
    /// 특정 인벤토리의 데이터를 캐시에 보관합니다.
    /// </summary>
    public void SaveToCache(string name, InventorySaveData data)
    {
        if (string.IsNullOrEmpty(name) == true)
        {
            Debug.LogError($"[{nameof(InventoryDataManager)}] SaveToCache 실패: 인벤토리 이름이 유효하지 않습니다.");
            return;
        }

        if (data == null)
        {
            Debug.LogWarning($"[{nameof(InventoryDataManager)}] SaveToCache 경고: '{name}'에 주입하려는 데이터가 Null입니다.");
            return;
        }

        dataCache[name] = data;
    }

    /// <summary>
    /// 캐시나 실시간 UI로부터 가장 최신 데이터를 추출합니다.
    /// </summary>
    public InventorySaveData GetInventoryData(string name)
    {
        // 1. 현재 씬에 활성화된 UI가 있다면 우선적으로 실시간 데이터를 가져옴
        if (InventorySystem.Instance != null)
        {
            var inven = InventorySystem.Instance.GetInventoryOrNull(name);
            if (inven != null) return inven.GetSaveData();
        }

        // 2. UI가 없다면 캐시된 데이터를 반환
        if (dataCache.TryGetValue(name, out var cachedData))
        {
            return cachedData;
        }

        return null;
    }

    /// <summary>
    /// 데이터를 강제로 주입하고, 현재 씬에 있는 모든 관련 UI를 즉시 동기화합니다.
    /// </summary>
    public void SetInventoryData(string name, InventorySaveData data)
    {
        if (data == null) return;

        // 1. 캐시 업데이트
        dataCache[name] = data;

        // 2. 현재 씬에 존재하는 모든 해당 이름의 UI 갱신 (상점용 가방, 일반 가방 모두)
        if (InventorySystem.Instance != null)
        {
            var allInvens = InventorySystem.Instance.GetAllInventoriesByName(name);
            foreach (var inven in allInvens)
            {
                inven.LoadFromSaveData(data);
                inven.RefreshInventory();
            }
        }
    }

    public InventorySaveData GetCachedDataOrNull(string name)
    {
        return dataCache.TryGetValue(name, out var data) ? data : null;
    }

    /// <summary>
    /// 모든 인벤토리 캐시 데이터를 삭제합니다. (새 게임 시작용)
    /// </summary>
        public void ClearAllData()
        {
            dataCache.Clear();
        }
    }
    