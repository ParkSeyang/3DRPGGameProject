using UnityEngine;
using System.Collections.Generic;

public class GuideObject : NPC
{
    [Header("Guide Settings")]
    public string DialogueKey = "Guide";

    protected override void OnInteract()
    {
        var dialogUI = UIManager.Instance.GetDialogueUI();
        if (dialogUI == null) return;

        // 1. Guide_Start 대화 가져오기
        string startKey = $"{DialogueKey}_Start";
        var startDialogs = DialogDataManager.Instance.GetDialogsByKey(startKey);

        if (startDialogs != null)
        {
            // 첫 번째 대화 시작
            dialogUI.StartDialog(startDialogs, NPCName, "", () => 
            {
                // 2. 첫 번째 대화가 끝나면 Guide_End 대화 가져오기
                string endKey = $"{DialogueKey}_End";
                var endDialogs = DialogDataManager.Instance.GetDialogsByKey(endKey);

                if (endDialogs != null)
                {
                    // 두 번째 대화 시작
                    dialogUI.StartDialog(endDialogs, NPCName, "", () => 
                    {
                        // 3. 모든 대화가 끝나면 오브젝트 삭제
                        Destroy(gameObject);
                    });
                }
                else
                {
                    // End 대화가 없으면 바로 삭제
                    Destroy(gameObject);
                }
            });
        }
    }
}