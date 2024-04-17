using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Expedition;
using HUD;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class RoomCamera
{
	public class CameraCutsceneType : ExtEnum<CameraCutsceneType>
	{
		public static readonly CameraCutsceneType VoidSea = new CameraCutsceneType("VoidSea", register: true);

		public static readonly CameraCutsceneType Oracle = new CameraCutsceneType("Oracle", register: true);

		public static readonly CameraCutsceneType HunterStart = new CameraCutsceneType("HunterStart", register: true);

		public static readonly CameraCutsceneType EndingOE = new CameraCutsceneType("EndingOE", register: true);

		public CameraCutsceneType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class SpriteLeaser
	{
		public IDrawable drawableObject;

		public FSprite[] sprites;

		public bool deleteMeNextFrame;

		public FContainer[] containers;

		public SpriteLeaser(IDrawable obj, RoomCamera rCam)
		{
			drawableObject = obj;
			drawableObject.InitiateSprites(this, rCam);
			drawableObject.ApplyPalette(this, rCam, rCam.currentPalette);
		}

		public void Update(float timeStacker, RoomCamera rCam, Vector2 camPos)
		{
			RainWorld.CurrentlyDrawingObject = drawableObject;
			drawableObject.DrawSprites(this, rCam, timeStacker, camPos);
			if (drawableObject is CosmeticSprite)
			{
				(drawableObject as CosmeticSprite).PausedDrawSprites(this, rCam, timeStacker, camPos);
			}
			if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.egg != null)
			{
				rbUpdate(timeStacker);
			}
		}

		public void CleanSpritesAndRemove()
		{
			deleteMeNextFrame = true;
			RemoveAllSpritesFromContainer();
		}

		public void RemoveAllSpritesFromContainer()
		{
			for (int i = 0; i < sprites.Length; i++)
			{
				sprites[i].RemoveFromContainer();
			}
			if (containers != null)
			{
				for (int j = 0; j < containers.Length; j++)
				{
					containers[j].RemoveFromContainer();
				}
			}
		}

		public void AddSpritesToContainer(FContainer newContainer, RoomCamera rCam)
		{
			RainWorld.CurrentlyDrawingObject = drawableObject;
			drawableObject.AddToContainer(this, rCam, newContainer);
		}

		public void UpdatePalette(RoomCamera rCam, RoomPalette palette)
		{
			RainWorld.CurrentlyDrawingObject = drawableObject;
			drawableObject.ApplyPalette(this, rCam, palette);
		}

		public void rbUpdate(float timeStacker)
		{
			if (ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer) <= -1 || ExpeditionData.ints[ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer)] != 2)
			{
				return;
			}
			if (drawableObject is PlayerGraphics)
			{
				for (int i = 0; i < sprites.Length; i++)
				{
					if (i < 9 || (ModManager.MSC && ((drawableObject as PlayerGraphics).owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && i >= 12 && i <= 12 + (drawableObject as PlayerGraphics).gills.numberOfSprites) || (ModManager.MSC && ((drawableObject as PlayerGraphics).owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && i == 12) || (ModManager.MSC && ((drawableObject as PlayerGraphics).owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && i == 12))
					{
						sprites[i].color = new HSLColor(Mathf.Sin(ExpeditionGame.egg.counter / 20f), 1f, 0.75f).rgb;
						if (sprites[i].shader != Custom.rainWorld.Shaders["MenuTextCustom"])
						{
							sprites[i].shader = Custom.rainWorld.Shaders["MenuTextCustom"];
						}
					}
				}
			}
			if (drawableObject is LizardGraphics && (drawableObject as LizardGraphics).lizard.Template.type == CreatureTemplate.Type.CyanLizard)
			{
				(drawableObject as LizardGraphics).lizard.effectColor = new HSLColor(Mathf.Sin(ExpeditionGame.egg.counter / 20f), 1f, 0.75f).rgb;
			}
		}

		public void PausedUpdate(float timeStacker, RoomCamera rCam, Vector2 camPos)
		{
			if (drawableObject is CosmeticSprite)
			{
				(drawableObject as CosmeticSprite).PausedDrawSprites(this, rCam, timeStacker, camPos);
			}
		}
	}

	public RainWorldGame game;

	public int currentCameraPosition;

	public Vector2 lastPos;

	public Vector2 pos;

	private Vector2 seekPos;

	private Vector2 leanPos;

	private List<SpriteLeaser> spriteLeasers = new List<SpriteLeaser>();

	private FContainer[] SpriteLayers;

	private Dictionary<string, int> SpriteLayerIndex;

	private List<ISingleCameraDrawable> singleCameraDrawables;

	public bool splitScreenMode;

	public float controllerShake;

	public float lightBloomAlpha;

	public RoomSettings.RoomEffect.Type lightBloomAlphaEffect;

	public int cameraNumber;

	public Vector2 offset;

	private WaterLight waterLight;

	public FSprite levelGraphic;

	private byte[] preLoadedTexture = new byte[0];

	private byte[] preLoadedBKG = new byte[0];

	private FSprite backgroundGraphic;

	private Texture2D paletteTexture;

	public int paletteA;

	public int paletteB = -1;

	private Texture2D fadeTexA;

	private Texture2D fadeTexB;

	private Texture2D ghostFadeTex;

	private Vector4 lastFadeCoord;

	public float paletteBlend;

	public bool voidSeaMode;

	public float voidSeaGoldFilter;

	public float ghostMode;

	public float mushroomMode;

	public FSprite fullScreenEffect;

	public RoomPalette currentPalette;

	public AbstractCreature followAbstractCreature;

	private Vector2 followCreatureInputForward;

	private bool applyPosChangeWhenTextureIsLoaded;

	private int loadingCameraPos;

	private int mostLikelyNextCamPos;

	private Room loadingRoom;

	private string quenedTexture;

	private static Texture2D allEffectColorsTexture;

	public ShortcutGraphics shortcutGraphics;

	private FLabel[,] EXITNUMBERLABELS;

	public VirtualMicrophone virtualMicrophone;

	public global::HUD.HUD hud;

	public float effect_dayNight;

	public float effect_darkness;

	public float effect_brightness;

	public float effect_contrast;

	public float effect_desaturation;

	public float effect_hue;

	public float effect_waterdepth;

	public bool dayNightNeedsRefresh;

	public Texture2D snowLightTex;

	private RenderTexture SnowTexture;

	public BlizzardGraphics blizzardGraphics;

	public bool snowChange;

	private int frameCount;

	private Rect lastRect;

	private bool isRT;

	private Color[] empty;

	public bool fullscreenSync;

	public Vector2 hardLevelGfxOffset;

	public bool takeSaintScreenshot;

	public int saintSpamSafety;

	public float sofBlackFade;

	public int cameraTriggerCooldown;

	private AbstractCreature cutscenePlayer;

	public Room room { get; private set; }

	public float screenShake { get; private set; }

	public float microShake { get; private set; }

	public float screenMovementShake { get; private set; }

	public float ScreenShake => Mathf.InverseLerp(0.01f, 1.5f, Mathf.Max(screenShake, microShake));

	private Texture2D levelTexture => game.rainWorld.persistentData.cameraTextures[cameraNumber, 0];

	private Texture2D backgroundTexture => game.rainWorld.persistentData.cameraTextures[cameraNumber, 1];

	private Vector4 fadeCoord => new Vector4(paletteBlend, DarkPalette, ghostMode, mushroomMode);

	private float DarkPalette
	{
		get
		{
			if (room == null)
			{
				return 0f;
			}
			if (room.roomSettings.DangerType == RoomRain.DangerType.None)
			{
				if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f && (!ModManager.MMF || room.world.rainCycle.brokenAntiGrav != null))
				{
					return 1f - room.world.rainCycle.brokenAntiGrav.CurrentLightsOn;
				}
				return 0f;
			}
			return room.world.rainCycle.RainDarkPalette * room.roomSettings.RainIntensity;
		}
	}

	public bool AboutToSwitchRoom => loadingRoom != null;

	public Vector2 sSize => game.rainWorld.screenSize;

	public float hDisplace => (1400f - sSize.x) / 2f - 8f;

	public CameraCutsceneType cutsceneType { get; private set; }

	public bool InCutscene
	{
		get
		{
			if (cutscenePlayer != null)
			{
				return cutsceneType != null;
			}
			return false;
		}
	}

	public bool roomSafeForPause
	{
		get
		{
			if (room != null)
			{
				return room.fullyLoaded;
			}
			return true;
		}
	}

	private Vector2 CamPos(int index)
	{
		return room.cameraPositions[index];
	}

	public void EnterCutsceneMode(AbstractCreature cutscenePlayer, CameraCutsceneType type)
	{
		if (ModManager.CoopAvailable)
		{
			if (this.cutscenePlayer != cutscenePlayer)
			{
				JollyCustom.Log($"New cutscene mode! Player {cutscenePlayer}, {type}");
			}
			this.cutscenePlayer = cutscenePlayer;
			cutsceneType = type;
		}
	}

	public void ExitCutsceneMode()
	{
		if (cutsceneType != null)
		{
			JollyCustom.Log($"Exiting cutscene mode! (prev: {cutsceneType})");
		}
		cutscenePlayer = null;
		cutsceneType = null;
	}

	public void ChangeCameraToPlayer(AbstractCreature cameraTarget)
	{
		if (cameraTarget != null && cameraTarget.realizedCreature != null && !InCutscene && cameraTriggerCooldown <= 0)
		{
			if (cameraTarget.Room.realizedRoom == null && cameraTarget.Room.world == room.world)
			{
				room.game.world.ActivateRoom(cameraTarget.Room);
			}
			followAbstractCreature = cameraTarget;
			JollyCustom.Log("Jolly Camera: Changed camera to playerNumber:  " + (cameraTarget.realizedCreature as Player).playerState.playerNumber);
			JollyCustom.Log("Camera viewing: " + room.abstractRoom.name + ", player at " + cameraTarget.Room.name);
			if (hud.jollyMeter != null)
			{
				hud.jollyMeter.customFade = 10f;
			}
			if (Custom.rainWorld.options.cameraCycling)
			{
				cameraTriggerCooldown = 20;
			}
		}
	}

	public RoomCamera(RainWorldGame game, int cameraNumber)
	{
		this.game = game;
		followAbstractCreature = null;
		room = null;
		pos = new Vector2(0f, 0f);
		lastPos = pos;
		leanPos = new Vector2(0f, 0f);
		screenShake = 0f;
		screenMovementShake = 0f;
		microShake = 0f;
		this.cameraNumber = cameraNumber;
		offset = new Vector2((float)cameraNumber * 6000f, 0f);
		followCreatureInputForward = new Vector2(0f, 0f);
		singleCameraDrawables = new List<ISingleCameraDrawable>();
		virtualMicrophone = new VirtualMicrophone(this);
		Shader.SetGlobalFloat(RainWorld.ShadPropRain, 0.5f);
		if (allEffectColorsTexture == null)
		{
			allEffectColorsTexture = new Texture2D(40, 4, TextureFormat.ARGB32, mipChain: false);
			string text = AssetManager.ResolveFilePath("Palettes" + Path.DirectorySeparatorChar + "effectColors.png");
			AssetManager.SafeWWWLoadTexture(ref allEffectColorsTexture, "file:///" + text, clampWrapMode: false, crispPixels: true);
		}
		SpriteLayers = new FContainer[13];
		for (int i = 0; i < SpriteLayers.Length; i++)
		{
			SpriteLayers[i] = new FContainer();
			Futile.stage.AddChild(SpriteLayers[i]);
		}
		SpriteLayerIndex = new Dictionary<string, int>();
		SpriteLayerIndex.Add("Shadows", 0);
		SpriteLayerIndex.Add("BackgroundShortcuts", 1);
		SpriteLayerIndex.Add("Background", 2);
		SpriteLayerIndex.Add("Midground", 3);
		SpriteLayerIndex.Add("Items", 4);
		SpriteLayerIndex.Add("Foreground", 5);
		SpriteLayerIndex.Add("ForegroundLights", 6);
		SpriteLayerIndex.Add("Shortcuts", 7);
		SpriteLayerIndex.Add("Water", 8);
		SpriteLayerIndex.Add("GrabShaders", 9);
		SpriteLayerIndex.Add("Bloom", 10);
		SpriteLayerIndex.Add("HUD", 11);
		SpriteLayerIndex.Add("HUD2", 12);
		levelGraphic = new FSprite("LevelTexture");
		levelGraphic.anchorX = 0f;
		levelGraphic.anchorY = 0f;
		ReturnFContainer("Foreground").AddChild(levelGraphic);
		backgroundGraphic = new FSprite("BackgroundTexture");
		backgroundGraphic.shader = game.rainWorld.Shaders["Background"];
		backgroundGraphic.anchorX = 0f;
		backgroundGraphic.anchorY = 0f;
		ReturnFContainer("Foreground").AddChild(backgroundGraphic);
		shortcutGraphics = new ShortcutGraphics(this, game.shortcuts, new FShader[1] { game.rainWorld.Shaders["Shortcuts"] });
		paletteTexture = new Texture2D(32, 8, TextureFormat.ARGB32, mipChain: false);
		paletteTexture.anisoLevel = 0;
		paletteTexture.filterMode = FilterMode.Point;
		paletteTexture.wrapMode = TextureWrapMode.Clamp;
		SnowTexture = new RenderTexture(1400, 800, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		SnowTexture.filterMode = FilterMode.Point;
		Shader.SetGlobalTexture(RainWorld.ShadPropSnowTex, SnowTexture);
		Shader.DisableKeyword("SNOW_ON");
		paletteA = -1;
		empty = new Color[49];
		Color color = new Color(0f, 0f, 0f, 0f);
		for (int j = 0; j < empty.Length; j++)
		{
			empty[j] = color;
		}
		snowLightTex = new Texture2D(7, 7, TextureFormat.RGBA32, mipChain: false);
		snowLightTex.filterMode = FilterMode.Point;
		Shader.SetGlobalTexture(RainWorld.ShadPropSnowSources, snowLightTex);
		fullscreenSync = Screen.fullScreen;
		LoadGhostPalette(32);
		ChangeMainPalette(0);
	}

	public void ClearAllSprites()
	{
		for (int i = 0; i < SpriteLayers.Length; i++)
		{
			SpriteLayers[i].RemoveAllChildren();
			SpriteLayers[i].RemoveFromContainer();
		}
		if (preLoadedTexture != null)
		{
			preLoadedTexture = new byte[0];
		}
		if (preLoadedBKG != null)
		{
			preLoadedBKG = new byte[0];
		}
		if (hud != null)
		{
			hud.ClearAllSprites();
		}
	}

	public void Update()
	{
		if (saintSpamSafety > 0)
		{
			saintSpamSafety--;
		}
		if (hud != null)
		{
			hud.Update();
		}
		else if (room != null && followAbstractCreature != null && followAbstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && followAbstractCreature.realizedCreature != null && game.world != null && !game.world.singleRoomWorld)
		{
			FireUpSinglePlayerHUD(followAbstractCreature.realizedCreature as Player);
		}
		else if (ModManager.MSC && room != null && followAbstractCreature != null && followAbstractCreature.realizedCreature != null && game.world != null && !game.world.singleRoomWorld && followAbstractCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer && (followAbstractCreature.realizedCreature as Overseer).SafariOverseer)
		{
			FireUpSafariHUD();
		}
		if (blizzardGraphics == null && room != null && ((ModManager.MSC && room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard) || room.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard))
		{
			blizzardGraphics = new BlizzardGraphics(this);
		}
		if (blizzardGraphics != null && room != null && (!ModManager.MSC || room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard) && room.roomSettings.DangerType != RoomRain.DangerType.AerieBlizzard)
		{
			blizzardGraphics = null;
			if (room.blizzardGraphics != null)
			{
				room.RemoveObject(room.blizzardGraphics);
				room.blizzardGraphics.Destroy();
				room.blizzardGraphics = null;
				room.blizzard = false;
			}
		}
		shortcutGraphics.Update();
		if (game.devToolsActive && Input.GetKey("n"))
		{
			pos += (Vector2)Futile.mousePosition / 35f;
		}
		if (applyPosChangeWhenTextureIsLoaded && preLoadedTexture != null && preLoadedTexture.Length != 0)
		{
			Custom.Log("Texture now loaded and applied.");
			ApplyPositionChange();
		}
		if (room == null)
		{
			return;
		}
		if (frameCount >= 10)
		{
			Camera mainCamera = Custom.rainWorld.MainCamera;
			if (isRT != (mainCamera.targetTexture == null) || lastRect.width != mainCamera.pixelRect.width || lastRect.height != mainCamera.pixelRect.height)
			{
				if (room.snow)
				{
					UpdateSnowLight();
				}
				if (blizzardGraphics != null)
				{
					blizzardGraphics.TileTexUpdate();
				}
			}
			frameCount = 0;
			isRT = mainCamera.targetTexture == null;
			lastRect = mainCamera.pixelRect;
		}
		else
		{
			frameCount++;
		}
		effect_dayNight = 0f;
		effect_darkness = 0f;
		effect_brightness = 0f;
		effect_contrast = 0f;
		effect_desaturation = 0f;
		effect_hue = 0f;
		effect_waterdepth = -1f;
		for (int i = 0; i < room.roomSettings.effects.Count; i++)
		{
			RoomSettings.RoomEffect roomEffect = room.roomSettings.effects[i];
			if (roomEffect.type == RoomSettings.RoomEffect.Type.DayNight)
			{
				effect_dayNight = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.Darkness)
			{
				effect_darkness = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.Brightness)
			{
				effect_brightness = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.Contrast)
			{
				effect_contrast = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.Desaturation)
			{
				effect_desaturation = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.Hue)
			{
				effect_hue = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.WaterDepth)
			{
				effect_waterdepth = roomEffect.amount;
			}
		}
		if (room.blizzardGraphics != null)
		{
			effect_desaturation = Mathf.Max(effect_desaturation, room.blizzardGraphics.BlizzardIntensity * 0.4f * room.roomSettings.RumbleIntensity);
			effect_brightness = Mathf.Max(effect_brightness, room.blizzardGraphics.BlizzardIntensity * 0.06f * room.roomSettings.RumbleIntensity);
		}
		UpdateDayNightPalette();
		if (effect_waterdepth != -1f)
		{
			Shader.SetGlobalFloat(RainWorld.ShadPropWaterDepth, effect_waterdepth);
		}
		if (ModManager.Expedition && game.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("bur-blinded"))
		{
			currentPalette.darkness = 0.6f;
			effect_darkness = 0.6f;
			effect_desaturation = 0.65f;
		}
		Creature creature = ((followAbstractCreature != null) ? followAbstractCreature.realizedCreature : null);
		if (creature != null && creature is Player)
		{
			mushroomMode = (creature as Player).Adrenaline;
			UpdatePlayerPosition(creature);
		}
		if (ModManager.MSC && game.IsStorySession && game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && room.world.region != null && room.world.region.name == "SB")
		{
			sofBlackFade = Math.Min(1f, sofBlackFade + 0.0025f);
		}
		else
		{
			sofBlackFade = 0f;
		}
		virtualMicrophone.Update();
		if (ModManager.CoopAvailable && game.IsStorySession && (!ModManager.MSC || (!game.rainWorld.safariMode && !game.wasAnArtificerDream)))
		{
			if (cameraTriggerCooldown > 0)
			{
				cameraTriggerCooldown--;
			}
			if ((followAbstractCreature == null || followAbstractCreature.realizedCreature == null || (followAbstractCreature.state is PlayerState && (followAbstractCreature.state as PlayerState).permaDead) || followAbstractCreature?.realizedCreature is Player { isNPC: not false }) && game.AlivePlayers.Count > 0)
			{
				AbstractCreature firstAlivePlayer = game.FirstAlivePlayer;
				if (firstAlivePlayer != null)
				{
					followAbstractCreature = firstAlivePlayer;
				}
			}
			if (cutscenePlayer != null)
			{
				if (cutsceneType == CameraCutsceneType.VoidSea)
				{
					Creature realizedCreature = cutscenePlayer.realizedCreature;
					if (realizedCreature != null && realizedCreature.Submersion > 0.5f)
					{
						followAbstractCreature = cutscenePlayer;
					}
					else
					{
						cutscenePlayer = null;
					}
				}
				else if (cutsceneType == CameraCutsceneType.Oracle)
				{
					Room realizedRoom = cutscenePlayer.Room.realizedRoom;
					if (realizedRoom != null && realizedRoom.physicalObjects.SelectMany((List<PhysicalObject> x) => x).Any((PhysicalObject x) => x is Oracle))
					{
						followAbstractCreature = cutscenePlayer;
					}
					else
					{
						cutscenePlayer = null;
					}
				}
				else if (cutsceneType == CameraCutsceneType.HunterStart)
				{
					followAbstractCreature = null;
				}
				else if (cutsceneType == CameraCutsceneType.EndingOE)
				{
					if (cutscenePlayer.Room.name == "OE_CAVE03" || cutscenePlayer.Room.name == "OE_PUMP01" || cutscenePlayer.Room.name == "OE_FINAL03")
					{
						followAbstractCreature = cutscenePlayer;
					}
					else
					{
						cutscenePlayer = null;
					}
				}
				else
				{
					cutscenePlayer = null;
				}
				if (cutscenePlayer?.realizedCreature?.dead ?? true)
				{
					cutscenePlayer = null;
				}
			}
			else
			{
				ExitCutsceneMode();
			}
			foreach (AbstractCreature player4 in game.Players)
			{
				Player player2 = (Player)player4.realizedCreature;
				if (player2 != null)
				{
					bool flag = RWInput.CheckSpecificButton(player2.playerState.playerNumber, 11);
					if (flag && (player2.stun != 0 || player2.dangerGrasp != null || player2.dead) && !room.game.cameras[0].InCutscene && (!player2.playerState.permaDead & !player2.requestedCameraWithoutInput))
					{
						JollyCustom.Log($"Player [{player2.playerState.playerNumber}] without control requesing the camera!!!");
						player2.TriggerCameraSwitch();
					}
					player2.requestedCameraWithoutInput = flag;
				}
			}
			if (followAbstractCreature != null)
			{
				if (followAbstractCreature.realizedCreature != null && followAbstractCreature.realizedCreature is Player player3 && hud?.owner != player3)
				{
					hud.owner = player3;
				}
				if (room.abstractRoom != followAbstractCreature.Room && followAbstractCreature.Room.realizedRoom != null)
				{
					JollyCustom.Log("Camera needs to move to new room!");
					MoveCamera(followAbstractCreature.Room.realizedRoom, -1);
				}
			}
		}
		GetCameraBestIndex();
		Shader.SetGlobalTexture(RainWorld.ShadPropPalTex, paletteTexture);
		paletteTexture.wrapMode = TextureWrapMode.Clamp;
		lastPos = pos;
		if (voidSeaMode)
		{
			if (followAbstractCreature.realizedCreature != null)
			{
				pos = followAbstractCreature.realizedCreature.mainBodyChunk.pos - new Vector2(700f, 400f);
			}
		}
		else
		{
			seekPos = CamPos(currentCameraPosition);
			seekPos.x += hDisplace + 8f;
			seekPos.y += 18f;
			seekPos += leanPos * 8f;
			pos = Vector2.Lerp(pos, seekPos, 0.1f);
			if (splitScreenMode && followAbstractCreature.realizedCreature != null)
			{
				pos.y = followAbstractCreature.realizedCreature.mainBodyChunk.pos.y - 384f;
			}
			pos.x = Mathf.Clamp(pos.x, CamPos(currentCameraPosition).x + hDisplace + 8f - 20f, CamPos(currentCameraPosition).x + hDisplace + 8f + 20f);
			pos.y = Mathf.Clamp(pos.y, CamPos(currentCameraPosition).y + 8f - 7f - (splitScreenMode ? 192f : 0f), CamPos(currentCameraPosition).y + 33f + (splitScreenMode ? 192f : 0f));
		}
		float num = screenShake;
		if (room.roomSettings.DangerType != RoomRain.DangerType.None && (!ModManager.MSC || room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard) && !room.abstractRoom.shelter && screenShake < room.world.rainCycle.ScreenShake)
		{
			screenShake = room.world.rainCycle.ScreenShake * room.roomSettings.RumbleIntensity;
			if (room.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard)
			{
				screenShake = room.world.rainCycle.ScreenShake * 1f;
			}
			num = screenShake * room.roomSettings.RainIntensity;
		}
		if (room.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard)
		{
			if (room.abstractRoom.shelter)
			{
				screenShake = Mathf.Lerp(screenShake, 0f, 0.1f);
				if (screenShake < 0.01f)
				{
					screenShake = 0f;
				}
			}
			else
			{
				screenShake = room.world.rainCycle.ScreenShake;
			}
			num = screenShake;
		}
		if (num > 0f)
		{
			num = ((!(num > 1f)) ? Mathf.Max(num - 0.025f, 0f) : Mathf.Max(num - 0.7f, 1f));
		}
		controllerShake = Mathf.InverseLerp(0.01f, 1.5f, num);
		if (ModManager.MMF && MMF.cfgDisableScreenShake.Value)
		{
			screenShake = 0f;
		}
		if (screenShake > 0f)
		{
			pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * 8f * screenShake;
			if (screenShake > 1f)
			{
				screenShake = Mathf.Max(screenShake - 0.7f, 1f);
			}
			else
			{
				screenShake = Mathf.Max(screenShake - 0.025f, 0f);
			}
		}
		microShake = Mathf.Max(microShake - 0.025f, (room.roomSettings.DangerType == RoomRain.DangerType.None || (ModManager.MSC && room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard) || room.abstractRoom.shelter) ? 0f : (room.world.rainCycle.MicroScreenShake * room.roomSettings.RumbleIntensity));
		if (room.waterFlux != null && room.waterFlux.roomShake > 0f && room.waterFlux.roomShake > microShake)
		{
			microShake = Mathf.Clamp(room.waterFlux.roomShake, 0f, 1f);
		}
		if (microShake > 2f)
		{
			microShake *= 0.8f;
		}
		if (ModManager.MMF && MMF.cfgDisableScreenShake.Value)
		{
			microShake = 0f;
		}
		if (fadeCoord != lastFadeCoord)
		{
			ApplyFade();
		}
		lastFadeCoord = fadeCoord;
		if (EXITNUMBERLABELS == null || !room.shortCutsReady)
		{
			return;
		}
		for (int j = 0; j < EXITNUMBERLABELS.GetLength(0); j++)
		{
			if (room.game.mapVisible)
			{
				EXITNUMBERLABELS[j, 0].isVisible = true;
				EXITNUMBERLABELS[j, 1].isVisible = true;
				Vector2 vector = new Vector2(0f, 0f);
				float num2 = 0f;
				if (room.abstractRoom.nodes[j].type == AbstractRoomNode.Type.Exit || room.abstractRoom.nodes[j].type == AbstractRoomNode.Type.Den || room.abstractRoom.nodes[j].type == AbstractRoomNode.Type.RegionTransportation)
				{
					vector = room.MiddleOfTile(room.ShortcutLeadingToNode(j).StartTile);
					num2 = 0f;
				}
				else if (room.abstractRoom.nodes[j].type == AbstractRoomNode.Type.SideExit || room.abstractRoom.nodes[j].type == AbstractRoomNode.Type.SkyExit || room.abstractRoom.nodes[j].type == AbstractRoomNode.Type.SeaExit)
				{
					vector = room.MiddleOfTile(room.borderExits[j - room.exitAndDenIndex.Length].borderTiles[room.borderExits[j - room.exitAndDenIndex.Length].borderTiles.Length / 2]);
					num2 = 0.3f;
				}
				else if (room.abstractRoom.nodes[j].type == AbstractRoomNode.Type.BatHive)
				{
					for (int k = 0; k < room.hives[j - room.exitAndDenIndex.Length - room.borderExits.Length].Length; k++)
					{
						vector += room.MiddleOfTile(room.hives[j - room.exitAndDenIndex.Length - room.borderExits.Length][k]);
					}
					vector /= (float)room.hives[j - room.exitAndDenIndex.Length - room.borderExits.Length].Length;
					num2 = 0.6f;
				}
				else if (room.abstractRoom.nodes[j].type == AbstractRoomNode.Type.GarbageHoles)
				{
					vector = room.MiddleOfTile(room.garbageHoles[UnityEngine.Random.Range(0, room.garbageHoles.Length)]);
				}
				if (room.abstractRoom.nodes[j].type == AbstractRoomNode.Type.Den)
				{
					EXITNUMBERLABELS[j, 0].color = Custom.HSL2RGB(num2 + UnityEngine.Random.value * 0.1f, 1f, 0.25f + UnityEngine.Random.value * 0.5f);
				}
				else
				{
					EXITNUMBERLABELS[j, 0].color = ((UnityEngine.Random.value < 0.5f) ? new Color(1f, 1f, 1f) : new Color(0f, 0f, 0f));
				}
				EXITNUMBERLABELS[j, 0].x = vector.x - pos.x;
				EXITNUMBERLABELS[j, 0].y = vector.y - pos.y;
				EXITNUMBERLABELS[j, 1].x = vector.x - pos.x + 1f;
				EXITNUMBERLABELS[j, 1].y = vector.y - pos.y - 1f;
			}
			else
			{
				EXITNUMBERLABELS[j, 0].isVisible = false;
				EXITNUMBERLABELS[j, 1].isVisible = false;
			}
		}
	}

	public void DrawUpdate(float timeStacker, float timeSpeed)
	{
		if (hud != null)
		{
			hud.Draw(timeStacker);
		}
		if (room == null)
		{
			return;
		}
		if (blizzardGraphics != null && room.blizzardGraphics == null)
		{
			blizzardGraphics.lerpBypass = true;
			room.AddObject(blizzardGraphics);
			room.blizzardGraphics = blizzardGraphics;
			room.blizzard = true;
		}
		bool flag = false;
		flag = fullscreenSync != Screen.fullScreen;
		if (snowChange || flag)
		{
			if (room.snow)
			{
				UpdateSnowLight();
			}
			if (blizzardGraphics != null)
			{
				blizzardGraphics.TileTexUpdate();
			}
		}
		fullscreenSync = Screen.fullScreen;
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		virtualMicrophone.DrawUpdate(timeStacker, timeSpeed);
		if (microShake > 0f)
		{
			vector += Custom.RNV() * 8f * microShake * UnityEngine.Random.value;
		}
		if (!voidSeaMode)
		{
			vector.x = Mathf.Clamp(vector.x, CamPos(currentCameraPosition).x + hDisplace + 8f - 20f, CamPos(currentCameraPosition).x + hDisplace + 8f + 20f);
			vector.y = Mathf.Clamp(vector.y, CamPos(currentCameraPosition).y + 8f - 7f - (splitScreenMode ? 192f : 0f), CamPos(currentCameraPosition).y + 33f + (splitScreenMode ? 192f : 0f));
			levelGraphic.isVisible = true;
			if (backgroundGraphic.isVisible)
			{
				backgroundGraphic.color = Color.Lerp(currentPalette.blackColor, currentPalette.fogColor, currentPalette.fogAmount);
			}
		}
		else
		{
			levelGraphic.isVisible = false;
			if (!ModManager.MSC || !room.waterInverted)
			{
				vector.y = Mathf.Min(vector.y, -528f);
			}
			else
			{
				vector.y = Mathf.Max(vector.y, room.PixelHeight + 128f);
			}
		}
		vector = new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
		vector.x -= 0.02f;
		vector.y -= 0.02f;
		vector += offset;
		vector += hardLevelGfxOffset;
		if (waterLight != null)
		{
			if (room.gameOverRoom)
			{
				waterLight.CleanOut();
			}
			else
			{
				waterLight.DrawUpdate(vector);
			}
		}
		for (int num = spriteLeasers.Count - 1; num >= 0; num--)
		{
			spriteLeasers[num].Update(timeStacker, this, vector);
			if (spriteLeasers[num].deleteMeNextFrame)
			{
				spriteLeasers.RemoveAt(num);
			}
		}
		for (int i = 0; i < singleCameraDrawables.Count; i++)
		{
			singleCameraDrawables[i].Draw(this, timeStacker, vector);
		}
		if (room.game.DEBUGMODE)
		{
			levelGraphic.x = 5000f;
		}
		else
		{
			levelGraphic.x = CamPos(currentCameraPosition).x - vector.x;
			levelGraphic.y = CamPos(currentCameraPosition).y - vector.y;
			backgroundGraphic.x = CamPos(currentCameraPosition).x - vector.x;
			backgroundGraphic.y = CamPos(currentCameraPosition).y - vector.y;
		}
		if (Futile.subjectToAspectRatioIrregularity)
		{
			int num2 = (int)(room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.PixelShift) * 8f);
			levelGraphic.x -= num2 % 3;
			backgroundGraphic.x -= num2 % 3;
			levelGraphic.y -= num2 / 3;
			backgroundGraphic.y -= num2 / 3;
		}
		shortcutGraphics.Draw(0f, vector);
		Shader.SetGlobalVector(RainWorld.ShadPropSpriteRect, new Vector4((0f - vector.x - 0.5f + CamPos(currentCameraPosition).x) / sSize.x, (0f - vector.y + 0.5f + CamPos(currentCameraPosition).y) / sSize.y, (0f - vector.x - 0.5f + levelGraphic.width + CamPos(currentCameraPosition).x) / sSize.x, (0f - vector.y + 0.5f + levelGraphic.height + CamPos(currentCameraPosition).y) / sSize.y));
		Shader.SetGlobalVector(RainWorld.ShadPropCamInRoomRect, new Vector4(vector.x / room.PixelWidth, vector.y / room.PixelHeight, sSize.x / room.PixelWidth, sSize.y / room.PixelHeight));
		Shader.SetGlobalVector(RainWorld.ShadPropScreenSize, sSize);
		if (!room.abstractRoom.gate && !room.abstractRoom.shelter)
		{
			float num3 = 0f;
			if (room.waterObject != null)
			{
				num3 = room.waterObject.fWaterLevel + 100f;
			}
			else if (room.deathFallGraphic != null)
			{
				num3 = room.deathFallGraphic.height + (ModManager.MMF ? 80f : 180f);
			}
			Shader.SetGlobalFloat(RainWorld.ShadPropWaterLevel, Mathf.InverseLerp(sSize.y, 0f, num3 - vector.y));
		}
		else
		{
			Shader.SetGlobalFloat(RainWorld.ShadPropWaterLevel, 0f);
		}
		float num4 = 1f;
		if (room.roomSettings.DangerType != RoomRain.DangerType.None)
		{
			num4 = room.world.rainCycle.ShaderLight;
		}
		if (room.lightning != null)
		{
			if (!room.lightning.bkgOnly)
			{
				num4 = room.lightning.CurrentLightLevel(timeStacker);
			}
			paletteTexture.SetPixel(0, 7, room.lightning.CurrentBackgroundColor(timeStacker, currentPalette));
			paletteTexture.SetPixel(1, 7, room.lightning.CurrentFogColor(timeStacker, currentPalette));
			paletteTexture.Apply();
		}
		if (room.roomSettings.Clouds == 0f)
		{
			Shader.SetGlobalFloat(RainWorld.ShadPropLight1, 1f);
		}
		else
		{
			Shader.SetGlobalFloat(RainWorld.ShadPropLight1, Mathf.Lerp(Mathf.Lerp(num4, -1f, room.roomSettings.Clouds), -0.4f, ghostMode));
		}
		Shader.SetGlobalFloat(RainWorld.ShadPropDarkness, 1f - effect_darkness);
		Shader.SetGlobalFloat(RainWorld.ShadPropBrightness, effect_brightness);
		Shader.SetGlobalFloat(RainWorld.ShadPropContrast, 1f + effect_contrast * 2f);
		Shader.SetGlobalFloat(RainWorld.ShadPropSaturation, 1f - effect_desaturation);
		Shader.SetGlobalFloat(RainWorld.ShadPropHue, 360f * effect_hue);
		Shader.SetGlobalFloat(RainWorld.ShadPropCloudsSpeed, 1f + 3f * ghostMode);
		if (lightBloomAlphaEffect != RoomSettings.RoomEffect.Type.None)
		{
			lightBloomAlpha = room.roomSettings.GetEffectAmount(lightBloomAlphaEffect);
		}
		if (lightBloomAlphaEffect == RoomSettings.RoomEffect.Type.VoidMelt && fullScreenEffect != null)
		{
			if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidSea) > 0f)
			{
				lightBloomAlpha *= voidSeaGoldFilter;
				fullScreenEffect.color = new Color(Mathf.InverseLerp(-1200f, -6000f, vector.y) * Mathf.InverseLerp(0.9f, 0f, screenShake), 0f, 0f);
				fullScreenEffect.isVisible = lightBloomAlpha > 0f;
			}
			else
			{
				fullScreenEffect.color = new Color(0f, 0f, 0f);
			}
		}
		if (fullScreenEffect != null)
		{
			if (lightBloomAlphaEffect == RoomSettings.RoomEffect.Type.Lightning)
			{
				fullScreenEffect.alpha = Mathf.InverseLerp(0f, 0.2f, lightBloomAlpha) * Mathf.InverseLerp(-0.7f, 0f, num4);
			}
			else if (lightBloomAlpha > 0f && (lightBloomAlphaEffect == RoomSettings.RoomEffect.Type.Bloom || lightBloomAlphaEffect == RoomSettings.RoomEffect.Type.SkyBloom || lightBloomAlphaEffect == RoomSettings.RoomEffect.Type.SkyAndLightBloom || lightBloomAlphaEffect == RoomSettings.RoomEffect.Type.LightBurn))
			{
				fullScreenEffect.alpha = lightBloomAlpha * Mathf.InverseLerp(-0.7f, 0f, num4);
			}
			else
			{
				fullScreenEffect.alpha = lightBloomAlpha;
			}
		}
		if (sofBlackFade > 0f && !voidSeaMode)
		{
			Shader.SetGlobalFloat(RainWorld.ShadPropDarkness, 1f - sofBlackFade);
		}
	}

	public Vector2 ApplyDepth(Vector2 ps, float depth)
	{
		return Custom.ApplyDepthOnVector(ps, CamPos(currentCameraPosition) + new Vector2(700f, 533.3334f), depth);
	}

	public void MoveCamera(int camPos)
	{
		Custom.Log("Change campos.", camPos.ToString());
		loadingCameraPos = camPos;
		loadingRoom = null;
		MoveCamera2(room.abstractRoom.name, camPos);
	}

	public void MoveCamera(Room newRoom, int camPos)
	{
		Custom.Log(newRoom.abstractRoom.name);
		Custom.Log("Change room. Camera position:", camPos.ToString());
		Shader.SetGlobalFloat(RainWorld.ShadPropGrime, newRoom.roomSettings.Grime);
		Shader.SetGlobalFloat(RainWorld.ShadPropWetTerrain, newRoom.roomSettings.wetTerrain ? 1f : 0f);
		if (newRoom.abstractRoom.swarmRoom)
		{
			Shader.SetGlobalFloat(RainWorld.ShadPropSwarmRoom, (newRoom.abstractRoom.swarmRoom && (!(newRoom.game.session is StoryGameSession) || newRoom.world.singleRoomWorld || game.world.regionState.SwarmRoomActive(newRoom.abstractRoom.swarmRoomIndex))) ? 1f : 0f);
		}
		if (ModManager.MSC && newRoom.roomSettings.GetEffectAmount(MoreSlugcatsEnums.RoomEffectType.BrokenPalette) != 0f)
		{
			levelGraphic.shader = FShader.defaultShader;
			ReturnFContainer("Foreground").RemoveChild(levelGraphic);
			ReturnFContainer("Background").RemoveChild(levelGraphic);
			ReturnFContainer("Background").AddChild(levelGraphic);
		}
		else if (newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt) > 0f)
		{
			levelGraphic.shader = game.rainWorld.Shaders["LevelMelt"];
			levelGraphic.alpha = newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt);
		}
		else if (newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.HeatWave) > 0f)
		{
			levelGraphic.shader = game.rainWorld.Shaders["LevelHeat"];
			levelGraphic.alpha = newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.HeatWave) / 2f;
		}
		else
		{
			levelGraphic.shader = game.rainWorld.Shaders["LevelColor"];
		}
		if (newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DirtyWater) > 0f)
		{
			Shader.EnableKeyword("Gutter");
		}
		else
		{
			Shader.DisableKeyword("Gutter");
		}
		virtualMicrophone.NewRoom(newRoom);
		if (camPos == -1)
		{
			camPos = 0;
		}
		loadingCameraPos = camPos;
		loadingRoom = newRoom;
		MoveCamera2(newRoom.abstractRoom.name, camPos);
		string path = WorldLoader.FindRoomFile(newRoom.abstractRoom.name, includeRootDirectory: true, "_" + (camPos + 1) + "_bkg.png");
		if (File.Exists(WorldLoader.FindRoomFile(newRoom.abstractRoom.name, includeRootDirectory: false, "_" + (camPos + 1) + "_bkg.png")))
		{
			preLoadedBKG = AssetManager.PreLoadTexture(path);
		}
		else
		{
			backgroundGraphic.isVisible = false;
		}
	}

	private void MoveCamera2(string roomName, int camPos)
	{
		string text = WorldLoader.FindRoomFile(roomName, includeRootDirectory: true, "_" + (camPos + 1) + ".png");
		string path = WorldLoader.FindRoomFile(roomName, includeRootDirectory: true, "_" + (camPos + 1) + "_bkg.png");
		if (File.Exists(WorldLoader.FindRoomFile(roomName, includeRootDirectory: false, "_" + (camPos + 1) + "_bkg.png")))
		{
			preLoadedBKG = AssetManager.PreLoadTexture(path);
			backgroundGraphic.shader = game.rainWorld.Shaders["Background"];
		}
		bool flag = preLoadedTexture != null;
		if (preLoadedBKG != null)
		{
			flag = false;
		}
		if (quenedTexture == text)
		{
			if (flag)
			{
				ApplyPositionChange();
			}
			else
			{
				applyPosChangeWhenTextureIsLoaded = true;
			}
		}
		else
		{
			preLoadedTexture = AssetManager.PreLoadTexture(text);
			applyPosChangeWhenTextureIsLoaded = true;
		}
	}

	public void GetCameraBestIndex()
	{
		Creature creature = ((followAbstractCreature != null) ? followAbstractCreature.realizedCreature : null);
		if (creature == null || room.cameraPositions.Length <= 1)
		{
			return;
		}
		leanPos *= 0.9f;
		if (creature is Player)
		{
			followCreatureInputForward.x += (creature as Player).input[0].x;
			followCreatureInputForward.y += (creature as Player).input[0].y;
		}
		followCreatureInputForward.x = Mathf.Clamp(followCreatureInputForward.x, -20f, 20f);
		followCreatureInputForward.y = Mathf.Clamp(followCreatureInputForward.y, -20f, 20f);
		followCreatureInputForward *= 0.995f;
		float num = 10000f;
		int num2 = -1;
		Vector2 testPos = creature.bodyChunks[0].pos + creature.bodyChunks[0].vel + followCreatureInputForward * 2f;
		Vector2 value = creature.bodyChunks[0].pos;
		if (creature.inShortcut)
		{
			Vector2? vector = room.game.shortcuts.OnScreenPositionOfInShortCutCreature(room, creature);
			if (vector.HasValue)
			{
				testPos = vector.Value;
				value = vector.Value;
			}
		}
		for (int i = 0; i < room.cameraPositions.Length; i++)
		{
			float num3 = CameraPositionDist(i, testPos, onlyInScreen: true, widescreen: false);
			if (num3 < num && num3 > 0f)
			{
				num = num3;
				num2 = i;
			}
		}
		if (num2 == -1)
		{
			for (int j = 0; j < room.cameraPositions.Length; j++)
			{
				float num4 = CameraPositionDist(j, testPos, onlyInScreen: true, widescreen: true);
				if (num4 < num && num4 > 0f)
				{
					num = num4;
					num2 = j;
				}
			}
		}
		num = 100000f;
		for (int k = 0; k < room.cameraPositions.Length; k++)
		{
			if (k != currentCameraPosition)
			{
				float num5 = CameraPositionDist(k, testPos, onlyInScreen: false, widescreen: false);
				if (num5 < num)
				{
					num = num5;
					mostLikelyNextCamPos = k;
				}
			}
		}
		if (num2 == -1 || num2 == currentCameraPosition || creature.abstractCreature.pos.room != room.abstractRoom.index)
		{
			return;
		}
		if (CameraPositionDist(currentCameraPosition, value, onlyInScreen: true, widescreen: false) < 0f)
		{
			MoveCamera(num2);
			return;
		}
		if (Mathf.Abs(CamPos(currentCameraPosition).x - CamPos(num2).x) > Mathf.Abs(CamPos(currentCameraPosition).y - CamPos(num2).y))
		{
			leanPos.x = ((CamPos(currentCameraPosition).x > CamPos(num2).x) ? (-1f) : 1f);
		}
		else
		{
			leanPos.y = ((CamPos(currentCameraPosition).y > CamPos(num2).y) ? (-1f) : 1f);
		}
		PreLoadTexture(room, num2);
	}

	public void PreLoadTexture(Room room, int camPos)
	{
		if (quenedTexture == "")
		{
			quenedTexture = WorldLoader.FindRoomFile(room.abstractRoom.name, includeRootDirectory: true, "_" + (camPos + 1) + ".png");
			preLoadedTexture = AssetManager.PreLoadTexture(quenedTexture);
		}
	}

	private void ApplyPositionChange()
	{
		levelTexture.LoadImage(preLoadedTexture, markNonReadable: false);
		quenedTexture = "";
		hardLevelGfxOffset = Vector2.zero;
		if (loadingRoom != null)
		{
			ChangeRoom(loadingRoom, loadingCameraPos);
		}
		room.camerasChangedTick++;
		mostLikelyNextCamPos = currentCameraPosition;
		currentCameraPosition = loadingCameraPos;
		if (room.roomSettings.fadePalette != null)
		{
			paletteBlend = room.roomSettings.fadePalette.fades[currentCameraPosition];
			ApplyFade();
		}
		seekPos = CamPos(currentCameraPosition);
		seekPos.x += hDisplace + 8f;
		seekPos.y += 18f;
		leanPos *= 0f;
		pos = seekPos;
		lastPos = seekPos;
		loadingRoom = null;
		loadingCameraPos = -1;
		applyPosChangeWhenTextureIsLoaded = false;
		UpdateGhostMode(room, currentCameraPosition);
		ApplyPalette();
		UpdateDayNightPalette();
		if (preLoadedBKG != null)
		{
			backgroundTexture.LoadImage(preLoadedBKG, markNonReadable: false);
			backgroundGraphic.isVisible = true;
		}
		else
		{
			backgroundGraphic.isVisible = false;
		}
	}

	private void ChangeRoom(Room newRoom, int cameraPosition)
	{
		if (room != null)
		{
			for (int i = 0; i < spriteLeasers.Count; i++)
			{
				spriteLeasers[i].CleanSpritesAndRemove();
			}
			spriteLeasers.Clear();
			spriteLeasers.TrimExcess();
			bool flag = false;
			for (int j = 0; j < game.cameras.Length; j++)
			{
				if (game.cameras[j] != this && game.cameras[j].room == room)
				{
					flag = true;
					break;
				}
			}
			if (!flag && newRoom != room)
			{
				room.NoLongerViewed();
			}
		}
		if (waterLight == null && newRoom.water)
		{
			waterLight = new WaterLight(this, newRoom.game.rainWorld.Shaders["WaterLight"]);
		}
		else if (waterLight != null && !newRoom.water)
		{
			waterLight.CleanOut();
			waterLight = null;
		}
		if (waterLight != null)
		{
			waterLight.NewRoom(newRoom.waterObject);
		}
		if (newRoom.roomSettings.fadePalette == null)
		{
			paletteBlend = 0f;
			ChangeMainPalette(newRoom.roomSettings.Palette);
		}
		else
		{
			ChangeBothPalettes(newRoom.roomSettings.Palette, newRoom.roomSettings.fadePalette.palette, newRoom.roomSettings.fadePalette.fades[cameraPosition]);
		}
		ApplyEffectColorsToAllPaletteTextures(newRoom.roomSettings.EffectColorA, newRoom.roomSettings.EffectColorB);
		if (!newRoom.BeingViewed)
		{
			newRoom.NowViewed();
		}
		room = newRoom;
		if (room.snowObject != null)
		{
			snowChange = true;
		}
		sofBlackFade = 0f;
		effect_dayNight = 0f;
		effect_darkness = 0f;
		effect_brightness = 0f;
		effect_contrast = 0f;
		effect_desaturation = 0f;
		effect_hue = 0f;
		effect_waterdepth = -1f;
		for (int k = 0; k < room.roomSettings.effects.Count; k++)
		{
			RoomSettings.RoomEffect roomEffect = room.roomSettings.effects[k];
			if (roomEffect.type == RoomSettings.RoomEffect.Type.DayNight)
			{
				effect_dayNight = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.Darkness)
			{
				effect_darkness = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.Brightness)
			{
				effect_brightness = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.Contrast)
			{
				effect_contrast = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.Desaturation)
			{
				effect_desaturation = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.Hue)
			{
				effect_hue = roomEffect.amount;
			}
			else if (roomEffect.type == RoomSettings.RoomEffect.Type.WaterDepth)
			{
				effect_waterdepth = roomEffect.amount;
			}
		}
		UpdateDayNightPalette();
		if (room.game.devToolsActive && ModManager.DevTools)
		{
			newRoom.AddObject(new DebugMouse());
		}
		Shader.SetGlobalTexture(RainWorld.ShadPropLevelTex, levelTexture);
		float num = (newRoom.waterInFrontOfTerrain ? 0f : 1f);
		if (newRoom.abstractRoom.gate)
		{
			num = 2f;
		}
		if (effect_waterdepth != -1f)
		{
			num = effect_waterdepth * 31f;
		}
		Shader.SetGlobalFloat(RainWorld.ShadPropWaterDepth, num / 31f);
		Shader.SetGlobalVector(RainWorld.ShadPropLightDirAndPixelSize, new Vector4(room.lightAngle.x, room.lightAngle.y, 0.0007142857f, 0.00125f));
		mostLikelyNextCamPos = currentCameraPosition;
		currentCameraPosition = cameraPosition;
		for (int l = 0; l < room.drawableObjects.Count; l++)
		{
			NewObjectInRoom(room.drawableObjects[l]);
		}
		if (blizzardGraphics != null && room.blizzardGraphics == null)
		{
			blizzardGraphics.lerpBypass = true;
			room.AddObject(blizzardGraphics);
			room.blizzardGraphics = blizzardGraphics;
			room.blizzard = true;
		}
		shortcutGraphics.NewRoom();
		RESETEXITLABELS();
	}

	private void RESETEXITLABELS()
	{
		if (EXITNUMBERLABELS != null)
		{
			for (int i = 0; i < EXITNUMBERLABELS.GetLength(0); i++)
			{
				EXITNUMBERLABELS[i, 0].RemoveFromContainer();
				EXITNUMBERLABELS[i, 1].RemoveFromContainer();
			}
		}
		EXITNUMBERLABELS = new FLabel[room.abstractRoom.nodes.Length, 2];
		for (int j = 0; j < room.abstractRoom.nodes.Length; j++)
		{
			EXITNUMBERLABELS[j, 0] = new FLabel(Custom.GetFont(), j.ToString());
			EXITNUMBERLABELS[j, 1] = new FLabel(Custom.GetFont(), j.ToString());
			EXITNUMBERLABELS[j, 1].color = new Color(0f, 0f, 0f);
			ReturnFContainer("HUD").AddChild(EXITNUMBERLABELS[j, 1]);
			ReturnFContainer("HUD").AddChild(EXITNUMBERLABELS[j, 0]);
		}
	}

	public void NewObjectInRoom(IDrawable obj)
	{
		spriteLeasers.Add(new SpriteLeaser(obj, this));
	}

	public void ChangeMainPalette(int palA)
	{
		LoadPalette(palA, ref fadeTexA);
		paletteA = palA;
		ApplyFade();
	}

	public void ChangeFadePalette(int palB, float blend)
	{
		bool flag = palB != paletteB;
		paletteB = palB;
		if (paletteB == -1)
		{
			blend = 0f;
			return;
		}
		if (flag)
		{
			LoadPalette(palB, ref fadeTexB);
		}
		paletteBlend = blend;
		ApplyFade();
	}

	public void ChangeBothPalettes(int palA, int palB, float blend)
	{
		if (palA != paletteA)
		{
			LoadPalette(palA, ref fadeTexA);
		}
		paletteA = palA;
		bool flag = palB != paletteB;
		paletteB = palB;
		if (paletteB == -1)
		{
			blend = 0f;
		}
		else
		{
			if (flag)
			{
				LoadPalette(palB, ref fadeTexB);
			}
			paletteBlend = blend;
		}
		ApplyFade();
	}

	public void FadeToPalette(int i, bool b, int i2)
	{
	}

	private void ApplyFade()
	{
		if (paletteB > -1)
		{
			for (int i = 0; i < 32; i++)
			{
				for (int j = 8; j < 16; j++)
				{
					paletteTexture.SetPixel(i, j - 8, Color.Lerp(Color.Lerp(fadeTexA.GetPixel(i, j), fadeTexA.GetPixel(i, j - 8), fadeCoord.y), Color.Lerp(fadeTexB.GetPixel(i, j), fadeTexB.GetPixel(i, j - 8), fadeCoord.y), fadeCoord.x));
				}
			}
		}
		else
		{
			for (int k = 0; k < 32; k++)
			{
				for (int l = 8; l < 16; l++)
				{
					paletteTexture.SetPixel(k, l - 8, Color.Lerp(fadeTexA.GetPixel(k, l), fadeTexA.GetPixel(k, l - 8), fadeCoord.y));
				}
			}
		}
		if (ghostMode > 0f)
		{
			for (int m = 0; m < 32; m++)
			{
				for (int n = 8; n < 16; n++)
				{
					Color pixel = paletteTexture.GetPixel(m, n - 8);
					pixel = Color.Lerp(pixel, new Color((pixel.r + pixel.g + pixel.b) / 3f, (pixel.r + pixel.g + pixel.b) / 3f, (pixel.r + pixel.g + pixel.b) / 3f), Mathf.Pow(ghostMode, 0.25f));
					paletteTexture.SetPixel(m, n - 8, Color.Lerp(pixel, ghostFadeTex.GetPixel(m, n), ghostMode * 0.9f));
				}
			}
		}
		if (mushroomMode > 0f)
		{
			for (int num = 0; num < 32; num++)
			{
				for (int num2 = 8; num2 < 16; num2++)
				{
					Color pixel2 = paletteTexture.GetPixel(num, num2 - 8);
					pixel2 = Color.Lerp(pixel2, new Color((pixel2.r + pixel2.g + pixel2.b) / 3f, (pixel2.r + pixel2.g + pixel2.b) / 3f, (pixel2.r + pixel2.g + pixel2.b) / 3f), mushroomMode * 0.5f);
					paletteTexture.SetPixel(num, num2 - 8, pixel2);
				}
			}
		}
		paletteTexture.Apply(updateMipmaps: false);
		ApplyPalette();
	}

	private void LoadPalette(int pal, ref Texture2D texture)
	{
		if (texture != null)
		{
			UnityEngine.Object.Destroy(texture);
		}
		texture = new Texture2D(32, 16, TextureFormat.ARGB32, mipChain: false);
		string text = AssetManager.ResolveFilePath("Palettes" + Path.DirectorySeparatorChar + "palette" + pal + ".png");
		try
		{
			AssetManager.SafeWWWLoadTexture(ref texture, "file:///" + text, clampWrapMode: false, crispPixels: true);
		}
		catch (FileLoadException)
		{
			text = AssetManager.ResolveFilePath("Palettes" + Path.DirectorySeparatorChar + "palette-1.png");
			AssetManager.SafeWWWLoadTexture(ref texture, "file:///" + text, clampWrapMode: false, crispPixels: true);
		}
		if (room != null)
		{
			ApplyEffectColorsToPaletteTexture(ref texture, room.roomSettings.EffectColorA, room.roomSettings.EffectColorB);
		}
		else
		{
			ApplyEffectColorsToPaletteTexture(ref texture, -1, -1);
		}
		texture.Apply(updateMipmaps: false);
	}

	public void ApplyEffectColorsToAllPaletteTextures(int color1, int color2)
	{
		ApplyEffectColorsToPaletteTexture(ref fadeTexA, color1, color2);
		if (paletteB > -1)
		{
			ApplyEffectColorsToPaletteTexture(ref fadeTexB, color1, color2);
		}
		ApplyFade();
	}

	private void ApplyEffectColorsToPaletteTexture(ref Texture2D texture, int color1, int color2)
	{
		if (color1 > -1)
		{
			texture.SetPixels(30, 4, 2, 2, allEffectColorsTexture.GetPixels(color1 * 2, 0, 2, 2, 0), 0);
			texture.SetPixels(30, 12, 2, 2, allEffectColorsTexture.GetPixels(color1 * 2, 2, 2, 2, 0), 0);
		}
		if (color2 > -1)
		{
			texture.SetPixels(30, 2, 2, 2, allEffectColorsTexture.GetPixels(color2 * 2, 0, 2, 2, 0), 0);
			texture.SetPixels(30, 10, 2, 2, allEffectColorsTexture.GetPixels(color2 * 2, 2, 2, 2, 0), 0);
		}
	}

	private void ApplyPalette()
	{
		currentPalette = new RoomPalette(paletteTexture, 1f - paletteTexture.GetPixel(9, 7).r, 1f - paletteTexture.GetPixel(30, 7).r, paletteTexture.GetPixel(2, 7), paletteTexture.GetPixel(4, 7), paletteTexture.GetPixel(5, 7), paletteTexture.GetPixel(6, 7), paletteTexture.GetPixel(7, 7), paletteTexture.GetPixel(8, 7), paletteTexture.GetPixel(1, 7), paletteTexture.GetPixel(0, 7), paletteTexture.GetPixel(10, 7), paletteTexture.GetPixel(11, 7), paletteTexture.GetPixel(12, 7), paletteTexture.GetPixel(13, 7));
		Color pixel = paletteTexture.GetPixel(9, 7);
		float value = 1f - pixel.r;
		if (pixel.r == 0f && pixel.g == 0f && pixel.b > 0f)
		{
			value = 1f + pixel.b;
		}
		Shader.SetGlobalFloat(RainWorld.ShadPropFogAmount, value);
		for (int i = 0; i < spriteLeasers.Count; i++)
		{
			spriteLeasers[i].UpdatePalette(this, currentPalette);
		}
		if (room != null && ghostMode > 0f)
		{
			SetUpFullScreenEffect("GrabShaders");
			fullScreenEffect.shader = game.rainWorld.Shaders["LevelMelt2"];
			fullScreenEffect.color = new Color(1f, 0f, 0f);
			lightBloomAlpha = Mathf.Pow(ghostMode, 0.5f) * 0.8f;
			lightBloomAlphaEffect = RoomSettings.RoomEffect.Type.None;
			fullScreenEffect.alpha = lightBloomAlpha;
		}
		else if (room != null && room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyBloom) > 0f)
		{
			SetUpFullScreenEffect("Bloom");
			fullScreenEffect.shader = game.rainWorld.Shaders["SkyBloom"];
			lightBloomAlpha = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyBloom);
			lightBloomAlphaEffect = RoomSettings.RoomEffect.Type.SkyBloom;
			fullScreenEffect.alpha = lightBloomAlpha;
		}
		else if (room != null && room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyAndLightBloom) > 0f)
		{
			SetUpFullScreenEffect("Bloom");
			fullScreenEffect.shader = game.rainWorld.Shaders["LightAndSkyBloom"];
			lightBloomAlpha = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyAndLightBloom);
			lightBloomAlphaEffect = RoomSettings.RoomEffect.Type.SkyAndLightBloom;
			fullScreenEffect.alpha = lightBloomAlpha;
		}
		else if (room != null && (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.LightBurn) > 0f || room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.Lightning) > 0f))
		{
			SetUpFullScreenEffect("Bloom");
			fullScreenEffect.shader = game.rainWorld.Shaders["LightBloom"];
			if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.Lightning) > 0f)
			{
				lightBloomAlphaEffect = RoomSettings.RoomEffect.Type.Lightning;
				lightBloomAlpha = 1f;
			}
			else
			{
				lightBloomAlphaEffect = RoomSettings.RoomEffect.Type.LightBurn;
				lightBloomAlpha = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.LightBurn);
			}
		}
		else if (room != null && room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.Fog) > 0f)
		{
			SetUpFullScreenEffect("Foreground");
			fullScreenEffect.shader = game.rainWorld.Shaders["Fog"];
			lightBloomAlphaEffect = RoomSettings.RoomEffect.Type.Fog;
			lightBloomAlpha = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.Fog);
		}
		else if (room != null && room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.Bloom) > 0f)
		{
			SetUpFullScreenEffect("Bloom");
			fullScreenEffect.shader = game.rainWorld.Shaders["Bloom"];
			lightBloomAlphaEffect = RoomSettings.RoomEffect.Type.Bloom;
			lightBloomAlpha = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.Bloom);
		}
		else if (room != null && room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt) > 0f)
		{
			SetUpFullScreenEffect("Bloom");
			fullScreenEffect.shader = game.rainWorld.Shaders["LevelMelt2"];
			lightBloomAlphaEffect = RoomSettings.RoomEffect.Type.VoidMelt;
			fullScreenEffect.alpha = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt);
		}
		else if (fullScreenEffect != null)
		{
			fullScreenEffect.RemoveFromContainer();
			fullScreenEffect = null;
		}
	}

	private void SetUpFullScreenEffect(string container)
	{
		if (fullScreenEffect == null)
		{
			fullScreenEffect = new FSprite("Futile_White");
			fullScreenEffect.scaleX = game.rainWorld.options.ScreenSize.x / 16f;
			fullScreenEffect.scaleY = 48f;
			fullScreenEffect.anchorX = 0f;
			fullScreenEffect.anchorY = 0f;
		}
		fullScreenEffect.RemoveFromContainer();
		ReturnFContainer(container).AddChild(fullScreenEffect);
	}

	public FContainer ReturnFContainer(string layerName)
	{
		return SpriteLayers[SpriteLayerIndex[layerName]];
	}

	private float CameraPositionDist(int camPos, Vector2 testPos, bool onlyInScreen, bool widescreen)
	{
		if (!onlyInScreen)
		{
			return Vector2.Distance(testPos, CamPos(camPos) + new Vector2(700f, 402f));
		}
		if (testPos.x > CamPos(camPos).x + 188f - (widescreen ? 190f : 0f) && testPos.x < CamPos(camPos).x + 188f + 1024f + (widescreen ? 190f : 0f) && testPos.y > CamPos(camPos).y + 18f && testPos.y < CamPos(camPos).y + 18f + 768f)
		{
			return Vector2.Distance(testPos, CamPos(camPos) + new Vector2(700f, 402f));
		}
		return -1f;
	}

	public bool IsViewedByCameraPosition(int camPos, Vector2 testPos)
	{
		Vector2 vector = CamPos(camPos);
		if (testPos.x > vector.x + 188f && testPos.x < vector.x + 188f + 1024f && testPos.y > vector.y + 18f)
		{
			return testPos.y < vector.y + 18f + 768f;
		}
		return false;
	}

	public bool IsVisibleAtCameraPosition(int camPos, Vector2 testPos)
	{
		Vector2 vector = CamPos(camPos);
		if (testPos.x > vector.x + 188f && testPos.x < vector.x + 188f + game.rainWorld.options.ScreenSize.x && testPos.y > vector.y + 18f)
		{
			return testPos.y < vector.y + 18f + 768f;
		}
		return false;
	}

	public int ViewedByCameraPosition(Vector2 v)
	{
		for (int i = 0; i < room.cameraPositions.Length; i++)
		{
			if (IsViewedByCameraPosition(i, v))
			{
				return i;
			}
		}
		return -1;
	}

	public bool RectCurrentlyVisible(Rect testRect, float margin, bool widescreen)
	{
		Rect otherRect = default(Rect);
		otherRect.xMin = CamPos(currentCameraPosition).x - 188f - margin - (widescreen ? 190f : 0f);
		otherRect.xMax = CamPos(currentCameraPosition).x + 188f + (ModManager.MMF ? game.rainWorld.options.ScreenSize.x : 1024f) + margin + (widescreen ? 190f : 0f);
		otherRect.yMin = CamPos(currentCameraPosition).y - 18f - margin;
		otherRect.yMax = CamPos(currentCameraPosition).y + 18f + 768f + margin;
		return testRect.CheckIntersect(otherRect);
	}

	public bool PositionCurrentlyVisible(Vector2 testPos, float margin, bool widescreen)
	{
		if (testPos.x > CamPos(currentCameraPosition).x - 188f - margin - (widescreen ? 190f : 0f) && testPos.x < CamPos(currentCameraPosition).x + 188f + (ModManager.MMF ? game.rainWorld.options.ScreenSize.x : 1024f) + margin + (widescreen ? 190f : 0f) && testPos.y > CamPos(currentCameraPosition).y - 18f - margin)
		{
			return testPos.y < CamPos(currentCameraPosition).y + 18f + 768f + margin;
		}
		return false;
	}

	public bool PositionVisibleInNextScreen(Vector2 testPos, float margin, bool widescreen)
	{
		if (mostLikelyNextCamPos < 0 || mostLikelyNextCamPos >= room.cameraPositions.Length)
		{
			return false;
		}
		if (testPos.x > CamPos(mostLikelyNextCamPos).x - 188f - margin - (widescreen ? 190f : 0f) && testPos.x < CamPos(mostLikelyNextCamPos).x + 188f + (ModManager.MMF ? game.rainWorld.options.ScreenSize.x : 1024f) + margin + (widescreen ? 190f : 0f) && testPos.y > CamPos(mostLikelyNextCamPos).y - 18f - margin)
		{
			return testPos.y < CamPos(mostLikelyNextCamPos).y + 18f + 768f + margin;
		}
		return false;
	}

	public void MoveObjectToInternalContainer(IDrawable obj, IDrawable containerObj, int container)
	{
		FContainer container2 = null;
		foreach (SpriteLeaser spriteLeaser in spriteLeasers)
		{
			if (spriteLeaser.drawableObject == containerObj)
			{
				container2 = spriteLeaser.containers[container];
				break;
			}
		}
		MoveObjectToContainer(obj, container2);
	}

	public void MoveObjectToContainer(IDrawable obj, FContainer container)
	{
		foreach (SpriteLeaser spriteLeaser in spriteLeasers)
		{
			if (spriteLeaser.drawableObject == obj)
			{
				spriteLeaser.AddSpritesToContainer(container, this);
				break;
			}
		}
	}

	public void AddSingleCameraDrawable(ISingleCameraDrawable obj)
	{
		if (!singleCameraDrawables.Contains(obj))
		{
			singleCameraDrawables.Add(obj);
		}
	}

	public void RemoveSingleCameraDrawable(ISingleCameraDrawable obj)
	{
		singleCameraDrawables.Remove(obj);
	}

	public void ScreenMovement(Vector2? sourcePos, Vector2 bump, float shake)
	{
		float num = 1f;
		if (sourcePos.HasValue)
		{
			num = 1f + Mathf.Lerp(DistanceFromViewedScreen(sourcePos.Value), 1f, 0.95f);
		}
		pos -= bump / num;
		shake /= num;
		screenMovementShake = shake;
		if (screenShake < shake)
		{
			screenShake = shake;
		}
		if (microShake < shake - 3f)
		{
			microShake = shake - 3f;
		}
		if (ModManager.MMF && MMF.cfgDisableScreenShake.Value)
		{
			screenShake = 0f;
			microShake = 0f;
			screenMovementShake = 0f;
		}
	}

	public float DistanceFromViewedScreen(Vector2 v)
	{
		return Vector2.Distance(v, Custom.RestrictInRect(v, new FloatRect(pos.x, pos.y, pos.x + game.rainWorld.options.ScreenSize.x, pos.y + 768f)));
	}

	public Color PixelColorAtCoordinate(Vector2 coord)
	{
		Vector2 vector = coord - CamPos(currentCameraPosition);
		Color pixel = levelTexture.GetPixel(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
		if (pixel.r == 1f && pixel.g == 1f && pixel.b == 1f)
		{
			return paletteTexture.GetPixel(0, 7);
		}
		int num = Mathf.FloorToInt(pixel.r * 255f);
		float t = 0f;
		if (num > 90)
		{
			num -= 90;
		}
		else
		{
			t = 1f;
		}
		int num2 = Mathf.FloorToInt((float)num / 30f);
		int num3 = (num - 1) % 30;
		return Color.Lerp(Color.Lerp(paletteTexture.GetPixel(num3, num2 + 3), paletteTexture.GetPixel(num3, num2), t), paletteTexture.GetPixel(1, 7), (float)num3 * (1f - paletteTexture.GetPixel(9, 7).r) / 30f);
	}

	public float DepthAtCoordinate(Vector2 coord)
	{
		Vector2 vector = coord - CamPos(currentCameraPosition);
		Color pixel = levelTexture.GetPixel(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
		if (pixel.r == 1f && pixel.g == 1f && pixel.b == 1f)
		{
			return 1f;
		}
		int num = Mathf.FloorToInt(pixel.r * 255f);
		if (num > 90)
		{
			num -= 90;
		}
		return (float)((num - 1) % 30) / 30f;
	}

	public void FireUpSinglePlayerHUD(Player player)
	{
		hud = new global::HUD.HUD(new FContainer[2]
		{
			ReturnFContainer("HUD"),
			ReturnFContainer("HUD2")
		}, game.rainWorld, player);
		hud.InitSinglePlayerHud(this);
		if (game.session is StoryGameSession && (game.session as StoryGameSession).saveState.cycleNumber > 0)
		{
			hud.foodMeter.visibleCounter = 200;
			hud.karmaMeter.forceVisibleCounter = 200;
			hud.rainMeter.remainVisibleCounter = 200;
		}
	}

	public void LoadGhostPalette(int gPal)
	{
		LoadPalette(gPal, ref ghostFadeTex);
	}

	public float PaletteDarkness()
	{
		return currentPalette.darkness;
	}

	private void UpdateGhostMode(Room newRoom, int newCamPos)
	{
		if (game.world.worldGhost != null)
		{
			if (ModManager.MMF)
			{
				game.world.worldGhost.CleanSeperationDistance();
			}
			ghostMode = game.world.worldGhost.GhostMode(newRoom, newCamPos);
			lightBloomAlpha = ghostMode * 0.8f;
		}
		else
		{
			ghostMode = 0f;
		}
	}

	public void SaintJourneyScreenshot()
	{
		if (!ModManager.MSC)
		{
			return;
		}
		if (saintSpamSafety > 0)
		{
			Custom.Log("Spam safety");
			return;
		}
		if (!room.game.IsStorySession || room.world.singleRoomWorld || room.game.rainWorld.safariMode)
		{
			Custom.Log("Not in a viable mode");
			return;
		}
		if (hud != null && hud.map != null && hud.map.fade > 0f)
		{
			Custom.Log("Map visible cancel screenshot");
			return;
		}
		if (room.abstractRoom.shelter || room.abstractRoom.gate)
		{
			Custom.Log("Not a viable room");
			return;
		}
		if (followAbstractCreature == null || followAbstractCreature.state.dead || followAbstractCreature.InDen || followAbstractCreature.realizedCreature == null || followAbstractCreature.realizedCreature.inShortcut)
		{
			Custom.Log("Player not in a viable state");
			return;
		}
		Custom.Log("Took saint journey screenshot");
		string text = Application.persistentDataPath + Path.DirectorySeparatorChar + "SJ_" + game.rainWorld.options.saveSlot;
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = text + Path.DirectorySeparatorChar + "karcap" + game.GetStorySession.saveState.deathPersistentSaveData.karmaCap + ".png";
		bool flag = !File.Exists(text2) || !(UnityEngine.Random.value > 0.2f);
		if (room.world.region.name == "HR")
		{
			Custom.Log("--HR disabled screenshot");
			flag = false;
		}
		if (flag)
		{
			Custom.Log("--Success! slot:", game.rainWorld.options.saveSlot.ToString(), "karcap:", game.GetStorySession.saveState.deathPersistentSaveData.karmaCap.ToString());
			ScreenCapture.CaptureScreenshot(text2);
		}
		saintSpamSafety = 500;
	}

	public void UpdatePlayerPosition(Creature creature)
	{
		Vector2 vector = creature.mainBodyChunk.pos;
		Shader.SetGlobalVector(value: new Vector2(vector.x / room.PixelWidth, vector.y / room.PixelHeight), nameID: RainWorld.ShadPropPlayerPos);
	}

	public void ReplaceDrawable(IDrawable oldDrawable, IDrawable newDrawable)
	{
		for (int i = 0; i < spriteLeasers.Count; i++)
		{
			if (spriteLeasers[i].drawableObject == oldDrawable)
			{
				spriteLeasers[i].CleanSpritesAndRemove();
				if (newDrawable != null)
				{
					NewObjectInRoom(newDrawable);
				}
				return;
			}
		}
		Custom.LogWarning("DRAWABLE WAS NOT FOUND ON CAMERA", cameraNumber.ToString(), ". CANNOT REPLACE.");
	}

	public void UpdateDayNightPalette()
	{
		if ((effect_dayNight > 0f && room.world.rainCycle.timer >= room.world.rainCycle.cycleLength) || (ModManager.Expedition && room.game.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("bur-blinded")))
		{
			float num = 1320f;
			float num2 = 1.47f;
			float num3 = 1.92f;
			if ((float)room.world.rainCycle.dayNightCounter < num)
			{
				if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.AboveCloudsView) > 0f && room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyAndLightBloom) > 0f)
				{
					room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.SkyAndLightBloom).amount = 0f;
				}
				float a = paletteBlend;
				paletteBlend = Mathf.Lerp(a, 1f, (float)room.world.rainCycle.dayNightCounter / num);
				ApplyFade();
				paletteBlend = a;
			}
			else if ((float)room.world.rainCycle.dayNightCounter == num)
			{
				ChangeBothPalettes(paletteB, room.world.rainCycle.duskPalette, 0f);
			}
			else if ((float)room.world.rainCycle.dayNightCounter < num * num2)
			{
				if (paletteBlend == 1f || paletteB != room.world.rainCycle.duskPalette || dayNightNeedsRefresh)
				{
					ChangeBothPalettes(paletteB, room.world.rainCycle.duskPalette, 0f);
				}
				paletteBlend = Mathf.InverseLerp(num, num * num2, room.world.rainCycle.dayNightCounter);
				ApplyFade();
			}
			else if ((float)room.world.rainCycle.dayNightCounter == num * num2)
			{
				ChangeBothPalettes(room.world.rainCycle.duskPalette, room.world.rainCycle.nightPalette, 0f);
			}
			else if ((float)room.world.rainCycle.dayNightCounter < num * num3)
			{
				if (paletteBlend == 1f || paletteB != room.world.rainCycle.nightPalette || paletteA != room.world.rainCycle.duskPalette || dayNightNeedsRefresh)
				{
					ChangeBothPalettes(room.world.rainCycle.duskPalette, room.world.rainCycle.nightPalette, 0f);
				}
				paletteBlend = Mathf.InverseLerp(num * num2, num * num3, room.world.rainCycle.dayNightCounter) * (effect_dayNight * 0.99f);
				ApplyFade();
			}
			else if ((float)room.world.rainCycle.dayNightCounter == num * num3)
			{
				ChangeBothPalettes(room.world.rainCycle.duskPalette, room.world.rainCycle.nightPalette, effect_dayNight * 0.99f);
			}
			else if ((float)room.world.rainCycle.dayNightCounter > num * num3)
			{
				if (paletteBlend == 1f || paletteB != room.world.rainCycle.nightPalette || paletteA != room.world.rainCycle.duskPalette || dayNightNeedsRefresh)
				{
					ChangeBothPalettes(room.world.rainCycle.duskPalette, room.world.rainCycle.nightPalette, effect_dayNight);
				}
				paletteBlend = effect_dayNight * 0.99f;
				ApplyFade();
			}
		}
		dayNightNeedsRefresh = false;
	}

	public void UpdateSnowLight()
	{
		int i = 0;
		int num = 0;
		int num2 = 0;
		Color[] array = (Color[])empty.Clone();
		float[] array2 = new float[4];
		for (; i < room.snowSources.Count; i++)
		{
			if (num >= 20)
			{
				break;
			}
			if (room.snowSources[i].visibility == 1)
			{
				Vector4[] array3 = room.snowSources[i].PackSnowData();
				array[num] = new Color(array3[0].x, array3[0].y, array3[0].z, array3[0].w);
				array[num + 20] = new Color(array3[1].x, array3[1].y, array3[1].z, array3[1].w);
				array2[num2] = array3[2].w;
				if (num2 == 3)
				{
					array[num / 4 + 40] = new Color(array2[0], array2[1], array2[2], array2[3]);
					num2 = 0;
					array2 = new float[4];
				}
				else
				{
					num2++;
				}
				num++;
			}
		}
		if (num2 > 0)
		{
			array[num / 4 + 40] = new Color(array2[0], array2[1], array2[2], array2[3]);
		}
		if (num > 0)
		{
			Shader.EnableKeyword("SNOW_ON");
		}
		else
		{
			Shader.DisableKeyword("SNOW_ON");
		}
		room.snowObject.visibleSnow = num;
		snowLightTex.SetPixels(array);
		snowLightTex.Apply();
		Graphics.Blit(levelTexture, SnowTexture, new Material(game.rainWorld.Shaders["LevelSnowShader"].shader));
		snowChange = false;
	}

	public void FireUpSafariHUD()
	{
		hud = new global::HUD.HUD(new FContainer[2]
		{
			ReturnFContainer("HUD"),
			ReturnFContainer("HUD2")
		}, room.game.rainWorld, followAbstractCreature.realizedCreature as Overseer);
		hud.InitSafariHud(this);
	}

	public void PausedDrawUpdate(float timeStacker, float timeSpeed)
	{
		if (room == null)
		{
			return;
		}
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		if (microShake > 0f)
		{
			vector += Custom.RNV() * 8f * microShake * UnityEngine.Random.value;
		}
		if (!voidSeaMode)
		{
			vector.x = Mathf.Clamp(vector.x, CamPos(currentCameraPosition).x + hDisplace + 8f - 20f, CamPos(currentCameraPosition).x + hDisplace + 8f + 20f);
			vector.y = Mathf.Clamp(vector.y, CamPos(currentCameraPosition).y + 8f - 7f - ((!splitScreenMode) ? 0f : 192f), CamPos(currentCameraPosition).y + 33f + ((!splitScreenMode) ? 0f : 192f));
		}
		else if (!room.waterInverted)
		{
			vector.y = Mathf.Min(vector.y, -528f);
		}
		else
		{
			vector.y = Mathf.Max(vector.y, room.PixelHeight + 128f);
		}
		vector = new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
		vector.x -= 0.02f;
		vector.y -= 0.02f;
		vector += offset;
		vector += hardLevelGfxOffset;
		for (int num = spriteLeasers.Count - 1; num >= 0; num--)
		{
			spriteLeasers[num].PausedUpdate(timeStacker, this, vector);
			if (spriteLeasers[num].deleteMeNextFrame)
			{
				spriteLeasers.RemoveAt(num);
			}
		}
	}
}
