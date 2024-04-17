using System;
using Menu;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class SpecialUnlockScreen : global::Menu.Menu
{
	public SimpleButton exitButton;

	public MenuLabel messageLabel;

	public MenuLabel noticeLabel;

	public int counter;

	public override bool ForceNoMouseMode
	{
		get
		{
			if (exitButton != null && !(exitButton.black > 0.5f))
			{
				return base.ForceNoMouseMode;
			}
			return true;
		}
	}

	protected override bool FreezeMenuFunctions
	{
		get
		{
			if (!base.FreezeMenuFunctions)
			{
				return counter < 20;
			}
			return true;
		}
	}

	public SpecialUnlockScreen(ProcessManager manager)
		: base(manager, MoreSlugcatsEnums.ProcessID.SpecialUnlock)
	{
		pages.Add(new Page(this, null, "main", 0));
		mySoundLoopID = SoundID.MENU_Dream_LOOP;
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.FadeOutAllSongs(30f);
		}
		messageLabel = new MenuLabel(this, pages[0], base.manager.specialUnlockText, new Vector2(base.manager.rainWorld.options.ScreenSize.x * 0.5f - 50f + (1366f - base.manager.rainWorld.options.ScreenSize.x) / 2f, base.manager.rainWorld.options.ScreenSize.y * 0.5f - 25f), new Vector2(100f, 30f), bigText: true);
		messageLabel.label.color = new Color(1f, 1f, 1f);
		messageLabel.label.alignment = FLabelAlignment.Center;
		noticeLabel = new MenuLabel(this, pages[0], Translate("[ Notice ]"), new Vector2(base.manager.rainWorld.options.ScreenSize.x * 0.5f - 50f + (1366f - base.manager.rainWorld.options.ScreenSize.x) / 2f, base.manager.rainWorld.options.ScreenSize.y * 0.5f + 50f), new Vector2(100f, 30f), bigText: true);
		noticeLabel.label.color = new Color(1f, 1f, 1f);
		noticeLabel.label.alignment = FLabelAlignment.Center;
		pages[0].subObjects.Add(messageLabel);
		pages[0].subObjects.Add(noticeLabel);
	}

	public override void Update()
	{
		base.Update();
		counter++;
		if (counter == 10)
		{
			PlaySound(SoundID.MENU_Dream_Init);
		}
		if ((float)counter > 80f && exitButton == null)
		{
			exitButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(manager.rainWorld.options.ScreenSize.x * 0.9f - 110f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 15f), new Vector2(110f, 30f));
			pages[0].subObjects.Add(exitButton);
			pages[0].lastSelectedObject = exitButton;
			exitButton.black = 1f;
		}
		manager.fadeToBlack = Custom.LerpAndTick(manager.fadeToBlack, 0f, 0f, 0.0125f);
		if (exitButton != null)
		{
			exitButton.buttonBehav.greyedOut = FreezeMenuFunctions;
			exitButton.black = Math.Max(0f, exitButton.black - 0.0125f);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "CONTINUE")
		{
			manager.specialUnlockText = "";
			manager.RequestMainProcessSwitch(manager.specialUnlockDestination);
			PlaySound(SoundID.MENU_Dream_Button);
		}
	}
}
