using System;
using System.Collections.Generic;
using Noise;
using RWCustom;
using UnityEngine;

public class MirosBird : Creature
{
	public struct IndividualVariations
	{
		public float fatness;

		public IndividualVariations(float fatness)
		{
			this.fatness = fatness;
		}
	}

	public class BirdLeg
	{
		public class Joint
		{
			public Vector2 pos;

			public Vector2 lastPos;
		}

		public Color debugColor;

		public MirosBird bird;

		public int legNumber;

		public bool groundContact;

		public float springPower;

		public float lastSpringPow;

		public float forwardPower;

		public Vector2 springDir;

		public Vector2? footSecurePos;

		public Vector2? moveToPos;

		public Vector2? lastFootSecurePos;

		private Vector2 moveFromPos;

		private float moveProgress;

		private int footSecureFrames;

		public int lightUp;

		public Vector2 lightUpPos1;

		public Vector2 lightUpPos2;

		private bool modeConservativeBias;

		private float lowerLegLength = 55f;

		private float tighLenght = 35f;

		public Joint[] joints;

		public float flip;

		public float lastRunMode;

		public float runMode;

		public Room room => bird.room;

		public float ConnectionSide => Mathf.Lerp(0.5f + flip * 0.5f, 0.5f + 0.5f * Mathf.Sin(bird.RunCycle((float)legNumber * 0.5f, 1f) * (float)Math.PI * 2f), runMode);

		public Vector2 ConnectionPos => Vector2.Lerp(bird.bodyChunks[2].pos, bird.bodyChunks[3].pos, ConnectionSide);

		public Joint Hip => joints[0];

		public Joint Knee => joints[1];

		public Joint Foot => joints[2];

		public BirdLeg(MirosBird bird, int legNumber)
		{
			this.bird = bird;
			this.legNumber = legNumber;
			joints = new Joint[3];
			for (int i = 0; i < joints.Length; i++)
			{
				joints[i] = new Joint();
			}
		}

		public void Reset()
		{
			Vector2 connectionPos = ConnectionPos;
			Foot.pos = connectionPos;
			Hip.pos = connectionPos;
			Knee.pos = connectionPos;
			Foot.lastPos = connectionPos;
			Hip.lastPos = connectionPos;
			Knee.lastPos = connectionPos;
			footSecurePos = null;
			lastFootSecurePos = null;
			moveToPos = null;
		}

		public void Update()
		{
			if (footSecurePos.HasValue)
			{
				lastFootSecurePos = footSecurePos.Value;
			}
			else
			{
				lastFootSecurePos = null;
			}
			lastSpringPow = springPower;
			springPower = 0f;
			forwardPower = 0f;
			groundContact = false;
			lastRunMode = runMode;
			if (Mathf.Abs(bird.bodyFlip) > ((bird.moveDir.y < -0.1f) ? 0.1f : 0.3f) && Mathf.Abs(bird.mainBodyChunk.vel.x) > 3f && (Mathf.Abs(bird.moveDir.x) > 0.5f || bird.moveDir.y < -0.1f) && Custom.ManhattanDistance(bird.AI.pathFinder.GetDestination, bird.abstractCreature.pos) > 3)
			{
				runMode = Mathf.Min(1f, runMode + 0.025f);
			}
			else
			{
				runMode = Mathf.Max(0f, runMode - 0.1f);
			}
			if (runMode > (modeConservativeBias ? 0.4f : 0.6f))
			{
				RunMode();
				modeConservativeBias = true;
			}
			else
			{
				StandMode();
				modeConservativeBias = false;
			}
			if (footSecurePos.HasValue && lastFootSecurePos.HasValue && footSecurePos.Value == lastFootSecurePos.Value)
			{
				footSecureFrames++;
			}
			else
			{
				footSecureFrames = 0;
			}
			if (!footSecurePos.HasValue && lastFootSecurePos.HasValue && Foot.pos.x - lastFootSecurePos.Value.x < 0f != bird.moveDir.x < 0f && !Custom.DistLess(Foot.pos, lastFootSecurePos.Value, 18f) && !Custom.DistLess(Foot.pos, Foot.lastPos, 18f))
			{
				SmallSparks(lastFootSecurePos.Value, Foot.pos);
				room.PlaySound(SoundID.Miros_Piston_Scrape, Foot.pos);
				room.InGameNoise(new InGameNoise(Foot.pos, 1200f, bird, 1f));
			}
			else if (footSecurePos.HasValue && !lastFootSecurePos.HasValue)
			{
				room.PlaySound(SoundID.Miros_Piston_Ground_Impact, Foot.pos);
				room.InGameNoise(new InGameNoise(footSecurePos.Value, 800f, bird, 1f));
			}
			if (footSecurePos.HasValue && !lastFootSecurePos.HasValue && !Custom.DistLess(Foot.pos, Foot.lastPos, 60f))
			{
				room.PlaySound(SoundID.Miros_Piston_Sharp_Impact, Foot.pos);
				SmallSparks(Foot.pos, Foot.pos);
			}
			if (bird.safariControlled && bird.inputWithDiagonals.HasValue && bird.inputWithDiagonals.Value.jmp && bird.inputWithDiagonals.Value.x != 0 && footSecurePos.HasValue)
			{
				SmallSparks(Foot.pos, Foot.pos);
			}
			if (lightUp > 0)
			{
				lightUp--;
			}
			if (!Custom.DistLess(Knee.pos, Hip.pos, tighLenght))
			{
				Knee.pos = Hip.pos + Custom.DirVec(Hip.pos, Knee.pos) * tighLenght;
			}
			if (!Custom.DistLess(Knee.pos, Foot.pos, lowerLegLength + (groundContact ? 20f : 0f)))
			{
				Foot.pos = Knee.pos + Custom.DirVec(Knee.pos, Foot.pos) * (lowerLegLength + (groundContact ? 20f : 0f));
			}
		}

		private void RunMode()
		{
			flip = Mathf.Lerp(flip, bird.bodyFlip, 0.2f);
			Vector2 vector = Vector3.Slerp(Custom.DirVec(bird.bodyChunks[1].pos, bird.bodyChunks[0].pos), new Vector2(0f, -1f).normalized, 0.75f);
			Vector2 vec = Custom.DegToVec(360f * bird.RunCycle((float)legNumber * 0.5f, 1f));
			vec = Custom.FlattenVectorAlongAxis(vec, Custom.VecToDeg(vector) + flip * 30f, 0.3f);
			Vector2 vector2 = ConnectionPos + vector * 20f + vec * 35f + Custom.PerpendicularVector(vector) * flip * 10f;
			if (!Custom.DistLess(ConnectionPos, vector2, tighLenght))
			{
				vector2 = ConnectionPos + Custom.DirVec(ConnectionPos, vector2) * tighLenght;
			}
			Vector2? vector3 = SharedPhysics.ExactTerrainRayTracePos(room, ConnectionPos, vector2);
			if (vector3.HasValue)
			{
				vector2 = vector3.Value + Custom.DirVec(vector3.Value, ConnectionPos);
			}
			vec = Custom.DegToVec(360f * bird.RunCycle((float)legNumber * 0.5f + Mathf.Sin(bird.RunCycle((float)legNumber * 0.5f, 1f) * (float)Math.PI * 2f) * 0.01f * flip, 1f));
			vec = Custom.FlattenVectorAlongAxis(vec, Custom.VecToDeg(vector), 0.8f);
			Vector2 vector4 = vector2 + Custom.DirVec(vector2, bird.mainBodyChunk.pos + vector * 50f + vec * 55f) * lowerLegLength;
			Vector2 vector5 = vector4;
			vector3 = SharedPhysics.ExactTerrainRayTracePos(room, vector2, vector4);
			if (vector3.HasValue)
			{
				vector5 = vector3.Value;
				springPower = 1f - Vector2.Distance(vector2, vector5) / lowerLegLength;
				springDir = Custom.DirVec(vector4, vector2);
				forwardPower = Mathf.InverseLerp(-0.5f, 0.5f, Vector2.Dot(bird.moveDir.normalized, springDir));
				if (lastSpringPow > 0f)
				{
					float num = Mathf.Clamp(springPower - lastSpringPow, 0f, 0.2f);
					bird.bodyChunks[0].pos += springDir * num * lowerLegLength;
					bird.bodyChunks[0].vel += springDir * num * lowerLegLength;
					if (footSecurePos.HasValue)
					{
						if (footSecureFrames > 4 && !Custom.DistLess(vector5, footSecurePos.Value, 28f) && vector5.y == footSecurePos.Value.y && vector5.x - footSecurePos.Value.x < 0f != bird.moveDir.x < 0f)
						{
							Slip(footSecurePos.Value, vector5);
							footSecurePos = vector5;
						}
						else
						{
							vector5 = footSecurePos.Value;
						}
					}
				}
				else
				{
					footSecurePos = vector5;
				}
				float a = Vector2.Distance(ConnectionPos, vector2);
				vector2 = Vector2.Lerp(Custom.InverseKinematic(ConnectionPos, vector5, a, lowerLegLength, 0f - flip), vector2, 0.5f);
				groundContact = true;
			}
			else
			{
				footSecurePos = null;
			}
			Hip.lastPos = Hip.pos;
			Hip.pos = ConnectionPos;
			Knee.lastPos = Knee.pos;
			Knee.pos += bird.mainBodyChunk.vel;
			Knee.pos += Vector2.ClampMagnitude(vector2 - Knee.pos, Custom.LerpMap(runMode, 0.5f, 1f, 5f, 40f));
			Foot.lastPos = Foot.pos;
			Foot.pos += bird.mainBodyChunk.vel;
			Foot.pos += Vector2.ClampMagnitude(vector5 - Foot.pos, Custom.LerpMap(runMode, 0.5f, 1f, 5f, 40f));
			moveFromPos = Foot.pos;
			moveProgress = 0f;
			moveToPos = null;
		}

		private void StandMode()
		{
			flip = Mathf.Lerp(flip, -1f + 2f * (float)legNumber, 0.2f);
			Hip.lastPos = Hip.pos;
			Hip.pos = ConnectionPos;
			Vector2 vector = Vector3.Slerp(Custom.DirVec(bird.bodyChunks[1].pos, bird.bodyChunks[0].pos), new Vector2(0f, -1f).normalized, 0.75f);
			Vector2 vector2 = Hip.pos + ((vector * 1.8f + Custom.DirVec(bird.mainBodyChunk.pos, Hip.pos)).normalized * 0.7f + new Vector2(bird.moveDir.x, bird.moveDir.y * 0.4f)).normalized * (tighLenght + lowerLegLength);
			Foot.lastPos = Foot.pos;
			if (moveToPos.HasValue)
			{
				moveProgress = Mathf.Min(1f, moveProgress + 0.1f);
				vector2 = Vector2.Lerp(moveFromPos, moveToPos.Value, moveProgress);
				Foot.pos = vector2;
				if (moveProgress >= 1f)
				{
					footSecurePos = moveToPos;
					moveToPos = null;
					moveProgress = 0f;
				}
			}
			else if (footSecurePos.HasValue)
			{
				forwardPower = Custom.LerpMap(Vector2.Dot(Custom.DirVec(Foot.pos, bird.mainBodyChunk.pos), bird.moveDir), -1f, 1f, 0f, 0.2f);
				groundContact = true;
				Foot.pos = footSecurePos.Value;
				if (!Custom.DistLess(Hip.pos, footSecurePos.Value, tighLenght + lowerLegLength + (bird.legs[1 - legNumber].groundContact ? 0f : 20f)))
				{
					footSecurePos = null;
				}
			}
			else
			{
				Foot.pos += Vector2.ClampMagnitude(vector2 - Foot.pos, 10f);
				moveFromPos = Foot.pos;
				moveProgress = 0f;
				Vector2? vector3 = SharedPhysics.ExactTerrainRayTracePos(room, Hip.pos, vector2);
				if (vector3.HasValue)
				{
					vector2 = vector3.Value;
					moveToPos = vector2;
					groundContact = true;
				}
			}
			Vector2 a = Custom.InverseKinematic(Hip.pos, Foot.pos, tighLenght, lowerLegLength, 0f - flip);
			Vector2? vector4 = SharedPhysics.ExactTerrainRayTracePos(room, a, Foot.pos);
			if (vector4.HasValue)
			{
				Foot.pos = vector4.Value;
			}
			Knee.lastPos = Knee.pos;
			Knee.pos += Vector2.ClampMagnitude(Vector2.Lerp(a, Custom.InverseKinematic(Hip.pos, Foot.pos, tighLenght, lowerLegLength, 0f - flip), 0.5f) - Knee.pos, 40f);
			vector4 = SharedPhysics.ExactTerrainRayTracePos(room, Hip.pos, Knee.pos);
			if (vector4.HasValue)
			{
				forwardPower = Mathf.Max(forwardPower, 0.08f);
				Knee.pos = vector4.Value;
			}
			if (legNumber == 1 && footSecurePos.HasValue && bird.legs[0].footSecurePos.HasValue && Custom.ManhattanDistance(bird.AI.pathFinder.GetDestination, bird.abstractCreature.pos) < 4 && bird.legs[0].Foot.pos.x < bird.mainBodyChunk.pos.x == bird.legs[1].Foot.pos.x < bird.mainBodyChunk.pos.x)
			{
				BirdLeg obj = bird.legs[(bird.moveDir.x < 0f != bird.legs[0].Foot.pos.x < bird.legs[1].Foot.pos.x) ? 1 : 0];
				obj.moveFromPos = obj.footSecurePos.Value;
				obj.moveProgress = 0f;
				obj.footSecurePos = null;
			}
		}

		public void Slip(Vector2 lastPs, Vector2 ps)
		{
			bird.mainBodyChunk.pos += bird.moveDir.normalized * 12f;
			bird.mainBodyChunk.vel += bird.moveDir.normalized * 12f;
			bird.bodyChunks[1].pos += bird.moveDir.normalized * 12f;
			bird.bodyChunks[1].vel += bird.moveDir.normalized * 12f;
			room.PlaySound(SoundID.Miros_Piston_Big_Scrape, Foot.pos);
			for (int i = 0; i < 6 + (int)(Vector2.Distance(lastPs, ps) / 5f); i++)
			{
				bird.room.AddObject(new Spark(Vector2.Lerp(lastPs, ps, UnityEngine.Random.value) + Custom.DirVec(ps, Knee.pos), Custom.RNV() * 12f + (ps - lastPs) * 0.4f, new Color(1f, 1f, 0.8f), null, 11, 60));
			}
			lightUp = UnityEngine.Random.Range(3, 6) + (int)Custom.LerpMap(Vector2.Distance(lastPs, ps), 20f, 40f, 1f, 3f);
			lightUpPos1 = lastPs;
			lightUpPos2 = ps;
		}

		public void SmallSparks(Vector2 lastPs, Vector2 ps)
		{
			for (int i = 0; i < 1 + (int)(Vector2.Distance(lastPs, ps) / 12f); i++)
			{
				bird.room.AddObject(new Spark(Vector2.Lerp(lastPs, ps, UnityEngine.Random.value) + Custom.DirVec(ps, Knee.pos), Custom.RNV() * 6f + (ps - lastPs) * 0.4f, new Color(1f, 1f, 0.8f), null, 5, 18));
			}
			lightUp = UnityEngine.Random.Range(2, 4) + (int)Custom.LerpMap(Vector2.Distance(lastPs, ps), 10f, 40f, 0f, 3f);
			lightUpPos1 = lastPs;
			lightUpPos2 = ps;
		}
	}

	public MirosBirdAI AI;

	public float lungs;

	public IndividualVariations iVars;

	private float forwardPower;

	private float weightDownToStandOnBothLegs;

	public float lastBodyFlip;

	public float bodyFlip;

	public Vector2 moveDir;

	public float jawOpen;

	public float lastJawOpen;

	public float jawVel;

	private float keepJawOpenPos;

	private int jawSlamPause;

	private int jawKeepOpenPause;

	public List<IntVector2> pastPositions;

	public int stuckCounter;

	public Tentacle neck;

	public BirdLeg[] legs;

	private bool enterRoomHalf;

	private float runCycle;

	private float lastRunCycle;

	public bool controlledJawSnap;

	public Vector2 neutralDir;

	public Vector2 remMoveDir;

	public int sprintStuckCounter;

	public BodyChunk Head => base.bodyChunks[4];

	public override Vector2 VisionPoint => Head.pos;

	public bool RoomHalf(Room room)
	{
		return base.abstractCreature.pos.x > room.TileWidth / 2;
	}

	public float RunCycle(float cycleSpot, float timeStacker)
	{
		float num = Mathf.Lerp(lastRunCycle, runCycle, timeStacker) + cycleSpot;
		if (num < 0f)
		{
			num += Mathf.Floor(Mathf.Abs(num) + 1f);
		}
		return num - Mathf.Floor(num);
	}

	private void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		iVars = new IndividualVariations(UnityEngine.Random.value);
		UnityEngine.Random.state = state;
	}

	public MirosBird(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		List<Vector2> list = new List<Vector2>();
		List<float> list2 = new List<float>();
		list.Add(new Vector2(0f, 0f));
		list2.Add(18f);
		list.Add(new Vector2(0f, 25f));
		list2.Add(8f);
		list.Add(new Vector2(-17f, 0f));
		list2.Add(7f);
		list.Add(new Vector2(17f, 0f));
		list2.Add(7f);
		neutralDir = new Vector2(0f, -0.025f);
		base.bodyChunks = new BodyChunk[list.Count + 1];
		float num = 0f;
		for (int i = 0; i < list2.Count; i++)
		{
			num += list2[i];
		}
		float num2 = 8f;
		for (int j = 0; j < list.Count; j++)
		{
			base.bodyChunks[j] = new BodyChunk(this, j, new Vector2(0f, 0f), list2[j], list2[j] / num * num2);
		}
		base.bodyChunks[4] = new BodyChunk(this, 4, new Vector2(0f, 0f), 9f, 0.6f);
		base.bodyChunks[4].goThroughFloors = true;
		bodyChunkConnections = new BodyChunkConnection[list.Count * (list.Count - 1) / 2];
		int num3 = 0;
		for (int k = 0; k < list.Count; k++)
		{
			for (int l = k + 1; l < list.Count; l++)
			{
				bodyChunkConnections[num3] = new BodyChunkConnection(base.bodyChunks[k], base.bodyChunks[l], Vector2.Distance(list[k], list[l]), BodyChunkConnection.Type.Normal, 1f, -1f);
				num3++;
			}
		}
		legs = new BirdLeg[2];
		for (int m = 0; m < legs.Length; m++)
		{
			legs[m] = new BirdLeg(this, m);
		}
		neck = new Tentacle(this, base.bodyChunks[1], 60f);
		neck.tProps = new Tentacle.TentacleProps(stiff: false, rope: false, shorten: true, 0.5f, 0.1f, 0.5f, 1.8f, 0.2f, 1.2f, 10f, 0.25f, 10f, 15, 20, 20, 0);
		neck.tChunks = new Tentacle.TentacleChunk[3];
		for (int n = 0; n < neck.tChunks.Length; n++)
		{
			neck.tChunks[n] = new Tentacle.TentacleChunk(neck, n, (float)(n + 1) / (float)neck.tChunks.Length, 3f);
		}
		neck.tChunks[neck.tChunks.Length - 1].rad = 5f;
		neck.stretchAndSqueeze = 0f;
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new MirosBirdGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		CheckFlip();
		if (!enteringShortCut.HasValue)
		{
			UpdateNeck();
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (base.Consious)
		{
			Act();
		}
		if (room != null)
		{
			for (int i = 0; i < legs.Length; i++)
			{
				legs[i].Update();
			}
			if (base.grasps[0] != null)
			{
				Carry();
			}
		}
	}

	private void Swim()
	{
		base.mainBodyChunk.vel.y += 0.5f;
	}

	private void Act()
	{
		if (moveDir != neutralDir)
		{
			remMoveDir = moveDir;
		}
		lastJawOpen = jawOpen;
		if (base.grasps[0] != null)
		{
			jawOpen = 0.15f;
		}
		else if (jawSlamPause > 0)
		{
			jawSlamPause--;
		}
		else
		{
			if (jawVel == 0f)
			{
				jawVel = 0.15f;
			}
			if (base.safariControlled && jawVel >= 0f && jawVel < 1f && !controlledJawSnap)
			{
				jawVel = 0f;
				jawOpen = 0f;
			}
			jawOpen += jawVel;
			if (jawKeepOpenPause > 0)
			{
				jawKeepOpenPause--;
				jawOpen = Mathf.Clamp(Mathf.Lerp(jawOpen, keepJawOpenPos, UnityEngine.Random.value * 0.5f), 0f, 1f);
			}
			else if (UnityEngine.Random.value < 1f / (base.Blinded ? 15f : 40f) && !base.safariControlled)
			{
				jawKeepOpenPause = UnityEngine.Random.Range(10, UnityEngine.Random.Range(10, 60));
				keepJawOpenPos = ((UnityEngine.Random.value < 0.5f) ? 0f : 1f);
				jawVel = Mathf.Lerp(-0.4f, 0.4f, UnityEngine.Random.value);
				jawOpen = Mathf.Clamp(jawOpen, 0f, 1f);
			}
			else if (jawOpen <= 0f)
			{
				jawOpen = 0f;
				if (jawVel < -0.4f)
				{
					JawSlamShut();
					controlledJawSnap = false;
				}
				jawVel = 0.15f;
				jawSlamPause = 5;
			}
			else if (jawOpen >= 1f)
			{
				jawOpen = 1f;
				jawVel = -0.5f;
			}
		}
		int num = 0;
		if (Custom.ManhattanDistance(base.abstractCreature.pos, AI.pathFinder.GetEffectualDestination) > 3)
		{
			pastPositions.Insert(0, base.abstractCreature.pos.Tile);
			if (pastPositions.Count > 40)
			{
				pastPositions.RemoveAt(pastPositions.Count - 1);
			}
			for (int i = 20; i < pastPositions.Count; i++)
			{
				if (Custom.DistLess(base.abstractCreature.pos.Tile, pastPositions[i], 4f))
				{
					num++;
				}
			}
		}
		if (num > 10)
		{
			stuckCounter++;
			sprintStuckCounter++;
		}
		else
		{
			stuckCounter -= 2;
			sprintStuckCounter--;
		}
		stuckCounter = Custom.IntClamp(stuckCounter, 0, 200);
		if (base.safariControlled)
		{
			stuckCounter = 0;
		}
		if (stuckCounter > 100)
		{
			if (UnityEngine.Random.value < 1f / 30f)
			{
				legs[UnityEngine.Random.Range(0, 2)].footSecurePos = null;
			}
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				base.bodyChunks[j].vel += Custom.RNV() * Custom.LerpMap(stuckCounter, 100f, 200f, 0f, 3f);
			}
		}
		if (base.safariControlled && sprintStuckCounter > 100 && inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp)
		{
			if (UnityEngine.Random.value < 1f / 30f)
			{
				legs[UnityEngine.Random.Range(0, 2)].footSecurePos = null;
			}
			for (int k = 0; k < base.bodyChunks.Length; k++)
			{
				base.bodyChunks[k].vel += Custom.RNV() * Custom.LerpMap(sprintStuckCounter, 100f, 200f, 0f, 3f);
			}
		}
		lastRunCycle = runCycle;
		runCycle += Mathf.Sign(bodyFlip) / Mathf.Lerp(Mathf.Lerp(30f, 40f, Mathf.Pow(Mathf.Max(legs[0].springPower, legs[1].springPower), 0.3f)), Custom.LerpMap(Mathf.Abs(base.mainBodyChunk.vel.x), 2f, 10f, 50f, 20f), 1f);
		if (room.game.devToolsActive && Input.GetKey("t"))
		{
			bool flag = true;
			for (int l = 0; l < AI.tracker.CreaturesCount; l++)
			{
				if (AI.tracker.GetRep(l).representedCreature.creatureTemplate.type == CreatureTemplate.Type.MirosBird && AI.tracker.GetRep(l).representedCreature.realizedCreature.mainBodyChunk.pos.x < base.mainBodyChunk.pos.x)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				base.mainBodyChunk.vel.y -= 12f;
			}
		}
		AI.Update();
		moveDir = new Vector2(0f, 1f);
		if (base.safariControlled)
		{
			moveDir = neutralDir;
		}
		if (!base.safariControlled && room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.mainBodyChunk.pos), base.Template) && AI.pathFinder.GetEffectualDestination.TileDefined && AI.pathFinder.GetEffectualDestination.room == room.abstractRoom.index && Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(AI.pathFinder.GetEffectualDestination), 30f))
		{
			moveDir = Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetEffectualDestination) - base.mainBodyChunk.pos, 30f) / 30f;
		}
		else
		{
			FollowPath();
		}
		if (room == null)
		{
			return;
		}
		lastBodyFlip = bodyFlip;
		if (Mathf.Abs(moveDir.x) < 0.3f)
		{
			bodyFlip *= 0.7f;
		}
		else if (moveDir.x < 0f)
		{
			bodyFlip = Mathf.Max(-1f, bodyFlip - 0.1f);
		}
		else
		{
			bodyFlip = Mathf.Min(1f, bodyFlip + 0.1f);
		}
		float num2 = 0f;
		float num3 = forwardPower;
		forwardPower = 0f;
		bool flag2 = true;
		for (int m = 0; m < legs.Length; m++)
		{
			if (legs[m].groundContact)
			{
				num2 = 1f;
				forwardPower += legs[m].forwardPower / 2f;
			}
			else
			{
				flag2 = false;
			}
		}
		if (!flag2 && Custom.ManhattanDistance(base.abstractCreature.pos, AI.pathFinder.GetEffectualDestination) < 6)
		{
			weightDownToStandOnBothLegs = Mathf.Min(1f, weightDownToStandOnBothLegs + 1f / 30f);
		}
		else if (Mathf.Abs(moveDir.x) > 0.3f)
		{
			weightDownToStandOnBothLegs = Mathf.Max(0f, weightDownToStandOnBothLegs - 0.1f);
		}
		bool flag3 = false;
		int num4 = base.abstractCreature.pos.y;
		while (num4 >= base.abstractCreature.pos.y - 3 && !flag3)
		{
			flag3 = room.aimap.TileAccessibleToCreature(new IntVector2(base.abstractCreature.pos.x, num4), base.Template);
			num4--;
		}
		if (!flag3)
		{
			num2 = 0f;
		}
		forwardPower = Mathf.Pow(forwardPower, 0.4f);
		forwardPower = Custom.LerpMap(stuckCounter, 100f, 200f, forwardPower, 1.5f);
		for (int n = 0; n < 5; n++)
		{
			base.bodyChunks[n].vel *= Mathf.Lerp(1f, 0.85f, Mathf.Pow(num2, 0.5f));
			base.bodyChunks[n].vel.y += base.gravity * Mathf.Pow(num2, 0.5f) * Mathf.InverseLerp(1f, 0.5f, weightDownToStandOnBothLegs * (1f - (legs[0].runMode + legs[1].runMode) / 2f));
			base.bodyChunks[n].vel += moveDir * Mathf.Max(forwardPower, num3 * 0.5f) * 2.6f;
		}
		Head.vel += moveDir * forwardPower * 1.5f;
		WeightedPush(1, 2, new Vector2(moveDir.x, 0f), Custom.LerpMap(Vector2.Dot(Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos), new Vector2(0f, 1f)), 0f, 1f, 0f, 1f));
		WeightedPush(1, 2, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Dot(Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos), new Vector2(0f, 1f)), -1f, 1f, 8f, 0f) * num2);
		if (!base.safariControlled && enterRoomHalf != RoomHalf(room))
		{
			if (base.abstractCreature.pos.x < 4 && enterRoomHalf && !room.GetTile(0, base.abstractCreature.pos.y).Solid)
			{
				base.mainBodyChunk.pos.x -= 3.5f;
				base.mainBodyChunk.vel.x -= 3.5f;
			}
			else if (ModManager.MMF && base.abstractCreature.pos.x < 4 && enterRoomHalf && !room.GetTile(0, base.abstractCreature.pos.y + 2).Solid)
			{
				base.mainBodyChunk.pos.x -= 3.5f;
				base.mainBodyChunk.vel.x -= 3.5f;
				base.mainBodyChunk.pos.y += 2.5f;
				base.mainBodyChunk.vel.y += 2.5f;
			}
			else if (base.abstractCreature.pos.x > room.TileWidth - 5 && !enterRoomHalf && !room.GetTile(room.TileWidth - 1, base.abstractCreature.pos.y).Solid)
			{
				base.mainBodyChunk.pos.x += 3.5f;
				base.mainBodyChunk.vel.x += 3.5f;
			}
			else if (ModManager.MMF && base.abstractCreature.pos.x > room.TileWidth - 5 && !enterRoomHalf && !room.GetTile(room.TileWidth - 1, base.abstractCreature.pos.y + 2).Solid)
			{
				base.mainBodyChunk.pos.x += 3.5f;
				base.mainBodyChunk.vel.x += 3.5f;
				base.mainBodyChunk.pos.y += 2.5f;
				base.mainBodyChunk.vel.y += 2.5f;
			}
		}
		if (!base.safariControlled || !inputWithDiagonals.HasValue || !inputWithDiagonals.Value.jmp || inputWithDiagonals.Value.x == 0)
		{
			return;
		}
		bool flag4 = false;
		for (int num5 = 0; num5 < 2; num5++)
		{
			if (legs[num5].footSecurePos.HasValue)
			{
				flag4 = true;
				break;
			}
		}
		base.mainBodyChunk.pos.x = base.mainBodyChunk.pos.x + (flag4 ? 3.5f : 0.5f) * (float)inputWithDiagonals.Value.x;
		base.mainBodyChunk.vel.x = base.mainBodyChunk.vel.x + (flag4 ? 3.5f : 0.5f) * (float)inputWithDiagonals.Value.x;
	}

	public void FollowPath()
	{
		int num = base.mainBodyChunkIndex;
		MovementConnection movementConnection = (AI.pathFinder as MirosBirdPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		if (movementConnection == default(MovementConnection))
		{
			for (int i = 0; i < 4; i++)
			{
				if (!(movementConnection == default(MovementConnection)))
				{
					break;
				}
				if (room == null)
				{
					break;
				}
				for (int j = 0; j < 5; j++)
				{
					if (!(movementConnection == default(MovementConnection)))
					{
						break;
					}
					if (room == null)
					{
						break;
					}
					movementConnection = (AI.pathFinder as MirosBirdPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[i].pos) + Custom.fourDirectionsAndZero[j], actuallyFollowingThisPath: true);
					if (movementConnection != default(MovementConnection))
					{
						num = i;
						break;
					}
				}
			}
		}
		if (movementConnection == default(MovementConnection))
		{
			for (int k = 2; k < 4; k++)
			{
				if (!(movementConnection == default(MovementConnection)))
				{
					break;
				}
				if (room == null)
				{
					break;
				}
				for (int l = 0; l < 4; l++)
				{
					if (!(movementConnection == default(MovementConnection)))
					{
						break;
					}
					if (room == null)
					{
						break;
					}
					for (int m = 0; m < 4; m++)
					{
						if (!(movementConnection == default(MovementConnection)))
						{
							break;
						}
						if (room == null)
						{
							break;
						}
						movementConnection = (AI.pathFinder as MirosBirdPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[l].pos) + Custom.fourDirections[m] * k, actuallyFollowingThisPath: true);
						if (movementConnection != default(MovementConnection))
						{
							num = l;
							break;
						}
					}
				}
			}
		}
		if (room == null)
		{
			return;
		}
		if (movementConnection == default(MovementConnection))
		{
			float num2 = room.aimap.AccessibilityForCreature(room.GetTilePosition(base.mainBodyChunk.pos), base.Template);
			IntVector2? intVector = null;
			for (int n = 1; n < 3; n++)
			{
				for (int num3 = 0; num3 < 8; num3++)
				{
					float num4 = room.aimap.AccessibilityForCreature(room.GetTilePosition(base.mainBodyChunk.pos) + Custom.eightDirections[num3] * n, base.Template);
					if (num4 > num2)
					{
						num2 = num4;
						intVector = room.GetTilePosition(base.mainBodyChunk.pos) + Custom.eightDirections[num3] * n;
					}
				}
			}
			if (intVector.HasValue)
			{
				movementConnection = new MovementConnection(MovementConnection.MovementType.Standard, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(intVector.Value), 1);
			}
		}
		if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
		{
			movementConnection = default(MovementConnection);
			if (inputWithDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (movementConnection != default(MovementConnection))
				{
					type = movementConnection.type;
				}
				if (inputWithDiagonals.Value.AnyDirectionalInput)
				{
					Vector2 vector = Vector2.zero;
					if (inputWithDiagonals.Value.y == 0 && inputWithDiagonals.Value.x != 0)
					{
						vector = new Vector2(0f, 80f);
					}
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 200f + vector), 2);
				}
				if (inputWithDiagonals.Value.pckp && !lastInputWithDiagonals.Value.pckp)
				{
					controlledJawSnap = true;
				}
				if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
				{
					LoseAllGrasps();
				}
				if (inputWithDiagonals.Value.y < 0)
				{
					base.GoThroughFloors = true;
				}
				else
				{
					base.GoThroughFloors = false;
				}
			}
		}
		if (!(movementConnection != default(MovementConnection)))
		{
			return;
		}
		moveDir = Custom.DirVec(base.bodyChunks[num].pos, room.MiddleOfTile(movementConnection.DestTile));
		MovementConnection movementConnection2 = movementConnection;
		Vector2 b = room.MiddleOfTile(movementConnection2.destinationCoord);
		for (int num5 = 0; num5 < 10; num5++)
		{
			movementConnection2 = (AI.pathFinder as MirosBirdPather).FollowPath(movementConnection2.destinationCoord, actuallyFollowingThisPath: false);
			if (movementConnection2 == default(MovementConnection) || !room.VisualContact(movementConnection.StartTile, movementConnection2.DestTile) || Vector2.Distance(room.MiddleOfTile(movementConnection.startCoord), b) > Vector2.Distance(room.MiddleOfTile(movementConnection.startCoord), room.MiddleOfTile(movementConnection2.destinationCoord)))
			{
				break;
			}
			b = room.MiddleOfTile(movementConnection2.destinationCoord);
			moveDir += Custom.DirVec(base.bodyChunks[num].pos, room.MiddleOfTile(movementConnection2.DestTile));
		}
		moveDir.Normalize();
	}

	private void CheckFlip()
	{
		if (Custom.DistanceToLine(base.bodyChunks[3].pos, base.bodyChunks[1].pos, base.bodyChunks[0].pos) < 0f)
		{
			Vector2 pos = base.bodyChunks[3].pos;
			Vector2 vel = base.bodyChunks[3].vel;
			Vector2 lastPos = base.bodyChunks[3].lastPos;
			Vector2 lastLastPos = base.bodyChunks[3].lastLastPos;
			base.bodyChunks[3].pos = base.bodyChunks[2].pos;
			base.bodyChunks[3].vel = base.bodyChunks[2].vel;
			base.bodyChunks[3].lastPos = base.bodyChunks[2].lastPos;
			base.bodyChunks[3].lastLastPos = base.bodyChunks[2].lastLastPos;
			base.bodyChunks[2].pos = pos;
			base.bodyChunks[2].vel = vel;
			base.bodyChunks[2].lastPos = lastPos;
			base.bodyChunks[2].lastLastPos = lastLastPos;
		}
	}

	private void UpdateNeck()
	{
		neck.Update();
		Vector2 vector = moveDir;
		if (base.safariControlled)
		{
			vector = remMoveDir;
		}
		for (int i = 0; i < neck.tChunks.Length; i++)
		{
			float t = (float)i / (float)(neck.tChunks.Length - 1);
			neck.tChunks[i].vel *= 0.95f;
			neck.tChunks[i].vel.y -= (neck.limp ? 0.7f : 0.1f);
			if ((float)neck.backtrackFrom == -1f || neck.backtrackFrom > i)
			{
				neck.tChunks[i].vel += Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos) * Mathf.Lerp(3f, 1f, t);
			}
			neck.tChunks[i].vel -= neck.connectedChunk.vel;
			neck.tChunks[i].vel *= 0.75f;
			neck.tChunks[i].vel += neck.connectedChunk.vel;
		}
		neck.limp = !base.Consious;
		float num = ((neck.backtrackFrom == -1) ? 0.5f : 0f);
		if (base.grasps[0] == null)
		{
			Vector2 vector2 = Custom.DirVec(base.bodyChunks[4].pos, neck.tChunks[neck.tChunks.Length - 1].pos);
			float num2 = Vector2.Distance(base.bodyChunks[4].pos, neck.tChunks[neck.tChunks.Length - 1].pos);
			base.bodyChunks[4].pos -= (6f - num2) * vector2 * (1f - num);
			base.bodyChunks[4].vel -= (6f - num2) * vector2 * (1f - num);
			neck.tChunks[neck.tChunks.Length - 1].pos += (6f - num2) * vector2 * num;
			neck.tChunks[neck.tChunks.Length - 1].vel += (6f - num2) * vector2 * num;
			base.bodyChunks[4].vel += Custom.DirVec(neck.tChunks[neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * 6f * (1f - num);
			base.bodyChunks[4].vel += Custom.DirVec(neck.tChunks[neck.tChunks.Length - 1].pos, base.bodyChunks[4].pos) * 6f * (1f - num);
			neck.tChunks[neck.tChunks.Length - 1].vel -= Custom.DirVec(neck.tChunks[neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * 6f * num;
			neck.tChunks[neck.tChunks.Length - 2].vel -= Custom.DirVec(neck.tChunks[neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * 6f * num;
		}
		if (!base.Consious)
		{
			neck.retractFac = 0.5f;
			neck.floatGrabDest = null;
			return;
		}
		Head.vel.y += base.gravity;
		Vector2 a = ((AI.creatureLooker.lookCreature == null) ? (base.mainBodyChunk.pos + vector * 400f) : ((!AI.creatureLooker.lookCreature.VisualContact) ? room.MiddleOfTile(AI.creatureLooker.lookCreature.BestGuessForPosition()) : AI.creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos));
		float num3 = (legs[0].runMode + legs[1].runMode) / 2f;
		neck.retractFac = Mathf.Lerp(0.5f, 0.8f, num3);
		a = Vector2.Lerp(a, base.mainBodyChunk.pos + vector * 200f, Mathf.Pow(num3, 6f));
		if (base.Blinded)
		{
			a = base.mainBodyChunk.pos + Custom.RNV() * UnityEngine.Random.value * 400f;
		}
		if ((Custom.DistLess(a, base.mainBodyChunk.pos, 220f) && !room.VisualContact(a, Head.pos)) || num3 > 0.5f)
		{
			List<IntVector2> path = null;
			neck.MoveGrabDest(a, ref path);
		}
		else if (neck.backtrackFrom == -1)
		{
			neck.floatGrabDest = null;
		}
		Vector2 vector3 = Custom.DirVec(Head.pos, a);
		if (base.grasps[0] == null)
		{
			neck.tChunks[neck.tChunks.Length - 1].vel += vector3 * num * 1.2f;
			neck.tChunks[neck.tChunks.Length - 2].vel -= vector3 * 0.5f * num;
			Head.vel += vector3 * 6f * (1f - num);
		}
		else
		{
			neck.tChunks[neck.tChunks.Length - 1].vel += vector3 * 2f * num;
			neck.tChunks[neck.tChunks.Length - 2].vel -= vector3 * 2f * num;
			base.grasps[0].grabbedChunk.vel += vector3 / base.grasps[0].grabbedChunk.mass;
		}
		if (Custom.DistLess(Head.pos, a, 80f * Mathf.InverseLerp(1f, 0.5f, jawOpen)))
		{
			for (int j = 0; j < neck.tChunks.Length; j++)
			{
				neck.tChunks[j].vel -= vector3 * Mathf.InverseLerp(80f, 20f, Vector2.Distance(Head.pos, a)) * 8f * num;
			}
		}
	}

	public void Carry()
	{
		if (!base.Consious)
		{
			LoseAllGrasps();
			return;
		}
		BodyChunk grabbedChunk = base.grasps[0].grabbedChunk;
		if (ModManager.MMF && grabbedChunk.owner is TentaclePlant && UnityEngine.Random.value < 0.1f)
		{
			(grabbedChunk.owner as TentaclePlant).Stun(100);
			LoseAllGrasps();
		}
		if (!base.safariControlled && UnityEngine.Random.value < 1f / 120f && (!(grabbedChunk.owner is Creature) || base.Template.CreatureRelationship((grabbedChunk.owner as Creature).Template).type != CreatureTemplate.Relationship.Type.Eats))
		{
			LoseAllGrasps();
			return;
		}
		float num = grabbedChunk.mass / (grabbedChunk.mass + Head.mass);
		float num2 = grabbedChunk.mass / (grabbedChunk.mass + base.bodyChunks[0].mass);
		if (neck.backtrackFrom != -1 || enteringShortCut.HasValue)
		{
			num = 0f;
			num2 = 0f;
		}
		if (!Custom.DistLess(grabbedChunk.pos, neck.tChunks[neck.tChunks.Length - 1].pos, 20f))
		{
			Vector2 vector = Custom.DirVec(grabbedChunk.pos, neck.tChunks[neck.tChunks.Length - 1].pos);
			float num3 = Vector2.Distance(grabbedChunk.pos, neck.tChunks[neck.tChunks.Length - 1].pos);
			grabbedChunk.pos -= (20f - num3) * vector * (1f - num);
			grabbedChunk.vel -= (20f - num3) * vector * (1f - num);
			neck.tChunks[neck.tChunks.Length - 1].pos += (20f - num3) * vector * num;
			neck.tChunks[neck.tChunks.Length - 1].vel += (20f - num3) * vector * num;
		}
		if (!enteringShortCut.HasValue)
		{
			Head.pos = Vector2.Lerp(neck.tChunks[neck.tChunks.Length - 1].pos, grabbedChunk.pos, 0.1f);
			Head.vel = neck.tChunks[neck.tChunks.Length - 1].vel;
		}
		float num4 = 40f;
		if (!Custom.DistLess(base.mainBodyChunk.pos, grabbedChunk.pos, num4))
		{
			if (!Custom.DistLess(base.mainBodyChunk.pos, grabbedChunk.pos, num4 * 3f))
			{
				LoseAllGrasps();
				return;
			}
			Vector2 vector2 = Custom.DirVec(grabbedChunk.pos, base.bodyChunks[0].pos);
			float num5 = Vector2.Distance(grabbedChunk.pos, base.bodyChunks[0].pos);
			grabbedChunk.pos -= (num4 - num5) * vector2 * (1f - num2);
			grabbedChunk.vel -= (num4 - num5) * vector2 * (1f - num2);
			base.bodyChunks[0].pos += (num4 - num5) * vector2 * num2;
			base.bodyChunks[0].vel += (num4 - num5) * vector2 * num2;
		}
		if (grabbedChunk.owner is Creature && (grabbedChunk.owner as Creature).enteringShortCut.HasValue)
		{
			LoseAllGrasps();
		}
	}

	public void JawSlamShut()
	{
		Vector2 vector = Custom.DirVec(neck.Tip.pos, Head.pos);
		neck.Tip.vel -= vector * 10f;
		neck.Tip.pos += vector * 20f;
		Head.pos += vector * 20f;
		int num = 0;
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (base.grasps[0] != null)
			{
				break;
			}
			if (room.abstractRoom.creatures[i] == base.abstractCreature || !AI.DoIWantToBiteCreature(room.abstractRoom.creatures[i]) || room.abstractRoom.creatures[i].realizedCreature == null || room.abstractRoom.creatures[i].realizedCreature.enteringShortCut.HasValue)
			{
				continue;
			}
			for (int j = 0; j < room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length; j++)
			{
				if (base.grasps[0] != null)
				{
					break;
				}
				if (Custom.DistLess(Head.pos + vector * 20f, room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].pos, 20f + room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].rad) && room.VisualContact(Head.pos, room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].pos))
				{
					Grab(room.abstractRoom.creatures[i].realizedCreature, 0, j, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, overrideEquallyDominant: true, pacifying: true);
					jawOpen = 0.15f;
					jawVel = 0f;
					num = ((!(room.abstractRoom.creatures[i].realizedCreature is Player)) ? 1 : 2);
					room.abstractRoom.creatures[i].realizedCreature.Violence(Head, Custom.DirVec(Head.pos, room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].pos) * 4f, room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j], null, DamageType.Bite, 1.2f, 0f);
					break;
				}
			}
			if (!(room.abstractRoom.creatures[i].realizedCreature is DaddyLongLegs))
			{
				continue;
			}
			for (int k = 0; k < (room.abstractRoom.creatures[i].realizedCreature as DaddyLongLegs).tentacles.Length; k++)
			{
				for (int l = 0; l < (room.abstractRoom.creatures[i].realizedCreature as DaddyLongLegs).tentacles[k].tChunks.Length; l++)
				{
					if (Custom.DistLess(Head.pos + vector * 20f, (room.abstractRoom.creatures[i].realizedCreature as DaddyLongLegs).tentacles[k].tChunks[l].pos, 20f))
					{
						(room.abstractRoom.creatures[i].realizedCreature as DaddyLongLegs).tentacles[k].stun = UnityEngine.Random.Range(10, 70);
						for (int m = l; m < (room.abstractRoom.creatures[i].realizedCreature as DaddyLongLegs).tentacles[k].tChunks.Length; m++)
						{
							(room.abstractRoom.creatures[i].realizedCreature as DaddyLongLegs).tentacles[k].tChunks[m].vel += Custom.DirVec((room.abstractRoom.creatures[i].realizedCreature as DaddyLongLegs).tentacles[k].tChunks[m].pos, (room.abstractRoom.creatures[i].realizedCreature as DaddyLongLegs).tentacles[k].connectedChunk.pos) * Mathf.Lerp(10f, 50f, UnityEngine.Random.value);
						}
						break;
					}
				}
			}
		}
		switch (num)
		{
		case 0:
			room.PlaySound(SoundID.Miros_Beak_Snap_Miss, Head);
			break;
		case 1:
			room.PlaySound(SoundID.Miros_Beak_Snap_Hit_Slugcat, Head);
			break;
		default:
			room.PlaySound(SoundID.Miros_Beak_Snap_Hit_Other, Head);
			break;
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (speed > 2.5f && firstContact)
		{
			room.PlaySound((speed < 12f) ? SoundID.Miros_Light_Terrain_Impact : SoundID.Miros_Heavy_Terrain_Impact, base.mainBodyChunk);
		}
	}

	public override void Stun(int st)
	{
		if (UnityEngine.Random.value < Mathf.InverseLerp(st, 0f, 30f))
		{
			LoseAllGrasps();
		}
		base.Stun(st);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		neck.Reset(base.mainBodyChunk.pos);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * (-1.5f + (float)i) * 15f;
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 8f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		neck.NewRoom(room);
		enterRoomHalf = RoomHalf(newRoom);
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].Reset();
		}
		pastPositions = new List<IntVector2>();
	}
}
