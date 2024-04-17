using System;
using System.Linq;
using Rewired;
using RWCustom;
using UnityEngine;

internal static class RWInput
{
	private static readonly AGLog<FromStaticClass> Log = new AGLog<FromStaticClass>();

	public static bool IsXboxControllerType(string deviceName, string hid)
	{
		if (deviceName.ToLower().Contains("xbox") || deviceName.ToLower().Contains("x-box") || deviceName.ToLower().Contains("xinput"))
		{
			return true;
		}
		if (hid.Contains("-"))
		{
			string[] array = new string[211]
			{
				"18d40079", "b326044f", "028e045e", "028f045e", "0291045e", "02a0045e", "02a1045e", "02d1045e", "02dd045e", "02e0045e",
				"02e3045e", "02ea045e", "02fd045e", "02ff045e", "0719045e", "c21d046d", "c21e046d", "c21f046d", "c242046d", "2004056e",
				"f51a06a3", "47160738", "47180738", "47260738", "47280738", "47360738", "47380738", "47400738", "4a010738", "b7260738",
				"beef0738", "cb020738", "cb030738", "f7380738", "b4000955", "01050e6f", "01130e6f", "011f0e6f", "01310e6f", "01330e6f",
				"01390e6f", "013a0e6f", "01460e6f", "01470e6f", "015c0e6f", "01610e6f", "01620e6f", "01630e6f", "01640e6f", "01650e6f",
				"02010e6f", "02130e6f", "021f0e6f", "02460e6f", "02a00e6f", "03010e6f", "03460e6f", "04010e6f", "04130e6f", "05010e6f",
				"f5010e6f", "f9000e6f", "000a0f0d", "000c0f0d", "000d0f0d", "00160f0d", "001b0f0d", "00630f0d", "00670f0d", "00780f0d",
				"008c0f0d", "55f011c9", "000412ab", "030112ab", "030312ab", "02a01430", "47481430", "f8011430", "0601146b", "00371532",
				"0a001532", "0a031532", "3f0015e4", "3f0a15e4", "3f1015e4", "beef162e", "fd001689", "fd011689", "fe001689", "00021bad",
				"00031bad", "f0161bad", "f0181bad", "f0191bad", "f0211bad", "f0231bad", "f0251bad", "f0271bad", "f0281bad", "f02e1bad",
				"f0361bad", "f0381bad", "f0391bad", "f03a1bad", "f03d1bad", "f03e1bad", "f03f1bad", "f0421bad", "f0801bad", "f5011bad",
				"f5021bad", "f5031bad", "f5041bad", "f5051bad", "f5061bad", "f9001bad", "f9011bad", "f9021bad", "f9031bad", "f9041bad",
				"f9061bad", "fa011bad", "fd001bad", "fd011bad", "500024c6", "530024c6", "530324c6", "530a24c6", "531a24c6", "539724c6",
				"541a24c6", "542a24c6", "543a24c6", "550024c6", "550124c6", "550224c6", "550324c6", "550624c6", "551024c6", "550d24c6",
				"550e24c6", "551a24c6", "561a24c6", "5b0024c6", "5b0224c6", "5b0324c6", "5d0424c6", "fafa24c6", "fafb24c6", "fafc24c6",
				"fafe24c6", "fafd24c6", "72100955", "02050e6f", "00000000", "02a2045e", "14140e6f", "13140e6f", "01590e6f", "faff24c6",
				"00860f0d", "006d0f0d", "00a40f0d", "18320079", "187f0079", "18830079", "ff0103eb", "23032c22", "0ef80c12", "1000046d",
				"60061345", "2012056e", "0602146b", "00ae0f0d", "0603146b", "2013056e", "0401046d", "0301046d", "caa3046d", "c261046d",
				"0291046d", "18d30079", "00b10f0d", "00010001", "60051345", "188e0079", "18d40079", "20032c22", "00b10f0d", "187c0079",
				"189c0079", "18740079", "00502f24", "581a24c6", "2e2f24", "249886", "912f24", "2a4e6f", "7191430", "edf0d",
				"ff023eb", "c0f0d", "152e6f", "2a7e6f", "2a6e6f", "100746d", "2b8e6f", "2a8e6f", "25032c22", "18a179",
				"b3601038"
			};
			string text = hid.Substring(0, hid.IndexOf("-"));
			for (int i = 0; i < array.Length; i++)
			{
				if (text.EndsWith(array[i]))
				{
					return true;
				}
			}
		}
		if (new string[9] { "Microsoft Wireless 360 Controller", "Mad Catz, Inc. Mad Catz FPS Pro GamePad", "©Microsoft Corporation Controller", "Controller (Infinity Controller 360)", "Controller (Mad Catz FPS Pro GamePad)", "Controller (MadCatz Call of Duty GamePad)", "Controller (MadCatz GamePad)", "Controller (Razer Sabertooth Elite)", "Controller (XEOX Gamepad)" }.Contains(deviceName))
		{
			return true;
		}
		return false;
	}

	public static bool IsPlaystationControllerType(string deviceName, string hid)
	{
		if (deviceName.ToLower().Contains("playstation") || deviceName.ToLower().Contains("ps3") || deviceName.ToLower().Contains("ps4") || deviceName.ToLower().Contains("ps5"))
		{
			return true;
		}
		if (hid.Contains("-"))
		{
			string[] array = new string[90]
			{
				"0268054c", "00050925", "03088888", "08361a34", "006e0f0d", "00660f0d", "005f0f0d", "005e0f0d", "32500738", "82500738",
				"181a0079", "00060079", "18440079", "03088888", "05752563", "00010810", "00030810", "05232563", "333111ff", "550020bc",
				"100405b8", "0603146b", "b315044f", "88880925", "004d0f0d", "00090f0d", "00080e8f", "006a0f0d", "011e0e6f", "02140e6f",
				"88660925", "310d0e8f", "20032c22", "2013056e", "88380738", "08361a34", "11000f30", "60051345", "00870f0d", "5500146b",
				"ca6d20d6", "c12125f0", "00038380", "10001345", "30750e8f", "01280e6f", "20002c22", "f62206a3", "d007044f", "83c325f0",
				"100605b8", "576d20d6", "63020e6f", "200f056e", "13140e6f", "31800738", "81800738", "02030e6f", "05c4054c", "09cc054c",
				"0ba0054c", "008a0f0d", "00550f0d", "00660f0d", "83840738", "82500738", "0E100C12", "1CF60C12", "10001532", "04011532",
				"05c5054c", "0d01146b", "0d02146b", "00a00f0d", "009c0f0d", "0ef60c12", "181b0079", "32500738", "00ee0f0d", "84810738",
				"84800738", "01047545", "0e150c12", "400111c0", "10071532", "100A1532", "10041532", "10091532", "10081532", "00259886"
			};
			string text = hid.Substring(0, hid.IndexOf("-"));
			for (int i = 0; i < array.Length; i++)
			{
				if (text.EndsWith(array[i]))
				{
					return true;
				}
			}
		}
		if (new string[3] { "Twin USB Joystick", "MotioninJoy Virtual Game Controller", "Sony Computer Entertainment Wireless Controller" }.Contains(deviceName))
		{
			return true;
		}
		return false;
	}

	public static bool IsSwitchProControllerType(string deviceName, string hid)
	{
		if (deviceName.ToLower().Contains("nintendo switch") || deviceName.ToLower().Contains("switch pad"))
		{
			return true;
		}
		if (hid.Contains("-"))
		{
			string[] array = new string[9] { "2009057e", "00c10f0d", "00920f0d", "00f60f0d", "00dc0f0d", "a71120d6", "01850e6f", "01800e6f", "01810e6f" };
			string text = hid.Substring(0, hid.IndexOf("-"));
			for (int i = 0; i < array.Length; i++)
			{
				if (text.EndsWith(array[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void RefreshSystemControllers()
	{
		ReInput.players.SystemPlayer.controllers.ClearAllControllers();
		ReInput.players.SystemPlayer.controllers.AddController(ControllerType.Keyboard, 0, removeFromOtherPlayers: false);
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			ReInput.players.SystemPlayer.controllers.AddController(joystick, removeFromOtherPlayers: false);
		}
	}

	public static bool CheckPauseButton(int playerNumber)
	{
		return CheckPauseButton(playerNumber, inMenu: true);
	}

	public static bool CheckPauseButton(int playerNumber, bool inMenu = true)
	{
		if (inMenu && ReInput.controllers.Mouse.GetButton(0))
		{
			return false;
		}
		if (!CheckSpecificButton(playerNumber, 5))
		{
			if (Custom.rainWorld.options.controls[playerNumber].GetControlPreference() == Options.ControlSetup.ControlToUse.KEYBOARD)
			{
				return Input.GetKey(KeyCode.Escape);
			}
			return false;
		}
		return true;
	}

	[Obsolete("Use version without rainWorld parameter.")]
	public static bool CheckPauseButton(int playerNumber, RainWorld rainWorld)
	{
		return CheckPauseButton(playerNumber);
	}

	public static bool CheckSpecificButton(int playerNumber, int actionID)
	{
		return Custom.rainWorld.options.controls[playerNumber].GetButton(actionID);
	}

	[Obsolete("Use version without rainWorld parameter.")]
	public static bool CheckSpecificButton(int playerNumber, int actionID, RainWorld rainWorld)
	{
		return CheckSpecificButton(playerNumber, actionID);
	}

	public static bool CheckSpecificButtonDown(int playerNumber, int actionID)
	{
		return Custom.rainWorld.options.controls[playerNumber].GetButtonDown(actionID);
	}

	public static float CheckSpecificAxis(int playerNumber, int actionID)
	{
		return Custom.rainWorld.options.controls[playerNumber].GetAxis(actionID);
	}

	private static Player.InputPackage PlayerInputLogic(int categoryID, int playerNumber)
	{
		Player.InputPackage result = default(Player.InputPackage);
		Controller newController = PlayerRecentController(playerNumber);
		Custom.rainWorld.options.controls[playerNumber].UpdateActiveController(newController);
		result.controllerType = Custom.rainWorld.options.controls[playerNumber].GetActivePreset();
		result.gamePad = result.controllerType != Options.ControlSetup.Preset.KeyboardSinglePlayer && result.controllerType != Options.ControlSetup.Preset.None;
		Options.ControlSetup controlSetup = Custom.rainWorld.options.controls[playerNumber];
		switch (categoryID)
		{
		case 0:
			if (controlSetup.GetButton(0))
			{
				result.jmp = true;
			}
			if (controlSetup.GetButton(4))
			{
				result.thrw = true;
			}
			if (controlSetup.GetButton(11))
			{
				result.mp = true;
			}
			if (controlSetup.GetButton(3))
			{
				result.pckp = true;
			}
			result.analogueDir = new Vector2(controlSetup.GetAxis(1), controlSetup.GetAxis(2));
			break;
		case 1:
			if (controlSetup.GetButton(8))
			{
				result.jmp = true;
			}
			if (controlSetup.GetButton(9))
			{
				result.thrw = true;
			}
			if (controlSetup.GetButton(13))
			{
				result.mp = true;
			}
			result.analogueDir = new Vector2(controlSetup.GetAxis(6), controlSetup.GetAxis(7));
			break;
		}
		result.analogueDir = Vector2.ClampMagnitude(result.analogueDir * (ModManager.MMF ? Custom.rainWorld.options.analogSensitivity : 1f), 1f);
		if (Custom.rainWorld.options.controls[playerNumber].xInvert)
		{
			result.analogueDir.x *= -1f;
		}
		if (Custom.rainWorld.options.controls[playerNumber].yInvert)
		{
			result.analogueDir.y *= -1f;
		}
		if (result.analogueDir.x < -0.5f)
		{
			result.x = -1;
		}
		if (result.analogueDir.x > 0.5f)
		{
			result.x = 1;
		}
		if (result.analogueDir.y < -0.5f)
		{
			result.y = -1;
		}
		if (result.analogueDir.y > 0.5f)
		{
			result.y = 1;
		}
		if (ModManager.MMF)
		{
			if (result.analogueDir.y < -0.05f || result.y < 0)
			{
				if (result.analogueDir.x < -0.05f || result.x < 0)
				{
					result.downDiagonal = -1;
				}
				else if (result.analogueDir.x > 0.05f || result.x > 0)
				{
					result.downDiagonal = 1;
				}
			}
		}
		else if (result.analogueDir.y < -0.05f)
		{
			if (result.analogueDir.x < -0.05f)
			{
				result.downDiagonal = -1;
			}
			else if (result.analogueDir.x > 0.05f)
			{
				result.downDiagonal = 1;
			}
		}
		return result;
	}

	[Obsolete("Use version without rainWorld parameter.")]
	private static Player.InputPackage PlayerInputLogic(int categoryID, int playerNumber, RainWorld rainWorld)
	{
		return PlayerInputLogic(categoryID, playerNumber);
	}

	public static Player.InputPackage PlayerInput(int playerNumber)
	{
		return PlayerInputLogic(0, playerNumber);
	}

	[Obsolete("Use version without rainWorld parameter.")]
	public static Player.InputPackage PlayerInput(int playerNumber, RainWorld rainWorld)
	{
		return PlayerInput(playerNumber);
	}

	public static Player.InputPackage PlayerUIInput(int playerNumber)
	{
		if ((Custom.rainWorld.processManager == null || !Custom.rainWorld.processManager.IsGameInMultiplayerContext()) && playerNumber < 0)
		{
			playerNumber = 0;
		}
		if (playerNumber >= 0)
		{
			return PlayerInputLogic(1, playerNumber);
		}
		Player.InputPackage[] array = new Player.InputPackage[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = PlayerInputLogic(1, i);
		}
		bool gamePad = false;
		Options.ControlSetup.Preset controllerType = Options.ControlSetup.Preset.KeyboardSinglePlayer;
		int num = 0;
		int num2 = 0;
		bool jmp = false;
		bool thrw = false;
		bool pckp = false;
		bool mp = false;
		bool crouchToggle = false;
		int num3 = 0;
		Vector2 zero = Vector2.zero;
		for (int j = 0; j < 4; j++)
		{
			bool flag = false;
			if (Math.Abs(array[j].x) > Math.Abs(num))
			{
				num = array[j].x;
				flag = true;
			}
			if (Math.Abs(array[j].y) > Math.Abs(num2))
			{
				num2 = array[j].y;
				flag = true;
			}
			if (Custom.rainWorld.options.controls[j].xInvert)
			{
				array[j].analogueDir.x *= -1f;
			}
			if (Custom.rainWorld.options.controls[j].yInvert)
			{
				array[j].analogueDir.y *= -1f;
			}
			if (Math.Abs(array[j].analogueDir.x) > Math.Abs(zero.x))
			{
				zero.x = array[j].analogueDir.x;
				flag = true;
			}
			if (Math.Abs(array[j].analogueDir.y) > Math.Abs(zero.y))
			{
				zero.y = array[j].analogueDir.y;
				flag = true;
			}
			if (Math.Abs(array[j].downDiagonal) > Math.Abs(num3))
			{
				num3 = array[j].downDiagonal;
				flag = true;
			}
			if (array[j].jmp)
			{
				jmp = true;
				flag = true;
			}
			if (array[j].thrw)
			{
				thrw = true;
				flag = true;
			}
			if (array[j].pckp)
			{
				pckp = true;
				flag = true;
			}
			if (array[j].mp)
			{
				mp = true;
				flag = true;
			}
			if (array[j].crouchToggle)
			{
				crouchToggle = true;
				flag = true;
			}
			if (flag)
			{
				controllerType = array[j].controllerType;
				gamePad = array[j].gamePad;
			}
		}
		Player.InputPackage result = new Player.InputPackage(gamePad, controllerType, num, num2, jmp, thrw, pckp, mp, crouchToggle);
		result.analogueDir = zero;
		result.downDiagonal = num3;
		return result;
	}

	[Obsolete("Use version without rainWorld parameter.")]
	public static Player.InputPackage PlayerUIInput(int playerNumber, RainWorld rainWorld)
	{
		return PlayerUIInput(playerNumber);
	}

	public static Controller PlayerRecentController(int playerNumber)
	{
		if (Custom.rainWorld.options == null || Custom.rainWorld.options.controls == null || Custom.rainWorld.options.controls[playerNumber] == null)
		{
			return null;
		}
		Options.ControlSetup controlSetup = Custom.rainWorld.options.controls[playerNumber];
		Rewired.Player player = controlSetup.player;
		Controller controller = null;
		double num = 0.0;
		foreach (Controller controller2 in player.controllers.Controllers)
		{
			if (controller2.type == ControllerType.Keyboard || controller2.type == ControllerType.Joystick)
			{
				double num2 = controller2.GetLastTimeActive();
				if (controller2.type == ControllerType.Keyboard && Custom.rainWorld.lastMouseActiveTime > num2)
				{
					num2 = Custom.rainWorld.lastMouseActiveTime;
				}
				if (controller == null || num2 > num)
				{
					controller = controller2;
					num = num2;
				}
			}
		}
		if (controller != null)
		{
			Controller activeController = controlSetup.GetActiveController();
			if (activeController == null || activeController.name != controller.name || activeController.deviceInstanceGuid != controller.deviceInstanceGuid)
			{
				controlSetup.UpdateActiveController(controller);
			}
			return controller;
		}
		return null;
	}

	[Obsolete("Use version without rainWorld parameter.")]
	public static Controller PlayerRecentController(int playerNumber, RainWorld rainWorld)
	{
		return PlayerRecentController(playerNumber);
	}
}
