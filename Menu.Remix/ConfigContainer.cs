using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AssetBundles;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu.Remix;

public class ConfigContainer : PositionedMenuObject
{
	public enum Mode
	{
		ModSelect,
		ModView,
		ModConfig
	}

	private class FocusCursor : RectangularMenuObject
	{
		public readonly ConfigContainer cfg;

		private readonly FSprite[] _sprites;

		private float _fade = 1f;

		private float _lastFade = 1f;

		internal float Alpha => _sprites[0].alpha;

		public Rect? focusRect
		{
			get
			{
				if (FocusedElement != null)
				{
					return FocusedElement.FocusRect;
				}
				return null;
			}
		}

		public FocusCursor(ConfigContainer cfg)
			: base(cfg.menu, cfg.menu.pages[0], Vector2.zero, Vector2.one)
		{
			this.cfg = cfg;
			_sprites = new FSprite[4];
			for (int i = 0; i < _sprites.Length; i++)
			{
				_sprites[i] = new FSprite("modMenuCursor0")
				{
					rotation = 90f * (float)i,
					anchorX = 1f,
					anchorY = 1f,
					color = MenuColorEffect.rgbMediumGrey
				};
				Container.AddChild(_sprites[i]);
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			for (int i = 0; i < _sprites.Length; i++)
			{
				_sprites[i].element = Futile.atlasManager.GetElementWithName("modMenuCursor" + ((!holdElement) ? "0" : "1"));
				_sprites[i].alpha = 1f - Mathf.Lerp(_lastFade, _fade, timeStacker);
			}
			_sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) + 0.01f;
			_sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) + 0.01f;
			_sprites[1].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) + 0.01f;
			_sprites[1].y = Mathf.Lerp(lastPos.y + lastSize.y, pos.y + size.y, timeStacker) + 0.01f;
			_sprites[2].x = Mathf.Lerp(lastPos.x + lastSize.x, pos.x + size.x, timeStacker) + 0.01f;
			_sprites[2].y = Mathf.Lerp(lastPos.y + lastSize.y, pos.y + size.y, timeStacker) + 0.01f;
			_sprites[3].x = Mathf.Lerp(lastPos.x + lastSize.x, pos.x + size.x, timeStacker) + 0.01f;
			_sprites[3].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) + 0.01f;
		}

		public void Bump(IntVector2 direction)
		{
			pos += new Vector2((float)direction.x * 10f, (float)direction.y * 10f);
		}

		public override void Update()
		{
			base.Update();
			_lastFade = _fade;
			if (cfg._forceMouseMode ?? cfg.menu.manager.menuesMouseMode)
			{
				_fade = Custom.LerpAndTick(_fade, 1f, 0.2f, 0.1f / UIelement.frameMulti);
			}
			else
			{
				_fade = Custom.LerpAndTick(_fade, 0f, 0.6f, 0.1f / UIelement.frameMulti);
			}
			if (focusRect.HasValue)
			{
				Rect res = focusRect.Value;
				if (FocusedElement.InScrollBox)
				{
					_TrimFocusRectToScrollBox(FocusedElement.scrollBox, ref res);
				}
				pos = Vector2.Lerp(pos, new Vector2(res.x, res.y), 0.6f / UIelement.frameMulti);
				size = Vector2.Lerp(size, new Vector2(res.width, res.height), 0.6f / UIelement.frameMulti);
			}
		}

		private void _TrimFocusRectToScrollBox(OpScrollBox scrollBox, ref Rect res)
		{
			Vector2 vector = scrollBox._camPos - (scrollBox.horizontal ? Vector2.right : Vector2.up) * scrollBox.scrollOffset - scrollBox.pos;
			res.x -= vector.x;
			res.y -= vector.y;
			Rect rect = scrollBox.FocusRect;
			res.width = Mathf.Max(Mathf.Min(res.width, rect.width, rect.x + rect.width - res.x, res.x + res.width - rect.x), 0f);
			res.height = Mathf.Max(Mathf.Min(res.height, rect.height, rect.y + rect.height - res.y, res.y + res.height - rect.y), 0f);
			res.x = Mathf.Clamp(res.x, rect.x, rect.x + rect.width);
			res.y = Mathf.Clamp(res.y, rect.y, rect.y + rect.height);
		}
	}

	private struct ConfigHistory
	{
		public UIconfig config;

		public string origValue;
	}

	private readonly FocusCursor _cursor;

	public static ConfigContainer instance;

	internal static ConfigMenuTab menuTab;

	internal static OpTab activeTab;

	private Queue<MenuModList.ModButton> _modThumbnailWaiting;

	internal static OptionInterface[] OptItfs;

	internal static string[] OptItfID;

	internal static int[] savedActiveTabIndex;

	internal static readonly CompareInfo comInfo = CultureInfo.InvariantCulture.CompareInfo;

	private static UIfocusable lastFocusedElement;

	internal static bool holdElement;

	internal bool _allowFocusMove;

	private bool? _forceMouseMode;

	private bool _lastPressZ;

	private int modThumbLoadCap = -1;

	private bool _halt;

	private byte _lastHalt;

	private readonly Stack<ConfigHistory> _history;

	private static int _soundFill;

	public Mode _Mode { get; private set; }

	internal float _cursorAlpha => _cursor.Alpha;

	private ModdingMenu CfgMenu => menu as ModdingMenu;

	public static int ActiveTabIndex { get; private set; }

	internal static OptionInterface ActiveInterface => OptItfs[ActiveItfIndex];

	internal int _ScrollInitDelay { get; private set; }

	internal int _ScrollDelay { get; private set; }

	internal static int ActiveItfIndex { get; private set; }

	internal static bool[] OptItfInitialized { get; private set; }

	public static UIfocusable FocusedElement { get; private set; }

	private bool _ShouldHalt
	{
		get
		{
			if (!CfgMenu.HasDialogBox && !menu.GetFreezeMenuFunctions() && !_halt)
			{
				return CfgMenu._blackFade > 0f;
			}
			return true;
		}
	}

	private static bool _soundFilled
	{
		get
		{
			if (_soundFill <= UIelement.FrameMultiply(50))
			{
				return mute;
			}
			return true;
		}
	}

	public static bool mute { get; internal set; }

	public ConfigContainer(Menu menu, MenuObject owner)
		: base(menu, owner, Vector2.zero)
	{
		instance = this;
		_Mode = ((ModManager.ActiveMods.Count > 0) ? Mode.ModView : Mode.ModSelect);
		myContainer = new FContainer();
		owner.Container.AddChild(myContainer);
		_soundFill = 0;
		holdElement = false;
		_history = new Stack<ConfigHistory>();
		OpKeyBinder._InitBoundKey();
		_modThumbnailWaiting = new Queue<MenuModList.ModButton>();
		if (!CfgMenu.isReload)
		{
			_LoadItfs();
		}
		else
		{
			_ReloadItfs();
		}
		ActiveItfIndex = 0;
		if (!OptItfInitialized[0])
		{
			_InitializeItf();
		}
		ActiveItfIndex = ((ActiveItfIndex < OptItfs.Length) ? ActiveItfIndex : 0);
		if (!OptItfInitialized[ActiveItfIndex])
		{
			_InitializeItf();
		}
		savedActiveTabIndex[0] = 0;
		ActiveTabIndex = savedActiveTabIndex[ActiveItfIndex];
		activeTab = ActiveInterface.Tabs[ActiveTabIndex];
		menuTab = new ConfigMenuTab();
		lastFocusedElement = menuTab.BackButton;
		FocusedElement = menuTab.BackButton;
		menuTab._Activate();
		activeTab._Activate();
		(OptItfs[0] as InternalOI_Stats)._RefreshStats();
		_cursor = new FocusCursor(this);
		subObjects.Add(_cursor);
		if (CfgMenu.lastMode.HasValue)
		{
			_SwitchMode(CfgMenu.lastMode.Value);
		}
		menuTab._Update();
		try
		{
			activeTab?._Update();
		}
		catch
		{
		}
		menuTab._GrafUpdate(0f);
		try
		{
			activeTab?._GrafUpdate(0f);
		}
		catch
		{
		}
	}

	internal void _SwitchMode(Mode newMode)
	{
		if (newMode == _Mode)
		{
			return;
		}
		switch (newMode)
		{
		case Mode.ModSelect:
			_Mode = newMode;
			break;
		case Mode.ModConfig:
			_Mode = newMode;
			_FocusNewElement(menuTab.RevertButton, silent: true);
			break;
		case Mode.ModView:
			if (_Mode == Mode.ModConfig)
			{
				_Mode = newMode;
				_ChangeActiveMod(0);
				_FocusNewElement(GrabLastActiveModButton(top: true), silent: true);
			}
			else
			{
				_Mode = newMode;
			}
			break;
		}
	}

	internal static void _ChangeActiveTab(int newIndex)
	{
		activeTab?._Deactivate();
		ActiveTabIndex = newIndex;
		activeTab = ActiveInterface.Tabs[ActiveTabIndex];
		activeTab._Activate();
	}

	internal static void _ChangeActiveMod(int newIndex)
	{
		ActiveInterface.config.pendingReset = false;
		ActiveInterface._TriggerOnDeactivate();
		if (activeTab != null)
		{
			activeTab._Deactivate();
		}
		savedActiveTabIndex[ActiveItfIndex] = ActiveTabIndex;
		menuTab._ClearCustomNextFocusable();
		mute = false;
		ActiveItfIndex = newIndex;
		if (!OptItfInitialized[ActiveItfIndex] || OptItfs[ActiveItfIndex].Tabs == null)
		{
			_InitializeItf();
		}
		ActiveTabIndex = savedActiveTabIndex[ActiveItfIndex];
		activeTab = ActiveInterface.Tabs[ActiveTabIndex];
		activeTab._Activate();
		if (ActiveInterface.HasConfigurables())
		{
			try
			{
				mute = true;
				ActiveInterface._LoadConfigFile();
				ActiveInterface.ShowConfigs();
				mute = false;
			}
			catch (Exception ex)
			{
				ActiveInterface.ErrorScreen(new LoadDataException("OILoad/ShowConfigError: " + ActiveInterface.mod.id + " had a problem in Load/ShowConfig() \r\nAre you editing LoadConfig()/ShowConfig()? That could cause serious error." + "\r\n" + ex), isInit: true);
			}
		}
		menuTab.modList.ScrollToShow(newIndex - 1);
		menuTab.tabCtrler.Change();
		instance._history.Clear();
		ActiveInterface._TriggerOnActivate();
	}

	internal static string _GetThumbnailName(string id)
	{
		return id + "_ModThumb";
	}

	private void _LoadModThumbnails()
	{
		if (Futile.atlasManager.GetAtlasWithName(_GetThumbnailName(MenuModList.ModButton.RainWorldDummy.mod.id)) == null)
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "basegame_thumbnail.png");
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: true, crispPixels: true);
			_TrimModThumbnail(ref texture2D);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture(_GetThumbnailName(MenuModList.ModButton.RainWorldDummy.mod.id), texture2D, textureFromAsset: false);
			MenuModList.ModButton._thumbDummy = texture2D;
		}
		if (MenuModList.ModButton._thumbD == null)
		{
			Texture2D texture2D2 = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			string text2 = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "mod_empty_thumbnail.png");
			AssetManager.SafeWWWLoadTexture(ref texture2D2, "file:///" + text2, clampWrapMode: true, crispPixels: true);
			MenuModList.ModButton._thumbD = texture2D2;
			texture2D2 = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			text2 = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "mod_empty_thumbnail_grey.png");
			AssetManager.SafeWWWLoadTexture(ref texture2D2, "file:///" + text2, clampWrapMode: true, crispPixels: true);
			MenuModList.ModButton._thumbDG = texture2D2;
		}
		string[] source = ModManager.PrePackagedModIDs.ToArray();
		MenuModList.ModButton._ppThumbs = new List<Texture2D>();
		KeyValuePair<string, OptionInterface>[] array = MachineConnector._registeredOIs.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, OptionInterface> keyValuePair = array[i];
			if (source.Contains(keyValuePair.Value.mod.id) && (!string.IsNullOrEmpty(keyValuePair.Value.mod.basePath) || !string.IsNullOrEmpty(keyValuePair.Value.mod.path)))
			{
				string text3 = _GetThumbnailName(keyValuePair.Key);
				string thumbnailPath = keyValuePair.Value.mod.GetThumbnailPath();
				if (File.Exists(thumbnailPath) && Futile.atlasManager.GetAtlasWithName(text3) == null)
				{
					byte[] data = File.ReadAllBytes(thumbnailPath);
					Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
					texture.LoadImage(data);
					TextureScale.Bilinear(texture, 426, 240);
					_TrimModThumbnail(ref texture);
					HeavyTexturesCache.LoadAndCacheAtlasFromTexture(text3, texture, textureFromAsset: false);
					MenuModList.ModButton._ppThumbs.Add(texture);
				}
			}
		}
	}

	internal void QueueModThumbnails(MenuModList.ModButton[] buttons)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			if (!buttons[i].IsDummy)
			{
				_modThumbnailWaiting.Enqueue(buttons[i]);
			}
		}
	}

	internal int _LoadModThumbnail(MenuModList.ModButton button)
	{
		ModManager.Mod mod = button.itf.mod;
		if (string.IsNullOrEmpty(mod.basePath) && string.IsNullOrEmpty(mod.path))
		{
			button._PingThumbnailLoaded(blank: true);
			return 0;
		}
		int num = 0;
		string text = _GetThumbnailName(mod.id);
		string thumbnailPath = mod.GetThumbnailPath();
		if (File.Exists(thumbnailPath))
		{
			if (Futile.atlasManager.GetAtlasWithName(text) == null)
			{
				byte[] data = File.ReadAllBytes(thumbnailPath);
				Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
				texture.LoadImage(data);
				num++;
				if (texture.width != 426 || texture.height != 240)
				{
					TextureScale.Bilinear(texture, 426, 240);
					num++;
				}
				_TrimModThumbnail(ref texture);
				HeavyTexturesCache.LoadAndCacheAtlasFromTexture(text, texture, textureFromAsset: false);
				button._PingThumbnailLoaded();
			}
			else
			{
				button._PingThumbnailLoaded();
			}
		}
		else
		{
			button._PingThumbnailLoaded(blank: true);
		}
		return num;
	}

	private static void _TrimModThumbnail(ref Texture2D texture)
	{
		texture.wrapMode = TextureWrapMode.Clamp;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				texture.SetPixel((i <= 1) ? 425 : 0, (i % 2 == 0) ? j : (239 - j), Color.clear);
			}
			for (int k = 0; k < 3; k++)
			{
				texture.SetPixel((i > 1) ? 1 : 424, (i % 2 == 0) ? k : (239 - k), Color.clear);
			}
			for (int l = 0; l < 2; l++)
			{
				texture.SetPixel((i > 1) ? 2 : 423, (i % 2 == 0) ? l : (239 - l), Color.clear);
			}
			texture.SetPixel((i > 1) ? 3 : 422, (i % 2 != 0) ? 239 : 0, Color.clear);
			texture.SetPixel((i > 1) ? 4 : 421, (i % 2 != 0) ? 239 : 0, Color.clear);
		}
		texture.filterMode = FilterMode.Point;
		texture.Apply();
	}

	private static void _InitializeItf()
	{
		mute = true;
		try
		{
			if ((!MachineConnector._testing || !(ActiveInterface is InternalOI_Test)) && ActiveInterface.GetType() != MachineConnector.GetRegisteredOI(ActiveInterface.mod.id).GetType())
			{
				OptItfs[ActiveItfIndex] = MachineConnector.GetRegisteredOI(ActiveInterface.mod.id);
			}
			ActiveInterface.Initialize();
		}
		catch (Exception ex)
		{
			ActiveInterface.ErrorScreen(ex, isInit: true);
		}
		OptItfInitialized[ActiveItfIndex] = true;
		savedActiveTabIndex[ActiveItfIndex] = ((savedActiveTabIndex[ActiveItfIndex] < ActiveInterface.Tabs.Length) ? savedActiveTabIndex[ActiveItfIndex] : 0);
		for (int i = 0; i < ActiveInterface.Tabs.Length; i++)
		{
			if (ActiveInterface.Tabs[i] != null)
			{
				ActiveInterface.Tabs[i]._Deactivate();
			}
		}
		mute = false;
	}

	private void _LoadItfs()
	{
		List<OptionInterface> list = new List<OptionInterface>();
		mute = true;
		OptionInterface[] array = MachineConnector._registeredOIs.Values.ToArray();
		foreach (OptionInterface item in array)
		{
			list.Add(item);
		}
		if (MachineConnector._testing)
		{
			OptionInterface item2 = new InternalOI_Test();
			list.Add(item2);
		}
		_LoadModThumbnails();
		if (list.Count > 0)
		{
			list.Sort(CompareOIModName);
			if (OptItfs != null && OptItfs.Length == list.Count + 1)
			{
				int num = 0;
				while (true)
				{
					if (num < list.Count)
					{
						if (OptItfID[num + 1] != list[num].mod.id)
						{
							break;
						}
						num++;
						continue;
					}
					OptItfs = new OptionInterface[list.Count + 1];
					OptItfs[0] = new InternalOI_Stats();
					OptItfs[0].Initialize();
					for (int j = 0; j < list.Count; j++)
					{
						OptItfs[j + 1] = list[j];
					}
					_ReloadItfs();
					return;
				}
			}
		}
		OptItfs = new OptionInterface[list.Count + 1];
		OptItfs[0] = new InternalOI_Stats();
		OptItfs[0].Initialize();
		for (int k = 0; k < list.Count; k++)
		{
			OptItfs[k + 1] = list[k];
		}
		_RegistItfs(reload: false);
		mute = false;
	}

	private void _ReloadItfs()
	{
		mute = true;
		_RegistItfs(reload: true);
		mute = false;
	}

	private void _RegistItfs(bool reload)
	{
		if (!reload)
		{
			OptItfID = new string[OptItfs.Length];
			savedActiveTabIndex = new int[OptItfs.Length];
			OptItfInitialized = new bool[OptItfs.Length];
		}
		for (int i = 0; i < OptItfs.Length; i++)
		{
			if (!reload)
			{
				OptItfID[i] = OptItfs[i].mod.id;
				savedActiveTabIndex[i] = 0;
				OptItfInitialized[i] = false;
			}
		}
		OptItfInitialized[0] = true;
	}

	internal void _ShutdownConfigContainer()
	{
		savedActiveTabIndex[ActiveItfIndex] = ActiveTabIndex;
		_halt = true;
		if (MenuModList.ModButton._thumbD != null)
		{
			UnityEngine.Object.Destroy(MenuModList.ModButton._thumbD);
			MenuModList.ModButton._thumbD = null;
		}
		if (MenuModList.ModButton._thumbDG != null)
		{
			UnityEngine.Object.Destroy(MenuModList.ModButton._thumbDG);
			MenuModList.ModButton._thumbDG = null;
		}
		if (MenuModList.ModButton._thumbDummy != null)
		{
			UnityEngine.Object.Destroy(MenuModList.ModButton._thumbDummy);
			MenuModList.ModButton._thumbDummy = null;
		}
		if (MenuModList.ModButton._ppThumbs != null)
		{
			for (int i = 0; i < MenuModList.ModButton._ppThumbs.Count; i++)
			{
				UnityEngine.Object.Destroy(MenuModList.ModButton._ppThumbs[i]);
			}
			MenuModList.ModButton._ppThumbs.Clear();
		}
		for (int j = 0; j < OptItfs.Length; j++)
		{
			if (!OptItfInitialized[j] || OptItfs[j].Tabs == null)
			{
				continue;
			}
			OptItfs[j]._TriggerOnUnload();
			for (int k = 0; k < OptItfs[j].Tabs.Length; k++)
			{
				if (OptItfs[j].Tabs[k] != null)
				{
					OptItfs[j].Tabs[k]._Unload();
				}
			}
			OptItfInitialized[j] = false;
		}
		menuTab._Unload();
		CanBeTypedExt._ClearAssigned();
		FocusMenuPointer._ClearPointers();
		instance = null;
	}

	internal void _FarewellFreeze()
	{
		foreach (UIelement item in menuTab.items)
		{
			item.Freeze();
		}
		if (activeTab == null)
		{
			return;
		}
		foreach (UIelement item2 in activeTab.items)
		{
			try
			{
				item2.Freeze();
			}
			catch
			{
			}
		}
	}

	internal static int FindItfIndex(UIelement element)
	{
		return FindItfIndex(element.tab.owner);
	}

	internal static int FindItfIndex(OptionInterface itf)
	{
		return FindItfIndex(itf.mod);
	}

	internal static int FindItfIndex(ModManager.Mod mod)
	{
		return FindItfIndex(mod.id);
	}

	internal static int FindItfIndex(string modID)
	{
		return Array.IndexOf(OptItfID, modID);
	}

	private static int CompareOIModName(OptionInterface x, OptionInterface y)
	{
		return comInfo.Compare(ListItem.GetRealName(x.mod.LocalizedName), ListItem.GetRealName(y.mod.LocalizedName), CompareOptions.StringSort);
	}

	private List<UIfocusable> GetFocusables()
	{
		List<UIfocusable> list = new List<UIfocusable>();
		if (activeTab != null)
		{
			UIfocusable[] array = activeTab.focusables.ToArray();
			foreach (UIfocusable uIfocusable in array)
			{
				if (!uIfocusable.IsInactive)
				{
					list.Add(uIfocusable);
				}
			}
		}
		if (_Mode != Mode.ModConfig)
		{
			UIfocusable[] array = menuTab.focusables.ToArray();
			foreach (UIfocusable uIfocusable2 in array)
			{
				if (!uIfocusable2.IsInactive)
				{
					list.Add(uIfocusable2);
				}
			}
		}
		else
		{
			UIfocusable[] array = menuTab.MenuButtons;
			foreach (UIfocusable uIfocusable3 in array)
			{
				if (!uIfocusable3.IsInactive)
				{
					list.Add(uIfocusable3);
				}
			}
			ConfigTabController.TabSelectButton[] tabButtons = menuTab.tabCtrler._tabButtons;
			foreach (UIfocusable uIfocusable4 in tabButtons)
			{
				if (uIfocusable4 != null && !uIfocusable4.IsInactive)
				{
					list.Add(uIfocusable4);
				}
			}
		}
		List<UIfocusable> list2 = new List<UIfocusable>();
		if (menu.manager.menuesMouseMode)
		{
			UIfocusable[] array = list.ToArray();
			foreach (UIfocusable uIfocusable5 in array)
			{
				if (uIfocusable5.CurrentlyFocusableMouse)
				{
					list2.Add(uIfocusable5);
				}
			}
		}
		else if (FocusedElement != null && FocusedElement.InScrollBox && !FocusedElement.scrollBox.ScrollLocked)
		{
			UIfocusable[] array = list.ToArray();
			foreach (UIfocusable uIfocusable6 in array)
			{
				if (uIfocusable6.CurrentlyFocusableNonMouse && uIfocusable6.InScrollBox && uIfocusable6.scrollBox.Equals(FocusedElement.scrollBox))
				{
					list2.Add(uIfocusable6);
				}
			}
			if (list2.Count < 1)
			{
				list2.Add(FocusedElement.scrollBox);
			}
		}
		else
		{
			UIfocusable[] array = list.ToArray();
			foreach (UIfocusable uIfocusable7 in array)
			{
				if (uIfocusable7.CurrentlyFocusableNonMouse && (!uIfocusable7.InScrollBox || (uIfocusable7.scrollBox.ScrollLocked && OpScrollBox.IsChildVisible(uIfocusable7))))
				{
					list2.Add(uIfocusable7);
				}
			}
		}
		return list2;
	}

	internal void _FocusNewElement(UIfocusable element, bool silent = false)
	{
		if (element != null && element != FocusedElement)
		{
			lastFocusedElement = FocusedElement;
			FocusedElement = element;
			lastFocusedElement?._InvokeOnFocusLose();
			FocusedElement._InvokeOnFocusGet();
			if (element.InScrollBox)
			{
				OpScrollBox.ScrollToChild(element);
			}
			if (!silent)
			{
				FocusedElement.PlaySound(FocusedElement.greyedOut ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
			}
		}
	}

	internal void _FocusNewElementInDirection(IntVector2 direction)
	{
		if (FocusedElement == null)
		{
			return;
		}
		UIfocusable uIfocusable = null;
		if (FocusedElement.NextFocusable[_DirToNextDir(direction)] != null)
		{
			if (FocusedElement.NextFocusable[_DirToNextDir(direction)] is FocusPointer focusPointer)
			{
				if (focusPointer.CurrentlyFocusableNonMouse)
				{
					uIfocusable = focusPointer.GetPointed((UIfocusable.NextDirection)_DirToNextDir(direction));
					if (uIfocusable == null)
					{
						uIfocusable = FocusedElement;
					}
				}
			}
			else
			{
				uIfocusable = FocusedElement.NextFocusable[_DirToNextDir(direction)];
			}
			if (uIfocusable == FocusedElement)
			{
				MoveFail();
				return;
			}
			if (!uIfocusable.CurrentlyFocusableNonMouse)
			{
				uIfocusable = null;
			}
		}
		if (uIfocusable == null)
		{
			uIfocusable = _FocusCandidateCalculate(direction);
		}
		if (uIfocusable == FocusedElement)
		{
			MoveFail();
		}
		else
		{
			_FocusNewElement(uIfocusable);
		}
		void MoveFail()
		{
			FocusedElement.PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
			_cursor.Bump(direction);
		}
	}

	private static int _DirToNextDir(IntVector2 direction)
	{
		if (direction.x != 0)
		{
			if (direction.x <= 0)
			{
				return 0;
			}
			return 2;
		}
		if (direction.y <= 0)
		{
			return 3;
		}
		return 1;
	}

	private UIfocusable GrabLastActiveModButton(bool top)
	{
		if (ActiveInterface is InternalOI_Stats && (ActiveInterface as InternalOI_Stats).previewMod != ActiveInterface.mod)
		{
			return menuTab.modList.GetModButton((ActiveInterface as InternalOI_Stats).previewMod.id);
		}
		if (!top)
		{
			return menuTab.modList.GetModButtonAtBottom();
		}
		return menuTab.modList.GetModButtonAtTop();
	}

	private UIfocusable _FocusCandidateCalculate(IntVector2 direction)
	{
		if (FocusedElement == null)
		{
			if (_Mode != Mode.ModConfig)
			{
				return menuTab.BackButton;
			}
			return menuTab.RevertButton;
		}
		UIfocusable uIfocusable = lastFocusedElement;
		Vector2 vector = FocusedElement._CenterPos();
		List<UIfocusable> focusables = GetFocusables();
		float num = float.MaxValue;
		for (int i = 0; i < focusables.Count; i++)
		{
			if (focusables[i] == FocusedElement)
			{
				continue;
			}
			Vector2 vector2 = focusables[i]._CenterPos();
			float num2 = Vector2.Distance(vector, vector2);
			Vector2 vector3 = Custom.DirVec(vector, vector2);
			float num3 = 0.5f - 0.5f * Vector2.Dot(vector3, direction.ToVector2().normalized);
			if (num3 > 0.5f)
			{
				if (num3 > 0.8f)
				{
					num3 = 0.5f - 0.5f * Vector2.Dot(-vector3, direction.ToVector2().normalized);
					num2 = Vector2.Distance(vector, vector2 + direction.ToVector2() * ((direction.x == 0) ? 1800f : 2400f));
				}
				else
				{
					num2 += 100000f;
				}
			}
			num3 *= 50f;
			float num4 = (1f + num2) * (1f + num3);
			if (num4 < num)
			{
				uIfocusable = focusables[i];
				num = num4;
			}
		}
		if (uIfocusable.tab is ConfigMenuTab)
		{
			if (uIfocusable == FocusedElement || FocusedElement.tab is ConfigMenuTab)
			{
				bool flag;
				if (menuTab.IsPartOfButtonManager(uIfocusable))
				{
					if (direction.x != 0)
					{
						return uIfocusable;
					}
					if (direction.y >= 0)
					{
						return FocusedElement;
					}
					flag = false;
				}
				else
				{
					if (!(uIfocusable.GetType() == FocusedElement.GetType()) || (!(FocusedElement is ConfigTabController.TabSelectButton) && !(FocusedElement is MenuModList.IAmPartOfModList)) || direction.y != 0)
					{
						return uIfocusable;
					}
					if (direction.x <= 0)
					{
						return FocusedElement;
					}
					flag = true;
				}
				num = float.MaxValue;
				for (int j = 0; j < focusables.Count; j++)
				{
					if (!(focusables[j].tab is ConfigMenuTab) && focusables[j] != FocusedElement)
					{
						Vector2 vector4 = focusables[j]._CenterPos();
						float num5 = (flag ? vector4.x : vector4.y);
						if (num5 < num)
						{
							uIfocusable = focusables[j];
							num = num5;
						}
					}
				}
			}
			else if (!(FocusedElement.tab is ConfigMenuTab))
			{
				for (int k = 0; k < focusables.Count; k++)
				{
					if (!(focusables[k].tab is ConfigMenuTab) && focusables[k] != FocusedElement)
					{
						Vector2 vector5 = focusables[k]._CenterPos();
						float num6 = ((direction.x != 0) ? ((direction.x > 0) ? (vector5.x - vector.x) : (vector.x - vector5.x)) : ((direction.y > 0) ? (vector5.y - vector.y) : (vector.y - vector5.y)));
						if (num6 > 1f && num6 < num)
						{
							uIfocusable = focusables[k];
							num = num6;
						}
					}
				}
			}
		}
		return uIfocusable;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (_ShouldHalt && _lastHalt > 0)
		{
			return;
		}
		menuTab._GrafUpdate(timeStacker);
		try
		{
			activeTab?._GrafUpdate(timeStacker);
		}
		catch (Exception ex)
		{
			InterfaceUpdateError(ex);
		}
	}

	public static void ForceMenuMouseMode(bool? value)
	{
		instance._forceMouseMode = value;
	}

	private void _MouseModeChange()
	{
		if (!menu.manager.menuesMouseMode && FocusedElement == null)
		{
			_ = lastFocusedElement;
			Vector2 a = menu.mousePosition - pos;
			List<UIfocusable> focusables = GetFocusables();
			float num = float.MaxValue;
			for (int i = 0; i < focusables.Count; i++)
			{
				if (!focusables[i].InScrollBox)
				{
					Vector2 center = focusables[i].FocusRect.center;
					float num2 = Vector2.Distance(a, center);
					float num3 = 1f + num2;
					if (num3 < num)
					{
						_ = focusables[i];
						num = num3;
					}
				}
			}
			_FocusNewElement(FocusedElement, silent: true);
		}
		if (holdElement)
		{
			FocusedElement.NonMouseSetHeld(newHeld: false);
			holdElement = false;
		}
	}

	public override void Update()
	{
		bool menuesMouseMode = menu.manager.menuesMouseMode;
		base.Update();
		_soundFill = ((_soundFill > 0) ? (_soundFill - 1) : 0);
		if (_ShouldHalt)
		{
			if (_lastHalt < 4 && activeTab != null)
			{
				foreach (UIelement item in activeTab.items)
				{
					item.Freeze();
				}
			}
			_lastHalt |= 4;
			if (CfgMenu.modCalledDialog != null && CfgMenu.modCalledDialog is DialogBoxAsyncWait)
			{
				ActiveInterface?.Update();
			}
			return;
		}
		bool flag = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		if (flag)
		{
			_forceMouseMode = true;
		}
		if (_forceMouseMode.HasValue)
		{
			menu.manager.menuesMouseMode = _forceMouseMode.Value;
		}
		_forceMouseMode = null;
		if (menuesMouseMode != menu.manager.menuesMouseMode)
		{
			menuesMouseMode = menu.manager.menuesMouseMode;
			_MouseModeChange();
		}
		UIfocusable focusedElement = FocusedElement;
		_allowFocusMove = true;
		bool flag2 = holdElement;
		if (holdElement)
		{
			if (FocusedElement == null)
			{
				holdElement = false;
			}
			else
			{
				if (_lastHalt == 0)
				{
					if (activeTab != null)
					{
						foreach (UIelement item2 in activeTab.items)
						{
							if (item2 != FocusedElement)
							{
								item2.Freeze();
							}
						}
					}
					foreach (UIelement item3 in menuTab.items)
					{
						if (item3 != FocusedElement)
						{
							item3.Freeze();
						}
					}
				}
				_lastHalt |= 2;
				if ((!(FocusedElement is UIconfig) || !menu.input.pckp) && !(FocusedElement.tab is ConfigMenuTab))
				{
					try
					{
						if (!FocusedElement.greyedOut)
						{
							FocusedElement.Update();
						}
						else
						{
							holdElement = false;
						}
					}
					catch (Exception ex)
					{
						InterfaceUpdateError(ex);
						return;
					}
				}
			}
			if (holdElement)
			{
				menuTab._Update();
			}
		}
		if (!holdElement)
		{
			_lastHalt = 0;
			menuTab._Update();
			try
			{
				activeTab?._Update();
			}
			catch (Exception ex2)
			{
				InterfaceUpdateError(ex2);
				return;
			}
		}
		try
		{
			if (!ActiveInterface.error)
			{
				ActiveInterface.Update();
			}
		}
		catch (Exception ex3)
		{
			InterfaceUpdateError(ex3);
			return;
		}
		if (_forceMouseMode.HasValue)
		{
			menu.manager.menuesMouseMode = _forceMouseMode.Value;
		}
		if (menuesMouseMode != menu.manager.menuesMouseMode)
		{
			_MouseModeChange();
		}
		_allowFocusMove = _allowFocusMove && focusedElement == FocusedElement;
		if (menu.manager.menuesMouseMode)
		{
			if (!holdElement)
			{
				lastFocusedElement = FocusedElement;
				FocusedElement = null;
				List<UIfocusable> focusables = GetFocusables();
				for (int i = 0; i < focusables.Count; i++)
				{
					if (!focusables[i].CurrentlyFocusableMouse || !focusables[i].MouseOver)
					{
						continue;
					}
					FocusedElement = focusables[i];
					if (FocusedElement != lastFocusedElement)
					{
						lastFocusedElement?._InvokeOnFocusLose();
						FocusedElement._InvokeOnFocusGet();
						if (!FocusedElement.mute)
						{
							PlaySound(FocusedElement.greyedOut ? SoundID.MENU_Greyed_Out_Button_Select_Mouse : SoundID.MENU_Button_Select_Mouse);
						}
					}
					break;
				}
				if (flag)
				{
					if (Input.GetKey(KeyCode.Z))
					{
						if (!_lastPressZ)
						{
							_UndoConfigChange();
						}
						_lastPressZ = true;
					}
					else
					{
						_lastPressZ = false;
					}
				}
			}
			else if (FocusedElement is UIconfig && flag)
			{
				if (Input.GetKey(KeyCode.V) && !string.IsNullOrEmpty(UniClipboard.GetText()))
				{
					string text = UniClipboard.GetText();
					if ((FocusedElement as UIconfig).CopyFromClipboard(text))
					{
						(FocusedElement as UIconfig).bumpBehav.flash = 1f;
						if ((FocusedElement as UIconfig).cosmetic)
						{
							CfgMenu.ShowAlert(OptionalText.GetText(OptionalText.ID.ConfigContainer_AlertPasteCosmetic).Replace("<Text>", text));
						}
						else
						{
							CfgMenu.ShowAlert(OptionalText.GetText(OptionalText.ID.ConfigContainer_AlertPasteNoncosmetic).Replace("<Text>", text).Replace("<ObjectName>", (FocusedElement as UIconfig).Key));
						}
					}
				}
				else if (Input.GetKey(KeyCode.C))
				{
					string text2 = (FocusedElement as UIconfig).CopyToClipboard();
					if (!string.IsNullOrEmpty(text2))
					{
						(FocusedElement as UIconfig).bumpBehav.flash = 1f;
						UniClipboard.SetText(text2);
						if ((FocusedElement as UIconfig).cosmetic)
						{
							CfgMenu.ShowAlert(OptionalText.GetText(OptionalText.ID.ConfigContainer_AlertCopyCosmetic).Replace("<Text>", text2));
						}
						else
						{
							CfgMenu.ShowAlert(OptionalText.GetText(OptionalText.ID.ConfigContainer_AlertCopyNoncosmetic).Replace("<Text>", text2).Replace("<ObjectName>", (FocusedElement as UIconfig).Key));
						}
					}
				}
			}
		}
		else
		{
			if (FocusedElement == null || FocusedElement.IsInactive)
			{
				_FocusNewElement(lastFocusedElement ?? menuTab.BackButton, silent: true);
			}
			if (!_allowFocusMove)
			{
				_ScrollInitDelay = 0;
			}
			else if (holdElement)
			{
				if (FocusedElement is UIconfig && menu.input.pckp)
				{
					if (menu.input.thrw && !string.IsNullOrEmpty(UniClipboard.GetText()))
					{
						string text3 = UniClipboard.GetText();
						if ((FocusedElement as UIconfig).CopyFromClipboard(text3))
						{
							(FocusedElement as UIconfig).bumpBehav.flash = 1f;
							if ((FocusedElement as UIconfig).cosmetic)
							{
								CfgMenu.ShowAlert(OptionalText.GetText(OptionalText.ID.ConfigContainer_AlertPasteCosmetic).Replace("<Text>", text3));
							}
							else
							{
								CfgMenu.ShowAlert(OptionalText.GetText(OptionalText.ID.ConfigContainer_AlertPasteNoncosmetic).Replace("<Text>", text3).Replace("<ObjectName>", (FocusedElement as UIconfig).Key));
							}
						}
					}
					else if (menu.input.jmp)
					{
						string text4 = (FocusedElement as UIconfig).CopyToClipboard();
						if (!string.IsNullOrEmpty(text4))
						{
							(FocusedElement as UIconfig).bumpBehav.flash = 1f;
							UniClipboard.SetText(text4);
							if ((FocusedElement as UIconfig).cosmetic)
							{
								CfgMenu.ShowAlert(OptionalText.GetText(OptionalText.ID.ConfigContainer_AlertCopyCosmetic).Replace("<Text>", text4));
							}
							else
							{
								CfgMenu.ShowAlert(OptionalText.GetText(OptionalText.ID.ConfigContainer_AlertCopyNoncosmetic).Replace("<Text>", text4).Replace("<ObjectName>", (FocusedElement as UIconfig).Key));
							}
						}
					}
				}
			}
			else if (!flag2)
			{
				if (menu.input.thrw && !menu.lastInput.thrw)
				{
					lastFocusedElement = FocusedElement;
					UIfocusable uIfocusable = null;
					if (FocusedElement.NextFocusable[4] != null && FocusedElement.NextFocusable[4].CurrentlyFocusableNonMouse)
					{
						if (FocusedElement.NextFocusable[4] is FocusPointer focusPointer)
						{
							UIfocusable pointed = focusPointer.GetPointed(UIfocusable.NextDirection.Back);
							if (pointed != null)
							{
								uIfocusable = pointed;
							}
						}
						else if (FocusedElement.NextFocusable[4] == FocusedElement)
						{
							PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
						}
						else
						{
							uIfocusable = FocusedElement.NextFocusable[4];
						}
					}
					else if (FocusedElement.InScrollBox && FocusedElement.scrollBox.CurrentlyFocusableNonMouse)
					{
						FocusedElement.scrollBox._lastFocusedElement = FocusedElement;
						uIfocusable = FocusedElement.scrollBox;
					}
					else if (FocusedElement.tab is ConfigMenuTab)
					{
						uIfocusable = ((!(FocusedElement is ConfigTabController.TabSelectButton)) ? menuTab.BackButton : (HasConfigChanged() ? menuTab.SaveButton : menuTab.RevertButton));
					}
					else if (_Mode == Mode.ModConfig)
					{
						if (ActiveInterface.Tabs.Length > 1)
						{
							FocusedElement = menuTab.tabCtrler.GetCurrentTabButton();
						}
						else
						{
							FocusedElement = (HasConfigChanged() ? menuTab.SaveButton : menuTab.RevertButton);
						}
					}
					else
					{
						uIfocusable = menuTab.BackButton;
					}
					if (uIfocusable != lastFocusedElement)
					{
						_FocusNewElement(uIfocusable);
					}
				}
				else if (menu.input.jmp && !menu.lastInput.jmp && !FocusedElement.greyedOut)
				{
					FocusedElement.NonMouseSetHeld(newHeld: true);
				}
				else
				{
					if (menu.input.y != 0 && menu.lastInput.y != menu.input.y)
					{
						_FocusNewElementInDirection(new IntVector2(0, menu.input.y));
					}
					else if (menu.input.x != 0 && menu.lastInput.x != menu.input.x)
					{
						_FocusNewElementInDirection(new IntVector2(menu.input.x, 0));
					}
					if (menu.input.y != 0 && menu.lastInput.y == menu.input.y && menu.input.x == 0)
					{
						_ScrollInitDelay++;
					}
					else if (menu.input.x != 0 && menu.lastInput.x == menu.input.y && menu.input.y == 0)
					{
						_ScrollInitDelay++;
					}
					else
					{
						_ScrollInitDelay = 0;
					}
					if (_ScrollInitDelay > ModdingMenu.DASinit)
					{
						_ScrollDelay += ((!(FocusedElement is MenuModList.IAmPartOfModList)) ? 1 : 2);
						if (_ScrollDelay > ModdingMenu.DASdelay)
						{
							_ScrollDelay = 0;
							if (menu.input.y != 0 && menu.lastInput.y == menu.input.y)
							{
								_FocusNewElementInDirection(new IntVector2(0, menu.input.y));
							}
							else if (menu.input.x != 0 && menu.lastInput.x == menu.input.x)
							{
								_FocusNewElementInDirection(new IntVector2(menu.input.x, 0));
							}
						}
					}
					else
					{
						_ScrollDelay = 0;
					}
				}
			}
		}
		string text5 = string.Empty;
		if (FocusedElement != null)
		{
			text5 = FocusedElement.DisplayDescription();
		}
		if (menu.manager.menuesMouseMode && activeTab != null)
		{
			foreach (UIelement item4 in activeTab.items)
			{
				if (item4.IsInactive || !item4.MouseOver)
				{
					continue;
				}
				string text6 = item4.DisplayDescription();
				if (!string.IsNullOrEmpty(text6))
				{
					text5 = text6;
					if (!(item4 is OpScrollBox) && !(item4 is OpRect))
					{
						break;
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(text5))
		{
			CfgMenu.ShowDescription(text5);
		}
		if (_modThumbnailWaiting.Count <= 0)
		{
			return;
		}
		if (modThumbLoadCap < 0)
		{
			modThumbLoadCap = 4;
			if (ModManager.MMF)
			{
				if (Custom.rainWorld.options.quality == Options.Quality.HIGH)
				{
					modThumbLoadCap = 8;
				}
				else if (Custom.rainWorld.options.quality == Options.Quality.MEDIUM)
				{
					modThumbLoadCap = 6;
				}
			}
		}
		int num = modThumbLoadCap;
		if (_lastHalt > 0)
		{
			num *= 2;
		}
		while (num > 0 && _modThumbnailWaiting.Count > 0)
		{
			num -= _LoadModThumbnail(_modThumbnailWaiting.Dequeue());
		}
	}

	internal void InterfaceUpdateError(Exception ex)
	{
		_halt = true;
		holdElement = false;
		lastFocusedElement = null;
		FocusedElement = menuTab.RevertButton;
		ActiveInterface.ErrorScreen(ex, isInit: false);
		_halt = false;
	}

	public void NotifyConfigChange(UIconfig config, string oldValue, string value)
	{
		if (config.cosmetic || config.tab == null)
		{
			return;
		}
		if (_history.Count > 0 && _history.Peek().config == config)
		{
			oldValue = _history.Pop().origValue;
			if (oldValue == value)
			{
				return;
			}
		}
		_history.Push(new ConfigHistory
		{
			config = config,
			origValue = oldValue
		});
	}

	internal void _UndoConfigChange()
	{
		if (_history.Count > 0)
		{
			ConfigHistory configHistory = _history.Pop();
			string value = configHistory.config.value;
			configHistory.config.ForceValue(configHistory.origValue);
			configHistory.config._UndoCallChanges();
			configHistory.config.Change();
			ModdingMenu.instance.ShowAlert("Undo change of the value of [<UIconfigKey>] from [<CurrentValue>] to [<OriginalValue>]".Replace("<UIconfigKey>", configHistory.config.Key).Replace("<CurrentValue>", value).Replace("<OriginalValue>", configHistory.config.value));
		}
	}

	internal static void _ResetCurrentConfig()
	{
		mute = true;
		ActiveInterface.config.pendingReset = true;
		foreach (UIconfig item in ActiveInterface.GrabUIConfigs())
		{
			item.value = item.cfgEntry.defaultValue;
		}
		mute = false;
		ActiveInterface._TriggerOnConfigReset();
	}

	public static bool HasConfigChanged()
	{
		return instance._history.Count > 0;
	}

	public static void PlaySound(SoundID soundID)
	{
		if (!(soundID == SoundID.None) && !_soundFilled && !Mathf.Approximately(ModdingMenu.instance.manager.rainWorld.options.soundEffectsVolume, 0f))
		{
			_soundFill += _GetSoundFill(soundID);
			ModdingMenu.instance.PlaySound(soundID);
		}
	}

	public static void PlaySound(SoundID soundID, float pan, float vol, float pitch)
	{
		if (!(soundID == SoundID.None) && !_soundFilled && !Mathf.Approximately(ModdingMenu.instance.manager.rainWorld.options.soundEffectsVolume, 0f))
		{
			_soundFill += _GetSoundFill(soundID);
			ModdingMenu.instance.PlaySound(soundID, pan, vol, pitch);
		}
	}

	internal static int _GetSoundFill(SoundID soundID)
	{
		try
		{
			SoundLoader.SoundData soundData = Custom.rainWorld.processManager.menuMic.GetSoundData(soundID, -1);
			AssetBundleLoadAssetOperation loadOp;
			string name;
			AudioClip audioClip = Custom.rainWorld.processManager.menuMic.soundLoader.GetAudioClip(soundData.audioClip, out loadOp, out name);
			if (audioClip == null)
			{
				return 0;
			}
			return Mathf.CeilToInt(Mathf.Sqrt(audioClip.length * 60f)) + 1;
		}
		catch
		{
			return 0;
		}
	}
}
