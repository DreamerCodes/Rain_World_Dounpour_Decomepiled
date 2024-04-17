using RWCustom;
using UnityEngine;

public class SuperHearing : AIModule
{
	public Room room;

	public Tracker tracker;

	public float superHearingSkill;

	public SuperHearing(ArtificialIntelligence AI, Tracker tracker, float superHearingSkill)
		: base(AI)
	{
		this.superHearingSkill = superHearingSkill;
		this.tracker = tracker;
	}

	public override void NewRoom(Room room)
	{
		if (this.room != room)
		{
			this.room = room;
		}
	}

	public override void Update()
	{
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				if (!(room.physicalObjects[i][j] is Creature) || room.physicalObjects[i][j] == AI.creature.realizedCreature)
				{
					continue;
				}
				for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
				{
					BodyChunk bodyChunk = room.physicalObjects[i][j].bodyChunks[k];
					if ((bodyChunk.ContactPoint.x != 0 || bodyChunk.ContactPoint.y != 0) && Custom.DistLess(bodyChunk.pos, AI.creature.realizedCreature.mainBodyChunk.pos, Mathf.Min((Vector2.Distance(bodyChunk.lastPos, bodyChunk.pos) - 3f) * bodyChunk.loudness * 4f, 10f) * 0.1f * superHearingSkill))
					{
						tracker.SeeCreature((room.physicalObjects[i][j] as Creature).abstractCreature);
					}
				}
			}
		}
	}
}
