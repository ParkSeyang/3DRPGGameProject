using System;
using UnityEngine;

public class ThirdPersonView : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform headPivotTransform;
    [SerializeField] private Transform cameraPivotTransform;
    [SerializeField] private float horizontalSensitivity = 1.0f;
    [SerializeField] private float verticalSensitivity = 1.0f;
    [SerializeField] private float minAngleX = -90;
    [SerializeField] private float maxAngleX = 90;

    private Vector2 currentAngle = Vector2.zero;

    private void Awake()
    {
        LockCursor();
        Camera cam = Camera.main;
        cam.transform.position = cameraPivotTransform.transform.position;
    }

    private void Update()
    {
        UpdateCameraAngle();
        LookAtThePlayer();
    }

    private void UpdateCameraAngle()
    {
        Vector2 mouseInput = new Vector2(
            Input.GetAxis("Mouse X") * horizontalSensitivity
            , Input.GetAxis("Mouse Y") * verticalSensitivity);
        
        currentAngle.x -= mouseInput.y;

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            currentAngle.y += mouseInput.x;
        }
        else
        {
            transform.Rotate(Vector3.up, mouseInput.x);
            currentAngle.y = 0.0f;
        }
        
        currentAngle.x = Mathf.Clamp(currentAngle.x, minAngleX, maxAngleX);
        headPivotTransform.localRotation = Quaternion.Euler(currentAngle);
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void LookAtThePlayer()
    {
        cameraTransform.LookAt(cameraTarget);
    }



}
