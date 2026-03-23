using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Mushroom : MonoBehaviour, ICombatAgent, IPoolAbleObject
{
    [Header("Data Settings")]
    [SerializeField] private int enemyID = 2; // TSV 파일의 ID와 일치해야 함
    
    // IPoolAbleObject 인터페이스 구현
    public int EnemyID => enemyID; 
    
    // [추가] 오브젝트 풀링 인터페이스 구현
    public void OnGet()
    {
        isDead = false;
        InitializeStat(); // 스탯(체력 등) 다시 로드
        
        // 에이전트 및 콜라이더 일시 비활성화 (위치 동기화 전 에러 방지)
        if (Agent != null) Agent.enabled = false;
        
        var mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.isTrigger = false;

        // [추가] 공격 판정 및 애니메이터 초기화 (풀링 안전장치)
        if (AttackCollider != null) AttackCollider.enabled = false;
        if (Animator != null)
        {
            Animator.Rebind(); // 애니메이터 상태 및 파라미터 완전 초기화
            Animator.Update(0f);
        }

        // UI 초기화
        if (statUI != null)
        {
            statUI.Initialize(EnemyName, Level, CurrentHP, MaxHP);
        }

        ChangeState<IdleState>();
    }

    public void OnRelease()
    {
        // 반환 전 필요한 정리 작업 (현재는 특별히 없음)
    }
    public string EnemyName { get; private set; } // [추가] 이름
    public int Level { get; private set; }       // [추가] 레벨
    public int Exp { get; private set; }
    public int DropGold { get; private set; } = 50;
    
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
    
    public float MoveSpeed { get; set; } = 5.0f;

    [Header("AI 설정")] 
    [SerializeField] private float patrolRadius = 12.0f;
    
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
    private Dictionary<Type, BaseState> States { get; set; }
    public BaseState CurrentState { get; set; }
    private BaseState DefaultState { get; set; }

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
        
        States = new Dictionary<Type, BaseState>();
        States.Add(typeof(IdleState), new IdleState());
        States.Add(typeof(PatrolState), new PatrolState());
        States.Add(typeof(ChaseState), new ChaseState());
        States.Add(typeof(AttackState), new AttackState());
        States.Add(typeof(HitState), new HitState());
        States.Add(typeof(DeadState), new DeadState());
        States.Add(typeof(ReturnState), new ReturnState()); 
        
        DefaultState = States[typeof(IdleState)];

        var parameter = new BaseState.StateControllerParameter
        {
            mushroom = this,
            attackCollider = AttackCollider,
            mushroomAnimator = Animator,
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
                QuestManager.Instance.UpdateKillQuest("Mushroom");
            }
        };

        ChangeState<IdleState>();
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

        // [추가] Root Motion 동기화: 애니메이션이 이동시킨 위치로 Agent를 강제 이동시켜 '순간이동' 현상 방지
        if (Animator.applyRootMotion && Agent.isOnNavMesh)
        {
            Agent.nextPosition = transform.position;
        }
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
    }

    public void TakeDamage(float damage, HitInfo hitInfo)
    {
        if (isDead == true) return; // [추가] 이미 죽은 몬스터는 데미지를 입지 않음

        // [수정] 방어력 적용 및 최소 데미지 1 보정
        float finalDamage = Mathf.Max(1f, damage - DEF);
        CurrentHP -= finalDamage;
        
        // [수정] 피격 시 즉시 반응 강화
        if (Player.Instance != null)
        {
            SetTarget(Player.Instance.transform);
            
            // 즉시 플레이어 방향으로 고개 돌리기
            transform.rotation = Quaternion.LookRotation(transform.FlatDirectionTo(Player.Instance.transform));
        }

        // 에이전트 즉시 정지 (이전 경로 무효화)
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
            ChangeState<DeadState>();
        }
        else
        {
            ChangeState<HitState>();
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