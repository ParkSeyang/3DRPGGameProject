using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AI;

public abstract class BaseState
{
    public struct StateControllerParameter
    {
        public Mushroom mushroom;
        public Collider attackCollider;
        public Animator mushroomAnimator;
        public AnimEventReceiver AnimEventReceiver;
        public NavMeshAgent agent;
        public Transform target;
        public Transform eyeTransform;
        public LayerMask playerLayer;
        public LayerMask obstacleLayer;
        public float detectionRadius;
        public float detectionAngle;
    }
    
    protected Mushroom MushRoom { get; private set; }
    protected Animator MushRoomAnimator { get; private set; }
    protected NavMeshAgent Agent { get; private set; }
    protected Transform Target { get; private set; }
    protected Transform EyeTransform { get; private set; }
    protected LayerMask PlayerLayer { get; private set; }
    protected LayerMask ObstacleLayer { get; private set; }
    
    protected float DetectionRadius { get; private set; }
    
    protected float DetectionAngle { get; private set; }
    
    public virtual void Initialize(StateControllerParameter parameter)
    {
        MushRoom = parameter.mushroom;
        MushRoomAnimator = parameter.mushroomAnimator;
        Agent = parameter.agent;
        Target = parameter.target;
        EyeTransform = parameter.eyeTransform;
        PlayerLayer = parameter.playerLayer;
        ObstacleLayer = parameter.obstacleLayer;
        DetectionRadius = parameter.detectionRadius;
        DetectionAngle = parameter.detectionAngle;
    }


    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public bool IsPlayerInSight()
    {
        if (Target == null)
        {
            return false;
        }

        Collider[] playerColliders = Physics.OverlapSphere(EyeTransform.position, DetectionRadius, PlayerLayer);
        
        if (playerColliders.Length == 0)
        {
            return false;
        }

        Vector3 directionToPlayer = (Target.position - EyeTransform.position).normalized;

        if (Vector3.Angle(EyeTransform.forward, directionToPlayer) < DetectionAngle / 2)
        {
            float distanceToPlayer = Vector3.Distance(EyeTransform.position, Target.position);

            if (Physics.Raycast(EyeTransform.position, directionToPlayer, 
                    distanceToPlayer, ObstacleLayer) == false)
            {
                return true;
            }
        }
        
        return false;
    }

}
