using DevInterface;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class LightningMachineRepresentation : PlacedObjectRepresentation
{
	public class LightningMachinePanel : Panel, IDevUISignals
	{
		public class LightningMachineSlider : Slider
		{
			public LightningMachineSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null)
				{
					if (iDstring == "Intensity_Slider")
					{
						((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).intensity = nubPos;
					}
					if (iDstring == "Chance_Slider")
					{
						((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).chance = nubPos;
					}
					if (iDstring == "Lifetime_Slider")
					{
						((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).lifeTime = nubPos;
					}
					if (iDstring == "Width_Slider")
					{
						((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).width = nubPos;
					}
					if (iDstring == "lightningParam_Slider")
					{
						((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).lightningParam = nubPos;
					}
					if (iDstring == "lightningType_Slider")
					{
						((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).lightningType = nubPos;
					}
					if (iDstring == "Volume_Slider")
					{
						((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).volume = nubPos;
					}
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				string iDstring = IDstring;
				if (iDstring == "Intensity_Slider")
				{
					num = ((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).intensity;
					base.NumberText = (int)(num * 100f) + "%";
				}
				if (iDstring == "Chance_Slider")
				{
					num = ((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).chance;
					base.NumberText = (int)(num * 100f) + "%";
				}
				if (iDstring == "Lifetime_Slider")
				{
					num = ((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).lifeTime;
					base.NumberText = (int)(num * 100f) + "%";
				}
				if (iDstring == "Width_Slider")
				{
					num = ((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).width;
					base.NumberText = (int)(num * 100f) + "%";
				}
				if (iDstring == "lightningParam_Slider")
				{
					num = ((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).lightningParam;
					base.NumberText = num + " ";
				}
				if (iDstring == "lightningType_Slider")
				{
					num = ((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).lightningType;
					base.NumberText = num + " ";
				}
				if (iDstring == "Volume_Slider")
				{
					num = ((parentNode.parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).volume;
					base.NumberText = (int)(num * 100f) + "%";
				}
				RefreshNubPos(num);
			}
		}

		private string[] impactTypes;

		private string[] sounds;

		public LightningMachinePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 205f), "Lightning Machine")
		{
			impactTypes = new string[4] { "No impact", "On ground", "On target", "Both ends" };
			sounds = new string[2] { "Spark", "ZapCoilSpark" };
			float num = 5f;
			float num2 = 5f;
			subNodes.Add(new Button(owner, "Radial_Switch", this, new Vector2(num, num2), 100f, ((parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).radial ? "Radial" : "Linear"));
			num2 += 20f;
			subNodes.Add(new Button(owner, "Permanint_Switch", this, new Vector2(120f, num), 100f, ((parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).permanent ? "Permanent" : "Momentary"));
			subNodes.Add(new Button(owner, "Impact_Switch", this, new Vector2(num, num2), 100f, impactTypes[((parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).impact % 4]));
			num2 += 20f;
			subNodes.Add(new Button(owner, "Sound_Switch", this, new Vector2(120f, num + 20f), 100f, "Sound: " + sounds[((parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).soundType % 2]));
			subNodes.Add(new Button(owner, "Random_Switch", this, new Vector2(num, num2), 100f, ((parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).random ? "Random: On" : "Random: Off"));
			num2 += 20f;
			subNodes.Add(new Button(owner, "Light_Switch", this, new Vector2(120f, num + 40f), 100f, "Lights: " + (((parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).light ? "On" : "Off")));
			string[,] array = new string[7, 2]
			{
				{ "Intensity_Slider", "Intensity: " },
				{
					"Chance_Slider",
					((parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData).random ? "Chance: " : "Rate: "
				},
				{ "Lifetime_Slider", "Lifetime: " },
				{ "Width_Slider", "Width: " },
				{ "lightningParam_Slider", "Morph: " },
				{ "lightningType_Slider", "Hue: " },
				{ "Volume_Slider", "Volume: " }
			};
			for (int i = 0; i < array.GetLength(0); i++)
			{
				subNodes.Add(new LightningMachineSlider(owner, array[i, 0], this, new Vector2(num, num2), array[i, 1]));
				num2 += 20f;
			}
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
			PlacedObject.LightningMachineData lightningMachineData = (parentNode as LightningMachineRepresentation).pObj.data as PlacedObject.LightningMachineData;
			if (sender.IDstring == "Light_Switch")
			{
				lightningMachineData.light = !lightningMachineData.light;
				(sender as Button).Text = "Lights: " + (lightningMachineData.light ? "On" : "Off");
			}
			if (sender.IDstring == "Radial_Switch")
			{
				lightningMachineData.radial = !lightningMachineData.radial;
				(sender as Button).Text = (lightningMachineData.radial ? "Radial" : "Linear");
			}
			if (sender.IDstring == "Permanint_Switch")
			{
				lightningMachineData.permanent = !lightningMachineData.permanent;
				(sender as Button).Text = (lightningMachineData.permanent ? "Permanent" : "Momentary");
			}
			if (sender.IDstring == "Impact_Switch")
			{
				lightningMachineData.impact++;
				lightningMachineData.impact %= 4;
				(sender as Button).Text = impactTypes[lightningMachineData.impact];
			}
			if (sender.IDstring == "Sound_Switch")
			{
				lightningMachineData.soundType++;
				lightningMachineData.soundType %= 2;
				(sender as Button).Text = "Sound: " + sounds[lightningMachineData.soundType];
			}
			if (!(sender.IDstring == "Random_Switch"))
			{
				return;
			}
			lightningMachineData.random = !lightningMachineData.random;
			(sender as Button).Text = (lightningMachineData.random ? "Random: On" : "Random: Off");
			foreach (DevUINode subNode in subNodes)
			{
				if (subNode is Slider && (subNode as Slider).IDstring == "Chance_Slider")
				{
					((subNode as Slider).subNodes[0] as DevUILabel).Text = (lightningMachineData.random ? "Chance: " : "Rate: ");
				}
			}
		}
	}

	public LightningMachine machine;

	public LightningMachineRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "startPoint_Handle", this, new Vector2(-100f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.LightningMachineData).startPoint;
		subNodes.Add(new Handle(owner, "endPoint_Handle", this, new Vector2(100f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.LightningMachineData).endPoint;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[3]);
		fSprites[3].anchorY = 0f;
		subNodes.Add(new LightningMachinePanel(owner, "Lightning_Machine_Control_Panel", this, new Vector2(0f, 200f)));
		(subNodes[subNodes.Count - 1] as LightningMachinePanel).pos = (pObj.data as PlacedObject.LightningMachineData).panelPos;
		for (int i = 0; i < owner.room.lightningMachines.Count; i++)
		{
			if (owner.room.lightningMachines[i].pos == pObj.pos)
			{
				machine = owner.room.lightningMachines[i];
				break;
			}
		}
		if (machine == null)
		{
			machine = new LightningMachine(pos, (base.pObj.data as PlacedObject.LightningMachineData).startPoint, (base.pObj.data as PlacedObject.LightningMachineData).endPoint, 100f, permanent: false, radial: false, 1f, 1f, 10f);
			owner.room.AddObject(machine);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		MoveSprite(2, absPos);
		fSprites[2].scaleY = (subNodes[1] as Handle).pos.magnitude;
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as Handle).absPos);
		MoveSprite(3, absPos);
		fSprites[3].scaleY = (subNodes[2] as LightningMachinePanel).pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[2] as LightningMachinePanel).absPos);
		(pObj.data as PlacedObject.LightningMachineData).startPoint = (subNodes[0] as Handle).pos;
		(pObj.data as PlacedObject.LightningMachineData).endPoint = (subNodes[1] as Handle).pos;
		(pObj.data as PlacedObject.LightningMachineData).panelPos = (subNodes[2] as Panel).pos;
		machine.pos = pObj.pos;
		machine.startPoint = (pObj.data as PlacedObject.LightningMachineData).startPoint;
		machine.endPoint = (pObj.data as PlacedObject.LightningMachineData).endPoint;
		machine.chance = (pObj.data as PlacedObject.LightningMachineData).chance;
		machine.permanent = (pObj.data as PlacedObject.LightningMachineData).permanent;
		machine.radial = (pObj.data as PlacedObject.LightningMachineData).radial;
		machine.width = (pObj.data as PlacedObject.LightningMachineData).width;
		machine.intensity = (pObj.data as PlacedObject.LightningMachineData).intensity;
		machine.lifeTime = (pObj.data as PlacedObject.LightningMachineData).lifeTime;
		machine.lightningParam = (pObj.data as PlacedObject.LightningMachineData).lightningParam;
		machine.lightningType = (pObj.data as PlacedObject.LightningMachineData).lightningType;
		machine.impactType = (pObj.data as PlacedObject.LightningMachineData).impact;
		machine.volume = (pObj.data as PlacedObject.LightningMachineData).volume;
		machine.soundType = (pObj.data as PlacedObject.LightningMachineData).soundType;
		machine.random = (pObj.data as PlacedObject.LightningMachineData).random;
		machine.light = (pObj.data as PlacedObject.LightningMachineData).light;
	}
}
