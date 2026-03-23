using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyDataManager : SingletonBase<EnemyDataManager>
{
    public Dictionary<int, EnemyStat> EnemyTable { get; private set; } = new Dictionary<int, EnemyStat>();
    
        protected override void OnInitialize() => LoadEnemyTables();

    private void LoadEnemyTables()
    {
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
            }
        }
    }

    public EnemyStat GetEnemyStat(int id)
    {
        if (EnemyTable.ContainsKey(id))
        {
            return EnemyTable[id];
        }
        return null;
    }
}
    
    