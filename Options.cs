using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JollyCoop.JollyMenu;
using Kittehface.Framework20;
using Menu;
using Rewired;
using RWCustom;
using Steamworks;
using UnityEngine;

public class Options
{
	public class JollyDifficulty : ExtEnum<JollyDifficulty>
	{
		public static readonly JollyDifficulty EASY = new JollyDifficulty("EASY", register: true);

		public static readonly JollyDifficulty NORMAL = new JollyDifficulty("NORMAL", register: true);

		public static readonly JollyDifficulty HARD = new JollyDifficulty("HARD", register: true);

		public JollyDifficulty(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class JollyCameraInputSpeed : ExtEnum<JollyCameraInputSpeed>
	{
		public static readonly JollyCameraInputSpeed FAST = new JollyCameraInputSpeed("FAST", register: true);

		public static readonly JollyCameraInputSpeed NORMAL = new JollyCameraInputSpeed("NORMAL", register: true);

		public static readonly JollyCameraInputSpeed SLOW = new JollyCameraInputSpeed("SLOW", register: true);

		public JollyCameraInputSpeed(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class JollyColorMode : ExtEnum<JollyColorMode>
	{
		public static readonly JollyColorMode DEFAULT = new JollyColorMode("DEFAULT", register: true);

		public static readonly JollyColorMode AUTO = new JollyColorMode("AUTO", register: true);

		public static readonly JollyColorMode CUSTOM = new JollyColorMode("CUSTOM", register: true);

		public JollyColorMode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Quality : ExtEnum<Quality>
	{
		public static readonly Quality LOW = new Quality("LOW", register: true);

		public static readonly Quality MEDIUM = new Quality("MEDIUM", register: true);

		public static readonly Quality HIGH = new Quality("HIGH", register: true);

		public Quality(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class ControlSetup
	{
		public enum ControlToUse
		{
			UNDEFINED,
			KEYBOARD,
			ANY,
			SPECIFIC_GAMEPAD
		}

		public class Preset : ExtEnum<Preset>
		{
			public static readonly Preset None = new Preset("None", register: true);

			public static readonly Preset KeyboardSinglePlayer = new Preset("KeyboardSinglePlayer", register: true);

			public static readonly Preset PS4DualShock = new Preset("PS4DualShock", register: true);

			public static readonly Preset PS5DualSense = new Preset("PS5DualSense", register: true);

			public static readonly Preset XBox = new Preset("XBox", register: true);

			public static readonly Preset SwitchHandheld = new Preset("SwitchHandheld", register: true);

			public static readonly Preset SwitchDualJoycon = new Preset("SwitchDualJoycon", register: true);

			public static readonly Preset SwitchSingleJoyconL = new Preset("SwitchSingleJoyconL", register: true);

			public static readonly Preset SwitchSingleJoyconR = new Preset("SwitchSingleJoyconR", register: true);

			public static readonly Preset SwitchProController = new Preset("SwitchProController", register: true);

			public Preset(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Rewired.Player player;

		public PlayerHandler handler;

		public int index;

		private bool active;

		private ControlToUse controlPreference;

		public bool xInvert;

		public bool yInvert;

		public bool controlSetupInitialized;

		public Dictionary<string, int> mouseButtonMappings = new Dictionary<string, int>();

		public ControllerMap uiControlMap;

		public ControllerMap gameControlMap;

		private Controller recentController;

		private Preset? recentPreset;

		public int gamePadNumber;

		public string gamePadGuid;

		public int usingGamePadNumber;

		public string[] unrecognizedControlAttrs;

		public bool gamePad
		{
			get
			{
				if (recentController != null)
				{
					return recentController.type == ControllerType.Joystick;
				}
				return false;
			}
		}

		public KeyCode KeyboardPause => KeyCodeFromAction(5, 0);

		public KeyCode KeyboardMap => KeyCodeFromAction(11, 0);

		public KeyCode KeyboardPickUp => KeyCodeFromAction(3, 0);

		public KeyCode KeyboardJump => KeyCodeFromAction(0, 0);

		public KeyCode KeyboardThrow => KeyCodeFromAction(4, 0);

		public KeyCode KeyboardLeft => KeyCodeFromAction(1, 0, axisPositive: false);

		public KeyCode KeyboardUp => KeyCodeFromAction(2, 0);

		public KeyCode KeyboardRight => KeyCodeFromAction(1, 0);

		public KeyCode KeyboardDown => KeyCodeFromAction(2, 0, axisPositive: false);

		public ControlSetup(int index, bool active)
		{
			this.index = index;
			if (index == 0)
			{
				this.active = true;
			}
			else
			{
				this.active = active;
			}
			gamePadNumber = Math.Max(index - 1, 0);
			usingGamePadNumber = gamePadNumber;
			InitiateControlSetup();
		}

		public void InitiateControlSetup()
		{
			if (!controlSetupInitialized)
			{
				handler = Custom.rainWorld.InstantiatePlayerHandler(index);
				InitRewiredObjects();
				UpdateControlPreference((index == 0) ? ControlToUse.ANY : ControlToUse.SPECIFIC_GAMEPAD);
				controlSetupInitialized = true;
			}
		}

		public void InitRewiredObjects()
		{
			foreach (Rewired.Player player in ReInput.players.GetPlayers())
			{
				if (player.id == index)
				{
					this.player = player;
					break;
				}
			}
		}

		public void UpdateControlPreference(ControlToUse preference, bool forceUpdate = false)
		{
			if (player == null)
			{
				return;
			}
			UserInput.RefreshConsoleControllerMap();
			if (preference == controlPreference && preference != ControlToUse.SPECIFIC_GAMEPAD && !forceUpdate)
			{
				return;
			}
			controlPreference = preference;
			player.controllers.ClearAllControllers();
			if (templatePlayer != null)
			{
				templatePlayer.controllers.ClearAllControllers();
			}
			recentController = null;
			recentPreset = Preset.None;
			if (index == 0)
			{
				player.controllers.AddController(player.controllers.Mouse, removeFromOtherPlayers: false);
			}
			if (controlPreference == ControlToUse.KEYBOARD)
			{
				player.controllers.AddController(player.controllers.Keyboard, removeFromOtherPlayers: false);
				if (templatePlayer != null)
				{
					templatePlayer.controllers.AddController(player.controllers.Keyboard, removeFromOtherPlayers: false);
				}
				UpdateActiveController(player.controllers.Keyboard, 0, forceUpdate);
			}
			else if (controlPreference == ControlToUse.SPECIFIC_GAMEPAD)
			{
				bool flag = false;
				if (gamePadGuid != null)
				{
					int num = 0;
					foreach (Joystick joystick in ReInput.controllers.Joysticks)
					{
						if (joystick.deviceInstanceGuid.ToString() == gamePadGuid)
						{
							player.controllers.AddController(joystick, removeFromOtherPlayers: false);
							if (templatePlayer != null)
							{
								templatePlayer.controllers.AddController(joystick, removeFromOtherPlayers: false);
							}
							usingGamePadNumber = num;
							flag = true;
							UpdateActiveController(joystick, num, forceUpdate);
							break;
						}
						num++;
					}
				}
				if (flag)
				{
					return;
				}
				if (UserInput.consoleControllerMap.Length != 0)
				{
					Controller controller = null;
					if (controller == null)
					{
						if (UserInput.consoleControllerMap[gamePadNumber] != null)
						{
							player.controllers.AddController(UserInput.consoleControllerMap[gamePadNumber], removeFromOtherPlayers: false);
							UpdateActiveController(UserInput.consoleControllerMap[gamePadNumber], gamePadNumber, forceUpdate);
						}
					}
					else
					{
						player.controllers.AddController(controller, removeFromOtherPlayers: true);
						UpdateActiveController(controller, gamePadNumber, forceUpdate);
					}
				}
				else
				{
					int num2 = 0;
					foreach (Joystick joystick2 in ReInput.controllers.Joysticks)
					{
						if (num2 == gamePadNumber)
						{
							player.controllers.AddController(joystick2, removeFromOtherPlayers: false);
							if (templatePlayer != null)
							{
								templatePlayer.controllers.AddController(joystick2, removeFromOtherPlayers: false);
							}
							UpdateActiveController(joystick2, num2, forceUpdate);
							break;
						}
						num2++;
					}
				}
				usingGamePadNumber = gamePadNumber;
			}
			else
			{
				if (controlPreference != ControlToUse.ANY)
				{
					return;
				}
				player.controllers.AddController(player.controllers.Keyboard, removeFromOtherPlayers: false);
				if (templatePlayer != null)
				{
					templatePlayer.controllers.AddController(player.controllers.Keyboard, removeFromOtherPlayers: false);
				}
				foreach (Joystick joystick3 in ReInput.controllers.Joysticks)
				{
					player.controllers.AddController(joystick3, removeFromOtherPlayers: false);
					if (templatePlayer != null)
					{
						templatePlayer.controllers.AddController(joystick3, removeFromOtherPlayers: false);
					}
				}
				Controller controller2 = RWInput.PlayerRecentController(index);
				bool flag2 = false;
				if (controller2 != null && controller2.type != 0)
				{
					int num3 = 0;
					foreach (Joystick joystick4 in ReInput.controllers.Joysticks)
					{
						if (joystick4.name == controller2.name && joystick4.deviceInstanceGuid == controller2.deviceInstanceGuid)
						{
							UpdateActiveController(joystick4, num3, forceUpdate);
							flag2 = true;
							break;
						}
						num3++;
					}
				}
				if (!flag2)
				{
					UpdateActiveController(player.controllers.Keyboard, 0, forceUpdate);
				}
			}
		}

		public ControlToUse GetControlPreference()
		{
			return controlPreference;
		}

		public void SetActive(bool activeState)
		{
			if (index != 0)
			{
				active = activeState;
			}
		}

		public bool GetActive()
		{
			return active;
		}

		public void UpdateActiveController(Controller newController, bool forceUpdate = false)
		{
			if (newController == null)
			{
				return;
			}
			if (newController.type == ControllerType.Keyboard)
			{
				UpdateActiveController(player.controllers.Keyboard, 0, forceUpdate);
				return;
			}
			int num = 0;
			foreach (Joystick joystick in ReInput.controllers.Joysticks)
			{
				if (joystick.name == newController.name && joystick.deviceInstanceGuid == newController.deviceInstanceGuid)
				{
					UpdateActiveController(joystick, num, forceUpdate);
					break;
				}
				num++;
			}
		}

		public void UpdateActiveController(Controller newController, int controllerIndex, bool forceUpdate = false)
		{
			if (newController != null && (forceUpdate || recentController == null || !(newController.name == recentController.name) || !(newController.deviceInstanceGuid == recentController.deviceInstanceGuid)))
			{
				recentController = newController;
				if (newController.type == ControllerType.Keyboard || newController.type == ControllerType.Mouse)
				{
					recentPreset = Preset.KeyboardSinglePlayer;
				}
				else if (RWInput.IsXboxControllerType(newController.name, newController.hardwareIdentifier))
				{
					recentPreset = Preset.XBox;
				}
				else if (RWInput.IsPlaystationControllerType(newController.name, newController.hardwareIdentifier))
				{
					recentPreset = Preset.PS4DualShock;
				}
				else if (RWInput.IsSwitchProControllerType(newController.name, newController.hardwareIdentifier))
				{
					recentPreset = Preset.SwitchProController;
				}
				else
				{
					recentPreset = Preset.XBox;
				}
				if (newController.type == ControllerType.Joystick && gamePadGuid == null)
				{
					gamePadGuid = newController.deviceInstanceGuid.ToString();
				}
				if (player != null)
				{
					uiControlMap = GetFirstMapInCategory(player.controllers.maps, newController.type, controllerIndex, 1);
					gameControlMap = GetFirstMapInCategory(player.controllers.maps, newController.type, controllerIndex, 0);
				}
			}
		}

		public static ControllerMap GetFirstMapInCategory(Rewired.Player.ControllerHelper.MapHelper maps, ControllerType ctype, int controllerId, int categoryId)
		{
			foreach (ControllerMap allMap in maps.GetAllMaps())
			{
				if (allMap.controllerType == ctype && allMap.categoryId == categoryId && allMap.controllerId == controllerId)
				{
					return allMap;
				}
			}
			foreach (ControllerMap allMap2 in maps.GetAllMaps())
			{
				if (allMap2.controllerType == ctype && allMap2.categoryId == categoryId)
				{
					return allMap2;
				}
			}
			return null;
		}

		public static void SaveAllControllerUserdata()
		{
			foreach (Rewired.Player allPlayer in ReInput.players.AllPlayers)
			{
				if (allPlayer.id == 4 || allPlayer.id == 9999999)
				{
					continue;
				}
				foreach (Controller controller in allPlayer.controllers.Controllers)
				{
					ReInput.userDataStore.SaveControllerData(allPlayer.id, controller.type, controller.id);
				}
			}
		}

		public Controller GetActiveController()
		{
			return recentController;
		}

		public Preset GetActivePreset()
		{
			return recentPreset;
		}

		public ActionElementMap GetActionElement(int actionID, int categoryID, bool axisPositive = true)
		{
			if (categoryID == 0)
			{
				return GetActionElement(gameControlMap, actionID, axisPositive);
			}
			return GetActionElement(uiControlMap, actionID, axisPositive);
		}

		public static ActionElementMap GetActionElement(ControllerMap cmap, int actionID, bool axisPositive = true)
		{
			if (cmap == null)
			{
				return null;
			}
			IEnumerable<ActionElementMap> enumerable = cmap.ElementMapsWithAction(actionID);
			ActionElementMap actionElementMap = null;
			foreach (ActionElementMap item in enumerable)
			{
				if (item.axisContribution == Pole.Positive && axisPositive)
				{
					actionElementMap = item;
				}
				else if (item.axisContribution == Pole.Negative && !axisPositive)
				{
					actionElementMap = item;
				}
			}
			if (actionElementMap != null)
			{
				return actionElementMap;
			}
			foreach (ActionElementMap item2 in enumerable)
			{
				if (item2.axisType == AxisType.None)
				{
					return item2;
				}
			}
			return null;
		}

		public KeyCode KeyCodeFromAction(int actionID, int categoryID, bool axisPositive = true)
		{
			if (recentController == null || recentController.type != 0)
			{
				return KeyCode.None;
			}
			return GetActionElement(actionID, categoryID, axisPositive)?.keyCode ?? KeyCode.None;
		}

		public bool GetButton(int actionID)
		{
			if (!active)
			{
				return false;
			}
			string key = actionID + ",1";
			if (mouseButtonMappings.ContainsKey(key) && ReInput.controllers.Mouse.GetButton(mouseButtonMappings[key]))
			{
				return true;
			}
			return player.GetButton(actionID);
		}

		public bool GetButtonDown(int actionID)
		{
			if (!active)
			{
				return false;
			}
			string key = actionID + ",1";
			if (mouseButtonMappings.ContainsKey(key) && ReInput.controllers.Mouse.GetButtonDown(mouseButtonMappings[key]))
			{
				return true;
			}
			return player.GetButtonDown(actionID);
		}

		public float GetAxis(int actionID)
		{
			if (!active)
			{
				return 0f;
			}
			string key = actionID + ",0";
			if (mouseButtonMappings.ContainsKey(key) && ReInput.controllers.Mouse.GetButton(mouseButtonMappings[key]))
			{
				return -1f;
			}
			key = actionID + ",1";
			if (mouseButtonMappings.ContainsKey(key) && ReInput.controllers.Mouse.GetButton(mouseButtonMappings[key]))
			{
				return 1f;
			}
			return player.GetAxisRaw(actionID);
		}

		public bool GetAnyButton()
		{
			if (!active)
			{
				return false;
			}
			if (player.GetAnyButton())
			{
				return true;
			}
			if (ReInput.controllers.Mouse.GetAnyButton())
			{
				return true;
			}
			if (handler.profile != null && UserInput.GetRewiredPlayer(handler.profile, index).GetAnyButton())
			{
				return true;
			}
			return false;
		}

		public bool GameActionMatchesTemplate(int actionID, bool axisPositive = true)
		{
			return ActionMatchesTemplate(gameControlMap, actionID, axisPositive);
		}

		public bool UIActionMatchesTemplate(int actionID, bool axisPositive = true)
		{
			return ActionMatchesTemplate(uiControlMap, actionID, axisPositive);
		}

		public void UpdateTemplateToMatchControllers()
		{
			if (templatePlayer == null)
			{
				return;
			}
			bool flag = false;
			foreach (Controller controller in templatePlayer.controllers.Controllers)
			{
				if (recentController.type == ControllerType.Keyboard && controller.type == ControllerType.Keyboard)
				{
					flag = true;
					break;
				}
				if (controller.name == recentController.name && controller.deviceInstanceGuid == recentController.deviceInstanceGuid)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				templatePlayer.controllers.ClearAllControllers();
				templatePlayer.controllers.AddController(recentController, removeFromOtherPlayers: false);
			}
		}

		private bool ActionMatchesTemplate(ControllerMap compareMap, int actionID, bool axisPositive = true)
		{
			if (recentController == null)
			{
				return false;
			}
			if (templatePlayer == null)
			{
				return true;
			}
			UpdateTemplateToMatchControllers();
			ControllerMap firstMapInCategory = GetFirstMapInCategory(templatePlayer.controllers.maps, recentController.type, (recentController.type == ControllerType.Joystick) ? usingGamePadNumber : 0, 0);
			if (firstMapInCategory == null)
			{
				return false;
			}
			ActionElementMap actionElement = GetActionElement(firstMapInCategory, actionID, axisPositive);
			ActionElementMap actionElement2 = GetActionElement(compareMap, actionID, axisPositive);
			if (actionElement == null || actionElement2 == null)
			{
				return false;
			}
			if (recentController.type == ControllerType.Keyboard)
			{
				return actionElement.keyCode == actionElement2.keyCode;
			}
			return actionElement.elementIndex == actionElement2.elementIndex;
		}

		public bool IsDefaultControlMapping(ControllerMap compareMap)
		{
			if (compareMap == null)
			{
				return false;
			}
			InputMapCategory mapCategory = ReInput.mapping.GetMapCategory(compareMap.categoryId);
			if (mapCategory == null)
			{
				return false;
			}
			InputCategory actionCategory = ReInput.mapping.GetActionCategory(mapCategory.name);
			if (actionCategory == null)
			{
				return false;
			}
			foreach (InputAction item in ReInput.mapping.ActionsInCategory(actionCategory.id))
			{
				if (!ActionMatchesTemplate(compareMap, item.id))
				{
					return false;
				}
			}
			return true;
		}

		public override string ToString()
		{
			string text = "";
			text = text + controlPreference.ToString() + "<ctrlA>";
			text += string.Format(CultureInfo.InvariantCulture, "{0}<ctrlA>", gamePadNumber);
			text += ((gamePadGuid == null) ? "" : gamePadGuid);
			text += "<ctrlA>";
			text = text + (xInvert ? "1" : "0") + "<ctrlA>";
			text = text + (yInvert ? "1" : "0") + "<ctrlA>";
			foreach (KeyValuePair<string, int> mouseButtonMapping in mouseButtonMappings)
			{
				text = text + mouseButtonMapping.Key + ":" + mouseButtonMapping.Value + "<ctrlB>";
			}
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "<ctrlA>", unrecognizedControlAttrs);
		}

		public void FromString(string s)
		{
			Custom.Log("Options: ControlSetup from string " + s);
			if (!s.Contains("<crlA>"))
			{
				string[] array = Regex.Split(s, "<ctrlA>");
				if (array.Length > 4)
				{
					gamePadNumber = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					gamePadGuid = array[2];
					xInvert = array[3] == "1";
					yInvert = array[4] == "1";
					UpdateControlPreference((ControlToUse)Enum.Parse(typeof(ControlToUse), array[0]), forceUpdate: true);
				}
				if (array.Length > 5)
				{
					mouseButtonMappings.Clear();
					string[] array2 = Regex.Split(array[5], "<ctrlB>");
					for (int i = 0; i < array2.Length; i++)
					{
						if (array2[i].Contains(":"))
						{
							string[] array3 = Regex.Split(array2[i], ":");
							if (array3.Length >= 2 && int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
							{
								mouseButtonMappings[array3[0]] = result;
							}
						}
					}
				}
				unrecognizedControlAttrs = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
			}
			else
			{
				string[] array4 = Regex.Split(s, "<crlA>");
				gamePadNumber = int.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				if (array4.Length > 5)
				{
					gamePadGuid = array4[5];
				}
				if (array4.Length > 6)
				{
					xInvert = array4[6] == "1";
				}
				if (array4.Length > 7)
				{
					yInvert = array4[7] == "1";
				}
				UpdateControlPreference((!(array4[0] == "1")) ? ControlToUse.KEYBOARD : ControlToUse.SPECIFIC_GAMEPAD, forceUpdate: true);
				unrecognizedControlAttrs = SaveUtils.PopulateUnrecognizedStringAttrs(array4, 8);
			}
		}
	}

	public const int PLAYER_COUNT = 4;

	public const string SWITCH_MOUNT_NAME = "rainworld";

	private static readonly Guid guidSwitchJoyConL = new Guid("3eb01142-da0e-4a86-8ae8-a15c2b1f2a04");

	private static readonly Guid guidSwitchJoyConR = new Guid("605dc720-1b38-473d-a459-67d5857aa6ea");

	private static readonly Guid guidSwitchDual = new Guid("521b808c-0248-4526-bc10-f1d16ee76bf1");

	private static readonly Guid guidSwitchHandheld = new Guid("1fbdd13b-0795-4173-8a95-a2a75de9d204");

	private static readonly Guid guidSwitchPro = new Guid("7bf3154b-9db8-4d52-950f-cd0eed8a5819");

	private static readonly UserData.FileDefinition OPTIONS_FILE_DEFINITION = new UserData.FileDefinition("options", useRawData: false, cloudEnabled: false, useEncryption: false, prettyPrint: false, useBinarySerialization: false, null, ps4Definition: new UserData.FileDefinition.PS4Definition(null, null, null, null, null, null, "Media/StreamingAssets/SaveIcon.png", 2097152L), switchDefinition: new UserData.FileDefinition.SwitchDefinition("rainworld", 2097152L));

	private const string OPTIONS_KEY = "options";

	private static readonly UserData.FileDefinition OLD_PS4_SAVE_FILE_DEFINITION = new UserData.FileDefinition("unitySaveData", useRawData: true, cloudEnabled: false, useEncryption: false, prettyPrint: false, useBinarySerialization: false, null, null, new UserData.FileDefinition.PS4Definition(null, "autoSaveData", null, null, null, null, "Media/StreamingAssets/SaveIcon.png", 1048576L));

	private const string OLD_PS4_OPTIONS_KEY = "rw_options";

	private const string OLD_PS4_SAVE_KEY = "rw_savedata";

	public ControlSetup[] controls;

	public int resolution;

	public bool windowed;

	public static Vector2[] screenResolutions = new Vector2[5]
	{
		new Vector2(1024f, 768f),
		new Vector2(1366f, 768f),
		new Vector2(1360f, 768f),
		new Vector2(1280f, 768f),
		new Vector2(1229f, 768f)
	};

	public static string[] aspectRatioStrings = new string[5] { "4:3", "16:9", "16:9", "5:3", "16:10" };

	public static Rewired.Player templatePlayer;

	public readonly Vector2 SafeScreenOffset = Vector2.zero;

	public float soundEffectsVolume;

	public float musicVolume;

	public float arenaMusicVolume;

	public InGameTranslator.LanguageID language = InGameTranslator.LanguageID.English;

	public int saveSlot;

	public bool vibration = true;

	public bool createdNewThisLaunch;

	public bool foundOldOptionsFile;

	public static bool isOnSteamDeck;

	public int playerToSetInputFor;

	public bool allGamePads;

	public int timeSinceLastSaveCopy;

	private RainWorldPlugin.EnterButton _enterButton = RainWorldPlugin.EnterButton.Cross;

	private bool _enterButtonSet;

	private RainWorld rainWorld;

	internal UserData.File optionsFile;

	public bool optionsFileCanSave = true;

	private string arenaSittingLocalData;

	private Dictionary<string, string> sandboxRoomLocalData;

	private UserData.File oldPS4SaveFile;

	private bool oldPS4FileLoaded;

	private Dictionary<string, string> oldPS4FileSettings = new Dictionary<string, string>();

	private Dictionary<string, string> oldPS4FileData = new Dictionary<string, string>();

	public List<string> enabledMods = new List<string>();

	public Dictionary<string, int> modLoadOrder = new Dictionary<string, int>();

	public Dictionary<string, string> modChecksums = new Dictionary<string, string>();

	private List<string> unrecognizedSaveStrings = new List<string>();

	private List<string> unrecognizedNonSyncedSaveStrings = new List<string>();

	public bool validation;

	public bool remixTutorialShown;

	public bool dlcTutorialShown;

	public string lastGameVersion;

	public bool commentary;

	public JollyPlayerOptions[] jollyPlayerOptionsArray;

	public JollyCameraInputSpeed jollyCameraInputSpeed;

	public JollyColorMode jollyColorMode;

	public JollyDifficulty jollyDifficulty;

	public bool friendlyFire;

	public bool friendlySteal;

	public bool jollyHud;

	public bool cameraCycling;

	public bool smartShortcuts;

	public bool friendlyLizards;

	public int playersBeforeEnterJollyMenu;

	public bool jollyControllersNeedUpdating = true;

	private int pendingMultiplayerProfileRequest;

	private DialogNotify profileRequestDialog;

	public int fpsCap;

	public bool vsync;

	public Quality quality;

	public float analogSensitivity;

	public MenuScene.SceneID titleBackground;

	public MenuScene.SceneID subBackground;

	public bool fullScreen => !windowed;

	public Vector2 ScreenSize => screenResolutions[resolution];

	public RainWorldPlugin.EnterButton enterButton
	{
		get
		{
			if (!_enterButtonSet)
			{
				_enterButtonSet = true;
				_enterButton = RainWorldPlugin.GetEnterButton();
			}
			return _enterButton;
		}
	}

	public bool optionsLoaded { get; private set; }

	public int JollyPlayerCount
	{
		get
		{
			if (!ModManager.JollyCoop)
			{
				return 1;
			}
			return jollyPlayerOptionsArray.Count((JollyPlayerOptions x) => x.joined);
		}
	}

	public MenuScene.SceneID TitleBackground
	{
		get
		{
			if (titleBackground.Index != -1)
			{
				return titleBackground;
			}
			if (rainWorld.dlcVersion <= 0)
			{
				return MenuScene.SceneID.MainMenu;
			}
			return MenuScene.SceneID.MainMenu_Downpour;
		}
	}

	public MenuScene.SceneID SubBackground
	{
		get
		{
			if (subBackground.Index != -1)
			{
				return subBackground;
			}
			return MenuScene.SceneID.Landscape_SU;
		}
	}

	public event Action<bool> onOldPS4SaveLoaded;

	public string GetSaveFileName_SavOrExp()
	{
		if (ModManager.Expedition && saveSlot < 0)
		{
			return "exp" + Math.Abs(saveSlot);
		}
		if (saveSlot != 0)
		{
			return "sav" + (saveSlot + 1);
		}
		return "sav";
	}

	public void ResetJollyProfileRequest()
	{
		pendingMultiplayerProfileRequest = 0;
	}

	public bool IsJollyProfileRequesting()
	{
		return pendingMultiplayerProfileRequest > 0;
	}

	private void OnProfileRequestDialogClosed()
	{
		int num = pendingMultiplayerProfileRequest;
		rainWorld.RequestPlayerSignIn(pendingMultiplayerProfileRequest, UserInput.consoleControllerMap[num] as Joystick);
	}

	public bool CheckJollyProfileRequestNeeded()
	{
		if (ModManager.JollyCoop && rainWorld.options.JollyPlayerCount > 1)
		{
			pendingMultiplayerProfileRequest = 0;
			for (int i = 1; i < rainWorld.options.JollyPlayerCount; i++)
			{
				pendingMultiplayerProfileRequest = i;
				if (rainWorld.options.controls[i].handler != null && rainWorld.options.controls[i].handler.SigningIn)
				{
					return true;
				}
				if (rainWorld.options.controls[i].handler == null || rainWorld.options.controls[i].handler.profile == null)
				{
					UserInput.RefreshConsoleControllerMap();
					OnProfileRequestDialogClosed();
					return true;
				}
			}
		}
		return false;
	}

	public Options(RainWorld rainWorld)
	{
		optionsLoaded = false;
		this.rainWorld = rainWorld;
		foreach (Rewired.Player player in ReInput.players.GetPlayers())
		{
			if (player.id == 4)
			{
				templatePlayer = player;
				break;
			}
		}
		isOnSteamDeck = false;
		if (SteamManager.Initialized && SteamUtils.IsSteamRunningOnSteamDeck())
		{
			resolution = 4;
			isOnSteamDeck = true;
		}
		else
		{
			resolution = 1;
		}
		timeSinceLastSaveCopy = 0;
		soundEffectsVolume = 0.8f;
		musicVolume = 0.8f;
		arenaMusicVolume = 0.625f;
		validation = false;
		controls = new ControlSetup[4];
		for (int i = 0; i < controls.Length; i++)
		{
			controls[i] = new ControlSetup(i, i == 0);
		}
		commentary = false;
		quality = Quality.HIGH;
		fpsCap = 60;
		vsync = false;
		analogSensitivity = 1f;
		titleBackground = MenuScene.SceneID.MainMenu_Downpour;
		subBackground = MenuScene.SceneID.Landscape_SU;
		jollyDifficulty = JollyDifficulty.NORMAL;
		friendlyFire = false;
		friendlySteal = true;
		jollyHud = true;
		smartShortcuts = true;
		cameraCycling = false;
		friendlyLizards = true;
		jollyPlayerOptionsArray = new JollyPlayerOptions[4];
		jollyCameraInputSpeed = JollyCameraInputSpeed.NORMAL;
		jollyColorMode = JollyColorMode.AUTO;
		for (int j = 0; j < jollyPlayerOptionsArray.Length; j++)
		{
			jollyPlayerOptionsArray[j] = new JollyPlayerOptions(j);
			if (j != 0)
			{
				jollyPlayerOptionsArray[j].joined = false;
			}
		}
		LoadControllerOptions();
		if (Platform.initialized)
		{
			LoadOptions();
		}
		else
		{
			Platform.OnRequestUserDataRead += Platform_OnRequestUserDataRead;
		}
	}

	public void Load()
	{
		if (optionsLoaded && optionsFile != null && optionsFile.Contains("options"))
		{
			FromString(optionsFile.Get("options", ""));
			string path = Application.persistentDataPath + Path.DirectorySeparatorChar + "localoptions.txt";
			if (File.Exists(path))
			{
				FromUnsyncedString(File.ReadAllText(path));
			}
		}
		else
		{
			LoadOptionsFallback();
		}
	}

	public void OnLoadFinished()
	{
		if (Screen.fullScreen != fullScreen || Screen.width != (int)ScreenSize.x || Screen.height != (int)ScreenSize.y || Futile.screen.pixelWidth != (int)ScreenSize.x || Futile.screen.pixelHeight != (int)ScreenSize.y)
		{
			Screen.SetResolution((int)ScreenSize.x, (int)ScreenSize.y, fullscreen: false);
			Screen.fullScreen = fullScreen;
			Futile.instance.UpdateScreenWidth((int)ScreenSize.x);
		}
		if (rainWorld.buildType != RainWorld.BuildType.Development && !ModManager.DevTools)
		{
			Cursor.visible = !fullScreen;
		}
		else
		{
			Cursor.visible = true;
		}
	}

	public bool DeveloperCommentaryLocalized()
	{
		if (!(language == InGameTranslator.LanguageID.English) && !(language == InGameTranslator.LanguageID.French))
		{
			return language == InGameTranslator.LanguageID.Korean;
		}
		return true;
	}

	public void Save()
	{
		if (optionsLoaded)
		{
			if (optionsFile != null && optionsFileCanSave)
			{
				optionsFile.Set("options", ToString(), (!optionsFileCanSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
			}
			File.WriteAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "localoptions.txt", ToStringNonSynced());
		}
	}

	public string LoadArenaSetup(string arenaSetupFallbackFile)
	{
		if (optionsLoaded && optionsFile != null && optionsFile.Contains("ArenaSetup"))
		{
			return optionsFile.Get("ArenaSetup", "");
		}
		if (File.Exists(arenaSetupFallbackFile.ToLowerInvariant()))
		{
			return File.ReadAllText(arenaSetupFallbackFile.ToLowerInvariant());
		}
		return null;
	}

	public void SaveArenaSetup(string arenaSetup)
	{
		if (optionsLoaded && optionsFile != null)
		{
			optionsFile.Set("ArenaSetup", arenaSetup, (!optionsFileCanSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
		}
	}

	public string LoadArenaSitting()
	{
		if (optionsLoaded)
		{
			if (optionsFile != null)
			{
				return optionsFile.Get("ArenaSitting", "");
			}
			if (arenaSittingLocalData != null)
			{
				return arenaSittingLocalData;
			}
		}
		return "";
	}

	public void SaveArenaSitting(string arenaSitting)
	{
		if (optionsLoaded)
		{
			if (optionsFile != null)
			{
				optionsFile.Set("ArenaSitting", arenaSitting, (!optionsFileCanSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
			}
			else
			{
				arenaSittingLocalData = arenaSitting;
			}
		}
	}

	public bool ContainsArenaSitting()
	{
		if (optionsLoaded)
		{
			if (optionsFile != null)
			{
				return optionsFile.Contains("ArenaSitting");
			}
			return arenaSittingLocalData != null;
		}
		return false;
	}

	public void DeleteArenaSitting()
	{
		if (optionsLoaded)
		{
			if (optionsFile != null)
			{
				optionsFile.Remove("ArenaSitting", (!optionsFileCanSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
			}
			else
			{
				arenaSittingLocalData = null;
			}
		}
	}

	public string LoadSandbox(string room, string sandboxRoomFallbackFile)
	{
		if (optionsLoaded)
		{
			if (optionsFile != null && optionsFile.Contains($"{room}_Sandbox"))
			{
				return optionsFile.Get($"{room}_Sandbox", "");
			}
			if (sandboxRoomLocalData != null && sandboxRoomLocalData.ContainsKey(room))
			{
				return sandboxRoomLocalData[room];
			}
		}
		if (File.Exists(sandboxRoomFallbackFile.ToLowerInvariant()))
		{
			return File.ReadAllText(sandboxRoomFallbackFile.ToLowerInvariant());
		}
		return null;
	}

	public void SaveSandbox(string room, string sandbox)
	{
		if (!optionsLoaded)
		{
			return;
		}
		if (optionsFile != null)
		{
			optionsFile.Set($"{room}_Sandbox", sandbox, (!optionsFileCanSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
			return;
		}
		if (sandboxRoomLocalData == null)
		{
			sandboxRoomLocalData = new Dictionary<string, string>();
		}
		if (sandboxRoomLocalData.ContainsKey(room))
		{
			sandboxRoomLocalData[room] = sandbox;
		}
		else
		{
			sandboxRoomLocalData.Add(room, sandbox);
		}
	}

	public void SetAchievementsFile()
	{
		if (optionsLoaded)
		{
			if (Profiles.ActiveProfiles.Count > 0)
			{
				Achievements.SetAchievementsFile(Profiles.ActiveProfiles[0], optionsFile);
				return;
			}
			Custom.LogWarning("There is no active profile for achievements!");
		}
		else
		{
			Custom.LogWarning("Trying to set achievements file before options are loaded!");
		}
	}

	public bool CheckLoadOldPS4SaveFile()
	{
		if (!oldPS4FileLoaded)
		{
			LoadOldPS4OptionsFile();
			return false;
		}
		return true;
	}

	public string GetOldPS4ProgressionSave()
	{
		if (oldPS4FileData.ContainsKey("rw_savedata"))
		{
			return oldPS4FileData["rw_savedata"];
		}
		return "";
	}

	public void ReassignAllJoysticks()
	{
		if (controls == null)
		{
			return;
		}
		for (int i = 0; i < controls.Length; i++)
		{
			controls[i].UpdateControlPreference(controls[i].GetControlPreference(), forceUpdate: true);
		}
		foreach (Rewired.Player allPlayer in ReInput.players.AllPlayers)
		{
			if (allPlayer.id == 4 || allPlayer.id == 9999999)
			{
				continue;
			}
			foreach (Controller controller in allPlayer.controllers.Controllers)
			{
				ReInput.userDataStore.LoadControllerData(allPlayer.id, controller.type, controller.id);
			}
		}
	}

	public string ToStringNonSynced()
	{
		string text = "";
		text = text + "ScreenResolution<optB>" + resolution + "<optA>";
		if (windowed)
		{
			text += "Windowed<optA>";
		}
		for (int i = 0; i < controls.Length; i++)
		{
			text += string.Format(CultureInfo.InvariantCulture, "InputSetup<optB>{0}<optB>{1}<optA>", i, controls[i]);
		}
		text += string.Format(CultureInfo.InvariantCulture, "SoundVol<optB>{0}<optA>", soundEffectsVolume);
		text += string.Format(CultureInfo.InvariantCulture, "MusicVol<optB>{0}<optA>", musicVolume);
		text += string.Format(CultureInfo.InvariantCulture, "ArenaMusicVol<optB>{0}<optA>", arenaMusicVolume);
		text = text + "AllGamePads<optB>" + (allGamePads ? "1" : "0") + "<optA>";
		text += string.Format(CultureInfo.InvariantCulture, "PlayerToSetInputFor<optB>{0}<optA>", playerToSetInputFor);
		text += string.Format(CultureInfo.InvariantCulture, "Vibration<optB>{0}<optA>", vibration ? 1 : 0);
		text += "LastGameVersion<optB>v1.9.15b<optA>";
		if (ModManager.MMF)
		{
			text += string.Format(CultureInfo.InvariantCulture, "AnalogSensitivity<optB>{0}<optA>", analogSensitivity);
			text = text + "Quality<optB>" + quality.value + "<optA>";
			if (vsync)
			{
				text += "VSync<optA>";
			}
		}
		foreach (string unrecognizedNonSyncedSaveString in unrecognizedNonSyncedSaveStrings)
		{
			text = text + unrecognizedNonSyncedSaveString + "<optA>";
		}
		return text;
	}

	public override string ToString()
	{
		string text = "";
		text = text + "ScreenResolution<optB>" + resolution + "<optA>";
		if (windowed)
		{
			text += "Windowed<optA>";
		}
		for (int i = 0; i < controls.Length; i++)
		{
			text += string.Format(CultureInfo.InvariantCulture, "InputSetup<optB>{0}<optB>{1}<optA>", i, controls[i]);
		}
		text += string.Format(CultureInfo.InvariantCulture, "SoundVol<optB>{0}<optA>", soundEffectsVolume);
		text += string.Format(CultureInfo.InvariantCulture, "MusicVol<optB>{0}<optA>", musicVolume);
		text += string.Format(CultureInfo.InvariantCulture, "ArenaMusicVol<optB>{0}<optA>", arenaMusicVolume);
		text += string.Format(CultureInfo.InvariantCulture, "TimeSinceLastSaveCopy<optB>{0}<optA>", timeSinceLastSaveCopy);
		text += string.Format(CultureInfo.InvariantCulture, "Language<optB>{0}<optA>", language);
		text += string.Format(CultureInfo.InvariantCulture, "SaveSlot<optB>{0}<optA>", saveSlot);
		text = text + "AllGamePads<optB>" + (allGamePads ? "1" : "0") + "<optA>";
		text += string.Format(CultureInfo.InvariantCulture, "PlayerToSetInputFor<optB>{0}<optA>", playerToSetInputFor);
		text += string.Format(CultureInfo.InvariantCulture, "Vibration<optB>{0}<optA>", vibration ? 1 : 0);
		if (enabledMods.Count > 0)
		{
			text = text + "EnabledMods<optB>" + string.Join("<optC>", enabledMods.ToArray()) + "<optA>";
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, int> item in modLoadOrder)
		{
			list.Add(item.Key + "<optD>" + item.Value);
		}
		text = text + "ModLoadOrder<optB>" + string.Join("<optC>", list.ToArray()) + "<optA>";
		List<string> list2 = new List<string>();
		foreach (KeyValuePair<string, string> modChecksum in modChecksums)
		{
			list2.Add(modChecksum.Key + "<optD>" + modChecksum.Value);
		}
		text = text + "ModChecksums<optB>" + string.Join("<optC>", list2.ToArray()) + "<optA>";
		if (validation)
		{
			text += "Validation<optA>";
		}
		if (remixTutorialShown)
		{
			text += "RemixTutorial<optA>";
		}
		if (dlcTutorialShown)
		{
			text += "DLCTutorial<optA>";
		}
		text += "LastGameVersion<optB>v1.9.15b<optA>";
		if (ModManager.MMF)
		{
			text += string.Format(CultureInfo.InvariantCulture, "FpsCap<optB>{0}<optA>", fpsCap);
			text += string.Format(CultureInfo.InvariantCulture, "AnalogSensitivity<optB>{0}<optA>", analogSensitivity);
			text = text + "Quality<optB>" + quality.value + "<optA>";
			text = text + "TitleBackground<optB>" + titleBackground.value + "<optA>";
			text = text + "SubBackground<optB>" + subBackground.value + "<optA>";
			if (vsync)
			{
				text += "VSync<optA>";
			}
		}
		if (ModManager.MSC && commentary)
		{
			text += "DevCommentary<optA>";
		}
		if (ModManager.JollyCoop)
		{
			text = text + "FriendLizards<optB>" + friendlyLizards + "<optA>";
			text = text + "JollyHud<optB>" + jollyHud + "<optA>";
			text = text + "FriendlyFire<optB>" + friendlyFire + "<optA>";
			text = text + "FriendlySteal<optB>" + friendlySteal + "<optA>";
			text = text + "SmartShortcuts<optB>" + smartShortcuts + "<optA>";
			text = text + "CameraCycling<optB>" + cameraCycling + "<optA>";
			text = text + "JollyDifficulty<optB>" + jollyDifficulty.value + "<optA>";
			text = text + "JollyCameraInputSpeed<optB>" + jollyCameraInputSpeed.value + "<optA>";
			text = text + "JollyColorMode<optB>" + jollyColorMode.value + "<optA>";
			text += "JollySetupPlayers<optB>";
			for (int j = 0; j < jollyPlayerOptionsArray.Length; j++)
			{
				text = text + jollyPlayerOptionsArray[j].ToString() + "<optC>";
			}
			text += "<optA>";
		}
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString + "<optA>";
		}
		return text;
	}

	public void FromString(string s)
	{
		validation = false;
		dlcTutorialShown = false;
		remixTutorialShown = false;
		lastGameVersion = null;
		commentary = false;
		vsync = false;
		enabledMods = new List<string>();
		modLoadOrder = new Dictionary<string, int>();
		friendlyLizards = true;
		jollyHud = true;
		friendlyFire = false;
		friendlySteal = true;
		smartShortcuts = true;
		cameraCycling = true;
		modChecksums = new Dictionary<string, string>();
		unrecognizedSaveStrings.Clear();
		string[] array = Regex.Split(s, "<optA>");
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], "<optB>");
			if (ApplyOption(array2) && array[i].Trim().Length > 0 && array2.Length >= 1)
			{
				unrecognizedSaveStrings.Add(array[i]);
			}
		}
		ReInput.userDataStore.Load();
		ModManager.RefreshModsLists(rainWorld);
	}

	public void FromUnsyncedString(string s)
	{
		lastGameVersion = null;
		vsync = false;
		unrecognizedNonSyncedSaveStrings.Clear();
		string[] array = Regex.Split(s, "<optA>");
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], "<optB>");
			if (ApplyOption(array2) && array[i].Trim().Length > 0 && array2.Length >= 1)
			{
				unrecognizedNonSyncedSaveStrings.Add(array[i]);
			}
		}
	}

	public void ReapplyUnrecognized()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < unrecognizedSaveStrings.Count; i++)
		{
			string[] splt = Regex.Split(unrecognizedSaveStrings[i], "<optB>");
			if (ApplyOption(splt))
			{
				list.Add(unrecognizedSaveStrings[i]);
			}
		}
		unrecognizedSaveStrings = list;
		List<string> list2 = new List<string>();
		for (int j = 0; j < unrecognizedNonSyncedSaveStrings.Count; j++)
		{
			string[] splt2 = Regex.Split(unrecognizedNonSyncedSaveStrings[j], "<optB>");
			if (ApplyOption(splt2))
			{
				list2.Add(unrecognizedNonSyncedSaveStrings[j]);
			}
		}
		unrecognizedNonSyncedSaveStrings = list2;
	}

	public bool ApplyOption(string[] splt2)
	{
		bool result = false;
		switch (splt2[0])
		{
		case "ScreenResolution":
			resolution = int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			break;
		case "Windowed":
			windowed = true;
			break;
		case "InputSetup":
			controls[int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture)].FromString(splt2[2]);
			break;
		case "SoundVol":
			soundEffectsVolume = float.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			break;
		case "MusicVol":
			musicVolume = float.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			break;
		case "ArenaMusicVol":
			arenaMusicVolume = float.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			break;
		case "TimeSinceLastSaveCopy":
			timeSinceLastSaveCopy = int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			break;
		case "Language":
		{
			InGameTranslator.LanguageID lang = language;
			if (int.TryParse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
			{
				language = BackwardsCompatibilityRemix.ParseLanguage(result2);
			}
			else
			{
				language = new InGameTranslator.LanguageID(splt2[1]);
			}
			InGameTranslator.UnloadFonts(lang);
			InGameTranslator.LoadFonts(rainWorld.inGameTranslator.currentLanguage, null);
			break;
		}
		case "SaveSlot":
			saveSlot = int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			break;
		case "AllGamePads":
			allGamePads = splt2[1] == "1";
			break;
		case "PlayerToSetInputFor":
			playerToSetInputFor = int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			break;
		case "Vibration":
			vibration = int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture) == 1;
			break;
		case "Validation":
			validation = true;
			break;
		case "RemixTutorial":
			remixTutorialShown = true;
			break;
		case "DLCTutorial":
			dlcTutorialShown = true;
			break;
		case "LastGameVersion":
			lastGameVersion = splt2[1];
			break;
		case "EnabledMods":
		{
			string[] array7 = Regex.Split(splt2[1], "<optC>");
			for (int l = 0; l < array7.Length; l++)
			{
				if (array7[l] != string.Empty)
				{
					enabledMods.Add(array7[l]);
				}
			}
			break;
		}
		case "ModLoadOrder":
		{
			string[] array5 = Regex.Split(splt2[1], "<optC>");
			for (int k = 0; k < array5.Length; k++)
			{
				if (array5[k].Contains("<optD>"))
				{
					string[] array6 = Regex.Split(array5[k], "<optD>");
					modLoadOrder[array6[0]] = int.Parse(array6[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
			}
			break;
		}
		case "FpsCap":
			if (ModManager.MMF)
			{
				fpsCap = int.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			else
			{
				result = true;
			}
			break;
		case "VSync":
			if (ModManager.MMF)
			{
				vsync = true;
			}
			else
			{
				result = true;
			}
			break;
		case "AnalogSensitivity":
			if (ModManager.MMF)
			{
				analogSensitivity = float.Parse(splt2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			else
			{
				result = true;
			}
			break;
		case "Quality":
			if (ModManager.MMF)
			{
				quality = new Quality(splt2[1]);
			}
			else
			{
				result = true;
			}
			break;
		case "TitleBackground":
			if (ModManager.MMF)
			{
				titleBackground = new MenuScene.SceneID(splt2[1]);
			}
			else
			{
				result = true;
			}
			break;
		case "SubBackground":
			if (ModManager.MMF)
			{
				subBackground = new MenuScene.SceneID(splt2[1]);
			}
			else
			{
				result = true;
			}
			break;
		case "DevCommentary":
			if (ModManager.MSC)
			{
				commentary = true;
			}
			else
			{
				result = true;
			}
			break;
		case "JollyDifficulty":
			if (ModManager.JollyCoop)
			{
				jollyDifficulty = new JollyDifficulty(splt2[1]);
			}
			else
			{
				result = true;
			}
			break;
		case "JollyCameraInputSpeed":
			if (ModManager.JollyCoop)
			{
				jollyCameraInputSpeed = new JollyCameraInputSpeed(splt2[1]);
			}
			else
			{
				result = true;
			}
			break;
		case "JollyColorMode":
			if (ModManager.JollyCoop)
			{
				jollyColorMode = new JollyColorMode(splt2[1]);
			}
			else
			{
				result = true;
			}
			break;
		case "FriendlyFire":
			if (ModManager.JollyCoop)
			{
				friendlyFire = splt2[1].ToLower() == "true";
			}
			else
			{
				result = true;
			}
			break;
		case "FriendlySteal":
			if (ModManager.JollyCoop)
			{
				friendlySteal = splt2[1].ToLower() == "true";
			}
			else
			{
				result = true;
			}
			break;
		case "JollyHud":
			if (ModManager.JollyCoop)
			{
				jollyHud = splt2[1].ToLower() == "true";
			}
			else
			{
				result = true;
			}
			break;
		case "SmartShortcuts":
			if (ModManager.JollyCoop)
			{
				smartShortcuts = splt2[1].ToLower() == "true";
			}
			else
			{
				result = true;
			}
			break;
		case "CameraCycling":
			if (ModManager.JollyCoop)
			{
				cameraCycling = splt2[1].ToLower() == "true";
			}
			else
			{
				result = true;
			}
			break;
		case "FriendLizards":
			if (ModManager.JollyCoop)
			{
				friendlyLizards = splt2[1].ToLower() == "true";
			}
			else
			{
				result = true;
			}
			break;
		case "JollySetupPlayers":
			if (ModManager.JollyCoop)
			{
				string[] array3 = Regex.Split(splt2[1], "<optC>");
				int num = 0;
				string[] array4 = array3;
				foreach (string text in array4)
				{
					if (!(text == string.Empty))
					{
						JollyPlayerOptions jollyPlayerOptions = new JollyPlayerOptions(num);
						jollyPlayerOptions.FromString(text);
						jollyPlayerOptionsArray[jollyPlayerOptions.playerNumber] = jollyPlayerOptions;
						num++;
					}
				}
			}
			else
			{
				result = true;
			}
			break;
		case "ModChecksums":
		{
			string[] array = Regex.Split(splt2[1], "<optC>");
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Contains("<optD>"))
				{
					string[] array2 = Regex.Split(array[i], "<optD>");
					modChecksums[array2[0]] = array2[1];
				}
			}
			break;
		}
		default:
			result = true;
			break;
		}
		return result;
	}

	private void LoadOptions()
	{
		if (Profiles.ActiveProfiles.Count > 0)
		{
			UserData.OnFileMounted += UserData_OnFileMounted;
			UserData.FileDefinition fileDefinition = new UserData.FileDefinition(OPTIONS_FILE_DEFINITION);
			fileDefinition.ps4Definition.title = rainWorld.inGameTranslator.Translate("ps4_save_options_title");
			fileDefinition.ps4Definition.detail = rainWorld.inGameTranslator.Translate("ps4_save_options_description");
			UserData.Mount(Profiles.ActiveProfiles[0], null, fileDefinition);
			return;
		}
		LoadOptionsFallback();
		optionsLoaded = true;
		if (rainWorld.OptionsReady)
		{
			Platform.NotifyUserDataReadCompleted(this);
			OnLoadFinished();
		}
	}

	private void LoadOptionsFallback()
	{
		language = InGameTranslator.systemLanguage;
		if (File.Exists(Custom.LegacyRootFolderDirectory() + (Path.DirectorySeparatorChar + "UserData" + Path.DirectorySeparatorChar + "options.txt").ToLowerInvariant()))
		{
			FromString(File.ReadAllText(Custom.LegacyRootFolderDirectory() + (Path.DirectorySeparatorChar + "UserData" + Path.DirectorySeparatorChar + "options.txt").ToLowerInvariant()));
			foundOldOptionsFile = true;
		}
		string path = Application.persistentDataPath + Path.DirectorySeparatorChar + "localoptions.txt";
		if (File.Exists(path))
		{
			FromUnsyncedString(File.ReadAllText(path));
		}
	}

	private void LoadControllerOptions()
	{
		foreach (Rewired.Player player in ReInput.players.Players)
		{
			player.controllers.maps.SetMapsEnabled(enterButton == RainWorldPlugin.EnterButton.Cross, ControllerType.Joystick, 1, 0);
			player.controllers.maps.SetMapsEnabled(enterButton == RainWorldPlugin.EnterButton.Circle, ControllerType.Joystick, 1, 1);
		}
		UserInput.OnControllerConnected += UserInput_OnControllerConnected;
		UserInput.AfterControllerConnectedChange += UserInput_AfterControllersConnected;
	}

	private void UserInput_OnControllerConnected(Profiles.Profile profile, int controllerId)
	{
		Rewired.Player rewiredPlayer = UserInput.GetRewiredPlayer(profile, -1);
		if (rewiredPlayer != null)
		{
			Custom.Log("Setting controller maps enabled!");
			rewiredPlayer.controllers.maps.SetMapsEnabled(enterButton == RainWorldPlugin.EnterButton.Cross, ControllerType.Joystick, 1, 0);
			rewiredPlayer.controllers.maps.SetMapsEnabled(enterButton == RainWorldPlugin.EnterButton.Circle, ControllerType.Joystick, 1, 1);
		}
	}

	private void UserInput_AfterControllersConnected(Profiles.Profile profile)
	{
	}

	private void Platform_OnRequestUserDataRead(List<object> pendingUserDataReads)
	{
		Platform.OnRequestUserDataRead -= Platform_OnRequestUserDataRead;
		pendingUserDataReads.Add(this);
		LoadOptions();
	}

	private void UserData_OnFileMounted(UserData.File file, UserData.Result result)
	{
		if (file.filename != OPTIONS_FILE_DEFINITION.fileName)
		{
			return;
		}
		UserData.OnFileMounted -= UserData_OnFileMounted;
		if (result.IsSuccess())
		{
			optionsFile = file;
			optionsFile.OnReadCompleted += OptionsFile_OnReadCompleted;
			optionsFile.Read();
			return;
		}
		optionsFile = null;
		LoadOptionsFallback();
		optionsLoaded = true;
		if (rainWorld.OptionsReady)
		{
			Platform.NotifyUserDataReadCompleted(this);
			OnLoadFinished();
		}
	}

	private void OptionsFile_OnReadCompleted(UserData.File file, UserData.Result result)
	{
		if (!rainWorld.processManagerInitialized)
		{
			rainWorld.ProcessManagerInitializsation();
		}
		optionsFile.OnReadCompleted -= OptionsFile_OnReadCompleted;
		if (result.IsSuccess())
		{
			if (result.Contains(UserData.Result.FileNotFound))
			{
				optionsFile.OnWriteCompleted += OptionsFile_OnWriteCompleted_NewFile;
				LoadOptionsFallback();
				optionsFile.Write();
				return;
			}
			optionsLoaded = true;
			Load();
			if (rainWorld.OptionsReady)
			{
				Platform.NotifyUserDataReadCompleted(this);
				OnLoadFinished();
			}
		}
		else if (result.Contains(UserData.Result.CorruptData))
		{
			string text = rainWorld.inGameTranslator.Translate("ps4_load_options_failed");
			Vector2 size = DialogBoxNotify.CalculateDialogBoxSize(text, dialogUsesWordWrapping: false);
			DialogNotify dialog = new DialogNotify(text, size, rainWorld.processManager, delegate
			{
				optionsFile.OnDeleteCompleted += OptionsFile_OnDeleteCompleted;
				optionsFile.Delete();
			});
			rainWorld.processManager.ShowDialog(dialog);
		}
		else if (result.Contains(UserData.Result.FileNotFound))
		{
			optionsFile.OnWriteCompleted += OptionsFile_OnWriteCompleted_NewFile;
			LoadOptionsFallback();
			optionsFile.Write();
		}
		else
		{
			LoadOptionsFallback();
			optionsLoaded = true;
			if (rainWorld.OptionsReady)
			{
				Platform.NotifyUserDataReadCompleted(this);
				OnLoadFinished();
			}
		}
	}

	private void OptionsFile_OnDeleteCompleted(UserData.File file, UserData.Result result)
	{
		optionsFile.OnDeleteCompleted -= OptionsFile_OnDeleteCompleted;
		if (result.IsSuccess())
		{
			optionsFile.OnWriteCompleted += OptionsFile_OnWriteCompleted_NewFile;
			LoadOptionsFallback();
			optionsFile.Write();
			return;
		}
		optionsFile.Unmount();
		optionsFile = null;
		LoadOptionsFallback();
		optionsLoaded = true;
		if (rainWorld.OptionsReady)
		{
			Platform.NotifyUserDataReadCompleted(this);
			OnLoadFinished();
		}
	}

	private void OptionsFile_OnWriteCompleted_NewFile(UserData.File file, UserData.Result result)
	{
		if (!rainWorld.processManagerInitialized)
		{
			rainWorld.ProcessManagerInitializsation();
		}
		optionsFile.OnWriteCompleted -= OptionsFile_OnWriteCompleted_NewFile;
		if (result.IsSuccess())
		{
			createdNewThisLaunch = !foundOldOptionsFile;
			optionsLoaded = true;
			if (rainWorld.OptionsReady)
			{
				Platform.NotifyUserDataReadCompleted(this);
				OnLoadFinished();
			}
			return;
		}
		if (result.Contains(UserData.Result.NoFreeSpace))
		{
			DialogConfirm dialog = new DialogConfirm(rainWorld.inGameTranslator.Translate("ps4_save_options_failed_free_space"), rainWorld.processManager, delegate
			{
				optionsFile.OnWriteCompleted += OptionsFile_OnWriteCompleted_NewFile;
				optionsFile.Write();
			}, delegate
			{
				optionsFileCanSave = false;
				optionsLoaded = true;
				if (rainWorld.OptionsReady)
				{
					Platform.NotifyUserDataReadCompleted(this);
					OnLoadFinished();
				}
			});
			rainWorld.processManager.ShowDialog(dialog);
			return;
		}
		optionsFileCanSave = false;
		string text = rainWorld.inGameTranslator.Translate("ps4_save_options_failed");
		Vector2 size = DialogBoxNotify.CalculateDialogBoxSize(text, dialogUsesWordWrapping: false);
		DialogNotify dialog2 = new DialogNotify(text, size, rainWorld.processManager, delegate
		{
			optionsLoaded = true;
			if (rainWorld.OptionsReady)
			{
				Platform.NotifyUserDataReadCompleted(this);
				OnLoadFinished();
			}
		});
		rainWorld.processManager.ShowDialog(dialog2);
	}

	private void LoadOldPS4OptionsFile()
	{
		if (Profiles.ActiveProfiles.Count == 0)
		{
			Custom.LogWarning("No profile to load old PS4 options from");
			return;
		}
		UserData.OnFileMounted += UserData_OnOldPS4FileMounted;
		UserData.FileDefinition fileDefinition = new UserData.FileDefinition(OLD_PS4_SAVE_FILE_DEFINITION);
		fileDefinition.ps4Definition.title = "SaveData";
		fileDefinition.ps4Definition.detail = "SaveData";
		UserData.Mount(Profiles.ActiveProfiles[0], null, fileDefinition);
	}

	private void UserData_OnOldPS4FileMounted(UserData.File file, UserData.Result result)
	{
		UserData.OnFileMounted -= UserData_OnOldPS4FileMounted;
		if (result.IsSuccess())
		{
			oldPS4SaveFile = file;
			oldPS4SaveFile.OnReadCompleted += OldPS4SaveFile_OnReadCompleted;
			oldPS4SaveFile.Read();
			return;
		}
		oldPS4FileLoaded = true;
		if (this.onOldPS4SaveLoaded != null)
		{
			this.onOldPS4SaveLoaded(obj: false);
		}
	}

	private void OldPS4SaveFile_OnReadCompleted(UserData.File file, UserData.Result result)
	{
		bool obj = false;
		oldPS4SaveFile.OnReadCompleted -= OldPS4SaveFile_OnReadCompleted;
		if (result.IsSuccess() && !result.Contains(UserData.Result.FileNotFound))
		{
			byte[] bytes = oldPS4SaveFile.GetBytes();
			if (bytes != null)
			{
				using MemoryStream input = new MemoryStream(bytes);
				try
				{
					BinaryReader binaryReader = new BinaryReader(input);
					if (binaryReader.ReadInt32() == 101)
					{
						int num = binaryReader.ReadInt32();
						for (int i = 0; i < num; i++)
						{
							string key = binaryReader.ReadString();
							string value = binaryReader.ReadString();
							oldPS4FileSettings.Add(key, value);
						}
						int num2 = binaryReader.ReadInt32();
						for (int j = 0; j < num2; j++)
						{
							string key2 = binaryReader.ReadString();
							string value2 = binaryReader.ReadString();
							oldPS4FileData.Add(key2, value2);
						}
					}
					binaryReader.Close();
				}
				catch
				{
				}
			}
			obj = true;
		}
		oldPS4FileLoaded = true;
		if (this.onOldPS4SaveLoaded != null)
		{
			this.onOldPS4SaveLoaded(obj);
		}
	}

	private void Options_OnOldPS4SaveLoaded(bool success)
	{
		onOldPS4SaveLoaded -= Options_OnOldPS4SaveLoaded;
		if (success)
		{
			if (oldPS4FileData.ContainsKey("rw_options"))
			{
				string s = oldPS4FileData["rw_options"];
				FromString(s);
			}
		}
		else
		{
			LoadOptionsFallback();
		}
		optionsFile.OnWriteCompleted += OptionsFile_OnWriteCompleted_NewFile;
		optionsFile.Write();
	}
}
