using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class InsectCoordinator : UpdatableAndDeletable, Explosion.IReactToExplosions
{
	public class Swarm
	{
		public InsectCoordinator owner;

		public PlacedObject placedObject;

		public List<CosmeticInsect> members;

		public PlacedObject.InsectGroupData insectGroupData => placedObject.data as PlacedObject.InsectGroupData;

		public Swarm(InsectCoordinator owner, PlacedObject placedObject)
		{
			this.owner = owner;
			this.placedObject = placedObject;
			members = new List<CosmeticInsect>();
		}

		public void Initiate()
		{
			float num = (float)Math.PI * Mathf.Lerp(insectGroupData.Rad, 150f, 0.5f) * 2f;
			float num2 = SpeciesDensity(insectGroupData.insectType) * insectGroupData.density;
			for (int num3 = 1 + (int)(num * num2 / 50f); num3 >= 0; num3--)
			{
				Vector2 pos = placedObject.pos + Custom.RNV() * (insectGroupData.Rad * UnityEngine.Random.value + insectGroupData.Rad * 0.1f * Mathf.Pow(UnityEngine.Random.value, 3f));
				if (!owner.room.GetTile(pos).Solid)
				{
					owner.CreateInsect(insectGroupData.insectType, pos, this);
				}
			}
		}
	}

	public List<Swarm> swarms;

	public List<RoomSettings.RoomEffect> effects;

	public List<CosmeticInsect> allInsects;

	public InsectCoordinator(Room room)
	{
		base.room = room;
		swarms = new List<Swarm>();
		effects = new List<RoomSettings.RoomEffect>();
		allInsects = new List<CosmeticInsect>();
	}

	public void AddGroup(PlacedObject newpObj)
	{
		swarms.Add(new Swarm(this, newpObj));
	}

	public void AddEffect(RoomSettings.RoomEffect roomEffect)
	{
		effects.Add(roomEffect);
	}

	public void NowViewed()
	{
		if (room.world.worldGhost != null && room.world.worldGhost.CreaturesSleepInRoom(room.abstractRoom))
		{
			return;
		}
		for (int i = 0; i < effects.Count; i++)
		{
			CosmeticInsect.Type type = RoomEffectToInsectType(effects[i].type);
			for (int num = (int)((float)(room.TileWidth * room.TileHeight) * effects[i].amount * SpeciesDensity(type) / 50f); num >= 0; num--)
			{
				Vector2 vector = new Vector2(UnityEngine.Random.value * room.PixelWidth, UnityEngine.Random.value * room.PixelHeight);
				if (!room.GetTile(vector).Solid && EffectSpawnChanceForInsect(type, room, vector, 0.5f))
				{
					CreateInsect(type, vector, null);
				}
			}
		}
		for (int j = 0; j < swarms.Count; j++)
		{
			swarms[j].Initiate();
		}
	}

	public void NoLongerViewed()
	{
		for (int i = 0; i < allInsects.Count; i++)
		{
			allInsects[i].Destroy();
		}
		allInsects.Clear();
		for (int j = 0; j < swarms.Count; j++)
		{
			swarms[j].members.Clear();
		}
	}

	public void CreateInsect(CosmeticInsect.Type type, Vector2 pos, Swarm swarm)
	{
		if (!TileLegalForInsect(type, room, pos) || room.world.rainCycle.TimeUntilRain < UnityEngine.Random.Range(1200, 1600))
		{
			return;
		}
		CosmeticInsect cosmeticInsect = null;
		if (type == CosmeticInsect.Type.StandardFly)
		{
			cosmeticInsect = new MiniFly(room, pos);
		}
		else if (type == CosmeticInsect.Type.FireFly)
		{
			cosmeticInsect = new FireFly(room, pos);
		}
		else if (type == CosmeticInsect.Type.TinyDragonFly)
		{
			cosmeticInsect = new TinyDragonfly(room, pos);
		}
		else if (type == CosmeticInsect.Type.RockFlea)
		{
			if (!room.PointSubmerged(pos))
			{
				pos = BringPosToGround(pos);
			}
			cosmeticInsect = new RockFlea(room, pos);
		}
		else if (type == CosmeticInsect.Type.GrassHopper)
		{
			pos = BringPosToGround(pos);
			cosmeticInsect = new GrassHopper(room, pos);
		}
		else if (type == CosmeticInsect.Type.RedSwarmer)
		{
			cosmeticInsect = new RedSwarmer(room, pos);
		}
		else if (type == CosmeticInsect.Type.Ant)
		{
			cosmeticInsect = new Ant(room, pos);
		}
		else if (type == CosmeticInsect.Type.Beetle)
		{
			if (UnityEngine.Random.value > 1f / 7f)
			{
				bool flag = false;
				if (!room.readyForAI || room.aimap.getTerrainProximity(pos) < 10)
				{
					for (int i = 0; i < 5; i++)
					{
						Vector2? vector = SharedPhysics.ExactTerrainRayTracePos(room, pos, pos + Custom.RNV() * 200f);
						if (vector.HasValue)
						{
							pos = vector.Value;
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					pos = BringPosToGround(pos);
				}
			}
			cosmeticInsect = new Beetle(room, pos);
		}
		else if (type == CosmeticInsect.Type.WaterGlowworm)
		{
			cosmeticInsect = new WaterGlowworm(room, pos);
		}
		else if (type == CosmeticInsect.Type.Wasp)
		{
			cosmeticInsect = new Wasp(room, pos);
		}
		else if (type == CosmeticInsect.Type.Moth)
		{
			if (!room.readyForAI || room.aimap.getTerrainProximity(pos) < 5)
			{
				for (int j = 0; j < 5; j++)
				{
					Vector2? vector2 = SharedPhysics.ExactTerrainRayTracePos(room, pos, pos + Custom.RNV() * 100f);
					if (vector2.HasValue)
					{
						pos = vector2.Value;
						break;
					}
				}
			}
			cosmeticInsect = new Moth(room, pos);
		}
		if (cosmeticInsect != null)
		{
			allInsects.Add(cosmeticInsect);
			if (swarm != null)
			{
				swarm.members.Add(cosmeticInsect);
				cosmeticInsect.mySwarm = swarm;
			}
			room.AddObject(cosmeticInsect);
		}
	}

	public Vector2 BringPosToGround(Vector2 pos)
	{
		int x = room.GetTilePosition(pos).x;
		for (int num = room.GetTilePosition(pos).y - 1; num >= 0; num--)
		{
			if (room.GetTile(x, num).Solid)
			{
				return new Vector2(pos.x, room.MiddleOfTile(x, num).y + 10f);
			}
		}
		return new Vector2(pos.x, -10f);
	}

	public static float SpeciesDensity(RoomSettings.RoomEffect.Type type)
	{
		return SpeciesDensity(RoomEffectToInsectType(type));
	}

	public static float SpeciesDensity(CosmeticInsect.Type type)
	{
		if (type == CosmeticInsect.Type.FireFly)
		{
			return 0.8f;
		}
		if (type == CosmeticInsect.Type.Ant)
		{
			return 6f;
		}
		if (type == CosmeticInsect.Type.WaterGlowworm)
		{
			return 0.33f;
		}
		if (type == CosmeticInsect.Type.Wasp)
		{
			return 0.15f;
		}
		return 1f;
	}

	public static bool TileLegalForInsect(CosmeticInsect.Type type, Room room, Vector2 testPos)
	{
		if (type == CosmeticInsect.Type.StandardFly || type == CosmeticInsect.Type.FireFly || type == CosmeticInsect.Type.TinyDragonFly || type == CosmeticInsect.Type.GrassHopper || type == CosmeticInsect.Type.RedSwarmer || type == CosmeticInsect.Type.Ant || type == CosmeticInsect.Type.Beetle || type == CosmeticInsect.Type.Wasp || type == CosmeticInsect.Type.Moth)
		{
			return !room.GetTile(testPos).AnyWater;
		}
		if (type == CosmeticInsect.Type.WaterGlowworm)
		{
			return room.GetTile(testPos).DeepWater;
		}
		_ = type == CosmeticInsect.Type.RockFlea;
		return true;
	}

	public static bool EffectSpawnChanceForInsect(CosmeticInsect.Type type, Room room, Vector2 testPos, float effectAmount)
	{
		if (type == CosmeticInsect.Type.StandardFly || type == CosmeticInsect.Type.FireFly || type == CosmeticInsect.Type.TinyDragonFly || type == CosmeticInsect.Type.RedSwarmer || type == CosmeticInsect.Type.Moth)
		{
			return Mathf.Pow(UnityEngine.Random.value, 1f - effectAmount) > (float)(room.readyForAI ? room.aimap.getTerrainProximity(testPos) : 5) * 0.05f;
		}
		if (type == CosmeticInsect.Type.WaterGlowworm || type == CosmeticInsect.Type.RockFlea || type == CosmeticInsect.Type.GrassHopper || type == CosmeticInsect.Type.Ant || type == CosmeticInsect.Type.Wasp)
		{
			return true;
		}
		if (type == CosmeticInsect.Type.Beetle)
		{
			if (room.readyForAI)
			{
				return !room.aimap.getAItile(testPos).narrowSpace;
			}
			return true;
		}
		return true;
	}

	public static CosmeticInsect.Type RoomEffectToInsectType(RoomSettings.RoomEffect.Type type)
	{
		if (type == RoomSettings.RoomEffect.Type.Flies)
		{
			return CosmeticInsect.Type.StandardFly;
		}
		if (type == RoomSettings.RoomEffect.Type.FireFlies)
		{
			return CosmeticInsect.Type.FireFly;
		}
		if (type == RoomSettings.RoomEffect.Type.TinyDragonFly)
		{
			return CosmeticInsect.Type.TinyDragonFly;
		}
		if (type == RoomSettings.RoomEffect.Type.RockFlea)
		{
			return CosmeticInsect.Type.RockFlea;
		}
		if (type == RoomSettings.RoomEffect.Type.RedSwarmer)
		{
			return CosmeticInsect.Type.RedSwarmer;
		}
		if (type == RoomSettings.RoomEffect.Type.Ant)
		{
			return CosmeticInsect.Type.Ant;
		}
		if (type == RoomSettings.RoomEffect.Type.Beetle)
		{
			return CosmeticInsect.Type.Beetle;
		}
		if (type == RoomSettings.RoomEffect.Type.WaterGlowworm)
		{
			return CosmeticInsect.Type.WaterGlowworm;
		}
		if (type == RoomSettings.RoomEffect.Type.Wasp)
		{
			return CosmeticInsect.Type.Wasp;
		}
		if (type == RoomSettings.RoomEffect.Type.Moth)
		{
			return CosmeticInsect.Type.Moth;
		}
		Custom.LogWarning("Insect type invalid! " + type);
		return CosmeticInsect.Type.StandardFly;
	}

	public void Explosion(Explosion explosion)
	{
		float num = Mathf.Lerp(explosion.rad, Mathf.Min(explosion.rad, 90f), 0.5f);
		int num2 = 0;
		for (int i = 0; i < allInsects.Count; i++)
		{
			if (!Custom.DistLess(allInsects[i].pos, explosion.pos, num * 3f))
			{
				continue;
			}
			float num3 = Vector2.Distance(allInsects[i].pos, explosion.pos);
			if (num2 < 15)
			{
				if (!room.VisualContact(allInsects[i].pos, explosion.pos))
				{
					num3 /= 4f;
				}
				num2++;
			}
			allInsects[i].pos += Custom.DirVec(explosion.pos, allInsects[i].pos) * Mathf.Pow(Mathf.InverseLerp(num * 3f, num * 1.5f, num3), 0.3f) * (10f + explosion.force * 6f);
			allInsects[i].vel += Custom.DirVec(explosion.pos, allInsects[i].pos) * Mathf.Pow(Mathf.InverseLerp(num * 2f, num * 0.2f, num3), 2f) * explosion.force * 22.2f;
			if (num3 < explosion.rad)
			{
				allInsects[i].alive = false;
			}
		}
	}
}
