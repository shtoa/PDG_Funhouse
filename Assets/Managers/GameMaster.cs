using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameMaster
{
    public enum GameState
    {
        PreStart,
        Started,
        Ended,
    }

    public static GameState gameState = GameState.PreStart;
   
    public static int numberOfCylinders = 1;
    public static int numberOfSpheres = 0;
    public static int numberOfCubes = 0;

    public static int cylinderCollected = 0;
    public static int spheresCollected = 0;
    public static int cubesCollected = 0;

}
