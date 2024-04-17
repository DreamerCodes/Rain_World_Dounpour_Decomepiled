using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class SporePuffVisionObscurer : VisionObscurer
{
	private float prog;

	public bool doNotCallDeer;

	public SporePuffVisionObscurer(Vector2 pos)
		: base(pos, 70f, 140f, 1f)
	{
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		float num = prog;
		prog += 1f / 130f;
		if (num <= 0.3f && prog > 0.3f)
		{
			AttractADeer();
		}
		obscureFac = Mathf.InverseLerp(1f, 0.3f, prog);
		rad = Mathf.Lerp(70f, 140f, Mathf.Pow(prog, 0.5f));
		if (prog > 1f)
		{
			Destroy();
		}
	}

	private void AttractADeer()
	{
		if (doNotCallDeer)
		{
			return;
		}
		WorldCoordinate worldCoordinate = room.GetWorldCoordinate(pos);
		if (!room.aimap.TileAccessibleToCreature(worldCoordinate.Tile, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer)))
		{
			for (int i = 0; i < 7; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (room.aimap.TileAccessibleToCreature(worldCoordinate.Tile + Custom.eightDirections[j] * i, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer)))
					{
						worldCoordinate.Tile += Custom.eightDirections[j] * i;
						i = 1000;
						break;
					}
				}
			}
		}
		CreatureSpecificAImap creatureSpecificAImap = room.aimap.CreatureSpecificAImap(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer));
		int num = int.MaxValue;
		int num2 = -1;
		for (int k = 0; k < creatureSpecificAImap.numberOfNodes; k++)
		{
			if (room.abstractRoom.nodes[room.abstractRoom.CreatureSpecificToCommonNodeIndex(k, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer))].entranceWidth > 4 && creatureSpecificAImap.GetDistanceToExit(worldCoordinate.x, worldCoordinate.y, k) > 0 && creatureSpecificAImap.GetDistanceToExit(worldCoordinate.x, worldCoordinate.y, k) < num)
			{
				num = creatureSpecificAImap.GetDistanceToExit(worldCoordinate.x, worldCoordinate.y, k);
				num2 = k;
			}
		}
		if (num2 > -1)
		{
			worldCoordinate.abstractNode = room.abstractRoom.CreatureSpecificToCommonNodeIndex(num2, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer));
		}
		List<AbstractCreature> list = new List<AbstractCreature>();
		for (int l = 0; l < room.abstractRoom.creatures.Count; l++)
		{
			if (room.abstractRoom.creatures[l].creatureTemplate.type == CreatureTemplate.Type.Deer && room.abstractRoom.creatures[l].realizedCreature != null && room.abstractRoom.creatures[l].realizedCreature.Consious && (room.abstractRoom.creatures[l].realizedCreature as Deer).AI.goToPuffBall == null && (room.abstractRoom.creatures[l].realizedCreature as Deer).AI.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
			{
				list.Add(room.abstractRoom.creatures[l]);
			}
		}
		if (list.Count > 0)
		{
			(list[Random.Range(0, list.Count)].abstractAI as DeerAbstractAI).AttractToSporeCloud(worldCoordinate);
			Custom.Log("A DEER IN THE ROOM WAS ATTRACTED!");
			room.PlaySound(SoundID.In_Room_Deer_Summoned, 0f, 1f, 1f);
			if (Random.value < 0.5f)
			{
				return;
			}
		}
		if (room.world.rainCycle.TimeUntilRain < 800)
		{
			return;
		}
		bool flag = false;
		if (!ModManager.MMF)
		{
			for (int m = 0; m < DeerAbstractAI.UGLYHARDCODEDALLOWEDROOMS.Length; m++)
			{
				if (flag)
				{
					break;
				}
				if (DeerAbstractAI.UGLYHARDCODEDALLOWEDROOMS[m] == room.abstractRoom.name)
				{
					flag = true;
				}
			}
		}
		else
		{
			if (DeerAbstractAI.ALLOWEDROOMS == null)
			{
				return;
			}
			for (int n = 0; n < DeerAbstractAI.ALLOWEDROOMS.Count; n++)
			{
				if (flag)
				{
					break;
				}
				if (DeerAbstractAI.ALLOWEDROOMS[n] == room.abstractRoom.name)
				{
					flag = true;
				}
			}
		}
		if (!flag || !worldCoordinate.NodeDefined)
		{
			return;
		}
		list.Clear();
		for (int num3 = 0; num3 < room.world.NumberOfRooms; num3++)
		{
			if (room.world.firstRoomIndex + num3 == room.abstractRoom.index)
			{
				continue;
			}
			AbstractRoom abstractRoom = room.world.GetAbstractRoom(room.world.firstRoomIndex + num3);
			if (abstractRoom.realizedRoom != null)
			{
				continue;
			}
			for (int num4 = 0; num4 < abstractRoom.creatures.Count; num4++)
			{
				if (abstractRoom.creatures[num4].creatureTemplate.type == CreatureTemplate.Type.Deer)
				{
					list.Add(abstractRoom.creatures[num4]);
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		(list[Random.Range(0, list.Count)].abstractAI as DeerAbstractAI).AttractToSporeCloud(worldCoordinate);
		float pan = 0f;
		if (room.abstractRoom.nodes[worldCoordinate.abstractNode].type == AbstractRoomNode.Type.SideExit)
		{
			RoomBorderExit roomBorderExit = room.borderExits[worldCoordinate.abstractNode - room.exitAndDenIndex.Length];
			if (roomBorderExit.borderTiles[0].x == 0)
			{
				pan = -1f;
			}
			else if (roomBorderExit.borderTiles[0].x == room.TileWidth - 1)
			{
				pan = 1f;
			}
		}
		room.PlaySound(SoundID.Distant_Deer_Summoned, pan, 1f, 1f);
		Custom.Log($"A DEER WAS ATTRACTED! {worldCoordinate}");
	}
}
