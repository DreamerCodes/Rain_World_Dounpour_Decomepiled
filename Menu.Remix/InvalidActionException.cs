using System;
using System.Runtime.Serialization;
using Menu.Remix.MixedUI;

namespace Menu.Remix;

[Serializable]
public class InvalidActionException : InvalidOperationException
{
	public InvalidActionException(string message)
		: base(message)
	{
	}

	public InvalidActionException(UIelement element, string message, string key = "")
		: base(element.GetType().Name + " threw exception : " + message + (string.IsNullOrEmpty(key) ? string.Empty : (" (Key : " + key + ")")))
	{
	}

	public InvalidActionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public InvalidActionException()
	{
	}

	protected InvalidActionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new NotImplementedException();
	}
}
