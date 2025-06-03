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
        public float minDistanceToStop = 2f;

        public Transform playerTarget;

        private float decisionTimer = 0f;
        private float shootTimer = 0f;

        private void Awake()
        {
            if (playerTarget == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                    playerTarget = player.transform;
                else
                    Debug.LogWarning("NemesisInputSimulator: No se encontró ningún GameObject con tag 'Player'. Asigna manualmente el playerTarget.");
            }
        }

        private void Update()
        {
            if (playerTarget == null) return;

            decisionTimer -= Time.deltaTime;
            shootTimer -= Time.deltaTime;

            // Dirección y distancia
            Vector3 toPlayer = playerTarget.position - transform.position;
            float distanceToPlayer = toPlayer.magnitude;

            // 🧠 Movimiento según distancia
            if (distanceToPlayer > 6f)
            {
                SprintToggledOn = true;
                MovementInput = new Vector2(0f, 1f);

            }
            else if (distanceToPlayer > 2.5f)
            {
                SprintToggledOn = false;
                MovementInput = new Vector2(0f, 1f);

            }
            else
            {
                MovementInput = Vector2.zero;
            }

            // 🔁 Rotar hacia el jugador
            Vector3 lookDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            // 🔫 Disparo con cooldown
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
