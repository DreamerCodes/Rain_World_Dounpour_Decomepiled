using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kittehface.Framework20;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace Menu.Remix;

internal class MenuModList : UIelement
{
	internal class ListButton : OpSimpleImageButton, IAmPartOfModList
	{
		public enum Role
		{
			Stat = 0,
			ScrollUp = -1,
			ScrollDown = 1,
			SwapUp = 9,
			SwapDown = 11,
			Expand = 7
		}

		private readonly GlowGradient _glow;

		public readonly Role role;

		private readonly MenuModList _list;

		internal static int _swapIndex = -1;

		protected internal override bool CurrentlyFocusableNonMouse
		{
			get
			{
				if (role == Role.Stat || role == Role.Expand)
				{
					return _list.CfgContainer._Mode != ConfigContainer.Mode.ModConfig;
				}
				return false;
			}
		}

		protected internal override bool MouseOver
		{
			get
			{
				if (role == Role.ScrollUp)
				{
					if (!_list._searchBox.held)
					{
						return base.MouseOver;
					}
					return false;
				}
				return base.MouseOver;
			}
		}

		public ListButton(MenuModList list, Role role)
			: base(Vector2.zero, new Vector2(24f, 24f), RoleSprite(role))
		{
			this.role = role;
			_list = list;
			_list.MenuTab.AddItems(this);
			switch (this.role)
			{
			case Role.Stat:
				_pos = new Vector2(466f, 680f);
				soundClick = SoundID.MENU_Button_Standard_Button_Pressed;
				greyedOut = true;
				base.OnClick += Signal;
				description = OptionalText.GetText(OptionalText.ID.MenuModList_ListButton_Stat);
				break;
			case Role.ScrollUp:
				_pos = new Vector2(321f, 720f);
				soundClick = SoundID.None;
				base.OnPressInit += Signal;
				base.OnPressHold += SignalHold;
				description = OptionalText.GetText(OptionalText.ID.MenuModList_ListButton_ScrollUp);
				break;
			case Role.ScrollDown:
				_pos = new Vector2(321f, 26f);
				sprite.rotation = 180f;
				soundClick = SoundID.None;
				base.OnPressInit += Signal;
				base.OnPressHold += SignalHold;
				description = OptionalText.GetText(OptionalText.ID.MenuModList_ListButton_ScrollDw);
				break;
			case Role.SwapUp:
				_pos = new Vector2(list.pos.x + 200f, -200f);
				soundClick = SoundID.None;
				base.OnClick += Signal;
				description = OptionalText.GetText(OptionalText.ID.MenuModList_ListButton_SwapUp);
				_glow = new GlowGradient(myContainer, -0.5f * base.size, 2f * base.size);
				_glow.sprite.MoveToBack();
				Hide();
				break;
			case Role.SwapDown:
				_pos = new Vector2(list.pos.x + 226f, -200f);
				sprite.rotation = 180f;
				soundClick = SoundID.None;
				base.OnClick += Signal;
				description = OptionalText.GetText(OptionalText.ID.MenuModList_ListButton_SwapDw);
				_glow = new GlowGradient(myContainer, -0.5f * base.size, 2f * base.size);
				_glow.sprite.MoveToBack();
				Hide();
				break;
			case Role.Expand:
				_pos = new Vector2(466f, 80f);
				base.OnClick += Signal;
				description = OptionalText.GetText(OptionalText.ID.MenuModList_ListButton_Expand);
				break;
			}
			Change();
		}

		private static string RoleSprite(Role role)
		{
			switch (role)
			{
			case Role.Stat:
				return "Menu_InfoI";
			case Role.ScrollUp:
			case Role.ScrollDown:
			case Role.SwapUp:
			case Role.SwapDown:
				return "Menu_Symbol_Arrow";
			case Role.Expand:
				return "Menu_Symbol_Show_Thumbs";
			default:
				return "pixel";
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			if (role >= Role.SwapUp)
			{
				_rect.Hide();
				_rectH.Hide();
				_glow.color = sprite.color;
				_glow.alpha = (greyedOut ? 0f : (_rectH.sprites[0].alpha * 0.6f));
			}
		}

		public override void Update()
		{
			base.Update();
			mute = !base.MenuMouseMode;
			if (_list.CfgContainer._Mode == ConfigContainer.Mode.ModConfig)
			{
				greyedOut = true;
			}
			else
			{
				if (ConfigContainer.holdElement && !held)
				{
					return;
				}
				switch (role)
				{
				case Role.Stat:
					greyedOut = ConfigContainer.ActiveTabIndex == 0;
					break;
				case Role.ScrollUp:
					greyedOut = _list._scrollPos <= 0;
					break;
				case Role.ScrollDown:
					greyedOut = _list._scrollPos > _list.visibleModButtons.Count - _scrollVisible;
					break;
				case Role.SwapUp:
					if (_swapIndex < 0)
					{
						greyedOut = true;
						break;
					}
					greyedOut = _list.modButtons[_swapIndex].selectOrder <= 0;
					base.PosY = _list.modButtons[_swapIndex].PosY + (ModButton._boolExpand ? 9f : 1f);
					break;
				case Role.SwapDown:
					if (_swapIndex < 0)
					{
						greyedOut = true;
						break;
					}
					greyedOut = _list.modButtons[_swapIndex].selectOrder >= _list._currentSelections.Length - 1;
					base.PosY = _list.modButtons[_swapIndex].PosY + (ModButton._boolExpand ? 8f : 0f);
					break;
				case Role.Expand:
					greyedOut = ModButton._expand > 0f && ModButton._expand < 1f;
					break;
				}
			}
		}

		private void Signal(UIfocusable self)
		{
			_list.Signal(this, (int)role);
			if (role == Role.Expand)
			{
				ChangeElement(ModButton._boolExpand ? "Menu_Symbol_Show_List" : "Menu_Symbol_Show_Thumbs");
				description = OptionalText.GetText(ModButton._boolExpand ? OptionalText.ID.MenuModList_ListButton_Collapse : OptionalText.ID.MenuModList_ListButton_Expand);
			}
			else if (role != 0)
			{
				PlaySound(SoundID.MENU_First_Scroll_Tick);
			}
		}

		private void SignalHold(UIfocusable self)
		{
			_list.Signal(this, (int)role);
			PlaySound(SoundID.MENU_Scroll_Tick);
		}
	}

	internal class ListSlider : OpSlider, IAmPartOfModList
	{
		private static readonly Configurable<int> _dummy = ModButton.RainWorldDummy.config.Bind("_listSlider", 0);

		private const float _SUBSIZE = 10f;

		private readonly MenuModList _list;

		private readonly FSprite _subtleCircle;

		internal float _floatPos;

		protected internal override bool CurrentlyFocusableMouse => base.CurrentlyFocusableMouse;

		protected internal override bool CurrentlyFocusableNonMouse => false;

		protected internal override bool MouseOver => Custom.DistLess(new Vector2(15f, _subtleCircle.y), base.MousePos, 5f);

		public ListSlider(MenuModList list)
			: base(_dummy, new Vector2(list.pos.x - 30f, list.pos.y), 650, vertical: true)
		{
			_list = list;
			_subtleCircle = new FSprite("Menu_Subtle_Slider_Nob")
			{
				anchorX = 0.5f,
				anchorY = 0.5f
			};
			myContainer.AddChild(_subtleCircle);
			_list.MenuTab.AddItems(this);
			description = OptionalText.GetText(OptionalText.ID.MenuModList_ListSlider_Desc);
		}

		public override void GrafUpdate(float dt)
		{
			base.GrafUpdate(dt);
			_rect.Hide();
			_label.isVisible = false;
			_labelGlow.Hide();
			_lineSprites[0].isVisible = false;
			_lineSprites[3].isVisible = false;
			if (base.Span <= 1)
			{
				_subtleCircle.y = -1000f;
				_lineSprites[1].isVisible = true;
				_lineSprites[1].y = -25f;
				_lineSprites[1].scaleY = base.size.y + 50f;
				_lineSprites[2].isVisible = false;
				return;
			}
			_subtleCircle.x = 15f;
			_subtleCircle.y = Mathf.Clamp(base._mul * ((float)max - ((base.MenuMouseMode && held) ? _floatPos : _list._floatScrollPos)), -15f, base.size.y + 15f);
			_subtleCircle.scale = 1f;
			_subtleCircle.color = _rect.colorEdge;
			_lineSprites[1].isVisible = true;
			_lineSprites[1].y = -25f;
			_lineSprites[2].isVisible = true;
			float num = _subtleCircle.y - 5f;
			_lineSprites[1].scaleY = 25f + num;
			_lineSprites[2].y = base.size.y + 25f;
			_lineSprites[2].scaleY = base.size.y + 25f - (num + 10f);
		}

		public override void Update()
		{
			mousewheelTick = (ModButton._boolExpand ? 1 : 5);
			base.Update();
			if (!ConfigContainer.holdElement || held)
			{
				greyedOut = _list.CfgContainer._Mode == ConfigContainer.Mode.ModConfig;
				if (!held)
				{
					this.SetValueInt(max - _list._scrollPos);
				}
				else if (base.MenuMouseMode)
				{
					_floatPos = (float)max - Mathf.Clamp(base.MousePos.y / base._mul, 0f, max);
					_list._scrollPos = Mathf.RoundToInt(_floatPos);
					_list._ClampScrollPos();
				}
			}
		}
	}

	internal class SearchBox : OpTextBox, IAmPartOfModList
	{
		private static readonly Configurable<string> _dummy = ModButton.RainWorldDummy.config.Bind("_listSearch", "");

		private readonly MenuModList _list;

		private readonly FSprite _searchIcon;

		private readonly string _emptyText = "";

		private float _sin;

		private int _timer;

		private readonly int _delay;

		internal float _fold = 1f;

		private float _lastFold = 1f;

		private const float _FOLDWIDTH = 258f;

		protected internal override bool CurrentlyFocusableMouse => base.CurrentlyFocusableMouse;

		protected internal override bool CurrentlyFocusableNonMouse => false;

		protected internal override bool MouseOver
		{
			get
			{
				if (!held)
				{
					if (base.MouseOver)
					{
						return base.MousePos.x > 258f;
					}
					return false;
				}
				return base.MouseOver;
			}
		}

		public SearchBox(MenuModList list)
			: base(_dummy, new Vector2(list.pos.x, 720f), 282f)
		{
			description = OptionalText.GetText(OptionalText.ID.MenuModList_SearchBox_Desc);
			_emptyText = OptionalText.GetText(OptionalText.ID.MenuModList_SearchBox_Empty);
			allowSpace = true;
			_list = list;
			_list.MenuTab.AddItems(this);
			_searchIcon = new FSprite("modSearch")
			{
				anchorX = 1f,
				anchorY = 0.5f
			};
			myContainer.AddChild(_searchIcon);
			_delay = Mathf.FloorToInt(Custom.LerpMap(Mathf.Clamp(_list.modButtons.Length, 10, 90), 10f, 90f, 10f, 50f));
			_delay = UIelement.FrameMultiply(_delay);
			if (ModManager.MMF)
			{
				if (Custom.rainWorld.options.quality == Options.Quality.HIGH)
				{
					_delay = 10;
				}
				else if (Custom.rainWorld.options.quality == Options.Quality.MEDIUM)
				{
					_delay = Math.Max(10, _delay / 2);
				}
			}
			else
			{
				_delay = 10;
			}
			_timer = -1;
			base.OnValueUpdate += SearchDelay;
		}

		protected internal override string DisplayDescription()
		{
			if (_list._SearchMode && _fold > 0.5f)
			{
				return OptionalText.GetText(OptionalText.ID.MenuModList_SearchBox_Query).Replace("<Query>", _list._searchQuery);
			}
			return base.DisplayDescription();
		}

		public override void GrafUpdate(float timeStacker)
		{
			colorEdge = Color.Lerp(MenuColorEffect.rgbMediumGrey, MenuColorEffect.rgbDarkGrey, ConfigContainer.instance._cursorAlpha);
			rect.pos.x = 258f * Mathf.Lerp(_lastFold, _fold, timeStacker);
			rect.size.x = base.size.x - rect.pos.x;
			base.GrafUpdate(timeStacker);
			label.color = ((value == "") ? MenuColorEffect.rgbDarkGrey : colorText);
			_searchIcon.color = (_list._SearchMode ? base.bumpBehav.GetColor(Color.Lerp(MenuColorEffect.rgbWhite, MenuColorEffect.rgbDarkGrey, 0.5f - 0.5f * Mathf.Sin(_sin / 20f * 3.1416f))) : base.bumpBehav.GetColor(colorEdge));
			_searchIcon.y = base.size.y / 2f;
			_searchIcon.x = base.size.x - 1f;
			label.alpha = Custom.LerpMap(Mathf.Lerp(_lastFold, _fold, timeStacker), 0.5f, 0f, 0f, 1f, 2f);
			_cursor.alpha *= label.alpha;
		}

		protected internal override void Change()
		{
			base.Change();
			if (label != null && value == "")
			{
				label.text = _emptyText;
				_curTextWidth = LabelTest.GetWidth(label.text);
			}
		}

		public override void Update()
		{
			if (_list._SearchMode)
			{
				_sin += 1f / UIelement.frameMulti;
			}
			if (ConfigContainer.holdElement && !held)
			{
				return;
			}
			greyedOut = _list.CfgContainer._Mode == ConfigContainer.Mode.ModConfig;
			if (_mouseDown && MouseOver && _keyboardOn && !Input.GetMouseButton(0))
			{
				_mouseDown = false;
				if (lastValue == value)
				{
					value = "";
					SignalSearch();
				}
				if (_timer > 0)
				{
					SignalSearch();
				}
				_keyboardOn = false;
				held = false;
				PlaySound(SoundID.MENU_Checkbox_Uncheck);
			}
			base.Update();
			_lastFold = _fold;
			if (held)
			{
				_fold = Custom.LerpAndTick(_fold, 0f, 0.2f, 0.1f / UIelement.frameMulti);
				if (ConfigContainer.ActiveInterface is InternalOI_Stats internalOI_Stats)
				{
					foreach (ModButton visibleModButton in ConfigContainer.menuTab.modList.visibleModButtons)
					{
						if (visibleModButton.MouseOver)
						{
							internalOI_Stats._TryPreview(visibleModButton);
							break;
						}
					}
				}
			}
			else
			{
				_fold = Custom.LerpAndTick(_fold, 1f, 0.05f, 0.1f / UIelement.frameMulti);
			}
			if (_timer > 0)
			{
				_timer--;
			}
			if (_timer == 0)
			{
				SignalSearch();
			}
		}

		private void SearchDelay(UIconfig cfg, string value, string oldValue)
		{
			_timer = _delay;
		}

		private void SignalSearch()
		{
			_timer = -1;
			if (!(_list._searchQuery == value))
			{
				_list._searchQuery = value;
				_list.RefreshVisibleButtons();
			}
		}
	}

	internal interface IAmPartOfModList
	{
	}

	internal class ModButton : OpSimpleButton, IAmPartOfModList
	{
		private enum IconContext
		{
			None,
			Error,
			Selected,
			Unselected,
			Requires,
			Required,
			Core,
			HasPriority,
			GivesPriority
		}

		public enum ItfType
		{
			Blank,
			Inconfigurable,
			Configurable
		}

		internal const float _HEIGHT_S = 25f;

		internal const float _HEIGHT_E = 130f;

		internal const float _WIDTH_S = 250f;

		internal const float _ICON_SIZE = 18f;

		internal static float _height = 25f;

		internal static float _thumbRatio = 0.4f;

		internal static float _expand = 0f;

		internal static float _lastExpand = 0f;

		internal static bool _boolExpand = false;

		private readonly MenuModList _list;

		public int viewIndex = int.MinValue;

		public readonly int index;

		private readonly FLabel _labelVer;

		internal float _fade;

		private float _lastFade;

		internal static readonly OptionInterface RainWorldDummy = new InternalOI_Auto(new ModManager.Mod
		{
			id = "RainWorld_BaseGame",
			name = "Rain World",
			authors = "Videocult",
			enabled = false,
			description = OptionalText.GetText(OptionalText.ID.MenuModList_DummyDesc),
			version = ""
		})
		{
			automated = false
		};

		internal const string DummyID = "RainWorld_BaseGame";

		public readonly ItfType type;

		private readonly FSprite _icon;

		private IconContext _curIcon;

		private readonly FTexture _thumbnail;

		private Texture2D _thumb;

		private Texture2D _thumbG;

		internal static Texture2D _thumbD;

		internal static Texture2D _thumbDG;

		internal static Texture2D _thumbDummy;

		internal static List<Texture2D> _ppThumbs;

		internal bool _thumbProcessed;

		internal bool _thumbLoaded;

		internal bool _thumbBlank;

		private readonly GlowGradient _glow;

		private readonly FSprite _separator;

		internal static ModButton _ButtonBelowSeparator;

		internal static ModButton _ButtonAboveSeparator;

		public bool selectEnabled;

		public bool invalid;

		internal bool outdated;

		public int selectOrder;

		public HashSet<int> requirementIndexes = new HashSet<int>();

		public HashSet<int> dependentIndexes = new HashSet<int>();

		public HashSet<int> priorityIndexes = new HashSet<int>();

		public HashSet<int> prioritizedIndexes = new HashSet<int>();

		internal static bool _overCheck = false;

		private bool _swapPressed;

		private static int _scrollCounter = 0;

		private bool _lastFocused;

		private bool otherHeld;

		private Color? _setColor;

		private static readonly Color cItfError = OptionInterface.errorBlue;

		private static readonly Color cDark = MenuColorEffect.rgbDarkGrey;

		private static readonly Color cGrey = MenuColorEffect.rgbMediumGrey;

		internal static readonly Color cRed = new Color(1f, 0.451f, 0.451f);

		private static readonly Color cBlank = Color.Lerp(MenuColorEffect.rgbMediumGrey, MenuColorEffect.rgbDarkGrey, 0.5f);

		private static readonly Color cDepend = new Color(1f, 1f, 0.451f);

		private static readonly Color cRequire = new Color(0.451f, 1f, 0.451f);

		internal static readonly Color cOutdated = new Color(1f, 0.7255f, 0.451f);

		private static readonly Color cHasPriority = new Color(0.27f, 1f, 0.86f);

		private static readonly Color cGivesPriority = new Color(1f, 0.65f, 0.87f);

		private int _alertTime = -10;

		private float MyPos => _list.pos.y + 650f - ((float)(viewIndex + 1) - _list._floatScrollPos) * _height;

		public OptionInterface itf
		{
			get
			{
				if (index != 0)
				{
					return ConfigContainer.OptItfs[index];
				}
				return RainWorldDummy;
			}
		}

		public string ModID => itf.mod.id;

		public bool IsDummy => index == 0;

		protected internal override Rect FocusRect
		{
			get
			{
				Rect result = ((!_boolExpand) ? new Rect(base.ScreenPos.x + (_overCheck ? (-0.5f) : 0.5f) * 25f, base.ScreenPos.y, _overCheck ? 25f : (base.size.x - base.size.y), 25f) : ((!_overCheck) ? new Rect(base.ScreenPos.x - 10f, base.ScreenPos.y + 6f, _rect.size.x, _rect.size.y) : new Rect(base.ScreenPos.x - 8f, base.ScreenPos.y + _rect.size.y - 18f - 2f, 18f, 18f)));
				if (tab != null)
				{
					result.x += tab._container.x;
					result.y += tab._container.y;
				}
				return result;
			}
		}

		protected internal override bool held
		{
			get
			{
				return base.held;
			}
			set
			{
				if (!otherHeld)
				{
					base.held = value;
				}
			}
		}

		protected internal override bool CurrentlyFocusableMouse => _fade < 0.5f;

		protected internal override bool CurrentlyFocusableNonMouse
		{
			get
			{
				if (_fade < 0.5f)
				{
					return _list.CfgContainer._Mode != ConfigContainer.Mode.ModConfig;
				}
				return false;
			}
		}

		protected internal override bool MouseOver
		{
			get
			{
				if (_fade < 0.5f)
				{
					if (base.MousePos.x > -10f && base.MousePos.y > 0f && base.MousePos.x < base.size.x)
					{
						return base.MousePos.y < base.size.y;
					}
					return false;
				}
				return false;
			}
		}

		private void _ProcessThumbnail()
		{
			_thumbProcessed = true;
			if (_thumbLoaded)
			{
				FAtlas atlasWithName = Futile.atlasManager.GetAtlasWithName(ConfigContainer._GetThumbnailName(itf.mod.id));
				if (atlasWithName != null)
				{
					_thumb = (atlasWithName.texture as Texture2D).Clone();
					_thumbG = _thumb.Clone();
					MenuColorEffect.TextureGreyscale(ref _thumbG);
					_UpdateThumbnail();
				}
			}
			else
			{
				_thumb = _thumbD.Clone();
				_thumbG = _thumbDG.Clone();
			}
		}

		internal void _PingThumbnailLoaded(bool blank = false)
		{
			_thumbBlank = blank;
			_thumbLoaded = true;
			_thumbProcessed = false;
		}

		internal void _UpdateThumbnail()
		{
			if (!_thumbBlank)
			{
				_thumbnail.SetTexture(selectEnabled ? _thumb : _thumbG);
			}
		}

		public ModButton(MenuModList list, int index)
			: base(Vector2.zero, new Vector2(250f, 25f))
		{
			_list = list;
			this.index = index + 1;
			_list.MenuTab.AddItems(this);
			_pos = new Vector2(list.pos.x, MyPos);
			selectEnabled = IsDummy || (itf.mod.enabled && !itf.mod.DLCMissing);
			selectOrder = itf.mod.loadOrder;
			FocusMenuPointer pointer = FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._ModButtonPointer);
			SetNextFocusable(this, pointer, pointer, pointer, pointer);
			_thumb = _thumbD.Clone();
			_thumbG = _thumbDG.Clone();
			_thumbnail = new FTexture(selectEnabled ? _thumb : _thumbG, itf.mod.id)
			{
				anchorX = 0f,
				anchorY = 0f,
				x = 125f - (float)_thumb.width * _thumbRatio / 2f - 6f,
				y = 9f,
				scaleX = _thumbRatio,
				scaleY = _thumbRatio
			};
			myContainer.AddChild(_thumbnail);
			_thumbnail.MoveBehindOtherNode(_rectH.sprites[0].container);
			_glow = new GlowGradient(myContainer, Vector2.zero, base.size);
			_label.MoveToFront();
			_icon = new FSprite("pixel")
			{
				anchorX = 0.5f,
				anchorY = 0.5f,
				x = 125f,
				y = 12.5f,
				color = colorEdge,
				isVisible = false
			};
			myContainer.AddChild(_icon);
			_separator = new FSprite("listDivider")
			{
				color = MenuColorEffect.rgbDarkGrey,
				anchorX = 0.5f,
				anchorY = 0.5f,
				scaleX = 1.75f,
				x = 125f,
				y = 0f,
				alpha = 0f
			};
			myContainer.AddChild(_separator);
			if (itf is InternalOI)
			{
				if ((itf as InternalOI).reason == InternalOI.Reason.TestOI)
				{
					type = ItfType.Inconfigurable;
				}
				else if (!IsDummy && itf is InternalOI_Auto internalOI_Auto)
				{
					if (internalOI_Auto.IsAutomated)
					{
						type = ItfType.Configurable;
					}
					else
					{
						type = ItfType.Blank;
					}
				}
			}
			else
			{
				type = ((!itf.HasConfigurables()) ? ItfType.Inconfigurable : ItfType.Configurable);
			}
			string localizedName = itf.mod.LocalizedName;
			if (itf.mod.authors != "Unknown" && !_AuthorsLong(itf.mod.authors))
			{
				description = OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_NameAndAuthor).Replace("<ModName>", localizedName).Replace("<ModAuthor>", OptionInterface.Translate(itf.mod.authors));
			}
			else
			{
				description = localizedName;
			}
			soundClick = SoundID.None;
			if (!string.IsNullOrEmpty(itf.mod.version) && !itf.mod.hideVersion)
			{
				_labelVer = UIelement.FLabelCreate(itf.mod.version);
				myContainer.AddChild(_labelVer);
				_labelVer.y = _label.y;
				_labelVer.alignment = FLabelAlignment.Right;
			}
			base.text = LabelTest.TrimText(localizedName, base.size.x - Mathf.Max(50f, (_labelVer != null) ? (_labelVer.textRect.width + 30f) : 0f), addDots: true);
			_list.MenuTab.AddItems(this);
			Change();
			base.OnClick += Signal;
			base.OnUnload += UnloadUI;
		}

		internal void UnloadUI()
		{
			if (_thumb != null)
			{
				UnityEngine.Object.Destroy(_thumb);
				_thumb = null;
			}
			if (_thumbG != null)
			{
				UnityEngine.Object.Destroy(_thumbG);
				_thumbG = null;
			}
		}

		protected internal override string DisplayDescription()
		{
			if (_overCheck)
			{
				if (_list.CfgContainer._Mode == ConfigContainer.Mode.ModConfig)
				{
					return description;
				}
				if (base.MenuMouseMode)
				{
					return OptionalText.GetText(selectEnabled ? OptionalText.ID.MenuModList_ModButton_Disable_Mouse : OptionalText.ID.MenuModList_ModButton_Enable_Mouse).Replace("<Mod>", description);
				}
				return OptionalText.GetText(selectEnabled ? OptionalText.ID.MenuModList_ModButton_Disable_NonMouse : OptionalText.ID.MenuModList_ModButton_Enable_NonMouse);
			}
			if (base.MenuMouseMode)
			{
				if (_list._roleButtons[3].MouseOver)
				{
					return _list._roleButtons[3].DisplayDescription();
				}
				if (_list._roleButtons[4].MouseOver)
				{
					return _list._roleButtons[4].DisplayDescription();
				}
				if (_list.CfgContainer._Mode != ConfigContainer.Mode.ModView)
				{
					return description;
				}
			}
			else if (!selectEnabled)
			{
				return description;
			}
			if (itf.error)
			{
				if (!base.MenuMouseMode)
				{
					return OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_NonMouse_Display).Replace("<Mod>", description);
				}
				return OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_Display).Replace("<Mod>", description);
			}
			switch (type)
			{
			case ItfType.Configurable:
				if (base.MenuMouseMode)
				{
					return OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_Configure).Replace("<Mod>", description);
				}
				return OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_NonMouse_Configure).Replace("<Mod>", description);
			case ItfType.Inconfigurable:
				if (base.MenuMouseMode)
				{
					return OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_Display).Replace("<Mod>", description);
				}
				return OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_NonMouse_Display).Replace("<Mod>", description);
			default:
				return description;
			}
		}

		private bool _AuthorsLong(string authors)
		{
			if (authors.Contains("<LINE>") || authors.Contains("\n"))
			{
				return true;
			}
			return authors.Length > 50;
		}

		private void _UpdateIcon(IconContext context)
		{
			if (invalid)
			{
				context = IconContext.Unselected;
			}
			if (_curIcon != context)
			{
				_curIcon = context;
				string elementName = "";
				_icon.rotation = 0f;
				switch (context)
				{
				case IconContext.None:
					_icon.isVisible = false;
					return;
				case IconContext.Core:
					elementName = "Kill_Slugcat";
					break;
				case IconContext.Error:
					elementName = "Sandbox_SmallQuestionmark";
					break;
				case IconContext.Selected:
					elementName = "Menu_Symbol_CheckBox";
					break;
				case IconContext.Unselected:
					elementName = "Menu_Symbol_Clear_All";
					break;
				case IconContext.Requires:
				case IconContext.GivesPriority:
					elementName = "keyShift" + ((!selectEnabled) ? "A" : "B");
					_icon.rotation = 90f;
					break;
				case IconContext.Required:
				case IconContext.HasPriority:
					elementName = "keyShift" + ((!selectEnabled) ? "A" : "B");
					_icon.rotation = 270f;
					break;
				}
				_icon.isVisible = true;
				_icon.element = Futile.atlasManager.GetElementWithName(elementName);
			}
		}

		protected internal override void NonMouseSetHeld(bool newHeld)
		{
			base.NonMouseSetHeld(newHeld);
			_swapPressed = false;
		}

		protected internal override void Change()
		{
			base.Change();
			_label.alignment = FLabelAlignment.Left;
			if (_labelVer != null)
			{
				_labelVer.x = base.size.x;
			}
			if (_expand == 0f)
			{
				_rect.Hide();
				return;
			}
			_rectH.size = _rect.size - Vector2.one * 8f;
			_rectH.pos = _rect.pos + Vector2.one * 4f;
		}

		public override void GrafUpdate(float timeStacker)
		{
			if (_fade >= 1f)
			{
				_label.alpha = 0f;
				if (_labelVer != null)
				{
					_labelVer.alpha = 0f;
				}
				if (_icon != null)
				{
					_icon.alpha = 0f;
				}
				_separator.alpha = 0f;
				_thumbnail.isVisible = false;
				_rect.Hide();
				_rectH.Hide();
				return;
			}
			base.GrafUpdate(timeStacker);
			float num = Mathf.Lerp(_lastExpand, _expand, timeStacker);
			_label.alpha = Mathf.Pow(1f - Mathf.Lerp(_lastFade, _fade, timeStacker), 2f);
			_label.x = Mathf.Lerp(15f, 19f, num);
			_label.y = base.size.y - 12.5f - Mathf.Lerp(0f, 4f, num);
			_glow.alpha = ((greyedOut && num == 0f) ? 0f : (_rectH.sprites[0].alpha * _label.alpha));
			if (num > 0f)
			{
				_rect.Show();
				_rect.size = base.size + new Vector2(16f, -8f);
				_rect.pos = new Vector2(-10f, 6f);
				_rect.addSize = Vector2.zero;
				_rectH.size = _rect.size - Vector2.one * 8f;
				_rectH.pos = _rect.pos + Vector2.one * 4f;
				for (int i = 0; i < 9; i++)
				{
					_rect.sprites[i].alpha = num * base.bumpBehav.FillAlpha * _label.alpha;
				}
				for (int j = 9; j < _rect.sprites.Length; j++)
				{
					_rect.sprites[j].alpha = num * _label.alpha;
				}
				_thumbnail.isVisible = !_thumbBlank;
				_thumbnail.alpha = num * (selectEnabled ? 0.8f : 0.4f) * _label.alpha;
				_thumbnail.scaleY = num * _thumbRatio;
				_glow.color = MenuColorEffect.rgbBlack;
				_glow.alpha = Mathf.Lerp(0.6f, 1f, _glow.alpha);
				if (_labelVer != null)
				{
					_labelVer.y = num * 8f + 12.5f;
				}
				if (greyedOut)
				{
					_thumbnail.alpha *= 0.5f;
				}
			}
			else
			{
				_rect.Hide();
				_rectH.Hide();
				_thumbnail.isVisible = false;
				_glow.color = _label.color;
				_glow.alpha *= 0.6f;
				if (_labelVer != null)
				{
					_labelVer.y = 12.5f;
				}
			}
			if (_labelVer != null)
			{
				_labelVer.alpha = _label.alpha;
				_labelVer.color = _label.color;
			}
			if (_icon != null)
			{
				if (_overCheck && MouseOver)
				{
					_icon.color = Color.Lerp(_label.color, MenuColorEffect.MidToVeryDark(_label.color), base.bumpBehav.Sin(held ? 30f : 10f));
				}
				else
				{
					_icon.color = _label.color;
				}
				_icon.alpha = _label.alpha;
				_icon.x = Mathf.Lerp(0f, 4f, num);
				_icon.y = _label.y;
			}
			_glow.centerPos = new Vector2(125f, _label.y);
			if (_ButtonAboveSeparator == this && _ButtonBelowSeparator != null)
			{
				_separator.alpha = Mathf.Min(_label.alpha, Mathf.Pow(1f - _ButtonBelowSeparator._fade, 2f));
				_separator.color = _list._sideLines[0].color;
				_separator.y = num * 2f;
			}
			else
			{
				_separator.alpha = 0f;
			}
		}

		public override void Update()
		{
			_lastFade = _fade;
			_fade = Mathf.Clamp01(Mathf.Max(_list._floatScrollPos - (float)viewIndex, (float)viewIndex - _list._floatScrollPos - (float)_scrollVisible + 1f));
			if (ConfigContainer.FocusedElement == this && _fade >= 1f)
			{
				_list.ScrollToShow(index);
			}
			if (_fade >= 1f)
			{
				_lastFocused = false;
				return;
			}
			_pos = new Vector2(_list.pos.x, MyPos);
			_size = new Vector2(base.size.x, _height);
			otherHeld = false;
			if (ConfigContainer.holdElement && !held)
			{
				if (!(ConfigContainer.FocusedElement is IAmPartOfModList))
				{
					return;
				}
				otherHeld = true;
			}
			base.Update();
			if (_boolExpand && !_thumbProcessed)
			{
				_ProcessThumbnail();
			}
			greyedOut = _list.CfgContainer._Mode == ConfigContainer.Mode.ModConfig;
			bool flag = false;
			if (IsDummy)
			{
				_UpdateIcon(IconContext.Core);
				flag = true;
			}
			else if ((greyedOut && ConfigContainer.ActiveItfIndex != index) || (!greyedOut && ConfigContainer.FocusedElement is ModButton && ConfigContainer.FocusedElement != this))
			{
				ModButton modButton = (greyedOut ? _list.modButtons[ConfigContainer.ActiveItfIndex] : (ConfigContainer.FocusedElement as ModButton));
				foreach (int dependentIndex in modButton.dependentIndexes)
				{
					if (dependentIndex == index)
					{
						_UpdateIcon(IconContext.Requires);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					foreach (int requirementIndex in modButton.requirementIndexes)
					{
						if (requirementIndex == index)
						{
							_UpdateIcon(IconContext.Required);
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					foreach (int priorityIndex in modButton.priorityIndexes)
					{
						if (priorityIndex == index)
						{
							_UpdateIcon(IconContext.HasPriority);
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					foreach (int prioritizedIndex in modButton.prioritizedIndexes)
					{
						if (prioritizedIndex == index)
						{
							_UpdateIcon(IconContext.GivesPriority);
							flag = true;
							break;
						}
					}
				}
			}
			if (!flag)
			{
				if (itf.error)
				{
					_UpdateIcon(IconContext.Error);
				}
				else if (selectEnabled || index == 0)
				{
					_UpdateIcon(IconContext.Selected);
				}
				else
				{
					_UpdateIcon(IconContext.Unselected);
				}
			}
			UpdateColor();
			if (otherHeld)
			{
				return;
			}
			if (base.MenuMouseMode)
			{
				if (base.Focused)
				{
					if (!held)
					{
						_overCheck = base.MousePos.x < 15f && (!_boolExpand || base.MousePos.y > _label.y - 9f);
					}
					ListButton._swapIndex = ((!_list._SearchMode && selectEnabled) ? index : (-1));
					if (base.Menu.mouseScrollWheelMovement != 0 && !_list._roleButtons[1].IsInactive)
					{
						_list.Signal(_list._roleButtons[(base.Menu.mouseScrollWheelMovement < 0) ? 1 : 2], Math.Sign(base.Menu.mouseScrollWheelMovement) * (_boolExpand ? 1 : 5));
						_list.PlaySound(SoundID.MENU_Scroll_Tick);
					}
				}
				if (_labelVer != null)
				{
					_labelVer.isVisible = !_list._listFocused || ListButton._swapIndex != index;
				}
				return;
			}
			if (base.Focused)
			{
				if (!held)
				{
					if (_list.CfgContainer._Mode == ConfigContainer.Mode.ModSelect)
					{
						_overCheck = true;
					}
					else if (base.bumpBehav.JoystickPress(new IntVector2(-1, 0)) && !_overCheck)
					{
						_overCheck = true;
						FocusMoveDisallow();
					}
					else if (base.bumpBehav.JoystickPress(new IntVector2(1, 0)) && _overCheck)
					{
						_overCheck = false;
						FocusMoveDisallow();
					}
				}
				ListButton._swapIndex = ((!_list._SearchMode && selectEnabled) ? index : (-1));
				if (_labelVer != null)
				{
					_labelVer.isVisible = ListButton._swapIndex != index;
				}
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
				if (_overCheck && held && ListButton._swapIndex == index)
				{
					_list.CfgContainer._allowFocusMove = false;
					if (!_list._roleButtons[3].greyedOut && base.CtlrInput.y > 0)
					{
						_swapPressed = true;
						if (_scrollCounter == 0)
						{
							_list.Signal(_list._roleButtons[3], 9);
							PlaySound(SoundID.MENU_First_Scroll_Tick);
							held = true;
						}
						else if (_scrollCounter > ModdingMenu.DASinit && _scrollCounter % (ModdingMenu.DASdelay / 2) == 1)
						{
							_list.Signal(_list._roleButtons[3], 9);
							PlaySound(SoundID.MENU_Scroll_Tick);
							held = true;
						}
					}
					if (!_list._roleButtons[4].greyedOut && base.CtlrInput.y < 0)
					{
						_swapPressed = true;
						if (_scrollCounter == 0)
						{
							_list.Signal(_list._roleButtons[4], 11);
							PlaySound(SoundID.MENU_First_Scroll_Tick);
							held = true;
						}
						else if (_scrollCounter > ModdingMenu.DASinit && _scrollCounter % (ModdingMenu.DASdelay / 2) == 1)
						{
							_list.Signal(_list._roleButtons[4], 11);
							PlaySound(SoundID.MENU_Scroll_Tick);
							held = true;
						}
					}
					return;
				}
				if (_list._roleButtons[1].IsInactive)
				{
					return;
				}
				if (viewIndex == _list._scrollPos && !_list._roleButtons[1].greyedOut)
				{
					if (base.CtlrInput.y > 0)
					{
						_list.CfgContainer._allowFocusMove = false;
						if (_scrollCounter == 0)
						{
							SignalScroll(up: true, first: true);
						}
						else if (_scrollCounter > ModdingMenu.DASinit && _scrollCounter % (ModdingMenu.DASdelay / 2) == 1)
						{
							SignalScroll(up: true, first: false);
						}
					}
				}
				else if (viewIndex == _list._scrollPos + _scrollVisible - 1 && !_list._roleButtons[2].greyedOut && base.CtlrInput.y < 0)
				{
					_list.CfgContainer._allowFocusMove = false;
					if (_scrollCounter == 0)
					{
						SignalScroll(up: false, first: true);
					}
					else if (_scrollCounter > ModdingMenu.DASinit && _scrollCounter % (ModdingMenu.DASdelay / 2) == 1)
					{
						SignalScroll(up: false, first: false);
					}
				}
			}
			else if (_labelVer != null)
			{
				_labelVer.isVisible = true;
			}
			_lastFocused = base.Focused;
		}

		private void SignalScroll(bool up, bool first)
		{
			if (_lastFocused)
			{
				_list.Signal(_list._roleButtons[up ? 1 : 2], (!up) ? 1 : (-1));
				_list.PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
				ModButton element = _list.visibleModButtons[Custom.IntClamp(viewIndex + ((!up) ? 1 : (-1)), 0, _list.visibleModButtons.Count - 1)];
				_list.CfgContainer._FocusNewElement(element, silent: true);
			}
		}

		public void SetColor(Color? color)
		{
			_setColor = color;
		}

		public void UpdateColor()
		{
			if (itf.mod.DLCMissing)
			{
				colorEdge = MenuColorEffect.rgbDarkRed;
				return;
			}
			if (invalid)
			{
				colorEdge = cRed;
				return;
			}
			if (_setColor.HasValue)
			{
				colorEdge = _setColor.Value;
				return;
			}
			if (_curIcon == IconContext.Requires)
			{
				colorEdge = (selectEnabled ? cDepend : MenuColorEffect.MidToDark(cDepend));
				return;
			}
			if (_curIcon == IconContext.Required)
			{
				colorEdge = (selectEnabled ? cRequire : MenuColorEffect.MidToDark(cRequire));
				return;
			}
			if (_curIcon == IconContext.HasPriority)
			{
				colorEdge = (selectEnabled ? cHasPriority : MenuColorEffect.MidToDark(cHasPriority));
				return;
			}
			if (_curIcon == IconContext.GivesPriority)
			{
				colorEdge = (selectEnabled ? cGivesPriority : MenuColorEffect.MidToDark(cGivesPriority));
				return;
			}
			if (_curIcon == IconContext.Error)
			{
				colorEdge = cItfError;
			}
			if (outdated)
			{
				colorEdge = (selectEnabled ? cOutdated : MenuColorEffect.MidToDark(cOutdated));
				return;
			}
			if (!selectEnabled)
			{
				colorEdge = cDark;
			}
			else if (type == ItfType.Blank)
			{
				colorEdge = cBlank;
			}
			else
			{
				colorEdge = cGrey;
			}
			if (greyedOut)
			{
				colorEdge = MenuColorEffect.Greyscale(MenuColorEffect.MidToDark(colorEdge));
			}
		}

		private void SignalToggle()
		{
			_list.CfgContainer._allowFocusMove = false;
			if (itf.mod.DLCMissing)
			{
				PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
				_NotifyDisabled(base.Menu.Translate("remix_dlc_locked"));
			}
			else if (invalid || IsDummy || itf is InternalOI_Test)
			{
				PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
			}
			else
			{
				_list._ToggleMod(this);
				PlaySound(selectEnabled ? SoundID.MENU_Checkbox_Uncheck : SoundID.MENU_Checkbox_Check);
			}
		}

		public void Signal(UIfocusable self)
		{
			if (_overCheck)
			{
				if (!_swapPressed)
				{
					SignalToggle();
				}
			}
			else
			{
				if (base.MenuMouseMode && ListButton._swapIndex == index && base.MousePos.x > 200f)
				{
					return;
				}
				if (itf.mod.DLCMissing && SteamManager.Initialized)
				{
					Custom.rainWorld.processManager.mySteamManager.ShowDownpourStorePage();
					_NotifyDisabled(base.Menu.Translate("remix_dlc_restart"), forcePopup: true);
				}
				else if (itf.mod.DLCMissing && GogGalaxyManager.IsFullyInitialized())
				{
					Application.OpenURL("https://www.gog.com/en/game/rain_world_downpour");
					_NotifyDisabled(base.Menu.Translate("remix_dlc_restart"), forcePopup: true);
				}
				else if (itf.mod.DLCMissing)
				{
					PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
					AOC.GetStoreContent();
					if (AOC.HasStoreContentImpl())
					{
						_NotifyDisabled(base.Menu.Translate("remix_dlc_restart"), forcePopup: true);
					}
					else
					{
						_NotifyDisabled(base.Menu.Translate("remix_dlc_locked"));
					}
				}
				else if (invalid || IsDummy)
				{
					PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
				}
				else if (_list.CfgContainer._Mode == ConfigContainer.Mode.ModSelect)
				{
					_list.MenuTab.FlashApplyButton();
					_NotifyDisabled(base.Menu.Translate("You must apply new mods setup before attempting to configure them."));
					PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
				}
				else if (_list.CfgContainer._Mode == ConfigContainer.Mode.ModConfig)
				{
					if (!_list.MenuTab.SaveButton.greyedOut)
					{
						_list.MenuTab.SaveButton.bumpBehav.flash = 2f;
					}
					else
					{
						_list.MenuTab.RevertButton.bumpBehav.flash = 2f;
					}
					PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
				}
				else if (!(itf is InternalOI_Test) && !selectEnabled)
				{
					_NotifyDisabled(base.Menu.Translate("You must activate this mod before attempting to configure it."));
					PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
				}
				else if (type == ItfType.Blank && (!itf.HasConfigurables() || itf is InternalOI_Auto { automated: false }))
				{
					_NotifyDisabled(base.Menu.Translate("This mod does not have any configurable settings."));
					PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
				}
				else
				{
					PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
					_list.Signal(this, index);
				}
			}
		}

		private void _NotifyDisabled(string message, bool forcePopup = false)
		{
			if (Mathf.FloorToInt(Time.timeSinceLevelLoad) - _alertTime > 9 && !forcePopup)
			{
				string text = message.Replace("<LINE>", " ");
				ModdingMenu.instance.ShowAlert(text);
				_alertTime = Mathf.FloorToInt(Time.timeSinceLevelLoad);
			}
			else
			{
				ConfigConnector.CreateDialogBoxNotify(message.Replace("<LINE>", " <LINE>"));
			}
		}

		public bool MatchQuery(string query)
		{
			if (!ListItem.SearchMatch(query, itf.mod.id))
			{
				return ListItem.SearchMatch(query, itf.mod.LocalizedName);
			}
			return true;
		}
	}

	private float _floatScrollPos;

	private float _floatScrollVel;

	internal readonly ModButton[] modButtons;

	internal readonly List<ModButton> visibleModButtons;

	internal readonly List<ModButton> sortedModButtons;

	private readonly FSprite _backSide;

	private readonly FSprite[] _sideLines;

	internal readonly ListButton[] _roleButtons;

	private readonly ListSlider _slider;

	private readonly SearchBox _searchBox;

	private int _scrollPos;

	private static int _scrollVisible = 26;

	private readonly bool _initialized;

	private string _searchQuery = "";

	internal bool selectionChanged;

	private string[] _savedSelections;

	private string[] _currentSelections;

	private bool _listFocused;

	private ConfigMenuTab MenuTab => tab as ConfigMenuTab;

	private ConfigContainer CfgContainer => ConfigContainer.instance;

	private int _ScrollMax
	{
		get
		{
			if (visibleModButtons.Count >= _scrollVisible)
			{
				return visibleModButtons.Count - _scrollVisible + 1;
			}
			return 0;
		}
	}

	internal bool _SearchMode => _searchQuery.Length > 1;

	protected internal override bool MouseOver
	{
		get
		{
			if (!base.MouseOver)
			{
				if (base.MousePos.x > -15f && base.MousePos.x < 295f && base.MousePos.y > 140f)
				{
					return base.MousePos.y < 700f;
				}
				return false;
			}
			return true;
		}
	}

	public MenuModList(ConfigMenuTab tab)
		: base(new Vector2(208f, 60f), new Vector2(250f, 650f))
	{
		_initialized = false;
		tab.AddItems(this);
		ModButton.RainWorldDummy.mod.description = OptionalText.GetText(OptionalText.ID.MenuModList_DummyDesc);
		_backSide = new FSprite("pixel")
		{
			anchorX = 0f,
			anchorY = 0f,
			scaleX = 250f,
			color = MenuColorEffect.rgbBlack,
			alpha = 0.4f
		};
		myContainer.AddChild(_backSide);
		_sideLines = new FSprite[4];
		_sideLines[0] = new FSprite("pixel")
		{
			anchorX = 0f,
			anchorY = 0f,
			scaleX = 2f,
			x = 265f
		};
		_sideLines[1] = new FSprite("pixel")
		{
			anchorX = 0f,
			anchorY = 0f,
			scaleX = 2f,
			x = 265f
		};
		_sideLines[2] = new FSprite("pixel")
		{
			anchorX = 0f,
			anchorY = 0f,
			scaleX = 2f,
			x = 265f
		};
		_sideLines[3] = new FSprite("pixel")
		{
			anchorX = 0f,
			anchorY = 0f,
			scaleX = 2f,
			x = 265f
		};
		for (int i = 0; i < _sideLines.Length; i++)
		{
			myContainer.AddChild(_sideLines[i]);
		}
		modButtons = new ModButton[ConfigContainer.OptItfs.Length];
		for (int j = 0; j < modButtons.Length; j++)
		{
			modButtons[j] = new ModButton(this, j - 1);
		}
		visibleModButtons = new List<ModButton>();
		sortedModButtons = new List<ModButton>();
		RefreshSavedSelections();
		RefreshAllButtons();
		_slider = new ListSlider(this)
		{
			max = _ScrollMax,
			value = _ScrollMax.ToString(CultureInfo.InvariantCulture)
		};
		FSprite[] sideLines = _sideLines;
		for (int k = 0; k < sideLines.Length; k++)
		{
			sideLines[k].color = MenuColorEffect.rgbMediumGrey;
		}
		_roleButtons = new ListButton[6];
		_roleButtons[0] = new ListButton(this, ListButton.Role.Stat);
		_roleButtons[0].SetNextFocusable(FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._LastActiveModButtonOrTop), _roleButtons[0], null, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._ModStatsRightPointer), FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._BackButton));
		_roleButtons[1] = new ListButton(this, ListButton.Role.ScrollUp);
		_roleButtons[2] = new ListButton(this, ListButton.Role.ScrollDown);
		_roleButtons[3] = new ListButton(this, ListButton.Role.SwapUp);
		_roleButtons[4] = new ListButton(this, ListButton.Role.SwapDown);
		_roleButtons[5] = new ListButton(this, ListButton.Role.Expand);
		_roleButtons[5].SetNextFocusable(FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._LastActiveModButtonOrBottom), _roleButtons[0], FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._ApplyButton), FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI._ApplyButton));
		UIfocusable.MutualVerticalFocusableBind(_roleButtons[5], _roleButtons[0]);
		_searchBox = new SearchBox(this);
		if (visibleModButtons.Count < _scrollVisible)
		{
			_roleButtons[1].Hide();
			_roleButtons[2].Hide();
		}
		CfgContainer.QueueModThumbnails(sortedModButtons.ToArray());
		modButtons[0]._PingThumbnailLoaded();
		_initialized = true;
	}

	internal void RefreshAllButtons()
	{
		sortedModButtons.Clear();
		ConfigMenuTab._countModEnabled = 0;
		ConfigMenuTab._countModTotal = 0;
		List<ModButton> btns = new List<ModButton>();
		List<ModButton> list = new List<ModButton>();
		for (int i = 1; i < modButtons.Length; i++)
		{
			if (modButtons[i].selectEnabled)
			{
				btns.Add(modButtons[i]);
				ConfigMenuTab._countModEnabled++;
			}
			else
			{
				list.Add(modButtons[i]);
			}
			if (!modButtons[i].itf.mod.DLCMissing)
			{
				ConfigMenuTab._countModTotal++;
			}
		}
		string text = string.Join("|", btns.Select((ModButton x) => x.ModID));
		for (int j = 0; j <= btns.Count; j++)
		{
			_CheckRequirementsOrder(ref btns);
			btns.Sort(CompareBySelectOrder);
			for (int k = 0; k < btns.Count; k++)
			{
				btns[k].selectOrder = k;
			}
			string text2 = string.Join("|", btns.Select((ModButton x) => x.ModID));
			if (text2 == text)
			{
				break;
			}
			text = text2;
		}
		sortedModButtons.AddRange(btns);
		if (btns.Count > 0)
		{
			ModButton._ButtonAboveSeparator = btns.Last();
		}
		else
		{
			ModButton._ButtonAboveSeparator = null;
		}
		list.Sort(CompareByModName);
		if (list.Count > 0)
		{
			ModButton._ButtonBelowSeparator = list[0];
		}
		else
		{
			ModButton._ButtonBelowSeparator = null;
		}
		sortedModButtons.AddRange(list);
		RefreshVisibleButtons();
		selectionChanged = _savedSelections.Length != btns.Count;
		_currentSelections = new string[btns.Count];
		for (int l = 0; l < btns.Count; l++)
		{
			_currentSelections[l] = btns[l].ModID;
			selectionChanged = selectionChanged || _savedSelections[l] != _currentSelections[l];
		}
		if (selectionChanged && CfgContainer._Mode != 0 && _initialized)
		{
			SwitchSelectMode(toSelect: true);
		}
		else if (!selectionChanged && CfgContainer._Mode == ConfigContainer.Mode.ModSelect)
		{
			SwitchSelectMode(toSelect: false);
		}
	}

	private void RefreshVisibleButtons()
	{
		for (int i = 0; i < modButtons.Length; i++)
		{
			modButtons[i].viewIndex = -1;
			modButtons[i].Hide();
		}
		visibleModButtons.Clear();
		if (_SearchMode)
		{
			for (int j = 0; j < sortedModButtons.Count; j++)
			{
				if (!sortedModButtons[j].IsDummy && sortedModButtons[j].MatchQuery(_searchQuery))
				{
					visibleModButtons.Add(sortedModButtons[j]);
				}
			}
		}
		else
		{
			visibleModButtons.AddRange(sortedModButtons);
		}
		for (int k = 0; k < visibleModButtons.Count; k++)
		{
			visibleModButtons[k].viewIndex = k;
			visibleModButtons[k].Show();
		}
		_ClampScrollPos();
	}

	private void _CheckRequirementsOrder(ref List<ModButton> btns)
	{
		for (int i = 0; i < btns.Count; i++)
		{
			foreach (string item in btns[i].itf.mod.requirements.Union(btns[i].itf.mod.priorities))
			{
				for (int j = 0; j < btns.Count; j++)
				{
					if (i != j && btns[j].ModID == item && btns[j].selectOrder < btns[i].selectOrder)
					{
						ModButton modButton = btns[j];
						ModButton modButton2 = btns[i];
						int selectOrder = btns[i].selectOrder;
						int selectOrder2 = btns[j].selectOrder;
						modButton.selectOrder = selectOrder;
						modButton2.selectOrder = selectOrder2;
					}
				}
			}
		}
	}

	private static int CompareBySelectOrder(ModButton x, ModButton y)
	{
		return x.selectOrder.CompareTo(y.selectOrder);
	}

	private static int CompareByModName(ModButton x, ModButton y)
	{
		if (x.itf.mod.DLCMissing == y.itf.mod.DLCMissing)
		{
			return ConfigContainer.comInfo.Compare(ListItem.GetRealName(x.itf.mod.LocalizedName), ListItem.GetRealName(y.itf.mod.LocalizedName), CompareOptions.StringSort);
		}
		if (x.itf.mod.DLCMissing && !y.itf.mod.DLCMissing)
		{
			return 1;
		}
		return -1;
	}

	internal void SwitchSelectMode(bool toSelect)
	{
		selectionChanged = false;
		if (toSelect)
		{
			CfgContainer._SwitchMode(ConfigContainer.Mode.ModSelect);
		}
		else
		{
			CfgContainer._SwitchMode(ConfigContainer.Mode.ModView);
		}
	}

	internal void RefreshSavedSelections()
	{
		ModButton[] array = modButtons;
		foreach (ModButton modButton in array)
		{
			modButton.dependentIndexes.Clear();
			modButton.selectOrder = 0;
			foreach (ModManager.Mod installedMod in ModManager.InstalledMods)
			{
				if (modButton.ModID == installedMod.id)
				{
					if (!"v1.9.15b".StartsWith(installedMod.targetGameVersion))
					{
						modButton.outdated = true;
					}
					if (ModManager.FailedRequirementIds.Contains(installedMod.id))
					{
						modButton.invalid = true;
					}
					if (modButton.invalid)
					{
						_ToggleMod_SubDisable(modButton);
					}
					else
					{
						modButton.selectEnabled = installedMod.enabled;
					}
					break;
				}
			}
		}
		List<ModButton> list = new List<ModButton>();
		array = modButtons;
		foreach (ModButton modButton2 in array)
		{
			modButton2.requirementIndexes.Clear();
			modButton2.dependentIndexes.Clear();
			if (modButton2.selectEnabled)
			{
				list.Add(modButton2);
			}
		}
		array = modButtons;
		foreach (ModButton modButton3 in array)
		{
			foreach (ModManager.Mod installedMod2 in ModManager.InstalledMods)
			{
				if (!(modButton3.ModID == installedMod2.id))
				{
					continue;
				}
				if (modButton3.selectEnabled)
				{
					modButton3.selectOrder = list.Count - installedMod2.loadOrder - 1;
				}
				for (int j = 0; j < installedMod2.requirements.Length; j++)
				{
					for (int k = 0; k < modButtons.Length; k++)
					{
						if (modButtons[k].ModID == installedMod2.requirements[j])
						{
							_AttachRequirement(modButton3, k);
							break;
						}
					}
				}
				for (int l = 0; l < installedMod2.priorities.Length; l++)
				{
					for (int m = 0; m < modButtons.Length; m++)
					{
						if (modButtons[m].ModID == installedMod2.priorities[l])
						{
							_AttachPriority(modButton3, m);
							break;
						}
					}
				}
				break;
			}
		}
		list.Sort(CompareBySelectOrder);
		_savedSelections = new string[list.Count];
		for (int n = 0; n < list.Count; n++)
		{
			_savedSelections[n] = list[n].ModID;
			list[n].selectOrder = n;
		}
	}

	private void _AttachRequirement(ModButton btn, int requires)
	{
		if (!btn.requirementIndexes.Add(requires))
		{
			return;
		}
		foreach (ModManager.Mod installedMod in ModManager.InstalledMods)
		{
			if (!(modButtons[requires].ModID == installedMod.id))
			{
				continue;
			}
			if (installedMod.requirements.Length != 0)
			{
				for (int i = 0; i < installedMod.requirements.Length; i++)
				{
					for (int j = 0; j < modButtons.Length; j++)
					{
						if (modButtons[j].ModID == installedMod.requirements[i])
						{
							_AttachRequirement(btn, j);
							break;
						}
					}
				}
			}
			else
			{
				_AttachDependent(btn);
			}
			break;
		}
	}

	private void _AttachDependent(ModButton btn)
	{
		foreach (int requirementIndex in btn.requirementIndexes)
		{
			modButtons[requirementIndex].dependentIndexes.Add(btn.index);
			_AttachDependent(modButtons[requirementIndex]);
		}
	}

	private void _AttachPriority(ModButton btn, int requires, bool nested = false)
	{
		if (!nested && !btn.priorityIndexes.Add(requires))
		{
			return;
		}
		foreach (ModManager.Mod installedMod in ModManager.InstalledMods)
		{
			if (!(modButtons[requires].ModID == installedMod.id))
			{
				continue;
			}
			if (installedMod.priorities.Length != 0)
			{
				for (int i = 0; i < installedMod.priorities.Length; i++)
				{
					for (int j = 0; j < modButtons.Length; j++)
					{
						if (modButtons[j].ModID == installedMod.priorities[i])
						{
							_AttachPriority(btn, j, nested: true);
							break;
						}
					}
				}
			}
			else
			{
				_AttachPrioritezed(btn);
			}
			break;
		}
	}

	private void _AttachPrioritezed(ModButton btn, bool nested = false)
	{
		foreach (int priorityIndex in btn.priorityIndexes)
		{
			modButtons[priorityIndex].prioritizedIndexes.Add(btn.index);
			if (!nested)
			{
				_AttachPrioritezed(modButtons[priorityIndex], nested: true);
			}
		}
	}

	internal void ScrollToShow(int targetIndex)
	{
		Custom.Log("Target index is:", targetIndex.ToString());
		if (targetIndex <= 0)
		{
			return;
		}
		int viewIndex = modButtons[targetIndex].viewIndex;
		Custom.Log("Mod button count is:", modButtons.Length.ToString(), "current view index is:", modButtons[targetIndex].viewIndex.ToString());
		if (viewIndex >= 0)
		{
			if (_scrollPos > viewIndex)
			{
				_scrollPos = viewIndex;
			}
			else if (_scrollPos + _scrollVisible < viewIndex)
			{
				_scrollPos = viewIndex - _scrollVisible;
			}
			_ClampScrollPos();
		}
	}

	internal ModButton GetModButton(string modID)
	{
		return modButtons[ConfigContainer.FindItfIndex(modID)];
	}

	internal ModButton GetModButtonAtTop()
	{
		return visibleModButtons[_scrollPos];
	}

	internal ModButton GetModButtonAtBottom()
	{
		return visibleModButtons[Math.Min(_scrollPos + _scrollVisible - 1, visibleModButtons.Count - 1)];
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = _roleButtons[0].bumpBehav.AddSize * 2f;
		_sideLines[0].y = 299.9f;
		_sideLines[1].y = 643.5f + num;
		_sideLines[0].scaleY = 320.6f - num;
		_sideLines[1].scaleY = 32.5f - num;
		_sideLines[2].y = -25f;
		num = _roleButtons[5].bumpBehav.AddSize * 2f;
		_sideLines[3].y = 43.5f + num;
		_sideLines[2].scaleY = 45.5f - num;
		_sideLines[3].scaleY = 281.6f - num;
		_sideLines[1].scaleY -= 16f;
		Color color = ((CfgContainer._Mode == ConfigContainer.Mode.ModConfig) ? MenuColorEffect.rgbDarkGrey : MenuColorEffect.rgbMediumGrey);
		_sideLines[0].color = color;
		_sideLines[1].color = color;
		_sideLines[2].color = color;
		_sideLines[3].color = color;
	}

	public override void Update()
	{
		base.Update();
		float value = _scrollPos;
		if (_slider.held)
		{
			value = _slider._floatPos;
		}
		value = Mathf.Clamp(value, 0f, _ScrollMax);
		_floatScrollPos = Custom.LerpAndTick(_floatScrollPos, value, 0.01f, 0.01f);
		_floatScrollVel *= Custom.LerpMap(Math.Abs(value - _floatScrollPos), 0.25f, 1.5f, 0.45f, 0.99f);
		_floatScrollVel += Mathf.Clamp(value - _floatScrollPos, -2.5f, 2.5f) / 2.5f * 0.15f;
		_floatScrollVel = Mathf.Clamp(_floatScrollVel, -1.2f, 1.2f);
		_floatScrollPos += _floatScrollVel;
		_listFocused = ConfigContainer.FocusedElement is IAmPartOfModList || (base.MenuMouseMode && MouseOver);
		if (!_SearchMode && (ConfigContainer.FocusedElement is ModButton || (base.MenuMouseMode && MouseOver)) && ListButton._swapIndex > 0)
		{
			_roleButtons[3].Show();
			_roleButtons[4].Show();
		}
		else
		{
			_roleButtons[3].Hide();
			_roleButtons[4].Hide();
		}
		ModButton._lastExpand = ModButton._expand;
		ModButton._expand = Custom.LerpAndTick(ModButton._expand, ModButton._boolExpand ? 1f : 0f, 0.15f, 0.05f);
		ModButton._height = Mathf.Lerp(25f, 130f, Custom.SCurve(ModButton._expand, 0.6f));
		int num = Mathf.FloorToInt(650f / ModButton._height);
		if (_scrollVisible != num)
		{
			_scrollVisible = num;
			_ClampScrollPos();
			_slider.max = _ScrollMax;
			if (_ScrollMax > 0)
			{
				_roleButtons[1].Show();
				_roleButtons[2].Show();
			}
			else
			{
				_roleButtons[1].Hide();
				_roleButtons[2].Hide();
			}
		}
	}

	private void Signal(UIfocusable trigger, int index = -1)
	{
		if (trigger is ModButton)
		{
			if (CfgContainer._Mode == ConfigContainer.Mode.ModSelect)
			{
				MenuTab.FlashApplyButton();
			}
			else if (CfgContainer._Mode == ConfigContainer.Mode.ModView)
			{
				CfgContainer._SwitchMode(ConfigContainer.Mode.ModConfig);
				ConfigContainer._ChangeActiveMod(index);
			}
		}
		else
		{
			if (!(trigger is ListButton))
			{
				return;
			}
			switch (index)
			{
			case 0:
				if (ConfigContainer.ActiveTabIndex != 0)
				{
					ConfigContainer._ChangeActiveTab(0);
				}
				return;
			case 7:
				ModButton._boolExpand = !ModButton._boolExpand;
				return;
			}
			Custom.Log("Scrollpos is:", _scrollPos.ToString(), "just added index of:", index.ToString());
			if (index < 9)
			{
				_scrollPos += index;
				_ClampScrollPos();
				return;
			}
			ModButton modButton = modButtons[ListButton._swapIndex];
			ModButton modButton2 = visibleModButtons[modButton.viewIndex + index - 10];
			int selectOrder = modButton2.selectOrder;
			modButton2.selectOrder = modButton.selectOrder;
			modButton.selectOrder = selectOrder;
			RefreshAllButtons();
			if (modButton2.selectOrder == selectOrder)
			{
				ModdingMenu.instance.ShowAlert(OptionalText.GetText(OptionalText.ID.MenuModList_SwapFailed));
			}
		}
	}

	private void _ToggleMod(ModButton btn)
	{
		if (!btn.selectEnabled && !btn.itf.mod.DLCMissing)
		{
			int maxOrder = _currentSelections.Length;
			_ToggleMod_SubEnable(btn, ref maxOrder);
		}
		else
		{
			_ToggleMod_SubDisable(btn);
		}
		int viewIndex = btn.viewIndex;
		RefreshAllButtons();
		CfgContainer._FocusNewElement(visibleModButtons[viewIndex]);
	}

	private void _ToggleMod_SubEnable(ModButton btn, ref int maxOrder)
	{
		if (btn.invalid || btn.itf.mod.DLCMissing)
		{
			return;
		}
		btn.selectEnabled = true;
		btn._UpdateThumbnail();
		btn.selectOrder = maxOrder;
		maxOrder++;
		foreach (int requirementIndex in btn.requirementIndexes)
		{
			if (requirementIndex >= 0 && btn.index != modButtons[requirementIndex].index && !modButtons[requirementIndex].selectEnabled)
			{
				_ToggleMod_SubEnable(modButtons[requirementIndex], ref maxOrder);
			}
		}
	}

	private void _ToggleMod_SubDisable(ModButton btn)
	{
		btn.selectEnabled = false;
		btn._UpdateThumbnail();
		btn.selectOrder = 0;
		foreach (int dependentIndex in btn.dependentIndexes)
		{
			if (modButtons[dependentIndex].selectEnabled)
			{
				_ToggleMod_SubDisable(modButtons[dependentIndex]);
			}
		}
	}

	internal void _SetEnableToAllMods(bool enable)
	{
		int maxOrder = _currentSelections.Length;
		for (int i = 1; i < modButtons.Length; i++)
		{
			if (enable)
			{
				if (!modButtons[i].selectEnabled && !modButtons[i].itf.mod.DLCMissing)
				{
					_ToggleMod_SubEnable(modButtons[i], ref maxOrder);
				}
			}
			else if (modButtons[i].selectEnabled)
			{
				_ToggleMod_SubDisable(modButtons[i]);
			}
		}
		RefreshAllButtons();
	}

	private void _ClampScrollPos()
	{
		_scrollPos = Custom.IntClamp(_scrollPos, 0, _ScrollMax);
	}
}
