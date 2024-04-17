using System.Collections.Generic;

namespace Expedition;

public class Eggspedition
{
	public RainWorldGame rwGame;

	public List<Aura> auras;

	public float counter;

	public int[] ints;

	public Eggspedition(RainWorldGame game)
	{
		rwGame = game;
		auras = new List<Aura>();
	}

	public void Update()
	{
		if (rwGame != null)
		{
			counter += 1f;
			if (rwGame.Players == null || rwGame.Players.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < rwGame.Players.Count; i++)
			{
				bool flag = false;
				if (rwGame.Players[i].realizedCreature == null || rwGame.Players[i].realizedCreature.room == null)
				{
					continue;
				}
				for (int j = 0; j < auras.Count; j++)
				{
					if (auras[j] != null)
					{
						if (auras[j].room == rwGame.Players[i].realizedCreature.room && auras[j].ply.playerState.playerNumber == (rwGame.Players[i].realizedCreature as Player).playerState.playerNumber)
						{
							flag = true;
						}
						continue;
					}
					auras.Remove(auras[j]);
					break;
				}
				if (!flag)
				{
					Aura aura = new Aura(rwGame.Players[i].realizedCreature as Player);
					auras.Add(aura);
					rwGame.Players[i].realizedCreature.room.AddObject(aura);
					ExpLog.Log("Add Aura for player " + i);
				}
			}
		}
		else
		{
			ExpeditionGame.egg = null;
		}
	}
}
