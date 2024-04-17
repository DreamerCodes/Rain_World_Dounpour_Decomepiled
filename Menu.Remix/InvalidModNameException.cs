using System;
using System.Runtime.Serialization;

namespace Menu.Remix;

[Serializable]
public class InvalidModNameException : FormatException
{
	public InvalidModNameException(string modID)
		: base(modID + " is invalid ModID! Use something that can be used as folder name!")
	{
	}

	public InvalidModNameException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public InvalidModNameException()
	{
	}

	protected InvalidModNameException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new NotImplementedException();
	}
}
