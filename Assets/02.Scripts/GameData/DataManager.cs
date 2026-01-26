using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : SingletonBase<DataManager>
{
    // 데이터 저장소 (외부에서 읽기 전용)
    public Dictionary<int, EnemyStat> EnemyTable { get; private set; } = new Dictionary<int, EnemyStat>();
    public Dictionary<string, PlayerStat> PlayerTable { get; private set; } = new Dictionary<string, PlayerStat>();

    protected override void OnInitialize()
    {
        Debug.Log("[DataManager] 데이터 로드 시작 (동기 방식)");
        LoadAllTables();
    }

    private void LoadAllTables()
    {
        // 1. 경로 설정 (StreamingAssets/TSVData/파일이름.tsv)
        string basePath = Path.Combine(Application.streamingAssetsPath, "TSVData");
        
        string enemyPath = Path.Combine(basePath, "EnemyData.tsv");
        string playerPath = Path.Combine(basePath, "PlayerData.tsv");

        // 2. 적 데이터 로드
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
                    Debug.LogWarning($"[DataManager] Enemy ID 중복 발견: {stat.ID}");
                }
            }
        }

        // 3. 플레이어 데이터 로드
        var playerList = TSVReader.ReadTable<PlayerStat>(playerPath);
        if (playerList != null)
        {
            foreach (var stat in playerList)
            {
                if (PlayerTable.ContainsKey(stat.Name) == false)
                {
                    PlayerTable.Add(stat.Name, stat);
                }
                else
                {
                    Debug.LogWarning($"[DataManager] Player Name 중복 발견: {stat.Name}");
                }
            }
        }

        Debug.Log($"[DataManager] 로드 완료 - Enemy: {EnemyTable.Count}, Player: {PlayerTable.Count}");
    }

    // --- 데이터 제공 메서드 ---

    public EnemyStat GetEnemyStat(int id)
    {
        if (EnemyTable.TryGetValue(id, out var stat))
        {
            return stat;
        }
        Debug.LogError($"[DataManager] Enemy ID {id}를 찾을 수 없습니다.");
        return null;
    }

    public PlayerStat GetPlayerStat(string name)
    {
        if (PlayerTable.TryGetValue(name, out var stat))
        {
            return stat;
        }
        Debug.LogError($"[DataManager] Player Name {name}을 찾을 수 없습니다.");
        return null;
    }
}
