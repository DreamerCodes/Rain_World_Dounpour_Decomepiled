using System.Collections.Generic;
using System.Linq;
using Expedition;
using Menu.Remix;
using RWCustom;
using UnityEngine;

namespace Menu;

public class MusicTrackContainer : PositionedMenuObject
{
	public MusicTrackButton[] trackList;

	public RoundedRect borderRect;

	public SymbolButton backPage;

	public SymbolButton forwardPage;

	public MenuLabel pageLabel;

	public MenuLabel pageTitle;

	public int currentPage;

	public int maxPages;

	public float selectedPos;

	public float listSize;

	public MusicTrackContainer(Menu menu, MenuObject owner, Vector2 pos, List<string> trackFilenames)
		: base(menu, owner, pos)
	{
		float[] screenOffsets = Custom.GetScreenOffsets();
		float num = screenOffsets[0];
		_ = screenOffsets[1];
		FSprite fSprite = new FSprite("pixel");
		fSprite.x = pos.x - 15f - num;
		fSprite.y = pos.y - 900f;
		fSprite.SetAnchor(0f, 0f);
		fSprite.scaleX = 270f;
		fSprite.scaleY = 1200f;
		fSprite.color = new Color(0f, 0f, 0f);
		fSprite.alpha = 0.55f;
		Container.AddChild(fSprite);
		borderRect = new RoundedRect(menu, this, new Vector2(-15f, -900f), new Vector2(270f, 1200f), filled: false);
		for (int i = 0; i < borderRect.sprites.Length; i++)
		{
			borderRect.sprites[i].shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
		subObjects.Add(borderRect);
		trackList = new MusicTrackButton[trackFilenames.Count];
		for (int j = 0; j < trackList.Length; j++)
		{
			MusicTrackButton[] array = trackList;
			int num2 = j;
			string displayText = trackFilenames[j];
			string singalText = "mus-" + (j + 1);
			Vector2 vector = new Vector2(0f, 0f - 60f * (float)j);
			Vector2 size = new Vector2(240f, 50f);
			SelectOneButton[] buttonArray = trackList;
			array[num2] = new MusicTrackButton(menu, this, displayText, singalText, vector, size, buttonArray, j);
			subObjects.Add(trackList[j]);
		}
		for (int k = 0; k < trackList.Length; k++)
		{
			if (k == 0)
			{
				trackList[k].nextSelectable[1] = trackList.Last();
			}
			else
			{
				trackList[k].nextSelectable[1] = trackList[k - 1];
			}
			if (k == trackList.Length - 1)
			{
				trackList[k].nextSelectable[3] = trackList[0];
			}
			else
			{
				trackList[k].nextSelectable[3] = trackList[k + 1];
			}
			trackList[k].nextSelectable[0] = (menu as ExpeditionJukebox).nextButton;
			trackList[k].nextSelectable[2] = (menu as ExpeditionJukebox).prevButton;
		}
		listSize = Mathf.Abs(trackList.Last().pos.y);
		ExpLog.Log("Track List Length: " + listSize);
		currentPage = 0;
		maxPages = Mathf.CeilToInt((float)(trackList.Length / 10) + 0.5f);
		pageLabel = new MenuLabel(menu, this, currentPage + 1 + " / " + maxPages, new Vector2(350f, 105f) - pos, default(Vector2), bigText: true);
		pageLabel.label.color = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
		subObjects.Add(pageLabel);
		pageTitle = new MenuLabel(menu, this, menu.Translate("- PAGE -"), pageLabel.pos + new Vector2(0f, -20f), default(Vector2), bigText: false);
		pageTitle.label.color = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
		subObjects.Add(pageTitle);
		backPage = new SymbolButton(menu, this, "pageleft", "PAGEBACK", pageTitle.pos + new Vector2(-105f, -10f));
		backPage.size = new Vector2(60f, 40f);
		backPage.roundedRect.size = backPage.size;
		subObjects.Add(backPage);
		forwardPage = new SymbolButton(menu, this, "pageright", "PAGENEXT", pageTitle.pos + new Vector2(45f, -10f));
		forwardPage.size = new Vector2(60f, 40f);
		forwardPage.roundedRect.size = forwardPage.size;
		subObjects.Add(forwardPage);
		int num3 = 0;
		for (int l = 0; l < trackList.Length; l++)
		{
			if (trackList[l].unlocked)
			{
				num3++;
			}
		}
		string text = menu.Translate("<Unlocked> of <Max> Tracks Unlocked").Replace("<Unlocked>", ValueConverter.ConvertToString(num3)).Replace("<Max>", ValueConverter.ConvertToString(trackList.Length));
		MenuLabel menuLabel = new MenuLabel(menu, this, text, new Vector2(120f, 75f), default(Vector2), bigText: false)
		{
			label = 
			{
				shader = ((num3 == trackList.Length) ? menu.manager.rainWorld.Shaders["MenuTextGold"] : menu.manager.rainWorld.Shaders["MenuText"])
			}
		};
		subObjects.Add(menuLabel);
		if ((menu as ExpeditionJukebox).demoMode)
		{
			menuLabel.label.text = menu.Translate("- D E M O   M O D E -");
		}
		SwitchPage();
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		pageLabel.text = currentPage + 1 + " / " + maxPages;
		for (int i = 0; i < forwardPage.roundedRect.sprites.Length; i++)
		{
			forwardPage.roundedRect.sprites[i].alpha = 0f;
		}
		for (int j = 0; j < backPage.roundedRect.sprites.Length; j++)
		{
			backPage.roundedRect.sprites[j].alpha = 0f;
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "PAGENEXT")
		{
			if (currentPage + 1 < maxPages)
			{
				currentPage++;
				SwitchPage();
			}
			else
			{
				menu.PlaySound(SoundID.MENU_Error_Ping);
			}
		}
		if (message == "PAGEBACK")
		{
			if (currentPage - 1 >= 0)
			{
				currentPage--;
				SwitchPage();
			}
			else
			{
				menu.PlaySound(SoundID.MENU_Error_Ping);
			}
		}
	}

	public void GoToPlayingTrackPage()
	{
		if (trackList == null)
		{
			return;
		}
		for (int i = 0; i < trackList.Length; i++)
		{
			if (trackList[i].AmISelected)
			{
				currentPage = Mathf.FloorToInt(i / 10);
				SwitchPage();
			}
		}
	}

	public void SwitchPage()
	{
		if (trackList == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < trackList.Length; i++)
		{
			if (Mathf.FloorToInt(i / 10) == currentPage)
			{
				trackList[i].pos = new Vector2(0f, -(60 * num));
				trackList[i].lastPos = trackList[i].pos;
				trackList[i].trackColor = (trackList[i].unlocked ? new HSLColor(Mathf.InverseLerp(0f, trackList.Length, i), 1f, 0.7f).rgb : new Color(0.4f, 0.4f, 0.4f));
				num++;
				if (num == 10)
				{
					trackList[i].nextSelectable[3] = forwardPage;
				}
				else if (i == trackList.Length - 1)
				{
					trackList[i].nextSelectable[3] = trackList[i];
				}
				else
				{
					trackList[i].nextSelectable[3] = trackList[i + 1];
				}
			}
			else
			{
				trackList[i].pos = new Vector2(0f, -1000f);
				trackList[i].lastPos = trackList[i].pos;
				trackList[i].nextSelectable[3] = forwardPage;
			}
		}
	}
}
