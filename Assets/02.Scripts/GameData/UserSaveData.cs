using System;
using UnityEngine;

[Serializable]
public class UserSaveData
{
    // 플레이어 스탯 정보 (PlayerStat 클래스 재활용 가능)
    public PlayerStat playerStat;
    
    // 위치 정보
    public float posX;
    public float posY;
    public float posZ;

    public void SetPosition(Vector3 position)    
    {
        posX = position.x;
        posY = position.y;
        posZ = position.z;
    }

    public Vector3 GetPosition() => new Vector3(posX, posY, posZ);
}
