using RWCustom;
using UnityEngine;

namespace CoralBrain;

public class CircuitCrab : CosmeticSprite
{
	private Vector2 cPos;

	private Vector2 nextCPos;

	private float prog;

	private float lastProg;

	private float moveSpeed;

	private CoralCircuit.CircuitBit bit;

	private CoralCircuit.CircuitBit nextBit;

	public CrabLeg[] legs;

	public Vector2[] legGrabPositions;

	public CoralCircuit.CircuitBit[] legBits;

	public CircuitCrab(CoralCircuit.CircuitBit bit, Room room)
	{
		this.bit = bit;
		nextBit = bit;
		cPos = new Vector2(Random.value * bit.size.x, Random.value * bit.size.y) - bit.size * 0.5f;
		nextCPos = cPos;
		prog = 1f;
		legs = new CrabLeg[Random.Range(4, 8)];
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i] = new CrabLeg(this, i, 30f, OnCircuitPos(cPos, bit, 1f));
			legs[i].color = Custom.HSL2RGB(0.025f, 0.5f, 0.85f);
		}
		legGrabPositions = new Vector2[legs.Length];
		legBits = new CoralCircuit.CircuitBit[legs.Length];
	}

	public override void Update(bool eu)
	{
		lastProg = prog;
		if (prog == 1f)
		{
			if (Random.value < 0.05f)
			{
				CoralCircuit.CircuitBit circuitBit = ((!(Random.value < 0.5f) || bit.neighbors.Count <= 0) ? bit : bit.neighbors[Random.Range(0, bit.neighbors.Count)]);
				Vector2 relPos = new Vector2(circuitBit.size.x * Random.value, circuitBit.size.y * Random.value);
				if (Vector2.Distance(OnCircuitPos(cPos, bit, 1f), OnCircuitPos(relPos, circuitBit, 1f)) < 50f)
				{
					nextCPos = relPos;
					nextBit = circuitBit;
					prog = 0f;
					lastProg = 0f;
					moveSpeed = 0.5f / Vector2.Distance(OnCircuitPos(cPos, bit, 1f), OnCircuitPos(relPos, circuitBit, 1f));
				}
			}
		}
		else
		{
			prog = Mathf.Min(prog + moveSpeed, 1f);
			if (prog == 1f)
			{
				bit = nextBit;
				cPos = nextCPos;
			}
		}
		Vector2 vector = Vector2.Lerp(OnCircuitPos(cPos, bit, 1f), OnCircuitPos(nextCPos, nextBit, 1f), prog);
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].Update();
			legs[i].points[0, 0] = vector;
			legs[i].points[0, 2] = Custom.DegToVec((float)i / (float)legs.Length * 360f);
			legs[i].points[1, 2] += legs[i].points[0, 2] * 0.1f;
			if (Random.value < 0.025f)
			{
				legGrabPositions[i] = new Vector2(nextBit.size.x * Random.value, nextBit.size.y * Random.value);
				legBits[i] = nextBit;
			}
			else if (Random.value < 1f / 60f)
			{
				legBits[i] = null;
			}
			if (legBits[i] != null)
			{
				Vector2 vector2 = OnCircuitPos(legGrabPositions[i], legBits[i], 1f);
				legs[i].points[legs[i].points.GetLength(0) - 1, 2] *= 0.8f;
				legs[i].points[legs[i].points.GetLength(0) - 1, 2] += Custom.DirVec(legs[i].points[legs[i].points.GetLength(0) - 1, 0], vector2);
				if (Custom.DistLess(legs[i].points[legs[i].points.GetLength(0) - 1, 0], vector2, 5f))
				{
					legs[i].points[legs[i].points.GetLength(0) - 1, 2] *= 0f;
					legs[i].points[legs[i].points.GetLength(0) - 1, 0] = vector2;
				}
			}
		}
		base.Update(eu);
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
		sLeaser.sprites = new FSprite[2 + legs.Length];
		sLeaser.sprites[0] = new FSprite("Circle20");
		sLeaser.sprites[0].color = Custom.HSL2RGB(0.025f, 0.5f, 0.85f);
		sLeaser.sprites[0].scale = 0.2f;
		sLeaser.sprites[1] = new FSprite("Circle20");
		sLeaser.sprites[1].color = new Color(1f, 1f, 1f);
		sLeaser.sprites[1].scale = 0.120000005f;
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].InitiateSprites(i + 2, sLeaser, rCam);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(OnCircuitPos(cPos, bit, timeStacker), OnCircuitPos(nextCPos, nextBit, timeStacker), Mathf.Lerp(lastProg, prog, timeStacker));
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[1].x = vector.x - camPos.x - 1f;
		sLeaser.sprites[1].y = vector.y - camPos.y + 1f;
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].DrawSprites(i + 2, sLeaser, rCam, timeStacker, camPos);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public Vector2 ConnectionPos(int index, float timeStacker)
	{
		return Vector2.Lerp(OnCircuitPos(cPos, bit, timeStacker), OnCircuitPos(nextCPos, nextBit, timeStacker), Mathf.Lerp(lastProg, prog, timeStacker));
	}
}
