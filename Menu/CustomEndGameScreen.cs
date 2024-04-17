using System;
using Modding.Passages;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class CustomEndGameScreen : Menu
{
	public MenuLabel titleLabel;

	public SimpleButton exitButton;

	public int counter;

	public FSprite blackSprite;

	public FSprite glyphGlowSprite;

	public FSprite localBloomSprite;

	private MenuIllustration glyphIllustration;

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

	public CustomEndGameScreen(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.CustomEndGameScreen)
	{
		pages.Add(new Page(this, null, "main", 0));
		blackSprite = new FSprite("pixel");
		blackSprite.color = new Color(0f, 0f, 0f);
		blackSprite.scaleX = 1400f;
		blackSprite.scaleY = 800f;
		blackSprite.x = manager.rainWorld.screenSize.x / 2f;
		blackSprite.y = manager.rainWorld.screenSize.y / 2f;
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.MenuRequestsSong("Passages", 1.2f, 10f);
		}
	}

	public override void Update()
	{
		base.Update();
		counter++;
		if (counter <= 100)
		{
			if (scene.flatIllustrations.Count > 0 && (!ModManager.MMF || scene.flatMode))
			{
				scene.flatIllustrations[0].alpha = Mathf.InverseLerp(0f, 100f, counter);
			}
			blackSprite.alpha = 1f;
		}
		else if (counter <= 250)
		{
			blackSprite.alpha = Custom.SCurve(Mathf.InverseLerp(250f, 100f, counter), 0.6f);
		}
		if (counter > 100 && scene.flatIllustrations.Count > 0 && (!ModManager.MMF || scene.flatMode))
		{
			scene.flatIllustrations[0].alpha = 1f;
		}
		if (exitButton != null)
		{
			exitButton.buttonBehav.greyedOut = FreezeMenuFunctions && counter > 200;
			exitButton.black = Math.Max(0f, exitButton.black - 0.0125f);
		}
		else if (counter >= 350)
		{
			exitButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(manager.rainWorld.options.ScreenSize.x - 320f + (1366f - manager.rainWorld.options.ScreenSize.x) - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
			pages[0].subObjects.Add(exitButton);
			pages[0].lastSelectedObject = exitButton;
			exitButton.black = 1f;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		titleLabel.label.alpha = Mathf.InverseLerp(180f, 400f, (float)(counter - 1) + timeStacker);
		if (glyphIllustration != null)
		{
			if (ModManager.MMF && scene.flatMode)
			{
				glyphIllustration.setAlpha = 0f;
			}
			float num = Mathf.Sin(((float)counter + timeStacker) / 60f * (float)Math.PI);
			float num2 = Mathf.Clamp(Mathf.Sin(Mathf.InverseLerp(3f, 200f, (float)(counter - 1) + timeStacker) * (float)Math.PI), 0f, 1f);
			glyphGlowSprite.scale = Mathf.Lerp(90f, 105f, num2) / 8f;
			glyphGlowSprite.alpha = Mathf.Lerp(0.19f + 0.05f * num, 0.24f, Mathf.Pow(num2, 2f));
			glyphGlowSprite.x = glyphIllustration.DrawX(1f) + glyphIllustration.size.x / 2f;
			glyphGlowSprite.y = glyphIllustration.DrawY(1f) + glyphIllustration.size.y / 2f;
			localBloomSprite.scale = 50f;
			localBloomSprite.x = glyphIllustration.DrawX(1f) + glyphIllustration.size.x / 2f;
			localBloomSprite.y = glyphIllustration.DrawY(1f) + glyphIllustration.size.y / 2f;
			localBloomSprite.alpha = Mathf.Lerp(0.4f + 0.15f * num, 1f, Mathf.Pow(num2, 1.7f));
		}
	}

	public void StartGame()
	{
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "CONTINUE")
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.FastTravelScreen);
		}
	}

	public void GetDataFromSleepScreen(WinState.EndgameID endGameID)
	{
		MenuScene.SceneID sceneID = MenuScene.SceneID.Empty;
		if (endGameID == WinState.EndgameID.Survivor)
		{
			sceneID = MenuScene.SceneID.Endgame_Survivor;
		}
		else if (endGameID == WinState.EndgameID.Hunter)
		{
			sceneID = MenuScene.SceneID.Endgame_Hunter;
		}
		else if (endGameID == WinState.EndgameID.Saint)
		{
			sceneID = MenuScene.SceneID.Endgame_Saint;
		}
		else if (endGameID == WinState.EndgameID.Traveller)
		{
			sceneID = MenuScene.SceneID.Endgame_Traveller;
		}
		else if (endGameID == WinState.EndgameID.Chieftain)
		{
			sceneID = MenuScene.SceneID.Endgame_Chieftain;
		}
		else if (endGameID == WinState.EndgameID.Monk)
		{
			sceneID = MenuScene.SceneID.Endgame_Monk;
		}
		else if (endGameID == WinState.EndgameID.Outlaw)
		{
			sceneID = MenuScene.SceneID.Endgame_Outlaw;
		}
		else if (endGameID == WinState.EndgameID.DragonSlayer)
		{
			sceneID = MenuScene.SceneID.Endgame_DragonSlayer;
		}
		else if (endGameID == WinState.EndgameID.Scholar)
		{
			sceneID = MenuScene.SceneID.Endgame_Scholar;
		}
		else if (endGameID == WinState.EndgameID.Friend)
		{
			sceneID = MenuScene.SceneID.Endgame_Friend;
		}
		if (ModManager.MSC)
		{
			if (endGameID == MoreSlugcatsEnums.EndgameID.Martyr)
			{
				sceneID = MenuScene.SceneID.Endgame_Martyr;
			}
			else if (endGameID == MoreSlugcatsEnums.EndgameID.Nomad)
			{
				sceneID = MoreSlugcatsEnums.MenuSceneID.Endgame_Nomad;
			}
			else if (endGameID == MoreSlugcatsEnums.EndgameID.Pilgrim)
			{
				sceneID = MoreSlugcatsEnums.MenuSceneID.Endgame_Pilgrim;
			}
			else if (endGameID == MoreSlugcatsEnums.EndgameID.Mother)
			{
				sceneID = MenuScene.SceneID.Endgame_Mother;
			}
		}
		CustomPassage customPassage = CustomPassages.PassageForID(endGameID);
		if (customPassage != null)
		{
			sceneID = customPassage.Scene;
		}
		scene = new InteractiveMenuScene(this, pages[0], sceneID);
		pages[0].subObjects.Add(scene);
		pages[0].Container.AddChild(blackSprite);
		if (scene.flatIllustrations.Count > 0)
		{
			scene.flatIllustrations[0].RemoveSprites();
			scene.flatIllustrations[0].Container.AddChild(scene.flatIllustrations[0].sprite);
			glyphIllustration = scene.flatIllustrations[0];
			glyphGlowSprite = new FSprite("Futile_White");
			glyphGlowSprite.shader = manager.rainWorld.Shaders["FlatLight"];
			pages[0].Container.AddChild(glyphGlowSprite);
			localBloomSprite = new FSprite("Futile_White");
			localBloomSprite.shader = manager.rainWorld.Shaders["LocalBloom"];
			pages[0].Container.AddChild(localBloomSprite);
		}
		titleLabel = new MenuLabel(this, pages[0], "", new Vector2(583f, 5f), new Vector2(200f, 30f), bigText: false);
		pages[0].subObjects.Add(titleLabel);
		titleLabel.text = Translate(WinState.PassageDisplayName(endGameID));
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		if (glyphGlowSprite != null)
		{
			glyphGlowSprite.RemoveFromContainer();
		}
		if (localBloomSprite != null)
		{
			localBloomSprite.RemoveFromContainer();
		}
	}
}
