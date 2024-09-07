using UnityEngine;

public class DanceState : ICharacterState
{
    public readonly string STATE_NAME = "Dance";
    public void EnterState(Character character)
    {
        ///IDK WHY
        character.Animator.Play(STATE_NAME);
        Debug.Log($"Entering {STATE_NAME} State");
    }

    public void UpdateState(Character character)
    {

    }
    public void ExitState(Character character) { }
}
