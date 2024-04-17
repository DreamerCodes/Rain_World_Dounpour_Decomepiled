using System.Text.RegularExpressions;
using UnityEngine;

namespace DevInterface;

public class InheritFromTemplateMenu : RectangularDevUINode
{
	public InheritFromTemplateMenu(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size)
		: base(owner, IDstring, parentNode, pos, size)
	{
		subNodes.Add(new DevUILabel(owner, IDstring + "_Heading", this, new Vector2(0f, 0f), 130f, "Inherit from Template:"));
		subNodes.Add(new Arrow(owner, IDstring + "_Arrow", this, new Vector2(20f, 0f), 90f));
		subNodes[1].fSprites[0].color = new Color(0f, 0f, 1f);
		float num = 1f;
		subNodes.Add(new MouseOverSwitchColorLabel(owner, IDstring + "_NONE", this, new Vector2(20f, (0f - num) * 20f), 100f, "NONE"));
		if (owner.room.world.region != null)
		{
			for (int i = 0; i < owner.room.world.region.roomSettingTemplateNames.Length; i++)
			{
				num += 1f;
				subNodes.Add(new MouseOverSwitchColorLabel(owner, IDstring + "_" + owner.room.world.region.roomSettingTemplateNames[i], this, new Vector2(20f, (0f - num) * 20f), 100f, owner.room.world.region.name + " - " + owner.room.world.region.roomSettingTemplateNames[i]));
			}
		}
		Move(pos);
	}

	public override void Update()
	{
		base.Update();
		if (!owner.mouseClick)
		{
			return;
		}
		for (int i = 2; i < subNodes.Count; i++)
		{
			if ((subNodes[i] as RectangularDevUINode).MouseOver)
			{
				base.RoomSettings.SetTemplate((subNodes[i] as DevUILabel).Text, owner.room.world.region);
				base.TopNode.Refresh();
				break;
			}
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		for (int i = 2; i < subNodes.Count; i++)
		{
			(subNodes[i] as MouseOverSwitchColorLabel).colorB = new Color(1f, 0f, 0f);
		}
		int num = 1;
		if (base.RoomSettings.parent != null && !base.RoomSettings.parent.isAncestor)
		{
			string text = Regex.Split(base.RoomSettings.parent.name, "_")[2];
			for (int j = 3; j < subNodes.Count; j++)
			{
				if (Regex.Split((subNodes[j] as MouseOverSwitchColorLabel).Text, " - ")[1] == text)
				{
					num = j - 1;
					break;
				}
			}
		}
		(subNodes[1] as Arrow).Move(new Vector2(5f, (float)(-num) * 20f + 3f));
		(subNodes[num + 1] as MouseOverSwitchColorLabel).colorB = new Color(0f, 0f, 1f);
	}
}
