using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Expedition;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ProgressionPage : PositionedMenuObject
{
	public SlugcatStats.Name currentSlugcat;

	public float leftAnchor;

	public float rightAnchor;

	public FSprite pageTitle;

	public SimpleButton backButton;

	public List<MenuLabel> statLabels;

	public QuestButton[] questButtons;

	public MenuLabel perkStat;

	public MenuLabel musicStat;

	public MenuLabel burdenStat;

	public SymbolButton leftPage;

	public SymbolButton milestoneButton;

	public List<MissionButton> missionButtons;

	public bool missionsChecked;

	public MenuLabel missionRequirementLabel;

	public MenuLabel localizedSubtitle;

	public ProgressionPage(Menu menu, MenuObject owner, Vector2 pos)
		: base(menu, owner, pos)
	{
		leftAnchor = (menu as ExpeditionMenu).leftAnchor;
		rightAnchor = (menu as ExpeditionMenu).rightAnchor;
		pageTitle = new FSprite("progression");
		pageTitle.SetAnchor(0.5f, 0f);
		pageTitle.x = 720f;
		pageTitle.y = 680f;
		pageTitle.shader = menu.manager.rainWorld.Shaders["MenuText"];
		Container.AddChild(pageTitle);
		if (menu.manager.rainWorld.options.language != InGameTranslator.LanguageID.English)
		{
			localizedSubtitle = new MenuLabel(menu, this, menu.Translate("-PROGRESSION-"), new Vector2(683f, 740f), default(Vector2), bigText: false);
			localizedSubtitle.label.color = new Color(0.5f, 0.5f, 0.5f);
			subObjects.Add(localizedSubtitle);
		}
		MenuLabel item = new MenuLabel(menu, this, menu.Translate("Challenge Select"), new Vector2(423f, 710f), default(Vector2), bigText: false)
		{
			label = 
			{
				alignment = FLabelAlignment.Right,
				color = new Color(0.7f, 0.7f, 0.7f)
			}
		};
		subObjects.Add(item);
		leftPage = new SymbolButton(menu, this, "Big_Menu_Arrow", "LEFT", new Vector2(433f, 685f));
		leftPage.symbolSprite.rotation = 270f;
		leftPage.size = new Vector2(45f, 45f);
		leftPage.roundedRect.size = leftPage.size;
		subObjects.Add(leftPage);
		if (ExpeditionProgression.customMissions.Keys.Count > 0)
		{
			SymbolButton item2 = new SymbolButton(menu, this, "custommenu", "CUSTOM", new Vector2(888f, 685f))
			{
				size = new Vector2(45f, 45f),
				roundedRect = 
				{
					size = leftPage.size
				}
			};
			subObjects.Add(item2);
			MenuLabel item3 = new MenuLabel(menu, this, menu.Translate("Custom"), new Vector2(943f, 710f), default(Vector2), bigText: false)
			{
				label = 
				{
					alignment = FLabelAlignment.Left,
					color = new Color(0.7f, 0.7f, 0.7f)
				}
			};
			subObjects.Add(item3);
		}
		Vector2 vector = (ModManager.MSC ? new Vector2(430f, 560f) : new Vector2(280f, 570f));
		MenuLabel item4 = new MenuLabel(menu, this, menu.Translate("MISSIONS"), new Vector2(683f, 645f), default(Vector2), bigText: true)
		{
			label = 
			{
				shader = menu.manager.rainWorld.Shaders["MenuTextCustom"]
			}
		};
		subObjects.Add(item4);
		int num = 0;
		int num2 = 0;
		missionButtons = new List<MissionButton>();
		for (int i = 0; i < 8 && (i <= 2 || ModManager.MSC); i++)
		{
			MissionButton item5 = new MissionButton(menu, this, "MISSION", "mis" + (i + 1), new Vector2(683f - (vector.x - 190f * (float)num), vector.y - 70f * (float)num2), new Vector2(180f, 60f), FLabelAlignment.Center, bigText: true);
			num++;
			if (i == 3)
			{
				num = 0;
				num2 = 1;
			}
			missionButtons.Add(item5);
			subObjects.Add(item5);
		}
		missionRequirementLabel = new MenuLabel(menu, this, "", new Vector2(683f, 565f), default(Vector2), bigText: true);
		missionRequirementLabel.label.color = new Color(0.8f, 0.1f, 0.1f);
		subObjects.Add(missionRequirementLabel);
		milestoneButton = new SymbolButton(menu, this, "milestone", "STATS", ModManager.MSC ? new Vector2(1050f, 540f) : new Vector2(653f, 460f));
		milestoneButton.size = new Vector2(60f, 60f);
		milestoneButton.roundedRect.size = milestoneButton.size;
		subObjects.Add(milestoneButton);
		MenuLabel item6 = new MenuLabel(menu, this, menu.Translate("MILESTONES"), new Vector2(milestoneButton.pos.x + milestoneButton.size.x / 2f, milestoneButton.pos.y - 15f), default(Vector2), bigText: false)
		{
			label = 
			{
				color = new Color(0.7f, 0.7f, 0.7f)
			}
		};
		subObjects.Add(item6);
		if (ExpeditionData.devMode)
		{
			subObjects.Add(new MenuLabel(menu, this, "Debug", new Vector2(leftAnchor + 70f, 105f), default(Vector2), bigText: false));
			SimpleButton item7 = new SimpleButton(menu, this, "UNLOCK ALL", "UNLOCKALL", new Vector2(leftAnchor + 20f, 60f), new Vector2(100f, 30f))
			{
				labelColor = new HSLColor(0.1f, 1f, 0.6f),
				rectColor = new HSLColor(0.1f, 1f, 0.6f)
			};
			subObjects.Add(item7);
			SimpleButton item8 = new SimpleButton(menu, this, "RESET ALL", "RESETALL", new Vector2(leftAnchor + 20f, 20f), new Vector2(100f, 30f))
			{
				labelColor = new HSLColor(0f, 1f, 0.6f),
				rectColor = new HSLColor(0f, 1f, 0.6f)
			};
			subObjects.Add(item8);
		}
		MenuLabel item9 = new MenuLabel(menu, this, menu.Translate("QUESTS"), new Vector2(683f, ModManager.MSC ? 445f : 400f), default(Vector2), bigText: true)
		{
			label = 
			{
				shader = menu.manager.rainWorld.Shaders["MenuTextCustom"]
			}
		};
		subObjects.Add(item9);
		MenuLabel item10 = new MenuLabel(menu, this, menu.Translate("Reach milestones and complete expeditions with certain restrictions to earn rewards").WrapText(bigText: true, 850f), new Vector2(683f, ModManager.MSC ? 400f : 370f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = new Color(0.6f, 0.6f, 0.6f)
			}
		};
		subObjects.Add(item10);
		questButtons = new QuestButton[ModManager.MSC ? 75 : 45];
		int num3 = 0;
		int num4 = 0;
		float num5 = 50f;
		List<string> list = new List<string>();
		for (int j = 0; j < ExpeditionProgression.questList.Count; j++)
		{
			list.Add(ExpeditionProgression.questList[j].key);
		}
		for (int k = 0; k < questButtons.Length; k++)
		{
			questButtons[k] = new QuestButton(menu, this, ValueConverter.ConvertToString(k + 1), "qst" + ValueConverter.ConvertToString(k + 1), new Vector2(240f + (num5 + 10f) * (float)num3, (ModManager.MSC ? 300f : 285f) - (num5 + 10f) * (float)num4), new Vector2(num5, num5), "qst" + ValueConverter.ConvertToString(k + 1));
			num3++;
			if (num3 == 15)
			{
				num3 = 0;
				num4++;
			}
			subObjects.Add(questButtons[k]);
		}
		ExpeditionProgression.CountUnlockables();
		Dictionary<string, string> unlockedSongs = ExpeditionProgression.GetUnlockedSongs();
		perkStat = new MenuLabel(menu, this, menu.Translate("PERKS: <current>/<limit>").Replace("<current>", ExpeditionProgression.currentPerks.ToString(CultureInfo.InvariantCulture)).Replace("<limit>", ExpeditionProgression.totalPerks.ToString(CultureInfo.InvariantCulture)), new Vector2(410f, 30f), default(Vector2), bigText: true);
		perkStat.label.shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
		perkStat.label.color = ((ExpeditionProgression.currentPerks >= ExpeditionProgression.totalPerks) ? new Color(1f, 0.75f, 0f) : new Color(0.6f, 0.6f, 0.6f));
		subObjects.Add(perkStat);
		burdenStat = new MenuLabel(menu, this, menu.Translate("BURDENS:") + " " + ExpeditionProgression.currentBurdens.ToString(CultureInfo.InvariantCulture) + "/" + ExpeditionProgression.totalBurdens.ToString(CultureInfo.InvariantCulture), new Vector2(680f, 30f), default(Vector2), bigText: true);
		burdenStat.label.shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
		burdenStat.label.color = ((ExpeditionProgression.currentBurdens >= ExpeditionProgression.totalBurdens) ? new Color(1f, 0.75f, 0f) : new Color(0.6f, 0.6f, 0.6f));
		subObjects.Add(burdenStat);
		musicStat = new MenuLabel(menu, this, menu.Translate("MUSIC:") + " " + ExpeditionProgression.currentTracks.ToString(CultureInfo.InvariantCulture) + "/" + unlockedSongs.Count.ToString(CultureInfo.InvariantCulture), new Vector2(950f, 30f), default(Vector2), bigText: true);
		musicStat.label.shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
		musicStat.label.color = ((ExpeditionProgression.currentTracks >= unlockedSongs.Count) ? new Color(1f, 0.75f, 0f) : new Color(0.6f, 0.6f, 0.6f));
		subObjects.Add(musicStat);
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if ((menu as ExpeditionMenu).pagesMoving)
		{
			return;
		}
		if (message == "LEFT")
		{
			(menu as ExpeditionMenu).MovePage(new Vector2(1500f, 0f));
			(menu as ExpeditionMenu).UpdatePage(2);
		}
		if (message == "MISSION")
		{
			string key = (sender as MissionButton).mission.key;
			foreach (ExpeditionProgression.Mission mission in ExpeditionProgression.missionList)
			{
				if (mission.key == key)
				{
					string text = ExpeditionProgression.MissionRequirements(mission.key);
					ExpLog.Log(text);
					if (menu.manager.rainWorld.progression.IsThereASavedGame(new SlugcatStats.Name(mission.slugcat)))
					{
						menu.PlaySound(SoundID.MENU_Error_Ping);
						return;
					}
					if (text == "")
					{
						AssignMission(mission);
						Vector3 vector = Custom.RGB2HSL(Color.Lerp(PlayerGraphics.DefaultSlugcatColor(new SlugcatStats.Name(mission.slugcat)), Menu.MenuRGB(Menu.MenuColors.MediumGrey), 0.2f));
						(menu as ExpeditionMenu).challengeSelect.missionColor = new HSLColor(vector.x, vector.y, vector.z);
						(menu as ExpeditionMenu).challengeSelect.missionName = ExpeditionProgression.GetMissionName(mission.key);
					}
					else
					{
						menu.PlaySound(SoundID.MENU_Error_Ping);
					}
				}
			}
		}
		if (message == "STATS")
		{
			StatsDialog dialog = new StatsDialog(menu.manager);
			menu.manager.ShowDialog(dialog);
		}
		if (message == "CUSTOM")
		{
			CustomProgressionDialog dialog2 = new CustomProgressionDialog(menu.manager, menu as ExpeditionMenu, this);
			menu.manager.ShowDialog(dialog2);
		}
		if (message == "UNLOCKALL")
		{
			for (int i = 0; i < ExpeditionProgression.questList.Count; i++)
			{
				if (ExpeditionData.completedQuests.Contains(ExpeditionProgression.questList[i].key))
				{
					continue;
				}
				for (int j = 0; j < ExpeditionProgression.questList[i].reward.Length; j++)
				{
					if (!ExpeditionData.unlockables.Contains(ExpeditionProgression.questList[i].reward[j]) || ExpeditionProgression.questList[i].reward[j].StartsWith("per"))
					{
						if (ExpeditionProgression.questList[i].reward[j].StartsWith("mus-"))
						{
							ExpeditionData.newSongs.Add(ExpeditionProgression.questList[i].reward[j]);
						}
						ExpeditionData.unlockables.Add(ExpeditionProgression.questList[i].reward[j]);
					}
				}
				ExpeditionData.completedQuests.Add(ExpeditionProgression.questList[i].key);
			}
			for (int k = 0; k < ExpeditionProgression.missionList.Count; k++)
			{
				if (!ExpeditionData.completedMissions.Contains(ExpeditionProgression.missionList[k].key))
				{
					ExpeditionData.completedMissions.Add(ExpeditionProgression.missionList[k].key);
				}
			}
			global::Expedition.Expedition.coreFile.Save(runEnded: false);
			menu.manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionMenu);
		}
		if (message == "RESETALL")
		{
			ExpeditionData.unlockables = new List<string> { "mus-1" };
			ExpeditionData.completedQuests = new List<string>();
			ExpeditionData.completedMissions = new List<string>();
			ExpeditionData.slugcatWins = new Dictionary<string, int>();
			ExpeditionData.perkLimit = 1;
			ExpeditionData.totalWins = 0;
			ExpeditionData.totalPoints = 0;
			ExpeditionProgression.currentTracks = 0;
			ExpeditionProgression.currentPerks = 0;
			ExpeditionProgression.currentBurdens = 0;
			ExpeditionData.level = 1;
			ExpeditionData.newSongs = new List<string>();
			ExpeditionData.totalChallengesCompleted = 0;
			ExpeditionData.totalHiddenChallengesCompleted = 0;
			ExpeditionData.challengeTypes = new Dictionary<string, int>();
			ExpeditionData.requiredExpeditionContent = new Dictionary<string, List<string>>();
			global::Expedition.Expedition.coreFile.Save(runEnded: false);
			menu.manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionMenu);
		}
	}

	public override void Update()
	{
		base.Update();
		if (currentSlugcat != ExpeditionData.slugcatPlayer)
		{
			currentSlugcat = ExpeditionData.slugcatPlayer;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		pageTitle.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 685f;
		pageTitle.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 680f;
		missionRequirementLabel.label.alpha -= 0.01f * timeStacker;
		for (int i = 0; i < missionButtons.Count; i++)
		{
			if (!missionButtons[i].Selected && !missionButtons[i].MouseOver)
			{
				continue;
			}
			string value = "mis-" + missionButtons[i].mission.key;
			for (int j = 0; j < questButtons.Length && questButtons[j].quest.conditions != null && questButtons[j].quest.conditions.Length != 0; j++)
			{
				if (questButtons[j].quest.conditions.Contains(value))
				{
					questButtons[j].indicate = 1f;
					questButtons[j].color = Color.Lerp(Color.black, Color.white, missionButtons[i].selectRect.sprites[4].alpha);
				}
			}
		}
	}

	public void SetUpSelectables()
	{
		(menu as ExpeditionMenu).muteButton.nextSelectable[3] = missionButtons[0];
		(menu as ExpeditionMenu).muteButton.nextSelectable[2] = backButton;
		(menu as ExpeditionMenu).muteButton.nextSelectable[1] = (menu as ExpeditionMenu).muteButton;
		(menu as ExpeditionMenu).muteButton.nextSelectable[0] = (menu as ExpeditionMenu).exitButton;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		pageTitle.RemoveFromContainer();
	}

	public void AssignMission(ExpeditionProgression.Mission mission)
	{
		ExpeditionData.slugcatPlayer = new SlugcatStats.Name(mission.slugcat);
		(menu as ExpeditionMenu).currentSelection = ExpeditionGame.playableCharacters.IndexOf(ExpeditionData.slugcatPlayer);
		(menu as ExpeditionMenu).characterSelect.UpdateSelectedSlugcat(ExpeditionGame.playableCharacters.IndexOf(ExpeditionData.slugcatPlayer));
		ExpeditionData.activeMission = mission.key;
		ExpeditionData.allChallengeLists[ExpeditionData.slugcatPlayer] = new List<Challenge>();
		for (int i = 0; i < mission.challenges.Count; i++)
		{
			ExpeditionData.allChallengeLists[ExpeditionData.slugcatPlayer].Add(mission.challenges[i]);
		}
		ExpeditionGame.allUnlocks[ExpeditionData.slugcatPlayer] = new List<string>();
		for (int j = 0; j < mission.requirements.Count; j++)
		{
			ExpeditionGame.allUnlocks[ExpeditionData.slugcatPlayer].Add(mission.requirements[j]);
		}
		if ((menu as ExpeditionMenu).challengeSelect.unlocksIndicator != null)
		{
			(menu as ExpeditionMenu).challengeSelect.unlocksIndicator.forceUpdate = true;
		}
		string text = "";
		if (ExpeditionData.missionBestTimes.ContainsKey(mission.key))
		{
			text = TimeSpan.FromSeconds(ExpeditionData.missionBestTimes[mission.key]).ToString("hh\\:mm\\:ss");
		}
		if ((menu as ExpeditionMenu).challengeSelect.missionTime == null)
		{
			(menu as ExpeditionMenu).challengeSelect.missionTime = new MenuLabel(menu, (menu as ExpeditionMenu).challengeSelect, menu.Translate("BEST TIME:") + " " + ((text == "") ? "--/--/--" : text), new Vector2(683f, 570f), default(Vector2), bigText: false);
			(menu as ExpeditionMenu).challengeSelect.missionTime.label.color = new Color(0.6f, 0.6f, 0.6f);
			(menu as ExpeditionMenu).challengeSelect.subObjects.Add((menu as ExpeditionMenu).challengeSelect.missionTime);
			ExpLog.Log("Add time label");
		}
		else
		{
			(menu as ExpeditionMenu).challengeSelect.missionTime.label.text = menu.Translate("BEST TIME:") + " " + ((text == "") ? "--/--/--" : text);
		}
		(menu as ExpeditionMenu).challengeSelect.UpdateChallengeButtons();
		(menu as ExpeditionMenu).MovePage(new Vector2(1500f, 0f));
		(menu as ExpeditionMenu).UpdatePage(2);
		menu.PlaySound(SoundID.MENU_Start_New_Game);
	}
}
