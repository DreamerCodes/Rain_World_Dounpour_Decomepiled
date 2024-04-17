using System;
using MoreSlugcats;
using Noise;
using RWCustom;
using UnityEngine;

public class LizardAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction, IReactToSocialEvents, FriendTracker.IHaveFriendTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Travelling = new Behavior("Travelling", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public static readonly Behavior Injured = new Behavior("Injured", register: true);

		public static readonly Behavior Fighting = new Behavior("Fighting", register: true);

		public static readonly Behavior Frustrated = new Behavior("Frustrated", register: true);

		public static readonly Behavior ActingOutMission = new Behavior("ActingOutMission", register: true);

		public static readonly Behavior Lurk = new Behavior("Lurk", register: true);

		public static readonly Behavior InvestigateSound = new Behavior("InvestigateSound", register: true);

		public static readonly Behavior GoToSpitPos = new Behavior("GoToSpitPos", register: true);

		public static readonly Behavior FollowFriend = new Behavior("FollowFriend", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class LizardCommunication : ExtEnum<LizardCommunication>
	{
		public static readonly LizardCommunication Agression = new LizardCommunication("Agression", register: true);

		public static readonly LizardCommunication Submission = new LizardCommunication("Submission", register: true);

		public LizardCommunication(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class LizardTrackState : RelationshipTracker.TrackedCreatureState
	{
		public bool spear;

		public int vultureMask;
	}

	private class LizardInjuryTracker : AIModule
	{
		private Lizard lizard;

		public LizardInjuryTracker(ArtificialIntelligence AI, Lizard lizard)
			: base(AI)
		{
			this.lizard = lizard;
		}

		public override float Utility()
		{
			if (ModManager.MMF && MMF.cfgNoArenaFleeing.Value && AI.creature.world.game.IsArenaSession)
			{
				return 0f;
			}
			return Custom.SCurve(Mathf.InverseLerp(0.2f, 0.9f, 1f - Mathf.Clamp(lizard.LizardState.health, 0f, 1f)), 0.01f);
		}
	}

	public class LurkTracker : AIModule
	{
		private Lizard lizard;

		public WorldCoordinate lurkPosition;

		public IntVector2 lookPosition;

		public int bestVisLook;

		public LurkTracker(ArtificialIntelligence AI, Lizard lizard)
			: base(AI)
		{
			this.lizard = lizard;
			lurkPosition = lizard.abstractCreature.pos;
		}

		public override void Update()
		{
			if (lizard.room.game.world.GetAbstractRoom(lurkPosition).realizedRoom != null && lizard.room.game.world.GetAbstractRoom(lurkPosition).realizedRoom.readyForAI)
			{
				IntVector2 intVector = new IntVector2(UnityEngine.Random.Range(0, lizard.room.game.world.GetAbstractRoom(lurkPosition).realizedRoom.TileWidth), UnityEngine.Random.Range(0, lizard.room.game.world.GetAbstractRoom(lurkPosition).realizedRoom.TileHeight));
				if (lizard.room.game.world.GetAbstractRoom(lurkPosition).realizedRoom.aimap.getAItile(intVector).visibility > bestVisLook && lizard.room.game.world.GetAbstractRoom(lurkPosition).realizedRoom.VisualContact(lurkPosition, lizard.room.game.world.GetAbstractRoom(lurkPosition).realizedRoom.ToWorldCoordinate(intVector)))
				{
					lookPosition = intVector;
					bestVisLook = lizard.room.game.world.GetAbstractRoom(lurkPosition).realizedRoom.aimap.getAItile(intVector).visibility;
				}
			}
			if (Custom.ManhattanDistance(lizard.abstractCreature.pos, lurkPosition) > 10 || !AI.pathFinder.CoordinateReachable(lurkPosition) || !AI.pathFinder.CoordinatePossibleToGetBackFrom(lurkPosition))
			{
				WorldCoordinate worldCoordinate = lizard.room.GetWorldCoordinate(new IntVector2(lizard.abstractCreature.pos.Tile.x + UnityEngine.Random.Range(-15, 16), lizard.abstractCreature.pos.Tile.y + UnityEngine.Random.Range(-15, 16)));
				if (LurkPosScore(worldCoordinate) > LurkPosScore(lurkPosition))
				{
					lurkPosition = worldCoordinate;
					lookPosition = worldCoordinate.Tile;
					bestVisLook = 0;
				}
			}
			if (UnityEngine.Random.value < 0.00083333335f)
			{
				lurkPosition = lizard.room.GetWorldCoordinate(new IntVector2(UnityEngine.Random.Range(0, lizard.room.TileWidth), UnityEngine.Random.Range(0, lizard.room.TileHeight)));
			}
		}

		public float LurkPosScore(WorldCoordinate testLurkPos)
		{
			if (!lizard.room.aimap.TileAccessibleToCreature(testLurkPos.Tile, lizard.Template))
			{
				return -100000f;
			}
			if (lizard.room.GetTile(testLurkPos).Terrain == Room.Tile.TerrainType.Slope)
			{
				return -100000f;
			}
			if (testLurkPos.room != lizard.abstractCreature.pos.room)
			{
				return -100000f;
			}
			if (!AI.pathFinder.CoordinateReachable(testLurkPos) || !AI.pathFinder.CoordinatePossibleToGetBackFrom(testLurkPos))
			{
				return -100000f;
			}
			float num = 0f;
			if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
			{
				num = Mathf.Clamp(lizard.room.aimap.getAItile(testLurkPos).floorAltitude, 1f, 5f) / (float)lizard.room.aimap.getTerrainProximity(testLurkPos);
			}
			else if (lizard.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
			{
				if (lizard.room.aimap.getTerrainProximity(testLurkPos) > 2)
				{
					return -100000f;
				}
				if (lizard.room.aimap.getAItile(testLurkPos).acc != AItile.Accessibility.Floor && !lizard.room.GetTile(testLurkPos).DeepWater)
				{
					return -100000f;
				}
				num = 40f / Mathf.Max(1f, Mathf.Abs(lizard.room.defaultWaterLevel - testLurkPos.y) - 5);
				num = ((testLurkPos.y >= lizard.room.defaultWaterLevel) ? (num / 5f) : (num * 5f));
				if (testLurkPos.y < lizard.room.defaultWaterLevel - 10)
				{
					num -= Custom.LerpMap(testLurkPos.y, lizard.room.defaultWaterLevel - 10, lizard.room.defaultWaterLevel - 40, 0f, 100f);
				}
				if (lizard.room.aimap.getAItile(testLurkPos).acc == AItile.Accessibility.Floor)
				{
					num += 20f;
				}
			}
			int visibility = lizard.room.aimap.getAItile(testLurkPos).visibility;
			num -= (float)visibility / 1000f;
			for (int i = 0; i < 8; i++)
			{
				if (lizard.room.VisualContact(testLurkPos.Tile, testLurkPos.Tile + Custom.eightDirections[i] * 10))
				{
					num += (float)lizard.room.aimap.getAItile(testLurkPos.Tile + Custom.eightDirections[i] * 10).visibility / 8000f;
				}
			}
			if (lizard.room.aimap.getAItile(testLurkPos).narrowSpace)
			{
				num -= 10000f;
			}
			for (int j = 0; j < AI.tracker.CreaturesCount; j++)
			{
				if (AI.tracker.GetRep(j).BestGuessForPosition().room == testLurkPos.room && !AI.tracker.GetRep(j).representedCreature.creatureTemplate.smallCreature && AI.tracker.GetRep(j).dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Eats && AI.tracker.GetRep(j).BestGuessForPosition().Tile.FloatDist(testLurkPos.Tile) < 20f && AI.tracker.GetRep(j).representedCreature.creatureTemplate.bodySize >= lizard.Template.bodySize * 0.8f)
				{
					num += AI.tracker.GetRep(j).BestGuessForPosition().Tile.FloatDist(testLurkPos.Tile) / 10f;
				}
			}
			return num;
		}

		public override float Utility()
		{
			if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
			{
				return 0.5f;
			}
			if (lizard.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
			{
				if (LurkPosScore(lurkPosition) > 0f)
				{
					if (!lizard.room.GetTile(lurkPosition).AnyWater)
					{
						return 0.2f;
					}
					return 0.5f;
				}
				return 0f;
			}
			return 0f;
		}
	}

	public class LizardSpitTracker : AIModule
	{
		public bool spitting;

		public bool wantToSpit;

		public bool targetReachable;

		public WorldCoordinate wantToSpitAtPos;

		public WorldCoordinate tempSpitFromPos;

		public WorldCoordinate spitFromPos;

		private int randomCycle;

		public int delay;

		private int dontWannaSpitCounter;

		private int wannaSpitCounter;

		public int goToSpitPosCounter;

		public float aimUp;

		private float generalAimUp = 0.5f;

		public BodyChunk aimForChunk;

		public LizardAI lizardAI => AI as LizardAI;

		public bool AtSpitPos => Custom.DistLess(lizardAI.lizard.mainBodyChunk.pos, lizardAI.lizard.room.MiddleOfTile(spitFromPos), 50f);

		public LizardSpitTracker(ArtificialIntelligence AI)
			: base(AI)
		{
			randomCycle = UnityEngine.Random.Range(100, 800);
		}

		public override void Update()
		{
			if (!targetReachable && wantToSpit && !AtSpitPos)
			{
				goToSpitPosCounter = 200;
			}
			else if (goToSpitPosCounter > 0)
			{
				goToSpitPosCounter--;
			}
			base.Update();
			randomCycle--;
			if (randomCycle < -100)
			{
				randomCycle = UnityEngine.Random.Range(100, 800);
			}
			if (delay > 0)
			{
				delay--;
			}
			if (lizardAI.lizard.safariControlled)
			{
				spitFromPos = AI.creature.pos;
			}
			spitting = false;
			wantToSpit = false;
			if (lizardAI.lizard.safariControlled && lizardAI.lizard.inputWithDiagonals.HasValue && lizardAI.lizard.inputWithDiagonals.Value.thrw && !lizardAI.lizard.lastInputWithDiagonals.Value.thrw)
			{
				spitting = true;
				wantToSpit = true;
				if (lizardAI.preyTracker.MostAttractivePrey != null)
				{
					wantToSpitAtPos = lizardAI.preyTracker.MostAttractivePrey.BestGuessForPosition();
				}
				else
				{
					WorldCoordinate pos = lizardAI.creature.pos;
					Vector2 vector = Custom.RNV() * 10f;
					wantToSpitAtPos = new WorldCoordinate(pos.room, pos.x + (int)vector.x, pos.y + (int)vector.y, pos.abstractNode);
				}
			}
			if (lizardAI.behavior == Behavior.Flee || lizardAI.behavior == Behavior.Travelling || lizardAI.behavior == Behavior.EscapeRain)
			{
				return;
			}
			Tracker.CreatureRepresentation mostAttractivePrey = lizardAI.preyTracker.MostAttractivePrey;
			if (mostAttractivePrey == null || mostAttractivePrey.BestGuessForPosition().Tile.FloatDist(AI.creature.pos.Tile) < 6f)
			{
				return;
			}
			if (AtSpitPos)
			{
				if (lizardAI.behavior == Behavior.GoToSpitPos && !lizardAI.lizard.safariControlled)
				{
					spitting = true;
				}
				if (!mostAttractivePrey.VisualContact && mostAttractivePrey.representedCreature.realizedCreature != null && mostAttractivePrey.representedCreature.realizedCreature.room == lizardAI.lizard.room && lizardAI.lizard.room.VisualContact(lizardAI.lizard.mainBodyChunk.pos, mostAttractivePrey.representedCreature.realizedCreature.RandomChunk.pos))
				{
					lizardAI.tracker.SeeCreature(mostAttractivePrey.representedCreature);
				}
			}
			if (randomCycle < 0 && !lizardAI.lizard.safariControlled)
			{
				wantToSpit = true;
			}
			targetReachable = false;
			int num = 1;
			while (!targetReachable && num < 3)
			{
				int num2 = 0;
				while (!targetReachable && num2 < 9)
				{
					targetReachable = AI.pathFinder.CoordinateReachableAndGetbackable(mostAttractivePrey.BestGuessForPosition() + Custom.eightDirectionsAndZero[num2] * num);
					num2++;
				}
				num++;
			}
			if (!wantToSpit)
			{
				wantToSpit = !targetReachable;
			}
			if (lizardAI.lizard.safariControlled)
			{
				wantToSpit = false;
			}
			if (targetReachable)
			{
				goToSpitPosCounter = 0;
			}
			if ((wantToSpit && randomCycle >= 0) || (lizardAI.lizard.room.aimap.getAItile(mostAttractivePrey.BestGuessForPosition()).floorAltitude > 4 && AI.creature.pos.Tile.FloatDist(mostAttractivePrey.BestGuessForPosition().Tile) > 20f - (float)lizardAI.lizard.room.aimap.getAItile(mostAttractivePrey.BestGuessForPosition()).floorAltitude))
			{
				wantToSpit = false;
				if (lizardAI.lizard.room.aimap.getAItile(mostAttractivePrey.BestGuessForPosition()).acc >= AItile.Accessibility.Climb && mostAttractivePrey.representedCreature.realizedCreature != null)
				{
					float num3 = Mathf.Sign(mostAttractivePrey.representedCreature.realizedCreature.DangerPos.x - lizardAI.lizard.mainBodyChunk.pos.x);
					int num4 = 0;
					while (!wantToSpit && num4 < 2)
					{
						Vector2 dangerPos = mostAttractivePrey.representedCreature.realizedCreature.DangerPos;
						for (int i = 0; i < 40; i++)
						{
							dangerPos.y -= 20f;
							if (num4 == 1)
							{
								dangerPos.x += num3 * 5f;
							}
							if (lizardAI.lizard.room.aimap.getAItile(dangerPos).acc == AItile.Accessibility.Floor && lizardAI.pathFinder.CoordinateReachableAndGetbackable(lizardAI.lizard.room.GetWorldCoordinate(dangerPos)))
							{
								wantToSpit = true;
								break;
							}
						}
						num4++;
					}
				}
			}
			if (!wantToSpit)
			{
				dontWannaSpitCounter++;
				wannaSpitCounter = Custom.IntClamp(wannaSpitCounter - 1, 0, 200);
				if (dontWannaSpitCounter > 100)
				{
					spitFromPos = AI.creature.pos;
				}
				return;
			}
			dontWannaSpitCounter = 0;
			wannaSpitCounter = Custom.IntClamp(wannaSpitCounter + 1, 0, 200);
			wantToSpitAtPos = mostAttractivePrey.BestGuessForPosition();
			for (int j = 0; j < 10; j++)
			{
				WorldCoordinate testPos = new WorldCoordinate(wantToSpitAtPos.room, UnityEngine.Random.Range(0, lizardAI.lizard.room.TileWidth), UnityEngine.Random.Range(0, lizardAI.lizard.room.TileHeight), -1);
				if (UnityEngine.Random.value < 0.5f)
				{
					testPos.Tile = lizardAI.lizard.room.GetTilePosition(lizardAI.lizard.room.MiddleOfTile(spitFromPos) + Custom.RNV() * 200f * UnityEngine.Random.value);
				}
				if (SpitFromPosScore(testPos) > SpitFromPosScore(tempSpitFromPos))
				{
					tempSpitFromPos = testPos;
				}
			}
			if (SpitFromPosScore(tempSpitFromPos) > SpitFromPosScore(spitFromPos) + 10f)
			{
				spitFromPos = tempSpitFromPos;
			}
			if (mostAttractivePrey.representedCreature.realizedCreature != null && mostAttractivePrey.representedCreature.realizedCreature.room == lizardAI.lizard.room && (wannaSpitCounter <= 100 || AtSpitPos) && (wannaSpitCounter > 100 || mostAttractivePrey.VisualContact))
			{
				if (!lizardAI.lizard.safariControlled)
				{
					spitting = true;
				}
				generalAimUp = Mathf.Lerp(generalAimUp, Mathf.Lerp(UnityEngine.Random.value, 0.5f, UnityEngine.Random.value), 0.003f);
			}
		}

		public Vector2? AimPos()
		{
			Tracker.CreatureRepresentation mostAttractivePrey = lizardAI.preyTracker.MostAttractivePrey;
			if (lizardAI.lizard.safariControlled)
			{
				if (lizardAI.lizard.inputWithDiagonals.HasValue && (lizardAI.lizard.inputWithDiagonals.Value.x != 0 || lizardAI.lizard.inputWithDiagonals.Value.y != 0))
				{
					return lizardAI.lizard.mainBodyChunk.pos + new Vector2(lizardAI.lizard.inputWithDiagonals.Value.x, lizardAI.lizard.inputWithDiagonals.Value.y) * 100f;
				}
				if (mostAttractivePrey == null || !mostAttractivePrey.VisualContact)
				{
					return lizardAI.lizard.mainBodyChunk.pos + Custom.RNV() * 100f;
				}
			}
			if (mostAttractivePrey == null || !mostAttractivePrey.VisualContact)
			{
				return null;
			}
			Vector2 vector;
			Vector2 vector2;
			if (mostAttractivePrey.representedCreature.realizedCreature == null)
			{
				vector = lizardAI.lizard.room.MiddleOfTile(mostAttractivePrey.BestGuessForPosition());
				vector2 = default(Vector2);
			}
			else
			{
				int num = UnityEngine.Random.Range(0, mostAttractivePrey.representedCreature.realizedCreature.bodyChunks.Length);
				vector = mostAttractivePrey.representedCreature.realizedCreature.bodyChunks[num].pos;
				vector2 = mostAttractivePrey.representedCreature.realizedCreature.bodyChunks[num].pos - mostAttractivePrey.representedCreature.realizedCreature.bodyChunks[num].lastPos;
				aimForChunk = mostAttractivePrey.representedCreature.realizedCreature.bodyChunks[num];
			}
			aimUp = generalAimUp + Mathf.Lerp(-0.15f, 0.15f, UnityEngine.Random.value);
			vector.y += Mathf.Pow(Vector2.Distance(vector, lizardAI.lizard.mainBodyChunk.pos), Mathf.Lerp(1.2f, 1.55f, aimUp)) * 0.015f * aimUp;
			return vector + vector2 * Vector2.Distance(vector, lizardAI.lizard.mainBodyChunk.pos) * 0.01f * UnityEngine.Random.value;
		}

		public void AimABitUp(float am)
		{
			if (am < generalAimUp)
			{
				generalAimUp = Mathf.Clamp(generalAimUp + 0.3f * Mathf.InverseLerp(1f, 0.5f, generalAimUp) * UnityEngine.Random.value, am, 1f);
			}
		}

		public void AimABitDown(float am)
		{
			if (am > generalAimUp)
			{
				generalAimUp = Mathf.Clamp(generalAimUp - 0.3f * Mathf.InverseLerp(0f, 0.5f, generalAimUp) * UnityEngine.Random.value, 0f, am);
			}
		}

		public void SetAim(float am)
		{
			generalAimUp = am;
		}

		private float SpitFromPosScore(WorldCoordinate testPos)
		{
			if (!AI.pathFinder.CoordinateReachableAndGetbackable(testPos))
			{
				return float.MinValue;
			}
			float num = 0f - testPos.Tile.FloatDist(wantToSpitAtPos.Tile);
			if (lizardAI.lizard.room.VisualContact(testPos.Tile, wantToSpitAtPos.Tile))
			{
				num += 1000f;
			}
			return num;
		}
	}

	public float excitement;

	public int panic;

	public float hunger;

	public float fear;

	public float rainFear;

	public int lastDistressLength;

	public float runSpeed;

	public float currentUtility;

	public int idleCounter;

	public int idleSpotWinStreak;

	private WorldCoordinate forbiddenIdleSpot = new WorldCoordinate(-1, -1, -1, -1);

	public int idleRestlessness;

	public int unableToFindComfortablePosition;

	public Tracker.CreatureRepresentation focusCreature;

	private new int timeInRoom;

	public Tracker.CreatureRepresentation casualAggressionTarget;

	public AbstractCreature submittedTo;

	public int attemptBiteFrames;

	public YellowAI yellowAI;

	public LizardSpitTracker redSpitAI;

	private DebugSprite dbspr;

	private DebugSprite dbspr2;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public LurkTracker lurkTracker;

	private int usedToVultureMask;

	public Behavior behavior;

	private Lizard lizard => creature.realizedCreature as Lizard;

	public float CombinedFear => Mathf.Max(fear, Mathf.Pow(rainFear, 3f));

	private float TravelUtility
	{
		get
		{
			if (!creature.abstractAI.WantToMigrate)
			{
				return 0f;
			}
			if (!base.pathFinder.CoordinateReachable(creature.abstractAI.MigrationDestination))
			{
				return 0f;
			}
			float num = lizard.room.abstractRoom.AttractionValueForCreature(lizard.Template.type);
			float num2 = lizard.room.world.GetAbstractRoom(creature.abstractAI.MigrationDestination).AttractionValueForCreature(lizard.Template.type);
			if (num > num2)
			{
				return 0f;
			}
			float num3 = Mathf.Lerp(0.99f, 0.1f, Mathf.Pow(num, 0.25f));
			if (num == num2)
			{
				return num3 *= 0.5f;
			}
			return num3;
		}
	}

	private float RoomLike
	{
		get
		{
			if (base.friendTracker.friend != null && base.friendTracker.friendDest.room == lizard.room.abstractRoom.index)
			{
				return 1f;
			}
			float num = lizard.room.abstractRoom.SizeDependentAttractionValueForCreature(lizard.Template.type);
			for (int i = 0; i < lizard.room.abstractRoom.creatures.Count; i++)
			{
				if (lizard.room.abstractRoom.creatures[i].creatureTemplate.IsLizard && lizard.room.abstractRoom.creatures[i] != creature)
				{
					num *= 0.9f;
				}
			}
			return num;
		}
	}

	public LizardAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		AddModule(new LizardPather(this, world, creature));
		if (creature.creatureTemplate.type == CreatureTemplate.Type.RedLizard || (ModManager.MSC && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard))
		{
			base.pathFinder.stepsPerFrame = 30;
		}
		AddModule(new Tracker(this, 10, 10, -1, 0.35f, 5, 5, 10));
		AddModule(new NoiseTracker(this, base.tracker));
		if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
		{
			AddModule(new SuperHearing(this, base.tracker, 350f));
		}
		AddModule(new DenFinder(this, creature));
		AddModule(new RainTracker(this));
		AddModule(new PreyTracker(this, 5, 2f, 3f, 70f, 0.5f));
		if (lizard.Template.type == CreatureTemplate.Type.RedLizard)
		{
			base.preyTracker.giveUpOnUnreachablePrey = 1800;
		}
		AddModule(new ThreatTracker(this, 3));
		AddModule(new AgressionTracker(this, 0.001f, 0.001f));
		AddModule(new UtilityComparer(this));
		AddModule(new MissionTracker(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: true));
		base.stuckTracker.totalTrackedLastPositions = 40;
		base.stuckTracker.checkPastPositionsFrom = 30;
		base.stuckTracker.pastPosStuckDistance = 5;
		base.stuckTracker.pastStuckPositionsCloseToIncrementStuckCounter = 10;
		base.stuckTracker.AddSubModule(new StuckTracker.MoveBacklog(base.stuckTracker));
		AddModule(new FriendTracker(this));
		base.friendTracker.tamingDifficlty = lizard.lizardParams.tamingDifficulty * Mathf.Lerp(0.9f, 1.1f, creature.personality.dominance);
		FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.0025f));
		base.utilityComparer.AddComparedModule(base.threatTracker, smoother, 1f, 1f);
		smoother = new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.15f);
		base.utilityComparer.AddComparedModule(base.preyTracker, smoother, 0.6f, 1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 0.9f, 1f);
		smoother = new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.15f);
		base.utilityComparer.AddComparedModule(new LizardInjuryTracker(this, lizard), null, (creature.creatureTemplate.type == CreatureTemplate.Type.RedLizard) ? 0.4f : 0.9f, 1f);
		base.utilityComparer.AddComparedModule(base.agressionTracker, null, 0.5f, 1.2f);
		base.utilityComparer.AddComparedModule(base.missionTracker, null, 0.8f, 1f);
		base.utilityComparer.AddComparedModule(base.noiseTracker, null, 0.2f, 1.2f);
		base.utilityComparer.AddComparedModule(base.friendTracker, null, 0.9f, 1.2f);
		if (creature.creatureTemplate.type == CreatureTemplate.Type.WhiteLizard || creature.creatureTemplate.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
		{
			lurkTracker = new LurkTracker(this, lizard);
			AddModule(lurkTracker);
			base.utilityComparer.AddComparedModule(lurkTracker, null, Mathf.Lerp(0.4f, 0.3f, creature.personality.energy), 1f);
		}
		else if (creature.creatureTemplate.type == CreatureTemplate.Type.YellowLizard)
		{
			yellowAI = new YellowAI(this);
			AddModule(yellowAI);
		}
		else if (lizard.Template.type == CreatureTemplate.Type.RedLizard || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard))
		{
			redSpitAI = new LizardSpitTracker(this);
			AddModule(redSpitAI);
		}
		behavior = Behavior.Idle;
		lizard.AI = this;
		panic = 0;
		excitement = 0.5f;
	}

	public override void NewRoom(Room room)
	{
		attemptBiteFrames = 0;
		timeInRoom = 0;
		base.NewRoom(room);
	}

	private Behavior DetermineBehavior()
	{
		Behavior behavior = Behavior.Idle;
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			if (aIModule is ThreatTracker)
			{
				behavior = Behavior.Flee;
			}
			else if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
			else if (aIModule is PreyTracker)
			{
				behavior = Behavior.Hunt;
			}
			else if (aIModule is LizardInjuryTracker)
			{
				behavior = Behavior.Injured;
			}
			else if (aIModule is AgressionTracker)
			{
				behavior = Behavior.Fighting;
			}
			else if (aIModule is MissionTracker)
			{
				behavior = Behavior.ActingOutMission;
			}
			else if (aIModule is LurkTracker)
			{
				behavior = Behavior.Lurk;
			}
			else if (aIModule is NoiseTracker)
			{
				behavior = Behavior.InvestigateSound;
			}
			else if (aIModule is FriendTracker)
			{
				behavior = Behavior.FollowFriend;
			}
		}
		if (currentUtility < 0.05f)
		{
			currentUtility = 0.05f;
			behavior = Behavior.Idle;
		}
		if (behavior != Behavior.Flee && lizard.grasps[0] != null && lizard.grasps[0].grabbed is Creature && DynamicRelationship((lizard.grasps[0].grabbed as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
		{
			behavior = Behavior.ReturnPrey;
			currentUtility = 1f;
		}
		if (behavior == Behavior.Hunt && base.preyTracker.MostAttractivePrey != null && base.preyTracker.frustration > 0.75f && lizard.Template.type != CreatureTemplate.Type.RedLizard && ((base.preyTracker.MostAttractivePrey.EstimatedChanceOfFinding > 0.4f && Custom.DistLess(lizard.mainBodyChunk.pos, lizard.room.MiddleOfTile(base.preyTracker.MostAttractivePrey.BestGuessForPosition()), 350f)) || creature.pos.room != base.preyTracker.MostAttractivePrey.BestGuessForPosition().room))
		{
			behavior = Behavior.Frustrated;
		}
		if (stranded && (behavior == Behavior.ReturnPrey || behavior == Behavior.EscapeRain))
		{
			behavior = Behavior.Frustrated;
		}
		if (behavior != Behavior.ReturnPrey && behavior != Behavior.Injured && behavior != Behavior.EscapeRain && behavior != Behavior.Flee && behavior != Behavior.ActingOutMission && currentUtility < TravelUtility)
		{
			currentUtility = TravelUtility;
			behavior = Behavior.Travelling;
		}
		if (redSpitAI != null && redSpitAI.goToSpitPosCounter > 0 && redSpitAI.spitFromPos.room == creature.pos.room)
		{
			behavior = Behavior.GoToSpitPos;
		}
		return behavior;
	}

	public override void Update()
	{
		if (behavior == Behavior.Flee && !RainWorldGame.RequestHeavyAi(lizard))
		{
			return;
		}
		if (ModManager.MSC && lizard.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(lizard.LickedByPlayer.abstractCreature);
		}
		if (panic > 0)
		{
			panic--;
		}
		timeInRoom++;
		if (UnityEngine.Random.value < 1f / ((lizard.Template.type == CreatureTemplate.Type.RedLizard) ? 50f : Custom.LerpMap(RoomLike, 0.35f, 1f, 10f, 1000f)))
		{
			creature.abstractAI.AbstractBehavior(1);
		}
		creature.state.socialMemory.EvenOutAllTemps(0.0005f);
		base.pathFinder.walkPastPointOfNoReturn = stranded || !base.denFinder.GetDenPosition().HasValue || !base.pathFinder.CoordinatePossibleToGetBackFrom(base.denFinder.GetDenPosition().Value);
		fear = base.utilityComparer.GetSmoothedNonWeightedUtility(base.threatTracker);
		hunger = base.utilityComparer.GetSmoothedNonWeightedUtility(base.preyTracker);
		rainFear = base.utilityComparer.GetSmoothedNonWeightedUtility(base.rainTracker);
		excitement = Mathf.Lerp(excitement, Mathf.Max(hunger, CombinedFear), 0.1f);
		if (fear > 0.8f)
		{
			lastDistressLength = Custom.IntClamp(lastDistressLength + 1, 0, 500);
		}
		else if (fear < 0.2f)
		{
			lastDistressLength = 0;
		}
		((base.utilityComparer.GetUtilityTracker(base.threatTracker).smoother as FloatTweener.FloatTweenUpAndDown).down as FloatTweener.FloatTweenBasic).speed = 1f / ((float)(lastDistressLength + 20) * 3f);
		base.utilityComparer.GetUtilityTracker(base.agressionTracker).weight = (creature.world.game.IsStorySession ? 0.25f : 0.125f) * lizard.LizardState.health;
		base.utilityComparer.GetUtilityTracker(base.rainTracker).weight = (base.friendTracker.CareAboutRain() ? 0.9f : 0.1f);
		behavior = DetermineBehavior();
		lizard.JawOpen -= 0.01f;
		if (lurkTracker != null && behavior == Behavior.Hunt && base.preyTracker.MostAttractivePrey != null && runSpeed < 0.1f && lizard.abstractCreature.pos.Tile.FloatDist(base.preyTracker.MostAttractivePrey.BestGuessForPosition().Tile) > 17f)
		{
			behavior = Behavior.Lurk;
		}
		if (behavior == Behavior.Flee)
		{
			WorldCoordinate destination = base.threatTracker.FleeTo(creature.pos, 5, 20, fear > 1f / 3f);
			creature.abstractAI.SetDestination(destination);
			if (base.threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: true) > 1f)
			{
				if (panic < 40)
				{
					panic = 40;
				}
				else
				{
					panic += 2;
				}
				if (UnityEngine.Random.value < 0.05f && base.threatTracker.mostThreateningCreature != null && base.threatTracker.mostThreateningCreature.BestGuessForPosition().Tile.FloatDist(creature.pos.Tile) < 15f)
				{
					lizard.EnterAnimation(Lizard.Animation.ThreatSpotted, forceAnimationChange: false);
				}
			}
			runSpeed = Mathf.Lerp(runSpeed, Mathf.Pow(CombinedFear, 0.1f), 0.5f);
			focusCreature = base.threatTracker.mostThreateningCreature;
			if (lizard.graphicsModule != null)
			{
				(lizard.graphicsModule as LizardGraphics).lookPos = lizard.room.MiddleOfTile(base.pathFinder.GetDestination);
			}
		}
		else if (behavior == Behavior.Travelling)
		{
			runSpeed = Mathf.Lerp(runSpeed, 0.5f + 0.5f * TravelUtility, 0.01f);
			creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
		}
		else if (behavior == Behavior.Hunt)
		{
			Tracker.CreatureRepresentation mostAttractivePrey = base.preyTracker.MostAttractivePrey;
			if (mostAttractivePrey != null)
			{
				AggressiveBehavior(mostAttractivePrey, 1f);
			}
			runSpeed = Mathf.Lerp(runSpeed, Mathf.Max(hunger, 0.3f), 0.1f);
			if (UnityEngine.Random.value < 0.0125f && !lizard.safariControlled && creature.creatureTemplate.type != CreatureTemplate.Type.BlackLizard)
			{
				lizard.voice.MakeSound(LizardVoice.Emotion.BloodLust);
			}
		}
		else if (behavior == Behavior.Fighting)
		{
			focusCreature = base.agressionTracker.AgressionTarget();
			if (focusCreature != null)
			{
				if (focusCreature.VisualContact && Custom.DistLess(lizard.mainBodyChunk.pos, focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, 160f) && (!(focusCreature.representedCreature.realizedCreature is Lizard) || Vector2.Distance(lizard.mainBodyChunk.pos, focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos) < Vector2.Distance(lizard.mainBodyChunk.pos, focusCreature.representedCreature.realizedCreature.bodyChunks[2].pos)))
				{
					if (UnityEngine.Random.value < 0.05f)
					{
						lizard.EnterAnimation(Lizard.Animation.FightingStance, forceAnimationChange: false);
					}
					runSpeed = Mathf.Lerp(runSpeed, 0.2f, 0.15f);
				}
				else
				{
					runSpeed = Mathf.Lerp(runSpeed, 0.8f, 0.15f);
					if (UnityEngine.Random.value < 1f / 180f)
					{
						lizard.EnterAnimation(Lizard.Animation.FightingStance, forceAnimationChange: false);
					}
				}
				if (focusCreature.representedCreature.realizedCreature is Lizard && UnityEngine.Random.value < 0.05f && UnityEngine.Random.value < DynamicRelationship(focusCreature).intensity)
				{
					SendCommunication(focusCreature.representedCreature.realizedCreature as Lizard, LizardCommunication.Agression);
				}
				AggressiveBehavior(focusCreature, 0.025f);
			}
		}
		else if (behavior == Behavior.Idle)
		{
			bool flag = true;
			int num = idleCounter;
			idleCounter--;
			if (creature.pos.room == base.pathFinder.GetDestination.room)
			{
				if (!base.pathFinder.GetDestination.TileDefined)
				{
					Custom.Log("lizard idle spot in den. Setting to critter pos");
					creature.abstractAI.SetDestination(creature.pos);
					idleCounter = 0;
				}
				else if (Custom.ManhattanDistance(creature.pos.Tile, base.pathFinder.GetDestination.Tile) < 5)
				{
					idleCounter -= lizard.lizardParams.idleCounterSubtractWhenCloseToIdlePos;
					idleRestlessness--;
					flag = idleCounter < 1 || idleRestlessness > 0;
					if (!flag && !ComfortableIdlePosition())
					{
						unableToFindComfortablePosition++;
						flag = true;
					}
					else if (lizard.swim < 0.5f && (lizard.bodyChunks[0].vel.magnitude > 2f || lizard.bodyChunks[1].vel.magnitude > 2f || lizard.bodyChunks[2].vel.magnitude > 2f))
					{
						unableToFindComfortablePosition++;
					}
					else
					{
						unableToFindComfortablePosition = 0;
					}
					if (unableToFindComfortablePosition > 40)
					{
						idleCounter = 0;
						unableToFindComfortablePosition = 0;
					}
					else if (lizard.IsTileSolid(0, 0, -1) && lizard.IsTileSolid(1, 0, -1) && lizard.IsTileSolid(2, 0, -1))
					{
						for (int i = 0; i < 3; i++)
						{
							lizard.bodyChunks[i].vel.y -= 0.1f;
						}
					}
					if (idleRestlessness < 40 && UnityEngine.Random.value < 0.0016666667f)
					{
						idleRestlessness = UnityEngine.Random.Range(1, 40);
					}
				}
			}
			if (IdleSpotScore(creature.abstractAI.destination) == float.MaxValue)
			{
				idleCounter = 0;
			}
			if (idleCounter < 1)
			{
				if (num > 0)
				{
					forbiddenIdleSpot = base.pathFinder.GetDestination;
				}
				WorldCoordinate worldCoordinate = new WorldCoordinate(creature.Room.index, UnityEngine.Random.Range(0, lizard.room.TileWidth), UnityEngine.Random.Range(0, lizard.room.TileHeight), -1);
				if (IdleSpotScore(worldCoordinate) < IdleSpotScore(creature.abstractAI.destination))
				{
					creature.abstractAI.SetDestination(worldCoordinate);
					idleSpotWinStreak = 0;
				}
				else
				{
					idleSpotWinStreak++;
					if (idleSpotWinStreak > 50)
					{
						idleCounter = UnityEngine.Random.Range(1000, 4000);
						idleSpotWinStreak = 0;
					}
				}
			}
			if (flag)
			{
				if (!forbiddenIdleSpot.CompareDisregardingNode(creature.abstractAI.destination))
				{
					runSpeed = Mathf.Lerp(runSpeed, 0.25f, 0.5f);
				}
			}
			else
			{
				runSpeed = Mathf.Lerp(runSpeed, 0f, 0.5f);
			}
			if (UnityEngine.Random.value < 0.00125f && !lizard.safariControlled)
			{
				lizard.voice.MakeSound(LizardVoice.Emotion.Boredom);
			}
			focusCreature = null;
		}
		else if (behavior == Behavior.EscapeRain)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			runSpeed = Mathf.Lerp(runSpeed, 1f, 0.1f);
		}
		else if (behavior == Behavior.ReturnPrey)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			runSpeed = Mathf.Lerp(runSpeed, Mathf.Pow(Mathf.Max(CombinedFear, 0.5f), 0.5f), 0.1f);
		}
		else if (behavior == Behavior.Injured)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			runSpeed = Mathf.Lerp(runSpeed, 1f, 0.01f);
			if (UnityEngine.Random.value < 0.0125f)
			{
				lizard.voice.MakeSound(LizardVoice.Emotion.PainIdle, 1f - (lizard.State as LizardState).health);
			}
		}
		else if (behavior == Behavior.Frustrated)
		{
			focusCreature = base.preyTracker.MostAttractivePrey;
			runSpeed = Mathf.Lerp(runSpeed, 1f, 0.5f);
			if (UnityEngine.Random.value < 0.1f || (Custom.ManhattanDistance(creature.pos.Tile, creature.abstractAI.destination.Tile) < 10 && creature.pos.room == creature.abstractAI.destination.room))
			{
				WorldCoordinate worldCoordinate2 = new WorldCoordinate(creature.Room.index, UnityEngine.Random.Range(0, lizard.room.TileWidth), UnityEngine.Random.Range(0, lizard.room.TileHeight), -1);
				if (base.pathFinder.CoordinateReachable(worldCoordinate2) && base.pathFinder.CoordinatePossibleToGetBackFrom(worldCoordinate2))
				{
					creature.abstractAI.SetDestination(worldCoordinate2);
					idleRestlessness = UnityEngine.Random.Range(30, 100);
				}
			}
			if (UnityEngine.Random.value < 0.0125f)
			{
				lizard.EnterAnimation(Lizard.Animation.PreyReSpotted, forceAnimationChange: false);
			}
			if (UnityEngine.Random.value < 0.0125f)
			{
				lizard.bodyWiggleCounter = Math.Max(lizard.bodyWiggleCounter, (int)(UnityEngine.Random.value * 100f));
			}
			if (UnityEngine.Random.value < 0.0125f && !lizard.safariControlled && creature.creatureTemplate.type != CreatureTemplate.Type.BlackLizard)
			{
				lizard.voice.MakeSound(LizardVoice.Emotion.Frustration);
			}
		}
		else if (behavior == Behavior.ActingOutMission)
		{
			base.missionTracker.ActOnMostImportantMission();
			runSpeed = Mathf.Lerp(runSpeed, 0.6f, 0.05f);
		}
		else if (behavior == Behavior.Lurk)
		{
			if (Custom.ManhattanDistance(creature.pos, creature.abstractAI.destination) > 5 && lurkTracker.LurkPosScore(creature.pos) * 1.2f < lurkTracker.LurkPosScore(lurkTracker.lurkPosition))
			{
				runSpeed = Mathf.Lerp(runSpeed, 0.5f, 0.5f);
				creature.abstractAI.SetDestination(lurkTracker.lurkPosition);
			}
			else if (VisualContact(lizard.room.MiddleOfTile(lurkTracker.lookPosition), float.MaxValue))
			{
				creature.abstractAI.SetDestination(lurkTracker.lurkPosition);
				runSpeed = Mathf.Lerp(runSpeed, 0f, 0.5f);
			}
			else
			{
				creature.abstractAI.SetDestination(Custom.MakeWorldCoordinate(lurkTracker.lurkPosition.Tile + Custom.fourDirections[UnityEngine.Random.Range(0, 4)], lurkTracker.lurkPosition.room));
				runSpeed = Mathf.Lerp(runSpeed, 0.7f, 0.2f);
			}
		}
		else if (behavior == Behavior.InvestigateSound)
		{
			creature.abstractAI.SetDestination(base.noiseTracker.ExaminePos);
			runSpeed = Mathf.Lerp(runSpeed, 0.6f, 0.2f);
		}
		else if (behavior == Behavior.GoToSpitPos)
		{
			if (redSpitAI.spitFromPos.room == creature.pos.room)
			{
				creature.abstractAI.SetDestination(redSpitAI.spitFromPos);
			}
			runSpeed = Mathf.Lerp(runSpeed, 1f, 0.2f);
		}
		else if (behavior == Behavior.FollowFriend)
		{
			creature.abstractAI.SetDestination(base.friendTracker.friendDest);
			runSpeed = Mathf.Lerp(runSpeed, base.friendTracker.RunSpeed(), 0.6f);
			if (lizard.grasps[0] != null && lizard.grasps[0].grabbed == base.friendTracker.friend)
			{
				lizard.ReleaseGrasp(0);
			}
		}
		if (behavior != Behavior.Idle && creature.creatureTemplate.type == CreatureTemplate.Type.RedLizard)
		{
			runSpeed = Mathf.Pow(runSpeed, 0.7f);
		}
		runSpeed = Mathf.Max(runSpeed, base.stuckTracker.Utility());
		if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
		{
			base.noiseTracker.hearingSkill = 2f;
		}
		else
		{
			base.noiseTracker.hearingSkill = Custom.LerpMap(runSpeed, 0f, 0.6f, 1.3f, 0.6f);
		}
		if (lizard.grasps[0] == null && base.tracker.CreaturesCount > 0)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(UnityEngine.Random.Range(0, base.tracker.CreaturesCount));
			if ((DoIWantToBiteThisCreature(rep) || (UnityEngine.Random.value > lizard.LizardState.health && UnityEngine.Random.value < 0.05f)) && rep.VisualContact && (casualAggressionTarget == null || casualAggressionTarget.representedCreature.realizedCreature == null || Vector2.Distance(lizard.mainBodyChunk.pos, rep.representedCreature.realizedCreature.mainBodyChunk.pos) < Vector2.Distance(lizard.mainBodyChunk.pos, casualAggressionTarget.representedCreature.realizedCreature.mainBodyChunk.pos)))
			{
				casualAggressionTarget = rep;
			}
		}
		if (casualAggressionTarget != null && !lizard.safariControlled && UnityEngine.Random.value < 0.5f && casualAggressionTarget.VisualContact && Custom.DistLess(casualAggressionTarget.representedCreature.realizedCreature.mainBodyChunk.pos, lizard.mainBodyChunk.pos, lizard.lizardParams.attemptBiteRadius))
		{
			lizard.AttemptBite(casualAggressionTarget.representedCreature.realizedCreature);
		}
		if ((lizard.Template.type == CreatureTemplate.Type.RedLizard || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)) && redSpitAI.spitting)
		{
			lizard.EnterAnimation(Lizard.Animation.Spit, forceAnimationChange: false);
		}
		if (lizard.safariControlled && lizard.animation != Lizard.Animation.Lounge)
		{
			if (!lizard.inputWithDiagonals.HasValue || !lizard.inputWithDiagonals.Value.pckp)
			{
				lizard.biteControlReset = true;
			}
			if (lizard.inputWithDiagonals.HasValue && lizard.inputWithDiagonals.Value.jmp && !lizard.lastInputWithDiagonals.Value.jmp && lizard.tongue != null && lizard.tongue.Ready && lizard.grasps[0] == null && lizard.Submersion < 0.5f && !lizard.tongue.Out && (casualAggressionTarget == null || casualAggressionTarget.representedCreature.realizedCreature == null || !Custom.DistLess(casualAggressionTarget.representedCreature.realizedCreature.mainBodyChunk.pos, lizard.mainBodyChunk.pos, lizard.lizardParams.attemptBiteRadius)))
			{
				lizard.EnterAnimation(Lizard.Animation.ShootTongue, forceAnimationChange: false);
			}
			else if (lizard.inputWithDiagonals.HasValue && lizard.inputWithDiagonals.Value.pckp && (lizard.biteControlReset || (lizard.tongue != null && lizard.tongue.StuckToSomething)))
			{
				if (casualAggressionTarget != null && casualAggressionTarget.VisualContact && Custom.DistLess(casualAggressionTarget.representedCreature.realizedCreature.mainBodyChunk.pos, lizard.mainBodyChunk.pos, lizard.lizardParams.attemptBiteRadius))
				{
					lizard.AttemptBite(casualAggressionTarget.representedCreature.realizedCreature);
				}
				else if (lizard.biteControlReset)
				{
					lizard.AttemptBite(null);
				}
			}
			if (lizard.inputWithDiagonals.HasValue && lizard.inputWithDiagonals.Value.jmp && lizard.lizardParams.loungeTendensy >= 0.5f && lizard.loungeDelay < 1 && (lizard.Template.type != MoreSlugcatsEnums.CreatureTemplateType.SpitLizard || lizard.jumpHeldTime >= 15))
			{
				lizard.EnterAnimation(Lizard.Animation.PrepareToLounge, forceAnimationChange: false);
			}
		}
		base.Update();
	}

	public override void NewArea(bool strandedFromExits)
	{
		base.NewArea(strandedFromExits);
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		if (!(behavior == Behavior.EscapeRain))
		{
			if (behavior == Behavior.Injured)
			{
				if (ModManager.MMF && MMF.cfgNoArenaFleeing.Value)
				{
					return !creature.Room.world.game.IsArenaSession;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public override float VisualScore(Vector2 lookAtPoint, float bonus)
	{
		return base.VisualScore(lookAtPoint, bonus) - Mathf.InverseLerp(lizard.lizardParams.perfectVisionAngle, lizard.lizardParams.periferalVisionAngle, Vector2.Dot((lizard.bodyChunks[0].pos - lizard.bodyChunks[1].pos).normalized, (lookAtPoint - lizard.bodyChunks[1].pos).normalized));
	}

	private void AggressiveBehavior(Tracker.CreatureRepresentation target, float tongueChance)
	{
		casualAggressionTarget = null;
		creature.abstractAI.SetDestination(target.BestGuessForPosition());
		if (lizard.animation == Lizard.Animation.PrepareToLounge)
		{
			lizard.JawOpen = Mathf.Lerp(lizard.JawOpen, 0.2f, 0.2f);
		}
		else if (lizard.animation == Lizard.Animation.Lounge)
		{
			lizard.JawOpen += 0.2f;
		}
		else if (lizard.animation == Lizard.Animation.ShootTongue)
		{
			if (lizard.tongue.Out)
			{
				lizard.JawOpen += 0.1f;
			}
			else
			{
				lizard.JawOpen = Mathf.Lerp(lizard.JawOpen, UnityEngine.Random.value, 0.2f);
			}
		}
		else if (target.VisualContact)
		{
			lizard.JawOpen = Mathf.InverseLerp(400f, 90f + lizard.lizardParams.biteInFront, Vector2.Distance(lizard.mainBodyChunk.pos, target.representedCreature.realizedCreature.mainBodyChunk.pos));
		}
		else
		{
			lizard.JawOpen = Mathf.Lerp(lizard.JawOpen, Mathf.InverseLerp(0.6f, 0.9f, hunger * hunger) * 0.7f, 0.2f);
		}
		if (lizard.Template.type == CreatureTemplate.Type.RedLizard && target.VisualContact)
		{
			lizard.JawOpen = Mathf.Clamp(lizard.JawOpen + 0.1f, 0f, 1f);
		}
		if (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard && target.VisualContact)
		{
			lizard.JawOpen = Mathf.Clamp(lizard.JawOpen + 0.01f, 0f, 0.5f);
		}
		focusCreature = target;
		if (!focusCreature.VisualContact || lizard.safariControlled)
		{
			return;
		}
		if (lizard.loungeDelay < 1 && UnityEngine.Random.value < lizard.lizardParams.loungeTendensy && !UnpleasantFallRisk(creature.pos.Tile) && !FallRisk(creature.pos.Tile) && !UnpleasantFallRisk(focusCreature.representedCreature.pos.Tile) && Custom.DistLess(creature.realizedCreature.mainBodyChunk.pos, focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, lizard.lizardParams.loungeDistance) && !lizard.room.aimap.getAItile(focusCreature.representedCreature.pos.Tile).narrowSpace && !lizard.room.aimap.getAItile(creature.pos).narrowSpace && (creature.creatureTemplate.type != CreatureTemplate.Type.GreenLizard || lizard.room.GetTile(lizard.mainBodyChunk.pos + new Vector2(0f, -20f)).Solid) && lizard.LegsGripping > 2)
		{
			lizard.EnterAnimation(Lizard.Animation.PrepareToLounge, forceAnimationChange: false);
		}
		else if (lizard.tongue != null && lizard.tongue.Ready && lizard.grasps[0] == null && Custom.DistLess(creature.realizedCreature.mainBodyChunk.pos, focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, lizard.lizardParams.tongueAttackRange) && !Custom.DistLess(creature.realizedCreature.mainBodyChunk.pos, focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, Mathf.Lerp(lizard.lizardParams.attemptBiteRadius, lizard.lizardParams.tongueAttackRange, 0.2f)) && (lizard.Submersion < 0.5f || target.representedCreature.realizedCreature.Submersion < 0.5f) && UnityEngine.Random.value < lizard.lizardParams.tongueChance * tongueChance)
		{
			lizard.EnterAnimation(Lizard.Animation.ShootTongue, forceAnimationChange: false);
		}
		if (focusCreature.age > 120 && lizard.JawReadyForBite)
		{
			attemptBiteFrames++;
			for (int i = 0; i < focusCreature.representedCreature.realizedCreature.bodyChunks.Length; i++)
			{
				if (Custom.DistLess(focusCreature.representedCreature.realizedCreature.bodyChunks[i].pos, lizard.mainBodyChunk.pos, lizard.lizardParams.attemptBiteRadius))
				{
					if (ModManager.MMF && base.friendTracker.creature != null && base.friendTracker.friend == focusCreature.representedCreature.realizedCreature && base.friendTracker.friendRel.like > 0.25f)
					{
						lizard.AttemptBite(null);
					}
					else
					{
						lizard.AttemptBite(focusCreature.representedCreature.realizedCreature);
					}
					break;
				}
			}
		}
		else
		{
			if (attemptBiteFrames > 5)
			{
				lizard.JawsSnapShut(focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
			}
			attemptBiteFrames = 0;
		}
	}

	public bool UnpleasantFallRisk(IntVector2 tile)
	{
		if (lizard.room.GetTile(lizard.room.aimap.getAItile(tile).fallRiskTile).AnyWater)
		{
			return true;
		}
		if (lizard.room.aimap.getAItile(tile).fallRiskTile.y < 0)
		{
			return true;
		}
		if (lizard.room.aimap.getAItile(tile).fallRiskTile.y < tile.y - 20)
		{
			return true;
		}
		if (lizard.room.aimap.getAItile(tile).fallRiskTile.y < tile.y - 10 && !base.pathFinder.CoordinatePossibleToGetBackFrom(lizard.room.GetWorldCoordinate(lizard.room.aimap.getAItile(tile).fallRiskTile)))
		{
			return true;
		}
		return false;
	}

	private bool FallRisk(IntVector2 tile)
	{
		if (lizard.room.aimap.getAItile(tile).fallRiskTile.y < creature.pos.y - 5)
		{
			return true;
		}
		return false;
	}

	public void BitCreature(BodyChunk chunk)
	{
		if (!UnpleasantFallRisk(creature.pos.Tile))
		{
			lizard.EnterAnimation(Lizard.Animation.ShakePrey, forceAnimationChange: true);
		}
		if (base.obstacleTracker != null)
		{
			base.obstacleTracker.EraseObstacleObject(chunk.owner);
		}
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation otherCreature)
	{
		if (lizard.graphicsModule != null)
		{
			(lizard.graphicsModule as LizardGraphics).CreatureSpotted(firstSpot, otherCreature);
		}
		CreatureTemplate.Relationship relationship = DynamicRelationship(otherCreature);
		if (relationship.GoForKill && relationship.intensity > (firstSpot ? 0.2f : 0.45f) && (lizard.grasps[0] == null || lizard.grasps[0].grabbed.abstractPhysicalObject != otherCreature.representedCreature))
		{
			if (firstSpot)
			{
				lizard.EnterAnimation(Lizard.Animation.PreySpotted, forceAnimationChange: false);
			}
			else
			{
				lizard.EnterAnimation(Lizard.Animation.PreyReSpotted, forceAnimationChange: false);
			}
		}
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false);
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 5);
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Attacks)
		{
			return base.preyTracker;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.AgressiveRival)
		{
			return base.agressionTracker;
		}
		return null;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		if (rel.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
		{
			return new LizardTrackState();
		}
		return null;
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (base.friendTracker.giftOfferedToMe != null && base.friendTracker.giftOfferedToMe.active && base.friendTracker.giftOfferedToMe.item == dRelation.trackerRep.representedCreature.realizedCreature)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, dRelation.trackerRep.representedCreature.state.dead ? 1f : 0.65f);
		}
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
		{
			if (ModManager.MSC && base.friendTracker.friend != null && dRelation.trackerRep.representedCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && dRelation.trackerRep.representedCreature.abstractAI.RealAI != null && (dRelation.trackerRep.representedCreature.abstractAI.RealAI.friendTracker.friend == null || dRelation.trackerRep.representedCreature.abstractAI.RealAI.friendTracker.friend == base.friendTracker.friend))
			{
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.5f);
			}
			if (base.friendTracker.friend != null && base.friendTracker.Urgency > 0.2f && dRelation.trackerRep.representedCreature.creatureTemplate.dangerousToPlayer > 0f && dRelation.trackerRep.representedCreature != base.friendTracker.friend.abstractCreature && dRelation.trackerRep.representedCreature.state.alive && dRelation.trackerRep.representedCreature.realizedCreature != null && dRelation.trackerRep.representedCreature.abstractAI != null && dRelation.trackerRep.representedCreature.abstractAI.RealAI != null)
			{
				float num = Mathf.InverseLerp(0.2f, 0.8f, base.friendTracker.Urgency) * Mathf.Pow(dRelation.trackerRep.representedCreature.creatureTemplate.dangerousToPlayer * ((base.friendTracker.friend is Player) ? dRelation.trackerRep.representedCreature.abstractAI.RealAI.CurrentPlayerAggression(base.friendTracker.friend.abstractCreature) : 1f), 0.5f);
				num *= Mathf.InverseLerp(30f, 7f, Custom.WorldCoordFloatDist(base.friendTracker.friend.abstractCreature.pos, dRelation.trackerRep.BestGuessForPosition()));
				if (!Custom.DistLess(base.friendTracker.friend.abstractCreature.pos, dRelation.trackerRep.BestGuessForPosition(), Custom.WorldCoordFloatDist(creature.pos, base.friendTracker.friend.abstractCreature.pos)))
				{
					num *= 0.5f;
				}
				if (num > 0f && (StaticRelationship(dRelation.trackerRep.representedCreature).type != CreatureTemplate.Relationship.Type.Eats || StaticRelationship(dRelation.trackerRep.representedCreature).intensity < num))
				{
					return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, num);
				}
			}
			return StaticRelationship(dRelation.trackerRep.representedCreature).Duplicate();
		}
		if (dRelation.state != null)
		{
			if (dRelation.trackerRep.VisualContact && dRelation.trackerRep.representedCreature.realizedCreature != null)
			{
				(dRelation.state as LizardTrackState).spear = false;
				(dRelation.state as LizardTrackState).vultureMask = 0;
				if (dRelation.trackerRep.representedCreature.realizedCreature.grasps != null)
				{
					for (int i = 0; i < dRelation.trackerRep.representedCreature.realizedCreature.grasps.Length; i++)
					{
						if (dRelation.trackerRep.representedCreature.realizedCreature.grasps[i] != null)
						{
							if (dRelation.trackerRep.representedCreature.realizedCreature.grasps[i].grabbed is Spear)
							{
								(dRelation.state as LizardTrackState).spear = true;
							}
							else if (dRelation.trackerRep.representedCreature.realizedCreature.grasps[i].grabbed is VultureMask)
							{
								(dRelation.state as LizardTrackState).vultureMask = Math.Max((dRelation.state as LizardTrackState).vultureMask, (!(dRelation.trackerRep.representedCreature.realizedCreature.grasps[i].grabbed as VultureMask).King) ? 1 : 2);
							}
						}
					}
				}
			}
			if ((dRelation.state as LizardTrackState).vultureMask > 0 && creature.creatureTemplate.type != CreatureTemplate.Type.BlackLizard && creature.creatureTemplate.type != CreatureTemplate.Type.RedLizard && usedToVultureMask < (((dRelation.state as LizardTrackState).vultureMask == 2) ? 1200 : 700))
			{
				usedToVultureMask++;
				if (creature.creatureTemplate.type == CreatureTemplate.Type.GreenLizard && (dRelation.state as LizardTrackState).vultureMask < 2)
				{
					return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
				}
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(((dRelation.state as LizardTrackState).vultureMask == 2) ? 1200 : 700, 600f, usedToVultureMask) * (((dRelation.state as LizardTrackState).vultureMask != 2) ? ((creature.creatureTemplate.type == CreatureTemplate.Type.BlueLizard) ? 0.8f : 0.6f) : ((creature.creatureTemplate.type == CreatureTemplate.Type.GreenLizard) ? 0.4f : 0.9f)));
			}
		}
		float num2 = LikeOfPlayer(dRelation.trackerRep);
		if (ModManager.CoopAvailable && Custom.rainWorld.options.friendlyLizards)
		{
			foreach (AbstractCreature nonPermaDeadPlayer in lizard.abstractCreature.world.game.NonPermaDeadPlayers)
			{
				Tracker.CreatureRepresentation player = base.tracker.RepresentationForCreature(nonPermaDeadPlayer, addIfMissing: false);
				num2 = Mathf.Max(LikeOfPlayer(player), num2);
			}
		}
		return (!(num2 < 0.5f)) ? new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f) : new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Mathf.Pow(Mathf.InverseLerp(0.5f, -1f, num2), lizard.lizardParams.aggressionCurveExponent));
	}

	public float LikeOfPlayer(Tracker.CreatureRepresentation player)
	{
		if (player == null)
		{
			return 0f;
		}
		float a = creature.world.game.session.creatureCommunities.LikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (player.representedCreature.state as PlayerState).playerNumber);
		a = Mathf.Lerp(a, 0f - lizard.spawnDataEvil, Mathf.Abs(lizard.spawnDataEvil));
		float tempLike = creature.state.socialMemory.GetTempLike(player.representedCreature.ID);
		a = Mathf.Lerp(a, tempLike, Mathf.Abs(tempLike));
		if (base.friendTracker.giftOfferedToMe != null && base.friendTracker.giftOfferedToMe.owner == player.representedCreature.realizedCreature)
		{
			a = Custom.LerpMap(a, -0.5f, 1f, 0f, 1f, 0.8f);
		}
		return a;
	}

	public override float CurrentPlayerAggression(AbstractCreature player)
	{
		Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForCreature(player, addIfMissing: false);
		if (creatureRepresentation == null || creatureRepresentation.dynamicRelationship == null)
		{
			return 1f;
		}
		return Mathf.InverseLerp(0.5f, 0f, LikeOfPlayer(creatureRepresentation));
	}

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		if (base.obstacleTracker != null)
		{
			int num = base.obstacleTracker.ObstacleWarning(connection);
			cost += new PathCost(Mathf.Pow(num, 3f) * 5f, PathCost.Legality.Allowed);
		}
		if (base.threatTracker != null && base.threatTracker.Utility() > 0f)
		{
			float f = base.threatTracker.ThreatOfTile(connection.destinationCoord, accountThreatCreatureAccessibility: true);
			cost += new PathCost(Mathf.Pow(f, 3f) * 2f * Mathf.Pow(fear, 0.5f), PathCost.Legality.Allowed);
		}
		if (fear > 0.2f && (connection.type == MovementConnection.MovementType.DropToClimb || connection.type == MovementConnection.MovementType.DropToFloor) && base.threatTracker.ThreatOfArea(connection.destinationCoord, accountThreatCreatureAccessibility: true) > base.threatTracker.ThreatOfArea(connection.startCoord, accountThreatCreatureAccessibility: true))
		{
			cost += new PathCost(100f, PathCost.Legality.Allowed);
		}
		if (creature.creatureTemplate.type == CreatureTemplate.Type.GreenLizard)
		{
			if (lizard.room.aimap.getAItile(connection.destinationCoord).narrowSpace)
			{
				cost += new PathCost(10f, PathCost.Legality.Allowed);
			}
		}
		else if (creature.creatureTemplate.type == CreatureTemplate.Type.BlueLizard)
		{
			cost += new PathCost(Mathf.Clamp(30 - lizard.room.aimap.getAItile(connection.destinationCoord).floorAltitude * 3, 0f, 10f), PathCost.Legality.Allowed);
		}
		else if (creature.creatureTemplate.type == CreatureTemplate.Type.PinkLizard)
		{
			cost += new PathCost((float)lizard.room.aimap.getAItile(connection.destinationCoord).floorAltitude * 2f, PathCost.Legality.Allowed);
		}
		else if (creature.creatureTemplate.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
		{
			if (!lizard.room.GetTile(connection.destinationCoord).AnyWater)
			{
				cost.resistance += 5f;
			}
		}
		else if (creature.creatureTemplate.type == CreatureTemplate.Type.YellowLizard)
		{
			cost = yellowAI.TravelPreference(connection, cost);
		}
		return cost;
	}

	private float IdleSpotScore(WorldCoordinate coord)
	{
		if (coord.room != creature.pos.room || !coord.TileDefined)
		{
			return 50f;
		}
		if (!lizard.room.aimap.WorldCoordinateAccessibleToCreature(coord, creature.creatureTemplate) || !base.pathFinder.CoordinateReachableAndGetbackable(coord) || coord.CompareDisregardingNode(forbiddenIdleSpot))
		{
			return float.MaxValue;
		}
		float num = 10f;
		num /= Mathf.Lerp(lizard.room.aimap.getAItile(coord.Tile).visibility, 1f, 0.9995f);
		if (lizard.room.aimap.getAItile(coord).narrowSpace)
		{
			num += 10f;
		}
		if (lizard.room.GetTile(coord).AnyWater)
		{
			num += 100f;
		}
		if (creature.creatureTemplate.type == CreatureTemplate.Type.GreenLizard)
		{
			if (lizard.room.aimap.getAItile(coord).acc == AItile.Accessibility.Floor)
			{
				num *= 0.5f;
			}
			if (lizard.room.aimap.getAItile(coord).narrowSpace)
			{
				num += 20f;
			}
		}
		else if (creature.creatureTemplate.type == CreatureTemplate.Type.BlueLizard)
		{
			num += Mathf.Clamp(10f - (float)lizard.room.aimap.getAItile(coord).floorAltitude, 0f, 10f);
			num += Mathf.Clamp(3f - (float)lizard.room.aimap.getTerrainProximity(coord), 0f, 3f);
			if (lizard.room.aimap.getAItile(coord).acc == AItile.Accessibility.Climb || lizard.room.aimap.getAItile(coord).acc == AItile.Accessibility.Wall)
			{
				num *= 0.5f;
			}
		}
		else if (creature.creatureTemplate.type == CreatureTemplate.Type.PinkLizard)
		{
			num += (float)lizard.room.aimap.getAItile(coord).floorAltitude * 2f;
			if (lizard.room.aimap.getAItile(coord).narrowSpace)
			{
				num -= 15f;
			}
		}
		else if (creature.creatureTemplate.type == CreatureTemplate.Type.BlackLizard)
		{
			if (lizard.room.aimap.getAItile(coord).narrowSpace)
			{
				num -= 20f;
			}
			num += (float)lizard.room.aimap.getAItile(coord.Tile).visibility * 0.1f;
		}
		else if (creature.creatureTemplate.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
		{
			if (!lizard.room.GetTile(coord).AnyWater)
			{
				num += 20f;
			}
			num += Mathf.Max(0f, coord.Tile.FloatDist(creature.pos.Tile) - 30f) * 1.5f;
			num += (float)Mathf.Abs(coord.y - lizard.room.defaultWaterLevel) * 10f;
			num += (float)lizard.room.aimap.getTerrainProximity(coord) * 10f;
		}
		if (base.threatTracker != null && base.threatTracker.Utility() > 0f)
		{
			num += Mathf.Clamp(base.threatTracker.ThreatOfArea(coord, accountThreatCreatureAccessibility: true), 0f, 100f);
		}
		return num;
	}

	private bool ComfortableIdlePosition()
	{
		if (lizard.room.GetTile(lizard.bodyChunks[0].pos).AnyWater)
		{
			if (lizard.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
			{
				return true;
			}
			return false;
		}
		if (lizard.room.GetTile(lizard.bodyChunks[0].pos + Custom.DirVec(lizard.bodyChunks[1].pos, lizard.bodyChunks[0].pos) * 30f).Terrain == Room.Tile.TerrainType.Solid)
		{
			return false;
		}
		if (lizard.room.GetTilePosition(lizard.bodyChunks[0].pos).y == lizard.room.GetTilePosition(lizard.bodyChunks[2].pos).y)
		{
			return true;
		}
		if (creature.creatureTemplate.type == CreatureTemplate.Type.BlueLizard && lizard.room.GetTilePosition(lizard.bodyChunks[0].pos).x == lizard.room.GetTilePosition(lizard.bodyChunks[2].pos).x)
		{
			return true;
		}
		return false;
	}

	public bool DoIWantToHoldThisWithMyTongue(BodyChunk chunk)
	{
		if (chunk == null || (chunk.owner is Creature && (chunk.owner as Creature).enteringShortCut.HasValue) || chunk.owner.room != lizard.room || chunk.submersion > 0.5f || (UnpleasantFallRisk(creature.pos.Tile) && UnpleasantFallRisk(lizard.room.GetTilePosition(chunk.pos))))
		{
			return false;
		}
		if (chunk.owner is Creature)
		{
			return DynamicRelationship((chunk.owner as Creature).abstractCreature).GoForKill;
		}
		return false;
	}

	public bool DoIWantToBiteThisCreature(Tracker.CreatureRepresentation otherCrit)
	{
		if (UnityEngine.Random.value < 0.1f && UnityEngine.Random.value > lizard.LizardState.health)
		{
			return true;
		}
		CreatureTemplate.Relationship currentRelationship = otherCrit.dynamicRelationship.currentRelationship;
		if (currentRelationship.GoForKill)
		{
			return true;
		}
		if (UnityEngine.Random.value < 0.5f && currentRelationship.type == CreatureTemplate.Relationship.Type.AgressiveRival)
		{
			return true;
		}
		return false;
	}

	private void SendCommunication(Lizard otherLizard, LizardCommunication signal)
	{
		if (otherLizard == null || otherLizard.dead || otherLizard.room != lizard.room)
		{
			return;
		}
		focusCreature = base.tracker.RepresentationForObject(otherLizard, AddIfMissing: false);
		if (signal == LizardCommunication.Agression)
		{
			if (lizard.graphicsModule != null)
			{
				(lizard.graphicsModule as LizardGraphics).showDominance += UnityEngine.Random.value;
			}
			lizard.voice.MakeSound(LizardVoice.Emotion.Dominance);
		}
		else if (signal == LizardCommunication.Submission)
		{
			if (lizard.graphicsModule != null)
			{
				(lizard.graphicsModule as LizardGraphics).showDominance -= UnityEngine.Random.value;
			}
			lizard.voice.MakeSound(LizardVoice.Emotion.Submission);
		}
		otherLizard.AI.RecieveCommunication(lizard, signal);
	}

	public void RecieveCommunication(Lizard otherLizard, LizardCommunication signal)
	{
		if (!VisualContact(otherLizard.mainBodyChunk))
		{
			return;
		}
		focusCreature = base.tracker.RepresentationForObject(otherLizard, AddIfMissing: false);
		if (signal == LizardCommunication.Agression)
		{
			if (otherLizard.AI.submittedTo != creature)
			{
				if (submittedTo != otherLizard.abstractCreature)
				{
					base.agressionTracker.IncrementAnger(base.tracker.RepresentationForObject(otherLizard, AddIfMissing: false), 0.1f);
				}
				if (UnityEngine.Random.value < 1f / 24f)
				{
					SendCommunication(otherLizard, LizardCommunication.Submission);
					base.agressionTracker.SetAnger(base.tracker.RepresentationForObject(otherLizard, AddIfMissing: false), 0f, 0f);
					submittedTo = base.tracker.RepresentationForObject(otherLizard, AddIfMissing: false).representedCreature;
					base.missionTracker.AddMission(new MissionTracker.LeaveRoom(base.missionTracker, 0.4f, 1000), canCoexistWithMissionOfSameType: false);
				}
			}
		}
		else if (signal == LizardCommunication.Submission)
		{
			base.agressionTracker.SetAnger(base.tracker.RepresentationForObject(otherLizard, AddIfMissing: false), 0f, -1f);
		}
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
		if (source.creatureRep != null && lizard.Template.type == CreatureTemplate.Type.BlackLizard)
		{
			lizard.bubble = Math.Max(lizard.bubble, 4);
			return;
		}
		if (source.creatureRep != null && source.creatureRep == base.preyTracker.MostAttractivePrey)
		{
			lizard.EnterAnimation(Lizard.Animation.PreyReSpotted, forceAnimationChange: false);
			return;
		}
		if ((behavior == Behavior.Idle || behavior == Behavior.InvestigateSound || behavior == Behavior.Lurk || behavior == Behavior.Travelling) && source == source.noiseTracker.soundToExamine)
		{
			lizard.EnterAnimation(Lizard.Animation.HearSound, forceAnimationChange: false);
			return;
		}
		if (!lizard.safariControlled)
		{
			lizard.voice.MakeSound(LizardVoice.Emotion.GeneralSmallNoise);
		}
		lizard.bubble = Math.Max(lizard.bubble, 5);
	}

	public void SocialEvent(SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
	{
		if (!(subjectCrit is Player))
		{
			return;
		}
		Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForObject(subjectCrit, AddIfMissing: false);
		if (creatureRepresentation == null)
		{
			return;
		}
		Tracker.CreatureRepresentation creatureRepresentation2 = null;
		bool flag = objectCrit == lizard;
		if (!flag)
		{
			creatureRepresentation2 = base.tracker.RepresentationForObject(objectCrit, AddIfMissing: false);
			if (creatureRepresentation2 == null)
			{
				return;
			}
		}
		if (creatureRepresentation2 != null && creatureRepresentation.TicksSinceSeen > 40 && creatureRepresentation2.TicksSinceSeen > 40)
		{
			return;
		}
		if (ID == SocialEventRecognizer.EventID.ItemOffering)
		{
			if (flag)
			{
				base.friendTracker.ItemOffered(creatureRepresentation, involvedItem);
			}
			return;
		}
		float num = 0f;
		if (ID == SocialEventRecognizer.EventID.NonLethalAttackAttempt)
		{
			num = 0.05f;
		}
		else if (ID == SocialEventRecognizer.EventID.NonLethalAttack)
		{
			num = 0.15f;
		}
		else if (ID == SocialEventRecognizer.EventID.LethalAttackAttempt)
		{
			num = 0.35f;
		}
		else if (ID == SocialEventRecognizer.EventID.LethalAttack)
		{
			num = 1f;
		}
		if (objectCrit.dead)
		{
			num /= 3f;
		}
		if (flag)
		{
			if (base.friendTracker.friend != null && subjectCrit == base.friendTracker.friend)
			{
				if (ID == SocialEventRecognizer.EventID.NonLethalAttackAttempt || ID == SocialEventRecognizer.EventID.LethalAttackAttempt)
				{
					return;
				}
				num /= 2f;
			}
			LizardPlayerRelationChange(0f - num, subjectCrit.abstractCreature);
		}
		else if (creatureRepresentation2.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			float num2 = 0.1f;
			if (base.threatTracker.GetThreatCreature(objectCrit.abstractCreature) != null)
			{
				num2 += 0.7f * Custom.LerpMap(Vector2.Distance(lizard.mainBodyChunk.pos, objectCrit.DangerPos), 120f, 320f, 1f, 0.1f);
			}
			bool flag2 = false;
			for (int i = 0; i < objectCrit.grasps.Length; i++)
			{
				if (flag2)
				{
					break;
				}
				if (objectCrit.grasps[i] != null && objectCrit.grasps[i].grabbed == lizard)
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				if (ID == SocialEventRecognizer.EventID.NonLethalAttack || ID == SocialEventRecognizer.EventID.LethalAttack)
				{
					num = 1f;
				}
				num2 = 2f;
			}
			LizardPlayerRelationChange(Mathf.Pow(num, 0.5f) * num2, subjectCrit.abstractCreature);
		}
		else if (creatureRepresentation2.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack)
		{
			LizardPlayerRelationChange((0f - num) * 0.75f, subjectCrit.abstractCreature);
		}
	}

	private void LizardPlayerRelationChange(float change, AbstractCreature player)
	{
		SocialMemory.Relationship orInitiateRelationship = creature.state.socialMemory.GetOrInitiateRelationship(player.ID);
		orInitiateRelationship.InfluenceTempLike(change * 1.5f);
		orInitiateRelationship.InfluenceLike(change * 0.75f);
		orInitiateRelationship.InfluenceKnow(Mathf.Abs(change) * 0.25f);
		creature.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (player.state as PlayerState).playerNumber, change * 0.05f, 0.25f, 0.3f);
	}

	public void GiftRecieved(SocialEventRecognizer.OwnedItemOnGround giftOfferedToMe)
	{
		Custom.Log("Lizard recieved gift!");
		SocialMemory.Relationship orInitiateRelationship = creature.realizedCreature.State.socialMemory.GetOrInitiateRelationship(giftOfferedToMe.owner.abstractCreature.ID);
		bool flag = giftOfferedToMe.item is Creature && (giftOfferedToMe.item as Creature).dead;
		if (orInitiateRelationship.like > -0.9f)
		{
			if (ModManager.MSC && lizard.room.game.session is ArenaGameSession && lizard.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && lizard.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.tamingDifficultyMultiplier > 0)
			{
				float num = (float)lizard.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.tamingDifficultyMultiplier / 10f;
				orInitiateRelationship.InfluenceLike((flag ? 1.2f : 0.6f) / num);
				orInitiateRelationship.InfluenceTempLike((flag ? 1.7f : 0.9f) / num);
			}
			else
			{
				orInitiateRelationship.InfluenceLike((flag ? 1.2f : 0.6f) / base.friendTracker.tamingDifficlty);
				orInitiateRelationship.InfluenceTempLike((flag ? 1.7f : 0.9f) / base.friendTracker.tamingDifficlty);
			}
		}
		if (giftOfferedToMe.owner is Player)
		{
			creature.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (!(creature.world.game.session is StoryGameSession)) ? (giftOfferedToMe.owner as Player).playerState.playerNumber : 0, 0.1f, 0.2f, 0.1f);
		}
		if (ModManager.MMF && MMF.cfgExtraLizardSounds.Value && lizard.voice.articulationIndex != MMFEnums.LizardVoiceEmotion.Love.Index)
		{
			lizard.voice.MakeSound(MMFEnums.LizardVoiceEmotion.Love, 1f);
		}
	}
}
