using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class AgressionTracker : AIModule
{
	public class AngerRelationship
	{
		public AgressionTracker tTracker;

		public Tracker.CreatureRepresentation crit;

		public float anger;

		public float baseAnger;

		public AngerRelationship(AgressionTracker tTracker, Tracker.CreatureRepresentation crit, float initialAnger, float baseAnger)
		{
			this.tTracker = tTracker;
			this.crit = crit;
			anger = initialAnger;
			this.baseAnger = baseAnger;
		}

		public void Update()
		{
			float num = baseAnger;
			num *= Mathf.InverseLerp(10f + baseAnger * 70f, 5f, Custom.ManhattanDistance(tTracker.AI.creature.pos, crit.BestGuessForPosition()));
			if (tTracker.AI.preyTracker != null && tTracker.AI.preyTracker.MostAttractivePrey != null)
			{
				float b = Mathf.InverseLerp(10f, 3f, Custom.ManhattanDistance(crit.BestGuessForPosition(), tTracker.AI.preyTracker.MostAttractivePrey.BestGuessForPosition()));
				num = Mathf.Lerp(num, b, 0.5f);
			}
			num *= Mathf.InverseLerp(0.3f, 0.6f, tTracker.AI.creature.world.GetAbstractRoom(crit.BestGuessForPosition().room).AttractionValueForCreature(tTracker.AI.creature.creatureTemplate.type));
			if (anger < num)
			{
				anger = Mathf.Clamp(anger + tTracker.angerSpeedUp, 0f, num);
			}
			else if (anger > num)
			{
				anger = Mathf.Clamp(anger - tTracker.angerSpeedDown, num, 1f);
			}
			crit.forgetCounter = 0;
		}
	}

	private List<AngerRelationship> creatures;

	public float highestAnger;

	public AngerRelationship highestAgressionTarget;

	public float angerSpeedUp;

	public float angerSpeedDown;

	public int TotalTrackedCreatures => creatures.Count;

	public AgressionTracker(ArtificialIntelligence AI, float angerSpeedUp, float angerSpeedDown)
		: base(AI)
	{
		creatures = new List<AngerRelationship>();
		this.angerSpeedUp = angerSpeedUp;
		this.angerSpeedDown = angerSpeedDown;
	}

	public override void Update()
	{
		highestAnger = 0f;
		highestAgressionTarget = null;
		for (int num = creatures.Count - 1; num >= 0; num--)
		{
			creatures[num].Update();
			if (creatures[num].crit.deleteMeNextFrame)
			{
				creatures.RemoveAt(num);
			}
			else if (creatures[num].anger > highestAnger)
			{
				highestAnger = creatures[num].anger;
				highestAgressionTarget = creatures[num];
			}
		}
	}

	public override float Utility()
	{
		return Mathf.InverseLerp(0.35f, 1f, highestAnger);
	}

	public Tracker.CreatureRepresentation AgressionTarget()
	{
		if (highestAgressionTarget == null)
		{
			return null;
		}
		return highestAgressionTarget.crit;
	}

	public void AddCreature(Tracker.CreatureRepresentation newCrit, float initialAnger, float baseAnger)
	{
		for (int i = 0; i < creatures.Count; i++)
		{
			if (creatures[i].crit == newCrit)
			{
				return;
			}
		}
		creatures.Add(new AngerRelationship(this, newCrit, initialAnger, baseAnger));
	}

	public void ForgetCreature(AbstractCreature removeCrit)
	{
		for (int i = 0; i < creatures.Count; i++)
		{
			if (creatures[i].crit.representedCreature == removeCrit)
			{
				if (AI.relationshipTracker != null)
				{
					AI.relationshipTracker.ModuleHasAbandonedCreature(creatures[i].crit, this);
				}
				creatures.RemoveAt(i);
				break;
			}
		}
	}

	public void IncrementAnger(Tracker.CreatureRepresentation crit, float angerAdd)
	{
		for (int i = 0; i < creatures.Count; i++)
		{
			if (creatures[i].crit == crit)
			{
				creatures[i].anger = Mathf.Clamp(creatures[i].anger + angerAdd, 0f, 1f);
				break;
			}
		}
	}

	public void SetAnger(Tracker.CreatureRepresentation crit, float newAnger, float newBaseAnger)
	{
		for (int i = 0; i < creatures.Count; i++)
		{
			if (creatures[i].crit == crit)
			{
				creatures[i].anger = Mathf.Clamp(newAnger, 0f, 1f);
				if (newBaseAnger != -1f)
				{
					creatures[i].baseAnger = Mathf.Clamp(newBaseAnger, 0f, 1f);
				}
				break;
			}
		}
	}
}
