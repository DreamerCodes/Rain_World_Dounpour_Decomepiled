using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class BumpBehaviour
{
	public enum ButtonType
	{
		Jump,
		Map,
		Pickup,
		Throw,
		Pause
	}

	public readonly UIelement owner;

	public float sizeBump;

	public float extraSizeBump;

	public float col;

	public float flash;

	public float sin;

	public bool flashBool;

	private bool? _greyedOut;

	private bool? _held;

	private bool? _focused;

	private int _scrollInitCounter;

	private int _scrollCounter;

	public bool greyedOut
	{
		get
		{
			if (_greyedOut.HasValue)
			{
				return _greyedOut.Value;
			}
			if (owner is UIfocusable)
			{
				return (owner as UIfocusable).greyedOut;
			}
			return false;
		}
		set
		{
			_greyedOut = value;
		}
	}

	public bool held
	{
		get
		{
			if (_held.HasValue)
			{
				return _held.Value;
			}
			if (owner is UIfocusable)
			{
				return (owner as UIfocusable).held;
			}
			return false;
		}
		set
		{
			_held = value;
		}
	}

	public bool Focused
	{
		get
		{
			if (!_focused.HasValue)
			{
				if (owner is UIfocusable)
				{
					if (!owner.MenuMouseMode)
					{
						return (owner as UIfocusable).Focused;
					}
					return owner.MouseOver;
				}
				return owner.MouseOver;
			}
			return _focused.Value;
		}
		set
		{
			_focused = value;
		}
	}

	public float AddSize
	{
		get
		{
			if (!held && (!owner.MouseOver || !Input.GetMouseButton(0)))
			{
				return sizeBump + 0.5f * Mathf.Sin(extraSizeBump * 3.1416f);
			}
			return 0.5f * sizeBump;
		}
	}

	public float FillAlpha => Mathf.Lerp(0.3f, 0.6f, col);

	public BumpBehaviour(UIelement owner)
	{
		this.owner = owner;
		_greyedOut = null;
		_held = false;
	}

	public Color GetColor(Color orig)
	{
		if (greyedOut)
		{
			return MenuColorEffect.Greyscale(MenuColorEffect.MidToVeryDark(orig));
		}
		return Color.Lerp(orig, MenuColorEffect.rgbWhite, Mathf.Max(Mathf.Min(col, held ? 0.5f : 0.8f) / 2f, Mathf.Clamp01(flash)));
	}

	public void Update()
	{
		if (owner.IsInactive)
		{
			sin = 0f;
			extraSizeBump = 0f;
			return;
		}
		float num = 1f / UIelement.frameMulti;
		flash = Custom.LerpAndTick(flash, 0f, 0.03f, 0.16667f * num);
		if (Focused)
		{
			sizeBump = Custom.LerpAndTick(sizeBump, 1f, 0.1f, 0.1f * num);
			sin += 1f * num;
			if (!flashBool)
			{
				flashBool = true;
				flash = 1f;
			}
			if (!greyedOut)
			{
				col = Mathf.Min(1f, col + 0.1f * num);
				extraSizeBump = Mathf.Min(1f, extraSizeBump + 0.1f * num);
			}
		}
		else
		{
			flashBool = false;
			sizeBump = Custom.LerpAndTick(sizeBump, 0f, 0.1f, 0.05f * num);
			col = Mathf.Max(0f, col - 0.03333f * num);
			extraSizeBump = 0f;
		}
		if (!greyedOut)
		{
			if (owner.CtlrInput.y == 0 && owner.CtlrInput.x != 0 && owner.CtlrInput.x == owner.LastCtlrInput.x)
			{
				_scrollInitCounter++;
			}
			else if (owner.CtlrInput.x == 0 && owner.CtlrInput.y != 0 && owner.CtlrInput.y == owner.LastCtlrInput.y)
			{
				_scrollInitCounter++;
			}
			else
			{
				_scrollInitCounter = 0;
			}
			if (_scrollInitCounter > ModdingMenu.DASinit)
			{
				_scrollCounter++;
			}
			else
			{
				_scrollCounter = 0;
			}
		}
	}

	public float Sin(float period = 30f)
	{
		return 0.5f - 0.5f * Mathf.Sin(sin / period * 3.1416f);
	}

	public bool JoystickPress(IntVector2 direction)
	{
		if (direction.x != 0)
		{
			if (direction.x != owner.CtlrInput.x)
			{
				return false;
			}
			if (owner.CtlrInput.x == owner.LastCtlrInput.x)
			{
				return false;
			}
		}
		else if (owner.CtlrInput.x != 0)
		{
			return false;
		}
		if (direction.y != 0)
		{
			if (direction.y != owner.CtlrInput.y)
			{
				return false;
			}
			if (owner.CtlrInput.y == owner.LastCtlrInput.y)
			{
				return false;
			}
		}
		else if (owner.CtlrInput.y != 0)
		{
			return false;
		}
		return true;
	}

	public bool JoystickPress(int xDir, int yDir)
	{
		return JoystickPress(new IntVector2(xDir, yDir));
	}

	public int JoystickPressAxis(bool vertical)
	{
		if (vertical)
		{
			if (owner.CtlrInput.y != owner.LastCtlrInput.y)
			{
				return owner.CtlrInput.y;
			}
			return 0;
		}
		if (owner.CtlrInput.x != owner.LastCtlrInput.x)
		{
			return owner.CtlrInput.x;
		}
		return 0;
	}

	public bool JoystickHeld(IntVector2 direction, float speed = 1f)
	{
		if (speed > 0f && _scrollCounter < Mathf.CeilToInt((float)ModdingMenu.DASdelay / speed))
		{
			return false;
		}
		if (direction.x != 0)
		{
			if (owner.CtlrInput.x != direction.x)
			{
				return false;
			}
		}
		else if (owner.CtlrInput.x != 0)
		{
			return false;
		}
		if (direction.y != 0)
		{
			if (owner.CtlrInput.y != direction.y)
			{
				return false;
			}
		}
		else if (owner.CtlrInput.y != 0)
		{
			return false;
		}
		_scrollCounter = 0;
		return true;
	}

	public bool JoystickHeld(int xDir, int yDir, float speed = 1f)
	{
		return JoystickHeld(new IntVector2(xDir, yDir), speed);
	}

	public int JoystickHeldAxis(bool vertical, float speed = 1f)
	{
		if (speed > 0f && _scrollCounter < Mathf.CeilToInt((float)ModdingMenu.DASdelay / speed))
		{
			return 0;
		}
		if (vertical && owner.CtlrInput.y != 0)
		{
			_scrollCounter = 0;
			return owner.CtlrInput.y;
		}
		if (!vertical && owner.CtlrInput.x != 0)
		{
			_scrollCounter = 0;
			return owner.CtlrInput.x;
		}
		return 0;
	}

	public bool ButtonPress(ButtonType type)
	{
		switch (type)
		{
		default:
			if (owner.CtlrInput.jmp)
			{
				return !owner.LastCtlrInput.jmp;
			}
			return false;
		case ButtonType.Map:
			if (owner.CtlrInput.mp)
			{
				return !owner.LastCtlrInput.mp;
			}
			return false;
		case ButtonType.Pickup:
			if (owner.CtlrInput.pckp)
			{
				return !owner.LastCtlrInput.pckp;
			}
			return false;
		case ButtonType.Throw:
			if (owner.CtlrInput.thrw)
			{
				return !owner.LastCtlrInput.thrw;
			}
			return false;
		case ButtonType.Pause:
			return RWInput.CheckPauseButton(0);
		}
	}
}
