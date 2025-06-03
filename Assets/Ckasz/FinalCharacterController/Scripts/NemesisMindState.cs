using UnityEngine;

namespace Ckasz.FinalCharacterController
{
    public enum NemesisMentalState
    {
        Passive = 0,
        Neutral1 = 1,
        Neutral2 = 2,
        Stalker = 3
    }

    public class NemesisMindState : MonoBehaviour
    {
        [Header("Timers")]
        public float timeInPeripheralToNeutral2 = 3f;
        public float timeWithoutPlayerToPassive = 5f;
        public float stalkerCooldownTime = 10f;

        [Header("State Debug")]
        [field: SerializeField] public NemesisMentalState CurrentMentalState { get; private set; } = NemesisMentalState.Passive;
        public int damageCount = 0;

        private float peripheralTimer = 0f;
        private float lostPlayerTimer = 0f;
        private float stalkerTimer = 0f;

        private bool playerInPeripheral;
        private bool playerInFrontal;

        private void Update()
        {
            switch (CurrentMentalState)
            {
                case NemesisMentalState.Passive:
                    if (playerInPeripheral)
                        CurrentMentalState = NemesisMentalState.Neutral1;
                    break;

                case NemesisMentalState.Neutral1:
                    if (playerInPeripheral)
                    {
                        peripheralTimer += Time.deltaTime;
                        if (peripheralTimer >= timeInPeripheralToNeutral2)
                        {
                            CurrentMentalState = NemesisMentalState.Neutral2;
                            peripheralTimer = 0f;
                        }
                    }
                    else
                    {
                        peripheralTimer = 0f;
                        CurrentMentalState = NemesisMentalState.Passive;
                    }
                    break;

                case NemesisMentalState.Neutral2:
                    if (!playerInPeripheral && !playerInFrontal)
                    {
                        lostPlayerTimer += Time.deltaTime;
                        if (lostPlayerTimer >= timeWithoutPlayerToPassive)
                        {
                            CurrentMentalState = NemesisMentalState.Passive;
                            lostPlayerTimer = 0f;
                        }
                    }
                    else
                    {
                        lostPlayerTimer = 0f;
                    }

                    if (damageCount >= 3)
                    {
                        CurrentMentalState = NemesisMentalState.Stalker;
                        stalkerTimer = 0f;
                    }
                    break;

                case NemesisMentalState.Stalker:
                    stalkerTimer += Time.deltaTime;
                    if (stalkerTimer >= stalkerCooldownTime)
                    {
                        damageCount = 0;
                        CurrentMentalState = NemesisMentalState.Neutral2;
                    }
                    break;
            }
        }

        public void SetVision(bool inPeripheral, bool inFrontal)
        {
            playerInPeripheral = inPeripheral;
            playerInFrontal = inFrontal;
        }

        public void RegisterHit()
        {
            damageCount++;
            Debug.Log(" Nemesis recibió daño. Total: " + damageCount);
        }
    }
}
