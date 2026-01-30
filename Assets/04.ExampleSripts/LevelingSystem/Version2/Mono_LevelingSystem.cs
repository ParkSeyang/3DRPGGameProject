using UnityEngine;
using System;
public class Mono_LevelingSystem : SingletonBase<Mono_LevelingSystem>
{
    // Mono 싱글톤을 사용하면 게임오브젝트 판정입니다.
    // Don't destroy on load를 꼭 해줘야 다른 씬에서도 해당 개체를 참조 할 수 있다.
    // 해당 객체의 생명주기를 고려해서 스크립터블을 사용할지, 모노를 사용할지
    // 잘 선택 할 것
    
    // 원래는 TSV나 Table같은거로 사용하면 됨
    private int[] LevelUpExpTable = new[]
    {
        100,
        200,
        300,
        400,
        500,
        600,
        700,
        800,
        900,
        1000,
    };

    // Runtime Data Field
    private int currentLevel = 0;
    private int currentExp = 0;

    // 원래는 이런식으로 캡슐화한 데이터 모델을 사용하는게 옯습니다.
    // 우리는 튜플을 배우기 위해 위의 currentLevel을 사용합니다.
    private LevelData CurrentLevelData = new LevelData();

    // event 키워드는 해당 델리게이트를 Invoke 하는 권한을 제한합니다.
    // 해당 델리게이트를 가지고 있는 객체만 Invoke를 할 수 있습니다.
    // 구독과 구독해제는 가능합니다.

    // 매개변수 int = currentLevel
    public event Action<int> OnLevelChange;

    // 첫번째 매개변수 int = currentExp
    // 두번째 매개변수 int = 다음레벨까지 남은 경험치
    public event Action<int, int> OnExpChange;

    private int GetRequiredExpForNextLevel(int level)
    {
        return LevelUpExpTable[level];
    }

    public void AddExp(int expAmount)
    {
        if (expAmount <= 0)
        {
            return;
        }

        // 먼저 연산을 해주고
        currentExp += expAmount;

        while (true)
        {
            int requiredExp = GetRequiredExpForNextLevel(currentLevel);

            if (currentExp >= requiredExp)
            {
                currentExp -= requiredExp;
                currentLevel++;

                OnLevelChange?.Invoke(currentLevel);
            }
            else
            {
                break;
            }

        }

        OnExpChange?.Invoke(currentExp, GetRequiredExpForNextLevel(currentLevel) - currentExp);
        // Exp += expAmount;
        // OnExpChange?.Invoke(expAmount);
        // if (expAmount >= 100)
        // {
        //     Exp = 0;
        //     Level++;
        //     OnLevelChange?.Invoke(Level);
        // }
    }

    // 이 함수는 초기화 전용 함수 입니다.
    // Json같은 곳에 LevelData를 저장해 놓고 불러올때 Set하는 용도로 사용을 합니다.
    // 게임내에서 발생하는 이벤트가 아니니까 이벤트 호출을 하지 않습니다.
    public void SetLevelData(int level, int exp)
    {
        CurrentLevelData.Level = level;
        CurrentLevelData.Exp = exp;
    }

    // 튜플을 배우기 위해 아래의 반환 형식을 사용합니다.
    // 사실은 DTO형태(데이터 모델 클래스가 있는)가 더 좋습니다.
    // 반드시 반환자료형으로 사용할때는 명시를 해둬야 합니다.
    // 첫번째가 무엇인지, 두번째가 무엇인지, n번째가 무엇인지
    // 튜플이 안되는 안드로이드 OS에서 동작하지 않는 Case가 있었습니다.
    
    public (int, int) GetCurrentLevelData()
    {
        return (currentLevel, currentExp);
    }  
}
