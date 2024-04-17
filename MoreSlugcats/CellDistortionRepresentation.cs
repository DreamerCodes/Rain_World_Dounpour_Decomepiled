using DevInterface;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class CellDistortionRepresentation : PlacedObjectRepresentation
{
	public class CellDistortionPanel : Panel, IDevUISignals
	{
		public class CellDistortionSlider : Slider
		{
			public CellDistortionSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Intensity_Slider")
				{
					((parentNode.parentNode as CellDistortionRepresentation).pObj.data as PlacedObject.CellDistortionData).intensity = nubPos;
				}
				if (iDstring != null && iDstring == "Scale_Slider")
				{
					((parentNode.parentNode as CellDistortionRepresentation).pObj.data as PlacedObject.CellDistortionData).scale = nubPos;
				}
				if (iDstring != null && iDstring == "Chroma_Slider")
				{
					((parentNode.parentNode as CellDistortionRepresentation).pObj.data as PlacedObject.CellDistortionData).chromaticIntensity = nubPos;
				}
				if (iDstring != null && iDstring == "Time_Slider")
				{
					((parentNode.parentNode as CellDistortionRepresentation).pObj.data as PlacedObject.CellDistortionData).timeMult = nubPos;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				_ = IDstring;
				if (IDstring == "Intensity_Slider")
				{
					num = ((parentNode.parentNode as CellDistortionRepresentation).pObj.data as PlacedObject.CellDistortionData).intensity;
					base.NumberText = (int)(num * 100f) + "% ";
					RefreshNubPos(num);
				}
				if (IDstring == "Scale_Slider")
				{
					num = ((parentNode.parentNode as CellDistortionRepresentation).pObj.data as PlacedObject.CellDistortionData).scale;
					base.NumberText = (int)(num * 100f) + "% ";
					RefreshNubPos(num);
				}
				if (IDstring == "Chroma_Slider")
				{
					num = ((parentNode.parentNode as CellDistortionRepresentation).pObj.data as PlacedObject.CellDistortionData).chromaticIntensity;
					base.NumberText = (int)(num * 100f) + "% ";
					RefreshNubPos(num);
				}
				if (IDstring == "Time_Slider")
				{
					num = ((parentNode.parentNode as CellDistortionRepresentation).pObj.data as PlacedObject.CellDistortionData).timeMult;
					base.NumberText = (int)(num * 100f) + "% ";
					RefreshNubPos(num);
				}
			}
		}

		public CellDistortionPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 85f), "Cellular Distortion")
		{
			subNodes.Add(new CellDistortionSlider(owner, "Intensity_Slider", this, new Vector2(5f, 5f), "Intensity: "));
			subNodes.Add(new CellDistortionSlider(owner, "Scale_Slider", this, new Vector2(5f, 25f), "Scale: "));
			subNodes.Add(new CellDistortionSlider(owner, "Chroma_Slider", this, new Vector2(5f, 45f), "Chromatic Distortion: "));
			subNodes.Add(new CellDistortionSlider(owner, "Time_Slider", this, new Vector2(5f, 65f), "Speed: "));
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

	private CellDistortion distortion;

	public CellDistortionRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.CellDistortionData).handlePos;
		fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[3]);
		fSprites[3].anchorY = 0f;
		subNodes.Add(new CellDistortionPanel(owner, "CellDistortion_Control_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as CellDistortionPanel).pos = (pObj.data as PlacedObject.CellDistortionData).panelPos;
		for (int i = 0; i < owner.room.cellDistortions.Count; i++)
		{
			if (owner.room.cellDistortions[i].pos == pObj.pos)
			{
				distortion = owner.room.cellDistortions[i];
				break;
			}
		}
		if (distortion == null)
		{
			distortion = new CellDistortion(pos, 100f, 1f, 0.5f, 0f, 0f);
			owner.room.AddObject(distortion);
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
		(pObj.data as PlacedObject.CellDistortionData).handlePos = (subNodes[0] as Handle).pos;
		MoveSprite(3, absPos);
		fSprites[3].scaleY = (subNodes[1] as CellDistortionPanel).pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as CellDistortionPanel).absPos);
		(pObj.data as PlacedObject.CellDistortionData).handlePos = (subNodes[0] as Handle).pos;
		(pObj.data as PlacedObject.CellDistortionData).panelPos = (subNodes[1] as Panel).pos;
		distortion.pos = pObj.pos;
		distortion.rad = (pObj.data as PlacedObject.CellDistortionData).Rad;
		distortion.intensity = (pObj.data as PlacedObject.CellDistortionData).intensity;
		distortion.scale = (pObj.data as PlacedObject.CellDistortionData).scale;
		distortion.cromaticIntensity = (pObj.data as PlacedObject.CellDistortionData).chromaticIntensity;
		distortion.timeMult = (pObj.data as PlacedObject.CellDistortionData).timeMult;
	}
}
