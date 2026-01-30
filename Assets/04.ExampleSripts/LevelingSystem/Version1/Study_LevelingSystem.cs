using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Study_LevelingSystem : MonoBehaviour
{
    private void Awake()
    {
        var levelingSystem = SO_LevelingSystem.Instance;
    }

    private void Start()
    {
        //DontDestroyOnLoad(gameObject);
        
        var levelingSystem = SO_LevelingSystem.Instance;
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {   
            SO_LevelingSystem.Instance.AddExp(1500);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            (int, int) levelData = SO_LevelingSystem.Instance.GetCurrentLevelData();
            int level = levelData.Item1;
            int exp = levelData.Item2;
            
            Debug.Log($"level = {level},  exp = {exp}");

            // 튜플을 사용할때는 반환받은 값개체를 수정하면 안됩니다. 불문율 같은것입니다.
            // 실수가 많이 나옵니다.
            levelData.Item1 += 5;
            levelData.Item2 += 5;
            
            Debug.Log($"level = {levelData.Item1},  exp = {levelData.Item2}");

        }
        
    }
    
   
}
