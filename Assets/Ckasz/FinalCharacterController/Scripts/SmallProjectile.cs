using UnityEngine;

namespace Ckasz.FinalCharacterController
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class SmallProjectile : MonoBehaviour
    {
        [Header("Movement")]
        public float speed = 20f;
        public float lifeTime = 5f;

        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.linearVelocity = transform.forward * speed;

            Destroy(gameObject, lifeTime);
            Debug.Log("?? SmallProjectile lanzado desde: " + transform.position);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("?? Impacto con: " + other.name);

            if (!other.CompareTag("Player"))
            {
                Destroy(gameObject);
            }
        }
    }
}
