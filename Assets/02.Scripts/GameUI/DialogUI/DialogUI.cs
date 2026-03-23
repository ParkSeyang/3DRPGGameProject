using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogUI : BaseUI
{
    public override UIType UIType => UIType.Dialogue;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private List<DialogData> currentDialogs;
    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    
    private System.Action onYesAction;
    private System.Action onNoAction;
    private System.Action onCompleteAction; // 대화 종료 콜백 추가
    
    private float openTime;
    private string currentSpeakerName;
    private string currentQuestName;

    protected override void Awake()
    {
        base.Awake();
        yesButton?.onClick.AddListener(() => { onYesAction?.Invoke(); Close(); });
        noButton?.onClick.AddListener(() => { onNoAction?.Invoke(); Close(); });
        
        SetChoiceButtonsActive(false);
    }

    public override void Open()
    {
        base.Open();
        openTime = Time.unscaledTime;
        if (UIManager.IsInitialized) UIManager.Instance.RefreshUIState();
    }

    public override void Close()
    {
        base.Close();
        if (UIManager.IsInitialized) UIManager.Instance.RefreshUIState();
    }

    // 일반 대화 시작 (종료 콜백 포함 가능)
    public void StartDialog(List<DialogData> dialogs, string speakerName, string questName = "", System.Action onComplete = null)
    {
        if (dialogs == null || dialogs.Count == 0) return;

        this.currentDialogs = dialogs;
        this.currentSpeakerName = speakerName;
        this.currentQuestName = questName;
        this.currentIndex = 0;
        this.onYesAction = null;
        this.onNoAction = null;
        this.onCompleteAction = onComplete; // 콜백 저장

        SetChoiceButtonsActive(false);
        if (questNameText != null) questNameText.text = questName;

        Open();
        ShowNextDialog();
    }

    // 퀘스트 대화 시작
    public void StartQuestDialog(List<DialogData> dialogs, string speakerName, string questName, System.Action onYes, System.Action onNo)
    {
        this.currentDialogs = dialogs;
        this.currentSpeakerName = speakerName;
        this.currentQuestName = questName;
        this.currentIndex = 0;
        this.onYesAction = onYes;
        this.onNoAction = onNo;
        this.onCompleteAction = null; // 퀘스트는 Yes/No로 종료되므로 일반 콜백은 비움

        SetChoiceButtonsActive(false);
        if (questNameText != null) questNameText.text = questName;

        Open();
        ShowNextDialog();
    }

    private void Update()
    {
        if (currentDialogs == null || gameObject.activeSelf == false) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F))
        {
            if (Time.unscaledTime - openTime < 0.1f) return;

            if (IsChoiceActive()) return;

            if (isTyping == true) 
            {
                FinishTyping();
            }
            else 
            {
                ShowNextDialog();
            }
        }
    }

    private bool IsChoiceActive()
    {
        return yesButton != null && yesButton.gameObject.activeSelf == true;
    }

    private void ShowNextDialog()
    {
        if (currentDialogs == null || currentDialogs.Count == 0)
        {
            Close();
            return;
        }

        if (currentIndex >= currentDialogs.Count)
        {
            if (currentIndex > 0)
            {
                string lastKey = currentDialogs[currentIndex - 1].Key;
                if (QuestManager.IsInitialized) QuestManager.Instance.UpdateTalkQuest(lastKey);
            }

            if (onYesAction != null)
            {
                SetChoiceButtonsActive(true);
            }
            else
            {
                Close();
                onCompleteAction?.Invoke(); // 대화가 끝나고 창이 닫힐 때 콜백 실행
            }
            return;
        }

        var data = currentDialogs[currentIndex];
        nameText.text = string.IsNullOrEmpty(currentSpeakerName) ? data.Category : currentSpeakerName;
        
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(data.Dialogue));
        
        currentIndex++;
    }

    private IEnumerator TypeText(string context)
    {
        isTyping = true;
        descriptionText.text = "";

        foreach (char letter in context.ToCharArray())
        {
            descriptionText.text += letter;
            yield return new WaitForSecondsRealtime(0.02f);
        }

        isTyping = false;
    }

    private void FinishTyping()
    {
        if (isTyping == false) return;
        StopCoroutine(typingCoroutine);
        descriptionText.text = currentDialogs[currentIndex - 1].Dialogue;
        isTyping = false;
    }

    private void SetChoiceButtonsActive(bool isActive)
    {
        if (yesButton != null) 
        {
            yesButton.gameObject.SetActive(isActive);
            if (isActive == true) yesButton.transform.SetAsLastSibling();
        }
        
        if (noButton != null) 
        {
            noButton.gameObject.SetActive(isActive);
            if (isActive == true) noButton.transform.SetAsLastSibling();
        }

        if (isActive == true)
        {
            Canvas.ForceUpdateCanvases();
        }
    }
}