using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Slime : MonoBehaviour
{
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Dead = Animator.StringToHash("Dead");
    public float MoveSpeed { get; set; } = 3.0f;

    [Header("AI 설정")] 
    [SerializeField] private float patrolRadius = 10.0f;
    
    [Header("몬스터의 탐지 범위 설정")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform eyeTransform;
    [SerializeField] private float detectionRadius = 10.0f;
    [SerializeField, Range(0, 360)] 
    // 부채꼴 시야각을 위한 범위 지정
    private float detectionAngle = 90.0f;
    [SerializeField] private LayerMask playerLayer; // 플레이어 탐지를 위한 레이어
    [SerializeField] private LayerMask obstacleLayer; // 장애물 탐지를 위한 레이어
    
    
    [Header("애니메이션 및 Collider")]
    [SerializeField] private Collider AttackCollider;
    [SerializeField] private AnimEventReceiver AnimEventReceiver;


    [Header("테스트용 사망 로직")] 
    [SerializeField] private int maxHitCount = 3;

    public int CurrentHitCount { get; private set; } = 0;

    // 개발 초기 테스트를 위해서 공개프로퍼티로 생성
    public Transform Target => target;
    public Transform EyeTransform => eyeTransform;
    public float PatrolRadius => patrolRadius;
    public float DetectionRadius => detectionRadius;
    public float DetectionAngle => detectionAngle;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask ObstacleLayer => obstacleLayer;
    
    
    private Animator Animator { get; set; }
    private NavMeshAgent Agent { get; set; }
    private Dictionary<Type, SlimeBaseState> States { get; set; }
    public SlimeBaseState CurrentState { get; set; }
    private SlimeBaseState DefaultState { get; set; }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        AttackCollider.enabled = false;

        States = new Dictionary<Type, SlimeBaseState>();
        States.Add(typeof(SlimeIdleState), new SlimeIdleState());
        States.Add(typeof(SlimePatrolState), new SlimePatrolState());
        States.Add(typeof(SlimeChaseState), new SlimeChaseState());
        States.Add(typeof(SlimeAttackState), new SlimeAttackState());
        States.Add(typeof(SlimeHitState), new SlimeHitState());
        States.Add(typeof(SlimeDeadState), new SlimeDeadState());
        
        DefaultState = States[typeof(SlimeIdleState)];

        var param = new SlimeBaseState.StateControllerParameter
        {
            slime = this,
            attackCollider = AttackCollider,
            slimeAnimator = Animator,
            animEventReceiver = AnimEventReceiver,
            agent = Agent,
   
        };
        
        foreach (var state in States.Values)
        {
            state.Initialize(param);
        }
        
    }

    private void Start()
    {
        ChangeState<SlimeIdleState>();
    }
    
    private void Update()
    {
        CurrentState.UpdateState();
    }

    public void ChangeState<T>() where T : SlimeBaseState
    {
        var prevState = CurrentState;
        prevState?.ExitState();

        CurrentState = DefaultState;
        if (States.ContainsKey(typeof(T)))
        {
            CurrentState = States[typeof(T)];
        }
        
        CurrentState.EnterState();
        Debug.Log($"{prevState?.GetType().Name} changed to {CurrentState.GetType().Name}");
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            if ((CurrentState is SlimeDeadState) == false)
            {
                var attacker = other.GetComponentInParent<PlayerController>();
                if (attacker != null)
                {
                    SetTarget(attacker.transform);
                }

                CurrentHitCount++;
                Debug.Log($"슬라임이 피격당했다! {CurrentHitCount} / {maxHitCount}");
                if (CurrentHitCount >= maxHitCount)
                {
                    ChangeState<SlimeDeadState>();
                }
                else
                {
                    ChangeState<SlimeHitState>();
                }
            }
        }
    }

    
    
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (eyeTransform == null)
        {
            return;
        }
        // 탐지 반경을 하얀색 와이어 스피어로 그립니다.
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(eyeTransform.position, detectionRadius);
        
        // 시야각(부채꼴)을 그립니다.
        Handles.color = new Color(1f,1f,0f, 0.2f);
        
        // 부채꼴의 시작 방향을 계산합니다.
        Vector3 rangeDirection = Quaternion.Euler(0, -detectionAngle / 2, 0) * eyeTransform.forward;
        
        // 채워진 부채꼴을 그려줍니다.
        Handles.DrawSolidArc(
            eyeTransform.position, // 중심점
            eyeTransform.up, // 부채꼴이 그려질 평면의 법선 백터(몬스터의 위 방향)
            rangeDirection, // 부채꼴의 시작 방향
            detectionAngle,      // 부채꼴의 총 각도
            detectionRadius);   // 부채꼴의 반 지름

        Handles.color = Color.yellow;
        Vector3 leftDirection = rangeDirection; // 시작 방향과 동일
        Vector3 rightDirection = Quaternion.Euler(0, detectionAngle / 2, 0) * eyeTransform.forward;
        
        Handles.DrawLine(eyeTransform.position, eyeTransform.position + rightDirection * detectionRadius, 2f);
        Handles.DrawLine(eyeTransform.position, eyeTransform.position + leftDirection * detectionRadius, 2f);
    }
#endif
    
    
}
