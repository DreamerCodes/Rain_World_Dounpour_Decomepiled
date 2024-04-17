using System;
using System.Collections.Generic;
using MoreSlugcats;
using Noise;
using RWCustom;
using UnityEngine;

public class ScavengerAI : ArtificialIntelligence, IUseARelationshipTracker, IUseItemTracker, IAINoiseReaction, IReactToSocialEvents
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Attack = new Behavior("Attack", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior Injured = new Behavior("Injured", register: true);

		public static readonly Behavior Scavange = new Behavior("Scavange", register: true);

		public static readonly Behavior Travel = new Behavior("Travel", register: true);

		public static readonly Behavior Investigate = new Behavior("Investigate", register: true);

		public static readonly Behavior FindPackLeader = new Behavior("FindPackLeader", register: true);

		public static readonly Behavior LeaveRoom = new Behavior("LeaveRoom", register: true);

		public static readonly Behavior GuardOutpost = new Behavior("GuardOutpost", register: true);

		public static readonly Behavior CommunicateWithPlayer = new Behavior("CommunicateWithPlayer", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class ViolenceType : ExtEnum<ViolenceType>
	{
		public static readonly ViolenceType None = new ViolenceType("None", register: true);

		public static readonly ViolenceType ForFun = new ViolenceType("ForFun", register: true);

		public static readonly ViolenceType Warning = new ViolenceType("Warning", register: true);

		public static readonly ViolenceType NonLethal = new ViolenceType("NonLethal", register: true);

		public static readonly ViolenceType Lethal = new ViolenceType("Lethal", register: true);

		public ViolenceType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class ScavengerTrackState : RelationshipTracker.TrackedCreatureState
	{
		public bool armed;

		public bool jawsOccupied;

		public bool holdingAFriend;

		public int hitsOnThisCreature;

		public int throwsTowardsThisCreature;

		public int bumpedByThisCreature;

		public int consideredWarnedCounter;

		public int mostValuableItem;

		public float moving;

		public float gettingCloser;

		public float lastDistance;

		public float smoothedMoving;

		public float smoothedGettingCloser;

		public ViolenceType taggedViolenceType = ViolenceType.None;
	}

	public class CommunicationModule : AIModule
	{
		public class CommunicationRecord
		{
			public EntityID playerID;

			public Scavenger.ScavengerAnimation.ID animationID;

			public CommunicationRecord(EntityID playerID, Scavenger.ScavengerAnimation.ID animationID)
			{
				this.playerID = playerID;
				this.animationID = animationID;
			}
		}

		public ScavengerAI scavAI;

		public List<CommunicationRecord> communicationHistory;

		public Tracker.CreatureRepresentation target;

		public Scavenger.ScavengerAnimation.ID nextAnim;

		public Scavenger.ScavengerAnimation playingAnim;

		private IntVector2 communicationSpot;

		private IntVector2 tempCommunicationSpot;

		private float currentSpotScore = float.MinValue;

		private int targetMovingCounter;

		public WorldCoordinate? MoveToSpot
		{
			get
			{
				if (currentSpotScore < -1000f)
				{
					return null;
				}
				return scavAI.scavenger.room.GetWorldCoordinate(communicationSpot);
			}
		}

		public CommunicationModule(ScavengerAI scavAI)
			: base(scavAI)
		{
			this.scavAI = scavAI;
			communicationHistory = new List<CommunicationRecord>();
		}

		public override void Update()
		{
			if (playingAnim != null)
			{
				if (playingAnim.Continue)
				{
					return;
				}
				target = null;
				Custom.Log($"anim finished {playingAnim.id}");
				AddAnimationToRecord((playingAnim as Scavenger.CommunicationAnimation).creatureRep.representedCreature.ID, playingAnim.id);
				playingAnim = null;
			}
			if (target != null && target.representedCreature.realizedCreature != null && target.representedCreature.realizedCreature.room == scavAI.scavenger.room)
			{
				IntVector2 tilePosition = scavAI.scavenger.room.GetTilePosition(target.representedCreature.realizedCreature.DangerPos + Custom.RNV() * 400f * UnityEngine.Random.value);
				if (CommunicationSpotScore(tilePosition) > CommunicationSpotScore(tempCommunicationSpot))
				{
					tempCommunicationSpot = tilePosition;
				}
				currentSpotScore = CommunicationSpotScore(communicationSpot);
				if (CommunicationSpotScore(tempCommunicationSpot) > currentSpotScore + 40f)
				{
					communicationSpot = tempCommunicationSpot;
				}
				if ((target.dynamicRelationship.state as ScavengerTrackState).smoothedMoving > 2f)
				{
					targetMovingCounter++;
				}
				else
				{
					targetMovingCounter = Math.Max(0, targetMovingCounter - 2);
				}
				if (!scavAI.scavenger.safariControlled && scavAI.behavior == Behavior.CommunicateWithPlayer && currentSpotScore > -1000f && scavAI.scavenger.occupyTile.FloatDist(communicationSpot) < 4f)
				{
					InitiateAnimation();
				}
				EvaluateCommunicationDemand(target);
			}
			else
			{
				target = null;
			}
			if (target == null && AI.tracker.CreaturesCount > 0)
			{
				Tracker.CreatureRepresentation rep = AI.tracker.GetRep(UnityEngine.Random.Range(0, AI.tracker.CreaturesCount));
				if (rep.dynamicRelationship != null && rep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
				{
					EvaluateCommunicationDemand(rep);
				}
			}
		}

		public override float Utility()
		{
			if (playingAnim != null)
			{
				return 1f;
			}
			if (target == null)
			{
				return 0f;
			}
			if (currentSpotScore < -1000f)
			{
				return 0f;
			}
			return Mathf.InverseLerp(200f, 0f, targetMovingCounter);
		}

		private void EvaluateCommunicationDemand(Tracker.CreatureRepresentation playerRepresentation)
		{
			target = null;
			for (int i = 0; i < scavAI.tracker.CreaturesCount; i++)
			{
				if (scavAI.tracker.GetRep(i).representedCreature.realizedCreature != null && scavAI.tracker.GetRep(i).representedCreature.personality.dominance > scavAI.creature.personality.dominance && scavAI.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger && (scavAI.tracker.GetRep(i).representedCreature.realizedCreature as Scavenger).AI.communicationModule.target != null && (scavAI.tracker.GetRep(i).representedCreature.realizedCreature as Scavenger).AI.communicationModule.target.representedCreature == playerRepresentation.representedCreature)
				{
					return;
				}
			}
			Scavenger.ScavengerAnimation.ID iD = null;
			bool flag = ModManager.MSC && playerRepresentation.representedCreature.realizedCreature != null && (playerRepresentation.representedCreature.realizedCreature as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer;
			if (scavAI.outpostModule.outpost != null)
			{
				for (int j = 0; j < scavAI.outpostModule.outpost.playerTrackers.Count; j++)
				{
					if (scavAI.outpostModule.outpost.playerTrackers[j].player == playerRepresentation.representedCreature)
					{
						if (scavAI.outpostModule.outpost.playerTrackers[j].AllowedToPass && !flag)
						{
							iD = Scavenger.ScavengerAnimation.ID.PlayerMayPass;
						}
						else if ((ModManager.MMF && scavAI.outpostModule.outpost.playerTrackers[j].killOrder) || (!ModManager.MMF && !scavAI.outpostModule.outpost.playerTrackers[j].killOrder))
						{
							iD = Scavenger.ScavengerAnimation.ID.BackOff;
						}
						else if (playerRepresentation.dynamicRelationship != null && (playerRepresentation.dynamicRelationship.state as ScavengerTrackState).mostValuableItem > 9)
						{
							iD = Scavenger.ScavengerAnimation.ID.WantToTrade;
						}
						break;
					}
				}
			}
			else if (playerRepresentation == scavAI.wantToTradeWith)
			{
				iD = Scavenger.ScavengerAnimation.ID.WantToTrade;
			}
			if (iD == null)
			{
				return;
			}
			for (int k = 0; k < communicationHistory.Count; k++)
			{
				if (communicationHistory[k].playerID == playerRepresentation.representedCreature.ID && communicationHistory[k].animationID == iD)
				{
					return;
				}
			}
			target = playerRepresentation;
			nextAnim = iD;
		}

		private float CommunicationSpotScore(IntVector2 test)
		{
			if (scavAI.scavenger.room.aimap.getAItile(test).acc != AItile.Accessibility.Floor || !Custom.DistLess(target.representedCreature.realizedCreature.DangerPos, scavAI.scavenger.room.MiddleOfTile(test), 400f) || !scavAI.pathFinder.CoordinateReachableAndGetbackable(scavAI.scavenger.room.GetWorldCoordinate(test)))
			{
				return float.MinValue;
			}
			float num = 0f;
			for (int i = -1; i < 2; i++)
			{
				if (!scavAI.scavenger.room.GetTile(test.x + i, test.y).Solid && scavAI.scavenger.room.GetTile(test.x + i, test.y - 1).Solid)
				{
					num += 50f;
				}
			}
			num -= Mathf.Abs(Vector2.Distance(target.representedCreature.realizedCreature.DangerPos, scavAI.scavenger.room.MiddleOfTile(test)) - 200f);
			num += Mathf.Max(0f, Mathf.Abs(target.representedCreature.realizedCreature.DangerPos.y - scavAI.scavenger.room.MiddleOfTile(test).y) - 100f) * 0.5f;
			if (Custom.DistLess(target.representedCreature.realizedCreature.DangerPos, scavAI.scavenger.room.MiddleOfTile(test), 400f) && scavAI.scavenger.room.VisualContact(target.representedCreature.realizedCreature.DangerPos, scavAI.scavenger.room.MiddleOfTile(test)))
			{
				num += 500f;
			}
			if (scavAI.scavenger.room.MiddleOfTile(test).x < target.representedCreature.realizedCreature.DangerPos.x != scavAI.scavenger.DangerPos.x < target.representedCreature.realizedCreature.DangerPos.x)
			{
				num -= 75f;
			}
			if (scavAI.outpostModule.outpost != null && Custom.InRange(scavAI.scavenger.room.MiddleOfTile(test).x, target.representedCreature.realizedCreature.DangerPos.x, scavAI.outpostModule.outpost.placedObj.pos.x))
			{
				num += 50f;
			}
			return num;
		}

		private void InitiateAnimation()
		{
			if (nextAnim == Scavenger.ScavengerAnimation.ID.BackOff)
			{
				playingAnim = new Scavenger.BackOffAnimation(scavAI.scavenger, target, target.representedCreature.realizedCreature.DangerPos);
			}
			else if (nextAnim == Scavenger.ScavengerAnimation.ID.PlayerMayPass)
			{
				if (scavAI.outpostModule.outpost != null)
				{
					playingAnim = new Scavenger.PlayerMayPassAnimation(scavAI.scavenger, target, target.representedCreature.realizedCreature.DangerPos, scavAI.outpostModule.outpost);
				}
			}
			else if (nextAnim == Scavenger.ScavengerAnimation.ID.WantToTrade)
			{
				PhysicalObject desiredItem = null;
				int num = 0;
				if (target.representedCreature.realizedCreature != null)
				{
					for (int i = 0; i < target.representedCreature.realizedCreature.grasps.Length; i++)
					{
						if (target.representedCreature.realizedCreature.grasps[i] != null && scavAI.CollectScore(target.representedCreature.realizedCreature.grasps[i].grabbed, weaponFiltered: false) > num)
						{
							num = scavAI.CollectScore(target.representedCreature.realizedCreature.grasps[i].grabbed, weaponFiltered: false);
							desiredItem = target.representedCreature.realizedCreature.grasps[i].grabbed;
						}
					}
				}
				if (scavAI.DoIWantToTrade(num))
				{
					playingAnim = new Scavenger.WantToTradeAnimation(scavAI.scavenger, target, target.representedCreature.realizedCreature.DangerPos, desiredItem);
				}
			}
			scavAI.scavenger.animation = playingAnim;
		}

		private void AddAnimationToRecord(EntityID targetID, Scavenger.ScavengerAnimation.ID animID)
		{
			CommunicationRecord item = new CommunicationRecord(targetID, animID);
			communicationHistory.Add(item);
			for (int i = 0; i < scavAI.tracker.CreaturesCount; i++)
			{
				if (scavAI.tracker.GetRep(i).representedCreature.realizedCreature != null && scavAI.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger)
				{
					(scavAI.tracker.GetRep(i).representedCreature.realizedCreature as Scavenger).AI.communicationModule.communicationHistory.Add(item);
				}
			}
		}
	}

	public Scavenger scavenger;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	private DebugSprite dbSpr;

	private DebugSprite dbSpr2;

	public WorldCoordinate testIdlePos;

	public WorldCoordinate testThrowPos;

	public List<WorldCoordinate> alreadyIdledAt;

	public int idleCounter;

	public int scavageItemCheck;

	public Vector2 testLookPos;

	public Vector2 alreadyLookedAtPos;

	public float currentUtility;

	public float scared;

	public float runSpeedGoal;

	public int arrangeInventoryCounter;

	public Tracker.CreatureRepresentation focusCreature;

	public ItemTracker.ItemRepresentation scavengeCandidate;

	public AbstractPhysicalObject giftForMe;

	public Tracker.CreatureRepresentation wantToTradeWith;

	public float agitation;

	public List<IntVector2> previousAttackPositions;

	public int changeAttackPositionDelay;

	public float discomfortWithOtherCreatures;

	public int ticksSinceSeenPackLeader;

	public int age;

	public float backedByPack;

	public int seenSquadLeaderInRoom = -1;

	public int goToSquadLeaderFirstTime;

	public ScavengerOutpost.GuardOutpostModule outpostModule;

	public CommunicationModule communicationModule;

	public ScavengerTradeSpot tradeSpot;

	public Behavior behavior;

	public ViolenceType currentViolenceType;

	public int noiseReactDelay;

	public float filteredLikeB = float.NaN;

	public float tempLikeB;

	public float likeB;

	private List<IntVector2> creatureMovementArea = new List<IntVector2>(50);

	private List<IntVector2> myMovementArea = new List<IntVector2>(50);

	private WorldCoordinate? lockedSafariDoorEntry;

	public int lastPickupPressed;

	public int pickupDownTime;

	public int numPickupPressed;

	public int controlWalkCooldown;

	public int controlStuckTime;

	public bool NeedAWeapon
	{
		get
		{
			if (!(behavior == Behavior.Attack) && !(behavior == Behavior.Flee))
			{
				return scared > 0.5f;
			}
			return true;
		}
	}

	public bool HoldWeapon
	{
		get
		{
			if (!(behavior == Behavior.Attack) && !(behavior == Behavior.Flee) && !(scared > 0.3f))
			{
				return ActNervous > 0.8f;
			}
			return true;
		}
	}

	public float ActNervous => Mathf.Lerp(creature.personality.nervous, agitation, agitation * 0.5f);

	public ScavengerAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		scavenger = creature.realizedCreature as Scavenger;
		scavenger.AI = this;
		AddModule(new StandardPather(this, world, creature));
		base.pathFinder.stepsPerFrame = 40;
		(base.pathFinder as StandardPather).savedPastConnections = 0;
		AddModule(new Tracker(this, 10, 10, -1, 0.5f, 5, 5, 10));
		AddModule(new ItemTracker(this, 10, 10, -1, -1, stopTrackingCarried: true));
		AddModule(new NoiseTracker(this, base.tracker));
		if (scavenger.Elite)
		{
			AddModule(new PreyTracker(this, 10, 1f, 40f, 450f, 0.5f));
		}
		else
		{
			AddModule(new PreyTracker(this, 10, 1f, 5f, 150f, 0.5f));
		}
		AddModule(new ThreatTracker(this, 10));
		base.threatTracker.accessibilityConsideration = 7.5f;
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new UtilityComparer(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new DiscomfortTracker(this, base.tracker, Mathf.InverseLerp(0.3f, 0.2f, creature.personality.bravery)));
		AddModule(new InjuryTracker(this, 0.01f));
		communicationModule = new CommunicationModule(this);
		AddModule(communicationModule);
		outpostModule = new ScavengerOutpost.GuardOutpostModule(this);
		AddModule(outpostModule);
		base.utilityComparer.AddComparedModule(base.threatTracker, new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 1f / 30f), 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.preyTracker, new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 1f / 30f), 0.5f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		float weight = Mathf.Lerp(0.1f * creature.personality.bravery, 0.3f, Mathf.Pow(Mathf.InverseLerp(0.25f, 1f, creature.personality.bravery), 1.5f));
		base.utilityComparer.AddComparedModule(base.noiseTracker, new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.002f), weight, 1.1f);
		base.utilityComparer.GetUtilityTracker(base.noiseTracker).exponent = 0.5f;
		base.utilityComparer.AddComparedModule(outpostModule, new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.0125f), 0.9f, 1.1f);
		base.utilityComparer.AddComparedModule(base.injuryTracker, new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 1f / 30f), 1f, 1.1f);
		base.utilityComparer.AddComparedModule(communicationModule, new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 1f / 30f), 0.4f, 1.1f);
		alreadyIdledAt = new List<WorldCoordinate>();
		behavior = Behavior.Idle;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		previousAttackPositions = new List<IntVector2>();
		testIdlePos = new WorldCoordinate(room.abstractRoom.index, UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.TileHeight), -1);
		testThrowPos = new WorldCoordinate(room.abstractRoom.index, UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.TileHeight), -1);
	}

	public override void Update()
	{
		if (behavior == Behavior.Attack && !RainWorldGame.RequestHeavyAi(scavenger))
		{
			return;
		}
		base.Update();
		if (noiseReactDelay > 0)
		{
			noiseReactDelay--;
		}
		if (ModManager.MSC)
		{
			if (scavenger.LickedByPlayer != null)
			{
				base.tracker.SeeCreature(scavenger.LickedByPlayer.abstractCreature);
				if (scared < 1f)
				{
					scared += 0.0125f;
				}
				if (agitation < 1f)
				{
					agitation += 0.0213f;
				}
			}
			if (scavenger.King)
			{
				if ((creature.abstractAI as ScavengerAbstractAI).squad != null)
				{
					(creature.abstractAI as ScavengerAbstractAI).squad.RemoveMember(scavenger.abstractCreature);
					(creature.abstractAI as ScavengerAbstractAI).squad = null;
				}
				backedByPack = 0f;
				tradeSpot = null;
				wantToTradeWith = null;
				giftForMe = null;
			}
			creature.abstractAI.freezeDestination = false;
			if (scavenger.room != null && scavenger.room.abstractRoom.name == "LC_FINAL")
			{
				if (scavenger.King)
				{
					int x = UnityEngine.Random.Range(70, 80);
					if (base.pathFinder.GetDestination.x < 70)
					{
						WorldCoordinate getDestination = base.pathFinder.GetDestination;
						getDestination.x = x;
						base.pathFinder.SetDestination(getDestination);
						testIdlePos = getDestination;
					}
					if (creature.abstractAI.destination.x < 70)
					{
						WorldCoordinate destination = creature.abstractAI.destination;
						destination.x = x;
						creature.abstractAI.SetDestination(destination);
						testIdlePos = destination;
						creature.abstractAI.freezeDestination = true;
					}
				}
				else
				{
					int x2 = UnityEngine.Random.Range(6, 50);
					if (base.pathFinder.GetDestination.x > 50)
					{
						WorldCoordinate getDestination2 = base.pathFinder.GetDestination;
						getDestination2.x = x2;
						base.pathFinder.SetDestination(getDestination2);
						testIdlePos = getDestination2;
					}
					if (creature.abstractAI.destination.x > 50)
					{
						WorldCoordinate destination2 = creature.abstractAI.destination;
						destination2.x = x2;
						creature.abstractAI.SetDestination(destination2);
						testIdlePos = destination2;
						creature.abstractAI.freezeDestination = true;
					}
				}
			}
		}
		else
		{
			creature.abstractAI.freezeDestination = false;
		}
		age++;
		if (scavenger.safariControlled)
		{
			ControlledBehavior();
			return;
		}
		creature.abstractAI.AbstractBehavior(1);
		if ((creature.abstractAI as ScavengerAbstractAI).UnderSquadLeaderControl && seenSquadLeaderInRoom != (creature.abstractAI as ScavengerAbstractAI).squad.leader.pos.room && creature.pos.room == (creature.abstractAI as ScavengerAbstractAI).squad.leader.pos.room && (creature.abstractAI as ScavengerAbstractAI).squad.leader.realizedCreature != null)
		{
			if ((creature.abstractAI as ScavengerAbstractAI).squad.leader.realizedCreature.room == scavenger.room)
			{
				base.tracker.SeeCreature((creature.abstractAI as ScavengerAbstractAI).squad.leader);
				seenSquadLeaderInRoom = creature.pos.room;
				goToSquadLeaderFirstTime = 1000;
				Custom.Log("SEE squad leader in room");
			}
			if (goToSquadLeaderFirstTime > 0)
			{
				goToSquadLeaderFirstTime--;
				Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForObject((creature.abstractAI as ScavengerAbstractAI).squad.leader.realizedCreature, AddIfMissing: false);
				if (creatureRepresentation != null && creatureRepresentation.VisualContact)
				{
					goToSquadLeaderFirstTime = 0;
					Custom.Log("See squad leader");
				}
			}
		}
		base.noiseTracker.hearingSkill = (scavenger.moving ? 0.8f : Mathf.Lerp(1.4f, 0.9f, agitation));
		for (int i = 0; i < creature.state.socialMemory.relationShips.Count; i++)
		{
			creature.state.socialMemory.relationShips[i].EvenOutTemps(0.0005f);
			if (creature.state.socialMemory.relationShips[i].subjectID.number == 0)
			{
				likeB = creature.state.socialMemory.relationShips[i].like;
				tempLikeB = creature.state.socialMemory.relationShips[i].tempLike;
			}
		}
		arrangeInventoryCounter--;
		if (arrangeInventoryCounter < 0)
		{
			bool flag = scavenger.ArrangeInventory();
			arrangeInventoryCounter = UnityEngine.Random.Range(10, (!scavenger.moving || flag) ? 20 : 400);
		}
		if (scavenger.room == null)
		{
			return;
		}
		base.pathFinder.walkPastPointOfNoReturn = stranded || !base.denFinder.GetDenPosition().HasValue || !base.pathFinder.CoordinatePossibleToGetBackFrom(base.denFinder.GetDenPosition().Value);
		DecideBehavior();
		UpdateLookPoint();
		scavageItemCheck--;
		if (giftForMe != null)
		{
			scavageItemCheck = Math.Min(scavageItemCheck, 20);
			if (giftForMe.realizedObject == null || giftForMe.Room != scavenger.room.abstractRoom || giftForMe.realizedObject.grabbedBy.Count > 0)
			{
				giftForMe = null;
				Custom.Log("scav lost gift");
			}
		}
		if (scavageItemCheck < 1)
		{
			CheckForScavangeItems(conservativeBias: true);
			scavageItemCheck = UnityEngine.Random.Range(40, 200);
		}
		if (scavengeCandidate != null)
		{
			if (scavengeCandidate.BestGuessForPosition().Tile.FloatDist(creature.pos.Tile) < 4f)
			{
				scavenger.LookForItemsToPickUp();
			}
			if (CollectScore(scavengeCandidate, weaponFiltered: true) < 1 || scavengeCandidate.deleteMeNextFrame || scavengeCandidate.BestGuessForPosition().room != creature.pos.room)
			{
				scavengeCandidate = null;
				CheckForScavangeItems(conservativeBias: false);
			}
		}
		float a = 0f;
		UpdateCurrentViolenceType();
		if (behavior == Behavior.Idle)
		{
			a = discomfortWithOtherCreatures * 0.5f;
			runSpeedGoal = creature.personality.energy * 0.3f + creature.personality.nervous * 0.7f;
			runSpeedGoal = Mathf.Lerp(runSpeedGoal, 1f, discomfortWithOtherCreatures * 0.5f);
			IdleBehavior();
		}
		else if (behavior == Behavior.Travel)
		{
			if (base.pathFinder.CoordinateViable(creature.abstractAI.MigrationDestination))
			{
				runSpeedGoal = 0.2f + creature.personality.energy * 0.6f + agitation * 0.2f;
				if ((creature.abstractAI as ScavengerAbstractAI).GhostOutOfCurrentRoom)
				{
					runSpeedGoal = 1f;
				}
				creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
			}
		}
		else if (behavior == Behavior.Attack)
		{
			runSpeedGoal = 1f;
			AttackBehavior();
			a = 1f;
		}
		else if (behavior == Behavior.Flee)
		{
			runSpeedGoal = 1f;
			focusCreature = base.threatTracker.mostThreateningCreature;
			creature.abstractAI.SetDestination(base.threatTracker.FleeTo(scavenger.room.GetWorldCoordinate(scavenger.occupyTile), 10, 30, outpostModule.outpost == null || (creature.abstractAI as ScavengerAbstractAI).GhostOutOfCurrentRoom, (float)age > 900f * Mathf.InverseLerp(0.2f, 0f, (creature.abstractAI as ScavengerAbstractAI).Shyness)));
			if (UnityEngine.Random.value < 0.25f)
			{
				CheckThrow();
			}
		}
		else if (behavior == Behavior.EscapeRain || behavior == Behavior.Injured)
		{
			runSpeedGoal = 0.7f;
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			a = 0.5f;
			if (UnityEngine.Random.value < 0.25f)
			{
				CheckThrow();
			}
		}
		else if (behavior == Behavior.Scavange)
		{
			runSpeedGoal = 0.7f;
			if (scavengeCandidate != null)
			{
				if (scavengeCandidate.representedItem.realizedObject != null)
				{
					creature.abstractAI.SetDestination(scavengeCandidate.BestGuessForPosition());
					if (Custom.DistLess(scavenger.mainBodyChunk.pos, scavengeCandidate.representedItem.realizedObject.firstChunk.pos, 50f) && scavenger.room == scavengeCandidate.representedItem.realizedObject.room)
					{
						scavenger.PickUpAndPlaceInInventory(scavengeCandidate.representedItem.realizedObject);
						scavengeCandidate = null;
					}
				}
				else
				{
					scavengeCandidate = null;
				}
			}
		}
		else if (behavior == Behavior.Investigate)
		{
			runSpeedGoal = 0.6f;
			if (base.noiseTracker.soundToExamine != null)
			{
				creature.abstractAI.SetDestination(base.noiseTracker.ExaminePos);
			}
		}
		else if (behavior == Behavior.FindPackLeader)
		{
			runSpeedGoal = Mathf.Lerp(1f, 0.5f, Mathf.Pow(creature.personality.bravery, 0.3f));
			a = Mathf.InverseLerp(0.35f, 0f, creature.personality.bravery);
			Tracker.CreatureRepresentation creatureRepresentation2 = PackLeader();
			if (creatureRepresentation2 != null)
			{
				creature.abstractAI.SetDestination(creatureRepresentation2.BestGuessForPosition());
			}
		}
		else if (behavior == Behavior.GuardOutpost)
		{
			runSpeedGoal = 0.85f;
			if (outpostModule.outpost != null && (UnityEngine.Random.value < 0.0005f || base.pathFinder.GetDestination.room != outpostModule.outpost.room.abstractRoom.index || !base.pathFinder.GetDestination.TileDefined || base.pathFinder.GetDestination.Tile.FloatDist(outpostModule.outpost.room.GetTilePosition(outpostModule.outpost.placedObj.pos)) > outpostModule.outpost.Rad / 20f))
			{
				creature.abstractAI.SetDestination(outpostModule.outpost.GoToPos);
			}
		}
		else if (behavior == Behavior.CommunicateWithPlayer && communicationModule.target != null)
		{
			focusCreature = communicationModule.target;
			if (communicationModule.MoveToSpot.HasValue)
			{
				creature.abstractAI.SetDestination(communicationModule.MoveToSpot.Value);
			}
		}
		runSpeedGoal = Mathf.Lerp(runSpeedGoal, Mathf.Max(runSpeedGoal, scared), 0.5f);
		a = Mathf.Max(a, scared);
		a = Mathf.Lerp(a, 1f, base.noiseTracker.Utility() * 0.3f);
		float num = Mathf.Lerp(Mathf.Pow(base.threatTracker.Utility(), Mathf.Lerp(0.2f, 1.8f, creature.personality.bravery)), 1f, base.noiseTracker.Utility() * Mathf.Pow(1f - creature.personality.bravery, 3f) * 0.6f);
		scared = Mathf.Lerp(scared, num, 0.01f);
		if (scared < num)
		{
			scared = Mathf.Min(num, scared + 1f / 30f);
		}
		else
		{
			scared = Mathf.Max(num, scared - 1f / Mathf.Lerp(900f, 30f, Mathf.Pow(creature.personality.bravery, 0.7f)));
		}
		if (agitation < a)
		{
			agitation = Mathf.Min(a, agitation + 1f / (180f * (1f + agitation * 2f)));
		}
		else
		{
			agitation = Mathf.Max(a, agitation - 1f / (Mathf.Lerp(280f, 600f, creature.personality.nervous) * (1f + agitation * 2f)));
		}
		float num2 = 0f;
		for (int j = 0; j < base.tracker.CreaturesCount; j++)
		{
			if (base.tracker.GetRep(j).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack && base.tracker.GetRep(j).representedCreature.state.alive && base.tracker.GetRep(j).BestGuessForPosition().room == creature.pos.room && base.tracker.GetRep(j).BestGuessForPosition().Tile.FloatDist(scavenger.occupyTile) < Mathf.Lerp(10f, 50f, creature.personality.bravery))
			{
				num2 += 1f;
				if (base.tracker.GetRep(j).TicksSinceSeen < 50)
				{
					num2 += 1f;
				}
			}
		}
		num2 = Mathf.Pow(Mathf.InverseLerp(0f, 10f, num2), 0.5f);
		if (backedByPack < num2)
		{
			backedByPack = Mathf.Min(num2, backedByPack + 0.05f);
		}
		else
		{
			backedByPack = Mathf.Max(num2, backedByPack - 1f / Mathf.Lerp(20f, 800f, creature.personality.bravery));
		}
		if (wantToTradeWith != null)
		{
			if (currentUtility < 0.7f)
			{
				focusCreature = wantToTradeWith;
			}
			if (wantToTradeWith.TicksSinceSeen > 400 || wantToTradeWith.dynamicRelationship == null || !DoIWantToTrade((wantToTradeWith.dynamicRelationship.state as ScavengerTrackState).mostValuableItem))
			{
				wantToTradeWith = null;
			}
		}
		if (tradeSpot != null && scavenger.room != tradeSpot.room)
		{
			tradeSpot = null;
		}
	}

	private void DecideBehavior()
	{
		base.utilityComparer.GetUtilityTracker(base.threatTracker).weight = 1f - outpostModule.FearDebuff;
		if (outpostModule.outpost == null)
		{
			base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = 0.5f;
		}
		else
		{
			base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = 0.25f + 0.75f * outpostModule.AngerBuff;
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		float num = currentUtility;
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
				behavior = Behavior.Attack;
			}
			else if (aIModule is NoiseTracker)
			{
				behavior = Behavior.Investigate;
			}
			else if (aIModule is ScavengerOutpost.GuardOutpostModule)
			{
				behavior = Behavior.GuardOutpost;
			}
			else if (aIModule is InjuryTracker)
			{
				behavior = Behavior.Injured;
			}
			else if (aIModule is CommunicationModule)
			{
				behavior = Behavior.CommunicateWithPlayer;
			}
		}
		if (currentUtility < ((outpostModule.outpost != null || tradeSpot != null) ? 0.1f : 0.05f))
		{
			behavior = Behavior.Idle;
			num = 0f;
		}
		if (currentUtility < 0.5f && outpostModule.outpost == null && !scavenger.King)
		{
			Tracker.CreatureRepresentation creatureRepresentation = PackLeader();
			if (creatureRepresentation != null)
			{
				float num2 = Mathf.InverseLerp(Mathf.Lerp(50f, 1800f, Mathf.Pow(creature.personality.bravery * creature.personality.dominance, 1f + scared)), 2300f, creatureRepresentation.TicksSinceSeen) * 0.5f * Mathf.InverseLerp(0.07f, 0.2f, creatureRepresentation.EstimatedChanceOfFinding) * Mathf.Lerp(1f, 0.1f, creature.personality.bravery * creature.personality.dominance);
				if (goToSquadLeaderFirstTime > 0 && (creature.abstractAI as ScavengerAbstractAI).UnderSquadLeaderControl && creatureRepresentation.representedCreature == (creature.abstractAI as ScavengerAbstractAI).squad.leader)
				{
					num2 = Mathf.Max(num2, 0.4f);
				}
				if (creatureRepresentation.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger)
				{
					num2 *= creatureRepresentation.representedCreature.personality.dominance;
				}
				else if (creatureRepresentation.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack)
				{
					num2 *= creatureRepresentation.dynamicRelationship.currentRelationship.intensity;
				}
				if (num < num2)
				{
					behavior = Behavior.FindPackLeader;
					num = num2;
				}
			}
		}
		if (!scavenger.King && num < 0.08f + Mathf.InverseLerp(0.5f, 1f, scared) * 0.2f + ((creature.abstractAI as ScavengerAbstractAI).GhostOutOfCurrentRoom ? 0.6f : 0f) && creature.abstractAI.MigrationDestination.room != scavenger.room.abstractRoom.index && base.pathFinder.CoordinateViable(creature.abstractAI.MigrationDestination))
		{
			behavior = Behavior.Travel;
			num = 0.08f + Mathf.InverseLerp(0.5f, 1f, scared) * 0.2f + ((creature.abstractAI as ScavengerAbstractAI).GhostOutOfCurrentRoom ? 0.6f : 0f);
		}
		if (scavengeCandidate != null)
		{
			float num3 = Mathf.Min(0.3f, (float)CollectScore(scavengeCandidate.representedItem.realizedObject, weaponFiltered: true) * 0.05f);
			bool flag = scavengeCandidate.representedItem == giftForMe;
			if (flag)
			{
				num3 = Mathf.Max(num3, 0.7f);
			}
			if (num < num3)
			{
				if (flag)
				{
					behavior = Behavior.Scavange;
					num = num3;
				}
				else
				{
					int num4 = int.MaxValue;
					for (int i = 0; i < scavenger.grasps.Length; i++)
					{
						if (num4 <= 0)
						{
							break;
						}
						if (scavenger.grasps[i] == null)
						{
							num4 = 0;
						}
						else if (CollectScore(scavenger.grasps[i].grabbed, weaponFiltered: true) < num4)
						{
							num4 = CollectScore(scavenger.grasps[i].grabbed, weaponFiltered: true);
						}
					}
					if (CollectScore(scavengeCandidate, weaponFiltered: true) > num4 && base.threatTracker.ThreatOfArea(scavengeCandidate.BestGuessForPosition(), accountThreatCreatureAccessibility: true) <= base.threatTracker.ThreatOfArea(scavenger.room.GetWorldCoordinate(scavenger.occupyTile), accountThreatCreatureAccessibility: true))
					{
						behavior = Behavior.Scavange;
						num = num3;
					}
				}
			}
		}
		if (discomfortWithOtherCreatures > 0.5f && (behavior == Behavior.Idle || focusCreature == null))
		{
			focusCreature = base.discomfortTracker.MostUncomfortableCreature();
		}
	}

	public Tracker.CreatureRepresentation PackLeader()
	{
		Tracker.CreatureRepresentation result = null;
		float num = creature.personality.dominance;
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).dynamicRelationship.state.alive && base.tracker.GetRep(i).EstimatedChanceOfFinding > 0.07f)
			{
				if (base.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger && base.tracker.GetRep(i).representedCreature.personality.dominance > num)
				{
					result = base.tracker.GetRep(i);
					num = base.tracker.GetRep(i).representedCreature.personality.dominance;
				}
				else if (base.tracker.GetRep(i).representedCreature.creatureTemplate.type != CreatureTemplate.Type.Scavenger && base.tracker.GetRep(i).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack && base.tracker.GetRep(i).dynamicRelationship.currentRelationship.intensity > num)
				{
					result = base.tracker.GetRep(i);
					num = base.tracker.GetRep(i).dynamicRelationship.currentRelationship.intensity;
				}
			}
		}
		return result;
	}

	public int WeaponScore(PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection)
	{
		if (obj is Spear)
		{
			if (ModManager.MMF && MMF.cfgHunterBackspearProtect.Value && (obj as Spear).onPlayerBack)
			{
				return 0;
			}
			if ((obj as Spear).mode == Weapon.Mode.StuckInWall)
			{
				return 0;
			}
			if (obj is ExplosiveSpear)
			{
				if ((obj as ExplosiveSpear).Ignited)
				{
					return 0;
				}
				if (!(currentViolenceType == ViolenceType.Lethal || pickupDropInsteadOfWeaponSelection))
				{
					return 1;
				}
				return 4;
			}
			if (!pickupDropInsteadOfWeaponSelection && (currentViolenceType == ViolenceType.NonLethal || currentViolenceType == ViolenceType.ForFun))
			{
				return 2;
			}
			return 3;
		}
		if (obj is Rock)
		{
			if (currentViolenceType == ViolenceType.NonLethal)
			{
				for (int i = 0; i < scavenger.grasps.Length; i++)
				{
					if (scavenger.grasps[0] == null)
					{
						return 4;
					}
				}
			}
			return 2;
		}
		if (obj is SporePlant && !(obj as SporePlant).UsableAsWeapon)
		{
			return 0;
		}
		if (obj is ScavengerBomb || obj is SporePlant || (ModManager.MSC && obj is SingularityBomb))
		{
			if (pickupDropInsteadOfWeaponSelection)
			{
				return 3;
			}
			if ((obj is ScavengerBomb || (ModManager.MSC && obj is SingularityBomb)) && currentViolenceType != ViolenceType.Lethal)
			{
				return 0;
			}
			if (focusCreature == null || Custom.DistLess(scavenger.mainBodyChunk.pos, scavenger.room.MiddleOfTile(focusCreature.BestGuessForPosition()), 300f))
			{
				if (!(scared > 0.9f))
				{
					return 0;
				}
				return 1;
			}
			for (int j = 0; j < base.tracker.CreaturesCount; j++)
			{
				if (base.tracker.GetRep(j).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack && (float)Custom.ManhattanDistance(base.tracker.GetRep(j).BestGuessForPosition(), focusCreature.BestGuessForPosition()) < 7f)
				{
					return 0;
				}
			}
			return 3;
		}
		if (obj is FirecrackerPlant)
		{
			if ((obj as FirecrackerPlant).fuseCounter != 0)
			{
				return 0;
			}
			if (focusCreature == null || !focusCreature.representedCreature.creatureTemplate.IsLizard)
			{
				return 1;
			}
			return 3;
		}
		if (obj is PuffBall)
		{
			return 1;
		}
		if (obj is FlareBomb)
		{
			return 1;
		}
		if (obj is JellyFish)
		{
			if ((obj as JellyFish).electricCounter >= 1)
			{
				return 0;
			}
			if (!(currentViolenceType == ViolenceType.Lethal))
			{
				return 3;
			}
			return 2;
		}
		if (ModManager.MSC && obj is LillyPuck)
		{
			if ((obj as IPlayerEdible).BitesLeft != 3)
			{
				return 0;
			}
			if (currentViolenceType == ViolenceType.ForFun)
			{
				return 5;
			}
			if (currentViolenceType == ViolenceType.NonLethal)
			{
				return 2;
			}
			return 1;
		}
		return 0;
	}

	public bool RealWeapon(PhysicalObject obj)
	{
		if (!(obj is Spear) && !(obj is ScavengerBomb))
		{
			if (ModManager.MSC)
			{
				return obj is SingularityBomb;
			}
			return false;
		}
		return true;
	}

	public int CollectScore(ItemTracker.ItemRepresentation obj, bool weaponFiltered)
	{
		if (obj.representedItem.realizedObject != null)
		{
			return CollectScore(obj.representedItem.realizedObject, weaponFiltered);
		}
		return 0;
	}

	public int CollectScore(PhysicalObject obj, bool weaponFiltered)
	{
		if (scavenger.room != null)
		{
			SocialEventRecognizer.OwnedItemOnGround ownedItemOnGround = scavenger.room.socialEventRecognizer.ItemOwnership(obj);
			if (ownedItemOnGround != null && ownedItemOnGround.offeredTo != null && ownedItemOnGround.offeredTo != scavenger)
			{
				return 0;
			}
		}
		if (weaponFiltered && NeedAWeapon)
		{
			return WeaponScore(obj, pickupDropInsteadOfWeaponSelection: true);
		}
		if (obj is DataPearl)
		{
			return 10;
		}
		if (obj is Spear)
		{
			if (ModManager.MMF && MMF.cfgHunterBackspearProtect.Value && (obj as Spear).onPlayerBack)
			{
				return 0;
			}
			if ((obj as Spear).mode == Weapon.Mode.StuckInWall)
			{
				return 0;
			}
			if (obj is ExplosiveSpear)
			{
				if (!(obj as ExplosiveSpear).Ignited)
				{
					return 4;
				}
				return 0;
			}
			if (ModManager.MSC && obj is ElectricSpear)
			{
				if ((obj as ElectricSpear).abstractSpear.electricCharge <= 0)
				{
					return 3;
				}
				return 5;
			}
			return 3;
		}
		if (obj is Rock)
		{
			int num = 0;
			for (int i = 0; i < scavenger.grasps.Length; i++)
			{
				if (scavenger.grasps[i] != null && scavenger.grasps[i].grabbed is Rock && scavenger.grasps[i].grabbed != obj)
				{
					num++;
				}
			}
			if (num >= (creature.abstractAI as ScavengerAbstractAI).carryRocks)
			{
				return 0;
			}
			return 1;
		}
		if (obj is PuffBall)
		{
			return 2;
		}
		if (obj is FlareBomb)
		{
			return 2;
		}
		if (obj is Lantern)
		{
			return 3;
		}
		if (obj is KarmaFlower)
		{
			return 5;
		}
		if (obj is Mushroom)
		{
			return 2;
		}
		if (obj is VultureMask)
		{
			if (!(obj as VultureMask).King)
			{
				return 5;
			}
			return 10;
		}
		if (obj is OverseerCarcass)
		{
			if (ModManager.MSC && (obj.abstractPhysicalObject as OverseerCarcass.AbstractOverseerCarcass).InspectorMode)
			{
				return 20;
			}
			return 7;
		}
		if (obj is ScavengerBomb)
		{
			return 3;
		}
		if (obj is FirecrackerPlant)
		{
			if ((obj as FirecrackerPlant).fuseCounter != 0)
			{
				return 0;
			}
			return 2;
		}
		if (obj is JellyFish)
		{
			if ((obj as JellyFish).electricCounter >= 1)
			{
				return 0;
			}
			return 2;
		}
		if (obj is FlyLure)
		{
			return 2;
		}
		if (obj is SporePlant)
		{
			if (!(obj as SporePlant).UsableAsWeapon)
			{
				return 0;
			}
			return 4;
		}
		if (ModManager.MSC)
		{
			if (obj is GlowWeed)
			{
				return (obj as IPlayerEdible).BitesLeft;
			}
			if (obj is LillyPuck)
			{
				if ((obj as IPlayerEdible).BitesLeft == 3)
				{
					return 4;
				}
				return 0;
			}
			if (obj is GooieDuck)
			{
				if (scavenger.room.game.IsStorySession && scavenger.room.world.region.name == "LF")
				{
					return 2;
				}
				return 1;
			}
			if (obj is SingularityBomb)
			{
				return 10;
			}
		}
		return 0;
	}

	public bool DoIWantToTrade(int valueOfMerch)
	{
		if (valueOfMerch < 2)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < scavenger.grasps.Length; i++)
		{
			if (scavenger.grasps[i] != null)
			{
				int num3 = CollectScore(scavenger.grasps[i].grabbed, weaponFiltered: false);
				num += num3;
				num2 = Math.Max(num2, num3);
			}
		}
		if (valueOfMerch > num2)
		{
			return num2 > 1;
		}
		return false;
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed) - Mathf.Lerp(0.8f, 0.2f, agitation) * Mathf.InverseLerp(1f, Mathf.Lerp(-1f, 0.8f, scavenger.narrowVision), Vector2.Dot(scavenger.HeadLookDir, (lookAtPoint - scavenger.bodyChunks[2].pos).normalized)) - (1f - scavenger.visionFactor);
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false);
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Attacks)
		{
			return base.preyTracker;
		}
		return null;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return new ScavengerTrackState();
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger)
		{
			if (dRelation.trackerRep.representedCreature.realizedCreature == null || dRelation.trackerRep.representedCreature.realizedCreature.room != this.scavenger.room)
			{
				return StaticRelationship(dRelation.trackerRep.representedCreature).Duplicate();
			}
			Scavenger scavenger = dRelation.trackerRep.representedCreature.realizedCreature as Scavenger;
			if (dRelation.trackerRep.VisualContact && this.scavenger.animation != null && this.scavenger.animation.id == Scavenger.ScavengerAnimation.ID.Look && scavenger.animation != null && scavenger.animation.id == Scavenger.ScavengerAnimation.ID.GeneralPoint && !this.scavenger.safariControlled && UnityEngine.Random.value < 0.1f)
			{
				(this.scavenger.animation as Scavenger.AttentiveAnimation).point = (scavenger.animation as Scavenger.GeneralPointAnimation).point;
			}
			return StaticRelationship(dRelation.trackerRep.representedCreature).Duplicate();
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature).Duplicate();
		if (ModManager.MSC && dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer && (dRelation.trackerRep.representedCreature.abstractAI as OverseerAbstractAI).safariOwner)
		{
			result.type = CreatureTemplate.Relationship.Type.Ignores;
			return result;
		}
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
			if (dRelation.trackerRep.representedCreature.realizedCreature != null)
			{
				Creature realizedCreature = dRelation.trackerRep.representedCreature.realizedCreature;
				bool flag = false;
				if (ModManager.MMF)
				{
					flag = dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Centipede || dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.RedCentipede || dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Centiwing || dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.PoleMimic || dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.BigSpider || dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.SpitterSpider || dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.DropBug;
				}
				bool flag2 = false;
				if (ModManager.MSC)
				{
					flag2 = dRelation.trackerRep.representedCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti || dRelation.trackerRep.representedCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider || dRelation.trackerRep.representedCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug;
				}
				if (dRelation.trackerRep.representedCreature.creatureTemplate.IsLizard || dRelation.trackerRep.representedCreature.creatureTemplate.IsVulture || dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.MirosBird || dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.TentaclePlant || dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.JetFish || flag || flag2)
				{
					(dRelation.state as ScavengerTrackState).jawsOccupied = realizedCreature.grasps[0] != null;
					(dRelation.state as ScavengerTrackState).holdingAFriend = realizedCreature.grasps[0] != null && (realizedCreature.grasps[0].grabbed is Scavenger || (realizedCreature.grasps[0].grabbed is Player && base.tracker.RepresentationForObject(realizedCreature.grasps[0].grabbed, AddIfMissing: false) != null && base.tracker.RepresentationForObject(realizedCreature.grasps[0].grabbed, AddIfMissing: false).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack));
				}
				else if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
				{
					(dRelation.state as ScavengerTrackState).armed = false;
					(dRelation.state as ScavengerTrackState).mostValuableItem = 0;
					for (int i = 0; i < realizedCreature.grasps.Length; i++)
					{
						if (realizedCreature.grasps[i] != null)
						{
							if (realizedCreature.grasps[i].grabbed is Spear || (ModManager.MMF && realizedCreature.grasps[i].grabbed is ScavengerBomb) || (ModManager.MSC && realizedCreature.grasps[i].grabbed is SingularityBomb) || (ModManager.MSC && realizedCreature.grasps[i].grabbed is FireEgg))
							{
								(dRelation.state as ScavengerTrackState).armed = true;
							}
							(dRelation.state as ScavengerTrackState).mostValuableItem = Math.Max((dRelation.state as ScavengerTrackState).mostValuableItem, CollectScore(realizedCreature.grasps[i].grabbed, weaponFiltered: false));
						}
					}
				}
				if (result.type != CreatureTemplate.Relationship.Type.Ignores)
				{
					(dRelation.state as ScavengerTrackState).moving = Mathf.Lerp((dRelation.state as ScavengerTrackState).moving, Vector2.Distance(dRelation.trackerRep.representedCreature.realizedCreature.mainBodyChunk.lastPos, dRelation.trackerRep.representedCreature.realizedCreature.mainBodyChunk.pos), 0.1f);
					float num = Vector2.Distance(this.scavenger.mainBodyChunk.pos, dRelation.trackerRep.representedCreature.realizedCreature.mainBodyChunk.pos);
					(dRelation.state as ScavengerTrackState).gettingCloser = Mathf.Lerp((dRelation.state as ScavengerTrackState).gettingCloser, (dRelation.state as ScavengerTrackState).lastDistance - num, 0.1f);
					(dRelation.state as ScavengerTrackState).lastDistance = num;
					if ((dRelation.state as ScavengerTrackState).smoothedMoving < (dRelation.state as ScavengerTrackState).moving)
					{
						(dRelation.state as ScavengerTrackState).smoothedMoving = Mathf.Min((dRelation.state as ScavengerTrackState).smoothedMoving + 0.5f, (dRelation.state as ScavengerTrackState).moving);
					}
					else
					{
						(dRelation.state as ScavengerTrackState).smoothedMoving = Mathf.Max((dRelation.state as ScavengerTrackState).smoothedMoving - 0.25f, (dRelation.state as ScavengerTrackState).moving);
					}
					(dRelation.state as ScavengerTrackState).smoothedMoving = Mathf.Lerp((dRelation.state as ScavengerTrackState).smoothedMoving, (dRelation.state as ScavengerTrackState).moving, 0.01f);
					if ((dRelation.state as ScavengerTrackState).smoothedGettingCloser < (dRelation.state as ScavengerTrackState).moving)
					{
						(dRelation.state as ScavengerTrackState).smoothedGettingCloser = Mathf.Min((dRelation.state as ScavengerTrackState).smoothedGettingCloser + 0.25f, (dRelation.state as ScavengerTrackState).gettingCloser);
					}
					else
					{
						(dRelation.state as ScavengerTrackState).smoothedGettingCloser = Mathf.Max((dRelation.state as ScavengerTrackState).smoothedGettingCloser - 0.25f, (dRelation.state as ScavengerTrackState).gettingCloser);
					}
					(dRelation.state as ScavengerTrackState).smoothedGettingCloser = Mathf.Lerp((dRelation.state as ScavengerTrackState).smoothedGettingCloser, (dRelation.state as ScavengerTrackState).gettingCloser, 0.01f);
				}
			}
		}
		bool flag3 = false;
		if (result.type == CreatureTemplate.Relationship.Type.SocialDependent)
		{
			flag3 = true;
			if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat || (ModManager.MSC && dRelation.trackerRep.representedCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC))
			{
				result = PlayerRelationship(dRelation);
			}
			else
			{
				result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f);
				(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.None;
			}
		}
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.BrotherLongLegs && dRelation.trackerRep.representedCreature.realizedCreature != null && (dRelation.trackerRep.representedCreature.realizedCreature as DaddyLongLegs).digestingCounter > 10)
		{
			result.intensity = 0.3f;
		}
		if (result.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			if (!dRelation.state.alive)
			{
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
			}
			if ((dRelation.state as ScavengerTrackState).jawsOccupied)
			{
				result.intensity *= 0.5f;
			}
			if ((dRelation.state as ScavengerTrackState).holdingAFriend && creature.personality.bravery > 0.1f && creature.personality.aggression > 0.1f && creature.personality.sympathy > 0.1f)
			{
				result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.5f + 0.5f * creature.personality.bravery);
			}
			if (creature.personality.bravery > 0.8f && creature.personality.aggression > 0.8f)
			{
				result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.5f + 0.5f * creature.personality.bravery);
			}
			if ((float)(dRelation.state as ScavengerTrackState).hitsOnThisCreature > Mathf.Lerp(5f, 1f, Mathf.Pow(creature.personality.aggression * creature.personality.bravery, 0.5f)) && (dRelation.state as ScavengerTrackState).hitsOnThisCreature < 15)
			{
				result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.5f + 0.5f * creature.personality.bravery);
			}
			if (!flag3)
			{
				if (result.intensity > 1f - Mathf.Pow(creature.personality.aggression, Mathf.Lerp(0.5f, 0.01f, scared)))
				{
					(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.Lethal;
				}
				else
				{
					(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.NonLethal;
				}
			}
			result.intensity *= Custom.LerpMap((dRelation.state as ScavengerTrackState).smoothedGettingCloser, 3f, -8f, 1f, 0.5f);
		}
		else if (result.type == CreatureTemplate.Relationship.Type.Attacks && !flag3)
		{
			(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.Lethal;
		}
		if (!dRelation.state.alive)
		{
			result.intensity *= 0.1f;
		}
		if ((result.type == CreatureTemplate.Relationship.Type.Attacks || result.type == CreatureTemplate.Relationship.Type.Afraid) && !dRelation.state.alive)
		{
			result.type = CreatureTemplate.Relationship.Type.Uncomfortable;
		}
		return result;
	}

	private CreatureTemplate.Relationship PlayerRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		CreatureTemplate.Relationship relationship = StaticRelationship(dRelation.trackerRep.representedCreature).Duplicate();
		if (scavenger.King)
		{
			if (CurrentPlayerAggression(dRelation.trackerRep.representedCreature) > 0.15f)
			{
				relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.5f + scared / 2f);
				(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.Lethal;
			}
			else if ((scavenger.State as HealthState).ClampedHealth < 0.9f && scared > 0.1f)
			{
				relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f);
				(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.Warning;
			}
			else
			{
				relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.5f + scared / 2f);
				(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.Warning;
			}
			return relationship;
		}
		if (ModManager.MSC && dRelation.trackerRep.representedCreature.realizedCreature != null && dRelation.trackerRep.representedCreature.realizedCreature is Player && (dRelation.trackerRep.representedCreature.realizedCreature as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			if (scavenger.PlayerHasImmunity(dRelation.trackerRep.representedCreature.realizedCreature as Player))
			{
				relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f);
			}
			else
			{
				relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f);
				(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.Lethal;
			}
			return relationship;
		}
		float num = LikeOfPlayer(dRelation);
		if (dRelation.trackerRep.representedCreature.ID.number == 0)
		{
			filteredLikeB = num;
		}
		if (num < 0.7f && creature.abstractAI.MigrationDestination.room == creature.pos.room)
		{
			(creature.abstractAI as ScavengerAbstractAI).dontMigrate = Math.Min((creature.abstractAI as ScavengerAbstractAI).dontMigrate, 10);
		}
		if ((dRelation.state as ScavengerTrackState).consideredWarnedCounter > 0)
		{
			(dRelation.state as ScavengerTrackState).consideredWarnedCounter--;
		}
		float num2 = Mathf.Lerp(creature.personality.bravery, Mathf.Max(creature.personality.bravery, backedByPack), 0.5f);
		if ((dRelation.state as ScavengerTrackState).armed)
		{
			num2 = Mathf.Lerp(num2, 0f, 0.5f * (1f - creature.personality.bravery));
		}
		num2 *= 1f - 0.5f * scared * (1f - creature.personality.bravery);
		num2 = Mathf.Lerp(num2, 1f, outpostModule.FearDebuff);
		float num3 = Mathf.InverseLerp(Mathf.Lerp(5f, 15f, creature.personality.bravery), 3f, creature.pos.Tile.FloatDist(dRelation.trackerRep.BestGuessForPosition().Tile));
		if (dRelation.currentRelationship.type == CreatureTemplate.Relationship.Type.Afraid || base.pathFinder.GetDestination.room != creature.pos.room)
		{
			num3 = Mathf.Pow(num3, Custom.LerpMap(dRelation.trackerRep.age, 40f, 300f, 2f, 0.5f));
		}
		num3 = Mathf.Lerp(num3, 1f, Mathf.InverseLerp(0.15f, 0f, num));
		num3 = Mathf.Lerp(num3, 1f, Mathf.InverseLerp(4f, Mathf.Lerp(50f, 5f, creature.personality.aggression), (dRelation.state as ScavengerTrackState).bumpedByThisCreature) * 0.9f);
		float num4 = Mathf.InverseLerp(0f, 3f, (dRelation.state as ScavengerTrackState).smoothedGettingCloser) * Mathf.InverseLerp(1f, 3f, (dRelation.state as ScavengerTrackState).smoothedMoving);
		num3 = Mathf.Pow(num3, Mathf.Lerp(2f, 0.5f, num4));
		num3 = Mathf.Clamp(num3 - outpostModule.LikeModifier(dRelation.trackerRep.representedCreature), 0f, 1f);
		RelationshipTracker.DynamicRelationship dynamicRelationship = null;
		if (dRelation.trackerRep.representedCreature.realizedCreature != null)
		{
			if (dRelation.trackerRep.representedCreature.realizedCreature is Player)
			{
				if ((float)(dRelation.trackerRep.representedCreature.realizedCreature as Player).touchedNoInputCounter > 10f)
				{
					num4 *= 0f;
					num3 = Mathf.Lerp(num3, 0f, Custom.LerpMap(num, 0.1f, 0.3f, 0f, 0.2f));
					num3 = Mathf.Pow(num3, 1f + Mathf.InverseLerp(0.1f, 0.3f, num) * 1.5f);
				}
				if (!(dRelation.trackerRep.representedCreature.realizedCreature as Player).standing)
				{
					num3 = Mathf.Pow(num3, 1f + Mathf.InverseLerp(0.3f, 0.1f, num));
				}
			}
			Tracker.CreatureRepresentation creatureRepresentation = PackLeader();
			if (creatureRepresentation != null && creatureRepresentation.representedCreature.realizedCreature != null && creatureRepresentation.representedCreature.realizedCreature is Scavenger)
			{
				Tracker.CreatureRepresentation creatureRepresentation2 = (creatureRepresentation.representedCreature.realizedCreature as Scavenger).AI.tracker.RepresentationForObject(dRelation.trackerRep.representedCreature.realizedCreature, AddIfMissing: false);
				if (creatureRepresentation2 != null)
				{
					dynamicRelationship = creatureRepresentation2.dynamicRelationship;
				}
			}
		}
		float num5 = 0.3f;
		if (scavenger.Elite)
		{
			num5 = 0.7f;
		}
		if (num > num5)
		{
			if (wantToTradeWith == dRelation.trackerRep)
			{
				num = Mathf.Max(num, 0.85f);
			}
			else if (wantToTradeWith == null && DoIWantToTrade((dRelation.state as ScavengerTrackState).mostValuableItem))
			{
				wantToTradeWith = dRelation.trackerRep;
			}
		}
		if (num < num5)
		{
			float num6 = creature.personality.aggression * num2 - outpostModule.LikeModifier(dRelation.trackerRep.representedCreature);
			if ((creature.abstractAI as ScavengerAbstractAI).squad != null && (creature.abstractAI as ScavengerAbstractAI).squad.targetCreature == dRelation.trackerRep.representedCreature)
			{
				if ((creature.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.HuntCreature)
				{
					num6 += 0.5f;
				}
				else if ((creature.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.ProtectCreature)
				{
					num6 -= 0.5f;
				}
			}
			float num7 = 0f;
			if (creature.world.game.IsArenaSession && dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
			{
				num7 = Mathf.InverseLerp(0f, -1f, creature.world.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, (dRelation.trackerRep.representedCreature.state as PlayerState).playerNumber));
			}
			num6 += 0.5f * num7;
			relationship = ((!(num6 > Custom.LerpMap(num, 0f, num5, 0.08f, 0.18f)) || (!((dRelation.state as ScavengerTrackState).taggedViolenceType == ViolenceType.Lethal) && !((float)(dRelation.state as ScavengerTrackState).throwsTowardsThisCreature < 19f * creature.personality.aggression * creature.personality.bravery))) ? new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f + Mathf.InverseLerp(num5, 0f, num) * 0.5f) : new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.5f + Mathf.InverseLerp(num5, 0f, num) * 0.5f));
			float a = Mathf.Lerp(Mathf.InverseLerp(num5, 0f, num), 1f - (1f - creature.personality.aggression) * creature.personality.sympathy, 0.5f);
			a = Mathf.Lerp(a, Mathf.Max(a, Mathf.InverseLerp(num5, 0f, num)), (dRelation.state as ScavengerTrackState).armed ? 0.75f : 0.25f);
			if (dynamicRelationship != null)
			{
				a = Mathf.Lerp(a, ((dynamicRelationship.state as ScavengerTrackState).taggedViolenceType == ViolenceType.Lethal) ? 1f : 0f, Mathf.Pow(1f - creature.personality.dominance, 0.1f));
			}
			a = Mathf.Max(a, Mathf.InverseLerp(num5 / 2f, 0f, num));
			a = Mathf.Clamp(a - outpostModule.LikeModifier(dRelation.trackerRep.representedCreature), 0f, 1f);
			a = Mathf.Lerp(a, 1f, num7);
			if (a > 1f - num3 || scavenger.Elite)
			{
				(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.Lethal;
			}
			else
			{
				(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.Warning;
			}
		}
		else if (num < 0.7f)
		{
			float num8 = Mathf.InverseLerp(0.3f, 0.7f, num);
			float num9 = Mathf.Pow(num8, Mathf.Lerp(2f, 0.5f, backedByPack));
			num9 *= Custom.LerpMap(creature.pos.Tile.FloatDist(dRelation.trackerRep.BestGuessForPosition().Tile), 5f, Mathf.Lerp(40f, 10f, creature.personality.dominance), 1f, num8);
			if (dRelation.currentRelationship.type != CreatureTemplate.Relationship.Type.Afraid && dRelation.currentRelationship.type != CreatureTemplate.Relationship.Type.Attacks)
			{
				num9 = Mathf.Lerp(num9, 1f, 0.1f);
			}
			if (dynamicRelationship != null)
			{
				num9 = Mathf.Lerp(num9, (dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Uncomfortable) ? 1f : 0f, 0.5f * (1f - creature.personality.dominance));
			}
			num9 = Mathf.Lerp(num9, 0f, num3 * Mathf.Pow(1f - num8, 0.2f));
			float a2 = Mathf.Pow((1f - num8) * (0.5f + creature.personality.dominance), Mathf.Lerp(1f, 0.8f, num4));
			a2 = Mathf.Lerp(a2, 1f, creature.personality.dominance * (0.5f + 0.5f * backedByPack) * 0.5f);
			a2 *= Mathf.InverseLerp(1800f, 450f, (dRelation.state as ScavengerTrackState).consideredWarnedCounter);
			a2 *= 0.5f + 0.5f * Mathf.InverseLerp(creature.personality.aggression * 6f, 1f, (dRelation.state as ScavengerTrackState).throwsTowardsThisCreature);
			a2 *= Mathf.Lerp(Mathf.InverseLerp(0f, 0.5f, num2), 1f, 0.5f * backedByPack);
			if (dRelation.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks)
			{
				a2 = Mathf.Lerp(a2, 1f, 0.1f);
			}
			if (dynamicRelationship != null)
			{
				a2 = Mathf.Lerp(a2, (dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks) ? 1f : 0f, 0.5f + 0.5f * Mathf.Pow((1f - creature.personality.dominance) * (0.5f + 0.5f * backedByPack), 0.2f));
			}
			if (a2 > (1f - num3) * Mathf.Lerp(0.9f, 0.6f, creature.personality.aggression) && a2 > num9)
			{
				relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, (1f - num8) * 0.3f);
				(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.Warning;
				if (dRelation.trackerRep.VisualContact && creature.pos.Tile.FloatDist(dRelation.trackerRep.BestGuessForPosition().Tile) < 15f)
				{
					(dRelation.state as ScavengerTrackState).consideredWarnedCounter += 3;
				}
			}
			else if (num9 > Mathf.Lerp(0.65f, 0.35f, creature.personality.dominance * creature.personality.bravery))
			{
				if (Mathf.Pow((1f - creature.personality.sympathy) * creature.personality.aggression * num2 * (1f - num8), 0.2f) > Mathf.Pow(1f - num3, 2.8f) * 0.95f && (dRelation.state as ScavengerTrackState).throwsTowardsThisCreature < 1)
				{
					relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, (1f - num8) * 0.3f);
					(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.NonLethal;
				}
				else
				{
					relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f + (1f - num8) * 0.5f);
					(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.None;
				}
			}
			else
			{
				relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f + (1f - num9) * 0.5f);
				(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.NonLethal;
			}
		}
		else
		{
			relationship = ((!(num < 0.8f)) ? new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, Mathf.InverseLerp(0.8f, 1f, num)) : new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, Mathf.InverseLerp(0.8f, 0.7f, num) * 0.3f));
			(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.None;
		}
		if (num < 0.7f && num4 * Mathf.Pow(Mathf.InverseLerp(450f, 700f, (dRelation.state as ScavengerTrackState).consideredWarnedCounter), 1.5f - creature.personality.aggression) > 1f - Mathf.InverseLerp(0f, 0.7f, num))
		{
			(dRelation.state as ScavengerTrackState).taggedViolenceType = ViolenceType.Lethal;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid || relationship.type == CreatureTemplate.Relationship.Type.Attacks)
		{
			relationship.intensity *= 1f - communicationModule.Utility();
		}
		return relationship;
	}

	public float LikeOfPlayer(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (dRelation.trackerRep.representedCreature.realizedCreature != null && dRelation.trackerRep.representedCreature.realizedCreature is Player && scavenger.PlayerHasImmunity(dRelation.trackerRep.representedCreature.realizedCreature as Player))
		{
			return 1f;
		}
		float num = creature.world.game.session.creatureCommunities.LikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (dRelation.trackerRep.representedCreature.state as PlayerState).playerNumber);
		float num2 = Mathf.Pow(Mathf.InverseLerp(-1f, 1f, num), 1.5f);
		num2 += Mathf.Lerp(-0.1f, 0.1f, creature.personality.sympathy);
		float num3 = Mathf.InverseLerp(-1f, 1f, creature.state.socialMemory.GetTempLike(dRelation.trackerRep.representedCreature.ID));
		float num4 = Mathf.Min(Mathf.Abs(num3 - Mathf.InverseLerp(-1f, 1f, creature.state.socialMemory.GetLike(dRelation.trackerRep.representedCreature.ID))) * 5f, 1f);
		num4 = 1f - (1f - num4) * (1f - Mathf.Pow(creature.state.socialMemory.GetKnow(dRelation.trackerRep.representedCreature.ID), 0.5f));
		num4 = Mathf.Pow(num4, 0.3f);
		num2 = Mathf.Lerp(num2, num3, num4);
		num2 = Mathf.Clamp(num2 + outpostModule.LikeModifier(dRelation.trackerRep.representedCreature), 0f, 1f);
		if ((creature.abstractAI as ScavengerAbstractAI).squad != null)
		{
			if ((creature.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
			{
				num2 = Mathf.Lerp(num2, Mathf.Max(num2, 0.85f), Mathf.InverseLerp(0.25f, 0.5f, num2));
			}
			else if ((creature.abstractAI as ScavengerAbstractAI).squad.targetCreature != null && (creature.abstractAI as ScavengerAbstractAI).squad.targetCreature == dRelation.trackerRep.representedCreature)
			{
				if ((creature.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.HuntCreature)
				{
					num2 -= 0.5f;
				}
				else if ((creature.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.ProtectCreature)
				{
					num2 += 0.5f;
				}
			}
		}
		if (creature.world.game.IsArenaSession)
		{
			num2 -= Custom.LerpMap(num, 0f, -1f, 0f, 0.5f);
		}
		if (dRelation.trackerRep.representedCreature.realizedCreature != null && dRelation.trackerRep.TicksSinceSeen < 40)
		{
			for (int i = 0; i < dRelation.trackerRep.representedCreature.realizedCreature.grasps.Length; i++)
			{
				if (dRelation.trackerRep.representedCreature.realizedCreature.grasps[i] != null)
				{
					if (num2 < -0.2f && dRelation.trackerRep.representedCreature.realizedCreature.grasps[i].grabbed is Spear)
					{
						num2 -= 0.1f;
					}
					else if (dRelation.trackerRep.representedCreature.realizedCreature.grasps[i].grabbed is DataPearl)
					{
						num2 += ((num2 < 0.5f) ? (-1f) : 1f) * 0.1f;
					}
				}
			}
		}
		return Mathf.Clamp(num2, 0f, 1f);
	}

	public override bool TrackerToDiscardDeadCreature(AbstractCreature crit)
	{
		return false;
	}

	public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
	{
		if (coord.destinationCoord.TileDefined)
		{
			if (behavior == Behavior.Idle)
			{
				for (int i = 0; i < base.tracker.CreaturesCount; i++)
				{
					if (base.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger && base.tracker.GetRep(i).representedCreature.personality.dominance > creature.personality.dominance && base.tracker.GetRep(i).BestGuessForPosition().room == coord.destinationCoord.room)
					{
						cost.resistance += Custom.LerpMap(coord.DestTile.FloatDist(base.tracker.GetRep(i).BestGuessForPosition().Tile), 0f, 4f, 50f, 0f);
					}
				}
				cost.resistance += base.discomfortTracker.DiscomfortOfTile(coord.destinationCoord) * 10f;
			}
			if (base.threatTracker.Utility() > 0.1f)
			{
				cost.resistance += base.threatTracker.ThreatOfTile(coord.destinationCoord, accountThreatCreatureAccessibility: true) * 100f;
			}
		}
		return cost;
	}

	public bool TrackItem(AbstractPhysicalObject obj)
	{
		if (obj.realizedObject != null && obj.realizedObject is Weapon && (obj.realizedObject as Weapon).mode == Weapon.Mode.StuckInWall)
		{
			return false;
		}
		return true;
	}

	public void SeeThrownWeapon(PhysicalObject obj, Creature thrower)
	{
		if (thrower == scavenger)
		{
			return;
		}
		if (base.tracker.RepresentationForObject(thrower, AddIfMissing: false) == null)
		{
			base.noiseTracker.mysteriousNoises += 20f;
			base.noiseTracker.mysteriousNoiseCounter = 200;
			if (scavenger.animation == null)
			{
				scavenger.animation = new Scavenger.LookAnimation(scavenger, null, obj.firstChunk.pos, 1f, stop: true);
			}
			else if (scavenger.animation.id == Scavenger.ScavengerAnimation.ID.Look && !scavenger.safariControlled)
			{
				(scavenger.animation as Scavenger.LookAnimation).point = obj.firstChunk.pos;
			}
			if (scavenger.graphicsModule != null)
			{
				(scavenger.graphicsModule as ScavengerGraphics).ShockReaction(((obj is Spear) ? 1f : 0.7f) * Mathf.Lerp(1f, 0.5f, agitation) * Mathf.Pow(1f - creature.personality.bravery, 0.4f));
			}
			agitation = Mathf.Lerp(agitation, 1f, 0.25f * creature.personality.energy);
			if (obj is Spear || (ModManager.MMF && obj is ScavengerBomb) || (ModManager.MSC && obj is SingularityBomb) || (ModManager.MSC && obj is FireEgg))
			{
				scared = Mathf.Lerp(scared, 1f, 1f - creature.personality.bravery);
			}
		}
		else if (behavior != Behavior.Attack && behavior != Behavior.Flee && (scavenger.animation == null || scavenger.animation.id == Scavenger.ScavengerAnimation.ID.Rummage))
		{
			scavenger.animation = new Scavenger.LookAnimation(scavenger, null, obj.firstChunk.pos, 1f, stop: true);
		}
		if (ModManager.MMF && scavenger.animation != null && scavenger.animation is Scavenger.LookAnimation)
		{
			(scavenger.animation as Scavenger.LookAnimation).point = obj.firstChunk.pos;
		}
	}

	private void UpdateLookPoint()
	{
		float actNervous = ActNervous;
		alreadyLookedAtPos += Custom.DirVec(alreadyLookedAtPos, scavenger.lookPoint);
		alreadyLookedAtPos = Vector2.Lerp(alreadyLookedAtPos, scavenger.lookPoint, Mathf.Lerp(0.01f, 0.12f, Mathf.Pow(actNervous, 1.2f)));
		Vector2 vector = testLookPos + Custom.RNV() * Mathf.Lerp(10f * creature.personality.energy, 20f + 30f * actNervous, UnityEngine.Random.value);
		if (UnityEngine.Random.value < 1f / Mathf.Lerp(22f, 6f, Mathf.Pow(actNervous, 1.5f)))
		{
			vector = ((!(UnityEngine.Random.value < 0.5f)) ? (scavenger.mainBodyChunk.pos + Custom.RNV() * Mathf.Lerp(200f, 700f, UnityEngine.Random.value)) : (scavenger.mainBodyChunk.pos + Custom.DegToVec(45f + 90f * UnityEngine.Random.value) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * Mathf.Lerp(200f, 700f, UnityEngine.Random.value)));
		}
		if (!scavenger.room.GetTile(vector).Solid && LookPointScore(vector) > LookPointScore(testLookPos))
		{
			testLookPos = vector;
		}
		scavenger.lookPoint = Vector2.Lerp(scavenger.lookPoint, testLookPos, Custom.LerpMap(LookPointScore(testLookPos) - LookPointScore(scavenger.lookPoint), Mathf.Lerp(200f, 0f, actNervous), Mathf.Lerp(600f, 200f, Mathf.Pow(actNervous, 1.5f)), 0f, Mathf.Lerp(0.1f, 0.6f, creature.personality.energy)));
	}

	private float LookPointScore(Vector2 tst)
	{
		if (scavenger.room.GetTile(tst).Solid)
		{
			return float.MinValue;
		}
		float num = Mathf.Min(scavenger.room.aimap.getAItile(tst).visibility, 200);
		num -= Mathf.Abs(500f - Vector2.Distance(scavenger.mainBodyChunk.pos, tst)) * 0.25f;
		num += Mathf.Abs(Custom.DirVec(scavenger.mainBodyChunk.pos, tst).x) * 100f;
		num += Mathf.Min(500f, Vector2.Distance(tst, alreadyLookedAtPos));
		if (tst.x < scavenger.mainBodyChunk.pos.x == scavenger.flip < 0f)
		{
			num += 70f;
		}
		if (tst.x < scavenger.mainBodyChunk.pos.x != testLookPos.x < scavenger.mainBodyChunk.pos.x)
		{
			num -= 70f;
		}
		num += Mathf.InverseLerp(1000f, 0f, Vector2.Distance(tst, scavenger.room.MiddleOfTile(base.pathFinder.GetDestination))) * Mathf.InverseLerp(0f, 500f, Vector2.Distance(scavenger.mainBodyChunk.pos, scavenger.room.MiddleOfTile(base.pathFinder.GetDestination))) * 500f;
		return num + UnityEngine.Random.value * Mathf.Pow(creature.personality.nervous, 2.5f) * 100f;
	}

	private void IdleBehavior()
	{
		discomfortWithOtherCreatures = base.discomfortTracker.DiscomfortOfTile(scavenger.room.GetWorldCoordinate(scavenger.occupyTile));
		WorldCoordinate worldCoordinate = scavenger.room.GetWorldCoordinate(scavenger.mainBodyChunk.pos + Custom.RNV() * UnityEngine.Random.value * 800f);
		if (UnityEngine.Random.value < 0.01f)
		{
			worldCoordinate.x = UnityEngine.Random.Range(0, scavenger.room.TileWidth);
			worldCoordinate.y = UnityEngine.Random.Range(0, scavenger.room.TileHeight);
		}
		if (IdleScore(worldCoordinate) > IdleScore(testIdlePos))
		{
			testIdlePos = worldCoordinate;
		}
		float num = (float)(200 + idleCounter) * (1f - discomfortWithOtherCreatures);
		if (scavenger.Elite && !scavenger.moving)
		{
			num = 0f;
		}
		if (testIdlePos != base.pathFinder.GetDestination && IdleScore(testIdlePos) > IdleScore(base.pathFinder.GetDestination) + num)
		{
			creature.abstractAI.SetDestination(testIdlePos);
		}
		idleCounter--;
		if (!scavenger.moving && !scavenger.Rummaging)
		{
			idleCounter -= 3;
		}
		if (scavenger.ReallyStuck > 1f)
		{
			idleCounter = Mathf.Min(idleCounter, 10);
		}
		if (idleCounter < 1)
		{
			if (outpostModule.outpost == null || !Custom.DistLess(scavenger.room.MiddleOfTile(testIdlePos), outpostModule.outpost.placedObj.pos, outpostModule.outpost.Rad + 500f))
			{
				alreadyIdledAt.Insert(0, testIdlePos);
			}
			if (alreadyIdledAt.Count > 10)
			{
				alreadyIdledAt.RemoveAt(ModManager.MMF ? (alreadyIdledAt.Count - 1) : 5);
			}
			idleCounter = UnityEngine.Random.Range(100, 200 + (int)(1900f * (1f - creature.personality.nervous))) * 4;
			creature.abstractAI.SetDestination(testIdlePos);
		}
		worldCoordinate = base.pathFinder.GetDestination + Custom.fourDirections[UnityEngine.Random.Range(0, 4)];
		if (IdleScore(worldCoordinate) > IdleScore(base.pathFinder.GetDestination))
		{
			creature.abstractAI.SetDestination(worldCoordinate);
		}
		if (!scavenger.moving && idleCounter > (scavenger.Elite ? 300 : 100) && scavenger.animation == null && Scavenger.RummageAnimation.RummagePossible(scavenger))
		{
			scavenger.animation = new Scavenger.RummageAnimation(scavenger);
		}
	}

	private float IdleScore(WorldCoordinate tstPs)
	{
		if (!base.pathFinder.CoordinateViable(tstPs))
		{
			return float.MinValue;
		}
		float num = 0f;
		float num2 = 0f;
		IntVector2 tile = creature.pos.Tile;
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger)
			{
				if (base.tracker.GetRep(i).representedCreature.personality.dominance > creature.personality.dominance && base.tracker.GetRep(i).BestGuessForPosition().room == tstPs.room && base.tracker.GetRep(i).representedCreature.realizedCreature != null && (base.tracker.GetRep(i).representedCreature.realizedCreature as Scavenger).AI.testIdlePos.room == tstPs.room)
				{
					num -= Custom.LerpMap(tstPs.Tile.FloatDist((base.tracker.GetRep(i).representedCreature.realizedCreature as Scavenger).AI.testIdlePos.Tile), 0f, 4f, 250f, 0f);
					if (base.tracker.GetRep(i).representedCreature.personality.dominance > num2)
					{
						num2 = base.tracker.GetRep(i).representedCreature.personality.dominance;
						tile = (base.tracker.GetRep(i).representedCreature.realizedCreature as Scavenger).AI.pathFinder.GetDestination.Tile;
					}
				}
			}
			else if (base.tracker.GetRep(i).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack && base.tracker.GetRep(i).BestGuessForPosition().room == tstPs.room)
			{
				num += Custom.LerpMap(tstPs.Tile.FloatDist(base.tracker.GetRep(i).BestGuessForPosition().Tile), 10f, 20f, 100f * base.tracker.GetRep(i).dynamicRelationship.currentRelationship.intensity, 0f);
				if (base.tracker.GetRep(i).dynamicRelationship.currentRelationship.intensity > creature.personality.dominance && base.tracker.GetRep(i).dynamicRelationship.currentRelationship.intensity > num2)
				{
					num2 = base.tracker.GetRep(i).dynamicRelationship.currentRelationship.intensity;
					tile = base.tracker.GetRep(i).BestGuessForPosition().Tile;
				}
			}
		}
		if (outpostModule.outpost != null)
		{
			tile = scavenger.room.GetTilePosition(outpostModule.outpost.placedObj.pos);
			num -= Custom.LerpMap(Vector2.Distance(scavenger.room.MiddleOfTile(tstPs), outpostModule.outpost.placedObj.pos), outpostModule.outpost.Rad * (1f - outpostModule.Utility()), outpostModule.outpost.Rad + 300f, 0f, 1000f);
		}
		else if (tradeSpot != null)
		{
			tile = scavenger.room.GetTilePosition(tradeSpot.placedObj.pos);
			num -= Custom.LerpMap(Vector2.Distance(scavenger.room.MiddleOfTile(tstPs), tradeSpot.placedObj.pos), tradeSpot.Rad, tradeSpot.Rad + 300f, 0f, 1000f);
		}
		else if (num2 > 0f && tile != creature.pos.Tile)
		{
			num += Custom.LerpMap(tstPs.Tile.FloatDist(tile), 10f + 40f * Mathf.Pow(creature.personality.bravery, 0.5f), 60f, Mathf.Lerp(500f, 0f, Mathf.Pow(creature.personality.dominance, 0.3f)), 0f);
			if (tile.FloatDist(tstPs.Tile) < 3f)
			{
				num -= 1000f;
			}
		}
		for (int j = 0; j < alreadyIdledAt.Count; j++)
		{
			if (tstPs.room == alreadyIdledAt[j].room)
			{
				num -= Mathf.InverseLerp(15f, 3f, tstPs.Tile.FloatDist(alreadyIdledAt[j].Tile)) * Custom.LerpMap(j, 0f, alreadyIdledAt.Count - 1, 100f, 40f) * (0.5f + creature.personality.bravery);
			}
		}
		if ((double)creature.personality.bravery > 0.3)
		{
			num += Mathf.Clamp(scavenger.room.aimap.getAItile(tstPs).visibility, 0f, Custom.LerpMap(creature.personality.bravery * creature.personality.energy, 0.3f, 1f, 50f, 150f));
		}
		else if ((double)creature.personality.bravery < 0.15)
		{
			num -= Mathf.Clamp(scavenger.room.aimap.getAItile(tstPs).visibility, 0f, Custom.LerpMap(creature.personality.bravery, 0f, 0.15f, 50f, 300f));
		}
		for (int k = -1; k < 2; k++)
		{
			if (scavenger.room.aimap.getAItile(tstPs + new IntVector2(k, 0)).acc != AItile.Accessibility.Floor)
			{
				num -= 10f;
			}
			if (scavenger.room.aimap.getAItile(tstPs + new IntVector2(k, 0)).narrowSpace)
			{
				num -= 10f;
			}
		}
		if (!scavenger.room.GetTile(tstPs + new IntVector2(0, -1)).Solid)
		{
			num -= 10f;
		}
		if (scavenger.room.GetTile(tstPs).AnyWater)
		{
			num -= 500f;
		}
		num -= 1000f * base.discomfortTracker.DiscomfortOfTile(tstPs);
		int num3 = int.MaxValue;
		for (int l = 0; l < scavenger.room.abstractRoom.NodesRelevantToCreature(creature.creatureTemplate); l++)
		{
			int num4 = scavenger.room.aimap.ExitDistanceForCreature(tstPs.Tile, l, creature.creatureTemplate);
			if (num4 > 0 && num4 < num3)
			{
				num3 = num4;
			}
		}
		return num + (float)Math.Min(num3, 100);
	}

	public void PackMemberEncounter(Scavenger otherPackMember)
	{
		if (!otherPackMember.AI.NeedAWeapon || !(otherPackMember.AI.threatTracker.Utility() > 1f - creature.personality.sympathy))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < otherPackMember.grasps.Length; i++)
		{
			if (flag)
			{
				break;
			}
			if (otherPackMember.grasps[i] != null && otherPackMember.AI.RealWeapon(otherPackMember.grasps[i].grabbed))
			{
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		int num = 0;
		for (int j = 0; j < scavenger.grasps.Length; j++)
		{
			if (scavenger.grasps[j] != null && RealWeapon(scavenger.grasps[j].grabbed))
			{
				num++;
			}
		}
		if (!flag && num > 1)
		{
			scavenger.GiveWeaponToOther(otherPackMember);
		}
	}

	private void AttackBehavior()
	{
		if (base.preyTracker.MostAttractivePrey == null)
		{
			return;
		}
		focusCreature = base.preyTracker.MostAttractivePrey;
		int num = 0;
		for (int i = 0; i < scavenger.grasps.Length; i++)
		{
			if (scavenger.grasps[i] != null && WeaponScore(scavenger.grasps[i].grabbed, pickupDropInsteadOfWeaponSelection: false) > num)
			{
				num = WeaponScore(scavenger.grasps[i].grabbed, pickupDropInsteadOfWeaponSelection: false);
			}
		}
		if (num > 0)
		{
			CheckThrow();
		}
		int num2 = 0;
		if (num < 3)
		{
			for (int j = 0; j < base.itemTracker.ItemCount; j++)
			{
				if (WeaponScore(base.itemTracker.GetRep(j).representedItem.realizedObject, pickupDropInsteadOfWeaponSelection: false) > num2)
				{
					num2 = WeaponScore(base.itemTracker.GetRep(j).representedItem.realizedObject, pickupDropInsteadOfWeaponSelection: false);
				}
			}
		}
		if (num == 0 || (num < 3 && num2 > 2))
		{
			RetrieveWeapon();
			return;
		}
		if (focusCreature.BestGuessForPosition().room != scavenger.room.abstractRoom.index || focusCreature.TicksSinceSeen > 400)
		{
			creature.abstractAI.SetDestination(focusCreature.BestGuessForPosition());
			if (ModManager.MMF)
			{
				return;
			}
		}
		if (focusCreature.representedCreature.creatureTemplate.PreBakedPathingIndex < 0)
		{
			creatureMovementArea = new List<IntVector2> { focusCreature.BestGuessForPosition().Tile };
		}
		else
		{
			QuickConnectivity.FloodFill(scavenger.room, focusCreature.representedCreature.creatureTemplate, focusCreature.BestGuessForPosition().Tile, 40, 500, creatureMovementArea);
		}
		IntVector2 pos;
		if (UnityEngine.Random.value < 0.5f && creatureMovementArea.Count > 0)
		{
			IntVector2 intVector = creatureMovementArea[UnityEngine.Random.Range(0, creatureMovementArea.Count)];
			pos = intVector;
			int num3 = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
			for (int k = 0; k < 40 && !scavenger.room.GetTile(intVector + new IntVector2(num3 * k, 0)).Solid; k++)
			{
				if (k > 5 && base.pathFinder.CoordinateViable(scavenger.room.GetWorldCoordinate(intVector + new IntVector2(num3 * k, 0))))
				{
					pos = intVector + new IntVector2(num3 * k, 0);
					break;
				}
			}
		}
		else
		{
			pos = creature.pos.Tile + new IntVector2(UnityEngine.Random.Range(1, 10) * ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1)), UnityEngine.Random.Range(1, 10) * ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1)));
			if (ModManager.MMF)
			{
				for (int l = 0; l < 40; l++)
				{
					if (base.pathFinder.CoordinateViable(scavenger.room.GetWorldCoordinate(pos)))
					{
						break;
					}
					pos = creature.pos.Tile + new IntVector2(UnityEngine.Random.Range(1, 10) * ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1)), UnityEngine.Random.Range(1, 10) * ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1)));
				}
			}
		}
		if (base.pathFinder.CoordinateViable(scavenger.room.GetWorldCoordinate(pos)) && SpearThrowPositionScore(scavenger.room.GetWorldCoordinate(pos), focusCreature.BestGuessForPosition().Tile, ref creatureMovementArea) > SpearThrowPositionScore(testThrowPos, focusCreature.BestGuessForPosition().Tile, ref creatureMovementArea))
		{
			testThrowPos = scavenger.room.GetWorldCoordinate(pos);
		}
		if (testThrowPos != base.pathFinder.GetDestination && (Custom.ManhattanDistance(creature.pos, base.pathFinder.GetDestination) < 3 || SpearThrowPositionScore(testThrowPos, focusCreature.BestGuessForPosition().Tile, ref creatureMovementArea) > SpearThrowPositionScore(base.pathFinder.GetDestination, focusCreature.BestGuessForPosition().Tile, ref creatureMovementArea) + (float)changeAttackPositionDelay))
		{
			creature.abstractAI.SetDestination(testThrowPos);
			changeAttackPositionDelay = 400;
			previousAttackPositions.Insert(0, testThrowPos.Tile);
			if (previousAttackPositions.Count > 20)
			{
				previousAttackPositions.RemoveAt(20);
			}
		}
		if (changeAttackPositionDelay > 0)
		{
			changeAttackPositionDelay--;
		}
	}

	public override float CurrentPlayerAggression(AbstractCreature player)
	{
		Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForCreature(player, addIfMissing: false);
		if (scavenger.King)
		{
			return Mathf.Lerp((0.1f + scared / 3f + agitation / 3f) * Mathf.InverseLerp(-3f, 0.15f, Mathf.Sin(age / 100)), 1f, Mathf.InverseLerp(50f, 200f, scavenger.explosionDamageCooldown));
		}
		if (player.realizedCreature != null && scavenger.PlayerHasImmunity(player.realizedCreature as Player))
		{
			return 0f;
		}
		if (creatureRepresentation == null || creatureRepresentation.dynamicRelationship == null)
		{
			return Mathf.InverseLerp(0f, -0.5f, creature.world.game.session.creatureCommunities.LikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (player.state as PlayerState).playerNumber));
		}
		return Mathf.InverseLerp(0.75f, 0.2f, LikeOfPlayer(creatureRepresentation.dynamicRelationship)) * ((!(behavior == Behavior.Attack) || base.preyTracker.MostAttractivePrey == null || base.preyTracker.MostAttractivePrey.representedCreature != player) ? 0.5f : ((currentViolenceType == ViolenceType.Lethal) ? 1f : 0.75f));
	}

	private float SpearThrowPositionScore(WorldCoordinate tst, IntVector2 creaturePosition, ref List<IntVector2> creatureMovementArea)
	{
		if (!base.pathFinder.CoordinateViable(tst))
		{
			return float.MinValue;
		}
		QuickConnectivity.FloodFill(scavenger.room, creature.creatureTemplate, tst.Tile, 40, 500, myMovementArea);
		int num = int.MinValue;
		int num2 = int.MaxValue;
		int count = creatureMovementArea.Count;
		for (int i = 0; i < count; i++)
		{
			IntVector2 intVector = creatureMovementArea[i];
			if (intVector.y > num)
			{
				num = intVector.y;
			}
			if (intVector.y < num2)
			{
				num2 = intVector.y;
			}
		}
		float num3 = 0f;
		int count2 = myMovementArea.Count;
		for (int j = 0; j < count2; j++)
		{
			IntVector2 a = myMovementArea[j];
			if (a.y < num2 || a.y > num || !scavenger.room.VisualContact(a, creaturePosition))
			{
				continue;
			}
			int count3 = creatureMovementArea.Count;
			for (int k = 0; k < count3; k++)
			{
				IntVector2 intVector2 = creatureMovementArea[k];
				if (a.y == intVector2.y && NoSolidTilesBetween(a.x, intVector2.x, a.y))
				{
					num3 += 1f;
				}
			}
		}
		num3 = ((num3 != 0f) ? (num3 + 100f) : 1f);
		int count4 = myMovementArea.Count;
		IntVector2 tile = creature.pos.Tile;
		for (int l = 0; l < count4; l++)
		{
			if (myMovementArea[l].FloatDist(tile) < 3f)
			{
				num3 *= 2f;
				break;
			}
		}
		if (base.pathFinder.GetDestination.room == creature.pos.room)
		{
			for (int m = 0; m < myMovementArea.Count; m++)
			{
				if (myMovementArea[m].FloatDist(base.pathFinder.GetDestination.Tile) < 3f)
				{
					num3 *= 2f;
					break;
				}
			}
		}
		num3 *= Custom.LerpMap(tst.Tile.FloatDist(creaturePosition), 30f, 60f, 1f, 0f);
		num3 *= Custom.LerpMap(tst.Tile.FloatDist(creaturePosition), 5f, 0f, 1f, 0.1f);
		num3 *= 1f - base.threatTracker.ThreatOfArea(tst, accountThreatCreatureAccessibility: true);
		num3 *= ((Math.Abs(tst.Tile.y - creaturePosition.y) < 3) ? 1f : Mathf.InverseLerp(5f, 10f, tst.Tile.FloatDist(creaturePosition)));
		if (scavenger.occupyTile == tst.Tile)
		{
			num3 *= 0.1f;
		}
		int count5 = previousAttackPositions.Count;
		for (int n = 1; n < count5; n++)
		{
			num3 -= Custom.LerpMap(tst.Tile.FloatDist(previousAttackPositions[n]), 0f, 5f, 50f, 0f);
		}
		int creaturesCount = base.tracker.CreaturesCount;
		for (int num4 = 0; num4 < creaturesCount; num4++)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(num4);
			if (rep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger && rep.representedCreature.personality.dominance > creature.personality.dominance && rep.BestGuessForPosition().room == creature.pos.room && rep.representedCreature.realizedCreature != null && rep.representedCreature.abstractAI.RealAI.pathFinder.GetDestination.room == tst.room && (rep.representedCreature.abstractAI.RealAI as ScavengerAI).behavior == Behavior.Attack && (rep.representedCreature.abstractAI.RealAI as ScavengerAI).focusCreature.representedCreature.ID == focusCreature.representedCreature.ID)
			{
				num3 -= Custom.LerpMap(tst.Tile.FloatDist(rep.representedCreature.abstractAI.RealAI.pathFinder.GetDestination.Tile), 0f, 50f, 500f, 0f);
				num3 -= Custom.LerpMap(tst.Tile.FloatDist(rep.representedCreature.abstractAI.RealAI.pathFinder.GetDestination.Tile), 0f, 5f, 500f, 0f);
			}
		}
		if (currentViolenceType == ViolenceType.Warning)
		{
			num3 -= (float)Math.Abs(tst.Tile.y - creaturePosition.y) * 10f;
			num3 -= (float)Math.Abs(Math.Abs(tst.Tile.x - creaturePosition.x) - 10) * 10f;
		}
		if ((currentViolenceType == ViolenceType.Warning || ModManager.MMF) && tst.x != creaturePosition.x && creaturePosition.x != creature.pos.x && tst.x < creaturePosition.x == creature.pos.x < creaturePosition.x)
		{
			num3 += 500f;
		}
		if (behavior == Behavior.GuardOutpost && outpostModule.outpost != null)
		{
			num3 -= Custom.LerpMap(Vector2.Distance(scavenger.room.MiddleOfTile(tst), outpostModule.outpost.placedObj.pos), outpostModule.outpost.Rad * (1f - outpostModule.Utility()), outpostModule.outpost.Rad + 300f, 0f, 1000f);
		}
		return num3;
	}

	private void UpdateCurrentViolenceType()
	{
		if (focusCreature != null)
		{
			ViolenceType violenceType = currentViolenceType;
			currentViolenceType = ViolenceTypeAgainstCreature(focusCreature);
			if (violenceType != currentViolenceType)
			{
				arrangeInventoryCounter = Math.Min(5, arrangeInventoryCounter);
			}
		}
	}

	public ViolenceType ViolenceTypeAgainstCreature(Tracker.CreatureRepresentation critRep)
	{
		return (critRep.dynamicRelationship.state as ScavengerTrackState).taggedViolenceType;
	}

	public void HitAnObjectWithWeapon(Weapon weapon, PhysicalObject hitObject)
	{
		Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForObject(hitObject, AddIfMissing: false);
		if (creatureRepresentation != null)
		{
			(creatureRepresentation.dynamicRelationship.state as ScavengerTrackState).hitsOnThisCreature++;
		}
	}

	private bool NoSolidTilesBetween(int xA, int xB, int y)
	{
		if (xB < xA)
		{
			int num = xA;
			xA = xB;
			xB = num;
		}
		return !scavenger.room.HasAnySolidTileInXRange(y, xA, xB);
	}

	private void RetrieveWeapon()
	{
		CheckForScavangeItems(conservativeBias: false);
		if (scavengeCandidate != null)
		{
			creature.abstractAI.SetDestination(scavengeCandidate.BestGuessForPosition());
		}
	}

	private float PickUpItemScore(ItemTracker.ItemRepresentation rep)
	{
		if (!base.pathFinder.CoordinateViable(rep.BestGuessForPosition()))
		{
			return float.MinValue;
		}
		if (rep.BestGuessForPosition().room != creature.pos.room)
		{
			return -10000f + (float)CollectScore(rep, weaponFiltered: true) * 100f;
		}
		float num = (float)CollectScore(rep, weaponFiltered: true) * 100f;
		num -= rep.BestGuessForPosition().Tile.FloatDist(scavenger.occupyTile);
		if (outpostModule.outpost != null)
		{
			num -= Mathf.Max(0f, outpostModule.outpost.room.GetTilePosition(outpostModule.outpost.placedObj.pos).FloatDist(rep.BestGuessForPosition().Tile) - outpostModule.outpost.Rad / 20f) * 2f;
		}
		if (scavenger.room.socialEventRecognizer.ItemOfferedTo(rep.representedItem.realizedObject) == scavenger)
		{
			num += 100f;
		}
		if (rep.representedItem == giftForMe)
		{
			num += 1000f;
		}
		return num;
	}

	public void CheckThrow()
	{
		if (scavenger.grasps[0] == null || WeaponScore(scavenger.grasps[0].grabbed, pickupDropInsteadOfWeaponSelection: false) < 1 || UnityEngine.Random.value < 0.5f)
		{
			return;
		}
		float num = 0f;
		Creature creature = null;
		ViolenceType violenceType = ViolenceType.None;
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (!base.tracker.GetRep(i).VisualContact || base.tracker.GetRep(i).representedCreature.realizedCreature == null || !((base.tracker.GetRep(i).dynamicRelationship.state as ScavengerTrackState).taggedViolenceType != ViolenceType.None))
			{
				continue;
			}
			float num2 = WantToThrowSpearAtCreature(base.tracker.GetRep(i));
			if (num2 > 0f)
			{
				num2 *= 0.2f + 0.8f * Mathf.InverseLerp(0.5f, 0.7f, Mathf.Abs(Custom.DirVec(scavenger.mainBodyChunk.pos, base.tracker.GetRep(i).representedCreature.realizedCreature.DangerPos).x));
				if (num2 > num)
				{
					num = num2;
					creature = base.tracker.GetRep(i).representedCreature.realizedCreature;
					violenceType = ViolenceTypeAgainstCreature(base.tracker.GetRep(i));
				}
			}
		}
		if (creature == null || (UnityEngine.Random.value < 0.7f && Mathf.Abs(scavenger.flip) > 0.5f && creature.DangerPos.x < scavenger.mainBodyChunk.pos.x != scavenger.flip < 0f))
		{
			return;
		}
		int num3 = -1;
		float num4 = 0f;
		for (int j = 0; j < creature.bodyChunks.Length; j++)
		{
			float num5 = Mathf.Abs(Custom.DirVec(scavenger.mainBodyChunk.pos, creature.bodyChunks[j].pos).x);
			float num6 = 40f;
			if (scavenger.Elite)
			{
				num6 = 40f + Mathf.InverseLerp(150f, 600f, Vector2.Distance(scavenger.mainBodyChunk.pos, creature.bodyChunks[j].pos)) * 60f;
			}
			if (num5 > num4 && (num5 > 0.5f || Mathf.Abs(scavenger.mainBodyChunk.pos.y - creature.bodyChunks[j].pos.y) < num6) && Custom.DistLess(scavenger.mainBodyChunk.pos, creature.bodyChunks[j].pos, 1000f) && SharedPhysics.RayTraceTilesForTerrain(scavenger.room, scavenger.mainBodyChunk.pos, creature.bodyChunks[j].pos))
			{
				num4 = num5;
				num3 = j;
			}
		}
		if (num3 >= 0)
		{
			scavenger.TryThrow(creature.bodyChunks[num3], violenceType);
		}
	}

	private float WantToThrowSpearAtCreature(Tracker.CreatureRepresentation rep)
	{
		if ((rep.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Afraid || creature.pos.Tile.FloatDist(rep.BestGuessForPosition().Tile) > Mathf.Lerp(5f, 500f, UnityEngine.Random.value)) && rep.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Attacks)
		{
			return 0f;
		}
		return Mathf.Pow(rep.dynamicRelationship.currentRelationship.intensity * Custom.LerpMap(creature.pos.Tile.FloatDist(rep.BestGuessForPosition().Tile), 5f, 50f, 1f, 0.5f), (rep == focusCreature) ? 0.2f : 1f);
	}

	private void CheckForScavangeItems(bool conservativeBias)
	{
		int num = -1;
		float num2 = -10000f;
		for (int i = 0; i < base.itemTracker.ItemCount; i++)
		{
			float num3 = PickUpItemScore(base.itemTracker.GetRep(i));
			if (conservativeBias && base.itemTracker.GetRep(i) == scavengeCandidate)
			{
				num3 += 100f;
			}
			if (num3 > num2)
			{
				num = i;
				num2 = num3;
			}
		}
		if (num >= 0)
		{
			scavengeCandidate = base.itemTracker.GetRep(num);
		}
	}

	public bool IsThrowPathClearFromFriends(Vector2 throwPos, float margin)
	{
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).VisualContact && DontWantToThrowAt(base.tracker.GetRep(i)) && base.tracker.GetRep(i).representedCreature.realizedCreature != null)
			{
				Vector2 pos = base.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos;
				if (Vector2.Dot((scavenger.mainBodyChunk.pos - pos).normalized, (scavenger.mainBodyChunk.pos - throwPos).normalized) > 0f && Mathf.Abs(Custom.DistanceToLine(pos, scavenger.mainBodyChunk.pos, throwPos)) < 50f + margin && Custom.DistLess(scavenger.mainBodyChunk.pos, pos, Vector2.Distance(scavenger.mainBodyChunk.pos, throwPos) + margin))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool DontWantToThrowAt(Tracker.CreatureRepresentation rep)
	{
		if (rep.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Afraid)
		{
			return rep.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Attacks;
		}
		return false;
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
		if (scavenger.graphicsModule != null)
		{
			if (noiseReactDelay < UnityEngine.Random.Range(1, 50))
			{
				noiseReactDelay += (int)Mathf.Lerp(180f, 2f, creature.personality.nervous);
				if (scavenger.animation == null || scavenger.animation.id != Scavenger.ScavengerAnimation.ID.Look)
				{
					(scavenger.graphicsModule as ScavengerGraphics).ShockReaction(0.3f + 0.7f * Mathf.InverseLerp(Mathf.Lerp(40f, 10f, creature.personality.bravery), 4f, base.noiseTracker.mysteriousNoises) * Mathf.InverseLerp(0f, 400f, noise.strength));
				}
				else
				{
					(scavenger.graphicsModule as ScavengerGraphics).ShockReaction(0.1f + 0.5f * Mathf.InverseLerp(0f, 400f, noise.strength));
				}
			}
			else
			{
				noiseReactDelay = Math.Max(noiseReactDelay, (int)Mathf.Lerp(20f, 2f, creature.personality.nervous));
			}
		}
		if (source.creatureRep == null && (scavenger.animation == null || !(scavenger.animation.id != Scavenger.ScavengerAnimation.ID.Rummage) || !(scavenger.animation.id != Scavenger.ScavengerAnimation.ID.Look)))
		{
			float f = UnityEngine.Random.value * 0.3f + 0.7f * Mathf.InverseLerp(100f, 1000f, Vector2.Distance(scavenger.mainBodyChunk.pos, noise.pos));
			f = Mathf.Pow(f, Mathf.InverseLerp(40f, 500f, noise.strength));
			if (source.creatureRep != null)
			{
				f = Mathf.InverseLerp(20f, 800f, source.creatureRep.TicksSinceSeen) * Mathf.Pow(source.creatureRep.dynamicRelationship.currentRelationship.intensity, 0.5f);
			}
			if (scavenger.animation == null || !(scavenger.animation.id == Scavenger.ScavengerAnimation.ID.Look) || !((scavenger.animation as Scavenger.LookAnimation).prio > f))
			{
				scavenger.animation = new Scavenger.LookAnimation(scavenger, source.creatureRep, noise.pos, f, base.noiseTracker.mysteriousNoises < 10f && currentUtility < 0.4f);
			}
		}
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation otherCreature)
	{
		if (firstSpot)
		{
			base.noiseTracker.ClearAllUnkown();
			CreatureTemplate.Relationship relationship = creature.creatureTemplate.CreatureRelationship(otherCreature.representedCreature.creatureTemplate).Duplicate();
			if (relationship.type != CreatureTemplate.Relationship.Type.Uncomfortable)
			{
				relationship.intensity = Mathf.Lerp(relationship.intensity, 1f, 0.5f);
			}
			if (relationship.type == CreatureTemplate.Relationship.Type.SocialDependent)
			{
				relationship.type = CreatureTemplate.Relationship.Type.Afraid;
				if (creature.state.socialMemory.GetRelationship(otherCreature.representedCreature.ID) != null)
				{
					relationship.intensity = Custom.LerpMap(creature.state.socialMemory.GetRelationship(otherCreature.representedCreature.ID).like, -1f, 1f, 1f, 0f);
				}
				else
				{
					relationship.intensity = 0.5f;
				}
			}
			if (scavenger.graphicsModule != null && relationship.type != CreatureTemplate.Relationship.Type.Ignores && relationship.type != CreatureTemplate.Relationship.Type.Pack)
			{
				(scavenger.graphicsModule as ScavengerGraphics).ShockReaction((relationship.type == CreatureTemplate.Relationship.Type.Uncomfortable) ? 0.2f : 1f);
			}
			if (otherCreature.representedCreature.realizedCreature != null && !scavenger.safariControlled)
			{
				scavenger.mainBodyChunk.vel += Custom.DirVec(otherCreature.representedCreature.realizedCreature.DangerPos, scavenger.mainBodyChunk.pos) * relationship.intensity * 2f;
			}
			bool flag = false;
			if (!scavenger.safariControlled && relationship.intensity > 1f / 3f && creature.personality.sympathy > 0.2f && (relationship.type == CreatureTemplate.Relationship.Type.Attacks || relationship.type == CreatureTemplate.Relationship.Type.Afraid) && otherCreature.representedCreature.realizedCreature != null && otherCreature.BestGuessForPosition().Tile.FloatDist(creature.pos.Tile) > Mathf.Lerp(20f, 6f, creature.personality.sympathy))
			{
				List<Tracker.CreatureRepresentation> list = CloseByPack();
				int num = 0;
				bool flag2 = false;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].representedCreature.realizedCreature is Scavenger)
					{
						if ((list[i].representedCreature.abstractAI.RealAI as ScavengerAI).tracker.RepresentationForObject(otherCreature.representedCreature.realizedCreature, AddIfMissing: false) == null)
						{
							num++;
						}
						if (UnityEngine.Random.value < 0.5f && list[i].VisualContact && (list[i].representedCreature.realizedCreature as Scavenger).Pointing)
						{
							flag2 = true;
						}
					}
					else
					{
						num++;
					}
				}
				if (num > ((list.Count >= 3) ? 1 : 0) && !flag2)
				{
					flag = true;
					scavenger.animation = new Scavenger.GeneralPointAnimation(scavenger, otherCreature, otherCreature.representedCreature.realizedCreature.DangerPos, list);
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].representedCreature.realizedCreature is Scavenger)
						{
							(list[j].representedCreature.abstractAI.RealAI as ScavengerAI).MakeLookHere(scavenger.mainBodyChunk.pos);
							(list[j].representedCreature.realizedCreature as Scavenger).visionFactor = 0f;
							(list[j].representedCreature.realizedCreature as Scavenger).narrowVision = 1f;
						}
					}
				}
			}
			if (!flag)
			{
				scavenger.animation = new Scavenger.LookAnimation(scavenger, otherCreature, (otherCreature.representedCreature.realizedCreature != null) ? otherCreature.representedCreature.realizedCreature.DangerPos : scavenger.bodyChunks[2].pos, relationship.intensity, relationship.type == CreatureTemplate.Relationship.Type.Afraid || relationship.type == CreatureTemplate.Relationship.Type.Attacks);
			}
			if (!ModManager.MMF || relationship.type == CreatureTemplate.Relationship.Type.Attacks || relationship.type == CreatureTemplate.Relationship.Type.Afraid)
			{
				agitation = Mathf.Lerp(agitation, 1f, relationship.intensity * 0.4f);
			}
		}
		else if (otherCreature.TicksSinceSeen > 400)
		{
			float num2 = ((otherCreature.dynamicRelationship != null) ? otherCreature.dynamicRelationship.currentRelationship.intensity : scavenger.Template.CreatureRelationship(otherCreature.representedCreature.creatureTemplate).intensity);
			if (scavenger.animation == null || (scavenger.animation.id == Scavenger.ScavengerAnimation.ID.Look && (scavenger.animation as Scavenger.LookAnimation).prio < num2))
			{
				scavenger.animation = new Scavenger.LookAnimation(scavenger, otherCreature, (otherCreature.representedCreature.realizedCreature != null) ? otherCreature.representedCreature.realizedCreature.DangerPos : scavenger.bodyChunks[2].pos, otherCreature.dynamicRelationship.currentRelationship.intensity, stop: false);
				if (scavenger.graphicsModule != null && creature.creatureTemplate.CreatureRelationship(otherCreature.representedCreature.creatureTemplate).type != CreatureTemplate.Relationship.Type.Ignores && creature.creatureTemplate.CreatureRelationship(otherCreature.representedCreature.creatureTemplate).type != CreatureTemplate.Relationship.Type.Pack)
				{
					(scavenger.graphicsModule as ScavengerGraphics).ShockReaction(0.1f + 0.6f * num2);
				}
			}
		}
		base.CreatureSpotted(firstSpot, otherCreature);
	}

	private List<Tracker.CreatureRepresentation> CloseByPack()
	{
		List<Tracker.CreatureRepresentation> list = new List<Tracker.CreatureRepresentation>();
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack && base.tracker.GetRep(i).representedCreature.state.alive && base.tracker.GetRep(i).representedCreature.realizedCreature != null && base.tracker.GetRep(i).representedCreature.realizedCreature.room == scavenger.room && (base.tracker.GetRep(i).VisualContact || base.tracker.GetRep(i).TicksSinceSeen < 40 || (Custom.DistLess(scavenger.mainBodyChunk.pos, base.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos, 800f) && scavenger.room.VisualContact(scavenger.mainBodyChunk.pos, base.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos))))
			{
				list.Add(base.tracker.GetRep(i));
			}
		}
		return list;
	}

	public void MakeLookHere(Vector2 point)
	{
		if (!(base.threatTracker.Utility() > 0.9f) && (currentUtility < 0.6f || (behavior != Behavior.Attack && behavior != Behavior.Flee)))
		{
			scavenger.animation = new Scavenger.LookAnimation(scavenger, null, point, 1f, stop: true);
		}
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
		bool flag = objectCrit == scavenger;
		if (!flag)
		{
			creatureRepresentation2 = base.tracker.RepresentationForObject(objectCrit, AddIfMissing: false);
			if (creatureRepresentation2 == null)
			{
				return;
			}
		}
		if ((!flag && scavenger.dead) || (creatureRepresentation2 != null && creatureRepresentation.TicksSinceSeen > 40 && creatureRepresentation2.TicksSinceSeen > 40))
		{
			return;
		}
		float num = 0f;
		if (ID == SocialEventRecognizer.EventID.Theft)
		{
			num = 0.1f * (float)CollectScore(involvedItem, weaponFiltered: false);
		}
		else if (ID == SocialEventRecognizer.EventID.NonLethalAttackAttempt)
		{
			num = 0.1f;
		}
		else if (ID == SocialEventRecognizer.EventID.NonLethalAttack)
		{
			num = 0.25f;
		}
		else if (ID == SocialEventRecognizer.EventID.LethalAttackAttempt)
		{
			num = 0.4f;
		}
		else if (ID == SocialEventRecognizer.EventID.LethalAttack)
		{
			num = 0.8f;
		}
		else if (ID == SocialEventRecognizer.EventID.Killing)
		{
			num = 1f;
		}
		else
		{
			if (ID == SocialEventRecognizer.EventID.ItemOffering)
			{
				RecognizePlayerOfferingGift(creatureRepresentation, creatureRepresentation2, flag, involvedItem);
				return;
			}
			if (ID == SocialEventRecognizer.EventID.ItemTransaction)
			{
				RecognizeCreatureAcceptingGift(creatureRepresentation, creatureRepresentation2, flag, involvedItem);
				return;
			}
		}
		if (objectCrit.dead)
		{
			num /= 3f;
		}
		if (objectCrit is Player && num > 0f)
		{
			WitnessPlayerOnPlayerViolence(creatureRepresentation, creatureRepresentation2, num);
		}
		else if (flag)
		{
			if (scavenger.graphicsModule != null)
			{
				(scavenger.graphicsModule as ScavengerGraphics).ShockReaction(Mathf.InverseLerp(0f, 0.35f, num));
			}
			ScavPlayerRelationChange(0f - num, subjectCrit.abstractCreature);
			if (!(subjectCrit is Player))
			{
				return;
			}
			if (num > 0.5f && outpostModule.outpost != null)
			{
				outpostModule.outpost.ScavengerReportTransgression(subjectCrit as Player);
			}
			if (tradeSpot != null && (num >= 0.5f || ID == SocialEventRecognizer.EventID.Theft))
			{
				Custom.Log("PLAYER MESSING WITH TRADER!");
				tradeSpot.worldTradeSpot.transgressedByPlayer = true;
				tradeSpot = null;
				if ((creature.abstractAI as ScavengerAbstractAI).squad != null && (creature.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
				{
					(creature.abstractAI as ScavengerAbstractAI).squad.RemoveMember(creature);
				}
				num = 2f;
			}
		}
		else if (creatureRepresentation2.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			float num2 = 0.1f;
			if (base.threatTracker.GetThreatCreature(objectCrit.abstractCreature) != null)
			{
				num2 += 0.7f * Custom.LerpMap(Vector2.Distance(scavenger.mainBodyChunk.pos, objectCrit.DangerPos), 120f, 320f, 1f, 0.1f);
			}
			if ((creatureRepresentation2.dynamicRelationship.state as ScavengerTrackState).holdingAFriend)
			{
				num2 += 0.8f;
			}
			bool flag2 = false;
			for (int i = 0; i < objectCrit.grasps.Length; i++)
			{
				if (flag2)
				{
					break;
				}
				if (objectCrit.grasps[i] != null && objectCrit.grasps[i].grabbed == scavenger)
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
			ScavPlayerRelationChange(Mathf.Pow(num, 0.5f) * num2, subjectCrit.abstractCreature);
		}
		else if (creatureRepresentation2.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack)
		{
			ScavPlayerRelationChange((0f - num) * 0.75f, subjectCrit.abstractCreature);
			if (num > 0.5f && outpostModule.outpost != null && subjectCrit is Player)
			{
				outpostModule.outpost.ScavengerReportTransgression(subjectCrit as Player);
			}
		}
	}

	private void WitnessPlayerOnPlayerViolence(Tracker.CreatureRepresentation subjectCritRep, Tracker.CreatureRepresentation objectCritRep, float violenceSeverity)
	{
		if (subjectCritRep.dynamicRelationship != null && objectCritRep.dynamicRelationship != null)
		{
			float a = creature.world.game.session.creatureCommunities.LikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (subjectCritRep.representedCreature.state as PlayerState).playerNumber);
			float a2 = creature.world.game.session.creatureCommunities.LikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (objectCritRep.representedCreature.state as PlayerState).playerNumber);
			float num = Mathf.InverseLerp(-1f, 1f, creature.state.socialMemory.GetTempLike(subjectCritRep.representedCreature.ID));
			float num2 = Mathf.InverseLerp(-1f, 1f, creature.state.socialMemory.GetTempLike(objectCritRep.representedCreature.ID));
			a = Mathf.Lerp(a, num, Mathf.Abs(num) * 0.5f);
			a2 = Mathf.Lerp(a2, num2, Mathf.Abs(num2) * 0.5f);
			float change = -0.5f * a2 * violenceSeverity * Mathf.Pow(Mathf.Clamp(Mathf.Abs(a - a2), 0f, 1f), 0.25f);
			ScavPlayerRelationChange(change, subjectCritRep.representedCreature);
		}
	}

	private void ScavPlayerRelationChange(float change, AbstractCreature player)
	{
		SocialMemory.Relationship orInitiateRelationship = creature.state.socialMemory.GetOrInitiateRelationship(player.ID);
		orInitiateRelationship.InfluenceTempLike(change * 1.5f);
		orInitiateRelationship.InfluenceLike(change * 0.5f);
		orInitiateRelationship.InfluenceKnow(Mathf.Abs(change));
		creature.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (player.state as PlayerState).playerNumber, change * 0.1f, 0.75f, 0.25f);
	}

	private void RecognizePlayerOfferingGift(Tracker.CreatureRepresentation subRep, Tracker.CreatureRepresentation objRep, bool objIsMe, PhysicalObject item)
	{
		if (CollectScore(item, weaponFiltered: false) < 2)
		{
			return;
		}
		if (subRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
		{
			SocialMemory.Relationship orInitiateRelationship = creature.state.socialMemory.GetOrInitiateRelationship(subRep.representedCreature.ID);
			if (orInitiateRelationship.tempLike < -0.9f)
			{
				return;
			}
			orInitiateRelationship.InfluenceTempLike((float)CollectScore(item, weaponFiltered: false) / 6f * (objIsMe ? 1f : 0.5f));
		}
		if (objIsMe)
		{
			Custom.Log("Player is offering me an item!", CollectScore(item, weaponFiltered: false).ToString());
			giftForMe = item.abstractPhysicalObject;
			scavengeCandidate = base.itemTracker.RepresentationForObject(item, AddIfMissing: true);
			creature.abstractAI.SetDestination(item.room.GetWorldCoordinate(item.firstChunk.pos));
		}
	}

	private void RecognizeCreatureAcceptingGift(Tracker.CreatureRepresentation subRep, Tracker.CreatureRepresentation objRep, bool objIsMe, PhysicalObject item)
	{
		if (subRep.representedCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat || (!objIsMe && objRep != null && objRep.representedCreature.creatureTemplate.type != CreatureTemplate.Type.Scavenger) || CollectScore(item, weaponFiltered: false) < 2)
		{
			return;
		}
		SocialMemory.Relationship orInitiateRelationship = creature.state.socialMemory.GetOrInitiateRelationship(subRep.representedCreature.ID);
		if (objIsMe)
		{
			Custom.Log("I have accepted a gift from a player!", CollectScore(item, weaponFiltered: false).ToString());
			if (outpostModule.outpost != null && subRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && subRep.representedCreature.realizedCreature != null && Custom.DistLess(subRep.representedCreature.realizedCreature.mainBodyChunk.pos, outpostModule.outpost.placedObj.pos, outpostModule.outpost.Rad + 700f))
			{
				outpostModule.outpost.FeeRecieved(subRep.representedCreature.realizedCreature as Player, item.abstractPhysicalObject, CollectScore(item, weaponFiltered: false));
				Custom.Log("outpost payment");
				orInitiateRelationship.InfluenceLike((float)CollectScore(item, weaponFiltered: false) / 10f * 0.05f);
				orInitiateRelationship.InfluenceTempLike((float)CollectScore(item, weaponFiltered: false) / 10f * 0.2f);
			}
			else
			{
				float num = scavenger.room.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, scavenger.room.game.world.RegionNumber, (subRep.representedCreature.state as PlayerState).playerNumber);
				if (subRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
				{
					bool flag = false;
					if (num > -0.5f && subRep.representedCreature.realizedCreature != null)
					{
						int num2 = CollectScore(item, weaponFiltered: false);
						int num3 = 0;
						int num4 = -1;
						int num5 = 0;
						for (int i = 0; i < scavenger.grasps.Length; i++)
						{
							if (scavenger.grasps[i] == null || scavenger.grasps[i].grabbed == item)
							{
								continue;
							}
							int num6 = CollectScore(scavenger.grasps[i].grabbed, weaponFiltered: false);
							if (num6 > 0)
							{
								num3 += num6;
								if (num6 < num2 && num6 > num5)
								{
									num4 = i;
									num5 = num6;
								}
							}
						}
						if (num3 < CollectScore(item, weaponFiltered: false))
						{
							for (int j = 0; j < scavenger.grasps.Length; j++)
							{
								if (scavenger.grasps[j] != null && scavenger.grasps[j].grabbed != item && CollectScore(scavenger.grasps[j].grabbed, weaponFiltered: false) > 0)
								{
									PhysicalObject grabbed = scavenger.grasps[j].grabbed;
									scavenger.room.socialEventRecognizer.CreaturePutItemOnGround(grabbed, scavenger);
									scavenger.ReleaseGrasp(j);
									grabbed.firstChunk.vel = Custom.DirVec(grabbed.firstChunk.pos, subRep.representedCreature.realizedCreature.mainBodyChunk.pos) * 3f + Custom.RNV() + new Vector2(0f, 3f);
								}
							}
						}
						else if (num4 > -1)
						{
							PhysicalObject grabbed2 = scavenger.grasps[num4].grabbed;
							scavenger.room.socialEventRecognizer.CreaturePutItemOnGround(grabbed2, scavenger);
							scavenger.ReleaseGrasp(num4);
							grabbed2.firstChunk.vel = Custom.DirVec(grabbed2.firstChunk.pos, subRep.representedCreature.realizedCreature.mainBodyChunk.pos) * 3f + Custom.RNV() + new Vector2(0f, 3f);
						}
						int num7 = 0;
						for (int k = 0; k < scavenger.room.socialEventRecognizer.ownedItemsOnGround.Count; k++)
						{
							if (scavenger.room.socialEventRecognizer.ownedItemsOnGround[k].owner == scavenger && !scavenger.room.socialEventRecognizer.ownedItemsOnGround[k].offered)
							{
								Custom.Log($"{scavenger.room.socialEventRecognizer.ownedItemsOnGround[k].item.abstractPhysicalObject.type} {scavenger.room.socialEventRecognizer.ownedItemsOnGround[k].item.abstractPhysicalObject.ID}");
								num7++;
								scavenger.room.socialEventRecognizer.ownedItemsOnGround[k].offered = true;
								scavenger.room.socialEventRecognizer.ownedItemsOnGround[k].offeredTo = subRep.representedCreature.realizedCreature;
								base.discomfortTracker.uncomfortableItem = base.itemTracker.RepresentationForObject(scavenger.room.socialEventRecognizer.ownedItemsOnGround[k].item, AddIfMissing: true);
								base.discomfortTracker.uncomfortableItemDiscomfort = 100f;
							}
						}
						Custom.Log("offered", num7.ToString(), "items to player");
						if (num7 > 0)
						{
							flag = true;
						}
					}
					Custom.Log(flag ? "Trade" : "Gift");
					float num8 = Custom.LerpMap(num, -0.9f, 0f, 0.001f, 0.02f) * (float)CollectScore(item, weaponFiltered: false) * (flag ? 0.5f : 1f);
					if (item is DataPearl)
					{
						num8 += 0.1f;
					}
					scavenger.room.game.session.creatureCommunities.InfluenceLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, scavenger.room.game.world.RegionNumber, (subRep.representedCreature.state as PlayerState).playerNumber, num8, 0.75f, 0f);
					orInitiateRelationship.InfluenceLike((float)CollectScore(item, weaponFiltered: false) / 7.5f * (flag ? 0.3f : 0.6f));
					orInitiateRelationship.InfluenceTempLike(Mathf.Max(0.5f, (float)CollectScore(item, weaponFiltered: false) / 5f * (flag ? 0.3f : 0.6f)));
				}
			}
			if (item is DataPearl)
			{
				orInitiateRelationship.InfluenceTempLike(2f);
			}
		}
		else
		{
			orInitiateRelationship.InfluenceLike((float)CollectScore(item, weaponFiltered: false) / 7.5f * 0.05f);
			orInitiateRelationship.InfluenceTempLike((float)CollectScore(item, weaponFiltered: false) / 5f * 0.1f);
			if (item is DataPearl)
			{
				orInitiateRelationship.InfluenceTempLike(1f);
			}
		}
	}

	public void ControlledBehavior()
	{
		if ((base.creature.abstractAI as ScavengerAbstractAI).squad != null)
		{
			(base.creature.abstractAI as ScavengerAbstractAI).squad.RemoveMember(scavenger.abstractCreature);
			(base.creature.abstractAI as ScavengerAbstractAI).squad = null;
		}
		(base.creature.abstractAI as ScavengerAbstractAI).controlledMigrateTime--;
		backedByPack = 0f;
		scavageItemCheck = 200;
		goToSquadLeaderFirstTime = 1000;
		arrangeInventoryCounter = 400;
		runSpeedGoal = 1f;
		scavengeCandidate = null;
		agitation = 0f;
		scared = 0f;
		discomfortWithOtherCreatures = 0f;
		idleCounter = 0;
		alreadyIdledAt.Clear();
		tradeSpot = null;
		wantToTradeWith = null;
		giftForMe = null;
		scavenger.GoThroughFloors = false;
		controlWalkCooldown--;
		if (base.creature.abstractAI.path.Count > 0)
		{
			base.creature.abstractAI.FollowPath(1);
		}
		base.noiseTracker.hearingSkill = (scavenger.moving ? 0.8f : Mathf.Lerp(1.4f, 0.9f, agitation));
		for (int i = 0; i < base.creature.state.socialMemory.relationShips.Count; i++)
		{
			base.creature.state.socialMemory.relationShips[i].EvenOutTemps(0.0005f);
			if (base.creature.state.socialMemory.relationShips[i].subjectID.number == 0)
			{
				likeB = base.creature.state.socialMemory.relationShips[i].like;
				tempLikeB = base.creature.state.socialMemory.relationShips[i].tempLike;
			}
		}
		if (scavenger.inputWithDiagonals.HasValue)
		{
			if (scavenger.inputWithDiagonals.Value.pckp)
			{
				if (lastPickupPressed != 0)
				{
					numPickupPressed++;
					if (numPickupPressed == 2)
					{
						numPickupPressed = 0;
						scavenger.ControlCycleInventory();
					}
				}
				lastPickupPressed = 0;
				if (pickupDownTime == 0)
				{
					if (scavenger.inputWithDiagonals.Value.y < 0)
					{
						scavenger.CycleItemIntoPrimaryHand();
						if (scavenger.grasps[0] != null)
						{
							scavenger.ReleaseGrasp(0);
						}
					}
					else
					{
						scavenger.LookForItemsToPickUp();
					}
				}
				pickupDownTime++;
			}
			else
			{
				lastPickupPressed++;
				if (lastPickupPressed > 20)
				{
					numPickupPressed = 0;
				}
				pickupDownTime = 0;
			}
		}
		else
		{
			lastPickupPressed++;
			if (lastPickupPressed > 20)
			{
				numPickupPressed = 0;
			}
			pickupDownTime = 0;
		}
		if (scavenger.room == null)
		{
			return;
		}
		base.pathFinder.walkPastPointOfNoReturn = true;
		UpdateLookPoint();
		runSpeedGoal = 0f;
		if (!scavenger.inputWithDiagonals.HasValue)
		{
			return;
		}
		if (scavenger.inputWithDiagonals.Value.thrw)
		{
			scavenger.CycleItemIntoPrimaryHand();
			float num = -1f;
			Creature creature = null;
			for (int j = 0; j < base.tracker.CreaturesCount; j++)
			{
				if (base.tracker.GetRep(j).VisualContact && base.tracker.GetRep(j).representedCreature.realizedCreature != null && base.tracker.GetRep(j).representedCreature.creatureTemplate.type != CreatureTemplate.Type.Scavenger && base.tracker.GetRep(j).dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Pack)
				{
					float num2 = WantToThrowSpearAtCreature(base.tracker.GetRep(j));
					num2 *= 0.2f + 0.8f * Mathf.InverseLerp(0.5f, 0.7f, Mathf.Abs(Custom.DirVec(scavenger.mainBodyChunk.pos, base.tracker.GetRep(j).representedCreature.realizedCreature.DangerPos).x));
					if (num2 > num)
					{
						num = num2;
						creature = base.tracker.GetRep(j).representedCreature.realizedCreature;
					}
				}
			}
			int num3 = -1;
			if (creature != null)
			{
				float num4 = 0f;
				for (int k = 0; k < creature.bodyChunks.Length; k++)
				{
					float num5 = Mathf.Abs(Custom.DirVec(scavenger.mainBodyChunk.pos, creature.bodyChunks[k].pos).x);
					if (num5 > num4 && (num5 > 0.5f || Mathf.Abs(scavenger.mainBodyChunk.pos.y - creature.bodyChunks[k].pos.y) < 40f) && Custom.DistLess(scavenger.mainBodyChunk.pos, creature.bodyChunks[k].pos, 1000f) && SharedPhysics.RayTraceTilesForTerrain(scavenger.room, scavenger.mainBodyChunk.pos, creature.bodyChunks[k].pos))
					{
						num4 = num5;
						num3 = k;
					}
				}
			}
			if (num3 >= 0)
			{
				scavenger.TryThrow(creature.bodyChunks[num3], ViolenceType.Lethal);
				return;
			}
			Vector2? aimPosition = scavenger.mainBodyChunk.pos + new Vector2(scavenger.flip * 250f, 0f);
			scavenger.TryThrow(null, ViolenceType.Lethal, aimPosition);
		}
		else if (scavenger.IsControlPointing && !scavenger.swingPos.HasValue)
		{
			runSpeedGoal = 0f;
			bool flag = false;
			if (scavenger.animation == null || !(scavenger.animation is Scavenger.GeneralPointAnimation))
			{
				scavenger.animation = new Scavenger.GeneralPointAnimation(scavenger, null, scavenger.HeadLookPoint, new List<Tracker.CreatureRepresentation>());
				if (scavenger.graphicsModule != null)
				{
					(scavenger.graphicsModule as ScavengerGraphics).ShockReaction(UnityEngine.Random.Range(0.25f, 1f));
					flag = true;
				}
			}
			if (!scavenger.lastInputWithDiagonals.Value.jmp && !flag)
			{
				(scavenger.graphicsModule as ScavengerGraphics).ShockReaction(UnityEngine.Random.Range(0.25f, 1f));
				flag = true;
			}
			Scavenger.GeneralPointAnimation generalPointAnimation = scavenger.animation as Scavenger.GeneralPointAnimation;
			generalPointAnimation.creatureRep = null;
			generalPointAnimation.stop = false;
			if (scavenger.inputWithDiagonals.Value.x != 0 || scavenger.inputWithDiagonals.Value.y != 0)
			{
				Vector2 point = scavenger.mainBodyChunk.pos + new Vector2(scavenger.inputWithDiagonals.Value.x, scavenger.inputWithDiagonals.Value.y) * 150f;
				generalPointAnimation.point = point;
				if (scavenger.lastInputWithDiagonals.Value.x == 0 && scavenger.lastInputWithDiagonals.Value.y == 0)
				{
					if (scavenger.graphicsModule != null && !flag)
					{
						(scavenger.graphicsModule as ScavengerGraphics).ShockReaction(UnityEngine.Random.Range(0.1f, 0.4f));
					}
				}
				else if ((Mathf.Sign(scavenger.lastInputWithDiagonals.Value.x) != Mathf.Sign(scavenger.inputWithDiagonals.Value.x) || Mathf.Sign(scavenger.lastInputWithDiagonals.Value.y) != Mathf.Sign(scavenger.inputWithDiagonals.Value.y)) && scavenger.graphicsModule != null && !flag)
				{
					(scavenger.graphicsModule as ScavengerGraphics).ShockReaction(UnityEngine.Random.Range(0f, 0.2f));
				}
			}
			else
			{
				generalPointAnimation.point = scavenger.HeadLookPoint;
			}
		}
		else
		{
			if (scavenger.inputWithDiagonals.Value.x == 0 && scavenger.inputWithDiagonals.Value.y == 0)
			{
				return;
			}
			runSpeedGoal = 1f;
			if (scavenger.inputWithDiagonals.Value.y < 0)
			{
				scavenger.GoThroughFloors = true;
			}
			if (scavenger.shortcutDelay < 1 && scavenger.grabbedBy.Count == 0)
			{
				for (int l = 0; l <= 2; l++)
				{
					if (scavenger.enteringShortCut.HasValue || scavenger.room.GetTile(scavenger.bodyChunks[l].pos).Terrain != Room.Tile.TerrainType.ShortcutEntrance || !(scavenger.room.shortcutData(scavenger.room.GetTilePosition(scavenger.bodyChunks[l].pos)).shortCutType != ShortcutData.Type.DeadEnd) || !(scavenger.room.shortcutData(scavenger.room.GetTilePosition(scavenger.bodyChunks[l].pos)).shortCutType != ShortcutData.Type.CreatureHole))
					{
						continue;
					}
					IntVector2 intVector = scavenger.room.ShorcutEntranceHoleDirection(scavenger.room.GetTilePosition(scavenger.bodyChunks[l].pos));
					if ((Custom.SignZero(scavenger.inputWithDiagonals.Value.x) == 0f - Custom.SignZero(intVector.x) && intVector.x != 0) || (Custom.SignZero(scavenger.inputWithDiagonals.Value.y) == 0f - Custom.SignZero(intVector.y) && intVector.y != 0))
					{
						scavenger.enteringShortCut = scavenger.room.GetTilePosition(scavenger.bodyChunks[l].pos);
						if (scavenger.room.shortcutData(scavenger.room.GetTilePosition(scavenger.bodyChunks[l].pos)).shortCutType == ShortcutData.Type.RegionTransportation)
						{
							(scavenger.abstractCreature.abstractAI as ScavengerAbstractAI).ControlledLongTermDestination();
							scavenger.NPCTransportationDestination = (scavenger.abstractCreature.abstractAI as ScavengerAbstractAI).longTermMigration;
						}
						else
						{
							bool flag2 = false;
							List<IntVector2> list = new List<IntVector2>();
							ShortcutData[] shortcuts = scavenger.room.shortcuts;
							for (int m = 0; m < shortcuts.Length; m++)
							{
								ShortcutData shortcutData = shortcuts[m];
								if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != scavenger.room.GetTilePosition(scavenger.bodyChunks[l].pos))
								{
									list.Add(shortcutData.StartTile);
								}
								if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == scavenger.room.GetTilePosition(scavenger.bodyChunks[l].pos))
								{
									flag2 = true;
								}
							}
							if (flag2 && list.Count > 0)
							{
								list.Shuffle();
								scavenger.NPCTransportationDestination = scavenger.room.GetWorldCoordinate(list[0]);
							}
						}
						scavenger.shortcutDelay = 100;
						break;
					}
					scavenger.enteringShortCut = null;
				}
			}
			if (controlWalkCooldown > 0)
			{
				return;
			}
			int num6 = 0;
			int num7 = 0;
			if (scavenger.inputWithDiagonals.Value.x != 0)
			{
				num7 = (int)Mathf.Sign(scavenger.inputWithDiagonals.Value.x);
			}
			if (scavenger.inputWithDiagonals.Value.y != 0)
			{
				num6 = (int)Mathf.Sign(scavenger.inputWithDiagonals.Value.y);
			}
			IntVector2? intVector2 = null;
			float num8 = float.MinValue;
			if (num7 != 0)
			{
				for (int n = 0; n < 6 && (!intVector2.HasValue || n <= 1); n++)
				{
					for (int num9 = 7; num9 >= 1; num9--)
					{
						for (int num10 = -1; num10 <= 1; num10 += 2)
						{
							IntVector2 pos = new IntVector2(scavenger.occupyTile.x + num7 * num9, scavenger.occupyTile.y + n * num10);
							bool flag3 = scavenger.room.GetTile(pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance;
							IntVector2? intVector3 = null;
							if (flag3)
							{
								intVector3 = scavenger.room.ShorcutEntranceHoleDirection(pos);
							}
							if (n > 1)
							{
								flag3 = false;
							}
							if ((num9 <= 3 && intVector2.HasValue && !flag3) || (num9 <= 1 && !flag3))
							{
								continue;
							}
							WorldCoordinate worldCoordinate = scavenger.room.GetWorldCoordinate(pos);
							if (base.pathFinder.CoordinateViable(worldCoordinate) || flag3)
							{
								float num11 = IdleScore(worldCoordinate);
								if (flag3 && Custom.SignZero(scavenger.inputWithDiagonals.Value.x) == 0f - Custom.SignZero(intVector3.Value.x) && intVector3.Value.x != 0)
								{
									num11 = Mathf.Max(10000f, num8 + 10000f);
								}
								if (num11 > num8)
								{
									intVector2 = worldCoordinate.Tile;
									num8 = num11;
								}
							}
						}
					}
				}
			}
			if (num6 != 0)
			{
				for (int num12 = 0; num12 < 6 && (!intVector2.HasValue || num12 <= 1); num12++)
				{
					for (int num13 = 7; num13 >= 1; num13--)
					{
						for (int num14 = -1; num14 <= 1; num14 += 2)
						{
							IntVector2 pos2 = new IntVector2(scavenger.occupyTile.x + num12 * num14, scavenger.occupyTile.y + num6 * num13);
							bool flag4 = scavenger.room.GetTile(pos2).Terrain == Room.Tile.TerrainType.ShortcutEntrance;
							IntVector2? intVector4 = null;
							if (flag4)
							{
								intVector4 = scavenger.room.ShorcutEntranceHoleDirection(pos2);
							}
							if (num12 > 1)
							{
								flag4 = false;
							}
							if (num13 <= 1 && !flag4)
							{
								continue;
							}
							WorldCoordinate worldCoordinate2 = scavenger.room.GetWorldCoordinate(pos2);
							if (base.pathFinder.CoordinateViable(worldCoordinate2) || flag4)
							{
								float num15 = IdleScore(worldCoordinate2);
								if (flag4 && Custom.SignZero(scavenger.inputWithDiagonals.Value.y) == 0f - Custom.SignZero(intVector4.Value.y) && intVector4.Value.y != 0)
								{
									num15 = Mathf.Max(10000f, num8 + 10000f);
								}
								if (num15 > num8)
								{
									intVector2 = worldCoordinate2.Tile;
									num8 = num15;
								}
							}
						}
					}
				}
			}
			if (intVector2.HasValue && (scavenger.abstractCreature.abstractAI as ScavengerAbstractAI).controlledMigrateTime <= 0)
			{
				base.creature.abstractAI.SetDestination(scavenger.room.GetWorldCoordinate(intVector2.Value));
				controlStuckTime = 0;
				controlWalkCooldown = 10;
				return;
			}
			for (int num16 = 0; num16 < 100; num16++)
			{
				int num17 = (((double)UnityEngine.Random.value < 0.5) ? 1 : (-1));
				int num18 = (((double)UnityEngine.Random.value < 0.5) ? 1 : (-1));
				int minInclusive = Mathf.Min((40 - num16 / 5) * num17, (20 - num16 / 10) * num17);
				int maxExclusive = Mathf.Max((40 - num16 / 5) * num17, (20 - num16 / 10) * num17);
				int minInclusive2 = Mathf.Min((40 - num16 / 5) * num18, (20 - num16 / 10) * num18);
				int maxExclusive2 = Mathf.Max((40 - num16 / 5) * num18, (20 - num16 / 10) * num18);
				IntVector2 intVector5 = new IntVector2(UnityEngine.Random.Range(minInclusive, maxExclusive), UnityEngine.Random.Range(minInclusive2, maxExclusive2));
				if ((num7 != 0 && Mathf.Sign(num7) != Mathf.Sign(intVector5.x)) || (num6 != 0 && Mathf.Sign(num6) != Mathf.Sign(intVector5.y)) || Mathf.Abs(Custom.VecToDeg(Custom.DirVec(Vector2.zero, intVector5.ToVector2())) - Custom.VecToDeg(Custom.DirVec(Vector2.zero, new Vector2(num7, num6)))) > 45f)
				{
					continue;
				}
				WorldCoordinate worldCoordinate3 = scavenger.room.GetWorldCoordinate(new IntVector2(scavenger.occupyTile.x + intVector5.x, scavenger.occupyTile.y + intVector5.y));
				if (base.pathFinder.CoordinateViable(worldCoordinate3))
				{
					float num19 = IdleScore(worldCoordinate3);
					if (num19 > num8)
					{
						intVector2 = worldCoordinate3.Tile;
						num8 = num19;
					}
				}
			}
			if (intVector2.HasValue && (scavenger.abstractCreature.abstractAI as ScavengerAbstractAI).controlledMigrateTime <= 0)
			{
				base.creature.abstractAI.SetDestination(scavenger.room.GetWorldCoordinate(intVector2.Value));
				controlStuckTime = 0;
				controlWalkCooldown = 80;
				return;
			}
			runSpeedGoal = 0f;
			if (scavenger.inputWithDiagonals.Value.x != 0 || scavenger.inputWithDiagonals.Value.y < 0)
			{
				controlStuckTime++;
			}
			if (controlStuckTime <= 20)
			{
				return;
			}
			for (int num20 = 0; num20 < scavenger.bodyChunks.Length; num20++)
			{
				float y = Mathf.Min(0f, (float)scavenger.inputWithDiagonals.Value.y * 5f, scavenger.bodyChunks[num20].vel.y);
				if (scavenger.Submersion > 0f)
				{
					y = scavenger.bodyChunks[num20].vel.y;
				}
				scavenger.bodyChunks[num20].vel = new Vector2((float)scavenger.inputWithDiagonals.Value.x * 5f, y);
			}
		}
	}
}
