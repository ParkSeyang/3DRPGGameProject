using UnityEngine;
using System;
using System.Collections.Generic;
public abstract class Enemy : MonoBehaviour
{
    public struct StateControllerParameter
    {
        public Mushroom mushroom;
        public Collider attackCollider;
        public Animator mushroomAnimator;
        public AnimEventReceiver AnimEventReceiver;

    }

    protected Mushroom Mushroom { get; private set; }
    
    protected Animator MushRoomAnimator { get; private set; }

    public virtual void Initialize(StateControllerParameter parameter)
    {
        Mushroom = parameter.mushroom;
        MushRoomAnimator = parameter.mushroomAnimator;

    }


    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();
    

    
}
