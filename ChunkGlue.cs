using UnityEngine;

public struct ChunkGlue
{
	public BodyChunk moveChunk;

	public BodyChunk otherChunk;

	public Vector2 relativePos;

	public ChunkGlue(BodyChunk moveChunk, BodyChunk otherChunk, Vector2 relativePos)
	{
		this.moveChunk = moveChunk;
		this.otherChunk = otherChunk;
		this.relativePos = relativePos;
	}
}
