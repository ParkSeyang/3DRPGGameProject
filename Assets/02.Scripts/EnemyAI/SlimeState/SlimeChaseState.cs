using UnityEngine;

public class SlimeChaseState : SlimeBaseState
{
    private static readonly int Walk = Animator.StringToHash("Walk");
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }
    
    public override void EnterState()
    {
        // 타겟이 없거나 또는 시야에 플레이어가 안보일 경우
        if (Slime.Target == null || IsPlayerInSight() == false)
        {
            Slime.ChangeState<SlimeIdleState>();
            return;
        }

        Agent.speed = Slime.MoveSpeed * 2.0f;
        Agent.isStopped = false;
        SlimeAnimator.SetTrigger(Walk);
    }

    public override void UpdateState()
    {
        if (Slime.Target == null || IsPlayerInSight() == false)
        {
            Slime.ChangeState<SlimeIdleState>();
            return;
        }
        // 적을 쫒는 로직
        Agent.SetDestination(Slime.Target.position);

        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            if (Agent.pathPending == false)
            {
                Slime.ChangeState<SlimeAttackState>();
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
