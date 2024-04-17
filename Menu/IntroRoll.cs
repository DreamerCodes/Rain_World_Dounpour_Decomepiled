using System.IO;
using Kittehface.Framework20;
using MoreSlugcats;
using Music;
using RWCustom;
using UnityEngine;

namespace Menu;

public class IntroRoll : Menu
{
	public MenuIllustration[] illustrations;

	public float time;

	public RainEffect rainEffect;

	public MenuLabel anyButtonLabel;

	private bool continueToMenu;

	private bool lastAnyButton;

	public bool forceWatchRoll;

	public bool languageFontDirty;

	public InGameTranslator.LanguageID dirtyLanguage;

	private float delayedTime;

	private bool requestIntroMusic;

	public override bool ShowCursor => false;

	public IntroRoll(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.IntroRoll)
	{
		pages.Add(new Page(this, null, "main", 0));
		manager.menuesMouseMode = false;
		rainEffect = new RainEffect(this, pages[0]);
		pages[0].subObjects.Add(rainEffect);
		illustrations = new MenuIllustration[3];
		if (ModManager.MSC && manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			illustrations[0] = new MenuIllustration(this, pages[0], "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "Intro_Roll_A_inv", new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			illustrations[1] = new MenuIllustration(this, pages[0], "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "Intro_Roll_A_inv", new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
		}
		else
		{
			illustrations[0] = new MenuIllustration(this, pages[0], "", "Intro_Roll_A", new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
			illustrations[1] = new MenuIllustration(this, pages[0], "", "Intro_Roll_B", new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
		}
		if (ModManager.MSC && manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			illustrations[2] = new MenuIllustration(this, pages[0], "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "Intro_Roll_C", new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
		}
		else if (manager.rainWorld.dlcVersion == 0)
		{
			illustrations[2] = new MenuIllustration(this, pages[0], "", "Intro_Roll_C", new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
		}
		else
		{
			string[] array = new string[5] { "gourmand", "rivulet", "spear", "artificer", "saint" };
			illustrations[2] = new MenuIllustration(this, pages[0], "", "Intro_Roll_C_" + array[Random.Range(0, array.Length)], new Vector2(0f, 0f), crispPixels: true, anchorCenter: false);
		}
		for (int i = 0; i < illustrations.Length; i++)
		{
			pages[0].subObjects.Add(illustrations[i]);
			illustrations[i].sprite.isVisible = false;
		}
		bool flag = true;
		bool flag2 = true;
		if (Platform.initialized && flag)
		{
			if (flag2)
			{
				anyButtonLabel = new MenuLabel(this, pages[0], Translate("Press any button to continue"), new Vector2(manager.rainWorld.screenSize.x / 2f - 50f, 40f), new Vector2(100f, 30f), bigText: false);
			}
			else
			{
				InGameTranslator.LanguageID language = manager.rainWorld.options.language;
				manager.rainWorld.options.language = InGameTranslator.systemLanguage;
				if (language != manager.rainWorld.options.language)
				{
					languageFontDirty = true;
					dirtyLanguage = InGameTranslator.systemLanguage;
					InGameTranslator.UnloadFonts(language);
					InGameTranslator.LoadFonts(manager.rainWorld.options.language, this);
				}
				anyButtonLabel = new MenuLabel(this, pages[0], Translate("Please log in to STOVE Client with the account that has purchased the game"), new Vector2(manager.rainWorld.screenSize.x / 2f - 50f, 40f), new Vector2(100f, 30f), bigText: false);
				manager.rainWorld.options.language = language;
			}
			pages[0].subObjects.Add(anyButtonLabel);
			anyButtonLabel.pos.x += Menu.HorizontalMoveToGetCentered(manager);
			anyButtonLabel.label.alpha = 0f;
		}
		if (manager.musicPlayer != null && manager.rainWorld.OptionsReady)
		{
			manager.musicPlayer.RequestIntroRollMusic();
		}
		else if (!manager.rainWorld.OptionsReady)
		{
			requestIntroMusic = true;
		}
		if (!ModManager.DevTools)
		{
			Cursor.visible = !manager.rainWorld.options.fullScreen;
		}
	}

	public override void Update()
	{
		base.Update();
		bool flag = false;
		for (int i = 0; i < manager.rainWorld.options.controls.Length; i++)
		{
			flag = flag || manager.rainWorld.options.controls[i].GetAnyButton() || (manager.rainWorld.options.controls[i].player.controllers.hasMouse && manager.rainWorld.options.controls[i].player.controllers.Mouse.GetAnyButton());
		}
		if (time > (forceWatchRoll ? 14.5f : 1f) && flag && !lastAnyButton)
		{
			GoToMenu();
		}
		lastAnyButton = flag;
		if (requestIntroMusic && manager.rainWorld.OptionsReady)
		{
			requestIntroMusic = false;
			if (manager.musicPlayer != null)
			{
				manager.musicPlayer.RequestIntroRollMusic();
			}
		}
		if (delayedTime > 14f && Random.value < 1f / 160f)
		{
			rainEffect.LightningSpike(Mathf.Pow(Random.value, 2f) * 0.85f, Mathf.Lerp(20f, 120f, Random.value));
		}
	}

	public override void RawUpdate(float dt)
	{
		base.RawUpdate(dt);
		float lastTime = time;
		time += dt;
		float lastTime2 = delayedTime;
		if (delayedTime < 4f || ((manager.musicPlayer == null || manager.musicPlayer.assetBundlesLoaded) && manager.soundLoader.assetBundlesLoaded && manager.rainWorld.platformInitialized && manager.rainWorld.OptionsReady && manager.rainWorld.progression.progressionLoaded))
		{
			delayedTime += dt;
		}
		rainEffect.rainFade = Custom.SCurve(Mathf.InverseLerp(4f, 10f, time), 0.8f);
		illustrations[0].alpha = Custom.SCurve(Mathf.Min(Mathf.InverseLerp(1f, 2f, time), Mathf.InverseLerp(4f, 3f, time)), 0.65f);
		illustrations[1].alpha = Custom.SCurve(Mathf.Min(Mathf.InverseLerp(4f, 5f, time), Mathf.InverseLerp(7f, 6f, time)), 0.65f);
		illustrations[2].alpha = Custom.SCurve(Mathf.InverseLerp(10f, 11f, delayedTime), 0.65f);
		for (int i = 0; i < illustrations.Length; i++)
		{
			illustrations[i].sprite.isVisible = illustrations[i].alpha > 0f;
		}
		if (time > 7f)
		{
			ProcessManager.initializationAndIntroRollStarted = true;
		}
		bool flag = true;
		bool flag2 = true;
		if (anyButtonLabel != null)
		{
			anyButtonLabel.label.alpha = Custom.SCurve(Mathf.InverseLerp(14f, 15f, delayedTime), 0.65f);
		}
		else if (Platform.initialized && flag)
		{
			if (flag2)
			{
				anyButtonLabel = new MenuLabel(this, pages[0], Translate("Press any button to continue"), new Vector2(manager.rainWorld.screenSize.x / 2f - 50f, 40f), new Vector2(100f, 30f), bigText: false);
			}
			else
			{
				InGameTranslator.LanguageID language = manager.rainWorld.options.language;
				manager.rainWorld.options.language = InGameTranslator.systemLanguage;
				if (language != manager.rainWorld.options.language)
				{
					languageFontDirty = true;
					dirtyLanguage = InGameTranslator.systemLanguage;
					InGameTranslator.UnloadFonts(language);
					InGameTranslator.LoadFonts(manager.rainWorld.options.language, this);
				}
				anyButtonLabel = new MenuLabel(this, pages[0], Translate("Please log in to STOVE Client with the account that has purchased the game"), new Vector2(manager.rainWorld.screenSize.x / 2f - 50f, 40f), new Vector2(100f, 30f), bigText: false);
				manager.rainWorld.options.language = language;
			}
			pages[0].subObjects.Add(anyButtonLabel);
			anyButtonLabel.pos.x += Menu.HorizontalMoveToGetCentered(manager);
			anyButtonLabel.label.alpha = 0f;
		}
		if (CheckTimeStamp(lastTime, time, 1.5f))
		{
			rainEffect.LightningSpike(0.4f, 30f);
		}
		if (CheckTimeStamp(lastTime, time, 3f))
		{
			rainEffect.LightningSpike(0.6f, 40f);
		}
		if (CheckTimeStamp(lastTime, time, 5f))
		{
			rainEffect.LightningSpike(0.3f, 1600f);
		}
		if (CheckTimeStamp(lastTime2, delayedTime, 10.3f))
		{
			manager.menuMic.PlaySound(SoundID.Thunder_Close, 0f, 0.3f, 1f);
			manager.menuMic.PlaySound(SoundID.Thunder, 0f, 0.4f, 1f);
		}
		if (CheckTimeStamp(lastTime2, delayedTime, 10.5f))
		{
			rainEffect.LightningSpike(1f, 40f);
		}
		if (CheckTimeStamp(lastTime2, delayedTime, 8.5f) && manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song is IntroRollMusic)
		{
			(manager.musicPlayer.song as IntroRollMusic).StartMusic();
		}
		if (!continueToMenu && delayedTime > 40f)
		{
			GoToMenu();
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
	}

	public void GoToMenu()
	{
		bool flag = true;
		bool flag2 = true;
		if (!continueToMenu && (manager.musicPlayer == null || manager.musicPlayer.assetBundlesLoaded) && manager.soundLoader.assetBundlesLoaded && manager.rainWorld.platformInitialized && manager.rainWorld.OptionsReady && manager.rainWorld.progression.progressionLoaded && manager.dialog == null && flag && flag2)
		{
			continueToMenu = true;
			if (manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song is IntroRollMusic)
			{
				(manager.musicPlayer.song as IntroRollMusic).StartMusic();
				(manager.musicPlayer.song as IntroRollMusic).fadeOutRain = true;
			}
			if (languageFontDirty)
			{
				InGameTranslator.UnloadFonts(dirtyLanguage);
				InGameTranslator.LoadFonts(manager.rainWorld.options.language, this);
			}
			ProcessManager.initializationAndIntroRollStarted = true;
			if (ProcessManager.activityID == null)
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
			}
		}
	}

	private bool CheckTimeStamp(float lastTime, float currentTime, float checkTime)
	{
		if (currentTime >= checkTime)
		{
			return lastTime < checkTime;
		}
		return false;
	}
}
