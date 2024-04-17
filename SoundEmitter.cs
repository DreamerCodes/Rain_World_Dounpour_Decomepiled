using UnityEngine;

public class SoundEmitter : UpdatableAndDeletable
{
	public bool alive;

	public bool requireActiveUpkeep;

	public bool soundStillPlaying;

	public int lastSoundPlayingFrame;

	public VirtualMicrophone.SoundObject currentSoundObject;

	public float volume;

	public float pitch;

	public int volumeGroup;

	public SoundEmitter(float volume, float pitch)
	{
		this.volume = volume;
		this.pitch = pitch;
		alive = true;
		soundStillPlaying = true;
		lastSoundPlayingFrame = Time.frameCount;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (currentSoundObject != null && currentSoundObject.slatedForDeletion)
		{
			currentSoundObject = null;
		}
		if (alive)
		{
			if (requireActiveUpkeep)
			{
				alive = false;
			}
		}
		else
		{
			Destroy();
		}
		if (soundStillPlaying)
		{
			if (lastSoundPlayingFrame < Time.frameCount - 1)
			{
				soundStillPlaying = false;
			}
		}
		else
		{
			Destroy();
		}
	}

	public override void Destroy()
	{
		base.Destroy();
	}
}
