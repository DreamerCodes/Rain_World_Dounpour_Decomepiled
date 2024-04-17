using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class DeerGraphics : GraphicsModule, HasDanglers, ILookingAtCreatures
{
	public class Antlers
	{
		public class Part
		{
			public int lastBranchingSegment = 2;

			public Vector3[] positions;

			public float[] rads;

			public Vector3[,] indPos;

			public Vector3 GetTransoformedPos(int pos, float flip)
			{
				return positions[pos] + indPos[(!(flip < 0f)) ? 1u : 0u, pos];
			}

			public Part(Vector3[] positions, float[] rads)
			{
				this.positions = positions;
				this.rads = rads;
			}

			public void GenerateInds(float rad)
			{
				indPos = new Vector3[2, positions.Length];
				for (int i = 0; i < 2; i++)
				{
					Vector3 vector = UnityEngine.Random.onUnitSphere * 11f * Mathf.Pow(UnityEngine.Random.value, 0.75f);
					for (int j = lastBranchingSegment + 2; j < positions.Length; j++)
					{
						indPos[i, j] += vector;
						vector *= 1.1f;
						vector += UnityEngine.Random.onUnitSphere * 2f * UnityEngine.Random.value;
					}
				}
			}
		}

		public class GenerateValues
		{
			public float circumferenceTend;

			public float goalTend;

			public float randomTend;

			public float circumferenceTendChange;

			public float goalTendChange;

			public float randomTendChange;

			public float attractRad;

			public GenerateValues(float rad)
			{
				attractRad = rad * Mathf.Lerp(0.7f, 1f, UnityEngine.Random.value);
				circumferenceTend = Mathf.Lerp(0f, 0.01f, UnityEngine.Random.value);
				goalTend = Mathf.Lerp(0f, 0.02f, UnityEngine.Random.value);
				randomTend = Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value);
				circumferenceTendChange = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.01f * 0.5f;
				goalTendChange = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.02f * 0.5f;
				randomTendChange = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.1f * 0.5f;
			}
		}

		public Part[] parts;

		public float rad;

		public float thickness;

		public int SpritesClaimed => parts.Length * 2;

		public Antlers(float rad, float thickness)
		{
			this.rad = rad;
			this.thickness = thickness;
			List<Part> list = new List<Part>();
			GenerateValues generateValues = new GenerateValues(rad);
			Part part = GeneratePart(spine: true, 5f, null, new Vector3(0f, 0f - rad), Custom.DegToVec(Mathf.Lerp(10f, 45f, UnityEngine.Random.value)) * rad, new Vector3(Mathf.Lerp(0.4f, 1f, UnityEngine.Random.value), 1f, Mathf.Lerp(-1f, 1f, UnityEngine.Random.value)).normalized, generateValues);
			generateValues.goalTend = Mathf.Lerp(generateValues.goalTend, 0.2f, 0.5f);
			list = new List<Part> { part };
			int num = part.positions.Length;
			int num2 = 30;
			Vector3[] array = new Vector3[num2];
			Vector3 vector = UnityEngine.Random.onUnitSphere;
			for (int i = 0; i < num2; i++)
			{
				array[i] = vector;
				vector = (-vector + UnityEngine.Random.onUnitSphere).normalized;
				if (array[i].x < 0f)
				{
					array[i].x *= -1f;
				}
			}
			array[0].z -= 1f;
			array[0].Normalize();
			array[1].z += 1f;
			array[1].Normalize();
			for (int j = 0; j < num2; j++)
			{
				if (num >= 80)
				{
					break;
				}
				array[j] *= rad * Mathf.Lerp(0.2f, 2f, Mathf.Pow(UnityEngine.Random.value, 0.45f));
				float num3 = float.MaxValue;
				int num4 = 0;
				int index = 0;
				for (int k = 0; k < list.Count; k++)
				{
					for (int l = 0; l < list[k].positions.Length - 1; l++)
					{
						float num5 = Vector3.Distance(list[k].positions[l], array[j]);
						float num6 = Mathf.Lerp(num5, list[k].positions[l].y + rad, Mathf.InverseLerp(0f, rad * 2.5f, num5) * 0.15f);
						num6 *= 2f - Mathf.Pow(Mathf.Sin(Mathf.Pow((float)l / (float)(list[k].positions.Length - 2), 1f) * (float)Math.PI), 0.5f);
						if (num6 < num3)
						{
							num4 = l;
							num3 = num6;
							index = k;
						}
					}
				}
				num4 = Math.Max(1, num4 - Mathf.FloorToInt(Vector3.Distance(list[index].positions[num4], array[j]) / 30f));
				list[index].lastBranchingSegment = num4;
				Vector3 start = list[index].positions[num4];
				Vector3 normalized = (list[index].positions[num4] - list[index].positions[num4 - 1]).normalized;
				Part part2 = GeneratePart(beforeStart: list[index].positions[num4 - 1], spine: false, startRad: list[index].rads[num4], start: start, goal: array[j], initDir: normalized, genVals: generateValues);
				if (part2 != null)
				{
					list.Add(part2);
					num += part2.positions.Length;
				}
			}
			parts = list.ToArray();
			for (int m = 0; m < parts.Length; m++)
			{
				parts[m].GenerateInds(rad);
			}
		}

		private Part GeneratePart(bool spine, float startRad, Vector3? beforeStart, Vector3 start, Vector3 goal, Vector3 initDir, GenerateValues genVals)
		{
			List<Vector3> list = new List<Vector3>();
			List<float> list2 = new List<float>();
			if (beforeStart.HasValue)
			{
				list.Add(beforeStart.Value);
				list2.Add(startRad * 0.5f);
			}
			Vector3 vector = start;
			Vector3 vector2 = initDir;
			float num = genVals.circumferenceTend;
			float attractRad = genVals.attractRad;
			float num2 = genVals.goalTend;
			float num3 = genVals.randomTend;
			float num4 = 10f;
			int num5 = Mathf.FloorToInt(Vector3.Distance(start, goal) / num4);
			if (num5 > 2)
			{
				for (int i = 0; i < num5; i++)
				{
					float f = (float)i / (float)(num5 - 1);
					list.Add(vector);
					float a = Mathf.Lerp(startRad, 1.2f, Mathf.Pow(f, 0.5f));
					a = Mathf.Lerp(a, Custom.LerpMap(Vector3.Distance(vector, new Vector3(0f, rad * -0.5f, 0f)), 0f, rad * 1.5f, 6f, 1.2f), 0.5f);
					list2.Add(a);
					vector += vector2 * num4;
					if (spine)
					{
						vector.z -= 11f * Mathf.Sin((float)i / (float)(num5 - 1) * (float)Math.PI);
					}
					if (vector.x < 15f)
					{
						vector.x = 15f;
					}
					vector = Vector3.Lerp(vector, goal, Mathf.Pow(f, 4f));
					vector2 = (vector2 + UnityEngine.Random.onUnitSphere * num3 + vector.normalized * (attractRad - vector.magnitude) * ((vector.magnitude > rad) ? Mathf.Max(num, 0.2f) : num) + (goal - vector).normalized * num2).normalized;
					num = Mathf.Clamp(num + genVals.circumferenceTendChange, 0f, 0.5f);
					num2 = Mathf.Clamp(num2 + genVals.goalTendChange, 0f, 0.5f);
					num3 = Mathf.Clamp(num3 + genVals.randomTendChange, 0f, 0.5f);
				}
				Vector3 a2 = Vector3.Lerp(list[list.Count - 1], list[list.Count - 2], 0.75f);
				a2 = Vector3.Lerp(a2, list[list.Count - 1] - (list[list.Count - 1] - list[list.Count - 2]).normalized * 3f, 1f);
				list.Insert(list.Count - 1, a2);
				list2.Insert(list2.Count - 1, Mathf.Lerp(startRad, Custom.LerpMap(Vector3.Distance(goal, new Vector3(0f, rad * -0.5f, 0f)), 0f, rad * 1.5f, 7f, 4f), 0.5f));
				list2[list2.Count - 1] *= 0.15f;
				return new Part(list.ToArray(), list2.ToArray());
			}
			return null;
		}

		public Vector2 TransformToHeadRotat(Vector3 dpPos, Vector2 antlerPos, float rotation, float flip, float deerFaceDir)
		{
			float degAng = Mathf.Lerp(-116.99999f, 116.99999f, Mathf.InverseLerp(-1.3f, 1.3f, deerFaceDir));
			dpPos.x *= flip;
			return Custom.RotateAroundOrigo(new Vector2(Custom.RotateAroundOrigo(new Vector2(dpPos.x, dpPos.z), degAng).x, dpPos.y), rotation) + antlerPos;
		}

		public void InitiateSprites(int firstAntlerSprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < parts.Length; j++)
				{
					sLeaser.sprites[firstAntlerSprite + i * parts.Length + j] = TriangleMesh.MakeLongMesh(parts[j].positions.Length, pointyTip: false, customColor: false);
				}
			}
		}

		public void DrawSprites(int firstAntlerSprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 headPos, Vector2 antlerPos, float deerFaceDir, Color blackCol, Color foggedCol)
		{
			float rotation = Custom.AimFromOneVectorToAnother(headPos, antlerPos);
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < parts.Length; j++)
				{
					int num = ((deerFaceDir < 0f) ? i : (1 - i));
					Part part = parts[j];
					float flip = ((i == 0) ? (-1f) : 1f);
					int num2 = firstAntlerSprite + num * parts.Length + j;
					Vector2 vector = TransformToHeadRotat(part.GetTransoformedPos(0, flip), antlerPos, rotation, flip, deerFaceDir);
					float num3 = 0.2f;
					if (num == 0)
					{
						sLeaser.sprites[num2].color = foggedCol;
					}
					else
					{
						sLeaser.sprites[num2].color = blackCol;
					}
					for (int k = 0; k < part.positions.Length; k++)
					{
						Vector2 vector2 = TransformToHeadRotat(part.GetTransoformedPos(k, flip), antlerPos, rotation, flip, deerFaceDir);
						Vector2 normalized = (vector2 - vector).normalized;
						Vector2 vector3 = Custom.PerpendicularVector(normalized);
						float num4 = Vector2.Distance(vector2, vector) / 5f;
						(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(k * 4, vector - vector3 * (num3 + part.rads[k]) * 0.5f * thickness + normalized * num4 - camPos);
						(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(k * 4 + 1, vector + vector3 * (num3 + part.rads[k]) * 0.5f * thickness + normalized * num4 - camPos);
						(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(k * 4 + 2, vector2 - vector3 * part.rads[k] * thickness - normalized * num4 - camPos);
						(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(k * 4 + 3, vector2 + vector3 * part.rads[k] * thickness - normalized * num4 - camPos);
						num3 = part.rads[k];
						vector = vector2;
					}
				}
			}
		}
	}

	private Dangler.DanglerProps danglerVals;

	private float resting;

	private float lastResting;

	private static Vector2[,,] legGraphicAnchors = new Vector2[2, 2, 2]
	{
		{
			{
				new Vector2(19f, 44f),
				new Vector2(13f, 62f)
			},
			{
				new Vector2(32f, 69f),
				new Vector2(13f, 56f)
			}
		},
		{
			{
				new Vector2(8f, 5f),
				new Vector2(6f, 4f)
			},
			{
				new Vector2(3.5f, 3f),
				new Vector2(6f, 3f)
			}
		}
	};

	private float lastFlip;

	private float flip;

	private float antlerRandomMovement;

	private float lastAntlerRandomMovement;

	private float antlerRandomMovementVel;

	private float[] chunksRotat;

	public Antlers antlers;

	private Color fogCol;

	public Dangler[] danglers;

	public int[,] bodyDanglerPositions;

	public float[,] bodyDanglerOrientations;

	public int bodyDanglers;

	public int hornDanglers;

	public int[,] hornDanglerPositions;

	public CreatureLooker looker;

	public Vector2[] lookPoint;

	public int blink;

	public int lastBlink;

	public Color bodyColor;

	private Deer deer => base.owner as Deer;

	private int AntlerSprite => 0;

	private int FirstAntlerSprite => 1;

	private int LastAntlerSprite => FirstAntlerSprite + antlers.SpritesClaimed - 1;

	private int FirstDanglerSprite => LastAntlerSprite + 1 + 5;

	private int LastDanglerSprite => FirstDanglerSprite + danglers.Length - 1;

	private int TotalSprites => LastDanglerSprite + 1 + 12 + 4;

	private int BodySprite(int chunk)
	{
		return LastAntlerSprite + 1 + chunk;
	}

	private int LegSprite(int leg, int part)
	{
		return LastDanglerSprite + 1 + leg * 3 + part;
	}

	private int EyeSprite(int eye, int part)
	{
		return LastDanglerSprite + 1 + 12 + eye * 2 + part;
	}

	private float LegPartLength(int pos, int part, bool includeExtension)
	{
		if (pos == 0)
		{
			return ((part == 0) ? 35f : 55f) * (includeExtension ? (deer.preferredHeight / 15f) : 1f);
		}
		return ((part == 0) ? 60f : 45f) * (includeExtension ? (deer.preferredHeight / 15f) : 1f);
	}

	public DeerGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		looker = new CreatureLooker(this, deer.AI.tracker, deer, 0.1f, 150);
		lookPoint = new Vector2[2];
		cullRange = 1400f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(deer.abstractCreature.ID.RandomSeed);
		antlers = new Antlers(deer.antlers.rad, 1f);
		chunksRotat = new float[4];
		for (int i = 0; i < 4; i++)
		{
			chunksRotat[i] = UnityEngine.Random.value * 360f;
		}
		bodyDanglers = UnityEngine.Random.Range(10, 20);
		hornDanglers = UnityEngine.Random.Range(5, 10);
		danglers = new Dangler[bodyDanglers + hornDanglers];
		danglerVals = new Dangler.DanglerProps();
		bodyDanglerPositions = new int[bodyDanglers, 2];
		bodyDanglerOrientations = new float[bodyDanglers, 5];
		hornDanglerPositions = new int[hornDanglers, 3];
		internalContainerObjects = new List<ObjectHeldInInternalContainer>();
		for (int j = 0; j < bodyDanglers + hornDanglers; j++)
		{
			danglers[j] = new Dangler(this, j, UnityEngine.Random.Range(4, UnityEngine.Random.Range(6, UnityEngine.Random.Range(8, 16))), 5f, 0f);
			float num = Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value) * ((j < bodyDanglers) ? 1f : 0.5f);
			float num2 = ((j < bodyDanglers) ? 1f : Mathf.Lerp(0.2f, 0.7f, UnityEngine.Random.value));
			for (int k = 0; k < danglers[j].segments.Length; k++)
			{
				float num3 = (float)k / (float)(danglers[j].segments.Length - 1);
				danglers[j].segments[k].rad = Mathf.Lerp(Mathf.Lerp((j < bodyDanglers) ? 11f : 6f, 2.5f, Mathf.Pow(num3, 0.7f)), 2f + Mathf.Sin(Mathf.Pow(num3, 2.5f) * (float)Math.PI) * 6f, num3) * num;
				danglers[j].segments[k].conRad = Mathf.Lerp(30f, 5f, num3) * num2 * 0.5f;
			}
			if (j < bodyDanglers)
			{
				bodyDanglerPositions[j, 0] = UnityEngine.Random.Range(0, 5);
				bodyDanglerPositions[j, 1] = UnityEngine.Random.Range(1, 5);
				bodyDanglerOrientations[j, 0] = Mathf.Lerp(0f, 360f, UnityEngine.Random.value);
				bodyDanglerOrientations[j, 1] = deer.bodyChunks[bodyDanglerPositions[j, 0]].rad * UnityEngine.Random.value * 0.7f;
				bodyDanglerOrientations[j, 2] = Mathf.Lerp(0f, 360f, UnityEngine.Random.value);
				bodyDanglerOrientations[j, 3] = deer.bodyChunks[bodyDanglerPositions[j, 1]].rad * UnityEngine.Random.value * 0.7f;
				bodyDanglerOrientations[j, 4] = UnityEngine.Random.value;
			}
			else
			{
				hornDanglerPositions[j - bodyDanglers, 0] = UnityEngine.Random.Range(0, antlers.parts.Length);
				hornDanglerPositions[j - bodyDanglers, 1] = antlers.parts[hornDanglerPositions[j - bodyDanglers, 0]].positions.Length - 1 - UnityEngine.Random.Range(1, UnityEngine.Random.Range(1, antlers.parts[hornDanglerPositions[j - bodyDanglers, 0]].positions.Length));
				hornDanglerPositions[j - bodyDanglers, 2] = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
			}
		}
		UnityEngine.Random.state = state;
		Reset();
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < danglers.Length; i++)
		{
			danglers[i].Reset();
		}
	}

	public override void Update()
	{
		base.Update();
		if (culled)
		{
			return;
		}
		looker.Update();
		lookPoint[1] = lookPoint[0];
		lookPoint[0] *= 0f;
		if (deer.AI.goToPuffBall != null && deer.AI.goToPuffBall.VisualContact && deer.AI.goToPuffBall.representedItem.realizedObject != null)
		{
			lookPoint[0] = Vector2.ClampMagnitude(deer.AI.goToPuffBall.representedItem.realizedObject.firstChunk.pos - deer.mainBodyChunk.pos, 200f) / 200f;
		}
		else if (looker.lookCreature != null)
		{
			if (looker.lookCreature.VisualContact && looker.lookCreature.representedCreature.realizedCreature != null)
			{
				lookPoint[0] = looker.lookCreature.representedCreature.realizedCreature.DangerPos;
			}
			else
			{
				lookPoint[0] = deer.room.MiddleOfTile(looker.lookCreature.BestGuessForPosition());
			}
			lookPoint[0] = Vector2.ClampMagnitude(lookPoint[0] - deer.mainBodyChunk.pos, 200f) / 200f;
		}
		else if (deer.AI.sporePos.HasValue && deer.AI.sporePos.Value.room == deer.room.abstractRoom.index)
		{
			lookPoint[0] = Vector2.ClampMagnitude(deer.room.MiddleOfTile(deer.AI.sporePos.Value) - deer.mainBodyChunk.pos, 200f) / 200f;
		}
		blink--;
		if (blink < -UnityEngine.Random.Range(5, 14))
		{
			blink = UnityEngine.Random.Range(15, 305);
		}
		if (!deer.Consious || deer.Kneeling || deer.AI.closeEyesCounter > 0)
		{
			blink = -5;
		}
		for (int i = 0; i < danglers.Length; i++)
		{
			danglers[i].Update();
			for (int j = 0; j < danglers[i].segments.Length; j++)
			{
				DeerTentacle deerTentacle = deer.legs[UnityEngine.Random.Range(0, 4)];
				if (Custom.DistLess(danglers[i].segments[j].pos, deerTentacle.tChunks[1].pos, 10f))
				{
					danglers[i].segments[j].vel += deerTentacle.tChunks[1].vel;
				}
			}
		}
		for (int num = internalContainerObjects.Count - 1; num >= 0; num--)
		{
			if (!(internalContainerObjects[num].obj is PlayerGraphics) || ((internalContainerObjects[num].obj as PlayerGraphics).owner as Player).playerInAntlers == null || ((internalContainerObjects[num].obj as PlayerGraphics).owner as Player).playerInAntlers.deer != deer)
			{
				ReleaseSpecificInternallyContainedObjectSprites(num);
			}
		}
		float num2 = Mathf.Clamp(-1f + 2f * Mathf.InverseLerp(-20f, 20f, deer.mainBodyChunk.pos.x - deer.bodyChunks[1].pos.x) + deer.flipDir, -1f, 1f);
		lastFlip = flip;
		if (num2 < 0f)
		{
			flip = Mathf.Max(-1f, flip - 1f / Custom.LerpMap(Mathf.Abs(flip - num2), 0f, 1f, 120f, 20f));
		}
		else
		{
			flip = Mathf.Min(1f, flip + 1f / Custom.LerpMap(Mathf.Abs(flip - num2), 0f, 1f, 120f, 20f));
		}
		antlerRandomMovementVel *= 0.8f;
		if (deer.Consious)
		{
			antlerRandomMovementVel += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * deer.mainBodyChunk.vel.magnitude * 0.01f;
		}
		antlerRandomMovementVel -= antlerRandomMovement * 0.0001f;
		lastAntlerRandomMovement = antlerRandomMovement;
		antlerRandomMovement += antlerRandomMovementVel;
		if (antlerRandomMovement < -1f)
		{
			antlerRandomMovement = -1f;
		}
		else if (antlerRandomMovement > 1f)
		{
			antlerRandomMovement = 1f;
		}
		lastResting = resting;
		resting = deer.resting;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[AntlerSprite] = new FSprite("Circle20");
		sLeaser.sprites[AntlerSprite].scale = 0f;
		sLeaser.sprites[AntlerSprite].color = new Color(0.5f, 0f, 0f);
		for (int i = 0; i < 5; i++)
		{
			sLeaser.sprites[BodySprite(i)] = new FSprite("Futile_White");
			sLeaser.sprites[BodySprite(i)].scaleX = base.owner.bodyChunks[i].rad / 8f * 1.05f;
			sLeaser.sprites[BodySprite(i)].scaleY = base.owner.bodyChunks[i].rad / 8f * 1.3f;
			sLeaser.sprites[BodySprite(i)].shader = rCam.room.game.rainWorld.Shaders["JaggedCircle"];
			sLeaser.sprites[BodySprite(i)].alpha = 0.5f;
		}
		for (int j = 0; j < 4; j++)
		{
			sLeaser.sprites[LegSprite(j, 0)] = new FSprite("deerLeg" + ((j < 2) ? "A" : "B") + "1");
			sLeaser.sprites[LegSprite(j, 0)].anchorY = legGraphicAnchors[1, (j >= 2) ? 1u : 0u, 0].y / legGraphicAnchors[0, (j >= 2) ? 1u : 0u, 0].y;
			sLeaser.sprites[LegSprite(j, 0)].anchorX = legGraphicAnchors[1, (j >= 2) ? 1u : 0u, 0].x / legGraphicAnchors[0, (j >= 2) ? 1u : 0u, 0].x;
			sLeaser.sprites[LegSprite(j, 1)] = new FSprite("deerLeg" + ((j < 2) ? "A" : "B") + "2");
			sLeaser.sprites[LegSprite(j, 1)].anchorY = legGraphicAnchors[1, (j >= 2) ? 1u : 0u, 1].y / legGraphicAnchors[0, (j >= 2) ? 1u : 0u, 1].y;
			sLeaser.sprites[LegSprite(j, 1)].anchorX = legGraphicAnchors[1, (j >= 2) ? 1u : 0u, 1].x / legGraphicAnchors[0, (j >= 2) ? 1u : 0u, 1].x;
			sLeaser.sprites[LegSprite(j, 2)] = TriangleMesh.MakeLongMesh(7, pointyTip: false, customColor: true);
		}
		antlers.InitiateSprites(FirstAntlerSprite, sLeaser, rCam);
		for (int k = 0; k < danglers.Length; k++)
		{
			danglers[k].InitSprite(sLeaser, FirstDanglerSprite + k);
		}
		for (int l = 0; l < 2; l++)
		{
			sLeaser.sprites[EyeSprite(l, 0)] = new FSprite("deerEyeA");
			sLeaser.sprites[EyeSprite(l, 1)] = new FSprite("deerEyeB");
		}
		sLeaser.containers = new FContainer[3]
		{
			new FContainer(),
			new FContainer(),
			new FContainer()
		};
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		bodyColor = Color.Lerp(palette.blackColor, palette.texture.GetPixel(5, 4), 0.2f);
		for (int i = 0; i < 5; i++)
		{
			sLeaser.sprites[BodySprite(i)].color = bodyColor;
		}
		ReColorLegs(sLeaser, rCam, palette);
		fogCol = palette.fogColor;
		for (int j = 0; j < 2; j++)
		{
			for (int k = 0; k < antlers.parts.Length; k++)
			{
				sLeaser.sprites[FirstAntlerSprite + j * antlers.parts.Length + k].color = bodyColor;
			}
		}
		for (int l = 0; l < 2; l++)
		{
			sLeaser.sprites[EyeSprite(l, 0)].color = new Color(1f, 0.7f, 0f);
			sLeaser.sprites[EyeSprite(l, 1)].color = bodyColor;
		}
		for (int m = 0; m < danglers.Length; m++)
		{
			sLeaser.sprites[FirstDanglerSprite + m].color = bodyColor;
		}
	}

	private void ReColorLegs(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color pixel = palette.texture.GetPixel(5, 4);
		for (int i = 0; i < 4; i++)
		{
			sLeaser.sprites[LegSprite(i, 0)].color = bodyColor;
			sLeaser.sprites[LegSprite(i, 1)].color = bodyColor;
			for (int j = 0; j < 28; j++)
			{
				(sLeaser.sprites[LegSprite(i, 2)] as TriangleMesh).verticeColors[j] = Color.Lerp(bodyColor, pixel, Mathf.Pow(Mathf.InverseLerp(0.2f, 0.9f, (float)j / 27f), 1.6f) * (1f - deer.resting));
			}
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		newContatiner = rCam.ReturnFContainer("Midground");
		for (int i = FirstAntlerSprite; i < FirstAntlerSprite + antlers.parts.Length; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
		newContatiner.AddChild(sLeaser.containers[0]);
		newContatiner.AddChild(sLeaser.containers[1]);
		newContatiner.AddChild(sLeaser.containers[2]);
		for (int j = 0; j < FirstAntlerSprite; j++)
		{
			newContatiner.AddChild(sLeaser.sprites[j]);
		}
		for (int k = FirstAntlerSprite + antlers.parts.Length; k < TotalSprites; k++)
		{
			newContatiner.AddChild(sLeaser.sprites[k]);
		}
	}

	public float CurrentFaceDir(float timeStacker)
	{
		return Mathf.Lerp(lastFlip, flip, timeStacker) * 0.85f + Mathf.Lerp(lastAntlerRandomMovement, antlerRandomMovement, timeStacker) * 0.25f * (1f - Mathf.Abs(Mathf.Lerp(lastFlip, flip, timeStacker)) * 0.7f);
	}

	public Color CurrentFoggedHornColor(float timeStacker)
	{
		return Color.Lerp(bodyColor, fogCol, Mathf.Clamp(Mathf.Abs(CurrentFaceDir(timeStacker)), 0f, 1f) * 0.35f);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		float num = CurrentFaceDir(timeStacker);
		antlers.DrawSprites(FirstAntlerSprite, sLeaser, rCam, timeStacker, camPos, Vector2.Lerp(base.owner.bodyChunks[0].lastPos, base.owner.bodyChunks[0].pos, timeStacker), Vector2.Lerp(base.owner.bodyChunks[5].lastPos, base.owner.bodyChunks[5].pos, timeStacker), num, bodyColor, CurrentFoggedHornColor(timeStacker));
		Vector2 p = Vector2.Lerp(base.owner.bodyChunks[0].lastPos, base.owner.bodyChunks[0].pos, timeStacker);
		p += Custom.DirVec(Vector2.Lerp(base.owner.bodyChunks[5].lastPos, base.owner.bodyChunks[5].pos, timeStacker), p) * 50f + Custom.PerpendicularVector(Custom.DirVec(Vector2.Lerp(base.owner.bodyChunks[5].lastPos, base.owner.bodyChunks[5].pos, timeStacker), p)) * 100f * num;
		for (int i = 0; i < 2; i++)
		{
			float num2 = ((i == 0) ? (-1f) : 1f);
			float num3 = Mathf.Pow(Mathf.InverseLerp(0.5f, -0.5f, num * num2), 2f);
			Vector2 vector = Vector2.Lerp(base.owner.bodyChunks[0].lastPos, base.owner.bodyChunks[0].pos, timeStacker);
			Vector2 vector2 = vector;
			Vector2 vector3 = Custom.DirVec(vector, Vector2.Lerp(base.owner.bodyChunks[5].lastPos, base.owner.bodyChunks[5].pos, timeStacker));
			Vector2 vector4 = Custom.PerpendicularVector(vector3);
			vector2 += vector3 * 10f * (1f - num3) + vector4 * num2 * Mathf.Lerp(Mathf.Cos(Mathf.Abs(num) * (float)Math.PI * 0.5f) * 15f, deer.bodyChunks[0].rad, num3) + Custom.DirVec(vector2, p) * 10f * (1f - num3);
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[EyeSprite(i, j)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector + Custom.DirVec(vector, vector2) * 25f, num3), p) - 90f * num2;
				sLeaser.sprites[EyeSprite(i, j)].scaleY = 1f - num3;
				sLeaser.sprites[EyeSprite(i, j)].x = vector2.x - camPos.x;
				sLeaser.sprites[EyeSprite(i, j)].y = vector2.y - camPos.y;
				if (blink <= 0)
				{
					vector2 = new Vector2(-1000f, -1000f);
					sLeaser.sprites[EyeSprite(i, 0)].element = Futile.atlasManager.GetElementWithName("deerEyeA2");
				}
				else
				{
					sLeaser.sprites[EyeSprite(i, 0)].element = Futile.atlasManager.GetElementWithName("deerEyeA");
					vector2 += Vector2.Lerp(lookPoint[1], lookPoint[0], timeStacker) * 2f;
				}
			}
		}
		for (int k = 0; k < danglers.Length; k++)
		{
			danglers[k].DrawSprite(FirstDanglerSprite + k, sLeaser, rCam, timeStacker, camPos);
		}
		for (int l = 0; l < hornDanglers; l++)
		{
			if (hornDanglerPositions[l, 2] < 0 == num < 0f)
			{
				sLeaser.sprites[FirstDanglerSprite + bodyDanglers + l].color = CurrentFoggedHornColor(timeStacker);
				sLeaser.sprites[FirstDanglerSprite + bodyDanglers + l].RemoveFromContainer();
				sLeaser.containers[0].AddChild(sLeaser.sprites[FirstDanglerSprite + bodyDanglers + l]);
			}
			else
			{
				sLeaser.sprites[FirstDanglerSprite + bodyDanglers + l].color = bodyColor;
				sLeaser.sprites[FirstDanglerSprite + bodyDanglers + l].RemoveFromContainer();
				sLeaser.containers[2].AddChild(sLeaser.sprites[FirstDanglerSprite + bodyDanglers + l]);
			}
		}
		if (lastResting != resting)
		{
			ReColorLegs(sLeaser, rCam, rCam.currentPalette);
		}
		for (int m = 0; m < 5; m++)
		{
			Vector2 vector5 = Vector2.Lerp(base.owner.bodyChunks[m].lastPos, base.owner.bodyChunks[m].pos, timeStacker);
			sLeaser.sprites[BodySprite(m)].x = vector5.x - camPos.x;
			sLeaser.sprites[BodySprite(m)].y = vector5.y - camPos.y;
			if (m == 0)
			{
				sLeaser.sprites[BodySprite(m)].rotation = Custom.AimFromOneVectorToAnother(vector5, p);
			}
			else
			{
				sLeaser.sprites[BodySprite(m)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(base.owner.bodyChunks[m].rotationChunk.lastPos, base.owner.bodyChunks[m].rotationChunk.pos, timeStacker), vector5) + chunksRotat[m - 1];
			}
		}
		Vector2 vector6 = Custom.DirVec(Vector2.Lerp(deer.bodyChunks[3].lastPos, deer.bodyChunks[3].pos, timeStacker), Vector2.Lerp(deer.bodyChunks[1].lastPos, deer.bodyChunks[1].pos, timeStacker));
		for (int n = 0; n < 4; n++)
		{
			Vector2 vector7 = Vector2.Lerp(deer.legs[n].connectedChunk.lastPos, deer.legs[n].connectedChunk.pos, timeStacker);
			Vector2 vector8 = Vector2.Lerp(deer.legs[n].tChunks[1].lastPos, deer.legs[n].tChunks[1].pos, timeStacker);
			if (n < 2)
			{
				vector8 = Vector2.Lerp(vector8, vector7, 0.2f) + vector6 * 20f;
				vector7 = Vector2.Lerp(vector7, Vector2.Lerp(deer.bodyChunks[0].lastPos, deer.bodyChunks[0].pos, timeStacker), 0.4f) + new Vector2(0f, -20f) + vector6 * 5f;
			}
			else
			{
				vector7 = Vector2.Lerp(vector7, Vector2.Lerp(deer.bodyChunks[4].lastPos, deer.bodyChunks[4].pos, timeStacker), 0.5f) + new Vector2(0f, -5f);
				vector7 -= vector6 * 17f;
				vector8 -= vector6 * 10f;
			}
			Vector2 a = Custom.InverseKinematic(vector7, vector8, LegPartLength((n >= 2) ? 1 : 0, 0, includeExtension: true), LegPartLength((n >= 2) ? 1 : 0, 1, includeExtension: true), ((n < 2) ? 1f : (-1f)) * Mathf.Sign(num));
			a = Vector2.Lerp(a, vector7 + Custom.DirVec(vector7, Vector2.Lerp(deer.legs[n].tChunks[0].lastPos, deer.legs[n].tChunks[0].pos, timeStacker)) * LegPartLength((n >= 2) ? 1 : 0, 0, includeExtension: true), 1f - Mathf.Abs(num));
			Vector2 a2 = Vector2.Lerp(deer.legs[n].tChunks[4].lastPos, deer.legs[n].tChunks[4].pos, timeStacker);
			Vector2 vector9 = Vector2.Lerp(deer.legs[n].tChunks[5].lastPos, deer.legs[n].tChunks[5].pos, timeStacker);
			a2 = Vector2.Lerp(a2, vector9 + Custom.DirVec(vector9, Vector2.Lerp(deer.legs[n].tChunks[4].lastPos, deer.legs[n].tChunks[4].pos, timeStacker)) * 40f, 0.5f);
			float num4 = Vector2.Distance(vector8, a2);
			Vector2 a3 = Custom.InverseKinematic(vector8, a2, Mathf.Lerp(num4 / 2f, 80f, 0.5f), Mathf.Lerp(num4 / 2f, 80f, 0.5f), 0f - Mathf.Sign(num));
			a3 = Vector2.Lerp(a3, Vector2.Lerp(Vector2.Lerp(deer.legs[n].tChunks[2].lastPos, deer.legs[n].tChunks[2].pos, timeStacker), Vector2.Lerp(deer.legs[n].tChunks[3].lastPos, deer.legs[n].tChunks[3].pos, timeStacker), 0.5f), Mathf.Lerp(1f, 0.5f, Mathf.Abs(num)));
			float num5 = ((num < ((n % 2 == 0) ? 0.5f : (-0.5f))) ? (-1f) : 1f);
			if (n >= 2 && Mathf.Abs(num) < 0.7f && Mathf.Sign(num) != num5)
			{
				vector7 += Custom.PerpendicularVector((vector7 - a).normalized) * 50f * num5 * Mathf.InverseLerp(0f, 0.7f, Mathf.Abs(num));
				vector7 = a + Custom.DirVec(a, vector7) * LegPartLength(1, 0, includeExtension: true) * (1f - resting);
			}
			sLeaser.sprites[LegSprite(n, 0)].x = a.x - camPos.x;
			sLeaser.sprites[LegSprite(n, 0)].y = a.y - camPos.y;
			sLeaser.sprites[LegSprite(n, 0)].rotation = Custom.AimFromOneVectorToAnother(a, vector7);
			sLeaser.sprites[LegSprite(n, 0)].scaleY = Vector2.Distance(vector7, a) / LegPartLength((n >= 2) ? 1 : 0, 0, includeExtension: false);
			sLeaser.sprites[LegSprite(n, 0)].scaleX = num5 * Mathf.Min(1f, Vector2.Distance(vector7, a) / LegPartLength((n >= 2) ? 1 : 0, 0, includeExtension: false));
			sLeaser.sprites[LegSprite(n, 1)].x = vector8.x - camPos.x;
			sLeaser.sprites[LegSprite(n, 1)].y = vector8.y - camPos.y;
			sLeaser.sprites[LegSprite(n, 1)].rotation = Custom.AimFromOneVectorToAnother(vector8, a);
			sLeaser.sprites[LegSprite(n, 1)].scaleY = Vector2.Distance(a, vector8) / LegPartLength((n >= 2) ? 1 : 0, 1, includeExtension: false);
			sLeaser.sprites[LegSprite(n, 1)].scaleX = num5 * Mathf.Min(1f, Vector2.Distance(a, vector8) / LegPartLength((n >= 2) ? 1 : 0, 1, includeExtension: false));
			Vector2 vector10 = a;
			for (int num6 = 0; num6 < 7; num6++)
			{
				Vector2 vector11 = a;
				float num7 = Mathf.Lerp(5f, 1f, (float)num6 / 6f);
				float num8 = Mathf.Lerp(5f, 1f, ((float)num6 + 0.5f) / 6f);
				switch (num6)
				{
				case 0:
					vector11 = vector8 - Custom.DirVec(a, a3) * 2.5f;
					num7 = 0f;
					num8 = 4f;
					break;
				case 1:
					vector11 = vector8 + Custom.DirVec(a, a3) * 7.5f;
					num7 = 4f;
					num8 = 6f;
					break;
				case 2:
					vector11 = a3 - Custom.DirVec(vector8, a2) * 7.5f;
					break;
				case 3:
					vector11 = a3 + Custom.DirVec(vector8, a2) * 7.5f;
					num8 = 4.5f;
					break;
				case 4:
					vector11 = a2 - Custom.DirVec(a3, vector9) * 7.5f;
					break;
				case 5:
					vector11 = a2 + Custom.DirVec(a3, vector9) * 7.5f;
					num7 = 3f;
					num8 = 2f;
					break;
				case 6:
					vector11 = vector9;
					break;
				}
				Vector2 normalized = (vector11 - vector10).normalized;
				Vector2 vector12 = Custom.PerpendicularVector(normalized);
				float num9 = Vector2.Distance(vector11, vector10) / 5f;
				(sLeaser.sprites[LegSprite(n, 2)] as TriangleMesh).MoveVertice(num6 * 4 + 2, vector11 - vector12 * num8 - normalized * num9 - camPos);
				(sLeaser.sprites[LegSprite(n, 2)] as TriangleMesh).MoveVertice(num6 * 4 + 3, vector11 + vector12 * num8 - normalized * num9 - camPos);
				if (num6 == 6)
				{
					num9 = Vector2.Distance(vector11, vector10);
				}
				(sLeaser.sprites[LegSprite(n, 2)] as TriangleMesh).MoveVertice(num6 * 4, vector10 - vector12 * num7 + normalized * num9 - camPos);
				(sLeaser.sprites[LegSprite(n, 2)] as TriangleMesh).MoveVertice(num6 * 4 + 1, vector10 + vector12 * num7 + normalized * num9 - camPos);
				vector10 = vector11;
			}
		}
	}

	public Vector2 DanglerConnection(int index, float timeStacker)
	{
		if (index < bodyDanglers)
		{
			Vector2 vector = Vector2.Lerp(deer.bodyChunks[bodyDanglerPositions[index, 0]].lastPos, deer.bodyChunks[bodyDanglerPositions[index, 0]].pos, timeStacker);
			Vector2 vector2 = Vector2.Lerp(deer.bodyChunks[bodyDanglerPositions[index, 1]].lastPos, deer.bodyChunks[bodyDanglerPositions[index, 1]].pos, timeStacker);
			Vector2 a = vector + Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(deer.bodyChunks[bodyDanglerPositions[index, 0]].rotationChunk.lastPos, deer.bodyChunks[bodyDanglerPositions[index, 0]].rotationChunk.pos, timeStacker)) + bodyDanglerOrientations[index, 0]) * bodyDanglerOrientations[index, 1];
			vector2 += Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector2, Vector2.Lerp(deer.bodyChunks[bodyDanglerPositions[index, 1]].rotationChunk.lastPos, deer.bodyChunks[bodyDanglerPositions[index, 1]].rotationChunk.pos, timeStacker)) + bodyDanglerOrientations[index, 2]) * bodyDanglerOrientations[index, 3];
			return Vector2.Lerp(a, vector2, bodyDanglerOrientations[index, 4]);
		}
		index -= bodyDanglers;
		Vector2 vector3 = Vector2.Lerp(deer.antlers.lastPos, deer.antlers.pos, timeStacker);
		return antlers.TransformToHeadRotat(antlers.parts[hornDanglerPositions[index, 0]].GetTransoformedPos(hornDanglerPositions[index, 1], hornDanglerPositions[index, 2]), vector3, Custom.AimFromOneVectorToAnother(Vector2.Lerp(deer.mainBodyChunk.lastPos, deer.mainBodyChunk.pos, timeStacker), vector3), hornDanglerPositions[index, 2], CurrentFaceDir(timeStacker));
	}

	public Dangler.DanglerProps Props(int index)
	{
		return danglerVals;
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score;
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return null;
	}

	public void LookAtNothing()
	{
	}
}
