using UnityEngine;

namespace DevInterface;

public class DangerTypeCycler : Button
{
	public DangerTypeCycler(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width)
		: base(owner, IDstring, parentNode, pos, width, "")
	{
		subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(-50f, 0f), 40f, "G.O: "));
	}

	public override void Refresh()
	{
		base.Refresh();
		string text = "";
		text = ((base.RoomSettings.dType != null) ? "" : ((base.RoomSettings.parent.isAncestor || !(base.RoomSettings.parent.dType != null)) ? "<A>" : "<T>"));
		base.Text = text + " " + base.RoomSettings.DangerType;
	}

	public override void Clicked()
	{
		if (base.RoomSettings.dType == null)
		{
			base.RoomSettings.dType = new RoomRain.DangerType(ExtEnum<RoomRain.DangerType>.values.GetEntry(0));
		}
		else if (base.RoomSettings.dType == new RoomRain.DangerType(ExtEnum<RoomRain.DangerType>.values.GetEntry(ExtEnum<RoomRain.DangerType>.values.Count - 1)))
		{
			base.RoomSettings.dType = null;
		}
		else
		{
			base.RoomSettings.dType = new RoomRain.DangerType(ExtEnum<RoomRain.DangerType>.values.GetEntry(base.RoomSettings.dType.Index + 1));
		}
		Refresh();
	}
}
