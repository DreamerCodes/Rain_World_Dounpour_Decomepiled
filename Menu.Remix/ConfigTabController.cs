using System;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu.Remix;

internal class ConfigTabController : UIelement
{
	internal class TabSelectButton : OpSimpleButton
	{
		internal float darken;

		public readonly ConfigTabController ctrl;

		public readonly int buttonIndex;

		private float _height;

		private string _lastName;

		private bool _scrolled;

		private bool _lastFocused;

		private static int _scrollCounter;

		public OpTab RepresentingTab => ConfigContainer.ActiveInterface.Tabs[RepresentingIndex];

		public string RepresentingName => RepresentingTab.name;

		public bool Active => ctrl._ActiveIndex == RepresentingIndex;

		internal int RepresentingIndex => buttonIndex + ctrl._topIndex;

		public Color ColorButton => RepresentingTab.colorButton;

		public Color ColorCanvas => RepresentingTab.colorCanvas;

		private bool IsTop => buttonIndex == 0;

		private bool IsBottom => buttonIndex == 7;

		internal TabSelectButton(int index, ConfigTabController ctrler)
			: base(Vector2.one, Vector2.one)
		{
			buttonIndex = index;
			ctrl = ctrler;
			Reset();
			_rect.hiddenSide = DyeableRect.HiddenSide.Right;
			_rectH.hiddenSide = DyeableRect.HiddenSide.Right;
			_label.alignment = FLabelAlignment.Left;
			_label.rotation = -90f;
			soundClick = SoundID.None;
			myContainer.AddChild(_label);
			ctrl.MenuTab.AddItems(this);
			base.OnClick += Signal;
			SetNextFocusable(NextDirection.Left, this);
			SetNextFocusable(NextDirection.Right, null);
			SetNextFocusable(NextDirection.Down, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton));
			if (index > 0)
			{
				UIfocusable.MutualVerticalFocusableBind(this, ctrler._tabButtons[index - 1]);
			}
			SetNextFocusable(NextDirection.Back, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton));
			Update();
			GrafUpdate(0f);
		}

		protected internal override string DisplayDescription()
		{
			if (string.IsNullOrEmpty(RepresentingName))
			{
				return OptionalText.GetText(OptionalText.ID.ConfigTabController_TabSelectButton_UnnamedTab).Replace("<TabIndex>", RepresentingIndex.ToString());
			}
			return OptionalText.GetText(OptionalText.ID.ConfigTabController_TabSelectButton_NamedTab).Replace("<TabName>", RepresentingName);
		}

		public override void Reset()
		{
			base.Reset();
			_height = Mathf.Min(120f, 600f / (float)Custom.IntClamp(ctrl._tabCount, 1, 8));
			_pos = ctrl.pos + new Vector2(0f, _height * (float)(-buttonIndex - 1) + 603f);
			_size = new Vector2(30f, _height - 6f);
			string text = (string.IsNullOrEmpty(RepresentingName) ? RepresentingIndex.ToString() : RepresentingName);
			if (_lastName != text)
			{
				_lastName = text;
				if (LabelTest.GetWidth(text) > _height - 16f)
				{
					text = LabelTest.TrimText(text, _height - 16f, addDots: true);
				}
				_label.text = text;
			}
			Change();
			_label.alignment = FLabelAlignment.Left;
			GrafUpdate(0f);
		}

		public override void GrafUpdate(float timeStacker)
		{
			if (ConfigContainer.ActiveInterface.Tabs.Length <= RepresentingIndex || RepresentingTab == null)
			{
				Hide();
				return;
			}
			colorEdge = ColorButton;
			colorFill = MenuColorEffect.MidToVeryDark(ColorButton);
			base.GrafUpdate(timeStacker);
			float num = (Active ? 1f : base.bumpBehav.AddSize);
			float num2 = Mathf.Lerp(ctrl._lastScrollBump, ctrl._scrollBump, timeStacker);
			_label.x = (0f - num) * 4f + 15f;
			_label.y = num2 + 6f;
			_rect.addSize = new Vector2(8f, 4f) * num;
			_rect.pos.x = (0f - _rect.addSize.x) * 0.5f;
			_rect.pos.y = num2;
			_rectH.addSize = new Vector2(4f, -4f) * num;
			_rectH.pos.x = (0f - _rectH.addSize.x) * 0.5f;
			_rectH.pos.y = num2;
			_rect.GrafUpdate(timeStacker);
			_rectH.GrafUpdate(timeStacker);
			float num3 = (MouseOver ? ((0.5f + 0.5f * base.bumpBehav.Sin(10f)) * num) : 0f);
			for (int i = 0; i < 8; i++)
			{
				_rectH.sprites[i].alpha = (Active ? 1f : num3);
			}
		}

		public override void Update()
		{
			base.Update();
			if (ConfigContainer.ActiveInterface.Tabs.Length <= RepresentingIndex || RepresentingTab == null)
			{
				Hide();
				return;
			}
			if (!Active && !MouseOver)
			{
				darken = Mathf.Max(0f, darken - 1f / 30f / UIelement.frameMulti);
			}
			else
			{
				darken = Mathf.Min(1f, darken + 0.1f / UIelement.frameMulti);
			}
			if (base.MenuMouseMode)
			{
				if (!MouseOver)
				{
					return;
				}
				if (base.Menu.mouseScrollWheelMovement != 0)
				{
					if (!_scrolled)
					{
						bool flag = base.Menu.mouseScrollWheelMovement < 0;
						if (!ctrl._scrollButtons[(!flag) ? 1u : 0u].IsInactive)
						{
							ctrl._Scroll(flag, first: false);
						}
						_scrolled = true;
					}
				}
				else
				{
					_scrolled = false;
				}
				return;
			}
			if (base.Focused)
			{
				if (base.CtlrInput.y != 0)
				{
					if (base.CtlrInput.y != base.LastCtlrInput.y)
					{
						_scrollCounter = 0;
					}
					else
					{
						_scrollCounter++;
					}
				}
				if (_lastFocused)
				{
					if (IsTop && base.CtlrInput.y > 0)
					{
						if (base.Focused)
						{
							ConfigContainer.instance._allowFocusMove = false;
						}
						if (_scrollCounter == 0)
						{
							ctrl._Scroll(upward: true, first: true);
						}
						else if (_scrollCounter > ModdingMenu.DASinit && _scrollCounter % ModdingMenu.DASdelay == 1)
						{
							ctrl._Scroll(upward: true, first: false);
						}
					}
					if (IsBottom && base.CtlrInput.y < 0)
					{
						if (base.Focused)
						{
							ConfigContainer.instance._allowFocusMove = false;
						}
						if (_scrollCounter == 0)
						{
							ctrl._Scroll(upward: false, first: true);
						}
						else if (_scrollCounter > ModdingMenu.DASinit && _scrollCounter % ModdingMenu.DASdelay == 1)
						{
							ctrl._Scroll(upward: false, first: false);
						}
					}
				}
			}
			_lastFocused = base.Focused;
		}

		private void Signal(UIfocusable self)
		{
			ctrl.Signal(this, RepresentingIndex);
		}
	}

	internal class TabScrollButton : OpSimpleImageButton
	{
		internal readonly bool _up;

		private readonly ConfigTabController _ctrl;

		private readonly GlowGradient _glow;

		protected internal override bool CurrentlyFocusableNonMouse => false;

		public TabScrollButton(bool up, ConfigTabController ctrl)
			: base(Vector2.one, Vector2.one, "Big_Menu_Arrow")
		{
			_size = new Vector2(30f, 20f);
			_up = up;
			_ctrl = ctrl;
			if (up)
			{
				_pos = ctrl.pos + new Vector2(0f, 600f);
			}
			else
			{
				_pos = ctrl.pos + new Vector2(0f, -20f);
			}
			sprite.rotation = (up ? 0f : 180f);
			sprite.scale = 0.5f;
			sprite.x = 15f;
			soundClick = SoundID.None;
			_glow = new GlowGradient(myContainer, new Vector2(-5f, -10f), new Vector2(40f, 40f));
			_glow.sprite.MoveToBack();
			_ctrl.MenuTab.AddItems(this);
			base.OnPressInit += SignalPressInit;
			base.OnPressHold += SignalPressHold;
		}

		protected internal override string DisplayDescription()
		{
			return OptionalText.GetText(_up ? OptionalText.ID.ConfigTabController_TabScrollButton_Up : OptionalText.ID.MenuModList_ListButton_ScrollDw);
		}

		protected internal override void Change()
		{
			base.Change();
			_size = new Vector2(30f, 20f);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			_rect.Hide();
			_rectH.Hide();
			if (Mathf.Abs(_ctrl._scrollBump) > Mathf.Abs(_ctrl._lastScrollBump))
			{
				base.bumpBehav.flash += 0.6f;
			}
			sprite.y = 10f + (float)(_up ? 1 : (-1)) * Mathf.Abs(Mathf.Lerp(_ctrl._lastScrollBump, _ctrl._scrollBump, timeStacker));
			sprite.color = base.bumpBehav.GetColor(MenuColorEffect.rgbMediumGrey);
			_glow.color = sprite.color;
			_glow.alpha = (greyedOut ? 0f : (_rectH.sprites[0].alpha * 0.6f));
		}

		public override void Update()
		{
			greyedOut = (_up ? (_ctrl._topIndex <= 0) : (_ctrl._topIndex >= _ctrl._tabCount - 8));
			base.Update();
		}

		private void SignalPressInit(UIfocusable self)
		{
			_ctrl.Signal(this, (!_up) ? 1 : (-1));
		}

		private void SignalPressHold(UIfocusable self)
		{
			_ctrl.Signal(this, _up ? (-2) : 2);
		}
	}

	private readonly DyeableRect _rectCanvas;

	private readonly DyeableRect _rectButtons;

	internal TabSelectButton[] _tabButtons;

	internal TabScrollButton[] _scrollButtons;

	internal const int TABBUTTONLIMIT = 8;

	private int _tabCount = -1;

	private string _modID;

	internal float _scrollBump;

	internal float _lastScrollBump;

	internal int _topIndex;

	internal ConfigMenuTab MenuTab => tab as ConfigMenuTab;

	internal int _ActiveIndex
	{
		get
		{
			return ConfigContainer.ActiveTabIndex;
		}
		set
		{
			if (ConfigContainer.ActiveTabIndex == value)
			{
				PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
				return;
			}
			PlaySound(SoundID.MENU_MultipleChoice_Clicked);
			ConfigContainer._ChangeActiveTab(value);
			Change();
		}
	}

	public static int TabCount => ConfigContainer.ActiveInterface.Tabs.Length;

	private static string _ModID => ConfigContainer.ActiveInterface.mod.id;

	public ConfigTabController(ConfigMenuTab tab)
		: base(new Vector2(520f, 120f), new Vector2(40f, 600f))
	{
		tab.AddItems(this);
		_tabButtons = new TabSelectButton[8];
		_topIndex = 0;
		_scrollButtons = new TabScrollButton[2];
		_scrollButtons[0] = new TabScrollButton(up: true, this);
		_scrollButtons[1] = new TabScrollButton(up: false, this);
		_rectButtons = new DyeableRect(myContainer, new Vector2(768f, 30f) - _pos, new Vector2(385f, 83f))
		{
			hiddenSide = DyeableRect.HiddenSide.Top
		};
		_rectCanvas = new DyeableRect(myContainer, new Vector2(543f, 105f) - _pos, new Vector2(630f, 630f))
		{
			fillAlpha = 0.5f
		};
		Change();
	}

	public override void Reset()
	{
		base.Reset();
		Change();
		Update();
		GrafUpdate(0f);
	}

	internal TabSelectButton GetCurrentTabButton()
	{
		return _tabButtons[ConfigContainer.ActiveTabIndex - _topIndex];
	}

	internal void _ClearCustomNextFocusable()
	{
		TabSelectButton[] tabButtons = _tabButtons;
		for (int i = 0; i < tabButtons.Length; i++)
		{
			tabButtons[i]?.SetNextFocusable(UIfocusable.NextDirection.Right, null);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (ConfigContainer.activeTab != null)
		{
			_rectCanvas.colorEdge = ConfigContainer.activeTab.colorCanvas;
			_rectCanvas.colorFill = MenuColorEffect.MidToVeryDark(ConfigContainer.activeTab.colorCanvas);
		}
		_rectButtons.colorEdge = ((ConfigContainer.instance._Mode == ConfigContainer.Mode.ModConfig) ? _rectCanvas.colorEdge : MenuColorEffect.rgbDarkGrey);
		_rectCanvas.GrafUpdate(timeStacker);
		_rectButtons.GrafUpdate(timeStacker);
	}

	public override void Update()
	{
		base.Update();
		_rectCanvas.Update();
		_rectButtons.Update();
		if (_ModID != _modID)
		{
			Change();
		}
		_lastScrollBump = _scrollBump;
		_scrollBump = Custom.LerpAndTick(_scrollBump, 0f, 0.1f, 0.1f / UIelement.frameMulti);
	}

	public void ScrollToShow(int targetIndex)
	{
		if (targetIndex < _topIndex)
		{
			_scrollBump = Mathf.Max(_scrollBump - 2.5f * (float)(_topIndex - targetIndex), -12f);
			_topIndex = targetIndex;
		}
		else if (targetIndex >= _topIndex + 8)
		{
			_scrollBump = Mathf.Min(_scrollBump + 2.5f * (float)(_topIndex - targetIndex), 12f);
			_topIndex = targetIndex - 8 + 1;
		}
	}

	protected internal override void Change()
	{
		base.Change();
		if (_ModID != _modID || _tabCount != TabCount)
		{
			_modID = _ModID;
			_tabCount = TabCount;
			_topIndex = 0;
			_scrollBump = 0f;
			_lastScrollBump = 0f;
			if (_tabCount > 8)
			{
				_scrollButtons[0].Show();
				_scrollButtons[1].Show();
			}
			else
			{
				_scrollButtons[0].Hide();
				_scrollButtons[1].Hide();
			}
			ScrollToShow(ConfigContainer.ActiveTabIndex);
			_Refresh();
		}
	}

	private void _Refresh()
	{
		if (ConfigContainer.instance._Mode != ConfigContainer.Mode.ModConfig || _tabCount < 2)
		{
			TabSelectButton[] tabButtons = _tabButtons;
			for (int i = 0; i < tabButtons.Length; i++)
			{
				tabButtons[i]?.Hide();
			}
			_tabCount = -1;
			return;
		}
		for (int j = 0; j < 8; j++)
		{
			if (_tabButtons[j] == null)
			{
				if (j < _tabCount)
				{
					_tabButtons[j] = new TabSelectButton(j, this);
				}
				continue;
			}
			if (j >= _tabCount)
			{
				_tabButtons[j].Hide();
			}
			else if (_tabButtons[j].IsInactive)
			{
				_tabButtons[j].Show();
			}
			if (!_tabButtons[j].IsInactive)
			{
				_tabButtons[j].Reset();
			}
		}
	}

	private void _Scroll(bool upward, bool first)
	{
		if (upward)
		{
			if (_topIndex > 0)
			{
				_topIndex--;
				_Refresh();
				PlaySound((!first) ? SoundID.MENU_Scroll_Tick : SoundID.MENU_First_Scroll_Tick);
				_scrollBump = Mathf.Max(_scrollBump - ((!first) ? 4f : 6f), -12f);
				return;
			}
			if (_topIndex != 0)
			{
				_topIndex = 0;
				_Refresh();
			}
			PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Mouse);
			_scrollBump = Mathf.Max(_scrollBump - 3f, -12f);
		}
		else if (_topIndex < _tabCount - 8)
		{
			_topIndex++;
			_Refresh();
			PlaySound((!first) ? SoundID.MENU_Scroll_Tick : SoundID.MENU_First_Scroll_Tick);
			_scrollBump = Mathf.Min(_scrollBump + ((!first) ? 4f : 6f), 12f);
		}
		else
		{
			if (_topIndex != _tabCount - 8)
			{
				_topIndex = _tabCount - 8;
				_Refresh();
			}
			PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Mouse);
			_scrollBump = Mathf.Min(_scrollBump + 3f, 12f);
		}
	}

	private void Signal(UIfocusable trigger, int index = -1)
	{
		if (trigger is TabSelectButton)
		{
			if (index == ConfigContainer.ActiveTabIndex)
			{
				PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
				return;
			}
			PlaySound(SoundID.MENU_MultipleChoice_Clicked);
			ConfigContainer._ChangeActiveTab(index);
		}
		else if (trigger is TabScrollButton)
		{
			_Scroll(index < 0, Math.Abs(index) > 1);
		}
	}
}
