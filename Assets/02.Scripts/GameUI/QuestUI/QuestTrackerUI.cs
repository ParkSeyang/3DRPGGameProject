using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestTrackerUI : MonoBehaviour
{
    [System.Serializable]
    public struct QuestBoxUI
    {
        public GameObject root; // MiniBox 오브젝트
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
    }

    [Header("UI Components")]
    [SerializeField] private List<QuestBoxUI> questBoxes;

    public void RefreshTracker()
    {
        if (QuestManager.Instance == null) return;

        var activeQuests = QuestManager.Instance.GetActiveQuests();

        for (int i = 0; i < questBoxes.Count; i++)
        {
            if (i < activeQuests.Count)
            {
                var quest = activeQuests[i];
                if (questBoxes[i].root != null)
                {
                    questBoxes[i].root.SetActive(true);
                    if (questBoxes[i].titleText != null) questBoxes[i].titleText.text = quest.Name;
                    if (questBoxes[i].descriptionText != null)
                    {
                        if (quest.IsCompleted == true)
                        {
                            questBoxes[i].descriptionText.text = "<color=yellow>Complete! (Talk to NPC)</color>";
                        }
                        else
                        {
                            questBoxes[i].descriptionText.text = $"{quest.TargetName} : {quest.CurrentProgress}/{quest.TargetProgress}";
                        }
                    }
                }
            }
            else
            {
                if (questBoxes[i].root != null) questBoxes[i].root.SetActive(false);
            }
        }
    }
}
