using System;

namespace Menu.Remix.MixedUI;

public interface ICanBeTyped
{
	Action<char> OnKeyDown { get; set; }
}
