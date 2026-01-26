using System;
using UnityEngine;

public class PlayerGuard : MonoBehaviour
{
    private static readonly int Guard = Animator.StringToHash("Guard");
    private static readonly int IsGuard = Animator.StringToHash("IsGuard");
    private Animator animator;
    private AnimEventReceiver animEventReceiver;
    
    // 조건식을 체크해서 해주자.

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animEventReceiver = GetComponent<AnimEventReceiver>();
    }

    private void Start()
    {
       
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            animator.SetBool(IsGuard, true);
           // Debug.Log($"가드가 잘되고있음? {animator.GetBool(IsGuard)}");
        }
        else
        {
            animator.SetBool(IsGuard, false);
           // Debug.Log($"가드가 풀려야됨 {animator.GetBool(IsGuard)}");
        }
    }
    
 
   
}
