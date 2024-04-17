using DevInterface;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class SnowSourceRepresentation : PlacedObjectRepresentation
{
	public class SnowSourcePanel : Panel, IDevUISignals
	{
		public class SnowSourceSlider : Slider
		{
			public SnowSourceSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Intensity_Slider")
				{
					((parentNode.parentNode as SnowSourceRepresentation).pObj.data as PlacedObject.SnowSourceData).intensity = nubPos;
				}
				if (iDstring != null && iDstring == "Irregularity_Slider")
				{
					((parentNode.parentNode as SnowSourceRepresentation).pObj.data as PlacedObject.SnowSourceData).noisiness = nubPos;
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
					num = ((parentNode.parentNode as SnowSourceRepresentation).pObj.data as PlacedObject.SnowSourceData).intensity;
				}
				if (IDstring == "Irregularity_Slider")
				{
					num = ((parentNode.parentNode as SnowSourceRepresentation).pObj.data as PlacedObject.SnowSourceData).noisiness;
				}
				base.NumberText = (int)(num * 100f) + "%";
				RefreshNubPos(num);
			}
		}

		public SnowSourcePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 65f), "Snow Source")
		{
			subNodes.Add(new Button(owner, "FallOff_Button", this, new Vector2(5f, 5f), 100f, ((parentNode as SnowSourceRepresentation).pObj.data as PlacedObject.SnowSourceData).shape.ToString()));
			subNodes.Add(new SnowSourceSlider(owner, "Irregularity_Slider", this, new Vector2(5f, 25f), "Irregularity: "));
			subNodes.Add(new SnowSourceSlider(owner, "Intensity_Slider", this, new Vector2(5f, 45f), "Intensity: "));
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
			PlacedObject.SnowSourceData snowSourceData = (parentNode as SnowSourceRepresentation).pObj.data as PlacedObject.SnowSourceData;
			if (sender.IDstring == "FallOff_Button")
			{
				int num = snowSourceData.shape.Index + 1;
				if (num >= ExtEnum<PlacedObject.SnowSourceData.Shape>.values.Count)
				{
					num = 0;
				}
				snowSourceData.shape = new PlacedObject.SnowSourceData.Shape(ExtEnum<PlacedObject.SnowSourceData.Shape>.values.GetEntry(num));
				(sender as Button).Text = snowSourceData.shape.ToString();
			}
		}
	}

	private SnowSource source;

	public SnowSourceRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.SnowSourceData).handlePos;
		fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[3]);
		fSprites[3].anchorY = 0f;
		subNodes.Add(new SnowSourcePanel(owner, "Snow_Source_Control_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as SnowSourcePanel).pos = (pObj.data as PlacedObject.SnowSourceData).panelPos;
		for (int i = 0; i < owner.room.snowSources.Count; i++)
		{
			if (owner.room.snowSources[i].pos == pObj.pos)
			{
				source = owner.room.snowSources[i];
				break;
			}
		}
		if (source == null)
		{
			source = new SnowSource(pos);
			owner.room.AddObject(source);
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
		(pObj.data as PlacedObject.SnowSourceData).handlePos = (subNodes[0] as Handle).pos;
		MoveSprite(3, absPos);
		fSprites[3].scaleY = (subNodes[1] as SnowSourcePanel).pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as SnowSourcePanel).absPos);
		(pObj.data as PlacedObject.SnowSourceData).handlePos = (subNodes[0] as Handle).pos;
		(pObj.data as PlacedObject.SnowSourceData).panelPos = (subNodes[1] as Panel).pos;
		source.pos = pObj.pos;
		source.intensity = (pObj.data as PlacedObject.SnowSourceData).intensity;
		source.noisiness = (pObj.data as PlacedObject.SnowSourceData).noisiness;
		source.rad = (pObj.data as PlacedObject.SnowSourceData).Rad;
		source.shape = (pObj.data as PlacedObject.SnowSourceData).shape;
	}
}
