using System;
using System.Runtime.Serialization;

namespace Menu.Remix;

[Serializable]
public class DupelicateKeyException : FormatException
{
	public DupelicateKeyException(string tab, string key)
		: base((string.IsNullOrEmpty(tab) ? "Tab" : "Tab ") + tab + " has duplicated key for UIconfig." + "\r\n" + "(dupe key: " + key + ")")
	{
	}

	public DupelicateKeyException()
	{
	}

	public DupelicateKeyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected DupelicateKeyException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new NotImplementedException();
	}

	public DupelicateKeyException(string message)
		: base(message)
	{
	}
}
