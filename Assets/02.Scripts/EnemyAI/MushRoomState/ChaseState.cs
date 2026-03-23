using UnityEngine;
using UnityEngine.AI;

public class ChaseState : BaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private const float DAMP_TIME = 0.1f;

    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }
    
    public override void EnterState()
    {
        if (MushRoom.Target == null || IsPlayerInSight() == false)
        {
            MushRoom.ChangeState<IdleState>();
            return;
        }

        Agent.speed = MushRoom.MoveSpeed * 2.0f;
        Agent.isStopped = false;
    }

    public override void UpdateState()
    {
        MushRoomAnimator.SetFloat(MoveSpeed, 1.5f, DAMP_TIME, Time.deltaTime);

        if (MushRoom.Target == null || IsPlayerInSight() == false)
        {
            MushRoom.ChangeState<IdleState>();
            return;
        }

        if (MushRoom.SpawnPoint != null)
        {
            float distanceFromSpawn = Vector3.Distance(MushRoom.transform.position, MushRoom.SpawnPoint.transform.position);
            if (distanceFromSpawn > MushRoom.SpawnPoint.leashRange)
            {
                MushRoom.ChangeState<ReturnState>();
                return;
            }
        }

        float distance = MushRoom.transform.FlatDistanceTo(MushRoom.Target);
        if (distance <= Agent.stoppingDistance)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
            Agent.ResetPath();
            MushRoom.ChangeState<AttackState>();
            return;
        }

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