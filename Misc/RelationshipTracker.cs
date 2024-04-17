using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class RelationshipTracker : AIModule
{
	public class DynamicRelationship
	{
		public RelationshipTracker rt;

		public Tracker.CreatureRepresentation trackerRep;

		public CreatureTemplate.Relationship currentRelationship;

		public AIModule trackedByModule;

		public float trackedByModuleWeigth;

		public TrackedCreatureState state;

		public DynamicRelationship(RelationshipTracker rt, Tracker.CreatureRepresentation trackerRep, CreatureTemplate.Relationship initialRelationship)
		{
			this.rt = rt;
			this.trackerRep = trackerRep;
			_ = rt.visualize;
			currentRelationship = initialRelationship;
			rt.SortCreatureIntoModule(this, initialRelationship);
			state = (rt.AI as IUseARelationshipTracker).CreateTrackedCreatureState(this);
			trackerRep.dynamicRelationship = this;
			trackedByModuleWeigth = 1f;
		}

		public void Update()
		{
			CreatureTemplate.Relationship newRelationship = (rt.AI as IUseARelationshipTracker).UpdateDynamicRelationship(this);
			if (newRelationship.type != currentRelationship.type)
			{
				rt.SortCreatureIntoModule(this, newRelationship);
			}
			trackerRep.priority = newRelationship.intensity * trackedByModuleWeigth;
			currentRelationship = newRelationship;
		}
	}

	public class TrackedCreatureState
	{
		public bool alive;
	}

	public class RelationshipVisualizer
	{
		public class RelVis
		{
			public DynamicRelationship relationship;

			public FLabel txt;

			public FSprite line;

			public RelVis(DynamicRelationship relationship)
			{
				this.relationship = relationship;
				txt = new FLabel(Custom.GetFont(), "");
				Futile.stage.AddChild(txt);
				line = new FSprite("pixel");
				Futile.stage.AddChild(line);
				line.anchorY = 0f;
			}

			public void UpdateGraphics(Vector2 dispPos)
			{
				txt.x = dispPos.x;
				txt.y = dispPos.y;
				dispPos.y -= 8f;
				line.x = dispPos.x;
				line.y = dispPos.y;
				Vector2 vector = relationship.rt.AI.creature.realizedCreature.mainBodyChunk.pos - relationship.rt.AI.creature.realizedCreature.room.game.cameras[0].pos;
				line.scaleY = Vector2.Distance(dispPos, vector);
				line.rotation = Custom.AimFromOneVectorToAnother(dispPos, vector);
				txt.text = relationship.currentRelationship.type.ToString() + "  |  " + relationship.currentRelationship.intensity + "   | " + ((relationship.trackedByModule != null) ? ("Tracked By: " + relationship.trackedByModule.ToString()) : "~");
				txt.color = RelationshipColor(relationship.currentRelationship.type);
				line.color = RelationshipColor(relationship.currentRelationship.type);
			}

			public void ClearSprites()
			{
				txt.RemoveFromContainer();
				line.RemoveFromContainer();
			}
		}

		public RelationshipTracker rt;

		public List<RelVis> relVises;

		public FLabel trackerText;

		public FLabel preyText;

		public FLabel threatsText;

		public FLabel aggressionText;

		public static Color RelationshipColor(CreatureTemplate.Relationship.Type tp)
		{
			if (tp == CreatureTemplate.Relationship.Type.Afraid)
			{
				return new Color(1f, 0f, 0f);
			}
			if (tp == CreatureTemplate.Relationship.Type.Uncomfortable)
			{
				return new Color(1f, 0.5f, 0.5f);
			}
			if (tp == CreatureTemplate.Relationship.Type.Eats)
			{
				return new Color(0.1f, 0.9f, 0f);
			}
			if (tp == CreatureTemplate.Relationship.Type.Antagonizes)
			{
				return new Color(0f, 0f, 1f);
			}
			if (tp == CreatureTemplate.Relationship.Type.AgressiveRival)
			{
				return new Color(1f, 0f, 1f);
			}
			if (tp == CreatureTemplate.Relationship.Type.PlaysWith)
			{
				return new Color(1f, 1f, 0f);
			}
			if (tp == CreatureTemplate.Relationship.Type.StayOutOfWay)
			{
				return new Color(1f, 1f, 0.5f);
			}
			return new Color(1f, 1f, 1f);
		}

		public RelationshipVisualizer(RelationshipTracker rt)
		{
			this.rt = rt;
			relVises = new List<RelVis>();
			trackerText = new FLabel(Custom.GetFont(), "");
			preyText = new FLabel(Custom.GetFont(), "");
			threatsText = new FLabel(Custom.GetFont(), "");
			aggressionText = new FLabel(Custom.GetFont(), "");
			preyText.color = RelationshipColor(CreatureTemplate.Relationship.Type.Eats);
			threatsText.color = RelationshipColor(CreatureTemplate.Relationship.Type.Afraid);
			aggressionText.color = RelationshipColor(CreatureTemplate.Relationship.Type.AgressiveRival);
			Futile.stage.AddChild(trackerText);
			Futile.stage.AddChild(preyText);
			Futile.stage.AddChild(threatsText);
			Futile.stage.AddChild(aggressionText);
			for (int i = 0; i < rt.relationships.Count; i++)
			{
				NewRel(rt.relationships[i]);
			}
		}

		public void NewRel(DynamicRelationship rel)
		{
			relVises.Add(new RelVis(rel));
		}

		public void Update()
		{
			int num = 0;
			Vector2 vector = rt.AI.creature.realizedCreature.mainBodyChunk.pos - rt.AI.creature.realizedCreature.room.game.cameras[0].pos;
			trackerText.x = vector.x + 20f;
			trackerText.y = vector.y + 120f;
			preyText.x = vector.x + 20f;
			preyText.y = vector.y + 100f;
			threatsText.x = vector.x + 20f;
			threatsText.y = vector.y + 80f;
			aggressionText.x = vector.x + 20f;
			aggressionText.y = vector.y + 60f;
			trackerText.text = "Total creatures tracked: " + rt.tracker.CreaturesCount;
			preyText.text = ((rt.AI.preyTracker != null) ? ("Prey tracked: " + rt.AI.preyTracker.TotalTrackedPrey) : "No PreyTracker");
			threatsText.text = ((rt.AI.threatTracker != null) ? ("Threats tracked: " + rt.AI.threatTracker.TotalTrackedThreats + " (Creatures: " + rt.AI.threatTracker.TotalTrackedThreatCreatures + ")") : "No ThreatTracker");
			aggressionText.text = ((rt.AI.agressionTracker != null) ? ("Aggression Targets tracked: " + rt.AI.agressionTracker.TotalTrackedCreatures) : "No AgressionTracker");
			for (int i = 0; i < relVises.Count; i++)
			{
				Vector2 dispPos;
				if (relVises[i].relationship.trackerRep.representedCreature.realizedCreature != null && relVises[i].relationship.trackerRep.representedCreature.realizedCreature.room == rt.AI.creature.realizedCreature.room)
				{
					dispPos = relVises[i].relationship.trackerRep.representedCreature.realizedCreature.mainBodyChunk.pos + new Vector2(0f, 30f) - rt.AI.creature.realizedCreature.room.game.cameras[0].pos;
				}
				else
				{
					dispPos = vector + new Vector2(160f, 30f - 20f * (float)num);
					num++;
				}
				relVises[i].UpdateGraphics(dispPos);
			}
		}

		public void ClearAll()
		{
			for (int num = relVises.Count - 1; num >= 0; num--)
			{
				relVises[num].ClearSprites();
				relVises.RemoveAt(num);
			}
		}

		public void ClearSpecific(DynamicRelationship relationship)
		{
			for (int num = relVises.Count - 1; num >= 0; num--)
			{
				if (relVises[num].relationship == relationship)
				{
					relVises[num].ClearSprites();
					relVises.RemoveAt(num);
				}
			}
		}
	}

	private static readonly AGLog<RelationshipTracker> Log = new AGLog<RelationshipTracker>();

	public Tracker tracker;

	public bool visualize;

	private RelationshipVisualizer viz;

	public List<DynamicRelationship> relationships;

	private bool ignoreModuleAbandon;

	public RelationshipTracker(ArtificialIntelligence AI, Tracker tracker)
		: base(AI)
	{
		this.tracker = tracker;
		relationships = new List<DynamicRelationship>();
	}

	public override void Update()
	{
		if (visualize)
		{
			if (viz == null)
			{
				viz = new RelationshipVisualizer(this);
			}
			else
			{
				viz.Update();
			}
		}
		else if (viz != null)
		{
			viz.ClearAll();
			viz = null;
		}
		for (int num = relationships.Count - 1; num >= 0; num--)
		{
			if (relationships[num].trackerRep.deleteMeNextFrame)
			{
				if (visualize)
				{
					viz.ClearSpecific(relationships[num]);
				}
				relationships.RemoveAt(num);
			}
			else
			{
				relationships[num].Update();
			}
		}
	}

	public void EstablishDynamicRelationship(Tracker.CreatureRepresentation crit)
	{
		_ = visualize;
		CreatureTemplate.Relationship initialRelationship = AI.StaticRelationship(crit.representedCreature);
		if (initialRelationship.type == CreatureTemplate.Relationship.Type.DoesntTrack)
		{
			return;
		}
		for (int i = 0; i < relationships.Count; i++)
		{
			if (relationships[i].trackerRep == crit)
			{
				return;
			}
		}
		DynamicRelationship dynamicRelationship = new DynamicRelationship(this, crit, initialRelationship);
		relationships.Add(dynamicRelationship);
		if (viz != null)
		{
			viz.NewRel(dynamicRelationship);
		}
	}

	private void ForgetCreatureAndStopTracking(AbstractCreature creature)
	{
		for (int i = 0; i < relationships.Count; i++)
		{
			if (relationships[i].trackerRep.representedCreature == creature)
			{
				relationships[i].trackerRep.Destroy();
				if (visualize)
				{
					viz.ClearSpecific(relationships[i]);
				}
				relationships.RemoveAt(i);
			}
		}
	}

	public void ModuleHasAbandonedCreature(Tracker.CreatureRepresentation crit, AIModule module)
	{
		if (ignoreModuleAbandon)
		{
			return;
		}
		_ = visualize;
		for (int i = 0; i < relationships.Count; i++)
		{
			if (relationships[i].trackerRep == crit)
			{
				_ = visualize;
				ForgetCreatureAndStopTracking(crit.representedCreature);
				break;
			}
		}
	}

	public void SortCreatureIntoModule(DynamicRelationship relCrit, CreatureTemplate.Relationship newRelationship)
	{
		_ = visualize;
		_ = visualize;
		_ = visualize;
		AIModule aIModule = (AI as IUseARelationshipTracker).ModuleToTrackRelationship(newRelationship);
		if (relCrit.trackedByModule == aIModule)
		{
			return;
		}
		if (relCrit.trackedByModule != null)
		{
			ignoreModuleAbandon = true;
			if (relCrit.trackedByModule is PreyTracker)
			{
				(relCrit.trackedByModule as PreyTracker).ForgetPrey(relCrit.trackerRep.representedCreature);
				if (!visualize)
				{
				}
			}
			else if (relCrit.trackedByModule is ThreatTracker)
			{
				(relCrit.trackedByModule as ThreatTracker).RemoveThreatCreature(relCrit.trackerRep.representedCreature);
				if (!visualize)
				{
				}
			}
			else if (relCrit.trackedByModule is AgressionTracker)
			{
				(relCrit.trackedByModule as AgressionTracker).ForgetCreature(relCrit.trackerRep.representedCreature);
				_ = visualize;
			}
			ignoreModuleAbandon = false;
		}
		relCrit.trackedByModule = aIModule;
		if (newRelationship.type == CreatureTemplate.Relationship.Type.DoesntTrack)
		{
			ForgetCreatureAndStopTracking(relCrit.trackerRep.representedCreature);
			_ = visualize;
		}
		else if (relCrit.trackedByModule != null)
		{
			if (relCrit.trackedByModule is PreyTracker)
			{
				(relCrit.trackedByModule as PreyTracker).AddPrey(relCrit.trackerRep);
				_ = visualize;
			}
			else if (relCrit.trackedByModule is ThreatTracker)
			{
				(relCrit.trackedByModule as ThreatTracker).AddThreatCreature(relCrit.trackerRep);
				_ = visualize;
			}
			else if (relCrit.trackedByModule is AgressionTracker)
			{
				(relCrit.trackedByModule as AgressionTracker).AddCreature(relCrit.trackerRep, 0.1f, newRelationship.intensity);
				_ = visualize;
			}
		}
	}
}
