using RWCustom;
using UnityEngine;

namespace Menu;

public class VerticalSlider : Slider
{
	public VerticalSlider(Menu menu, MenuObject owner, string text, Vector2 pos, Vector2 size, SliderID ID, bool subtleSlider)
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
				mouseDragOffset = menu.mousePosition.y - pos.y - ((owner is PositionedMenuObject) ? (owner as PositionedMenuObject).ScreenPos.y : 0f);
			}
			if (mouseDragged)
			{
				base.floatValue = Mathf.InverseLerp(base.RelativeAnchorPoint.y, base.RelativeAnchorPoint.y + length, menu.mousePosition.y - mouseDragOffset);
			}
			buttonBehav.sizeBump = Mathf.Max(buttonBehav.sizeBump, graphicInDraggedMode);
			graphicInDraggedMode = Custom.LerpAndTick(graphicInDraggedMode, mouseDragged ? 1f : 0f, 0.1f, 1f / 30f);
		}
		else
		{
			mouseDragged = false;
			if (Selected && menu.input.y != 0)
			{
				movSpeed = Custom.LerpAndTick(movSpeed, (menu.input.y != 0) ? 1f : 0f, 0.08f, 0.0125f);
				base.floatValue = Mathf.Clamp(base.floatValue + (float)menu.input.y * movSpeed * 0.02f, 0f, 1f);
			}
			else
			{
				movSpeed = Custom.LerpAndTick(movSpeed, 0f, 0.11f, 0.02f);
			}
			graphicInDraggedMode = movSpeed;
		}
		float num = base.RelativeAnchorPoint.y + length * base.floatValue;
		if (Custom.Decimal(num) == 0f)
		{
			num += 0.01f;
		}
		pos.y = num - ((owner is PositionedMenuObject) ? (owner as PositionedMenuObject).ScreenPos.y : 0f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		Vector2 vector;
		Vector2 vector2;
		if (subtleSlider)
		{
			vector = subtleSliderNob.DrawPos(timeStacker) + new Vector2(1f, 1f);
			vector2 = new Vector2(subtleSliderNob.DrawSize(timeStacker), subtleSliderNob.DrawSize(timeStacker)) - new Vector2(2f, 2f);
		}
		else
		{
			menuLabel.label.y = base.RelativeAnchorPoint.y - 20f;
			menuLabel.label.x = DrawX(timeStacker);
			vector = roundedRect.DrawPos(timeStacker);
			vector2 = roundedRect.DrawSize(timeStacker);
			vector -= Vector2.Lerp(roundedRect.lastAddSize, roundedRect.addSize, timeStacker) / 2f;
			vector2 += Vector2.Lerp(roundedRect.lastAddSize, roundedRect.addSize, timeStacker);
		}
		lineSprites[1].y = base.RelativeAnchorPoint.y;
		lineSprites[1].x = base.RelativeAnchorPoint.x + 15f;
		lineSprites[1].isVisible = vector.y > base.RelativeAnchorPoint.y;
		lineSprites[1].scaleY = vector.y - base.RelativeAnchorPoint.y;
		lineSprites[2].y = base.RelativeAnchorPoint.y + length + base.ExtraLengthAtEnd;
		lineSprites[2].x = base.RelativeAnchorPoint.x + 15f;
		lineSprites[2].isVisible = vector.y + vector2.y < base.RelativeAnchorPoint.y + length + base.ExtraLengthAtEnd;
		lineSprites[2].scaleY = length + base.ExtraLengthAtEnd - (vector.y + vector2.y - base.RelativeAnchorPoint.y);
		if (subtleSlider)
		{
			lineSprites[0].isVisible = false;
			lineSprites[3].isVisible = false;
			return;
		}
		lineSprites[0].y = base.RelativeAnchorPoint.y;
		lineSprites[0].x = base.RelativeAnchorPoint.x + 15f;
		lineSprites[0].isVisible = vector.y > base.RelativeAnchorPoint.y;
		lineSprites[3].y = base.RelativeAnchorPoint.y + length + base.ExtraLengthAtEnd;
		lineSprites[3].x = base.RelativeAnchorPoint.x + 15f;
		lineSprites[3].isVisible = vector.y + vector2.y < base.RelativeAnchorPoint.y + length + base.ExtraLengthAtEnd;
	}
}
