using System.Reflection;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public abstract class Dialog : Menu
{
	protected Vector2 size;

	protected Vector2 pos;

	protected MenuLabel descriptionLabel;

	protected OpLabelLong descriptionLabelLong;

	protected FSprite darkSprite;

	private RoundedRect roundedRect;

	protected Page dialogPage;

	public Dialog(string description, ProcessManager manager, bool longLabel = false)
		: base(manager, ProcessManager.ProcessID.Dialog)
	{
		Init(description, new Vector2(478.1f, 115.200005f), longLabel);
	}

	public Dialog(string description, ProcessManager manager)
		: this(description, manager, longLabel: false)
	{
	}

	public Dialog(string longDescription, string title, Vector2 size, ProcessManager manager, bool longLabel = false)
		: base(manager, ProcessManager.ProcessID.Dialog)
	{
		Init(longDescription, title, size);
	}

	public Dialog(string description, Vector2 size, ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.Dialog)
	{
		Init(description, size);
	}

	public Dialog(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.Dialog)
	{
		dialogPage = new Page(this, null, "main", 0);
		pages.Add(dialogPage);
		pos = new Vector2((manager.rainWorld.options.ScreenSize.x - size.x) * 0.5f, (manager.rainWorld.options.ScreenSize.x - size.y) * 0.5f);
		darkSprite = new FSprite("pixel");
		darkSprite.color = new Color(0f, 0f, 0f);
		darkSprite.anchorX = 0f;
		darkSprite.anchorY = 0f;
		darkSprite.scaleX = manager.rainWorld.screenSize.x + 2f;
		darkSprite.scaleY = manager.rainWorld.screenSize.x + 2f;
		darkSprite.x = -1f;
		darkSprite.y = -1f;
		darkSprite.alpha = 0f;
		pages[0].Container.AddChild(darkSprite);
	}

	private void Init(string longDescription, string title, Vector2 size)
	{
		Init(longDescription, size, longLabel: true);
		MenuLabel item = new MenuLabel(this, pages[0], title, new Vector2(pos.x + size.x * 0.07f, pos.y - 30f + size.y - 15f), new Vector2(size.x * 0.86f, 30f), bigText: true);
		pages[0].subObjects.Add(item);
	}

	private void Init(string description, Vector2 size, bool longLabel = false)
	{
		this.size = size;
		pos = new Vector2((1366f - size.x) * 0.5f, (768f - size.y) * 0.5f);
		dialogPage = new Page(this, null, "main", 0);
		pages.Add(dialogPage);
		darkSprite = new FSprite("pixel");
		darkSprite.color = new Color(0f, 0f, 0f);
		darkSprite.anchorX = 0f;
		darkSprite.anchorY = 0f;
		darkSprite.scaleX = 1368f;
		darkSprite.scaleY = 770f;
		darkSprite.x = -1f;
		darkSprite.y = -1f;
		darkSprite.alpha = 0.75f;
		pages[0].Container.AddChild(darkSprite);
		roundedRect = new RoundedRect(this, pages[0], new Vector2(pos.x, pos.y), new Vector2(size.x, size.y), filled: true);
		roundedRect.fillAlpha = 0.95f;
		pages[0].subObjects.Add(roundedRect);
		if (longLabel)
		{
			descriptionLabelLong = new OpLabelLong(Vector2.zero, new Vector2(size.x * 0.86f, size.y * 0.58f - 30f), description);
			descriptionLabelLong.pos = new Vector2(pos.x + size.x * 0.07f, pos.y + size.y - descriptionLabelLong.size.y - 55f);
			MenuTabWrapper menuTabWrapper = new MenuTabWrapper(this, pages[0]);
			pages[0].subObjects.Add(menuTabWrapper);
			new UIelementWrapper(menuTabWrapper, descriptionLabelLong);
		}
		else
		{
			descriptionLabel = new MenuLabel(this, pages[0], description, new Vector2(pos.x + size.x * 0.07f, pos.y + 30f + size.y * 0.08f), new Vector2(size.x * 0.86f, size.y * 0.88f - 30f), bigText: false);
			pages[0].subObjects.Add(descriptionLabel);
		}
	}

	public void HackHide()
	{
		dialogPage.Container.isVisible = false;
	}

	public void HackShow()
	{
		dialogPage.Container.isVisible = true;
		dialogPage.Container.MoveToFront();
		Custom.LogWarning($"{MethodBase.GetCurrentMethod().Name} index:{dialogPage.Container.depth} ");
	}
}
