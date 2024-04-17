using RWCustom;
using UnityEngine;

namespace DevInterface;

public class MultiplayerItemRepresentation : PlacedObjectRepresentation
{
	public class MultiplayerItemControlPanel : Panel, IDevUISignals
	{
		public class ChanceControlSlider : Slider
		{
			public ChanceControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Chance_Slider")
				{
					num = ((parentNode.parentNode as MultiplayerItemRepresentation).pObj.data as PlacedObject.MultiplayerItemData).chance;
				}
				base.NumberText = (int)(num * 100f) + "%";
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Chance_Slider")
				{
					((parentNode.parentNode as MultiplayerItemRepresentation).pObj.data as PlacedObject.MultiplayerItemData).chance = nubPos;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public MultiplayerItemControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 50f), "Multiplayer Item")
		{
			subNodes.Add(new ChanceControlSlider(owner, "Chance_Slider", this, new Vector2(5f, 25f), "Chance: "));
			subNodes.Add(new Button(owner, "Type_Button", this, new Vector2(5f, 5f), 100f, ((parentNode as MultiplayerItemRepresentation).pObj.data as PlacedObject.MultiplayerItemData).type.ToString()));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			PlacedObject.MultiplayerItemData multiplayerItemData = (parentNode as MultiplayerItemRepresentation).pObj.data as PlacedObject.MultiplayerItemData;
			string iDstring = sender.IDstring;
			if (iDstring != null && iDstring == "Type_Button")
			{
				if ((int)multiplayerItemData.type >= ExtEnum<PlacedObject.MultiplayerItemData.Type>.values.Count - 1)
				{
					multiplayerItemData.type = new PlacedObject.MultiplayerItemData.Type(ExtEnum<PlacedObject.MultiplayerItemData.Type>.values.GetEntry(0));
				}
				else
				{
					multiplayerItemData.type = new PlacedObject.MultiplayerItemData.Type(ExtEnum<PlacedObject.MultiplayerItemData.Type>.values.GetEntry(multiplayerItemData.type.Index + 1));
				}
				(sender as Button).Text = multiplayerItemData.type.ToString();
				(parentNode as MultiplayerItemRepresentation).Name = multiplayerItemData.type.ToString();
			}
		}
	}

	public MultiplayerItemRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, (pObj.data as PlacedObject.MultiplayerItemData).type.ToString())
	{
		subNodes.Add(new MultiplayerItemControlPanel(owner, "Multiplayer_Item_Chance_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as MultiplayerItemControlPanel).pos = (pObj.data as PlacedObject.MultiplayerItemData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scaleY = (subNodes[0] as MultiplayerItemControlPanel).pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as MultiplayerItemControlPanel).absPos);
		(pObj.data as PlacedObject.MultiplayerItemData).panelPos = (subNodes[0] as Panel).pos;
	}
}
