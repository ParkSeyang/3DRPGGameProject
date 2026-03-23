using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : SingletonBase<DataManager>
{
    // Player 데이터만 관리
    public Dictionary<string, PlayerStat> PlayerTable { get; private set; } = new Dictionary<string, PlayerStat>();

    private string SaveDirectory => Path.Combine(Application.persistentDataPath, "UserData");
    private string SavePath => Path.Combine(SaveDirectory, "SaveData.json");

    // --- 저장 정책 관련 필드 ---
    public bool CanSave { get; set; } = true; // 기본적으로는 저장 가능 (마을 등)
    
    /// <summary>
    /// 현재 씬이 자유 저장이 가능한 곳인지, 아니면 특정 구역이 필요한 곳인지 판단합니다.
    /// </summary>
    public void UpdateSavePolicy(string sceneName)
    {
        // 사냥터(BeginnersForest)인 경우 기본적으로 저장을 막음
        if (sceneName.Contains("BeginnersForest"))
        {
            CanSave = false;
        }
        else
        {
            CanSave = true; // 마을 등은 자유 저장
        }
    }

    protected override void OnInitialize()
    {
        LoadPlayerTables();
    }

    public void SaveUserData(UserSaveData saveData)
    {
        if (saveData == null) return;

        // UserData 폴더가 없으면 생성
        if (Directory.Exists(SaveDirectory) == false)
        {
            Directory.CreateDirectory(SaveDirectory);
        }

        JsonWriter.Save(saveData, SavePath);
    }

    /// <summary>
    /// 기존 세이브 파일을 물리적으로 삭제합니다.
    /// </summary>
    public void DeleteSaveData()
    {
        if (File.Exists(SavePath) == true)
        {
            File.Delete(SavePath);
        }
    }

    public UserSaveData LoadUserData()
    {
        var data = JsonReader.Load<UserSaveData>(SavePath);
        if (data != null)
        {
            return data;
        }
        
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
            }
        }
    }

    public PlayerStat GetPlayerStat(string name)
    {
        if (PlayerTable.TryGetValue(name, out var stat))
        {
            return stat;
        }
        return null;
    }
}

    