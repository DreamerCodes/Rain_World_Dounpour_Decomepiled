public class DisembodiedDynamicSoundLoop : DynamicSoundLoop
{
	protected float pn;

	public float Pan
	{
		get
		{
			return pn;
		}
		set
		{
			pn = value;
			if (emitter != null)
			{
				(emitter as DisembodiedLoopEmitter).pan = pn;
			}
		}
	}

	public DisembodiedDynamicSoundLoop(UpdatableAndDeletable owner)
		: base(owner)
	{
	}

	protected override void InitSound()
	{
		if (soundName != "")
		{
			destroyClipWhenDone = true;
			emitter = owner.room.PlayCustomDisembodiedLoop(soundName, v, p, pn);
		}
		else
		{
			emitter = owner.room.PlayDisembodiedLoop(sound, v, p, pn);
		}
	}
}
