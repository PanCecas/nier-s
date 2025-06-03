using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Ckasz.FinalCharacterController
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float locomotionBlendSpeed = 0.02f;

        private MonoBehaviour inputSource;
        private IInputSource input;


        private PlayerState playerState;

        private static int inputXHasH = Animator.StringToHash("inputX");
        private static int inputYHasH = Animator.StringToHash("inputY");
        private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
        private static int isGroundedHash = Animator.StringToHash("isGrounded");
        private static int isFallingHash = Animator.StringToHash("isFalling");
        private static int isLandingHash = Animator.StringToHash("isLanding");
        private static int isJumpingHash = Animator.StringToHash("isJumping");


        private Vector3 currentBlendInput = Vector3.zero;
        private void Awake()
        {
            input = GetComponent<IInputSource>();
            playerState = GetComponent<PlayerState>();
            animator = GetComponent<Animator>();
        }




        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isIdling = playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            bool isRunning = playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
            bool iSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isJumping = playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
            bool isFalling = playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
            bool isLanding = playerState.CurrentPlayerMovementState == PlayerMovementState.Landing;

            bool isGrounded = playerState.InGroundedState();




            Vector2 inputTarget = iSprinting ? input.MovementInput * 1.5f : input.MovementInput;
            currentBlendInput = Vector3.Lerp(currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);


            animator.SetBool(isGroundedHash, isGrounded);
            animator.SetBool(isFallingHash, isFalling);
            animator.SetBool(isJumpingHash, isJumping);
            animator.SetBool(isLandingHash, isLanding);

            animator.SetFloat(inputXHasH, currentBlendInput.x);
            animator.SetFloat(inputYHasH, currentBlendInput.y);
            animator.SetFloat(inputMagnitudeHash, currentBlendInput.magnitude);
        }
    }
}


