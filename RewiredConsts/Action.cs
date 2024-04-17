using Rewired.Dev;

namespace RewiredConsts;

public static class Action
{
	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Jump")]
	public const int Jump = 0;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MoveHorizontal")]
	public const int MoveHorizontal = 1;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "MoveVertical")]
	public const int MoveVertical = 2;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Take")]
	public const int Take = 3;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Throw")]
	public const int Throw = 4;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Pause")]
	public const int Pause = 5;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Map")]
	public const int Map = 11;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UIHorizontal")]
	public const int UIHorizontal = 6;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UIVertical")]
	public const int UIVertical = 7;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UISubmit")]
	public const int UISubmit = 8;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UICancel")]
	public const int UICancel = 9;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UIAlternate")]
	public const int UIAlternate = 10;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UICheatHoldLeft")]
	public const int UICheatHoldLeft = 12;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UICheatHoldRight")]
	public const int UICheatHoldRight = 13;
}
