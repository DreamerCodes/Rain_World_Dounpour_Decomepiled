using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class DiscomfortTracker : AIModule
{
	private Tracker tracker;

	private float uncomfortableWithUnknownNoises;

	public ItemTracker.ItemRepresentation uncomfortableItem;

	public float uncomfortableItemDiscomfort;

	public DiscomfortTracker(ArtificialIntelligence AI, Tracker tracker, float uncomfortableWithUnknownNoises)
		: base(AI)
	{
		this.uncomfortableWithUnknownNoises = uncomfortableWithUnknownNoises;
		this.tracker = tracker;
	}

	public override void Update()
	{
		base.Update();
		if (uncomfortableItem != null && uncomfortableItem.representedItem.realizedObject != null && uncomfortableItem.representedItem.realizedObject.grabbedBy.Count > 0 && uncomfortableItem.representedItem.realizedObject.grabbedBy[0].grabber.abstractCreature == AI.creature)
		{
			uncomfortableItem = null;
			uncomfortableItemDiscomfort = 0f;
		}
	}

	public Tracker.CreatureRepresentation MostUncomfortableCreature()
	{
		Tracker.CreatureRepresentation result = null;
		float num = 0f;
		for (int i = 0; i < tracker.CreaturesCount; i++)
		{
			if (tracker.GetRep(i).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Uncomfortable && tracker.GetRep(i).BestGuessForPosition().room == AI.creature.pos.room)
			{
				float num2 = Mathf.InverseLerp(1f + tracker.GetRep(i).dynamicRelationship.currentRelationship.intensity * 40f, 0f, tracker.GetRep(i).BestGuessForPosition().Tile.FloatDist(AI.creature.pos.Tile));
				num2 *= Mathf.InverseLerp(1000f, 50f, tracker.GetRep(i).TicksSinceSeen);
				if (num2 > num)
				{
					num = num2;
					result = tracker.GetRep(i);
				}
			}
		}
		return result;
	}

	public float DiscomfortOfTile(WorldCoordinate test)
	{
		if (test.room != AI.creature.pos.room || !test.TileDefined)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < tracker.CreaturesCount; i++)
		{
			if (tracker.GetRep(i).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Uncomfortable && tracker.GetRep(i).BestGuessForPosition().room == test.room)
			{
				float num2 = Mathf.InverseLerp(1f + tracker.GetRep(i).dynamicRelationship.currentRelationship.intensity * 40f, 0f, tracker.GetRep(i).BestGuessForPosition().Tile.FloatDist(test.Tile));
				num2 *= Mathf.InverseLerp(1000f, 50f, tracker.GetRep(i).TicksSinceSeen);
				num2 *= 0.5f + 0.5f * AI.creature.realizedCreature.room.aimap.AccessibilityForCreature(test.Tile, tracker.GetRep(i).representedCreature.creatureTemplate);
				num = Mathf.Lerp(num, Mathf.Max(num, Mathf.Pow(num2, 0.5f)), 0.5f + 0.5f * tracker.GetRep(i).dynamicRelationship.currentRelationship.intensity);
			}
		}
		if (uncomfortableWithUnknownNoises > 0f && tracker.noiseTracker.Utility() > 0f)
		{
			for (int j = 0; j < tracker.noiseTracker.sources.Count; j++)
			{
				if (tracker.noiseTracker.sources[j].creatureRep == null)
				{
					float num3 = Mathf.InverseLerp(600f, 0f, Vector2.Distance(tracker.noiseTracker.sources[j].pos, AI.creature.realizedCreature.room.MiddleOfTile(test)));
					num3 *= Mathf.InverseLerp(1000f, 50f, tracker.noiseTracker.sources[j].age);
					num = Mathf.Lerp(num, Mathf.Max(num, Mathf.Pow(num3, 0.5f)), (0.5f + 0.5f * tracker.noiseTracker.Utility()) * uncomfortableWithUnknownNoises);
				}
			}
		}
		if (uncomfortableItem != null)
		{
			num += Custom.LerpMap(Vector2.Distance(AI.creature.realizedCreature.room.MiddleOfTile(test), AI.creature.realizedCreature.room.MiddleOfTile(uncomfortableItem.BestGuessForPosition())), 40f, 200f, uncomfortableItemDiscomfort, 0f);
		}
		if (AI.threatTracker != null)
		{
			num = Mathf.Lerp(num, 1f, Mathf.Pow(Mathf.InverseLerp(0.1f, 1f, AI.threatTracker.ThreatOfTile(test, accountThreatCreatureAccessibility: true)), 0.25f));
		}
		if (ModManager.MSC && AI.creature.Room.realizedRoom != null && !AI.creature.creatureTemplate.BlizzardAdapted && AI.creature.Room.realizedRoom.blizzard && AI.creature.realizedCreature != null)
		{
			foreach (IProvideWarmth blizzardHeatSource in AI.creature.Room.realizedRoom.blizzardHeatSources)
			{
				float value = Vector2.Distance(AI.creature.realizedCreature.firstChunk.pos, blizzardHeatSource.Position());
				float value2 = blizzardHeatSource.warmth * Mathf.InverseLerp(blizzardHeatSource.range, blizzardHeatSource.range * 0.8f, value);
				num = Mathf.Lerp(num, 0.8f, Mathf.InverseLerp(1f, 0.45f, value2));
			}
		}
		return num;
	}

	public bool TileUncomfortable(WorldCoordinate test)
	{
		return DiscomfortOfTile(test) > 0f;
	}
}
