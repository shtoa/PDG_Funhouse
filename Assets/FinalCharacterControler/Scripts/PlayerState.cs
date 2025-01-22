using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerMovementState
{
    Idle = 0,
    Walk = 1,
    Run = 2,
    Sprint = 3,
    Jump = 4,
    Fall = 5,
    Strafe = 6,
    Climb = 7,
}

public class PlayerState : MonoBehaviour
{

    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idle;


    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        CurrentPlayerMovementState = playerMovementState;
    }
    public bool InGroundedState()
    {
        return IsStateGroundedState(CurrentPlayerMovementState);
    }

    public bool IsStateGroundedState(PlayerMovementState movementState)
    {
        return movementState == PlayerMovementState.Idle ||
               movementState == PlayerMovementState.Walk ||
               movementState == PlayerMovementState.Run ||
               movementState == PlayerMovementState.Sprint;
    }
  
}
