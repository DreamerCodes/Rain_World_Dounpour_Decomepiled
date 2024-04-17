using RWCustom;
using UnityEngine;

namespace DevInterface;

public class QuadObjectRepresentation : PlacedObjectRepresentation
{
	public QuadObjectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rect_Handle", this, new Vector2(0f, 40f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.QuadObjectData).handles[0];
		subNodes.Add(new Handle(owner, "Rect_Handle", this, new Vector2(40f, 40f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.QuadObjectData).handles[1];
		subNodes.Add(new Handle(owner, "Rect_Handle", this, new Vector2(40f, 0f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.QuadObjectData).handles[2];
		for (int i = 0; i < 4; i++)
		{
			fSprites.Add(new FSprite("pixel"));
			owner.placedObjectsContainer.AddChild(fSprites[1 + i]);
			fSprites[1 + i].anchorY = 0f;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		for (int i = 0; i < 3; i++)
		{
			(pObj.data as PlacedObject.QuadObjectData).handles[i] = (subNodes[i] as Handle).pos;
		}
		MoveSprite(1, absPos);
		fSprites[1].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[1].rotation = Custom.VecToDeg((subNodes[0] as Handle).pos);
		MoveSprite(2, absPos);
		fSprites[2].scaleY = (subNodes[2] as Handle).pos.magnitude;
		fSprites[2].rotation = Custom.VecToDeg((subNodes[2] as Handle).pos);
		MoveSprite(3, absPos + (subNodes[0] as Handle).pos);
		fSprites[3].scaleY = Vector2.Distance((subNodes[0] as Handle).pos, (subNodes[1] as Handle).pos);
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother((subNodes[0] as Handle).pos, (subNodes[1] as Handle).pos);
		MoveSprite(4, absPos + (subNodes[1] as Handle).pos);
		fSprites[4].scaleY = Vector2.Distance((subNodes[1] as Handle).pos, (subNodes[2] as Handle).pos);
		fSprites[4].rotation = Custom.AimFromOneVectorToAnother((subNodes[1] as Handle).pos, (subNodes[2] as Handle).pos);
	}
}
