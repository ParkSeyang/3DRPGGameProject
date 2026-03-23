using UnityEngine;

public class DeadState : BaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Dead = Animator.StringToHash("Dead");
    private float destroyTimer = 0.0f;
    private const float DESTROY_DELAY = 5.0f; 
    private bool isReleased = false; // [추가] 중복 반환 방지 플래그
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }

    public override void EnterState()
    {
        isReleased = false; // 상태 진입 시 초기화
        // ... (이전 코드 동일)
        // 1. 이동 애니메이션 및 에이전트 완전 정지
        MushRoomAnimator.SetFloat(MoveSpeed, 0f);

        if (Agent.isOnNavMesh)
        {
            Agent.velocity = Vector3.zero; // 물리적 속도 즉시 제거
            Agent.isStopped = true;
            Agent.ResetPath();
        }
        Agent.enabled = false; // 내비메시 연산에서 제외

        // 2. 리지드바디 물리 초기화 (존재할 경우)
        var monsterRigidbody = MushRoom.GetComponent<Rigidbody>();
        if (monsterRigidbody != null)
        {
            // 속도와 회전력을 즉시 제거하여 제자리에 멈추게 함
            monsterRigidbody.linearVelocity = Vector3.zero;
            monsterRigidbody.angularVelocity = Vector3.zero;
        }

        // 3. 충돌체 및 공격 판정 유지 (바닥 뚫기 방지)
        var mainCollider = MushRoom.GetComponent<Collider>();
        if (mainCollider != null)
        {
            // [수정] 충돌체를 끄는 대신 트리거로 변경하여 바닥은 유지하되 플레이어는 통과하게 함
            mainCollider.isTrigger = true; 
        }

        AttackCollider.enabled = false;
        MushRoomAnimator.SetTrigger(Dead);
        MushRoom.TriggerOnDeadEvent();

        // 4. 스폰 지점에 사망 알림
        if (MushRoom.SpawnPoint != null)
        {
            MushRoom.SpawnPoint.OnMonsterDead(MushRoom.gameObject);
        }
    }

    public override void UpdateState()
    {
        if (isReleased == true) return; // 이미 반환 처리 중이면 중단

        destroyTimer += Time.deltaTime;
        if (destroyTimer >= DESTROY_DELAY)
        {
            isReleased = true; // 반환 시작됨을 표시

            if (MonsterSpawnManager.IsInitialized == true)
            {
                MonsterSpawnManager.Instance.ReleaseMonster(MushRoom.EnemyID, MushRoom.gameObject);
            }
            else
            {
                GameObject.Destroy(MushRoom.gameObject);
            }
        }
    }

    public override void ExitState() { }
}