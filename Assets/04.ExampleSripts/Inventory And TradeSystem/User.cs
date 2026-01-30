using System;

// static class 를 사용ㅎ던가
// singleton 을 사용하던가
// public static class UserA
// {
//         
// }

public class User
{
    public static User Instance;

    public string Name { get; private set; }
    public int Money { get; private set; }

    public User(string name, int money)
    {
        Name = name;
        Money = money;
    }

    public void IncreaseMoney(int amount)
    {
        Money += amount;
    }

    // 함수를 따로해놓는 이유
    // price를 -로 입력하는것을 좋아하지 않음
    // 그리고 생각외로 디테일한 처리가 필요할때도 있음
    public void DecreaseMoney(int amount)
    {
        Money -= amount;
    }
}

