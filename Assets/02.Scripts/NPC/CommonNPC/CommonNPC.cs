using UnityEngine;

public class CommonNPC : NPC
{
    [Header("Common NPC Settings")]
    public string DialogueKey; // TSV의 Category (General, Guard 등) 기반 식별 키

    protected override void OnInteract()
    {
        // 해당 다이얼로그 키(카테고리) 내에서 무작위 대화 하나를 가져옴
        var dialogs = DialogDataManager.Instance.GetRandomDialogByCategory(DialogueKey);
        
        if (dialogs != null)
        {
            var dialogUI = UIManager.Instance.GetDialogueUI();
            dialogUI?.StartDialog(dialogs, NPCName);
        }
        else
        {
            Debug.LogWarning($"[CommonNPC] '{DialogueKey}' 키에 해당하는 대화 데이터를 찾을 수 없습니다.");
        }
    }

}
