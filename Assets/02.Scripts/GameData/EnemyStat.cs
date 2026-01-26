using System;
using System.Collections.Generic;
[Serializable]
public class EnemyStat
{
    public int ID { get; set; }
    public string Name { get; set; }
    public float HP { get; set; }
    public float ATK { get; set; }
    public float DEF { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public float MoveSpeed { get; set; }
}
