using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class QuestManager : SingletonBase<QuestManager>
{
    private Dictionary<string, QuestData> questTable = new Dictionary<string, QuestData>();
    private List<Quest> activeQuests = new List<Quest>();
    private HashSet<string> completedQuests = new HashSet<string>();

    protected override void OnInitialize()
    {
        LoadQuestTable();
    }

    private void LoadQuestTable()
    {
        string tsvPath = Path.Combine(Application.streamingAssetsPath, "TSVData", "QuestData.tsv");
        var questDataList = TSVReader.ReadTable<QuestData>(tsvPath);
        
        if (questDataList != null)
        {
            questTable.Clear();
            foreach (var questData in questDataList)
            {
                if (string.IsNullOrEmpty(questData.Key)) continue;
                questTable[questData.Key] = questData;
            }
        }
    }

    public QuestData GetQuestData(string key) => questTable.TryGetValue(key, out var data) ? data : null;

    public event Action OnQuestUpdated;

    public void AcceptQuest(string key)
    {
        if (activeQuests.Any(q => q.Key == key)) return;
        if (completedQuests.Contains(key)) return;
        
        var questData = GetQuestData(key);
        if (questData != null)
        {
            Quest newQuest = ScriptableObject.CreateInstance<Quest>();
            newQuest.Initialize(questData);
            activeQuests.Add(newQuest);
            OnQuestUpdated?.Invoke();
        }
    }

    public void UpdateKillQuest(string monsterName) => UpdateProgress(monsterName);
    public void UpdateBuyQuest(string itemID) => UpdateProgress(itemID);
    public void UpdateTalkQuest(string dialogKey) => UpdateProgress(dialogKey);

    private void UpdateProgress(string targetID)
    {
        if (string.IsNullOrEmpty(targetID) == true) return;

        bool isStateChanged = false;
        foreach (var quest in activeQuests)
        {
            if (quest.IsCompleted == false && quest.TargetID == targetID)
            {
                quest.CurrentProgress++;
                isStateChanged = true;
            }
        }

        if (isStateChanged == true) OnQuestUpdated?.Invoke();
    }

    public Quest GetActiveQuest(string key) => activeQuests.Find(q => q.Key == key);
    public List<Quest> GetActiveQuests() => activeQuests;
    public bool IsQuestCompletedForever(string key) => completedQuests.Contains(key);

    public void CompleteQuest(string key)
    {
        var targetQuest = GetActiveQuest(key);
        if (targetQuest != null && targetQuest.IsCompleted == true)
        {
            // 1. 아이템 제출 퀘스트 처리 (실제 존재하는 아이템 ID인지 확인)
            if (string.IsNullOrEmpty(targetQuest.TargetID) == false && ItemDataManager.Instance != null && ItemDataManager.Instance.GetItem(targetQuest.TargetID) != null)
            {
                var userInven = InventorySystem.Instance.GetInventoryOrNull("User");
                if (userInven != null)
                {
                    // 정확히 일치하는 아이템 ID(예: "I001")만 인벤토리에서 찾아 제거합니다.
                    userInven.RemoveItem(targetQuest.TargetID, targetQuest.TargetProgress);
                }
            }

            // 2. 보상 지급
            if (PlayerStatusController.IsInitialized == true)
            {
                PlayerStatusController.Instance.AddGold(targetQuest.RewardGold);
                PlayerStatusController.Instance.AddExp(targetQuest.RewardExp);
            }

            if (targetQuest.Type != QuestType.Repeat) completedQuests.Add(key); 
            
            activeQuests.Remove(targetQuest);
            OnQuestUpdated?.Invoke();
        }
    }

    public QuestSaveContainer GetSaveData()
    {
        var saveContainer = new QuestSaveContainer();
        saveContainer.completedQuestKeys = completedQuests.ToList();
        
        foreach (var quest in activeQuests)
        {
            saveContainer.activeQuests.Add(new ActiveQuestSaveData 
            { 
                questKey = quest.Key, 
                currentProgress = quest.CurrentProgress 
            });
        }
        
        return saveContainer;
    }

    public void LoadSaveData(QuestSaveContainer saveContainer)
    {
        if (saveContainer == null) return;

        activeQuests.Clear();
        completedQuests.Clear();

        if (saveContainer.completedQuestKeys != null)
        {
            completedQuests = new HashSet<string>(saveContainer.completedQuestKeys);
        }

        if (saveContainer.activeQuests != null)
        {
            foreach (var activeQuestData in saveContainer.activeQuests)
            {
                var questData = GetQuestData(activeQuestData.questKey);
                if (questData != null)
                {
                    Quest restoredQuest = ScriptableObject.CreateInstance<Quest>();
                    restoredQuest.Initialize(questData);
                    restoredQuest.CurrentProgress = activeQuestData.currentProgress;
                    activeQuests.Add(restoredQuest);
                }
            }
        }
    }
}
