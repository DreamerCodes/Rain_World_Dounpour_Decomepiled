using System.Globalization;

namespace MoreSlugcats;

public class AbstractBullet : AbstractPhysicalObject
{
	public JokeRifle.AbstractRifle.AmmoType bulletType;

	public int timeToLive;

	public AbstractBullet(World world, Bullet realizedObject, WorldCoordinate pos, EntityID ID, JokeRifle.AbstractRifle.AmmoType type, int timeToLive)
		: base(world, MoreSlugcatsEnums.AbstractObjectType.Bullet, realizedObject, pos, ID)
	{
		if (type != null)
		{
			bulletType = type;
		}
		this.timeToLive = timeToLive;
	}

	public void SetBulletType(JokeRifle.AbstractRifle.AmmoType type)
	{
		bulletType = type;
	}

	public override string ToString()
	{
		string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}", ID.ToString(), type.ToString(), pos.SaveToString(), bulletType.ToString(), timeToLive.ToString());
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
	}
}
