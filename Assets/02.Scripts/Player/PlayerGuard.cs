using System;
using UnityEngine;

public class PlayerGuard : MonoBehaviour
{
    private static readonly int Guard = Animator.StringToHash("Guard");
    private static readonly int IsGuard = Animator.StringToHash("IsGuard");
    private Animator animator;
    private AnimEventReceiver animEventReceiver;
    
    [SerializeField] private Collider guardCollider;
    private HurtBox guardHurtBox;

    // 조건식을 체크해서 해주자.

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animEventReceiver = GetComponent<AnimEventReceiver>();
        
        if (guardCollider != null)
        {
            guardHurtBox = guardCollider.GetComponent<HurtBox>();
            if (guardHurtBox == null)
            {
                guardHurtBox = guardCollider.gameObject.AddComponent<HurtBox>();
            }
            guardCollider.enabled = false; // 기본은 꺼둠
        }
    }
    
    // Player.cs에서 피격 시 호출하여 가드 성공 여부를 판단
    public bool IsGuardSuccess(Collider hitCollider)
    {
        return guardCollider != null && guardCollider == hitCollider;
    }

    private void Start()
    {
        // PlayerStatusController가 전투 에이전트이므로 그쪽으로 연결
        if (guardHurtBox != null && PlayerStatusController.Instance != null)
        {
            guardHurtBox.Initialize(PlayerStatusController.Instance);
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            animator.SetBool(IsGuard, true);
            if (guardCollider != null) guardCollider.enabled = true;
        }
        else
        {
            animator.SetBool(IsGuard, false);
            if (guardCollider != null) guardCollider.enabled = false;
        }
    }
    
 
   
}
