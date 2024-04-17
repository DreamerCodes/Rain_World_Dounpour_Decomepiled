using System;
using System.Collections.Generic;
using Noise;
using RWCustom;
using Smoke;
using UnityEngine;

public class ExplosiveSpear : Spear
{
	public class SpearFragment : CosmeticSprite
	{
		public float rotation;

		public float lastRotation;

		public float rotVel;

		public SpearFragment(Vector2 pos, Vector2 vel)
		{
			base.pos = pos + vel * 2f;
			lastPos = pos;
			base.vel = vel;
			rotation = UnityEngine.Random.value * 360f;
			lastRotation = rotation;
			rotVel = Mathf.Lerp(-26f, 26f, UnityEngine.Random.value);
		}

		public override void Update(bool eu)
		{
			vel *= 0.999f;
			vel.y -= room.gravity * 0.9f;
			lastRotation = rotation;
			rotation += rotVel * vel.magnitude;
			if (Vector2.Distance(lastPos, pos) > 18f && room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
			{
				IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
				FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
				pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
				bool flag = false;
				if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
				{
					vel.x = Mathf.Abs(vel.x) * 0.5f;
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
				{
					vel.x = (0f - Mathf.Abs(vel.x)) * 0.5f;
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
				{
					vel.y = Mathf.Abs(vel.y) * 0.5f;
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
				{
					vel.y = (0f - Mathf.Abs(vel.y)) * 0.5f;
					flag = true;
				}
				if (flag)
				{
					rotVel *= 0.8f;
					rotVel += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 4f * UnityEngine.Random.value;
					room.PlaySound(SoundID.Spear_Fragment_Bounce, pos);
				}
			}
			if ((room.GetTile(pos).Solid && room.GetTile(lastPos).Solid) || pos.x < -100f)
			{
				Destroy();
			}
			base.Update(eu);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("SpearFragment" + (1 + UnityEngine.Random.Range(0, 2)));
			sLeaser.sprites[0].scaleX = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			sLeaser.sprites[0].scaleY = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y;
			sLeaser.sprites[0].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			sLeaser.sprites[0].color = palette.blackColor;
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public Vector2[,] rag;

	private float conRad = 7f;

	public Color redColor;

	public int igniteCounter;

	public int explodeAt;

	public bool exploded;

	public Color explodeColor = new Color(1f, 0.4f, 0.3f);

	public List<int> miniExplosions;

	public int destroyCounter;

	private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public bool Ignited => igniteCounter > 0;

	public void Ignite()
	{
		if (igniteCounter <= 0)
		{
			igniteCounter = 1;
			room.PlaySound(SoundID.Fire_Spear_Ignite, base.firstChunk);
		}
	}

	public ExplosiveSpear(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		explodeAt = UnityEngine.Random.Range(60, 100);
		rag = new Vector2[UnityEngine.Random.Range(4, UnityEngine.Random.Range(4, 10)), 6];
		miniExplosions = new List<int>();
		int num = 20;
		for (int i = 0; i < explodeAt / num; i++)
		{
			miniExplosions.Add(UnityEngine.Random.Range(i * num, (i + 1) * num));
		}
		UnityEngine.Random.state = state;
	}

	public void ResetRag()
	{
		Vector2 vector = RagAttachPos(1f);
		for (int i = 0; i < rag.GetLength(0); i++)
		{
			rag[i, 0] = vector;
			rag[i, 1] = vector;
			rag[i, 2] *= 0f;
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		ResetRag();
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetRag();
	}

	public override void Update(bool eu)
	{
		bool flag = base.mode == Mode.Thrown;
		base.Update(eu);
		for (int i = 0; i < rag.GetLength(0); i++)
		{
			float t = (float)i / (float)(rag.GetLength(0) - 1);
			rag[i, 1] = rag[i, 0];
			rag[i, 0] += rag[i, 2];
			rag[i, 2] -= rotation * Mathf.InverseLerp(1f, 0f, i) * 0.8f;
			rag[i, 4] = rag[i, 3];
			rag[i, 3] = (rag[i, 3] + rag[i, 5] * Custom.LerpMap(Vector2.Distance(rag[i, 0], rag[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
			rag[i, 5] = (rag[i, 5] + Custom.RNV() * UnityEngine.Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(rag[i, 0], rag[i, 1])), 0.3f)).normalized;
			if (room.PointSubmerged(rag[i, 0]))
			{
				rag[i, 2] *= Custom.LerpMap(rag[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
				rag[i, 2].y += 0.05f;
				rag[i, 2] += Custom.RNV() * 0.1f;
				continue;
			}
			rag[i, 2] *= Custom.LerpMap(Vector2.Distance(rag[i, 0], rag[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
			rag[i, 2].y -= room.gravity * Custom.LerpMap(Vector2.Distance(rag[i, 0], rag[i, 1]), 1f, 6f, 0.6f, 0f);
			if (i % 3 == 2 || i == rag.GetLength(0) - 1)
			{
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(rag[i, 0], rag[i, 1], rag[i, 2], 1f, new IntVector2(0, 0), goThroughFloors: false);
				cd = SharedPhysics.HorizontalCollision(room, cd);
				cd = SharedPhysics.VerticalCollision(room, cd);
				cd = SharedPhysics.SlopesVertically(room, cd);
				rag[i, 0] = cd.pos;
				rag[i, 2] = cd.vel;
				if (cd.contactPoint.x != 0)
				{
					rag[i, 2].y *= 0.6f;
				}
				if (cd.contactPoint.y != 0)
				{
					rag[i, 2].x *= 0.6f;
				}
			}
		}
		for (int j = 0; j < rag.GetLength(0); j++)
		{
			if (j > 0)
			{
				Vector2 normalized = (rag[j, 0] - rag[j - 1, 0]).normalized;
				float num = Vector2.Distance(rag[j, 0], rag[j - 1, 0]);
				float num2 = ((num > conRad) ? 0.5f : 0.25f);
				rag[j, 0] += normalized * (conRad - num) * num2;
				rag[j, 2] += normalized * (conRad - num) * num2;
				rag[j - 1, 0] -= normalized * (conRad - num) * num2;
				rag[j - 1, 2] -= normalized * (conRad - num) * num2;
				if (j > 1)
				{
					normalized = (rag[j, 0] - rag[j - 2, 0]).normalized;
					rag[j, 2] += normalized * 0.2f;
					rag[j - 2, 2] -= normalized * 0.2f;
				}
				if (j < rag.GetLength(0) - 1)
				{
					rag[j, 3] = Vector3.Slerp(rag[j, 3], (rag[j - 1, 3] * 2f + rag[j + 1, 3]) / 3f, 0.1f);
					rag[j, 5] = Vector3.Slerp(rag[j, 5], (rag[j - 1, 5] * 2f + rag[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(rag[j, 1], rag[j, 0]), 1f, 8f, 0.05f, 0.5f));
				}
			}
			else
			{
				rag[j, 0] = RagAttachPos(1f);
				rag[j, 2] *= 0f;
			}
		}
		if (flag && base.mode != Mode.Thrown && igniteCounter < 1)
		{
			Ignite();
		}
		if (base.Submersion > 0.2f && room.waterObject.WaterIsLethal && igniteCounter < 1)
		{
			Ignite();
		}
		if (igniteCounter > 0)
		{
			int num3 = igniteCounter;
			igniteCounter++;
			if (stuckInObject == null)
			{
				igniteCounter++;
			}
			room.AddObject(new Spark(base.firstChunk.pos + rotation * 15f, -rotation * Mathf.Lerp(6f, 11f, UnityEngine.Random.value) + Custom.RNV() * UnityEngine.Random.value * 1.5f, explodeColor, null, 8, 18));
			room.MakeBackgroundNoise(0.5f);
			if (miniExplosions.Count > 0 && num3 < miniExplosions[0] && igniteCounter >= miniExplosions[0])
			{
				miniExplosions.RemoveAt(0);
				MiniExplode();
			}
			if (igniteCounter > explodeAt && !exploded)
			{
				Explode();
			}
		}
		if (exploded)
		{
			destroyCounter++;
			for (int k = 0; k < 2; k++)
			{
				room.AddObject(new SpearFragment(base.firstChunk.pos, Custom.RNV() * Mathf.Lerp(20f, 40f, UnityEngine.Random.value)));
			}
			room.AddObject(new PuffBallSkin(base.firstChunk.pos + rotation * (pivotAtTip ? 0f : 10f), Custom.RNV() * Mathf.Lerp(10f, 30f, UnityEngine.Random.value), redColor, Color.Lerp(redColor, new Color(0f, 0f, 0f), 0.3f)));
			if (destroyCounter > 4)
			{
				Destroy();
			}
		}
	}

	public override void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
	{
		base.WeaponDeflect(inbetweenPos, deflectDir, bounceSpeed);
		if (UnityEngine.Random.value < 0.5f)
		{
			Ignite();
		}
		else
		{
			Explode();
		}
	}

	private float ConchunkWeight(Vector2 pushDir, BodyChunkConnection con)
	{
		if (con.chunk1 == base.stuckInChunk)
		{
			return Custom.LerpMap(Vector2.Dot(pushDir, Custom.DirVec(base.stuckInChunk.pos, con.chunk2.pos)), -0.5f, 1f, 0f, 7f, 1.5f);
		}
		if (con.chunk2 == base.stuckInChunk)
		{
			return Custom.LerpMap(Vector2.Dot(pushDir, Custom.DirVec(base.stuckInChunk.pos, con.chunk1.pos)), -0.5f, 1f, 0f, 7f, 1.5f);
		}
		return 0f;
	}

	private void MiniExplode()
	{
		if (stuckInObject != null)
		{
			float num = 7f;
			float num2 = 0f;
			Vector2 vector = rotation;
			if (room.readyForAI && room.aimap.getAItile(base.firstChunk.pos).floorAltitude < 3)
			{
				vector = Vector3.Slerp(vector, new Vector2(0f, 1f), 0.2f);
			}
			for (int i = 0; i < stuckInObject.bodyChunkConnections.Length; i++)
			{
				if (stuckInObject.bodyChunkConnections[i].type != BodyChunkConnection.Type.Pull && (stuckInObject.bodyChunkConnections[i].chunk1 == base.stuckInChunk || stuckInObject.bodyChunkConnections[i].chunk2 == base.stuckInChunk))
				{
					num2 += ConchunkWeight(vector, stuckInObject.bodyChunkConnections[i]);
				}
			}
			if (num2 > 0f)
			{
				float num3 = Mathf.Clamp(num2 * 2f, 0f, 6f);
				num -= num3;
				for (int j = 0; j < stuckInObject.bodyChunkConnections.Length; j++)
				{
					if (stuckInObject.bodyChunkConnections[j].type != BodyChunkConnection.Type.Pull)
					{
						if (stuckInObject.bodyChunkConnections[j].chunk2 == base.stuckInChunk)
						{
							stuckInObject.bodyChunkConnections[j].chunk1.vel += vector * num3 * ConchunkWeight(vector, stuckInObject.bodyChunkConnections[j]) / (num2 * stuckInObject.bodyChunkConnections[j].chunk1.mass);
							stuckInObject.bodyChunkConnections[j].chunk1.pos += vector * num3 * ConchunkWeight(vector, stuckInObject.bodyChunkConnections[j]) / (num2 * stuckInObject.bodyChunkConnections[j].chunk1.mass);
						}
						else if (stuckInObject.bodyChunkConnections[j].chunk1 == base.stuckInChunk)
						{
							stuckInObject.bodyChunkConnections[j].chunk2.vel += vector * num3 * ConchunkWeight(vector, stuckInObject.bodyChunkConnections[j]) / (num2 * stuckInObject.bodyChunkConnections[j].chunk2.mass);
							stuckInObject.bodyChunkConnections[j].chunk2.pos += vector * num3 * ConchunkWeight(vector, stuckInObject.bodyChunkConnections[j]) / (num2 * stuckInObject.bodyChunkConnections[j].chunk2.mass);
						}
					}
				}
			}
			if (stuckInObject is Creature)
			{
				(stuckInObject as Creature).Violence(base.firstChunk, vector * num, base.stuckInChunk, null, Creature.DamageType.Explosion, (stuckInAppendage != null) ? 0.2f : 0.6f, 0f);
			}
			else
			{
				base.stuckInChunk.vel += vector * num / base.stuckInChunk.mass;
			}
			base.stuckInChunk.pos += vector * num / base.stuckInChunk.mass;
		}
		Vector2 vector2 = base.firstChunk.pos + rotation * (pivotAtTip ? 0f : 15f);
		room.AddObject(new Explosion.ExplosionLight(vector2, 40f, 1f, 2, explodeColor));
		for (int k = 0; k < 8; k++)
		{
			Vector2 vector3 = Custom.RNV();
			room.AddObject(new Spark(vector2 + vector3 * UnityEngine.Random.value * 10f, vector3 * Mathf.Lerp(6f, 18f, UnityEngine.Random.value), explodeColor, null, 4, 18));
		}
		room.AddObject(new ShockWave(vector2, 30f, 0.035f, 2));
		room.PlaySound(SoundID.Fire_Spear_Pop, vector2);
		for (int l = 0; l < rag.GetLength(0); l++)
		{
			rag[l, 2] += (Custom.DirVec(vector2, rag[l, 0]) + Custom.RNV() - rotation) * UnityEngine.Random.value * 5f;
			rag[l, 0] += (Custom.DirVec(vector2, rag[l, 0]) + Custom.RNV() - rotation) * UnityEngine.Random.value * 5f;
		}
		if (base.mode == Mode.Free && stuckInObject == null)
		{
			if (base.firstChunk.vel.y < 0f)
			{
				base.firstChunk.vel.y *= 0.5f;
			}
			base.firstChunk.vel -= (Custom.RNV() + rotation) * UnityEngine.Random.value * ((base.firstChunk.ContactPoint.y < 0) ? 5f : 15f);
			SetRandomSpin();
		}
		room.InGameNoise(new InGameNoise(vector2, 800f, this, 1f));
		vibrate = Math.Max(vibrate, 6);
	}

	public void Explode()
	{
		if (exploded)
		{
			return;
		}
		exploded = true;
		if (stuckInObject != null)
		{
			if (stuckInObject is Creature)
			{
				(stuckInObject as Creature).Violence(base.firstChunk, rotation * 12f, base.stuckInChunk, null, Creature.DamageType.Explosion, (stuckInAppendage != null) ? 1.8f : 4.2f, 120f);
			}
			else
			{
				base.stuckInChunk.vel += rotation * 12f / base.stuckInChunk.mass;
			}
		}
		Vector2 vector = base.firstChunk.pos + rotation * (pivotAtTip ? 0f : 10f);
		room.AddObject(new SootMark(room, vector, 50f, bigSprite: false));
		room.AddObject(new Explosion(room, this, vector, 5, 110f, 5f, 1.1f, 60f, 0.3f, thrownBy, 0.8f, 0f, 0.7f));
		for (int i = 0; i < 14; i++)
		{
			room.AddObject(new Explosion.ExplosionSmoke(vector, Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
		}
		room.AddObject(new Explosion.ExplosionLight(vector, 160f, 1f, 3, explodeColor));
		room.AddObject(new ExplosionSpikes(room, vector, 9, 4f, 5f, 5f, 90f, explodeColor));
		room.AddObject(new ShockWave(vector, 60f, 0.045f, 4));
		for (int j = 0; j < 20; j++)
		{
			Vector2 vector2 = Custom.RNV();
			room.AddObject(new Spark(vector + vector2 * UnityEngine.Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), explodeColor, null, 4, 18));
		}
		room.ScreenMovement(vector, default(Vector2), 0.7f);
		for (int k = 0; k < 2; k++)
		{
			Smolder smolder = null;
			if (stuckInObject != null)
			{
				smolder = new Smolder(room, base.stuckInChunk.pos, base.stuckInChunk, stuckInAppendage);
			}
			else
			{
				Vector2? vector3 = SharedPhysics.ExactTerrainRayTracePos(room, base.firstChunk.pos, base.firstChunk.pos + ((k == 0) ? (rotation * 20f) : (Custom.RNV() * 20f)));
				if (vector3.HasValue)
				{
					smolder = new Smolder(room, vector3.Value + Custom.DirVec(vector3.Value, base.firstChunk.pos) * 3f, null, null);
				}
			}
			if (smolder != null)
			{
				room.AddObject(smolder);
			}
		}
		abstractPhysicalObject.LoseAllStuckObjects();
		room.PlaySound(SoundID.Fire_Spear_Explode, vector);
		room.InGameNoise(new InGameNoise(vector, 8000f, this, 1f));
		Destroy();
	}

	private Vector2 RagAttachPos(float timeStacker)
	{
		return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) + (Vector2)Vector3.Slerp(lastRotation, rotation, timeStacker) * 15f;
	}

	public override void TryImpaleSmallCreature(Creature smallCrit)
	{
	}

	public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
	{
		base.HitByExplosion(hitFac, explosion, hitChunk);
		if (UnityEngine.Random.value < hitFac)
		{
			Ignite();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (stuckIns != null)
		{
			rCam.ReturnFContainer("HUD").AddChild(stuckIns.label);
		}
		sLeaser.sprites = new FSprite[3];
		sLeaser.sprites[1] = new FSprite("SmallSpear");
		sLeaser.sprites[0] = new FSprite("SpearRag");
		sLeaser.sprites[2] = TriangleMesh.MakeLongMesh(rag.GetLength(0), pointyTip: false, customColor: false);
		sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
		sLeaser.sprites[2].alpha = rCam.game.SeededRandom(abstractPhysicalObject.ID.RandomSeed);
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		sLeaser.sprites[0].color = redColor;
		sLeaser.sprites[2].color = redColor;
		if (blink > 0)
		{
			if (blink > 1 && UnityEngine.Random.value < 0.5f)
			{
				sLeaser.sprites[1].color = new Color(1f, 1f, 1f);
			}
			else
			{
				sLeaser.sprites[1].color = color;
			}
		}
		else if (sLeaser.sprites[1].color != color)
		{
			sLeaser.sprites[1].color = color;
		}
		if (base.mode == Mode.Free && base.firstChunk.ContactPoint.y < 0)
		{
			sLeaser.sprites[0].anchorY += 0.2f;
		}
		float num = 0f;
		Vector2 vector = RagAttachPos(timeStacker);
		for (int i = 0; i < rag.GetLength(0); i++)
		{
			float f = (float)i / (float)(rag.GetLength(0) - 1);
			Vector2 vector2 = Vector2.Lerp(rag[i, 1], rag[i, 0], timeStacker);
			float num2 = (2f + 2f * Mathf.Sin(Mathf.Pow(f, 2f) * (float)Math.PI)) * Vector3.Slerp(rag[i, 4], rag[i, 3], timeStacker).x;
			Vector2 normalized = (vector - vector2).normalized;
			Vector2 vector3 = Custom.PerpendicularVector(normalized);
			float num3 = Vector2.Distance(vector, vector2) / 5f;
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4, vector - normalized * num3 - vector3 * (num2 + num) * 0.5f - camPos);
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 1, vector - normalized * num3 + vector3 * (num2 + num) * 0.5f - camPos);
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 + normalized * num3 - vector3 * num2 - camPos);
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized * num3 + vector3 * num2 - camPos);
			vector = vector2;
			num = num2;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		sLeaser.sprites[1].color = color;
		redColor = Color.Lerp(new Color(1f, 0.05f, 0.04f), palette.blackColor, 0.1f + 0.8f * palette.darkness);
	}
}
