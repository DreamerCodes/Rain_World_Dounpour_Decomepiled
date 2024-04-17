using RWCustom;
using UnityEngine;

namespace DevInterface;

public class LightFixtureRepresentation : PlacedObjectRepresentation
{
	public class LightFixtureControlPanel : Panel, IDevUISignals
	{
		public class SeedControlSlider : Slider
		{
			public SeedControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				int num = 0;
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Seed_Slider")
				{
					num = ((parentNode.parentNode as LightFixtureRepresentation).pObj.data as PlacedObject.LightFixtureData).randomSeed;
				}
				base.NumberText = num.ToString();
				RefreshNubPos((float)num / 100f);
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Seed_Slider")
				{
					((parentNode.parentNode as LightFixtureRepresentation).pObj.data as PlacedObject.LightFixtureData).randomSeed = (int)(nubPos * 100f);
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public LightFixtureControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 50f), "Light Fixture")
		{
			subNodes.Add(new SeedControlSlider(owner, "Seed_Slider", this, new Vector2(5f, 25f), "Seed: "));
			subNodes.Add(new Button(owner, "Type_Button", this, new Vector2(5f, 5f), 100f, ((parentNode as LightFixtureRepresentation).pObj.data as PlacedObject.LightFixtureData).type.ToString()));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			PlacedObject.LightFixtureData lightFixtureData = (parentNode as LightFixtureRepresentation).pObj.data as PlacedObject.LightFixtureData;
			string iDstring = sender.IDstring;
			if (iDstring != null && iDstring == "Type_Button")
			{
				if ((int)lightFixtureData.type >= ExtEnum<PlacedObject.LightFixtureData.Type>.values.Count - 1)
				{
					lightFixtureData.type = new PlacedObject.LightFixtureData.Type(ExtEnum<PlacedObject.LightFixtureData.Type>.values.GetEntry(0));
				}
				else
				{
					lightFixtureData.type = new PlacedObject.LightFixtureData.Type(ExtEnum<PlacedObject.LightFixtureData.Type>.values.GetEntry(lightFixtureData.type.Index + 1));
				}
				(sender as Button).Text = lightFixtureData.type.ToString();
				(parentNode as LightFixtureRepresentation).Name = lightFixtureData.type.ToString();
				(owner.activePage as ObjectsPage).lastPlacedLightFixture = lightFixtureData.type;
			}
		}
	}

	public LightFixtureRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, (pObj.data as PlacedObject.LightFixtureData).type.ToString())
	{
		subNodes.Add(new LightFixtureControlPanel(owner, "Fixture_Seed_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as LightFixtureControlPanel).pos = (pObj.data as PlacedObject.LightFixtureData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scaleY = (subNodes[0] as LightFixtureControlPanel).pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as LightFixtureControlPanel).absPos);
		(pObj.data as PlacedObject.LightFixtureData).panelPos = (subNodes[0] as Panel).pos;
	}
}
