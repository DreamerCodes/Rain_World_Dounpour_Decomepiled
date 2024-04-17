using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class PreyTracker : AIModule
{
	public class TrackedPrey
	{
		public PreyTracker owner;

		public Tracker.CreatureRepresentation critRep;

		public int unreachableCounter;

		public int atPositionButCantSeeCounter;

		public WorldCoordinate lastBestGuessPos;

		public float Reachable
		{
			get
			{
				if (critRep.BestGuessForPosition().room == owner.AI.creature.pos.room && owner.AI.creature.Room.realizedRoom != null && owner.AI.creature.Room.realizedRoom.GetTile(critRep.BestGuessForPosition()).Solid)
				{
					return 0f;
				}
				float num = 0f;
				num = ((owner.giveUpOnUnreachablePrey >= 0) ? (Mathf.InverseLerp(owner.giveUpOnUnreachablePrey, 0f, unreachableCounter) * Mathf.InverseLerp(200f, 100f, atPositionButCantSeeCounter)) : 1f);
				if (owner.AI.creature.world.GetAbstractRoom(critRep.BestGuessForPosition()).AttractionValueForCreature(owner.AI.creature.creatureTemplate.type) < owner.AI.creature.world.GetAbstractRoom(owner.AI.creature.pos).AttractionValueForCreature(owner.AI.creature.creatureTemplate.type))
				{
					num *= 0.5f;
				}
				return num;
			}
		}

		public TrackedPrey(PreyTracker owner, Tracker.CreatureRepresentation critRep)
		{
			this.owner = owner;
			this.critRep = critRep;
		}

		public void Update()
		{
			if (owner.AI.pathFinder != null && owner.AI.pathFinder.DoneMappingAccessibility)
			{
				WorldCoordinate worldCoordinate = critRep.BestGuessForPosition();
				bool flag = owner.AI.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate);
				if (!flag)
				{
					for (int i = 0; i < 4; i++)
					{
						if (flag)
						{
							break;
						}
						flag = owner.AI.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate + Custom.fourDirections[i]);
					}
				}
				if (flag)
				{
					unreachableCounter = 0;
				}
				else
				{
					unreachableCounter++;
				}
			}
			if (lastBestGuessPos == critRep.BestGuessForPosition() && owner.AI.creature.pos.room == critRep.BestGuessForPosition().room && owner.AI.creature.pos.Tile.FloatDist(critRep.BestGuessForPosition().Tile) < 5f && owner.AI.pathFinder != null && owner.AI.pathFinder.GetDestination.room == critRep.BestGuessForPosition().room && owner.AI.pathFinder.GetDestination.Tile.FloatDist(critRep.BestGuessForPosition().Tile) < 5f && owner.AI.creature.Room.realizedRoom != null && owner.AI.creature.Room.realizedRoom.VisualContact(owner.AI.creature.pos, critRep.BestGuessForPosition()))
			{
				atPositionButCantSeeCounter += 5;
			}
			else
			{
				atPositionButCantSeeCounter--;
			}
			atPositionButCantSeeCounter = Custom.IntClamp(atPositionButCantSeeCounter, 0, 200);
			lastBestGuessPos = critRep.BestGuessForPosition();
		}

		public bool PathFinderCanGetToPrey()
		{
			WorldCoordinate wc = critRep.BestGuessForPosition();
			for (int i = 0; i < 9; i++)
			{
				if (owner.AI.pathFinder.CoordinateReachable(WorldCoordinate.AddIntVector(wc, Custom.eightDirectionsAndZero[i])) && owner.AI.pathFinder.CoordinatePossibleToGetBackFrom(WorldCoordinate.AddIntVector(wc, Custom.eightDirectionsAndZero[i])))
				{
					return true;
				}
			}
			for (int j = 0; j < 4; j++)
			{
				if (owner.AI.pathFinder.CoordinateReachable(WorldCoordinate.AddIntVector(wc, Custom.fourDirections[j] * 2)) && owner.AI.pathFinder.CoordinatePossibleToGetBackFrom(WorldCoordinate.AddIntVector(wc, Custom.fourDirections[j] * 2)))
				{
					return true;
				}
			}
			return false;
		}

		public float Attractiveness()
		{
			float num = owner.AI.DynamicRelationship(critRep).intensity;
			WorldCoordinate worldCoordinate = critRep.BestGuessForPosition();
			float f = owner.DistanceEstimation(owner.AI.creature.pos, worldCoordinate, critRep.representedCreature.creatureTemplate);
			f = Mathf.Pow(f, 1.5f);
			f = Mathf.Lerp(f, 1f, 0.5f);
			if (owner.AI.pathFinder != null)
			{
				if (!owner.AI.pathFinder.CoordinateReachable(worldCoordinate))
				{
					num /= 2f;
				}
				if (!owner.AI.pathFinder.CoordinatePossibleToGetBackFrom(worldCoordinate))
				{
					num /= 2f;
				}
				if (!PathFinderCanGetToPrey())
				{
					num /= 2f;
				}
			}
			num *= critRep.EstimatedChanceOfFinding;
			num *= Reachable;
			if (ModManager.MMF && critRep.representedCreature.realizedCreature != null && critRep.representedCreature.realizedCreature.grabbedBy.Count > 0)
			{
				num = ((owner.AI.creature.creatureTemplate.TopAncestor() != critRep.representedCreature.realizedCreature.grabbedBy[0].grabber.abstractCreature.creatureTemplate.TopAncestor()) ? (num * owner.AI.creature.creatureTemplate.interestInOtherCreaturesCatches) : (num * owner.AI.creature.creatureTemplate.interestInOtherAncestorsCatches));
			}
			if (worldCoordinate.room != owner.AI.creature.pos.room)
			{
				num *= Mathf.InverseLerp(0f, 0.5f, owner.AI.creature.world.GetAbstractRoom(worldCoordinate).AttractionValueForCreature(owner.AI.creature.creatureTemplate.type));
			}
			num /= f;
			num *= Mathf.InverseLerp(owner.giveUpOnGhostGeneration, owner.giveUpOnGhostGeneration / 2, critRep.LowestGenerationAvailable);
			if (ModManager.MSC && owner.AI.creature.Room.world.game.IsStorySession && owner.AI.creature.Room.world.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && critRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger && critRep.representedCreature.realizedCreature != null && critRep.representedCreature.realizedCreature.grabbedBy.Count > 0 && critRep.representedCreature.realizedCreature.grabbedBy[0] != null && critRep.representedCreature.realizedCreature.grabbedBy[0].grabber is Player)
			{
				num /= 10f;
			}
			return num;
		}
	}

	private int maxRememberedCreatures;

	private float persistanceBias;

	private List<TrackedPrey> prey;

	private TrackedPrey currentPrey;

	private float sureToGetPreyDistance;

	private float sureToLosePreyDistance;

	public float frustration;

	private float frustrationSpeed;

	public int giveUpOnUnreachablePrey = 400;

	public int giveUpOnGhostGeneration = 50;

	public AImap aimap
	{
		get
		{
			if (AI.creature.realizedCreature.room == null)
			{
				return null;
			}
			return AI.creature.realizedCreature.room.aimap;
		}
	}

	public int TotalTrackedPrey => prey.Count;

	public Tracker.CreatureRepresentation MostAttractivePrey
	{
		get
		{
			if (currentPrey != null)
			{
				return currentPrey.critRep;
			}
			return null;
		}
	}

	public Tracker.CreatureRepresentation GetTrackedPrey(int index)
	{
		return prey[index].critRep;
	}

	public override float Utility()
	{
		if (currentPrey == null)
		{
			return 0f;
		}
		if (AI.creature.abstractAI.WantToMigrate && currentPrey.critRep.BestGuessForPosition().room != AI.creature.abstractAI.MigrationDestination.room && currentPrey.critRep.BestGuessForPosition().room != AI.creature.pos.room)
		{
			return 0f;
		}
		float num = DistanceEstimation(AI.creature.pos, currentPrey.critRep.BestGuessForPosition(), AI.creature.creatureTemplate);
		num = Mathf.Lerp(1f - Mathf.InverseLerp(sureToGetPreyDistance, sureToLosePreyDistance, num), Mathf.Lerp(sureToGetPreyDistance, sureToLosePreyDistance, 0.25f) / num, 0.5f);
		return Mathf.Min(num, currentPrey.critRep.EstimatedChanceOfFinding * Mathf.InverseLerp(giveUpOnGhostGeneration, giveUpOnGhostGeneration / 2, currentPrey.critRep.LowestGenerationAvailable)) * Mathf.Pow(AI.DynamicRelationship(currentPrey.critRep).intensity, 0.75f) * currentPrey.Reachable;
	}

	public PreyTracker(ArtificialIntelligence AI, int maxRememberedCreatures, float persistanceBias, float sureToGetPreyDistance, float sureToLosePreyDistance, float successEstimationDistReliance)
		: base(AI)
	{
		this.maxRememberedCreatures = maxRememberedCreatures;
		this.persistanceBias = persistanceBias;
		this.sureToGetPreyDistance = sureToGetPreyDistance;
		this.sureToLosePreyDistance = sureToLosePreyDistance;
		frustrationSpeed = 0.0125f;
		prey = new List<TrackedPrey>();
	}

	public void AddPrey(Tracker.CreatureRepresentation creature)
	{
		foreach (TrackedPrey item in prey)
		{
			if (item.critRep == creature)
			{
				return;
			}
		}
		prey.Add(new TrackedPrey(this, creature));
		if (prey.Count > maxRememberedCreatures)
		{
			float num = float.MaxValue;
			TrackedPrey trackedPrey = null;
			foreach (TrackedPrey item2 in prey)
			{
				if (item2.Attractiveness() < num)
				{
					num = item2.Attractiveness();
					trackedPrey = item2;
				}
			}
			if (AI.relationshipTracker != null)
			{
				AI.relationshipTracker.ModuleHasAbandonedCreature(trackedPrey.critRep, this);
			}
			else
			{
				trackedPrey.critRep.Destroy();
			}
			prey.Remove(trackedPrey);
		}
		Update();
	}

	public void ForgetPrey(AbstractCreature crit)
	{
		for (int num = prey.Count - 1; num >= 0; num--)
		{
			if (prey[num].critRep.representedCreature == crit)
			{
				prey.RemoveAt(num);
			}
		}
	}

	public void ForgetAllPrey()
	{
		prey.Clear();
		currentPrey = null;
	}

	public override void Update()
	{
		float num = float.MinValue;
		TrackedPrey trackedPrey = null;
		for (int num2 = prey.Count - 1; num2 >= 0; num2--)
		{
			prey[num2].Update();
			float num3 = prey[num2].Attractiveness();
			prey[num2].critRep.forgetCounter = 0;
			if (prey[num2] == currentPrey)
			{
				num3 *= persistanceBias;
			}
			if (prey[num2].critRep.deleteMeNextFrame)
			{
				prey.RemoveAt(num2);
			}
			else if (num3 > num)
			{
				num = num3;
				trackedPrey = prey[num2];
			}
		}
		currentPrey = trackedPrey;
		if (currentPrey != null && AI.pathFinder != null && AI.creature.pos.room == currentPrey.critRep.BestGuessForPosition().room && !currentPrey.PathFinderCanGetToPrey())
		{
			frustration = Mathf.Clamp(frustration + frustrationSpeed, 0f, 1f);
		}
		else
		{
			frustration = Mathf.Clamp(frustration - frustrationSpeed * 4f, 0f, 1f);
		}
	}

	public float DistanceEstimation(WorldCoordinate from, WorldCoordinate to, CreatureTemplate crit)
	{
		if (from.room != to.room)
		{
			if (AI.creature.world.GetAbstractRoom(from).realizedRoom != null && AI.creature.world.GetAbstractRoom(from).realizedRoom.readyForAI && AI.creature.world.GetAbstractRoom(from).ExitIndex(to.room) > -1)
			{
				int creatureSpecificExitIndex = AI.creature.world.GetAbstractRoom(from).CommonToCreatureSpecificNodeIndex(AI.creature.world.GetAbstractRoom(from).ExitIndex(to.room), crit);
				int num = AI.creature.world.GetAbstractRoom(from).realizedRoom.aimap.ExitDistanceForCreatureAndCheckNeighbours(from.Tile, creatureSpecificExitIndex, crit);
				if (crit.ConnectionResistance(MovementConnection.MovementType.SkyHighway).Allowed && num > -1 && AI.creature.world.GetAbstractRoom(from).AnySkyAccess && AI.creature.world.GetAbstractRoom(to).AnySkyAccess)
				{
					num = Math.Min(num, 50);
				}
				if (num > -1)
				{
					return num;
				}
			}
			return 50f;
		}
		return Vector2.Distance(IntVector2.ToVector2(from.Tile), IntVector2.ToVector2(to.Tile));
	}
}
