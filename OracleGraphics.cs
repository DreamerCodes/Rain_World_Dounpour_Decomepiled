using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class OracleGraphics : GraphicsModule
{
	public class ArmJointGraphics
	{
		private OracleGraphics owner;

		public Oracle.OracleArm.Joint myJoint;

		public int firstSprite;

		public int totalSprites;

		public int meshSegs;

		private float smallArmLength;

		private float cogRotat;

		private float lastCogRotat;

		private float lastAC;

		public Color metalColor;

		public StaticSoundLoop armJointSound;

		public int CogSprite(int cog)
		{
			return firstSprite + cog;
		}

		public int MetalPartSprite(int part)
		{
			return firstSprite + 3 + part;
		}

		public int SegmentSprite(int segment, int highLight)
		{
			return firstSprite + 3 + 6 + segment * 2 + highLight;
		}

		public int JointSprite(int joint, int part)
		{
			if (joint == 0)
			{
				return firstSprite + 3 + 10 + part;
			}
			return firstSprite + 3 + 4 + part;
		}

		public ArmJointGraphics(OracleGraphics owner, Oracle.OracleArm.Joint myJoint, int firstSprite)
		{
			this.owner = owner;
			this.myJoint = myJoint;
			this.firstSprite = firstSprite;
			totalSprites = 15;
			meshSegs = (int)(myJoint.totalLength / 10f);
			smallArmLength = myJoint.totalLength / 4f;
			cogRotat = UnityEngine.Random.value;
			lastCogRotat = cogRotat;
			armJointSound = new StaticSoundLoop((owner.oracle.ID == Oracle.OracleID.SS) ? SoundID.SS_AI_Arm_Joint_LOOP : SoundID.SL_AI_Arm_Joint_LOOP, myJoint.pos, myJoint.arm.oracle.room, 1f, Custom.LerpMap(myJoint.index, 0f, 3f, 0.5f, 1.5f));
		}

		public void Update()
		{
			float num = Vector2.Distance(b: (myJoint.next == null) ? owner.oracle.bodyChunks[1].pos : myJoint.next.pos, a: myJoint.pos);
			float num2 = (lastAC - num) / myJoint.totalLength;
			lastCogRotat = cogRotat;
			cogRotat += num2 * ((myJoint.index % 2 == 0) ? (-1f) : 1f) * 2f;
			lastAC = num;
			armJointSound.Update();
			armJointSound.volume = Mathf.InverseLerp(0.0001f, 0.0003f, Mathf.Abs(num2)) * Custom.LerpMap(myJoint.index, 0f, 3f, 1f, 0.15f, 0.5f) * (ModManager.MSC ? (1f - myJoint.arm.oracle.noiseSuppress) : 1f);
			armJointSound.pitch = Mathf.Clamp(1f - num2 * 100f, 0.5f, 1.5f) * Custom.LerpMap(myJoint.index, 0f, 3f, 0.5f, 1.5f);
			armJointSound.pos = myJoint.pos;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[MetalPartSprite(0)] = new FSprite("MirosLegSmallPart");
			sLeaser.sprites[MetalPartSprite(0)].anchorY = 1f;
			sLeaser.sprites[MetalPartSprite(1)] = new FSprite("pixel");
			sLeaser.sprites[MetalPartSprite(2)] = new FSprite("pixel");
			sLeaser.sprites[MetalPartSprite(3)] = new FSprite("deerEyeB");
			sLeaser.sprites[MetalPartSprite(0)].scaleX = ((myJoint.index == 0) ? 1.5f : ((myJoint.index == 1) ? 1f : 0.8f)) * ((myJoint.index % 2 == 0) ? (-1f) : 1f);
			sLeaser.sprites[MetalPartSprite(0)].scaleY = ((myJoint.index == 0) ? 2f : ((myJoint.index == 1) ? 1f : 0.5f));
			sLeaser.sprites[MetalPartSprite(1)].scaleX = ((myJoint.index == 0) ? 2f : (((double)myJoint.index == 1.5) ? 1f : 1f));
			sLeaser.sprites[MetalPartSprite(2)].scaleX = ((myJoint.index == 0) ? 2f : (((double)myJoint.index == 1.5) ? 1f : 1f));
			sLeaser.sprites[MetalPartSprite(2)].anchorY = 0f;
			sLeaser.sprites[MetalPartSprite(1)].anchorY = 0f;
			sLeaser.sprites[MetalPartSprite(1)].scaleY = smallArmLength;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					sLeaser.sprites[SegmentSprite(i, j)] = TriangleMesh.MakeLongMesh(meshSegs, pointyTip: false, owner.oracle.ID == Oracle.OracleID.SL);
					sLeaser.sprites[JointSprite(i, 0)] = new FSprite("Circle20");
					sLeaser.sprites[JointSprite(i, 1)] = new FSprite("deerEyeB");
				}
			}
			sLeaser.sprites[JointSprite(0, 0)].scale = (7f - (float)myJoint.index * 0.5f) / 10f;
			sLeaser.sprites[JointSprite(1, 0)].scale = (6f - (float)myJoint.index * 1.5f) / 10f;
			for (int k = 0; k < 3; k++)
			{
				sLeaser.sprites[CogSprite(k)] = new FSprite("pixel");
				sLeaser.sprites[CogSprite(k)].scaleY = 18f - (float)myJoint.index * 2f;
				sLeaser.sprites[CogSprite(k)].scaleX = 5f - (float)myJoint.index * 0.5f;
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(myJoint.lastPos, myJoint.pos, timeStacker);
			Vector2 vector2 = ((myJoint.next == null) ? Vector2.Lerp(owner.oracle.bodyChunks[1].lastPos, owner.oracle.bodyChunks[1].pos, timeStacker) : Vector2.Lerp(myJoint.next.lastPos, myJoint.next.pos, timeStacker));
			Vector2 vector3 = myJoint.ElbowPos(timeStacker, vector2);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[JointSprite(0, i)].x = vector.x + Custom.DirVec(vector, vector3).x * ((myJoint.index == 0) ? 12f : 2f) - camPos.x;
				sLeaser.sprites[JointSprite(0, i)].y = vector.y + Custom.DirVec(vector, vector3).y * ((myJoint.index == 0) ? 12f : 2f) - camPos.y;
				sLeaser.sprites[JointSprite(1, i)].x = vector3.x - camPos.x;
				sLeaser.sprites[JointSprite(1, i)].y = vector3.y - camPos.y;
			}
			sLeaser.sprites[MetalPartSprite(0)].x = vector3.x - camPos.x;
			sLeaser.sprites[MetalPartSprite(0)].y = vector3.y - camPos.y;
			sLeaser.sprites[MetalPartSprite(1)].x = vector3.x - camPos.x;
			sLeaser.sprites[MetalPartSprite(1)].y = vector3.y - camPos.y;
			sLeaser.sprites[MetalPartSprite(0)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector, vector2, 0.2f), vector3);
			sLeaser.sprites[MetalPartSprite(1)].rotation = Custom.AimFromOneVectorToAnother(vector3, Vector2.Lerp(vector, vector2, 0.8f));
			Vector2 vector4 = vector3 + Custom.DirVec(vector3, Vector2.Lerp(vector, vector2, 0.8f)) * smallArmLength;
			sLeaser.sprites[MetalPartSprite(2)].x = vector2.x - camPos.x;
			sLeaser.sprites[MetalPartSprite(2)].y = vector2.y - camPos.y;
			sLeaser.sprites[MetalPartSprite(2)].rotation = Custom.AimFromOneVectorToAnother(vector2, vector4);
			sLeaser.sprites[MetalPartSprite(2)].scaleY = Vector2.Distance(vector4, vector2) + 10f;
			sLeaser.sprites[MetalPartSprite(3)].x = vector4.x - camPos.x;
			sLeaser.sprites[MetalPartSprite(3)].y = vector4.y - camPos.y;
			float num = Mathf.Lerp(lastCogRotat, cogRotat, timeStacker);
			for (int j = 0; j < 3; j++)
			{
				sLeaser.sprites[CogSprite(j)].x = vector.x + Custom.DirVec(vector, vector3).x * ((myJoint.index == 0) ? 12f : 2f) - camPos.x;
				sLeaser.sprites[CogSprite(j)].y = vector.y + Custom.DirVec(vector, vector3).y * ((myJoint.index == 0) ? 12f : 2f) - camPos.y;
				sLeaser.sprites[CogSprite(j)].rotation = Custom.AimFromOneVectorToAnother(vector, vector3) + 60f * (float)j + num * 360f;
			}
			Vector2 vector5 = Custom.DirVec(vector, vector3);
			Vector2 vector6 = Custom.DirVec(vector3, vector2);
			Vector2 vector7 = Vector3.Slerp(vector5, vector6, 0.5f);
			vector += vector5 * Mathf.Lerp(myJoint.totalLength * 0.05f, 4f, 0.5f);
			vector2 -= vector6 * Mathf.Lerp(myJoint.totalLength * 0.025f, 4f, 0.5f);
			Vector2 cA = vector + vector5 * Vector2.Distance(vector, vector3) * 0.2f;
			Vector2 cB = vector3 - vector7 * Vector2.Distance(vector, vector3) * 0.2f;
			Vector2 cA2 = vector3 + vector7 * Vector2.Distance(vector3, vector2) * 0.2f;
			Vector2 cB2 = vector2 - vector6 * Vector2.Distance(vector3, vector2) * 0.2f;
			for (int k = 0; k < 2; k++)
			{
				Vector2 vector8 = ((k == 0) ? vector : vector3);
				Vector2 v = ((k == 0) ? vector5 : vector7);
				float num2 = 0.5f;
				float num3 = 5f;
				if (myJoint.index == 0)
				{
					num3 = 7f;
				}
				else if (myJoint.index == 2)
				{
					num3 = 4f;
				}
				else if (myJoint.index == 3)
				{
					num3 = 3f;
				}
				for (int l = 1; l <= meshSegs; l++)
				{
					float num4 = (float)l / (float)(meshSegs - 1);
					float num5 = 0.6f + 0.5f * (1f - Mathf.Sin(num4 * (float)Math.PI)) + 0.3f * Mathf.Max(Mathf.Sin(Mathf.InverseLerp(0f, 0.3f, num4) * (float)Math.PI), Mathf.Sin(Mathf.InverseLerp(0.7f, 1f, num4) * (float)Math.PI));
					if (num4 == 1f)
					{
						num5 = 0.5f;
					}
					Vector2 vector9 = ((k == 0) ? Custom.Bezier(vector, cA, vector3, cB, num4) : Custom.Bezier(vector3, cA2, vector2, cB2, num4));
					Vector2 vector10 = ((!(num4 < 1f)) ? ((k == 0) ? vector7 : vector6) : ((k != 0) ? Custom.DirVec(vector9, Custom.Bezier(vector3, cA2, vector2, cB2, (float)(l + 1) / (float)(meshSegs - 1))) : Custom.DirVec(vector9, Custom.Bezier(vector, cA, vector3, cB, (float)(l + 1) / (float)(meshSegs - 1)))));
					float num6 = 0f;
					if ((k == 0 && num4 > 0.75f) || (k == 1 && num4 < 0.25f))
					{
						num5 *= 0.5f;
						num6 = num5 * num3 * ((myJoint.index % 2 == 0) ? 1f : (-1f));
					}
					(sLeaser.sprites[SegmentSprite(k, 0)] as TriangleMesh).MoveVertice(l * 4 - 4, (vector8 + vector9) / 2f + Custom.PerpendicularVector(v) * (num6 + num3 * (num5 + num2) * 0.5f) - camPos);
					(sLeaser.sprites[SegmentSprite(k, 0)] as TriangleMesh).MoveVertice(l * 4 - 3, (vector8 + vector9) / 2f + Custom.PerpendicularVector(v) * (num6 - num3 * (num5 + num2) * 0.5f) - camPos);
					(sLeaser.sprites[SegmentSprite(k, 0)] as TriangleMesh).MoveVertice(l * 4 - 2, vector9 + Custom.PerpendicularVector(vector10) * (num6 + num3 * num5) - camPos);
					(sLeaser.sprites[SegmentSprite(k, 0)] as TriangleMesh).MoveVertice(l * 4 - 1, vector9 + Custom.PerpendicularVector(vector10) * (num6 - num3 * num5) - camPos);
					(sLeaser.sprites[SegmentSprite(k, 1)] as TriangleMesh).MoveVertice(l * 4 - 4, (vector8 + vector9) / 2f + Custom.PerpendicularVector(v) * (num6 + num3 * (num5 + num2) * 0.5f * 0.5f) - camPos);
					(sLeaser.sprites[SegmentSprite(k, 1)] as TriangleMesh).MoveVertice(l * 4 - 3, (vector8 + vector9) / 2f + Custom.PerpendicularVector(v) * (num6 - num3 * (num5 + num2) * 0.5f * 0.5f) - camPos);
					(sLeaser.sprites[SegmentSprite(k, 1)] as TriangleMesh).MoveVertice(l * 4 - 2, vector9 + Custom.PerpendicularVector(vector10) * (num6 + num3 * num5 * 0.5f) - camPos);
					(sLeaser.sprites[SegmentSprite(k, 1)] as TriangleMesh).MoveVertice(l * 4 - 1, vector9 + Custom.PerpendicularVector(vector10) * (num6 - num3 * num5 * 0.5f) - camPos);
					vector8 = vector9;
					v = vector10;
					num2 = num5;
				}
				if (owner.oracle.ID == Oracle.OracleID.SL)
				{
					for (int m = 0; m < (sLeaser.sprites[SegmentSprite(k, 0)] as TriangleMesh).verticeColors.Length; m++)
					{
						(sLeaser.sprites[SegmentSprite(k, 0)] as TriangleMesh).verticeColors[m] = BaseColor((sLeaser.sprites[SegmentSprite(k, 0)] as TriangleMesh).vertices[m] + camPos);
						(sLeaser.sprites[SegmentSprite(k, 1)] as TriangleMesh).verticeColors[m] = HighLightColor((sLeaser.sprites[SegmentSprite(k, 1)] as TriangleMesh).vertices[m] + camPos);
					}
				}
			}
		}

		public Color BaseColor(Vector2 ps)
		{
			if (owner.oracle.ID == Oracle.OracleID.SL || (ModManager.MSC && owner.oracle.ID == MoreSlugcatsEnums.OracleID.CL))
			{
				return Color.Lerp(owner.SLArmBaseColA, owner.SLArmBaseColB, Mathf.InverseLerp(50f, 400f, Vector2.Distance(ps, new Vector2(1387f, 188f))));
			}
			return Color.Lerp(Custom.HSL2RGB(0.025f, Mathf.Lerp(0.4f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.05f, 0.7f - 0.5f * owner.oracle.room.Darkness(myJoint.pos), Mathf.Pow(1f, 0.45f))), new Color(0f, 0f, 0.1f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.5f);
		}

		public Color HighLightColor(Vector2 ps)
		{
			if (owner.oracle.ID == Oracle.OracleID.SL || (ModManager.MSC && owner.oracle.ID == MoreSlugcatsEnums.OracleID.CL))
			{
				return Color.Lerp(owner.SLArmHighLightColA, owner.SLArmHighLightColB, Mathf.InverseLerp(50f, 400f, Vector2.Distance(ps, new Vector2(1387f, 188f))));
			}
			return Color.Lerp(Custom.HSL2RGB(0.025f, Mathf.Lerp(0.5f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.15f, 0.85f - 0.65f * owner.oracle.room.Darkness(myJoint.pos), Mathf.Pow(1f, 0.45f))), new Color(0f, 0f, 0.15f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.4f);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			if (owner.oracle.ID == Oracle.OracleID.SL)
			{
				metalColor = palette.blackColor;
			}
			else
			{
				metalColor = Color.Lerp(palette.blackColor, palette.texture.GetPixel(5, 5), 0.12f);
			}
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[SegmentSprite(i, 0)].color = BaseColor(default(Vector2));
				sLeaser.sprites[SegmentSprite(i, 1)].color = HighLightColor(default(Vector2));
				sLeaser.sprites[JointSprite(i, 0)].color = metalColor;
				sLeaser.sprites[JointSprite(i, 1)].color = Color.Lerp(metalColor, HighLightColor(default(Vector2)), 0.5f);
			}
			for (int j = 0; j < 3; j++)
			{
				sLeaser.sprites[CogSprite(j)].color = metalColor;
			}
			for (int k = 0; k < 4; k++)
			{
				sLeaser.sprites[MetalPartSprite(k)].color = metalColor;
			}
		}
	}

	public class Gown
	{
		private OracleGraphics owner;

		private int divs = 11;

		public Vector2[,,] clothPoints;

		public Gown(OracleGraphics owner)
		{
			this.owner = owner;
			clothPoints = new Vector2[divs, divs, 3];
		}

		public void Update()
		{
			Vector2 pos = owner.oracle.firstChunk.pos;
			Vector2 vector = Custom.DirVec(owner.oracle.bodyChunks[1].pos, owner.oracle.firstChunk.pos);
			Vector2 perp = Custom.PerpendicularVector(vector);
			for (int i = 0; i < divs; i++)
			{
				for (int j = 0; j < divs; j++)
				{
					float t = Mathf.InverseLerp(0f, divs - 1, j);
					clothPoints[i, j, 1] = clothPoints[i, j, 0];
					clothPoints[i, j, 0] += clothPoints[i, j, 2];
					clothPoints[i, j, 2] *= 0.999f;
					clothPoints[i, j, 2].y -= 0.9f * owner.owner.room.gravity;
					Vector2 vector2 = IdealPosForPoint(i, j, pos, vector, perp);
					clothPoints[i, j, 2] += (Vector2)Vector3.Slerp(-vector, Custom.DirVec(owner.oracle.bodyChunks[1].pos, vector2), t) * 0.02f;
					float num = Vector2.Distance(clothPoints[i, j, 0], vector2);
					float num2 = Mathf.Lerp(0f, 9f, t);
					Vector2 vector3 = Custom.DirVec(clothPoints[i, j, 0], vector2);
					if (num > num2)
					{
						clothPoints[i, j, 0] -= (num2 - num) * vector3;
						clothPoints[i, j, 2] -= (num2 - num) * vector3;
					}
					for (int k = 0; k < 4; k++)
					{
						IntVector2 intVector = new IntVector2(i, j) + Custom.fourDirections[k];
						if (intVector.x >= 0 && intVector.y >= 0 && intVector.x < divs && intVector.y < divs)
						{
							num = Vector2.Distance(clothPoints[i, j, 0], clothPoints[intVector.x, intVector.y, 0]);
							vector3 = Custom.DirVec(clothPoints[i, j, 0], clothPoints[intVector.x, intVector.y, 0]);
							float num3 = Vector2.Distance(vector2, IdealPosForPoint(intVector.x, intVector.y, pos, vector, perp));
							clothPoints[i, j, 2] -= (num3 - num) * vector3 * 0.05f;
							clothPoints[intVector.x, intVector.y, 2] += (num3 - num) * vector3 * 0.05f;
						}
					}
				}
			}
		}

		private Vector2 IdealPosForPoint(int x, int y, Vector2 bodyPos, Vector2 dir, Vector2 perp)
		{
			float num = Mathf.InverseLerp(0f, divs - 1, x);
			float t = Mathf.InverseLerp(0f, divs - 1, y);
			return bodyPos + Mathf.Lerp(-1f, 1f, num) * perp * Mathf.Lerp(5f, 11f, t) + dir * Mathf.Lerp(5f, -18f, t) * (1f + Mathf.Sin((float)Math.PI * num) * 0.35f * Mathf.Lerp(-1f, 1f, t));
		}

		public Color Color(float f)
		{
			if (owner.IsPebbles || owner.IsSaintPebbles)
			{
				Color color = Custom.HSL2RGB(Mathf.Lerp(0.08f, 0.02f, Mathf.Pow(f, 2f)), Mathf.Lerp(1f, 0.8f, f), 0.5f);
				if (owner.IsSaintPebbles)
				{
					color = UnityEngine.Color.Lerp(color, UnityEngine.Color.white, 0.3f);
				}
				return color;
			}
			if (owner.IsStraw)
			{
				return Custom.HSL2RGB(Mathf.Lerp(0f, 0.2f, f), Mathf.Lerp(0.5f, 1f, f), Mathf.Lerp(0.4f, 0.75f, f));
			}
			if (owner.IsMoon)
			{
				return Custom.HSL2RGB(Mathf.Lerp(0.38f, 0.32f, Mathf.Pow(f, 2f)), Mathf.Lerp(0f, 0.1f, Mathf.Pow(f, 1.1f)), Mathf.Lerp(0.7f, 0.3f, Mathf.Pow(f, 6f)));
			}
			return Custom.HSL2RGB(Mathf.Lerp(0.08f, 0.02f, Mathf.Pow(f, 2f)), Mathf.Lerp(0.6f, 0.4f, f), 0.4f);
		}

		public void InitiateSprite(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			if (owner.IsMoon || owner.IsSaintPebbles)
			{
				sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh("MoonCloakTex", divs - 1);
			}
			else
			{
				sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh("Futile_White", divs - 1);
			}
			for (int i = 0; i < divs; i++)
			{
				for (int j = 0; j < divs; j++)
				{
					(sLeaser.sprites[sprite] as TriangleMesh).verticeColors[j * divs + i] = Color((float)i / (float)(divs - 1));
				}
			}
		}

		public void DrawSprite(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < divs; i++)
			{
				for (int j = 0; j < divs; j++)
				{
					(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * divs + j, Vector2.Lerp(clothPoints[i, j, 1], clothPoints[i, j, 0], timeStacker) - camPos);
				}
			}
		}
	}

	public class Halo
	{
		public class Connection
		{
			public Halo halo;

			public Vector2 stuckAt;

			public Vector2 handle;

			public float lightUp;

			public float lastLightUp;

			public Connection(Halo halo, Vector2 stuckAt)
			{
				this.halo = halo;
				Vector2 b = stuckAt;
				b.x = Mathf.Clamp(b.x, halo.owner.oracle.arm.cornerPositions[0].x, halo.owner.oracle.arm.cornerPositions[1].x);
				b.y = Mathf.Clamp(b.y, halo.owner.oracle.arm.cornerPositions[2].y, halo.owner.oracle.arm.cornerPositions[1].y);
				this.stuckAt = Vector2.Lerp(stuckAt, b, 0.5f);
				handle = stuckAt + Custom.RNV() * Mathf.Lerp(400f, 700f, UnityEngine.Random.value);
			}
		}

		public class MemoryBit
		{
			public Halo halo;

			public IntVector2 position;

			private float filled;

			private float lastFilled;

			private float getToFilled;

			private float fillSpeed;

			public int blinkCounter;

			public float Fill(float timeStacker)
			{
				if (blinkCounter % 4 > 1 && filled == getToFilled)
				{
					return 0f;
				}
				return Mathf.Lerp(lastFilled, filled, timeStacker);
			}

			public MemoryBit(Halo halo, IntVector2 position)
			{
				this.halo = halo;
				this.position = position;
				filled = UnityEngine.Random.value;
				lastFilled = filled;
				getToFilled = filled;
				fillSpeed = 0f;
			}

			public void SetToMax()
			{
				getToFilled = 1f;
				fillSpeed = Mathf.Lerp(fillSpeed, 0.25f, 0.25f);
				blinkCounter = 20;
			}

			public void Update()
			{
				lastFilled = filled;
				if (filled != getToFilled)
				{
					filled = Custom.LerpAndTick(filled, getToFilled, 0.03f, fillSpeed);
				}
				else if (blinkCounter > 0)
				{
					blinkCounter--;
				}
				else if (UnityEngine.Random.value < 1f / 60f)
				{
					getToFilled = UnityEngine.Random.value;
					fillSpeed = 1f / Mathf.Lerp(2f, 80f, UnityEngine.Random.value);
				}
			}
		}

		private OracleGraphics owner;

		public int firstSprite;

		public int totalSprites;

		private int firstBitSprite;

		public Connection[] connections;

		public float connectionsFireChance;

		public MemoryBit[][] bits;

		public float[,] ringRotations;

		public float expand;

		public float lastExpand;

		public float getToExpand;

		public float push;

		public float lastPush;

		public float getToPush;

		public float white;

		public float lastWhite;

		public float getToWhite;

		public Halo(OracleGraphics owner, int firstSprite)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
			totalSprites = 2;
			connections = new Connection[20];
			totalSprites += connections.Length;
			for (int i = 0; i < connections.Length; i++)
			{
				connections[i] = new Connection(this, new Vector2(owner.owner.room.PixelWidth / 2f, owner.owner.room.PixelHeight / 2f) + Custom.RNV() * Mathf.Lerp(300f, 500f, UnityEngine.Random.value));
			}
			connectionsFireChance = Mathf.Pow(UnityEngine.Random.value, 3f);
			firstBitSprite = firstSprite + totalSprites;
			bits = new MemoryBit[3][];
			bits[0] = new MemoryBit[10];
			bits[1] = new MemoryBit[30];
			bits[2] = new MemoryBit[60];
			for (int j = 0; j < bits.Length; j++)
			{
				for (int k = 0; k < bits[j].Length; k++)
				{
					bits[j][k] = new MemoryBit(this, new IntVector2(j, k));
				}
			}
			totalSprites += 100;
			ringRotations = new float[10, 5];
			expand = 1f;
			getToExpand = 1f;
		}

		public void Update()
		{
			for (int i = 0; i < connections.Length; i++)
			{
				connections[i].lastLightUp = connections[i].lightUp;
				connections[i].lightUp *= 0.9f;
				if (UnityEngine.Random.value < connectionsFireChance / 40f && owner.oracle.Consious)
				{
					connections[i].lightUp = 1f;
					owner.owner.room.PlaySound(SoundID.SS_AI_Halo_Connection_Light_Up, 0f, ModManager.MSC ? (1f * (1f - owner.oracle.noiseSuppress)) : 1f, 1f);
				}
			}
			if (UnityEngine.Random.value < 1f / 60f)
			{
				connectionsFireChance = Mathf.Pow(UnityEngine.Random.value, 3f);
			}
			if (ModManager.MSC && owner.oracle.suppressConnectionFires)
			{
				connectionsFireChance = 0f;
			}
			for (int j = 0; j < ringRotations.GetLength(0); j++)
			{
				ringRotations[j, 1] = ringRotations[j, 0];
				if (ringRotations[j, 0] != ringRotations[j, 3])
				{
					ringRotations[j, 4] += 1f / Mathf.Lerp(20f, Mathf.Abs(ringRotations[j, 2] - ringRotations[j, 3]), 0.5f);
					ringRotations[j, 0] = Mathf.Lerp(ringRotations[j, 2], ringRotations[j, 3], Custom.SCurve(ringRotations[j, 4], 0.5f));
					if (ringRotations[j, 4] > 1f)
					{
						ringRotations[j, 4] = 0f;
						ringRotations[j, 2] = ringRotations[j, 3];
						ringRotations[j, 0] = ringRotations[j, 3];
					}
				}
				else if (UnityEngine.Random.value < 1f / 30f)
				{
					ringRotations[j, 3] = ringRotations[j, 0] + ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * Mathf.Lerp(15f, 150f, UnityEngine.Random.value);
				}
			}
			for (int k = 0; k < bits.Length; k++)
			{
				for (int l = 0; l < bits[k].Length; l++)
				{
					bits[k][l].Update();
				}
			}
			if (UnityEngine.Random.value < 1f / 60f && bits.Length != 0)
			{
				int num = UnityEngine.Random.Range(0, bits.Length);
				for (int m = 0; m < bits[num].Length; m++)
				{
					bits[num][m].SetToMax();
				}
			}
			lastExpand = expand;
			lastPush = push;
			lastWhite = white;
			expand = Custom.LerpAndTick(expand, getToExpand, 0.05f, 0.0125f);
			push = Custom.LerpAndTick(push, getToPush, 0.02f, 0.025f);
			white = Custom.LerpAndTick(white, getToWhite, 0.07f, 1f / 44f);
			bool flag = false;
			if (UnityEngine.Random.value < 1f / 160f)
			{
				if (UnityEngine.Random.value < 0.125f)
				{
					flag = getToWhite < 1f;
					getToWhite = 1f;
				}
				else
				{
					getToWhite = 0f;
				}
			}
			if (UnityEngine.Random.value < 1f / 160f || flag)
			{
				getToExpand = ((UnityEngine.Random.value < 0.5f && !flag) ? 1f : Mathf.Lerp(0.8f, 2f, Mathf.Pow(UnityEngine.Random.value, 1.5f)));
			}
			if (UnityEngine.Random.value < 1f / 160f || flag)
			{
				getToPush = ((UnityEngine.Random.value < 0.5f && !flag) ? 0f : ((float)(-1 + UnityEngine.Random.Range(0, UnityEngine.Random.Range(1, 6)))));
			}
		}

		public void ChangeAllRadi()
		{
			getToExpand = Mathf.Lerp(0.8f, 2f, Mathf.Pow(UnityEngine.Random.value, 1.5f));
			getToPush = -1 + UnityEngine.Random.Range(0, UnityEngine.Random.Range(1, 6));
		}

		private float Radius(float ring, float timeStacker)
		{
			return (3f + ring + Mathf.Lerp(lastPush, push, timeStacker) - 0.5f * owner.averageVoice) * Mathf.Lerp(lastExpand, expand, timeStacker) * 10f;
		}

		private float Rotation(int ring, float timeStacker)
		{
			return Mathf.Lerp(ringRotations[ring, 1], ringRotations[ring, 0], timeStacker);
		}

		public Vector2 Center(float timeStacker)
		{
			Vector2 vector = Vector2.Lerp(owner.head.lastPos, owner.head.pos, timeStacker);
			return vector + Custom.DirVec(Vector2.Lerp(owner.owner.firstChunk.lastPos, owner.owner.firstChunk.pos, timeStacker), vector) * 20f;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[firstSprite + i] = new FSprite("Futile_White");
				sLeaser.sprites[firstSprite + i].shader = rCam.game.rainWorld.Shaders["VectorCircle"];
				sLeaser.sprites[firstSprite + i].color = new Color(0f, 0f, 0f);
			}
			for (int j = 0; j < connections.Length; j++)
			{
				sLeaser.sprites[firstSprite + 2 + j] = TriangleMesh.MakeLongMesh(20, pointyTip: false, customColor: false);
				sLeaser.sprites[firstSprite + 2 + j].color = new Color(0f, 0f, 0f);
			}
			for (int k = 0; k < 100; k++)
			{
				sLeaser.sprites[firstBitSprite + k] = new FSprite("pixel");
				sLeaser.sprites[firstBitSprite + k].scaleX = 4f;
				sLeaser.sprites[firstBitSprite + k].color = new Color(0f, 0f, 0f);
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (sLeaser.sprites[firstSprite].isVisible != owner.oracle.Consious)
			{
				for (int i = 0; i < 2 + connections.Length; i++)
				{
					sLeaser.sprites[firstSprite + i].isVisible = owner.oracle.Consious;
				}
				for (int j = 0; j < 100; j++)
				{
					sLeaser.sprites[firstBitSprite + j].isVisible = owner.oracle.Consious;
				}
			}
			Vector2 vector = Center(timeStacker);
			for (int k = 0; k < 2; k++)
			{
				sLeaser.sprites[firstSprite + k].x = vector.x - camPos.x;
				sLeaser.sprites[firstSprite + k].y = vector.y - camPos.y;
				sLeaser.sprites[firstSprite + k].scale = Radius(k, timeStacker) / 8f;
			}
			sLeaser.sprites[firstSprite].alpha = Mathf.Lerp(3f / Radius(0f, timeStacker), 1f, Mathf.Lerp(lastWhite, white, timeStacker));
			sLeaser.sprites[firstSprite + 1].alpha = 3f / Radius(1f, timeStacker);
			for (int l = 0; l < connections.Length; l++)
			{
				if (connections[l].lastLightUp > 0.05f || connections[l].lightUp > 0.05f)
				{
					Vector2 vector2 = connections[l].stuckAt;
					float num = 2f * Mathf.Lerp(connections[l].lastLightUp, connections[l].lightUp, timeStacker);
					for (int m = 0; m < 20; m++)
					{
						float f = (float)m / 19f;
						Vector2 vector3 = Custom.DirVec(vector, connections[l].stuckAt);
						Vector2 vector4 = Custom.Bezier(connections[l].stuckAt, connections[l].handle, vector + vector3 * Radius(2f, timeStacker), vector + vector3 * 400f, f);
						Vector2 vector5 = Custom.DirVec(vector2, vector4);
						Vector2 vector6 = Custom.PerpendicularVector(vector5);
						float num2 = Vector2.Distance(vector2, vector4);
						(sLeaser.sprites[firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4, vector4 - vector5 * num2 * 0.3f - vector6 * num - camPos);
						(sLeaser.sprites[firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4 + 1, vector4 - vector5 * num2 * 0.3f + vector6 * num - camPos);
						(sLeaser.sprites[firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4 + 2, vector4 - vector6 * num - camPos);
						(sLeaser.sprites[firstSprite + 2 + l] as TriangleMesh).MoveVertice(m * 4 + 3, vector4 + vector6 * num - camPos);
						vector2 = vector4;
					}
				}
			}
			int num3 = firstBitSprite;
			for (int n = 0; n < bits.Length; n++)
			{
				for (int num4 = 0; num4 < bits[n].Length; num4++)
				{
					float num5 = (float)num4 / (float)bits[n].Length * 360f + Rotation(n, timeStacker);
					Vector2 vector7 = vector + Custom.DegToVec(num5) * Radius((float)n + 0.5f, timeStacker);
					sLeaser.sprites[num3].scaleY = 8f * bits[n][num4].Fill(timeStacker);
					sLeaser.sprites[num3].x = vector7.x - camPos.x;
					sLeaser.sprites[num3].y = vector7.y - camPos.y;
					sLeaser.sprites[num3].rotation = num5;
					num3++;
				}
			}
		}
	}

	public class UbilicalCord
	{
		private OracleGraphics owner;

		public int firstSprite;

		public int totalSprites;

		public float[] smallCordsLengths;

		public Vector2[] smallCordsHeadDirs;

		public int[] smallCoordColors;

		public Vector2[,] coord;

		public Vector2[,,] smallCords;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public int SegmentSprite(int seg, int part)
		{
			return firstSprite + 1 + seg * 2 + part;
		}

		public int SmallCordSprite(int c)
		{
			return firstSprite + 1 + coord.GetLength(0) * 2 + c;
		}

		public UbilicalCord(OracleGraphics owner, int firstSprite)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
			totalSprites = 1;
			coord = new Vector2[80, 3];
			for (int i = 0; i < coord.GetLength(0); i++)
			{
				coord[i, 0] = owner.owner.firstChunk.pos;
				coord[i, 1] = coord[i, 0];
			}
			totalSprites += coord.GetLength(0) * 2;
			smallCords = new Vector2[14, 20, 3];
			smallCordsLengths = new float[smallCords.GetLength(0)];
			smallCordsHeadDirs = new Vector2[smallCords.GetLength(0)];
			smallCoordColors = new int[smallCords.GetLength(0)];
			for (int j = 0; j < smallCords.GetLength(0); j++)
			{
				smallCordsLengths[j] = ((UnityEngine.Random.value < 0.5f) ? (50f + UnityEngine.Random.value * 15f) : Mathf.Lerp(50f, 200f, Mathf.Pow(UnityEngine.Random.value, 1.5f)));
				smallCoordColors[j] = UnityEngine.Random.Range(0, 3);
				smallCordsHeadDirs[j] = Custom.RNV() * UnityEngine.Random.value;
				for (int k = 0; k < smallCords.GetLength(1); k++)
				{
					coord[k, 0] = owner.owner.firstChunk.pos;
					coord[k, 1] = coord[k, 0];
				}
			}
			totalSprites += smallCords.GetLength(0);
		}

		public void Update()
		{
			for (int i = 0; i < coord.GetLength(0); i++)
			{
				float value = (float)i / (float)(coord.GetLength(0) - 1);
				coord[i, 1] = coord[i, 0];
				coord[i, 0] += coord[i, 2];
				coord[i, 2] *= 0.995f;
				coord[i, 2].y += Mathf.InverseLerp(0.2f, 0f, value);
				coord[i, 2].y -= owner.owner.room.gravity * 0.9f;
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(coord[i, 0], coord[i, 1], coord[i, 2], 5f, new IntVector2(0, 0), goThroughFloors: true);
				cd = SharedPhysics.VerticalCollision(owner.owner.room, cd);
				cd = SharedPhysics.HorizontalCollision(owner.owner.room, cd);
				cd = SharedPhysics.SlopesVertically(owner.owner.room, cd);
				coord[i, 0] = cd.pos;
				coord[i, 2] = cd.vel;
			}
			SetStuckSegments();
			for (int j = 1; j < coord.GetLength(0); j++)
			{
				Vector2 vector = Custom.DirVec(coord[j, 0], coord[j - 1, 0]);
				float num = Vector2.Distance(coord[j, 0], coord[j - 1, 0]);
				coord[j, 0] -= (10f - num) * vector * 0.5f;
				coord[j, 2] -= (10f - num) * vector * 0.5f;
				coord[j - 1, 0] += (10f - num) * vector * 0.5f;
				coord[j - 1, 2] += (10f - num) * vector * 0.5f;
			}
			SetStuckSegments();
			for (int k = 0; k < coord.GetLength(0) - 1; k++)
			{
				Vector2 vector2 = Custom.DirVec(coord[k, 0], coord[k + 1, 0]);
				float num2 = Vector2.Distance(coord[k, 0], coord[k + 1, 0]);
				coord[k, 0] -= (10f - num2) * vector2 * 0.5f;
				coord[k, 2] -= (10f - num2) * vector2 * 0.5f;
				coord[k + 1, 0] += (10f - num2) * vector2 * 0.5f;
				coord[k + 1, 2] += (10f - num2) * vector2 * 0.5f;
			}
			SetStuckSegments();
			float num3 = 0.5f;
			for (int l = 2; l < 4; l++)
			{
				for (int m = l; m < coord.GetLength(0) - l; m++)
				{
					coord[m, 2] += Custom.DirVec(coord[m - l, 0], coord[m, 0]) * num3;
					coord[m - l, 2] -= Custom.DirVec(coord[m - l, 0], coord[m, 0]) * num3;
					coord[m, 2] += Custom.DirVec(coord[m + l, 0], coord[m, 0]) * num3;
					coord[m + l, 2] -= Custom.DirVec(coord[m + l, 0], coord[m, 0]) * num3;
				}
				num3 *= 0.75f;
			}
			if (!Custom.DistLess(coord[coord.GetLength(0) - 1, 0], owner.owner.firstChunk.pos, 80f))
			{
				Vector2 vector3 = Custom.DirVec(coord[coord.GetLength(0) - 1, 0], owner.owner.firstChunk.pos);
				float num4 = Vector2.Distance(coord[coord.GetLength(0) - 1, 0], owner.owner.firstChunk.pos);
				coord[coord.GetLength(0) - 1, 0] -= (80f - num4) * vector3 * 0.25f;
				coord[coord.GetLength(0) - 1, 2] -= (80f - num4) * vector3 * 0.5f;
			}
			for (int n = 0; n < smallCords.GetLength(0); n++)
			{
				for (int num5 = 0; num5 < smallCords.GetLength(1); num5++)
				{
					smallCords[n, num5, 1] = smallCords[n, num5, 0];
					smallCords[n, num5, 0] += smallCords[n, num5, 2];
					smallCords[n, num5, 2] *= Custom.LerpMap(smallCords[n, num5, 2].magnitude, 2f, 6f, 0.999f, 0.9f);
					smallCords[n, num5, 2].y -= owner.owner.room.gravity * 0.9f;
				}
				float num6 = smallCordsLengths[n] / (float)smallCords.GetLength(1);
				for (int num7 = 1; num7 < smallCords.GetLength(1); num7++)
				{
					Vector2 vector4 = Custom.DirVec(smallCords[n, num7, 0], smallCords[n, num7 - 1, 0]);
					float num8 = Vector2.Distance(smallCords[n, num7, 0], smallCords[n, num7 - 1, 0]);
					smallCords[n, num7, 0] -= (num6 - num8) * vector4 * 0.5f;
					smallCords[n, num7, 2] -= (num6 - num8) * vector4 * 0.5f;
					smallCords[n, num7 - 1, 0] += (num6 - num8) * vector4 * 0.5f;
					smallCords[n, num7 - 1, 2] += (num6 - num8) * vector4 * 0.5f;
				}
				for (int num9 = 0; num9 < smallCords.GetLength(1) - 1; num9++)
				{
					Vector2 vector5 = Custom.DirVec(smallCords[n, num9, 0], smallCords[n, num9 + 1, 0]);
					float num10 = Vector2.Distance(smallCords[n, num9, 0], smallCords[n, num9 + 1, 0]);
					smallCords[n, num9, 0] -= (num6 - num10) * vector5 * 0.5f;
					smallCords[n, num9, 2] -= (num6 - num10) * vector5 * 0.5f;
					smallCords[n, num9 + 1, 0] += (num6 - num10) * vector5 * 0.5f;
					smallCords[n, num9 + 1, 2] += (num6 - num10) * vector5 * 0.5f;
				}
				smallCords[n, 0, 0] = coord[coord.GetLength(0) - 1, 0];
				smallCords[n, 0, 2] *= 0f;
				smallCords[n, 1, 2] += Custom.DirVec(coord[coord.GetLength(0) - 2, 0], coord[coord.GetLength(0) - 1, 0]) * 5f;
				smallCords[n, 2, 2] += Custom.DirVec(coord[coord.GetLength(0) - 2, 0], coord[coord.GetLength(0) - 1, 0]) * 3f;
				smallCords[n, 3, 2] += Custom.DirVec(coord[coord.GetLength(0) - 2, 0], coord[coord.GetLength(0) - 1, 0]) * 1.5f;
				smallCords[n, smallCords.GetLength(1) - 1, 0] = owner.head.pos;
				smallCords[n, smallCords.GetLength(1) - 1, 2] *= 0f;
				smallCords[n, smallCords.GetLength(1) - 2, 2] -= (owner.lookDir + smallCordsHeadDirs[n]) * 2f;
				smallCords[n, smallCords.GetLength(1) - 3, 2] -= owner.lookDir + smallCordsHeadDirs[n];
			}
		}

		private void SetStuckSegments()
		{
			if (ModManager.MSC && owner.oracle.room.world.region != null && owner.oracle.room.world.region.name == "RM")
			{
				coord[0, 0] = owner.owner.room.MiddleOfTile(75, 38);
			}
			else if (ModManager.MSC && owner.oracle.room.world.region != null && owner.oracle.room.world.region.name == "CL")
			{
				coord[0, 0] = owner.owner.room.MiddleOfTile(118, 6);
			}
			else
			{
				coord[0, 0] = owner.owner.room.MiddleOfTile(24, 2);
			}
			coord[0, 2] *= 0f;
			Vector2 pos = owner.armJointGraphics[1].myJoint.pos;
			Vector2 vector = owner.armJointGraphics[1].myJoint.ElbowPos(1f, owner.armJointGraphics[2].myJoint.pos);
			for (int i = -1; i < 2; i++)
			{
				float num = ((i == 0) ? 1f : 0.5f);
				coord[coord.GetLength(0) - 20 + i, 0] = Vector2.Lerp(coord[coord.GetLength(0) - 20 + i, 0], Vector2.Lerp(pos, vector, 0.4f + 0.07f * (float)i) + Custom.PerpendicularVector(pos, vector) * 8f, num);
				coord[coord.GetLength(0) - 20 + i, 2] *= 1f - num;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(coord.GetLength(0), pointyTip: false, customColor: false);
			for (int i = 0; i < coord.GetLength(0); i++)
			{
				sLeaser.sprites[SegmentSprite(i, 0)] = new FSprite("CentipedeSegment");
				sLeaser.sprites[SegmentSprite(i, 1)] = new FSprite("CentipedeSegment");
				sLeaser.sprites[SegmentSprite(i, 0)].scaleX = 0.5f;
				sLeaser.sprites[SegmentSprite(i, 0)].scaleY = 0.3f;
				sLeaser.sprites[SegmentSprite(i, 1)].scaleX = 0.4f;
				sLeaser.sprites[SegmentSprite(i, 1)].scaleY = 0.15f;
			}
			for (int j = 0; j < smallCords.GetLength(0); j++)
			{
				sLeaser.sprites[SmallCordSprite(j)] = TriangleMesh.MakeLongMesh(smallCords.GetLength(1), pointyTip: false, customColor: false);
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = coord[0, 0];
			float num = 1.2f;
			for (int i = 0; i < coord.GetLength(0); i++)
			{
				Vector2 vector2 = Vector2.Lerp(coord[i, 1], coord[i, 0], timeStacker);
				Vector2 vector3 = Custom.DirVec(vector, vector2);
				Vector2 vector4 = Custom.PerpendicularVector(vector3);
				float num2 = Vector2.Distance(vector, vector2);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector2 - vector3 * num2 * 0.5f - vector4 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector2 - vector3 * num2 * 0.5f + vector4 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector4 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector4 * num - camPos);
				Vector2 vector5 = vector3;
				if (i < coord.GetLength(0) - 1)
				{
					vector5 = Custom.DirVec(vector2, Vector2.Lerp(coord[i + 1, 1], coord[i + 1, 0], timeStacker));
				}
				sLeaser.sprites[SegmentSprite(i, 0)].x = vector2.x - camPos.x;
				sLeaser.sprites[SegmentSprite(i, 0)].y = vector2.y - camPos.y;
				sLeaser.sprites[SegmentSprite(i, 0)].rotation = Custom.VecToDeg((vector3 + vector5).normalized) + 90f;
				sLeaser.sprites[SegmentSprite(i, 1)].x = vector2.x - camPos.x;
				sLeaser.sprites[SegmentSprite(i, 1)].y = vector2.y - camPos.y;
				sLeaser.sprites[SegmentSprite(i, 1)].rotation = Custom.VecToDeg((vector3 + vector5).normalized) + 90f;
				vector = vector2;
			}
			for (int j = 0; j < smallCords.GetLength(0); j++)
			{
				Vector2 vector6 = Vector2.Lerp(smallCords[j, 0, 1], smallCords[j, 0, 0], timeStacker);
				float num3 = 0.5f;
				for (int k = 0; k < smallCords.GetLength(1); k++)
				{
					Vector2 vector7 = Vector2.Lerp(smallCords[j, k, 1], smallCords[j, k, 0], timeStacker);
					Vector2 normalized = (vector6 - vector7).normalized;
					Vector2 vector8 = Custom.PerpendicularVector(normalized);
					float num4 = Vector2.Distance(vector6, vector7) / 5f;
					(sLeaser.sprites[SmallCordSprite(j)] as TriangleMesh).MoveVertice(k * 4, vector6 - normalized * num4 - vector8 * num3 - camPos);
					(sLeaser.sprites[SmallCordSprite(j)] as TriangleMesh).MoveVertice(k * 4 + 1, vector6 - normalized * num4 + vector8 * num3 - camPos);
					(sLeaser.sprites[SmallCordSprite(j)] as TriangleMesh).MoveVertice(k * 4 + 2, vector7 + normalized * num4 - vector8 * num3 - camPos);
					(sLeaser.sprites[SmallCordSprite(j)] as TriangleMesh).MoveVertice(k * 4 + 3, vector7 + normalized * num4 + vector8 * num3 - camPos);
					vector6 = vector7;
				}
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			sLeaser.sprites[firstSprite].color = owner.armJointGraphics[0].metalColor;
			for (int i = 0; i < coord.GetLength(0); i++)
			{
				sLeaser.sprites[SegmentSprite(i, 0)].color = Color.Lerp(owner.armJointGraphics[0].BaseColor(default(Vector2)), owner.armJointGraphics[0].metalColor, 0.5f);
				sLeaser.sprites[SegmentSprite(i, 1)].color = Color.Lerp(owner.armJointGraphics[0].HighLightColor(default(Vector2)), owner.armJointGraphics[0].metalColor, 0.35f);
			}
			for (int j = 0; j < smallCords.GetLength(0); j++)
			{
				if (smallCoordColors[j] == 0)
				{
					sLeaser.sprites[SmallCordSprite(j)].color = owner.armJointGraphics[0].metalColor;
				}
				else if (smallCoordColors[j] == 1)
				{
					sLeaser.sprites[SmallCordSprite(j)].color = Color.Lerp(new Color(1f, 0f, 0f), owner.armJointGraphics[0].metalColor, 0.5f);
				}
				else if (smallCoordColors[j] == 2)
				{
					sLeaser.sprites[SmallCordSprite(j)].color = Color.Lerp(new Color(0f, 0f, 1f), owner.armJointGraphics[0].metalColor, 0.5f);
				}
			}
		}
	}

	public class DisconnectedUbilicalCord
	{
		private OracleGraphics owner;

		public int firstSprite;

		public int totalSprites;

		public float[] smallCordsLengths;

		public Vector2[] smallCordsHeadDirs;

		public int[] smallCoordColors;

		public Vector2[,,] smallCords;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public int SmallCordSprite(int c)
		{
			return firstSprite + c;
		}

		public DisconnectedUbilicalCord(OracleGraphics owner, int firstSprite)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
			smallCords = new Vector2[10, 6, 3];
			smallCordsLengths = new float[smallCords.GetLength(0)];
			smallCordsHeadDirs = new Vector2[smallCords.GetLength(0)];
			smallCoordColors = new int[smallCords.GetLength(0)];
			for (int i = 0; i < smallCords.GetLength(0); i++)
			{
				smallCordsLengths[i] = Mathf.Lerp(1f, 20f, UnityEngine.Random.value);
				smallCoordColors[i] = ((!(UnityEngine.Random.value < 0.5f)) ? UnityEngine.Random.Range(1, 3) : 0);
				smallCordsHeadDirs[i] = Custom.RNV() * UnityEngine.Random.value;
			}
			totalSprites = smallCords.GetLength(0);
		}

		public void Reset(Vector2 pnt)
		{
			for (int i = 0; i < smallCords.GetLength(0); i++)
			{
				for (int j = 0; j < smallCords.GetLength(1); j++)
				{
					smallCords[i, j, 0] = pnt + Custom.RNV() * UnityEngine.Random.value;
					smallCords[i, j, 1] = smallCords[i, j, 0];
					smallCords[i, j, 2] *= 0f;
				}
			}
		}

		public void Update()
		{
			for (int i = 0; i < smallCords.GetLength(0); i++)
			{
				for (int j = 0; j < smallCords.GetLength(1); j++)
				{
					smallCords[i, j, 1] = smallCords[i, j, 0];
					smallCords[i, j, 0] += smallCords[i, j, 2];
					smallCords[i, j, 2] *= 0.94f;
					smallCords[i, j, 2].y -= owner.owner.room.gravity * 0.9f;
					SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(smallCords[i, j, 0], smallCords[i, j, 1], smallCords[i, j, 2], 1f, new IntVector2(0, 0), goThroughFloors: true);
					cd = SharedPhysics.VerticalCollision(owner.owner.room, cd);
					cd = SharedPhysics.HorizontalCollision(owner.owner.room, cd);
					cd = SharedPhysics.SlopesVertically(owner.owner.room, cd);
					smallCords[i, j, 0] = cd.pos;
					smallCords[i, j, 2] = cd.vel;
				}
				float num = smallCordsLengths[i] / (float)smallCords.GetLength(1);
				for (int k = 1; k < smallCords.GetLength(1); k++)
				{
					Vector2 vector = Custom.DirVec(smallCords[i, k, 0], smallCords[i, k - 1, 0]);
					float num2 = Vector2.Distance(smallCords[i, k, 0], smallCords[i, k - 1, 0]);
					smallCords[i, k, 0] -= (num - num2) * vector * 0.5f;
					smallCords[i, k, 2] -= (num - num2) * vector * 0.5f;
					smallCords[i, k - 1, 0] += (num - num2) * vector * 0.5f;
					smallCords[i, k - 1, 2] += (num - num2) * vector * 0.5f;
				}
				for (int l = 0; l < smallCords.GetLength(1) - 1; l++)
				{
					Vector2 vector2 = Custom.DirVec(smallCords[i, l, 0], smallCords[i, l + 1, 0]);
					float num3 = Vector2.Distance(smallCords[i, l, 0], smallCords[i, l + 1, 0]);
					smallCords[i, l, 0] -= (num - num3) * vector2 * 0.5f;
					smallCords[i, l, 2] -= (num - num3) * vector2 * 0.5f;
					smallCords[i, l + 1, 0] += (num - num3) * vector2 * 0.5f;
					smallCords[i, l + 1, 2] += (num - num3) * vector2 * 0.5f;
				}
				smallCords[i, smallCords.GetLength(1) - 1, 0] = owner.head.pos + Custom.DirVec(owner.owner.firstChunk.pos, owner.head.pos) * 3f;
				smallCords[i, smallCords.GetLength(1) - 1, 2] *= 0f;
				smallCords[i, smallCords.GetLength(1) - 2, 2] -= (owner.lookDir + smallCordsHeadDirs[i]) * 3f;
				smallCords[i, smallCords.GetLength(1) - 3, 2] -= (owner.lookDir + smallCordsHeadDirs[i]) * 1.5f;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < smallCords.GetLength(0); i++)
			{
				sLeaser.sprites[SmallCordSprite(i)] = TriangleMesh.MakeLongMesh(smallCords.GetLength(1), pointyTip: false, customColor: false);
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < smallCords.GetLength(0); i++)
			{
				Vector2 vector = Vector2.Lerp(smallCords[i, 0, 1], smallCords[i, 0, 0], timeStacker);
				float num = 0.5f;
				for (int j = 0; j < smallCords.GetLength(1); j++)
				{
					Vector2 vector2 = Vector2.Lerp(smallCords[i, j, 1], smallCords[i, j, 0], timeStacker);
					Vector2 normalized = (vector - vector2).normalized;
					Vector2 vector3 = Custom.PerpendicularVector(normalized);
					float num2 = Vector2.Distance(vector, vector2) / 5f;
					(sLeaser.sprites[SmallCordSprite(i)] as TriangleMesh).MoveVertice(j * 4, vector - normalized * num2 - vector3 * num - camPos);
					(sLeaser.sprites[SmallCordSprite(i)] as TriangleMesh).MoveVertice(j * 4 + 1, vector - normalized * num2 + vector3 * num - camPos);
					(sLeaser.sprites[SmallCordSprite(i)] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 + normalized * num2 - vector3 * num - camPos);
					(sLeaser.sprites[SmallCordSprite(i)] as TriangleMesh).MoveVertice(j * 4 + 3, vector2 + normalized * num2 + vector3 * num - camPos);
					vector = vector2;
				}
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color b = Color.Lerp(owner.armJointGraphics[0].metalColor, new Color(0.7f, 0.7f, 0.7f), 0.15f);
			for (int i = 0; i < smallCords.GetLength(0); i++)
			{
				if (smallCoordColors[i] == 0)
				{
					sLeaser.sprites[SmallCordSprite(i)].color = Color.Lerp(owner.armJointGraphics[0].metalColor, b, 0.5f);
				}
				else if (smallCoordColors[i] == 1)
				{
					sLeaser.sprites[SmallCordSprite(i)].color = Color.Lerp(new Color(1f, 0f, 0f), b, 0.5f);
				}
				else if (smallCoordColors[i] == 2)
				{
					sLeaser.sprites[SmallCordSprite(i)].color = Color.Lerp(new Color(0f, 0f, 1f), b, 0.5f);
				}
			}
		}
	}

	public class ArmBase
	{
		private OracleGraphics owner;

		public int firstSprite;

		public int totalSprites;

		public int SupportSprite(int side, int part)
		{
			return firstSprite + side * 3 + part;
		}

		public int CircleSprite(int side, int part)
		{
			return firstSprite + 9 + side * 2 + part;
		}

		public ArmBase(OracleGraphics owner, int firstSprite)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
			totalSprites = 13;
		}

		public void Update()
		{
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[12]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(0, 2, 3),
				new TriangleMesh.Triangle(0, 3, 4),
				new TriangleMesh.Triangle(0, 4, 5),
				new TriangleMesh.Triangle(4, 5, 7),
				new TriangleMesh.Triangle(6, 5, 7),
				new TriangleMesh.Triangle(11, 6, 7),
				new TriangleMesh.Triangle(11, 7, 8),
				new TriangleMesh.Triangle(11, 8, 9),
				new TriangleMesh.Triangle(11, 9, 10),
				default(TriangleMesh.Triangle),
				default(TriangleMesh.Triangle)
			};
			sLeaser.sprites[firstSprite + 6] = new TriangleMesh("Futile_White", tris, customColor: false);
			sLeaser.sprites[firstSprite + 7] = new TriangleMesh("Futile_White", tris, customColor: false);
			sLeaser.sprites[firstSprite + 8] = new TriangleMesh("Futile_White", tris, customColor: false);
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					sLeaser.sprites[SupportSprite(i, j)] = new FSprite("pixel");
					sLeaser.sprites[SupportSprite(i, j)].scaleX = 2f;
					sLeaser.sprites[SupportSprite(i, j)].anchorY = 0f;
					sLeaser.sprites[CircleSprite(i, j)] = new FSprite("Circle20");
				}
				sLeaser.sprites[CircleSprite(i, 0)].scale = 0.5f;
				sLeaser.sprites[CircleSprite(i, 1)].scale = 0.45f;
				sLeaser.sprites[SupportSprite(i, 2)] = new FSprite("deerEyeB");
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			bool isVisible = rCam.IsViewedByCameraPosition(rCam.currentCameraPosition, owner.oracle.firstChunk.pos);
			sLeaser.sprites[firstSprite + 6].isVisible = isVisible;
			sLeaser.sprites[firstSprite + 7].isVisible = isVisible;
			sLeaser.sprites[firstSprite + 8].isVisible = isVisible;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					sLeaser.sprites[SupportSprite(i, j)].isVisible = isVisible;
					sLeaser.sprites[CircleSprite(i, j)].isVisible = isVisible;
				}
				sLeaser.sprites[SupportSprite(i, 2)].isVisible = isVisible;
			}
			Vector2 vector = Vector2.Lerp(owner.armJointGraphics[0].myJoint.lastPos, owner.armJointGraphics[0].myJoint.pos, timeStacker);
			Vector2 vector2 = owner.oracle.arm.BaseDir(timeStacker);
			vector -= vector2 * 10f;
			Vector2 vector3 = Custom.PerpendicularVector(vector2);
			UpdateMesh(sLeaser, camPos, firstSprite + 6, vector, vector2, vector3, 30f, 17f, 20f);
			UpdateMesh(sLeaser, camPos, firstSprite + 7, vector, vector2, vector3, 24f, 15f, 30f);
			UpdateMesh(sLeaser, camPos, firstSprite + 8, vector, vector2, vector3, 30f, 17f, 20f);
			Vector2 b = owner.armJointGraphics[0].myJoint.ElbowPos(timeStacker, Vector2.Lerp(owner.armJointGraphics[1].myJoint.lastPos, owner.armJointGraphics[1].myJoint.pos, timeStacker));
			for (int k = 0; k < 2; k++)
			{
				Vector2 vector4 = vector + vector2 * 11f + vector3 * 17f * ((k == 0) ? (-1f) : 1f);
				Vector2 vector5 = Vector2.Lerp(Vector2.Lerp(owner.armJointGraphics[0].myJoint.lastPos, owner.armJointGraphics[0].myJoint.pos, timeStacker), b, 0.25f);
				Vector2 vector6 = Custom.InverseKinematic(vector4, vector5, 25f, 45f, (k == 0) ? 1f : (-1f));
				sLeaser.sprites[SupportSprite(k, 0)].x = vector4.x - camPos.x;
				sLeaser.sprites[SupportSprite(k, 0)].y = vector4.y - camPos.y;
				sLeaser.sprites[SupportSprite(k, 0)].rotation = Custom.AimFromOneVectorToAnother(vector4, vector6);
				sLeaser.sprites[SupportSprite(k, 0)].scaleY = Vector2.Distance(vector4, vector6) + 10f;
				sLeaser.sprites[SupportSprite(k, 1)].x = vector6.x - camPos.x;
				sLeaser.sprites[SupportSprite(k, 1)].y = vector6.y - camPos.y;
				sLeaser.sprites[SupportSprite(k, 1)].rotation = Custom.AimFromOneVectorToAnother(vector6, vector5);
				sLeaser.sprites[SupportSprite(k, 1)].scaleY = Vector2.Distance(vector6, vector5);
				sLeaser.sprites[SupportSprite(k, 2)].x = vector6.x - camPos.x;
				sLeaser.sprites[SupportSprite(k, 2)].y = vector6.y - camPos.y;
				Vector2 vector7 = vector + 17f * vector3 * ((k == 0) ? (-1f) : 1f);
				sLeaser.sprites[CircleSprite(k, 0)].x = vector7.x - camPos.x;
				sLeaser.sprites[CircleSprite(k, 0)].y = vector7.y - camPos.y;
				sLeaser.sprites[CircleSprite(k, 1)].x = vector7.x - camPos.x - 1f;
				sLeaser.sprites[CircleSprite(k, 1)].y = vector7.y - camPos.y + 1f;
			}
		}

		private void UpdateMesh(RoomCamera.SpriteLeaser sLeaser, Vector2 camPos, int sprite, Vector2 pos, Vector2 dir, Vector2 perp, float width, float height, float innerWidth)
		{
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(0, pos - innerWidth * 0.5f * perp - height * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(1, pos - width * perp - height * 0.75f * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(2, pos - width * perp + height * 0.75f * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(3, pos - width * 0.8f * perp + height * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(4, pos - width * 0.5f * perp + height * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(5, pos - width * 0.3f * perp - height * 0.1f * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(6, pos + width * 0.3f * perp - height * 0.1f * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(7, pos + width * 0.5f * perp + height * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(8, pos + width * 0.8f * perp + height * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(9, pos + width * perp + height * 0.75f * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(10, pos + width * perp - height * 0.75f * dir - camPos);
			(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(11, pos + innerWidth * 0.5f * perp - height * dir - camPos);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color color = Color.Lerp(owner.armJointGraphics[0].metalColor, owner.armJointGraphics[0].BaseColor(default(Vector2)), 0.8f);
			Color color2 = Color.Lerp(owner.armJointGraphics[0].metalColor, owner.armJointGraphics[0].HighLightColor(default(Vector2)), 0.8f);
			sLeaser.sprites[firstSprite + 6].color = color;
			sLeaser.sprites[firstSprite + 7].color = color2;
			sLeaser.sprites[firstSprite + 8].color = color;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					sLeaser.sprites[SupportSprite(i, j)].color = owner.armJointGraphics[0].metalColor;
				}
				sLeaser.sprites[CircleSprite(i, 0)].color = color;
				sLeaser.sprites[CircleSprite(i, 1)].color = color2;
			}
		}
	}

	public int totalSprites;

	public int firstBodyChunkSprite;

	public int firstFootSprite;

	public int firstHandSprite;

	public int firstUmbilicalSprite;

	public int firstArmBaseSprite;

	public int neckSprite;

	public ArmJointGraphics[] armJointGraphics;

	public Gown gown;

	public Halo halo;

	public UbilicalCord umbCord;

	public DisconnectedUbilicalCord discUmbCord;

	public ArmBase armBase;

	public int fadeSprite;

	public int killSprite;

	public GenericBodyPart head;

	public GenericBodyPart[] hands;

	public GenericBodyPart[] feet;

	public Vector2[,] knees;

	public int robeSprite;

	public int firstHeadSprite;

	private Vector2 lookDir;

	private Vector2 lastLookDir;

	public float eyesOpen;

	public float lastEyesOpen;

	public float breathe;

	public float[] voiceFreqSamples;

	public float averageVoice;

	public Vector2 randomTalkVector;

	public LightSource lightsource;

	private Color SLArmBaseColA;

	private Color SLArmBaseColB;

	private Color SLArmHighLightColA;

	private Color SLArmHighLightColB;

	private float breathFac;

	private float lastBreatheFac;

	public bool initiated;

	public Oracle oracle => base.owner as Oracle;

	public int HeadSprite => firstHeadSprite + 3;

	public int ChinSprite => firstHeadSprite + 4;

	public int MoonThirdEyeSprite => firstHeadSprite + 11;

	public bool IsPebbles => oracle.ID == Oracle.OracleID.SS;

	public bool IsMoon => oracle.ID == Oracle.OracleID.SL;

	public bool IsPastMoon
	{
		get
		{
			if (ModManager.MSC)
			{
				return oracle.ID == MoreSlugcatsEnums.OracleID.DM;
			}
			return false;
		}
	}

	public bool IsStraw
	{
		get
		{
			if (ModManager.MSC)
			{
				return oracle.ID == MoreSlugcatsEnums.OracleID.ST;
			}
			return false;
		}
	}

	public bool IsSaintPebbles
	{
		get
		{
			if (ModManager.MSC)
			{
				return oracle.ID == MoreSlugcatsEnums.OracleID.CL;
			}
			return false;
		}
	}

	public bool IsRottedPebbles
	{
		get
		{
			if (ModManager.MSC && oracle.ID == Oracle.OracleID.SS)
			{
				return oracle.room.world.name == "RM";
			}
			return false;
		}
	}

	public int MoonSigilSprite => MoonThirdEyeSprite + 1;

	public int FootSprite(int side, int part)
	{
		return firstFootSprite + side * 2 + part;
	}

	public int HandSprite(int side, int part)
	{
		return firstHandSprite + side * 2 + part;
	}

	public int PhoneSprite(int side, int part)
	{
		if (side == 0)
		{
			return firstHeadSprite + part;
		}
		return firstHeadSprite + 7 + part;
	}

	public int EyeSprite(int e)
	{
		return firstHeadSprite + 5 + e;
	}

	private Vector2 RelativeLookDir(float timeStacker)
	{
		return Custom.RotateAroundOrigo(Vector2.Lerp(lastLookDir, lookDir, timeStacker), 0f - Custom.AimFromOneVectorToAnother(Vector2.Lerp(oracle.bodyChunks[1].lastPos, oracle.bodyChunks[1].pos, timeStacker), Vector2.Lerp(oracle.firstChunk.lastPos, oracle.firstChunk.pos, timeStacker)));
	}

	public OracleGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((!IsPebbles && !IsSaintPebbles) ? 10544 : 56);
		totalSprites = 0;
		armJointGraphics = new ArmJointGraphics[oracle.arm.joints.Length];
		for (int i = 0; i < oracle.arm.joints.Length; i++)
		{
			armJointGraphics[i] = new ArmJointGraphics(this, oracle.arm.joints[i], totalSprites);
			totalSprites += armJointGraphics[i].totalSprites;
		}
		if (IsMoon)
		{
			firstUmbilicalSprite = totalSprites;
			discUmbCord = new DisconnectedUbilicalCord(this, totalSprites);
			totalSprites += discUmbCord.totalSprites;
			discUmbCord.Reset(oracle.firstChunk.pos);
		}
		else
		{
			firstUmbilicalSprite = totalSprites;
			umbCord = new UbilicalCord(this, totalSprites);
			totalSprites += umbCord.totalSprites;
		}
		firstBodyChunkSprite = totalSprites;
		totalSprites += 2;
		neckSprite = totalSprites;
		totalSprites++;
		firstFootSprite = totalSprites;
		totalSprites += 4;
		if (!IsMoon)
		{
			if (!IsRottedPebbles && !IsSaintPebbles)
			{
				halo = new Halo(this, totalSprites);
				totalSprites += halo.totalSprites;
			}
			gown = new Gown(this);
			robeSprite = totalSprites;
			totalSprites++;
		}
		else if (IsMoon && oracle.room.game.MoonHasRobe())
		{
			gown = new Gown(this);
			robeSprite = totalSprites;
			totalSprites++;
		}
		firstHandSprite = totalSprites;
		totalSprites += 4;
		head = new GenericBodyPart(this, 5f, 0.5f, 0.995f, oracle.firstChunk);
		firstHeadSprite = totalSprites;
		totalSprites += 10;
		fadeSprite = totalSprites;
		totalSprites++;
		if (IsPebbles)
		{
			killSprite = totalSprites;
			totalSprites++;
		}
		else if (IsMoon || IsPastMoon || IsStraw)
		{
			totalSprites++;
		}
		if (IsPastMoon)
		{
			totalSprites++;
		}
		hands = new GenericBodyPart[2];
		for (int j = 0; j < 2; j++)
		{
			hands[j] = new GenericBodyPart(this, 2f, 0.5f, 0.98f, oracle.firstChunk);
		}
		feet = new GenericBodyPart[2];
		for (int k = 0; k < 2; k++)
		{
			feet[k] = new GenericBodyPart(this, 2f, 0.5f, 0.98f, oracle.firstChunk);
		}
		knees = new Vector2[2, 2];
		for (int l = 0; l < 2; l++)
		{
			for (int m = 0; m < 2; m++)
			{
				knees[l, m] = oracle.firstChunk.pos;
			}
		}
		firstArmBaseSprite = totalSprites;
		armBase = new ArmBase(this, firstArmBaseSprite);
		totalSprites += armBase.totalSprites;
		voiceFreqSamples = new float[64];
		UnityEngine.Random.state = state;
	}

	public override void Update()
	{
		base.Update();
		if (oracle == null || oracle.room == null)
		{
			return;
		}
		breathe += 1f / Mathf.Lerp(10f, 60f, oracle.health);
		lastBreatheFac = breathFac;
		breathFac = Mathf.Lerp(0.5f + 0.5f * Mathf.Sin(breathe * (float)Math.PI * 2f), 1f, Mathf.Pow(oracle.health, 2f));
		if (gown != null)
		{
			gown.Update();
		}
		if (halo != null)
		{
			halo.Update();
		}
		if (armBase != null)
		{
			armBase.Update();
		}
		bool flag = ((IsMoon && (oracle.oracleBehavior as SLOracleBehavior).holdKnees) || (IsRottedPebbles && (oracle.oracleBehavior as SSOracleRotBehavior).holdKnees) || (IsSaintPebbles && (oracle.oracleBehavior as CLOracleBehavior).holdKnees)) && oracle.bodyChunks[1].ContactPoint.y < 0;
		lastLookDir = lookDir;
		if (oracle.Consious)
		{
			lookDir = Vector2.ClampMagnitude(oracle.oracleBehavior.lookPoint - oracle.firstChunk.pos, 100f) / 100f;
			lookDir = Vector2.ClampMagnitude(lookDir + randomTalkVector * averageVoice * 0.3f, 1f);
		}
		head.Update();
		head.ConnectToPoint(oracle.firstChunk.pos + Custom.DirVec(oracle.bodyChunks[1].pos, oracle.firstChunk.pos) * 6f, 8f, push: true, 0f, oracle.firstChunk.vel, 0.5f, 0.01f);
		if (oracle.Consious)
		{
			if (flag && oracle.oracleBehavior.EyesClosed)
			{
				head.vel += Custom.DegToVec(-90f);
			}
			else
			{
				head.vel += Custom.DirVec(oracle.bodyChunks[1].pos, oracle.firstChunk.pos) * breathFac;
				head.vel += lookDir * 0.5f * breathFac;
			}
		}
		else
		{
			head.vel += Custom.DirVec(oracle.bodyChunks[1].pos, oracle.firstChunk.pos) * 0.75f;
			head.vel.y -= 0.7f;
		}
		for (int i = 0; i < 2; i++)
		{
			feet[i].Update();
			feet[i].ConnectToPoint(oracle.bodyChunks[1].pos, IsMoon ? 20f : 10f, push: false, 0f, oracle.bodyChunks[1].vel, 0.3f, 0.01f);
			if (IsMoon)
			{
				feet[i].vel.y -= 0.5f;
			}
			feet[i].vel += Custom.DirVec(oracle.firstChunk.pos, oracle.bodyChunks[1].pos) * 0.3f;
			feet[i].vel += Custom.PerpendicularVector(Custom.DirVec(oracle.firstChunk.pos, oracle.bodyChunks[1].pos)) * 0.15f * ((i == 0) ? (-1f) : 1f);
			hands[i].Update();
			hands[i].ConnectToPoint(oracle.firstChunk.pos, 15f, push: false, 0f, oracle.firstChunk.vel, 0.3f, 0.01f);
			hands[i].vel.y -= 0.5f;
			hands[i].vel += Custom.DirVec(oracle.firstChunk.pos, oracle.bodyChunks[1].pos) * 0.3f;
			hands[i].vel += Custom.PerpendicularVector(Custom.DirVec(oracle.firstChunk.pos, oracle.bodyChunks[1].pos)) * 0.3f * ((i == 0) ? (-1f) : 1f);
			knees[i, 1] = knees[i, 0];
			if (IsRottedPebbles)
			{
				Vector2? vector = ((!flag) ? SharedPhysics.ExactTerrainRayTracePos(oracle.room, oracle.bodyChunks[(!(oracle.oracleBehavior as SSOracleRotBehavior).InSitPosition) ? 1u : 0u].pos, oracle.bodyChunks[(!(oracle.oracleBehavior as SSOracleRotBehavior).InSitPosition) ? 1u : 0u].pos + new Vector2((i != 0) ? (-24f) : (-54f), -40f) * 2f * (oracle.oracleBehavior as SSOracleRotBehavior).CrawlSpeed) : SharedPhysics.ExactTerrainRayTracePos(oracle.room, oracle.firstChunk.pos, oracle.firstChunk.pos + new Vector2((i != 0) ? (-14f) : (-24f), -40f)));
				if (vector.HasValue)
				{
					feet[i].vel += Vector2.ClampMagnitude(vector.Value - feet[i].pos, 10f) / 2f;
				}
				Vector2 vector2 = (feet[i].pos + oracle.bodyChunks[1].pos) / 2f;
				if (flag && vector.HasValue)
				{
					Vector2 vector3 = feet[i].pos + Custom.DirVec(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * 15f;
					vector3 += Custom.DirVec(oracle.firstChunk.pos, vector3) * 5f;
					vector2 = Vector2.Lerp(vector3, (feet[i].pos + oracle.bodyChunks[1].pos) / 2f, Mathf.InverseLerp(7f, 14f, Vector2.Distance(feet[i].pos, oracle.bodyChunks[1].pos)));
				}
				else
				{
					vector2 += Custom.PerpendicularVector(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * ((i != 0) ? 1f : (-1f)) * 5f;
				}
				knees[i, 0] = Vector2.Lerp(knees[i, 0], vector2, 0.4f);
				if (!Custom.DistLess(knees[i, 0], vector2, 15f))
				{
					knees[i, 0] = vector2 + Custom.DirVec(vector2, knees[i, 0]);
				}
				if (oracle.Consious && i == 0 && (oracle.oracleBehavior as SSOracleRotBehavior).holdingObject != null)
				{
					hands[i].pos = (oracle.oracleBehavior as SSOracleRotBehavior).holdingObject.firstChunk.pos;
					hands[i].vel *= 0f;
				}
				if (oracle.oracleBehavior.player != null && i == 0 == oracle.firstChunk.pos.x > oracle.oracleBehavior.player.DangerPos.x && Custom.DistLess(oracle.firstChunk.pos, oracle.oracleBehavior.player.DangerPos, 40f))
				{
					hands[i].vel = Vector2.Lerp(hands[i].vel, Custom.DirVec(hands[i].pos, oracle.oracleBehavior.player.mainBodyChunk.pos) * 10f, 0.5f);
				}
				else if (flag)
				{
					hands[i].vel += Vector2.ClampMagnitude(knees[i, 0] - hands[i].pos, 10f) / 3f;
				}
				else if (!(oracle.oracleBehavior as SSOracleRotBehavior).InSitPosition)
				{
					vector = SharedPhysics.ExactTerrainRayTracePos(oracle.room, oracle.firstChunk.pos, oracle.firstChunk.pos + new Vector2(((i != 0) ? 1f : (-1f)) * (oracle.oracleBehavior as SSOracleRotBehavior).Crawl * 40f, -40f));
					if (vector.HasValue)
					{
						hands[i].vel += Vector2.ClampMagnitude(vector.Value - hands[i].pos, 10f) / 3f;
					}
					else
					{
						GenericBodyPart genericBodyPart = hands[i];
						genericBodyPart.vel.x = genericBodyPart.vel.x + ((i != 0) ? 1f : (-1f)) * (oracle.oracleBehavior as SSOracleRotBehavior).Crawl;
					}
					knees[i, 0] = feet[i].pos + Custom.DirVec(feet[i].pos, oracle.oracleBehavior.OracleGetToPos + new Vector2(-50f, 0f)) * 8f + Custom.PerpendicularVector(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * ((i != 0) ? 1f : (-1f)) * Mathf.Lerp(2f, 6f, (oracle.oracleBehavior as SSOracleRotBehavior).CrawlSpeed);
				}
				continue;
			}
			if (IsSaintPebbles)
			{
				Vector2? vector4 = ((!flag) ? SharedPhysics.ExactTerrainRayTracePos(oracle.room, oracle.bodyChunks[(!(oracle.oracleBehavior as CLOracleBehavior).InSitPosition) ? 1u : 0u].pos, oracle.bodyChunks[(!(oracle.oracleBehavior as CLOracleBehavior).InSitPosition) ? 1u : 0u].pos + new Vector2((i != 0) ? (-24f) : (-54f), -40f) * 2f * (oracle.oracleBehavior as CLOracleBehavior).CrawlSpeed) : SharedPhysics.ExactTerrainRayTracePos(oracle.room, oracle.firstChunk.pos, oracle.firstChunk.pos + new Vector2((i != 0) ? (-14f) : (-24f), -40f)));
				if (vector4.HasValue)
				{
					feet[i].vel += Vector2.ClampMagnitude(vector4.Value - feet[i].pos, 10f) / 2f;
				}
				Vector2 vector5 = (feet[i].pos + oracle.bodyChunks[1].pos) / 2f;
				if (flag && vector4.HasValue)
				{
					Vector2 vector6 = feet[i].pos + Custom.DirVec(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * 15f;
					vector6 += Custom.DirVec(oracle.firstChunk.pos, vector6) * 5f;
					vector5 = Vector2.Lerp(vector6, (feet[i].pos + oracle.bodyChunks[1].pos) / 2f, Mathf.InverseLerp(7f, 14f, Vector2.Distance(feet[i].pos, oracle.bodyChunks[1].pos)));
				}
				else
				{
					vector5 += Custom.PerpendicularVector(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * ((i != 0) ? 1f : (-1f)) * 5f;
				}
				knees[i, 0] = Vector2.Lerp(knees[i, 0], vector5, 0.4f);
				if (!Custom.DistLess(knees[i, 0], vector5, 15f))
				{
					knees[i, 0] = vector5 + Custom.DirVec(vector5, knees[i, 0]);
				}
				if (oracle.oracleBehavior.player != null && i == 0 == oracle.firstChunk.pos.x > oracle.oracleBehavior.player.DangerPos.x && Custom.DistLess(oracle.firstChunk.pos, oracle.oracleBehavior.player.DangerPos, 40f))
				{
					hands[i].vel = Vector2.Lerp(hands[i].vel, Custom.DirVec(hands[i].pos, oracle.oracleBehavior.player.mainBodyChunk.pos) * 10f, 0.5f);
				}
				else if (flag)
				{
					hands[i].vel += Vector2.ClampMagnitude(knees[i, 0] - hands[i].pos, 10f) / 3f;
				}
				else if (!(oracle.oracleBehavior as CLOracleBehavior).InSitPosition)
				{
					vector4 = SharedPhysics.ExactTerrainRayTracePos(oracle.room, oracle.firstChunk.pos, oracle.firstChunk.pos + new Vector2(((i != 0) ? 1f : (-1f)) * (oracle.oracleBehavior as CLOracleBehavior).Crawl * 40f, -40f));
					if (vector4.HasValue)
					{
						hands[i].vel += Vector2.ClampMagnitude(vector4.Value - hands[i].pos, 10f) / 3f;
					}
					else
					{
						GenericBodyPart genericBodyPart2 = hands[i];
						genericBodyPart2.vel.x = genericBodyPart2.vel.x + ((i != 0) ? 1f : (-1f)) * (oracle.oracleBehavior as CLOracleBehavior).Crawl;
					}
					knees[i, 0] = feet[i].pos + Custom.DirVec(feet[i].pos, oracle.oracleBehavior.OracleGetToPos + new Vector2(-50f, 0f)) * 8f + Custom.PerpendicularVector(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * ((i != 0) ? 1f : (-1f)) * Mathf.Lerp(2f, 6f, (oracle.oracleBehavior as CLOracleBehavior).CrawlSpeed);
				}
				continue;
			}
			if (!IsMoon)
			{
				hands[i].vel += randomTalkVector * averageVoice * 0.8f;
				if (oracle.oracleBehavior.player != null && i == 0 && oracle.oracleBehavior is SSOracleBehavior && (oracle.oracleBehavior as SSOracleBehavior).HandTowardsPlayer())
				{
					hands[0].vel += Custom.DirVec(hands[0].pos, oracle.oracleBehavior.player.mainBodyChunk.pos) * 3f;
				}
				if (i == 0 && oracle.oracleBehavior is STOracleBehavior)
				{
					Vector2? vector7 = (oracle.oracleBehavior as STOracleBehavior).HandDirection();
					if (vector7.HasValue)
					{
						hands[0].vel += vector7.Value * 3f;
					}
				}
				knees[i, 0] = (feet[i].pos + oracle.bodyChunks[1].pos) / 2f + Custom.PerpendicularVector(Custom.DirVec(oracle.firstChunk.pos, oracle.bodyChunks[1].pos)) * 4f * ((i == 0) ? (-1f) : 1f);
				continue;
			}
			Vector2? vector8 = null;
			vector8 = ((!flag) ? SharedPhysics.ExactTerrainRayTracePos(oracle.room, oracle.bodyChunks[(!(oracle.oracleBehavior as SLOracleBehavior).InSitPosition) ? 1u : 0u].pos, oracle.bodyChunks[(!(oracle.oracleBehavior as SLOracleBehavior).InSitPosition) ? 1u : 0u].pos + new Vector2((i == 0) ? (-54f) : (-24f), -40f) * 2f * (oracle.oracleBehavior as SLOracleBehavior).CrawlSpeed) : SharedPhysics.ExactTerrainRayTracePos(oracle.room, oracle.firstChunk.pos, oracle.firstChunk.pos + new Vector2((i == 0) ? (-24f) : (-14f), -40f)));
			if (vector8.HasValue)
			{
				feet[i].vel += Vector2.ClampMagnitude(vector8.Value - feet[i].pos, 10f) / 2f;
			}
			Vector2 vector9 = (feet[i].pos + oracle.bodyChunks[1].pos) / 2f;
			if (flag && vector8.HasValue)
			{
				Vector2 vector10 = feet[i].pos + Custom.DirVec(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * 15f;
				vector10 += Custom.DirVec(oracle.firstChunk.pos, vector10) * 5f;
				vector9 = Vector2.Lerp(vector10, (feet[i].pos + oracle.bodyChunks[1].pos) / 2f, Mathf.InverseLerp(7f, 14f, Vector2.Distance(feet[i].pos, oracle.bodyChunks[1].pos)));
			}
			else
			{
				vector9 += Custom.PerpendicularVector(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * ((i == 0) ? (-1f) : 1f) * 5f;
			}
			knees[i, 0] = Vector2.Lerp(knees[i, 0], vector9, 0.4f);
			if (!Custom.DistLess(knees[i, 0], vector9, 15f))
			{
				knees[i, 0] = vector9 + Custom.DirVec(vector9, knees[i, 0]);
			}
			if (oracle.Consious && i == 0 && (oracle.oracleBehavior as SLOracleBehavior).holdingObject != null)
			{
				hands[i].pos = (oracle.oracleBehavior as SLOracleBehavior).holdingObject.firstChunk.pos;
				hands[i].vel *= 0f;
			}
			if (oracle.oracleBehavior.player != null && oracle.Consious && i == 0 == oracle.firstChunk.pos.x > oracle.oracleBehavior.player.DangerPos.x && Custom.DistLess(oracle.firstChunk.pos, oracle.oracleBehavior.player.DangerPos, 40f))
			{
				hands[i].vel = Vector2.Lerp(hands[i].vel, Custom.DirVec(hands[i].pos, oracle.oracleBehavior.player.mainBodyChunk.pos) * 10f, 0.5f);
			}
			else if (oracle.oracleBehavior.player != null && oracle.Consious && i == 0 == oracle.firstChunk.pos.x > oracle.oracleBehavior.player.DangerPos.x && (oracle.oracleBehavior as SLOracleBehavior).armsProtest)
			{
				hands[i].vel = Vector2.Lerp(hands[i].vel, Custom.DirVec(hands[i].pos, oracle.oracleBehavior.player.mainBodyChunk.pos) * 10f, 0.5f) + new Vector2(0f, 10f * Mathf.Sin((oracle.oracleBehavior as SLOracleBehavior).protestCounter * (float)Math.PI * 2f));
			}
			else if (flag)
			{
				hands[i].vel += Vector2.ClampMagnitude(knees[i, 0] - hands[i].pos, 10f) / 3f;
			}
			else if (!(oracle.oracleBehavior as SLOracleBehavior).InSitPosition)
			{
				vector8 = SharedPhysics.ExactTerrainRayTracePos(oracle.room, oracle.firstChunk.pos, oracle.firstChunk.pos + new Vector2(((i == 0) ? (-1f) : 1f) * (oracle.oracleBehavior as SLOracleBehavior).Crawl * 40f, -40f));
				if (vector8.HasValue)
				{
					hands[i].vel += Vector2.ClampMagnitude(vector8.Value - hands[i].pos, 10f) / 3f;
				}
				else
				{
					hands[i].vel.x += ((i == 0) ? (-1f) : 1f) * (oracle.oracleBehavior as SLOracleBehavior).Crawl;
				}
				knees[i, 0] = feet[i].pos + Custom.DirVec(feet[i].pos, oracle.oracleBehavior.OracleGetToPos + new Vector2(-50f, 0f)) * 8f + Custom.PerpendicularVector(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * ((i == 0) ? (-1f) : 1f) * Mathf.Lerp(2f, 6f, (oracle.oracleBehavior as SLOracleBehavior).CrawlSpeed);
			}
		}
		for (int j = 0; j < armJointGraphics.Length; j++)
		{
			armJointGraphics[j].Update();
		}
		if (umbCord != null)
		{
			umbCord.Update();
		}
		else if (discUmbCord != null)
		{
			discUmbCord.Update();
		}
		if (oracle.oracleBehavior.voice != null && oracle.oracleBehavior.voice.currentSoundObject != null && oracle.oracleBehavior.voice.currentSoundObject.IsPlaying)
		{
			if (oracle.oracleBehavior.voice.currentSoundObject.IsLoaded)
			{
				oracle.oracleBehavior.voice.currentSoundObject.GetSpectrumData(voiceFreqSamples, 0, FFTWindow.BlackmanHarris);
				averageVoice = 0f;
				for (int k = 0; k < voiceFreqSamples.Length; k++)
				{
					averageVoice += voiceFreqSamples[k];
				}
				averageVoice /= voiceFreqSamples.Length;
				averageVoice = Mathf.InverseLerp(0f, 0.00014f, averageVoice);
				if (averageVoice > 0.7f && UnityEngine.Random.value < averageVoice / 14f)
				{
					randomTalkVector = Custom.RNV();
				}
			}
		}
		else
		{
			randomTalkVector *= 0.9f;
			if (averageVoice > 0f)
			{
				for (int l = 0; l < voiceFreqSamples.Length; l++)
				{
					voiceFreqSamples[l] = 0f;
				}
				averageVoice = 0f;
			}
		}
		lastEyesOpen = eyesOpen;
		eyesOpen = (oracle.oracleBehavior.EyesClosed ? 0f : 1f);
		if (base.owner.room.game.cameras[0].AboutToSwitchRoom && lightsource != null)
		{
			lightsource.RemoveFromRoom();
		}
		else
		{
			if (!IsPebbles)
			{
				return;
			}
			if (lightsource == null)
			{
				lightsource = new LightSource(oracle.firstChunk.pos, environmentalLight: false, Custom.HSL2RGB(0.1f, 1f, 0.5f), oracle);
				lightsource.affectedByPaletteDarkness = 0f;
				oracle.room.AddObject(lightsource);
				return;
			}
			if (IsRottedPebbles)
			{
				lightsource.setAlpha = 0.5f;
			}
			else if (ModManager.MSC && base.owner.room.world.name == "HR")
			{
				lightsource.setAlpha = 1f;
			}
			else
			{
				lightsource.setAlpha = (oracle.oracleBehavior as SSOracleBehavior).working;
			}
			lightsource.setRad = 400f;
			lightsource.setPos = oracle.firstChunk.pos;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			sLeaser.sprites[firstBodyChunkSprite + i] = new FSprite("Circle20");
			sLeaser.sprites[firstBodyChunkSprite + i].scale = base.owner.bodyChunks[i].rad / 10f;
			sLeaser.sprites[firstBodyChunkSprite + i].color = new Color(1f, (i == 0) ? 0.5f : 0f, (i == 0) ? 0.5f : 0f);
		}
		if (IsMoon || IsPastMoon)
		{
			sLeaser.sprites[firstBodyChunkSprite].scaleY = (base.owner.bodyChunks[0].rad + 2f) / 10f;
		}
		for (int j = 0; j < armJointGraphics.Length; j++)
		{
			armJointGraphics[j].InitiateSprites(sLeaser, rCam);
		}
		if (gown != null)
		{
			gown.InitiateSprite(robeSprite, sLeaser, rCam);
		}
		if (halo != null)
		{
			halo.InitiateSprites(sLeaser, rCam);
		}
		if (armBase != null)
		{
			armBase.InitiateSprites(sLeaser, rCam);
		}
		sLeaser.sprites[neckSprite] = new FSprite("pixel");
		sLeaser.sprites[neckSprite].scaleX = ((IsPebbles || IsSaintPebbles) ? 3f : 4f);
		sLeaser.sprites[neckSprite].anchorY = 0f;
		sLeaser.sprites[HeadSprite] = new FSprite("Circle20");
		sLeaser.sprites[ChinSprite] = new FSprite("Circle20");
		for (int k = 0; k < 2; k++)
		{
			sLeaser.sprites[EyeSprite(k)] = new FSprite("pixel");
			if (IsMoon || IsPastMoon || IsStraw)
			{
				sLeaser.sprites[EyeSprite(k)].color = new Color(0.02f, 0f, 0f);
			}
			sLeaser.sprites[PhoneSprite(k, 0)] = new FSprite("Circle20");
			sLeaser.sprites[PhoneSprite(k, 1)] = new FSprite("Circle20");
			sLeaser.sprites[PhoneSprite(k, 2)] = new FSprite("LizardScaleA1");
			sLeaser.sprites[PhoneSprite(k, 2)].anchorY = 0f;
			sLeaser.sprites[PhoneSprite(k, 2)].scaleY = 0.8f;
			sLeaser.sprites[PhoneSprite(k, 2)].scaleX = ((k == 0) ? (-1f) : 1f) * 0.75f;
			if (IsStraw)
			{
				sLeaser.sprites[PhoneSprite(k, 2)].isVisible = false;
			}
			sLeaser.sprites[HandSprite(k, 0)] = new FSprite("haloGlyph-1");
			sLeaser.sprites[HandSprite(k, 1)] = TriangleMesh.MakeLongMesh(7, pointyTip: false, customColor: true);
			sLeaser.sprites[FootSprite(k, 0)] = new FSprite("haloGlyph-1");
			sLeaser.sprites[FootSprite(k, 1)] = TriangleMesh.MakeLongMesh(7, pointyTip: false, customColor: true);
		}
		if (IsMoon || IsPastMoon)
		{
			sLeaser.sprites[MoonThirdEyeSprite] = new FSprite("Circle20");
		}
		if (IsStraw)
		{
			sLeaser.sprites[MoonThirdEyeSprite] = new FSprite("mouseEyeA5");
		}
		if (IsPastMoon)
		{
			sLeaser.sprites[MoonSigilSprite] = new FSprite("MoonSigil");
			sLeaser.sprites[MoonSigilSprite].shader = rCam.game.rainWorld.Shaders["Hologram"];
		}
		if (umbCord != null)
		{
			umbCord.InitiateSprites(sLeaser, rCam);
		}
		else if (discUmbCord != null)
		{
			discUmbCord.InitiateSprites(sLeaser, rCam);
		}
		sLeaser.sprites[HeadSprite].scaleX = head.rad / 9f;
		sLeaser.sprites[HeadSprite].scaleY = head.rad / 11f;
		sLeaser.sprites[ChinSprite].scale = head.rad / 15f;
		sLeaser.sprites[fadeSprite] = new FSprite("Futile_White");
		sLeaser.sprites[fadeSprite].scale = 12.5f;
		sLeaser.sprites[fadeSprite].color = (IsPebbles ? new Color(0f, 0f, 0f) : new Color(1f, 1f, 1f));
		if (IsPastMoon)
		{
			sLeaser.sprites[fadeSprite].color = new Color(0f, 0f, 1f);
		}
		sLeaser.sprites[fadeSprite].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		sLeaser.sprites[fadeSprite].alpha = (IsPebbles ? 0.5f : 0.2f);
		if (IsPebbles || IsPastMoon)
		{
			sLeaser.sprites[killSprite] = new FSprite("Futile_White");
			sLeaser.sprites[killSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];
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
		if (armBase != null)
		{
			for (int i = firstArmBaseSprite; i < firstArmBaseSprite + armBase.totalSprites; i++)
			{
				rCam.ReturnFContainer((i < firstArmBaseSprite + 6 || i == firstArmBaseSprite + 8) ? "Midground" : "Shortcuts").AddChild(sLeaser.sprites[i]);
			}
		}
		if (halo == null)
		{
			for (int j = 0; j < firstArmBaseSprite; j++)
			{
				newContatiner.AddChild(sLeaser.sprites[j]);
			}
		}
		else
		{
			for (int k = 0; k < halo.firstSprite; k++)
			{
				newContatiner.AddChild(sLeaser.sprites[k]);
			}
			FContainer fContainer = rCam.ReturnFContainer("BackgroundShortcuts");
			for (int l = halo.firstSprite; l < halo.firstSprite + halo.totalSprites; l++)
			{
				fContainer.AddChild(sLeaser.sprites[l]);
			}
			for (int m = halo.firstSprite + halo.totalSprites; m < firstArmBaseSprite; m++)
			{
				if (m != fadeSprite && m != killSprite)
				{
					newContatiner.AddChild(sLeaser.sprites[m]);
				}
			}
		}
		rCam.ReturnFContainer("Shortcuts").AddChild(sLeaser.sprites[fadeSprite]);
		rCam.ReturnFContainer("Shortcuts").AddChild(sLeaser.sprites[killSprite]);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (oracle == null || oracle.room == null)
		{
			return;
		}
		Vector2 vector = Vector2.Lerp(base.owner.firstChunk.lastPos, base.owner.firstChunk.pos, timeStacker);
		Vector2 vector2 = Custom.DirVec(Vector2.Lerp(base.owner.bodyChunks[1].lastPos, base.owner.bodyChunks[1].pos, timeStacker), vector);
		Vector2 vector3 = Custom.PerpendicularVector(vector2);
		Vector2 vector4 = Vector2.Lerp(lastLookDir, lookDir, timeStacker);
		Vector2 vector5 = Vector2.Lerp(head.lastPos, head.pos, timeStacker);
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			sLeaser.sprites[firstBodyChunkSprite + i].x = Mathf.Lerp(base.owner.bodyChunks[i].lastPos.x, base.owner.bodyChunks[i].pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[firstBodyChunkSprite + i].y = Mathf.Lerp(base.owner.bodyChunks[i].lastPos.y, base.owner.bodyChunks[i].pos.y, timeStacker) - camPos.y;
		}
		sLeaser.sprites[firstBodyChunkSprite].rotation = Custom.AimFromOneVectorToAnother(vector, vector5) - Mathf.Lerp(14f, 0f, Mathf.Lerp(lastBreatheFac, breathFac, timeStacker));
		sLeaser.sprites[firstBodyChunkSprite + 1].rotation = Custom.VecToDeg(vector2);
		for (int j = 0; j < armJointGraphics.Length; j++)
		{
			armJointGraphics[j].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		if (IsPebbles || IsPastMoon || IsSaintPebbles)
		{
			if (IsRottedPebbles || IsSaintPebbles)
			{
				sLeaser.sprites[killSprite].isVisible = false;
			}
			else if ((oracle.oracleBehavior as SSOracleBehavior).killFac > 0f)
			{
				sLeaser.sprites[killSprite].isVisible = true;
				if (IsPastMoon && (oracle.oracleBehavior as SSOracleBehavior).inspectPearl != null)
				{
					DataPearl inspectPearl = (oracle.oracleBehavior as SSOracleBehavior).inspectPearl;
					sLeaser.sprites[killSprite].x = Mathf.Lerp(inspectPearl.firstChunk.lastPos.x, inspectPearl.firstChunk.pos.x, timeStacker) - camPos.x;
					sLeaser.sprites[killSprite].y = Mathf.Lerp(inspectPearl.firstChunk.lastPos.y, inspectPearl.firstChunk.pos.y, timeStacker) - camPos.y;
				}
				else if (oracle.oracleBehavior.player != null)
				{
					sLeaser.sprites[killSprite].x = Mathf.Lerp(oracle.oracleBehavior.player.mainBodyChunk.lastPos.x, oracle.oracleBehavior.player.mainBodyChunk.pos.x, timeStacker) - camPos.x;
					sLeaser.sprites[killSprite].y = Mathf.Lerp(oracle.oracleBehavior.player.mainBodyChunk.lastPos.y, oracle.oracleBehavior.player.mainBodyChunk.pos.y, timeStacker) - camPos.y;
				}
				float f = Mathf.Lerp((oracle.oracleBehavior as SSOracleBehavior).lastKillFac, (oracle.oracleBehavior as SSOracleBehavior).killFac, timeStacker);
				sLeaser.sprites[killSprite].scale = Mathf.Lerp(200f, 2f, Mathf.Pow(f, 0.5f));
				sLeaser.sprites[killSprite].alpha = Mathf.Pow(f, 3f);
			}
			else
			{
				sLeaser.sprites[killSprite].isVisible = false;
				if (ModManager.MSC)
				{
					if ((oracle.oracleBehavior as SSOracleBehavior).killFacOverseer <= 0f)
					{
						sLeaser.sprites[killSprite].isVisible = false;
					}
					else
					{
						AbstractCreature lockedOverseer = ((oracle.oracleBehavior as SSOracleBehavior).currSubBehavior as SSOracleBehavior.SSOracleMeetPurple).getLockedOverseer();
						if (lockedOverseer == null)
						{
							(oracle.oracleBehavior as SSOracleBehavior).killFacOverseer = 0f;
						}
						else
						{
							sLeaser.sprites[killSprite].isVisible = true;
							sLeaser.sprites[killSprite].x = Mathf.Lerp(lockedOverseer.realizedCreature.mainBodyChunk.lastPos.x, lockedOverseer.realizedCreature.mainBodyChunk.pos.x, timeStacker) - camPos.x;
							sLeaser.sprites[killSprite].y = Mathf.Lerp(lockedOverseer.realizedCreature.mainBodyChunk.lastPos.y, lockedOverseer.realizedCreature.mainBodyChunk.pos.y, timeStacker) - camPos.y;
							float f2 = Mathf.Lerp((oracle.oracleBehavior as SSOracleBehavior).lastKillFacOverseer, (oracle.oracleBehavior as SSOracleBehavior).killFacOverseer, timeStacker);
							sLeaser.sprites[killSprite].scale = Mathf.Lerp(200f, 2f, Mathf.Pow(f2, 0.5f));
							sLeaser.sprites[killSprite].alpha = Mathf.Pow(f2, 3f);
						}
					}
				}
			}
		}
		sLeaser.sprites[fadeSprite].x = vector5.x - camPos.x;
		sLeaser.sprites[fadeSprite].y = vector5.y - camPos.y;
		sLeaser.sprites[neckSprite].x = vector.x - camPos.x;
		sLeaser.sprites[neckSprite].y = vector.y - camPos.y;
		sLeaser.sprites[neckSprite].rotation = Custom.AimFromOneVectorToAnother(vector, vector5);
		sLeaser.sprites[neckSprite].scaleY = Vector2.Distance(vector, vector5);
		if (gown != null)
		{
			gown.DrawSprite(robeSprite, sLeaser, rCam, timeStacker, camPos);
		}
		if (halo != null)
		{
			halo.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		if (armBase != null)
		{
			armBase.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		Vector2 vector6 = Custom.DirVec(vector5, vector);
		Vector2 vector7 = Custom.PerpendicularVector(vector6);
		sLeaser.sprites[HeadSprite].x = vector5.x - camPos.x;
		sLeaser.sprites[HeadSprite].y = vector5.y - camPos.y;
		sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(vector6);
		Vector2 vector8 = RelativeLookDir(timeStacker);
		Vector2 vector9 = Vector2.Lerp(vector5, vector, 0.15f);
		vector9 += vector7 * vector8.x * 2f;
		sLeaser.sprites[ChinSprite].x = vector9.x - camPos.x;
		sLeaser.sprites[ChinSprite].y = vector9.y - camPos.y;
		float num = Mathf.Lerp(lastEyesOpen, eyesOpen, timeStacker);
		for (int k = 0; k < 2; k++)
		{
			float num2 = ((k == 0) ? (-1f) : 1f);
			Vector2 vector10 = vector5 + vector7 * Mathf.Clamp(vector8.x * 3f + 2.5f * num2, -5f, 5f) + vector6 * (1f - vector8.y * 3f);
			sLeaser.sprites[EyeSprite(k)].rotation = Custom.VecToDeg(vector6);
			sLeaser.sprites[EyeSprite(k)].scaleX = 1f + ((k == 0) ? Mathf.InverseLerp(-1f, -0.5f, vector8.x) : Mathf.InverseLerp(1f, 0.5f, vector8.x)) + (1f - num);
			sLeaser.sprites[EyeSprite(k)].scaleY = Mathf.Lerp(1f, IsPebbles ? 2f : 3f, num);
			sLeaser.sprites[EyeSprite(k)].x = vector10.x - camPos.x;
			sLeaser.sprites[EyeSprite(k)].y = vector10.y - camPos.y;
			sLeaser.sprites[EyeSprite(k)].alpha = 0.5f + 0.5f * num;
			int side = ((k < 1 != vector8.x < 0f) ? 1 : 0);
			Vector2 vector11 = vector5 + vector7 * Mathf.Clamp(Mathf.Lerp(7f, 5f, Mathf.Abs(vector8.x)) * num2, -11f, 11f);
			for (int l = 0; l < 2; l++)
			{
				sLeaser.sprites[PhoneSprite(side, l)].rotation = Custom.VecToDeg(vector6);
				sLeaser.sprites[PhoneSprite(side, l)].scaleY = 5.5f * ((l == 0) ? 1f : 0.8f) / 20f;
				sLeaser.sprites[PhoneSprite(side, l)].scaleX = Mathf.Lerp(3.5f, 5f, Mathf.Abs(vector8.x)) * ((l == 0) ? 1f : 0.8f) / 20f;
				sLeaser.sprites[PhoneSprite(side, l)].x = vector11.x - camPos.x;
				sLeaser.sprites[PhoneSprite(side, l)].y = vector11.y - camPos.y;
			}
			sLeaser.sprites[PhoneSprite(side, 2)].x = vector11.x - camPos.x;
			sLeaser.sprites[PhoneSprite(side, 2)].y = vector11.y - camPos.y;
			sLeaser.sprites[PhoneSprite(side, 2)].rotation = Custom.AimFromOneVectorToAnother(vector, vector11 - vector6 * 40f - vector4 * 10f);
			Vector2 vector12 = Vector2.Lerp(hands[k].lastPos, hands[k].pos, timeStacker);
			Vector2 vector13 = vector + vector3 * 4f * ((k == 1) ? (-1f) : 1f);
			if (IsMoon)
			{
				vector13 += vector2 * 3f;
			}
			Vector2 cB = vector12 + Custom.DirVec(vector12, vector13) * 3f + vector2;
			Vector2 cA = vector13 + vector3 * 5f * ((k == 1) ? (-1f) : 1f);
			sLeaser.sprites[HandSprite(k, 0)].x = vector12.x - camPos.x;
			sLeaser.sprites[HandSprite(k, 0)].y = vector12.y - camPos.y;
			Vector2 vector14 = vector13 - vector3 * 2f * ((k == 1) ? (-1f) : 1f);
			float num3 = (IsPebbles ? 4f : 2f);
			for (int m = 0; m < 7; m++)
			{
				float f3 = (float)m / 6f;
				Vector2 vector15 = Custom.Bezier(vector13, cA, vector12, cB, f3);
				Vector2 vector16 = Custom.DirVec(vector14, vector15);
				Vector2 vector17 = Custom.PerpendicularVector(vector16) * ((k == 0) ? (-1f) : 1f);
				float num4 = Vector2.Distance(vector14, vector15);
				(sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4, vector15 - vector16 * num4 * 0.3f - vector17 * num3 - camPos);
				(sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4 + 1, vector15 - vector16 * num4 * 0.3f + vector17 * num3 - camPos);
				(sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4 + 2, vector15 - vector17 * num3 - camPos);
				(sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4 + 3, vector15 + vector17 * num3 - camPos);
				vector14 = vector15;
			}
			vector12 = Vector2.Lerp(feet[k].lastPos, feet[k].pos, timeStacker);
			vector13 = Vector2.Lerp(oracle.bodyChunks[1].lastPos, oracle.bodyChunks[1].pos, timeStacker);
			Vector2 b = Vector2.Lerp(knees[k, 1], knees[k, 0], timeStacker);
			cB = Vector2.Lerp(vector12, b, 0.9f);
			cA = Vector2.Lerp(vector13, b, 0.9f);
			sLeaser.sprites[FootSprite(k, 0)].x = vector12.x - camPos.x;
			sLeaser.sprites[FootSprite(k, 0)].y = vector12.y - camPos.y;
			vector14 = vector13 - vector3 * 2f * ((k == 1) ? (-1f) : 1f);
			num3 = 4f;
			float num5 = 4f;
			for (int n = 0; n < 7; n++)
			{
				float f4 = (float)n / 6f;
				num3 = (IsPebbles ? 2f : Mathf.Lerp(4f, 2f, Mathf.Pow(f4, 0.5f)));
				Vector2 vector18 = Custom.Bezier(vector13, cA, vector12, cB, f4);
				Vector2 vector19 = Custom.DirVec(vector14, vector18);
				Vector2 vector20 = Custom.PerpendicularVector(vector19) * ((k == 0) ? (-1f) : 1f);
				float num6 = Vector2.Distance(vector14, vector18);
				(sLeaser.sprites[FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4, vector18 - vector19 * num6 * 0.3f - vector20 * (num5 + num3) * 0.5f - camPos);
				(sLeaser.sprites[FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4 + 1, vector18 - vector19 * num6 * 0.3f + vector20 * (num5 + num3) * 0.5f - camPos);
				(sLeaser.sprites[FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4 + 2, vector18 - vector20 * num3 - camPos);
				(sLeaser.sprites[FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4 + 3, vector18 + vector20 * num3 - camPos);
				vector14 = vector18;
				num5 = num3;
			}
		}
		if (IsMoon || IsPastMoon || IsStraw)
		{
			Vector2 p = vector5 + vector7 * vector8.x * 2.5f + vector6 * (-2f - vector8.y * 1.5f);
			sLeaser.sprites[MoonThirdEyeSprite].x = p.x - camPos.x;
			sLeaser.sprites[MoonThirdEyeSprite].y = p.y - camPos.y;
			sLeaser.sprites[MoonThirdEyeSprite].rotation = Custom.AimFromOneVectorToAnother(p, vector5 - vector6 * 10f);
			if (IsStraw)
			{
				sLeaser.sprites[MoonThirdEyeSprite].scaleX = Mathf.Lerp(0.8f, 0.6f, Mathf.Abs(vector8.x));
				sLeaser.sprites[MoonThirdEyeSprite].scaleY = Custom.LerpMap(vector8.y, 0f, 1f, 0.8f, 0.2f);
			}
			else
			{
				sLeaser.sprites[MoonThirdEyeSprite].scaleX = Mathf.Lerp(0.2f, 0.15f, Mathf.Abs(vector8.x));
				sLeaser.sprites[MoonThirdEyeSprite].scaleY = Custom.LerpMap(vector8.y, 0f, 1f, 0.2f, 0.05f);
			}
		}
		if (IsPastMoon)
		{
			sLeaser.sprites[MoonSigilSprite].x = sLeaser.sprites[MoonThirdEyeSprite].x;
			sLeaser.sprites[MoonSigilSprite].y = sLeaser.sprites[MoonThirdEyeSprite].y + 10f;
			SSOracleBehavior sSOracleBehavior = oracle.oracleBehavior as SSOracleBehavior;
			if (!oracle.Consious)
			{
				sLeaser.sprites[MoonSigilSprite].alpha = 0f;
			}
			else if (sSOracleBehavior.working > 0.9f && sSOracleBehavior.pearlConversation == null && sSOracleBehavior.conversation == null)
			{
				sLeaser.sprites[MoonSigilSprite].alpha = Mathf.Lerp(sLeaser.sprites[MoonSigilSprite].alpha, 1f, 0.02f);
			}
			else if (sSOracleBehavior.pearlConversation != null || sSOracleBehavior.conversation != null)
			{
				sLeaser.sprites[MoonSigilSprite].alpha = 0.25f + UnityEngine.Random.value / 3f;
			}
			else
			{
				sLeaser.sprites[MoonSigilSprite].alpha = Mathf.Lerp(sLeaser.sprites[MoonSigilSprite].alpha, 0f, 0.05f);
			}
		}
		if (umbCord != null)
		{
			umbCord.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		else if (discUmbCord != null)
		{
			discUmbCord.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		SLArmBaseColA = new Color(0.52156866f, 0.52156866f, 0.5137255f);
		SLArmHighLightColA = new Color(29f / 51f, 29f / 51f, 28f / 51f);
		SLArmBaseColB = palette.texture.GetPixel(5, 1);
		SLArmHighLightColB = palette.texture.GetPixel(5, 2);
		for (int i = 0; i < armJointGraphics.Length; i++)
		{
			armJointGraphics[i].ApplyPalette(sLeaser, rCam, palette);
		}
		Color color = ((IsPebbles || IsSaintPebbles) ? new Color(1f, 0.4f, 0.79607844f) : (IsMoon ? new Color(9f / 85f, 23f / 85f, 29f / 85f) : ((!IsPastMoon) ? new Color(0.89f, 0.89f, 0.79f) : new Color(0.13f, 0.53f, 0.69f))));
		for (int j = 0; j < base.owner.bodyChunks.Length; j++)
		{
			sLeaser.sprites[firstBodyChunkSprite + j].color = color;
		}
		sLeaser.sprites[neckSprite].color = color;
		sLeaser.sprites[HeadSprite].color = color;
		sLeaser.sprites[ChinSprite].color = color;
		for (int k = 0; k < 2; k++)
		{
			if (armJointGraphics.Length == 0)
			{
				sLeaser.sprites[PhoneSprite(k, 0)].color = GenericJointBaseColor();
				sLeaser.sprites[PhoneSprite(k, 1)].color = GenericJointHighLightColor();
				sLeaser.sprites[PhoneSprite(k, 2)].color = GenericJointHighLightColor();
			}
			else
			{
				sLeaser.sprites[PhoneSprite(k, 0)].color = armJointGraphics[0].BaseColor(default(Vector2));
				sLeaser.sprites[PhoneSprite(k, 1)].color = armJointGraphics[0].HighLightColor(default(Vector2));
				sLeaser.sprites[PhoneSprite(k, 2)].color = armJointGraphics[0].HighLightColor(default(Vector2));
			}
			sLeaser.sprites[HandSprite(k, 0)].color = color;
			if (gown != null)
			{
				for (int l = 0; l < 7; l++)
				{
					(sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4] = gown.Color(0.4f);
					(sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 1] = gown.Color(0f);
					(sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 2] = gown.Color(0.4f);
					(sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 3] = gown.Color(0f);
				}
			}
			else
			{
				sLeaser.sprites[HandSprite(k, 1)].color = color;
			}
			sLeaser.sprites[FootSprite(k, 0)].color = color;
			sLeaser.sprites[FootSprite(k, 1)].color = color;
		}
		if (umbCord != null)
		{
			umbCord.ApplyPalette(sLeaser, rCam, palette);
			sLeaser.sprites[firstUmbilicalSprite].color = palette.blackColor;
		}
		else if (discUmbCord != null)
		{
			discUmbCord.ApplyPalette(sLeaser, rCam, palette);
		}
		if (armBase != null)
		{
			armBase.ApplyPalette(sLeaser, rCam, palette);
		}
		if (IsMoon)
		{
			sLeaser.sprites[MoonThirdEyeSprite].color = Color.Lerp(new Color(1f, 0f, 1f), color, 0.5f);
		}
		if (IsPastMoon)
		{
			sLeaser.sprites[MoonThirdEyeSprite].color = Color.Lerp(new Color(1f, 0f, 1f), color, 0.3f);
			sLeaser.sprites[MoonSigilSprite].color = new Color(0.12156863f, 0.28627452f, 41f / 85f);
		}
		if (IsStraw)
		{
			sLeaser.sprites[MoonThirdEyeSprite].color = Color.Lerp(new Color(0.5f, 0.4f, 0.1f), color, 0.5f);
		}
	}

	public Color GenericJointBaseColor()
	{
		if (IsMoon)
		{
			return Color.Lerp(SLArmBaseColA, SLArmBaseColB, 0.75f);
		}
		return Color.Lerp(Custom.HSL2RGB(0.025f, Mathf.Lerp(0.4f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.05f, 0.7f, Mathf.Pow(1f, 0.45f))), new Color(0f, 0f, 0.1f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.5f);
	}

	public Color GenericJointHighLightColor()
	{
		if (IsMoon)
		{
			return Color.Lerp(SLArmHighLightColA, SLArmHighLightColB, 0.75f);
		}
		return Color.Lerp(Custom.HSL2RGB(0.025f, Mathf.Lerp(0.5f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.15f, 0.85f, Mathf.Pow(1f, 0.45f))), new Color(0f, 0f, 0.15f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.4f);
	}
}
