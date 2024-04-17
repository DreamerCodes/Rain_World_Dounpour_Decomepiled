using System;
using RWCustom;
using UnityEngine;

public class SpiderGraphics : GraphicsModule
{
	private Color blackColor;

	public Spider behindMeSpider;

	private Vector2 lastBodyDir;

	public Vector2 bodyDir;

	private Limb[,] limbs;

	private Vector2[,] deathLegPositions;

	private bool legsPosition;

	private bool lastLegsPosition;

	public float walkCycle;

	public bool blackedOut;

	public static float[,] legSpriteSizes = new float[4, 2]
	{
		{ 19f, 20f },
		{ 26f, 20f },
		{ 21f, 23f },
		{ 26f, 17f }
	};

	public static float[,] limbLengths = new float[4, 2]
	{
		{ 0.85f, 0.5f },
		{ 1f, 0.6f },
		{ 0.95f, 0.5f },
		{ 0.9f, 0.65f }
	};

	public float[,] limbGoalDistances;

	private float limbLength;

	private Spider spider => base.owner as Spider;

	private int BodySprite => 0;

	private int TotalSprites => 17;

	private float Radius(float bodyPos)
	{
		return 2f + Mathf.Sin(bodyPos * (float)Math.PI);
	}

	private int LimbSprite(int limb, int side, int segment)
	{
		return 1 + limb + segment * 4 + side * 8;
	}

	public SpiderGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		bodyDir = Custom.DegToVec(UnityEngine.Random.value * 360f);
		limbs = new Limb[4, 2];
		limbGoalDistances = new float[4, 2];
		deathLegPositions = new Vector2[4, 2];
		limbLength = Mathf.Lerp(10f, 40f, spider.iVars.size);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				deathLegPositions[i, j] = Custom.DegToVec(UnityEngine.Random.value * 360f);
				limbs[i, j] = new Limb(this, spider.mainBodyChunk, i + j * 4, 1f, 0.5f, 0.98f, 15f, 0.95f);
				limbs[i, j].mode = Limb.Mode.Dangle;
				limbs[i, j].pushOutOfTerrain = false;
			}
		}
		legsPosition = UnityEngine.Random.value < 0.5f;
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				limbs[i, j].Reset(spider.mainBodyChunk.pos);
			}
		}
	}

	public override void Update()
	{
		if (!ModManager.MSC)
		{
			bool flag = blackedOut;
			blackedOut = base.owner.room.CompleteDarkness(spider.mainBodyChunk.pos, 40f, 0.98f, checkForPlayers: true);
			if (blackedOut)
			{
				return;
			}
			base.Update();
			if (flag)
			{
				Reset();
			}
		}
		else
		{
			base.Update();
		}
		lastBodyDir = bodyDir;
		if (spider.graphicsAttachedToBodyChunk != null)
		{
			bodyDir = Custom.DirVec(spider.mainBodyChunk.pos, spider.graphicsAttachedToBodyChunk.pos);
		}
		else
		{
			bodyDir -= Custom.DirVec(spider.mainBodyChunk.pos, spider.dragPos);
			bodyDir += spider.mainBodyChunk.vel * 0.2f;
			if (!spider.Consious)
			{
				bodyDir += Custom.DegToVec(UnityEngine.Random.value * 360f) * spider.deathSpasms;
			}
			bodyDir = bodyDir.normalized;
		}
		float magnitude = spider.mainBodyChunk.vel.magnitude;
		if (magnitude > 1f)
		{
			walkCycle += Mathf.Max(0f, (magnitude - 1f) / 30f);
			if (walkCycle > 1f)
			{
				walkCycle -= 1f;
			}
		}
		lastLegsPosition = legsPosition;
		legsPosition = walkCycle > 0.5f;
		Vector2 vector = Custom.PerpendicularVector(bodyDir);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				Vector2 vector2 = bodyDir;
				if (behindMeSpider != null && behindMeSpider.graphicsModule != null)
				{
					vector2 = Vector3.Slerp(vector2, (behindMeSpider.graphicsModule as SpiderGraphics).bodyDir, 0.2f);
				}
				if (spider.graphicsAttachedToBodyChunk != null && spider.graphicsAttachedToBodyChunk.owner is Spider && spider.graphicsAttachedToBodyChunk.owner.graphicsModule != null)
				{
					vector2 = Vector3.Slerp(vector2, (spider.graphicsAttachedToBodyChunk.owner.graphicsModule as SpiderGraphics).bodyDir, 0.2f);
				}
				bool flag2 = i % 2 == j == legsPosition;
				vector2 = Custom.DegToVec(Custom.VecToDeg(vector2) + Mathf.Lerp(Mathf.Lerp(30f, 140f, (float)i * (1f / 3f)) + 20f * spider.legsPosition + ((i == 3 && spider.centipede == null) ? 20f : 0f) + 15f * (flag2 ? (-1f) : 1f) * Mathf.InverseLerp(0.5f, 5f, magnitude), 180f * (0.5f + spider.legsPosition / 2f), Mathf.Abs(spider.legsPosition) * 0.3f) * (float)(-1 + 2 * j));
				float num = limbLengths[i, 0] * limbLength;
				Vector2 vector3 = spider.mainBodyChunk.pos + vector2 * num * 0.85f + spider.mainBodyChunk.vel.normalized * num * 0.4f * Mathf.InverseLerp(0.5f, 5f, magnitude);
				if (i == 0 && !spider.dead && (!spider.idle || !(spider.lightExp > 0f)))
				{
					limbs[i, j].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value;
				}
				bool flag3 = false;
				if (spider.Consious)
				{
					limbs[i, j].mode = Limb.Mode.HuntAbsolutePosition;
					if ((spider.followingConnection != default(MovementConnection) && spider.followingConnection.type == MovementConnection.MovementType.DropToFloor) || !spider.inAccessibleTerrain)
					{
						flag3 = true;
						limbs[i, j].mode = Limb.Mode.Dangle;
						limbs[i, j].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 3f;
					}
					else if (i == 0 && spider.graphicsAttachedToBodyChunk != null)
					{
						flag3 = true;
						limbs[i, j].absoluteHuntPos = spider.graphicsAttachedToBodyChunk.pos + vector * (-1 + 2 * j) * spider.graphicsAttachedToBodyChunk.rad * 0.5f;
						limbs[i, j].pos = limbs[i, j].absoluteHuntPos;
					}
					else if (i == 3 && behindMeSpider != null)
					{
						flag3 = true;
						limbs[i, j].absoluteHuntPos = behindMeSpider.mainBodyChunk.pos + vector * (-1 + 2 * j) * behindMeSpider.mainBodyChunk.rad * -0.5f;
						limbs[i, j].pos = limbs[i, j].absoluteHuntPos;
					}
					else if (spider.centipede != null && spider.centipede.FirstSpider == spider && i < ((spider.centipede.spiders.Count <= 2) ? 1 : 2))
					{
						limbs[i, j].mode = Limb.Mode.Dangle;
						limbs[i, j].vel += vector2 * 5.5f;
						if (i == 1 && spider.centipede.prey != null)
						{
							limbs[i, j].vel += Custom.DirVec(limbs[i, j].pos, spider.centipede.preyPos) * spider.centipede.hunt * 4f;
						}
					}
					else if (spider.centipede != null && spider.graphicsAttachedToBodyChunk != null && behindMeSpider == null && i > ((spider.centipede.spiders.Count > 3) ? 1 : 2))
					{
						limbs[i, j].mode = Limb.Mode.Dangle;
						limbs[i, j].vel += vector2 * 3.5f;
					}
				}
				else
				{
					limbs[i, j].mode = Limb.Mode.Dangle;
				}
				if (limbs[i, j].mode == Limb.Mode.HuntAbsolutePosition)
				{
					if (!flag3)
					{
						if (magnitude < 1f)
						{
							if (UnityEngine.Random.value < 0.05f && !Custom.DistLess(limbs[i, j].pos, vector3, num / 6f))
							{
								FindGrip(i, j, vector3, num, magnitude);
							}
						}
						else if (flag2 && (lastLegsPosition != legsPosition || i == 3) && !Custom.DistLess(limbs[i, j].pos, vector3, num * 0.5f))
						{
							FindGrip(i, j, vector3, num, magnitude);
						}
					}
				}
				else
				{
					limbs[i, j].vel += Custom.RotateAroundOrigo(deathLegPositions[i, j], Custom.AimFromOneVectorToAnother(-bodyDir, bodyDir)) * 0.65f;
					limbs[i, j].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * spider.deathSpasms * 5f;
					limbs[i, j].vel += vector2 * 0.7f;
					limbs[i, j].vel.y -= 0.8f;
					limbGoalDistances[i, j] = 0f;
				}
				limbs[i, j].huntSpeed = 15f * Mathf.InverseLerp(-0.05f, 2f, magnitude);
				limbs[i, j].Update();
				limbs[i, j].ConnectToPoint(spider.mainBodyChunk.pos, num, push: false, 0f, spider.mainBodyChunk.vel, 1f, 0.5f);
			}
		}
		if (spider.graphicsAttachedToBodyChunk != null && spider.graphicsAttachedToBodyChunk.owner is Spider && spider.graphicsAttachedToBodyChunk.owner.graphicsModule != null)
		{
			(spider.graphicsAttachedToBodyChunk.owner.graphicsModule as SpiderGraphics).behindMeSpider = spider;
		}
		behindMeSpider = null;
	}

	private void FindGrip(int l, int s, Vector2 idealPos, float rad, float moveSpeed)
	{
		if (base.owner.room.GetTile(idealPos).wallbehind)
		{
			limbs[l, s].absoluteHuntPos = idealPos;
		}
		else
		{
			limbs[l, s].FindGrip(base.owner.room, spider.mainBodyChunk.pos, idealPos, rad, idealPos + bodyDir * Mathf.Lerp(moveSpeed * 2f, rad / 2f, 0.5f), 2, 2, behindWalls: true);
		}
		limbGoalDistances[l, s] = Vector2.Distance(limbs[l, s].pos, limbs[l, s].absoluteHuntPos);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[BodySprite] = new FSprite("SpiderBody");
		sLeaser.sprites[BodySprite].scale = Mathf.Lerp(0.2f, 1f, spider.iVars.size);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[LimbSprite(i, j, 0)] = new FSprite("SpiderLeg" + i + "A");
				sLeaser.sprites[LimbSprite(i, j, 0)].anchorY = 1f / legSpriteSizes[i, 0];
				sLeaser.sprites[LimbSprite(i, j, 0)].scaleX = ((j == 0) ? 1f : (-1f)) * Mathf.Lerp(0.45f, 0.65f, spider.iVars.size);
				sLeaser.sprites[LimbSprite(i, j, 0)].scaleY = limbLengths[i, 0] * limbLengths[i, 1] * limbLength / legSpriteSizes[i, 0];
				sLeaser.sprites[LimbSprite(i, j, 1)] = new FSprite("SpiderLeg" + i + "B");
				sLeaser.sprites[LimbSprite(i, j, 1)].anchorY = 1f / legSpriteSizes[i, 1];
				sLeaser.sprites[LimbSprite(i, j, 1)].scaleX = ((j == 0) ? 1f : (-1f)) * Mathf.Lerp(0.45f, 0.65f, spider.iVars.size);
			}
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (!ModManager.MSC)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = !blackedOut;
			}
			if (blackedOut)
			{
				return;
			}
		}
		else
		{
			if (!rCam.PositionCurrentlyVisible(spider.mainBodyChunk.pos, 32f, widescreen: true))
			{
				if (sLeaser.sprites[0].isVisible)
				{
					for (int j = 0; j < sLeaser.sprites.Length; j++)
					{
						sLeaser.sprites[j].isVisible = false;
					}
				}
				return;
			}
			if (!sLeaser.sprites[0].isVisible)
			{
				for (int k = 0; k < sLeaser.sprites.Length; k++)
				{
					sLeaser.sprites[k].isVisible = true;
				}
			}
		}
		Vector2 vector = Vector2.Lerp(spider.mainBodyChunk.lastPos, spider.mainBodyChunk.pos, timeStacker);
		sLeaser.sprites[BodySprite].x = vector.x - camPos.x;
		sLeaser.sprites[BodySprite].y = vector.y - camPos.y;
		Vector2 vector2 = Vector3.Slerp(lastBodyDir, bodyDir, timeStacker);
		Vector2 vector3 = -Custom.PerpendicularVector(vector2);
		sLeaser.sprites[BodySprite].rotation = Custom.AimFromOneVectorToAnother(-vector2, vector2);
		for (int l = 0; l < 4; l++)
		{
			for (int m = 0; m < 2; m++)
			{
				Vector2 vector4 = vector;
				vector4 += vector2 * (7f - (float)l * 0.5f - ((l == 3) ? 1.5f : 0f)) * spider.iVars.size;
				vector4 += vector3 * (3f + (float)l * 0.5f - ((l == 3) ? 5.5f : 0f)) * (-1 + 2 * m) * spider.iVars.size;
				Vector2 vector5 = vector4;
				Vector2 a = Vector2.Lerp(limbs[l, m].lastPos, limbs[l, m].pos, timeStacker);
				a = Vector2.Lerp(a, vector4 + vector2 * limbLength * 0.1f, Mathf.Sin(Mathf.InverseLerp(0f, limbGoalDistances[l, m], Vector2.Distance(a, limbs[l, m].absoluteHuntPos)) * (float)Math.PI) * 0.4f);
				float num = limbLengths[l, 0] * limbLengths[l, 1] * limbLength;
				float num2 = limbLengths[l, 0] * (1f - limbLengths[l, 1]) * limbLength;
				float num3 = Vector2.Distance(vector4, a);
				float num4 = ((l < 3) ? 1f : (-1f));
				if (l == 2)
				{
					num4 *= 0.7f;
				}
				if (spider.legsPosition != 0f)
				{
					num4 = 1f - 2f * Mathf.Pow(0.5f + 0.5f * spider.legsPosition, 0.65f);
				}
				num4 *= -1f + 2f * (float)m;
				float num5 = Mathf.Acos(Mathf.Clamp((num3 * num3 + num * num - num2 * num2) / (2f * num3 * num), 0.2f, 0.98f)) * (180f / (float)Math.PI) * num4;
				vector5 = vector4 + Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector4, a) + num5) * num;
				sLeaser.sprites[LimbSprite(l, m, 0)].x = vector4.x - camPos.x;
				sLeaser.sprites[LimbSprite(l, m, 0)].y = vector4.y - camPos.y;
				sLeaser.sprites[LimbSprite(l, m, 1)].x = vector5.x - camPos.x;
				sLeaser.sprites[LimbSprite(l, m, 1)].y = vector5.y - camPos.y;
				sLeaser.sprites[LimbSprite(l, m, 0)].rotation = Custom.AimFromOneVectorToAnother(vector4, vector5);
				sLeaser.sprites[LimbSprite(l, m, 1)].rotation = Custom.AimFromOneVectorToAnother(vector5, a);
				sLeaser.sprites[LimbSprite(l, m, 1)].scaleY = Vector2.Distance(vector5, a);
				sLeaser.sprites[LimbSprite(l, m, 1)].scaleY = limbLengths[l, 0] * limbLengths[l, 1] * limbLength / legSpriteSizes[l, 1];
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		Color color = blackColor;
		sLeaser.sprites[BodySprite].color = color;
		for (int i = 1; i < 17; i++)
		{
			sLeaser.sprites[i].color = color;
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
