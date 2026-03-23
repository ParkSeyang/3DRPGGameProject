using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGuard : MonoBehaviour
{
    private static readonly int IsGuard = Animator.StringToHash("IsGuard");
    private static readonly int Guard = Animator.StringToHash("Guard");

    [Header("Components")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private AnimEventReceiver animEventReceiver; // [추가] 이벤트 수신용
    [SerializeField] private Collider guardCollider; // 가드 영역 콜라이더 (Shield 등)
    [SerializeField] private Transform effectPoint; // 가드 이펙트 생성 지점 (검 등)
    
    [Header("Guard Logic Settings")]
    [Tooltip("가드 시 비활성화할 본체 HurtBox 리스트 (인스펙터에서 직접 할당)")]
    [SerializeField] private HurtBox[] bodyHurtBoxes; 

    public Transform EffectPoint => effectPoint;
    private HurtBox guardHurtBox;

    // 현재 플레이어가 가드 키를 누르고 있는지 여부
    public bool IsGuarding { get; private set; }
    // [추가] 가드 동작(해제 애니메이션 포함)이 진행 중인지 여부
    public bool IsGuardActionActive { get; private set; }

    private void Awake()
    {
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
        if (animEventReceiver == null) animEventReceiver = GetComponent<AnimEventReceiver>();

        // [수정] 이제 bodyHurtBoxes는 인스펙터에서 직접 할당한 것을 사용합니다.
        // Awake에서의 자동 수집 로직 제거 (사용자 제어권 강화)

        if (guardCollider != null)
        {
            guardHurtBox = guardCollider.GetComponent<HurtBox>();
            if (guardHurtBox == null)
            {
                guardHurtBox = guardCollider.gameObject.AddComponent<HurtBox>();
            }
            guardCollider.isTrigger = true;
            guardCollider.enabled = false; 
        }
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

    private void Start()
    {
        // 전투 시스템에 가드 박스 등록
        if (guardHurtBox != null && PlayerStatusController.Instance != null)
        {
            guardHurtBox.Initialize(PlayerStatusController.Instance);
        }
    }

    private void Update()
    {
        if (CanGuard() == false)
        {
            if (IsGuarding == true) StopGuarding();
            return;
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (IsGuarding == false) StartGuarding();
        }
        else
        {
            if (IsGuarding == true) StopGuarding();
        }
    }

    private bool CanGuard()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        bool isTitle = activeSceneName.Contains("StartGame") || activeSceneName.Contains("GameStart");
        if (isTitle || (UIManager.IsInitialized && UIManager.Instance.IsPopupOpen)) return false;

        return true;
    }

    private void StartGuarding()
    {
        IsGuarding = true;
        IsGuardActionActive = true; 
        playerAnimator.SetTrigger(Guard);
        playerAnimator.SetBool(IsGuard, true);

        // [제거] 즉시 판정 교체 로직을 제거하고 애니메이션 이벤트로 위임합니다.
    }

    private void StopGuarding()
    {
        IsGuarding = false;
        playerAnimator.SetBool(IsGuard, false);

        // [제거] 즉시 판정 교체 로직을 제거합니다.
    }

    // [추가] 외부에서 강제로 가드 동작을 해제할 때 (피격 등)
    public void CancelGuardAction()
    {
        IsGuarding = false;
        IsGuardActionActive = false;
        playerAnimator.SetBool(IsGuard, false);
        
        // 판정 강제 복구
        SetBodyHurtBoxesActive(true);
        if (guardCollider != null) guardCollider.enabled = false;
    }

    private void OnAnimationEvent(string eventName)
    {
        // 1. 가드 판정 활성화 (방패를 완전히 들어 올린 시점)
        if (eventName.Equals("Guard_On", StringComparison.OrdinalIgnoreCase))
        {
            SetBodyHurtBoxesActive(false);
            if (guardCollider != null) guardCollider.enabled = true;
        }
        // 2. 가드 판정 비활성화 (방패를 내리기 시작하는 시점)
        else if (eventName.Equals("Guard_Off", StringComparison.OrdinalIgnoreCase))
        {
            SetBodyHurtBoxesActive(true);
            if (guardCollider != null) guardCollider.enabled = false;
        }
        // 3. 가드 전체 동작 종료 (이동 가능해지는 시점)
        else if (eventName.Equals("Guard_End", StringComparison.OrdinalIgnoreCase))
        {
            IsGuardActionActive = false;
        }
    }

    private void SetBodyHurtBoxesActive(bool isActive)
    {
        if (bodyHurtBoxes == null) return;
        
        foreach (var hurtBox in bodyHurtBoxes)
        {
            // 가드 콜라이더 자신은 제외하고 나머지(몸통 등)만 제어
            if (hurtBox != guardHurtBox && hurtBox.Collider != null)
            {
                hurtBox.Collider.enabled = isActive;
            }
        }
    }

    /// <summary>
    /// 맞은 콜라이더가 가드용 콜라이더인지 확인합니다.
    /// </summary>
    public bool IsGuardCollider(Collider hitCollider)
    {
        return guardCollider != null && guardCollider == hitCollider;
    }
}