public class ChunkSoundEmitter : PositionedSoundEmitter
{
	public BodyChunk chunk;

	public ChunkSoundEmitter(BodyChunk chunk, float vol, float ptch)
		: base(chunk.pos, vol, ptch)
	{
		this.chunk = chunk;
	}

	public override void Update(bool eu)
	{
		lastPos = pos;
		pos = chunk.pos;
		if (chunk.owner.room != room)
		{
			alive = false;
		}
		base.Update(eu);
	}
}
