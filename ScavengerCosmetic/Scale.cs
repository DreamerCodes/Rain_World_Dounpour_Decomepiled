using RWCustom;
using UnityEngine;

namespace ScavengerCosmetic;

public class Scale
{
	public class Stats
	{
		public float grav;

		public float airFric;

		public float rigid;

		public float rigidGradRad;

		public float rigidExp;

		public float elastic;

		public Stats(float grav, float airFric, float rigid, float rigidGradRad, float rigidExp, float elastic)
		{
			this.grav = grav;
			this.airFric = airFric;
			this.rigid = rigid;
			this.rigidGradRad = rigidGradRad;
			this.rigidExp = rigidExp;
			this.elastic = elastic;
		}
	}

	public Stats stats;

	public Vector2 pos;

	public Vector2 lastPos;

	public Vector2 vel;

	public int index;

	public float length;

	public Scale(int index, Stats stats, float length)
	{
		this.index = index;
		this.stats = stats;
		this.length = length;
	}

	public void Update(Vector2 attachedPos, Vector2 idealPos)
	{
		lastPos = pos;
		pos += vel;
		vel *= stats.airFric;
		vel.y -= stats.grav;
		vel += Custom.DirVec(pos, idealPos) * Mathf.Pow(Mathf.InverseLerp(0f, stats.rigidGradRad, Vector2.Distance(pos, idealPos)), stats.rigidExp) * stats.rigid;
		pos += Custom.DirVec(attachedPos, pos) * (length - Vector2.Distance(attachedPos, pos)) * (1f - stats.elastic);
		if (!Custom.DistLess(pos, idealPos, length))
		{
			vel += Custom.DirVec(pos, idealPos) * (Vector2.Distance(pos, idealPos) - length);
			pos += Custom.DirVec(pos, idealPos) * (Vector2.Distance(pos, idealPos) - length);
		}
	}
}
