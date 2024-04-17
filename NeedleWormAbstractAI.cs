using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class NeedleWormAbstractAI : AbstractCreatureAI
{
	public class NeedleWormState : HealthState
	{
		public bool eggSpawn;

		public List<int> confirmedNarrowRooms;

		public NeedleWormState(AbstractCreature crit)
			: base(crit)
		{
			confirmedNarrowRooms = new List<int>();
		}

		public override string ToString()
		{
			string text = HealthBaseSaveString() + (eggSpawn ? "<cB>EGGSPAWN" : "");
			foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
			{
				text = text + "<cB>" + unrecognizedSaveString.Key + "<cC>" + unrecognizedSaveString.Value;
			}
			return text;
		}

		public override void LoadFromString(string[] s)
		{
			base.LoadFromString(s);
			for (int i = 0; i < s.Length; i++)
			{
				string text = Regex.Split(s[i], "<cC>")[0];
				if (text != null && text == "EGGSPAWN")
				{
					eggSpawn = true;
				}
			}
			unrecognizedSaveStrings.Remove("EGGSPAWN");
		}
	}

	public AbstractCreature mother;

	public bool migrateActive;

	public int migrateActiveCounter;

	private bool Small => parent.creatureTemplate.type == CreatureTemplate.Type.SmallNeedleWorm;

	public NeedleWormAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		AbstractRoom abstractRoom = world.GetAbstractRoom(parent.pos);
		migrateActive = Random.value < 0.5f;
		if (Small)
		{
			FindMotherInRoom(abstractRoom);
			return;
		}
		for (int i = 0; i < abstractRoom.creatures.Count; i++)
		{
			if (abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.SmallNeedleWorm)
			{
				(abstractRoom.creatures[i].abstractAI as NeedleWormAbstractAI).FindMotherInRoom(abstractRoom);
			}
		}
	}

	private bool LikeRoom()
	{
		if (parent.realizedCreature != null)
		{
			return (RealAI as NeedleWormAI).LikeRoom();
		}
		if (parent.Room.AttractionForCreature(parent.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
		{
			return parent.Room.AttractionForCreature(parent.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Avoid;
		}
		return false;
	}

	public override void AbstractBehavior(int time)
	{
		if (Small)
		{
			SmallBehavior(time);
		}
		else
		{
			BigBehavior(time);
		}
		migrateActiveCounter -= time;
		if (migrateActiveCounter < 0)
		{
			migrateActiveCounter = Random.Range(1200, 3000);
			migrateActive = !migrateActive;
		}
		if (followCreature != null)
		{
			migrateActive = true;
		}
		if (path.Count > 0 && parent.realizedCreature == null)
		{
			FollowPath(time);
		}
	}

	private void SmallBehavior(int time)
	{
		if (mother != null)
		{
			followCreature = mother;
			MoveWithCreature(mother, goToCreatureDestination: false);
			if (mother.state.dead && TimeInfluencedRandomRoll(0.0016666667f, time))
			{
				mother = null;
				followCreature = null;
			}
		}
		else
		{
			FindMotherInRoom(world.GetAbstractRoom(Random.Range(world.firstRoomIndex, world.firstRoomIndex + world.NumberOfRooms)));
			if ((migrateActive || !LikeRoom()) && (parent.realizedCreature == null || (RealAI as NeedleWormAI).MigrationBehaviorRoll()))
			{
				MigrationBehavior(time);
			}
		}
	}

	private void BigBehavior(int time)
	{
		if (!world.singleRoomWorld && (migrateActive || !LikeRoom()) && (parent.realizedCreature == null || (RealAI as NeedleWormAI).MigrationBehaviorRoll()))
		{
			MigrationBehavior(time);
		}
	}

	public void FindMotherInRoom(AbstractRoom room)
	{
		if (room == null)
		{
			return;
		}
		float num = float.MinValue;
		for (int i = 0; i < room.creatures.Count; i++)
		{
			if (room.creatures[i].creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm && room.creatures[i].state.alive)
			{
				float num2 = Random.value;
				if (room.creatures[i].pos == parent.pos)
				{
					num2 += 1f;
				}
				if (room.creatures[i].abstractAI.denPosition.HasValue && base.denPosition.HasValue && room.creatures[i].abstractAI.denPosition.Value == base.denPosition.Value)
				{
					num2 += 1f;
				}
				if (num2 > num)
				{
					num = num2;
					mother = room.creatures[i];
				}
			}
		}
		for (int j = 0; j < room.entitiesInDens.Count; j++)
		{
			if (room.entitiesInDens[j] is AbstractCreature && (room.entitiesInDens[j] as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm && (room.entitiesInDens[j] as AbstractCreature).state.alive)
			{
				float num3 = Random.value;
				if (room.entitiesInDens[j].pos == parent.pos)
				{
					num3 += 1f;
				}
				if ((room.entitiesInDens[j] as AbstractCreature).abstractAI.denPosition.HasValue && base.denPosition.HasValue && (room.entitiesInDens[j] as AbstractCreature).abstractAI.denPosition.Value == base.denPosition.Value)
				{
					num3 += 1f;
				}
				if (num3 > num)
				{
					num = num3;
					mother = room.entitiesInDens[j] as AbstractCreature;
				}
			}
		}
	}
}
