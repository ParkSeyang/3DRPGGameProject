using UnityEngine;

public class SaveZone : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (playerLayer.Contains(other.gameObject))
        {
            if (DataManager.Instance != null)
            {
                DataManager.Instance.CanSave = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerLayer.Contains(other.gameObject))
        {
            if (DataManager.Instance != null)
            {
                // 현재 씬이 사냥터라면 다시 저장을 막음
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (currentScene.Contains("BeginnersForest"))
                {
                    DataManager.Instance.CanSave = false;
                }
            }
        }
    }
}
