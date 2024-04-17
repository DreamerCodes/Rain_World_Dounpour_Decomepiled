namespace ScavengerCosmetic;

public abstract class Template : ComplexGraphicsModule.GraphicsSubModule
{
	public ScavengerGraphics scavGrphs => owner as ScavengerGraphics;

	public Template(ScavengerGraphics owner, int firstSprite)
		: base(owner, firstSprite)
	{
	}
}
