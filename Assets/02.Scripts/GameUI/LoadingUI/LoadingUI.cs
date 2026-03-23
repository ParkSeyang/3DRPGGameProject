using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUI : BaseUI
{
    public override UIType UIType => UIType.Loading;

    [Header("UI Components")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TextMeshProUGUI progressText;

    protected override void Awake()
    {
        // 로딩 UI는 항상 최상단에 있어야 하므로 UIManager 관리 대상에서 제외하거나
        // 별도의 정렬 순서를 가집니다. 여기서는 관리 대상에 포함하되 레이어만 높게 씁니다.
        base.Awake();
        if (loadingSlider != null) loadingSlider.value = 0f;
        if (progressText != null) progressText.text = "0%";
    }

    /// <summary>
    /// 로딩 진행률을 갱신합니다. (0.0 ~ 1.0)
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (loadingSlider != null) loadingSlider.value = progress;
        if (progressText != null) progressText.text = $"{(progress * 100f):F0}%";
    }

    public override void Open()
    {
        base.Open();
        UpdateProgress(0f);
        // 다른 UI들보다 항상 위에 보이도록 설정
        transform.SetAsLastSibling();
    }
}
