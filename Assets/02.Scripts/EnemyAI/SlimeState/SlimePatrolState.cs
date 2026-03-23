using UnityEngine;
using UnityEngine.AI;

public class SlimePatrolState : SlimeBaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private const float DAMP_TIME = 0.15f; // 부드러운 전이 시간

    private Vector3 startPos;
    private Vector3 targetPos;
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }

    public override void EnterState()
    {
        // [수정] 현재 위치가 아닌 '스폰 위치'를 기준으로 시작점 설정
        startPos = Slime.SpawnPoint != null ? Slime.SpawnPoint.transform.position : Slime.transform.position;
        targetPos = CalculatePatrolDestination();
        
        Agent.speed = Slime.MoveSpeed;
        Agent.SetDestination(targetPos);
        Agent.isStopped = false;
    }

    public override void UpdateState()
    {
        // 댐핑을 이용해 부드럽게 1.0(Walk)으로 가속
        SlimeAnimator.SetFloat(MoveSpeed, 1f, DAMP_TIME, Time.deltaTime);

        if (IsPlayerInSight())
        {
            Slime.ChangeState<SlimeChaseState>();
            return;
        }

        if (IsMonsterInFront())
        {
            Slime.ChangeState<SlimeIdleState>();
            return;
        }

        if (Agent.pathPending == false && Agent.remainingDistance <= Agent.stoppingDistance + 0.1f)
        {
            Slime.ChangeState<SlimeIdleState>();
        }
    }

    public override void ExitState()
    {
        if (Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }
    }
    
    private Vector3 CalculatePatrolDestination()
    {
        Vector3 finalPos = startPos;
        int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector3 direction = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
            float minDistance = Slime.PatrolRadius * 0.3f;
            float moveDistance = Random.Range(minDistance, Slime.PatrolRadius);
            Vector3 candidatePos = startPos + (direction * moveDistance);
            if (Physics.Raycast(startPos + Vector3.up * 0.5f, direction, out RaycastHit hit, moveDistance, Slime.ObstacleLayer))
                candidatePos = hit.point - (direction * 0.5f);
            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit navHit, 5.0f, NavMesh.AllAreas))
                if (Vector3.Distance(startPos, navHit.position) > 2.0f) return navHit.position;
        }
        return startPos;
    }
}