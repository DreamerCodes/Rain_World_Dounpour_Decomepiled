using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DevInterface;

public abstract class Page : DevUINode, IDevUISignals
{
	public string name;

	public List<DevUINode> tempNodes;

	public Page(DevUI owner, string IDstring, DevUINode parentNode, string name)
		: base(owner, IDstring, null)
	{
		this.name = name;
		subNodes.Add(new DevUILabel(owner, "Page_Name :" + name, null, new Vector2(100f, 690f), 100f, name));
		subNodes.Add(new DevUILabel(owner, "Switch_Page_Label", null, new Vector2(100f, 750f), 100f, "PAGES"));
		if (owner != null)
		{
			for (int i = 0; i < owner.pages.Length; i++)
			{
				subNodes.Add(new SwitchPageButton(owner, this, new Vector2(100f + 100f * (float)i, 730f), 95f, i));
			}
		}
		subNodes.Add(new Button(owner, "Save_Settings", this, new Vector2(900f, 730f), 100f, "Save"));
		if (owner != null && owner.game != null && owner.game.IsArenaSession && owner.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == ArenaSetup.GameTypeID.Sandbox)
		{
			subNodes.Add(new Button(owner, "Export_Sandbox", this, new Vector2(1005f, 730f), 100f, "Export Sandbox"));
		}
		if (owner != null && owner.game != null && owner.game.IsStorySession)
		{
			if (File.Exists(base.RoomSettings.SpecificPath((base.owner.game.session as StoryGameSession).saveState.saveStateNumber)))
			{
				subNodes.Add(new DevUILabel(owner, "Specific_Label", null, new Vector2(790f, 730f), 100f, "Using Specific!"));
			}
			else
			{
				subNodes.Add(new Button(owner, "Save_Specific", this, new Vector2(790f, 730f), 100f, "Create Specific"));
			}
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		if (tempNodes != null)
		{
			for (int i = 0; i < tempNodes.Count; i++)
			{
				tempNodes[i].ClearSprites();
				subNodes.Remove(tempNodes[i]);
			}
		}
		tempNodes = new List<DevUINode>();
	}

	public virtual void Signal(DevUISignalType type, DevUINode sender, string message)
	{
	}
}
