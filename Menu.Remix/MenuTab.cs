using Menu.Remix.MixedUI;

namespace Menu.Remix;

public class MenuTab : OpTab
{
	protected internal FContainer myContainer => _container;

	internal MenuTab()
		: base(null)
	{
		_container.x -= OpTab._offset.x;
		_container.y -= OpTab._offset.y;
		_container.isVisible = true;
		base.isInactive = false;
	}
}
