using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DungeonConfig
{
    public int maxIterations;
    public BoundsInt dungeonBounds; 
    public Vector3 splitCenterDeviation;
    public BoundsInt roomBoundsMin;
    public Vector3Int roomOffsetMin;
    public Vector3Int roomPlacementRandomness;
    public bool seededGenerationEnabled;
    public int randomSeed;
    public int corridorWidth = 1;
    public int corridorHeight = 2;
    public int wallThickness = 1;
}
