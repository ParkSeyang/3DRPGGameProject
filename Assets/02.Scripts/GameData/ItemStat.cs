using System;

[Serializable]
public class ItemStat
{
    public float Attack;
    public float Defense;
    public float Health; // 회복량 또는 최대 체력 증가량
    public float Mana;   // 회복량 또는 최대 마나 증가량

    public ItemStat() { }

    // 연산자 오버로딩 (스탯 합산용)
    public static ItemStat operator +(ItemStat a, ItemStat b)
    {
        ItemStat result = new ItemStat();
        result.Attack = a.Attack + b.Attack;
        result.Defense = a.Defense + b.Defense;
        result.Health = a.Health + b.Health;
        result.Mana = a.Mana + b.Mana;
        return result;
    }
}