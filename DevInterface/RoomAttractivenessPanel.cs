using System.Collections.Generic;
using MoreSlugcats;
using UnityEngine;

namespace DevInterface;

public class RoomAttractivenessPanel : Panel, IDevUISignals
{
	public class Category : ExtEnum<Category>
	{
		public static readonly Category All = new Category("All", register: true);

		public static readonly Category Lizards = new Category("Lizards", register: true);

		public static readonly Category Flying = new Category("Flying", register: true);

		public static readonly Category Dark = new Category("Dark", register: true);

		public static readonly Category LikesOutside = new Category("LikesOutside", register: true);

		public static readonly Category LikesInside = new Category("LikesInside", register: true);

		public static readonly Category Swimming = new Category("Swimming", register: true);

		public static readonly Category LikesWater = new Category("LikesWater", register: true);

		public Category(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class CreatureButton : Button
	{
		private int tempIndex;

		private bool Toggled
		{
			get
			{
				if (!Category)
				{
					return (parentNode as RoomAttractivenessPanel).toggledCreatures[tempIndex];
				}
				return false;
			}
			set
			{
				if (!Category)
				{
					(parentNode as RoomAttractivenessPanel).toggledCreatures[tempIndex] = value;
				}
			}
		}

		private bool Category => tempIndex < 0;

		public CreatureButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, int tempIndex)
			: base(owner, IDstring, parentNode, pos, 100f, title)
		{
			this.tempIndex = tempIndex;
		}

		public override void Update()
		{
			base.Update();
			if (!Category)
			{
				if (Toggled)
				{
					colorB = new Color(1f, 1f, 1f);
					if (owner != null && owner.mouseClick && base.MouseOver)
					{
						colorA = new Color(1f, 0.8f, 0.8f);
					}
					else
					{
						colorA = new Color(0.8f, 0.8f, 1f);
					}
					return;
				}
				if (base.MouseOver)
				{
					colorA = new Color(1f, 1f, 1f);
				}
				else
				{
					colorA = new Color(0f, 0f, 0f);
				}
				if (owner != null && owner.mouseClick && base.MouseOver)
				{
					colorB = new Color(1f, 0f, 0f);
				}
				else
				{
					colorB = new Color(0f, 0f, 1f);
				}
			}
			else
			{
				colorA = new Color(0f, 0f, 0f);
				if (owner != null && owner.mouseClick && base.MouseOver)
				{
					colorB = new Color(1f, 1f, 1f);
				}
				if ((down && !base.MouseOver) || owner == null || !owner.mouseDown)
				{
					colorB = new Color(0f, 1f, 0f);
				}
			}
		}

		public override void Clicked()
		{
			if (!Category)
			{
				Toggled = !Toggled;
				if (!StaticWorld.creatureTemplates[tempIndex].virtualCreature)
				{
					return;
				}
				for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
				{
					CreatureTemplate creatureTemplate = StaticWorld.creatureTemplates[i];
					while (creatureTemplate.ancestor != null)
					{
						creatureTemplate = creatureTemplate.ancestor;
						if (creatureTemplate.index == tempIndex)
						{
							(parentNode as RoomAttractivenessPanel).toggledCreatures[i] = Toggled;
							break;
						}
					}
				}
			}
			else
			{
				bool flag = false;
				flag = !(parentNode as RoomAttractivenessPanel).toggledCreatures[(parentNode as RoomAttractivenessPanel).categories[-tempIndex - 1][0]];
				for (int j = 0; j < (parentNode as RoomAttractivenessPanel).toggledCreatures.Length; j++)
				{
					(parentNode as RoomAttractivenessPanel).toggledCreatures[j] = false;
				}
				for (int k = 0; k < (parentNode as RoomAttractivenessPanel).categories[-tempIndex - 1].Length; k++)
				{
					(parentNode as RoomAttractivenessPanel).toggledCreatures[(parentNode as RoomAttractivenessPanel).categories[-tempIndex - 1][k]] = flag;
				}
			}
		}
	}

	public class ToolSwitcher : Cycler
	{
		public ToolSwitcher(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, 200f, "TOOL: ", new List<string> { "Cycle", "Neutral", "Forbidden", "Avoid", "Like", "Stay" })
		{
		}

		public override void Update()
		{
			base.Update();
			Color color = ((currentAlternative != 0) ? attractionColors[currentAlternative - 1] : new Color(1f, 0f, 1f));
			colorA = (base.MouseOver ? new Color(1f, 1f, 1f) : color);
			colorB = color;
		}
	}

	public MapPage mapPage;

	public World world;

	public bool[] toggledCreatures;

	public int[][] categories;

	private ToolSwitcher toolSwitcher;

	public DevUILabel label;

	private bool mouseOverAny;

	public int selectedCreatures;

	public int showCreature;

	private int showCreatureCounter;

	public static Color[] attractionColors = new Color[5]
	{
		new Color(1f, 1f, 1f),
		new Color(1f, 0f, 0f),
		new Color(1f, 0.4f, 0f),
		new Color(0f, 1f, 0f),
		new Color(0f, 1f, 0.8f)
	};

	public RoomAttractivenessPanel(DevUI owner, World world, string IDstring, DevUINode parentNode, Vector2 pos, string title, MapPage mapPage)
		: base(owner, IDstring, parentNode, pos + new Vector2(0f, -225f), new Vector2(250f, 700f), title)
	{
		this.world = world;
		this.mapPage = mapPage;
		toggledCreatures = new bool[StaticWorld.creatureTemplates.Length];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
		{
			if ((!StaticWorld.creatureTemplates[i].quantified && StaticWorld.creatureTemplates[i].PreBakedPathingIndex >= 0 && !StaticWorld.creatureTemplates[i].virtualCreature) || StaticWorld.creatureTemplates[i].type == CreatureTemplate.Type.Overseer || StaticWorld.creatureTemplates[i].type == CreatureTemplate.Type.Leech || StaticWorld.creatureTemplates[i].type == CreatureTemplate.Type.SeaLeech)
			{
				subNodes.Add(new CreatureButton(owner, StaticWorld.creatureTemplates[i].type.ToString() + "_Button", this, new Vector2(5f + (float)num2 * 120f, 680f - 20f * (float)num), StaticWorld.creatureTemplates[i].name, i));
				num++;
				if (num > 31)
				{
					num = 0;
					num2++;
				}
			}
		}
		categories = new int[ExtEnum<Category>.values.Count][];
		for (int j = 0; j < categories.Length; j++)
		{
			List<int> list = new List<int>();
			Category category = new Category(ExtEnum<Category>.values.GetEntry(j));
			if (category == Category.All)
			{
				for (int k = 0; k < StaticWorld.creatureTemplates.Length; k++)
				{
					list.Add(k);
				}
			}
			else if (category == Category.Flying)
			{
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Vulture).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.KingVulture).index);
				if (ModManager.MSC)
				{
					list.Add(StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture).index);
				}
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaA).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaB).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Centiwing).index);
			}
			else if (category == Category.Dark)
			{
				if (ModManager.MSC)
				{
					list.Add(StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture).index);
				}
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LanternMouse).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlackLizard).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigSpider).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SpitterSpider).index);
				if (ModManager.MSC)
				{
					list.Add(StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider).index);
				}
			}
			else if (category == Category.Swimming)
			{
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.JetFish).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigEel).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Leech).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SeaLeech).index);
				if (ModManager.MSC)
				{
					list.Add(StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti).index);
				}
				if (ModManager.MSC)
				{
					list.Add(StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.EelLizard).index);
				}
			}
			else if (category == Category.LikesOutside)
			{
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Vulture).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.KingVulture).index);
				if (ModManager.MSC)
				{
					list.Add(StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture).index);
				}
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaA).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaB).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Centiwing).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.WhiteLizard).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard).index);
			}
			else if (category == Category.LikesInside)
			{
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BrotherLongLegs).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Centipede).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigSpider).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SpitterSpider).index);
			}
			else if (category == Category.LikesWater)
			{
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.JetFish).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigEel).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Snail).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Leech).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SeaLeech).index);
				if (ModManager.MSC)
				{
					list.Add(StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti).index);
				}
				if (ModManager.MSC)
				{
					list.Add(StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.EelLizard).index);
				}
			}
			else if (category == Category.Lizards)
			{
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.WhiteLizard).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlackLizard).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.YellowLizard).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.RedLizard).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander).index);
				list.Add(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CyanLizard).index);
			}
			categories[j] = list.ToArray();
			subNodes.Add(new CreatureButton(owner, "Cat_" + category?.ToString() + "_Button", this, new Vector2(5f + (float)num2 * 120f, 680f - 20f * (float)num), "<" + category?.ToString() + ">", -1 - j));
			num++;
			if (num > 31)
			{
				num = 0;
				num2++;
			}
		}
		toolSwitcher = new ToolSwitcher(owner, "Tool_Switcher", this, new Vector2(5f, 5f));
		subNodes.Add(toolSwitcher);
		subNodes.Add(new Button(owner, "Apply_To_All", this, new Vector2(5f, 25f), 200f, "Apply Tool To All Rooms"));
		label = new DevUILabel(owner, "Attractiveness_Label", this, default(Vector2), 100f, toolSwitcher.alternatives[0]);
		subNodes.Add(label);
	}

	public override void Update()
	{
		base.Update();
		if (mouseOverAny)
		{
			label.AbsMove((Vector2)Futile.mousePosition + new Vector2(70f, 30f));
		}
		else
		{
			label.AbsMove(new Vector2(-700f, -300f));
		}
		mouseOverAny = false;
		int num = showCreature;
		showCreature = 0;
		showCreatureCounter++;
		if (showCreatureCounter >= selectedCreatures)
		{
			showCreatureCounter = 0;
		}
		selectedCreatures = 0;
		int num2 = 0;
		for (int i = 0; i < toggledCreatures.Length; i++)
		{
			if (toggledCreatures[i])
			{
				selectedCreatures++;
				if (num2 == showCreatureCounter)
				{
					showCreature = i;
				}
				num2++;
			}
		}
		if (showCreature != num)
		{
			parentNode.Refresh();
		}
	}

	public Color ColorOfRoom(int r)
	{
		if (selectedCreatures < 1 || world.GetAbstractRoom(r).roomAttractions[showCreature] == null || world.GetAbstractRoom(r).roomAttractions[showCreature].Index == -1)
		{
			return new Color(0.1f, 0.1f, 0.1f);
		}
		return attractionColors[world.GetAbstractRoom(r).roomAttractions[showCreature].Index];
	}

	public void RoomClicked(int r)
	{
		int num = toolSwitcher.currentAlternative - 1;
		for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
		{
			if (!toggledCreatures[i])
			{
				continue;
			}
			if (num < 0)
			{
				if (world.GetAbstractRoom(r).roomAttractions[i] != null)
				{
					num = world.GetAbstractRoom(r).roomAttractions[i].Index + 1;
				}
				if (num >= ExtEnum<AbstractRoom.CreatureRoomAttraction>.values.Count || num < 0)
				{
					num = 0;
				}
			}
			string entry = ExtEnum<AbstractRoom.CreatureRoomAttraction>.values.GetEntry(num);
			if (entry == null)
			{
				entry = ExtEnum<AbstractRoom.CreatureRoomAttraction>.values.GetEntry(0);
			}
			world.GetAbstractRoom(r).roomAttractions[i] = new AbstractRoom.CreatureRoomAttraction(entry);
		}
	}

	public void RoomMouseOver(int r)
	{
		mouseOverAny = true;
		AbstractRoom abstractRoom = world.GetAbstractRoom(r);
		AbstractRoom.CreatureRoomAttraction creatureRoomAttraction = null;
		CreatureTemplate.Type tp = CreatureTemplate.Type.StandardGroundCreature;
		for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
		{
			if (toggledCreatures[i])
			{
				tp = StaticWorld.creatureTemplates[i].type;
				if (creatureRoomAttraction == null)
				{
					creatureRoomAttraction = abstractRoom.roomAttractions[i];
				}
				else if (creatureRoomAttraction != abstractRoom.roomAttractions[i])
				{
					label.spriteColor = new Color(1f, 0f, 1f);
					label.textColor = new Color(1f, 1f, 1f);
					label.Text = "MIXED (" + selectedCreatures + ") creatures)";
					return;
				}
			}
		}
		if (creatureRoomAttraction != null)
		{
			int num = ((creatureRoomAttraction.Index != -1) ? creatureRoomAttraction.Index : 0);
			label.spriteColor = attractionColors[num];
			label.textColor = ((creatureRoomAttraction == AbstractRoom.CreatureRoomAttraction.Neutral) ? new Color(0f, 0f, 0f) : new Color(1f, 1f, 1f));
			label.Text = creatureRoomAttraction.ToString() + " " + (int)(world.GetAbstractRoom(r).SizeDependentAttractionValueForCreature(tp) * 100f) + "%";
		}
		else
		{
			label.spriteColor = new Color(0.1f, 0.1f, 0.1f);
			label.textColor = new Color(1f, 1f, 1f);
			label.Text = "NO CREATURES SELECTED";
		}
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		int num = toolSwitcher.currentAlternative - 1;
		for (int i = world.firstRoomIndex; i < world.NumberOfRooms + world.firstRoomIndex; i++)
		{
			for (int j = 0; j < StaticWorld.creatureTemplates.Length; j++)
			{
				if (!toggledCreatures[j])
				{
					continue;
				}
				if (num < 0)
				{
					if (world.GetAbstractRoom(i).roomAttractions[j] != null)
					{
						num = world.GetAbstractRoom(i).roomAttractions[j].Index + 1;
					}
					if (num >= ExtEnum<AbstractRoom.CreatureRoomAttraction>.values.Count || num < 0)
					{
						num = 0;
					}
				}
				string entry = ExtEnum<AbstractRoom.CreatureRoomAttraction>.values.GetEntry(num);
				if (entry == null)
				{
					entry = ExtEnum<AbstractRoom.CreatureRoomAttraction>.values.GetEntry(0);
				}
				world.GetAbstractRoom(i).roomAttractions[j] = new AbstractRoom.CreatureRoomAttraction(entry);
			}
		}
	}
}
