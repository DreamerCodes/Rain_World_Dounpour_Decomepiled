using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace VoidSea;

public class VoidWorm : VoidSeaScene.VoidSeaSceneElement
{
	public class Arm
	{
		public VoidWorm owner;

		public int firstSprite;

		public int totalSprites;

		public Vector2[,] segments;

		public Vector2 goalPoint;

		public float goToGoal;

		private float segLength;

		private float side;

		public int index;

		private Vector2 driftDir;

		public float[,] fingers;

		public Vector2[,] thread;

		public int threadStatus = -1;

		public float threadStretched;

		public Arm(VoidWorm owner, int firstSprite, float totalLength, float side, int index, bool hasThread)
		{
			this.firstSprite = firstSprite;
			this.owner = owner;
			this.side = side;
			this.index = index;
			segments = new Vector2[(int)(totalLength / (Mathf.Lerp(25f, 6f, Mathf.Pow(owner.graphicFidelity, 2f)) * owner.scale)), 3];
			segLength = totalLength / (float)segments.GetLength(0);
			totalSprites = 2;
			if (owner.graphicFidelity > 0.8f)
			{
				totalSprites += 2;
				fingers = new float[2, 3];
				for (int i = 0; i < fingers.GetLength(0); i++)
				{
					fingers[i, 2] = UnityEngine.Random.value;
				}
			}
			driftDir = Custom.RNV();
			if (hasThread)
			{
				thread = new Vector2[40, 3];
				totalSprites++;
			}
		}

		public void Update()
		{
			if (fingers != null)
			{
				float num = 0f;
				for (int i = 0; i < fingers.GetLength(0); i++)
				{
					fingers[i, 1] = fingers[i, 0];
					fingers[i, 0] = Custom.LerpAndTick(fingers[i, 0], fingers[i, 2], 0.05f, 1f / 30f);
					if (UnityEngine.Random.value < 0.025f)
					{
						fingers[i, 2] = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
					}
					num += fingers[i, 0];
				}
				num /= (float)fingers.GetLength(0);
				for (int j = 0; j < fingers.GetLength(0); j++)
				{
					fingers[j, 0] = Custom.LerpAndTick(fingers[j, 0], num, 0.05f, 1f / 30f);
				}
			}
			Player player = owner.voidSea.room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = owner.voidSea.room.game.RealizedPlayerFollowedByCamera;
			}
			if (thread != null)
			{
				Vector2 vector = Custom.DirVec(segments[segments.GetLength(0) - 2, 0], segments[segments.GetLength(0) - 1, 0]);
				vector -= Custom.PerpendicularVector(vector) * side * 1.2f;
				vector.Normalize();
				for (int k = 0; k < thread.GetLength(0); k++)
				{
					thread[k, 1] = thread[k, 0];
					if (threadStatus > -1)
					{
						thread[k, 0] += thread[k, 2];
						thread[k, 2] *= Custom.LerpMap(thread[k, 2].magnitude, 2f, 9f, 1f, 0.95f);
						if (k > 0)
						{
							Vector2 vector2 = (thread[k - 1, 0] - thread[k, 0]).normalized * (Vector2.Distance(thread[k - 1, 0], thread[k, 0]) - 30f);
							thread[k, 0] += vector2 * 0.5f;
							thread[k, 2] += vector2 * 0.5f;
							thread[k - 1, 0] -= vector2 * 0.5f;
							thread[k - 1, 2] -= vector2 * 0.5f;
						}
						else
						{
							Vector2 vector3 = segments[segments.GetLength(0) - 1, 0] + Custom.DirVec(segments[segments.GetLength(0) - 2, 0], segments[segments.GetLength(0) - 1, 0]) * owner.scale * 2f;
							Vector2 vector4 = (vector3 - thread[k, 0]).normalized * (Vector2.Distance(thread[k, 0], vector3) - 30f);
							thread[k, 0] += vector4;
							thread[k, 2] += vector4;
						}
						if (k > 1)
						{
							Vector2 vector5 = Custom.DirVec(thread[k - 2, 0], thread[k, 0]) * Mathf.InverseLerp(60f, 20f, Vector2.Distance(thread[k - 2, 0], thread[k, 0]));
							thread[k, 2] += vector5 * 0.4f;
							thread[k - 2, 2] -= vector5 * 0.4f;
						}
						if (k < 4)
						{
							thread[k, 2] += vector * Mathf.InverseLerp(3f, 0f, k) * 6f;
						}
						if (threadStretched > 0f && owner.voidSea.room.game.Players.Count > 0)
						{
							thread[k, 0] = Vector2.Lerp(thread[k, 0], Vector2.Lerp(segments[segments.GetLength(0) - 1, 0], player.mainBodyChunk.pos, (float)k / (float)(segments.GetLength(0) - 1)), threadStretched);
						}
					}
					else
					{
						thread[k, 0] = segments[segments.GetLength(0) - 1, 0];
						thread[k, 2] *= 0f;
					}
				}
				if (threadStatus > 0 && player != null && player.room == owner.room)
				{
					thread[thread.GetLength(0) - 1, 0] = player.mainBodyChunk.pos;
					thread[thread.GetLength(0) - 1, 2] *= 0f;
					float num2 = Vector2.Distance(segments[segments.GetLength(0) - 1, 0], player.mainBodyChunk.pos);
					if (num2 > (float)thread.GetLength(0) * 30f)
					{
						Vector2 vector6 = (segments[segments.GetLength(0) - 1, 0] - player.mainBodyChunk.pos).normalized * (num2 - (float)thread.GetLength(0) * 30f);
						if (vector6.magnitude > 100f)
						{
							player.Stun(12);
						}
						owner.voidSea.room.ScreenMovement(null, default(Vector2), Mathf.InverseLerp(60f, 140f, vector6.magnitude) * 5f);
						player.mainBodyChunk.pos += vector6 * 0.8f;
						player.mainBodyChunk.vel += vector6 * 0.8f;
						segments[segments.GetLength(0) - 1, 0] -= vector6 * 0.1f;
						segments[segments.GetLength(0) - 1, 2] -= vector6 * 0.1f;
					}
					threadStretched = Mathf.InverseLerp((float)thread.GetLength(0) * 30f, (float)thread.GetLength(0) * 30f + 100f, num2);
				}
				else
				{
					threadStretched = 0f;
				}
			}
			Vector2 vector7 = Custom.DirVec(owner.chunks[1].pos, owner.chunks[0].pos);
			vector7 += Custom.PerpendicularVector(vector7) * side * Custom.LerpMap(index, 0f, 3f, 0f, 2.9f, 1.5f);
			vector7.Normalize();
			if (UnityEngine.Random.value < 0.1f)
			{
				driftDir = (driftDir + Custom.RNV() * UnityEngine.Random.value * 0.7f).normalized;
			}
			for (int l = 0; l < segments.GetLength(0); l++)
			{
				float num3 = (float)l / (float)(segments.GetLength(0) - 1);
				segments[l, 1] = segments[l, 0];
				segments[l, 0] += segments[l, 2];
				segments[l, 2] -= owner.chunks[0].vel;
				segments[l, 2] *= Mathf.Lerp(0.6f, 1f, Mathf.Pow(num3, 0.25f));
				segments[l, 2] += owner.chunks[0].vel;
				segments[l, 2] *= Mathf.Lerp(1f, 0.6f, Mathf.Pow(num3, 0.25f));
				segments[l, 2] += owner.chunks[0].vel * Mathf.InverseLerp(0.3f, 0f, Mathf.Pow(num3, 0.5f)) * 0.4f;
				if (goToGoal > 0f)
				{
					segments[l, 2] += Custom.DirVec(segments[l, 0], goalPoint) * Mathf.Lerp(-1f, 1f, Mathf.Pow(num3, 0.5f)) * num3 * 2.6f * owner.scale * Mathf.InverseLerp(0f, 40f, Vector2.Distance(segments[segments.GetLength(0) - 1, 0], goalPoint)) * goToGoal;
					if (goToGoal > 0.9f)
					{
						segments[l, 0] = Vector2.Lerp(segments[l, 0], goalPoint, Mathf.InverseLerp(0.9f, 1f, goToGoal) * Mathf.Pow(num3, 2f) * 0.5f);
					}
				}
				else if (goToGoal < 0f)
				{
					segments[l, 2] += Custom.DirVec(segments[l, 0], goalPoint) * 2.6f * owner.scale * num3 * Mathf.InverseLerp(600f, 140f, Vector2.Distance(segments[l, 0], goalPoint)) * goToGoal;
				}
				segments[l, 2] += driftDir * 0.35f * owner.scale * num3 * (1f - Mathf.Abs(goToGoal));
				segments[l, 2] += vector7 * Mathf.Pow(1f - num3, 1.5f) * 2.1f * owner.scale;
				if (l > 2)
				{
					segments[l, 2] += (Vector2)Vector3.Slerp(Custom.PerpendicularVector(vector7), Custom.PerpendicularVector(segments[l, 0], segments[l - 2, 0]), Mathf.Lerp(1f, 0.2f, num3)) * side * Mathf.Sin(num3 * (float)Math.PI * 1.3f) * Mathf.Pow(num3, 0.8f) * Custom.LerpMap(index, 0f, 3f, Mathf.Lerp(0.8f, 0.3f, num3), 0.3f, 0.5f) * owner.scale;
					segments[l, 2] += Custom.DirVec(segments[l - 2, 0], segments[l, 0]) * 0.2f * owner.scale;
				}
				if (l > 0)
				{
					Vector2 vector8 = (segments[l - 1, 0] - segments[l, 0]).normalized * (Vector2.Distance(segments[l - 1, 0], segments[l, 0]) - segLength);
					float num4 = 0.7f;
					segments[l, 0] += vector8 * num4;
					segments[l, 2] += vector8 * num4;
					segments[l - 1, 0] -= vector8 * (1f - num4);
					segments[l - 1, 2] -= vector8 * (1f - num4);
				}
				else
				{
					Vector2 vector9 = owner.chunks[0].pos + Custom.DirVec(owner.chunks[0].pos, owner.chunks[1].pos) * Custom.LerpMap(index, 0f, 3f, 7.5f, 20f, 2f) * owner.scale + Custom.PerpendicularVector(Custom.DirVec(owner.chunks[1].pos, owner.chunks[0].pos)) * side * Custom.LerpMap(index, 0f, 3f, 7.5f, 30f, 1.5f) * owner.scale;
					Vector2 vector10 = (vector9 - segments[l, 0]).normalized * (Vector2.Distance(segments[l, 0], vector9) - segLength);
					segments[l, 0] += vector10;
					segments[l, 2] += vector10;
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: true, "wormSkin");
			sLeaser.sprites[firstSprite].shader = rCam.game.rainWorld.Shaders["VoidWormPincher"];
			sLeaser.sprites[firstSprite + 1] = new FSprite("Cicada5body");
			sLeaser.sprites[firstSprite + 1].anchorX = 0.1f;
			if (fingers != null)
			{
				sLeaser.sprites[firstSprite + 2] = new FSprite("SpiderLeg1B");
				sLeaser.sprites[firstSprite + 2].anchorY = 0.05f;
				sLeaser.sprites[firstSprite + 2].scaleX = side * owner.scale * (1f / (8f * owner.depth));
				sLeaser.sprites[firstSprite + 2].scaleY = owner.scale * (1f / (8f * owner.depth));
				sLeaser.sprites[firstSprite + 3] = new FSprite("SpiderLeg2B");
				sLeaser.sprites[firstSprite + 3].anchorY = 0.05f;
				sLeaser.sprites[firstSprite + 3].scaleX = side * owner.scale * (1f / (8f * owner.depth));
				sLeaser.sprites[firstSprite + 3].scaleY = owner.scale * (1f / (8f * owner.depth));
			}
			for (int i = 0; i < (sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors.Length; i++)
			{
				float value = (float)i / (float)((sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors.Length - 1);
				(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(new Color(0.85f, 0.85f, 0.85f), new Color(1f, 0.5f, 0.5f), Mathf.Pow(Mathf.InverseLerp(0.05f, Custom.LerpMap(index, 0f, 3f, 0.2f, 0.5f), value), 0.5f));
			}
			if (thread != null)
			{
				sLeaser.sprites[firstSprite + 4] = TriangleMesh.MakeLongMesh(thread.GetLength(0), pointyTip: false, customColor: false);
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = 0.5f + 0.5f * Mathf.Pow(Mathf.InverseLerp(3f, 0f, index), 0.5f);
			Vector2 vector = (Vector2.Lerp(owner.chunks[0].lastPos, owner.chunks[0].pos, timeStacker) + Vector2.Lerp(owner.chunks[1].lastPos, owner.chunks[1].pos, timeStacker)) / 2f - camPos;
			vector = (vector - owner.voidSea.convergencePoint) / owner.depth + owner.voidSea.convergencePoint;
			float num2 = 6f * num * owner.scale / owner.depth;
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				float num3 = (float)i / (float)(segments.GetLength(0) - 1);
				Vector2 vector2 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker) - camPos;
				vector2 = (vector2 - owner.voidSea.convergencePoint) / owner.depth + owner.voidSea.convergencePoint;
				Vector2 normalized = (vector - vector2).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num4 = (Mathf.Lerp(6.75f, 3f, num3) - Mathf.Sin(num3 * (float)Math.PI) * 2f) * num * 0.4f * owner.scale / owner.depth;
				if (num3 == 1f)
				{
					num4 *= 0.5f;
				}
				float num5 = Vector2.Distance(vector, vector2) / 5f;
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector - normalized * num5 - vector3 * (num4 + num2) * 0.5f);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector - normalized * num5 + vector3 * (num4 + num2) * 0.5f);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 + normalized * num5 - vector3 * num4);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized * num5 + vector3 * num4);
				vector = vector2;
				num2 = num4;
			}
			Vector2 vector4 = Vector2.Lerp(segments[segments.GetLength(0) - 2, 1], segments[segments.GetLength(0) - 2, 0], timeStacker);
			Vector2 vector5 = Custom.DirVec(vector4, Vector2.Lerp(segments[segments.GetLength(0) - 1, 1], segments[segments.GetLength(0) - 1, 0], timeStacker));
			vector4 += vector5 * 2f * owner.scale;
			vector4 -= camPos;
			vector4 = (vector4 - owner.voidSea.convergencePoint) / owner.depth + owner.voidSea.convergencePoint;
			sLeaser.sprites[firstSprite + 1].scaleX = 1f / owner.depth;
			sLeaser.sprites[firstSprite + 1].scaleY = side / owner.depth;
			sLeaser.sprites[firstSprite + 1].x = vector4.x;
			sLeaser.sprites[firstSprite + 1].y = vector4.y;
			sLeaser.sprites[firstSprite + 1].rotation = Custom.VecToDeg(vector5) - 90f;
			if (fingers != null)
			{
				Vector2 vector6 = vector4 + vector5 * 41f / owner.depth + Custom.PerpendicularVector(vector5) * (0f - side) * 6f / owner.depth;
				for (int j = 0; j < fingers.GetLength(0); j++)
				{
					sLeaser.sprites[firstSprite + 2 + j].x = vector6.x;
					sLeaser.sprites[firstSprite + 2 + j].y = vector6.y;
					sLeaser.sprites[firstSprite + 2 + j].rotation = Custom.VecToDeg(vector5) + Mathf.Lerp(40f, 160f, Mathf.Lerp(fingers[j, 1], fingers[j, 0], timeStacker)) * side;
				}
			}
			if (thread == null)
			{
				return;
			}
			if (threadStatus > -1)
			{
				sLeaser.sprites[firstSprite + 4].isVisible = true;
				vector = vector4 + vector5 * 4f * owner.scale / owner.depth;
				for (int k = 0; k < thread.GetLength(0); k++)
				{
					Vector2 vector7 = Vector2.Lerp(thread[k, 1], thread[k, 0], timeStacker) - camPos;
					vector7 = (vector7 - owner.voidSea.convergencePoint) / owner.depth + owner.voidSea.convergencePoint;
					if (k == 0)
					{
						vector7 = Vector2.Lerp(vector7, vector, 0.8f);
					}
					Vector2 normalized2 = (vector - vector7).normalized;
					Vector2 vector8 = Custom.PerpendicularVector(normalized2);
					float num6 = Vector2.Distance(vector, vector7) / 5f;
					(sLeaser.sprites[firstSprite + 4] as TriangleMesh).MoveVertice(k * 4, vector - normalized2 * num6 - vector8 * 0.5f);
					(sLeaser.sprites[firstSprite + 4] as TriangleMesh).MoveVertice(k * 4 + 1, vector - normalized2 * num6 + vector8 * 0.5f);
					(sLeaser.sprites[firstSprite + 4] as TriangleMesh).MoveVertice(k * 4 + 2, vector7 + normalized2 * num6 - vector8 * 0.5f);
					(sLeaser.sprites[firstSprite + 4] as TriangleMesh).MoveVertice(k * 4 + 3, vector7 + normalized2 * num6 + vector8 * 0.5f);
					vector = vector7;
				}
			}
			else
			{
				sLeaser.sprites[firstSprite + 4].isVisible = false;
			}
		}
	}

	public class Head
	{
		public VoidWorm owner;

		public int firstSprite;

		public int totalSprites;

		public Vector2[,] neck;

		public Vector2 lookAtPoint;

		public int NeckSprite => firstSprite;

		public int HeadSprite(int part)
		{
			return firstSprite + 1 + part;
		}

		public int EyeSprite(int eye, int part)
		{
			return firstSprite + 3 + part + eye * 4;
		}

		public Vector2 HeadPos(float timeStacker)
		{
			return Vector2.Lerp(neck[neck.GetLength(0) - 1, 1], neck[neck.GetLength(0) - 1, 0], timeStacker);
		}

		public Head(VoidWorm owner, int firstSprite)
		{
			this.firstSprite = firstSprite;
			this.owner = owner;
			neck = new Vector2[10, 3];
			totalSprites = 11;
		}

		public void Update()
		{
			Player player = owner.voidSea.room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = owner.voidSea.room.game.RealizedPlayerFollowedByCamera;
			}
			if (player != null)
			{
				lookAtPoint = player.mainBodyChunk.pos;
			}
			for (int i = 0; i < neck.GetLength(0); i++)
			{
				float num = (float)i / (float)(neck.GetLength(0) - 1);
				neck[i, 1] = neck[i, 0];
				neck[i, 0] += neck[i, 2];
				neck[i, 2] -= owner.chunks[0].vel;
				neck[i, 2] *= 0.6f;
				neck[i, 2] += owner.chunks[0].vel;
				neck[i, 2] += Custom.DirVec(owner.chunks[1].pos, owner.chunks[0].pos) * Mathf.Pow(1f - num, 1.5f) * 2f * owner.scale;
				neck[i, 2] += Custom.DirVec(neck[i, 0], lookAtPoint) * Mathf.Lerp(-1f, 1f, num) * num * 2f * owner.scale;
				neck[i, 2] += Custom.DirVec(neck[i, 0], lookAtPoint) * Mathf.InverseLerp(200f, 50f, Vector2.Distance(neck[i, 0], lookAtPoint)) * -2.8f * num * owner.scale;
				neck[0, 2] += Custom.DirVec(owner.chunks[1].pos, owner.chunks[0].pos) * owner.chunks[0].vel.magnitude * 40f;
				if (i > 2)
				{
					neck[i, 2] += Custom.DirVec(neck[i - 2, 0], neck[i, 0]) * 0.4f * owner.scale;
				}
				if (i > 0)
				{
					Vector2 vector = (neck[i - 1, 0] - neck[i, 0]).normalized * (Vector2.Distance(neck[i - 1, 0], neck[i, 0]) - 10f * owner.scale);
					float num2 = 0.7f;
					neck[i, 0] += vector * num2;
					neck[i, 2] += vector * num2;
					neck[i - 1, 0] -= vector * (1f - num2);
					neck[i - 1, 2] -= vector * (1f - num2);
				}
				else
				{
					Vector2 vector2 = owner.chunks[0].pos + Custom.DirVec(owner.chunks[0].pos, owner.chunks[1].pos) * 10f * owner.scale;
					Vector2 vector3 = (vector2 - neck[i, 0]).normalized * (Vector2.Distance(neck[i, 0], vector2) - 10f * owner.scale);
					neck[i, 0] += vector3;
					neck[i, 2] += vector3;
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[NeckSprite] = TriangleMesh.MakeLongMesh(neck.GetLength(0), pointyTip: false, customColor: true, "wormSkin");
			sLeaser.sprites[NeckSprite].shader = rCam.game.rainWorld.Shaders["VoidWormPincher"];
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[HeadSprite(i)] = new FSprite("Futile_White");
				sLeaser.sprites[HeadSprite(i)].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
				sLeaser.sprites[HeadSprite(i)].alpha = 0.9f;
			}
			sLeaser.sprites[HeadSprite(0)].color = new Color(1f, 1f, 1f) * 0.9f;
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[EyeSprite(j, 0)] = new FSprite("pixel");
				sLeaser.sprites[EyeSprite(j, 0)].color = new Color(0f, 0f, 0.003921569f, 0.5f);
				for (int k = 0; k < 2; k++)
				{
					sLeaser.sprites[EyeSprite(j, k + 1)] = new FSprite("Circle20");
					sLeaser.sprites[EyeSprite(j, k + 1)].color = new Color((k == 0) ? 0f : 0.5f, (k == 0) ? 0f : 0.5f, (k == 0) ? 0f : 0.5f);
				}
				sLeaser.sprites[EyeSprite(j, 3)] = new FSprite("Futile_White");
				sLeaser.sprites[EyeSprite(j, 3)].shader = rCam.game.rainWorld.Shaders["FlatLight"];
				sLeaser.sprites[EyeSprite(j, 3)].color = new Color(0.529f, 0.365f, 0.184f);
			}
			for (int l = 0; l < (sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors.Length; l++)
			{
				float num = (float)l / (float)((sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors.Length - 1);
				(sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors[l] = Color.Lerp(new Color(0.85f, 0.85f, 0.85f), new Color(1f, 0.5f, 0.5f), Mathf.Sin(num * (float)Math.PI));
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = HeadPos(timeStacker) - camPos;
			vector = (vector - owner.voidSea.convergencePoint) / owner.depth + owner.voidSea.convergencePoint;
			Vector2 vector2 = Custom.DirVec(HeadPos(timeStacker), lookAtPoint);
			Vector2 vector3 = Custom.DirVec(Vector2.Lerp(owner.chunks[1].lastPos, owner.chunks[1].pos, timeStacker), Vector2.Lerp(owner.chunks[0].lastPos, owner.chunks[0].pos, timeStacker));
			float num = Custom.VecToDeg(-vector3);
			float num2 = Mathf.InverseLerp(-0.5f, -1f, Vector2.Dot(vector2, vector3));
			float num3 = 0f;
			num2 *= 1f - num3;
			Vector2 normalized = Vector2.Lerp(vector2.normalized, -Custom.DegToVec(0f - num), Mathf.Lerp(0.5f, 1f, Mathf.Max(Mathf.Pow(num2, 1.1f), num3))).normalized;
			Vector2 vector4 = Custom.RotateAroundOrigo(normalized.normalized, num);
			vector2.Normalize();
			normalized.Normalize();
			float num4 = 0.34f * owner.scale / owner.depth;
			Vector2 vector5 = Vector3.Slerp(normalized.normalized, -vector2.normalized, num2);
			Vector2 vector6 = vector + vector5 * (4f - Mathf.Max(10f * Mathf.Pow(num2, 0.5f), 3f * num3)) * num4;
			Vector2 p = vector6 + vector5 * 60f;
			for (int i = 0; i < 2; i++)
			{
				float value = ((i == 0) ? (-1f) : 1f) * 0.5f + vector4.x * (1f - num3);
				value = Mathf.Clamp(value, -1f, 1f);
				Vector2 p2 = vector6 + Custom.PerpendicularVector(vector5) * 8f * num4 * value;
				float num5 = Custom.AimFromOneVectorToAnother(p2, p);
				float num6 = Mathf.Lerp(1.5f, 2f, Mathf.Pow(num2, 1.5f)) * 1.5f * (1f + 0.2f * (1f - num2)) * Mathf.InverseLerp(1f, 0.7f, Mathf.Abs(value)) * 0.8f * num4;
				float num7 = Mathf.Lerp(2.5f, 1.5f, Mathf.Pow(num2, 0.5f)) * 1.5f * num4;
				Vector2 vec = -vector2;
				vec = Custom.RotateAroundOrigo(vec, num5);
				vec.x *= num6 * 0.19999999f;
				vec.y *= num7 * 0.19999999f;
				vec = Custom.RotateAroundOrigo(vec, 0f - num5);
				sLeaser.sprites[EyeSprite(i, 0)].x = p2.x;
				sLeaser.sprites[EyeSprite(i, 0)].y = p2.y;
				sLeaser.sprites[EyeSprite(i, 0)].rotation = num5;
				sLeaser.sprites[EyeSprite(i, 0)].scaleX = Mathf.Min(1f, num6 * 6f) * 3f;
				sLeaser.sprites[EyeSprite(i, 0)].scaleY = num7 * 3f;
				sLeaser.sprites[EyeSprite(i, 1)].x = p2.x;
				sLeaser.sprites[EyeSprite(i, 1)].y = p2.y;
				sLeaser.sprites[EyeSprite(i, 1)].scaleX = num6 * 0.1f;
				sLeaser.sprites[EyeSprite(i, 1)].scaleY = num7 * 0.1f;
				sLeaser.sprites[EyeSprite(i, 1)].rotation = num5;
				sLeaser.sprites[EyeSprite(i, 2)].x = p2.x + vec.x;
				sLeaser.sprites[EyeSprite(i, 2)].y = p2.y + vec.y;
				sLeaser.sprites[EyeSprite(i, 2)].scaleX = num6 * 0.08f;
				sLeaser.sprites[EyeSprite(i, 2)].scaleY = num7 * 0.08f;
				sLeaser.sprites[EyeSprite(i, 2)].rotation = num5;
				sLeaser.sprites[EyeSprite(i, 3)].x = p2.x + vec.x * 0.5f;
				sLeaser.sprites[EyeSprite(i, 3)].y = p2.y + vec.y * 0.5f;
				sLeaser.sprites[EyeSprite(i, 3)].scaleX = Mathf.Lerp(Mathf.Min(num6, num7), num6, 0.5f) * 0.35f;
				sLeaser.sprites[EyeSprite(i, 3)].scaleY = Mathf.Lerp(Mathf.Min(num6, num7), num7, 0.5f) * 0.35f;
				sLeaser.sprites[EyeSprite(i, 3)].rotation = num5;
			}
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[HeadSprite(j)].rotation = Custom.VecToDeg(normalized);
				sLeaser.sprites[HeadSprite(j)].x = vector.x;
				sLeaser.sprites[HeadSprite(j)].y = vector.y;
				sLeaser.sprites[HeadSprite(j)].scaleX = Mathf.Lerp(8f, 9f, Mathf.Pow(num2, 0.5f)) * 1.25f * ((j == 0) ? 1f : 0.8f) * num4 / 10f;
				sLeaser.sprites[HeadSprite(j)].scaleY = Mathf.Lerp(11f, 8f, Mathf.Pow(num2, 0.5f)) * 1.25f * ((j == 0) ? 1f : 0.8f) * num4 / 10f;
			}
			Vector2 vector7 = (Vector2.Lerp(owner.chunks[0].lastPos, owner.chunks[0].pos, timeStacker) + Vector2.Lerp(owner.chunks[1].lastPos, owner.chunks[1].pos, timeStacker)) / 2f - camPos;
			vector7 = (vector7 - owner.voidSea.convergencePoint) / owner.depth + owner.voidSea.convergencePoint;
			float num8 = 12f * owner.scale / owner.depth;
			for (int k = 0; k < neck.GetLength(0); k++)
			{
				float num9 = (float)k / (float)(neck.GetLength(0) - 1);
				Vector2 vector8 = Vector2.Lerp(neck[k, 1], neck[k, 0], timeStacker) - camPos;
				vector8 -= normalized * Mathf.Sin(Mathf.InverseLerp(0.5f, 1f, num9) * (float)Math.PI) * Mathf.Lerp(1f, -1f, num2) * 4f * owner.scale;
				vector8 = (vector8 - owner.voidSea.convergencePoint) / owner.depth + owner.voidSea.convergencePoint;
				Vector2 normalized2 = (vector7 - vector8).normalized;
				Vector2 vector9 = Custom.PerpendicularVector(normalized2);
				float num10 = (Mathf.Lerp(7.5f, 4f, num9) - Mathf.Sin(num9 * (float)Math.PI) * 1.5f) * 0.4f * owner.scale / owner.depth;
				float num11 = Vector2.Distance(vector7, vector8) / 5f;
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(k * 4, vector7 - normalized2 * num11 - vector9 * (num10 + num8) * 0.5f);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(k * 4 + 1, vector7 - normalized2 * num11 + vector9 * (num10 + num8) * 0.5f);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(k * 4 + 2, vector8 + normalized2 * num11 - vector9 * num10);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(k * 4 + 3, vector8 + normalized2 * num11 + vector9 * num10);
				vector7 = vector8;
				num8 = num10;
			}
		}
	}

	public class Scales
	{
		public VoidWorm owner;

		public int firstSprite;

		public int totalSprites;

		public int index;

		public Vector2[] scalePositions;

		public float[,] scalesData;

		public int ScaleSprite(int sc, int pt)
		{
			return sc + pt * scalePositions.Length;
		}

		public Scales(VoidWorm owner, int firstSprite, int index)
		{
			this.firstSprite = firstSprite;
			this.owner = owner;
			this.index = index;
			scalePositions = new Vector2[(index == 0) ? 28 : 18];
			scalesData = new float[scalePositions.Length, 3];
			for (int i = 0; i < scalePositions.Length; i++)
			{
				float num = (float)i / (float)(scalePositions.Length - 1);
				float num2 = Mathf.Sin(num * (float)Math.PI);
				scalePositions[i] = Custom.DegToVec(Mathf.Lerp(-90f, 90f, num) + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Pow(UnityEngine.Random.value, 2f) * 10f * num2) * Mathf.Lerp(0.8f, 1.2f, Mathf.Lerp(0.7f, UnityEngine.Random.value, num2));
				scalePositions[i].y *= ((index == 0) ? 0.425f : 0.6f);
				scalesData[i, 0] = UnityEngine.Random.value * 360f;
				scalesData[i, 1] = 0.5f + 0.5f * Mathf.Sin(num * (float)Math.PI) + UnityEngine.Random.value * 0.2f;
				scalesData[i, 2] = Mathf.Lerp(-40f, 40f, num) + Mathf.Lerp(-10f, 10f, UnityEngine.Random.value);
			}
			totalSprites = scalePositions.Length * 2;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < scalePositions.Length / 2; j++)
				{
					int num = ((i == 0) ? j : (scalePositions.Length - 1 - j));
					float num2 = (float)num / (float)(scalePositions.Length - 1);
					sLeaser.sprites[firstSprite + ScaleSprite(num, 0)] = new FSprite("Futile_White");
					sLeaser.sprites[firstSprite + ScaleSprite(num, 0)].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
					sLeaser.sprites[firstSprite + ScaleSprite(num, 0)].alpha = 0f;
					sLeaser.sprites[firstSprite + ScaleSprite(num, 0)].color = new Color(1f, 1f, 1f) * Mathf.Lerp(0.55f, (index == 0) ? 0.65f : 0.8f, Mathf.Lerp(Mathf.Sin(num2 * (float)Math.PI), 1f, (1f - num2) * 0.5f));
					sLeaser.sprites[firstSprite + ScaleSprite(num, 0)].scale = scalesData[j, 1] * 14f * owner.scale / (owner.depth * 16f);
					sLeaser.sprites[firstSprite + ScaleSprite(num, 1)] = new FSprite("LizardHead0.1");
					sLeaser.sprites[firstSprite + ScaleSprite(num, 1)].scaleY = scalesData[j, 1] * 3.5f * owner.scale / (owner.depth * 16f);
					sLeaser.sprites[firstSprite + ScaleSprite(num, 1)].scaleX = ((i == 1) ? (-1f) : 1f) * scalesData[j, 1] * 0.8f * owner.scale / (owner.depth * 16f);
					sLeaser.sprites[firstSprite + ScaleSprite(num, 1)].color = new Color(1f, 0.5f, 0.5f);
					sLeaser.sprites[firstSprite + ScaleSprite(num, 1)].anchorY = 1.5f;
				}
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(owner.chunks[0].lastPos, owner.chunks[0].pos, timeStacker);
			Vector2 vector2 = Custom.DirVec(Vector2.Lerp(owner.chunks[1].lastPos, owner.chunks[1].pos, timeStacker), vector);
			vector -= vector2 * owner.chunks[0].rad * ((index == 0) ? 0.55f : 0.75f);
			float num = Custom.VecToDeg(vector2);
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < scalePositions.Length / 2; j++)
				{
					int num2 = ((i == 0) ? j : (scalePositions.Length - 1 - j));
					Vector2 vector3 = vector + Custom.RotateAroundOrigo(scalePositions[num2], num) * owner.chunks[0].rad;
					vector3 -= camPos;
					vector3 = (vector3 - owner.voidSea.convergencePoint) / owner.depth + owner.voidSea.convergencePoint;
					sLeaser.sprites[firstSprite + ScaleSprite(num2, 0)].x = vector3.x;
					sLeaser.sprites[firstSprite + ScaleSprite(num2, 0)].y = vector3.y;
					sLeaser.sprites[firstSprite + ScaleSprite(num2, 0)].rotation = num + scalesData[num2, 0];
					sLeaser.sprites[firstSprite + ScaleSprite(num2, 1)].x = vector3.x;
					sLeaser.sprites[firstSprite + ScaleSprite(num2, 1)].y = vector3.y;
					sLeaser.sprites[firstSprite + ScaleSprite(num2, 1)].rotation = num + scalesData[num2, 2] + 180f;
				}
			}
		}
	}

	public class Chunk
	{
		public VoidWorm owner;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float rad;

		public float connectionRad;

		public float mass;

		public float rotat;

		public float lastRotat;

		public Chunk(VoidWorm worm, Vector2 pos, float rad, float mass)
		{
			owner = worm;
			this.pos = pos;
			lastPos = pos;
			this.rad = rad;
			this.mass = mass;
		}

		public void Update()
		{
			lastPos = pos;
			pos += vel;
			lastRotat = rotat;
		}
	}

	public abstract class VoidWormBehavior
	{
		public VoidWorm worm;

		public Vector2 goalPos;

		public bool swim = true;

		public VoidSeaScene voidSea => worm.voidSea;

		public VoidWormBehavior(VoidWorm worm)
		{
			this.worm = worm;
			goalPos = worm.chunks[0].pos;
		}

		public virtual void Update()
		{
		}
	}

	public class BackgroundWormBehavior : VoidWormBehavior
	{
		public BackgroundWormBehavior(VoidWorm worm)
			: base(worm)
		{
		}

		public override void Update()
		{
			base.Update();
			Player player = base.voidSea.room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = base.voidSea.room.game.RealizedPlayerFollowedByCamera;
			}
			if (player != null && Custom.DistLess(worm.chunks[0].pos, goalPos, 400f * worm.scale))
			{
				float num = (base.voidSea.Inverted ? (-17000f) : 0f);
				goalPos = new Vector2(player.mainBodyChunk.pos.x + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Max(14000f, 600f * worm.depth), base.voidSea.voidWormsAltitude + num + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3000f);
			}
		}
	}

	public class MainWormBehavior : VoidWormBehavior
	{
		public class Phase : ExtEnum<Phase>
		{
			public static readonly Phase Idle = new Phase("Idle", register: true);

			public static readonly Phase GetToPlayer = new Phase("GetToPlayer", register: true);

			public static readonly Phase Looking = new Phase("Looking", register: true);

			public static readonly Phase AttachingString = new Phase("AttachingString", register: true);

			public static readonly Phase StringAttached = new Phase("StringAttached", register: true);

			public static readonly Phase SwimUp = new Phase("SwimUp", register: true);

			public static readonly Phase SwimDown = new Phase("SwimDown", register: true);

			public static readonly Phase DepthReached = new Phase("DepthReached", register: true);

			public static readonly Phase SwimBackUp = new Phase("SwimBackUp", register: true);

			public Phase(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		private float attachStringFac;

		public Phase phase = Phase.Idle;

		public Vector2 relativeLookFromGoal;

		public Vector2 getToRelativeLookFromGoal;

		public Vector2 lastGetToRelativeLookFromGoal;

		public float relativeLookFromGoalProgression = 1f;

		public float relativeLookFromGoalProgressionSpeed = 1f;

		public int timeInPhase;

		public bool ascensionCutStarted;

		public float saintFlasher;

		private float beganAscendingHeight;

		public MainWormBehavior(VoidWorm worm)
			: base(worm)
		{
			if (base.voidSea.Inverted)
			{
				relativeLookFromGoal = new Vector2(-200f, -400f);
				getToRelativeLookFromGoal = new Vector2(-200f, -400f);
				lastGetToRelativeLookFromGoal = new Vector2(-200f, -400f);
			}
			else
			{
				relativeLookFromGoal = new Vector2(-200f, 800f);
				getToRelativeLookFromGoal = new Vector2(-200f, 800f);
				lastGetToRelativeLookFromGoal = new Vector2(-200f, 800f);
			}
		}

		public override void Update()
		{
			base.Update();
			Player player = base.voidSea.room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = base.voidSea.room.game.RealizedPlayerFollowedByCamera;
			}
			if (player == null)
			{
				return;
			}
			if (player.room != worm.room)
			{
				phase = Phase.Idle;
			}
			player.wormCutsceneTarget = worm.head.HeadPos(0.5f);
			if (phase == Phase.Idle)
			{
				swim = true;
				if (player != null && Custom.DistLess(worm.chunks[0].pos, goalPos, 400f * worm.scale))
				{
					goalPos = new Vector2(player.mainBodyChunk.pos.x + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Max(14000f, 600f * worm.depth), base.voidSea.voidWormsAltitude + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3000f);
					if (ModManager.MSC && base.voidSea.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint && player.firstChunk.pos.y < 38000f)
					{
						player.bodyChunks[0].pos.y += 4f;
						player.bodyChunks[1].pos.y += 4f;
						goalPos += new Vector2(-10000f, -16000f);
					}
				}
				if ((player.mainBodyChunk.pos.y < -25000f && !base.voidSea.Inverted) || (player.mainBodyChunk.pos.y > 30000f && base.voidSea.Inverted))
				{
					SwitchPhase(Phase.GetToPlayer);
				}
			}
			else if (phase == Phase.GetToPlayer)
			{
				swim = false;
				RainWorld.lockGameTimer = true;
				goalPos = player.mainBodyChunk.pos + relativeLookFromGoal;
				if (ModManager.MSC && base.voidSea.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint && player.firstChunk.pos.y < 35000f)
				{
					player.bodyChunks[0].pos.y += 4f;
					player.bodyChunks[1].pos.y += 4f;
					goalPos += new Vector2(-10000f, -16000f);
				}
				if ((worm.chunks[0].pos.y > player.mainBodyChunk.pos.y + 2000f && !base.voidSea.Inverted) || (worm.chunks[0].pos.y < player.mainBodyChunk.pos.y - 2000f && base.voidSea.Inverted))
				{
					for (int i = 0; i < worm.chunks.Length; i++)
					{
						worm.chunks[i].vel += i * new Vector2(-0.01f, 0.05f) * worm.scale;
					}
					worm.chunks[0].vel += Custom.DirVec(worm.chunks[0].pos, goalPos) * Mathf.InverseLerp(100f, 800f, Vector2.Distance(worm.chunks[0].pos, goalPos)) * worm.scale * 22f;
				}
				if (((ModManager.MSC && base.voidSea.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint && player.firstChunk.pos.y >= 35000f) || !ModManager.MSC || base.voidSea.room.game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Saint) && Custom.DistLess(worm.chunks[0].pos, player.mainBodyChunk.pos + relativeLookFromGoal, 800f))
				{
					SwitchPhase(Phase.Looking);
				}
				if (worm.glowLoop == null)
				{
					worm.glowLoop = new StaticSoundLoop(SoundID.Void_Sea_Individual_Worm_Glow_LOOP, default(Vector2), base.voidSea.room, 1f, 1f);
				}
				if (worm.intenseGlowLoop == null)
				{
					worm.intenseGlowLoop = new StaticSoundLoop(SoundID.Void_Sea_Individual_Worm_Intense_Glow_LOOP, default(Vector2), base.voidSea.room, 1f, 1f);
				}
			}
			else if (phase == Phase.Looking || phase == Phase.StringAttached)
			{
				if (phase == Phase.Looking)
				{
					worm.lightDimmed = Custom.LerpAndTick(worm.lightDimmed, (timeInPhase < 70) ? 0f : 1f, 0.003f, 0.0033333334f);
				}
				else
				{
					worm.lightDimmed = Custom.LerpAndTick(worm.lightDimmed, (timeInPhase < 170) ? 1f : 0f, 0.003f, 0.0033333334f);
				}
				goalPos = player.mainBodyChunk.pos + relativeLookFromGoal;
				if (relativeLookFromGoalProgression >= 1f && UnityEngine.Random.value < 1f / 30f)
				{
					lastGetToRelativeLookFromGoal = relativeLookFromGoal;
					if (ModManager.MSC && base.voidSea.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint)
					{
						getToRelativeLookFromGoal = Vector2.Lerp(new Vector2(-0.8f, -0.2f), new Vector2(-1f, -0.75f), UnityEngine.Random.value) * Mathf.Lerp(600f, 700f, Mathf.Pow(UnityEngine.Random.value, 0.85f));
					}
					else
					{
						getToRelativeLookFromGoal = Custom.DegToVec(Mathf.Pow(UnityEngine.Random.value, 2f) * 130f * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f)) * Mathf.Lerp(400f, 900f, Mathf.Pow(UnityEngine.Random.value, 0.85f));
					}
					relativeLookFromGoalProgressionSpeed = 15f / Vector2.Distance(lastGetToRelativeLookFromGoal, getToRelativeLookFromGoal);
					relativeLookFromGoalProgression = 0f;
				}
				relativeLookFromGoalProgression = Mathf.Min(1f, relativeLookFromGoalProgression + relativeLookFromGoalProgressionSpeed);
				relativeLookFromGoal = Vector3.Slerp(lastGetToRelativeLookFromGoal, getToRelativeLookFromGoal, Custom.SCurve(relativeLookFromGoalProgression, 0.25f));
				if (phase == Phase.Looking && timeInPhase > 600)
				{
					if (ModManager.MSC && player.playerState.slugcatCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
					{
						SwitchPhase(Phase.SwimDown);
					}
					else if (!ModManager.MSC || player.playerState.slugcatCharacter != MoreSlugcatsEnums.SlugcatStatsName.Saint || base.voidSea.room.world.name != "HR")
					{
						SwitchPhase(Phase.AttachingString);
					}
					else if (player.voidSceneTimer == 0 && !ascensionCutStarted)
					{
						player.InitVoidWormCutscene();
						ascensionCutStarted = true;
					}
					else if (player.voidSceneTimer == 0 && ascensionCutStarted)
					{
						beganAscendingHeight = Mathf.Clamp(player.firstChunk.pos.y + 2000f, 0f, 45000f);
						base.voidSea.DestroyDistantWorms();
						base.voidSea.DestroyCeiling();
						base.voidSea.DestroyAllWormsExceptMainWorm();
						saintFlasher = 1f;
						SwitchPhase(MoreSlugcatsEnums.MainWormBehaviorPhase.Ascended);
						worm.voidSea.switchSaintEndPhase(VoidSeaScene.SaintEndingPhase.WormDeath);
						worm.room.AddObject(new ShockWave(worm.head.HeadPos(0.5f), 500f, 0.75f, 18));
						worm.room.AddObject(new Explosion.ExplosionLight(worm.head.HeadPos(0.5f), 320f, 1f, 5, Color.white));
					}
				}
				if (phase == Phase.StringAttached && timeInPhase > 220)
				{
					SwitchPhase(Phase.SwimUp);
				}
			}
			else if (phase == Phase.AttachingString)
			{
				if (Custom.DistLess(worm.chunks[0].pos, goalPos, 300f) || timeInPhase > 800)
				{
					attachStringFac = Mathf.Min(1f, attachStringFac + 0.005f);
				}
				else
				{
					attachStringFac = Mathf.Max(0f, attachStringFac - 0.0025f);
				}
				goalPos = Custom.MoveTowards(goalPos, player.mainBodyChunk.pos + new Vector2(100f, 500f), 60f);
				for (int j = 0; j < worm.arms.Count; j++)
				{
					worm.arms[j].goalPoint = player.mainBodyChunk.pos;
					worm.arms[j].goToGoal = Mathf.Pow(attachStringFac, 2f) * ((worm.arms[j].thread != null) ? 1f : (-1f));
					if (worm.arms[j].thread != null && (Custom.DistLess(worm.arms[j].segments[worm.arms[j].segments.GetLength(0) - 1, 0], player.mainBodyChunk.pos, 10f) || attachStringFac >= 1f))
					{
						player.mainBodyChunk.vel += Custom.DirVec(worm.arms[j].segments[worm.arms[j].segments.GetLength(0) - 1, 0], player.mainBodyChunk.pos) * 6f;
						worm.arms[j].segments[worm.arms[j].segments.GetLength(0) - 1, 2] -= Custom.DirVec(worm.arms[j].segments[worm.arms[j].segments.GetLength(0) - 1, 0], player.mainBodyChunk.pos) * 14f;
						player.Blink(6);
						worm.arms[j].threadStatus = 1;
						SwitchPhase(Phase.StringAttached);
						for (int k = 0; k < worm.arms.Count; k++)
						{
							worm.arms[k].goToGoal = 0f;
						}
						break;
					}
				}
			}
			else if (phase == Phase.SwimUp)
			{
				worm.lightDimmed = Custom.LerpAndTick(worm.lightDimmed, 0f, 0.003f, 1f / 90f);
				goalPos = new Vector2(base.voidSea.sceneOrigo.x, base.voidSea.voidWormsAltitude + 7000f);
				swim = true;
				base.voidSea.ridingWorm = true;
				if (worm.chunks[0].pos.y > base.voidSea.voidWormsAltitude + 7000f)
				{
					SwitchPhase(Phase.SwimDown);
				}
			}
			else if (phase == Phase.SwimDown)
			{
				worm.lightDimmed = Custom.LerpAndTick(worm.lightDimmed, 0f, 0.003f, 1f / 90f);
				goalPos = worm.chunks[0].pos + new Vector2(0f, -100000f);
				swim = true;
				SuperSwim(-40f * Mathf.InverseLerp(0f, 200f, timeInPhase));
				if (!ModManager.MSC || player.playerState.slugcatCharacter != MoreSlugcatsEnums.SlugcatStatsName.Artificer)
				{
					if (!base.voidSea.secondSpace)
					{
						if (worm.chunks[0].pos.y < -17000f && (int)base.voidSea.deepDivePhase < (int)VoidSeaScene.DeepDivePhase.CeilingDestroyed)
						{
							base.voidSea.DestroyCeiling();
						}
						if (worm.chunks[0].pos.y < -35000f && (int)base.voidSea.deepDivePhase < (int)VoidSeaScene.DeepDivePhase.CloseWormsDestroyed)
						{
							base.voidSea.DestroyAllWormsExceptMainWorm();
						}
						if (worm.chunks[0].pos.y < -200000f && (int)base.voidSea.deepDivePhase < (int)VoidSeaScene.DeepDivePhase.DistantWormsDestroyed)
						{
							base.voidSea.DestroyDistantWorms();
						}
						if (worm.chunks[0].pos.y < -440000f && (int)base.voidSea.deepDivePhase < (int)VoidSeaScene.DeepDivePhase.MovedIntoSecondSpace)
						{
							base.voidSea.MovedToSecondSpace();
						}
					}
					else if (worm.chunks[0].pos.y < -11000f)
					{
						SwitchPhase(Phase.DepthReached);
					}
				}
				else if (worm.chunks[0].pos.y < -35000f)
				{
					base.voidSea.fadeOutLights = true;
				}
			}
			else if (phase == Phase.DepthReached)
			{
				worm.lightDimmed = Custom.LerpAndTick(worm.lightDimmed, (timeInPhase < 500) ? 1f : 0f, 0.003f, 0.0033333334f);
				base.voidSea.room.game.cameras[0].voidSeaGoldFilter = Mathf.InverseLerp(10000f, 3000f, Vector2.Distance(player.mainBodyChunk.pos, worm.chunks[0].pos));
				base.voidSea.room.game.cameras[0].voidSeaGoldFilter = Mathf.Lerp(base.voidSea.room.game.cameras[0].voidSeaGoldFilter, 1f, Mathf.InverseLerp(600f, 100f, timeInPhase));
				if (timeInPhase < 100)
				{
					swim = true;
				}
				else if (timeInPhase < 400)
				{
					goalPos = Vector2.Lerp(goalPos, player.mainBodyChunk.pos + new Vector2(-100f, 500f), 0.2f);
					swim = false;
				}
				else if (timeInPhase == 400)
				{
					for (int l = 0; l < worm.arms.Count; l++)
					{
						if (worm.arms[l].thread != null)
						{
							worm.arms[l].threadStatus = 0;
						}
					}
					goalPos = player.mainBodyChunk.pos;
				}
				else if (timeInPhase < 600)
				{
					goalPos = Vector2.Lerp(goalPos, player.mainBodyChunk.pos + new Vector2(-100f, 500f), 0.2f);
					swim = false;
				}
				else if (timeInPhase < 700)
				{
					swim = true;
					goalPos = new Vector2(player.mainBodyChunk.pos.x, worm.chunks[0].pos.y - 100000f);
					SuperSwim(-150f * Mathf.InverseLerp(600f, 650f, timeInPhase));
				}
				else if (timeInPhase == 700)
				{
					for (int m = 0; m < worm.arms.Count; m++)
					{
						if (worm.arms[m].thread != null)
						{
							worm.arms[m].threadStatus = -1;
						}
					}
				}
				else
				{
					swim = true;
					goalPos = new Vector2(player.mainBodyChunk.pos.x, 100000f);
					SuperSwim(50f);
					if (worm.chunks[0].pos.y > 10000f && worm.chunks[worm.chunks.Length - 1].pos.y > 10000f)
					{
						base.voidSea.DestroyMainWorm();
						base.voidSea.room.game.cameras[0].voidSeaGoldFilter = 0f;
					}
				}
			}
			else if (ModManager.MSC && phase == MoreSlugcatsEnums.MainWormBehaviorPhase.Ascended)
			{
				worm.glowLoopVolMuffle = Mathf.Lerp(worm.glowLoopVolMuffle, 1f, 0.05f);
				worm.lightDimmed = Custom.LerpAndTick(worm.lightDimmed, 1f, 0.003f, 1f / 90f);
				goalPos = new Vector2(player.mainBodyChunk.pos.x, player.mainBodyChunk.pos.y - (float)timeInPhase * 5f);
				swim = true;
				if (player.mainBodyChunk.pos.y <= 81000f)
				{
					player.bodyChunks[1].vel += new Vector2(0f, Mathf.Sqrt(timeInPhase) * Mathf.Pow(timeInPhase, 0.9f));
					float num = Mathf.InverseLerp(beganAscendingHeight, 80000f, player.mainBodyChunk.pos.y);
					int num2 = (int)Mathf.Clamp((1f - num) * 10f, 0f, 9f);
					num2++;
					int num3 = num2;
					if (num2 < 6)
					{
						num2--;
					}
					if (num3 < 5)
					{
						num3--;
					}
					num = Mathf.Lerp(num, 1f, saintFlasher);
					saintFlasher *= 0.8f;
					player.room.game.cameras[0].hud.karmaMeter.forceVisibleCounter = 80;
					base.voidSea.voidSeaBackground.color = new Color(num, num, num);
					if ((player.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma > num2)
					{
						base.voidSea.storedJourneyIllustration.UpdateImageJourney(num3 + 1, base.voidSea);
						base.voidSea.storedJourneyIllustration.fadeCounter = 1f;
						saintFlasher = 1f;
						(player.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = num2;
						player.room.game.cameras[0].hud.karmaMeter.UpdateGraphic(num2, num2);
					}
				}
				else
				{
					base.voidSea.storedJourneyIllustration.UpdateImageJourney(0, base.voidSea);
					base.voidSea.storedJourneyIllustration.fadeCounter = 1f;
					base.voidSea.voidSeaBackground.color = Color.white;
					base.voidSea.switchSaintEndPhase(VoidSeaScene.SaintEndingPhase.EchoTransform);
					base.voidSea.DeleteMainWorm();
					(player.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = -1;
					player.room.game.cameras[0].hud.karmaMeter.forceVisibleCounter = 200;
					player.room.game.cameras[0].hud.karmaMeter.reinforceAnimation = 0;
					player.room.game.cameras[0].hud.karmaMeter.displayKarma = new IntVector2(-1, (player.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap);
					player.room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
				}
			}
			timeInPhase++;
		}

		private void SuperSwim(float add)
		{
			for (int i = 0; i < worm.chunks.Length; i++)
			{
				worm.chunks[i].vel.y += add;
			}
		}

		private void SwitchPhase(Phase nextPhase)
		{
			phase = nextPhase;
			timeInPhase = 0;
		}
	}

	public VoidSeaScene voidSea;

	public Chunk[] chunks;

	public Chunk[][,] fins;

	private float[,] finsData;

	public float swimSpeed = 1f;

	public float swimMotion;

	public Vector2 swimDir;

	public float scale = 8f;

	private int meshDivsPerTailSegment;

	private int meshSegments;

	public float graphicFidelity;

	public float lightDimmed;

	public StaticSoundLoop swimLoop;

	public StaticSoundLoop glowLoop;

	public StaticSoundLoop intenseGlowLoop;

	public int[] lightSprites;

	public int totalSprites;

	private float dark;

	private float transparent;

	private float shakeScreen;

	private Head head;

	public List<Arm> arms;

	public List<Scales> scales;

	private VoidWormBehavior behavior;

	public bool mainWorm;

	public float glowLoopVolMuffle;

	public float lightAlpha;

	public int BodySprite => fins.Length * 2;

	public int FinSprite(int fin, int side)
	{
		return fin * 2 + side;
	}

	public int FinConnectChunk(int finPair)
	{
		return 1 + finPair * (int)Custom.LerpMap(finPair, 0f, 6f, 1f, 3f);
	}

	private float FinContour(float f)
	{
		if (f < 1f / 3f)
		{
			return 0.3f + 0.7f * Custom.SCurve(Mathf.InverseLerp(0f, 1f / 3f, f), 0.5f);
		}
		return Mathf.InverseLerp(1f, 1f / 3f, f);
	}

	public VoidWorm(VoidSeaScene voidSea, Vector2 pos, float depth, bool mainWorm)
		: base(voidSea, pos, depth)
	{
		this.voidSea = voidSea;
		this.mainWorm = mainWorm;
		dark = 1f - 1f / (depth * 0.5f);
		dark = Mathf.Lerp(dark, 1f, Mathf.InverseLerp(25f, 32f, depth));
		dark = Mathf.Lerp(dark, 0f, Mathf.InverseLerp(15f, 0f, depth) * 0.75f);
		transparent = Mathf.InverseLerp(15f, 70f, depth);
		shakeScreen = Mathf.Pow(Mathf.InverseLerp(3f, 1f, depth), 1.5f);
		voidSea.LoadGraphic("wormSkin", crispPixels: false, clampWrapMode: false);
		voidSea.LoadGraphic("wormSkin2", crispPixels: false, clampWrapMode: false);
		graphicFidelity = Mathf.Pow(Mathf.InverseLerp(10f, 1f, depth), 2f);
		meshDivsPerTailSegment = (int)Mathf.Lerp(1f, 8f, graphicFidelity);
		chunks = new Chunk[60];
		for (int i = 0; i < 30; i++)
		{
			float num = (float)i / 29f;
			num = (1f - num) * 0.5f + Mathf.Sin(Mathf.Pow(num, 0.5f) * (float)Math.PI) * 0.5f;
			chunks[i] = new Chunk(this, default(Vector2), Mathf.Lerp(10f, 60f, num) * scale, Mathf.Lerp(0.5f, 20f, num) * scale);
			if (i > 0)
			{
				chunks[i].connectionRad = Mathf.Max(chunks[i - 1].rad, chunks[i].rad) * 2f;
			}
		}
		for (int j = 30; j < chunks.Length; j++)
		{
			float num2 = Mathf.InverseLerp(30f, chunks.Length - 1, j);
			chunks[j] = new Chunk(this, default(Vector2), Mathf.Lerp(chunks[29].rad, 1f, num2), Mathf.Lerp(0.5f, 0.01f, num2));
			chunks[j].connectionRad = Mathf.Lerp(Mathf.Max(chunks[j - 1].rad, chunks[j].rad) * 2f, 400f * scale, Mathf.Pow(num2, 0.25f));
		}
		visible = true;
		meshSegments = 30 + (chunks.Length - 30) * meshDivsPerTailSegment;
		if (depth < 4f)
		{
			swimLoop = new StaticSoundLoop(SoundID.Void_Sea_Individual_Worm_Swimming_LOOP, default(Vector2), voidSea.room, 1f, 1f);
		}
		fins = new Chunk[6][,];
		finsData = new float[fins.Length, 2];
		float num3 = 15f;
		float num4 = Mathf.Lerp(240f, 40f, graphicFidelity);
		for (int k = 0; k < fins.Length; k++)
		{
			float f = (float)k / (float)(fins.Length - 1);
			finsData[k, 0] = 100f + 800f * Mathf.Sin(Mathf.Pow(f, 0.5f) * (float)Math.PI);
			if (k == 0)
			{
				finsData[k, 0] = 400f;
			}
			int num5 = Mathf.FloorToInt(finsData[k, 0] / num4) + 1;
			float num6 = num3 + num3 * Mathf.Sin(Mathf.Pow(f, 0.8f) * (float)Math.PI);
			fins[k] = new Chunk[2, num5];
			for (int l = 0; l < 2; l++)
			{
				finsData[k, 1] = UnityEngine.Random.value;
				for (int m = 0; m < fins[k].GetLength(1); m++)
				{
					fins[k][l, m] = new Chunk(this, default(Vector2), 1f + FinContour((float)m / (float)(fins[k].GetLength(1) - 1)) * num6 * scale, 0.5f);
					fins[k][l, m].connectionRad = num4 * scale;
				}
			}
		}
		totalSprites = 1;
		totalSprites += fins.Length * 2;
		arms = new List<Arm>();
		scales = new List<Scales>();
		if (mainWorm)
		{
			scales.Add(new Scales(this, totalSprites, 0));
			totalSprites += scales[scales.Count - 1].totalSprites;
			for (int n = 0; n < 4; n++)
			{
				for (int num7 = 0; num7 < 2; num7++)
				{
					float num8 = (float)n / 3f;
					Arm arm = new Arm(this, totalSprites, Mathf.Lerp(150f, 37f, Mathf.Pow(1f - num8, 0.7f)) * scale, (num7 == 0) ? (-1f) : 1f, 3 - n, num7 == 0 && n == 2);
					arms.Add(arm);
					totalSprites += arm.totalSprites;
				}
			}
			head = new Head(this, totalSprites);
			totalSprites += head.totalSprites;
			scales.Add(new Scales(this, totalSprites, 1));
			totalSprites += scales[scales.Count - 1].totalSprites;
		}
		lightSprites = new int[3]
		{
			totalSprites,
			totalSprites + 1,
			totalSprites + 2
		};
		totalSprites += 3;
		swimMotion = UnityEngine.Random.value;
		if (mainWorm)
		{
			behavior = new MainWormBehavior(this);
		}
		else
		{
			behavior = new BackgroundWormBehavior(this);
		}
		lightAlpha = 1f;
		float num9 = (voidSea.Inverted ? (-17000f) : 0f);
		Reset(new Vector2(voidSea.sceneOrigo.x + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 700f * depth, voidSea.voidWormsAltitude + num9 + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3000f), new Vector2(0f, 1f));
		behavior.goalPos = chunks[0].pos;
	}

	public void Reset(Vector2 pos, Vector2 dir)
	{
		float num = 0f;
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].pos = pos + dir * num;
			chunks[i].lastPos = pos;
			chunks[i].vel *= 0f;
			num += chunks[i].connectionRad;
		}
		for (int j = 0; j < arms.Count; j++)
		{
			for (int k = 0; k < arms[j].segments.GetLength(0); k++)
			{
				arms[j].segments[k, 0] = chunks[0].pos + Custom.RNV();
				arms[j].segments[k, 1] = arms[j].segments[k, 0];
				arms[j].segments[k, 2] *= 0f;
			}
		}
		if (head != null)
		{
			for (int l = 0; l < head.neck.GetLength(0); l++)
			{
				head.neck[l, 0] = chunks[0].pos + Custom.RNV();
				head.neck[l, 1] = head.neck[l, 0];
				head.neck[l, 2] *= 0f;
			}
		}
		for (int m = 0; m < fins.Length; m++)
		{
			for (int n = 0; n < fins[m].GetLength(0); n++)
			{
				for (int num2 = 0; num2 < fins[m].GetLength(1); num2++)
				{
					fins[m][n, num2].pos = chunks[FinConnectChunk(m)].pos + Custom.RNV();
					fins[m][n, num2].lastPos = fins[m][n, num2].pos;
					fins[m][n, num2].vel *= 0f;
				}
			}
		}
	}

	public void Move(Vector2 mv)
	{
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].pos += mv;
			chunks[i].lastPos += mv;
		}
		for (int j = 0; j < arms.Count; j++)
		{
			for (int k = 0; k < arms[j].segments.GetLength(0); k++)
			{
				arms[j].segments[k, 0] += mv;
				arms[j].segments[k, 1] += mv;
			}
			if (arms[j].thread != null)
			{
				for (int l = 0; l < arms[j].thread.GetLength(0); l++)
				{
					arms[j].thread[l, 0] += mv;
					arms[j].thread[l, 1] += mv;
				}
			}
		}
		if (head != null)
		{
			for (int m = 0; m < head.neck.GetLength(0); m++)
			{
				head.neck[m, 0] += mv;
				head.neck[m, 1] += mv;
			}
		}
		for (int n = 0; n < fins.Length; n++)
		{
			for (int num = 0; num < fins[n].GetLength(0); num++)
			{
				for (int num2 = 0; num2 < fins[n].GetLength(1); num2++)
				{
					fins[n][num, num2].pos += mv;
					fins[n][num, num2].lastPos += mv;
				}
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		behavior.Update();
		if (head != null)
		{
			head.Update();
		}
		for (int i = 0; i < arms.Count; i++)
		{
			arms[i].Update();
		}
		for (int j = 0; j < chunks.Length; j++)
		{
			chunks[j].Update();
			if (j > 0)
			{
				Vector2 vector = (chunks[j - 1].pos - chunks[j].pos).normalized * (Vector2.Distance(chunks[j].pos, chunks[j - 1].pos) - chunks[j].connectionRad);
				float a = chunks[j - 1].mass / (chunks[j].mass + chunks[j - 1].mass);
				a = Mathf.Lerp(a, 1f, Mathf.InverseLerp(20f, 40f, j));
				chunks[j].pos += vector * a;
				chunks[j].vel += vector * a;
				chunks[j - 1].pos -= vector * (1f - a);
				chunks[j - 1].vel -= vector * (1f - a);
				chunks[j].rotat = Mathf.Clamp(chunks[j].rotat, chunks[j - 1].rotat - 0.1f, chunks[j].rotat + 0.1f);
			}
		}
		chunks[0].rotat += 0.05f;
		for (int k = 2; k < 4; k++)
		{
			for (int l = 0; l < 30 - k; l++)
			{
				Vector2 vector2 = Custom.DirVec(chunks[l].pos, chunks[l + k].pos);
				float num = chunks[l + k].mass / (chunks[l].mass + chunks[l + k].mass);
				chunks[l].vel -= vector2 * ((k == 2) ? 0.15f : 0.075f) * num;
				chunks[l + k].vel += vector2 * ((k == 2) ? 0.15f : 0.075f) * (1f - num);
			}
		}
		Player player = voidSea.room.game.FirstRealizedPlayer;
		if (ModManager.CoopAvailable)
		{
			player = voidSea.room.game.RealizedPlayerFollowedByCamera;
		}
		if (shakeScreen > 0f && player != null && player.room == room && voidSea.room.game.cameras[0].voidSeaMode)
		{
			for (int m = 0; m < 30; m++)
			{
				float num2 = Mathf.Pow(Mathf.InverseLerp(0.5f * scale, 20f * scale, chunks[m].rad), 0.5f);
				num2 *= Mathf.InverseLerp(4f, 40f, chunks[m].vel.magnitude / scale);
				num2 *= Mathf.InverseLerp(600f, 200f, Vector2.Distance(chunks[m].pos, player.mainBodyChunk.pos));
				if (num2 > 0.2f)
				{
					voidSea.room.ScreenMovement(null, new Vector2(0f, 0f), Mathf.InverseLerp(0.2f, 1f, num2) * 4f * shakeScreen);
					player.mainBodyChunk.vel += chunks[m].vel * 10f * Mathf.InverseLerp(0.2f, 1f, num2) * shakeScreen;
					player.bodyChunks[1].vel += chunks[m].vel * 10f * Mathf.InverseLerp(0.2f, 1f, num2) * shakeScreen;
				}
			}
		}
		if (behavior.swim)
		{
			Swim();
		}
		else
		{
			Float();
		}
		for (int n = 0; n < fins.Length; n++)
		{
			Vector2 normalized = (chunks[FinConnectChunk(n)].pos - chunks[FinConnectChunk(n) + 1].pos).normalized;
			for (int num3 = 0; num3 < 2; num3++)
			{
				Vector2 vector3 = Custom.PerpendicularVector(normalized) * ((num3 == 0) ? (-1f) : 1f);
				Vector2 vector4 = chunks[FinConnectChunk(n)].pos + vector3 * chunks[FinConnectChunk(n)].rad * 0.9f;
				Vector2 vector5 = vector3 + normalized * Custom.LerpMap(n, 0f, 6f, 1f, -1f) + normalized * Mathf.Sin(swimMotion * (float)Math.PI * 2f) * ((n % 2 == 0 == (num3 == 0)) ? (-1f) : 1f);
				vector5.Normalize();
				for (int num4 = 0; num4 < fins[n].GetLength(1); num4++)
				{
					float num5 = (float)num4 / (float)(fins[n].GetLength(1) - 1);
					fins[n][num3, num4].Update();
					fins[n][num3, num4].vel *= Custom.LerpMap(fins[n][num3, num4].vel.magnitude, 2f * scale, 12f * scale, 0.999f, 0.75f, Mathf.Lerp(5f, 2f, num5));
					if (num5 < Custom.LerpMap(n, 0f, 5f, 0.5f, 0.1f))
					{
						Vector2 vector6 = vector4 + vector5 * (num4 + 1) * fins[n][num3, num4].connectionRad;
						fins[n][num3, num4].vel += (vector6 - fins[n][num3, num4].pos) * Mathf.Pow(Mathf.InverseLerp(Custom.LerpMap(n, 0f, 5f, 0.5f, 0.1f), 0f, num5), Custom.LerpMap(n, 0f, 5f, 0.5f, 4f)) / Custom.LerpMap(n, 0f, 5f, 10f, 40f);
					}
					fins[n][num3, num4].vel += vector5 * (1f - num5) * scale * 0.5f;
					for (int num6 = 2; num6 < 6; num6++)
					{
						if (num4 - num6 >= 0)
						{
							fins[n][num3, num4].vel += Custom.DirVec(fins[n][num3, num4 - num6].pos, fins[n][num3, num4].pos) * Mathf.Lerp(1.4f, 0.1f, num5) * scale;
						}
					}
					if (num4 > 0)
					{
						Vector2 vector7 = (fins[n][num3, num4 - 1].pos - fins[n][num3, num4].pos).normalized * (Vector2.Distance(fins[n][num3, num4].pos, fins[n][num3, num4 - 1].pos) - fins[n][num3, num4].connectionRad);
						float a2 = fins[n][num3, num4 - 1].mass / (fins[n][num3, num4].mass + fins[n][num3, num4 - 1].mass);
						a2 = Mathf.Lerp(a2, 1f, 1f - num5);
						fins[n][num3, num4].pos += vector7 * a2;
						fins[n][num3, num4].vel += vector7 * a2;
						fins[n][num3, num4 - 1].pos -= vector7 * (1f - a2);
						fins[n][num3, num4 - 1].vel -= vector7 * (1f - a2);
					}
					else
					{
						Vector2 vector8 = (vector4 - fins[n][num3, num4].pos).normalized * (Vector2.Distance(fins[n][num3, num4].pos, vector4) - fins[n][num3, num4].connectionRad);
						fins[n][num3, num4].pos += vector8;
						fins[n][num3, num4].vel += vector8;
					}
				}
			}
		}
		if (swimLoop != null)
		{
			swimLoop.Update();
			swimLoop.pos = (chunks[0].pos - voidSea.convergencePoint) / depth + voidSea.convergencePoint;
			swimLoop.volume = Mathf.InverseLerp(30f, 150f, Vector2.Distance(chunks[0].lastPos, chunks[0].pos)) * Mathf.InverseLerp(8f, 1f, depth);
			swimLoop.pitch = 1f;
		}
		if (glowLoop != null)
		{
			glowLoop.Update();
			glowLoop.pos = (chunks[0].pos - voidSea.convergencePoint) / depth + voidSea.convergencePoint;
			glowLoop.volume = 1f * (1f - glowLoopVolMuffle);
		}
		if (intenseGlowLoop != null)
		{
			intenseGlowLoop.Update();
			intenseGlowLoop.pos = (chunks[0].pos - voidSea.convergencePoint) / depth + voidSea.convergencePoint;
			intenseGlowLoop.volume = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(lightDimmed * (float)Math.PI)), 0.3f) * (1f - glowLoopVolMuffle);
			intenseGlowLoop.pitch = Mathf.Lerp(3.5f, 0.5f, lightDimmed);
		}
	}

	public void Swim()
	{
		swimMotion -= 1f / Mathf.Lerp(220f, 60f, swimSpeed);
		if (behavior.swim)
		{
			swimSpeed = Mathf.Min(1f, swimSpeed + 1f / 120f);
		}
		else
		{
			swimSpeed *= 0.8f;
		}
		swimDir = Vector3.Slerp(swimDir, Custom.DirVec(chunks[0].pos, behavior.goalPos), 0.3f);
		for (int i = 0; i < 30; i++)
		{
			float num = (float)i / 29f;
			float num2 = Mathf.InverseLerp(1f, 0.75f, num);
			chunks[i].vel *= Custom.LerpMap(chunks[i].vel.magnitude, 2f * scale, 12f * scale, 0.99f, 0.75f);
			chunks[i].vel += swimDir * Mathf.Lerp(1f, -1f, Mathf.Pow(num, 0.5f)) * 0.25f * swimSpeed * scale * num2;
			Vector2 vector = Custom.DirVec(chunks[i + 1].pos, chunks[i].pos);
			chunks[i].vel += vector * 5.125f * Mathf.Lerp(0.5f, 1f, swimSpeed) * scale * num2;
			chunks[i].vel += Custom.PerpendicularVector(vector) * Mathf.Sin((swimMotion + num * 3f) * (float)Math.PI * 2f) * 0.95f * num * Mathf.Lerp(0.5f, 1f, swimSpeed) * scale * num2;
		}
		chunks[0].vel += swimDir * 0.25f * scale;
		for (int j = 30; j < chunks.Length; j++)
		{
			float num3 = Mathf.InverseLerp(30f, chunks.Length - 1, j);
			chunks[j].vel *= Mathf.Lerp(0.98f, 0.1f, Mathf.Pow(Mathf.InverseLerp(2f * scale, 20f * scale, chunks[j].vel.magnitude), Mathf.Lerp(5f, 0.5f, num3)));
			for (int k = 2; k < 8; k++)
			{
				chunks[j].vel += Custom.DirVec(chunks[j - k].pos, chunks[j].pos) * Mathf.Lerp(1.4f, 0.1f, num3) * scale;
			}
			if (num3 < 0.3f)
			{
				chunks[j].vel += Custom.DirVec(chunks[28].pos, chunks[29].pos) * Custom.LerpMap(num3, 0f, 0.3f, 6f, 0f) * scale;
			}
		}
	}

	public void Float()
	{
		float num = Mathf.InverseLerp(0f, 50f * scale, Vector2.Distance(chunks[0].pos, behavior.goalPos));
		for (int i = 0; i < 30; i++)
		{
			float f = (float)i / 29f;
			chunks[i].vel *= Custom.LerpMap(chunks[i].vel.magnitude, 2f * scale, 12f * scale, 0.99f, 0.75f) * num;
			chunks[i].vel += Custom.DirVec(chunks[0].pos, behavior.goalPos) * Mathf.Lerp(1f, -1f, Mathf.Pow(f, 0.8f)) * num;
		}
		if (!Custom.DistLess(chunks[0].pos, behavior.goalPos, 800f))
		{
			chunks[0].vel += Custom.DirVec(chunks[0].pos, behavior.goalPos) * Mathf.InverseLerp(Vector2.Distance(chunks[0].pos, behavior.goalPos), 200f * scale, 300f * scale) * 3f;
		}
		for (int j = 30; j < chunks.Length; j++)
		{
			float t = Mathf.InverseLerp(30f, chunks.Length - 1, j);
			chunks[j].vel *= Mathf.Lerp(0.98f, 0.1f, Mathf.Pow(Mathf.InverseLerp(2f * scale, 20f * scale, chunks[j].vel.magnitude), Mathf.Lerp(5f, 0.5f, t)));
		}
	}

	public override void Destroy()
	{
		if (swimLoop != null && swimLoop.emitter != null)
		{
			swimLoop.emitter.Destroy();
		}
		if (glowLoop != null && glowLoop.emitter != null)
		{
			glowLoop.emitter.Destroy();
		}
		if (intenseGlowLoop != null && intenseGlowLoop.emitter != null)
		{
			intenseGlowLoop.emitter.Destroy();
		}
		base.Destroy();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[BodySprite] = TriangleMesh.MakeLongMesh(meshSegments, pointyTip: false, customColor: true, "wormSkin");
		sLeaser.sprites[BodySprite].shader = rCam.game.rainWorld.Shaders["VoidWormBody"];
		for (int i = 0; i < fins.Length; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[FinSprite(i, j)] = TriangleMesh.MakeLongMesh(fins[i].GetLength(1), pointyTip: false, customColor: false, "wormSkin2");
				sLeaser.sprites[FinSprite(i, j)].shader = rCam.game.rainWorld.Shaders["VoidWormFin"];
				sLeaser.sprites[FinSprite(i, j)].color = new Color(1f - transparent, 0f, 0f, dark);
			}
		}
		if (head != null)
		{
			head.InitiateSprites(sLeaser, rCam);
		}
		for (int k = 0; k < arms.Count; k++)
		{
			arms[k].InitiateSprites(sLeaser, rCam);
		}
		for (int l = 0; l < scales.Count; l++)
		{
			scales[l].InitiateSprites(sLeaser, rCam);
		}
		sLeaser.sprites[lightSprites[0]] = new FSprite("Futile_White");
		sLeaser.sprites[lightSprites[0]].shader = rCam.game.rainWorld.Shaders["FlatLight"];
		sLeaser.sprites[lightSprites[1]] = new FSprite("Futile_White");
		sLeaser.sprites[lightSprites[1]].shader = rCam.game.rainWorld.Shaders["FlatWaterLight"];
		sLeaser.sprites[lightSprites[2]] = new FSprite("Futile_White");
		sLeaser.sprites[lightSprites[2]].shader = rCam.game.rainWorld.Shaders["FlatLight"];
		base.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 a = default(Vector2);
		Vector2 b = default(Vector2);
		Vector2 cA = default(Vector2);
		Vector2 cB = default(Vector2);
		int num = -1;
		float num2 = 0.5f;
		Vector2 vector = Vector2.Lerp(chunks[0].lastPos, chunks[0].pos, timeStacker) - camPos;
		for (int i = 0; i < meshSegments; i++)
		{
			float t = (float)i / (float)(30 + (chunks.Length - 30) * meshDivsPerTailSegment - 1);
			Vector2 vector2;
			float num3;
			if (i < 30)
			{
				vector2 = Vector2.Lerp(chunks[i].lastPos, chunks[i].pos, timeStacker) - camPos;
				num3 = chunks[i].rad / depth;
			}
			else
			{
				int num4 = 30 + Mathf.FloorToInt((i - 30) / meshDivsPerTailSegment);
				int num5 = Math.Min(num4 + 1, chunks.Length - 1);
				if (num4 != num)
				{
					num = num4;
					int num6 = num4 - 1;
					int num7 = Math.Min(num4 + 2, chunks.Length - 1);
					Vector2 vector3 = Vector2.Lerp(chunks[num4].lastPos, chunks[num4].pos, timeStacker) - camPos;
					Vector2 vector4 = Vector2.Lerp(chunks[num5].lastPos, chunks[num5].pos, timeStacker) - camPos;
					Vector2 vector5 = Vector2.Lerp(chunks[num6].lastPos, chunks[num6].pos, timeStacker) - camPos;
					Vector2 vector6 = Vector2.Lerp(chunks[num7].lastPos, chunks[num7].pos, timeStacker) - camPos;
					a = vector3;
					b = vector4;
					cA = vector3 - (Vector2)Vector3.Slerp((vector5 - vector3).normalized, (vector3 - vector4).normalized, 0.5f) * (Vector2.Distance(vector5, vector3) + Vector2.Distance(vector3, vector4)) * 0.15f;
					cB = vector4 + (Vector2)Vector3.Slerp((vector3 - vector4).normalized, (vector4 - vector6).normalized, 0.5f) * (Vector2.Distance(vector3, vector4) + Vector2.Distance(vector4, vector6)) * 0.15f;
				}
				float f = (float)((i - 30) % meshDivsPerTailSegment) / (float)meshDivsPerTailSegment;
				vector2 = Custom.Bezier(a, cA, b, cB, f);
				num3 = Mathf.Lerp(chunks[num4].rad, chunks[num5].rad, t) / depth;
			}
			Vector2 vector7 = (vector - voidSea.convergencePoint) / depth + voidSea.convergencePoint;
			Vector2 vector8 = (vector2 - voidSea.convergencePoint) / depth + voidSea.convergencePoint;
			Vector2 normalized = (vector - vector2).normalized;
			Vector2 vector9 = Custom.PerpendicularVector(normalized);
			normalized = (vector7 - vector8).normalized;
			vector9 = Custom.PerpendicularVector(normalized);
			float num8 = Vector2.Distance(vector7, vector8) / 5f;
			(sLeaser.sprites[BodySprite] as TriangleMesh).MoveVertice(meshSegments * 4 - 4 - i * 4 + 2, vector7 - normalized * num8 - vector9 * (num3 + num2) * 0.5f);
			(sLeaser.sprites[BodySprite] as TriangleMesh).MoveVertice(meshSegments * 4 - 4 - i * 4 + 3, vector7 - normalized * num8 + vector9 * (num3 + num2) * 0.5f);
			(sLeaser.sprites[BodySprite] as TriangleMesh).MoveVertice(meshSegments * 4 - 4 - i * 4, vector8 + normalized * num8 - vector9 * num3);
			(sLeaser.sprites[BodySprite] as TriangleMesh).MoveVertice(meshSegments * 4 - 4 - i * 4 + 1, vector8 + normalized * num8 + vector9 * num3);
			float g = Mathf.InverseLerp(-1f, 1f, Vector2.Dot(normalized, new Vector2(1f, 0f)));
			for (int j = 0; j < 4; j++)
			{
				(sLeaser.sprites[BodySprite] as TriangleMesh).verticeColors[meshSegments * 4 - 4 - i * 4 + j] = new Color(1f - transparent, g, Mathf.InverseLerp(1f, 8f, meshDivsPerTailSegment), dark);
			}
			vector = vector2;
			num2 = num3;
		}
		for (int k = 0; k < fins.Length; k++)
		{
			for (int l = 0; l < 2; l++)
			{
				vector = Vector2.Lerp(fins[k][l, 0].lastPos, fins[k][l, 0].pos, timeStacker);
				vector = Vector2.Lerp(vector, Vector2.Lerp(chunks[FinConnectChunk(k)].lastPos, chunks[FinConnectChunk(k)].pos, timeStacker), Mathf.Lerp(Mathf.Lerp(1f, 0.85f, graphicFidelity), 1f, (float)k / (float)(fins.Length - 1))) - camPos;
				vector = (vector - voidSea.convergencePoint) / depth + voidSea.convergencePoint;
				num2 = 12f * scale / depth;
				for (int m = 0; m < fins[k].GetLength(1); m++)
				{
					Vector2 vector10 = Vector2.Lerp(fins[k][l, m].lastPos, fins[k][l, m].pos, timeStacker) - camPos;
					vector10 = (vector10 - voidSea.convergencePoint) / depth + voidSea.convergencePoint;
					Vector2 normalized2 = (vector - vector10).normalized;
					Vector2 vector11 = Custom.PerpendicularVector(normalized2) * ((l == 1) ? (-1f) : 1f);
					float num9 = fins[k][l, m].rad / depth;
					float num10 = Vector2.Distance(vector, vector10) / 5f;
					(sLeaser.sprites[FinSprite(k, l)] as TriangleMesh).MoveVertice(m * 4, vector - normalized2 * num10 - vector11 * (num9 + num2) * 0.5f);
					(sLeaser.sprites[FinSprite(k, l)] as TriangleMesh).MoveVertice(m * 4 + 1, vector - normalized2 * num10 + vector11 * (num9 + num2) * 0.5f);
					(sLeaser.sprites[FinSprite(k, l)] as TriangleMesh).MoveVertice(m * 4 + 2, vector10 + normalized2 * num10 - vector11 * num9);
					(sLeaser.sprites[FinSprite(k, l)] as TriangleMesh).MoveVertice(m * 4 + 3, vector10 + normalized2 * num10 + vector11 * num9);
					vector = vector10;
					num2 = num9;
				}
			}
		}
		if (head != null)
		{
			head.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		for (int n = 0; n < arms.Count; n++)
		{
			arms[n].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		for (int num11 = 0; num11 < scales.Count; num11++)
		{
			scales[num11].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		Vector2 vector12 = (Vector2.Lerp(chunks[0].lastPos, chunks[0].pos, timeStacker) + Vector2.Lerp(chunks[1].lastPos, chunks[1].pos, timeStacker)) / 2f - camPos;
		vector12 = (vector12 - voidSea.convergencePoint) / depth + voidSea.convergencePoint;
		for (int num12 = 0; num12 < lightSprites.Length; num12++)
		{
			sLeaser.sprites[lightSprites[num12]].x = vector12.x;
			sLeaser.sprites[lightSprites[num12]].y = vector12.y;
			sLeaser.sprites[lightSprites[num12]].color = new Color(1f - dark * 0.5f, 1f - dark * 0.5f, 1f - dark * 0.5f);
		}
		sLeaser.sprites[lightSprites[0]].scale = scale * Mathf.Lerp(350f, 120f, lightDimmed) / (8f * depth);
		sLeaser.sprites[lightSprites[1]].scale = scale * Mathf.Lerp(300f, 120f, lightDimmed) / (8f * depth);
		sLeaser.sprites[lightSprites[2]].scale = scale * Mathf.Lerp(150f, 80f, lightDimmed) / (8f * depth);
		if (lightAlpha != 1f)
		{
			for (int num13 = 0; num13 < sLeaser.sprites.Length; num13++)
			{
				sLeaser.sprites[num13].alpha = lightAlpha;
				sLeaser.sprites[num13].color = new Color(lightAlpha, lightAlpha, lightAlpha);
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
