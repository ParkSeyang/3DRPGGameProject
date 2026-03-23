using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class WildBoar : MonoBehaviour, ICombatAgent, IPoolAbleObject
{
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Dead = Animator.StringToHash("Dead");
    
    [Header("Data Settings")]
    [SerializeField] private int enemyID = 3; 
    
    // IPoolAbleObject 인터페이스 구현
    public int EnemyID => enemyID;

    public void OnGet()
    {
        isDead = false;
        InitializeStat();
        
        if (Agent != null) Agent.enabled = false;
        var mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.isTrigger = false;

        // [추가] 공격 판정 및 애니메이터 초기화
        if (AttackCollider != null) AttackCollider.enabled = false;
        if (Animator != null)
        {
            Animator.Rebind();
            Animator.Update(0f);
        }

        if (statUI != null)
        {
            statUI.Initialize(EnemyName, Level, CurrentHP, MaxHP);
        }

        ChangeState<WildBoarIdleState>();
    }

    public void OnRelease() { }
    public string EnemyName { get; private set; } // [추가] 이름
    public int Level { get; private set; }       // [추가] 레벨
    
    public float MoveSpeed { get; set; } = 10.0f;
    public int Exp { get; private set; }
    
    public int DropGold { get; private set; } = 100;
    public float MaxHP { get; private set; }
    public float CurrentHP { get; private set; }
    public float ATK { get; private set; }
    public float DEF { get; private set; } // [추가] 방어력
    
    private bool isDead = false;
    public event Action OnDead;
    public void TriggerOnDeadEvent()
    {
        if (isDead) return;
        isDead = true;
        OnDead?.Invoke();
    }

    [Header("AI 설정")] 
    [SerializeField] private float patrolRadius = 15.0f;
    
    [Header("몬스터의 탐지 범위 설정")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform eyeTransform;
    [SerializeField] private float detectionRadius = 10.0f;
    [SerializeField, Range(0, 360)] 
    private float detectionAngle = 90.0f;
    [SerializeField] private LayerMask playerLayer; 
    [SerializeField] private LayerMask obstacleLayer; 
    [SerializeField] private LayerMask enemyLayer; 
    
    
    [Header("애니메이션 및 Collider")]
    [SerializeField] private Collider AttackCollider;
    [SerializeField] private AnimEventReceiver AnimEventReceiver;
    [SerializeField] private MonsterStatUI statUI;

    public Transform Target => target;
    public Transform EyeTransform => eyeTransform;
    public float PatrolRadius => patrolRadius;
    public float DetectionRadius => detectionRadius;
    public float DetectionAngle => detectionAngle;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask ObstacleLayer => obstacleLayer;
    public LayerMask EnemyLayer => enemyLayer;

    private Animator Animator { get; set; }
    private NavMeshAgent Agent { get; set; }
    private Dictionary<Type, WildBoarBaseState> States { get; set; }
    public WildBoarBaseState CurrentState { get; set; }
    private WildBoarBaseState DefaultState { get; set; }

    public MonsterSpawnPoint SpawnPoint { get; private set; }
    public void SetSpawnPoint(MonsterSpawnPoint point) => SpawnPoint = point;

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

        // [수정] 초기 스탯 UI 데이터 주입
        if (statUI != null)
        {
            statUI.Initialize(EnemyName, Level, CurrentHP, MaxHP);
        }

        States = new Dictionary<Type, WildBoarBaseState>();
        States.Add(typeof(WildBoarIdleState), new WildBoarIdleState());
        States.Add(typeof(WildBoarPatrolState), new WildBoarPatrolState());
        States.Add(typeof(WildBoarChaseState), new WildBoarChaseState());
        States.Add(typeof(WildBoarAttackState), new WildBoarAttackState());
        States.Add(typeof(WildBoarHitState), new WildBoarHitState());
        States.Add(typeof(WildBoarDeadState), new WildBoarDeadState());
        States.Add(typeof(WildBoarReturnState), new WildBoarReturnState()); 
        
        DefaultState = States[typeof(WildBoarIdleState)];

        var parameter = new WildBoarBaseState.StateControllerParameter
        {
            wildboar = this,
            attackCollider = AttackCollider,
            wildboarAnimator = Animator,
            animEventReceiver = AnimEventReceiver,
            agent = Agent,
        };
        
        foreach (var state in States.Values)
        {
            state.Initialize(parameter);
        }
    }

    private void InitializeCombat()
    {
        if (AttackCollider != null)
        {
            var hitBox = AttackCollider.GetComponent<HitBox>();
            if (hitBox != null)
            {
                hitBox.Initialize(this, playerLayer);
            }
        }

        var hurtBoxes = GetComponentsInChildren<HurtBox>();
        foreach (var hurtBox in hurtBoxes)
        {
            hurtBox.Initialize(this);
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
            }

            if (QuestManager.IsInitialized)
            {
                QuestManager.Instance.UpdateKillQuest("WildBoar");
            }
        };
        
        ChangeState<WildBoarIdleState>();
    }

    private void InitializeStat()
    {
        if (EnemyDataManager.Instance == null) return;

        var enemyStat = EnemyDataManager.Instance.GetEnemyStat(enemyID);
        if (enemyStat != null)
        {
            EnemyName = enemyStat.Name; // [추가] 이름 연동
            Level = enemyStat.Level;     // [추가] 레벨 연동
            Exp = enemyStat.Exp;
            MoveSpeed = enemyStat.MoveSpeed;
            MaxHP = enemyStat.HP;
            CurrentHP = enemyStat.HP;
            ATK = enemyStat.ATK;
            DEF = enemyStat.DEF; // [추가] 방어력 주입
        }
    }
    
    private void Update()
    {
        CurrentState.UpdateState();
    }

    public void ChangeState<T>() where T : WildBoarBaseState
    {
        var prevState = CurrentState;
        prevState?.ExitState();

        CurrentState = DefaultState;
        if (States.ContainsKey(typeof(T)))
        {
            CurrentState = States[typeof(T)];
        }
        
        CurrentState.EnterState();
    }

    public void TakeDamage(float damage, HitInfo hitInfo)
    {
        if (isDead == true) return; // [추가] 이미 사망한 경우 피해 무시

        // [수정] 방어력 적용 및 최소 데미지 1 보정
        float finalDamage = Mathf.Max(1f, damage - DEF);
        CurrentHP -= finalDamage;
        
        // [수정] 피격 시 즉시 반응 강화
        if (Player.Instance != null)
        {
            SetTarget(Player.Instance.transform);
            transform.rotation = Quaternion.LookRotation(transform.FlatDirectionTo(Player.Instance.transform));
        }

        if (Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
            Agent.ResetPath();
        }

        // 체력바 UI 갱신
        if (statUI != null)
        {
            statUI.UpdateHPBar(CurrentHP, MaxHP);
        }

        if (CurrentHP <= 0)
        {
            ChangeState<WildBoarDeadState>();
        }
        else
        {
            // [핵심] Idle이나 Patrol 중이었다면 즉시 HitState로 전환하여 반응함
            ChangeState<WildBoarHitState>();
        }
    }

    public void OnHitDetected(HitInfo hitInfo)
    {
        CombatEvent combatEvent = new CombatEvent();
        combatEvent.Sender = this;
        combatEvent.Receiver = hitInfo.receiver;
        combatEvent.Damage = ATK;
        combatEvent.HitInfo = hitInfo;
        
        CombatSystem.Instance.AddCombatEvent(combatEvent);
    }
    
    
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (eyeTransform == null) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(eyeTransform.position, detectionRadius);
        Handles.color = new Color(1f,1f,0f, 0.2f);
        Vector3 rangeDirection = Quaternion.Euler(0, -detectionAngle / 2, 0) * eyeTransform.forward;
        Handles.DrawSolidArc(eyeTransform.position, eyeTransform.up, rangeDirection, detectionAngle, detectionRadius);
        Handles.color = Color.yellow;
        Vector3 leftDirection = rangeDirection; 
        Vector3 rightDirection = Quaternion.Euler(0, detectionAngle / 2, 0) * eyeTransform.forward;
        Handles.DrawLine(eyeTransform.position, eyeTransform.position + rightDirection * detectionRadius, 2f);
        Handles.DrawLine(eyeTransform.position, eyeTransform.position + leftDirection * detectionRadius, 2f);
    }
#endif
}