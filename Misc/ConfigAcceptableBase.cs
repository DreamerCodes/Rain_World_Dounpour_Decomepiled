using System;

public abstract class ConfigAcceptableBase
{
	public readonly Type ValueType;

	protected ConfigAcceptableBase(Type valueType)
	{
		ValueType = valueType;
	}

	public abstract object Clamp(object value);

	public abstract bool IsValid(object value);

	public abstract string ToDescriptionString();
}
