using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private static readonly int InputX = Animator.StringToHash("InputX");
    private static readonly int InputY = Animator.StringToHash("InputY");
    private static readonly int IsMove = Animator.StringToHash("IsMove");
    private static readonly int IsRun = Animator.StringToHash("IsRun");

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 10.0f;
    [SerializeField] private float runSpeed = 20.0f;

    [Header("Physics Settings")]
    [SerializeField] private float gravityScale = 5.0f; // 중력 배율
    [SerializeField] private float groundCheckDistance = 0.5f; 
    [SerializeField] private LayerMask groundLayer; 

    private Rigidbody playerRigidbody;
    private Animator playerAnimator;
    private PlayerDiveRoll playerDiveRoll;
    
    private Vector3 moveInput;
    private bool isGrounded;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        playerDiveRoll = GetComponent<PlayerDiveRoll>();
        
        // 유니티 기본 중력은 끄고 코드로 정밀 제어 (더 묵직하게)
        if (playerRigidbody != null) playerRigidbody.useGravity = false;
    }

    private void Update()
    {
        if (UIManager.IsInitialized && UIManager.Instance.IsPopupOpen)
        {
            ResetMovementState();
            return;
        }

        // [추가] 가드 중에는 이동 불가
        var playerGuard = GetComponent<PlayerGuard>();
        if (playerGuard != null && playerGuard.IsGuardActionActive) // [수정] 동작 완료까지 차단
        {
            ResetMovementState();
            return;
        }

        // [추가] 스킬 시전 중 이동 조작 차단
        var skillController = GetComponent<PlayerSkillController>();
        if (skillController != null && skillController.IsCasting)
        {
            ResetMovementState();
            return;
        }
        
        if (playerDiveRoll != null && playerDiveRoll.IsRolling) return;

        HandleInput();
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // [추가] 달리기 중 조작 제한 (W + LShift 시에만 달리기 허용 및 ASD 차단)
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && vertical > 0.1f;
        if (isSprinting == true)
        {
            horizontal = 0f; // A, D 키 무시
            // vertical은 이미 0.1 이상이므로 그대로 유지 (후진 입력은 자연스럽게 차단됨)
        }

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        moveInput = (forward * vertical + right * horizontal).normalized;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool isForwardRunning = isMoving && isSprinting;

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat(InputX, horizontal);
            playerAnimator.SetFloat(InputY, vertical);
            playerAnimator.SetBool(IsMove, isMoving);
            playerAnimator.SetBool(IsRun, isForwardRunning);
        }
    }

    private void FixedUpdate()
    {
        if (playerRigidbody == null || playerRigidbody.isKinematic) return;

        // [추가] 스킬 시전 중 물리 속도 완전 고정 (Vector3.zero)
        var skillController = GetComponent<PlayerSkillController>();
        if (skillController != null && skillController.IsCasting)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            return;
        }

        CheckGroundStatus();
        ApplyCustomGravity(); // [상시 실행] 구르기 중에도 중력은 작동해야 함

        // 구르기 중이면 수평 이동 입력만 무시하고 중력값은 유지
        if (playerDiveRoll != null && playerDiveRoll.IsRolling)
        {
            // [핵심] 구르기 중에도 중력(Y)은 계속 적용되어야 함
            // 수평 속도는 애니메이션 루트 모션이나 관성에 맡기고, 수직 속도만 갱신
            playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.y, playerRigidbody.linearVelocity.z);
            return;
        }

        ApplyMovement();
    }

    private void CheckGroundStatus()
    {
        // 바닥에 붙어있는지 레이캐스트로 체크
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void ApplyCustomGravity()
    {
        if (isGrounded && playerRigidbody.linearVelocity.y < 0)
        {
            // 바닥에 안착했다면 아주 작은 하방 속도만 유지 (기울기 대비)
            playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, -1.0f, playerRigidbody.linearVelocity.z);
        }
        else
        {
            // [수정] ForceMode.Force를 사용하여 캐릭터의 질량(Mass)이 반영된 묵직한 중력을 가합니다.
            playerRigidbody.AddForce(Vector3.down * (9.81f * gravityScale), ForceMode.Force);
        }
    }

    private void ApplyMovement()
    {
        float currentSpeed = playerAnimator.GetBool(IsRun) ? runSpeed : walkSpeed;

        // [핵심] 현재 중력(Y값)은 절대 건드리지 않고, X와 Z 속도만 교체합니다.
        // 이렇게 해야 중력 가속도가 덮어씌워지지 않고 실시간으로 작동합니다.
        Vector3 targetHorizontalVelocity = moveInput * currentSpeed;
        
        // 최종 속도 합산: (새로 계산한 수평 속도) + (이미 물리 엔진이 계산한 현재 수직 속도)
        playerRigidbody.linearVelocity = new Vector3(targetHorizontalVelocity.x, playerRigidbody.linearVelocity.y, targetHorizontalVelocity.z);
    }

    public void ResetMovementState()
    {
        moveInput = Vector3.zero;
        if (playerRigidbody != null && playerRigidbody.isKinematic == false)
        {
            playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, 0);
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat(InputX, 0);
            playerAnimator.SetFloat(InputY, 0);
            playerAnimator.SetBool(IsMove, false);
            playerAnimator.SetBool(IsRun, false);
        }
    }
}
