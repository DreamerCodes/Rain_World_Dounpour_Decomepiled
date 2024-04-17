using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Rewired;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpKeyBinder : UIconfig
{
	public enum BindController
	{
		AnyController,
		Controller1,
		Controller2,
		Controller3,
		Controller4
	}

	public class Queue : ConfigQueue
	{
		protected readonly BindController controllerNo;

		protected override float sizeY => 30f;

		public Queue(Configurable<KeyCode> config, BindController controllerNo = BindController.AnyController, object sign = null)
			: base(config, sign)
		{
			this.controllerNo = controllerNo;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 150f, 50f);
			List<UIelement> list = new List<UIelement>();
			OpKeyBinder opKeyBinder = new OpKeyBinder(config as Configurable<KeyCode>, new Vector2(posX, posY), new Vector2(width, 30f), collisionCheck: true, controllerNo)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opKeyBinder.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opKeyBinder.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opKeyBinder.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opKeyBinder.OnValueChanged += onValueChanged;
			}
			mainFocusable = opKeyBinder;
			list.Add(opKeyBinder);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opKeyBinder.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opKeyBinder.bumpBehav,
					description = opKeyBinder.description
				};
				list.Add(item);
			}
			opKeyBinder.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	protected static readonly string NONE = KeyCode.None.ToString();

	private static readonly string SEP = "|";

	private static readonly string VANILLA = "RWVanilla";

	protected readonly bool _collisionCheck;

	protected static Dictionary<string, string> _BoundKey;

	protected const string GAMEPADICON = "GamepadIcon";

	protected readonly string _controlKey;

	protected readonly string _modID;

	public DyeableRect rect;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	protected FLabel _label;

	protected FSprite _sprite;

	protected BindController _bind;

	protected bool _anyKeyDown;

	protected bool _lastAnyKeyDown;

	protected string _desError;

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
			if (!_collisionCheck || !_BoundKey.ContainsValue(value) || value == NONE)
			{
				_desError = "";
			}
			else
			{
				foreach (KeyValuePair<string, string> item in _BoundKey)
				{
					if (item.Value != value || item.Key == _controlKey)
					{
						continue;
					}
					string text = Regex.Split(item.Key, SEP)[0];
					if (text != _modID)
					{
						if (text == VANILLA)
						{
							_desError = OptionalText.GetText(OptionalText.ID.OpKeyBinder_ErrorConflictVanilla);
						}
						else
						{
							_desError = OptionalText.GetText(OptionalText.ID.OpKeyBinder_ErrorConflictOtherMod).Replace("<AnotherModID>", text);
						}
					}
					else
					{
						_desError = OptionalText.GetText(OptionalText.ID.OpKeyBinder_ErrorConflictCurrMod).Replace("<ConflictButton>", value);
					}
					break;
				}
			}
			if (string.IsNullOrEmpty(_desError))
			{
				base.value = value;
				base.Menu.PlaySound(SoundID.MENU_Button_Successfully_Assigned);
				if (_collisionCheck)
				{
					_BoundKey.Remove(_controlKey);
					_BoundKey.Add(_controlKey, value);
				}
			}
			else
			{
				base.Menu.PlaySound(SoundID.MENU_Error_Ping);
			}
			Change();
		}
	}

	public OpKeyBinder(Configurable<KeyCode> config, Vector2 pos, Vector2 size, bool collisionCheck = true, BindController controllerNo = BindController.AnyController)
		: base(config, pos, size)
	{
		if (config?.OI?.mod == null)
		{
			throw new ArgumentNullException("");
		}
		if (string.IsNullOrEmpty(base.defaultValue))
		{
			_value = NONE;
		}
		_modID = config.OI.mod.id;
		_controlKey = (cosmetic ? "_" : "") + _modID + SEP + base.Key;
		_size = new Vector2(Mathf.Max(30f, size.x), Mathf.Max(30f, size.y));
		_collisionCheck = !cosmetic && collisionCheck && !UIelement.ContextWrapped;
		_bind = controllerNo;
		base.defaultValue = value;
		_Initalize(base.defaultValue);
	}

	private void _Initalize(string defaultKey)
	{
		if (_collisionCheck && _BoundKey.ContainsValue(defaultKey) && defaultKey != NONE)
		{
			foreach (KeyValuePair<string, string> item in _BoundKey)
			{
				if (!(item.Value == defaultKey))
				{
					continue;
				}
				string[] array = Regex.Split(item.Key, SEP);
				if (array.Length < 2)
				{
					MachineConnector.LogError("item.Key is not in correct format: [" + item.Key + "]");
					continue;
				}
				if (_modID != array[0])
				{
					MachineConnector.LogError("More than two mods are using same defaultKey!" + "\r\n" + "Conflicting Control: " + item.Key + " & " + _controlKey + " (duplicate defalutKey: " + item.Value + ")");
					_desError = OptionalText.GetText(OptionalText.ID.OpKeyBinder_ErrorConflictOtherModDefault).Replace("<ModID>", array[0]);
					break;
				}
				throw new ElementFormatException(this, "You are using duplicated defaultKey for OpKeyBinders!", base.Key);
			}
		}
		else
		{
			_desError = "";
		}
		if (_collisionCheck)
		{
			_BoundKey.Add(_controlKey, defaultKey);
		}
		rect = new DyeableRect(myContainer, Vector2.zero, base.size)
		{
			fillAlpha = 0.3f
		};
		_label = UIelement.FLabelCreate(defaultKey, bigText: true);
		UIelement.FLabelPlaceAtCenter(_label, Vector2.zero, base.size);
		myContainer.AddChild(_label);
		_sprite = new FSprite("GamepadIcon")
		{
			anchorX = 0f,
			anchorY = 0.5f,
			scale = 0.333f
		};
		myContainer.AddChild(_sprite);
		Change();
	}

	protected internal override string DisplayDescription()
	{
		if (!string.IsNullOrEmpty(_desError))
		{
			return _desError;
		}
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		if (base.MenuMouseMode)
		{
			if (!held)
			{
				return OptionalText.GetText(OptionalText.ID.OpKeyBinder_MouseSelectTuto);
			}
			return OptionalText.GetText((!_IsJoystick(value)) ? OptionalText.ID.OpKeyBinder_MouseBindTuto : OptionalText.ID.OpKeyBinder_MouseJoystickBindTuto);
		}
		if (!held)
		{
			return OptionalText.GetText(OptionalText.ID.OpKeyBinder_NonMouseSelectTuto);
		}
		return OptionalText.GetText((!_IsJoystick(value)) ? OptionalText.ID.OpKeyBinder_NonMouseBindTuto : OptionalText.ID.OpKeyBinder_NonMouseJoystickBindTuto);
	}

	internal static void _InitWrapped(MenuTabWrapper tabWrapper)
	{
		if (!Futile.atlasManager.DoesContainElementWithName("GamepadIcon"))
		{
			MenuIllustration menuIllustration = new MenuIllustration(tabWrapper.menu, tabWrapper, string.Empty, "GamepadIcon", Vector2.zero, crispPixels: true, anchorCenter: true);
			tabWrapper.subObjects.Add(menuIllustration);
			menuIllustration.sprite.isVisible = false;
		}
	}

	internal static void _InitBoundKey()
	{
		MenuIllustration menuIllustration = new MenuIllustration(ModdingMenu.instance, ConfigContainer.instance, string.Empty, "GamepadIcon", Vector2.zero, crispPixels: true, anchorCenter: true);
		ConfigContainer.instance.subObjects.Add(menuIllustration);
		menuIllustration.sprite.isVisible = false;
		_BoundKey = new Dictionary<string, string>();
		for (int i = 0; i < Custom.rainWorld.options.controls.Length; i++)
		{
			Options.ControlSetup controlSetup = Custom.rainWorld.options.controls[i];
			InputMapCategory mapCategory = ReInput.mapping.GetMapCategory(0);
			if (mapCategory == null || controlSetup == null || controlSetup.gameControlMap == null)
			{
				continue;
			}
			InputCategory actionCategory = ReInput.mapping.GetActionCategory(mapCategory.name);
			if (actionCategory == null)
			{
				break;
			}
			foreach (InputAction item in ReInput.mapping.ActionsInCategory(actionCategory.id))
			{
				foreach (ActionElementMap allMap in controlSetup.gameControlMap.AllMaps)
				{
					if (allMap != null && allMap.actionId == item.id)
					{
						string key = $"{VANILLA}{SEP}{i}-{allMap.id}";
						if (!_BoundKey.ContainsKey(key))
						{
							_BoundKey.Add(key, allMap.keyCode.ToString());
						}
					}
				}
			}
		}
	}

	public static BindController GetControllerForPlayer(int player)
	{
		if (player < 1 || player > 4)
		{
			throw new ElementFormatException("OpKeyBinder.GetControllerForPlayer threw error: Player number must be 1 ~ 4.");
		}
		return (BindController)Custom.rainWorld.options.controls[player - 1].usingGamePadNumber;
	}

	public static KeyCode StringToKeyCode(string str)
	{
		return (KeyCode)Enum.Parse(typeof(KeyCode), str, ignoreCase: true);
	}

	public void SetController(BindController controller)
	{
		string text = _ChangeBind(value, _bind, controller);
		_bind = controller;
		value = text;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
		rect.addSize = new Vector2(4f, 4f) * base.bumpBehav.AddSize;
		_sprite.color = base.bumpBehav.GetColor(colorEdge);
		if (greyedOut)
		{
			rect.colorFill = base.bumpBehav.GetColor(colorFill);
			rect.GrafUpdate(timeStacker);
			if (string.IsNullOrEmpty(_desError))
			{
				_label.color = base.bumpBehav.GetColor(colorEdge);
			}
			else
			{
				_label.color = new Color(0.5f, 0f, 0f);
			}
			return;
		}
		rect.colorFill = colorFill;
		rect.fillAlpha = base.bumpBehav.FillAlpha;
		rect.GrafUpdate(timeStacker);
		Color color = base.bumpBehav.GetColor(string.IsNullOrEmpty(_desError) ? colorEdge : Color.red);
		if (base.Focused || MouseOver)
		{
			color = Color.Lerp(color, Color.white, base.bumpBehav.Sin());
		}
		_label.color = color;
	}

	protected static bool _IsJoystick(string keyCode)
	{
		if (keyCode.Length > 8)
		{
			return keyCode.ToLower().Substring(0, 8) == "joystick";
		}
		return false;
	}

	protected static string _ChangeBind(string oldKey, BindController oldBind, BindController newBind)
	{
		if (!_IsJoystick(oldKey))
		{
			return oldKey;
		}
		string text = oldKey.Substring((oldBind != 0) ? 9 : 8);
		int num = (int)newBind;
		return "Joystick" + ((newBind != 0) ? num.ToString() : "") + text;
	}

	protected internal override void NonMouseSetHeld(bool newHeld)
	{
		base.NonMouseSetHeld(newHeld);
		if (newHeld)
		{
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			_label.text = "?";
		}
	}

	public unsafe override void Update()
	{
		base.Update();
		rect.Update();
		if (greyedOut)
		{
			return;
		}
		if (MouseOver && Input.GetMouseButton(0))
		{
			held = true;
		}
		_lastAnyKeyDown = _anyKeyDown;
		_anyKeyDown = Input.anyKey;
		if (held)
		{
			if (_IsJoystick(value))
			{
				if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
				{
					bool flag = false;
					BindController bindController = BindController.AnyController;
					if (Input.GetKey(KeyCode.BackQuote) || Input.GetKey(KeyCode.Alpha0) || Input.GetKey(KeyCode.Escape))
					{
						flag = true;
						bindController = BindController.AnyController;
					}
					else if (Input.GetKey(KeyCode.Alpha1))
					{
						flag = true;
						bindController = BindController.Controller1;
					}
					else if (Input.GetKey(KeyCode.Alpha2))
					{
						flag = true;
						bindController = BindController.Controller2;
					}
					else if (Input.GetKey(KeyCode.Alpha3))
					{
						flag = true;
						bindController = BindController.Controller3;
					}
					else if (Input.GetKey(KeyCode.Alpha4))
					{
						flag = true;
						bindController = BindController.Controller4;
					}
					if (flag)
					{
						if (_bind == bindController)
						{
							PlaySound(SoundID.MENU_Error_Ping);
							held = false;
						}
						else
						{
							SetController(bindController);
							held = false;
							FocusMoveDisallow(this);
						}
					}
					return;
				}
				if (Input.GetKey(KeyCode.JoystickButton7))
				{
					bool flag2 = false;
					BindController bindController2 = BindController.AnyController;
					if (Input.GetKey(KeyCode.Joystick1Button7))
					{
						flag2 = true;
						bindController2 = BindController.Controller1;
					}
					else if (Input.GetKey(KeyCode.Joystick2Button7))
					{
						flag2 = true;
						bindController2 = BindController.Controller2;
					}
					else if (Input.GetKey(KeyCode.Joystick3Button7))
					{
						flag2 = true;
						bindController2 = BindController.Controller3;
					}
					else if (Input.GetKey(KeyCode.Joystick4Button7))
					{
						flag2 = true;
						bindController2 = BindController.Controller4;
					}
					if (flag2)
					{
						if (_bind == bindController2)
						{
							SetController(BindController.AnyController);
						}
						else
						{
							SetController(bindController2);
						}
						held = false;
						FocusMoveDisallow(this);
					}
					return;
				}
			}
			if (_lastAnyKeyDown || !_anyKeyDown)
			{
				return;
			}
			if (!base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Pause))
			{
				foreach (int value in Enum.GetValues(typeof(KeyCode)))
				{
					if (Input.GetKey((KeyCode)value))
					{
						string text = ((KeyCode*)(&value))->ToString();
						if (text.Length > 4 && text.Substring(0, 5) == "Mouse")
						{
							if (!MouseOver)
							{
								PlaySound(SoundID.MENU_Error_Ping);
								held = false;
							}
						}
						else
						{
							if (_IsJoystick(text) && _bind != 0)
							{
								int bind = (int)_bind;
								text = text.Substring(0, 8) + bind + text.Substring(8);
							}
							this.value = text;
							held = false;
						}
						break;
					}
				}
				return;
			}
			this.value = NONE;
			held = false;
		}
		else if (!held && MouseOver && Input.GetMouseButton(0))
		{
			held = true;
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			_label.text = "?";
		}
	}

	protected internal override void Change()
	{
		_size = new Vector2(Mathf.Max(30f, base.size.x), Mathf.Max(30f, base.size.y));
		base.Change();
		_sprite.isVisible = _IsJoystick(value);
		if (_IsJoystick(value))
		{
			_sprite.SetPosition(5f, base.size.y / 2f);
			_label.text = value.Replace("Joystick", "");
			UIelement.FLabelPlaceAtCenter(_label, new Vector2(20f, 0f), base.size - new Vector2(20f, 0f));
		}
		else
		{
			_label.text = value;
			UIelement.FLabelPlaceAtCenter(_label, Vector2.zero, base.size);
		}
		rect.size = base.size;
	}

	protected internal override void Unload()
	{
		base.Unload();
		_BoundKey.Remove(_controlKey);
	}
}
