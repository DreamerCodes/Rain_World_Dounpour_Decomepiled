using System;
using System.Runtime.Serialization;
using Menu.Remix.MixedUI;

namespace Menu.Remix;

[Serializable]
public class ElementFormatException : ArgumentException
{
	public ElementFormatException(UIelement element, string message, string key = "")
		: base(element.GetType().Name + " threw exception : " + message + (string.IsNullOrEmpty(key) ? string.Empty : (" (Key : " + key + ")")))
	{
	}

	public ElementFormatException(string message)
		: base("Invalid argument for UIelement ctor : " + message)
	{
	}

	public ElementFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected ElementFormatException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
	}

	public ElementFormatException()
		: base("One of UIelement threw exception for Invalid arguments!")
	{
	}
}
