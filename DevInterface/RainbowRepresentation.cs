using RWCustom;
using UnityEngine;

namespace DevInterface;

public class RainbowRepresentation : ResizeableObjectRepresentation
{
	public class RainbowControlPanel : Panel
	{
		public class FadeSlider : Slider
		{
			private int index;

			public Rainbow.RainbowData RainbowData => (parentNode.parentNode as RainbowRepresentation).pObj.data as Rainbow.RainbowData;

			public FadeSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, int index)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
				this.index = index;
			}

			public override void Refresh()
			{
				base.Refresh();
				base.NumberText = Mathf.RoundToInt(RainbowData.fades[index] * 100f) + "%";
				RefreshNubPos(RainbowData.fades[index]);
			}

			public override void NubDragged(float nubPos)
			{
				RainbowData.fades[index] = nubPos;
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public Rainbow.RainbowData RainbowData => (parentNode as RainbowRepresentation).pObj.data as Rainbow.RainbowData;

		public RainbowControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 125f), "Rainbow")
		{
			for (int i = 0; i < 4; i++)
			{
				subNodes.Add(new FadeSlider(owner, "Fade_Slider", this, new Vector2(5.01f, 5f + 20f * (float)i), "Fade " + i + ":", i));
			}
			subNodes.Add(new FadeSlider(owner, "Thick_Slider", this, new Vector2(5.01f, 85f), "Thickness:", 4));
			subNodes.Add(new FadeSlider(owner, "Chance_Slider", this, new Vector2(5.01f, 105f), "Per cycle chance:", 5));
		}
	}

	private int lineSprite;

	private Rainbow rainbow;

	public RainbowRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "Rainbow", showRing: false)
	{
		subNodes.Add(new RainbowControlPanel(owner, "Rainbow_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as RainbowControlPanel).pos = (pObj.data as Rainbow.RainbowData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
		for (int i = 0; i < owner.room.updateList.Count; i++)
		{
			if (owner.room.updateList[i] is Rainbow && (owner.room.updateList[i] as Rainbow).placedObject == pObj)
			{
				rainbow = owner.room.updateList[i] as Rainbow;
				break;
			}
		}
		if (rainbow == null)
		{
			rainbow = new Rainbow(owner.room, pObj);
			owner.room.AddObject(rainbow);
		}
		rainbow.alwaysShow = true;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = (subNodes[1] as RainbowControlPanel).pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as RainbowControlPanel).absPos);
		rainbow.Refresh();
		(pObj.data as Rainbow.RainbowData).panelPos = (subNodes[1] as Panel).pos;
	}
}
