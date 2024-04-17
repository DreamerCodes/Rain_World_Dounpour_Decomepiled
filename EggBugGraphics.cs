using System;
using RWCustom;
using UnityEngine;

public class EggBugGraphics : GraphicsModule
{
	public EggBug bug;

	public float flip;

	public float lastFlip;

	public Vector2 zRotat;

	public Vector2 lastZRotat;

	private float darkness;

	private float lastDarkness;

	private Color blackColor;

	public Limb[,] legs;

	public int legsDangleCounter;

	public GenericBodyPart[,] antennas;

	public GenericBodyPart tailEnd;

	public GenericBodyPart[,] eggs;

	public float legLength = 35f;

	public static float HUE_OFF = 1.5f;

	public static float HUE_OFF2 = 0.65f;

	public float drawTicker;

	private Color antennaTipColor;

	private Color[] eggColors;

	public int MeshSprite
	{
		get
		{
			if (!bug.FireBug)
			{
				return 19;
			}
			return 25;
		}
	}

	public int HeadSprite
	{
		get
		{
			if (!bug.FireBug)
			{
				return 20;
			}
			return 26;
		}
	}

	public int TotalSprites
	{
		get
		{
			if (!bug.FireBug)
			{
				return 50;
			}
			return 62;
		}
	}

	public bool ShowEggs
	{
		get
		{
			if (bug.FireBug || bug.dead)
			{
				if (bug.FireBug)
				{
					return bug.eggsLeft > 0;
				}
				return false;
			}
			return true;
		}
	}

	public int BackEggSprite(int s, int e, int part)
	{
		return ((!bug.FireBug) ? 1 : 7) + s * 9 + (2 - e) * 3 + part;
	}

	public int AntennaSprite(int side)
	{
		return (bug.FireBug ? 27 : 21) + side;
	}

	public int EyeSprite(int eye)
	{
		if (eye != 0)
		{
			if (!bug.FireBug)
			{
				return 23;
			}
			return 29;
		}
		return 0;
	}

	public int LegSprite(int leg, int side, int part)
	{
		return (bug.FireBug ? 30 : 24) + side * 4 + leg * 2 + part;
	}

	public int FrontEggSprite(int s, int e, int part)
	{
		return (bug.FireBug ? 44 : 32) + s * 9 + (2 - e) * 3 + part;
	}

	public int FrontSpearSprite(int s, int e)
	{
		return 38 + s * 3 + e;
	}

	public int BackSpearSprite(int s, int e)
	{
		return 1 + s * 3 + e;
	}

	public EggBugGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		bug = ow as EggBug;
		tailEnd = new GenericBodyPart(this, 3f, 0.5f, 0.99f, bug.bodyChunks[1]);
		lastDarkness = -1f;
		antennas = new GenericBodyPart[2, 4];
		for (int i = 0; i < antennas.GetLength(0); i++)
		{
			for (int j = 0; j < antennas.GetLength(1); j++)
			{
				antennas[i, j] = new GenericBodyPart(this, 1f, 0.5f, 0.9f, base.owner.bodyChunks[0]);
			}
		}
		legs = new Limb[2, 2];
		for (int k = 0; k < legs.GetLength(0); k++)
		{
			for (int l = 0; l < legs.GetLength(1); l++)
			{
				legs[k, l] = new Limb(this, bug.mainBodyChunk, k * 2 + l, 0.1f, 0.7f, 0.99f, 17f, 0.8f);
			}
		}
		eggs = new GenericBodyPart[2, 3];
		for (int m = 0; m < eggs.GetLength(0); m++)
		{
			for (int n = 0; n < eggs.GetLength(1); n++)
			{
				eggs[m, n] = new GenericBodyPart(this, 4f, 0.5f, 0.99f, bug.bodyChunks[1]);
			}
		}
		bodyParts = new BodyPart[19];
		bodyParts[0] = tailEnd;
		int num = 1;
		for (int num2 = 0; num2 < legs.GetLength(0); num2++)
		{
			for (int num3 = 0; num3 < legs.GetLength(1); num3++)
			{
				bodyParts[num] = legs[num2, num3];
				num++;
			}
		}
		for (int num4 = 0; num4 < antennas.GetLength(0); num4++)
		{
			for (int num5 = 0; num5 < antennas.GetLength(1); num5++)
			{
				bodyParts[num] = antennas[num4, num5];
				num++;
			}
		}
		for (int num6 = 0; num6 < eggs.GetLength(0); num6++)
		{
			for (int num7 = 0; num7 < eggs.GetLength(1); num7++)
			{
				bodyParts[num] = eggs[num6, num7];
				num++;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (culled)
		{
			return;
		}
		if (!bug.Consious || !bug.Footing)
		{
			legsDangleCounter = 30;
		}
		else if (legsDangleCounter > 0)
		{
			legsDangleCounter--;
			if (bug.Footing)
			{
				for (int i = 0; i < 2; i++)
				{
					if (bug.room.aimap.TileAccessibleToCreature(bug.bodyChunks[i].pos, bug.Template))
					{
						legsDangleCounter = 0;
					}
				}
			}
		}
		lastFlip = flip;
		lastZRotat = zRotat;
		if (bug.Consious && bug.room.aimap.getAItile(bug.mainBodyChunk.pos).acc == AItile.Accessibility.Climb)
		{
			zRotat = Vector3.Slerp(zRotat, Custom.DegToVec(180f + 90f * flip), 0.5f);
		}
		else
		{
			zRotat = Vector3.Slerp(zRotat, Custom.DegToVec(-90f * flip), 0.5f);
		}
		tailEnd.Update();
		tailEnd.ConnectToPoint(bug.bodyChunks[1].pos + Custom.DirVec(bug.bodyChunks[0].pos, bug.bodyChunks[1].pos) * 7f + Custom.PerpendicularVector(bug.bodyChunks[0].pos, bug.bodyChunks[1].pos) * flip * 7f, 6f, push: false, 0.2f, base.owner.bodyChunks[1].vel, 0.5f, 0.1f);
		tailEnd.vel.y -= 0.4f;
		tailEnd.vel += Custom.DirVec(bug.bodyChunks[0].pos, bug.bodyChunks[1].pos) * 0.8f;
		float num = Custom.AimFromOneVectorToAnother(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos);
		float num2 = 0f;
		float num3 = 0f;
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				num3 += Custom.LerpMap(Vector2.Dot(Custom.DirVec(bug.mainBodyChunk.pos, legs[j, k].absoluteHuntPos), bug.travelDir.normalized), -0.6f, 0.6f, 0f, 0.25f);
				num2 += Custom.DistanceToLine(legs[j, k].pos, bug.bodyChunks[1].pos, bug.bodyChunks[0].pos);
			}
		}
		num3 *= Mathf.InverseLerp(0f, 0.1f, bug.travelDir.magnitude);
		flip = Custom.LerpAndTick(flip, Mathf.Clamp(num2 / 40f, -1f, 1f), 0.07f, 0.1f);
		if (legsDangleCounter > 0)
		{
			num3 = 1f;
		}
		float num4 = 0f;
		if (bug.room.GetTile(bug.mainBodyChunk.pos + Custom.PerpendicularVector(bug.mainBodyChunk.pos, bug.bodyChunks[1].pos) * 20f).Solid)
		{
			num4 += 1f;
		}
		if (bug.room.GetTile(bug.mainBodyChunk.pos - Custom.PerpendicularVector(bug.mainBodyChunk.pos, bug.bodyChunks[1].pos) * 20f).Solid)
		{
			num4 -= 1f;
		}
		if (num4 != 0f)
		{
			flip = Custom.LerpAndTick(flip, num4, 0.07f, 0.05f);
		}
		int num5 = 0;
		for (int l = 0; l < legs.GetLength(0); l++)
		{
			for (int m = 0; m < legs.GetLength(1); m++)
			{
				legs[l, m].Update();
				float num6 = 0.5f + 0.5f * Mathf.Sin((bug.runCycle + (float)num5 * 0.25f) * (float)Math.PI);
				if (legs[l, m].mode == Limb.Mode.HuntRelativePosition || legsDangleCounter > 0)
				{
					legs[l, m].mode = Limb.Mode.Dangle;
				}
				Vector2 vector = Custom.DegToVec(num + ((m == 1) ? 45f : 135f) * ((num4 != 0f) ? (0f - num4) : ((l == 0) ? 1f : (-1f))));
				if (bug.Consious)
				{
					vector += bug.travelDir * Custom.LerpMap(num3, 0f, 0.6f, 3f, 0f) * Mathf.Pow(num6, 0.5f);
					vector.Normalize();
				}
				Vector2 vector2 = bug.bodyChunks[0].pos + vector * legLength * 0.2f + Custom.DegToVec(num + ((m == 1) ? 45f : 135f) * ((l == 0) ? 1f : (-1f))) * legLength * 0.1f;
				legs[l, m].ConnectToPoint(vector2, legLength, push: false, 0f, bug.mainBodyChunk.vel, 0.1f, 0f);
				legs[l, m].ConnectToPoint(bug.bodyChunks[0].pos, legLength, push: false, 0f, bug.mainBodyChunk.vel, 0.1f, 0f);
				if (Custom.DistLess(legs[l, m].pos, bug.mainBodyChunk.pos, 6f))
				{
					legs[l, m].pos = bug.mainBodyChunk.pos + Custom.DirVec(bug.mainBodyChunk.pos, legs[l, m].pos) * 6f;
				}
				if (legsDangleCounter > 0)
				{
					Vector2 vector3 = vector2 + vector * legLength * 0.7f;
					legs[l, m].vel = Vector2.Lerp(legs[l, m].vel, vector3 - legs[l, m].pos, 0.3f);
					legs[l, m].vel.y -= 0.4f;
					if (bug.Consious)
					{
						legs[l, m].vel += Custom.RNV() * 3f;
					}
				}
				else
				{
					Vector2 vector4 = vector2 + vector * legLength;
					for (int n = 0; n < legs.GetLength(0); n++)
					{
						for (int num7 = 0; num7 < legs.GetLength(1); num7++)
						{
							if (n != l && num7 != m && Custom.DistLess(vector4, legs[n, num7].absoluteHuntPos, legLength * 0.3f))
							{
								vector4 = legs[n, num7].absoluteHuntPos + Custom.DirVec(legs[n, num7].absoluteHuntPos, vector4) * legLength * 0.3f;
							}
						}
					}
					float num8 = 1.5f;
					if (!legs[l, m].reachedSnapPosition)
					{
						legs[l, m].FindGrip(bug.room, vector2, vector2, legLength * num8, vector4, -2, -2, behindWalls: true);
						if (legs[l, m].mode != Limb.Mode.HuntAbsolutePosition)
						{
							legs[l, m].FindGrip(bug.room, vector2, vector2 + vector * legLength * 0.5f, legLength * num8, vector4, -2, -2, behindWalls: true);
						}
					}
					if (!Custom.DistLess(legs[l, m].pos, legs[l, m].absoluteHuntPos, legLength * num8 * Mathf.Pow(1f - num6, 0.2f)))
					{
						legs[l, m].mode = Limb.Mode.Dangle;
						legs[l, m].vel += vector * 7f;
						legs[l, m].vel = Vector2.Lerp(legs[l, m].vel, vector4 - legs[l, m].pos, 0.5f);
					}
					else
					{
						legs[l, m].vel += vector * 2f;
					}
				}
				num5++;
			}
		}
		for (int num9 = 0; num9 < 2; num9++)
		{
			for (int num10 = 0; num10 < antennas.GetLength(1); num10++)
			{
				float num11 = Mathf.InverseLerp(0f, antennas.GetLength(1) - 1, num10);
				antennas[num9, num10].Update();
				antennas[num9, num10].vel *= 0.9f;
				if (num10 == 0)
				{
					antennas[num9, num10].ConnectToPoint(bug.mainBodyChunk.pos + AntennaDir(0, 1f, 1f) * 4f, bug.FireBug ? 10f : 20f, push: false, 0f, new Vector2(0f, 0f), 0f, 0f);
				}
				else if (!Custom.DistLess(antennas[num9, num10].pos, antennas[num9, num10 - 1].pos, bug.FireBug ? 7.5f : 15f))
				{
					Vector2 vector5 = -Custom.DirVec(antennas[num9, num10].pos, antennas[num9, num10 - 1].pos) * ((bug.FireBug ? 7.5f : 15f) - Vector2.Distance(antennas[num9, num10].pos, antennas[num9, num10 - 1].pos)) * 0.5f;
					antennas[num9, num10].pos += vector5;
					antennas[num9, num10].vel += vector5;
					antennas[num9, num10 - 1].pos -= vector5;
					antennas[num9, num10 - 1].vel -= vector5;
				}
				if (num10 > 1)
				{
					antennas[num9, num10].vel += Custom.DirVec(antennas[num9, num10 - 2].pos, antennas[num9, num10].pos) * 0.8f;
					antennas[num9, num10 - 2].vel -= Custom.DirVec(antennas[num9, num10 - 2].pos, antennas[num9, num10].pos) * 0.8f;
				}
				antennas[num9, num10].vel += (AntennaDir(num9, Mathf.Pow(1f - num11, 0.5f), 1f) + bug.awayFromTerrainDir * Mathf.Sin(num11 * (float)Math.PI) * 1.7f) * Mathf.Lerp(6f, 2f, num11);
				antennas[num9, num10].vel.y -= 0.3f;
				if (bug.Consious)
				{
					antennas[num9, num10].pos += Custom.RNV() * num11 * UnityEngine.Random.value * (1f + bug.AI.fear + bug.antennaAttention);
					if (bug.sitting)
					{
						antennas[num9, num10].vel += Custom.DirVec(antennas[num9, num10].pos, bug.antennaDir) * 11f * Mathf.Pow(num11, 0.5f) * UnityEngine.Random.value * bug.antennaAttention;
					}
				}
			}
			if (!ShowEggs)
			{
				continue;
			}
			for (int num12 = 0; num12 < eggs.GetLength(1); num12++)
			{
				eggs[num9, num12].Update();
				Vector2 vector6 = EggAttachPos(num9, num12, 1f);
				eggs[num9, num12].ConnectToPoint(vector6, 5f, push: true, 0f, bug.bodyChunks[1].vel, 0.1f, 0f);
				eggs[num9, num12].vel.y -= 0.9f;
				eggs[num9, num12].vel += Custom.DirVec(Vector2.Lerp(bug.mainBodyChunk.pos, bug.bodyChunks[1].pos, 0.5f), vector6) * 0.7f;
				eggs[num9, num12].vel += Custom.PerpendicularVector(bug.bodyChunks[1].pos, bug.mainBodyChunk.pos) * flip * 0.4f;
				if (UnityEngine.Random.value < 0.05f && (bug.shake > 0 || bug.noJumps > 60) && UnityEngine.Random.value < bug.AI.fear && bug.room.ViewedByAnyCamera(eggs[num9, num12].pos, 50f))
				{
					Vector2 vel = Custom.DirVec(vector6, eggs[num9, num12].pos) * Mathf.Lerp(3f, 7f, UnityEngine.Random.value) + Custom.RNV() * UnityEngine.Random.value * 4f;
					bug.room.AddObject(new EggBugEgg.LiquidDrip(eggs[num9, num12].pos, vel, Color.Lerp(eggColors[1], blackColor, 0.4f)));
				}
			}
		}
	}

	public void Squirt(float intensity)
	{
		if (!ShowEggs)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < eggs.GetLength(1); j++)
			{
				if (UnityEngine.Random.value < Mathf.Lerp(0.07f, 0.4f, intensity) && bug.room.ViewedByAnyCamera(eggs[i, j].pos, 50f))
				{
					Vector2 vel = Custom.DirVec(EggAttachPos(i, j, 1f), eggs[i, j].pos) * Mathf.Lerp(3f, 7f, Mathf.Pow(UnityEngine.Random.value, 1.5f - 0.6f * intensity)) + Custom.RNV() * UnityEngine.Random.value * 4f;
					vel *= Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.4f, 0.4f, intensity));
					bug.room.AddObject(new EggBugEgg.LiquidDrip(eggs[i, j].pos, vel, Color.Lerp(eggColors[1], blackColor, 0.4f)));
				}
			}
		}
	}

	public Vector2 AntennaDir(int s, float sideFac, float timeStacker)
	{
		Vector2 vector = Custom.DirVec(Vector2.Lerp(bug.bodyChunks[1].lastPos, bug.bodyChunks[1].pos, timeStacker), Vector2.Lerp(bug.mainBodyChunk.lastPos, bug.mainBodyChunk.pos, timeStacker));
		return (vector + Custom.PerpendicularVector(vector) * Mathf.Lerp((s == 0) ? (-0.7f) : 0.7f, Mathf.Lerp(lastFlip, flip, timeStacker) * -1.4f, Mathf.Abs(Mathf.Lerp(lastFlip, flip, timeStacker) * 0.7f)) * sideFac).normalized;
	}

	public Vector2 EggAttachPos(int s, int egg, float timeStacker)
	{
		float t = Mathf.InverseLerp(0f, 2f, egg);
		Vector2 v = Vector3.Slerp(lastZRotat, zRotat, timeStacker);
		Vector2 vector = Vector2.Lerp(Vector2.Lerp(bug.bodyChunks[1].lastPos, bug.bodyChunks[1].pos, timeStacker), Vector2.Lerp(tailEnd.lastPos, tailEnd.pos, timeStacker), t);
		Vector2 p = Vector2.Lerp(bug.bodyChunks[0].lastPos, bug.bodyChunks[0].pos, timeStacker);
		Vector2 vector2 = Custom.DirVec(vector, p);
		float num = Custom.DegToVec(Custom.VecToDeg(v) + ((s == 0) ? (-45f) : 45f)).x;
		if (ShowEggs)
		{
			num *= Mathf.Lerp(1.5f, 1f, Mathf.Abs(Mathf.Lerp(lastFlip, flip, timeStacker)));
		}
		return vector + vector2 * Mathf.Lerp(bug.FireBug ? 10f : 2f, bug.FireBug ? 0f : (-2f), t) + Custom.PerpendicularVector(vector2) * num * ((egg == 2) ? 3f : 5f);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[MeshSprite] = TriangleMesh.MakeLongMesh(7, pointyTip: false, customColor: false);
		sLeaser.sprites[HeadSprite] = new FSprite("Circle20");
		for (int i = 0; i < 2; i++)
		{
			if (bug.FireBug)
			{
				sLeaser.sprites[EyeSprite(i)] = new FSprite("LizardBubble6");
				sLeaser.sprites[EyeSprite(i)].scale = 0.35f;
			}
			else
			{
				sLeaser.sprites[EyeSprite(i)] = new FSprite("JetFishEyeB");
			}
		}
		for (int j = 0; j < 2; j++)
		{
			sLeaser.sprites[AntennaSprite(j)] = TriangleMesh.MakeLongMesh(antennas.GetLength(1), pointyTip: true, customColor: true);
			for (int k = 0; k < 2; k++)
			{
				sLeaser.sprites[LegSprite(j, k, 0)] = new FSprite("CentipedeLegA");
				sLeaser.sprites[LegSprite(j, k, 1)] = new FSprite("CentipedeLegB");
			}
			for (int l = 0; l < 3; l++)
			{
				if (bug.FireBug)
				{
					sLeaser.sprites[BackSpearSprite(j, l)] = new FSprite("FireBugSpear");
					sLeaser.sprites[FrontSpearSprite(j, l)] = new FSprite("FireBugSpearColor");
					sLeaser.sprites[BackSpearSprite(j, l)].anchorY = 0.5f;
					sLeaser.sprites[FrontSpearSprite(j, l)].anchorY = 0.5f;
				}
				sLeaser.sprites[BackEggSprite(j, l, 0)] = new FSprite(bug.FireBug ? "LegsAPole" : "DangleFruit0A");
				sLeaser.sprites[FrontEggSprite(j, l, 0)] = new FSprite(bug.FireBug ? "LegsAPole" : "EggBugEggColor");
				if (!bug.FireBug)
				{
					sLeaser.sprites[BackEggSprite(j, l, 0)].scaleX = 0.7f;
					sLeaser.sprites[FrontEggSprite(j, l, 0)].scaleX = 0.7f;
					sLeaser.sprites[BackEggSprite(j, l, 0)].scaleY = 0.75f;
					sLeaser.sprites[FrontEggSprite(j, l, 0)].scaleY = 0.75f;
				}
				sLeaser.sprites[BackEggSprite(j, l, 0)].anchorY = 0.3f;
				sLeaser.sprites[FrontEggSprite(j, l, 0)].anchorY = 0.3f;
				sLeaser.sprites[BackEggSprite(j, l, 1)] = new FSprite(bug.FireBug ? "JetFishEyeB" : "DangleFruit0B");
				sLeaser.sprites[FrontEggSprite(j, l, 1)] = new FSprite(bug.FireBug ? "JetFishEyeB" : "EggBugEggColor");
				if (!bug.FireBug)
				{
					sLeaser.sprites[BackEggSprite(j, l, 1)].scaleX = 0.7f;
					sLeaser.sprites[FrontEggSprite(j, l, 1)].scaleX = 0.7f;
					sLeaser.sprites[BackEggSprite(j, l, 1)].scaleY = 0.75f;
					sLeaser.sprites[FrontEggSprite(j, l, 1)].scaleY = 0.75f;
				}
				sLeaser.sprites[BackEggSprite(j, l, 1)].anchorY = 0.3f;
				sLeaser.sprites[FrontEggSprite(j, l, 1)].anchorY = 0.3f;
				sLeaser.sprites[BackEggSprite(j, l, 2)] = new FSprite(bug.FireBug ? "Futile_White" : "JetFishEyeA");
				sLeaser.sprites[FrontEggSprite(j, l, 2)] = new FSprite(bug.FireBug ? "Futile_White" : "JetFishEyeA");
				if (!bug.FireBug)
				{
					sLeaser.sprites[BackEggSprite(j, l, 2)].scale = 0.45f;
					sLeaser.sprites[FrontEggSprite(j, l, 2)].scale = 0.45f;
				}
				sLeaser.sprites[BackEggSprite(j, l, 2)].anchorY = (bug.FireBug ? 0.3f : 0.7f);
				sLeaser.sprites[FrontEggSprite(j, l, 2)].anchorY = (bug.FireBug ? 0.3f : 0.7f);
				if (bug.FireBug)
				{
					sLeaser.sprites[BackEggSprite(j, l, 2)].shader = rCam.game.rainWorld.Shaders["WaterNut"];
					sLeaser.sprites[FrontEggSprite(j, l, 2)].shader = rCam.game.rainWorld.Shaders["WaterNut"];
				}
			}
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		drawTicker += 1f;
		float num = Mathf.Lerp(lastFlip, flip, timeStacker);
		Vector2 v = Vector3.Slerp(lastZRotat, zRotat, timeStacker);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(Vector2.Lerp(bug.mainBodyChunk.lastPos, bug.mainBodyChunk.pos, timeStacker));
		if (darkness > 0.5f)
		{
			darkness = Mathf.Lerp(darkness, 0.5f, rCam.room.LightSourceExposure(Vector2.Lerp(bug.mainBodyChunk.lastPos, bug.mainBodyChunk.pos, timeStacker)));
		}
		if (lastDarkness != darkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		Vector2 vector = Vector2.Lerp(bug.mainBodyChunk.lastPos, bug.mainBodyChunk.pos, timeStacker);
		if (bug.shake > 0)
		{
			vector += Custom.RNV() * UnityEngine.Random.value * 4f;
		}
		Vector2 vector2 = Vector2.Lerp(bug.bodyChunks[1].lastPos, bug.bodyChunks[1].pos, timeStacker);
		if (bug.shake > 0)
		{
			vector2 += Custom.RNV() * UnityEngine.Random.value * 4f;
		}
		Vector2 vector3 = Custom.DirVec(vector2, vector);
		Vector2 vector4 = Custom.PerpendicularVector(vector3);
		Vector2 vector5 = Vector2.Lerp(tailEnd.lastPos, tailEnd.pos, timeStacker);
		vector5 += Custom.DirVec(vector2, vector5) * 3f;
		sLeaser.sprites[HeadSprite].x = vector.x - camPos.x;
		sLeaser.sprites[HeadSprite].y = vector.y - camPos.y;
		sLeaser.sprites[HeadSprite].scaleX = (bug.FireBug ? 0.85f : 0.45f);
		sLeaser.sprites[HeadSprite].scaleY = (bug.FireBug ? 0.95f : 0.55f);
		sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(vector3);
		for (int i = 0; i < 2; i++)
		{
			float num2 = ((i == 0 == num < 0f) ? (-1f) : 1f) * (1f - Mathf.Abs(num));
			Vector2 vector6 = vector + vector3 * 4f + vector4 * num2 * 3f;
			sLeaser.sprites[EyeSprite(i)].x = vector6.x - camPos.x;
			sLeaser.sprites[EyeSprite(i)].y = vector6.y - camPos.y;
		}
		Vector2 vector7 = vector + vector3;
		float num3 = 0f;
		float b = Mathf.Lerp(7f, 5f, Mathf.Abs(num));
		for (int j = 0; j < 7; j++)
		{
			float f = Mathf.InverseLerp(0f, 6f, j);
			Vector2 vector8 = Custom.Bezier(vector + vector3 * 3f, vector2, vector5, vector2, f);
			float num4 = Mathf.Lerp(1.5f, b, Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(f, 0.75f) * (float)Math.PI)), 0.3f));
			Vector2 vector9 = Custom.PerpendicularVector(vector8, vector7);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4, (vector7 + vector8) / 2f - vector9 * (num4 + num3) * 0.5f - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 1, (vector7 + vector8) / 2f + vector9 * (num4 + num3) * 0.5f - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector8 - vector9 * num4 - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector8 + vector9 * num4 - camPos);
			vector7 = vector8;
			num3 = num4;
		}
		for (int k = 0; k < 2; k++)
		{
			for (int l = 0; l < 2; l++)
			{
				Vector2 vector10 = Vector2.Lerp(vector, vector2, 0.3f);
				vector10 += vector4 * ((k == 0) ? (-1f) : 1f) * 3f * (1f - Mathf.Abs(num));
				vector10 += vector3 * ((l == 0) ? (-1f) : 1f) * 4f;
				Vector2 vector11 = Vector2.Lerp(legs[k, l].lastPos, legs[k, l].pos, timeStacker);
				if (Custom.DistLess(vector10, vector11, 6f))
				{
					vector11 = vector10 + Custom.DirVec(vector10, vector11) * 6f;
				}
				float f2 = Mathf.Lerp((k == 0) ? (-1f) : 1f, num * Mathf.Clamp(Custom.DistanceToLine(vector11, vector2 - vector3 * 20f, vector2 - vector3 * 20f + vector4) / -20f, -1f, 1f), Mathf.Abs(num));
				Vector2 vector12 = Custom.InverseKinematic(vector10, vector11, legLength / 3f, legLength * (2f / 3f), f2);
				sLeaser.sprites[LegSprite(l, k, 0)].x = vector10.x - camPos.x;
				sLeaser.sprites[LegSprite(l, k, 0)].y = vector10.y - camPos.y;
				sLeaser.sprites[LegSprite(l, k, 0)].rotation = Custom.AimFromOneVectorToAnother(vector10, vector12);
				sLeaser.sprites[LegSprite(l, k, 0)].scaleY = Vector2.Distance(vector10, vector12) / (bug.FireBug ? 17f : 27f);
				sLeaser.sprites[LegSprite(l, k, 0)].anchorY = 0.1f;
				sLeaser.sprites[LegSprite(l, k, 1)].anchorY = 0.1f;
				sLeaser.sprites[LegSprite(l, k, 0)].scaleX = (0f - Mathf.Sign(flip)) * 0.8f;
				sLeaser.sprites[LegSprite(l, k, 1)].scaleX = (0f - Mathf.Sign(f2)) * 1f;
				sLeaser.sprites[LegSprite(l, k, 1)].x = vector12.x - camPos.x;
				sLeaser.sprites[LegSprite(l, k, 1)].y = vector12.y - camPos.y;
				sLeaser.sprites[LegSprite(l, k, 1)].rotation = Custom.AimFromOneVectorToAnother(vector12, vector11);
				sLeaser.sprites[LegSprite(l, k, 1)].scaleY = (Vector2.Distance(vector12, vector11) + 1f) / (bug.FireBug ? 15f : 25f);
			}
		}
		for (int m = 0; m < 2; m++)
		{
			vector7 = vector;
			float num5 = 1f;
			for (int n = 0; n < antennas.GetLength(1); n++)
			{
				Vector2 vector13 = Vector2.Lerp(antennas[m, n].lastPos, antennas[m, n].pos, timeStacker);
				if (bug.FireBug && n > 0)
				{
					Vector2 vector14 = Vector2.Lerp(antennas[m, n - 1].lastPos, antennas[m, n - 1].pos, timeStacker);
					Vector2 v2 = Custom.DirVec(vector14, vector13);
					float num6 = Custom.Dist(vector14, vector13);
					float ang = Custom.VecToDeg(v2) + (float)((n % 2 == 0) ? (-45) : 45);
					vector13 = vector14 + Custom.DegToVec(ang) * num6;
				}
				Vector2 normalized = (vector13 - vector7).normalized;
				Vector2 vector15 = Custom.PerpendicularVector(normalized);
				if (n == 0)
				{
					(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(n * 4, vector7 - vector15 * num5 - camPos);
					(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(n * 4 + 1, vector7 + vector15 * num5 - camPos);
					(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(n * 4 + 2, (vector13 + vector7) / 2f - vector15 * num5 - camPos);
					(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(n * 4 + 3, (vector13 + vector7) / 2f + vector15 * num5 - camPos);
				}
				else
				{
					float num7 = Vector2.Distance(vector13, vector7) / 5f;
					(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(n * 4, vector7 - vector15 * num5 + normalized * num7 - camPos);
					(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(n * 4 + 1, vector7 + vector15 * num5 + normalized * num7 - camPos);
					if (n < antennas.GetLength(1) - 1)
					{
						(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(n * 4 + 2, vector13 - vector15 * num5 - normalized * num7 - camPos);
						(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(n * 4 + 3, vector13 + vector15 * num5 - normalized * num7 - camPos);
					}
					else
					{
						(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(n * 4 + 2, vector13 - camPos);
					}
				}
				vector7 = vector13;
			}
		}
		if (bug.FireBug)
		{
			for (int num8 = 0; num8 < 2; num8++)
			{
				for (int num9 = 0; num9 < 3; num9++)
				{
					int num10 = num8 * 3 + num9;
					if (bug.eggsLeft <= num10)
					{
						for (int num11 = 0; num11 < 2; num11++)
						{
							sLeaser.sprites[BackEggSprite(num8, num9, num11)].isVisible = false;
							sLeaser.sprites[FrontEggSprite(num8, num9, num11)].isVisible = false;
						}
						sLeaser.sprites[BackSpearSprite(num8, num9)].isVisible = true;
						sLeaser.sprites[FrontSpearSprite(num8, num9)].isVisible = true;
						if (!bug.stabSpine[num8, num9])
						{
							sLeaser.sprites[BackSpearSprite(num8, num9)].color = blackColor;
							sLeaser.sprites[FrontSpearSprite(num8, num9)].color = Custom.HSL2RGB(Custom.Decimal(bug.hue + HUE_OFF), 1f, 0.5f);
						}
						else
						{
							sLeaser.sprites[BackSpearSprite(num8, num9)].color = Color.white;
							sLeaser.sprites[FrontSpearSprite(num8, num9)].color = Color.white;
						}
					}
					else
					{
						sLeaser.sprites[BackSpearSprite(num8, num9)].isVisible = false;
						sLeaser.sprites[FrontSpearSprite(num8, num9)].isVisible = false;
					}
					Vector2 vector16 = EggAttachPos(num8, num9, timeStacker);
					Vector2 p = Vector2.Lerp(eggs[num8, num9].lastPos, eggs[num8, num9].pos, timeStacker);
					sLeaser.sprites[BackSpearSprite(num8, num9)].x = vector16.x - camPos.x;
					sLeaser.sprites[BackSpearSprite(num8, num9)].y = vector16.y - camPos.y;
					sLeaser.sprites[BackSpearSprite(num8, num9)].rotation = Custom.AimFromOneVectorToAnother(vector16, vector);
					sLeaser.sprites[BackSpearSprite(num8, num9)].scaleY = bug.spineExtensions[num8, num9];
					sLeaser.sprites[FrontSpearSprite(num8, num9)].x = vector16.x - camPos.x;
					sLeaser.sprites[FrontSpearSprite(num8, num9)].y = vector16.y - camPos.y;
					sLeaser.sprites[FrontSpearSprite(num8, num9)].rotation = Custom.AimFromOneVectorToAnother(vector16, vector) + 180f;
					sLeaser.sprites[FrontSpearSprite(num8, num9)].scaleY = bug.spineExtensions[num8, num9];
					bool isVisible = Custom.DegToVec(Custom.VecToDeg(v) + ((num8 != 0) ? 45f : (-45f))).y > 0f;
					for (int num12 = 0; num12 < 3; num12++)
					{
						if (num12 <= 1 && bug.eggsLeft <= num10)
						{
							continue;
						}
						if (num12 == 2 && bug.eggsLeft <= num10)
						{
							sLeaser.sprites[FrontEggSprite(num8, num9, 2)].x = vector16.x - camPos.x;
							sLeaser.sprites[FrontEggSprite(num8, num9, 2)].y = vector16.y - camPos.y;
							sLeaser.sprites[FrontEggSprite(num8, num9, 2)].scaleY = 0.35f;
							sLeaser.sprites[FrontEggSprite(num8, num9, 2)].scaleX = 0.2f;
							sLeaser.sprites[FrontEggSprite(num8, num9, 2)].color = Color.Lerp(antennaTipColor, eggColors[2], 0.5f);
							sLeaser.sprites[FrontEggSprite(num8, num9, 2)].rotation = Custom.AimFromOneVectorToAnother(vector16, vector);
							sLeaser.sprites[BackEggSprite(num8, num9, 2)].x = vector16.x - camPos.x;
							sLeaser.sprites[BackEggSprite(num8, num9, 2)].y = vector16.y - camPos.y;
							sLeaser.sprites[BackEggSprite(num8, num9, 2)].scaleY = 0.35f;
							sLeaser.sprites[BackEggSprite(num8, num9, 2)].scaleX = 0.2f;
							sLeaser.sprites[BackEggSprite(num8, num9, 2)].color = Color.Lerp(antennaTipColor, eggColors[2], 0.5f);
							sLeaser.sprites[BackEggSprite(num8, num9, 2)].rotation = Custom.AimFromOneVectorToAnother(vector16, vector);
							sLeaser.sprites[FrontEggSprite(num8, num9, 2)].isVisible = isVisible;
						}
						else
						{
							float num13 = Mathf.Sin(drawTicker * 0.15f + (float)num10 * (float)Math.PI);
							sLeaser.sprites[BackEggSprite(num8, num9, num12)].x = p.x - camPos.x;
							sLeaser.sprites[BackEggSprite(num8, num9, num12)].y = p.y - camPos.y;
							sLeaser.sprites[FrontEggSprite(num8, num9, num12)].x = p.x - camPos.x;
							sLeaser.sprites[FrontEggSprite(num8, num9, num12)].y = p.y - camPos.y;
							if (num12 < 2)
							{
								sLeaser.sprites[BackEggSprite(num8, num9, num12)].scaleX = 1f + num13 * 0.2f;
								sLeaser.sprites[BackEggSprite(num8, num9, num12)].scaleY = 1f + num13 * 0.2f;
							}
							else
							{
								sLeaser.sprites[BackEggSprite(num8, num9, num12)].scaleX = 0.85f + num13 * 0.15f;
								sLeaser.sprites[BackEggSprite(num8, num9, num12)].scaleY = 0.85f + num13 * 0.15f;
							}
							sLeaser.sprites[BackEggSprite(num8, num9, num12)].rotation = Custom.AimFromOneVectorToAnother(p, vector16);
							sLeaser.sprites[FrontEggSprite(num8, num9, num12)].rotation = Custom.AimFromOneVectorToAnother(p, vector16);
							sLeaser.sprites[FrontEggSprite(num8, num9, num12)].isVisible = isVisible;
						}
					}
				}
			}
			return;
		}
		if (ShowEggs)
		{
			for (int num14 = 0; num14 < 2; num14++)
			{
				for (int num15 = 0; num15 < 3; num15++)
				{
					Vector2 p2 = EggAttachPos(num14, num15, timeStacker);
					Vector2 p3 = Vector2.Lerp(eggs[num14, num15].lastPos, eggs[num14, num15].pos, timeStacker);
					bool isVisible2 = Custom.DegToVec(Custom.VecToDeg(v) + ((num14 == 0) ? (-45f) : 45f)).y > 0f;
					for (int num16 = 0; num16 < 3; num16++)
					{
						sLeaser.sprites[BackEggSprite(num14, num15, num16)].x = p3.x - camPos.x;
						sLeaser.sprites[BackEggSprite(num14, num15, num16)].y = p3.y - camPos.y;
						sLeaser.sprites[FrontEggSprite(num14, num15, num16)].x = p3.x - camPos.x;
						sLeaser.sprites[FrontEggSprite(num14, num15, num16)].y = p3.y - camPos.y;
						sLeaser.sprites[BackEggSprite(num14, num15, num16)].rotation = Custom.AimFromOneVectorToAnother(p3, p2);
						sLeaser.sprites[FrontEggSprite(num14, num15, num16)].rotation = Custom.AimFromOneVectorToAnother(p3, p2);
						sLeaser.sprites[FrontEggSprite(num14, num15, num16)].isVisible = isVisible2;
					}
				}
			}
			return;
		}
		for (int num17 = 0; num17 < 2; num17++)
		{
			for (int num18 = 0; num18 < 3; num18++)
			{
				for (int num19 = 0; num19 < 2; num19++)
				{
					sLeaser.sprites[BackEggSprite(num17, num18, num19)].isVisible = false;
					sLeaser.sprites[FrontEggSprite(num17, num18, num19)].isVisible = false;
				}
				Vector2 p4 = EggAttachPos(num17, num18, timeStacker);
				bool isVisible3 = Custom.DegToVec(Custom.VecToDeg(v) + ((num17 == 0) ? (-45f) : 45f)).y > 0f;
				sLeaser.sprites[FrontEggSprite(num17, num18, 2)].x = p4.x - camPos.x;
				sLeaser.sprites[FrontEggSprite(num17, num18, 2)].y = p4.y - camPos.y;
				sLeaser.sprites[FrontEggSprite(num17, num18, 2)].scaleY = 0.35f;
				sLeaser.sprites[FrontEggSprite(num17, num18, 2)].scaleX = 0.2f;
				sLeaser.sprites[FrontEggSprite(num17, num18, 2)].color = Color.Lerp(antennaTipColor, eggColors[2], 0.5f);
				sLeaser.sprites[FrontEggSprite(num17, num18, 2)].rotation = Custom.AimFromOneVectorToAnother(p4, vector);
				sLeaser.sprites[BackEggSprite(num17, num18, 2)].x = p4.x - camPos.x;
				sLeaser.sprites[BackEggSprite(num17, num18, 2)].y = p4.y - camPos.y;
				sLeaser.sprites[BackEggSprite(num17, num18, 2)].scaleY = 0.35f;
				sLeaser.sprites[BackEggSprite(num17, num18, 2)].scaleX = 0.2f;
				sLeaser.sprites[BackEggSprite(num17, num18, 2)].color = Color.Lerp(antennaTipColor, eggColors[2], 0.5f);
				sLeaser.sprites[BackEggSprite(num17, num18, 2)].rotation = Custom.AimFromOneVectorToAnother(p4, vector);
				sLeaser.sprites[FrontEggSprite(num17, num18, 2)].isVisible = isVisible3;
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = Custom.HSL2RGB(Custom.Decimal(bug.hue + (bug.FireBug ? HUE_OFF : 1.5f)), 1f, 0.5f);
		Color color2 = Custom.HSL2RGB(Custom.Decimal(bug.hue + (bug.FireBug ? HUE_OFF2 : 1f)), 1f, 0.5f);
		blackColor = Color.Lerp(palette.blackColor, Color.Lerp(color2, palette.fogColor, 0.3f), 0.1f * (1f - darkness));
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = blackColor;
		}
		if (bug.FireBug)
		{
			eggColors = FireEggColors(palette, bug.hue, darkness);
		}
		else
		{
			eggColors = EggColors(palette, bug.hue, darkness);
		}
		for (int j = 0; j < 2; j++)
		{
			for (int k = 0; k < 3; k++)
			{
				for (int l = 0; l < 3; l++)
				{
					sLeaser.sprites[BackEggSprite(j, k, l)].color = eggColors[bug.FireBug ? 1 : l];
					sLeaser.sprites[FrontEggSprite(j, k, l)].color = eggColors[bug.FireBug ? 1 : l];
					if (bug.FireBug)
					{
						sLeaser.sprites[BackSpearSprite(j, k)].color = blackColor;
						sLeaser.sprites[FrontSpearSprite(j, k)].color = color;
					}
				}
			}
		}
		for (int m = 0; m < 2; m++)
		{
			sLeaser.sprites[EyeSprite(m)].color = Color.Lerp(Color.Lerp(palette.fogColor, color, 0.5f), blackColor, Mathf.InverseLerp(0.75f, 1f, darkness));
		}
		antennaTipColor = Color.Lerp(blackColor, color2, 0.2f * Mathf.Pow(1f - darkness, 0.2f));
		for (int n = 0; n < 2; n++)
		{
			for (int num = 0; num < (sLeaser.sprites[AntennaSprite(n)] as TriangleMesh).verticeColors.Length; num++)
			{
				(sLeaser.sprites[AntennaSprite(n)] as TriangleMesh).verticeColors[num] = Color.Lerp(blackColor, antennaTipColor, (float)num / (float)((sLeaser.sprites[AntennaSprite(n)] as TriangleMesh).verticeColors.Length - 1));
			}
		}
	}

	public static Color[] EggColors(RoomPalette palette, float hue, float darkness)
	{
		Color a = Custom.HSL2RGB(Custom.Decimal(hue + 1.5f), 1f, 0.5f);
		Color a2 = Custom.HSL2RGB(Custom.Decimal(hue + 1f), 1f, 0.5f);
		Color color = Color.Lerp(palette.blackColor, Color.Lerp(a2, palette.fogColor, 0.3f), 0.1f * (1f - darkness));
		darkness = Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, darkness), 2f);
		return new Color[3]
		{
			color,
			Color.Lerp(a, color, darkness),
			Color.Lerp(a2, color, 0.5f + 0.5f * darkness)
		};
	}

	public static Color[] FireEggColors(RoomPalette palette, float hue, float darkness)
	{
		Color a = Custom.HSL2RGB(Custom.Decimal(hue + HUE_OFF), 1f, 0.5f);
		Color a2 = Custom.HSL2RGB(Custom.Decimal(hue + HUE_OFF2), 1f, 0.5f);
		Color color = Color.Lerp(palette.blackColor, Color.Lerp(a2, palette.fogColor, 0.3f), 0.1f * (1f - darkness));
		darkness = Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, darkness), 2f);
		return new Color[3]
		{
			color,
			Color.Lerp(a, color, darkness),
			Color.Lerp(a2, color, 0.5f + 0.5f * darkness)
		};
	}
}
