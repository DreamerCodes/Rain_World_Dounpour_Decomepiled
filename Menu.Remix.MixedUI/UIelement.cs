using System;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public abstract class UIelement
{
	public bool mute;

	public string description = "";

	public readonly bool isRectangular;

	protected Vector2 _pos;

	protected Vector2 _size;

	protected float _rad;

	protected Vector2? fixedSize;

	protected float? fixedRad;

	protected internal OpTab tab;

	protected internal FContainer myContainer;

	protected internal Vector2 lastScreenPos;

	protected internal UIelementWrapper wrapper;

	public float PosX
	{
		get
		{
			return GetPos().x;
		}
		set
		{
			SetPos(new Vector2(value, GetPos().y));
		}
	}

	public float PosY
	{
		get
		{
			return GetPos().y;
		}
		set
		{
			SetPos(new Vector2(GetPos().x, value));
		}
	}

	public Vector2 size
	{
		get
		{
			if (!isRectangular)
			{
				throw new InvalidGetPropertyException(this, "size");
			}
			return _size;
		}
		set
		{
			if (!isRectangular)
			{
				throw new InvalidActionException(this, "This Non-Rectangular item tried to set size which is Invalid!");
			}
			if (fixedSize.HasValue)
			{
				_size = new Vector2(Mathf.Max(value.x, 0f), Mathf.Max(value.y, 0f));
				if (fixedSize.Value.x > 0f)
				{
					_size.x = fixedSize.Value.x;
				}
				if (fixedSize.Value.y > 0f)
				{
					_size.y = fixedSize.Value.y;
				}
			}
			else if (_size != value)
			{
				_size = new Vector2(Mathf.Max(value.x, 0f), Mathf.Max(value.y, 0f));
			}
			Change();
		}
	}

	public float rad
	{
		get
		{
			if (isRectangular)
			{
				throw new InvalidGetPropertyException(this, "rad");
			}
			return _rad;
		}
		set
		{
			if (isRectangular)
			{
				throw new InvalidActionException(this, "This Rectangular item tried to set rad which is Invalid!");
			}
			if (fixedRad.HasValue)
			{
				_rad = fixedRad.Value;
				Change();
			}
			else if (_rad != value)
			{
				_rad = Mathf.Max(value, 0f);
				Change();
			}
		}
	}

	public bool Hidden { get; private set; }

	public bool IsInactive
	{
		get
		{
			if (Hidden)
			{
				return true;
			}
			if (tab != null && tab.isInactive)
			{
				return true;
			}
			if (InScrollBox && scrollBox.IsInactive)
			{
				return true;
			}
			return false;
		}
	}

	protected internal Vector2 pos
	{
		get
		{
			return _pos;
		}
		set
		{
			if (_pos != value)
			{
				_pos = value;
				Change();
			}
		}
	}

	protected Menu Menu
	{
		get
		{
			if (!ContextWrapped)
			{
				return ModdingMenu.instance;
			}
			if (wrapper == null)
			{
				return Custom.rainWorld.processManager.currentMainLoop as Menu;
			}
			return wrapper.menu;
		}
	}

	protected PositionedMenuObject Owner
	{
		get
		{
			if (ContextWrapped)
			{
				if (wrapper != null && wrapper.tabWrapper != null)
				{
					return wrapper.tabWrapper;
				}
				return null;
			}
			return ConfigContainer.instance;
		}
	}

	protected internal virtual bool MouseOver
	{
		get
		{
			if (isRectangular)
			{
				if (MousePos.x > 0f && MousePos.y > 0f && MousePos.x < size.x)
				{
					return MousePos.y < size.y;
				}
				return false;
			}
			return Custom.DistLess(new Vector2(rad, rad), MousePos, rad);
		}
	}

	protected internal Vector2 MousePos
	{
		get
		{
			if (Menu == null)
			{
				return -10000f * Vector2.one;
			}
			Vector2 result = new Vector2(Menu.mousePosition.x - ScreenPos.x, Menu.mousePosition.y - ScreenPos.y);
			if (InScrollBox && (scrollBox.MouseOver || (this is UIfocusable && (this as UIfocusable).held)))
			{
				result += scrollBox._camPos - (scrollBox.horizontal ? Vector2.right : Vector2.up) * scrollBox.scrollOffset - scrollBox.pos;
			}
			if (tab != null)
			{
				result -= new Vector2(tab._container.x, tab._container.y);
			}
			return result;
		}
	}

	protected internal OpScrollBox scrollBox { get; private set; }

	protected internal bool InScrollBox { get; private set; }

	protected internal Vector2 ScreenPos
	{
		get
		{
			if (Owner == null)
			{
				return _pos;
			}
			return Owner.ScreenPos + _pos;
		}
	}

	public static float frameMulti => Mathf.Max(1f, Time.smoothDeltaTime * 40f);

	public bool MenuMouseMode => Menu?.manager.menuesMouseMode ?? Custom.rainWorld.processManager.menuesMouseMode;

	public Player.InputPackage CtlrInput => Menu?.input ?? RWInput.PlayerUIInput(-1);

	public Player.InputPackage LastCtlrInput => Menu?.lastInput ?? RWInput.PlayerUIInput(-1);

	protected internal static bool ContextWrapped => ModdingMenu.instance == null;

	public event OnEventHandler OnChange;

	public event OnEventHandler OnUpdate;

	public event OnGrafUpdateHandler OnGrafUpdate;

	public event OnEventHandler OnReset;

	public event OnEventHandler OnDeactivate;

	public event OnEventHandler OnReactivate;

	public event OnEventHandler OnUnload;

	public UIelement(Vector2 pos, Vector2 size)
	{
		isRectangular = true;
		_pos = pos;
		_size = size;
		myContainer = new FContainer
		{
			x = ScreenPos.x + 0.01f,
			y = ScreenPos.y + 0.01f,
			scaleX = 1f,
			scaleY = 1f
		};
		Hidden = false;
		InScrollBox = false;
	}

	public UIelement(Vector2 pos, float rad)
	{
		isRectangular = false;
		_pos = pos;
		_rad = rad;
		myContainer = new FContainer
		{
			x = ScreenPos.x + 0.01f,
			y = ScreenPos.y + 0.01f,
			scaleX = 1f,
			scaleY = 1f
		};
		Hidden = false;
		InScrollBox = false;
	}

	public virtual void Reset()
	{
		this.OnReset?.Invoke();
	}

	public void Hide()
	{
		if (!Hidden)
		{
			Hidden = true;
			Deactivate();
		}
	}

	public void Show()
	{
		if (Hidden)
		{
			Hidden = false;
			Reactivate();
		}
	}

	public Vector2 GetPos()
	{
		if (InScrollBox)
		{
			return _pos - scrollBox._childOffset;
		}
		return _pos;
	}

	public void SetPos(Vector2 value)
	{
		if (InScrollBox)
		{
			if (_pos != value + scrollBox._childOffset)
			{
				_pos = value + scrollBox._childOffset;
				Change();
			}
		}
		else if (_pos != value)
		{
			_pos = value;
			Change();
		}
	}

	public void MoveToBack()
	{
		myContainer.MoveToBack();
	}

	public void MoveToFront()
	{
		myContainer.MoveToFront();
	}

	public void MoveInFrontOfElement(UIelement reference)
	{
		myContainer.MoveInFrontOfOtherNode(reference.myContainer);
	}

	public void MoveBehindElement(UIelement reference)
	{
		myContainer.MoveBehindOtherNode(reference.myContainer);
	}

	protected internal virtual void Change()
	{
		this.OnChange?.Invoke();
	}

	public virtual void Update()
	{
		lastScreenPos = ScreenPos;
		this.OnUpdate?.Invoke();
	}

	public virtual void GrafUpdate(float timeStacker)
	{
		myContainer.x = Mathf.Lerp(lastScreenPos.x, ScreenPos.x, timeStacker) + 0.01f;
		myContainer.y = Mathf.Lerp(lastScreenPos.y, ScreenPos.y, timeStacker) + 0.01f;
		this.OnGrafUpdate?.Invoke(timeStacker);
	}

	public void PlaySound(SoundID soundID)
	{
		if (mute)
		{
			return;
		}
		if (ContextWrapped)
		{
			if (wrapper != null)
			{
				wrapper.tabWrapper._PlaySound(soundID);
			}
		}
		else
		{
			ConfigContainer.PlaySound(soundID);
		}
	}

	public void PlaySound(SoundID soundID, float pan, float vol, float pitch)
	{
		if (mute)
		{
			return;
		}
		if (ContextWrapped)
		{
			if (wrapper != null)
			{
				wrapper.tabWrapper._PlaySound(soundID, pan, vol, pitch);
			}
		}
		else
		{
			ConfigContainer.PlaySound(soundID, pan, vol, pitch);
		}
	}

	protected internal virtual void Unload()
	{
		try
		{
			this.OnUnload?.Invoke();
		}
		catch (Exception msg)
		{
			MachineConnector.LogError(msg);
		}
		myContainer.RemoveAllChildren();
		myContainer.RemoveFromContainer();
	}

	public static int FrameMultiply(int origFrameCount)
	{
		return Mathf.RoundToInt((float)origFrameCount * frameMulti);
	}

	protected internal virtual string DisplayDescription()
	{
		return description;
	}

	protected internal virtual void Freeze()
	{
		GrafUpdate(0f);
	}

	protected internal static FLabel FLabelCreate(string text, bool bigText = false)
	{
		return new FLabel(LabelTest.GetFont(bigText), text)
		{
			color = MenuColorEffect.rgbMediumGrey,
			alignment = FLabelAlignment.Center
		};
	}

	protected static void FLabelPlaceAtCenter(FLabel label, Vector2 pos, Vector2 size)
	{
		FLabelPlaceAtCenter(label, pos.x, pos.y, size.x, size.y);
	}

	protected static void FLabelPlaceAtCenter(FLabel label, float offsetLeft, float offsetBottom, float width, float height)
	{
		label.alignment = FLabelAlignment.Center;
		label.x = offsetLeft + width / 2f;
		label.y = offsetBottom + height / 2f;
	}

	protected void ForceMenuMouseMode(bool? value)
	{
		if (!ContextWrapped)
		{
			ConfigContainer.ForceMenuMouseMode(value);
		}
		else if (wrapper != null)
		{
			wrapper.tabWrapper._forceMouseMode = value;
		}
	}

	protected internal virtual void Deactivate()
	{
		myContainer.isVisible = false;
		this.OnDeactivate?.Invoke();
	}

	protected internal virtual void Reactivate()
	{
		if (!Hidden)
		{
			myContainer.isVisible = true;
			this.OnReactivate?.Invoke();
		}
	}

	internal bool _AddToScrollBox(OpScrollBox scrollBox)
	{
		if (OpScrollBox.ChildBlacklist.Contains(GetType()))
		{
			MachineConnector.LogError(GetType().Name + " instances may not be added to a scrollbox!");
			return false;
		}
		if (InScrollBox)
		{
			MachineConnector.LogError("This item is already in an OpScrollBox! The later call is ignored.");
			return false;
		}
		InScrollBox = true;
		this.scrollBox = scrollBox;
		_pos += this.scrollBox._childOffset;
		Change();
		return true;
	}

	internal void _RemoveFromScrollBox()
	{
		if (!InScrollBox)
		{
			MachineConnector.LogError("This item is not in an OpScrollBox! This call will be ignored.");
			return;
		}
		if (scrollBox._lastFocusedElement == this)
		{
			scrollBox._lastFocusedElement = null;
			foreach (UIelement item in scrollBox.items)
			{
				if (item != this && item is UIfocusable)
				{
					scrollBox._lastFocusedElement = item as UIfocusable;
					break;
				}
			}
		}
		_pos -= scrollBox._childOffset;
		InScrollBox = false;
		scrollBox = null;
		Change();
	}

	internal void _SetTab(OpTab newTab)
	{
		if (tab != null && newTab != null)
		{
			tab.RemoveItems(this);
		}
		tab = newTab;
	}

	internal virtual Vector2 _CenterPos()
	{
		Vector2 result = ((!isRectangular) ? (ScreenPos + rad / 2f * Vector2.one) : (ScreenPos + size / 2f));
		if (tab != null)
		{
			result += tab._container.GetPosition();
		}
		if (InScrollBox)
		{
			Vector2 vector = scrollBox._camPos - (scrollBox.horizontal ? Vector2.right : Vector2.up) * scrollBox.scrollOffset - scrollBox.pos;
			result.x -= vector.x;
			result.y -= vector.y;
		}
		return result;
	}
}
