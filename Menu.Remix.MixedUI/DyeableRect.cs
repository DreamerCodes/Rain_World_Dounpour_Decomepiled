using UnityEngine;

namespace Menu.Remix.MixedUI;

public class DyeableRect
{
	public enum HiddenSide
	{
		None,
		Left,
		Right,
		Top,
		Bottom
	}

	public readonly FSprite[] sprites;

	protected readonly FContainer container;

	protected internal const int MainFillSprite = 8;

	public Color colorEdge;

	public Color colorFill;

	public Vector2 pos;

	public Vector2 size;

	private readonly bool _filled;

	public Vector2 addSize;

	private Vector2 _lastAddSize;

	public HiddenSide hiddenSide;

	public float fillAlpha;

	private float _lastFillAlpha;

	public bool isHidden { get; private set; }

	public DyeableRect(FContainer container, Vector2 pos, Vector2 size, bool filled = true)
	{
		isHidden = false;
		this.container = new FContainer();
		container.AddChild(this.container);
		this.pos = pos;
		this.size = size;
		this.container.x = this.pos.x;
		this.container.y = this.pos.y;
		colorEdge = MenuColorEffect.rgbMediumGrey;
		colorFill = MenuColorEffect.rgbBlack;
		_filled = filled;
		sprites = new FSprite[(!filled) ? 8 : 17];
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
			sprites[8] = new FSprite("pixel");
			sprites[8].anchorY = 0f;
			sprites[8].anchorX = 0f;
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
			this.container.AddChild(sprites[k]);
		}
	}

	protected internal int SideSprite(int side)
	{
		return (_filled ? 9 : 0) + side;
	}

	protected internal int CornerSprite(int corner)
	{
		return (_filled ? 9 : 0) + 4 + corner;
	}

	protected internal int FillSideSprite(int side)
	{
		return side;
	}

	protected internal int FillCornerSprite(int corner)
	{
		return 4 + corner;
	}

	public void Update()
	{
		if (!isHidden)
		{
			_lastFillAlpha = fillAlpha;
			_lastAddSize = addSize;
			container.x = pos.x;
			container.y = pos.y;
		}
	}

	private int[] SideSprites()
	{
		switch (hiddenSide)
		{
		case HiddenSide.Left:
			if (_filled)
			{
				return new int[6] { 0, 4, 5, 9, 13, 14 };
			}
			return new int[3] { 0, 4, 5 };
		case HiddenSide.Right:
			if (_filled)
			{
				return new int[6] { 2, 6, 7, 11, 15, 16 };
			}
			return new int[3] { 2, 6, 7 };
		case HiddenSide.Top:
			if (_filled)
			{
				return new int[6] { 1, 5, 6, 10, 14, 15 };
			}
			return new int[3] { 1, 5, 6 };
		case HiddenSide.Bottom:
			if (_filled)
			{
				return new int[6] { 3, 4, 7, 12, 13, 16 };
			}
			return new int[3] { 3, 4, 7 };
		default:
			return new int[0];
		}
	}

	public void GrafUpdate(float timeStacker)
	{
		if (isHidden)
		{
			return;
		}
		container.x = pos.x;
		container.y = pos.y;
		Vector2 vector = Vector2.Lerp(_lastAddSize, addSize, timeStacker) * -0.5f;
		Vector2 vector2 = size + Vector2.Lerp(_lastAddSize, addSize, timeStacker);
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
		for (int i = 0; i < 4; i++)
		{
			sprites[SideSprite(i)].color = colorEdge;
			sprites[CornerSprite(i)].color = colorEdge;
		}
		if (_filled)
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
			sprites[8].x = vector.x + 7f;
			sprites[8].y = vector.y + 7f;
			sprites[8].scaleX = vector2.x - 14f;
			sprites[8].scaleY = vector2.y - 14f;
			for (int j = 0; j < 9; j++)
			{
				sprites[j].alpha = Mathf.Lerp(_lastFillAlpha, fillAlpha, timeStacker);
			}
			sprites[8].color = colorFill;
			for (int k = 0; k < 4; k++)
			{
				sprites[FillSideSprite(k)].color = colorFill;
				sprites[FillCornerSprite(k)].color = colorFill;
			}
		}
		if (hiddenSide != 0)
		{
			int[] array = SideSprites();
			for (int l = 0; l < array.Length; l++)
			{
				sprites[array[l]].isVisible = false;
			}
		}
	}

	public void Hide()
	{
		if (!isHidden)
		{
			container.isVisible = false;
			isHidden = true;
		}
	}

	public void Show()
	{
		if (isHidden)
		{
			container.isVisible = true;
			isHidden = false;
		}
	}
}
