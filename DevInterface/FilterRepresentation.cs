using RWCustom;
using UnityEngine;

namespace DevInterface;

public class FilterRepresentation : ResizeableObjectRepresentation
{
	public class FilterControlPanel : Panel, IDevUISignals
	{
		public Button[] buttons;

		public FilterControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 55f), "Placable Filter")
		{
			buttons = new Button[ExtEnum<SlugcatStats.Name>.values.Count];
			float num = 10f;
			float num2 = 20f;
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i] = new Button(owner, "Button_" + i, this, new Vector2(5f + (num2 + 5f) * ((float)i % num), 5f + (float)(int)((float)i / num) * (num2 + 5f)), num2, "");
				subNodes.Add(buttons[i]);
			}
			UpdateButtonText();
		}

		private void UpdateButtonText()
		{
			int num = 0;
			for (int i = 0; i < buttons.Length; i++)
			{
				SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
				if (((parentNode as FilterRepresentation).pObj.data as PlacedObject.FilterData).availableToPlayers.Contains(name))
				{
					num++;
					buttons[i].Text = name.value.Substring(0, 2);
				}
				else
				{
					buttons[i].Text = "--";
				}
			}
			Color color = Color.magenta;
			if (num == 1)
			{
				for (int j = 0; j < buttons.Length; j++)
				{
					SlugcatStats.Name name2 = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(j));
					if (((parentNode as FilterRepresentation).pObj.data as PlacedObject.FilterData).availableToPlayers.Contains(name2))
					{
						color = PlayerGraphics.DefaultSlugcatColor(name2);
						if (j > 0)
						{
							color = Custom.Saturate(color, 0.5f);
						}
						break;
					}
				}
			}
			for (int k = 0; k < parentNode.fSprites.Count; k++)
			{
				parentNode.fSprites[k].color = color;
			}
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			for (int i = 0; i < buttons.Length; i++)
			{
				if (buttons[i] == sender)
				{
					SlugcatStats.Name item = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
					if (((parentNode as FilterRepresentation).pObj.data as PlacedObject.FilterData).availableToPlayers.Contains(item))
					{
						((parentNode as FilterRepresentation).pObj.data as PlacedObject.FilterData).availableToPlayers.Remove(item);
					}
					else
					{
						((parentNode as FilterRepresentation).pObj.data as PlacedObject.FilterData).availableToPlayers.Add(item);
					}
				}
			}
			UpdateButtonText();
		}
	}

	private int lineSprite;

	public FilterRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "Filter", showRing: true)
	{
		subNodes.Add(new FilterControlPanel(owner, "Filter_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as FilterControlPanel).pos = (pObj.data as PlacedObject.FilterData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = (subNodes[1] as FilterControlPanel).pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as FilterControlPanel).absPos);
		(pObj.data as PlacedObject.FilterData).panelPos = (subNodes[1] as Panel).pos;
	}
}
