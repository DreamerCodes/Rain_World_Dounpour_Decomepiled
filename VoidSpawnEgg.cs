using System;
using RWCustom;
using UnityEngine;

public class VoidSpawnEgg : CosmeticSprite, INotifyWhenRoomIsReady
{
	public VoidSpawn spawn;

	public int originRoom;

	public int placedObjectIndex;

	public PlacedObject placedObject;

	public float lastRad;

	public float rad;

	public bool holdingSpawn = true;

	public int wiggleCounter;

	public float popping;

	public VoidSpawnEgg(Room room, int placedObjectIndex, PlacedObject placedObject)
	{
		base.room = room;
		originRoom = room.abstractRoom.index;
		this.placedObjectIndex = placedObjectIndex;
		this.placedObject = placedObject;
		spawn = new VoidSpawn(new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.VoidSpawn, null, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID()), room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt), VoidSpawnKeeper.DayLightMode(room));
		spawn.egg = this;
		spawn.PlaceInRoom(room);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (holdingSpawn)
		{
			wiggleCounter--;
			if (wiggleCounter < 1)
			{
				spawn.consious = !spawn.consious;
				wiggleCounter = UnityEngine.Random.Range(10, spawn.consious ? 80 : 220);
			}
			if (spawn.consious)
			{
				for (int i = 0; i < spawn.bodyChunks.Length; i++)
				{
					spawn.bodyChunks[i].vel += Custom.RNV() * 1f * UnityEngine.Random.value;
				}
			}
			spawn.inEggMode = 1f;
		}
		Vector2 a = new Vector2(0f, 0f);
		float num = 0f;
		for (int j = 0; j < spawn.bodyChunks.Length; j++)
		{
			a += spawn.bodyChunks[j].pos * spawn.bodyChunks[j].mass;
			num += spawn.bodyChunks[j].mass;
		}
		a /= num;
		a = Vector2.Lerp(a, placedObject.pos, 0.9f);
		float num2 = 0f;
		for (int k = 0; k < spawn.bodyChunks.Length; k++)
		{
			num2 += (Vector2.Distance(a, spawn.bodyChunks[k].pos) + spawn.bodyChunks[k].rad) * spawn.bodyChunks[k].mass;
		}
		num2 /= num;
		float num3 = Mathf.Lerp(num * 7f, 15f, 0.65f);
		num2 = ((!(num2 < num3)) ? Mathf.Lerp(num2, num3, 0.2f) : Mathf.Lerp(num2, num3, 0.8f));
		if (holdingSpawn)
		{
			for (int l = 0; l < spawn.bodyChunks.Length; l++)
			{
				float num4 = Mathf.Max(0f, num2 - spawn.bodyChunks[l].rad);
				if (!Custom.DistLess(spawn.bodyChunks[l].pos, a, num4))
				{
					Vector2 vector = Custom.DirVec(spawn.bodyChunks[l].pos, a);
					spawn.bodyChunks[l].vel -= (num4 - Vector2.Distance(spawn.bodyChunks[l].pos, a)) * vector * 0.5f;
					spawn.bodyChunks[l].pos -= (num4 - Vector2.Distance(spawn.bodyChunks[l].pos, a)) * vector * 0.5f;
				}
			}
		}
		pos = new Vector2(0f, 0f);
		for (int m = 0; m < spawn.bodyChunks.Length; m++)
		{
			pos += spawn.bodyChunks[m].pos * spawn.bodyChunks[m].mass;
		}
		pos /= num;
		pos = Vector2.Lerp(pos, a, 0.5f);
		lastRad = rad;
		if (popping > 0f)
		{
			popping += 1f / 30f;
			rad += 3f * Mathf.Sin(popping * (float)Math.PI);
			if (popping >= 1f)
			{
				popping = 1f;
				Destroy();
			}
		}
		else
		{
			rad = num2;
			for (int n = 0; n < spawn.bodyChunks.Length; n++)
			{
				rad = Mathf.Max(rad, Vector2.Distance(pos, spawn.bodyChunks[n].pos) + spawn.bodyChunks[n].rad);
			}
			for (int num5 = 0; num5 < room.game.Players.Count; num5++)
			{
				if (popping != 0f)
				{
					break;
				}
				if (room.game.Players[num5].realizedCreature == null || room.game.Players[num5].realizedCreature.room != room)
				{
					continue;
				}
				for (int num6 = 0; num6 < room.game.Players[num5].realizedCreature.bodyChunks.Length; num6++)
				{
					if (Custom.DistLess(room.game.Players[num5].realizedCreature.bodyChunks[num6].pos, pos, room.game.Players[num5].realizedCreature.bodyChunks[num6].rad + rad))
					{
						popping = 0.01f;
						holdingSpawn = false;
						spawn.consious = true;
						(room.game.session as StoryGameSession).saveState.ReportConsumedItem(room.world, karmaFlower: false, originRoom, placedObjectIndex, UnityEngine.Random.Range((placedObject.data as PlacedObject.VoidSpawnEggData).minRegen, (placedObject.data as PlacedObject.VoidSpawnEggData).maxRegen));
						break;
					}
				}
			}
		}
		if (holdingSpawn && spawn.graphicsModule != null)
		{
			for (int num7 = 0; num7 < (spawn.graphicsModule as VoidSpawnGraphics).subModules.Count; num7++)
			{
				if (!((spawn.graphicsModule as VoidSpawnGraphics).subModules[num7] is VoidSpawnGraphics.Antenna))
				{
					continue;
				}
				VoidSpawnGraphics.Antenna antenna = (spawn.graphicsModule as VoidSpawnGraphics).subModules[num7] as VoidSpawnGraphics.Antenna;
				for (int num8 = 0; num8 < antenna.segments.GetLength(0); num8++)
				{
					if (!Custom.DistLess(antenna.segments[num8, 0], pos, rad))
					{
						Vector2 vector2 = Custom.DirVec(antenna.segments[num8, 0], pos);
						antenna.segments[num8, 2] -= (rad - Vector2.Distance(antenna.segments[num8, 0], pos)) * vector2;
						antenna.segments[num8, 0] -= (rad - Vector2.Distance(antenna.segments[num8, 0], pos)) * vector2;
					}
				}
			}
		}
		if (spawn.slatedForDeletetion)
		{
			Destroy();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["VectorCircleFadable"];
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].scale = Mathf.Lerp(lastRad, rad, timeStacker) / 8f;
		sLeaser.sprites[0].alpha = 1f / Mathf.Lerp(lastRad, rad, timeStacker);
		if (spawn.graphicsModule != null)
		{
			sLeaser.sprites[0].color = new Color(0.015686275f, 0f, (spawn.graphicsModule as VoidSpawnGraphics).AlphaFromGlowDist((spawn.graphicsModule as VoidSpawnGraphics).glowPos, Vector2.Lerp(lastPos, pos, timeStacker)) * Mathf.Lerp(0.9f, 0.4f, (spawn.graphicsModule as VoidSpawnGraphics).darkness) * (1f - popping));
		}
	}

	public void ShortcutsReady()
	{
	}

	public void AIMapReady()
	{
		if (room.game.StoryCharacter == SlugcatStats.Name.Red)
		{
			spawn.behavior = new VoidSpawn.EggAndAway(spawn, room);
		}
		else
		{
			spawn.behavior = new VoidSpawn.EggToExit(spawn, (placedObject.data as PlacedObject.VoidSpawnEggData).exit, room, spawn.dayLightMode);
		}
	}
}
