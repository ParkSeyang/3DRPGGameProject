using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static readonly int InputX = Animator.StringToHash("InputX");
    private static readonly int InputY = Animator.StringToHash("InputY");
    private static readonly int IsAttack = Animator.StringToHash("IsAttack");
    private static readonly int IsMove = Animator.StringToHash("IsMove");
    private static readonly int ComboAttack = Animator.StringToHash("ComboAttack");
    private static readonly int Dead = Animator.StringToHash("Dead");
    private static readonly int Guard = Animator.StringToHash("Guard");
    private static readonly int IsGuard = Animator.StringToHash("IsGuard");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int IsRun = Animator.StringToHash("IsRun");
    
   // private CharacterController controller;
    private float walkSpeed = 5.0f;
    private float runSpeed = 10.0f;
   // private float dampTime = 0.15f;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimEventReceiver animEventReceiver;
    [SerializeField]private Collider playerCollider;

   // [SerializeField] private Transform groundCheck;
   // public float groundCheckDistance = 0.1f;
   // public LayerMask groundMask = ~0;
   // public bool isGrounded = true;
   // public bool prevIsGrounded;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        animEventReceiver = GetComponent<AnimEventReceiver>();
        playerCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        
        animator.SetFloat(InputX, inputX);
        animator.SetFloat(InputY, inputY);
        
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            animator.SetBool(IsMove, true);
        }
        else
        {
            animator.SetBool(IsMove, false);
        }
        
        transform.Translate(new Vector3(inputX * walkSpeed , 0, inputY * walkSpeed) * Time.deltaTime);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool(IsRun, true);
            transform.Translate(new Vector3(inputX * runSpeed, 0, inputY * runSpeed) * Time.deltaTime);
        }
        else
        {
            animator.SetBool(IsRun, false);
        }

    }

  //  private void OnDrawGizmos()
  //  {
  //      if (playerCollider == null)
  //      {
  //          return;
  //      }
//
  //      Gizmos.color = Color.red;
  //      Vector3 boxSize = new Vector3(transform.lossyScale.x, 0.4f, transform.lossyScale.z);
  //      Gizmos.DrawWireCube(groundCheck.position, boxSize);
//
  //  }
//
  //  private bool IsGrounded()
  //  {
  //      Vector3 boxChecker = new Vector3(transform.lossyScale.x, 0.4f, transform.lossyScale.z);
//
  //      return Physics.CheckBox(groundCheck.position, boxChecker, Quaternion.identity, groundMask);
  //  }

}
