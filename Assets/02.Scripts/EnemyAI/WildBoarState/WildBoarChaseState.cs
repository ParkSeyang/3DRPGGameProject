using UnityEngine;

public class WildBoarChaseState : WildBoarBaseState
{
    private static readonly int Run = Animator.StringToHash("Run");
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }
    
    public override void EnterState()
    {
        // 타겟이 없거나 또는 시야에 플레이어가 안보일 경우
        if (WildBoar.Target == null || IsPlayerInSight() == false)
        {
            WildBoar.ChangeState<WildBoarIdleState>();
            return;
        }

        Agent.speed = WildBoar.MoveSpeed * 2.0f;
        Agent.isStopped = false;
        WildBoarAnimator.SetTrigger(Run);
    }

    public override void UpdateState()
    {
        if (WildBoar.Target == null || IsPlayerInSight() == false)
        {
            WildBoar.ChangeState<WildBoarIdleState>();
            return;
        }
        // 적을 쫒는 로직
        Agent.SetDestination(WildBoar.Target.position);

        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            if (Agent.pathPending == false)
            {
                WildBoar.ChangeState<WildBoarAttackState>();
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
