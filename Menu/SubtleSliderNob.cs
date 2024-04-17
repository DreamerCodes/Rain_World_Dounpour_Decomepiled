using RWCustom;
using UnityEngine;

namespace Menu;

public class SubtleSliderNob : PositionedMenuObject
{
	public FSprite outerCircle;

	private float size;

	public float DrawSize(float timeStacker)
	{
		return size;
	}

	public override float DrawX(float timeStacker)
	{
		return base.DrawX(timeStacker) + ((owner as Slider).Vertical ? ((0f - DrawSize(timeStacker)) / 2f) : DrawSize(timeStacker));
	}

	public override float DrawY(float timeStacker)
	{
		return base.DrawY(timeStacker) + ((owner as Slider).Vertical ? (0f - DrawSize(timeStacker)) : (DrawSize(timeStacker) / 2f));
	}

	public override Vector2 DrawPos(float timeStacker)
	{
		return new Vector2(DrawX(timeStacker), DrawY(timeStacker));
	}

	public SubtleSliderNob(Menu menu, MenuObject owner, Vector2 pos)
		: base(menu, owner, pos)
	{
		outerCircle = new FSprite("Menu_Subtle_Slider_Nob");
		outerCircle.anchorX = 0f;
		outerCircle.anchorY = 0f;
		size = 10f;
		Container.AddChild(outerCircle);
	}

	public override void Update()
	{
		base.Update();
		if ((owner as Slider).buttonBehav.greyedOut)
		{
			size = Custom.LerpAndTick(size, 0f, 0.08f, 1f / 3f);
		}
		else
		{
			size = Custom.LerpAndTick(size, 10f, 0.08f, 1f / 3f);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		outerCircle.x = DrawX(timeStacker) + 0.01f;
		outerCircle.y = DrawY(timeStacker) + 0.01f;
		outerCircle.scale = size / 10f;
		outerCircle.color = (owner as Slider).MyColor(timeStacker);
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		outerCircle.RemoveFromContainer();
	}
}
