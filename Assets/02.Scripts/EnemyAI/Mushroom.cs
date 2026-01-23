using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Mushroom : MonoBehaviour
{
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Dead = Animator.StringToHash("Dead");
    
    [SerializeField] private Collider AttackCollider;
    [SerializeField] private AnimEventReceiver AnimEventReceiver;

    [SerializeField] private Transform target;
    
    public Transform Player { get; private set; }
    [Header("몬스터의 탐지 범위 설정")]
    [SerializeField] private float detectionRadius = 10.0f;
    [SerializeField, Range(0, 360)] 
    // 부채꼴 시야각을 위한 범위 지정
    private float detecionAngle = 90.0f;
    [SerializeField] private LayerMask playerLayer; // 플레이어 탐지를 위한 레이어
    [SerializeField] private LayerMask obstacleLayer; // 장애물 탐지를 위한 레이어
    [SerializeField] private Transform eyeTransform;
    
    private Animator Animator { get; set; }
    private NavMeshAgent Agent { get; set; }
    private Dictionary<Type, BaseState> States { get; set; }
    private BaseState CurrentState { get; set; }
    private BaseState DefaultState { get; set; }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        AttackCollider.enabled = false;

        States = new Dictionary<Type, BaseState>();
        States.Add(typeof(IdleState), new IdleState());
        States.Add(typeof(PatrolState), new PatrolState());
        States.Add(typeof(ChaseState), new ChaseState());
        States.Add(typeof(AttackState), new AttackState());
        States.Add(typeof(HitState), new HitState());
        States.Add(typeof(DeadState), new DeadState());
        
        DefaultState = States[typeof(IdleState)];

        var param = new BaseState.StateControllerParameter
        {
            mushroom = this,
            attackCollider = AttackCollider,
            mushroomAnimator = Animator,
            AnimEventReceiver = AnimEventReceiver,
            agent = Agent,
            target = target,
            eyeTransform = eyeTransform,
            playerLayer = playerLayer,
            obstacleLayer = obstacleLayer,
            detectionRadius = detectionRadius,
            detectionAngle = detecionAngle
        };
        
       // BaseState.StateControllerParameter param = 
       //     new BaseState.StateControllerParameter();

       // param.mushroom = this;
       // param.attackCollider = AttackCollider;
       // param.mushroomAnimator = Animator;
       // param.AnimEventReceiver = AnimEventReceiver;
        
        
        
        foreach (var state in States.Values)
        {
            state.Initialize(param);
        }
        
    }

    private void Update()
    {
        CurrentState.UpdateState();
    }

    public void ChangeState<T>() where T : BaseState
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
        // 피격 판정 당했을시 몬스터에게 피격 상태로 전환됨.
        // 방법1 피격당할시 충돌한 해당 태그의 이름이 무기일경우 피격상태로 전환되게한다.
        // 방법2 피격당할시 무기의 ID정보를 확인해서 무기일경우 피격상태로 전환되게한다.
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
        Vector3 rangeDirection = Quaternion.Euler(0, -detecionAngle / 2, 0) * eyeTransform.forward;
        
        // 채워진 부채꼴을 그려줍니다.
        Handles.DrawSolidArc(
            eyeTransform.position, // 중심점
            eyeTransform.up, // 부채꼴이 그려질 평면의 법선 백터(몬스터의 위 방향)
            rangeDirection, // 부채꼴의 시작 방향
            detecionAngle,      // 부채꼴의 총 각도
            detectionRadius);   // 부채꼴의 반 지름

        Handles.color = Color.yellow;
        Vector3 rightDirection = Quaternion.Euler(0, detecionAngle / 2, 0) * eyeTransform.forward;
        Vector3 leftDirection = rangeDirection; // 시작 방향과 동일
        
        Handles.DrawLine(eyeTransform.position, eyeTransform.position + rightDirection * detectionRadius, 2f);
        Handles.DrawLine(eyeTransform.position, eyeTransform.position + leftDirection * detectionRadius, 2f);
    }
#endif
    
}
