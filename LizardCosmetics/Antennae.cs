using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class Antennae : Template
{
	public GenericBodyPart[,] antennae;

	private Color redderTint;

	private int segments;

	private float length;

	private float alpha;

	private int Sprite(int side, int part)
	{
		return startSprite + part * 2 + side;
	}

	public Antennae(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.InFront;
		length = Random.value;
		segments = Mathf.FloorToInt(Mathf.Lerp(3f, 8f, Mathf.Pow(length, Mathf.Lerp(1f, 6f, length))));
		alpha = length * 0.9f + Random.value * 0.1f;
		antennae = new GenericBodyPart[2, segments];
		for (int i = 0; i < segments; i++)
		{
			antennae[0, i] = new GenericBodyPart(lGraphics, 1f, 0.6f, 0.9f, lGraphics.lizard.mainBodyChunk);
			antennae[1, i] = new GenericBodyPart(lGraphics, 1f, 0.6f, 0.9f, lGraphics.lizard.mainBodyChunk);
		}
		redderTint = new Color(lGraphics.effectColor.r, lGraphics.effectColor.g, lGraphics.effectColor.b);
		redderTint.g *= 0.5f;
		redderTint.b *= 0.5f;
		redderTint.r = Mathf.Lerp(redderTint.r, 1f, 0.75f);
		numberOfSprites = 4;
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < segments; j++)
			{
				antennae[i, j].Reset(AnchorPoint(i, 1f));
			}
		}
	}

	public override void Update()
	{
		float num = lGraphics.lizard.AI.yellowAI.commFlicker;
		if (!lGraphics.lizard.Consious)
		{
			num = 0f;
		}
		float num2 = Mathf.Lerp(10f, 7f, length);
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < segments; j++)
			{
				float a = (float)j / (float)(segments - 1);
				a = Mathf.Lerp(a, Mathf.InverseLerp(0f, 5f, j), 0.2f);
				antennae[i, j].vel += AntennaDir(i, 1f) * (1f - a + 0.6f * num);
				if (lGraphics.lizard.room.PointSubmerged(antennae[i, j].pos))
				{
					antennae[i, j].vel *= 0.8f;
				}
				else
				{
					antennae[i, j].vel.y -= 0.4f * a * (1f - num);
				}
				antennae[i, j].Update();
				antennae[i, j].pos += Custom.RNV() * 3f * num;
				Vector2 p;
				if (j == 0)
				{
					antennae[i, j].vel += AntennaDir(i, 1f) * 5f;
					p = lGraphics.head.pos;
					antennae[i, j].ConnectToPoint(AnchorPoint(i, 1f), num2, push: true, 0f, lGraphics.lizard.mainBodyChunk.vel, 0f, 0f);
				}
				else
				{
					p = ((j != 1) ? antennae[i, j - 2].pos : AnchorPoint(i, 1f));
					Vector2 vector = Custom.DirVec(antennae[i, j].pos, antennae[i, j - 1].pos);
					float num3 = Vector2.Distance(antennae[i, j].pos, antennae[i, j - 1].pos);
					antennae[i, j].pos -= vector * (num2 - num3) * 0.5f;
					antennae[i, j].vel -= vector * (num2 - num3) * 0.5f;
					antennae[i, j - 1].pos += vector * (num2 - num3) * 0.5f;
					antennae[i, j - 1].vel += vector * (num2 - num3) * 0.5f;
				}
				antennae[i, j].vel += Custom.DirVec(p, antennae[i, j].pos) * 3f * Mathf.Pow(1f - a, 0.3f);
				if (j > 1)
				{
					antennae[i, j - 2].vel += Custom.DirVec(antennae[i, j].pos, antennae[i, j - 2].pos) * 3f * Mathf.Pow(1f - a, 0.3f);
				}
				if (!Custom.DistLess(lGraphics.head.pos, antennae[i, j].pos, 200f))
				{
					antennae[i, j].pos = lGraphics.head.pos;
				}
			}
		}
	}

	private Vector2 AntennaDir(int side, float timeStacker)
	{
		float num = Mathf.Lerp(lGraphics.lastHeadDepthRotation, lGraphics.headDepthRotation, timeStacker);
		return Custom.RotateAroundOrigo(new Vector2(((side == 0) ? (-1f) : 1f) * (1f - Mathf.Abs(num)) * 1.5f + num * 3.5f, -1f).normalized, Custom.AimFromOneVectorToAnother(Vector2.Lerp(lGraphics.drawPositions[0, 1], lGraphics.drawPositions[0, 0], timeStacker), Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker)));
	}

	private Vector2 AnchorPoint(int side, float timeStacker)
	{
		return Vector2.Lerp(lGraphics.drawPositions[0, 1], lGraphics.drawPositions[0, 0], timeStacker) + AntennaDir(side, timeStacker) * 3f * lGraphics.iVars.headSize;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[Sprite(i, j)] = TriangleMesh.MakeLongMesh(segments, pointyTip: true, customColor: true);
			}
		}
		for (int k = 0; k < 2; k++)
		{
			sLeaser.sprites[Sprite(k, 1)].shader = rCam.room.game.rainWorld.Shaders["LizardAntenna"];
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float flicker = Mathf.Pow(Random.value, 1f - 0.5f * lGraphics.lizard.AI.yellowAI.commFlicker) * lGraphics.lizard.AI.yellowAI.commFlicker;
		if (!lGraphics.lizard.Consious)
		{
			flicker = 0f;
		}
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[startSprite + i].color = lGraphics.HeadColor(timeStacker);
			Vector2 vector = Vector2.Lerp(Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker), AnchorPoint(i, timeStacker), 0.5f);
			float num = 1f;
			float num2 = 0f;
			for (int j = 0; j < segments; j++)
			{
				float num3 = (float)j / (float)(segments - 1);
				Vector2 vector2 = Vector2.Lerp(antennae[i, j].lastPos, antennae[i, j].pos, timeStacker);
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num4 = Vector2.Distance(vector2, vector) / 5f;
				float num5 = Mathf.Lerp(3f, 1f, Mathf.Pow(num3, 0.8f));
				for (int k = 0; k < 2; k++)
				{
					(sLeaser.sprites[Sprite(i, k)] as TriangleMesh).MoveVertice(j * 4, vector - vector3 * (num + num5) * 0.5f + normalized * num4 - camPos);
					(sLeaser.sprites[Sprite(i, k)] as TriangleMesh).MoveVertice(j * 4 + 1, vector + vector3 * (num + num5) * 0.5f + normalized * num4 - camPos);
					(sLeaser.sprites[Sprite(i, k)] as TriangleMesh).verticeColors[j * 4] = EffectColor(k, (num3 + num2) / 2f, timeStacker, flicker);
					(sLeaser.sprites[Sprite(i, k)] as TriangleMesh).verticeColors[j * 4 + 1] = EffectColor(k, (num3 + num2) / 2f, timeStacker, flicker);
					(sLeaser.sprites[Sprite(i, k)] as TriangleMesh).verticeColors[j * 4 + 2] = EffectColor(k, num3, timeStacker, flicker);
					if (j < segments - 1)
					{
						(sLeaser.sprites[Sprite(i, k)] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - vector3 * num5 - normalized * num4 - camPos);
						(sLeaser.sprites[Sprite(i, k)] as TriangleMesh).MoveVertice(j * 4 + 3, vector2 + vector3 * num5 - normalized * num4 - camPos);
						(sLeaser.sprites[Sprite(i, k)] as TriangleMesh).verticeColors[j * 4 + 3] = EffectColor(k, num3, timeStacker, flicker);
					}
					else
					{
						(sLeaser.sprites[Sprite(i, k)] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - camPos);
					}
				}
				num = num5;
				vector = vector2;
				num2 = num3;
			}
		}
	}

	public Color EffectColor(int part, float tip, float timeStacker, float flicker)
	{
		tip = Mathf.Pow(Mathf.InverseLerp(0f, 0.6f, tip), 0.5f);
		if (part == 0)
		{
			return Color.Lerp(lGraphics.HeadColor(timeStacker), Color.Lerp(lGraphics.effectColor, lGraphics.palette.blackColor, flicker), tip);
		}
		return Color.Lerp(new Color(redderTint.r, redderTint.g, redderTint.b, alpha), new Color(1f, 1f, 1f, alpha), flicker);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
	}
}
