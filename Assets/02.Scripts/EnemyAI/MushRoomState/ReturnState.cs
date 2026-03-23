using UnityEngine;

public class ReturnState : BaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private float returnStopDistance = 0.5f;
    private const float DAMP_TIME = 0.15f;

    public override void EnterState()
    {
        MushRoom.SetTarget(null);
        Agent.isStopped = false;
        Agent.speed = MushRoom.MoveSpeed * 1.5f;
        Agent.SetDestination(MushRoom.SpawnPoint.transform.position);
    }

    public override void UpdateState()
    {
        MushRoomAnimator.SetFloat(MoveSpeed, 1f, DAMP_TIME, Time.deltaTime);

        if (MushRoom.SpawnPoint != null)
        {
            float distance = Vector3.Distance(MushRoom.transform.position, MushRoom.SpawnPoint.transform.position);
            if (distance <= returnStopDistance)
            {
                MushRoom.ChangeState<IdleState>();
            }
        }
        else
        {
            MushRoom.ChangeState<IdleState>();
        }
    }

    public override void ExitState()
    {
        Agent.speed = MushRoom.MoveSpeed;
    }
}