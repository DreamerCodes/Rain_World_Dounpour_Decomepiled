using RWCustom;
using UnityEngine;

public class StationaryEffect : CosmeticSprite
{
	public class EffectType : ExtEnum<EffectType>
	{
		public static readonly EffectType FlashingOrb = new EffectType("FlashingOrb", register: true);

		public static readonly EffectType Line = new EffectType("Line", register: true);

		public EffectType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public float life;

	private float lastLife;

	public int lifeTime;

	public Color color;

	public LizardGraphics lizard;

	public EffectType type;

	public StationaryEffect(Vector2 pos, Color color, LizardGraphics lizard, EffectType type)
	{
		life = 1f;
		lastLife = 1f;
		base.pos = pos;
		lastPos = pos;
		this.lizard = lizard;
		this.type = type;
		if (type == EffectType.FlashingOrb)
		{
			lifeTime = 4;
		}
		else if (type == EffectType.Line)
		{
			lifeTime = 4;
		}
	}

	public override void Update(bool eu)
	{
		lastLife = life;
		life -= 1f / (float)lifeTime;
		if (type == EffectType.FlashingOrb && lizard != null && lizard.whiteFlicker < 0)
		{
			life = 0f;
		}
		if (life <= 0f)
		{
			Destroy();
		}
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (type == EffectType.FlashingOrb)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Circle20");
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
		if (type == EffectType.FlashingOrb)
		{
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			vector += Custom.DegToVec(Random.value * 360f) * Random.value * 10f * num;
			sLeaser.sprites[0].scale = Mathf.Pow(Random.value * num, 0.1f);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			if (lizard != null)
			{
				sLeaser.sprites[0].color = lizard.HeadColor(timeStacker);
			}
			else
			{
				sLeaser.sprites[0].color = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), new Color(1f, 1f, 1f), Random.value);
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite fSprite in sprites)
		{
			fSprite.RemoveFromContainer();
			newContatiner.AddChild(fSprite);
		}
	}
}
