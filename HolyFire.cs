using RWCustom;
using UnityEngine;

public class HolyFire : LightFixture
{
	public class HolyFireSprite : CosmeticSprite
	{
		public float lifeTime;

		public float life;

		public float lastLife;

		public HolyFireSprite(Vector2 pos)
		{
			base.pos = pos;
			lastPos = pos;
			vel = Custom.RNV() * 1.5f * Random.value;
			life = 1f;
			lifeTime = Mathf.Lerp(10f, 40f, Random.value);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			vel *= 0.8f;
			vel.y += 0.4f;
			vel += Custom.RNV() * Random.value * 0.5f;
			lastLife = life;
			life -= 1f / lifeTime;
			if (life < 0f)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("deerEyeB");
			AddToContainer(sLeaser, rCam, null);
			base.InitiateSprites(sLeaser, rCam);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			sLeaser.sprites[0].scale = num;
			sLeaser.sprites[0].color = Custom.HSL2RGB(Mathf.Lerp(0.01f, 0.08f, num), 1f, Mathf.Lerp(0.5f, 1f, Mathf.Pow(num, 3f)));
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	private LightSource[] lightSources;

	private Vector2[] getToPositions;

	private float[] getToRads;

	private LightSource flatLightSource;

	public HolyFire(Room placedInRoom, PlacedObject placedObject, PlacedObject.LightFixtureData lightData)
		: base(placedInRoom, placedObject, lightData)
	{
		lightSources = new LightSource[3];
		getToPositions = new Vector2[lightSources.Length];
		getToRads = new float[lightSources.Length];
		for (int i = 0; i < lightSources.Length; i++)
		{
			lightSources[i] = new LightSource(placedObject.pos, environmentalLight: false, Custom.HSL2RGB(Mathf.Lerp(0.01f, 0.07f, (float)i / (float)(lightSources.Length - 1)), 1f, 0.5f), this);
			placedInRoom.AddObject(lightSources[i]);
			lightSources[i].setAlpha = 1f;
		}
		flatLightSource = new LightSource(placedObject.pos, environmentalLight: false, new Color(1f, 1f, 1f), this);
		flatLightSource.flat = true;
		placedInRoom.AddObject(flatLightSource);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < lightSources.Length; i++)
		{
			if (Random.value < 0.2f)
			{
				getToPositions[i] = Custom.RNV() * 50f * Random.value;
			}
			if (Random.value < 0.2f)
			{
				getToRads[i] = Mathf.Lerp(50f, Mathf.Lerp(400f, 200f, (float)i / (float)(lightSources.Length - 1)), Mathf.Pow(Random.value, 0.5f));
			}
			lightSources[i].setPos = Vector2.Lerp(lightSources[i].Pos, placedObject.pos + getToPositions[i], 0.2f);
			lightSources[i].setRad = Mathf.Lerp(lightSources[i].Rad, getToRads[i], 0.2f);
		}
		room.AddObject(new HolyFireSprite(placedObject.pos));
		flatLightSource.setAlpha = Mathf.Lerp(0.2f, 0.4f, Random.value);
		flatLightSource.setRad = Mathf.Lerp(24f, 33f, Random.value);
		flatLightSource.setPos = placedObject.pos;
	}
}
