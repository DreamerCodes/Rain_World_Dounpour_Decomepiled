using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu.Remix;

internal class ConfigMenuTab : MenuTab
{
	private class ButtonManager : UIelement
	{
		internal OpSimpleButton applyButton;

		internal OpSimpleButton backButton;

		internal OpSimpleButton saveButton;

		internal OpSimpleButton revertButton;

		internal OpHoldButton resetButton;

		internal OpHoldButton backHoldButton;

		internal OpHoldButton revertHoldButton;

		internal OpHoldButton modPercentageButton;

		private readonly OpContainer allApplyIcon;

		private readonly GlowGradient allApplyIconGlow;

		private readonly FSprite allApplyIconSpr;

		private bool lastPauseButton;

		internal int applyFlash;

		internal bool modAllApply;

		private bool reverted;

		public ButtonManager(ConfigMenuTab tab)
			: base(Vector2.zero, Vector2.one)
		{
			tab.AddItems(this);
			applyButton = new OpSimpleButton(new Vector2(490f, 50f), new Vector2(120f, 30f), OptionalText.GetText(OptionalText.ID.ConfigMenuTab_ApplyButton_Label))
			{
				description = OptionalText.GetText(OptionalText.ID.ConfigMenuTab_ApplyButton_Desc),
				soundClick = SoundID.None
			};
			backButton = new OpSimpleButton(new Vector2(630f, 50f), new Vector2(120f, 30f), OptionalText.GetText(OptionalText.ID.ConfigMenuTab_BackButton_Label))
			{
				description = OptionalText.GetText(OptionalText.ID.ConfigMenuTab_BackButton_Desc),
				soundClick = SoundID.None
			};
			backHoldButton = new OpHoldButton(new Vector2(630f, 50f), new Vector2(120f, 30f), OptionalText.GetText(OptionalText.ID.ConfigMenuTab_BackButton_Label), 20f)
			{
				description = OptionalText.GetText(OptionalText.ID.ConfigMenuTab_BackHoldButton_Desc)
			};
			backHoldButton.Hide();
			revertButton = new OpSimpleButton(new Vector2(780f, 50f), new Vector2(110f, 30f), OptionalText.GetText(OptionalText.ID.ConfigMenuTab_RevertButton_Label))
			{
				description = OptionalText.GetText(OptionalText.ID.ConfigMenuTab_RevertButton_Desc)
			};
			revertHoldButton = new OpHoldButton(new Vector2(780f, 50f), new Vector2(110f, 30f), OptionalText.GetText(OptionalText.ID.ConfigMenuTab_RevertHoldButton_Label), 20f)
			{
				description = OptionalText.GetText(OptionalText.ID.ConfigMenuTab_RevertHoldButton_Desc)
			};
			revertHoldButton.Hide();
			saveButton = new OpSimpleButton(new Vector2(905f, 50f), new Vector2(110f, 30f), OptionalText.GetText(OptionalText.ID.ConfigMenuTab_SaveButton_Label))
			{
				description = OptionalText.GetText(OptionalText.ID.ConfigMenuTab_SaveButton_Desc)
			};
			resetButton = new OpHoldButton(new Vector2(1030f, 50f), new Vector2(110f, 30f), OptionalText.GetText(OptionalText.ID.ConfigMenuTab_ResetButton_Label), 30f)
			{
				description = OptionalText.GetText(OptionalText.ID.ConfigMenuTab_ResetButton_Desc)
			};
			modPercentageButton = new OpHoldButton(new Vector2(780f, 50f), new Vector2(360f, 30f), OptionalText.GetText(OptionalText.ID.ConfigMenuTab_ResetButton_Desc));
			allApplyIcon = new OpContainer(new Vector2(790f, 65f));
			allApplyIconGlow = new GlowGradient(allApplyIcon.container, new Vector2(15f, 0f), 30f, 30f, 0.6f);
			allApplyIconSpr = new FSprite("enableAll")
			{
				anchorX = 0f,
				anchorY = 0.5f,
				color = modPercentageButton.colorEdge
			};
			allApplyIconGlow.centerPos = new Vector2(allApplyIconSpr.element.sourceSize.x / 2f, 0f);
			allApplyIcon.container.AddChild(allApplyIconSpr);
			tab.AddItems(applyButton, backButton, saveButton, revertButton, resetButton, backHoldButton, revertHoldButton, modPercentageButton, allApplyIcon);
			applyButton.OnClick += SignalApply;
			backHoldButton.OnPressDone += SignalBack;
			backButton.OnClick += SignalBack;
			revertButton.OnClick += SignalRevert;
			revertHoldButton.OnPressDone += SignalRevert;
			saveButton.OnClick += SignalSave;
			resetButton.OnPressDone += SignalReset;
			modPercentageButton.OnClick += SignalApplyAll;
			Update();
		}

		internal void _SetFocusPointers(ConfigMenuTab tab)
		{
			applyButton.SetNextFocusable(UIfocusable.NextDirection.Up, tab.modList._roleButtons[5]);
			applyButton.SetNextFocusable(UIfocusable.NextDirection.Left, tab.modList._roleButtons[5]);
			applyButton.SetNextFocusable(UIfocusable.NextDirection.Right, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._BackButton));
			applyButton.SetNextFocusable(UIfocusable.NextDirection.Back, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._BackButton));
			UIfocusable[] array = new UIfocusable[2] { backButton, backHoldButton };
			foreach (UIfocusable uIfocusable in array)
			{
				uIfocusable.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._ApplyButton));
				uIfocusable.SetNextFocusable(UIfocusable.NextDirection.Right, modPercentageButton);
				uIfocusable.SetNextFocusable(UIfocusable.NextDirection.Up, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._BackButtonUpPointer));
				uIfocusable.SetNextFocusable(UIfocusable.NextDirection.Back, uIfocusable);
			}
			modPercentageButton.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._BackButton));
			modPercentageButton.SetNextFocusable(UIfocusable.NextDirection.Right, modPercentageButton);
			modPercentageButton.SetNextFocusable(UIfocusable.NextDirection.Up, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._BackButtonUpPointer));
			modPercentageButton.SetNextFocusable(UIfocusable.NextDirection.Back, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._BackButton));
			array = new UIfocusable[2] { revertButton, revertHoldButton };
			foreach (UIfocusable uIfocusable2 in array)
			{
				uIfocusable2.SetNextFocusable(UIfocusable.NextDirection.Left, uIfocusable2);
				uIfocusable2.SetNextFocusable(UIfocusable.NextDirection.Right, saveButton);
				uIfocusable2.SetNextFocusable(UIfocusable.NextDirection.Up, null);
				uIfocusable2.SetNextFocusable(UIfocusable.NextDirection.Back, uIfocusable2);
			}
			saveButton.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton));
			saveButton.SetNextFocusable(UIfocusable.NextDirection.Right, resetButton);
			saveButton.SetNextFocusable(UIfocusable.NextDirection.Up, null);
			saveButton.SetNextFocusable(UIfocusable.NextDirection.Back, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton));
			resetButton.SetNextFocusable(UIfocusable.NextDirection.Left, saveButton);
			resetButton.SetNextFocusable(UIfocusable.NextDirection.Right, resetButton);
			resetButton.SetNextFocusable(UIfocusable.NextDirection.Up, null);
			resetButton.SetNextFocusable(UIfocusable.NextDirection.Back, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton));
			array = new UIfocusable[8] { applyButton, backButton, saveButton, revertButton, resetButton, backHoldButton, revertHoldButton, modPercentageButton };
			foreach (UIfocusable uIfocusable3 in array)
			{
				uIfocusable3.SetNextFocusable(UIfocusable.NextDirection.Down, uIfocusable3);
			}
		}

		internal void FlashApplyButton(int period)
		{
			if (applyFlash > 0)
			{
				ConfigConnector.CreateDialogBoxNotify(OptionalText.GetText(OptionalText.ID.MenuModList_AskModApply));
			}
			else
			{
				ModdingMenu.instance.ShowAlert(OptionalText.GetText(OptionalText.ID.MenuModList_AskModApply));
			}
			applyFlash = UIelement.FrameMultiply(period);
			applyButton.bumpBehav.flash = 1.5f;
		}

		private void UpdateAllApplyIcon()
		{
			allApplyIconSpr.SetElementByName(modAllApply ? "enableAll" : "disableAll");
			float t = Custom.SCurve(modPercentageButton.progress / 100f, 0.6f);
			allApplyIconSpr.color = Color.Lerp(MenuColorEffect.rgbMediumGrey, MenuColorEffect.rgbVeryDarkGrey, t);
			allApplyIconGlow.color = Color.Lerp(MenuColorEffect.rgbBlack, MenuColorEffect.rgbMediumGrey, t);
		}

		public override void Update()
		{
			base.Update();
			bool flag = RWInput.CheckPauseButton(0);
			if (flag && !lastPauseButton && Custom.rainWorld.processManager.dialog == null)
			{
				if (!backButton.greyedOut && !backButton.Hidden)
				{
					SignalBack(backButton);
				}
				else if (!backHoldButton.greyedOut && !backHoldButton.Hidden)
				{
					SignalBack(backHoldButton);
				}
			}
			lastPauseButton = flag;
			if (applyFlash > 0)
			{
				applyFlash--;
				if (applyFlash % UIelement.FrameMultiply(20) == 1)
				{
					applyButton.bumpBehav.flash = 1.5f;
				}
			}
			if (reverted)
			{
				reverted = Input.GetMouseButton(0);
			}
			switch (ConfigMode)
			{
			case ConfigContainer.Mode.ModSelect:
			case ConfigContainer.Mode.ModView:
				revertButton.Hide();
				revertHoldButton.Hide();
				saveButton.Hide();
				resetButton.Hide();
				modPercentageButton.Show();
				allApplyIcon.Show();
				if (ConfigMode == ConfigContainer.Mode.ModView)
				{
					backButton.Show();
					backHoldButton.Hide();
				}
				else
				{
					backButton.Hide();
					backHoldButton.Show();
				}
				applyButton.greyedOut = ConfigMode == ConfigContainer.Mode.ModView;
				backButton.greyedOut = false;
				backButton.SetNextFocusable(UIfocusable.NextDirection.Right, modPercentageButton);
				modPercentageButton.SetProgress(100f * (float)_countModEnabled / (float)_countModTotal);
				modPercentageButton.text = OptionalText.GetText(OptionalText.ID.ConfigMenuTab_ModPercentageButton_Label).Replace("<CountModEnabled>", _countModEnabled.ToString()).Replace("<CountModTotal>", _countModTotal.ToString());
				modPercentageButton.greyedOut = reverted;
				if (base.MenuMouseMode)
				{
					modPercentageButton.description = OptionalText.GetText(modAllApply ? OptionalText.ID.ConfigMenuTab_ModPercentageButton_MouseEnableAll : OptionalText.ID.ConfigMenuTab_ModPercentageButton_MouseDisableAll);
				}
				else
				{
					modPercentageButton.description = OptionalText.GetText(modAllApply ? OptionalText.ID.ConfigMenuTab_ModPercentageButton_NonMouseEnableAll : OptionalText.ID.ConfigMenuTab_ModPercentageButton_NonMouseDisableAll);
				}
				UpdateAllApplyIcon();
				break;
			case ConfigContainer.Mode.ModConfig:
				backHoldButton.Hide();
				backButton.Show();
				modPercentageButton.Hide();
				allApplyIcon.Hide();
				saveButton.Show();
				resetButton.Show();
				applyButton.greyedOut = true;
				backButton.greyedOut = true;
				backButton.SetNextFocusable(UIfocusable.NextDirection.Right, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton));
				saveButton.greyedOut = ConfigContainer.ActiveInterface.error || (!ConfigContainer.HasConfigChanged() && !ConfigContainer.ActiveInterface.config.pendingReset);
				resetButton.greyedOut = ConfigContainer.ActiveInterface.error || !ConfigContainer.ActiveInterface.HasConfigurables();
				if (saveButton.greyedOut)
				{
					revertButton.Show();
					revertHoldButton.Hide();
				}
				else
				{
					revertButton.Hide();
					revertHoldButton.Show();
				}
				break;
			}
		}

		private void SignalApplyAll(UIfocusable trigger)
		{
			if (!reverted)
			{
				ModdingMenu.instance.Singal(null, modAllApply ? "ENABLEALL" : "DISABLEALL");
				modAllApply = !modAllApply;
			}
		}

		private void SignalApply(UIfocusable trigger)
		{
			ModdingMenu.instance.Singal(null, "APPLYMODS");
		}

		private void SignalBack(UIfocusable trigger)
		{
			ModdingMenu.instance.Singal(null, "EXIT");
		}

		private void SignalRevert(UIfocusable trigger)
		{
			trigger.held = false;
			ModdingMenu.instance.Singal(null, "REVERT");
			reverted = true;
		}

		private void SignalSave(UIfocusable trigger)
		{
			ModdingMenu.instance.Singal(null, "APPLY");
			reverted = true;
		}

		private void SignalReset(UIfocusable trigger)
		{
			ModdingMenu.instance.Singal(null, "RESET");
		}
	}

	private readonly ButtonManager btnManager;

	internal readonly MenuModList modList;

	internal readonly ConfigTabController tabCtrler;

	internal const string exitSignal = "EXIT";

	internal const string applyModSignal = "APPLYMODS";

	internal const string enableAllSignal = "ENABLEALL";

	internal const string disableAllSignal = "DISABLEALL";

	internal const string revertSignal = "REVERT";

	internal const string saveSignal = "APPLY";

	internal const string resetSignal = "RESET";

	internal const string yesDialogSignal = "DIALOG_YES";

	internal const string noDialogSignal = "DIALOG_NO";

	internal static int _countModEnabled;

	internal static int _countModTotal;

	private static ConfigContainer.Mode ConfigMode => ConfigContainer.instance._Mode;

	internal OpSimpleButton ApplyButton => btnManager.applyButton;

	internal UIfocusable BackButton
	{
		get
		{
			if (btnManager.backHoldButton.IsInactive)
			{
				return btnManager.backButton;
			}
			return btnManager.backHoldButton;
		}
	}

	internal UIfocusable RevertButton
	{
		get
		{
			if (btnManager.revertHoldButton.IsInactive)
			{
				return btnManager.revertButton;
			}
			return btnManager.revertHoldButton;
		}
	}

	internal OpSimpleButton SaveButton => btnManager.saveButton;

	internal OpHoldButton ResetConfigButton => btnManager.resetButton;

	internal UIfocusable[] MenuButtons => new UIfocusable[8] { btnManager.applyButton, btnManager.backButton, btnManager.saveButton, btnManager.revertButton, btnManager.resetButton, btnManager.backHoldButton, btnManager.revertHoldButton, btnManager.modPercentageButton };

	internal ConfigMenuTab()
	{
		ConfigContainer.instance.Container.AddChild(_container);
		_container.MoveToBack();
		btnManager = new ButtonManager(this);
		modList = new MenuModList(this);
		tabCtrler = new ConfigTabController(this);
		btnManager.modAllApply = _countModEnabled < _countModTotal;
		btnManager._SetFocusPointers(this);
	}

	internal bool IsPartOfButtonManager(UIfocusable test)
	{
		if (test != btnManager.applyButton && test != btnManager.backButton && test != btnManager.saveButton)
		{
			return test == btnManager.resetButton;
		}
		return true;
	}

	internal void FlashApplyButton(int period = 80)
	{
		btnManager.FlashApplyButton(period);
	}

	internal void _ClearCustomNextFocusable()
	{
		btnManager.revertButton.SetNextFocusable(UIfocusable.NextDirection.Up, null);
		btnManager.revertHoldButton.SetNextFocusable(UIfocusable.NextDirection.Up, null);
		btnManager.saveButton.SetNextFocusable(UIfocusable.NextDirection.Up, null);
		btnManager.resetButton.SetNextFocusable(UIfocusable.NextDirection.Up, null);
		tabCtrler._ClearCustomNextFocusable();
	}

	internal void _SetRevertButtonUpPointer(UIfocusable up)
	{
		btnManager.revertButton.SetNextFocusable(UIfocusable.NextDirection.Up, up);
		btnManager.revertHoldButton.SetNextFocusable(UIfocusable.NextDirection.Up, up);
	}
}
