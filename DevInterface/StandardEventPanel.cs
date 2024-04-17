using UnityEngine;

namespace DevInterface;

public class StandardEventPanel : Panel
{
	public TriggerPanel triggerPanel;

	public TriggeredEvent tEvent => triggerPanel.trigger.tEvent;

	public StandardEventPanel(DevUI owner, DevUINode parentNode, float height)
		: base(owner, (parentNode as TriggerPanel).trigger.tEvent.type.ToString() + "_Panel", parentNode, new Vector2(0f, 0f), new Vector2(245f, height), "Event : " + (parentNode as TriggerPanel).trigger.tEvent.type.ToString())
	{
		triggerPanel = parentNode as TriggerPanel;
		subNodes.Add(new DevUILabel(owner, (parentNode as TriggerPanel).trigger.tEvent.type.ToString(), this, new Vector2(5f, size.y - 16f - 5f), size.x - 10f, (parentNode as TriggerPanel).trigger.tEvent.type.ToString()));
		Move(new Vector2(0f, 0f - size.y - 20f));
	}
}
