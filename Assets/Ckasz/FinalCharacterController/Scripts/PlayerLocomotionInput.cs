using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ckasz.FinalCharacterController
{
    [DefaultExecutionOrder(-2)]

    public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPLayerLocomotionMapActions
    {
        #region Class Variables
        [SerializeField] private bool holdToSprint = true;

        public bool SprintToggledOn {  get; private set; }
        public PlayerControls PlayerControls { get; private set; }
        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        public bool JumpPressed { get; private set; }
        #endregion
        #region Starup  
        private void OnEnable()
        {
            PlayerControls = new PlayerControls();
            PlayerControls.Enable();

            PlayerControls.PLayerLocomotionMap.Enable();
            PlayerControls.PLayerLocomotionMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            PlayerControls.PLayerLocomotionMap.Disable();
            PlayerControls.PLayerLocomotionMap.RemoveCallbacks(this);
        }
        #endregion
        #region LateUpdate Logic
        private void LateUpdate()
        {
           JumpPressed = false;
        }
        #endregion
        #region Input Callbacks

        public void OnMovement(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context .ReadValue<Vector2>();
        }

        public void OnToggleSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SprintToggledOn = holdToSprint || !SprintToggledOn;
                Debug.Log("SHIFT PRESIONADO: SprintToggledOn = true");

            }
            else if(context.canceled)
            {
                SprintToggledOn = !holdToSprint && SprintToggledOn;
                Debug.Log("SHIFT LIBERADO: SprintToggledOn = false");
            }


        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if(!context.performed)
                return;

            JumpPressed = true;
        }
        #endregion
    }
}


