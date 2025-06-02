using UnityEngine;

namespace Ckasz.FinalCharacterController
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class SmallProjectile : MonoBehaviour
    {
        [Header("Settings")]
        public float speed = 20f;
        public float lifeTime = 5f;
        public float delayBeforeMorph = 0.1f;
        public float morphScaleZ = 3f;
        public float morphDuration = 0.1f;

        [Header("Homing to Center")]
        public float homingDuration = 0.2f;
        public float homingSharpness = 20f;

        private Vector3 currentDirection;
        private Vector3 targetDirection;
        private float homingTimer = 0f;

        private Rigidbody rb;
        private CapsuleCollider capsule;
        private Vector3 originalScale;
        private bool hasStartedMorph = false;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();

            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.linearVelocity = transform.forward * speed;

            capsule.enabled = false;
            originalScale = transform.localScale;

            Camera cam = Camera.main;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            currentDirection = transform.forward;
            targetDirection = ray.direction.normalized;
            homingTimer = homingDuration;

            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            if (homingTimer > 0f)
            {
                if (!hasStartedMorph)
                {
                    StartCoroutine(MorphStretchZ());
                    hasStartedMorph = true;
                }

                homingTimer -= Time.deltaTime;
                currentDirection = Vector3.Lerp(currentDirection, targetDirection, homingSharpness * Time.deltaTime).normalized;
                rb.linearVelocity = currentDirection * speed;
            }
        }

        private System.Collections.IEnumerator MorphStretchZ()
        {
            float t = 0f;
            Vector3 startScale = originalScale;
            Vector3 endScale = new Vector3(
                originalScale.x,
                originalScale.y,
                originalScale.z * morphScaleZ
            );

            while (t < 1f)
            {
                t += Time.deltaTime / morphDuration;
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            transform.localScale = endScale;
            capsule.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                Destroy(gameObject);
            }
        }
    }
}
