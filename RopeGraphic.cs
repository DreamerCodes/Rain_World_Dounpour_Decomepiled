using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class RopeGraphic
{
	public class Segment
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public int index;

		public int bendIndex;

		public RopeGraphic owner;

		public bool claimedForBend => bendIndex > -1;

		public Segment(int index, RopeGraphic owner)
		{
			this.index = index;
			this.owner = owner;
		}

		public void Update()
		{
			if (claimedForBend)
			{
				return;
			}
			lastPos = pos;
			pos += vel;
			vel *= owner.airFricition;
			int num = index;
			int num2 = index;
			while (num > 0)
			{
				num--;
				if (owner.segments[num].claimedForBend)
				{
					break;
				}
			}
			while (num2 < owner.segments.Length - 1)
			{
				num2++;
				if (owner.segments[num2].claimedForBend)
				{
					break;
				}
			}
			Vector2 vector = owner.segments[num].pos;
			Vector2 vector2 = owner.segments[num2].pos;
			float num3 = Mathf.InverseLerp(num, num2, index);
			if (owner.segments[num].bendIndex > 0)
			{
				vector += (Custom.DirVec(owner.positionsList[owner.segments[num].bendIndex - 1], vector) + Custom.DirVec(owner.segments[num].pos, owner.segments[num2].pos)).normalized * Vector2.Distance(owner.segments[num].pos, owner.segments[num2].pos) * (1f / 3f);
			}
			if (owner.segments[num2].bendIndex < owner.positionsListCount - 1)
			{
				vector2 += (Custom.DirVec(owner.positionsList[owner.segments[num2].bendIndex + 1], vector2) + Custom.DirVec(owner.segments[num2].pos, owner.segments[num].pos)).normalized * Vector2.Distance(owner.segments[num].pos, owner.segments[num2].pos) * (1f / 3f);
			}
			owner.MoveSegment(index, Vector2.Lerp(owner.segments[num].pos, owner.segments[num2].pos, num3), Custom.Bezier(owner.segments[num].pos, vector, owner.segments[num2].pos, vector2, num3));
		}
	}

	public Segment[] segments;

	public float airFricition;

	public List<Vector2> positionsList = new List<Vector2>();

	public int positionsListCount;

	public RopeGraphic(int segs)
	{
		segments = new Segment[segs];
		for (int i = 0; i < segs; i++)
		{
			segments[i] = new Segment(i, this);
		}
	}

	public virtual void Reset(Vector2 ps)
	{
		for (int i = 0; i < segments.Length; i++)
		{
			segments[i].pos = ps;
			segments[i].lastPos = ps;
			segments[i].vel *= 0f;
		}
	}

	public virtual void Update()
	{
	}

	public void AlignAndConnect(int listCount)
	{
		positionsListCount = listCount;
		float num = 0f;
		for (int i = 1; i < positionsListCount; i++)
		{
			num += Vector2.Distance(positionsList[i - 1], positionsList[i]);
		}
		float num2 = 0f;
		for (int j = 0; j < positionsListCount; j++)
		{
			if (j > 0)
			{
				num2 += Vector2.Distance(positionsList[j - 1], positionsList[j]);
			}
			if (j == 0 || positionsList[j - 1] != positionsList[j])
			{
				AlignRope(num2 / num, j);
			}
		}
		for (int k = 0; k < segments.Length; k++)
		{
			segments[k].Update();
		}
		ConnectPhase(num);
		for (int l = 0; l < segments.Length; l++)
		{
			segments[l].bendIndex = -1;
		}
	}

	public void AddToPositionsList(int index, Vector2 pos)
	{
		while (positionsList.Count < index + 1)
		{
			positionsList.Add(Vector2.zero);
		}
		positionsList[index] = pos;
	}

	public virtual void ConnectPhase(float totalRopeLength)
	{
	}

	protected void ConnectRopeSegments(int A, int B, float idealDist, float elastic)
	{
		Vector2 vector = Custom.DirVec(segments[A].pos, segments[B].pos);
		float num = Vector2.Distance(segments[A].pos, segments[B].pos);
		if (!segments[A].claimedForBend)
		{
			segments[A].pos += vector * (num - idealDist) * 0.5f * elastic;
			segments[A].vel += vector * (num - idealDist) * 0.5f * elastic;
		}
		if (!segments[B].claimedForBend)
		{
			segments[B].pos -= vector * (num - idealDist) * 0.5f * elastic;
			segments[B].vel -= vector * (num - idealDist) * 0.5f * elastic;
		}
	}

	protected void AlignRope(float f, int alignPos)
	{
		int num = Custom.IntClamp((int)(f * (float)(segments.Length - 1)), 0, segments.Length - 1);
		if (!segments[num].claimedForBend)
		{
			segments[num].lastPos = segments[num].pos;
		}
		segments[num].pos = positionsList[alignPos];
		segments[num].vel *= 0f;
		segments[num].bendIndex = alignPos;
	}

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
	}

	public virtual void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public virtual void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
	{
		segments[segment].vel += (goalPos - segments[segment].pos) * 0.2f;
		segments[segment].pos = Vector2.Lerp(segments[segment].pos, goalPos, 0.4f);
	}
}
