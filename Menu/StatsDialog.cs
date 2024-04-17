using System;
using System.Collections.Generic;
using System.Globalization;
using Expedition;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class StatsDialog : Dialog
{
	public MenuTabWrapper menuTabWrapper;

	public OpHoldButton resetAll;

	public CheckBox resetQuest;

	public CheckBox resetMission;

	public FSprite pageTitle;

	public FSprite topSeparator;

	public FSprite bottomSeparator;

	public SimpleButton cancelButton;

	public float leftAnchor;

	public float rightAnchor;

	public bool opening;

	public bool closing;

	public float movementCounter;

	public bool wipeQuests;

	public bool wipeMissions;

	public float targetAlpha;

	public float currentAlpha;

	public float lastAlpha;

	public float uAlpha;

	public MenuLabel localizedSubtitle;

	public StatsDialog(ProcessManager manager)
		: base(manager)
	{
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		pages[0].pos.y += 2000f;
		pageTitle = new FSprite("milestones");
		pageTitle.SetAnchor(0.5f, 0f);
		pageTitle.x = 720f;
		pageTitle.y = 680f;
		pages[0].Container.AddChild(pageTitle);
		topSeparator = new FSprite("pixel");
		topSeparator.scaleX = 800f;
		topSeparator.scaleY = 2f;
		topSeparator.alpha = 0.6f;
		topSeparator.SetAnchor(0.5f, 0f);
		topSeparator.color = new Color(0.4f, 0.4f, 0.4f);
		pages[0].Container.AddChild(topSeparator);
		bottomSeparator = new FSprite("pixel");
		bottomSeparator.scaleX = 800f;
		bottomSeparator.scaleY = 2f;
		bottomSeparator.alpha = 0.6f;
		bottomSeparator.SetAnchor(0.5f, 0f);
		bottomSeparator.color = new Color(0.4f, 0.4f, 0.4f);
		pages[0].Container.AddChild(bottomSeparator);
		if (manager.rainWorld.options.language != InGameTranslator.LanguageID.English)
		{
			localizedSubtitle = new MenuLabel(this, pages[0], Translate("-MILESTONES-"), new Vector2(683f, 740f), default(Vector2), bigText: false);
			localizedSubtitle.label.color = new Color(0.5f, 0.5f, 0.5f);
			pages[0].subObjects.Add(localizedSubtitle);
		}
		float num = 85f;
		float num2 = LabelTest.GetWidth(Translate("CLOSE")) + 15f;
		if (num2 > num)
		{
			num = num2;
		}
		cancelButton = new SimpleButton(this, pages[0], Translate("CLOSE"), "CLOSE", new Vector2(1083f - num, 150f), new Vector2(num, 35f));
		pages[0].subObjects.Add(cancelButton);
		menuTabWrapper = new MenuTabWrapper(this, pages[0]);
		pages[0].subObjects.Add(menuTabWrapper);
		float num3 = 150f;
		float num4 = LabelTest.GetWidth(Translate("RESET PROGRESS")) + 15f;
		if (num4 > num3)
		{
			num3 = num4;
		}
		resetAll = new OpHoldButton(new Vector2(283f, 150f), new Vector2(num3, 35f), Translate("RESET PROGRESS"), 900f);
		resetAll.OnPressDone += ResetAll_OnPressDone;
		resetAll.description = " ";
		resetAll.colorEdge = new Color(1f, 0f, 0f);
		new UIelementWrapper(menuTabWrapper, resetAll);
		float x = 683f;
		MenuLabel item = new MenuLabel(this, pages[0], Translate("GENERAL"), new Vector2(x, 600f), default(Vector2), bigText: true)
		{
			label = 
			{
				shader = manager.rainWorld.Shaders["MenuTextCustom"]
			}
		};
		pages[0].subObjects.Add(item);
		MenuLabel item2 = new MenuLabel(this, pages[0], Translate("Current Level: <level>").Replace("<level>", ExpeditionData.level.ToString(CultureInfo.InvariantCulture)), new Vector2(x, 560f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.MediumGrey)
			}
		};
		pages[0].subObjects.Add(item2);
		MenuLabel item3 = new MenuLabel(this, pages[0], Translate("Current Points: <points>").Replace("<points>", ExpeditionData.currentPoints.ToString(CultureInfo.InvariantCulture)), new Vector2(x, 535f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.MediumGrey)
			}
		};
		pages[0].subObjects.Add(item3);
		MenuLabel item4 = new MenuLabel(this, pages[0], Translate("Points to Next Level: <points>").Replace("<points>", (ExpeditionProgression.LevelCap(ExpeditionData.level) - ExpeditionData.currentPoints).ToString(CultureInfo.InvariantCulture)), new Vector2(x, 510f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.MediumGrey)
			}
		};
		pages[0].subObjects.Add(item4);
		MenuLabel item5 = new MenuLabel(this, pages[0], Translate("Total Points:  <points>").Replace("<points>", ExpeditionData.totalPoints.ToString(CultureInfo.InvariantCulture)), new Vector2(x, 470f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.MediumGrey)
			}
		};
		pages[0].subObjects.Add(item5);
		MenuLabel menuLabel = new MenuLabel(this, pages[0], (Translate("QUESTS") + ": <quests> / " + (ModManager.MSC ? "75" : "45")).Replace("<quests>", ExpeditionData.completedQuests.Count.ToString(CultureInfo.InvariantCulture)), new Vector2(x, 390f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.MediumGrey)
			}
		};
		if (ExpeditionData.completedQuests.Count >= 75)
		{
			menuLabel.label.shader = manager.rainWorld.Shaders["MenuTextGold"];
		}
		pages[0].subObjects.Add(menuLabel);
		MenuLabel menuLabel2 = new MenuLabel(this, pages[0], (Translate("MISSIONS") + ": <missions> / " + (ModManager.MSC ? "8" : "3")).Replace("<missions>", ExpeditionData.completedMissions.Count.ToString(CultureInfo.InvariantCulture)), new Vector2(x, 365f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.MediumGrey)
			}
		};
		if (ExpeditionData.completedMissions.Count >= 8)
		{
			menuLabel2.label.shader = manager.rainWorld.Shaders["MenuTextGold"];
		}
		pages[0].subObjects.Add(menuLabel2);
		float num5 = 283f;
		MenuLabel item6 = new MenuLabel(this, pages[0], Translate("UNLOCKABLES"), new Vector2(num5, 265f), default(Vector2), bigText: true)
		{
			label = 
			{
				shader = manager.rainWorld.Shaders["MenuTextCustom"],
				alignment = FLabelAlignment.Left
			}
		};
		pages[0].subObjects.Add(item6);
		Dictionary<string, string> unlockedSongs = ExpeditionProgression.GetUnlockedSongs();
		MenuLabel item7 = new MenuLabel(this, pages[0], Translate("PERKS: <current>/<limit>").Replace("<current>", ExpeditionProgression.currentPerks.ToString(CultureInfo.InvariantCulture)).Replace("<limit>", ExpeditionProgression.totalPerks.ToString(CultureInfo.InvariantCulture)), new Vector2(num5, 230f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = ((ExpeditionProgression.currentPerks >= ExpeditionProgression.totalPerks) ? new Color(1f, 0.75f, 0f) : Menu.MenuRGB(MenuColors.MediumGrey)),
				shader = ((ExpeditionProgression.currentPerks >= ExpeditionProgression.totalPerks) ? manager.rainWorld.Shaders["MenuTextGold"] : manager.rainWorld.Shaders["Basic"]),
				alignment = FLabelAlignment.Left
			}
		};
		pages[0].subObjects.Add(item7);
		MenuLabel item8 = new MenuLabel(this, pages[0], Translate("BURDENS:") + " " + ExpeditionProgression.currentBurdens.ToString(CultureInfo.InvariantCulture) + "/" + ExpeditionProgression.totalBurdens.ToString(CultureInfo.InvariantCulture), new Vector2(num5 + 170f, 230f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.MediumGrey),
				shader = ((ExpeditionProgression.currentBurdens >= ExpeditionProgression.totalBurdens) ? manager.rainWorld.Shaders["MenuTextGold"] : manager.rainWorld.Shaders["Basic"]),
				alignment = FLabelAlignment.Left
			}
		};
		pages[0].subObjects.Add(item8);
		MenuLabel item9 = new MenuLabel(this, pages[0], Translate("MUSIC:") + " " + ExpeditionProgression.currentTracks.ToString(CultureInfo.InvariantCulture) + "/" + unlockedSongs.Count.ToString(CultureInfo.InvariantCulture), new Vector2(num5 + 350f, 230f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = ((ExpeditionProgression.currentTracks >= unlockedSongs.Count) ? new Color(1f, 0.75f, 0f) : Menu.MenuRGB(MenuColors.MediumGrey)),
				shader = ((ExpeditionProgression.currentTracks >= unlockedSongs.Count) ? manager.rainWorld.Shaders["MenuTextGold"] : manager.rainWorld.Shaders["Basic"]),
				alignment = FLabelAlignment.Left
			}
		};
		pages[0].subObjects.Add(item9);
		float x2 = 283f;
		MenuLabel item10 = new MenuLabel(this, pages[0], Translate("EXPEDITIONS"), new Vector2(x2, 600f), default(Vector2), bigText: true)
		{
			label = 
			{
				shader = manager.rainWorld.Shaders["MenuTextCustom"],
				alignment = FLabelAlignment.Left
			}
		};
		pages[0].subObjects.Add(item10);
		MenuLabel item11 = new MenuLabel(this, pages[0], Translate("Total: <total>").Replace("<total>", ExpeditionData.totalWins.ToString(CultureInfo.InvariantCulture)), new Vector2(x2, 560f), default(Vector2), bigText: true)
		{
			label = 
			{
				alignment = FLabelAlignment.Left,
				color = Menu.MenuRGB(MenuColors.MediumGrey)
			}
		};
		pages[0].subObjects.Add(item11);
		for (int i = 0; i < ExpeditionGame.playableCharacters.Count; i++)
		{
			int num6 = 0;
			if (ExpeditionData.slugcatWins.ContainsKey(ExpeditionGame.playableCharacters[i].value))
			{
				num6 = ExpeditionData.slugcatWins[ExpeditionGame.playableCharacters[i].value];
			}
			MenuLabel item12 = new MenuLabel(this, pages[0], Translate(SlugcatStats.getSlugcatName(ExpeditionGame.playableCharacters[i])) + ": " + num6, new Vector2(x2, 520f - 26f * (float)i), default(Vector2), bigText: true)
			{
				label = 
				{
					color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(ExpeditionGame.playableCharacters[i]), Menu.MenuRGB(MenuColors.MediumGrey), 0.2f),
					alignment = FLabelAlignment.Left
				}
			};
			pages[0].subObjects.Add(item12);
		}
		float x3 = 1083f;
		MenuLabel item13 = new MenuLabel(this, pages[0], Translate("CHALLENGES"), new Vector2(x3, 600f), default(Vector2), bigText: true)
		{
			label = 
			{
				shader = manager.rainWorld.Shaders["MenuTextCustom"],
				alignment = FLabelAlignment.Right
			}
		};
		pages[0].subObjects.Add(item13);
		MenuLabel item14 = new MenuLabel(this, pages[0], Translate("Normal: <num>").Replace("<num>", ExpeditionData.totalChallengesCompleted.ToString(CultureInfo.InvariantCulture)), new Vector2(x3, 560f), default(Vector2), bigText: true)
		{
			label = 
			{
				alignment = FLabelAlignment.Right,
				color = Menu.MenuRGB(MenuColors.MediumGrey)
			}
		};
		pages[0].subObjects.Add(item14);
		MenuLabel item15 = new MenuLabel(this, pages[0], Translate("Hidden: <num>").Replace("<num>", ExpeditionData.totalHiddenChallengesCompleted.ToString(CultureInfo.InvariantCulture)), new Vector2(x3, 535f), default(Vector2), bigText: true)
		{
			label = 
			{
				alignment = FLabelAlignment.Right,
				color = new Color(0.9f, 0.65f, 0.1f)
			}
		};
		pages[0].subObjects.Add(item15);
		for (int j = 0; j < ChallengeOrganizer.availableChallengeTypes.Count; j++)
		{
			int num7 = 0;
			if (ExpeditionData.challengeTypes.ContainsKey(ChallengeOrganizer.availableChallengeTypes[j].GetType().Name))
			{
				num7 = ExpeditionData.challengeTypes[ChallengeOrganizer.availableChallengeTypes[j].GetType().Name];
			}
			MenuLabel item16 = new MenuLabel(this, pages[0], ChallengeOrganizer.availableChallengeTypes[j].ChallengeName() + ": " + num7, new Vector2(x3, 490f - 26f * (float)j), default(Vector2), bigText: true)
			{
				label = 
				{
					alignment = FLabelAlignment.Right,
					color = new HSLColor(Mathf.InverseLerp(0f, ChallengeOrganizer.availableChallengeTypes.Count, j), 0.3f, 0.75f).rgb
				}
			};
			pages[0].subObjects.Add(item16);
		}
		opening = true;
		targetAlpha = 1f;
	}

	private void ResetAll_OnPressDone(UIfocusable trigger)
	{
		ExpeditionData.unlockables = new List<string> { "mus-1" };
		ExpeditionData.level = 1;
		ExpeditionData.currentPoints = 0;
		ExpeditionData.totalPoints = 0;
		ExpeditionData.perkLimit = 1;
		ExpeditionData.totalChallengesCompleted = 0;
		ExpeditionData.totalHiddenChallengesCompleted = 0;
		ExpeditionData.challengeTypes = new Dictionary<string, int>();
		ExpeditionData.totalWins = 0;
		ExpeditionData.slugcatPlayer = SlugcatStats.Name.White;
		ExpeditionData.saveSlot = -1;
		ExpeditionData.unlockables = new List<string> { "mus-1" };
		ExpeditionData.completedQuests = new List<string>();
		ExpeditionData.completedMissions = new List<string>();
		ExpeditionData.slugcatWins = new Dictionary<string, int>();
		ExpeditionData.newSongs = new List<string>();
		ExpeditionData.menuSong = "";
		ExpeditionData.allChallengeLists = new Dictionary<SlugcatStats.Name, List<Challenge>>();
		ExpeditionGame.allUnlocks = new Dictionary<SlugcatStats.Name, List<string>>();
		ExpeditionData.allActiveMissions = new Dictionary<string, string>();
		ExpeditionData.missionBestTimes = new Dictionary<string, int>();
		ExpeditionData.ints = new int[8];
		ExpeditionData.requiredExpeditionContent = new Dictionary<string, List<string>>();
		ExpeditionData.allChallengeLists = new Dictionary<SlugcatStats.Name, List<Challenge>>();
		manager.rainWorld.progression.WipeAll();
		global::Expedition.Expedition.coreFile.Save(runEnded: false);
		closing = true;
		PlaySound(SoundID.Slugcat_Ghost_Dissappear);
		manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionMenu);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (opening || closing)
		{
			uAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, currentAlpha, timeStacker)), 1.5f);
			darkSprite.alpha = uAlpha * 0.95f;
		}
		pages[0].pos.y = Mathf.Lerp(manager.rainWorld.options.ScreenSize.y + 100f, 0.01f, (uAlpha < 0.999f) ? uAlpha : 1f);
		pageTitle.x = Mathf.Lerp(pages[0].lastPos.x + 685f, pages[0].pos.x + 685f, timeStacker);
		pageTitle.y = Mathf.Lerp(pages[0].lastPos.y + 680f, pages[0].pos.y + 680f, timeStacker);
		pageTitle.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 0.85f, darkSprite.alpha));
		topSeparator.x = Mathf.Lerp(pages[0].lastPos.x + 683f, pages[0].pos.x + 683f, timeStacker);
		topSeparator.y = Mathf.Lerp(pages[0].lastPos.y + 630f, pages[0].pos.y + 630f, timeStacker);
		bottomSeparator.x = Mathf.Lerp(pages[0].lastPos.x + 683f, pages[0].pos.x + 683f, timeStacker);
		bottomSeparator.y = Mathf.Lerp(pages[0].lastPos.y + 200f, pages[0].pos.y + 200f, timeStacker);
		if (!opening && !closing)
		{
			return;
		}
		for (int i = 0; i < pages[0].subObjects.Count; i++)
		{
			if (pages[0].subObjects[i] is MenuLabel)
			{
				(pages[0].subObjects[i] as MenuLabel).label.alpha = Mathf.InverseLerp(0f, 0.85f, darkSprite.alpha);
			}
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (message == "CLOSE")
		{
			closing = true;
			targetAlpha = 0f;
		}
	}

	public override void Update()
	{
		base.Update();
		lastAlpha = currentAlpha;
		currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, 0.2f);
		if (opening && pages[0].pos.y <= 0.011f)
		{
			opening = false;
		}
		if (closing && Math.Abs(currentAlpha - targetAlpha) < 0.09f)
		{
			pageTitle.RemoveFromContainer();
			topSeparator.RemoveFromContainer();
			bottomSeparator.RemoveFromContainer();
			manager.StopSideProcess(this);
			closing = false;
		}
		cancelButton.buttonBehav.greyedOut = opening;
	}
}
