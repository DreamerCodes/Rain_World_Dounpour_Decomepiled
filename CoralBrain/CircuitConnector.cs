using RWCustom;
using UnityEngine;

namespace CoralBrain;

public class CircuitConnector : CosmeticSprite, IOwnMycelia
{
	private Vector2 cPos;

	private CoralCircuit.CircuitBit bit;

	public Mycelium[] connections;

	public CoralCircuit.CircuitBit[] legBits;

	public Room OwnerRoom => room;

	public CircuitConnector(CoralCircuit.CircuitBit bit, Room room)
	{
		this.bit = bit;
		cPos = new Vector2(Random.value * bit.size.x, Random.value * bit.size.y) - bit.size * 0.5f;
		connections = new Mycelium[Random.Range(5, 10)];
		for (int i = 0; i < connections.Length; i++)
		{
			connections[i] = new Mycelium(bit.circuit.system, this, i, 30f, OnCircuitPos(cPos, bit, 1f));
			connections[i].color = new Color(0f, 0f, 0.1f);
			connections[i].useStaticCulling = false;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < connections.Length; i++)
		{
			connections[i].Update();
			connections[i].points[1, 2] += Custom.DegToVec((float)i / (float)connections.Length * 360f);
		}
	}

	public Vector2 OnCircuitPos(Vector2 relPos, CoralCircuit.CircuitBit bitInQuestion, float timeStacker)
	{
		relPos.x *= Vector3.Slerp(bitInQuestion.lastDRotat, bitInQuestion.dRotat, timeStacker).y;
		relPos = Custom.RotateAroundOrigo(relPos, Custom.VecToDeg(Vector3.Slerp(bitInQuestion.lastRotat, bitInQuestion.rotat, timeStacker)));
		relPos += Vector2.Lerp(bitInQuestion.lastPos, bitInQuestion.pos, timeStacker);
		return relPos;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[4 + connections.Length];
		for (int i = 0; i < connections.Length; i++)
		{
			connections[i].InitiateSprites(i, sLeaser, rCam);
		}
		sLeaser.sprites[connections.Length] = new FSprite("Circle20");
		sLeaser.sprites[connections.Length].color = Custom.HSL2RGB(0.9638889f, 1f, 0.1f);
		sLeaser.sprites[connections.Length].scale = 0.8f;
		sLeaser.sprites[connections.Length + 1] = new FSprite("Circle20");
		sLeaser.sprites[connections.Length + 1].color = Custom.HSL2RGB(0.9638889f, 1f, 0.25f);
		sLeaser.sprites[connections.Length + 1].scale = 0.4f;
		sLeaser.sprites[connections.Length + 2] = new FSprite("Circle20");
		sLeaser.sprites[connections.Length + 2].color = Custom.HSL2RGB(0.9638889f, 1f, 0.1f);
		sLeaser.sprites[connections.Length + 2].scale = 0.3f;
		sLeaser.sprites[connections.Length + 3] = new FSprite("Circle20");
		sLeaser.sprites[connections.Length + 3].scale = 0.15f;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = OnCircuitPos(cPos, bit, timeStacker);
		float y = Vector3.Slerp(bit.lastDRotat, bit.dRotat, timeStacker).y;
		y = Mathf.Lerp(0.2f, 1f, Mathf.Abs(y)) * Mathf.Sign(y);
		float rotation = Custom.VecToDeg(Vector3.Slerp(bit.lastRotat, bit.rotat, timeStacker));
		sLeaser.sprites[connections.Length].x = vector.x - camPos.x;
		sLeaser.sprites[connections.Length].y = vector.y - camPos.y;
		sLeaser.sprites[connections.Length].rotation = rotation;
		sLeaser.sprites[connections.Length].scaleX = 0.8f * y;
		sLeaser.sprites[connections.Length + 1].x = vector.x - camPos.x - 1f - 2f * Mathf.Abs(y);
		sLeaser.sprites[connections.Length + 1].y = vector.y - camPos.y + 1f + 2f * Mathf.Abs(y);
		sLeaser.sprites[connections.Length + 1].rotation = rotation;
		sLeaser.sprites[connections.Length + 1].scaleX = 0.4f * y;
		sLeaser.sprites[connections.Length + 2].x = vector.x - camPos.x - 1f;
		sLeaser.sprites[connections.Length + 2].y = vector.y - camPos.y + 1f;
		sLeaser.sprites[connections.Length + 2].rotation = rotation;
		sLeaser.sprites[connections.Length + 2].scaleX = 0.3f * y;
		sLeaser.sprites[connections.Length + 3].x = vector.x - camPos.x - 1f;
		sLeaser.sprites[connections.Length + 3].y = vector.y - camPos.y + 1f;
		sLeaser.sprites[connections.Length + 3].rotation = rotation;
		sLeaser.sprites[connections.Length + 3].scaleX = 0.15f * y;
		sLeaser.sprites[connections.Length + 3].color = new Color(0f, 0f, Random.value);
		for (int i = 0; i < connections.Length; i++)
		{
			connections[i].DrawSprites(i, sLeaser, rCam, timeStacker, camPos);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public Vector2 ConnectionPos(int index, float timeStacker)
	{
		return OnCircuitPos(cPos, bit, timeStacker);
	}

	public Vector2 ResetDir(int index)
	{
		return default(Vector2);
	}
}
