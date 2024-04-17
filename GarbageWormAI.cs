using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class GarbageWormAI : ArtificialIntelligence
{
	private class CreatureInterest
	{
		public GarbageWormAI ai;

		public Tracker.CreatureRepresentation crit;

		public float baseInterest;

		public float interest;

		public int ticksSinceDiscovered;

		public int ticksSinceVisual;

		public float danger;

		private bool lastVisual;

		public CreatureInterest(GarbageWormAI ai, Tracker.CreatureRepresentation crit, float baseInterest)
		{
			this.ai = ai;
			this.crit = crit;
			this.baseInterest = baseInterest;
			interest = baseInterest;
			if (crit.representedCreature.creatureTemplate.bodySize >= 1f)
			{
				danger = crit.representedCreature.creatureTemplate.bodySize;
			}
		}

		public void Update()
		{
			ticksSinceDiscovered++;
			interest = baseInterest;
			if (crit.VisualContact && !lastVisual && ticksSinceVisual > 40)
			{
				ticksSinceVisual = 0;
			}
			else
			{
				ticksSinceVisual += Random.Range(0, 3);
			}
			lastVisual = crit.VisualContact;
			float num = Vector2.Distance(ai.creature.realizedCreature.bodyChunks[1].pos, ai.creature.realizedCreature.room.MiddleOfTile(crit.BestGuessForPosition()));
			if (danger == 0f)
			{
				interest /= 800 + ticksSinceDiscovered;
				interest /= 80 + ticksSinceVisual;
			}
			else
			{
				interest /= 800f + (float)ticksSinceDiscovered * Mathf.InverseLerp(40f, 660f, num);
				interest /= 80f + (float)ticksSinceDiscovered * Mathf.InverseLerp(40f, 220f, num);
			}
			interest /= Mathf.Lerp(400f, num, 0.3f);
			interest *= 25600000f;
			if (crit.VisualContact)
			{
				interest *= Mathf.Lerp(1f + crit.representedCreature.realizedCreature.mainBodyChunk.vel.magnitude, 3f, 0.75f);
			}
		}
	}

	private List<CreatureInterest> creatureInterests;

	private CreatureInterest currentInterest;

	public List<IntVector2> floorTiles;

	private IntVector2 lookAtFloor;

	private float s;

	private int retractCounter;

	private Vector2 shakeDir;

	public int attackCounter;

	public int searchCounter;

	public int comeBackOutCounter;

	public bool showAsAngry;

	private GarbageWorm worm => creature.realizedCreature as GarbageWorm;

	public float stress
	{
		get
		{
			return s;
		}
		set
		{
			s = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public bool searchingGarbage => searchCounter > 0;

	public GarbageWormAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		AddModule(new Tracker(this, 10, 10, 200, 0.5f, 10, 5, 20));
		creatureInterests = new List<CreatureInterest>();
		shakeDir = Custom.DegToVec(Mathf.Lerp(-140f, 140f, Random.value));
		for (int i = world.firstRoomIndex; i < world.firstRoomIndex + world.NumberOfRooms; i++)
		{
			for (int j = 0; j < world.GetAbstractRoom(i).creatures.Count; j++)
			{
				if (world.GetAbstractRoom(i).creatures[j].state.dead || !(world.GetAbstractRoom(i).creatures[j].creatureTemplate.type == CreatureTemplate.Type.GarbageWorm) || world.GetAbstractRoom(i).creatures[j] == creature)
				{
					continue;
				}
				for (int k = 0; k < (world.GetAbstractRoom(i).creatures[j].state as GarbageWormState).angryAt.Count; k++)
				{
					if (!(creature.state as GarbageWormState).angryAt.Contains((world.GetAbstractRoom(i).creatures[j].state as GarbageWormState).angryAt[k]))
					{
						(creature.state as GarbageWormState).angryAt.Add((world.GetAbstractRoom(i).creatures[j].state as GarbageWormState).angryAt[k]);
					}
				}
			}
		}
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
	}

	public void MapFloor(Room room)
	{
		floorTiles = new List<IntVector2>();
		for (int i = -1; i < 2; i += 2)
		{
			IntVector2 tilePosition = room.GetTilePosition(worm.rootPos);
			for (int j = 0; j < (int)(12f * worm.bodySize); j++)
			{
				bool flag = false;
				for (int k = -1; k < 2; k++)
				{
					if (!room.GetTile(tilePosition + new IntVector2(i, k)).Solid && room.GetTile(tilePosition + new IntVector2(i, k - 1)).Solid)
					{
						tilePosition += new IntVector2(i, k);
						flag = true;
						break;
					}
				}
				if (flag)
				{
					floorTiles.Add(tilePosition);
				}
				if (!flag)
				{
					break;
				}
			}
		}
		if (floorTiles.Count == 0)
		{
			floorTiles.Add(worm.room.garbageHoles[worm.hole]);
		}
		lookAtFloor = floorTiles[Random.Range(0, floorTiles.Count)];
	}

	public override void Update()
	{
		base.Update();
		showAsAngry = false;
		if (attackCounter > 0)
		{
			attackCounter++;
			if (attackCounter > 220)
			{
				attackCounter = 0;
			}
			return;
		}
		float num = float.MinValue;
		CreatureInterest creatureInterest = null;
		for (int num2 = creatureInterests.Count - 1; num2 >= 0; num2--)
		{
			creatureInterests[num2].Update();
			float num3 = creatureInterests[num2].interest * ((creatureInterests[num2] == currentInterest) ? 1.2f : 1f);
			if (num3 > num)
			{
				num = num3;
				creatureInterest = creatureInterests[num2];
			}
			if (creatureInterests[num2].crit.deleteMeNextFrame)
			{
				creatureInterests.RemoveAt(num2);
			}
		}
		stress -= 0.005f;
		AbstractCreature abstractCreature = base.creature.Room.creatures[Random.Range(0, base.creature.Room.creatures.Count)];
		if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.GarbageWorm && (abstractCreature.state as GarbageWormState).angryAt.Count > 0 && (!ModManager.MSC || (!base.creature.controlled && !abstractCreature.controlled)))
		{
			EntityID item = (abstractCreature.state as GarbageWormState).angryAt[Random.Range(0, (abstractCreature.state as GarbageWormState).angryAt.Count)];
			if (!(base.creature.state as GarbageWormState).angryAt.Contains(item))
			{
				(base.creature.state as GarbageWormState).angryAt.Add(item);
			}
		}
		if (creatureInterest != null && num > 0.05f)
		{
			currentInterest = creatureInterest;
			if (creatureInterest.crit.VisualContact)
			{
				worm.lookPoint = creatureInterest.crit.representedCreature.realizedCreature.DangerPos;
			}
			else
			{
				worm.lookPoint = worm.room.MiddleOfTile(creatureInterest.crit.BestGuessForPosition());
			}
			float danger = creatureInterest.danger;
			if (creatureInterest.crit.VisualContact)
			{
				danger *= 7f + creatureInterest.crit.representedCreature.realizedCreature.mainBodyChunk.vel.magnitude;
				danger /= Vector2.Distance(worm.mainBodyChunk.pos, creatureInterest.crit.representedCreature.realizedCreature.mainBodyChunk.pos);
			}
			else
			{
				danger /= Vector2.Distance(worm.mainBodyChunk.pos, worm.room.MiddleOfTile(creatureInterest.crit.BestGuessForPosition()));
			}
			stress += danger / 5f;
			searchCounter = 0;
			if (worm.grabSpears && worm.extended > 0.5f && worm.grasps[0] == null && creatureInterest.crit.VisualContact)
			{
				Weapon weapon = null;
				Creature realizedCreature = creatureInterest.crit.representedCreature.realizedCreature;
				if (realizedCreature.grasps != null)
				{
					for (int i = 0; i < realizedCreature.grasps.Length; i++)
					{
						if (realizedCreature.grasps[i] != null && realizedCreature.grasps[i].grabbed is Weapon && realizedCreature.grasps[i].grabbed is Spear)
						{
							weapon = realizedCreature.grasps[i].grabbed as Weapon;
							break;
						}
					}
				}
				if (weapon == null && realizedCreature is Player && (realizedCreature as Player).spearOnBack != null && (realizedCreature as Player).spearOnBack.spear != null)
				{
					weapon = (realizedCreature as Player).spearOnBack.spear;
				}
				if (weapon != null && Custom.DistLess(worm.mainBodyChunk.pos, weapon.firstChunk.pos, worm.tentacle.idealLength * 0.8f))
				{
					worm.chargePos = weapon.firstChunk.pos;
					if (Custom.DistLess(worm.mainBodyChunk.pos, weapon.firstChunk.pos, 10f))
					{
						worm.Grab(weapon, 0, 0, Creature.Grasp.Shareability.NonExclusive, 0.1f, overrideEquallyDominant: true, pacifying: true);
						worm.room.PlaySound(SoundID.Garbage_Worm_Snatch_Spear, worm.mainBodyChunk);
						worm.grabSpears = false;
						weapon.Forbid();
						Creature creature = null;
						int num4 = -1;
						for (int j = 0; j < weapon.grabbedBy.Count; j++)
						{
							if (weapon.grabbedBy[j].grabber == realizedCreature)
							{
								creature = realizedCreature;
								num4 = weapon.grabbedBy[j].graspUsed;
							}
						}
						if (creature == null && weapon.grabbedBy.Count > 0)
						{
							creature = weapon.grabbedBy[0].grabber;
							num4 = weapon.grabbedBy[0].graspUsed;
						}
						if (creature != null && num4 > -1)
						{
							realizedCreature.ReleaseGrasp(num4);
						}
					}
				}
				else if (weapon == null && AngryAtCreature(realizedCreature))
				{
					worm.chargePos = realizedCreature.mainBodyChunk.pos;
					for (int k = 0; k < realizedCreature.bodyChunks.Length; k++)
					{
						if (Custom.DistLess(worm.mainBodyChunk.pos, realizedCreature.bodyChunks[k].pos, 10f + realizedCreature.bodyChunks[k].rad))
						{
							worm.Grab(realizedCreature, 0, k, Creature.Grasp.Shareability.NonExclusive, 0.1f, overrideEquallyDominant: false, pacifying: false);
							worm.room.PlaySound(SoundID.Garbage_Worm_Grab_Creature, worm.mainBodyChunk);
							worm.grabSpears = false;
							break;
						}
					}
				}
			}
			if (worm.grasps[0] != null && worm.grasps[0].grabbed is Creature)
			{
				retractCounter = 0;
				float num5 = ((worm.grasps[0].grabbed as Creature).dead ? 40f : 400f);
				if (floorTiles.Count > 0)
				{
					if (lookAtFloor.y > worm.room.defaultWaterLevel - 3)
					{
						lookAtFloor = floorTiles[Random.Range(0, floorTiles.Count)];
						if (Random.value < 1f / 7f)
						{
							shakeDir = Custom.DegToVec(Mathf.Lerp(-140f, 140f, Random.value));
						}
						worm.chargePos = worm.mainBodyChunk.pos + shakeDir * 200f;
						worm.mainBodyChunk.vel += shakeDir * Random.value * 25f;
						num5 = ((worm.grasps[0].grabbedChunk.vel.magnitude > 15f) ? 20f : 120f);
					}
					else
					{
						if (Random.value < 1f / 120f)
						{
							lookAtFloor = floorTiles[Random.Range(0, floorTiles.Count)];
						}
						worm.chargePos = worm.room.MiddleOfTile(lookAtFloor) + Custom.DegToVec(Random.value * 360f) * Random.value * 10f + new Vector2(0f, -200f);
					}
				}
				if (Random.value < 1f / num5)
				{
					worm.ReleaseGrasp(0);
				}
			}
		}
		else
		{
			currentInterest = null;
			if ((Random.value < 0.025f || searchCounter < 5) && floorTiles.Count > 0)
			{
				lookAtFloor = floorTiles[Random.Range(0, floorTiles.Count)];
				worm.lookPoint = worm.room.MiddleOfTile(lookAtFloor) + new Vector2(Mathf.Lerp(-10f, 10f, Random.value), -10f);
			}
			searchCounter++;
			if (searchCounter > 100 && Random.value < 0.003030303f && Custom.DistLess(worm.mainBodyChunk.pos, worm.lookPoint, 100f))
			{
				attackCounter = 1;
				searchCounter = 0;
			}
		}
		float num6 = 340f;
		if (creatureInterest != null)
		{
			if (creatureInterest.crit.representedCreature.creatureTemplate.IsVulture)
			{
				num6 = 1000f;
			}
			else if (creatureInterest.crit.representedCreature.creatureTemplate.type == CreatureTemplate.Type.BigEel)
			{
				num6 = 1000f;
			}
		}
		if (stress > 0.9f && creatureInterest != null && Vector2.Distance(worm.bodyChunks[1].pos, worm.room.MiddleOfTile(creatureInterest.crit.BestGuessForPosition())) < num6)
		{
			retractCounter++;
			if (Vector2.Distance(worm.bodyChunks[1].pos, worm.room.MiddleOfTile(creatureInterest.crit.BestGuessForPosition())) < 40f)
			{
				retractCounter += 10;
			}
			if (Vector2.Distance(worm.bodyChunks[0].pos, worm.room.MiddleOfTile(creatureInterest.crit.BestGuessForPosition())) < 40f)
			{
				retractCounter++;
			}
			if (retractCounter > 40)
			{
				worm.Retract();
				retractCounter = 0;
			}
		}
		else if (retractCounter > 0)
		{
			retractCounter--;
		}
		if (worm.extended == 0f && worm.room.world.rainCycle.TimeUntilRain > 60 && retractCounter == 0)
		{
			comeBackOutCounter += Random.Range(0, 3);
			if (comeBackOutCounter > 200)
			{
				worm.Extend();
			}
		}
		else
		{
			comeBackOutCounter = 0;
		}
		if (creatureInterest != null && creatureInterest.crit != null && creatureInterest.crit.representedCreature != null && creatureInterest.crit.representedCreature.realizedCreature != null && AngryAtCreature(creatureInterest.crit.representedCreature.realizedCreature))
		{
			showAsAngry = true;
		}
	}

	private bool AngryAtCreature(Creature crit)
	{
		if (!crit.dead)
		{
			return (creature.state as GarbageWormState).angryAt.Contains(crit.abstractCreature.ID);
		}
		return false;
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.type == CreatureTemplate.Type.GarbageWorm || otherCreature.creatureTemplate.type == CreatureTemplate.Type.Leech)
		{
			return null;
		}
		Tracker.SimpleCreatureRepresentation simpleCreatureRepresentation = new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, otherCreature.creatureTemplate.bodySize, forgetWhenNotVisible: false);
		creatureInterests.Add(new CreatureInterest(this, simpleCreatureRepresentation, otherCreature.creatureTemplate.bodySize));
		return simpleCreatureRepresentation;
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed);
	}

	public bool CurrentlyLookingAtScaryCreature()
	{
		if (currentInterest == null)
		{
			return false;
		}
		return currentInterest.danger > 0f;
	}

	public static void MoveAbstractCreatureToGarbage(AbstractCreature creature, AbstractRoom abstrRoom)
	{
		if (abstrRoom.garbageHoles <= 0)
		{
			return;
		}
		for (int i = 0; i < abstrRoom.nodes.Length; i++)
		{
			if (abstrRoom.nodes[i].type == AbstractRoomNode.Type.GarbageHoles)
			{
				creature.pos.abstractNode = i;
				break;
			}
		}
	}
}
