using UnityEngine;

namespace DevInterface;

public class IntegerControl : PositionedDevUINode, IDevUISignals
{
	public string NumberLabelText
	{
		get
		{
			return subNodes[1].fLabels[0].text;
		}
		set
		{
			subNodes[1].fLabels[0].text = value;
		}
	}

	public IntegerControl(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
		: base(owner, IDstring, parentNode, pos)
	{
		subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0f, 0f), 110f, title));
		subNodes.Add(new DevUILabel(owner, "Number", this, new Vector2(140f, 0f), 36f, "0"));
		subNodes.Add(new ArrowButton(owner, "Less", this, new Vector2(120f, 0f), -90f));
		subNodes.Add(new ArrowButton(owner, "More", this, new Vector2(180f, 0f), 90f));
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender.IDstring == "Less")
		{
			Increment(Input.GetKey(KeyCode.LeftShift) ? (-10) : (-1));
		}
		if (sender.IDstring == "More")
		{
			Increment((!Input.GetKey(KeyCode.LeftShift)) ? 1 : 10);
		}
	}

	public virtual void Increment(int change)
	{
	}
}
