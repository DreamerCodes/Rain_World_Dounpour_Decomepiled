using RWCustom;
using UnityEngine;

namespace DevInterface;

public class PlacedObjectRepresentation : Handle
{
	public PlacedObject pObj;

	public string Name
	{
		get
		{
			return fLabels[0].text;
		}
		set
		{
			fLabels[0].text = value;
		}
	}

	public PlacedObjectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj.pos)
	{
		this.pObj = pObj;
		fLabels.Add(new FLabel(Custom.GetFont(), name));
		owner.placedObjectsContainer.AddChild(fLabels[0]);
		if (pObj.pos == new Vector2(0f, 0f))
		{
			pObj.pos = pos + owner.game.cameras[0].pos;
		}
	}

	public override void Update()
	{
		base.Update();
		if (dragged)
		{
			if ((base.Page as ObjectsPage).draggedObject == null)
			{
				(base.Page as ObjectsPage).draggedObject = this;
				Refresh();
				pObj.pos = pos + owner.game.cameras[0].pos;
			}
			else
			{
				dragged = false;
			}
		}
		if (!dragged)
		{
			AbsMove(pObj.pos - owner.game.cameras[0].pos);
		}
	}

	public override void SetColor(Color col)
	{
		base.SetColor(col);
		fLabels[0].color = col;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveLabel(0, absPos + new Vector2(20f, 20f));
	}
}
