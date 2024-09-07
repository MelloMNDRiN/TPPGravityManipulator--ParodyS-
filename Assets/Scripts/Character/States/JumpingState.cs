using UnityEngine;

public class JumpingState : ICharacterState
{
    public readonly string STATE_NAME = "Jump";
    private bool IsJumping = false;
    public void EnterState(Character character)
    {

        character.Animator.Play(STATE_NAME);

        Vector3 jumpDirection = -character.GravityDirection.normalized;

        character.Rigidbody.AddForce(jumpDirection * character.JumpPower, ForceMode.Impulse);

        IsJumping = true;

        Debug.Log($"Entering {STATE_NAME} State");

    }

    public void UpdateState(Character character)
    {
        Vector3 gravityDirection = -character.GravityDirection.normalized;
        float verticalVelocity = Vector3.Dot(character.Rigidbody.velocity, gravityDirection);

        character.Move(1f);
        character.Rotate(200f);

        if (IsJumping)
        {
            
            if (!character.IsGrounded && verticalVelocity > 0)
            {
                IsJumping = false;
            }
        }
        else
        {
            if (character.IsGrounded && verticalVelocity <= 0)
            {
                character.SetState(new IdleState());
            }
        }
    }



    public void ExitState(Character character)
    {
       
    }
}
