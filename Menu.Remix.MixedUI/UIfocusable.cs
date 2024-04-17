using UnityEngine;

namespace Menu.Remix.MixedUI;

public abstract class UIfocusable : UIelement
{
	public enum NextDirection
	{
		Left,
		Up,
		Right,
		Down,
		Back
	}

	public abstract class FocusableQueue : UIQueue
	{
		public readonly object sign;

		public OnEventHandler onChange;

		public OnHeldHandler onHeld;

		protected internal UIfocusable mainFocusable;

		protected FocusableQueue(object sign = null)
		{
			this.sign = sign;
		}
	}

	public bool greyedOut;

	private bool _lastGreyedOut;

	public object sign;

	protected internal bool mouseOverStopsScrollwheel;

	private bool _held;

	public BumpBehaviour bumpBehav { get; private set; }

	public UIfocusable[] NextFocusable { get; private set; } = new UIfocusable[5];


	protected internal virtual bool CurrentlyFocusableMouse => !greyedOut;

	protected internal virtual bool CurrentlyFocusableNonMouse => true;

	protected internal virtual Rect FocusRect
	{
		get
		{
			Rect result = (isRectangular ? new Rect(base.ScreenPos.x, base.ScreenPos.y, base.size.x, base.size.y) : new Rect(base.ScreenPos.x, base.ScreenPos.y, base.rad * 2f, base.rad * 2f));
			if (tab != null)
			{
				result.x += tab._container.x;
				result.y += tab._container.y;
			}
			return result;
		}
	}

	public bool Focused
	{
		get
		{
			if (!UIelement.ContextWrapped)
			{
				return ConfigContainer.FocusedElement == this;
			}
			if (wrapper != null)
			{
				return wrapper.Selected;
			}
			return false;
		}
	}

	protected internal virtual bool held
	{
		get
		{
			return _held;
		}
		set
		{
			if (_held == value)
			{
				return;
			}
			_held = value;
			this.OnHeld?.Invoke(_held);
			if (value)
			{
				if (UIelement.ContextWrapped)
				{
					wrapper.menu.selectedObject = wrapper;
				}
				else
				{
					ConfigContainer.instance._FocusNewElement(this);
				}
			}
			else if (!Focused)
			{
				return;
			}
			if (UIelement.ContextWrapped)
			{
				wrapper.tabWrapper.holdElement = value;
			}
			else
			{
				ConfigContainer.holdElement = value;
			}
		}
	}

	public event OnHeldHandler OnHeld;

	public event OnSignalHandler OnFocusGet;

	public event OnSignalHandler OnFocusLose;

	public UIfocusable(Vector2 pos, Vector2 size)
		: base(pos, size)
	{
		bumpBehav = new BumpBehaviour(this);
	}

	public UIfocusable(Vector2 pos, float rad)
		: base(pos, rad)
	{
		bumpBehav = new BumpBehaviour(this);
	}

	internal void _InvokeOnFocusGet()
	{
		this.OnFocusGet?.Invoke(this);
	}

	internal void _InvokeOnFocusLose()
	{
		this.OnFocusLose?.Invoke(this);
	}

	public void SetNextFocusable(NextDirection dir, UIfocusable candidate)
	{
		NextFocusable[(int)dir] = candidate;
	}

	public void SetNextFocusable(params UIfocusable[] candidates)
	{
		for (int i = 0; i < 5 && candidates.Length > i; i++)
		{
			NextFocusable[i] = candidates[i];
		}
	}

	public static void MutualHorizontalFocusableBind(UIfocusable left, UIfocusable right)
	{
		left.SetNextFocusable(NextDirection.Right, right);
		right.SetNextFocusable(NextDirection.Left, left);
	}

	public static void MutualVerticalFocusableBind(UIfocusable bottom, UIfocusable top)
	{
		bottom.SetNextFocusable(NextDirection.Up, top);
		top.SetNextFocusable(NextDirection.Down, bottom);
	}

	protected internal virtual void NonMouseSetHeld(bool newHeld)
	{
		held = newHeld;
		if (held && base.InScrollBox)
		{
			OpScrollBox.ScrollToChild(this);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
	}

	public override void Update()
	{
		base.Update();
		bumpBehav.Update();
		if (greyedOut)
		{
			held = false;
			if (!_lastGreyedOut)
			{
				Freeze();
			}
		}
		_lastGreyedOut = greyedOut;
		if (held && base.InScrollBox)
		{
			base.scrollBox.MarkDirty(0.5f);
			base.scrollBox.Update();
		}
	}

	protected internal override void Deactivate()
	{
		held = false;
		base.Deactivate();
	}

	protected internal void FocusMoveDisallow(UIfocusable _ = null)
	{
		if (UIelement.ContextWrapped)
		{
			if (wrapper != null && wrapper.tabWrapper != null)
			{
				wrapper.tabWrapper.allowFocusMove = false;
			}
		}
		else
		{
			ConfigContainer.instance._allowFocusMove = false;
		}
	}

	protected internal override void Freeze()
	{
		base.Freeze();
		NonMouseSetHeld(newHeld: false);
	}
}
