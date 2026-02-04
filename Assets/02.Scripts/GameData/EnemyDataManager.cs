using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyDataManager : SingletonBase<EnemyDataManager>
{
    public Dictionary<int, EnemyStat> EnemyTable { get; private set; } = new Dictionary<int, EnemyStat>();
    
    protected override void OnInitialize()
    {
        Debug.Log("[EnemyDataManager] 적 데이터 로드 시작...");
        LoadEnemyTables();
    }

    private void LoadEnemyTables()
    {
        Debug.Log($"[EnemyDataManager] LoadEnemyTables 호출됨 (현재 데이터 수: {EnemyTable.Count})");
        EnemyTable.Clear(); // 중복 방지

        string basePath = Path.Combine(Application.streamingAssetsPath, "TSVData");
        string enemyPath = Path.Combine(basePath, "EnemyData.tsv");

        var enemyList = TSVReader.ReadTable<EnemyStat>(enemyPath);
        if (enemyList != null)
        {
            foreach (var stat in enemyList)
            {
                if (EnemyTable.ContainsKey(stat.ID) == false)
                {
                    EnemyTable.Add(stat.ID, stat);
                }
                else
                {
                    Debug.LogWarning($"[EnemyDataManager] 중복된 ID 발견: {stat.ID}");
                }
            }
        }
        
        Debug.Log($"[EnemyDataManager] 로드 완료 - 총 {EnemyTable.Count}개");
    }

    public EnemyStat GetEnemyStat(int id)
    {
        if (EnemyTable.TryGetValue(id, out var stat))
        {
            return stat;
        }
        Debug.LogError($"[EnemyDataManager] ID {id}에 해당하는 적 데이터를 찾을 수 없습니다.");
        return null;
    }
}