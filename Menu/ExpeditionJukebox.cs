using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Expedition;
using Menu.Remix;
using Menu.Remix.MixedUI;
using MoreSlugcats;
using Music;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ExpeditionJukebox : Menu, Slider.ISliderOwner, SelectOneButton.SelectOneButtonOwner
{
	public List<string> songList;

	public FSprite jukeboxLogo;

	public FSprite shade;

	public OpSimpleButton[] trackButtons;

	public OpHoldButton seekBar;

	public MenuLabel nowPlaying;

	public MenuLabel currentSong;

	public HorizontalSlider playbackSlider;

	public VerticalSlider volumeSlider;

	public SymbolButton prevButton;

	public SymbolButton playButton;

	public SymbolButton nextButton;

	public SymbolButton repeatButton;

	public SymbolButton shuffleButton;

	public SymbolButton favouriteButton;

	public SimpleButton backButton;

	public MusicTrackContainer trackContainer;

	public string selectedSong;

	public int selectedTrack;

	public bool isPlaying;

	public bool repeat;

	public bool shuffle;

	public bool switchingTrack;

	public float selectedTrackPos;

	public int pendingSong = -1;

	public float baseVolume;

	public bool demoMode;

	public float anchorLeft;

	public ExpeditionJukebox(ProcessManager manager)
		: base(manager, ExpeditionEnums.ProcessID.ExpeditionJukebox)
	{
		songList = ExpeditionProgression.GetUnlockedSongs().Values.ToList();
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.FadeOutAllSongs(1f);
		}
		else
		{
			manager.musicPlayer = new MusicPlayer(manager);
			manager.sideProcesses.Add(manager.musicPlayer);
			manager.musicPlayer.UpdateMusicContext(manager.currentMainLoop);
		}
		baseVolume = manager.rainWorld.options.musicVolume;
		float[] screenOffsets = Custom.GetScreenOffsets();
		float num = screenOffsets[0];
		_ = screenOffsets[1];
		anchorLeft = num;
		pages = new List<Page>();
		pages.Add(new Page(this, null, "Main", 0));
		MenuScene.SceneID randomJukeboxScene = GetRandomJukeboxScene();
		scene = new InteractiveMenuScene(this, pages[0], randomJukeboxScene);
		pages[0].subObjects.Add(scene);
		shade = new FSprite("Futile_White");
		shade.scaleX = 1000f;
		shade.scaleY = 1000f;
		shade.x = 0f;
		shade.y = 0f;
		shade.color = new Color(0f, 0f, 0f);
		shade.alpha = 0.7f;
		pages[0].Container.AddChild(shade);
		MenuTabWrapper menuTabWrapper = new MenuTabWrapper(this, pages[0]);
		pages[0].subObjects.Add(menuTabWrapper);
		_ = songList.Count;
		jukeboxLogo = new FSprite("jukebox");
		jukeboxLogo.x = 640f - num;
		jukeboxLogo.y = 618f;
		jukeboxLogo.SetAnchor(0f, 0f);
		jukeboxLogo.shader = manager.rainWorld.Shaders["MenuText"];
		pages[0].Container.AddChild(jukeboxLogo);
		float num2 = 80f;
		volumeSlider = new VerticalSlider(this, pages[0], Translate(""), new Vector2(980f + num2 * 2f, 75f), new Vector2(0f, 200f), Slider.SliderID.MusicVol, subtleSlider: false);
		pages[0].subObjects.Add(volumeSlider);
		prevButton = new SymbolButton(this, pages[0], "mediaprev", "PREV", new Vector2(530f + num2 * 0f, 70f));
		prevButton.size = new Vector2(60f, 60f);
		prevButton.roundedRect.size = prevButton.size;
		pages[0].subObjects.Add(prevButton);
		playButton = new SymbolButton(this, pages[0], "mediaplay", "PLAY", new Vector2(530f + num2 * 1f, 70f));
		playButton.size = new Vector2(60f, 60f);
		playButton.roundedRect.size = playButton.size;
		pages[0].subObjects.Add(playButton);
		nextButton = new SymbolButton(this, pages[0], "medianext", "NEXT", new Vector2(530f + num2 * 2f, 70f));
		nextButton.size = new Vector2(60f, 60f);
		nextButton.roundedRect.size = nextButton.size;
		pages[0].subObjects.Add(nextButton);
		repeatButton = new SymbolButton(this, pages[0], "mediarepeat", "REPEAT", new Vector2(980f - num2, 70f));
		repeatButton.size = new Vector2(60f, 60f);
		repeatButton.roundedRect.size = repeatButton.size;
		pages[0].subObjects.Add(repeatButton);
		shuffleButton = new SymbolButton(this, pages[0], "mediashuffle", "SHUFFLE", new Vector2(980f, 70f));
		shuffleButton.size = new Vector2(60f, 60f);
		shuffleButton.roundedRect.size = shuffleButton.size;
		pages[0].subObjects.Add(shuffleButton);
		favouriteButton = new SymbolButton(this, pages[0], "mediafav", "FAVOURITE", new Vector2(980f + num2, 70f));
		favouriteButton.size = new Vector2(60f, 60f);
		favouriteButton.roundedRect.size = favouriteButton.size;
		pages[0].subObjects.Add(favouriteButton);
		seekBar = new OpHoldButton(new Vector2(535f, 180f), new Vector2(570f, 45f), "", 200f);
		new UIelementWrapper(menuTabWrapper, seekBar);
		playbackSlider = new MusicProgressSlider(this, pages[0], "", new Vector2(530f, 150f), new Vector2(560f, 0f), ExpeditionEnums.SliderID.Playback, subtleSlider: false);
		pages[0].subObjects.Add(playbackSlider);
		nowPlaying = new MenuLabel(this, pages[0], "", new Vector2(530f, 250f), default(Vector2), bigText: true);
		nowPlaying.label.alignment = FLabelAlignment.Left;
		nowPlaying.label.shader = manager.rainWorld.Shaders["MenuText"];
		pages[0].subObjects.Add(nowPlaying);
		currentSong = new MenuLabel(this, pages[0], "", new Vector2(1120f, 250f), default(Vector2), bigText: true);
		currentSong.label.alignment = FLabelAlignment.Right;
		currentSong.label.shader = manager.rainWorld.Shaders["MenuText"];
		pages[0].subObjects.Add(currentSong);
		trackContainer = new MusicTrackContainer(this, pages[0], new Vector2(230f, 670f), songList);
		pages[0].subObjects.Add(trackContainer);
		float num3 = LabelTest.GetWidth(Translate("BACK")) + 45f;
		backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(350f - num3 / 2f, 20f), new Vector2(num3, 30f));
		pages[0].subObjects.Add(backButton);
		backObject = backButton;
		MusicVisualizer item = new MusicVisualizer(this, pages[0], new Vector2(535f, 280f));
		pages[0].subObjects.Add(item);
		playbackSlider.nextSelectable[0] = playbackSlider;
		playbackSlider.nextSelectable[2] = playbackSlider;
		playbackSlider.nextSelectable[3] = playButton;
		playbackSlider.nextSelectable[1] = playButton;
		prevButton.nextSelectable[1] = playbackSlider;
		prevButton.nextSelectable[2] = playButton;
		prevButton.nextSelectable[3] = backButton;
		playButton.nextSelectable[0] = prevButton;
		playButton.nextSelectable[1] = playbackSlider;
		playButton.nextSelectable[2] = nextButton;
		playButton.nextSelectable[3] = backButton;
		nextButton.nextSelectable[0] = playButton;
		nextButton.nextSelectable[1] = playbackSlider;
		nextButton.nextSelectable[2] = repeatButton;
		nextButton.nextSelectable[3] = backButton;
		repeatButton.nextSelectable[0] = nextButton;
		repeatButton.nextSelectable[1] = playbackSlider;
		repeatButton.nextSelectable[2] = shuffleButton;
		repeatButton.nextSelectable[3] = backButton;
		shuffleButton.nextSelectable[0] = repeatButton;
		shuffleButton.nextSelectable[1] = playbackSlider;
		shuffleButton.nextSelectable[2] = favouriteButton;
		shuffleButton.nextSelectable[3] = backButton;
		favouriteButton.nextSelectable[0] = shuffleButton;
		favouriteButton.nextSelectable[1] = playbackSlider;
		favouriteButton.nextSelectable[2] = volumeSlider;
		favouriteButton.nextSelectable[3] = backButton;
		volumeSlider.nextSelectable[0] = favouriteButton;
		volumeSlider.nextSelectable[2] = prevButton;
		trackContainer.backPage.nextSelectable[3] = backButton;
		trackContainer.forwardPage.nextSelectable[3] = backButton;
		backButton.nextSelectable[1] = trackContainer.forwardPage;
		backButton.nextSelectable[2] = trackContainer.forwardPage;
		backButton.nextSelectable[0] = trackContainer.backPage;
		seekBar.SetProgress(0f);
	}

	public override void Update()
	{
		base.Update();
		if (repeatButton.Selected || repeatButton.MouseOver)
		{
			infoLabel.text = Translate("Repeats the currently selected track");
			infoLabelFade = 1f;
		}
		if (shuffleButton.Selected || shuffleButton.MouseOver)
		{
			infoLabel.text = Translate("Plays unlocked tracks at random");
			infoLabelFade = 1f;
		}
		if (favouriteButton.Selected || favouriteButton.MouseOver)
		{
			infoLabel.text = Translate("Set a track to play as the menu theme");
			infoLabelFade = 1f;
		}
		if (pendingSong == 0)
		{
			manager.musicPlayer.MenuRequestsSong(songList[selectedTrack], 1f, 0f);
			ExpLog.Log("Jukebox Song: " + songList[selectedTrack]);
			pendingSong--;
			Dictionary<string, string> unlockedSongs = ExpeditionProgression.GetUnlockedSongs();
			if (ExpeditionData.newSongs.Contains(unlockedSongs.ElementAt(selectedTrack).Key))
			{
				ExpeditionData.newSongs.Remove(unlockedSongs.ElementAt(selectedTrack).Key);
			}
		}
		else if (pendingSong > 0)
		{
			pendingSong--;
		}
		if (manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song.subTracks[0] != null && manager.musicPlayer.song.subTracks[0].source.clip != null)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(manager.musicPlayer.song.subTracks[0].source.time);
			string value = $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:00}";
			TimeSpan timeSpan2 = TimeSpan.FromSeconds(manager.musicPlayer.song.subTracks[0].source.clip.length);
			string value2 = $"{(int)timeSpan2.TotalMinutes}:{timeSpan2.Seconds:00}";
			nowPlaying.label.text = Translate("Now Playing:");
			float num = Mathf.InverseLerp(0f, (float)timeSpan2.TotalMilliseconds, (float)timeSpan.TotalMilliseconds) * 100f;
			seekBar.SetProgress(num);
			seekBar.text = ValueConverter.ConvertToString(value) + " | " + ValueConverter.ConvertToString(value2);
			isPlaying = true;
			if (playbackSlider.mouseDragged)
			{
				manager.musicPlayer.song.volume = 0f;
			}
			else
			{
				manager.musicPlayer.song.volume = manager.musicPlayer.song.baseVolume;
			}
			if (playButton.symbolSprite.element.name != "mediastop")
			{
				playButton.symbolSprite.SetElementByName("mediastop");
			}
			if (num >= 99f)
			{
				if (repeat)
				{
					manager.musicPlayer.song.subTracks[0].source.time = 0f;
				}
				else if (shuffle)
				{
					manager.musicPlayer.FadeOutAllSongs(0f);
					NextTrack(shuffle: true);
					seekBar.SetProgress(0f);
					currentSong.label.text = ExpeditionProgression.TrackName(songList[selectedTrack]);
					pendingSong = 1;
					trackContainer.GoToPlayingTrackPage();
				}
			}
		}
		else
		{
			isPlaying = false;
			seekBar.SetProgress(0f);
			if (playButton.symbolSprite.element.name != "mediaplay")
			{
				playButton.symbolSprite.SetElementByName("mediaplay");
			}
		}
	}

	public MenuScene.SceneID GetRandomJukeboxScene()
	{
		List<MenuScene.SceneID> list = new List<MenuScene.SceneID>
		{
			MenuScene.SceneID.Landscape_SS,
			MenuScene.SceneID.Intro_13_Alone,
			MenuScene.SceneID.Intro_1_Tree,
			MenuScene.SceneID.Intro_4_Walking,
			MenuScene.SceneID.Intro_5_Hunting,
			MenuScene.SceneID.Landscape_SU,
			MenuScene.SceneID.Yellow_Intro_B,
			MenuScene.SceneID.Landscape_HI
		};
		if (ExpeditionGame.unlockedExpeditionSlugcats.Contains(SlugcatStats.Name.Red))
		{
			list.Add(MenuScene.SceneID.Landscape_LF);
			list.Add(MenuScene.SceneID.Landscape_SB);
			list.Add(MenuScene.SceneID.Landscape_SL);
			list.Add(MenuScene.SceneID.Landscape_UW);
			list.Add(MenuScene.SceneID.Landscape_GW);
			list.Add(MenuScene.SceneID.Landscape_SI);
			list.Add(MenuScene.SceneID.Landscape_CC);
			list.Add(MenuScene.SceneID.Landscape_SH);
			list.Add(MenuScene.SceneID.Landscape_DS);
			if (ModManager.MSC)
			{
				list.Add(MoreSlugcatsEnums.MenuSceneID.Landscape_VS);
			}
		}
		if (ModManager.MSC && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
		{
			list.Add(MoreSlugcatsEnums.MenuSceneID.Landscape_OE);
		}
		if (ModManager.MSC && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
		{
			list.Add(MoreSlugcatsEnums.MenuSceneID.Landscape_LC);
		}
		if (ModManager.MSC && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Spear))
		{
			list.Add(MoreSlugcatsEnums.MenuSceneID.Landscape_DM);
		}
		if (ModManager.MSC && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
		{
			list.Add(MoreSlugcatsEnums.MenuSceneID.Landscape_MS);
			list.Add(MoreSlugcatsEnums.MenuSceneID.Landscape_RM);
		}
		if (ModManager.MSC && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Saint))
		{
			list.Add(MoreSlugcatsEnums.MenuSceneID.Landscape_CL);
			list.Add(MoreSlugcatsEnums.MenuSceneID.Landscape_UG);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public override void SliderSetValue(Slider slider, float f)
	{
		if (slider.ID == ExpeditionEnums.SliderID.Playback && manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song.subTracks[0] != null && manager.musicPlayer.song.subTracks[0].source.clip != null)
		{
			manager.musicPlayer.song.subTracks[0].source.time = Mathf.Lerp(0f, manager.musicPlayer.song.subTracks[0].source.clip.length - 1f, f);
		}
		else if (slider.ID == Slider.SliderID.MusicVol)
		{
			manager.rainWorld.options.musicVolume = f;
		}
	}

	public override float ValueOfSlider(Slider slider)
	{
		if (slider.ID == ExpeditionEnums.SliderID.Playback)
		{
			if (manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song.subTracks[0] != null && manager.musicPlayer.song.subTracks[0].source.clip != null)
			{
				return Mathf.InverseLerp(0f, manager.musicPlayer.song.subTracks[0].source.clip.length, manager.musicPlayer.song.subTracks[0].source.time);
			}
			return 0f;
		}
		if (slider.ID == Slider.SliderID.MusicVol)
		{
			return manager.rainWorld.options.musicVolume;
		}
		return 0f;
	}

	public int GetCurrentlySelectedOfSeries(string series)
	{
		if (series.StartsWith("mus-"))
		{
			return selectedTrack;
		}
		return 0;
	}

	public override string UpdateInfoText()
	{
		if (selectedObject is Slider && (selectedObject as Slider).ID == Slider.SliderID.MusicVol)
		{
			return Translate("Music volume:") + " " + Custom.IntClamp((int)(manager.rainWorld.options.musicVolume * 100f), 0, 100).ToString(CultureInfo.InvariantCulture) + "%";
		}
		return "";
	}

	public void SetCurrentlySelectedOfSeries(string series, int to)
	{
		if (series.StartsWith("mus-"))
		{
			selectedTrack = to;
			seekBar.SetProgress(0f);
			currentSong.label.text = ExpeditionProgression.TrackName(songList[selectedTrack]);
			manager.musicPlayer.FadeOutAllSongs(0f);
			pendingSong = 1;
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "BACK")
		{
			global::Expedition.Expedition.coreFile.Save(runEnded: false);
			PlaySound(SoundID.MENU_Switch_Page_Out);
			manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionMenu);
		}
		if (message.StartsWith("mus-"))
		{
			pendingSong = 1;
		}
		if (message == "PREV")
		{
			manager.musicPlayer.FadeOutAllSongs(0f);
			PreviousTrack(shuffle);
			seekBar.SetProgress(0f);
			currentSong.label.text = ExpeditionProgression.TrackName(songList[selectedTrack]);
			pendingSong = 1;
			trackContainer.GoToPlayingTrackPage();
			PlaySound(SoundID.MENU_Button_Press_Init);
		}
		if (message == "NEXT")
		{
			manager.musicPlayer.FadeOutAllSongs(0f);
			NextTrack(shuffle);
			seekBar.SetProgress(0f);
			currentSong.label.text = ExpeditionProgression.TrackName(songList[selectedTrack]);
			pendingSong = 1;
			trackContainer.GoToPlayingTrackPage();
			PlaySound(SoundID.MENU_Button_Press_Init);
		}
		if (message == "PLAY")
		{
			if (isPlaying)
			{
				manager.musicPlayer.FadeOutAllSongs(0f);
				seekBar.SetProgress(0f);
			}
			else
			{
				pendingSong = 1;
			}
			seekBar.SetProgress(0f);
			currentSong.label.text = ExpeditionProgression.TrackName(songList[selectedTrack]);
		}
		if (message == "REPEAT")
		{
			if (!repeat)
			{
				repeat = true;
				PlaySound(SoundID.MENU_Player_Join_Game);
			}
			else
			{
				repeat = false;
				PlaySound(SoundID.MENU_Button_Press_Init);
			}
		}
		if (message == "SHUFFLE")
		{
			if (!shuffle)
			{
				shuffle = true;
				PlaySound(SoundID.MENU_Player_Join_Game);
			}
			else
			{
				shuffle = false;
				PlaySound(SoundID.MENU_Button_Press_Init);
			}
		}
		if (message == "FAVOURITE")
		{
			if (ExpeditionData.menuSong != songList[selectedTrack])
			{
				ExpeditionData.menuSong = songList[selectedTrack];
				PlaySound(SoundID.MENU_Player_Join_Game);
			}
			else
			{
				ExpeditionData.menuSong = "";
				PlaySound(SoundID.MENU_Button_Press_Init);
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		shuffleButton.symbolSprite.color = (shuffle ? Menu.MenuRGB(MenuColors.MediumGrey) : Menu.MenuRGB(MenuColors.VeryDarkGrey));
		repeatButton.symbolSprite.color = (repeat ? Menu.MenuRGB(MenuColors.MediumGrey) : Menu.MenuRGB(MenuColors.VeryDarkGrey));
		if (ExpeditionData.menuSong == songList[selectedTrack])
		{
			favouriteButton.symbolSprite.color = new Color(1f, 0.7f, 0f);
			favouriteButton.symbolSprite.shader = manager.rainWorld.Shaders["MenuTextCustom"];
		}
		else
		{
			favouriteButton.symbolSprite.color = Menu.MenuRGB(MenuColors.VeryDarkGrey);
			favouriteButton.symbolSprite.shader = manager.rainWorld.Shaders["Basic"];
		}
	}

	public void NextTrack(bool shuffle)
	{
		int num = selectedTrack;
		if (shuffle)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < trackContainer.trackList.Length; i++)
			{
				if (trackContainer.trackList[i].unlocked && i != num)
				{
					list.Add(i);
				}
			}
			selectedTrack = ((list.Count > 0) ? list[UnityEngine.Random.Range(0, list.Count)] : num);
			return;
		}
		for (int j = num + 1; j < trackContainer.trackList.Length; j++)
		{
			if (trackContainer.trackList[j].unlocked)
			{
				selectedTrack = j;
				return;
			}
		}
		for (int k = 0; k < num; k++)
		{
			if (trackContainer.trackList[k].unlocked)
			{
				selectedTrack = k;
				return;
			}
		}
		selectedTrack = num;
	}

	public void PreviousTrack(bool shuffle)
	{
		int num = selectedTrack;
		if (shuffle)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < trackContainer.trackList.Length; i++)
			{
				if (trackContainer.trackList[i].unlocked && i != num)
				{
					list.Add(i);
				}
			}
			selectedTrack = ((list.Count > 0) ? list[UnityEngine.Random.Range(0, list.Count)] : num);
			return;
		}
		for (int num2 = num - 1; num2 > 0; num2--)
		{
			if (trackContainer.trackList[num2].unlocked)
			{
				selectedTrack = num2;
				return;
			}
		}
		for (int num3 = trackContainer.trackList.Length - 1; num3 > num; num3--)
		{
			if (trackContainer.trackList[num3].unlocked)
			{
				selectedTrack = num3;
				return;
			}
		}
		selectedTrack = num;
	}
}
