using System;
using System.Globalization;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class MMFOptionInterface : OptionInterface
{
	private const int PRESET_TAB = 0;

	private const int GENERAL_TAB = 1;

	private const int ACCESSIBILITY_TAB = 2;

	private const string RED_MARK = "CHEAT_MARK";

	private readonly Color cheatColor = new Color(0.85f, 0.35f, 0.4f);

	private OpSimpleButton remixPresetBtn;

	private OpSimpleButton classicPresetBtn;

	private OpSimpleButton relaxedPresetBtn;

	private OpLabel currentPresetLabel;

	public override void Initialize()
	{
		base.Initialize();
		Tabs = new OpTab[3]
		{
			new OpTab(this, OptionInterface.Translate("Presets")),
			new OpTab(this, OptionInterface.Translate("General")),
			new OpTab(this, OptionInterface.Translate("Assists"))
		};
		OpLabel opLabel = new OpLabel(new Vector2(150f, 520f), new Vector2(300f, 30f), base.mod.LocalizedName, FLabelAlignment.Center, bigText: true);
		Tabs[0].AddItems(opLabel);
		OpLabel opLabel2 = new OpLabel(new Vector2(150f, 420f), new Vector2(300f, 30f), Custom.ReplaceLineDelimeters(OptionInterface.Translate("The presets below can be used to quickly reset settings.") + Environment.NewLine + OptionInterface.Translate("These are recommended starting points based on different playstyles.")));
		Tabs[0].AddItems(opLabel2);
		remixPresetBtn = new OpSimpleButton(new Vector2(250f, 360f), new Vector2(120f, 30f), OptionInterface.Translate("REMIX"))
		{
			description = OptionInterface.Translate("Standard Remix settings. Many mechanical tweaks that may mix up the way the game is experienced.")
		};
		classicPresetBtn = new OpSimpleButton(new Vector2(250f, 320f), new Vector2(120f, 30f), OptionInterface.Translate("CLASSIC"))
		{
			description = OptionInterface.Translate("Similar to the original Rain World experience, but with a few quality of life settings enabled.")
		};
		relaxedPresetBtn = new OpSimpleButton(new Vector2(250f, 280f), new Vector2(120f, 30f), OptionInterface.Translate("RELAXED"))
		{
			description = OptionInterface.Translate("Settings designed to make the game significantly easier, but may weaken the overall game experience.")
		};
		remixPresetBtn.OnClick += PresetRemix;
		classicPresetBtn.OnClick += PresetClassic;
		relaxedPresetBtn.OnClick += ConfirmPresetRelaxed;
		Tabs[0].AddItems(remixPresetBtn);
		Tabs[0].AddItems(classicPresetBtn);
		Tabs[0].AddItems(relaxedPresetBtn);
		ConfigurableBase[][] array = new ConfigurableBase[3][];
		string[] names = new string[3]
		{
			OptionInterface.Translate("HUD"),
			OptionInterface.Translate("Remix"),
			OptionInterface.Translate("Legacy")
		};
		array[0] = new ConfigurableBase[11]
		{
			MMF.cfgSpeedrunTimer,
			MMF.cfgHideRainMeterNoThreat,
			MMF.cfgLoadingScreenTips,
			MMF.cfgExtraTutorials,
			MMF.cfgClearerDeathGradients,
			MMF.cfgShowUnderwaterShortcuts,
			MMF.cfgBreathTimeVisualIndicator,
			MMF.cfgCreatureSense,
			MMF.cfgTickTock,
			MMF.cfgFastMapReveal,
			MMF.cfgThreatMusicPulse
		};
		array[1] = new ConfigurableBase[10]
		{
			MMF.cfgExtraLizardSounds,
			MMF.cfgVulnerableJellyfish,
			MMF.cfgNewDynamicDifficulty,
			MMF.cfgSurvivorPassageNotRequired,
			MMF.cfgIncreaseStuns,
			MMF.cfgUpwardsSpearThrow,
			MMF.cfgDislodgeSpears,
			MMF.cfgAlphaRedLizards,
			MMF.cfgSandboxItemStems,
			MMF.cfgNoArenaFleeing
		};
		array[2] = new ConfigurableBase[3]
		{
			MMF.cfgVanillaExploits,
			MMF.cfgOldTongue,
			MMF.cfgWallpounce
		};
		PopulateWithConfigs(1, array, names);
		array = new ConfigurableBase[3][];
		names = new string[3]
		{
			OptionInterface.Translate("Quality of Life"),
			OptionInterface.Translate("Minor Assist"),
			OptionInterface.Translate("Major Assist")
		};
		array[0] = new ConfigurableBase[13]
		{
			MMF.cfgFasterShelterOpen,
			MMF.cfgQuieterGates,
			MMF.cfgNoMoreTinnitus,
			MMF.cfgDisableScreenShake,
			MMF.cfgClimbingGrip,
			MMF.cfgSwimBreathLeniency,
			MMF.cfgJetfishItemProtection,
			MMF.cfgKeyItemTracking,
			MMF.cfgKeyItemPassaging,
			MMF.cfgScavengerKillSquadDelay,
			MMF.cfgDeerBehavior,
			MMF.cfgHunterBatflyAutograb,
			MMF.cfgHunterBackspearProtect
		};
		array[1] = new ConfigurableBase[5]
		{
			MMF.cfgMonkBreathTime,
			MMF.cfgLargeHologramLight,
			MMF.cfgGraspWiggling,
			MMF.cfgNoRandomCycles,
			MMF.cfgSafeCentipedes
		};
		array[2] = new ConfigurableBase[8]
		{
			MMF.cfgFreeSwimBoosts,
			MMF.cfgHunterCycles,
			MMF.cfgHunterBonusCycles,
			MMF.cfgSlowTimeFactor,
			new Configurable<string>("CHEAT_MARK"),
			MMF.cfgGlobalMonkGates,
			MMF.cfgDisableGateKarma,
			MMF.cfgRainTimeMultiplier
		};
		PopulateWithConfigs(2, array, names);
		currentPresetLabel = new OpLabel(new Vector2(150f, 220f), new Vector2(300f, 30f), GetCurrentPresetString());
		Tabs[0].AddItems(currentPresetLabel);
	}

	public override string ValidationString()
	{
		string text = "[" + base.mod.id + "]  ";
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < MMF.boolPresets.Count; i++)
		{
			num += (int)Mathf.Pow(2f, i % 4) * (MMF.boolPresets[i].config.Value ? 1 : 0);
			if (i % 4 == 3 || i == MMF.boolPresets.Count - 1)
			{
				text += num.ToString("X");
				num2++;
				if (num2 % 4 == 0)
				{
					text += "  ";
				}
				num = 0;
			}
		}
		for (int j = 0; j < MMF.intPresets.Count; j++)
		{
			text = text + " " + MMF.intPresets[j].config.Value;
		}
		for (int k = 0; k < MMF.floatPresets.Count; k++)
		{
			text = text + " " + MMF.floatPresets[k].config.Value.ToString("0.00");
		}
		text = text + " " + Custom.rainWorld.options.fpsCap;
		text = text + " " + (Custom.rainWorld.options.vsync ? "1" : "0");
		text = text + " " + Custom.rainWorld.options.analogSensitivity.ToString("0.00");
		return text + " " + Custom.rainWorld.options.quality;
	}

	private void WarningOff(UIfocusable trigger)
	{
	}

	private string GetCurrentPresetString()
	{
		string text = OptionInterface.Translate("Currently Active Preset") + ": ";
		if (CheckPresetRemix())
		{
			return text + remixPresetBtn.text;
		}
		if (CheckPresetClassic())
		{
			return text + classicPresetBtn.text;
		}
		if (CheckPresetRelaxed())
		{
			return text + relaxedPresetBtn.text;
		}
		return text + OptionInterface.Translate("NONE (User Defined)");
	}

	private void PopulateWithConfigs(int tabIndex, ConfigurableBase[][] lists, string[] names)
	{
		new OpLabel(new Vector2(100f, 560f), new Vector2(400f, 30f), Tabs[tabIndex].name, FLabelAlignment.Center, bigText: true);
		OpTab opTab = Tabs[tabIndex];
		float num = 40f;
		float num2 = 20f;
		float num3 = 550f;
		UIconfig uIconfig = null;
		for (int i = 0; i < lists.Length; i++)
		{
			bool flag = false;
			opTab.AddItems(new OpLabel(new Vector2(num2, num3 - num + 10f), new Vector2(260f, 30f), "~ " + names[i] + " ~", FLabelAlignment.Center, bigText: true));
			FTextParams fTextParams = new FTextParams();
			if (InGameTranslator.LanguageID.UsesLargeFont(Custom.rainWorld.inGameTranslator.currentLanguage))
			{
				fTextParams.lineHeightOffset = -12f;
			}
			else
			{
				fTextParams.lineHeightOffset = -5f;
			}
			for (int j = 0; j < lists[i].Length; j++)
			{
				switch (ValueConverter.GetTypeCategory(lists[i][j].settingType))
				{
				case ValueConverter.TypeCategory.String:
					if (lists[i][j].defaultValue == "CHEAT_MARK")
					{
						flag = true;
					}
					break;
				case ValueConverter.TypeCategory.Integrals:
				{
					num += 36f;
					Configurable<int> configurable3 = lists[i][j] as Configurable<int>;
					OpUpdown opUpdown2 = new OpUpdown(configurable3, new Vector2(num2, num3 - num), 100f)
					{
						description = OptionInterface.Translate(configurable3.info.description),
						sign = i
					};
					opUpdown2.OnChange += RefreshCurrentPresetLabel;
					UIfocusable.MutualVerticalFocusableBind(opUpdown2, uIconfig ?? opUpdown2);
					OpLabel opLabel3 = new OpLabel(new Vector2(num2 + 110f, num3 - num), new Vector2(170f, 36f), Custom.ReplaceLineDelimeters(OptionInterface.Translate(configurable3.info.Tags[0] as string)), FLabelAlignment.Left, bigText: false, fTextParams)
					{
						bumpBehav = opUpdown2.bumpBehav,
						description = opUpdown2.description
					};
					if (flag)
					{
						opUpdown2.colorEdge = cheatColor;
						opUpdown2.colorText = cheatColor;
						opLabel3.color = cheatColor;
					}
					opTab.AddItems(opUpdown2, opLabel3);
					uIconfig = opUpdown2;
					break;
				}
				case ValueConverter.TypeCategory.Floats:
				{
					num += 36f;
					Configurable<float> configurable2 = lists[i][j] as Configurable<float>;
					OpUpdown opUpdown = new OpUpdown(configurable2, new Vector2(num2, num3 - num), 100f, (byte)((configurable2 != MMF.cfgSlowTimeFactor) ? 1 : 2))
					{
						description = OptionInterface.Translate(configurable2.info.description),
						sign = i
					};
					opUpdown.OnChange += RefreshCurrentPresetLabel;
					UIfocusable.MutualVerticalFocusableBind(opUpdown, uIconfig ?? opUpdown);
					OpLabel opLabel2 = new OpLabel(new Vector2(num2 + 110f, num3 - num), new Vector2(170f, 36f), Custom.ReplaceLineDelimeters(OptionInterface.Translate(configurable2.info.Tags[0] as string)), FLabelAlignment.Left, bigText: false, fTextParams)
					{
						bumpBehav = opUpdown.bumpBehav,
						description = opUpdown.description
					};
					if (flag)
					{
						opUpdown.colorEdge = cheatColor;
						opUpdown.colorText = cheatColor;
						opLabel2.color = cheatColor;
					}
					opTab.AddItems(opUpdown, opLabel2);
					uIconfig = opUpdown;
					break;
				}
				case ValueConverter.TypeCategory.Boolean:
				{
					num += 30f;
					Configurable<bool> configurable = lists[i][j] as Configurable<bool>;
					OpCheckBox opCheckBox = new OpCheckBox(configurable, new Vector2(num2, num3 - num))
					{
						description = OptionInterface.Translate(configurable.info.description),
						sign = i
					};
					opCheckBox.OnChange += RefreshCurrentPresetLabel;
					UIfocusable.MutualVerticalFocusableBind(opCheckBox, uIconfig ?? opCheckBox);
					OpLabel opLabel = new OpLabel(new Vector2(num2 + 40f, num3 - num), new Vector2(240f, 30f), Custom.ReplaceLineDelimeters(OptionInterface.Translate(configurable.info.Tags[0] as string)), FLabelAlignment.Left, bigText: false, fTextParams)
					{
						bumpBehav = opCheckBox.bumpBehav,
						description = opCheckBox.description
					};
					if (flag)
					{
						opCheckBox.colorEdge = cheatColor;
						opLabel.color = cheatColor;
					}
					opTab.AddItems(opCheckBox, opLabel);
					uIconfig = opCheckBox;
					if (opCheckBox.Key == MMF.cfgDisableGateKarma.key)
					{
						opCheckBox.OnChange += ConfirmDisableKarma;
					}
					break;
				}
				}
			}
			num3 -= 70f;
			if (i == 0)
			{
				num2 += 300f;
				num3 = 550f;
				num = 40f;
				uIconfig = null;
			}
		}
		for (int k = 0; k < lists.Length; k++)
		{
			if (k == 0 || k == 1)
			{
				lists[k][0].BoundUIconfig.SetNextFocusable(UIfocusable.NextDirection.Up, lists[k][0].BoundUIconfig);
			}
			if (k == 0 || k == lists.Length - 1)
			{
				lists[k][lists[k].Length - 1].BoundUIconfig.SetNextFocusable(UIfocusable.NextDirection.Down, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.SaveButton));
			}
		}
		int num4 = 0;
		for (int l = 1; l < lists.Length; l++)
		{
			for (int m = 0; m < lists[l].Length; m++)
			{
				if (lists[l][m].BoundUIconfig == null)
				{
					continue;
				}
				lists[l][m].BoundUIconfig.SetNextFocusable(UIfocusable.NextDirection.Right, lists[l][m].BoundUIconfig);
				if (num4 < lists[0].Length)
				{
					if (lists[0][num4].BoundUIconfig == null)
					{
						num4++;
						continue;
					}
					UIfocusable.MutualHorizontalFocusableBind(lists[0][num4].BoundUIconfig, lists[l][m].BoundUIconfig);
					lists[0][num4].BoundUIconfig.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
					num4++;
				}
				else
				{
					lists[l][m].BoundUIconfig.SetNextFocusable(UIfocusable.NextDirection.Left, lists[0][lists[0].Length - 1].BoundUIconfig);
				}
			}
		}
	}

	public void RefreshCurrentPresetLabel()
	{
		currentPresetLabel.text = GetCurrentPresetString();
	}

	public void PresetRemix(UIfocusable trigger)
	{
		for (int i = 0; i < MMF.boolPresets.Count; i++)
		{
			if (MMF.boolPresets[i].config.BoundUIconfig != null)
			{
				MMF.boolPresets[i].config.BoundUIconfig.value = ValueConverter.ConvertToString(MMF.boolPresets[i].remixValue);
			}
		}
		for (int j = 0; j < MMF.intPresets.Count; j++)
		{
			if (MMF.intPresets[j].config.BoundUIconfig != null)
			{
				MMF.intPresets[j].config.BoundUIconfig.value = ValueConverter.ConvertToString(MMF.intPresets[j].remixValue);
			}
		}
		for (int k = 0; k < MMF.floatPresets.Count; k++)
		{
			if (MMF.floatPresets[k].config.BoundUIconfig != null)
			{
				MMF.floatPresets[k].config.BoundUIconfig.value = ValueConverter.ConvertToString(MMF.floatPresets[k].remixValue);
			}
		}
		RefreshCurrentPresetLabel();
	}

	public void PresetClassic(UIfocusable trigger)
	{
		for (int i = 0; i < MMF.boolPresets.Count; i++)
		{
			if (MMF.boolPresets[i].config.BoundUIconfig != null)
			{
				MMF.boolPresets[i].config.BoundUIconfig.value = ValueConverter.ConvertToString(MMF.boolPresets[i].classicValue);
			}
		}
		for (int j = 0; j < MMF.intPresets.Count; j++)
		{
			if (MMF.intPresets[j].config.BoundUIconfig != null)
			{
				MMF.intPresets[j].config.BoundUIconfig.value = ValueConverter.ConvertToString(MMF.intPresets[j].classicValue);
			}
		}
		for (int k = 0; k < MMF.floatPresets.Count; k++)
		{
			if (MMF.floatPresets[k].config.BoundUIconfig != null)
			{
				MMF.floatPresets[k].config.BoundUIconfig.value = ValueConverter.ConvertToString(MMF.floatPresets[k].classicValue);
			}
		}
		RefreshCurrentPresetLabel();
	}

	public void ConfirmDisableKarma()
	{
		if (!ConfigContainer.mute && MMF.cfgDisableGateKarma.BoundUIconfig.value == ValueConverter.ConvertToString(value: true))
		{
			ConfigConnector.CreateDialogBoxYesNo(OptionInterface.Translate("This option disables multiple core mechanics of the game.") + Environment.NewLine + OptionInterface.Translate("It also largely negates the game's difficulty curve and progression.") + Environment.NewLine + OptionInterface.Translate("This may SIGNIFICANTLY alter your experience with the game.") + Environment.NewLine + " " + Environment.NewLine + OptionInterface.Translate("You should try the 'Monk-style Gates' option first before resorting to using this option.") + Environment.NewLine + " " + Environment.NewLine + OptionInterface.Translate("Are you sure you want to disable ALL karma mechanics?"), EnableDisabledKarma, DisableDisabledKarma);
		}
	}

	public void EnableDisabledKarma()
	{
		MMF.cfgDisableGateKarma.BoundUIconfig.value = ValueConverter.ConvertToString(value: true);
	}

	public void DisableDisabledKarma()
	{
		MMF.cfgDisableGateKarma.BoundUIconfig.value = ValueConverter.ConvertToString(value: false);
	}

	public void ConfirmPresetRelaxed(UIfocusable trigger)
	{
		ConfigConnector.CreateDialogBoxYesNo(OptionInterface.Translate("RELAXED preset is a significant reduction in difficulty.") + Environment.NewLine + OptionInterface.Translate("If you are having trouble, you may want to try The Monk campaign first. It may help you get a better feel for the game and ease you into Rain World.") + Environment.NewLine + " " + Environment.NewLine + OptionInterface.Translate("By comparison, the RELAXED preset may weaken the overall game experience.") + Environment.NewLine + " " + Environment.NewLine + OptionInterface.Translate("Are you sure you want to use this preset?"), PresetRelaxed);
	}

	public void PresetRelaxed()
	{
		for (int i = 0; i < MMF.boolPresets.Count; i++)
		{
			if (MMF.boolPresets[i].config.BoundUIconfig != null)
			{
				MMF.boolPresets[i].config.BoundUIconfig.value = ValueConverter.ConvertToString(MMF.boolPresets[i].casualValue);
			}
		}
		for (int j = 0; j < MMF.intPresets.Count; j++)
		{
			if (MMF.intPresets[j].config.BoundUIconfig != null)
			{
				MMF.intPresets[j].config.BoundUIconfig.value = ValueConverter.ConvertToString(MMF.intPresets[j].casualValue);
			}
		}
		for (int k = 0; k < MMF.floatPresets.Count; k++)
		{
			if (MMF.floatPresets[k].config.BoundUIconfig != null)
			{
				MMF.floatPresets[k].config.BoundUIconfig.value = ValueConverter.ConvertToString(MMF.floatPresets[k].casualValue);
			}
		}
		RefreshCurrentPresetLabel();
	}

	public bool CheckPresetRemix()
	{
		for (int i = 0; i < MMF.boolPresets.Count; i++)
		{
			if (MMF.boolPresets[i].config.BoundUIconfig != null && MMF.boolPresets[i].config.BoundUIconfig.value != ValueConverter.ConvertToString(MMF.boolPresets[i].remixValue))
			{
				return false;
			}
		}
		for (int j = 0; j < MMF.intPresets.Count; j++)
		{
			int result;
			bool num = int.TryParse(MMF.intPresets[j].config.BoundUIconfig.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			int num2 = int.Parse(ValueConverter.ConvertToString(MMF.intPresets[j].remixValue), NumberStyles.Any, CultureInfo.InvariantCulture);
			if (!num || result != num2)
			{
				return false;
			}
		}
		for (int k = 0; k < MMF.floatPresets.Count; k++)
		{
			float result2;
			bool num3 = float.TryParse(MMF.floatPresets[k].config.BoundUIconfig.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result2);
			float num4 = float.Parse(ValueConverter.ConvertToString(MMF.floatPresets[k].remixValue), NumberStyles.Any, CultureInfo.InvariantCulture);
			if (!num3 || result2 != num4)
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckPresetClassic()
	{
		for (int i = 0; i < MMF.boolPresets.Count; i++)
		{
			if (MMF.boolPresets[i].config.BoundUIconfig != null && MMF.boolPresets[i].config.BoundUIconfig.value != ValueConverter.ConvertToString(MMF.boolPresets[i].classicValue))
			{
				return false;
			}
		}
		for (int j = 0; j < MMF.intPresets.Count; j++)
		{
			int result;
			bool num = int.TryParse(MMF.intPresets[j].config.BoundUIconfig.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			int num2 = int.Parse(ValueConverter.ConvertToString(MMF.intPresets[j].classicValue), NumberStyles.Any, CultureInfo.InvariantCulture);
			if (!num || result != num2)
			{
				return false;
			}
		}
		for (int k = 0; k < MMF.floatPresets.Count; k++)
		{
			float result2;
			bool num3 = float.TryParse(MMF.floatPresets[k].config.BoundUIconfig.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result2);
			float num4 = float.Parse(ValueConverter.ConvertToString(MMF.floatPresets[k].classicValue), NumberStyles.Any, CultureInfo.InvariantCulture);
			if (!num3 || result2 != num4)
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckPresetRelaxed()
	{
		for (int i = 0; i < MMF.boolPresets.Count; i++)
		{
			if (MMF.boolPresets[i].config.BoundUIconfig != null && MMF.boolPresets[i].config.BoundUIconfig.value != ValueConverter.ConvertToString(MMF.boolPresets[i].casualValue))
			{
				return false;
			}
		}
		for (int j = 0; j < MMF.intPresets.Count; j++)
		{
			int result;
			bool num = int.TryParse(MMF.intPresets[j].config.BoundUIconfig.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			int num2 = int.Parse(ValueConverter.ConvertToString(MMF.intPresets[j].casualValue), NumberStyles.Any, CultureInfo.InvariantCulture);
			if (!num || result != num2)
			{
				return false;
			}
		}
		for (int k = 0; k < MMF.floatPresets.Count; k++)
		{
			float result2;
			bool num3 = float.TryParse(MMF.floatPresets[k].config.BoundUIconfig.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result2);
			float num4 = float.Parse(ValueConverter.ConvertToString(MMF.floatPresets[k].casualValue), NumberStyles.Any, CultureInfo.InvariantCulture);
			if (!num3 || result2 != num4)
			{
				return false;
			}
		}
		return true;
	}
}
