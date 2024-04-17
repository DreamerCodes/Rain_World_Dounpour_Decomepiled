using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class HazerGraphics : GraphicsModule
{
	public ChunkDynamicSoundLoop soundLoop;

	public float deadColor;

	public float lastDeadColor;

	public Vector2 lookDir;

	public Vector2 lastLookDir;

	public float eyeOpen;

	public float lastEyeOpen;

	public int blink;

	public List<Vector2[,]> tentacles;

	public HSLColor skinColor;

	public HSLColor secondColor;

	public HSLColor eyeColor;

	public BodyChunk lookAtObj;

	public Vector2 lookPos;

	public Vector2 smallEyeMovements;

	public float pupSize;

	public float lastPupSize;

	public float pupGetToSize;

	public Color camoColor;

	public Color camoPickupColor;

	public float camo;

	public float lastCamo;

	public float camoGetTo;

	public bool camoPickup;

	public Vector2[] scales;

	private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public Hazer bug => base.owner as Hazer;

	public int MeshSprite => 0;

	public int BodySprite => 1;

	public int ClosedEyeSprite => tentacles.Count + scales.Length + 2;

	public int EyeSprite => tentacles.Count + scales.Length + 3;

	public int PupilSprite => tentacles.Count + scales.Length + 4;

	public int EyeDotSprite => tentacles.Count + scales.Length + 5;

	public int EyeHighLightSprite => tentacles.Count + scales.Length + 6;

	public int TotalSprites => tentacles.Count + scales.Length + 7;

	public int ScaleSprite(int s)
	{
		return 2 + s;
	}

	public int TentacleSprite(int t)
	{
		return 2 + scales.Length + t;
	}

	public HazerGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
		skinColor = new HSLColor(((UnityEngine.Random.value < 0.5f) ? 0.348f : 0.56f) + Mathf.Lerp(-0.03f, 0.03f, UnityEngine.Random.value), 0.6f + UnityEngine.Random.value * 0.1f, 0.7f + UnityEngine.Random.value * 0.1f);
		secondColor = new HSLColor(skinColor.hue + Mathf.Lerp(-0.1f, 0.1f, UnityEngine.Random.value), Mathf.Lerp(skinColor.saturation, 1f, UnityEngine.Random.value), skinColor.lightness - UnityEngine.Random.value * 0.4f);
		eyeColor = new HSLColor((skinColor.hue + secondColor.hue) * 0.5f + 0.5f, 1f, 0.4f + UnityEngine.Random.value * 0.1f);
		tentacles = new List<Vector2[,]>();
		int num = ((UnityEngine.Random.value < 0.5f) ? UnityEngine.Random.Range(3, 6) : UnityEngine.Random.Range(2, 7));
		for (int i = 0; i < num; i++)
		{
			float f = Mathf.Lerp(34f / (float)num * Mathf.Lerp(0.7f, 1.2f, UnityEngine.Random.value), 8f, 0.2f);
			tentacles.Add(new Vector2[Math.Max(2, Mathf.RoundToInt(f)), 4]);
			for (int j = 0; j < tentacles[i].GetLength(0); j++)
			{
				tentacles[i][j, 3] = Custom.RNV() * UnityEngine.Random.value;
			}
		}
		scales = new Vector2[UnityEngine.Random.Range(4, 14)];
		for (int k = 0; k < scales.Length; k++)
		{
			scales[k] = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
		}
		UnityEngine.Random.state = state;
		camoColor = skinColor.rgb;
		camoPickupColor = camoColor;
		camoGetTo = UnityEngine.Random.value;
		deadColor = (bug.State.alive ? 0f : 1f);
		lastDeadColor = deadColor;
		eyeOpen = 1f;
		lastEyeOpen = eyeOpen;
		Reset();
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < tentacles.Count; i++)
		{
			for (int j = 0; j < tentacles[i].GetLength(0); j++)
			{
				tentacles[i][j, 0] = bug.ChunkInOrder(0).pos + new Vector2(0f, 5f * (float)i);
				tentacles[i][j, 1] = tentacles[i][j, 0];
				tentacles[i][j, 2] *= 0f;
			}
		}
	}

	private float ObjectInterestingScore(BodyChunk randomChunk)
	{
		if (randomChunk == null)
		{
			return 0f;
		}
		return (1f + Vector2.Distance(randomChunk.lastLastPos, randomChunk.pos) * 0.4f) * (1f + randomChunk.owner.TotalMass) * 0.5f / Vector2.Distance(bug.mainBodyChunk.pos, randomChunk.pos);
	}

	public override void Update()
	{
		base.Update();
		lastDeadColor = deadColor;
		lastLookDir = lookDir;
		lastEyeOpen = eyeOpen;
		lastPupSize = pupSize;
		lastCamo = camo;
		if (bug.dead)
		{
			deadColor = Mathf.Min(1f, deadColor + 1f / 154f);
		}
		if (!bug.dead && !bug.spraying && !bug.tossed)
		{
			if (bug.grabbedBy.Count == 0 && bug.moveCounter < 0 && bug.bodyChunks[0].ContactPoint.y < 0)
			{
				if (UnityEngine.Random.value < 1f / 17f)
				{
					camoGetTo = Mathf.Max(camoGetTo, UnityEngine.Random.value);
				}
			}
			else if (UnityEngine.Random.value < 1f / 17f)
			{
				camoGetTo = Mathf.Min(camoGetTo, UnityEngine.Random.value);
			}
		}
		else
		{
			camoGetTo = 0f;
		}
		camo = Custom.LerpAndTick(camo, camoGetTo, 0.07f, UnityEngine.Random.value / ((camo < camoGetTo) ? 240f : 40f));
		if (UnityEngine.Random.value < camo)
		{
			camoPickup = true;
		}
		if (soundLoop == null && bug.spraying)
		{
			soundLoop = new ChunkDynamicSoundLoop(bug.bodyChunks[1]);
			soundLoop.sound = SoundID.Hazer_Squirt_Smoke_LOOP;
		}
		else if (soundLoop != null)
		{
			soundLoop.Volume = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(bug.inkLeft * (float)Math.PI)), 0.2f);
			soundLoop.Pitch = 0.6f + 0.4f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(bug.inkLeft * (float)Math.PI)), 0.7f);
			soundLoop.Update();
			if (!bug.spraying)
			{
				if (soundLoop.emitter != null)
				{
					soundLoop.emitter.slatedForDeletetion = true;
				}
				soundLoop = null;
			}
		}
		if (UnityEngine.Random.value < 0.05f)
		{
			BodyChunk bodyChunk = null;
			int num = UnityEngine.Random.Range(0, bug.room.physicalObjects.Length);
			if (bug.room.physicalObjects[num].Count > 0)
			{
				PhysicalObject physicalObject = bug.room.physicalObjects[num][UnityEngine.Random.Range(0, bug.room.physicalObjects[num].Count)];
				bodyChunk = physicalObject.bodyChunks[UnityEngine.Random.Range(0, physicalObject.bodyChunks.Length)];
				if (bodyChunk.owner != bug && ObjectInterestingScore(bodyChunk) > ObjectInterestingScore(lookAtObj) && Custom.DistLess(bug.mainBodyChunk.pos, bodyChunk.pos, 400f))
				{
					lookAtObj = bodyChunk;
				}
			}
		}
		if (lookAtObj != null)
		{
			if (lookAtObj.owner.room != bug.room || lookAtObj.owner.slatedForDeletetion || !Custom.DistLess(lookAtObj.pos, bug.mainBodyChunk.pos, 600f) || UnityEngine.Random.value < 0.02f)
			{
				lookAtObj = null;
			}
			else
			{
				lookPos = lookAtObj.pos;
			}
		}
		else if (UnityEngine.Random.value < 1f / 7f)
		{
			if (UnityEngine.Random.value < 0.1f)
			{
				Vector2 pos = bug.mainBodyChunk.pos + Custom.RNV() * UnityEngine.Random.value * 600f;
				if (!bug.room.GetTile(pos).Solid)
				{
					lookPos = pos;
				}
			}
			else
			{
				Vector2 pos2 = lookPos + Custom.RNV() * UnityEngine.Random.value * 100f;
				if (!bug.room.GetTile(pos2).Solid)
				{
					lookPos = pos2;
				}
			}
		}
		if (bug.dead)
		{
			eyeOpen = Custom.LerpAndTick(eyeOpen, 1f, 0.08f, 1f / 30f);
			lookDir *= 0.9f;
		}
		else
		{
			smallEyeMovements *= 0.94f;
			if (UnityEngine.Random.value < 1f / 3f)
			{
				smallEyeMovements = Custom.RNV() * UnityEngine.Random.value * Mathf.Min(Vector2.Distance(bug.mainBodyChunk.pos, lookPos) * 0.5f, 120f);
			}
			if (lookAtObj != null)
			{
				lookDir = Vector2.Lerp(lookDir, Vector2.ClampMagnitude((lookPos + smallEyeMovements - bug.ChunkInOrder(0).pos) / 60f, 1f), 0.3f);
			}
			blink--;
			if (blink < -UnityEngine.Random.Range(80, 1000) || (UnityEngine.Random.value < 0.05f && blink < 0 && blink > -10))
			{
				blink = UnityEngine.Random.Range(6, 27);
			}
			eyeOpen = Custom.LerpAndTick(eyeOpen, (blink < 0 && (bug.moveCounter < 0 || bug.swim > 0.5f) && !bug.spraying && bug.swallowed < 0.5f) ? 1f : 0f, 0.08f, 0.1f);
			pupSize = Custom.LerpAndTick(pupSize, pupGetToSize, 0.04f, 1f / 21f);
			if (UnityEngine.Random.value < 1f / 41f)
			{
				pupGetToSize = ((UnityEngine.Random.value < 0.5f) ? 1f : UnityEngine.Random.value);
			}
		}
		float degAng = Custom.AimFromOneVectorToAnother(bug.ChunkInOrder(1).pos, bug.ChunkInOrder(0).pos);
		Vector2 vector = Custom.DirVec(bug.ChunkInOrder(1).pos, bug.ChunkInOrder(0).pos);
		for (int i = 0; i < tentacles.Count; i++)
		{
			Vector2 vector2 = TentacleDir(i, 1f, con: false);
			for (int j = 0; j < tentacles[i].GetLength(0); j++)
			{
				float num2 = (float)j / (float)(tentacles[i].GetLength(0) - 1);
				tentacles[i][j, 1] = tentacles[i][j, 0];
				tentacles[i][j, 0] += tentacles[i][j, 2];
				tentacles[i][j, 2] *= 1f - 0.5f * num2;
				tentacles[i][j, 2] += (Vector2)Vector3.Slerp(vector, vector2, Mathf.Pow(num2, 1f - 0.7f * bug.swim)) * (2.5f + bug.swim) * Mathf.Pow(1f - num2, 1.5f);
				if (j > 1 && num2 > camo && bug.room.GetTile(tentacles[i][j, 0]).Solid)
				{
					SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(tentacles[i][j, 0], tentacles[i][j, 1], tentacles[i][j, 2], 1f, new IntVector2(0, 0), goThroughFloors: true);
					cd = SharedPhysics.VerticalCollision(bug.room, cd);
					cd = SharedPhysics.HorizontalCollision(bug.room, cd);
					tentacles[i][j, 0] = cd.pos;
					tentacles[i][j, 2] = cd.vel;
				}
				if (bug.room.PointSubmerged(tentacles[i][j, 0]))
				{
					tentacles[i][j, 2] *= 0.8f;
					tentacles[i][j, 2].y += 0.3f * num2;
				}
				else
				{
					tentacles[i][j, 2].y -= 0.9f * bug.room.gravity * num2;
				}
				tentacles[i][j, 2] += Custom.RotateAroundOrigo(tentacles[i][j, 3], degAng) * (1f + num2) * (1f - camo);
				if (!bug.dead)
				{
					if (UnityEngine.Random.value < 0.025f)
					{
						tentacles[i][j, 3] = Custom.RNV() * UnityEngine.Random.value;
					}
					else if (j > 0)
					{
						tentacles[i][j, 3] = Vector2.Lerp(tentacles[i][j, 3], tentacles[i][j - 1, 3], 0.1f);
					}
				}
				ConnectSegment(i, j);
			}
			for (int num3 = tentacles[i].GetLength(0) - 1; num3 >= 0; num3--)
			{
				ConnectSegment(i, num3);
			}
		}
	}

	public Vector2 TentacleDir(int t, float timeStacker, bool con)
	{
		float num = Custom.AimFromOneVectorToAnother(Vector2.Lerp(bug.ChunkInOrder(1).lastPos, bug.ChunkInOrder(1).pos, timeStacker), Vector2.Lerp(bug.ChunkInOrder(0).lastPos, bug.ChunkInOrder(0).pos, timeStacker));
		float t2 = Mathf.InverseLerp(0f, tentacles.Count - 1, t);
		if (bug.swim > 0f)
		{
			float t3 = Mathf.Lerp(0f, Mathf.Pow(0.5f + 0.5f * Mathf.Sin(bug.swimCycle * (float)Math.PI * 2f), 0.5f), bug.swim);
			return Custom.DegToVec(num + Mathf.Lerp(-1f, 1f, t2) * (con ? Mathf.Lerp(10f, 70f, t3) : Mathf.Lerp(20f, 160f, t3)));
		}
		return Custom.DegToVec(num + Mathf.Lerp(-1f, 1f, t2) * (con ? 30f : 80f));
	}

	public Vector2 TentacleConPos(int t, float timeStacker)
	{
		Vector2 vector = Custom.DirVec(Vector2.Lerp(bug.ChunkInOrder(1).lastPos, bug.ChunkInOrder(1).pos, timeStacker), Vector2.Lerp(bug.ChunkInOrder(0).lastPos, bug.ChunkInOrder(0).pos, timeStacker));
		return Vector2.Lerp(Vector2.Lerp(bug.ChunkInOrder(0).lastPos, bug.ChunkInOrder(0).pos, timeStacker) + vector * 4f + TentacleDir(t, timeStacker, con: true) * 4f, Vector2.Lerp(bug.ChunkInOrder(1).lastPos, bug.ChunkInOrder(1).pos, timeStacker), bug.swallowed);
	}

	private void ConnectSegment(int c, int i)
	{
		float num = (2f + Mathf.Sin(Mathf.InverseLerp(0f, tentacles.Count - 1, c) * (float)Math.PI) * 3f) * (1f - bug.swallowed);
		if (i == 0)
		{
			Vector2 vector = TentacleConPos(c, 1f);
			Vector2 vector2 = Custom.DirVec(tentacles[c][i, 0], vector);
			float num2 = Vector2.Distance(tentacles[c][i, 0], vector);
			tentacles[c][i, 0] -= vector2 * (num - num2);
			tentacles[c][i, 2] -= vector2 * (num - num2);
		}
		else
		{
			Vector2 vector3 = Custom.DirVec(tentacles[c][i, 0], tentacles[c][i - 1, 0]);
			float num3 = Vector2.Distance(tentacles[c][i, 0], tentacles[c][i - 1, 0]);
			tentacles[c][i, 0] -= vector3 * (num - num3) * 0.6f;
			tentacles[c][i, 2] -= vector3 * (num - num3) * 0.6f;
			tentacles[c][i - 1, 0] += vector3 * (num - num3) * 0.4f;
			tentacles[c][i - 1, 2] += vector3 * (num - num3) * 0.4f;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[MeshSprite] = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: false);
		sLeaser.sprites[BodySprite] = new FSprite("Circle20");
		sLeaser.sprites[BodySprite].scaleX = 0.45f;
		sLeaser.sprites[BodySprite].scaleY = 0.6f;
		sLeaser.sprites[ClosedEyeSprite] = new FSprite("pixel");
		sLeaser.sprites[ClosedEyeSprite].scaleY = 9f;
		sLeaser.sprites[EyeSprite] = new FSprite("Circle20");
		sLeaser.sprites[EyeSprite].scaleY = 0.35f;
		sLeaser.sprites[PupilSprite] = new FSprite("Circle20");
		sLeaser.sprites[EyeDotSprite] = new FSprite("pixel");
		sLeaser.sprites[EyeHighLightSprite] = new FSprite("tinyStar");
		for (int i = 0; i < tentacles.Count; i++)
		{
			sLeaser.sprites[TentacleSprite(i)] = TriangleMesh.MakeLongMesh(tentacles[i].GetLength(0), pointyTip: true, customColor: true);
		}
		for (int j = 0; j < scales.Length; j++)
		{
			sLeaser.sprites[ScaleSprite(j)] = new FSprite("pixel");
			sLeaser.sprites[ScaleSprite(j)].scaleX = 1.5f;
			sLeaser.sprites[ScaleSprite(j)].scaleY = 3f;
			sLeaser.sprites[ScaleSprite(j)].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Vector2.Lerp(bug.ChunkInOrder(0).lastPos, bug.ChunkInOrder(0).pos, timeStacker);
		vector += Custom.DirVec(Vector2.Lerp(bug.ChunkInOrder(1).lastPos, bug.ChunkInOrder(1).pos, timeStacker), vector) * 5f;
		Vector2 a = vector;
		Vector2 a2 = Vector2.Lerp(bug.ChunkInOrder(2).lastPos, bug.ChunkInOrder(2).pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(bug.ChunkInOrder(1).lastPos, bug.ChunkInOrder(1).pos, timeStacker);
		a = Vector2.Lerp(a, vector2, bug.swallowed * 0.9f);
		a2 = Vector2.Lerp(a2, vector2, bug.swallowed * 0.9f);
		Vector2 vector3 = Custom.DirVec(a2, a);
		a2 += Custom.DirVec(vector2, a2) * 5f;
		Vector2 vector4 = Vector2.Lerp(lastLookDir, lookDir, timeStacker);
		sLeaser.sprites[BodySprite].x = a.x - camPos.x;
		sLeaser.sprites[BodySprite].y = a.y - camPos.y;
		sLeaser.sprites[BodySprite].rotation = Custom.VecToDeg(Vector3.Slerp(Custom.DirVec(a2, a), vector3, 0.5f));
		float num = Mathf.Lerp(lastEyeOpen, eyeOpen, timeStacker);
		Vector2 vector5 = a + vector3 * 1.5f;
		sLeaser.sprites[EyeSprite].x = vector5.x - camPos.x;
		sLeaser.sprites[EyeSprite].y = vector5.y - camPos.y;
		sLeaser.sprites[ClosedEyeSprite].x = vector5.x - camPos.x;
		sLeaser.sprites[ClosedEyeSprite].y = vector5.y - camPos.y;
		if (num == 1f)
		{
			sLeaser.sprites[EyeSprite].rotation = 0f;
		}
		else
		{
			sLeaser.sprites[EyeSprite].rotation = Custom.VecToDeg(vector3);
		}
		sLeaser.sprites[ClosedEyeSprite].rotation = Custom.VecToDeg(vector3);
		sLeaser.sprites[EyeSprite].scaleX = Mathf.Lerp(1f, 7f, Mathf.Pow(num, 0.5f)) / 20f;
		sLeaser.sprites[EyeHighLightSprite].x = vector5.x - 2f * num - camPos.x;
		sLeaser.sprites[EyeHighLightSprite].y = vector5.y + 2f * num - camPos.y;
		sLeaser.sprites[EyeHighLightSprite].alpha = 0.5f * Mathf.InverseLerp(0.5f, 1f, num) * (1f - deadColor);
		float num2 = (bug.tossed ? 3f : Mathf.Lerp(1.2f, 2.25f, Custom.SCurve(Mathf.Lerp(lastPupSize, pupSize, timeStacker), 0.75f))) * Mathf.Pow(num, 0.75f);
		vector5 += vector4 * (3.5f - num2) * num;
		sLeaser.sprites[PupilSprite].x = vector5.x - camPos.x;
		sLeaser.sprites[PupilSprite].y = vector5.y - camPos.y;
		if (!bug.dead && rCam.room.PointSubmerged(vector5 + new Vector2(0f, 5f)))
		{
			sLeaser.sprites[PupilSprite].color = new Color(0f, 0.003921569f, 0f);
		}
		else
		{
			sLeaser.sprites[PupilSprite].color = Color.Lerp(eyeColor.rgb, new Color(0.35f, 0.35f, 0.35f), deadColor);
		}
		sLeaser.sprites[PupilSprite].scale = num2 * 2f / 20f;
		sLeaser.sprites[EyeDotSprite].x = vector5.x - camPos.x;
		sLeaser.sprites[EyeDotSprite].y = vector5.y - camPos.y;
		for (int i = 0; i < scales.Length; i++)
		{
			float num3 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(a, vector2, scales[i].y), Vector2.Lerp(vector2, a2, scales[i].y));
			Vector2 vector6 = ((scales[i].y < 0.5f) ? Vector2.Lerp(a, vector2, Mathf.InverseLerp(0f, 0.5f, scales[i].y)) : Vector2.Lerp(vector2, a2, Mathf.InverseLerp(0.5f, 1f, scales[i].y)));
			vector6 += Custom.RotateAroundOrigo(new Vector2((-1f + 2f * scales[i].x) * (2f + 2f * bug.inkLeft), 0f), num3);
			sLeaser.sprites[ScaleSprite(i)].x = vector6.x - camPos.x;
			sLeaser.sprites[ScaleSprite(i)].y = vector6.y - camPos.y;
			sLeaser.sprites[ScaleSprite(i)].rotation = num3;
			sLeaser.sprites[ScaleSprite(i)].isVisible = scales[i].y < Mathf.InverseLerp(-1f, 3f, bug.BitesLeft);
		}
		for (int j = 0; j < 6; j++)
		{
			float num4 = (float)j / 5f;
			float f = (float)(j + 1) / 5f;
			Vector2 vector7 = Bez(a, a2, vector2, num4);
			Vector2 b = Bez(a, a2, vector2, f);
			if (j > bug.BitesLeft * 3)
			{
				vector7 = a;
				b = a;
			}
			Vector2 normalized = (vector - vector7).normalized;
			Vector2 vector8 = Custom.PerpendicularVector(normalized);
			float num5 = Vector2.Distance(vector7, vector) / 5f;
			float num6 = Vector2.Distance(vector7, b) / 5f;
			float num7 = 2f * num4 + Mathf.Sin(num4 * (float)Math.PI) * Mathf.Lerp(1f, 3f, bug.inkLeft);
			float num8 = num7;
			if (num4 == 0f)
			{
				num7 *= 0.5f;
			}
			else if (num4 == 1f)
			{
				num8 *= 0.5f;
			}
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4, vector - vector8 * num7 - normalized * num5 - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector + vector8 * num7 - normalized * num5 - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector7 - vector8 * num8 + normalized * num6 - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector7 + vector8 * num8 + normalized * num6 - camPos);
			vector = vector7;
		}
		if (bug.BitesLeft < 3)
		{
			for (int k = 0; k < tentacles.Count; k++)
			{
				sLeaser.sprites[TentacleSprite(k)].isVisible = false;
			}
		}
		else
		{
			for (int l = 0; l < tentacles.Count; l++)
			{
				sLeaser.sprites[TentacleSprite(l)].isVisible = true;
				vector = TentacleConPos(l, timeStacker);
				float num9 = 2f;
				for (int m = 0; m < tentacles[l].GetLength(0); m++)
				{
					Vector2 vector9 = Vector2.Lerp(tentacles[l][m, 1], tentacles[l][m, 0], timeStacker);
					Vector2 normalized2 = (vector9 - vector).normalized;
					Vector2 vector10 = Custom.PerpendicularVector(normalized2);
					float num10 = Vector2.Distance(vector9, vector) / 5f;
					float num11 = 0.5f + 0.6f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, 5f, m) * (float)Math.PI)), 2f);
					if (m == 0)
					{
						(sLeaser.sprites[TentacleSprite(l)] as TriangleMesh).MoveVertice(m * 4, vector - vector10 * (num9 + num11) * 0.5f - camPos);
						(sLeaser.sprites[TentacleSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 1, vector + vector10 * (num9 + num11) * 0.5f - camPos);
					}
					else
					{
						(sLeaser.sprites[TentacleSprite(l)] as TriangleMesh).MoveVertice(m * 4, vector - vector10 * (num9 + num11) * 0.5f + normalized2 * num10 - camPos);
						(sLeaser.sprites[TentacleSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 1, vector + vector10 * (num9 + num11) * 0.5f + normalized2 * num10 - camPos);
					}
					(sLeaser.sprites[TentacleSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 2, vector9 - vector10 * num11 - normalized2 * num10 - camPos);
					if (m < tentacles[l].GetLength(0) - 1)
					{
						(sLeaser.sprites[TentacleSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 3, vector9 + vector10 * num11 - normalized2 * num10 - camPos);
					}
					vector = vector9;
					num9 = num11;
				}
			}
		}
		if (camoPickup)
		{
			camoPickupColor = Color.Lerp(camoPickupColor, rCam.PixelColorAtCoordinate(bug.bodyChunks[0].pos), 0.1f);
		}
		camoPickup = false;
		if (deadColor != lastDeadColor || camo != lastCamo)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
	}

	private Vector2 Bez(Vector2 A, Vector2 B, Vector2 C, float f)
	{
		if (f < 0.5f)
		{
			return Custom.Bezier(A, (A + C) / 2f, C, C + Custom.DirVec(B, A) * Vector2.Distance(A, C) / 4f, f);
		}
		return Custom.Bezier(C, C + Custom.DirVec(A, B) * Vector2.Distance(C, B) / 2f, B, (B + C) / 2f, f);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		float num = Custom.SCurve(Mathf.InverseLerp(0.1f, 1.1f, camo), 0.8f);
		Color a = Color.Lerp(Custom.HSL2RGB(skinColor.hue, skinColor.saturation * (1f - 0.4f * deadColor), skinColor.lightness * (1f - 0.3f * deadColor)), palette.fogColor, 0.2f + 0.1f * deadColor);
		a = Color.Lerp(a, palette.blackColor, 0.2f * deadColor);
		Color a2 = Color.Lerp(Custom.HSL2RGB(secondColor.hue, secondColor.saturation * (1f - 0.2f * deadColor), secondColor.lightness * (1f - 0.5f * deadColor)), palette.blackColor, 0.05f + 0.4f * Mathf.Max(deadColor * 0.5f, Mathf.Pow(num, 2f)));
		a = Color.Lerp(a, camoPickupColor, num);
		a2 = Color.Lerp(a2, camoPickupColor, num * 0.5f);
		sLeaser.sprites[MeshSprite].color = a;
		sLeaser.sprites[BodySprite].color = a;
		Color b = Color.Lerp(a2, palette.blackColor, 0.1f + 0.3f * Mathf.Max(deadColor, num));
		for (int i = 0; i < tentacles.Count; i++)
		{
			for (int j = 0; j < (sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors.Length; j++)
			{
				(sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors[j] = Color.Lerp(a, b, Mathf.InverseLerp(1f, (sLeaser.sprites[TentacleSprite(i)] as TriangleMesh).verticeColors.Length - 1, j));
			}
		}
		sLeaser.sprites[ClosedEyeSprite].color = Color.Lerp(Color.Lerp(a, a2, 0.8f), palette.blackColor, 0.3f);
		a = Color.Lerp(Color.Lerp(a, palette.blackColor, 0.7f - 0.5f * deadColor), a2, 0.6f + 0.4f * num);
		for (int k = 0; k < scales.Length; k++)
		{
			sLeaser.sprites[ScaleSprite(k)].color = a;
		}
		sLeaser.sprites[EyeSprite].color = palette.blackColor;
		sLeaser.sprites[EyeDotSprite].color = palette.blackColor;
	}
}
