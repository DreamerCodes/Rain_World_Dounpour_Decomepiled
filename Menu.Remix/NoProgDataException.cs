using System;

namespace Menu.Remix;

public class NoProgDataException : InvalidOperationException
{
	public NoProgDataException(OptionInterface oi)
		: base("OptionInterface " + oi.mod.id + " hasn't enabled hasProgData")
	{
	}
}
