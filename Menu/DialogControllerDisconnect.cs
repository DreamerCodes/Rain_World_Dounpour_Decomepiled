using Kittehface.Framework20;
using Rewired;
using UnityEngine;

namespace Menu;

public class DialogControllerDisconnect : Dialog
{
	private SimpleButton cancelButton;

	private int playerIndex;

	private int exitTimer;

	public DialogControllerDisconnect(string description, ProcessManager manager, int playerIndex, bool allowCancel = false)
		: base(description, manager)
	{
		this.playerIndex = playerIndex;
		exitTimer = -1;
		if (allowCancel)
		{
			cancelButton = new SimpleButton(this, pages[0], Translate("cancel"), "CANCEL", new Vector2(pos.x + (size.x - 110f) * 0.5f, pos.y + Mathf.Max(size.y * 0.04f, 7f)), new Vector2(110f, 30f));
			pages[0].subObjects.Add(cancelButton);
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message != null && message == "CANCEL")
		{
			if (playerIndex != 0)
			{
				manager.rainWorld.DeactivatePlayer(playerIndex);
			}
			manager.StopSideProcess(this);
		}
	}

	public override void Update()
	{
		base.Update();
		bool flag = false;
		Rewired.Player player = null;
		Profiles.Profile profile = null;
		PlayerHandler playerHandler = manager.rainWorld.GetPlayerHandler(playerIndex);
		if (playerHandler != null)
		{
			profile = playerHandler.profile;
			if (profile != null)
			{
				player = UserInput.GetRewiredPlayer(profile, playerHandler.playerIndex);
			}
		}
		Options.ControlSetup controlSetup = manager.rainWorld.options.controls[playerIndex];
		if (player == null)
		{
			player = controlSetup.player;
		}
		if (player == null)
		{
			player = controlSetup.player;
		}
		if (ReInput.controllers.Mouse.GetAnyButtonDown())
		{
			flag = true;
		}
		if (player != null && player.controllers.joystickCount >= ReInput.configuration.maxJoysticksPerPlayer)
		{
			flag = true;
		}
		if (player != null && player.GetAnyButton() && exitTimer < 0)
		{
			exitTimer = 5;
		}
		for (int i = 0; i < player.controllers.Joysticks.Count; i++)
		{
			if (controlSetup.gamePadGuid != null && player.controllers.Joysticks[i].deviceInstanceGuid.ToString() == controlSetup.gamePadGuid)
			{
				flag = true;
				break;
			}
		}
		if (!flag && exitTimer < 0)
		{
			foreach (Joystick joystick in ReInput.controllers.Joysticks)
			{
				if (joystick.GetButton((manager.rainWorld.options.enterButton != RainWorldPlugin.EnterButton.Cross) ? 1 : 0))
				{
					Profiles.Profile joystickProfile = UserInput.GetJoystickProfile(joystick);
					if (joystickProfile != null && profile != null && joystickProfile == profile)
					{
						UserInput.ReconnectJoystick(profile, joystick);
						manager.rainWorld.RequestPlayerSignIn(playerIndex, joystick);
						exitTimer = 5;
						break;
					}
					exitTimer = 5;
				}
			}
		}
		if (player == null || (profile == null && player == null) || (playerHandler == null && player == null))
		{
			flag = true;
		}
		if (exitTimer > 0)
		{
			exitTimer--;
			if (exitTimer == 0)
			{
				flag = true;
			}
		}
		if (flag)
		{
			manager.StopSideProcess(this);
		}
	}
}
