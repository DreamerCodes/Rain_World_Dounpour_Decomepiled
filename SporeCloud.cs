using System;
using RWCustom;
using UnityEngine;

public class SporeCloud : CosmeticSprite
{
	public float life;

	private float lastLife;

	public float lifeTime;

	public float rotation;

	public float lastRotation;

	public float rotVel;

	public Vector2 getToPos;

	public float rad;

	private Color color;

	public int checkInsectsDelay;

	public AbstractCreature killTag;

	private InsectCoordinator smallInsects;

	public bool nonToxic;

	public SporeCloud(Vector2 pos, Vector2 vel, Color color, float size, AbstractCreature killTag, int checkInsectsDelay, InsectCoordinator smallInsects)
	{
		this.checkInsectsDelay = checkInsectsDelay;
		this.smallInsects = smallInsects;
		life = size;
		lastLife = size;
		lastPos = pos;
		base.vel = vel;
		this.color = color;
		this.killTag = killTag;
		getToPos = pos + new Vector2(Mathf.Lerp(-50f, 50f, UnityEngine.Random.value), Mathf.Lerp(-100f, 400f, UnityEngine.Random.value));
		base.pos = pos + vel.normalized * 60f * UnityEngine.Random.value;
		rad = Mathf.Lerp(0.6f, 1.5f, UnityEngine.Random.value) * size;
		rotation = UnityEngine.Random.value * 360f;
		lastRotation = rotation;
		rotVel = Mathf.Lerp(-6f, 6f, UnityEngine.Random.value);
		lifeTime = Mathf.Lerp(170f, 400f, UnityEngine.Random.value);
	}

	public override void Update(bool eu)
	{
		vel *= 0.9f;
		vel += Custom.DirVec(pos, getToPos) * UnityEngine.Random.value * 0.04f;
		lastRotation = rotation;
		rotation += rotVel * vel.magnitude;
		lastLife = life;
		life -= 1f / lifeTime;
		if (room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
		{
			IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
			FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
			pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
			if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
			{
				vel.x = Mathf.Abs(vel.x);
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
			{
				vel.x = 0f - Mathf.Abs(vel.x);
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
			{
				vel.y = Mathf.Abs(vel.y);
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
			{
				vel.y = 0f - Mathf.Abs(vel.y);
			}
		}
		if (lastLife <= 0f)
		{
			Destroy();
		}
		if (!nonToxic && checkInsectsDelay > -1)
		{
			checkInsectsDelay--;
			if (checkInsectsDelay < 1)
			{
				checkInsectsDelay = 20;
				for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
				{
					if (room.abstractRoom.creatures[i].realizedCreature == null)
					{
						continue;
					}
					if (room.abstractRoom.creatures[i].realizedCreature.Template.type == CreatureTemplate.Type.Fly || room.abstractRoom.creatures[i].realizedCreature.Template.type == CreatureTemplate.Type.Spider)
					{
						if (Custom.DistLess(pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, rad + room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.rad + 20f))
						{
							if (UnityEngine.Random.value < life)
							{
								room.abstractRoom.creatures[i].realizedCreature.Die();
							}
							else
							{
								room.abstractRoom.creatures[i].realizedCreature.Stun(UnityEngine.Random.Range(10, 120));
							}
						}
					}
					else
					{
						if (!(room.abstractRoom.creatures[i].realizedCreature is InsectoidCreature))
						{
							continue;
						}
						for (int j = 0; j < room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length; j++)
						{
							if (!Custom.DistLess(pos, room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].pos, rad + room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].rad + 20f))
							{
								continue;
							}
							(room.abstractRoom.creatures[i].realizedCreature as InsectoidCreature).poison += 0.025f * Mathf.Pow(life, 4f) / Mathf.Lerp(room.abstractRoom.creatures[i].realizedCreature.TotalMass, 1f, 0.15f);
							if ((room.abstractRoom.creatures[i].realizedCreature as InsectoidCreature).poison >= 1f)
							{
								if ((room.abstractRoom.creatures[i].realizedCreature as InsectoidCreature).State is HealthState)
								{
									((room.abstractRoom.creatures[i].realizedCreature as InsectoidCreature).State as HealthState).health -= 0.0125f * Mathf.Pow(life, 4f) / Mathf.Lerp(room.abstractRoom.creatures[i].realizedCreature.TotalMass, 1f, 0.15f);
								}
								else
								{
									room.abstractRoom.creatures[i].realizedCreature.Die();
								}
							}
							room.abstractRoom.creatures[i].realizedCreature.SetKillTag(killTag);
							room.abstractRoom.creatures[i].realizedCreature.Stun(Mathf.RoundToInt(20f * UnityEngine.Random.value * life / Mathf.Lerp(room.abstractRoom.creatures[i].realizedCreature.TotalMass, 1f, 0.15f)));
						}
					}
				}
				for (int k = 0; k < room.physicalObjects.Length; k++)
				{
					for (int l = 0; l < room.physicalObjects[k].Count; l++)
					{
						if (room.physicalObjects[k][l] is SporePlant)
						{
							(room.physicalObjects[k][l] as SporePlant).PuffBallSpores(pos, rad);
						}
						else if (room.physicalObjects[k][l] is SporePlant.AttachedBee && Custom.DistLess(pos, room.physicalObjects[k][l].firstChunk.pos, rad + 20f))
						{
							(room.physicalObjects[k][l] as SporePlant.AttachedBee).life -= 0.5f;
						}
					}
				}
				if (smallInsects != null)
				{
					for (int m = 0; m < smallInsects.allInsects.Count; m++)
					{
						if (Custom.DistLess(smallInsects.allInsects[m].pos, pos, rad + 70f))
						{
							smallInsects.allInsects[m].alive = false;
						}
					}
				}
			}
		}
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["Spores"];
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
		float num = Mathf.Lerp(lastLife, life, timeStacker);
		sLeaser.sprites[0].scale = 7f * rad * ((num > 0.5f) ? Custom.LerpMap(num, 1f, 0.5f, 0.5f, 1f) : Mathf.Sin(num * (float)Math.PI));
		sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastLife, life, timeStacker)), 1.2f);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = color;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
