using System;
using System.Runtime.Serialization;

namespace Menu.Remix;

[Serializable]
public class NullModException : NullReferenceException
{
	public NullModException(string id)
		: base("OptionInterface.rwMod is null! (id: " + id + "})")
	{
	}

	public NullModException()
	{
	}

	public NullModException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NullModException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new NotImplementedException();
	}
}
