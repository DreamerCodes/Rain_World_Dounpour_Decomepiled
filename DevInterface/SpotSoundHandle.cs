using RWCustom;
using UnityEngine;

namespace DevInterface;

public class SpotSoundHandle : Handle
{
	private SpotSound spotSound;

	public SpotSoundHandle(DevUI owner, string IDstring, DevUINode parentNode, SpotSound sound, Vector2 pos, string name)
		: base(owner, IDstring, parentNode, pos)
	{
		spotSound = sound;
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = spotSound.radHandlePosition;
		fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[3]);
		fSprites[3].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[4]);
		fSprites[4].anchorY = 0f;
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
			spotSound.pos = absPos + owner.game.cameras[0].pos;
			if ((base.Page as SoundPage).draggedObject == null)
			{
				(base.Page as SoundPage).draggedObject = parentNode;
			}
		}
		else
		{
			AbsMove(spotSound.pos - owner.game.cameras[0].pos);
		}
		if ((subNodes[0] as Handle).dragged)
		{
			spotSound.radHandlePosition = (subNodes[0] as Handle).pos;
			spotSound.rad = (subNodes[0] as Handle).pos.magnitude;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scale = spotSound.TaperRad / 8f;
		fSprites[1].alpha = 1f / Mathf.Min(spotSound.TaperRad, 400f);
		MoveSprite(2, absPos);
		fSprites[2].scale = spotSound.rad / 8f;
		fSprites[2].alpha = 3f / Mathf.Min(spotSound.rad, 1600f);
		MoveSprite(3, absPos);
		fSprites[3].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		MoveSprite(4, absPos);
		fSprites[4].scaleY = pos.magnitude;
		fSprites[4].rotation = Custom.AimFromOneVectorToAnother(absPos, (parentNode as PositionedDevUINode).absPos);
	}
}
