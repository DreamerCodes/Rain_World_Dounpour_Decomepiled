using DevInterface;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class LocalBlizzardRepresentation : PlacedObjectRepresentation
{
	public class LocalBlizzardPanel : Panel, IDevUISignals
	{
		public class LocalBlizzardSlider : Slider
		{
			public LocalBlizzardSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Intensity_Slider")
				{
					((parentNode.parentNode as LocalBlizzardRepresentation).pObj.data as PlacedObject.LocalBlizzardData).intensity = nubPos;
				}
				if (iDstring != null && iDstring == "Scale_Slider")
				{
					((parentNode.parentNode as LocalBlizzardRepresentation).pObj.data as PlacedObject.LocalBlizzardData).scale = nubPos;
				}
				if (iDstring != null && iDstring == "Angle_Slider")
				{
					((parentNode.parentNode as LocalBlizzardRepresentation).pObj.data as PlacedObject.LocalBlizzardData).angle = nubPos;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}

			public override void Refresh()
			{
				base.Refresh();
				_ = IDstring;
				if (IDstring == "Intensity_Slider")
				{
					float intensity = ((parentNode.parentNode as LocalBlizzardRepresentation).pObj.data as PlacedObject.LocalBlizzardData).intensity;
					base.NumberText = (int)(intensity * 100f) + "% ";
					RefreshNubPos(intensity);
				}
				if (IDstring == "Scale_Slider")
				{
					float scale = ((parentNode.parentNode as LocalBlizzardRepresentation).pObj.data as PlacedObject.LocalBlizzardData).scale;
					base.NumberText = (int)(scale * 100f) + "% ";
					RefreshNubPos(scale);
				}
				if (IDstring == "Angle_Slider")
				{
					float angle = ((parentNode.parentNode as LocalBlizzardRepresentation).pObj.data as PlacedObject.LocalBlizzardData).angle;
					base.NumberText = string.Concat((int)(angle * 360f));
					RefreshNubPos(angle);
				}
			}
		}

		public LocalBlizzardPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 65f), "Local Blizzard FX")
		{
			subNodes.Add(new LocalBlizzardSlider(owner, "Angle_Slider", this, new Vector2(5f, 5f), "Angle: "));
			subNodes.Add(new LocalBlizzardSlider(owner, "Scale_Slider", this, new Vector2(5f, 25f), "Scale: "));
			subNodes.Add(new LocalBlizzardSlider(owner, "Intensity_Slider", this, new Vector2(5f, 45f), "Intensity: "));
		}

		public override void Move(Vector2 newPos)
		{
			base.Move(newPos);
			parentNode.Refresh();
		}

		public override void Refresh()
		{
			base.Refresh();
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
		}
	}

	private LocalBlizzard blizzs;

	public LocalBlizzardRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.LocalBlizzardData).handlePos;
		fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[3]);
		fSprites[3].anchorY = 0f;
		subNodes.Add(new LocalBlizzardPanel(owner, "LocalBlizzard_Control_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as LocalBlizzardPanel).pos = (pObj.data as PlacedObject.LocalBlizzardData).panelPos;
		for (int i = 0; i < owner.room.localBlizzards.Count; i++)
		{
			if (owner.room.localBlizzards[i].pos == pObj.pos)
			{
				blizzs = owner.room.localBlizzards[i];
				break;
			}
		}
		if (blizzs == null)
		{
			blizzs = new LocalBlizzard(pos, 100f, 1f, 0.5f);
			owner.room.AddObject(blizzs);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scale = (subNodes[0] as Handle).pos.magnitude / 8f;
		fSprites[1].alpha = 2f / (subNodes[0] as Handle).pos.magnitude;
		MoveSprite(2, absPos);
		fSprites[2].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		(pObj.data as PlacedObject.LocalBlizzardData).handlePos = (subNodes[0] as Handle).pos;
		MoveSprite(3, absPos);
		fSprites[3].scaleY = (subNodes[1] as LocalBlizzardPanel).pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as LocalBlizzardPanel).absPos);
		(pObj.data as PlacedObject.LocalBlizzardData).handlePos = (subNodes[0] as Handle).pos;
		(pObj.data as PlacedObject.LocalBlizzardData).panelPos = (subNodes[1] as Panel).pos;
		blizzs.pos = pObj.pos;
		blizzs.rad = (pObj.data as PlacedObject.LocalBlizzardData).Rad;
		blizzs.angle = (pObj.data as PlacedObject.LocalBlizzardData).angle;
		blizzs.intensity = (pObj.data as PlacedObject.LocalBlizzardData).intensity;
		blizzs.scale = (pObj.data as PlacedObject.LocalBlizzardData).scale;
	}
}
