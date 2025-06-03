using UnityEngine;

namespace Ckasz.FinalCharacterController
{
    public class NemesisCombat : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject smallProjectilePrefab;
        [SerializeField] private Transform rightShootPoint;
        [SerializeField] private Transform leftShootPoint;
        [SerializeField] private float timeBetweenShots = 1f;
        [SerializeField] private float shootSpeed = 20f;

        private float shootTimer;
        private bool useRightHand = true;

        private IInputSource input;
        private PlayerState playerState;
        private Animator animator;
        private Transform playerTarget;

        private void Awake()
        {
            input = GetComponent<IInputSource>();
            playerState = GetComponent<PlayerState>();
            animator = GetComponent<Animator>();

            // Encuentra al jugador automáticamente si es necesario
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                playerTarget = player.transform;
        }

        private void Update()
        {
            if (playerState.CurrentPlayerCombatState != PlayerCombatState.Unarmed)
                return;

            shootTimer -= Time.deltaTime;

            if (input.RangedAttackStarted && shootTimer <= 0f)
            {
                FireSmallProjectile();
                shootTimer = timeBetweenShots;
            }
        }

        private void FireSmallProjectile()
        {
            if (playerTarget == null) return;

            Transform shootPoint = useRightHand ? rightShootPoint : leftShootPoint;
            Vector3 shootDirection = (playerTarget.position - shootPoint.position).normalized;

            GameObject projectile = Instantiate(smallProjectilePrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.linearVelocity = shootDirection * shootSpeed;

            if (animator != null)
                animator.SetTrigger(useRightHand ? "shootRight" : "shootLeft");

            useRightHand = !useRightHand;
        }
    }
}
