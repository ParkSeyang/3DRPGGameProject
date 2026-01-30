using UnityEngine;
using Jay;
public class MainSystem : SingletonBase<MainSystem>
{
    // 이거는 재훈썜 스타일
    // MainSystem에서는 프로그램이 실행될 수 있게 각종 초기화,
    // 데이터 로드(테이블, 에셋) 로직들을 넣습니다.
    // 싱글톤으로 할때도 있고 안할때도 있다.
    

    protected override void Awake()
    {
        base.Awake();

        InitializeUser();
        Debug.Log($"[MainSystem] 유저정보 초기화 : User Name : {User.Instance.Name},  Money : {User.Instance.Money}");
    }

    private void InitializeUser()
    {
        User.Instance = new User("PSY", 10000);
    }

}
