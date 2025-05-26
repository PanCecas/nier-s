using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.VirtualTexturing;

namespace Ckasz.FinalCharacterController
{
    [DefaultExecutionOrder(-1)]
    public class Playercontroller : MonoBehaviour
    {
        #region Class Variables
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        Vector3 nelocity;
        [SerializeField] private Camera playercamera;

        [Header("Base Movement")]
        public float runAcceleration = 0.25f;
        public float runSpeed = 4f;
        public float sprintAcceleration = 0.5f;
        public float sprintSpeed = 7f; 
        public float drag = 0.1f;
        public float movingThreshold = 0.01f; 

        [Header("Camera Settings")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;


        private PlayerLocomotionInput playerLocomotionInput;
        private PlayerState playerState;
        private Vector2 cameraRotation = Vector2.zero;
        private Vector2 playerTargetRotation = Vector2.zero;
        #endregion

        #region Startup
        private void Awake()
        {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();
        }
        #endregion

        #region Update Logic
        private void Update()
        {
            UpdateMovementState();
            HandleLateralMovement();
        }

        private void UpdateMovementState()
        {
            bool isMovementInput = playerLocomotionInput.MovementInput != Vector2.zero;
            bool isMovingLaterally = IsMovingLaterally();
            bool isSprinting = playerLocomotionInput.SprintToggledOn && isMovingLaterally; //false ?

           // Debug.Log("is sprinting"+playerLocomotionInput.SprintToggledOn );
           // Debug.Log("is movingLaterally"+isMovingLaterally);

            PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting :
                                                isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            playerState.SetPlayerMovementState(lateralState);
        }

        private void HandleLateralMovement()
        {
            // referencia 
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting; // true ?  

            float lateralAcceleration = isSprinting ? sprintAcceleration : runAcceleration; 
            float clampLateralMagnitude = isSprinting ? sprintSpeed : runSpeed;


            Vector3 cameraForwardXZ = new Vector3(playercamera.transform.forward.x, 0f, playercamera.transform.forward.z).normalized;
            Vector3 cameraRightYZ = new Vector3(playercamera.transform.right.x, 0f, playercamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightYZ * playerLocomotionInput.MovementInput.x + cameraForwardXZ * playerLocomotionInput.MovementInput.y;

            Vector3 movemenDelta = movementDirection * lateralAcceleration;
            Vector3 newVelocity = characterController.velocity + movemenDelta;


            Vector3 currentDrag = newVelocity.normalized * drag;
            newVelocity = (newVelocity.magnitude > drag) ?  newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(newVelocity, clampLateralMagnitude);
            nelocity = newVelocity;
            characterController.Move(newVelocity * Time.deltaTime);
        }
        #endregion

        #region Late Update Logic
        private void LateUpdate()
        {
            cameraRotation.x += lookSenseH * playerLocomotionInput.LookInput.x;
            cameraRotation.y = Mathf.Clamp(cameraRotation.y - lookSenseV * playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

            playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * playerLocomotionInput.LookInput.x;
            transform.rotation = Quaternion.Euler(0f, playerTargetRotation.x, 0f);

            playercamera.transform.rotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0f);

        }
        #endregion

        #region State Checks 
        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3 (nelocity.x, 0f, nelocity.z);
          
            /* if(characterController.velocity.x > movingThreshold || characterController.velocity.z > movingThreshold)
            {
                Debug.Log("valor x" + characterController.velocity.x);
                Debug.Log("valor y" + characterController.velocity.z);
                Debug.Log("magnitud" + lateralVelocity.magnitude);
            } */
            
            
            return lateralVelocity.magnitude > movingThreshold;

            
        }
        #endregion
    }
}


