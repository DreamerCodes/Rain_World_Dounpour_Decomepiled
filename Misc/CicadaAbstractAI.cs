using System.Collections.Generic;
using UnityEngine;

public class CicadaAbstractAI : AbstractCreatureAI
{
	public WorldCoordinate? swarmRoom;

	private bool cantGetToSwarmRoom;

	public CicadaAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
	}

	public override void AbstractBehavior(int time)
	{
		if (parent.realizedCreature == null)
		{
			if (path.Count > 0)
			{
				FollowPath(time);
			}
			else if (HavePrey() || (parent.creatureTemplate.type == CreatureTemplate.Type.CicadaA && base.denPosition.HasValue && parent.pos.room != base.denPosition.Value.room))
			{
				GoToDen();
			}
			else if (Random.value < 0.1f && Random.value > 1f / (float)time)
			{
				int num = Random.Range(0, parent.Room.nodes.Length);
				if (parent.Room.NumberOfQuantifiedCreatureInNode(CreatureTemplate.Type.Fly, num) > 0 && Random.value < 1f / (float)parent.Room.NumberOfQuantifiedCreatureInNode(CreatureTemplate.Type.Fly, num) && (num == parent.pos.abstractNode || parent.Room.ConnectionAndBackPossible(parent.pos.abstractNode, num, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly))))
				{
					AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly), null, parent.pos, world.game.GetNewID());
					parent.Room.AddEntity(abstractCreature);
					parent.Room.RemoveQuantifiedCreature(num, CreatureTemplate.Type.Fly);
					new AbstractPhysicalObject.CreatureGripStick(parent, abstractCreature, 0, carry: true);
					GoToDen();
				}
			}
			else
			{
				if (cantGetToSwarmRoom || world.rainCycle.TimeUntilRain <= 4800 || !world.game.IsStorySession)
				{
					return;
				}
				if (swarmRoom.HasValue)
				{
					if (parent.pos.room != swarmRoom.Value.room)
					{
						SetDestination(swarmRoom.Value);
					}
					return;
				}
				AbstractSpaceNodeFinder abstractSpaceNodeFinder = new AbstractSpaceNodeFinder(AbstractSpaceNodeFinder.SearchingFor.SwarmRoom, AbstractSpaceNodeFinder.FloodMethod.Random, 100, parent.pos, parent.creatureTemplate, world, 1f);
				while (!abstractSpaceNodeFinder.finished)
				{
					abstractSpaceNodeFinder.Update();
				}
				List<WorldCoordinate> list = abstractSpaceNodeFinder.ReturnPathToClosest();
				if (list != null)
				{
					path = list;
					swarmRoom = path[0];
					SetDestination(swarmRoom.Value);
					cantGetToSwarmRoom = false;
				}
				else
				{
					cantGetToSwarmRoom = true;
				}
			}
		}
		else
		{
			cantGetToSwarmRoom = false;
		}
	}
}
