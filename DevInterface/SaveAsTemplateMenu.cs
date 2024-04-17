using UnityEngine;

namespace DevInterface;

public class SaveAsTemplateMenu : RectangularDevUINode, IDevUISignals
{
	public SaveAsTemplateMenu(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size)
		: base(owner, IDstring, parentNode, pos, size)
	{
		subNodes.Add(new DevUILabel(owner, IDstring + "_Heading", this, new Vector2(0f, 0f), 130f, "Save as Template:"));
		float num = 0f;
		for (int i = 0; i < owner.room.world.region.roomSettingTemplateNames.Length; i++)
		{
			num += 1f;
			subNodes.Add(new Button(owner, IDstring + "_" + owner.room.world.region.roomSettingTemplateNames[i], this, new Vector2(20f, (0f - num) * 20f), 100f, owner.room.world.region.name + " - " + owner.room.world.region.roomSettingTemplateNames[i]));
		}
		Move(pos);
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		base.RoomSettings.SaveAsTemplate((sender as DevUILabel).Text, owner.room.world.region);
		base.TopNode.Refresh();
	}
}
