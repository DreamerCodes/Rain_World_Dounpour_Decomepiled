using RWCustom;
using UnityEngine;

public class CreatureLooker
{
	public Tracker.CreatureRepresentation lookCreature;

	private int lookFocusDelay;

	public Tracker tracker;

	public Creature creature;

	public ILookingAtCreatures lookingAtCreatures;

	private float chanceToLookAtNothing;

	private int framesBetweenFocusChange;

	public CreatureLooker(ILookingAtCreatures lookingAtCreatures, Tracker tracker, Creature creature, float chanceToLookAtNothing, int framesBetweenFocusChange)
	{
		this.lookingAtCreatures = lookingAtCreatures;
		this.tracker = tracker;
		this.creature = creature;
		this.chanceToLookAtNothing = chanceToLookAtNothing;
		this.framesBetweenFocusChange = framesBetweenFocusChange;
	}

	public void Update()
	{
		if (lookFocusDelay > 0)
		{
			lookFocusDelay--;
		}
		if (lookingAtCreatures.ForcedLookCreature() != null)
		{
			lookCreature = lookingAtCreatures.ForcedLookCreature();
		}
		else if (lookFocusDelay < 1 && Random.value < 1f / (float)framesBetweenFocusChange)
		{
			lookFocusDelay = framesBetweenFocusChange;
			if (Random.value > chanceToLookAtNothing)
			{
				ReevaluateLookObject(null, 0f);
				return;
			}
			lookCreature = null;
			lookingAtCreatures.LookAtNothing();
		}
	}

	public void ReevaluateLookObject(Tracker.CreatureRepresentation favorThisCreature, float bonus)
	{
		lookCreature = null;
		float num = 0f;
		bool flag = Random.value < 0.5f && favorThisCreature == null;
		for (int i = 0; i < tracker.CreaturesCount; i++)
		{
			Tracker.CreatureRepresentation rep = tracker.GetRep(i);
			if (rep.BestGuessForPosition().room != creature.abstractCreature.pos.room)
			{
				continue;
			}
			float num2 = 1f + Mathf.Lerp(rep.representedCreature.creatureTemplate.bodySize, 1f, 0.5f) * creature.abstractCreature.creatureTemplate.CreatureRelationship(rep.representedCreature.creatureTemplate).intensity * rep.EstimatedChanceOfFinding * Mathf.Lerp(rep.representedCreature.creatureTemplate.scaryness, 1f, 0.7f);
			if (flag)
			{
				num2 += Random.value;
			}
			if (rep.VisualContact && rep.representedCreature.realizedCreature != null)
			{
				num2 *= 1f + rep.representedCreature.realizedCreature.mainBodyChunk.vel.magnitude;
			}
			num2 /= Mathf.Lerp(Vector2.Distance(IntVector2.ToVector2(rep.BestGuessForPosition().Tile), IntVector2.ToVector2(creature.abstractCreature.pos.Tile)), 1f, 0.6f);
			if (rep == favorThisCreature)
			{
				num2 *= bonus;
			}
			if (creature.abstractCreature.creatureTemplate.CreatureRelationship(rep.representedCreature.creatureTemplate).type == CreatureTemplate.Relationship.Type.Ignores)
			{
				num2 /= 2f;
			}
			num2 = lookingAtCreatures.CreatureInterestBonus(rep, num2);
			if (creature.grasps != null)
			{
				for (int j = 0; j < creature.grasps.Length; j++)
				{
					if (creature.grasps[j] != null && creature.grasps[j].grabbed == rep.representedCreature.realizedCreature)
					{
						num2 = float.MinValue;
						break;
					}
				}
			}
			if (num2 > num)
			{
				num = num2;
				lookCreature = rep;
			}
		}
	}

	public void LookAtNothing()
	{
		lookFocusDelay = framesBetweenFocusChange;
		lookCreature = null;
		lookingAtCreatures.LookAtNothing();
	}
}
