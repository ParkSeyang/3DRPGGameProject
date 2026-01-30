using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Bounds bounds;
    public BlockData Data { get; set; }

    public bool OverLaps(Bounds other) => bounds.Intersects(other);
    
    private void Start()
    {
       
    }

    public void Place()
    {
        bounds = BoundsUtil.CalculateRendererBounds(gameObject);
        Data = new BlockData();
        
        Data.PosX = transform.position.x;
        Data.PosY = transform.position.y;
        Data.PosZ = transform.position.z;
        
        Data.AngleY = transform.eulerAngles.y;
    }
}

public class BlockData
{
    public int PrefabIndex { get; set; }
    
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    
    public float AngleY { get; set; }
    
    public Vector3 GetPosition()
    {
        return new Vector3(PosX, PosY, PosZ);
    }
}

