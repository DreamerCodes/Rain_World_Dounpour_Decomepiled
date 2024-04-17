using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RWCustom;
using UnityEngine;

namespace Menu;

public class BackupManager : Menu
{
	private bool lastPauseButton;

	private SimpleButton backButton;

	private SimpleButton nextPageButton;

	private SimpleButton prevPageButton;

	private SimpleButton createButton;

	private SimpleButton deleteButton;

	private SimpleButton restoreButton;

	private SimpleButton infoButton;

	private SimpleButton[] backupButtons;

	private RoundedRect backupsBorderRect;

	private FSprite backupsBackgroundRect;

	private int pageNum;

	private List<string> backupDirectories = new List<string>();

	private int selectedBackup = -1;

	private FSprite darkSprite;

	private string pendingFollowupConfirmationMessage;

	private bool backupCurrent;

	private int originalSaveSlot;

	private bool infoViewed;

	private bool ProgressionBusy => !manager.rainWorld.progression.progressionLoaded;

	public int ButtonsPerPage => 60;

	public int TotalPages => Mathf.CeilToInt((float)backupDirectories.Count / (float)ButtonsPerPage);

	public override bool ForceNoMouseMode => false;

	public BackupManager(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.BackupManager)
	{
		pages.Add(new Page(this, null, "main", 0));
		scene = new InteractiveMenuScene(this, pages[0], manager.rainWorld.options.subBackground);
		pages[0].subObjects.Add(scene);
		darkSprite = new FSprite("pixel");
		darkSprite.color = new Color(0f, 0f, 0f);
		darkSprite.anchorX = 0f;
		darkSprite.anchorY = 0f;
		darkSprite.scaleX = 1368f;
		darkSprite.scaleY = 770f;
		darkSprite.x = -1f;
		darkSprite.y = -1f;
		darkSprite.alpha = 0.85f;
		pages[0].Container.AddChild(darkSprite);
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
		float num = 1366f;
		deleteButton = new SimpleButton(this, pages[0], Translate("DELETE"), "DELETE", new Vector2(num - 200f - 110f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(deleteButton);
		createButton = new SimpleButton(this, pages[0], Translate("CREATE"), "CREATE", new Vector2(num - 200f - 110f - 120f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(createButton);
		restoreButton = new SimpleButton(this, pages[0], Translate("RESTORE"), "RESTORE", new Vector2(num - 200f - 110f - 240f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(restoreButton);
		infoButton = new SimpleButton(this, pages[0], Translate("INFO"), "INFO", new Vector2(num - 200f - 110f - 360f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(infoButton);
		backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(200f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(backButton);
		backObject = backButton;
		prevPageButton = new SimpleButton(this, pages[0], Translate("PREVIOUS"), "PREV", new Vector2(388f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(prevPageButton);
		nextPageButton = new SimpleButton(this, pages[0], Translate("NEXT"), "NEXT", new Vector2(508f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(nextPageButton);
		backupsBorderRect = new RoundedRect(this, pages[0], new Vector2(200f, 163f), new Vector2(966f, 488f), filled: true);
		backupsBackgroundRect = new FSprite("pixel");
		backupsBackgroundRect.color = new Color(0f, 0f, 0f);
		backupsBackgroundRect.anchorX = 0f;
		backupsBackgroundRect.anchorY = 0f;
		backupsBackgroundRect.scaleX = backupsBorderRect.size.x - 12f;
		backupsBackgroundRect.scaleY = backupsBorderRect.size.y - 12f;
		backupsBackgroundRect.x = backupsBorderRect.pos.x + 6f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
		backupsBackgroundRect.y = backupsBorderRect.pos.y + 6f;
		backupsBackgroundRect.alpha = 0.65f;
		pages[0].Container.AddChild(backupsBackgroundRect);
		pages[0].subObjects.Add(backupsBorderRect);
		originalSaveSlot = manager.rainWorld.options.saveSlot;
		PopulateBackups();
		PopulateButtons();
	}

	public string BackupFolderFriendlyName(string backupFolderPath)
	{
		string fileName = Path.GetFileName(backupFolderPath);
		string[] array = fileName.Split('_');
		string text = array[1].Replace("-", "/") + " " + array[2].Replace("-", ":");
		if (!fileName.Contains("_USR"))
		{
			text = text + " (" + Translate("AUTO") + ")";
		}
		return text;
	}

	public override string UpdateInfoText()
	{
		if (selectedObject == backButton)
		{
			return Translate("Back to options");
		}
		if (nextPageButton != null && selectedObject == nextPageButton)
		{
			return Translate("Next Page");
		}
		if (prevPageButton != null && selectedObject == prevPageButton)
		{
			return Translate("Previous Page");
		}
		if (selectedObject == createButton)
		{
			return Translate("backup_create_description");
		}
		if (selectedObject == deleteButton)
		{
			return Translate("backup_delete_description");
		}
		if (selectedObject == restoreButton)
		{
			return Translate("backup_restore_description");
		}
		if (selectedObject == infoButton)
		{
			return Translate("backup_info_description");
		}
		return base.UpdateInfoText();
	}

	public override void Update()
	{
		base.Update();
		bool flag = RWInput.CheckPauseButton(0);
		if (flag && !lastPauseButton && manager.dialog == null && !backButton.buttonBehav.greyedOut)
		{
			Singal(backButton, backButton.signalText);
		}
		createButton.buttonBehav.greyedOut = backupCurrent || ProgressionBusy;
		restoreButton.buttonBehav.greyedOut = selectedBackup == -1 || ProgressionBusy;
		deleteButton.buttonBehav.greyedOut = selectedBackup == -1 || ProgressionBusy;
		infoButton.buttonBehav.greyedOut = selectedBackup == -1 || ProgressionBusy;
		nextPageButton.buttonBehav.greyedOut = pageNum >= TotalPages - 1;
		prevPageButton.buttonBehav.greyedOut = pageNum == 0;
		backButton.buttonBehav.greyedOut = ProgressionBusy;
		lastPauseButton = flag;
		if (pendingFollowupConfirmationMessage != null)
		{
			Vector2 size = DialogBoxNotify.CalculateDialogBoxSize(pendingFollowupConfirmationMessage);
			DialogNotify dialog = new DialogNotify(Custom.ReplaceLineDelimeters(pendingFollowupConfirmationMessage), size, manager, delegate
			{
			});
			manager.ShowDialog(dialog);
			pendingFollowupConfirmationMessage = null;
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		switch (message)
		{
		case "BACK":
			if (manager.rainWorld.options.saveSlot != originalSaveSlot || infoViewed)
			{
				manager.rainWorld.options.saveSlot = originalSaveSlot;
				manager.rainWorld.progression.Destroy();
				manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
			}
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
			PlaySound(SoundID.MENU_Switch_Page_Out);
			return;
		case "NEXT":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			pageNum++;
			if (pageNum >= TotalPages)
			{
				pageNum = 0;
			}
			PopulateButtons();
			return;
		case "PREV":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			pageNum--;
			if (pageNum < 0)
			{
				pageNum = TotalPages - 1;
			}
			PopulateButtons();
			return;
		case "CREATE":
		{
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			manager.rainWorld.progression.CreateCopyOfSaves(userCreated: true);
			pageNum = 0;
			selectedBackup = 0;
			backupCurrent = true;
			PopulateBackups();
			PopulateButtons();
			string text = Translate("backup_create_success").Replace("{X}", BackupFolderFriendlyName(backupDirectories[selectedBackup]));
			DialogNotify dialog3 = new DialogNotify(size: DialogBoxNotify.CalculateDialogBoxSize(text), description: Custom.ReplaceLineDelimeters(text), manager: manager, onOK: delegate
			{
			});
			manager.ShowDialog(dialog3);
			return;
		}
		case "DELETE":
		{
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			DialogConfirm dialog4 = new DialogConfirm(Custom.ReplaceLineDelimeters(Translate("backup_delete_confirm").Replace("{X}", BackupFolderFriendlyName(backupDirectories[selectedBackup]))), new Vector2(1024.5f, 268.8f), manager, delegate
			{
				string text2 = backupDirectories[selectedBackup];
				bool flag = false;
				try
				{
					string[] files = Directory.GetFiles(text2);
					for (int j = 0; j < files.Length; j++)
					{
						File.Delete(files[j]);
					}
					Directory.Delete(text2);
					flag = true;
				}
				catch (Exception ex)
				{
					Custom.LogWarning("Failed to delete old save file backup", text2, "::", ex.Message);
				}
				PopulateBackups();
				if (selectedBackup >= backupDirectories.Count)
				{
					selectedBackup = backupDirectories.Count - 1;
				}
				while (pageNum >= TotalPages && pageNum > 0)
				{
					pageNum--;
				}
				PopulateButtons();
				if (flag)
				{
					string text3 = Translate("backup_delete_success");
					text3 = text3.Replace("{X}", BackupFolderFriendlyName(text2));
					pendingFollowupConfirmationMessage = text3;
				}
			}, delegate
			{
			});
			manager.ShowDialog(dialog4);
			return;
		}
		case "RESTORE":
		{
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			DialogConfirm dialog2 = new DialogConfirm(Custom.ReplaceLineDelimeters(Translate("backup_restore_confirm").Replace("{X}", BackupFolderFriendlyName(backupDirectories[selectedBackup]))), new Vector2(1024.5f, 268.8f), manager, delegate
			{
				backupCurrent = true;
				manager.rainWorld.progression.Destroy();
				RestoreSaveFile("sav");
				RestoreSaveFile("sav2");
				RestoreSaveFile("sav3");
				RestoreSaveFile("expCore");
				RestoreSaveFile("expCore1");
				RestoreSaveFile("expCore2");
				RestoreSaveFile("expCore3");
				RestoreSaveFile("exp");
				RestoreSaveFile("exp1");
				manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
				infoViewed = false;
				PopulateBackups();
				PopulateButtons();
				string text4 = Translate("backup_restore_success");
				text4 = text4.Replace("{X}", BackupFolderFriendlyName(backupDirectories[selectedBackup]));
				pendingFollowupConfirmationMessage = text4;
			}, delegate
			{
			});
			manager.ShowDialog(dialog2);
			return;
		}
		case "INFO":
		{
			infoViewed = true;
			DialogBackupSaveInfo dialog = new DialogBackupSaveInfo(new Vector2(Mathf.Max(DialogBackupSaveInfo.TotalButtonsWidth(base.CurrLang) + 40f, 700f), 500f), manager, backupDirectories[selectedBackup]);
			manager.ShowDialog(dialog);
			return;
		}
		}
		if (!message.Contains("BACKUP"))
		{
			return;
		}
		PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		int num = (selectedBackup = int.Parse(message.Substring("BACKUP".Length), NumberStyles.Any, CultureInfo.InvariantCulture));
		int num2 = ButtonsPerPage * pageNum;
		for (int i = 0; i < backupButtons.Length; i++)
		{
			if (i == num - num2)
			{
				backupButtons[i].toggled = true;
			}
			else
			{
				backupButtons[i].toggled = false;
			}
		}
	}

	private void RestoreSaveFile(string sourceName)
	{
		if (File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + sourceName))
		{
			File.Delete(Application.persistentDataPath + Path.DirectorySeparatorChar + sourceName);
		}
		if (File.Exists(backupDirectories[selectedBackup] + Path.DirectorySeparatorChar + sourceName))
		{
			File.Copy(backupDirectories[selectedBackup] + Path.DirectorySeparatorChar + sourceName, Application.persistentDataPath + Path.DirectorySeparatorChar + sourceName);
		}
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		backupsBackgroundRect.RemoveFromContainer();
		darkSprite.RemoveFromContainer();
	}

	public int ButtonsOnPage(int currentPageNum)
	{
		if (currentPageNum == 0)
		{
			return Math.Min(ButtonsPerPage, backupDirectories.Count);
		}
		if (currentPageNum == TotalPages - 1)
		{
			return (backupDirectories.Count - ButtonsPerPage) % ButtonsPerPage;
		}
		return ButtonsPerPage;
	}

	public void PopulateBackups()
	{
		backupDirectories.Clear();
		string path = Application.persistentDataPath + Path.DirectorySeparatorChar + "backup";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] directories = Directory.GetDirectories(path);
		foreach (string text in directories)
		{
			if (Path.GetFileName(text).Split('_').Length >= 3)
			{
				backupDirectories.Add(text);
			}
		}
		backupDirectories.Sort();
		backupDirectories.Reverse();
	}

	public void PopulateButtons()
	{
		if (backupButtons != null)
		{
			for (int i = 0; i < backupButtons.Length; i++)
			{
				backupButtons[i].RemoveSprites();
				pages[0].RemoveSubObject(backupButtons[i]);
			}
			backupButtons = null;
		}
		backupButtons = new SimpleButton[ButtonsOnPage(pageNum)];
		int num = ButtonsPerPage * pageNum;
		float num2 = 615f;
		float num3 = 170f;
		float num4 = 30f;
		float num5 = 8f;
		int num6 = 5;
		for (int j = 0; j < ButtonsOnPage(pageNum); j++)
		{
			Vector2 pos = new Vector2(242f + (float)(j % num6) * (num3 + num5), num2 - num4 * 0.5f - (num4 + num5) * (float)(j / num6));
			int num7 = j + num;
			backupButtons[j] = new SimpleButton(this, pages[0], BackupFolderFriendlyName(backupDirectories[num7]), "BACKUP" + num7, pos, new Vector2(num3, num4));
			backupButtons[j].toggled = selectedBackup == num7;
			pages[0].subObjects.Add(backupButtons[j]);
		}
	}
}
