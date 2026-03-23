using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject hintSprite; // SpriteRenderer가 있는 자식 오브젝트

    private void Awake()
    {
        if (hintSprite != null) hintSprite.SetActive(false);
    }

    public void Show(string message = "")
    {
        if (hintSprite != null) hintSprite.SetActive(true);
    }

    public void Hide()
    {
        if (hintSprite != null) hintSprite.SetActive(false);
    }

    private void LateUpdate()
    {
        if (hintSprite != null && hintSprite.activeSelf == true)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                // 카메라의 회전값과 일치시켜 항상 정면을 바라보게 함 (반전 버그 해결)
                transform.rotation = mainCam.transform.rotation;
            }
        }
    }
}
