using System;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class LightSourceRepresentation : PlacedObjectRepresentation
{
	public class LightControlPanel : Panel, IDevUISignals
	{
		public class LightControlSlider : Slider
		{
			public LightControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Strength_Slider")
				{
					num = ((parentNode.parentNode as LightSourceRepresentation).pObj.data as PlacedObject.LightSourceData).strength;
				}
				base.NumberText = (int)(num * 100f) + "%";
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Strength_Slider")
				{
					((parentNode.parentNode as LightSourceRepresentation).pObj.data as PlacedObject.LightSourceData).strength = nubPos;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public class RateControlSlider : Slider
		{
			public RateControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				if (IDstring == "BlinkRate_Slider")
				{
					num = ((parentNode.parentNode as LightSourceRepresentation).pObj.data as PlacedObject.LightSourceData).blinkRate;
				}
				base.NumberText = (int)(num * 100f) + "%";
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				if (IDstring == "BlinkRate_Slider")
				{
					((parentNode.parentNode as LightSourceRepresentation).pObj.data as PlacedObject.LightSourceData).blinkRate = nubPos;
					(parentNode.parentNode as LightSourceRepresentation).light.blinkRate = nubPos;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public LightControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 90f), "Light Source")
		{
			subNodes.Add(new LightControlSlider(owner, "Strength_Slider", this, new Vector2(5f, 65f), "Strength: "));
			subNodes.Add(new Button(owner, "Color_Button", this, new Vector2(5f, 45f), 100f, ((parentNode as LightSourceRepresentation).pObj.data as PlacedObject.LightSourceData).colorType.ToString()));
			subNodes.Add(new Button(owner, "Fade_With_Sun_Button", this, new Vector2(125f, 45f), 50f, ((parentNode as LightSourceRepresentation).pObj.data as PlacedObject.LightSourceData).fadeWithSun ? "Sun" : "Static"));
			subNodes.Add(new Button(owner, "Flat_Button", this, new Vector2(180f, 45f), 50f, ((parentNode as LightSourceRepresentation).pObj.data as PlacedObject.LightSourceData).flat ? "Flat: ON" : "Flat: OFF"));
			subNodes.Add(new RateControlSlider(owner, "BlinkRate_Slider", this, new Vector2(5f, 25f), "Blink Rate: "));
			subNodes.Add(new Button(owner, "BlinkType_Button", this, new Vector2(5f, 5f), 100f, ((parentNode as LightSourceRepresentation).pObj.data as PlacedObject.LightSourceData).blinkType.ToString()));
			subNodes.Add(new Button(owner, "NightLight_Button", this, new Vector2(125f, 5f), 100f, ((parentNode as LightSourceRepresentation).pObj.data as PlacedObject.LightSourceData).nightLight ? "Night Only" : "Always On"));
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
			PlacedObject.LightSourceData lightSourceData = (parentNode as LightSourceRepresentation).pObj.data as PlacedObject.LightSourceData;
			switch (sender.IDstring)
			{
			case "Color_Button":
				if ((int)lightSourceData.colorType >= ExtEnum<PlacedObject.LightSourceData.ColorType>.values.Count - 1)
				{
					lightSourceData.colorType = new PlacedObject.LightSourceData.ColorType(ExtEnum<PlacedObject.LightSourceData.ColorType>.values.GetEntry(0));
				}
				else
				{
					lightSourceData.colorType = new PlacedObject.LightSourceData.ColorType(ExtEnum<PlacedObject.LightSourceData.ColorType>.values.GetEntry(lightSourceData.colorType.Index + 1));
				}
				(sender as Button).Text = lightSourceData.colorType.ToString();
				(parentNode as LightSourceRepresentation).light.color = new Color(1f, 1f, 1f);
				break;
			case "Fade_With_Sun_Button":
				lightSourceData.fadeWithSun = !lightSourceData.fadeWithSun;
				(sender as Button).Text = (lightSourceData.fadeWithSun ? "Sun" : "Static");
				break;
			case "Flat_Button":
				lightSourceData.flat = !lightSourceData.flat;
				(sender as Button).Text = (lightSourceData.flat ? "Flat: ON" : "FLAT: OFF");
				break;
			case "BlinkType_Button":
			{
				int num = lightSourceData.blinkType.Index + 1;
				if (num >= ExtEnum<PlacedObject.LightSourceData.BlinkType>.values.Count)
				{
					num = 0;
				}
				lightSourceData.blinkType = new PlacedObject.LightSourceData.BlinkType(ExtEnum<PlacedObject.LightSourceData.BlinkType>.values.GetEntry(num));
				(parentNode as LightSourceRepresentation).light.blinkType = lightSourceData.blinkType;
				(sender as Button).Text = lightSourceData.blinkType.ToString();
				break;
			}
			case "NightLight_Button":
				lightSourceData.nightLight = !lightSourceData.nightLight;
				(sender as Button).Text = ((!lightSourceData.nightLight) ? "Always On" : "Night Only");
				break;
			}
		}
	}

	public LightSource light;

	public LightSourceRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.LightSourceData).handlePos;
		fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[3]);
		fSprites[3].anchorY = 0f;
		subNodes.Add(new LightControlPanel(owner, "Light_Control_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as LightControlPanel).pos = (pObj.data as PlacedObject.LightSourceData).panelPos;
		for (int i = 0; i < owner.room.lightSources.Count; i++)
		{
			if (owner.room.lightSources[i].Pos == pObj.pos)
			{
				light = owner.room.lightSources[i];
				break;
			}
		}
		if (ModManager.MMF && light == null)
		{
			for (int j = 0; j < owner.room.cosmeticLightSources.Count; j++)
			{
				if (owner.room.cosmeticLightSources[j].Pos == pObj.pos)
				{
					light = owner.room.cosmeticLightSources[j];
					break;
				}
			}
		}
		if (light == null)
		{
			light = new LightSource(pos, environmentalLight: true, new Color(1f, 1f, 1f), null);
			owner.room.AddObject(light);
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
		MoveSprite(3, absPos);
		fSprites[3].scaleY = (subNodes[1] as LightControlPanel).pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as LightControlPanel).absPos);
		(pObj.data as PlacedObject.LightSourceData).handlePos = (subNodes[0] as Handle).pos;
		(pObj.data as PlacedObject.LightSourceData).panelPos = (subNodes[1] as Panel).pos;
		light.setPos = pObj.pos;
		light.setRad = (pObj.data as PlacedObject.LightSourceData).Rad;
		light.setAlpha = (pObj.data as PlacedObject.LightSourceData).strength;
		light.fadeWithSun = (pObj.data as PlacedObject.LightSourceData).fadeWithSun;
		light.colorFromEnvironment = (pObj.data as PlacedObject.LightSourceData).colorType == PlacedObject.LightSourceData.ColorType.Environment;
		light.flat = (pObj.data as PlacedObject.LightSourceData).flat;
		light.effectColor = Math.Max(-1, (int)(pObj.data as PlacedObject.LightSourceData).colorType - 2);
	}
}
