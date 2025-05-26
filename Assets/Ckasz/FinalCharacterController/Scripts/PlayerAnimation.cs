using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ckasz.FinalCharacterController
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float locomotionBlendSpeed = 0.02f;

        private PlayerLocomotionInput playerLocomotionInput;

        private static int inputXHasH = Animator.StringToHash("inputX");
        private static int inputYHasH = Animator.StringToHash("inputY");

        private Vector3 currentBlendInput = Vector3.zero;
        private void Awake()
        {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        }

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            Vector2 inputTarget = playerLocomotionInput.MovementInput;
            currentBlendInput = Vector3.Lerp(currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

            animator.SetFloat(inputXHasH, currentBlendInput.x);
            animator.SetFloat(inputYHasH, currentBlendInput.y);
        }
    }
}


