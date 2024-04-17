using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace HUD;

public class GeneralMultiplayerHud : HudPart
{
	public ArenaGameSession session;

	public int surviveTime;

	public FLabel surviveLabel;

	public int parryTarget;

	public FLabel parryLabel;

	public GeneralMultiplayerHud(HUD hud, ArenaGameSession session)
		: base(hud)
	{
		this.session = session;
		if (ModManager.MSC)
		{
			if (session.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && (session.arenaSitting.gameTypeSetup.challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.SURVIVE || session.arenaSitting.gameTypeSetup.challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.SURVIVE))
			{
				surviveTime = session.arenaSitting.gameTypeSetup.challengeMeta.surviveTime;
				surviveLabel = new FLabel(Custom.GetDisplayFont(), "");
				hud.fContainers[1].AddChild(surviveLabel);
			}
			if (session.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && (session.arenaSitting.gameTypeSetup.challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.PARRY || session.arenaSitting.gameTypeSetup.challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.PARRY))
			{
				parryTarget = session.arenaSitting.gameTypeSetup.challengeMeta.parries;
				parryLabel = new FLabel(Custom.GetDisplayFont(), "");
				hud.fContainers[1].AddChild(parryLabel);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (surviveLabel != null)
		{
			int num = session.arenaSitting.players[0].timeAlive / 40;
			int num2 = surviveTime - num;
			if (num2 < 0)
			{
				num2 = 0;
			}
			surviveLabel.text = SecondsToMinutesAndSecondsString(num2);
		}
		if (parryLabel != null)
		{
			int num3 = Math.Min(parryTarget, session.arenaSitting.players[0].parries);
			parryLabel.text = hud.rainWorld.inGameTranslator.Translate("Parries:") + " " + num3 + " / " + parryTarget;
		}
	}

	public override void Draw(float timeStacker)
	{
		base.Draw(timeStacker);
		if (surviveLabel != null)
		{
			surviveLabel.x = Mathf.Ceil(hud.rainWorld.options.ScreenSize.x / 2f) - 1f / 3f;
			surviveLabel.y = hud.rainWorld.options.ScreenSize.y - 50f - 1f / 3f;
		}
		if (parryLabel != null)
		{
			int num = ((surviveLabel != null) ? 40 : 0);
			parryLabel.x = Mathf.Ceil(hud.rainWorld.options.ScreenSize.x / 2f) - 1f / 3f;
			parryLabel.y = hud.rainWorld.options.ScreenSize.y - 50f - 1f / 3f - (float)num;
		}
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		if (surviveLabel != null)
		{
			surviveLabel.RemoveFromContainer();
		}
		if (parryLabel != null)
		{
			parryLabel.RemoveFromContainer();
		}
	}

	public string SecondsToMinutesAndSecondsString(int secs)
	{
		int num = Mathf.FloorToInt((float)secs / 60f);
		secs -= num * 60;
		return num.ToString("D2") + ":" + secs.ToString("D2");
	}
}
