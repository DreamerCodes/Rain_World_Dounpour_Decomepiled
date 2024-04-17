namespace JollyCoop;

internal struct LogElement
{
	public string logText;

	public bool shouldThrow;

	public LogElement(string logText, bool shouldThrow)
	{
		this.logText = logText;
		this.shouldThrow = shouldThrow;
	}
}
