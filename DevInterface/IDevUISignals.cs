namespace DevInterface;

public interface IDevUISignals
{
	void Signal(DevUISignalType type, DevUINode sender, string message);
}
