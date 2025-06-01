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
        public float gravity = 25f;
        public float jumpspeed = 1.0f;
        public float airControlStrenthg = 0.2f;
        public float landingCooldown = 0.2f; 
        private float verticalVelocity = 0f;
        private bool wasFallingLastFrame = false;        
        private float landingTimer = 0f;

        public float movingThreshold = 0.01f;
        private Vector3 airborneLateralVelocity = Vector3.zero;


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
            HandleVerticalMovement();
            HandleLateralMovement();
        }

        private void UpdateMovementState()
        {
            bool isMovementInput = playerLocomotionInput.MovementInput != Vector2.zero;
            bool isMovingLaterally = IsMovingLaterally();
            bool isSprinting = playerLocomotionInput.SprintToggledOn && isMovingLaterally; //false ?
            bool isGrounded = IsGrounded();

            //quiero saber si islanding o el estado landing si se esta llamando 

            /*Debug.Log($"[DEBUG] CurrentState: {playerState.CurrentPlayerMovementState}, " +
          $"isGrounded: {isGrounded}, verticalVelocity: {verticalVelocity}, " +
          $"wasFallingLastFrame: {wasFallingLastFrame}, landingTimer: {landingTimer}"); */


            //  BLOQUE 1: Si el personaje está aterrizando, mantener el estado Landing hasta que expire el timer
            if (playerState.CurrentPlayerMovementState == PlayerMovementState.Landing)
            {
                // Reducimos el tiempo de espera de la animación de aterrizaje
                landingTimer -= Time.deltaTime;

                // Cuando se termina el timer, se decide a qué estado pasar (Idle, Running, Sprinting)
                if (landingTimer <= 0f)
                {
                    PlayerMovementState nextGroundedState = isSprinting ? PlayerMovementState.Sprinting :
                                                           isMovingLaterally ? PlayerMovementState.Running :
                                                           PlayerMovementState.Idling;

                    // Transición final hacia locomotion una vez terminada la animación de aterrizaje
                    playerState.SetPlayerMovementState(nextGroundedState);
                }

                // Se corta aquí para que nada más pueda cambiar el estado durante el aterrizaje
                return;
            }

            // 🟡 BLOQUE 2: Si está en el aire subiendo, está saltando
            if (!isGrounded && verticalVelocity >= 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
                wasFallingLastFrame = false; // aún no está cayendo
                Debug.Log("🟡 CAMBIO A JUMPING");
            }
            // 🔻 BLOQUE 3: Si está en el aire y bajando, está cayendo
            else if (!isGrounded && verticalVelocity < 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Falling);
                wasFallingLastFrame = true; // marcamos que venía cayendo
                Debug.Log("🔻 CAMBIO A FALLING");
            }
            // 🟢 BLOQUE 4: Aterrizó luego de caer → entramos en Landing
            else if (isGrounded && wasFallingLastFrame)
            {
                Debug.Log("🟢 CAMBIO A LANDING");
                playerState.SetPlayerMovementState(PlayerMovementState.Landing);
                landingTimer = landingCooldown; // reiniciamos timer de animación de aterrizaje
                wasFallingLastFrame = false;

                return;
            }

            // Debug.Log("is sprinting"+playerLocomotionInput.SprintToggledOn );
            // Debug.Log("is movingLaterally"+isMovingLaterally);

            PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting :
                                                isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            playerState.SetPlayerMovementState(lateralState);

            if (!isGrounded && verticalVelocity >= 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
            }
            else if (!isGrounded && verticalVelocity <  0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Falling);
            }
                
        }

        private void HandleVerticalMovement()
        {
            bool isgrounded = playerState.InGroundedState();

            if (isgrounded && verticalVelocity < 0) 
                verticalVelocity = 0;

            verticalVelocity -= gravity * Time.deltaTime;

            if (playerLocomotionInput.JumpPressed && isgrounded)
            {
                verticalVelocity += Mathf.Sqrt(jumpspeed * 3 * gravity);
            }
        }

        private void HandleLateralMovement()
        {
            // referencia 
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting; // true ?  
            bool isGrounded = playerState.InGroundedState();
            

            float lateralAcceleration = isSprinting ? sprintAcceleration : runAcceleration; 
            float clampLateralMagnitude = isSprinting ? sprintSpeed : runSpeed;


            Vector3 cameraForwardXZ = new Vector3(playercamera.transform.forward.x, 0f, playercamera.transform.forward.z).normalized;
            Vector3 cameraRightYZ = new Vector3(playercamera.transform.right.x, 0f, playercamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightYZ * playerLocomotionInput.MovementInput.x + cameraForwardXZ * playerLocomotionInput.MovementInput.y;

            Vector3 movemenDelta = movementDirection * lateralAcceleration;
            Vector3 newVelocity = characterController.velocity + movemenDelta;

            Vector3 currentDrag = newVelocity.normalized * drag;
            newVelocity = (newVelocity.magnitude > drag) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(newVelocity, clampLateralMagnitude);
            newVelocity.y += verticalVelocity;

            //cree esta variable para solucionar lo del character controller, siempre da0 
            nelocity = newVelocity;
            characterController.Move(newVelocity * Time.deltaTime); //Character.velocity.x 
            
            if (isGrounded)
            {
                // este if declara esta velocidad, para el movimiento parabolico 
                airborneLateralVelocity = new Vector3(newVelocity.x, 0f, newVelocity.z);

                newVelocity.y = verticalVelocity;

            }
            
            else
            {
                // 🟢 Movimiento en el aire (mantiene inercia + leve control tipo Nier)
                Vector3 airControl = movementDirection * lateralAcceleration * airControlStrenthg; // 🟢 control en el aire (ajustable)
                airborneLateralVelocity += airControl;
                airborneLateralVelocity = Vector3.ClampMagnitude(airborneLateralVelocity, clampLateralMagnitude);

                Vector3 airborneVelocity = airborneLateralVelocity;
                airborneVelocity.y = verticalVelocity;

                nelocity = airborneVelocity;
                characterController.Move(airborneVelocity * Time.deltaTime);
            }
            

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

            //acabo de hacer el github
            
            
            return lateralVelocity.magnitude > movingThreshold;

            
        }

        private bool IsGrounded()
        {
            return characterController.isGrounded;  
        }
        #endregion
    }
}


