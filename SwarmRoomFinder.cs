using System.Collections.Generic;
using RWCustom;

public class SwarmRoomFinder : NodeFinder
{
	private WorldCoordinate? privSwarmPos;

	public WorldCoordinate? SwarmPosition
	{
		get
		{
			if (creature.creatureTemplate.type == CreatureTemplate.Type.CicadaB && (creature.abstractAI as CicadaAbstractAI).swarmRoom.HasValue)
			{
				return (creature.abstractAI as CicadaAbstractAI).swarmRoom;
			}
			return privSwarmPos;
		}
		set
		{
			if (creature.creatureTemplate.type == CreatureTemplate.Type.CicadaB && (creature.abstractAI as CicadaAbstractAI).swarmRoom.HasValue)
			{
				(creature.abstractAI as CicadaAbstractAI).swarmRoom = value;
			}
			else
			{
				privSwarmPos = value;
			}
		}
	}

	public SwarmRoomFinder(ArtificialIntelligence AI, AbstractCreature creature)
		: base(AI, creature)
	{
		if (!SwarmPosition.HasValue)
		{
			ResetMapping(strandedAllFromExits: false);
		}
		else
		{
			status = Status.Found;
		}
	}

	public override void ResetMappingIfNecessary(bool strandedAllFromExits)
	{
		if (!SwarmPosition.HasValue)
		{
			ResetMapping(strandedAllFromExits);
		}
	}

	public override void ResetMapping(bool strandedAllFromExits)
	{
		if (creature.world.GetAbstractRoom(creature.pos.room).swarmRoom)
		{
			SwarmPosition = creature.pos.WashTileData();
			return;
		}
		base.ResetMapping(strandedAllFromExits);
		Custom.Log($"{creature.creatureTemplate.name} {creature.ID} searching for closeby Swarm Room using coordinate r:{creature.pos.room} n:{creature.pos.abstractNode}");
		if (strandedAllFromExits)
		{
			SwarmPosition = null;
			Custom.LogWarning(creature.creatureTemplate.name, "stranded From All Exits, can't find Swarm Room");
		}
	}

	public override void Initiate()
	{
		abstractSpaceNodeFinder = new AbstractSpaceNodeFinder(AbstractSpaceNodeFinder.SearchingFor.SwarmRoom, AbstractSpaceNodeFinder.FloodMethod.Random, 100, creature.pos, creature.creatureTemplate, creature.Room.realizedRoom.game.world, 1f);
		status = Status.Working;
	}

	public override void Finished(List<WorldCoordinate> path)
	{
		if (path != null)
		{
			Custom.Log($"{creature.creatureTemplate} {creature.ID} found a Swarm Room!");
			Custom.Log($"Swarm Room {path[0]}");
			SwarmPosition = path[0];
			status = Status.Found;
		}
		else
		{
			SwarmPosition = null;
			status = Status.NoAccessible;
			Custom.LogWarning($"{creature.creatureTemplate.name} {creature.ID} found no Swarm room!");
		}
	}

	public override void NewWorld()
	{
		SwarmPosition = null;
		base.NewWorld();
	}
}
