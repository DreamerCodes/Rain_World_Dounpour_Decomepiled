using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class NSHSwarmer : PhysicalObject, IDrawable
{
	public class Shape
	{
		public class ShapeType : ExtEnum<ShapeType>
		{
			public static readonly ShapeType Main = new ShapeType("Main", register: true);

			public static readonly ShapeType Shell = new ShapeType("Shell", register: true);

			public static readonly ShapeType Belt = new ShapeType("Belt", register: true);

			public static readonly ShapeType DiamondHolder = new ShapeType("DiamondHolder", register: true);

			public static readonly ShapeType SmallDiamondHolder = new ShapeType("SmallDiamondHolder", register: true);

			public static readonly ShapeType Diamond = new ShapeType("Diamond", register: true);

			public static readonly ShapeType Cube = new ShapeType("Cube", register: true);

			public static readonly ShapeType Ribbon = new ShapeType("Ribbon", register: true);

			public static readonly ShapeType Sphere = new ShapeType("Sphere", register: true);

			public static readonly ShapeType BigDiamonds = new ShapeType("BigDiamonds", register: true);

			public static readonly ShapeType BigDiamonds2 = new ShapeType("BigDiamonds2", register: true);

			public ShapeType(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public class FloatChanger
		{
			public float prog;

			public float lastProg;

			public float from;

			public float to;

			public float speed;

			public float SmoothValue(float timeStacker)
			{
				return Mathf.Lerp(from, to, Custom.SCurve(Mathf.Lerp(lastProg, prog, timeStacker), 0.65f));
			}

			public FloatChanger()
			{
				prog = 1f;
				lastProg = 1f;
			}

			public void Update()
			{
				lastProg = prog;
				prog = Mathf.Min(1f, prog + speed);
			}

			public void NewGoal(float goal, float distanceTravelledInOneFrame, float framesToGetToGoal, float absTimeFac)
			{
				if (!(prog < 1f) && !(lastProg < 1f) && goal != to)
				{
					from = to;
					to = goal;
					prog = 0f;
					lastProg = 0f;
					speed = Mathf.Lerp(distanceTravelledInOneFrame / Mathf.Abs(from - to), 1f / framesToGetToGoal, absTimeFac);
				}
			}
		}

		public class Vert
		{
			public Vector3 A;

			public Vector3 B;

			public Vector3 C;

			public Vector3 lastPos;

			public Vector3 pos;

			public Vector2 drawPos;

			public Vector3 errorDrift;

			public Vector3 errorDriftTarget;

			public float errors;

			public bool on;

			public Vert(float x, float y, float z)
			{
				A = new Vector3(x, y, z);
				B = A;
				C = A;
			}
		}

		public class Line
		{
			public Vert A;

			public Vert B;

			public Line(Vert A, Vert B)
			{
				this.A = A;
				this.B = B;
			}
		}

		public Shape owner;

		public Vector3 pos;

		public Vector3 lastPos;

		public Vector3 startPos;

		public List<Vert> verts = new List<Vert>();

		public List<Line> holoLines = new List<Line>();

		public List<Shape> subShapes = new List<Shape>();

		public ShapeType shapeType;

		public FloatChanger[] floatChangers = new FloatChanger[8]
		{
			new FloatChanger(),
			new FloatChanger(),
			new FloatChanger(),
			new FloatChanger(),
			new FloatChanger(),
			new FloatChanger(),
			new FloatChanger(),
			new FloatChanger()
		};

		private int dRotA;

		private int dRotB;

		private bool shakeError;

		public float[] rotats;

		public FloatChanger MainRotation => floatChangers[0];

		public FloatChanger Height => floatChangers[1];

		public FloatChanger ShapeA => floatChangers[2];

		public FloatChanger ShapeB => floatChangers[3];

		public FloatChanger Errors => floatChangers[4];

		public FloatChanger Fade => floatChangers[5];

		public FloatChanger DRotA => floatChangers[6];

		public FloatChanger DRotB => floatChangers[7];

		public int LinesCount
		{
			get
			{
				int num = holoLines.Count;
				for (int i = 0; i < subShapes.Count; i++)
				{
					num += subShapes[i].LinesCount;
				}
				return num;
			}
		}

		public Shape(Shape owner, ShapeType shapeType, Vector3 pos, float width, float height)
		{
			this.owner = owner;
			this.shapeType = shapeType;
			this.pos = pos;
			startPos = pos;
			lastPos = pos;
			int num = 0;
			if (shapeType == ShapeType.Main)
			{
				subShapes.Add(new Shape(this, ShapeType.Shell, pos, 9f, 22f));
				subShapes.Add(new Shape(this, ShapeType.Belt, pos, 25f, 6f));
				subShapes.Add(new Shape(this, ShapeType.DiamondHolder, pos, 35f, 0f));
				subShapes.Add(new Shape(this, ShapeType.Cube, pos, 27f, 27f));
				subShapes.Add(new Shape(this, ShapeType.Ribbon, pos, 44f, 7f));
				subShapes.Add(new Shape(this, ShapeType.Sphere, pos, 36f, 28f));
				subShapes.Add(new Shape(this, ShapeType.BigDiamonds, pos, 39f, 11f));
			}
			else if (shapeType == ShapeType.Shell)
			{
				num = 5;
				verts.Add(new Vert(0f, 0f, 0f - height));
				verts[0].B *= 0.5f;
				verts[0].C *= 1.5f;
				verts.Add(new Vert(0f, 0f, height));
				verts[1].B *= 0.5f;
				verts[1].C *= 1.5f;
				for (int i = 0; i < num; i++)
				{
					Vector2 vector = Custom.DegToVec((float)i / (float)num * 360f) * width;
					verts.Add(new Vert(vector.x, vector.y, (0f - height) / 2.2f));
					verts[verts.Count - 1].B *= 1.2f;
					verts[verts.Count - 1].B.z *= 0.2f;
					verts[verts.Count - 1].C *= 0.8f;
					verts[verts.Count - 1].C.z *= 2f;
					verts.Add(new Vert(vector.x, vector.y, height / 2.2f));
					verts[verts.Count - 1].B *= 1.2f;
					verts[verts.Count - 1].B.z *= 0.2f;
					verts[verts.Count - 1].C *= 0.8f;
					verts[verts.Count - 1].C.z *= 2f;
				}
				for (int j = 0; j < num; j++)
				{
					int num2 = ((j < num - 1) ? (j + 1) : 0);
					holoLines.Add(new Line(verts[2 + j * 2], verts[0]));
					holoLines.Add(new Line(verts[2 + j * 2], verts[2 + num2 * 2]));
					holoLines.Add(new Line(verts[2 + j * 2 + 1], verts[2 + num2 * 2 + 1]));
					holoLines.Add(new Line(verts[2 + j * 2], verts[2 + j * 2 + 1]));
					holoLines.Add(new Line(verts[2 + j * 2 + 1], verts[1]));
				}
			}
			else if (shapeType == ShapeType.Belt)
			{
				num = 7;
				for (int k = 0; k < num; k++)
				{
					Vector2 vector2 = Custom.DegToVec((float)k / (float)num * 360f);
					verts.Add(new Vert(vector2.x * width, vector2.y * width, (0f - height) * 1.2f));
					verts[verts.Count - 1].B = new Vector3(vector2.x * (width - height * 0.25f), vector2.y * (width - height * 0.25f), 0f);
					verts[verts.Count - 1].C = new Vector3(vector2.x * (width + height * 2f), vector2.y * (width + height * 1.5f), 0f);
					verts.Add(new Vert(vector2.x * width, vector2.y * width, height * 1.2f));
					verts[verts.Count - 1].B = new Vector3(vector2.x * (width + height * 2f), vector2.y * (width + height * 2f), 0f);
					verts[verts.Count - 1].C = new Vector3(vector2.x * (width - height * 0.25f), vector2.y * (width - height * 0.25f), 0f);
				}
				for (int l = 0; l < num; l++)
				{
					int num3 = ((l < num - 1) ? (l + 1) : 0);
					holoLines.Add(new Line(verts[l * 2], verts[num3 * 2]));
					holoLines.Add(new Line(verts[l * 2 + 1], verts[num3 * 2 + 1]));
					holoLines.Add(new Line(verts[l * 2], verts[l * 2 + 1]));
				}
			}
			else if (shapeType == ShapeType.DiamondHolder)
			{
				num = 5;
				for (int m = 0; m < num; m++)
				{
					Vector2 vector3 = Custom.DegToVec((float)m / (float)num * 360f) * width;
					subShapes.Add(new Shape(this, ShapeType.Diamond, new Vector3(vector3.x, vector3.y, 0f), 3f, 5f));
				}
				subShapes.Add(new Shape(this, ShapeType.SmallDiamondHolder, pos + new Vector3(0f, 0f, -22f), 20f, 0f));
				subShapes.Add(new Shape(this, ShapeType.SmallDiamondHolder, pos + new Vector3(0f, 0f, 22f), 20f, 0f));
			}
			else if (shapeType == ShapeType.SmallDiamondHolder)
			{
				num = 3;
				for (int n = 0; n < num; n++)
				{
					Vector2 vector4 = Custom.DegToVec((float)n / (float)num * 360f) * width;
					subShapes.Add(new Shape(this, ShapeType.Diamond, new Vector3(vector4.x, vector4.y, 0f), 3f, 5f));
				}
			}
			else if (shapeType == ShapeType.Diamond)
			{
				verts.Add(new Vert(0f - width, 0f, 0f));
				verts.Add(new Vert(0f, 0f, height));
				verts.Add(new Vert(width, 0f, 0f));
				verts.Add(new Vert(0f, 0f, 0f - height));
				num = 4;
				for (int num4 = 0; num4 < num; num4++)
				{
					int index = ((num4 < num - 1) ? (num4 + 1) : 0);
					holoLines.Add(new Line(verts[num4], verts[index]));
				}
			}
			else if (shapeType == ShapeType.Cube)
			{
				num = 4;
				for (int num5 = 0; num5 < num; num5++)
				{
					Vector2 vector5 = Custom.DegToVec((float)num5 / (float)num * 360f) * width * 1.42f;
					verts.Add(new Vert(vector5.x, vector5.y, 0f - height));
					verts[verts.Count - 1].B = MultVec(verts[verts.Count - 1].B, new Vector3(1.4f, 1.4f, 0.2f));
					verts[verts.Count - 1].C *= 0.2f;
					verts.Add(new Vert(vector5.x, vector5.y, height));
					verts[verts.Count - 1].B = MultVec(verts[verts.Count - 1].B, new Vector3(1.4f, 1.4f, 0.2f));
					verts[verts.Count - 1].C *= 0.2f;
				}
				for (int num6 = 0; num6 < num; num6++)
				{
					int num7 = ((num6 < num - 1) ? (num6 + 1) : 0);
					holoLines.Add(new Line(verts[num6 * 2], verts[num7 * 2]));
					holoLines.Add(new Line(verts[num6 * 2 + 1], verts[num7 * 2 + 1]));
					holoLines.Add(new Line(verts[num6 * 2], verts[num6 * 2 + 1]));
				}
			}
			else if (shapeType == ShapeType.Ribbon)
			{
				num = 22;
				for (int num8 = 0; num8 < num; num8++)
				{
					Vector2 vector6 = Custom.DegToVec((float)num8 / (float)num * 360f) * width;
					verts.Add(new Vert(vector6.x, vector6.y, 0f - height));
					verts.Add(new Vert(vector6.x, vector6.y, height));
					vector6 = Custom.DegToVec(((float)num8 + ((num8 % 2 == 0) ? (-0.5f) : 0.5f)) / (float)num * 360f) * width;
					verts[verts.Count - 2].B = new Vector3(vector6.x, vector6.y, (0f - height) * 0.75f);
					verts[verts.Count - 1].B = new Vector3(vector6.x, vector6.y, height * 0.75f);
					vector6 = Custom.DegToVec(((float)num8 + ((num8 % 2 == 0) ? 0.5f : (-0.5f))) / (float)num * 360f) * width;
					verts[verts.Count - 2].C = new Vector3(vector6.x, vector6.y, (0f - height) * 1.5f);
					verts[verts.Count - 1].C = new Vector3(vector6.x, vector6.y, height * 1.5f);
				}
				for (int num9 = 0; num9 < num; num9++)
				{
					int num10 = ((num9 < num - 1) ? (num9 + 1) : 0);
					if (num9 % 2 == 0)
					{
						holoLines.Add(new Line(verts[num9 * 2], verts[num10 * 2]));
						holoLines.Add(new Line(verts[num9 * 2 + 1], verts[num10 * 2 + 1]));
					}
					holoLines.Add(new Line(verts[num9 * 2], verts[num9 * 2 + 1]));
				}
			}
			else if (shapeType == ShapeType.Sphere)
			{
				num = 18;
				for (int num11 = 0; num11 < 2; num11++)
				{
					for (int num12 = 0; num12 < num; num12++)
					{
						Vector2 vector7 = Custom.DegToVec((float)num12 / (float)num * 360f) * width;
						verts.Add(new Vert(vector7.x, vector7.y, height * 0.5f * (float)((num11 != 0) ? 1 : (-1))));
						verts.Add(new Vert(vector7.x * 0.72f, vector7.y * 0.75f, height * (float)((num11 != 0) ? 1 : (-1))));
						vector7 = Custom.DegToVec(((float)num12 + ((num12 % 2 == num11) ? (-0.5f) : 0.5f)) / (float)num * 360f) * width;
						verts[verts.Count - 2].B = new Vector3(vector7.x, vector7.y, height * 0.5f * (float)((num11 != 0) ? 1 : (-1)));
						verts[verts.Count - 1].B = new Vector3(vector7.x * 0.72f, vector7.y * 0.72f, height * (float)((num11 != 0) ? 1 : (-1)));
						vector7 = Custom.DegToVec(((float)num12 + ((num12 % 2 != num11) ? (-0.5f) : 0.5f)) / (float)num * 360f) * width;
						verts[verts.Count - 2].C = new Vector3(vector7.x, vector7.y, height * 0.5f * (float)((num11 != 0) ? 1 : (-1)));
						verts[verts.Count - 1].C = new Vector3(vector7.x * 0.72f, vector7.y * 0.72f, height * (float)((num11 != 0) ? 1 : (-1)));
					}
				}
				for (int num13 = 0; num13 < 2; num13++)
				{
					for (int num14 = 0; num14 < num; num14++)
					{
						int num15 = ((num14 < num - 1) ? (num14 + 1) : 0);
						if (num14 % 2 == num13)
						{
							holoLines.Add(new Line(verts[num * 2 * num13 + num14 * 2], verts[num * 2 * num13 + num15 * 2]));
							holoLines.Add(new Line(verts[num * 2 * num13 + num14 * 2 + 1], verts[num * 2 * num13 + num15 * 2 + 1]));
						}
						holoLines.Add(new Line(verts[num * 2 * num13 + num14 * 2], verts[num * 2 * num13 + num14 * 2 + 1]));
					}
				}
			}
			else if (shapeType == ShapeType.BigDiamonds)
			{
				num = 7;
				for (int num16 = 0; num16 < num; num16++)
				{
					Vector2 vector8 = Custom.DegToVec((float)num16 / (float)num * 360f) * (width - height / 3f);
					Vector2 vector9 = Custom.DegToVec(((float)num16 - 0.15f) / (float)num * 360f) * width;
					Vector2 vector10 = Custom.DegToVec(((float)num16 + 0.15f) / (float)num * 360f) * width;
					verts.Add(new Vert(vector8.x, vector8.y, 0f - height));
					verts.Add(new Vert(vector9.x, vector9.y, 0f));
					verts.Add(new Vert(vector8.x, vector8.y, height));
					verts.Add(new Vert(vector10.x, vector10.y, 0f));
					for (int num17 = 0; num17 < 4; num17++)
					{
						int num18 = ((num17 < 3) ? (num17 + 1) : 0);
						holoLines.Add(new Line(verts[num16 * 4 + num17], verts[num16 * 4 + num18]));
					}
				}
				subShapes.Add(new Shape(this, ShapeType.BigDiamonds2, pos, width, height - 3.5f));
			}
			else
			{
				if (!(shapeType == ShapeType.BigDiamonds2))
				{
					return;
				}
				num = 7;
				for (int num19 = 0; num19 < num; num19++)
				{
					Vector2 vector11 = Custom.DegToVec((float)num19 / (float)num * 360f) * (width - height / 5f);
					Vector2 vector12 = Custom.DegToVec(((float)num19 - 0.08f) / (float)num * 360f) * width;
					Vector2 vector13 = Custom.DegToVec(((float)num19 + 0.08f) / (float)num * 360f) * width;
					verts.Add(new Vert(vector11.x, vector11.y, 0f - height));
					verts.Add(new Vert(vector12.x, vector12.y, 0f));
					verts.Add(new Vert(vector11.x, vector11.y, height));
					verts.Add(new Vert(vector13.x, vector13.y, 0f));
					for (int num20 = 0; num20 < 4; num20++)
					{
						int num21 = ((num20 < 3) ? (num20 + 1) : 0);
						holoLines.Add(new Line(verts[num19 * 4 + num20], verts[num19 * 4 + num21]));
					}
				}
			}
		}

		public void Update(bool changeLikely, float errors, float fade, Vector2 movement, float upRotat, ref float[,] directionsPower)
		{
			lastPos = pos;
			fade = Mathf.Min(fade, Fade.SmoothValue(1f));
			errors = Mathf.Max(Mathf.Max(errors, Errors.SmoothValue(1f)), (1f - fade) * 0.25f);
			if (errors > 0.5f && UnityEngine.Random.value < 0.25f)
			{
				fade *= Custom.LerpMap(errors * UnityEngine.Random.value, 0.5f, 1f, 1f, 0.3f);
			}
			for (int i = 0; i < subShapes.Count; i++)
			{
				subShapes[i].Update(changeLikely, errors, fade, movement, upRotat, ref directionsPower);
			}
			for (int j = 0; j < floatChangers.Length; j++)
			{
				floatChangers[j].Update();
			}
			if (UnityEngine.Random.value < 1f / (changeLikely ? 6f : 600f))
			{
				if (shapeType == ShapeType.BigDiamonds2 && UnityEngine.Random.value < 0.8f)
				{
					MainRotation.NewGoal(0f, 2f, 60f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.6f);
				}
				else
				{
					MainRotation.NewGoal(MainRotation.to + Mathf.Lerp(-360f, 360f, UnityEngine.Random.value), 2f, 60f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.3f);
				}
			}
			if (shapeType == ShapeType.BigDiamonds2)
			{
				if (UnityEngine.Random.value < 0.8f)
				{
					dRotA = owner.dRotA;
					DRotA.NewGoal((float)dRotA * (float)Math.PI, 1f / 30f, 30f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.5f);
					dRotB = owner.dRotB;
					DRotB.NewGoal((float)dRotB * (float)Math.PI, 1f / 30f, 30f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.5f);
				}
			}
			else
			{
				pos.z = startPos.z + Mathf.Lerp(-10f, 10f, Height.SmoothValue(1f));
			}
			if (UnityEngine.Random.value < 1f / (changeLikely ? 6f : 600f))
			{
				Height.NewGoal(UnityEngine.Random.value, 1f / 60f, 60f, 0.5f);
			}
			if (UnityEngine.Random.value < 1f / (changeLikely ? 6f : 600f))
			{
				ShapeA.NewGoal((UnityEngine.Random.value < 0.5f) ? 0f : Mathf.Pow(UnityEngine.Random.value, 0.6f), 1f / 60f, 60f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.5f);
			}
			if (UnityEngine.Random.value < 1f / (changeLikely ? 6f : 600f))
			{
				ShapeB.NewGoal(Custom.PushFromHalf(UnityEngine.Random.value, 2f), 1f / 60f, 60f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.5f);
			}
			if (UnityEngine.Random.value < 1f / 60f && verts.Count > 0)
			{
				shakeError = UnityEngine.Random.value < 0.5f;
				Errors.NewGoal((UnityEngine.Random.value < Mathf.Lerp(0.95f, 0.82f, errors)) ? 0f : Mathf.Pow(UnityEngine.Random.value, 0.75f), 0.025f, 60f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.5f);
			}
			if (UnityEngine.Random.value < 1f / (changeLikely ? 15f : 1500f))
			{
				if (owner != null && owner.owner != null && owner.Fade.to == 0f)
				{
					Fade.NewGoal(0f, 0.025f, 40f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.5f);
				}
				else if (shapeType == ShapeType.Cube || shapeType == ShapeType.Ribbon || shapeType == ShapeType.Sphere || shapeType == ShapeType.BigDiamonds)
				{
					float num = 0.2f;
					for (int k = 0; k < owner.subShapes.Count; k++)
					{
						if (owner.subShapes[k] != this && (owner.subShapes[k].shapeType == ShapeType.Ribbon || owner.subShapes[k].shapeType == ShapeType.Cube || owner.subShapes[k].shapeType == ShapeType.Sphere || owner.subShapes[k].shapeType == ShapeType.Belt || owner.subShapes[k].shapeType == ShapeType.BigDiamonds) && owner.subShapes[k].Fade.to != 0f)
						{
							num *= ((owner.subShapes[k].shapeType == ShapeType.Belt) ? 0.5f : 0f);
						}
					}
					Fade.NewGoal((UnityEngine.Random.value < num) ? 1f : 0f, 0.025f, 40f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.5f);
				}
				else
				{
					Fade.NewGoal((UnityEngine.Random.value < 0.75f) ? 1f : Mathf.Pow(Mathf.InverseLerp(0.75f, 1f, UnityEngine.Random.value), 0.5f), 0.025f, 40f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.5f);
				}
			}
			if (owner == null)
			{
				Fade.from = 1f;
				Fade.to = 1f;
				Fade.prog = 1f;
			}
			if (UnityEngine.Random.value < 1f / (changeLikely ? 22f : 2200f) && (shapeType == ShapeType.Shell || shapeType == ShapeType.Belt || shapeType == ShapeType.Cube || shapeType == ShapeType.Ribbon || shapeType == ShapeType.Sphere || (UnityEngine.Random.value < 0.4f && (shapeType == ShapeType.BigDiamonds || shapeType == ShapeType.BigDiamonds2))))
			{
				if (UnityEngine.Random.value < 0.5f)
				{
					dRotA += ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
					DRotA.NewGoal((float)dRotA * (float)Math.PI, 1f / 30f, 30f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.5f);
				}
				if (UnityEngine.Random.value < 0.5f)
				{
					dRotB += ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
					DRotB.NewGoal((float)dRotB * (float)Math.PI, 1f / 30f, 30f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value), 0.5f);
				}
			}
			float num2 = SmoothRotat(1f);
			Vector3 vector = MultVec(SmoothPos(1f), new Vector3(Mathf.Pow(fade, 0.3f), Mathf.Pow(fade, 0.3f), fade));
			for (int l = 0; l < verts.Count; l++)
			{
				verts[l].lastPos = verts[l].pos;
				Vector3 b = Rotate(Vector3.Lerp(verts[l].A, Vector3.Lerp(verts[l].B, verts[l].C, ShapeB.SmoothValue(1f)), ShapeA.SmoothValue(1f)), (float)Math.PI / 2f + DRotA.SmoothValue(1f), 0f + DRotB.SmoothValue(1f), num2 * ((float)Math.PI / 180f)) + vector;
				Vector2 vector2 = Custom.RotateAroundOrigo(new Vector2(b.x, b.y), upRotat);
				b.x = vector2.x;
				b.y = vector2.y;
				verts[l].errors = Mathf.Lerp(verts[l].errors, errors, 0.07f);
				if (UnityEngine.Random.value < 1f / 14f)
				{
					verts[l].errors = errors;
				}
				if (UnityEngine.Random.value < 1f / 14f)
				{
					if (verts[l].errors > 0.1f && UnityEngine.Random.value < 0.1f && fade > 0.3f)
					{
						verts[l].on = !verts[l].on;
					}
					else if (verts[l].errors > 0.45f && UnityEngine.Random.value < 0.2f * verts[l].errors)
					{
						verts[l].on = false;
					}
					else if (verts[l].errors < 0.45f && fade > 0.7f)
					{
						verts[l].on = true;
					}
				}
				verts[l].errorDrift *= Mathf.Lerp(0.4f, 0.8f, verts[l].errors);
				verts[l].errorDrift = Vector3.Lerp(verts[l].errorDrift, verts[l].errorDriftTarget, 0.1f * UnityEngine.Random.value * verts[l].errors);
				if (UnityEngine.Random.value < 0.1f)
				{
					verts[l].errorDriftTarget = UnityEngine.Random.insideUnitSphere * 0.2f;
				}
				if (verts[l].errors < 0.1f || UnityEngine.Random.value < 1f / Mathf.Lerp(10f, 100f, verts[l].errors))
				{
					verts[l].pos = b;
				}
				else
				{
					if (verts[l].errors > 0.75f)
					{
						verts[l].pos = Vector3.Lerp(verts[l].pos, new Vector3(0f, 0f, 0f), 0.5f * Mathf.Pow(Mathf.InverseLerp(0.75f, 1f, verts[l].errors), 5f) * UnityEngine.Random.value);
					}
					verts[l].pos += verts[l].errorDrift;
					verts[l].pos -= new Vector3(movement.x, movement.y, 0f);
					verts[l].pos = Vector3.Slerp(verts[l].pos, b, Custom.LerpMap(Vector3.Distance(verts[l].pos, b), 20f + 35f * UnityEngine.Random.value * errors, 7f, 0.1f, Mathf.Lerp(0.03f, 0.01f, Mathf.Pow(verts[l].errors, 0.5f))));
				}
				verts[l].pos = Vector3.Lerp(new Vector3(0f, 0f, 0f), verts[l].pos, Mathf.Pow(fade, 0.2f));
				if (verts[l].on && fade > 0f)
				{
					int num3 = Mathf.RoundToInt((360f + Custom.VecToDeg(verts[l].pos) + upRotat) * ((float)directionsPower.GetLength(0) / 360f)) % directionsPower.GetLength(0);
					directionsPower[num3, 2] = Mathf.Lerp(directionsPower[num3, 2], 1f, fade * (1f - verts[l].errors));
				}
			}
		}

		public void ResetUpdate(Vector2 v)
		{
			for (int i = 0; i < verts.Count; i++)
			{
				verts[i].drawPos = v;
			}
			Fade.from = 0f;
			Fade.to = 0f;
			Fade.prog = 1f;
			for (int j = 0; j < subShapes.Count; j++)
			{
				subShapes[j].ResetUpdate(v);
			}
		}

		public float SmoothRotat(float timeStacker)
		{
			if (owner == null)
			{
				return MainRotation.SmoothValue(timeStacker);
			}
			return MainRotation.SmoothValue(timeStacker) + owner.SmoothRotat(timeStacker);
		}

		protected Vector3 SmoothPos(float timeStacker)
		{
			if (owner == null)
			{
				return Vector3.Lerp(pos, lastPos, timeStacker);
			}
			return Rotate(Vector3.Lerp(pos, lastPos, timeStacker), (float)Math.PI / 2f + DRotA.SmoothValue(timeStacker), 0f + DRotB.SmoothValue(timeStacker), owner.SmoothRotat(timeStacker) * ((float)Math.PI / 180f)) + owner.SmoothPos(timeStacker);
		}

		private Vector3 MultVec(Vector3 A, Vector3 B)
		{
			return new Vector3(A.x * B.x, A.y * B.y, A.z * B.z);
		}

		private float DirPow(Vector2 v, float upRotat, float timeStacker, ref float[,] directionsPower)
		{
			float num = (360f + Custom.VecToDeg(v) + upRotat) * ((float)directionsPower.GetLength(0) / 360f);
			int num2 = Mathf.FloorToInt(num);
			return Mathf.Lerp(Mathf.Lerp(directionsPower[num2 % directionsPower.GetLength(0), 1], directionsPower[(num2 + 1) % directionsPower.GetLength(0), 1], Mathf.InverseLerp(num2, num2 + 1, num)), Mathf.Lerp(directionsPower[num2 % directionsPower.GetLength(0), 0], directionsPower[(num2 + 1) % directionsPower.GetLength(0), 0], Mathf.InverseLerp(num2, num2 + 1, num)), timeStacker);
		}

		public void Draw(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 projPos, Vector2 camPos, ref int sprite, float upRotat, float errors, float fade, bool shakeErr, ref Vector2 pointsVec, ref float pointsWeight, ref float maxDist, ref float[,] directionsPower)
		{
			shakeErr = shakeErr || shakeError;
			fade = Mathf.Min(fade, Fade.SmoothValue(timeStacker));
			errors = Mathf.Max(Mathf.Max(errors, Errors.SmoothValue(timeStacker)), (1f - fade) * 0.25f);
			if (UnityEngine.Random.value < 0.5f && UnityEngine.Random.value < Mathf.Pow(Errors.SmoothValue(timeStacker), 2f))
			{
				fade *= UnityEngine.Random.value;
			}
			if (fade > 0f)
			{
				for (int i = 0; i < verts.Count; i++)
				{
					Vector3 a = Vector3.Lerp(verts[i].lastPos, verts[i].pos, timeStacker);
					a = MultVec(a, new Vector3(fade, 3f - 2f * Mathf.Pow(fade, 0.4f), Mathf.Pow(fade, 0.3f)));
					float num = DirPow(a, upRotat, timeStacker, ref directionsPower);
					a *= Mathf.Lerp(Mathf.InverseLerp(0.4f, 0.6f, num) * num + Mathf.Sin(Mathf.InverseLerp(0.4f, 0.6f, num) * (float)Math.PI), 1f, fade * (1f - errors));
					a = rCam.ApplyDepth(new Vector2(a.x, a.y) + projPos, a.z / 4f);
					verts[i].drawPos = a;
				}
				for (int j = 0; j < holoLines.Count; j++)
				{
					float num2 = (holoLines[j].A.errors + holoLines[j].B.errors) / 2f;
					if (UnityEngine.Random.value > 0.2f * num2 && holoLines[j].A.on && holoLines[j].B.on)
					{
						Vector2 vector = holoLines[j].A.drawPos;
						Vector2 vector2 = holoLines[j].B.drawPos;
						if (shakeErr && UnityEngine.Random.value < Mathf.Pow(num2, 3f))
						{
							if (UnityEngine.Random.value < num2)
							{
								vector = Vector2.Lerp(vector, (UnityEngine.Random.value < 0.5f) ? holoLines[UnityEngine.Random.Range(0, holoLines.Count)].A.drawPos : holoLines[UnityEngine.Random.Range(0, holoLines.Count)].B.drawPos, Mathf.Pow(UnityEngine.Random.value * errors, 4f));
							}
							if (UnityEngine.Random.value < num2)
							{
								vector2 = Vector2.Lerp(vector2, (UnityEngine.Random.value < 0.5f) ? holoLines[UnityEngine.Random.Range(0, holoLines.Count)].A.drawPos : holoLines[UnityEngine.Random.Range(0, holoLines.Count)].B.drawPos, Mathf.Pow(UnityEngine.Random.value * errors, 4f));
							}
						}
						if (UnityEngine.Random.value < num2 * (0.5f + 0.5f * fade))
						{
							if (UnityEngine.Random.value < 0.5f)
							{
								vector = Vector2.Lerp(Vector2.Lerp(vector, vector2, UnityEngine.Random.value), projPos, UnityEngine.Random.value * Mathf.Max(num2, 1f - fade));
							}
							else
							{
								vector2 = Vector2.Lerp(Vector2.Lerp(vector, vector2, UnityEngine.Random.value), projPos, UnityEngine.Random.value * Mathf.Max(num2, 1f - fade));
							}
						}
						sLeaser.sprites[sprite].x = vector.x - camPos.x;
						sLeaser.sprites[sprite].y = vector.y - camPos.y;
						sLeaser.sprites[sprite].scaleY = Vector2.Distance(vector, vector2);
						sLeaser.sprites[sprite].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
						sLeaser.sprites[sprite].isVisible = true;
						pointsVec += (vector + vector2) * fade;
						pointsWeight += 2f * fade;
						if (!Custom.DistLess(projPos, vector, maxDist))
						{
							maxDist = Vector2.Distance(projPos, vector);
						}
						if (!Custom.DistLess(projPos, vector2, maxDist))
						{
							maxDist = Vector2.Distance(projPos, vector2);
						}
						sLeaser.sprites[sprite].alpha = (0.9f + 0.1f * UnityEngine.Random.value) * Mathf.Pow(fade, 0.2f);
					}
					else
					{
						sLeaser.sprites[sprite].isVisible = false;
					}
					sprite++;
				}
			}
			else
			{
				for (int k = 0; k < holoLines.Count; k++)
				{
					sLeaser.sprites[sprite].isVisible = false;
					sprite++;
				}
			}
			for (int l = 0; l < subShapes.Count; l++)
			{
				subShapes[l].Draw(sLeaser, rCam, timeStacker, projPos, camPos, ref sprite, upRotat, errors, fade, shakeErr, ref pointsVec, ref pointsWeight, ref maxDist, ref directionsPower);
			}
		}

		private Vector3 Rotate(Vector3 position, float urX, float urY, float urZ)
		{
			float x = position.x * Mathf.Cos(urZ) - position.y * Mathf.Sin(urZ);
			position.y = position.x * Mathf.Sin(urZ) + position.y * Mathf.Cos(urZ);
			position.x = x;
			float y = position.y * Mathf.Cos(urX) - position.z * Mathf.Sin(urX);
			position.z = position.y * Mathf.Sin(urX) + position.z * Mathf.Cos(urX);
			position.y = y;
			float z = position.z * Mathf.Cos(urY) - position.x * Mathf.Sin(urY);
			position.x = position.z * Mathf.Sin(urY) + position.x * Mathf.Cos(urY);
			position.z = z;
			return position;
		}
	}

	public Vector2 direction;

	public Vector2 lastDirection;

	public Vector2 lazyDirection;

	public Vector2 lastLazyDirection;

	public float rotation;

	public float lastRotation;

	public float revolveSpeed;

	public float flying;

	public int flyingCounter;

	private int hoverSinCounter;

	private int hologramCounter;

	public Shape holoShape;

	public float holoFade;

	public float lastHoloFade;

	public float holoErrors;

	public float lastHoloErrors;

	public bool storyFly;

	public Vector2 storyFlyTarget;

	public LightSource lightsource;

	public float[,] directionsPower;

	public Vector2? lastOutsideTerrainPos;

	private QuickPathFinder quickPather;

	private QuickPath path;

	private float roomDarkness;

	public bool AnyFly
	{
		get
		{
			if (flyingCounter <= 40)
			{
				return flying > 0f;
			}
			return true;
		}
	}

	public Color myColor => new Color(0f, 1f, 0.3f);

	public NSHSwarmer(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		collisionLayer = 1;
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), 3f, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		base.waterFriction = 0.94f;
		base.buoyancy = 1.1f;
		holoShape = new Shape(null, Shape.ShapeType.Main, new Vector3(0f, 0f, 0f), 0f, 0f);
		directionsPower = new float[12, 3];
		rotation = 0.25f;
		lastRotation = rotation;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		lastOutsideTerrainPos = null;
	}

	public override void Update(bool eu)
	{
		base.firstChunk.vel.y += room.gravity * flying;
		lastHoloFade = holoFade;
		lastHoloErrors = holoErrors;
		lastDirection = direction;
		lastLazyDirection = lazyDirection;
		lastRotation = rotation;
		rotation += revolveSpeed;
		if (room.GetTile(base.firstChunk.pos).Solid)
		{
			if (grabbedBy.Count == 0 && base.firstChunk.collideWithTerrain && room.GetTile(base.firstChunk.lastPos).Solid && room.GetTile(base.firstChunk.lastLastPos).Solid && lastOutsideTerrainPos.HasValue)
			{
				Custom.LogWarning("Resetting NSHswarmer to outside terrain");
				for (int i = 0; i < base.bodyChunks.Length; i++)
				{
					base.bodyChunks[i].HardSetPosition(lastOutsideTerrainPos.Value + Custom.RNV() * UnityEngine.Random.value);
					base.bodyChunks[i].vel /= 2f;
				}
			}
		}
		else
		{
			lastOutsideTerrainPos = base.firstChunk.pos;
		}
		for (int j = 0; j < directionsPower.GetLength(0); j++)
		{
			directionsPower[j, 1] = directionsPower[j, 0];
			directionsPower[j, 0] = Custom.LerpAndTick(directionsPower[j, 0], directionsPower[j, 2], 0.03f, 1f / 15f);
			directionsPower[j, 2] = 0f;
		}
		lazyDirection = Vector3.Slerp(lazyDirection, direction, 0.06f);
		base.GoThroughFloors = storyFly;
		bool increaseFly = flyingCounter > 40 || storyFly;
		bool wantToShowHologram = false;
		if (grabbedBy.Count > 0)
		{
			flyingCounter = 0;
			if (grabbedBy[0].grabber.bodyChunks.Length > 1)
			{
				direction = Custom.DirVec(grabbedBy[0].grabber.bodyChunks[1].pos, grabbedBy[0].grabber.bodyChunks[0].pos);
				direction += Custom.DirVec(grabbedBy[0].grabber.bodyChunks[0].pos, base.firstChunk.pos) * 0.5f;
				direction.Normalize();
			}
			else
			{
				direction = Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos);
			}
			revolveSpeed *= 0.8f;
		}
		else if (AnyFly)
		{
			if (storyFly)
			{
				StoryMovement(ref increaseFly, ref wantToShowHologram);
			}
			else
			{
				HoverBehavior(ref increaseFly, ref wantToShowHologram);
			}
		}
		else if (base.firstChunk.submersion > 0f)
		{
			direction = Vector3.Slerp(direction, new Vector2(0f, 1f), 0.2f * base.firstChunk.submersion);
			revolveSpeed *= 0.8f;
			if (HoverTile(room.GetTilePosition(base.firstChunk.pos)))
			{
				flyingCounter++;
			}
		}
		else if (!Custom.DistLess(base.firstChunk.pos, base.firstChunk.lastPos, 1f))
		{
			direction = Vector3.Slerp(direction, (base.firstChunk.pos - base.firstChunk.lastPos).normalized, 0.4f);
			revolveSpeed = Custom.LerpAndTick(revolveSpeed, Mathf.Sign(revolveSpeed) / 95f, 0.02f, 1f / 60f);
			flyingCounter = 0;
		}
		else if (base.firstChunk.ContactPoint.y < 0)
		{
			direction = Vector3.Slerp(direction, new Vector2(Mathf.Sign(direction.x), 0f), 0.4f);
			revolveSpeed *= 0.8f;
			if (HoverTile(room.GetTilePosition(base.firstChunk.pos)))
			{
				flyingCounter++;
			}
			else
			{
				flyingCounter = 0;
			}
		}
		else
		{
			revolveSpeed = Custom.LerpAndTick(revolveSpeed, Mathf.Sign(revolveSpeed) / 95f, 0.02f, 1f / 60f);
		}
		flying = Custom.LerpAndTick(flying, increaseFly ? 1f : 0f, 0.02f, 0.025f);
		if (!Custom.DistLess(base.firstChunk.lastPos, base.firstChunk.pos, 1.2f))
		{
			holoErrors = Mathf.Min(1f, holoErrors + UnityEngine.Random.value / Custom.LerpMap(Vector2.Distance(base.firstChunk.lastPos, base.firstChunk.pos), 1.2f, 7f, 30f, 8f));
		}
		else if (UnityEngine.Random.value < 0.05f)
		{
			holoErrors = Mathf.Max(0f, holoErrors - UnityEngine.Random.value / 7f);
		}
		if (UnityEngine.Random.value < 1f / Mathf.Lerp(600f, 10f, holoErrors))
		{
			holoErrors = UnityEngine.Random.value;
		}
		if (wantToShowHologram)
		{
			hologramCounter++;
		}
		else
		{
			hologramCounter--;
		}
		hologramCounter = Custom.IntClamp(hologramCounter, 0, 160);
		if (hologramCounter > 60 && grabbedBy.Count == 0)
		{
			holoFade = Custom.LerpAndTick(holoFade, Mathf.InverseLerp(60f, 160f, hologramCounter), 0.06f, 1f / 60f);
		}
		else
		{
			holoFade = Mathf.Max(0f, holoFade - 1f / 12f);
		}
		if (holoFade > 0f || lastHoloFade > 0f)
		{
			holoShape.Update(hologramCounter < 100 || UnityEngine.Random.value < 0.05f, holoErrors, holoFade, base.firstChunk.pos - base.firstChunk.lastPos, Custom.VecToDeg(lazyDirection), ref directionsPower);
		}
		else
		{
			holoShape.ResetUpdate(base.firstChunk.pos);
		}
		if (lightsource != null)
		{
			lightsource.setPos = base.firstChunk.pos;
			if (roomDarkness < 0.2f || lightsource.room != room)
			{
				room.RemoveObject(lightsource);
				lightsource = null;
			}
			else if (lightsource.slatedForDeletetion)
			{
				lightsource = null;
			}
		}
		else if (roomDarkness >= 0.2f)
		{
			lightsource = new LightSource(base.firstChunk.pos, environmentalLight: false, myColor, this);
			room.AddObject(lightsource);
		}
		base.Update(eu);
	}

	private void HoverBehavior(ref bool increaseFly, ref bool wantToShowHologram)
	{
		if (!room.readyForAI)
		{
			increaseFly = false;
			wantToShowHologram = false;
			return;
		}
		hoverSinCounter++;
		direction = Vector3.Slerp(direction, Vector3.Slerp((base.firstChunk.pos - base.firstChunk.lastPos).normalized, new Vector2(0f, 1f), Mathf.Max(0.5f * flying, 1f - Mathf.Abs(base.firstChunk.vel.x))), 0.2f);
		revolveSpeed = Custom.LerpAndTick(revolveSpeed, Mathf.Sign(revolveSpeed) / 95f, 0.02f, 1f / 60f);
		base.firstChunk.vel *= 1f - 0.05f * flying;
		if (!room.GetTile(base.firstChunk.pos + new Vector2(0f, -10f)).Solid && base.firstChunk.ContactPoint.y < 0)
		{
			base.firstChunk.vel.x += Mathf.Sign(base.firstChunk.pos.x - room.MiddleOfTile(base.firstChunk.pos).x);
		}
		if (HoverTile(room.GetTilePosition(base.firstChunk.pos)))
		{
			int num = 8;
			for (int i = -1; i <= 1; i++)
			{
				int floorAltitude = room.aimap.getAItile(base.firstChunk.pos + new Vector2(20f * (float)i, 0f)).floorAltitude;
				if (floorAltitude < 8 && floorAltitude > 0 && (floorAltitude > num || num == 8))
				{
					num = floorAltitude;
				}
			}
			num = Math.Min(num, room.water ? (room.GetTilePosition(base.firstChunk.pos).y - (room.defaultWaterLevel + 2)) : 8);
			if (num < 8)
			{
				float num2 = room.MiddleOfTile(base.firstChunk.pos + new Vector2(0f, 20f * (float)(2 - num))).y + 5f + Mathf.Sin((float)hoverSinCounter / 20f) * 6f;
				if (room.GetTile(new Vector2(base.firstChunk.pos.x, base.firstChunk.pos.y + 20f)).Solid)
				{
					num2 = room.MiddleOfTile(base.firstChunk.pos).y - 4f + Mathf.Sin((float)hoverSinCounter / 20f) * 4f;
				}
				else if (room.aimap.getAItile(base.firstChunk.pos).narrowSpace)
				{
					num2 = base.firstChunk.pos.y + 100f;
				}
				base.firstChunk.vel.y += Custom.LerpMap(base.firstChunk.pos.y, num2 - 30f, num2 + 10f, 0.3f, -0.1f) * flying;
				if (Mathf.Abs(num2 - base.firstChunk.pos.y) < 12f && room.aimap.getTerrainProximity(base.firstChunk.pos) > 1)
				{
					wantToShowHologram = true;
				}
			}
			else
			{
				increaseFly = false;
			}
			if (increaseFly && room.GetTile(base.firstChunk.pos + new Vector2(-20f, 0f)).Solid != room.GetTile(base.firstChunk.pos + new Vector2(20f, 0f)).Solid)
			{
				base.firstChunk.vel.x += (room.GetTile(base.firstChunk.pos + new Vector2(20f, 0f)).Solid ? (-0.1f) : 0.1f) * flying;
				wantToShowHologram = false;
			}
		}
		else
		{
			increaseFly = false;
		}
	}

	private void StoryMovement(ref bool increaseFly, ref bool wantToShowHologram)
	{
		increaseFly = true;
		wantToShowHologram = false;
		Vector2 vector = Custom.DirVec(base.firstChunk.pos, storyFlyTarget);
		if (room.readyForAI && Custom.DistLess(base.firstChunk.pos, storyFlyTarget, 2000f) && !room.VisualContact(base.firstChunk.pos, storyFlyTarget))
		{
			if (path != null)
			{
				bool flag = false;
				IntVector2 pos = new IntVector2(-1, -1);
				for (int num = path.tiles.Length - 1; num >= 0; num--)
				{
					if (pos.x == -1 && pos.y == -1 && room.VisualContact(base.firstChunk.pos, room.MiddleOfTile(path.tiles[num])))
					{
						pos = path.tiles[num];
					}
					if (!flag && room.VisualContact(storyFlyTarget, room.MiddleOfTile(path.tiles[num])))
					{
						flag = true;
					}
					if ((pos.x != -1 || pos.y != -1) && flag)
					{
						break;
					}
				}
				if (!flag || (pos.x == -1 && pos.y == -1))
				{
					path = null;
				}
				else
				{
					vector = Custom.DirVec(base.firstChunk.pos, room.MiddleOfTile(pos));
				}
			}
			else
			{
				if (quickPather == null)
				{
					quickPather = new QuickPathFinder(room.GetTilePosition(base.firstChunk.pos), room.GetTilePosition(storyFlyTarget), room.aimap, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
				}
				for (int i = 0; i < 100; i++)
				{
					quickPather.Update();
					if (quickPather.status != 0)
					{
						path = quickPather.ReturnPath();
						quickPather = null;
						break;
					}
				}
			}
		}
		else
		{
			path = null;
			quickPather = null;
		}
		bool solid = room.GetTile(base.firstChunk.pos + (base.firstChunk.pos - base.firstChunk.lastPos) * 7f + direction * 30f).Solid;
		if (solid)
		{
			base.firstChunk.vel *= 0.7f;
			if (room.readyForAI)
			{
				IntVector2 tilePosition = room.GetTilePosition(base.firstChunk.pos);
				for (int j = 0; j < 8; j++)
				{
					if (room.aimap.getTerrainProximity(tilePosition + Custom.eightDirections[j]) > room.aimap.getTerrainProximity(tilePosition))
					{
						vector += 0.2f * Custom.eightDirections[j].ToVector2();
					}
				}
				vector.Normalize();
			}
		}
		else if (base.firstChunk.lastPos != base.firstChunk.pos)
		{
			base.firstChunk.vel *= Custom.LerpMap(Vector2.Dot((base.firstChunk.pos - base.firstChunk.lastPos).normalized, vector), -1f, 1f, 0.85f, 0.97f);
		}
		direction = Vector3.Slerp(direction, vector, solid ? 1f : Custom.LerpMap(Vector3.Distance(base.firstChunk.pos, storyFlyTarget), 20f, 200f, 1f, 0.3f));
		base.firstChunk.vel += direction;
	}

	private bool HoverTile(IntVector2 tile)
	{
		if (!room.readyForAI)
		{
			return false;
		}
		if (room.aimap.getAItile(tile).floorAltitude > 8 && tile.y - room.defaultWaterLevel > 8)
		{
			return false;
		}
		if (room.aimap.getAItile(tile).narrowSpace)
		{
			for (int i = 1; i < 4; i++)
			{
				if (room.GetTile(tile.x, tile.y + i).Solid)
				{
					return false;
				}
				if (!room.aimap.getAItile(tile.x, tile.y + i).narrowSpace)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		holoErrors = Mathf.Min(1f, holoErrors + UnityEngine.Random.value / 7f);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[6 + holoShape.LinesCount];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		sLeaser.sprites[0].scale = 1.5f;
		sLeaser.sprites[0].alpha = 0.2f;
		sLeaser.sprites[1] = new FSprite("JetFishEyeA");
		sLeaser.sprites[1].scaleY = 1.2f;
		sLeaser.sprites[1].scaleX = 0.75f;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[2 + i] = new FSprite("deerEyeA2");
			sLeaser.sprites[2 + i].anchorX = 0f;
		}
		sLeaser.sprites[4] = new FSprite("JetFishEyeB");
		sLeaser.sprites[5] = new FSprite("Futile_White");
		sLeaser.sprites[5].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		for (int num = holoShape.LinesCount - 1; num >= 0; num--)
		{
			sLeaser.sprites[6 + num] = new FSprite("pixel");
			sLeaser.sprites[6 + num].anchorY = 0f;
			sLeaser.sprites[6 + num].shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];
		}
		for (int j = 0; j < sLeaser.sprites.Length; j++)
		{
			sLeaser.sprites[j].color = myColor;
		}
		sLeaser.sprites[4].color = Color.Lerp(myColor, new Color(1f, 1f, 1f), 0.5f);
		AddToContainer(sLeaser, rCam, null);
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastDirection, direction, timeStacker);
		Vector2 vector3 = Vector3.Slerp(lastLazyDirection, lazyDirection, timeStacker);
		Vector3 vector4 = Custom.PerpendicularVector(vector2);
		float num = Mathf.Sin(Mathf.Lerp(lastRotation, rotation, timeStacker) * (float)Math.PI * 2f);
		float num2 = Mathf.Cos(Mathf.Lerp(lastRotation, rotation, timeStacker) * (float)Math.PI * 2f);
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[1].x = vector.x - camPos.x;
		sLeaser.sprites[1].y = vector.y - camPos.y;
		sLeaser.sprites[4].x = vector.x + vector4.x * 2f * num2 * Mathf.Sign(num) - camPos.x;
		sLeaser.sprites[4].y = vector.y + vector4.y * 2f * num2 * Mathf.Sign(num) - camPos.y;
		sLeaser.sprites[1].rotation = Custom.VecToDeg(vector2);
		sLeaser.sprites[4].rotation = Custom.VecToDeg(vector2);
		sLeaser.sprites[4].scaleX = 1f - Mathf.Abs(num2);
		sLeaser.sprites[1].isVisible = true;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[2 + i].x = vector.x - vector2.x * 4f - camPos.x;
			sLeaser.sprites[2 + i].y = vector.y - vector2.y * 4f - camPos.y;
			sLeaser.sprites[2 + i].rotation = Custom.VecToDeg(vector3) + 90f + ((i == 0) ? (-1f) : 1f) * Custom.LerpMap(Vector2.Distance(vector2, vector3), 0.06f, 0.7f, 10f, 45f, 2f) * num;
		}
		sLeaser.sprites[2].scaleY = -1f * num;
		sLeaser.sprites[3].scaleY = num;
		float pointsWeight = 1f;
		Vector2 pointsVec = vector;
		float maxDist = 1f;
		float num3 = Custom.SCurve(Mathf.Lerp(lastHoloFade, holoFade, timeStacker), 0.65f);
		num3 *= holoShape.Fade.SmoothValue(timeStacker);
		if (num3 > 0f)
		{
			int sprite = 6;
			holoShape.Draw(sLeaser, rCam, timeStacker, vector, camPos, ref sprite, Custom.VecToDeg(Vector3.Slerp(vector3, new Vector2(0f, 1f), 0.51f)), Mathf.Lerp(lastHoloErrors, holoErrors, timeStacker), num3, shakeErr: false, ref pointsVec, ref pointsWeight, ref maxDist, ref directionsPower);
			pointsVec /= pointsWeight;
			sLeaser.sprites[5].isVisible = true;
			sLeaser.sprites[5].x = pointsVec.x - camPos.x;
			sLeaser.sprites[5].y = pointsVec.y - camPos.y;
			sLeaser.sprites[5].alpha = Custom.SCurve(Mathf.Pow(Mathf.InverseLerp(15f, 500f, pointsWeight), 0.5f), 0.8f) * Mathf.Pow(num3, 0.4f) * 0.4f;
			sLeaser.sprites[5].scaleX = (40f + Mathf.Lerp(Custom.SCurve(Mathf.InverseLerp(5f, 300f, pointsWeight), 0.8f) * 120f, maxDist, 0.5f)) * Mathf.Pow(num3, 1.4f) / 8f;
			sLeaser.sprites[5].scaleY = (40f + Mathf.Lerp(Custom.SCurve(Mathf.InverseLerp(5f, 300f, pointsWeight), 0.8f) * 120f, maxDist, 0.5f)) * (0.5f + 0.5f * Mathf.Pow(num3, 0.4f)) / 8f;
			sLeaser.sprites[5].rotation = Custom.VecToDeg(lazyDirection);
		}
		else
		{
			sLeaser.sprites[5].isVisible = false;
			for (int j = 6; j < sLeaser.sprites.Length; j++)
			{
				sLeaser.sprites[j].isVisible = false;
			}
		}
		if (lightsource != null)
		{
			lightsource.HardSetAlpha((0.3f + 0.7f * Custom.SCurve(Mathf.Pow(Mathf.InverseLerp(15f, 400f, pointsWeight), 0.5f), 0.8f) * Mathf.Pow(num3, 0.4f)) * Custom.LerpMap(roomDarkness, 0.2f, 0.7f, 0f, 0.5f));
			lightsource.HardSetPos(pointsVec);
			lightsource.HardSetRad(Custom.LerpMap(pointsWeight * num3, 2f, 15f, 65f, 160f) + Mathf.Lerp(Custom.SCurve(Mathf.InverseLerp(5f, 300f, pointsWeight), 0.8f) * 120f, maxDist, 0.5f) * (0.5f + 0.5f * Mathf.Pow(num3, 0.4f)) * 2f);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		roomDarkness = palette.darkness;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		FContainer fContainer = rCam.ReturnFContainer("Water");
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			if (i == 0 || i > 4)
			{
				fContainer.AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}
}
