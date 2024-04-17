using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class FlyLure : PlayerCarryableItem, IDrawable
{
	public class Part
	{
		public FlyLure owner;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float rad;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public Part(FlyLure owner)
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
				vel *= 0.4f;
				vel.y += 0.1f;
			}
			else
			{
				vel *= 0.88f;
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
			pos = owner.firstChunk.pos + Custom.RNV() * UnityEngine.Random.value;
			lastPos = pos;
			vel *= 0f;
		}
	}

	public Part[] stalk;

	public Part[] lumps;

	private Vector2? growPos;

	public Color blackColor;

	public int[] lumpConnections;

	public Vector2[] lumpDirs;

	public bool[] lumpsPopped;

	public float swallowed;

	public int StalkSprite => 0;

	public int TotalSprites => 1 + lumps.Length;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public int LumpSprite(int l)
	{
		return 1 + l;
	}

	public FlyLure(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.991f;
		base.gravity = 0.9f;
		bounce = 0f;
		surfaceFriction = 0.3f;
		collisionLayer = 2;
		base.waterFriction = 0.92f;
		base.buoyancy = 1.2f;
		base.firstChunk.loudness = 0.1f;
		stalk = new Part[6];
		for (int i = 0; i < stalk.Length; i++)
		{
			stalk[i] = new Part(this);
		}
		lumps = new Part[UnityEngine.Random.Range(6, 11)];
		lumpConnections = new int[lumps.Length];
		lumpDirs = new Vector2[lumps.Length];
		lumpsPopped = new bool[lumps.Length];
		float num = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		float p = Mathf.Lerp(0.9f, 1.5f, UnityEngine.Random.value);
		for (int j = 0; j < lumps.Length; j++)
		{
			lumpConnections[j] = j / 2;
			float f = (float)lumpConnections[j] / (float)(stalk.Length - 1);
			lumpDirs[j] = Custom.DegToVec(Mathf.Lerp(10f, 130f, Mathf.Pow(f, p)) * ((j % 2 == 0) ? (0f - num) : num));
			lumps[j] = new Part(this);
			lumps[j].rad = Custom.LerpMap(lumps.Length, 6f, 12f, 1.6f, 1.2f) + Mathf.Lerp(0.75f, 1.25f, UnityEngine.Random.value) * (float)j * Custom.LerpMap(lumps.Length, 6f, 12f, 0.4f, 0.1f, 0.8f) * ((j == lumps.Length - 1) ? 0.65f : 1f);
			if (lumps.Length % 2 == 1 && j == lumps.Length - 1)
			{
				lumps[j].rad *= 0.5f;
			}
		}
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
		else if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			IntVector2 tilePosition = room.GetTilePosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			growPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, -10f);
			base.firstChunk.HardSetPosition(growPos.Value + new Vector2(0f, 30f));
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
		for (int j = 0; j < lumps.Length; j++)
		{
			lumps[j].Reset();
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (!AbstrConsumable.isConsumed)
		{
			AbstrConsumable.Consume();
		}
		if (growPos.HasValue)
		{
			growPos = null;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (growPos.HasValue && !Custom.DistLess(base.firstChunk.pos, growPos.Value, 100f))
		{
			growPos = null;
		}
		if (grabbedBy.Count > 0)
		{
			if (!AbstrConsumable.isConsumed)
			{
				AbstrConsumable.Consume();
			}
			if (growPos.HasValue)
			{
				growPos = null;
			}
		}
		for (int i = 0; i < stalk.Length; i++)
		{
			stalk[i].Update();
			if (!growPos.HasValue)
			{
				stalk[i].vel.y -= Mathf.InverseLerp(0f, stalk.Length - 1, i) * 0.4f;
			}
		}
		for (int j = 0; j < lumps.Length; j++)
		{
			lumps[j].Update();
			Vector2 vector = ((lumpConnections[j] != 0) ? Custom.RotateAroundOrigo(lumpDirs[j], Custom.AimFromOneVectorToAnother(stalk[lumpConnections[j]].pos, stalk[lumpConnections[j] - 1].pos)) : Custom.RotateAroundOrigo(lumpDirs[j], Custom.AimFromOneVectorToAnother(stalk[lumpConnections[j] + 1].pos, stalk[lumpConnections[j]].pos)));
			lumps[j].vel += vector;
			stalk[lumpConnections[j]].vel -= vector;
			if (!growPos.HasValue)
			{
				lumps[j].vel.y -= 0.9f;
			}
		}
		for (int k = 0; k < stalk.Length; k++)
		{
			ConnectStalkSegment(k);
		}
		for (int num = stalk.Length - 1; num >= 0; num--)
		{
			ConnectStalkSegment(num);
		}
		for (int l = 0; l < lumps.Length; l++)
		{
			ConnectLump(l);
		}
		for (int m = 0; m < stalk.Length; m++)
		{
			if (m > 1)
			{
				Vector2 vector2 = Custom.DirVec(stalk[m].pos, stalk[m - 2].pos);
				stalk[m].vel -= vector2 * 8.5f;
				stalk[m - 2].vel += vector2 * 8.5f;
			}
		}
		for (int n = 0; n < stalk.Length; n++)
		{
			ConnectStalkSegment(n);
		}
		for (int num2 = stalk.Length - 1; num2 >= 0; num2--)
		{
			ConnectStalkSegment(num2);
		}
		for (int num3 = 0; num3 < lumps.Length; num3++)
		{
			ConnectLump(num3);
		}
		if (growPos.HasValue)
		{
			stalk[stalk.Length - 1].pos = growPos.Value + new Vector2(0f, -6f);
			stalk[stalk.Length - 1].vel *= 0f;
			base.firstChunk.vel.y += base.gravity;
			base.firstChunk.vel += (growPos.Value + new Vector2(0f, 30f) - base.firstChunk.pos) / 100f;
			if (!Custom.DistLess(base.firstChunk.pos, growPos.Value, 50f))
			{
				base.firstChunk.pos = growPos.Value + Custom.DirVec(growPos.Value, base.firstChunk.pos) * 50f;
			}
			if (grabbedBy.Count > 0)
			{
				growPos = null;
			}
		}
		bool flag = false;
		if (grabbedBy.Count > 0)
		{
			stalk[3].vel += base.firstChunk.pos - stalk[3].pos;
			stalk[3].pos = base.firstChunk.pos;
			if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).swallowAndRegurgitateCounter > 50 && (grabbedBy[0].grabber as Player).objectInStomach == null && (grabbedBy[0].grabber as Player).input[0].pckp)
			{
				int num4 = -1;
				for (int num5 = 0; num5 < 2; num5++)
				{
					if ((grabbedBy[0].grabber as Player).grasps[num5] != null && (grabbedBy[0].grabber as Player).CanBeSwallowed((grabbedBy[0].grabber as Player).grasps[num5].grabbed))
					{
						num4 = num5;
						break;
					}
				}
				if (num4 > -1 && (grabbedBy[0].grabber as Player).grasps[num4] != null && (grabbedBy[0].grabber as Player).grasps[num4].grabbed == this)
				{
					flag = true;
				}
			}
		}
		swallowed = Custom.LerpAndTick(swallowed, flag ? 1f : 0f, 0.05f, 0.05f);
		if (UnityEngine.Random.value < 1f / ((grabbedBy.Count == 0) ? 5f : 21f) && room.fliesRoomAi != null && room.fliesRoomAi.flies.Count > 0)
		{
			Fly fly = room.fliesRoomAi.flies[UnityEngine.Random.Range(0, room.fliesRoomAi.flies.Count)];
			if ((fly.AI.luredCounter == 0 || fly.AI.lure == null || Custom.DistLess(fly.firstChunk.pos, base.firstChunk.pos, Vector2.Distance(fly.firstChunk.pos, fly.AI.lure.firstChunk.pos))) && Custom.DistLess(fly.firstChunk.pos, base.firstChunk.pos, 800f) && room.VisualContact(fly.firstChunk.pos, base.firstChunk.pos))
			{
				fly.AI.lure = this;
				fly.AI.luredCounter = Math.Max(fly.AI.luredCounter, UnityEngine.Random.Range(5, 80));
			}
		}
	}

	private void ConnectStalkSegment(int i)
	{
		float num = 4f * (1f - swallowed);
		if (i == 3)
		{
			Vector2 vector = Custom.DirVec(stalk[i].pos, base.firstChunk.pos);
			float num2 = Vector2.Distance(stalk[i].pos, base.firstChunk.pos);
			stalk[i].pos -= (num - num2) * vector * 0.95f;
			stalk[i].vel -= (num - num2) * vector * 0.95f;
			base.firstChunk.pos += (num - num2) * vector * 0.05f;
			base.firstChunk.vel += (num - num2) * vector * 0.05f;
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
		float num2 = (lumps[i].rad * 4.5f + 1.8f) * (1f - swallowed);
		Vector2 vector = Custom.DirVec(lumps[i].pos, stalk[num].pos);
		float num3 = Vector2.Distance(lumps[i].pos, stalk[num].pos);
		lumps[i].pos -= (num2 - num3) * vector * 0.5f;
		lumps[i].vel -= (num2 - num3) * vector * 0.5f;
		stalk[num].pos += (num2 - num3) * vector * 0.5f;
		stalk[num].vel += (num2 - num3) * vector * 0.5f;
	}

	public override void PickedUp(Creature upPicker)
	{
		room.PlaySound(SoundID.Slugcat_Pick_Up_Fly_Lure, base.firstChunk);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[StalkSprite] = TriangleMesh.MakeLongMesh(stalk.Length, pointyTip: false, customColor: true);
		for (int i = 0; i < lumps.Length; i++)
		{
			sLeaser.sprites[LumpSprite(i)] = new FSprite("Circle20");
			sLeaser.sprites[LumpSprite(i)].scaleX = 0.1f;
			sLeaser.sprites[LumpSprite(i)].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < lumps.Length; i++)
		{
			Vector2 vector = Vector2.Lerp(lumps[i].lastPos, lumps[i].pos, timeStacker);
			Vector2 vector2 = Vector2.Lerp(stalk[lumpConnections[i]].lastPos, stalk[lumpConnections[i]].pos, timeStacker);
			sLeaser.sprites[LumpSprite(i)].x = vector.x - camPos.x;
			sLeaser.sprites[LumpSprite(i)].y = vector.y - camPos.y;
			sLeaser.sprites[LumpSprite(i)].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
			sLeaser.sprites[LumpSprite(i)].scaleY = Vector2.Distance(vector, vector2) / 20f;
		}
		Vector2 vector3 = Vector2.Lerp(stalk[0].lastPos, stalk[0].pos, timeStacker);
		vector3 += Custom.DirVec(Vector2.Lerp(stalk[1].lastPos, stalk[1].pos, timeStacker), vector3) * 4f;
		float num = 0.6f;
		for (int j = 0; j < stalk.Length; j++)
		{
			Vector2 vector4 = Vector2.Lerp(stalk[j].lastPos, stalk[j].pos, timeStacker);
			Vector2 normalized = (vector4 - vector3).normalized;
			Vector2 vector5 = Custom.PerpendicularVector(normalized);
			float num2 = Vector2.Distance(vector4, vector3) / 5f;
			if (j == 0)
			{
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * num - camPos);
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * num - camPos);
			}
			else
			{
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * num + normalized * num2 - camPos);
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * num + normalized * num2 - camPos);
			}
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector4 - vector5 * num - normalized * num2 - camPos);
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector4 + vector5 * num - normalized * num2 - camPos);
			vector3 = vector4;
		}
		if (blink > 0)
		{
			UpdateColor(sLeaser, blink > 4 && UnityEngine.Random.value < 0.5f);
		}
		else if (sLeaser.sprites[StalkSprite].color == base.blinkColor)
		{
			UpdateColor(sLeaser, blink: false);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		color = Color.Lerp(new Color(0.7f, 0.1f, 0f), palette.fogColor, 0.3f);
		UpdateColor(sLeaser, blink: false);
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

	public void UpdateColor(RoomCamera.SpriteLeaser sLeaser, bool blink)
	{
		if (blink)
		{
			sLeaser.sprites[StalkSprite].color = base.blinkColor;
			for (int i = 0; i < lumps.Length; i++)
			{
				sLeaser.sprites[LumpSprite(i)].color = base.blinkColor;
			}
			return;
		}
		for (int j = 0; j < (sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors.Length; j++)
		{
			(sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors[j] = StalkColor((float)j / (float)((sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors.Length - 1));
		}
		for (int k = 0; k < lumps.Length; k++)
		{
			sLeaser.sprites[LumpSprite(k)].color = StalkColor((float)lumpConnections[k] / (float)(stalk.Length - 1));
		}
	}

	public Color StalkColor(float f)
	{
		return Color.Lerp(color, blackColor, Mathf.InverseLerp(0.3f, 1f, f));
	}
}
