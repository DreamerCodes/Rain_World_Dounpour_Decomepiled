using RWCustom;
using UnityEngine;

namespace DevInterface;

public class DeepProcessingRepresentation : QuadObjectRepresentation
{
	public class DeepProcessingControlPanel : Panel
	{
		public class DeepProcessControlSlider : Slider
		{
			public DeepProcessControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "From_Depth_Slider":
					num = ((parentNode.parentNode as DeepProcessingRepresentation).pObj.data as PlacedObject.DeepProcessingData).fromDepth;
					base.NumberText = ((int)(num * 30f)).ToString();
					break;
				case "To_Depth_Slider":
					num = ((parentNode.parentNode as DeepProcessingRepresentation).pObj.data as PlacedObject.DeepProcessingData).toDepth;
					base.NumberText = ((int)(num * 30f)).ToString();
					break;
				case "Intensity_Slider":
					num = ((parentNode.parentNode as DeepProcessingRepresentation).pObj.data as PlacedObject.DeepProcessingData).intensity;
					base.NumberText = (int)(num * 100f) + "%";
					break;
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "From_Depth_Slider":
					nubPos = Mathf.Min(nubPos, ((parentNode.parentNode as DeepProcessingRepresentation).pObj.data as PlacedObject.DeepProcessingData).toDepth);
					((parentNode.parentNode as DeepProcessingRepresentation).pObj.data as PlacedObject.DeepProcessingData).fromDepth = nubPos;
					break;
				case "To_Depth_Slider":
					nubPos = Mathf.Max(nubPos, ((parentNode.parentNode as DeepProcessingRepresentation).pObj.data as PlacedObject.DeepProcessingData).fromDepth);
					((parentNode.parentNode as DeepProcessingRepresentation).pObj.data as PlacedObject.DeepProcessingData).toDepth = nubPos;
					break;
				case "Intensity_Slider":
					((parentNode.parentNode as DeepProcessingRepresentation).pObj.data as PlacedObject.DeepProcessingData).intensity = nubPos;
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public DeepProcessingControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 65f), "Deep Processing")
		{
			subNodes.Add(new DeepProcessControlSlider(owner, "From_Depth_Slider", this, new Vector2(5f, 45f), "From Depth: "));
			subNodes.Add(new DeepProcessControlSlider(owner, "To_Depth_Slider", this, new Vector2(5f, 25f), "To Depth: "));
			subNodes.Add(new DeepProcessControlSlider(owner, "Intensity_Slider", this, new Vector2(5f, 5f), "Intensity: "));
		}
	}

	public DeepProcessing DP;

	private DeepProcessingControlPanel controlPanel;

	private int lineSprite;

	public DeepProcessingRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Deep Processing")
	{
		controlPanel = new DeepProcessingControlPanel(owner, "Deep_Processing_Panel", this, new Vector2(0f, 100f));
		subNodes.Add(controlPanel);
		controlPanel.pos = (pObj.data as PlacedObject.DeepProcessingData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
		if (DP == null)
		{
			DP = new DeepProcessing(pObj);
			owner.room.AddObject(DP);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = controlPanel.pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
		(pObj.data as PlacedObject.DeepProcessingData).panelPos = controlPanel.pos;
	}
}
