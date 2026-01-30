using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : SingletonBase<DataManager>
{
    // Player 데이터만 관리
    public Dictionary<string, PlayerStat> PlayerTable { get; private set; } = new Dictionary<string, PlayerStat>();

    protected override void OnInitialize()
    {
        Debug.Log("[DataManager] 플레이어 데이터 로드 시작...");
        LoadPlayerTables();
    }

    private void LoadPlayerTables()
    {
        string basePath = Path.Combine(Application.streamingAssetsPath, "TSVData");
        string playerPath = Path.Combine(basePath, "PlayerData.tsv");

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

        Debug.Log($"[DataManager] 로드 완료 - Player: {PlayerTable.Count}");
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