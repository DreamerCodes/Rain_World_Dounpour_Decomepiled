using System;
using RWCustom;
using UnityEngine;

public class CicadaGraphics : GraphicsModule, ILookingAtCreatures
{
	public Vector2 zRotation;

	private Vector2 lastZRotation;

	private int blinkCounter;

	private Color shieldColor;

	private Color eyeColor;

	public Vector2 lookDir;

	private float lookRotation;

	public CreatureLooker creatureLooker;

	private float wingOffset;

	private float wingTimeAdd;

	public BodyPart[,] wings;

	private float[,] wingDeployment;

	private float wingDeploymentGetTo;

	private float[,] wingDeploymentSpeed;

	private int lazyWing;

	public SoundID currentLoop = SoundID.None;

	public ChunkSoundEmitter loopSoundEmitter;

	public Limb[,] tentacles;

	private float climbCounter;

	private float chargingVisuals;

	public Cicada cicada => base.owner as Cicada;

	public int BodySprite => 0;

	public int HighlightSprite => 1;

	public int HeadSprite => 6;

	public int ShieldSprite => 7;

	public int EyesASprite => 8;

	public int EyesBSprite => 9;

	public int TotalSprites => 14;

	private Cicada.IndividualVariations iVars => cicada.iVars;

	public int TentacleSprite(int side, int tentacle)
	{
		return 2 + side + tentacle + tentacle;
	}

	public int WingSprite(int side, int wing)
	{
		return 10 + side + wing + wing;
	}

	public CicadaGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		wingOffset = UnityEngine.Random.value;
		wings = new BodyPart[2, 2];
		tentacles = new Limb[2, 2];
		wingDeployment = new float[2, 2];
		wingDeploymentSpeed = new float[2, 2];
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				wings[i, j] = new BodyPart(this);
				tentacles[i, j] = new Limb(this, cicada.mainBodyChunk, i + j + j, 0f, 0.7f, 0.9f, 6f, 0.5f);
				tentacles[i, j].mode = Limb.Mode.Dangle;
				wingDeployment[i, j] = iVars.defaultWingDeployment;
			}
		}
		bodyParts = new BodyPart[8];
		bodyParts[0] = wings[0, 0];
		bodyParts[1] = wings[1, 0];
		bodyParts[2] = wings[0, 1];
		bodyParts[3] = wings[1, 1];
		bodyParts[4] = tentacles[0, 0];
		bodyParts[5] = tentacles[1, 0];
		bodyParts[6] = tentacles[0, 1];
		bodyParts[7] = tentacles[1, 1];
		lazyWing = UnityEngine.Random.Range(-2, 4);
		creatureLooker = new CreatureLooker(this, cicada.AI.tracker, cicada, 0.4f, 20);
		Reset();
	}

	public override void Update()
	{
		base.Update();
		float num = 0f;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				if (wingDeployment[i, j] == 1f)
				{
					num += 0.25f;
				}
			}
		}
		num = Mathf.Pow(num, 1.4f);
		SoundID soundID = SoundID.None;
		if (cicada.Consious && num > 0f)
		{
			soundID = SoundID.Cicada_Wings_LOOP;
			if (cicada.chargeCounter > 0)
			{
				soundID = (cicada.Charging ? SoundID.Cicada_Wings_Bump_Attack_Charge_LOOP : SoundID.Cicada_Wings_Bump_Attack_Prepare_LOOP);
			}
		}
		if (soundID != currentLoop)
		{
			if (loopSoundEmitter != null)
			{
				loopSoundEmitter.alive = false;
				loopSoundEmitter = null;
			}
			if (soundID == SoundID.None)
			{
				cicada.room.PlaySound(cicada.Consious ? SoundID.Cicada_Wings_Stop : SoundID.Cicada_Wings_Violent_Stop, cicada.mainBodyChunk, loop: false, num, iVars.wingSoundPitch);
			}
			currentLoop = soundID;
			if (currentLoop != SoundID.None)
			{
				loopSoundEmitter = cicada.room.PlaySound(currentLoop, cicada.mainBodyChunk, loop: true, 1f, 1f);
				loopSoundEmitter.requireActiveUpkeep = true;
			}
		}
		if (loopSoundEmitter != null)
		{
			loopSoundEmitter.alive = true;
			loopSoundEmitter.volume = num;
			if (currentLoop == SoundID.Cicada_Wings_LOOP)
			{
				loopSoundEmitter.pitch = iVars.wingSoundPitch * (0.85f + 0.3f * Mathf.Pow(Mathf.InverseLerp(-1f, 1f, Vector2.Dot((cicada.bodyChunks[0].pos - cicada.bodyChunks[1].pos).normalized, zRotation.normalized)), 2f));
			}
			else
			{
				loopSoundEmitter.pitch = iVars.wingSoundPitch;
			}
			if (cicada.room != null && loopSoundEmitter.room != cicada.room)
			{
				loopSoundEmitter.Destroy();
			}
			if (loopSoundEmitter.slatedForDeletetion)
			{
				loopSoundEmitter = null;
				currentLoop = SoundID.None;
			}
		}
		creatureLooker.Update();
		if (cicada.chargeCounter > 0)
		{
			chargingVisuals = Mathf.Min(chargingVisuals + 0.1f, 1f);
		}
		else
		{
			chargingVisuals = Mathf.Max(chargingVisuals - 0.05f, 0f);
		}
		lastZRotation = zRotation;
		zRotation = Vector2.Lerp(zRotation, Custom.DirVec(cicada.bodyChunks[1].pos, cicada.bodyChunks[0].pos), 0.15f);
		if (cicada.Consious)
		{
			blinkCounter--;
			if (blinkCounter < -15 || (blinkCounter < -2 && UnityEngine.Random.value < 1f / 3f))
			{
				blinkCounter = UnityEngine.Random.Range(10, 300);
			}
			if (!cicada.flying && !Custom.DistLess(cicada.mainBodyChunk.pos, cicada.mainBodyChunk.lastPos, 0.4f))
			{
				climbCounter += Vector2.Distance(cicada.mainBodyChunk.pos, cicada.mainBodyChunk.lastPos);
				if (climbCounter > 30f)
				{
					cicada.bodyChunks[0].vel += Custom.DegToVec(UnityEngine.Random.value * 360f);
					cicada.room.PlaySound(SoundID.Cicada_Tentacle_Climb, cicada.mainBodyChunk);
					climbCounter = 0f;
				}
			}
			if (cicada.flying)
			{
				wingDeploymentGetTo = 1f;
				if (cicada.chargeCounter > 0)
				{
					blinkCounter = -5;
					zRotation.y -= 0.5f;
				}
				else if (Mathf.Abs(cicada.bodyChunks[1].pos.x - cicada.bodyChunks[0].pos.x) > 5f)
				{
					zRotation += Custom.DegToVec((cicada.bodyChunks[1].pos.x < cicada.bodyChunks[0].pos.x) ? 112.5f : (-112.5f)) * 0.3f;
				}
			}
			else
			{
				if (cicada.sitDirection.x != 0)
				{
					zRotation += new Vector2(0.4f * (float)(-cicada.sitDirection.x) * ((cicada.bodyChunks[1].pos.y > cicada.bodyChunks[0].pos.y) ? 1f : (-1f)), 0f);
				}
				else
				{
					zRotation += new Vector2(0.4f * (float)cicada.sitDirection.y * ((cicada.bodyChunks[1].pos.x > cicada.bodyChunks[0].pos.x) ? 1f : (-1f)), 0f);
				}
				if (cicada.waitToFlyCounter > 0)
				{
					wingDeploymentGetTo = 0.9f;
				}
				else if (wingDeploymentGetTo == 1f)
				{
					wingDeploymentGetTo = 0.9f;
				}
				else if (UnityEngine.Random.value < 1f / 14f)
				{
					wingDeploymentGetTo = Mathf.Max(0f, wingDeploymentGetTo - UnityEngine.Random.value / 6f);
				}
			}
		}
		else
		{
			blinkCounter = -5;
		}
		if (UnityEngine.Random.value < 0.025f)
		{
			lazyWing = UnityEngine.Random.Range(-2, 4);
		}
		zRotation = zRotation.normalized;
		Vector2 vector = Custom.DirVec(cicada.bodyChunks[1].pos, cicada.mainBodyChunk.pos);
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		if (cicada.Consious && creatureLooker.lookCreature != null && blinkCounter > 0)
		{
			if (creatureLooker.lookCreature.VisualContact)
			{
				lookDir = Custom.DirVec(cicada.bodyChunks[0].pos, creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos);
			}
			else
			{
				lookDir = Custom.DirVec(cicada.bodyChunks[0].pos, cicada.room.MiddleOfTile(creatureLooker.lookCreature.BestGuessForPosition()));
			}
			Vector2 vector3 = Custom.RotateAroundOrigo(lookDir, Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), new Vector2(0f - vector.x, vector.y)));
			lookRotation = Custom.AimFromOneVectorToAnother(-vector3, vector3);
		}
		else
		{
			lookDir *= 0.9f;
			lookRotation *= 0.8f;
		}
		float num2 = (cicada.Consious ? 1f : 0.5f);
		for (int k = 0; k < 2; k++)
		{
			for (int l = 0; l < 2; l++)
			{
				if (cicada.Consious && iVars.bustedWing != k + l + l)
				{
					if (UnityEngine.Random.value < 1f / 30f)
					{
						wingDeploymentSpeed[k, l] = UnityEngine.Random.value * UnityEngine.Random.value * 0.3f;
					}
					if (wingDeploymentGetTo == 1f && lazyWing != k + l + l)
					{
						wingDeployment[k, l] = 1f;
					}
					else if (wingDeployment[k, l] < wingDeploymentGetTo)
					{
						wingDeployment[k, l] = Mathf.Min(wingDeployment[k, l] + wingDeploymentSpeed[k, l], wingDeploymentGetTo);
						if (cicada.waitToFlyCounter > 0 && lazyWing != k + l + l)
						{
							wingDeployment[k, l] = Mathf.Min(wingDeployment[k, l] + wingDeploymentSpeed[k, l] * 2.4f, wingDeploymentGetTo);
						}
					}
					else if (wingDeployment[k, l] > wingDeploymentGetTo)
					{
						if (wingDeployment[k, l] == 1f)
						{
							ResetWing(k, l);
						}
						wingDeployment[k, l] = Mathf.Max(wingDeployment[k, l] - wingDeploymentSpeed[k, l], wingDeploymentGetTo);
					}
				}
				else if (wingDeployment[k, l] == 1f)
				{
					wingDeployment[k, l] = 0.9f;
					ResetWing(k, l);
				}
				if (wingDeployment[k, l] < 1f)
				{
					wings[k, l].lastPos = wings[k, l].pos;
					wings[k, l].pos += wings[k, l].vel;
					wings[k, l].vel *= 0.8f;
					float t = Mathf.InverseLerp(0.5f, 1f, wingDeployment[k, l]) * cicada.flyingPower;
					wings[k, l].vel -= ((l == 0) ? 0.6f : 0.3f) * vector * num2 * Mathf.Lerp(1f, (l == 0) ? 0f : (-5.5f), t);
					wings[k, l].vel += 0.2f * vector2 * ((k == 0) ? (-1f) : 1f) * Mathf.Abs(zRotation.y) * num2 * Mathf.Lerp(1f, 6f, t);
					wings[k, l].vel += 0.2f * vector2 * zRotation.x * num2 * Mathf.Lerp(1f, 6f, t);
					if (!cicada.Consious)
					{
						wings[k, l].vel.y -= 0.3f;
					}
					if (wingDeployment[k, l] < 0.5f)
					{
						float num3 = Mathf.InverseLerp(0.5f, 0f, wingDeployment[k, l]);
						Vector2 b = cicada.mainBodyChunk.pos - vector * ((l == 0) ? 20f : 15f);
						b += ((l == 0) ? (-2f) : (-2f)) * vector2 * ((k == 0) ? (-1f) : 1f) * Mathf.Abs(zRotation.y);
						b += ((l == 0) ? 5f : 5f) * vector2 * zRotation.x;
						wings[k, l].vel *= 1f - num3;
						wings[k, l].pos = Vector2.Lerp(wings[k, l].pos, b, num3);
					}
					wings[k, l].Update();
					wings[k, l].ConnectToPoint(cicada.mainBodyChunk.pos - vector * 5f, ((l == 0) ? 23f : 17f) * iVars.wingLength, push: true, 0f, cicada.mainBodyChunk.vel, 0.5f, 0f);
					wings[k, l].PushOutOfTerrain(cicada.room, cicada.mainBodyChunk.pos);
				}
			}
		}
		for (int m = 0; m < 2; m++)
		{
			for (int n = 0; n < 2; n++)
			{
				tentacles[m, n].vel.y -= 0.6f;
				tentacles[m, n].vel += vector * 0.55f;
				tentacles[m, n].vel += ((n == 0) ? 0.2f : 0.6f) * vector2 * ((m == 0) ? (-1f) : 1f) * Mathf.Abs(zRotation.y);
				tentacles[m, n].vel -= ((n == 0) ? (-0.1f) : 0.6f) * vector2 * zRotation.x;
				tentacles[m, n].Update();
				if (n == 1 && cicada.grabbedBy.Count > 0 && cicada.grabbedBy[0].grabber is Player && cicada.grabbedBy[0].grabber.graphicsModule != null)
				{
					tentacles[m, n].pos = (cicada.grabbedBy[0].grabber.graphicsModule as PlayerGraphics).hands[m].pos;
					continue;
				}
				BodyChunk bodyChunk = cicada.stickyCling;
				if (bodyChunk == null && cicada.grasps[0] != null)
				{
					bodyChunk = cicada.grasps[0].grabbedChunk;
				}
				bool flag = true;
				if (bodyChunk != null && (cicada.flying || n == 0))
				{
					float num4 = ((n == 1) ? 45f : 135f);
					if (m == 0)
					{
						num4 *= -1f;
					}
					tentacles[m, n].mode = Limb.Mode.HuntAbsolutePosition;
					tentacles[m, n].absoluteHuntPos = bodyChunk.pos + Custom.DegToVec(Custom.AimFromOneVectorToAnother(cicada.mainBodyChunk.pos, bodyChunk.pos) + num4) * bodyChunk.rad;
					flag = !tentacles[m, n].reachedSnapPosition && !Custom.DistLess(tentacles[m, n].pos, cicada.mainBodyChunk.pos, 40f);
				}
				else if (cicada.flying || !cicada.Consious)
				{
					tentacles[m, n].mode = Limb.Mode.Dangle;
				}
				else
				{
					float num5 = 0f;
					num5 = ((bodyChunk != null) ? ((m == 1 == climbCounter > 15f) ? 20f : 160f) : ((n == 1 == climbCounter > 15f) ? 20f : 160f));
					if (m == 0)
					{
						num5 *= -1f;
					}
					num5 += Custom.AimFromOneVectorToAnother(-vector, vector);
					Vector2 vector4 = cicada.mainBodyChunk.pos - vector * 10f + Custom.DegToVec(num5) * ((n == 0) ? 24f : 19f) * iVars.tentacleLength;
					if (tentacles[m, n].mode == Limb.Mode.Dangle || !Custom.DistLess(tentacles[m, n].pos, vector4, 25f))
					{
						tentacles[m, n].FindGrip(cicada.room, vector4, vector4, ((n == 0) ? 24f : 19f) * 1.2f * iVars.tentacleLength, vector4, -2, -2, behindWalls: false);
					}
				}
				if (flag)
				{
					tentacles[m, n].ConnectToPoint(cicada.mainBodyChunk.pos + vector * 10f, ((n == 0) ? 24f : 19f) * iVars.tentacleLength, push: false, 0f, cicada.mainBodyChunk.vel, 0f, 0f);
					tentacles[m, n].PushOutOfTerrain(cicada.room, cicada.mainBodyChunk.pos);
				}
			}
		}
		wingOffset += 1f / (float)UnityEngine.Random.Range(50, 60);
		wingTimeAdd += 1f;
		if (wingTimeAdd >= 3f)
		{
			wingTimeAdd = 0f;
		}
	}

	private void ResetWing(int side, int wing)
	{
		Vector2 vector = Custom.DirVec(cicada.bodyChunks[1].pos, cicada.mainBodyChunk.pos);
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		wings[side, wing].vel *= 0f;
		wings[side, wing].pos = cicada.mainBodyChunk.pos - vector * 5f;
		wings[side, wing].pos += ((wing == 0) ? (-3f) : 10f) * vector;
		wings[side, wing].pos += 17f * vector2 * ((side == 0) ? (-1f) : 1f) * Mathf.Abs(zRotation.y);
		wings[side, wing].pos += 17f * vector2 * zRotation.x;
		wings[side, wing].ConnectToPoint(cicada.mainBodyChunk.pos - vector * 5f, (wing == 0) ? 23f : 17f, push: true, 0f, cicada.mainBodyChunk.vel, 0f, 0f);
		wings[side, wing].PushOutOfTerrain(cicada.room, cicada.mainBodyChunk.pos);
	}

	public override void Reset()
	{
		base.Reset();
		BodyPart[] array = bodyParts;
		foreach (BodyPart obj in array)
		{
			obj.vel *= 0f;
			obj.pos = cicada.mainBodyChunk.pos;
			obj.lastPos = obj.pos;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[BodySprite] = new FSprite("pixel");
		sLeaser.sprites[HeadSprite] = new FSprite("pixel");
		sLeaser.sprites[ShieldSprite] = new FSprite("pixel");
		sLeaser.sprites[EyesASprite] = new FSprite("pixel");
		sLeaser.sprites[EyesBSprite] = new FSprite("pixel");
		sLeaser.sprites[HighlightSprite] = new FSprite("Circle20");
		sLeaser.sprites[BodySprite].scale = iVars.fatness;
		sLeaser.sprites[HighlightSprite].scaleX = Mathf.Lerp(5f, 3f, Mathf.Abs(iVars.fatness - 1f) * 10f) / 20f;
		sLeaser.sprites[HighlightSprite].scaleY = Mathf.Lerp(12f, 8f, Mathf.Abs(iVars.fatness - 1f) * 10f) / 20f;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[WingSprite(i, j)] = new FSprite("CicadaWing" + ((j == 0) ? "A" : "B"));
				sLeaser.sprites[WingSprite(i, j)].anchorX = 0f;
				sLeaser.sprites[WingSprite(i, j)].scaleY = iVars.wingThickness;
				sLeaser.sprites[WingSprite(i, j)].shader = rCam.room.game.rainWorld.Shaders["CicadaWing"];
				sLeaser.sprites[TentacleSprite(i, j)] = TriangleMesh.MakeLongMesh(3, pointyTip: true, customColor: false);
			}
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		Vector2 p = Vector3.Slerp(lastZRotation, zRotation, timeStacker);
		Vector2 vector = Vector2.Lerp(Vector2.Lerp(cicada.bodyChunks[0].lastPos, cicada.bodyChunks[0].pos, timeStacker), Vector2.Lerp(cicada.bodyChunks[1].lastPos, cicada.bodyChunks[1].pos, timeStacker), 0.5f);
		Vector2 normalized = (Vector2.Lerp(cicada.bodyChunks[0].lastPos, cicada.bodyChunks[0].pos, timeStacker) - Vector2.Lerp(cicada.bodyChunks[1].lastPos, cicada.bodyChunks[1].pos, timeStacker)).normalized;
		Vector2 vector2 = Custom.PerpendicularVector(-normalized);
		float num = Custom.AimFromOneVectorToAnother(-normalized, normalized);
		float num2 = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), p);
		int num3 = Custom.IntClamp(8 - (int)(Mathf.Abs(num2 / 180f) * 9f), 0, 8);
		float num4 = (float)(8 - num3) * Mathf.Sign(num2) * 22.5f;
		sLeaser.sprites[BodySprite].element = Futile.atlasManager.GetElementWithName("Cicada" + num3 + "body");
		sLeaser.sprites[HeadSprite].element = Futile.atlasManager.GetElementWithName("Cicada" + num3 + "head");
		sLeaser.sprites[ShieldSprite].element = Futile.atlasManager.GetElementWithName("Cicada" + num3 + "shield");
		sLeaser.sprites[EyesASprite].element = Futile.atlasManager.GetElementWithName("Cicada" + num3 + "eyes1");
		sLeaser.sprites[EyesBSprite].element = Futile.atlasManager.GetElementWithName("Cicada" + num3 + "eyes2");
		for (int i = 0; i < 10; i++)
		{
			if (i == 1)
			{
				i = 6;
			}
			float num5 = 0f;
			if (i == HeadSprite)
			{
				num5 = 1f;
			}
			else if (i == EyesASprite)
			{
				num5 = 2f;
			}
			else if (i == EyesBSprite)
			{
				num5 = 3f;
			}
			sLeaser.sprites[i].x = vector.x - camPos.x + lookDir.x * num5;
			sLeaser.sprites[i].y = vector.y - camPos.y + lookDir.y * num5;
			if (i == ShieldSprite)
			{
				num5 = 0.05f;
			}
			else if (num5 > 0f)
			{
				num5 = 0.1f;
			}
			float num6 = num + lookRotation * num5;
			sLeaser.sprites[i].rotation = num6 - num4;
			sLeaser.sprites[i].scaleX = ((num2 > 0f) ? (-1f) : 1f);
		}
		for (int j = 0; j < 2; j++)
		{
			float num7 = ((j == 0) ? 5f : 11f);
			float num8 = ((j == 0) ? (-20f) : 24f);
			num7 += 3f * Mathf.Abs(p.x);
			for (int k = 0; k < 2; k++)
			{
				float num9 = ((j == 0) ? 11f : 9f) * (0.2f + 0.8f * Mathf.Abs(p.y)) * Mathf.Lerp(1f, 0.85f, Mathf.InverseLerp(0.5f, 0f, wingDeployment[k, j]));
				Vector2 p2 = vector + normalized * num7 + vector2 * num9 * ((k == 0) ? 1f : (-1f)) + vector2 * p.x * Mathf.Lerp(-3f, -5f, Mathf.InverseLerp(0.5f, 0f, wingDeployment[k, j]));
				p2 += lookDir * ((j == 0) ? 1f : 3f);
				sLeaser.sprites[WingSprite(k, j)].x = p2.x - camPos.x;
				sLeaser.sprites[WingSprite(k, j)].y = p2.y - camPos.y;
				float a = p.x;
				if (wingDeployment[k, j] < 1f)
				{
					a = Mathf.Max(Mathf.InverseLerp(30f, 18f, Vector2.Distance(cicada.bodyChunks[1].pos, wings[k, j].pos)), Mathf.InverseLerp(1f, 0.5f, wingDeployment[k, j]));
				}
				a = Mathf.Lerp(a, 1f, chargingVisuals);
				sLeaser.sprites[WingSprite(k, j)].alpha = Mathf.Pow(Mathf.Abs(a), 3f);
				sLeaser.sprites[WingSprite(k, j)].color = Color.Lerp(new Color(0f, 0f, 0f), shieldColor, Mathf.Abs(a) + 0.2f);
				if (wingDeployment[k, j] == 1f)
				{
					float a2;
					float b;
					float num10;
					if (j == 0)
					{
						num10 = Mathf.Pow(Custom.Decimal(wingOffset + Mathf.InverseLerp(0f, 3f, wingTimeAdd + timeStacker)), 0.75f);
						a2 = -65f;
						b = 40f;
					}
					else
					{
						num10 = Mathf.Pow(Custom.Decimal(wingOffset + Mathf.InverseLerp(0f, 3f, wingTimeAdd + timeStacker) + 0.8f), 1.3f);
						a2 = -45f;
						b = 75f;
					}
					num10 = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(num10 * (float)Math.PI * 2f) * Mathf.Lerp(1f, 0.3f, chargingVisuals), 0.7f);
					num10 = Mathf.Lerp(a2, b, num10);
					if (chargingVisuals > 0f)
					{
						num10 = ((!cicada.Charging) ? (num10 + 30f * chargingVisuals) : (num10 - 60f * chargingVisuals));
					}
					sLeaser.sprites[WingSprite(k, j)].rotation = num - 180f + (num8 + num10) * ((k == 0) ? (-1f) : 1f);
					sLeaser.sprites[WingSprite(k, j)].scaleX = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(Mathf.Abs(p.y), 1f, Mathf.Abs(0.5f - num10) * 1.4f)), 1f) * ((k == 0) ? (-1f) : 1f) * iVars.wingLength;
				}
				else
				{
					sLeaser.sprites[WingSprite(k, j)].scaleX = ((k == 0) ? (-1f) : 1f) * iVars.wingLength;
					sLeaser.sprites[WingSprite(k, j)].rotation = Custom.AimFromOneVectorToAnother(p2, Vector2.Lerp(wings[k, j].lastPos, wings[k, j].pos, timeStacker)) - 90f * ((k == 0) ? (-1f) : 1f);
				}
			}
		}
		Vector2 vector3 = Vector2.Lerp(cicada.mainBodyChunk.lastPos, cicada.mainBodyChunk.pos, timeStacker) + normalized * (8f + 2f * Mathf.Abs(p.x) * (1f - lookDir.magnitude));
		vector3 += vector2 * p.x * (3f - 2f * lookDir.magnitude);
		vector3 += lookDir * 6f;
		for (int l = 0; l < 2; l++)
		{
			for (int m = 0; m < 2; m++)
			{
				Vector2 vector4 = Vector2.Lerp(tentacles[m, l].lastPos, tentacles[m, l].pos, timeStacker);
				Vector2 vector5 = vector3;
				if (l == 0)
				{
					vector5 += normalized * 3f * Mathf.Abs(p.x);
				}
				float num11 = 2f * iVars.tentacleThickness;
				float num12 = 1f;
				for (int n = 0; n < 3; n++)
				{
					Vector2 vector6 = vector4;
					if (n < 2)
					{
						vector6 = Vector2.Lerp(vector3, vector4, (float)(n + 1) / 3f);
						vector6 += normalized * num12 * 15f * Mathf.InverseLerp(1f, -1f, Vector2.Dot(normalized, (vector6 - vector5).normalized));
						if (l == 1 && n == 0)
						{
							vector6 += vector2 * p.x * num12 * 5f;
						}
						vector6 += vector2 * Mathf.Abs(p.y) * ((m == 0) ? 1f : (-1f)) * num12 * ((l == 1) ? 3f : 1.5f);
					}
					num12 *= 0.5f;
					Vector2 normalized2 = (vector6 - vector5).normalized;
					Vector2 vector7 = Custom.PerpendicularVector(normalized2);
					float num13 = Vector2.Distance(vector6, vector5) / ((n == 0) ? 1f : 5f);
					float num14 = ((n < 1) ? 1.2f : 0.8f) * iVars.tentacleThickness;
					(sLeaser.sprites[TentacleSprite(m, l)] as TriangleMesh).MoveVertice(n * 4, vector5 - vector7 * (num14 + num11) * 0.5f + normalized2 * num13 - camPos);
					(sLeaser.sprites[TentacleSprite(m, l)] as TriangleMesh).MoveVertice(n * 4 + 1, vector5 + vector7 * (num14 + num11) * 0.5f + normalized2 * num13 - camPos);
					if (n < 2)
					{
						(sLeaser.sprites[TentacleSprite(m, l)] as TriangleMesh).MoveVertice(n * 4 + 2, vector6 - vector7 * num14 - normalized2 * num13 - camPos);
						(sLeaser.sprites[TentacleSprite(m, l)] as TriangleMesh).MoveVertice(n * 4 + 3, vector6 + vector7 * num14 - normalized2 * num13 - camPos);
					}
					else
					{
						(sLeaser.sprites[TentacleSprite(m, l)] as TriangleMesh).MoveVertice(n * 4 + 2, vector6 + normalized2 * 2.1f - camPos);
					}
					num11 = num14;
					vector5 = vector6;
				}
			}
		}
		sLeaser.sprites[EyesBSprite].isVisible = blinkCounter >= 0;
		sLeaser.sprites[EyesASprite].color = ((blinkCounter > 0) ? eyeColor : shieldColor);
		sLeaser.sprites[HighlightSprite].x = Mathf.Lerp(cicada.bodyChunks[1].lastPos.x, cicada.bodyChunks[1].pos.x, timeStacker) - 2f - camPos.x;
		sLeaser.sprites[HighlightSprite].y = Mathf.Lerp(cicada.bodyChunks[1].lastPos.y, cicada.bodyChunks[1].pos.y, timeStacker) + 3f - camPos.y;
		sLeaser.sprites[HighlightSprite].rotation = num + 12f;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = ((!cicada.gender) ? Color.Lerp(HSLColor.Lerp(cicada.iVars.color, new HSLColor(cicada.iVars.color.hue, 1f, 0f), 0.8f).rgb, palette.blackColor, 0.85f) : Color.Lerp(HSLColor.Lerp(iVars.color, new HSLColor(cicada.iVars.color.hue, 0f, 1f), 0.8f).rgb, palette.fogColor, 0.1f));
		if (cicada.gender)
		{
			shieldColor = Color.Lerp(color, new HSLColor(cicada.iVars.color.hue, 0.5f, 0.4f).rgb, 0.8f);
		}
		else
		{
			shieldColor = Color.Lerp(color, new HSLColor(cicada.iVars.color.hue, 0.5f, 0.5f).rgb, 0.4f);
		}
		if (cicada.gender)
		{
			sLeaser.sprites[HighlightSprite].color = Color.Lerp(color, new Color(1f, 1f, 1f), 0.7f);
		}
		else
		{
			sLeaser.sprites[HighlightSprite].color = Color.Lerp(color, cicada.iVars.color.rgb, 0.07f);
		}
		sLeaser.sprites[BodySprite].color = color;
		sLeaser.sprites[HeadSprite].color = color;
		sLeaser.sprites[ShieldSprite].color = shieldColor;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[WingSprite(i, j)].color = shieldColor;
				sLeaser.sprites[TentacleSprite(i, j)].color = color;
			}
		}
		if (cicada.gender)
		{
			eyeColor = Color.Lerp(color, palette.blackColor, 0.8f);
			sLeaser.sprites[EyesASprite].color = eyeColor;
			sLeaser.sprites[EyesBSprite].color = iVars.color.rgb;
		}
		else
		{
			eyeColor = iVars.color.rgb;
			sLeaser.sprites[EyesASprite].color = eyeColor;
			sLeaser.sprites[EyesBSprite].color = palette.blackColor;
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
		for (int i = 0; i < TotalSprites; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score;
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return cicada.AI.focusCreature;
	}

	public void LookAtNothing()
	{
	}
}
