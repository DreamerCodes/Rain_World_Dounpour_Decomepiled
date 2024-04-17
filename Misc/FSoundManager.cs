using System.Collections.Generic;
using UnityEngine;

public class FSoundManager
{
	public static string resourcePrefix = "Audio/";

	private static GameObject _gameObject;

	private static AudioSource _soundSource;

	private static AudioSource _musicSource;

	private static string _currentMusicPath = "";

	private static bool _isMuted = false;

	private static Dictionary<string, AudioClip> _soundClips = new Dictionary<string, AudioClip>();

	private static AudioClip _currentMusicClip = null;

	private static float _volume = 1f;

	public static float volume
	{
		get
		{
			return AudioListener.volume;
		}
		set
		{
			_volume = value;
			if (_isMuted)
			{
				AudioListener.volume = 0f;
			}
			else
			{
				AudioListener.volume = _volume;
			}
		}
	}

	public static bool isMuted
	{
		get
		{
			return AudioListener.pause;
		}
		set
		{
			_isMuted = value;
			AudioListener.pause = value;
			if (_isMuted)
			{
				AudioListener.volume = 0f;
			}
			else
			{
				AudioListener.volume = _volume;
			}
		}
	}

	public static void Init()
	{
		_gameObject = new GameObject("FSoundManager");
		_musicSource = _gameObject.AddComponent<AudioSource>();
		_soundSource = _gameObject.AddComponent<AudioSource>();
		_gameObject.AddComponent<AudioListener>();
	}

	public static void SetResourcePrefix(string prefix)
	{
		resourcePrefix = prefix;
	}

	public static void PreloadSound(string resourceName)
	{
		string text = resourcePrefix + resourceName;
		if (!_soundClips.ContainsKey(text))
		{
			AudioClip audioClip = Resources.Load(text) as AudioClip;
			if (!(audioClip == null))
			{
				_soundClips[text] = audioClip;
			}
		}
	}

	public static void PlaySound(string resourceName, float volume)
	{
		if (_isMuted)
		{
			return;
		}
		if (_soundSource == null)
		{
			Init();
		}
		string text = resourcePrefix + resourceName;
		AudioClip audioClip;
		if (_soundClips.ContainsKey(text))
		{
			audioClip = _soundClips[text];
		}
		else
		{
			audioClip = Resources.Load(text) as AudioClip;
			if (audioClip == null)
			{
				return;
			}
			_soundClips[text] = audioClip;
		}
		_soundSource.PlayOneShot(audioClip, volume);
	}

	public static void PlaySound(string resourceName)
	{
		PlaySound(resourceName, 1f);
	}

	public static void PlayMusic(string resourceName, float volume)
	{
		PlayMusic(resourceName, volume, shouldRestartIfSameSongIsAlreadyPlaying: true);
	}

	public static void PlayMusic(string resourceName, float volume, bool shouldRestartIfSameSongIsAlreadyPlaying)
	{
		if (_isMuted)
		{
			return;
		}
		if (_musicSource == null)
		{
			Init();
		}
		string text = resourcePrefix + resourceName;
		if (_currentMusicClip != null)
		{
			if (_currentMusicPath == text)
			{
				if (shouldRestartIfSameSongIsAlreadyPlaying)
				{
					_musicSource.Stop();
					_musicSource.volume = volume;
					_musicSource.loop = true;
					_musicSource.Play();
				}
				return;
			}
			_musicSource.Stop();
			Resources.UnloadAsset(_currentMusicClip);
			_currentMusicClip = null;
			_currentMusicPath = "";
		}
		_currentMusicClip = Resources.Load(text) as AudioClip;
		if (!(_currentMusicClip == null))
		{
			_currentMusicPath = text;
			_musicSource.clip = _currentMusicClip;
			_musicSource.volume = volume;
			_musicSource.loop = true;
			_musicSource.Play();
		}
	}

	public static void PlayMusic(string resourceName)
	{
		PlayMusic(resourceName, 1f);
	}

	public static void StopMusic()
	{
		if (_musicSource != null)
		{
			_musicSource.Stop();
		}
	}

	public static void UnloadSound(string resourceName)
	{
		string key = resourcePrefix + resourceName;
		if (_soundClips.ContainsKey(key))
		{
			Resources.UnloadAsset(_soundClips[key]);
			_soundClips.Remove(key);
		}
	}

	public static void UnloadMusic()
	{
		if (_currentMusicClip != null)
		{
			Resources.UnloadAsset(_currentMusicClip);
			_currentMusicClip = null;
			_currentMusicPath = "";
		}
	}

	public static void UnloadAllSounds()
	{
		foreach (AudioClip value in _soundClips.Values)
		{
			Resources.UnloadAsset(value);
		}
		_soundClips.Clear();
	}

	public static void UnloadAllSoundsAndMusic()
	{
		UnloadAllSounds();
		UnloadMusic();
	}
}
