using RWCustom;
using UnityEngine;

namespace DevInterface;

public class SpotTriggerHandle : Handle
{
	private SpotTrigger spotTrigger;

	public SpotTriggerHandle(DevUI owner, string IDstring, DevUINode parentNode, SpotTrigger spotTrigger, Vector2 pos)
		: base(owner, IDstring, parentNode, pos)
	{
		this.spotTrigger = spotTrigger;
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = spotTrigger.radHandlePosition;
		fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[3]);
		fSprites[3].anchorY = 0f;
	}

	public override void Update()
	{
		base.Update();
		if ((subNodes[0] as Handle).dragged)
		{
			Refresh();
		}
		if (dragged)
		{
			spotTrigger.pos = absPos + owner.game.cameras[0].pos;
			if ((base.Page as TriggersPage).draggedObject == null)
			{
				(base.Page as TriggersPage).draggedObject = parentNode;
			}
		}
		else
		{
			AbsMove(spotTrigger.pos - owner.game.cameras[0].pos);
		}
		if ((subNodes[0] as Handle).dragged)
		{
			spotTrigger.radHandlePosition = (subNodes[0] as Handle).pos;
			spotTrigger.rad = (subNodes[0] as Handle).pos.magnitude;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scale = spotTrigger.rad / 8f;
		fSprites[1].alpha = 3f / Mathf.Min(spotTrigger.rad, 1600f);
		MoveSprite(2, absPos);
		fSprites[2].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		MoveSprite(3, absPos);
		fSprites[3].scaleY = pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (parentNode as PositionedDevUINode).absPos);
	}
}
