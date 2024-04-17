using System;
using RWCustom;
using UnityEngine;

public class GarbageWormGraphics : GraphicsModule
{
	private float sinWave;

	private float numberOfWavesOnBody;

	private float sinSpeed;

	private float[] swallowArray;

	private float lastExtended;

	private float extended;

	private GarbageWorm worm => base.owner as GarbageWorm;

	public GarbageWormGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		numberOfWavesOnBody = 1.8f;
		sinSpeed = 1f / 60f;
		swallowArray = new float[worm.tentacle.tChunks.Length];
		cullRange = 1000f;
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void Update()
	{
		base.Update();
		if (culled)
		{
			return;
		}
		if (worm.Consious)
		{
			if (worm.AI.attackCounter < 20)
			{
				numberOfWavesOnBody = Mathf.Lerp(numberOfWavesOnBody, Mathf.Lerp(1.8f, 3.4f, worm.AI.stress), 0.1f);
				sinSpeed = Mathf.Lerp(sinSpeed, Mathf.Lerp(1f / 60f, 0.05f, worm.AI.stress), 0.05f);
			}
			else
			{
				numberOfWavesOnBody = Mathf.Lerp(numberOfWavesOnBody, 5f, 0.01f);
				sinSpeed = Mathf.Lerp(sinSpeed, 0.05f, 0.1f);
			}
			sinWave += sinSpeed;
			if (sinWave > 1f)
			{
				sinWave -= 1f;
			}
			if (worm.AI.attackCounter > 40 && worm.AI.attackCounter < 190 && UnityEngine.Random.value < 1f / 30f)
			{
				swallowArray[swallowArray.Length - 1] = Mathf.Pow(UnityEngine.Random.value, 0.5f);
			}
			if (UnityEngine.Random.value < 1f / 3f)
			{
				for (int i = 0; i < swallowArray.Length - 1; i++)
				{
					swallowArray[i] = Mathf.Lerp(swallowArray[i], swallowArray[i + 1], 0.7f);
				}
			}
			swallowArray[swallowArray.Length - 1] = Mathf.Lerp(swallowArray[swallowArray.Length - 1], 0f, 0.7f);
		}
		lastExtended = extended;
		extended = worm.extended;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[4];
		sLeaser.sprites[0] = new FSprite("WormEye");
		sLeaser.sprites[1] = TriangleMesh.MakeLongMesh(worm.tentacle.tChunks.Length, pointyTip: false, customColor: false);
		sLeaser.sprites[2] = new FSprite("WormHead");
		sLeaser.sprites[3] = new FSprite("WormEye");
		sLeaser.sprites[2].scale = Mathf.Lerp(worm.bodySize, 1f, 0.5f);
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		float num = Mathf.Lerp(lastExtended, extended, timeStacker);
		Vector2 vector = worm.bodyChunks[1].pos + new Vector2(0f, -30f - 100f * (1f - num));
		float num2 = 4f;
		for (int i = 0; i < worm.tentacle.tChunks.Length; i++)
		{
			Vector2 a = Vector2.Lerp(worm.tentacle.tChunks[i].lastPos, worm.tentacle.tChunks[i].pos, timeStacker);
			float num3 = (float)i / (float)(worm.tentacle.tChunks.Length - 1);
			float num4 = Mathf.Pow(Mathf.Max(1f - num3 - num, 0f), 1.5f);
			if (num < 0.2f)
			{
				num4 = Mathf.Min(1f, num4 + Mathf.InverseLerp(0.2f, 0f, num));
			}
			a = Vector2.Lerp(a, worm.bodyChunks[1].pos, num4) + new Vector2(0f, -100f * Mathf.Pow(num4, 0.5f));
			float num5 = Mathf.Sin((Mathf.Lerp(sinWave - sinSpeed, sinWave, timeStacker) + num3 * numberOfWavesOnBody) * (float)Math.PI * 2f);
			a += Custom.PerpendicularVector((a - vector).normalized) * num5 * 11f * Mathf.Pow(Mathf.Max(0f, Mathf.Sin(num3 * (float)Math.PI)), 0.75f) * num;
			Vector2 normalized = (a - vector).normalized;
			Vector2 vector2 = Custom.PerpendicularVector(normalized);
			if (i == worm.tentacle.tChunks.Length - 1)
			{
				sLeaser.sprites[2].x = a.x - camPos.x;
				sLeaser.sprites[2].y = a.y - camPos.y;
				sLeaser.sprites[2].rotation = Custom.AimFromOneVectorToAnother(-normalized, normalized);
				float f = Mathf.Cos(Custom.AimFromOneVectorToAnother(-normalized, normalized) / 360f * 2f * (float)Math.PI);
				f = Mathf.Pow(Mathf.Abs(f), 0.25f) * Mathf.Sign(f);
				int num6 = ((f * Mathf.Sign(normalized.x) > 0f) ? 3 : 0);
				sLeaser.sprites[3 - num6].x = a.x - camPos.x + normalized.x * 5f * worm.bodySize + vector2.x * 3f * Mathf.Lerp(worm.bodySize, 1f, 0.75f) * f;
				sLeaser.sprites[3 - num6].y = a.y - camPos.y + normalized.y * 5f * worm.bodySize + vector2.y * 3f * Mathf.Lerp(worm.bodySize, 1f, 0.75f) * f;
				sLeaser.sprites[num6].x = a.x - camPos.x + normalized.x * 5f * worm.bodySize - vector2.x * 3f * Mathf.Lerp(worm.bodySize, 1f, 0.75f) * f;
				sLeaser.sprites[num6].y = a.y - camPos.y + normalized.y * 5f * worm.bodySize - vector2.y * 3f * Mathf.Lerp(worm.bodySize, 1f, 0.75f) * f;
			}
			float num7 = Vector2.Distance(a, vector) / 7f;
			float num8 = worm.tentacle.tChunks[i].stretchedRad + swallowArray[i] * 5f;
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(i * 4, vector - vector2 * (num8 + num2) * 0.5f + normalized * num7 - camPos);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector2 * (num8 + num2) * 0.5f + normalized * num7 - camPos);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(i * 4 + 2, a - vector2 * num8 - normalized * num7 - camPos);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(i * 4 + 3, a + vector2 * num8 - normalized * num7 - camPos);
			num2 = num8;
			vector = a;
		}
		sLeaser.sprites[0].color = (worm.AI.showAsAngry ? new Color(1f, 0f, 0f) : new Color(1f, 1f, 1f));
		sLeaser.sprites[3].color = (worm.AI.showAsAngry ? new Color(1f, 0f, 0f) : new Color(1f, 1f, 1f));
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[1].color = palette.blackColor;
		sLeaser.sprites[2].color = palette.blackColor;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
