using System.Collections.Generic;
using Noise;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class BigJellyFish : Creature, IDrawable
{
	public Vector2[][,] tentacles;

	public bool anyTentaclePulled;

	public float tentaclesWithdrawn;

	public Vector2 rotation;

	public Vector2 lastRotation;

	private Color color;

	public float darkness;

	public float lastDarkness;

	private BodyChunk[] latchOnToBodyChunks;

	private float[] tentacleScaler;

	private int CoreChunk;

	private Vector2 StartPos;

	private Vector2? huntPos;

	private int huntingCounter;

	private List<Creature> consumedCreatures;

	private int leftHoodChunk;

	private int rightHoodChunk;

	private float hoodPulse;

	private float hoodSwayingPulse;

	private bool surfaceMode;

	private float minTGap;

	private float maxTGap;

	private float[] mouthBeads;

	private Color coreColor;

	private Color coreColorDark;

	private Vector2 driftGoalPos;

	private float driftCounter;

	private float driftMaxim;

	private bool goHome;

	private LightSource myLight;

	private float LightCounter;

	private Vector2[] oralArmOffsets;

	private float oralArmSway;

	private int mooCounter;

	private int SMSuckCounter;

	public int timeAbove;

	public int TotalSprites => CoreSpriteLength + tentacles.Length + BodySpriteLength + MouthSpriteLength;

	public int CoreSpriteLength => 3 + oralArmOffsets.Length;

	public int BodySpriteStart => CoreSpriteLength + tentacles.Length;

	public int BodySpriteLength => 5;

	public int CoreSpriteStart => 0;

	private int hoodSpriteStart => BodySpriteStart + BodySpriteLength;

	private int MouthSpriteLength => 2 + mouthBeads.Length;

	private BigJellyState abstractState => base.abstractCreature.state as BigJellyState;

	public int OralArmsStart => CoreSpriteStart + 3;

	public bool canBeSurfaceMode
	{
		get
		{
			if (surfaceMode)
			{
				return room.FloatWaterLevel(base.bodyChunks[CoreChunk].pos.x) < StartPos.y + 70f;
			}
			return false;
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetTentacles();
	}

	public void ResetTentacles()
	{
		for (int i = 0; i < tentacles.Length; i++)
		{
			for (int j = 0; j < tentacles[i].GetLength(0); j++)
			{
				tentacles[i][j, 0] = base.firstChunk.pos + Custom.RNV();
				tentacles[i][j, 1] = tentacles[i][j, 0];
				tentacles[i][j, 2] *= 0f;
			}
		}
	}

	public BigJellyFish(AbstractCreature abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		Random.State state = Random.state;
		Random.InitState(base.abstractCreature.ID.RandomSeed);
		minTGap = 2.4f;
		maxTGap = 7.1f;
		base.bodyChunks = new BodyChunk[7];
		bodyChunkConnections = new BodyChunkConnection[6];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 60.5f, 70.2f);
		CoreChunk = 1;
		base.bodyChunks[CoreChunk] = new BodyChunk(this, CoreChunk, new Vector2(0f, 0f), 17f, 20f);
		base.bodyChunks[CoreChunk].pos = base.firstChunk.pos + new Vector2(0f, -70f);
		leftHoodChunk = CoreChunk + 1;
		base.bodyChunks[leftHoodChunk] = new BodyChunk(this, leftHoodChunk, new Vector2(0f, 0f), 0f, 1f);
		rightHoodChunk = CoreChunk + 2;
		base.bodyChunks[rightHoodChunk] = new BodyChunk(this, rightHoodChunk, new Vector2(0f, 0f), 0f, 1f);
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 120f, BodyChunkConnection.Type.Pull, 0.4f, 0.3f);
		base.bodyChunks[4] = new BodyChunk(this, 4, new Vector2(0f, 0f), Random.Range(17f, 23f), 0.5f);
		base.bodyChunks[4].pos = base.bodyChunks[0].pos + new Vector2(0f, 10f);
		base.bodyChunks[5] = new BodyChunk(this, 5, new Vector2(0f, 0f), Random.Range(17f, 23f), 0.5f);
		base.bodyChunks[5].pos = base.bodyChunks[5].pos + new Vector2(-10f, 0f);
		base.bodyChunks[6] = new BodyChunk(this, 6, new Vector2(0f, 0f), Random.Range(17f, 23f), 0.5f);
		base.bodyChunks[6].pos = base.bodyChunks[6].pos + new Vector2(10f, 0f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[4], base.bodyChunks[0], 50f, BodyChunkConnection.Type.Pull, 0.7f, 0.1f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[5], base.bodyChunks[0], 50f, BodyChunkConnection.Type.Pull, 0.7f, 0.1f);
		bodyChunkConnections[3] = new BodyChunkConnection(base.bodyChunks[6], base.bodyChunks[0], 50f, BodyChunkConnection.Type.Pull, 0.7f, 0.1f);
		bodyChunkConnections[4] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[leftHoodChunk], 150f, BodyChunkConnection.Type.Pull, 0.1f, 1f);
		bodyChunkConnections[5] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[rightHoodChunk], 150f, BodyChunkConnection.Type.Pull, 0.1f, 1f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.7f;
		collisionLayer = 1;
		base.waterFriction = 0.95f;
		base.buoyancy = 0f;
		consumedCreatures = new List<Creature>();
		int num = Random.Range(16, 21);
		tentacles = new Vector2[num][,];
		if (abstractState.deadArmDriftPos == null)
		{
			abstractState.deadArmDriftPos = new Vector2[num];
		}
		latchOnToBodyChunks = new BodyChunk[num];
		tentacleScaler = new float[num];
		mouthBeads = new float[Random.Range(17, 23)];
		oralArmOffsets = new Vector2[Random.Range(25, 40)];
		StartPos = Vector2.zero;
		for (int i = 0; i < tentacles.Length; i++)
		{
			int num2 = Random.Range(4, 17);
			tentacles[i] = new Vector2[num2, 3];
			tentacleScaler[i] = Mathf.Lerp(18f, 60f, Mathf.InverseLerp(4f, 17f, num2));
		}
		for (int j = 0; j < mouthBeads.Length; j++)
		{
			mouthBeads[j] = 90f + (-8f + Random.value * 16f);
			if (Random.value < 0.5f)
			{
				mouthBeads[j] += 180f;
			}
		}
		_ = (float)oralArmOffsets.Length / 6f;
		float num3 = -60f;
		for (int k = 0; k < oralArmOffsets.Length; k++)
		{
			float num4 = (float)k / (float)oralArmOffsets.Length;
			float num5 = 1f - num4;
			float num6 = 96f * num4;
			oralArmOffsets[k] = new Vector2(Random.Range(0f - num6, num6), num3 * num5);
		}
		coreColor = new Color(0.82f, 0.42f, 0.24f);
		coreColorDark = new Color(0.64f, 0.14f, 0.09f);
		Random.state = state;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (base.dead)
		{
			return;
		}
		DebugDrag();
		DisableAirAccess();
		mooCounter--;
		if (SMSuckCounter > 0)
		{
			SMSuckCounter++;
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				if (i != CoreChunk)
				{
					base.bodyChunks[i].rad *= 0.98f;
					base.bodyChunks[i].vel += Custom.RNV();
				}
			}
			for (int j = 0; j < bodyChunkConnections.Length; j++)
			{
				bodyChunkConnections[j].distance *= 0.98f;
			}
			if (SMSuckCounter >= 100)
			{
				Die();
				return;
			}
		}
		if (huntingCounter > 0)
		{
			huntingCounter--;
		}
		else
		{
			huntingCounter = 0;
			huntPos = null;
		}
		oralArmSway += 0.05f;
		ConsumeCreateUpdate();
		if (StartPos == Vector2.zero)
		{
			if (abstractState.HomePos == Vector2.zero)
			{
				StartPos = room.MiddleOfTile(base.abstractCreature.pos);
			}
			else
			{
				StartPos = abstractState.HomePos;
			}
			surfaceMode = !room.PointSubmerged(StartPos + new Vector2(0f, 80f));
			if (surfaceMode)
			{
				StartPos.y = (float)(room.defaultWaterLevel + 1) * 20f;
			}
			if (abstractState.DriftPos == Vector2.zero)
			{
				driftGoalPos = room.MiddleOfTile(base.abstractCreature.pos);
			}
			else
			{
				driftGoalPos = abstractState.DriftPos;
			}
			if (!surfaceMode && driftGoalPos.y > (float)(room.defaultWaterLevel + 1) * 20f)
			{
				driftGoalPos.y = (float)(room.defaultWaterLevel + 1) * 20f;
			}
			Custom.Log("Jelly home at", StartPos.ToString());
			Custom.Log("Jelly goal at", driftGoalPos.ToString());
			Custom.Log("Jelly on surf", surfaceMode.ToString());
			goHome = false;
			driftCounter = 0f;
			driftMaxim = Vector2.Distance(StartPos, driftGoalPos);
			base.firstChunk.pos = StartPos;
			base.bodyChunks[CoreChunk].pos = StartPos + new Vector2(0f, -40f);
			base.bodyChunks[CoreChunk].vel *= 0f;
			base.bodyChunks[leftHoodChunk].pos = StartPos + new Vector2(-20f, -10f);
			base.bodyChunks[leftHoodChunk].vel *= 0f;
			base.bodyChunks[rightHoodChunk].pos = StartPos + new Vector2(20f, -10f);
			base.bodyChunks[rightHoodChunk].vel *= 0f;
		}
		if (canBeSurfaceMode)
		{
			if (base.firstChunk.pos.y < StartPos.y)
			{
				base.bodyChunks[0].vel += new Vector2(0f, base.gravity * 2.7f * Mathf.InverseLerp(0f, 0.2f, base.Submersion));
				base.bodyChunks[4].vel = new Vector2(0f, base.gravity * 4.1f * Mathf.InverseLerp(0f, 0.3f, base.Submersion));
				base.bodyChunks[5].vel = new Vector2(0f, base.gravity * 3f * Mathf.InverseLerp(0f, 0.5f, base.Submersion));
				base.bodyChunks[6].vel = new Vector2(0f, base.gravity * 3f * Mathf.InverseLerp(0f, 0.5f, base.Submersion));
			}
			if (base.firstChunk.pos.y > StartPos.y - 10f)
			{
				BodyChunk bodyChunk = base.bodyChunks[0];
				bodyChunk.vel.y = bodyChunk.vel.y * 0.4f;
			}
			Vector2 vector = Custom.DirVec(base.firstChunk.pos, StartPos);
			vector.y *= 0f;
			vector.x *= Mathf.InverseLerp(0f, 150f, Vector2.Distance(base.firstChunk.pos, StartPos)) / 2f;
			base.firstChunk.vel *= 0.9f;
			base.firstChunk.vel += vector;
		}
		else
		{
			base.bodyChunks[CoreChunk].vel += new Vector2(0f, -0.28f);
			base.bodyChunks[0].vel += new Vector2(0f, (base.gravity * 1.95f + 1f) * 0.72f);
			base.bodyChunks[4].vel = new Vector2(0f, base.gravity * 3f * 0.72f);
			base.bodyChunks[5].vel = new Vector2(0f, base.gravity * 3f * 0.72f);
			base.bodyChunks[6].vel = new Vector2(0f, base.gravity * 3f * 0.72f);
		}
		base.bodyChunks[4].pos = Custom.MoveTowards(base.bodyChunks[4].pos, base.bodyChunks[0].pos + rotation * 40f, 25f);
		Vector2 vector2 = Custom.PerpendicularVector(Custom.DirVec(base.bodyChunks[4].pos, base.bodyChunks[0].pos + rotation));
		base.bodyChunks[5].pos = Custom.MoveTowards(base.bodyChunks[4].pos, base.bodyChunks[0].pos + rotation * -20f + vector2 * -135f, 25f);
		base.bodyChunks[6].pos = Custom.MoveTowards(base.bodyChunks[4].pos, base.bodyChunks[0].pos + rotation * -20f + vector2 * 135f, 25f);
		bool flag = true;
		if (!base.safariControlled)
		{
			Vector2 b = (goHome ? driftGoalPos : StartPos);
			driftCounter += 1f;
			if (driftCounter > driftMaxim * 2f || Vector2.Distance(base.firstChunk.pos, b) > driftMaxim)
			{
				goHome = !goHome;
				driftCounter = 0f;
			}
		}
		else if (inputWithDiagonals.HasValue)
		{
			flag = false;
			if (inputWithDiagonals.Value.AnyDirectionalInput)
			{
				if (!huntPos.HasValue)
				{
					huntPos = base.bodyChunks[CoreChunk].pos;
				}
				if (Vector2.Distance(huntPos.Value + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 5f, base.bodyChunks[CoreChunk].pos) < 900f)
				{
					newHuntPos(huntPos.Value + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 5f);
				}
			}
			if (!canBeSurfaceMode)
			{
				if (inputWithDiagonals.Value.thrw)
				{
					if (driftGoalPos.y > StartPos.y)
					{
						goHome = true;
					}
					else
					{
						goHome = false;
					}
					flag = true;
				}
				else if (inputWithDiagonals.Value.jmp)
				{
					if (driftGoalPos.y > StartPos.y)
					{
						goHome = false;
					}
					else
					{
						goHome = true;
					}
					flag = true;
				}
			}
			if (inputWithDiagonals.Value.pckp)
			{
				PlayHorrifyingMoo();
			}
		}
		base.bodyChunks[leftHoodChunk].vel *= 0.2f;
		base.bodyChunks[rightHoodChunk].vel *= 0.2f;
		Vector2 zero = Vector2.zero;
		zero = ((!goHome) ? (Custom.DirVec(base.bodyChunks[0].pos, driftGoalPos) / 10f) : (Custom.DirVec(base.bodyChunks[0].pos, StartPos) / 10f));
		bool flag2 = false;
		float num = Mathf.Clamp(Mathf.Abs(zero.y * 10f), 0f, 1f);
		if (Mathf.Abs(StartPos.y - driftGoalPos.y) < 40f)
		{
			hoodPulse += 0.02f;
			hoodSwayingPulse = 0.5f + Mathf.Sin(hoodPulse) / 2f;
			flag2 = true;
		}
		if (!flag)
		{
			zero = new Vector2(0f, -0.09f * base.gravity);
			hoodPulse = Mathf.Lerp(hoodPulse, 0.63f, 0.08f);
		}
		if (zero.y < 0f)
		{
			if (!flag2)
			{
				hoodPulse = Mathf.Clamp(hoodPulse - 0.01f, 0.1f, 1f);
			}
			zero.y *= 8f * num;
		}
		if (zero.y > 0f)
		{
			if (!flag2)
			{
				hoodPulse = Mathf.Clamp(hoodPulse + 0.03f, 0.1f, 1f);
			}
			zero.y *= -7f * (1f - num);
		}
		base.bodyChunks[0].vel += zero;
		if (canBeSurfaceMode)
		{
			hoodSwayingPulse = 0.1f + Mathf.Pow(1f - base.Submersion, 20f) * 0.9f;
		}
		else if (!flag2)
		{
			hoodSwayingPulse = 0.1f + hoodPulse * 0.9f;
		}
		else
		{
			hoodSwayingPulse = 0.1f + hoodSwayingPulse * 0.9f;
		}
		base.bodyChunks[CoreChunk].vel += Custom.DirVec(base.bodyChunks[CoreChunk].pos, base.bodyChunks[0].pos) / 5f;
		Custom.DirVec(base.bodyChunks[CoreChunk].pos, base.firstChunk.pos);
		Vector2 vector3 = base.firstChunk.pos + Custom.DirVec(base.firstChunk.pos, base.bodyChunks[4].pos).normalized * 20f + Custom.DirVec(base.firstChunk.pos, base.bodyChunks[CoreChunk].pos) * 130f * hoodSwayingPulse;
		float speed = 29f;
		Vector2 vector4 = Custom.PerpendicularVector(vector3) * 30f;
		base.bodyChunks[leftHoodChunk].pos = Custom.MoveTowards(base.bodyChunks[leftHoodChunk].pos, vector3 - vector4, speed);
		base.bodyChunks[rightHoodChunk].pos = Custom.MoveTowards(base.bodyChunks[rightHoodChunk].pos, vector3 + vector4, speed);
		tentaclesWithdrawn = 0f;
		if (!anyTentaclePulled)
		{
			rotation = Vector3.Slerp(rotation, new Vector2(0f, 1f), (1f - 2f * Mathf.Abs(0.5f - base.firstChunk.submersion)) * 0.1f);
		}
		rotation = Vector3.Slerp(rotation, new Vector2(0f, 1f), 1f - Mathf.Abs(rotation.y));
		if (base.firstChunk.ContactPoint.y < 0)
		{
			BodyChunk bodyChunk2 = base.firstChunk;
			bodyChunk2.vel.x = bodyChunk2.vel.x * 0.8f;
		}
		LightUpdate();
		anyTentaclePulled = false;
		for (int k = 0; k < tentacles.Length; k++)
		{
			float num2 = Mathf.Lerp(tentacleScaler[k], 1f, tentaclesWithdrawn);
			for (int l = 0; l < tentacles[k].GetLength(0); l++)
			{
				float t = (float)l / (float)(tentacles[k].GetLength(0) - 1);
				tentacles[k][l, 1] = tentacles[k][l, 0];
				tentacles[k][l, 0] += tentacles[k][l, 2];
				tentacles[k][l, 2] -= rotation * Mathf.InverseLerp(4f, 0f, l) * 0.8f;
				if (room.PointSubmerged(tentacles[k][l, 0]))
				{
					tentacles[k][l, 2] *= Custom.LerpMap(tentacles[k][l, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
					Vector2 vector5 = new Vector2(0f, 0f);
					if (huntPos.HasValue)
					{
						vector5 = Custom.DirVec(tentacles[k][l, 0], huntPos.Value);
					}
					tentacles[k][l, 2] += Custom.RNV() * 0.2f + vector5 * 0.2f;
				}
				else
				{
					tentacles[k][l, 2] *= 0.999f;
					tentacles[k][l, 2].y -= room.gravity * 0.6f;
					SharedPhysics.TerrainCollisionData cd2 = SharedPhysics.HorizontalCollision(cd: new SharedPhysics.TerrainCollisionData(tentacles[k][l, 0], tentacles[k][l, 1], tentacles[k][l, 2], 1f, new IntVector2(0, 0), goThroughFloors: false), room: room);
					cd2 = SharedPhysics.VerticalCollision(room, cd2);
					cd2 = SharedPhysics.SlopesVertically(room, cd2);
					tentacles[k][l, 0] = cd2.pos;
					tentacles[k][l, 2] = cd2.vel;
				}
			}
			for (int m = 0; m < tentacles[k].GetLength(0); m++)
			{
				if (m > 0)
				{
					Vector2 normalized = (tentacles[k][m, 0] - tentacles[k][m - 1, 0]).normalized;
					float num3 = Vector2.Distance(tentacles[k][m, 0], tentacles[k][m - 1, 0]);
					tentacles[k][m, 0] += normalized * (num2 - num3) * 0.5f;
					tentacles[k][m, 2] += normalized * (num2 - num3) * 0.5f;
					tentacles[k][m - 1, 0] -= normalized * (num2 - num3) * 0.5f;
					tentacles[k][m - 1, 2] -= normalized * (num2 - num3) * 0.5f;
					if (m > 1)
					{
						normalized = (tentacles[k][m, 0] - tentacles[k][m - 2, 0]).normalized;
						tentacles[k][m, 2] += normalized * 0.2f;
						tentacles[k][m - 2, 2] -= normalized * 0.2f;
					}
				}
				else
				{
					float num4 = 0f;
					Vector2 vector6 = AttachPos(k, 1f);
					if (canBeSurfaceMode && base.Submersion > 0.1f)
					{
						num4 = (room.FloatWaterLevel(vector6.x) - (float)(room.defaultWaterLevel + 1) * 20f) / 1.9f;
					}
					Vector2 vector7 = rotation * num4;
					tentacles[k][m, 0] = vector6 + vector7;
					tentacles[k][m, 2] *= 0f;
				}
			}
			if (latchOnToBodyChunks[k] != null && latchOnToBodyChunks[k].owner is Creature && (latchOnToBodyChunks[k].owner as Creature).enteringShortCut.HasValue)
			{
				Custom.Log($"Big jelly released door traveling object {latchOnToBodyChunks[k].owner}");
				latchOnToBodyChunks[k] = null;
			}
			if (latchOnToBodyChunks[k] != null)
			{
				bool flag3 = false;
				if (latchOnToBodyChunks[k].owner is Player && MMF.cfgGraspWiggling.Value)
				{
					flag3 = (latchOnToBodyChunks[k].owner as Player).GraspWiggle > 0.8f;
				}
				if (!base.dead && room.PointSubmerged(tentacles[k][tentacles[k].GetLength(0) - 1, 0]) && !flag3 && !base.Stunned && !consumedCreatures.Contains(latchOnToBodyChunks[k].owner as Creature))
				{
					newHuntPos(latchOnToBodyChunks[k].pos);
					float num5 = base.bodyChunks[0].pos.y - base.bodyChunks[0].rad;
					float num6 = Mathf.InverseLerp(num5 - 10f, num5 + 10f, latchOnToBodyChunks[k].pos.y);
					float num7 = Mathf.Sign(latchOnToBodyChunks[k].pos.x - base.bodyChunks[0].pos.x) * Mathf.InverseLerp(num5 - 10f, num5 + 5f, latchOnToBodyChunks[k].pos.y) / 5f;
					num7 *= Mathf.InverseLerp(50f, 0f, Mathf.Abs(latchOnToBodyChunks[k].pos.x - base.bodyChunks[0].pos.x));
					Vector2 vector8 = Custom.DirVec(latchOnToBodyChunks[k].pos + rotation * (-40f * num6), new Vector2(base.bodyChunks[0].pos.x, num5)) / 10f;
					vector8.x += num7;
					latchOnToBodyChunks[k].vel += vector8;
					if (latchOnToBodyChunks[k].pos.y > num5)
					{
						timeAbove++;
						BodyChunk bodyChunk3 = latchOnToBodyChunks[k];
						float f = latchOnToBodyChunks[k].pos.x - base.bodyChunks[0].pos.x;
						float f2 = latchOnToBodyChunks[k].pos.y - base.bodyChunks[0].pos.y;
						float num8 = (1f - Mathf.InverseLerp(0f, base.bodyChunks[0].rad * 3f, Mathf.Abs(f))) * 1.8f;
						bodyChunk3.vel.x = bodyChunk3.vel.x + Mathf.Sign(f) * num8;
						bodyChunk3.vel.y = bodyChunk3.vel.y + Mathf.Sign(f2) * num8;
					}
					else
					{
						timeAbove = Mathf.Max(timeAbove - 1, 0);
					}
					anyTentaclePulled = true;
					Vector2 normalized2 = (tentacles[k][tentacles[k].GetLength(0) - 1, 0] - latchOnToBodyChunks[k].pos).normalized;
					float num9 = Vector2.Distance(tentacles[k][tentacles[k].GetLength(0) - 1, 0], latchOnToBodyChunks[k].pos);
					tentacles[k][tentacles[k].GetLength(0) - 1, 0] += normalized2 * (latchOnToBodyChunks[k].rad * 0.5f - num9) * 0.5f;
					tentacles[k][tentacles[k].GetLength(0) - 1, 2] += normalized2 * (latchOnToBodyChunks[k].rad * 0.5f - num9) * 0.5f;
					if (!Custom.DistLess(base.firstChunk.pos, latchOnToBodyChunks[k].pos, (float)tentacles[k].GetLength(0) * num2 * 1.1f))
					{
						latchOnToBodyChunks[k] = null;
						room.PlaySound(SoundID.Jelly_Fish_Tentacle_Release, tentacles[k][tentacles[k].GetLength(0) - 1, 0]);
					}
					else if (Random.value < 0.00045f && room.PointSubmerged(new Vector2(tentacles[k][tentacles[k].GetLength(0) - 1, 0].x, tentacles[k][tentacles[k].GetLength(0) - 1, 0].y + 30f)))
					{
						if (latchOnToBodyChunks[k].owner is Creature)
						{
							(latchOnToBodyChunks[k].owner as Creature).Stun(100 * (int)Mathf.InverseLerp(50f, 10f, latchOnToBodyChunks[k].owner.TotalMass));
							if (latchOnToBodyChunks[k].owner is Player && (latchOnToBodyChunks[k].owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
							{
								room.AddObject(new CreatureSpasmer(latchOnToBodyChunks[k].owner as Creature, allowDead: true, 80));
								(latchOnToBodyChunks[k].owner as Player).SaintStagger(500);
							}
							room.AddObject(new ShockWave(tentacles[k][tentacles[k].GetLength(0) - 1, 0], Mathf.Lerp(40f, 60f, Random.value), 0.07f, 6));
							tentacles[k][tentacles[k].GetLength(0) - 1, 0] = Vector2.Lerp(tentacles[k][tentacles[k].GetLength(0) - 1, 0], base.firstChunk.pos, 0.2f);
						}
						room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, tentacles[k][tentacles[k].GetLength(0) - 1, 0]);
						latchOnToBodyChunks[k] = null;
					}
					else if (!Custom.DistLess(base.firstChunk.pos, latchOnToBodyChunks[k].pos, (float)tentacles[k].GetLength(0) * num2 * 1.4f))
					{
						normalized2 = (base.firstChunk.pos - latchOnToBodyChunks[k].pos).normalized;
						num9 = Vector2.Distance(base.firstChunk.pos, latchOnToBodyChunks[k].pos);
						float num10 = base.firstChunk.mass / (base.firstChunk.mass + latchOnToBodyChunks[k].mass);
						latchOnToBodyChunks[k].pos -= normalized2 * ((float)tentacles[k].GetLength(0) * num2 * 1.4f - num9) * num10;
						latchOnToBodyChunks[k].vel -= normalized2 * ((float)tentacles[k].GetLength(0) * num2 * 1.4f - num9) * num10;
						rotation = (rotation + normalized2 * Mathf.InverseLerp((float)tentacles[k].GetLength(0) * num2 * 0.4f, (float)tentacles[k].GetLength(0) * num2 * 2.4f, Vector2.Distance(base.firstChunk.pos, latchOnToBodyChunks[k].pos))).normalized;
					}
				}
				else
				{
					latchOnToBodyChunks[k] = null;
					room.PlaySound(SoundID.Jelly_Fish_Tentacle_Release, tentacles[k][tentacles[k].GetLength(0) - 1, 0]);
				}
			}
			if (latchOnToBodyChunks[k] != null || !room.PointSubmerged(tentacles[k][tentacles[k].GetLength(0) - 1, 0]))
			{
				continue;
			}
			Vector2 vector9 = tentacles[k][tentacles[k].GetLength(0) - 1, 0];
			int num11 = 0;
			while (latchOnToBodyChunks[k] == null && num11 < room.abstractRoom.creatures.Count)
			{
				if (ValidGrabCreature(room.abstractRoom.creatures[num11]))
				{
					int num12 = 0;
					while (latchOnToBodyChunks[k] == null && num12 < room.abstractRoom.creatures[num11].realizedCreature.bodyChunks.Length)
					{
						if (Custom.DistLess(room.abstractRoom.creatures[num11].realizedCreature.bodyChunks[num12].pos, vector9, room.abstractRoom.creatures[num11].realizedCreature.bodyChunks[num12].rad * 1.15f))
						{
							latchOnToBodyChunks[k] = room.abstractRoom.creatures[num11].realizedCreature.bodyChunks[num12];
							room.PlaySound((!(room.abstractRoom.creatures[num11].realizedCreature is Player)) ? SoundID.Jelly_Fish_Tentacle_Latch_On_NPC : SoundID.Jelly_Fish_Tentacle_Latch_On_Player, vector9);
							PlayHorrifyingMoo();
						}
						num12++;
					}
				}
				num11++;
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[CoreSpriteStart] = TriangleMesh.MakeLongMesh(8, pointyTip: false, customColor: true);
		sLeaser.sprites[CoreSpriteStart + 1] = new FSprite("Futile_White");
		sLeaser.sprites[CoreSpriteStart + 1].scale = 2.5f;
		sLeaser.sprites[CoreSpriteStart + 1].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
		sLeaser.sprites[CoreSpriteStart + 2] = new FSprite("Futile_White");
		sLeaser.sprites[CoreSpriteStart + 2].scale = 1.9230769f;
		sLeaser.sprites[CoreSpriteStart + 2].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
		for (int i = 0; i < tentacles.Length; i++)
		{
			sLeaser.sprites[TentacleSprite(i)] = TriangleMesh.MakeLongMesh(tentacles[i].GetLength(0), pointyTip: false, customColor: true);
		}
		sLeaser.sprites[BodySpriteStart] = new FSprite("Futile_White");
		sLeaser.sprites[BodySpriteStart].scale = base.bodyChunks[0].rad / 20f;
		sLeaser.sprites[BodySpriteStart].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
		sLeaser.sprites[BodySpriteStart].isVisible = false;
		sLeaser.sprites[BodySpriteStart + 1] = new FSprite("Futile_White");
		sLeaser.sprites[BodySpriteStart + 1].scale = base.bodyChunks[4].rad / 2f;
		sLeaser.sprites[BodySpriteStart + 1].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
		sLeaser.sprites[BodySpriteStart + 2] = new FSprite("Futile_White");
		sLeaser.sprites[BodySpriteStart + 2].scale = base.bodyChunks[5].rad / 2f;
		sLeaser.sprites[BodySpriteStart + 2].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
		sLeaser.sprites[BodySpriteStart + 3] = new FSprite("Futile_White");
		sLeaser.sprites[BodySpriteStart + 3].scale = base.bodyChunks[6].rad / 2f;
		sLeaser.sprites[BodySpriteStart + 3].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
		sLeaser.sprites[BodySpriteStart + 4] = TriangleMesh.MakeLongMesh(3, pointyTip: false, customColor: true);
		sLeaser.sprites[hoodSpriteStart] = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: true);
		sLeaser.sprites[hoodSpriteStart + 1] = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: true);
		for (int j = 0; j < mouthBeads.Length; j++)
		{
			sLeaser.sprites[hoodSpriteStart + 2 + j] = new FSprite("DangleFruit0A");
			sLeaser.sprites[hoodSpriteStart + 2 + j].rotation = mouthBeads[j];
			sLeaser.sprites[hoodSpriteStart + 2 + j].scale = 1.34f;
		}
		for (int k = 0; k < oralArmOffsets.Length; k++)
		{
			sLeaser.sprites[OralArmsStart + k] = new FSprite("OralArm");
			sLeaser.sprites[OralArmsStart + k].rotation = 180f;
			sLeaser.sprites[OralArmsStart + k].scaleX = Random.Range(0.75f, 1f);
			sLeaser.sprites[OralArmsStart + k].scaleY = Random.Range(0.45f, 0.9f);
			sLeaser.sprites[OralArmsStart + k].anchorX = 0.5f;
			sLeaser.sprites[OralArmsStart + k].anchorY = 0.05f;
			sLeaser.sprites[OralArmsStart + k].alpha = 0.75f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Background");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			if (i >= OralArmsStart)
			{
				rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = new Color(0.87f, 0.78f, 0.55f);
		float num = rCam.PaletteDarkness();
		Color a = Color.Lerp(Color.Lerp(palette.fogColor, color, 0.3f), palette.blackColor, Mathf.Pow(num, 2f));
		Color a2 = Color.Lerp(Color.Lerp(a, new Color(1f, 1f, 1f), 0.5f), palette.blackColor, Mathf.Pow(num, 2f));
		Color a3 = Color.Lerp(color, palette.blackColor, Mathf.Clamp(num, 0.1f, 1f));
		color = Color.Lerp(color, palette.blackColor, darkness);
		a = Color.Lerp(a, palette.blackColor, darkness);
		a2 = Color.Lerp(a2, palette.blackColor, darkness);
		a3 = Color.Lerp(a3, palette.blackColor, darkness);
		sLeaser.sprites[BodySpriteStart].color = a2;
		sLeaser.sprites[BodySpriteStart + 1].color = Color.Lerp(a2, coreColorDark, 0.2f);
		sLeaser.sprites[BodySpriteStart + 2].color = Color.Lerp(a2, coreColorDark, 0.2f);
		sLeaser.sprites[BodySpriteStart + 3].color = Color.Lerp(a2, coreColorDark, 0.3f);
		sLeaser.sprites[CoreSpriteStart + 1].color = coreColor;
		sLeaser.sprites[CoreSpriteStart + 2].color = coreColorDark;
		this.color = a;
		for (int i = 0; i < tentacles.Length; i++)
		{
			for (int j = 0; j < (sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors.Length; j++)
			{
				(sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors[j] = Color.Lerp(a, a3, (float)j / (float)((sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors.Length - 1));
			}
		}
		Color a4 = a2;
		for (int k = 0; k < (sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).verticeColors.Length; k++)
		{
			(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).verticeColors[k] = Color.Lerp(a2, sLeaser.sprites[BodySpriteStart + 3].color, (float)k / (float)((sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).verticeColors.Length - 1));
			if (k == 5)
			{
				a4 = (sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).verticeColors[k];
			}
		}
		for (int l = 0; l < (sLeaser.sprites[hoodSpriteStart] as TriangleMesh).verticeColors.Length; l++)
		{
			(sLeaser.sprites[hoodSpriteStart] as TriangleMesh).verticeColors[l] = Color.Lerp(a4, a, (float)l / (float)((sLeaser.sprites[hoodSpriteStart + 1] as TriangleMesh).verticeColors.Length - 1));
			(sLeaser.sprites[hoodSpriteStart + 1] as TriangleMesh).verticeColors[l] = Color.Lerp(a4, a, (float)l / (float)((sLeaser.sprites[hoodSpriteStart + 1] as TriangleMesh).verticeColors.Length - 1));
		}
		for (int m = 0; m < (sLeaser.sprites[CoreSpriteStart] as TriangleMesh).verticeColors.Length; m++)
		{
			(sLeaser.sprites[CoreSpriteStart] as TriangleMesh).verticeColors[m] = Color.Lerp(a, coreColor, (float)m / (float)((sLeaser.sprites[CoreSpriteStart] as TriangleMesh).verticeColors.Length - 1));
		}
		for (int n = 0; n < mouthBeads.Length; n++)
		{
			sLeaser.sprites[hoodSpriteStart + 2 + n].color = a;
		}
		for (int num2 = 0; num2 < oralArmOffsets.Length; num2++)
		{
			sLeaser.sprites[OralArmsStart + num2].color = Color.Lerp(a, coreColor, 0.4f);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
			return;
		}
		Vector2 p = Vector2.Lerp(base.bodyChunks[5].lastPos, base.bodyChunks[5].pos, timeStacker);
		Vector2 vector = Vector2.Lerp(base.bodyChunks[0].lastPos, base.bodyChunks[0].pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(base.bodyChunks[CoreChunk].lastPos, base.bodyChunks[CoreChunk].pos, timeStacker);
		Vector2 p2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		p2 = Custom.DirVec(Vector2.zero, p2);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		sLeaser.sprites[BodySpriteStart + 1].scale = base.bodyChunks[4].rad / 10f;
		sLeaser.sprites[BodySpriteStart + 2].scale = base.bodyChunks[5].rad / 10f;
		sLeaser.sprites[BodySpriteStart + 3].scale = base.bodyChunks[6].rad / 10f;
		Vector2 vector3 = MouthLeftPos(timeStacker);
		Vector2 vector4 = MouthRightPos(timeStacker);
		for (int i = 0; i < mouthBeads.Length; i++)
		{
			float t = (float)i / (float)mouthBeads.Length;
			float num = 0f;
			Vector2 vector5 = Custom.PerpendicularVector(p2) * -8f + p2 * 3f + Vector2.Lerp(MouthLeftPos(timeStacker), MouthRightPos(timeStacker), t);
			if (canBeSurfaceMode && base.Submersion > 0.1f)
			{
				num = room.FloatWaterLevel(vector5.x) - (float)(room.defaultWaterLevel + 1) * 20f;
				num /= 2f;
			}
			Vector2 vector6 = p2 * num;
			sLeaser.sprites[hoodSpriteStart + 2 + i].x = vector5.x + vector6.x - camPos.x;
			sLeaser.sprites[hoodSpriteStart + 2 + i].y = vector5.y + vector6.y - 5f - camPos.y;
			sLeaser.sprites[hoodSpriteStart + 2 + i].rotation = mouthBeads[i];
			if (i == 0)
			{
				vector3 = vector5 + vector6;
			}
			if (i == mouthBeads.Length - 1)
			{
				vector4 = vector5 + vector6;
			}
		}
		int coreSpriteStart = CoreSpriteStart;
		int num2 = 8;
		Vector2 vector7 = vector;
		Vector2 vector8 = Custom.DirVec(vector, vector2).normalized;
		Vector2 vector9 = Custom.PerpendicularVector(vector8);
		float num3 = Vector2.Distance(vector, vector2) / (float)num2;
		float num4 = Vector2.Distance(vector, vector2) / 3.137254f;
		float num5 = 0f;
		for (int j = 0; j < num2; j++)
		{
			vector8 = Vector2.Lerp(Custom.DirVec(vector, vector2).normalized, Custom.DirVec(p, vector).normalized, j / num2);
			float t2 = (float)SMSuckCounter / 100f;
			Vector2 vector10 = vector7;
			float num6 = Mathf.Sin(num5 / num4);
			Vector2 vector11 = vector9 * (Mathf.Lerp(20f, 8f, t2) - 10f * num6);
			if (j == 0)
			{
				(sLeaser.sprites[coreSpriteStart] as TriangleMesh).MoveVertice(j * 4, vector3 - camPos);
				(sLeaser.sprites[coreSpriteStart] as TriangleMesh).MoveVertice(j * 4 + 1, vector4 - camPos);
				(sLeaser.sprites[coreSpriteStart] as TriangleMesh).MoveVertice(j * 4 + 2, Vector2.Lerp(vector10 - vector11, vector3, 0.5f) - camPos);
				(sLeaser.sprites[coreSpriteStart] as TriangleMesh).MoveVertice(j * 4 + 3, Vector2.Lerp(vector10 + vector11, vector4, 0.5f) - camPos);
			}
			else
			{
				(sLeaser.sprites[coreSpriteStart] as TriangleMesh).MoveVertice(j * 4, vector10 - vector11 - camPos);
				(sLeaser.sprites[coreSpriteStart] as TriangleMesh).MoveVertice(j * 4 + 1, vector10 + vector11 - camPos);
				(sLeaser.sprites[coreSpriteStart] as TriangleMesh).MoveVertice(j * 4 + 2, vector10 - vector11 - camPos);
				(sLeaser.sprites[coreSpriteStart] as TriangleMesh).MoveVertice(j * 4 + 3, vector10 + vector11 - camPos);
			}
			num5 += num3;
			vector7 = vector10 + vector8 * num3;
		}
		for (int k = 0; k < BodySpriteLength - 1; k++)
		{
			Vector2 vector12 = vector;
			if (k == 1)
			{
				vector12 = Vector2.Lerp(base.bodyChunks[6].lastPos, base.bodyChunks[6].pos, timeStacker);
			}
			if (k == 2)
			{
				vector12 = Vector2.Lerp(base.bodyChunks[5].lastPos, base.bodyChunks[5].pos, timeStacker);
			}
			if (k == 3)
			{
				vector12 = Vector2.Lerp(base.bodyChunks[4].lastPos, base.bodyChunks[4].pos, timeStacker);
			}
			sLeaser.sprites[BodySpriteStart + k].x = vector12.x - camPos.x;
			sLeaser.sprites[BodySpriteStart + k].y = vector12.y - camPos.y;
			sLeaser.sprites[BodySpriteStart + k].rotation = Custom.VecToDeg(p2);
		}
		num2 = 6;
		Vector2 a = (vector7 = vector + p2 * 10f);
		Vector2 vector13 = p2 * -1f;
		num3 = Vector2.Distance(a, AttachPos(0, 1f)) / (float)num2;
		num4 = Vector2.Distance(a, AttachPos(0, 1f)) / 3.137254f;
		num5 = 0f;
		for (int l = 0; l < num2; l++)
		{
			float t3 = (float)l / ((float)num2 - 1f);
			Vector2 vector14 = vector7;
			Vector2 vector15 = vector7 + p2 * -25f;
			float num7 = Mathf.Sin(num5 / num4);
			Vector2 vector16 = vector9 * (50f + 20f * num7);
			Vector2 vector17 = vector9 * (10f + 60f * num7);
			(sLeaser.sprites[hoodSpriteStart] as TriangleMesh).MoveVertice(l * 4, Vector2.Lerp(vector15 - vector17, vector3, t3) - camPos);
			(sLeaser.sprites[hoodSpriteStart] as TriangleMesh).MoveVertice(l * 4 + 1, Vector2.Lerp(vector15 + vector17, vector4, t3) - camPos);
			(sLeaser.sprites[hoodSpriteStart] as TriangleMesh).MoveVertice(l * 4 + 2, Vector2.Lerp(vector15 - vector17, vector3, t3) - camPos);
			(sLeaser.sprites[hoodSpriteStart] as TriangleMesh).MoveVertice(l * 4 + 3, Vector2.Lerp(vector15 + vector17, vector4, t3) - camPos);
			(sLeaser.sprites[hoodSpriteStart + 1] as TriangleMesh).MoveVertice(l * 4, Vector2.Lerp(vector14 - vector16, vector3, t3) - camPos);
			(sLeaser.sprites[hoodSpriteStart + 1] as TriangleMesh).MoveVertice(l * 4 + 1, Vector2.Lerp(vector14 + vector16, vector4, t3) - camPos);
			(sLeaser.sprites[hoodSpriteStart + 1] as TriangleMesh).MoveVertice(l * 4 + 2, Vector2.Lerp(vector14 - vector16, vector3, t3) - camPos);
			(sLeaser.sprites[hoodSpriteStart + 1] as TriangleMesh).MoveVertice(l * 4 + 3, Vector2.Lerp(vector14 + vector16, vector4, t3) - camPos);
			num5 += num3;
			vector7 = vector14 + vector13 * num3;
		}
		Vector2 vector18 = Vector2.Lerp(base.bodyChunks[4].lastPos, base.bodyChunks[4].pos, timeStacker) + p2 * base.bodyChunks[4].rad / 2f;
		Vector2 vector19 = Vector2.Lerp(base.bodyChunks[6].lastPos, base.bodyChunks[6].pos, timeStacker);
		Vector2 vector20 = Vector2.Lerp(base.bodyChunks[5].lastPos, base.bodyChunks[5].pos, timeStacker);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(0, vector + p2 * -15f + Custom.PerpendicularVector(p2) * (base.bodyChunks[4].rad * -0.3f) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(1, vector + p2 * -15f + Custom.PerpendicularVector(p2) * (base.bodyChunks[4].rad * 0.3f) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(2, vector + p2 * -10f + Custom.PerpendicularVector(p2) * (base.bodyChunks[4].rad * -0.45f) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(3, vector + p2 * -10f + Custom.PerpendicularVector(p2) * (base.bodyChunks[4].rad * 0.45f) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(4, vector + p2 * 5f + Custom.PerpendicularVector(vector8) * Mathf.Lerp(48f, 55f, 1f - hoodSwayingPulse) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(5, vector + p2 * 5f + Custom.PerpendicularVector(vector8) * Mathf.Lerp(-48f, -55f, 1f - hoodSwayingPulse) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(6, vector19 + Custom.PerpendicularVector(p2) * (base.bodyChunks[6].rad * Mathf.Lerp(-0.8f, -0.6f, 1f - hoodSwayingPulse)) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(7, vector20 + Custom.PerpendicularVector(p2) * (base.bodyChunks[5].rad * Mathf.Lerp(0.8f, 0.6f, 1f - hoodSwayingPulse)) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(8, vector19 + p2 * base.bodyChunks[6].rad / 1.9f + Custom.PerpendicularVector(p2) * (base.bodyChunks[5].rad * -0.4f) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(9, vector20 + p2 * base.bodyChunks[5].rad / 1.9f + Custom.PerpendicularVector(p2) * (base.bodyChunks[6].rad * 0.4f) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(10, vector18 + Custom.PerpendicularVector(p2) * (base.bodyChunks[4].rad * -0.5f) - camPos);
		(sLeaser.sprites[BodySpriteStart + 4] as TriangleMesh).MoveVertice(11, vector18 + Custom.PerpendicularVector(p2) * (base.bodyChunks[4].rad * 0.5f) - camPos);
		for (int m = 0; m < oralArmOffsets.Length; m++)
		{
			float num8 = oralArmOffsets[m].x * (1f - hoodSwayingPulse);
			sLeaser.sprites[OralArmsStart + m].x = vector.x + num8 - camPos.x;
			sLeaser.sprites[OralArmsStart + m].y = vector.y - camPos.y;
			sLeaser.sprites[OralArmsStart + m].rotation = Mathf.Lerp(sLeaser.sprites[OralArmsStart + m].rotation, 180f + Mathf.Sin(oralArmSway + num8) * 5f, 0.05f);
		}
		sLeaser.sprites[CoreSpriteStart + 1].x = vector2.x - camPos.x;
		sLeaser.sprites[CoreSpriteStart + 1].y = vector2.y - camPos.y;
		sLeaser.sprites[CoreSpriteStart + 2].x = vector2.x - camPos.x;
		sLeaser.sprites[CoreSpriteStart + 2].y = vector2.y - camPos.y;
		for (int n = 0; n < tentacles.Length; n++)
		{
			float num9 = 0f;
			Vector2 vector21 = AttachPos(n, timeStacker);
			for (int num10 = 0; num10 < tentacles[n].GetLength(0); num10++)
			{
				Vector2 vector22 = Vector2.Lerp(tentacles[n][num10, 1], tentacles[n][num10, 0], timeStacker);
				float num11 = Mathf.Lerp(3f, 0.2f, (float)num10 / (float)tentacles[n].GetLength(0));
				Vector2 normalized = (vector21 - vector22).normalized;
				Vector2 vector23 = Custom.PerpendicularVector(normalized);
				float num12 = Vector2.Distance(vector21, vector22) / 5f;
				(sLeaser.sprites[TentacleSprite(n)] as TriangleMesh).MoveVertice(num10 * 4, vector21 - normalized * num12 - vector23 * (num11 + num9) * 0.5f - camPos);
				(sLeaser.sprites[TentacleSprite(n)] as TriangleMesh).MoveVertice(num10 * 4 + 1, vector21 - normalized * num12 + vector23 * (num11 + num9) * 0.5f - camPos);
				(sLeaser.sprites[TentacleSprite(n)] as TriangleMesh).MoveVertice(num10 * 4 + 2, vector22 + normalized * num12 - vector23 * num11 - camPos);
				(sLeaser.sprites[TentacleSprite(n)] as TriangleMesh).MoveVertice(num10 * 4 + 3, vector22 + normalized * num12 + vector23 * num11 - camPos);
				vector21 = vector22;
				num9 = num11;
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public int TentacleSprite(int t)
	{
		return CoreSpriteLength + t;
	}

	public Vector2 AttachPos(int rag, float timeStacker)
	{
		float num = Mathf.Lerp(maxTGap, minTGap, hoodSwayingPulse);
		return Vector2.Lerp(Vector2.Lerp(base.bodyChunks[leftHoodChunk].lastPos, base.bodyChunks[leftHoodChunk].pos, timeStacker), Vector2.Lerp(base.bodyChunks[rightHoodChunk].lastPos, base.bodyChunks[rightHoodChunk].pos, timeStacker), 0.5f) + new Vector2(Mathf.Sin(rag) * (float)rag * num, 0f);
	}

	public override void HeardNoise(InGameNoise noise)
	{
		if (!base.safariControlled && noise != default(InGameNoise))
		{
			newCuriousHuntPos(noise.pos);
		}
	}

	private void newHuntPos(Vector2 pos)
	{
		huntPos = pos;
		huntingCounter = Random.Range(60, 100);
	}

	public void newCuriousHuntPos(Vector2 pos)
	{
		huntPos = pos;
		huntingCounter = Random.Range(10, 90);
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		if (otherObject == this && ((myChunk == 0 && (otherChunk == 4 || otherChunk == 5 || otherChunk == 6)) || (otherChunk == 0 && (myChunk == 4 || myChunk == 5 || myChunk == 6))))
		{
			return;
		}
		base.Collide(otherObject, myChunk, otherChunk);
		if ((myChunk != CoreChunk && (myChunk != 0 || !(otherObject.Submersion > 0.8f) || !(otherObject.firstChunk.pos.y < base.firstChunk.pos.y - 10f))) || !(otherObject is Creature) || consumedCreatures.Contains(otherObject as Creature) || !(otherObject.TotalMass < base.TotalMass))
		{
			return;
		}
		if (!(otherObject as Creature).dead)
		{
			room.AddObject(new UnderwaterShock(room, this, otherObject.bodyChunks[otherChunk].pos, 14, 80f, 10f, this, new Color(0.7f, 0.7f, 1f)));
			for (int i = 0; i < 4; i++)
			{
				room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Volt_Shock, otherObject.bodyChunks[otherChunk].pos, 1f, Random.value * 0.5f + 0.4f);
			}
			room.PlaySound(SoundID.Zapper_Zap, otherObject.bodyChunks[otherChunk].pos, 1f, 2f);
			(otherObject as Creature).Die();
		}
		consumedCreatures.Add(otherObject as Creature);
	}

	private void DebugDrag()
	{
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Input.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
	}

	private void DisableAirAccess()
	{
		float num = room.FloatWaterLevel(base.firstChunk.pos.y) + 60f;
		if (base.firstChunk.pos.y > num)
		{
			Custom.Log("jelly body escaped water");
			if (room.defaultWaterLevel < 0)
			{
				base.bodyChunks[1].pos = base.bodyChunks[0].pos;
			}
			Die();
		}
		else if (base.bodyChunks[CoreChunk].pos.y > base.firstChunk.pos.y + 15f)
		{
			Custom.Log("jelly body twisted beyond use");
			Die();
		}
	}

	private bool ValidGrabCreature(AbstractCreature abs)
	{
		if (abs.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.BigJelly && abs.creatureTemplate.type != CreatureTemplate.Type.Leech && abs.creatureTemplate.type != CreatureTemplate.Type.SeaLeech && abs.creatureTemplate.type != CreatureTemplate.Type.BigEel && abs.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.MirosVulture && abs.realizedCreature != null && abs.realizedCreature != this && !consumedCreatures.Contains(abs.realizedCreature) && abs.realizedCreature.room == room)
		{
			return !abs.realizedCreature.enteringShortCut.HasValue;
		}
		return false;
	}

	private void ConsumeCreateUpdate()
	{
		Vector2 vector = base.firstChunk.pos + new Vector2(0f, -60f);
		for (int i = 0; i < consumedCreatures.Count; i++)
		{
			float num = Vector2.Distance(consumedCreatures[i].firstChunk.pos, vector);
			float num2 = Mathf.Sign(consumedCreatures[i].firstChunk.pos.x - vector.x);
			Vector2 vector2 = Custom.DirVec(consumedCreatures[i].firstChunk.pos, vector + new Vector2(num2 * 15f, 0f)) * 1.3f;
			vector2.y /= 10f;
			vector2.x *= Mathf.InverseLerp(10f, 60f, num);
			consumedCreatures[i].firstChunk.vel = (Custom.DirVec(consumedCreatures[i].firstChunk.pos, vector) + vector2) / 3f;
			if (num > 100f || consumedCreatures[i].slatedForDeletetion || consumedCreatures[i].room != room)
			{
				consumedCreatures.RemoveAt(i);
				break;
			}
			if (num < 9f)
			{
				consumedCreatures[i].Destroy();
				consumedCreatures.RemoveAt(i);
				break;
			}
		}
	}

	public Vector2 MouthLeftPos(float timestacker)
	{
		float num = (float)tentacles.Length * Mathf.Lerp(maxTGap, minTGap, hoodSwayingPulse);
		return AttachPos(0, timestacker) + new Vector2(0f - num, 0f);
	}

	public Vector2 MouthRightPos(float timestacker)
	{
		float x = (float)tentacles.Length * Mathf.Lerp(maxTGap, minTGap, hoodSwayingPulse);
		return AttachPos(0, timestacker) + new Vector2(x, 0f);
	}

	private void LightUpdate()
	{
		LightCounter += Random.Range(0.01f, 0.2f);
		if (myLight != null && (myLight.room != room || !myLight.room.BeingViewed))
		{
			myLight.slatedForDeletetion = true;
			myLight = null;
		}
		if (myLight == null && room.BeingViewed)
		{
			LightCounter = Random.Range(0f, 100f);
			myLight = new LightSource(base.firstChunk.pos, environmentalLight: true, coreColor, this);
			room.AddObject(myLight);
			myLight.colorFromEnvironment = false;
			myLight.flat = true;
			myLight.noGameplayImpact = true;
			myLight.stayAlive = true;
			myLight.requireUpKeep = true;
		}
		else if (myLight != null)
		{
			myLight.HardSetPos(base.bodyChunks[CoreChunk].pos);
			myLight.HardSetRad(180f);
			myLight.HardSetAlpha(Mathf.Lerp(0f, 0.765f, (0.5f + (1f - hoodSwayingPulse) / 2f) * room.Darkness(myLight.Pos)));
			myLight.stayAlive = true;
		}
	}

	public override void Die()
	{
		if (!base.dead)
		{
			SMSuckCounter = 100;
			while (grabbedBy.Count > 0)
			{
				grabbedBy[0].Release();
			}
			base.abstractCreature.LoseAllStuckObjects();
			consumedCreatures.Clear();
			BodyChunk[] array = base.bodyChunks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].collideWithObjects = false;
			}
			int num = Random.Range(8, 19);
			for (int j = 0; j < num; j++)
			{
				AbstractConsumable abstractConsumable = new AbstractConsumable(room.world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, base.abstractCreature.pos, room.game.GetNewID(), -1, -1, null);
				abstractConsumable.destroyOnAbstraction = true;
				room.abstractRoom.AddEntity(abstractConsumable);
				abstractConsumable.RealizeInRoom();
				(abstractConsumable.realizedObject as SlimeMold).JellyfishMode = true;
				abstractConsumable.realizedObject.firstChunk.pos += Custom.RNV() * Random.value * 85f;
				abstractConsumable.realizedObject.firstChunk.vel *= 0f;
			}
			num = Random.Range(3, 5);
			for (int k = 0; k < num; k++)
			{
				AbstractConsumable abstractConsumable2 = new AbstractConsumable(room.world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, base.abstractCreature.pos, room.game.GetNewID(), -1, -1, null);
				room.abstractRoom.AddEntity(abstractConsumable2);
				abstractConsumable2.RealizeInRoom();
				abstractConsumable2.realizedObject.firstChunk.pos = base.bodyChunks[CoreChunk].pos + Custom.RNV() * Random.value * 15f;
				abstractConsumable2.realizedObject.firstChunk.vel *= 0f;
			}
			base.Die();
			Destroy();
			base.abstractCreature.Destroy();
		}
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (source != null && source.owner is Spear && (source.owner as Spear).Spear_NeedleCanFeed() && SMSuckCounter == 0)
		{
			room.PlaySound(SoundID.Daddy_Digestion_Init, base.firstChunk);
			SMSuckCounter = 1;
			if ((source.owner as Spear).thrownBy != null && (source.owner as Spear).thrownBy is Player)
			{
				((source.owner as Spear).thrownBy as Player).AddFood(10);
			}
		}
		if (type == DamageType.Explosion && damage >= 1f)
		{
			Die();
		}
		else
		{
			base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
		}
	}

	private void PlayHorrifyingMoo()
	{
		if (mooCounter < 0)
		{
			Custom.Log("Moo!");
			mooCounter = 140;
			room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Terror_Moo, base.bodyChunks[CoreChunk].pos, 1f, 0.75f + Random.value * 0.5f);
			room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Terror_Moo, base.bodyChunks[CoreChunk].pos, 1f, 0.75f + Random.value * 0.5f);
			for (int num = Random.Range(16, 36); num > 0; num--)
			{
				room.AddObject(new Bubble(base.bodyChunks[CoreChunk].pos + new Vector2(Random.Range(-28f, 28f), Random.Range(-38f, 20f)), new Vector2(Random.Range(-4f, 4f), 0f), bottomBubble: false, fakeWaterBubble: false));
			}
		}
	}
}
