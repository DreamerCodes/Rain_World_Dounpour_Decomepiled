using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpContainer : UIelement
{
	public FContainer container => myContainer;

	public OpContainer(Vector2 pos)
		: base(pos, Vector2.one)
	{
	}
}
