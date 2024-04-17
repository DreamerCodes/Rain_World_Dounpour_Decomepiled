using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class MapPage : Page
{
	public class ModeCycler : Button
	{
		public ModeCycler(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width)
			: base(owner, IDstring, parentNode, pos, width, "")
		{
			subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(-50f, 0f), 40f, "Mode: "));
		}

		public override void Refresh()
		{
			base.Refresh();
			if ((parentNode as MapPage).canonView)
			{
				base.Text = "Canon View";
			}
			else
			{
				base.Text = "Dev View";
			}
		}

		public override void Clicked()
		{
			(parentNode as MapPage).canonView = !(parentNode as MapPage).canonView;
			(parentNode as MapPage).NewMode();
			parentNode.Refresh();
		}
	}

	public class CreatureVis
	{
		public MapPage myMapPage;

		public AbstractCreature crit;

		public WorldCoordinate lastPos;

		public FLabel label;

		public FLabel label2;

		public bool slatedForDeletion;

		public FSprite sprite;

		public FSprite sprite2;

		private Vector2 drawPos;

		private Vector2 dragPos;

		private bool drag;

		public CreatureVis(MapPage myMapPage, AbstractCreature crit)
		{
			this.myMapPage = myMapPage;
			this.crit = crit;
			label = new FLabel(Custom.GetFont(), CritString(crit));
			label2 = new FLabel(Custom.GetFont(), CritString(crit));
			label2.color = new Color(1f - label.color.r, 1f - label.color.g, 1f - label.color.b);
			sprite = new FSprite("pixel");
			sprite.anchorY = 0f;
			if (crit.abstractAI != null)
			{
				sprite2 = new FSprite("pixel");
				sprite2.alpha = 0.25f;
				sprite2.anchorY = 0f;
				if (myMapPage.owner != null)
				{
					Futile.stage.AddChild(sprite2);
				}
			}
			if (myMapPage.owner != null)
			{
				Futile.stage.AddChild(sprite);
				Futile.stage.AddChild(label);
			}
		}

		public void Update()
		{
			if (crit.pos != lastPos)
			{
				lastPos = crit.pos;
				if (!drag)
				{
					dragPos = drawPos;
				}
				drag = true;
			}
			drawPos = myMapPage.CreatureVisPos(crit.pos, crit.InDen, stack: true);
			if (crit.realizedCreature == null && crit.distanceToMyNode < 0)
			{
				drawPos.y += Mathf.Lerp(-2f, 2f, Random.value);
			}
			if (crit.InDen)
			{
				drawPos.y -= 10f;
			}
			sprite.x = drawPos.x;
			sprite.y = drawPos.y;
			if (drag)
			{
				dragPos += Vector2.ClampMagnitude(drawPos - dragPos, 10f);
				sprite.scaleY = Vector2.Distance(drawPos, dragPos);
				sprite.rotation = Custom.AimFromOneVectorToAnother(drawPos, dragPos);
				if (Custom.DistLess(dragPos, drawPos, 5f))
				{
					drag = false;
				}
			}
			else if (crit.pos.NodeDefined)
			{
				Vector2 vector = myMapPage.NodeVisPos(crit.pos.room, crit.pos.abstractNode);
				sprite.scaleY = Vector2.Distance(drawPos, vector);
				sprite.rotation = Custom.AimFromOneVectorToAnother(drawPos, vector);
			}
			else
			{
				sprite.scaleY = 10f;
				sprite.rotation = 135f;
			}
			if (crit.abstractAI != null)
			{
				Vector2 vector2 = myMapPage.CreatureVisPos(crit.abstractAI.destination, inDen: false, stack: false);
				sprite2.x = drawPos.x;
				sprite2.y = drawPos.y;
				sprite2.color = Color.Lerp(new Color(1f, 1f, 1f), label.color, Random.value);
				sprite2.scaleY = Vector2.Distance(drawPos, vector2);
				sprite2.rotation = Custom.AimFromOneVectorToAnother(drawPos, vector2);
			}
			label.x = drawPos.x;
			label.y = drawPos.y;
			label.color = CritCol(crit);
			sprite.color = label.color;
			label2.x = drawPos.x + 1f;
			label2.y = drawPos.y - 1f;
			label.text = CritString(crit);
			label2.text = label.text;
			if (crit.slatedForDeletion)
			{
				Destroy();
			}
		}

		public void Destroy()
		{
			label.RemoveFromContainer();
			label2.RemoveFromContainer();
			sprite.RemoveFromContainer();
			if (sprite2 != null)
			{
				sprite2.RemoveFromContainer();
			}
			slatedForDeletion = true;
		}

		private static string CritString(AbstractCreature crit)
		{
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
			{
				return "Sl";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.PinkLizard)
			{
				return "L";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.GreenLizard)
			{
				return "L";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BlueLizard)
			{
				return "L";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.YellowLizard)
			{
				return "L";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.WhiteLizard)
			{
				return "L";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.RedLizard)
			{
				return "L";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BlackLizard)
			{
				return "L";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.CyanLizard)
			{
				return "L";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Salamander)
			{
				return "A";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Fly)
			{
				return "b";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Leech)
			{
				return "l";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.SeaLeech)
			{
				return "sl";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Snail)
			{
				return "S";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Vulture)
			{
				return "Vu";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.KingVulture)
			{
				return "KVu";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.GarbageWorm)
			{
				return "Gw";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.LanternMouse)
			{
				return "m";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.CicadaA)
			{
				return "cA";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.CicadaB)
			{
				return "cB";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Spider)
			{
				return "s";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.JetFish)
			{
				return "j";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BigEel)
			{
				return "Lev";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Deer)
			{
				return "Dr";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.TubeWorm)
			{
				return "tw";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.DaddyLongLegs)
			{
				return "Dll";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BrotherLongLegs)
			{
				return "bll";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.TentaclePlant)
			{
				return "tp";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.PoleMimic)
			{
				return "pm";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.MirosBird)
			{
				return "M";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Centipede)
			{
				return "c";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.RedCentipede)
			{
				return "RC";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Scavenger || (ModManager.MSC && (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite || crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)))
			{
				string text = "Sc";
				if (ModManager.MSC && crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)
				{
					text = "El";
				}
				if (ModManager.MSC && crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)
				{
					text = "SK";
				}
				if ((crit.abstractAI as ScavengerAbstractAI).squad == null)
				{
					return text;
				}
				if ((crit.abstractAI as ScavengerAbstractAI).squad.missionType != ScavengerAbstractAI.ScavengerSquad.MissionID.None)
				{
					if ((crit.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.HuntCreature)
					{
						return text + "(H)";
					}
					if ((crit.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.GuardOutpost)
					{
						return text + "(G)";
					}
					if ((crit.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.ProtectCreature)
					{
						return text + "(P)";
					}
					if ((crit.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
					{
						return text + "(T)";
					}
				}
				return "Sc";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Centiwing)
			{
				return "cW";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.SmallCentipede)
			{
				return "sc";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Overseer)
			{
				return "o";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BigSpider)
			{
				return "sp";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.SpitterSpider)
			{
				return "SP";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.EggBug)
			{
				return "eb";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm)
			{
				return "Bnw";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.SmallNeedleWorm)
			{
				return "nw";
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.DropBug)
			{
				return "db";
			}
			if (ModManager.MSC)
			{
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
				{
					return "Tll";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
				{
					return "aC";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)
				{
					return "mV";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
				{
					return "hll";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider)
				{
					return "SM";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug)
				{
					return "Sb";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
				{
					return "Fb";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
				{
					return "L";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
				{
					return "E";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
				{
					return "L";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Inspector)
				{
					return "Is";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Yeek)
				{
					return "Ye";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.BigJelly)
				{
					return "Jy";
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
				{
					return "NPC";
				}
			}
			return "";
		}

		private static Color CritCol(AbstractCreature crit)
		{
			if (crit.InDen && Random.value < 0.5f)
			{
				return new Color(0.5f, 0.5f, 0.5f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
			{
				return new Color(1f, 1f, 1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.PinkLizard)
			{
				return new Color(1f, 0f, 1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.GreenLizard)
			{
				return new Color(0f, 1f, 0.2f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BlueLizard)
			{
				return new Color(0f, 0.4f, 1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.YellowLizard)
			{
				return new Color(1f, 0.7f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.WhiteLizard)
			{
				return new Color(1f, 1f, 1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.RedLizard)
			{
				return new Color(1f, 0f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BlackLizard)
			{
				return new Color(0.1f, 0.1f, 0.1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Salamander)
			{
				return new Color(1f, 0.7f, 0.7f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.CyanLizard)
			{
				return new Color(0f, 0.8f, 1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Fly)
			{
				return new Color(0.5f, 0.5f, 0.7f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Leech)
			{
				return new Color(1f, 0.5f, 0.5f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.SeaLeech)
			{
				return new Color(0.5f, 0.5f, 1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Snail)
			{
				return new Color(0.4f, 0.8f, 0.6f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Vulture)
			{
				return new Color(0.6f, 0.4f, 0.15f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.KingVulture)
			{
				return new Color(1f, 0f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.GarbageWorm)
			{
				return new Color(0.3f, 0.3f, 0.3f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.LanternMouse)
			{
				return new Color(1f, 1f, 0.7f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.CicadaA)
			{
				return new Color(1f, 1f, 1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.CicadaB)
			{
				return new Color(0.1f, 0.1f, 0.1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Spider)
			{
				return new Color(0.1f, 0.1f, 0.1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.JetFish)
			{
				return new Color(0.1f, 0.2f, 0.1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BigEel)
			{
				return new Color(0.1f, 0.3f, 0.3f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Deer)
			{
				return new Color(0.8f, 1f, 0.8f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.TubeWorm)
			{
				return new Color(0f, 0.4f, 0.6f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.DaddyLongLegs)
			{
				return new Color(0f, 0f, 1f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BrotherLongLegs)
			{
				return new Color(0.6f, 0.8f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.TentaclePlant)
			{
				return new Color(1f, 0f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.PoleMimic)
			{
				return new Color(0f, 0f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.MirosBird)
			{
				return new Color(0.5f, 0f, 0.5f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Centipede)
			{
				return new Color(1f, 0.7f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.RedCentipede)
			{
				return Color.red;
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Centiwing)
			{
				return new Color(0f, 1f, 0.2f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.SmallCentipede)
			{
				return new Color(1f, 0.7f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.BigSpider)
			{
				return new Color(1f, 1f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.SpitterSpider)
			{
				return new Color(1f, 0f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Overseer)
			{
				if (Random.value < 0.5f && (crit.abstractAI as OverseerAbstractAI).playerGuide)
				{
					return new Color(1f, 1f, 0f);
				}
				return new Color(38f / 85f, 46f / 51f, 0.76862746f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.Scavenger || (ModManager.MSC && (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite || crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)))
			{
				if ((crit.abstractAI as ScavengerAbstractAI).freeze > 0)
				{
					return new Color(0.5f, 0.5f, 0.5f);
				}
				if ((crit.abstractAI as ScavengerAbstractAI).squad == null)
				{
					return new Color(0f, 0.2f, 0.14f);
				}
				if (Random.value < 0.3f && (crit.abstractAI as ScavengerAbstractAI).squad.missionType != ScavengerAbstractAI.ScavengerSquad.MissionID.None)
				{
					if ((crit.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.HuntCreature)
					{
						return new Color(1f, 0f, 0f);
					}
					if ((crit.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.GuardOutpost)
					{
						return new Color(0f, 0f, 1f);
					}
					if ((crit.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.ProtectCreature)
					{
						return new Color(0f, 1f, 0f);
					}
					if ((crit.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
					{
						return new Color(1f, 1f, 0f);
					}
				}
				if ((crit.abstractAI as ScavengerAbstractAI).squad.leader == crit)
				{
					return Color.Lerp((crit.abstractAI as ScavengerAbstractAI).squad.color, new Color(1f, 1f, 1f), Random.value);
				}
				return (crit.abstractAI as ScavengerAbstractAI).squad.color;
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.EggBug)
			{
				return new Color(0f, 1f, 0f);
			}
			if (crit.creatureTemplate.type == CreatureTemplate.Type.SmallNeedleWorm)
			{
				if ((crit.abstractAI as NeedleWormAbstractAI).mother == null)
				{
					return new Color(1f, 0f, 0f);
				}
				return new Color(0f, 1f, 0f);
			}
			if (ModManager.MSC)
			{
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs || crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
				{
					return new Color(0f, 0f, 1f);
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)
				{
					new Color(0.75f, 0f, 0f);
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
				{
					new Color(0.57f, 0.11f, 0.23f);
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider)
				{
					new Color(0f, 1f, 0f);
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug)
				{
					return new Color(0f, 1f, 0.47058824f);
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
				{
					return new Color(1f, 0.5f, 0.5f);
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
				{
					return new Color(0.5f, 0.5f, 0.5f);
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
				{
					return new Color(0f, 0f, 0f, 1f);
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
				{
					return new Color(0.95f, 0.73f, 0.73f);
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Inspector)
				{
					return Color.white;
				}
				if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Yeek)
				{
					return new Color(1f, 1f, 0.7f);
				}
				_ = crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.BigJelly;
				return new Color(1f, 1f, 1f);
			}
			return new Color(1f, 1f, 1f);
		}
	}

	public MapObject map;

	private string filePath;

	public bool canonView;

	public bool viewNodeLabels = true;

	private bool lastJ;

	public Vector2 panPos;

	public MapRenderOutput renderOutput;

	public RoomAttractivenessPanel attractivenessPanel;

	private int[][] creatureStacker;

	private List<CreatureVis> creatureVisualizations;

	public List<DevUINode> modeSpecificNodes;

	public bool subRegionsMode;

	public DevUILabel subregionLabel;

	public World world;

	private DevUILabel floodingLabel;

	public static Color SubregionColor(AbstractRoom rm)
	{
		int num = 0;
		if (rm.subregionName != null && rm.world.region != null)
		{
			for (int i = 1; i < rm.world.region.subRegions.Count; i++)
			{
				if (rm.subregionName == rm.world.region.subRegions[i])
				{
					num = i;
					break;
				}
			}
		}
		switch (num)
		{
		case 0:
			return new Color(0.5f, 0.5f, 0.5f);
		case 1:
			return new Color(1f, 0.2f, 0.2f);
		case 2:
			return new Color(0.2f, 1f, 0.2f);
		case 3:
			return new Color(0.2f, 0.2f, 1f);
		case 4:
			return new Color(1f, 0.2f, 1f);
		case 5:
			return new Color(1f, 1f, 0.2f);
		case 6:
			return new Color(0.2f, 1f, 1f);
		default:
		{
			byte[] array = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(rm.subregionName));
			return new Color((float)(int)array[0] / 255f, (float)(int)array[1] / 255f, (float)(int)array[2] / 255f);
		}
		}
	}

	public MapPage(DevUI owner, World world, string IDstring, DevUINode parentNode, string name, bool forceRenderMode)
		: base(owner, IDstring, parentNode, name)
	{
		this.world = world;
		map = new MapObject(world, forceRenderMode);
		modeSpecificNodes = new List<DevUINode>();
		if (owner != null && (owner.game.rainWorld.processManager.currentMainLoop as RainWorldGame).IsStorySession)
		{
			filePath = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + world.name + Path.DirectorySeparatorChar + "map_" + world.name + "-" + (owner.game.rainWorld.processManager.currentMainLoop as RainWorldGame).GetStorySession.saveState.saveStateNumber.value + ".txt");
		}
		if (owner == null || filePath == null || !File.Exists(filePath))
		{
			filePath = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + world.name + Path.DirectorySeparatorChar + "map_" + world.name + ".txt");
		}
		fSprites.Add(new FSprite("pixel"));
		fSprites[fSprites.Count - 1].color = new Color(0f, 0f, 0f);
		fSprites[fSprites.Count - 1].scaleX = 1366f;
		fSprites[fSprites.Count - 1].scaleY = 768f;
		fSprites[fSprites.Count - 1].anchorX = 0f;
		fSprites[fSprites.Count - 1].anchorY = 0f;
		if (owner != null)
		{
			Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
		}
		Vector2 pos = new Vector2(100f, 500f);
		for (int i = 0; i < map.roomReps.Length; i++)
		{
			if (map.roomReps[i].room.offScreenDen)
			{
				pos = new Vector2(100f, 650f);
			}
			subNodes.Add(new RoomPanel(owner, world, this, pos, map.roomReps[i]));
			pos.x += 110f;
			if (pos.x > 1200f)
			{
				pos.x = 100f;
				pos.y -= 50f;
			}
		}
		creatureStacker = new int[world.NumberOfRooms][];
		for (int j = 0; j < world.NumberOfRooms; j++)
		{
			creatureStacker[j] = new int[world.GetAbstractRoom(j + world.firstRoomIndex).nodes.Length];
		}
		LoadMapConfig();
		creatureVisualizations = new List<CreatureVis>();
		for (int k = 0; k < world.NumberOfRooms; k++)
		{
			for (int l = 0; l < world.GetAbstractRoom(k + world.firstRoomIndex).creatures.Count; l++)
			{
				creatureVisualizations.Add(new CreatureVis(this, world.GetAbstractRoom(k + world.firstRoomIndex).creatures[l]));
			}
			for (int m = 0; m < world.GetAbstractRoom(k + world.firstRoomIndex).entitiesInDens.Count; m++)
			{
				if (world.GetAbstractRoom(k + world.firstRoomIndex).entitiesInDens[m] is AbstractCreature)
				{
					creatureVisualizations.Add(new CreatureVis(this, world.GetAbstractRoom(k + world.firstRoomIndex).entitiesInDens[m] as AbstractCreature));
				}
			}
		}
		subregionLabel = new DevUILabel(owner, "Subregion_Label", this, default(Vector2), 100f, "");
		subNodes.Add(subregionLabel);
		subregionLabel.spriteColor = new Color(0f, 0f, 0f);
		subregionLabel.textColor = new Color(1f, 1f, 1f);
		if (ModManager.MSC)
		{
			floodingLabel = new DevUILabel(owner, "FloodLevel", this, default(Vector2), 100f, string.Empty);
			floodingLabel.spriteColor = new Color(1f, 0f, 0f);
			floodingLabel.textColor = new Color(1f, 1f, 1f);
			subNodes.Add(floodingLabel);
		}
		NewMode();
	}

	public void NewMode()
	{
		for (int i = 0; i < modeSpecificNodes.Count; i++)
		{
			modeSpecificNodes[i].ClearSprites();
			subNodes.Remove(modeSpecificNodes[i]);
		}
		modeSpecificNodes.Clear();
		modeSpecificNodes.Add(new ModeCycler(owner, "Mode_Cycler", this, new Vector2(170f, 660f), 120f));
		if (canonView)
		{
			modeSpecificNodes.Add(new Button(owner, "Render_Map", this, new Vector2(170f, 640f), 120f, "Render Map"));
			modeSpecificNodes.Add(new Button(owner, "New_Default", this, new Vector2(170f, 620f), 120f, "Create Def. Mat. Rect"));
			modeSpecificNodes.Add(new DevUILabel(owner, "txt", this, new Vector2(120f, 600f), 260f, "Hold 'N' and click room to change its layer"));
			LoadDefMatRects();
		}
		else
		{
			modeSpecificNodes.Add(new Button(owner, "Update_Dev_Positions", this, new Vector2(170f, 640f), 220f, "Reset Dev Positions (Hold 'N' and click)"));
			modeSpecificNodes.Add(new Button(owner, "Room_Attractiveness_Button", this, new Vector2(170f, 620f), 220f, "Room Attractiveness Tool"));
			modeSpecificNodes.Add(new Button(owner, "Sub_Regions_Toggle", this, new Vector2(170f, 600f), 220f, "Sub Regions"));
		}
		for (int j = 0; j < modeSpecificNodes.Count; j++)
		{
			subNodes.Add(modeSpecificNodes[j]);
		}
	}

	public override void Update()
	{
		base.Update();
		map.Update();
		if (Input.GetKey("j") && !lastJ)
		{
			viewNodeLabels = !viewNodeLabels;
			Refresh();
		}
		lastJ = Input.GetKey("j");
		if (canonView || attractivenessPanel != null)
		{
			subRegionsMode = false;
		}
		if (ModManager.MSC)
		{
			if (canonView)
			{
				float y = 0f;
				if (map.world.game.cameras[0].room != null)
				{
					float num = Mathf.Max(map.world.game.globalRain.drainWorldFlood, map.world.game.globalRain.flood);
					num -= map.world.RoomToWorldPos(new Vector2(0f, 0f), map.world.game.cameras[0].room.abstractRoom.index).y;
					y = GetMiniMapOfRoom(map.world.game.cameras[0].room.abstractRoom.index).absPos.y + num / 6.666f;
					if (map.world.game.globalRain.flood > 0f)
					{
						floodingLabel.Text = " |Flood: " + map.world.game.globalRain.flood + " |Fast: " + !map.world.game.globalRain.forceSlowFlood;
					}
					else
					{
						floodingLabel.Text = " |Flood: " + map.world.game.globalRain.drainWorldFlood + " |Fast: " + map.world.game.globalRain.drainWorldFastDrainCounter;
					}
				}
				floodingLabel.pos = new Vector2(owner.mousePos.x - 50f, y);
			}
			else
			{
				floodingLabel.Text = " |Flood: " + map.world.game.globalRain.drainWorldFlood + " |Fast: " + map.world.game.globalRain.drainWorldFastDrainCounter;
				floodingLabel.pos = new Vector2(0f, 0f);
			}
			floodingLabel.Refresh();
		}
		if (!subRegionsMode)
		{
			subregionLabel.Text = "";
		}
		else
		{
			string text = null;
			for (int i = 0; i < subNodes.Count; i++)
			{
				if (subNodes[i] is RoomPanel && (subNodes[i] as RoomPanel).MouseOver)
				{
					text = (subNodes[i] as RoomPanel).roomRep.room.DisplaySubregionName;
					if (text != null)
					{
						subregionLabel.Text = Custom.rainWorld.inGameTranslator.Translate(text);
					}
					break;
				}
			}
			if (text == null)
			{
				subregionLabel.Text = "NONE";
			}
			if (owner != null)
			{
				subregionLabel.pos = owner.mousePos + new Vector2(50f, -30f);
			}
			subregionLabel.Refresh();
		}
		for (int j = 0; j < world.NumberOfRooms; j++)
		{
			for (int k = 0; k < world.GetAbstractRoom(j + world.firstRoomIndex).nodes.Length; k++)
			{
				creatureStacker[j][k] = 0;
			}
		}
		for (int l = 0; l < subNodes.Count; l++)
		{
			if (subNodes[l] is RoomPanel && map.toRefreshRooms[(subNodes[l] as RoomPanel).roomRep.room.index - map.world.firstRoomIndex] > 0)
			{
				map.toRefreshRooms[(subNodes[l] as RoomPanel).roomRep.room.index - map.world.firstRoomIndex]--;
				(subNodes[l] as RoomPanel).Refresh();
			}
		}
		if (owner != null && owner.mouseDown && owner.draggedNode == null)
		{
			panPos -= owner.lastMousePos - owner.mousePos;
			Refresh();
		}
		for (int num2 = creatureVisualizations.Count - 1; num2 >= 0; num2--)
		{
			if (creatureVisualizations[num2].slatedForDeletion)
			{
				creatureVisualizations.RemoveAt(num2);
			}
			else
			{
				creatureVisualizations[num2].Update();
			}
		}
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		for (int num = creatureVisualizations.Count - 1; num >= 0; num--)
		{
			creatureVisualizations[num].Destroy();
		}
	}

	public override void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (!(type == DevUISignalType.ButtonClick))
		{
			return;
		}
		string iDstring = sender.IDstring;
		if (iDstring == null)
		{
			return;
		}
		switch (iDstring)
		{
		case "Save_Settings":
			SaveMapConfig();
			break;
		case "Export_Sandbox":
			(owner.game.GetArenaGameSession as SandboxGameSession).editor.DevToolsExportConfig();
			break;
		case "Update_Dev_Positions":
		{
			if (canonView || !Input.GetKey("n"))
			{
				break;
			}
			for (int i = 0; i < subNodes.Count; i++)
			{
				if (subNodes[i] is RoomPanel)
				{
					(subNodes[i] as RoomPanel).devPos = (subNodes[i] as RoomPanel).pos;
				}
			}
			Refresh();
			break;
		}
		case "Render_Map":
			if (renderOutput != null)
			{
				subNodes.Remove(renderOutput);
				renderOutput.ClearSprites();
				renderOutput = null;
				Refresh();
			}
			else
			{
				renderOutput = new MapRenderOutput(owner, world, "Render_Output", this, new Vector2(20f, 20f), "Rendered Map", this);
				subNodes.Add(renderOutput);
				Refresh();
			}
			break;
		case "New_Default":
			modeSpecificNodes.Add(new MapRenderDefaultMaterial(owner, "Def_Mat", this, new Vector2(20f, 20f)));
			subNodes.Add(modeSpecificNodes[modeSpecificNodes.Count - 1]);
			break;
		case "Room_Attractiveness_Button":
			if (attractivenessPanel != null)
			{
				subNodes.Remove(attractivenessPanel);
				attractivenessPanel.ClearSprites();
				attractivenessPanel = null;
				Refresh();
			}
			else
			{
				attractivenessPanel = new RoomAttractivenessPanel(owner, world, "Attractiveness_Panel", this, new Vector2(1100f, 240f), "Room Attractiveness", this);
				subNodes.Add(attractivenessPanel);
				Refresh();
			}
			break;
		case "Sub_Regions_Toggle":
			subRegionsMode = !subRegionsMode;
			break;
		case "Save_Specific":
			base.RoomSettings.Save(owner.game.GetStorySession.saveStateNumber);
			break;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		if (fSprites.Count > 0)
		{
			fSprites[0].alpha = (canonView ? 0.9f : 0.6f);
		}
	}

	public Vector2 ExitVisPos(int room, int node)
	{
		for (int i = 0; i < subNodes.Count; i++)
		{
			if (subNodes[i] is RoomPanel && (subNodes[i] as RoomPanel).roomRep.room.index == room)
			{
				return (subNodes[i] as RoomPanel).miniMap.GetExitVisPos(node);
			}
		}
		return new Vector2(0f, 0f);
	}

	public Vector2 NodeVisPos(int room, int node)
	{
		for (int i = 0; i < subNodes.Count; i++)
		{
			if (subNodes[i] is RoomPanel && (subNodes[i] as RoomPanel).roomRep.room.index == room)
			{
				return (subNodes[i] as RoomPanel).miniMap.GetNodeSquarePos(node);
			}
		}
		return new Vector2(0f, 0f);
	}

	public MiniMap GetMiniMapOfRoom(int room)
	{
		for (int i = 0; i < subNodes.Count; i++)
		{
			if (subNodes[i] is RoomPanel && (subNodes[i] as RoomPanel).roomRep.room.index == room)
			{
				return (subNodes[i] as RoomPanel).miniMap;
			}
		}
		return null;
	}

	public Vector2 CreatureVisPos(WorldCoordinate critPos, bool inDen, bool stack)
	{
		for (int i = 0; i < subNodes.Count; i++)
		{
			if (!(subNodes[i] is RoomPanel) || (subNodes[i] as RoomPanel).roomRep.room.index != critPos.room)
			{
				continue;
			}
			if (critPos.TileDefined && !inDen)
			{
				return (subNodes[i] as RoomPanel).miniMap.absPos + critPos.Tile.ToVector2() * 2f;
			}
			Vector2 vector = new Vector2(0f, 0f);
			if (critPos.NodeDefined && stack)
			{
				try
				{
					vector = new Vector2(1f, -6f) * creatureStacker[critPos.room - world.firstRoomIndex][critPos.abstractNode];
					creatureStacker[critPos.room - world.firstRoomIndex][critPos.abstractNode]++;
				}
				catch
				{
					if (world.IsRoomInRegion(critPos.room))
					{
						Custom.Log($"{world.GetAbstractRoom(critPos).name} nds:{world.GetAbstractRoom(critPos).nodes.Length}, {critPos.abstractNode}");
					}
					else
					{
						Custom.LogWarning($"Creature out of region :{critPos.room} ({world.firstRoomIndex}:{world.firstRoomIndex + world.NumberOfRooms})");
					}
				}
			}
			return (subNodes[i] as RoomPanel).miniMap.GetNodeSquarePos(critPos.abstractNode) + vector;
		}
		return new Vector2(0f, 0f);
	}

	public void SaveMapConfig()
	{
		Vector2 vector = new Vector2(0f, 0f);
		Vector2 vector2 = new Vector2(0f, 0f);
		int num = 0;
		for (int i = 0; i < subNodes.Count; i++)
		{
			if (subNodes[i] is RoomPanel)
			{
				num++;
				vector += (subNodes[i] as RoomPanel).pos;
				vector2 += (subNodes[i] as RoomPanel).devPos;
			}
		}
		vector /= (float)num;
		vector2 /= (float)num;
		for (int j = 0; j < subNodes.Count; j++)
		{
			if (subNodes[j] is RoomPanel)
			{
				(subNodes[j] as RoomPanel).pos -= vector;
				(subNodes[j] as RoomPanel).devPos -= vector2;
			}
			else if (subNodes[j] is MapRenderDefaultMaterial)
			{
				(subNodes[j] as MapRenderDefaultMaterial).handleA.pos -= vector;
				(subNodes[j] as MapRenderDefaultMaterial).handleB.pos -= vector;
			}
		}
		using (StreamWriter streamWriter = File.CreateText(filePath))
		{
			for (int k = 0; k < subNodes.Count; k++)
			{
				if (!(subNodes[k] is RoomPanel))
				{
					continue;
				}
				bool flag = true;
				foreach (string disabledMapRoom in map.world.DisabledMapRooms)
				{
					if (disabledMapRoom == (subNodes[k] as RoomPanel).roomRep.room.name)
					{
						flag = false;
						Custom.Log("MAP WRITER IGNORED HIDDEN ROOM:", disabledMapRoom);
						break;
					}
				}
				if (flag)
				{
					streamWriter.WriteLine((subNodes[k] as RoomPanel).roomRep.room.name + ": " + (subNodes[k] as RoomPanel).pos.x + "><" + (subNodes[k] as RoomPanel).pos.y + "><" + (subNodes[k] as RoomPanel).devPos.x + "><" + (subNodes[k] as RoomPanel).devPos.y + "><" + (subNodes[k] as RoomPanel).layer + "><" + (((subNodes[k] as RoomPanel).roomRep.room.subregionName == null) ? "" : (subNodes[k] as RoomPanel).roomRep.room.subregionName) + "><" + (subNodes[k] as RoomPanel).roomRep.room.size.x + "><" + (subNodes[k] as RoomPanel).roomRep.room.size.y);
				}
			}
			for (int l = 0; l < subNodes.Count; l++)
			{
				if (subNodes[l] is MapRenderDefaultMaterial)
				{
					streamWriter.WriteLine("Def_Mat: " + (subNodes[l] as MapRenderDefaultMaterial).handleA.pos.x + "," + (subNodes[l] as MapRenderDefaultMaterial).handleA.pos.y + "," + (subNodes[l] as MapRenderDefaultMaterial).handleB.pos.x + "," + (subNodes[l] as MapRenderDefaultMaterial).handleB.pos.y + "," + ((subNodes[l] as MapRenderDefaultMaterial).handleA.subNodes[0] as Panel).pos.x + "," + ((subNodes[l] as MapRenderDefaultMaterial).handleA.subNodes[0] as Panel).pos.y + "," + ((subNodes[l] as MapRenderDefaultMaterial).materialIsAir ? "1" : "0"));
				}
			}
			for (int m = 0; m < subNodes.Count; m++)
			{
				if (!(subNodes[m] is RoomPanel))
				{
					continue;
				}
				int index = (subNodes[m] as RoomPanel).roomRep.room.index;
				for (int n = 0; n < (subNodes[m] as RoomPanel).roomRep.room.connections.Length; n++)
				{
					int num2 = (subNodes[m] as RoomPanel).roomRep.room.connections[n];
					if (num2 <= index)
					{
						continue;
					}
					RoomPanel roomPanel = null;
					for (int num3 = 0; num3 < subNodes.Count; num3++)
					{
						if (subNodes[num3] is RoomPanel && (subNodes[num3] as RoomPanel).roomRep.room.index == num2)
						{
							roomPanel = subNodes[num3] as RoomPanel;
							break;
						}
					}
					if (roomPanel == null || n <= -1 || n >= (subNodes[m] as RoomPanel).roomRep.nodePositions.Length)
					{
						continue;
					}
					Vector2 vector3 = (subNodes[m] as RoomPanel).roomRep.nodePositions[n];
					int num4 = roomPanel.roomRep.room.ExitIndex(index);
					if (num4 > -1)
					{
						bool flag2 = true;
						foreach (string disabledMapRoom2 in map.world.DisabledMapRooms)
						{
							if (disabledMapRoom2 == (subNodes[m] as RoomPanel).roomRep.room.name || disabledMapRoom2 == roomPanel.roomRep.room.name)
							{
								flag2 = false;
								Custom.Log("MAP WRITER IGNORED HIDDEN CONNECTION:", disabledMapRoom2);
								break;
							}
						}
						if (flag2)
						{
							Vector2 vector4 = roomPanel.roomRep.nodePositions[num4];
							streamWriter.WriteLine("Connection: " + (subNodes[m] as RoomPanel).roomRep.room.name + "," + roomPanel.roomRep.room.name + "," + vector3.x + "," + vector3.y + "," + vector4.x + "," + vector4.y + "," + (subNodes[m] as RoomPanel).roomRep.exitDirections[n] + "," + roomPanel.roomRep.exitDirections[num4]);
						}
					}
					else
					{
						Custom.LogWarning("failed connection:", roomPanel.roomRep.room.name, "->", (subNodes[m] as RoomPanel).roomRep.room.name);
					}
				}
			}
		}
		List<string> list = new List<string>();
		for (int num5 = world.firstRoomIndex; num5 < world.firstRoomIndex + world.NumberOfRooms; num5++)
		{
			bool flag3 = false;
			AbstractRoom abstractRoom = world.GetAbstractRoom(num5);
			for (int num6 = 0; num6 < abstractRoom.roomAttractions.Length; num6++)
			{
				if (flag3)
				{
					break;
				}
				if (abstractRoom.roomAttractions[num6] != null && abstractRoom.roomAttractions[num6] != AbstractRoom.CreatureRoomAttraction.Neutral)
				{
					flag3 = true;
				}
			}
			if (!flag3)
			{
				continue;
			}
			string text = "Room_Attr: " + abstractRoom.name + ": ";
			for (int num7 = 0; num7 < abstractRoom.roomAttractions.Length; num7++)
			{
				if (abstractRoom.roomAttractions[num7] != null && abstractRoom.roomAttractions[num7] != AbstractRoom.CreatureRoomAttraction.Neutral)
				{
					text = text + StaticWorld.creatureTemplates[num7].type.ToString() + "-" + abstractRoom.roomAttractions[num7].ToString() + ",";
				}
			}
			list.Add(text);
		}
		if (list.Count <= 0)
		{
			return;
		}
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + world.name + Path.DirectorySeparatorChar + "Properties_" + (owner.game.rainWorld.processManager.currentMainLoop as RainWorldGame).GetStorySession.saveState.saveStateNumber?.ToString() + ".txt");
		if (!File.Exists(path))
		{
			path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + world.name + Path.DirectorySeparatorChar + "Properties.txt");
		}
		string[] array = File.ReadAllLines(path);
		using StreamWriter streamWriter2 = File.CreateText(path);
		for (int num8 = 0; num8 < array.Length; num8++)
		{
			if (array[num8].Substring(0, 10) != "Room_Attr:")
			{
				streamWriter2.WriteLine(array[num8]);
			}
		}
		for (int num9 = 0; num9 < list.Count; num9++)
		{
			streamWriter2.WriteLine(list[num9]);
		}
	}

	public void LoadMapConfig()
	{
		if (!File.Exists(filePath))
		{
			return;
		}
		string[] array = File.ReadAllLines(filePath);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ":"), ": ");
			if (array2.Length != 2)
			{
				continue;
			}
			for (int j = 0; j < subNodes.Count; j++)
			{
				if (subNodes[j] is RoomPanel && (subNodes[j] as RoomPanel).roomRep.room.name == array2[0])
				{
					string[] array3 = Regex.Split(array2[1], "><");
					(subNodes[j] as RoomPanel).pos.x = float.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture);
					(subNodes[j] as RoomPanel).pos.y = float.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					if (array3.Length >= 4)
					{
						(subNodes[j] as RoomPanel).devPos.x = float.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture);
						(subNodes[j] as RoomPanel).devPos.y = float.Parse(array3[3], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					else
					{
						(subNodes[j] as RoomPanel).devPos = (subNodes[j] as RoomPanel).pos;
					}
					if (array3.Length >= 5)
					{
						(subNodes[j] as RoomPanel).layer = int.Parse(array3[4], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					break;
				}
			}
		}
	}

	public void LoadDefMatRects()
	{
		if (!File.Exists(filePath))
		{
			return;
		}
		string[] array = File.ReadAllLines(filePath);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ":"), ": ");
			if (array2.Length == 2 && array2[0] == "Def_Mat")
			{
				string[] array3 = Regex.Split(array2[1], ",");
				MapRenderDefaultMaterial mapRenderDefaultMaterial = new MapRenderDefaultMaterial(owner, "Def_Mat", this, new Vector2(0f, 0f));
				mapRenderDefaultMaterial.handleA.pos = new Vector2(float.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture));
				mapRenderDefaultMaterial.handleB.pos = new Vector2(float.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[3], NumberStyles.Any, CultureInfo.InvariantCulture));
				(mapRenderDefaultMaterial.handleA.subNodes[0] as Panel).pos = new Vector2(float.Parse(array3[4], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[5], NumberStyles.Any, CultureInfo.InvariantCulture));
				mapRenderDefaultMaterial.materialIsAir = int.Parse(array3[6], NumberStyles.Any, CultureInfo.InvariantCulture) == 1;
				modeSpecificNodes.Add(mapRenderDefaultMaterial);
			}
		}
	}
}
