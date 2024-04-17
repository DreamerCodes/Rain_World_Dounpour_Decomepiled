using UnityEngine;

namespace DevInterface;

public class AddSoundButton : Button
{
	public string sound;

	public AddSoundButton(DevUI owner, DevUINode parentNode, Vector2 pos, float width, string sound)
		: base(owner, "Add_Sound_" + sound, parentNode, pos, width, sound)
	{
		this.sound = sound;
	}

	public override void Clicked()
	{
		DevUINode devUINode = this;
		while (devUINode != null)
		{
			devUINode = devUINode.parentNode;
			if (devUINode is Page)
			{
				(devUINode as IDevUISignals).Signal(DevUISignalType.Create, this, sound);
				break;
			}
		}
	}
}
