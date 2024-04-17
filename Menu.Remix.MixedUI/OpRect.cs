using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpRect : UIelement
{
	public readonly DyeableRect rect;

	public bool doesBump;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	public float fillAlpha;

	public BumpBehaviour bumpBehav;

	public OpRect(Vector2 pos, Vector2 size, float alpha = 0.3f)
		: base(pos, size)
	{
		fillAlpha = alpha;
		doesBump = false;
		rect = new DyeableRect(myContainer, Vector2.zero, size)
		{
			colorEdge = colorEdge,
			colorFill = colorFill,
			fillAlpha = fillAlpha
		};
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		rect.colorFill = colorFill;
		if (bumpBehav == null)
		{
			rect.fillAlpha = fillAlpha;
			rect.colorEdge = colorEdge;
			rect.GrafUpdate(timeStacker);
		}
		else
		{
			rect.fillAlpha = bumpBehav.FillAlpha;
			rect.addSize = new Vector2(4f, 4f) * bumpBehav.AddSize;
			rect.colorEdge = bumpBehav.GetColor(colorEdge);
			rect.GrafUpdate(timeStacker);
		}
	}

	public override void Update()
	{
		base.Update();
		rect.Update();
		if (doesBump)
		{
			if (bumpBehav == null)
			{
				bumpBehav = new BumpBehaviour(this);
			}
			if (bumpBehav.owner == this)
			{
				bumpBehav.Update();
			}
		}
	}

	protected internal override void Change()
	{
		base.Change();
		rect.size = base.size;
	}
}
