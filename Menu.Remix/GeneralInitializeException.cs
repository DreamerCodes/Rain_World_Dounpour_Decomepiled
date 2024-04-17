using System;
using System.Runtime.Serialization;

namespace Menu.Remix;

[Serializable]
public class GeneralInitializeException : FormatException
{
	public GeneralInitializeException(Exception ex)
		: base("GeneralInitializeException: OI had a problem in Initialize!" + "\r\n" + OptionalText.GetText(OptionalText.ID.OIError_GeneralAdvice) + "\r\n" + ex)
	{
	}

	public GeneralInitializeException(string message)
		: base("GeneralInitializeException: OI had a problem in Initialize!" + "\r\n" + OptionalText.GetText(OptionalText.ID.OIError_GeneralAdvice) + "\r\n" + message)
	{
	}

	public GeneralInitializeException()
	{
	}

	public GeneralInitializeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected GeneralInitializeException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new NotImplementedException();
	}
}
