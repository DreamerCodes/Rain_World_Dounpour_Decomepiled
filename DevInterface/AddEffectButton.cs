using UnityEngine;

namespace DevInterface;

public class AddEffectButton : Button
{
	public RoomSettings.RoomEffect.Type type;

	public AddEffectButton(DevUI owner, DevUINode parentNode, Vector2 pos, float width, RoomSettings.RoomEffect.Type type)
		: base(owner, "Add_Effect_" + type.ToString(), parentNode, pos, width, type.ToString())
	{
		this.type = type;
	}

	public override void Clicked()
	{
		DevUINode devUINode = this;
		while (devUINode != null)
		{
			devUINode = devUINode.parentNode;
			if (devUINode is Page)
			{
				(devUINode as IDevUISignals).Signal(DevUISignalType.Create, this, type.ToString());
				break;
			}
		}
	}
}
