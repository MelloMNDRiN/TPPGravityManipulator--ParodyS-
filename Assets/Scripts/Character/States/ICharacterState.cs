public interface ICharacterState
{
    void EnterState(Character character);
    void UpdateState(Character character);
    void ExitState(Character character);
}