using UnityEngine;

public class WildBoarReturnState : WildBoarBaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private float returnStopDistance = 0.5f;
    private const float DAMP_TIME = 0.15f;

    public override void EnterState()
    {
        WildBoar.SetTarget(null);
        Agent.isStopped = false;
        Agent.speed = WildBoar.MoveSpeed * 1.5f;
        Agent.SetDestination(WildBoar.SpawnPoint.transform.position);
    }

    public override void UpdateState()
    {
        WildBoarAnimator.SetFloat(MoveSpeed, 1f, DAMP_TIME, Time.deltaTime);

        if (WildBoar.SpawnPoint != null)
        {
            float distance = Vector3.Distance(WildBoar.transform.position, WildBoar.SpawnPoint.transform.position);
            if (distance <= returnStopDistance)
            {
                WildBoar.ChangeState<WildBoarIdleState>();
            }
        }
        else
        {
            WildBoar.ChangeState<WildBoarIdleState>();
        }
    }

    public override void ExitState()
    {
        Agent.speed = WildBoar.MoveSpeed;
    }
}