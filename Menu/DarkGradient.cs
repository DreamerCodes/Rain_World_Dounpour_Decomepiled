using UnityEngine;

namespace Menu;

public class DarkGradient : RectangularMenuObject
{
	public FSprite sprite;

	public float alpha;

	public DarkGradient(Menu menu, MenuObject owner, Vector2 centerPos, float horizontalRad, float verticalRad, float alpha)
		: base(menu, owner, centerPos + new Vector2(0f - horizontalRad, 0f - verticalRad), new Vector2(horizontalRad * 2f, verticalRad * 2f))
	{
		this.alpha = alpha;
		sprite = new FSprite("Futile_White");
		sprite.scaleX = horizontalRad / 8f;
		sprite.scaleY = verticalRad / 8f;
		sprite.shader = menu.manager.rainWorld.Shaders["FlatLight"];
		sprite.color = new Color(0f, 0f, 0f);
		sprite.alpha = alpha;
		Container.AddChild(sprite);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		sprite.x = DrawX(timeStacker) + size.x / 2f;
		sprite.y = DrawY(timeStacker) + size.y / 2f;
		sprite.alpha = alpha;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		sprite.RemoveFromContainer();
	}
}
