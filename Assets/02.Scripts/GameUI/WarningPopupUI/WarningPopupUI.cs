using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WarningPopupUI : BaseUI
{
    public override UIType UIType => UIType.WarningPopup;

    [Header("Components")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;

    protected override void Awake()
    {
        base.Awake();
        confirmButton?.onClick.AddListener(Close);
    }

    public void Show(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
        Open();
    }

    public override void Open()
    {
        // 팝업이 열릴 때 최상단으로 오도록 설정 가능
        transform.SetAsLastSibling();
        base.Open();
    }
}
