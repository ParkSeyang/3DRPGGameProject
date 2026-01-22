using System;
using UnityEngine;

public class PlayerDiveRoll : MonoBehaviour
{
    private static readonly int DiveRoll = Animator.StringToHash("DiveRoll");
    [SerializeField] private Animator animator;
    [SerializeField] private AnimEventReceiver animEventReceiver;
    [SerializeField] private float DiveRollSpeed = 10.0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animEventReceiver = GetComponent<AnimEventReceiver>();
        
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool(DiveRoll, true);
        }
        else
        {
            animator.SetBool(DiveRoll, false);
        }
    }
}
