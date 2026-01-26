using UnityEngine;
using System;
using System.Collections.Generic;
public class Player : MonoBehaviour
{
    [Header("Data Settings")] 
    [SerializeField] private string className = "ZeroDarkMos";
    // 스탯등 로직 정리
    public string Name { get; private set; }
    public float HP { get; private set; }
    public float MP { get; private set; }
    public float ATK { get; private set; }
    public float DEF { get; private set; }
    public int SP { get; private set; }
    public float MoveSpeed { get; private set; }
    public int Level { get; private set; }
    public int Exp { get; private set; }
    public int Gold { get; private set; }

    private void Awake()
    {
        if (DataManager.Instance != null)
        {
            var stat = DataManager.Instance.GetPlayerStat(className);
            if (stat != null)
            {
                Name = stat.Name;
                HP = stat.HP;
                MP = stat.MP;
                ATK = stat.ATK;
                DEF = stat.DEF;
                SP = stat.SP;
                MoveSpeed = stat.MoveSpeed;
                Level = stat.Level;
                Exp = stat.Exp;
                Gold = stat.Gold;

                Debug.Log($"[Player] {className} 데이터 로드 완료.");
            }
        }
    }
}
