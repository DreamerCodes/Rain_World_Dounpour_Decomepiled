using RWCustom;
using UnityEngine;

namespace Menu;

public class InputTesterHolder : MenuObject
{
	public class InputTester : PositionedMenuObject
	{
		public class TestButton : RectangularMenuObject
		{
			public RoundedRect roundedRect;

			public RoundedRect extraRect;

			public FSprite symbolSprite;

			public bool pressed;

			public float showAsPressed;

			public float lastShowAsPressed;

			public MenuLabel menuLabel;

			public bool selectedPlayer;

			public int buttonIndex;

			public int playerIndex;

			public string labelText;

			public bool playerAssignedToAnything;

			public TestButton(Menu menu, MenuObject owner, Vector2 pos, string symbolName, int symbolRotat, string labelText, int buttonIndex, int playerIndex)
				: base(menu, owner, pos + new Vector2(0.01f, 0.01f) + new Vector2(-12f, -12f), new Vector2(24f, 24f))
			{
				this.labelText = labelText;
				this.buttonIndex = buttonIndex;
				this.playerIndex = playerIndex;
				roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
				subObjects.Add(roundedRect);
				if (symbolName != null)
				{
					symbolSprite = new FSprite(symbolName);
					symbolSprite.rotation = (float)symbolRotat * 90f;
					Container.AddChild(symbolSprite);
				}
				else
				{
					extraRect = new RoundedRect(menu, this, new Vector2(2f, 2f), size - new Vector2(4f, 4f), filled: false);
					subObjects.Add(extraRect);
				}
				if (labelText != null)
				{
					menuLabel = new MenuLabel(menu, this, labelText, new Vector2(10f, 3f), new Vector2(50f, 20f), bigText: false);
					menuLabel.label.alignment = FLabelAlignment.Left;
					subObjects.Add(menuLabel);
				}
			}

			public override void Update()
			{
				base.Update();
				lastShowAsPressed = showAsPressed;
				showAsPressed = Custom.LerpAndTick(showAsPressed, pressed ? 1f : 0f, 0.12f, 0.1f);
				roundedRect.fillAlpha = 1f;
				roundedRect.addSize = new Vector2(4f, 4f) * (pressed ? 1f : 0f);
			}

			public override void GrafUpdate(float timeStacker)
			{
				base.GrafUpdate(timeStacker);
				float num = Mathf.Lerp(lastShowAsPressed, showAsPressed, timeStacker);
				if (menuLabel != null)
				{
					menuLabel.label.color = Color.Lerp(Menu.MenuRGB(playerAssignedToAnything ? Menu.MenuColors.MediumGrey : Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.White), num);
				}
				if (symbolSprite != null)
				{
					symbolSprite.color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.White), num);
					symbolSprite.x = DrawX(timeStacker) + DrawSize(timeStacker).x / 2f;
					symbolSprite.y = DrawY(timeStacker) + DrawSize(timeStacker).y / 2f;
				}
				Color color = Color.Lerp(Menu.MenuRGB(playerAssignedToAnything ? Menu.MenuColors.DarkGrey : Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.MediumGrey), num);
				for (int i = 9; i < 17; i++)
				{
					roundedRect.sprites[i].color = color;
				}
				if (extraRect != null)
				{
					for (int j = 0; j < 8; j++)
					{
						extraRect.sprites[j].alpha = num;
					}
				}
				color = Menu.MenuRGB(Menu.MenuColors.Black);
				for (int k = 0; k < 9; k++)
				{
					roundedRect.sprites[k].color = color;
				}
			}

			public override void RemoveSprites()
			{
				if (symbolSprite != null)
				{
					symbolSprite.RemoveFromContainer();
				}
				base.RemoveSprites();
			}
		}

		public FSprite circleSprite;

		public FSprite centerKnobSprite;

		public FSprite crossSpriteH;

		public FSprite crossSpriteV;

		public Player.InputPackage package;

		public Player.InputPackage lastPackage;

		public int playerIndex;

		public Vector2 knobPos;

		public Vector2 lastKnobPos;

		public Vector2 knobVel;

		private float rad = 50f;

		public TestButton[] testButtons;

		private bool selectedPlayer;

		private bool playerAssignedToAnything;

		private float inPlace;

		private Vector2 GetToPos()
		{
			return (menu as InputOptionsMenu).playerButtons[playerIndex].pos + new Vector2((playerIndex % 2 == 0) ? 200f : 260f, (menu as InputOptionsMenu).playerButtons[playerIndex].size.y / 2f) + new Vector2(1500f * Custom.SCurve(1f - inPlace, 0.6f), 0f);
		}

		public InputTester(Menu menu, MenuObject owner, int playerIndex)
			: base(menu, owner, new Vector2(3000f, 0f))
		{
			this.playerIndex = playerIndex;
			crossSpriteH = new FSprite("pixel");
			crossSpriteH.scaleX = rad * 2f;
			Container.AddChild(crossSpriteH);
			crossSpriteV = new FSprite("pixel");
			crossSpriteV.scaleY = rad * 2f;
			Container.AddChild(crossSpriteV);
			circleSprite = new FSprite("Futile_White");
			circleSprite.shader = menu.manager.rainWorld.Shaders["VectorCircle"];
			Container.AddChild(circleSprite);
			testButtons = new TestButton[9];
			testButtons[0] = new TestButton(menu, this, new Vector2(0f - rad - 6f, 0f), "Menu_Symbol_Arrow", -1, null, 5, playerIndex);
			subObjects.Add(testButtons[0]);
			testButtons[1] = new TestButton(menu, this, new Vector2(0f, rad + 6f), "Menu_Symbol_Arrow", 0, null, 6, playerIndex);
			subObjects.Add(testButtons[1]);
			testButtons[2] = new TestButton(menu, this, new Vector2(rad + 6f, 0f), "Menu_Symbol_Arrow", 1, null, 7, playerIndex);
			subObjects.Add(testButtons[2]);
			testButtons[3] = new TestButton(menu, this, new Vector2(0f, 0f - rad - 6f), "Menu_Symbol_Arrow", 2, null, 8, playerIndex);
			subObjects.Add(testButtons[3]);
			testButtons[4] = new TestButton(menu, this, new Vector2(120f, 30f), null, 0, menu.Translate("Pick up / Eat"), 2, playerIndex);
			subObjects.Add(testButtons[4]);
			testButtons[5] = new TestButton(menu, this, new Vector2(120f, 0f), null, 0, menu.Translate("Jump"), 3, playerIndex);
			subObjects.Add(testButtons[5]);
			testButtons[6] = new TestButton(menu, this, new Vector2(120f, -30f), null, 0, menu.Translate("Throw"), 4, playerIndex);
			subObjects.Add(testButtons[6]);
			float num = ((menu.CurrLang == InGameTranslator.LanguageID.French || menu.CurrLang == InGameTranslator.LanguageID.German) ? 30f : 0f);
			testButtons[7] = new TestButton(menu, this, new Vector2(330f + num, 15f), null, 0, menu.Translate("Pause"), 0, playerIndex);
			subObjects.Add(testButtons[7]);
			testButtons[8] = new TestButton(menu, this, new Vector2(330f + num, -15f), null, 0, menu.Translate("Map"), 1, playerIndex);
			subObjects.Add(testButtons[8]);
			centerKnobSprite = new FSprite("Circle20");
			Container.AddChild(centerKnobSprite);
		}

		public override void Update()
		{
			base.Update();
			selectedPlayer = playerIndex == (menu as InputOptionsMenu).GetCurrentlySelectedOfSeries("PlayerButtons");
			for (int i = 0; i < testButtons.Length; i++)
			{
				testButtons[i].selectedPlayer = selectedPlayer;
			}
			if (menu.manager.rainWorld.options.controls[playerIndex].GetControlPreference() == Options.ControlSetup.ControlToUse.ANY)
			{
				UpdateTestButtons();
			}
			lastPackage = package;
			package = RWInput.PlayerInput(playerIndex);
			playerAssignedToAnything = package.controllerType != Options.ControlSetup.Preset.None;
			inPlace = Custom.LerpAndTick(inPlace, Mathf.InverseLerp(0.5f, 1f, (owner as InputTesterHolder).darkness), 0.08f, 0.025f);
			pos = GetToPos();
			testButtons[0].pressed = package.x < 0;
			testButtons[1].pressed = package.y > 0;
			testButtons[2].pressed = package.x > 0;
			testButtons[3].pressed = package.y < 0;
			testButtons[4].pressed = package.pckp;
			testButtons[5].pressed = package.jmp;
			testButtons[6].pressed = package.thrw;
			testButtons[7].pressed = RWInput.CheckSpecificButton(playerIndex, 5);
			testButtons[8].pressed = package.mp;
			for (int j = 0; j <= 8; j++)
			{
				testButtons[j].playerAssignedToAnything = playerAssignedToAnything;
			}
			lastKnobPos = knobPos;
			knobPos += knobVel;
			knobVel *= 0.5f;
			if (menu.manager.rainWorld.options.controls[playerIndex].gamePad)
			{
				knobVel += (package.analogueDir - knobPos) / 8f;
				knobPos += (package.analogueDir - knobPos) / 4f;
			}
			else
			{
				knobVel -= knobPos / 6f;
				knobVel.x += (float)package.x * 0.3f;
				knobVel.y += (float)package.y * 0.3f;
				knobPos.x += (float)package.x * 0.3f;
				knobPos.y += (float)package.y * 0.3f;
			}
			if (knobPos.magnitude > 1f)
			{
				Vector2 vector = Vector2.ClampMagnitude(knobPos, 1f) - knobPos;
				knobPos += vector;
				knobVel += vector;
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			Vector2 vector = DrawPos(timeStacker);
			Color color = (playerAssignedToAnything ? Menu.MenuRGB(Menu.MenuColors.DarkGrey) : Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey));
			circleSprite.color = color;
			crossSpriteH.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
			crossSpriteV.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
			centerKnobSprite.color = color;
			circleSprite.x = vector.x;
			circleSprite.y = vector.y;
			circleSprite.scale = rad / 8f;
			circleSprite.alpha = 2f / rad;
			crossSpriteH.x = vector.x;
			crossSpriteH.y = vector.y;
			crossSpriteV.x = vector.x;
			crossSpriteV.y = vector.y;
			Vector2 vector2 = Vector2.Lerp(lastKnobPos, knobPos, timeStacker);
			centerKnobSprite.x = vector.x + vector2.x * (rad - 18f) + 0.01f;
			centerKnobSprite.y = vector.y + vector2.y * (rad - 18f) + 0.01f;
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
		}

		public void UpdateTestButtons()
		{
			for (int i = 0; i < testButtons.Length; i++)
			{
				if (testButtons[i].menuLabel != null)
				{
					testButtons[i].menuLabel.text = testButtons[i].labelText + "    ( " + InputOptionsMenu.InputSelectButton.ButtonText(menu, menu.manager.rainWorld.options.controls[playerIndex].gamePad, playerIndex, testButtons[i].buttonIndex, inputTesterDisplay: true) + " )";
				}
			}
		}
	}

	public class Back : MenuObject
	{
		public MenuLabel textLabel;

		public HoldButton holdButton;

		public InputTesterHolder holder;

		public Back(Menu menu, MenuObject owner)
			: base(menu, owner)
		{
			textLabel = new MenuLabel(menu, this, menu.Translate("Hold Pause Button or Escape to exit"), new Vector2(10000f, 0f), new Vector2(100f, 20f), bigText: true);
			textLabel.label.alignment = FLabelAlignment.Left;
			textLabel.label.color = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
			subObjects.Add(textLabel);
			holdButton = new HoldButton(menu, this, menu.Translate("EXIT"), "EXIT TEST INPUT", new Vector2(270f, -1000f), 80f);
			if (menu.manager.rainWorld.screenSize.x == 1280f)
			{
				holdButton.pos.x -= 50f;
			}
			subObjects.Add(holdButton);
			holder = owner as InputTesterHolder;
		}

		public override void Update()
		{
			base.Update();
			holdButton.controlledFromOutside = holder.active;
			holdButton.buttonBehav.greyedOut = !holder.active;
			holdButton.held = false;
			if (holder.active)
			{
				for (int i = 0; i < menu.manager.rainWorld.options.controls.Length; i++)
				{
					if (holdButton.held)
					{
						break;
					}
					holdButton.held = RWInput.CheckPauseButton(i);
				}
			}
			if (menu.manager.menuesMouseMode && holdButton.MouseOver)
			{
				menu.selectedObject = holdButton;
				if (menu.mouseDown && !menu.lastMouseDown)
				{
					Singal(this, "EXIT TEST INPUT");
				}
			}
			holdButton.pos.y = Mathf.Lerp(-200.2f, 80.2f, (owner as InputTesterHolder).darkness);
			textLabel.pos = holdButton.pos + new Vector2(55.2f, -6.8f);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			textLabel.label.alpha = Mathf.InverseLerp(0.5f, 1f, Mathf.Lerp((owner as InputTesterHolder).lastDarkness, (owner as InputTesterHolder).darkness, timeStacker));
		}
	}

	private FSprite darkSprite;

	public bool active;

	public bool lastActive;

	public float darkness;

	public float lastDarkness;

	public InputTester[] testers;

	public Back back;

	public InputTesterHolder(Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		darkSprite = new FSprite("pixel");
		darkSprite.color = new Color(0f, 0f, 0f);
		darkSprite.anchorX = 0f;
		darkSprite.anchorY = 0f;
		darkSprite.scaleX = 1368f;
		darkSprite.scaleY = 770f;
		darkSprite.x = -1f;
		darkSprite.y = -1f;
		Container.AddChild(darkSprite);
		back = new Back(menu, this);
		subObjects.Add(back);
	}

	public void Initiate()
	{
		testers = new InputTester[(menu as InputOptionsMenu).playerButtons.Length];
		for (int i = 0; i < testers.Length; i++)
		{
			testers[i] = new InputTester(menu, this, i);
			subObjects.Add(testers[i]);
		}
	}

	public override void Update()
	{
		base.Update();
		lastDarkness = darkness;
		darkness = Custom.LerpAndTick(darkness, active ? (1f - 0.25f * Mathf.Pow(Mathf.InverseLerp(0.66f, 1f, back.holdButton.filled), 3f)) : 0f, 0.08f, 0.02f);
		if (active && !lastActive)
		{
			for (int i = 0; i < testers.Length; i++)
			{
				testers[i].UpdateTestButtons();
			}
		}
		lastActive = active;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		darkSprite.alpha = Mathf.Lerp(lastDarkness, darkness, timeStacker);
	}

	public override void RemoveSprites()
	{
		darkSprite.RemoveFromContainer();
		base.RemoveSprites();
	}
}
