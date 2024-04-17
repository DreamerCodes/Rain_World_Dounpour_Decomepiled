using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class JellyFish : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float darkness;

	public float lastDarkness;

	public float tentaclesWithdrawn;

	public int bites = 3;

	public BodyChunk[] latchOnToBodyChunks;

	public Vector2[][,] tentacles;

	public bool anyTentaclePulled;

	public int electricCounter;

	public Creature thrownBy;

	private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public bool dead;

	public float deathDarken;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public AbstractCreature abstractCreature => (AbstractCreature)abstractPhysicalObject;

	public int HighLightSprite => tentacles.Length + 2;

	public int TotalSprites => tentacles.Length + 3;

	public bool Electric => electricCounter > 0;

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public int BodySprite(int part)
	{
		return tentacles.Length + part;
	}

	public int TentacleSprite(int t)
	{
		return t;
	}

	public JellyFish(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 9.5f, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.7f;
		collisionLayer = 1;
		base.waterFriction = 0.95f;
		base.buoyancy = 1.1f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(AbstrConsumable.originRoom * 100 + AbstrConsumable.placedObjectIndex);
		int num = UnityEngine.Random.Range(4, 8);
		tentacles = new Vector2[num][,];
		latchOnToBodyChunks = new BodyChunk[num];
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i] = new Vector2[UnityEngine.Random.Range(4, 17), 3];
		}
		UnityEngine.Random.state = state;
		if (ModManager.MMF && MMF.cfgVulnerableJellyfish.Value)
		{
			canBeHitByWeapons = true;
		}
	}

	public void ResetTentacles()
	{
		for (int i = 0; i < tentacles.Length; i++)
		{
			for (int j = 0; j < tentacles[i].GetLength(0); j++)
			{
				tentacles[i][j, 0] = base.firstChunk.pos + Custom.RNV();
				tentacles[i][j, 1] = tentacles[i][j, 0];
				tentacles[i][j, 2] *= 0f;
			}
		}
	}

	public void Tossed(Creature tossedBy)
	{
		if (!ModManager.MMF || !dead)
		{
			electricCounter = 120;
		}
		thrownBy = tossedBy;
		for (int i = 0; i < latchOnToBodyChunks.Length; i++)
		{
			latchOnToBodyChunks[i] = null;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (ModManager.MMF && dead)
		{
			electricCounter = 0;
		}
		if (Electric)
		{
			electricCounter--;
			if (electricCounter < 100)
			{
				thrownBy = null;
			}
		}
		base.CollideWithTerrain = grabbedBy.Count == 0;
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Futile.mousePosition) * 3f;
		}
		lastRotation = rotation;
		if (grabbedBy.Count > 0)
		{
			rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			rotation.y = Mathf.Abs(rotation.y);
		}
		if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		tentaclesWithdrawn = Custom.LerpAndTick(tentaclesWithdrawn, (bites == 3) ? Mathf.InverseLerp(0.5f, 0f, base.firstChunk.submersion) : 1f, 0.1f, 1f / 60f);
		if (!anyTentaclePulled)
		{
			rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * base.firstChunk.vel.x * ((base.firstChunk.ContactPoint.y < 0) ? 1f : 0.3f)).normalized;
			rotation = Vector3.Slerp(rotation, new Vector2(0f, 1f), (1f - 2f * Mathf.Abs(0.5f - base.firstChunk.submersion)) * 0.1f);
		}
		if (base.firstChunk.ContactPoint.y < 0)
		{
			base.firstChunk.vel.x *= 0.8f;
		}
		float num = Mathf.Lerp(10f, 1f, tentaclesWithdrawn);
		anyTentaclePulled = false;
		for (int i = 0; i < tentacles.Length; i++)
		{
			for (int j = 0; j < tentacles[i].GetLength(0); j++)
			{
				float t = (float)j / (float)(tentacles[i].GetLength(0) - 1);
				tentacles[i][j, 1] = tentacles[i][j, 0];
				tentacles[i][j, 0] += tentacles[i][j, 2];
				tentacles[i][j, 2] -= rotation * Mathf.InverseLerp(4f, 0f, j) * 0.8f;
				if (room.PointSubmerged(tentacles[i][j, 0]))
				{
					tentacles[i][j, 2] *= Custom.LerpMap(tentacles[i][j, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
					tentacles[i][j, 2] += Custom.RNV() * 0.2f;
					continue;
				}
				tentacles[i][j, 2] *= 0.999f;
				tentacles[i][j, 2].y -= room.gravity * 0.6f;
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(tentacles[i][j, 0], tentacles[i][j, 1], tentacles[i][j, 2], 1f, new IntVector2(0, 0), goThroughFloors: false);
				cd = SharedPhysics.HorizontalCollision(room, cd);
				cd = SharedPhysics.VerticalCollision(room, cd);
				cd = SharedPhysics.SlopesVertically(room, cd);
				tentacles[i][j, 0] = cd.pos;
				tentacles[i][j, 2] = cd.vel;
			}
			for (int k = 0; k < tentacles[i].GetLength(0); k++)
			{
				if (k > 0)
				{
					Vector2 normalized = (tentacles[i][k, 0] - tentacles[i][k - 1, 0]).normalized;
					float num2 = Vector2.Distance(tentacles[i][k, 0], tentacles[i][k - 1, 0]);
					tentacles[i][k, 0] += normalized * (num - num2) * 0.5f;
					tentacles[i][k, 2] += normalized * (num - num2) * 0.5f;
					tentacles[i][k - 1, 0] -= normalized * (num - num2) * 0.5f;
					tentacles[i][k - 1, 2] -= normalized * (num - num2) * 0.5f;
					if (k > 1)
					{
						normalized = (tentacles[i][k, 0] - tentacles[i][k - 2, 0]).normalized;
						tentacles[i][k, 2] += normalized * 0.2f;
						tentacles[i][k - 2, 2] -= normalized * 0.2f;
					}
				}
				else
				{
					tentacles[i][k, 0] = AttachPos(i, 1f);
					tentacles[i][k, 2] *= 0f;
				}
			}
			if (latchOnToBodyChunks[i] != null)
			{
				if (Electric || room.PointSubmerged(tentacles[i][tentacles[i].GetLength(0) - 1, 0]))
				{
					anyTentaclePulled = true;
					Vector2 normalized2 = (tentacles[i][tentacles[i].GetLength(0) - 1, 0] - latchOnToBodyChunks[i].pos).normalized;
					float num3 = Vector2.Distance(tentacles[i][tentacles[i].GetLength(0) - 1, 0], latchOnToBodyChunks[i].pos);
					tentacles[i][tentacles[i].GetLength(0) - 1, 0] += normalized2 * (latchOnToBodyChunks[i].rad * 0.5f - num3) * 0.5f;
					tentacles[i][tentacles[i].GetLength(0) - 1, 2] += normalized2 * (latchOnToBodyChunks[i].rad * 0.5f - num3) * 0.5f;
					bool flag = ((!ModManager.MSC) ? (room.FloatWaterLevel(tentacles[i][tentacles[i].GetLength(0) - 1, 0].x) > tentacles[i][tentacles[i].GetLength(0) - 1, 0].y + 30f) : room.PointSubmerged(new Vector2(tentacles[i][tentacles[i].GetLength(0) - 1, 0].x, tentacles[i][tentacles[i].GetLength(0) - 1, 0].y + 30f)));
					if (!Custom.DistLess(base.firstChunk.pos, latchOnToBodyChunks[i].pos, (float)tentacles[i].GetLength(0) * num * 1.8f))
					{
						latchOnToBodyChunks[i] = null;
						room.PlaySound(SoundID.Jelly_Fish_Tentacle_Release, tentacles[i][tentacles[i].GetLength(0) - 1, 0]);
					}
					else if ((Electric || UnityEngine.Random.value < 0.005f) && flag)
					{
						if (latchOnToBodyChunks[i].owner is Creature)
						{
							(latchOnToBodyChunks[i].owner as Creature).Stun(Mathf.RoundToInt(Custom.LerpMap(latchOnToBodyChunks[i].owner.TotalMass, 0.1f, 5f, Electric ? 75f : 50f, 0f)));
							if (ModManager.MSC && latchOnToBodyChunks[i].owner is Player && (latchOnToBodyChunks[i].owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
							{
								room.AddObject(new CreatureSpasmer(latchOnToBodyChunks[i].owner as Creature, allowDead: true, 80));
								(latchOnToBodyChunks[i].owner as Player).SaintStagger(520);
							}
							room.AddObject(new ShockWave(tentacles[i][tentacles[i].GetLength(0) - 1, 0], Mathf.Lerp(40f, 60f, UnityEngine.Random.value), 0.07f, 6));
							tentacles[i][tentacles[i].GetLength(0) - 1, 0] = Vector2.Lerp(tentacles[i][tentacles[i].GetLength(0) - 1, 0], base.firstChunk.pos, 0.2f);
						}
						room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, tentacles[i][tentacles[i].GetLength(0) - 1, 0]);
						latchOnToBodyChunks[i] = null;
					}
					else if (!Custom.DistLess(base.firstChunk.pos, latchOnToBodyChunks[i].pos, (float)tentacles[i].GetLength(0) * num * 1.4f))
					{
						normalized2 = (base.firstChunk.pos - latchOnToBodyChunks[i].pos).normalized;
						num3 = Vector2.Distance(base.firstChunk.pos, latchOnToBodyChunks[i].pos);
						float num4 = base.firstChunk.mass / (base.firstChunk.mass + latchOnToBodyChunks[i].mass);
						latchOnToBodyChunks[i].pos -= normalized2 * ((float)tentacles[i].GetLength(0) * num * 1.4f - num3) * num4;
						latchOnToBodyChunks[i].vel -= normalized2 * ((float)tentacles[i].GetLength(0) * num * 1.4f - num3) * num4;
						rotation = (rotation + normalized2 * Mathf.InverseLerp((float)tentacles[i].GetLength(0) * num * 0.4f, (float)tentacles[i].GetLength(0) * num * 2.4f, Vector2.Distance(base.firstChunk.pos, latchOnToBodyChunks[i].pos))).normalized;
					}
				}
				else
				{
					latchOnToBodyChunks[i] = null;
					room.PlaySound(SoundID.Jelly_Fish_Tentacle_Release, tentacles[i][tentacles[i].GetLength(0) - 1, 0]);
				}
			}
			if (latchOnToBodyChunks[i] != null || bites != 3 || !(UnityEngine.Random.value < 1f / (float)tentacles.Length) || (!Electric && !room.PointSubmerged(tentacles[i][tentacles[i].GetLength(0) - 1, 0])))
			{
				continue;
			}
			Vector2 vector = tentacles[i][tentacles[i].GetLength(0) - 1, 0];
			int num5 = 0;
			while (latchOnToBodyChunks[i] == null && num5 < room.abstractRoom.creatures.Count)
			{
				if (room.abstractRoom.creatures[num5].realizedCreature != null && (grabbedBy.Count == 0 || room.abstractRoom.creatures[num5].realizedCreature != grabbedBy[0].grabber) && room.abstractRoom.creatures[num5].realizedCreature.room == room)
				{
					int num6 = 0;
					while (latchOnToBodyChunks[i] == null && num6 < room.abstractRoom.creatures[num5].realizedCreature.bodyChunks.Length)
					{
						if (Custom.DistLess(room.abstractRoom.creatures[num5].realizedCreature.bodyChunks[num6].pos, vector, room.abstractRoom.creatures[num5].realizedCreature.bodyChunks[num6].rad))
						{
							latchOnToBodyChunks[i] = room.abstractRoom.creatures[num5].realizedCreature.bodyChunks[num6];
							room.PlaySound((room.abstractRoom.creatures[num5].realizedCreature is Player) ? SoundID.Jelly_Fish_Tentacle_Latch_On_Player : SoundID.Jelly_Fish_Tentacle_Latch_On_NPC, vector);
						}
						num6++;
					}
				}
				num5++;
			}
		}
		if (!AbstrConsumable.isConsumed && room.abstractRoom.index != AbstrConsumable.originRoom)
		{
			AbstrConsumable.Consume();
		}
	}

	public Vector2 AttachPos(int rag, float timeStacker)
	{
		return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) - (Vector2)Vector3.Slerp(lastRotation, rotation, timeStacker) * 7f;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			Vector2 pos = placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos;
			if (room.water)
			{
				pos.y = room.FloatWaterLevel(pos.x);
			}
			base.firstChunk.HardSetPosition(pos);
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			rotation = Custom.RNV();
			lastRotation = rotation;
		}
		ResetTentacles();
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!(otherObject is Creature) || otherObject == thrownBy || !Electric)
		{
			return;
		}
		if (!(otherObject is BigEel))
		{
			(otherObject as Creature).Violence(base.firstChunk, Custom.DirVec(base.firstChunk.pos, otherObject.bodyChunks[otherChunk].pos) * 5f, otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Electric, 0.1f, (otherObject is Player) ? 140f : (320f * Mathf.Lerp((otherObject as Creature).Template.baseStunResistance, 1f, 0.5f)));
			room.AddObject(new CreatureSpasmer(otherObject as Creature, allowDead: false, (otherObject as Creature).stun));
			if (ModManager.MSC && otherObject is Player && (otherObject as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				(otherObject as Player).SaintStagger(520);
			}
		}
		room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, base.firstChunk.pos);
		room.AddObject(new Explosion.ExplosionLight(base.firstChunk.pos, 200f, 1f, 4, new Color(0.7f, 1f, 1f)));
		if (electricCounter > 5)
		{
			for (int i = 0; i < 15; i++)
			{
				Vector2 vector = Custom.DegToVec(360f * UnityEngine.Random.value);
				room.AddObject(new MouseSpark(base.firstChunk.pos + vector * 9f, base.firstChunk.vel + vector * 36f * UnityEngine.Random.value, 20f, new Color(0.7f, 1f, 1f)));
			}
		}
		electricCounter = Math.Min(electricCounter, 5);
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetTentacles();
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[BodySprite(0)] = new FSprite("JellyFish0A");
		sLeaser.sprites[BodySprite(1)] = new FSprite("JellyFish0B");
		sLeaser.sprites[HighLightSprite] = new FSprite("JetFishEyeA");
		sLeaser.sprites[HighLightSprite].scaleX = 0.45f;
		sLeaser.sprites[HighLightSprite].scaleY = 0.8f;
		for (int i = 0; i < tentacles.Length; i++)
		{
			sLeaser.sprites[TentacleSprite(i)] = TriangleMesh.MakeLongMesh(tentacles[i].GetLength(0), pointyTip: false, customColor: true);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		if (ModManager.MMF && dead)
		{
			deathDarken = Mathf.Lerp(deathDarken, 1f, 0.025f);
		}
		else
		{
			deathDarken = 0f;
		}
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[BodySprite(i)].x = vector.x - camPos.x;
			sLeaser.sprites[BodySprite(i)].y = vector.y - camPos.y;
			sLeaser.sprites[BodySprite(i)].rotation = Custom.VecToDeg(vector2);
			sLeaser.sprites[BodySprite(i)].element = Futile.atlasManager.GetElementWithName("JellyFish" + Custom.IntClamp(3 - bites, 0, 2) + ((i == 0) ? "A" : "B"));
		}
		Vector2 p = vector + vector2 * 4f + Custom.DegToVec(-35f) * 3f;
		sLeaser.sprites[HighLightSprite].x = p.x - camPos.x;
		sLeaser.sprites[HighLightSprite].y = p.y - camPos.y;
		sLeaser.sprites[HighLightSprite].rotation = Custom.AimFromOneVectorToAnother(p, vector + vector2 * 9.5f);
		sLeaser.sprites[HighLightSprite].isVisible = bites == 3;
		for (int j = 0; j < tentacles.Length; j++)
		{
			float num = 0f;
			Vector2 vector3 = AttachPos(j, timeStacker);
			for (int k = 0; k < tentacles[j].GetLength(0); k++)
			{
				Vector2 vector4 = Vector2.Lerp(tentacles[j][k, 1], tentacles[j][k, 0], timeStacker);
				float num2 = 0.5f;
				Vector2 normalized = (vector3 - vector4).normalized;
				Vector2 vector5 = Custom.PerpendicularVector(normalized);
				float num3 = Vector2.Distance(vector3, vector4) / 5f;
				(sLeaser.sprites[TentacleSprite(j)] as TriangleMesh).MoveVertice(k * 4, vector3 - normalized * num3 - vector5 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[TentacleSprite(j)] as TriangleMesh).MoveVertice(k * 4 + 1, vector3 - normalized * num3 + vector5 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[TentacleSprite(j)] as TriangleMesh).MoveVertice(k * 4 + 2, vector4 + normalized * num3 - vector5 * num2 - camPos);
				(sLeaser.sprites[TentacleSprite(j)] as TriangleMesh).MoveVertice(k * 4 + 3, vector4 + normalized * num3 + vector5 * num2 - camPos);
				vector3 = vector4;
				num = num2;
			}
		}
		if (Electric)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		if (blink > 0 && UnityEngine.Random.value < 0.5f)
		{
			sLeaser.sprites[BodySprite(1)].color = base.blinkColor;
		}
		else
		{
			sLeaser.sprites[BodySprite(1)].color = Color.Lerp(color, Color.black, deathDarken * 0.3f);
			if (deathDarken > 0f)
			{
				sLeaser.sprites[BodySprite(0)].color = Color.Lerp(color, Color.black, deathDarken * 0.2f);
				sLeaser.sprites[HighLightSprite].color = Color.Lerp(color, Color.black, deathDarken * 0.1f);
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = new Color(0.6f, 0.3f, 0.6f);
		float num = rCam.PaletteDarkness();
		Color a = Color.Lerp(Color.Lerp(palette.fogColor, color, 0.3f), palette.blackColor, Mathf.Pow(num, 2f));
		Color color2 = Color.Lerp(Color.Lerp(a, new Color(1f, 1f, 1f), 0.5f), palette.blackColor, Mathf.Pow(num, 2f));
		Color color3 = Color.Lerp(color, palette.blackColor, num);
		if (electricCounter > 5)
		{
			color3 = Color.Lerp(color3, new Color(1f, 1f, 1f), UnityEngine.Random.value);
			a = Color.Lerp(a, new Color(1f, 1f, 1f), UnityEngine.Random.value);
			color2 = Color.Lerp(color2, new Color(1f, 1f, 1f), UnityEngine.Random.value);
		}
		sLeaser.sprites[BodySprite(0)].color = color2;
		base.color = a;
		sLeaser.sprites[HighLightSprite].color = color2 + new Color(0.2f, 0.2f, 0.2f) * (1f - Mathf.Pow(num, 4f));
		for (int i = 0; i < tentacles.Length; i++)
		{
			for (int j = 0; j < (sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors.Length; j++)
			{
				(sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors[j] = Color.Lerp(a, color3, (float)j / (float)((sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors.Length - 1));
			}
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

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Jelly_Fish : SoundID.Slugcat_Bite_Jelly_Fish, base.firstChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (!AbstrConsumable.isConsumed)
		{
			AbstrConsumable.Consume();
		}
		for (int i = 0; i < tentacles.Length; i++)
		{
			for (int j = 0; j < tentacles[i].GetLength(0); j++)
			{
				tentacles[i][j, 0] = Vector2.Lerp(tentacles[i][j, 0], base.firstChunk.pos, 0.2f);
			}
		}
		if (bites < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
	}

	public void ThrowByPlayer()
	{
	}
}
