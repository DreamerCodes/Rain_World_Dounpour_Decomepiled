using System;
using System.Collections.Generic;
using System.Globalization;
using Menu.Remix.MixedUI;
using MoreSlugcats;
using Music;
using RWCustom;
using UnityEngine;

namespace Menu;

public class OptionsMenu : Menu, SelectOneButton.SelectOneButtonOwner, CheckBox.IOwnCheckBox
{
	private bool resolutionDirty;

	private bool placementDirty;

	private bool languageDirty;

	public SimpleButton backButton;

	public SimpleButton creditsButton;

	public SimpleButton saveBackupsButton;

	public SelectOneButton[] screenResolutionButtons;

	public SelectOneButton[] languageButtons;

	public SelectOneButton[] saveSlotButtons;

	public CustomMessageButton fullScreenButton;

	public HoldButton wipeSaveButton;

	public ControlsButton controlsButton;

	public BackgroundsButton backgroundsButton;

	public MenuLabel resetWarningText;

	public CheckBox validationCheckbox;

	public CheckBox enableStatsCheckbox;

	public int resetWarningTextCounter;

	public float resetWarningTextAlpha;

	private int wrongFullscreenSettingCounter;

	public Slider musicSlider;

	public Slider soundSlider;

	public Slider arenaMusicSlider;

	private FSprite darkSprite;

	public bool consoleMenu;

	private bool lastPauseButton;

	private bool exiting;

	private bool waitingOnProgressionLoaded;

	private int leavingSaveSlot;

	private bool checkingForBackup;

	private bool checkedForBackup;

	private bool backupExists;

	private bool startedBackupRestore;

	private bool waitingForBackupRestore;

	private bool backupRestoreSuccess;

	private bool reportCorruptedDialogDisplaying;

	private bool waitingToRecreateFromBackup;

	public CheckBox commentaryCheckbox;

	public HorizontalSlider fpsSlider;

	public HorizontalSlider analogSlider;

	public SelectOneButton[] qualityButtons;

	public bool qualityDirty;

	public List<CheckBox> miscCheckboxes;

	private bool ProgressionBusy
	{
		get
		{
			if (!waitingOnProgressionLoaded)
			{
				return !manager.rainWorld.progression.progressionLoaded;
			}
			return true;
		}
	}

	public OptionsMenu(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.OptionsMenu)
	{
		pages.Add(new Page(this, null, "main", 0));
		consoleMenu = false;
		float num = 0f;
		Vector2 vector = new Vector2(200f, 620f + num);
		Vector2 zero = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		Vector2 zero3 = Vector2.zero;
		Vector2 zero4 = Vector2.zero;
		Vector2 zero5 = Vector2.zero;
		Vector2 zero6 = Vector2.zero;
		float num2 = 0f;
		scene = new InteractiveMenuScene(this, pages[0], ModManager.MMF ? manager.rainWorld.options.subBackground : MenuScene.SceneID.Landscape_SU);
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
		zero = new Vector2(0f, 100f + num);
		vector2 = new Vector2(ModManager.MMF ? (-60f) : 0f, 0f);
		float saveSlotButtonWidth = GetSaveSlotButtonWidth(base.CurrLang);
		float warningTextXOffset = GetWarningTextXOffset(base.CurrLang);
		LabelTest.Initialize(this);
		if (!consoleMenu)
		{
			pages[0].subObjects.Add(new MenuLabel(this, pages[0], Translate("SCREEN"), vector + new Vector2(20f, 40f), new Vector2(100f, 30f), bigText: false));
			string text = (manager.rainWorld.options.fullScreen ? "WINDOWED" : "FULLSCREEN");
			fullScreenButton = new CustomMessageButton(this, pages[0], Translate(text), text, vector, new Vector2(150f, 30f), "Toggle Fullscreen");
			pages[0].subObjects.Add(fullScreenButton);
			screenResolutionButtons = new SelectOneButton[Options.screenResolutions.Length];
			for (int i = 0; i < Options.screenResolutions.Length; i++)
			{
				text = ResolutionString(Options.screenResolutions[i], Options.aspectRatioStrings[i]);
				screenResolutionButtons[i] = new SelectOneButton(this, pages[0], text, "ScreenRes", vector - new Vector2(0f, 40f * (float)(i + 1)), new Vector2(150f, 30f), screenResolutionButtons, i);
				pages[0].subObjects.Add(screenResolutionButtons[i]);
			}
		}
		else
		{
			fullScreenButton = null;
		}
		float x = 503f + zero.x;
		if (consoleMenu)
		{
			x = 270f + saveSlotButtonWidth + 20f;
		}
		wipeSaveButton = new HoldButton(this, pages[0], Translate("RESET PROGRESS").Replace("<LINE>", "\r\n"), "RESET PROGRESS", new Vector2(x, 270f + zero.y) + new Vector2(consoleMenu ? 0f : (221f + (saveSlotButtonWidth - 100f) / 2f), 207f), 400f);
		pages[0].subObjects.Add(wipeSaveButton);
		string text2 = Translate("WARNING!<LINE>This will reset all progress in the selected save slot,<LINE>including map exploration. Unlocked arenas and<LINE>sandbox items are retained.");
		text2 = text2.Replace("<LINE>", "\r\n");
		resetWarningText = new MenuLabel(this, pages[0], text2, wipeSaveButton.pos + new Vector2(warningTextXOffset, -140f) + vector2, new Vector2(200f, 30f), bigText: false);
		if (ModManager.MMF || consoleMenu)
		{
			resetWarningText.label.alignment = FLabelAlignment.Left;
		}
		pages[0].subObjects.Add(resetWarningText);
		backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(200f, 50f), new Vector2(110f, 30f));
		creditsButton = new SimpleButton(this, pages[0], Translate("CREDITS"), "CREDITS", new Vector2(1056f, 50f) + zero5, new Vector2(110f, 30f));
		pages[0].subObjects.Add(backButton);
		pages[0].subObjects.Add(creditsButton);
		backObject = backButton;
		saveSlotButtons = new SelectOneButton[3];
		Vector2 vector3 = wipeSaveButton.pos + new Vector2(-200f, 40f * (float)(saveSlotButtons.Length - 1) / 2f - 15f);
		languageButtons = new SelectOneButton[ExtEnum<InGameTranslator.LanguageID>.values.Count];
		Vector2 vector4 = new Vector2(856f, vector.y);
		pages[0].subObjects.Add(new MenuLabel(this, pages[0], Translate("LANGUAGE"), vector4 + new Vector2(0f, 40f), new Vector2(310f, 30f), bigText: false));
		for (int j = 0; j < languageButtons.Length; j++)
		{
			languageButtons[j] = new SelectOneButton(this, pages[0], Translate(ExtEnum<InGameTranslator.LanguageID>.values.GetEntry(j).ToUpper()), "Language", vector4 + new Vector2((float)(j % 3) * 110f, (float)(j / 3) * -40f), new Vector2(100f, 30f), languageButtons, j);
			pages[0].subObjects.Add(languageButtons[j]);
		}
		pages[0].subObjects.Add(new MenuLabel(this, pages[0], Translate("SAVE FILES"), vector3 + new Vector2(65f - (saveSlotButtonWidth - 100f) / 2f, 58f), new Vector2(122f, 30f), bigText: false));
		vector3.x -= saveSlotButtonWidth - 100f;
		for (int k = 0; k < saveSlotButtons.Length; k++)
		{
			saveSlotButtons[k] = new SelectOneButton(this, pages[0], Translate("SAVE SLOT") + " " + (k + 1), "SaveSlot", vector3 + new Vector2(0f, (float)(-k) * 40f), new Vector2(saveSlotButtonWidth, 30f), saveSlotButtons, k);
			pages[0].subObjects.Add(saveSlotButtons[k]);
		}
		if (!consoleMenu)
		{
			saveBackupsButton = new SimpleButton(this, pages[0], Translate("BACKUPS"), "BACKUPS", vector3 + new Vector2(0f, (float)(-saveSlotButtons.Length) * 40f), new Vector2(saveSlotButtonWidth, 30f));
			pages[0].subObjects.Add(saveBackupsButton);
		}
		musicSlider = new HorizontalSlider(this, pages[0], Translate("Music"), new Vector2(192f, 235f) + zero2, new Vector2(340f + num2, 30f), Slider.SliderID.MusicVol, subtleSlider: false);
		pages[0].subObjects.Add(musicSlider);
		arenaMusicSlider = new HorizontalSlider(this, pages[0], Translate("Arena Music"), new Vector2(192f, 195f) + zero2, new Vector2(340f + num2, 30f), Slider.SliderID.ArenaMusicVolume, subtleSlider: false);
		pages[0].subObjects.Add(arenaMusicSlider);
		soundSlider = new HorizontalSlider(this, pages[0], Translate("Sound Effects"), new Vector2(192f, 155f) + zero2, new Vector2(340f + num2, 30f), Slider.SliderID.SfxVol, subtleSlider: false);
		pages[0].subObjects.Add(soundSlider);
		controlsButton = null;
		if (!consoleMenu)
		{
			controlsButton = new ControlsButton(this, pages[0], new Vector2(1056f, 169f), Translate("Input Settings"));
			pages[0].subObjects.Add(controlsButton);
		}
		backgroundsButton = null;
		if (ModManager.MMF)
		{
			backgroundsButton = new BackgroundsButton(this, pages[0], new Vector2((controlsButton == null) ? 1056f : 936f, 169f) + zero6, Translate("Backgrounds"));
			pages[0].subObjects.Add(backgroundsButton);
		}
		Vector2 vector5 = musicSlider.pos + new Vector2(0f, 60f) + zero3;
		CheckBox checkBox = new CheckBox(this, pages[0], this, new Vector2(192f + (consoleMenu ? 0f : 340f), vector5.y + ((!ModManager.MMF) ? 0f : (consoleMenu ? 60f : 120f))) + zero4, 65f, Translate("vibration"), "VIBRATION", textOnRight: true);
		pages[0].subObjects.Add(checkBox);
		CheckBox checkBox2 = null;
		if (ModManager.MMF && !consoleMenu)
		{
			checkBox2 = new CheckBox(this, pages[0], this, new Vector2(432f, vector5.y + ((!ModManager.MMF) ? 0f : 120f)), 65f, "VSync", "VSYNC", textOnRight: true);
			pages[0].subObjects.Add(checkBox2);
		}
		InitMiscCheckboxes();
		float num3 = 0f;
		for (int l = 0; l < miscCheckboxes.Count; l++)
		{
			float width = miscCheckboxes[l].label.label.textRect.width;
			if (width > num3)
			{
				num3 = width;
			}
		}
		for (int m = 0; m < miscCheckboxes.Count; m++)
		{
			miscCheckboxes[m].pos = new Vector2(1126f - num3, 259f + (float)m * 35f);
			pages[0].subObjects.Add(miscCheckboxes[m]);
		}
		if (ModManager.MMF)
		{
			analogSlider = new HorizontalSlider(this, pages[0], Translate("Analog Sensitivity"), vector5 + new Vector2(0f, 80f), new Vector2(340f + num2, 30f), MMFEnums.SliderID.AnalogSensitivity, subtleSlider: false);
			pages[0].subObjects.Add(analogSlider);
			if (!consoleMenu)
			{
				fpsSlider = new HorizontalSlider(this, pages[0], Translate("FPS Limit"), vector5 + new Vector2(0f, 40f), new Vector2(340f, 30f), MMFEnums.SliderID.FPSLimit, subtleSlider: false);
				pages[0].subObjects.Add(fpsSlider);
				if (manager.rainWorld.options.vsync)
				{
					fpsSlider.buttonBehav.greyedOut = true;
				}
				qualityButtons = new SelectOneButton[ExtEnum<Options.Quality>.values.Count];
				pages[0].subObjects.Add(new MenuLabel(this, pages[0], Translate("Quality"), vector5 + new Vector2(340f, 0f), new Vector2(120f, 30f), bigText: false));
				for (int n = 0; n < qualityButtons.Length; n++)
				{
					qualityButtons[n] = new SelectOneButton(this, pages[0], Translate(ExtEnum<Options.Quality>.values.GetEntry(n)), "Quality", vector5 + new Vector2(125f * (float)n, 0f), new Vector2(100f, 30f), qualityButtons, n);
					pages[0].subObjects.Add(qualityButtons[n]);
				}
			}
		}
		if (fullScreenButton != null)
		{
			fullScreenButton.nextSelectable[0] = fullScreenButton;
			for (int num4 = 0; num4 < screenResolutionButtons.Length; num4++)
			{
				screenResolutionButtons[num4].nextSelectable[0] = screenResolutionButtons[num4];
			}
		}
		if (checkBox != null)
		{
			if (screenResolutionButtons != null)
			{
				checkBox.nextSelectable[0] = screenResolutionButtons[screenResolutionButtons.Length - 1];
			}
			else
			{
				checkBox.nextSelectable[0] = checkBox;
			}
		}
		if (checkBox2 != null)
		{
			checkBox.nextSelectable[0] = checkBox2;
			checkBox2.nextSelectable[0] = screenResolutionButtons[screenResolutionButtons.Length - 1];
		}
		if (qualityButtons != null)
		{
			for (int num5 = 0; num5 < qualityButtons.Length; num5++)
			{
				if (num5 == 0)
				{
					qualityButtons[num5].nextSelectable[0] = qualityButtons[num5];
				}
				else
				{
					qualityButtons[num5].nextSelectable[0] = qualityButtons[num5 - 1];
				}
			}
		}
		backButton.nextSelectable[0] = backButton;
		for (int num6 = 0; num6 < saveSlotButtons.Length; num6++)
		{
			if (fullScreenButton != null)
			{
				saveSlotButtons[num6].nextSelectable[0] = fullScreenButton;
			}
			else
			{
				saveSlotButtons[num6].nextSelectable[0] = saveSlotButtons[num6];
			}
		}
		if (saveBackupsButton != null)
		{
			if (fullScreenButton != null)
			{
				saveBackupsButton.nextSelectable[0] = fullScreenButton;
			}
			else
			{
				saveBackupsButton.nextSelectable[0] = saveBackupsButton;
			}
		}
		wipeSaveButton.nextSelectable[0] = saveSlotButtons[0];
		if (languageButtons != null)
		{
			for (int num7 = 0; num7 < languageButtons.Length; num7++)
			{
				if (num7 % 3 == 0)
				{
					languageButtons[num7].nextSelectable[0] = wipeSaveButton;
				}
				else
				{
					languageButtons[num7].nextSelectable[0] = languageButtons[num7 - 1];
				}
			}
		}
		for (int num8 = 0; num8 < miscCheckboxes.Count; num8++)
		{
			if (consoleMenu)
			{
				miscCheckboxes[num8].nextSelectable[0] = checkBox;
			}
			else if (qualityButtons != null)
			{
				miscCheckboxes[num8].nextSelectable[0] = qualityButtons[qualityButtons.Length - 1];
			}
			else
			{
				miscCheckboxes[num8].nextSelectable[0] = musicSlider;
			}
		}
		if (backgroundsButton != null)
		{
			backgroundsButton.nextSelectable[0] = musicSlider;
		}
		if (controlsButton != null)
		{
			if (backgroundsButton != null)
			{
				controlsButton.nextSelectable[0] = backgroundsButton;
			}
			else
			{
				controlsButton.nextSelectable[0] = musicSlider;
			}
		}
		creditsButton.nextSelectable[0] = backButton;
		if (fullScreenButton != null)
		{
			fullScreenButton.nextSelectable[2] = saveSlotButtons[0];
			for (int num9 = 0; num9 < screenResolutionButtons.Length; num9++)
			{
				screenResolutionButtons[num9].nextSelectable[2] = saveSlotButtons[0];
				if (num9 >= screenResolutionButtons.Length - 2)
				{
					if (checkBox2 != null)
					{
						screenResolutionButtons[num9].nextSelectable[2] = checkBox2;
					}
					else if (checkBox != null)
					{
						screenResolutionButtons[num9].nextSelectable[2] = checkBox;
					}
				}
			}
		}
		if (checkBox2 != null)
		{
			checkBox2.nextSelectable[2] = checkBox;
		}
		if (miscCheckboxes.Count > 0)
		{
			checkBox.nextSelectable[2] = miscCheckboxes[miscCheckboxes.Count - 1];
		}
		else if (backgroundsButton != null && !consoleMenu)
		{
			checkBox.nextSelectable[2] = backgroundsButton;
		}
		else if (controlsButton != null)
		{
			checkBox.nextSelectable[2] = controlsButton;
		}
		else
		{
			checkBox.nextSelectable[2] = saveSlotButtons[0];
		}
		if (qualityButtons != null)
		{
			for (int num10 = 0; num10 < qualityButtons.Length; num10++)
			{
				if (num10 == qualityButtons.Length - 1)
				{
					if (miscCheckboxes.Count > 0)
					{
						qualityButtons[num10].nextSelectable[2] = miscCheckboxes[0];
					}
					else if (backgroundsButton != null)
					{
						qualityButtons[num10].nextSelectable[2] = backgroundsButton;
					}
					else if (controlsButton != null)
					{
						qualityButtons[num10].nextSelectable[2] = controlsButton;
					}
				}
				else
				{
					qualityButtons[num10].nextSelectable[2] = qualityButtons[num10 + 1];
				}
			}
		}
		backButton.nextSelectable[2] = creditsButton;
		for (int num11 = 0; num11 < saveSlotButtons.Length; num11++)
		{
			saveSlotButtons[num11].nextSelectable[2] = wipeSaveButton;
		}
		if (saveBackupsButton != null)
		{
			saveBackupsButton.nextSelectable[2] = wipeSaveButton;
		}
		if (languageButtons != null)
		{
			wipeSaveButton.nextSelectable[2] = languageButtons[0];
		}
		else
		{
			wipeSaveButton.nextSelectable[2] = wipeSaveButton;
		}
		if (languageButtons != null)
		{
			for (int num12 = 0; num12 < languageButtons.Length; num12++)
			{
				if (num12 % 3 == 2 || num12 == languageButtons.Length - 1)
				{
					languageButtons[num12].nextSelectable[2] = languageButtons[num12];
				}
				else
				{
					languageButtons[num12].nextSelectable[2] = languageButtons[num12 + 1];
				}
			}
		}
		for (int num13 = 0; num13 < miscCheckboxes.Count; num13++)
		{
			miscCheckboxes[num13].nextSelectable[2] = miscCheckboxes[num13];
		}
		if (backgroundsButton != null)
		{
			if (controlsButton != null)
			{
				backgroundsButton.nextSelectable[2] = controlsButton;
			}
			else
			{
				backgroundsButton.nextSelectable[2] = backgroundsButton;
			}
		}
		if (controlsButton != null)
		{
			controlsButton.nextSelectable[2] = controlsButton;
		}
		creditsButton.nextSelectable[2] = creditsButton;
		if (fullScreenButton != null)
		{
			fullScreenButton.nextSelectable[1] = fullScreenButton;
			for (int num14 = 0; num14 < screenResolutionButtons.Length; num14++)
			{
				if (num14 == 0)
				{
					screenResolutionButtons[num14].nextSelectable[1] = fullScreenButton;
				}
				else
				{
					screenResolutionButtons[num14].nextSelectable[1] = screenResolutionButtons[num14 - 1];
				}
			}
		}
		if (saveBackupsButton != null)
		{
			checkBox.nextSelectable[1] = saveBackupsButton;
			if (checkBox2 != null)
			{
				checkBox2.nextSelectable[1] = saveBackupsButton;
			}
		}
		else
		{
			checkBox.nextSelectable[1] = saveSlotButtons[saveSlotButtons.Length - 1];
			if (checkBox2 != null)
			{
				checkBox2.nextSelectable[1] = saveSlotButtons[saveSlotButtons.Length - 1];
			}
		}
		if (fpsSlider != null && qualityButtons != null)
		{
			fpsSlider.nextSelectable[1] = analogSlider;
			for (int num15 = 0; num15 < qualityButtons.Length; num15++)
			{
				qualityButtons[num15].nextSelectable[1] = fpsSlider;
			}
		}
		if (qualityButtons == null)
		{
			musicSlider.nextSelectable[1] = checkBox;
		}
		if (analogSlider != null)
		{
			analogSlider.nextSelectable[1] = checkBox;
			musicSlider.nextSelectable[1] = analogSlider;
		}
		arenaMusicSlider.nextSelectable[1] = musicSlider;
		soundSlider.nextSelectable[1] = arenaMusicSlider;
		backButton.nextSelectable[1] = soundSlider;
		for (int num16 = 0; num16 < saveSlotButtons.Length; num16++)
		{
			if (num16 == 0)
			{
				saveSlotButtons[num16].nextSelectable[1] = saveSlotButtons[num16];
			}
			else
			{
				saveSlotButtons[num16].nextSelectable[1] = saveSlotButtons[num16 - 1];
			}
		}
		if (saveBackupsButton != null)
		{
			saveBackupsButton.nextSelectable[1] = saveSlotButtons[saveSlotButtons.Length - 1];
		}
		wipeSaveButton.nextSelectable[1] = wipeSaveButton;
		if (languageButtons != null)
		{
			for (int num17 = 0; num17 < languageButtons.Length; num17++)
			{
				if (num17 < 3)
				{
					languageButtons[num17].nextSelectable[1] = languageButtons[num17];
				}
				else
				{
					languageButtons[num17].nextSelectable[1] = languageButtons[num17 - 3];
				}
			}
		}
		for (int num18 = 0; num18 < miscCheckboxes.Count; num18++)
		{
			if (num18 == miscCheckboxes.Count - 1)
			{
				if (languageButtons == null)
				{
					miscCheckboxes[num18].nextSelectable[1] = miscCheckboxes[num18];
				}
				else
				{
					miscCheckboxes[num18].nextSelectable[1] = languageButtons[languageButtons.Length - 1];
				}
			}
			else
			{
				miscCheckboxes[num18].nextSelectable[1] = miscCheckboxes[num18 + 1];
			}
		}
		if (backgroundsButton != null)
		{
			if (miscCheckboxes.Count > 0)
			{
				backgroundsButton.nextSelectable[1] = miscCheckboxes[0];
			}
			else if (languageButtons != null)
			{
				backgroundsButton.nextSelectable[1] = languageButtons[languageButtons.Length - 1];
			}
			else
			{
				backgroundsButton.nextSelectable[1] = backgroundsButton;
			}
		}
		if (controlsButton != null)
		{
			if (miscCheckboxes.Count > 0)
			{
				controlsButton.nextSelectable[1] = miscCheckboxes[0];
			}
			else if (languageButtons != null)
			{
				controlsButton.nextSelectable[1] = languageButtons[languageButtons.Length - 1];
			}
			else
			{
				controlsButton.nextSelectable[1] = controlsButton;
			}
		}
		if (controlsButton != null)
		{
			creditsButton.nextSelectable[1] = controlsButton;
		}
		else if (backgroundsButton != null)
		{
			creditsButton.nextSelectable[1] = backgroundsButton;
		}
		else if (miscCheckboxes.Count > 0)
		{
			creditsButton.nextSelectable[1] = miscCheckboxes[0];
		}
		if (fullScreenButton != null)
		{
			fullScreenButton.nextSelectable[3] = screenResolutionButtons[0];
			for (int num19 = 0; num19 < screenResolutionButtons.Length; num19++)
			{
				if (num19 == screenResolutionButtons.Length - 1)
				{
					screenResolutionButtons[num19].nextSelectable[3] = ((analogSlider != null) ? analogSlider : musicSlider);
				}
				else
				{
					screenResolutionButtons[num19].nextSelectable[3] = screenResolutionButtons[num19 + 1];
				}
			}
		}
		checkBox.nextSelectable[3] = ((analogSlider != null) ? analogSlider : musicSlider);
		if (checkBox2 != null)
		{
			checkBox2.nextSelectable[3] = ((analogSlider != null) ? analogSlider : musicSlider);
		}
		if (analogSlider != null)
		{
			analogSlider.nextSelectable[3] = ((fpsSlider != null) ? fpsSlider : musicSlider);
		}
		if (qualityButtons != null)
		{
			for (int num20 = 0; num20 < qualityButtons.Length; num20++)
			{
				qualityButtons[num20].nextSelectable[3] = musicSlider;
			}
		}
		musicSlider.nextSelectable[3] = arenaMusicSlider;
		arenaMusicSlider.nextSelectable[3] = soundSlider;
		soundSlider.nextSelectable[3] = backButton;
		backButton.nextSelectable[3] = backButton;
		for (int num21 = 0; num21 < saveSlotButtons.Length; num21++)
		{
			if (num21 == saveSlotButtons.Length - 1)
			{
				if (saveBackupsButton != null)
				{
					saveSlotButtons[num21].nextSelectable[3] = saveBackupsButton;
				}
				else
				{
					saveSlotButtons[num21].nextSelectable[3] = checkBox;
				}
			}
			else
			{
				saveSlotButtons[num21].nextSelectable[3] = saveSlotButtons[num21 + 1];
			}
		}
		if (saveBackupsButton != null)
		{
			saveBackupsButton.nextSelectable[3] = checkBox;
		}
		wipeSaveButton.nextSelectable[3] = checkBox;
		if (languageButtons != null)
		{
			for (int num22 = 0; num22 < languageButtons.Length; num22++)
			{
				if (num22 + 3 > languageButtons.Length - 1)
				{
					if (miscCheckboxes.Count > 0)
					{
						languageButtons[num22].nextSelectable[3] = miscCheckboxes[miscCheckboxes.Count - 1];
					}
					else
					{
						languageButtons[num22].nextSelectable[3] = ((backgroundsButton != null) ? backgroundsButton : ((controlsButton != null) ? controlsButton : creditsButton));
					}
				}
				else
				{
					languageButtons[num22].nextSelectable[3] = languageButtons[num22 + 3];
				}
			}
		}
		for (int num23 = 0; num23 < miscCheckboxes.Count; num23++)
		{
			if (num23 == 0)
			{
				miscCheckboxes[num23].nextSelectable[3] = ((backgroundsButton != null) ? backgroundsButton : ((controlsButton != null) ? controlsButton : creditsButton));
			}
			else
			{
				miscCheckboxes[num23].nextSelectable[3] = miscCheckboxes[num23 - 1];
			}
		}
		if (backgroundsButton != null)
		{
			backgroundsButton.nextSelectable[3] = creditsButton;
		}
		if (controlsButton != null)
		{
			controlsButton.nextSelectable[3] = creditsButton;
		}
		creditsButton.nextSelectable[3] = creditsButton;
		MenuLabel menuLabel = new MenuLabel(this, pages[0], "v1.9.15b", new Vector2((1366f - base.manager.rainWorld.screenSize.x) / 2f + 20f, manager.rainWorld.screenSize.y - 30f), new Vector2(200f, 20f), bigText: false);
		menuLabel.size = new Vector2(menuLabel.label.textRect.width, menuLabel.size.y);
		pages[0].subObjects.Add(menuLabel);
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
	}

	public static float GetSaveSlotButtonWidth(InGameTranslator.LanguageID lang)
	{
		float result = 100f;
		if (lang == InGameTranslator.LanguageID.French)
		{
			result = 220f;
		}
		else if (lang == InGameTranslator.LanguageID.Italian)
		{
			result = 160f;
		}
		else if (lang == InGameTranslator.LanguageID.German)
		{
			result = 130f;
		}
		else if (lang == InGameTranslator.LanguageID.Spanish || lang == InGameTranslator.LanguageID.Russian)
		{
			result = 170f;
		}
		else if (lang == InGameTranslator.LanguageID.Portuguese)
		{
			result = 150f;
		}
		else if (lang == InGameTranslator.LanguageID.Japanese)
		{
			result = 160f;
		}
		return result;
	}

	private static float GetWarningTextXOffset(InGameTranslator.LanguageID lang)
	{
		float result = -100f;
		if (lang == InGameTranslator.LanguageID.French)
		{
			result = -200f;
		}
		else if (lang == InGameTranslator.LanguageID.Italian)
		{
			result = -150f;
		}
		else if (lang == InGameTranslator.LanguageID.German)
		{
			result = -200f;
		}
		else if (lang == InGameTranslator.LanguageID.Spanish || lang == InGameTranslator.LanguageID.Russian)
		{
			result = -175f;
		}
		else if (lang == InGameTranslator.LanguageID.Portuguese)
		{
			result = -125f;
		}
		else if (lang == InGameTranslator.LanguageID.Japanese)
		{
			result = -300f;
		}
		return result;
	}

	public void InitMiscCheckboxes()
	{
		miscCheckboxes = new List<CheckBox>();
		validationCheckbox = new CheckBox(this, pages[0], this, Vector2.zero, -40f, Translate("Speedrun Validation"), "VALIDATION");
		validationCheckbox.selectable = true;
		miscCheckboxes.Add(validationCheckbox);
		if (!ModManager.MSC)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < Math.Min(MultiplayerUnlocks.TOTAL_CHALLENGES, manager.rainWorld.progression.miscProgressionData.completedChallenges.Count); i++)
		{
			if (manager.rainWorld.progression.miscProgressionData.completedChallenges[i])
			{
				num++;
			}
		}
		if ((num >= MultiplayerUnlocks.TOTAL_CHALLENGES || global::MoreSlugcats.MoreSlugcats.chtUnlockDevCommentary.Value) && manager.rainWorld.options.DeveloperCommentaryLocalized())
		{
			commentaryCheckbox = new CheckBox(this, pages[0], this, Vector2.zero, -40f, Translate("Enable Developer Commentary?"), "DEVCOMM");
			commentaryCheckbox.selectable = true;
		}
		else
		{
			commentaryCheckbox = null;
		}
		if (commentaryCheckbox != null)
		{
			miscCheckboxes.Add(commentaryCheckbox);
		}
	}

	public override void SliderSetValue(Slider slider, float f)
	{
		if (slider.ID == Slider.SliderID.MusicVol)
		{
			manager.rainWorld.options.musicVolume = f;
		}
		else if (slider.ID == Slider.SliderID.SfxVol)
		{
			manager.rainWorld.options.soundEffectsVolume = f;
		}
		else if (slider.ID == Slider.SliderID.ArenaMusicVolume)
		{
			manager.rainWorld.options.arenaMusicVolume = Mathf.InverseLerp(0f, manager.rainWorld.options.musicVolume, f);
			if (manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song.context == MusicPlayer.MusicContext.Arena)
			{
				manager.musicPlayer.song.baseVolume = 0.3f * manager.rainWorld.options.arenaMusicVolume;
			}
		}
		else if (slider.ID == MMFEnums.SliderID.FPSLimit)
		{
			manager.rainWorld.options.fpsCap = (int)(f * 81f + 40f);
			if (manager.rainWorld.options.fpsCap > 120)
			{
				Application.targetFrameRate = -1;
			}
			else
			{
				Application.targetFrameRate = manager.rainWorld.options.fpsCap;
			}
		}
		else if (slider.ID == MMFEnums.SliderID.AnalogSensitivity)
		{
			manager.rainWorld.options.analogSensitivity = f * 2.49f + 0.51f;
		}
		selectedObject = slider;
		infoLabel.text = UpdateInfoText();
		infoLabelSin = 0f;
		infoLabelFade = 1f;
		if (manager.rainWorld.options.musicVolume > 0f)
		{
			if (manager.musicPlayer == null)
			{
				manager.musicPlayer = new MusicPlayer(manager);
				manager.sideProcesses.Add(manager.musicPlayer);
			}
			if (manager.musicPlayer.song == null)
			{
				manager.musicPlayer.MenuRequestsSong("RW_8 - Sundown", 1f, 0.7f);
			}
		}
	}

	public override float ValueOfSlider(Slider slider)
	{
		if (slider.ID == Slider.SliderID.MusicVol)
		{
			return manager.rainWorld.options.musicVolume;
		}
		if (slider.ID == Slider.SliderID.SfxVol)
		{
			return manager.rainWorld.options.soundEffectsVolume;
		}
		if (slider.ID == Slider.SliderID.ArenaMusicVolume)
		{
			return manager.rainWorld.options.arenaMusicVolume * manager.rainWorld.options.musicVolume;
		}
		if (slider.ID == MMFEnums.SliderID.FPSLimit)
		{
			return (float)(manager.rainWorld.options.fpsCap - 40) / 81f;
		}
		if (slider.ID == MMFEnums.SliderID.AnalogSensitivity)
		{
			return (manager.rainWorld.options.analogSensitivity - 0.51f) / 2.49f;
		}
		return 0f;
	}

	public override string UpdateInfoText()
	{
		if (selectedObject is SelectOneButton)
		{
			if ((selectedObject as SelectOneButton).signalText == "ScreenRes")
			{
				return Translate("Change screen resolution");
			}
			if ((selectedObject as SelectOneButton).signalText == "Language")
			{
				return Translate("Change language");
			}
			if ((selectedObject as SelectOneButton).signalText == "SaveSlot")
			{
				return Translate("Select save slot") + " " + ((selectedObject as SelectOneButton).buttonArrayIndex + 1).ToString(CultureInfo.InvariantCulture);
			}
			if ((selectedObject as SelectOneButton).signalText == "Quality")
			{
				return Translate("Change graphics quality");
			}
		}
		if (selectedObject is Slider)
		{
			if ((selectedObject as Slider).ID == Slider.SliderID.MusicVol)
			{
				return Translate("Music volume:") + " " + Custom.IntClamp((int)(manager.rainWorld.options.musicVolume * 100f), 0, 100).ToString(CultureInfo.InvariantCulture) + "%";
			}
			if ((selectedObject as Slider).ID == Slider.SliderID.SfxVol)
			{
				return Translate("Sound effects volume:") + " " + Custom.IntClamp((int)(manager.rainWorld.options.soundEffectsVolume * 100f), 0, 100).ToString(CultureInfo.InvariantCulture) + "%";
			}
			if ((selectedObject as Slider).ID == Slider.SliderID.ArenaMusicVolume)
			{
				return Translate("Arena mode music volume:") + " " + Custom.IntClamp((int)(manager.rainWorld.options.arenaMusicVolume * manager.rainWorld.options.musicVolume * 100f), 0, 100).ToString(CultureInfo.InvariantCulture) + "%";
			}
			if ((selectedObject as Slider).ID == MMFEnums.SliderID.FPSLimit)
			{
				if (manager.rainWorld.options.fpsCap > 120)
				{
					return Translate("FPS Limit") + ": " + Translate("Unlimited");
				}
				return Translate("FPS Limit") + ": " + manager.rainWorld.options.fpsCap.ToString(CultureInfo.InvariantCulture);
			}
			if ((selectedObject as Slider).ID == MMFEnums.SliderID.AnalogSensitivity)
			{
				if (manager.rainWorld.options.analogSensitivity >= 1f)
				{
					return Translate("Analog Sensitivity") + ": " + manager.rainWorld.options.analogSensitivity.ToString("0.00", CultureInfo.InvariantCulture) + "x";
				}
				return Translate("Analog Sensitivity") + ": " + ((manager.rainWorld.options.analogSensitivity - 0.51f) / 0.49f).ToString("0.00", CultureInfo.InvariantCulture) + "x";
			}
		}
		if (selectedObject is HoldButton)
		{
			return Translate("Hold down to wipe your save slot and start over");
		}
		if (selectedObject is ControlsButton)
		{
			return Translate("Configure controls");
		}
		if (selectedObject == backButton)
		{
			return Translate("Back to main menu");
		}
		if (selectedObject == creditsButton)
		{
			return Translate("View credits");
		}
		if (saveBackupsButton != null && selectedObject == saveBackupsButton)
		{
			return Translate("backups_description");
		}
		if (selectedObject is CheckBox)
		{
			if ((selectedObject as CheckBox).IDString == "DEVCOMM")
			{
				return Translate("Spawn developer commentary collectables in the single player campaigns?");
			}
			if ((selectedObject as CheckBox).IDString == "VALIDATION")
			{
				return Translate("Show information on loading screens that helps verify settings used and the legitimacy of runs.");
			}
		}
		if (selectedObject is BackgroundsButton)
		{
			return Translate("Change menu background images");
		}
		return base.UpdateInfoText();
	}

	private string ResolutionString(Vector2 resolution, string ratio)
	{
		return (int)resolution.x + " x " + (int)resolution.y + "  [" + ratio + "]";
	}

	private void OnBackupChecked(bool response)
	{
		backupExists = response;
		checkedForBackup = true;
	}

	private void OnBackupRestored(bool response)
	{
		backupRestoreSuccess = response;
		waitingForBackupRestore = false;
	}

	public override void Update()
	{
		base.Update();
		arenaMusicSlider.buttonBehav.greyedOut = manager.rainWorld.options.musicVolume == 0f;
		for (int i = 0; i < saveSlotButtons.Length; i++)
		{
			saveSlotButtons[i].buttonBehav.greyedOut = ProgressionBusy;
		}
		if (saveBackupsButton != null)
		{
			saveBackupsButton.buttonBehav.greyedOut = ProgressionBusy;
		}
		backButton.buttonBehav.greyedOut = ProgressionBusy;
		wipeSaveButton.buttonBehav.greyedOut = ProgressionBusy;
		if (controlsButton != null)
		{
			controlsButton.buttonBehav.greyedOut = ProgressionBusy;
		}
		if (backgroundsButton != null)
		{
			backgroundsButton.buttonBehav.greyedOut = ProgressionBusy;
		}
		if (creditsButton != null)
		{
			creditsButton.buttonBehav.greyedOut = ProgressionBusy;
		}
		if (qualityButtons != null)
		{
			for (int j = 0; j < qualityButtons.Length; j++)
			{
				qualityButtons[j].buttonBehav.greyedOut = ProgressionBusy;
			}
		}
		if (screenResolutionButtons != null)
		{
			for (int k = 0; k < screenResolutionButtons.Length; k++)
			{
				screenResolutionButtons[k].buttonBehav.greyedOut = ProgressionBusy;
			}
		}
		if (fullScreenButton != null)
		{
			fullScreenButton.buttonBehav.greyedOut = ProgressionBusy;
		}
		if (languageButtons != null)
		{
			for (int l = 0; l < languageButtons.Length; l++)
			{
				languageButtons[l].buttonBehav.greyedOut = ProgressionBusy;
			}
		}
		if ((languageDirty || qualityDirty) && !ProgressionBusy)
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
		}
		else if (resolutionDirty && !ProgressionBusy)
		{
			resolutionDirty = false;
			Screen.SetResolution((int)manager.rainWorld.options.ScreenSize.x, (int)manager.rainWorld.options.ScreenSize.y, fullscreen: false);
			Screen.fullScreen = false;
			Futile.instance.UpdateScreenWidth((int)manager.rainWorld.options.ScreenSize.x);
			Cursor.visible = true;
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
		}
		else if (Screen.fullScreen != manager.rainWorld.options.fullScreen)
		{
			wrongFullscreenSettingCounter++;
			if (wrongFullscreenSettingCounter > 10)
			{
				Screen.fullScreen = manager.rainWorld.options.fullScreen;
				Cursor.visible = !manager.rainWorld.options.fullScreen;
				wrongFullscreenSettingCounter = 0;
			}
		}
		bool flag = RWInput.CheckPauseButton(0);
		if (flag && !lastPauseButton && manager.dialog == null)
		{
			OnExit();
		}
		lastPauseButton = flag;
		if (waitingOnProgressionLoaded && manager.rainWorld.progression.progressionLoaded)
		{
			PlayerProgression.ProgressionLoadResult progressionLoadedResult = manager.rainWorld.progression.progressionLoadedResult;
			if (progressionLoadedResult == PlayerProgression.ProgressionLoadResult.SUCCESS_CREATE_NEW_FILE || progressionLoadedResult == PlayerProgression.ProgressionLoadResult.SUCCESS_LOAD_EXISTING_FILE || progressionLoadedResult == PlayerProgression.ProgressionLoadResult.ERROR_SAVE_DATA_MISSING)
			{
				waitingOnProgressionLoaded = false;
			}
			else
			{
				HandleSaveSlotChangeFailed(progressionLoadedResult);
			}
		}
		if (wipeSaveButton.FillingUp)
		{
			resetWarningTextCounter++;
			resetWarningTextAlpha = Custom.LerpAndTick(resetWarningTextAlpha, 1f, 0.08f, 0.025f);
		}
		else
		{
			resetWarningTextAlpha = Custom.LerpAndTick(resetWarningTextAlpha, 0f, 0.08f, 0.05f);
		}
		if (qualityButtons != null)
		{
			int num = 0;
			for (int m = 0; m < qualityButtons.Length; m++)
			{
				if (qualityButtons[m].AmISelected)
				{
					num = m;
					break;
				}
			}
			musicSlider.nextSelectable[1] = qualityButtons[num];
			if (fpsSlider != null)
			{
				fpsSlider.nextSelectable[3] = qualityButtons[num];
			}
		}
		resetWarningText.label.alpha = (0.7f + 0.3f * Mathf.Sin((float)resetWarningTextCounter / 40f * (float)Math.PI * 2f)) * resetWarningTextAlpha;
		resetWarningText.label.color = Color.Lerp(new Color(1f, 0f, 0f), new Color(1f, 1f, 1f), Mathf.Pow(0.5f + 0.5f * Mathf.Sin((float)resetWarningTextCounter / 40f * (float)Math.PI * 2f), 2f));
	}

	public void HandleSaveSlotChangeFailed(PlayerProgression.ProgressionLoadResult loadResult)
	{
		if (reportCorruptedDialogDisplaying)
		{
			return;
		}
		reportCorruptedDialogDisplaying = true;
		string text = manager.rainWorld.inGameTranslator.Translate("ps4_load_save_slot_load_failed");
		string text2 = loadResult.ToString();
		if (loadResult == PlayerProgression.ProgressionLoadResult.ERROR_READ_FAILED && manager.rainWorld.progression.SaveDataReadFailureError != null)
		{
			text2 = text2 + Environment.NewLine + manager.rainWorld.progression.SaveDataReadFailureError;
		}
		string text3 = text.Replace("{ERROR}", text2);
		DialogNotify dialog = new DialogNotify(size: DialogBoxNotify.CalculateDialogBoxSize(text3, dialogUsesWordWrapping: false), description: Custom.ReplaceLineDelimeters(text3), manager: manager.rainWorld.processManager, onOK: delegate
		{
			reportCorruptedDialogDisplaying = false;
			if (leavingSaveSlot >= 0)
			{
				manager.rainWorld.options.saveSlot = leavingSaveSlot;
				manager.rainWorld.progression.Destroy();
				manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
				waitingOnProgressionLoaded = true;
				leavingSaveSlot = -1;
			}
			else
			{
				waitingOnProgressionLoaded = false;
			}
		});
		manager.rainWorld.processManager.ShowDialog(dialog);
	}

	public void OnExit()
	{
		if (!exiting && !ProgressionBusy)
		{
			exiting = true;
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
			PlaySound(SoundID.MENU_Switch_Page_Out);
			manager.rainWorld.options.Save();
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (!ProgressionBusy && message != null)
		{
			switch (message)
			{
			case "Toggle Fullscreen":
				manager.rainWorld.options.windowed = !manager.rainWorld.options.windowed;
				resolutionDirty = true;
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				break;
			case "BACK":
				OnExit();
				break;
			case "CREDITS":
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Credits);
				PlaySound(SoundID.MENU_Switch_Page_In);
				manager.rainWorld.options.Save();
				break;
			case "RESET PROGRESS":
				manager.rainWorld.progression.WipeAll();
				PlaySound(SoundID.MENU_Switch_Page_In);
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
				break;
			case "INPUT":
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.InputOptions);
				PlaySound(SoundID.MENU_Switch_Page_In);
				manager.rainWorld.options.Save();
				break;
			case "BACKGROUND":
				manager.RequestMainProcessSwitch(MMFEnums.ProcessID.BackgroundOptions);
				PlaySound(SoundID.MENU_Switch_Page_In);
				manager.rainWorld.options.Save();
				break;
			case "BACKUPS":
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.BackupManager);
				PlaySound(SoundID.MENU_Switch_Page_In);
				manager.rainWorld.options.Save();
				break;
			}
		}
	}

	public void ChangeResolution(int newRes)
	{
		manager.rainWorld.options.resolution = newRes;
		resolutionDirty = true;
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		if (darkSprite != null)
		{
			darkSprite.RemoveFromContainer();
		}
		if (manager.rainWorld.options.musicVolume == 0f && manager.musicPlayer != null)
		{
			manager.StopSideProcess(manager.musicPlayer);
		}
	}

	public int GetCurrentlySelectedOfSeries(string series)
	{
		switch (series)
		{
		case "Language":
			return manager.rainWorld.options.language.Index;
		case "SaveSlot":
			return manager.rainWorld.options.saveSlot;
		case "ScreenRes":
			return manager.rainWorld.options.resolution;
		case "Quality":
			if (manager.rainWorld.options.quality.Index != -1)
			{
				return manager.rainWorld.options.quality.Index;
			}
			return 0;
		default:
			return -1;
		}
	}

	public void SetCurrentlySelectedOfSeries(string series, int to)
	{
		switch (series)
		{
		case "Language":
			if (manager.rainWorld.options.language.Index != to && !languageDirty)
			{
				InGameTranslator.LanguageID language = manager.rainWorld.options.language;
				manager.rainWorld.options.language = new InGameTranslator.LanguageID(ExtEnum<InGameTranslator.LanguageID>.values.GetEntry(to));
				InGameTranslator.UnloadFonts(language);
				InGameTranslator.LoadFonts(manager.rainWorld.options.language, this);
				languageDirty = true;
			}
			break;
		case "SaveSlot":
			if (manager.rainWorld.options.saveSlot != to)
			{
				leavingSaveSlot = manager.rainWorld.options.saveSlot;
				manager.rainWorld.options.saveSlot = to;
				manager.rainWorld.progression.Destroy(leavingSaveSlot);
				manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
				waitingOnProgressionLoaded = true;
			}
			break;
		case "ScreenRes":
			if (manager.rainWorld.options.resolution != to && !resolutionDirty)
			{
				ChangeResolution(to);
			}
			break;
		case "Quality":
			if (manager.rainWorld.options.quality.Index != to && !qualityDirty)
			{
				manager.rainWorld.options.quality = new Options.Quality(ExtEnum<Options.Quality>.values.GetEntry(to));
				qualityDirty = true;
			}
			break;
		}
	}

	public bool GetChecked(CheckBox box)
	{
		switch (box.IDString)
		{
		case "VIBRATION":
			return manager.rainWorld.options.vibration;
		case "DEVCOMM":
			return manager.rainWorld.options.commentary;
		case "VALIDATION":
			return manager.rainWorld.options.validation;
		case "VSYNC":
			return manager.rainWorld.options.vsync;
		case "ENABLESTATS":
		{
			AGGamePerfStats aGGamePerfStats = UnityEngine.Object.FindObjectOfType<AGGamePerfStats>(includeInactive: true);
			if (aGGamePerfStats == null)
			{
				return false;
			}
			return aGGamePerfStats.gameObject.activeSelf;
		}
		default:
			return false;
		}
	}

	public void SetChecked(CheckBox box, bool c)
	{
		switch (box.IDString)
		{
		case "VIBRATION":
			manager.rainWorld.options.vibration = !manager.rainWorld.options.vibration;
			break;
		case "DEVCOMM":
			manager.rainWorld.options.commentary = !manager.rainWorld.options.commentary;
			break;
		case "VALIDATION":
			manager.rainWorld.options.validation = !manager.rainWorld.options.validation;
			break;
		case "VSYNC":
			manager.rainWorld.options.vsync = !manager.rainWorld.options.vsync;
			QualitySettings.vSyncCount = (manager.rainWorld.options.vsync ? 1 : 0);
			fpsSlider.buttonBehav.greyedOut = manager.rainWorld.options.vsync;
			break;
		case "ENABLESTATS":
		{
			AGGamePerfStats aGGamePerfStats = UnityEngine.Object.FindObjectOfType<AGGamePerfStats>(includeInactive: true);
			if (aGGamePerfStats != null)
			{
				aGGamePerfStats.gameObject.SetActive(!aGGamePerfStats.gameObject.activeSelf);
			}
			break;
		}
		}
	}
}
