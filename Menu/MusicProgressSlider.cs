using UnityEngine;

namespace Menu;

public class MusicProgressSlider : HorizontalSlider
{
	public float fade;

	public float lastFade;

	public MusicProgressSlider(Menu menu, MenuObject owner, string text, Vector2 pos, Vector2 size, SliderID ID, bool subtleSlider)
		: base(menu, owner, text, pos, size, ID, subtleSlider)
	{
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		for (int i = 0; i < lineSprites.Length; i++)
		{
			lineSprites[i].alpha = 0f;
		}
		for (int j = 0; j < roundedRect.sprites.Length; j++)
		{
			if (j != 8)
			{
				roundedRect.sprites[j].alpha = 0f;
				continue;
			}
			if (roundedRect.sprites[j].element.name != "Multiplayer_Arrow")
			{
				roundedRect.sprites[j].SetElementByName("Multiplayer_Arrow");
			}
			roundedRect.sprites[j].x = pos.x + 10f + Mathf.Lerp(roundedRect.lastPos.x, roundedRect.pos.x, timeStacker) + owner.page.pos.x;
			roundedRect.sprites[j].y = pos.y + 15f + Mathf.Lerp(roundedRect.lastPos.y, roundedRect.pos.y, timeStacker) + owner.page.pos.y;
			roundedRect.sprites[j].scaleX = 1f;
			roundedRect.sprites[j].scaleY = 1.5f;
			roundedRect.sprites[j].rotation = 180f;
			roundedRect.sprites[j].color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Color.white, Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker));
			roundedRect.sprites[j].alpha = 1f;
			roundedRect.sprites[j].SetAnchor(0.5f, 0.5f);
		}
		for (int k = 0; k < selectRect.sprites.Length; k++)
		{
			selectRect.sprites[k].alpha = 0f;
		}
	}
}
