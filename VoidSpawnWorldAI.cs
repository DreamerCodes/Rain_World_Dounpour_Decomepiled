using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;

public class VoidSpawnWorldAI : World.WorldProcess
{
	public class DirectionFinder
	{
		private World world;

		public float[][] matrix;

		public List<IntVector2> checkNext;

		public bool done;

		public int showToRoom = -1;

		public bool destroy;

		public DirectionFinder(World world)
		{
			this.world = world;
			checkNext = new List<IntVector2>();
			matrix = new float[world.NumberOfRooms][];
			for (int i = 0; i < matrix.Length; i++)
			{
				matrix[i] = new float[world.GetAbstractRoom(i + world.firstRoomIndex).connections.Length];
				for (int j = 0; j < matrix[i].Length; j++)
				{
					matrix[i][j] = -1f;
				}
			}
			string text = "";
			text = world.region.name switch
			{
				"SH" => "SH_D02", 
				"SB" => (!ModManager.MSC || !world.game.IsStorySession || !(world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint)) ? "SB_L01" : "SB_E05SAINT", 
				_ => world.GetAbstractRoom(world.region.firstRoomIndex).name, 
			};
			if (world.GetAbstractRoom(text) != null)
			{
				showToRoom = world.GetAbstractRoom(text).index;
			}
			AbstractRoom abstractRoom = world.GetAbstractRoom(showToRoom);
			for (int k = 0; k < abstractRoom.connections.Length; k++)
			{
				checkNext.Add(new IntVector2(abstractRoom.index - world.firstRoomIndex, k));
				matrix[abstractRoom.index - world.firstRoomIndex][k] = 0f;
			}
		}

		public void Update()
		{
			if (done || destroy)
			{
				return;
			}
			if (checkNext.Count < 1)
			{
				done = true;
				return;
			}
			float num = float.MaxValue;
			int num2 = -1;
			for (int i = 0; i < checkNext.Count; i++)
			{
				float num3 = ResistanceOfCell(checkNext[i]);
				if (num3 > -1f && num3 < num)
				{
					num = num3;
					num2 = i;
				}
			}
			if (num2 < 0)
			{
				done = true;
				return;
			}
			IntVector2 testCell = checkNext[num2];
			checkNext.RemoveAt(num2);
			AbstractRoom abstractRoom = world.GetAbstractRoom(testCell.x + world.firstRoomIndex);
			float num4 = ResistanceOfCell(testCell);
			for (int j = 0; j < abstractRoom.connections.Length; j++)
			{
				float num5 = -1f;
				WorldCoordinate worldCoordinate;
				if (j == testCell.y && abstractRoom.connections[j] > -1)
				{
					worldCoordinate = new WorldCoordinate(abstractRoom.connections[j], -1, -1, world.GetAbstractRoom(abstractRoom.connections[j]).ExitIndex(abstractRoom.index));
					num5 = world.TotalShortCutLengthBetweenTwoConnectedRooms(abstractRoom.index, abstractRoom.connections[j]);
				}
				else
				{
					worldCoordinate = new WorldCoordinate(abstractRoom.index, -1, -1, j);
					num5 = abstractRoom.nodes[j].ConnectionLength(testCell.y, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
					if (num5 == -1f)
					{
						num5 = 10000f;
					}
				}
				if (num5 > -1f && matrix[worldCoordinate.room - world.firstRoomIndex][worldCoordinate.abstractNode] == -1f)
				{
					matrix[worldCoordinate.room - world.firstRoomIndex][worldCoordinate.abstractNode] = num4 + num5;
					checkNext.Add(new IntVector2(worldCoordinate.room - world.firstRoomIndex, worldCoordinate.abstractNode));
				}
			}
		}

		private float ResistanceOfCell(IntVector2 testCell)
		{
			if (testCell.x < 0 || testCell.x > world.NumberOfRooms)
			{
				return -1f;
			}
			if (testCell.y < 0 || testCell.y >= matrix[testCell.x].Length)
			{
				return -1f;
			}
			return matrix[testCell.x][testCell.y];
		}

		public float DistanceToDestination(WorldCoordinate testPos)
		{
			if (testPos.room - world.firstRoomIndex < 0 || testPos.room - world.firstRoomIndex > world.NumberOfRooms)
			{
				return -1f;
			}
			return matrix[testPos.room - world.firstRoomIndex][testPos.abstractNode];
		}
	}

	public DirectionFinder directionFinder;

	private bool triedAddProgressionFinder;

	public VoidSpawnWorldAI(World world)
		: base(world)
	{
	}

	public override void Update()
	{
		base.Update();
		if (this.directionFinder == null)
		{
			if (triedAddProgressionFinder)
			{
				return;
			}
			triedAddProgressionFinder = true;
			if (world.region.name == "SH" || world.region.name == "SB")
			{
				DirectionFinder directionFinder = new DirectionFinder(world);
				if (!directionFinder.destroy)
				{
					this.directionFinder = directionFinder;
				}
			}
		}
		else if (!this.directionFinder.done)
		{
			this.directionFinder.Update();
		}
	}
}
