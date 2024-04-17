using System.Collections.Generic;
using UnityEngine;

namespace DevInterface;

public abstract class DevUINode
{
	public DevUI owner;

	public DevUINode parentNode;

	public readonly string IDstring;

	public List<DevUINode> subNodes = new List<DevUINode>();

	public List<FSprite> fSprites = new List<FSprite>();

	public List<FLabel> fLabels = new List<FLabel>();

	private bool initRefresh = true;

	public RoomSettings RoomSettings => owner.room.roomSettings;

	public DevUINode TopNode
	{
		get
		{
			DevUINode devUINode = this;
			while (devUINode.parentNode != null)
			{
				devUINode = devUINode.parentNode;
			}
			return devUINode;
		}
	}

	public Page Page
	{
		get
		{
			DevUINode devUINode = this;
			while (devUINode.parentNode != null)
			{
				devUINode = devUINode.parentNode;
				if (devUINode is Page)
				{
					return devUINode as Page;
				}
			}
			return null;
		}
	}

	public DevUINode(DevUI owner, string IDstring, DevUINode parentNode)
	{
		this.owner = owner;
		this.IDstring = IDstring;
		this.parentNode = parentNode;
	}

	public virtual void Update()
	{
		for (int num = subNodes.Count - 1; num >= 0; num--)
		{
			subNodes[num].Update();
		}
		if (initRefresh)
		{
			Refresh();
			initRefresh = false;
		}
	}

	public virtual void ClearSprites()
	{
		for (int num = subNodes.Count - 1; num >= 0; num--)
		{
			subNodes[num].ClearSprites();
		}
		for (int i = 0; i < fSprites.Count; i++)
		{
			fSprites[i].RemoveFromContainer();
		}
		fSprites.Clear();
		for (int j = 0; j < fLabels.Count; j++)
		{
			fLabels[j].RemoveFromContainer();
		}
		fLabels.Clear();
	}

	public void MoveSprite(int sprite, Vector2 newPos)
	{
		if (sprite < fSprites.Count && sprite >= 0)
		{
			fSprites[sprite].x = newPos.x + 0.01f;
			fSprites[sprite].y = newPos.y + 0.01f;
		}
	}

	public void MoveLabel(int label, Vector2 newPos)
	{
		if (label < fLabels.Count && label >= 0)
		{
			fLabels[label].x = newPos.x + 0.01f;
			fLabels[label].y = newPos.y + 0.01f;
		}
	}

	public virtual void Refresh()
	{
		for (int num = subNodes.Count - 1; num >= 0; num--)
		{
			subNodes[num].Refresh();
		}
	}
}
