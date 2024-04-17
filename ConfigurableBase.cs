using System;
using Menu.Remix.MixedUI;

public abstract class ConfigurableBase
{
	public readonly OptionInterface OI;

	public readonly string key;

	public readonly Type settingType;

	public readonly string defaultValue;

	public readonly ConfigurableInfo info;

	public bool IsCosmetic
	{
		get
		{
			if (!string.IsNullOrEmpty(key))
			{
				return key.StartsWith("_");
			}
			return true;
		}
	}

	public UIconfig BoundUIconfig { get; internal set; }

	public string Tab
	{
		get
		{
			if (BoundUIconfig == null || BoundUIconfig.tab == null)
			{
				return "";
			}
			return BoundUIconfig.tab.name;
		}
	}

	public abstract object BoxedValue { get; set; }

	internal ConfigurableBase(OptionInterface OI, string key, Type settingType, string defaultValue, ConfigurableInfo info)
	{
		this.OI = OI;
		this.key = key;
		this.settingType = settingType;
		this.info = ((info != null) ? info : ConfigurableInfo.Empty);
		if (this.info.acceptable != null && !settingType.IsAssignableFrom(this.info.acceptable.ValueType))
		{
			throw new ArgumentException("info.acceptable is for a different type than the type of this config");
		}
		this.defaultValue = defaultValue;
		BoxedValue = defaultValue;
		BoundUIconfig = null;
	}

	protected T ClampValue<T>(T value)
	{
		if (info.acceptable != null)
		{
			return (T)info.acceptable.Clamp(value);
		}
		return value;
	}
}
