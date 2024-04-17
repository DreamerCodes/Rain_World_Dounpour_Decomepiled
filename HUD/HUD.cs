using System.Collections.Generic;
using Expedition;
using JollyCoop.JollyHUD;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace HUD;

public class HUD
{
	public class OwnerType : ExtEnum<OwnerType>
	{
		public static readonly OwnerType Player = new OwnerType("Player", register: true);

		public static readonly OwnerType SleepScreen = new OwnerType("SleepScreen", register: true);

		public static readonly OwnerType DeathScreen = new OwnerType("DeathScreen", register: true);

		public static readonly OwnerType FastTravelScreen = new OwnerType("FastTravelScreen", register: true);

		public static readonly OwnerType RegionOverview = new OwnerType("RegionOverview", register: true);

		public static readonly OwnerType ArenaSession = new OwnerType("ArenaSession", register: true);

		public static readonly OwnerType CharacterSelect = new OwnerType("CharacterSelect", register: true);

		public OwnerType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public IOwnAHUD owner;

	public RainWorld rainWorld;

	public List<HudPart> parts;

	public FContainer[] fContainers;

	public List<FadeCircle> fadeCircles;

	public bool showKarmaFoodRain;

	public BreathMeter breathMeter;

	public ThreatPulser threatPulser;

	public ChatLogDisplay chatLog;

	public HypothermiaMeter HypoMeter;

	public GourmandMeter gourmandmeter;

	public JollyMeter jollyMeter;

	public Dictionary<JollyPlayerSpecificHud.JollyPointer, Vector2> pointerPositions;

	private List<JollyPlayerSpecificHud.JollyPointer> pointersList;

	public FoodMeter foodMeter { get; private set; }

	public Map map { get; private set; }

	public TextPrompt textPrompt { get; private set; }

	public KarmaMeter karmaMeter { get; private set; }

	public RainMeter rainMeter { get; private set; }

	public DialogBox dialogBox { get; private set; }

	public bool HideGeneralHud
	{
		get
		{
			if (textPrompt == null)
			{
				return false;
			}
			if (textPrompt.hideHud)
			{
				return textPrompt.show > 0f;
			}
			return false;
		}
	}

	public HUD(FContainer[] fContainers, RainWorld rainWorld, IOwnAHUD owner)
	{
		this.fContainers = fContainers;
		this.rainWorld = rainWorld;
		this.owner = owner;
		parts = new List<HudPart>();
		pointersList = new List<JollyPlayerSpecificHud.JollyPointer>(12);
		fadeCircles = new List<FadeCircle>();
		pointerPositions = new Dictionary<JollyPlayerSpecificHud.JollyPointer, Vector2>();
	}

	public void Update()
	{
		for (int num = parts.Count - 1; num >= 0; num--)
		{
			if (parts[num].slatedForDeletion)
			{
				if (parts[num] is ChatLogDisplay)
				{
					chatLog = null;
				}
				parts[num].ClearSprites();
				parts.RemoveAt(num);
			}
			else
			{
				parts[num].Update();
			}
		}
		for (int num2 = fadeCircles.Count - 1; num2 >= 0; num2--)
		{
			if (fadeCircles[num2].life <= 0f)
			{
				fadeCircles[num2].Destroy();
				fadeCircles.RemoveAt(num2);
			}
			else
			{
				fadeCircles[num2].Update();
			}
		}
		if (owner.GetOwnerType() == OwnerType.Player)
		{
			showKarmaFoodRain = owner.RevealMap || (owner as Player).showKarmaFoodRainTime > 0 || ((owner as Player).room != null && (owner as Player).room.abstractRoom.shelter && (owner as Player).room.abstractRoom.realizedRoom != null && (owner as Player).room.abstractRoom.realizedRoom.shelterDoor != null && !(owner as Player).room.abstractRoom.realizedRoom.shelterDoor.Broken) || (HypoMeter != null && HypoMeter.tutorialTimer > 0);
		}
		pointersList.Clear();
		for (int i = 0; i < parts.Count; i++)
		{
			if (!(parts[i] is JollyPlayerSpecificHud jollyPlayerSpecificHud))
			{
				continue;
			}
			for (int j = 0; j < jollyPlayerSpecificHud.parts.Count; j++)
			{
				if (jollyPlayerSpecificHud.parts[j] is JollyPlayerSpecificHud.JollyPointer item)
				{
					pointersList.Add(item);
				}
			}
		}
		for (int k = 0; k < pointersList.Count; k++)
		{
			JollyPlayerSpecificHud.JollyPointer jollyPointer = pointersList[k];
			bool flag = pointerPositions.ContainsKey(jollyPointer);
			if (jollyPointer.hidden)
			{
				if (flag)
				{
					pointerPositions.Remove(jollyPointer);
				}
			}
			else if (!flag)
			{
				pointerPositions.Add(jollyPointer, jollyPointer.targetPos);
			}
			else
			{
				pointerPositions[jollyPointer] = jollyPointer.targetPos;
			}
		}
	}

	public void Draw(float timeStacker)
	{
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].Draw(timeStacker);
		}
		for (int num = fadeCircles.Count - 1; num >= 0; num--)
		{
			fadeCircles[num].circle.Draw(timeStacker);
		}
	}

	public void ResetMap(Map.MapData mapData)
	{
		if (map != null)
		{
			map.ClearSprites();
			map.DestroyTextures();
			map.slatedForDeletion = true;
			map = null;
		}
		AddPart(new Map(this, mapData));
	}

	public void PlaySound(SoundID soundID)
	{
		owner.PlayHUDSound(soundID);
	}

	public void AddPart(HudPart part)
	{
		if (part is FoodMeter)
		{
			foodMeter = part as FoodMeter;
		}
		else if (part is TextPrompt)
		{
			textPrompt = part as TextPrompt;
		}
		else if (part is Map)
		{
			map = part as Map;
		}
		else if (part is KarmaMeter)
		{
			karmaMeter = part as KarmaMeter;
		}
		else if (part is RainMeter)
		{
			rainMeter = part as RainMeter;
		}
		else if (part is DialogBox)
		{
			dialogBox = part as DialogBox;
		}
		else if (part is ChatLogDisplay)
		{
			chatLog = part as ChatLogDisplay;
		}
		else if (part is HypothermiaMeter)
		{
			HypoMeter = part as HypothermiaMeter;
		}
		else if (part is GourmandMeter)
		{
			gourmandmeter = part as GourmandMeter;
		}
		else if (part is JollyMeter)
		{
			jollyMeter = part as JollyMeter;
		}
		else if (part is BreathMeter)
		{
			breathMeter = part as BreathMeter;
		}
		else if (part is ThreatPulser)
		{
			threatPulser = part as ThreatPulser;
		}
		parts.Add(part);
	}

	public void InitGameOverMode(Creature.Grasp dependentOnGrasp, int foodInStomach, int deathRoom, Vector2 deathPos)
	{
		textPrompt.EnterGameOverMode(dependentOnGrasp, foodInStomach, deathRoom, deathPos);
	}

	public void InitSinglePlayerHud(RoomCamera cam)
	{
		AddPart(new TextPrompt(this));
		AddPart(new KarmaMeter(this, fContainers[1], new IntVector2((owner as Player).Karma, (owner as Player).KarmaCap), (owner as Player).KarmaIsReinforced));
		AddPart(new FoodMeter(this, (owner as Player).slugcatStats.maxFood, (owner as Player).slugcatStats.foodToHibernate));
		AddPart(new Map(this, new Map.MapData(cam.room.world, cam.room.game.rainWorld)));
		AddPart(new RainMeter(this, fContainers[1]));
		if (ModManager.MSC)
		{
			AddPart(new AmmoMeter(this, null, fContainers[1]));
			AddPart(new HypothermiaMeter(this, fContainers[1]));
			if ((owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				AddPart(new GourmandMeter(this, fContainers[1]));
			}
		}
		if (ModManager.MMF && MMF.cfgBreathTimeVisualIndicator.Value)
		{
			AddPart(new BreathMeter(this, fContainers[1]));
			if (ModManager.CoopAvailable && cam.room.game.session != null)
			{
				for (int i = 1; i < cam.room.game.session.Players.Count; i++)
				{
					AddPart(new BreathMeter(this, fContainers[1], cam.room.game.session.Players[i]));
				}
			}
		}
		if (ModManager.MMF && MMF.cfgThreatMusicPulse.Value)
		{
			AddPart(new ThreatPulser(this, fContainers[1]));
		}
		if (ModManager.MMF && MMF.cfgSpeedrunTimer.Value)
		{
			AddPart(new SpeedRunTimer(this, null, fContainers[1]));
		}
		if (ModManager.CoopAvailable && Custom.rainWorld.options.jollyHud && cam.room.game.session != null && !cam.InCutscene)
		{
			AddPart(new JollyMeter(this, fContainers[1]));
			for (int j = 0; j < cam.room.game.session.Players.Count; j++)
			{
				JollyPlayerSpecificHud part = new JollyPlayerSpecificHud(this, fContainers[1], cam.room.game.session.Players[j]);
				AddPart(part);
			}
		}
		if (ModManager.Expedition && rainWorld.options.saveSlot < 0 && ExpeditionData.challengeList != null)
		{
			AddPart(new ExpeditionHUD(this, fContainers[1]));
		}
		if (cam.room.abstractRoom.shelter)
		{
			karmaMeter.fade = 1f;
			rainMeter.fade = 1f;
			foodMeter.fade = 1f;
		}
	}

	public void InitSleepHud(SleepAndDeathScreen sleepAndDeathScreen, Map.MapData mapData, SlugcatStats charStats)
	{
		AddPart(new FoodMeter(this, charStats.maxFood, charStats.foodToHibernate));
		if (mapData != null)
		{
			AddPart(new Map(this, mapData));
		}
		foodMeter.pos = new Vector2(sleepAndDeathScreen.FoodMeterXPos((sleepAndDeathScreen.ID == ProcessManager.ProcessID.SleepScreen) ? 0f : 1f), 0f);
		foodMeter.lastPos = foodMeter.pos;
	}

	public void InitFastTravelHud(Map.MapData mapData)
	{
		ResetMap(mapData);
		map.revealAllDiscovered = true;
		map.resetRevealCounter = 0;
	}

	public void InitMultiplayerHud(ArenaGameSession session)
	{
		AddPart(new TextPrompt(this));
		AddPart(new GeneralMultiplayerHud(this, session));
		for (int i = 0; i < session.Players.Count; i++)
		{
			PlayerSpecificMultiplayerHud playerSpecificMultiplayerHud = new PlayerSpecificMultiplayerHud(this, session, session.Players[i]);
			AddPart(playerSpecificMultiplayerHud);
			if (ModManager.MSC)
			{
				AddPart(new AmmoMeter(this, playerSpecificMultiplayerHud, fContainers[1]));
			}
		}
		if (session.Players.Count > 0 && ModManager.MSC && session.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && ModManager.MMF && MMF.cfgBreathTimeVisualIndicator.Value)
		{
			AddPart(new BreathMeter(this, fContainers[1], session.Players[0]));
		}
	}

	public DialogBox InitDialogBox()
	{
		if (dialogBox == null)
		{
			AddPart(new DialogBox(this));
		}
		return dialogBox;
	}

	public void ClearAllSprites()
	{
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].ClearSprites();
		}
		for (int j = 0; j < fContainers.Length; j++)
		{
			fContainers[j].RemoveFromContainer();
		}
	}

	public ChatLogDisplay InitChatLog(string[] messages)
	{
		if (chatLog == null)
		{
			AddPart(new ChatLogDisplay(this, messages));
		}
		return chatLog;
	}

	public void DisposeChatLog()
	{
		if (owner is Player)
		{
			(owner as Player).abstractCreature.world.game.pauseUpdate = false;
		}
		if (chatLog != null)
		{
			chatLog.slatedForDeletion = true;
			chatLog.ClearSprites();
			parts.Remove(chatLog);
			chatLog = null;
		}
	}

	public void InitSafariHud(RoomCamera cam)
	{
		AddPart(new TextPrompt(this));
		AddPart(new Map(this, new Map.MapData(cam.room.world, cam.room.game.rainWorld)));
		textPrompt.AddMessage(rainWorld.inGameTranslator.Translate("Refer to the Pause Menu for Safari Mode controls."), 120, 400, darken: false, hideHud: true);
	}
}
