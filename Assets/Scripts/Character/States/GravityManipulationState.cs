using UnityEngine;

public class GravityManipulationState : ICharacterState
{
    private Vector3 overrideDirection;
    private Vector3 oldDirection;

    public void EnterState(Character character)
    {
        character.ToggleHologram(true);
        overrideDirection = character.GravityDirection;
        // Activating the hologram for gravity adjustments.
    }

    public void UpdateState(Character character)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            character.SetState(new JumpingState());
            // Switching to Jump... let's hope for a smooth landing.
        }
        else
        {
            UpdateChangeGravity(character);
        }
    }

    public void ExitState(Character character)
    {
        character.ChangeGravityDirection(overrideDirection);
        character.ToggleHologram(false);
        
    }

    private void UpdateChangeGravity(Character character)
    {
        Vector3 gravityDirection = character.GravityDirection;

        // Adjust directions relative to the current gravity.
        Vector3 worldForward = Vector3.forward;
        Vector3 worldRight = Vector3.right;

        Vector3 adjustedForward = Quaternion.FromToRotation(Vector3.up, gravityDirection) * worldForward;
        Vector3 adjustedRight = Quaternion.FromToRotation(Vector3.up, gravityDirection) * worldRight;

        // Modify gravity based on player's arrow input.
        if (Input.GetKey(KeyCode.UpArrow))
        {
            overrideDirection = adjustedForward;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            overrideDirection = -adjustedForward;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            overrideDirection = -adjustedRight;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            overrideDirection = adjustedRight;
        }

        // Rotate hologram only if the direction changed.
        if (overrideDirection != oldDirection)
        {
            Quaternion targetRotation = Quaternion.LookRotation(overrideDirection, gravityDirection);
            character.RotateHoloToTarget(targetRotation);
            oldDirection = overrideDirection;
        }
    }
}
