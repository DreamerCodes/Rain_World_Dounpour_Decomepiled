using UnityEngine;

namespace ArenaBehaviors;

public class Evilifier : ArenaGameBehavior
{
	private bool playersSpawned;

	public Evilifier(ArenaGameSession session)
		: base(session)
	{
		session.difficulty = 1f;
		for (int i = 0; i < session.creatureCommunities.playerOpinions.GetLength(0); i++)
		{
			for (int j = 0; j < session.creatureCommunities.playerOpinions.GetLength(1); j++)
			{
				for (int k = 0; k < session.creatureCommunities.playerOpinions.GetLength(2); k++)
				{
					session.creatureCommunities.playerOpinions[i, j, k] = -1f;
				}
			}
		}
	}

	public override void Update()
	{
		if (base.room.abstractRoom.creatures.Count < 1)
		{
			return;
		}
		if (!playersSpawned && gameSession.Players.Count > 0)
		{
			playersSpawned = true;
			for (int i = 0; i < gameSession.arenaSitting.players.Count; i++)
			{
				if (!gameSession.arenaSitting.players[i].hasEnteredGameArea)
				{
					playersSpawned = false;
					break;
				}
			}
			if (playersSpawned)
			{
				CreatePlayerHate();
			}
		}
		AbstractCreature abstractCreature = base.room.abstractRoom.creatures[Random.Range(0, base.room.abstractRoom.creatures.Count)];
		if (abstractCreature.state.socialMemory != null)
		{
			for (int j = 0; j < abstractCreature.state.socialMemory.relationShips.Count; j++)
			{
				abstractCreature.state.socialMemory.relationShips[j].like = -1f;
				abstractCreature.state.socialMemory.relationShips[j].tempLike = -1f;
			}
		}
	}

	public void CreatePlayerHate()
	{
		for (int i = 0; i < base.room.abstractRoom.creatures.Count; i++)
		{
			if (base.room.abstractRoom.creatures[i].state.socialMemory != null && base.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm)
			{
				for (int j = 0; j < gameSession.Players.Count; j++)
				{
					base.room.abstractRoom.creatures[i].state.socialMemory.GetOrInitiateRelationship(gameSession.Players[j].ID).like = -1f;
					base.room.abstractRoom.creatures[i].state.socialMemory.GetOrInitiateRelationship(gameSession.Players[j].ID).tempLike = -1f;
				}
			}
		}
	}
}
