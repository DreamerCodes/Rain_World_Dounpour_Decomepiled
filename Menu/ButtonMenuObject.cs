namespace Menu;

public interface ButtonMenuObject
{
	ButtonBehavior GetButtonBehavior { get; }

	void Clicked();
}
