using System;
using System.Runtime.Serialization;

namespace Menu.Remix;

[Serializable]
public class NoTabException : FormatException
{
	public NoTabException(string modID)
		: base("NoTabException: " + modID + " OI has No OpTabs! " + "\r\n" + "Did you put base.Initialize() after your code?" + "\r\n" + "Leaving OI.Initialize() completely blank will prevent the mod from using LoadData/SaveData.")
	{
	}

	public NoTabException()
	{
	}

	public NoTabException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NoTabException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new NotImplementedException();
	}
}
