using UnityEngine;

public class WalkingState : ICharacterState
{
    public readonly string STATE_NAME = "Walk";
    public void EnterState(Character character)
    {
        character.Animator.Play(STATE_NAME);
        Debug.Log($"Entering {STATE_NAME} State");
    }
    public void UpdateState(Character character)
    {
        if (!character.IsMoving)
        {
            character.SetState(new IdleState());
        }
        if (Input.GetKeyDown(KeyCode.Space) && character.IsGrounded)
        {
            character.SetState(new JumpingState());
        }
    }

    public void ExitState(Character character) { }
}
