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
            character.SetState(new JumpingState());
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


        Vector3 worldForward = Vector3.forward;
        Vector3 worldRight = Vector3.right;


        Vector3 adjustedForward = Quaternion.FromToRotation(Vector3.up, gravityDirection) * worldForward;
        Vector3 adjustedRight = Quaternion.FromToRotation(Vector3.up, gravityDirection) * worldRight;

        
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
        if (overrideDirection != oldDirection)
        {
            Quaternion targetRotation = Quaternion.LookRotation(overrideDirection, gravityDirection);
            character.RotateHoloToTarget(targetRotation);
            oldDirection = overrideDirection;
        }
    }







}
