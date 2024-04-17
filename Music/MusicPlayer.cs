using System;
using AssetBundles;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using VoidSea;

namespace Music;

public class MusicPlayer : MainLoopProcess
{
	public class MusicContext : ExtEnum<MusicContext>
	{
		public static readonly MusicContext Menu = new MusicContext("Menu", register: true);

		public static readonly MusicContext StoryMode = new MusicContext("StoryMode", register: true);

		public static readonly MusicContext Arena = new MusicContext("Arena", register: true);

		public MusicContext(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public const string ASSETBUNDLE_MUSIC_PROCEDURAL = "music_procedural";

	public const string ASSETBUNDLE_MUSIC_SONGS = "music_songs";

	public GameObject gameObj;

	public Song song;

	public Song nextSong;

	public float mainSongMix;

	public float droneGoalMix;

	public MusicContext musicContext = MusicContext.Menu;

	public PlayerThreatTracker threatTracker;

	public MultiplayerDJ multiplayerDJ;

	public ProceduralMusic proceduralMusic;

	private string nextProcedural;

	public bool hasPlayedASongThisCycle;

	private bool requestedAssetBundlesLoad;

	public bool assetBundlesLoaded { get; private set; }

	public MusicPlayer(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.MusicPlayer)
	{
		gameObj = new GameObject("Music Player");
		mainSongMix = 0f;
		droneGoalMix = 0f;
		UpdateMusicContext(manager.currentMainLoop);
	}

	public void UpdateMusicContext(MainLoopProcess currentProcess)
	{
		bool flag = false;
		if (currentProcess == null)
		{
			musicContext = MusicContext.Menu;
		}
		else if (currentProcess.ID == ProcessManager.ProcessID.Game)
		{
			if ((currentProcess as RainWorldGame).IsStorySession)
			{
				musicContext = MusicContext.StoryMode;
			}
			else if ((currentProcess as RainWorldGame).IsArenaSession)
			{
				musicContext = MusicContext.Arena;
			}
			if (song != null && song.context == MusicContext.Menu)
			{
				song.FadeOut(2f);
			}
		}
		else if (currentProcess.ID == ProcessManager.ProcessID.SlugcatSelect || currentProcess.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			musicContext = MusicContext.StoryMode;
		}
		else if (currentProcess.ID == ProcessManager.ProcessID.MultiplayerMenu)
		{
			musicContext = MusicContext.Arena;
		}
		else if (currentProcess.ID == ProcessManager.ProcessID.MainMenu)
		{
			musicContext = MusicContext.Menu;
		}
		if (musicContext == MusicContext.StoryMode || (ModManager.MSC && musicContext == MusicContext.Arena && currentProcess.ID == ProcessManager.ProcessID.Game && (currentProcess as RainWorldGame).GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && (currentProcess as RainWorldGame).GetArenaGameSession.chMeta != null && !string.IsNullOrEmpty((currentProcess as RainWorldGame).GetArenaGameSession.chMeta.threatMusic)))
		{
			if (threatTracker == null)
			{
				threatTracker = new PlayerThreatTracker(this, 0);
			}
			if (musicContext == MusicContext.Arena)
			{
				flag = true;
			}
		}
		else if (threatTracker != null)
		{
			threatTracker = null;
		}
		if (threatTracker != null && (currentProcess == null || currentProcess.ID != ProcessManager.ProcessID.Game))
		{
			threatTracker.threatDetermine.currentThreat = 0f;
			threatTracker.threatDetermine.currentMusicAgnosticThreat = 0f;
		}
		if (!(musicContext == MusicContext.Menu))
		{
			if (musicContext == MusicContext.Arena && manager.rainWorld.options.arenaMusicVolume > 0f && multiplayerDJ == null)
			{
				multiplayerDJ = new MultiplayerDJ(this);
			}
			else if ((musicContext == MusicContext.StoryMode || manager.rainWorld.options.arenaMusicVolume == 0f) && multiplayerDJ != null)
			{
				multiplayerDJ.ShutDown();
				multiplayerDJ = null;
			}
			if (song != null && song.context != MusicContext.Menu && song.context != musicContext)
			{
				song.FadeOut(2f);
			}
			if (nextSong != null && nextSong.context != MusicContext.Menu && nextSong.context != musicContext)
			{
				nextSong = null;
			}
			if (musicContext != MusicContext.StoryMode && !flag)
			{
				nextProcedural = "";
			}
		}
	}

	public override void Update()
	{
		if (!assetBundlesLoaded && manager.rainWorld.assetBundlesInitialized)
		{
			string error;
			if (!requestedAssetBundlesLoad)
			{
				requestedAssetBundlesLoad = true;
				AssetBundleManager.LoadAssetBundle("music_procedural");
				AssetBundleManager.LoadAssetBundle("music_songs");
			}
			else if (AssetBundleManager.GetLoadedAssetBundle("music_procedural", out error) != null && AssetBundleManager.GetLoadedAssetBundle("music_songs", out error) != null)
			{
				assetBundlesLoaded = true;
			}
		}
		base.Update();
		if (song != null)
		{
			song.Update();
			if (threatTracker != null)
			{
				if (threatTracker.currentThreat > song.fadeOutAtThreat)
				{
					song.FadeOut(200f);
				}
				droneGoalMix = threatTracker.recommendedDroneVolume * Mathf.Lerp(1f - mainSongMix, 1f, song.droneVolume);
			}
			else
			{
				droneGoalMix = 0f;
			}
			if (song.FadingOut)
			{
				mainSongMix = Mathf.Max(0f, mainSongMix - 1f / (1f + song.fadeOutTime));
				if (mainSongMix <= 0f)
				{
					song.StopAndDestroy();
					song = null;
				}
			}
			else if (nextSong == null || nextSong.FadingOut)
			{
				mainSongMix = Mathf.Min(1f, mainSongMix + 1f / (1f + song.fadeInTime));
			}
		}
		else
		{
			mainSongMix = Mathf.Max(0f, mainSongMix - 0.025f);
			if (threatTracker != null)
			{
				droneGoalMix = threatTracker.recommendedDroneVolume;
			}
			else
			{
				droneGoalMix = 0f;
			}
		}
		if (nextSong != null)
		{
			if (song == null)
			{
				song = nextSong;
				song.playWhenReady = true;
				nextSong = null;
				if (song.fadeInTime == 0f)
				{
					mainSongMix = 1f;
				}
				else
				{
					mainSongMix = 0f;
				}
			}
			else
			{
				mainSongMix = Mathf.Max(0f, mainSongMix - 0.0125f);
				if (mainSongMix == 0f)
				{
					song.StopAndDestroy();
					song = null;
				}
			}
		}
		if (proceduralMusic != null)
		{
			if (nextProcedural != null)
			{
				droneGoalMix = 0f;
			}
			proceduralMusic.Update();
			if (nextProcedural != null && proceduralMusic.volume == 0f)
			{
				proceduralMusic.StopAndDestroy();
				proceduralMusic = null;
			}
		}
		else if (nextProcedural != null && assetBundlesLoaded)
		{
			if (nextProcedural != "")
			{
				proceduralMusic = new ProceduralMusic(this, nextProcedural);
				nextProcedural = null;
			}
			else
			{
				proceduralMusic = null;
			}
		}
		if (threatTracker != null)
		{
			threatTracker.Update();
		}
		if (multiplayerDJ != null)
		{
			multiplayerDJ.Update();
		}
	}

	public void NewCycleEvent()
	{
		hasPlayedASongThisCycle = false;
	}

	public void DeathEvent()
	{
		if (song != null && song.stopAtDeath)
		{
			song.FadeOut(60f);
		}
		if (nextSong != null && nextSong.stopAtDeath)
		{
			nextSong = null;
		}
		if (threatTracker != null)
		{
			threatTracker.threatDetermine.threatDeclineCounter = Math.Max(threatTracker.threatDetermine.threatDeclineCounter, 400);
		}
	}

	public void GateEvent()
	{
		if (song != null && song.stopAtGate)
		{
			song.FadeOut(120f);
		}
		if (nextSong != null && nextSong.stopAtGate)
		{
			nextSong = null;
		}
	}

	public void FadeOutAllNonGhostSongs(float fadeOutTime)
	{
		if (song != null && !(song is GhostSong) && (!ModManager.MSC || !(song is SaintEndingSong)))
		{
			song.FadeOut(fadeOutTime);
		}
		if (nextSong != null && !(nextSong is GhostSong) && (!ModManager.MSC || !(song is SaintEndingSong)))
		{
			nextSong = null;
		}
		nextProcedural = "";
	}

	public void FadeOutAllSongs(float fadeOutTime)
	{
		if (song != null)
		{
			song.FadeOut(fadeOutTime);
		}
		if (nextSong != null)
		{
			nextSong = null;
		}
		nextProcedural = "";
	}

	public void RequestGhostSong(string ghostSongName)
	{
		if ((this.song == null || !(this.song is GhostSong)) && (nextSong == null || !(nextSong is GhostSong)) && manager.rainWorld.setup.playMusic)
		{
			Song song = new GhostSong(this, ghostSongName);
			if (this.song == null)
			{
				this.song = song;
				this.song.playWhenReady = true;
			}
			else
			{
				nextSong = song;
				nextSong.playWhenReady = false;
			}
		}
	}

	public void RequestSSSong()
	{
		if ((this.song == null || !(this.song is SSSong)) && (nextSong == null || !(nextSong is SSSong)) && manager.rainWorld.setup.playMusic)
		{
			Song song = null;
			song = ((ModManager.MSC && manager.currentMainLoop is RainWorldGame && (manager.currentMainLoop as RainWorldGame).IsStorySession && ((manager.currentMainLoop as RainWorldGame).world.region.name == "DM" || (manager.currentMainLoop as RainWorldGame).world.region.name == "MS" || (manager.currentMainLoop as RainWorldGame).world.region.name == "SL")) ? new SSSong(this, "RW_95 - Reflection of the Moon") : ((!ModManager.MSC || !(manager.currentMainLoop is RainWorldGame) || !(manager.currentMainLoop as RainWorldGame).IsStorySession || !((manager.currentMainLoop as RainWorldGame).world.region.name == "HR")) ? new SSSong(this) : new SSSong(this, "RW_86 - The Cycle")));
			if (this.song == null)
			{
				this.song = song;
				this.song.playWhenReady = true;
			}
			else
			{
				nextSong = song;
				nextSong.playWhenReady = false;
			}
		}
	}

	public void RequestVoidSeaMusic(VoidSeaScene scene)
	{
		if ((this.song == null || !(this.song is VoidSeaMusic)) && (nextSong == null || !(nextSong is VoidSeaMusic)) && manager.rainWorld.setup.playMusic)
		{
			nextProcedural = "";
			Song song = new VoidSeaMusic(this, scene);
			if (this.song == null)
			{
				this.song = song;
				this.song.playWhenReady = true;
			}
			else
			{
				nextSong = song;
				nextSong.playWhenReady = false;
			}
		}
	}

	public void RequestIntroRollMusic()
	{
		if ((this.song == null || !(this.song is IntroRollMusic)) && (nextSong == null || !(nextSong is IntroRollMusic)) && manager.rainWorld.setup.playMusic)
		{
			nextProcedural = "";
			Song song = new IntroRollMusic(this);
			if (this.song == null)
			{
				this.song = song;
				this.song.playWhenReady = true;
			}
			else
			{
				nextSong = song;
				nextSong.playWhenReady = false;
			}
		}
	}

	public void RequestArenaSong(string songName, float fadeInTime)
	{
		if (song == null)
		{
			Custom.Log("Arena song:", songName);
			song = new Song(this, songName, MusicContext.Arena);
			mainSongMix = 0f;
			song.fadeInTime = fadeInTime;
			song.playWhenReady = true;
			song.baseVolume = 0.3f * manager.rainWorld.options.arenaMusicVolume;
			nextSong = null;
		}
	}

	public void GameRequestsSong(MusicEvent musicEvent)
	{
		Custom.Log("Game request song", musicEvent.songName);
		if (this.song != null)
		{
			Custom.Log("already playing ", this.song.name);
		}
		if (!manager.rainWorld.setup.playMusic || (this.song != null && (this.song.priority >= musicEvent.prio || this.song.name == musicEvent.songName)) || (threatTracker != null && (musicEvent.maxThreatLevel < threatTracker.currentThreat || threatTracker.ghostMode > 0f)))
		{
			return;
		}
		if (manager.currentMainLoop.ID == ProcessManager.ProcessID.Game && (manager.currentMainLoop as RainWorldGame).session is StoryGameSession)
		{
			SaveState saveState = ((manager.currentMainLoop as RainWorldGame).session as StoryGameSession).saveState;
			for (int i = 0; i < saveState.deathPersistentSaveData.songsPlayRecords.Count; i++)
			{
				if (saveState.deathPersistentSaveData.songsPlayRecords[i].songName == musicEvent.songName && (saveState.cycleNumber - saveState.deathPersistentSaveData.songsPlayRecords[i].cycleLastPlayed < musicEvent.cyclesRest || musicEvent.cyclesRest < 0))
				{
					return;
				}
			}
		}
		if (musicEvent.oneSongPerCycle && hasPlayedASongThisCycle)
		{
			return;
		}
		hasPlayedASongThisCycle = true;
		Custom.Log("Play song", musicEvent.songName);
		Song song = new Song(this, musicEvent.songName, MusicContext.StoryMode);
		song.fadeOutAtThreat = musicEvent.maxThreatLevel;
		song.Loop = musicEvent.loop;
		song.priority = musicEvent.prio;
		song.baseVolume = musicEvent.volume;
		song.fadeInTime = musicEvent.fadeInTime;
		song.stopAtDeath = musicEvent.stopAtDeath;
		song.stopAtGate = musicEvent.stopAtGate;
		if (musicEvent.roomsRange > -1 && threatTracker != null)
		{
			song.roomTransitions = musicEvent.roomsRange + threatTracker.roomSwitches;
		}
		if (this.song == null)
		{
			this.song = song;
			this.song.playWhenReady = true;
		}
		else
		{
			if (nextSong != null && (nextSong.priority >= musicEvent.prio || nextSong.name == musicEvent.songName))
			{
				return;
			}
			nextSong = song;
			nextSong.playWhenReady = false;
		}
		if (!(manager.currentMainLoop.ID == ProcessManager.ProcessID.Game) || !((manager.currentMainLoop as RainWorldGame).session is StoryGameSession))
		{
			return;
		}
		SaveState saveState2 = ((manager.currentMainLoop as RainWorldGame).session as StoryGameSession).saveState;
		for (int j = 0; j < saveState2.deathPersistentSaveData.songsPlayRecords.Count; j++)
		{
			if (saveState2.deathPersistentSaveData.songsPlayRecords[j].songName == musicEvent.songName)
			{
				saveState2.deathPersistentSaveData.songsPlayRecords[j].cycleLastPlayed = saveState2.cycleNumber;
				return;
			}
		}
		saveState2.deathPersistentSaveData.songsPlayRecords.Add(new DeathPersistentSaveData.SongPlayRecord(musicEvent.songName, saveState2.cycleNumber));
	}

	public void MenuRequestsSong(string name, float priority, float fadeInTime)
	{
		Custom.Log("Song", name);
		if (song == null || (!(song.priority >= priority) && !(song.name == name)))
		{
			MenuOrSlideShowSong menuOrSlideShowSong = new MenuOrSlideShowSong(this, name, priority, fadeInTime);
			if (song == null)
			{
				song = menuOrSlideShowSong;
				song.playWhenReady = true;
			}
			else if (nextSong == null || (!(nextSong.priority >= priority) && !(nextSong.name == name)))
			{
				nextSong = menuOrSlideShowSong;
				nextSong.playWhenReady = false;
			}
		}
	}

	public void GameRequestsSongStop(StopMusicEvent stopMusicEvent)
	{
		StopSongIfItShouldStop(stopMusicEvent, song);
		StopSongIfItShouldStop(stopMusicEvent, nextSong);
	}

	public void RainRequestStopSong()
	{
		if (song != null && (!ModManager.MSC || !(song is MSSirenSong)))
		{
			song.FadeOut(400f);
		}
		if (nextSong != null)
		{
			nextSong = null;
		}
	}

	private void StopSongIfItShouldStop(StopMusicEvent stopMusicEvent, Song testSong)
	{
		if (testSong != null && !(stopMusicEvent.prio < testSong.priority) && (!(stopMusicEvent.type == StopMusicEvent.Type.SpecificSong) || !(testSong.name != stopMusicEvent.songName)) && (!(stopMusicEvent.type == StopMusicEvent.Type.AllButSpecific) || !(testSong.name == stopMusicEvent.songName)))
		{
			testSong.FadeOut(stopMusicEvent.fadeOutTime);
		}
	}

	public void NewRegion(string newRegion)
	{
		Custom.Log("NEW MUSIC REGION:", newRegion);
		nextProcedural = null;
		if (proceduralMusic == null && assetBundlesLoaded)
		{
			proceduralMusic = new ProceduralMusic(this, newRegion);
		}
		else
		{
			nextProcedural = newRegion;
		}
	}

	public void RequestMSSirenSong()
	{
		if ((this.song == null || !(this.song is MSSirenSong)) && (nextSong == null || !(nextSong is MSSirenSong)) && manager.rainWorld.setup.playMusic)
		{
			Song song = new MSSirenSong(this, manager.currentMainLoop is RainWorldGame && (manager.currentMainLoop as RainWorldGame).IsStorySession && (manager.currentMainLoop as RainWorldGame).world.region.name == "MS");
			if (this.song == null)
			{
				this.song = song;
				this.song.playWhenReady = true;
			}
			else
			{
				nextSong = song;
				nextSong.playWhenReady = false;
			}
		}
	}

	public void RequestSaintEndingSong()
	{
		if ((this.song == null || !(this.song is SaintEndingSong)) && (nextSong == null || !(nextSong is SaintEndingSong)) && manager.rainWorld.setup.playMusic)
		{
			Song song = new SaintEndingSong(this);
			if (this.song == null)
			{
				this.song = song;
				this.song.playWhenReady = true;
			}
			else
			{
				nextSong = song;
				nextSong.playWhenReady = false;
			}
		}
	}

	public void RequestSMEndingSong()
	{
		if ((this.song == null || !(this.song is SMEndingSong)) && (nextSong == null || !(nextSong is SMEndingSong)) && manager.rainWorld.setup.playMusic)
		{
			Song song = new SMEndingSong(this);
			if (this.song == null)
			{
				this.song = song;
				this.song.playWhenReady = true;
			}
			else
			{
				nextSong = song;
				nextSong.playWhenReady = false;
			}
		}
	}

	public void RequestHalcyonSong(string songName)
	{
		if ((this.song == null || !(this.song is HalcyonSong)) && (nextSong == null || !(nextSong is HalcyonSong)) && manager.rainWorld.setup.playMusic)
		{
			Song song = new HalcyonSong(this, songName);
			if (this.song == null)
			{
				this.song = song;
				this.song.playWhenReady = true;
			}
			else
			{
				nextSong = song;
				nextSong.playWhenReady = false;
			}
		}
	}
}
