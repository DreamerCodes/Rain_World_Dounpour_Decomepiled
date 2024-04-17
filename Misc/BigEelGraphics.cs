using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class BigEelGraphics : GraphicsModule
{
	public BigEel eel;

	public TailSegment[] tail;

	public TailSegment[][,] fins;

	private float[,] finsData;

	private float[,] chunksData;

	private Vector2[] eyesData;

	private float[,] eyeScales;

	public int vibrateSegment;

	private int numberOfScales;

	private int numberOfEyes;

	private float scaleSize;

	private float lastTailSwim;

	private float tailSwim;

	public float jawCharge;

	public float lastJawCharge;

	private StaticSoundLoop hydraulicsSound;

	private StaticSoundLoop chargedJawsSound;

	private StaticSoundLoop finSound;

	private int scaleStart;

	private int MeshSprite => 20 + eel.bodyChunks.Length;

	private int TotalSprites => 20 + eel.bodyChunks.Length + 1 + fins.Length * 2 + numberOfScales * 2 + numberOfEyes * 2;

	public bool Albino => eel.albino;

	public int TotalSegments => eel.bodyChunks.Length + tail.Length;

	private int BeakSprite(int side, int module)
	{
		return module * 2 + side;
	}

	private int BeakArmSprite(int module, int arm, int side)
	{
		return 4 + 8 * arm + side * 4 + module;
	}

	private int BodyChunksSprite(int chnk)
	{
		return 20 + chnk;
	}

	private int FinSprite(int fin, int side)
	{
		return 21 + eel.bodyChunks.Length + fin * 2 + side;
	}

	private int ScaleSprite(int scale, int side)
	{
		return 21 + eel.bodyChunks.Length + fins.Length * 2 + scale * 2 + side;
	}

	private int EyeSprite(int eye, int part)
	{
		return 21 + eel.bodyChunks.Length + fins.Length * 2 + numberOfScales * 2 + part * numberOfEyes + eye;
	}

	private float FinContour(float f)
	{
		if (f < 1f / 3f)
		{
			return 0.3f + 0.7f * Custom.SCurve(Mathf.InverseLerp(0f, 1f / 3f, f), 0.5f);
		}
		return Mathf.InverseLerp(1f, 1f / 3f, f);
	}

	public BigEelGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		eel = ow as BigEel;
		cullRange = -1f;
		Custom.Log("NEW GRAPH MOD!");
		internalContainerObjects = new List<ObjectHeldInInternalContainer>();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(eel.iVars.finsSeed);
		tail = new TailSegment[60];
		for (int i = 0; i < tail.Length; i++)
		{
			float t = (float)i / (float)(tail.Length - 1);
			tail[i] = new TailSegment(this, Mathf.Lerp(eel.bodyChunks[eel.bodyChunks.Length - 1].rad, 1f, t), 30f, (i > 0) ? tail[i - 1] : null, 0.5f, 1f, 0.1f, pullInPreviousPosition: true);
		}
		scaleStart = UnityEngine.Random.Range(1, 6);
		fins = new TailSegment[UnityEngine.Random.Range(6, 8)][,];
		finsData = new float[fins.Length, 2];
		float num = Mathf.Lerp(6f, 8f, UnityEngine.Random.value);
		if (fins.Length > 6)
		{
			num *= 0.8f;
		}
		for (int j = 0; j < fins.Length; j++)
		{
			finsData[j, 0] = 100f + 200f * Mathf.Sin(Mathf.Pow((float)j / (float)(fins.Length - 1), 0.5f) * (float)Math.PI);
			int num2 = Mathf.FloorToInt(finsData[j, 0] / 20f) + 1;
			float num3 = num + num * Mathf.Sin(Mathf.Pow((float)j / 5f, 0.8f) * (float)Math.PI);
			fins[j] = new TailSegment[2, num2];
			for (int k = 0; k < 2; k++)
			{
				finsData[j, 1] = UnityEngine.Random.value;
				for (int l = 0; l < fins[j].GetLength(1); l++)
				{
					fins[j][k, l] = new TailSegment(this, 1f + FinContour((float)l / (float)(fins[j].GetLength(1) - 1)) * num3, finsData[j, 0] / (float)num2, (l > 0) ? fins[j][k, l - 1] : null, 0.5f, 1f, 0.2f, pullInPreviousPosition: true);
				}
			}
		}
		chunksData = new float[eel.bodyChunks.Length, 2];
		for (int m = 0; m < eel.bodyChunks.Length; m++)
		{
			chunksData[m, 0] = UnityEngine.Random.value * 360f;
			chunksData[m, 1] = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		}
		numberOfScales = UnityEngine.Random.Range(20, 40);
		scaleSize = Mathf.Lerp(0.5f, 1.2f, Mathf.Pow(UnityEngine.Random.value, 0.5f));
		numberOfEyes = 40;
		eyesData = new Vector2[numberOfEyes];
		eyeScales = new float[numberOfEyes, 3];
		for (int n = 0; n < eyesData.Length; n++)
		{
			eyesData[n] = Custom.RNV() * Mathf.Pow(UnityEngine.Random.value, 0.6f);
			if (eyesData[n].y > 0.7f)
			{
				eyesData[n].y = Mathf.Lerp(eyesData[n].y, 0.7f, 0.3f);
			}
			eyeScales[n, 0] = Mathf.Lerp(0.5f, 1.5f, Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap(eyesData[n].y, -1f, 0.7f, 1.5f, 0.2f)));
			eyeScales[n, 1] = Mathf.Lerp(0.1f, eyeScales[n, 0] * 0.9f, Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap(eyesData[n].y, -1f, 0.7f, 2f, 0.1f)));
			eyeScales[n, 2] = Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap(eyesData[n].y, -1f, 0.7f, 2f, 0.1f));
		}
		UnityEngine.Random.state = state;
		int num4 = 0;
		for (int num5 = 0; num5 < fins.Length; num5++)
		{
			num4 += 2 * fins[num5].GetLength(1);
		}
		bodyParts = new BodyPart[tail.Length + num4];
		for (int num6 = 0; num6 < tail.Length; num6++)
		{
			bodyParts[num6] = tail[num6];
		}
		num4 = 0;
		for (int num7 = 0; num7 < fins.Length; num7++)
		{
			for (int num8 = 0; num8 < 2; num8++)
			{
				for (int num9 = 0; num9 < fins[num7].GetLength(1); num9++)
				{
					bodyParts[tail.Length + num4] = fins[num7][num8, num9];
					num4++;
				}
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		hydraulicsSound = new StaticSoundLoop(SoundID.Leviathan_Hydraulics_LOOP, eel.mainBodyChunk.pos, eel.room, 1f, 1f);
		chargedJawsSound = new StaticSoundLoop(SoundID.Leviathan_Charged_Jaws_LOOP, eel.mainBodyChunk.pos, eel.room, 1f, 1f);
		finSound = new StaticSoundLoop(SoundID.Leviathan_Fin_Working_In_Deep_Water_LOOP, eel.bodyChunks[eel.bodyChunks.Length - 1].pos, eel.room, 1f, 1f);
	}

	public override void Update()
	{
		base.Update();
		if (hydraulicsSound == null)
		{
			Reset();
		}
		hydraulicsSound.Update();
		hydraulicsSound.pos = eel.mainBodyChunk.pos;
		hydraulicsSound.volume = Mathf.Lerp(hydraulicsSound.volume, ((eel.jawCharge < 0.3f || eel.jawCharge > 0.4f) && eel.jawCharge > 0f && eel.jawCharge < 1f) ? 1f : 0f, (eel.jawCharge == 0f) ? 0.1f : 0.6f);
		hydraulicsSound.pitch = Mathf.Lerp(0.5f, 1.2f, (eel.jawCharge < 0.3f) ? Mathf.InverseLerp(0f, 0.3f, eel.jawCharge) : 0.1f);
		chargedJawsSound.Update();
		chargedJawsSound.pos = eel.mainBodyChunk.pos;
		chargedJawsSound.volume = Mathf.Lerp(chargedJawsSound.volume, (eel.jawCharge > 0.2f && eel.jawCharge < 0.4f) ? 1f : 0f, 0.7f);
		chargedJawsSound.pitch = Custom.LerpMap(eel.jawChargeFatigue, 0.5f, 1f, 1f, 0.5f);
		finSound.Update();
		BodyChunk bodyChunk = eel.bodyChunks[eel.bodyChunks.Length - 1];
		finSound.pos = bodyChunk.pos;
		finSound.volume = Mathf.Pow(Custom.LerpMap(Vector2.Distance(bodyChunk.lastPos, bodyChunk.pos), 2f, 14f, 0f, 1f), 1.5f);
		finSound.pitch = Mathf.Pow(Custom.LerpMap(Vector2.Distance(bodyChunk.lastPos, bodyChunk.pos), 2f, 14f, 0.2f, 1.8f), 1.2f);
		tail[0].connectedPoint = eel.bodyChunks[eel.bodyChunks.Length - 1].pos;
		lastJawCharge = jawCharge;
		jawCharge = eel.jawCharge;
		if (jawCharge == 0f)
		{
			lastJawCharge = 0f;
		}
		if (eel.snapFrame)
		{
			vibrateSegment = 0;
			for (int i = 0; i < fins.Length; i++)
			{
				Vector2 vector = BodyMeshDir(FinConnectionIndex(i), 1f);
				for (int j = 0; j < 2; j++)
				{
					for (int k = 0; k < fins[i].GetLength(1); k++)
					{
						fins[i][j, k].vel += vector * Mathf.Sin((float)k / ((float)fins[i].GetLength(1) - 1f) * (float)Math.PI) * 30f;
					}
				}
			}
			Vector2 a = SmoothedBodyMeshPos(0, 1f) + BodyMeshDir(0, 1f) * 40f;
			Vector2 b = SmoothedBodyMeshPos(0, 1f) + BodyMeshDir(0, 1f) * 200f;
			for (int l = 0; l < 20; l++)
			{
				Vector2 pos = Vector2.Lerp(a, b, Mathf.Pow(UnityEngine.Random.value, 1.6f));
				if (eel.room.PointSubmerged(pos))
				{
					eel.room.AddObject(new Bubble(pos, Custom.RNV() * Mathf.Pow(UnityEngine.Random.value, 2.1f) * 30f, bottomBubble: false, fakeWaterBubble: false));
				}
				else
				{
					eel.room.AddObject(new WaterDrip(pos, Custom.RNV() * UnityEngine.Random.value * 60f, waterColor: false));
				}
			}
		}
		else
		{
			vibrateSegment++;
		}
		lastTailSwim = tailSwim;
		if (!ModManager.MMF || eel.Consious)
		{
			tailSwim -= 1f / Mathf.Lerp(20f, 10f, eel.swimSpeed);
		}
		else
		{
			tailSwim = 0f;
		}
		for (int m = 0; m < tail.Length; m++)
		{
			tail[m].Update();
			float t = (float)m / (float)(tail.Length - 1);
			if (!ModManager.MMF || eel.Consious || !eel.room.PointSubmerged(tail[m].pos))
			{
				if (eel.room.PointSubmerged(tail[m].pos))
				{
					tail[m].vel *= Mathf.Lerp(0.98f, 0.1f, Mathf.Pow(Mathf.InverseLerp(2f, 20f, tail[m].vel.magnitude), Mathf.Lerp(5f, 0.5f, t)));
					for (int n = 2; n < 8; n++)
					{
						Vector2 p = ((m >= n) ? tail[m - n].pos : eel.bodyChunks[eel.bodyChunks.Length - n].pos);
						tail[m].vel += Custom.DirVec(p, tail[m].pos) * Mathf.Lerp(1.4f, 0.1f, t);
					}
				}
				else
				{
					tail[m].vel *= Mathf.Lerp(0.98f, 0.5f, t);
					tail[m].vel.y -= 0.9f;
				}
			}
			else
			{
				tail[m].vel.x = Mathf.Lerp(tail[m].vel.x, 0f, 0.05f);
				tail[m].vel.y = Mathf.Lerp(tail[m].vel.y, 0f, 0.05f);
			}
			if (!Custom.DistLess(tail[m].pos, eel.bodyChunks[eel.bodyChunks.Length - 1].pos, 30f * (float)(m + 1)))
			{
				tail[m].pos = eel.bodyChunks[eel.bodyChunks.Length - 1].pos + Custom.DirVec(eel.bodyChunks[eel.bodyChunks.Length - 1].pos, tail[m].pos) * 30f * (m + 1);
			}
		}
		for (int num = 0; num < fins.Length; num++)
		{
			for (int num2 = 0; num2 < 2; num2++)
			{
				Vector2 vector2 = FinConnectionPos(num, num2, 1f);
				fins[num][num2, 0].connectedPoint = vector2;
				if (FinConnectionIndex(num) == vibrateSegment)
				{
					fins[num][num2, 1].vel += BodyMeshDir(FinConnectionIndex(num), 1f) * 20f;
				}
				for (int num3 = 0; num3 < fins[num].GetLength(1); num3++)
				{
					float num4 = (float)num3 / (float)(fins[num].GetLength(1) - 1);
					fins[num][num2, num3].Update();
					Vector2 vector3 = vector2 + Custom.PerpendicularVector(BodyMeshDir(FinConnectionIndex(num), 1f)) * ((num2 == 0) ? (-1f) : 1f) * 20f * (num3 + 1);
					vector3 += BodyMeshDir(FinConnectionIndex(num), 1f) * Mathf.Sin(tailSwim * 0.25f * (float)Math.PI) * ((num2 == num % 2) ? (-1f) : 1f) * Mathf.Lerp(10f, 17f, eel.swimSpeed) * (num3 + 1);
					float maxLength = Mathf.InverseLerp(3f, 0f, num3) * 1.1f + Mathf.Pow(1f - num4, 2.5f) * 1.8f;
					fins[num][num2, num3].vel += Vector2.ClampMagnitude(vector3 - fins[num][num2, num3].pos, maxLength);
					if (eel.room.PointSubmerged(fins[num][num2, num3].pos))
					{
						fins[num][num2, num3].vel *= Mathf.Lerp(0.98f, 0.1f, Mathf.Pow(Mathf.InverseLerp(2f, 20f, fins[num][num2, num3].vel.magnitude), Mathf.Lerp(5f, 0.5f, num4)));
						Vector2 p2 = ((num3 != 0) ? fins[num][num2, num3 - 1].pos : vector2);
						fins[num][num2, num3].vel += Custom.DirVec(p2, fins[num][num2, num3].pos) * Mathf.Lerp(1.8f, 0.3f, num4);
					}
					else
					{
						fins[num][num2, num3].vel *= 0.9f;
						fins[num][num2, num3].vel.y -= 0.9f;
					}
					if (!Custom.DistLess(fins[num][num2, num3].pos, vector2, 20f * (float)(num3 + 1)))
					{
						fins[num][num2, num3].pos = vector2 + Custom.DirVec(vector2, fins[num][num2, num3].pos) * 20f * (num3 + 1);
					}
				}
			}
		}
	}

	private int FinConnectionIndex(int fin)
	{
		return fin * 2 + ((fin == 0) ? 1 : 0);
	}

	private Vector2 FinConnectionPos(int fin, int side, float timeStacker)
	{
		return Vector2.Lerp(SidePos(FinConnectionIndex(fin), side, 10f, timeStacker), SidePos(FinConnectionIndex(fin) + 1, side, 10f, timeStacker), 0.3f);
	}

	private Vector2 SidePos(int segment, int side, float radSubtract, float timeStacker)
	{
		return SmoothedBodyMeshPos(segment, timeStacker) + Custom.PerpendicularVector(BodyMeshDir(segment, timeStacker)) * ((side == 0) ? (-1f) : 1f) * (BodyMeshRad(segment) - radSubtract);
	}

	public Vector2 BodyMeshPos(int index, float timeStacker)
	{
		index = Custom.IntClamp(index, 0, TotalSegments - 1);
		if (index < base.owner.bodyChunks.Length)
		{
			return Vector2.Lerp(base.owner.bodyChunks[index].lastPos, base.owner.bodyChunks[index].pos, timeStacker);
		}
		index -= base.owner.bodyChunks.Length;
		Vector2 vector = Vector2.Lerp(tail[index].lastPos, tail[index].pos, timeStacker);
		if (index > 0)
		{
			vector += Custom.PerpendicularVector((vector - Vector2.Lerp(tail[index - 1].lastPos, tail[index - 1].pos, timeStacker)).normalized) * Mathf.Sin((Mathf.Lerp(lastTailSwim, tailSwim, timeStacker) + (float)index / 6f) * (float)Math.PI * 2f) * 20f * Mathf.Sin((float)index / (float)tail.Length * (float)Math.PI);
		}
		return vector;
	}

	public float BodyMeshRad(int index)
	{
		index = Custom.IntClamp(index, 0, TotalSegments - 1);
		if (index < base.owner.bodyChunks.Length)
		{
			return base.owner.bodyChunks[index].rad;
		}
		index -= base.owner.bodyChunks.Length;
		return tail[index].rad;
	}

	public float SmoothedBodyMeshRad(int index)
	{
		return (BodyMeshRad(index - 2) + BodyMeshRad(index) + BodyMeshRad(index + 2)) / 3f;
	}

	public Vector2 SmoothedBodyMeshPos(int index, float timeStacker)
	{
		if (index == vibrateSegment)
		{
			return BodyMeshPos(index, timeStacker) + Custom.RNV() * Mathf.Lerp(BodyMeshRad(index) / 3f, 10f, 0.5f);
		}
		float num = Mathf.InverseLerp((index < eel.bodyChunks.Length) ? 4f : 20f, 1f, Mathf.Abs(index - eel.bodyChunks.Length));
		if (num == 0f)
		{
			return BodyMeshPos(index, timeStacker);
		}
		Vector2 b = BodyMeshPos(index, timeStacker);
		for (int i = 1; i < 5; i++)
		{
			b += BodyMeshPos(index - i, timeStacker) + BodyMeshPos(index + i, timeStacker);
		}
		b /= 9f;
		return Vector2.Lerp(BodyMeshPos(index, timeStacker), b, num);
	}

	public Vector2 BodyMeshDir(int index, float timeStacker)
	{
		return (SmoothedBodyMeshPos(index, timeStacker) - SmoothedBodyMeshPos(index + 1, timeStacker)).normalized;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.containers = new FContainer[1];
		sLeaser.containers[0] = new FContainer();
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[MeshSprite] = TriangleMesh.MakeLongMesh(TotalSegments, pointyTip: true, customColor: true);
		sLeaser.sprites[MeshSprite].shader = rCam.room.game.rainWorld.Shaders["EelBody"];
		for (int i = 0; i < (sLeaser.sprites[MeshSprite] as TriangleMesh).verticeColors.Length; i++)
		{
			if (eel.abstractCreature.IsVoided())
			{
				(sLeaser.sprites[MeshSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(Color.white, RainWorld.SaturatedGold, Mathf.InverseLerp(0f, 40f, i));
			}
			else
			{
				(sLeaser.sprites[MeshSprite] as TriangleMesh).verticeColors[i] = HSLColor.Lerp(eel.iVars.patternColorA, eel.iVars.patternColorB, Mathf.InverseLerp(0f, 40f, i)).rgb;
			}
		}
		sLeaser.sprites[MeshSprite].alpha = eel.iVars.patternDisplacement;
		for (int j = 0; j < base.owner.bodyChunks.Length; j++)
		{
			sLeaser.sprites[BodyChunksSprite(j)] = new FSprite("Futile_White");
			sLeaser.sprites[BodyChunksSprite(j)].scale = base.owner.bodyChunks[j].rad * 1.06f / 8f;
			sLeaser.sprites[BodyChunksSprite(j)].color = new Color(1f, (j == 0) ? 0.5f : 0f, (j == 0) ? 0.5f : 0f);
			sLeaser.sprites[BodyChunksSprite(j)].shader = rCam.room.game.rainWorld.Shaders["JaggedCircle"];
			sLeaser.sprites[BodyChunksSprite(j)].alpha = 0.35f;
			sLeaser.sprites[BodyChunksSprite(j)].scaleX *= chunksData[j, 1];
		}
		for (int k = 0; k < 2; k++)
		{
			for (int l = 0; l < fins.Length; l++)
			{
				sLeaser.sprites[FinSprite(l, k)] = TriangleMesh.MakeLongMesh(fins[l].GetLength(1), pointyTip: true, customColor: false);
				sLeaser.sprites[FinSprite(l, k)].shader = rCam.room.game.rainWorld.Shaders["EelFin"];
			}
			for (int m = 0; m < numberOfScales; m++)
			{
				sLeaser.sprites[ScaleSprite(m, k)] = new FSprite("LizardScaleA4");
				sLeaser.sprites[ScaleSprite(m, k)].anchorY = 0f;
				sLeaser.sprites[ScaleSprite(m, k)].scaleX = ((k == 0) ? 1f : (-1f));
			}
			sLeaser.sprites[BeakSprite(k, 0)] = new FSprite("EelJaw" + (2 - k) + "A");
			sLeaser.sprites[BeakSprite(k, 1)] = new FSprite("EelJaw" + (2 - k) + "B");
			sLeaser.sprites[BeakSprite(k, 0)].anchorX = ((k == 1) ? 0.25f : 0.75f);
			sLeaser.sprites[BeakSprite(k, 1)].anchorX = ((k == 1) ? 0.25f : 0.75f);
			for (int n = 0; n < 4; n++)
			{
				for (int num = 0; num < 2; num++)
				{
					if (n % 2 == 0)
					{
						sLeaser.sprites[BeakArmSprite(n, num, k)] = new FSprite("pixel");
						sLeaser.sprites[BeakArmSprite(n, num, k)].scaleX = 5f;
						sLeaser.sprites[BeakArmSprite(n, num, k)].scaleY = 55f;
						sLeaser.sprites[BeakArmSprite(n, num, k)].anchorY = 0f;
					}
					else
					{
						sLeaser.sprites[BeakArmSprite(n, num, k)] = new FSprite("Circle20");
						sLeaser.sprites[BeakArmSprite(n, num, k)].scale = 0.5f;
					}
				}
			}
			for (int num2 = 0; num2 < numberOfEyes; num2++)
			{
				if (k == 0)
				{
					sLeaser.sprites[EyeSprite(num2, k)] = new FSprite("Cicada8head");
					sLeaser.sprites[EyeSprite(num2, k)].anchorY = 0.7f;
					sLeaser.sprites[EyeSprite(num2, k)].scale = eyeScales[num2, 0];
				}
				else
				{
					sLeaser.sprites[EyeSprite(num2, k)] = new FSprite("Cicada0shield");
					sLeaser.sprites[EyeSprite(num2, k)].anchorY = 0.4f;
					sLeaser.sprites[EyeSprite(num2, k)].scale = eyeScales[num2, 1];
				}
			}
		}
		for (int num3 = 0; num3 < fins.Length; num3++)
		{
			for (int num4 = 0; num4 < (sLeaser.sprites[FinSprite(num3, 0)] as TriangleMesh).UVvertices.Length; num4++)
			{
				(sLeaser.sprites[FinSprite(num3, 0)] as TriangleMesh).UVvertices[num4].x = 1f - (sLeaser.sprites[FinSprite(num3, 0)] as TriangleMesh).UVvertices[num4].x;
			}
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		newContatiner.AddChild(sLeaser.containers[0]);
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite node in sprites)
		{
			newContatiner.AddChild(node);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = palette.blackColor;
		Color color2 = palette.blackColor;
		Color color3 = palette.texture.GetPixel(3, 7);
		if (eel.abstractCreature.IsVoided())
		{
			color = Color.Lerp(palette.blackColor, RainWorld.SaturatedGold, 0.6f);
			color2 = Color.Lerp(color2, RainWorld.SaturatedGold, 0.3f);
			color3 = Color.Lerp(color3, Color.Lerp(color2, RainWorld.SaturatedGold, 0.5f), 0.7f);
		}
		else if (Albino)
		{
			color = Color.Lerp(palette.blackColor, new Color(0.9f, 0.4f, 0.4f), 0.6f);
			color2 = Color.Lerp(Color.Lerp(color2, Color.white, 0.9f), color, 0.15f);
			color3 = Color.Lerp(color3, Color.Lerp(color2, Color.red, 0.5f), 0.7f);
		}
		Shader.SetGlobalColor(RainWorld.ShadPropLeviathanColorA, color2);
		Shader.SetGlobalColor(RainWorld.ShadPropLeviathanColorB, color3);
		Shader.SetGlobalColor(RainWorld.ShadPropLeviathanColorHead, color);
		for (int i = 0; i < 20; i++)
		{
			sLeaser.sprites[i].color = palette.blackColor;
		}
		for (int j = 0; j < eel.bodyChunks.Length; j++)
		{
			sLeaser.sprites[BodyChunksSprite(j)].color = Color.Lerp(color, color2, (float)j / 2f);
		}
		for (int k = 0; k < 2; k++)
		{
			for (int l = 0; l < numberOfScales; l++)
			{
				sLeaser.sprites[ScaleSprite(l, k)].color = color2;
			}
			for (int m = 0; m < fins.Length; m++)
			{
				sLeaser.sprites[FinSprite(m, k)].color = new Color(Mathf.InverseLerp(100f, 300f, finsData[m, 0]), finsData[m, 1], (m == 0) ? 0.65f : 1f);
			}
			if (eel.abstractCreature.IsVoided())
			{
				for (int n = 0; n < numberOfEyes; n++)
				{
					sLeaser.sprites[EyeSprite(n, k)].color = ((k == 0) ? color : Color.white);
				}
			}
			else if (Albino)
			{
				for (int num = 0; num < numberOfEyes; num++)
				{
					sLeaser.sprites[EyeSprite(num, k)].color = ((k == 0) ? color : Color.Lerp(eel.iVars.patternColorB.rgb, palette.blackColor, eyeScales[num, 2]));
				}
			}
			else
			{
				for (int num2 = 0; num2 < numberOfEyes; num2++)
				{
					sLeaser.sprites[EyeSprite(num2, k)].color = ((k == 0) ? color : HSLColor.Lerp(eel.iVars.patternColorA, eel.iVars.patternColorB, eyeScales[num2, 2]).rgb);
				}
			}
			sLeaser.sprites[BeakSprite(k, 1)].color = Color.Lerp(palette.blackColor, new Color(1f, 1f, 1f), 0.07f);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			sLeaser.sprites[BodyChunksSprite(i)].x = SmoothedBodyMeshPos(i, timeStacker).x - camPos.x;
			sLeaser.sprites[BodyChunksSprite(i)].y = SmoothedBodyMeshPos(i, timeStacker).y - camPos.y;
			sLeaser.sprites[BodyChunksSprite(i)].rotation = Custom.VecToDeg(BodyMeshDir(i, timeStacker)) + chunksData[i, 0];
		}
		Vector2 vector = SmoothedBodyMeshPos(0, timeStacker);
		float num = SmoothedBodyMeshRad(0);
		for (int j = 0; j < TotalSegments; j++)
		{
			Vector2 vector2 = SmoothedBodyMeshPos(j, timeStacker);
			Vector2 normalized = (vector2 - vector).normalized;
			Vector2 vector3 = Custom.PerpendicularVector(normalized);
			float num2 = SmoothedBodyMeshRad(j) + Custom.LerpMap(Mathf.Abs(j - 16), 1f, 6f, 20f, 5f) * Mathf.InverseLerp(20f, 2f, j);
			float num3 = Vector2.Distance(vector2, vector) / 5f;
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4, vector - vector3 * (num + num2) * 0.5f + normalized * num3 - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector + vector3 * (num + num2) * 0.5f + normalized * num3 - camPos);
			if (j < TotalSegments - 1)
			{
				(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - vector3 * num2 - normalized * num3 - camPos);
				(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector2 + vector3 * num2 - normalized * num3 - camPos);
			}
			else
			{
				(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - camPos);
			}
			num = num2;
			vector = vector2;
		}
		Vector2 vector4 = Vector2.Lerp(eel.bodyChunks[0].lastPos, eel.bodyChunks[0].pos, timeStacker);
		Vector2 vector5 = Custom.DirVec(Vector2.Lerp(eel.bodyChunks[1].lastPos, eel.bodyChunks[1].pos, timeStacker), vector4);
		Vector2 vector6 = Custom.PerpendicularVector(vector5);
		float num4 = Mathf.Lerp(lastJawCharge, jawCharge, timeStacker);
		float num5 = Mathf.Min(Mathf.InverseLerp(0f, 0.3f, num4), Mathf.InverseLerp(1f, 0.7f, num4));
		float t = ((num4 > 0.35f) ? Mathf.InverseLerp(1f, 0.65f, num4) : 0f);
		float num6 = num5 * Mathf.InverseLerp(0.7f, 0.4f, num4);
		float num7 = Mathf.Sin(eel.jawChargeFatigue * (float)Math.PI) * num6;
		for (int k = 0; k < 2; k++)
		{
			float num8 = ((k == 0) ? (-1f) : 1f);
			for (int l = 0; l < fins.Length; l++)
			{
				vector = FinConnectionPos(l, k, timeStacker);
				num = 20f;
				for (int m = 0; m < fins[l].GetLength(1); m++)
				{
					Vector2 vector7 = Vector2.Lerp(fins[l][k, m].lastPos, fins[l][k, m].pos, timeStacker);
					Vector2 normalized2 = (vector7 - vector).normalized;
					Vector2 vector8 = Custom.PerpendicularVector(normalized2);
					float rad = fins[l][k, m].rad;
					float num9 = Vector2.Distance(vector7, vector) / 5f;
					(sLeaser.sprites[FinSprite(l, k)] as TriangleMesh).MoveVertice(m * 4, vector - vector8 * (num + rad) * 0.5f + normalized2 * num9 - camPos);
					(sLeaser.sprites[FinSprite(l, k)] as TriangleMesh).MoveVertice(m * 4 + 1, vector + vector8 * (num + rad) * 0.5f + normalized2 * num9 - camPos);
					if (m < fins[l].GetLength(1) - 1)
					{
						(sLeaser.sprites[FinSprite(l, k)] as TriangleMesh).MoveVertice(m * 4 + 2, vector7 - vector8 * rad - normalized2 * num9 - camPos);
						(sLeaser.sprites[FinSprite(l, k)] as TriangleMesh).MoveVertice(m * 4 + 3, vector7 + vector8 * rad - normalized2 * num9 - camPos);
					}
					else
					{
						(sLeaser.sprites[FinSprite(l, k)] as TriangleMesh).MoveVertice(m * 4 + 2, vector7 - camPos);
					}
					num = rad;
					vector = vector7;
				}
			}
			for (int n = 0; n < numberOfScales; n++)
			{
				float num10 = (float)n / (float)(numberOfScales - 1);
				Vector2 vector9 = SidePos(scaleStart + n, k, 3f, timeStacker);
				sLeaser.sprites[ScaleSprite(n, k)].x = vector9.x - camPos.x;
				sLeaser.sprites[ScaleSprite(n, k)].y = vector9.y - camPos.y;
				Vector2 normalized3 = (BodyMeshDir(scaleStart + n - 2, timeStacker) + BodyMeshDir(scaleStart + n, timeStacker) + BodyMeshDir(scaleStart + n + 2, timeStacker)).normalized;
				Vector2 vector10 = vector9 + (Custom.PerpendicularVector(normalized3) * num8 + normalized3 * (Mathf.Lerp(0.5f, -1.3f, num10) + 2f * Mathf.Sin((num10 * 2f + Mathf.Lerp(lastTailSwim, tailSwim, timeStacker) * 0.13f) * (float)Math.PI * 2f)) * Mathf.Pow(Mathf.Sin(num10 * (float)Math.PI), 1f - 0.7f * num10)).normalized * Mathf.Lerp(5f, 42f, Mathf.Sin(num10 * (float)Math.PI)) * scaleSize;
				sLeaser.sprites[ScaleSprite(n, k)].scaleY = Vector2.Distance(vector9, vector10) / 37f;
				sLeaser.sprites[ScaleSprite(n, k)].rotation = Custom.AimFromOneVectorToAnother(vector9, vector10);
			}
			Vector2 vector11 = vector4 + vector5 * 60f * num6 + Custom.RNV() * num7 * 2f;
			vector11 += vector6 * num8 * (Mathf.Lerp(30f, 12.5f + eel.beakGap / 20f, t) + 10f * Mathf.Sin(Mathf.Pow(num5, 2f) * (float)Math.PI));
			float num11 = Custom.VecToDeg(vector5) + Mathf.Sin(num5 * (float)Math.PI) * ((num4 < 0.35f) ? (-20f) : (-10f)) * num8 + Mathf.Lerp(-2f, 2f, UnityEngine.Random.value) * num7;
			for (int num12 = 0; num12 < 2; num12++)
			{
				sLeaser.sprites[BeakSprite(k, num12)].x = vector11.x - camPos.x;
				sLeaser.sprites[BeakSprite(k, num12)].y = vector11.y - camPos.y;
				sLeaser.sprites[BeakSprite(k, num12)].rotation = num11;
				float num13 = ((!(num4 < 0.35f)) ? ((num12 == 0) ? (43f * Mathf.Abs(Mathf.Cos(Mathf.InverseLerp(1f, 0.4f, num4) * (float)Math.PI))) : (30f * Mathf.InverseLerp(1f, 0.4f, num4))) : ((num12 == 0) ? Mathf.Lerp(15f, 43f, Mathf.InverseLerp(0.35f, 0.15f, num4)) : (30f * Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, num5), 0.2f))));
				Vector2 vector12 = vector4 + vector6 * num8 * num13 - vector5 * ((num12 == 0) ? 22f : 30f);
				Vector2 vector13 = vector11 + Custom.DegToVec(num11) * ((num12 == 0) ? (-12f) : 10f) + Custom.PerpendicularVector(Custom.DegToVec(num11)) * 15f * num8;
				Vector2 vector14 = Custom.InverseKinematic(vector12, vector13, 55f, 45f, 0f - num8);
				for (int num14 = 0; num14 < 2; num14++)
				{
					sLeaser.sprites[BeakArmSprite(num14, num12, k)].x = vector14.x - camPos.x;
					sLeaser.sprites[BeakArmSprite(num14, num12, k)].y = vector14.y - camPos.y;
				}
				sLeaser.sprites[BeakArmSprite(0, num12, k)].rotation = Custom.AimFromOneVectorToAnother(vector14, vector12);
				for (int num15 = 2; num15 < 4; num15++)
				{
					sLeaser.sprites[BeakArmSprite(num15, num12, k)].x = vector13.x - camPos.x;
					sLeaser.sprites[BeakArmSprite(num15, num12, k)].y = vector13.y - camPos.y;
				}
				sLeaser.sprites[BeakArmSprite(2, num12, k)].rotation = Custom.AimFromOneVectorToAnother(vector13, vector14);
			}
		}
		for (int num16 = 0; num16 < numberOfEyes; num16++)
		{
			Vector2 vector15 = vector4 + Custom.RotateAroundOrigo(new Vector2(eyesData[num16].x, eyesData[num16].y * ((eyesData[num16].y > 0f) ? Custom.LerpMap(Mathf.Abs(eyesData[num16].x) * num5, 0f, 1f, 1f, 1.5f) : 1f)), Custom.VecToDeg(vector5)) * 45f;
			for (int num17 = 0; num17 < 2; num17++)
			{
				sLeaser.sprites[EyeSprite(num16, num17)].x = vector15.x - camPos.x;
				sLeaser.sprites[EyeSprite(num16, num17)].y = vector15.y - camPos.y;
				sLeaser.sprites[EyeSprite(num16, num17)].rotation = Custom.AimFromOneVectorToAnother(vector4, vector15 + vector5 * Mathf.Lerp(40f, 80f, num5));
			}
		}
	}
}
