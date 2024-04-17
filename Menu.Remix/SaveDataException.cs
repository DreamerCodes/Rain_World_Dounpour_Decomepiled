using System;
using System.Runtime.Serialization;

namespace Menu.Remix;

[Serializable]
public class SaveDataException : Exception
{
	public SaveDataException(string message)
		: base(message)
	{
	}

	public SaveDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public SaveDataException()
	{
	}

	protected SaveDataException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new NotImplementedException();
	}
}
