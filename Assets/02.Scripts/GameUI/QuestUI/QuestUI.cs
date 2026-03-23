using UnityEngine;

public class QuestUI : BaseUI
{
    // QuestUICanvas는 팝업이 아닌 HUD(추적기) 역할을 수행함
    public override UIType UIType => UIType.Quest; 
    public override bool IsPopup => false; 

    [Header("Quest Tracker Reference")]
    [SerializeField] private QuestTrackerUI tracker;

    protected override void Awake()
    {
        // 퀘스트 UI는 항상 관리자에 등록되어야 하지만, HUD로 취급되어야 함
        base.Awake();
        if (tracker == null) tracker = GetComponentInChildren<QuestTrackerUI>(true);
    }

    protected override void Start()
    {
        base.Start();

        if (QuestManager.IsInitialized == true)
        {
            QuestManager.Instance.OnQuestUpdated += Refresh;
        }
        Refresh();
    }

    public void ShowQuest(string key)
    {
        // 퀘스트 수락 시 추적기 강제 갱신
        Refresh();
    }

    public override void Refresh()
    {
        if (tracker == null) return;

        // [안전장치] QuestManager가 없거나 초기화 전이면 로직 수행 안 함
        if (QuestManager.IsInitialized == false || QuestManager.Instance == null)
        {
            tracker.gameObject.SetActive(false);
            return;
        }

        // 현재 진행 중인 퀘스트가 있는지 확인
        var activeQuests = QuestManager.Instance.GetActiveQuests();
        bool hasActiveQuest = (activeQuests != null && activeQuests.Count > 0);

        // 1. 퀘스트가 있을 때만 QuestBox(tracker) 자체를 활성화
        tracker.gameObject.SetActive(hasActiveQuest);

        // 2. 활성화된 경우에만 리스트를 갱신
        if (hasActiveQuest == true)
        {
            tracker.RefreshTracker();
        }
    }

    private void OnDestroy()
    {
        // 싱글톤이 파괴된 후 접근하는 것을 막기 위해 Instance 직접 체크 추가
        if (QuestManager.IsInitialized == true && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= Refresh;
        }
    }
}
