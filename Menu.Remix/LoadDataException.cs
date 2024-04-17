using System;
using System.Runtime.Serialization;

namespace Menu.Remix;

[Serializable]
public class LoadDataException : Exception
{
	public LoadDataException(string message)
		: base(message)
	{
	}

	public LoadDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public LoadDataException()
	{
	}

	protected LoadDataException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new NotImplementedException();
	}
}
