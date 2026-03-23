using UnityEngine;

public class SlimeChaseState : SlimeBaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private const float TARGET_CHASE_ANIM_SPEED = 1.5f;
    private const float DAMP_TIME = 0.1f;

    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }
    
    public override void EnterState()
    {
        if (Slime.Target == null || IsPlayerInSight() == false)
        {
            Slime.ChangeState<SlimeIdleState>();
            return;
        }

        Agent.speed = Slime.MoveSpeed * 2.0f;
        Agent.isStopped = false;
    }

    public override void UpdateState()
    {
        // 댐핑을 이용해 부드럽게 추격 속도로 가속
        SlimeAnimator.SetFloat(MoveSpeed, TARGET_CHASE_ANIM_SPEED, DAMP_TIME, Time.deltaTime);

        if (Slime.Target == null || IsPlayerInSight() == false)
        {
            Slime.ChangeState<SlimeIdleState>();
            return;
        }

        if (Slime.SpawnPoint != null)
        {
            float distanceFromSpawn = Vector3.Distance(Slime.transform.position, Slime.SpawnPoint.transform.position);
            if (distanceFromSpawn > Slime.SpawnPoint.leashRange)
            {
                Slime.ChangeState<SlimeReturnState>();
                return;
            }
        }

        float distance = Slime.transform.FlatDistanceTo(Slime.Target);
        if (distance <= Agent.stoppingDistance + 0.2f)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
            Agent.ResetPath();
            Slime.ChangeState<SlimeAttackState>();
            return;
        }

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