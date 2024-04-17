using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class MirosBirdGraphics : GraphicsModule
{
	public class LegGraphic
	{
		private MirosBirdGraphics owner;

		public int totalSprites;

		public int firstSprite;

		public MirosBird.BirdLeg leg;

		public LegGraphic(MirosBirdGraphics owner, MirosBird.BirdLeg leg, int firstSprite)
		{
			this.owner = owner;
			this.leg = leg;
			this.firstSprite = firstSprite;
			totalSprites = 5;
		}

		public void Update()
		{
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = new FSprite("MirosTigh");
			sLeaser.sprites[firstSprite].anchorX = 0.9f;
			sLeaser.sprites[firstSprite + 1] = new FSprite("pixel");
			sLeaser.sprites[firstSprite + 1].scaleX = 3f;
			sLeaser.sprites[firstSprite + 1].anchorY = 0f;
			sLeaser.sprites[firstSprite + 2] = new FSprite("MirosLeg");
			sLeaser.sprites[firstSprite + 2].anchorX = 0.6f;
			sLeaser.sprites[firstSprite + 3] = new FSprite("MirosLegSmallPart");
			sLeaser.sprites[firstSprite + 3].anchorX = 0.6f;
			sLeaser.sprites[firstSprite + 4] = new FSprite("deerEyeB");
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(owner.bird.mainBodyChunk.lastPos, owner.bird.mainBodyChunk.pos, timeStacker);
			Vector2 vector2 = Vector2.Lerp(leg.Hip.lastPos, leg.Hip.pos, timeStacker);
			if (!Custom.DistLess(vector2, vector, 20f))
			{
				vector2 = vector + Custom.DirVec(vector, vector2) * 20f;
			}
			Vector2 vector3 = Vector2.Lerp(leg.Knee.lastPos, leg.Knee.pos, timeStacker);
			Vector2 vector4 = Vector2.Lerp(leg.Foot.lastPos, leg.Foot.pos, timeStacker);
			sLeaser.sprites[firstSprite].x = vector2.x - camPos.x;
			sLeaser.sprites[firstSprite].y = vector2.y - camPos.y;
			sLeaser.sprites[firstSprite].rotation = Custom.AimFromOneVectorToAnother(vector2, vector3) + 90f;
			sLeaser.sprites[firstSprite].scaleX = (Vector2.Distance(vector2, vector3) + 5f) / 44f;
			sLeaser.sprites[firstSprite].scaleY = (0f - Mathf.Sign(leg.flip)) * owner.tighSize;
			Vector2 vector5 = Custom.DirVec(vector3, vector4);
			Vector2 vector6 = Custom.PerpendicularVector(vector5) * leg.flip;
			vector3 += vector6 * 5f;
			sLeaser.sprites[firstSprite + 1].x = vector4.x - camPos.x;
			sLeaser.sprites[firstSprite + 1].y = vector4.y - camPos.y;
			sLeaser.sprites[firstSprite + 1].rotation = Custom.AimFromOneVectorToAnother(vector4, vector3);
			sLeaser.sprites[firstSprite + 1].scaleY = 70f;
			sLeaser.sprites[firstSprite + 2].x = vector3.x - (vector6 * 7f).x - camPos.x;
			sLeaser.sprites[firstSprite + 2].y = vector3.y - (vector6 * 7f).y - camPos.y;
			sLeaser.sprites[firstSprite + 2].scaleX = 0f - Mathf.Sign(leg.flip);
			sLeaser.sprites[firstSprite + 2].rotation = Custom.AimFromOneVectorToAnother(vector4, vector3);
			sLeaser.sprites[firstSprite + 3].x = vector3.x - (vector6 * 8f).x + (vector5 * 18f).x - camPos.x;
			sLeaser.sprites[firstSprite + 3].y = vector3.y - (vector6 * 8f).y + (vector5 * 18f).y - camPos.y;
			sLeaser.sprites[firstSprite + 3].scaleX = 0f - Mathf.Sign(leg.flip);
			sLeaser.sprites[firstSprite + 3].rotation = Custom.AimFromOneVectorToAnother(vector4, vector3);
			sLeaser.sprites[firstSprite + 4].x = vector4.x - camPos.x;
			sLeaser.sprites[firstSprite + 4].y = vector4.y - camPos.y;
		}
	}

	public class BeakGraphic
	{
		private MirosBirdGraphics owner;

		public int totalSprites;

		public int firstSprite;

		public int index;

		public float[,] teeth;

		private float OuterShape(float f)
		{
			return Mathf.Max(1f - f, Mathf.Sin(f * (float)Math.PI));
		}

		public BeakGraphic(MirosBirdGraphics owner, int index, int firstSprite)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
			this.index = index;
			teeth = new float[UnityEngine.Random.Range(10, 20), 3];
			for (int i = 0; i < teeth.GetLength(0); i++)
			{
				teeth[i, 0] = Mathf.Lerp(0.1f, 0.9f, Mathf.Pow(UnityEngine.Random.value, 0.8f));
				teeth[i, 1] = Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value);
				teeth[i, 2] = Mathf.Lerp(-1f, 1f, Mathf.Lerp(UnityEngine.Random.value, teeth[i, 0], 1f));
			}
			totalSprites = 1 + teeth.GetLength(0);
		}

		public void Update()
		{
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[11];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new TriangleMesh.Triangle(i, i + 1, i + 2);
			}
			sLeaser.sprites[firstSprite] = new TriangleMesh("Futile_White", array, customColor: false);
			for (int j = 0; j < teeth.GetLength(0); j++)
			{
				sLeaser.sprites[firstSprite + 1 + j] = new FSprite("LizardScaleA6");
				sLeaser.sprites[firstSprite + 1 + j].anchorY = 0.8f;
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 headPos, Vector2 headDir, Vector2 headPerp, float headAng, float useFlip)
		{
			float num = Mathf.Lerp(owner.bird.lastJawOpen, owner.bird.jawOpen, timeStacker);
			Vector2 vector = Custom.DegToVec(headAng + 60f * num * (-1f + 2f * (float)index) * useFlip * Mathf.Pow(Mathf.Abs(useFlip), 0.5f));
			Vector2 vector2 = Custom.PerpendicularVector(vector) * (-1f + 2f * (float)index) * Mathf.Sign(useFlip);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(0, headPos + headDir * owner.bird.Head.rad - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(1, headPos + headPerp * (1f - 2f * (float)index) * owner.bird.Head.rad * useFlip - camPos);
			float num2 = 65f * Mathf.Lerp(Mathf.Lerp(1f, 0.6f, num), 1f, Mathf.Abs(useFlip));
			float num3 = Mathf.Lerp(5f, 7f, num) * Mathf.Lerp(0.5f, 1.5f, owner.beakFatness);
			for (int i = 1; i < 6; i++)
			{
				float num4 = (float)i / 6f;
				Vector2 vector3 = headPos + headDir * owner.bird.Head.rad * (1f - num4) * num + vector * num2 * num4;
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 2, vector3 + vector2 * num3 * Mathf.Lerp(0.6f, 1f, num) * OuterShape(num4) * Mathf.Pow(1f - Mathf.Abs(useFlip), 0.4f) - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 2 + 1, vector3 - vector2 * num3 * OuterShape(num4) * Mathf.Abs(useFlip) - camPos);
			}
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(12, headPos + vector * num2 + vector2 * num3 * 0.75f * num - camPos);
			for (int j = 0; j < teeth.GetLength(0); j++)
			{
				Vector2 vector4 = headPos + headDir * owner.bird.Head.rad * (1f - teeth[j, 0]) * num + vector * num2 * teeth[j, 0];
				sLeaser.sprites[firstSprite + 1 + j].x = vector4.x - camPos.x;
				sLeaser.sprites[firstSprite + 1 + j].y = vector4.y - camPos.y;
				sLeaser.sprites[firstSprite + 1 + j].rotation = Custom.VecToDeg((-vector2 - vector * teeth[j, 2]).normalized);
				sLeaser.sprites[firstSprite + 1 + j].scaleY = 0.5f * OuterShape(teeth[j, 0]) * Mathf.Abs(useFlip) * Mathf.InverseLerp(0f, 0.1f, num) * teeth[j, 1];
				sLeaser.sprites[firstSprite + 1 + j].scaleX = 0.4f * Mathf.InverseLerp(0f, 0.1f, num) * teeth[j, 1];
			}
		}
	}

	public class EyeTrail
	{
		public int sprite;

		public MirosBirdGraphics owner;

		public List<Vector2> positionsList;

		public List<Color> colorsList;

		public int savPoss;

		public EyeTrail(MirosBirdGraphics owner, int sprite)
		{
			this.sprite = sprite;
			this.owner = owner;
			savPoss = 10;
			Reset();
		}

		public void Reset()
		{
			positionsList = new List<Vector2> { owner.bird.Head.pos };
			colorsList = new List<Color> { owner.EyeColor };
		}

		private Vector2 GetSmoothPos(int i, float timeStacker)
		{
			return Vector2.Lerp(GetPos(i + 1), GetPos(i), timeStacker);
		}

		private Vector2 GetPos(int i)
		{
			return positionsList[Custom.IntClamp(i, 0, positionsList.Count - 1)];
		}

		private Color GetCol(int i)
		{
			return colorsList[Custom.IntClamp(i, 0, colorsList.Count - 1)];
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[sprite] = TriangleMesh.MakeLongMesh(savPoss - 1, pointyTip: false, customColor: true);
		}

		public void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = owner.EyePos(timeStacker);
			float num = 2f * owner.eyeSize;
			for (int i = 0; i < savPoss - 1; i++)
			{
				Vector2 smoothPos = GetSmoothPos(i, timeStacker);
				Vector2 smoothPos2 = GetSmoothPos(i + 1, timeStacker);
				Vector2 normalized = (vector - smoothPos).normalized;
				Vector2 vector2 = Custom.PerpendicularVector(normalized);
				normalized *= Vector2.Distance(vector, smoothPos2) / 5f;
				(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4, vector - vector2 * num - normalized - camPos);
				(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector2 * num - normalized - camPos);
				(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4 + 2, smoothPos - vector2 * num + normalized - camPos);
				(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4 + 3, smoothPos + vector2 * num + normalized - camPos);
				vector = smoothPos;
			}
			for (int j = 0; j < (sLeaser.sprites[sprite] as TriangleMesh).verticeColors.Length; j++)
			{
				float num2 = (float)j / (float)((sLeaser.sprites[sprite] as TriangleMesh).verticeColors.Length - 1);
				(sLeaser.sprites[sprite] as TriangleMesh).verticeColors[j] = new Color(GetCol(j).r, GetCol(j).g, GetCol(j).b, Mathf.Pow(1f - num2, 2f) * 0.25f * Mathf.InverseLerp(0.4f, 0.6f, rCam.room.Darkness(owner.EyePos(timeStacker))));
			}
		}

		public void Update()
		{
			positionsList.Insert(0, owner.EyePos(1f));
			if (positionsList.Count > 10)
			{
				positionsList.RemoveAt(10);
			}
			colorsList.Insert(0, owner.EyeColor);
			if (colorsList.Count > 10)
			{
				colorsList.RemoveAt(10);
			}
		}
	}

	public abstract class Plumage
	{
		public class Feather
		{
			private Plumage owner;

			public int index;

			public Vector2 pos;

			public Vector2 lastPos;

			public Vector2 vel;

			public float length;

			public float lazy;

			public Feather(Plumage owner, int index, float length)
			{
				this.owner = owner;
				this.index = index;
				this.length = length;
				if (UnityEngine.Random.value < 1f / 21f)
				{
					lazy = Mathf.Pow(UnityEngine.Random.value, 0.2f);
				}
				else if (UnityEngine.Random.value < 0.5f)
				{
					lazy = Mathf.Pow(UnityEngine.Random.value, 4f);
				}
			}

			public void Update()
			{
				lastPos = pos;
				pos += vel;
				vel *= owner.owner.plumageFriction;
				vel.y -= owner.owner.plumageGravity;
				vel += owner.FeatherDirection(index) * owner.owner.plumageDirection * (1f - lazy);
				Vector2 vector = owner.FeatherConnectionPos(index, 1f);
				float num = Vector2.Distance(pos, vector);
				Vector2 vector2 = Custom.DirVec(pos, vector);
				pos -= (length - num) * vector2 * 1f;
				vel -= (length - num) * vector2 * 1f;
			}
		}

		public MirosBirdGraphics owner;

		public int totalSprites;

		public int firstSprite;

		public Feather[] feathers;

		public Plumage(MirosBirdGraphics owner, int firstSprite)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
		}

		public void Reset()
		{
			for (int i = 0; i < feathers.Length; i++)
			{
				feathers[i].pos = FeatherConnectionPos(i, 1f);
				feathers[i].lastPos = feathers[i].pos;
				feathers[i].vel *= 0f;
			}
		}

		public virtual Vector2 FeatherConnectionPos(int index, float timeStacker)
		{
			return new Vector2(0f, 0f);
		}

		public virtual Vector2 FeatherDirection(int index)
		{
			return new Vector2(0f, 0f);
		}

		public virtual float FeatherFlip(int index, float timeStacker)
		{
			return 1f;
		}

		public void Update()
		{
			for (int i = 0; i < feathers.Length; i++)
			{
				feathers[i].Update();
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < feathers.Length; i++)
			{
				sLeaser.sprites[firstSprite + i] = new FSprite("LizardScaleA" + owner.plumageGraphic);
				sLeaser.sprites[firstSprite + i].anchorY = 0f;
				sLeaser.sprites[firstSprite + i].scaleY = feathers[i].length / owner.plumageGraphLength;
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < feathers.Length; i++)
			{
				Vector2 p = FeatherConnectionPos(i, timeStacker);
				sLeaser.sprites[firstSprite + i].x = p.x - camPos.x;
				sLeaser.sprites[firstSprite + i].y = p.y - camPos.y;
				sLeaser.sprites[firstSprite + i].rotation = Custom.AimFromOneVectorToAnother(p, Vector2.Lerp(feathers[i].lastPos, feathers[i].pos, timeStacker));
				sLeaser.sprites[firstSprite + i].scaleX = owner.plumageWidth * FeatherFlip(i, timeStacker);
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}
	}

	public class NeckPlumage : Plumage
	{
		private bool dirType;

		public NeckPlumage(MirosBirdGraphics owner, int firstSprite)
			: base(owner, firstSprite)
		{
			_ = UnityEngine.Random.value;
			float num = Mathf.Lerp(6f, 30f, Mathf.Pow(owner.plumageDensity, 1.7f));
			feathers = new Feather[(int)num];
			int num2 = UnityEngine.Random.Range(0, 3);
			for (int i = 0; i < feathers.Length; i++)
			{
				float num3 = (float)i / (float)(feathers.Length - 1);
				float num4 = 1f;
				switch (num2)
				{
				case 0:
					num4 = UnityEngine.Random.value;
					break;
				case 1:
					num4 = Mathf.Sin(num3 * (float)Math.PI);
					break;
				case 2:
					num4 = num3;
					if (num3 == 1f)
					{
						num4 = 0.65f;
					}
					break;
				}
				feathers[i] = new Feather(this, i, owner.plumageGraphLength * owner.plumageLength * num4 * Mathf.Lerp(0.9f, 1.1f, UnityEngine.Random.value));
			}
			dirType = UnityEngine.Random.value < 0.5f;
			totalSprites = feathers.Length;
		}

		private Vector2 NeckPos(float f, float timeStacker)
		{
			return OneDimensionalNeckPos(f, timeStacker) + NeckPerpendicular(f, timeStacker) * NeckFlip(f, timeStacker) * NeckRad(f);
		}

		private float NeckFlip(float f, float timeStacker)
		{
			return 0f - Mathf.Lerp(Mathf.Lerp(owner.bird.lastBodyFlip, owner.bird.bodyFlip, timeStacker), Mathf.Lerp(owner.lastHeadFlip, owner.headFlip, timeStacker), 0.5f + 0.5f * f);
		}

		private Vector2 OneDimensionalNeckPos(float f, float timeStacker)
		{
			int num = Custom.IntClamp((int)(f * 3f), 0, 3);
			int num2 = Custom.IntClamp((int)(f * 3f + 1f), 0, 4);
			return Vector2.Lerp(NeckKeyPos(num, timeStacker), NeckKeyPos(num2, timeStacker), Mathf.InverseLerp(num, num2, f * 3f));
		}

		private float NeckRad(float f)
		{
			int num = Custom.IntClamp((int)(f * 3f), 0, 3);
			int num2 = Custom.IntClamp((int)(f * 3f + 1f), 0, 4);
			return Mathf.Lerp(NeckKeyRad(num), NeckKeyRad(num2), Mathf.InverseLerp(num, num2, f * 3f));
		}

		private Vector2 NeckDir(float f, float timeStacker)
		{
			int num = Custom.IntClamp((int)(f * 3f), 0, 3);
			int num2 = Custom.IntClamp((int)(f * 3f + 1f), 0, 4);
			return Vector2.Lerp(NeckKeyDir(num, timeStacker), NeckKeyDir(num2, timeStacker), Mathf.InverseLerp(num, num2, f * 3f));
		}

		private Vector2 NeckPerpendicular(float f, float timeStacker)
		{
			return Custom.PerpendicularVector(NeckDir(f, timeStacker));
		}

		public override Vector2 FeatherConnectionPos(int index, float timeStacker)
		{
			float f = (float)index / (float)(feathers.Length - 1);
			return NeckPos(f, timeStacker);
		}

		public override Vector2 FeatherDirection(int index)
		{
			float num = (float)index / (float)(feathers.Length - 1);
			Vector2 vector = NeckDir(num, 1f);
			if (dirType)
			{
				return (vector * Mathf.Lerp(-1f, 1f, num) + Custom.PerpendicularVector(vector) * NeckFlip(num, 1f)).normalized;
			}
			return (Custom.PerpendicularVector(vector) * NeckFlip(num, 1f)).normalized;
		}

		public override float FeatherFlip(int index, float timeStacker)
		{
			float f = (float)index / (float)(feathers.Length - 1);
			float f2 = NeckFlip(f, timeStacker);
			return Mathf.Pow(Mathf.Abs(f2), 0.2f) * Mathf.Sign(f2);
		}

		private Vector2 NeckKeyDir(int index, float timeStacker)
		{
			if (index < 4)
			{
				return Custom.DirVec(NeckKeyPos(index, timeStacker), NeckKeyPos(index + 1, timeStacker));
			}
			return Custom.DirVec(NeckKeyPos(index - 1, timeStacker), NeckKeyPos(index, timeStacker)) + (Vector2)Futile.mousePosition;
		}

		private Vector2 NeckKeyPos(int index, float timeStacker)
		{
			if (index == 0)
			{
				return Vector2.Lerp(owner.bird.bodyChunks[1].lastPos, owner.bird.bodyChunks[1].pos, timeStacker);
			}
			if (index >= 4)
			{
				Vector2 vector = Vector2.Lerp(owner.bird.Head.lastPos, owner.bird.Head.pos, timeStacker);
				return vector + Custom.DirVec(Vector2.Lerp(owner.bird.neck.Tip.lastPos, owner.bird.neck.Tip.pos, timeStacker), vector) * 7f * (index - 3);
			}
			index--;
			return Vector2.Lerp(owner.bird.neck.tChunks[index].lastPos, owner.bird.neck.tChunks[index].pos, timeStacker);
		}

		private float NeckKeyRad(int index)
		{
			if (index == 0)
			{
				return owner.bird.bodyChunks[1].rad - 2f;
			}
			if (index >= 4)
			{
				return 6f;
			}
			index--;
			return owner.bird.neck.tChunks[index].rad;
		}
	}

	public class TailPlumage : Plumage
	{
		private Vector2[] positions;

		private float[] flips;

		public TailPlumage(MirosBirdGraphics owner, int firstSprite)
			: base(owner, firstSprite)
		{
			feathers = new Feather[(int)Mathf.Lerp(2f, 30f, owner.plumageDensity)];
			positions = new Vector2[feathers.Length];
			flips = new float[feathers.Length];
			int num = 3;
			float num2 = Mathf.Lerp(1f, 3f, Mathf.Pow(UnityEngine.Random.value, 4f));
			if (UnityEngine.Random.value < 0.05f)
			{
				num2 = UnityEngine.Random.value;
			}
			for (int i = 0; i < feathers.Length; i++)
			{
				positions[i] = new Vector2(-1f + 2f * UnityEngine.Random.value, UnityEngine.Random.value);
				flips[i] = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
				float num3 = 1f;
				switch (num)
				{
				case 0:
					num3 = UnityEngine.Random.value;
					break;
				case 2:
					num3 = 1f;
					break;
				case 3:
				case 4:
					num3 = positions[i].y;
					break;
				}
				feathers[i] = new Feather(this, i, owner.plumageGraphLength * owner.plumageLength * num3 * Mathf.Lerp(0.9f, 1.1f, UnityEngine.Random.value) * num2);
				feathers[i].lazy = Mathf.Lerp(feathers[i].lazy, 1f, 0.5f);
			}
			totalSprites = feathers.Length;
		}

		public Vector2 BodPos(float timeStacker)
		{
			return Vector2.Lerp(owner.bird.mainBodyChunk.lastPos, owner.bird.mainBodyChunk.pos, timeStacker);
		}

		public Vector2 BodDir(float timeStacker)
		{
			Vector2 p = BodPos(timeStacker);
			return Custom.DirVec(Vector2.Lerp(owner.bird.bodyChunks[1].lastPos, owner.bird.bodyChunks[1].pos, timeStacker), p);
		}

		private float SideFac(int index, float timeStacker)
		{
			float num = Mathf.Lerp(0.5f + Mathf.Lerp(owner.bird.lastBodyFlip, owner.bird.bodyFlip, timeStacker) * 0.5f, 0.5f + 0.5f * Mathf.Sin(owner.bird.RunCycle(0f, timeStacker) * (float)Math.PI * 2f), (Mathf.Lerp(owner.bird.legs[0].lastRunMode, owner.bird.legs[0].runMode, timeStacker) + Mathf.Lerp(owner.bird.legs[1].lastRunMode, owner.bird.legs[1].runMode, timeStacker)) / 2f);
			num = -1f + 2f * num;
			num = Mathf.Sin((positions[index].x + num * 0.5f) * (float)Math.PI * 2f);
			return num * Mathf.Sin((0.5f + positions[index].y * 0.5f) * (float)Math.PI);
		}

		public override Vector2 FeatherConnectionPos(int index, float timeStacker)
		{
			Vector2 vector = Custom.PerpendicularVector(BodDir(timeStacker)) * SideFac(index, timeStacker) * 18f;
			return BodPos(timeStacker) + BodDir(timeStacker) * 20f * positions[index].y + vector;
		}

		public override Vector2 FeatherDirection(int index)
		{
			return Custom.DirVec(FeatherConnectionPos(index, 1f), BodPos(1f) + BodDir(1f) * (1f + positions[index].y * 2f) * feathers[index].length);
		}

		public override float FeatherFlip(int index, float timeStacker)
		{
			return flips[index];
		}
	}

	private MirosBird bird;

	private int lastPlumageSprite;

	public LegGraphic[] legs;

	public BeakGraphic[] beak;

	public EyeTrail eyeTrail;

	public List<Plumage> plumage;

	private Color eyeCol;

	public LightSource[] lightSources;

	public int plumageGraphic;

	public float plumageGraphLength;

	public float neckFatness;

	public float beakFatness;

	public float plumageLength;

	public float plumageWidth;

	public float plumageGravity;

	public float plumageFriction;

	public float plumageDirection;

	public float plumageDensity;

	public float eyeSize;

	public float tighSize;

	public float headFlip;

	public float lastHeadFlip;

	private int NeckSprite => 0;

	private int BodySprite => 1;

	private int FirstLegSprite => 2;

	private int LastLegSprite => FirstLegSprite + legs[0].totalSprites + legs[1].totalSprites - 1;

	private int FirstBeakSprite => LastLegSprite + 1;

	private int LastBeakSprite => FirstBeakSprite + beak[0].totalSprites + beak[1].totalSprites - 1;

	private int HeadSprite => LastBeakSprite + 1;

	private int FirstPlumageSprite => LastBeakSprite + 2;

	private int EyeSprite => lastPlumageSprite + 1;

	private int EyeTrailSprite => lastPlumageSprite + 2;

	private int TotalSprites => lastPlumageSprite + 3;

	public Color EyeColor
	{
		get
		{
			if (bird.Blinded)
			{
				return Custom.HSL2RGB(UnityEngine.Random.value, 1f, 0.5f + 0.5f * UnityEngine.Random.value);
			}
			return eyeCol;
		}
	}

	public Vector2 EyePos(float timeStacker)
	{
		float f = Mathf.Lerp(lastHeadFlip, headFlip, timeStacker);
		Vector2 vector = Vector2.Lerp(bird.Head.lastPos, bird.Head.pos, timeStacker);
		Vector2 vector2 = Custom.PerpendicularVector(Custom.DirVec(Vector2.Lerp(bird.neck.Tip.lastPos, bird.neck.Tip.pos, timeStacker), vector));
		return vector + vector2 * Mathf.Sign(f) * 10f * (1f - Mathf.Abs(f));
	}

	public MirosBirdGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		bird = base.owner as MirosBird;
		cullRange = 1400f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(bird.abstractCreature.ID.RandomSeed);
		plumageGraphic = UnityEngine.Random.Range(0, 7);
		plumageGraphLength = Futile.atlasManager.GetElementWithName("LizardScaleA" + plumageGraphic).sourcePixelSize.y;
		neckFatness = Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value);
		beakFatness = UnityEngine.Random.value;
		plumageLength = Mathf.Lerp(0.2f, 1.2f, Mathf.Pow(UnityEngine.Random.value, 0.75f));
		plumageWidth = Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		plumageGravity = Mathf.Lerp(0.2f, 1.2f, UnityEngine.Random.value);
		plumageFriction = Mathf.Lerp(0.7f, 1f, UnityEngine.Random.value);
		plumageDirection = Mathf.Lerp(1f, 11f, UnityEngine.Random.value);
		plumageDensity = UnityEngine.Random.value;
		eyeSize = Mathf.Lerp(0.5f, 1.4f, UnityEngine.Random.value);
		tighSize = Mathf.Lerp(0.5f, 1.4f, UnityEngine.Random.value);
		legs = new LegGraphic[2];
		int num = FirstLegSprite;
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i] = new LegGraphic(this, bird.legs[i], num);
			num += legs[i].totalSprites;
		}
		beak = new BeakGraphic[2];
		num = FirstBeakSprite;
		for (int j = 0; j < 2; j++)
		{
			beak[j] = new BeakGraphic(this, j, num);
			num += beak[j].totalSprites;
		}
		plumage = new List<Plumage>();
		num = FirstPlumageSprite;
		if (UnityEngine.Random.value > 1f / 18f)
		{
			plumage.Add(new NeckPlumage(this, num));
			num += plumage[plumage.Count - 1].totalSprites;
		}
		if (UnityEngine.Random.value > 1f / 18f)
		{
			plumage.Add(new TailPlumage(this, num));
			num += plumage[plumage.Count - 1].totalSprites;
		}
		lastPlumageSprite = num - 1;
		eyeCol = Custom.HSL2RGB(Mathf.Lerp(0.08f, 0.17f, UnityEngine.Random.value), 1f, 0.5f);
		UnityEngine.Random.state = state;
		lightSources = new LightSource[2];
		eyeTrail = new EyeTrail(this, EyeTrailSprite);
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < plumage.Count; i++)
		{
			plumage[i].Reset();
		}
		eyeTrail.Reset();
	}

	public override void Update()
	{
		lastHeadFlip = headFlip;
		if (Custom.DistanceToLine(bird.Head.pos, bird.bodyChunks[1].pos, bird.bodyChunks[0].pos) < 0f)
		{
			headFlip = Mathf.Min(1f, headFlip + 1f / 6f);
		}
		else
		{
			headFlip = Mathf.Max(-1f, headFlip - 1f / 6f);
		}
		base.Update();
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].Update();
		}
		for (int j = 0; j < 2; j++)
		{
			beak[j].Update();
		}
		if (!culled)
		{
			for (int k = 0; k < plumage.Count; k++)
			{
				plumage[k].Update();
			}
		}
		eyeTrail.Update();
		for (int l = 0; l < lightSources.Length; l++)
		{
			if (lightSources[l] != null)
			{
				lightSources[l].stayAlive = true;
				if (lightSources[l].slatedForDeletetion || bird.room.Darkness(bird.mainBodyChunk.pos) == 0f)
				{
					lightSources[l] = null;
				}
			}
			else if (bird.room.Darkness(bird.mainBodyChunk.pos) > 0f)
			{
				lightSources[l] = new LightSource(bird.legs[l].Foot.pos, environmentalLight: false, new Color(1f, 1f, 0.8f), bird);
				lightSources[l].requireUpKeep = true;
				bird.room.AddObject(lightSources[l]);
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[HeadSprite] = new FSprite("Circle20");
		sLeaser.sprites[HeadSprite].scaleX = 0.9f;
		sLeaser.sprites[HeadSprite].scaleY = 1.2f;
		sLeaser.sprites[EyeSprite] = new FSprite("Circle20");
		sLeaser.sprites[EyeSprite].scale = 0.3f * eyeSize;
		sLeaser.sprites[BodySprite] = new FSprite("MirosBody");
		sLeaser.sprites[BodySprite].anchorY = 0.4f;
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].InitiateSprites(sLeaser, rCam);
		}
		for (int j = 0; j < 2; j++)
		{
			beak[j].InitiateSprites(sLeaser, rCam);
		}
		for (int k = 0; k < plumage.Count; k++)
		{
			plumage[k].InitiateSprites(sLeaser, rCam);
		}
		eyeTrail.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites[NeckSprite] = TriangleMesh.MakeLongMesh(bird.neck.tChunks.Length, pointyTip: false, customColor: false);
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
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (i != EyeTrailSprite)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[EyeTrailSprite]);
		if (sLeaser.containers != null)
		{
			FContainer[] containers = sLeaser.containers;
			foreach (FContainer node in containers)
			{
				newContatiner.AddChild(node);
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		float num = Mathf.Lerp(lastHeadFlip, headFlip, timeStacker);
		Vector2 vector = Vector2.Lerp(bird.Head.lastPos, bird.Head.pos, timeStacker);
		Vector2 vector2 = Custom.DirVec(Vector2.Lerp(bird.neck.Tip.lastPos, bird.neck.Tip.pos, timeStacker), vector);
		Vector2 headPerp = Custom.PerpendicularVector(vector2);
		eyeTrail.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		float num2 = Custom.VecToDeg(vector2);
		for (int i = 0; i < 2; i++)
		{
			beak[i].DrawSprites(sLeaser, rCam, timeStacker, camPos, vector, vector2, headPerp, num2, num);
		}
		for (int j = 0; j < lightSources.Length; j++)
		{
			if (lightSources[j] != null)
			{
				if (bird.legs[j].lightUp > 0)
				{
					lightSources[j].HardSetAlpha(Mathf.Pow(UnityEngine.Random.value, 0.5f) * 0.7f);
					lightSources[j].HardSetRad(Mathf.Lerp(30f, 50f, UnityEngine.Random.value));
					lightSources[j].HardSetPos(Vector2.Lerp(bird.legs[j].lightUpPos1, bird.legs[j].lightUpPos2, UnityEngine.Random.value));
				}
				else
				{
					lightSources[j].HardSetAlpha(0f);
				}
			}
		}
		sLeaser.sprites[HeadSprite].x = vector.x - camPos.x;
		sLeaser.sprites[HeadSprite].y = vector.y - camPos.y;
		sLeaser.sprites[HeadSprite].rotation = num2;
		Vector2 vector3 = EyePos(timeStacker);
		sLeaser.sprites[EyeSprite].x = vector3.x - camPos.x;
		sLeaser.sprites[EyeSprite].y = vector3.y - camPos.y;
		sLeaser.sprites[EyeSprite].rotation = num2;
		sLeaser.sprites[EyeSprite].scaleX = 0.3f * Mathf.Abs(num) * eyeSize;
		Vector2 vector4 = Vector2.Lerp(bird.mainBodyChunk.lastPos, bird.mainBodyChunk.pos, timeStacker);
		sLeaser.sprites[BodySprite].x = vector4.x - camPos.x;
		sLeaser.sprites[BodySprite].y = vector4.y - camPos.y;
		Vector2 vector5 = Custom.DirVec(vector4, Vector2.Lerp(bird.bodyChunks[1].lastPos, bird.bodyChunks[1].pos, timeStacker));
		sLeaser.sprites[BodySprite].rotation = Custom.AimFromOneVectorToAnother(vector4, Vector2.Lerp(bird.bodyChunks[1].lastPos, bird.bodyChunks[1].pos, timeStacker));
		sLeaser.sprites[BodySprite].scaleX = Custom.LerpMap(Vector2.Distance(Vector2.Lerp(bird.legs[0].Hip.lastPos, bird.legs[0].Hip.pos, timeStacker), Vector2.Lerp(bird.legs[1].Hip.lastPos, bird.legs[1].Hip.pos, timeStacker)), 5f, 50f, 0.75f, 1.1f);
		for (int k = 0; k < legs.Length; k++)
		{
			legs[k].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		Vector2 vector6 = Vector2.Lerp(bird.neck.connectedChunk.lastPos, bird.neck.connectedChunk.pos, timeStacker);
		float num3 = 8f;
		for (int l = 0; l < bird.neck.tChunks.Length; l++)
		{
			Vector2 vector7 = Vector2.Lerp(bird.neck.tChunks[l].lastPos, bird.neck.tChunks[l].pos, timeStacker);
			if (l == bird.neck.tChunks.Length - 1)
			{
				vector7 = Vector2.Lerp(vector7, vector, 0.5f);
			}
			else if (l == 0)
			{
				vector7 = Vector2.Lerp(vector7, vector4 + vector5 * 40f, 0.3f);
			}
			Vector2 normalized = (vector7 - vector6).normalized;
			Vector2 vector8 = Custom.PerpendicularVector(normalized);
			float num4 = Vector2.Distance(vector7, vector6) / 5f;
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(l * 4, vector6 - vector8 * (bird.neck.tChunks[l].stretchedRad + num3) * 0.5f * neckFatness + normalized * num4 * ((l == 0) ? 0f : 1f) - camPos);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(l * 4 + 1, vector6 + vector8 * (bird.neck.tChunks[l].stretchedRad + num3) * 0.5f * neckFatness + normalized * num4 * ((l == 0) ? 0f : 1f) - camPos);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(l * 4 + 2, vector7 - vector8 * bird.neck.tChunks[l].stretchedRad * neckFatness - normalized * num4 * ((l == bird.neck.tChunks.Length - 1) ? 0f : 1f) - camPos);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(l * 4 + 3, vector7 + vector8 * bird.neck.tChunks[l].stretchedRad * neckFatness - normalized * num4 * ((l == bird.neck.tChunks.Length - 1) ? 0f : 1f) - camPos);
			num3 = bird.neck.tChunks[l].stretchedRad;
			vector6 = vector7;
		}
		sLeaser.sprites[EyeSprite].color = EyeColor;
		for (int m = 0; m < plumage.Count; m++)
		{
			plumage[m].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (i != EyeTrailSprite)
			{
				sLeaser.sprites[i].color = palette.blackColor;
			}
		}
		for (int j = 0; j < plumage.Count; j++)
		{
			plumage[j].ApplyPalette(sLeaser, rCam, palette);
		}
	}
}
