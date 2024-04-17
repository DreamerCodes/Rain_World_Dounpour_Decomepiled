using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class DeerAI : ArtificialIntelligence, IUseARelationshipTracker, IUseItemTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior TrackSpores = new Behavior("TrackSpores", register: true);

		public static readonly Behavior Kneeling = new Behavior("Kneeling", register: true);

		public static readonly Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class SporeTracker : AIModule
	{
		public SporeTracker(ArtificialIntelligence AI)
			: base(AI)
		{
		}

		public override void Update()
		{
		}

		public override float Utility()
		{
			if ((AI as DeerAI).goToPuffBall == null && !(AI.creature.abstractAI as DeerAbstractAI).sporePos.HasValue)
			{
				return 0f;
			}
			return 1f;
		}
	}

	public Deer deer;

	public int timeInThisRoom;

	public int kneelCounter;

	public int closeEyesCounter;

	public int tiredOfClosingEyesCounter;

	public int layDownAndRestCounter;

	public int restingCounter;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public ItemTracker.ItemRepresentation goToPuffBall;

	private WorldCoordinate? restPos;

	private WormGrass wormGrass;

	public float currentUtility;

	private SporeTracker sporeTracker;

	public int seriouslyStuck;

	public WorldCoordinate cantEatFromCoordinate;

	public Behavior behavior;

	public int deerPileCounter;

	public Tracker.CreatureRepresentation focusCreature;

	public WorldCoordinate inRoomDestination;

	private bool lastPlayerInAntlers;

	public int heldPuffballNotGiven;

	public List<EntityID> deniedPuffballs;

	public int minorWanderTimer;

	public bool AllowMovementBetweenRooms
	{
		get
		{
			if (!deer.safariControlled)
			{
				if (timeInThisRoom > 100)
				{
					return deer.playersInAntlers.Count == 0;
				}
				return false;
			}
			return true;
		}
	}

	public WorldCoordinate? sporePos
	{
		get
		{
			return (creature.abstractAI as DeerAbstractAI).sporePos;
		}
		set
		{
			(creature.abstractAI as DeerAbstractAI).sporePos = value;
		}
	}

	public DeerAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		deer = creature.realizedCreature as Deer;
		deer.AI = this;
		AddModule(new DeerPather(this, world, creature));
		base.pathFinder.accessibilityStepsPerFrame = 60;
		base.pathFinder.stepsPerFrame = 30;
		AddModule(new Tracker(this, 10, 5, 250, 0.5f, 5, 5, 20));
		AddModule(new RainTracker(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		AddModule(new ItemTracker(this, 10, 10, 4000, 10, stopTrackingCarried: false));
		AddModule(new DenFinder(this, creature));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: false));
		base.stuckTracker.checkPastPositionsFrom = 0;
		base.stuckTracker.totalTrackedLastPositions = 40;
		base.stuckTracker.pastStuckPositionsCloseToIncrementStuckCounter = 35;
		base.stuckTracker.minStuckCounter = 140;
		base.stuckTracker.maxStuckCounter = 240;
		base.stuckTracker.goalSatisfactionDistance = 7;
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		sporeTracker = new SporeTracker(this);
		AddModule(sporeTracker);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(sporeTracker, null, 1f, 1.1f);
		behavior = Behavior.Idle;
		deniedPuffballs = new List<EntityID>();
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		timeInThisRoom = 0;
		wormGrass = null;
		for (int i = 0; i < newRoom.accessModifiers.Count; i++)
		{
			if (wormGrass != null)
			{
				break;
			}
			if (newRoom.accessModifiers[i] is WormGrass)
			{
				wormGrass = newRoom.accessModifiers[i] as WormGrass;
			}
		}
		for (int j = 0; j < newRoom.abstractRoom.entities.Count; j++)
		{
			if (newRoom.abstractRoom.entities[j] is AbstractPhysicalObject && (newRoom.abstractRoom.entities[j] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.PuffBall && (newRoom.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject != null)
			{
				base.itemTracker.SeeItem(newRoom.abstractRoom.entities[j] as AbstractPhysicalObject);
			}
		}
		if (ModManager.MMF && MMF.cfgDeerBehavior.Value)
		{
			WorldCoordinate destination = IdleRoomWanderGoal();
			creature.abstractAI.SetDestination(destination);
			inRoomDestination = destination;
		}
		else if (base.pathFinder.GetDestination.room == newRoom.abstractRoom.index && !base.pathFinder.GetDestination.TileDefined)
		{
			creature.abstractAI.SetDestination(new WorldCoordinate(newRoom.abstractRoom.index, UnityEngine.Random.Range(0, newRoom.TileWidth), UnityEngine.Random.Range(newRoom.defaultWaterLevel, newRoom.TileHeight), base.pathFinder.GetDestination.abstractNode));
		}
		restingCounter = UnityEngine.Random.Range(90, 600);
		layDownAndRestCounter = UnityEngine.Random.Range(190, 1100);
		restPos = null;
		if (!ModManager.MMF || !MMF.cfgDeerBehavior.Value)
		{
			inRoomDestination = creature.pos;
		}
	}

	public override void Update()
	{
		focusCreature = null;
		if (deerPileCounter > 100)
		{
			base.stuckTracker.stuckCounter = Math.Min(base.stuckTracker.stuckCounter + 2, base.stuckTracker.maxStuckCounter);
			kneelCounter -= 5;
			layDownAndRestCounter -= 5;
		}
		deerPileCounter = Custom.IntClamp(deerPileCounter - 1, 0, 150);
		base.Update();
		if (creature.pos.x > 10 && creature.pos.x < deer.room.TileWidth - 11)
		{
			timeInThisRoom++;
		}
		base.utilityComparer.GetUtilityTracker(sporeTracker).weight = 0.9f * Mathf.InverseLerp(100f, 30f, deerPileCounter);
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
			else if (aIModule is SporeTracker)
			{
				behavior = Behavior.TrackSpores;
			}
		}
		if ((creature.abstractAI as DeerAbstractAI).damageGoHome)
		{
			currentUtility = Mathf.Max(currentUtility, 0.8f);
			behavior = Behavior.EscapeRain;
		}
		if (currentUtility < 0.1f)
		{
			behavior = Behavior.Idle;
		}
		if (!deer.safariControlled && base.stuckTracker.Utility() > 0.9f && !deer.Kneeling && deer.resting < 0.5f)
		{
			seriouslyStuck++;
			if (seriouslyStuck > 100)
			{
				behavior = Behavior.GetUnstuck;
			}
		}
		else
		{
			seriouslyStuck = 0;
		}
		if (deer.Kneeling && behavior != Behavior.EscapeRain)
		{
			behavior = Behavior.Kneeling;
		}
		if (deer.room.game.devToolsActive && Input.GetKey("e"))
		{
			creature.abstractAI.SetDestination(deer.room.GetWorldCoordinate((Vector2)Futile.mousePosition + deer.room.game.cameras[0].pos));
		}
		if (deer.room.game.devToolsActive && Input.GetKey("n"))
		{
			creature.abstractAI.SetDestination(new WorldCoordinate(deer.room.world.offScreenDen.index, -1, -1, 0));
		}
		if (goToPuffBall != null && (goToPuffBall.deleteMeNextFrame || !PuffBallLegal(goToPuffBall)))
		{
			goToPuffBall = null;
		}
		if (!ModManager.MMF || !MMF.cfgDeerBehavior.Value)
		{
			FindGotoPuffBall();
		}
		if (deer.playersInAntlers.Count > 0)
		{
			if (ModManager.MMF && MMF.cfgDeerBehavior.Value)
			{
				(deer.abstractCreature.abstractAI as DeerAbstractAI).timeInRoom = 1;
			}
			layDownAndRestCounter = 0;
			restPos = null;
			if (!lastPlayerInAntlers && deer.room.TileWidth > 180 && Mathf.Abs(inRoomDestination.x - deer.room.TileWidth / 2) < deer.room.TileWidth / 3)
			{
				inRoomDestination = creature.pos;
			}
		}
		else if (ModManager.MMF && MMF.cfgDeerBehavior.Value)
		{
			FindGotoPuffBall();
		}
		lastPlayerInAntlers = deer.playersInAntlers.Count > 0;
		if (deerPileCounter < 50 && ((base.stuckTracker.Utility() < 0.5f) & (behavior == Behavior.Idle)))
		{
			for (int i = 0; i < base.tracker.CreaturesCount; i++)
			{
				if (!(base.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Deer) || !(base.tracker.GetRep(i).representedCreature.personality.dominance > deer.abstractCreature.personality.dominance) || !base.tracker.GetRep(i).VisualContact || base.tracker.GetRep(i).representedCreature.realizedCreature == null || !((base.tracker.GetRep(i).representedCreature.realizedCreature as Deer).AI.behavior == Behavior.Idle))
				{
					continue;
				}
				Vector2 pos = base.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos;
				if (Mathf.Abs(deer.mainBodyChunk.pos.x + 150f * deer.flipDir - pos.x) < 300f && (base.tracker.GetRep(i).representedCreature.realizedCreature as Deer).resting < 0.7f && Custom.DistLess(deer.mainBodyChunk.pos + new Vector2(deer.flipDir * 700f, 0f), pos, 900f) && (base.tracker.GetRep(i).representedCreature.realizedCreature as Deer).flipDir != deer.flipDir)
				{
					kneelCounter = Math.Max(kneelCounter, UnityEngine.Random.Range(40, 120));
					if ((base.tracker.GetRep(i).representedCreature.realizedCreature as Deer).AI.tiredOfClosingEyesCounter < 300)
					{
						(base.tracker.GetRep(i).representedCreature.realizedCreature as Deer).AI.closeEyesCounter = 5;
					}
					(base.tracker.GetRep(i).representedCreature.realizedCreature as Deer).AI.tiredOfClosingEyesCounter += 2;
				}
			}
		}
		kneelCounter--;
		closeEyesCounter--;
		layDownAndRestCounter--;
		tiredOfClosingEyesCounter = Custom.IntClamp(tiredOfClosingEyesCounter - 1, 0, 600);
		bool flag = false;
		if (behavior == Behavior.Idle)
		{
			if (AllowMovementBetweenRooms)
			{
				if (deer.room.IsPositionInsideBoundries(creature.pos.Tile))
				{
					creature.abstractAI.AbstractBehavior(1);
				}
				if (creature.abstractAI.destination.room != creature.pos.room)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				if (!deer.safariControlled && layDownAndRestCounter < 0 && deer.playersInAntlers.Count == 0 && deerPileCounter < 50)
				{
					if (!restPos.HasValue)
					{
						if (base.pathFinder.GetDestination.room == creature.pos.room && base.pathFinder.GetDestination.TileDefined)
						{
							CheckRestSpot(new IntVector2((creature.pos.x + base.pathFinder.GetDestination.x) / 2, (creature.pos.y + base.pathFinder.GetDestination.y) / 2));
						}
					}
					else
					{
						creature.abstractAI.SetDestination(restPos.Value);
						if (deer.resting > 0.5f)
						{
							restingCounter--;
							if (restingCounter < 1)
							{
								restingCounter = UnityEngine.Random.Range(90, 1200);
								layDownAndRestCounter = UnityEngine.Random.Range(190, 1100);
								restPos = null;
							}
						}
						flag = true;
					}
				}
				if (!flag && (creature.abstractAI.destination.room == creature.pos.room || !AllowMovementBetweenRooms))
				{
					if (!LegalInRoomDest(inRoomDestination))
					{
						minorWanderTimer++;
						if (minorWanderTimer > ((ModManager.MMF && MMF.cfgDeerBehavior.Value) ? 400 : (-999)))
						{
							WorldCoordinate rndm = ((!ModManager.MMF || !MMF.cfgDeerBehavior.Value) ? deer.room.GetWorldCoordinate(new IntVector2(UnityEngine.Random.Range(0, deer.room.TileWidth), UnityEngine.Random.Range(deer.room.defaultWaterLevel, deer.room.TileHeight))) : IdleRoomWanderGoal());
							if (GoodInRoomDest(rndm))
							{
								inRoomDestination = rndm;
								minorWanderTimer = UnityEngine.Random.Range(-20, 60);
							}
						}
					}
					creature.abstractAI.SetDestination(inRoomDestination);
				}
			}
		}
		else if (behavior == Behavior.EscapeRain)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (behavior == Behavior.TrackSpores)
		{
			if (sporePos.HasValue)
			{
				creature.abstractAI.SetDestination(sporePos.Value);
				inRoomDestination = sporePos.Value;
				if (sporePos.Value.room == deer.room.abstractRoom.index && Custom.DistLess(sporePos.Value.Tile, creature.pos.Tile, 17f) && VisualContact(deer.room.MiddleOfTile(sporePos.Value), 1f))
				{
					sporePos = null;
				}
				flag = true;
			}
			if (!flag && goToPuffBall != null)
			{
				if (goToPuffBall.BestGuessForPosition().room != creature.pos.room || Custom.ManhattanDistance(creature.pos, goToPuffBall.BestGuessForPosition()) < 6)
				{
					creature.abstractAI.SetDestination(goToPuffBall.BestGuessForPosition());
				}
				else
				{
					WorldCoordinate worldCoordinate = goToPuffBall.BestGuessForPosition();
					for (int j = 0; j < 2; j++)
					{
						if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
						{
							break;
						}
						worldCoordinate.y++;
					}
					creature.abstractAI.SetDestination(worldCoordinate);
				}
				if (goToPuffBall.representedItem.realizedObject != null && goToPuffBall.VisualContact && goToPuffBall.representedItem.realizedObject.grabbedBy.Count > 0 && Custom.DistLess(deer.mainBodyChunk.pos, goToPuffBall.representedItem.realizedObject.firstChunk.pos, 150f))
				{
					heldPuffballNotGiven++;
					if (heldPuffballNotGiven > 200)
					{
						Custom.Log($"pufball denied {goToPuffBall.representedItem.ID}");
						if (!deniedPuffballs.Contains(goToPuffBall.representedItem.ID))
						{
							deniedPuffballs.Add(goToPuffBall.representedItem.ID);
						}
						heldPuffballNotGiven = 0;
					}
				}
				if ((!deer.safariControlled || (deer.safariControlled && deer.inputWithoutDiagonals.HasValue && deer.inputWithoutDiagonals.Value.pckp)) && goToPuffBall.VisualContact && goToPuffBall.representedItem.realizedObject != null && Custom.DistLess(deer.mainBodyChunk.pos, goToPuffBall.representedItem.realizedObject.firstChunk.pos, 100f) && !deer.room.GetWorldCoordinate(goToPuffBall.representedItem.realizedObject.firstChunk.pos).CompareDisregardingNode(cantEatFromCoordinate))
				{
					deer.EatObject(goToPuffBall.representedItem.realizedObject);
				}
			}
		}
		else if (behavior == Behavior.Kneeling)
		{
			WorldCoordinate pos2 = creature.pos;
			int num = creature.pos.y;
			while (num >= 0 && deer.room.aimap.TileAccessibleToCreature(pos2.x, num, creature.creatureTemplate) && !deer.room.GetTile(pos2.x, num - 4).wormGrass && deer.room.aimap.getAItile(pos2.x, num).smoothedFloorAltitude > 5)
			{
				pos2.y = num;
				num--;
			}
			creature.abstractAI.SetDestination(pos2);
		}
		else if (behavior == Behavior.GetUnstuck)
		{
			creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
		}
		if ((double)deer.GetUnstuckForce > 0.5 && deer.CloseToEdge < 0.5f && base.pathFinder.GetDestination.TileDefined && base.pathFinder.GetDestination.room == deer.room.abstractRoom.index)
		{
			creature.abstractAI.SetDestination(base.pathFinder.GetDestination + Custom.fourDirections[UnityEngine.Random.Range(0, 4)]);
		}
	}

	private bool LegalInRoomDest(WorldCoordinate rndm)
	{
		if (rndm.room != creature.pos.room || !rndm.TileDefined)
		{
			return false;
		}
		if (!base.pathFinder.CoordinateReachableAndGetbackable(rndm))
		{
			return false;
		}
		if (Custom.DistLess(creature.pos.Tile, rndm.Tile, 10f))
		{
			return false;
		}
		if (rndm.x < ((deer.playersInAntlers.Count > 0) ? 15 : 5) || rndm.x > deer.room.TileWidth - ((deer.playersInAntlers.Count > 0) ? 15 : 5))
		{
			return false;
		}
		return true;
	}

	private bool GoodInRoomDest(WorldCoordinate rndm)
	{
		if (!LegalInRoomDest(rndm))
		{
			return false;
		}
		if (deer.playersInAntlers.Count > 0 && deer.room.TileWidth > 180)
		{
			if (Math.Abs(creature.pos.Tile.x - rndm.Tile.x) < deer.room.TileWidth / 3)
			{
				return false;
			}
			if (rndm.Tile.x > 70 && rndm.Tile.x < deer.room.TileWidth - 70)
			{
				return false;
			}
		}
		bool num = WormGrassUnderPos(inRoomDestination.Tile);
		bool flag = WormGrassUnderPos(rndm.Tile);
		if (num != flag)
		{
			return !flag;
		}
		return true;
	}

	private bool PuffBallLegal(ItemTracker.ItemRepresentation itemRep)
	{
		if (ModManager.MMF && MMF.cfgDeerBehavior.Value && itemRep.representedItem.realizedObject == null)
		{
			return false;
		}
		for (int i = 0; i < itemRep.representedItem.realizedObject.grabbedBy.Count; i++)
		{
			if (itemRep.representedItem.realizedObject.grabbedBy[i].grabber is Player && (itemRep.representedItem.realizedObject.grabbedBy[i].grabber as Player).playerInAntlers != null)
			{
				return false;
			}
		}
		for (int j = 0; j < base.tracker.CreaturesCount; j++)
		{
			if (base.tracker.GetRep(j).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Deer && base.tracker.GetRep(j).representedCreature.realizedCreature != null && base.tracker.GetRep(j).representedCreature.personality.dominance > deer.abstractCreature.personality.dominance && (base.tracker.GetRep(j).representedCreature.abstractAI.RealAI as DeerAI).goToPuffBall != null && (base.tracker.GetRep(j).representedCreature.abstractAI.RealAI as DeerAI).goToPuffBall.representedItem == itemRep.representedItem)
			{
				return false;
			}
		}
		if (!deniedPuffballs.Contains(itemRep.representedItem.ID))
		{
			if (itemRep.representedItem.realizedObject != null)
			{
				return itemRep.representedItem.realizedObject.grabbedBy.Count <= 0;
			}
			return true;
		}
		return false;
	}

	private void FindGotoPuffBall()
	{
		float num = float.MaxValue;
		if (goToPuffBall != null && !PuffBallLegal(goToPuffBall))
		{
			goToPuffBall = null;
		}
		for (int i = 0; i < base.itemTracker.ItemCount; i++)
		{
			float num2 = Vector2.Distance(deer.mainBodyChunk.pos, deer.room.MiddleOfTile(base.itemTracker.GetRep(i).BestGuessForPosition()));
			if (base.itemTracker.GetRep(i).representedItem.realizedObject != null && base.itemTracker.GetRep(i).representedItem.realizedObject.grabbedBy.Count > 0)
			{
				num2 = num2 * 2f + 700f;
			}
			if (base.itemTracker.GetRep(i).VisualContact)
			{
				num2 -= base.itemTracker.GetRep(i).representedItem.realizedObject.firstChunk.vel.magnitude * 5f;
			}
			if (base.itemTracker.GetRep(i) == goToPuffBall)
			{
				num2 -= 300f;
			}
			if (!base.pathFinder.CoordinateReachableAndGetbackable(base.itemTracker.GetRep(i).BestGuessForPosition() + new IntVector2(0, 2)))
			{
				num2 += 3000f;
			}
			if (base.itemTracker.GetRep(i).BestGuessForPosition().CompareDisregardingNode(cantEatFromCoordinate))
			{
				num2 = float.MaxValue;
			}
			if (num2 < num && PuffBallLegal(base.itemTracker.GetRep(i)))
			{
				num = num2;
				goToPuffBall = base.itemTracker.GetRep(i);
			}
		}
	}

	private void CheckRestSpot(IntVector2 testSpot)
	{
		if ((ModManager.MMF && MMF.cfgDeerBehavior.Value && (testSpot.x < 31 || testSpot.x >= deer.room.TileWidth - 30)) || UnityEngine.Random.value > 1f / 6f || deer.room.aimap.getAItile(testSpot).floorAltitude > deer.room.TileHeight)
		{
			return;
		}
		testSpot.y -= deer.room.aimap.getAItile(testSpot).floorAltitude - 1;
		if (deer.room.aimap.getAItile(testSpot).acc != AItile.Accessibility.Floor)
		{
			return;
		}
		int num = testSpot.x;
		int num2 = testSpot.x;
		for (int i = 0; i < 20; i++)
		{
			if (deer.room.aimap.getAItile(num - 1, testSpot.y).acc != AItile.Accessibility.Floor)
			{
				break;
			}
			num--;
		}
		for (int j = 0; j < 20; j++)
		{
			if (deer.room.aimap.getAItile(num2 + 1, testSpot.y).acc != AItile.Accessibility.Floor)
			{
				break;
			}
			num2++;
		}
		if (num2 - num <= 7)
		{
			return;
		}
		if (wormGrass != null)
		{
			for (int k = num; k <= num2; k++)
			{
				for (int l = 0; l < wormGrass.patches.Count; l++)
				{
					for (int m = 0; m < wormGrass.patches[l].tiles.Count; m++)
					{
						if (wormGrass.patches[l].tiles[m].x == k && Math.Abs(testSpot.y - wormGrass.patches[l].tiles[m].y) < 5)
						{
							return;
						}
					}
				}
			}
		}
		restPos = new WorldCoordinate(deer.room.abstractRoom.index, (num + num2) / 2, testSpot.y + 2, -1);
		if (ModManager.MMF && MMF.cfgDeerBehavior.Value && restPos.HasValue)
		{
			IntVector2[] shortcutsIndex = creature.Room.realizedRoom.shortcutsIndex;
			for (int n = 0; n < shortcutsIndex.Length; n++)
			{
				IntVector2 intVector = shortcutsIndex[n];
				if (Vector2.Distance(intVector.ToVector2(), new Vector2(restPos.Value.x, restPos.Value.y)) < 3f)
				{
					Custom.Log("Deer rest at:", new Vector2(restPos.Value.x, restPos.Value.y).ToString());
					Custom.Log("Rest cancel, was over door! at:", intVector.ToVector2().ToString());
					restPos = null;
					break;
				}
			}
		}
		if (ModManager.MMF && MMF.cfgDeerBehavior.Value && restPos.HasValue)
		{
			for (int num3 = 0; num3 < base.tracker.CreaturesCount; num3++)
			{
				if (base.tracker.GetRep(num3).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Deer && base.tracker.GetRep(num3).representedCreature.personality.dominance > creature.personality.dominance && base.tracker.GetRep(num3).representedCreature.realizedCreature != null && base.tracker.GetRep(num3).representedCreature.realizedCreature.room == deer.room)
				{
					if (!(Custom.WorldCoordFloatDist(restPos.Value, base.tracker.GetRep(num3).BestGuessForPosition()) < 10f) && (!(base.tracker.GetRep(num3).representedCreature.realizedCreature as Deer).AI.restPos.HasValue || !(Custom.WorldCoordFloatDist(restPos.Value, (base.tracker.GetRep(num3).representedCreature.realizedCreature as Deer).AI.restPos.Value) < 10f)))
					{
						break;
					}
					restPos = null;
					return;
				}
			}
		}
		for (int num4 = 0; num4 < base.tracker.CreaturesCount; num4++)
		{
			if (base.tracker.GetRep(num4).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Deer && base.tracker.GetRep(num4).representedCreature.personality.dominance > creature.personality.dominance && base.tracker.GetRep(num4).representedCreature.realizedCreature != null && base.tracker.GetRep(num4).representedCreature.realizedCreature.room == deer.room)
			{
				if (Custom.WorldCoordFloatDist(restPos.Value, base.tracker.GetRep(num4).BestGuessForPosition()) < 10f || ((base.tracker.GetRep(num4).representedCreature.realizedCreature as Deer).AI.restPos.HasValue && Custom.WorldCoordFloatDist(restPos.Value, (base.tracker.GetRep(num4).representedCreature.realizedCreature as Deer).AI.restPos.Value) < 10f))
				{
					restPos = null;
				}
				break;
			}
		}
	}

	private bool WormGrassUnderPos(IntVector2 testPos)
	{
		for (int num = testPos.y; num >= 0; num--)
		{
			if (deer.room.GetTile(testPos.x, num).Solid)
			{
				return false;
			}
			if (deer.room.GetTile(testPos.x, num).wormGrass)
			{
				return true;
			}
		}
		return false;
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed);
	}

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		if (!AllowMovementBetweenRooms && (connection.type == MovementConnection.MovementType.OutsideRoom || connection.startCoord.room != connection.destinationCoord.room))
		{
			return new PathCost(0f, PathCost.Legality.Unallowed);
		}
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Deer && base.tracker.GetRep(i).BestGuessForPosition().room == deer.room.abstractRoom.index && Custom.DistLess(connection.DestTile, base.tracker.GetRep(i).BestGuessForPosition().Tile, 7f))
			{
				cost.resistance += 250f;
			}
		}
		float num = 0f;
		if (connection.destinationCoord.room == deer.room.abstractRoom.index && connection.destinationCoord.TileDefined)
		{
			num = Mathf.Max(Mathf.InverseLerp(10f, 0f, connection.DestTile.x), Mathf.InverseLerp(deer.room.TileWidth - 10, deer.room.TileWidth, connection.DestTile.x));
		}
		if (num > 0f)
		{
			cost.resistance += Mathf.InverseLerp(6f, 3f, Math.Max(deer.room.aimap.getAItile(connection.destinationCoord).floorAltitude, deer.room.aimap.getAItile(connection.destinationCoord).smoothedFloorAltitude)) * 100f * num;
			cost.resistance += Mathf.InverseLerp(deer.room.TileHeight - 3, deer.room.TileHeight - 1, connection.destinationCoord.y) * 100f * num;
		}
		return new PathCost(cost.resistance + Mathf.Clamp(Mathf.Abs(deer.preferredHeight - (float)deer.room.aimap.getTerrainProximity(connection.destinationCoord)), 0f, 15f) * 0.3f, cost.legality);
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return null;
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		return null;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		return StaticRelationship(dRelation.trackerRep.representedCreature);
	}

	public bool TrackItem(AbstractPhysicalObject obj)
	{
		return obj.type == AbstractPhysicalObject.AbstractObjectType.PuffBall;
	}

	public void SeeThrownWeapon(PhysicalObject obj, Creature thrower)
	{
	}

	public WorldCoordinate IdleRoomWanderGoal()
	{
		if (deer.room == null || deer.playersInAntlers == null)
		{
			return deer.abstractCreature.pos;
		}
		float minInclusive = ((deer.playersInAntlers.Count == 0) ? 0.25f : 0.85f);
		float maxInclusive = ((deer.playersInAntlers.Count == 0) ? 0.5f : 1f);
		float num = deer.flipDir * -1f;
		if (deer.playersInAntlers.Count == 0 && UnityEngine.Random.value < 0.4f)
		{
			num = deer.flipDir;
		}
		int num2 = UnityEngine.Random.Range(16, 25);
		WorldCoordinate wanderCoordFromXPosition;
		do
		{
			int num3 = Mathf.Clamp((int)((float)deer.abstractCreature.pos.x + num * ((float)deer.room.TileWidth * UnityEngine.Random.Range(minInclusive, maxInclusive))), num2, deer.room.TileWidth - num2);
			if (Mathf.Abs(deer.abstractCreature.pos.x - num3) < 22)
			{
				num *= -1f;
				num3 = Mathf.Clamp((int)((float)deer.abstractCreature.pos.x + num * ((float)deer.room.TileWidth * UnityEngine.Random.Range(minInclusive, maxInclusive))), num2, deer.room.TileWidth - num2);
			}
			wanderCoordFromXPosition = GetWanderCoordFromXPosition(num3);
			num2 += 7;
		}
		while (num2 <= 80 && deer.room.GetTile(wanderCoordFromXPosition).Solid);
		return wanderCoordFromXPosition;
	}

	public WorldCoordinate GetWanderCoordFromXPosition(int tileX)
	{
		int i = 0;
		int num = deer.abstractCreature.pos.y;
		for (; i < 2000; i += 20)
		{
			if (LegalInRoomDest(deer.room.GetWorldCoordinate(new Vector2((float)tileX * 20f, i))))
			{
				num = Mathf.Clamp(i / 20 + (int)deer.preferredHeight + UnityEngine.Random.Range(-15, 15), 0, deer.room.TileHeight - 2);
				if (GoodInRoomDest(deer.room.GetWorldCoordinate(new Vector2((float)tileX * 20f, (float)num * 20f))))
				{
					break;
				}
			}
		}
		return new WorldCoordinate(deer.abstractCreature.pos.room, tileX, num, deer.abstractCreature.pos.abstractNode);
	}
}
