using UnityEngine;

namespace DevInterface;

public class AddSoundType : Button
{
	public int index;

	public AddSoundType(DevUI owner, DevUINode parentNode, Vector2 pos, float width, int index, string text)
		: base(owner, "Add_Sound_Type_" + text, parentNode, pos, width, text)
	{
		this.index = index;
	}

	public override void Update()
	{
		colorA = (((owner.activePage as SoundPage).soundType == index) ? new Color(0f, 0f, 0f) : new Color(1f, 1f, 1f));
		colorB = (((owner.activePage as SoundPage).soundType == index) ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 0f));
		base.Update();
	}

	public override void Clicked()
	{
		(owner.activePage as SoundPage).soundType = index;
	}
}
