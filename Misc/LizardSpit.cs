using RWCustom;
using UnityEngine;

public class LizardSpit : UpdatableAndDeletable, IDrawable
{
	public Vector2 pos;

	public Vector2 lastPos;

	public Vector2 vel;

	public Lizard lizard;

	public BodyChunk stickChunk;

	private int fallOff;

	private float massLeft;

	private float dissapearSpeed;

	private float myAimUp;

	private BodyChunk myAimChunk;

	private Vector2[,] slime;

	private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	private float Rad => 4f * massLeft;

	public int JaggedSprite => 0;

	public int DotSprite => 1 + slime.GetLength(0);

	public int TotalSprites => slime.GetLength(0) + 2;

	public int SlimeSprite(int s)
	{
		return 1 + s;
	}

	public LizardSpit(Vector2 pos, Vector2 vel, Lizard lizard)
	{
		lastPos = pos;
		this.vel = vel;
		this.pos = pos + vel;
		this.lizard = lizard;
		fallOff = Random.Range(2, 100);
		massLeft = 1f;
		dissapearSpeed = Random.value;
		myAimUp = lizard.AI.redSpitAI.aimUp;
		myAimChunk = lizard.AI.redSpitAI.aimForChunk;
		slime = new Vector2[(int)Mathf.Lerp(8f, 15f, Random.value), 4];
		for (int i = 0; i < slime.GetLength(0); i++)
		{
			slime[i, 0] = pos + Custom.RNV() * 4f * Random.value;
			slime[i, 1] = slime[i, 0];
			slime[i, 2] = vel + Custom.RNV() * 4f * Random.value;
			int num = -1;
			num = ((i != 0 && !(Random.value < 0.3f)) ? ((!(Random.value < 0.7f)) ? Random.Range(0, slime.GetLength(0)) : (i - 1)) : (-1));
			slime[i, 3] = new Vector2(num, Mathf.Lerp(3f, 8f, Random.value));
		}
	}

	public override void Update(bool eu)
	{
		lastPos = pos;
		pos += vel;
		vel.y -= 0.9f;
		for (int i = 0; i < slime.GetLength(0); i++)
		{
			slime[i, 1] = slime[i, 0];
			slime[i, 0] += slime[i, 2];
			slime[i, 2] *= 0.99f;
			slime[i, 2].y -= 0.9f * (ModManager.MMF ? room.gravity : 1f);
			if ((int)slime[i, 3].x < 0 || (int)slime[i, 3].x >= slime.GetLength(0))
			{
				Vector2 vector = pos;
				Vector2 vector2 = Custom.DirVec(slime[i, 0], vector);
				float num = Vector2.Distance(slime[i, 0], vector);
				slime[i, 0] -= vector2 * (slime[i, 3].y * massLeft - num) * 0.9f;
				slime[i, 2] -= vector2 * (slime[i, 3].y * massLeft - num) * 0.9f;
				pos += vector2 * (slime[i, 3].y - num) * 0.1f;
				vel += vector2 * (slime[i, 3].y - num) * 0.1f;
			}
			else
			{
				Vector2 vector3 = Custom.DirVec(slime[i, 0], slime[(int)slime[i, 3].x, 0]);
				float num2 = Vector2.Distance(slime[i, 0], slime[(int)slime[i, 3].x, 0]);
				slime[i, 0] -= vector3 * (slime[i, 3].y * massLeft - num2) * 0.5f;
				slime[i, 2] -= vector3 * (slime[i, 3].y * massLeft - num2) * 0.5f;
				slime[(int)slime[i, 3].x, 0] += vector3 * (slime[i, 3].y * massLeft - num2) * 0.5f;
				slime[(int)slime[i, 3].x, 2] += vector3 * (slime[i, 3].y * massLeft - num2) * 0.5f;
			}
		}
		if (stickChunk != null && stickChunk.owner is Creature && (stickChunk.owner as Creature).inShortcut)
		{
			stickChunk = null;
		}
		if (stickChunk != null && stickChunk.owner.room == room && Custom.DistLess(stickChunk.pos, pos, stickChunk.rad + 40f) && fallOff > 0)
		{
			float num3 = Rad + stickChunk.rad;
			Vector2 vector4 = Custom.DirVec(pos, stickChunk.pos);
			float num4 = Vector2.Distance(pos, stickChunk.pos);
			float num5 = stickChunk.mass / (0.1f * massLeft + stickChunk.mass);
			pos += vector4 * (num4 - num3) * num5;
			vel += vector4 * (num4 - num3) * num5;
			stickChunk.pos -= vector4 * (num4 - num3) * (1f - num5);
			stickChunk.vel -= vector4 * (num4 - num3) * (1f - num5);
			fallOff--;
		}
		else
		{
			stickChunk = null;
			bool flag = false;
			IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
			if (intVector.HasValue)
			{
				FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(Rad));
				pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
				if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
				{
					vel.x = Mathf.Abs(vel.x) * 0.2f;
					vel.y *= 0.8f;
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
				{
					vel.x = (0f - Mathf.Abs(vel.x)) * 0.2f;
					vel.y *= 0.8f;
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
				{
					vel.y = Mathf.Abs(vel.y) * 0.2f;
					vel.x *= 0.8f;
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
				{
					vel.y = (0f - Mathf.Abs(vel.y)) * 0.2f;
					vel.x *= 0.8f;
					flag = true;
				}
			}
			if (!flag)
			{
				Vector2 vector5 = vel;
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, Rad, new IntVector2(0, 0), goThroughFloors: true);
				cd = SharedPhysics.VerticalCollision(room, cd);
				cd = SharedPhysics.HorizontalCollision(room, cd);
				cd = SharedPhysics.SlopesVertically(room, cd);
				pos = cd.pos;
				vel = cd.vel;
				if (cd.contactPoint.x != 0)
				{
					vel.x = Mathf.Abs(vector5.x) * 0.2f * (float)(-cd.contactPoint.x);
					vel.y *= 0.8f;
					flag = true;
				}
				if (cd.contactPoint.y != 0)
				{
					vel.y = Mathf.Abs(vector5.y) * 0.2f * (float)(-cd.contactPoint.y);
					vel.x *= 0.8f;
					flag = true;
				}
			}
			SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, room, lastPos, ref pos, Rad, 1, lizard, hitAppendages: false);
			if (collisionResult.chunk != null)
			{
				pos = collisionResult.collisionPoint;
				stickChunk = collisionResult.chunk;
				vel *= 0.25f;
				if (stickChunk.owner is Creature)
				{
					if (myAimChunk != null && stickChunk.owner == myAimChunk.owner)
					{
						myAimChunk = null;
						lizard.AI.redSpitAI.SetAim(myAimUp);
					}
					(stickChunk.owner as Creature).Violence(null, vel * 0.6f, stickChunk, null, Creature.DamageType.Blunt, 0f, Random.value * 7f);
					room.PlaySound((stickChunk.owner is Player) ? SoundID.Red_Lizard_Spit_Hit_Player : SoundID.Red_Lizard_Spit_Hit_NPC, pos);
				}
				else
				{
					stickChunk.vel += vel * 0.6f / Mathf.Max(1f, stickChunk.mass);
					room.PlaySound(SoundID.Red_Lizard_Spit_Hit_NPC, pos);
				}
				flag = true;
			}
			if (flag)
			{
				if (myAimChunk != null)
				{
					if (pos.y < myAimChunk.pos.y)
					{
						lizard.AI.redSpitAI.AimABitUp(myAimUp);
					}
					else if (pos.y > myAimChunk.pos.y)
					{
						lizard.AI.redSpitAI.AimABitDown(myAimUp);
					}
				}
				myAimChunk = null;
				if (massLeft == 1f)
				{
					massLeft = 0.99f;
					if (stickChunk == null)
					{
						room.PlaySound(SoundID.Red_Lizard_Spit_Hit_Wall, pos);
					}
				}
			}
		}
		if (myAimChunk != null && lastPos.x < myAimChunk.pos.x != pos.x < myAimChunk.pos.x)
		{
			if (pos.y < myAimChunk.pos.y)
			{
				lizard.AI.redSpitAI.AimABitUp(myAimUp);
			}
			else if (pos.y > myAimChunk.pos.y)
			{
				lizard.AI.redSpitAI.AimABitDown(myAimUp);
			}
			myAimChunk = null;
		}
		if (massLeft < 1f)
		{
			massLeft -= Mathf.Lerp(0.5f, 1.5f, dissapearSpeed) / ((stickChunk == null) ? 30f : 120f);
		}
		if (massLeft <= 0f || pos.y < -300f)
		{
			Destroy();
		}
		base.Update(eu);
	}

	private Vector2 StuckPosOfSlime(int s, float timeStacker)
	{
		if ((int)slime[s, 3].x < 0 || (int)slime[s, 3].x >= slime.GetLength(0))
		{
			return Vector2.Lerp(lastPos, pos, timeStacker);
		}
		return Vector2.Lerp(slime[(int)slime[s, 3].x, 1], slime[(int)slime[s, 3].x, 0], timeStacker);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[DotSprite] = new FSprite("Futile_White");
		sLeaser.sprites[DotSprite].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
		sLeaser.sprites[DotSprite].alpha = Random.value * 0.5f;
		sLeaser.sprites[JaggedSprite] = new FSprite("Futile_White");
		sLeaser.sprites[JaggedSprite].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
		sLeaser.sprites[JaggedSprite].alpha = Random.value * 0.5f;
		for (int i = 0; i < slime.GetLength(0); i++)
		{
			sLeaser.sprites[SlimeSprite(i)] = new FSprite("Futile_White");
			sLeaser.sprites[SlimeSprite(i)].anchorY = 0.05f;
			sLeaser.sprites[SlimeSprite(i)].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
			sLeaser.sprites[SlimeSprite(i)].alpha = Random.value;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		float t = Mathf.InverseLerp(30f, 6f, Vector2.Distance(lastPos, pos));
		float t2 = Mathf.InverseLerp(6f, 30f, Mathf.Lerp(Vector2.Distance(lastPos, pos), Vector2.Distance(vector, Vector2.Lerp(slime[0, 1], slime[0, 0], timeStacker)), t));
		Vector2 v = Vector3.Slerp(Custom.DirVec(lastPos, pos), Custom.DirVec(vector, Vector2.Lerp(slime[0, 1], slime[0, 0], timeStacker)), t);
		sLeaser.sprites[DotSprite].x = vector.x - camPos.x;
		sLeaser.sprites[DotSprite].y = vector.y - camPos.y;
		sLeaser.sprites[DotSprite].rotation = Custom.VecToDeg(v);
		sLeaser.sprites[DotSprite].scaleX = Mathf.Lerp(0.4f, 0.2f, t2) * massLeft;
		sLeaser.sprites[DotSprite].scaleY = Mathf.Lerp(0.3f, 0.7f, t2) * massLeft;
		sLeaser.sprites[JaggedSprite].x = vector.x - camPos.x;
		sLeaser.sprites[JaggedSprite].y = vector.y - camPos.y;
		sLeaser.sprites[JaggedSprite].rotation = Custom.VecToDeg(v);
		sLeaser.sprites[JaggedSprite].scaleX = Mathf.Lerp(0.6f, 0.4f, t2) * massLeft;
		sLeaser.sprites[JaggedSprite].scaleY = Mathf.Lerp(0.5f, 1f, t2) * massLeft;
		for (int i = 0; i < slime.GetLength(0); i++)
		{
			Vector2 vector2 = Vector2.Lerp(slime[i, 1], slime[i, 0], timeStacker);
			Vector2 vector3 = StuckPosOfSlime(i, timeStacker);
			sLeaser.sprites[SlimeSprite(i)].x = vector2.x - camPos.x;
			sLeaser.sprites[SlimeSprite(i)].y = vector2.y - camPos.y;
			sLeaser.sprites[SlimeSprite(i)].scaleY = (Vector2.Distance(vector2, vector3) + 3f) / 16f;
			sLeaser.sprites[SlimeSprite(i)].rotation = Custom.AimFromOneVectorToAnother(vector2, vector3);
			sLeaser.sprites[SlimeSprite(i)].scaleX = Custom.LerpMap(Vector2.Distance(vector2, vector3), 0f, slime[i, 3].y * 3.5f, 6f, 2f, 2f) * massLeft / 16f;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[JaggedSprite].color = palette.blackColor;
		if (lizard.abstractCreature.IsVoided())
		{
			sLeaser.sprites[DotSprite].color = RainWorld.SaturatedGold;
		}
		else if (lizard.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.RedLizard)
		{
			sLeaser.sprites[DotSprite].color = new Color(1f, 0f, 0f);
		}
		else
		{
			sLeaser.sprites[DotSprite].color = lizard.effectColor;
		}
		for (int i = 0; i < slime.GetLength(0); i++)
		{
			sLeaser.sprites[SlimeSprite(i)].color = palette.blackColor;
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
}
