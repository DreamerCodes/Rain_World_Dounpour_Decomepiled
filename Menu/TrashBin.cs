using System;
using ArenaBehaviors;
using RWCustom;
using UnityEngine;

namespace Menu;

public class TrashBin : RectangularMenuObject
{
	public SandboxEditorSelector editorSelector;

	private MenuLabel label;

	private bool active;

	private bool overlapped;

	private bool lastOverlapped;

	private float fade;

	private float lastFade;

	private RoundedRect roundedRect;

	public float sin;

	public float lastSin;

	public float red;

	public float lastRed;

	private float lastBkgFade;

	private float bkgFade;

	public int bump;

	public int lingerCounter;

	public int smallBump;

	public int bigRangeCounter = -1;

	public bool firstDrag = true;

	public TrashBin(Menu menu, MenuObject owner, SandboxEditorSelector editorSelector)
		: base(menu, owner, new Vector2(60f, 60f), new Vector2(100f, 100f))
	{
		this.editorSelector = editorSelector;
		roundedRect = new RoundedRect(menu, this, new Vector2(0.01f, 0.01f), size, filled: true);
		subObjects.Add(roundedRect);
		label = new MenuLabel(menu, this, menu.manager.rainWorld.inGameTranslator.Translate("REMOVE"), size / 2f + new Vector2(-30f, -10f), new Vector2(60f, 20f), bigText: false);
		subObjects.Add(label);
	}

	public override void Update()
	{
		base.Update();
		lastFade = fade;
		lastSin = sin;
		lastRed = red;
		lastBkgFade = bkgFade;
		lastOverlapped = overlapped;
		active = false;
		if (!editorSelector.currentlyVisible)
		{
			for (int i = 0; i < editorSelector.editor.cursors.Count; i++)
			{
				if (editorSelector.editor.cursors[i].mouseMode)
				{
					if (editorSelector.editor.cursors[i].dragIcon != null)
					{
						active = true;
						lingerCounter = 30;
						overlapped = MouseOver;
					}
					break;
				}
			}
		}
		else
		{
			lingerCounter = 0;
		}
		if (editorSelector.editor.gameSession.game.pauseMenu != null)
		{
			active = false;
			lingerCounter = 0;
		}
		if (active && bigRangeCounter < 0)
		{
			bigRangeCounter = 100;
		}
		else if (!active && bigRangeCounter > 0)
		{
			firstDrag = false;
		}
		if (bigRangeCounter > 0 && !firstDrag)
		{
			bigRangeCounter--;
		}
		if (bump > 0)
		{
			bump--;
			active = true;
			overlapped = true;
		}
		if (lingerCounter > 0)
		{
			lingerCounter--;
			if (!firstDrag && bigRangeCounter <= 0 && !Custom.DistLess(Futile.mousePosition, DrawPos(1f), 400f))
			{
				lingerCounter = 0;
			}
		}
		fade = Custom.LerpAndTick(fade, ((active || lingerCounter > 0) && (firstDrag || bigRangeCounter > 0 || Custom.DistLess(Futile.mousePosition, DrawPos(1f), 400f))) ? 1f : 0f, 0.05f, 1f / 30f);
		red = Custom.LerpAndTick(red, overlapped ? 1f : 0f, 0.05f, 1f / 30f);
		roundedRect.addSize = new Vector2(1f, 1f) * (Mathf.Lerp(-25f, 0f, Custom.SCurve(fade, 0.65f)) + (float)bump);
		if (smallBump > 0)
		{
			smallBump--;
		}
		if (overlapped)
		{
			sin += red;
			roundedRect.addSize += new Vector2(1f, 1f) * (Mathf.Sin(sin / 30f * (float)Math.PI * 2f) * 2f + (float)smallBump * 0.4f);
			if (!lastOverlapped)
			{
				smallBump = 15;
			}
		}
		bkgFade = Custom.LerpAndTick(bkgFade, overlapped ? 0f : 1f, 0.01f, 1f / 30f);
	}

	public void IconReleased(SandboxEditor.PlacedIcon icon)
	{
		if (MouseOver && overlapped)
		{
			editorSelector.editor.RemoveIcon(icon, updatePerfEstimate: true);
			menu.PlaySound(SoundID.SANDBOX_Remove_Item);
			Bump();
		}
	}

	public void Bump()
	{
		bump = 20;
		red = 1f;
		sin = (float)Math.PI / 2f;
		lastSin = sin;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (lastFade == 0f && fade == 0f)
		{
			for (int i = 0; i < roundedRect.sprites.Length; i++)
			{
				roundedRect.sprites[i].isVisible = false;
			}
			label.label.isVisible = false;
			return;
		}
		label.label.isVisible = true;
		float num = Custom.SCurve(Mathf.Lerp(lastFade, fade, timeStacker), 0.65f);
		for (int j = 0; j < 9; j++)
		{
			roundedRect.sprites[j].color = Color.black;
			roundedRect.sprites[j].alpha = 0.75f * num * Mathf.Lerp(lastBkgFade, bkgFade, timeStacker);
			roundedRect.sprites[j].isVisible = true;
		}
		Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Color.red, Mathf.Lerp(lastRed, red, timeStacker) * (0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastSin, sin, timeStacker) / 30f * (float)Math.PI * 2f)));
		label.label.color = color;
		label.label.alpha = num;
		num = Mathf.InverseLerp(0f, 0.25f, num);
		for (int k = 9; k < 17; k++)
		{
			roundedRect.sprites[k].color = color;
			roundedRect.sprites[k].alpha = num;
			roundedRect.sprites[k].isVisible = true;
		}
	}
}
