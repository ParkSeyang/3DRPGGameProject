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

        WildBoarAnimator.SetTrigger(Run);
        Agent.speed = WildBoar.MoveSpeed * 2.0f;
        Agent.isStopped = false;
    }

    public override void UpdateState()
    {
        if (WildBoar.Target == null || IsPlayerInSight() == false)
        {
            WildBoar.ChangeState<WildBoarIdleState>();
            return;
        }

        // 거리 체크를 이동 명령보다 먼저 수행
        float distance = WildBoar.transform.FlatDistanceTo(WildBoar.Target);
        
        // 정지 거리보다 약간 더 여유있게 체크 (관성 고려)
        if (distance <= Agent.stoppingDistance + 0.5f)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
            Agent.ResetPath(); // 경로 초기화로 완전 정지 보장
            
            // 공격 상태로 전환
            WildBoar.ChangeState<WildBoarAttackState>();
            return;
        }

        // 아직 거리가 멀면 이동
        Agent.SetDestination(WildBoar.Target.position);
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
