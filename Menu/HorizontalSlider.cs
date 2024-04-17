using RWCustom;
using UnityEngine;

namespace Menu;

public class HorizontalSlider : Slider
{
	public HorizontalSlider(Menu menu, MenuObject owner, string text, Vector2 pos, Vector2 size, SliderID ID, bool subtleSlider)
		: base(menu, owner, text, pos, size, ID, subtleSlider)
	{
	}

	public override void Update()
	{
		base.Update();
		if (menu.manager.menuesMouseMode)
		{
			if (!menu.mouseDown)
			{
				mouseDragged = false;
			}
			else if (!mouseDragged && Selected && !menu.lastMouseDown)
			{
				mouseDragged = true;
				mouseDragOffset = menu.mousePosition.x - pos.x - ((owner is PositionedMenuObject) ? (owner as PositionedMenuObject).ScreenPos.x : 0f);
				menu.PlaySound(SoundID.MENU_Mouse_Grab_Slider);
			}
			if (mouseDragged)
			{
				base.floatValue = Mathf.InverseLerp(base.RelativeAnchorPoint.x, base.RelativeAnchorPoint.x + length, menu.mousePosition.x - mouseDragOffset);
			}
			buttonBehav.sizeBump = Mathf.Max(buttonBehav.sizeBump, graphicInDraggedMode);
			graphicInDraggedMode = Custom.LerpAndTick(graphicInDraggedMode, mouseDragged ? 1f : 0f, 0.1f, 1f / 30f);
		}
		else
		{
			mouseDragged = false;
			if (Selected && menu.input.x != 0)
			{
				movSpeed = Custom.LerpAndTick(movSpeed, (menu.input.x != 0) ? 1f : 0f, 0.08f, 0.0125f);
				base.floatValue = Mathf.Clamp(base.floatValue + (float)menu.input.x * movSpeed * 0.02f, 0f, 1f);
			}
			else
			{
				movSpeed = Custom.LerpAndTick(movSpeed, 0f, 0.11f, 0.02f);
			}
			graphicInDraggedMode = movSpeed;
		}
		float num = base.RelativeAnchorPoint.x + length * base.floatValue;
		if (Custom.Decimal(num) == 0f)
		{
			num += 0.01f;
		}
		pos.x = num - ((owner is PositionedMenuObject) ? (owner as PositionedMenuObject).ScreenPos.x : 0f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		Vector2 vector;
		Vector2 vector2;
		if (subtleSlider)
		{
			vector = subtleSliderNob.DrawPos(timeStacker);
			vector2 = new Vector2(subtleSliderNob.DrawSize(timeStacker), subtleSliderNob.DrawSize(timeStacker));
		}
		else
		{
			menuLabel.label.x = base.RelativeAnchorPoint.x + length + 40.01f;
			menuLabel.label.y = DrawY(timeStacker) + 16f;
			vector = roundedRect.DrawPos(timeStacker);
			vector2 = roundedRect.DrawSize(timeStacker);
			vector -= Vector2.Lerp(roundedRect.lastAddSize, roundedRect.addSize, timeStacker) / 2f;
			vector2 += Vector2.Lerp(roundedRect.lastAddSize, roundedRect.addSize, timeStacker);
			vector.x = Mathf.Floor(vector.x) + 0.01f;
			vector.y = Mathf.Floor(vector.y) + 0.01f;
			vector2.x = Mathf.Round(vector2.x) + 0.01f;
			vector2.y = Mathf.Round(vector2.y) + 0.01f;
		}
		lineSprites[1].x = base.RelativeAnchorPoint.x;
		lineSprites[1].y = base.RelativeAnchorPoint.y + 15f;
		lineSprites[1].isVisible = vector.x > base.RelativeAnchorPoint.x;
		lineSprites[1].scaleX = vector.x - base.RelativeAnchorPoint.x;
		lineSprites[2].x = base.RelativeAnchorPoint.x + length + base.ExtraLengthAtEnd;
		lineSprites[2].y = base.RelativeAnchorPoint.y + 15f;
		lineSprites[2].isVisible = vector.x + vector2.x < base.RelativeAnchorPoint.x + length + base.ExtraLengthAtEnd;
		lineSprites[2].scaleX = length + base.ExtraLengthAtEnd - (vector.x + vector2.x - base.RelativeAnchorPoint.x);
		if (subtleSlider)
		{
			lineSprites[0].isVisible = false;
			lineSprites[3].isVisible = false;
			return;
		}
		lineSprites[0].x = base.RelativeAnchorPoint.x;
		lineSprites[0].y = base.RelativeAnchorPoint.y + 15f;
		lineSprites[0].isVisible = vector.x > base.RelativeAnchorPoint.x;
		lineSprites[3].x = base.RelativeAnchorPoint.x + length + base.ExtraLengthAtEnd;
		lineSprites[3].y = base.RelativeAnchorPoint.y + 15f;
		lineSprites[3].isVisible = vector.x + vector2.x < base.RelativeAnchorPoint.x + length + base.ExtraLengthAtEnd;
	}
}
