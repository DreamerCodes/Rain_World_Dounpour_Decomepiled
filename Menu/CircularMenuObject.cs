using RWCustom;
using UnityEngine;

namespace Menu;

public abstract class CircularMenuObject : PositionedMenuObject
{
	public float rad;

	public float lastRad;

	public bool MouseOver => Custom.DistLess(base.ScreenPos, menu.mousePosition, rad);

	public float DrawRad(float timeStacker)
	{
		return Mathf.Lerp(lastRad, rad, timeStacker);
	}

	public CircularMenuObject(Menu menu, MenuObject owner, Vector2 pos, float rad)
		: base(menu, owner, pos)
	{
		this.rad = rad;
	}

	public override void Update()
	{
		base.Update();
		lastRad = rad;
	}
}
