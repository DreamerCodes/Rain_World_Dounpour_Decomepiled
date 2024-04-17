using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class StowawayBugAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior Hidden = new Behavior("Hidden", register: true);

		public static readonly Behavior Attacking = new Behavior("Attacking", register: true);

		public static readonly Behavior Digesting = new Behavior("Digesting", register: true);

		public static readonly Behavior Sleeping = new Behavior("Sleeping", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public StowawayBug myBug;

	public float currentUtility;

	public Behavior behavior;

	public Tracker.CreatureRepresentation focusCreature;

	public bool activeThisCycle;

	public StowawayBugAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		activeThisCycle = true;
		myBug = creature.realizedCreature as StowawayBug;
		myBug.AI = this;
		AddModule(new Tracker(this, 10, 3, 450, 0.5f, 5, 5, 20));
		AddModule(new PreyTracker(this, 1, 8f, 0f, 2000f, 0.95f));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new UtilityComparer(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 0.5f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 0.8f, 0.4f);
		behavior = Behavior.Hidden;
		if (myBug.abstractCreature.Room.world.game.IsStorySession && !(myBug.abstractCreature.state as StowawayBugState).AwakeThisCycle(myBug.abstractCreature.Room.world.game.GetStorySession.saveState.cycleNumber))
		{
			activeThisCycle = false;
			behavior = Behavior.Sleeping;
		}
		if (myBug.placedDirection.y > -0.3f || myBug.abstractCreature.world.game.IsArenaSession)
		{
			Custom.Log("forced on stowaway");
			activeThisCycle = true;
			behavior = Behavior.Idle;
		}
	}

	public override void Update()
	{
		focusCreature = null;
		base.Update();
		if (myBug.LickedByPlayer != null && Random.value < 0.01f)
		{
			myBug.AI.activeThisCycle = true;
			myBug.anyTentaclePulled = true;
			behavior = Behavior.Idle;
			base.tracker.SeeCreature(myBug.LickedByPlayer.abstractCreature);
		}
		if (behavior == Behavior.Sleeping)
		{
			if (myBug.anyTentaclePulled && Random.value < 0.001f && !(myBug.State as StowawayBugState).CurrentlyDigesting(myBug.room.world.rainCycle.timer))
			{
				behavior = Behavior.Idle;
			}
			if (activeThisCycle && !(myBug.State as StowawayBugState).CurrentlyDigesting(myBug.room.world.rainCycle.timer))
			{
				behavior = Behavior.Idle;
			}
			return;
		}
		if (behavior != Behavior.Hidden && behavior != Behavior.EscapeRain && base.preyTracker.MostAttractivePrey != null)
		{
			focusCreature = base.preyTracker.MostAttractivePrey;
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			if (aIModule is ThreatTracker)
			{
				behavior = Behavior.Hidden;
			}
			else if (aIModule is RainTracker && base.preyTracker.MostAttractivePrey == null)
			{
				behavior = Behavior.EscapeRain;
			}
			else if (myBug.huntDelay > 0)
			{
				behavior = Behavior.Idle;
			}
			else if (aIModule is PreyTracker)
			{
				if (base.preyTracker.MostAttractivePrey == null)
				{
					behavior = Behavior.Idle;
				}
				else if (base.preyTracker.MostAttractivePrey.representedCreature.state.alive || WantToEat(base.preyTracker.MostAttractivePrey.representedCreature.creatureTemplate.type))
				{
					behavior = Behavior.Attacking;
				}
				else
				{
					behavior = Behavior.Idle;
				}
			}
		}
		if ((double)currentUtility < 0.1)
		{
			behavior = Behavior.Idle;
		}
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return 0f;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
		Custom.Log("Creature spotted!");
		if (creatureRep != null && myBug.graphicsModule != null)
		{
			(myBug.graphicsModule as StowawayBugGraphics).creatureLooker.ReevaluateLookObject(creatureRep, 2f);
		}
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
		CreatureTemplate.Relationship.Type type = relationship.type;
		if (type != CreatureTemplate.Relationship.Type.Eats)
		{
			if (type == CreatureTemplate.Relationship.Type.Afraid)
			{
				return base.threatTracker;
			}
			if (type != CreatureTemplate.Relationship.Type.Antagonizes)
			{
				return null;
			}
		}
		return base.preyTracker;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return new RelationshipTracker.TrackedCreatureState();
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		return StaticRelationship(dRelation.trackerRep.representedCreature);
	}

	public bool WantToEat(CreatureTemplate.Type input)
	{
		if (input != CreatureTemplate.Type.Fly && input != CreatureTemplate.Type.Deer && input != CreatureTemplate.Type.BrotherLongLegs && input != CreatureTemplate.Type.DaddyLongLegs && input != MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs && input != CreatureTemplate.Type.BigEel && input != MoreSlugcatsEnums.CreatureTemplateType.BigJelly && input != CreatureTemplate.Type.GarbageWorm && input != CreatureTemplate.Type.Vulture && input != CreatureTemplate.Type.KingVulture && input != MoreSlugcatsEnums.CreatureTemplateType.MirosVulture && input != CreatureTemplate.Type.MirosBird && input != CreatureTemplate.Type.PoleMimic && input != CreatureTemplate.Type.TentaclePlant && input != CreatureTemplate.Type.Overseer && input != CreatureTemplate.Type.TempleGuard)
		{
			return input != MoreSlugcatsEnums.CreatureTemplateType.StowawayBug;
		}
		return false;
	}
}
