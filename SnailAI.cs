using RWCustom;
using UnityEngine;

public class SnailAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public Snail snail;

	public bool move;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public float scared;

	public int shuffleDestinationDelay;

	public bool CanDropIntoWater
	{
		get
		{
			if (scared > 0.1f)
			{
				return true;
			}
			if (base.rainTracker.Utility() > 0.2f)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsOccupyingShortcut => snail.room.GetTile(new IntVector2(creature.pos.x, creature.pos.y)).Terrain == Room.Tile.TerrainType.ShortcutEntrance;

	public SnailAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		snail = creature.realizedCreature as Snail;
		snail.AI = this;
		AddModule(new SnailPather(this, world, creature));
		AddModule(new Tracker(this, 10, 5, -1, 0.5f, 5, 5, 10));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new RelationshipTracker(this, base.tracker));
	}

	public override void NewRoom(Room room)
	{
		if (base.denFinder.GetDenPosition().HasValue && base.denFinder.GetDenPosition().Value.CompareDisregardingTile(base.pathFinder.GetDestination))
		{
			creature.abstractAI.SetDestination(new WorldCoordinate(room.abstractRoom.index, Random.Range(0, room.TileWidth), Random.Range(0, room.TileHeight), -1));
		}
		base.NewRoom(room);
	}

	public override void Update()
	{
		if (ModManager.MSC && snail.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(snail.LickedByPlayer.abstractCreature);
			snail.suckPoint = null;
		}
		if (debugDestinationVisualizer != null)
		{
			debugDestinationVisualizer.Update();
		}
		bool flag = Mathf.Lerp(0.01f, 0.9f, scared) < base.rainTracker.Utility() && base.denFinder.GetDenPosition().HasValue;
		if (flag)
		{
			move = true;
			creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
		}
		else if (base.pathFinder.GetDestination.room == creature.pos.room)
		{
			IntVector2 pos = new IntVector2(creature.pos.x + Random.Range(-10, 11), creature.pos.y + Random.Range(-10, 11));
			if (snail.room.GetTile(pos).Terrain == Room.Tile.TerrainType.Air && TileIdleScore(snail.room.GetWorldCoordinate(pos)) > TileIdleScore(base.pathFinder.GetDestination) && snail.room.VisualContact(snail.mainBodyChunk.pos, snail.room.MiddleOfTile(pos)))
			{
				creature.abstractAI.SetDestination(snail.room.GetWorldCoordinate(pos));
			}
		}
		if (!flag && snail.room.GetWorldCoordinate(snail.mainBodyChunk.pos).CompareDisregardingNode(base.pathFinder.GetDestination))
		{
			move = false;
		}
		else if (base.pathFinder.CoordinateAtCurrentPathingGeneration(creature.pos))
		{
			move = true;
		}
		float num = 0f;
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(i);
			if (creature.creatureTemplate.CreatureRelationship(rep.representedCreature.creatureTemplate).type == CreatureTemplate.Relationship.Type.Uncomfortable && Custom.DistLess(snail.mainBodyChunk.pos, snail.room.MiddleOfTile(rep.BestGuessForPosition()), 300f * CreatureUnease(rep.representedCreature)))
			{
				num += Mathf.InverseLerp(300f * CreatureUnease(rep.representedCreature), 100f * CreatureUnease(rep.representedCreature), Vector2.Distance(snail.mainBodyChunk.pos, snail.room.MiddleOfTile(rep.BestGuessForPosition()))) * Mathf.Lerp(CreatureUnease(rep.representedCreature), 1f, 0.5f) * rep.EstimatedChanceOfFinding;
			}
		}
		if (num != scared)
		{
			FloatTweener.FloatTween floatTween = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.1f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 1f / 120f));
			scared = Mathf.Clamp(floatTween.Tween(scared, Mathf.Min(num, 5f)), 0f, 1f);
		}
		if (scared > 0.2f && !flag)
		{
			move = false;
		}
		if (ModManager.MMF)
		{
			shuffleDestinationDelay--;
			if (IsOccupyingShortcut && shuffleDestinationDelay <= 0)
			{
				shuffleDestinationDelay = 200;
				creature.abstractAI.SetDestination(new WorldCoordinate(snail.room.abstractRoom.index, Random.Range(0, snail.room.TileWidth), Random.Range(0, snail.room.TileHeight), -1));
			}
			if (shuffleDestinationDelay > 0)
			{
				move = true;
			}
		}
		if (snail.safariControlled && snail.inputWithDiagonals.HasValue)
		{
			if (snail.inputWithDiagonals.Value.x != 0 || snail.inputWithDiagonals.Value.y != 0)
			{
				move = true;
			}
			else
			{
				move = false;
			}
		}
		base.Update();
	}

	public void CollideWithSnail()
	{
		if (move && Random.value < 0.1f)
		{
			creature.abstractAI.SetDestination(snail.room.GetWorldCoordinate(new IntVector2(Custom.IntClamp(creature.pos.x + Random.Range(-10, 11), 0, snail.room.TileWidth), Custom.IntClamp(creature.pos.y + Random.Range(-10, 11), 0, snail.room.TileHeight))));
		}
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed);
	}

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(i);
			if (Custom.ManhattanDistance(connection.startCoord, rep.BestGuessForPosition()) < 3)
			{
				if (Custom.ManhattanDistance(creature.pos, rep.BestGuessForPosition()) < 3)
				{
					cost.resistance += 100f;
				}
				else
				{
					cost.resistance += 10f + 90f * CreatureUnease(rep.representedCreature);
				}
			}
		}
		cost.resistance += Random.value;
		return cost;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	private float TileIdleScore(WorldCoordinate pos)
	{
		if (!pos.TileDefined || !base.pathFinder.CoordinateReachable(pos) || (ModManager.MMF && snail.room.GetTile(pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance))
		{
			return float.MinValue;
		}
		float num = 0f;
		if (!base.pathFinder.CoordinatePossibleToGetBackFrom(pos))
		{
			num -= 1000f;
		}
		if (snail.room.aimap.getAItile(pos).narrowSpace)
		{
			num -= 100f;
		}
		if (base.denFinder.GetDenPosition().HasValue && base.denFinder.GetDenPosition().Value.room == pos.room)
		{
			num += Mathf.Min(snail.room.aimap.ExitDistanceForCreature(pos.Tile, base.denFinder.GetDenPosition().Value.abstractNode, snail.Template), 15f) * 0.01f;
		}
		bool flag = false;
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(i);
			if (rep.BestGuessForPosition().room != creature.pos.room)
			{
				continue;
			}
			if (snail.Template.CreatureRelationship(rep.representedCreature.creatureTemplate).type != CreatureTemplate.Relationship.Type.Ignores)
			{
				num = ((!rep.VisualContact) ? (num + (float)Custom.ManhattanDistance(pos, rep.BestGuessForPosition())) : (num - 200f * snail.Template.CreatureRelationship(rep.representedCreature.creatureTemplate).intensity / (float)(Custom.ManhattanDistance(pos, rep.BestGuessForPosition()) + 1)));
				num *= CreatureUnease(rep.representedCreature);
				if (rep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Leech)
				{
					flag = true;
				}
			}
			else if (rep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Snail && rep.representedCreature != creature && Custom.ManhattanDistance(pos, rep.BestGuessForPosition()) < 1)
			{
				num -= 20f;
			}
		}
		if (flag)
		{
			num = ((!snail.room.GetTile(pos).AnyWater) ? (num + 100f) : (num - 100f));
		}
		return num * Mathf.Lerp(snail.room.aimap.Visibility(pos.Tile), 1f, 0.5f);
	}

	public float CreatureUnease(AbstractCreature crit)
	{
		if (crit.state.dead || snail.Template.CreatureRelationship(crit.creatureTemplate).type == CreatureTemplate.Relationship.Type.Ignores)
		{
			return 0f;
		}
		float num = 1f;
		if (crit.creatureTemplate.type == CreatureTemplate.Type.Leech)
		{
			if (crit.realizedCreature != null && crit.realizedCreature.mainBodyChunk.ContactPoint.y < 0)
			{
				return 0f;
			}
			num = 10f - scared * 7f;
		}
		num *= crit.creatureTemplate.bodySize;
		num *= Mathf.Lerp(snail.abstractCreature.creatureTemplate.CreatureRelationship(crit.creatureTemplate).intensity, 1f, 0.5f);
		if (crit.realizedCreature != null && !crit.realizedCreature.Consious)
		{
			num /= 20f;
		}
		if (snail.room.GetTile(snail.mainBodyChunk.pos).DeepWater && crit.creatureTemplate.waterRelationship == CreatureTemplate.WaterRelationship.AirOnly)
		{
			num /= 3f;
		}
		else if (!snail.room.GetTile(snail.mainBodyChunk.pos).AnyWater && crit.creatureTemplate.waterRelationship == CreatureTemplate.WaterRelationship.WaterOnly)
		{
			num /= 3f;
		}
		return num;
	}

	public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		return null;
	}

	public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		return creature.creatureTemplate.CreatureRelationship(dRelation.trackerRep.representedCreature.creatureTemplate);
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}
}
