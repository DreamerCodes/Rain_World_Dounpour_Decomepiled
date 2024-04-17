using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class ClimbableVinesSystem : UpdatableAndDeletable
{
	public class VinePosition
	{
		public int vine;

		public float floatPos;

		public VinePosition(int vine, float floatPos)
		{
			this.vine = vine;
			this.floatPos = floatPos;
		}
	}

	public List<IClimbableVine> vines;

	public VineVisualizer viz;

	public ClimbableVinesSystem()
	{
		vines = new List<IClimbableVine>();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
	}

	public VinePosition VineOverlap(Vector2 pos, float rad)
	{
		float num = float.MaxValue;
		VinePosition result = null;
		for (int i = 0; i < vines.Count; i++)
		{
			for (int j = 0; j < vines[i].TotalPositions() - 1; j++)
			{
				if (OverlappingSegment(vines[i].Pos(j), vines[i].Rad(j), vines[i].Pos(j + 1), vines[i].Rad(j + 1), pos, rad))
				{
					Vector2 b = ClosestPointOnSegment(vines[i].Pos(j), vines[i].Pos(j + 1), pos);
					float num2 = Vector2.Distance(pos, b);
					if (num2 < num)
					{
						num = num2;
						float t = Mathf.InverseLerp(0f, Vector2.Distance(vines[i].Pos(j), vines[i].Pos(j + 1)), Vector2.Distance(vines[i].Pos(j), b));
						result = new VinePosition(i, Mathf.Lerp(FloatAtSegment(i, j), FloatAtSegment(i, j + 1), t));
					}
				}
			}
		}
		return result;
	}

	public Vector2 OnVinePos(VinePosition vPos)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vines[vPos.vine].TotalPositions() - 1);
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		return Vector2.Lerp(vines[vPos.vine].Pos(num), vines[vPos.vine].Pos(num2), t);
	}

	public float VineRad(VinePosition vPos)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vines[vPos.vine].TotalPositions() - 1);
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		return Mathf.Lerp(vines[vPos.vine].Rad(num), vines[vPos.vine].Rad(num2), t);
	}

	public Vector2 VineDir(VinePosition vPos)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vines[vPos.vine].TotalPositions() - 1);
		if (num == num2)
		{
			return Custom.DirVec(vines[vPos.vine].Pos(vines[vPos.vine].TotalPositions() - 2), vines[vPos.vine].Pos(vines[vPos.vine].TotalPositions() - 1));
		}
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		return Vector2.Lerp(DirOfSegment(vPos.vine, num), DirOfSegment(vPos.vine, num2), t).normalized;
	}

	private Vector2 DirOfSegment(int vine, int seg)
	{
		if (seg == vines[vine].TotalPositions() - 1)
		{
			return Custom.DirVec(vines[vine].Pos(vines[vine].TotalPositions() - 2), vines[vine].Pos(vines[vine].TotalPositions() - 1));
		}
		return Custom.DirVec(vines[vine].Pos(seg), vines[vine].Pos(seg + 1));
	}

	public float ClimbOnVineSpeed(VinePosition vPos, Vector2 goalPos)
	{
		if (vPos.floatPos == 0f)
		{
			return 1f;
		}
		if (vPos.floatPos == 1f)
		{
			return -1f;
		}
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vines[vPos.vine].TotalPositions() - 1);
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		Vector2 vector = Vector2.Lerp(vines[vPos.vine].Pos(num), vines[vPos.vine].Pos(num2), t);
		float f = Vector2.Dot((vines[vPos.vine].Pos(num) - vines[vPos.vine].Pos(num2)).normalized, (vector - goalPos).normalized);
		return Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(f)) * Mathf.Sign(f);
	}

	public VinePosition VineSwitch(VinePosition vPos, Vector2 goalPos, float rad)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vines[vPos.vine].TotalPositions() - 1);
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		Vector2 vector = Vector2.Lerp(vines[vPos.vine].Pos(num), vines[vPos.vine].Pos(num2), t);
		if (Mathf.Abs(Vector2.Dot((vines[vPos.vine].Pos(num) - vines[vPos.vine].Pos(num2)).normalized, (vector - goalPos).normalized)) > 0.5f)
		{
			return null;
		}
		float num3 = float.MaxValue;
		VinePosition result = null;
		for (int i = 0; i < vines.Count; i++)
		{
			for (int j = 0; j < vines[i].TotalPositions() - 1; j++)
			{
				if (!OverlappingSegment(vines[i].Pos(j), vines[i].Rad(j), vines[i].Pos(j + 1), vines[i].Rad(j + 1), vector, rad))
				{
					continue;
				}
				Vector2 vector2 = ClosestPointOnSegment(vines[i].Pos(j), vines[i].Pos(j + 1), vector);
				float num4 = Vector2.Distance(vector2, goalPos);
				num4 *= 1f - 0.25f * Mathf.Abs(Vector2.Dot((vines[i].Pos(j) - vines[i].Pos(j + 1)).normalized, (vector - goalPos).normalized));
				if (i == vPos.vine)
				{
					float num5 = Mathf.Lerp(FloatAtSegment(i, j), FloatAtSegment(i, j + 1), Mathf.InverseLerp(0f, Vector2.Distance(vines[i].Pos(j), vines[i].Pos(j + 1)), Vector2.Distance(vines[i].Pos(j), vector2))) * TotalLength(i);
					if (Mathf.Abs(vPos.floatPos * TotalLength(vPos.vine) - num5) < 100f)
					{
						num4 = float.MaxValue;
					}
				}
				if (num4 < num3)
				{
					num3 = num4;
					float t2 = Mathf.InverseLerp(0f, Vector2.Distance(vines[i].Pos(j), vines[i].Pos(j + 1)), Vector2.Distance(vines[i].Pos(j), vector2));
					result = new VinePosition(i, Mathf.Lerp(FloatAtSegment(i, j), FloatAtSegment(i, j + 1), t2));
				}
			}
		}
		return result;
	}

	public float TotalLength(int vine)
	{
		float num = 0f;
		for (int i = 0; i < vines[vine].TotalPositions() - 1; i++)
		{
			num += Vector2.Distance(vines[vine].Pos(i), vines[vine].Pos(i + 1));
		}
		return num;
	}

	public float FloatAtSegment(int vine, int segment)
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < vines[vine].TotalPositions() - 1; i++)
		{
			if (i < segment)
			{
				num2 += Vector2.Distance(vines[vine].Pos(i), vines[vine].Pos(i + 1));
			}
			num += Vector2.Distance(vines[vine].Pos(i), vines[vine].Pos(i + 1));
		}
		return num2 / num;
	}

	public int PrevSegAtFloat(int vine, float fPos)
	{
		fPos *= TotalLength(vine);
		float num = 0f;
		for (int i = 0; i < vines[vine].TotalPositions() - 1; i++)
		{
			num += Vector2.Distance(vines[vine].Pos(i), vines[vine].Pos(i + 1));
			if (num > fPos)
			{
				return i;
			}
		}
		return vines[vine].TotalPositions() - 1;
	}

	private bool OverlappingSegment(Vector2 A, float aRad, Vector2 B, float bRad, Vector2 testPos, float tRad)
	{
		Vector2 vector = Custom.PerpendicularVector((A - B).normalized);
		if (Custom.DistanceToLine(testPos, A + vector, A - vector) >= 0f)
		{
			return Vector2.Distance(testPos, A) < aRad + tRad;
		}
		if (Custom.DistanceToLine(testPos, B + vector, B - vector) <= 0f)
		{
			return Vector2.Distance(testPos, B) < bRad + tRad;
		}
		Vector2 vector2 = Custom.ClosestPointOnLine(A, B, testPos);
		return Vector2.Distance(vector2, testPos) < Custom.LerpMap(Vector2.Distance(A, vector2), 0f, Vector2.Distance(A, B), aRad, bRad);
	}

	private Vector2 ClosestPointOnSegment(Vector2 A, Vector2 B, Vector2 testPos)
	{
		Vector2 vector = Custom.PerpendicularVector((A - B).normalized);
		if (Custom.DistanceToLine(testPos, A + vector, A - vector) >= 0f)
		{
			return A;
		}
		if (Custom.DistanceToLine(testPos, B + vector, B - vector) <= 0f)
		{
			return B;
		}
		return Custom.ClosestPointOnLine(A, B, testPos);
	}

	public void ConnectChunkToVine(BodyChunk chunk, VinePosition vPos, float conRad)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vines[vPos.vine].TotalPositions() - 1);
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		Vector2 vector = Vector2.Lerp(vines[vPos.vine].Pos(num), vines[vPos.vine].Pos(num2), t);
		float num3 = chunk.mass / (chunk.mass + Mathf.Lerp(vines[vPos.vine].Mass(num), vines[vPos.vine].Mass(num2), t));
		float num4 = Vector2.Distance(chunk.pos, vector);
		Vector2 vector2 = Custom.DirVec(chunk.pos, vector);
		if (num4 > conRad)
		{
			chunk.pos += vector2 * (num4 - conRad) * (1f - num3);
			chunk.vel += vector2 * (num4 - conRad) * (1f - num3);
			vines[vPos.vine].Push(num, -vector2 * (num4 - conRad) * num3);
			vines[vPos.vine].Push(num2, -vector2 * (num4 - conRad) * num3);
		}
	}

	public void PushAtVine(VinePosition vPos, Vector2 push)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vines[vPos.vine].TotalPositions() - 1);
		float num3 = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		float num4 = Mathf.Max(1f, Mathf.Lerp(vines[vPos.vine].Mass(num), vines[vPos.vine].Mass(num2), num3));
		vines[vPos.vine].Push(num, push / num4 * (1f - num3));
		vines[vPos.vine].Push(num2, push / num4 * num3);
	}

	public void VineBeingClimbedOn(VinePosition vPos, Creature crit)
	{
		vines[vPos.vine].BeingClimbedOn(crit);
	}

	public bool VineCurrentlyClimbable(VinePosition vPos)
	{
		return vines[vPos.vine].CurrentlyClimbable();
	}

	public IClimbableVine GetVineObject(VinePosition vPos)
	{
		return vines[vPos.vine];
	}
}
