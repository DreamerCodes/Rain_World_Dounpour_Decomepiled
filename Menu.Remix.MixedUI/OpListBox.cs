using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpListBox : OpComboBox
{
	public new class Queue : ConfigQueue
	{
		protected readonly ushort lineCount;

		protected readonly bool downward;

		protected override float sizeY => 20f * (float)(int)lineCount + 34f;

		public Queue(Configurable<string> config, ushort lineCount = 5, bool downward = true, object sign = null)
			: base(config, sign)
		{
			if (config == null || config.info == null || config.info.acceptable == null || !(config.info.acceptable is ConfigAcceptableList<string>))
			{
				throw new ArgumentNullException("To use this constructor, Configurable<string> must have ConfigurableInfo with ConfigAccpetableList.");
			}
			this.lineCount = lineCount;
			this.downward = downward;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 150f, 50f);
			List<UIelement> list = new List<UIelement>();
			OpListBox opListBox = new OpListBox(config as Configurable<string>, new Vector2(posX, posY), width, lineCount, downward)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opListBox.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opListBox.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opListBox.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opListBox.OnValueChanged += onValueChanged;
			}
			mainFocusable = opListBox;
			list.Add(opListBox);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opListBox.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY + sizeY - 30f), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opListBox.bumpBehav,
					description = opListBox.description
				};
				list.Add(item);
			}
			opListBox.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	protected internal override Rect FocusRect
	{
		get
		{
			if (!_downward)
			{
				return base.FocusRect;
			}
			Rect result = new Rect(base.ScreenPos.x, base.ScreenPos.y + _rectList.size.y, base.size.x, base.size.y);
			if (tab != null)
			{
				result.x += tab._container.x;
				result.y += tab._container.y;
			}
			return result;
		}
	}

	public OpListBox(Configurable<string> config, Vector2 pos, float width, List<ListItem> list, ushort lineCount = 5, bool downward = true)
		: this((ConfigurableBase)config, pos, width, list, lineCount, downward)
	{
	}

	public OpListBox(Configurable<string> config, Vector2 pos, float width, string[] array, ushort lineCount = 5, bool downward = true)
		: this((ConfigurableBase)config, pos, width, OpComboBox._ArrayToList(array), lineCount, downward)
	{
	}

	internal OpListBox(ConfigurableBase config, Vector2 pos, float width, List<ListItem> list, ushort lineCount = 5, bool downward = true)
		: base(config, pos, width, list)
	{
		listHeight = lineCount;
		_downward = downward;
		if (!(this is OpResourceList))
		{
			_OpenList();
		}
	}

	public OpListBox(Configurable<string> config, Vector2 pos, float width, ushort lineCount = 5, bool downward = true)
		: this(config, pos, width, OpComboBox._InfoToList(config), lineCount, downward)
	{
	}

	protected internal override string DisplayDescription()
	{
		if (base.MenuMouseMode)
		{
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
		if (held && _listHover >= 0)
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

	public override void GrafUpdate(float dt)
	{
		base.GrafUpdate(dt);
	}

	public override void Update()
	{
		base.Update();
	}

	protected override void _MouseModeUpdate()
	{
		if (greyedOut)
		{
			return;
		}
		_bumpList.Focused = _MouseOverList();
		bool flag = base.MousePos.x >= _rectScroll.pos.x && base.MousePos.x <= _rectScroll.pos.x + _rectScroll.size.x;
		_bumpScroll.Focused = flag && base.MousePos.y >= _rectScroll.pos.y && base.MousePos.y <= _rectScroll.pos.y + _rectScroll.size.y;
		flag = base.MousePos.x >= 0f && base.MousePos.x <= base.size.x;
		base.bumpBehav.Focused = (_downward ? (flag && base.MousePos.y >= _rectList.size.y && base.MousePos.y <= _rectList.size.y + base.size.y) : (base.MouseOver && !_MouseOverList()));
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
		else if ((!_downward && MouseOver) || (flag && base.MousePos.y >= 0f && base.MousePos.y <= _rectList.size.y + base.size.y))
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
					if (base.allowEmpty)
					{
						value = "";
					}
					PlaySound(SoundID.MENU_Checkbox_Uncheck);
				}
			}
			else
			{
				if (base.MousePos.x >= 10f && base.MousePos.x <= _rectList.size.x - 30f)
				{
					if (_downward)
					{
						_listHover = Mathf.FloorToInt((base.MousePos.y + 10f) / 20f);
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
							goto IL_078d;
						}
						PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
					}
				}
				if (_listHover >= 0)
				{
					string text = "";
					if (_searchMode)
					{
						if (_listTop + _listHover < _searchList.Count)
						{
							text = _searchList[_listTop + _listHover].desc;
						}
					}
					else if (_listTop + _listHover < _itemList.Length)
					{
						text = _itemList[_listTop + _listHover].desc;
					}
					if (!string.IsNullOrEmpty(text))
					{
						ModdingMenu.instance.ShowDescription(text);
					}
				}
			}
		}
		else if ((Input.GetMouseButton(0) && !_mouseDown) || (_mouseDown && !Input.GetMouseButton(0)))
		{
			goto IL_078d;
		}
		held = _bumpScroll.held || _searchMode;
		_bumpList.Update();
		_bumpScroll.Update();
		return;
		IL_078d:
		held = false;
		_searchMode = false;
		_searchCursor.isVisible = false;
		_bumpScroll.held = false;
	}

	protected override void _NonMouseModeUpdate()
	{
		base._NonMouseModeUpdate();
	}

	protected internal override void NonMouseSetHeld(bool newHeld)
	{
		base.NonMouseSetHeld(newHeld);
		if (held && base.InScrollBox)
		{
			base.scrollBox.ScrollToRect(new Rect(base.PosX, base.PosY, base.size.x, base.size.y + _rectList.size.y));
		}
	}

	public static ushort GetLineCountFromHeight(float height)
	{
		return (ushort)Math.Max(1, Mathf.FloorToInt((height - 34f) / 20f));
	}

	internal override Vector2 _CenterPos()
	{
		Vector2 result = base._CenterPos();
		if (_downward)
		{
			result += new Vector2(0f, _rectList.size.y);
		}
		return result;
	}
}
