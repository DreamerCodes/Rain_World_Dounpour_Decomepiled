using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Leech : Creature
{
	public class LeechSchool
	{
		public class LeechPrey
		{
			public Creature creature;

			public int forgetCounter;

			public float bitesAfterDeath;

			public LeechPrey(Creature creature)
			{
				this.creature = creature;
				forgetCounter = 0;
				bitesAfterDeath = 1f;
			}
		}

		public List<Leech> leeches;

		public List<LeechPrey> prey;

		public Color col;

		public LeechSchool()
		{
			leeches = new List<Leech>();
			prey = new List<LeechPrey>();
			col = Custom.HSL2RGB(UnityEngine.Random.value, 1f, 0.5f);
		}

		public void AddLeech(Leech leech)
		{
			if (!leeches.Contains(leech))
			{
				leeches.Add(leech);
			}
		}

		public void AddPrey(Creature p)
		{
			for (int i = 0; i < prey.Count; i++)
			{
				if (prey[i].creature == p)
				{
					return;
				}
			}
			prey.Add(new LeechPrey(p));
		}

		public void RemovePrey(Creature p)
		{
			for (int num = prey.Count - 1; num >= 0; num--)
			{
				if (prey[num].creature == p)
				{
					prey.RemoveAt(num);
				}
			}
		}

		public void BitDeadPrey(Creature p)
		{
			for (int i = 0; i < prey.Count; i++)
			{
				if (prey[i].creature == p)
				{
					prey[i].bitesAfterDeath -= 0.01f / prey[i].creature.Template.bodySize;
					if (prey[i].bitesAfterDeath < 0f)
					{
						prey[i].creature.leechedOut = true;
					}
					break;
				}
			}
		}

		public void Merge(LeechSchool school)
		{
			if (school != this)
			{
				for (int i = 0; i < school.leeches.Count; i++)
				{
					school.leeches[i].school = this;
					AddLeech(school.leeches[i]);
				}
				for (int j = 0; j < school.prey.Count; j++)
				{
					AddPrey(school.prey[j].creature);
				}
			}
		}

		public void RemoveLeech(Leech leech)
		{
			leeches.Remove(leech);
			leech.school = null;
		}
	}

	public Vector2 swimDirection;

	public LeechSchool school;

	public Creature huntPrey;

	public Vector2? offShootGoal;

	public IntVector2? seaLeechTerritoryTile;

	private int seaLeechFarFromTerritoryCounter;

	private int lonerCounter;

	private IntVector2 avoidThisShortCutEntrance;

	public int landWalkDir;

	public float landWalkCycle;

	private int chargeCounter;

	public float airDrown;

	public bool fleeFromRain;

	private List<Vector3> afraidOfPositions;

	public bool seaLeech;

	public int inputVeloCooldown;

	public bool tempControlled;

	public bool jungleLeech;

	public bool ChargingAttack
	{
		get
		{
			if (chargeCounter > 0)
			{
				return chargeCounter < 15;
			}
			return false;
		}
	}

	public bool Attacking => chargeCounter > 15;

	public bool InForbiddenRoom
	{
		get
		{
			if (room == null)
			{
				return false;
			}
			return room.abstractRoom.AttractionForCreature(base.abstractCreature.creatureTemplate.type) == AbstractRoom.CreatureRoomAttraction.Forbidden;
		}
	}

	public bool Controlled
	{
		get
		{
			if (!tempControlled)
			{
				return base.safariControlled;
			}
			return true;
		}
	}

	public Leech(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		seaLeech = base.Template.type == CreatureTemplate.Type.SeaLeech;
		jungleLeech = ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech;
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 4f, seaLeech ? 0.15f : 0.05f);
		bodyChunkConnections = new BodyChunkConnection[0];
		landWalkDir = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
		landWalkCycle = UnityEngine.Random.value;
		base.GoThroughFloors = true;
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.47f;
		collisionLayer = 0;
		base.waterFriction = 0.92f;
		base.buoyancy = 0.95f;
		afraidOfPositions = new List<Vector3>();
		ChangeCollisionLayer(0);
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new LeechGraphics(this);
		}
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		ResetSchool();
		ChangeCollisionLayer(0);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null || enteringShortCut.HasValue)
		{
			return;
		}
		if (tempControlled)
		{
			lastInputWithDiagonals = inputWithDiagonals;
			inputWithDiagonals = RWInput.PlayerInput(0);
			if (!lastInputWithDiagonals.HasValue)
			{
				lastInputWithDiagonals = inputWithDiagonals;
			}
		}
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
		}
		if (grabbedBy.Count > 0)
		{
			LoseAllGrasps();
		}
		if (base.grasps[0] != null)
		{
			Attached();
		}
		else if (base.Consious)
		{
			if (room.world.rainCycle.TimeUntilRain < 40 * (room.world.game.IsStorySession ? 60 : 15) && UnityEngine.Random.value < 0.0125f && !base.abstractCreature.nightCreature && !base.abstractCreature.ignoreCycle)
			{
				fleeFromRain = true;
			}
			if (base.mainBodyChunk.submersion > 0.2f)
			{
				Swim();
			}
			else
			{
				Crawl();
			}
		}
		if (base.Consious && huntPrey != null && base.grasps[0] == null && chargeCounter > 20)
		{
			BodyChunk[] array = huntPrey.bodyChunks;
			foreach (BodyChunk bodyChunk in array)
			{
				if (Custom.DistLess(base.mainBodyChunk.pos, bodyChunk.pos, bodyChunk.rad + base.mainBodyChunk.rad))
				{
					Grab(bodyChunk.owner, 0, bodyChunk.index, Grasp.Shareability.NonExclusive, 0f, overrideEquallyDominant: false, pacifying: false);
					room.PlaySound((huntPrey is Player) ? SoundID.Leech_Attatch_Player : SoundID.Leech_Attatch_NPC, base.mainBodyChunk);
					if (base.graphicsModule != null)
					{
						base.graphicsModule.BringSpritesToFront();
					}
					chargeCounter = 0;
					if (huntPrey.dead)
					{
						school.BitDeadPrey(huntPrey);
					}
					huntPrey.Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass, bodyChunk, null, DamageType.Bite, 0.02f, 3f);
					huntPrey = null;
					if (!Controlled)
					{
						landWalkDir = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
					}
					break;
				}
			}
		}
		if (!base.dead && (base.mainBodyChunk.submersion > 0f || jungleLeech))
		{
			airDrown = Mathf.Max(airDrown - 0.1f, 0f);
		}
		else
		{
			airDrown += 0.0011111111f;
			if (airDrown >= 1f && !base.dead)
			{
				Die();
			}
		}
		if (base.safariControlled)
		{
			for (int j = 0; j < school.leeches.Count; j++)
			{
				school.leeches[j].tempControlled = true;
			}
		}
		tempControlled = false;
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (base.grasps[0] == null && speed > 2f)
		{
			Stun(5 + (int)Mathf.Min((speed - 2f) * 4f, 20f));
		}
	}

	public override void Stun(int st)
	{
		if (st > 10)
		{
			LoseAllGrasps();
		}
		base.Stun(st);
	}

	public override void Die()
	{
		LoseAllGrasps();
		base.buoyancy = 0.83f;
		base.Die();
	}

	private void Swim()
	{
		if (huntPrey == null)
		{
			chargeCounter = 0;
		}
		if (chargeCounter == 0)
		{
			base.mainBodyChunk.vel += swimDirection * 0.3f;
		}
		else if (ChargingAttack)
		{
			base.mainBodyChunk.vel *= 0.9f;
		}
		else
		{
			base.mainBodyChunk.vel += swimDirection * 1.2f;
		}
		if (((Controlled && !inputWithDiagonals.HasValue) || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.x == 0 && inputWithDiagonals.Value.y == 0)) && huntPrey == null)
		{
			inputVeloCooldown++;
			base.mainBodyChunk.vel *= Mathf.Lerp(1f, 0.5f, Custom.LerpQuadEaseIn(0f, 1f, (float)inputVeloCooldown / (tempControlled ? 120f : 40f)));
		}
		else
		{
			inputVeloCooldown = 0;
		}
		swimDirection += Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.15f;
		if (base.mainBodyChunk.pos.y < 0f)
		{
			swimDirection.y += 0.1f;
		}
		if (seaLeech && !seaLeechTerritoryTile.HasValue && room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.mainBodyChunk.pos), base.Template))
		{
			seaLeechTerritoryTile = room.GetTilePosition(base.mainBodyChunk.pos);
		}
		huntPrey = null;
		if (fleeFromRain)
		{
			if (!offShootGoal.HasValue || room.GetTile(offShootGoal.Value).Terrain != Room.Tile.TerrainType.ShortcutEntrance)
			{
				offShootGoal = FleeFromRainGoal();
			}
		}
		else
		{
			float num = 300f;
			for (int num2 = school.prey.Count - 1; num2 >= 0; num2--)
			{
				if (!school.prey[num2].creature.leechedOut)
				{
					if (Custom.DistLess(TargetBodyChunk(school.prey[num2].creature).pos, base.mainBodyChunk.pos, base.Template.visualRadius) && room.ReallyTrulyRealizedInRoom(school.prey[num2].creature.abstractPhysicalObject) && room.VisualContact(TargetBodyChunk(school.prey[num2].creature).pos, base.mainBodyChunk.pos))
					{
						float num3 = Vector2.Distance(TargetBodyChunk(school.prey[num2].creature).pos, base.mainBodyChunk.pos) / Mathf.Lerp(school.prey[num2].creature.Template.bodySize, 1f, 0.7f);
						if (TargetBodyChunk(school.prey[num2].creature).submersion > 0f)
						{
							num3 /= 2f;
						}
						for (int i = 0; i < afraidOfPositions.Count; i++)
						{
							if (Custom.DistLess(new Vector2(afraidOfPositions[i].x, afraidOfPositions[i].y), TargetBodyChunk(school.prey[num2].creature).pos, 100f + afraidOfPositions[i].z / 2f))
							{
								num3 += 300f;
								break;
							}
						}
						if (school.prey[num2].creature.Template.type == CreatureTemplate.Type.Snail)
						{
							num3 *= 10f;
						}
						if (seaLeech && seaLeechTerritoryTile.HasValue)
						{
							num3 *= Mathf.Clamp((float)Mathf.Abs(seaLeechTerritoryTile.Value.x - room.GetTilePosition(TargetBodyChunk(school.prey[num2].creature).pos).x) - 3f, 1f, 100f);
						}
						if (num3 < num)
						{
							num = num3;
							huntPrey = school.prey[num2].creature;
						}
						school.prey[num2].forgetCounter = 0;
					}
					else
					{
						school.prey[num2].forgetCounter++;
						if (school.prey[num2].forgetCounter > school.leeches.Count * 400 || school.prey[num2].creature.room != room)
						{
							school.prey.RemoveAt(num2);
						}
					}
				}
			}
		}
		bool flag = false;
		if (afraidOfPositions.Count > 0)
		{
			float num4 = 0f;
			Vector2 vector = new Vector2(0f, 0f);
			for (int num5 = afraidOfPositions.Count - 1; num5 >= 0; num5--)
			{
				float num6 = Vector2.Distance(new Vector2(afraidOfPositions[num5].x, afraidOfPositions[num5].y), base.mainBodyChunk.pos);
				vector += Custom.DirVec(new Vector2(afraidOfPositions[num5].x, afraidOfPositions[num5].y), base.mainBodyChunk.pos) * afraidOfPositions[num5].z / num6;
				num4 += afraidOfPositions[num5].z / num6;
				afraidOfPositions[num5] = new Vector3(afraidOfPositions[num5].x, afraidOfPositions[num5].y, afraidOfPositions[num5].z - 1f);
				if (afraidOfPositions[num5].z <= 0f)
				{
					afraidOfPositions.RemoveAt(num5);
				}
			}
			swimDirection += (vector.normalized + new Vector2(0f, -1f)) * 0.05f * Mathf.InverseLerp(0f, 0.8f, num4);
			flag = num4 > 0.15f;
		}
		if (!flag)
		{
			Leech leech = null;
			if (ModManager.MMF && InForbiddenRoom)
			{
				huntPrey = null;
			}
			if (Controlled)
			{
				huntPrey = null;
				if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp)
				{
					float num7 = 9999f;
					for (int j = 0; j < base.abstractCreature.Room.creatures.Count; j++)
					{
						if (base.abstractCreature.Room.creatures[j].realizedCreature == null)
						{
							continue;
						}
						Creature realizedCreature = base.abstractCreature.Room.creatures[j].realizedCreature;
						for (int k = 0; k < realizedCreature.bodyChunks.Length; k++)
						{
							float num8 = Custom.Dist(base.mainBodyChunk.pos, realizedCreature.bodyChunks[k].pos);
							if (realizedCreature.abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Leech && realizedCreature.abstractCreature.creatureTemplate.type != CreatureTemplate.Type.SeaLeech && num8 < 150f + realizedCreature.bodyChunks[k].rad && num8 < num7)
							{
								num7 = num8;
								huntPrey = realizedCreature;
								if (chargeCounter < 1)
								{
									chargeCounter = 1;
								}
							}
						}
					}
				}
			}
			if (seaLeech || huntPrey != null)
			{
				bool flag2 = false;
				for (int num9 = school.leeches.Count - 1; num9 >= 0; num9--)
				{
					if (flag2 && school.leeches[num9].huntPrey == huntPrey)
					{
						leech = school.leeches[num9];
						break;
					}
					if (school.leeches[num9] == this)
					{
						flag2 = true;
					}
				}
			}
			if (huntPrey != null)
			{
				if (chargeCounter == 0)
				{
					if (!seaLeech && leech != null)
					{
						swimDirection += Custom.DirVec(base.mainBodyChunk.pos, Vector2.Lerp(leech.mainBodyChunk.pos, TargetBodyChunk(huntPrey).pos, 0.1f)) * (seaLeech ? 1.5f : 0.5f);
					}
					else
					{
						swimDirection += Custom.DirVec(base.mainBodyChunk.pos, TargetBodyChunk(huntPrey).pos) * (seaLeech ? 1.5f : 0.5f);
					}
					for (int l = 0; l < school.leeches.Count; l++)
					{
						if (Custom.DistLess(base.mainBodyChunk.pos, school.leeches[l].mainBodyChunk.pos, 25f) && this != school.leeches[l])
						{
							swimDirection += Custom.DirVec(school.leeches[l].mainBodyChunk.pos, base.mainBodyChunk.pos) * 0.1f;
						}
					}
					if (huntPrey.enteringShortCut.HasValue)
					{
						offShootGoal = room.MiddleOfTile(huntPrey.enteringShortCut.Value);
					}
					if (Custom.DistLess(base.mainBodyChunk.pos, TargetBodyChunk(huntPrey).pos, TargetBodyChunk(huntPrey).rad + 50f) && !Custom.DistLess(base.mainBodyChunk.pos, TargetBodyChunk(huntPrey).pos, TargetBodyChunk(huntPrey).rad + 10f) && Vector2.Dot(swimDirection.normalized, (TargetBodyChunk(huntPrey).pos - base.mainBodyChunk.pos).normalized) > 0.6f)
					{
						chargeCounter = 1;
					}
				}
				else
				{
					swimDirection += Custom.DirVec(base.mainBodyChunk.pos, TargetBodyChunk(huntPrey).pos) * (ChargingAttack ? 0.25f : 0.02f);
					chargeCounter++;
					if (chargeCounter > 30)
					{
						chargeCounter = 0;
					}
				}
			}
			else if (offShootGoal.HasValue)
			{
				if (leech != null)
				{
					swimDirection += Custom.DirVec(base.mainBodyChunk.pos, leech.mainBodyChunk.pos + Custom.DirVec(leech.mainBodyChunk.pos, leech.mainBodyChunk.lastPos) * 5f) * 1.5f;
				}
				else
				{
					swimDirection += Custom.DirVec(base.mainBodyChunk.pos, offShootGoal.Value) * (room.aimap.getAItile(offShootGoal.Value).narrowSpace ? 0.8f : 0.2f) * (seaLeech ? 4f : 1f);
					if (room.GetTile(offShootGoal.Value).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					{
						if (shortcutDelay == 0 && room.GetTilePosition(base.mainBodyChunk.pos) == room.GetTilePosition(offShootGoal.Value))
						{
							enteringShortCut = room.GetTilePosition(offShootGoal.Value);
							offShootGoal = null;
						}
						if (UnityEngine.Random.value < 0.025f && (base.mainBodyChunk.ContactPoint.x != 0 || base.mainBodyChunk.ContactPoint.y != 0 || base.mainBodyChunk.submersion < 1f))
						{
							offShootGoal = null;
						}
					}
					else if (Custom.DistLess(base.mainBodyChunk.pos, offShootGoal.Value, 40f) || !room.VisualContact(base.mainBodyChunk.pos, offShootGoal.Value))
					{
						offShootGoal = null;
					}
				}
			}
			else
			{
				int terrainProximity = room.aimap.getTerrainProximity(base.mainBodyChunk.pos);
				if (terrainProximity < 4 && room.aimap.getTerrainProximity(base.mainBodyChunk.pos + swimDirection * 30f) < terrainProximity)
				{
					for (int m = 0; m < 8; m++)
					{
						if (room.aimap.getTerrainProximity(base.mainBodyChunk.pos + IntVector2.ToVector2(Custom.eightDirections[m]) * 20f) < terrainProximity)
						{
							swimDirection -= IntVector2.ToVector2(Custom.eightDirections[m]) * 0.3f;
						}
					}
				}
				if ((!ModManager.MSC) ? (base.mainBodyChunk.pos.y > room.FloatWaterLevel(base.mainBodyChunk.pos.x) - 50f) : (!room.PointSubmerged(base.mainBodyChunk.pos, 50f)))
				{
					swimDirection.y -= 0.2f;
				}
				bool flag3 = true;
				for (int n = 0; n < school.leeches.Count; n++)
				{
					if (Custom.DistLess(base.mainBodyChunk.pos, school.leeches[n].mainBodyChunk.pos, 50f) && this != school.leeches[n])
					{
						flag3 = false;
						swimDirection += school.leeches[n].swimDirection * 0.05f;
						if (Custom.DistLess(base.mainBodyChunk.pos, school.leeches[n].mainBodyChunk.pos, 25f))
						{
							swimDirection += Custom.DirVec(school.leeches[n].mainBodyChunk.pos, base.mainBodyChunk.pos) * 0.2f;
						}
					}
				}
				if (flag3 && school.leeches.Count > 1)
				{
					lonerCounter++;
				}
				else if (lonerCounter > 0)
				{
					lonerCounter--;
				}
				if (lonerCounter > 100)
				{
					ResetSchool();
				}
				SwitchDestination();
			}
		}
		swimDirection = swimDirection.normalized;
		if (room.abstractRoom.creatures.Count > 0)
		{
			ConsiderOtherCreature(room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)].realizedCreature);
		}
		if (school.leeches.Count > 0)
		{
			Leech leech2 = school.leeches[UnityEngine.Random.Range(0, school.leeches.Count)];
			if (leech2.dead || !room.VisualContact(base.mainBodyChunk.pos, leech2.mainBodyChunk.pos))
			{
				leech2.ResetSchool();
			}
		}
		if (!Controlled)
		{
			return;
		}
		if (inputWithDiagonals.HasValue && (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0) && huntPrey == null)
		{
			swimDirection = new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y);
			swimDirection += Custom.DegToVec(UnityEngine.Random.value * 360f) * (tempControlled ? 0.4f : 0.15f);
			if (tempControlled)
			{
				swimDirection += new Vector2(UnityEngine.Random.Range(-0.25f, 0.25f), UnityEngine.Random.Range(-0.25f, 0.25f));
			}
		}
		if (huntPrey != null)
		{
			if (chargeCounter == 1)
			{
				swimDirection = Custom.DirVec(base.mainBodyChunk.pos, TargetBodyChunk(huntPrey).pos) * ((!ChargingAttack) ? 0.02f : 0.25f);
			}
			else
			{
				swimDirection += Custom.DirVec(base.mainBodyChunk.pos, TargetBodyChunk(huntPrey).pos) * ((!ChargingAttack) ? 0.02f : 0.25f);
			}
		}
		if (base.mainBodyChunk.pos.y < 0f)
		{
			swimDirection.y += 0.1f;
		}
		if (!room.PointSubmerged(base.mainBodyChunk.pos, 50f))
		{
			swimDirection.y -= 0.2f;
		}
	}

	private void SwitchDestination()
	{
		IntVector2 pos;
		if (seaLeech && seaLeechTerritoryTile.HasValue)
		{
			pos = new IntVector2(seaLeechTerritoryTile.Value.x, UnityEngine.Random.Range(0, room.TileHeight));
			if ((UnityEngine.Random.value < 0.1f || !offShootGoal.HasValue || room.GetTilePosition(offShootGoal.Value).x != seaLeechTerritoryTile.Value.x) && room.GetTile(pos).DeepWater && room.VisualContact(base.mainBodyChunk.pos, room.MiddleOfTile(pos)))
			{
				offShootGoal = room.MiddleOfTile(pos) + new Vector2(Mathf.Lerp(-9f, 9f, UnityEngine.Random.value), Mathf.Lerp(-9f, 9f, UnityEngine.Random.value));
				seaLeechFarFromTerritoryCounter = 0;
				return;
			}
			if (!room.VisualContact(base.mainBodyChunk.pos, room.MiddleOfTile(pos)))
			{
				seaLeechFarFromTerritoryCounter++;
				if (seaLeechFarFromTerritoryCounter > 5)
				{
					seaLeechFarFromTerritoryCounter = 0;
					seaLeechTerritoryTile = null;
				}
			}
		}
		pos = room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(UnityEngine.Random.Range(-10, 11), UnityEngine.Random.Range(-10, 11));
		if (jungleLeech)
		{
			base.GoThroughFloors = false;
			if (UnityEngine.Random.value < 0.2f && !room.GetTile(pos).Solid)
			{
				offShootGoal = room.MiddleOfTile(pos) + new Vector2(Mathf.Lerp(-39f, 39f, UnityEngine.Random.value), Mathf.Lerp(-8f, 8f, UnityEngine.Random.value));
			}
			else
			{
				pos = room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(UnityEngine.Random.Range(-10, 11), 0);
				if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.Floor && !room.GetTile(pos).Solid)
				{
					offShootGoal = room.MiddleOfTile(pos) + new Vector2(Mathf.Lerp(-9f, 9f, UnityEngine.Random.value), Mathf.Lerp(-2f, 2f, UnityEngine.Random.value));
				}
			}
		}
		else if (UnityEngine.Random.value < 0.1f && !room.GetTile(pos).DeepWater && !room.VisualContact(base.mainBodyChunk.pos, room.MiddleOfTile(pos)))
		{
			offShootGoal = room.MiddleOfTile(pos) + new Vector2(Mathf.Lerp(-9f, 9f, UnityEngine.Random.value), Mathf.Lerp(-9f, 9f, UnityEngine.Random.value));
		}
		else if (shortcutDelay == 0 && UnityEngine.Random.value < 0.05f)
		{
			IntVector2 intVector = Custom.fourDirections[UnityEngine.Random.Range(0, 4)];
			for (int i = 1; i < 6 && room.GetTile(base.abstractCreature.pos.Tile + intVector * i).Terrain != Room.Tile.TerrainType.Solid; i++)
			{
				if (room.GetTile(base.abstractCreature.pos.Tile + intVector * i).Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(base.abstractCreature.pos.Tile + intVector * i).shortCutType != ShortcutData.Type.DeadEnd && base.abstractCreature.pos.Tile + intVector * i != avoidThisShortCutEntrance)
				{
					offShootGoal = room.MiddleOfTile(base.abstractCreature.pos.Tile + intVector * i);
				}
			}
		}
		if (UnityEngine.Random.value < 1f / 180f)
		{
			avoidThisShortCutEntrance.x = -1;
		}
	}

	public void HeardSnailClick(Vector2 pos)
	{
		if (base.mainBodyChunk.submersion < 0.5f)
		{
			return;
		}
		if (base.Consious)
		{
			for (int num = afraidOfPositions.Count - 1; num >= 0; num--)
			{
				if (Custom.DistLess(pos, new Vector2(afraidOfPositions[num].x, afraidOfPositions[num].y), 20f))
				{
					afraidOfPositions.RemoveAt(num);
				}
			}
			afraidOfPositions.Add(new Vector3(pos.x, pos.y, 440f));
		}
		base.mainBodyChunk.vel = Vector2.Lerp(base.mainBodyChunk.vel, Custom.DirVec(pos, base.mainBodyChunk.pos) * 6f, base.Consious ? 0.5f : 0.2f);
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as LeechGraphics).Vibrate();
		}
	}

	public void ResetSchool()
	{
		if (school != null)
		{
			school.RemoveLeech(this);
		}
		school = new LeechSchool();
		school.AddLeech(this);
	}

	private void Crawl()
	{
		if (Controlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.x != 0)
		{
			landWalkDir = (int)Mathf.Sign(inputWithDiagonals.Value.x);
		}
		if (jungleLeech && offShootGoal.HasValue)
		{
			landWalkDir = (int)Mathf.Sign(Custom.DirVec(base.firstChunk.pos, offShootGoal.Value).normalized.x);
		}
		if (shortcutDelay == 0 && room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(room.GetTilePosition(base.mainBodyChunk.pos)).LeadingSomewhere)
		{
			if (room.ShorcutEntranceHoleDirection(room.GetTilePosition(base.mainBodyChunk.pos)).y == -1)
			{
				enteringShortCut = room.GetTilePosition(base.mainBodyChunk.pos);
			}
			else if (room.ShorcutEntranceHoleDirection(room.GetTilePosition(base.mainBodyChunk.pos)).x == -landWalkDir)
			{
				enteringShortCut = room.GetTilePosition(base.mainBodyChunk.pos);
			}
		}
		if (Controlled && (!inputWithDiagonals.HasValue || inputWithDiagonals.Value.x == 0))
		{
			return;
		}
		if (jungleLeech)
		{
			if (UnityEngine.Random.value < 0.3f)
			{
				int num = UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count - 1);
				for (int i = 0; i < 5; i++)
				{
					int index = (i + num) % room.abstractRoom.creatures.Count;
					if (room.abstractRoom.creatures[index] == null || !room.abstractRoom.creatures[index].state.alive || !(room.abstractRoom.creatures[index].creatureTemplate.TopAncestor().type != CreatureTemplate.Type.Leech) || room.abstractRoom.creatures[index].realizedCreature == null || !(base.abstractCreature.creatureTemplate.CreatureRelationship(room.abstractRoom.creatures[index].creatureTemplate).type == CreatureTemplate.Relationship.Type.Eats))
					{
						continue;
					}
					BodyChunk[] array = room.abstractRoom.creatures[index].realizedCreature.bodyChunks;
					foreach (BodyChunk bodyChunk in array)
					{
						if (Custom.DistLess(base.mainBodyChunk.pos, bodyChunk.pos, bodyChunk.rad + base.mainBodyChunk.rad))
						{
							Grab(bodyChunk.owner, 0, bodyChunk.index, Grasp.Shareability.NonExclusive, 0f, overrideEquallyDominant: false, pacifying: false);
							room.PlaySound((!(bodyChunk.owner is Player)) ? SoundID.Leech_Attatch_NPC : SoundID.Leech_Attatch_Player, base.mainBodyChunk);
							if (base.graphicsModule != null)
							{
								base.graphicsModule.BringSpritesToFront();
							}
							(bodyChunk.owner as Creature).Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass, bodyChunk, null, DamageType.Bite, 0.02f, 3f);
							i = 1000;
							break;
						}
					}
				}
			}
			if (offShootGoal.HasValue)
			{
				if (base.firstChunk.pos.y > offShootGoal.Value.y + 10f && UnityEngine.Random.value < 0.001f)
				{
					base.GoThroughFloors = true;
				}
				if (Mathf.Abs(base.firstChunk.pos.x - offShootGoal.Value.x) < 10f)
				{
					offShootGoal = null;
				}
				if (UnityEngine.Random.value < 0.008f && room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos + new Vector2(0f, -18f))).Solid)
				{
					base.GoThroughFloors = false;
					base.firstChunk.vel += new Vector2(0f, 13f);
				}
			}
			else
			{
				SwitchDestination();
			}
		}
		if (room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(landWalkDir, 0)).Terrain != Room.Tile.TerrainType.Solid && room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(0, -2)).Terrain != 0 && room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(0, -1)).Terrain == Room.Tile.TerrainType.Air && room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(landWalkDir, -1)).Terrain == Room.Tile.TerrainType.Solid)
		{
			base.mainBodyChunk.vel.x += (float)landWalkDir * 2f;
		}
		if (base.mainBodyChunk.ContactPoint.y != -1)
		{
			return;
		}
		swimDirection = new Vector2(landWalkDir, 0f);
		landWalkCycle += 1f / Mathf.Lerp(20f, 60f, Mathf.Pow(airDrown, 3f));
		if (landWalkCycle > 1f)
		{
			room.PlaySound(SoundID.Leech_Crawl_On_Ground, base.mainBodyChunk);
			landWalkCycle -= 1f;
		}
		base.mainBodyChunk.vel.x += (1f + 0.9f * Mathf.Sin(landWalkCycle * (float)Math.PI * 2f)) * (float)landWalkDir * 0.2f * (1f - Mathf.Pow(airDrown, 3f));
		base.mainBodyChunk.vel.x *= Mathf.Lerp(1f, 0.8f, landWalkCycle);
		if (base.mainBodyChunk.ContactPoint.x != landWalkDir)
		{
			return;
		}
		if (room.GetTile(base.mainBodyChunk.pos).Terrain != Room.Tile.TerrainType.Slope && room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(landWalkDir, 1)).Terrain != Room.Tile.TerrainType.Solid)
		{
			base.mainBodyChunk.vel.y = 8f;
		}
		else if (!Controlled)
		{
			landWalkDir *= -1;
			if (jungleLeech)
			{
				SwitchDestination();
			}
		}
	}

	private void Attached()
	{
		if (ModManager.MMF && InForbiddenRoom)
		{
			Stun(80);
			LoseAllGrasps();
			return;
		}
		if (Controlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
		{
			room.PlaySound(SoundID.Leech_Detatch_NPC, base.mainBodyChunk);
			LoseAllGrasps();
			return;
		}
		BodyChunk bodyChunk = base.grasps[0].grabbed.bodyChunks[base.grasps[0].chunkGrabbed];
		Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, bodyChunk.pos);
		float num = Vector2.Distance(base.mainBodyChunk.pos, bodyChunk.pos);
		float num2 = base.mainBodyChunk.rad + bodyChunk.rad;
		float num3 = base.mainBodyChunk.mass / (base.mainBodyChunk.mass + bodyChunk.mass);
		base.mainBodyChunk.vel += vector * (num - num2) * (1f - num3);
		base.mainBodyChunk.pos += vector * (num - num2) * (1f - num3);
		bodyChunk.vel -= vector * (num - num2) * num3;
		bodyChunk.pos -= vector * (num - num2) * num3;
		swimDirection = Custom.DirVec(base.mainBodyChunk.pos, bodyChunk.pos);
		for (int i = 0; i < base.grasps[0].grabbed.bodyChunks.Length; i++)
		{
			PushOutOfChunk(base.grasps[0].grabbed.bodyChunks[i]);
		}
		for (int j = 0; j < base.grasps[0].grabbed.grabbedBy.Count; j++)
		{
			if (base.grasps[0].grabbed.grabbedBy[j].grabber != this)
			{
				for (int k = 0; k < base.grasps[0].grabbed.grabbedBy[j].grabber.bodyChunks.Length; k++)
				{
					PushOutOfChunk(base.grasps[0].grabbed.grabbedBy[j].grabber.bodyChunks[k]);
				}
			}
		}
		float num4 = (seaLeech ? 0.07f : 0.025f) * base.mainBodyChunk.submersion / bodyChunk.mass;
		if (ModManager.MMF && room.GetTile(bodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
		{
			num4 *= 0.1f;
		}
		bodyChunk.vel.y -= num4;
		if (jungleLeech)
		{
			if (bodyChunk.owner is Player && UnityEngine.Random.value < (((bodyChunk.owner as Player).stun > 40) ? 0.009f : 0.002f))
			{
				if ((bodyChunk.owner as Player).playerState.foodInStomach > 0)
				{
					if (UnityEngine.Random.value < 0.1f)
					{
						(bodyChunk.owner as Player).stun += 5;
						(bodyChunk.owner as Player).SubtractFood(1);
					}
				}
				else
				{
					(bodyChunk.owner as Player).aerobicLevel = 1f;
					(bodyChunk.owner as Player).exhausted = true;
					(bodyChunk.owner as Player).stun += 5;
					if ((bodyChunk.owner as Player).stun > 60)
					{
						(bodyChunk.owner as Player).Die();
					}
				}
			}
			if (!(bodyChunk.owner is Player) && UnityEngine.Random.value < (((bodyChunk.owner as Creature).stun > 10) ? 0.008f : 0.004f))
			{
				if ((float)(bodyChunk.owner as Creature).stun > Mathf.Lerp(60f, 300f, Mathf.InverseLerp(0f, 20f, (bodyChunk.owner as Creature).TotalMass)))
				{
					(bodyChunk.owner as Creature).Die();
				}
				(bodyChunk.owner as Creature).stun += (int)Mathf.Lerp(15f, 3f, Mathf.InverseLerp(0f, 20f, (bodyChunk.owner as Creature).TotalMass));
			}
			if ((bodyChunk.owner as Creature).dead)
			{
				room.PlaySound((!(bodyChunk.owner is Player)) ? SoundID.Leech_Detatch_NPC : SoundID.Leech_Detatch_Player, base.mainBodyChunk);
				LoseAllGrasps();
				return;
			}
		}
		else if (!jungleLeech && ((UnityEngine.Random.value < 0.0025f && base.mainBodyChunk.submersion < 0.1f) || (airDrown > 0.8f && UnityEngine.Random.value < 0.0125f) || ((bodyChunk.owner as Creature).dead && UnityEngine.Random.value < 0.01f) || UnityEngine.Random.value < 0.00083333335f || grabbedBy.Count > 0))
		{
			room.PlaySound((bodyChunk.owner is Player) ? SoundID.Leech_Detatch_Player : SoundID.Leech_Detatch_NPC, base.mainBodyChunk);
			LoseAllGrasps();
		}
		if (ModManager.MMF && MMF.cfgGraspWiggling.Value && bodyChunk.owner is Player && UnityEngine.Random.value < Mathf.Lerp(0f, jungleLeech ? 0.8f : 0.15f, (bodyChunk.owner as Player).GraspWiggle))
		{
			room.PlaySound(SoundID.Leech_Detatch_Player, base.mainBodyChunk);
			Stun(100);
			LoseAllGrasps();
		}
	}

	private void PushOutOfChunk(BodyChunk chunk)
	{
		if (Custom.DistLess(chunk.pos, base.mainBodyChunk.pos, chunk.rad + base.mainBodyChunk.rad))
		{
			Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, chunk.pos);
			float num = Vector2.Distance(base.mainBodyChunk.pos, chunk.pos);
			float num2 = base.mainBodyChunk.mass / (base.mainBodyChunk.mass + chunk.mass);
			base.mainBodyChunk.vel += vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * (1f - num2);
			base.mainBodyChunk.pos += vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * (1f - num2);
			chunk.vel -= vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * num2;
			chunk.pos -= vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * num2;
		}
	}

	private void ConsiderOtherCreature(Creature crit)
	{
		if (crit == null || !room.ReallyTrulyRealizedInRoom(crit.abstractPhysicalObject) || crit.bodyChunks.Length == 0)
		{
			return;
		}
		BodyChunk bodyChunk = crit.bodyChunks[UnityEngine.Random.Range(0, crit.bodyChunks.Length)];
		if (!Custom.DistLess(base.mainBodyChunk.pos, bodyChunk.pos, 150f) || !room.VisualContact(base.mainBodyChunk.pos, bodyChunk.pos))
		{
			return;
		}
		if (crit is Leech)
		{
			if (school.leeches.Count < (crit as Leech).school.leeches.Count || !Custom.DistLess(base.mainBodyChunk.pos, bodyChunk.pos, 50f) || school.leeches.Contains(crit as Leech) || (crit as Leech).seaLeech != seaLeech)
			{
				return;
			}
			if (seaLeech && (crit as Leech).seaLeech)
			{
				if (seaLeechTerritoryTile.HasValue && (crit as Leech).seaLeechTerritoryTile.HasValue && Math.Abs((crit as Leech).seaLeechTerritoryTile.Value.x - seaLeechTerritoryTile.Value.x) > 5)
				{
					return;
				}
				if (!seaLeechTerritoryTile.HasValue && (crit as Leech).seaLeechTerritoryTile.HasValue)
				{
					seaLeechTerritoryTile = (crit as Leech).seaLeechTerritoryTile;
				}
				else if (seaLeechTerritoryTile.HasValue && !(crit as Leech).seaLeechTerritoryTile.HasValue)
				{
					(crit as Leech).seaLeechTerritoryTile = seaLeechTerritoryTile;
				}
			}
			school.Merge((crit as Leech).school);
			(crit as Leech).school = school;
		}
		else if (base.Template.CreatureRelationship(crit.Template).type == CreatureTemplate.Relationship.Type.Eats && Custom.DistLess(base.mainBodyChunk.pos, bodyChunk.pos, 150f * base.Template.CreatureRelationship(crit.Template).intensity))
		{
			school.AddPrey(crit);
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		shortcutDelay = 20;
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		base.mainBodyChunk.pos = newRoom.MiddleOfTile(pos) - vector * 5f;
		base.mainBodyChunk.lastPos = base.mainBodyChunk.pos;
		base.mainBodyChunk.vel = vector * 5f;
		ResetSchool();
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
		if (jungleLeech)
		{
			SwitchDestination();
		}
		avoidThisShortCutEntrance = pos;
	}

	public BodyChunk TargetBodyChunk(Creature creature)
	{
		return creature.bodyChunks[room.game.SeededRandomRange(base.abstractCreature.ID.RandomSeed, 0, creature.bodyChunks.Length)];
	}

	public Vector2? FleeFromRainGoal()
	{
		int num = int.MaxValue;
		Vector2? result = null;
		for (int i = room.abstractRoom.connections.Length; i < room.abstractRoom.nodes.Length; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				int num2 = room.aimap.ExitDistanceForCreature(room.GetTilePosition(base.mainBodyChunk.pos) + Custom.fourDirections[j], i, base.Template);
				if (num2 > 0 && num2 < num)
				{
					result = room.MiddleOfTile(room.GetTilePosition(base.mainBodyChunk.pos) + Custom.fourDirections[j]);
					num = num2;
				}
			}
		}
		return result;
	}
}
