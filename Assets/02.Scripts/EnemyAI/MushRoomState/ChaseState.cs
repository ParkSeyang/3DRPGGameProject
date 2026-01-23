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
        // 적을 쫒는 로직
        Agent.SetDestination(MushRoom.Target.position);

        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            if (Agent.pathPending == false)
            {
                MushRoom.ChangeState<AttackState>();
            }
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
}
