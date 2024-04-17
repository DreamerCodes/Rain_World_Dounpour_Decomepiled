using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class NeedleEgg : PlayerCarryableItem, IDrawable
{
	public class Shell
	{
		public int sprite;

		public float flip;

		private int segments;

		public float[,] uneven;

		public Shell(int sprite, float flip)
		{
			this.sprite = sprite;
			this.flip = flip;
			segments = UnityEngine.Random.Range(7, 10);
			uneven = new float[segments, 2];
			for (int i = 0; i < uneven.GetLength(0); i++)
			{
				uneven[i, 0] = UnityEngine.Random.value;
				uneven[i, 1] = UnityEngine.Random.value * 0.5f;
			}
			uneven[UnityEngine.Random.Range(0, uneven.GetLength(0)), 1] = 1f;
			uneven[UnityEngine.Random.Range(0, uneven.GetLength(0)), 1] = 1f;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[sprite] = TriangleMesh.MakeLongMesh(segments, pointyTip: false, customColor: false);
			sLeaser.sprites[sprite + 1] = TriangleMesh.MakeLongMesh(segments - 4, pointyTip: false, customColor: false);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			sLeaser.sprites[sprite].color = palette.blackColor;
			sLeaser.sprites[sprite + 1].color = Color.Lerp(palette.blackColor, palette.fogColor, 0.2f);
		}

		public void Draw(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 drwPos, Vector2 rotVec, Vector2 prp)
		{
			Vector2 vector = drwPos - rotVec * 12f;
			float num = 1f;
			for (int i = 0; i < segments; i++)
			{
				float num2 = Mathf.InverseLerp(0f, segments - 1, i);
				float num3 = 0.5f + Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(num2, 0.6f) * (float)Math.PI)), 0.4f) * (2.5f + uneven[i, 0]);
				float num4 = num3 + 1f + Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(num2, 0.9f) * (float)Math.PI)), 1.5f) * (1.5f + 2f * uneven[i, 1]);
				if (i == 0)
				{
					num3 = 0f;
				}
				Vector2 vector2 = drwPos + rotVec * Mathf.Lerp(-10f, 11f, num2) + prp * (num4 + num3) * 0.5f * flip;
				float num5 = num4 - num3;
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 normalized2 = (Custom.PerpendicularVector(normalized) + prp).normalized;
				float num6 = Vector2.Distance(vector2, vector) / 5f;
				(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4, vector - normalized2 * (num + num5) * 0.5f + normalized * num6 - camPos);
				(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + normalized2 * (num + num5) * 0.5f + normalized * num6 - camPos);
				(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - normalized2 * num5 - normalized * num6 - camPos);
				(sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized2 * num5 - normalized * num6 - camPos);
				num = num5;
				vector = vector2;
			}
			num = 1f;
			for (int j = 2; j < segments - 2; j++)
			{
				float num7 = Mathf.InverseLerp(0f, segments - 1, j);
				float num8 = 0.5f + Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(num7, 0.6f) * (float)Math.PI)), 0.4f) * (2.5f + uneven[j, 0]);
				float num9 = num8 + 1f + Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(num7, 0.9f) * (float)Math.PI)), 1.5f) * (1.5f + 2f * uneven[j, 1]);
				num8 = Mathf.Lerp(num8, num9, 0.5f);
				Vector2 vector3 = drwPos + rotVec * Mathf.Lerp(-10f, 11f, num7) + prp * (num9 + num8) * 0.5f * flip;
				if (j == 2)
				{
					vector = vector3 - rotVec;
				}
				vector3 += Custom.DegToVec(-45f) * 1.5f;
				float num10 = (num9 - num8) * Mathf.Lerp(0.2f, 0.4f, Mathf.Sin(Mathf.InverseLerp(2f, segments - 3, j) * (float)Math.PI));
				Vector2 normalized3 = (vector3 - vector).normalized;
				Vector2 normalized4 = (Custom.PerpendicularVector(normalized3) + prp).normalized;
				float num11 = Vector2.Distance(vector3, vector) / 5f;
				(sLeaser.sprites[sprite + 1] as TriangleMesh).MoveVertice((j - 2) * 4, vector - normalized4 * (num + num10) * 0.5f + normalized3 * num11 - camPos);
				(sLeaser.sprites[sprite + 1] as TriangleMesh).MoveVertice((j - 2) * 4 + 1, vector + normalized4 * (num + num10) * 0.5f + normalized3 * num11 - camPos);
				(sLeaser.sprites[sprite + 1] as TriangleMesh).MoveVertice((j - 2) * 4 + 2, vector3 - normalized4 * num10 - normalized3 * num11 - camPos);
				(sLeaser.sprites[sprite + 1] as TriangleMesh).MoveVertice((j - 2) * 4 + 3, vector3 + normalized4 * num10 - normalized3 * num11 - camPos);
				num = num10;
				vector = vector3;
			}
		}
	}

	public class Stalk : UpdatableAndDeletable, IDrawable
	{
		public NeedleEgg egg;

		public Vector2 stuckPos;

		public float ropeLength;

		public Vector2[,] segs;

		public int releaseCounter;

		private float connRad;

		private Color eggColor;

		private float pulse;

		private float lastPulse;

		private Color blackColor;

		private Color useColor;

		public Stalk(NeedleEgg egg, Room room, Vector2 eggPos)
		{
			this.egg = egg;
			egg.firstChunk.HardSetPosition(eggPos);
			eggColor = egg.color;
			stuckPos.x = eggPos.x;
			ropeLength = -1f;
			int x = room.GetTilePosition(eggPos).x;
			for (int i = room.GetTilePosition(eggPos).y; i < room.TileHeight; i++)
			{
				if (room.GetTile(x, i).Solid)
				{
					stuckPos.y = room.MiddleOfTile(x, i).y - 10f;
					ropeLength = Mathf.Abs(stuckPos.y - eggPos.y);
					break;
				}
			}
			segs = new Vector2[Math.Max(1, (int)(ropeLength / 13f)), 3];
			for (int j = 0; j < segs.GetLength(0); j++)
			{
				float t = (float)j / (float)(segs.GetLength(0) - 1);
				segs[j, 0] = Vector2.Lerp(stuckPos, eggPos, t);
				segs[j, 1] = segs[j, 0];
			}
			connRad = ropeLength / Mathf.Pow(segs.GetLength(0), 1.1f);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (ropeLength == -1f)
			{
				Destroy();
				return;
			}
			lastPulse = pulse;
			pulse += UnityEngine.Random.value / Mathf.Lerp(0.2f * ropeLength, 40f, 0.5f);
			if (pulse > 1.5f)
			{
				pulse = 0f;
				lastPulse = 0f;
			}
			ConnectSegments(dir: true);
			ConnectSegments(dir: false);
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				segs[i, 1] = segs[i, 0];
				segs[i, 0] += segs[i, 2];
				segs[i, 2] *= 0.99f;
				segs[i, 2].y -= 0.9f;
			}
			if (UnityEngine.Random.value < 1f / Custom.LerpMap(pulse, 0f, 1f, 300f, 20f))
			{
				for (int num = UnityEngine.Random.Range(0, segs.GetLength(0)); num >= 0; num--)
				{
					segs[UnityEngine.Random.Range(0, segs.GetLength(0)), 2] += Custom.RNV() * UnityEngine.Random.value * (1f + pulse);
				}
			}
			ConnectSegments(dir: false);
			ConnectSegments(dir: true);
			List<Vector2> list = new List<Vector2>();
			list.Add(stuckPos);
			for (int j = 0; j < segs.GetLength(0); j++)
			{
				list.Add(segs[j, 0]);
			}
			if (releaseCounter > 0)
			{
				releaseCounter--;
			}
			if (egg != null)
			{
				list.Add(egg.firstChunk.pos);
				egg.setRotation = Custom.DirVec(egg.firstChunk.pos, segs[segs.GetLength(0) - 1, 0]);
				if (!Custom.DistLess(egg.firstChunk.pos, stuckPos, ropeLength * 1.2f + 20f) || egg.slatedForDeletetion || egg.room != room || releaseCounter == 1)
				{
					egg.AbstrConsumable.Consume();
					egg = null;
				}
			}
		}

		private void ConnectSegments(bool dir)
		{
			int num = ((!dir) ? (segs.GetLength(0) - 1) : 0);
			bool flag = false;
			while (!flag)
			{
				if (num == 0)
				{
					if (!Custom.DistLess(segs[num, 0], stuckPos, connRad))
					{
						Vector2 vector = Custom.DirVec(segs[num, 0], stuckPos) * (Vector2.Distance(segs[num, 0], stuckPos) - connRad);
						segs[num, 0] += vector;
						segs[num, 2] += vector;
					}
				}
				else
				{
					if (!Custom.DistLess(segs[num, 0], segs[num - 1, 0], connRad))
					{
						Vector2 vector2 = Custom.DirVec(segs[num, 0], segs[num - 1, 0]) * (Vector2.Distance(segs[num, 0], segs[num - 1, 0]) - connRad);
						segs[num, 0] += vector2 * 0.5f;
						segs[num, 2] += vector2 * 0.5f;
						segs[num - 1, 0] -= vector2 * 0.5f;
						segs[num - 1, 2] -= vector2 * 0.5f;
					}
					if (num == segs.GetLength(0) - 1 && egg != null && !Custom.DistLess(segs[num, 0], egg.firstChunk.pos, connRad))
					{
						Vector2 vector3 = Custom.DirVec(segs[num, 0], egg.firstChunk.pos) * (Vector2.Distance(segs[num, 0], egg.firstChunk.pos) - connRad);
						segs[num, 0] += vector3 * 0.75f;
						segs[num, 2] += vector3 * 0.75f;
						egg.firstChunk.vel -= vector3 * 0.25f;
					}
				}
				num += (dir ? 1 : (-1));
				if (dir && num >= segs.GetLength(0))
				{
					flag = true;
				}
				else if (!dir && num < 0)
				{
					flag = true;
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segs.GetLength(0), pointyTip: false, customColor: true);
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = stuckPos;
			float num = 1.5f;
			float num2 = Mathf.Lerp(lastPulse, pulse, timeStacker);
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float num3 = (float)i / (float)(segs.GetLength(0) - 1);
				float num4 = Mathf.InverseLerp(0.5f, 0.1f, Mathf.Abs(num3 - num2));
				float num5 = ((i % 2 == 0 && (num3 < 1f || egg != null)) ? Mathf.Lerp(1.5f, 2.5f + num4, num3) : 0.5f);
				Vector2 vector2 = Vector2.Lerp(segs[i, 1], segs[i, 0], timeStacker);
				if (i == segs.GetLength(0) - 1 && egg != null)
				{
					vector2 = Vector2.Lerp(egg.firstChunk.lastPos, egg.firstChunk.pos, timeStacker);
				}
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				vector2 = new Vector2(Mathf.Floor(vector2.x) + 0.5f, Mathf.Floor(vector2.y) + 0.5f);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num5 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num5 - camPos);
				for (int j = 0; j < 4; j++)
				{
					(sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + j] = ((i % 2 == 0 == j < 2) ? blackColor : Color.Lerp(blackColor, useColor, num3 * num4));
				}
				vector = vector2;
				num = num5;
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			blackColor = palette.blackColor;
			useColor = Color.Lerp(Color.Lerp(eggColor, palette.fogColor, 0.35f), new Color(0f, 0f, 0f), 0.15f);
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].color = palette.blackColor;
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public class EggHalf : UpdatableAndDeletable, IDrawable
	{
		public SmallNeedleWorm worm;

		public Vector2[,] positions;

		public Vector2[][,] slime;

		private float[] slimeAttaches;

		private float[] onWormAttaches;

		private float[] slimeLengths;

		public Shell shell;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public EggHalf(SmallNeedleWorm worm, Room room)
		{
			this.worm = worm;
			positions = new Vector2[2, 3];
			for (int i = 0; i < positions.GetLength(0); i++)
			{
				positions[i, 0] = worm.mainBodyChunk.pos + Custom.RNV();
				positions[i, 1] = worm.mainBodyChunk.pos + Custom.RNV();
			}
			slime = new Vector2[UnityEngine.Random.Range(2, 6)][,];
			slimeAttaches = new float[slime.Length];
			onWormAttaches = new float[slime.Length];
			slimeLengths = new float[slime.Length];
			for (int j = 0; j < slime.Length; j++)
			{
				slimeAttaches[j] = UnityEngine.Random.value;
				slime[j] = new Vector2[UnityEngine.Random.Range(3, 8), 3];
				onWormAttaches[j] = ((UnityEngine.Random.value < 0.35f) ? UnityEngine.Random.value : (-1f));
				slimeLengths[j] = Mathf.Lerp(1f, 4f, UnityEngine.Random.value);
				for (int k = 0; k < slime[j].GetLength(0); k++)
				{
					slime[j][k, 0] = positions[0, 0] + Custom.RNV();
					slime[j][k, 1] = slime[j][k, 0];
				}
			}
			if (onWormAttaches.Length != 0)
			{
				onWormAttaches[UnityEngine.Random.Range(0, onWormAttaches.Length)] = UnityEngine.Random.value;
			}
			shell = new Shell(slime.Length, (UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < positions.GetLength(0); i++)
			{
				positions[i, 1] = positions[i, 0];
				positions[i, 0] += positions[i, 2];
				positions[i, 2] *= 0.95f;
				positions[i, 2].y -= 0.9f;
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(positions[i, 0], positions[i, 1], positions[i, 2], 4f, new IntVector2(0, 0), goThroughFloors: true);
				cd = SharedPhysics.VerticalCollision(room, cd);
				cd = SharedPhysics.HorizontalCollision(room, cd);
				positions[i, 0] = cd.pos;
				positions[i, 2] = cd.vel;
				if (cd.contactPoint.y < 0)
				{
					positions[i, 2].x *= 0.5f;
				}
			}
			Vector2 vector = Custom.DirVec(positions[0, 0], positions[1, 0]) * (Vector2.Distance(positions[0, 0], positions[1, 0]) - 10f);
			positions[0, 0] += vector * 0.5f;
			positions[0, 2] += vector * 0.5f;
			positions[1, 0] -= vector * 0.5f;
			positions[1, 2] -= vector * 0.5f;
			Vector2 vector2 = Custom.PerpendicularVector(positions[0, 0], positions[1, 0]) * shell.flip;
			for (int j = 0; j < slime.Length; j++)
			{
				Vector2 normalized = (vector2 + Custom.DirVec(positions[0, 0], positions[1, 0]) * Mathf.Lerp(-1f, 1f, slimeAttaches[j])).normalized;
				Vector2 vector3 = Vector2.Lerp(positions[0, 0], positions[1, 0], slimeAttaches[j]) + normalized * 3f;
				for (int k = 0; k < slime[j].GetLength(0); k++)
				{
					slime[j][k, 1] = slime[j][k, 0];
					slime[j][k, 0] += slime[j][k, 2];
					slime[j][k, 2] *= 0.98f;
					slime[j][k, 2].y -= 0.9f;
					slime[j][k, 2] += normalized * Mathf.InverseLerp(4f, 0f, k) * 2f;
					SharedPhysics.TerrainCollisionData cd2 = scratchTerrainCollisionData.Set(slime[j][k, 0], slime[j][k, 1], slime[j][k, 2], 1f, new IntVector2(0, 0), goThroughFloors: true);
					cd2 = SharedPhysics.VerticalCollision(room, cd2);
					cd2 = SharedPhysics.HorizontalCollision(room, cd2);
					slime[j][k, 0] = cd2.pos;
					slime[j][k, 2] = cd2.vel;
					if (cd2.contactPoint.y < 0)
					{
						slime[j][k, 2].x *= 0.5f;
					}
					if (k > 0)
					{
						vector = Custom.DirVec(slime[j][k, 0], slime[j][k - 1, 0]) * (Vector2.Distance(slime[j][k, 0], slime[j][k - 1, 0]) - 2f);
						slime[j][k, 0] += vector * 0.3f;
						slime[j][k, 2] += vector * 0.3f;
						slime[j][k - 1, 0] -= vector * 0.3f;
						slime[j][k - 1, 2] -= vector * 0.3f;
					}
				}
				vector = Custom.DirVec(slime[j][0, 0], vector3) * (Vector2.Distance(slime[j][0, 0], vector3) - slimeLengths[j]);
				slime[j][0, 0] += vector;
				slime[j][0, 2] += vector;
				if (worm != null && onWormAttaches[j] >= 0f)
				{
					slime[j][slime[j].GetLength(0) - 1, 0] = worm.OnBodyPos(onWormAttaches[j], 1f);
					slime[j][slime[j].GetLength(0) - 1, 2] *= 0f;
				}
			}
			if (worm == null)
			{
				return;
			}
			if (UnityEngine.Random.value < 1f / Mathf.Lerp(400f, 40f, worm.flying) && onWormAttaches.Length != 0)
			{
				onWormAttaches[UnityEngine.Random.Range(0, onWormAttaches.Length)] = -1f;
			}
			bool flag = false;
			for (int l = 0; l < onWormAttaches.Length; l++)
			{
				if (onWormAttaches[l] >= 0f)
				{
					flag = true;
					if (Vector2.Distance(positions[0, 0], worm.OnBodyPos(onWormAttaches[l], 1f)) > (float)slime[l].GetLength(0) * slimeLengths[l])
					{
						vector = Custom.DirVec(positions[0, 0], worm.OnBodyPos(onWormAttaches[l], 1f)) * (Vector2.Distance(positions[0, 0], worm.OnBodyPos(onWormAttaches[l], 1f)) - (float)slime[l].GetLength(0) * 4f);
						positions[0, 0] += vector * 0.07f;
						positions[0, 2] += vector * 0.07f;
					}
				}
			}
			if (!flag || worm.slatedForDeletetion || worm.enteringShortCut.HasValue)
			{
				worm = null;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2 + slime.Length];
			shell.InitiateSprites(sLeaser, rCam);
			for (int i = 0; i < slime.Length; i++)
			{
				sLeaser.sprites[i] = TriangleMesh.MakeLongMesh(slime[i].GetLength(0), pointyTip: false, customColor: false);
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(positions[0, 1], positions[0, 0], timeStacker);
			Vector2 vector2 = Vector2.Lerp(positions[1, 1], positions[1, 0], timeStacker);
			Vector2 vector3 = Custom.PerpendicularVector(Custom.DirVec(vector, vector2));
			shell.Draw(sLeaser, rCam, timeStacker, camPos, (vector + vector2) / 2f - vector3 * 4f * shell.flip, Custom.DirVec(vector, vector2), vector3);
			for (int i = 0; i < slime.Length; i++)
			{
				_ = (vector3 + Custom.DirVec(vector, vector2) * Mathf.Lerp(-1f, 1f, slimeAttaches[i])).normalized;
				Vector2 vector4 = Vector2.Lerp(vector, vector2, slimeAttaches[i]);
				Vector2 vector5 = vector4;
				float num = 1f;
				Vector2? vector6 = null;
				float num2 = 0f;
				if (worm != null && onWormAttaches[i] >= 0f)
				{
					vector6 = worm.OnBodyPos(onWormAttaches[i], timeStacker);
					num2 = Mathf.InverseLerp((float)slime[i].GetLength(0) * slimeLengths[i] * 0.8f, (float)slime[i].GetLength(0) * slimeLengths[i] * 3f, Vector2.Distance(vector4, vector6.Value));
				}
				else
				{
					num2 = Mathf.InverseLerp((float)slime[i].GetLength(0) * slimeLengths[i] * 0.8f, (float)slime[i].GetLength(0) * slimeLengths[i] * 3f, Vector2.Distance(vector4, slime[i][slime[i].GetLength(0) - 1, 0]));
				}
				for (int j = 0; j < slime[i].GetLength(0); j++)
				{
					float num3 = Mathf.InverseLerp(0f, slime[i].GetLength(0) - 1, j);
					Vector2 vector7 = Vector2.Lerp(slime[i][j, 1], slime[i][j, 0], timeStacker);
					if (vector6.HasValue)
					{
						if (num3 == 1f)
						{
							vector7 = vector6.Value;
						}
						vector7 = Vector2.Lerp(vector7, Vector2.Lerp(vector4, vector6.Value, num3), num2);
					}
					float num4 = 1.2f - Mathf.Pow(Mathf.Clamp01(Mathf.Sin(num3 * (float)Math.PI)), Mathf.Lerp(1f, 0.3f, num2)) * Mathf.Lerp(0.1f, 0.95f, num2);
					if (num3 == 1f)
					{
						num4 /= 2f;
					}
					Vector2 normalized = (vector7 - vector5).normalized;
					Vector2 vector8 = Custom.PerpendicularVector(normalized);
					float num5 = Vector2.Distance(vector7, vector5) / 5f;
					(sLeaser.sprites[i] as TriangleMesh).MoveVertice(j * 4, vector5 - vector8 * (num + num4) * 0.5f + normalized * num5 - camPos);
					(sLeaser.sprites[i] as TriangleMesh).MoveVertice(j * 4 + 1, vector5 + vector8 * (num + num4) * 0.5f + normalized * num5 - camPos);
					(sLeaser.sprites[i] as TriangleMesh).MoveVertice(j * 4 + 2, vector7 - vector8 * num4 - normalized * num5 - camPos);
					(sLeaser.sprites[i] as TriangleMesh).MoveVertice(j * 4 + 3, vector7 + vector8 * num4 - normalized * num5 - camPos);
					num = num4;
					vector5 = vector7;
				}
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			shell.ApplyPalette(sLeaser, rCam, palette);
			for (int i = 0; i < slime.Length; i++)
			{
				sLeaser.sprites[i].color = Color.Lerp(new Color(1f, 0.5f, 0.4f), palette.fogColor, 0.3f);
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Items");
			}
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public Stalk stalk;

	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float darkness;

	public float lastDarkness;

	public Shell[] halves;

	private float breath;

	private float wiggle;

	private float GTwiggle;

	private Vector2[,] shellpositions;

	private Color blackCol;

	private Color fogColor;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public NeedleEgg(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 9f, 0.4f);
		bodyChunkConnections = new BodyChunkConnection[0];
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		halves = new Shell[2];
		for (int i = 0; i < halves.Length; i++)
		{
			halves[i] = new Shell(2 + i * 2, (i == 0) ? (-1f) : 1f);
		}
		color = Custom.HSL2RGB(Custom.Decimal(1f + Mathf.Lerp(-0.04f, 0.05f, UnityEngine.Random.value)), Mathf.Lerp(0.8f, 1f, UnityEngine.Random.value), 0.5f);
		UnityEngine.Random.state = state;
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.7f;
		collisionLayer = 1;
		base.waterFriction = 0.95f;
		base.buoyancy = 1.1f;
		breath = UnityEngine.Random.value;
		shellpositions = new Vector2[2, 3];
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Futile.mousePosition) * 3f;
		}
		lastRotation = rotation;
		if (grabbedBy.Count > 0)
		{
			rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			rotation.y = Mathf.Abs(rotation.y);
		}
		if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		if (base.firstChunk.ContactPoint.y < 0)
		{
			rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * base.firstChunk.vel.x).normalized;
			base.firstChunk.vel.x *= 0.8f;
		}
		if (UnityEngine.Random.value < 1f / 60f)
		{
			GTwiggle = UnityEngine.Random.value * UnityEngine.Random.value;
		}
		else
		{
			GTwiggle = Mathf.Max(0f, GTwiggle - 0.02f);
		}
		wiggle = Custom.LerpAndTick(wiggle, GTwiggle, 0.03f, 0.025f);
		if (wiggle > 0f)
		{
			base.firstChunk.vel += Custom.RNV() * UnityEngine.Random.value * 2f * wiggle;
			rotation = (rotation + Custom.RNV() * UnityEngine.Random.value * 0.3f * wiggle).normalized;
			for (int i = 0; i < shellpositions.GetLength(0); i++)
			{
				shellpositions[i, 2] += Custom.RNV() * UnityEngine.Random.value * wiggle * 0.3f;
			}
		}
		for (int j = 0; j < shellpositions.GetLength(0); j++)
		{
			shellpositions[j, 1] = shellpositions[j, 0];
			shellpositions[j, 0] += shellpositions[j, 2];
			shellpositions[j, 2] *= 0.8f;
			if (shellpositions[j, 0].magnitude > 1f)
			{
				shellpositions[j, 2] -= shellpositions[j, 0].normalized * (shellpositions[j, 0].magnitude - 1f) * 0.7f;
				shellpositions[j, 0] -= shellpositions[j, 0].normalized * (shellpositions[j, 0].magnitude - 1f) * 0.7f;
			}
			shellpositions[j, 0] *= 0.99f;
		}
		breath += 1f;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			stalk = new Stalk(this, placeRoom, base.firstChunk.pos);
			placeRoom.AddObject(stalk);
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			rotation = Custom.RNV();
			lastRotation = rotation;
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (stalk != null && stalk.releaseCounter == 0)
		{
			stalk.releaseCounter = UnityEngine.Random.Range(3, 15);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[6];
		sLeaser.sprites[0] = new FSprite("DangleFruit0A");
		sLeaser.sprites[1] = new FSprite("DangleFruit0B");
		sLeaser.sprites[0].scaleY = 1.3f;
		sLeaser.sprites[1].scaleY = 1.3f;
		sLeaser.sprites[1].scaleX = 1.3f;
		for (int i = 0; i < 2; i++)
		{
			halves[i].InitiateSprites(sLeaser, rCam);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		Vector2 prp = Custom.PerpendicularVector(vector2);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i].x = vector.x - camPos.x;
			sLeaser.sprites[i].y = vector.y - camPos.y;
			sLeaser.sprites[i].rotation = Custom.VecToDeg(vector2);
		}
		for (int j = 0; j < 2; j++)
		{
			Vector2 vector3 = vector + Custom.RotateAroundOrigo(Vector2.Lerp(shellpositions[j, 1], shellpositions[j, 0], timeStacker) * 1.2f, Custom.VecToDeg(vector2));
			halves[j].Draw(sLeaser, rCam, timeStacker, camPos, vector3, -Custom.DirVec(vector3, vector - vector2 * 20f), prp);
		}
		if (blink > 0 && UnityEngine.Random.value < 0.5f)
		{
			sLeaser.sprites[1].color = new Color(1f, 1f, 1f);
		}
		else
		{
			sLeaser.sprites[1].color = Color.Lerp(color, blackCol, Mathf.Pow(0.4f + 0.4f * Mathf.Sin((breath + timeStacker) / 20f), 1.75f));
		}
		sLeaser.sprites[0].color = Color.Lerp(blackCol, Color.Lerp(color, fogColor, 0.4f), Mathf.Pow(0.5f + 0.4f * Mathf.Sin((breath + timeStacker) / 20f), 1.5f));
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackCol = palette.blackColor;
		fogColor = palette.fogColor;
		for (int i = 0; i < halves.Length; i++)
		{
			halves[i].ApplyPalette(sLeaser, rCam, palette);
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
