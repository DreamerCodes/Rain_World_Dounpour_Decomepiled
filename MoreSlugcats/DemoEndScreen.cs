using System;
using Menu;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class DemoEndScreen : global::Menu.Menu
{
	public SimpleButton exitButton;

	public MenuLabel[] labels;

	public int counter;

	public MenuIllustration slugcat;

	public override bool ForceNoMouseMode
	{
		get
		{
			if (exitButton == null || exitButton.black > 0.5f)
			{
				return true;
			}
			return base.ForceNoMouseMode;
		}
	}

	protected override bool FreezeMenuFunctions
	{
		get
		{
			if (base.FreezeMenuFunctions)
			{
				return true;
			}
			return counter < 20;
		}
	}

	public DemoEndScreen(ProcessManager manager)
		: base(manager, MoreSlugcatsEnums.ProcessID.DemoEnd)
	{
		pages.Add(new Page(this, null, "main", 0));
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.FadeOutAllSongs(30f);
		}
		labels = new MenuLabel[2]
		{
			new MenuLabel(this, pages[0], Translate("[ End of Demo ]"), new Vector2(base.manager.rainWorld.options.ScreenSize.x * 0.1f + (1366f - base.manager.rainWorld.options.ScreenSize.x) - 100f, base.manager.rainWorld.options.ScreenSize.y * 0.8f + 50f), new Vector2(100f, 30f), bigText: true),
			null
		};
		labels[0].label.color = new Color(0f, 0f, 0f);
		labels[0].label.alignment = FLabelAlignment.Left;
		labels[1] = new MenuLabel(this, pages[0], Translate("To be Continued..."), new Vector2(base.manager.rainWorld.options.ScreenSize.x * 0.1f + (1366f - base.manager.rainWorld.options.ScreenSize.x) - 100f, base.manager.rainWorld.options.ScreenSize.y * 0.8f), new Vector2(100f, 30f), bigText: true);
		labels[1].label.color = new Color(0f, 0f, 0f);
		labels[1].label.alignment = FLabelAlignment.Left;
		pages[0].subObjects.Add(labels[0]);
		pages[0].subObjects.Add(labels[1]);
		slugcat = new MenuIllustration(this, scene, string.Empty, "DemoEnd", new Vector2(base.manager.rainWorld.options.ScreenSize.x / 2f + (1366f - base.manager.rainWorld.options.ScreenSize.x) - 50f, base.manager.rainWorld.options.ScreenSize.y * 0.5f), crispPixels: true, anchorCenter: true)
		{
			alpha = 0f
		};
		pages[0].subObjects.Add(slugcat);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "EXIT")
		{
			if (manager.musicPlayer != null)
			{
				manager.musicPlayer.FadeOutAllSongs(30f);
			}
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
			PlaySound(SoundID.MENU_Dream_Button);
		}
	}

	public override void Update()
	{
		base.Update();
		counter++;
		if ((float)counter > 100f)
		{
			float num = Mathf.Max(0f, Mathf.Min(1f, ((float)counter - 100f) / 100f));
			float num2 = Mathf.Max(0f, Mathf.Min(1f, ((float)counter - 250f) / 100f));
			labels[0].label.color = new Color(num, num, num);
			labels[1].label.color = new Color(num2, num2, num2);
			if ((float)counter > 400f && exitButton == null)
			{
				exitButton = new SimpleButton(this, pages[0], Translate("EXIT"), "EXIT", new Vector2(manager.rainWorld.options.ScreenSize.x * 0.9f - 110f + (1366f - manager.rainWorld.options.ScreenSize.x), 15f), new Vector2(110f, 30f));
				pages[0].subObjects.Add(exitButton);
				pages[0].lastSelectedObject = exitButton;
				exitButton.black = 1f;
			}
		}
		manager.fadeToBlack = Custom.LerpAndTick(manager.fadeToBlack, 0f, 0f, 0.0125f);
		if (exitButton == null)
		{
			if (manager.musicPlayer != null && manager.musicPlayer.song == null)
			{
				manager.musicPlayer.MenuRequestsSong("NA_26 - Energy Circuit", 1f, 0.7f);
			}
		}
		else
		{
			exitButton.buttonBehav.greyedOut = FreezeMenuFunctions;
			exitButton.black = Math.Max(0f, exitButton.black - 0.0125f);
			slugcat.alpha = 1f - exitButton.black;
		}
	}
}
