using UnityEngine;
using UnityEngine.AI;

public abstract class WildBoarBaseState
{
      public struct StateControllerParameter 
      { 
        public WildBoar wildboar;
        public Animator wildboarAnimator;
        public NavMeshAgent agent;
        public AnimEventReceiver animEventReceiver;
        public Collider attackCollider; 
      }
    
    private readonly Collider[] targetBuffer = new Collider[1];

    protected WildBoar WildBoar { get; private set; }
    protected Animator WildBoarAnimator { get; private set; }
    protected NavMeshAgent Agent { get; private set; }
    protected AnimEventReceiver AnimEventReceiver { get; private set; }
    protected Collider AttackCollider { get; private set; }

    public virtual void Initialize(StateControllerParameter parameter)
    {
        WildBoar = parameter.wildboar;
        WildBoarAnimator = parameter.wildboarAnimator;
        Agent = parameter.agent;
        AnimEventReceiver = parameter.animEventReceiver;
        AttackCollider = parameter.attackCollider;
    }


    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public bool IsPlayerInSight()
    {
        int detectionCount = Physics.OverlapSphereNonAlloc(WildBoar.EyeTransform.position, WildBoar.DetectionRadius,
            targetBuffer, WildBoar.PlayerLayer);
        
        if (detectionCount == 0) return false;

        Player targetPlayer = targetBuffer[0].GetComponentInParent<Player>();
        Transform potentialTarget = (targetPlayer != null) ? targetPlayer.transform : targetBuffer[0].transform;
        
        Vector3 targetPosition = potentialTarget.position + Vector3.up * 1.0f;
        Vector3 directionToTarget = (targetPosition - WildBoar.EyeTransform.position).normalized;
        float angleToTarget = Vector3.Angle(WildBoar.EyeTransform.forward, directionToTarget);

        if (angleToTarget > WildBoar.DetectionAngle / 2.0f) return false;

        float distanceToTarget = Vector3.Distance(WildBoar.EyeTransform.position, targetPosition);
        
        if (Physics.Raycast(WildBoar.EyeTransform.position, directionToTarget, distanceToTarget, WildBoar.ObstacleLayer))
        {
            return false;
        }

        WildBoar.SetTarget(potentialTarget);
        return true;
    }

    public bool IsMonsterInFront()
    {
        Vector3 originPosition = WildBoar.EyeTransform.position;
        Vector3 checkDirection = WildBoar.transform.forward;
        float checkDistance = 3.0f; // 멧돼지는 빠르므로 3미터 전방 확인

        if (Physics.SphereCast(originPosition, 0.5f, checkDirection, out RaycastHit enemyHit, checkDistance, WildBoar.EnemyLayer))
        {
            if (enemyHit.collider.gameObject != WildBoar.gameObject)
            {
                return true;
            }
        }
        return false;
    }
}