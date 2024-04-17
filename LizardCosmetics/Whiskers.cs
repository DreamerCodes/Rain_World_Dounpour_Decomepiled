using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class Whiskers : Template
{
	public GenericBodyPart[,] whiskers;

	public Vector2[] whiskerDirections;

	public float[,] whiskerProps;

	public float[,,] whiskerLightUp;

	public int amount;

	public Whiskers(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.InFront;
		amount = Random.Range(3, 5);
		whiskers = new GenericBodyPart[2, amount];
		whiskerDirections = new Vector2[amount];
		whiskerProps = new float[amount, 5];
		whiskerLightUp = new float[amount, 2, 2];
		for (int i = 0; i < amount; i++)
		{
			whiskers[0, i] = new GenericBodyPart(lGraphics, 1f, 0.6f, 0.9f, lGraphics.lizard.mainBodyChunk);
			whiskers[1, i] = new GenericBodyPart(lGraphics, 1f, 0.6f, 0.9f, lGraphics.lizard.mainBodyChunk);
			whiskerDirections[i] = Custom.DegToVec(Mathf.Lerp(4f, 100f, Random.value));
			whiskerProps[i, 0] = Custom.ClampedRandomVariation(0.5f, 0.4f, 0.5f) * 40f;
			whiskerProps[i, 1] = Mathf.Lerp(-0.5f, 0.8f, Random.value);
			whiskerProps[i, 2] = Mathf.Lerp(11f, 720f, Mathf.Pow(Random.value, 1.5f)) / whiskerProps[i, 0];
			whiskerProps[i, 3] = Random.value;
			whiskerProps[i, 4] = Mathf.Lerp(0.6f, 1.2f, Mathf.Pow(Random.value, 1.6f));
			if (i <= 0)
			{
				continue;
			}
			for (int j = 0; j < 5; j++)
			{
				if (j != 1)
				{
					whiskerProps[i, j] = Mathf.Lerp(whiskerProps[i, j], whiskerProps[i - 1, j], Mathf.Pow(Random.value, 0.3f) * 0.6f);
				}
			}
		}
		numberOfSprites = amount * 2;
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < amount; j++)
			{
				whiskers[i, j].Reset(AnchorPoint(i, j, 1f));
			}
		}
	}

	public override void Update()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < amount; j++)
			{
				whiskers[i, j].vel += whiskerDir(i, j, 1f) * whiskerProps[j, 2];
				if (lGraphics.lizard.room.PointSubmerged(whiskers[i, j].pos))
				{
					whiskers[i, j].vel *= 0.8f;
				}
				else
				{
					whiskers[i, j].vel.y -= 0.6f;
				}
				whiskers[i, j].Update();
				whiskers[i, j].ConnectToPoint(AnchorPoint(i, j, 1f), whiskerProps[j, 0], push: false, 0f, lGraphics.lizard.mainBodyChunk.vel, 0f, 0f);
				if (!Custom.DistLess(lGraphics.head.pos, whiskers[i, j].pos, 200f))
				{
					whiskers[i, j].pos = lGraphics.head.pos;
				}
				whiskerLightUp[j, i, 1] = whiskerLightUp[j, i, 0];
				if (whiskerLightUp[j, i, 0] < Mathf.InverseLerp(0f, 0.3f, lGraphics.blackLizardLightUpHead))
				{
					whiskerLightUp[j, i, 0] = Mathf.Lerp(whiskerLightUp[j, i, 0], Mathf.InverseLerp(0f, 0.3f, lGraphics.blackLizardLightUpHead), 0.7f) + 0.05f;
				}
				else
				{
					whiskerLightUp[j, i, 0] -= 0.025f;
				}
				whiskerLightUp[j, i, 0] += Mathf.Lerp(-1f, 1f, Random.value) * 0.03f * lGraphics.blackLizardLightUpHead;
				whiskerLightUp[j, i, 0] = Mathf.Clamp(whiskerLightUp[j, i, 0], 0f, 1f);
			}
		}
	}

	private Vector2 whiskerDir(int side, int m, float timeStacker)
	{
		float num = Mathf.Lerp(lGraphics.lastHeadDepthRotation, lGraphics.headDepthRotation, timeStacker);
		return Custom.RotateAroundOrigo(new Vector2(((side == 0) ? (-1f) : 1f) * (1f - Mathf.Abs(num)) * whiskerDirections[m].x + num * whiskerProps[m, 1], whiskerDirections[m].y).normalized, Custom.AimFromOneVectorToAnother(Vector2.Lerp(lGraphics.drawPositions[0, 1], lGraphics.drawPositions[0, 0], timeStacker), Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker)));
	}

	private Vector2 AnchorPoint(int side, int m, float timeStacker)
	{
		if (ModManager.MMF)
		{
			return Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker) + Custom.DegToVec(lGraphics.HeadRotation(timeStacker)) * 2.85f * lGraphics.iVars.headSize + whiskerDir(side, m, timeStacker);
		}
		return Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker) + whiskerDir(side, m, timeStacker) * 3f * lGraphics.iVars.headSize;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int num = startSprite + amount * 2 - 1; num >= startSprite; num--)
		{
			sLeaser.sprites[num] = TriangleMesh.MakeLongMesh(4, pointyTip: true, customColor: true);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Custom.DegToVec(lGraphics.HeadRotation(timeStacker));
		for (int i = 0; i < amount; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				Vector2 vector2 = Vector2.Lerp(whiskers[j, i].lastPos, whiskers[j, i].pos, timeStacker);
				Vector2 vector3 = whiskerDir(j, i, timeStacker);
				Vector2 vector4 = AnchorPoint(j, i, timeStacker);
				vector3 = (vector3 + vector).normalized;
				Vector2 vector5 = vector4;
				float num = whiskerProps[i, 4];
				float num2 = 1f;
				for (int k = 0; k < 4; k++)
				{
					Vector2 vector6;
					if (k < 3)
					{
						vector6 = Vector2.Lerp(vector4, vector2, (float)(k + 1) / 4f);
						vector6 += vector3 * num2 * whiskerProps[i, 0] * 0.2f;
					}
					else
					{
						vector6 = vector2;
					}
					num2 *= 0.7f;
					Vector2 normalized = (vector6 - vector5).normalized;
					Vector2 vector7 = Custom.PerpendicularVector(normalized);
					float num3 = Vector2.Distance(vector6, vector5) / ((k == 0) ? 1f : 5f);
					float num4 = Custom.LerpMap(k, 0f, 3f, whiskerProps[i, 4], 0.5f);
					for (int l = k * 4; l < k * 4 + ((k == 3) ? 3 : 4); l++)
					{
						(sLeaser.sprites[startSprite + i * 2 + j] as TriangleMesh).verticeColors[l] = Color.Lerp(lGraphics.HeadColor(timeStacker), new Color(1f, 1f, 1f), (float)(k - 1) / 2f * Mathf.Lerp(whiskerLightUp[i, j, 1], whiskerLightUp[i, j, 0], timeStacker));
					}
					(sLeaser.sprites[startSprite + i * 2 + j] as TriangleMesh).MoveVertice(k * 4, vector5 - vector7 * (num4 + num) * 0.5f + normalized * num3 - camPos);
					(sLeaser.sprites[startSprite + i * 2 + j] as TriangleMesh).MoveVertice(k * 4 + 1, vector5 + vector7 * (num4 + num) * 0.5f + normalized * num3 - camPos);
					if (k < 3)
					{
						(sLeaser.sprites[startSprite + i * 2 + j] as TriangleMesh).MoveVertice(k * 4 + 2, vector6 - vector7 * num4 - normalized * num3 - camPos);
						(sLeaser.sprites[startSprite + i * 2 + j] as TriangleMesh).MoveVertice(k * 4 + 3, vector6 + vector7 * num4 - normalized * num3 - camPos);
					}
					else
					{
						(sLeaser.sprites[startSprite + i * 2 + j] as TriangleMesh).MoveVertice(k * 4 + 2, vector6 + normalized * 2.1f - camPos);
					}
					num = num4;
					vector5 = vector6;
				}
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < amount; j++)
			{
				for (int k = 0; k < (sLeaser.sprites[startSprite + j * 2 + i] as TriangleMesh).verticeColors.Length; k++)
				{
					(sLeaser.sprites[startSprite + j * 2 + i] as TriangleMesh).verticeColors[k] = palette.blackColor;
				}
			}
		}
	}
}
