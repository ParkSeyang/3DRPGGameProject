using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Mushroom : MonoBehaviour, ICombatAgent
{
    [Header("Data Settings")]
    [SerializeField] private int enemyID = 2; // TSV 파일의 ID와 일치해야 함
    public int Exp { get; private set; }
    public int DropGold { get; private set; } = 50;
    
    public float MaxHP { get; private set; }
    public float CurrentHP { get; private set; }
        public float ATK { get; private set; }
        
        private bool isDead = false;
        public event Action OnDead;
    
        public void TriggerOnDeadEvent()
        {
            if (isDead) return;
            isDead = true;
            OnDead?.Invoke();
        }
    
        public float MoveSpeed { get; set; } = 5.0f;

    [Header("AI 설정")] 
    [SerializeField] private float patrolRadius = 12.0f;
    
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
    private Dictionary<Type, BaseState> States { get; set; }
    public BaseState CurrentState { get; set; }
    private BaseState DefaultState { get; set; }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        AttackCollider.enabled = false;

        InitializeStat();
        InitializeCombat();
        
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
            animEventReceiver = AnimEventReceiver,
            agent = Agent,
   
        };
        
        foreach (var state in States.Values)
        {
            state.Initialize(param);
        }
    }

    private void InitializeCombat()
    {
        // HitBox 초기화
        if (AttackCollider != null)
        {
            var hitBox = AttackCollider.GetComponent<HitBox>();
            if (hitBox != null)
            {
                hitBox.Initialize(this);
            }
        }

        // HurtBox 초기화 (자식이나 본인에게 있는 모든 HurtBox)
        var hurtBoxes = GetComponentsInChildren<HurtBox>();
        foreach (var hb in hurtBoxes)
        {
            hb.Initialize(this);
        }
    }

    private void Start()
    {
        OnDead += () =>
        {
            if (PlayerStatusController.Instance != null)
            {
                PlayerStatusController.Instance.AddExp(Exp);
                PlayerStatusController.Instance.AddGold(DropGold);
                Debug.Log($"[Mushroom] 처치 보상 지급: EXP {Exp}, Gold {DropGold}");
            }
        };

        ChangeState<IdleState>();
    }

    private void InitializeStat()
    {
        if (EnemyDataManager.Instance == null)
        {
            Debug.LogError("[Mushroom] EnemyDataManager가 없습니다.");
            return;
        }

        var stat = EnemyDataManager.Instance.GetEnemyStat(enemyID);
        if (stat != null)
        {
            Exp = stat.Exp;
            MoveSpeed = stat.MoveSpeed;
            MaxHP = stat.HP;
            CurrentHP = stat.HP;
            ATK = stat.ATK;
            
            Debug.Log($"<color=yellow>[Mushroom Data]</color> {stat.Name} (ID:{stat.ID}) 로드 완료\n" +
                      $"HP: {stat.HP}, ATK: {stat.ATK}, DEF: {stat.DEF}, Exp: {stat.Exp}, Speed: {stat.MoveSpeed}");
        }
        else
        {
            Debug.LogError($"[Mushroom] ID {enemyID}에 해당하는 데이터를 찾지 못했습니다.");
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

    // --- ICombatAgent Implementation ---

    public void TakeDamage(float damage, HitInfo hitInfo)
    {
        CurrentHP -= damage;
        CurrentHitCount++; // 기존 로직 유지 (필요시 제거 가능)
        
        Debug.Log($"[Mushroom] 피격! 데미지 : {damage}, 남은 HP: {CurrentHP}");

        if (CurrentHP <= 0)
        {
            ChangeState<DeadState>();
        }
        else
        {
            // 슈퍼아머 체크 로직이 있다면 여기서 분기 처리
            ChangeState<HitState>();
        }
    }

    public void OnHitDetected(HitInfo hitInfo)
    {
        // 내가 때린 대상에게 데미지를 준다
        CombatEvent combatEvent = new CombatEvent();
        combatEvent.Sender = this;
        combatEvent.Receiver = hitInfo.receiver;
        combatEvent.Damage = ATK; // ATK를 기반으로 데미지 설정
        combatEvent.HitInfo = hitInfo;
        
        CombatSystem.Instance.AddCombatEvent(combatEvent);
        
        Debug.Log($"[Mushroom] 공격 적중! 대상: {hitInfo.receiver}");
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
