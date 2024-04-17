using System;
using UnityEngine;

public class FMatrix
{
	public static FMatrix tempMatrix = new FMatrix();

	public float a = 1f;

	public float b;

	public float c;

	public float d = 1f;

	public float tx;

	public float ty;

	public FMatrix Clone()
	{
		return new FMatrix
		{
			a = a,
			b = b,
			c = c,
			d = d,
			tx = tx,
			ty = ty
		};
	}

	public void CopyValues(FMatrix sourceMatrix)
	{
		a = sourceMatrix.a;
		b = sourceMatrix.b;
		c = sourceMatrix.c;
		d = sourceMatrix.d;
		tx = sourceMatrix.tx;
		ty = sourceMatrix.ty;
	}

	public void SetRotateThenScale(float x, float y, float scaleX, float scaleY, float rotationInRadians)
	{
		float num = Mathf.Sin(rotationInRadians);
		float num2 = Mathf.Cos(rotationInRadians);
		a = scaleX * num2;
		c = scaleX * (0f - num);
		b = scaleY * num;
		d = scaleY * num2;
		tx = x;
		ty = y;
	}

	public void SetScaleThenRotate(float x, float y, float scaleX, float scaleY, float rotationInRadians)
	{
		float num = Mathf.Sin(rotationInRadians);
		float num2 = Mathf.Cos(rotationInRadians);
		a = scaleX * num2;
		b = scaleX * num;
		c = scaleY * (0f - num);
		d = scaleY * num2;
		tx = x;
		ty = y;
	}

	public void Translate(float deltaX, float deltaY)
	{
		tx += deltaX;
		ty += deltaY;
	}

	public void Scale(float scaleX, float scaleY)
	{
		a *= scaleX;
		c *= scaleX;
		tx *= scaleX;
		b *= scaleY;
		d *= scaleY;
		ty *= scaleY;
	}

	public void Rotate(float rotationInRadians)
	{
		float num = Mathf.Sin(rotationInRadians);
		float num2 = Mathf.Cos(rotationInRadians);
		float num3 = a;
		float num4 = b;
		float num5 = c;
		float num6 = d;
		float num7 = tx;
		float num8 = ty;
		a = num3 * num2 - num4 * num;
		b = num3 * num + num4 * num2;
		c = num5 * num2 - num6 * num;
		d = num5 * num + num6 * num2;
		tx = num7 * num2 - num8 * num;
		ty = num7 * num + num8 * num2;
	}

	public void RotateInPlace(float rotationInRadians)
	{
		float num = Mathf.Sin(rotationInRadians);
		float num2 = Mathf.Cos(rotationInRadians);
		float num3 = a;
		float num4 = b;
		float num5 = c;
		float num6 = d;
		a = num3 * num2 - num4 * num;
		b = num3 * num + num4 * num2;
		c = num5 * num2 - num6 * num;
		d = num5 * num + num6 * num2;
	}

	public float GetScaleX()
	{
		return Mathf.Sqrt(a * a + b * b);
	}

	public float GetScaleY()
	{
		return Mathf.Sqrt(c * c + d * d);
	}

	public float GetRotation()
	{
		Vector2 newTransformedVector = GetNewTransformedVector(new Vector2(0f, 1f));
		return Mathf.Atan2(newTransformedVector.y - ty, newTransformedVector.x - tx) - (float)Math.PI / 2f;
	}

	public void Concat(FMatrix other)
	{
		float num = a;
		float num2 = b;
		float num3 = c;
		float num4 = d;
		float num5 = tx;
		float num6 = ty;
		a = num * other.a + num2 * other.c;
		b = num * other.b + num2 * other.d;
		c = num3 * other.a + num4 * other.c;
		d = num3 * other.b + num4 * other.d;
		tx = num5 * other.a + num6 * other.c + other.tx;
		ty = num5 * other.b + num6 * other.d + other.ty;
	}

	public void ConcatOther(FMatrix other)
	{
		float num = a;
		float num2 = b;
		float num3 = c;
		float num4 = d;
		float num5 = tx;
		float num6 = ty;
		a = other.a * num + other.b * num3;
		b = other.a * num2 + other.b * num4;
		c = other.c * num + other.d * num3;
		d = other.c * num2 + other.d * num4;
		tx = other.tx * num + other.ty * num3 + num5;
		ty = other.tx * num2 + other.ty * num4 + num6;
	}

	public void ConcatAndCopyValues(FMatrix first, FMatrix second)
	{
		a = first.a * second.a + first.b * second.c;
		b = first.a * second.b + first.b * second.d;
		c = first.c * second.a + first.d * second.c;
		d = first.c * second.b + first.d * second.d;
		tx = first.tx * second.a + first.ty * second.c + second.tx;
		ty = first.tx * second.b + first.ty * second.d + second.ty;
	}

	public void Invert()
	{
		float num = a;
		float num2 = b;
		float num3 = c;
		float num4 = d;
		float num5 = tx;
		float num6 = ty;
		float num7 = 1f / (a * d - b * c);
		a = num4 * num7;
		b = (0f - num2) * num7;
		c = (0f - num3) * num7;
		d = num * num7;
		tx = (num3 * num6 - num4 * num5) * num7;
		ty = (0f - (num * num6 - num2 * num5)) * num7;
	}

	public void InvertAndCopyValues(FMatrix other)
	{
		float num = 1f / (other.a * other.d - other.b * other.c);
		a = other.d * num;
		b = (0f - other.b) * num;
		c = (0f - other.c) * num;
		d = other.a * num;
		tx = (other.c * other.ty - other.d * other.tx) * num;
		ty = (0f - (other.a * other.ty - other.b * other.tx)) * num;
	}

	public Vector2 GetNewTransformedVector(Vector2 vector)
	{
		return new Vector2(vector.x * a + vector.y * c + tx, vector.x * b + vector.y * d + ty);
	}

	public Vector2 GetTransformedUnitVector()
	{
		return new Vector2(a + c + tx, b + d + ty);
	}

	public Vector3 GetVector3FromLocalVector2(Vector2 localVector, float z)
	{
		return new Vector3(localVector.x * a + localVector.y * c + tx, localVector.x * b + localVector.y * d + ty, z);
	}

	public void ApplyVector3FromLocalVector2(ref Vector3 outVector, Vector2 localVector, float z)
	{
		outVector.x = localVector.x * a + localVector.y * c + tx;
		outVector.y = localVector.x * b + localVector.y * d + ty;
		outVector.z = z;
	}

	public void ResetToIdentity()
	{
		a = 1f;
		b = 0f;
		c = 0f;
		d = 1f;
		tx = 0f;
		ty = 0f;
	}

	public override string ToString()
	{
		return $"[[Matrix A:{a} B:{b} C:{c} D:{d} TX:{tx} TY:{ty} ]]";
	}
}
