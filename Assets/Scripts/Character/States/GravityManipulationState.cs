using UnityEngine;
using UnityEngine.TextCore.Text;

public class GravityManipulationState : ICharacterState
{
    private bool AppliedGravity = false;
    private Vector3 overrideDirection;
    private Vector3 oldDirection;

    public void EnterState(Character character)
    {
        character.ToggleHologram(true);
        overrideDirection = character.GravityDirection;
    }

    public void UpdateState(Character character)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            character.SetState(new IdleState());
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

        if (Input.GetKey(KeyCode.UpArrow))
        {
            overrideDirection = Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            overrideDirection = Vector3.back;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            overrideDirection = Vector3.left;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            overrideDirection = Vector3.right;
        }

        if (overrideDirection != oldDirection)
        {
          
            character.RotateHoloToTarget(Quaternion.LookRotation(overrideDirection, Vector3.up));
            oldDirection = overrideDirection;
        }
    }

}
