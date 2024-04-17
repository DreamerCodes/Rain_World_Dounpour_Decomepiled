using System.Globalization;
using MoreSlugcats;
using RWCustom;
using Smoke;
using UnityEngine;

public class BubbleGrass : PlayerCarryableItem, IDrawable
{
	public class AbstractBubbleGrass : AbstractConsumable
	{
		public float oxygenLeft;

		public AbstractBubbleGrass(World world, PhysicalObject obj, WorldCoordinate pos, EntityID ID, float oxygen, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData)
			: base(world, AbstractObjectType.BubbleGrass, obj, pos, ID, originRoom, placedObjectIndex, consumableData)
		{
			oxygenLeft = oxygen;
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}", ID.ToString(), type.ToString(), pos.SaveToString(), originRoom, placedObjectIndex, oxygenLeft);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	public class Part
	{
		public BubbleGrass owner;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float rad;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public Part(BubbleGrass owner)
		{
			this.owner = owner;
			pos = owner.firstChunk.pos;
			lastPos = owner.firstChunk.pos;
			vel *= 0f;
		}

		public void Update()
		{
			lastPos = pos;
			pos += vel;
			if (owner.room.PointSubmerged(pos))
			{
				vel *= 0.7f;
			}
			else
			{
				vel *= 0.95f;
			}
			if (!owner.growPos.HasValue)
			{
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, rad, new IntVector2(0, 0), owner.firstChunk.goThroughFloors);
				cd = SharedPhysics.VerticalCollision(owner.room, cd);
				cd = SharedPhysics.HorizontalCollision(owner.room, cd);
				pos = cd.pos;
				vel = cd.vel;
			}
		}

		public void Reset()
		{
			pos = owner.firstChunk.pos + Custom.RNV() * Random.value;
			lastPos = pos;
			vel *= 0f;
		}
	}

	public Part[] stalk;

	public Part[,] lumps;

	private Vector2? growPos;

	public Color explodeColor = new Color(1f, 0.4f, 0.3f);

	public int[] lumpConnections;

	public Vector2[] lumpDirs;

	public float[] lumpLengths;

	public float swallowed;

	private float hover;

	private Vector2 windDir;

	public float oxygen;

	public float lastOxygen;

	private Color blackColor;

	public int StalkSprite => 0;

	public int TotalSprites => 1 + lumps.GetLength(0) * 4;

	private AbstractBubbleGrass AbstrBubbleGrass => abstractPhysicalObject as AbstractBubbleGrass;

	public int LumpSprite(int l, int p)
	{
		return 1 + l * 4 + p;
	}

	public BubbleGrass(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 2f, 0.07f);
		bodyChunkConnections = new BodyChunkConnection[0];
		oxygen = AbstrBubbleGrass.oxygenLeft;
		lastOxygen = oxygen;
		base.airFriction = 0.999f;
		base.gravity = 0.7f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		collisionLayer = 2;
		base.waterFriction = 0.98f;
		base.buoyancy = 1.8f;
		base.firstChunk.loudness = 0f;
		windDir = Custom.RNV();
		stalk = new Part[6];
		for (int i = 0; i < stalk.Length; i++)
		{
			stalk[i] = new Part(this);
			stalk[i].rad = 1f;
		}
		Random.State state = Random.state;
		Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		lumps = new Part[Random.Range(6, 10), 2];
		lumpConnections = new int[lumps.Length];
		lumpLengths = new float[lumps.Length];
		lumpDirs = new Vector2[lumps.Length];
		float num = Mathf.Lerp(20f / (float)lumps.GetLength(0), 4f, 0.75f);
		for (int j = 0; j < lumps.GetLength(0); j++)
		{
			lumpConnections[j] = 0;
			lumpLengths[j] = Mathf.Lerp(2f, 10f, Random.value);
			if (j > 4)
			{
				lumpConnections[j] = Random.Range(0, Random.Range(0, 4));
			}
			lumpDirs[j] = Custom.RNV() * Random.value;
			lumps[j, 0] = new Part(this);
			lumps[j, 0].rad = Mathf.Clamp(num * Mathf.Lerp(0.75f, 1.25f, Random.value), 3f, 8f);
			lumps[j, 1] = new Part(this);
			lumps[j, 1].rad = 1f;
		}
		Random.state = state;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && room.game.GetArenaGameSession.counter < 10)
		{
			bool flag = false;
			for (int i = 1; i < 5; i++)
			{
				if (room.GetTile(abstractPhysicalObject.pos.Tile + new IntVector2(0, -i)).Solid)
				{
					if (!room.GetTile(abstractPhysicalObject.pos.Tile + new IntVector2(0, -i + 2)).Solid)
					{
						IntVector2 pos = abstractPhysicalObject.pos.Tile + new IntVector2(0, -i + 2);
						growPos = room.MiddleOfTile(pos) + new Vector2(0f, -30f);
						base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos) + new Vector2(0f, -10f));
						flag = true;
						break;
					}
					if (!room.GetTile(abstractPhysicalObject.pos.Tile + new IntVector2(0, -i + 1)).Solid)
					{
						_ = abstractPhysicalObject.pos.Tile + new IntVector2(0, -i + 1);
						base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			}
		}
		else if (!AbstrBubbleGrass.isConsumed && AbstrBubbleGrass.placedObjectIndex >= 0 && AbstrBubbleGrass.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			IntVector2 tilePosition = room.GetTilePosition(placeRoom.roomSettings.placedObjects[AbstrBubbleGrass.placedObjectIndex].pos);
			for (int j = 0; j < 4; j++)
			{
				if (room.GetTile(tilePosition + new IntVector2(0, -j - 1)).Solid)
				{
					tilePosition += new IntVector2(0, -j);
					break;
				}
			}
			growPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, -10f);
			base.firstChunk.HardSetPosition(growPos.Value + new Vector2(0f, 20f));
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}
		ResetParts();
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetParts();
	}

	public void ResetParts()
	{
		for (int i = 0; i < stalk.Length; i++)
		{
			stalk[i].Reset();
		}
		for (int j = 0; j < lumps.GetLength(0); j++)
		{
			lumps[j, 0].Reset();
			lumps[j, 1].Reset();
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (!AbstrBubbleGrass.isConsumed)
		{
			AbstrBubbleGrass.Consume();
		}
		if (growPos.HasValue)
		{
			growPos = null;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastOxygen = oxygen;
		oxygen = AbstrBubbleGrass.oxygenLeft;
		base.gravity = Mathf.Lerp(0.7f, 0.3f, AbstrBubbleGrass.oxygenLeft);
		if (Random.value < base.firstChunk.submersion)
		{
			hover = Mathf.Max(hover, Random.value);
		}
		if (hover > 0f)
		{
			hover = Mathf.Max(0f, hover - 0.004f);
			base.firstChunk.vel.y += 0.65f * hover * oxygen;
			base.firstChunk.vel += 0.2f * windDir * hover * oxygen;
			windDir = (windDir + Custom.RNV() * 0.4f * Random.value).normalized;
		}
		if (room != null && AbstrBubbleGrass.oxygenLeft > 0f && ((ModManager.MSC && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer) || (base.Submersion >= 0.2f && room.waterObject.WaterIsLethal)))
		{
			AbstrBubbleGrass.oxygenLeft -= 0.04f;
			if (Random.value < 0.1f)
			{
				room.AddObject(new Smolder(room, base.firstChunk.pos, base.firstChunk, null));
			}
		}
		if (base.firstChunk.submersion > 0.9f)
		{
			bool flag = true;
			if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).animation == Player.AnimationIndex.SurfaceSwim && (grabbedBy[0].grabber as Player).airInLungs > 0.5f)
			{
				flag = false;
			}
			if (flag)
			{
				AbstrBubbleGrass.oxygenLeft = Mathf.Max(0f, AbstrBubbleGrass.oxygenLeft - 0.0009090909f);
				if (Random.value < Mathf.InverseLerp(0f, 0.3f, oxygen))
				{
					Bubble bubble = new Bubble(base.firstChunk.pos + Custom.RNV() * Random.value * 4f, Custom.RNV() * Mathf.Lerp(6f, 16f, Random.value) * Mathf.InverseLerp(0f, 0.45f, oxygen), bottomBubble: false, fakeWaterBubble: false);
					room.AddObject(bubble);
					bubble.age = 600 - Random.Range(20, Random.Range(30, 80));
					for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
					{
						if (room.abstractRoom.creatures[i].realizedCreature == null)
						{
							continue;
						}
						if (room.abstractRoom.creatures[i].realizedCreature is Player && Custom.DistLess(base.firstChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, 40f))
						{
							(room.abstractRoom.creatures[i].realizedCreature as Player).airInLungs = 1f;
						}
						else if (room.abstractRoom.creatures[i].realizedCreature is AirBreatherCreature && Custom.DistLess(base.firstChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, 40f))
						{
							(room.abstractRoom.creatures[i].realizedCreature as AirBreatherCreature).lungs = Mathf.Min(1f, (room.abstractRoom.creatures[i].realizedCreature as AirBreatherCreature).lungs + 1f / 21f);
						}
						else if (room.abstractRoom.creatures[i].realizedCreature is Leech && !room.abstractRoom.creatures[i].realizedCreature.dead && Custom.DistLess(base.firstChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, 70f))
						{
							float num = Mathf.InverseLerp(70f, 40f, Vector2.Distance(base.firstChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos)) * room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.submersion;
							if (Random.value < 0.007f * num)
							{
								room.abstractRoom.creatures[i].realizedCreature.Stun(16);
							}
							if (room.abstractRoom.creatures[i].realizedCreature.Consious && room.abstractRoom.creatures[i].realizedCreature.grasps[0] == null)
							{
								room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.vel += Custom.DirVec(base.firstChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos) * num * Random.value * 12f;
							}
						}
					}
				}
			}
		}
		for (int j = 0; j < stalk.Length; j++)
		{
			stalk[j].Update();
			if (!growPos.HasValue)
			{
				stalk[j].vel.y -= Mathf.InverseLerp(0f, stalk.Length - 1, j) * 0.4f;
			}
		}
		for (int k = 0; k < lumps.GetLength(0); k++)
		{
			lumps[k, 0].Update();
			lumps[k, 1].Update();
			Vector2 vector = Custom.DirVec(stalk[lumpConnections[k] + 1].pos, stalk[lumpConnections[k]].pos);
			lumps[k, 1].vel += vector * 0.1f;
			lumps[k, 0].vel -= vector * 0.05f;
			lumps[k, 0].vel += Custom.RotateAroundOrigo(lumpDirs[k], Custom.VecToDeg(vector)) * 0.6f;
			lumps[k, 1].vel -= Custom.RotateAroundOrigo(lumpDirs[k], Custom.VecToDeg(vector)) * 0.1f;
			stalk[lumpConnections[k]].vel -= Custom.RotateAroundOrigo(lumpDirs[k], Custom.VecToDeg(vector)) * 0.1f;
			float num2 = Mathf.InverseLerp(k, k + 1, oxygen * (float)lumps.GetLength(0));
			if (room.PointSubmerged(lumps[k, 0].pos))
			{
				lumps[k, 0].vel *= 0.6f;
				lumps[k, 0].vel.y += 1f + num2;
			}
			else
			{
				lumps[k, 0].vel *= 0.95f;
				lumps[k, 0].vel.y += Mathf.Lerp(-0.7f, Mathf.Lerp(0.1f, 0.6f, hover), num2);
			}
		}
		for (int l = 0; l < stalk.Length; l++)
		{
			ConnectStalkSegment(l);
		}
		for (int num3 = stalk.Length - 1; num3 >= 0; num3--)
		{
			ConnectStalkSegment(num3);
		}
		for (int m = 0; m < lumps.GetLength(0); m++)
		{
			ConnectLump(m);
		}
		for (int n = 0; n < stalk.Length; n++)
		{
			if (n > 1)
			{
				Vector2 vector2 = Custom.DirVec(stalk[n].pos, stalk[n - 2].pos);
				stalk[n].vel -= vector2 * 8.5f;
				stalk[n - 2].vel += vector2 * 8.5f;
			}
		}
		for (int num4 = 0; num4 < stalk.Length; num4++)
		{
			ConnectStalkSegment(num4);
		}
		for (int num5 = stalk.Length - 1; num5 >= 0; num5--)
		{
			ConnectStalkSegment(num5);
		}
		for (int num6 = 0; num6 < lumps.GetLength(0); num6++)
		{
			ConnectLump(num6);
		}
		if (growPos.HasValue && !Custom.DistLess(base.firstChunk.pos, growPos.Value, 100f))
		{
			growPos = null;
		}
		if (growPos.HasValue)
		{
			stalk[stalk.Length - 1].pos = growPos.Value + new Vector2(0f, -7f);
			stalk[stalk.Length - 1].vel *= 0f;
			base.firstChunk.vel.y += base.gravity;
			base.firstChunk.vel = Vector2.Lerp(base.firstChunk.vel, (growPos.Value + new Vector2(0f, 20f) - base.firstChunk.pos) / 20f, 0.2f);
			if (!Custom.DistLess(base.firstChunk.pos, growPos.Value, 50f))
			{
				base.firstChunk.pos = growPos.Value + Custom.DirVec(growPos.Value, base.firstChunk.pos) * 50f;
			}
			if (grabbedBy.Count > 0)
			{
				growPos = null;
			}
		}
		bool flag2 = false;
		if (grabbedBy.Count > 0)
		{
			stalk[0].vel += base.firstChunk.pos - stalk[3].pos;
			stalk[0].pos = base.firstChunk.pos;
			hover = 1f;
			if (!AbstrBubbleGrass.isConsumed)
			{
				AbstrBubbleGrass.Consume();
			}
			if (growPos.HasValue)
			{
				growPos = null;
			}
			if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).swallowAndRegurgitateCounter > 50 && (grabbedBy[0].grabber as Player).objectInStomach == null && (grabbedBy[0].grabber as Player).input[0].pckp)
			{
				int num7 = -1;
				for (int num8 = 0; num8 < 2; num8++)
				{
					if ((grabbedBy[0].grabber as Player).grasps[num8] != null && (grabbedBy[0].grabber as Player).CanBeSwallowed((grabbedBy[0].grabber as Player).grasps[num8].grabbed))
					{
						num7 = num8;
						break;
					}
				}
				if (num7 > -1 && (grabbedBy[0].grabber as Player).grasps[num7] != null && (grabbedBy[0].grabber as Player).grasps[num7].grabbed == this)
				{
					flag2 = true;
				}
			}
		}
		swallowed = Custom.LerpAndTick(swallowed, flag2 ? 1f : 0f, 0.05f, 0.05f);
	}

	private void ConnectStalkSegment(int i)
	{
		float num = 2f * (1f - swallowed);
		if (i == 0)
		{
			Vector2 vector = Custom.DirVec(stalk[i].pos, base.firstChunk.pos);
			float num2 = Vector2.Distance(stalk[i].pos, base.firstChunk.pos);
			stalk[i].pos -= (num - num2) * vector * 0.9f;
			stalk[i].vel -= (num - num2) * vector * 0.9f;
			base.firstChunk.pos += (num - num2) * vector * 0.1f;
			base.firstChunk.vel += (num - num2) * vector * 0.1f;
		}
		if (i > 0)
		{
			Vector2 vector2 = Custom.DirVec(stalk[i].pos, stalk[i - 1].pos);
			float num3 = Vector2.Distance(stalk[i].pos, stalk[i - 1].pos);
			stalk[i].pos -= (num - num3) * vector2 * 0.5f;
			stalk[i].vel -= (num - num3) * vector2 * 0.5f;
			stalk[i - 1].pos += (num - num3) * vector2 * 0.5f;
			stalk[i - 1].vel += (num - num3) * vector2 * 0.5f;
		}
	}

	private void ConnectLump(int i)
	{
		int num = lumpConnections[i];
		float num2 = lumpLengths[i] * (1f - swallowed);
		Vector2 vector = Custom.DirVec(lumps[i, 1].pos, stalk[num].pos);
		float num3 = Vector2.Distance(lumps[i, 1].pos, stalk[num].pos);
		lumps[i, 1].pos -= (num2 - num3) * vector * 0.5f;
		lumps[i, 1].vel -= (num2 - num3) * vector * 0.5f;
		stalk[num].pos += (num2 - num3) * vector * 0.5f;
		stalk[num].vel += (num2 - num3) * vector * 0.5f;
		vector = Custom.DirVec(lumps[i, 0].pos, lumps[i, 1].pos);
		num3 = Vector2.Distance(lumps[i, 0].pos, lumps[i, 1].pos);
		lumps[i, 0].pos -= (num2 - num3) * vector * 0.5f;
		lumps[i, 0].vel -= (num2 - num3) * vector * 0.5f;
		lumps[i, 1].pos += (num2 - num3) * vector * 0.5f;
		lumps[i, 1].vel += (num2 - num3) * vector * 0.5f;
		num2 = lumpLengths[i] * 2f;
		num3 = Vector2.Distance(lumps[i, 0].pos, base.firstChunk.pos);
		if (num3 > num2)
		{
			vector = Custom.DirVec(lumps[i, 0].pos, base.firstChunk.pos);
			lumps[i, 0].pos -= (num2 - num3) * vector * 0.8f;
			lumps[i, 0].vel -= (num2 - num3) * vector * 0.8f;
			base.firstChunk.pos += (num2 - num3) * vector * 0.2f;
			base.firstChunk.vel += (num2 - num3) * vector * 0.2f;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[StalkSprite] = TriangleMesh.MakeLongMesh(stalk.Length, pointyTip: false, customColor: true);
		for (int i = 0; i < lumps.GetLength(0); i++)
		{
			sLeaser.sprites[LumpSprite(i, 1)] = new FSprite("Circle20");
			sLeaser.sprites[LumpSprite(i, 2)] = new FSprite("Circle20");
			sLeaser.sprites[LumpSprite(i, 0)] = TriangleMesh.MakeLongMesh(4, pointyTip: false, customColor: true);
			sLeaser.sprites[LumpSprite(i, 3)] = new FSprite("Circle20");
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(stalk[0].lastPos, stalk[0].pos, timeStacker);
		float num = 1.2f;
		for (int i = 0; i < stalk.Length; i++)
		{
			Vector2 vector2 = Vector2.Lerp(stalk[i].lastPos, stalk[i].pos, timeStacker);
			Vector2 normalized = (vector2 - vector).normalized;
			Vector2 vector3 = Custom.PerpendicularVector(normalized);
			float num2 = Vector2.Distance(vector2, vector) / 5f;
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num + normalized * num2 - camPos);
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num + normalized * num2 - camPos);
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num - normalized * num2 - camPos);
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num - normalized * num2 - camPos);
			vector = vector2;
		}
		float num3 = Mathf.Lerp(lastOxygen, oxygen, timeStacker);
		if (lastOxygen != oxygen)
		{
			UpdateLumpColors(sLeaser);
		}
		for (int j = 0; j < lumps.GetLength(0); j++)
		{
			Vector2 p = Vector2.Lerp(lumps[j, 0].lastPos, lumps[j, 0].pos, timeStacker);
			Vector2 p2 = Vector2.Lerp(stalk[lumpConnections[j]].lastPos, stalk[lumpConnections[j]].pos, timeStacker);
			float num4 = Mathf.InverseLerp(j, j + 1, num3 * (float)lumps.GetLength(0));
			float num5 = Mathf.Lerp(2f, lumps[j, 0].rad * num4, num4);
			sLeaser.sprites[LumpSprite(j, 1)].x = p.x - camPos.x;
			sLeaser.sprites[LumpSprite(j, 1)].y = p.y - camPos.y;
			sLeaser.sprites[LumpSprite(j, 1)].rotation = Custom.AimFromOneVectorToAnother(p, p2);
			sLeaser.sprites[LumpSprite(j, 1)].scale = num5 / 10f;
			sLeaser.sprites[LumpSprite(j, 2)].x = p.x - camPos.x;
			sLeaser.sprites[LumpSprite(j, 2)].y = p.y - camPos.y;
			sLeaser.sprites[LumpSprite(j, 2)].rotation = Custom.AimFromOneVectorToAnother(p, p2);
			sLeaser.sprites[LumpSprite(j, 2)].scale = (num5 - 1.5f) / 10f;
			sLeaser.sprites[LumpSprite(j, 3)].x = p.x - camPos.x - num5 * 0.4f;
			sLeaser.sprites[LumpSprite(j, 3)].y = p.y - camPos.y + num5 * 0.4f;
			sLeaser.sprites[LumpSprite(j, 3)].alpha = 0.3f * num4;
			sLeaser.sprites[LumpSprite(j, 3)].scale = num5 / 20f;
			vector = Vector2.Lerp(stalk[1].lastPos, stalk[1].pos, timeStacker);
			for (int k = 0; k < 4; k++)
			{
				Vector2 vector4 = k switch
				{
					0 => Vector2.Lerp(stalk[1].lastPos, stalk[1].pos, timeStacker), 
					1 => Vector2.Lerp(stalk[0].lastPos, stalk[0].pos, timeStacker), 
					2 => Vector2.Lerp(lumps[j, 1].lastPos, lumps[j, 1].pos, timeStacker), 
					_ => Vector2.Lerp(lumps[j, 0].lastPos, lumps[j, 0].pos, timeStacker), 
				};
				Vector2 normalized2 = (vector4 - vector).normalized;
				Vector2 vector5 = Custom.PerpendicularVector(normalized2);
				float num6 = Vector2.Distance(vector4, vector) / 5f;
				(sLeaser.sprites[LumpSprite(j, 0)] as TriangleMesh).MoveVertice(k * 4, vector - vector5 * 0.75f + normalized2 * num6 - camPos);
				(sLeaser.sprites[LumpSprite(j, 0)] as TriangleMesh).MoveVertice(k * 4 + 1, vector + vector5 * 0.75f + normalized2 * num6 - camPos);
				(sLeaser.sprites[LumpSprite(j, 0)] as TriangleMesh).MoveVertice(k * 4 + 2, vector4 - vector5 * 0.75f - normalized2 * num6 - camPos);
				(sLeaser.sprites[LumpSprite(j, 0)] as TriangleMesh).MoveVertice(k * 4 + 3, vector4 + vector5 * 0.75f - normalized2 * num6 - camPos);
				vector = vector4;
			}
		}
		if (blink > 0)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		else if (color == Color.white)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		color = Color.Lerp(Color.Lerp(palette.blackColor, new Color(0f, 1f, 0.4f), 0.4f), palette.fogColor, 0.2f);
		if (blink > 1 && Random.value < 0.5f)
		{
			color = new Color(1f, 1f, 1f);
		}
		for (int i = 0; i < (sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors.Length; i++)
		{
			(sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors[i] = StalkColor(Mathf.InverseLerp((sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors.Length - 1, 0f, i));
		}
		UpdateLumpColors(sLeaser);
	}

	private void UpdateLumpColors(RoomCamera.SpriteLeaser sLeaser)
	{
		for (int i = 0; i < lumps.GetLength(0); i++)
		{
			float num = Mathf.InverseLerp(i, i + 1, oxygen * (float)lumps.GetLength(0));
			Color color = Color.Lerp(blackColor, base.color, 0.2f + 0.8f * num);
			sLeaser.sprites[LumpSprite(i, 1)].color = color;
			sLeaser.sprites[LumpSprite(i, 2)].color = Color.Lerp(color, Color.Lerp(blackColor, new Color(0f, 0f, 1f), 0.4f), 0.6f);
			Color a = StalkColor(Mathf.InverseLerp(stalk.Length - 1, 0f, lumpConnections[i]));
			for (int j = 0; j < (sLeaser.sprites[LumpSprite(i, 0)] as TriangleMesh).verticeColors.Length; j++)
			{
				(sLeaser.sprites[LumpSprite(i, 0)] as TriangleMesh).verticeColors[j] = Color.Lerp(a, color, Mathf.InverseLerp(0f, (sLeaser.sprites[LumpSprite(i, 0)] as TriangleMesh).verticeColors.Length - 1, j));
			}
		}
	}

	private Color StalkColor(float f)
	{
		return Color.Lerp(blackColor, color, Mathf.Pow(f, 0.5f) * 0.8f);
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
}
