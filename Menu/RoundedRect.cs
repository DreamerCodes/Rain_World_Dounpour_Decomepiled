using UnityEngine;

namespace Menu;

public class RoundedRect : RectangularMenuObject
{
	public Vector2 addSize;

	public Vector2 lastAddSize;

	public float fillAlpha;

	private float lasFillAplha;

	public FSprite[] sprites;

	private bool filled;

	public HSLColor? borderColor;

	private int MainFillSprite => 8;

	private int SideSprite(int side)
	{
		return (filled ? 9 : 0) + side;
	}

	private int CornerSprite(int corner)
	{
		return (filled ? 9 : 0) + 4 + corner;
	}

	private int FillSideSprite(int side)
	{
		return side;
	}

	private int FillCornerSprite(int corner)
	{
		return 4 + corner;
	}

	public RoundedRect(Menu menu, MenuObject owner, Vector2 pos, Vector2 size, bool filled)
		: base(menu, owner, pos, size)
	{
		this.filled = filled;
		sprites = new FSprite[filled ? 17 : 8];
		for (int i = 0; i < 4; i++)
		{
			sprites[SideSprite(i)] = new FSprite("pixel");
			sprites[SideSprite(i)].scaleY = 2f;
			sprites[SideSprite(i)].scaleX = 2f;
			sprites[CornerSprite(i)] = new FSprite("UIroundedCorner");
			if (filled)
			{
				sprites[FillSideSprite(i)] = new FSprite("pixel");
				sprites[FillSideSprite(i)].scaleY = 6f;
				sprites[FillSideSprite(i)].scaleX = 6f;
				sprites[FillCornerSprite(i)] = new FSprite("UIroundedCornerInside");
			}
		}
		sprites[SideSprite(0)].anchorY = 0f;
		sprites[SideSprite(2)].anchorY = 0f;
		sprites[SideSprite(1)].anchorX = 0f;
		sprites[SideSprite(3)].anchorX = 0f;
		sprites[CornerSprite(0)].scaleY = -1f;
		sprites[CornerSprite(2)].scaleX = -1f;
		sprites[CornerSprite(3)].scaleY = -1f;
		sprites[CornerSprite(3)].scaleX = -1f;
		if (filled)
		{
			sprites[MainFillSprite] = new FSprite("pixel");
			sprites[MainFillSprite].anchorY = 0f;
			sprites[MainFillSprite].anchorX = 0f;
			sprites[FillSideSprite(0)].anchorY = 0f;
			sprites[FillSideSprite(2)].anchorY = 0f;
			sprites[FillSideSprite(1)].anchorX = 0f;
			sprites[FillSideSprite(3)].anchorX = 0f;
			sprites[FillCornerSprite(0)].scaleY = -1f;
			sprites[FillCornerSprite(2)].scaleX = -1f;
			sprites[FillCornerSprite(3)].scaleY = -1f;
			sprites[FillCornerSprite(3)].scaleX = -1f;
			for (int j = 0; j < 9; j++)
			{
				sprites[j].color = new Color(0f, 0f, 0f);
				sprites[j].alpha = 0.75f;
			}
		}
		for (int k = 0; k < sprites.Length; k++)
		{
			Container.AddChild(sprites[k]);
		}
		try
		{
			Update();
			GrafUpdate(0f);
		}
		catch
		{
		}
	}

	public override void Update()
	{
		base.Update();
		lasFillAplha = fillAlpha;
		lastAddSize = addSize;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		Vector2 vector = DrawPos(timeStacker);
		Vector2 vector2 = DrawSize(timeStacker);
		vector -= Vector2.Lerp(lastAddSize, addSize, timeStacker) / 2f;
		vector2 += Vector2.Lerp(lastAddSize, addSize, timeStacker);
		vector.x = Mathf.Floor(vector.x) + 0.41f;
		vector.y = Mathf.Floor(vector.y) + 0.41f;
		sprites[SideSprite(0)].x = vector.x + 1f;
		sprites[SideSprite(0)].y = vector.y + 7f;
		sprites[SideSprite(0)].scaleY = vector2.y - 14f;
		sprites[SideSprite(1)].x = vector.x + 7f;
		sprites[SideSprite(1)].y = vector.y + vector2.y - 1f;
		sprites[SideSprite(1)].scaleX = vector2.x - 14f;
		sprites[SideSprite(2)].x = vector.x + vector2.x - 1f;
		sprites[SideSprite(2)].y = vector.y + 7f;
		sprites[SideSprite(2)].scaleY = vector2.y - 14f;
		sprites[SideSprite(3)].x = vector.x + 7f;
		sprites[SideSprite(3)].y = vector.y + 1f;
		sprites[SideSprite(3)].scaleX = vector2.x - 14f;
		sprites[CornerSprite(0)].x = vector.x + 3.5f;
		sprites[CornerSprite(0)].y = vector.y + 3.5f;
		sprites[CornerSprite(1)].x = vector.x + 3.5f;
		sprites[CornerSprite(1)].y = vector.y + vector2.y - 3.5f;
		sprites[CornerSprite(2)].x = vector.x + vector2.x - 3.5f;
		sprites[CornerSprite(2)].y = vector.y + vector2.y - 3.5f;
		sprites[CornerSprite(3)].x = vector.x + vector2.x - 3.5f;
		sprites[CornerSprite(3)].y = vector.y + 3.5f;
		Color color = new Color(1f, 1f, 1f);
		if (borderColor.HasValue)
		{
			color = borderColor.Value.rgb;
		}
		if (owner is ButtonTemplate)
		{
			color = ((!borderColor.HasValue) ? (owner as ButtonTemplate).MyColor(timeStacker) : (owner as ButtonTemplate).InterpColor(timeStacker, borderColor.Value));
		}
		for (int i = 0; i < 4; i++)
		{
			sprites[SideSprite(i)].color = color;
			sprites[CornerSprite(i)].color = color;
		}
		if (filled)
		{
			sprites[FillSideSprite(0)].x = vector.x + 4f;
			sprites[FillSideSprite(0)].y = vector.y + 7f;
			sprites[FillSideSprite(0)].scaleY = vector2.y - 14f;
			sprites[FillSideSprite(1)].x = vector.x + 7f;
			sprites[FillSideSprite(1)].y = vector.y + vector2.y - 4f;
			sprites[FillSideSprite(1)].scaleX = vector2.x - 14f;
			sprites[FillSideSprite(2)].x = vector.x + vector2.x - 4f;
			sprites[FillSideSprite(2)].y = vector.y + 7f;
			sprites[FillSideSprite(2)].scaleY = vector2.y - 14f;
			sprites[FillSideSprite(3)].x = vector.x + 7f;
			sprites[FillSideSprite(3)].y = vector.y + 4f;
			sprites[FillSideSprite(3)].scaleX = vector2.x - 14f;
			sprites[FillCornerSprite(0)].x = vector.x + 3.5f;
			sprites[FillCornerSprite(0)].y = vector.y + 3.5f;
			sprites[FillCornerSprite(1)].x = vector.x + 3.5f;
			sprites[FillCornerSprite(1)].y = vector.y + vector2.y - 3.5f;
			sprites[FillCornerSprite(2)].x = vector.x + vector2.x - 3.5f;
			sprites[FillCornerSprite(2)].y = vector.y + vector2.y - 3.5f;
			sprites[FillCornerSprite(3)].x = vector.x + vector2.x - 3.5f;
			sprites[FillCornerSprite(3)].y = vector.y + 3.5f;
			sprites[MainFillSprite].x = vector.x + 7f;
			sprites[MainFillSprite].y = vector.y + 7f;
			sprites[MainFillSprite].scaleX = vector2.x - 14f;
			sprites[MainFillSprite].scaleY = vector2.y - 14f;
			for (int j = 0; j < 9; j++)
			{
				sprites[j].alpha = Mathf.Lerp(lasFillAplha, fillAlpha, timeStacker);
			}
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].RemoveFromContainer();
		}
	}
}
