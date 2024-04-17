using System.Collections.Generic;
using UnityEngine;

public abstract class GameSession
{
	public RainWorldGame game;

	public List<AbstractCreature> Players;

	public CreatureCommunities creatureCommunities;

	public float difficulty;

	public SlugcatStats characterStats;

	private float deltaTimer;

	public GameSession(RainWorldGame game)
	{
		this.game = game;
		Players = new List<AbstractCreature>();
		if (this is ArenaGameSession && game.manager.arenaSitting.savCommunities != null)
		{
			creatureCommunities = game.manager.arenaSitting.savCommunities;
			creatureCommunities.session = this;
		}
		else
		{
			creatureCommunities = new CreatureCommunities(this);
		}
		if (game.setupValues.scavengersShy > -1)
		{
			creatureCommunities.scavengerShyness = Mathf.InverseLerp(0f, 100f, game.setupValues.scavengersShy);
		}
		else
		{
			creatureCommunities.scavengerShyness = 1f;
		}
		if (game.setupValues.scavengersLikePlayer != 0)
		{
			for (int i = 0; i < creatureCommunities.playerOpinions.GetLength(1); i++)
			{
				creatureCommunities.SetLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, i - 1, 0, (float)game.setupValues.scavengersLikePlayer / 100f);
			}
		}
		if (ModManager.MSC && game.rainWorld.safariMode)
		{
			creatureCommunities.scavengerShyness = 0f;
		}
		if (ModManager.MMF && game.IsArenaSession)
		{
			creatureCommunities.scavengerShyness = 0f;
		}
		RainWorld.lockGameTimer = false;
	}

	public virtual void AddPlayer(AbstractCreature player)
	{
		Players.Add(player);
	}
}
