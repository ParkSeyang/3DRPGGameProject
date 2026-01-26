using UnityEngine;
using UnityEngine.AI;
public class SlimePatrolState : SlimeBaseState
{
    private static readonly int Walk = Animator.StringToHash("Walk");

    // 정찰시 시작 위치
    private Vector3 startPos;
    // 정찰시 도착해야될 위치
    private Vector3 targetPos;
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }

    public override void EnterState()
    {
        startPos = Slime.transform.position;
        targetPos = CalculatePatrolDestination();
        
        Agent.speed = Slime.MoveSpeed;
        Agent.SetDestination(targetPos);
        Agent.isStopped = false;
        
        SlimeAnimator.SetTrigger(Walk);
    }

    public override void UpdateState()
    {
        if (IsPlayerInSight())
        {
            Slime.ChangeState<SlimeChaseState>();
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
    
    
    // 8방향 체크 및 장애물 회피 이동 위치 계산
    private Vector3 CalculatePatrolDestination()
    {
        int randomDirIndex = Random.Range(0, 8);
        float angle = randomDirIndex * 45f;
        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        float moveDistance = Slime.PatrolRadius;

        Vector3 origin = startPos + Vector3.up * 0.5f;
        
        // 장애물을 체크하는 로직 : 장애물이 있으면 그앞 까지만 이동하도록 해줌
        if (Physics.Raycast(origin, direction, out RaycastHit hit, moveDistance, Slime.ObstacleLayer))
        {
            float safeDist = Mathf.Max(0, hit.distance - Agent.radius);
            return startPos + (direction * safeDist);
        }

        Vector3 finalPos = startPos + (direction * moveDistance);
        
        // NavMesh 위인지 확인
        if (NavMesh.SamplePosition(finalPos, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        return startPos;

    }
}
