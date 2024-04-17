using UnityEngine;

public class NeuronSpark : CosmeticSprite
{
	private float lastLife;

	private float life;

	private float lifeTime;

	public NeuronSpark(Vector2 pos)
	{
		base.pos = pos;
		lastPos = pos;
		lifeTime = Mathf.Lerp(1f, 4f, Random.value);
		life = 1f;
		lastLife = 1f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (life == 1f)
		{
			room.PlaySound(SoundID.SS_Mycelia_Spark, pos, 1f, 1f);
		}
		lastLife = life;
		life -= 1f / lifeTime;
		if (lastLife < 0f)
		{
			Destroy();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].color = new Color(0f, 0f, 1f);
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
		sLeaser.sprites[1] = new FSprite("pixel");
		sLeaser.sprites[1].rotation = 45f;
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[1].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[1].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		float num = Mathf.Lerp(lastLife, life, timeStacker);
		float num2 = Mathf.Pow(num, 3f) * Random.value;
		sLeaser.sprites[1].color = new Color(num2, num2, 1f);
		sLeaser.sprites[1].scale = 4f * Random.value * num;
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].alpha = Random.value * num;
		sLeaser.sprites[0].scale = (15f * Random.value + 15f * Mathf.Pow(Random.value, 3f)) * num / 10f;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
