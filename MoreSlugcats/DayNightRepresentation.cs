using DevInterface;
using UnityEngine;

namespace MoreSlugcats;

public class DayNightRepresentation : PlacedObjectRepresentation
{
	public class DayNightControlPanel : Panel, IDevUISignals
	{
		public class PaletteController : IntegerControl
		{
			private int controlPoint;

			public PaletteController(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, int controlPoint)
				: base(owner, IDstring, parentNode, pos, title)
			{
				this.controlPoint = controlPoint;
			}

			public override void Increment(int change)
			{
				if (controlPoint == 0)
				{
					((parentNode.parentNode as DayNightRepresentation).pObj.data as PlacedObject.DayNightData).duskPalette += change;
					if (((parentNode.parentNode as DayNightRepresentation).pObj.data as PlacedObject.DayNightData).duskPalette < 0)
					{
						((parentNode.parentNode as DayNightRepresentation).pObj.data as PlacedObject.DayNightData).duskPalette = 0;
					}
				}
				if (controlPoint == 1)
				{
					((parentNode.parentNode as DayNightRepresentation).pObj.data as PlacedObject.DayNightData).nightPalette += change;
					if (((parentNode.parentNode as DayNightRepresentation).pObj.data as PlacedObject.DayNightData).nightPalette < 0)
					{
						((parentNode.parentNode as DayNightRepresentation).pObj.data as PlacedObject.DayNightData).nightPalette = 0;
					}
				}
				((parentNode.parentNode as DayNightRepresentation).pObj.data as PlacedObject.DayNightData).Apply((parentNode.parentNode as DayNightRepresentation).owner.room);
				Refresh();
			}

			public override void Refresh()
			{
				if (controlPoint == 0)
				{
					base.NumberLabelText = ((parentNode.parentNode as DayNightRepresentation).pObj.data as PlacedObject.DayNightData).duskPalette.ToString();
				}
				if (controlPoint == 1)
				{
					base.NumberLabelText = ((parentNode.parentNode as DayNightRepresentation).pObj.data as PlacedObject.DayNightData).nightPalette.ToString();
				}
				base.Refresh();
			}
		}

		public DayNightControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(200f, 40f), "Day Night Settings")
		{
			subNodes.Add(new PaletteController(owner, "Palette", this, new Vector2(5f, 5f), "Night Palette : ", 1));
			subNodes.Add(new PaletteController(owner, "Palette", this, new Vector2(5f, 25f), "Dusk Palette : ", 0));
		}

		public override void Move(Vector2 newPos)
		{
			base.Move(newPos);
			parentNode.Refresh();
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			((parentNode as DayNightRepresentation).pObj.data as PlacedObject.DayNightData).Apply((parentNode as DayNightRepresentation).owner.room);
		}
	}

	public DayNightRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new DayNightControlPanel(owner, "DayNight_Control_Panel", this, new Vector2(10f, 10f)));
		(subNodes[subNodes.Count - 1] as DayNightControlPanel).pos = (pObj.data as PlacedObject.DayNightData).panelPos;
		(pObj.data as PlacedObject.DayNightData).Apply(owner.room);
	}

	public override void Refresh()
	{
		base.Refresh();
		(pObj.data as PlacedObject.DayNightData).panelPos = (subNodes[0] as Panel).pos;
	}
}
