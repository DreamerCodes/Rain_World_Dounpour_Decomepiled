public class ChunkDynamicSoundLoop : DynamicSoundLoop
{
	public BodyChunk chunk;

	public ChunkDynamicSoundLoop(BodyChunk chunk)
		: base(chunk.owner)
	{
		this.chunk = chunk;
	}

	protected override void InitSound()
	{
		emitter = owner.room.PlaySound(sound, chunk, loop: true, v, p);
	}
}
