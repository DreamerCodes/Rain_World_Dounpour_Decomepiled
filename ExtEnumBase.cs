using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

public abstract class ExtEnumBase : IComparable
{
	public string value;

	protected int valueHash;

	protected int index;

	public int localVersion;

	protected Type enumType;

	protected static Dictionary<Type, ExtEnumType> valueDictionary = new Dictionary<Type, ExtEnumType>();

	public abstract int Index { get; }

	internal ExtEnumBase(Type enumType)
	{
		this.enumType = enumType;
	}

	public override string ToString()
	{
		return value;
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		Type type = GetType();
		if (obj.GetType() != type)
		{
			throw new ArgumentException($"Object must be the same type as the extEnum. The type passed in was {obj.GetType()}; the extEnum type was {type}.");
		}
		return Index.CompareTo((obj as ExtEnumBase).Index);
	}

	public static object Parse(Type enumType, string value, bool ignoreCase)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!enumType.IsExtEnum())
		{
			throw new ArgumentException("enumType is not an ExtEnum type.", "enumType");
		}
		value = value.Trim();
		if (value.Length == 0)
		{
			throw new ArgumentException("An empty string is not considered a valid value.");
		}
		StringComparer stringComparer = (ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);
		ExtEnumType extEnumType = GetExtEnumType(enumType);
		for (int i = 0; i < extEnumType.entries.Count; i++)
		{
			if (stringComparer.Compare(extEnumType.entries[i], value) == 0)
			{
				value = extEnumType.entries[i];
				ExtEnumBase obj = (ExtEnumBase)FormatterServices.GetUninitializedObject(enumType);
				obj.index = i;
				obj.value = value;
				obj.valueHash = value.GetHashCode();
				obj.localVersion = GetExtEnumType(enumType).version;
				obj.enumType = enumType;
				return obj;
			}
		}
		ExtEnumBase obj2 = (ExtEnumBase)FormatterServices.GetUninitializedObject(enumType);
		obj2.index = -1;
		obj2.value = value;
		obj2.valueHash = value.GetHashCode();
		obj2.localVersion = GetExtEnumType(enumType).version;
		obj2.enumType = enumType;
		return obj2;
	}

	public static bool TryParse(Type enumType, string value, bool ignoreCase, out ExtEnumBase result)
	{
		result = null;
		if (!enumType.IsExtEnum())
		{
			return false;
		}
		if (value == null)
		{
			return false;
		}
		value = value.Trim();
		if (value.Length == 0)
		{
			return false;
		}
		ExtEnumType extEnumType = GetExtEnumType(enumType);
		StringComparer stringComparer = (ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);
		for (int i = 0; i < extEnumType.entries.Count; i++)
		{
			if (stringComparer.Compare(extEnumType.entries[i], value) == 0)
			{
				value = extEnumType.entries[i];
				result = (ExtEnumBase)FormatterServices.GetUninitializedObject(enumType);
				result.index = i;
				result.value = value;
				result.valueHash = value.GetHashCode();
				result.localVersion = GetExtEnumType(enumType).version;
				result.enumType = enumType;
				return true;
			}
		}
		return false;
	}

	public static ExtEnumType GetExtEnumType(Type enumType)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsExtEnum())
		{
			throw new ArgumentException("enumType is not an ExtEnum type.", "enumType");
		}
		if (valueDictionary.TryGetValue(enumType, out var result))
		{
			return result;
		}
		throw new ArgumentException("enumType is not initialized ExtEnum type.", "enumType");
	}

	public static bool TryGetExtEnumType(Type enumType, out ExtEnumType type)
	{
		try
		{
			type = GetExtEnumType(enumType);
		}
		catch
		{
			type = null;
			return false;
		}
		return true;
	}

	public static string[] GetNames(Type enumType)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsExtEnum())
		{
			throw new ArgumentException("enumType is not an ExtEnum type.");
		}
		return GetExtEnumType(enumType).entries.ToArray();
	}

	public bool IsDefined(Type enumType, string value, bool ignoreCase)
	{
		if (value == null)
		{
			throw new ArgumentNullException("name");
		}
		ExtEnumType extEnumType = GetExtEnumType(enumType);
		if (!ignoreCase)
		{
			return extEnumType.entries.Contains(value);
		}
		for (int i = 0; i < extEnumType.entries.Count; i++)
		{
			if (string.Compare(this.value, extEnumType.entries[i], ignoreCase: true, CultureInfo.InvariantCulture) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsDefined(Type enumType, int index)
	{
		if (index < 0)
		{
			return false;
		}
		return GetExtEnumType(enumType).entries.Count > index;
	}
}
