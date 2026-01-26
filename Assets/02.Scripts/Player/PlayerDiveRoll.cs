using System;
using UnityEngine;

public class PlayerDiveRoll : MonoBehaviour
{
    private static readonly int DiveRoll = Animator.StringToHash("DiveRoll");
    [SerializeField] private Animator animator;
    [SerializeField] private AnimEventReceiver animEventReceiver;
    [SerializeField] private float DiveRollSpeed = 10.0f;
    [SerializeField] private bool isDiveRoll = false;

    private AnimatorStateInfo timeInfo;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        animEventReceiver = GetComponent<AnimEventReceiver>();
        
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isDiveRoll == false)
        {
            isDiveRoll = true;
            animator.SetBool(DiveRoll, true);
            timeInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
        else
        {
            animator.SetBool(DiveRoll, false);  
        }
    }
}
