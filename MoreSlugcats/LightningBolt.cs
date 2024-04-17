using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class LightningBolt : CosmeticSprite
{
	public float life;

	protected float lastLife;

	public float lifeTime;

	public Vector2 from;

	public Vector2 target;

	public int type;

	public float width;

	public float length;

	private float progress;

	public float intensity;

	public float lightningType;

	public float randomOffset;

	public float lightningParam;

	private bool light;

	public Color color;

	public float Length => Custom.Dist(from, target) * 0.075f;

	public Color Parameters => new Color(Mathf.Clamp(life, 0f, 1f) * intensity, lightningParam, lightningType);

	public LightningBolt(Vector2 from, Vector2 target, int type, float width)
	{
		this.from = from;
		this.target = target;
		this.type = type;
		this.width = width * 30f;
		Init();
	}

	public void Init()
	{
		lastLife = -1000f;
		life = 1f;
		randomOffset = Random.value;
		if (type != 0)
		{
			if (type == 1)
			{
				intensity = 1f;
				return;
			}
			Custom.LogWarning("Unknown Lightning Type");
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Foreground");
		}
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = target - camPos;
		Vector2 vector2 = from - camPos;
		Vector2 vector3 = (vector - vector2) * 0.5f + vector2;
		float num = Mathf.Lerp(width, width * Custom.LerpQuadEaseOut(0f, 1f, Length / 50f), Custom.LerpQuadEaseOut(1f, 0f, width));
		sLeaser.sprites[0].x = vector3.x;
		sLeaser.sprites[0].y = vector3.y;
		sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(from, target);
		sLeaser.sprites[0].scaleY = Length * 1.03f;
		sLeaser.sprites[0].scaleX = num;
		sLeaser.sprites[0].color = Parameters;
		sLeaser.sprites[0].alpha = randomOffset;
		sLeaser.sprites[1].rotation = Custom.AimFromOneVectorToAnother(from, target);
		sLeaser.sprites[1].x = vector3.x;
		sLeaser.sprites[1].y = vector3.y;
		sLeaser.sprites[1].scaleY = Length * (1f + intensity);
		sLeaser.sprites[1].scaleX = (num + Length * 0.3f) * (1f + intensity);
		sLeaser.sprites[1].color = Custom.HSL2RGB(lightningType - 0.0001f, 1f, 0.6f);
		sLeaser.sprites[1].alpha = ((light && width > 0f) ? Parameters.r : 0f);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["LightningBolt"];
		sLeaser.sprites[0].color = Parameters;
		sLeaser.sprites[0].x = pos.x;
		sLeaser.sprites[0].y = pos.y;
		sLeaser.sprites[0].scaleX = width;
		sLeaser.sprites[0].scaleY = length;
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[1].x = pos.x;
		sLeaser.sprites[1].y = pos.y;
		sLeaser.sprites[1].scale = 10f;
		sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["LightSource"];
		sLeaser.sprites[1].color = color;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastLife = life;
		if (type == 0)
		{
			life -= 1f / lifeTime;
			if (lastLife <= 0f)
			{
				Destroy();
			}
		}
	}

	public LightningBolt(Vector2 from, Vector2 target, int type, float width, float lifeTime)
		: this(from, target, type, width)
	{
		this.lifeTime = lifeTime * 30f;
	}

	public LightningBolt(Vector2 from, Vector2 target, int type, float width, float lifeTime, float lightningParam, float lightningType)
		: this(from, target, type, width, lifeTime)
	{
		this.lightningParam = lightningParam;
		this.lightningType = lightningType;
	}

	public LightningBolt(Vector2 from, Vector2 target, int type, float width, float lifeTime, float lightningParam, float lightningType, bool light)
		: this(from, target, type, width, lifeTime, lightningParam, lightningType)
	{
		this.light = light;
		color = Custom.HSL2RGB(this.lightningType - 0.0001f, 1f, 0.6f);
	}
}
