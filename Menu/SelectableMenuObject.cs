namespace Menu;

public interface SelectableMenuObject
{
	bool IsMouseOverMe { get; }

	bool CurrentlySelectableMouse { get; }

	bool CurrentlySelectableNonMouse { get; }
}
