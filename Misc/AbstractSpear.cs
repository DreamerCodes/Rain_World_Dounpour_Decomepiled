using System;
using System.Globalization;

public class AbstractSpear : AbstractPhysicalObject
{
	public int stuckInWallCycles;

	public bool explosive;

	public bool stuckVertically;

	public float hue;

	public bool electric;

	public int electricCharge;

	public bool needle;

	public bool stuckInWall => stuckInWallCycles != 0;

	public bool onPlayerBack
	{
		get
		{
			if (realizedObject != null)
			{
				return (realizedObject as Spear).onPlayerBack;
			}
			return false;
		}
	}

	public AbstractSpear(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, bool explosive)
		: base(world, AbstractObjectType.Spear, realizedObject, pos, ID)
	{
		this.explosive = explosive;
	}

	public AbstractSpear(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, bool explosive, float hue)
		: this(world, realizedObject, pos, ID, explosive)
	{
		this.hue = hue;
	}

	public AbstractSpear(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, bool explosive, bool electric)
		: this(world, realizedObject, pos, ID, explosive)
	{
		this.electric = electric;
		if (this.electric)
		{
			electricCharge = 3;
		}
	}

	public void StuckInWallTick(int ticks)
	{
		if (stuckInWallCycles > 0)
		{
			stuckInWallCycles = Math.Max(0, stuckInWallCycles - ticks);
		}
		else if (stuckInWallCycles < 0)
		{
			stuckInWallCycles = Math.Min(0, stuckInWallCycles + ticks);
		}
	}

	public override string ToString()
	{
		string text = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}", ID.ToString(), type.ToString(), pos.SaveToString(), stuckInWallCycles, explosive ? "1" : "0");
		if (ModManager.MSC)
		{
			text += string.Format(CultureInfo.InvariantCulture, "<oA>{0}<oA>{1}<oA>{2}<oA>{3}", hue.ToString(), electric ? "1" : "0", electricCharge.ToString(), needle ? "1" : "0");
		}
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", unrecognizedAttributes);
	}
}
