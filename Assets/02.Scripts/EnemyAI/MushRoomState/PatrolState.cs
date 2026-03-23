using UnityEngine;
using UnityEngine.AI;

public class PatrolState : BaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private const float DAMP_TIME = 0.15f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }

    public override void EnterState()
    {
        // [수정] 현재 위치가 아닌 '스폰 위치'를 기준으로 시작점 설정
        startPosition = MushRoom.SpawnPoint != null ? MushRoom.SpawnPoint.transform.position : MushRoom.transform.position;
        targetPosition = CalculatePatrolDestination();
        
        Agent.speed = MushRoom.MoveSpeed;
        Agent.SetDestination(targetPosition);
        Agent.isStopped = false;
    }

    public override void UpdateState()
    {
        MushRoomAnimator.SetFloat(MoveSpeed, 1f, DAMP_TIME, Time.deltaTime);

        if (IsPlayerInSight())
        {
            MushRoom.ChangeState<ChaseState>();
            return;
        }

        if (IsMonsterInFront())
        {
            MushRoom.ChangeState<IdleState>();
            return;
        }

        if (Agent.pathPending == false && Agent.remainingDistance <= Agent.stoppingDistance + 0.1f)
        {
            MushRoom.ChangeState<IdleState>();
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
        Vector3 finalPosition = startPosition;
        int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
            float minPatrolDistance = MushRoom.PatrolRadius * 0.3f;
            float moveDistance = Random.Range(minPatrolDistance, MushRoom.PatrolRadius);
            Vector3 candidatePosition = startPosition + (randomDirection * moveDistance);
            
            if (Physics.Raycast(startPosition + Vector3.up * 0.5f, randomDirection, out RaycastHit hitInfo, moveDistance, MushRoom.ObstacleLayer))
                candidatePosition = hitInfo.point - (randomDirection * 0.5f);
            
            if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit navMeshHit, 5.0f, NavMesh.AllAreas))
                if (Vector3.Distance(startPosition, navMeshHit.position) > 2.0f) return navMeshHit.position;
        }
        return startPosition;
    }
}
