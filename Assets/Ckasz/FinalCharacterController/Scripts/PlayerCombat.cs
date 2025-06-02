using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Ckasz.FinalCharacterController
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Components")]
         

        [Header("Projectile Settings")]
        [SerializeField] private GameObject smallProjectilePrefab;
        [SerializeField] private Transform rightShootPoint;
        [SerializeField] private Transform leftShootPoint;
        [SerializeField] public float timeBetweenShots = 0.2f;

        private float shootTimer;
        private bool useRightHand = true;

        [Header("Camera Settings")]
        private Camera playerCamera;

        private PlayerLocomotionInput input;
        private PlayerState playerState;
        private Animator animator;

        private void Awake()
        {
            input = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();
            animator = GetComponent<Animator>();
            playerCamera = Camera.main;
        }

        private void Update()   
        {
            UpdateCombatState();
            HandleRangedAttack();
            HandleMeleedAttack();


        }

        private void UpdateCombatState()
        {

        }

        private void HandleRangedAttack()
        {
            // Solo puede disparar proyectiles peque�os si est� DESARMADO
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
            Transform shootPoint = useRightHand ? rightShootPoint : leftShootPoint;

            // Lanza un rayo desde el centro de la pantalla
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 shootDirection;

            Debug.DrawRay(playerCamera.transform.position, ray.direction * 100f, Color.red, 2f);

            // Si el rayo choca con algo, apunta ahí
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                shootDirection = (hit.point - shootPoint.position).normalized;
            }
            else
            {
                // Si no, dispara hacia adelante
                shootDirection = ray.direction;
            }

            // Instanciar proyectil con rotación hacia la dirección deseada
            GameObject projectile = Instantiate(smallProjectilePrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));

            Debug.Log("🔥 Proyectil instanciado en: " + shootPoint.position);


            // Aplicar velocidad al Rigidbody
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.linearVelocity = shootDirection * 20f;

            // Activar animación (cuando esté lista)
            animator.SetTrigger(useRightHand ? "shootRight" : "shootLeft");

            // Cambiar de mano
            useRightHand = !useRightHand;
        }


        private void HandleMeleedAttack()
        {

        }



    }

}
