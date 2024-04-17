using System;
using System.Collections.Generic;
using Menu.Remix;
using Menu.Remix.MixedUI;

public sealed class Configurable<T> : ConfigurableBase
{
	private T _typedValue;

	public T Value
	{
		get
		{
			return _typedValue;
		}
		set
		{
			value = ClampValue(value);
			if (EqualityComparer<T>.Default.Equals(_typedValue, value))
			{
				return;
			}
			_typedValue = value;
			try
			{
				this.OnChange?.Invoke();
			}
			catch (Exception msg)
			{
				MachineConnector.LogError(msg);
			}
		}
	}

	public override object BoxedValue
	{
		get
		{
			return Value;
		}
		set
		{
			Value = ((value.GetType() == typeof(string)) ? ValueConverter.ConvertToValue<T>((string)value) : ((T)value));
		}
	}

	public event OnEventHandler OnChange;

	internal Configurable(OptionInterface oi, string key, T defaultValue, ConfigurableInfo info)
		: base(oi, key, typeof(T), ValueConverter.ConvertToString(defaultValue, typeof(T)), info)
	{
		if (oi != null)
		{
			oi.OnConfigChanged += this.OnChange;
		}
	}

	public Configurable(T defaultValue, ConfigurableInfo info = null)
		: this((OptionInterface)null, "_", defaultValue, info)
	{
	}

	public Configurable(T defaultValue, ConfigAcceptableBase accept)
		: this(defaultValue, new ConfigurableInfo("", accept, ""))
	{
	}
}
