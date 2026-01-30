using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelingSystem : MonoBehaviour
{

    private UserData userData;
    [SerializeField] private TMP_Text userName;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text mpText; 
    [SerializeField] private TMP_Text expText;
    
    [SerializeField] private Image hpBarImage; 
    [SerializeField] private Image mpBarImage; 
    [SerializeField] private Image expBarImage;
    
    
    
    // 비율 = 전체 중에 부분
    // 부분 / 전체 = 현재 HP / 최대 HP
    
    private void Awake()
    {
        userData = new UserData();
        userData.userName = "ZeroOne";
        userData.userlevel = 3;
        userData.Hp = 50.0f;
        userData.Mp = 50.0f;
        userData.currentExp = 99.0f;
        userData.MaxExp = 999.0f;
        userData.MaxHp = 100.0f;
        userData.MaxMp = 100.0f;
        
    }
    
    // Update is called once per frame
    void Update()
    {
        userName.text = userData.userName;
        levelText.text = userData.userlevel.ToString();
        
        hpBarImage.fillAmount = userData.Hp / userData.MaxHp;
        hpText.SetText($"{userData.Hp.ToString()} / {userData.MaxHp.ToString()}");
        
        mpBarImage.fillAmount = userData.Mp / userData.MaxMp;
        mpText.SetText($"{userData.Mp.ToString()} / {userData.MaxMp.ToString()}");
        
        expBarImage.fillAmount = userData.currentExp / userData.MaxExp;
        expText.SetText($"{userData.currentExp.ToString()} / {userData.MaxExp.ToString()}");
        
        
    }
}
