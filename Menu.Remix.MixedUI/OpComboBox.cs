using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpComboBox : UIconfig, ICanBeTyped
{
	public class Queue : ConfigQueue
	{
		public OnSignalHandler onListOpen;

		public OnSignalHandler onListClose;

		protected override float sizeY => 30f;

		public Queue(Configurable<string> config, object sign = null)
			: base(config, sign)
		{
			if (config == null || config.info == null || config.info.acceptable == null || !(config.info.acceptable is ConfigAcceptableList<string>))
			{
				throw new ArgumentNullException("To use this constructor, Configurable<string> must have ConfigurableInfo with ConfigAccpetableList.");
			}
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 150f, 50f);
			List<UIelement> list = new List<UIelement>();
			OpComboBox opComboBox = new OpComboBox(config as Configurable<string>, new Vector2(posX, posY), width)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opComboBox.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opComboBox.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opComboBox.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opComboBox.OnValueChanged += onValueChanged;
			}
			if (onListOpen != null)
			{
				opComboBox.OnListOpen += onListOpen;
			}
			if (onListClose != null)
			{
				opComboBox.OnListClose += onListClose;
			}
			mainFocusable = opComboBox;
			list.Add(opComboBox);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opComboBox.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opComboBox.bumpBehav,
					description = opComboBox.description
				};
				list.Add(item);
			}
			opComboBox.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	private const float LBLTEXTTRIM = 32f;

	private readonly string DASHES = "------";

	protected ListItem[] _itemList;

	protected List<ListItem> _searchList;

	protected bool _neverOpened;

	protected DyeableRect _rect;

	protected DyeableRect _rectList;

	protected DyeableRect _rectScroll;

	protected FLabel _lblText;

	protected FLabel[] _lblList;

	protected FSprite _sprArrow;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	public ushort listHeight = 5;

	protected bool _mouseDown;

	protected bool _searchMode;

	protected bool _downward = true;

	protected int _dTimer;

	protected int _searchDelay;

	protected int _searchIdle;

	protected int _listTop;

	protected int _listHover = -1;

	protected float _scrollHeldPos;

	protected BumpBehaviour _bumpList;

	protected BumpBehaviour _bumpScroll;

	protected string _searchQuery = "";

	protected FSprite _searchCursor;

	protected int _inputDelay;

	private char _lastChar = '\r';

	protected GlowGradient _glowFocus;

	protected bool _IsResourceSelector
	{
		get
		{
			if (!(this is OpResourceSelector))
			{
				return this is OpResourceList;
			}
			return true;
		}
	}

	protected bool _IsListBox => this is OpListBox;

	public override string value
	{
		get
		{
			return base.value;
		}
		set
		{
			if (!(base.value != value))
			{
				return;
			}
			if (string.IsNullOrEmpty(value))
			{
				base.value = (allowEmpty ? "" : _itemList[0].name);
				return;
			}
			ListItem[] itemList = _itemList;
			for (int i = 0; i < itemList.Length; i++)
			{
				if (itemList[i].name == value)
				{
					base.value = value;
					break;
				}
			}
		}
	}

	public bool allowEmpty { get; private set; }

	protected int _listHeight => Custom.IntClamp(listHeight, 1, _itemList.Length);

	public Action<char> OnKeyDown { get; set; }

	protected internal override bool MouseOver
	{
		get
		{
			if (!base.MouseOver)
			{
				return _MouseOverList();
			}
			return true;
		}
	}

	public event OnSignalHandler OnListOpen;

	public event OnSignalHandler OnListClose;

	public OpComboBox(Configurable<string> config, Vector2 pos, float width, List<ListItem> list)
		: this((ConfigurableBase)config, pos, width, list)
	{
	}

	public OpComboBox(Configurable<string> config, Vector2 pos, float width, string[] array)
		: this(config, pos, width, _ArrayToList(array))
	{
	}

	protected static List<ListItem> _ArrayToList(string[] array)
	{
		List<ListItem> list = new List<ListItem>();
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(new ListItem(array[i], i));
		}
		return list;
	}

	public OpComboBox(Configurable<string> config, Vector2 pos, float width)
		: this(config, pos, width, _InfoToList(config))
	{
	}

	protected static List<ListItem> _InfoToList(Configurable<string> config)
	{
		if (config == null || config.info == null || config.info.acceptable == null || !(config.info.acceptable is ConfigAcceptableList<string>))
		{
			throw new ElementFormatException("To use this constructor, Configurable<string> must have ConfigurableInfo with ConfigAccpetableList.");
		}
		ConfigAcceptableList<string> configAcceptableList = config.info.acceptable as ConfigAcceptableList<string>;
		List<ListItem> list = new List<ListItem>();
		for (int i = 0; i < configAcceptableList.AcceptableValues.Length; i++)
		{
			list.Add(new ListItem(configAcceptableList.AcceptableValues[i], i));
		}
		return list;
	}

	internal OpComboBox(ConfigurableBase configBase, Vector2 pos, float width, List<ListItem> list)
		: base(configBase, pos, new Vector2(width, 24f))
	{
		_size = new Vector2(Mathf.Max(30f, base.size.x), 24f);
		fixedSize = new Vector2(-1f, 24f);
		if (!_IsResourceSelector)
		{
			if (list == null || list.Count < 1)
			{
				throw new ElementFormatException(this, "The list must contain at least one ListItem", base.Key);
			}
			list.Sort(ListItem.Comparer);
			_itemList = list.ToArray();
			_ResetIndex();
			_Initialize(base.defaultValue);
		}
	}

	protected internal override string DisplayDescription()
	{
		if (base.MenuMouseMode)
		{
			if (!held)
			{
				if (!string.IsNullOrEmpty(description))
				{
					return description;
				}
				return OptionalText.GetText(OptionalText.ID.OpComboBox_MouseOpenTuto);
			}
			if (_listHover >= 0)
			{
				string result = "";
				if (_searchMode)
				{
					if (_listTop + _listHover < _searchList.Count)
					{
						result = _searchList[_listTop + _listHover].desc;
					}
				}
				else if (_listTop + _listHover < _itemList.Length)
				{
					result = _itemList[_listTop + _listHover].desc;
				}
				if (!string.IsNullOrEmpty(result))
				{
					return result;
				}
			}
			if (!string.IsNullOrEmpty(description))
			{
				return description;
			}
			if (_searchMode)
			{
				return OptionalText.GetText(OptionalText.ID.OpComboBox_MouseSearchTuto);
			}
			return OptionalText.GetText(OptionalText.ID.OpComboBox_MouseUseTuto);
		}
		if (!held)
		{
			if (!string.IsNullOrEmpty(description))
			{
				return description;
			}
			return OptionalText.GetText(OptionalText.ID.OpComboBox_NonMouseOpenTuto);
		}
		if (_listHover >= 0)
		{
			string result2 = "";
			if (_searchMode)
			{
				if (_listTop + _listHover < _searchList.Count)
				{
					result2 = _searchList[_listTop + _listHover].desc;
				}
			}
			else if (_listTop + _listHover < _itemList.Length)
			{
				result2 = _itemList[_listTop + _listHover].desc;
			}
			if (!string.IsNullOrEmpty(result2))
			{
				return result2;
			}
		}
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		return OptionalText.GetText(OptionalText.ID.OpComboBox_NonMouseUseTuto);
	}

	protected void _ResetIndex()
	{
		for (int i = 0; i < _itemList.Length; i++)
		{
			_itemList[i].index = i;
		}
		_searchDelay = Mathf.FloorToInt(Custom.LerpMap(Mathf.Clamp(_itemList.Length, 10, 90), 10f, 90f, 10f, 50f));
		_searchDelay = UIelement.FrameMultiply(_searchDelay);
		if (ModManager.MMF)
		{
			if (Custom.rainWorld.options.quality == Options.Quality.HIGH)
			{
				_searchDelay = 10;
			}
			else if (Custom.rainWorld.options.quality == Options.Quality.MEDIUM)
			{
				_searchDelay = Math.Max(10, _searchDelay / 2);
			}
		}
	}

	protected virtual void _Initialize(string defaultName)
	{
		OnKeyDown = (Action<char>)Delegate.Combine(OnKeyDown, new Action<char>(KeyboardAccept));
		this.Assign();
		mouseOverStopsScrollwheel = true;
		if (string.IsNullOrEmpty(defaultName))
		{
			allowEmpty = true;
			base.defaultValue = "";
			_value = "";
		}
		else
		{
			bool flag = false;
			for (int i = 0; i < _itemList.Length; i++)
			{
				if (_itemList[i].name == defaultName)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				base.defaultValue = defaultName;
				_value = base.defaultValue;
			}
			else
			{
				base.defaultValue = _itemList[0].name;
				_value = _itemList[0].name;
			}
		}
		_rect = new DyeableRect(myContainer, Vector2.zero, base.size);
		_lblText = UIelement.FLabelCreate("");
		_lblText.text = LabelTest.TrimText(string.IsNullOrEmpty(value) ? DASHES : _GetDisplayValue(), base.size.x - 32f, addDots: true);
		_lblText.alignment = FLabelAlignment.Left;
		_lblText.x = 12f;
		_lblText.y = base.size.y / 2f;
		myContainer.AddChild(_lblText);
		if (_IsListBox)
		{
			_neverOpened = false;
			_rectList = new DyeableRect(myContainer, Vector2.zero, base.size);
			_rectScroll = new DyeableRect(myContainer, Vector2.zero, Vector2.one * 15f);
			_glowFocus = new GlowGradient(myContainer, Vector2.zero, new Vector2((base.size.x - 25f) / 2f, 15f))
			{
				color = colorEdge
			};
			_glowFocus.Hide();
		}
		else
		{
			_neverOpened = true;
			_sprArrow = new FSprite("Big_Menu_Arrow")
			{
				scale = 0.5f,
				rotation = 180f,
				anchorX = 0.5f,
				anchorY = 0.5f
			};
			myContainer.AddChild(_sprArrow);
			_sprArrow.SetPosition(base.size.x - 12f, base.size.y / 2f);
		}
		_lblList = new FLabel[0];
		_searchCursor = new FSprite("modInputCursor");
		myContainer.AddChild(_searchCursor);
		_searchCursor.isVisible = false;
		_bumpList = new BumpBehaviour(this)
		{
			held = false,
			Focused = false
		};
		_bumpScroll = new BumpBehaviour(this)
		{
			held = false,
			Focused = false
		};
		GrafUpdate(0f);
	}

	public ListItem[] GetItemList()
	{
		return _itemList;
	}

	public override void Reset()
	{
		if (held)
		{
			_CloseList();
		}
		base.Reset();
	}

	public void SetAllowEmpty()
	{
		if (string.IsNullOrEmpty(cfgEntry.defaultValue))
		{
			if (!allowEmpty && value == _itemList[0].name)
			{
				_value = "";
			}
			base.defaultValue = "";
		}
		allowEmpty = true;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		base.bumpBehav.greyedOut = greyedOut;
		_rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
		_rect.fillAlpha = base.bumpBehav.FillAlpha;
		_rect.addSize = new Vector2(4f, 4f) * base.bumpBehav.AddSize;
		_rect.colorFill = (greyedOut ? base.bumpBehav.GetColor(colorFill) : colorFill);
		_rect.GrafUpdate(timeStacker);
		Color color = (held ? MenuColorEffect.MidToDark(_rect.colorEdge) : _rect.colorEdge);
		if (!_IsListBox)
		{
			_sprArrow.color = color;
			_lblText.color = (_searchMode ? _rect.colorEdge : color);
		}
		_lblText.color = (_searchMode ? _rect.colorEdge : color);
		if (!_IsListBox && !held)
		{
			_lblText.text = LabelTest.TrimText(string.IsNullOrEmpty(value) ? DASHES : _GetDisplayValue(), base.size.x - 32f, addDots: true);
			return;
		}
		_rectList.size.x = base.size.x;
		_rectList.pos = new Vector2(0f, _downward ? (0f - _rectList.size.y) : base.size.y);
		if (_IsListBox && _downward)
		{
			_rectList.pos.y += _rectList.size.y;
		}
		_rectList.addSize = new Vector2(4f, 4f) * _bumpList.AddSize;
		_rectList.colorEdge = _bumpList.GetColor(colorEdge);
		_rectList.colorFill = colorFill;
		_rectList.fillAlpha = (_IsListBox ? _bumpList.FillAlpha : Mathf.Lerp(0.5f, 0.7f, _bumpList.col));
		_rectList.GrafUpdate(timeStacker);
		for (int i = 0; i < _lblList.Length; i++)
		{
			string text = ((!_searchMode) ? _itemList[_listTop + i].displayName : ((_searchList.Count > _listTop + i) ? _searchList[_listTop + i].displayName : ""));
			_lblList[i].text = LabelTest.TrimText(text, base.size.x - 36f, addDots: true);
			_lblList[i].color = ((text == value) ? MenuColorEffect.MidToDark(_rect.colorEdge) : _rectList.colorEdge);
			if (i == _listHover)
			{
				_lblList[i].color = Color.Lerp(_lblList[i].color, (_mouseDown || text == value) ? MenuColorEffect.MidToDark(_lblList[i].color) : Color.white, _bumpList.Sin((text == _GetDisplayValue()) ? 60f : 10f));
			}
			_lblList[i].x = 12f;
			_lblList[i].y = -15f - 20f * (float)i + (_downward ? 0f : (base.size.y + _rectList.size.y));
			if (_IsListBox && _downward)
			{
				_lblList[i].y += _rectList.size.y;
			}
		}
		if (_glowFocus != null)
		{
			if (_listHover >= 0 && (_MouseOverList() || (!base.MenuMouseMode && held)))
			{
				_glowFocus.Show();
				_glowFocus.pos = _lblList[Math.Min(_listHover, _lblList.Length - 1)].GetPosition() - new Vector2(0f, _glowFocus.radV);
				_glowFocus.color = Color.Lerp(_rect.colorEdge, Color.white, 0.6f);
				_glowFocus.alpha = Mathf.Lerp(0.2f, 0.6f, _bumpList.Sin(10f));
			}
			else
			{
				_glowFocus.Hide();
			}
		}
		_lblText.text = LabelTest.TrimText(_searchMode ? _searchQuery : (string.IsNullOrEmpty(value) ? DASHES : _GetDisplayValue()), base.size.x - 32f, addDots: true);
		if (_searchMode)
		{
			_searchCursor.x = Mathf.Min(base.size.x - 24f, 12f + LabelTest.GetWidth(_searchQuery) + LabelTest.CharMean(bigText: false));
			_searchCursor.y = base.size.y * 0.5f + ((_IsListBox && _downward) ? _rectList.size.y : 0f);
			_searchCursor.color = Color.Lerp(colorEdge, MenuColorEffect.MidToDark(colorEdge), base.bumpBehav.Sin((_searchList.Count > 0) ? 10f : 30f));
			_searchCursor.alpha = 0.4f + 0.6f * Mathf.Clamp01((float)_searchIdle / (float)_searchDelay);
		}
		int num = (_searchMode ? _searchList.Count : _itemList.Length);
		if (num > _lblList.Length)
		{
			_rectScroll.Show();
			_rectScroll.pos.x = _rectList.pos.x + _rectList.size.x - 20f;
			if (!Mathf.Approximately(_rectScroll.size.y, _ScrollLen(num)))
			{
				_rectScroll.size.y = _ScrollLen(num);
				_rectScroll.pos.y = _ScrollPos(num);
			}
			else
			{
				_rectScroll.pos.y = Custom.LerpAndTick(_rectScroll.pos.y, _ScrollPos(num), _bumpScroll.held ? 0.6f : 0.2f, (_bumpScroll.held ? 0.6f : 0.2f) / UIelement.frameMulti);
			}
			_rectScroll.addSize = new Vector2(2f, 2f) * _bumpScroll.AddSize;
			_rectScroll.colorEdge = _bumpScroll.GetColor(colorEdge);
			_rectScroll.colorFill = (_bumpScroll.held ? _rectScroll.colorEdge : colorFill);
			_rectScroll.fillAlpha = (_bumpScroll.held ? 1f : _bumpScroll.FillAlpha);
			_rectScroll.GrafUpdate(timeStacker);
		}
		else
		{
			_rectScroll.Hide();
		}
	}

	protected float _ScrollPos(int listSize)
	{
		return _rectList.pos.y + 10f + (_rectList.size.y - 20f - _rectScroll.size.y) * (float)(listSize - _lblList.Length - _listTop) / (float)(listSize - _lblList.Length);
	}

	protected float _ScrollLen(int listSize)
	{
		return (_rectList.size.y - 40f) * Mathf.Clamp01((float)_lblList.Length / (float)listSize) + 20f;
	}

	public int GetIndex(string checkName = "")
	{
		if (string.IsNullOrEmpty(checkName))
		{
			checkName = value;
		}
		for (int i = 0; i < _itemList.Length; i++)
		{
			if (_itemList[i].name == checkName)
			{
				return _itemList[i].index;
			}
		}
		return -1;
	}

	public void KeyboardAccept(char input)
	{
		if (!held || !_searchMode || _bumpScroll.held)
		{
			return;
		}
		if (input == '\b')
		{
			if (_searchQuery.Length > 0)
			{
				_searchQuery = _searchQuery.Substring(0, _searchQuery.Length - 1);
				_searchIdle = -1;
				PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
			}
		}
		else if (char.IsLetterOrDigit(input) || input == ' ')
		{
			base.bumpBehav.flash = 2.5f;
			_searchQuery += input;
			_searchIdle = -1;
			PlaySound(SoundID.MENU_Checkbox_Uncheck);
		}
	}

	protected void _SearchModeUpdate()
	{
		ForceMenuMouseMode(true);
		if (_inputDelay == 0 && _searchIdle < UIelement.FrameMultiply(_searchDelay))
		{
			_searchIdle++;
			if (_searchIdle == _searchDelay)
			{
				_RefreshSearchList();
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (_dTimer > 0)
		{
			_dTimer--;
		}
		_rect.Update();
		_rectList?.Update();
		_rectScroll?.Update();
		if (greyedOut)
		{
			DyeableRect rectList = _rectList;
			if (rectList != null && !rectList.isHidden)
			{
				_mouseDown = false;
				held = false;
				if (!_IsListBox)
				{
					_CloseList();
				}
			}
		}
		else if (base.MenuMouseMode)
		{
			_MouseModeUpdate();
		}
		else
		{
			_NonMouseModeUpdate();
		}
	}

	protected virtual void _MouseModeUpdate()
	{
		base.bumpBehav.Focused = base.MouseOver && !_MouseOverList();
		if (held)
		{
			_bumpList.Focused = _MouseOverList();
			_bumpScroll.Focused = base.MousePos.x >= _rectScroll.pos.x && base.MousePos.x <= _rectScroll.pos.x + _rectScroll.size.x;
			_bumpScroll.Focused = _bumpScroll.Focused && base.MousePos.y >= _rectScroll.pos.y && base.MousePos.y <= _rectScroll.pos.y + _rectScroll.size.y;
			if (_searchMode && !_bumpScroll.held)
			{
				_SearchModeUpdate();
			}
			int num = (_searchMode ? _searchList.Count : _itemList.Length);
			if (_bumpScroll.held)
			{
				if (Input.GetMouseButton(0))
				{
					int num2 = Mathf.RoundToInt((base.MousePos.y - _rectList.pos.y + base.pos.y - 10f - _scrollHeldPos) * (float)(num - _lblList.Length) / (_rectList.size.y - 20f - _rectScroll.size.y));
					num2 = Custom.IntClamp(num - _lblList.Length - num2, 0, num - _lblList.Length);
					if (_listTop != num2)
					{
						PlaySound(SoundID.MENU_Scroll_Tick);
						_listTop = num2;
						_bumpScroll.flash = Mathf.Min(1f, _bumpScroll.flash + 0.2f);
						_bumpScroll.sizeBump = Mathf.Min(2.5f, _bumpScroll.sizeBump + 0.3f);
					}
				}
				else
				{
					_bumpScroll.held = false;
					_mouseDown = false;
					PlaySound(SoundID.MENU_Scroll_Tick);
				}
			}
			else if (MouseOver)
			{
				if (Input.GetMouseButton(0) && !_mouseDown)
				{
					if (_bumpScroll.Focused && num > _lblList.Length)
					{
						_scrollHeldPos = base.MousePos.y - _rectScroll.pos.y + base.pos.y;
						_bumpScroll.held = true;
						PlaySound(SoundID.MENU_First_Scroll_Tick);
					}
					else
					{
						_mouseDown = true;
					}
				}
				if (!_MouseOverList())
				{
					if (!Input.GetMouseButton(0) && _mouseDown)
					{
						_mouseDown = false;
						if (_dTimer > 0)
						{
							_dTimer = 0;
							_searchMode = true;
							_EnterSearchMode();
							return;
						}
						_dTimer = UIelement.FrameMultiply(15);
						if (allowEmpty)
						{
							value = "";
						}
						PlaySound(SoundID.MENU_Checkbox_Uncheck);
						goto IL_0626;
					}
				}
				else
				{
					if (base.MousePos.x >= 10f && base.MousePos.x <= _rectList.size.x - 30f)
					{
						if (_downward)
						{
							_listHover = Mathf.FloorToInt((base.MousePos.y + _rectList.size.y + 10f) / 20f);
						}
						else
						{
							_listHover = Mathf.FloorToInt((base.MousePos.y - base.size.y + 10f) / 20f);
						}
						if (_listHover > _lblList.Length || _listHover <= 0)
						{
							_listHover = -1;
						}
						else
						{
							_listHover = _lblList.Length - _listHover;
						}
					}
					else
					{
						_listHover = -1;
					}
					if (base.Menu.mouseScrollWheelMovement != 0)
					{
						int val = _listTop + (int)Mathf.Sign(base.Menu.mouseScrollWheelMovement) * Mathf.CeilToInt((float)_lblList.Length / 2f);
						val = Custom.IntClamp(val, 0, num - _lblList.Length);
						if (_listTop != val)
						{
							PlaySound(SoundID.MENU_Scroll_Tick);
							_listTop = val;
							_bumpScroll.flash = Mathf.Min(1f, _bumpScroll.flash + 0.2f);
							_bumpScroll.sizeBump = Mathf.Min(2.5f, _bumpScroll.sizeBump + 0.3f);
						}
					}
					else if (!Input.GetMouseButton(0) && _mouseDown)
					{
						_mouseDown = false;
						if (_listHover >= 0)
						{
							string name = value;
							if (_searchMode)
							{
								if (_listTop + _listHover < _searchList.Count)
								{
									name = _searchList[_listTop + _listHover].name;
								}
							}
							else if (_listTop + _listHover < _itemList.Length)
							{
								name = _itemList[_listTop + _listHover].name;
							}
							if (name != value)
							{
								value = name;
								PlaySound(SoundID.MENU_MultipleChoice_Clicked);
								goto IL_0626;
							}
							PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
						}
					}
				}
			}
			else if ((Input.GetMouseButton(0) && !_mouseDown) || (_mouseDown && !Input.GetMouseButton(0)))
			{
				PlaySound(SoundID.MENU_Checkbox_Uncheck);
				goto IL_0626;
			}
			_bumpList.Update();
			_bumpScroll.Update();
		}
		else
		{
			if (greyedOut)
			{
				return;
			}
			if (base.MouseOver)
			{
				if (Input.GetMouseButton(0))
				{
					_mouseDown = true;
				}
				else if (_mouseDown)
				{
					_mouseDown = false;
					if (_dTimer > 0)
					{
						_dTimer = 0;
						_searchMode = true;
						_EnterSearchMode();
					}
					else
					{
						_dTimer = UIelement.FrameMultiply(15);
					}
					held = true;
					fixedSize = base.size;
					_OpenList();
					PlaySound(SoundID.MENU_Checkbox_Check);
				}
				else if (base.Menu.mouseScrollWheelMovement != 0)
				{
					int index = GetIndex();
					int val2 = index + (int)Mathf.Sign(base.Menu.mouseScrollWheelMovement);
					val2 = Custom.IntClamp(val2, 0, _itemList.Length - 1);
					if (val2 != index)
					{
						base.bumpBehav.flash = 1f;
						PlaySound(SoundID.MENU_Scroll_Tick);
						base.bumpBehav.sizeBump = Mathf.Min(2.5f, base.bumpBehav.sizeBump + 1f);
						value = _itemList[val2].name;
					}
				}
			}
			else if (!Input.GetMouseButton(0))
			{
				_mouseDown = false;
			}
		}
		return;
		IL_0626:
		_mouseDown = false;
		held = false;
		_CloseList();
	}

	protected virtual void _NonMouseModeUpdate()
	{
		base.bumpBehav.Focused = base.Focused && !held;
		if (!held)
		{
			return;
		}
		_bumpList.Focused = true;
		int listSize;
		int dir;
		if (!base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Throw))
		{
			listSize = (_searchMode ? _searchList.Count : _itemList.Length);
			if (base.CtlrInput.y != 0)
			{
				dir = base.bumpBehav.JoystickPressAxis(vertical: true);
				if (dir != 0)
				{
					_ScrollTick(first: true);
				}
				else
				{
					dir = base.bumpBehav.JoystickHeldAxis(vertical: true, 2f);
					if (dir != 0)
					{
						_ScrollTick(first: false);
					}
				}
			}
			if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump))
			{
				string name = value;
				if (_searchMode)
				{
					if (_listTop + _listHover < _searchList.Count)
					{
						name = _searchList[_listTop + _listHover].name;
					}
				}
				else if (_listTop + _listHover < _itemList.Length)
				{
					name = _itemList[_listTop + _listHover].name;
				}
				if (name != value)
				{
					value = name;
					PlaySound(SoundID.MENU_MultipleChoice_Clicked);
					goto IL_019e;
				}
				PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
			}
			_bumpList.Update();
			_bumpScroll.Update();
			return;
		}
		goto IL_019e;
		IL_019e:
		held = false;
		if (!_IsListBox)
		{
			_CloseList();
		}
		void _ScrollTick(bool first)
		{
			bool flag = true;
			_listHover -= Math.Sign(dir);
			if (_listHover < 0)
			{
				if (_listTop > 0)
				{
					_listTop--;
					_listHover = 0;
					_bumpScroll.flash = Mathf.Min(1f, _bumpScroll.flash + 0.2f);
					_bumpScroll.sizeBump = Mathf.Min(2.5f, _bumpScroll.sizeBump + 0.3f);
				}
				else
				{
					flag = false;
					_listHover = 0;
				}
			}
			if (_listHover >= _lblList.Length)
			{
				if (_listTop < listSize - _lblList.Length)
				{
					_listTop++;
					_listHover--;
					_bumpScroll.flash = Mathf.Min(1f, _bumpScroll.flash + 0.2f);
					_bumpScroll.sizeBump = Mathf.Min(2.5f, _bumpScroll.sizeBump + 0.3f);
				}
				else
				{
					flag = false;
					_listHover--;
				}
			}
			if (flag)
			{
				PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
			}
		}
	}

	protected internal override void NonMouseSetHeld(bool newHeld)
	{
		base.NonMouseSetHeld(newHeld);
		if (newHeld)
		{
			if (!_IsListBox)
			{
				_OpenList();
				PlaySound(SoundID.MENU_Checkbox_Check);
			}
			_listHover = Custom.IntClamp(GetIndex() - _listTop, 0, _lblList.Length - 1);
		}
		else if (!_IsListBox)
		{
			_CloseList();
		}
	}

	protected internal override void Change()
	{
		base.Change();
		_size.x = Mathf.Max(30f, _size.x);
		_rect.size = base.size;
		_lblText.x = 12f;
		_lblText.y = base.size.y / 2f;
		if (_glowFocus != null)
		{
			_glowFocus.radH = (_size.x - 25f) / 2f;
		}
		if (_IsListBox && _downward)
		{
			_rect.pos.y = _rectList.size.y;
			_lblText.y += _rectList.size.y;
		}
		else
		{
			_rect.pos.y = 0f;
		}
		if (!_IsListBox)
		{
			_sprArrow.SetPosition(base.size.x - 12f, base.size.y / 2f);
		}
	}

	protected internal override void Deactivate()
	{
		base.Deactivate();
		if (_IsListBox)
		{
			_searchMode = false;
			_bumpScroll.held = false;
		}
		else
		{
			_CloseList();
		}
	}

	protected void _OpenList()
	{
		float num = 20f * (float)Mathf.Clamp(_itemList.Length, 1, _listHeight) + 10f;
		if (!_IsListBox)
		{
			this.OnListOpen?.Invoke(this);
			if (num < GetPos().y)
			{
				_downward = true;
			}
			else if (100f < GetPos().y)
			{
				_downward = true;
				num = 100f;
			}
			else
			{
				float num2 = 600f;
				if (base.InScrollBox)
				{
					num2 = (base.scrollBox.horizontal ? base.scrollBox.size.y : base.scrollBox.contentSize);
				}
				num2 -= GetPos().y + base.size.y;
				if (num2 < GetPos().y)
				{
					_downward = true;
					num = Mathf.Floor(GetPos().y / 20f) * 20f - 10f;
				}
				else
				{
					_downward = false;
					num = Mathf.Min(num, Mathf.Clamp(Mathf.Floor(num2 / 20f), 1f, _listHeight) * 20f + 10f);
				}
			}
			if (_neverOpened)
			{
				_rectList = new DyeableRect(myContainer, Vector2.zero, base.size)
				{
					fillAlpha = 0.95f
				};
				_rectScroll = new DyeableRect(myContainer, Vector2.zero, 15f * Vector2.one);
				_glowFocus = new GlowGradient(myContainer, Vector2.zero, new Vector2((base.size.x - 25f) / 2f, 15f))
				{
					color = colorEdge
				};
				_neverOpened = false;
			}
			_sprArrow.rotation = (_downward ? 180f : 0f);
		}
		_rectList.Show();
		_rectList.size = new Vector2(base.size.x, num);
		_rectList.pos = new Vector2(0f, _downward ? (0f - _rectList.size.y) : base.size.y);
		if (_IsListBox && _downward)
		{
			_rectList.pos.y += _rectList.size.y;
			_rect.pos.y = _rectList.size.y;
			_lblText.y += _rectList.size.y;
		}
		_glowFocus.Show();
		_glowFocus.radH = (base.size.x - 25f) / 2f;
		_lblList = new FLabel[Mathf.FloorToInt(num / 20f)];
		if (_downward)
		{
			_listTop = GetIndex() + 1;
			if (_listTop > _itemList.Length - _lblList.Length)
			{
				_listTop = _itemList.Length - _lblList.Length;
			}
		}
		else
		{
			_listTop = GetIndex() - _lblList.Length;
		}
		if (_listTop < 0)
		{
			_listTop = 0;
		}
		for (int i = 0; i < _lblList.Length; i++)
		{
			_lblList[i] = UIelement.FLabelCreate("");
			_lblList[i].text = _itemList[_listTop + i].EffectiveDisplayName;
			_lblList[i].alignment = FLabelAlignment.Left;
			myContainer.AddChild(_lblList[i]);
		}
		if (_lblList.Length < _itemList.Length)
		{
			_rectScroll.Show();
			_rectScroll.size = new Vector2(15f, _ScrollLen(_itemList.Length));
			_rectScroll.pos = new Vector2(_rectList.pos.x + _rectList.size.x - 20f, _ScrollPos(_itemList.Length));
		}
		else
		{
			_rectScroll.Hide();
		}
		base.bumpBehav.flash = 1f;
		_bumpList.flash = 1f;
		_bumpList.held = false;
		_bumpScroll.held = false;
		if (base.InScrollBox && !_IsListBox)
		{
			base.scrollBox.ScrollToRect(new Rect(base.PosX, base.PosY - (_downward ? _rectList.size.y : 0f), base.size.x, base.size.y + _rectList.size.y));
		}
	}

	private void _CloseList()
	{
		this.OnListClose?.Invoke(this);
		_searchMode = false;
		_searchCursor.isVisible = false;
		fixedSize = null;
		if (!_neverOpened)
		{
			_rectList.Hide();
			_rectScroll.Hide();
			_glowFocus.Hide();
		}
		for (int i = 0; i < _lblList.Length; i++)
		{
			_lblList[i].isVisible = false;
			_lblList[i].RemoveFromContainer();
		}
		_lblList = new FLabel[0];
		_bumpScroll.held = false;
	}

	protected bool _MouseOverList()
	{
		if (!base.MenuMouseMode)
		{
			return false;
		}
		if (!held && !_IsListBox)
		{
			return false;
		}
		if (base.MousePos.x < 0f || base.MousePos.x > base.size.x)
		{
			return false;
		}
		if (_downward)
		{
			if (_IsListBox)
			{
				if (base.MousePos.y >= 0f)
				{
					return base.MousePos.y <= _rectList.size.y;
				}
				return false;
			}
			if (base.MousePos.y >= 0f - _rectList.size.y)
			{
				return base.MousePos.y <= 0f;
			}
			return false;
		}
		if (base.MousePos.y >= base.size.y)
		{
			return base.MousePos.y <= base.size.y + _rectList.size.y;
		}
		return false;
	}

	public void AddItems(bool sort = true, params ListItem[] newItems)
	{
		if (_IsResourceSelector)
		{
			throw new InvalidActionException(this, "You cannot use AddItems for OpResourceSelector", base.Key);
		}
		List<ListItem> list = new List<ListItem>(_itemList);
		list.AddRange(newItems);
		if (sort)
		{
			list.Sort(ListItem.Comparer);
		}
		_itemList = list.ToArray();
		_ResetIndex();
		Change();
	}

	public void RemoveItems(bool selectNext = true, params string[] names)
	{
		if (_IsResourceSelector)
		{
			throw new InvalidActionException(this, "You cannot use RemoveItems for OpResourceSelector", base.Key);
		}
		List<ListItem> list = new List<ListItem>(_itemList);
		foreach (string text in names)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (!(list[j].name == text))
				{
					continue;
				}
				if (list.Count == 1)
				{
					throw new InvalidActionException(this, "You cannot remove every items in OpComboBox", base.Key);
				}
				if (text == value)
				{
					_value = (selectNext ? list[(j == 0) ? 1 : (j - 1)].name : "");
				}
				list.RemoveAt(j);
				break;
			}
		}
		_itemList = list.ToArray();
		_ResetIndex();
		Change();
	}

	protected internal override string CopyToClipboard()
	{
		if (!_IsListBox)
		{
			_CloseList();
		}
		return base.CopyToClipboard();
	}

	protected internal override bool CopyFromClipboard(string value)
	{
		if (!_searchMode)
		{
			_searchMode = true;
			_EnterSearchMode();
		}
		_searchQuery = value;
		_RefreshSearchList();
		return true;
	}

	protected void _EnterSearchMode()
	{
		_searchQuery = "";
		_searchList = new List<ListItem>(_itemList);
		_searchList.Sort(ListItem.Comparer);
		_searchCursor.isVisible = true;
		_searchCursor.SetPosition(LabelTest.CharMean(bigText: false) * 1.5f, base.size.y * 0.5f + ((_IsListBox && _downward) ? _rectList.size.y : 0f));
		_searchIdle = 1000;
	}

	protected void _RefreshSearchList()
	{
		int num = ((_searchList.Count > 0) ? GetIndex(_searchList[_listTop].name) : 0);
		_searchList.Clear();
		for (int i = 0; i < _itemList.Length; i++)
		{
			if (ListItem.SearchMatch(_searchQuery, _itemList[i].displayName) || ListItem.SearchMatch(_searchQuery, _itemList[i].name))
			{
				_searchList.Add(_itemList[i]);
			}
		}
		_searchList.Sort(ListItem.Comparer);
		for (int j = 1; j < _searchList.Count; j++)
		{
			if (num > GetIndex(_searchList[j].name))
			{
				_listTop = Math.Max(0, j - 1);
				return;
			}
		}
		_listTop = 0;
	}

	protected string _GetDisplayValue()
	{
		ListItem[] itemList = _itemList;
		for (int i = 0; i < itemList.Length; i++)
		{
			ListItem listItem = itemList[i];
			if (listItem.name == value)
			{
				return listItem.EffectiveDisplayName;
			}
		}
		return value;
	}
}
