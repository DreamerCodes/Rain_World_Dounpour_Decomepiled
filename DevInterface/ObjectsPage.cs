using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class ObjectsPage : Page
{
	public class DevObjectCategories : ExtEnum<DevObjectCategories>
	{
		public static readonly DevObjectCategories Gameplay = new DevObjectCategories("Gameplay", register: true);

		public static readonly DevObjectCategories Consumable = new DevObjectCategories("Consumable", register: true);

		public static readonly DevObjectCategories CoralBrain = new DevObjectCategories("CoralBrain", register: true);

		public static readonly DevObjectCategories Decoration = new DevObjectCategories("Decoration", register: true);

		public static readonly DevObjectCategories Tutorial = new DevObjectCategories("Tutorial", register: true);

		public static readonly DevObjectCategories Unsorted = new DevObjectCategories("Unsorted", register: true);

		public DevObjectCategories(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public PlacedObject.LightFixtureData.Type lastPlacedLightFixture = PlacedObject.LightFixtureData.Type.RedLight;

	public PlacedObjectRepresentation draggedObject;

	public PlacedObjectRepresentation removeIfReleaseObject;

	private TrashBin trashBin;

	private Panel objectsPanel;

	public PlacedObject.Type[] placedObjectTypes;

	public int maxObjectsPerPage = 20;

	public int currObjectsPage;

	public int totalObjectsPages;

	public ObjectsPage(DevUI owner, string IDstring, DevUINode parentNode, string name)
		: base(owner, IDstring, parentNode, name)
	{
		objectsPanel = new Panel(owner, "Objects_Panel", this, new Vector2(550f, 80f), new Vector2(400f, 260f), "OBJECTS: ");
		subNodes.Add(objectsPanel);
		for (int i = 0; i < 2; i++)
		{
			objectsPanel.subNodes.Add(new Button(owner, (i != 0) ? "Next_Button" : "Prev_Button", objectsPanel, new Vector2(5f + 100f * (float)i, objectsPanel.size.y - 16f - 5f), 95f, (i != 0) ? "Next Page" : "Previous Page"));
		}
		trashBin = new TrashBin(owner, "Trash_Bin", this, new Vector2(40f, 40f));
		subNodes.Add(trashBin);
		AssembleObjectPages();
		RefreshObjectsPage();
	}

	public void RefreshObjectsPage()
	{
		if (totalObjectsPages == 0)
		{
			currObjectsPage = 0;
		}
		for (int num = objectsPanel.subNodes.Count - 1; num >= 2; num--)
		{
			objectsPanel.subNodes[num].ClearSprites();
			objectsPanel.subNodes.RemoveAt(num);
		}
		int num2 = currObjectsPage * maxObjectsPerPage;
		int i = 0;
		objectsPanel.Title = "OBJECTS: ...";
		for (; i < maxObjectsPerPage && i + num2 < placedObjectTypes.Length; i++)
		{
			float num3 = (float)maxObjectsPerPage / 2f;
			float num4 = (float)Mathf.FloorToInt((float)i / num3) * 195f;
			float num5 = 5f;
			num5 += num4;
			float num6 = 20f * num3 * (float)Mathf.FloorToInt((float)i / num3);
			float num7 = objectsPanel.size.y - 16f - 35f - 20f * (float)i;
			num7 += num6;
			if (i == 0)
			{
				objectsPanel.Title = "OBJECTS: " + DevObjectGetCategoryFromPlacedType(placedObjectTypes[num2 + i]);
			}
			if (placedObjectTypes[num2 + i] != PlacedObject.Type.None)
			{
				objectsPanel.subNodes.Add(new AddObjectButton(owner, objectsPanel, new Vector2(num5, num7), 190f, placedObjectTypes[num2 + i]));
			}
		}
	}

	public override void Update()
	{
		draggedObject = null;
		base.Update();
		if (draggedObject != null && trashBin.MouseOver)
		{
			trashBin.LineColor = ((Random.value < 0.5f) ? new Color(1f, 0f, 0f) : new Color(1f, 1f, 1f));
			removeIfReleaseObject = draggedObject;
			return;
		}
		trashBin.LineColor = new Color(1f, 1f, 1f);
		if (!owner.mouseDown && removeIfReleaseObject != null)
		{
			RemoveObject(removeIfReleaseObject);
		}
		removeIfReleaseObject = null;
	}

	public override void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (type == DevUISignalType.ButtonClick)
		{
			switch (sender.IDstring)
			{
			case "Save_Settings":
				base.RoomSettings.Save();
				break;
			case "Export_Sandbox":
				(owner.game.GetArenaGameSession as SandboxGameSession).editor.DevToolsExportConfig();
				break;
			case "Prev_Button":
				currObjectsPage--;
				if (currObjectsPage < 0)
				{
					currObjectsPage = totalObjectsPages - 1;
				}
				RefreshObjectsPage();
				break;
			case "Next_Button":
				currObjectsPage++;
				if (currObjectsPage >= totalObjectsPages)
				{
					currObjectsPage = 0;
				}
				RefreshObjectsPage();
				break;
			case "Save_Specific":
				base.RoomSettings.Save(owner.game.GetStorySession.saveStateNumber);
				break;
			}
		}
		else if (type == DevUISignalType.Create)
		{
			CreateObjRep(new PlacedObject.Type(message), null);
		}
	}

	private void RemoveObject(PlacedObjectRepresentation objRep)
	{
		base.RoomSettings.placedObjects.Remove(objRep.pObj);
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();
		for (int i = 0; i < base.RoomSettings.placedObjects.Count; i++)
		{
			CreateObjRep(base.RoomSettings.placedObjects[i].type, base.RoomSettings.placedObjects[i]);
		}
	}

	private void CreateObjRep(PlacedObject.Type tp, PlacedObject pObj)
	{
		if (pObj == null)
		{
			pObj = new PlacedObject(tp, null);
			pObj.pos = owner.room.game.cameras[0].pos + Vector2.Lerp(owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f;
			base.RoomSettings.placedObjects.Add(pObj);
			if (tp == PlacedObject.Type.LightFixture)
			{
				(pObj.data as PlacedObject.LightFixtureData).type = lastPlacedLightFixture;
			}
		}
		PlacedObjectRepresentation placedObjectRepresentation = null;
		placedObjectRepresentation = ((tp == PlacedObject.Type.LightSource) ? new LightSourceRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.LightFixture) ? new LightFixtureRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((!(tp == PlacedObject.Type.CoralCircuit) && !(tp == PlacedObject.Type.CoralStem) && !(tp == PlacedObject.Type.CoralStemWithNeurons) && !(tp == PlacedObject.Type.Corruption) && !(tp == PlacedObject.Type.CorruptionDarkness) && !(tp == PlacedObject.Type.StuckDaddy) && !(tp == PlacedObject.Type.WallMycelia) && !(tp == PlacedObject.Type.ProjectedStars) && !(tp == PlacedObject.Type.CentipedeAttractor) && !(tp == PlacedObject.Type.DandelionPatch) && !(tp == PlacedObject.Type.NoSpearStickZone) && !(tp == PlacedObject.Type.TradeOutpost) && !(tp == PlacedObject.Type.ScavengerTreasury) && !(tp == PlacedObject.Type.CosmeticSlimeMold) && !(tp == PlacedObject.Type.CosmeticSlimeMold2) && !(tp == PlacedObject.Type.PlayerPushback) && !(tp == PlacedObject.Type.NoLeviathanStrandingZone) && !(tp == PlacedObject.Type.NeuronSpawner)) ? ((!(tp == PlacedObject.Type.CoralNeuron) && !(tp == PlacedObject.Type.CorruptionTube) && !(tp == PlacedObject.Type.LanternOnStick) && !(tp == PlacedObject.Type.ScavTradeInstruction) && !(tp == PlacedObject.Type.DeadTokenStalk) && !(tp == PlacedObject.Type.Vine)) ? ((!(tp == PlacedObject.Type.ZapCoil) && !(tp == PlacedObject.Type.SuperStructureFuses)) ? ((!(tp == PlacedObject.Type.SpotLight) && !(tp == PlacedObject.Type.SuperJumpInstruction)) ? ((tp == PlacedObject.Type.DeepProcessing) ? new DeepProcessingRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.SSLightRod) ? new SSLightRodRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.ScavengerOutpost) ? new ScavOutpostRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((!(tp == PlacedObject.Type.DangleFruit) && !(tp == PlacedObject.Type.DataPearl) && !(tp == PlacedObject.Type.UniqueDataPearl) && !(tp == PlacedObject.Type.SeedCob) && !(tp == PlacedObject.Type.FlareBomb) && !(tp == PlacedObject.Type.PuffBall) && !(tp == PlacedObject.Type.WaterNut) && !(tp == PlacedObject.Type.JellyFish) && !(tp == PlacedObject.Type.KarmaFlower) && !(tp == PlacedObject.Type.Mushroom) && !(tp == PlacedObject.Type.VoidSpawnEgg) && !(tp == PlacedObject.Type.FirecrackerPlant) && !(tp == PlacedObject.Type.VultureGrub) && !(tp == PlacedObject.Type.DeadVultureGrub) && !(tp == PlacedObject.Type.SlimeMold) && !(tp == PlacedObject.Type.FlyLure) && !(tp == PlacedObject.Type.SporePlant) && !(tp == PlacedObject.Type.NeedleEgg) && !(tp == PlacedObject.Type.BubbleGrass) && !(tp == PlacedObject.Type.Hazer) && !(tp == PlacedObject.Type.DeadHazer) && !(tp == PlacedObject.Type.Lantern)) ? ((!(tp == PlacedObject.Type.ExitSymbolHidden) && !(tp == PlacedObject.Type.ExitSymbolShelter)) ? ((tp == PlacedObject.Type.CustomDecal) ? new CustomDecalRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.InsectGroup) ? new InsectGroupRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.MultiplayerItem) ? new MultiplayerItemRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((!(tp == PlacedObject.Type.BlueToken) && !(tp == PlacedObject.Type.GoldToken)) ? ((tp == PlacedObject.Type.Filter) ? new FilterRepresentation(owner, tp.ToString() + "_Rep", this, pObj) : ((tp == PlacedObject.Type.ReliableIggyDirection) ? new ReliableDirectionRep(owner, tp.ToString() + "_Rep", this, pObj) : ((tp == PlacedObject.Type.Rainbow) ? new RainbowRepresentation(owner, tp.ToString() + "_Rep", this, pObj) : ((tp == PlacedObject.Type.LightBeam) ? new LightBeamRepresentation(owner, tp.ToString() + "_Rep", this, pObj) : ((tp == PlacedObject.Type.ExitSymbolAncientShelter) ? new TileObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.FairyParticleSettings) ? new FairyParticleRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.DayNightSettings) ? new DayNightRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.LightningMachine) ? new LightningMachineRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.EnergySwirl) ? new EnergySwirlRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.SteamPipe || tp == PlacedObject.Type.WallSteamer) ? new SteamPipeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.SnowSource) ? new SnowSourceRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.CellDistortion) ? new CellDistortionRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((tp == PlacedObject.Type.LocalBlizzard) ? new LocalBlizzardRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((!ModManager.MSC) ? new PlacedObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : ((!(tp == MoreSlugcatsEnums.PlacedObjectType.Germinator) && !(tp == MoreSlugcatsEnums.PlacedObjectType.GooieDuck) && !(tp == MoreSlugcatsEnums.PlacedObjectType.LillyPuck) && !(tp == MoreSlugcatsEnums.PlacedObjectType.GlowWeed) && !(tp == MoreSlugcatsEnums.PlacedObjectType.MoonCloak) && !(tp == MoreSlugcatsEnums.PlacedObjectType.DandelionPeach) && !(tp == MoreSlugcatsEnums.PlacedObjectType.HRGuard)) ? ((!(tp == MoreSlugcatsEnums.PlacedObjectType.MSArteryPush) && !(tp == MoreSlugcatsEnums.PlacedObjectType.BigJellyFish) && !(tp == MoreSlugcatsEnums.PlacedObjectType.RotFlyPaper) && !(tp == MoreSlugcatsEnums.PlacedObjectType.KarmaShrine) && !(tp == MoreSlugcatsEnums.PlacedObjectType.Stowaway)) ? ((!(tp == MoreSlugcatsEnums.PlacedObjectType.GreenToken) && !(tp == MoreSlugcatsEnums.PlacedObjectType.WhiteToken) && !(tp == MoreSlugcatsEnums.PlacedObjectType.RedToken) && !(tp == MoreSlugcatsEnums.PlacedObjectType.DevToken)) ? ((!(tp == MoreSlugcatsEnums.PlacedObjectType.OEsphere)) ? new PlacedObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()) : new OEsphereRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString())) : new TokenRepresentation(owner, tp.ToString() + "_Rep", this, pObj)) : new ResizeableObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: true)) : new ConsumableRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString())))))))))))))))) : new TokenRepresentation(owner, tp.ToString() + "_Rep", this, pObj))))) : new TileObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString())) : new ConsumableRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString()))))) : new QuadObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString())) : new GridRectObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString())) : new ResizeableObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: false)) : new ResizeableObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: true))));
		if (placedObjectRepresentation != null)
		{
			tempNodes.Add(placedObjectRepresentation);
			subNodes.Add(placedObjectRepresentation);
		}
	}

	public DevObjectCategories DevObjectGetCategoryFromPlacedType(PlacedObject.Type type)
	{
		if (type == PlacedObject.Type.LightSource || type == PlacedObject.Type.LightFixture || type == PlacedObject.Type.SpotLight || type == PlacedObject.Type.LightBeam || type == PlacedObject.Type.CustomDecal || type == PlacedObject.Type.Rainbow || type == PlacedObject.Type.FairyParticleSettings || type == PlacedObject.Type.DandelionPatch || type == PlacedObject.Type.LanternOnStick || type == PlacedObject.Type.InsectGroup || type == PlacedObject.Type.BrokenShelterWaterLevel || type == PlacedObject.Type.SnowSource || type == PlacedObject.Type.LocalBlizzard || type == PlacedObject.Type.DayNightSettings || type == PlacedObject.Type.LightningMachine || type == PlacedObject.Type.SteamPipe || type == PlacedObject.Type.WallSteamer || type == PlacedObject.Type.BlinkingFlower || type == PlacedObject.Type.CellDistortion || type == PlacedObject.Type.EnergySwirl)
		{
			return DevObjectCategories.Decoration;
		}
		if (type == PlacedObject.Type.Filter || type == PlacedObject.Type.NoSpearStickZone || type == PlacedObject.Type.PlayerPushback || type == PlacedObject.Type.CentipedeAttractor || type == PlacedObject.Type.VoidSpawnEgg || type == PlacedObject.Type.ScavengerOutpost || type == PlacedObject.Type.TradeOutpost || type == PlacedObject.Type.ScavengerTreasury || type == PlacedObject.Type.ExitSymbolShelter || type == PlacedObject.Type.ExitSymbolAncientShelter || type == PlacedObject.Type.ExitSymbolHidden || type == PlacedObject.Type.NoLeviathanStrandingZone || type == PlacedObject.Type.NeuronSpawner || type == PlacedObject.Type.DeathFallFocus || type == PlacedObject.Type.Vine)
		{
			return DevObjectCategories.Gameplay;
		}
		if (type == PlacedObject.Type.ReliableSpear || type == PlacedObject.Type.DataPearl || type == PlacedObject.Type.UniqueDataPearl || type == PlacedObject.Type.DangleFruit || type == PlacedObject.Type.WaterNut || type == PlacedObject.Type.FlareBomb || type == PlacedObject.Type.PuffBall || type == PlacedObject.Type.JellyFish || type == PlacedObject.Type.KarmaFlower || type == PlacedObject.Type.Mushroom || type == PlacedObject.Type.SlimeMold || type == PlacedObject.Type.CosmeticSlimeMold || type == PlacedObject.Type.CosmeticSlimeMold2 || type == PlacedObject.Type.FlyLure || type == PlacedObject.Type.FirecrackerPlant || type == PlacedObject.Type.VultureGrub || type == PlacedObject.Type.DeadVultureGrub || type == PlacedObject.Type.SporePlant || type == PlacedObject.Type.NeedleEgg || type == PlacedObject.Type.BubbleGrass || type == PlacedObject.Type.Hazer || type == PlacedObject.Type.DeadHazer || type == PlacedObject.Type.HangingPearls || type == PlacedObject.Type.Lantern || type == PlacedObject.Type.VultureMask || type == PlacedObject.Type.SeedCob || type == PlacedObject.Type.DeadSeedCob)
		{
			return DevObjectCategories.Consumable;
		}
		if (type == PlacedObject.Type.CoralStem || type == PlacedObject.Type.CoralStemWithNeurons || type == PlacedObject.Type.CoralNeuron || type == PlacedObject.Type.CoralCircuit || type == PlacedObject.Type.WallMycelia || type == PlacedObject.Type.ProjectedStars || type == PlacedObject.Type.SuperStructureFuses || type == PlacedObject.Type.DeepProcessing || type == PlacedObject.Type.GravityDisruptor || type == PlacedObject.Type.ZapCoil || type == PlacedObject.Type.SSLightRod || type == PlacedObject.Type.Corruption || type == PlacedObject.Type.CorruptionTube || type == PlacedObject.Type.CorruptionDarkness || type == PlacedObject.Type.StuckDaddy)
		{
			return DevObjectCategories.CoralBrain;
		}
		if (type == PlacedObject.Type.SuperJumpInstruction || type == PlacedObject.Type.ProjectedImagePosition || type == PlacedObject.Type.ScavTradeInstruction || type == PlacedObject.Type.ReliableIggyDirection || type == PlacedObject.Type.DeadTokenStalk || type == PlacedObject.Type.BlueToken || type == PlacedObject.Type.GoldToken)
		{
			return DevObjectCategories.Tutorial;
		}
		if (ModManager.MSC)
		{
			if (type == MoreSlugcatsEnums.PlacedObjectType.OEsphere)
			{
				return DevObjectCategories.Decoration;
			}
			if (type == MoreSlugcatsEnums.PlacedObjectType.KarmaShrine || type == MoreSlugcatsEnums.PlacedObjectType.MSArteryPush)
			{
				return DevObjectCategories.Gameplay;
			}
			if (type == MoreSlugcatsEnums.PlacedObjectType.DandelionPeach || type == MoreSlugcatsEnums.PlacedObjectType.RotFlyPaper || type == MoreSlugcatsEnums.PlacedObjectType.GooieDuck || type == MoreSlugcatsEnums.PlacedObjectType.LillyPuck || type == MoreSlugcatsEnums.PlacedObjectType.GlowWeed)
			{
				return DevObjectCategories.Consumable;
			}
			if (type == MoreSlugcatsEnums.PlacedObjectType.GreenToken || type == MoreSlugcatsEnums.PlacedObjectType.WhiteToken || type == MoreSlugcatsEnums.PlacedObjectType.RedToken || type == MoreSlugcatsEnums.PlacedObjectType.DevToken)
			{
				return DevObjectCategories.Tutorial;
			}
		}
		return DevObjectCategories.Unsorted;
	}

	public void AssembleObjectPages()
	{
		maxObjectsPerPage = 22;
		Dictionary<DevObjectCategories, List<PlacedObject.Type>> dictionary = new Dictionary<DevObjectCategories, List<PlacedObject.Type>>();
		foreach (string entry in ExtEnum<DevObjectCategories>.values.entries)
		{
			dictionary[new DevObjectCategories(entry)] = new List<PlacedObject.Type>();
		}
		foreach (string entry2 in ExtEnum<PlacedObject.Type>.values.entries)
		{
			PlacedObject.Type type = new PlacedObject.Type(entry2);
			DevObjectCategories key = DevObjectGetCategoryFromPlacedType(type);
			dictionary[key].Add(type);
		}
		int num = 0;
		foreach (string entry3 in ExtEnum<DevObjectCategories>.values.entries)
		{
			DevObjectCategories key2 = new DevObjectCategories(entry3);
			int num2 = maxObjectsPerPage * Mathf.CeilToInt(((float)dictionary[key2].Count + 0.5f) / ((float)maxObjectsPerPage + 1f));
			while (dictionary[key2].Count < num2)
			{
				dictionary[key2].Add(PlacedObject.Type.None);
			}
			num += num2;
		}
		int num3 = 0;
		placedObjectTypes = new PlacedObject.Type[num];
		foreach (string entry4 in ExtEnum<DevObjectCategories>.values.entries)
		{
			DevObjectCategories key3 = new DevObjectCategories(entry4);
			for (int i = 0; i < dictionary[key3].Count; i++)
			{
				placedObjectTypes[num3] = dictionary[key3][i];
				num3++;
			}
		}
		totalObjectsPages = 1 + (int)((float)placedObjectTypes.Length / (float)maxObjectsPerPage + 0.5f);
	}
}
