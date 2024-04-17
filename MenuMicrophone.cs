using System.Collections.Generic;
using System.IO;
using AssetBundles;
using Menu;
using RWCustom;
using UnityEngine;

public class MenuMicrophone : MainLoopProcess
{
	public class MenuSoundObject
	{
		public SoundLoader.SoundData soundData;

		protected MenuMicrophone mic;

		protected GameObject gameObject;

		public AudioSource audioSource;

		private AudioSourcePoolItem aspi;

		public bool loop;

		public bool slatedForDeletion;

		private AssetBundleLoadAssetOperation loadOp;

		private bool autoPlayAfterLoad = true;

		private bool needsRandomTime;

		private bool needsVolumeUpdate;

		private float initVol;

		public bool allowPlay;

		public bool singleUseSound;

		public virtual bool Done
		{
			get
			{
				if (audioSource == null || (!audioSource.isPlaying && loadOp == null))
				{
					if (soundData.dontAutoPlay)
					{
						return allowPlay;
					}
					return true;
				}
				return false;
			}
		}

		public virtual bool PlayAgain
		{
			get
			{
				if (audioSource != null)
				{
					return loop;
				}
				return false;
			}
		}

		protected float SetVolume
		{
			set
			{
				if (mic.manager.rainWorld.OptionsReady)
				{
					audioSource.volume = Mathf.Clamp01(Mathf.Pow(value * soundData.vol * mic.manager.rainWorld.options.soundEffectsVolume, mic.soundLoader.volumeExponent));
				}
				else
				{
					audioSource.volume = 0f;
				}
			}
		}

		protected float SetPitch
		{
			set
			{
				audioSource.pitch = value * soundData.pitch;
			}
		}

		public MenuSoundObject(MenuMicrophone mic, SoundLoader.SoundData soundData, bool loop, float initPan, float initVol, float initPitch, bool startAtRandomTime)
		{
			this.mic = mic;
			this.soundData = soundData;
			string name;
			AudioClip audioClip = mic.soundLoader.GetAudioClip(soundData.audioClip, out loadOp, out name);
			aspi = SimplePool<AudioSourcePoolItem>.Instance.Pop();
			if (aspi.slatedForPlay)
			{
				Custom.Log("Slated for play was true for aspi!");
			}
			slatedForDeletion = false;
			aspi.slatedForPlay = true;
			gameObject = aspi.gameObject;
			audioSource = aspi.audioSource;
			audioSource.clip = audioClip;
			audioSource.spatialBlend = 0f;
			audioSource.loop = loop;
			audioSource.panStereo = initPan;
			SetVolume = initVol;
			SetPitch = initPitch;
			if (!mic.soundLoader.assetBundlesLoaded || !mic.manager.rainWorld.OptionsReady)
			{
				needsVolumeUpdate = true;
				this.initVol = initVol;
			}
			this.loop = loop;
			Play();
			if (startAtRandomTime)
			{
				if (audioClip != null)
				{
					audioSource.time = audioClip.length * Random.value;
				}
				else
				{
					needsRandomTime = true;
				}
			}
		}

		public virtual void Update(float timeStacker, float timeSpeed)
		{
			if (needsVolumeUpdate && mic.soundLoader.assetBundlesLoaded && mic.manager.rainWorld.OptionsReady)
			{
				needsVolumeUpdate = false;
				SetVolume = initVol;
			}
			if (!slatedForDeletion && audioSource.clip == null && loadOp != null && loadOp.IsDone())
			{
				audioSource.clip = loadOp.GetAsset<AudioClip>();
				loadOp = null;
				if (autoPlayAfterLoad)
				{
					Play();
				}
			}
			if (audioSource.clip != null && !autoPlayAfterLoad && soundData.dontAutoPlay && !allowPlay && audioSource.clip.loadState == AudioDataLoadState.Loaded)
			{
				allowPlay = true;
				audioSource.Play();
			}
		}

		public void Destroy(bool push = true)
		{
			slatedForDeletion = true;
			aspi.slatedForPlay = false;
			if (push)
			{
				SimplePool<AudioSourcePoolItem>.Instance.Push(aspi);
			}
			if (singleUseSound && audioSource != null && audioSource.clip != null)
			{
				Object.Destroy(audioSource.clip);
				audioSource.clip = null;
			}
		}

		public void Destroy()
		{
			Destroy(push: true);
		}

		public void Play()
		{
			if (audioSource.clip != null)
			{
				if (!soundData.dontAutoPlay || allowPlay)
				{
					audioSource.Play();
				}
				if (needsRandomTime)
				{
					needsRandomTime = false;
					audioSource.time = audioSource.clip.length * Random.value;
				}
			}
		}

		public void Stop()
		{
			if (audioSource != null)
			{
				audioSource.Stop();
				autoPlayAfterLoad = false;
			}
		}
	}

	public class MenuSoundLoop : MenuSoundObject
	{
		public float loopVolume;

		public float loopPitch;

		private float fade;

		public bool isBkgLoop;

		public MenuSoundLoop(MenuMicrophone mic, SoundLoader.SoundData soundData, bool loop, float initPan, float initVol, float initPitch, bool startAtRandomTime, bool isBkgLoop)
			: base(mic, soundData, loop: true, initPan, initVol, initPitch, startAtRandomTime: false)
		{
			loopVolume = initVol;
			loopPitch = initPitch;
			base.SetVolume = fade * loopVolume;
			base.SetPitch = loopPitch;
			this.isBkgLoop = isBkgLoop;
		}

		public override void Update(float timeStacker, float timeSpeed)
		{
			base.Update(timeStacker, timeSpeed);
			if (!audioSource.isPlaying)
			{
				return;
			}
			base.SetVolume = fade * loopVolume;
			base.SetPitch = loopPitch;
			if (mic.manager.currentMainLoop is global::Menu.Menu)
			{
				fade = Mathf.Min(1f, fade + 0.025f);
				return;
			}
			fade = Mathf.Max(0f, fade - 0.025f);
			if (fade == 0f)
			{
				Destroy();
			}
		}
	}

	public SoundLoader soundLoader;

	public List<MenuSoundObject> soundObjects;

	public MenuMicrophone(ProcessManager manager, SoundLoader soundLoader)
		: base(manager, ProcessManager.ProcessID.MenuMic)
	{
		this.soundLoader = soundLoader;
		soundObjects = new List<MenuSoundObject>();
		AudioListener.volume = soundLoader.volume;
	}

	public void AllQuiet()
	{
		for (int i = 0; i < soundObjects.Count; i++)
		{
			soundObjects[i].Destroy();
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		for (int num = soundObjects.Count - 1; num >= 0; num--)
		{
			if (soundObjects[num].slatedForDeletion)
			{
				soundObjects[num].Destroy(push: false);
				soundObjects.RemoveAt(num);
			}
			else if (soundObjects[num].Done)
			{
				if (soundObjects[num].PlayAgain)
				{
					soundObjects[num].Play();
				}
				else
				{
					soundObjects[num].Destroy();
					soundObjects.RemoveAt(num);
				}
			}
			else
			{
				soundObjects[num].Update(timeStacker, 1f);
			}
		}
	}

	public void PlaySound(SoundID soundId)
	{
		PlaySound(soundId, 0f, 1f, 1f);
	}

	public void PlaySound(SoundID soundId, float pan, float vol, float pitch)
	{
		if (!soundLoader.ShouldSoundPlay(soundId))
		{
			return;
		}
		if (!soundLoader.TriggerPlayAll(soundId))
		{
			SoundLoader.SoundData soundData = GetSoundData(soundId, -1);
			if (SoundClipReady(soundData))
			{
				soundObjects.Add(new MenuSoundObject(this, soundData, loop: false, pan, vol, pitch, startAtRandomTime: false));
			}
			return;
		}
		for (int i = 0; i < soundLoader.TriggerSamples(soundId); i++)
		{
			SoundLoader.SoundData soundData2 = GetSoundData(soundId, i);
			if (SoundClipReady(soundData2))
			{
				soundObjects.Add(new MenuSoundObject(this, soundData2, loop: false, pan, vol, pitch, startAtRandomTime: false));
			}
		}
	}

	public MenuSoundLoop PlayLoop(SoundID soundId, float pan, float vol, float pitch, bool isBkgLoop)
	{
		if (!soundLoader.ShouldSoundPlay(soundId))
		{
			return null;
		}
		MenuSoundLoop menuSoundLoop = null;
		SoundLoader.SoundData soundData = GetSoundData(soundId, -1);
		if (SoundClipReady(soundData))
		{
			menuSoundLoop = new MenuSoundLoop(this, soundData, loop: false, pan, vol, pitch, startAtRandomTime: false, isBkgLoop);
			soundObjects.Add(menuSoundLoop);
		}
		return menuSoundLoop;
	}

	public SoundLoader.SoundData GetSoundData(SoundID soundId, int index)
	{
		if (index > -1)
		{
			return soundLoader.GetSoundData(soundId, index);
		}
		return soundLoader.GetSoundData(soundId);
	}

	private bool SoundClipReady(SoundLoader.SoundData soundData)
	{
		if (soundLoader.GetAudioClip(soundData.audioClip, out var loadOp, out var _) != null)
		{
			return true;
		}
		return loadOp != null;
	}

	public void PlayCustomSound(string soundName, float pan, float vol, float pitch)
	{
		SoundLoader.SoundData soundData = GetSoundData(SoundID.Slugcat_Stash_Spear_On_Back, -1);
		soundData.dontAutoPlay = true;
		soundData.soundName = soundName;
		MenuSoundObject menuSoundObject = new MenuSoundObject(this, soundData, loop: false, pan, vol, pitch, startAtRandomTime: false);
		menuSoundObject.singleUseSound = true;
		string text = AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + soundName + ".wav");
		menuSoundObject.audioSource.clip = AssetManager.SafeWWWAudioClip("file://" + text, threeD: false, stream: true, AudioType.WAV);
		soundObjects.Add(menuSoundObject);
	}

	public MenuSoundLoop PlayCustomLoop(string soundName, float pan, float vol, float pitch, bool isBkgLoop)
	{
		SoundLoader.SoundData soundData = GetSoundData(SoundID.Slugcat_Stash_Spear_On_Back, -1);
		soundData.dontAutoPlay = true;
		soundData.soundName = soundName;
		MenuSoundLoop menuSoundLoop = new MenuSoundLoop(this, soundData, loop: false, pan, vol, pitch, startAtRandomTime: false, isBkgLoop);
		menuSoundLoop.singleUseSound = true;
		string text = AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + soundName + ".wav");
		menuSoundLoop.audioSource.clip = AssetManager.SafeWWWAudioClip("file://" + text, threeD: false, stream: true, AudioType.WAV);
		soundObjects.Add(menuSoundLoop);
		return menuSoundLoop;
	}
}
