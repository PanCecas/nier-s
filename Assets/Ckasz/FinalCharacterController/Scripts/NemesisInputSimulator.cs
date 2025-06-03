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

            Vector3 toPlayer = playerTarget.position - transform.position;
            float distanceToPlayer = toPlayer.magnitude;

            // Movimiento si está lejos
            if (distanceToPlayer > minDistanceToStop)
            {
                Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z).normalized;
                MovementInput = new Vector2(flatDir.x, flatDir.z);
            }
            else
            {
                MovementInput = Vector2.zero;
            }

            // Rotar en eje Y hacia el jugador
            Vector3 directionToPlayer = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer.normalized);
                Quaternion yOnlyRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, yOnlyRotation, Time.deltaTime * 720f);
            }

            // Sprint aleatorio
            if (decisionTimer <= 0f)
            {
                SprintToggledOn = Random.value > 0.5f;
                decisionTimer = decisionInterval;
            }

            // Disparo con cooldown
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
