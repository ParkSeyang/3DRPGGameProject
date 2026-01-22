using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AI;

public abstract class BaseState
{
    protected enum MushroomState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Hit,
        Dead,
    }
    
    public struct StateControllerParameter
    {
        public Mushroom mushroom;
        public Collider attackCollider;
        public Animator mushroomAnimator;
        public AnimEventReceiver AnimEventReceiver;

    }

    protected Mushroom Mushroom { get; private set; }
    
    protected Animator MushRoomAnimator { get; private set; }
    [SerializeField] private NavMeshAgent agent;
    
    protected MushroomState mushroomState;
    public virtual void Initialize(StateControllerParameter parameter)
    {
        Mushroom = parameter.mushroom;
        MushRoomAnimator = parameter.mushroomAnimator;
    }


    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();
    
}
