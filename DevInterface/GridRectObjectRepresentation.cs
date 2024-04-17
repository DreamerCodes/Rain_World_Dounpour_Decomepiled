using RWCustom;
using UnityEngine;

namespace DevInterface;

public class GridRectObjectRepresentation : PlacedObjectRepresentation
{
	public GridRectObjectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rect_Handle", this, new Vector2(40f, 40f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.GridRectObjectData).handlePos;
		for (int i = 0; i < 5; i++)
		{
			fSprites.Add(new FSprite("pixel"));
			owner.placedObjectsContainer.AddChild(fSprites[1 + i]);
			fSprites[1 + i].anchorX = 0f;
			fSprites[1 + i].anchorY = 0f;
		}
		fSprites[5].alpha = 0.05f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		(pObj.data as PlacedObject.GridRectObjectData).handlePos = (subNodes[0] as Handle).pos;
		IntRect rect = (pObj.data as PlacedObject.GridRectObjectData).Rect;
		rect.right++;
		rect.top++;
		MoveSprite(1, new Vector2((float)rect.left * 20f, (float)rect.bottom * 20f) - owner.room.game.cameras[0].pos);
		fSprites[1].scaleY = (float)rect.Height * 20f + 1f;
		MoveSprite(2, new Vector2((float)rect.left * 20f, (float)rect.bottom * 20f) - owner.room.game.cameras[0].pos);
		fSprites[2].scaleX = (float)rect.Width * 20f + 1f;
		MoveSprite(3, new Vector2((float)rect.right * 20f, (float)rect.bottom * 20f) - owner.room.game.cameras[0].pos);
		fSprites[3].scaleY = (float)rect.Height * 20f + 1f;
		MoveSprite(4, new Vector2((float)rect.left * 20f, (float)rect.top * 20f) - owner.room.game.cameras[0].pos);
		fSprites[4].scaleX = (float)rect.Width * 20f + 1f;
		MoveSprite(5, new Vector2((float)rect.left * 20f, (float)rect.bottom * 20f) - owner.room.game.cameras[0].pos);
		fSprites[5].scaleX = (float)rect.Width * 20f + 1f;
		fSprites[5].scaleY = (float)rect.Height * 20f + 1f;
	}
}
