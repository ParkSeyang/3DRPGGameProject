using UnityEngine;
using UnityEngine.SceneManagement;

public class ThirdPersonView : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform headPivotTransform;
    [SerializeField] private Transform cameraPivotTransform;
    [SerializeField] private float horizontalSensitivity = 1.0f;
    [SerializeField] private float verticalSensitivity = 1.0f;
    [SerializeField] private float minAngleX = -60;
    [SerializeField] private float maxAngleX = 60;

    private Vector2 currentAngle = Vector2.zero;

    private void Awake()
    {
        LockCursor();
        ResetCameraPosition();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ResetCameraPosition();

    public void ResetCameraPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && cameraPivotTransform != null)
        {
            mainCamera.transform.position = cameraPivotTransform.position;
            LookAtThePlayer();
        }
    }

    private void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        bool isTitle = sceneName.Equals("00_GameStart", System.StringComparison.OrdinalIgnoreCase);

        if (Time.timeScale == 0f || isTitle == true || (UIManager.IsInitialized && UIManager.Instance.IsPopupOpen))
        {
            return;
        }

        UpdateCameraAngle();
        LookAtThePlayer();
    }

    private void UpdateCameraAngle()
    {
        if (headPivotTransform == null) return;

        Vector2 mouseInput = new Vector2(
            Input.GetAxis("Mouse X") * horizontalSensitivity,
            Input.GetAxis("Mouse Y") * verticalSensitivity
        );

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
        if (cameraTransform != null && cameraTarget != null)
        {
            cameraTransform.LookAt(cameraTarget);
        }
    }
}