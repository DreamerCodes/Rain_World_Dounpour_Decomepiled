using System;
using DevInterface;
using Expedition;
using HUD;
using JollyCoop;
using LizardCosmetics;
using Menu;
using MoreSlugcats;
using Music;
using OverseerHolograms;
using ScavengerCosmetic;
using ScavTradeInstruction;
using Smoke;
using VoidSea;

public static class ExtEnumInitializer
{
	public static void InitTypes()
	{
		_ = AbstractPhysicalObject.AbstractObjectType.Creature;
		_ = AbstractRoom.CreatureRoomAttraction.Neutral;
		_ = AbstractRoomNode.Type.Exit;
		_ = AbstractSpaceNodeFinder.Status.Flooding;
		_ = AbstractSpaceNodeFinder.SearchingFor.Den;
		_ = AbstractSpaceNodeFinder.FloodMethod.Cost;
		_ = AmbientSound.Type.Omnidirectional;
		_ = ArenaOverlay.Phase.Init;
		_ = ArenaSetup.GameTypeID.Competitive;
		_ = ArenaSetup.GameTypeSetup.WildLifeSetting.Off;
		_ = ArenaSetup.GameTypeSetup.DenEntryRule.Score;
		_ = BigEelAI.Behavior.Idle;
		_ = BigSpiderAI.Behavior.Idle;
		_ = Bubble.Mode.Growing;
		_ = CentipedeAI.Behavior.Idle;
		_ = CicadaAI.Behavior.Idle;
		_ = Conversation.ID.None;
		_ = ControllerHandler.RumbleState.Off;
		_ = CosmeticInsect.Type.StandardFly;
		_ = Creature.DamageType.Blunt;
		_ = Creature.Grasp.Shareability.NonExclusive;
		_ = CreatureCommunities.CommunityID.None;
		_ = CreatureTemplate.Type.Slugcat;
		_ = CreatureTemplate.Relationship.Type.DoesntTrack;
		_ = CreatureTemplate.WaterRelationship.AirOnly;
		_ = DaddyAI.Behavior.Idle;
		_ = DaddyTentacle.Task.Locomotion;
		_ = DeerAI.Behavior.Idle;
		_ = DartMaggot.Mode.Free;
		_ = DataPearl.AbstractDataPearl.DataPearlType.Misc;
		_ = DeathPersistentSaveData.Tutorial.GoExplore;
		_ = DevUISignalType.ButtonClick;
		_ = DreamsState.DreamID.MoonFriend;
		_ = DropBugAI.Behavior.Idle;
		_ = EggBugAI.Behavior.Idle;
		_ = EndCredits.Stage.InitialWait;
		_ = EventTrigger.TriggerType.Spot;
		_ = FliesWorldAI.Behavior.Inactive;
		_ = FloatTweener.TweenType.None;
		_ = Fly.MovementMode.BatFlight;
		_ = FlyAI.DropStatus.Dropping;
		_ = FlyAI.Behavior.Idle;
		_ = RegionGateGraphics.Clamp.Mode.Stacked;
		_ = GhostWorldPresence.GhostID.CC;
		_ = GlobalRain.DeathRain.DeathRainMode.None;
		_ = HardmodeStart.Phase.Init;
		_ = global::HUD.HUD.OwnerType.Player;
		_ = HUDCircle.SnapToGraphic.None;
		_ = InGameTranslator.LanguageID.English;
		_ = JetFishAI.Behavior.Idle;
		_ = KarmaLadder.Phase.Resting;
		_ = KingTusks.Tusk.Mode.Attached;
		_ = Limb.Mode.HuntRelativePosition;
		_ = Lizard.Animation.Standard;
		_ = LizardAI.LizardCommunication.Agression;
		_ = LizardAI.Behavior.Idle;
		_ = LizardCosmetics.Template.SpritesOverlap.Behind;
		_ = LizardTongue.State.Hidden;
		_ = LizardVoice.Emotion.SpottedPreyFirstTime;
		_ = LocalizationTranslator.TranslationProcess.FindingCoordinates;
		_ = global::Menu.Menu.MenuColors.White;
		_ = MenuDepthIllustration.MenuShader.Normal;
		_ = MenuScene.SceneID.Empty;
		_ = MirosBirdAI.Behavior.Idle;
		_ = MouseAI.Behavior.Idle;
		_ = MultiplayerResults.Phase.Init;
		_ = MultiplayerUnlocks.LevelUnlockID.Default;
		_ = MultiplayerUnlocks.SandboxUnlockID.Slugcat;
		_ = MusicPlayer.MusicContext.Menu;
		_ = NeedleWormAI.Behavior.Idle;
		_ = NodeFinder.Status.Working;
		_ = NSHSwarmer.Shape.ShapeType.Main;
		_ = OddJobAIModule.Tag.MouseDanglePosFinder;
		_ = Options.ControlSetup.Preset.None;
		_ = Oracle.OracleID.SS;
		_ = Overseer.Mode.Watching;
		_ = OverseerCommunicationModule.PlayerConcern.None;
		_ = GateScene.SceneID.MoonAndSlugcats;
		_ = GateScene.SubScene.SubSceneID.Slugcats;
		_ = GateScene.SubScene.GateSceneActor.ActorID.Moon;
		_ = OverseerHologram.Message.None;
		_ = OverseerImage.ImageID.Moon_Full_Figure;
		_ = PhysicalObject.BodyChunkConnection.Type.Normal;
		_ = PlacedObject.Type.None;
		_ = PlacedObject.LightFixtureData.Type.RedLight;
		_ = PlacedObject.LightSourceData.ColorType.Environment;
		_ = PlacedObject.MultiplayerItemData.Type.Rock;
		_ = Player.AnimationIndex.None;
		_ = Player.BodyModeIndex.Crawl;
		_ = ProcessManager.ProcessID.MainMenu;
		_ = ProcessManager.MenuSetup.StoryGameInitCondition.Dev;
		_ = RegionGate.Mode.MiddleClosed;
		_ = ReliableIggyDirection.ReliableIggyDirectionData.Condition.AnyTime;
		_ = ReliableIggyDirection.ReliableIggyDirectionData.Symbol.Shelter;
		_ = Room.SlopeDirection.UpLeft;
		_ = RoomAttractivenessPanel.Category.All;
		_ = RoomRain.DangerType.Rain;
		_ = RoomSettings.RoomEffect.Type.None;
		_ = RoomSettingSlider.Type.RainIntensity;
		_ = SandboxEditorSelector.ActionButton.Action.ClearAll;
		_ = Scavenger.MovementMode.Run;
		_ = Scavenger.ScavengerAnimation.ID.Rummage;
		_ = ScavengerAI.Behavior.Idle;
		_ = ScavengerAI.ViolenceType.None;
		_ = ScavengerAbstractAI.ScavengerSquad.MissionID.None;
		_ = BackDecals.Pattern.SpineRidge;
		_ = ScavTradeInputInstructionController.Phase.None;
		_ = ShortcutData.Type.Normal;
		_ = SlideShow.SlideShowID.WhiteIntro;
		_ = Menu.Slider.SliderID.SfxVol;
		_ = SLOracleBehaviorHasMark.MiscItemType.NA;
		_ = SLOracleBehaviorHasMark.PauseReason.Annoyance;
		_ = SLOrcacleState.PlayerOpinion.NotSpeaking;
		_ = SLOracleWakeUpProcedure.Phase.LookingForSwarmer;
		_ = SlugcatStats.Name.White;
		_ = SmokeSystem.SmokeType.VultureSmoke;
		_ = SocialEventRecognizer.EventID.LethalAttackAttempt;
		_ = SoundID.None;
		_ = SoundPage.SoundTypes.Omnidirectional;
		_ = Weapon.Mode.Free;
		_ = SSOracleBehavior.MovementBehavior.Idle;
		_ = SSOracleSwarmer.MovementMode.Swarm;
		_ = StationaryEffect.EffectType.FlashingOrb;
		_ = StopMusicEvent.Type.AllSongs;
		_ = StoryGameStatisticsScreen.TickerID.Food;
		_ = StoryGameStatisticsScreen.TickMode.OnlyTicker;
		_ = TextPrompt.InfoID.Nothing;
		_ = TileVisualizer.vizType.block;
		_ = TubeWorm.Tongue.Mode.Retracted;
		_ = TriggeredEvent.EventType.MusicEvent;
		_ = Weapon.Mode.Free;
		_ = WinState.EndgameID.Survivor;
		_ = WorldLoader.Activity.Init;
		_ = VoidSeaScene.DeepDivePhase.Start;
		_ = VoidWorm.MainWormBehavior.Phase.Idle;
		_ = VultureAI.Behavior.Idle;
		_ = VultureTentacle.Mode.Climb;
		_ = YellowAI.YellowPack.Role.Leader;
		_ = LightBeam.LightBeamData.BlinkType.None;
		_ = ObjectsPage.DevObjectCategories.Gameplay;
		_ = RoomSettingsPage.DevEffectsCategories.Gameplay;
		_ = FairyParticle.LerpMethod.SIN_IO;
		_ = MenuIllustration.CrossfadeType.Standard;
		_ = PlacedObject.EnergySwirlData.ColorType.Environment;
		_ = PlacedObject.FairyParticleData.SpriteType.Pixel;
		_ = PlacedObject.LightSourceData.BlinkType.None;
		_ = PlacedObject.SnowSourceData.Shape.None;
		_ = RegionGate.GateRequirement.OneKarma;
		if (ModManager.MMF)
		{
			MMFEnums.InitExtEnumTypes();
		}
		if (ModManager.MSC)
		{
			MoreSlugcatsEnums.InitExtEnumTypes();
		}
		if (ModManager.JollyCoop)
		{
			JollyEnums.InitExtEnumTypes();
		}
		if (ModManager.Expedition)
		{
			ExpeditionEnums.InitExtEnumTypes();
		}
	}

	public static bool IsExtEnum(this Type type)
	{
		return type.IsSubclassOf(typeof(ExtEnumBase));
	}
}
