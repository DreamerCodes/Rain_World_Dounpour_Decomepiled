using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Rewired;
using RWCustom;
using UnityEngine;

namespace Menu;

public class InputOptionsMenu : Menu, SelectOneButton.SelectOneButtonOwner, CheckBox.IOwnCheckBox
{
	public class PlayerButton : SelectOneButton
	{
		public int index;

		public MenuIllustration portrait;

		private float portraitBlack = 1f;

		private float lastPortraitBlack = 1f;

		private bool lastInput;

		public FSprite lineSprite;

		public FSprite arrowSprite;

		public float pointPos;

		public float lastPointPos;

		public int lineSegs;

		public float lastArrowFade;

		public float arrowFade;

		public Vector2 originalPos;

		public PlayerButton(Menu menu, MenuObject owner, Vector2 pos, PlayerButton[] array, int index)
			: base(menu, owner, menu.Translate("Player") + " " + (index + 1), "PlayerButtons", pos, new Vector2(100f, 100f), array, index)
		{
			this.index = index;
			originalPos = new Vector2(Mathf.Floor(pos.x) + 0.01f, Mathf.Floor(pos.y) + 0.01f);
			roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
			subObjects.Add(roundedRect);
			selectRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: false);
			subObjects.Add(selectRect);
			portrait = new MenuIllustration(menu, this, "", "MultiplayerPortrait" + index + "1", size / 2f, crispPixels: true, anchorCenter: true);
			subObjects.Add(portrait);
			lineSegs = 100;
			lineSprite = TriangleMesh.MakeLongMesh(lineSegs, pointyTip: false, customColor: false);
			Container.AddChild(lineSprite);
			arrowSprite = new FSprite("Multiplayer_Arrow");
			arrowSprite.rotation = -90f;
			Container.AddChild(arrowSprite);
			menuLabel.pos.y -= 65f;
			handleSelectedColInChild = true;
		}

		public override void Update()
		{
			base.Update();
			pos = originalPos + new Vector2(((menu.manager.rainWorld.screenSize.x == 1024f) ? 50f : 150f) * (menu as InputOptionsMenu).inputTesterHolder.darkness, 0f);
			lastPointPos = pointPos;
			lastArrowFade = arrowFade;
			arrowFade = Mathf.InverseLerp(0.5f, 0f, (menu as InputOptionsMenu).inputTesterHolder.darkness);
			if (base.AmISelected)
			{
				if (menu.selectedObject == null || !(menu.selectedObject is SelectOneButton) || (menu.selectedObject as SelectOneButton).signalText != signalText)
				{
					buttonBehav.col = 1f;
				}
				selectedCol = Custom.LerpAndTick(selectedCol, 1f, 0.06f, 0.05f);
			}
			else if (Selected)
			{
				selectedCol = Custom.LerpAndTick(selectedCol, 1f, 0.06f, 0.05f);
			}
			else
			{
				selectedCol = Custom.LerpAndTick(selectedCol, 0f, 0.06f, 0.05f);
			}
			lastPortraitBlack = portraitBlack;
			float to = 1f;
			if (Selected)
			{
				to = 0f;
			}
			else if (base.AmISelected)
			{
				to = ((!(menu.selectedObject is SelectOneButton) || !((menu.selectedObject as SelectOneButton).signalText == signalText)) ? 0f : 0.5f);
			}
			float num = IdealPointHeight();
			pointPos = Custom.LerpAndTick(pointPos, num, 0.08f, Custom.LerpMap(Mathf.Abs(pointPos - num), 60f, 120f, 0.5f, 12f));
			portraitBlack = Custom.LerpAndTick(portraitBlack, to, 0.06f, 0.025f);
		}

		public float IdealPointHeight()
		{
			int deviceIndex = GetDeviceIndex();
			float num = (menu as InputOptionsMenu).deviceButtons[deviceIndex].pos.y + (menu as InputOptionsMenu).deviceButtons[deviceIndex].size.y / 2f;
			bool flag = false;
			for (int i = 0; i < (menu as InputOptionsMenu).playerButtons.Length; i++)
			{
				if (i != index && (menu as InputOptionsMenu).playerButtons[i].GetDeviceIndex() == deviceIndex)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				List<int> list = new List<int>();
				for (int j = 0; j < (menu as InputOptionsMenu).playerButtons.Length; j++)
				{
					if ((menu as InputOptionsMenu).playerButtons[j].GetDeviceIndex() == deviceIndex)
					{
						list.Add(j);
					}
				}
				int num2 = list.IndexOf(index);
				num += Custom.LerpMap(num2, 0f, list.Count - 1, 7.5f * (float)list.Count, -7.5f * (float)list.Count);
			}
			return num;
		}

		public int GetDeviceIndex()
		{
			if (menu.manager.rainWorld.options.controls[index].GetControlPreference() == Options.ControlSetup.ControlToUse.SPECIFIC_GAMEPAD)
			{
				return Math.Min(menu.manager.rainWorld.options.controls[index].usingGamePadNumber + 2, (menu as InputOptionsMenu).deviceButtons.Length - 1);
			}
			if (menu.manager.rainWorld.options.controls[index].GetControlPreference() == Options.ControlSetup.ControlToUse.ANY)
			{
				return 0;
			}
			return 1;
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			portrait.sprite.color = Color.Lerp(Color.white, Color.black, Custom.SCurve(Mathf.Lerp(lastPortraitBlack, portraitBlack, timeStacker), 0.5f) * 0.75f);
			Vector2 vector = outerRect.DrawPos(timeStacker) + new Vector2(outerRect.DrawSize(timeStacker).x + Mathf.Lerp(outerRect.lastAddSize.x, outerRect.addSize.x, timeStacker) / 2f, outerRect.DrawSize(timeStacker).y / 2f);
			Vector2 vector2 = new Vector2((menu as InputOptionsMenu).deviceButtons[GetDeviceIndex()].DrawX(timeStacker) - 15f, Mathf.Lerp(lastPointPos, pointPos, timeStacker));
			Vector2 cA = vector + new Vector2(20f + Mathf.Abs(vector.y - vector2.y) / 4f, 0f);
			Vector2 cB = vector2 + new Vector2(-20f - Mathf.Abs(vector.y - vector2.y) / 4f, 0f);
			arrowSprite.x = vector2.x;
			arrowSprite.y = vector2.y;
			vector2.x += 1f;
			Vector2 vector3 = vector;
			vector2.x -= 5f;
			float num = Mathf.Lerp(lastArrowFade, arrowFade, timeStacker);
			if (num > 0f)
			{
				lineSprite.isVisible = true;
				arrowSprite.isVisible = true;
				Color color = Color.Lerp(Menu.MenuRGB(MenuColors.Black), MyColor(timeStacker), Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, num), 4f));
				lineSprite.color = color;
				arrowSprite.color = color;
				lineSprite.alpha = num;
				arrowSprite.alpha = num;
				float num2 = 1f + Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
				for (int i = 0; i < lineSegs; i++)
				{
					float f = Mathf.InverseLerp(0f, lineSegs - 1, i);
					Vector2 vector4 = Custom.Bezier(vector, cA, vector2, cB, f);
					Vector2 normalized = (vector3 - vector4).normalized;
					Vector2 vector5 = Custom.PerpendicularVector(normalized);
					float num3 = Vector2.Distance(vector3, vector4) / 5f;
					(lineSprite as TriangleMesh).MoveVertice(i * 4, vector3 - normalized * num3 - vector5 * num2);
					(lineSprite as TriangleMesh).MoveVertice(i * 4 + 1, vector3 - normalized * num3 + vector5 * num2);
					(lineSprite as TriangleMesh).MoveVertice(i * 4 + 2, vector4 + normalized * num3 - vector5 * num2);
					(lineSprite as TriangleMesh).MoveVertice(i * 4 + 3, vector4 + normalized * num3 + vector5 * num2);
					vector3 = vector4;
				}
			}
			else
			{
				lineSprite.isVisible = false;
				arrowSprite.isVisible = false;
			}
		}

		public override void RemoveSprites()
		{
			arrowSprite.RemoveFromContainer();
			lineSprite.RemoveFromContainer();
			base.RemoveSprites();
		}
	}

	public class DeviceButton : SelectOneButton
	{
		public int index;

		public MenuIllustration deviceImage;

		public MenuIllustration numberImage;

		private float showSideLabel;

		private float lastShowSideLabel;

		private float extraSelectCol;

		private float lastExtraSelectCol;

		public FSprite darkFade;

		private string displayName;

		public DeviceButton(Menu menu, MenuObject owner, Vector2 pos, string displayName, DeviceButton[] array, int index)
			: base(menu, owner, displayName, "DeviceButtons", pos, new Vector2(120f, 80f), array, index)
		{
			this.index = index;
			this.displayName = displayName;
			roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
			subObjects.Add(roundedRect);
			selectRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: false);
			subObjects.Add(selectRect);
			if (index != 0)
			{
				deviceImage = new MenuIllustration(menu, this, "", (index == 1) ? "KeyboardIcon" : "GamepadIcon", size / 2f, crispPixels: true, anchorCenter: true);
				subObjects.Add(deviceImage);
			}
			menuLabel.pos.x += 75f;
			menuLabel.label.alignment = FLabelAlignment.Left;
			if (index != 1)
			{
				darkFade = new FSprite("Futile_White");
				darkFade.shader = menu.manager.rainWorld.Shaders["FlatLight"];
				darkFade.color = new Color(0f, 0f, 0f);
				Container.AddChild(darkFade);
				numberImage = new MenuIllustration(menu, this, "", (index == 0) ? "GamepadAny" : ("Gamepad" + (index - 1)), size / 2f, crispPixels: true, anchorCenter: true);
				subObjects.Add(numberImage);
			}
			handleSelectedColInChild = true;
		}

		public override void Update()
		{
			base.Update();
			lastShowSideLabel = showSideLabel;
			lastExtraSelectCol = extraSelectCol;
			if (index > 1 && index - 2 < (menu as InputOptionsMenu).currentGamepads.Count)
			{
				showSideLabel = Custom.LerpAndTick(showSideLabel, 1f, 0.08f, 0.1f);
				menuLabel.text = (menu as InputOptionsMenu).currentGamepads[index - 2];
			}
			else
			{
				menuLabel.text = displayName;
				if (Selected)
				{
					showSideLabel = Custom.LerpAndTick(showSideLabel, 1f, 0.08f, 0.1f);
				}
				else
				{
					showSideLabel = Custom.LerpAndTick(showSideLabel, 0f, 0.04f, 1f / 30f);
				}
			}
			if (base.AmISelected)
			{
				extraSelectCol = Custom.LerpAndTick(extraSelectCol, 1f, 0.08f, 0.1f);
			}
			else
			{
				extraSelectCol = Custom.LerpAndTick(extraSelectCol, 0f, 0.04f, 1f / 30f);
			}
			if (base.AmISelected)
			{
				if (menu.selectedObject == null || !(menu.selectedObject is SelectOneButton) || (menu.selectedObject as SelectOneButton).signalText != signalText)
				{
					buttonBehav.col = 1f;
				}
				selectedCol = Custom.LerpAndTick(selectedCol, 1f, 0.06f, 0.05f);
			}
			else if (Selected)
			{
				selectedCol = Custom.LerpAndTick(selectedCol, 1f, 0.06f, 0.05f);
			}
			else
			{
				selectedCol = Custom.LerpAndTick(selectedCol, 0f, 0.06f, 0.05f);
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			float num = Mathf.Lerp(lastExtraSelectCol, extraSelectCol, timeStacker);
			float num2 = Mathf.Lerp(lastSelectedCol, selectedCol, timeStacker);
			float num3 = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
			base.GrafUpdate(timeStacker);
			if (deviceImage != null)
			{
				deviceImage.sprite.color = Color.Lerp(Menu.MenuRGB(MenuColors.VeryDarkGrey), Color.Lerp(Menu.MenuRGB(MenuColors.DarkGrey), Menu.MenuRGB(MenuColors.MediumGrey), num), Mathf.Max(num3, num2));
			}
			menuLabel.label.color = MyColor(timeStacker);
			menuLabel.label.alpha = Mathf.Lerp(lastShowSideLabel, showSideLabel, timeStacker);
			float t = Mathf.Max(num, 1f * num3 - 0.5f * num2);
			if (darkFade != null)
			{
				darkFade.x = DrawX(timeStacker) + DrawSize(timeStacker).x / 2f;
				darkFade.y = DrawY(timeStacker) + DrawSize(timeStacker).y / 2f;
				if (index == 1)
				{
					darkFade.scaleX = Mathf.Lerp(90f, 140f, t) * (1f + 0.35f * num) / 16f;
					darkFade.scaleY = Mathf.Lerp(50f, 70f, t) * (1f + 0.35f * num) / 16f;
				}
				else
				{
					darkFade.scale = Mathf.Lerp(70f, 110f, t) * (1f + 0.35f * num) / 16f;
				}
				darkFade.alpha = Mathf.Lerp(0.5f, 0.85f, t);
			}
			if (numberImage != null)
			{
				numberImage.sprite.color = Color.Lerp(Menu.MenuRGB(MenuColors.DarkGrey), Menu.MenuRGB(MenuColors.White), t);
			}
		}

		public override void RemoveSprites()
		{
			if (darkFade != null)
			{
				darkFade.RemoveFromContainer();
			}
			base.RemoveSprites();
		}
	}

	public class InputSelectButton : SimpleButton
	{
		private MenuLabel textLabel;

		public bool gamepad;

		public int index;

		private FSprite circle;

		private float filled;

		private float lastFilled;

		public int blinkCounter;

		public bool recentlyUsedGreyedOut;

		public float recentlyUsedFlash;

		public float lastRecentlyUsedFlash;

		public override Color MyColor(float timeStacker)
		{
			return Color.Lerp(base.MyColor(timeStacker), Menu.MenuRGB(MenuColors.White), (blinkCounter % 4 < 2) ? 0f : Custom.SCurve(Mathf.Lerp(lastRecentlyUsedFlash, recentlyUsedFlash, timeStacker), 0.4f));
		}

		public InputSelectButton(Menu menu, MenuObject owner, string text, Vector2 pos, Options options, bool gamepad, int index)
			: base(menu, owner, text, text, pos, new Vector2(30f, 30f))
		{
			this.gamepad = gamepad;
			this.index = index;
			textLabel = new MenuLabel(menu, this, text, new Vector2(30f, 0f), size, bigText: false);
			textLabel.label.alignment = FLabelAlignment.Left;
			subObjects.Add(textLabel);
			circle = new FSprite("Futile_White");
			circle.shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"];
			Container.AddChild(circle);
			RefreshLabelText();
		}

		public override void Update()
		{
			base.Update();
			blinkCounter++;
			lastFilled = filled;
			if ((menu as InputOptionsMenu).settingInput.HasValue && (menu as InputOptionsMenu).settingInput.Value.x == GAMEPAD_ASSIGNMENT == gamepad && (menu as InputOptionsMenu).settingInput.Value.y == index)
			{
				filled = Custom.LerpAndTick(filled, 1f, 0.05f, 0.05f);
				if (blinkCounter % 30 < 15)
				{
					textLabel.text = "?";
				}
				else
				{
					RefreshLabelText();
				}
			}
			else
			{
				filled = Custom.LerpAndTick(filled, 0f, 0.05f, 0.05f);
			}
			if (recentlyUsedGreyedOut)
			{
				if (Selected)
				{
					buttonBehav.greyedOut = true;
				}
				else if (menu.selectedObject != null)
				{
					recentlyUsedGreyedOut = false;
					buttonBehav.greyedOut = false;
				}
			}
			lastRecentlyUsedFlash = recentlyUsedFlash;
			recentlyUsedFlash = Mathf.Max(0f, recentlyUsedFlash - 0.025f);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			textLabel.label.color = MyColor(timeStacker);
			circle.x = DrawX(timeStacker) + size.x / 2f;
			circle.y = DrawY(timeStacker) + size.y / 2f;
			circle.scale = Mathf.Lerp(0.5f, 0.875f, Mathf.Pow(Custom.SCurve(Mathf.Lerp(lastFilled, filled, timeStacker), 0.3f), 1.4f));
			circle.color = new Color(0f, 1f, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastFilled, filled, timeStacker)), 0.5f));
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
			circle.RemoveFromContainer();
		}

		public void InputAssigned()
		{
			RefreshLabelText();
			recentlyUsedGreyedOut = true;
			recentlyUsedFlash = 1f;
		}

		public void RefreshLabelText()
		{
			textLabel.text = ButtonText(menu, gamepad, (menu as InputOptionsMenu).manager.rainWorld.options.playerToSetInputFor, index, inputTesterDisplay: false);
		}

		public bool InputDeviceAvailable()
		{
			return IsInputDeviceCurrentlyAvailable(menu, gamepad, (menu as InputOptionsMenu).manager.rainWorld.options.playerToSetInputFor);
		}

		public static bool IsInputDeviceCurrentlyAvailable(Menu menu, bool gamePadBool, int player)
		{
			Options.ControlSetup controlSetup = (menu as InputOptionsMenu).manager.rainWorld.options.controls[player];
			if (controlSetup.GetControlPreference() == Options.ControlSetup.ControlToUse.ANY)
			{
				return false;
			}
			if (!gamePadBool && !controlSetup.player.controllers.hasKeyboard)
			{
				return false;
			}
			if (gamePadBool && (controlSetup.GetActiveController() == null || controlSetup.GetActiveController().type != ControllerType.Joystick))
			{
				return false;
			}
			return true;
		}

		public static string ButtonText(Menu menu, bool gamePadBool, int player, int button, bool inputTesterDisplay)
		{
			Options.ControlSetup controlSetup = (menu as InputOptionsMenu).manager.rainWorld.options.controls[player];
			bool flag = controlSetup.GetControlPreference() == Options.ControlSetup.ControlToUse.ANY && controlSetup.GetActiveController() != null && inputTesterDisplay;
			if (!IsInputDeviceCurrentlyAvailable(menu, gamePadBool, player) && !flag)
			{
				return "-";
			}
			string text = ((menu as InputOptionsMenu).inputAxesPositive[button] ? "1" : "0");
			string key = (menu as InputOptionsMenu).inputActions[button][0] + "," + text;
			if (!gamePadBool && controlSetup.mouseButtonMappings.ContainsKey(key) && controlSetup.mouseButtonMappings[key] >= 0 && controlSetup.mouseButtonMappings[key] < ReInput.controllers.Mouse.Buttons.Count)
			{
				int num = controlSetup.mouseButtonMappings[key];
				return num switch
				{
					0 => "Left Click", 
					1 => "Right Click", 
					2 => "Middle Click", 
					_ => "Mouse " + num, 
				};
			}
			ActionElementMap actionElementMap = null;
			for (int i = 0; i < (menu as InputOptionsMenu).inputActions[button].Length; i++)
			{
				actionElementMap = controlSetup.GetActionElement((menu as InputOptionsMenu).inputActions[button][0], (menu as InputOptionsMenu).inputActionCategories[button][0], (menu as InputOptionsMenu).inputAxesPositive[button]);
				if (actionElementMap != null)
				{
					break;
				}
			}
			if (actionElementMap == null)
			{
				return "???";
			}
			return actionElementMap.elementIdentifierName;
		}

		public override void Clicked()
		{
			if (!(menu as InputOptionsMenu).settingInput.HasValue)
			{
				(menu as InputOptionsMenu).mouseModeBeforeAssigningInput = menu.manager.menuesMouseMode;
				(menu as InputOptionsMenu).settingInput = new IntVector2(gamepad ? GAMEPAD_ASSIGNMENT : KEYBOARD_ASSIGNMENT, index);
				for (int i = 0; i < (menu as InputOptionsMenu).inputMappers.Length; i++)
				{
					(menu as InputOptionsMenu).inputMappers[i].Stop();
					(menu as InputOptionsMenu).mappingContexts[i] = null;
				}
				for (int j = 0; j < (menu as InputOptionsMenu).inputActions[index].Length; j++)
				{
					int num = (menu as InputOptionsMenu).inputActions[index][j];
					int num2 = (menu as InputOptionsMenu).inputActionCategories[index][j];
					(menu as InputOptionsMenu).mappingContexts[j] = new InputMapper.Context
					{
						actionId = num,
						actionRange = ((menu as InputOptionsMenu).inputAxesPositive[index] ? AxisRange.Positive : AxisRange.Negative),
						controllerMap = ((num2 == 0) ? (menu as InputOptionsMenu).CurrentControlSetup.gameControlMap : (menu as InputOptionsMenu).CurrentControlSetup.uiControlMap),
						actionElementMapToReplace = (menu as InputOptionsMenu).CurrentControlSetup.GetActionElement(num, num2, (menu as InputOptionsMenu).inputAxesPositive[index])
					};
				}
				(menu as InputOptionsMenu).startListening = true;
				menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			}
		}
	}

	public SimpleButton backButton;

	public SimpleButton testButton;

	public SimpleButton keyboardDefaultsButton;

	public SimpleButton gamepadDefaultsButton;

	public InputSelectButton[] keyBoardKeysButtons;

	public InputSelectButton[] gamePadButtonButtons;

	public MenuLabel[] inputLabels;

	public CheckBox xInvCheck;

	public CheckBox yInvCheck;

	private static readonly int KEYBOARD_ASSIGNMENT = 0;

	private static readonly int GAMEPAD_ASSIGNMENT = 1;

	public IntVector2? settingInput;

	private bool mouseModeBeforeAssigningInput;

	private int forceMouseMode;

	public int freezeMenuFunctionsCounter;

	private FSprite darkSprite;

	private string[] inputLabelTexts;

	private string[] inputDevicedTexts;

	private List<int[]> inputActions;

	private List<int[]> inputActionCategories;

	private bool[] inputAxesPositive;

	public PlayerButton[] playerButtons;

	public DeviceButton[] deviceButtons;

	public List<string> currentGamepads = new List<string>();

	public List<Joystick> currentJoystickObjects = new List<Joystick>();

	public bool[] rememberPlayersSignedIn;

	public InputMapper[] inputMappers;

	private InputMapper.Context[] mappingContexts;

	public bool startListening;

	public bool mappersStarted;

	public InputTesterHolder inputTesterHolder;

	public bool fromJollyMenu;

	public ProcessManager.ProcessID previousMenu;

	public override bool ForceNoMouseMode => false;

	protected override bool FreezeMenuFunctions
	{
		get
		{
			if (!base.FreezeMenuFunctions)
			{
				return freezeMenuFunctionsCounter > 0;
			}
			return true;
		}
	}

	public Options.ControlSetup CurrentControlSetup => manager.rainWorld.options.controls[manager.rainWorld.options.playerToSetInputFor];

	public InputOptionsMenu(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.InputOptions)
	{
		pages.Add(new Page(this, null, "main", 0));
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
		Vector2 vector = new Vector2(0f, -30f);
		inputLabelTexts = new string[9] { "Pause", "Map", "Pick up / Eat", "Jump", "Throw", "Left", "Up", "Right", "Down" };
		inputAxesPositive = new bool[9] { true, true, true, true, true, false, true, true, false };
		inputActions = new List<int[]>();
		inputActionCategories = new List<int[]>();
		inputActions.Add(new int[1] { 5 });
		inputActionCategories.Add(new int[1]);
		inputActions.Add(new int[1] { 11 });
		inputActionCategories.Add(new int[1]);
		inputActions.Add(new int[1] { 3 });
		inputActionCategories.Add(new int[1]);
		inputActions.Add(new int[2] { 0, 8 });
		inputActionCategories.Add(new int[2] { 0, 1 });
		inputActions.Add(new int[2] { 4, 9 });
		inputActionCategories.Add(new int[2] { 0, 1 });
		inputActions.Add(new int[2] { 1, 6 });
		inputActionCategories.Add(new int[2] { 0, 1 });
		inputActions.Add(new int[2] { 2, 7 });
		inputActionCategories.Add(new int[2] { 0, 1 });
		inputActions.Add(new int[2] { 1, 6 });
		inputActionCategories.Add(new int[2] { 0, 1 });
		inputActions.Add(new int[2] { 2, 7 });
		inputActionCategories.Add(new int[2] { 0, 1 });
		inputDevicedTexts = new string[3] { "Takes input from any connected gamepad", "Keyboard", "Gamepad <X> (Not found)" };
		for (int i = 0; i < inputLabelTexts.Length; i++)
		{
			inputLabelTexts[i] = Translate(inputLabelTexts[i]);
		}
		for (int j = 0; j < inputDevicedTexts.Length; j++)
		{
			inputDevicedTexts[j] = Custom.ReplaceLineDelimeters(Translate(inputDevicedTexts[j]));
		}
		deviceButtons = new DeviceButton[6];
		for (int k = 0; k < deviceButtons.Length; k++)
		{
			string text = inputDevicedTexts[Math.Min(k, inputDevicedTexts.Length - 1)];
			if (k > 1)
			{
				text = Regex.Replace(text, "<X>", (k - 1).ToString());
			}
			if (k == 0 && base.CurrLang != InGameTranslator.LanguageID.English)
			{
				text = InGameTranslator.EvenSplit(text, 1);
			}
			deviceButtons[k] = new DeviceButton(this, pages[0], new Vector2(450f, 620f - (float)k * 90f) + vector, text, deviceButtons, k);
			pages[0].subObjects.Add(deviceButtons[k]);
		}
		Vector2 vector2 = new Vector2(839f, 615f) + new Vector2(82f, 0f) + vector;
		pages[0].subObjects.Add(new MenuLabel(this, pages[0], Translate("KEYBOARD"), vector2 + new Vector2(0f, 50f), new Vector2(100f, 30f), bigText: false));
		pages[0].subObjects.Add(new MenuLabel(this, pages[0], Translate("GAMEPAD"), vector2 + new Vector2(150f, 50f), new Vector2(100f, 30f), bigText: false));
		inputLabels = new MenuLabel[9];
		keyBoardKeysButtons = new InputSelectButton[9];
		for (int l = 0; l < 9; l++)
		{
			keyBoardKeysButtons[l] = new InputSelectButton(this, pages[0], "", vector2 + new Vector2(0f, -40f * (float)l), manager.rainWorld.options, gamepad: false, l);
			pages[0].subObjects.Add(keyBoardKeysButtons[l]);
			inputLabels[l] = new MenuLabel(this, pages[0], inputLabelTexts[l], vector2 + new Vector2(-80f, -40f * (float)l), new Vector2(100f, 30f), bigText: false);
			inputLabels[l].label.alignment = FLabelAlignment.Right;
			pages[0].subObjects.Add(inputLabels[l]);
		}
		gamePadButtonButtons = new InputSelectButton[5];
		for (int m = 0; m < 5; m++)
		{
			gamePadButtonButtons[m] = new InputSelectButton(this, pages[0], "", vector2 + new Vector2(150f, -40f * (float)m), manager.rainWorld.options, gamepad: true, m);
			pages[0].subObjects.Add(gamePadButtonButtons[m]);
		}
		xInvCheck = new CheckBox(this, pages[0], this, new Vector2(vector2.x + 150f, vector2.y + -320f + 30f), 65f, Translate("Invert X"), "XINV", textOnRight: true);
		pages[0].subObjects.Add(xInvCheck);
		yInvCheck = new CheckBox(this, pages[0], this, new Vector2(vector2.x + 150f, vector2.y + -320f), 65f, Translate("Invert Y"), "YINV", textOnRight: true);
		pages[0].subObjects.Add(yInvCheck);
		float x = 110f;
		keyboardDefaultsButton = new SimpleButton(this, pages[0], Translate("PRESET"), "KEYBOARD_DEFAULTS", vector2 + new Vector2(0f, -40f * (float)keyBoardKeysButtons.Length), new Vector2(x, 30f));
		pages[0].subObjects.Add(keyboardDefaultsButton);
		gamepadDefaultsButton = new SimpleButton(this, pages[0], Translate("PRESET"), "GAMEPAD_DEFAULTS", vector2 + new Vector2(150f, -40f * (float)gamePadButtonButtons.Length), new Vector2(x, 30f));
		pages[0].subObjects.Add(gamepadDefaultsButton);
		MutualVerticalButtonBind(keyboardDefaultsButton, keyBoardKeysButtons[keyBoardKeysButtons.Length - 1]);
		MutualVerticalButtonBind(gamepadDefaultsButton, gamePadButtonButtons[gamePadButtonButtons.Length - 1]);
		MutualVerticalButtonBind(xInvCheck, gamepadDefaultsButton);
		backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(200f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(backButton);
		backObject = backButton;
		float num = ((base.CurrLang == InGameTranslator.LanguageID.French || base.CurrLang == InGameTranslator.LanguageID.Spanish || base.CurrLang == InGameTranslator.LanguageID.Portuguese) ? 130f : 110f);
		testButton = new SimpleButton(this, pages[0], Translate("TEST INPUT"), "TEST INPUT", new Vector2(1166f - num, 50f), new Vector2(num, 30f));
		pages[0].subObjects.Add(testButton);
		keyboardDefaultsButton.nextSelectable[3] = testButton;
		MenuLabel item = new MenuLabel(this, pages[0], Translate("1 - SELECT PLAYER"), new Vector2(200f + Custom.LerpMap(manager.rainWorld.screenSize.x, 1360f, 1024f, 0f, 25f), 730f) + vector, new Vector2(100f, 20f), base.CurrLang == InGameTranslator.LanguageID.English || base.CurrLang == InGameTranslator.LanguageID.German)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.DarkGrey)
			}
		};
		pages[0].subObjects.Add(item);
		item = new MenuLabel(this, pages[0], Translate("2 - SELECT DEVICE"), new Vector2(deviceButtons[0].pos.x, 730f + vector.y), new Vector2(100f, 20f), base.CurrLang == InGameTranslator.LanguageID.English || base.CurrLang == InGameTranslator.LanguageID.German)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.DarkGrey)
			}
		};
		pages[0].subObjects.Add(item);
		item = new MenuLabel(this, pages[0], Translate("3 - CHOOSE PRESET OR BIND BUTTONS"), new Vector2(1121f, 730f) + vector, new Vector2(100f, 20f), base.CurrLang == InGameTranslator.LanguageID.English || base.CurrLang == InGameTranslator.LanguageID.German)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.DarkGrey),
				alignment = FLabelAlignment.Right,
				anchorX = 1f
			}
		};
		pages[0].subObjects.Add(item);
		item = new MenuLabel(this, pages[0], Translate("4 - TEST"), testButton.pos + new Vector2(-100f, 6f), new Vector2(100f, 20f), base.CurrLang == InGameTranslator.LanguageID.English || base.CurrLang == InGameTranslator.LanguageID.German)
		{
			label = 
			{
				color = Menu.MenuRGB(MenuColors.DarkGrey)
			}
		};
		pages[0].subObjects.Add(item);
		inputTesterHolder = new InputTesterHolder(this, pages[0]);
		pages[0].subObjects.Add(inputTesterHolder);
		playerButtons = new PlayerButton[4];
		rememberPlayersSignedIn = new bool[playerButtons.Length];
		for (int num2 = playerButtons.Length - 1; num2 >= 0; num2--)
		{
			playerButtons[num2] = new PlayerButton(this, pages[0], new Vector2(200f, 600f - (float)num2 * 143.33333f) + vector, playerButtons, num2);
			pages[0].subObjects.Add(playerButtons[num2]);
			rememberPlayersSignedIn[num2] = manager.rainWorld.IsPlayerActive(num2);
		}
		for (int n = 1; n < manager.rainWorld.options.controls.Length; n++)
		{
			manager.rainWorld.ActivatePlayer(n);
		}
		for (int num3 = 0; num3 < playerButtons.Length; num3++)
		{
			playerButtons[num3].pointPos = playerButtons[num3].IdealPointHeight();
			playerButtons[num3].lastPointPos = playerButtons[num3].pointPos;
		}
		int num4 = 0;
		for (int num5 = 0; num5 < inputActions.Count; num5++)
		{
			if (inputActions[num5].Length > num4)
			{
				num4 = inputActions[num5].Length;
			}
		}
		inputMappers = new InputMapper[num4];
		mappingContexts = new InputMapper.Context[num4];
		for (int num6 = 0; num6 < inputMappers.Length; num6++)
		{
			inputMappers[num6] = new InputMapper();
			inputMappers[num6].options.timeout = 0f;
			inputMappers[num6].options.ignoreMouseXAxis = true;
			inputMappers[num6].options.ignoreMouseYAxis = true;
			inputMappers[num6].options.defaultActionWhenConflictFound = InputMapper.ConflictResponse.Add;
			inputMappers[num6].options.checkForConflicts = false;
			inputMappers[num6].options.allowButtons = true;
			inputMappers[num6].options.allowAxes = true;
			inputMappers[num6].options.allowButtonsOnFullAxisAssignment = true;
			inputMappers[num6].options.allowKeyboardKeysWithModifiers = false;
			inputMappers[num6].options.allowKeyboardModifierKeyAsPrimary = true;
		}
		inputTesterHolder.Initiate();
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
		UpdateConnectedControllerLabels();
		RefreshInputGreyOut();
		MutualVerticalButtonBind(playerButtons[0], backButton);
		MutualHorizontalButtonBind(backButton, testButton);
		for (int num7 = 0; num7 < gamePadButtonButtons.Length; num7++)
		{
			gamePadButtonButtons[num7].nextSelectable[2] = playerButtons[(num7 >= gamePadButtonButtons.Length / 2) ? 1 : 0];
		}
	}

	public override string UpdateInfoText()
	{
		if (selectedObject is PlayerButton)
		{
			return Translate("Configure controls for player") + " " + ((selectedObject as PlayerButton).index + 1);
		}
		if (selectedObject is DeviceButton)
		{
			return (selectedObject as DeviceButton).index switch
			{
				1 => Regex.Replace(Translate("Player <X> will use the keyboard"), "<X>", (manager.rainWorld.options.playerToSetInputFor + 1).ToString()), 
				0 => Regex.Replace(Translate("Player <X> will use any connected gamepad"), "<X>", (manager.rainWorld.options.playerToSetInputFor + 1).ToString()), 
				_ => Regex.Replace(Translate("Player <X> will use gamepad"), "<X>", (manager.rainWorld.options.playerToSetInputFor + 1).ToString()) + " " + ((selectedObject as DeviceButton).index - 1), 
			};
		}
		if (selectedObject is InputSelectButton)
		{
			return Regex.Replace(Translate("Bind <X> button"), "<X>", "< " + inputLabelTexts[(selectedObject as InputSelectButton).index] + " >");
		}
		if (selectedObject == keyboardDefaultsButton || selectedObject == gamepadDefaultsButton)
		{
			return Regex.Replace(Translate("Assign player <X> to the default controls"), "<X>", (manager.rainWorld.options.playerToSetInputFor + 1).ToString());
		}
		if (selectedObject == backButton)
		{
			return Translate("Back to options");
		}
		if (selectedObject == testButton)
		{
			return Translate("Test your new controls");
		}
		if (selectedObject == xInvCheck)
		{
			return Translate("invert_x_description");
		}
		if (selectedObject == yInvCheck)
		{
			return Translate("invert_y_description");
		}
		return base.UpdateInfoText();
	}

	public override void Update()
	{
		base.Update();
		if (inputTesterHolder.active)
		{
			freezeMenuFunctionsCounter = Math.Max(freezeMenuFunctionsCounter, 10);
			selectedObject = null;
		}
		if (forceMouseMode > 1)
		{
			manager.menuesMouseMode = true;
			forceMouseMode--;
		}
		bool flag = true;
		for (int i = 0; i < inputMappers.Length; i++)
		{
			if (inputMappers[i].status != 0)
			{
				flag = false;
			}
		}
		if (startListening && flag)
		{
			for (int j = 0; j < inputMappers.Length; j++)
			{
				if (mappingContexts[j] != null)
				{
					inputMappers[j].Start(mappingContexts[j]);
				}
			}
			startListening = false;
			mappersStarted = true;
		}
		else if (mappersStarted && !flag)
		{
			freezeMenuFunctionsCounter = 5;
			selectedObject = ((settingInput.Value.x == KEYBOARD_ASSIGNMENT) ? keyBoardKeysButtons[settingInput.Value.y] : gamePadButtonButtons[settingInput.Value.y]);
			if (settingInput.Value.x == KEYBOARD_ASSIGNMENT && ReInput.controllers.Mouse.GetAnyButton())
			{
				int num = -1;
				for (int k = 0; k < ReInput.controllers.Mouse.Buttons.Count; k++)
				{
					if (ReInput.controllers.Mouse.GetButton(k))
					{
						num = k;
						break;
					}
				}
				if (num >= 0)
				{
					PlaySound(SoundID.MENU_Button_Successfully_Assigned);
					for (int l = 0; l < mappingContexts.Length; l++)
					{
						if (mappingContexts[l] != null)
						{
							Pole axisContribution = ((mappingContexts[l].actionRange != AxisRange.Positive) ? Pole.Negative : Pole.Positive);
							string text = ((mappingContexts[l].actionRange == AxisRange.Positive) ? "1" : "0");
							mappingContexts[l].controllerMap.ReplaceElementMap(mappingContexts[l].actionElementMapToReplace.id, mappingContexts[l].actionId, axisContribution, KeyCode.None, ModifierKeyFlags.None, out var _);
							manager.rainWorld.options.controls[manager.rainWorld.options.playerToSetInputFor].mouseButtonMappings[mappingContexts[l].actionId + "," + text] = num;
						}
					}
					StopInputAssignment();
					Options.ControlSetup.SaveAllControllerUserdata();
					CurrentControlSetup.UpdateControlPreference(CurrentControlSetup.GetControlPreference(), forceUpdate: true);
					RefreshInputGreyOut();
					mappersStarted = false;
				}
			}
		}
		else if (mappersStarted && flag)
		{
			PlaySound(SoundID.MENU_Button_Successfully_Assigned);
			for (int m = 0; m < mappingContexts.Length; m++)
			{
				if (mappingContexts[m] != null)
				{
					string text2 = ((mappingContexts[m].actionRange == AxisRange.Positive) ? "1" : "0");
					manager.rainWorld.options.controls[manager.rainWorld.options.playerToSetInputFor].mouseButtonMappings[mappingContexts[m].actionId + "," + text2] = -1;
				}
			}
			StopInputAssignment();
			Options.ControlSetup.SaveAllControllerUserdata();
			CurrentControlSetup.UpdateControlPreference(CurrentControlSetup.GetControlPreference(), forceUpdate: true);
			RefreshInputGreyOut();
			mappersStarted = false;
		}
		if (!mappersStarted && freezeMenuFunctionsCounter > 0)
		{
			freezeMenuFunctionsCounter--;
		}
	}

	public void UpdateConnectedControllerLabels()
	{
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		currentGamepads = new List<string>();
		currentJoystickObjects = new List<Joystick>();
		for (int i = 0; i < joysticks.Count; i++)
		{
			currentGamepads.Add(Translate("Connected") + "\r\n" + Custom.TruncateString(joysticks[i].name, 30));
			currentJoystickObjects.Add(joysticks[i]);
		}
	}

	public void StopInputAssignment()
	{
		if (settingInput.HasValue)
		{
			settingInput = null;
			for (int i = 0; i < inputMappers.Length; i++)
			{
				inputMappers[i].Stop();
			}
			if (mouseModeBeforeAssigningInput)
			{
				forceMouseMode = 10;
				mouseModeBeforeAssigningInput = false;
			}
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		switch (message)
		{
		case "BACK":
			if (ModManager.CoopAvailable && fromJollyMenu && previousMenu != null)
			{
				manager.RequestMainProcessSwitch(previousMenu);
			}
			else
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
			}
			PlaySound(SoundID.MENU_Switch_Page_Out);
			manager.rainWorld.options.Save();
			break;
		case "TEST INPUT":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			inputTesterHolder.active = true;
			break;
		case "EXIT TEST INPUT":
			PlaySound(SoundID.MENU_Switch_Page_Out);
			inputTesterHolder.active = false;
			break;
		case "KEYBOARD_DEFAULTS":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			CurrentControlSetup.player.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
			manager.rainWorld.options.controls[manager.rainWorld.options.playerToSetInputFor].mouseButtonMappings.Clear();
			Options.ControlSetup.SaveAllControllerUserdata();
			CurrentControlSetup.UpdateControlPreference(CurrentControlSetup.GetControlPreference(), forceUpdate: true);
			RefreshInputGreyOut();
			break;
		case "GAMEPAD_DEFAULTS":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			CurrentControlSetup.player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
			Options.ControlSetup.SaveAllControllerUserdata();
			CurrentControlSetup.UpdateControlPreference(CurrentControlSetup.GetControlPreference(), forceUpdate: true);
			RefreshInputGreyOut();
			break;
		}
	}

	public void RefreshInputGreyOut()
	{
		bool gamePad = CurrentControlSetup.gamePad;
		bool greyedOut = false;
		for (int i = 0; i < keyBoardKeysButtons.Length; i++)
		{
			keyBoardKeysButtons[i].RefreshLabelText();
			keyBoardKeysButtons[i].buttonBehav.greyedOut = gamePad || !keyBoardKeysButtons[i].InputDeviceAvailable();
			greyedOut = keyBoardKeysButtons[i].buttonBehav.greyedOut;
		}
		keyboardDefaultsButton.buttonBehav.greyedOut = greyedOut;
		bool greyedOut2 = false;
		for (int j = 0; j < gamePadButtonButtons.Length; j++)
		{
			gamePadButtonButtons[j].RefreshLabelText();
			gamePadButtonButtons[j].buttonBehav.greyedOut = !gamePad || !gamePadButtonButtons[j].InputDeviceAvailable();
			greyedOut2 = gamePadButtonButtons[j].buttonBehav.greyedOut;
		}
		gamepadDefaultsButton.buttonBehav.greyedOut = greyedOut2;
		xInvCheck.buttonBehav.greyedOut = greyedOut2;
		yInvCheck.buttonBehav.greyedOut = greyedOut2;
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		for (int i = 0; i < rememberPlayersSignedIn.Length; i++)
		{
			if (manager.rainWorld.IsPlayerActive(i) && !rememberPlayersSignedIn[i])
			{
				manager.rainWorld.DeactivatePlayer(i);
			}
		}
		darkSprite.RemoveFromContainer();
		Options.ControlSetup.SaveAllControllerUserdata();
		for (int j = 0; j < inputMappers.Length; j++)
		{
			inputMappers[j].RemoveAllEventListeners();
		}
	}

	public int GetCurrentlySelectedOfSeries(string series)
	{
		switch (series)
		{
		case "PlayerButtons":
			return manager.rainWorld.options.playerToSetInputFor;
		case "DeviceButtons":
			if (CurrentControlSetup.GetControlPreference() == Options.ControlSetup.ControlToUse.KEYBOARD)
			{
				return 1;
			}
			if (CurrentControlSetup.GetControlPreference() == Options.ControlSetup.ControlToUse.ANY)
			{
				return 0;
			}
			return CurrentControlSetup.usingGamePadNumber + 2;
		default:
			return -1;
		}
	}

	public void SetCurrentlySelectedOfSeries(string series, int to)
	{
		switch (series)
		{
		case "PlayerButtons":
		{
			manager.rainWorld.options.playerToSetInputFor = to;
			RefreshInputGreyOut();
			for (int i = 0; i < keyBoardKeysButtons.Length; i++)
			{
				keyBoardKeysButtons[i].RefreshLabelText();
			}
			for (int j = 0; j < gamePadButtonButtons.Length; j++)
			{
				gamePadButtonButtons[j].RefreshLabelText();
			}
			break;
		}
		case "DeviceButtons":
			switch (to)
			{
			case 1:
				CurrentControlSetup.UpdateControlPreference(Options.ControlSetup.ControlToUse.KEYBOARD);
				break;
			case 0:
				CurrentControlSetup.gamePadNumber = 0;
				CurrentControlSetup.UpdateControlPreference(Options.ControlSetup.ControlToUse.ANY);
				break;
			default:
				if (manager.rainWorld.options.playerToSetInputFor > 0 || ReInput.controllers.Joysticks.Count > to - 2)
				{
					CurrentControlSetup.gamePadGuid = null;
					CurrentControlSetup.gamePadNumber = to - 2;
					CurrentControlSetup.UpdateControlPreference(Options.ControlSetup.ControlToUse.SPECIFIC_GAMEPAD);
				}
				break;
			}
			RefreshInputGreyOut();
			break;
		}
	}

	public bool GetChecked(CheckBox box)
	{
		return box.IDString switch
		{
			"XINV" => CurrentControlSetup.xInvert, 
			"YINV" => CurrentControlSetup.yInvert, 
			_ => false, 
		};
	}

	public void SetChecked(CheckBox box, bool c)
	{
		switch (box.IDString)
		{
		case "XINV":
			CurrentControlSetup.xInvert = !CurrentControlSetup.xInvert;
			break;
		case "YINV":
			CurrentControlSetup.yInvert = !CurrentControlSetup.yInvert;
			break;
		}
	}
}
