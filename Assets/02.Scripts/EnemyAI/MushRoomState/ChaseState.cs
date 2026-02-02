using UnityEngine;
using UnityEngine.AI;

public class ChaseState : BaseState
{
    private static readonly int Walk = Animator.StringToHash("Walk");
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }
    
    public override void EnterState()
    {
        // 타겟이 없거나 또는 시야에 플레이어가 안보일 경우
        if (MushRoom.Target == null || IsPlayerInSight() == false)
        {
            MushRoom.ChangeState<IdleState>();
            return;
        }

        Agent.speed = MushRoom.MoveSpeed * 2.0f;
        Agent.isStopped = false;
        MushRoomAnimator.SetTrigger(Walk);
    }

    public override void UpdateState()
    {
        if (MushRoom.Target == null || IsPlayerInSight() == false)
        {
            MushRoom.ChangeState<IdleState>();
            return;
        }

        // 거리 체크를 이동 명령보다 먼저 수행
        float distance = MushRoom.transform.FlatDistanceTo(MushRoom.Target);

        if (distance <= Agent.stoppingDistance + 0.2f)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
            Agent.ResetPath(); // 경로 초기화로 완전 정지 보장
            
            MushRoom.ChangeState<AttackState>();
            return;
        }

        // 적을 쫒는 로직
        Agent.SetDestination(MushRoom.Target.position);
    }

    public override void ExitState()
    {
        if (Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }
    }
}
