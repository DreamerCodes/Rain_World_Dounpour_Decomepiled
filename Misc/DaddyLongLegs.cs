using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class DaddyLongLegs : Creature, PhysicalObject.IHaveAppendages
{
	private class EatObject
	{
		public BodyChunk chunk;

		public float distance;

		public float progression;

		public EatObject(BodyChunk chunk, float distance)
		{
			this.chunk = chunk;
			this.distance = distance;
			progression = 0f;
		}
	}

	public class DaddyState : HealthState
	{
		public float[] tentacleHealth;

		public DaddyState(AbstractCreature creature)
			: base(creature)
		{
			tentacleHealth = new float[13];
			for (int i = 0; i < tentacleHealth.Length; i++)
			{
				tentacleHealth[i] = 1f;
			}
		}
	}

	public DaddyAI AI;

	public bool hangingInTentacle;

	public DaddyTentacle[] tentacles;

	public Vector2 moveDirection = new Vector2(0f, -1f);

	public bool tentaclesHoldOn;

	public bool moving;

	public List<IntVector2> pastPositions;

	public int stuckCounter;

	public bool squeeze;

	public float squeezeFac;

	public int eyesClosed;

	public Color effectColor;

	public Color eyeColor;

	public bool colorClass;

	public int digestingCounter;

	public int graphicsSeed;

	public int notFollowingPathToCurrentGoalCounter;

	private float unconditionalSupport;

	public PlacedObject stuckPos;

	private List<EatObject> eatObjects;

	public bool isHD;

	private World world;

	public bool SizeClass
	{
		get
		{
			if (ModManager.MSC && (base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs || world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Spear || world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer))
			{
				return true;
			}
			return base.Template.type == CreatureTemplate.Type.DaddyLongLegs;
		}
	}

	public Vector2 MiddleOfBody
	{
		get
		{
			Vector2 vector = base.mainBodyChunk.pos * base.mainBodyChunk.mass;
			for (int i = 1; i < base.bodyChunks.Length; i++)
			{
				vector += base.bodyChunks[i].pos * base.bodyChunks[i].mass;
			}
			return vector / base.TotalMass;
		}
	}

	public float MostDigestedEatObject
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < eatObjects.Count; i++)
			{
				num = Mathf.Max(num, eatObjects[i].progression);
			}
			return num;
		}
	}

	public bool HDmode
	{
		get
		{
			if (ModManager.MSC)
			{
				return isHD;
			}
			return false;
		}
	}

	public DaddyLongLegs(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		this.world = world;
		colorClass = SizeClass && (world.region == null || (world.region.name != "GW" && (!ModManager.MSC || world.region.name != "CL")) || (ModManager.MSC && world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Spear) || world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer);
		if (ModManager.MSC && world.region != null && world.region.name == "RM")
		{
			colorClass = true;
		}
		if (ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
		{
			colorClass = true;
			abstractCreature.superSizeMe = true;
		}
		if (ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
		{
			isHD = true;
		}
		if (HDmode)
		{
			effectColor = new Color(0.57255f, 0.11373f, 0.22745f);
			eyeColor = effectColor;
		}
		else if (ModManager.MSC && abstractCreature.superSizeMe)
		{
			effectColor = new Color(0.3f, 0f, 1f);
			eyeColor = effectColor;
		}
		else if (SizeClass)
		{
			if (world.region != null)
			{
				effectColor = world.region.regionParams.corruptionEffectColor;
				eyeColor = world.region.regionParams.corruptionEyeColor;
			}
			else
			{
				effectColor = new Color(0f, 0f, 1f);
				eyeColor = effectColor;
			}
		}
		else if (colorClass)
		{
			effectColor = new Color(0f, 0f, 1f);
			eyeColor = effectColor;
		}
		else
		{
			effectColor = new Color(0.7f, 0.7f, 0.4f);
			eyeColor = new Color(0.5f, 0.3f, 0f);
		}
		if (base.abstractCreature.IsVoided())
		{
			effectColor = RainWorld.SaturatedGold;
			eyeColor = effectColor;
		}
		eatObjects = new List<EatObject>();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractCreature.ID.RandomSeed);
		graphicsSeed = UnityEngine.Random.Range(0, int.MaxValue);
		float num = (SizeClass ? 12f : 8f);
		int num2 = 8;
		int num3 = 7;
		if (ModManager.MSC && abstractCreature.superSizeMe)
		{
			num2 = 16;
			num3 = 11;
			num = 18f;
		}
		else if (HDmode)
		{
			num2 = 6;
			num3 = 6;
			num = 4f;
		}
		base.bodyChunks = new BodyChunk[UnityEngine.Random.Range(4, SizeClass ? num2 : num3)];
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			float num4 = (float)i / (float)(base.bodyChunks.Length - 1);
			float num5 = Mathf.Lerp(num * 0.2f, num * Mathf.Lerp(0.3f, 1f, num4), Mathf.Pow(UnityEngine.Random.value, 1f - num4));
			num -= num5;
			base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(0f, 0f), num5 * 3.5f + 3.5f, (HDmode && i < 2) ? 20f : num5);
			list.Add(Custom.RNV() * base.bodyChunks[i].rad);
		}
		for (int j = 0; j < 5; j++)
		{
			for (int k = 0; k < base.bodyChunks.Length; k++)
			{
				for (int l = 0; l < base.bodyChunks.Length; l++)
				{
					if (k != l && Vector2.Distance(list[k], list[l]) < (base.bodyChunks[k].rad + base.bodyChunks[l].rad) * 0.85f)
					{
						list[l] -= Custom.DirVec(list[l], list[k]) * ((base.bodyChunks[k].rad + base.bodyChunks[l].rad) * 0.85f - Vector2.Distance(list[k], list[l]));
					}
				}
			}
			for (int m = 0; m < base.bodyChunks.Length; m++)
			{
				list[m] *= 0.9f;
			}
		}
		bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length * (base.bodyChunks.Length - 1) / 2];
		int num6 = 0;
		for (int n = 0; n < base.bodyChunks.Length; n++)
		{
			for (int num7 = n + 1; num7 < base.bodyChunks.Length; num7++)
			{
				bodyChunkConnections[num6] = new BodyChunkConnection(base.bodyChunks[n], base.bodyChunks[num7], Vector2.Distance(list[n], list[num7]), BodyChunkConnection.Type.Normal, 1f, -1f);
				num6++;
			}
		}
		int num8 = 13;
		int num9 = 10;
		float num10 = 400f;
		if (ModManager.MSC && abstractCreature.superSizeMe)
		{
			num8 = 13;
			num9 = 12;
			num10 = 1000f;
		}
		float num11;
		if (HDmode)
		{
			tentacles = new DaddyTentacle[4];
			num11 = Mathf.Lerp(600f, (float)tentacles.Length * 115f, 0.5f);
		}
		else
		{
			tentacles = new DaddyTentacle[UnityEngine.Random.Range(5, SizeClass ? num8 : num9)];
			num11 = Mathf.Lerp(SizeClass ? 3000f : 1600f, (float)tentacles.Length * (SizeClass ? num10 : 300f), 0.5f);
		}
		List<float> list2 = new List<float>();
		for (int num12 = 0; num12 < tentacles.Length; num12++)
		{
			list2.Add(num11 / (float)tentacles.Length);
		}
		for (int num13 = 0; num13 < 5 * tentacles.Length; num13++)
		{
			int index = UnityEngine.Random.Range(0, tentacles.Length);
			float num14 = list2[index] * UnityEngine.Random.value * 0.3f;
			if (list2[index] - num14 > 100f)
			{
				list2[UnityEngine.Random.Range(0, tentacles.Length)] += num14;
				list2[index] -= num14;
			}
		}
		appendages = new List<Appendage>();
		for (int num15 = 0; num15 < tentacles.Length; num15++)
		{
			tentacles[num15] = new DaddyTentacle(this, base.bodyChunks[num15 % base.bodyChunks.Length], list2[num15], num15, Custom.DegToVec(Mathf.Lerp(0f, 360f, (float)num15 / (float)tentacles.Length)));
			appendages.Add(new Appendage(this, num15, tentacles[num15].tChunks.Length + 1));
		}
		UnityEngine.Random.state = state;
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		if (ModManager.MSC && abstractCreature.superSizeMe)
		{
			base.gravity = 0.75f;
			base.waterFriction = 0.99f;
			base.buoyancy = 0.75f;
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new DaddyGraphics(this);
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i].NewRoom(newRoom);
		}
		pastPositions = new List<IntVector2>();
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		Vector2 middleOfBody = MiddleOfBody;
		for (int i = 0; i < tentacles.Length; i++)
		{
			IntVector2 tilePosition = room.GetTilePosition(tentacles[i].connectedChunk.pos);
			Vector2 vector = Custom.DirVec(middleOfBody, tentacles[i].connectedChunk.pos);
			IntVector2 tilePosition2 = room.GetTilePosition(tentacles[i].connectedChunk.pos + vector * tentacles[i].idealLength);
			List<IntVector2> path = new List<IntVector2>();
			room.RayTraceTilesList(tilePosition.x, tilePosition.y, tilePosition2.x, tilePosition2.y, ref path);
			for (int j = 1; j < path.Count; j++)
			{
				if (room.GetTile(path[j]).Solid)
				{
					path.RemoveRange(j, path.Count - j);
					break;
				}
			}
			tentacles[i].segments = path;
			for (int k = 0; k < tentacles[i].tChunks.Length; k++)
			{
				tentacles[i].tChunks[k].Reset();
			}
			tentacles[i].MoveGrabDest(room.MiddleOfTile(path[path.Count - 1]), ref path);
		}
		unconditionalSupport = 1f;
	}

	public override void Die()
	{
		if (!base.dead && HDmode && base.abstractCreature.world.game.IsStorySession && killTag != null && killTag.realizedCreature != null && killTag.realizedCreature is Player)
		{
			base.abstractCreature.world.game.manager.rainWorld.progression.miscProgressionData.redsFlower = null;
		}
		base.Die();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		unconditionalSupport = Mathf.Max(0f, unconditionalSupport - 0.025f);
		if (squeeze || enteringShortCut.HasValue)
		{
			squeezeFac = Mathf.Min(1f, squeezeFac + 0.02f);
		}
		else
		{
			squeezeFac = Mathf.Max(0f, squeezeFac - 1f / 30f);
		}
		if (squeezeFac > 0.8f)
		{
			for (int i = 0; i < tentacles.Length; i++)
			{
				for (int j = 0; j < tentacles[i].tChunks.Length; j++)
				{
					tentacles[i].tChunks[j].pos = Vector2.Lerp(tentacles[i].tChunks[j].pos, base.mainBodyChunk.pos, Custom.LerpMap(squeezeFac, 0.8f, 1f, 0f, 0.5f));
				}
			}
		}
		for (int k = 0; k < base.bodyChunks.Length; k++)
		{
			base.bodyChunks[k].terrainSqueeze = 1f - squeezeFac;
		}
		for (int l = 0; l < bodyChunkConnections.Length; l++)
		{
			bodyChunkConnections[l].type = (squeeze ? BodyChunkConnection.Type.Pull : BodyChunkConnection.Type.Normal);
		}
		squeeze = false;
		hangingInTentacle = false;
		int num = 0;
		for (int m = 0; m < tentacles.Length; m++)
		{
			if (ModManager.MSC)
			{
				if ((base.State as DaddyState).tentacleHealth[m] < 1f)
				{
					if (base.abstractCreature.superSizeMe || isHD)
					{
						(base.State as DaddyState).tentacleHealth[m] += 0.0012f;
					}
					else if (SizeClass)
					{
						(base.State as DaddyState).tentacleHealth[m] += 0.0003f;
					}
				}
				if ((base.State as DaddyState).tentacleHealth[m] > 1f)
				{
					(base.State as DaddyState).tentacleHealth[m] = 1f;
				}
			}
			tentacles[m].Update();
			if (tentacles[m].atGrabDest)
			{
				num++;
			}
			tentacles[m].retractFac = squeezeFac;
		}
		if (digestingCounter > 0)
		{
			digestingCounter--;
			if (digestingCounter > 30)
			{
				eyesClosed = Math.Max(10, eyesClosed);
			}
			base.stun = Math.Max(10, base.stun);
		}
		if (eyesClosed > 0)
		{
			eyesClosed--;
		}
		Eat(eu);
		if (base.Consious)
		{
			Act(num);
			if (room.BackgroundNoise > 0.35f)
			{
				eyesClosed = Math.Max(eyesClosed, 15 + (int)Custom.LerpMap(room.BackgroundNoise, 0.35f, 1f, 15f, 100f));
			}
		}
		else
		{
			if (HDmode && !base.dead && !base.Consious && AI.preyTracker != null)
			{
				AI.preyTracker.ForgetAllPrey();
			}
			eyesClosed = Math.Max(eyesClosed, 15);
		}
	}

	private void Act(int legsGrabbing)
	{
		AI.Update();
		float num = 0.6f;
		int num2 = 3;
		float num3 = 1.2f;
		float num4 = 0.2f;
		if (HDmode)
		{
			num = 0.5f;
			num2 = 3;
			num3 = 1.1f;
			num4 = 1f;
		}
		Vector2? vector = null;
		MovementConnection movementConnection = default(MovementConnection);
		if (base.safariControlled)
		{
			stuckPos = null;
			if (isHD)
			{
				num = 0.45f;
			}
			else if (base.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.BrotherLongLegs)
			{
				num = 0.35f;
			}
			else if (base.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.DaddyLongLegs)
			{
				num = 0.25f;
			}
			else if (base.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
			{
				num = 0.15f;
			}
			MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
			if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				type = MovementConnection.MovementType.ShortCut;
			}
			else
			{
				for (int i = 0; i < Custom.fourDirections.Length; i++)
				{
					if (room.GetTile(base.mainBodyChunk.pos + Custom.fourDirections[i].ToVector2() * 20f).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					{
						type = MovementConnection.MovementType.BigCreatureShortCutSqueeze;
						break;
					}
				}
			}
			if (inputWithDiagonals.HasValue)
			{
				if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
				{
					bool flag = false;
					for (int j = 0; j < tentacles.Length; j++)
					{
						if (tentacles[j].grabChunk != null && tentacles[j].grabChunk.owner is Creature)
						{
							flag = true;
							break;
						}
					}
					if (!inputWithDiagonals.Value.pckp || flag)
					{
						vector = base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f;
						movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(vector.Value), 2);
					}
					else
					{
						moving = false;
					}
				}
				else
				{
					moving = false;
				}
				if ((inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw) || inputWithDiagonals.Value.jmp)
				{
					for (int k = 0; k < tentacles.Length; k++)
					{
						tentacles[k].neededForLocomotion = true;
						tentacles[k].SwitchTask(DaddyTentacle.Task.Locomotion);
					}
				}
			}
			else
			{
				moving = false;
			}
			if (!moving)
			{
				unconditionalSupport = 1f;
				num3 = (isHD ? Mathf.InverseLerp(0f, 3f, legsGrabbing) : ((legsGrabbing <= tentacles.Length / 2) ? (0.5f + Mathf.Lerp(0f, 0.5f, legsGrabbing / (tentacles.Length / 2))) : 1f));
			}
			else if (legsGrabbing < tentacles.Length / 2)
			{
				num3 *= Mathf.Lerp(0.6f, 1f, legsGrabbing / (tentacles.Length / 2));
			}
			if (inputWithDiagonals.Value.jmp)
			{
				unconditionalSupport = 0f;
				for (int l = 0; l < tentacles.Length; l++)
				{
					tentacles[l].neededForLocomotion = false;
					tentacles[l].SwitchTask(DaddyTentacle.Task.Hunt);
				}
				num3 = -1f;
			}
		}
		if (stuckPos == null)
		{
			if (notFollowingPathToCurrentGoalCounter < 200 && AI.pathFinder.GetEffectualDestination != AI.pathFinder.GetDestination)
			{
				notFollowingPathToCurrentGoalCounter++;
			}
			else if (notFollowingPathToCurrentGoalCounter > 0)
			{
				notFollowingPathToCurrentGoalCounter--;
			}
			if (notFollowingPathToCurrentGoalCounter > 100)
			{
				for (int m = 0; m < base.bodyChunks.Length; m++)
				{
					if (legsGrabbing != 0)
					{
						break;
					}
					if (base.bodyChunks[m].ContactPoint.x != 0 || base.bodyChunks[m].ContactPoint.y != 0)
					{
						legsGrabbing = 1;
					}
				}
			}
			int num5 = 0;
			if ((Custom.ManhattanDistance(base.abstractCreature.pos, AI.pathFinder.GetEffectualDestination) > num2 || notFollowingPathToCurrentGoalCounter > 100) && legsGrabbing > 0)
			{
				pastPositions.Insert(0, base.abstractCreature.pos.Tile);
				if (pastPositions.Count > 80)
				{
					pastPositions.RemoveAt(pastPositions.Count - 1);
				}
				for (int n = 40; n < pastPositions.Count; n++)
				{
					if (Custom.DistLess(base.abstractCreature.pos.Tile, pastPositions[n], 4f))
					{
						num5++;
					}
				}
			}
			if (num5 > 30)
			{
				stuckCounter++;
			}
			else
			{
				stuckCounter -= 2;
			}
			stuckCounter = Custom.IntClamp(stuckCounter, 0, 200);
			if (stuckCounter > 100)
			{
				for (int num6 = 0; num6 < base.bodyChunks.Length; num6++)
				{
					base.bodyChunks[num6].vel += Custom.RNV() * 3f * UnityEngine.Random.value * Mathf.InverseLerp(100f, 200f, stuckCounter);
				}
			}
			if (base.safariControlled)
			{
				stuckCounter = 0;
			}
		}
		else
		{
			stuckCounter = 0;
		}
		if ((legsGrabbing > tentacles.Length / 2 && moving) || stuckCounter > 100)
		{
			float num7 = float.MinValue;
			int num8 = -1;
			for (int num9 = 0; num9 < tentacles.Length; num9++)
			{
				if (tentacles[num9].atGrabDest && tentacles[num9].huntCreature == null && tentacles[num9].ReleaseScore() > num7)
				{
					num7 = tentacles[num9].ReleaseScore();
					num8 = num9;
				}
			}
			if (num8 > -1)
			{
				List<IntVector2> path = null;
				tentacles[num8].UpdateClimbGrabPos(ref path);
			}
		}
		float num10 = 0f;
		float num11 = 0f;
		for (int num12 = 0; num12 < tentacles.Length; num12++)
		{
			float num13 = Mathf.Pow(tentacles[num12].chunksGripping, 0.5f);
			if (tentacles[num12].atGrabDest && tentacles[num12].grabDest.HasValue)
			{
				num11 += Mathf.Pow(Mathf.InverseLerp(Custom.LerpMap(stuckCounter, 0f, 100f, -0.1f, -1f), 0.85f, Vector2.Dot((tentacles[num12].floatGrabDest.Value - base.mainBodyChunk.pos).normalized, moveDirection)), 0.8f) / (float)tentacles.Length;
				num13 = Mathf.Lerp(num13, 1f, 0.75f);
			}
			num10 += num13 / (float)tentacles.Length;
		}
		num11 = Mathf.Pow(num11 * num10, Custom.LerpMap(stuckCounter, 100f, 200f, 0.8f, 0.1f));
		num10 = Mathf.Pow(num10, 0.3f);
		num11 = Mathf.Max(num11, squeezeFac);
		num10 = Mathf.Max(num10, squeezeFac);
		num10 = Mathf.Max(num10, unconditionalSupport);
		num11 = Mathf.Max(num11, unconditionalSupport);
		float num14 = 0f;
		for (int num15 = 0; num15 < tentacles.Length; num15++)
		{
			if (tentacles[num15].neededForLocomotion)
			{
				num14 += 1f / (float)tentacles.Length;
			}
		}
		if (num10 < 1f - num14)
		{
			float num16 = float.MinValue;
			int num17 = UnityEngine.Random.Range(0, tentacles.Length);
			for (int num18 = 0; num18 < tentacles.Length; num18++)
			{
				if (!tentacles[num18].neededForLocomotion)
				{
					float num19 = 1000f / Mathf.Lerp(tentacles[num18].idealLength * (float)room.aimap.getTerrainProximity(tentacles[num18].Tip.pos), 200f, 0.8f);
					if (tentacles[num18].task == DaddyTentacle.Task.Grabbing)
					{
						num19 *= 0.01f;
					}
					if (tentacles[num18].task == DaddyTentacle.Task.Hunt)
					{
						num19 *= 0.1f;
					}
					if (tentacles[num18].task == DaddyTentacle.Task.ExamineSound)
					{
						num19 *= 0.6f;
					}
					if (num19 > num16)
					{
						num16 = num19;
						num17 = num18;
					}
				}
			}
			tentacles[num17].neededForLocomotion = true;
		}
		else if ((double)num10 > 0.85)
		{
			tentacles[UnityEngine.Random.Range(0, tentacles.Length)].neededForLocomotion = false;
		}
		for (int num20 = 0; num20 < base.bodyChunks.Length; num20++)
		{
			base.bodyChunks[num20].vel *= Mathf.Lerp(1f, Mathf.Lerp(0.95f, 0.8f, squeezeFac), num10);
			base.bodyChunks[num20].vel.y += (base.gravity - base.buoyancy * base.bodyChunks[num20].submersion) * num10 * num3;
		}
		MovementConnection movementConnection2 = default(MovementConnection);
		if (!base.safariControlled && Custom.ManhattanDistance(base.abstractCreature.pos, AI.pathFinder.GetEffectualDestination) < num2)
		{
			for (int num21 = 0; num21 < base.bodyChunks.Length; num21++)
			{
				base.bodyChunks[num21].vel += Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetEffectualDestination) - base.bodyChunks[0].pos, 30f) / 30f * num * num11;
			}
		}
		else if (base.safariControlled && vector.HasValue && Custom.ManhattanDistance(base.abstractCreature.pos, Custom.MakeWorldCoordinate(new IntVector2((int)vector.Value.x / 20, (int)vector.Value.y / 20), base.abstractCreature.Room.index)) < num2)
		{
			for (int num22 = 0; num22 < base.bodyChunks.Length; num22++)
			{
				base.bodyChunks[num22].vel += Vector2.ClampMagnitude(room.MiddleOfTile((int)vector.Value.x / 20, (int)vector.Value.y / 20) - base.bodyChunks[0].pos, 30f) / 30f * num * num11;
			}
		}
		else
		{
			for (int num23 = 0; num23 < base.bodyChunks.Length; num23++)
			{
				if (!(movementConnection2 == default(MovementConnection)))
				{
					break;
				}
				for (int num24 = 0; num24 < 9; num24++)
				{
					if (!(movementConnection2 == default(MovementConnection)))
					{
						break;
					}
					movementConnection2 = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[num23].pos + Custom.zeroAndEightDirectionsDiagonalsLast[num24].ToVector2() * 20f), actuallyFollowingThisPath: true);
				}
			}
			if (movementConnection2 == default(MovementConnection))
			{
				movementConnection2 = CheckTentaclesForAccessibleTerrain();
			}
		}
		if (base.safariControlled && (movementConnection2 == default(MovementConnection) || !base.AllowableControlledAIOverride(movementConnection2.type)))
		{
			movementConnection2 = movementConnection;
		}
		moving = movementConnection2 != default(MovementConnection);
		if (ModManager.MMF && movementConnection2 == default(MovementConnection))
		{
			if (!base.safariControlled)
			{
				moveDirection = (moveDirection + new Vector2(0f, 0f - num / 10f)).normalized;
			}
		}
		else if (movementConnection2 != default(MovementConnection))
		{
			if (shortcutDelay < 1)
			{
				squeeze = movementConnection2.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze;
			}
			if (shortcutDelay < 1 && movementConnection2.type == MovementConnection.MovementType.ShortCut)
			{
				enteringShortCut = movementConnection2.StartTile;
				return;
			}
			base.GoThroughFloors = movementConnection2.DestTile.y < movementConnection2.StartTile.y;
			for (int num25 = 0; num25 < base.bodyChunks.Length; num25++)
			{
				base.bodyChunks[num25].vel += Custom.DirVec(base.bodyChunks[0].pos, room.MiddleOfTile(movementConnection2.DestTile)) * num * num11;
			}
			MovementConnection movementConnection3 = movementConnection2;
			Vector2 vector2 = Custom.DirVec(movementConnection3.StartTile.ToVector2(), movementConnection3.DestTile.ToVector2());
			for (int num26 = 0; num26 < 10; num26++)
			{
				movementConnection3 = (AI.pathFinder as StandardPather).FollowPath(movementConnection3.destinationCoord, actuallyFollowingThisPath: false);
				if (movementConnection3 == default(MovementConnection))
				{
					break;
				}
				vector2 += Custom.DirVec(movementConnection3.StartTile.ToVector2(), movementConnection3.DestTile.ToVector2());
				if (num26 < 2 && movementConnection3.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze && shortcutDelay < 1)
				{
					squeeze = true;
				}
			}
			moveDirection = (moveDirection + vector2.normalized * num4).normalized;
			if (base.safariControlled && movementConnection != default(MovementConnection) && movementConnection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
			{
				squeeze = true;
			}
		}
		else
		{
			moveDirection = (moveDirection + new Vector2(0f, 0f - num / 10f)).normalized;
		}
	}

	private MovementConnection CheckTentaclesForAccessibleTerrain()
	{
		Vector2 pos = base.mainBodyChunk.pos;
		float num = float.MaxValue;
		Vector2 vector = base.mainBodyChunk.pos;
		if (AI.pathFinder.GetDestination.room == base.abstractCreature.pos.room && AI.pathFinder.GetDestination.NodeDefined)
		{
			vector = room.MiddleOfTile(AI.pathFinder.GetDestination);
		}
		for (int i = 0; i < tentacles.Length; i++)
		{
			for (int j = 0; j < tentacles[i].tChunks.Length; j++)
			{
				if (room.aimap.TileAccessibleToCreature(tentacles[i].tChunks[j].pos, base.Template) && Custom.DistLess(tentacles[i].tChunks[j].pos, vector, num))
				{
					pos = tentacles[i].tChunks[j].pos;
					num = Vector2.Distance(tentacles[i].tChunks[j].pos, vector);
				}
			}
		}
		if (num < float.MaxValue)
		{
			return new MovementConnection(MovementConnection.MovementType.Standard, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(pos), (int)(num / 20f));
		}
		return default(MovementConnection);
	}

	public void Eat(bool eu)
	{
		Vector2 middleOfBody = MiddleOfBody;
		for (int num = eatObjects.Count - 1; num >= 0; num--)
		{
			if (eatObjects[num].progression > 1f)
			{
				if (eatObjects[num].chunk.owner is Creature)
				{
					if (!SizeClass)
					{
						digestingCounter = (int)Custom.LerpMap((eatObjects[num].chunk.owner as Creature).Template.bodySize, 0.2f, 5f, 50f, 1100f);
					}
					AI.tracker.ForgetCreature((eatObjects[num].chunk.owner as Creature).abstractCreature);
					if (eatObjects[num].chunk.owner is Player player)
					{
						player.PermaDie();
					}
				}
				eatObjects[num].chunk.owner.Destroy();
				eatObjects.RemoveAt(num);
			}
			else
			{
				eyesClosed = Math.Max(eyesClosed, 15);
				if (eatObjects[num].chunk.owner.collisionLayer != 0)
				{
					eatObjects[num].chunk.owner.ChangeCollisionLayer(0);
				}
				if (ModManager.MMF && eatObjects[num].chunk.owner is Creature)
				{
					(eatObjects[num].chunk.owner as Creature).enteringShortCut = null;
				}
				float progression = eatObjects[num].progression;
				eatObjects[num].progression += 0.0125f;
				if (progression <= 0.5f && eatObjects[num].progression > 0.5f)
				{
					if (eatObjects[num].chunk.owner is Creature)
					{
						(eatObjects[num].chunk.owner as Creature).Die();
					}
					for (int i = 0; i < eatObjects[num].chunk.owner.bodyChunkConnections.Length; i++)
					{
						eatObjects[num].chunk.owner.bodyChunkConnections[i].type = BodyChunkConnection.Type.Pull;
					}
				}
				float num2 = eatObjects[num].distance * (1f - eatObjects[num].progression);
				eatObjects[num].chunk.vel *= 0f;
				eatObjects[num].chunk.MoveFromOutsideMyUpdate(eu, middleOfBody + Custom.DirVec(middleOfBody, eatObjects[num].chunk.pos) * num2);
				for (int j = 0; j < eatObjects[num].chunk.owner.bodyChunks.Length; j++)
				{
					eatObjects[num].chunk.owner.bodyChunks[j].vel *= 1f - eatObjects[num].progression;
					eatObjects[num].chunk.owner.bodyChunks[j].MoveFromOutsideMyUpdate(eu, Vector2.Lerp(eatObjects[num].chunk.owner.bodyChunks[j].pos, middleOfBody + Custom.DirVec(middleOfBody, eatObjects[num].chunk.owner.bodyChunks[j].pos) * num2, eatObjects[num].progression));
				}
				if (eatObjects[num].chunk.owner.graphicsModule != null && eatObjects[num].chunk.owner.graphicsModule.bodyParts != null)
				{
					for (int k = 0; k < eatObjects[num].chunk.owner.graphicsModule.bodyParts.Length; k++)
					{
						eatObjects[num].chunk.owner.graphicsModule.bodyParts[k].vel *= 1f - eatObjects[num].progression;
						eatObjects[num].chunk.owner.graphicsModule.bodyParts[k].pos = Vector2.Lerp(eatObjects[num].chunk.owner.graphicsModule.bodyParts[k].pos, middleOfBody, eatObjects[num].progression);
					}
				}
			}
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		Tracker.CreatureRepresentation creatureRepresentation = AI.tracker.RepresentationForObject(otherObject, AddIfMissing: false);
		if (creatureRepresentation == null || !(AI.DynamicRelationship(creatureRepresentation).type == CreatureTemplate.Relationship.Type.Eats) || !CheckDaddyConsumption(otherObject))
		{
			return;
		}
		bool flag = false;
		if (!SizeClass && digestingCounter > 0)
		{
			return;
		}
		for (int i = 0; i < tentacles.Length; i++)
		{
			if (flag)
			{
				break;
			}
			if (tentacles[i].grabChunk != null && tentacles[i].grabChunk.owner == otherObject)
			{
				flag = true;
			}
		}
		for (int j = 0; j < eatObjects.Count && flag; j++)
		{
			if (eatObjects[j].chunk.owner == otherObject)
			{
				flag = false;
			}
		}
		if (!flag || (base.safariControlled && (!base.safariControlled || !inputWithDiagonals.HasValue || !inputWithDiagonals.Value.pckp)))
		{
			return;
		}
		if (base.graphicsModule != null)
		{
			if (otherObject is IDrawable)
			{
				base.graphicsModule.AddObjectToInternalContainer(otherObject as IDrawable, 0);
			}
			else if (otherObject.graphicsModule != null)
			{
				base.graphicsModule.AddObjectToInternalContainer(otherObject.graphicsModule, 0);
			}
		}
		eatObjects.Add(new EatObject(otherObject.bodyChunks[otherChunk], Vector2.Distance(MiddleOfBody, otherObject.bodyChunks[otherChunk].pos)));
		room.PlaySound(SizeClass ? SoundID.Daddy_Digestion_Init : SoundID.Bro_Digestion_Init, base.bodyChunks[myChunk]);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		room.PlaySound((speed < 8f) ? SoundID.Cicada_Light_Terrain_Impact : SoundID.Cicada_Heavy_Terrain_Impact, base.mainBodyChunk);
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (hitAppendage != null)
		{
			damage /= (SizeClass ? 2.2f : 1.7f);
			stunBonus /= (SizeClass ? 2f : 0.9f);
			(base.State as DaddyState).tentacleHealth[hitAppendage.appendage.appIndex] -= damage;
			tentacles[hitAppendage.appendage.appIndex].stun = Math.Max(tentacles[hitAppendage.appendage.appIndex].stun, (int)(damage * 48f + stunBonus));
			damage = 0f;
			stunBonus = 0f;
		}
		else if (HDmode)
		{
			Stun(200);
		}
		damage /= ((ModManager.MSC && base.abstractCreature.superSizeMe) ? 4f : 1f);
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
	}

	public override void Stun(int st)
	{
		for (int i = 0; i < tentacles.Length; i++)
		{
			if (UnityEngine.Random.value < 0.5f || !SizeClass)
			{
				tentacles[i].neededForLocomotion = true;
				tentacles[i].SwitchTask(DaddyTentacle.Task.Locomotion);
			}
		}
		if (!HDmode)
		{
			st = ((!SizeClass) ? (st / 3) : 0);
		}
		base.Stun(st);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		shortcutDelay = 80;
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) + Custom.RNV();
			base.bodyChunks[i].lastPos = base.bodyChunks[i].pos;
			base.bodyChunks[i].vel = vector * 4f;
		}
		squeezeFac = 1f;
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
		for (int j = 0; j < tentacles.Length; j++)
		{
			tentacles[j].Reset(tentacles[j].connectedChunk.pos);
		}
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		segment--;
		if (segment < 0)
		{
			return tentacles[appendage].connectedChunk.pos;
		}
		return tentacles[appendage].tChunks[segment].pos;
	}

	public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
	{
		if (pos.prevSegment > 0)
		{
			tentacles[pos.appendage.appIndex].tChunks[pos.prevSegment - 1].pos += momentum / 0.04f * (1f - pos.distanceToNext);
			tentacles[pos.appendage.appIndex].tChunks[pos.prevSegment - 1].vel += momentum / 0.04f * (1f - pos.distanceToNext);
		}
		else
		{
			tentacles[pos.appendage.appIndex].connectedChunk.pos += momentum / tentacles[pos.appendage.appIndex].connectedChunk.mass * (1f - pos.distanceToNext);
			tentacles[pos.appendage.appIndex].connectedChunk.vel += momentum / tentacles[pos.appendage.appIndex].connectedChunk.mass * (1f - pos.distanceToNext);
		}
		tentacles[pos.appendage.appIndex].tChunks[pos.prevSegment].pos += momentum / 0.04f * pos.distanceToNext;
		tentacles[pos.appendage.appIndex].tChunks[pos.prevSegment].vel += momentum / 0.04f * pos.distanceToNext;
	}

	public bool CheckDaddyConsumption(PhysicalObject otherObject)
	{
		bool result = false;
		if (otherObject != null)
		{
			if (otherObject is DaddyLongLegs)
			{
				if (!(otherObject as DaddyLongLegs).SizeClass && SizeClass)
				{
					result = true;
				}
				if (ModManager.MSC && (otherObject as DaddyLongLegs).Template.type != MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
				{
					result = true;
				}
			}
			else
			{
				result = SizeClass || otherObject.TotalMass < 5f;
			}
		}
		return result;
	}

	public override Color ShortCutColor()
	{
		return effectColor;
	}
}
