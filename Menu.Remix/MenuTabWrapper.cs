using System.Collections.Generic;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu.Remix;

public class MenuTabWrapper : PositionedMenuObject
{
	internal readonly FContainer _glowContainer;

	internal readonly WrappedMenuTab _tab;

	internal bool? _forceMouseMode;

	private bool _lastMenuMouseMode;

	private bool _lastHalt;

	internal Dictionary<UIelement, UIelementWrapper> wrappers;

	internal bool _lastHoldElement;

	public bool allowFocusMove = true;

	private int _soundFill;

	public bool holdElement { get; internal set; }

	private bool _soundFilled
	{
		get
		{
			if (_soundFill <= UIelement.FrameMultiply(200))
			{
				return mute;
			}
			return true;
		}
	}

	public bool mute { get; internal set; }

	public MenuTabWrapper(Menu menu, MenuObject owner)
		: base(menu, owner, Vector2.zero)
	{
		_soundFill = 0;
		myContainer = new FContainer();
		owner.Container.AddChild(myContainer);
		_glowContainer = new FContainer
		{
			x = 0f - Menu.HorizontalMoveToGetCentered(menu.manager)
		};
		myContainer.AddChild(_glowContainer);
		wrappers = new Dictionary<UIelement, UIelementWrapper>();
		_lastMenuMouseMode = menu.manager.menuesMouseMode;
		_tab = new WrappedMenuTab(this);
		OpKeyBinder._InitWrapped(this);
		_tab._Update();
		_tab._GrafUpdate(0f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		_tab._GrafUpdate(timeStacker);
	}

	public override void Update()
	{
		_soundFill = ((_soundFill > 0) ? (_soundFill - 1) : 0);
		_lastHoldElement = holdElement;
		allowFocusMove = !holdElement;
		if (menu.GetFreezeMenuFunctions())
		{
			if (!_lastHalt)
			{
				foreach (UIelement item in _tab.items)
				{
					item.Freeze();
				}
			}
			_lastHalt = true;
			return;
		}
		_lastHalt = false;
		if (_forceMouseMode.HasValue)
		{
			menu.manager.menuesMouseMode = _forceMouseMode.Value;
		}
		_forceMouseMode = null;
		_tab._Update();
		base.Update();
		if (_forceMouseMode.HasValue)
		{
			menu.manager.menuesMouseMode = _forceMouseMode.Value;
		}
		if (!allowFocusMove)
		{
			menu.allowSelectMove = false;
		}
		else if (!menu.manager.menuesMouseMode && menu.selectedObject is UIelementWrapper && (menu.selectedObject as UIelementWrapper).tabWrapper == this && !holdElement && !_lastHoldElement && (menu.selectedObject as UIelementWrapper).ThisFocusable.InScrollBox)
		{
			OpScrollBox scrollBox = (menu.selectedObject as UIelementWrapper).thisElement.scrollBox;
			if (scrollBox._lastFocusedElement != (menu.selectedObject as UIelementWrapper).ThisFocusable)
			{
				OpScrollBox.ScrollToChild((menu.selectedObject as UIelementWrapper).ThisFocusable);
			}
			scrollBox._lastFocusedElement = (menu.selectedObject as UIelementWrapper).ThisFocusable;
			if (menu.input.thrw && !menu.lastInput.thrw)
			{
				menu.selectedObject = scrollBox.wrapper;
				if (!scrollBox.mute)
				{
					_PlaySound(scrollBox.greyedOut ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
			}
		}
		if (_lastMenuMouseMode != menu.manager.menuesMouseMode)
		{
			_MouseModeChange();
		}
		_lastMenuMouseMode = menu.manager.menuesMouseMode;
	}

	internal void Signal(UIfocusable trigger, string signal)
	{
		wrappers[trigger].Singal(null, signal);
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		_tab._Unload();
	}

	private void _MouseModeChange()
	{
		if (menu.selectedObject != null && menu.selectedObject is UIelementWrapper && (menu.selectedObject as UIelementWrapper).IsFocusable && (menu.selectedObject as UIelementWrapper).tabWrapper == this && holdElement)
		{
			(menu.selectedObject as UIelementWrapper).ThisFocusable.NonMouseSetHeld(newHeld: false);
			holdElement = false;
		}
	}

	internal void _PlaySound(SoundID soundID)
	{
		if (!(soundID == SoundID.None) && !_soundFilled && Custom.rainWorld.options.soundEffectsVolume != 0f)
		{
			_soundFill += ConfigContainer._GetSoundFill(soundID);
			menu.PlaySound(soundID);
		}
	}

	internal void _PlaySound(SoundID soundID, float pan, float vol, float pitch)
	{
		if (!(soundID == SoundID.None) && !_soundFilled && Custom.rainWorld.options.soundEffectsVolume != 0f)
		{
			_soundFill += ConfigContainer._GetSoundFill(soundID);
			menu.PlaySound(soundID, pan, vol, pitch);
		}
	}
}
