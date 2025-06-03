using UnityEngine;

namespace Ckasz.FinalCharacterController
{
    public class NemesisInputSimulator : MonoBehaviour, IInputSource
    {
        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; } = Vector2.zero;
        public bool SprintToggledOn { get; private set; } = false;
        public bool JumpPressed { get; private set; } = false;
        public bool RangedAttackStarted { get; private set; } = false;
        public bool RangedAttackReleased { get; private set; } = false;

        [Header("Behavior Settings")]
        public float decisionInterval = 1.5f;
        public float shootInterval = 2f;
        public Transform playerTarget;

        private float decisionTimer = 0f;
        private float shootTimer = 0f;

        private void Update()
        {
            if (playerTarget == null) return;

            decisionTimer -= Time.deltaTime;
            shootTimer -= Time.deltaTime;

            // Dirección hacia el jugador
            Vector3 dir = (playerTarget.position - transform.position).normalized;
            Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
            MovementInput = new Vector2(flatDir.x, flatDir.z).normalized;

            // 🔁 Rotar hacia el jugador
            Vector3 lookDir = playerTarget.position - transform.position;
            lookDir.y = 0f;
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            if (decisionTimer <= 0f)
            {
                SprintToggledOn = Random.value > 0.5f;
                decisionTimer = decisionInterval;
            }

            if (shootTimer <= 0f)
            {
                RangedAttackStarted = true;
                RangedAttackReleased = true;
                shootTimer = shootInterval;
            }
            else
            {
                RangedAttackStarted = false;
                RangedAttackReleased = false;
            }
        }
    }
}
