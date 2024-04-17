using System;
using RWCustom;
using UnityEngine;

public class BigSpiderGraphics : GraphicsModule
{
	public BigSpider bug;

	public float flip;

	public float lastFlip;

	public Vector2 spikesDir;

	private float darkness;

	private float lastDarkness;

	private Color blackColor;

	public Limb[,] legs;

	public float[,,] legFlips;

	public Vector2[,] legsTravelDirs;

	public int legsDangleCounter;

	public GenericBodyPart[] mandibles;

	public GenericBodyPart tailEnd;

	public float legLength;

	private Vector2 breathDir;

	private float breathCounter;

	private float lastBreathCounter;

	public Vector2[][,] scales;

	public Vector2[] scaleStuckPositions;

	public Vector2[,] scaleSpecs;

	public int totalScales;

	public float lastMandiblesCharge;

	public float mandiblesCharge;

	private float legsThickness;

	private float bodyThickness;

	private IntVector2 deadLeg = new IntVector2(-1, -1);

	public ChunkDynamicSoundLoop soundLoop;

	private float rustleSound;

	private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	private Color yellowCol;

	public int MeshSprite => 0;

	public int HeadSprite => 1;

	public int FirstScaleSprite => 30;

	public int TotalSprites => 30 + totalScales;

	public bool Spitter => bug.spitter;

	public bool Mother => bug.mother;

	public int LegSprite(int side, int leg, int part)
	{
		return 2 + side * 12 + leg * 3 + part;
	}

	public int MandibleSprite(int side, int part)
	{
		return 26 + side * 2 + part;
	}

	public BigSpiderGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		bug = ow as BigSpider;
		tailEnd = new GenericBodyPart(this, 3f, 0.5f, 0.99f, bug.bodyChunks[1]);
		lastDarkness = -1f;
		legLength = (Spitter ? 65f : 55f);
		if (Mother)
		{
			legLength = 90f;
		}
		mandibles = new GenericBodyPart[2];
		for (int i = 0; i < mandibles.GetLength(0); i++)
		{
			mandibles[i] = new GenericBodyPart(this, 1f, 0.5f, 0.9f, base.owner.bodyChunks[0]);
		}
		legs = new Limb[2, 4];
		legFlips = new float[2, 4, 2];
		legsTravelDirs = new Vector2[2, 4];
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				legs[j, k] = new Limb(this, bug.mainBodyChunk, j * 4 + k, 0.1f, 0.7f, 0.99f, Spitter ? 12f : 18f, 0.95f);
			}
		}
		bodyParts = new BodyPart[11];
		bodyParts[0] = tailEnd;
		bodyParts[1] = mandibles[0];
		bodyParts[2] = mandibles[1];
		int num = 3;
		for (int l = 0; l < legs.GetLength(0); l++)
		{
			for (int m = 0; m < legs.GetLength(1); m++)
			{
				bodyParts[num] = legs[l, m];
				num++;
			}
		}
		totalScales = 0;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(bug.abstractCreature.ID.RandomSeed);
		scales = new Vector2[(Spitter ? 10 : 0) + UnityEngine.Random.Range(Spitter ? 16 : 10, UnityEngine.Random.Range(20, 28))][,];
		scaleStuckPositions = new Vector2[scales.Length];
		scaleSpecs = new Vector2[scales.Length, 2];
		legsThickness = Mathf.Lerp(0.7f, 1.1f, UnityEngine.Random.value);
		bodyThickness = Mathf.Lerp(0.9f, 1.1f, UnityEngine.Random.value) + (Spitter ? 1.7f : 0.9f);
		if (Mother)
		{
			bodyThickness = Mathf.Lerp(0.9f, 1.1f, UnityEngine.Random.value) + 5f;
		}
		if (UnityEngine.Random.value < 0.5f)
		{
			deadLeg = new IntVector2(UnityEngine.Random.Range(0, 2), UnityEngine.Random.Range(0, 4));
		}
		num = 0;
		if (Spitter)
		{
			for (int n = 0; n < scales.Length; n++)
			{
				scaleSpecs[n, 0] = new Vector2(0.5f + 0.5f * UnityEngine.Random.value, Mathf.Lerp(3f, 11f, UnityEngine.Random.value));
				scaleSpecs[n, 1] = Custom.RNV();
				scales[n] = new Vector2[UnityEngine.Random.Range(2, UnityEngine.Random.Range(3, 5)), 4];
				totalScales += scales[n].GetLength(0);
				for (int num2 = 0; num2 < scales[n].GetLength(0); num2++)
				{
					scales[n][num2, 3].x = num;
					num++;
				}
				scaleStuckPositions[n] = new Vector2(Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value), Mathf.Lerp(UnityEngine.Random.value, Mathf.InverseLerp(0f, scales.Length - 1, n), 0.5f));
			}
		}
		else
		{
			for (int num3 = 0; num3 < scales.Length; num3++)
			{
				scaleSpecs[num3, 0] = new Vector2(UnityEngine.Random.value, 5f);
				if (num3 % 3 == 0)
				{
					scales[num3] = new Vector2[UnityEngine.Random.Range(2, UnityEngine.Random.Range(7, 13)), 4];
				}
				else
				{
					scales[num3] = new Vector2[UnityEngine.Random.Range(2, UnityEngine.Random.Range(4, 5)), 4];
				}
				totalScales += scales[num3].GetLength(0);
				for (int num4 = 0; num4 < scales[num3].GetLength(0); num4++)
				{
					scales[num3][num4, 3].x = num;
					num++;
				}
				scaleStuckPositions[num3] = new Vector2(Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value), Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap(scales[num3].GetLength(0), 2f, 9f, 0.5f, 2f)));
				if (num3 % 3 == 0 && scaleStuckPositions[num3].y > 0.5f)
				{
					scaleStuckPositions[num3].y *= 0.5f;
				}
			}
		}
		UnityEngine.Random.state = state;
		soundLoop = new ChunkDynamicSoundLoop(bug.mainBodyChunk);
		Reset();
	}

	public override void Reset()
	{
		base.Reset();
		float num = (ModManager.MMF ? 0f : 2f);
		for (int i = 0; i < scales.Length; i++)
		{
			for (int j = 0; j < scales[i].GetLength(0); j++)
			{
				scales[i][j, 0] = bug.mainBodyChunk.pos;
				scales[i][j, 1] = bug.mainBodyChunk.pos;
				scales[i][j, 2] *= num;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		lastMandiblesCharge = mandiblesCharge;
		mandiblesCharge = Mathf.InverseLerp(0.1f, 0.7f, bug.mandiblesCharged);
		soundLoop.Update();
		soundLoop.sound = SoundID.Big_Spider_Rustle_LOOP;
		if (bug.revivingBuddy != null)
		{
			rustleSound = 1f;
		}
		soundLoop.Volume = Mathf.Lerp(rustleSound * 0.5f, 1f, mandiblesCharge);
		soundLoop.Pitch = Custom.LerpMap(Vector2.Distance(bug.bodyChunks[1].lastLastPos - bug.bodyChunks[1].lastPos, bug.bodyChunks[1].lastPos - bug.bodyChunks[1].pos), 1f, 3f, 0.9f, 1.1f) + bug.mandiblesCharged * 0.25f;
		lastBreathCounter = breathCounter;
		if (!bug.dead)
		{
			breathCounter += UnityEngine.Random.value;
			if (!bug.Consious && bug.deathConvulsions > 0f)
			{
				soundLoop.Volume = Mathf.Max(soundLoop.Volume, 0.5f * bug.deathConvulsions);
			}
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
		tailEnd.Update();
		tailEnd.ConnectToPoint(bug.bodyChunks[1].pos + Custom.DirVec(bug.bodyChunks[0].pos, bug.bodyChunks[1].pos) * 7f + Custom.PerpendicularVector(bug.bodyChunks[0].pos, bug.bodyChunks[1].pos) * flip * 10f, Spitter ? 12f : 6f, push: false, 0.2f, base.owner.bodyChunks[1].vel, 0.5f, 0.1f);
		tailEnd.vel.y -= 0.4f;
		tailEnd.vel += Custom.DirVec(bug.bodyChunks[0].pos, bug.bodyChunks[1].pos) * 0.8f;
		if (!bug.dead)
		{
			tailEnd.vel += breathDir * 0.7f;
			if (UnityEngine.Random.value < 0.1f)
			{
				breathDir = Custom.RNV() * UnityEngine.Random.value;
			}
		}
		float num = Custom.AimFromOneVectorToAnother(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos);
		Vector2 vector = Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos);
		BodyChunk bodyChunk = ((bug.grasps[0] != null) ? bug.grasps[0].grabbedChunk : null);
		for (int j = 0; j < mandibles.Length; j++)
		{
			mandibles[j].Update();
			mandibles[j].ConnectToPoint(bug.mainBodyChunk.pos + vector * 10f * (1f - mandiblesCharge), 10f + 10f * mandiblesCharge, push: false, 0f, bug.mainBodyChunk.vel, 0.3f, 0.05f);
			float num2 = Mathf.Lerp((j == 0) ? (-1f) : 1f, flip * (1f - 2f * mandiblesCharge), Mathf.Abs(flip * (1f - 2f * mandiblesCharge)) * 0.85f);
			mandibles[j].vel += Vector2.Lerp((vector + Custom.PerpendicularVector(vector) * num2 * Mathf.Lerp(2.2f, 1.1f, Mathf.Abs(flip))).normalized * (1f - mandiblesCharge), (bug.mainBodyChunk.pos + (Custom.PerpendicularVector(vector) * num2 + vector * Mathf.Lerp(2f, 0.5f, bug.mandiblesCharged)).normalized * 20f - mandibles[j].pos) / 7f, mandiblesCharge);
			if (bodyChunk != null)
			{
				mandibles[j].pos = Vector2.Lerp(mandibles[j].pos, bodyChunk.pos, 0.5f);
				mandibles[j].vel *= 0.7f;
			}
			if (bug.Consious)
			{
				mandibles[j].vel += Custom.RNV() * UnityEngine.Random.value;
			}
		}
		float num3 = 0f;
		float num4 = 0f;
		int num5 = 0;
		for (int k = 0; k < legs.GetLength(0); k++)
		{
			for (int l = 0; l < legs.GetLength(1); l++)
			{
				num4 += Custom.LerpMap(Vector2.Dot(Custom.DirVec(bug.mainBodyChunk.pos, legs[k, l].absoluteHuntPos), bug.travelDir.normalized), -0.6f, 0.6f, 0f, 0.125f);
				num3 += Custom.DistanceToLine(legs[k, l].pos, bug.bodyChunks[1].pos, bug.bodyChunks[0].pos);
				if (legs[k, l].OverLappingHuntPos)
				{
					num5++;
				}
			}
		}
		num4 *= Mathf.InverseLerp(0f, 0.1f, bug.travelDir.magnitude);
		flip = Custom.LerpAndTick(flip, Mathf.Clamp(num3 / 40f, -1f, 1f), 0.07f, 0.1f);
		if (legsDangleCounter > 0)
		{
			num4 = 1f;
		}
		float num6 = 0f;
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
		int num7 = 0;
		spikesDir *= 0f;
		for (int m = 0; m < legs.GetLength(1); m++)
		{
			for (int n = 0; n < legs.GetLength(0); n++)
			{
				float num8 = Mathf.InverseLerp(0f, legs.GetLength(1) - 1, m);
				float num9 = 0.5f + 0.5f * Mathf.Sin((bug.runCycle + (float)num7 * 0.25f) * (float)Math.PI);
				spikesDir += Custom.DirVec(bug.mainBodyChunk.pos, legs[n, m].pos) * (1f - num8);
				legFlips[n, m, 1] = legFlips[n, m, 0];
				if (UnityEngine.Random.value < num9 * 0.5f && !Custom.DistLess(legs[n, m].lastPos, legs[n, m].pos, 2f))
				{
					if (UnityEngine.Random.value < num9)
					{
						legFlips[n, m, 0] = Custom.LerpAndTick(legFlips[n, m, 0], Mathf.Lerp((n == 0) ? (-1f) : 1f, flip, Mathf.Abs(flip)), 0.01f, UnityEngine.Random.value / 6f);
					}
					if (UnityEngine.Random.value < num9)
					{
						legsTravelDirs[n, m] = Vector2.Lerp(legsTravelDirs[n, m], bug.travelDir, Mathf.Pow(UnityEngine.Random.value, 1f - 0.9f * num9));
					}
				}
				if (!bug.Consious && UnityEngine.Random.value < bug.deathConvulsions)
				{
					legsTravelDirs[n, m] = Custom.RNV() * UnityEngine.Random.value;
				}
				else if (bug.charging > 0f)
				{
					legsTravelDirs[n, m] *= 0f;
				}
				legs[n, m].Update();
				if (bug.grabChunks[n, m] != null)
				{
					Vector2 vector2 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(bug.mainBodyChunk.pos, bug.grabChunks[n, m].pos) + Mathf.Lerp(10f, 170f, num8) * ((n == 0) ? 1f : (-1f)));
					legs[n, m].pos = bug.grabChunks[n, m].pos + vector2 * Mathf.Max(0f, bug.grabChunks[n, m].rad - 2f);
					legs[n, m].vel *= 0.7f;
					legs[n, m].mode = Limb.Mode.Dangle;
				}
				else
				{
					if (legs[n, m].mode == Limb.Mode.HuntRelativePosition || legsDangleCounter > 0 || (deadLeg.x == n && deadLeg.y == m))
					{
						legs[n, m].mode = Limb.Mode.Dangle;
					}
					Vector2 vector3 = Custom.DegToVec(num + Mathf.Lerp(40f, 160f, num8) * ((num6 != 0f) ? (0f - num6) : ((n == 0) ? 1f : (-1f))));
					Vector2 vector4 = bug.bodyChunks[0].pos + (Vector2)Vector3.Slerp(legsTravelDirs[n, m], vector3, 0.1f) * legLength * 0.85f * Mathf.Pow(num9, 0.5f);
					legs[n, m].ConnectToPoint(vector4, legLength, push: false, 0f, bug.mainBodyChunk.vel, 0.1f, 0f);
					legs[n, m].ConnectToPoint(bug.bodyChunks[0].pos, legLength, push: false, 0f, bug.mainBodyChunk.vel, 0.1f, 0f);
					if (legsDangleCounter > 0 || num9 < 0.1f || (deadLeg.x == n && deadLeg.y == m))
					{
						Vector2 vector5 = vector4 + vector3 * legLength * 0.5f;
						if (!bug.Consious)
						{
							vector5 = vector4 + legsTravelDirs[n, m] * legLength * 0.5f;
						}
						legs[n, m].vel = Vector2.Lerp(legs[n, m].vel, vector5 - legs[n, m].pos, 0.05f);
						legs[n, m].vel.y -= 0.4f;
						if (bug.Consious && (deadLeg.x != n || deadLeg.y != m))
						{
							legs[n, m].vel += Custom.RNV() * 3f;
						}
					}
					else
					{
						Vector2 vector6 = vector4 + vector3 * legLength;
						for (int num10 = 0; num10 < legs.GetLength(0); num10++)
						{
							for (int num11 = 0; num11 < legs.GetLength(1); num11++)
							{
								if (num10 != n && num11 != m && Custom.DistLess(vector6, legs[num10, num11].absoluteHuntPos, legLength * 0.1f))
								{
									vector6 = legs[num10, num11].absoluteHuntPos + Custom.DirVec(legs[num10, num11].absoluteHuntPos, vector6) * legLength * 0.1f;
								}
							}
						}
						float num12 = 1.2f;
						if (!legs[n, m].reachedSnapPosition)
						{
							legs[n, m].FindGrip(bug.room, vector4, vector4, legLength * num12, vector6, -2, -2, behindWalls: true);
						}
						else if (!Custom.DistLess(vector4, legs[n, m].absoluteHuntPos, legLength * num12 * Mathf.Pow(1f - num9, 0.2f)))
						{
							legs[n, m].mode = Limb.Mode.Dangle;
						}
					}
				}
				num7++;
			}
		}
		spikesDir /= 4f;
		if (!culled)
		{
			for (int num13 = 0; num13 < scales.Length; num13++)
			{
				Vector2 vector7 = ScaleDir(num13) * (Spitter ? 1.5f : 1f);
				Vector2 vector8 = Vector2.Lerp(Custom.RotateAroundOrigo(scaleSpecs[num13, 1], num), -vector, 0.5f);
				for (int num14 = 0; num14 < scales[num13].GetLength(0); num14++)
				{
					scales[num13][num14, 1] = scales[num13][num14, 0];
					scales[num13][num14, 0] += scales[num13][num14, 2];
					scales[num13][num14, 2] *= 0.9f;
					scales[num13][num14, 2].y -= 0.9f;
					if (scales[num13].GetLength(0) > 4)
					{
						SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(scales[num13][num14, 0], scales[num13][num14, 1], scales[num13][num14, 2], 1f, new IntVector2(0, 0), bug.GoThroughFloors);
						cd = SharedPhysics.HorizontalCollision(bug.room, cd);
						cd = SharedPhysics.VerticalCollision(bug.room, cd);
						cd = SharedPhysics.SlopesVertically(bug.room, cd);
						scales[num13][num14, 0] = cd.pos;
						scales[num13][num14, 2] = cd.vel;
					}
					scales[num13][num14, 2] += vector7 * Custom.LerpMap(num14, 0f, 8f, 4f, 0f, 0.5f);
					if (Spitter)
					{
						scales[num13][num14, 2] += vector8 * Mathf.InverseLerp(scales[num13].GetLength(0) / 2, scales[num13].GetLength(0) - 1, num14) * 3f;
					}
					if (num14 == 0)
					{
						Vector2 vector9 = ScaleAttachPos(num13, 1f);
						scales[num13][num14, 2] += Custom.DirVec(scales[num13][num14, 0], vector9) * (Vector2.Distance(scales[num13][num14, 0], vector9) - scaleSpecs[num13, 0].y * 2.4f);
						scales[num13][num14, 0] += Custom.DirVec(scales[num13][num14, 0], vector9) * (Vector2.Distance(scales[num13][num14, 0], vector9) - scaleSpecs[num13, 0].y * 2.4f);
					}
					else
					{
						Vector2 vector10 = Custom.DirVec(scales[num13][num14, 0], scales[num13][num14 - 1, 0]) * (Vector2.Distance(scales[num13][num14, 0], scales[num13][num14 - 1, 0]) - scaleSpecs[num13, 0].y) * 0.5f;
						scales[num13][num14, 2] += vector10;
						scales[num13][num14, 0] += vector10;
						scales[num13][num14 - 1, 2] -= vector10;
						scales[num13][num14 - 1, 0] -= vector10;
					}
					if (num14 > 1)
					{
						scales[num13][num14, 2] -= Custom.DirVec(scales[num13][num14, 0], scales[num13][num14 - 2, 0]) * 2f;
						scales[num13][num14 - 2, 2] += Custom.DirVec(scales[num13][num14, 0], scales[num13][num14 - 2, 0]) * 2f;
					}
					if (num14 > 2)
					{
						scales[num13][num14, 2] -= Custom.DirVec(scales[num13][num14, 0], scales[num13][num14 - 3, 0]) * 1f;
						scales[num13][num14 - 3, 2] += Custom.DirVec(scales[num13][num14, 0], scales[num13][num14 - 3, 0]) * 1f;
					}
				}
			}
			rustleSound = 0f;
			for (int num15 = 0; num15 < scales.Length; num15++)
			{
				Vector2 a = scales[num15][scales[num15].GetLength(0) - 1, 0] - scales[num15][scales[num15].GetLength(0) - 1, 1];
				rustleSound += Vector2.Distance(a, bug.bodyChunks[1].pos - bug.bodyChunks[1].lastPos);
			}
			rustleSound = Mathf.Pow(Mathf.InverseLerp(1f, Spitter ? 10f : 15f, rustleSound / (float)scales.Length), 0.7f);
		}
		else
		{
			rustleSound = Mathf.Max(1f, rustleSound - 1f / 30f);
		}
	}

	public Vector2 ScaleAttachPos(int scl, float timeStacker)
	{
		float num = Mathf.Lerp(lastFlip, flip, timeStacker) * (Spitter ? 0.25f : 1f);
		float num2 = Mathf.InverseLerp(Spitter ? 0.2f : 0.5f, 0f, scaleStuckPositions[scl].y);
		Vector2 vector = Vector2.Lerp(bug.bodyChunks[1].lastPos, bug.bodyChunks[1].pos, timeStacker) + Custom.RNV() * UnityEngine.Random.value * 3.5f * Mathf.Lerp(lastMandiblesCharge, mandiblesCharge, timeStacker);
		if (Spitter)
		{
			vector = Vector2.Lerp(vector, Vector2.Lerp(bug.bodyChunks[0].lastPos, bug.bodyChunks[0].pos, timeStacker), Mathf.InverseLerp(0.5f, 1f, scaleStuckPositions[scl].y) * 0.7f);
		}
		Vector2 vector2 = Custom.DirVec(vector, Vector2.Lerp(bug.bodyChunks[0].lastPos, bug.bodyChunks[0].pos, timeStacker));
		return Vector2.Lerp(vector, Vector2.Lerp(tailEnd.lastPos, tailEnd.pos, timeStacker), num2 * 0.6f) + Custom.PerpendicularVector(vector2) * (scaleStuckPositions[scl].x - 0.5f * num) * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(scaleStuckPositions[scl].y * (float)Math.PI)), 2f) * (Spitter ? 14f : 7f) + vector2 * Mathf.Lerp(-1f, 1f, scaleStuckPositions[scl].y) * (Spitter ? 14f : 7f) * Mathf.Pow(1f - num2, 2f);
	}

	public Vector2 ScaleDir(int scl)
	{
		Vector2 vector = bug.bodyChunks[1].pos + Custom.RNV() * UnityEngine.Random.value * 3.5f * mandiblesCharge;
		if (Spitter)
		{
			vector = Vector2.Lerp(vector, bug.bodyChunks[0].pos, Mathf.InverseLerp(0.5f, 1f, scaleStuckPositions[scl].y) * 0.7f);
		}
		Vector2 vector2 = Custom.DirVec(vector, bug.bodyChunks[0].pos);
		float t = Mathf.InverseLerp(Spitter ? 0.2f : 0.5f, 0f, scaleStuckPositions[scl].y);
		vector2 = Vector3.Slerp(vector2, Custom.DirVec(tailEnd.pos, vector), t);
		Vector2 p = ScaleAttachPos(scl, 1f);
		Vector2 normalized = ((Custom.DirVec(vector, p) - vector2 * (1f - mandiblesCharge)) / 2f - spikesDir * scaleStuckPositions[scl].y).normalized;
		if (Spitter)
		{
			return normalized;
		}
		normalized = Custom.RotateAroundOrigo(normalized, Custom.VecToDeg(vector2));
		normalized.x *= 1f + (Spitter ? 1f : 3f) * scaleStuckPositions[scl].y * (0.5f + 0.5f * Mathf.Sin(breathCounter / 10f)) + 3f * mandiblesCharge;
		return Custom.RotateAroundOrigo(normalized, 0f - Custom.VecToDeg(vector2)).normalized;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[MeshSprite] = TriangleMesh.MakeLongMesh(7, pointyTip: false, customColor: false);
		sLeaser.sprites[MeshSprite].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
		sLeaser.sprites[MeshSprite].alpha = (Spitter ? 0.5f : 0.75f);
		sLeaser.sprites[HeadSprite] = new FSprite("Circle20");
		sLeaser.sprites[HeadSprite].scaleX = (Spitter ? 14f : 11f) / 20f;
		sLeaser.sprites[HeadSprite].scaleY = (Spitter ? 16f : 14f) / 20f;
		for (int i = 0; i < legs.GetLength(0); i++)
		{
			for (int j = 0; j < legs.GetLength(1); j++)
			{
				sLeaser.sprites[LegSprite(i, j, 0)] = new FSprite("CentipedeLegA");
				sLeaser.sprites[LegSprite(i, j, 1)] = new FSprite("SpiderLeg" + j + "A");
				if (j == 0)
				{
					sLeaser.sprites[LegSprite(i, j, 2)] = new FSprite("CentipedeLegB");
				}
				else
				{
					sLeaser.sprites[LegSprite(i, j, 2)] = new FSprite("SpiderLeg" + j + "B");
				}
			}
		}
		for (int k = 0; k < mandibles.Length; k++)
		{
			sLeaser.sprites[MandibleSprite(k, 0)] = new FSprite("SpiderLeg0A");
			sLeaser.sprites[MandibleSprite(k, 1)] = new CustomFSprite("SpiderLeg0B");
		}
		for (int l = 0; l < totalScales; l++)
		{
			sLeaser.sprites[FirstScaleSprite + l] = new FSprite("LizardScaleA1");
			sLeaser.sprites[FirstScaleSprite + l].anchorY = 0f;
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
		float num = Mathf.Lerp(lastFlip, flip, timeStacker);
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
		float num2 = Mathf.Lerp(lastMandiblesCharge, mandiblesCharge, timeStacker);
		Vector2 vector2 = Vector2.Lerp(bug.bodyChunks[1].lastPos, bug.bodyChunks[1].pos, timeStacker) + Custom.RNV() * UnityEngine.Random.value * 3.5f * num2;
		Vector2 vector3 = Custom.DirVec(vector2, vector);
		Vector2 vector4 = Custom.PerpendicularVector(vector3);
		Vector2 b = Vector2.Lerp(tailEnd.lastPos, tailEnd.pos, timeStacker);
		sLeaser.sprites[HeadSprite].x = vector.x - camPos.x;
		sLeaser.sprites[HeadSprite].y = vector.y - camPos.y;
		sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(vector3);
		Vector2 vector5 = vector + vector3;
		float num3 = 0f;
		for (int i = 0; i < 7; i++)
		{
			float f = Mathf.InverseLerp(0f, 6f, i);
			Vector2 vector6 = Custom.Bezier(vector + vector3 * 3f, vector2, b, vector2, f);
			float num4 = Mathf.Lerp(2.5f, 10f + Mathf.Sin(Mathf.Lerp(lastBreathCounter, breathCounter, timeStacker) / 10f), Mathf.Sin(Mathf.Pow(f, 0.75f) * (float)Math.PI)) * bodyThickness;
			Vector2 vector7 = Custom.PerpendicularVector(vector6, vector5);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(i * 4, (vector5 + vector6) / 2f - vector7 * (num4 + num3) * 0.5f - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(i * 4 + 1, (vector5 + vector6) / 2f + vector7 * (num4 + num3) * 0.5f - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector6 - vector7 * num4 - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector6 + vector7 * num4 - camPos);
			vector5 = vector6;
			num3 = num4;
		}
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				float t = Mathf.InverseLerp(0f, legs.GetLength(1) - 1, k);
				Vector2 vector8 = Vector2.Lerp(vector, vector2, 0.3f);
				vector8 += vector4 * ((j == 0) ? (-1f) : 1f) * 3f * (1f - Mathf.Abs(num));
				vector8 += vector3 * Mathf.Lerp(10f, -6f, t);
				Vector2 vector9 = Vector2.Lerp(legs[j, k].lastPos, legs[j, k].pos, timeStacker);
				float f2 = Mathf.Lerp(legFlips[j, k, 1], legFlips[j, k, 0], timeStacker);
				Vector2 a = Custom.InverseKinematic(vector8, vector9, legLength * 0.7f, legLength * 0.7f, f2);
				a = Vector2.Lerp(a, (vector8 + vector9) * 0.5f - vector4 * num * Custom.LerpMap(Vector2.Distance(vector8, vector9), 0f, legLength * 1.3f, legLength * 0.7f, 0f, 3f), Mathf.Abs(num));
				Vector2 vector10 = Vector2.Lerp(vector8, a, 0.5f);
				Vector2 vector11 = Vector2.Lerp(a, vector9, 0.5f);
				float num5 = legLength / 4f;
				Vector2 vector12 = Vector2.Lerp(vector10, vector11, 0.5f);
				vector10 = vector12 + Custom.DirVec(vector12, vector10) * num5 / 2f;
				vector11 = vector12 + Custom.DirVec(vector12, vector11) * num5 / 2f;
				sLeaser.sprites[LegSprite(j, k, 0)].x = vector8.x - camPos.x;
				sLeaser.sprites[LegSprite(j, k, 0)].y = vector8.y - camPos.y;
				sLeaser.sprites[LegSprite(j, k, 0)].rotation = Custom.AimFromOneVectorToAnother(vector8, vector10);
				sLeaser.sprites[LegSprite(j, k, 0)].scaleY = Vector2.Distance(vector8, vector10) / sLeaser.sprites[LegSprite(j, k, 0)].element.sourcePixelSize.y;
				sLeaser.sprites[LegSprite(j, k, 0)].anchorY = 0f;
				sLeaser.sprites[LegSprite(j, k, 0)].scaleX = (0f - Mathf.Sign(f2)) * 1.5f * legsThickness;
				sLeaser.sprites[LegSprite(j, k, 1)].x = vector10.x - camPos.x;
				sLeaser.sprites[LegSprite(j, k, 1)].y = vector10.y - camPos.y;
				sLeaser.sprites[LegSprite(j, k, 1)].rotation = Custom.AimFromOneVectorToAnother(vector10, vector11);
				sLeaser.sprites[LegSprite(j, k, 1)].scaleY = (Vector2.Distance(vector10, vector11) + 2f) / sLeaser.sprites[LegSprite(j, k, 1)].element.sourcePixelSize.y;
				sLeaser.sprites[LegSprite(j, k, 1)].anchorY = 0.1f;
				sLeaser.sprites[LegSprite(j, k, 1)].scaleX = (0f - Mathf.Sign(f2)) * 1.2f * legsThickness;
				sLeaser.sprites[LegSprite(j, k, 2)].anchorY = 0.1f;
				sLeaser.sprites[LegSprite(j, k, 2)].scaleX = (0f - Mathf.Sign(f2)) * 1.2f * legsThickness;
				sLeaser.sprites[LegSprite(j, k, 2)].x = vector11.x - camPos.x;
				sLeaser.sprites[LegSprite(j, k, 2)].y = vector11.y - camPos.y;
				sLeaser.sprites[LegSprite(j, k, 2)].rotation = Custom.AimFromOneVectorToAnother(vector11, vector9);
				sLeaser.sprites[LegSprite(j, k, 2)].scaleY = (Vector2.Distance(vector11, vector9) + 1f) / sLeaser.sprites[LegSprite(j, k, 2)].element.sourcePixelSize.y;
			}
		}
		for (int l = 0; l < 2; l++)
		{
			float num6 = Mathf.Lerp((l == 0) ? 1f : (-1f), num, Mathf.Pow(Mathf.Abs(num), 2f));
			Vector2 vector13 = vector + vector3 * 4f + vector4 * num6 * -3f;
			Vector2 vector14 = Vector2.Lerp(mandibles[l].lastPos, mandibles[l].pos, timeStacker) + Custom.RNV() * UnityEngine.Random.value * 4f * num2;
			Vector2 vector15 = Custom.InverseKinematic(vector13, vector14, 13f, 12f, num6);
			sLeaser.sprites[MandibleSprite(l, 0)].x = vector13.x - camPos.x;
			sLeaser.sprites[MandibleSprite(l, 0)].y = vector13.y - camPos.y;
			sLeaser.sprites[MandibleSprite(l, 0)].anchorY = 0f;
			sLeaser.sprites[MandibleSprite(l, 0)].rotation = Custom.AimFromOneVectorToAnother(vector13, vector15);
			sLeaser.sprites[MandibleSprite(l, 0)].scaleY = Vector2.Distance(vector13, vector15) / sLeaser.sprites[MandibleSprite(l, 0)].element.sourcePixelSize.y;
			sLeaser.sprites[MandibleSprite(l, 0)].scaleX = 0f - Mathf.Sign(num6);
			Vector2 vector16 = Custom.PerpendicularVector(vector15, vector14);
			float num7 = 4.6f * Mathf.Sign(num6) * Mathf.Lerp(0.5f, 1f, Mathf.Abs(num6));
			(sLeaser.sprites[MandibleSprite(l, 1)] as CustomFSprite).MoveVertice(0, vector14 + vector16 * num7 - camPos);
			(sLeaser.sprites[MandibleSprite(l, 1)] as CustomFSprite).MoveVertice(1, vector14 - vector16 * num7 - camPos);
			num7 = Mathf.Lerp(num7, 2.5f * Mathf.Sign(num6), 0.5f);
			(sLeaser.sprites[MandibleSprite(l, 1)] as CustomFSprite).MoveVertice(2, vector15 - vector16 * num7 - camPos);
			(sLeaser.sprites[MandibleSprite(l, 1)] as CustomFSprite).MoveVertice(3, vector15 + vector16 * num7 - camPos);
			if (!Spitter)
			{
				(sLeaser.sprites[MandibleSprite(l, 1)] as CustomFSprite).verticeColors[2] = blackColor;
				(sLeaser.sprites[MandibleSprite(l, 1)] as CustomFSprite).verticeColors[3] = blackColor;
				(sLeaser.sprites[MandibleSprite(l, 1)] as CustomFSprite).verticeColors[0] = Color.Lerp(yellowCol, blackColor, 0.6f * (1f - num2) + 0.4f * darkness);
				(sLeaser.sprites[MandibleSprite(l, 1)] as CustomFSprite).verticeColors[1] = Color.Lerp(yellowCol, blackColor, 0.6f * (1f - num2) + 0.4f * darkness);
			}
		}
		for (int m = 0; m < scales.Length; m++)
		{
			Vector2 vector17 = Vector2.Lerp(Vector2.Lerp(tailEnd.lastPos, tailEnd.pos, timeStacker), vector2, scaleStuckPositions[m].y);
			for (int n = 0; n < scales[m].GetLength(0); n++)
			{
				Vector2 vector18 = Vector2.Lerp(scales[m][n, 1], scales[m][n, 0], timeStacker);
				float num8 = Mathf.InverseLerp(0f, scales[m].GetLength(0) - 1, n);
				sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].x = vector17.x - camPos.x;
				sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].y = vector17.y - camPos.y;
				sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].rotation = Custom.AimFromOneVectorToAnother(vector17, vector18);
				sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].scaleY = Vector2.Distance(vector18, vector17) / sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].element.sourcePixelSize.y;
				sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].scaleX = (Spitter ? Mathf.Lerp(1.2f, 0.9f, num8) : Mathf.Lerp(0.9f, 0.4f, Mathf.Pow(num8, 0.4f)));
				vector17 = vector18;
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		float num = 0f;
		if (ModManager.MSC && (base.owner as Creature).abstractCreature.Winterized)
		{
			blackColor = Color.Lerp(palette.texture.GetPixel(16, 2), palette.blackColor, Mathf.Min(0.4f, palette.darkness));
			yellowCol = bug.yellowCol;
			num = 1f;
		}
		else
		{
			blackColor = palette.blackColor;
			yellowCol = Color.Lerp(bug.yellowCol, palette.fogColor, 0.2f);
			num = 1f - darkness;
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = blackColor;
		}
		for (int j = 0; j < 2; j++)
		{
			for (int k = 0; k < 4; k++)
			{
				(sLeaser.sprites[MandibleSprite(j, 1)] as CustomFSprite).verticeColors[k] = blackColor;
			}
		}
		for (int l = 0; l < scales.Length; l++)
		{
			for (int m = 0; m < scales[l].GetLength(0); m++)
			{
				float num2 = (Mathf.InverseLerp(0f, scales[l].GetLength(0) - 1, m) + Mathf.InverseLerp(0f, 5f, m)) / 2f;
				sLeaser.sprites[FirstScaleSprite + (int)scales[l][m, 3].x].color = Color.Lerp(blackColor, yellowCol, num2 * Mathf.Lerp(0.3f, 0.9f, scaleSpecs[l, 0].x) * num);
			}
		}
	}
}
