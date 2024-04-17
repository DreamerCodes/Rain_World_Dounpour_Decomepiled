using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SeedCob : PhysicalObject, IDrawable, PhysicalObject.IHaveAppendages
{
	public class AbstractSeedCob : AbstractConsumable
	{
		public bool opened;

		public bool dead = true;

		public bool spawnedUtility;

		public AbstractSeedCob(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int consumableIndex, bool dead, PlacedObject.ConsumableObjectData consumableData)
			: base(world, AbstractObjectType.SeedCob, realizedObject, pos, ID, originRoom, consumableIndex, consumableData)
		{
			this.dead = dead;
			if (!dead && world.game.session is StoryGameSession && (world.game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, originRoom, consumableIndex))
			{
				this.dead = true;
			}
			opened = this.dead;
		}
	}

	public Vector2 placedPos;

	public Vector2 rootPos;

	public int totalSprites;

	public int stalkSegments;

	public int cobSegments;

	public float stalkLength;

	public Vector2 rootDir;

	public Vector2 cobDir;

	public Vector2[] seedPositions;

	public bool[] seedsPopped;

	public int seedPopCounter = -1;

	public Vector2[,] leaves;

	public float open;

	public float lastOpen;

	public Vector2? delayedPush;

	public int pushDelay;

	public Color yellowColor;

	private float freezingCounter;

	public int AllPlantsFrozenCycleTime;

	private Color StoredBlackColor;

	private Color StoredPlantColor;

	public int CobSprite => 2;

	public AbstractSeedCob AbstractCob => abstractPhysicalObject as AbstractSeedCob;

	public int StalkSprite(int part)
	{
		return part;
	}

	public int SeedSprite(int seed, int part)
	{
		return 3 + seed + part * seedPositions.Length;
	}

	public int ShellSprite(int side)
	{
		return 3 + seedPositions.Length * 3 + side;
	}

	public int LeafSprite(int leaf)
	{
		return 3 + seedPositions.Length * 3 + 2 + leaf;
	}

	public SeedCob(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, 0.1f);
		base.bodyChunks[1] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, 0.1f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 60f, BodyChunkConnection.Type.Normal, 1f, -1f);
		freezingCounter = 0f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		AllPlantsFrozenCycleTime = AbstractCob.world.rainCycle.cycleLength + (int)Mathf.Lerp(-150f, 3000f, 1f - Mathf.Pow(UnityEngine.Random.value + 1f, -4f));
		UnityEngine.Random.state = state;
		base.airFriction = 0.9f;
		base.gravity = 0f;
		bounce = 0.2f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.1f;
		base.bodyChunks[0].collideWithObjects = false;
		base.bodyChunks[1].collideWithObjects = false;
		appendages = new List<Appendage>();
		appendages.Add(new Appendage(this, 0, 2));
		if (ModManager.MSC && room != null && room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard)
		{
			freezingCounter = Mathf.InverseLerp(AbstractCob.world.rainCycle.cycleLength, (float)AllPlantsFrozenCycleTime + 2000f, AbstractCob.world.rainCycle.timer);
			if (!AbstractCob.opened && freezingCounter >= 1f)
			{
				AbstractCob.dead = true;
				AbstractCob.opened = true;
			}
		}
		if (AbstractCob.opened)
		{
			open = 1f;
			lastOpen = 1f;
		}
		canBeHitByWeapons = !AbstractCob.opened;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (seedPositions != null)
		{
			return;
		}
		if (AbstractCob.placedObjectIndex >= 0 && AbstractCob.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			placedPos = placeRoom.roomSettings.placedObjects[AbstractCob.placedObjectIndex].pos;
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((int)placedPos.x);
		rootPos = new Vector2(placedPos.x, -10f);
		List<IntVector2> list = new List<IntVector2>();
		for (int i = placeRoom.GetTilePosition(placedPos).x - 1; i <= placeRoom.GetTilePosition(placedPos).x + 1; i++)
		{
			for (int num = placeRoom.GetTilePosition(placedPos).y + ((i != 0) ? (-2) : 0); num >= 0; num--)
			{
				if (placeRoom.GetTile(i, num - 1).Solid)
				{
					if (!placeRoom.GetTile(i, num).Solid)
					{
						list.Add(new IntVector2(i, num - 1));
					}
					break;
				}
			}
		}
		if (list.Count > 0)
		{
			IntVector2 pos = list[UnityEngine.Random.Range(0, list.Count)];
			rootPos = placeRoom.MiddleOfTile(pos) + new Vector2(Mathf.Lerp(-10f, 10f, UnityEngine.Random.value), 10f);
		}
		bodyChunkConnections[0].distance = Mathf.Lerp(60f, Vector2.Distance(rootPos, placedPos) / 2f, 0.3f);
		leaves = new Vector2[Custom.IntClamp(Mathf.RoundToInt(Mathf.Lerp(bodyChunkConnections[0].distance / 10f, 10f, 0.5f)), 4, 14), 4];
		seedPositions = new Vector2[Custom.IntClamp(Mathf.RoundToInt(bodyChunkConnections[0].distance / 2f), 10, 70)];
		for (int j = 0; j < seedPositions.Length; j++)
		{
			float num2 = (float)j / (float)(seedPositions.Length - 1);
			seedPositions[j] = new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Pow(Mathf.Sin(num2 * (float)Math.PI), 0.5f), num2);
		}
		totalSprites = 5 + leaves.GetLength(0) + seedPositions.Length * 3;
		seedsPopped = new bool[seedPositions.Length];
		if (AbstractCob.opened)
		{
			for (int k = 0; k < seedsPopped.Length; k++)
			{
				seedsPopped[k] = true;
			}
		}
		stalkSegments = Custom.IntClamp((int)(Vector2.Distance(rootPos, placedPos) / 10f), 5, 50);
		cobSegments = 10;
		rootDir = Custom.DegToVec(Custom.AimFromOneVectorToAnother(rootPos, placedPos) + Mathf.Lerp(-45f, 45f, UnityEngine.Random.value));
		cobDir = Custom.DegToVec(Custom.AimFromOneVectorToAnother(placedPos, rootPos) + Mathf.Lerp(-25f, 25f, UnityEngine.Random.value));
		for (int l = 0; l < leaves.GetLength(0); l++)
		{
			leaves[l, 3].y = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value) * bodyChunkConnections[0].distance * 0.2f;
			leaves[l, 3].x = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value) * Mathf.Lerp(bodyChunkConnections[0].distance, 100f, 0.5f) * 0.01f * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			leaves[l, 0] = placedPos + cobDir * bodyChunkConnections[0].distance;
			leaves[l, 1] = leaves[l, 0];
		}
		stalkLength = Vector2.Distance(rootPos, placedPos + cobDir * bodyChunkConnections[0].distance) + 5f;
		base.firstChunk.HardSetPosition(placedPos);
		base.bodyChunks[1].HardSetPosition(placedPos + cobDir * bodyChunkConnections[0].distance);
		UnityEngine.Random.state = state;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if ((ModManager.MSC && room != null && room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard) || freezingCounter > 0f)
		{
			freezingCounter = Mathf.InverseLerp(AbstractCob.world.rainCycle.cycleLength, AllPlantsFrozenCycleTime, AbstractCob.world.rainCycle.timer);
			if (!AbstractCob.opened && freezingCounter >= 0.5f && UnityEngine.Random.value < 0.005f)
			{
				room.PlaySound(SoundID.Snail_Warning_Click, base.bodyChunks[0], loop: false, 0.4f, UnityEngine.Random.Range(1.4f, 1.9f));
				base.bodyChunks[0].vel += Custom.RNV() * freezingCounter;
			}
			if (!AbstractCob.opened && freezingCounter >= 1f && UnityEngine.Random.value < 0.002f)
			{
				spawnUtilityFoods();
				room.PlaySound(SoundID.Seed_Cob_Open, base.firstChunk);
			}
		}
		base.firstChunk.vel += (placedPos - base.firstChunk.pos) / Custom.LerpMap(Vector2.Distance(placedPos, base.firstChunk.pos), 5f, 100f, 2000f, 150f, 0.8f);
		base.bodyChunks[1].vel += (placedPos + cobDir * bodyChunkConnections[0].distance - base.bodyChunks[1].pos) / Custom.LerpMap(Vector2.Distance(placedPos + cobDir * bodyChunkConnections[0].distance, base.bodyChunks[1].pos), 5f, 100f, 800f, 50f, 0.2f);
		if (!Custom.DistLess(base.bodyChunks[1].pos, rootPos, stalkLength))
		{
			Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, rootPos);
			float num = Vector2.Distance(base.bodyChunks[1].pos, rootPos);
			base.bodyChunks[1].pos += vector * (num - stalkLength) * 0.2f;
			base.bodyChunks[1].vel += vector * (num - stalkLength) * 0.2f;
		}
		lastOpen = open;
		if (AbstractCob.opened)
		{
			open = Mathf.Lerp(open, 1f, Mathf.Lerp(0.01f, 0.0001f, open));
		}
		if (seedPopCounter > -1)
		{
			seedPopCounter--;
			if (seedPopCounter < 1)
			{
				for (int i = 0; i < seedsPopped.Length; i++)
				{
					if (!seedsPopped[i])
					{
						seedsPopped[i] = true;
						float num2 = (float)i / (float)(seedsPopped.Length - 1);
						if (i == seedsPopped.Length - 1)
						{
							seedPopCounter = -1;
						}
						else
						{
							seedPopCounter = Mathf.RoundToInt(Mathf.Pow(1f - num2, 0.5f) * 20f * (0.5f + 0.5f * UnityEngine.Random.value));
						}
						Vector2 normalized = (Custom.PerpendicularVector(base.bodyChunks[0].pos, base.bodyChunks[1].pos) * seedPositions[i].x + Custom.RNV() * UnityEngine.Random.value).normalized;
						base.firstChunk.vel += normalized * 0.7f * seedPositions[i].y;
						base.firstChunk.pos += normalized * 0.7f * seedPositions[i].y;
						base.bodyChunks[1].vel += normalized * 0.7f * (1f - seedPositions[i].y);
						base.bodyChunks[1].pos += normalized * 0.7f * (1f - seedPositions[i].y);
						Vector2 pos = Vector2.Lerp(base.bodyChunks[1].pos, base.bodyChunks[0].pos, seedPositions[i].y);
						room.PlaySound(SoundID.Seed_Cob_Pop, pos);
						room.AddObject(new WaterDrip(pos, (Vector2)Vector3.Slerp(Custom.PerpendicularVector(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * seedPositions[i].x, Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos), Mathf.Pow(num2, 2f) * 0.5f) * 11f + Custom.RNV() * 4f * UnityEngine.Random.value, waterColor: false));
						break;
					}
				}
			}
		}
		float num3 = Custom.AimFromOneVectorToAnother(base.firstChunk.pos, base.bodyChunks[1].pos);
		for (int j = 0; j < leaves.GetLength(0); j++)
		{
			leaves[j, 1] = leaves[j, 0];
			leaves[j, 0] += leaves[j, 2];
			leaves[j, 2] *= 0.9f;
			Vector2 vector2 = Custom.DirVec(leaves[j, 0], base.bodyChunks[1].pos);
			float num4 = Vector2.Distance(leaves[j, 0], base.bodyChunks[1].pos);
			leaves[j, 0] += vector2 * (num4 - leaves[j, 3].y);
			leaves[j, 2] += vector2 * (num4 - leaves[j, 3].y);
			leaves[j, 2] += Custom.DegToVec(num3 + Mathf.Lerp(-45f, 45f, (float)j / (float)(leaves.GetLength(0) - 1)));
		}
		if (delayedPush.HasValue)
		{
			if (pushDelay > 0)
			{
				pushDelay--;
			}
			else
			{
				base.firstChunk.vel += delayedPush.Value;
				base.bodyChunks[1].vel += delayedPush.Value;
				room.PlaySound(SoundID.Seed_Cob_Pick, base.firstChunk.pos);
				delayedPush = null;
			}
		}
		if (AbstractCob.dead || !(open > 0.8f))
		{
			return;
		}
		for (int k = 0; k < (ModManager.MSC ? room.abstractRoom.creatures.Count : room.game.Players.Count); k++)
		{
			Player player;
			if (ModManager.MSC)
			{
				Creature realizedCreature = room.abstractRoom.creatures[k].realizedCreature;
				if (realizedCreature == null || !(realizedCreature is Player))
				{
					continue;
				}
				player = realizedCreature as Player;
			}
			else
			{
				if (room.game.Players[k].realizedCreature == null)
				{
					continue;
				}
				player = room.game.Players[k].realizedCreature as Player;
			}
			if (player.room != room || player.handOnExternalFoodSource.HasValue || player.eatExternalFoodSourceCounter >= 1 || player.dontEatExternalFoodSourceCounter >= 1 || player.FoodInStomach >= player.MaxFoodInStomach || (player.touchedNoInputCounter <= 5 && !player.input[0].pckp && (!ModManager.MSC || !(player.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC))) || (ModManager.MSC && !(player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear)) || player.FreeHand() <= -1)
			{
				continue;
			}
			Vector2 pos2 = player.mainBodyChunk.pos;
			Vector2 vector3 = Custom.ClosestPointOnLineSegment(base.bodyChunks[0].pos, base.bodyChunks[1].pos, pos2);
			if (Custom.DistLess(pos2, vector3, 25f))
			{
				player.handOnExternalFoodSource = vector3 + Custom.DirVec(pos2, vector3) * 5f;
				player.eatExternalFoodSourceCounter = 15;
				if (room.game.IsStorySession && player.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && room.game.GetStorySession.playerSessionRecords != null)
				{
					room.game.GetStorySession.playerSessionRecords[(player.abstractCreature.state as PlayerState).playerNumber].AddEat(this);
				}
				delayedPush = Custom.DirVec(pos2, vector3) * 1.2f;
				pushDelay = 4;
				if (player.graphicsModule != null)
				{
					(player.graphicsModule as PlayerGraphics).LookAtPoint(vector3, 100f);
				}
			}
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		if (weapon == null || room == null || room.roomSettings == null)
		{
			return;
		}
		base.HitByWeapon(weapon);
		if (!(weapon is Spear))
		{
			return;
		}
		if (ModManager.MSC)
		{
			if (room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard && weapon.firstChunk.vel.magnitude < 20f)
			{
				if (UnityEngine.Random.Range(0.5f, 0.8f) < freezingCounter)
				{
					spawnUtilityFoods();
				}
				return;
			}
			if (weapon.thrownBy != null && weapon.thrownBy is Player && ((weapon.thrownBy as Player).slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear || (weapon.thrownBy as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint))
			{
				return;
			}
		}
		Open();
	}

	public void Open()
	{
		if (!AbstractCob.opened)
		{
			AbstractCob.opened = true;
			AbstractCob.Consume();
			canBeHitByWeapons = false;
			room.PlaySound(SoundID.Seed_Cob_Open, base.firstChunk);
			seedPopCounter = UnityEngine.Random.Range(30, 60);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (seedPositions != null)
		{
			sLeaser.sprites = new FSprite[totalSprites];
			sLeaser.sprites[StalkSprite(0)] = TriangleMesh.MakeLongMesh(stalkSegments, pointyTip: false, customColor: false);
			sLeaser.sprites[StalkSprite(1)] = TriangleMesh.MakeLongMesh(stalkSegments, pointyTip: false, customColor: true);
			sLeaser.sprites[CobSprite] = TriangleMesh.MakeLongMesh(cobSegments, pointyTip: false, customColor: false);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[ShellSprite(i)] = TriangleMesh.MakeLongMesh(cobSegments, pointyTip: false, customColor: true);
			}
			for (int j = 0; j < seedPositions.Length; j++)
			{
				sLeaser.sprites[SeedSprite(j, 0)] = new FSprite("JetFishEyeA");
				sLeaser.sprites[SeedSprite(j, 1)] = new FSprite("JetFishEyeA");
				sLeaser.sprites[SeedSprite(j, 2)] = new FSprite("pixel");
				sLeaser.sprites[SeedSprite(j, 0)].isVisible = false;
				sLeaser.sprites[SeedSprite(j, 1)].isVisible = false;
				sLeaser.sprites[SeedSprite(j, 2)].isVisible = false;
			}
			for (int k = 0; k < leaves.GetLength(0); k++)
			{
				sLeaser.sprites[LeafSprite(k)] = new FSprite("CentipedeLegB");
				sLeaser.sprites[LeafSprite(k)].anchorY = 0f;
				sLeaser.sprites[LeafSprite(k)].scaleX = leaves[k, 3].x;
			}
			AddToContainer(sLeaser, rCam, null);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (ModManager.MSC && freezingCounter > 0f)
		{
			FreezingPaletteUpdate(sLeaser, rCam);
		}
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(base.bodyChunks[1].lastPos, base.bodyChunks[1].pos, timeStacker);
		float num = 0.5f;
		Vector2 vector3 = rootPos;
		for (int i = 0; i < stalkSegments; i++)
		{
			float f = (float)i / (float)(stalkSegments - 1);
			Vector2 vector4 = Custom.Bezier(rootPos, rootPos + rootDir * Vector2.Distance(rootPos, placedPos) * 0.2f, vector2, vector2 + Custom.DirVec(vector, vector2) * Vector2.Distance(rootPos, placedPos) * 0.2f, f);
			Vector2 normalized = (vector3 - vector4).normalized;
			Vector2 vector5 = Custom.PerpendicularVector(normalized);
			float num2 = Vector2.Distance(vector3, vector4) / 5f;
			float num3 = Mathf.Lerp(bodyChunkConnections[0].distance / 14f, 1.5f, Mathf.Pow(Mathf.Sin(Mathf.Pow(f, 2f) * (float)Math.PI), 0.5f));
			float num4 = 1f;
			Vector2 vector6 = default(Vector2);
			for (int j = 0; j < 2; j++)
			{
				(sLeaser.sprites[StalkSprite(j)] as TriangleMesh).MoveVertice(i * 4, vector3 - normalized * num2 - vector5 * (num3 + num) * 0.5f * num4 - camPos + vector6);
				(sLeaser.sprites[StalkSprite(j)] as TriangleMesh).MoveVertice(i * 4 + 1, vector3 - normalized * num2 + vector5 * (num3 + num) * 0.5f * num4 - camPos + vector6);
				(sLeaser.sprites[StalkSprite(j)] as TriangleMesh).MoveVertice(i * 4 + 2, vector4 + normalized * num2 - vector5 * num3 * num4 - camPos + vector6);
				(sLeaser.sprites[StalkSprite(j)] as TriangleMesh).MoveVertice(i * 4 + 3, vector4 + normalized * num2 + vector5 * num3 * num4 - camPos + vector6);
				num4 = 0.35f;
				vector6 += -rCam.room.lightAngle.normalized * num3 * 0.5f;
			}
			vector3 = vector4;
			num = num3;
		}
		vector3 = vector2 + Custom.DirVec(vector, vector2);
		num = 2f;
		for (int k = 0; k < cobSegments; k++)
		{
			float t = (float)k / (float)(cobSegments - 1);
			Vector2 vector7 = Vector2.Lerp(vector2, vector, t);
			Vector2 normalized2 = (vector3 - vector7).normalized;
			Vector2 vector8 = Custom.PerpendicularVector(normalized2);
			float num5 = Vector2.Distance(vector3, vector7) / 5f;
			float num6 = 2f;
			(sLeaser.sprites[CobSprite] as TriangleMesh).MoveVertice(k * 4, vector3 - normalized2 * num5 - vector8 * (num6 + num) * 0.5f - camPos);
			(sLeaser.sprites[CobSprite] as TriangleMesh).MoveVertice(k * 4 + 1, vector3 - normalized2 * num5 + vector8 * (num6 + num) * 0.5f - camPos);
			(sLeaser.sprites[CobSprite] as TriangleMesh).MoveVertice(k * 4 + 2, vector7 + normalized2 * num5 - vector8 * num6 - camPos);
			(sLeaser.sprites[CobSprite] as TriangleMesh).MoveVertice(k * 4 + 3, vector7 + normalized2 * num5 + vector8 * num6 - camPos);
			vector3 = vector7;
			num = num6;
		}
		float num7 = Mathf.Lerp(lastOpen, open, timeStacker);
		for (int l = 0; l < 2; l++)
		{
			float num8 = -1f + (float)l * 2f;
			num = 2f;
			vector3 = vector + Custom.DirVec(vector2, vector) * 7f;
			float num9 = Custom.AimFromOneVectorToAnother(vector, vector2);
			Vector2 vector9 = vector;
			for (int m = 0; m < cobSegments; m++)
			{
				float num10 = (float)m / (float)(cobSegments - 1);
				vector9 += Custom.DegToVec(num9 + num8 * Mathf.Pow(num7, Mathf.Lerp(1f, 0.1f, num10)) * 50f * Mathf.Pow(num10, 0.5f)) * (Vector2.Distance(vector, vector2) * 1.1f + 8f) / cobSegments;
				Vector2 normalized3 = (vector3 - vector9).normalized;
				Vector2 vector10 = Custom.PerpendicularVector(normalized3);
				float num11 = Vector2.Distance(vector3, vector9) / 5f;
				float num12 = Mathf.Lerp(2f, 6f, Mathf.Pow(Mathf.Sin(Mathf.Pow(num10, 0.5f) * (float)Math.PI), 0.5f));
				(sLeaser.sprites[ShellSprite(l)] as TriangleMesh).MoveVertice(m * 4, vector3 - normalized3 * num11 - vector10 * (num12 + num) * 0.5f * (1 - l) - camPos);
				(sLeaser.sprites[ShellSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 1, vector3 - normalized3 * num11 + vector10 * (num12 + num) * 0.5f * l - camPos);
				(sLeaser.sprites[ShellSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 2, vector9 + normalized3 * num11 - vector10 * num12 * (1 - l) - camPos);
				(sLeaser.sprites[ShellSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 3, vector9 + normalized3 * num11 + vector10 * num12 * l - camPos);
				vector3 = new Vector2(vector9.x, vector9.y);
				num = num12;
				num9 = Custom.VecToDeg(-normalized3);
			}
		}
		if (num7 > 0f)
		{
			Vector2 vector11 = Custom.DirVec(vector2, vector);
			Vector2 vector12 = Custom.PerpendicularVector(vector11);
			for (int n = 0; n < seedPositions.Length; n++)
			{
				Vector2 vector13 = vector2 + vector11 * seedPositions[n].y * (Vector2.Distance(vector2, vector) - 10f) + vector12 * seedPositions[n].x * 3f;
				float num13 = 1f + Mathf.Sin((float)n / (float)(seedPositions.Length - 1) * (float)Math.PI);
				if (AbstractCob.dead)
				{
					num13 *= 0.5f;
				}
				sLeaser.sprites[SeedSprite(n, 0)].isVisible = true;
				sLeaser.sprites[SeedSprite(n, 1)].isVisible = seedsPopped[n];
				sLeaser.sprites[SeedSprite(n, 2)].isVisible = true;
				sLeaser.sprites[SeedSprite(n, 0)].scale = (seedsPopped[n] ? num13 : 0.35f);
				sLeaser.sprites[SeedSprite(n, 0)].x = vector13.x - camPos.x;
				sLeaser.sprites[SeedSprite(n, 0)].y = vector13.y - camPos.y;
				Vector2 vector14 = default(Vector2);
				if (seedsPopped[n])
				{
					vector14 = vector12 * Mathf.Pow(Mathf.Abs(seedPositions[n].x), Custom.LerpMap(num13, 1f, 2f, 1f, 0.5f)) * Mathf.Sign(seedPositions[n].x) * 3.5f * num13;
					if (!AbstractCob.dead)
					{
						sLeaser.sprites[SeedSprite(n, 2)].element = Futile.atlasManager.GetElementWithName("tinyStar");
					}
					sLeaser.sprites[SeedSprite(n, 2)].rotation = Custom.VecToDeg(vector11);
					sLeaser.sprites[SeedSprite(n, 2)].scaleX = Mathf.Pow(1f - Mathf.Abs(seedPositions[n].x), 0.2f);
				}
				sLeaser.sprites[SeedSprite(n, 1)].x = vector13.x + vector14.x * 0.35f - camPos.x;
				sLeaser.sprites[SeedSprite(n, 1)].y = vector13.y + vector14.y * 0.35f - camPos.y;
				sLeaser.sprites[SeedSprite(n, 1)].scale = (seedsPopped[n] ? num13 : 0.4f) * 0.5f;
				sLeaser.sprites[SeedSprite(n, 2)].x = vector13.x + vector14.x - camPos.x;
				sLeaser.sprites[SeedSprite(n, 2)].y = vector13.y + vector14.y - camPos.y;
			}
		}
		for (int num14 = 0; num14 < leaves.GetLength(0); num14++)
		{
			Vector2 vector15 = Vector2.Lerp(leaves[num14, 1], leaves[num14, 0], timeStacker);
			sLeaser.sprites[LeafSprite(num14)].x = vector2.x - camPos.x;
			sLeaser.sprites[LeafSprite(num14)].y = vector2.y - camPos.y;
			sLeaser.sprites[LeafSprite(num14)].rotation = Custom.AimFromOneVectorToAnother(vector2, vector15);
			sLeaser.sprites[LeafSprite(num14)].scaleY = Vector2.Distance(vector2, vector15) / 26f;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[StalkSprite(0)].color = palette.blackColor;
		StoredBlackColor = palette.blackColor;
		Color b = (StoredPlantColor = palette.texture.GetPixel(0, 5));
		for (int i = 0; i < (sLeaser.sprites[StalkSprite(1)] as TriangleMesh).verticeColors.Length; i++)
		{
			float num = (float)i / (float)((sLeaser.sprites[StalkSprite(1)] as TriangleMesh).verticeColors.Length - 1);
			(sLeaser.sprites[StalkSprite(1)] as TriangleMesh).verticeColors[i] = Color.Lerp(palette.blackColor, b, 0.4f + Mathf.Pow(1f - num, 0.5f) * 0.4f);
		}
		yellowColor = Color.Lerp(new Color(0.9f, 0.83f, 0.5f), palette.blackColor, AbstractCob.dead ? (0.95f + 0.5f * rCam.PaletteDarkness()) : (0.18f + 0.7f * rCam.PaletteDarkness()));
		for (int j = 0; j < 2; j++)
		{
			for (int k = 0; k < (sLeaser.sprites[ShellSprite(j)] as TriangleMesh).verticeColors.Length; k++)
			{
				float f = 1f - (float)k / (float)((sLeaser.sprites[ShellSprite(j)] as TriangleMesh).verticeColors.Length - 1);
				(sLeaser.sprites[ShellSprite(j)] as TriangleMesh).verticeColors[k] = Color.Lerp(palette.blackColor, new Color(1f, 0f, 0f), Mathf.Pow(f, 2.5f) * 0.4f);
			}
		}
		sLeaser.sprites[CobSprite].color = yellowColor;
		Color color = yellowColor + new Color(0.3f, 0.3f, 0.3f) * Mathf.Lerp(1f, 0.15f, rCam.PaletteDarkness());
		if (AbstractCob.dead)
		{
			color = Color.Lerp(yellowColor, b, 0.75f);
		}
		for (int l = 0; l < seedPositions.Length; l++)
		{
			sLeaser.sprites[SeedSprite(l, 0)].color = yellowColor;
			sLeaser.sprites[SeedSprite(l, 1)].color = color;
			sLeaser.sprites[SeedSprite(l, 2)].color = Color.Lerp(new Color(1f, 0f, 0f), palette.blackColor, AbstractCob.dead ? 0.6f : 0.3f);
		}
		for (int m = 0; m < leaves.GetLength(0); m++)
		{
			sLeaser.sprites[LeafSprite(m)].color = palette.blackColor;
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		return base.bodyChunks[segment].pos;
	}

	public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
	{
		base.bodyChunks[pos.prevSegment].vel += momentum / base.bodyChunks[pos.prevSegment].mass * (1f - pos.distanceToNext);
		base.bodyChunks[pos.prevSegment + 1].vel += momentum / base.bodyChunks[pos.prevSegment + 1].mass * pos.distanceToNext;
	}

	public void spawnUtilityFoods()
	{
		if (ModManager.MSC && !AbstractCob.spawnedUtility)
		{
			AbstractCob.opened = true;
			AbstractCob.dead = true;
			AbstractCob.minCycles = 1;
			AbstractCob.maxCycles = 3;
			AbstractCob.Consume();
			canBeHitByWeapons = false;
			base.bodyChunks[0].vel += Custom.RNV() * 2f;
			for (int i = 0; i < 4; i++)
			{
				AbstractConsumable abstractConsumable = new AbstractConsumable(room.world, MoreSlugcatsEnums.AbstractObjectType.Seed, null, room.GetWorldCoordinate(placedPos), room.game.GetNewID(), -1, -1, null);
				room.abstractRoom.AddEntity(abstractConsumable);
				abstractConsumable.pos = room.GetWorldCoordinate(placedPos);
				abstractConsumable.RealizeInRoom();
				abstractConsumable.realizedObject.firstChunk.HardSetPosition(Vector2.Lerp(base.bodyChunks[0].pos, base.bodyChunks[1].pos, (float)i / 5f));
			}
			AbstractCob.spawnedUtility = true;
		}
	}

	public void FreezingPaletteUpdate(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		float num = Mathf.InverseLerp(-1.2f, 2.4f, room.roomSettings.RumbleIntensity);
		Color color = Color.Lerp(StoredBlackColor, Color.white, Mathf.InverseLerp(0.4f, 2.6f, freezingCounter * num));
		sLeaser.sprites[StalkSprite(0)].color = color;
		Color a = Color.Lerp(Color.blue, StoredPlantColor, 0.3f);
		a = Color.Lerp(a, color, Mathf.InverseLerp(0.4f, 1f, freezingCounter) / 2f);
		Color b = Color.Lerp(StoredPlantColor, a, freezingCounter);
		for (int i = 0; i < (sLeaser.sprites[StalkSprite(1)] as TriangleMesh).verticeColors.Length; i++)
		{
			float num2 = (float)i / (float)((sLeaser.sprites[StalkSprite(1)] as TriangleMesh).verticeColors.Length - 1);
			(sLeaser.sprites[StalkSprite(1)] as TriangleMesh).verticeColors[i] = Color.Lerp(color, b, 0.4f + Mathf.Pow(1f - num2, 0.5f) * 0.4f);
		}
		yellowColor = Color.Lerp(new Color(0.9f, 0.83f, 0.5f), color, (!AbstractCob.dead) ? (0.18f + 0.7f * rCam.PaletteDarkness()) : (0.95f + 0.5f * rCam.PaletteDarkness()));
		for (int j = 0; j < 2; j++)
		{
			for (int k = 0; k < (sLeaser.sprites[ShellSprite(j)] as TriangleMesh).verticeColors.Length; k++)
			{
				float f = 1f - (float)k / (float)((sLeaser.sprites[ShellSprite(j)] as TriangleMesh).verticeColors.Length - 1);
				(sLeaser.sprites[ShellSprite(j)] as TriangleMesh).verticeColors[k] = Color.Lerp(color, b, Mathf.Pow(f, 2.5f) * 0.4f);
			}
		}
		sLeaser.sprites[CobSprite].color = yellowColor;
		Color color2 = yellowColor + new Color(0.3f, 0.3f, 0.3f) * Mathf.Lerp(1f, 0.15f, rCam.PaletteDarkness());
		if (AbstractCob.dead)
		{
			color2 = Color.Lerp(yellowColor, b, 0.75f);
		}
		for (int l = 0; l < seedPositions.Length; l++)
		{
			sLeaser.sprites[SeedSprite(l, 0)].color = yellowColor;
			sLeaser.sprites[SeedSprite(l, 1)].color = color2;
			sLeaser.sprites[SeedSprite(l, 2)].color = Color.Lerp(new Color(1f, 0f, 0f), color, (!AbstractCob.dead) ? 0.3f : 0.6f);
		}
		for (int m = 0; m < leaves.GetLength(0); m++)
		{
			sLeaser.sprites[LeafSprite(m)].color = color;
		}
	}
}
