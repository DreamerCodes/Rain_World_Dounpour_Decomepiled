using UnityEngine;

public abstract class DynamicSoundLoop
{
	private SoundID lastSound;

	public SoundID sound;

	public SoundEmitter emitter;

	public UpdatableAndDeletable owner;

	private float lastVol = 1f;

	protected float p = 1f;

	protected float v = 1f;

	protected int vg;

	public bool destroyClipWhenDone;

	public string soundName;

	public string lastSoundName;

	public float Pitch
	{
		get
		{
			return p;
		}
		set
		{
			p = value;
			if (emitter != null)
			{
				emitter.pitch = p;
			}
		}
	}

	public float Volume
	{
		get
		{
			return v;
		}
		set
		{
			lastVol = v;
			v = value;
			if (emitter != null)
			{
				emitter.volume = v;
			}
		}
	}

	public int VolumeGroup
	{
		get
		{
			return vg;
		}
		set
		{
			vg = value;
			if (emitter != null)
			{
				emitter.volumeGroup = vg;
			}
		}
	}

	public DynamicSoundLoop(UpdatableAndDeletable owner)
	{
		this.owner = owner;
		lastSound = SoundID.None;
		sound = SoundID.None;
		lastSoundName = "";
		soundName = "";
	}

	public void Update()
	{
		if (emitter != null && emitter.slatedForDeletetion)
		{
			if (destroyClipWhenDone && emitter.currentSoundObject.audioSource != null && emitter.currentSoundObject.audioSource.clip != null)
			{
				Object.Destroy(emitter.currentSoundObject.audioSource.clip);
				emitter.currentSoundObject.audioSource.clip = null;
			}
			emitter = null;
		}
		if (sound != lastSound || soundName != lastSoundName)
		{
			Stop();
			if ((sound != SoundID.None || soundName != "") && v > 0f && owner.room.game.manager.soundLoader.assetBundlesLoaded)
			{
				Start();
			}
			lastSound = sound;
			lastSoundName = soundName;
		}
		if (v == 0f && lastVol > 0f)
		{
			Stop();
		}
		else if (v > 0f && lastVol == 0f && (sound != SoundID.None || soundName != "") && owner.room.game.manager.soundLoader.assetBundlesLoaded)
		{
			Start();
		}
		if (emitter != null && (owner == null || owner.room == emitter.room))
		{
			emitter.alive = true;
			return;
		}
		lastSound = SoundID.None;
		lastSoundName = "";
	}

	private void Start()
	{
		if (owner != null && owner.room != null && (!(soundName == "") || owner.room.game.manager.soundLoader.workingTriggers[sound.Index]) && (!(soundName == "") || !(sound == null)) && (!(soundName == "") || sound.Index != -1))
		{
			InitSound();
			emitter.volumeGroup = VolumeGroup;
			emitter.requireActiveUpkeep = true;
		}
	}

	private void Stop()
	{
		if (emitter != null)
		{
			emitter.alive = false;
			emitter = null;
		}
	}

	protected virtual void InitSound()
	{
	}
}
