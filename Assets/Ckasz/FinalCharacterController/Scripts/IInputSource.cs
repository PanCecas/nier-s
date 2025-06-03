using UnityEngine;

namespace Ckasz.FinalCharacterController
{
    public interface IInputSource
    {
        bool SprintToggledOn { get; }
        Vector2 MovementInput { get; }
        Vector2 LookInput { get; }
        bool JumpPressed { get; }
        bool RangedAttackStarted { get; }
        bool RangedAttackReleased { get; }
    }
}
