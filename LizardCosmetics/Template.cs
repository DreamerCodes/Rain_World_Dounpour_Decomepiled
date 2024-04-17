using UnityEngine;

namespace LizardCosmetics;

public abstract class Template
{
	public class SpritesOverlap : ExtEnum<SpritesOverlap>
	{
		public static readonly SpritesOverlap Behind = new SpritesOverlap("Behind", register: true);

		public static readonly SpritesOverlap BehindHead = new SpritesOverlap("BehindHead", register: true);

		public static readonly SpritesOverlap InFront = new SpritesOverlap("InFront", register: true);

		public SpritesOverlap(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public LizardGraphics lGraphics;

	public int numberOfSprites;

	public int startSprite;

	public RoomPalette palette;

	public SpritesOverlap spritesOverlap;

	public Template(LizardGraphics lGraphics, int startSprite)
	{
		this.lGraphics = lGraphics;
		this.startSprite = startSprite;
	}

	public virtual void Update()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		this.palette = palette;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = startSprite; i < startSprite + numberOfSprites; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
