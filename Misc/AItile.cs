using System.Collections.Generic;
using RWCustom;

public class AItile
{
	public enum Accessibility
	{
		OffScreen,
		Floor,
		Corridor,
		Climb,
		Wall,
		Ceiling,
		Air,
		Solid
	}

	public Accessibility acc;

	public List<MovementConnection> incomingPaths;

	public List<MovementConnection> outgoingPaths;

	public bool walkable;

	public bool narrowSpace;

	public int floorAltitude = 100000;

	public int smoothedFloorAltitude = 100000;

	private int waterInt;

	public int visibility;

	public IntVector2 fallRiskTile;

	public bool DeepWater => waterInt == 1;

	public bool WaterSurface => waterInt == 2;

	public bool AnyWater => waterInt != 0;

	public AItile(Accessibility a, int waterInt)
	{
		acc = a;
		walkable = acc != Accessibility.Air && acc != Accessibility.Solid;
		incomingPaths = new List<MovementConnection>();
		outgoingPaths = new List<MovementConnection>();
		this.waterInt = waterInt;
	}
}
