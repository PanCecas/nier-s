using UnityEngine;

namespace Ckasz.FinalCharacterController
{
    public class NemesisMovementInput : MonoBehaviour, IInputSource
    {
        [Header("Target")]
        public Transform playerTarget;

        [Header("Settings")]
        public float visionAngle = 100f;
        public float walkDistance = 6f;
        public float sprintDistance = 12f;
        public float decisionInterval = 1f;

        private float decisionTimer;
        private Vector2 movementInput = Vector2.zero;
        private bool sprintToggled = false;

        public Vector2 MovementInput => movementInput;
        public Vector2 LookInput => Vector2.zero;
        public bool SprintToggledOn => sprintToggled;
        public bool JumpPressed => false;
        public bool RangedAttackStarted => false;
        public bool RangedAttackReleased => false;

        private void Update()
        {
            if (playerTarget == null) return;

            Vector3 toPlayer = playerTarget.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;

            float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
            bool canSeePlayer = angle < visionAngle * 0.5f;

            if (canSeePlayer)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toPlayer.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

                float alignment = Vector3.Dot(transform.forward, toPlayer.normalized);
                bool facingPlayer = alignment > 0.75f;

                decisionTimer -= Time.deltaTime;
                if (decisionTimer <= 0f)
                {
                    sprintToggled = distance > sprintDistance;
                    decisionTimer = decisionInterval;
                }

                if (facingPlayer)
                {
                    Vector3 forward = transform.forward;
                    movementInput = new Vector2(forward.x, forward.z).normalized;
                }
                else
                {
                    movementInput = Vector2.zero;
                }
            }
            else
            {
                movementInput = Vector2.zero;
                sprintToggled = false;
            }
        }

        //genera un campo de vision desde el editor 
        private void OnDrawGizmosSelected()
        {
            if (playerTarget == null) return;

            // Posición actual
            Vector3 position = transform.position;
            position.y += 1f; // Elevar para que se vea mejor

            // Dirección central
            Vector3 forward = transform.forward;

            // Ángulo izquierdo y derecho del cono
            Quaternion leftRayRotation = Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(visionAngle * 0.5f, Vector3.up);

            Vector3 leftDirection = leftRayRotation * forward;
            Vector3 rightDirection = rightRayRotation * forward;

            // Dibuja el cono de visión
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(position, leftDirection * 5f);
            Gizmos.DrawRay(position, rightDirection * 5f);
            Gizmos.DrawRay(position, forward * 5f);

            // Dibuja línea al jugador si lo ve
            Vector3 toPlayer = playerTarget.position - transform.position;
            toPlayer.y = 0f;
            float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
            bool canSeePlayer = angle < visionAngle * 0.5f;

            if (canSeePlayer)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(position, playerTarget.position);
            }
        }

    }
}
