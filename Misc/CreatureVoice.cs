using UnityEngine;

public abstract class CreatureVoice
{
	private class IntensityVisualizer
	{
		private CreatureVoice voice;

		private float[] samples;

		public float savedIntensity;

		private float lowerCutoff;

		private float upperCeiling;

		private float exponent;

		public IntensityVisualizer(CreatureVoice voice, float lowerCutoff, float upperCeiling, float exponent)
		{
			this.voice = voice;
			this.lowerCutoff = lowerCutoff;
			this.upperCeiling = upperCeiling;
			this.exponent = exponent;
			samples = new float[64];
		}

		public void Update()
		{
			if (voice.soundA == null || voice.soundA.currentSoundObject == null || !voice.soundA.currentSoundObject.IsPlaying)
			{
				savedIntensity = 0f;
			}
			else if (voice.soundA.currentSoundObject.IsLoaded)
			{
				voice.soundA.currentSoundObject.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
				float num = 0f;
				for (int i = 0; i < samples.Length; i++)
				{
					num += samples[i];
				}
				num /= (float)samples.Length;
				savedIntensity = Mathf.Pow(Mathf.InverseLerp(lowerCutoff, upperCeiling, num), exponent);
			}
		}
	}

	public Creature creature;

	public BodyChunk voiceChunk;

	public Room room;

	public ChunkSoundEmitter soundA;

	public SoundID soundID = SoundID.None;

	private SoundID lastSoundID = SoundID.None;

	private IntensityVisualizer visualizer;

	private float vol = 1f;

	private float ptch = 1f;

	public string soundName;

	public string lastSoundName;

	public float Volume
	{
		get
		{
			return vol;
		}
		set
		{
			vol = value;
			if (soundA != null)
			{
				soundA.volume = value;
			}
		}
	}

	public float Pitch
	{
		get
		{
			return ptch;
		}
		set
		{
			ptch = value;
			if (soundA != null)
			{
				soundA.pitch = value;
			}
		}
	}

	public bool MakingASound
	{
		get
		{
			if (soundID != SoundID.None || soundName != "")
			{
				return Volume > 0f;
			}
			return false;
		}
	}

	public float VisualizedIntensity
	{
		get
		{
			if (visualizer == null)
			{
				return 0f;
			}
			return visualizer.savedIntensity * Volume;
		}
	}

	public CreatureVoice(Creature creature, int voiceChunkIndex)
	{
		this.creature = creature;
		voiceChunk = creature.bodyChunks[voiceChunkIndex];
		soundName = "";
		lastSoundName = "";
	}

	public virtual void Update()
	{
		if (creature.room != null)
		{
			room = creature.room;
		}
		if ((soundID == SoundID.None && soundName == "") || Volume == 0f || soundID != lastSoundID || soundName != lastSoundName || (soundA != null && soundA.slatedForDeletetion))
		{
			soundA = null;
		}
		lastSoundID = soundID;
		lastSoundName = soundName;
		if (soundA != null)
		{
			soundA.alive = true;
		}
		else if (soundName != "" && Volume > 0f)
		{
			soundA = room.PlayCustomChunkSound(soundName, voiceChunk, Volume, Pitch);
			soundA.requireActiveUpkeep = true;
		}
		else if (soundID != SoundID.None && Volume > 0f)
		{
			soundA = room.PlaySound(soundID, voiceChunk, loop: false, Volume, Pitch);
			soundA.requireActiveUpkeep = true;
		}
		if (visualizer != null)
		{
			visualizer.Update();
		}
	}

	public void InitSoundVisualizer(float intensityLowerCutoff, float intensityUpperCeiling, float intensityExponent)
	{
		visualizer = new IntensityVisualizer(this, intensityLowerCutoff, intensityUpperCeiling, intensityExponent);
	}
}
