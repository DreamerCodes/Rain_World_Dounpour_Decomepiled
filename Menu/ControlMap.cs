using System.Linq;
using Menu.Remix;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ControlMap : PositionedMenuObject
{
	public MenuIllustration controlsMap;

	public MenuIllustration controlsMap2;

	public MenuIllustration controlsMap3;

	public FSprite controlsMapBlackFade;

	public MenuLabel[] controlLabels;

	public Vector2[] controlLabelPositions;

	public float fade;

	public float lastFade;

	public int counter;

	public FSprite pickupButtonInstructionsFade;

	public MenuLabel pickupButtonInstructions;

	public float pickupFade;

	public float lastPickupFade;

	public Options.ControlSetup.Preset preset;

	public int debugTemplateInd = -2;

	public ControlMap(Menu menu, MenuObject owner, Vector2 pos, Options.ControlSetup.Preset preset, bool showPickupInstructions)
		: base(menu, owner, pos)
	{
		controlsMapBlackFade = new FSprite("Futile_White");
		controlsMapBlackFade.shader = menu.manager.rainWorld.Shaders["FlatLight"];
		controlsMapBlackFade.color = Menu.MenuRGB(Menu.MenuColors.Black);
		Container.AddChild(controlsMapBlackFade);
		controlLabels = new MenuLabel[6];
		bool flag = true;
		if (ModManager.MSC && menu.manager.rainWorld.safariMode)
		{
			flag = false;
		}
		string text = ParseLineBreaks(menu.Translate("Pick up / Eat"));
		string text2 = ParseLineBreaks(menu.Translate("Jump"));
		string text3 = ParseLineBreaks(menu.Translate("Throw"));
		string text4 = ParseLineBreaks(menu.Translate("Map"));
		string text5 = ParseLineBreaks(menu.Translate("Move"));
		this.preset = preset;
		if (menu is ModdingMenu)
		{
			text2 = ParseLineBreaks(OptionalText.GetText(OptionalText.ID.ModdingMenu_ControlJump));
			text3 = ParseLineBreaks(OptionalText.GetText(OptionalText.ID.ModdingMenu_ControlThrow));
			text4 = ParseLineBreaks(OptionalText.GetText(OptionalText.ID.ModdingMenu_ControlMap));
			text5 = ParseLineBreaks(OptionalText.GetText(OptionalText.ID.ModdingMenu_ControlMove));
			showPickupInstructions = true;
			text = ParseLineBreaks(OptionalText.GetText(OptionalText.ID.ModdingMenu_ControlPckp));
		}
		else if (ModManager.MSC && menu.manager.rainWorld.safariMode && menu.manager.currentMainLoop is RainWorldGame)
		{
			RainWorldGame rainWorldGame = menu.manager.currentMainLoop as RainWorldGame;
			if (rainWorldGame.cameras[0].followAbstractCreature != null)
			{
				if (!rainWorldGame.cameras[0].followAbstractCreature.controlled)
				{
					text4 = ParseLineBreaks(menu.Translate("Play as target"));
					text5 = ParseLineBreaks(menu.Translate("Change target"));
					text = ParseLineBreaks(menu.Translate("Hold to view map"));
					text2 = ParseLineBreaks(menu.Translate("Warp to random creature"));
					text3 = ParseLineBreaks(menu.Translate("Hold to select and use doors"));
				}
				else
				{
					text4 = ParseLineBreaks(menu.Translate("Revoke Control"));
					text = ParseLineBreaks(TranslateSafariAction(rainWorldGame.cameras[0].followAbstractCreature.creatureTemplate.pickupAction));
					text2 = ParseLineBreaks(TranslateSafariAction(rainWorldGame.cameras[0].followAbstractCreature.creatureTemplate.jumpAction));
					text3 = ParseLineBreaks(TranslateSafariAction(rainWorldGame.cameras[0].followAbstractCreature.creatureTemplate.throwAction));
				}
			}
		}
		if (text == "")
		{
			text = "N/A";
		}
		if (text2 == "")
		{
			text2 = "N/A";
		}
		if (text3 == "")
		{
			text3 = "N/A";
		}
		if (preset != Options.ControlSetup.Preset.None || !flag)
		{
			controlLabels[0] = new MenuLabel(menu, this, text5, new Vector2(0f, 0f), new Vector2(100f, 20f), bigText: false);
			controlLabels[1] = new MenuLabel(menu, this, text4, new Vector2(0f, 0f), new Vector2(100f, 20f), bigText: false);
			controlLabels[2] = new MenuLabel(menu, this, menu.Translate("Pause"), new Vector2(0f, 0f), new Vector2(100f, 20f), bigText: false);
			controlLabels[3] = new MenuLabel(menu, this, text, new Vector2(0f, 0f), new Vector2(100f, 20f), bigText: false);
			controlLabels[4] = new MenuLabel(menu, this, text2, new Vector2(0f, 0f), new Vector2(100f, 20f), bigText: false);
			controlLabels[5] = new MenuLabel(menu, this, text3, new Vector2(0f, 0f), new Vector2(100f, 20f), bigText: false);
			controlLabelPositions = new Vector2[controlLabels.Length];
			for (int i = 0; i < controlLabels.Length; i++)
			{
				subObjects.Add(controlLabels[i]);
				controlLabels[i].label.alignment = FLabelAlignment.Left;
			}
			if (menu is ModdingMenu)
			{
				controlLabels[2].text = "N/A";
			}
		}
		if (showPickupInstructions && (!ModManager.MSC || !base.menu.manager.rainWorld.safariMode))
		{
			pickupButtonInstructionsFade = new FSprite("Futile_White");
			pickupButtonInstructionsFade.shader = menu.manager.rainWorld.Shaders["FlatLight"];
			pickupButtonInstructionsFade.color = Menu.MenuRGB(Menu.MenuColors.Black);
			pickupButtonInstructionsFade.scaleX = 30f;
			pickupButtonInstructionsFade.scaleY = 20f;
			Container.AddChild(pickupButtonInstructionsFade);
		}
		Vector2 vector = new Vector2(-200f, -150f);
		string text6 = (InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? " - " : " : ");
		if (preset == Options.ControlSetup.Preset.PS4DualShock)
		{
			controlsMap = new MenuIllustration(menu, this, "", "ps4Controller", new Vector2(-180f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap2 = new MenuIllustration(menu, this, "", "ps4Controller2", controlsMap.pos + new Vector2(-106f, 89f), crispPixels: true, anchorCenter: false);
			controlsMap3 = new MenuIllustration(menu, this, "", "ps4Controller3", controlsMap.pos + new Vector2(105f, 96f), crispPixels: true, anchorCenter: false);
			controlLabelPositions[0] = new Vector2(25f, 24f);
			controlLabels[0].text = menu.Translate("Left Stick") + " - " + controlLabels[0].text;
			controlLabelPositions[1] = new Vector2(470f, 180f);
			controlLabels[1].text = menu.Translate(flag ? "Hold R1" : "R1") + " - " + controlLabels[1].text;
			controlLabelPositions[2] = new Vector2(225f, 157f);
			controlLabels[2].text = menu.Translate("Options") + " - " + controlLabels[2].text;
			controlLabelPositions[3] = new Vector2(500f, 115f);
			controlLabels[3].text = menu.Translate("Square") + " - " + controlLabels[3].text;
			controlLabelPositions[4] = new Vector2(500f, 38f);
			controlLabels[4].text = menu.Translate("Cross") + " - " + controlLabels[4].text;
			controlLabelPositions[5] = new Vector2(525f, 74f);
			controlLabels[5].text = menu.Translate("Circle") + " - " + controlLabels[5].text;
			vector.x += 60f;
		}
		else if (preset == Options.ControlSetup.Preset.PS5DualSense)
		{
			controlsMap = new MenuIllustration(menu, this, "", "ps5Controller", new Vector2(-180f, -45f), crispPixels: true, anchorCenter: false);
			controlsMap2 = new MenuIllustration(menu, this, "", "ps5Controller2", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap3 = new MenuIllustration(menu, this, "", "ps5Controller2", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlLabelPositions[0] = new Vector2(70f, 165f);
			controlLabels[0].text = menu.Translate("Left Stick") + " - " + controlLabels[0].text;
			controlLabelPositions[1] = new Vector2(590f, 325f);
			controlLabels[1].text = menu.Translate(flag ? "Hold R1" : "R1") + " - " + controlLabels[1].text;
			controlLabelPositions[2] = new Vector2(285f, 328f);
			controlLabels[2].text = menu.Translate("Options") + " - " + controlLabels[2].text;
			controlLabelPositions[3] = new Vector2(600f, 135f);
			controlLabels[3].text = menu.Translate("Square") + " - " + controlLabels[3].text;
			controlLabelPositions[4] = new Vector2(600f, 175f);
			controlLabels[4].text = menu.Translate("Cross") + " - " + controlLabels[4].text;
			controlLabelPositions[5] = new Vector2(600f, 223f);
			controlLabels[5].text = menu.Translate("Circle") + " - " + controlLabels[5].text;
			vector.x += 60f;
			showPickupInstructions = false;
		}
		else if (preset == Options.ControlSetup.Preset.XBox)
		{
			string fileName = "xboxController_xb1";
			controlsMap = new MenuIllustration(menu, this, "", fileName, new Vector2(-172f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap2 = new MenuIllustration(menu, this, "", "xboxController2_gamecore", controlsMap.pos + new Vector2(-83f, 134f), crispPixels: true, anchorCenter: false);
			controlsMap3 = new MenuIllustration(menu, this, "", "xboxController3", controlsMap.pos + new Vector2(60f, 137f), crispPixels: true, anchorCenter: false);
			controlLabelPositions[0] = new Vector2(5f, 38f);
			controlLabels[0].text = menu.Translate("Left Stick") + " - " + controlLabels[0].text;
			controlLabelPositions[1] = new Vector2(430f, 122f);
			controlLabels[1].text = menu.Translate(flag ? "Hold RB" : "RB") + " - " + controlLabels[1].text;
			controlLabelPositions[2] = new Vector2(113f, 123f);
			controlLabels[2].text = menu.Translate("View") + " - " + controlLabels[2].text;
			controlLabelPositions[3] = new Vector2(430f, 75f);
			controlLabels[3].text = "X - " + controlLabels[3].text;
			controlLabelPositions[4] = new Vector2(430f, 3f);
			controlLabels[4].text = "A - " + controlLabels[4].text;
			controlLabelPositions[5] = new Vector2(430f, 39f);
			controlLabels[5].text = "B - " + controlLabels[5].text;
			vector.x += 60f;
		}
		else if (preset == Options.ControlSetup.Preset.SwitchHandheld)
		{
			controlsMap = new MenuIllustration(menu, this, "", "switch_handheld_controller", new Vector2(-290f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap2 = new MenuIllustration(menu, this, "", "switch_handheld_controller2", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap3 = new MenuIllustration(menu, this, "", "switch_handheld_controller3", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			SetSwitchHandheldTextOffset(menu);
			controlLabels[0].text = menu.Translate("switch_controls_movement") + text6 + controlLabels[0].text;
			controlLabelPositions[1] = new Vector2(558f, 202f);
			controlLabels[1].text = menu.Translate(flag ? "switch_controls_hold_map" : "switch_controls_map") + text6 + controlLabels[1].text;
			controlLabelPositions[2] = new Vector2(558f, 182f);
			controlLabels[2].text = menu.Translate("switch_controls_pause") + text6 + controlLabels[2].text;
			controlLabelPositions[3] = new Vector2(558f, 102f);
			controlLabels[3].text = menu.Translate("switch_controls_pickup") + text6 + controlLabels[3].text;
			controlLabelPositions[4] = new Vector2(558f, 122f);
			controlLabels[4].text = menu.Translate("switch_controls_jump") + text6 + controlLabels[4].text;
			controlLabelPositions[5] = new Vector2(558f, 142f);
			controlLabels[5].text = menu.Translate("switch_controls_throw") + text6 + controlLabels[5].text;
			vector.x += 60f;
		}
		else if (preset == Options.ControlSetup.Preset.SwitchDualJoycon)
		{
			controlsMap = new MenuIllustration(menu, this, "", "switch_dualjoycon_controller", new Vector2(-290f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap2 = new MenuIllustration(menu, this, "", "switch_dualjoycon_controller2", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap3 = new MenuIllustration(menu, this, "", "switch_dualjoycon_controller3", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlLabelPositions[0] = new Vector2(5f, 151f);
			controlLabels[0].text = menu.Translate("switch_controls_movement") + text6 + controlLabels[0].text;
			controlLabelPositions[1] = new Vector2(558f, 202f);
			controlLabels[1].text = menu.Translate(flag ? "switch_controls_hold_map" : "switch_controls_map") + text6 + controlLabels[1].text;
			controlLabelPositions[2] = new Vector2(558f, 182f);
			controlLabels[2].text = menu.Translate("switch_controls_pause") + text6 + controlLabels[2].text;
			controlLabelPositions[3] = new Vector2(558f, 102f);
			controlLabels[3].text = menu.Translate("switch_controls_pickup") + text6 + controlLabels[3].text;
			controlLabelPositions[4] = new Vector2(558f, 122f);
			controlLabels[4].text = menu.Translate("switch_controls_jump") + text6 + controlLabels[4].text;
			controlLabelPositions[5] = new Vector2(558f, 142f);
			controlLabels[5].text = menu.Translate("switch_controls_throw") + text6 + controlLabels[5].text;
			vector.x += 60f;
		}
		else if (preset == Options.ControlSetup.Preset.SwitchSingleJoyconL)
		{
			controlsMap = new MenuIllustration(menu, this, "", "switch_singlejoyconl_controller", new Vector2(-290f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap2 = new MenuIllustration(menu, this, "", "switch_singlejoyconl_controller2", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap3 = new MenuIllustration(menu, this, "", "switch_singlejoyconl_controller3", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlLabelPositions[0] = new Vector2(5f, 112f);
			controlLabels[0].text = menu.Translate("switch_controls_singlejoyconl_movement") + text6 + controlLabels[0].text;
			controlLabelPositions[1] = new Vector2(558f, 150f);
			controlLabels[1].text = menu.Translate(flag ? "switch_controls_singlejoycon_hold_map" : "switch_controls_singlejoycon_map") + text6 + controlLabels[1].text;
			controlLabelPositions[2] = new Vector2(5f, 132f);
			controlLabels[2].text = menu.Translate("switch_controls_singlejoyconl_pause") + text6 + controlLabels[2].text;
			controlLabelPositions[3] = new Vector2(558f, 60f);
			controlLabels[3].text = menu.Translate("switch_controls_singlejoyconl_pickup") + text6 + controlLabels[3].text;
			controlLabelPositions[4] = new Vector2(558f, 80f);
			controlLabels[4].text = menu.Translate("switch_controls_singlejoyconl_jump") + text6 + controlLabels[4].text;
			controlLabelPositions[5] = new Vector2(558f, 100f);
			controlLabels[5].text = menu.Translate("switch_controls_singlejoyconl_throw") + text6 + controlLabels[5].text;
			vector.x += 60f;
		}
		else if (preset == Options.ControlSetup.Preset.SwitchSingleJoyconR)
		{
			controlsMap = new MenuIllustration(menu, this, "", "switch_singlejoyconr_controller", new Vector2(-290f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap2 = new MenuIllustration(menu, this, "", "switch_singlejoyconr_controller2", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap3 = new MenuIllustration(menu, this, "", "switch_singlejoyconr_controller3", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlLabelPositions[0] = new Vector2(5f, 105f);
			controlLabels[0].text = menu.Translate("switch_controls_singlejoyconr_movement") + text6 + controlLabels[0].text;
			controlLabelPositions[1] = new Vector2(5f, 150f);
			controlLabels[1].text = menu.Translate(flag ? "switch_controls_singlejoycon_hold_map" : "switch_controls_singlejoycon_map") + text6 + controlLabels[1].text;
			controlLabelPositions[2] = new Vector2(558f, 140f);
			controlLabels[2].text = menu.Translate("switch_controls_singlejoyconr_pause") + text6 + controlLabels[2].text;
			controlLabelPositions[3] = new Vector2(558f, 60f);
			controlLabels[3].text = menu.Translate("switch_controls_singlejoyconr_pickup") + text6 + controlLabels[3].text;
			controlLabelPositions[4] = new Vector2(558f, 80f);
			controlLabels[4].text = menu.Translate("switch_controls_singlejoyconr_jump") + text6 + controlLabels[4].text;
			controlLabelPositions[5] = new Vector2(558f, 100f);
			controlLabels[5].text = menu.Translate("switch_controls_singlejoyconr_throw") + text6 + controlLabels[5].text;
			vector.x += 60f;
		}
		else if (preset == Options.ControlSetup.Preset.SwitchProController)
		{
			controlsMap = new MenuIllustration(menu, this, "", "switch_pro_controller", new Vector2(-290f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap2 = new MenuIllustration(menu, this, "", "switch_pro_controller2", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap3 = new MenuIllustration(menu, this, "", "switch_pro_controller3", controlsMap.pos + new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			controlLabelPositions[0] = new Vector2(5f, 147f);
			controlLabels[0].text = menu.Translate("switch_controls_movement") + text6 + controlLabels[0].text;
			controlLabelPositions[1] = new Vector2(558f, 202f);
			controlLabels[1].text = menu.Translate(flag ? "switch_controls_hold_map" : "switch_controls_map") + text6 + controlLabels[1].text;
			controlLabelPositions[2] = new Vector2(5f, 177f);
			controlLabels[2].text = menu.Translate("switch_controls_pause") + text6 + controlLabels[2].text;
			controlLabelPositions[3] = new Vector2(558f, 102f);
			controlLabels[3].text = menu.Translate("switch_controls_pickup") + text6 + controlLabels[3].text;
			controlLabelPositions[4] = new Vector2(558f, 122f);
			controlLabels[4].text = menu.Translate("switch_controls_jump") + text6 + controlLabels[4].text;
			controlLabelPositions[5] = new Vector2(558f, 142f);
			controlLabels[5].text = menu.Translate("switch_controls_throw") + text6 + controlLabels[5].text;
			vector.x += 60f;
		}
		else if (preset == Options.ControlSetup.Preset.KeyboardSinglePlayer || (!flag && preset == Options.ControlSetup.Preset.None))
		{
			controlsMap = new MenuIllustration(menu, this, "", "keyboard", new Vector2(-290f, 0f), crispPixels: true, anchorCenter: false);
			controlsMap2 = new MenuIllustration(menu, this, "", "keyboard2", controlsMap.pos + new Vector2(-134f, -27f), crispPixels: true, anchorCenter: false);
			controlsMap3 = new MenuIllustration(menu, this, "", "keyboard3", controlsMap.pos + new Vector2(16f, 11f), crispPixels: true, anchorCenter: false);
			controlLabelPositions[0] = new Vector2(742f, 42f);
			controlLabels[0].text = menu.Translate((preset == Options.ControlSetup.Preset.None) ? "Movement Buttons" : "Arrows") + " - " + controlLabels[0].text;
			controlLabelPositions[1] = new Vector2(300f, 1f);
			controlLabels[1].text = menu.Translate((preset == Options.ControlSetup.Preset.None) ? "Map Button" : (flag ? "Hold Space" : "Space")) + " - " + controlLabels[1].text;
			controlLabelPositions[2] = new Vector2(20f, 216f);
			controlLabels[2].text = menu.Translate((preset == Options.ControlSetup.Preset.None) ? "Pause Button" : "Esc") + " - " + controlLabels[2].text;
			controlLabelPositions[3] = new Vector2(20f, 90f);
			controlLabels[3].text = menu.Translate((preset == Options.ControlSetup.Preset.None) ? "Pick-Up Button" : "Shift") + " - " + controlLabels[3].text;
			controlLabelPositions[4] = new Vector2(20f, 45f);
			controlLabels[4].text = ((preset == Options.ControlSetup.Preset.None) ? (menu.Translate("Jump Button") + " - ") : "Z - ") + controlLabels[4].text;
			controlLabelPositions[5] = new Vector2(20f, 1f);
			controlLabels[5].text = ((preset == Options.ControlSetup.Preset.None) ? (menu.Translate("Throw Button") + " - ") : "X - ") + controlLabels[5].text;
		}
		if (controlLabels[0] != null)
		{
			for (int j = 0; j < controlLabelPositions.Length; j++)
			{
				controlLabelPositions[j].y += 1f;
			}
		}
		if (showPickupInstructions)
		{
			string text7 = "";
			if (menu is ModdingMenu)
			{
				text7 = text7 + OptionalText.GetText(OptionalText.ID.ModdingMenu_PckpTuto0) + "\r\n\r\n";
				text7 = text7 + "  - " + OptionalText.GetText(OptionalText.ID.ModdingMenu_PckpTuto1) + "\r\n";
				text7 = text7 + "  - " + OptionalText.GetText(OptionalText.ID.ModdingMenu_PckpTuto2) + "\r\n";
				text7 = text7 + "  - " + OptionalText.GetText(OptionalText.ID.ModdingMenu_PckpTuto3);
			}
			else if (!ModManager.MSC || !base.menu.manager.rainWorld.safariMode)
			{
				text7 = text7 + menu.Translate("Pick up / Eat button interactions:") + "\r\n\r\n";
				text7 = text7 + "  - " + menu.Translate("Tap to pick up objects") + "\r\n";
				text7 = text7 + "  - " + menu.Translate("Hold to eat / swallow objects") + "\r\n";
				text7 = text7 + "  - " + menu.Translate("Press while holding down direction (crouching) to drop object") + "\r\n";
				text7 = text7 + "  - " + menu.Translate("Double tap to switch hands") + "\r\n";
			}
			pickupButtonInstructions = new MenuLabel(menu, this, text7, vector, new Vector2(100f, 20f), bigText: false);
			pickupButtonInstructions.label.alignment = FLabelAlignment.Left;
			subObjects.Add(pickupButtonInstructions);
		}
		if (controlsMap != null)
		{
			subObjects.Add(controlsMap);
			subObjects.Add(controlsMap2);
			subObjects.Add(controlsMap3);
			controlsMap.color = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
			controlsMapBlackFade.scaleX = (controlsMap.size.x * 2f + 200f) / 16f;
			controlsMapBlackFade.scaleY = (controlsMap.size.y * 2f + 200f) / 16f;
			controlsMap.alpha = 0f;
			controlsMap.lastAlpha = 0f;
			controlsMap2.alpha = 0f;
			controlsMap2.lastAlpha = 0f;
			controlsMap3.alpha = 0f;
			controlsMap3.lastAlpha = 0f;
		}
	}

	private void SetSwitchHandheldTextOffset(Menu menu)
	{
		if (menu.manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.French || menu.manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Spanish)
		{
			controlLabelPositions[0] = new Vector2(-45f, 151f);
		}
		else if (menu.manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.German || menu.manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Italian)
		{
			controlLabelPositions[0] = new Vector2(-25f, 151f);
		}
		else if (menu.manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Japanese || menu.manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Korean || menu.manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese)
		{
			controlLabelPositions[0] = new Vector2(-65f, 151f);
		}
		else if (menu.manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Portuguese)
		{
			controlLabelPositions[0] = new Vector2(-50f, 151f);
		}
		else
		{
			controlLabelPositions[0] = new Vector2(5f, 151f);
		}
	}

	public string TranslateSafariAction(string action)
	{
		if (action == null)
		{
			return "";
		}
		if (action.Contains("/"))
		{
			string[] array = action.Split('/');
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = menu.Translate(array[i]);
			}
			return string.Join("/", array);
		}
		return menu.Translate(action);
	}

	public string ParseLineBreaks(string s)
	{
		if (preset != Options.ControlSetup.Preset.KeyboardSinglePlayer && preset != Options.ControlSetup.Preset.None)
		{
			return s.Replace("<LINE>", "");
		}
		return Custom.ReplaceLineDelimeters(s);
	}

	public override void Update()
	{
		base.Update();
		counter++;
		lastFade = fade;
		if (menu is PauseMenu)
		{
			fade = (menu as PauseMenu).blackFade;
		}
		else if (menu is TutorialControlsPage)
		{
			fade = (menu as TutorialControlsPage).blackFade * (1f - (menu as TutorialControlsPage).inGameObject.room.game.cameras[0].hud.map.fade);
			if ((menu as TutorialControlsPage).inGameObject.room.game.pauseMenu != null)
			{
				fade = 0f;
				lastFade = 0f;
				if (controlsMap != null)
				{
					controlsMap.alpha = 0f;
					controlsMap2.alpha = 0f;
					controlsMap3.alpha = 0f;
					controlsMap.lastAlpha = 0f;
					controlsMap2.lastAlpha = 0f;
					controlsMap3.lastAlpha = 0f;
				}
				GrafUpdate(1f);
			}
		}
		else if (menu is ModdingMenu)
		{
			fade = (menu as ModdingMenu)._blackFade;
		}
		if (ModManager.MSC && preset == Options.ControlSetup.Preset.None && controlsMap != null)
		{
			controlsMap.alpha = 0f;
			controlsMap2.alpha = 0f;
			controlsMap3.alpha = 0f;
			controlsMap.lastAlpha = 0f;
			controlsMap2.lastAlpha = 0f;
			controlsMap3.lastAlpha = 0f;
		}
		else
		{
			float num = 1f;
			if (ModManager.MSC && menu.manager.rainWorld.safariMode)
			{
				num = 0.5f;
			}
			if (controlsMap != null)
			{
				controlsMap.setAlpha = fade * num;
				controlsMap2.setAlpha = Mathf.Min(fade, Custom.SCurve(Mathf.InverseLerp(5f, 80f, counter), 0.8f)) * num;
				controlsMap3.setAlpha = Mathf.Min(fade, Custom.SCurve(Mathf.InverseLerp(5f, 80f, counter), 0.8f)) * 0.5f * num;
			}
		}
		if (pickupButtonInstructions != null)
		{
			lastPickupFade = pickupFade;
			pickupFade = Custom.SCurve(Mathf.InverseLerp(40f, 120f, counter), 0.5f);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = Custom.SCurve(Mathf.Lerp(lastFade, fade, timeStacker), 0.6f);
		if (controlsMap != null)
		{
			controlsMapBlackFade.x = controlsMap.DrawX(timeStacker) + controlsMap.size.x * 0.5f;
			controlsMapBlackFade.y = controlsMap.DrawY(timeStacker) + controlsMap.size.y * 0.5f;
			controlsMapBlackFade.alpha = num * 0.5f;
			for (int i = 0; i < controlLabels.Length; i++)
			{
				controlLabels[i].label.x = controlsMap2.DrawX(timeStacker) + controlLabelPositions[i].x;
				controlLabels[i].label.y = controlsMap2.DrawY(timeStacker) + controlLabelPositions[i].y + 6f;
				if (preset == Options.ControlSetup.Preset.KeyboardSinglePlayer)
				{
					if (i >= 3 && i <= 5)
					{
						controlLabels[i].label.x -= 80f;
					}
					else if (i == 1)
					{
						controlLabels[i].label.y -= 20f;
					}
					else
					{
						controlLabels[i].label.x -= 20f;
					}
				}
				else if (preset == Options.ControlSetup.Preset.SwitchProController)
				{
					switch (i)
					{
					case 1:
						controlLabels[i].label.x -= 120f;
						break;
					case 3:
					case 4:
					case 5:
						controlLabels[i].label.x -= 100f;
						break;
					}
					if (i == 0 || i == 2)
					{
						controlLabels[i].label.x -= 30f;
					}
				}
				else if (preset == Options.ControlSetup.Preset.XBox || preset == Options.ControlSetup.Preset.PS4DualShock || preset == Options.ControlSetup.Preset.PS5DualSense)
				{
					switch (i)
					{
					case 0:
						controlLabels[i].label.x -= 70f;
						break;
					case 1:
						controlLabels[i].label.x -= 60f;
						break;
					case 2:
						controlLabels[i].label.x -= 20f;
						break;
					}
				}
				if (InGameTranslator.LanguageID.UsesLargeFont(menu.manager.rainWorld.inGameTranslator.currentLanguage) && preset != Options.ControlSetup.Preset.SwitchProController && (!(preset == Options.ControlSetup.Preset.KeyboardSinglePlayer) || i != 1))
				{
					controlLabels[i].label.y += 2f;
				}
				if (controlLabels[i].text.Count((char f) => f == '\n') > 0)
				{
					controlLabels[i].label.y += 15f;
				}
				if (ModManager.MSC && (preset == Options.ControlSetup.Preset.None || menu.manager.rainWorld.safariMode))
				{
					controlLabels[i].label.alpha = num;
				}
				else
				{
					controlLabels[i].label.alpha = Mathf.Lerp(controlsMap2.lastAlpha, controlsMap2.alpha, timeStacker);
				}
			}
		}
		if (pickupButtonInstructions != null && pickupButtonInstructionsFade != null)
		{
			pickupButtonInstructionsFade.x = pickupButtonInstructions.DrawX(timeStacker) + 150f;
			pickupButtonInstructionsFade.y = pickupButtonInstructions.DrawY(timeStacker);
			pickupButtonInstructionsFade.alpha = 0.5f * num * Mathf.Lerp(lastPickupFade, pickupFade, timeStacker);
			pickupButtonInstructions.label.alpha = num * Mathf.Lerp(lastPickupFade, pickupFade, timeStacker);
		}
	}

	public override void RemoveSprites()
	{
		controlsMapBlackFade.RemoveFromContainer();
		if (pickupButtonInstructionsFade != null)
		{
			pickupButtonInstructionsFade.RemoveFromContainer();
		}
		base.RemoveSprites();
	}
}
