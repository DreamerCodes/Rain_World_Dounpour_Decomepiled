using System;
using UnityEngine;

namespace Noise;

public struct InGameNoise : IEquatable<InGameNoise>
{
	public Vector2 pos;

	public float strength;

	public PhysicalObject sourceObject;

	public float interesting;

	public bool Equals(InGameNoise other)
	{
		if (pos.Equals(other.pos) && strength.Equals(other.strength) && object.Equals(sourceObject, other.sourceObject))
		{
			return interesting.Equals(other.interesting);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is InGameNoise other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((pos.GetHashCode() * 397) ^ strength.GetHashCode()) * 397) ^ ((sourceObject != null) ? sourceObject.GetHashCode() : 0)) * 397) ^ interesting.GetHashCode();
	}

	public static bool operator ==(InGameNoise left, InGameNoise right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(InGameNoise left, InGameNoise right)
	{
		return !left.Equals(right);
	}

	public InGameNoise(Vector2 pos, float strength, PhysicalObject sourceObject, float interesting)
	{
		this.pos = pos;
		this.strength = strength;
		this.sourceObject = sourceObject;
		this.interesting = interesting;
	}
}
