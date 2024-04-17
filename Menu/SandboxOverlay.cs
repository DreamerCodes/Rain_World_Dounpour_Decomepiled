using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class SandboxOverlay : Menu
{
	public class MouseDragger : MenuObject
	{
		private BodyChunk dragChunk;

		private Vector2 dragOffset;

		private SandboxOverlay overlay => menu as SandboxOverlay;

		private Room room => overlay.game.cameras[0].room;

		public MouseDragger(Menu menu, MenuObject owner)
			: base(menu, owner)
		{
		}

		public override void Update()
		{
			base.Update();
			if (room.game.pauseMenu != null)
			{
				dragChunk = null;
				return;
			}
			Vector2 vector = menu.mousePosition + overlay.game.cameras[0].pos;
			if (dragChunk != null)
			{
				if (!menu.mouseDown || dragChunk.owner.slatedForDeletetion || (dragChunk.owner is Creature && (dragChunk.owner as Creature).enteringShortCut.HasValue))
				{
					dragChunk = null;
					return;
				}
				dragChunk.vel += vector + dragOffset - dragChunk.pos;
				dragChunk.pos += vector + dragOffset - dragChunk.pos;
			}
			else
			{
				if (!menu.manager.menuesMouseMode || !menu.pressButton)
				{
					return;
				}
				float b = float.MaxValue;
				for (int i = 0; i < room.physicalObjects.Length; i++)
				{
					for (int j = 0; j < room.physicalObjects[i].Count; j++)
					{
						if (room.physicalObjects[i][j].slatedForDeletetion || (room.physicalObjects[i][j] is Creature && (room.physicalObjects[i][j] as Creature).enteringShortCut.HasValue))
						{
							continue;
						}
						for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
						{
							if (Custom.DistLess(vector, room.physicalObjects[i][j].bodyChunks[k].pos, Mathf.Min(room.physicalObjects[i][j].bodyChunks[k].rad + 10f, b)))
							{
								b = Vector2.Distance(vector, room.physicalObjects[i][j].bodyChunks[k].pos);
								dragChunk = room.physicalObjects[i][j].bodyChunks[k];
								dragOffset = dragChunk.pos - vector;
							}
						}
					}
				}
			}
		}
	}

	public RainWorldGame game;

	public SandboxOverlayOwner overlayOwner;

	public SandboxEditorSelector sandboxEditorSelector;

	public TrashBin trashBin;

	public MouseDragger mouseDragger;

	private FSprite fadeSprite;

	private float darkFade;

	private float lastDarkFade;

	public bool playMode;

	public bool fadingOut;

	public SandboxOverlay(ProcessManager manager, RainWorldGame game, SandboxOverlayOwner overlayOwner)
		: base(manager, ProcessManager.ProcessID.PauseMenu)
	{
		this.game = game;
		this.overlayOwner = overlayOwner;
		darkFade = 1f;
		lastDarkFade = 1f;
		pages.Add(new Page(this, null, "main", 0));
		fadeSprite = new FSprite("Futile_White");
		fadeSprite.color = new Color(0f, 0f, 0f);
		fadeSprite.x = game.rainWorld.screenSize.x / 2f;
		fadeSprite.y = game.rainWorld.screenSize.y / 2f;
		fadeSprite.shader = game.rainWorld.Shaders["EdgeFade"];
		Futile.stage.AddChild(fadeSprite);
	}

	public void Initiate(bool playMode)
	{
		this.playMode = playMode;
		if (ModManager.MSC && game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			return;
		}
		if (playMode)
		{
			mouseDragger = new MouseDragger(this, pages[0]);
			pages[0].subObjects.Add(mouseDragger);
			return;
		}
		int num = MultiplayerUnlocks.ItemUnlockList.Count + MultiplayerUnlocks.CreatureUnlockList.Count + 6;
		float num2 = 980f;
		float num3 = 300f;
		SandboxEditorSelector.Width = 18;
		SandboxEditorSelector.Height = 4;
		SandboxEditorSelector.ButtonSize = 50f;
		while (SandboxEditorSelector.Width * SandboxEditorSelector.Height < num || (float)SandboxEditorSelector.Width * SandboxEditorSelector.ButtonSize > num2 || (float)SandboxEditorSelector.Height * SandboxEditorSelector.ButtonSize > num3)
		{
			if ((float)(SandboxEditorSelector.Height + 1) * SandboxEditorSelector.ButtonSize < num3)
			{
				SandboxEditorSelector.Height++;
			}
			else
			{
				SandboxEditorSelector.Height = 4;
				SandboxEditorSelector.ButtonSize = (int)(SandboxEditorSelector.ButtonSize * 0.9f);
			}
			SandboxEditorSelector.Width = (int)(num2 / SandboxEditorSelector.ButtonSize);
		}
		sandboxEditorSelector = new SandboxEditorSelector(this, pages[0], overlayOwner);
		pages[0].subObjects.Add(sandboxEditorSelector);
		trashBin = new TrashBin(this, pages[0], sandboxEditorSelector);
		pages[0].subObjects.Add(trashBin);
	}

	public override void Update()
	{
		base.Update();
		lastDarkFade = darkFade;
		if (fadingOut)
		{
			darkFade = Mathf.Max(0f, darkFade - 1f / 30f);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (lastDarkFade > 0f || darkFade > 0f)
		{
			float num = Mathf.Lerp(lastDarkFade, darkFade, timeStacker);
			fadeSprite.scaleX = (game.rainWorld.screenSize.x * Mathf.Lerp(1.5f, 1f, num) + 2f) / 16f;
			fadeSprite.scaleY = (game.rainWorld.screenSize.y * Mathf.Lerp(2.5f, 1.5f, num) + 2f) / 16f;
			fadeSprite.alpha = Mathf.InverseLerp(0f, 0.9f, num);
		}
		else if (fadeSprite != null)
		{
			fadeSprite.RemoveFromContainer();
			fadeSprite = null;
		}
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		if (fadeSprite != null)
		{
			fadeSprite.RemoveFromContainer();
		}
	}
}
