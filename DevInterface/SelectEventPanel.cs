using UnityEngine;

namespace DevInterface;

public class SelectEventPanel : Panel, IDevUISignals
{
	private TriggerPanel triggerPanel;

	public SelectEventPanel(DevUI owner, DevUINode parentNode, Vector2 pos)
		: base(owner, "Select_Event_Panel", parentNode, pos, new Vector2(245f, 160f), "Select an event type")
	{
		triggerPanel = parentNode as TriggerPanel;
		for (int i = 0; i < ExtEnum<TriggeredEvent.EventType>.values.Count; i++)
		{
			string entry = ExtEnum<TriggeredEvent.EventType>.values.GetEntry(i);
			subNodes.Add(new Button(owner, entry, this, new Vector2(5f, size.y - 25f - 20f * (float)i), 235f, entry));
		}
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		triggerPanel.AddEvent(new TriggeredEvent.EventType(sender.IDstring));
	}
}
