using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class DialogData
{
    public string Category { get; set; } // 화자 또는 그룹 (General, Smithy 등)
    public string Key { get; set; }      // 대화 식별 키 (General_1, Smithy_Start 등)
    public string Dialogue { get; set; } // 실제 대화 내용
}

public class DialogDataManager : SingletonBase<DialogDataManager>
{
    // 전체 대화 리스트
    private List<DialogData> allDialogs = new List<DialogData>();

    protected override void OnInitialize()
    {
        LoadDialogTable();
    }

    private void LoadDialogTable()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "TSVData", "DialogData.tsv");
        
        // TSVReader를 통해 데이터 로드
        var list = TSVReader.ReadTable<DialogData>(path);

        if (list != null)
        {
            allDialogs = list;
        }
    }

    /// <summary>
    /// 특정 Key와 일치하는 모든 대화 데이터를 가져옵니다.
    /// (예: Key가 'General_1'인 대화들)
    /// </summary>
    public List<DialogData> GetDialogsByKey(string key)
    {
        if (allDialogs == null || string.IsNullOrEmpty(key)) return null;
        
        string targetKey = key.Trim();
        var result = allDialogs.Where(dialog => dialog.Key.Trim().Equals(targetKey)).ToList();
        return result.Count > 0 ? result : null;
    }

    /// <summary>
    /// 특정 Category에 속한 대화 중 하나를 무작위로 가져옵니다. (일반 NPC용)
    /// </summary>
    public List<DialogData> GetRandomDialogByCategory(string category)
    {
        if (allDialogs == null || string.IsNullOrEmpty(category)) return null;

        string targetCategory = category.Trim();
        // 해당 카테고리의 유니크한 Key 목록 추출
        var keys = allDialogs
            .Where(dialog => dialog.Category.Trim().Equals(targetCategory))
            .Select(dialog => dialog.Key.Trim())
            .Distinct()
            .ToList();

        if (keys.Count == 0) return null;

        // 무작위 키 선택 후 해당 키의 대화 세트 반환
        string randomKey = keys[UnityEngine.Random.Range(0, keys.Count)];
        return GetDialogsByKey(randomKey);
    }
}
