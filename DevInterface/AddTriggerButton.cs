using UnityEngine;

namespace DevInterface;

public class AddTriggerButton : Button
{
	public EventTrigger.TriggerType triggerType;

	public AddTriggerButton(DevUI owner, DevUINode parentNode, Vector2 pos, float width, EventTrigger.TriggerType triggerType)
		: base(owner, "Add_Trigger_" + triggerType.ToString(), parentNode, pos, width, triggerType.ToString())
	{
		this.triggerType = triggerType;
	}

	public override void Clicked()
	{
		DevUINode devUINode = this;
		while (devUINode != null)
		{
			devUINode = devUINode.parentNode;
			if (devUINode is Page)
			{
				(devUINode as IDevUISignals).Signal(DevUISignalType.Create, this, "");
				break;
			}
		}
	}
}
