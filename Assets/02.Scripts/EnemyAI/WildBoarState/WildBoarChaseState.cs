using UnityEngine;

public class WildBoarChaseState : WildBoarBaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private const float DAMP_TIME = 0.1f;
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }
    
    public override void EnterState()
    {
        if (WildBoar.Target == null || IsPlayerInSight() == false)
        {
            WildBoar.ChangeState<WildBoarIdleState>();
            return;
        }

        Agent.speed = WildBoar.MoveSpeed * 2.0f;
        Agent.isStopped = false;
    }

    public override void UpdateState()
    {
        WildBoarAnimator.SetFloat(MoveSpeed, 2f, DAMP_TIME, Time.deltaTime);

        if (WildBoar.Target == null || IsPlayerInSight() == false)
        {
            WildBoar.ChangeState<WildBoarIdleState>();
            return;
        }

        if (WildBoar.SpawnPoint != null)
        {
            float distanceFromSpawn = Vector3.Distance(WildBoar.transform.position, WildBoar.SpawnPoint.transform.position);
            if (distanceFromSpawn > WildBoar.SpawnPoint.leashRange)
            {
                WildBoar.ChangeState<WildBoarReturnState>();
                return;
            }
        }

        float distance = WildBoar.transform.FlatDistanceTo(WildBoar.Target);
        if (distance <= Agent.stoppingDistance + 0.5f)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
            Agent.ResetPath();
            WildBoar.ChangeState<WildBoarAttackState>();
            return;
        }

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