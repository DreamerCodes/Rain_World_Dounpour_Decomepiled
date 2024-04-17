namespace MoreSlugcats;

public class MMFPreset<T>
{
	public Configurable<T> config;

	public T remixValue;

	public T classicValue;

	public T casualValue;

	public MMFPreset(Configurable<T> config, T remixValue, T classicValue, T casualValue)
	{
		this.config = config;
		this.remixValue = remixValue;
		this.classicValue = classicValue;
		this.casualValue = casualValue;
	}
}
