using System;
using System.IO;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class VoidChain : UpdatableAndDeletable, IDrawable
{
	public class ChainGraphic : RopeGraphic
	{
		public VoidChain owner;

		public int firstSprite;

		public int sprites;

		public ChainGraphic(VoidChain owner, int segments, int firstSprite)
			: base(segments)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
			LoadGraphic("chain", crispPixels: false, clampWrapMode: false);
			sprites = 1;
		}

		public override void Update()
		{
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].lastPos = segments[i].pos;
				segments[i].pos = owner.segments[i, 0];
			}
		}

		public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
		{
			segments[segment].vel *= 0f;
			if (owner.room.GetTile(smoothedGoalPos).Solid && !owner.room.GetTile(goalPos).Solid)
			{
				FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, owner.room.TileRect(owner.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
				segments[segment].pos = new Vector2(floatRect.left, floatRect.bottom);
			}
			else
			{
				segments[segment].pos = smoothedGoalPos;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: true, "chain");
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(segments[0].lastPos, segments[0].pos, timeStacker);
			vector += Custom.DirVec(Vector2.Lerp(segments[1].lastPos, segments[1].pos, timeStacker), vector) * 1f;
			float num = 4f;
			for (int i = 0; i < segments.Length; i++)
			{
				_ = (float)i / (float)(segments.Length - 1);
				Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
				if (i < segments.Length - 1)
				{
					Vector2.Lerp(segments[i + 1].lastPos, segments[i + 1].pos, timeStacker);
				}
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num - camPos);
				vector = vector2;
			}
			for (int j = 0; j < (sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length; j++)
			{
				float num2 = 1f - Custom.LerpExpEaseIn(0f, 1f, (float)(j / 2) / (float)((sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length / 2));
				(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[j] = new Color(owner.BaseColor.r, owner.BaseColor.g, owner.BaseColor.b, owner.proximityAlpha * num2);
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < (sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length; i++)
			{
				(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[i] = owner.BaseColor;
			}
		}

		public void LoadGraphic(string elementName, bool crispPixels, bool clampWrapMode)
		{
			if (Futile.atlasManager.GetAtlasWithName(elementName) == null)
			{
				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
				string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + elementName + ".png");
				AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode, crispPixels);
				HeavyTexturesCache.LoadAndCacheAtlasFromTexture(elementName, texture2D, textureFromAsset: false);
			}
		}
	}

	public class Halo
	{
		private VoidChain owner;

		public int firstSprite;

		public int totalSprites;

		public int firstLineSprite;

		public int firstSmallCircleSprite;

		public int[][] glyphs;

		public bool[][] dirtyGlyphs;

		public float[][,] glyphPositions;

		public int circles;

		public float[,] rotation;

		public float[,] lines;

		public float[,] smallCircles;

		private float[,] rad;

		private float savDisruption;

		public float activity;

		public int ringsActive;

		public Vector2 pos;

		public Vector2 lastPos;

		private bool firstUpdate;

		public float Speed => Mathf.Lerp(0.2f, 1.8f, activity);

		public Halo(VoidChain owner, int firstSprite)
		{
			circles = 3;
			ringsActive = 2;
			firstUpdate = true;
			this.owner = owner;
			this.firstSprite = firstSprite;
			rad = new float[2, 3];
			rad[0, 0] = 0f;
			rad[0, 1] = 0f;
			rad[0, 2] = 0f;
			rad[1, 0] = 1f;
			rad[1, 1] = 1f;
			rad[1, 2] = 1f;
			glyphs = new int[1][];
			dirtyGlyphs = new bool[glyphs.Length][];
			glyphPositions = new float[glyphs.Length][,];
			for (int i = 0; i < glyphs.Length; i++)
			{
				glyphs[i] = new int[(int)(CircumferenceAtCircle(i * 2, 1f, 0f) / 15f)];
				dirtyGlyphs[i] = new bool[glyphs[i].Length];
				glyphPositions[i] = new float[glyphs[i].Length, 3];
				for (int j = 0; j < glyphs[i].Length; j++)
				{
					glyphs[i][j] = ((UnityEngine.Random.value >= 1f / 30f) ? UnityEngine.Random.Range(0, 7) : (-1));
				}
			}
			rotation = new float[circles, 2];
			for (int k = 0; k < rotation.GetLength(0); k++)
			{
				rotation[k, 0] = UnityEngine.Random.value;
				rotation[k, 1] = rotation[k, 0];
			}
			totalSprites = circles;
			for (int l = 0; l < glyphs.Length; l++)
			{
				totalSprites += glyphs[l].Length;
			}
			firstLineSprite = totalSprites;
			lines = new float[10, 4];
			for (int m = 0; m < lines.GetLength(0); m++)
			{
				lines[m, 0] = UnityEngine.Random.value;
				lines[m, 1] = lines[m, 0];
				lines[m, 2] = UnityEngine.Random.Range(0, circles);
				lines[m, 3] = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
			}
			totalSprites += lines.GetLength(0);
			firstSmallCircleSprite = totalSprites;
			smallCircles = new float[5, 5];
			for (int n = 0; n < smallCircles.GetLength(0); n++)
			{
				smallCircles[n, 0] = UnityEngine.Random.value;
				smallCircles[n, 1] = smallCircles[n, 0];
				smallCircles[n, 2] = UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 6));
				smallCircles[n, 3] = UnityEngine.Random.Range((int)smallCircles[n, 2] + 1, circles);
				smallCircles[n, 4] = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
			}
			totalSprites += smallCircles.GetLength(0);
		}

		public float Circumference(float rad)
		{
			return 2f * rad * (float)Math.PI;
		}

		public float RadAtCircle(float circle, float timeStacker, float disruption)
		{
			return (circle + 1f) * 20f + Mathf.Lerp(rad[0, 1], rad[0, 0], timeStacker) * Mathf.Lerp(rad[1, 1], rad[1, 0], timeStacker) * Mathf.Lerp(1f, UnityEngine.Random.value * disruption, Mathf.Pow(disruption, 2f));
		}

		public float CircumferenceAtCircle(float circle, float timeStacker, float disruption)
		{
			return Circumference(RadAtCircle(circle, timeStacker, disruption));
		}

		public void Update()
		{
			activity = 0f;
			Vector2 stuckPosA = owner.stuckPosA;
			lastPos = pos;
			pos += Vector2.ClampMagnitude(stuckPosA - pos, 10f);
			pos = Vector2.Lerp(pos, stuckPosA, 0.1f);
			if (firstUpdate)
			{
				pos = stuckPosA;
				lastPos = pos;
				firstUpdate = false;
			}
			savDisruption = Mathf.InverseLerp(10f, 150f, Vector2.Distance(pos, stuckPosA));
			for (int i = 0; i < rotation.GetLength(0); i++)
			{
				rotation[i, 1] = rotation[i, 0];
				rotation[i, 0] += 0.2f / Mathf.Max(1f, CircumferenceAtCircle(i, 1f, savDisruption)) * ((i % 2 != 0) ? 1f : (-1f)) * Speed;
			}
			for (int j = 0; j < lines.GetLength(0); j++)
			{
				lines[j, 1] = lines[j, 0];
				lines[j, 0] += 1f / 120f * lines[j, 3] * Speed;
			}
			for (int k = 0; k < smallCircles.GetLength(0); k++)
			{
				smallCircles[k, 1] = smallCircles[k, 0];
				smallCircles[k, 0] += 0.004166667f * smallCircles[k, 4] * Speed;
			}
			for (int l = 0; l < glyphs.Length; l++)
			{
				for (int m = 0; m < glyphs[l].Length; m++)
				{
					glyphPositions[l][m, 1] = glyphPositions[l][m, 0];
					if (UnityEngine.Random.value < Speed / 160f)
					{
						if (UnityEngine.Random.value < 1f / 30f && glyphPositions[l][m, 0] == 0f && glyphs[l][m] > -1)
						{
							if (l == glyphs.Length - 1)
							{
								glyphPositions[l][m, 0] = -1f;
							}
						}
						else
						{
							glyphPositions[l][m, 0] = ((UnityEngine.Random.value >= 0.05f) ? 0f : 1f);
						}
					}
					if (glyphPositions[l][m, 0] == 1f && glyphs[l][m] == -1)
					{
						glyphs[l][m] = UnityEngine.Random.Range(0, 7);
						dirtyGlyphs[l][m] = true;
					}
					if (glyphPositions[l][m, 2] > 0f && glyphs[l][m] > -1)
					{
						glyphPositions[l][m, 2] -= 0.05f;
						glyphs[l][m] = UnityEngine.Random.Range(0, 7);
						dirtyGlyphs[l][m] = true;
					}
				}
			}
			for (int n = 0; n < smallCircles.GetLength(0); n++)
			{
				if (!(UnityEngine.Random.value < Speed / 120f) || !(smallCircles[n, 3] < (float)(ringsActive * 2)))
				{
					continue;
				}
				float num = RadAtCircle(smallCircles[n, 2] - 0.5f, 1f, savDisruption);
				float num2 = RadAtCircle(smallCircles[n, 3] - 0.5f, 1f, savDisruption);
				Vector2 p = Custom.DegToVec(smallCircles[n, 0] * 360f) * Mathf.Lerp(num, num2, 0.5f);
				for (int num3 = 0; num3 < glyphs.Length; num3++)
				{
					for (int num4 = 0; num4 < glyphs[num3].Length; num4++)
					{
						if (Custom.DistLess(p, GlyphPos(num3, num4, 1f), (num2 - num) / 2f))
						{
							glyphPositions[num3][num4, 2] = 1f;
						}
					}
				}
			}
			int num5 = 0;
			for (int num6 = 0; num6 < glyphs[0].Length; num6++)
			{
				if (glyphPositions[0][num6, 0] == 1f)
				{
					num5++;
				}
			}
			if (num5 > 1)
			{
				for (int num7 = 0; num7 < glyphs[0].Length; num7++)
				{
					glyphPositions[0][num7, 0] = 0f;
				}
			}
			for (int num8 = 0; num8 < 2; num8++)
			{
				rad[num8, 1] = rad[num8, 0];
				if (rad[num8, 0] < rad[num8, 2])
				{
					rad[num8, 0] = Mathf.Min(rad[num8, 2], rad[num8, 0] + ((num8 != 0) ? 0.0035714286f : 0.15f));
				}
				else
				{
					rad[num8, 0] = Mathf.Max(rad[num8, 2], rad[num8, 0] - ((num8 != 0) ? 0.0035714286f : 0.15f));
				}
				rad[num8, 0] = Mathf.Lerp(rad[num8, 0], rad[num8, 2], 0.01f);
			}
			if (UnityEngine.Random.value < Speed / 120f)
			{
				rad[0, 2] = ((UnityEngine.Random.value <= activity) ? ((float)UnityEngine.Random.Range(-1, 3) * 20f) : 0f);
				rad[1, 2] = ((UnityEngine.Random.value >= 1f / Mathf.Lerp(1f, 5f, activity)) ? Mathf.Lerp(0.75f, 1.25f, UnityEngine.Random.value) : 1f);
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = 0; i < circles; i++)
			{
				sLeaser.sprites[firstSprite + i] = new FSprite("Futile_White");
				sLeaser.sprites[firstSprite + i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
			}
			int num = circles;
			for (int j = 0; j < glyphs.Length; j++)
			{
				for (int k = 0; k < glyphs[j].Length; k++)
				{
					sLeaser.sprites[firstSprite + num] = new FSprite("haloGlyph" + glyphs[j][k]);
					num++;
				}
			}
			for (int l = 0; l < lines.GetLength(0); l++)
			{
				sLeaser.sprites[firstSprite + firstLineSprite + l] = new FSprite("pixel");
			}
			for (int m = 0; m < smallCircles.GetLength(0); m++)
			{
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + m] = new FSprite("Futile_White");
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + m].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = rCam.ApplyDepth(Vector2.Lerp(lastPos, pos, timeStacker), -5f);
			int num = 0;
			for (int i = firstSprite; i < firstSprite + totalSprites; i++)
			{
				sLeaser.sprites[i].isVisible = true;
				sLeaser.sprites[i].color = owner.BaseColor;
			}
			for (int j = 0; j < circles; j++)
			{
				sLeaser.sprites[firstSprite + j].x = vector.x - camPos.x;
				sLeaser.sprites[firstSprite + j].y = vector.y - camPos.y;
				float num2 = RadAtCircle((float)j - 0.5f, timeStacker, num);
				sLeaser.sprites[firstSprite + j].scale = num2 / 8f;
				sLeaser.sprites[firstSprite + j].alpha = 1f / num2;
				sLeaser.sprites[firstSprite + j].isVisible = j < ringsActive * 2;
			}
			int num3 = circles;
			for (int k = 0; k < glyphs.Length; k++)
			{
				for (int l = 0; l < glyphs[k].Length; l++)
				{
					Vector2 vector2 = vector + GlyphPos(k, l, timeStacker);
					sLeaser.sprites[firstSprite + num3].x = vector2.x - camPos.x;
					sLeaser.sprites[firstSprite + num3].y = vector2.y - camPos.y;
					if (dirtyGlyphs[k][l])
					{
						sLeaser.sprites[firstSprite + num3].element = Futile.atlasManager.GetElementWithName("haloGlyph" + glyphs[k][l]);
						dirtyGlyphs[k][l] = false;
					}
					sLeaser.sprites[firstSprite + num3].isVisible = UnityEngine.Random.value > (float)num && k < ringsActive;
					if (glyphs[k][l] == -1 || (k == 0 && glyphPositions[k][l, 0] == 1f))
					{
						sLeaser.sprites[firstSprite + num3].rotation = 0f;
					}
					else
					{
						sLeaser.sprites[firstSprite + num3].rotation = ((float)l / (float)glyphs[k].Length + Mathf.Lerp(rotation[k, 1], rotation[k, 0], timeStacker)) * 360f;
					}
					num3++;
				}
			}
			for (int m = 0; m < lines.GetLength(0); m++)
			{
				float num4 = Mathf.Lerp(lines[m, 1], lines[m, 0], timeStacker);
				Vector2 vector3 = Custom.DegToVec(num4 * 360f) * RadAtCircle(lines[m, 2] * 2f + 1f, timeStacker, num) + vector;
				sLeaser.sprites[firstSprite + firstLineSprite + m].isVisible = lines[m, 2] < (float)(ringsActive - 1);
				sLeaser.sprites[firstSprite + firstLineSprite + m].rotation = num4 * 360f;
				sLeaser.sprites[firstSprite + firstLineSprite + m].scaleY = RadAtCircle(lines[m, 2] - 0.5f, timeStacker, num) - RadAtCircle(lines[m, 2] + 0.5f, timeStacker, num);
				sLeaser.sprites[firstSprite + firstLineSprite + m].x = vector3.x - camPos.x;
				sLeaser.sprites[firstSprite + firstLineSprite + m].y = vector3.y - camPos.y;
			}
			for (int n = 0; n < smallCircles.GetLength(0); n++)
			{
				float num5 = Mathf.Lerp(smallCircles[n, 1], smallCircles[n, 0], timeStacker);
				float num6 = RadAtCircle(smallCircles[n, 2] - 0.5f, timeStacker, num);
				float num7 = RadAtCircle(smallCircles[n, 3] - 0.5f, timeStacker, num);
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + n].isVisible = smallCircles[n, 3] < (float)(ringsActive * 2);
				Vector2 vector4 = Custom.DegToVec(num5 * 360f) * Mathf.Lerp(num6, num7, 0.5f) + vector;
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + n].x = vector4.x - camPos.x;
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + n].y = vector4.y - camPos.y;
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + n].scale = (num7 - num6) / 16f;
				sLeaser.sprites[firstSprite + firstSmallCircleSprite + n].alpha = 2f / (num7 - num6);
			}
		}

		public Vector2 GlyphPos(int circle, int glyph, float timeStacker)
		{
			if ((float)circle * 2f - Mathf.Lerp(glyphPositions[circle][glyph, 1], glyphPositions[circle][glyph, 0], timeStacker) < 0f)
			{
				return new Vector2(0f, 0f);
			}
			float num = Mathf.Lerp(rotation[circle, 1], rotation[circle, 0], timeStacker);
			return Custom.DegToVec(((float)glyph / (float)glyphs[circle].Length + num) * 360f) * RadAtCircle((float)circle * 2f - Mathf.Lerp(glyphPositions[circle][glyph, 1], glyphPositions[circle][glyph, 0], timeStacker), timeStacker, savDisruption);
		}
	}

	public class ChainFragment : CosmeticSprite
	{
		private float zRotation;

		private float lastZRotation;

		private float zRotVel;

		public int dissapearCounter;

		public float rotation;

		public float lastRotation;

		public float rotVel;

		public float lifetime;

		public ChainFragment(Vector2 pos, Vector2 vel)
		{
			base.pos = pos + vel;
			lastPos = pos;
			base.vel = vel;
			rotation = UnityEngine.Random.value * 360f;
			lastRotation = rotation;
			rotVel = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Custom.LerpMap(vel.magnitude, 0f, 18f, 5f, 26f);
			zRotation = UnityEngine.Random.value * 360f;
			lastZRotation = rotation;
			zRotVel = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Custom.LerpMap(vel.magnitude, 0f, 18f, 2f, 16f);
			dissapearCounter = (int)UnityEngine.Random.Range(0f, 60f);
			lifetime = 80f;
		}

		public override void Update(bool eu)
		{
			lastRotation = rotation;
			rotation += rotVel * Vector2.Distance(lastPos, pos);
			lastZRotation = zRotation;
			zRotation += zRotVel * Vector2.Distance(lastPos, pos);
			if (!Custom.DistLess(lastPos, pos, 3f) && room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
			{
				IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
				FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
				pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
				bool flag = false;
				if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
				{
					vel.x = Mathf.Abs(vel.x) * 0.15f;
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
				{
					vel.x = (0f - Mathf.Abs(vel.x)) * 0.15f;
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
				{
					vel.y = Mathf.Abs(vel.y) * 0.15f;
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
				{
					vel.y = (0f - Mathf.Abs(vel.y)) * 0.15f;
					flag = true;
				}
				if (flag)
				{
					rotVel *= 0.8f;
					zRotVel *= 0.8f;
					if (vel.magnitude > 3f)
					{
						rotVel += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 4f * UnityEngine.Random.value * Mathf.Abs(rotVel / 15f);
						zRotVel += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 4f * UnityEngine.Random.value * Mathf.Abs(rotVel / 15f);
					}
				}
			}
			SharedPhysics.TerrainCollisionData cd2 = SharedPhysics.VerticalCollision(cd: new SharedPhysics.TerrainCollisionData(pos, lastPos, vel, 3f, new IntVector2(0, 0), goThroughFloors: true), room: room);
			cd2 = SharedPhysics.HorizontalCollision(room, cd2);
			pos = cd2.pos;
			vel = cd2.vel;
			if (cd2.contactPoint.x != 0)
			{
				vel.y *= 0.6f;
			}
			if (cd2.contactPoint.y != 0)
			{
				vel.x *= 0.6f;
			}
			vel.x *= 0.95f;
			vel.y *= 0.95f;
			rotVel *= 0.98f;
			zRotVel *= 0.98f;
			dissapearCounter++;
			if ((float)dissapearCounter > lifetime - 10f || pos.x < -100f || pos.y < -100f)
			{
				Destroy();
			}
			base.Update(eu);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			Custom.DegToVec(Mathf.Lerp(lastZRotation, zRotation, timeStacker));
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y;
			sLeaser.sprites[0].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
			sLeaser.sprites[0].x += Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker)).x * 1.5f;
			sLeaser.sprites[0].y += Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker)).y * 1.5f;
			sLeaser.sprites[0].color = RainWorld.SaturatedGold;
			sLeaser.sprites[0].alpha = 1f - (float)dissapearCounter / (lifetime - 10f);
			sLeaser.sprites[0].scale = 0.75f;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("ChainFrag");
			AddToContainer(sLeaser, rCam, null);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public ChainGraphic graphic;

	public float conRad;

	public float pushApart;

	public Vector2[,] segments;

	public Vector2 stuckPosA;

	public Vector2 stuckPosB;

	public float colorFlash;

	public Halo halo;

	public float driftTime;

	public float proximityAlpha;

	public Color BaseColor => new Color(Mathf.Lerp(0.01f, RainWorld.SaturatedGold.r, colorFlash), Mathf.Lerp(0.01f, RainWorld.SaturatedGold.g, colorFlash), Mathf.Lerp(0.01f, RainWorld.SaturatedGold.b, colorFlash));

	public VoidChain(Room room, Vector2 spawnPosA, Vector2 spawnPosB)
	{
		base.room = room;
		pushApart = 0.5f;
		stuckPosA = spawnPosA;
		stuckPosB = spawnPosB;
		segments = new Vector2[128, 3];
		conRad = Custom.Dist(spawnPosA, spawnPosB) / (float)segments.GetLength(0);
		graphic = new ChainGraphic(this, segments.GetLength(0), 0);
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			float t = (float)i / (float)(segments.GetLength(0) - 1);
			segments[i, 0] = Vector2.Lerp(spawnPosA, spawnPosB, t) + Custom.RNV() * UnityEngine.Random.value;
			segments[i, 1] = segments[i, 0];
			segments[i, 2] = Custom.RNV() * UnityEngine.Random.value;
		}
		halo = new Halo(this, 1);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		driftTime += 1f;
		for (int i = 2; i < segments.GetLength(0); i++)
		{
			Vector2 vector = Custom.DirVec(segments[i - 2, 0], segments[i, 0]);
			segments[i - 2, 2] -= vector * pushApart;
			segments[i, 2] += vector * pushApart;
		}
		for (int j = 0; j < segments.GetLength(0); j++)
		{
			segments[j, 2] += 0.2f * new Vector2(Mathf.Sin((float)Math.PI * (driftTime / 1200f)), Mathf.Cos((float)Math.PI * (driftTime / 1200f)));
			segments[j, 1] = segments[j, 0];
			segments[j, 0] += segments[j, 2];
			segments[j, 2] *= 0.599f;
		}
		ConnectToWalls();
		for (int num = segments.GetLength(0) - 1; num > 0; num--)
		{
			Connect(num, num - 1);
		}
		ConnectToWalls();
		for (int k = 1; k < segments.GetLength(0); k++)
		{
			Connect(k, k - 1);
		}
		ConnectToWalls();
		graphic.Update();
		halo.Update();
	}

	private void ConnectToWalls()
	{
		_ = stuckPosA;
		segments[0, 0] = stuckPosA;
		segments[0, 2] *= 0f;
		_ = stuckPosB;
		segments[segments.GetLength(0) - 1, 0] = stuckPosB;
		segments[segments.GetLength(0) - 1, 2] *= 0f;
	}

	private void Connect(int A, int B)
	{
		Vector2 normalized = (segments[A, 0] - segments[B, 0]).normalized;
		float num = Vector2.Distance(segments[A, 0], segments[B, 0]);
		float num2 = Mathf.InverseLerp(0f, conRad, num);
		segments[A, 0] += normalized * (conRad - num) * 0.5f * num2;
		segments[A, 2] += normalized * (conRad - num) * 0.5f * num2;
		segments[B, 0] -= normalized * (conRad - num) * 0.5f * num2;
		segments[B, 2] -= normalized * (conRad - num) * 0.5f * num2;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1 + halo.totalSprites];
		graphic.InitiateSprites(sLeaser, rCam);
		halo.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
		graphic.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		halo.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		graphic.ApplyPalette(sLeaser, rCam, palette);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
		}
	}

	public override void Destroy()
	{
		if (room != null)
		{
			for (int i = 0; i < segments.GetLength(0); i += 4)
			{
				room.AddObject(new ChainFragment(segments[i, 0], Custom.RNV() * UnityEngine.Random.Range(1f, 8f)));
			}
		}
		base.Destroy();
	}
}
