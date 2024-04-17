public class ConfigurableInfo
{
	public readonly string description;

	public readonly string autoTab;

	public readonly ConfigAcceptableBase acceptable;

	public readonly object[] Tags;

	public static ConfigurableInfo Empty => new ConfigurableInfo("", null, "");

	public ConfigurableInfo(string description, ConfigAcceptableBase acceptable = null, string autoTab = "", params object[] tags)
	{
		this.acceptable = acceptable;
		this.autoTab = autoTab;
		Tags = tags;
		this.description = (string.IsNullOrEmpty(description) ? "" : description);
	}
}
