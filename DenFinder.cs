using System.Collections.Generic;
using RWCustom;

public class DenFinder : NodeFinder
{
	private WorldCoordinate? privDenPos;

	private WorldCoordinate? denPosition
	{
		get
		{
			if (creature.abstractAI != null)
			{
				return creature.abstractAI.denPosition;
			}
			return privDenPos;
		}
		set
		{
			if (creature.abstractAI != null)
			{
				creature.abstractAI.denPosition = value;
			}
			else
			{
				privDenPos = value;
			}
		}
	}

	public WorldCoordinate? GetDenPosition()
	{
		return denPosition;
	}

	public DenFinder(ArtificialIntelligence AI, AbstractCreature creature)
		: base(AI, creature)
	{
		if (denPosition.HasValue)
		{
			status = Status.Found;
		}
		else
		{
			ResetMapping(strandedAllFromExits: false);
		}
	}

	public override void Update()
	{
		base.Update();
		if (AI.pathFinder != null && AI.pathFinder.DoneMappingAccessibility)
		{
			CheckOriginalDenReachability();
		}
	}

	private void CheckOriginalDenReachability()
	{
		WorldCoordinate? worldCoordinate = null;
		if (!creature.world.singleRoomWorld && creature.world.GetSpawner(creature.ID) != null)
		{
			worldCoordinate = creature.world.GetSpawner(creature.ID).den;
			if (!denPosition.HasValue || denPosition.Value != worldCoordinate.Value)
			{
				TryAssigningDen(worldCoordinate.Value);
			}
		}
		if ((!worldCoordinate.HasValue || !denPosition.HasValue || denPosition.Value != worldCoordinate.Value) && (!denPosition.HasValue || denPosition.Value != creature.spawnDen))
		{
			TryAssigningDen(creature.spawnDen);
		}
	}

	private void TryAssigningDen(WorldCoordinate tryDen)
	{
		if (tryDen.NodeDefined && creature.world.GetAbstractRoom(tryDen) != null && creature.world.GetNode(tryDen).type.Index >= 0 && creature.creatureTemplate.mappedNodeTypes[creature.world.GetNode(tryDen).type.Index] && AI.pathFinder != null && AI.pathFinder.CoordinateReachable(tryDen))
		{
			Custom.Log(creature.creatureTemplate.name, "found its original spawn den, resetting den position!");
			Custom.Log("From:", denPosition.HasValue ? denPosition.Value.ToString() : "null", "To:", tryDen.ToString());
			status = Status.Found;
			abstractSpaceNodeFinder = null;
			denPosition = tryDen;
		}
	}

	public override void ResetMapping(bool strandedAllFromExits)
	{
		base.ResetMapping(strandedAllFromExits);
		Custom.Log($"{creature.creatureTemplate.name} {creature.ID} searching for new den using coordinate r:{creature.pos.room} n:{creature.pos.abstractNode}");
		if (strandedAllFromExits)
		{
			denPosition = null;
			Custom.LogWarning(creature.creatureTemplate.name, "stranded From All Exits, can't find den");
		}
	}

	public override void Initiate()
	{
		abstractSpaceNodeFinder = new AbstractSpaceNodeFinder(AbstractSpaceNodeFinder.SearchingFor.Den, AbstractSpaceNodeFinder.FloodMethod.Cost, 1000, creature.pos, creature.creatureTemplate, creature.Room.realizedRoom.game.world, 0f);
		status = Status.Working;
	}

	public override void Finished(List<WorldCoordinate> path)
	{
		if (path != null)
		{
			Custom.Log($"{creature.creatureTemplate.name} {creature.ID} found a den!");
			Custom.Log($"den {path[0]}");
			denPosition = path[0].WashTileData();
			status = Status.Found;
		}
		else
		{
			denPosition = null;
			status = Status.NoAccessible;
			Custom.LogWarning($"{creature.creatureTemplate.name} {creature.ID} found no den, standed!");
		}
	}

	public override void NewWorld()
	{
		denPosition = null;
		base.NewWorld();
	}
}
