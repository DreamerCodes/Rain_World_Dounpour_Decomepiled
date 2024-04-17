using System;
using System.Collections.Generic;
using ArenaBehaviors;
using RWCustom;
using UnityEngine;

namespace Menu;

public class SandboxEditorSelector : RectangularMenuObject
{
	public class ButtonCursor : RectangularMenuObject
	{
		public IntVector2 intPos;

		private RoundedRect roundedRect;

		private RoundedRect extraRect;

		public SandboxEditor.EditCursor roomCursor;

		public Button selectedButton;

		private float active;

		private float lastActive;

		private float sin;

		private float lastSin;

		private float innerVisible;

		private float lastInnerVisible;

		private int stillCounter;

		public bool clickOnRelease;

		public IntVector2 lastMouseIntPos;

		public SandboxEditorSelector EditorSelector => owner as SandboxEditorSelector;

		public ButtonCursor(Menu menu, MenuObject owner, IntVector2 intPos, SandboxEditor.EditCursor roomCursor)
			: base(menu, owner, intPos.ToVector2() * ButtonSize, new Vector2(ButtonSize, ButtonSize))
		{
			this.intPos = intPos;
			this.roomCursor = roomCursor;
			roomCursor.menuCursor = this;
			roundedRect = new RoundedRect(menu, this, new Vector2(-2f, -2f), size + new Vector2(4f, 4f), filled: false);
			subObjects.Add(roundedRect);
			extraRect = new RoundedRect(menu, this, new Vector2(-2f, -2f), size + new Vector2(4f, 4f), filled: false);
			subObjects.Add(extraRect);
			selectedButton = EditorSelector.buttons[intPos.x, intPos.y];
		}

		public void Move(int xAdd, int yAdd)
		{
			if (xAdd == 0 && yAdd == 0)
			{
				return;
			}
			menu.PlaySound(SoundID.SANDBOX_Move_Library_Cursor);
			intPos.x += xAdd;
			if (intPos.x < 0)
			{
				intPos.x = Width - 1;
			}
			else if (intPos.x >= Width)
			{
				intPos.x = 0;
			}
			intPos.y += yAdd;
			if (intPos.y < 0)
			{
				intPos.y = Height - 1;
			}
			else if (intPos.y >= Height)
			{
				intPos.y = 0;
			}
			stillCounter = 0;
			clickOnRelease = false;
			selectedButton = EditorSelector.buttons[intPos.x, intPos.y];
			if (selectedButton != null)
			{
				selectedButton.Flash();
			}
			if (roomCursor.mouseMode)
			{
				return;
			}
			for (int i = 0; i < EditorSelector.editor.cursors.Count; i++)
			{
				if (EditorSelector.editor.cursors[i].menuMode && EditorSelector.editor.cursors[i].playerNumber < roomCursor.playerNumber)
				{
					return;
				}
			}
			EditorSelector.UpdateInfoLabel(intPos.x, intPos.y);
		}

		public void Click()
		{
			clickOnRelease = false;
			if (intPos.x < 0 || intPos.x >= Width || intPos.y < 0 || intPos.y >= Height || EditorSelector.buttons[intPos.x, intPos.y] == null)
			{
				menu.PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
			}
			else
			{
				EditorSelector.buttons[intPos.x, intPos.y].Clicked(this);
			}
		}

		public override void Update()
		{
			base.Update();
			lastActive = active;
			lastSin = sin;
			lastInnerVisible = innerVisible;
			if (roomCursor.menuMode && roomCursor.mouseMode && EditorSelector.editor.gameSession.game.pauseMenu == null)
			{
				IntVector2 intVector = IntVector2.FromVector2(((Vector2)Futile.mousePosition - EditorSelector.DrawPos(1f)) / ButtonSize);
				if (roomCursor.input[0].thrw && !roomCursor.input[1].thrw)
				{
					if (intVector.x >= 0 && intVector.x < Width && intVector.y >= 0 && intVector.y < Height)
					{
						if (intVector != intPos)
						{
							Move(intVector.x - intPos.x, intVector.y - intPos.y);
						}
						if (!(selectedButton is CreatureOrItemButton))
						{
							Click();
						}
					}
					else if (selectedButton is CreatureOrItemButton)
					{
						Click();
					}
				}
				if (intVector != intPos && intVector.x >= 0 && intVector.x < Width && intVector.y >= 0 && intVector.y < Height)
				{
					if (intVector != lastMouseIntPos)
					{
						lastMouseIntPos = intVector;
						EditorSelector.UpdateInfoLabel(intVector.x, intVector.y);
						if (EditorSelector.buttons[intVector.x, intVector.y] != null)
						{
							EditorSelector.buttons[intVector.x, intVector.y].Flash();
						}
					}
				}
				else
				{
					EditorSelector.UpdateInfoLabel(intPos.x, intPos.y);
				}
			}
			else
			{
				lastMouseIntPos = new IntVector2(-1, -1);
			}
			active = Custom.LerpAndTick(active, roomCursor.menuMode ? 1f : 0f, 0.03f, 1f / 15f);
			Vector2 vector = intPos.ToVector2() * ButtonSize;
			pos = Vector2.Lerp(Custom.MoveTowards(pos, vector, 5f), vector, 0.4f);
			float t = ((Custom.DistLess(pos, vector, 10f) && !roomCursor.input[0].thrw) ? 1f : 0f) * active;
			roundedRect.addSize = new Vector2(1f, 1f) * Mathf.Lerp(-8f, 0f, t);
			extraRect.addSize = new Vector2(1f, 1f) * -8f;
			stillCounter++;
			sin += innerVisible;
			innerVisible = Custom.LerpAndTick(innerVisible, Mathf.InverseLerp(10f, 30f, stillCounter), 0.06f, 0.04f);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			float num = Custom.SCurve(Mathf.Lerp(lastActive, active, timeStacker), 0.65f);
			Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(roomCursor.playerNumber)), 0.2f + 0.8f * num);
			if (roomCursor.playerNumber == 3)
			{
				color = Color.Lerp(Custom.Saturate(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(roomCursor.playerNumber)), 0.5f), Color.white, 0.2f);
			}
			for (int i = 0; i < roundedRect.sprites.Length; i++)
			{
				roundedRect.sprites[i].color = color;
				roundedRect.sprites[i].alpha = 1f;
				roundedRect.sprites[i].isVisible = true;
			}
			float num2 = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastSin, sin, timeStacker) / 30f * (float)Math.PI * 2f);
			num2 *= num * Mathf.Lerp(lastInnerVisible, innerVisible, timeStacker);
			for (int j = 0; j < extraRect.sprites.Length; j++)
			{
				extraRect.sprites[j].color = color;
				extraRect.sprites[j].alpha = num2;
				extraRect.sprites[j].isVisible = true;
			}
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
		}
	}

	public class Button : RectangularMenuObject
	{
		public IntVector2 intPos;

		public FSprite rightDivider;

		public FSprite upDivider;

		protected int counter;

		public float lastSin;

		public float sin;

		public SandboxEditorSelector EditorSelector => owner as SandboxEditorSelector;

		public virtual string DescriptorText => "";

		public override bool Selected
		{
			get
			{
				for (int i = 0; i < EditorSelector.cursors.Count; i++)
				{
					if (EditorSelector.cursors[i].roomCursor.menuMode && EditorSelector.cursors[i].selectedButton == this)
					{
						return true;
					}
				}
				return false;
			}
		}

		public virtual float White(float timeStacker)
		{
			return Mathf.Lerp(lastSin, sin, timeStacker) * (0.5f + 0.5f * Mathf.Sin(((float)counter + timeStacker) / 30f * (float)Math.PI * 2f));
		}

		public Button(Menu menu, MenuObject owner)
			: base(menu, owner, default(Vector2), new Vector2(ButtonSize, ButtonSize))
		{
		}

		public virtual void Initiate(IntVector2 intPos)
		{
			this.intPos = intPos;
			pos = intPos.ToVector2() * ButtonSize;
			lastPos = pos;
			if (intPos.x < Width - 1 && EditorSelector.buttons[intPos.x + 1, intPos.y] != null)
			{
				rightDivider = new FSprite("pixel");
				rightDivider.scaleX = 2f;
				rightDivider.scaleY = ButtonSize - 15f;
				rightDivider.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
				Container.AddChild(rightDivider);
			}
			if (intPos.y < Height - 1 && EditorSelector.buttons[intPos.x, intPos.y + 1] != null)
			{
				upDivider = new FSprite("pixel");
				upDivider.scaleX = ButtonSize - 15f;
				upDivider.scaleY = 2f;
				upDivider.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
				Container.AddChild(upDivider);
			}
		}

		public virtual void Flash()
		{
		}

		public virtual void Clicked(ButtonCursor cursor)
		{
		}

		public override void Update()
		{
			base.Update();
			lastSin = sin;
			counter++;
			sin = Custom.LerpAndTick(sin, Selected ? 1f : 0f, 0.03f, 1f / 30f);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			Vector2 vector = DrawPos(timeStacker);
			if (rightDivider != null)
			{
				rightDivider.x = vector.x + size.x;
				rightDivider.y = vector.y + size.y / 2f;
			}
			if (upDivider != null)
			{
				upDivider.x = vector.x + size.x / 2f;
				upDivider.y = vector.y + size.y;
			}
		}

		public override void RemoveSprites()
		{
			if (rightDivider != null)
			{
				rightDivider.RemoveFromContainer();
			}
			if (upDivider != null)
			{
				upDivider.RemoveFromContainer();
			}
			base.RemoveSprites();
		}
	}

	public class CreatureOrItemButton : Button
	{
		public IconSymbol symbol;

		public IconSymbol.IconSymbolData data => symbol.iconData;

		public override string DescriptorText => menu.Translate((data.itemType == AbstractPhysicalObject.AbstractObjectType.Creature) ? "Add creature" : "Add item");

		public CreatureOrItemButton(Menu menu, MenuObject owner, IconSymbol.IconSymbolData data)
			: base(menu, owner)
		{
			symbol = IconSymbol.CreateIconSymbol(data, Container);
			symbol.Show(showShadowSprites: true);
		}

		public override void Flash()
		{
			base.Flash();
			symbol.showFlash = 1f;
		}

		public override void Clicked(ButtonCursor cursor)
		{
			base.Clicked(cursor);
			cursor.roomCursor.SpawnObject(data, cursor.roomCursor.room.game.GetNewID());
		}

		public override void Update()
		{
			base.Update();
			symbol.Update();
			symbol.showFlash = Mathf.Max(symbol.showFlash, White(1f));
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			symbol.Draw(timeStacker, DrawPos(timeStacker) + DrawSize(timeStacker) / 2f);
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
			symbol.RemoveSprites();
		}
	}

	public abstract class ActionButton : Button
	{
		public class Action : ExtEnum<Action>
		{
			public static readonly Action ClearAll = new Action("ClearAll", register: true);

			public static readonly Action Play = new Action("Play", register: true);

			public static readonly Action Randomize = new Action("Randomize", register: true);

			public static readonly Action ConfigA = new Action("ConfigA", register: true);

			public static readonly Action ConfigB = new Action("ConfigB", register: true);

			public static readonly Action ConfigC = new Action("ConfigC", register: true);

			public static readonly Action Locked = new Action("Locked", register: true);

			public Action(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Action action;

		public string symbolSpriteName;

		public string descriptorText;

		public Color symbolSpriteColor;

		public FSprite symbolSprite;

		public FSprite shadow1;

		public FSprite shadow2;

		public float bump;

		public float lastBump;

		public override string DescriptorText => descriptorText;

		public override float White(float timeStacker)
		{
			return Mathf.Max(Mathf.Lerp(lastBump, bump, timeStacker), base.White(timeStacker));
		}

		public virtual Color MyColor(float timeStacker)
		{
			if (action == Action.Play && base.EditorSelector.editor.performanceWarning > 0)
			{
				return Color.Lerp(symbolSpriteColor, Color.red, 0.5f + 0.5f * Mathf.Sin(((float)counter + timeStacker) / 30f * (float)Math.PI * 2f));
			}
			return Color.Lerp(symbolSpriteColor, Color.white, White(timeStacker));
		}

		public override void Flash()
		{
			base.Flash();
			bump = 1f;
		}

		public ActionButton(Menu menu, MenuObject owner, Action action)
			: base(menu, owner)
		{
			this.action = action;
			descriptorText = "";
			symbolSpriteName = "Futile_White";
			symbolSpriteColor = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
			if (action == Action.ClearAll)
			{
				symbolSpriteName = "Sandbox_ClearAll";
				descriptorText = menu.Translate("Clear all");
			}
			else if (action == Action.Play)
			{
				symbolSpriteName = "Sandbox_Play";
				descriptorText = menu.Translate("Play!");
			}
			else if (action == Action.ConfigA)
			{
				symbolSpriteName = "Sandbox_A";
				descriptorText = menu.Translate("Configuration") + " A";
			}
			else if (action == Action.ConfigB)
			{
				symbolSpriteName = "Sandbox_B";
				descriptorText = menu.Translate("Configuration") + " B";
			}
			else if (action == Action.ConfigC)
			{
				symbolSpriteName = "Sandbox_C";
				descriptorText = menu.Translate("Configuration") + " C";
			}
			else if (action == Action.Locked)
			{
				symbolSpriteName = "Sandbox_QuestionMark";
			}
			else if (action == Action.Randomize)
			{
				symbolSpriteName = "Sandbox_Randomize";
			}
			shadow1 = new FSprite(symbolSpriteName);
			shadow1.color = Color.black;
			Container.AddChild(shadow1);
			shadow2 = new FSprite(symbolSpriteName);
			shadow2.color = Color.black;
			Container.AddChild(shadow2);
			symbolSprite = new FSprite(symbolSpriteName);
			symbolSprite.color = symbolSpriteColor;
			Container.AddChild(symbolSprite);
		}

		public override void Clicked(ButtonCursor cursor)
		{
			base.Clicked(cursor);
			if (action == Action.ClearAll)
			{
				cursor.roomCursor.Bump(redBump: true);
			}
			base.EditorSelector.ActionButtonClicked(this);
			bump = 1f;
		}

		public override void Update()
		{
			lastBump = bump;
			bump = Mathf.Max(0f, bump - 0.05f);
			base.Update();
		}

		public override void GrafUpdate(float timeStacker)
		{
			Vector2 vector = DrawPos(timeStacker) + DrawSize(timeStacker) / 2f;
			shadow1.x = vector.x - 2f;
			shadow1.y = vector.y - 1f;
			shadow2.x = vector.x - 1f;
			shadow2.y = vector.y + 1f;
			symbolSprite.x = vector.x;
			symbolSprite.y = vector.y;
			symbolSprite.color = MyColor(timeStacker);
			base.GrafUpdate(timeStacker);
		}

		public override void RemoveSprites()
		{
			shadow1.RemoveFromContainer();
			shadow2.RemoveFromContainer();
			symbolSprite.RemoveFromContainer();
			base.RemoveSprites();
		}
	}

	public class LockedButton : ActionButton
	{
		public override Color MyColor(float timeStacker)
		{
			return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), Mathf.Lerp(lastBump, bump, timeStacker));
		}

		public LockedButton(Menu menu, MenuObject owner)
			: base(menu, owner, Action.Locked)
		{
		}

		public override void Update()
		{
			base.Update();
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
		}
	}

	public class RandomizeButton : ActionButton
	{
		public bool Active
		{
			get
			{
				return !base.EditorSelector.editor.gameSession.GameTypeSetup.saveCreatures;
			}
			set
			{
				base.EditorSelector.editor.gameSession.GameTypeSetup.saveCreatures = !value;
			}
		}

		public override string DescriptorText => menu.Translate(Active ? "Random creatures" : "Persistent creatures");

		public override Color MyColor(float timeStacker)
		{
			if (Active)
			{
				return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(lastSin, sin, timeStacker));
			}
			return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), Mathf.Lerp(lastBump, bump, timeStacker));
		}

		public RandomizeButton(Menu menu, MenuObject owner)
			: base(menu, owner, Action.Randomize)
		{
		}

		public override void Update()
		{
			base.Update();
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
		}
	}

	public class RectButton : ActionButton
	{
		public RoundedRect roundedRect;

		public RectButton(Menu menu, MenuObject owner, Action action)
			: base(menu, owner, action)
		{
			roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: false);
			subObjects.Add(roundedRect);
		}

		public override void Update()
		{
			base.Update();
			roundedRect.addSize = new Vector2(1f, 1f) * Mathf.Lerp(-8f, -2f, bump);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), Menu.MenuRGB(Menu.MenuColors.White), White(timeStacker));
			float alpha = 1f - Mathf.Lerp(lastSin, sin, timeStacker);
			for (int i = 0; i < roundedRect.sprites.Length; i++)
			{
				roundedRect.sprites[i].color = color;
				roundedRect.sprites[i].alpha = alpha;
				roundedRect.sprites[i].isVisible = true;
			}
		}
	}

	public class ConfigButton : ActionButton
	{
		public RoundedRect roundedRect;

		private float rectVisible;

		private float lastRectVisible;

		private int configNumber;

		public ConfigButton(Menu menu, MenuObject owner, Action action, int configNumber)
			: base(menu, owner, action)
		{
			this.configNumber = configNumber;
			roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: false);
			subObjects.Add(roundedRect);
		}

		public override void Update()
		{
			base.Update();
			lastRectVisible = rectVisible;
			rectVisible = Custom.LerpAndTick(rectVisible, (base.EditorSelector.editor.currentConfig == configNumber) ? 1f : 0f, 0.03f, 1f / 30f);
			roundedRect.addSize = new Vector2(1f, 1f) * Mathf.Lerp(-8f, -2f, bump);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			symbolSprite.color = Color.Lerp(Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.MediumGrey), Mathf.Lerp(lastRectVisible, rectVisible, timeStacker)), Menu.MenuRGB(Menu.MenuColors.White), White(timeStacker));
			Color color = Color.Lerp(Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), Mathf.Lerp(lastRectVisible, rectVisible, timeStacker)), Menu.MenuRGB(Menu.MenuColors.White), White(timeStacker));
			float alpha = Mathf.Lerp(lastRectVisible, rectVisible, timeStacker) * (1f - Mathf.Lerp(lastSin, sin, timeStacker));
			for (int i = 0; i < roundedRect.sprites.Length; i++)
			{
				roundedRect.sprites[i].color = color;
				roundedRect.sprites[i].alpha = alpha;
				roundedRect.sprites[i].isVisible = true;
			}
		}
	}

	private RoundedRect bkgRect;

	private SandboxOverlayOwner overlayOwner;

	public SandboxEditor editor;

	public bool currentlyVisible;

	public int lingerCounter;

	public float visFac;

	public float lastVisFac;

	public Button[,] buttons;

	public static float ButtonSize = 50f;

	public static int Width = 19;

	public static int Height = 4;

	public bool upTop;

	public bool mouseModeClickedDownLow;

	public MenuLabel infoLabel;

	public List<ButtonCursor> cursors;

	private int counter;

	public MultiplayerUnlocks unlocks => overlayOwner.gameSession.arenaSitting.multiplayerUnlocks;

	public SandboxEditorSelector(Menu menu, MenuObject owner, SandboxOverlayOwner overlayOwner)
		: base(menu, owner, new Vector2(-1000f, -1000f), new Vector2(Width, Height) * ButtonSize)
	{
		lastPos = new Vector2(-1000f, -1000f);
		this.overlayOwner = overlayOwner;
		overlayOwner.selector = this;
		bkgRect = new RoundedRect(menu, this, new Vector2(-10f, -30f), size + new Vector2(20f, 60f), filled: true);
		subObjects.Add(bkgRect);
		infoLabel = new MenuLabel(menu, this, "", new Vector2(size.x / 2f - 100f, 0f), new Vector2(200f, 20f), bigText: false);
		subObjects.Add(infoLabel);
		buttons = new Button[Width, Height];
		int num = 0;
		AddButton(new RectButton(menu, this, ActionButton.Action.ClearAll), ref num);
		if (!ModManager.MSC)
		{
			for (int i = 0; i < 2; i++)
			{
				AddButton(null, ref num);
			}
		}
		foreach (MultiplayerUnlocks.SandboxUnlockID itemUnlock in MultiplayerUnlocks.ItemUnlockList)
		{
			if (unlocks.SandboxItemUnlocked(itemUnlock))
			{
				AddButton(new CreatureOrItemButton(menu, this, MultiplayerUnlocks.SymbolDataForSandboxUnlock(itemUnlock)), ref num);
			}
			else
			{
				AddButton(new LockedButton(menu, this), ref num);
			}
		}
		foreach (MultiplayerUnlocks.SandboxUnlockID creatureUnlock in MultiplayerUnlocks.CreatureUnlockList)
		{
			if (unlocks.SandboxItemUnlocked(creatureUnlock))
			{
				AddButton(new CreatureOrItemButton(menu, this, MultiplayerUnlocks.SymbolDataForSandboxUnlock(creatureUnlock)), ref num);
			}
			else
			{
				AddButton(new LockedButton(menu, this), ref num);
			}
		}
		AddButton(new RectButton(menu, this, ActionButton.Action.Play), Width - 1, 0);
		AddButton(new RandomizeButton(menu, this), ModManager.MSC ? (Width - 5) : (Width - 6), 0);
		AddButton(new ConfigButton(menu, this, ActionButton.Action.ConfigA, 0), ModManager.MSC ? (Width - 4) : (Width - 5), 0);
		AddButton(new ConfigButton(menu, this, ActionButton.Action.ConfigB, 1), ModManager.MSC ? (Width - 3) : (Width - 4), 0);
		AddButton(new ConfigButton(menu, this, ActionButton.Action.ConfigC, 2), ModManager.MSC ? (Width - 2) : (Width - 3), 0);
		for (int j = 0; j < Width; j++)
		{
			for (int k = 0; k < Height; k++)
			{
				if (buttons[j, k] != null)
				{
					buttons[j, k].Initiate(new IntVector2(j, k));
				}
			}
		}
		cursors = new List<ButtonCursor>();
	}

	public void ConnectToEditor(SandboxEditor editor)
	{
		this.editor = editor;
		for (int i = 0; i < editor.cursors.Count; i++)
		{
			ButtonCursor item = new ButtonCursor(menu, this, new IntVector2(i, Height - 1), editor.cursors[i]);
			cursors.Add(item);
			subObjects.Add(item);
		}
	}

	private void AddButton(Button button, ref int counter)
	{
		int num = counter / buttons.GetLength(0);
		int x = counter - num * buttons.GetLength(0);
		AddButton(button, x, buttons.GetLength(1) - 1 - num);
		counter++;
	}

	private void AddButton(Button button, int x, int y)
	{
		if (x >= 0 && x < buttons.GetLength(0) && y >= 0 && y < buttons.GetLength(1))
		{
			buttons[x, y] = button;
			if (button != null)
			{
				button.intPos = new IntVector2(x, y);
				subObjects.Add(button);
			}
		}
	}

	public void ActionButtonClicked(ActionButton actionButton)
	{
		if (actionButton.action == ActionButton.Action.ClearAll)
		{
			menu.PlaySound(SoundID.SANDBOX_Clear_All);
			editor.ClearAll();
			editor.UpdatePerformanceEstimate();
		}
		else if (actionButton.action == ActionButton.Action.Play)
		{
			menu.PlaySound(SoundID.SANDBOX_Play);
			editor.Play();
		}
		else if (actionButton.action == ActionButton.Action.ConfigA)
		{
			menu.PlaySound(SoundID.SANDBOX_Switch_Config);
			editor.SwitchConfig(0);
		}
		else if (actionButton.action == ActionButton.Action.ConfigB)
		{
			menu.PlaySound(SoundID.SANDBOX_Switch_Config);
			editor.SwitchConfig(1);
		}
		else if (actionButton.action == ActionButton.Action.ConfigC)
		{
			menu.PlaySound(SoundID.SANDBOX_Switch_Config);
			editor.SwitchConfig(2);
		}
		else if (actionButton.action == ActionButton.Action.Randomize)
		{
			(actionButton as RandomizeButton).Active = !(actionButton as RandomizeButton).Active;
			menu.PlaySound((actionButton as RandomizeButton).Active ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
			UpdateInfoLabel(actionButton.intPos.x, actionButton.intPos.y);
		}
	}

	public void UpdateInfoLabel(int x, int y)
	{
		if (editor.performanceWarning <= 0 && x >= 0 && x < Width && y >= 0 && y < Height)
		{
			infoLabel.text = ((buttons[x, y] != null) ? buttons[x, y].DescriptorText : "");
		}
	}

	public void MouseCursorEnterMenuMode(SandboxEditor.EditCursor cursor)
	{
		if (cursor.ScreenPos.y < size.y + 20f)
		{
			mouseModeClickedDownLow = true;
		}
	}

	public override void Update()
	{
		lastVisFac = visFac;
		counter++;
		base.Update();
		pos.x = overlayOwner.room.game.cameras[0].sSize.x * 0.5f - size.x * 0.5f + Menu.HorizontalMoveToGetCentered(menu.manager);
		if (editor.performanceWarning > 0)
		{
			if (editor.performanceWarning == 1)
			{
				infoLabel.text = menu.Translate("Warning, too many creatures may result in poor game performance.");
			}
			else if (editor.performanceWarning == 2)
			{
				infoLabel.text = menu.Translate("WARNING! Too many creatures may result in bad game performance or crashes.");
			}
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		for (int i = 0; i < editor.cursors.Count; i++)
		{
			if (editor.cursors[i].menuMode)
			{
				flag = true;
			}
			if (editor.cursors[i].mouseMode)
			{
				if (editor.cursors[i].menuMode)
				{
					flag4 = true;
				}
				continue;
			}
			if (editor.cursors[i].ScreenPos.y < size.y + 20f)
			{
				flag2 = true;
			}
			if (editor.cursors[i].ScreenPos.y > 768f - (size.y + 20f))
			{
				flag3 = true;
			}
		}
		if (mouseModeClickedDownLow)
		{
			flag2 = true;
			if (!flag4)
			{
				mouseModeClickedDownLow = false;
			}
		}
		if (editor.gameSession.game.pauseMenu != null)
		{
			flag = false;
		}
		bool flag5 = flag2 && !flag3;
		if (upTop != flag5)
		{
			currentlyVisible = false;
			lingerCounter = 0;
			if (visFac == 0f && lastVisFac == 0f)
			{
				upTop = flag5;
				pos.y = (upTop ? 808f : (-40f - size.y));
				lastPos.y = pos.y;
			}
		}
		else if (flag)
		{
			currentlyVisible = true;
			lingerCounter = 15;
		}
		else
		{
			lingerCounter--;
			if (lingerCounter < 1)
			{
				currentlyVisible = false;
			}
		}
		visFac = Custom.LerpAndTick(visFac, currentlyVisible ? 1f : 0f, 0.03f, 0.05f);
		if (upTop)
		{
			pos.y = Mathf.Lerp(808f, 768f - size.y - 5f, Custom.SCurve(Mathf.Pow(visFac, 0.6f), 0.75f));
			infoLabel.pos.y = -20f;
		}
		else
		{
			pos.y = Mathf.Lerp(-40f - size.y, 5f, Custom.SCurve(Mathf.Pow(visFac, 0.6f), 0.75f));
			infoLabel.pos.y = size.y;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (editor.performanceWarning == 0)
		{
			infoLabel.label.color = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
		}
		else if (editor.performanceWarning == 1)
		{
			infoLabel.label.color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Color.red, 0.5f + 0.5f * Mathf.Sin(((float)counter + timeStacker) / 30f * (float)Math.PI * 2f));
		}
		else if (editor.performanceWarning == 2)
		{
			infoLabel.label.color = ((counter % 10 < 4) ? Menu.MenuRGB(Menu.MenuColors.White) : Color.red);
		}
		for (int i = 0; i < 9; i++)
		{
			bkgRect.sprites[i].color = Color.black;
			bkgRect.sprites[i].alpha = 0.75f;
			bkgRect.sprites[i].isVisible = true;
		}
		Color color = Menu.MenuRGB(Menu.MenuColors.DarkGrey);
		for (int j = 9; j < 17; j++)
		{
			bkgRect.sprites[j].color = color;
			bkgRect.sprites[j].alpha = 1f;
			bkgRect.sprites[j].isVisible = true;
		}
	}
}
