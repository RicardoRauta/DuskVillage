namespace DuskVillage.Input;

public interface IInputService
{
    InputSnapshot Current { get; }

    void Update();
}
