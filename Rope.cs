using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class Rope
{
	public struct Corner
	{
		public FloatRect.CornerLabel dir;

		public Vector2 pos;

		public Corner(FloatRect.CornerLabel dir, Vector2 pos)
		{
			this.dir = dir;
			this.pos = pos;
		}
	}

	public class RopeDebugVisualizer
	{
		private Rope rope;

		private DebugSprite mainDebugSprite;

		private List<DebugSprite> sprts;

		public RopeDebugVisualizer(Rope rope)
		{
			this.rope = rope;
			mainDebugSprite = new DebugSprite(rope.A, new FSprite("pixel"), rope.room);
			mainDebugSprite.sprite.anchorY = 0f;
			mainDebugSprite.sprite.color = new Color(0f, 1f, 0.1f);
			mainDebugSprite.sprite.scaleX = 3f;
			mainDebugSprite.sprite.alpha = 0.5f;
			rope.room.AddObject(mainDebugSprite);
			sprts = new List<DebugSprite>();
		}

		public void Update()
		{
			mainDebugSprite.pos = rope.A;
			mainDebugSprite.sprite.rotation = Custom.AimFromOneVectorToAnother(rope.A, rope.B);
			mainDebugSprite.sprite.scaleY = Vector2.Distance(rope.A, rope.B);
			while (sprts.Count > rope.bends.Count + 1)
			{
				sprts[sprts.Count - 1].Destroy();
				sprts.RemoveAt(sprts.Count - 1);
			}
			while (sprts.Count < rope.bends.Count + 1)
			{
				sprts.Add(new DebugSprite(rope.A, new FSprite("pixel"), rope.room));
				rope.room.AddObject(sprts[sprts.Count - 1]);
				sprts[sprts.Count - 1].sprite.anchorY = 0f;
				sprts[sprts.Count - 1].sprite.scaleX = 2f;
			}
			for (int i = 0; i < rope.bends.Count + 1; i++)
			{
				Vector2 a = rope.A;
				Vector2 b = rope.B;
				a = ((i != 0) ? rope.bends[i - 1].pos : rope.A);
				b = ((i != rope.bends.Count) ? rope.bends[i].pos : rope.B);
				sprts[i].pos = a;
				sprts[i].sprite.rotation = Custom.AimFromOneVectorToAnother(a, b);
				sprts[i].sprite.scaleY = Vector2.Distance(a, b);
				sprts[i].sprite.color = (SharedPhysics.RayTraceTilesForTerrain(rope.room, a, b) ? new Color(0f, 1f, 0.1f) : new Color(1f, 0f, 0f));
			}
		}

		public void ClearSprites()
		{
			Custom.Log("clearSprites");
			mainDebugSprite.Destroy();
			for (int i = 0; i < sprts.Count; i++)
			{
				sprts[i].Destroy();
			}
		}
	}

	public Room room;

	public Vector2 A;

	public Vector2 B;

	public Vector2 lastA;

	public Vector2 lastB;

	public float totalLength;

	public List<Corner> bends;

	private float thickness;

	public RopeDebugVisualizer visualizer;

	private Corner[] _cachedCollideWithCorners = new Corner[400];

	private int _cachedMaxSize = 100;

	public Vector2 AConnect
	{
		get
		{
			if (bends.Count == 0)
			{
				return B;
			}
			return bends[0].pos;
		}
	}

	public Vector2 BConnect
	{
		get
		{
			if (bends.Count == 0)
			{
				return A;
			}
			return bends[bends.Count - 1].pos;
		}
	}

	public int TotalPositions => 2 + bends.Count;

	public Vector2 GetPosition(int index)
	{
		if (index == 0)
		{
			return A;
		}
		if (index - 1 >= bends.Count)
		{
			return B;
		}
		return bends[index - 1].pos;
	}

	public List<Vector2> GetAllPositions()
	{
		List<Vector2> list = new List<Vector2>();
		list.Add(A);
		for (int i = 0; i < bends.Count; i++)
		{
			list.Add(bends[i].pos);
		}
		list.Add(B);
		return list;
	}

	public Rope(Room room, Vector2 initA, Vector2 initB, float thickness)
	{
		this.room = room;
		A = initA;
		lastA = initA;
		B = initB;
		lastB = initB;
		totalLength = Vector2.Distance(initA, initB);
		bends = new List<Corner>();
		this.thickness = thickness;
	}

	public void Reset()
	{
		bends.Clear();
	}

	public void Reset(Vector2 pos)
	{
		bends.Clear();
		A = pos;
		lastA = pos;
		B = pos;
		lastB = pos;
	}

	public void Update(Vector2 newA, Vector2 newB)
	{
		lastA = A;
		lastB = B;
		A = newA;
		B = newB;
		if (bends.Count == 0)
		{
			CollideWithCorners(lastA, A, lastB, B, 0, 0);
		}
		else
		{
			CollideWithCorners(BConnect, BConnect, lastB, B, bends.Count, 0);
			CollideWithCorners(lastA, A, AConnect, AConnect, 0, 0);
		}
		if (bends.Count > 0)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < bends.Count; i++)
			{
				Vector2 l = A;
				Vector2 l2 = B;
				if (i > 0)
				{
					l = bends[i - 1].pos;
				}
				if (i < bends.Count - 1)
				{
					l2 = bends[i + 1].pos;
				}
				if (!DoesLineOverlapCorner(l, l2, bends[i]))
				{
					list.Add(i);
				}
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				bends.RemoveAt(list[num]);
			}
		}
		if (bends.Count == 0)
		{
			totalLength = Vector2.Distance(A, B);
		}
		else
		{
			totalLength = Vector2.Distance(A, AConnect) + Vector2.Distance(BConnect, B);
			for (int j = 1; j < bends.Count; j++)
			{
				totalLength += Vector2.Distance(bends[j - 1].pos, bends[j].pos);
			}
		}
		if (bends.Count > 50)
		{
			Reset();
		}
		if (visualizer != null)
		{
			visualizer.Update();
		}
	}

	private void CollideWithCorners(Vector2 la, Vector2 a, Vector2 lb, Vector2 b, int bend, int recursion)
	{
		if (recursion > 10)
		{
			return;
		}
		IntRect intRect = IntRect.MakeFromIntVector2(room.GetTilePosition(la));
		intRect.ExpandToInclude(room.GetTilePosition(a));
		intRect.ExpandToInclude(room.GetTilePosition(lb));
		intRect.ExpandToInclude(room.GetTilePosition(b));
		intRect.Grow(1);
		int num = (intRect.right - intRect.left + 1) * (intRect.top - intRect.bottom + 1);
		if (num > _cachedMaxSize)
		{
			int num2 = num * 4;
			Custom.LogWarning($"Increasing from:{_cachedMaxSize} to:{num2}");
			_cachedCollideWithCorners = new Corner[num2];
			_cachedMaxSize = num2;
		}
		int num3 = 0;
		for (int i = intRect.left; i <= intRect.right; i++)
		{
			for (int j = intRect.bottom; j <= intRect.top; j++)
			{
				if (!room.GetTile(i, j).Solid)
				{
					continue;
				}
				if (!room.GetTile(i - 1, j).Solid)
				{
					if (!room.GetTile(i, j - 1).Solid && !room.GetTile(i - 1, j - 1).Solid)
					{
						_cachedCollideWithCorners[num3++] = new Corner(FloatRect.CornerLabel.D, room.MiddleOfTile(i, j) + new Vector2(-10f - thickness, -10f - thickness));
					}
					if (!room.GetTile(i, j + 1).Solid && !room.GetTile(i - 1, j + 1).Solid)
					{
						_cachedCollideWithCorners[num3++] = new Corner(FloatRect.CornerLabel.A, room.MiddleOfTile(i, j) + new Vector2(-10f - thickness, 10f + thickness));
					}
				}
				if (!room.GetTile(i + 1, j).Solid)
				{
					if (!room.GetTile(i, j - 1).Solid && !room.GetTile(i + 1, j - 1).Solid)
					{
						_cachedCollideWithCorners[num3++] = new Corner(FloatRect.CornerLabel.C, room.MiddleOfTile(i, j) + new Vector2(10f + thickness, -10f - thickness));
					}
					if (!room.GetTile(i, j + 1).Solid && !room.GetTile(i + 1, j + 1).Solid)
					{
						_cachedCollideWithCorners[num3++] = new Corner(FloatRect.CornerLabel.B, room.MiddleOfTile(i, j) + new Vector2(10f + thickness, 10f + thickness));
					}
				}
			}
		}
		Corner? corner = null;
		float f = float.MaxValue;
		for (int k = 0; k < num3; k++)
		{
			Corner corner2 = _cachedCollideWithCorners[k];
			if (DoesLineOverlapCorner(a, b, corner2) && corner2.pos != la && corner2.pos != a && corner2.pos != lb && corner2.pos != b && (Custom.PointInTriangle(corner2.pos, a, la, b) || Custom.PointInTriangle(corner2.pos, a, lb, b) || Custom.PointInTriangle(corner2.pos, a, la, lb) || Custom.PointInTriangle(corner2.pos, la, lb, b)) && Mathf.Abs(Custom.DistanceToLine(corner2.pos, la, lb)) < Mathf.Abs(f))
			{
				corner = corner2;
				f = Custom.DistanceToLine(corner2.pos, lastA, lastB);
			}
		}
		if (corner.HasValue)
		{
			Vector2 pos = corner.Value.pos;
			bends.Insert(bend, corner.Value);
			Vector2 vector = Custom.ClosestPointOnLine(la, lb, pos);
			CollideWithCorners(vector, pos, lb, b, bend + 1, recursion + 1);
			CollideWithCorners(la, a, vector, pos, bend, recursion + 1);
		}
	}

	public bool DoesLineOverlapCorner(Vector2 l1, Vector2 l2, Corner corner)
	{
		IntVector2 intVector = new IntVector2((corner.dir != 0 && corner.dir != FloatRect.CornerLabel.D) ? 1 : (-1), (corner.dir == FloatRect.CornerLabel.A || corner.dir == FloatRect.CornerLabel.B) ? 1 : (-1));
		if (l1.y != l2.y && ((intVector.x < 0 && Custom.HorizontalCrossPoint(l1, l2, corner.pos.y).x < corner.pos.x) || (intVector.x > 0 && Custom.HorizontalCrossPoint(l1, l2, corner.pos.y).x > corner.pos.x)))
		{
			return false;
		}
		if (l1.x != l2.x && ((intVector.y < 0 && Custom.VerticalCrossPoint(l1, l2, corner.pos.x).y < corner.pos.y) || (intVector.y > 0 && Custom.VerticalCrossPoint(l1, l2, corner.pos.x).y > corner.pos.y)))
		{
			return false;
		}
		return true;
	}
}
