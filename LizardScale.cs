using LizardCosmetics;

public class LizardScale : BodyPart
{
	public Template lCosmetics;

	public float length;

	public float width;

	public LizardScale(Template lCosmetics)
		: base(lCosmetics.lGraphics)
	{
		this.lCosmetics = lCosmetics;
	}

	public override void Update()
	{
		base.Update();
		if (owner.owner.room.PointSubmerged(pos))
		{
			vel *= 0.5f;
		}
		else
		{
			vel *= 0.9f;
		}
		lastPos = pos;
		pos += vel;
	}
}
