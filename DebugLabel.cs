using RWCustom;
using UnityEngine;

public class DebugLabel
{
	public PhysicalObject obj;

	public FLabel label;

	public Vector2 pos;

	public bool relativePos = true;

	public DebugLabel(PhysicalObject o, Vector2 ps)
	{
		obj = o;
		pos = ps;
		label = new FLabel(Custom.GetFont(), "0");
		label.alignment = FLabelAlignment.Left;
		label.color = new Color(1f, 0f, 1f);
	}
}
