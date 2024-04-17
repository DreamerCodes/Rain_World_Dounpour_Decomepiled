using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Deer : Creature
{
	public struct IndividualVariations
	{
		public float patternDisplacement;

		public int finsSeed;

		public HSLColor patternColorA;

		public HSLColor patternColorB;

		public IndividualVariations(float patternDisplacement, int finsSeed, HSLColor patternColorA, HSLColor patternColorB)
		{
			this.patternDisplacement = patternDisplacement;
			this.finsSeed = finsSeed;
			this.patternColorA = patternColorA;
			this.patternColorB = patternColorB;
		}
	}

	public class PlayerInAntlers
	{
		public class GrabStance
		{
			public AntlerPoint upper;

			public AntlerPoint lower;

			public PlayerInAntlers PIA;

			public float LowerBodyFrc
			{
				get
				{
					if (lower == null)
					{
						return 0f;
					}
					return 1f;
				}
			}

			public GrabStance(PlayerInAntlers PIA, AntlerPoint upper, AntlerPoint lower)
			{
				this.PIA = PIA;
				this.upper = upper;
				this.lower = lower;
			}

			public void UpdateLower()
			{
				Vector2 vector = upper.PlaySpaceCoordinate(PIA);
				if (lower == null)
				{
					if (PIA.dangle)
					{
						return;
					}
					Vector2 vector2 = vector + new Vector2(0f, -15f);
					float dst = float.MaxValue;
					IntVector2 intVector = new IntVector2(-1, -1);
					for (int i = 0; i < PIA.antlers.parts.Length; i++)
					{
						for (int j = 0; j < PIA.antlers.parts[i].positions.Length; j++)
						{
							Vector2 vector3 = PIA.PlaySpaceCoordinate(i, j, upper.side);
							if (LegalLowerPos(vector3) && Custom.DistLess(vector2, vector3, dst))
							{
								dst = Vector2.Distance(vector2, vector3);
								intVector = new IntVector2(i, j);
							}
						}
					}
					if (intVector.x != -1 && intVector.y != -1)
					{
						lower = new AntlerPoint(intVector.x, intVector.y, upper.side);
					}
				}
				else if (!LegalLowerPos(lower.PlaySpaceCoordinate(PIA)))
				{
					lower = null;
				}
			}

			private bool LegalLowerPos(Vector2 testPos)
			{
				Vector2 p = upper.PlaySpaceCoordinate(PIA);
				if (Custom.DistLess(p, testPos, 30f) && !Custom.DistLess(p, testPos, 5f))
				{
					return !Custom.DistLess(testPos, PIA.deer.mainBodyChunk.pos, PIA.deer.mainBodyChunk.rad + 9f);
				}
				return false;
			}
		}

		public class AntlerPoint
		{
			public int part;

			public int segment;

			public float side;

			public AntlerPoint(int part, int segment, float side)
			{
				this.part = part;
				this.segment = segment;
				this.side = side;
			}

			public Vector2 PlaySpaceCoordinate(PlayerInAntlers PIA)
			{
				return PIA.PlaySpaceCoordinate(part, segment, side);
			}
		}

		public Player player;

		public Deer deer;

		public DeerGraphics.Antlers antlers;

		public BodyChunk antlerChunk;

		public GrabStance stance;

		public GrabStance nextStance;

		public float movProg;

		public Vector2 climbGoal;

		public bool dangle;

		public bool playerDisconnected;

		public DebugSprite dbSpr;

		public DebugSprite dbSpr2;

		public DebugSprite dbSpr3;

		private bool forceSideChange;

		public Vector2[] handGrabPoints;

		public AntlerPoint oddHandGrip;

		public int oddHand;

		public List<VultureAbstractAI> vultures;

		public int turnAroundCoolDown;

		public float FaceDir => (deer.graphicsModule as DeerGraphics).CurrentFaceDir(1f);

		public PlayerInAntlers(Player player, Deer deer)
		{
			this.player = player;
			this.deer = deer;
			antlers = (deer.graphicsModule as DeerGraphics).antlers;
			antlerChunk = deer.bodyChunks[5];
			if (ModManager.MMF && this.player.room.game.IsStorySession && !this.player.room.game.rainWorld.progression.miscProgressionData.deerControlTutorialShown && MMF.cfgExtraTutorials.Value && MMF.cfgDeerBehavior.Value)
			{
				this.player.room.game.cameras[0].hud.textPrompt.AddMessage(this.player.room.game.manager.rainWorld.inGameTranslator.Translate("Wiggling around quickly might startle this creature."), 30, 250, darken: true, hideHud: true);
				this.player.room.game.rainWorld.progression.miscProgressionData.deerControlTutorialShown = true;
			}
			handGrabPoints = new Vector2[2];
			for (int i = 0; i < 2; i++)
			{
				handGrabPoints[i] = player.mainBodyChunk.pos;
			}
			float num = Mathf.Sign(FaceDir);
			int part = -1;
			int segment = -1;
			float side = num;
			float num2 = float.MaxValue;
			for (int j = 0; j < antlers.parts.Length; j++)
			{
				for (int k = 0; k < antlers.parts[j].positions.Length; k++)
				{
					for (float num3 = -1f; num3 <= 1f; num3 += 2f)
					{
						float num4 = Vector2.Distance(PlaySpaceCoordinate(j, k, num3), player.mainBodyChunk.pos) + ((num3 != num) ? 100f : 0f) * Mathf.Abs(FaceDir);
						if (num4 < num2)
						{
							num2 = num4;
							part = j;
							segment = k;
							side = num3;
						}
					}
				}
			}
			dangle = true;
			stance = new GrabStance(this, new AntlerPoint(part, segment, side), null);
			deer.graphicsModule.AddObjectToInternalContainer(player.graphicsModule, 1);
			vultures = new List<VultureAbstractAI>();
			for (int l = 0; l < player.room.world.NumberOfRooms; l++)
			{
				for (int m = 0; m < player.room.world.GetAbstractRoom(player.room.world.firstRoomIndex + l).creatures.Count; m++)
				{
					if (player.room.world.GetAbstractRoom(player.room.world.firstRoomIndex + l).creatures[m].creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Vulture && player.room.world.GetAbstractRoom(player.room.world.firstRoomIndex + l).creatures[m].abstractAI != null && player.room.world.GetAbstractRoom(player.room.world.firstRoomIndex + l).creatures[m].abstractAI is VultureAbstractAI)
					{
						vultures.Add(player.room.world.GetAbstractRoom(player.room.world.firstRoomIndex + l).creatures[m].abstractAI as VultureAbstractAI);
					}
				}
			}
		}

		public void Update(bool eu)
		{
			if (ModManager.MMF && MMF.cfgDeerBehavior.Value)
			{
				if (player.Wiggle > 0.1f && turnAroundCoolDown < 0)
				{
					deer.room.PlaySound(SoundID.In_Room_Deer_Summoned, deer.mainBodyChunk, loop: false, 1f, 1.1f);
					turnAroundCoolDown = UnityEngine.Random.Range(20, 110);
					deer.AI.closeEyesCounter = turnAroundCoolDown;
					deer.AI.kneelCounter = 10;
					deer.AI.minorWanderTimer = UnityEngine.Random.Range(320, 380);
					WorldCoordinate worldCoordinate = deer.AI.IdleRoomWanderGoal();
					deer.AI.SetDestination(worldCoordinate);
					deer.AI.inRoomDestination = worldCoordinate;
					(deer.abstractCreature.abstractAI as DeerAbstractAI).damageGoHome = false;
					if (deer.AI.goToPuffBall != null && !deer.AI.deniedPuffballs.Contains(deer.AI.goToPuffBall.representedItem.ID))
					{
						deer.AI.deniedPuffballs.Add(deer.AI.goToPuffBall.representedItem.ID);
					}
					deer.AI.heldPuffballNotGiven = 0;
					deer.WeightedPush(5, 0, Custom.DegToVec(UnityEngine.Random.Range(0f, 360f)), 0.7f);
				}
				else
				{
					turnAroundCoolDown--;
				}
			}
			deer.WeightedPush(5, 0, Custom.DirVec(deer.mainBodyChunk.pos, player.mainBodyChunk.pos), 0.7f);
			for (int i = 0; i < vultures.Count; i++)
			{
				if (vultures[i].parent.pos.room == player.abstractCreature.pos.room)
				{
					continue;
				}
				for (int num = vultures[i].checkRooms.Count - 1; num >= 0; num--)
				{
					if (vultures[i].checkRooms[num].room == player.abstractCreature.pos.room)
					{
						Custom.Log("vulture repelled by player in antlers");
						vultures[i].checkRooms.RemoveAt(num);
					}
				}
				if (vultures[i].destination.room == player.abstractCreature.pos.room || vultures[i].MigrationDestination.room == player.abstractCreature.pos.room)
				{
					vultures[i].SetDestination(vultures[i].parent.pos);
					Custom.Log("vulture destination repelled by player in antlers");
				}
			}
			if (Custom.DistLess(player.mainBodyChunk.pos, antlerChunk.pos, antlerChunk.rad + 100f))
			{
				player.playerInAntlers = this;
				if (Mathf.Abs(FaceDir) > 0.5f && stance.upper.side != 0f - Mathf.Sign(FaceDir))
				{
					forceSideChange = true;
					FindCorrectSideStance(0f - Mathf.Sign(FaceDir));
				}
				else
				{
					forceSideChange = false;
				}
				if (forceSideChange)
				{
					movProg = Mathf.Min(1f, movProg + 0.1f);
					if (movProg == 1f)
					{
						stance = nextStance;
						nextStance = null;
						movProg = 0f;
						forceSideChange = false;
					}
				}
				else
				{
					Vector2 vector = new Vector2(0f, 0f);
					if (player.input[0].analogueDir.magnitude > 0f)
					{
						vector = player.input[0].analogueDir.normalized;
					}
					else if (player.input[0].x != 0 || player.input[0].y != 0)
					{
						vector = new Vector2(player.input[0].x, player.input[0].y).normalized;
					}
					if (vector.magnitude > 0f)
					{
						if (nextStance != null)
						{
							movProg = Mathf.Min(1f, movProg + 0.1f);
							if (movProg == 1f)
							{
								stance = nextStance;
								nextStance = null;
								movProg = 0f;
							}
						}
						climbGoal += vector * Custom.LerpMap(Vector2.Dot(vector, climbGoal.normalized), -1f, 1f, 1.6f, 0.8f);
						if (climbGoal.magnitude > 30f)
						{
							climbGoal = Vector2.ClampMagnitude(climbGoal, 30f);
						}
					}
					else
					{
						if (nextStance != null)
						{
							movProg = Mathf.Max(0f, movProg - 0.1f);
						}
						if (movProg == 0f)
						{
							nextStance = null;
						}
						if (climbGoal.magnitude < 1f)
						{
							climbGoal *= 0f;
						}
						else
						{
							climbGoal -= climbGoal.normalized;
						}
					}
					if (movProg == 0f)
					{
						if (player.input[0].y < 0 && player.input[1].y >= 0 && player.input[0].x == 0)
						{
							dangle = true;
							stance.lower = null;
						}
						else if (vector.magnitude > 0f)
						{
							FindNextStance();
						}
					}
					stance.UpdateLower();
				}
				Vector2 ps = ((nextStance == null) ? stance.upper.PlaySpaceCoordinate(this) : Vector2.Lerp(stance.upper.PlaySpaceCoordinate(this), nextStance.upper.PlaySpaceCoordinate(this), movProg));
				ps = PushOutOfHead(ps, 10f);
				FindHandPositions();
				if (!dangle)
				{
					player.mainBodyChunk.MoveFromOutsideMyUpdate(eu, ps);
					player.mainBodyChunk.vel = antlerChunk.vel;
				}
				else
				{
					Vector2 vector2 = Custom.DirVec(player.mainBodyChunk.pos, ps);
					float num2 = Vector2.Distance(player.mainBodyChunk.pos, ps);
					if (num2 > 10f)
					{
						player.mainBodyChunk.pos += vector2 * (num2 - 10f);
						player.mainBodyChunk.vel += vector2 * (num2 - 10f);
					}
				}
				if (stance.lower != null)
				{
					float num3 = 1f - movProg;
					player.bodyChunks[1].vel = Vector2.Lerp(player.bodyChunks[1].vel, antlerChunk.vel, num3 * 0.9f);
					player.bodyChunks[1].vel = -Vector2.ClampMagnitude(player.bodyChunks[1].pos - PushOutOfHead(stance.lower.PlaySpaceCoordinate(this), 10f), 10f * num3) / 3f;
				}
				if (!player.Consious || player.enteringShortCut.HasValue)
				{
					playerDisconnected = true;
					player.playerInAntlers = null;
					player.animation = Player.AnimationIndex.None;
				}
			}
			else
			{
				playerDisconnected = true;
				player.playerInAntlers = null;
				player.animation = Player.AnimationIndex.None;
			}
		}

		private Vector2 PushOutOfHead(Vector2 ps, float margin)
		{
			if (Custom.DistLess(ps, deer.mainBodyChunk.pos, deer.mainBodyChunk.rad + margin))
			{
				return deer.mainBodyChunk.pos + Custom.DirVec(deer.mainBodyChunk.pos, ps) * (deer.mainBodyChunk.rad + margin);
			}
			return ps;
		}

		private void FindNextStance()
		{
			Vector2 vector = stance.upper.PlaySpaceCoordinate(this);
			Vector2 vector2 = vector + climbGoal;
			IntVector2 intVector = new IntVector2(-1, -1);
			float dst = Vector2.Distance(vector, vector2) + 1f;
			for (int i = 0; i < antlers.parts.Length; i++)
			{
				for (int j = 0; j < antlers.parts[i].positions.Length; j++)
				{
					Vector2 vector3 = PlaySpaceCoordinate(i, j, stance.upper.side);
					if (Custom.DistLess(vector, vector3, 30f) && Custom.DistLess(vector2, vector3, dst))
					{
						dst = Vector2.Distance(vector2, vector3);
						intVector = new IntVector2(i, j);
					}
				}
			}
			if (intVector.x != -1 && intVector.y != -1 && (intVector.x != stance.upper.part || intVector.y != stance.upper.segment))
			{
				climbGoal *= 0f;
				nextStance = new GrabStance(this, new AntlerPoint(intVector.x, intVector.y, stance.upper.side), (stance.lower != null) ? new AntlerPoint(stance.lower.part, stance.lower.segment, stance.lower.side) : null);
				dangle = false;
			}
			else if (player.input[0].y > 0 && player.input[1].y <= 0 && player.input[0].x == 0)
			{
				dangle = false;
			}
		}

		private void FindCorrectSideStance(float side)
		{
			float dst = float.MaxValue;
			for (int i = 0; i < antlers.parts.Length; i++)
			{
				for (int j = 0; j < antlers.parts[i].positions.Length; j++)
				{
					Vector2 vector = PlaySpaceCoordinate(i, j, side);
					if (Custom.DistLess(player.mainBodyChunk.pos, vector, dst))
					{
						dst = Vector2.Distance(player.mainBodyChunk.pos, vector);
					}
				}
			}
			nextStance = new GrabStance(this, new AntlerPoint(stance.upper.part, stance.upper.segment, side), null);
		}

		private void FindHandPositions()
		{
			Vector2 vector = stance.upper.PlaySpaceCoordinate(this);
			handGrabPoints[1 - oddHand] = vector;
			if (dangle)
			{
				handGrabPoints[oddHand] = vector;
				return;
			}
			if (nextStance != null)
			{
				handGrabPoints[oddHand] = nextStance.upper.PlaySpaceCoordinate(this);
				return;
			}
			if (oddHandGrip != null && (oddHandGrip.side != stance.upper.side || !Custom.DistLess(vector, oddHandGrip.PlaySpaceCoordinate(this), 25f)))
			{
				oddHandGrip = null;
			}
			if (oddHandGrip == null)
			{
				float num = float.MaxValue;
				IntVector2 intVector = new IntVector2(-1, -1);
				for (int i = 0; i < antlers.parts.Length; i++)
				{
					for (int j = 0; j < antlers.parts[i].positions.Length; j++)
					{
						Vector2 vector2 = PlaySpaceCoordinate(i, j, stance.upper.side);
						float num2 = Mathf.Min(Vector2.Distance(vector2, vector + Custom.PerpendicularVector((player.mainBodyChunk.pos - player.bodyChunks[1].pos).normalized) * 35f), Vector2.Distance(vector2, vector + Custom.PerpendicularVector((player.mainBodyChunk.pos - player.bodyChunks[1].pos).normalized) * -35f));
						if (Custom.DistLess(vector, vector2, 25f) && num2 < num)
						{
							num = num2;
							intVector = new IntVector2(i, j);
							oddHand = ((!(Custom.DistanceToLine(vector2, vector, player.bodyChunks[1].pos) < 0f)) ? 1 : 0);
						}
					}
				}
				if (intVector.x != -1 && intVector.y != -1)
				{
					oddHandGrip = new AntlerPoint(intVector.x, intVector.y, stance.upper.side);
				}
			}
			if (oddHandGrip != null)
			{
				handGrabPoints[oddHand] = oddHandGrip.PlaySpaceCoordinate(this);
			}
			else
			{
				handGrabPoints[oddHand] = vector;
			}
		}

		public Vector2 PlaySpaceCoordinate(int part, int segment, float side)
		{
			return antlers.TransformToHeadRotat(antlers.parts[part].GetTransoformedPos(segment, side), antlerChunk.pos, Custom.AimFromOneVectorToAnother(deer.mainBodyChunk.pos, antlerChunk.pos), side, FaceDir);
		}
	}

	public DeerAI AI;

	public Vector2 bodDir;

	public float flipDir;

	public Vector2 moveDirection;

	public bool stayStill;

	public bool heldBackByLeg;

	public DeerTentacle[] legs;

	public int legsGrabbing;

	public float nextFloorHeight;

	public float resting;

	public float preferredHeight;

	public int hesistCounter;

	public bool wormGrassBelow;

	public PhysicalObject eatObject;

	public int eatCounter;

	private int violenceReaction;

	public IndividualVariations iVars;

	public List<PlayerInAntlers> playersInAntlers;

	public int controlledRoarCounter;

	public float lastControlX;

	private float enterRoomForcePush;

	public Vector2 HeadDir => Vector3.Slerp(Custom.PerpendicularVector(bodDir) * Mathf.Sign(bodDir.x), new Vector2(0f, 1f), Mathf.Pow(Mathf.InverseLerp(1f, 0f, Mathf.Abs(bodDir.x)), 0.5f));

	public BodyChunk antlers => base.bodyChunks[5];

	public float Hierarchy => base.abstractCreature.personality.dominance;

	public float GetUnstuckForce
	{
		get
		{
			if (base.safariControlled)
			{
				return 0f;
			}
			return AI.stuckTracker.Utility();
		}
	}

	public bool Kneeling
	{
		get
		{
			if (base.safariControlled)
			{
				if (inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.jmp)
				{
					return room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile);
				}
				return false;
			}
			if (AI.kneelCounter > 0)
			{
				return room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile);
			}
			return false;
		}
	}

	public float CloseToEdge => Mathf.Max(Mathf.InverseLerp(500f, 300f, base.mainBodyChunk.pos.x), Mathf.InverseLerp(room.PixelWidth - 500f, room.PixelWidth - 300f, base.mainBodyChunk.pos.x));

	private void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		float num = Custom.WrappedRandomVariation(0.65f, 0.2f, 0.8f);
		iVars = new IndividualVariations(UnityEngine.Random.value, UnityEngine.Random.Range(0, int.MaxValue), new HSLColor(num, Mathf.Lerp(0.5f, 0.95f, UnityEngine.Random.value), Mathf.Lerp(0.12f, 0.18f, UnityEngine.Random.value)), new HSLColor(num + ((UnityEngine.Random.value < 0.5f) ? (-0.15f) : 0.15f), 1f, 0.2f));
		UnityEngine.Random.state = state;
	}

	public Deer(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		collisionRange = 1000f;
		base.bodyChunks = new BodyChunk[6];
		bodyChunkConnections = new BodyChunkConnection[5];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 22.5f, 3f);
		for (int i = 1; i < 5; i++)
		{
			float num = (float)i / 4f;
			num = (1f - num) * 0.5f + Mathf.Sin(Mathf.Pow(num, 0.5f) * (float)Math.PI) * 0.5f;
			num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(num, 1f, 0.2f)), 0.7f);
			base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(0f, 0f), Mathf.Lerp(10f, 35f, num), Mathf.Lerp(1f, 8f, num));
			base.bodyChunks[i].restrictInRoomRange = 2000f;
			base.bodyChunks[i].defaultRestrictInRoomRange = 2000f;
		}
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 38f, BodyChunkConnection.Type.Normal, 1f, -1f);
		for (int j = 1; j < 4; j++)
		{
			bodyChunkConnections[j] = new BodyChunkConnection(base.bodyChunks[j], base.bodyChunks[j + 1], Mathf.Max(base.bodyChunks[j].rad, base.bodyChunks[j + 1].rad) * 0.8f, BodyChunkConnection.Type.Normal, 1f, -1f);
		}
		if (ModManager.MMF && MMF.cfgDeerBehavior.Value)
		{
			base.bodyChunks[5] = new BodyChunk(this, 0, new Vector2(0f, 0f), Mathf.Lerp(30f, 60f + 20f * Mathf.InverseLerp(0.8f, 1f, abstractCreature.personality.dominance), abstractCreature.personality.dominance), 0.5f);
		}
		else
		{
			base.bodyChunks[5] = new BodyChunk(this, 0, new Vector2(0f, 0f), Mathf.Lerp(30f, 70f, abstractCreature.personality.dominance), 0.5f);
		}
		bodyChunkConnections[4] = new BodyChunkConnection(base.bodyChunks[0], antlers, base.bodyChunks[0].rad + antlers.rad - 10f, BodyChunkConnection.Type.Normal, 1f, 0f);
		antlers.collideWithObjects = false;
		base.bodyChunks[0].rotationChunk = base.bodyChunks[5];
		base.bodyChunks[5].rotationChunk = base.bodyChunks[0];
		legs = new DeerTentacle[4];
		for (int k = 0; k < 4; k++)
		{
			legs[k] = new DeerTentacle(this, base.bodyChunks[(k < 2) ? 1 : 2], (ModManager.MMF && MMF.cfgDeerBehavior.Value) ? 300f : 220f, k);
		}
		flipDir = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		lastControlX = flipDir;
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.95f;
		waterRetardationImmunity = 0f;
		base.buoyancy = 0.93f;
		base.GoThroughFloors = true;
		playersInAntlers = new List<PlayerInAntlers>();
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new DeerGraphics(this);
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		preferredHeight = 55f;
		for (int i = 0; i < 4; i++)
		{
			legs[i].NewRoom(newRoom);
		}
		enterRoomForcePush = 0f;
	}

	public override void Update(bool eu)
	{
		if (violenceReaction > 0)
		{
			violenceReaction--;
		}
		WeightedPush(1, 3, Vector3.Slerp(bodDir, new Vector2(flipDir, 0f), base.Consious ? 0.3f : 0f), 0.35f);
		WeightedPush(1, 4, Vector3.Slerp(bodDir, new Vector2(flipDir, 0f), base.Consious ? 0.3f : 0f), 0.35f);
		if (!Kneeling && eatCounter < 1)
		{
			WeightedPush(0, 1, HeadDir, 0.85f);
			WeightedPush(0, 1, bodDir, 0.6f);
			for (int i = 0; i < 4; i++)
			{
				WeightedPush(5, i, base.Consious ? (bodDir * 0.4f + HeadDir) : Custom.DirVec(base.bodyChunks[i].pos, antlers.pos), 1.1f - (float)i * 0.3f);
			}
			WeightedPush(4, 5, new Vector2(flipDir, 0f), 0.6f);
		}
		else
		{
			if (Kneeling)
			{
				WeightedPush(0, 1, -HeadDir, 0.45f);
			}
			WeightedPush(0, 1, bodDir, 0.6f);
			for (int j = 0; j < 4; j++)
			{
				WeightedPush(5, j, bodDir * 0.4f, (1.6f - (float)j * 0.3f) * (Kneeling ? 1f : 0.3f));
			}
		}
		antlers.vel *= 0.92f;
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		legsGrabbing = 0;
		float num = 0f;
		float num2 = float.MinValue;
		DeerTentacle deerTentacle = null;
		for (int k = 0; k < 4; k++)
		{
			legs[k].Update();
			if (legs[k].attachedAtTip)
			{
				legsGrabbing++;
			}
			if (legs[k].ReleaseScore() > num2)
			{
				num2 = legs[k].ReleaseScore();
				deerTentacle = legs[k];
			}
			num += legs[k].Support();
		}
		if (!stayStill && legsGrabbing > 3)
		{
			deerTentacle?.ReleaseGrip();
		}
		num = Mathf.Pow(Mathf.Min(num / 3f, 1f), 0.8f) * Mathf.Pow(1f - resting, 0.3f);
		for (int l = 0; l < 5; l++)
		{
			if (base.bodyChunks[l].ContactPoint.x != 0 || base.bodyChunks[l].ContactPoint.y < 0)
			{
				num = Mathf.Lerp(num, 1f, 0.6f);
			}
		}
		num = Mathf.Lerp(num, 1f, CloseToEdge);
		num = Mathf.Pow(num, 0.1f);
		float num3 = Mathf.Lerp(Mathf.Pow((float)legsGrabbing / 4f, 0.8f), num, 0.5f);
		bool flag = false;
		for (int m = 0; m < 4; m++)
		{
			if (flag)
			{
				break;
			}
			if (legs[m].attachedAtTip && legs[m].Tip.pos.x > base.bodyChunks[2].pos.x == moveDirection.x > 0f)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			if (!stayStill)
			{
				num3 = Mathf.Lerp(Custom.LerpMap(hesistCounter, 20f, 150f, -0.5f, 1f), num3, CloseToEdge);
			}
			hesistCounter++;
		}
		else
		{
			hesistCounter = 0;
		}
		num3 = Mathf.Lerp(num3, 1f, GetUnstuckForce);
		bodDir *= 0f;
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (base.Consious)
		{
			Act(eu, num, num3);
		}
		if (room == null)
		{
			return;
		}
		for (int n = 0; n < 3; n++)
		{
			WeightedPush(n, n + 2, -Custom.DirVec(base.bodyChunks[n].pos, base.bodyChunks[n + 2].pos), 0.45f);
			bodDir += Custom.DirVec(base.bodyChunks[n + 2].pos, base.bodyChunks[n + 1].pos);
		}
		if (base.Consious)
		{
			bodDir += new Vector2(0.8f * flipDir, 0f);
		}
		bodDir.Normalize();
		if (base.graphicsModule != null)
		{
			for (int num4 = 0; num4 < room.game.Players.Count; num4++)
			{
				if (room.game.Players[num4].pos.room != room.abstractRoom.index || room.game.Players[num4].realizedCreature == null || (room.game.Players[num4].realizedCreature as Player).wantToGrab <= 0 || !Custom.DistLess(room.game.Players[num4].realizedCreature.mainBodyChunk.pos, antlers.pos, antlers.rad))
				{
					continue;
				}
				(room.game.Players[num4].realizedCreature as Player).wantToGrab = 0;
				bool flag2 = true;
				for (int num5 = 0; num5 < playersInAntlers.Count && flag2; num5++)
				{
					flag2 = playersInAntlers[num5].player != room.game.Players[num4].realizedCreature as Player;
				}
				if (flag2)
				{
					if ((room.game.Players[num4].realizedCreature as Player).playerInAntlers != null)
					{
						(room.game.Players[num4].realizedCreature as Player).playerInAntlers.playerDisconnected = true;
					}
					playersInAntlers.Add(new PlayerInAntlers(room.game.Players[num4].realizedCreature as Player, this));
				}
			}
			for (int num6 = playersInAntlers.Count - 1; num6 >= 0; num6--)
			{
				if (playersInAntlers[num6].playerDisconnected)
				{
					playersInAntlers.RemoveAt(num6);
				}
				else
				{
					playersInAntlers[num6].Update(eu);
				}
			}
		}
		else
		{
			playersInAntlers.Clear();
		}
	}

	private void Act(bool eu, float support, float forwardPower)
	{
		if (ModManager.MMF && MMF.cfgDeerBehavior.Value && base.Submersion > 0.8f)
		{
			WeightedPush(0, 1, new Vector2(0f, 1f), 0.26f);
		}
		AI.Update();
		AI.stuckTracker.satisfiedWithThisPosition = violenceReaction < 1 && (resting > 0.5f || Kneeling);
		if (violenceReaction > 0)
		{
			AI.stuckTracker.stuckCounter = AI.stuckTracker.maxStuckCounter;
		}
		if (eatCounter > 0)
		{
			eatCounter--;
			if (eatCounter < 1 || eatObject.room != room || eatObject.grabbedBy.Count > 0 || !Custom.DistLess(base.mainBodyChunk.pos, eatObject.firstChunk.pos, 100f))
			{
				eatObject = null;
			}
			if (eatObject != null)
			{
				WeightedPush(0, 1, Custom.DirVec(base.mainBodyChunk.pos, eatObject.firstChunk.pos), Custom.LerpMap(eatCounter, 80f, 10f, 0.1f, 1.2f));
				WeightedPush(0, 2, Custom.DirVec(base.mainBodyChunk.pos, eatObject.firstChunk.pos), Custom.LerpMap(eatCounter, 80f, 10f, 0.1f, 1.2f));
				base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, eatObject.firstChunk.pos) * Custom.LerpMap(eatCounter, 80f, 10f, 0.1f, 2.75f);
				AI.cantEatFromCoordinate = room.GetWorldCoordinate(eatObject.firstChunk.pos);
				if (Custom.DistLess(base.mainBodyChunk.pos, eatObject.firstChunk.pos, base.mainBodyChunk.rad + eatObject.firstChunk.rad + 20f))
				{
					AI.closeEyesCounter = Math.Max(5, AI.closeEyesCounter);
					eatObject.firstChunk.vel = Vector2.Lerp(eatObject.firstChunk.vel, Vector2.ClampMagnitude(base.mainBodyChunk.pos + new Vector2(0f, -14f) - eatObject.firstChunk.pos, 30f) / 10f, 0.8f);
					base.mainBodyChunk.vel += Custom.RNV() * 2.6f;
					if (eatCounter == 50)
					{
						room.PlaySound(SoundID.Puffball_Eaten_By_Deer, base.mainBodyChunk);
						if (eatObject is PuffBall)
						{
							(eatObject as PuffBall).beingEaten = Mathf.Max((eatObject as PuffBall).beingEaten, 0.01f);
						}
						else
						{
							eatObject.Destroy();
						}
					}
				}
			}
			else
			{
				eatCounter = 0;
			}
		}
		bool flag = false;
		if (AI.AllowMovementBetweenRooms)
		{
			enterRoomForcePush = Mathf.Max(0f, enterRoomForcePush - 0.001f);
		}
		else if (Mathf.Abs(base.abstractCreature.pos.x) < 10)
		{
			if (room.VisualContact(base.mainBodyChunk.pos, base.mainBodyChunk.pos + new Vector2(400f, 0f)))
			{
				if (ModManager.MMF && MMF.cfgDeerBehavior.Value)
				{
					flag = true;
					flipDir = Mathf.Lerp(flipDir, 1f, 0.5f);
					AI.restingCounter = 0;
					resting = 0f;
					moveDirection.x += 0.01f;
					AI.kneelCounter = 0;
					moveDirection.x += Mathf.Pow(enterRoomForcePush, 2f) * 2f;
				}
				else
				{
					base.bodyChunks[1].vel.x += Mathf.Pow(enterRoomForcePush, 2f) * 2f;
				}
				enterRoomForcePush = Mathf.Min(4.5f, enterRoomForcePush + 1f / 180f);
			}
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				if (base.bodyChunks[i].ContactPoint.x > 0)
				{
					base.bodyChunks[i].vel += Custom.DegToVec(-90f * UnityEngine.Random.value) * 15f;
				}
			}
		}
		else if (Mathf.Abs(base.abstractCreature.pos.x - (room.TileWidth - 1)) < 10)
		{
			if (room.VisualContact(base.mainBodyChunk.pos, base.mainBodyChunk.pos + new Vector2(-400f, 0f)))
			{
				if (ModManager.MMF && MMF.cfgDeerBehavior.Value)
				{
					flag = true;
					flipDir = Mathf.Lerp(flipDir, -1f, 0.5f);
					AI.restingCounter = 0;
					resting = 0f;
					moveDirection.x -= 0.01f;
					AI.kneelCounter = 0;
					moveDirection.x -= Mathf.Pow(enterRoomForcePush, 2f) * 2f;
				}
				else
				{
					base.bodyChunks[1].vel.x -= Mathf.Pow(enterRoomForcePush, 2f) * 2f;
				}
				enterRoomForcePush = Mathf.Min(4.5f, enterRoomForcePush + 1f / 180f);
			}
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				if (base.bodyChunks[j].ContactPoint.x < 0)
				{
					base.bodyChunks[j].vel += Custom.DegToVec(90f * UnityEngine.Random.value) * 15f;
				}
			}
		}
		stayStill = room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile) && AI.pathFinder.GetEffectualDestination.room == room.abstractRoom.index && Custom.ManhattanDistance(base.abstractCreature.pos, AI.pathFinder.GetEffectualDestination) < 4 && (!ModManager.MMF || !MMF.cfgDeerBehavior.Value || (GetUnstuckForce < 0.9f && !flag));
		bool flag2 = false;
		if (stayStill && AI.pathFinder.DestInRoom)
		{
			flag2 = true;
			for (int k = -1; k < 2 && flag2; k++)
			{
				if (room.aimap.getAItile(AI.pathFinder.GetEffectualDestination.Tile + new IntVector2(k * 2, 0)).floorAltitude > 5)
				{
					flag2 = false;
				}
			}
		}
		if (flag2)
		{
			resting = Mathf.Min(1f, resting + 1f / 60f);
		}
		else
		{
			resting = Mathf.Max(0f, resting - 1f / 160f);
		}
		if (!base.safariControlled && resting >= 0.1f && !Kneeling)
		{
			flipDir = Mathf.Sign(base.bodyChunks[0].pos.x - base.bodyChunks[4].pos.x);
		}
		if (base.safariControlled)
		{
			flipDir = lastControlX;
		}
		preferredHeight = 5f + 10f * Mathf.Pow(1f - resting, 5f);
		IntVector2 tilePosition = room.GetTilePosition(base.bodyChunks[1].pos);
		wormGrassBelow = false;
		int num = 99;
		for (int num2 = tilePosition.y; num2 >= 0; num2--)
		{
			if (room.GetTile(new IntVector2(tilePosition.x, num2)).wormGrass)
			{
				preferredHeight = 15f;
				wormGrassBelow = true;
				break;
			}
			if (room.GetTile(new IntVector2(tilePosition.x, num2)).Solid)
			{
				num = tilePosition.y - num2;
				break;
			}
		}
		if (stayStill)
		{
			moveDirection = Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetEffectualDestination) - base.mainBodyChunk.pos, 20f) / Mathf.Lerp(30f, 5f, resting);
			nextFloorHeight = Mathf.Lerp(nextFloorHeight, (float)Custom.IntClamp(room.aimap.getAItile(base.mainBodyChunk.pos).floorAltitude + 2, 0, 17) * 20f, 0.2f);
		}
		else
		{
			MovementConnection movementConnection = (AI.pathFinder as DeerPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
			if (movementConnection == default(MovementConnection))
			{
				for (int l = 1; l < 3; l++)
				{
					if (!(movementConnection == default(MovementConnection)))
					{
						break;
					}
					if (room == null)
					{
						break;
					}
					for (int m = 0; m < 5; m++)
					{
						if (!(movementConnection == default(MovementConnection)))
						{
							break;
						}
						if (room == null)
						{
							break;
						}
						for (int n = 0; n < 5; n++)
						{
							if (!(movementConnection == default(MovementConnection)))
							{
								break;
							}
							if (room == null)
							{
								break;
							}
							movementConnection = (AI.pathFinder as DeerPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[m].pos + Custom.fourDirectionsAndZero[n].ToVector2() * 20f * l), actuallyFollowingThisPath: true);
						}
					}
				}
			}
			if (room == null)
			{
				return;
			}
			if (base.abstractCreature.controlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
			{
				movementConnection = default(MovementConnection);
				moveDirection = Vector2.zero;
				if (inputWithoutDiagonals.HasValue)
				{
					MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
					if (movementConnection != default(MovementConnection))
					{
						type = movementConnection.type;
					}
					if (controlledRoarCounter <= 0)
					{
						if (inputWithoutDiagonals.Value.AnyDirectionalInput)
						{
							float y = 0f;
							if (inputWithoutDiagonals.Value.y < 0 && ((num > 4 && !Kneeling) || (num > 3 && Kneeling)))
							{
								y = inputWithoutDiagonals.Value.y;
							}
							float num3 = inputWithoutDiagonals.Value.x;
							if (num3 != 0f)
							{
								lastControlX = num3;
							}
							if (num3 == 0f && inputWithoutDiagonals.Value.y < 0)
							{
								num3 = lastControlX * 0.01f;
							}
							movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(num3, y) * 200f), 2);
						}
						if (inputWithoutDiagonals.Value.thrw && !lastInputWithoutDiagonals.Value.thrw)
						{
							room.PlaySound(SoundID.In_Room_Deer_Summoned, base.mainBodyChunk, loop: false, 1f, UnityEngine.Random.value * 0.5f + 0.8f);
							AI.closeEyesCounter = 120;
							controlledRoarCounter = 120;
						}
						if (inputWithoutDiagonals.Value.y < 0)
						{
							AI.layDownAndRestCounter = 100;
							AI.restingCounter = 100;
						}
						else
						{
							AI.layDownAndRestCounter = 0;
							AI.restingCounter = 0;
						}
					}
				}
				if (controlledRoarCounter > 0)
				{
					controlledRoarCounter--;
				}
			}
			if (movementConnection != default(MovementConnection))
			{
				if (AI.AllowMovementBetweenRooms && AI.pathFinder.GetDestination.room != room.abstractRoom.index && movementConnection.startCoord.x <= 0)
				{
					moveDirection = new Vector2(-1f, 0f);
				}
				else if (AI.AllowMovementBetweenRooms && AI.pathFinder.GetDestination.room != room.abstractRoom.index && movementConnection.startCoord.x >= room.TileWidth - 1)
				{
					moveDirection = new Vector2(1f, 0f);
				}
				else
				{
					WorldCoordinate destinationCoord = movementConnection.destinationCoord;
					if (room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile))
					{
						for (int num4 = 0; num4 < 15; num4++)
						{
							if (AI.AllowMovementBetweenRooms && AI.pathFinder.GetDestination.room != room.abstractRoom.index && destinationCoord.x <= 0)
							{
								destinationCoord.x--;
								hesistCounter = 0;
								continue;
							}
							if (AI.AllowMovementBetweenRooms && AI.pathFinder.GetDestination.room != room.abstractRoom.index && destinationCoord.x >= room.TileWidth - 1)
							{
								destinationCoord.x++;
								hesistCounter = 0;
								continue;
							}
							MovementConnection movementConnection2 = (AI.pathFinder as DeerPather).FollowPath(destinationCoord, actuallyFollowingThisPath: false);
							if (!(movementConnection2 != default(MovementConnection)) || movementConnection2.type != 0 || !(movementConnection2.DestTile != movementConnection.startCoord.Tile) || !AI.pathFinder.RayTraceInAccessibleTiles(movementConnection.startCoord.Tile, movementConnection2.DestTile))
							{
								break;
							}
							destinationCoord = movementConnection2.destinationCoord;
						}
					}
					if (!base.safariControlled && resting < 0.1f && destinationCoord.TileDefined && !Kneeling && Mathf.Abs(room.MiddleOfTile(destinationCoord).x - base.bodyChunks[0].pos.x) > 100f)
					{
						flipDir = Mathf.Sign(room.MiddleOfTile(destinationCoord).x - base.bodyChunks[0].pos.x);
					}
					if (base.abstractCreature.pos.Tile.x < 0 || base.abstractCreature.pos.Tile.x >= room.TileWidth || destinationCoord.x < 0 || destinationCoord.x >= room.TileWidth)
					{
						nextFloorHeight = 300f;
					}
					else
					{
						nextFloorHeight = Mathf.Lerp(nextFloorHeight, (float)Custom.IntClamp(room.aimap.getAItile(destinationCoord).smoothedFloorAltitude + 2, 0, 17) * 20f, 0.2f);
					}
					moveDirection = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(destinationCoord));
				}
			}
		}
		for (int num5 = 0; num5 < 5; num5++)
		{
			float num6 = (float)num5 / 5f;
			base.bodyChunks[num5].vel *= Mathf.Lerp(1f, stayStill ? 0.7f : 0.92f, support);
			base.bodyChunks[num5].vel *= 1f - resting * 0.5f;
			if (num5 < 4)
			{
				base.bodyChunks[num5].vel.y += base.gravity * Mathf.Lerp(1.3f, 2.5f, Mathf.Sin(Mathf.Pow(num6, 1.7f) * (float)Math.PI)) * Mathf.Lerp(support * Custom.LerpMap(room.aimap.getClampedAItile(base.bodyChunks[num5].pos).smoothedFloorAltitude, 14f, 18f, 1f, 0.5f) * Custom.LerpMap(moveDirection.y, -1f, 1f, 0.5f, 1f), 0.65f, CloseToEdge);
			}
			base.bodyChunks[num5].vel += moveDirection * Mathf.Lerp(0.35f, 0f, num6) * forwardPower * Mathf.Lerp(1f, 3f, GetUnstuckForce);
		}
		base.mainBodyChunk.vel.y += base.gravity * 1.2f * resting;
		if (GetUnstuckForce > 0f && (!base.safariControlled || (base.safariControlled && inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.AnyDirectionalInput)))
		{
			base.bodyChunks[1].vel += Custom.RNV() * UnityEngine.Random.value * 4f * GetUnstuckForce;
		}
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (damage > 0.9f)
		{
			violenceReaction = Math.Max(violenceReaction, UnityEngine.Random.Range(10, (int)Custom.LerpMap(damage, 0.9f, 3f, 20f, 60f)));
			(base.abstractCreature.abstractAI as DeerAbstractAI).damageGoHome = true;
			AI.timeInRoom += 100;
		}
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
	}

	public void AccessSideSpace(WorldCoordinate start, WorldCoordinate dest)
	{
		room.game.shortcuts.CreatureTakeFlight(this, AbstractRoomNode.Type.SideExit, start, dest);
	}

	public void EatObject(PhysicalObject obj)
	{
		if (eatObject == null)
		{
			eatObject = obj;
			eatCounter = 160;
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!base.safariControlled && otherObject is Deer)
		{
			AI.deerPileCounter += 5;
		}
		if (resting > 0.5f)
		{
			AI.restingCounter -= 10;
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (speed > 1.5f && firstContact)
		{
			room.PlaySound((speed < 8f) ? SoundID.Leviathan_Light_Terrain_Impact : SoundID.Leviathan_Heavy_Terrain_Impact, base.mainBodyChunk);
		}
	}

	public override void Die()
	{
		base.Die();
	}

	public override Color ShortCutColor()
	{
		return new Color(0f, 0f, 0f);
	}
}
