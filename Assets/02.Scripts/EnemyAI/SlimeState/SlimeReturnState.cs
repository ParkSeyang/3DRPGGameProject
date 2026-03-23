using UnityEngine;

public class SlimeReturnState : SlimeBaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private float returnStopDistance = 0.5f;
    private const float DAMP_TIME = 0.15f;

    public override void EnterState()
    {
        Slime.SetTarget(null);
        Agent.isStopped = false;
        Agent.speed = Slime.MoveSpeed * 1.5f;
        Agent.SetDestination(Slime.SpawnPoint.transform.position);
    }

    public override void UpdateState()
    {
        // 댐핑 복귀
        SlimeAnimator.SetFloat(MoveSpeed, 1f, DAMP_TIME, Time.deltaTime);

        if (Slime.SpawnPoint != null)
        {
            float distance = Vector3.Distance(Slime.transform.position, Slime.SpawnPoint.transform.position);
            if (distance <= returnStopDistance)
            {
                Slime.ChangeState<SlimeIdleState>();
            }
        }
        else
        {
            Slime.ChangeState<SlimeIdleState>();
        }
    }

    public override void ExitState()
    {
        Agent.speed = Slime.MoveSpeed;
    }
}