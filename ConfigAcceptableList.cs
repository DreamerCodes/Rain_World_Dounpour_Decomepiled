using System;
using System.Linq;

public class ConfigAcceptableList<T> : ConfigAcceptableBase where T : IEquatable<T>
{
	public virtual T[] AcceptableValues { get; private set; }

	public ConfigAcceptableList(params T[] acceptableValues)
		: base(typeof(T))
	{
		if (acceptableValues == null)
		{
			throw new ArgumentNullException("acceptableValues");
		}
		if (acceptableValues.Length == 0)
		{
			throw new ArgumentException("At least one acceptable value is needed");
		}
		AcceptableValues = acceptableValues;
	}

	public override object Clamp(object value)
	{
		if (IsValid(value))
		{
			return value;
		}
		return AcceptableValues[0];
	}

	public override bool IsValid(object value)
	{
		if (value is T)
		{
			return AcceptableValues.Any((T x) => x.Equals((T)value));
		}
		return false;
	}

	public override string ToDescriptionString()
	{
		return "# Acceptable values: " + string.Join(", ", AcceptableValues.Select((T x) => x.ToString()).ToArray());
	}
}
