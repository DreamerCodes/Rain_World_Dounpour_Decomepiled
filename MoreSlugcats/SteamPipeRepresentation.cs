using DevInterface;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class SteamPipeRepresentation : PlacedObjectRepresentation
{
	public SteamPipeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.SteamPipeData).handlePos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		(pObj.data as PlacedObject.SteamPipeData).handlePos = (subNodes[0] as Handle).pos;
		(pObj.data as PlacedObject.SteamPipeData).handlePos = (subNodes[0] as Handle).pos;
	}
}
