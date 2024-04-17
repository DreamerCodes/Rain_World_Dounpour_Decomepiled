using UnityEngine;

public class DrippingSound : UpdatableAndDeletable
{
	public DisembodiedDynamicSoundLoop soundLoop;

	public DrippingSound()
	{
		soundLoop = new DisembodiedDynamicSoundLoop(this);
		soundLoop.sound = SoundID.Cycle_Start_Drips;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		soundLoop.Update();
		soundLoop.Volume = Mathf.Pow(Mathf.InverseLerp(1f, 0.75f, room.world.rainCycle.CycleStartUp), 0.5f);
		if (room.world.rainCycle.CycleStartUp >= 1f)
		{
			Destroy();
		}
	}
}
