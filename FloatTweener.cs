using UnityEngine;

public static class FloatTweener
{
	public class TweenType : ExtEnum<TweenType>
	{
		public static readonly TweenType None = new TweenType("None", register: true);

		public static readonly TweenType Lerp = new TweenType("Lerp", register: true);

		public static readonly TweenType Tick = new TweenType("Tick", register: true);

		public TweenType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public abstract class FloatTween
	{
		public FloatTween()
		{
		}

		public virtual float Tween(float A, float B)
		{
			return B;
		}
	}

	public class FloatTweenMixer : FloatTween
	{
		public FloatTween M1;

		public FloatTween M2;

		public float mix;

		public FloatTweenMixer(FloatTween M1, FloatTween M2, float mix)
		{
			this.M1 = M1;
			this.M2 = M2;
			this.mix = mix;
		}

		public override float Tween(float A, float B)
		{
			return Mathf.Lerp(M1.Tween(A, B), M2.Tween(A, B), mix);
		}
	}

	public class FloatTweenUpAndDown : FloatTween
	{
		public FloatTween up;

		public FloatTween down;

		public FloatTweenUpAndDown(FloatTween up, FloatTween down)
		{
			this.up = up;
			this.down = down;
		}

		public override float Tween(float A, float B)
		{
			if (A < B)
			{
				return up.Tween(A, B);
			}
			return down.Tween(A, B);
		}
	}

	public class FloatTweenBasic : FloatTween
	{
		public TweenType type;

		public float speed;

		public FloatTweenBasic(TweenType type, float speed)
		{
			this.type = type;
			this.speed = speed;
		}

		public override float Tween(float A, float B)
		{
			return BaseTween(A, B, type, speed);
		}
	}

	public class FloatTweenExponent : FloatTween
	{
		public FloatTween twn;

		public float pow;

		public FloatTweenExponent(FloatTween twn, float pow)
		{
			this.twn = twn;
			this.pow = pow;
		}

		public override float Tween(float A, float B)
		{
			return twn.Tween(A, B);
		}
	}

	public static float BaseTween(float A, float B, TweenType type, float speed)
	{
		if (A == B)
		{
			return A;
		}
		if (type == TweenType.Lerp)
		{
			return Mathf.Lerp(A, B, speed);
		}
		if (type == TweenType.Tick)
		{
			if (A < B)
			{
				return Mathf.Min(A + speed, B);
			}
			return Mathf.Max(A - speed, B);
		}
		return B;
	}
}
