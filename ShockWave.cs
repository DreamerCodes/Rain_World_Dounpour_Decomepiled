using UnityEngine;

public class ShockWave : CosmeticSprite
{
	private float size;

	private float life;

	private float lastLife;

	private int lifeTime;

	private float intensity;

	public bool highLayer;

	public ShockWave(Vector2 pos, float size, float intensity, int lifeTime, bool highLayer = false)
	{
		base.pos = pos;
		lastPos = pos;
		this.size = size;
		this.intensity = intensity;
		this.lifeTime = lifeTime;
		this.highLayer = highLayer;
		life = 0f;
		lastLife = 0f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastLife = life;
		life += 1f / (float)lifeTime;
		if (lastLife > 1f)
		{
			Destroy();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["ShockWave"];
		sLeaser.sprites[0].color = new Color(0f, 0.5f, 1f);
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		float num = Mathf.Min(1f, Mathf.Lerp(lastLife, life, timeStacker));
		sLeaser.sprites[0].color = new Color(Mathf.Pow(num, 0.1f), intensity, num);
		sLeaser.sprites[0].scale = Mathf.Pow(num, 0.5f) * size / 8f;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer(highLayer ? "HUD2" : "HUD");
		}
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
