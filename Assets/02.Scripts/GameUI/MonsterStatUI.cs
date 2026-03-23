using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterStatUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TextMeshProUGUI hpText; 
    [SerializeField] private TextMeshProUGUI nameText;  // [추가] 이름 표시
    [SerializeField] private TextMeshProUGUI levelText; // [추가] 레벨 표시
    [SerializeField] private GameObject uiRoot;

    [Header("Settings")]
    [SerializeField] private float detectionRadius = 15.0f; // 플레이어 감지 범위

    private Canvas canvas;
    private bool isVisibleByDistance = false;
    private float hideTimer = 0f;
    private const float HIDE_DELAY = 5.0f;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        
        if (uiRoot == gameObject)
        {
            Debug.LogWarning($"[MonsterStatUI] {gameObject.name}: uiRoot는 자식 오브젝트여야 합니다!");
        }

        if (uiRoot != null) uiRoot.SetActive(false);
    }

    /// <summary>
    /// 스폰 시 초기 데이터를 설정합니다.
    /// </summary>
    public void Initialize(string name, int level, float currentHP, float maxHP)
    {
        if (nameText != null) nameText.text = name;
        if (levelText != null) levelText.text = $"Lv.{level}";
        
        UpdateHPBar(currentHP, maxHP);
        
        if (uiRoot != null) uiRoot.SetActive(false);
        isVisibleByDistance = false;
        hideTimer = 0f;
    }

    public void UpdateHPBar(float currentHP, float maxHP)
    {
        if (hpFillImage == null) return;

        // 음수 체력 방지
        float displayHP = Mathf.Max(0, currentHP);
        float ratio = Mathf.Clamp01(displayHP / maxHP);
        hpFillImage.fillAmount = ratio;

        // [추가] 텍스트 갱신 (예: 50 / 100)
        if (hpText != null)
        {
            hpText.text = $"{displayHP:F0} / {maxHP:F0}";
        }

        // 피격 시 즉시 표시 (강제 표시 타이머 작동)
        ShowUIForTime();
    }

    private void ShowUIForTime()
    {
        if (uiRoot != null) uiRoot.SetActive(true);
        hideTimer = HIDE_DELAY;
    }

    private void Update()
    {
        // 1. 빌보드: 항상 카메라 정면 주시
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }

        // 2. 거리 기반 표시 체크
        CheckDistanceVisibility();

        // 3. UI 최종 노출 상태 결정
        bool shouldShow = (hideTimer > 0) || isVisibleByDistance;
        if (uiRoot != null) uiRoot.SetActive(shouldShow);

        // 4. 피격 노출 타이머 감소
        if (hideTimer > 0) hideTimer -= Time.deltaTime;
    }

    private void CheckDistanceVisibility()
    {
        if (Player.Instance == null)
        {
            isVisibleByDistance = false;
            return;
        }

        // 높이를 무시한 평면 거리 계산 (TransformExtensions 활용)
        float distance = transform.FlatDistanceTo(Player.Instance.transform);
        isVisibleByDistance = distance <= detectionRadius;
    }
}
