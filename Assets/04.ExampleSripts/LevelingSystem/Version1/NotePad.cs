using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NotePad : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene(0);
    }
    
    private void OnEnable()
    {
        SO_LevelingSystem.Instance.OnLevelChange += OnChangedLevel;
        SO_LevelingSystem.Instance.OnExpChange += OnChangedExp;
    }

    private void OnDisable()
    {
        SO_LevelingSystem.Instance.OnLevelChange -= OnChangedLevel;
        SO_LevelingSystem.Instance.OnExpChange -= OnChangedExp;
    }

    private void OnChangedLevel(int level)
    {
        Debug.Log($"Level Up! : {level}");
    }

    private void OnChangedExp(int exp, int remainExp)
    {
        Debug.Log($"Get Exp! : {exp}, remain : {remainExp}");
    }

}
