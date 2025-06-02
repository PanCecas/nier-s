using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ckasz.FinalCharacterController
{
    public class PlayerState : MonoBehaviour
    {
        [field: SerializeField] public PlayerCombatState CurrentPlayerCombatState {  get; private set; } = PlayerCombatState.Unarmed;

            public void SetPlayerCombatState( PlayerCombatState playerCombatState)
        {
            CurrentPlayerCombatState = playerCombatState;
        }

        [field:SerializeField] public PlayerMovementState CurrentPlayerMovementState {  get; private set; } = PlayerMovementState.Idling;

        public void SetPlayerMovementState(PlayerMovementState playerMovementState)
        {
            CurrentPlayerMovementState = playerMovementState;
        }

        public bool InGroundedState()
        {
            return CurrentPlayerMovementState == PlayerMovementState.Idling ||
                   CurrentPlayerMovementState == PlayerMovementState.Walking ||
                   CurrentPlayerMovementState == PlayerMovementState.Running ||                 
                   CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        }
    }
    public enum PlayerCombatState
    {
        Unarmed = 0,
        Charging = 1,
        Armed = 2
    }
    public enum PlayerMovementState
    {
        Idling = 0,
        Walking = 1,
        Running = 2,
        Sprinting = 3,
        Jumping = 4,
        Falling = 5,
        Strafing = 6,
        Landing = 7,
    }

}
