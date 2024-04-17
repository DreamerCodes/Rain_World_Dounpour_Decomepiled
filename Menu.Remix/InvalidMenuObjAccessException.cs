using System;
using System.Runtime.Serialization;

namespace Menu.Remix;

[Serializable]
public class InvalidMenuObjAccessException : NullReferenceException
{
	public InvalidMenuObjAccessException(string message)
		: base(message)
	{
	}

	public InvalidMenuObjAccessException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public InvalidMenuObjAccessException()
		: base("If you are accessing MenuObject in UIelements, make sure those don't run when 'isOptionMenu' is 'false'.")
	{
	}

	public InvalidMenuObjAccessException(Exception ex)
		: base("If you are accessing MenuObject in UIelements, make sure those don't run when 'isOptionMenu' is 'false'." + "\r\n" + ex.ToString())
	{
	}

	protected InvalidMenuObjAccessException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new NotImplementedException();
	}
}
