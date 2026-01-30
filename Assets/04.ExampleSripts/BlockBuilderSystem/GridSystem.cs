using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem
{
    public Vector3 CellSize { get; private set; }

    public GridSystem(Vector3 cellSize)
    {
        CellSize = cellSize;
    }

    public Vector3Int WorldToGrid(Vector3 position)
    {
        Vector3Int reVal = new Vector3Int();

        // Mathf.FloorToInt(1); //버림
        // Mathf.RoundToInt(2); //반올림
        // Mathf.CeilToInt(1); //올림
        
        reVal.x = Mathf.FloorToInt(position.x / CellSize.x);
        reVal.y = Mathf.FloorToInt(position.y / CellSize.y);
        reVal.z = Mathf.FloorToInt(position.z / CellSize.z);
        
        return reVal;
    }

    public Vector3 GridToWorld(Vector3Int gridPosition)
    {
        Vector3 reVal = new Vector3();

        reVal.x = gridPosition.x * CellSize.x;
        reVal.y = gridPosition.y * CellSize.y;
        reVal.z = gridPosition.z * CellSize.z;
        
        return reVal;
    }
}