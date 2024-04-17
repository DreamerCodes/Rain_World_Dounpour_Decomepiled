using System.Globalization;
using UnityEngine;

namespace Menu.Remix.MixedUI.ValueTypes;

public static class ValueExt
{
	public static void SetValueFloat(this IValueFloat UIconfig, float value)
	{
		if (UIconfig is IValueInt)
		{
			UIconfig.valueString = Mathf.FloorToInt(value).ToString(NumberFormatInfo.InvariantInfo);
		}
		else
		{
			UIconfig.valueString = value.ToString(NumberFormatInfo.InvariantInfo);
		}
	}

	public static float GetValueFloat(this IValueFloat UIconfig)
	{
		if (!float.TryParse(UIconfig.valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			return 0f;
		}
		return result;
	}

	public static void SetValueInt(this IValueFloat UIconfig, int value)
	{
		UIconfig.valueString = value.ToString(NumberFormatInfo.InvariantInfo);
	}

	public static int GetValueInt(this IValueInt UIconfig)
	{
		if (!int.TryParse(UIconfig.valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			return 0;
		}
		return result;
	}

	public static void SetValueBool(this IValueBool UIconfig, bool value)
	{
		UIconfig.valueString = (value ? "true" : "false");
	}

	public static bool GetValueBool(this IValueBool UIconfig)
	{
		return UIconfig.valueString == "true";
	}
}
