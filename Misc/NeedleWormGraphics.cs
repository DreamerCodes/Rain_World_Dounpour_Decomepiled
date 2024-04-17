using System;
using RWCustom;
using UnityEngine;

public class NeedleWormGraphics : GraphicsModule
{
	public NeedleWorm worm;

	public GenericBodyPart[] snout;

	public GenericBodyPart[,] wings;

	public GenericBodyPart[,] legs;

	private float wingFlap;

	private float lastWingFlap;

	private float flying;

	private float lastFlying;

	private Vector2 zRot;

	private Vector2 lastZrot;

	private int totLegs;

	private float fangOut;

	private float lastFangOut;

	private float fangBlack;

	public bool small;

	public float wingsSize;

	public float legsFac;

	public float hue;

	public float lightness;

	public float hueDiv;

	public float fatness;

	public float snoutLength;

	public float whiplash;

	public float lastwhiplash;

	private float thinTail;

	public ChunkDynamicSoundLoop soundLoop;

	private Color blackCol;

	private Color bodyColor;

	private Color highLightColor;

	private Color eyeColor;

	private Color detailsColor;

	private bool[] cosBools;

	public int FangMesh => 5;

	public int BodyMesh
	{
		get
		{
			if (!small)
			{
				return 6;
			}
			return 3;
		}
	}

	public int HighLightMesh => BodyMesh + 1 + totLegs;

	public int TotalSprites => WingSprite(1, 1) + 1;

	public int TotGraphSegments => worm.TotalSegments + snout.Length;

	public int HighLightSegments => worm.TotalSegments * 2 / 3;

	private float Eaten
	{
		get
		{
			if (!small)
			{
				return 1f;
			}
			return Custom.LerpMap((worm as SmallNeedleWorm).bites, 4f, 1f, 1f, 0.4f);
		}
	}

	public int LegSprite(int s, int i)
	{
		return BodyMesh + 1 + s * legs.GetLength(1) + i;
	}

	public int LumpSprite(int side, int i)
	{
		if (side != 0)
		{
			return HighLightMesh + 1 + i;
		}
		return 3 + i;
	}

	public int EyeSprite(int eye)
	{
		if (eye != 0)
		{
			return HighLightMesh + (small ? 1 : 3);
		}
		return 2;
	}

	public int WingSprite(int s, int i)
	{
		if (s == 0)
		{
			return i;
		}
		return EyeSprite(1) + 1 + i;
	}

	public NeedleWormGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		worm = ow as NeedleWorm;
		small = worm.small;
		if (small)
		{
			cullRange = 800f;
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(worm.abstractCreature.ID.RandomSeed);
		wingsSize = (small ? 0.5f : 1f) * Mathf.Lerp(0.8f, 1.2f, Custom.ClampedRandomVariation(0.5f, 0.5f, 0.4f));
		legsFac = (small ? 0.8f : 1f) * Mathf.Lerp(0.6f, 1.4f, UnityEngine.Random.value);
		fatness = Custom.ClampedRandomVariation(0.5f, 0.5f, 0.4f);
		snoutLength = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
		cosBools = new bool[4];
		hue = Custom.WrappedRandomVariation(0.5f, 0.08f, 0.2f);
		cosBools[0] = UnityEngine.Random.value < 0.5f;
		cosBools[1] = UnityEngine.Random.value < (cosBools[0] ? 0.25f : 0.75f);
		cosBools[2] = UnityEngine.Random.value < 0.5f;
		cosBools[3] = UnityEngine.Random.value < 0.25f;
		if (!cosBools[2] && cosBools[3])
		{
			cosBools[1] = false;
		}
		if (small)
		{
			lightness = Mathf.Lerp(0.3f, 1f, Mathf.Pow(Custom.ClampedRandomVariation(0.5f, 0.5f, 0.4f), 0.4f));
			cosBools[0] = false;
			cosBools[1] = false;
			cosBools[3] = true;
			hueDiv = 0f;
		}
		else
		{
			lightness = 0.4f;
			if (UnityEngine.Random.value < 1f / 3f)
			{
				lightness = 0.4f * Mathf.Pow(UnityEngine.Random.value, 5f);
			}
			else if (UnityEngine.Random.value < 1f / 17f)
			{
				lightness = 1f - 0.6f * Mathf.Pow(UnityEngine.Random.value, 5f);
			}
			hueDiv = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Custom.LerpMap(Mathf.Abs(0.5f - hue), 0f, 0.08f, 0.06f, 0.3f);
		}
		if (!small && lightness < 0.4f && UnityEngine.Random.value > lightness && UnityEngine.Random.value < 0.1f)
		{
			hue = UnityEngine.Random.value;
		}
		UnityEngine.Random.state = state;
		snout = new GenericBodyPart[small ? 3 : 5];
		wings = new GenericBodyPart[2, 2];
		legs = new GenericBodyPart[2, small ? 1 : 3];
		totLegs = legs.GetLength(0) * legs.GetLength(1);
		bodyParts = new BodyPart[snout.Length + 4 + totLegs];
		int num = 0;
		for (int i = 0; i < snout.Length; i++)
		{
			snout[i] = new GenericBodyPart(this, 1f, 0.5f, 0.99f, worm.bodyChunks[0]);
			bodyParts[num] = snout[i];
			num++;
		}
		for (int j = 0; j < wings.GetLength(0); j++)
		{
			for (int k = 0; k < wings.GetLength(1); k++)
			{
				wings[j, k] = new GenericBodyPart(this, 2f, 0.5f, 0.95f, worm.bodyChunks[small ? k : (1 + k)]);
				bodyParts[num] = wings[j, k];
				num++;
			}
		}
		for (int l = 0; l < legs.GetLength(0); l++)
		{
			for (int m = 0; m < legs.GetLength(1); m++)
			{
				legs[l, m] = new GenericBodyPart(this, 1f, 0.5f, 0.999f, worm.bodyChunks[small ? 1 : (m + 1)]);
				bodyParts[num] = legs[l, m];
				num++;
			}
		}
		whiplash = -1f;
		lastwhiplash = -1f;
		soundLoop = new ChunkDynamicSoundLoop(worm.mainBodyChunk);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void Update()
	{
		base.Update();
		soundLoop.Update();
		if (!worm.small && (worm as BigNeedleWorm).swishDir.HasValue)
		{
			soundLoop.sound = SoundID.Big_Needle_Worm_Swish_Through_Air_LOOP;
			soundLoop.Volume = 1f;
			soundLoop.Pitch = 1f;
		}
		else if (flying == 0f)
		{
			soundLoop.sound = SoundID.None;
		}
		else if (worm.small)
		{
			soundLoop.sound = SoundID.Small_Needle_Worm_Wings_LOOP;
			soundLoop.Volume = worm.flying;
			soundLoop.Pitch = 0.5f + 0.5f * worm.flying;
		}
		else if ((worm as BigNeedleWorm).chargingAttack > 0f)
		{
			soundLoop.sound = SoundID.Big_Needle_Worm_Attack_Charge_Wings_LOOP;
			soundLoop.Volume = 1f;
			soundLoop.Pitch = 0.8f + 0.5f * (worm as BigNeedleWorm).chargingAttack;
		}
		else
		{
			soundLoop.sound = SoundID.Big_Needle_Worm_Wings_LOOP;
			soundLoop.Volume = worm.flying;
			soundLoop.Pitch = 0.5f + 0.5f * worm.flying;
		}
		Vector2 vector = Custom.DirVec(worm.bodyChunks[worm.bodyChunks.Length - 1].pos, worm.bodyChunks[0].pos);
		lastZrot = zRot;
		zRot = vector;
		lastWingFlap = wingFlap;
		wingFlap += (0.4f + UnityEngine.Random.value * 0.05f) * flying;
		lastFlying = flying;
		flying = worm.flying;
		if (!small)
		{
			lastFangOut = fangOut;
			if ((worm as BigNeedleWorm).swishDir.HasValue || (worm as BigNeedleWorm).stuckInWallPos.HasValue || (worm as BigNeedleWorm).impaleChunk != null)
			{
				fangOut = 1f;
			}
			else
			{
				fangOut = Custom.SCurve(Mathf.InverseLerp(0f, 0.75f, (worm as BigNeedleWorm).attackReady), 0.6f);
			}
			if ((worm as BigNeedleWorm).swishDir.HasValue)
			{
				thinTail = 1f;
			}
			else
			{
				thinTail = Custom.LerpAndTick(thinTail, Mathf.InverseLerp(0.5f, 1f, (worm as BigNeedleWorm).chargingAttack), 0.07f, 0.05f);
			}
			if (fangOut == 0f && lastFangOut == 0f)
			{
				fangBlack = 0f;
			}
			else
			{
				fangBlack = Custom.LerpAndTick(fangBlack, Mathf.InverseLerp(0.5f, 1f, fangOut), 0.002f, 0.0038461538f);
			}
		}
		Vector2 vector2 = Custom.DirVec(worm.bodyChunks[1].pos, worm.mainBodyChunk.pos);
		Vector2 vector3 = worm.mainBodyChunk.pos + vector2 * (worm.mainBodyChunk.rad + 5f);
		for (int i = 0; i < snout.Length; i++)
		{
			float f = Mathf.InverseLerp(0f, snout.Length - 1, i);
			snout[i].Update();
			snout[i].vel += (Vector2)Vector3.Slerp(vector2 * 3f, new Vector2(0f, -0.9f), Mathf.Pow(f, 0.6f) * Mathf.InverseLerp(0.25f, 0f, fangOut) * (1f - worm.screaming));
		}
		if (worm.screaming > 0f)
		{
			for (int j = 0; j < snout.Length; j++)
			{
				snout[j].vel += Custom.RNV();
				snout[j].pos += Custom.RNV();
			}
		}
		float num = (3f + 2f * Mathf.InverseLerp(0.5f, 0f, fangOut)) * snoutLength;
		Vector2 vector4 = Custom.DirVec(snout[0].pos, vector3);
		float num2 = Vector2.Distance(snout[0].pos, vector3);
		snout[0].vel += vector4 * (num2 - num);
		snout[0].pos += vector4 * (num2 - num);
		for (int k = 1; k < snout.Length; k++)
		{
			vector4 = Custom.DirVec(snout[k].pos, snout[k - 1].pos);
			num2 = Vector2.Distance(snout[k].pos, snout[k - 1].pos);
			snout[k].vel += vector4 * (num2 - num) * 0.5f;
			snout[k].pos += vector4 * (num2 - num) * 0.5f;
			snout[k - 1].vel -= vector4 * (num2 - num) * 0.5f;
			snout[k - 1].pos -= vector4 * (num2 - num) * 0.5f;
		}
		if (worm.small)
		{
			if ((worm as SmallNeedleWorm).HangingOnMother != null)
			{
				snout[snout.Length - 1].vel *= 0f;
				snout[snout.Length - 1].pos = (worm as SmallNeedleWorm).HangingOnMother.GetSegmentPos((worm as SmallNeedleWorm).HangingOnMother.bodyChunks.Length + (worm as SmallNeedleWorm).momTailSegment);
			}
		}
		else if (fangOut > 0f)
		{
			for (int l = 0; l < snout.Length; l++)
			{
				snout[l].pos = Vector2.Lerp(snout[l].pos, worm.mainBodyChunk.pos + vector2 * l * 3f, Mathf.InverseLerp(0f, 0.5f, fangOut));
				snout[l].vel *= Mathf.InverseLerp(0.5f, 0f, fangOut);
			}
		}
		for (int m = 0; m < legs.GetLength(0); m++)
		{
			for (int n = 0; n < legs.GetLength(1); n++)
			{
				legs[m, n].Update();
				legs[m, n].ConnectToPoint(LegConPos(m, n, 1f), ((n == 1) ? 16f : 11f) * legsFac, push: false, 0f, legs[m, n].connection.vel, 0f, 0f);
				legs[m, n].vel.y -= 0.9f;
				legs[m, n].vel += LegConDir(m, n, 1f) * 0.55f;
				if (worm.Consious && UnityEngine.Random.value < 1f / 9f)
				{
					legs[m, n].vel += Custom.RNV() * UnityEngine.Random.value * 2f * legsFac;
				}
			}
		}
		for (int num3 = 0; num3 < wings.GetLength(0); num3++)
		{
			for (int num4 = 0; num4 < wings.GetLength(1); num4++)
			{
				wings[num3, num4].Update();
				wings[num3, num4].ConnectToPoint(wings[num3, num4].connection.pos, 30f * wingsSize, push: true, (worm is BigNeedleWorm && (worm as BigNeedleWorm).swishCounter > 0) ? 0.6f : 0f, wings[num3, num4].connection.vel, 0.3f, 0.1f);
				Vector2 segmentDir = worm.GetSegmentDir(wings[num3, num4].connection.index, 1f);
				if (flying > 0f)
				{
					Vector2 b = Custom.DegToVec((90f + ((num4 == 0) ? (-45f) : 45f) * vector.y) * ((num3 == 0) ? (-1f) : 1f) * Mathf.Sign(vector.y));
					b.y = Mathf.Lerp(b.y, Mathf.Abs(b.y), 0.4f);
					Vector2 a = Custom.DegToVec(Custom.VecToDeg(segmentDir) + (90f + ((num4 == 0) ? (-45f) : 45f)) * ((num3 == 0) ? (-1f) : 1f));
					a.y = Mathf.Abs(a.y);
					Vector2 vector5 = Vector2.Lerp(a, b, Mathf.Abs(vector.y) * 0.6f).normalized;
					if (worm is BigNeedleWorm)
					{
						vector5 = Vector3.Slerp(vector5, vector, 0.7f * Mathf.Pow((worm as BigNeedleWorm).chargingAttack, 0.5f));
					}
					wings[num3, num4].vel += vector5 * 10f * wingsSize * flying;
				}
				if (worm.Consious)
				{
					wings[num3, num4].vel -= (segmentDir + Custom.PerpendicularVector(segmentDir) * ((num3 == 0) ? (-1f) : 1f) * 0.1f) * (1f - flying) * 4f * wingsSize;
				}
				else if (worm.room.PointSubmerged(wings[num3, num4].pos))
				{
					wings[num3, num4].vel.y -= 0.2f;
				}
			}
		}
		if (small)
		{
			return;
		}
		lastwhiplash = whiplash;
		if (whiplash == -1f && (worm as BigNeedleWorm).chargingAttack > 0.7f)
		{
			whiplash += 0.01f;
		}
		if (whiplash > -1f)
		{
			whiplash += 0.1f;
			if (whiplash > 2f && lastwhiplash > 2f)
			{
				whiplash = -1f;
				lastwhiplash = -1f;
			}
		}
	}

	public Vector2 LegConPos(int s, int i, float timeStacker)
	{
		float f = Custom.LerpMap(i, 0f, 2f, 0.03f, 0.1f, 2f);
		return worm.OnBodyPos(f, timeStacker) + Custom.PerpendicularVector(worm.OnBodyDir(f, timeStacker)) * worm.OnBodyRad(f) * ((s == 0) ? (-1f) : 1f) * Vector3.Slerp(lastZrot, zRot, timeStacker).y;
	}

	public Vector2 LegConBodyDir(int s, int i, float timeStacker)
	{
		float f = Custom.LerpMap(i, 0f, 2f, 0.03f, 0.1f, 2f);
		return worm.OnBodyDir(f, timeStacker);
	}

	public Vector2 LegConDir(int s, int i, float timeStacker)
	{
		return Custom.PerpendicularVector(LegConBodyDir(s, i, timeStacker)) * ((s == 0) ? (-1f) : 1f) * Vector3.Slerp(lastZrot, zRot, timeStacker).y;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		if (!small)
		{
			sLeaser.sprites[FangMesh] = TriangleMesh.MakeLongMesh(5, pointyTip: true, customColor: true);
		}
		sLeaser.sprites[BodyMesh] = TriangleMesh.MakeLongMesh(TotGraphSegments, pointyTip: true, !small);
		sLeaser.sprites[HighLightMesh] = TriangleMesh.MakeLongMesh(HighLightSegments, pointyTip: true, customColor: true);
		for (int i = 0; i < wings.GetLength(0); i++)
		{
			sLeaser.sprites[EyeSprite(i)] = new FSprite("JetFishEyeB");
			for (int j = 0; j < wings.GetLength(1); j++)
			{
				sLeaser.sprites[WingSprite(i, j)] = new CustomFSprite("CentipedeWing");
				sLeaser.sprites[WingSprite(i, j)].shader = rCam.room.game.rainWorld.Shaders["CicadaWing"];
				if (!small)
				{
					sLeaser.sprites[LumpSprite(i, j)] = new FSprite("JetFishEyeB");
				}
			}
		}
		for (int k = 0; k < legs.GetLength(0); k++)
		{
			for (int l = 0; l < legs.GetLength(1); l++)
			{
				sLeaser.sprites[LegSprite(k, l)] = new CustomFSprite("JetFishFlipper3");
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
		float num = Mathf.Lerp(lastFlying, flying, timeStacker);
		Vector2 vector = Custom.DirVec(Vector2.Lerp(worm.bodyChunks[worm.bodyChunks.Length / 2].lastPos, worm.bodyChunks[worm.bodyChunks.Length / 2].pos, timeStacker), Vector2.Lerp(worm.bodyChunks[0].lastPos, worm.bodyChunks[0].pos, timeStacker));
		Vector2 vector2 = Vector3.Slerp(lastZrot, zRot, timeStacker);
		Vector2 segmentPos = worm.GetSegmentPos(0, timeStacker);
		Vector2 vector3 = Custom.DirVec(segmentPos, Vector2.Lerp(snout[1].lastPos, snout[1].pos, timeStacker));
		segmentPos = Vector2.Lerp(segmentPos, Vector2.Lerp(snout[0].lastPos, snout[0].pos, timeStacker), 0.4f);
		float t = Mathf.InverseLerp(0f, 0.7f, Vector2.Dot(vector3, vector));
		for (int i = 0; i < 2; i++)
		{
			Vector2 vector4 = segmentPos + Custom.PerpendicularVector(vector3) * 4f * (small ? 0.65f : 1f) * ((i == 0 != vector3.x < 0f) ? (-1f) : 1f) * vector3.y;
			sLeaser.sprites[EyeSprite(i)].x = vector4.x - camPos.x;
			sLeaser.sprites[EyeSprite(i)].y = vector4.y - camPos.y;
			sLeaser.sprites[EyeSprite(i)].scaleX = Mathf.Lerp(0.8f, 0.6f, t) * (small ? 0.65f : 1f);
			sLeaser.sprites[EyeSprite(i)].scaleY = Mathf.Lerp(1.1f, 1.5f, t) * (small ? 0.65f : 1f);
			sLeaser.sprites[EyeSprite(i)].rotation = Custom.VecToDeg((vector3 + vector).normalized);
		}
		Vector2 vector5 = GraphSegmentPos(0, timeStacker);
		float num2 = GraphSegmentRad(0);
		float num3 = 1f;
		if (!small)
		{
			num3 *= Custom.LerpMap(Vector2.Distance(GraphSegmentPos(snout.Length, timeStacker), GraphSegmentPos(snout.Length + worm.bodyChunks.Length - 1, timeStacker)), 50f, 80f, 1f, 0.85f);
		}
		for (int j = 0; j < TotGraphSegments; j++)
		{
			Vector2 vector6 = GraphSegmentPos(j, timeStacker);
			if (small && j < snout.Length && (worm as SmallNeedleWorm).HangingOnMother != null)
			{
				vector6 = Vector2.Lerp(vector6, (worm as SmallNeedleWorm).HangingOnMother.GetSegmentPos((worm as SmallNeedleWorm).HangingOnMother.bodyChunks.Length + (worm as SmallNeedleWorm).momTailSegment, timeStacker), Mathf.InverseLerp(snout.Length - 1, 0f, j));
			}
			float num4 = GraphSegmentRad(j) * num3;
			if (worm.screaming > 0f)
			{
				num4 += Mathf.Pow(worm.screaming, 0.7f) * 4f * UnityEngine.Random.value * Mathf.InverseLerp(snout.Length + worm.bodyChunks.Length, snout.Length, j);
			}
			Vector2 normalized = (vector6 - vector5).normalized;
			Vector2 vector7 = Custom.PerpendicularVector(normalized);
			float num5 = Vector2.Distance(vector6, vector5) / 5f;
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4, vector5 - vector7 * (num2 + num4) * 0.5f + normalized * num5 - camPos);
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4 + 1, vector5 + vector7 * (num2 + num4) * 0.5f + normalized * num5 - camPos);
			if (j < TotGraphSegments - 1)
			{
				(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4 + 2, vector6 - vector7 * num4 - normalized * num5 - camPos);
				(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4 + 3, vector6 + vector7 * num4 - normalized * num5 - camPos);
			}
			else
			{
				(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4 + 2, vector6 - camPos);
			}
			num2 = num4;
			vector5 = vector6;
		}
		vector5 = GraphSegmentPos(snout.Length - 1, timeStacker);
		num2 = 0f;
		for (int k = 0; k < HighLightSegments; k++)
		{
			Vector2 vector8 = GraphSegmentPos(k + snout.Length, timeStacker);
			float num6 = GraphSegmentRad(k + snout.Length);
			vector8 += new Vector2(-1f, 1f) * num6 / 3f;
			num6 /= 3.2f;
			Vector2 normalized2 = (vector8 - vector5).normalized;
			Vector2 vector9 = Custom.PerpendicularVector(normalized2);
			float num7 = Vector2.Distance(vector8, vector5) / 5f;
			(sLeaser.sprites[HighLightMesh] as TriangleMesh).MoveVertice(k * 4, vector5 - vector9 * (num2 + num6) * 0.5f + normalized2 * num7 - camPos);
			(sLeaser.sprites[HighLightMesh] as TriangleMesh).MoveVertice(k * 4 + 1, vector5 + vector9 * (num2 + num6) * 0.5f + normalized2 * num7 - camPos);
			if (k < HighLightSegments - 1)
			{
				(sLeaser.sprites[HighLightMesh] as TriangleMesh).MoveVertice(k * 4 + 2, vector8 - vector9 * num6 - normalized2 * num7 - camPos);
				(sLeaser.sprites[HighLightMesh] as TriangleMesh).MoveVertice(k * 4 + 3, vector8 + vector9 * num6 - normalized2 * num7 - camPos);
			}
			else
			{
				(sLeaser.sprites[HighLightMesh] as TriangleMesh).MoveVertice(k * 4 + 2, vector8 - camPos);
			}
			num2 = num6;
			vector5 = vector8;
		}
		for (int l = 0; l < wings.GetLength(0); l++)
		{
			for (int m = 0; m < wings.GetLength(1); m++)
			{
				Vector2 vector10 = GraphSegmentPos(wings[l, m].connection.index + snout.Length, timeStacker);
				if (small && m == 0)
				{
					vector10 = GraphSegmentPos(1 + snout.Length, timeStacker);
				}
				Vector2 p = Vector2.Lerp(wings[l, m].lastPos, wings[l, m].pos, timeStacker);
				Vector2 segmentDir = worm.GetSegmentDir(wings[l, m].connection.index, timeStacker);
				vector10 -= Custom.PerpendicularVector(segmentDir) * wingsSize * 5f * Mathf.Abs(vector2.y) * ((l == 0) ? (-1f) : 1f);
				p.y -= (18f + 18f * Mathf.Sin((Mathf.Lerp(lastWingFlap, wingFlap, timeStacker) + ((m == 0) ? 0.33f : 0f)) * (float)Math.PI * 2f)) * num * wingsSize;
				p = vector10 + Custom.DirVec(vector10, p) * Mathf.Lerp(40f, 60f, num) * wingsSize;
				Vector2 vector11 = Vector3.Slerp(Custom.PerpendicularVector(segmentDir) * ((l == 0) ? (-1f) : 1f), new Vector2((l == 0) ? (-1f) : 1f, 0f), num);
				int num8 = ((l == 0 != vector2.x > 0f) ? 1 : 0);
				(sLeaser.sprites[WingSprite(num8, m)] as CustomFSprite).MoveVertice(1, p + vector11 * 2f * wingsSize - camPos);
				(sLeaser.sprites[WingSprite(num8, m)] as CustomFSprite).MoveVertice(0, p - vector11 * 2f * wingsSize - camPos);
				(sLeaser.sprites[WingSprite(num8, m)] as CustomFSprite).MoveVertice(2, vector10 + vector11 * 2f * wingsSize - camPos);
				(sLeaser.sprites[WingSprite(num8, m)] as CustomFSprite).MoveVertice(3, vector10 - vector11 * 2f * wingsSize - camPos);
				sLeaser.sprites[WingSprite(num8, m)].isVisible = !small || (worm as SmallNeedleWorm).bites > 4;
				if (!small)
				{
					vector10 += Custom.DirVec(vector10, p) * 0.5f;
					sLeaser.sprites[LumpSprite(num8, m)].x = vector10.x - camPos.x;
					sLeaser.sprites[LumpSprite(num8, m)].y = vector10.y - camPos.y;
					sLeaser.sprites[LumpSprite(num8, m)].scaleX = 0.9f;
					sLeaser.sprites[LumpSprite(num8, m)].scaleY = 1.2f;
					sLeaser.sprites[LumpSprite(num8, m)].rotation = Custom.VecToDeg(segmentDir);
					sLeaser.sprites[LumpSprite(num8, m)].color = ((num8 == 0) ? bodyColor : Color.Lerp(bodyColor, highLightColor, Mathf.Abs(vector2.x) * 0.6f));
				}
			}
		}
		for (int n = 0; n < legs.GetLength(0); n++)
		{
			for (int num9 = 0; num9 < legs.GetLength(1); num9++)
			{
				Vector2 vector12 = LegConPos(n, num9, timeStacker);
				Vector2 vector13 = Vector2.Lerp(legs[n, num9].lastPos, legs[n, num9].pos, timeStacker);
				Vector2 vector14 = Custom.PerpendicularVector(vector12, vector13);
				float f = Custom.DistanceToLine(vector13, vector12, vector12 - LegConBodyDir(n, num9, timeStacker));
				(sLeaser.sprites[LegSprite(n, num9)] as CustomFSprite).MoveVertice(3, vector13 - camPos);
				(sLeaser.sprites[LegSprite(n, num9)] as CustomFSprite).MoveVertice(2, (vector12 + vector13) / 2f + vector14 * 5f * legsFac * Mathf.Sign(f) - camPos);
				(sLeaser.sprites[LegSprite(n, num9)] as CustomFSprite).MoveVertice(1, vector12 - camPos);
				(sLeaser.sprites[LegSprite(n, num9)] as CustomFSprite).MoveVertice(0, (vector12 + vector13) / 2f - vector14 * 5f * legsFac * Mathf.Sign(f) - camPos);
				(sLeaser.sprites[LegSprite(n, num9)] as CustomFSprite).verticeColors[2] = bodyColor;
				(sLeaser.sprites[LegSprite(n, num9)] as CustomFSprite).verticeColors[3] = Color.Lerp(bodyColor, detailsColor, Mathf.InverseLerp(5f * legsFac, 9f * legsFac, Mathf.Abs(f)));
				(sLeaser.sprites[LegSprite(n, num9)] as CustomFSprite).verticeColors[0] = bodyColor;
				(sLeaser.sprites[LegSprite(n, num9)] as CustomFSprite).verticeColors[1] = bodyColor;
			}
		}
		if (small)
		{
			return;
		}
		Vector2? vector15 = null;
		if ((worm as BigNeedleWorm).stuckInWallPos.HasValue)
		{
			vector15 = (worm as BigNeedleWorm).stuckInWallPos.Value + (worm as BigNeedleWorm).stuckDir * (worm as BigNeedleWorm).fangLength * 1.2f;
		}
		else if ((worm as BigNeedleWorm).impaleChunk != null)
		{
			vector15 = Vector2.Lerp((worm as BigNeedleWorm).impaleChunk.lastPos, (worm as BigNeedleWorm).impaleChunk.pos, timeStacker);
		}
		float num10 = Mathf.InverseLerp(0.5f, 1f, Mathf.Lerp(lastFangOut, fangOut, timeStacker));
		if (num10 > 0f)
		{
			sLeaser.sprites[FangMesh].isVisible = true;
			Color a = Color.Lerp(new Color(1f, 0f, 0f), blackCol, Mathf.Pow(fangBlack, 3f));
			Color b = Color.Lerp(new Color(1f, 1f, 1f), blackCol, Mathf.Pow(Mathf.InverseLerp(0.4f, 0.55f, fangBlack), 0.8f));
			float p2 = 4f - 3.95f * (num10 * 3f + fangBlack) * 0.25f;
			for (int num11 = 0; num11 < (sLeaser.sprites[FangMesh] as TriangleMesh).verticeColors.Length; num11++)
			{
				float value = Mathf.InverseLerp(0f, (sLeaser.sprites[FangMesh] as TriangleMesh).verticeColors.Length - 1, num11);
				(sLeaser.sprites[FangMesh] as TriangleMesh).verticeColors[num11] = Color.Lerp(a, b, Mathf.Pow(Mathf.InverseLerp(0.1f, 0.35f + 0.65f * fangBlack, value), p2));
			}
			Vector2 vector16 = Custom.DirVec(worm.GetSegmentPos(1, timeStacker), worm.GetSegmentPos(0, timeStacker));
			vector5 = worm.GetSegmentPos(0, timeStacker);
			num2 = 1f;
			for (int num12 = 0; num12 < 5; num12++)
			{
				float num13 = Mathf.InverseLerp(0f, 4f, num12);
				vector16 = (vector16 + Custom.PerpendicularVector(vector16) * Mathf.Sin(num10 * (float)Math.PI) * Mathf.Lerp(-0.3f + 1.3f * Mathf.Pow(num13, 0.5f), num13, num10) * -0.2f * vector2.x).normalized;
				Vector2 vector17 = vector5 + vector16 * ((worm as BigNeedleWorm).fangLength / 3.5f) * Mathf.Pow(num10, 0.8f);
				if (vector15.HasValue)
				{
					vector17 = Vector2.Lerp(vector17, Vector2.Lerp(worm.GetSegmentPos(0, timeStacker), vector15.Value, num13), num13);
				}
				float num14 = Mathf.Lerp(0.6f + 0.6f * fangBlack, 0.5f, num13);
				Vector2 normalized3 = (vector17 - vector5).normalized;
				Vector2 vector18 = Custom.PerpendicularVector(normalized3);
				float num15 = Vector2.Distance(vector17, vector5) / 5f;
				(sLeaser.sprites[FangMesh] as TriangleMesh).MoveVertice(num12 * 4, vector5 - vector18 * (num2 + num14) * 0.5f + normalized3 * num15 - camPos);
				(sLeaser.sprites[FangMesh] as TriangleMesh).MoveVertice(num12 * 4 + 1, vector5 + vector18 * (num2 + num14) * 0.5f + normalized3 * num15 - camPos);
				if (num12 < 4)
				{
					(sLeaser.sprites[FangMesh] as TriangleMesh).MoveVertice(num12 * 4 + 2, vector17 - vector18 * num14 - normalized3 * num15 - camPos);
					(sLeaser.sprites[FangMesh] as TriangleMesh).MoveVertice(num12 * 4 + 3, vector17 + vector18 * num14 - normalized3 * num15 - camPos);
				}
				else
				{
					(sLeaser.sprites[FangMesh] as TriangleMesh).MoveVertice(num12 * 4 + 2, vector17 - camPos);
				}
				num2 = num14;
				vector5 = vector17;
			}
		}
		else
		{
			sLeaser.sprites[FangMesh].isVisible = false;
		}
	}

	private Vector2 GraphSegmentPos(int i, float timeStacker)
	{
		if (whiplash > -1f && lastwhiplash > -1f)
		{
			return WhiplashGraphSegmentPos(i, timeStacker);
		}
		if (Eaten < 1f)
		{
			return EatenGraphSegmentPos(i, timeStacker);
		}
		if (i < snout.Length)
		{
			return Vector2.Lerp(snout[snout.Length - 1 - i].lastPos, snout[snout.Length - 1 - i].pos, timeStacker);
		}
		return worm.GetSegmentPos(i - snout.Length, timeStacker);
	}

	private Vector2 WhiplashGraphSegmentPos(int i, float timeStacker)
	{
		if (i < snout.Length)
		{
			return Vector2.Lerp(snout[snout.Length - 1 - i].lastPos, snout[snout.Length - 1 - i].pos, timeStacker);
		}
		float num = Mathf.Lerp(lastwhiplash, whiplash, timeStacker);
		Vector2 segmentPos = worm.GetSegmentPos(i - snout.Length, timeStacker);
		float num2 = Mathf.InverseLerp(0f, worm.TotalSegments, i - snout.Length);
		float num3 = (Mathf.Pow(Mathf.Clamp01(Mathf.Sin(num2 * (float)Math.PI)), 0.5f) + 0.1f * num2) * Mathf.Lerp(30f, 80f, num2);
		return segmentPos + Custom.PerpendicularVector(worm.GetSegmentDir(i - snout.Length, timeStacker)) * Mathf.Pow(Mathf.InverseLerp(0.4f + Mathf.Clamp01(num) * 0.4f, 0f, Mathf.Abs(num2 - num)), 2f) * Mathf.Sin((num * 1.5f + num2) * (float)Math.PI * (2f + Mathf.Clamp01(num) * 6f)) * num3;
	}

	private Vector2 EatenGraphSegmentPos(int i, float timeStacker)
	{
		if (Mathf.InverseLerp(0f, TotGraphSegments - 1, i) > Eaten)
		{
			i = Custom.IntClamp(Mathf.RoundToInt(Eaten * (float)(TotGraphSegments - 1)), 0, TotGraphSegments - 1);
		}
		if (i < snout.Length)
		{
			return Vector2.Lerp(snout[snout.Length - 1 - i].lastPos, snout[snout.Length - 1 - i].pos, timeStacker);
		}
		return worm.GetSegmentPos(i - snout.Length, timeStacker);
	}

	private float GraphSegmentRad(int i)
	{
		if (i >= snout.Length)
		{
			if (i - snout.Length < worm.bodyChunks.Length)
			{
				return worm.GetSegmentRadForCollision(i - snout.Length) * Mathf.Lerp(0.75f, 1.35f, fatness);
			}
			return Custom.LerpMap(i - snout.Length, worm.bodyChunks.Length - 1, worm.TotalSegments, worm.GetSegmentRadForCollision(worm.bodyChunks.Length - 1) * Mathf.Lerp(0.75f, 1.35f, fatness), 0.7f, (1.4f - 1.35f * thinTail) * Mathf.Lerp(1.8f, 0.2f, fatness));
		}
		return 1f + Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, fangOut), 0.6f) * 1.5f;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackCol = palette.blackColor;
		float num = hue + 0.478f;
		bodyColor = Custom.HSL2RGB(num, Custom.LerpMap(lightness, 0.5f, 1f, 0.9f, 0.5f), Mathf.Lerp(0.1f, 0.8f, Mathf.Pow(lightness, 2f)));
		highLightColor = Custom.HSL2RGB(num, Custom.LerpMap(lightness, 0.5f, 1f, 0.5f, 1f), Mathf.Lerp(0.2f, 1f, lightness));
		num += Mathf.InverseLerp(0.5f, 0.6f, lightness) * 0.5f;
		if (cosBools[2])
		{
			eyeColor = Custom.HSL2RGB(num + 0.5f - hueDiv, 1f, Mathf.Lerp(0.7f, 0.3f, Mathf.Pow(lightness, 1.5f)));
			detailsColor = Custom.HSL2RGB(num + 0.5f + hueDiv, 0.8f, 0.4f);
		}
		else
		{
			eyeColor = Custom.HSL2RGB(num + 0.5f, 1f, Mathf.Lerp(0.7f, 0.3f, Mathf.Pow(lightness, 1.5f)));
			if (cosBools[3])
			{
				detailsColor = Custom.HSL2RGB(num + 0.5f, 1f, 0.5f);
			}
			else
			{
				detailsColor = Custom.HSL2RGB(num, 1f, 0.5f);
			}
		}
		if (lightness < 0.5f)
		{
			bodyColor = Color.Lerp(bodyColor, palette.blackColor, Mathf.Pow(Mathf.InverseLerp(0.5f, 0f, lightness), 0.5f));
			highLightColor = Color.Lerp(highLightColor, Color.Lerp(palette.blackColor, palette.fogColor, 0.4f), Mathf.Pow(Mathf.InverseLerp(0.5f, 0f, lightness), 2f));
		}
		else if (lightness > 0.5f)
		{
			bodyColor = Color.Lerp(bodyColor, palette.fogColor, Mathf.InverseLerp(0.5f, 1f, lightness) * 0.2f);
			highLightColor = Color.Lerp(bodyColor, new Color(1f, 1f, 1f), Mathf.InverseLerp(0.5f, 1f, lightness));
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = bodyColor;
		}
		for (int j = 0; j < 2; j++)
		{
			sLeaser.sprites[EyeSprite(j)].color = eyeColor;
		}
		for (int k = 0; k < (sLeaser.sprites[HighLightMesh] as TriangleMesh).verticeColors.Length; k++)
		{
			float f = Mathf.InverseLerp(0f, (sLeaser.sprites[HighLightMesh] as TriangleMesh).verticeColors.Length - 1, k);
			(sLeaser.sprites[HighLightMesh] as TriangleMesh).verticeColors[k] = Color.Lerp(bodyColor, highLightColor, Mathf.Sin(Mathf.Pow(f, 0.4f) * (float)Math.PI));
		}
		if (!small)
		{
			for (int l = 0; l < (sLeaser.sprites[BodyMesh] as TriangleMesh).verticeColors.Length; l++)
			{
				float num2 = Mathf.InverseLerp(0f, (sLeaser.sprites[BodyMesh] as TriangleMesh).verticeColors.Length - 1, l);
				(sLeaser.sprites[BodyMesh] as TriangleMesh).verticeColors[l] = Color.Lerp(bodyColor, Color.Lerp(detailsColor, palette.blackColor, cosBools[0] ? (Mathf.Pow(num2, 2f) * 0.85f) : 1f), Mathf.Pow(Mathf.InverseLerp(0.3f, 1f, num2), 2f) * (cosBools[0] ? 1f : 0.6f));
			}
		}
		for (int m = 0; m < wings.GetLength(0); m++)
		{
			for (int n = 0; n < wings.GetLength(1); n++)
			{
				(sLeaser.sprites[WingSprite(m, n)] as CustomFSprite).verticeColors[2] = Color.Lerp(palette.fogColor, detailsColor, 0.5f);
				(sLeaser.sprites[WingSprite(m, n)] as CustomFSprite).verticeColors[3] = Color.Lerp(palette.fogColor, detailsColor, 0.5f);
				(sLeaser.sprites[WingSprite(m, n)] as CustomFSprite).verticeColors[0] = Color.Lerp(cosBools[1] ? eyeColor : palette.fogColor, new Color(1f, 1f, 1f), cosBools[1] ? 0.35f : 0.5f);
				(sLeaser.sprites[WingSprite(m, n)] as CustomFSprite).verticeColors[1] = Color.Lerp(cosBools[1] ? eyeColor : palette.fogColor, new Color(1f, 1f, 1f), cosBools[1] ? 0.35f : 0.5f);
			}
		}
	}
}
