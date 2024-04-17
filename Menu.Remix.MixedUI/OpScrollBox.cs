using System;
using System.Collections.Generic;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpScrollBox : UIfocusable, IEquatable<OpScrollBox>, IHoldUIelements
{
	[Flags]
	public enum RedrawEvents : short
	{
		Never = 0,
		Always = 1,
		OnHover = 2,
		OnKeypress = 4
	}

	protected static readonly List<Camera> _cameras = new List<Camera>();

	protected internal readonly Vector2 _childOffset;

	public readonly bool horizontal;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	public float fillAlpha = 0.3f;

	public float targetScrollOffset;

	public RedrawEvents redrawFlags = RedrawEvents.Always;

	private bool _contentsDirty = true;

	private float _dirtyUntil = -1f;

	protected readonly Camera _cam;

	protected readonly int _camIndex;

	protected internal readonly Vector2 _camPos;

	private float _scrollVel;

	protected FTexture _insideTexture;

	protected RenderTexture _rt;

	protected readonly DyeableRect _rectBack;

	protected readonly DyeableRect _rectSlidebar;

	private bool _draggingSlider;

	protected const float MAXCONTENTSIZE = 10000f;

	protected readonly bool _isTab;

	protected bool _hasScrolled;

	protected FLabel _labelNotify;

	public bool doesBackBump;

	public static readonly Type[] ChildBlacklist = new Type[1] { typeof(OpScrollBox) };

	internal UIfocusable _lastFocusedElement;

	protected bool _firstUpdate = true;

	protected float _dragOffset;

	protected bool _lastMouseOver;

	protected bool _lastMouseDown;

	protected float _lastScrollOffset = 1f;

	protected readonly BumpBehaviour _bumpSlidebar;

	protected bool _scrollMouseOver;

	protected float _notifySin;

	protected int _scrollSoundTick;

	public HashSet<UIelement> items { get; protected set; }

	public bool IsTab => false;

	public Vector2 CanvasSize
	{
		get
		{
			if (!horizontal)
			{
				return new Vector2(base.size.x, contentSize);
			}
			return new Vector2(contentSize, base.size.y);
		}
	}

	public float ScrollOffset
	{
		get
		{
			return scrollOffset;
		}
		set
		{
			_scrollVel = 0f;
			scrollOffset = (targetScrollOffset = value);
		}
	}

	public float MaxScroll => 0f - Mathf.Max(contentSize - base.size.y, 0f);

	public float contentSize { get; private set; }

	public bool ScrollLocked { get; private set; }

	public float scrollOffset { get; protected set; }

	protected float _ScrollSize
	{
		get
		{
			if (!horizontal)
			{
				return base.size.y;
			}
			return base.size.x;
		}
	}

	protected Vector2 _SliderSize
	{
		get
		{
			if (!horizontal)
			{
				return new Vector2(15f, Mathf.Max(Mathf.Min(base.size.y, base.size.y * base.size.y / contentSize), 20f));
			}
			return new Vector2(Mathf.Max(Mathf.Min(base.size.x, base.size.x * base.size.x / contentSize), 20f), 15f);
		}
	}

	protected Vector2 _SliderPos
	{
		get
		{
			if (!horizontal)
			{
				return new Vector2((_isTab ? 15f : 0f) + base.size.x - 20f, (0f - scrollOffset) * base.size.y / contentSize);
			}
			return new Vector2((0f - scrollOffset) * base.size.x / contentSize, _isTab ? (-10f) : 5f);
		}
	}

	protected internal override Rect FocusRect
	{
		get
		{
			Rect result = new Rect(base.ScreenPos.x, base.ScreenPos.y, base.size.x, base.size.y);
			if (_isTab)
			{
				result.x += 10f;
				result.y += (horizontal ? 0f : 10f);
				result.width -= (horizontal ? 20f : 10f);
				result.height -= (horizontal ? 10f : 20f);
			}
			if (tab != null)
			{
				result.x += tab._container.x;
				result.y += tab._container.y;
			}
			return result;
		}
	}

	protected internal override bool CurrentlyFocusableMouse => false;

	protected internal override bool CurrentlyFocusableNonMouse => !ScrollLocked;

	protected internal override bool MouseOver
	{
		get
		{
			if (!_isTab)
			{
				return base.MouseOver;
			}
			if (base.MousePos.x > -15f && base.MousePos.x < 615f && base.MousePos.y > -15f)
			{
				return base.MousePos.y < 615f;
			}
			return false;
		}
	}

	public bool SetContentSize(float newSize, bool sortToTop = true)
	{
		float num = Mathf.Clamp(newSize, horizontal ? base.size.x : base.size.y, 10000f);
		if (Mathf.Approximately(contentSize, num))
		{
			return false;
		}
		if (sortToTop)
		{
			float num2 = num - contentSize;
			foreach (UIelement item in items)
			{
				item.SetPos(item.GetPos() + (horizontal ? new Vector2(num2, 0f) : new Vector2(0f, num2)));
			}
			contentSize = num;
			targetScrollOffset += num2;
			scrollOffset = targetScrollOffset;
		}
		else
		{
			contentSize = num;
		}
		_hasScrolled = false;
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is OpScrollBox)
		{
			return _camIndex == (obj as OpScrollBox)._camIndex;
		}
		return false;
	}

	public OpScrollBox(Vector2 pos, Vector2 size, float contentSize, bool horizontal = false, bool hasBack = true, bool hasSlideBar = true)
		: base(pos, size)
	{
		items = new HashSet<UIelement>();
		_size.x = Mathf.Min(_size.x, 800f);
		_size.y = Mathf.Min(_size.y, 800f);
		this.horizontal = horizontal;
		this.contentSize = Mathf.Clamp(contentSize, horizontal ? size.x : size.y, 10000f);
		_isTab = false;
		_hasScrolled = false;
		doesBackBump = true;
		GameObject gameObject = new GameObject("OpScrollBox Camera " + _camIndex);
		_cam = gameObject.AddComponent<Camera>();
		_camIndex = -1;
		int i = 0;
		for (int count = _cameras.Count; i < count; i++)
		{
			if (_cameras[i] == null)
			{
				_camIndex = i;
				_cameras[i] = _cam;
				break;
			}
		}
		if (_camIndex == -1)
		{
			_cameras.Add(_cam);
			_camIndex = _cameras.Count - 1;
		}
		gameObject.name = "OpScrollBox Camera " + _camIndex;
		_camPos = (horizontal ? new Vector2(10000f, -10000f - 10300f * (float)_camIndex) : new Vector2(10000f + 10300f * (float)_camIndex, 10000f));
		_childOffset = _camPos;
		if (hasBack)
		{
			_rectBack = new DyeableRect(myContainer, Vector2.zero, size)
			{
				colorEdge = colorEdge
			};
		}
		if (hasSlideBar)
		{
			_bumpSlidebar = new BumpBehaviour(this);
			_rectSlidebar = new DyeableRect(myContainer, Vector2.zero, _SliderSize)
			{
				colorEdge = colorEdge,
				colorFill = Color.Lerp(colorEdge, colorFill, 0.5f),
				fillAlpha = 0.5f
			};
		}
		ScrollToTop();
		Change();
		if (!UIelement.ContextWrapped)
		{
			GrafUpdate(0f);
		}
	}

	public OpScrollBox(OpTab tab, float contentSize, bool horizontal = false, bool hasSlideBar = true)
		: this(Vector2.zero, new Vector2(600f, 600f), contentSize, horizontal, hasBack: false, hasSlideBar)
	{
		tab.AddItems(this);
		_isTab = true;
		_labelNotify = UIelement.FLabelCreate(">>> " + _TutorialText() + " <<<");
		UIelement.FLabelPlaceAtCenter(_labelNotify, 200f, horizontal ? 10f : 0f, 200f, 20f);
		myContainer.AddChild(_labelNotify);
	}

	protected internal override string DisplayDescription()
	{
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		if (!ScrollLocked)
		{
			return _TutorialText();
		}
		return "";
	}

	protected string _TutorialText()
	{
		return OptionalText.GetText((!base.MenuMouseMode) ? OptionalText.ID.OpScrollBox_NonMouseTuto : ((_rectSlidebar != null) ? OptionalText.ID.OpScrollBox_MouseTutoSlidebar : OptionalText.ID.OpScrollBox_MouseTuto));
	}

	public void MarkDirty()
	{
		_contentsDirty = true;
	}

	public void MarkDirty(float time)
	{
		MarkDirty();
		_dirtyUntil = Mathf.Max(_dirtyUntil, Time.unscaledTime + time);
	}

	public void Lock(bool stopImmediately)
	{
		ScrollLocked = true;
		if (stopImmediately)
		{
			targetScrollOffset = scrollOffset;
			_scrollVel = 0f;
		}
	}

	public void Unlock()
	{
		ScrollLocked = false;
	}

	public void ScrollToTop(bool immediate = true)
	{
		targetScrollOffset = (horizontal ? 0f : MaxScroll);
		if (immediate)
		{
			scrollOffset = targetScrollOffset;
		}
	}

	public void ScrollToBottom(bool immediate = true)
	{
		targetScrollOffset = (horizontal ? MaxScroll : 0f);
		if (immediate)
		{
			scrollOffset = targetScrollOffset;
		}
	}

	public static void ScrollToChild(UIelement child, bool silent = false)
	{
		if (!child.InScrollBox)
		{
			return;
		}
		OpScrollBox opScrollBox = child.scrollBox;
		if (opScrollBox.ScrollLocked)
		{
			return;
		}
		if (child is UIfocusable)
		{
			UIfocusable uIfocusable = child as UIfocusable;
			if (opScrollBox.horizontal)
			{
				float num = opScrollBox._camPos.x - uIfocusable.FocusRect.x + child.tab._container.x;
				if (num > opScrollBox.scrollOffset)
				{
					opScrollBox.targetScrollOffset = num + 10f;
				}
				else
				{
					num -= uIfocusable.FocusRect.width;
					if (num < opScrollBox.scrollOffset - opScrollBox.size.x)
					{
						opScrollBox.targetScrollOffset = num - 10f + opScrollBox.size.x;
					}
				}
			}
			else
			{
				float num = opScrollBox._camPos.y - uIfocusable.FocusRect.y + child.tab._container.y;
				if (num > opScrollBox.scrollOffset)
				{
					opScrollBox.targetScrollOffset = num + 10f;
				}
				else
				{
					num -= uIfocusable.FocusRect.height;
					if (num < opScrollBox.scrollOffset - opScrollBox.size.y)
					{
						opScrollBox.targetScrollOffset = num - 10f + opScrollBox.size.y;
					}
				}
			}
		}
		else if (opScrollBox.horizontal)
		{
			float num2 = 0f - child.GetPos().x;
			if (num2 > opScrollBox.scrollOffset)
			{
				opScrollBox.targetScrollOffset = num2 + 10f;
			}
			else
			{
				num2 -= child.size.x;
				if (num2 < opScrollBox.scrollOffset - opScrollBox.size.x)
				{
					opScrollBox.targetScrollOffset = num2 - 10f + opScrollBox.size.x;
				}
			}
		}
		else
		{
			float num2 = 0f - child.GetPos().y;
			if (num2 > opScrollBox.scrollOffset)
			{
				opScrollBox.targetScrollOffset = num2 + 10f;
			}
			else
			{
				num2 -= child.size.y;
				if (num2 < opScrollBox.scrollOffset - opScrollBox.size.y)
				{
					opScrollBox.targetScrollOffset = num2 - 10f + opScrollBox.size.y;
				}
			}
		}
		opScrollBox.targetScrollOffset = Mathf.Clamp(opScrollBox.targetScrollOffset, opScrollBox.MaxScroll, 0f);
		if (!Mathf.Approximately(opScrollBox.scrollOffset, opScrollBox.targetScrollOffset))
		{
			opScrollBox._hasScrolled = true;
			opScrollBox.MarkDirty(0.5f);
			if (!silent)
			{
				opScrollBox.PlaySound(SoundID.MENU_First_Scroll_Tick);
			}
		}
	}

	public void ScrollToRect(Rect rect, bool silent = false)
	{
		if (ScrollLocked)
		{
			return;
		}
		if (horizontal)
		{
			float num = 0f - rect.x;
			if (num > scrollOffset)
			{
				targetScrollOffset = num + 10f;
			}
			else
			{
				num -= rect.width;
				if (num < scrollOffset - base.size.x)
				{
					targetScrollOffset = num - 10f + base.size.x;
				}
			}
		}
		else
		{
			float num = 0f - rect.y;
			if (num > scrollOffset)
			{
				targetScrollOffset = num + 10f;
			}
			else
			{
				num -= rect.height;
				if (num < scrollOffset - base.size.y)
				{
					targetScrollOffset = num - 10f + base.size.y;
				}
			}
		}
		targetScrollOffset = Mathf.Clamp(targetScrollOffset, MaxScroll, 0f);
		if (!Mathf.Approximately(scrollOffset, targetScrollOffset))
		{
			_hasScrolled = true;
			MarkDirty(0.5f);
			if (!silent)
			{
				PlaySound(SoundID.MENU_First_Scroll_Tick);
			}
		}
	}

	public static bool IsChildVisible(UIelement child)
	{
		if (!child.InScrollBox)
		{
			return true;
		}
		if (child.scrollBox.horizontal)
		{
			if (child.scrollBox._camPos.x - child._CenterPos().x > child.scrollBox.scrollOffset)
			{
				return child.scrollBox._camPos.x - child._CenterPos().x < child.scrollBox.scrollOffset - child.scrollBox.size.x;
			}
			return false;
		}
		if (child.scrollBox._camPos.y - child._CenterPos().y > child.scrollBox.scrollOffset)
		{
			return child.scrollBox._camPos.y - child._CenterPos().y < child.scrollBox.scrollOffset - child.scrollBox.size.y;
		}
		return false;
	}

	public void AddItems(params UIelement[] items)
	{
		if (tab == null)
		{
			throw new InvalidOperationException("OpScrollBox must be added to an OpTab before items are added.");
		}
		foreach (UIelement uIelement in items)
		{
			if (uIelement._AddToScrollBox(this))
			{
				tab.AddItems(uIelement);
				this.items.Add(uIelement);
				if (_lastFocusedElement == null && uIelement is UIfocusable)
				{
					_lastFocusedElement = uIelement as UIfocusable;
				}
			}
		}
	}

	public UIelementWrapper AddItemToWrapped(UIelement item)
	{
		if (wrapper == null)
		{
			throw new InvalidOperationException("OpScrollBox must be added to an UIelementWrapper before an item is added.");
		}
		if (item._AddToScrollBox(this))
		{
			tab.AddItems(item);
			items.Add(item);
			if (_lastFocusedElement == null && item is UIfocusable)
			{
				_lastFocusedElement = item as UIfocusable;
			}
		}
		UIelementWrapper uIelementWrapper = new UIelementWrapper(wrapper.tabWrapper, item);
		for (int i = 0; i < uIelementWrapper.nextSelectable.Length; i++)
		{
			uIelementWrapper.nextSelectable[i] = uIelementWrapper;
		}
		return uIelementWrapper;
	}

	public static void RemoveItemsFromScrollBox(params UIelement[] items)
	{
		foreach (UIelement uIelement in items)
		{
			if (uIelement.InScrollBox)
			{
				uIelement.scrollBox.items.Remove(uIelement);
				uIelement._RemoveFromScrollBox();
			}
		}
	}

	protected bool _IsThereChildMouseOver()
	{
		foreach (UIelement item in items)
		{
			if (!item.IsInactive && item is UIfocusable && (item as UIfocusable).CurrentlyFocusableMouse && item.MouseOver && (item as UIfocusable).mouseOverStopsScrollwheel)
			{
				return true;
			}
		}
		return false;
	}

	protected internal override void Change()
	{
		_size.x = Mathf.Min(_size.x, 800f);
		_size.y = Mathf.Min(_size.y, 800f);
		base.Change();
		_UpdateCam();
	}

	public override void Update()
	{
		if (_rectBack != null)
		{
			_rectBack.Update();
		}
		if (_rectSlidebar != null)
		{
			_rectSlidebar.Update();
		}
		if (!_hasScrolled && !ScrollLocked && !greyedOut)
		{
			_notifySin += 1f;
		}
		if (_labelNotify != null && !ScrollLocked && !greyedOut && _hasScrolled)
		{
			_labelNotify.alpha -= 0.03333f / UIelement.frameMulti;
			if (_labelNotify.alpha < float.Epsilon)
			{
				_labelNotify.isVisible = false;
				_labelNotify.RemoveFromContainer();
				_labelNotify = null;
			}
		}
		if ((redrawFlags & RedrawEvents.Always) != 0)
		{
			_contentsDirty = true;
		}
		else if ((redrawFlags & RedrawEvents.OnHover) != 0 && (MouseOver || _lastMouseOver))
		{
			MarkDirty(0.5f);
		}
		else if (Time.unscaledTime <= _dirtyUntil)
		{
			_contentsDirty = true;
		}
		else if ((redrawFlags & RedrawEvents.OnKeypress) != 0 && Input.anyKey)
		{
			MarkDirty(0.5f);
		}
		bool flag = false;
		if (base.MenuMouseMode)
		{
			_lastMouseOver = MouseOver;
			_scrollMouseOver = false;
			if (_draggingSlider)
			{
				if (ScrollLocked || greyedOut)
				{
					_draggingSlider = false;
					held = false;
				}
				else
				{
					float num = ((horizontal ? base.MousePos.x : base.MousePos.y) + _dragOffset) / _ScrollSize;
					num *= 0f - contentSize;
					scrollOffset = Mathf.Clamp(num, MaxScroll, 0f);
					if (!Mathf.Approximately(targetScrollOffset, scrollOffset))
					{
						flag = true;
						_hasScrolled = true;
						targetScrollOffset = scrollOffset;
						PlaySound(SoundID.MENU_Scroll_Tick);
					}
					if (!Input.GetMouseButton(0))
					{
						_draggingSlider = false;
						held = false;
					}
				}
			}
			else if (_rectSlidebar != null && !ScrollLocked && !greyedOut && base.MousePos.x > _SliderPos.x && base.MousePos.x < _SliderPos.x + _SliderSize.x && base.MousePos.y > _SliderPos.y && base.MousePos.y < _SliderPos.y + _SliderSize.y)
			{
				_scrollMouseOver = true;
				if (Input.GetMouseButton(0) && !_lastMouseDown)
				{
					_dragOffset = (horizontal ? (_SliderPos.x - base.MousePos.x) : (_SliderPos.y - base.MousePos.y));
					_draggingSlider = true;
					_hasScrolled = true;
					if (!base.Focused && !UIelement.ContextWrapped)
					{
						ConfigContainer.instance._FocusNewElement(this, silent: true);
					}
					held = true;
					PlaySound(SoundID.MENU_First_Scroll_Tick);
				}
			}
			_lastMouseDown = Input.GetMouseButton(0);
			if (!ScrollLocked && !greyedOut && base.Menu.mouseScrollWheelMovement != 0 && MouseOver && !_IsThereChildMouseOver() && !_draggingSlider)
			{
				targetScrollOffset = Mathf.Clamp(targetScrollOffset - (horizontal ? 40f : (-40f)) * (float)base.Menu.mouseScrollWheelMovement, MaxScroll, 0f);
				if (_bumpSlidebar != null)
				{
					_bumpSlidebar.flash = Mathf.Lerp(_bumpSlidebar.flash, 1f, 0.4f);
					_bumpSlidebar.sizeBump = Mathf.Lerp(_bumpSlidebar.sizeBump, 2.5f, 0.4f);
				}
				if (!Mathf.Approximately(targetScrollOffset, scrollOffset))
				{
					flag = true;
					_hasScrolled = true;
				}
				PlaySound(flag ? SoundID.MENU_Scroll_Tick : SoundID.MENU_First_Scroll_Tick);
			}
		}
		else if (held)
		{
			if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Throw))
			{
				held = false;
				FocusMoveDisallow(this);
			}
			else if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump))
			{
				if (_lastFocusedElement != null && !_lastFocusedElement.IsInactive && _lastFocusedElement.CurrentlyFocusableNonMouse && IsChildVisible(_lastFocusedElement))
				{
					held = false;
					if (UIelement.ContextWrapped)
					{
						if (_lastFocusedElement.wrapper != null)
						{
							wrapper.menu.selectedObject = _lastFocusedElement.wrapper;
						}
					}
					else
					{
						ConfigContainer.instance._FocusNewElement(_lastFocusedElement);
					}
					ScrollToChild(_lastFocusedElement);
				}
				else
				{
					float num2 = float.MaxValue;
					UIfocusable uIfocusable = null;
					foreach (UIelement item in items)
					{
						if (item.IsInactive || !(item is UIfocusable))
						{
							continue;
						}
						UIfocusable uIfocusable2 = item as UIfocusable;
						if (uIfocusable2.CurrentlyFocusableNonMouse)
						{
							float num3 = Mathf.Abs(horizontal ? (_camPos.x - _lastFocusedElement._CenterPos().x - scrollOffset - base.size.x / 2f) : (_camPos.y - _lastFocusedElement._CenterPos().y - scrollOffset + base.size.y / 2f));
							if (num2 > num3)
							{
								num2 = num3;
								uIfocusable = uIfocusable2;
							}
						}
					}
					if (uIfocusable != null)
					{
						held = false;
						if (UIelement.ContextWrapped)
						{
							if (uIfocusable.wrapper != null)
							{
								wrapper.menu.selectedObject = uIfocusable.wrapper;
							}
						}
						else
						{
							ConfigContainer.instance._FocusNewElement(uIfocusable);
						}
						ScrollToChild(uIfocusable);
					}
					else
					{
						PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
					}
				}
			}
			else if (!ScrollLocked && !greyedOut)
			{
				if ((horizontal && base.CtlrInput.x != 0) || (!horizontal && base.CtlrInput.y != 0))
				{
					float num4 = (_isTab ? 40f : 25f);
					if (horizontal)
					{
						targetScrollOffset -= num4 * Mathf.Sign(base.CtlrInput.x);
					}
					else
					{
						targetScrollOffset -= num4 * Mathf.Sign(base.CtlrInput.y);
					}
					if (!Mathf.Approximately(targetScrollOffset, scrollOffset))
					{
						flag = true;
						_hasScrolled = true;
						if (_scrollSoundTick % ModdingMenu.DASdelay == 0)
						{
							PlaySound((_scrollSoundTick == 0) ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
						}
						_scrollSoundTick++;
						if (_bumpSlidebar != null)
						{
							_bumpSlidebar.flash = Mathf.Min(1f, _bumpSlidebar.flash + 0.2f);
							_bumpSlidebar.sizeBump = Mathf.Min(2.5f, _bumpSlidebar.sizeBump + 0.3f);
						}
					}
				}
				else
				{
					_scrollSoundTick = 0;
				}
			}
		}
		scrollOffset = Mathf.SmoothDamp(scrollOffset, targetScrollOffset, ref _scrollVel, 0.15f * UIelement.frameMulti);
		if (Mathf.Abs(scrollOffset - targetScrollOffset) < 0.5f)
		{
			scrollOffset = targetScrollOffset;
			_scrollVel = 0f;
		}
		targetScrollOffset = Mathf.Clamp(targetScrollOffset, MaxScroll, 0f);
		scrollOffset = Mathf.Clamp(scrollOffset, MaxScroll, 0f);
		_MoveCam();
		if (flag || _firstUpdate)
		{
			_firstUpdate = false;
			Change();
		}
		base.Update();
		if (!Mathf.Approximately(_lastScrollOffset, scrollOffset))
		{
			_lastScrollOffset = scrollOffset;
			_contentsDirty = true;
		}
		_cam.enabled = _contentsDirty;
		_contentsDirty = false;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = 1f;
		if (!_hasScrolled && !ScrollLocked && !greyedOut)
		{
			num = 0.5f - 0.5f * Mathf.Sin(_notifySin / 30f * (float)Math.PI);
		}
		if (_rectBack != null)
		{
			_rectBack.GrafUpdate(timeStacker);
			_rectBack.colorFill = colorFill;
			_rectBack.colorEdge = (doesBackBump ? base.bumpBehav.GetColor(colorEdge) : colorEdge);
			_rectBack.fillAlpha = fillAlpha;
			_rectBack.addSize = (doesBackBump ? (new Vector2(4f, 4f) * base.bumpBehav.AddSize) : Vector2.zero);
			_rectBack.addSize += new Vector2(2f, 2f);
			_rectBack.size = base.size;
		}
		if (_rectSlidebar != null)
		{
			_bumpSlidebar.Focused = base.Focused || _scrollMouseOver;
			_bumpSlidebar.greyedOut = ScrollLocked || greyedOut;
			_bumpSlidebar.Update();
			if (_draggingSlider)
			{
				_rectSlidebar.colorFill = _bumpSlidebar.GetColor(colorEdge);
				_rectSlidebar.fillAlpha = 1f;
			}
			else if (_hasScrolled || ScrollLocked || greyedOut)
			{
				_rectSlidebar.colorFill = _bumpSlidebar.GetColor(colorFill);
				_rectSlidebar.fillAlpha = _bumpSlidebar.FillAlpha;
			}
			else
			{
				_rectSlidebar.colorFill = _bumpSlidebar.GetColor(colorEdge);
				_rectSlidebar.fillAlpha = 0.3f + 0.6f * num;
			}
			_rectSlidebar.colorEdge = _bumpSlidebar.GetColor(colorEdge);
			_rectSlidebar.size = _SliderSize;
			_rectSlidebar.addSize = new Vector2(2f, 2f) * _bumpSlidebar.AddSize;
			_rectSlidebar.pos = _SliderPos;
			_rectSlidebar.GrafUpdate(timeStacker);
		}
		if (_labelNotify == null)
		{
			return;
		}
		if (ScrollLocked || greyedOut)
		{
			_labelNotify.alpha = 0f;
			return;
		}
		_labelNotify.text = ">>> " + _TutorialText() + " <<<";
		_labelNotify.color = Color.Lerp(Color.white, _bumpSlidebar.GetColor(colorEdge), 0.5f);
		if (!_hasScrolled)
		{
			_labelNotify.alpha = 0.5f + 0.5f * num;
		}
	}

	protected internal override void Deactivate()
	{
		base.Deactivate();
		if (_firstUpdate)
		{
			Update();
			GrafUpdate(0f);
			foreach (UIelement item in items)
			{
				item.Update();
				item.GrafUpdate(0f);
			}
		}
		_insideTexture.isVisible = false;
		foreach (UIelement item2 in items)
		{
			item2.Deactivate();
		}
		_cam.gameObject.SetActive(value: false);
	}

	protected internal override void Reactivate()
	{
		base.Reactivate();
		if (base.Hidden)
		{
			return;
		}
		_insideTexture.isVisible = true;
		foreach (UIelement item in items)
		{
			item.Reactivate();
		}
		_cam.gameObject.SetActive(value: true);
	}

	protected internal override void Unload()
	{
		base.Unload();
		if ((bool)_cam)
		{
			UnityEngine.Object.Destroy(_cam.gameObject);
		}
		if (_insideTexture != null)
		{
			_insideTexture.Destroy();
		}
	}

	protected void _UpdateCam()
	{
		if (greyedOut)
		{
			return;
		}
		_cam.aspect = base.size.x / base.size.y;
		_cam.orthographic = true;
		_cam.orthographicSize = base.size.y / 2f;
		_cam.nearClipPlane = 1f;
		_cam.farClipPlane = 100f;
		_MoveCam();
		_cam.depth = -1000f;
		if (!(_rt == null) && Mathf.Approximately(_rt.width, base.size.x) && Mathf.Approximately(_rt.height, base.size.y))
		{
			return;
		}
		if (_rt != null)
		{
			_rt.Release();
		}
		_rt = new RenderTexture((int)base.size.x, (int)base.size.y, 8)
		{
			filterMode = FilterMode.Point
		};
		_cam.targetTexture = _rt;
		if (_insideTexture == null)
		{
			_insideTexture = new FTexture(_rt, "sb" + _camIndex)
			{
				anchorX = 0f,
				anchorY = 0f,
				x = 0f,
				y = 0f
			};
			myContainer.AddChild(_insideTexture);
			if (_rectBack != null)
			{
				_insideTexture.MoveBehindOtherNode(_rectBack.sprites[_rectBack.SideSprite(0)]);
			}
		}
		else
		{
			_insideTexture.SetTexture(_rt);
		}
	}

	protected void _MoveCam()
	{
		Vector3 vector = (Vector3)_camPos + new Vector3(base.size.x / 2f, base.size.y / 2f, -50f) + (horizontal ? Vector3.left : Vector3.down) * scrollOffset;
		if (base.Owner != null)
		{
			vector += (Vector3)base.Owner.ScreenPos;
		}
		if (tab != null)
		{
			vector += (Vector3)tab._container.GetPosition();
		}
		_cam.gameObject.transform.position = new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
	}

	public bool Equals(OpScrollBox other)
	{
		if (other != null)
		{
			return EqualityComparer<Camera>.Default.Equals(_cam, other._cam);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return -1513707931 + EqualityComparer<Camera>.Default.GetHashCode(_cam);
	}

	public static bool operator ==(OpScrollBox left, OpScrollBox right)
	{
		return EqualityComparer<OpScrollBox>.Default.Equals(left, right);
	}

	public static bool operator !=(OpScrollBox left, OpScrollBox right)
	{
		return !(left == right);
	}
}
