using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : SingletonBase<DataManager>
{
    // Player 데이터만 관리
    public Dictionary<string, PlayerStat> PlayerTable { get; private set; } = new Dictionary<string, PlayerStat>();

    private string SaveDirectory => Path.Combine(Application.persistentDataPath, "UserData");
    private string SavePath => Path.Combine(SaveDirectory, "SaveData.json");

    protected override void OnInitialize()
    {
        Debug.Log("[DataManager] 플레이어 데이터 로드 시작...");
        LoadPlayerTables();
    }

    public void SaveUserData(Vector3 position, PlayerStat stat)
    {
        // UserData 폴더가 없으면 생성
        if (Directory.Exists(SaveDirectory) == false)
        {
            Directory.CreateDirectory(SaveDirectory);
        }

        UserSaveData saveData = new UserSaveData();
        saveData.SetPosition(position);
        saveData.playerStat = stat;

        JsonWriter.Save(saveData, SavePath);
        Debug.Log($"[DataManager] 데이터 저장 완료: {SavePath}");
    }

    public UserSaveData LoadUserData()
    {
        var data = JsonReader.Load<UserSaveData>(SavePath);
        if (data != null)
        {
            Debug.Log("[DataManager] 데이터 로드 성공");
            return data;
        }
        
        Debug.LogWarning("[DataManager] 저장된 데이터를 찾을 수 없습니다.");
        return null;
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