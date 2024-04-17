using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class DialogBackupSaveInfo : Dialog
{
	private SimpleButton slot1Button;

	private SimpleButton slot2Button;

	private SimpleButton slot3Button;

	private SimpleButton closeButton;

	private bool saveDataLoaded;

	private string backupFolder;

	public DialogBackupSaveInfo(Vector2 size, ProcessManager manager, string backupFolder)
		: base("", size, manager)
	{
		this.backupFolder = backupFolder;
		Initialize();
	}

	public override void Update()
	{
		base.Update();
		if (!saveDataLoaded && manager.rainWorld.progression.progressionLoaded)
		{
			PopulateSaveSlotInfoDisplay();
		}
	}

	public static float TotalButtonsWidth(InGameTranslator.LanguageID language)
	{
		float saveSlotButtonWidth = OptionsMenu.GetSaveSlotButtonWidth(language);
		float num = 10f;
		return 110f + saveSlotButtonWidth * 3f + num * 3f;
	}

	public void PopulateSaveSlotInfoDisplay()
	{
		saveDataLoaded = true;
		bool flag = false;
		string text = Translate("STORY DATA") + ":" + Environment.NewLine + Custom.ReplaceLineDelimeters(Translate("backup_info_disclaimer")) + Environment.NewLine + Environment.NewLine;
		foreach (string entry in ExtEnum<SlugcatStats.Name>.values.entries)
		{
			SlugcatStats.Name name = new SlugcatStats.Name(entry);
			if (SlugcatStats.HiddenOrUnplayableSlugcat(name))
			{
				continue;
			}
			SlugcatSelectMenu.SaveGameData saveGameData = SlugcatSelectMenu.MineForSaveData(manager, name);
			text = text + Translate(SlugcatStats.getSlugcatName(name)) + ": ";
			if (saveGameData == null)
			{
				text += Translate("No data");
			}
			else
			{
				int num = ((name == SlugcatStats.Name.Red) ? (RedsIllness.RedsCycles(saveGameData.redsExtraCycles) - saveGameData.cycle) : saveGameData.cycle);
				string s = Translate("Unknown Region");
				if (saveGameData.shelterName != null && saveGameData.shelterName.Length > 2)
				{
					s = Region.GetRegionFullName(saveGameData.shelterName.Substring(0, 2), name);
				}
				text = text + Translate("Cycle") + " " + num + ", " + Translate(s);
				if (ModManager.MMF)
				{
					TimeSpan timeSpan = TimeSpan.FromSeconds((double)saveGameData.gameTimeAlive + (double)saveGameData.gameTimeDead);
					text = text + " (" + SpeedRunTimer.TimeFormat(timeSpan) + ")";
				}
				flag = true;
			}
			text += Environment.NewLine;
		}
		if (!flag)
		{
			descriptionLabel.text = Translate("backup_info_no_data");
		}
		else
		{
			descriptionLabel.text = text;
		}
	}

	public void ClearSaveSlotInfoDisplay()
	{
		saveDataLoaded = false;
		descriptionLabel.text = Translate("backup_info_loading");
	}

	private void SwitchSaveSlotDisplay(int slotNum, bool forceLoad)
	{
		if (manager.rainWorld.options.saveSlot != slotNum || forceLoad)
		{
			manager.rainWorld.options.saveSlot = slotNum;
			manager.rainWorld.progression.Destroy();
			manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false, backupFolder);
			ClearSaveSlotInfoDisplay();
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		switch (message)
		{
		case "SLOT1":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			slot3Button.toggled = false;
			slot2Button.toggled = false;
			slot1Button.toggled = true;
			SwitchSaveSlotDisplay(0, forceLoad: false);
			break;
		case "SLOT2":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			slot3Button.toggled = false;
			slot2Button.toggled = true;
			slot1Button.toggled = false;
			SwitchSaveSlotDisplay(1, forceLoad: false);
			break;
		case "SLOT3":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			slot3Button.toggled = true;
			slot2Button.toggled = false;
			slot1Button.toggled = false;
			SwitchSaveSlotDisplay(2, forceLoad: false);
			break;
		case "CLOSE":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			manager.StopSideProcess(this);
			break;
		}
	}

	private void Initialize()
	{
		float saveSlotButtonWidth = OptionsMenu.GetSaveSlotButtonWidth(base.CurrLang);
		float num = 10f;
		float num2 = TotalButtonsWidth(base.CurrLang) / 2f;
		closeButton = new SimpleButton(this, pages[0], Translate("CLOSE"), "CLOSE", new Vector2(pos.x + (size.x * 0.5f - num2) + num * 3f + saveSlotButtonWidth * 3f, pos.y + Mathf.Max(size.y * 0.04f, 7f)), new Vector2(110f, 30f));
		slot3Button = new SimpleButton(this, pages[0], Translate("SAVE SLOT") + " 3", "SLOT3", new Vector2(pos.x + (size.x * 0.5f - num2) + num * 2f + saveSlotButtonWidth * 2f, pos.y + Mathf.Max(size.y * 0.04f, 7f)), new Vector2(saveSlotButtonWidth, 30f));
		if (manager.rainWorld.options.saveSlot == 2)
		{
			slot3Button.toggled = true;
		}
		slot2Button = new SimpleButton(this, pages[0], Translate("SAVE SLOT") + " 2", "SLOT2", new Vector2(pos.x + (size.x * 0.5f - num2) + num + saveSlotButtonWidth, pos.y + Mathf.Max(size.y * 0.04f, 7f)), new Vector2(saveSlotButtonWidth, 30f));
		if (manager.rainWorld.options.saveSlot == 1)
		{
			slot2Button.toggled = true;
		}
		slot1Button = new SimpleButton(this, pages[0], Translate("SAVE SLOT") + " 1", "SLOT1", new Vector2(pos.x + (size.x * 0.5f - num2), pos.y + Mathf.Max(size.y * 0.04f, 7f)), new Vector2(saveSlotButtonWidth, 30f));
		if (manager.rainWorld.options.saveSlot == 0)
		{
			slot1Button.toggled = true;
		}
		pages[0].subObjects.Add(slot1Button);
		pages[0].subObjects.Add(slot2Button);
		pages[0].subObjects.Add(slot3Button);
		pages[0].subObjects.Add(closeButton);
		SwitchSaveSlotDisplay(manager.rainWorld.options.saveSlot, forceLoad: true);
	}
}
