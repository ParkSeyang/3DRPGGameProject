using UnityEngine;

public class QuestNPC : NPC
{
    [Header("Quest Settings")]
    public string QuestKey;
    public string DialogueKey; // TSV의 Category (예: RepeatQuest_1) 기반 식별 키

    protected override void OnInteract()
    {
        var questManager = QuestManager.Instance;
        var activeQuest = questManager.GetActiveQuest(QuestKey);
        var dialogUI = UIManager.Instance.GetDialogueUI();

        if (dialogUI == null) return;

        // 1. 아직 수락하지 않은 상태
        if (activeQuest == null)
        {
            if (questManager.IsQuestCompletedForever(QuestKey))
            {
                UIManager.Instance.ShowWarning("이미 완료된 임무입니다.");
                return;
            }

            var questData = questManager.GetQuestData(QuestKey);
            if (questData == null) return;

            string startKey = $"{DialogueKey}_Start";
            var dialogs = DialogDataManager.Instance.GetDialogsByKey(startKey);
            
            if (dialogs != null)
            {
                dialogUI.StartQuestDialog(dialogs, NPCName, questData.Name,
                    () => {
                        questManager.AcceptQuest(QuestKey);
                        UIManager.Instance.ShowWarning($"{questData.Name} 임무를 수락했습니다.");
                        
                        // 튜토리얼 타입은 수락 즉시 진행도를 1로 올려 완료 가능 상태로 만듦
                        if (questData.Type == "Tutorial") 
                        {
                            var q = questManager.GetActiveQuest(QuestKey);
                            q.CurrentProgress = q.TargetProgress;
                        }
                    },
                    () => UIManager.Instance.ShowWarning("임무 제안을 거절했습니다.")
                );
            }
        }
        // 2. 진행 중인 상태
        else
        {
            // 미션 완료됨 -> 보상 지급 및 종료 대화
            if (activeQuest.IsCompleted)
            {
                string endKey = $"{DialogueKey}_End";
                var dialogs = DialogDataManager.Instance.GetDialogsByKey(endKey);
                dialogUI.StartDialog(dialogs, NPCName, activeQuest.Name, () => 
                {
                    questManager.CompleteQuest(QuestKey);
                });
            }
            // 미션 미완료 -> 독촉 대화 또는 일반 대사
            else
            {
                UIManager.Instance.ShowWarning($"임무가 아직 완료되지 않았습니다. ({activeQuest.CurrentProgress}/{activeQuest.TargetProgress})");
            }
        }
    }
}