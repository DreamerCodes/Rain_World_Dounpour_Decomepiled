using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Music;

public class MultiplayerDJ
{
	private MusicPlayer musicPlayer;

	public List<string> playList;

	public string[] availableSongs;

	public bool firstSong = true;

	public int menuCounter;

	public string announceSong;

	public int announceCounter;

	public bool SandboxEditMode
	{
		get
		{
			if (musicPlayer.manager.currentMainLoop != null && musicPlayer.manager.currentMainLoop is RainWorldGame && (musicPlayer.manager.currentMainLoop as RainWorldGame).IsArenaSession && (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession is SandboxGameSession)
			{
				return !((musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession as SandboxGameSession).PlayMode;
			}
			return false;
		}
	}

	public bool ChallengeMode
	{
		get
		{
			if (ModManager.MSC && musicPlayer.manager.currentMainLoop != null && musicPlayer.manager.currentMainLoop is RainWorldGame && (musicPlayer.manager.currentMainLoop as RainWorldGame).IsArenaSession)
			{
				return (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge;
			}
			return false;
		}
	}

	public ChallengeInformation.ChallengeMeta ChallengeMeta
	{
		get
		{
			if (!ModManager.MSC)
			{
				return null;
			}
			if (musicPlayer.manager.currentMainLoop != null && musicPlayer.manager.currentMainLoop is RainWorldGame && (musicPlayer.manager.currentMainLoop as RainWorldGame).IsArenaSession)
			{
				return (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta;
			}
			return null;
		}
	}

	public MultiplayerDJ(MusicPlayer musicPlayer)
	{
		this.musicPlayer = musicPlayer;
		availableSongs = new string[0];
		playList = new List<string>();
		string path = AssetManager.ResolveFilePath("Music" + Path.DirectorySeparatorChar + "MPMusic.txt");
		if (File.Exists(path))
		{
			availableSongs = File.ReadAllLines(path);
		}
	}

	public void ShufflePlaylist()
	{
		playList.Clear();
		List<string> list = new List<string>();
		for (int i = 0; i < availableSongs.Length; i++)
		{
			list.Add(availableSongs[i]);
		}
		int num = 0;
		while (list.Count > 0 && num < 10000)
		{
			int index = Random.Range(0, list.Count);
			playList.Add(list[index]);
			list.RemoveAt(index);
			num++;
		}
	}

	public void Update()
	{
		if (musicPlayer.manager.currentMainLoop == null)
		{
			return;
		}
		ChallengeInformation.ChallengeMeta challengeMeta = ChallengeMeta;
		bool challengeMode = ChallengeMode;
		if (challengeMode && challengeMeta.specificMusic != null && challengeMeta.specificMusic != "")
		{
			if (musicPlayer.song != null)
			{
				if (musicPlayer.song.name != challengeMeta.specificMusic)
				{
					musicPlayer.song.FadeOut(60f);
				}
			}
			else
			{
				playList = new List<string>();
				playList.Add(challengeMeta.specificMusic);
			}
		}
		if (SandboxEditMode)
		{
			if (musicPlayer.song != null)
			{
				musicPlayer.song.baseVolume = Custom.LerpAndTick(musicPlayer.song.baseVolume, 0.05f * musicPlayer.manager.rainWorld.options.arenaMusicVolume, 0.03f, 1f / 70f);
			}
			return;
		}
		if (challengeMode && challengeMeta.musicMuted)
		{
			if (musicPlayer.song == null)
			{
				return;
			}
			if (!string.IsNullOrEmpty(challengeMeta.threatMusic))
			{
				if (!musicPlayer.song.FadingOut)
				{
					musicPlayer.song.FadeOut(1f);
				}
			}
			else
			{
				musicPlayer.song.baseVolume = Custom.LerpAndTick(musicPlayer.song.baseVolume, 0f, 0.03f, 0.02f);
			}
			return;
		}
		if (musicPlayer.song != null)
		{
			musicPlayer.song.baseVolume = Custom.LerpAndTick(musicPlayer.song.baseVolume, 0.3f * musicPlayer.manager.rainWorld.options.arenaMusicVolume, 0.03f, 0.02f);
		}
		if (firstSong)
		{
			if (musicPlayer.manager.currentMainLoop.ID == ProcessManager.ProcessID.Game)
			{
				firstSong = false;
				return;
			}
			menuCounter++;
			if (menuCounter > 800 && musicPlayer.musicContext == MusicPlayer.MusicContext.Arena && musicPlayer.song == null)
			{
				PlayNext(800f);
			}
			return;
		}
		if (musicPlayer.musicContext == MusicPlayer.MusicContext.Arena && musicPlayer.song == null)
		{
			PlayNext(60f);
		}
		if (announceSong != null)
		{
			announceCounter--;
			if (challengeMode && challengeMeta.musicMuted)
			{
				announceCounter = 0;
			}
			if (announceCounter < 1)
			{
				announceSong = null;
			}
			else if (musicPlayer.manager.currentMainLoop is RainWorldGame && (musicPlayer.manager.currentMainLoop as RainWorldGame).IsArenaSession && (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.initiated && (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.counter > 40 && (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0] != null && (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].room != null && (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].room.ReadyForPlayer && (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud != null && (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud.textPrompt != null && (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud.textPrompt.messages.Count == 0)
			{
				(musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud.textPrompt.AddMusicMessage(announceSong, 240);
				announceSong = null;
				announceCounter = 0;
			}
		}
	}

	private void PlayNext(float fadeInTime)
	{
		if (playList.Count < 1)
		{
			ShufflePlaylist();
		}
		musicPlayer.RequestArenaSong(playList[0], fadeInTime);
		string[] array = Regex.Split(playList[0], " - ");
		if (array.Length > 1)
		{
			announceSong = array[1];
			announceCounter = 500;
		}
		playList.RemoveAt(0);
		firstSong = false;
	}

	public void ShutDown()
	{
	}
}
