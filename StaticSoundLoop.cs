using UnityEngine;

public class StaticSoundLoop
{
	public Vector2 pos;

	private SoundID soundID;

	public PositionedSoundEmitter emitter;

	public float volume;

	public float pitch;

	private Room room;

	public StaticSoundLoop(SoundID soundID, Vector2 pos, Room room, float volume, float pitch)
	{
		this.soundID = soundID;
		this.pos = pos;
		this.room = room;
		this.volume = volume;
		this.pitch = pitch;
	}

	public void Update()
	{
		if (emitter != null && emitter.slatedForDeletetion)
		{
			emitter = null;
		}
		if (emitter != null)
		{
			if (volume == 0f)
			{
				emitter.alive = false;
				emitter = null;
				return;
			}
			emitter.lastPos = emitter.pos;
			emitter.pos = pos;
			emitter.alive = true;
			emitter.volume = volume;
			emitter.pitch = pitch;
		}
		else if (volume > 0f && room.game.manager.soundLoader.assetBundlesLoaded && soundID != null && soundID.Index != -1 && room.game.manager.soundLoader.workingTriggers[soundID.Index])
		{
			emitter = new PositionedSoundEmitter(pos, volume, pitch);
			emitter.requireActiveUpkeep = true;
			room.PlaySound(soundID, emitter, loop: true, volume, pitch, randomStartPosition: false);
		}
	}
}
