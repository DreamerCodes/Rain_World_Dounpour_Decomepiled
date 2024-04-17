using RWCustom;
using UnityEngine;

public class VisionObscurer : UpdatableAndDeletable
{
	public Vector2 pos;

	public float rad;

	public float fullObscureDist;

	public float obscureFac;

	public VisionObscurer(Vector2 pos, float rad, float fullObscureDist, float obscureFac)
	{
		this.pos = pos;
		this.rad = rad;
		this.fullObscureDist = fullObscureDist;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
	}

	public float VisionScore(Vector2 A, Vector2 B, float initalScore)
	{
		float num = Custom.CirclesCollisionTime(A.x, A.y, pos.x, pos.y, B.x - A.x, B.y - A.y, 1f, rad);
		float t = Custom.CirclesCollisionTime(B.x, B.y, pos.x, pos.y, A.x - B.x, A.y - B.y, 1f, rad);
		if ((float.IsNaN(num) || num <= 0f || num >= 1f) && !Custom.DistLess(A, pos, rad) && !Custom.DistLess(B, pos, rad))
		{
			return initalScore;
		}
		if (!Custom.DistLess(A, pos, rad))
		{
			A = Vector2.Lerp(A, B, num);
		}
		if (!Custom.DistLess(B, pos, rad))
		{
			B = Vector2.Lerp(B, A, t);
		}
		return initalScore - Mathf.InverseLerp(0f, fullObscureDist, Vector2.Distance(A, B)) * obscureFac;
	}

	public override void Destroy()
	{
		base.Destroy();
	}
}
