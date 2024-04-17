using RWCustom;
using UnityEngine;

public class AbstractSpaceVisualizer
{
	public World world;

	public Room room;

	public FLabel infoText;

	public FLabel[,] communityLabels;

	private Vector2 disp = new Vector2(20f, -30f);

	public DebugSprite current;

	public AbstractSpaceVisualizer(World world, Room room)
	{
		this.world = world;
		this.room = room;
		infoText = new FLabel(Custom.GetFont(), "");
		Futile.stage.AddChild(infoText);
		infoText.x = 20.1f;
		infoText.y = 200f;
		infoText.alignment = FLabelAlignment.Left;
		communityLabels = new FLabel[ExtEnum<CreatureCommunities.CommunityID>.values.Count, world.game.overWorld.regions.Length + 3];
		for (int i = 0; i < communityLabels.GetLength(0); i++)
		{
			for (int j = 0; j < communityLabels.GetLength(1); j++)
			{
				communityLabels[i, j] = new FLabel(Custom.GetFont(), "");
				Futile.stage.AddChild(communityLabels[i, j]);
				communityLabels[i, j].x = 20.1f + ((j > 0) ? 100f : 0f) + (float)j * 40f;
				communityLabels[i, j].y = 550f - 20f * (float)i;
				communityLabels[i, j].alignment = FLabelAlignment.Left;
				if (i == 1 && j == 1)
				{
					communityLabels[i, j].color = new Color(1f, 1f, 0f);
					continue;
				}
				if (i == 1)
				{
					communityLabels[i, j].color = new Color(0.35f, 1f, 0.35f);
					continue;
				}
				switch (j)
				{
				case 0:
					communityLabels[i, j].color = new Color(0.75f, 0.75f, 0.75f);
					continue;
				case 1:
					communityLabels[i, j].color = new Color(1f, 0.5f, 0.5f);
					continue;
				}
				if (j == communityLabels.GetLength(1) - 1)
				{
					communityLabels[i, j].color = new Color(0.5f, 0.5f, 1f);
				}
			}
		}
		FSprite sp = new FSprite("pixel")
		{
			scaleX = 24f,
			scaleY = 160f,
			color = new Color(1f, 1f, 1f),
			alpha = 0.1f
		};
		current = new DebugSprite(new Vector2(0f, 0f), sp, room);
		room.AddObject(current);
		Random.State state = Random.state;
		Random.InitState(1);
		ChangeRoom(room);
		Random.state = state;
	}

	public void Visibility(bool visibility)
	{
		current.sprite.isVisible = visibility;
		infoText.isVisible = visibility;
		for (int i = 0; i < communityLabels.GetLength(0); i++)
		{
			for (int j = 0; j < communityLabels.GetLength(1); j++)
			{
				communityLabels[i, j].isVisible = visibility;
			}
		}
	}

	public void ChangeRoom(Room newRoom)
	{
		if (world != newRoom.world)
		{
			world = newRoom.world;
		}
		room.RemoveObject(current);
		newRoom.AddObject(current);
		room = newRoom;
	}

	public Vector2 SpritePosition(int i, int a)
	{
		return new Vector2((float)i * 20f, 768f - (float)a * 15f) + disp + room.game.cameras[0].pos;
	}

	public void Update()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < world.NumberOfRooms; i++)
		{
			num += world.GetAbstractRoom(i + world.firstRoomIndex).NumberOfQuantifiedCreatureInRoom(CreatureTemplate.Type.Fly);
			for (int j = 0; j < world.GetAbstractRoom(i + world.firstRoomIndex).creatures.Count; j++)
			{
				if (!world.GetAbstractRoom(i + world.firstRoomIndex).creatures[j].creatureTemplate.quantified)
				{
					num4++;
					if (world.GetAbstractRoom(i + world.firstRoomIndex).creatures[j].creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate)
					{
						num2++;
					}
				}
			}
			for (int k = 0; k < world.GetAbstractRoom(i + world.firstRoomIndex).entitiesInDens.Count; k++)
			{
				if (world.GetAbstractRoom(i + world.firstRoomIndex).entitiesInDens[k] is AbstractCreature && !(world.GetAbstractRoom(i + world.firstRoomIndex).entitiesInDens[k] as AbstractCreature).creatureTemplate.quantified)
				{
					num5++;
					if ((world.GetAbstractRoom(i + world.firstRoomIndex).entitiesInDens[k] as AbstractCreature).creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate)
					{
						num3++;
					}
				}
			}
		}
		string text = "";
		text = text + "Total flies in world: " + num;
		text += "\r\n";
		text = text + "Total lizards: " + num2 + " + " + num3 + " (" + (num2 + num3) + ")";
		text += "\r\n";
		text = text + "Total creatures: " + num4 + " + " + num5 + " (" + (num4 + num5) + ")";
		text += "\r\n";
		text = text + "Rain Cycle Time Left: " + world.rainCycle.TimeUntilRain / 40;
		text = text + " (" + world.rainCycle.TimeUntilSunset / 40 + ")";
		text += "\r\n";
		if (ModManager.MSC)
		{
			text = text + "Pre Cycle Countdown: " + world.rainCycle.preTimer;
			text += "\r\n";
		}
		if (room.game.IsStorySession)
		{
			text = text + "Dynamic difficulty: " + room.game.GetStorySession.difficulty;
			text += "\r\n";
			text = text + "How well player do: " + room.game.GetStorySession.saveState.deathPersistentSaveData.howWellIsPlayerDoing;
			text += "\r\n";
		}
		infoText.text = text;
		for (int l = 0; l < communityLabels.GetLength(0); l++)
		{
			for (int m = 0; m < communityLabels.GetLength(1); m++)
			{
				text = "";
				if (l == 0)
				{
					text = m switch
					{
						0 => text + "PLAYER RELATIONSHIPS", 
						1 => text + "Global", 
						_ => (m != communityLabels.GetLength(1) - 1) ? ((!world.game.IsStorySession) ? (text + "PLR " + (m - 1)) : (text + world.game.rainWorld.progression.regionNames[m - 2])) : (text + "Filtered Local"), 
					};
				}
				else
				{
					CreatureCommunities.CommunityID communityID = new CreatureCommunities.CommunityID(ExtEnum<CreatureCommunities.CommunityID>.values.GetEntry(l));
					if (m == 0)
					{
						text += communityID;
					}
					else if (world.game.IsStorySession)
					{
						if (m == communityLabels.GetLength(1) - 1)
						{
							if (l > 1)
							{
								text += (world.game.session.creatureCommunities.LikeOfPlayer(communityID, world.RegionNumber, 0) * 100f).ToString("n1");
							}
						}
						else if (!world.singleRoomWorld || m == 1)
						{
							text += (world.game.session.creatureCommunities.playerOpinions[l - 1, m - 1, 0] * 100f).ToString("n1");
						}
					}
					else if (m > 1 && m <= world.game.session.Players.Count + 1)
					{
						text += (world.game.session.creatureCommunities.LikeOfPlayer(communityID, world.RegionNumber, m - 2) * 100f).ToString("n1");
					}
				}
				communityLabels[l, m].text = text;
			}
		}
	}
}
