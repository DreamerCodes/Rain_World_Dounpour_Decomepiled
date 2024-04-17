using RWCustom;
using UnityEngine;

namespace DevInterface;

public class LightBeamRepresentation : QuadObjectRepresentation
{
	public class LightBeamControlPanel : Panel, IDevUISignals
	{
		public class LightBeamSlider : Slider
		{
			public LightBeamSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "Alpha_Slider":
					num = ((parentNode.parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).alpha;
					base.NumberText = (int)(num * 100f) + "%";
					break;
				case "ColA_Slider":
					num = ((parentNode.parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).colorA;
					base.NumberText = (int)(num * 100f) + "%";
					break;
				case "ColB_Slider":
					num = ((parentNode.parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).colorB;
					base.NumberText = (int)(num * 100f) + "%";
					break;
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Alpha_Slider":
					((parentNode.parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).alpha = nubPos;
					break;
				case "ColA_Slider":
					((parentNode.parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).colorA = nubPos;
					break;
				case "ColB_Slider":
					((parentNode.parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).colorB = nubPos;
					break;
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
					num = ((parentNode.parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).blinkRate;
				}
				base.NumberText = (int)(num * 100f) + "%";
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				if (IDstring == "BlinkRate_Slider")
				{
					((parentNode.parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).blinkRate = nubPos;
					(parentNode.parentNode as LightBeamRepresentation).LB.blinkRate = nubPos;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public Button sunButton;

		public LightBeamControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 125f), "Light Beam")
		{
			subNodes.Add(new LightBeamSlider(owner, "Alpha_Slider", this, new Vector2(5f, 105f), "Alpha/depth: "));
			subNodes.Add(new LightBeamSlider(owner, "ColA_Slider", this, new Vector2(5f, 85f), "White-Standard: "));
			subNodes.Add(new LightBeamSlider(owner, "ColB_Slider", this, new Vector2(5f, 65f), "Pickup: "));
			sunButton = new Button(owner, "Sun_Button", this, new Vector2(5f, 45f), 110f, "");
			subNodes.Add(sunButton);
			sunButton.Text = (((parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).sun ? "SUN" : "STATIC");
			subNodes.Add(new RateControlSlider(owner, "BlinkRate_Slider", this, new Vector2(5f, 25f), "Blink Rate: "));
			subNodes.Add(new Button(owner, "BlinkType_Button", this, new Vector2(5f, 5f), 100f, ((parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).blinkType.ToString()));
			subNodes.Add(new Button(owner, "NightLight_Button", this, new Vector2(125f, 5f), 100f, ((parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).nightLight ? "Night Only" : "Always On"));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			LightBeam.LightBeamData lightBeamData = (parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData;
			if (sender.IDstring == "Sun_Button")
			{
				((parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).sun = !((parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).sun;
				sunButton.Text = (((parentNode as LightBeamRepresentation).pObj.data as LightBeam.LightBeamData).sun ? "SUN" : "STATIC");
			}
			else if (sender.IDstring == "BlinkType_Button")
			{
				if (lightBeamData.blinkType.Index >= ExtEnum<LightBeam.LightBeamData.BlinkType>.values.entries.Count - 1 || lightBeamData.blinkType.Index == -1)
				{
					lightBeamData.blinkType = LightBeam.LightBeamData.BlinkType.None;
				}
				else
				{
					lightBeamData.blinkType = new LightBeam.LightBeamData.BlinkType(ExtEnum<LightBeam.LightBeamData.BlinkType>.values.GetEntry(lightBeamData.blinkType.Index + 1));
				}
				(parentNode as LightBeamRepresentation).LB.blinkType = lightBeamData.blinkType;
				(sender as Button).Text = lightBeamData.blinkType.ToString();
			}
			else if (sender.IDstring == "NightLight_Button")
			{
				lightBeamData.nightLight = !lightBeamData.nightLight;
				(sender as Button).Text = ((!lightBeamData.nightLight) ? "Always On" : "Night Only");
			}
		}
	}

	public LightBeam LB;

	private LightBeamControlPanel controlPanel;

	private int lineSprite;

	public LightBeamRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "Light Beam")
	{
		controlPanel = new LightBeamControlPanel(owner, "Light_Beam_Panel", this, new Vector2(0f, 100f));
		subNodes.Add(controlPanel);
		controlPanel.pos = (pObj.data as LightBeam.LightBeamData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
		for (int i = 0; i < owner.room.updateList.Count; i++)
		{
			if (owner.room.updateList[i] is LightBeam && (owner.room.updateList[i] as LightBeam).placedObject == pObj)
			{
				LB = owner.room.updateList[i] as LightBeam;
				break;
			}
		}
		if (LB == null)
		{
			LB = new LightBeam(pObj);
			owner.room.AddObject(LB);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = controlPanel.pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
		(pObj.data as LightBeam.LightBeamData).panelPos = controlPanel.pos;
		LB.meshDirty = true;
	}
}
