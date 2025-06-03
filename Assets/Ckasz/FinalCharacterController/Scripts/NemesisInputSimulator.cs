using UnityEngine;

namespace Ckasz.FinalCharacterController
{
    public class NemesisInputSimulator : MonoBehaviour, IInputSource
    {
        #region Interface Inputs
        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; } = Vector2.zero;
        public bool SprintToggledOn { get; private set; } = false;
        public bool JumpPressed { get; private set; } = false;
        public bool RangedAttackStarted { get; private set; } = false;
        public bool RangedAttackReleased { get; private set; } = false;
        #endregion

        #region Configuration
        [Header("Settings")]
        public Transform playerTarget;
        public float shootInterval = 1f;
        [Range(30f, 180f)] public float fieldOfViewAngle = 120f;
        [Range(0.1f, 5f)] public float memoryTime = 1.5f;
        [Range(1f, 30f)] public float rotationSpeed = 10f;

        #endregion

        #region Internal State
        private float shootTimer = 0f;
        private bool canSeePlayer = false;
        private float memoryTimer = 0f;
        #endregion

        #region Combat Reaction
        private int hitCount = 0;
        private bool isHostile = false;
        private int revengeShots = 0;
        #endregion

        #region Unity Methods
        private void Update()
        {
            if (playerTarget == null) return;

            HandleVision();
            HandleRotation();
            HandleMovementAndAttack();
        }
        #endregion

        #region Vision
        private void HandleVision()
        {
            shootTimer -= Time.deltaTime;

            Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            if (angle < fieldOfViewAngle * 0.5f)
            {
                canSeePlayer = true;
                memoryTimer = memoryTime;
            }
            else
            {
                memoryTimer -= Time.deltaTime;
                if (memoryTimer <= 0f)
                    canSeePlayer = false;
            }
            #if UNITY_EDITOR
            Debug.DrawRay(transform.position, transform.forward * 5f, Color.green); // Centro
            Debug.DrawRay(transform.position, Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * transform.forward * 5f, Color.yellow); // Límite derecho
            Debug.DrawRay(transform.position, Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * transform.forward * 5f, Color.yellow); // Límite izquierdo
            #endif

        }
        #endregion

        #region Rotation
        private void HandleRotation()
        {
            Vector3 lookDir = playerTarget.position - transform.position;
            lookDir.y = 0f;

            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);

            }
        }
        #endregion

        #region Movement and Attack
        private void HandleMovementAndAttack()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            if (canSeePlayer)
            {
                Vector3 dir = (playerTarget.position - transform.position).normalized;
                Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
                MovementInput = new Vector2(flatDir.x, flatDir.z).normalized;

                if (distanceToPlayer > 8f)
                    SprintToggledOn = true;
                else if (distanceToPlayer < 4f)
                    SprintToggledOn = false;

                if (distanceToPlayer > 2.5f)
                {
                    if (isHostile || revengeShots > 0)
                    {
                        RangedAttackStarted = shootTimer <= 0f;
                        RangedAttackReleased = shootTimer <= 0f;

                        if (shootTimer <= 0f)
                        {
                            shootTimer = shootInterval;
                            if (revengeShots > 0)
                                revengeShots--;
                        }
                    }
                    else
                    {
                        RangedAttackStarted = false;
                        RangedAttackReleased = false;
                    }
                }
                else
                {
                    MovementInput = Vector2.zero;
                    RangedAttackStarted = false;
                    RangedAttackReleased = false;
                }
            }
            else
            {
                MovementInput = Vector2.zero;
                RangedAttackStarted = false;
                RangedAttackReleased = false;
            }
        }
        #endregion

        #region Public Methods
        public void RegisterHit()
        {
            hitCount++;

            if (!isHostile)
            {
                if (hitCount == 3)
                {
                    revengeShots = 2;
                }
                else if (hitCount >= 5)
                {
                    isHostile = true;
                }
            }
        }
        #endregion
    }
}
