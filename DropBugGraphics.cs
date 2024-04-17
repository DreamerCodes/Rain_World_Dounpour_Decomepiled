using System;
using RWCustom;
using UnityEngine;

public class DropBugGraphics : GraphicsModule
{
	public DropBug bug;

	private Vector2[,] drawPositions;

	public float flip;

	public float lastFlip;

	private float darkness;

	private float lastDarkness;

	private Color blackColor;

	private Color shineColor;

	private Color camoColor;

	private Color currSkinColor;

	public Limb[,] legs;

	public GenericBodyPart[,] knees;

	public Vector2[,] legsTravelDirs;

	public int legsDangleCounter;

	public GenericBodyPart[] mandibles;

	public float[,] mandibleMovements;

	public GenericBodyPart[] pinchers;

	public GenericBodyPart[] antennae;

	public GenericBodyPart tailEnd;

	public float legLength;

	private Vector2 breathDir;

	private float legsThickness;

	private float bodyThickness;

	private float antennaeLength;

	private float hue;

	private float coloredAntennae;

	private float pinchersFlip;

	private float lastPinchersFlip;

	private float pinchersLength;

	private float vibrate;

	private float lastVibrate;

	private float ceilingMode;

	private float lastCeilingMode;

	public float deepCeilingMode;

	public float lastDeepCeilingMode;

	public bool ceilingJump;

	public int HeadSprite => 8;

	public int MeshSprite => 9;

	public int ShineMeshSprite => 10;

	public int TotalSprites => 27;

	public int LegSprite(int side, int leg)
	{
		return side * 2 + leg;
	}

	public int MandibleSprite(int side, int part)
	{
		return 4 + side * 2 + part;
	}

	public int SegmentSprite(int s)
	{
		return 11 + s;
	}

	public int PincherSprite(int i)
	{
		return 21 + i;
	}

	public int WingSprite(int side)
	{
		return 23 + side;
	}

	public int AntennaSprite(int side)
	{
		return 25 + side;
	}

	public DropBugGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		bug = ow as DropBug;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(bug.abstractCreature.ID.RandomSeed);
		tailEnd = new GenericBodyPart(this, 3f, 0.5f, 0.99f, bug.bodyChunks[2]);
		drawPositions = new Vector2[bug.bodyChunks.Length, 2];
		lastDarkness = -1f;
		legLength = 45f;
		mandibles = new GenericBodyPart[2];
		for (int i = 0; i < mandibles.Length; i++)
		{
			mandibles[i] = new GenericBodyPart(this, 1f, 0.5f, 0.9f, base.owner.bodyChunks[0]);
		}
		pinchers = new GenericBodyPart[2];
		for (int j = 0; j < pinchers.Length; j++)
		{
			pinchers[j] = new GenericBodyPart(this, 1f, 0.5f, 1f, base.owner.bodyChunks[2]);
		}
		antennae = new GenericBodyPart[2];
		for (int k = 0; k < antennae.Length; k++)
		{
			antennae[k] = new GenericBodyPart(this, 1f, 0.5f, 0.9f, base.owner.bodyChunks[0]);
		}
		legs = new Limb[2, 2];
		knees = new GenericBodyPart[2, 2];
		legsTravelDirs = new Vector2[2, 2];
		for (int l = 0; l < legs.GetLength(0); l++)
		{
			for (int m = 0; m < legs.GetLength(1); m++)
			{
				legs[l, m] = new Limb(this, bug.mainBodyChunk, l * 4 + m, 0.1f, 0.7f, 0.99f, 22f, 0.95f);
				knees[l, m] = new GenericBodyPart(this, 1f, 0.5f, 0.99f, bug.mainBodyChunk);
			}
		}
		mandibleMovements = new float[2, 2];
		bodyParts = new BodyPart[15];
		bodyParts[0] = tailEnd;
		bodyParts[1] = mandibles[0];
		bodyParts[2] = mandibles[1];
		bodyParts[3] = pinchers[0];
		bodyParts[4] = pinchers[1];
		bodyParts[5] = antennae[0];
		bodyParts[6] = antennae[1];
		int num = 7;
		for (int n = 0; n < legs.GetLength(0); n++)
		{
			for (int num2 = 0; num2 < legs.GetLength(1); num2++)
			{
				bodyParts[num] = legs[n, num2];
				num++;
				bodyParts[num] = knees[n, num2];
				num++;
			}
		}
		bodyThickness = Mathf.Lerp(0.6f, 1.4f, UnityEngine.Random.value);
		legsThickness = Mathf.Lerp(0.6f, 1.4f, UnityEngine.Random.value);
		antennaeLength = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
		pinchersLength = Mathf.Lerp(40f, 60f, UnityEngine.Random.value);
		coloredAntennae = ((UnityEngine.Random.value < 0.5f) ? UnityEngine.Random.value : 0f);
		hue = 121f / 180f + (1f - Custom.ClampedRandomVariation(0.5f, 0.5f, 0.3f) * 2f) * 0.2f;
		Reset();
		UnityEngine.Random.state = state;
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < drawPositions.GetLength(0); i++)
		{
			drawPositions[i, 1] = bug.bodyChunks[i].pos;
			drawPositions[i, 0] = bug.bodyChunks[i].pos;
		}
		if (bug.AI.behavior == DropBugAI.Behavior.SitInCeiling && bug.abstractCreature.pos.Tile == bug.AI.ceilingModule.ceilingPos.Tile)
		{
			ceilingMode = 1f;
			lastCeilingMode = 1f;
			deepCeilingMode = 1f;
			lastDeepCeilingMode = 1f;
		}
	}

	public override void Update()
	{
		base.Update();
		lastVibrate = vibrate;
		vibrate = Custom.LerpAndTick(vibrate, (bug.charging > 0f || bug.attemptBite > 0f) ? 1f : 0f, 0.2f, 0.05f);
		lastCeilingMode = ceilingMode;
		ceilingMode = Custom.LerpAndTick(ceilingMode, bug.inCeilingMode, 0.06f, 0.025f);
		lastDeepCeilingMode = deepCeilingMode;
		deepCeilingMode = Custom.LerpAndTick(deepCeilingMode, (ceilingMode == 1f) ? 1f : 0f, 0.007f, 1f / 120f);
		if (ceilingJump)
		{
			ceilingJump = false;
			ceilingMode = Mathf.Min(ceilingMode, 0.1f);
			deepCeilingMode = Mathf.Min(deepCeilingMode, 0.1f);
		}
		lastPinchersFlip = pinchersFlip;
		pinchersFlip = Custom.LerpAndTick(pinchersFlip, flip, 0.001f, 1f / Custom.LerpMap(Mathf.Abs(pinchersFlip - flip), 0.03f, 0.6f, 400f, 2f, 0.35f));
		for (int i = 0; i < drawPositions.GetLength(0); i++)
		{
			drawPositions[i, 1] = drawPositions[i, 0];
			drawPositions[i, 0] = bug.bodyChunks[i].pos;
		}
		if (!bug.Consious || !bug.Footing || bug.swimming)
		{
			legsDangleCounter = 30;
		}
		else if (legsDangleCounter > 0)
		{
			legsDangleCounter--;
			if (bug.Footing)
			{
				for (int j = 0; j < 2; j++)
				{
					if (bug.room.aimap.TileAccessibleToCreature(bug.bodyChunks[j].pos, bug.Template))
					{
						legsDangleCounter = 0;
					}
				}
			}
		}
		lastFlip = flip;
		if (bug.inCeilingMode > 0f)
		{
			Vector2 vector = bug.room.MiddleOfTile(bug.AI.ceilingModule.ceilingPos);
			for (int k = 0; k < 2; k++)
			{
				for (int l = 0; l < 2; l++)
				{
					legs[k, l].mode = Limb.Mode.HuntAbsolutePosition;
					legs[k, l].absoluteHuntPos = vector + new Vector2(((k == 0) ? 1f : (-2f)) * ((l == 0) ? 7f : 14f), 10f);
				}
			}
		}
		tailEnd.Update();
		tailEnd.ConnectToPoint(bug.bodyChunks[2].pos + Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[2].pos) * 12f + Custom.PerpendicularVector(bug.bodyChunks[1].pos, bug.bodyChunks[2].pos) * flip * 10f, 12f, push: false, 0.2f, base.owner.bodyChunks[1].vel, 0.5f, 0.1f);
		tailEnd.vel.y -= 0.4f;
		tailEnd.vel += Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[2].pos) * 0.8f;
		if (!bug.dead)
		{
			tailEnd.vel += breathDir * 0.7f * (1f - deepCeilingMode);
			breathDir = Vector2.ClampMagnitude(breathDir + Custom.RNV() * UnityEngine.Random.value * 0.1f, 1f);
		}
		float num = Custom.AimFromOneVectorToAnother(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos);
		Vector2 vector2 = Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos);
		BodyChunk bodyChunk = ((bug.grasps[0] != null) ? bug.grasps[0].grabbedChunk : null);
		float num2 = deepCeilingMode * Mathf.Pow(1f - bug.dropAnticipation, 3f);
		for (int m = 0; m < mandibles.Length; m++)
		{
			mandibles[m].Update();
			mandibles[m].ConnectToPoint(bug.mainBodyChunk.pos + vector2 * Mathf.Lerp(12f + 4f * mandibleMovements[m, 0] + 4f * ceilingMode, 30f, bug.attemptBite), 12f + 4f * mandibleMovements[m, 0], push: false, 0f, bug.mainBodyChunk.vel, 0.1f, 0f);
			if (bug.Consious)
			{
				mandibleMovements[m, 0] = Custom.LerpAndTick(mandibleMovements[m, 0], mandibleMovements[m, 1], 0f, 1f / Mathf.Lerp(20f, 80f, num2));
				if (UnityEngine.Random.value < 1f / Mathf.Lerp(20f, 80f, num2))
				{
					mandibleMovements[m, 1] = Mathf.Clamp(mandibleMovements[m, 1] + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Lerp(0.75f, 0.25f, num2), -1f, 1f);
					mandibleMovements[m, 1] = Mathf.Lerp(mandibleMovements[m, 1], 0.5f, UnityEngine.Random.value * UnityEngine.Random.value * num2);
				}
			}
			mandibles[m].vel += (vector2 + Custom.PerpendicularVector(vector2) * Mathf.Lerp((m == 0) ? (-1f) : 1f, flip * 10f, Mathf.Abs(flip) * 0.9f)).normalized * (1f + 19f * bug.attemptBite);
			if (bodyChunk != null)
			{
				mandibles[m].pos = Vector2.Lerp(mandibles[m].pos, bodyChunk.pos, 0.5f);
				mandibles[m].vel *= 0.7f;
			}
			pinchers[m].Update();
			Vector2 vector3 = (Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[2].pos) + Custom.DirVec(bug.bodyChunks[2].pos, tailEnd.pos) + Custom.PerpendicularVector(bug.bodyChunks[1].pos, bug.bodyChunks[2].pos) * ((m == 0) ? (-1f) : 1f) * (1f - Mathf.Abs(flip)) * Mathf.Lerp(0.2f, 3f, vibrate)).normalized;
			if (ceilingMode > 0f)
			{
				vector3 = Vector3.Slerp(vector3, Custom.DegToVec(180f + ((m == 0) ? (-1f) : 1f) * Mathf.Lerp(30f, 50f, bug.dropAnticipation)), ceilingMode);
			}
			pinchers[m].ConnectToPoint(bug.bodyChunks[2].pos + vector3 * pinchersLength * 0.5f, pinchersLength * 0.5f, push: true, 0f, bug.bodyChunks[2].vel, 0.2f, 0f);
			pinchers[m].vel += vector3 * 10f;
			antennae[m].Update();
			Vector2 vector4 = (Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos) + Custom.PerpendicularVector(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos) * flip * 0.5f + Custom.PerpendicularVector(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos) * ((m == 0) ? (-1f) : 1f) * (1f - Mathf.Abs(flip)) * Mathf.Lerp(0.35f, 1.2f, vibrate)).normalized;
			if (num2 > 0f)
			{
				vector4 = Vector3.Slerp(vector4, Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos), num2 * 0.8f);
			}
			antennae[m].ConnectToPoint(bug.mainBodyChunk.pos, Mathf.Lerp(50f, 40f, num2) * antennaeLength, push: false, 0f, bug.mainBodyChunk.vel, 0.05f, 0f);
			antennae[m].vel += vector4 * Custom.LerpMap(Vector2.Distance(antennae[m].pos, bug.mainBodyChunk.pos + vector4 * Mathf.Lerp(50f, 40f, num2) * antennaeLength), 10f, 150f, 0f, 14f, 0.7f);
			if (bug.Consious && UnityEngine.Random.value > num2)
			{
				antennae[m].vel += Custom.RNV() * UnityEngine.Random.value * Mathf.Lerp(1f, 0.1f, num2);
			}
			if (vibrate > 0f)
			{
				mandibles[m].vel += Custom.RNV() * vibrate * UnityEngine.Random.value * 2f;
				mandibles[m].pos += Custom.RNV() * vibrate * UnityEngine.Random.value * 2f;
				antennae[m].vel -= vector2 * 5f * vibrate;
			}
		}
		float num3 = 0f;
		int num4 = 0;
		for (int n = 0; n < legs.GetLength(0); n++)
		{
			for (int num5 = 0; num5 < legs.GetLength(1); num5++)
			{
				num3 += Custom.DistanceToLine(legs[n, num5].pos, bug.bodyChunks[1].pos, bug.bodyChunks[0].pos);
				if (legs[n, num5].OverLappingHuntPos)
				{
					num4++;
				}
			}
		}
		if (!float.IsNaN(num3))
		{
			num3 = Mathf.Lerp(num3, 0f, bug.inCeilingMode);
			flip = Custom.LerpAndTick(flip, Mathf.Clamp(num3 / 40f, -1f, 1f), 0.07f, 0.1f);
		}
		float num6 = 0f;
		if (bug.inCeilingMode == 0f)
		{
			if (bug.Consious)
			{
				if (bug.room.GetTile(bug.mainBodyChunk.pos + Custom.PerpendicularVector(bug.mainBodyChunk.pos, bug.bodyChunks[1].pos) * 20f).Solid)
				{
					num6 += 1f;
				}
				if (bug.room.GetTile(bug.mainBodyChunk.pos - Custom.PerpendicularVector(bug.mainBodyChunk.pos, bug.bodyChunks[1].pos) * 20f).Solid)
				{
					num6 -= 1f;
				}
			}
			if (num6 != 0f)
			{
				flip = Custom.LerpAndTick(flip, num6, 0.07f, 0.05f);
			}
		}
		int num7 = 0;
		for (int num8 = 0; num8 < legs.GetLength(1); num8++)
		{
			for (int num9 = 0; num9 < legs.GetLength(0); num9++)
			{
				float t = Mathf.InverseLerp(0f, legs.GetLength(1) - 1, num8);
				float num10 = 0.5f + 0.5f * Mathf.Sin((bug.runCycle + (float)num7 * 0.25f) * (float)Math.PI);
				legsTravelDirs[num9, num8] = Vector2.Lerp(legsTravelDirs[num9, num8], bug.travelDir, Mathf.Pow(UnityEngine.Random.value, 1f - 0.9f * num10));
				if (bug.charging > 0f)
				{
					legsTravelDirs[num9, num8] *= 0f;
				}
				legs[num9, num8].Update();
				if (legs[num9, num8].mode == Limb.Mode.HuntRelativePosition || legsDangleCounter > 0)
				{
					legs[num9, num8].mode = Limb.Mode.Dangle;
				}
				Vector2 vector5 = Custom.DegToVec(num + Mathf.Lerp(40f, 160f, t) * ((num6 != 0f) ? (0f - num6) : ((num9 == 0) ? 1f : (-1f))));
				Vector2 vector6 = bug.bodyChunks[0].pos + (Vector2)Vector3.Slerp(legsTravelDirs[num9, num8], vector5, 0.1f) * legLength * 0.85f * Mathf.Pow(num10, 0.5f);
				legs[num9, num8].ConnectToPoint(vector6, legLength, push: false, 0f, bug.mainBodyChunk.vel, 0.1f, 0f);
				legs[num9, num8].ConnectToPoint(bug.bodyChunks[0].pos, legLength, push: false, 0f, bug.mainBodyChunk.vel, 0.1f, 0f);
				knees[num9, num8].Update();
				knees[num9, num8].vel += Custom.DirVec(vector6, knees[num9, num8].pos) * (legLength * 0.55f - Vector2.Distance(knees[num9, num8].pos, vector6)) * 0.6f;
				knees[num9, num8].pos += Custom.DirVec(vector6, knees[num9, num8].pos) * (legLength * 0.55f - Vector2.Distance(knees[num9, num8].pos, vector6)) * 0.6f;
				knees[num9, num8].vel += Custom.DirVec(legs[num9, num8].pos, knees[num9, num8].pos) * (legLength * 0.55f - Vector2.Distance(knees[num9, num8].pos, legs[num9, num8].pos)) * 0.6f;
				knees[num9, num8].pos += Custom.DirVec(legs[num9, num8].pos, knees[num9, num8].pos) * (legLength * 0.55f - Vector2.Distance(knees[num9, num8].pos, legs[num9, num8].pos)) * 0.6f;
				if (Custom.DistLess(knees[num9, num8].pos, bug.mainBodyChunk.pos, 15f))
				{
					knees[num9, num8].vel += Custom.DirVec(bug.mainBodyChunk.pos, knees[num9, num8].pos) * (15f - Vector2.Distance(knees[num9, num8].pos, bug.mainBodyChunk.pos));
					knees[num9, num8].pos += Custom.DirVec(bug.mainBodyChunk.pos, knees[num9, num8].pos) * (15f - Vector2.Distance(knees[num9, num8].pos, bug.mainBodyChunk.pos));
				}
				knees[num9, num8].vel = Vector2.Lerp(knees[num9, num8].vel, bug.mainBodyChunk.vel, 0.8f);
				knees[num9, num8].vel += Custom.PerpendicularVector(bug.bodyChunks[1].pos, bug.mainBodyChunk.pos) * Mathf.Lerp((num9 == 0) ? (-1f) : 1f, Mathf.Sign(flip), Mathf.Abs(flip)) * 9f;
				knees[num9, num8].vel += Custom.RNV() * 4f * vibrate * UnityEngine.Random.value;
				if (bug.Consious)
				{
					drawPositions[0, 0] += Custom.DirVec(legs[num9, num8].pos, drawPositions[0, 0]) * 4f * num10;
					drawPositions[1, 0] += Custom.DirVec(legs[num9, num8].pos, drawPositions[1, 0]) * 3f * num10;
					drawPositions[2, 0] += Custom.DirVec(knees[num9, num8].pos, drawPositions[2, 0]) * 1f * (1f - num10);
				}
				if (!Custom.DistLess(knees[num9, num8].pos, vector6, 200f))
				{
					knees[num9, num8].pos = vector6 + Custom.RNV() * UnityEngine.Random.value;
				}
				if (legsDangleCounter > 0 || num10 < 0.1f)
				{
					Vector2 vector7 = vector6 + vector5 * legLength * 0.5f;
					if (!bug.Consious)
					{
						vector7 = vector6 + legsTravelDirs[num9, num8] * legLength * 0.5f;
					}
					else if (bug.swimming)
					{
						vector7 += Custom.DirVec(drawPositions[2, 0], drawPositions[0, 0]) * Mathf.Lerp(-18f, 18f, num10);
					}
					legs[num9, num8].vel = Vector2.Lerp(legs[num9, num8].vel, vector7 - legs[num9, num8].pos, bug.swimming ? 0.5f : 0.05f);
					legs[num9, num8].vel.y -= 0.4f;
				}
				else
				{
					Vector2 vector8 = vector6 + vector5 * legLength;
					for (int num11 = 0; num11 < legs.GetLength(0); num11++)
					{
						for (int num12 = 0; num12 < legs.GetLength(1); num12++)
						{
							if (num11 != num9 && num12 != num8 && Custom.DistLess(vector8, legs[num11, num12].absoluteHuntPos, legLength * 0.1f))
							{
								vector8 = legs[num11, num12].absoluteHuntPos + Custom.DirVec(legs[num11, num12].absoluteHuntPos, vector8) * legLength * 0.1f;
							}
						}
					}
					float num13 = 1.2f;
					if (bug.inCeilingMode == 0f)
					{
						if (!legs[num9, num8].reachedSnapPosition)
						{
							legs[num9, num8].FindGrip(bug.room, vector6, vector6, legLength * num13, vector8, -2, -2, behindWalls: true);
						}
						else if (!Custom.DistLess(vector6, legs[num9, num8].absoluteHuntPos, legLength * num13 * Mathf.Pow(1f - num10, 0.2f)))
						{
							legs[num9, num8].mode = Limb.Mode.Dangle;
						}
					}
				}
				num7++;
			}
		}
		if (bug.swimming)
		{
			drawPositions[1, 0] += Custom.PerpendicularVector(drawPositions[0, 0], drawPositions[2, 0]) * 5f * Mathf.Sin(bug.runCycle * (float)Math.PI * 2f);
			drawPositions[2, 0] += Custom.PerpendicularVector(drawPositions[0, 0], drawPositions[2, 0]) * 5f * Mathf.Sin((bug.runCycle + 0.25f) * (float)Math.PI * 2f);
		}
		if (bug.voiceSound != null && !bug.voiceSound.slatedForDeletetion)
		{
			drawPositions[0, 0] += Custom.RNV() * UnityEngine.Random.value * 4f;
			for (int num14 = 0; num14 < antennae.Length; num14++)
			{
				antennae[num14].pos += Custom.RNV() * UnityEngine.Random.value * 4f;
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[MeshSprite] = TriangleMesh.MakeLongMesh(12, pointyTip: false, customColor: false);
		sLeaser.sprites[ShineMeshSprite] = TriangleMesh.MakeLongMesh(11, pointyTip: false, customColor: true);
		sLeaser.sprites[HeadSprite] = new FSprite("Circle20");
		sLeaser.sprites[HeadSprite].scaleX = 0.7f;
		sLeaser.sprites[HeadSprite].scaleY = 0.8f;
		for (int i = 0; i < 10; i++)
		{
			sLeaser.sprites[SegmentSprite(i)] = new FSprite("pixel");
			sLeaser.sprites[SegmentSprite(i)].anchorY = 0f;
		}
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				sLeaser.sprites[LegSprite(j, k)] = TriangleMesh.MakeLongMesh(12, pointyTip: false, customColor: false);
			}
		}
		for (int l = 0; l < 2; l++)
		{
			sLeaser.sprites[MandibleSprite(l, 0)] = new FSprite("CentipedeLegA");
			sLeaser.sprites[MandibleSprite(l, 1)] = new FSprite("CentipedeLegB");
			sLeaser.sprites[WingSprite(l)] = new FSprite("CentipedeBackShell");
			sLeaser.sprites[PincherSprite(l)] = TriangleMesh.MakeLongMesh(8, pointyTip: false, customColor: false);
			sLeaser.sprites[AntennaSprite(l)] = TriangleMesh.MakeLongMesh(8, pointyTip: false, coloredAntennae > 0f);
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
		RefreshColor(1f, sLeaser);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		float num = Mathf.Lerp(lastFlip, flip, timeStacker);
		float num2 = Mathf.Lerp(lastVibrate, vibrate, timeStacker);
		float t = Mathf.Lerp(lastDeepCeilingMode, deepCeilingMode, timeStacker);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(Vector2.Lerp(bug.mainBodyChunk.lastPos, bug.mainBodyChunk.pos, timeStacker));
		if (darkness > 0.5f)
		{
			darkness = Mathf.Lerp(darkness, 0.5f, rCam.room.LightSourceExposure(Vector2.Lerp(bug.mainBodyChunk.lastPos, bug.mainBodyChunk.pos, timeStacker)));
		}
		if (lastDarkness != darkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			RefreshColor(timeStacker, sLeaser);
		}
		else if (lastDeepCeilingMode != deepCeilingMode || lastCeilingMode != ceilingMode)
		{
			RefreshColor(timeStacker, sLeaser);
		}
		Vector2 vector = Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker);
		Vector2 vector2 = Vector2.Lerp(drawPositions[1, 1], drawPositions[1, 0], timeStacker) + Custom.RNV() * UnityEngine.Random.value * 10f * num2;
		Vector2 vector3 = Vector2.Lerp(drawPositions[2, 1], drawPositions[2, 0], timeStacker);
		Vector2 vector4 = Custom.DirVec(vector2, vector);
		Vector2 vector5 = Custom.PerpendicularVector(vector4);
		Vector2 vector6 = Vector2.Lerp(tailEnd.lastPos, tailEnd.pos, timeStacker);
		Vector2 normalized = rCam.room.lightAngle.normalized;
		normalized.y *= -1f;
		sLeaser.sprites[HeadSprite].x = vector.x - camPos.x;
		sLeaser.sprites[HeadSprite].y = vector.y - camPos.y;
		sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(vector4);
		Vector2 vector7 = vector + vector4;
		float num3 = 0f;
		float num4 = 0f;
		Vector2 vector8 = vector;
		for (int i = 0; i < 12; i++)
		{
			float num5 = Mathf.InverseLerp(0f, 11f, i);
			Vector2 vector9 = Custom.Bezier(vector + vector4 * 3f, vector2, vector6, vector3, num5);
			float num6 = Mathf.Lerp(6f, 2f, num5) + Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(num5, 1.7f) * (float)Math.PI)), 0.75f) * Mathf.Lerp(7f, 5f, Mathf.Abs(num)) * (1f + Mathf.Lerp(lastCeilingMode, ceilingMode, timeStacker)) * bodyThickness;
			float a = Mathf.Lerp(0.5f + Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, 10f, i) * (float)Math.PI)), 0.25f) * 2f, num6 * 0.5f, num5 * 0.5f);
			a = Mathf.Lerp(a, Mathf.Max(a, num6 * 0.5f), Mathf.Abs(Vector2.Dot((vector9 - vector7).normalized, normalized)));
			Vector2 vector10 = vector9 - normalized * (num6 - a);
			Vector2 vector11 = Custom.PerpendicularVector(vector9, vector7);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(i * 4, (vector7 + vector9) / 2f - vector11 * (num6 + num3) * 0.5f - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(i * 4 + 1, (vector7 + vector9) / 2f + vector11 * (num6 + num3) * 0.5f - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector9 - vector11 * num6 - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector9 + vector11 * num6 - camPos);
			if (i < 11)
			{
				(sLeaser.sprites[ShineMeshSprite] as TriangleMesh).MoveVertice(i * 4, (vector8 + vector10) / 2f - vector11 * (a + num4) * 0.25f - camPos);
				(sLeaser.sprites[ShineMeshSprite] as TriangleMesh).MoveVertice(i * 4 + 1, (vector8 + vector10) / 2f + vector11 * (a + num4) * 0.25f - camPos);
				(sLeaser.sprites[ShineMeshSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector10 - vector11 * a - camPos);
				(sLeaser.sprites[ShineMeshSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector10 + vector11 * a - camPos);
				if (i > 1)
				{
					Vector2 vector12 = vector9 - vector11 * num6;
					Vector2 vector13 = vector9 + vector11 * num6;
					sLeaser.sprites[SegmentSprite(i - 1)].x = vector12.x - camPos.x;
					sLeaser.sprites[SegmentSprite(i - 1)].y = vector12.y - camPos.y;
					sLeaser.sprites[SegmentSprite(i - 1)].rotation = Custom.AimFromOneVectorToAnother(vector12, vector13);
					sLeaser.sprites[SegmentSprite(i - 1)].scaleY = Vector2.Distance(vector12, vector13);
				}
			}
			vector7 = vector9;
			num3 = num6;
			vector8 = vector10;
			num4 = a;
		}
		vector7 = vector + vector4;
		num3 = 0f;
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				float t2 = Mathf.InverseLerp(0f, legs.GetLength(1) - 1, k);
				Vector2 vector14 = Vector2.Lerp(vector, vector2, 0.3f);
				vector14 += vector5 * ((j == 0) ? 1f : (-1f)) * 3f * (1f - Mathf.Abs(num));
				vector14 += vector4 * Mathf.Lerp(5f, -11f, t2);
				Vector2 vector15 = Vector2.Lerp(legs[j, k].lastPos, legs[j, k].pos, timeStacker);
				Vector2 vector16 = Vector2.Lerp(knees[j, k].lastPos, knees[j, k].pos, timeStacker);
				Vector2 vector17 = Vector2.Lerp(vector14, vector16, 0.5f);
				Vector2 vector18 = Vector2.Lerp(vector16, vector15, 0.5f);
				float num7 = 5f;
				Vector2 vector19 = Vector2.Lerp(vector17, vector18, 0.5f);
				vector17 = vector19 + Custom.DirVec(vector19, vector17) * num7 / 2f;
				vector18 = vector19 + Custom.DirVec(vector19, vector18) * num7 / 2f;
				vector7 = vector14;
				num3 = 2f;
				for (int l = 0; l < 12; l++)
				{
					float num8 = Mathf.InverseLerp(0f, 11f, l);
					Vector2 vector20 = ((!(num8 < 0.5f)) ? Custom.Bezier((vector18 + vector17) / 2f, vector18 + Custom.DirVec(vector17, vector18) * 7f, vector15, vector15 + Custom.DirVec(vector15, vector14) * 14f, Mathf.InverseLerp(0.5f, 1f, num8)) : Custom.Bezier(vector14, vector14 + Custom.DirVec(vector14, vector15) * 10f, (vector18 + vector17) / 2f, vector17 + Custom.DirVec(vector18, vector17) * 7f, Mathf.InverseLerp(0f, 0.5f, num8)));
					float num9 = (Mathf.Lerp(4f, 0.5f, Mathf.Pow(num8, 0.25f)) + Mathf.Sin(Mathf.Pow(num8, 2.5f) * (float)Math.PI) * 1.5f) * legsThickness;
					Vector2 vector21 = Custom.PerpendicularVector(vector20, vector7);
					(sLeaser.sprites[LegSprite(j, k)] as TriangleMesh).MoveVertice(l * 4, (vector7 + vector20) / 2f - vector21 * (num9 + num3) * 0.5f - camPos);
					(sLeaser.sprites[LegSprite(j, k)] as TriangleMesh).MoveVertice(l * 4 + 1, (vector7 + vector20) / 2f + vector21 * (num9 + num3) * 0.5f - camPos);
					(sLeaser.sprites[LegSprite(j, k)] as TriangleMesh).MoveVertice(l * 4 + 2, vector20 - vector21 * num9 - camPos);
					(sLeaser.sprites[LegSprite(j, k)] as TriangleMesh).MoveVertice(l * 4 + 3, vector20 + vector21 * num9 - camPos);
					vector7 = vector20;
					num3 = num9;
				}
			}
		}
		float num10 = Mathf.Lerp(lastPinchersFlip, pinchersFlip, timeStacker);
		Vector2 vector22 = Custom.PerpendicularVector(vector3, vector6);
		for (int m = 0; m < 2; m++)
		{
			float num11 = Mathf.Lerp((m == 0) ? 1f : (-1f), num, Mathf.Pow(Mathf.Abs(num), 2f));
			Vector2 vector23 = vector + vector4 * 4f + vector5 * num11 * -3f;
			Vector2 vector24 = Vector2.Lerp(mandibles[m].lastPos, mandibles[m].pos, timeStacker);
			Vector2 vector25 = Custom.InverseKinematic(vector23, vector24, 16f, 18f, num11);
			sLeaser.sprites[MandibleSprite(m, 0)].x = vector23.x - camPos.x;
			sLeaser.sprites[MandibleSprite(m, 0)].y = vector23.y - camPos.y;
			sLeaser.sprites[MandibleSprite(m, 0)].anchorY = 0f;
			sLeaser.sprites[MandibleSprite(m, 0)].rotation = Custom.AimFromOneVectorToAnother(vector23, vector25);
			sLeaser.sprites[MandibleSprite(m, 0)].scaleY = Vector2.Distance(vector23, vector25) / sLeaser.sprites[MandibleSprite(m, 0)].element.sourcePixelSize.y;
			sLeaser.sprites[MandibleSprite(m, 0)].scaleX = (0f - Mathf.Sign(num11)) * 2f;
			sLeaser.sprites[MandibleSprite(m, 1)].x = vector25.x - camPos.x;
			sLeaser.sprites[MandibleSprite(m, 1)].y = vector25.y - camPos.y;
			sLeaser.sprites[MandibleSprite(m, 1)].anchorY = 0f;
			sLeaser.sprites[MandibleSprite(m, 1)].rotation = Custom.AimFromOneVectorToAnother(vector25, vector24);
			sLeaser.sprites[MandibleSprite(m, 1)].scaleY = Vector2.Distance(vector25, vector24) / sLeaser.sprites[MandibleSprite(m, 0)].element.sourcePixelSize.y;
			sLeaser.sprites[MandibleSprite(m, 1)].scaleX = (0f - Mathf.Sign(num11)) * 1.2f;
			Vector2 vector26 = Custom.DegToVec(90f * num + ((m == 0) ? (-1f) : 1f) * 34f);
			if (vector26.y < 0f)
			{
				sLeaser.sprites[WingSprite(m)].isVisible = false;
			}
			else
			{
				sLeaser.sprites[WingSprite(m)].isVisible = true;
				Vector2 vector27 = Vector2.Lerp(vector, vector2, 0.2f) - vector5 * 6f * vector26.x;
				sLeaser.sprites[WingSprite(m)].x = vector27.x - camPos.x;
				sLeaser.sprites[WingSprite(m)].y = vector27.y - camPos.y;
				sLeaser.sprites[WingSprite(m)].rotation = Custom.VecToDeg(vector4);
				sLeaser.sprites[WingSprite(m)].scaleX = vector26.y * 0.4f;
				sLeaser.sprites[WingSprite(m)].scaleY = 0.8f;
				float val = Mathf.Abs(Vector2.Dot(vector4, Custom.DegToVec(Custom.VecToDeg(normalized) - 90f + (90f * num + ((m == 0) ? (-1f) : 1f) * 24f) * 0.4f)));
				sLeaser.sprites[WingSprite(m)].color = Color.Lerp(currSkinColor, shineColor, Mathf.Lerp(0.15f + Custom.LerpMap(val, 0.55f, 1f, 0f, 0.25f, 3f), 0.07f, t));
			}
			vector23 = Vector2.Lerp(vector3, vector6, 0.8f) + vector22 * ((m == 0) ? (-1f) : 1f) * 2f * (1f - Mathf.Abs(num10));
			vector25 = Vector2.Lerp(pinchers[m].lastPos, pinchers[m].pos, timeStacker);
			vector7 = vector6;
			num3 = 2f;
			for (int n = 0; n < 8; n++)
			{
				num11 = Mathf.InverseLerp(0f, 7f, n);
				Vector2 vector28 = Vector2.Lerp(vector23, vector25, num11);
				vector28 += Custom.PerpendicularVector(vector23, vector25) * ((m == 0) ? (-1f) : 1f) * Mathf.Sqrt(1f - Mathf.Pow(1f - num11 * 2f, 2f)) * Custom.LerpMap(num11, 0.1f, 0.4f, 0f, 15f) * (1f - Mathf.Abs(num10));
				vector28 += Custom.PerpendicularVector(vector23, vector25) * num10 * Mathf.InverseLerp(0.5f, 1f, num11) * -7f;
				float num12 = Mathf.Lerp(2.5f, 0.1f, num11) + Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(num11, Mathf.Lerp(3f, 1f, Mathf.Abs(num10))) * (float)Math.PI)), 3f) * Mathf.Lerp(2f, 1f, Mathf.Abs(num10));
				Vector2 vector29 = Custom.PerpendicularVector(vector28, vector7);
				(sLeaser.sprites[PincherSprite(m)] as TriangleMesh).MoveVertice(n * 4, (vector7 + vector28) / 2f - vector29 * (num12 + num3) * 0.5f - camPos);
				(sLeaser.sprites[PincherSprite(m)] as TriangleMesh).MoveVertice(n * 4 + 1, (vector7 + vector28) / 2f + vector29 * (num12 + num3) * 0.5f - camPos);
				(sLeaser.sprites[PincherSprite(m)] as TriangleMesh).MoveVertice(n * 4 + 2, vector28 - vector29 * num12 - camPos);
				(sLeaser.sprites[PincherSprite(m)] as TriangleMesh).MoveVertice(n * 4 + 3, vector28 + vector29 * num12 - camPos);
				vector7 = vector28;
				num3 = num12;
			}
			vector23 = vector;
			vector25 = Vector2.Lerp(antennae[m].lastPos, antennae[m].pos, timeStacker);
			Vector2 normalized2 = (Custom.DirVec(vector2, vector) + Custom.PerpendicularVector(vector2, vector) * num * 0.5f + Custom.PerpendicularVector(vector2, vector) * ((m == 0) ? (-1f) : 1f) * (1f - Mathf.Abs(num)) * 0.35f).normalized;
			vector7 = vector;
			num3 = 3f;
			for (int num13 = 0; num13 < 8; num13++)
			{
				num11 = Mathf.InverseLerp(0f, 7f, num13);
				Vector2 vector30 = Custom.Bezier(vector23, vector23 + normalized2 * 30f * antennaeLength, vector25, vector25, num11);
				Vector2 vector31 = Custom.PerpendicularVector(vector30, vector7);
				float num14 = Mathf.Lerp(1f, 0.5f, num11);
				(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(num13 * 4, (vector7 + vector30) / 2f - vector31 * (num3 + num14) * 0.5f - camPos);
				(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(num13 * 4 + 1, (vector7 + vector30) / 2f + vector31 * (num3 + num14) * 0.5f - camPos);
				(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(num13 * 4 + 2, vector30 - vector31 * num14 - camPos);
				(sLeaser.sprites[AntennaSprite(m)] as TriangleMesh).MoveVertice(num13 * 4 + 3, vector30 + vector31 * num14 - camPos);
				vector7 = vector30;
				num3 = num14;
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		shineColor = Color.Lerp(Custom.HSL2RGB(hue, 0.55f, 0.65f), palette.fogColor, 0.25f + 0.75f * Mathf.InverseLerp(0.5f, 1f, darkness));
		camoColor = Color.Lerp(palette.blackColor, Color.Lerp(palette.texture.GetPixel(4, 3), palette.fogColor, palette.fogAmount * (2f / 15f)), 0.5f);
		RefreshColor(0f, sLeaser);
	}

	private void RefreshColor(float timeStacker, RoomCamera.SpriteLeaser sLeaser)
	{
		float value = Mathf.Lerp(lastDeepCeilingMode, deepCeilingMode, timeStacker);
		float num = Mathf.Lerp(lastCeilingMode, ceilingMode, timeStacker);
		value = Custom.SCurve(Mathf.InverseLerp(0.1f, 0.5f, value), 0.4f);
		currSkinColor = Color.Lerp(blackColor, camoColor, value);
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = currSkinColor;
		}
		for (int j = 0; j < (sLeaser.sprites[ShineMeshSprite] as TriangleMesh).verticeColors.Length; j++)
		{
			(sLeaser.sprites[ShineMeshSprite] as TriangleMesh).verticeColors[j] = Color.Lerp(currSkinColor, shineColor, 0.25f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(0f, (sLeaser.sprites[ShineMeshSprite] as TriangleMesh).verticeColors.Length - 1, j), 4f) * (float)Math.PI)), 0.5f) * (1f - num));
		}
		if (!(coloredAntennae > 0f))
		{
			return;
		}
		for (int k = 0; k < 2; k++)
		{
			for (int l = 0; l < (sLeaser.sprites[AntennaSprite(k)] as TriangleMesh).verticeColors.Length; l++)
			{
				(sLeaser.sprites[AntennaSprite(k)] as TriangleMesh).verticeColors[l] = Color.Lerp(currSkinColor, shineColor, Mathf.InverseLerp((sLeaser.sprites[AntennaSprite(k)] as TriangleMesh).verticeColors.Length / 2, (sLeaser.sprites[AntennaSprite(k)] as TriangleMesh).verticeColors.Length - 1, l) * coloredAntennae * Mathf.Lerp(1f, 0.75f, value));
			}
		}
	}
}
