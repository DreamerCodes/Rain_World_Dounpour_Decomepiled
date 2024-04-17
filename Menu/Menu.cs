using System.Collections.Generic;
using Menu.Remix;
using Rewired;
using RWCustom;
using UnityEngine;

namespace Menu;

public abstract class Menu : MainLoopProcess
{
	public class MenuColors : ExtEnum<MenuColors>
	{
		public static readonly MenuColors White = new MenuColors("White", register: true);

		public static readonly MenuColors MediumGrey = new MenuColors("MediumGrey", register: true);

		public static readonly MenuColors DarkGrey = new MenuColors("DarkGrey", register: true);

		public static readonly MenuColors VeryDarkGrey = new MenuColors("VeryDarkGrey", register: true);

		public static readonly MenuColors Colored = new MenuColors("Colored", register: true);

		public static readonly MenuColors Black = new MenuColors("Black", register: true);

		public static readonly MenuColors DarkRed = new MenuColors("DarkRed", register: true);

		public MenuColors(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public List<Page> pages;

	public Vector2 mousePosition;

	public Vector2 lastMousePos;

	public FContainer container;

	public FContainer cursorContainer;

	public MenuScene scene;

	private bool modeSwitch;

	public bool mouseDown;

	public bool lastMouseDown;

	public MenuObject selectedObject;

	public MenuObject backObject;

	public bool pressButton;

	public bool holdButton;

	public bool lastHoldButton;

	public Player.InputPackage input;

	public Player.InputPackage lastInput;

	public int currentPage;

	private int scrollInitDelay;

	private int scrollDelay;

	public MenuMicrophone.MenuSoundLoop soundLoop;

	public SoundID mySoundLoopID = SoundID.None;

	private bool allAlienSoundLoopsAreGone;

	public string mySoundLoopName;

	protected bool infolabelDirty;

	public FLabel infoLabel;

	public float infoLabelFade;

	public float lastInfoLabelFade;

	public float infoLabelSin;

	public GradientsContainer gradientsContainer;

	public int mouseScrollWheelMovement;

	private int lastScrollWheel;

	private float floatScrollWheel;

	public bool init;

	internal bool allowSelectMove = true;

	public override bool AllowDialogs => true;

	public virtual bool ShowCursor
	{
		get
		{
			if (manager.rainWorld.options.fullScreen && !ForceNoMouseMode)
			{
				return manager.menuesMouseMode;
			}
			return false;
		}
	}

	public virtual bool ForceNoMouseMode => FreezeMenuFunctions;

	public bool Active => manager.upcomingProcess == null;

	public bool HoldButtonDisregardingFreeze
	{
		get
		{
			if (manager.menuesMouseMode)
			{
				return Input.GetMouseButton(0);
			}
			return RWInput.PlayerUIInput(-1).jmp;
		}
	}

	public Player.InputPackage NonMouseInputDisregardingFreeze => RWInput.PlayerUIInput(-1);

	protected virtual bool FreezeMenuFunctions
	{
		get
		{
			if (!(manager.upcomingProcess != null) && !(manager.fadeToBlack > 0.5f))
			{
				if (manager.dialog != null)
				{
					return manager.dialog != this;
				}
				return false;
			}
			return true;
		}
	}

	public InGameTranslator.LanguageID CurrLang => manager.rainWorld.inGameTranslator.currentLanguage;

	public static float HorizontalMoveToGetCentered(ProcessManager manager)
	{
		return (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
	}

	public bool GetFreezeMenuFunctions()
	{
		return FreezeMenuFunctions;
	}

	public static HSLColor MenuColor(MenuColors col)
	{
		if (col == MenuColors.White)
		{
			return new HSLColor(0f, 0f, 1f);
		}
		if (col == MenuColors.MediumGrey)
		{
			return new HSLColor(0.73055553f, 0.08f, 0.67f);
		}
		if (col == MenuColors.DarkGrey)
		{
			return new HSLColor(0.73055553f, 0.08f, 0.3f);
		}
		if (col == MenuColors.VeryDarkGrey)
		{
			return new HSLColor(0.73055553f, 0.08f, 0.15f);
		}
		if (col == MenuColors.Colored)
		{
			return new HSLColor(0f, 0f, 1f);
		}
		if (col == MenuColors.Black)
		{
			return new HSLColor(0.73055553f, 0f, 0f);
		}
		if (col == MenuColors.DarkRed)
		{
			return new HSLColor(1f / 30f, 0.65f, 0.235f);
		}
		return new HSLColor(0f, 0f, 1f);
	}

	public static Color MenuRGB(MenuColors col)
	{
		return MenuColor(col).rgb;
	}

	public Menu(ProcessManager manager, ProcessManager.ProcessID ID)
		: base(manager, ID)
	{
		pages = new List<Page>();
		container = new FContainer();
		Futile.stage.AddChild(container);
		mySoundLoopName = "";
		cursorContainer = new FContainer();
		currentPage = 0;
		if (ID != ProcessManager.ProcessID.Initialization)
		{
			infoLabel = new FLabel(Custom.GetFont(), "");
			infoLabel.y = Mathf.Max(0.01f + manager.rainWorld.options.SafeScreenOffset.y, 20.01f);
			infoLabel.x = manager.rainWorld.screenSize.x / 2f + 0.01f;
			Futile.stage.AddChild(infoLabel);
		}
		UpdateInfoText();
	}

	public string Translate(string s)
	{
		return manager.rainWorld.inGameTranslator.Translate(s);
	}

	public string Translate(string s, int evenSplits)
	{
		return InGameTranslator.EvenSplit(manager.rainWorld.inGameTranslator.Translate(s), evenSplits);
	}

	protected virtual void Init()
	{
		for (int i = 0; i < pages.Count; i++)
		{
			if (pages[i].selectables.Count > 0)
			{
				pages[i].lastSelectedObject = pages[i].selectables[0] as MenuObject;
			}
			pages[i].pos.x -= HorizontalMoveToGetCentered(manager);
		}
		if (scene != null)
		{
			scene.HorizontalDisplace(0f - HorizontalMoveToGetCentered(manager));
			if (gradientsContainer != null)
			{
				gradientsContainer.CreateButtonGradients();
			}
		}
		if (backObject != null && selectedObject == null)
		{
			selectedObject = backObject;
		}
		else if (!manager.menuesMouseMode && pages[0].lastSelectedObject != null)
		{
			selectedObject = pages[0].lastSelectedObject;
		}
		Futile.stage.AddChild(cursorContainer);
	}

	protected void MutualHorizontalButtonBind(MenuObject left, MenuObject right)
	{
		left.nextSelectable[2] = right;
		right.nextSelectable[0] = left;
	}

	protected void MutualVerticalButtonBind(MenuObject bottom, MenuObject top)
	{
		bottom.nextSelectable[1] = top;
		top.nextSelectable[3] = bottom;
	}

	public override void Update()
	{
		if (!init)
		{
			Init();
			init = true;
		}
		if (manager.menuesMouseMode)
		{
			floatScrollWheel += Input.GetAxis("Mouse ScrollWheel");
			if (Mathf.RoundToInt(floatScrollWheel * 15f) != lastScrollWheel)
			{
				mouseScrollWheelMovement = lastScrollWheel - Mathf.RoundToInt(floatScrollWheel * 10f);
				lastScrollWheel = Mathf.RoundToInt(floatScrollWheel * 10f);
			}
			else
			{
				mouseScrollWheelMovement = 0;
			}
		}
		else
		{
			floatScrollWheel = 0f;
			lastScrollWheel = 0;
			mouseScrollWheelMovement = 0;
		}
		base.Update();
		allowSelectMove = true;
		if ((manager.rainWorld.setup.devToolsActive || ModManager.DevTools) && (manager.currentMainLoop is RainWorldGame || manager.currentMainLoop is SlideShow || manager.currentMainLoop is EndCredits || manager.currentMainLoop is SleepAndDeathScreen || manager.currentMainLoop is GhostEncounterScreen || manager.currentMainLoop is DreamScreen))
		{
			if (Input.GetKey("s"))
			{
				framesPerSecond = 400;
			}
			else if (Input.GetKey("a"))
			{
				framesPerSecond = 10;
			}
			else
			{
				framesPerSecond = 40;
			}
		}
		lastMousePos = mousePosition;
		mousePosition = Futile.mousePosition;
		lastInput = input;
		input = RWInput.PlayerUIInput(-1);
		lastInfoLabelFade = infoLabelFade;
		if (selectedObject != null)
		{
			infoLabelFade = 1f;
		}
		else
		{
			infoLabelFade = Mathf.Max(0f, infoLabelFade - 1f / Mathf.Lerp(1f, 100f, infoLabelFade));
		}
		infoLabelSin += infoLabelFade;
		for (int i = 0; i < pages.Count; i++)
		{
			pages[i].Update();
		}
		lastHoldButton = holdButton;
		lastMouseDown = mouseDown;
		mouseDown = Input.GetMouseButton(0) && Active;
		if (FreezeMenuFunctions)
		{
			if (mouseDown && !lastMouseDown)
			{
				manager.menuesMouseMode = true;
				modeSwitch = true;
				holdButton = true;
			}
			return;
		}
		if (manager.menuesMouseMode)
		{
			if (ForceNoMouseMode)
			{
				selectedObject = null;
			}
			else
			{
				MenuObject menuObject = selectedObject;
				selectedObject = null;
				for (int j = 0; j < pages[currentPage].selectables.Count; j++)
				{
					if (!pages[currentPage].selectables[j].CurrentlySelectableMouse || !pages[currentPage].selectables[j].IsMouseOverMe)
					{
						continue;
					}
					selectedObject = pages[currentPage].selectables[j] as MenuObject;
					if (selectedObject != menuObject && !modeSwitch && selectedObject is UIelementWrapper)
					{
						if (!(selectedObject as UIelementWrapper).thisElement.mute)
						{
							PlaySound((!(selectedObject as UIelementWrapper).GreyedOut) ? SoundID.MENU_Button_Select_Mouse : SoundID.MENU_Greyed_Out_Button_Select_Mouse);
						}
						else
						{
							PlaySound((selectedObject is ButtonMenuObject && !(selectedObject as ButtonMenuObject).GetButtonBehavior.greyedOut) ? SoundID.MENU_Button_Select_Mouse : SoundID.MENU_Greyed_Out_Button_Select_Mouse);
						}
					}
					break;
				}
				holdButton = mouseDown;
				bool flag = false;
				flag = ReInput.controllers.Mouse.GetAnyButton();
				if ((input.x != 0 || input.y != 0 || input.jmp) && !flag)
				{
					manager.menuesMouseMode = false;
					modeSwitch = true;
					holdButton = input.jmp;
				}
			}
		}
		else
		{
			holdButton = input.jmp;
			if (mouseDown && !lastMouseDown)
			{
				manager.menuesMouseMode = true;
				modeSwitch = true;
				holdButton = true;
			}
			if (input.y != 0 && lastInput.y != input.y)
			{
				SelectNewObject(new IntVector2(0, input.y));
			}
			else if (input.x != 0 && lastInput.x != input.x)
			{
				SelectNewObject(new IntVector2(input.x, 0));
			}
			if (input.y != 0 && lastInput.y == input.y && input.x == 0)
			{
				scrollInitDelay++;
			}
			else if (input.x != 0 && lastInput.x == input.x && input.y == 0)
			{
				scrollInitDelay++;
			}
			else
			{
				scrollInitDelay = 0;
			}
			if (scrollInitDelay > 20)
			{
				scrollDelay++;
				if (scrollDelay > 6)
				{
					scrollDelay = 0;
					if (input.y != 0 && lastInput.y == input.y)
					{
						SelectNewObject(new IntVector2(0, input.y));
					}
					else if (input.x != 0 && lastInput.x == input.x)
					{
						SelectNewObject(new IntVector2(input.x, 0));
					}
				}
			}
			else
			{
				scrollDelay = 0;
			}
			if (allowSelectMove && backObject != null && input.thrw && !lastInput.thrw && !input.jmp && selectedObject != backObject)
			{
				selectedObject = backObject;
				if (selectedObject is UIelementWrapper)
				{
					if (!(selectedObject as UIelementWrapper).thisElement.mute)
					{
						PlaySound((!(selectedObject as UIelementWrapper).GreyedOut) ? SoundID.MENU_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
					}
					else
					{
						PlaySound((selectedObject is ButtonMenuObject && !(selectedObject as ButtonMenuObject).GetButtonBehavior.greyedOut) ? SoundID.MENU_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
					}
				}
			}
		}
		if (selectedObject != null)
		{
			if (infoLabel != null && (pages[currentPage].lastSelectedObject != selectedObject || infolabelDirty))
			{
				infoLabel.text = UpdateInfoText();
				infolabelDirty = false;
				infoLabelSin = 0f;
			}
			pages[currentPage].lastSelectedObject = selectedObject;
			if (selectedObject is UIelementWrapper)
			{
				holdButton = (selectedObject as UIelementWrapper).tabWrapper.holdElement;
			}
		}
		if (modeSwitch)
		{
			if (!holdButton)
			{
				modeSwitch = false;
			}
			holdButton = false;
			pressButton = false;
			selectedObject = null;
		}
		else
		{
			pressButton = holdButton && !lastHoldButton;
		}
		if (pressButton && selectedObject != null && selectedObject is ButtonTemplate && (selectedObject as ButtonTemplate).buttonBehav.greyedOut)
		{
			(selectedObject as ButtonTemplate).buttonBehav.extraSizeBump = 0f;
			PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
		}
		if (holdButton != lastHoldButton)
		{
			infolabelDirty = true;
		}
		if (soundLoop != null)
		{
			soundLoop.loopVolume = Custom.LerpAndTick(soundLoop.loopVolume, 1f, 0.02f, 0.05f);
		}
		else if (manager.menuMic != null)
		{
			bool flag2 = false;
			for (int k = 0; k < manager.menuMic.soundObjects.Count; k++)
			{
				if (manager.menuMic.soundObjects[k] is MenuMicrophone.MenuSoundLoop && (manager.menuMic.soundObjects[k] as MenuMicrophone.MenuSoundLoop).isBkgLoop && ((mySoundLoopID != SoundID.None && manager.menuMic.soundObjects[k].soundData.soundID == mySoundLoopID) || (mySoundLoopName != "" && manager.menuMic.soundObjects[k].soundData.soundName == mySoundLoopName)))
				{
					soundLoop = manager.menuMic.soundObjects[k] as MenuMicrophone.MenuSoundLoop;
					flag2 = true;
					break;
				}
			}
			if (!flag2 && mySoundLoopName != "")
			{
				soundLoop = PlayLoopCustom(mySoundLoopName, 0f, 1f, 1f, isBkgLoop: true);
			}
			else if (!flag2 && mySoundLoopID != SoundID.None)
			{
				soundLoop = PlayLoop(mySoundLoopID, 0f, 1f, 1f, isBkgLoop: true);
			}
		}
		if (allAlienSoundLoopsAreGone || manager.menuMic == null)
		{
			return;
		}
		allAlienSoundLoopsAreGone = true;
		for (int l = 0; l < manager.menuMic.soundObjects.Count; l++)
		{
			if (manager.menuMic.soundObjects[l] is MenuMicrophone.MenuSoundLoop && (manager.menuMic.soundObjects[l] as MenuMicrophone.MenuSoundLoop).isBkgLoop && (manager.menuMic.soundObjects[l].soundData.soundID != mySoundLoopID || manager.menuMic.soundObjects[l].soundData.soundName != mySoundLoopName))
			{
				(manager.menuMic.soundObjects[l] as MenuMicrophone.MenuSoundLoop).loopVolume = Mathf.Max(0f, (manager.menuMic.soundObjects[l] as MenuMicrophone.MenuSoundLoop).loopVolume - 0.025f);
				if ((manager.menuMic.soundObjects[l] as MenuMicrophone.MenuSoundLoop).loopVolume <= 0f)
				{
					(manager.menuMic.soundObjects[l] as MenuMicrophone.MenuSoundLoop).Destroy();
				}
				allAlienSoundLoopsAreGone = false;
			}
		}
	}

	public void ResetSelection()
	{
		selectedObject = null;
		for (int i = 0; i < pages.Count; i++)
		{
			if (pages[i].selectables.Count > 0)
			{
				pages[i].lastSelectedObject = pages[i].selectables[0] as MenuObject;
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (infoLabel != null)
		{
			infoLabel.alpha = Mathf.Lerp(lastInfoLabelFade, infoLabelFade, timeStacker);
			infoLabel.color = Color.Lerp(MenuRGB(MenuColors.White), MenuRGB(MenuColors.MediumGrey), 0.5f + 0.5f * Mathf.Sin((infoLabelSin + timeStacker) / 9f));
		}
		for (int i = 0; i < pages.Count; i++)
		{
			pages[i].GrafUpdate(timeStacker);
		}
	}

	public virtual string UpdateInfoText()
	{
		if (selectedObject is UIelementWrapper)
		{
			return (selectedObject as UIelementWrapper).thisElement.DisplayDescription();
		}
		return "";
	}

	public virtual void Singal(MenuObject sender, string message)
	{
	}

	public virtual void SliderSetValue(Slider slider, float f)
	{
	}

	public virtual float ValueOfSlider(Slider slider)
	{
		return 0f;
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (nextProcess is Menu && ((mySoundLoopID != SoundID.None && (nextProcess as Menu).mySoundLoopID == mySoundLoopID) || (mySoundLoopName != "" && (nextProcess as Menu).mySoundLoopName == mySoundLoopName)))
		{
			(nextProcess as Menu).soundLoop = soundLoop;
		}
		else if (soundLoop != null)
		{
			soundLoop.Destroy();
		}
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		for (int i = 0; i < pages.Count; i++)
		{
			pages[i].RemoveSprites();
		}
		container.RemoveFromContainer();
		cursorContainer.RemoveFromContainer();
		if (infoLabel != null)
		{
			infoLabel.RemoveFromContainer();
		}
		if (manager.menuMic == null)
		{
			return;
		}
		for (int j = 0; j < manager.menuMic.soundObjects.Count; j++)
		{
			if (manager.menuMic.soundObjects[j] is MenuMicrophone.MenuSoundLoop && !(manager.menuMic.soundObjects[j] as MenuMicrophone.MenuSoundLoop).isBkgLoop)
			{
				Custom.Log($"killing menu sound loop : {manager.menuMic.soundObjects[j].soundData.soundID}");
				manager.menuMic.soundObjects[j].Destroy();
			}
		}
	}

	private void SelectNewObject(IntVector2 direction)
	{
		if (!allowSelectMove)
		{
			return;
		}
		MenuObject menuObject = SelectCandidate(direction);
		if (menuObject != null && menuObject != selectedObject)
		{
			selectedObject = menuObject;
			if (selectedObject is UIelementWrapper)
			{
				PlaySound((!(selectedObject as UIelementWrapper).GreyedOut) ? SoundID.MENU_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
			}
			else
			{
				PlaySound((selectedObject is ButtonMenuObject && !(selectedObject as ButtonMenuObject).GetButtonBehavior.greyedOut) ? SoundID.MENU_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
			}
		}
	}

	private MenuObject SelectCandidate(IntVector2 direction)
	{
		if (selectedObject == null)
		{
			return pages[currentPage].lastSelectedObject;
		}
		if (selectedObject is HorizontalSlider && direction.x != 0)
		{
			return null;
		}
		if (selectedObject is VerticalSlider && direction.y != 0)
		{
			return null;
		}
		if (direction.x < 0 && selectedObject.nextSelectable[0] != null)
		{
			return selectedObject.nextSelectable[0];
		}
		if (direction.y > 0 && selectedObject.nextSelectable[1] != null)
		{
			return selectedObject.nextSelectable[1];
		}
		if (direction.x > 0 && selectedObject.nextSelectable[2] != null)
		{
			return selectedObject.nextSelectable[2];
		}
		if (direction.y < 0 && selectedObject.nextSelectable[3] != null)
		{
			return selectedObject.nextSelectable[3];
		}
		if (!(selectedObject is PositionedMenuObject))
		{
			return pages[currentPage].lastSelectedObject;
		}
		MenuObject result = pages[currentPage].lastSelectedObject;
		Vector2 vector = CenterPositionOfObject(selectedObject as PositionedMenuObject);
		float num = float.MaxValue;
		for (int i = 0; i < pages[currentPage].selectables.Count; i++)
		{
			if (!(pages[currentPage].selectables[i] is PositionedMenuObject) || !pages[currentPage].selectables[i].CurrentlySelectableNonMouse || pages[currentPage].selectables[i] == selectedObject)
			{
				continue;
			}
			Vector2 vector2 = CenterPositionOfObject(pages[currentPage].selectables[i] as PositionedMenuObject);
			float num2 = Vector2.Distance(vector, vector2);
			Vector2 vector3 = Custom.DirVec(vector, vector2);
			float num3 = 0.5f - 0.5f * Vector2.Dot(vector3, direction.ToVector2().normalized);
			if (num3 > 0.5f)
			{
				if (num3 > 0.8f)
				{
					num3 = 0.5f - 0.5f * Vector2.Dot(-vector3, direction.ToVector2().normalized);
					num2 = Vector2.Distance(vector, vector2 + direction.ToVector2() * ((direction.x != 0) ? 2400f : 1800f));
				}
				else
				{
					num2 += 10000f;
				}
			}
			num3 *= 50f;
			float num4 = (1f + num2) * (1f + num3);
			if (num4 < num)
			{
				result = pages[currentPage].selectables[i] as MenuObject;
				num = num4;
			}
		}
		return result;
	}

	private Vector2 CenterPositionOfObject(PositionedMenuObject obj)
	{
		if (obj is RectangularMenuObject)
		{
			return obj.ScreenPos + (obj as RectangularMenuObject).size / 2f;
		}
		return obj.pos;
	}

	public void PlaySound(SoundID soundID)
	{
		if (manager.menuMic != null)
		{
			manager.menuMic.PlaySound(soundID);
		}
		else if (manager.currentMainLoop is RainWorldGame)
		{
			(manager.currentMainLoop as RainWorldGame).cameras[0].virtualMicrophone.PlaySound(soundID, 0f, 1f, 1f, 1);
		}
	}

	public void PlaySound(SoundID soundID, float pan, float vol, float pitch)
	{
		if (manager.menuMic != null)
		{
			manager.menuMic.PlaySound(soundID, pan, vol, pitch);
		}
		else if (manager.currentMainLoop is RainWorldGame)
		{
			(manager.currentMainLoop as RainWorldGame).cameras[0].virtualMicrophone.PlaySound(soundID, pan, vol, pitch, 1);
		}
	}

	public MenuMicrophone.MenuSoundLoop PlayLoop(SoundID soundID, float pan, float vol, float pitch, bool isBkgLoop)
	{
		if (manager.menuMic == null)
		{
			return null;
		}
		return manager.menuMic.PlayLoop(soundID, pan, vol, pitch, isBkgLoop);
	}

	public void PlaySoundCustom(string soundName, float pan, float vol, float pitch)
	{
		if (manager.menuMic != null)
		{
			manager.menuMic.PlayCustomSound(soundName, pan, vol, pitch);
		}
		else if (manager.currentMainLoop is RainWorldGame)
		{
			(manager.currentMainLoop as RainWorldGame).cameras[0].virtualMicrophone.PlayCustomSoundDisembodied(soundName, pan, vol, pitch);
		}
	}

	public MenuMicrophone.MenuSoundLoop PlayLoopCustom(string soundName, float pan, float vol, float pitch, bool isBkgLoop)
	{
		if (manager.menuMic == null)
		{
			return null;
		}
		return manager.menuMic.PlayCustomLoop(soundName, pan, vol, pitch, isBkgLoop);
	}
}
