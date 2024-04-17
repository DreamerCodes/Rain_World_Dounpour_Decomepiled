using RWCustom;
using UnityEngine;

public class TubeWormAI : ArtificialIntelligence
{
	public DebugDestinationVisualizer destVis;

	public int destCounter;

	public TubeWorm worm => creature.realizedCreature as TubeWorm;

	public bool SleepAllowed => !worm.safariControlled;

	public TubeWormAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		AddModule(new StandardPather(this, world, creature));
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		destCounter = 0;
	}

	public override void Update()
	{
		base.Update();
		if (!worm.safariControlled)
		{
			destCounter--;
			if (destCounter < 1 || Custom.ManhattanDistance(creature.pos, base.pathFinder.GetDestination) < 6)
			{
				RandomDest();
			}
		}
	}

	private void RandomDest()
	{
		WorldCoordinate worldCoordinate = new WorldCoordinate(worm.room.abstractRoom.index, Random.Range(0, worm.room.TileWidth), Random.Range(0, worm.room.TileHeight), -1);
		if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
		{
			creature.abstractAI.SetDestination(worldCoordinate);
			destCounter = Random.Range(400, 1200);
		}
	}
}
