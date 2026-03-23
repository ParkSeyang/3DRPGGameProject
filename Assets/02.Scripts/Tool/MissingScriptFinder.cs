#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MissingScriptFinder : MonoBehaviour
{
    [MenuItem("Tools/3DRPG/Find All Missing Scripts")]
    public static void FindAllInScene()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int missingCount = 0;

        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    missingCount++;
                    string hiddenInfo = (go.hideFlags != HideFlags.None) ? " [숨겨진 오브젝트]" : "";
                    Debug.LogError($"<color=red>[범인 발견]</color> <b>{go.name}</b>{hiddenInfo}에 Missing 스크립트 존재!", go);
                }
            }
        }

        Debug.Log($"<color=cyan>[수색 완료]</color> 총 {allObjects.Length}개의 오브젝트를 검사하여 {missingCount}개의 Missing 스크립트를 발견했습니다.");
    }

    [MenuItem("Tools/3DRPG/Clear Hidden Missing Objects")]
    public static void ClearHiddenMissingObjects()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int deletedCount = 0;

        foreach (GameObject go in allObjects)
        {
            if (go.name.Contains("PrefabPainterSceneSettings"))
            {
                Debug.Log($"<color=yellow>[청소 중]</color> 숨겨진 유령 오브젝트 <b>{go.name}</b>를 삭제했습니다.");
                Undo.DestroyObjectImmediate(go);
                deletedCount++;
            }
        }

        Debug.Log($"<color=green>[청소 완료]</color> 총 {deletedCount}개의 유령 오브젝트를 정리했습니다.");
    }
}
#endif