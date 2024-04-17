using System;

public class ConfigAcceptableRange<T> : ConfigAcceptableBase where T : IComparable
{
	public virtual T MinValue { get; private set; }

	public virtual T MaxValue { get; private set; }

	public ConfigAcceptableRange(T minValue, T maxValue)
		: base(typeof(T))
	{
		if (maxValue == null)
		{
			throw new ArgumentNullException("maxValue");
		}
		if (minValue == null)
		{
			throw new ArgumentNullException("minValue");
		}
		if (minValue.CompareTo(maxValue) >= 0)
		{
			throw new ArgumentException("minValue has to be lower than maxValue");
		}
		MinValue = minValue;
		MaxValue = maxValue;
	}

	public override object Clamp(object value)
	{
		if (MinValue.CompareTo(value) > 0)
		{
			return MinValue;
		}
		if (MaxValue.CompareTo(value) < 0)
		{
			return MaxValue;
		}
		return value;
	}

	public override bool IsValid(object value)
	{
		if (MinValue.CompareTo(value) <= 0)
		{
			return MaxValue.CompareTo(value) >= 0;
		}
		return false;
	}

	public override string ToDescriptionString()
	{
		return string.Concat("# Acceptable value range: From ", MinValue, " to ", MaxValue);
	}
}
