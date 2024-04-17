using System;
using System.Collections.Generic;
using System.IO;
using AssetBundles;
using RWCustom;
using UnityEngine;

namespace Music;

public abstract class MusicPiece
{
	public class SubTrack
	{
		public MusicPiece piece;

		public int index;

		public AudioSource source;

		public bool readyToPlay;

		public float volume = 1f;

		public string trackName;

		private AssetBundleLoadAssetOperation loadOp;

		private bool requestPlay;

		public bool isStreamed;

		public bool isSynced;

		public SubTrack(MusicPiece piece, int index, string trackName)
		{
			this.piece = piece;
			this.index = index;
			this.trackName = trackName;
			isSynced = true;
			source = piece.musicPlayer.gameObj.AddComponent<AudioSource>();
			source.loop = piece.Loop;
			source.volume = 0f;
		}

		public void Update()
		{
			if (requestPlay && readyToPlay)
			{
				requestPlay = false;
				StartPlaying();
			}
			if (piece.startedPlaying && piece.musicPlayer.manager.rainWorld.OptionsReady)
			{
				source.volume = Mathf.Clamp01(Mathf.Pow(volume * piece.volume * piece.musicPlayer.manager.rainWorld.options.musicVolume, piece.musicPlayer.manager.soundLoader.volumeExponent));
			}
			if (readyToPlay || !piece.musicPlayer.assetBundlesLoaded)
			{
				return;
			}
			if (source.clip == null)
			{
				if (piece.IsProcedural)
				{
					isStreamed = false;
					string text = "Music" + Path.DirectorySeparatorChar + "Procedural" + Path.DirectorySeparatorChar + trackName + ".ogg";
					string text2 = AssetManager.ResolveFilePath(text);
					if (!Application.isConsolePlatform && text2 != Path.Combine(Custom.RootFolderDirectory(), text.ToLowerInvariant()) && File.Exists(text2))
					{
						source.clip = AssetManager.SafeWWWAudioClip("file://" + text2, threeD: false, stream: true, AudioType.OGGVORBIS);
						isStreamed = true;
					}
					else if (loadOp == null)
					{
						string error;
						LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle("music_procedural", out error);
						if (loadedAssetBundle != null)
						{
							source.clip = loadedAssetBundle.m_AssetBundle.LoadAsset<AudioClip>(trackName);
						}
						if (source.clip == null)
						{
							loadOp = AssetBundleManager.LoadAssetAsync("music_procedural", trackName, typeof(AudioClip));
						}
					}
				}
				else
				{
					isStreamed = false;
					string text3 = "Music" + Path.DirectorySeparatorChar + "Songs" + Path.DirectorySeparatorChar + trackName + ".ogg";
					string text4 = AssetManager.ResolveFilePath(text3);
					if (!Application.isConsolePlatform && text4 != Path.Combine(Custom.RootFolderDirectory(), text3.ToLowerInvariant()) && File.Exists(text4))
					{
						source.clip = AssetManager.SafeWWWAudioClip("file://" + text4, threeD: false, stream: true, AudioType.OGGVORBIS);
						isStreamed = true;
					}
					else if (loadOp == null)
					{
						string error2;
						LoadedAssetBundle loadedAssetBundle2 = AssetBundleManager.GetLoadedAssetBundle("music_songs", out error2);
						if (loadedAssetBundle2 != null)
						{
							source.clip = loadedAssetBundle2.m_AssetBundle.LoadAsset<AudioClip>(trackName);
						}
						if (source.clip == null)
						{
							loadOp = AssetBundleManager.LoadAssetAsync("music_songs", trackName, typeof(AudioClip));
						}
					}
				}
				if (loadOp != null && loadOp.IsDone())
				{
					source.clip = loadOp.GetAsset<AudioClip>();
					loadOp = null;
					source.clip.LoadAudioData();
				}
				else if (source.clip != null)
				{
					source.clip.LoadAudioData();
				}
			}
			else if (!source.isPlaying && AudioDataLoadState.Loaded == source.clip.loadState)
			{
				readyToPlay = true;
			}
		}

		public void StartPlaying()
		{
			if (!readyToPlay)
			{
				requestPlay = true;
				return;
			}
			bool flag = true;
			for (int i = 0; i < piece.subTracks.Count; i++)
			{
				if (piece.subTracks[i] != this && !piece.subTracks[i].readyToPlay)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				requestPlay = true;
			}
			else
			{
				source.Play();
			}
		}

		public void StopAndDestroy()
		{
			requestPlay = false;
			source.Stop();
			UnityEngine.Object.Destroy(source);
		}
	}

	public MusicPlayer musicPlayer;

	public List<SubTrack> subTracks;

	public bool startedPlaying;

	public bool playWhenReady;

	public string name;

	public float volume = 0.3f;

	private bool lp;

	public MusicPlayer.MusicContext context;

	public bool IsProcedural
	{
		get
		{
			if (!(this is ProceduralMusic))
			{
				return this is VoidSeaMusic;
			}
			return true;
		}
	}

	public bool Loop
	{
		get
		{
			return lp;
		}
		set
		{
			lp = value;
			for (int i = 0; i < subTracks.Count; i++)
			{
				subTracks[i].source.loop = lp;
			}
		}
	}

	public MusicPiece(MusicPlayer musicPlayer, string name, MusicPlayer.MusicContext context)
	{
		this.musicPlayer = musicPlayer;
		this.name = name;
		this.context = context;
		subTracks = new List<SubTrack>();
	}

	public virtual void Update()
	{
		AudioSource audioSource = null;
		for (int i = 0; i < subTracks.Count; i++)
		{
			subTracks[i].Update();
			if (ModManager.MMF && IsProcedural)
			{
				if (audioSource == null && subTracks[i].source.isPlaying)
				{
					audioSource = subTracks[i].source;
				}
				else if (audioSource != null && subTracks[i].isStreamed && subTracks[i].isSynced && subTracks[i].source.isPlaying && Math.Abs(audioSource.timeSamples - subTracks[i].source.timeSamples) >= audioSource.clip.frequency / 4)
				{
					subTracks[i].source.timeSamples = audioSource.timeSamples;
				}
			}
		}
		if (startedPlaying || !playWhenReady)
		{
			return;
		}
		bool flag = true;
		for (int j = 0; j < subTracks.Count && flag; j++)
		{
			if (!subTracks[j].readyToPlay)
			{
				flag = false;
			}
		}
		if (flag)
		{
			StartPlaying();
		}
	}

	protected virtual void StartPlaying()
	{
		startedPlaying = true;
		for (int i = 0; i < subTracks.Count; i++)
		{
			subTracks[i].StartPlaying();
		}
	}

	public void StopAndDestroy()
	{
		for (int i = 0; i < subTracks.Count; i++)
		{
			subTracks[i].StopAndDestroy();
		}
	}
}
