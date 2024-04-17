using System.Collections.Generic;

namespace MoreSlugcats;

public class MMF
{
	public static string MOD_ID = "rwremix";

	public static Configurable<bool> cfgSpeedrunTimer;

	public static Configurable<bool> cfgHideRainMeterNoThreat;

	public static Configurable<bool> cfgLoadingScreenTips;

	public static Configurable<bool> cfgExtraTutorials;

	public static Configurable<bool> cfgClearerDeathGradients;

	public static Configurable<bool> cfgShowUnderwaterShortcuts;

	public static Configurable<bool> cfgBreathTimeVisualIndicator;

	public static Configurable<bool> cfgCreatureSense;

	public static Configurable<bool> cfgTickTock;

	public static Configurable<bool> cfgFastMapReveal;

	public static Configurable<bool> cfgThreatMusicPulse;

	public static Configurable<bool> cfgExtraLizardSounds;

	public static Configurable<bool> cfgVulnerableJellyfish;

	public static Configurable<bool> cfgNewDynamicDifficulty;

	public static Configurable<bool> cfgSurvivorPassageNotRequired;

	public static Configurable<bool> cfgIncreaseStuns;

	public static Configurable<bool> cfgUpwardsSpearThrow;

	public static Configurable<bool> cfgDislodgeSpears;

	public static Configurable<bool> cfgAlphaRedLizards;

	public static Configurable<bool> cfgSandboxItemStems;

	public static Configurable<bool> cfgNoArenaFleeing;

	public static Configurable<bool> cfgVanillaExploits;

	public static Configurable<bool> cfgOldTongue;

	public static Configurable<bool> cfgWallpounce;

	public static Configurable<bool> cfgFasterShelterOpen;

	public static Configurable<bool> cfgQuieterGates;

	public static Configurable<bool> cfgSwimBreathLeniency;

	public static Configurable<bool> cfgJetfishItemProtection;

	public static Configurable<bool> cfgKeyItemTracking;

	public static Configurable<bool> cfgKeyItemPassaging;

	public static Configurable<bool> cfgScavengerKillSquadDelay;

	public static Configurable<bool> cfgDeerBehavior;

	public static Configurable<bool> cfgHunterBatflyAutograb;

	public static Configurable<bool> cfgHunterBackspearProtect;

	public static Configurable<bool> cfgDisableScreenShake;

	public static Configurable<bool> cfgClimbingGrip;

	public static Configurable<bool> cfgNoMoreTinnitus;

	public static Configurable<bool> cfgMonkBreathTime;

	public static Configurable<bool> cfgLargeHologramLight;

	public static Configurable<bool> cfgGraspWiggling;

	public static Configurable<bool> cfgFreeSwimBoosts;

	public static Configurable<bool> cfgNoRandomCycles;

	public static Configurable<bool> cfgSafeCentipedes;

	public static Configurable<int> cfgHunterCycles;

	public static Configurable<int> cfgHunterBonusCycles;

	public static Configurable<float> cfgSlowTimeFactor;

	public static Configurable<bool> cfgGlobalMonkGates;

	public static Configurable<bool> cfgDisableGateKarma;

	public static Configurable<float> cfgRainTimeMultiplier;

	public static List<MMFPreset<bool>> boolPresets;

	public static List<MMFPreset<int>> intPresets;

	public static List<MMFPreset<float>> floatPresets;

	public static void OnInit()
	{
		OptionInterface optionInterface = new MMFOptionInterface();
		MachineConnector.SetRegisteredOI(MOD_ID, optionInterface);
		boolPresets = new List<MMFPreset<bool>>();
		intPresets = new List<MMFPreset<int>>();
		floatPresets = new List<MMFPreset<float>>();
		optionInterface.config.configurables.Clear();
		cfgSurvivorPassageNotRequired = optionInterface.config.Bind("cfgSurvivorPassageNotRequired", defaultValue: true, new ConfigurableInfo("Allows certain passages to gain progress even before the Survivor passage has been achieved", null, "", "Passage progress without Survivor"));
		boolPresets.Add(new MMFPreset<bool>(cfgSurvivorPassageNotRequired, remixValue: true, classicValue: false, casualValue: true));
		cfgQuieterGates = optionInterface.config.Bind("cfgQuieterGates", defaultValue: false, new ConfigurableInfo("Reduces the volume for the sound effects of gates and shelters opening", null, "", "Quieter gates/shelters"));
		boolPresets.Add(new MMFPreset<bool>(cfgQuieterGates, remixValue: false, classicValue: false, casualValue: false));
		cfgNoMoreTinnitus = optionInterface.config.Bind("cfgNoMoreTinnitus", defaultValue: false, new ConfigurableInfo("Stops nearby explosions causing a ringing sound to play", null, "", "No Ringing in Ears"));
		boolPresets.Add(new MMFPreset<bool>(cfgNoMoreTinnitus, remixValue: false, classicValue: false, casualValue: false));
		cfgNoRandomCycles = optionInterface.config.Bind("cfgNoRandomCycles", defaultValue: false, new ConfigurableInfo("All cycles will have the same duration, and will always use the longest duration possible", null, "", "No randomized cycle durations"));
		boolPresets.Add(new MMFPreset<bool>(cfgNoRandomCycles, remixValue: false, classicValue: false, casualValue: true));
		cfgCreatureSense = optionInterface.config.Bind("cfgCreatureSense", defaultValue: true, new ConfigurableInfo("Show icons of nearby creatures on the minimap", null, "", "Slug Senses"));
		boolPresets.Add(new MMFPreset<bool>(cfgCreatureSense, remixValue: true, classicValue: true, casualValue: true));
		cfgFastMapReveal = optionInterface.config.Bind("cfgFastMapReveal", defaultValue: true, new ConfigurableInfo("Increase the speed that the minimap reveals itself", null, "", "Fast map reveal"));
		boolPresets.Add(new MMFPreset<bool>(cfgFastMapReveal, remixValue: true, classicValue: false, casualValue: true));
		cfgNoArenaFleeing = optionInterface.config.Bind("cfgNoArenaFleeing", defaultValue: true, new ConfigurableInfo("Prevent injured creatures from fleeing to dens in arena mode", null, "", "Arena creatures cannot flee"));
		boolPresets.Add(new MMFPreset<bool>(cfgNoArenaFleeing, remixValue: true, classicValue: false, casualValue: true));
		cfgHunterBatflyAutograb = optionInterface.config.Bind("cfgHunterBatflyAutograb", defaultValue: true, new ConfigurableInfo("Stops Hunter from automatically grabbing batflies with a free hand", null, "", "No Hunter batfly auto-grabbing"));
		boolPresets.Add(new MMFPreset<bool>(cfgHunterBatflyAutograb, remixValue: true, classicValue: false, casualValue: true));
		cfgHunterBackspearProtect = optionInterface.config.Bind("cfgHunterBackspearProtect", defaultValue: true, new ConfigurableInfo("Stops Scavengers from being able to steal the spear off of Hunter's back", null, "", "No stealing Hunter back-spear"));
		boolPresets.Add(new MMFPreset<bool>(cfgHunterBackspearProtect, remixValue: true, classicValue: false, casualValue: true));
		cfgLoadingScreenTips = optionInterface.config.Bind("cfgLoadingScreenTips", defaultValue: true, new ConfigurableInfo("Periodically shows tips and tutorials during the loading screen between cycles", null, "", "Loading screen tips"));
		boolPresets.Add(new MMFPreset<bool>(cfgLoadingScreenTips, remixValue: true, classicValue: false, casualValue: true));
		cfgExtraTutorials = optionInterface.config.Bind("cfgExtraTutorials", defaultValue: true, new ConfigurableInfo("Introduces a few additional in-game tutorial messages for certain mechanics and scenarios", null, "", "Extra tutorials"));
		boolPresets.Add(new MMFPreset<bool>(cfgExtraTutorials, remixValue: true, classicValue: true, casualValue: true));
		cfgIncreaseStuns = optionInterface.config.Bind("cfgIncreaseStuns", defaultValue: false, new ConfigurableInfo("Increases the amount of time that rocks and snail pops can cause some creatures to be stunned for", null, "", "Increased stun times"));
		boolPresets.Add(new MMFPreset<bool>(cfgIncreaseStuns, remixValue: false, classicValue: false, casualValue: true));
		cfgShowUnderwaterShortcuts = optionInterface.config.Bind("cfgShowUnderwaterShortcuts", defaultValue: true, new ConfigurableInfo("Prevents underwater shortcut symbols from being obscured by the water layer", null, "", "Show underwater shortcuts"));
		boolPresets.Add(new MMFPreset<bool>(cfgShowUnderwaterShortcuts, remixValue: true, classicValue: true, casualValue: true));
		cfgGraspWiggling = optionInterface.config.Bind("cfgGraspWiggling", defaultValue: true, new ConfigurableInfo("Gives a chance of escaping from different grasps by rapidly wiggling with the movement buttons", null, "", "Wiggle out of grasps"));
		boolPresets.Add(new MMFPreset<bool>(cfgGraspWiggling, remixValue: true, classicValue: false, casualValue: true));
		cfgJetfishItemProtection = optionInterface.config.Bind("cfgJetfishItemProtection", defaultValue: true, new ConfigurableInfo("Prevents Jetfish from being able to knock items out of your hands", null, "", "Jetfish item protection"));
		boolPresets.Add(new MMFPreset<bool>(cfgJetfishItemProtection, remixValue: true, classicValue: true, casualValue: true));
		cfgKeyItemPassaging = optionInterface.config.Bind("cfgKeyItemPassaging", defaultValue: true, new ConfigurableInfo("Passages bring all key items in a shelter with you to the new destination, rather than just the stomach item", null, "", "Key items on Passage"));
		boolPresets.Add(new MMFPreset<bool>(cfgKeyItemPassaging, remixValue: true, classicValue: false, casualValue: true));
		cfgKeyItemTracking = optionInterface.config.Bind("cfgKeyItemTracking", defaultValue: true, new ConfigurableInfo("Key items are tracked on the map and will respawn on subsequent cycles if they are lost, at the location they were lost", null, "", "Key item tracking"));
		boolPresets.Add(new MMFPreset<bool>(cfgKeyItemTracking, remixValue: true, classicValue: true, casualValue: true));
		cfgSafeCentipedes = optionInterface.config.Bind("cfgSafeCentipedes", defaultValue: true, new ConfigurableInfo("Centipedes will release you if you go through a pipe while you are grabbed by one", null, "", "Centipede pipe protection"));
		boolPresets.Add(new MMFPreset<bool>(cfgSafeCentipedes, remixValue: true, classicValue: true, casualValue: true));
		cfgMonkBreathTime = optionInterface.config.Bind("cfgMonkBreathTime", defaultValue: true, new ConfigurableInfo("Increases the amount of underwater breath time that Monk has", null, "", "Monk extended breath"));
		boolPresets.Add(new MMFPreset<bool>(cfgMonkBreathTime, remixValue: true, classicValue: false, casualValue: true));
		cfgLargeHologramLight = optionInterface.config.Bind("cfgLargeHologramLight", defaultValue: false, new ConfigurableInfo("Increases the radius of Monk's hologram light in pitch-black areas", null, "", "Monk extra light assistance"));
		boolPresets.Add(new MMFPreset<bool>(cfgLargeHologramLight, remixValue: false, classicValue: false, casualValue: true));
		cfgNewDynamicDifficulty = optionInterface.config.Bind("cfgNewDynamicDifficulty", defaultValue: true, new ConfigurableInfo("Dynamic difficulty is influenced by number of regions visited more than number of cycles survived", null, "", "New dynamic difficulty"));
		boolPresets.Add(new MMFPreset<bool>(cfgNewDynamicDifficulty, remixValue: true, classicValue: true, casualValue: true));
		cfgScavengerKillSquadDelay = optionInterface.config.Bind("cfgScavengerKillSquadDelay", defaultValue: true, new ConfigurableInfo("Give a grace period on cycle start and region entry before scavenger kill squads can attack you", null, "", "Scavenger kill squad leniency"));
		boolPresets.Add(new MMFPreset<bool>(cfgScavengerKillSquadDelay, remixValue: true, classicValue: false, casualValue: true));
		cfgClimbingGrip = optionInterface.config.Bind("cfgClimbingGrip", defaultValue: true, new ConfigurableInfo("Prevents falling off from poles when throwing objects", null, "", "Stronger climbing grip"));
		boolPresets.Add(new MMFPreset<bool>(cfgClimbingGrip, remixValue: true, classicValue: false, casualValue: true));
		cfgSwimBreathLeniency = optionInterface.config.Bind("cfgSwimBreathLeniency", defaultValue: true, new ConfigurableInfo("Increases the amount of time before you are forced to come up for air while drowning", null, "", "Breath time leniency"));
		boolPresets.Add(new MMFPreset<bool>(cfgSwimBreathLeniency, remixValue: true, classicValue: true, casualValue: true));
		cfgFreeSwimBoosts = optionInterface.config.Bind("cfgFreeSwimBoosts", defaultValue: false, new ConfigurableInfo("Swim boosting will not consume additional breath time", null, "", "No swim boost penalty"));
		boolPresets.Add(new MMFPreset<bool>(cfgFreeSwimBoosts, remixValue: false, classicValue: false, casualValue: true));
		cfgHunterCycles = optionInterface.config.Bind("cfgHunterCycles", 20, new ConfigurableInfo("Changes the amount of cycles Hunter starts with in their campaign", new ConfigAcceptableRange<int>(1, 9999), "", "Hunter Cycles"));
		intPresets.Add(new MMFPreset<int>(cfgHunterCycles, 20, 20, 20));
		cfgHunterBonusCycles = optionInterface.config.Bind("cfgHunterBonusCycles", 5, new ConfigurableInfo("Changes the amount of bonus cycles Hunter can receive during their campaign", new ConfigAcceptableRange<int>(0, 9999), "", "Hunter Bonus Cycles"));
		intPresets.Add(new MMFPreset<int>(cfgHunterBonusCycles, 5, 5, 5));
		cfgRainTimeMultiplier = optionInterface.config.Bind("cfgRainTimeMultiplier", 1f, new ConfigurableInfo("Multiplies the total duration of the rain timer by this amount", new ConfigAcceptableRange<float>(0.25f, 10f), "", "Rain Timer Multiplier"));
		floatPresets.Add(new MMFPreset<float>(cfgRainTimeMultiplier, 1f, 1f, 2f));
		cfgSlowTimeFactor = optionInterface.config.Bind("cfgSlowTimeFactor", 1f, new ConfigurableInfo("Reduces the overall speed of the game to assist with reaction times", new ConfigAcceptableRange<float>(1f, 5f), "", "Slow Motion Factor"));
		floatPresets.Add(new MMFPreset<float>(cfgSlowTimeFactor, 1f, 1f, 1f));
		cfgThreatMusicPulse = optionInterface.config.Bind("cfgThreatMusicPulse", defaultValue: false, new ConfigurableInfo("Shows a visual pulse on screen when threats are nearby", null, "", "Threat music visual pulse"));
		boolPresets.Add(new MMFPreset<bool>(cfgThreatMusicPulse, remixValue: false, classicValue: false, casualValue: false));
		cfgClearerDeathGradients = optionInterface.config.Bind("cfgClearerDeathGradients", defaultValue: false, new ConfigurableInfo("Makes the death gradients on bottomless pits more visible", null, "", "Stronger bottomless pit indicators"));
		boolPresets.Add(new MMFPreset<bool>(cfgClearerDeathGradients, remixValue: false, classicValue: false, casualValue: true));
		cfgGlobalMonkGates = optionInterface.config.Bind("cfgGlobalMonkGates", defaultValue: false, new ConfigurableInfo("For all campaigns, gates will remain open permanently after passing through them once", null, "", "Monk-style gates for all campaigns"));
		boolPresets.Add(new MMFPreset<bool>(cfgGlobalMonkGates, remixValue: false, classicValue: false, casualValue: true));
		cfgDisableGateKarma = optionInterface.config.Bind("cfgDisableGateKarma", defaultValue: false, new ConfigurableInfo("Allow passing any gate for free, regardless of your current karma level", null, "", "Disable all karma requirements"));
		boolPresets.Add(new MMFPreset<bool>(cfgDisableGateKarma, remixValue: false, classicValue: false, casualValue: false));
		cfgBreathTimeVisualIndicator = optionInterface.config.Bind("cfgBreathTimeVisualIndicator", defaultValue: false, new ConfigurableInfo("Show a visual indicator on the UI of your remaining breath time", null, "", "Visual breath meter"));
		boolPresets.Add(new MMFPreset<bool>(cfgBreathTimeVisualIndicator, remixValue: false, classicValue: false, casualValue: true));
		cfgOldTongue = optionInterface.config.Bind("cfgOldTongue", defaultValue: false, new ConfigurableInfo("Use the throw button to activate tongue controls rather than the jump button", null, "", "Legacy tongue controls"));
		boolPresets.Add(new MMFPreset<bool>(cfgOldTongue, remixValue: false, classicValue: false, casualValue: false));
		cfgDislodgeSpears = optionInterface.config.Bind("cfgDislodgeSpears", defaultValue: false, new ConfigurableInfo("Gives the player the ability to dislodge spears that are embedded in walls", null, "", "Pull spears from walls"));
		boolPresets.Add(new MMFPreset<bool>(cfgDislodgeSpears, remixValue: false, classicValue: false, casualValue: true));
		cfgDisableScreenShake = optionInterface.config.Bind("cfgDisableScreenShake", defaultValue: false, new ConfigurableInfo("Removes visual effects that cause the screen to shake", null, "", "Reduce screen shaking"));
		boolPresets.Add(new MMFPreset<bool>(cfgDisableScreenShake, remixValue: false, classicValue: false, casualValue: false));
		cfgSpeedrunTimer = optionInterface.config.Bind("cfgSpeedrunTimer", defaultValue: false, new ConfigurableInfo("The current session playtime will always be visible as a UI element in-game", null, "", "Speedrun timer"));
		boolPresets.Add(new MMFPreset<bool>(cfgSpeedrunTimer, remixValue: false, classicValue: false, casualValue: false));
		cfgDeerBehavior = optionInterface.config.Bind("cfgDeerBehavior", defaultValue: true, new ConfigurableInfo("Adds additional deer behavior, like being able to influence their behavior by wiggling in their antlers", null, "", "Tweaked deer behavior"));
		boolPresets.Add(new MMFPreset<bool>(cfgDeerBehavior, remixValue: true, classicValue: true, casualValue: true));
		cfgUpwardsSpearThrow = optionInterface.config.Bind("cfgUpwardsSpearThrow", defaultValue: true, new ConfigurableInfo("Gives additional options for the directions and ways that objects can be thrown in certain circumstances", null, "", "Upwards spear throwing"));
		boolPresets.Add(new MMFPreset<bool>(cfgUpwardsSpearThrow, remixValue: true, classicValue: true, casualValue: true));
		cfgExtraLizardSounds = optionInterface.config.Bind("cfgExtraLizardSounds", defaultValue: true, new ConfigurableInfo("All lizard breeds have unique sound effects and voices", null, "", "Additional lizard voices"));
		boolPresets.Add(new MMFPreset<bool>(cfgExtraLizardSounds, remixValue: true, classicValue: true, casualValue: true));
		cfgWallpounce = optionInterface.config.Bind("cfgWallpounce", defaultValue: true, new ConfigurableInfo("Enable the wall pounce mechanic", null, "", "Wall pouncing"));
		boolPresets.Add(new MMFPreset<bool>(cfgWallpounce, remixValue: true, classicValue: false, casualValue: false));
		cfgTickTock = optionInterface.config.Bind("cfgTickTock", defaultValue: true, new ConfigurableInfo("The rain timer makes a tick-tock noise while the UI is visible", null, "", "Rain timer tick-tock"));
		boolPresets.Add(new MMFPreset<bool>(cfgTickTock, remixValue: true, classicValue: false, casualValue: true));
		cfgHideRainMeterNoThreat = optionInterface.config.Bind("cfgHideRainMeterNoThreat", defaultValue: true, new ConfigurableInfo("Hide the remaining cycle time while in maps that are safe from the rain", null, "", "Hide rain timer in safe areas"));
		boolPresets.Add(new MMFPreset<bool>(cfgHideRainMeterNoThreat, remixValue: true, classicValue: true, casualValue: true));
		cfgAlphaRedLizards = optionInterface.config.Bind("cfgAlphaRedLizards", defaultValue: true, new ConfigurableInfo("Red Lizards have a tongue and can snap spears in half with their bite", null, "", "Alpha red lizards"));
		boolPresets.Add(new MMFPreset<bool>(cfgAlphaRedLizards, remixValue: true, classicValue: false, casualValue: false));
		cfgSandboxItemStems = optionInterface.config.Bind("cfgSandboxItemStems", defaultValue: true, new ConfigurableInfo("Items that typically attach to the terrain with a stem will spawn with their stems when placed in Sandbox mode", null, "", "Item stems in sandbox"));
		boolPresets.Add(new MMFPreset<bool>(cfgSandboxItemStems, remixValue: true, classicValue: true, casualValue: true));
		cfgVulnerableJellyfish = optionInterface.config.Bind("cfgVulnerableJellyfish", defaultValue: true, new ConfigurableInfo("Jellyfish are vulnerable to being stabbed", null, "", "Vulnerable jellyfish"));
		boolPresets.Add(new MMFPreset<bool>(cfgVulnerableJellyfish, remixValue: true, classicValue: false, casualValue: true));
		cfgVanillaExploits = optionInterface.config.Bind("cfgVanillaExploits", defaultValue: false, new ConfigurableInfo("Advantageous glitches and speedrunner exploits are made available to use again with this option", null, "", "Vanilla exploits"));
		boolPresets.Add(new MMFPreset<bool>(cfgVanillaExploits, remixValue: false, classicValue: true, casualValue: true));
		cfgFasterShelterOpen = optionInterface.config.Bind("cfgFasterShelterOpen", defaultValue: false, new ConfigurableInfo("Reduces the amount of time taken to get started with a new cycle", null, "", "Faster shelter opening"));
		boolPresets.Add(new MMFPreset<bool>(cfgFasterShelterOpen, remixValue: false, classicValue: false, casualValue: true));
	}

	public static void OnDisable(ProcessManager manager)
	{
	}
}
