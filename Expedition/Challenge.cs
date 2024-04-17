using HUD;

namespace Expedition;

public abstract class Challenge
{
	public bool revealCheck;

	public int revealCheckDelay;

	public RainWorldGame game;

	public bool completed;

	public string description;

	public bool hidden;

	public bool revealed;

	public Challenge()
	{
	}

	public virtual void Update()
	{
		if (game != null && !completed)
		{
			if (revealCheckDelay < 100)
			{
				revealCheckDelay++;
			}
			if (hidden && !revealCheck && revealCheckDelay >= 100)
			{
				revealCheck = true;
				CheckRevealable();
			}
		}
	}

	public virtual bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
	{
		return true;
	}

	public virtual void FromString(string args)
	{
	}

	public virtual int Points()
	{
		return 1;
	}

	public virtual string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Challenge");
	}

	public virtual void UpdateDescription()
	{
		if (hidden && !revealed)
		{
			description = ChallengeTools.IGT.Translate("HIDDEN");
		}
		else
		{
			if (game == null || game.cameras == null || game.cameras[0].hud == null)
			{
				return;
			}
			for (int i = 0; i < game.cameras[0].hud.parts.Count; i++)
			{
				if (game.cameras[0].hud.parts[i] is ExpeditionHUD)
				{
					(game.cameras[0].hud.parts[i] as ExpeditionHUD).pendingUpdates = true;
				}
			}
		}
	}

	public virtual void CompleteChallenge()
	{
		if (completed)
		{
			return;
		}
		completed = true;
		int num = 0;
		bool flag = true;
		foreach (Challenge challenge in ExpeditionData.challengeList)
		{
			if (!challenge.hidden && !challenge.completed)
			{
				flag = false;
			}
			else if (challenge.hidden && !challenge.revealed)
			{
				num++;
			}
		}
		if (game != null && game.cameras != null && game.cameras[0].hud != null)
		{
			UpdateDescription();
			for (int i = 0; i < game.cameras[0].hud.parts.Count; i++)
			{
				if (game.cameras[0].hud.parts[i] is ExpeditionHUD)
				{
					(game.cameras[0].hud.parts[i] as ExpeditionHUD).completeMode = true;
					(game.cameras[0].hud.parts[i] as ExpeditionHUD).challengesToComplete++;
					if (flag)
					{
						(game.cameras[0].hud.parts[i] as ExpeditionHUD).challengesToReveal = num;
						(game.cameras[0].hud.parts[i] as ExpeditionHUD).revealMode = true;
					}
				}
			}
		}
		if (ExpeditionGame.activeUnlocks.Contains("unl-passage"))
		{
			ExpeditionData.earnedPassages++;
		}
	}

	public virtual bool Duplicable(Challenge challenge)
	{
		return false;
	}

	public virtual bool CombatRequired()
	{
		return false;
	}

	public virtual bool RespondToCreatureKill()
	{
		return false;
	}

	public virtual bool CanBeHidden()
	{
		return true;
	}

	public virtual void CreatureKilled(Creature crit, int playerNumber)
	{
	}

	public virtual Challenge Generate()
	{
		return null;
	}

	public virtual void Reset()
	{
		completed = false;
		if (hidden)
		{
			hidden = false;
		}
		UpdateDescription();
	}

	public void CheckRevealable()
	{
		if (!hidden || revealed)
		{
			return;
		}
		int num = 0;
		bool flag = true;
		foreach (Challenge challenge in ExpeditionData.challengeList)
		{
			if (!challenge.hidden && !challenge.completed)
			{
				flag = false;
			}
			else if (challenge.hidden && !challenge.revealed)
			{
				num++;
			}
		}
		if (game == null || game.cameras == null || game.cameras[0].hud == null)
		{
			return;
		}
		for (int i = 0; i < game.cameras[0].hud.parts.Count; i++)
		{
			if (game.cameras[0].hud.parts[i] is ExpeditionHUD && flag)
			{
				(game.cameras[0].hud.parts[i] as ExpeditionHUD).challengesToReveal = num;
				(game.cameras[0].hud.parts[i] as ExpeditionHUD).revealMode = true;
			}
		}
	}
}
