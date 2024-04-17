using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class VoidSpawnKeeper : UpdatableAndDeletable
{
	private RoomSettings.RoomEffect effect;

	private VoidSpawnWorldAI worldAI;

	public int toRoom = -1;

	public int[] fromRooms;

	public bool initiated;

	public List<VoidSpawn> spawn;

	private bool lastViewed;

	public float voidMeltInRoom;

	public bool daylightMode;

	public int IdealSpawnNumber => Math.Min((int)((float)(room.TileWidth * room.TileHeight) * effect.amount), 4000) / 40;

	public bool CurrentlyViewed
	{
		get
		{
			if (room.BeingViewed)
			{
				if (!(room.game.cameras[0].pos.y > -0f))
				{
					return room.abstractRoom.name != "SB_L01";
				}
				return true;
			}
			return false;
		}
	}

	public static bool DayLightMode(Room testRoom)
	{
		if (testRoom.world.region == null || testRoom.world.region.name != "SL")
		{
			return false;
		}
		if (testRoom.roomSettings.Palette == 10 || testRoom.roomSettings.Palette == 8)
		{
			return false;
		}
		if (testRoom.roomSettings.fadePalette != null && (testRoom.roomSettings.fadePalette.palette == 10 || testRoom.roomSettings.fadePalette.palette == 8))
		{
			return false;
		}
		return true;
	}

	public VoidSpawnKeeper(Room room, RoomSettings.RoomEffect effect)
	{
		base.room = room;
		this.effect = effect;
		daylightMode = DayLightMode(room);
		if (room.world.voidSpawnWorldAI == null)
		{
			room.world.AddWorldProcess(new VoidSpawnWorldAI(room.world));
		}
		worldAI = room.world.voidSpawnWorldAI;
		spawn = new List<VoidSpawn>();
		voidMeltInRoom = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt);
		fromRooms = new int[0];
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (worldAI.directionFinder == null || !worldAI.directionFinder.done)
		{
			return;
		}
		if (!initiated)
		{
			initiated = true;
			Initiate();
			return;
		}
		if (CurrentlyViewed)
		{
			if (!lastViewed)
			{
				ScatterSpawn();
			}
			for (int num = spawn.Count - 1; num >= 0; num--)
			{
				if (spawn[num].slatedForDeletetion)
				{
					spawn.RemoveAt(num);
				}
			}
			if (spawn.Count < IdealSpawnNumber && UnityEngine.Random.value < 0.2f)
			{
				AddOneSpawn();
			}
		}
		else if (lastViewed)
		{
			for (int num2 = spawn.Count - 1; num2 >= 0; num2--)
			{
				spawn[num2].Destroy();
			}
			spawn.Clear();
		}
		lastViewed = CurrentlyViewed;
	}

	private void Initiate()
	{
		float num = float.MaxValue;
		List<int> list = new List<int>();
		for (int i = 0; i < room.abstractRoom.connections.Length; i++)
		{
			if (room.abstractRoom.connections[i] <= -1)
			{
				continue;
			}
			WorldCoordinate testPos = new WorldCoordinate(room.abstractRoom.index, -1, -1, i);
			WorldCoordinate testPos2 = new WorldCoordinate(room.abstractRoom.connections[i], -1, -1, room.world.GetAbstractRoom(room.abstractRoom.connections[i]).ExitIndex(room.abstractRoom.index));
			float num2 = worldAI.directionFinder.DistanceToDestination(testPos2);
			if (num2 > -1f)
			{
				if (num2 < num)
				{
					num = num2;
					toRoom = room.abstractRoom.connections[i];
				}
				if (worldAI.directionFinder.DistanceToDestination(testPos) > -1f && num2 > worldAI.directionFinder.DistanceToDestination(testPos))
				{
					list.Add(room.abstractRoom.connections[i]);
				}
			}
		}
		list.Remove(toRoom);
		fromRooms = list.ToArray();
		Custom.Log(toRoom.ToString());
		Custom.Log("::TO ROOM:", toRoom.ToString(), (toRoom == -1) ? "NULL" : room.world.GetAbstractRoom(toRoom).name);
		for (int j = 0; j < fromRooms.Length; j++)
		{
			Custom.Log("From room:", room.world.GetAbstractRoom(fromRooms[j]).name);
		}
		if (toRoom == -1)
		{
			Destroy();
		}
	}

	private void ScatterSpawn()
	{
		if (toRoom == -1)
		{
			return;
		}
		for (int i = 0; (float)i < (float)IdealSpawnNumber * 0.7f; i++)
		{
			Vector2 pos = room.RandomPos();
			VoidSpawn voidSpawn = new VoidSpawn(new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.VoidSpawn, null, room.GetWorldCoordinate(pos), room.game.GetNewID()), voidMeltInRoom, daylightMode);
			if (room.abstractRoom.name == "SB_L01" || (ModManager.MSC && (room.abstractRoom.name == "SB_DO6" || room.abstractRoom.name == "SB_E05SAINT")))
			{
				if (room.abstractRoom.name == "SB_E05SAINT")
				{
					voidSpawn.behavior = new VoidSpawn.SwimDown(voidSpawn, room);
				}
				else
				{
					voidSpawn.behavior = new VoidSpawn.VoidSeaDive(voidSpawn, room);
				}
				float value = UnityEngine.Random.value;
				voidSpawn.abstractPhysicalObject.pos = room.GetWorldCoordinate(Vector2.Lerp(new Vector2(160f, 2110f), Custom.RandomPointInRect(VoidSpawn.MillAround.MillRectInRoom("SB_L01")), value) + Custom.RNV() * UnityEngine.Random.value * Mathf.Lerp(500f, 50f, value));
			}
			else if (room.abstractRoom.name == "SH_D02" || room.abstractRoom.name == "SH_E02")
			{
				voidSpawn.behavior = new VoidSpawn.MillAround(voidSpawn, room);
				if (UnityEngine.Random.value < 0.7f)
				{
					voidSpawn.abstractPhysicalObject.pos = room.GetWorldCoordinate(Custom.RandomPointInRect((voidSpawn.behavior as VoidSpawn.MillAround).rect));
				}
			}
			else
			{
				voidSpawn.behavior = new VoidSpawn.PassThrough(voidSpawn, toRoom, room);
				(voidSpawn.behavior as VoidSpawn.PassThrough).pnt = room.MiddleOfTile(voidSpawn.abstractPhysicalObject.pos);
			}
			voidSpawn.PlaceInRoom(room);
			spawn.Add(voidSpawn);
		}
	}

	private void AddOneSpawn()
	{
		if (toRoom != -1)
		{
			Vector2 vector;
			if (fromRooms.Length != 0)
			{
				int roomIndex = fromRooms[UnityEngine.Random.Range(0, fromRooms.Length)];
				vector = room.world.RoomToWorldPos(new Vector2((float)room.world.GetAbstractRoom(roomIndex).size.x * UnityEngine.Random.value * 20f, (float)room.world.GetAbstractRoom(roomIndex).size.y * UnityEngine.Random.value * 20f), roomIndex);
				vector -= room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index);
				vector += Custom.DirVec(room.RandomPos(), vector) * 2000f;
				vector = Custom.RectCollision(room.RandomPos(), vector, room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
			}
			else
			{
				vector = room.RandomPos() + Custom.RNV() * 10000f;
				vector = Custom.RectCollision(room.RandomPos(), vector, room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
			}
			VoidSpawn voidSpawn = new VoidSpawn(new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.VoidSpawn, null, room.GetWorldCoordinate(vector), room.game.GetNewID()), voidMeltInRoom, daylightMode);
			voidSpawn.PlaceInRoom(room);
			if (room.abstractRoom.name == "SB_L01")
			{
				voidSpawn.behavior = new VoidSpawn.VoidSeaDive(voidSpawn, room);
			}
			else if (ModManager.MSC && room.abstractRoom.name == "SB_E05SAINT")
			{
				voidSpawn.behavior = new VoidSpawn.SwimDown(voidSpawn, room);
			}
			else if (room.abstractRoom.name == "SH_D02" || room.abstractRoom.name == "SH_E02")
			{
				voidSpawn.behavior = new VoidSpawn.MillAround(voidSpawn, room);
			}
			else
			{
				voidSpawn.behavior = new VoidSpawn.PassThrough(voidSpawn, toRoom, room);
			}
			spawn.Add(voidSpawn);
		}
	}
}
