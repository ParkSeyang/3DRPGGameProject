using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDiveRoll : MonoBehaviour
{
    private static readonly int DiveRollTrigger = Animator.StringToHash("DiveRoll");

    [Header("Components")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private AnimEventReceiver animEventReceiver;
    [SerializeField] private Collider playerHurtCollider; // 무적 판정을 위해 끌 콜라이더 (HurtBox)
    
    [Header("DiveRoll Settings")]
    [SerializeField] private float rollCooldown = 1.0f; // 구르기 사이의 최소 간격
    
    // [추가] 외부(PlayerController)에서 확인 가능한 속성
    public bool IsRolling { get; private set; }
    private float lastRollTime = -100f;

    private void Awake()
    {
        // 캐싱 및 컴포넌트 자동 탐색 (방어적 코드)
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
        if (animEventReceiver == null) animEventReceiver = GetComponent<AnimEventReceiver>();
        if (playerHurtCollider == null) playerHurtCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        if (animEventReceiver != null)
        {
            animEventReceiver.OnAnimationTriggerReceived += OnAnimationEvent;
        }
    }

    private void OnDisable()
    {
        if (animEventReceiver != null)
        {
            animEventReceiver.OnAnimationTriggerReceived -= OnAnimationEvent;
        }
    }

    private void Update()
    {
        if (CanRoll() == false) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartDiveRoll();
        }
    }

    private bool CanRoll()
    {
        // 1. 타이틀 씬 혹은 팝업 중 조작 차단
        string activeSceneName = SceneManager.GetActiveScene().name;
        bool isTitleScene = activeSceneName.Contains("StartGame") || activeSceneName.Contains("GameStart");
        if (isTitleScene || (UIManager.IsInitialized && UIManager.Instance.IsPopupOpen)) return false;

        // 2. 이미 구르는 중이거나 쿨타임인 경우 차단
        if (IsRolling == true) return false;
        if (Time.time - lastRollTime < rollCooldown) return false;

        return true;
    }

    private void StartDiveRoll()
    {
        IsRolling = true;
        lastRollTime = Time.time;

        // [중요] 구르기 시작 시 무적 상태 돌입 (HurtBox 비활성화)
        if (playerHurtCollider != null)
        {
            playerHurtCollider.enabled = false;
        }

        // 애니메이터 트리거 (블렌드 트리에 의해 현재 InputX, Y 방향으로 구름)
        playerAnimator.SetTrigger(DiveRollTrigger);
    }

    private void OnAnimationEvent(string eventName)
    {
        // 구르기 애니메이션의 특정 시점 혹은 끝점에서 호출될 이벤트 이름 (애니메이터에서 설정 필요)
        if (eventName.Equals("Roll_End", StringComparison.OrdinalIgnoreCase))
        {
            EndDiveRoll();
        }
    }

    public void EndDiveRoll()
    {
        if (IsRolling == false) return;

        IsRolling = false;

        // [중요] 구르기 종료 시 피격 판정 복구
        if (playerHurtCollider != null)
        {
            playerHurtCollider.enabled = true;
        }
    }
}

    