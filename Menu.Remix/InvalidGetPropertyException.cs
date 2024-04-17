using System;
using System.Runtime.Serialization;
using Menu.Remix.MixedUI;

namespace Menu.Remix;

[Serializable]
public class InvalidGetPropertyException : FormatException
{
	public InvalidGetPropertyException(OptionInterface oi, string name)
		: base(oi.mod.id + " called " + name + " eventhough its progData is false!")
	{
	}

	public InvalidGetPropertyException(UIelement element, string name)
		: base(string.Concat(element, (element is UIconfig) ? ("(key: " + (element as UIconfig).Key + ")") : string.Empty, " called ", name, " which is Invalid!"))
	{
	}

	public InvalidGetPropertyException(string message)
		: base("NoProgDataException: " + message)
	{
	}

	public InvalidGetPropertyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public InvalidGetPropertyException()
		: base("Invalid property called eventhough its progData is false")
	{
	}

	protected InvalidGetPropertyException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
	}
}
