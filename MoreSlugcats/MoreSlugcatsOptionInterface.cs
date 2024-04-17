using System;
using System.Collections.Generic;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class MoreSlugcatsOptionInterface : OptionInterface
{
	private const int ASSIST_TAB = 0;

	private const int CHEAT_TAB = 1;

	private const float gxo = 20f;

	private float gyo;

	private const float yoi = 30f;

	private const float yoi_big = 35f;

	private Color cheatColor;

	private OpLabel homeTitle;

	private OpLabel homeLabel;

	private OpHoldButton homeHold;

	private Configurable<bool>[] boolConfigOrder;

	private Configurable<int>[] intConfigOrder;

	private Configurable<bool>[] firstCheatBoolConfigOrder;

	private Configurable<bool>[] secondCheatBoolConfigOrder;

	private Configurable<bool>[] thirdCheatBoolConfigOrder;

	public override void Initialize()
	{
		base.Initialize();
		cheatColor = new Color(0.85f, 0.35f, 0.4f);
		Tabs = new OpTab[2]
		{
			new OpTab(this, OptionInterface.Translate("Assists")),
			new OpTab(this, OptionInterface.Translate("Cheats"))
			{
				colorButton = cheatColor
			}
		};
		if (boolConfigOrder == null)
		{
			PopulateConfigurableArrays();
		}
		float num = (float)boolConfigOrder.Length * 30f + (float)intConfigOrder.Length * 35f + 60f;
		gyo = 560f - num;
		OpLabel opLabel = new OpLabel(new Vector2(150f, gyo + num - 40f), new Vector2(300f, 30f), OptionInterface.Translate("Gameplay Assists"), FLabelAlignment.Center, bigText: true);
		Tabs[0].AddItems(opLabel);
		PopulateWithConfigs(0, boolConfigOrder, intConfigOrder);
		homeTitle = new OpLabel(new Vector2(150f, 520f), new Vector2(300f, 30f), OptionInterface.Translate("More Slugcats Cheats"), FLabelAlignment.Center, bigText: true)
		{
			color = cheatColor
		};
		Tabs[1].AddItems(homeTitle);
		homeLabel = new OpLabel(new Vector2(150f, 330f), new Vector2(300f, 30f), Custom.ReplaceLineDelimeters(OptionInterface.Translate("This menu contains cheats to unlock content early.") + Environment.NewLine + OptionInterface.Translate("It is not recommended to use these on a first playthrough.") + Environment.NewLine + OptionInterface.Translate("Are you sure you want to continue?")))
		{
			color = cheatColor
		};
		Tabs[1].AddItems(homeLabel);
		homeHold = new OpHoldButton(new Vector2(250f, 270f), new Vector2(120f, 30f), OptionInterface.Translate("CONTINUE"))
		{
			colorEdge = cheatColor
		};
		homeHold.OnPressDone += CreateFirstCheatLayer;
		Tabs[1].AddItems(homeHold);
	}

	public void PopulateConfigurableArrays()
	{
		boolConfigOrder = new Configurable<bool>[4]
		{
			MoreSlugcats.cfgDisablePrecycles,
			MoreSlugcats.cfgDisablePrecycleFloods,
			MoreSlugcats.cfgArtificerCorpseMaxKarma,
			MoreSlugcats.cfgArtificerCorpseNoKarmaLoss
		};
		intConfigOrder = new Configurable<int>[1] { MoreSlugcats.cfgArtificerExplosionCapacity };
		firstCheatBoolConfigOrder = new Configurable<bool>[1] { MoreSlugcats.chtUnlockCampaigns };
		secondCheatBoolConfigOrder = new Configurable<bool>[5]
		{
			MoreSlugcats.chtUnlockClasses,
			MoreSlugcats.chtUnlockArenas,
			MoreSlugcats.chtUnlockCreatures,
			MoreSlugcats.chtUnlockItems,
			MoreSlugcats.chtUnlockSafari
		};
		List<Configurable<bool>> list = new List<Configurable<bool>>
		{
			MoreSlugcats.chtUnlockChallenges,
			MoreSlugcats.chtUnlockCollections,
			MoreSlugcats.chtUnlockOuterExpanse,
			MoreSlugcats.chtUnlockSlugpups
		};
		if (Custom.rainWorld.options.DeveloperCommentaryLocalized())
		{
			list.Add(MoreSlugcats.chtUnlockDevCommentary);
		}
		thirdCheatBoolConfigOrder = list.ToArray();
	}

	public override string ValidationString()
	{
		if (boolConfigOrder == null)
		{
			PopulateConfigurableArrays();
		}
		string text = "[" + base.mod.id + "]  ";
		int num = 0;
		for (int i = 0; i < boolConfigOrder.Length; i++)
		{
			num += (int)Mathf.Pow(2f, i % 4) * (boolConfigOrder[i].Value ? 1 : 0);
			if (i % 4 == 3 || i == boolConfigOrder.Length - 1)
			{
				text += num.ToString("X");
				num = 0;
			}
		}
		for (int j = 0; j < intConfigOrder.Length; j++)
		{
			text = text + " " + intConfigOrder[j].Value;
		}
		text += " ";
		Configurable<bool>[] array = new Configurable<bool>[firstCheatBoolConfigOrder.Length + secondCheatBoolConfigOrder.Length + thirdCheatBoolConfigOrder.Length];
		firstCheatBoolConfigOrder.CopyTo(array, 0);
		secondCheatBoolConfigOrder.CopyTo(array, firstCheatBoolConfigOrder.Length);
		thirdCheatBoolConfigOrder.CopyTo(array, firstCheatBoolConfigOrder.Length + secondCheatBoolConfigOrder.Length);
		for (int k = 0; k < array.Length; k++)
		{
			num += (int)Mathf.Pow(2f, k % 4) * (array[k].Value ? 1 : 0);
			if (k % 4 == 3 || k == array.Length - 1)
			{
				text += num.ToString("X");
				num = 0;
			}
		}
		if (Custom.rainWorld.options.commentary && Custom.rainWorld.options.DeveloperCommentaryLocalized())
		{
			text += " commentary";
		}
		return text;
	}

	public void CreateFirstCheatLayer(UIfocusable trigger)
	{
		OpTab.DestroyItems(homeLabel, homeHold);
		gyo = 460f;
		PopulateCheatConfigs(1, firstCheatBoolConfigOrder);
		homeLabel = new OpLabel(new Vector2(150f, gyo - (float)firstCheatBoolConfigOrder.Length * 30f - 60f), new Vector2(300f, 30f), Custom.ReplaceLineDelimeters(OptionInterface.Translate("The following cheats unlock content you would normally unlock via finding unlock tokens.") + Environment.NewLine + OptionInterface.Translate("Are you sure you want to continue?")))
		{
			color = cheatColor
		};
		Tabs[1].AddItems(homeLabel);
		homeHold = new OpHoldButton(new Vector2(250f, gyo - (float)firstCheatBoolConfigOrder.Length * 30f - 120f), new Vector2(120f, 30f), OptionInterface.Translate("CONTINUE"))
		{
			colorEdge = cheatColor
		};
		homeHold.OnPressDone += CreateSecondCheatLayer;
		Tabs[1].AddItems(homeHold);
		ConfigConnector.FocusNewElement(homeHold);
	}

	public void CreateSecondCheatLayer(UIfocusable trigger)
	{
		OpTab.DestroyItems(homeLabel, homeHold);
		gyo -= (float)firstCheatBoolConfigOrder.Length * 30f;
		PopulateCheatConfigs(1, secondCheatBoolConfigOrder);
		homeLabel = new OpLabel(new Vector2(150f, gyo - (float)secondCheatBoolConfigOrder.Length * 30f - 60f), new Vector2(300f, 30f), Custom.ReplaceLineDelimeters(OptionInterface.Translate("The following cheats contain spoilers.") + Environment.NewLine + OptionInterface.Translate("Are you sure you want to view these?")))
		{
			color = cheatColor
		};
		Tabs[1].AddItems(homeLabel);
		homeHold = new OpHoldButton(new Vector2(250f, gyo - (float)secondCheatBoolConfigOrder.Length * 30f - 120f), new Vector2(120f, 30f), OptionInterface.Translate("CONTINUE"))
		{
			colorEdge = cheatColor
		};
		homeHold.OnPressDone += CreateThirdCheatLayer;
		Tabs[1].AddItems(homeHold);
		ConfigConnector.FocusNewElement(homeHold);
	}

	public void CreateThirdCheatLayer(UIfocusable trigger)
	{
		OpTab.DestroyItems(homeLabel, homeHold);
		gyo -= (float)secondCheatBoolConfigOrder.Length * 30f;
		PopulateCheatConfigs(1, thirdCheatBoolConfigOrder);
		ConfigConnector.FocusNewElement(thirdCheatBoolConfigOrder[0].BoundUIconfig);
	}

	public void ConfirmNoShelterFailures()
	{
		if (!ConfigContainer.mute && MoreSlugcats.cfgDisablePrecycles.BoundUIconfig.value == ValueConverter.ConvertToString(value: true))
		{
			ConfigConnector.CreateDialogBoxYesNo(OptionInterface.Translate("This option may actually make Rivulet's campaign harder.") + Environment.NewLine + OptionInterface.Translate("Shelter failures give periodic reprieves by providing longer cycle times.") + Environment.NewLine + " " + Environment.NewLine + OptionInterface.Translate("Are you sure you want to disable this mechanic?"), EnableNoShelterFailures, DisableNoShelterFailures);
		}
	}

	public void EnableNoShelterFailures()
	{
		MoreSlugcats.cfgDisablePrecycles.BoundUIconfig.value = ValueConverter.ConvertToString(value: true);
	}

	public void DisableNoShelterFailures()
	{
		MoreSlugcats.cfgDisablePrecycles.BoundUIconfig.value = ValueConverter.ConvertToString(value: false);
	}

	public void ConfirmNoCorpseDecay()
	{
		if (!ConfigContainer.mute && MoreSlugcats.cfgArtificerCorpseNoKarmaLoss.BoundUIconfig.value == ValueConverter.ConvertToString(value: true))
		{
			ConfigConnector.CreateDialogBoxYesNo(OptionInterface.Translate("This option significantly decreases the difficulty of Artificer's campaign.") + Environment.NewLine + OptionInterface.Translate("The intended gameplay loop is to have to actively hunt fresh corpses.") + Environment.NewLine + " " + Environment.NewLine + OptionInterface.Translate("It is recommended to use this only as a last resort.") + Environment.NewLine + " " + Environment.NewLine + OptionInterface.Translate("Are you sure you want to disable this mechanic?"), EnableNoCorpseDecay, DisableNoCorpseDecay);
		}
	}

	public void EnableNoCorpseDecay()
	{
		MoreSlugcats.cfgArtificerCorpseNoKarmaLoss.BoundUIconfig.value = ValueConverter.ConvertToString(value: true);
	}

	public void DisableNoCorpseDecay()
	{
		MoreSlugcats.cfgArtificerCorpseNoKarmaLoss.BoundUIconfig.value = ValueConverter.ConvertToString(value: false);
	}

	public void PopulateCheatConfigs(int tabIndex, Configurable<bool>[] boolConfigs)
	{
		float num = 0f;
		UIfocusable uIfocusable = null;
		for (int i = 0; i < boolConfigs.Length; i++)
		{
			OpCheckBox opCheckBox = new OpCheckBox(boolConfigs[i], new Vector2(20f, gyo + num))
			{
				colorEdge = cheatColor
			};
			opCheckBox.description = OptionInterface.Translate(boolConfigs[i].info.description);
			opCheckBox.ShowConfig();
			if (uIfocusable != null)
			{
				UIfocusable.MutualVerticalFocusableBind(opCheckBox, uIfocusable);
			}
			opCheckBox.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
			opCheckBox.SetNextFocusable(UIfocusable.NextDirection.Right, opCheckBox);
			uIfocusable = opCheckBox;
			Tabs[tabIndex].AddItems(opCheckBox);
			Tabs[tabIndex].AddItems(new OpLabel(60f, gyo + num, OptionInterface.Translate(boolConfigs[i].info.Tags[0] as string))
			{
				color = cheatColor
			});
			num -= 30f;
		}
	}

	public void PopulateWithConfigs(int tabIndex, Configurable<bool>[] boolConfigs, Configurable<int>[] intConfigs)
	{
		float num = 0f;
		UIfocusable uIfocusable = null;
		if (intConfigs != null)
		{
			for (int num2 = intConfigs.Length - 1; num2 >= 0; num2--)
			{
				OpUpdown opUpdown = new OpUpdown(intConfigs[num2], new Vector2(20f, gyo + num), 100f);
				opUpdown.description = OptionInterface.Translate(intConfigs[num2].info.description);
				if (uIfocusable != null)
				{
					UIfocusable.MutualVerticalFocusableBind(uIfocusable, opUpdown);
				}
				opUpdown.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
				opUpdown.SetNextFocusable(UIfocusable.NextDirection.Right, opUpdown);
				Tabs[tabIndex].AddItems(opUpdown);
				uIfocusable = opUpdown;
				Tabs[tabIndex].AddItems(new OpLabel(130f, gyo + num, OptionInterface.Translate(intConfigs[num2].info.Tags[0] as string))
				{
					bumpBehav = opUpdown.bumpBehav,
					description = opUpdown.description
				});
				num += 35f;
			}
		}
		if (boolConfigs == null)
		{
			return;
		}
		for (int num3 = boolConfigs.Length - 1; num3 >= 0; num3--)
		{
			OpCheckBox opCheckBox = ((boolConfigs[num3] != MoreSlugcats.cfgArtificerCorpseNoKarmaLoss) ? new OpCheckBox(boolConfigs[num3], new Vector2(20f, gyo + num)) : new OpCheckBox(boolConfigs[num3], new Vector2(20f, gyo + num))
			{
				colorEdge = cheatColor
			});
			opCheckBox.description = OptionInterface.Translate(boolConfigs[num3].info.description);
			if (uIfocusable != null)
			{
				UIfocusable.MutualVerticalFocusableBind(uIfocusable, opCheckBox);
			}
			opCheckBox.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
			opCheckBox.SetNextFocusable(UIfocusable.NextDirection.Right, opCheckBox);
			Tabs[tabIndex].AddItems(opCheckBox);
			uIfocusable = opCheckBox;
			if (boolConfigs[num3] == MoreSlugcats.cfgArtificerCorpseNoKarmaLoss)
			{
				Tabs[tabIndex].AddItems(new OpLabel(60f, gyo + num, OptionInterface.Translate(boolConfigs[num3].info.Tags[0] as string))
				{
					bumpBehav = opCheckBox.bumpBehav,
					description = opCheckBox.description,
					color = cheatColor
				});
			}
			else
			{
				Tabs[tabIndex].AddItems(new OpLabel(60f, gyo + num, OptionInterface.Translate(boolConfigs[num3].info.Tags[0] as string))
				{
					bumpBehav = opCheckBox.bumpBehav,
					description = opCheckBox.description
				});
			}
			num += 30f;
			if (opCheckBox.Key == MoreSlugcats.cfgArtificerCorpseNoKarmaLoss.key)
			{
				opCheckBox.OnChange += ConfirmNoCorpseDecay;
			}
			if (opCheckBox.Key == MoreSlugcats.cfgDisablePrecycles.key)
			{
				opCheckBox.OnChange += ConfirmNoShelterFailures;
			}
		}
	}
}
