using System.Collections.Generic;
using Expedition;
using RWCustom;
using UnityEngine;

namespace HUD;

public class ExpeditionHUD : HudPart
{
	public bool voidSeaWarning;

	public Vector2 pos;

	public Vector2 lastPos;

	public float fade;

	public float lastFade;

	public float endgameCounter = 2f;

	public FLabel challengeHeader;

	public FLabel[] challenges;

	public FSprite[] glowSprites;

	public FSprite[] dividers;

	public FSprite[] strikeThrough;

	public FLabel completeMessage;

	public FLabel completeHelp;

	public FSprite completeGlow;

	public bool revealMode;

	public float revealTimer;

	public bool completeMode;

	public float completeTimer;

	public int challengesToReveal;

	public int challengesToComplete;

	public float glowFade;

	public float lastGlowFade;

	public int revealNum = -1;

	public int completeNum = -1;

	public bool pendingUpdates;

	public List<int> completedChallenges;

	public float initialDisplay = 5f;

	public float alpha;

	public float lastAlpha;

	public ExpeditionHUD(HUD hud, FContainer fContainer)
		: base(hud)
	{
		ExpLog.Log("Init Expedition HUD");
		pos = new Vector2(20.2f, 725.2f);
		fade = 2f;
		challengeHeader = new FLabel(Custom.GetFont(), (ExpeditionData.activeMission == "") ? ChallengeTools.IGT.Translate("Challenge list") : ChallengeTools.IGT.Translate("Mission"));
		challengeHeader.alignment = FLabelAlignment.Left;
		challengeHeader.color = new Color(0.7f, 0.7f, 0.7f);
		hud.fContainers[1].AddChild(challengeHeader);
		challenges = new FLabel[ExpeditionData.challengeList.Count];
		glowSprites = new FSprite[ExpeditionData.challengeList.Count];
		strikeThrough = new FSprite[ExpeditionData.challengeList.Count];
		dividers = new FSprite[challenges.Length - 1];
		completedChallenges = new List<int>();
		float num = 768f;
		for (int i = 0; i < ExpeditionData.challengeList.Count; i++)
		{
			if (ExpeditionData.challengeList[i].completed)
			{
				completedChallenges.Add(i);
			}
			if (ExpeditionData.challengeList[i].hidden)
			{
				ExpeditionData.challengeList[i].revealCheck = false;
				ExpeditionData.challengeList[i].revealCheckDelay = 0;
			}
		}
		num -= 45f * (float)challenges.Length;
		for (int j = 0; j < challenges.Length; j++)
		{
			ExpLog.Log("Init challenge label");
			glowSprites[j] = new FSprite("Futile_White");
			glowSprites[j].scaleX = 60f;
			glowSprites[j].scaleY = 3.2f;
			glowSprites[j].anchorX = 0.2f;
			glowSprites[j].shader = hud.rainWorld.Shaders["FlatLight"];
			glowSprites[j].color = (ExpeditionData.challengeList[j].hidden ? new Color(1f, 0.75f, 0.1f) : new Color(1f, 1f, 1f));
			hud.fContainers[1].AddChild(glowSprites[j]);
			challenges[j] = new FLabel(Custom.GetDisplayFont(), ExpeditionData.challengeList[j].description);
			challenges[j].alignment = FLabelAlignment.Left;
			challenges[j].color = (ExpeditionData.challengeList[j].hidden ? new Color(1f, 0.75f, 0.1f) : new Color(1f, 1f, 1f));
			hud.fContainers[1].AddChild(challenges[j]);
			if (j < challenges.Length - 1)
			{
				dividers[j] = new FSprite("LinearGradient200");
				dividers[j].rotation = 90f;
				dividers[j].scaleY = 2f;
				dividers[j].color = new Color(0.6f, 0.6f, 0.6f);
				hud.fContainers[1].AddChild(dividers[j]);
			}
			strikeThrough[j] = new FSprite("pixel");
			strikeThrough[j].anchorX = 0f;
			strikeThrough[j].scaleY = 2f;
			strikeThrough[j].scaleX = challenges[j].textRect.width + 20f;
			strikeThrough[j].color = (ExpeditionData.challengeList[j].hidden ? new Color(1f, 0.75f, 0.1f) : new Color(1f, 1f, 1f));
			hud.fContainers[1].AddChild(strikeThrough[j]);
		}
		for (int k = 0; k < ExpeditionGame.unlockTrackers.Count; k++)
		{
			if (ExpeditionGame.unlockTrackers[k] is ExpeditionGame.SlowTimeTracker)
			{
				hud.AddPart(new SlowTimeHUD(ExpeditionGame.unlockTrackers[k] as ExpeditionGame.SlowTimeTracker, this, hud, num));
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (initialDisplay > 0f)
		{
			initialDisplay -= 0.025f;
			fade = 2f;
		}
		if (ExpeditionGame.expeditionComplete && ExpeditionData.challengeList != null && endgameCounter > 0f && !revealMode && !completeMode)
		{
			endgameCounter -= 0.015f;
			fade += 0.025f;
		}
		lastPos = pos;
		lastFade = fade;
		lastGlowFade = glowFade;
		if (completeMode)
		{
			CompleteMode();
		}
		if (revealMode && !completeMode)
		{
			RevealMode();
		}
		if (!revealMode && !completeMode)
		{
			fade += (((hud.map != null && hud.map.fade > 0f) || pendingUpdates) ? 0.05f : (ExpeditionGame.expeditionComplete ? (-0.015f) : (-0.025f)));
		}
		fade = Mathf.Clamp(fade, 0f, 2f);
		pos.x = Mathf.Lerp(-450f, 15f, Custom.SCurve(fade, 0.65f)) + 0.2f;
		if (ExpeditionGame.expeditionComplete && completeMessage == null)
		{
			InitCompleteSprites();
		}
		if (voidSeaWarning || !(ExpeditionData.activeMission == "") || !(hud.owner.GetOwnerType() == HUD.OwnerType.Player))
		{
			return;
		}
		if ((hud.owner as Player).room != null && (hud.owner as Player).room.world != null && (hud.owner as Player).room.world.region.name == "SB" && (hud.owner as Player).room.abstractRoom.subregionName != null)
		{
			string text = (hud.owner as Player).room.abstractRoom.subregionName.ToLowerInvariant().Trim();
			if (text.Length > 0 && text == "depths")
			{
				hud.textPrompt.AddMessage(Custom.rainWorld.inGameTranslator.Translate("The Expedition will end upon entering the Depths..."), 10, 300, darken: true, hideHud: true);
				voidSeaWarning = true;
			}
		}
		if ((hud.owner as Player).room != null && (hud.owner as Player).room.abstractRoom.name == "SB_E05SAINT")
		{
			hud.textPrompt.AddMessage(Custom.rainWorld.inGameTranslator.Translate("The Expedition will end upon taking the plunge..."), 10, 300, darken: true, hideHud: true);
			voidSeaWarning = true;
		}
	}

	public override void Draw(float timeStacker)
	{
		if (ExpeditionGame.expeditionComplete && ExpeditionData.challengeList != null && endgameCounter < 1f)
		{
			for (int i = 0; i < challenges.Length; i++)
			{
				glowSprites[i].alpha -= Mathf.Lerp(lastFade, fade, timeStacker);
				challenges[i].alpha -= Mathf.Lerp(lastFade, fade, timeStacker);
				strikeThrough[i].alpha -= Mathf.Lerp(lastFade, fade, timeStacker);
				if (i < challenges.Length - 1)
				{
					dividers[i].alpha -= Mathf.Lerp(lastFade, fade, timeStacker);
				}
			}
			challengeHeader.alpha -= Mathf.Lerp(lastFade, fade, timeStacker);
			if (completeMessage != null && completeGlow != null)
			{
				float y = hud.rainWorld.options.ScreenSize.y;
				float x = ((endgameCounter > 0f) ? Mathf.Lerp(1f, 0f, endgameCounter) : fade);
				completeMessage.y = y + 50f + Mathf.Lerp(0f, -80f, Custom.SCurve(x, 0.6f));
				completeHelp.y = y + 20f + Mathf.Lerp(0f, -80f, Custom.SCurve(x, 0.6f));
				completeGlow.y = y + 50f + Mathf.Lerp(0f, -50f, Custom.SCurve(x, 0.6f));
				completeGlow.alpha = Mathf.Lerp(0f, 0.3f, Custom.SCurve(x, 0.6f));
			}
		}
		else if (ExpeditionData.challengeList != null && ExpeditionData.challengeList.Count == challenges.Length)
		{
			challengeHeader.x = DrawPos(timeStacker).x;
			challengeHeader.y = DrawPos(timeStacker).y + 23f;
			challengeHeader.alpha = fade;
			for (int j = 0; j < challenges.Length; j++)
			{
				float num = 30f * (float)j;
				glowSprites[j].x = DrawPos(timeStacker).x - 120f;
				glowSprites[j].y = DrawPos(timeStacker).y - num;
				challenges[j].x = DrawPos(timeStacker).x;
				challenges[j].y = DrawPos(timeStacker).y;
				challenges[j].y -= num;
				challenges[j].alpha = fade;
				if (j < challenges.Length - 1)
				{
					dividers[j].x = DrawPos(timeStacker).x + 190f;
					dividers[j].y = DrawPos(timeStacker).y;
					dividers[j].y -= num + 16f;
					dividers[j].alpha = Mathf.Lerp(0f, 0.85f, fade);
				}
				strikeThrough[j].x = DrawPos(timeStacker).x - 10f;
				strikeThrough[j].y = DrawPos(timeStacker).y;
				strikeThrough[j].y -= num + 2f;
				if (completeNum != j && revealNum != j)
				{
					strikeThrough[j].alpha = (completedChallenges.Contains(j) ? Mathf.Lerp(0f, 0.95f, fade) : 0f);
					glowSprites[j].alpha = (completedChallenges.Contains(j) ? Mathf.Lerp(0f, 0f, fade) : 0f);
					strikeThrough[j].scaleX = (completedChallenges.Contains(j) ? (challenges[j].textRect.width + 20f) : 0f);
				}
			}
			if (completeMode && completeNum != -1)
			{
				glowSprites[completeNum].alpha = Mathf.Lerp(0.25f, 1f, Mathf.Lerp(lastGlowFade, glowFade, timeStacker));
				strikeThrough[completeNum].alpha = 1f;
				strikeThrough[completeNum].scaleX = Mathf.Lerp(challenges[completeNum].textRect.width + 20f, 0f, Mathf.Lerp(lastGlowFade, glowFade, timeStacker));
			}
			if (revealMode && revealNum != -1)
			{
				glowSprites[revealNum].alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(lastGlowFade, glowFade, timeStacker));
			}
		}
		if (pendingUpdates)
		{
			UpdateChallengeLabels();
		}
	}

	public void CompleteMode()
	{
		fade += 0.025f;
		if (fade < 1.5f)
		{
			return;
		}
		glowFade -= 0.05f;
		if (challengesToComplete > 0)
		{
			completeTimer += 0.05f;
			if (!(completeTimer > 1f))
			{
				return;
			}
			{
				foreach (Challenge challenge in ExpeditionData.challengeList)
				{
					if (challenge.completed && !completedChallenges.Contains(ExpeditionData.challengeList.IndexOf(challenge)))
					{
						if (challenge.hidden)
						{
							challenge.revealed = true;
						}
						challenge.UpdateDescription();
						completeTimer = 0f;
						completeNum = ExpeditionData.challengeList.IndexOf(challenge);
						completedChallenges.Add(completeNum);
						glowFade = 1f;
						challengesToComplete--;
						hud.PlaySound(SoundID.MENU_Karma_Ladder_Increase_Bump);
						break;
					}
				}
				return;
			}
		}
		if (glowFade <= 0f)
		{
			completeMode = false;
			completeNum = -1;
		}
	}

	public void RevealMode()
	{
		fade += 0.025f;
		if (fade < 2f)
		{
			return;
		}
		glowFade -= 0.05f;
		if (challengesToReveal > 0)
		{
			revealTimer += 0.025f;
			if (!(revealTimer > 1f))
			{
				return;
			}
			{
				foreach (Challenge challenge in ExpeditionData.challengeList)
				{
					if (challenge.hidden && !challenge.revealed)
					{
						challenge.revealed = true;
						challenge.UpdateDescription();
						revealTimer = 0f;
						revealNum = ExpeditionData.challengeList.IndexOf(challenge);
						glowFade = 1f;
						challengesToReveal--;
						hud.PlaySound(SoundID.MENU_Start_New_Game);
						break;
					}
				}
				return;
			}
		}
		if (glowFade <= 0f)
		{
			revealMode = false;
			revealNum = -1;
		}
	}

	public void InitCompleteSprites()
	{
		completeGlow = new FSprite("Futile_White");
		completeGlow.shader = hud.rainWorld.Shaders["FlatLight"];
		completeGlow.scaleX = 50f;
		completeGlow.scaleY = 20f;
		completeGlow.x = hud.rainWorld.options.ScreenSize.x / 2f;
		completeGlow.y = hud.rainWorld.options.ScreenSize.y - 50f;
		completeGlow.alpha = 0f;
		hud.fContainers[1].AddChild(completeGlow);
		completeMessage = new FLabel(Custom.GetDisplayFont(), hud.rainWorld.inGameTranslator.Translate("E X P E D I T I O N    C O M P L E T E"));
		completeMessage.shader = hud.rainWorld.Shaders["MenuText"];
		completeMessage.x = (float)(int)(hud.rainWorld.options.ScreenSize.x / 2f) + 0.2f;
		completeMessage.y = hud.rainWorld.options.ScreenSize.y + 50.2f;
		hud.fContainers[1].AddChild(completeMessage);
		completeHelp = new FLabel(Custom.GetFont(), hud.rainWorld.inGameTranslator.Translate("Successfully hibernate to finish this expedition"));
		completeHelp.shader = hud.rainWorld.Shaders["MenuText"];
		completeHelp.x = (float)(int)(hud.rainWorld.options.ScreenSize.x / 2f) + 0.2f;
		completeHelp.y = hud.rainWorld.options.ScreenSize.y + 20.2f;
		hud.fContainers[1].AddChild(completeHelp);
	}

	public void UpdateChallengeLabels()
	{
		if (!(fade < 2f))
		{
			for (int i = 0; i < challenges.Length; i++)
			{
				challenges[i].text = ExpeditionData.challengeList[i].description;
			}
			fade = 2f;
			pendingUpdates = false;
		}
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}
}
