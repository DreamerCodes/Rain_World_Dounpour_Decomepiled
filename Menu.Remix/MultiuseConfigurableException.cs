using System;

namespace Menu.Remix;

public class MultiuseConfigurableException : ArgumentException
{
	public MultiuseConfigurableException(ConfigurableBase config)
		: base("Configurable " + config.key + " is bound to multiple UIconfigs")
	{
	}
}
