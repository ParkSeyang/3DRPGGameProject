using UnityEngine;
using UnityEngine.AI;

public class PatrolState : BaseState
{
    private static readonly int Walk = Animator.StringToHash("Walk");

    private Transform target;
    private AnimEventReceiver receiver;
    private LayerMask playerLayer;
    private LayerMask obstacleLayer;
    private NavMeshAgent agent;
    private Transform[] PatrolPoints;
    private Transform StartPoint;
    
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
        playerLayer = parameter.playerLayer;
        obstacleLayer = parameter.obstacleLayer;
        receiver = parameter.AnimEventReceiver;
        target = parameter.target;
        agent = parameter.agent;
    }

    public override void EnterState()
    {
        StartPoint.position = agent.transform.position;
        PatrolPoints = new Transform[agent.transform.childCount];
        MushRoomAnimator.SetTrigger(Walk);
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }

    public void DetectPlayer()
    {
        
    }

}
