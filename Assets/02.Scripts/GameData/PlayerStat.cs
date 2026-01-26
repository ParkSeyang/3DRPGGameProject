using System;

[Serializable]
public class PlayerStat
{
    public string Name { get; set; }
    public float HP { get; set; }
    public float MP { get; set; }
    public float ATK { get; set; }
    public float DEF { get; set; }
    public int SP { get; set; }
    public float MoveSpeed { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int Gold { get; set; }
}
