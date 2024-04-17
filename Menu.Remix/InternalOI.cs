using Menu.Remix.MixedUI;

namespace Menu.Remix;

public abstract class InternalOI : OptionInterface
{
	public enum Reason
	{
		NoInterface,
		NoMod,
		Statistics,
		TestOI
	}

	public Reason reason;

	protected OpLabel labelID;

	protected OpLabel labelVersion;

	protected OpLabel labelAuthor;

	protected OpLabel labelSluggo0;

	protected OpLabel labelSluggo1;

	internal InternalOI(ModManager.Mod rwMod, Reason type)
		: base(rwMod)
	{
		reason = type;
	}

	public override void Initialize()
	{
		Tabs = new OpTab[1];
		Tabs[0] = new OpTab(this);
	}
}
