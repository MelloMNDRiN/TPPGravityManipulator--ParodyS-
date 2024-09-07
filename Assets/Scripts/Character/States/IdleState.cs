using UnityEngine;

public class IdleState : ICharacterState
{
    public readonly string STATE_NAME = "Idle";
    public void EnterState(Character character)
    {
        character.Animator.Play(STATE_NAME);
        Debug.Log($"Entering {STATE_NAME} State");
    }

    public void UpdateState(Character character)
    {

        if (Input.GetKeyDown(KeyCode.Space) && character.IsGrounded)
        {
            character.SetState(new JumpingState());
        }
    }
    public void ExitState(Character character) { }
}