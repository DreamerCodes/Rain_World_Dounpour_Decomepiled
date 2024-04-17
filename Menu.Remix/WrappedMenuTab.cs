namespace Menu.Remix;

public class WrappedMenuTab : MenuTab
{
	public readonly MenuTabWrapper wrapper;

	internal WrappedMenuTab(MenuTabWrapper wrapper)
	{
		this.wrapper = wrapper;
		this.wrapper.Container.AddChild(_container);
	}
}
