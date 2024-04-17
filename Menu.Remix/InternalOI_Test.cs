namespace Menu.Remix;

internal class InternalOI_Test : InternalOI
{
	public InternalOI_Test()
		: base(new ModManager.Mod
		{
			id = "_TestDummy_",
			name = "_TestDummy_",
			authors = "topicular",
			description = "Internal OI for testing UIelements of Config Machine",
			enabled = false
		}, Reason.TestOI)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		config.configurables.Clear();
	}
}
