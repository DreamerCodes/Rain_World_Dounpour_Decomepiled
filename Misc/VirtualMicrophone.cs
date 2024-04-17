using System;
using System.Collections.Generic;
using System.IO;
using AssetBundles;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class VirtualMicrophone
{
	public abstract class SoundObject
	{
		public SoundLoader.SoundData soundData;

		protected VirtualMicrophone mic;

		protected GameObject gameObject;

		public AudioSource audioSource;

		private AudioSourcePoolItem aspi;

		public bool loop;

		public int volumeGroup;

		public bool slatedForDeletion;

		protected AssetBundleLoadAssetOperation loadOp;

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

		public bool IsLoaded
		{
			get
			{
				if (audioSource != null)
				{
					return loadOp == null;
				}
				return false;
			}
		}

		public bool IsPlaying
		{
			get
			{
				if (audioSource != null)
				{
					if (!audioSource.isPlaying)
					{
						return loadOp != null;
					}
					return true;
				}
				return false;
			}
		}

		protected float SetVolume
		{
			set
			{
				if (mic.camera.game.rainWorld.OptionsReady)
				{
					audioSource.volume = Mathf.Clamp01(Mathf.Pow(value * soundData.vol * mic.volumeGroups[volumeGroup] * mic.camera.game.rainWorld.options.soundEffectsVolume, mic.soundLoader.volumeExponent));
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

		public SoundObject(VirtualMicrophone mic, SoundLoader.SoundData soundData, bool loop, float initPan, float initVol, float initPitch, bool startAtRandomTime)
		{
			this.mic = mic;
			this.soundData = soundData;
			string name;
			AudioClip audioClip = mic.soundLoader.GetAudioClip(soundData.audioClip, out loadOp, out name);
			aspi = SimplePool<AudioSourcePoolItem>.Instance.Pop();
			if (aspi.slatedForPlay)
			{
				Custom.LogWarning("Slated for play was true for aspi:", aspi.name);
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
			this.loop = loop;
			if (!mic.soundLoader.assetBundlesLoaded || !mic.camera.game.rainWorld.OptionsReady)
			{
				needsVolumeUpdate = true;
				this.initVol = initVol;
			}
			Play();
			if (startAtRandomTime)
			{
				if (audioClip != null)
				{
					audioSource.time = audioClip.length * UnityEngine.Random.value;
				}
				else
				{
					needsRandomTime = true;
				}
			}
		}

		public void SetLowPassCutOff(float effect)
		{
			bool enabled = aspi.alpf.enabled;
			if (effect == 0f && enabled)
			{
				aspi.alpf.enabled = false;
			}
			else if (effect > 0f && !enabled)
			{
				aspi.alpf.enabled = true;
			}
			if (effect > 0f)
			{
				aspi.alpf.cutoffFrequency = Mathf.Lerp(22000f, 1500f, Mathf.Pow(effect, 0.5f));
			}
		}

		public virtual void Update(float timeStacker, float timeSpeed)
		{
			UpdateLoading();
			SetLowPassCutOff(mic.underWater);
			if (soundData.dontAutoPlay && !allowPlay && audioSource.clip.loadState == AudioDataLoadState.Loaded)
			{
				allowPlay = true;
				audioSource.Play();
			}
		}

		public void Destroy(bool push = true)
		{
			if (gameObject != null && aspi.alpf.enabled)
			{
				aspi.alpf.enabled = false;
			}
			aspi.slatedForPlay = false;
			slatedForDeletion = true;
			if (push)
			{
				SimplePool<AudioSourcePoolItem>.Instance.Push(aspi);
			}
			if (singleUseSound && audioSource != null && audioSource.clip != null)
			{
				UnityEngine.Object.Destroy(audioSource.clip);
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
					audioSource.time = audioSource.clip.length * UnityEngine.Random.value;
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

		public void GetSpectrumData(float[] samples, int channel, FFTWindow window)
		{
			if (audioSource != null)
			{
				audioSource.GetSpectrumData(samples, channel, window);
			}
		}

		protected void UpdateLoading()
		{
			if (!slatedForDeletion && needsVolumeUpdate && mic.soundLoader.assetBundlesLoaded && mic.camera.game.rainWorld.OptionsReady)
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
		}
	}

	public class DisembodiedSound : SoundObject
	{
		public DisembodiedSound(VirtualMicrophone mic, SoundLoader.SoundData sData, float initPan, float initVol, float initPitch, bool startAtRandomTime, int volumeGroup)
			: base(mic, sData, loop: false, initPan, initVol, initPitch, startAtRandomTime)
		{
			base.volumeGroup = volumeGroup;
			base.SetVolume = initVol;
		}
	}

	public abstract class PositionedSound : SoundObject
	{
		private Vector2 privLastPos;

		private Vector2 privPos;

		public float volume;

		public float pitch;

		private float lastDopplerDist;

		private float lastDopplerPitch = 1f;

		private float lastTime;

		public virtual Vector2 lastPos
		{
			get
			{
				return privLastPos;
			}
			set
			{
				privLastPos = value;
			}
		}

		public virtual Vector2 pos
		{
			get
			{
				return privPos;
			}
			set
			{
				privPos = value;
			}
		}

		public PositionedSound(VirtualMicrophone mic, SoundLoader.SoundData sData, bool loop, Vector2 pos, float volume, float pitch, bool startAtRandomTime)
			: base(mic, sData, loop, 0f, 0f, 1f, startAtRandomTime)
		{
			this.pos = pos;
			lastPos = pos;
			this.volume = volume;
			this.pitch = pitch;
			lastTime = Time.time;
		}

		public override void Update(float timeStacker, float timeSpeed)
		{
			UpdateLoading();
			Vector2 ps = Vector2.Lerp(lastPos, pos, timeStacker);
			audioSource.panStereo = mic.PanFromPoint(ps, timeStacker);
			base.SetVolume = mic.VolFromPoint(ps, timeStacker, soundData.range) * volume;
			if (soundData.dopplerFac > 0f)
			{
				float num = Time.time - lastTime;
				if (num > 0.05f)
				{
					lastTime = Time.time;
					float num2 = Vector2.Distance(pos, mic.listenerPoint);
					float num3 = (lastDopplerDist - num2) / num;
					lastDopplerDist = num2;
					lastDopplerPitch = Mathf.Clamp(1f + num3 / 4000f * soundData.dopplerFac * (1f - mic.dopplerBlock), 0.1f, 2f);
				}
				base.SetPitch = pitch * lastDopplerPitch * timeSpeed;
			}
			else
			{
				base.SetPitch = pitch * timeSpeed;
			}
			if (ModManager.MSC && mic.room.waterInverted)
			{
				SetLowPassCutOff(Mathf.Max(mic.underWater, 0.75f * (1f - Mathf.InverseLerp(mic.room.FloatWaterLevel(ps.x) + 20f, mic.room.FloatWaterLevel(ps.x) - 20f, ps.y))));
			}
			else
			{
				SetLowPassCutOff(Mathf.Max(mic.underWater, 0.75f * Mathf.InverseLerp(mic.room.FloatWaterLevel(ps.x) + 20f, mic.room.FloatWaterLevel(ps.x) - 20f, ps.y)));
			}
			if (soundData.dontAutoPlay && !allowPlay && audioSource.clip.loadState == AudioDataLoadState.Loaded)
			{
				allowPlay = true;
				audioSource.Play();
			}
		}
	}

	public class StaticPositionSound : PositionedSound
	{
		public StaticPositionSound(VirtualMicrophone mic, SoundLoader.SoundData sData, Vector2 pos, float volume, float pitch, bool startAtRandomTime)
			: base(mic, sData, loop: false, pos, volume, pitch, startAtRandomTime)
		{
		}

		public override void Update(float timeStacker, float timeSpeed)
		{
			base.Update(timeStacker, timeSpeed);
		}
	}

	public class RectangularSound : PositionedSound
	{
		public RectSoundEmitter controller;

		public FloatRect rect;

		public override bool Done
		{
			get
			{
				if ((!(audioSource == null) && (audioSource.isPlaying || loadOp != null)) || (soundData.dontAutoPlay && !allowPlay))
				{
					return controller.slatedForDeletetion;
				}
				return true;
			}
		}

		public override bool PlayAgain
		{
			get
			{
				if (audioSource != null && loop)
				{
					return !controller.slatedForDeletetion;
				}
				return false;
			}
		}

		public override Vector2 lastPos
		{
			get
			{
				return Custom.RestrictInRect(mic.lastListenerPoint, rect);
			}
			set
			{
			}
		}

		public override Vector2 pos
		{
			get
			{
				return Custom.RestrictInRect(mic.listenerPoint, rect);
			}
			set
			{
			}
		}

		public RectangularSound(VirtualMicrophone mic, SoundLoader.SoundData sData, bool loop, RectSoundEmitter controller, float volume, float pitch, bool startAtRandomTime)
			: base(mic, sData, loop, new Vector2(0f, 0f), volume, pitch, startAtRandomTime)
		{
			this.controller = controller;
		}

		public override void Update(float timeStacker, float timeSpeed)
		{
			rect = controller.rect;
			pitch = controller.pitch;
			volume = controller.volume;
			volumeGroup = controller.volumeGroup;
			if (mic.room == controller.room)
			{
				controller.lastSoundPlayingFrame = Time.frameCount;
				controller.soundStillPlaying = true;
			}
			base.Update(timeStacker, timeSpeed);
		}
	}

	public class ObjectSound : PositionedSound
	{
		public PositionedSoundEmitter controller;

		public override bool Done
		{
			get
			{
				if ((!(audioSource == null) && (audioSource.isPlaying || loadOp != null)) || (soundData.dontAutoPlay && !allowPlay))
				{
					return controller.slatedForDeletetion;
				}
				return true;
			}
		}

		public override bool PlayAgain
		{
			get
			{
				if (audioSource != null && loop)
				{
					return !controller.slatedForDeletetion;
				}
				return false;
			}
		}

		public ObjectSound(VirtualMicrophone mic, SoundLoader.SoundData sData, bool loop, PositionedSoundEmitter controller, float volume, float pitch, bool startAtRandomTime)
			: base(mic, sData, loop, controller.pos, volume, pitch, startAtRandomTime)
		{
			this.controller = controller;
		}

		public override void Update(float timeStacker, float timeSpeed)
		{
			pos = controller.pos;
			lastPos = controller.lastPos;
			pitch = controller.pitch;
			volume = controller.volume;
			volumeGroup = controller.volumeGroup;
			if (mic.room == controller.room)
			{
				controller.lastSoundPlayingFrame = Time.frameCount;
				controller.soundStillPlaying = true;
				controller.currentSoundObject = this;
			}
			base.Update(timeStacker, timeSpeed);
		}
	}

	public class DisembodiedLoop : SoundObject
	{
		public DisembodiedLoopEmitter controller;

		public override bool Done
		{
			get
			{
				if ((!(audioSource == null) && (audioSource.isPlaying || loadOp != null)) || (soundData.dontAutoPlay && !allowPlay))
				{
					return controller.slatedForDeletetion;
				}
				return true;
			}
		}

		public override bool PlayAgain
		{
			get
			{
				if (audioSource != null && loop)
				{
					return !controller.slatedForDeletetion;
				}
				return false;
			}
		}

		public DisembodiedLoop(VirtualMicrophone mic, SoundLoader.SoundData sData, DisembodiedLoopEmitter controller, float pan, float volume, float pitch, bool startAtRandomTime)
			: base(mic, sData, loop: true, pan, volume, pitch, startAtRandomTime)
		{
			this.controller = controller;
		}

		public override void Update(float timeStacker, float timeSpeed)
		{
			base.SetPitch = controller.pitch;
			base.SetVolume = controller.volume;
			audioSource.panStereo = controller.pan;
			volumeGroup = controller.volumeGroup;
			if (mic.room == controller.room)
			{
				controller.lastSoundPlayingFrame = Time.frameCount;
				controller.soundStillPlaying = true;
			}
			base.Update(timeStacker, timeSpeed);
		}
	}

	public RoomCamera camera;

	public SoundLoader soundLoader;

	public List<SoundObject> soundObjects;

	private Vector2 listenerPoint;

	private Vector2 lastListenerPoint;

	private float listenerRad;

	private bool visualize;

	private FSprite visualization;

	private FSprite visualization2;

	private FLabel samplesText;

	private FLabel samplesText2;

	private List<string> log;

	private bool lastI;

	private bool lastU;

	public float dopplerBlock;

	public List<AmbientSoundPlayer> ambientSoundPlayers;

	public float[] volumeGroups;

	public float inWorldSoundsVolume;

	public float deafContribution;

	private float deaf;

	public float underWater;

	public float globalSoundMuffle;

	public Room room => camera.room;

	public float InWorldSoundsVolumeGoal
	{
		get
		{
			if (camera.game.arenaOverlay != null)
			{
				return 0.33f;
			}
			return 1f;
		}
	}

	public VirtualMicrophone(RoomCamera camera)
	{
		this.camera = camera;
		soundLoader = camera.game.soundLoader;
		soundObjects = new List<SoundObject>();
		AudioListener.volume = soundLoader.volume;
		volumeGroups = new float[5];
		for (int i = 0; i < volumeGroups.Length; i++)
		{
			volumeGroups[i] = 1f;
		}
		ambientSoundPlayers = new List<AmbientSoundPlayer>();
	}

	public void AllQuiet()
	{
		for (int i = 0; i < soundObjects.Count; i++)
		{
			soundObjects[i].Destroy();
		}
		for (int j = 0; j < ambientSoundPlayers.Count; j++)
		{
			ambientSoundPlayers[j].Destroy();
		}
	}

	public void NewRoom(Room room)
	{
		if (ModManager.MMF)
		{
			listenerPoint = new Vector2(-999999f, -999999f);
		}
		List<AmbientSoundPlayer> list = new List<AmbientSoundPlayer>();
		for (int num = ambientSoundPlayers.Count - 1; num >= 0; num--)
		{
			bool flag = false;
			for (int i = 0; i < room.roomSettings.parent.ambientSounds.Count; i++)
			{
				if (ambientSoundCloseEnoughCheck(ambientSoundPlayers[num].aSound, room.roomSettings.parent.ambientSounds[i]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < room.roomSettings.ambientSounds.Count; j++)
				{
					if (ambientSoundCloseEnoughCheck(ambientSoundPlayers[num].aSound, room.roomSettings.ambientSounds[j]))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				list.Add(ambientSoundPlayers[num]);
			}
			else
			{
				ambientSoundPlayers[num].Destroy();
				ambientSoundPlayers.RemoveAt(num);
			}
		}
		for (int k = 0; k < room.roomSettings.ambientSounds.Count; k++)
		{
			bool flag2 = true;
			for (int l = 0; l < list.Count; l++)
			{
				if (ambientSoundCloseEnoughCheck(room.roomSettings.ambientSounds[k], list[l].aSound))
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				ambientSoundPlayers.Add(new AmbientSoundPlayer(this, room.roomSettings.ambientSounds[k]));
			}
		}
	}

	public void Update()
	{
		deafContribution = 0f;
		if (camera.followAbstractCreature != null && camera.followAbstractCreature.realizedCreature != null)
		{
			if (room.game.IsArenaSession && camera.followAbstractCreature.realizedCreature is Player)
			{
				float num = 0f;
				for (int i = 0; i < room.game.Players.Count; i++)
				{
					if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.Deaf > num)
					{
						num = room.game.Players[i].realizedCreature.Deaf;
					}
				}
				deafContribution = num;
			}
			else
			{
				deafContribution = camera.followAbstractCreature.realizedCreature.Deaf;
			}
		}
		deaf = Custom.LerpAndTick(deaf, Mathf.Pow(deafContribution, 1.2f), 0.06f, 1f / 30f);
		inWorldSoundsVolume = Custom.LerpAndTick(inWorldSoundsVolume, InWorldSoundsVolumeGoal, 0.03f, 1f / 60f);
		if (room.roomSettings.DangerType == RoomRain.DangerType.None || (ModManager.MSC && room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard))
		{
			volumeGroups[0] = 1f;
		}
		else if (room.roomRain != null)
		{
			if (ModManager.MSC && room.world.rainCycle.preTimer >= 0)
			{
				volumeGroups[0] = Mathf.Lerp(1f, 0.9f, room.roomRain.intensity);
			}
			else
			{
				volumeGroups[0] = Mathf.InverseLerp(0.6f, 0f, room.roomRain.intensity);
			}
		}
		else
		{
			volumeGroups[0] = 1f;
		}
		volumeGroups[0] *= inWorldSoundsVolume * (1f - deaf) * (1f - room.game.manager.fadeToBlack) * (1f - globalSoundMuffle);
		if (!room.world.singleRoomWorld && room.world.region.name == "SS" && room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f)
		{
			volumeGroups[2] = Mathf.Lerp(1f, 0.05f, Mathf.Pow(room.gravity, 0.5f));
		}
		else
		{
			volumeGroups[2] = 1f;
		}
		volumeGroups[2] *= inWorldSoundsVolume * (1f - deaf) * (1f - room.game.manager.fadeToBlack) * (1f - globalSoundMuffle);
		volumeGroups[3] = inWorldSoundsVolume * (1f - deaf) * (1f - room.game.manager.fadeToBlack) * (1f - globalSoundMuffle);
		lastListenerPoint = listenerPoint;
		listenerPoint = camera.pos + new Vector2(512f, 384f);
		if (camera.followAbstractCreature != null && camera.followAbstractCreature.realizedCreature != null)
		{
			if (camera.followAbstractCreature.realizedCreature.inShortcut)
			{
				Vector2? vector = camera.game.shortcuts.OnScreenPositionOfInShortCutCreature(room, camera.followAbstractCreature.realizedCreature);
				if (vector.HasValue)
				{
					listenerPoint = vector.Value;
				}
			}
			else
			{
				listenerPoint = camera.followAbstractCreature.realizedCreature.mainBodyChunk.pos;
			}
		}
		if (camera.followAbstractCreature != null)
		{
			if (ModManager.MSC && room.waterInverted)
			{
				underWater = 1f - Mathf.InverseLerp(room.FloatWaterLevel(listenerPoint.x) + 30f, room.FloatWaterLevel(listenerPoint.x) - 30f, listenerPoint.y);
			}
			else
			{
				underWater = Mathf.InverseLerp(room.FloatWaterLevel(listenerPoint.x) + 30f, room.FloatWaterLevel(listenerPoint.x) - 30f, listenerPoint.y);
			}
			underWater = Mathf.Lerp(underWater, Mathf.Max(underWater, 0.9f), camera.ghostMode);
			if (camera.followAbstractCreature.realizedCreature != null && camera.followAbstractCreature.realizedCreature.inShortcut)
			{
				dopplerBlock = Mathf.Min(1f, dopplerBlock + 0.05f);
			}
			else
			{
				dopplerBlock = Mathf.Max(0f, dopplerBlock - 0.05f);
			}
		}
		else
		{
			underWater = 0f;
			dopplerBlock = 0.5f;
		}
		if (camera.game.devToolsActive)
		{
			if (visualize)
			{
				if (!lastU && Input.GetKey("u"))
				{
					soundLoader.LoadSounds();
					Log("RELOADING SAMPLES");
					LogLoaderErrors();
				}
				lastU = Input.GetKey("u");
			}
			if (!lastI && Input.GetKey("i"))
			{
				visualize = !visualize;
			}
			lastI = Input.GetKey("i");
		}
		else
		{
			visualize = false;
		}
	}

	public void DrawUpdate(float timeStacker, float timeSpeed)
	{
		Vector2 currentListenerPos = Vector2.Lerp(lastListenerPoint, listenerPoint, timeStacker);
		listenerRad = 1000f;
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
				soundObjects[num].Update(timeStacker, timeSpeed);
			}
		}
		for (int num2 = ambientSoundPlayers.Count - 1; num2 >= 0; num2--)
		{
			if (ambientSoundPlayers[num2].slatedForDeletion)
			{
				ambientSoundPlayers[num2].Destroy();
				ambientSoundPlayers.RemoveAt(num2);
			}
			else
			{
				ambientSoundPlayers[num2].DrawUpdate(timeStacker, timeSpeed, currentListenerPos);
			}
		}
		if (visualize)
		{
			if (visualization == null)
			{
				InitVisualization();
			}
			visualization.x = listenerPoint.x - camera.pos.x;
			visualization.y = listenerPoint.y - camera.pos.y;
			visualization.scale = listenerRad / 100f;
		}
		else if (!visualize && visualization != null)
		{
			visualization.RemoveFromContainer();
			visualization = null;
			visualization2.RemoveFromContainer();
			visualization2 = null;
			samplesText.RemoveFromContainer();
			samplesText = null;
			samplesText2.RemoveFromContainer();
			samplesText2 = null;
			log = null;
		}
	}

	public float PanFromPoint(Vector2 ps, float timeStacker)
	{
		return -1f + 2f * Mathf.InverseLerp(Mathf.Lerp(lastListenerPoint.x, listenerPoint.x, timeStacker) - listenerRad, Mathf.Lerp(lastListenerPoint.x, listenerPoint.x, timeStacker) + listenerRad, ps.x);
	}

	public float VolFromPoint(Vector2 ps, float timeStacker, float rangeFac)
	{
		return Mathf.InverseLerp(listenerRad * 2f, listenerRad / 2f, (Vector2.Distance(ps, Vector2.Lerp(lastListenerPoint, listenerPoint, timeStacker)) + camera.DistanceFromViewedScreen(ps)) / rangeFac);
	}

	private void InitVisualization()
	{
		visualization = new FSprite("Circle20");
		Futile.stage.AddChild(visualization);
		visualization.alpha = 0.2f;
		visualization.color = new Color(1f, 0f, 1f);
		visualization2 = new FSprite("Circle20");
		Futile.stage.AddChild(visualization2);
		visualization2.alpha = 0.2f;
		visualization2.color = new Color(0f, 0f, 1f);
		log = new List<string>();
		samplesText = new FLabel(Custom.GetFont(), "");
		samplesText.color = new Color(1f, 1f, 0f);
		samplesText.alignment = FLabelAlignment.Left;
		samplesText.x = 100.01f;
		samplesText.y = 668.02f;
		samplesText2 = new FLabel(Custom.GetFont(), "");
		samplesText2.color = new Color(0f, 0f, 0f);
		samplesText2.alignment = FLabelAlignment.Left;
		samplesText2.x = 101.01f;
		samplesText2.y = 667.02f;
		Futile.stage.AddChild(samplesText2);
		Futile.stage.AddChild(samplesText);
	}

	private void LogLoaderErrors()
	{
		for (int i = 0; i < soundLoader.errors.Count; i++)
		{
			Log(soundLoader.errors[i]);
		}
	}

	private void Log(SoundID soundId)
	{
		if (soundLoader.assetBundlesLoaded && !(soundId == null) && soundId.Index != -1 && (!soundLoader.workingTriggers[soundId.Index] || !soundLoader.DontLog(soundId)))
		{
			Log((soundLoader.workingTriggers[soundId.Index] ? "" : "--") + soundId.ToString() + (soundLoader.workingTriggers[soundId.Index] ? (" " + soundLoader.TriggerGroupVolume(soundId)) : " - UNDEFINED"));
		}
	}

	public void Log(string strng)
	{
		if (log != null)
		{
			if (log.Count > 10)
			{
				log.RemoveAt(0);
			}
			log.Add(strng);
			samplesText.text = "";
			for (int i = 0; i < log.Count; i++)
			{
				samplesText.text = samplesText.text + "\r\n" + log[i];
			}
			samplesText2.text = samplesText.text;
		}
	}

	public void PlaySound(SoundID soundId, float pan, float vol, float pitch)
	{
		if (visualize)
		{
			Log(soundId);
		}
		if (!AllowSound(soundId))
		{
			return;
		}
		if (!soundLoader.TriggerPlayAll(soundId))
		{
			SoundLoader.SoundData soundData = GetSoundData(soundId, -1);
			if (SoundClipReady(soundData))
			{
				soundObjects.Add(new DisembodiedSound(this, soundData, pan, vol, pitch, startAtRandomTime: false, 0));
			}
			return;
		}
		for (int i = 0; i < soundLoader.TriggerSamples(soundId); i++)
		{
			SoundLoader.SoundData soundData2 = GetSoundData(soundId, i);
			if (SoundClipReady(soundData2))
			{
				soundObjects.Add(new DisembodiedSound(this, soundData2, pan, vol, pitch, startAtRandomTime: false, 0));
			}
		}
	}

	public void PlaySound(SoundID soundId, float pan, float vol, float pitch, int volumeGroup)
	{
		if (visualize)
		{
			Log(soundId);
		}
		if (!AllowSound(soundId))
		{
			return;
		}
		if (!soundLoader.TriggerPlayAll(soundId))
		{
			SoundLoader.SoundData soundData = GetSoundData(soundId, -1);
			if (SoundClipReady(soundData))
			{
				soundObjects.Add(new DisembodiedSound(this, soundData, pan, vol, pitch, startAtRandomTime: false, volumeGroup));
			}
			return;
		}
		for (int i = 0; i < soundLoader.TriggerSamples(soundId); i++)
		{
			SoundLoader.SoundData soundData2 = GetSoundData(soundId, i);
			if (SoundClipReady(soundData2))
			{
				soundObjects.Add(new DisembodiedSound(this, soundData2, pan, vol, pitch, startAtRandomTime: false, volumeGroup));
			}
		}
	}

	public void PlaySound(SoundID soundId, Vector2 pos, float vol, float pitch)
	{
		if (visualize)
		{
			Log(soundId);
		}
		if (!AllowSound(soundId))
		{
			return;
		}
		if (!soundLoader.TriggerPlayAll(soundId))
		{
			SoundLoader.SoundData soundData = GetSoundData(soundId, -1);
			if (SoundClipReady(soundData))
			{
				soundObjects.Add(new StaticPositionSound(this, soundData, pos, vol, pitch, startAtRandomTime: false));
			}
			return;
		}
		for (int i = 0; i < soundLoader.TriggerSamples(soundId); i++)
		{
			SoundLoader.SoundData soundData2 = GetSoundData(soundId, i);
			if (SoundClipReady(soundData2))
			{
				soundObjects.Add(new StaticPositionSound(this, soundData2, pos, vol, pitch, startAtRandomTime: false));
			}
		}
	}

	public void PlaySound(SoundID soundId, PositionedSoundEmitter controller, bool loop, float vol, float pitch, bool randomStartPosition)
	{
		if (visualize)
		{
			Log(soundId);
		}
		if (!AllowSound(soundId))
		{
			return;
		}
		if (!soundLoader.TriggerPlayAll(soundId))
		{
			SoundLoader.SoundData soundData = GetSoundData(soundId, -1);
			if (SoundClipReady(soundData))
			{
				if (controller is RectSoundEmitter)
				{
					soundObjects.Add(new RectangularSound(this, soundData, loop, controller as RectSoundEmitter, vol, pitch, randomStartPosition));
				}
				else
				{
					soundObjects.Add(new ObjectSound(this, soundData, loop, controller, vol, pitch, randomStartPosition));
				}
			}
			return;
		}
		for (int i = 0; i < soundLoader.TriggerSamples(soundId); i++)
		{
			SoundLoader.SoundData soundData2 = GetSoundData(soundId, i);
			if (SoundClipReady(soundData2))
			{
				if (controller is RectSoundEmitter)
				{
					soundObjects.Add(new RectangularSound(this, soundData2, loop, controller as RectSoundEmitter, vol, pitch, randomStartPosition));
				}
				else
				{
					soundObjects.Add(new ObjectSound(this, soundData2, loop, controller, vol, pitch, randomStartPosition));
				}
			}
		}
	}

	public void PlayDisembodiedLoop(SoundID soundId, DisembodiedLoopEmitter emitter, float pan, float vol, float pitch)
	{
		if (visualize)
		{
			Log(soundId);
		}
		if (!AllowSound(soundId))
		{
			return;
		}
		if (!soundLoader.TriggerPlayAll(soundId))
		{
			SoundLoader.SoundData soundData = GetSoundData(soundId, -1);
			if (SoundClipReady(soundData))
			{
				soundObjects.Add(new DisembodiedLoop(this, soundData, emitter, pan, vol, pitch, startAtRandomTime: false));
			}
			return;
		}
		for (int i = 0; i < soundLoader.TriggerSamples(soundId); i++)
		{
			SoundLoader.SoundData soundData2 = GetSoundData(soundId, i);
			if (SoundClipReady(soundData2))
			{
				soundObjects.Add(new DisembodiedLoop(this, soundData2, emitter, pan, vol, pitch, startAtRandomTime: false));
			}
		}
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

	private bool AllowSound(SoundID soundId)
	{
		if (!soundLoader.ShouldSoundPlay(soundId))
		{
			return false;
		}
		if (soundObjects.Count > 25)
		{
			int num = 0;
			for (int i = 0; i < soundObjects.Count; i++)
			{
				if (soundObjects[i].soundData.soundID == soundId)
				{
					num++;
					if (num > Math.Max(0, 30 - soundObjects.Count))
					{
						Log("<CAP> " + soundId);
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool ambientSoundCloseEnoughCheck(AmbientSound ambientSoundA, AmbientSound ambientSoundB)
	{
		bool result = true;
		if (ambientSoundA.type != AmbientSound.Type.Omnidirectional || ambientSoundB.type != AmbientSound.Type.Omnidirectional)
		{
			result = false;
		}
		if (ambientSoundA.sample != ambientSoundB.sample)
		{
			result = false;
		}
		if (Mathf.Abs(ambientSoundA.volume - ambientSoundB.volume) > 0.01f)
		{
			result = false;
		}
		if (Mathf.Abs(ambientSoundA.pitch - ambientSoundB.pitch) > 0.01f)
		{
			result = false;
		}
		return result;
	}

	public void PlayCustomSound(string soundName, Vector2 pos, float vol, float pitch)
	{
		SoundLoader.SoundData soundData = GetSoundData(SoundID.Slugcat_Stash_Spear_On_Back, -1);
		soundData.dontAutoPlay = true;
		soundData.soundName = soundName;
		StaticPositionSound staticPositionSound = new StaticPositionSound(this, soundData, pos, vol, pitch, startAtRandomTime: false);
		staticPositionSound.singleUseSound = true;
		_ = Application.dataPath;
		string text = AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + soundName + ".wav");
		staticPositionSound.audioSource.clip = AssetManager.SafeWWWAudioClip("file://" + text, threeD: false, stream: true, AudioType.WAV);
		soundObjects.Add(staticPositionSound);
	}

	public void PlayCustomSoundDisembodied(string soundName, float pan, float vol, float pitch)
	{
		SoundLoader.SoundData soundData = GetSoundData(SoundID.Slugcat_Stash_Spear_On_Back, -1);
		soundData.dontAutoPlay = true;
		soundData.soundName = soundName;
		DisembodiedSound disembodiedSound = new DisembodiedSound(this, soundData, pan, vol, pitch, startAtRandomTime: false, 0);
		disembodiedSound.singleUseSound = true;
		_ = Application.dataPath;
		string text = AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + soundName + ".wav");
		disembodiedSound.audioSource.clip = AssetManager.SafeWWWAudioClip("file://" + text, threeD: false, stream: true, AudioType.WAV);
		soundObjects.Add(disembodiedSound);
	}

	public void PlayCustomDisembodiedLoop(string soundName, DisembodiedLoopEmitter emitter, float pan, float vol, float pitch)
	{
		SoundLoader.SoundData soundData = GetSoundData(SoundID.Slugcat_Stash_Spear_On_Back, -1);
		soundData.dontAutoPlay = true;
		soundData.soundName = soundName;
		DisembodiedLoop disembodiedLoop = new DisembodiedLoop(this, soundData, emitter, pan, vol, pitch, startAtRandomTime: false);
		disembodiedLoop.singleUseSound = true;
		_ = Application.dataPath;
		string text = AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + soundName + ".wav");
		disembodiedLoop.audioSource.clip = AssetManager.SafeWWWAudioClip("file://" + text, threeD: false, stream: true, AudioType.WAV);
		soundObjects.Add(disembodiedLoop);
	}

	public void PlayCustomPositionedSound(string soundName, PositionedSoundEmitter controller, float vol, float pitch, bool loop, bool randomPosition)
	{
		SoundLoader.SoundData soundData = GetSoundData(SoundID.Slugcat_Stash_Spear_On_Back, -1);
		soundData.dontAutoPlay = true;
		soundData.soundName = soundName;
		PositionedSound positionedSound = null;
		positionedSound = ((!(controller is RectSoundEmitter)) ? ((PositionedSound)new ObjectSound(this, soundData, loop, controller, vol, pitch, randomPosition)) : ((PositionedSound)new RectangularSound(this, soundData, loop, controller as RectSoundEmitter, vol, pitch, randomPosition)));
		positionedSound.singleUseSound = true;
		_ = Application.dataPath;
		string text = AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + soundName + ".wav");
		positionedSound.audioSource.clip = AssetManager.SafeWWWAudioClip("file://" + text, threeD: false, stream: true, AudioType.WAV);
		soundObjects.Add(positionedSound);
	}
}
