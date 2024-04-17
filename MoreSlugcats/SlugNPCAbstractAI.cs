using System.Collections.Generic;
using UnityEngine;

namespace MoreSlugcats;

public class SlugNPCAbstractAI : AbstractCreatureAI
{
	public WorldCoordinate? toldToStay;

	public bool isTamed;

	public SlugNPCAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
	}

	public override void AbstractBehavior(int time)
	{
		if (toldToStay.HasValue)
		{
			SetDestination(toldToStay.Value);
			return;
		}
		if (followCreature != null)
		{
			MoveWithCreature(followCreature, goToCreatureDestination: false);
		}
		if (path.Count > 0)
		{
			FollowPath(time);
		}
		else if (!parent.Room.swarmRoom && Random.value < 0.1f)
		{
			AbstractSpaceNodeFinder abstractSpaceNodeFinder = new AbstractSpaceNodeFinder(AbstractSpaceNodeFinder.SearchingFor.SwarmRoom, AbstractSpaceNodeFinder.FloodMethod.Random, 100, parent.pos, parent.creatureTemplate, world, 1f);
			while (!abstractSpaceNodeFinder.finished)
			{
				abstractSpaceNodeFinder.Update();
			}
			List<WorldCoordinate> list = abstractSpaceNodeFinder.ReturnPathToClosest();
			if (list != null)
			{
				path = list;
				SetDestination(path[0]);
			}
		}
		else
		{
			RandomMoveToOtherRoom(Random.Range(100, 500));
		}
	}
}
