using System;

namespace Menu.Remix;

public class InvalidSlugcatException : ArgumentException
{
	public InvalidSlugcatException(OptionInterface oi)
		: base("OptionInterface " + oi.mod.id + " tried to use an invalid Slugcat number")
	{
	}
}
