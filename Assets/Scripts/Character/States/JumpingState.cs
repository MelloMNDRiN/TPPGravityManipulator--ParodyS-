using UnityEngine;

public class JumpingState : ICharacterState
{
    public readonly string STATE_NAME = "Jump";
    public void EnterState(Character character)
    {
        character.Animator.Play(STATE_NAME);
        Debug.Log($"Entering {STATE_NAME} State");
    }

    public void UpdateState(Character character)
    {
      
    }

    public void ExitState(Character character)
    {
       
    }
}
