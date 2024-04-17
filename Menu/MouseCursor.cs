using RWCustom;
using UnityEngine;

namespace Menu;

public class MouseCursor : PositionedMenuObject
{
	private FSprite cursorSprite;

	private FSprite shadow;

	private float fade;

	private float lastFade;

	public MouseCursor(Menu menu, MenuObject owner, Vector2 pos)
		: base(menu, owner, pos)
	{
		myContainer = menu.cursorContainer;
		shadow = new FSprite("Futile_White");
		shadow.shader = menu.manager.rainWorld.Shaders["FlatLight"];
		shadow.color = new Color(0f, 0f, 0f);
		shadow.scale = 4f;
		Container.AddChild(shadow);
		cursorSprite = new FSprite("Cursor");
		cursorSprite.anchorX = 0f;
		cursorSprite.anchorY = 1f;
		Container.AddChild(cursorSprite);
	}

	public override void Update()
	{
		base.Update();
		pos = Futile.mousePosition;
		lastFade = fade;
		fade = Custom.LerpAndTick(fade, (menu.ShowCursor && menu.currentPage == (owner as Page).index) ? 1f : 0f, 0.01f, 1f / 30f);
		if ((menu.manager.rainWorld.setup.devToolsActive || ModManager.DevTools) && Input.GetMouseButton(1))
		{
			fade = 0f;
		}
	}

	public void BumToFront()
	{
		menu.cursorContainer.RemoveFromContainer();
		Futile.stage.AddChild(menu.cursorContainer);
		shadow.RemoveFromContainer();
		Container.AddChild(shadow);
		cursorSprite.RemoveFromContainer();
		Container.AddChild(cursorSprite);
	}

	public override void GrafUpdate(float timeStacker)
	{
		cursorSprite.x = Futile.mousePosition.x + 0.01f;
		cursorSprite.y = Futile.mousePosition.y + 0.01f;
		shadow.x = Futile.mousePosition.x + 3.01f;
		shadow.y = Futile.mousePosition.y - 8.01f;
		float num = Custom.SCurve(Mathf.Lerp(lastFade, fade, timeStacker), 0.6f);
		cursorSprite.alpha = num;
		shadow.alpha = Mathf.Pow(num, 3f) * 0.3f;
		base.GrafUpdate(timeStacker);
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		shadow.RemoveFromContainer();
		cursorSprite.RemoveFromContainer();
	}
}
