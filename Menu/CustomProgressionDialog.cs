using System.Collections.Generic;
using System.Linq;
using Expedition;
using RWCustom;
using UnityEngine;

namespace Menu;

public class CustomProgressionDialog : Dialog, SelectOneButton.SelectOneButtonOwner
{
	public FSprite pageTitle;

	public FSprite categoryDivider;

	public SimpleButton cancelButton;

	public ExpeditionMenu expeditionMenu;

	public ProgressionPage progressionPage;

	public float leftAnchor;

	public float rightAnchor;

	public bool opening;

	public bool closing;

	public float movementCounter;

	public List<MissionButton> missionButtons;

	public List<QuestButton> questButtons;

	public MenuLabel currentMod;

	public SymbolButton leftButton;

	public SymbolButton rightButton;

	public SelectOneButton[] contentCategories;

	public float movement;

	public float lastMovement;

	public float movementProgress;

	public float movementLimit;

	public int currentCategory;

	public string modKey;

	public List<string> modKeys;

	public CustomProgressionDialog(ProcessManager manager, ExpeditionMenu expMenu, ProgressionPage progPage)
		: base(manager)
	{
		expeditionMenu = expMenu;
		progressionPage = progPage;
		GetModKeys();
		modKey = modKeys[0];
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		pages[0].pos.y += 2000f;
		pages[0].pos.x += 0.011f;
		pageTitle = new FSprite("custom");
		pageTitle.SetAnchor(0.5f, 0f);
		pageTitle.x = 720f;
		pageTitle.y = 680f;
		pages[0].Container.AddChild(pageTitle);
		categoryDivider = new FSprite("pixel");
		categoryDivider.SetAnchor(0f, 0f);
		categoryDivider.x = 283f;
		categoryDivider.y = 550f;
		categoryDivider.scaleX = 750f;
		categoryDivider.scaleY = 2f;
		categoryDivider.color = new Color(0.4f, 0.4f, 0.4f);
		pages[0].Container.AddChild(categoryDivider);
		cancelButton = new SimpleButton(this, pages[0], Translate("CLOSE"), "CLOSE", new Vector2(633f, 60f), new Vector2(100f, 35f));
		pages[0].subObjects.Add(cancelButton);
		leftButton = new SymbolButton(this, pages[0], "pageleft", "LEFT", new Vector2(293f, 605f));
		leftButton.size = new Vector2(40f, 30f);
		leftButton.roundedRect.size = leftButton.size;
		pages[0].subObjects.Add(leftButton);
		rightButton = new SymbolButton(this, pages[0], "pageright", "RIGHT", new Vector2(343f, 605f));
		rightButton.size = new Vector2(40f, 30f);
		rightButton.roundedRect.size = rightButton.size;
		pages[0].subObjects.Add(rightButton);
		string modID = modKeys[0];
		currentMod = new MenuLabel(this, pages[0], "", new Vector2(398f, 620f), default(Vector2), bigText: true);
		currentMod.text = ModManager.ActiveMods.FirstOrDefault((ModManager.Mod x) => x.id == modKey)?.LocalizedName ?? modKey;
		currentMod.label.alignment = FLabelAlignment.Left;
		pages[0].subObjects.Add(currentMod);
		contentCategories = new SelectOneButton[3];
		contentCategories[0] = new SelectOneButton(this, pages[0], Translate("MISSIONS"), "MISSIONS", new Vector2(483f, 550f), new Vector2(150f, 40f), contentCategories, 0);
		contentCategories[1] = new SelectOneButton(this, pages[0], Translate("QUESTS"), "QUESTS", new Vector2(contentCategories[0].pos.x + 160f, 550f), new Vector2(150f, 40f), contentCategories, 1);
		contentCategories[2] = new SelectOneButton(this, pages[0], Translate("UNLOCKS"), "UNLOCKS", new Vector2(contentCategories[1].pos.x + 160f, 550f), new Vector2(150f, 40f), contentCategories, 2);
		for (int i = 0; i < contentCategories.Length; i++)
		{
			pages[0].subObjects.Add(contentCategories[i]);
		}
		missionButtons = new List<MissionButton>();
		questButtons = new List<QuestButton>();
		GetFirstCategory(modID);
		opening = true;
	}

	public void GetModKeys()
	{
		modKeys = new List<string>();
		for (int i = 0; i < ExpeditionProgression.customMissions.Keys.Count; i++)
		{
			if (!modKeys.Contains(ExpeditionProgression.customMissions.Keys.ElementAt(i)))
			{
				modKeys.Add(ExpeditionProgression.customMissions.Keys.ElementAt(i));
			}
		}
		for (int j = 0; j < ExpeditionProgression.customQuests.Keys.Count; j++)
		{
			if (!modKeys.Contains(ExpeditionProgression.customQuests.Keys.ElementAt(j)))
			{
				modKeys.Add(ExpeditionProgression.customQuests.Keys.ElementAt(j));
			}
		}
		for (int k = 0; k < ExpeditionProgression.perkGroups.Keys.Count; k++)
		{
			if (!modKeys.Contains(ExpeditionProgression.perkGroups.Keys.ElementAt(k)) && ExpeditionProgression.perkGroups.Keys.ElementAt(k) != "expedition" && ExpeditionProgression.perkGroups.Keys.ElementAt(k) != "moreslugcats")
			{
				modKeys.Add(ExpeditionProgression.perkGroups.Keys.ElementAt(k));
			}
		}
		for (int l = 0; l < ExpeditionProgression.burdenGroups.Keys.Count; l++)
		{
			if (!modKeys.Contains(ExpeditionProgression.burdenGroups.Keys.ElementAt(l)) && ExpeditionProgression.burdenGroups.Keys.ElementAt(l) != "expedition" && ExpeditionProgression.burdenGroups.Keys.ElementAt(l) != "moreslugcats")
			{
				modKeys.Add(ExpeditionProgression.burdenGroups.Keys.ElementAt(l));
			}
		}
	}

	public void GetFirstCategory(string modID)
	{
		int[] array = new int[3]
		{
			ExpeditionProgression.customMissions.ContainsKey(modID) ? 1 : 0,
			ExpeditionProgression.customQuests.ContainsKey(modID) ? 1 : 0,
			(ExpeditionProgression.perkGroups.ContainsKey(modID) || ExpeditionProgression.burdenGroups.ContainsKey(modID)) ? 1 : 0
		};
		contentCategories[0].buttonBehav.greyedOut = array[0] == 0;
		contentCategories[1].buttonBehav.greyedOut = array[1] == 0;
		contentCategories[2].buttonBehav.greyedOut = array[2] == 0;
		for (int i = 0; i < array.Length; i++)
		{
			ExpLog.Log($"Cat: {array[i]}");
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] == 1)
			{
				switch (j)
				{
				case 0:
					SetCurrentlySelectedOfSeries("", j);
					GenerateMissionButtons();
					return;
				case 1:
					SetCurrentlySelectedOfSeries("", j);
					GenerateQuestButtons();
					return;
				case 2:
					SetCurrentlySelectedOfSeries("", j);
					GenerateUnlockButtons();
					return;
				}
			}
		}
	}

	public void GenerateMissionButtons()
	{
		ClearButtons();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < ExpeditionProgression.customMissions[modKey].Count; i++)
		{
			MissionButton item = new MissionButton(this, pages[0], "MISSION", ExpeditionProgression.customMissions[modKey][i].key, new Vector2(283f + 190f * (float)num, 450f - 70f * (float)num2), new Vector2(180f, 60f), FLabelAlignment.Center, bigText: true);
			missionButtons.Add(item);
			num++;
			if (num == 4)
			{
				num = 0;
				num2++;
			}
		}
		for (int j = 0; j < missionButtons.Count; j++)
		{
			pages[0].subObjects.Add(missionButtons[j]);
		}
	}

	public void GenerateQuestButtons()
	{
		ClearButtons();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < ExpeditionProgression.customQuests[modKey].Count; i++)
		{
			QuestButton item = new QuestButton(this, pages[0], (i + 1).ToString(), "QUEST", new Vector2(283f + 60f * (float)num, 450f - 60f * (float)num2), new Vector2(50f, 50f), ExpeditionProgression.customQuests[modKey][i].key);
			questButtons.Add(item);
			num++;
			if (num == 10)
			{
				num = 0;
				num2++;
			}
		}
		for (int j = 0; j < questButtons.Count; j++)
		{
			pages[0].subObjects.Add(questButtons[j]);
		}
	}

	public void GenerateUnlockButtons()
	{
		ClearButtons();
	}

	public void ClearButtons()
	{
		for (int i = 0; i < missionButtons.Count; i++)
		{
			missionButtons[i].RemoveSprites();
			pages[0].RemoveSubObject(missionButtons[i]);
		}
		missionButtons = new List<MissionButton>();
		for (int j = 0; j < questButtons.Count; j++)
		{
			questButtons[j].RemoveSprites();
			pages[0].RemoveSubObject(questButtons[j]);
		}
		questButtons = new List<QuestButton>();
	}

	public int GetCurrentlySelectedOfSeries(string series)
	{
		return currentCategory;
	}

	public void SetCurrentlySelectedOfSeries(string series, int to)
	{
		currentCategory = to;
		switch (to)
		{
		case 0:
			GenerateMissionButtons();
			break;
		case 1:
			GenerateQuestButtons();
			break;
		case 2:
			GenerateUnlockButtons();
			break;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (opening)
		{
			if (darkSprite.alpha <= 0.92f)
			{
				darkSprite.alpha += 0.1f * timeStacker;
			}
			movementCounter += 2f * Time.deltaTime;
			float num = Mathf.Lerp(0f, 300f, Custom.SCurve(movementCounter, 0.9f));
			float a = Vector2.Distance(new Vector2(0f, 2000f), default(Vector2));
			float value = Vector2.Distance(pages[0].pos, new Vector2(0f, 0f));
			float num2 = Mathf.Lerp(1f, 0.05f, Mathf.InverseLerp(a, 0f, value));
			pages[0].pos = Custom.MoveTowards(pages[0].pos, new Vector2(0f - leftAnchor, 0.01f), num * num2);
			if (pages[0].pos.y <= 0.01f)
			{
				opening = false;
				movementCounter = 0f;
			}
		}
		if (closing)
		{
			darkSprite.alpha -= 0.1f * timeStacker;
			movementCounter += 1f * Time.deltaTime;
			float num3 = Mathf.Lerp(0f, 300f, Custom.SCurve(movementCounter, 0.9f));
			float a2 = Vector2.Distance(default(Vector2), new Vector2(0f, 2000f));
			float value2 = Vector2.Distance(pages[0].pos, new Vector2(0f, 2000f));
			float num4 = Mathf.Lerp(1f, 0.05f, Mathf.InverseLerp(a2, 0f, value2));
			pages[0].pos = Custom.MoveTowards(pages[0].pos, new Vector2(0f - leftAnchor, 2000f), num3 * num4);
		}
		pageTitle.x = pages[0].pos.x + 685f;
		pageTitle.y = pages[0].pos.y + 680f;
		pageTitle.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 0.85f, darkSprite.alpha));
		categoryDivider.x = pages[0].pos.x + 683f - 400f;
		categoryDivider.y = pages[0].pos.y + 530f;
		categoryDivider.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 0.85f, darkSprite.alpha));
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (message == "CLOSE")
		{
			closing = true;
		}
		if (message == "LEFT")
		{
			int num = modKeys.IndexOf(modKey);
			num = ((num != 0) ? (num - 1) : (modKeys.Count - 1));
			modKey = modKeys[num];
			currentMod.text = ModManager.ActiveMods.FirstOrDefault((ModManager.Mod x) => x.id == modKey)?.LocalizedName ?? modKey;
			GetFirstCategory(modKey);
		}
		if (message == "RIGHT")
		{
			int num2 = modKeys.IndexOf(modKey);
			num2 = ((num2 != modKeys.Count - 1) ? (num2 + 1) : 0);
			modKey = modKeys[num2];
			currentMod.text = ModManager.ActiveMods.FirstOrDefault((ModManager.Mod x) => x.id == modKey)?.LocalizedName ?? modKey;
			GetFirstCategory(modKey);
		}
		if (!(message == "MISSION"))
		{
			return;
		}
		string key = (sender as MissionButton).mission.key;
		foreach (ExpeditionProgression.Mission mission in ExpeditionProgression.missionList)
		{
			if (mission.key == key)
			{
				string text = ExpeditionProgression.MissionRequirements(mission.key);
				ExpLog.Log(text);
				if (manager.rainWorld.progression.IsThereASavedGame(new SlugcatStats.Name(mission.slugcat)))
				{
					PlaySound(SoundID.MENU_Error_Ping);
					break;
				}
				if (text == "")
				{
					progressionPage.AssignMission(mission);
					Vector3 vector = Custom.RGB2HSL(Color.Lerp(PlayerGraphics.DefaultSlugcatColor(new SlugcatStats.Name(mission.slugcat)), Menu.MenuRGB(MenuColors.MediumGrey), 0.2f));
					expeditionMenu.challengeSelect.missionColor = new HSLColor(vector.x, vector.y, vector.z);
					expeditionMenu.challengeSelect.missionName = ExpeditionProgression.GetMissionName(mission.key);
					closing = true;
				}
				else
				{
					PlaySound(SoundID.MENU_Error_Ping);
				}
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (closing && darkSprite.alpha <= 0f)
		{
			pageTitle.RemoveFromContainer();
			manager.StopSideProcess(this);
		}
		cancelButton.buttonBehav.greyedOut = opening;
	}
}
