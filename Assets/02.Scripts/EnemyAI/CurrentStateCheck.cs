using System;
using UnityEngine;

public class CurrentStateCheck : MonoBehaviour
{
    [SerializeField] private Mushroom mushroom;

    private void Update()
    {
        Debug.Log($"현재 상태 : {mushroom.CurrentState}");
    }
}
