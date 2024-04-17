using UnityEngine;

namespace DevInterface;

public class PaletteController : IntegerControl
{
	private int controlPoint;

	public PaletteController(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, int controlPoint)
		: base(owner, IDstring, parentNode, pos, title)
	{
		this.controlPoint = controlPoint;
	}

	public override void Refresh()
	{
		string text = "";
		switch (controlPoint)
		{
		case 0:
			text = (base.RoomSettings.pal.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.pal.HasValue) ? "<A>" : "<T>"));
			base.NumberLabelText = text + " " + base.RoomSettings.Palette;
			break;
		case 1:
			text = (base.RoomSettings.eColA.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.eColA.HasValue) ? "<A>" : "<T>"));
			base.NumberLabelText = text + " " + base.RoomSettings.EffectColorA;
			break;
		case 2:
			text = (base.RoomSettings.eColB.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.eColB.HasValue) ? "<A>" : "<T>"));
			base.NumberLabelText = text + " " + base.RoomSettings.EffectColorB;
			break;
		case 3:
			if (base.RoomSettings.fadePalette == null)
			{
				base.NumberLabelText = "NONE";
			}
			else
			{
				base.NumberLabelText = base.RoomSettings.fadePalette.palette.ToString();
			}
			break;
		}
		base.Refresh();
	}

	public override void Increment(int change)
	{
		switch (controlPoint)
		{
		case 0:
			if (base.RoomSettings.pal.HasValue)
			{
				base.RoomSettings.pal += change;
				if (base.RoomSettings.pal.Value < 0)
				{
					base.RoomSettings.pal = null;
				}
			}
			else
			{
				base.RoomSettings.pal = 0;
			}
			owner.room.game.cameras[0].ChangeMainPalette(base.RoomSettings.Palette);
			break;
		case 1:
			if (base.RoomSettings.eColA.HasValue)
			{
				base.RoomSettings.eColA += change;
				if (base.RoomSettings.eColA.Value < 0)
				{
					base.RoomSettings.eColA = null;
				}
			}
			else
			{
				base.RoomSettings.eColA = 0;
			}
			owner.room.game.cameras[0].ApplyEffectColorsToAllPaletteTextures(base.RoomSettings.EffectColorA, base.RoomSettings.EffectColorB);
			break;
		case 2:
			if (base.RoomSettings.eColB.HasValue)
			{
				base.RoomSettings.eColB += change;
				if (base.RoomSettings.eColB.Value < 0)
				{
					base.RoomSettings.eColB = null;
				}
			}
			else
			{
				base.RoomSettings.eColB = 0;
			}
			owner.room.game.cameras[0].ApplyEffectColorsToAllPaletteTextures(base.RoomSettings.EffectColorA, base.RoomSettings.EffectColorB);
			break;
		case 3:
			if (base.RoomSettings.fadePalette == null)
			{
				base.RoomSettings.fadePalette = new RoomSettings.FadePalette(0, owner.room.cameraPositions.Length);
			}
			else
			{
				base.RoomSettings.fadePalette.palette += change;
				if (base.RoomSettings.fadePalette.palette < 0)
				{
					base.RoomSettings.fadePalette = null;
					owner.room.game.cameras[0].ChangeFadePalette(-1, 0f);
				}
				else
				{
					owner.room.game.cameras[0].ChangeFadePalette(base.RoomSettings.fadePalette.palette, base.RoomSettings.fadePalette.fades[owner.room.game.cameras[0].currentCameraPosition]);
				}
			}
			parentNode.Refresh();
			break;
		}
		Refresh();
	}
}
