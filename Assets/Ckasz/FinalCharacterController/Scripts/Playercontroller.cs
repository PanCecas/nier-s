using UnityEngine;

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
        private float landingTimer = 0f;
        private float verticalVelocity = 0f;
        private bool wasFallingLastFrame = false;

        public float movingThreshold = 0.01f;
        private Vector3 airborneLateralVelocity = Vector3.zero;

        [Header("Camera Settings")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;

        private MonoBehaviour inputSource;
        private IInputSource input;

        private PlayerState playerState;
        private Vector2 cameraRotation = Vector2.zero;
        private Vector2 playerTargetRotation = Vector2.zero;
        #endregion

        #region Startup
        private void Awake()
        {
            inputSource = GetComponent<PlayerLocomotionInput>();
            if (inputSource == null)
                inputSource = GetComponent<NemesisInputSimulator>();

            input = inputSource as IInputSource;

            if (input == null)
                Debug.LogError("❌ El objeto no implementa IInputSource correctamente");

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
            bool isMovementInput = input.MovementInput != Vector2.zero;
            bool isMovingLaterally = IsMovingLaterally();
            bool isSprinting = input.SprintToggledOn && isMovingLaterally;
            bool isGrounded = IsGrounded();

            if (playerState.CurrentPlayerMovementState == PlayerMovementState.Landing)
            {
                landingTimer -= Time.deltaTime;

                if (landingTimer <= 0f)
                {
                    PlayerMovementState nextGroundedState = isSprinting ? PlayerMovementState.Sprinting :
                                                           isMovingLaterally ? PlayerMovementState.Running :
                                                           PlayerMovementState.Idling;

                    playerState.SetPlayerMovementState(nextGroundedState);
                }

                return;
            }

            if (!isGrounded && verticalVelocity >= 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
                wasFallingLastFrame = false;
            }
            else if (!isGrounded && verticalVelocity < 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Falling);
                wasFallingLastFrame = true;
            }
            else if (isGrounded && wasFallingLastFrame)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Landing);
                landingTimer = landingCooldown;
                wasFallingLastFrame = false;
                return;
            }

            PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting :
                                                isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            playerState.SetPlayerMovementState(lateralState);

            if (!isGrounded && verticalVelocity >= 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
            }
            else if (!isGrounded && verticalVelocity < 0f)
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

            if (input.JumpPressed && isgrounded)
            {
                verticalVelocity += Mathf.Sqrt(jumpspeed * 3 * gravity);
            }
        }

        private void HandleLateralMovement()
        {
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isGrounded = playerState.InGroundedState();

            float lateralAcceleration = isSprinting ? sprintAcceleration : runAcceleration;
            float clampLateralMagnitude = isSprinting ? sprintSpeed : runSpeed;

            Vector3 movementDirection;

            if (playercamera != null)
            {
                Vector3 cameraForwardXZ = new Vector3(playercamera.transform.forward.x, 0f, playercamera.transform.forward.z).normalized;
                Vector3 cameraRightYZ = new Vector3(playercamera.transform.right.x, 0f, playercamera.transform.right.z).normalized;
                movementDirection = cameraRightYZ * input.MovementInput.x + cameraForwardXZ * input.MovementInput.y;
            }
            else
            {
                Vector3 forward = transform.forward;
                Vector3 right = transform.right;
                movementDirection = right * input.MovementInput.x + forward * input.MovementInput.y;
            }

            Vector3 movemenDelta = movementDirection * lateralAcceleration;
            Vector3 newVelocity = characterController.velocity + movemenDelta;

            Vector3 currentDrag = newVelocity.normalized * drag;
            newVelocity = (newVelocity.magnitude > drag) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(newVelocity, clampLateralMagnitude);
            newVelocity.y += verticalVelocity;

            nelocity = newVelocity;
            characterController.Move(newVelocity * Time.deltaTime);

            if (isGrounded)
            {
                airborneLateralVelocity = new Vector3(newVelocity.x, 0f, newVelocity.z);
                newVelocity.y = verticalVelocity;
            }
            else
            {
                Vector3 airControl = movementDirection * lateralAcceleration * airControlStrenthg;
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
            if (playercamera != null)
            {
                cameraRotation.x += lookSenseH * input.LookInput.x;
                cameraRotation.y = Mathf.Clamp(cameraRotation.y - lookSenseV * input.LookInput.y, -lookLimitV, lookLimitV);

                playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * input.LookInput.x;
                playercamera.transform.rotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0f);
            }

            transform.rotation = Quaternion.Euler(0f, playerTargetRotation.x, 0f);
        }
        #endregion

        #region State Checks 
        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(nelocity.x, 0f, nelocity.z);
            return lateralVelocity.magnitude > movingThreshold;
        }

        private bool IsGrounded()
        {
            return characterController.isGrounded;
        }
        #endregion
    }
}