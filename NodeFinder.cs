using System.Collections.Generic;

public abstract class NodeFinder : AIModule
{
	public class Status : ExtEnum<Status>
	{
		public static readonly Status Working = new Status("Working", register: true);

		public static readonly Status Found = new Status("Found", register: true);

		public static readonly Status NoAccessible = new Status("NoAccessible", register: true);

		public Status(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	protected AbstractCreature creature;

	protected AbstractSpaceNodeFinder abstractSpaceNodeFinder;

	public Status status = Status.Working;

	public NodeFinder(ArtificialIntelligence AI, AbstractCreature creature)
		: base(AI)
	{
		this.creature = creature;
	}

	public override void Update()
	{
		if (abstractSpaceNodeFinder != null)
		{
			abstractSpaceNodeFinder.Update();
			if (abstractSpaceNodeFinder.finished)
			{
				List<WorldCoordinate> path = abstractSpaceNodeFinder.ReturnPathToClosest();
				abstractSpaceNodeFinder = null;
				Finished(path);
			}
		}
		else if (status == Status.Working)
		{
			Initiate();
		}
	}

	public virtual void ResetMapping(bool strandedAllFromExits)
	{
		if (!creature.pos.NodeDefined)
		{
			creature.pos = QuickConnectivity.DefineNodeOfLocalCoordinate(creature.pos, creature.world, creature.creatureTemplate);
		}
		if (!strandedAllFromExits)
		{
			status = Status.Working;
		}
		else
		{
			status = Status.NoAccessible;
		}
	}

	public virtual void NewWorld()
	{
		ResetMapping(strandedAllFromExits: false);
	}

	public virtual void ResetMappingIfNecessary(bool strandedAllFromExits)
	{
	}

	public virtual void Finished(List<WorldCoordinate> path)
	{
	}

	public virtual void Initiate()
	{
	}
}
