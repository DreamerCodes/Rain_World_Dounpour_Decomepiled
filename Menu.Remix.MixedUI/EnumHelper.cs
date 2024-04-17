using System;
using System.ComponentModel;
using System.Reflection;

namespace Menu.Remix.MixedUI;

internal static class EnumHelper
{
	public static string GetEnumDesc(Enum value)
	{
		if (value == null)
		{
			return null;
		}
		FieldInfo field = value.GetType().GetField(value.ToString());
		if (field == null)
		{
			return null;
		}
		if (field.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false) is DescriptionAttribute[] array && array.Length != 0)
		{
			return array[0].Description;
		}
		return null;
	}
}
