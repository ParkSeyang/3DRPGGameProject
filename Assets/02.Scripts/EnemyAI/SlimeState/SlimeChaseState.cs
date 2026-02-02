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

        // 거리 체크를 이동 명령보다 먼저 수행
        float distance = Slime.transform.FlatDistanceTo(Slime.Target);

        if (distance <= Agent.stoppingDistance + 0.2f)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
            Agent.ResetPath();
            
            Slime.ChangeState<SlimeAttackState>();
            return;
        }

        // 적을 쫒는 로직
        Agent.SetDestination(Slime.Target.position);
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
