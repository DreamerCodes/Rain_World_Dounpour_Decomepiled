using RWCustom;
using UnityEngine;

namespace DevInterface;

public class ResizeableObjectRepresentation : PlacedObjectRepresentation
{
	private bool showRing;

	public ResizeableObjectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name, bool showRing)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		this.showRing = showRing;
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.ResizableObjectData).handlePos;
		if (showRing)
		{
			fSprites.Add(new FSprite("Futile_White"));
			owner.placedObjectsContainer.AddChild(fSprites[1]);
			fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		}
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[(!showRing) ? 1 : 2]);
		fSprites[(!showRing) ? 1 : 2].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		if (showRing)
		{
			fSprites[1].scale = (subNodes[0] as Handle).pos.magnitude / 8f;
			fSprites[1].alpha = 2f / (subNodes[0] as Handle).pos.magnitude;
		}
		MoveSprite(2, absPos);
		fSprites[(!showRing) ? 1 : 2].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[(!showRing) ? 1 : 2].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		(pObj.data as PlacedObject.ResizableObjectData).handlePos = (subNodes[0] as Handle).pos;
	}
}
