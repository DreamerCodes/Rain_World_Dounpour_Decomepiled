using System;
using DevInterface;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class EnergySwirlRepresentation : PlacedObjectRepresentation
{
	public class EnergySwirlPanel : Panel, IDevUISignals
	{
		public class EnergySwirlSlider : Slider
		{
			public EnergySwirlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Depth_Slider")
				{
					((parentNode.parentNode as EnergySwirlRepresentation).pObj.data as PlacedObject.EnergySwirlData).depth = nubPos;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				_ = IDstring;
				if (IDstring == "Depth_Slider")
				{
					num = ((parentNode.parentNode as EnergySwirlRepresentation).pObj.data as PlacedObject.EnergySwirlData).depth;
				}
				base.NumberText = (int)(num * 30f) + " ";
				RefreshNubPos(num);
			}
		}

		public EnergySwirlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Energy Swirl")
		{
			subNodes.Add(new Button(owner, "Color_Button", this, new Vector2(5f, 5f), 100f, ((parentNode as EnergySwirlRepresentation).pObj.data as PlacedObject.EnergySwirlData).colorType.ToString()));
			subNodes.Add(new EnergySwirlSlider(owner, "Depth_Slider", this, new Vector2(5f, 25f), "Depth: "));
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
			PlacedObject.EnergySwirlData energySwirlData = (parentNode as EnergySwirlRepresentation).pObj.data as PlacedObject.EnergySwirlData;
			if (sender.IDstring == "Color_Button")
			{
				int num = energySwirlData.colorType.Index + 1;
				if (num >= ExtEnum<PlacedObject.EnergySwirlData.ColorType>.values.Count)
				{
					num = 0;
				}
				energySwirlData.colorType = new PlacedObject.EnergySwirlData.ColorType(ExtEnum<PlacedObject.EnergySwirlData.ColorType>.values.GetEntry(num));
				(sender as Button).Text = energySwirlData.colorType.ToString();
				(parentNode as EnergySwirlRepresentation).swirl.color = new Color(1f, 1f, 1f);
			}
		}
	}

	private EnergySwirl swirl;

	public EnergySwirlRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.EnergySwirlData).handlePos;
		fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[3]);
		fSprites[3].anchorY = 0f;
		subNodes.Add(new EnergySwirlPanel(owner, "Energy_Swirl_Control_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as EnergySwirlPanel).pos = (pObj.data as PlacedObject.EnergySwirlData).panelPos;
		for (int i = 0; i < owner.room.energySwirls.Count; i++)
		{
			if (owner.room.energySwirls[i].Pos == pObj.pos)
			{
				swirl = owner.room.energySwirls[i];
				break;
			}
		}
		if (swirl == null)
		{
			swirl = new EnergySwirl(pos, new Color(1f, 1f, 1f), null);
			owner.room.AddObject(swirl);
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
		(pObj.data as PlacedObject.EnergySwirlData).handlePos = (subNodes[0] as Handle).pos;
		MoveSprite(3, absPos);
		fSprites[3].scaleY = (subNodes[1] as EnergySwirlPanel).pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as EnergySwirlPanel).absPos);
		(pObj.data as PlacedObject.EnergySwirlData).handlePos = (subNodes[0] as Handle).pos;
		(pObj.data as PlacedObject.EnergySwirlData).panelPos = (subNodes[1] as Panel).pos;
		swirl.setPos = pObj.pos;
		swirl.setDepth = (pObj.data as PlacedObject.EnergySwirlData).depth;
		swirl.setRad = (pObj.data as PlacedObject.EnergySwirlData).Rad;
		swirl.colorFromEnviroment = (pObj.data as PlacedObject.EnergySwirlData).colorType == PlacedObject.EnergySwirlData.ColorType.Environment;
		swirl.effectColor = Math.Max(-1, (pObj.data as PlacedObject.EnergySwirlData).colorType.Index - PlacedObject.EnergySwirlData.ColorType.EffectColor1.Index);
	}
}
