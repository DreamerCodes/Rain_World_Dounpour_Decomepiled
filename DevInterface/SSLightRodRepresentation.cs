using RWCustom;
using UnityEngine;

namespace DevInterface;

public class SSLightRodRepresentation : PlacedObjectRepresentation
{
	public class SSLightRodControlPanel : Panel
	{
		public class DepthControlSlider : Slider
		{
			public DepthControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "Depth_Slider":
					num = ((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).depth;
					base.NumberText = ((int)(num * 30f)).ToString();
					break;
				case "Rotation_Slider":
					num = ((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).rotation / 315f;
					base.NumberText = ((int)((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).rotation).ToString();
					break;
				case "Length_Slider":
					num = Mathf.InverseLerp(40f, 800f, ((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).length);
					base.NumberText = ((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).length.ToString();
					break;
				case "Brightness_Slider":
					num = ((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).brightness;
					base.NumberText = (int)(((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).brightness * 100f) + "%";
					break;
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Depth_Slider":
					((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).depth = nubPos;
					break;
				case "Rotation_Slider":
					((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).rotation = Mathf.Floor(nubPos * 7f) / 8f * 360f;
					break;
				case "Length_Slider":
					((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).length = Mathf.Lerp(40f, 800f, nubPos);
					break;
				case "Brightness_Slider":
					((parentNode.parentNode as SSLightRodRepresentation).pObj.data as PlacedObject.SSLightRodData).brightness = nubPos;
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public SSLightRodControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 95f), "Light Rod")
		{
			subNodes.Add(new DepthControlSlider(owner, "Depth_Slider", this, new Vector2(5f, 65f), "Depth: "));
			subNodes.Add(new DepthControlSlider(owner, "Rotation_Slider", this, new Vector2(5f, 45f), "Rotation: "));
			subNodes.Add(new DepthControlSlider(owner, "Length_Slider", this, new Vector2(5f, 25f), "Length: "));
			subNodes.Add(new DepthControlSlider(owner, "Brightness_Slider", this, new Vector2(5f, 5f), "Bright: "));
		}
	}

	public SSLightRod rod;

	public SSLightRodRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "SS Light Rod")
	{
		subNodes.Add(new SSLightRodControlPanel(owner, "SS_Light_Rod_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as SSLightRodControlPanel).pos = (pObj.data as PlacedObject.SSLightRodData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].anchorY = 0f;
		if (rod == null)
		{
			rod = new SSLightRod(pObj, owner.room);
			owner.room.AddObject(rod);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scaleY = (subNodes[0] as SSLightRodControlPanel).pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as SSLightRodControlPanel).absPos);
		(pObj.data as PlacedObject.SSLightRodData).panelPos = (subNodes[0] as Panel).pos;
	}
}
