using System;
using System.Runtime.CompilerServices;

public abstract class ExtEnum<T> : ExtEnumBase, IEquatable<T> where T : class
{
	public static ExtEnumType values;

	public static int valuesVersion
	{
		get
		{
			return values.version;
		}
		set
		{
			values.version = value;
		}
	}

	public override int Index
	{
		get
		{
			if (localVersion == valuesVersion)
			{
				return index;
			}
			localVersion = valuesVersion;
			return index = values.entries.IndexOf(value);
		}
	}

	public ExtEnum(string value, bool register = false)
		: base(typeof(T))
	{
		base.value = value;
		valueHash = value.GetHashCode();
		if (values == null)
		{
			if (ExtEnumBase.valueDictionary.ContainsKey(GetType()))
			{
				values = ExtEnumBase.valueDictionary[GetType()];
			}
			else
			{
				values = new ExtEnumType();
				ExtEnumBase.valueDictionary.Add(GetType(), values);
			}
		}
		if (values.entries.Contains(value))
		{
			index = values.entries.IndexOf(value);
		}
		else if (register)
		{
			values.AddEntry(value);
			index = values.Count - 1;
		}
		else
		{
			index = -1;
		}
		localVersion = valuesVersion;
	}

	protected ExtEnum()
		: base(typeof(T))
	{
		throw new NotImplementedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object obj)
	{
		if (!(obj is T))
		{
			return false;
		}
		return Equals(obj as T);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ExtEnum<T> other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return valueHash == other.valueHash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(T other)
	{
		ExtEnum<T> extEnum = other as ExtEnum<T>;
		if (extEnum != null)
		{
			return extEnum.value == value;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return valueHash;
	}

	public void Unregister()
	{
		values.RemoveEntry(value);
		valuesVersion++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(ExtEnum<T> a, ExtEnum<T> b)
	{
		return a?.Equals(b) ?? ((object)b == null);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(ExtEnum<T> a, ExtEnum<T> b)
	{
		return !(a == b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int(ExtEnum<T> a)
	{
		if (a == null)
		{
			return -1;
		}
		return a.Index;
	}
}
