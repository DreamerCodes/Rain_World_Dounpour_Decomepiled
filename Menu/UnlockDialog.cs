using System;
using System.Collections.Generic;
using System.Linq;
using Expedition;
using Menu.Remix;
using Menu.Remix.MixedUI;
using Modding.Expedition;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class UnlockDialog : Dialog
{
	public class PerkPage
	{
		public List<PerkGroup> PerkGroups = new List<PerkGroup>();
	}

	public class PerkGroup
	{
		public string GroupName;

		public List<string> Perks = new List<string>();
	}

	public SimpleButton cancelButton;

	public float leftAnchor;

	public float rightAnchor;

	public ChallengeSelectPage owner;

	public bool opening;

	public bool closing;

	public FSprite pageTitle;

	public MenuLabel perkLimit;

	public float perkFade;

	public float movementCounter;

	public MenuLabel perkNameLabel;

	public MenuLabel perkDescLabel;

	public List<PerkButton> perkButtons;

	public string selectedUnlock = "";

	public SymbolButton perkPageLeft;

	public SymbolButton perkPageRight;

	public List<PerkPage> perkPages;

	public List<MenuLabel> perkGroupLabels;

	public int currentPerkPage;

	public float uAlpha;

	public float lastAlpha;

	public float currentAlpha;

	public float targetAlpha;

	public BigSimpleButton doomedBurden;

	public BigSimpleButton blindedBurden;

	public BigSimpleButton huntedBurden;

	public BigSimpleButton pursuedBurden;

	public List<string> burdenDescriptions;

	public List<string> burdenNames;

	public SymbolButton burdenPageLeft;

	public SymbolButton burdenPageRight;

	public List<BigSimpleButton> burdenButtons;

	public List<List<string>> burdenPages;

	public int currentBurdenPage;

	public MenuLabel localizedSubtitle;

	public UnlockDialog(ProcessManager manager, ChallengeSelectPage owner)
		: base(manager)
	{
		this.owner = owner;
		leftAnchor = owner.leftAnchor;
		rightAnchor = owner.rightAnchor;
		pages[0].pos.y += 2000f;
		float num = 85f;
		float num2 = LabelTest.GetWidth(Translate("CLOSE")) + 10f;
		if (num2 > num)
		{
			num = num2;
		}
		cancelButton = new SimpleButton(this, pages[0], Translate("CLOSE"), "CLOSE", new Vector2(683f - num / 2f, 60f), new Vector2(num, 35f));
		pages[0].subObjects.Add(cancelButton);
		pageTitle = new FSprite("unlockables");
		pageTitle.SetAnchor(0.5f, 0f);
		pageTitle.x = 720f;
		pageTitle.y = 680f;
		pages[0].Container.AddChild(pageTitle);
		if (manager.rainWorld.options.language != InGameTranslator.LanguageID.English)
		{
			localizedSubtitle = new MenuLabel(this, pages[0], Translate("-UNLOCKABLES-"), new Vector2(683f, 740f), default(Vector2), bigText: false);
			localizedSubtitle.label.color = new Color(0.5f, 0.5f, 0.5f);
			pages[0].subObjects.Add(localizedSubtitle);
		}
		perkButtons = new List<PerkButton>();
		perkGroupLabels = new List<MenuLabel>();
		burdenButtons = new List<BigSimpleButton>();
		MenuLabel item = new MenuLabel(this, pages[0], Translate("Enable a range of perks to aid you in your expedition"), new Vector2(683f, 645f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = new Color(0.6f, 0.6f, 0.6f)
			}
		};
		pages[0].subObjects.Add(item);
		int num3 = 0;
		for (int i = 0; i < ExpeditionGame.activeUnlocks.Count; i++)
		{
			if (ExpeditionGame.activeUnlocks[i].StartsWith("unl-"))
			{
				num3++;
			}
		}
		perkLimit = new MenuLabel(this, pages[0], "PERKS: 0/1", new Vector2(1000.01f, 545f), default(Vector2), bigText: true);
		perkLimit.label.alignment = FLabelAlignment.Left;
		perkLimit.label.color = new Color(0.5f, 0.5f, 0.5f);
		perkLimit.label.text = Translate("PERKS: <current>/<limit>").Replace("<current>", ValueConverter.ConvertToString(num3)).Replace("<limit>", ValueConverter.ConvertToString(ExpeditionData.perkLimit));
		pages[0].subObjects.Add(perkLimit);
		perkPages = new List<PerkPage>();
		perkPages.Add(new PerkPage());
		foreach (KeyValuePair<string, List<string>> perkGroup2 in ExpeditionProgression.perkGroups)
		{
			string groupName = perkGroup2.Key;
			List<string> value = perkGroup2.Value;
			PerkPage perkPage = perkPages.Last();
			if (perkPage.PerkGroups.Count >= 2)
			{
				perkPage = new PerkPage();
				perkPages.Add(perkPage);
			}
			PerkGroup perkGroup = perkPage.PerkGroups.FirstOrDefault((PerkGroup x) => x.GroupName == groupName && x.Perks.Count < 8);
			if (perkGroup == null)
			{
				perkGroup = new PerkGroup
				{
					GroupName = groupName
				};
				perkPage.PerkGroups.Add(perkGroup);
			}
			foreach (string item3 in value)
			{
				if (perkGroup.Perks.Count >= 8)
				{
					perkGroup = new PerkGroup
					{
						GroupName = groupName
					};
					if (perkPage.PerkGroups.Count >= 2)
					{
						perkPage = new PerkPage();
						perkPages.Add(perkPage);
					}
					perkPage.PerkGroups.Add(perkGroup);
				}
				perkGroup.Perks.Add(item3);
			}
		}
		burdenPages = new List<List<string>>();
		burdenPages.Add(new List<string>());
		foreach (KeyValuePair<string, List<string>> burdenGroup in ExpeditionProgression.burdenGroups)
		{
			foreach (string item4 in burdenGroup.Value)
			{
				List<string> list = burdenPages.Last();
				if (list.Count >= 4)
				{
					list = new List<string>();
					burdenPages.Add(list);
				}
				list.Add(item4);
			}
		}
		perkNameLabel = new MenuLabel(this, pages[0], "", new Vector2(683f, (perkPages[0].PerkGroups.Count > 1) ? 450f : 500f), default(Vector2), bigText: false);
		perkNameLabel.label.shader = manager.rainWorld.Shaders["MenuText"];
		pages[0].subObjects.Add(perkNameLabel);
		perkDescLabel = new MenuLabel(this, pages[0], "", new Vector2(683f, perkNameLabel.pos.y - 35f), default(Vector2), bigText: false);
		perkDescLabel.label.color = new Color(0.8f, 0.8f, 0.8f);
		pages[0].subObjects.Add(perkDescLabel);
		if (perkPages.Count > 1)
		{
			perkPageLeft = new SymbolButton(this, pages[0], "Big_Menu_Arrow", "PERK_PAGES_LEFT", new Vector2(388f, 512f));
			perkPageLeft.symbolSprite.rotation = 270f;
			perkPageLeft.size = new Vector2(45f, 45f);
			perkPageLeft.roundedRect.size = perkPageLeft.size;
			pages[0].subObjects.Add(perkPageLeft);
			perkPageRight = new SymbolButton(this, pages[0], "Big_Menu_Arrow", "PERK_PAGES_RIGHT", new Vector2(937f, 512f));
			perkPageRight.symbolSprite.rotation = 90f;
			perkPageRight.size = new Vector2(45f, 45f);
			perkPageRight.roundedRect.size = perkPageRight.size;
			pages[0].subObjects.Add(perkPageRight);
		}
		if (burdenPages.Count > 1)
		{
			burdenPageLeft = new SymbolButton(this, pages[0], "Big_Menu_Arrow", "BURDEN_PAGES_LEFT", new Vector2(293f, 298f));
			burdenPageLeft.symbolSprite.rotation = 270f;
			burdenPageLeft.size = new Vector2(45f, 44f);
			burdenPageLeft.roundedRect.size = burdenPageLeft.size;
			pages[0].subObjects.Add(burdenPageLeft);
			burdenPageRight = new SymbolButton(this, pages[0], "Big_Menu_Arrow", "BURDEN_PAGES_RIGHT", new Vector2(1032f, 298f));
			burdenPageRight.symbolSprite.rotation = 90f;
			burdenPageRight.size = new Vector2(45f, 44f);
			burdenPageRight.roundedRect.size = burdenPageRight.size;
			pages[0].subObjects.Add(burdenPageRight);
		}
		SetupPerksPage(0);
		SetupBurdensPage(0);
		MenuLabel item2 = new MenuLabel(this, pages[0], Translate("Apply Burdens to increase both difficulty and the amount of points received"), new Vector2(683f, 380f), default(Vector2), bigText: true)
		{
			label = 
			{
				color = new Color(0.6f, 0.6f, 0.6f)
			}
		};
		pages[0].subObjects.Add(item2);
		UpdateBurdens();
		SetUpBurdenDescriptions();
		UpdateSelectables();
		opening = true;
		targetAlpha = 1f;
	}

	public void UpdateSelectables()
	{
		PerkPage perkPage = perkPages[currentPerkPage];
		List<PerkButton> list = perkButtons.Where((PerkButton x) => perkPage.PerkGroups.First().Perks.Contains(x.signalText)).ToList();
		List<PerkButton> list2 = perkButtons.Where((PerkButton x) => perkPage.PerkGroups.Last().Perks.Contains(x.signalText)).ToList();
		if (burdenPageLeft != null && burdenPageRight != null)
		{
			if (perkPageLeft != null && perkPageRight != null)
			{
				burdenPageLeft.nextSelectable[1] = perkPageLeft;
				burdenPageRight.nextSelectable[1] = perkPageRight;
			}
			else
			{
				burdenPageLeft.nextSelectable[1] = list2.Last();
				burdenPageRight.nextSelectable[1] = list2.Last();
			}
			burdenPageLeft.nextSelectable[3] = cancelButton;
			burdenPageRight.nextSelectable[3] = cancelButton;
		}
		if (perkPageLeft != null && perkPageRight != null)
		{
			if (burdenPageLeft != null && burdenPageRight != null)
			{
				perkPageLeft.nextSelectable[3] = burdenPageLeft;
				perkPageRight.nextSelectable[3] = burdenPageRight;
			}
			else
			{
				perkPageLeft.nextSelectable[3] = burdenButtons.Last();
				perkPageRight.nextSelectable[3] = burdenButtons.First();
			}
			perkPageLeft.nextSelectable[1] = cancelButton;
			perkPageRight.nextSelectable[1] = cancelButton;
			perkPageLeft.nextSelectable[2] = list.First();
			perkPageRight.nextSelectable[0] = list.Last();
		}
		foreach (PerkButton item in list)
		{
			item.nextSelectable[1] = cancelButton;
		}
		for (int i = 0; i < list2.Count; i++)
		{
			PerkButton perkButton = list2[i];
			if (i >= 0 && i <= 1)
			{
				perkButton.nextSelectable[3] = burdenButtons[Mathf.Min(0, burdenButtons.Count - 1)];
			}
			if (i >= 2 && i <= 3)
			{
				perkButton.nextSelectable[3] = burdenButtons[Mathf.Min(1, burdenButtons.Count - 1)];
			}
			if (i >= 4 && i <= 5)
			{
				perkButton.nextSelectable[3] = burdenButtons[Mathf.Min(2, burdenButtons.Count - 1)];
			}
			if (i >= 6 && i <= 7)
			{
				perkButton.nextSelectable[3] = burdenButtons[Mathf.Min(3, burdenButtons.Count - 1)];
			}
		}
		for (int j = 0; j < burdenButtons.Count; j++)
		{
			BigSimpleButton bigSimpleButton = burdenButtons[j];
			PerkButton perkButton2;
			switch (burdenButtons.Count)
			{
			case 1:
				perkButton2 = list2[Mathf.Min(3, list2.Count - 1)];
				break;
			case 2:
			{
				PerkButton perkButton3 = ((j != 0) ? list2[Mathf.Min(5, list2.Count - 1)] : list2[Mathf.Min(2, list2.Count - 1)]);
				perkButton2 = perkButton3;
				break;
			}
			case 3:
				perkButton2 = j switch
				{
					0 => list2[Mathf.Min(1, list2.Count - 1)], 
					1 => list2[Mathf.Min(3, list2.Count - 1)], 
					_ => list2[Mathf.Min(6, list2.Count - 1)], 
				};
				break;
			default:
				perkButton2 = j switch
				{
					0 => list2[Mathf.Min(0, list2.Count - 1)], 
					1 => list2[Mathf.Min(2, list2.Count - 1)], 
					2 => list2[Mathf.Min(5, list2.Count - 1)], 
					_ => list2[Mathf.Min(7, list2.Count - 1)], 
				};
				break;
			}
			bigSimpleButton.nextSelectable[1] = perkButton2;
			bigSimpleButton.nextSelectable[3] = cancelButton;
		}
		cancelButton.nextSelectable[0] = cancelButton;
		cancelButton.nextSelectable[2] = cancelButton;
		cancelButton.nextSelectable[3] = perkButtons.First();
		cancelButton.nextSelectable[1] = burdenButtons.First();
	}

	public void SetupPerksPage(int pageNumber)
	{
		currentPerkPage = pageNumber;
		PerkPage perkPage = perkPages[pageNumber];
		if (perkPageLeft != null)
		{
			perkPageLeft.buttonBehav.greyedOut = pageNumber <= 0;
		}
		if (perkPageRight != null)
		{
			perkPageRight.buttonBehav.greyedOut = pageNumber >= perkPages.Count - 1;
		}
		foreach (PerkButton perkButton2 in perkButtons)
		{
			perkButton2.RemoveSprites();
			pages[0].RemoveSubObject(perkButton2);
		}
		perkButtons.Clear();
		foreach (MenuLabel perkGroupLabel in perkGroupLabels)
		{
			perkGroupLabel.RemoveSprites();
			pages[0].RemoveSubObject(perkGroupLabel);
		}
		perkGroupLabels.Clear();
		for (int i = 0; i < perkPage.PerkGroups.Count; i++)
		{
			PerkGroup perkGroup = perkPage.PerkGroups[i];
			float num = 610f - 75f * (float)i;
			string text = perkGroup.GroupName;
			foreach (ModManager.Mod activeMod in ModManager.ActiveMods)
			{
				if (activeMod.id == perkGroup.GroupName)
				{
					text = activeMod.LocalizedName;
				}
			}
			MenuLabel menuLabel = new MenuLabel(this, pages[0], text, new Vector2(450.01f, num), default(Vector2), bigText: false);
			menuLabel.label.alignment = FLabelAlignment.Left;
			menuLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
			pages[0].subObjects.Add(menuLabel);
			perkGroupLabels.Add(menuLabel);
			for (int j = 0; j < perkGroup.Perks.Count; j++)
			{
				string text2 = perkGroup.Perks[j];
				PerkButton perkButton = new PerkButton(this, pages[0], new Vector2(450f + 60f * (float)j, num - 63f), new Vector2(50f, 50f), text2);
				perkButton.buttonBehav.greyedOut = !ExpeditionData.unlockables.Contains(perkButton.key);
				Vector3 vector = Custom.RGB2HSL(perkButton.unlockColor);
				perkButton.rectColor = (ExpeditionGame.activeUnlocks.Contains(text2) ? new HSLColor(vector.x, vector.y, vector.z) : new HSLColor(1f, 0f, 0.2f));
				pages[0].subObjects.Add(perkButton);
				perkButtons.Add(perkButton);
			}
		}
	}

	public void SetupBurdensPage(int pageNumber)
	{
		currentBurdenPage = pageNumber;
		List<string> list = burdenPages[pageNumber];
		if (burdenPageLeft != null)
		{
			burdenPageLeft.buttonBehav.greyedOut = pageNumber <= 0;
		}
		if (burdenPageRight != null)
		{
			burdenPageRight.buttonBehav.greyedOut = pageNumber >= burdenPages.Count - 1;
		}
		foreach (BigSimpleButton burdenButton in burdenButtons)
		{
			burdenButton.RemoveSprites();
			pages[0].RemoveSubObject(burdenButton);
		}
		burdenButtons.Clear();
		float num = (leftAnchor + rightAnchor) / 2f;
		float num2 = 170f;
		Vector2 vector = new Vector2(num - (float)list.Count * num2 / 2f + 12f, 310f);
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i];
			BigSimpleButton bigSimpleButton = new BigSimpleButton(this, pages[0], Translate(ExpeditionProgression.BurdenName(text)), text, vector + new Vector2(num2 * (float)i, -15f), new Vector2(150f, 50f), FLabelAlignment.Center, bigText: true);
			bigSimpleButton.buttonBehav.greyedOut = !ExpeditionData.unlockables.Contains(text);
			pages[0].subObjects.Add(bigSimpleButton);
			burdenButtons.Add(bigSimpleButton);
			switch (text)
			{
			case "bur-blinded":
				blindedBurden = bigSimpleButton;
				break;
			case "bur-doomed":
				doomedBurden = bigSimpleButton;
				break;
			case "bur-hunted":
				huntedBurden = bigSimpleButton;
				break;
			case "bur-pursued":
				pursuedBurden = bigSimpleButton;
				break;
			}
		}
		UpdateBurdens();
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (message == "CLOSE")
		{
			owner.unlocksButton.greyedOut = false;
			owner.startButton.greyedOut = false;
			closing = true;
			targetAlpha = 0f;
		}
		if (message == "PERK_PAGES_LEFT")
		{
			SetupPerksPage(currentPerkPage - 1);
			UpdateSelectables();
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		if (message == "PERK_PAGES_RIGHT")
		{
			SetupPerksPage(currentPerkPage + 1);
			UpdateSelectables();
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		if (message == "BURDEN_PAGES_LEFT")
		{
			SetupBurdensPage(currentBurdenPage - 1);
			UpdateSelectables();
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		if (message == "BURDEN_PAGES_RIGHT")
		{
			SetupBurdensPage(currentBurdenPage + 1);
			UpdateSelectables();
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		if (ExpeditionData.activeMission != "")
		{
			for (int i = 0; i < ExpeditionProgression.missionList.Count; i++)
			{
				if (ExpeditionProgression.missionList[i].key == ExpeditionData.activeMission && ExpeditionProgression.missionList[i].requirements.Contains(message))
				{
					PlaySound(SoundID.MENU_Error_Ping);
					return;
				}
			}
		}
		if (message.StartsWith("unl-"))
		{
			TogglePerk(message);
		}
		if (message.StartsWith("bur-"))
		{
			ToggleBurden(message);
		}
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
			owner.Container.AddChild(owner.levelSprite);
			owner.Container.AddChild(owner.levelSprite2);
			owner.Container.AddChild(owner.levelContainer);
			owner.Container.AddChild(owner.currentLevelLabel.label);
			owner.Container.AddChild(owner.nextLevelLabel.label);
			owner.Container.AddChild(owner.levelOverloadLabel.label);
			owner.Container.AddChild(owner.pointsLabel.label);
			owner.unlocksButton.Reset();
			pageTitle.RemoveFromContainer();
			manager.StopSideProcess(this);
			closing = false;
		}
		pageTitle.x = owner.page.pos.x + 685f;
		pageTitle.y = owner.page.pos.y + 680f;
		pageTitle.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 0.85f, darkSprite.alpha));
		perkFade -= 0.025f;
		perkLimit.label.color = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), new Color(0.8f, 0f, 0f), perkFade);
		perkFade = Mathf.Clamp(perkFade, 0f, 1f);
		if (perkButtons != null)
		{
			perkNameLabel.text = "";
			perkDescLabel.text = "";
			for (int i = 0; i < perkButtons.Count; i++)
			{
				if (perkButtons[i].Selected || perkButtons[i].IsMouseOverMe)
				{
					perkNameLabel.text = perkButtons[i].name;
					perkDescLabel.text = perkButtons[i].desc;
					break;
				}
			}
		}
		foreach (BigSimpleButton burdenButton in burdenButtons)
		{
			if (burdenButton.Selected || burdenButton.IsMouseOverMe)
			{
				perkNameLabel.text = ExpeditionProgression.BurdenName(burdenButton.signalText) + " +" + ExpeditionProgression.BurdenScoreMultiplier(burdenButton.signalText) + "%";
				perkDescLabel.text = (ExpeditionData.unlockables.Contains(burdenButton.signalText) ? ExpeditionProgression.BurdenDescription(burdenButton.signalText).WrapText(bigText: false, 600f) : "? ? ?");
			}
		}
		cancelButton.buttonBehav.greyedOut = opening;
	}

	public void SetUpBurdenDescriptions()
	{
		burdenNames = new List<string>();
		burdenDescriptions = new List<string>();
		burdenNames.Add(ExpeditionProgression.BurdenName("bur-blinded") + " +" + ExpeditionProgression.BurdenScoreMultiplier("bur-blinded") + "%");
		burdenNames.Add(ExpeditionProgression.BurdenName("bur-doomed") + " +" + ExpeditionProgression.BurdenScoreMultiplier("bur-doomed") + "%");
		burdenNames.Add(ExpeditionProgression.BurdenName("bur-hunted") + " +" + ExpeditionProgression.BurdenScoreMultiplier("bur-hunted") + "%");
		burdenNames.Add(ExpeditionProgression.BurdenName("bur-pursued") + " +" + ExpeditionProgression.BurdenScoreMultiplier("bur-pursued") + "%");
		burdenDescriptions.Add(ExpeditionData.unlockables.Contains("bur-blinded") ? ExpeditionProgression.BurdenDescription("bur-blinded").WrapText(bigText: false, 600f) : "? ? ?");
		burdenDescriptions.Add(ExpeditionData.unlockables.Contains("bur-doomed") ? ExpeditionProgression.BurdenDescription("bur-doomed").WrapText(bigText: false, 600f) : "? ? ?");
		burdenDescriptions.Add(ExpeditionData.unlockables.Contains("bur-hunted") ? ExpeditionProgression.BurdenDescription("bur-hunted").WrapText(bigText: false, 600f) : "? ? ?");
		burdenDescriptions.Add(ExpeditionData.unlockables.Contains("bur-pursued") ? ExpeditionProgression.BurdenDescription("bur-pursued").WrapText(bigText: false, 600f) : "? ? ?");
	}

	public void TogglePerk(string message)
	{
		SlugcatStats.Name slugcatPlayer = ExpeditionData.slugcatPlayer;
		int num = 0;
		for (int i = 0; i < ExpeditionGame.activeUnlocks.Count; i++)
		{
			if (ExpeditionGame.activeUnlocks[i].StartsWith("unl-"))
			{
				num++;
			}
		}
		switch (message)
		{
		case "unl-explosionimmunity":
		case "unl-explosivejump":
		case "unl-crafting":
			if (ModManager.MSC && slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				PlaySound(SoundID.MENU_Error_Ping);
				return;
			}
			break;
		}
		if (message == "unl-backspear" && slugcatPlayer == SlugcatStats.Name.Red)
		{
			PlaySound(SoundID.MENU_Error_Ping);
			return;
		}
		if (message == "unl-dualwield" && ModManager.MSC && slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			PlaySound(SoundID.MENU_Error_Ping);
			return;
		}
		if (message == "unl-agility" && ModManager.MSC && slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			PlaySound(SoundID.MENU_Error_Ping);
			return;
		}
		CustomPerk customPerk = CustomPerks.PerkForID(message);
		if (customPerk != null && !customPerk.AvailableForSlugcat(slugcatPlayer))
		{
			PlaySound(SoundID.MENU_Error_Ping);
			return;
		}
		if (message != "NULL")
		{
			if (!ExpeditionGame.activeUnlocks.Contains(message))
			{
				if (num < ExpeditionData.perkLimit)
				{
					ExpeditionGame.activeUnlocks.Add(message);
					num++;
				}
				else
				{
					PlaySound(SoundID.MENU_Error_Ping);
					perkFade = 1f;
				}
			}
			else
			{
				ExpeditionGame.activeUnlocks.Remove(message);
				num--;
			}
		}
		PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		perkLimit.label.text = Translate("PERKS: <current>/<limit>").Replace("<current>", ValueConverter.ConvertToString(num)).Replace("<limit>", ValueConverter.ConvertToString(ExpeditionData.perkLimit));
	}

	public void ToggleBurden(string message)
	{
		if (!ExpeditionGame.activeUnlocks.Contains(message))
		{
			ExpeditionGame.activeUnlocks.Add(message);
		}
		else
		{
			ExpeditionGame.activeUnlocks.Remove(message);
		}
		PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		UpdateBurdens();
	}

	public void UpdateBurdens()
	{
		foreach (BigSimpleButton burdenButton in burdenButtons)
		{
			if (ExpeditionGame.activeUnlocks.Contains(burdenButton.signalText))
			{
				Vector3 vector = Custom.RGB2HSL(ExpeditionProgression.BurdenMenuColor(burdenButton.signalText));
				burdenButton.labelColor = new HSLColor(vector.x, vector.y, vector.z);
			}
			else
			{
				burdenButton.labelColor = new HSLColor(1f, 0f, 0.35f);
			}
		}
		owner.UpdateChallengeButtons();
	}
}
