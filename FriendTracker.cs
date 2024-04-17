using RWCustom;
using UnityEngine;

public class FriendTracker : AIModule
{
	public interface IHaveFriendTracker
	{
		void GiftRecieved(SocialEventRecognizer.OwnedItemOnGround gift);
	}

	private static readonly AGLog<FriendTracker> Log = new AGLog<FriendTracker>();

	public SocialEventRecognizer.OwnedItemOnGround giftOfferedToMe;

	public bool followClosestFriend = true;

	public Creature friend;

	public SocialMemory.Relationship friendRel;

	public WorldCoordinate friendDest;

	public WorldCoordinate tempFriendDest;

	public int friendMovingCounter;

	public WorldCoordinate lastFriendPos;

	public float tamingDifficlty = 1f;

	public float desiredCloseness;

	public Creature creature => AI.creature.realizedCreature;

	public float Urgency
	{
		get
		{
			if (friendRel == null)
			{
				return 0f;
			}
			return Mathf.InverseLerp(0.25f, 1f, friendRel.tempLike);
		}
	}

	public FriendTracker(ArtificialIntelligence AI)
		: base(AI)
	{
		friendDest = AI.creature.pos;
		tempFriendDest = AI.creature.pos;
		desiredCloseness = 10f;
	}

	public override float Utility()
	{
		if (friend == null)
		{
			return 0f;
		}
		if (friend.abstractCreature.pos.room != AI.creature.pos.room)
		{
			return Urgency;
		}
		if (friend.room != null && (friend.room.abstractRoom.gate || friend.room.abstractRoom.shelter))
		{
			return 1f;
		}
		return Custom.LerpMap(friend.abstractCreature.pos.Tile.FloatDist(AI.creature.pos.Tile), desiredCloseness, desiredCloseness * 3f, 0.2f, 1f) * Urgency;
	}

	public float RunSpeed()
	{
		if (AI.creature.pos.room == friendDest.room)
		{
			if (AI.creature.Room.shelter || AI.creature.Room.gate)
			{
				if (!(AI.creature.pos.Tile.FloatDist(friendDest.Tile) < 2f))
				{
					return 1f;
				}
				return 0f;
			}
			if (AI.creature.pos.Tile.FloatDist(friendDest.Tile) < 3f)
			{
				return 0f;
			}
		}
		return Custom.LerpMap(AI.creature.pos.Tile.FloatDist(friendDest.Tile), 3f, 25f, 0.25f, 1f, (friendMovingCounter > 0) ? 0.5f : 1f);
	}

	public bool CareAboutRain()
	{
		if (friend == null)
		{
			return true;
		}
		if (friend.abstractCreature.pos.room == AI.creature.pos.room)
		{
			return false;
		}
		for (int i = 0; i < friend.abstractCreature.Room.connections.Length; i++)
		{
			if (friend.abstractCreature.Room.connections[i] == AI.creature.pos.room)
			{
				return false;
			}
		}
		return true;
	}

	public override void Update()
	{
		base.Update();
		if (giftOfferedToMe != null)
		{
			if (!giftOfferedToMe.active)
			{
				giftOfferedToMe = null;
			}
			else if (creature != null && creature.room != null)
			{
				for (int i = 0; i < creature.grasps.Length; i++)
				{
					if (creature.grasps[i] != null && creature.grasps[i].grabbed == giftOfferedToMe.item)
					{
						GiftRecieved();
						break;
					}
				}
			}
		}
		if (!followClosestFriend)
		{
			return;
		}
		if (friend == null)
		{
			if (AI.creature.state.socialMemory == null || AI.creature.state.socialMemory.relationShips == null || AI.creature.state.socialMemory.relationShips.Count <= 0)
			{
				return;
			}
			for (int j = 0; j < AI.creature.state.socialMemory.relationShips.Count; j++)
			{
				if (!(AI.creature.state.socialMemory.relationShips[j].like > 0.5f) || !(AI.creature.state.socialMemory.relationShips[j].tempLike > 0.5f))
				{
					continue;
				}
				for (int k = 0; k < AI.creature.Room.creatures.Count; k++)
				{
					if (AI.creature.Room.creatures[k].ID == AI.creature.state.socialMemory.relationShips[j].subjectID && AI.creature.Room.creatures[k].realizedCreature != null)
					{
						friend = AI.creature.Room.creatures[k].realizedCreature;
						friendRel = AI.creature.state.socialMemory.relationShips[j];
						break;
					}
				}
			}
			return;
		}
		if (friend.abstractCreature.pos.room != lastFriendPos.room || friend.abstractCreature.pos.Tile.FloatDist(lastFriendPos.Tile) > desiredCloseness)
		{
			friendMovingCounter = 100;
			lastFriendPos = friend.abstractCreature.pos;
		}
		else if (friendMovingCounter > 0)
		{
			friendMovingCounter--;
		}
		if ((friendMovingCounter == 0 || friend.abstractCreature.pos.Tile.FloatDist(AI.creature.pos.Tile) < desiredCloseness) && !friend.abstractCreature.Room.shelter && !friend.abstractCreature.Room.gate)
		{
			if (friend.room != null)
			{
				WorldCoordinate testPos = ((!(Random.value < 0.5f)) ? friend.room.GetWorldCoordinate(friend.mainBodyChunk.pos + Custom.RNV() * 400f * Random.value) : creature.room.GetWorldCoordinate(creature.mainBodyChunk.pos + Custom.RNV() * 400f * Random.value));
				if (FriendDestScore(testPos) < FriendDestScore(tempFriendDest))
				{
					tempFriendDest = testPos;
				}
				if (FriendDestScore(tempFriendDest) < FriendDestScore(friendDest) - 2f)
				{
					friendDest = tempFriendDest;
				}
			}
		}
		else
		{
			WorldCoordinate coord = friend.abstractCreature.pos;
			if (!AI.pathFinder.CoordinateReachable(coord))
			{
				for (int l = 0; l < 8; l++)
				{
					if (AI.pathFinder.CoordinateReachable(friend.abstractCreature.pos + Custom.eightDirections[l]))
					{
						coord = friend.abstractCreature.pos + Custom.eightDirections[l];
						break;
					}
				}
			}
			if (AI.pathFinder.CoordinateReachable(coord))
			{
				friendDest = coord;
			}
		}
		if (Urgency == 0f)
		{
			friend = null;
			friendRel = null;
		}
	}

	public void GiftRecieved()
	{
		if (giftOfferedToMe != null && giftOfferedToMe.active)
		{
			if (AI is IHaveFriendTracker)
			{
				(AI as IHaveFriendTracker).GiftRecieved(giftOfferedToMe);
			}
			giftOfferedToMe = null;
		}
	}

	public float FriendDestScore(WorldCoordinate testPos)
	{
		if (friend == null || friend.room == null || !friend.room.readyForAI || testPos.room != friend.abstractCreature.pos.room || !friend.room.aimap.TileAccessibleToCreature(testPos.Tile, AI.creature.creatureTemplate) || !AI.pathFinder.CoordinateReachable(testPos))
		{
			return float.MaxValue;
		}
		float num = Mathf.Abs(5f - testPos.Tile.FloatDist(friend.abstractCreature.pos.Tile));
		if (testPos.Tile.FloatDist(friend.abstractCreature.pos.Tile) < desiredCloseness && friend.room.VisualContact(testPos, friend.abstractCreature.pos))
		{
			num -= 10f;
		}
		if (friend.room.aimap.getAItile(testPos).narrowSpace)
		{
			num += (friend.room.abstractRoom.shelter ? 100f : 2f);
		}
		if (AI.creature.pos.room == friend.abstractCreature.pos.room && testPos.x < friend.abstractCreature.pos.x == AI.creature.pos.x < friend.abstractCreature.pos.x)
		{
			num -= 6f;
		}
		return num;
	}

	public void ItemOffered(Tracker.CreatureRepresentation subjectRep, PhysicalObject involvedItem)
	{
		if (involvedItem is Creature && involvedItem.room != null && AI.DynamicRelationship((involvedItem as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
		{
			giftOfferedToMe = involvedItem.room.socialEventRecognizer.ItemOwnership(involvedItem);
		}
	}
}
