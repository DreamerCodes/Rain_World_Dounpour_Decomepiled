using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SlugcatGhost : UpdatableAndDeletable, IDrawable
{
	public Vector2[,,] floatyVecs;

	public float[,] lightFluctuations;

	public float lightBall;

	public float lastLightBall;

	public float dissapate;

	public float lastDissapate;

	public bool remove;

	private float flipX;

	private Vector2 pos;

	public int counter;

	private int soundsStatus;

	public float colorVariant;

	public Color MainColor => RainWorld.GoldRGB;

	public Color SecondaryColor
	{
		get
		{
			if (!ModManager.MSC || room == null || !room.game.IsStorySession || room.game.GetStorySession.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				return RainWorld.AntiGold.rgb;
			}
			if (colorVariant < 0.5f)
			{
				return new HSLColor(0.45f, 0.65f, 0.53f).rgb;
			}
			return new HSLColor(0.63f, 0.65f, 0.53f).rgb;
		}
	}

	public SlugcatGhost(Vector2 pos, Room room)
	{
		base.room = room;
		this.pos = pos;
		floatyVecs = new Vector2[2, 4, 4];
		lightFluctuations = new float[7, 4];
		flipX = ((Random.value < 0.5f) ? (-1f) : 1f);
		colorVariant = Random.value;
		if (room.game.manager.musicPlayer != null && (!ModManager.MSC || room.abstractRoom.name != "SI_SAINTINTRO"))
		{
			room.game.manager.musicPlayer.FadeOutAllSongs(30f);
		}
	}

	public float LightFluct(int i, float timeStacker)
	{
		return Mathf.Max(Mathf.Lerp(lightFluctuations[i, 1], lightFluctuations[i, 0], timeStacker), Mathf.Lerp(lastDissapate, dissapate, timeStacker));
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastLightBall = lightBall;
		lastDissapate = dissapate;
		if (soundsStatus == 0)
		{
			room.PlaySound(SoundID.Slugcat_Ghost_Appear, pos);
			soundsStatus = 1;
		}
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room)
			{
				counter++;
				if (Custom.DistLess(room.game.Players[i].realizedCreature.mainBodyChunk.pos, pos, 140f))
				{
					counter += 5;
				}
				if (Custom.DistLess(room.game.Players[i].realizedCreature.mainBodyChunk.pos, pos, 80f) || counter > 60)
				{
					remove = true;
				}
			}
		}
		if (remove)
		{
			lightBall = Mathf.Min(1f, lightBall + 1f / 26f);
			dissapate = Mathf.Min(1f, dissapate + Mathf.InverseLerp(0.4f, 1f, lightBall) / 26f);
			if (soundsStatus == 1 && lightBall > 0.5f)
			{
				room.PlaySound(SoundID.Slugcat_Ghost_Dissappear, pos);
				soundsStatus = 2;
			}
			if (dissapate == 1f && lastDissapate == 1f)
			{
				Destroy();
			}
		}
		for (int j = 0; j < floatyVecs.GetLength(0); j++)
		{
			for (int k = 0; k < floatyVecs.GetLength(1); k++)
			{
				floatyVecs[j, k, 1] = floatyVecs[j, k, 0];
				floatyVecs[j, k, 0] = Vector2.Lerp(floatyVecs[j, k, 0], floatyVecs[j, k, 2], 0.1f);
				floatyVecs[j, k, 2] = Vector2.Lerp(floatyVecs[j, k, 2], floatyVecs[j, k, 3], 0.3f);
				if (Random.value < 0.1f)
				{
					floatyVecs[j, k, 3] = Custom.RNV() * Mathf.Pow(Random.value, 2f);
				}
			}
		}
		for (int l = 0; l < lightFluctuations.GetLength(0); l++)
		{
			lightFluctuations[l, 1] = lightFluctuations[l, 0];
			lightFluctuations[l, 0] = Custom.LerpAndTick(lightFluctuations[l, 0], lightFluctuations[l, 2], 0.05f, 0.025f);
			lightFluctuations[l, 2] = Custom.LerpAndTick(lightFluctuations[l, 2], lightFluctuations[l, 3], 0.05f, 0.025f);
			if (Random.value < 0.1f)
			{
				lightFluctuations[l, 3] = Mathf.Clamp01(lightFluctuations[l, 3] + Mathf.Lerp(-0.5f, 0.5f, Random.value));
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[7];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLight"];
		sLeaser.sprites[0].color = SecondaryColor;
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["FlatLight"];
		sLeaser.sprites[1].color = SecondaryColor;
		sLeaser.sprites[2] = new CustomFSprite("slugcatSleeping");
		sLeaser.sprites[3] = new CustomFSprite("slugcatSleeping");
		sLeaser.sprites[4] = new FSprite("Futile_White");
		sLeaser.sprites[4].shader = rCam.game.rainWorld.Shaders["FlatLight"];
		sLeaser.sprites[4].color = MainColor;
		sLeaser.sprites[5] = new FSprite("Futile_White");
		sLeaser.sprites[5].shader = rCam.game.rainWorld.Shaders["FlatLightNoisy"];
		sLeaser.sprites[5].color = MainColor;
		sLeaser.sprites[6] = new FSprite("Futile_White");
		sLeaser.sprites[6].shader = rCam.game.rainWorld.Shaders["GoldenGlow"];
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = pos;
		float num = Custom.SCurve(Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastLightBall, lightBall, timeStacker)), 3f), 0.6f);
		float num2 = Custom.SCurve(Mathf.Lerp(lastDissapate, dissapate, timeStacker), 0.4f);
		num *= 1f - num2 * 0.2f;
		float num3 = Custom.LerpMap(num2, 0.3f, 1f, 1f, 0f, 0.7f);
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (i != 2 && i != 3)
			{
				sLeaser.sprites[i].x = vector.x - camPos.x;
				sLeaser.sprites[i].y = vector.y - camPos.y;
			}
		}
		Vector2 vector2 = new Vector2(37f * flipX, 20f) * Custom.LerpMap(num, 0.6f, 1f, 1f, 0.5f, 2f);
		for (int j = 0; j < 2; j++)
		{
			float num4 = ((j == 0) ? 1.5f : 5f);
			(sLeaser.sprites[j + 2] as CustomFSprite).MoveVertice(0, vector + new Vector2(0f - vector2.x, vector2.y) / 2f - camPos + Vector2.Lerp(floatyVecs[j, 0, 1], floatyVecs[j, 0, 0], timeStacker) * num4);
			(sLeaser.sprites[j + 2] as CustomFSprite).MoveVertice(1, vector + new Vector2(vector2.x, vector2.y) / 2f - camPos + Vector2.Lerp(floatyVecs[j, 1, 1], floatyVecs[j, 1, 0], timeStacker) * num4);
			(sLeaser.sprites[j + 2] as CustomFSprite).MoveVertice(2, vector + new Vector2(vector2.x, 0f - vector2.y) / 2f - camPos + Vector2.Lerp(floatyVecs[j, 2, 1], floatyVecs[j, 2, 0], timeStacker) * num4);
			(sLeaser.sprites[j + 2] as CustomFSprite).MoveVertice(3, vector + new Vector2(0f - vector2.x, 0f - vector2.y) / 2f - camPos + Vector2.Lerp(floatyVecs[j, 3, 1], floatyVecs[j, 3, 0], timeStacker) * num4);
			for (int k = 0; k < 4; k++)
			{
				(sLeaser.sprites[j + 2] as CustomFSprite).verticeColors[k] = Custom.RGB2RGBA(MainColor, Mathf.Pow(1f - Vector2.Lerp(floatyVecs[j, k, 1], floatyVecs[j, k, 0], timeStacker).magnitude, (j == 0) ? 0.5f : 2f) * Mathf.Lerp(0.8f, 1f, LightFluct(j + 2, timeStacker)) * Mathf.InverseLerp(0.1f, 0f, num2));
			}
		}
		float num5 = 1f + 0.1f * num2 + 0.2f * Mathf.Pow(num2, 0.2f);
		sLeaser.sprites[0].scale = Mathf.Lerp(Mathf.Lerp(60f, 70f, LightFluct(0, timeStacker)), 16f, num) * num5;
		sLeaser.sprites[0].alpha = 0.4f * num3;
		sLeaser.sprites[1].scale = Mathf.Lerp(Mathf.Lerp(22f, 28f, LightFluct(1, timeStacker)), 8f, num) * num5;
		sLeaser.sprites[1].alpha = 0.4f * num3;
		sLeaser.sprites[4].scale = Mathf.Lerp(Mathf.Lerp(5.5f, 6.5f, LightFluct(4, timeStacker)), 7f, num) * num5;
		sLeaser.sprites[4].alpha = Mathf.Lerp(0.4f, 1f, num) * num3;
		sLeaser.sprites[5].scale = Mathf.Lerp(Mathf.Lerp(30f, 50f, LightFluct(5, timeStacker)), 16f, num) * num5;
		sLeaser.sprites[5].alpha = 0.6f * num3;
		sLeaser.sprites[6].scale = Mathf.Lerp(Mathf.Lerp(31f, 37f, LightFluct(6, timeStacker)), 16f, num) * num5;
		sLeaser.sprites[6].alpha = (0.9f + 0.1f * num) * Mathf.Pow(num3, 2f);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Bloom");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
