using RWCustom;
using UnityEngine;

namespace DevInterface;

public class InsectGroupRepresentation : ResizeableObjectRepresentation
{
	public class InsectGroupPanel : Panel, IDevUISignals
	{
		public class InsectGroupSlider : Slider
		{
			public InsectGroupSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Density_Slider")
				{
					num = ((parentNode.parentNode as InsectGroupRepresentation).pObj.data as PlacedObject.InsectGroupData).density;
					base.NumberText = (int)(num * 100f) + "%";
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Density_Slider")
				{
					((parentNode.parentNode as InsectGroupRepresentation).pObj.data as PlacedObject.InsectGroupData).density = nubPos;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public Button typeButton;

		public InsectGroupPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Insect Group")
		{
			subNodes.Add(new InsectGroupSlider(owner, "Density_Slider", this, new Vector2(5f, 5f), "Density: "));
			typeButton = new Button(owner, "Type_Button", this, new Vector2(5f, 25f), 240f, ((parentNode as InsectGroupRepresentation).pObj.data as PlacedObject.InsectGroupData).insectType.ToString() + "s");
			subNodes.Add(typeButton);
		}

		public override void Refresh()
		{
			base.Refresh();
			typeButton.Text = ((parentNode as InsectGroupRepresentation).pObj.data as PlacedObject.InsectGroupData).insectType.ToString() + "s";
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (sender == typeButton)
			{
				int num = (int)((parentNode as InsectGroupRepresentation).pObj.data as PlacedObject.InsectGroupData).insectType;
				num++;
				if (num >= ExtEnum<CosmeticInsect.Type>.values.Count)
				{
					num = 0;
				}
				((parentNode as InsectGroupRepresentation).pObj.data as PlacedObject.InsectGroupData).insectType = new CosmeticInsect.Type(ExtEnum<CosmeticInsect.Type>.values.GetEntry(num));
				Refresh();
			}
		}
	}

	public InsectGroupRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Insect Group", showRing: true)
	{
		subNodes.Add(new InsectGroupPanel(owner, "Insect_Group_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as InsectGroupPanel).pos = (pObj.data as PlacedObject.InsectGroupData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites[fSprites.Count - 1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(fSprites.Count - 1, absPos);
		fSprites[fSprites.Count - 1].scaleY = (subNodes[subNodes.Count - 1] as InsectGroupPanel).pos.magnitude;
		fSprites[fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[subNodes.Count - 1] as InsectGroupPanel).absPos);
		(pObj.data as PlacedObject.InsectGroupData).panelPos = (subNodes[subNodes.Count - 1] as Panel).pos;
	}
}
