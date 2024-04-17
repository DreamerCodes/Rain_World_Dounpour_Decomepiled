using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Menu.Remix;

public static class ValueConverter
{
	public enum TypeCategory
	{
		Unsupported = -1,
		String,
		Boolean,
		Integrals,
		Floats,
		Enum,
		ExtEnum,
		Misc
	}

	public class Converter
	{
		public TypeCategory category;

		public Func<object, Type, string> ConvertToString { get; set; }

		public Func<string, Type, object> ConvertToObject { get; set; }
	}

	private static Dictionary<Type, Converter> _Converters;

	internal static void _Initialize()
	{
		_Converters = new Dictionary<Type, Converter>();
		_Converters.Add(typeof(string), new Converter
		{
			ConvertToString = (object obj, Type type) => ((string)obj)._Escape(),
			ConvertToObject = (string str, Type type) => Regex.IsMatch(str, "^\"?\\w:\\\\(?!\\\\)(?!.+\\\\\\\\)") ? str : str._Unescape(),
			category = TypeCategory.String
		});
		_Converters.Add(typeof(bool), new Converter
		{
			ConvertToString = (object obj, Type type) => ((bool)obj).ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
			ConvertToObject = (string str, Type type) => bool.Parse(str),
			category = TypeCategory.Boolean
		});
		_Converters.Add(typeof(byte), new Converter
		{
			ConvertToString = (object obj, Type type) => ((byte)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => byte.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Integrals
		});
		_Converters.Add(typeof(sbyte), new Converter
		{
			ConvertToString = (object obj, Type type) => ((sbyte)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => sbyte.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Integrals
		});
		_Converters.Add(typeof(short), new Converter
		{
			ConvertToString = (object obj, Type type) => ((short)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => short.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Integrals
		});
		_Converters.Add(typeof(ushort), new Converter
		{
			ConvertToString = (object obj, Type type) => ((ushort)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => ushort.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Integrals
		});
		_Converters.Add(typeof(int), new Converter
		{
			ConvertToString = (object obj, Type type) => ((int)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => int.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Integrals
		});
		_Converters.Add(typeof(uint), new Converter
		{
			ConvertToString = (object obj, Type type) => ((uint)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => uint.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Integrals
		});
		_Converters.Add(typeof(long), new Converter
		{
			ConvertToString = (object obj, Type type) => ((long)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => long.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Integrals
		});
		_Converters.Add(typeof(ulong), new Converter
		{
			ConvertToString = (object obj, Type type) => ((ulong)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => ulong.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Integrals
		});
		_Converters.Add(typeof(float), new Converter
		{
			ConvertToString = (object obj, Type type) => ((float)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => float.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Floats
		});
		_Converters.Add(typeof(double), new Converter
		{
			ConvertToString = (object obj, Type type) => ((double)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => double.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Floats
		});
		_Converters.Add(typeof(decimal), new Converter
		{
			ConvertToString = (object obj, Type type) => ((decimal)obj).ToString(NumberFormatInfo.InvariantInfo),
			ConvertToObject = (string str, Type type) => decimal.Parse(str, NumberFormatInfo.InvariantInfo),
			category = TypeCategory.Floats
		});
		_Converters.Add(typeof(Enum), new Converter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => Enum.Parse(type, str, ignoreCase: true),
			category = TypeCategory.Enum
		});
		_Converters.Add(typeof(Color), new Converter
		{
			ConvertToString = (object obj, Type type) => MenuColorEffect.ColorToHex((Color)obj),
			ConvertToObject = (string str, Type type) => MenuColorEffect.HexToColor(str),
			category = TypeCategory.Misc
		});
		_Converters.Add(typeof(ExtEnumBase), new Converter
		{
			ConvertToString = (object obj, Type type) => obj.ToString(),
			ConvertToObject = (string str, Type type) => ExtEnumBase.Parse(type, str, ignoreCase: true),
			category = TypeCategory.ExtEnum
		});
	}

	public static string ConvertToString(object value, Type valueType)
	{
		return (GetConverter(valueType) ?? throw new InvalidOperationException("Cannot convert from type " + valueType.ToString())).ConvertToString(value, valueType);
	}

	public static string ConvertToString<T>(T value)
	{
		return ConvertToString(value, typeof(T));
	}

	public static T ConvertToValue<T>(string value)
	{
		return (T)ConvertToValue(value, typeof(T));
	}

	public static object ConvertToValue(string value, Type valueType)
	{
		return (GetConverter(valueType) ?? throw new InvalidOperationException("Cannot convert to type " + valueType.Name)).ConvertToObject(value, valueType);
	}

	public static Converter GetConverter(Type valueType)
	{
		if (valueType == null)
		{
			throw new ArgumentNullException("valueType");
		}
		if (_Converters == null)
		{
			_Initialize();
			Custom.Log("Init value converter manually");
		}
		_Converters.TryGetValue(valueType, out var value);
		if (value == null)
		{
			if (valueType.IsEnum)
			{
				return _Converters[typeof(Enum)];
			}
			if (valueType.IsExtEnum())
			{
				return _Converters[typeof(ExtEnumBase)];
			}
		}
		return value;
	}

	public static bool CanConvert(Type type)
	{
		return GetConverter(type) != null;
	}

	public static IEnumerable<Type> GetSupportedTypes()
	{
		return _Converters.Keys;
	}

	private static string _Escape(this string txt)
	{
		if (string.IsNullOrEmpty(txt))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(txt.Length + 2);
		foreach (char c in txt)
		{
			switch (c)
			{
			case '\0':
				stringBuilder.Append("\\0");
				break;
			case '\a':
				stringBuilder.Append("\\a");
				break;
			case '\b':
				stringBuilder.Append("\\b");
				break;
			case '\t':
				stringBuilder.Append("\\t");
				break;
			case '\n':
				stringBuilder.Append("\\n");
				break;
			case '\v':
				stringBuilder.Append("\\v");
				break;
			case '\f':
				stringBuilder.Append("\\f");
				break;
			case '\r':
				stringBuilder.Append("\\r");
				break;
			case '\'':
				stringBuilder.Append("\\'");
				break;
			case '\\':
				stringBuilder.Append("\\");
				break;
			case '"':
				stringBuilder.Append("\\\"");
				break;
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	private static string _Unescape(this string txt)
	{
		if (string.IsNullOrEmpty(txt))
		{
			return txt;
		}
		StringBuilder stringBuilder = new StringBuilder(txt.Length);
		int num = 0;
		while (num < txt.Length)
		{
			int num2 = txt.IndexOf('\\', num);
			if (num2 < 0 || num2 == txt.Length - 1)
			{
				num2 = txt.Length;
			}
			stringBuilder.Append(txt, num, num2 - num);
			if (num2 >= txt.Length)
			{
				break;
			}
			char c = txt[num2 + 1];
			switch (c)
			{
			case '0':
				stringBuilder.Append('\0');
				break;
			case 'a':
				stringBuilder.Append('\a');
				break;
			case 'b':
				stringBuilder.Append('\b');
				break;
			case 't':
				stringBuilder.Append('\t');
				break;
			case 'n':
				stringBuilder.Append('\n');
				break;
			case 'v':
				stringBuilder.Append('\v');
				break;
			case 'f':
				stringBuilder.Append('\f');
				break;
			case 'r':
				stringBuilder.Append('\r');
				break;
			case '\'':
				stringBuilder.Append('\'');
				break;
			case '"':
				stringBuilder.Append('"');
				break;
			case '\\':
				stringBuilder.Append('\\');
				break;
			default:
				stringBuilder.Append('\\').Append(c);
				break;
			}
			num = num2 + 2;
		}
		return stringBuilder.ToString();
	}

	public static TypeCategory GetTypeCategory(Type type)
	{
		if (!CanConvert(type))
		{
			return TypeCategory.Unsupported;
		}
		return GetConverter(type).category;
	}

	public static TypeCategory GetTypeCategory<T>()
	{
		return GetTypeCategory(typeof(T));
	}
}
