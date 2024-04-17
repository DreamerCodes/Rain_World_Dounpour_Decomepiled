using RWCustom;
using UnityEngine;

public class SnailGraphics : GraphicsModule, ILookingAtCreatures
{
	private Limb[] limbs;

	private GenericBodyPart head;

	private GenericBodyPart tail;

	public CreatureLooker creatureLooker;

	private int movingHand;

	private int handChangeCounter;

	private Vector2 bodyDir;

	public float shadowExtensionFac;

	private DynamicSoundLoop soundLoop;

	public Snail snail => base.owner as Snail;

	public SnailGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		limbs = new Limb[2];
		for (int i = 0; i < 2; i++)
		{
			limbs[i] = new Limb(this, snail.bodyChunks[0], i, 0.5f, 0.5f, 0.98f, 5f, 0.5f);
		}
		head = new GenericBodyPart(this, Mathf.Lerp(snail.size, 1f, 0.7f) * 5f, 0.5f, 0.98f, snail.bodyChunks[0]);
		tail = new GenericBodyPart(this, 0.1f, 0.5f, 0.88f, snail.bodyChunks[0]);
		bodyParts = new BodyPart[4];
		bodyParts[0] = limbs[0];
		bodyParts[1] = limbs[1];
		bodyParts[2] = head;
		bodyParts[3] = tail;
		creatureLooker = new CreatureLooker(this, snail.AI.tracker, snail, 0f, 30);
		soundLoop = new ChunkDynamicSoundLoop(snail.mainBodyChunk);
		Reset();
	}

	public override void Update()
	{
		base.Update();
		creatureLooker.Update();
		if (!snail.Consious)
		{
			soundLoop.sound = SoundID.None;
		}
		else if (snail.clickCounter > 0f)
		{
			soundLoop.sound = SoundID.Snail_Charging_LOOP;
			soundLoop.Pitch = Mathf.Lerp(0.5f, 1.5f, Mathf.Pow(snail.clickCounter, 1.62f));
			soundLoop.Volume = Mathf.InverseLerp(0f, 0.2f, snail.clickCounter);
		}
		else if (snail.suckPoint.HasValue && snail.AI.move)
		{
			soundLoop.sound = SoundID.Snail_Walking_LOOP;
			soundLoop.Pitch = Mathf.Lerp(1f / snail.size, 1f, 0.7f);
			soundLoop.Volume = Mathf.InverseLerp(0.4f, 2.2f, snail.mainBodyChunk.vel.magnitude);
		}
		else
		{
			soundLoop.sound = SoundID.None;
		}
		soundLoop.Update();
		handChangeCounter++;
		for (int i = 0; i < 2; i++)
		{
			float num = 14f * snail.size;
			if (snail.suckPoint.HasValue)
			{
				limbs[i].mode = Limb.Mode.HuntAbsolutePosition;
				if (snail.AI.move)
				{
					if (i == movingHand)
					{
						Vector2 vector = Custom.DirVec(snail.mainBodyChunk.lastLastPos, snail.suckPoint.Value);
						vector = (vector + (Custom.DirVec(snail.bodyChunks[1].pos, snail.bodyChunks[0].pos) + Custom.PerpendicularVector((snail.bodyChunks[1].pos - snail.bodyChunks[0].pos).normalized) * 0.5f * ((i == 0) ? (-1f) : 1f))).normalized * num * 0.9f + snail.suckPoint.Value;
						limbs[i].huntSpeed = 2f + (float)handChangeCounter;
						if (handChangeCounter < 10)
						{
							limbs[i].absoluteHuntPos = vector;
						}
						else if (handChangeCounter == 10)
						{
							limbs[i].FindGrip(snail.room, snail.mainBodyChunk.pos, limbs[i].pos, num * 0.9f, vector, 2, 2, behindWalls: true);
						}
					}
					else if (!Custom.DistLess(limbs[i].absoluteHuntPos, snail.mainBodyChunk.pos, num * 0.9f) && handChangeCounter > 10)
					{
						movingHand = 1 - movingHand;
						handChangeCounter = 0;
					}
				}
			}
			else
			{
				limbs[i].mode = Limb.Mode.Dangle;
			}
			if (!snail.Consious)
			{
				limbs[i].mode = Limb.Mode.Dangle;
			}
			if (limbs[i].mode == Limb.Mode.Dangle)
			{
				limbs[i].vel.y -= 0.9f * (1f - snail.mainBodyChunk.submersion);
			}
			limbs[i].Update();
			limbs[i].ConnectToPoint(snail.mainBodyChunk.pos, num, push: false, 0f, snail.mainBodyChunk.vel, 0.5f, 0f);
		}
		if (snail.suckPoint.HasValue && snail.AI.move)
		{
			tail.vel = Vector2.Lerp(tail.vel, (tail.pos - snail.mainBodyChunk.lastLastPos) * 0.2f * Mathf.InverseLerp(0.5f, 3f, snail.mainBodyChunk.vel.magnitude), 0.5f);
		}
		else
		{
			tail.vel.y -= 0.5f * (1f - snail.mainBodyChunk.submersion);
		}
		tail.vel -= snail.shellDirection * 1.2f;
		tail.Update();
		if (snail.outOfShell > 0f)
		{
			tail.PushOutOfTerrain(snail.room, snail.mainBodyChunk.pos);
		}
		tail.ConnectToPoint(snail.mainBodyChunk.pos, 7f * snail.size * Mathf.Lerp(0.5f, 1f, snail.outOfShell), push: false, 0f, snail.mainBodyChunk.vel, 0f, 0f);
		bodyDir = Custom.DirVec(snail.bodyChunks[1].pos, snail.bodyChunks[0].pos) + Custom.DirVec(tail.pos, snail.bodyChunks[0].pos).normalized;
		if (snail.outOfShell > 0f)
		{
			head.PushOutOfTerrain(snail.room, snail.mainBodyChunk.pos);
			if (snail.Consious)
			{
				Vector2 vector2 = bodyDir * 0.2f;
				if (creatureLooker.lookCreature != null)
				{
					if (creatureLooker.lookCreature.VisualContact)
					{
						vector2 += Custom.DirVec(snail.mainBodyChunk.pos, creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos);
					}
					else if (creatureLooker.lookCreature.EstimatedChanceOfFinding * snail.AI.CreatureUnease(creatureLooker.lookCreature.representedCreature) > 0.2f)
					{
						vector2 += Custom.DirVec(snail.mainBodyChunk.pos, snail.room.MiddleOfTile(creatureLooker.lookCreature.BestGuessForPosition()));
					}
				}
				head.vel += vector2.normalized * 2.2f;
			}
			else
			{
				head.vel.y -= 0.5f * (1f - snail.mainBodyChunk.submersion);
			}
			head.vel += (Vector2.Lerp(limbs[movingHand].pos, limbs[0].pos, 0.6f) - head.pos) * 0.1f;
		}
		head.Update();
		head.ConnectToPoint(snail.mainBodyChunk.pos, 7.5f * snail.size * Mathf.Lerp(0.2f, 1f, snail.outOfShell), push: false, 0f, snail.mainBodyChunk.vel, 0.2f, 0f);
		if (snail.room.GetTile(snail.mainBodyChunk.pos).wallbehind && snail.room.aimap.getAItile(snail.mainBodyChunk.pos).acc == AItile.Accessibility.Wall && !snail.IsTileSolid(0, -1, 0) && !snail.IsTileSolid(0, 1, 0) && !snail.IsTileSolid(0, 0, -1) && !snail.IsTileSolid(0, 0, 1) && snail.Consious && snail.suckPoint.HasValue && snail.mainBodyChunk.submersion == 0f)
		{
			shadowExtensionFac = Mathf.Clamp(shadowExtensionFac + 0.05f, 0f, 1f);
		}
		else
		{
			shadowExtensionFac = Mathf.Clamp(shadowExtensionFac - 0.05f, 0f, 1f);
		}
	}

	public override void Reset()
	{
		base.Reset();
		BodyPart[] array = bodyParts;
		foreach (BodyPart obj in array)
		{
			obj.vel *= 0f;
			obj.pos = snail.mainBodyChunk.pos;
			obj.lastPos = obj.pos;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[9];
		sLeaser.sprites[0] = new FSprite("SnailTail");
		sLeaser.sprites[0].scale = snail.bodyChunks[0].rad * 4.5f / 20f;
		sLeaser.sprites[0].anchorY = 0.2f;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[1 + i] = new FSprite("SnailLimb");
			sLeaser.sprites[1 + i].anchorY = 0.1f;
		}
		sLeaser.sprites[1].scaleX = -1f;
		sLeaser.sprites[3] = new FSprite("SnailHead");
		sLeaser.sprites[3].anchorY = 0.3f;
		sLeaser.sprites[3].scale = head.rad / 6f;
		for (int j = 0; j < 2; j++)
		{
			sLeaser.sprites[4 + j] = new FSprite("pixel");
		}
		sLeaser.sprites[6] = new FSprite("SnailShellA");
		sLeaser.sprites[6].color = snail.shellColor[0];
		sLeaser.sprites[7] = new FSprite("SnailShellB");
		sLeaser.sprites[7].color = snail.shellColor[1];
		sLeaser.sprites[8] = new FSprite("Circle20");
		sLeaser.sprites[8].scaleX = snail.bodyChunks[0].rad / 10f;
		sLeaser.sprites[8].anchorY = 0f;
		sLeaser.sprites[8].rotation = Custom.AimFromOneVectorToAnother(new Vector2(rCam.room.lightAngle.x, 0f - rCam.room.lightAngle.y), new Vector2(0f, 0f));
		sLeaser.sprites[8].color = new Color(0.003921569f, 0f, 0f);
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = Mathf.Lerp(snail.bodyChunks[0].lastPos.x, snail.bodyChunks[0].pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(snail.bodyChunks[0].lastPos.y, snail.bodyChunks[0].pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[8].x = Mathf.Lerp(snail.bodyChunks[0].lastPos.x, snail.bodyChunks[0].pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[8].y = Mathf.Lerp(snail.bodyChunks[0].lastPos.y, snail.bodyChunks[0].pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[8].scaleY = rCam.room.lightAngle.magnitude * 0.25f * shadowExtensionFac;
		sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(snail.bodyChunks[0].lastPos, snail.bodyChunks[0].pos, timeStacker), Vector2.Lerp(tail.lastPos, tail.pos, timeStacker));
		sLeaser.sprites[0].scaleY = Mathf.Max(Vector2.Distance(Vector2.Lerp(snail.bodyChunks[0].lastPos, snail.bodyChunks[0].pos, timeStacker), Vector2.Lerp(tail.lastPos, tail.pos, timeStacker)) / 23f, snail.bodyChunks[0].rad * 3.5f / 20f);
		Vector2 vector = Vector2.Lerp(snail.bodyChunks[1].lastPos, snail.bodyChunks[1].pos, timeStacker) - camPos;
		if (!snail.dead && snail.triggered)
		{
			vector += Custom.DegToVec(Random.value * 360f) * Random.value * 5f;
		}
		else if (snail.Consious && Random.value < Mathf.InverseLerp(0.4f, 0.6f, snail.clickCounter))
		{
			vector += Custom.DegToVec(Random.value * 360f) * Mathf.InverseLerp(0.4f, 1f, snail.clickCounter) * Random.value * 4f;
		}
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[1 + i].x = Mathf.Lerp(limbs[i].lastPos.x, limbs[i].pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[1 + i].y = Mathf.Lerp(limbs[i].lastPos.y, limbs[i].pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[1 + i].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(limbs[i].lastPos, limbs[i].pos, timeStacker), Vector2.Lerp(snail.bodyChunks[0].lastPos, snail.bodyChunks[0].pos, timeStacker));
			sLeaser.sprites[1 + i].scaleY = Vector2.Distance(Vector2.Lerp(limbs[i].lastPos, limbs[i].pos, timeStacker), Vector2.Lerp(snail.bodyChunks[0].lastPos, snail.bodyChunks[0].pos, timeStacker)) / 15f;
		}
		Vector2 vector2 = Vector2.Lerp(head.lastPos, head.pos, timeStacker);
		sLeaser.sprites[3].x = vector2.x - camPos.x;
		sLeaser.sprites[3].y = vector2.y - camPos.y;
		sLeaser.sprites[3].rotation = Custom.AimFromOneVectorToAnother(vector2, Vector2.Lerp(snail.bodyChunks[0].lastPos, snail.bodyChunks[0].pos, timeStacker));
		for (int j = 0; j < 2; j++)
		{
			Vector2 vector3 = Custom.PerpendicularVector((vector2 - Vector2.Lerp(snail.bodyChunks[0].lastPos, snail.bodyChunks[0].pos, timeStacker)).normalized) * 2.2f * ((j == 1) ? (-1f) : 1f);
			vector3 *= 1f - Mathf.Abs(Vector2.Dot(bodyDir, (vector2 - Vector2.Lerp(snail.bodyChunks[0].lastPos, snail.bodyChunks[0].pos, timeStacker)).normalized));
			vector3 += (vector2 - Vector2.Lerp(snail.bodyChunks[0].lastPos, snail.bodyChunks[0].pos, timeStacker)).normalized * 2f;
			vector3 += vector2;
			sLeaser.sprites[4 + j].x = vector3.x - camPos.x;
			sLeaser.sprites[4 + j].y = vector3.y - camPos.y;
		}
		sLeaser.sprites[6].x = vector.x;
		sLeaser.sprites[6].y = vector.y;
		sLeaser.sprites[7].x = vector.x;
		sLeaser.sprites[7].y = vector.y;
		sLeaser.sprites[6].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(snail.bodyChunks[0].lastPos, snail.bodyChunks[0].pos, timeStacker), Vector2.Lerp(snail.bodyChunks[1].lastPos, snail.bodyChunks[1].pos, timeStacker));
		sLeaser.sprites[7].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(snail.bodyChunks[0].lastPos, snail.bodyChunks[0].pos, timeStacker), Vector2.Lerp(snail.bodyChunks[1].lastPos, snail.bodyChunks[1].pos, timeStacker));
		sLeaser.sprites[6].scale = Mathf.Lerp(snail.bodyChunks[1].rad / 9f, snail.size, 0.2f) * (snail.bloated ? Mathf.Lerp(1.5f, 1f, timeStacker) : 1f);
		sLeaser.sprites[7].scale = Mathf.Lerp(snail.bodyChunks[1].rad / 9f, snail.size, 0.2f) * (snail.bloated ? Mathf.Lerp(1.5f, 1f, timeStacker) : 1f);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < 4; i++)
		{
			sLeaser.sprites[i].color = palette.blackColor;
		}
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[8]);
		for (int i = 0; i < 8; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score * (1f + snail.AI.CreatureUnease(crit.representedCreature));
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return null;
	}

	public void LookAtNothing()
	{
	}
}
