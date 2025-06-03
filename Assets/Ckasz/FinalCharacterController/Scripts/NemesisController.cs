using UnityEngine;

namespace Ckasz.FinalCharacterController
{
    [DefaultExecutionOrder(-1)]
    public class NemesisController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform playerTarget;

        [Header("Follow Settings")]
        [SerializeField] private float minFollowDistance = 2.5f;
        [SerializeField] private float rotationSpeed = 720f;

        [Header("Movement")]
        public float runSpeed = 4f;
        public float sprintSpeed = 7f;
        public float acceleration = 0.25f;
        public float sprintAcceleration = 0.5f;
        public float gravity = 25f;
        public float drag = 0.1f;
        public float jumpSpeed = 1.0f;
        public float airControlStrength = 0.2f;
        public float landingCooldown = 0.2f;

        private float landingTimer = 0f;
        private float verticalVelocity = 0f;
        private bool wasFallingLastFrame = false;

        private Vector3 airborneLateralVelocity = Vector3.zero;
        private Vector3 velocity;

        private IInputSource input;
        private PlayerState playerState;

        private void Awake()
        {
            input = GetComponent<IInputSource>();
            playerState = GetComponent<PlayerState>();

            if (playerTarget == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                    playerTarget = player.transform;
            }
        }

        private void Update()
        {
            RotateTowardsPlayer();
            UpdateMovementState();
            HandleVerticalMovement();
            HandleLateralMovement();
        }

        private void RotateTowardsPlayer()
        {
            if (playerTarget == null) return;

            Vector3 direction = playerTarget.position - transform.position;
            direction.y = 0f;

            if (direction.magnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        private void UpdateMovementState()
        {
            bool isMovementInput = input.MovementInput != Vector2.zero;
            bool isSprinting = input.SprintToggledOn && isMovementInput;
            bool isGrounded = characterController.isGrounded;

            if (playerState.CurrentPlayerMovementState == PlayerMovementState.Landing)
            {
                landingTimer -= Time.deltaTime;

                if (landingTimer <= 0f)
                {
                    PlayerMovementState nextState = isSprinting ? PlayerMovementState.Sprinting :
                        isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

                    playerState.SetPlayerMovementState(nextState);
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

            PlayerMovementState movementState = isSprinting ? PlayerMovementState.Sprinting :
                isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            playerState.SetPlayerMovementState(movementState);
        }

        private void HandleVerticalMovement()
        {
            bool isGrounded = playerState.InGroundedState();

            if (isGrounded && verticalVelocity < 0f)
                verticalVelocity = 0f;

            verticalVelocity -= gravity * Time.deltaTime;

            if (input.JumpPressed && isGrounded)
                verticalVelocity += Mathf.Sqrt(jumpSpeed * 3f * gravity);
        }

        private void HandleLateralMovement()
        {
            if (playerTarget == null) return;

            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= minFollowDistance)
            {
                velocity = Vector3.zero;
                return;
            }

            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isGrounded = playerState.InGroundedState();

            float currentAccel = isSprinting ? sprintAcceleration : acceleration;
            float maxSpeed = isSprinting ? sprintSpeed : runSpeed;

            Vector3 direction = transform.forward;
            Vector3 delta = direction * currentAccel;
            Vector3 newVelocity = characterController.velocity + delta;
            Vector3 dragForce = newVelocity.normalized * drag;
            newVelocity = (newVelocity.magnitude > drag) ? newVelocity - dragForce : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
            newVelocity.y += verticalVelocity;

            velocity = newVelocity;
            characterController.Move(newVelocity * Time.deltaTime);

            if (!isGrounded)
            {
                Vector3 airControl = direction * currentAccel * airControlStrength;
                airborneLateralVelocity += airControl;
                airborneLateralVelocity = Vector3.ClampMagnitude(airborneLateralVelocity, maxSpeed);

                Vector3 airborneVelocity = airborneLateralVelocity;
                airborneVelocity.y = verticalVelocity;

                velocity = airborneVelocity;
                characterController.Move(airborneVelocity * Time.deltaTime);
            }
            else
            {
                airborneLateralVelocity = new Vector3(newVelocity.x, 0f, newVelocity.z);
            }
        }
    }
}
