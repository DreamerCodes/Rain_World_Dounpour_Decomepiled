using UnityEngine;

namespace Menu;

public abstract class CreditsObject : PositionedMenuObject
{
	public float scroll;

	public float lastScroll;

	public EndCredits.Stage stage;

	public float slowDownZone = 30f;

	public float defaulScrollSpeed = 4f;

	public float slowDownScrollSpeed = 1f;

	public int age;

	public virtual float CurrentDefaultScrollSpeed => 4f;

	public virtual bool OutOfScreen => false;

	public virtual bool BeforeScreen => scroll <= -1000f;

	public CreditsObject(Menu menu, MenuObject owner, EndCredits.Stage stage, bool startFromBottom)
		: base(menu, owner, default(Vector2))
	{
		this.stage = stage;
	}

	public override void Update()
	{
		base.Update();
		lastScroll = scroll;
		scroll += (menu as EndCredits).scrollSpeed;
		scroll = Mathf.Max(scroll, -1000f);
		age++;
	}
}
