using UnityEngine;

public class SlimeDeadState : SlimeBaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Dead = Animator.StringToHash("Dead");
    private float destroyTimer = 0.0f;
    private const float DESTROY_DELAY = 5.0f; 
    private bool isReleased = false;

    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }

    public override void EnterState()
    {
        isReleased = false;

        // 1. 이동 및 에이전트 완전 정지
        SlimeAnimator.SetFloat(MoveSpeed, 0f);

        if (Agent.isOnNavMesh)
        {
            Agent.velocity = Vector3.zero;
            Agent.isStopped = true;
            Agent.ResetPath();
        }
        Agent.enabled = false;

        // 2. 물리 관성 제거
        var monsterRigidbody = Slime.GetComponent<Rigidbody>();
        if (monsterRigidbody != null)
        {
            monsterRigidbody.linearVelocity = Vector3.zero;
            monsterRigidbody.angularVelocity = Vector3.zero;
        }

        // 3. 충돌체 및 공격 판정 처리
        var mainCollider = Slime.GetComponent<Collider>();
        if (mainCollider != null)
        {
            mainCollider.isTrigger = true; 
        }

        AttackCollider.enabled = false;
        SlimeAnimator.SetTrigger(Dead);
        Slime.TriggerOnDeadEvent();

        // 4. 스폰 지점에 사망 알림
        if (Slime.SpawnPoint != null)
        {
            Slime.SpawnPoint.OnMonsterDead(Slime.gameObject);
        }
    }

    public override void UpdateState()
    {
        if (isReleased == true) return;

        destroyTimer += Time.deltaTime;
        if (destroyTimer >= DESTROY_DELAY)
        {
            isReleased = true;
            if (MonsterSpawnManager.IsInitialized == true)
            {
                MonsterSpawnManager.Instance.ReleaseMonster(Slime.EnemyID, Slime.gameObject);
            }
            else
            {
                GameObject.Destroy(Slime.gameObject);
            }
        }
    }

    public override void ExitState() { }
}
