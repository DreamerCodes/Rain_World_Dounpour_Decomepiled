using System.Collections.Generic;
using UnityEngine;

namespace DevInterface;

public class Cycler : MouseOverSwitchColorLabel
{
	public List<string> alternatives;

	public int currentAlternative;

	public string baseName;

	public Cycler(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string baseName, List<string> alternatives)
		: base(owner, IDstring, parentNode, pos, width, baseName + alternatives[0])
	{
		this.alternatives = alternatives;
		this.baseName = baseName;
	}

	public override void Update()
	{
		base.Update();
		if (base.MouseOver && owner != null && owner.mouseClick)
		{
			currentAlternative++;
			if (currentAlternative >= alternatives.Count)
			{
				currentAlternative = 0;
			}
			base.Text = baseName + alternatives[currentAlternative];
		}
	}
}
