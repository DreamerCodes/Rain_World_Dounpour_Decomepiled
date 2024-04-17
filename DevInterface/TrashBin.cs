using RWCustom;
using UnityEngine;

namespace DevInterface;

public class TrashBin : LineRect
{
	public TrashBin(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
		: base(owner, IDstring, parentNode, pos, new Vector2(40f, 40f))
	{
		fLabels.Add(new FLabel(Custom.GetFont(), "Trash Bin"));
		Futile.stage.AddChild(fLabels[0]);
		fLabels[0].color = new Color(1f, 0f, 0f);
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveLabel(0, absPos + new Vector2(10f, size.y + 10f));
	}
}
