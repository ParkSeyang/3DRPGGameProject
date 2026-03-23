using UnityEngine;
using UnityEngine.AI;
public abstract class SlimeBaseState
{
     public struct StateControllerParameter
    {
        public Slime slime;
        public Animator slimeAnimator;
        public NavMeshAgent agent;
        public AnimEventReceiver animEventReceiver;
        public Collider attackCollider;
    }
    
    private readonly Collider[] targetBuffer = new Collider[1];
    
    protected Slime Slime { get; private set; }
    protected Animator SlimeAnimator { get; private set; }
    protected NavMeshAgent Agent { get; private set; }
    protected AnimEventReceiver AnimEventReceiver { get; private set; }
    protected Collider AttackCollider { get; private set; }

    public virtual void Initialize(StateControllerParameter parameter)
    {
        Slime = parameter.slime;
        SlimeAnimator = parameter.slimeAnimator;
        Agent = parameter.agent;
        AnimEventReceiver = parameter.animEventReceiver;
        AttackCollider = parameter.attackCollider;
    }


    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public bool IsPlayerInSight()
    {
        int detectionCount = Physics.OverlapSphereNonAlloc(Slime.EyeTransform.position, Slime.DetectionRadius,
            targetBuffer, Slime.PlayerLayer);
        
        if (detectionCount == 0) return false;

        Player targetPlayer = targetBuffer[0].GetComponentInParent<Player>();
        Transform potentialTarget = (targetPlayer != null) ? targetPlayer.transform : targetBuffer[0].transform;
        
        Vector3 targetPosition = potentialTarget.position + Vector3.up * 1.0f;
        Vector3 directionToTarget = (targetPosition - Slime.EyeTransform.position).normalized;
        float angleToTarget = Vector3.Angle(Slime.EyeTransform.forward, directionToTarget);

        if (angleToTarget > Slime.DetectionAngle / 2.0f) return false;

        float distanceToTarget = Vector3.Distance(Slime.EyeTransform.position, targetPosition);
        
        if (Physics.Raycast(Slime.EyeTransform.position, directionToTarget, distanceToTarget, Slime.ObstacleLayer))
        {
            return false;
        }

        Slime.SetTarget(potentialTarget);
        return true;
    }

    public bool IsMonsterInFront()
    {
        Vector3 originPosition = Slime.EyeTransform.position;
        Vector3 checkDirection = Slime.transform.forward;
        float checkDistance = 2.0f;

        if (Physics.SphereCast(originPosition, 0.5f, checkDirection, out RaycastHit enemyHit, checkDistance, Slime.EnemyLayer))
        {
            if (enemyHit.collider.gameObject != Slime.gameObject)
            {
                return true;
            }
        }
        return false;
    }
}