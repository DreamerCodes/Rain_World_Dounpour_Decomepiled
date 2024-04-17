using UnityEngine;

public class TentaclePlantAI : ArtificialIntelligence, IUseARelationshipTracker, IUseItemTracker
{
	public float itemInterest;

	public float preyInterest;

	public PhysicalObject mostInterestingItem;

	public TentaclePlantAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		AddModule(new Tracker(this, 4, 1, 100, 0.5f, 1, 1, 3));
		AddModule(new PreyTracker(this, 1, 1.2f, 0f, 400f, 0.75f));
		AddModule(new ThreatTracker(this, 1));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new ItemTracker(this, 40, 2, 4000, -1, stopTrackingCarried: true));
	}

	public override void Update()
	{
		base.Update();
		preyInterest = Mathf.Max(0f, preyInterest - 0.005f);
		if (base.preyTracker.MostAttractivePrey != null && base.preyTracker.MostAttractivePrey.VisualContact && base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null)
		{
			preyInterest = Mathf.Max(preyInterest, Mathf.InverseLerp(0f, 4f, Vector2.Distance(base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.lastPos, base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos)));
		}
		if (base.tracker.CreaturesCount > 0 && base.tracker.GetRep(0).TicksSinceSeen > 30)
		{
			base.tracker.ForgetCreature(base.tracker.GetRep(0).representedCreature);
		}
		itemInterest = Mathf.Max(0f, itemInterest - 1f / ((mostInterestingItem != null && mostInterestingItem.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.JellyFish) ? 400f : 100f));
		if (itemInterest == 0f)
		{
			mostInterestingItem = null;
		}
		if (base.itemTracker.ItemCount <= 0)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < base.itemTracker.ItemCount; i++)
		{
			if (base.itemTracker.GetRep(i).representedItem.realizedObject != null)
			{
				float num2 = Vector2.Distance(base.itemTracker.GetRep(i).representedItem.realizedObject.firstChunk.lastPos, base.itemTracker.GetRep(i).representedItem.realizedObject.firstChunk.pos);
				if (base.itemTracker.GetRep(i).representedItem.type == AbstractPhysicalObject.AbstractObjectType.JellyFish)
				{
					num2 *= 2f;
				}
				if (num2 > num)
				{
					num = num2;
					mostInterestingItem = base.itemTracker.GetRep(i).representedItem.realizedObject;
				}
			}
		}
		itemInterest = Mathf.Max(itemInterest, Mathf.InverseLerp(1f, 8f, num));
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (creature.creatureTemplate.CreatureRelationship(otherCreature.creatureTemplate).type != CreatureTemplate.Relationship.Type.Eats && creature.creatureTemplate.CreatureRelationship(otherCreature.creatureTemplate).type != CreatureTemplate.Relationship.Type.Afraid)
		{
			return null;
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, otherCreature.realizedCreature.TotalMass, 10);
	}

	public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Eats)
		{
			return base.preyTracker;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		return null;
	}

	public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (creature.creatureTemplate.CreatureRelationship(dRelation.trackerRep.representedCreature.creatureTemplate).type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return StaticRelationship(dRelation.trackerRep.representedCreature);
		}
		if (creature.creatureTemplate.CreatureRelationship(dRelation.trackerRep.representedCreature.creatureTemplate).type != CreatureTemplate.Relationship.Type.Eats)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.DoesntTrack, 0f);
		}
		if (dRelation.trackerRep.representedCreature.realizedCreature != null)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Mathf.InverseLerp(0f, 10000f, dRelation.trackerRep.representedCreature.realizedCreature.TotalMass));
		}
		return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0f);
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	public bool TrackItem(AbstractPhysicalObject obj)
	{
		return true;
	}

	public void SeeThrownWeapon(PhysicalObject obj, Creature thrower)
	{
	}
}
