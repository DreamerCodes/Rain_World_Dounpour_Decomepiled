namespace DevInterface;

public class DevUISignalType : ExtEnum<DevUISignalType>
{
	public static readonly DevUISignalType ButtonClick = new DevUISignalType("ButtonClick", register: true);

	public static readonly DevUISignalType Create = new DevUISignalType("Create", register: true);

	public DevUISignalType(string value, bool register = false)
		: base(value, register)
	{
	}
}
