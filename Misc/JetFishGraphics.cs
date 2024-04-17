using System;
using RWCustom;
using UnityEngine;

public class JetFishGraphics : GraphicsModule
{
	private JetFish fish;

	public TailSegment[,] tails;

	public GenericBodyPart[] flippers;

	public GenericBodyPart[,] whiskers;

	public Vector2[] whiskerDirections;

	public float[,] whiskerProps;

	public float swim;

	public Vector2 zRotation;

	private Vector2 lastZRotation;

	private float airEyes;

	private static float[] flipperGraphConPointHeights = new float[5] { 10f, 10f, 11f, 17f, 9f };

	private float flipperGraphWidth;

	private int BehindEyeSprite => 0;

	private int BodySprite => 1;

	private int TotalSprites => TotalWhiskerSprites + 10;

	private int TotalWhiskerSprites => fish.iVars.whiskers * 2;

	public bool Albino => fish.albino;

	private int WhiskerSprite(int side, int whisker)
	{
		return 2 + whisker * 2 + side;
	}

	private int EyeSprite(int eye, int part)
	{
		return TotalWhiskerSprites + 2 + eye * 2 + part;
	}

	private int TentacleSprite(int tentacle)
	{
		return TotalWhiskerSprites + 6 + tentacle;
	}

	private int FlipperSprite(int flipper)
	{
		return TotalWhiskerSprites + 8 + flipper;
	}

	public JetFishGraphics(JetFish ow)
		: base(ow, internalContainers: false)
	{
		fish = ow;
		bodyParts = new BodyPart[14 + fish.iVars.whiskers * 2];
		tails = new TailSegment[2, 6];
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < tails.GetLength(1); j++)
			{
				tails[i, j] = new TailSegment(this, Mathf.Lerp(3f, 1f, (float)j / (float)(tails.GetLength(1) - 1)), 15f, (j == 0) ? null : tails[i, j - 1], 0.5f, 0.99f, 0.4f, pullInPreviousPosition: false);
				bodyParts[i * 6 + j] = tails[i, j];
			}
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(fish.iVars.whiskerSeed);
		if (fish.iVars.whiskers > 0)
		{
			whiskers = new GenericBodyPart[2, fish.iVars.whiskers];
			whiskerDirections = new Vector2[fish.iVars.whiskers];
			whiskerProps = new float[fish.iVars.whiskers, 5];
			for (int k = 0; k < fish.iVars.whiskers; k++)
			{
				whiskers[0, k] = new GenericBodyPart(this, 1f, 0.6f, 0.9f, fish.mainBodyChunk);
				whiskers[1, k] = new GenericBodyPart(this, 1f, 0.6f, 0.9f, fish.mainBodyChunk);
				whiskerDirections[k] = Custom.DegToVec(Mathf.Lerp(4f, 80f, UnityEngine.Random.value));
				whiskerProps[k, 0] = Mathf.Lerp(7f, 40f, Mathf.Pow(UnityEngine.Random.value, 2f));
				whiskerProps[k, 1] = Mathf.Lerp(-1f, 0.5f, UnityEngine.Random.value);
				whiskerProps[k, 2] = Mathf.Lerp(5f, 720f, Mathf.Pow(UnityEngine.Random.value, 1.5f)) / whiskerProps[k, 0];
				whiskerProps[k, 3] = UnityEngine.Random.value;
				whiskerProps[k, 4] = Mathf.Lerp(0.6f, 1.6f, Mathf.Pow(UnityEngine.Random.value, 2f));
				bodyParts[14 + k * 2] = whiskers[0, k];
				bodyParts[14 + k * 2 + 1] = whiskers[1, k];
			}
		}
		UnityEngine.Random.state = state;
		flippers = new GenericBodyPart[2];
		for (int l = 0; l < 2; l++)
		{
			flippers[l] = new GenericBodyPart(this, 1f, 0.7f, 0.99f, fish.bodyChunks[1]);
			bodyParts[12 + l] = flippers[l];
		}
	}

	public override void Update()
	{
		base.Update();
		if (culled)
		{
			return;
		}
		if (fish.Consious)
		{
			swim -= Custom.LerpMap(fish.swimSpeed, 1.6f, 5f, 1f / 30f, 1f / 15f);
		}
		tails[0, 0].connectedPoint = fish.bodyChunks[1].pos;
		tails[1, 0].connectedPoint = fish.bodyChunks[1].pos;
		Vector2 vector = Custom.DirVec(fish.bodyChunks[1].pos, fish.bodyChunks[0].pos);
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		lastZRotation = zRotation;
		if (Mathf.Abs(vector.x) > 0.5f && fish.Consious)
		{
			zRotation = Vector2.Lerp(zRotation, new Vector2((vector.x > 0f) ? (-1f) : 1f, 0f), 0.2f);
		}
		else
		{
			zRotation = Vector2.Lerp(zRotation, -vector, 0.5f);
		}
		zRotation = zRotation.normalized;
		if (fish.Consious)
		{
			float num = 1f - fish.bodyChunks[1].submersion;
			if (airEyes < num)
			{
				airEyes = Mathf.Min(airEyes + 0.1f, num);
			}
			else
			{
				airEyes = Mathf.Max(airEyes - 1f / 30f, num);
			}
		}
		for (int i = 0; i < 2; i++)
		{
			flippers[i].Update();
			flippers[i].ConnectToPoint(fish.bodyChunks[1].pos, (flipperGraphWidth + 7f) * fish.iVars.flipperSize, push: false, 0f, fish.bodyChunks[1].vel, 0.3f, 0f);
			Vector2 vector3 = vector2 * zRotation.y * ((i == 0) ? (-1f) : 1f);
			vector3 += new Vector2(0f, -0.5f) * Mathf.Abs(zRotation.x);
			vector3 += vector * fish.iVars.flipperOrientation * 1.5f;
			if (fish.Consious)
			{
				if (i == 0 == zRotation.x < 0f)
				{
					vector3 += vector * Mathf.Sin(swim * (float)Math.PI * 2f) * 0.3f * (1f - fish.jetActive);
				}
				else
				{
					vector3 += vector * Mathf.Cos(swim * (float)Math.PI * 2f) * 0.3f * (1f - fish.jetActive);
				}
				vector3 = Vector2.Lerp(vector3, -vector, fish.jetActive * fish.jetWater);
			}
			flippers[i].vel += (fish.bodyChunks[1].pos + vector3 * (flipperGraphWidth + 7f) * fish.iVars.flipperSize - flippers[i].pos) / (fish.Consious ? 8f : 16f);
			if (fish.room.PointSubmerged(flippers[i].pos))
			{
				flippers[i].vel *= 0.9f;
			}
			else
			{
				flippers[i].vel.y -= 0.6f;
			}
			if (fish.iVars.whiskers > 0)
			{
				for (int j = 0; j < fish.iVars.whiskers; j++)
				{
					whiskers[i, j].vel += whiskerDir(i, j, zRotation, vector) * whiskerProps[j, 2];
					if (fish.room.PointSubmerged(whiskers[i, j].pos))
					{
						whiskers[i, j].vel *= 0.8f;
					}
					else
					{
						whiskers[i, j].vel.y -= 0.6f;
					}
					whiskers[i, j].Update();
					whiskers[i, j].ConnectToPoint(fish.mainBodyChunk.pos + vector * 5f + whiskerDir(i, j, zRotation, vector) * 5f, whiskerProps[j, 0], push: false, 0f, fish.mainBodyChunk.vel, 0f, 0f);
				}
			}
			for (int k = 0; k < tails.GetLength(1); k++)
			{
				tails[i, k].Update();
				float num2 = Mathf.InverseLerp(0f, tails.GetLength(1) - 1, k);
				if (!Custom.DistLess(tails[i, k].pos, fish.bodyChunks[1].pos, 15f * (float)(k + 1)))
				{
					tails[i, k].pos = fish.bodyChunks[1].pos + Custom.DirVec(fish.bodyChunks[1].pos, tails[i, k].pos) * 15f * (k + 1);
				}
				Vector2 pos = fish.bodyChunks[0].pos;
				if (k == 1)
				{
					pos = fish.bodyChunks[1].pos;
				}
				else if (k > 1)
				{
					pos = tails[i, k - 2].pos;
				}
				pos = Vector2.Lerp(pos, fish.bodyChunks[0].pos, 0.2f);
				float num3 = fish.jetActive;
				if (fish.room.PointSubmerged(tails[i, k].pos))
				{
					tails[i, k].vel *= 0.7f;
					num3 = Mathf.Lerp(num3, 0f, 0.5f);
				}
				else
				{
					tails[i, k].vel.y -= 0.9f * Mathf.Pow((float)k / (float)(tails.GetLength(1) - 1), 3f);
				}
				tails[i, k].vel += vector2 * Mathf.Sin((swim + (float)k / 5f) * (float)Math.PI * 2f) * ((i == 0) ? 1f : (-1f)) * Mathf.Pow(1f - num2, 2f) * Custom.LerpMap(fish.swimSpeed, 1.6f, 5f, 8f, 16f) * (1f - num3);
				tails[i, k].vel -= vector * (0.2f * (1f - num3) + Mathf.Pow(Mathf.InverseLerp(0.5f, 0f, num2), 2f) * Mathf.Lerp(27f, 11f, num3));
				float num4 = 30f + Mathf.Sin(Mathf.Pow(num2, 1f) * (float)Math.PI * -2f) * -100f;
				tails[i, k].vel -= Custom.DegToVec(Custom.AimFromOneVectorToAnother(fish.bodyChunks[1].pos, fish.bodyChunks[0].pos) + num4 * ((i == 0) ? 1f : (-1f))) * Mathf.Lerp(12f, 6f, num2) * num3;
				tails[i, k].connectionRad = Mathf.Lerp(10f, 0.5f, Mathf.Lerp(0f, num3, Mathf.Pow(num2, 0.2f))) * Mathf.Lerp(0.5f, 1.5f, fish.iVars.tentacleLength);
				tails[i, k].rad = Mathf.Lerp(TentacleContour(num2, k), Mathf.Lerp(8f, 2f, num2) * (0.5f + 0.5f * fish.jetWater), num3) * Mathf.Lerp(0.7f, 1.2f, fish.iVars.tentacleFatness);
			}
		}
	}

	private Vector2 whiskerDir(int side, int m, Vector2 zRot, Vector2 bodyDir)
	{
		return Custom.RotateAroundOrigo(new Vector2(((side == 0) ? (-1f) : 1f) * Mathf.Abs(zRot.y) * whiskerDirections[m].x + zRot.x * whiskerProps[m, 1], whiskerDirections[m].y).normalized, Custom.VecToDeg(bodyDir));
	}

	private float TentacleContour(float f, int i)
	{
		switch (fish.iVars.tentacleContour)
		{
		case 0:
			return Mathf.Lerp(5f, 1f, Mathf.Pow(f, 0.5f));
		case 1:
			if (i == tails.GetLength(1) - 2)
			{
				return 3.5f;
			}
			return Mathf.Lerp(4f, 1f, Mathf.Pow(f, 0.5f));
		default:
			return Mathf.Lerp(4f, 1f, Mathf.Pow(f, 1.5f));
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[BodySprite] = new FSprite("pixel");
		sLeaser.sprites[BodySprite].scale = Mathf.Lerp(0.8f, 1f, fish.iVars.fatness);
		sLeaser.sprites[BehindEyeSprite] = new FSprite("JetFishEyeA");
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < fish.iVars.whiskers; j++)
			{
				sLeaser.sprites[WhiskerSprite(i, j)] = TriangleMesh.MakeLongMesh(4, pointyTip: true, Albino);
			}
		}
		for (int k = 0; k < 2; k++)
		{
			sLeaser.sprites[TentacleSprite(k)] = TriangleMesh.MakeLongMesh(tails.GetLength(1), pointyTip: true, Albino);
			sLeaser.sprites[FlipperSprite(k)] = new FSprite("JetFishFlipper" + fish.iVars.flipper);
			flipperGraphWidth = Futile.atlasManager.GetElementWithName("JetFishFlipper" + fish.iVars.flipper).sourcePixelSize.x;
			sLeaser.sprites[FlipperSprite(k)].anchorX = 0f;
			sLeaser.sprites[FlipperSprite(k)].anchorY = flipperGraphConPointHeights[fish.iVars.flipper] / Futile.atlasManager.GetElementWithName("JetFishFlipper" + fish.iVars.flipper).sourcePixelSize.y;
			for (int l = 0; l < 2; l++)
			{
				sLeaser.sprites[EyeSprite(k, l)] = new FSprite("JetFishEye" + ((l == 0) ? "A" : "B"));
			}
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = palette.blackColor;
		if (Albino)
		{
			color = Color.Lerp(color, Color.white, 0.87f);
		}
		sLeaser.sprites[BodySprite].color = color;
		for (int i = 0; i < 2; i++)
		{
			if (Albino)
			{
				for (int j = 0; j < (sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors.Length; j++)
				{
					(sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors[j] = Color.Lerp(color, Color.red, Mathf.Pow(Mathf.InverseLerp(3f, (sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors.Length - 1, j), 2f) * 0.5f);
				}
			}
			else
			{
				sLeaser.sprites[TentacleSprite(i)].color = color;
			}
			sLeaser.sprites[EyeSprite(i, 1)].color = color;
			sLeaser.sprites[EyeSprite(i, 1)].color = color;
			sLeaser.sprites[FlipperSprite(i)].color = color;
			for (int k = 0; k < fish.iVars.whiskers; k++)
			{
				if (Albino)
				{
					for (int l = 0; l < (sLeaser.sprites[WhiskerSprite(i, k)] as TriangleMesh).verticeColors.Length; l++)
					{
						(sLeaser.sprites[WhiskerSprite(i, k)] as TriangleMesh).verticeColors[l] = Color.Lerp(color, Color.red, Mathf.Pow(Mathf.InverseLerp(3f, (sLeaser.sprites[WhiskerSprite(i, k)] as TriangleMesh).verticeColors.Length - 1, l), 0.5f) * 0.65f);
					}
				}
				else
				{
					sLeaser.sprites[WhiskerSprite(i, k)].color = color;
				}
			}
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		Vector2 vector = Vector3.Slerp(lastZRotation, zRotation, timeStacker);
		Vector2 vector2 = Vector2.Lerp(Vector2.Lerp(fish.bodyChunks[0].lastPos, fish.bodyChunks[0].pos, timeStacker), Vector2.Lerp(fish.bodyChunks[1].lastPos, fish.bodyChunks[1].pos, timeStacker), 0.3f);
		Vector2 normalized = (Vector2.Lerp(fish.bodyChunks[0].lastPos, fish.bodyChunks[0].pos, timeStacker) - Vector2.Lerp(fish.bodyChunks[1].lastPos, fish.bodyChunks[1].pos, timeStacker)).normalized;
		Vector2 vector3 = Custom.PerpendicularVector(-normalized);
		float num = Custom.AimFromOneVectorToAnother(normalized, -normalized);
		float num2 = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector);
		int num3 = Custom.IntClamp(8 - (int)(Mathf.Abs(num2 / 180f) * 9f), 0, 8);
		float num4 = (float)(8 - num3) * Mathf.Sign(num2) * 22.5f;
		sLeaser.sprites[BodySprite].x = vector2.x - camPos.x;
		sLeaser.sprites[BodySprite].y = vector2.y - camPos.y;
		sLeaser.sprites[BodySprite].element = Futile.atlasManager.GetElementWithName("JetFish" + num3);
		sLeaser.sprites[BodySprite].rotation = num - num4;
		sLeaser.sprites[BodySprite].scaleX = ((num2 > 0f) ? (-1f) : 1f);
		for (int i = 0; i < 2; i++)
		{
			Vector2 vector4 = vector2 + normalized * 13f + vector3 * Mathf.Cos(num2 / 360f * (float)Math.PI * 2f) * Mathf.Lerp(7.5f, 5f, airEyes) * ((i == 0 == num2 > 0f) ? (-1f) : 1f);
			if (i == 0)
			{
				sLeaser.sprites[BehindEyeSprite].x = vector4.x - camPos.x;
				sLeaser.sprites[BehindEyeSprite].y = vector4.y - camPos.y;
				sLeaser.sprites[BehindEyeSprite].scale = Mathf.Lerp(1f, 0.4f, airEyes);
				sLeaser.sprites[BehindEyeSprite].color = (rCam.room.PointSubmerged(vector4 + new Vector2(0f, -5f)) ? new Color(0f, 0.003921569f, 0f) : fish.iVars.eyeColor);
				sLeaser.sprites[EyeSprite(i, 0)].isVisible = Mathf.Abs(Mathf.Cos(num2 / 360f * (float)Math.PI * 2f)) > 0.9f;
			}
			sLeaser.sprites[EyeSprite(i, 0)].color = (rCam.room.PointSubmerged(vector4 + new Vector2(0f, 5f)) ? new Color(0f, 0.003921569f, 0f) : fish.iVars.eyeColor);
			sLeaser.sprites[EyeSprite(i, 0)].scale = Mathf.Lerp(1f, 0.4f, airEyes);
			sLeaser.sprites[EyeSprite(i, 1)].scale = Mathf.Lerp(1f, 0f, Mathf.Pow(airEyes, 0.4f));
			sLeaser.sprites[EyeSprite(i, 0)].x = vector4.x - camPos.x;
			sLeaser.sprites[EyeSprite(i, 0)].y = vector4.y - camPos.y;
			sLeaser.sprites[EyeSprite(i, 1)].x = vector4.x - camPos.x;
			sLeaser.sprites[EyeSprite(i, 1)].y = vector4.y - camPos.y;
			sLeaser.sprites[FlipperSprite(i)].x = Mathf.Lerp(flippers[i].lastPos.x, flippers[i].pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[FlipperSprite(i)].y = Mathf.Lerp(flippers[i].lastPos.y, flippers[i].pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[FlipperSprite(i)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(flippers[i].lastPos, flippers[i].pos, timeStacker), vector2) - 90f;
			sLeaser.sprites[FlipperSprite(i)].scaleY = Mathf.Sign(Custom.DistanceToLine(Vector2.Lerp(flippers[i].lastPos, flippers[i].pos, timeStacker), vector2 - normalized, vector2 + normalized)) * fish.iVars.flipperSize;
			sLeaser.sprites[FlipperSprite(i)].scaleX = Mathf.Lerp(Vector2.Distance(Vector2.Lerp(flippers[i].lastPos, flippers[i].pos, timeStacker), vector2) / flipperGraphWidth, fish.iVars.flipperSize, 0.5f);
			Vector2 vector5 = Vector2.Lerp(fish.bodyChunks[1].lastPos, fish.bodyChunks[1].pos, timeStacker);
			float num5 = fish.bodyChunks[1].rad;
			for (int j = 0; j < tails.GetLength(1); j++)
			{
				Vector2 vector6 = Vector2.Lerp(tails[i, j].lastPos, tails[i, j].pos, timeStacker);
				Vector2 normalized2 = (vector6 - vector5).normalized;
				Vector2 vector7 = Custom.PerpendicularVector(normalized2);
				float num6 = Vector2.Distance(vector6, vector5) / 5f;
				(sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).MoveVertice(j * 4, vector5 - vector7 * (num5 + tails[i, j].StretchedRad) * 0.5f + normalized2 * num6 - camPos);
				(sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).MoveVertice(j * 4 + 1, vector5 + vector7 * (num5 + tails[i, j].StretchedRad) * 0.5f + normalized2 * num6 - camPos);
				if (j < tails.GetLength(1) - 1)
				{
					(sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).MoveVertice(j * 4 + 2, vector6 - vector7 * tails[i, j].StretchedRad - normalized2 * num6 - camPos);
					(sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).MoveVertice(j * 4 + 3, vector6 + vector7 * tails[i, j].StretchedRad - normalized2 * num6 - camPos);
				}
				else
				{
					(sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).MoveVertice(j * 4 + 2, vector6 - camPos);
				}
				num5 = tails[i, j].StretchedRad;
				vector5 = vector6;
			}
			for (int k = 0; k < fish.iVars.whiskers; k++)
			{
				for (int l = 0; l < 2; l++)
				{
					Vector2 vector8 = Vector2.Lerp(whiskers[l, k].lastPos, whiskers[l, k].pos, timeStacker);
					Vector2 vector9 = whiskerDir(l, k, vector, normalized);
					Vector2 vector10 = Vector2.Lerp(fish.bodyChunks[0].lastPos, fish.bodyChunks[0].pos, timeStacker) + normalized * Mathf.Lerp(10f, 5f, whiskerProps[k, 3]) + vector9 * 5f * whiskerProps[k, 3];
					vector9 = (vector9 + normalized).normalized;
					vector5 = vector10;
					num5 = whiskerProps[k, 4];
					float num7 = 1f;
					for (int m = 0; m < 4; m++)
					{
						Vector2 vector11;
						if (m < 3)
						{
							vector11 = Vector2.Lerp(vector10, vector8, (float)(m + 1) / 4f);
							vector11 += vector9 * num7 * whiskerProps[k, 0] * 0.2f;
						}
						else
						{
							vector11 = vector8;
						}
						num7 *= 0.7f;
						Vector2 normalized3 = (vector11 - vector5).normalized;
						Vector2 vector12 = Custom.PerpendicularVector(normalized3);
						float num8 = Vector2.Distance(vector11, vector5) / ((m == 0) ? 1f : 5f);
						float num9 = Custom.LerpMap(m, 0f, 3f, whiskerProps[k, 4], 0.5f);
						(sLeaser.sprites[WhiskerSprite(l, k)] as TriangleMesh).MoveVertice(m * 4, vector5 - vector12 * (num9 + num5) * 0.5f + normalized3 * num8 - camPos);
						(sLeaser.sprites[WhiskerSprite(l, k)] as TriangleMesh).MoveVertice(m * 4 + 1, vector5 + vector12 * (num9 + num5) * 0.5f + normalized3 * num8 - camPos);
						if (m < 3)
						{
							(sLeaser.sprites[WhiskerSprite(l, k)] as TriangleMesh).MoveVertice(m * 4 + 2, vector11 - vector12 * num9 - normalized3 * num8 - camPos);
							(sLeaser.sprites[WhiskerSprite(l, k)] as TriangleMesh).MoveVertice(m * 4 + 3, vector11 + vector12 * num9 - normalized3 * num8 - camPos);
						}
						else
						{
							(sLeaser.sprites[WhiskerSprite(l, k)] as TriangleMesh).MoveVertice(m * 4 + 2, vector11 + normalized3 * 2.1f - camPos);
						}
						num5 = num9;
						vector5 = vector11;
					}
				}
			}
		}
	}
}
