using System.Collections.Generic;
using System.IO;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class TriggersPage : Page
{
	public DevUINode draggedObject;

	public DevUINode removeIfReleaseObject;

	private TrashBin trashBin;

	public string[] songNames;

	private Panel triggersPanel;

	public TriggersPage(DevUI owner, string IDstring, DevUINode parentNode, string name)
		: base(owner, IDstring, parentNode, name)
	{
		triggersPanel = new Panel(owner, "Triggers_Panel", this, new Vector2(1050f, 20f), new Vector2(300f, 100f), "CREATE TRIGGER");
		for (int i = 0; i < ExtEnum<EventTrigger.TriggerType>.values.Count; i++)
		{
			triggersPanel.subNodes.Add(new AddTriggerButton(owner, triggersPanel, new Vector2(5f, triggersPanel.size.y - 16f - 5f - (float)(i * 20)), 260f, new EventTrigger.TriggerType(ExtEnum<EventTrigger.TriggerType>.values.GetEntry(i))));
		}
		subNodes.Add(triggersPanel);
		trashBin = new TrashBin(owner, "Trash_Bin", this, new Vector2(40f, 40f));
		subNodes.Add(trashBin);
		DirectoryInfo directoryInfo = null;
		FileInfo[] files = ((!Directory.Exists("./Assets/Resources/Music/Songs")) ? new DirectoryInfo(AssetManager.ResolveDirectory("Music" + Path.DirectorySeparatorChar + "Songs")) : new DirectoryInfo("./Assets/Resources/Music/Songs")).GetFiles();
		List<string> list = new List<string>();
		for (int j = 0; j < files.Length; j++)
		{
			if (!files[j].Name.EndsWith(".meta"))
			{
				list.Add(Path.GetFileNameWithoutExtension(files[j].Name));
			}
		}
		songNames = list.ToArray();
	}

	public override void Update()
	{
		draggedObject = null;
		base.Update();
		if (draggedObject != null && trashBin.MouseOver)
		{
			trashBin.LineColor = ((Random.value < 0.5f) ? new Color(1f, 0f, 0f) : new Color(1f, 1f, 1f));
			removeIfReleaseObject = draggedObject;
			return;
		}
		trashBin.LineColor = new Color(1f, 1f, 1f);
		if (!owner.mouseDown && removeIfReleaseObject != null)
		{
			RemoveObject(removeIfReleaseObject);
		}
		removeIfReleaseObject = null;
	}

	public override void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (type == DevUISignalType.ButtonClick)
		{
			switch (sender.IDstring)
			{
			case "Save_Settings":
				base.RoomSettings.Save();
				break;
			case "Export_Sandbox":
				(owner.game.GetArenaGameSession as SandboxGameSession).editor.DevToolsExportConfig();
				break;
			case "Save_Specific":
				base.RoomSettings.Save(owner.game.GetStorySession.saveStateNumber);
				break;
			}
		}
		else if (type == DevUISignalType.Create)
		{
			CreateTriggerRep((sender as AddTriggerButton).triggerType);
		}
	}

	private void RemoveObject(DevUINode objRep)
	{
		if (objRep is TriggerPanel)
		{
			base.RoomSettings.triggers.Remove((objRep as TriggerPanel).trigger);
		}
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();
		for (int i = 0; i < base.RoomSettings.triggers.Count; i++)
		{
			TriggerPanel triggerPanel = new TriggerPanel(owner, this, base.RoomSettings.triggers[i].panelPosition, base.RoomSettings.triggers[i]);
			triggerPanel.Move(base.RoomSettings.triggers[i].panelPosition);
			tempNodes.Add(triggerPanel);
			subNodes.Add(triggerPanel);
		}
	}

	private void CreateTriggerRep(EventTrigger.TriggerType tp)
	{
		Vector2 vector = Vector2.Lerp(owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f + new Vector2(40f, 40f);
		EventTrigger eventTrigger;
		if (tp == EventTrigger.TriggerType.Spot)
		{
			eventTrigger = new SpotTrigger();
			(eventTrigger as SpotTrigger).pos = vector + owner.room.game.cameras[0].pos;
		}
		else
		{
			eventTrigger = new EventTrigger(tp);
		}
		if (eventTrigger != null)
		{
			base.RoomSettings.triggers.Add(eventTrigger);
			eventTrigger.panelPosition = vector;
		}
		Refresh();
	}
}
