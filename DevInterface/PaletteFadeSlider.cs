using UnityEngine;

namespace DevInterface;

public class PaletteFadeSlider : Slider
{
	public int index;

	public PaletteFadeSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, int index)
		: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 65f)
	{
		this.index = index;
	}

	public override void Refresh()
	{
		base.Refresh();
		if (base.RoomSettings.fadePalette == null)
		{
			base.NumberText = " - ";
			RefreshNubPos(0f);
		}
		else
		{
			base.NumberText = (int)(base.RoomSettings.fadePalette.fades[index] * 100f) + "%";
			RefreshNubPos(base.RoomSettings.fadePalette.fades[index]);
		}
	}

	public override void NubDragged(float nubPos)
	{
		if (base.RoomSettings.fadePalette != null)
		{
			base.RoomSettings.fadePalette.fades[index] = nubPos;
			if (owner.room.game.cameras[0].currentCameraPosition == index)
			{
				owner.room.game.cameras[0].ChangeFadePalette(base.RoomSettings.fadePalette.palette, base.RoomSettings.fadePalette.fades[index]);
			}
			Refresh();
		}
	}
}
