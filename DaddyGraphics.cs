using System;
using System.Collections.Generic;
using Noise;
using RWCustom;
using UnityEngine;

public class DaddyGraphics : GraphicsModule
{
	public interface DaddyBubbleOwner
	{
		Color GetColor();

		Vector2 GetPosition();
	}

	public class Eye : DaddyBubbleOwner
	{
		private int index;

		public Vector2 dir;

		public Vector2 lastDir;

		public float focus;

		public float lastFocus;

		public float getToFocus;

		public float closed;

		public float lastClosed;

		private int eyesClosedDelay;

		public DaddyGraphics owner;

		public NoiseTracker.TheorizedSource soundSource;

		public Tracker.CreatureRepresentation creatureRep;

		public float light;

		public float flash;

		public Vector2 centerRenderPos;

		public Color renderColor;

		public BodyChunk chunk => owner.daddy.bodyChunks[index];

		public Eye(DaddyGraphics owner, int index)
		{
			this.index = index;
			this.owner = owner;
			dir = new Vector2(0f, 0f);
			lastDir = new Vector2(0f, 0f);
			centerRenderPos = owner.daddy.bodyChunks[index].pos;
		}

		public void Update()
		{
			lastDir = dir;
			lastClosed = closed;
			closed = Mathf.Max(Mathf.Lerp(closed, Mathf.InverseLerp(0f, eyesClosedDelay, owner.daddy.eyesClosed), 1f / (float)eyesClosedDelay), owner.daddy.Deaf);
			if (owner.daddy.eyesClosed == 0)
			{
				eyesClosedDelay = UnityEngine.Random.Range(1, 20);
			}
			if (ModManager.MMF && owner.daddy.dead)
			{
				eyesClosedDelay = Mathf.Min(eyesClosedDelay + 2, 15);
			}
			Vector2 vector = new Vector2(0f, 0f);
			if (soundSource != null)
			{
				vector = soundSource.pos;
				if (soundSource.slatedForDeletion)
				{
					soundSource = null;
				}
			}
			else if (creatureRep != null)
			{
				vector = ((!creatureRep.VisualContact) ? owner.daddy.room.MiddleOfTile(creatureRep.BestGuessForPosition()) : creatureRep.representedCreature.realizedCreature.DangerPos);
				if (creatureRep.deleteMeNextFrame)
				{
					creatureRep = null;
				}
			}
			float num = 0f;
			if (vector.x != 0f && vector.y != 0f)
			{
				dir = Vector3.Slerp(dir, Custom.DirVec(chunk.pos, vector) * Mathf.InverseLerp(0f, 200f, Vector2.Distance(chunk.pos, vector)), 0.3f);
				num = light * Mathf.InverseLerp(0f, 1f, Vector2.Distance(lastDir, dir));
				light = Mathf.Max(owner.daddy.dead ? 0f : 0.2f, light - 0.05f);
			}
			else
			{
				dir *= 0.9f;
				light = Mathf.Max(owner.daddy.dead ? 0f : 0.1f, light - 0.05f);
				FindNewLookObject();
			}
			flash = Mathf.Max(0f, flash - 1f / 6f);
			if (UnityEngine.Random.value < num)
			{
				getToFocus = Mathf.Max(getToFocus, UnityEngine.Random.value);
			}
			else if (UnityEngine.Random.value < 1f / 70f)
			{
				getToFocus = 0f;
			}
			lastFocus = focus;
			if (focus < getToFocus)
			{
				focus = Mathf.Min(focus + 0.05f, getToFocus);
			}
			else
			{
				focus = Mathf.Max(focus - 0.05f, getToFocus);
			}
		}

		private void FindNewLookObject()
		{
			bool flag = false;
			if (owner.daddy.AI.tracker.CreaturesCount > 0)
			{
				flag = true;
				Tracker.CreatureRepresentation rep = owner.daddy.AI.tracker.GetRep(UnityEngine.Random.Range(0, owner.daddy.AI.tracker.CreaturesCount));
				for (int i = 0; i < owner.eyes.Length && flag; i++)
				{
					if (owner.eyes[i].creatureRep == rep)
					{
						flag = false;
					}
				}
				if (flag)
				{
					creatureRep = rep;
					light = Mathf.Max(0.75f, light);
				}
			}
			if (!owner.SizeClass || flag || !((float)owner.daddy.AI.noiseTracker.sources.Count > 0f))
			{
				return;
			}
			flag = true;
			NoiseTracker.TheorizedSource theorizedSource = owner.daddy.AI.noiseTracker.sources[UnityEngine.Random.Range(0, owner.daddy.AI.noiseTracker.sources.Count)];
			for (int j = 0; j < owner.eyes.Length && flag; j++)
			{
				if (owner.eyes[j].soundSource == theorizedSource)
				{
					flag = false;
				}
			}
			if (flag)
			{
				soundSource = theorizedSource;
				light = Mathf.Max(0.5f, light);
			}
		}

		public void ReactToSound(NoiseTracker.TheorizedSource newSound)
		{
			creatureRep = null;
			soundSource = newSound;
			light = 1f;
			flash = 1f;
		}

		public void ReactToCreature(Tracker.CreatureRepresentation newCrit)
		{
			soundSource = null;
			creatureRep = newCrit;
			light = Mathf.Max(light, UnityEngine.Random.value);
		}

		public Color GetColor()
		{
			return renderColor;
		}

		public Vector2 GetPosition()
		{
			return centerRenderPos;
		}
	}

	public abstract class DaddyTubeGraphic : RopeGraphic
	{
		public struct Bump
		{
			public Vector2 pos;

			public float size;

			public float eyeSize;

			public Bump(Vector2 pos, float size, float eyeSize)
			{
				this.pos = pos;
				this.size = size;
				this.eyeSize = eyeSize;
			}
		}

		public DaddyGraphics owner;

		public int firstSprite;

		public int sprites;

		public Bump[] bumps;

		public DaddyTubeGraphic(DaddyGraphics owner, int segments, int firstSprite)
			: base(segments)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
		}

		public override void Update()
		{
		}

		public override void ConnectPhase(float totalRopeLength)
		{
		}

		public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
		{
			segments[segment].vel *= 0f;
			if (owner.daddy.room.GetTile(smoothedGoalPos).Solid && !owner.daddy.room.GetTile(goalPos).Solid)
			{
				FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, owner.daddy.room.TileRect(owner.daddy.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
				segments[segment].pos = new Vector2(floatRect.left, floatRect.bottom);
			}
			else
			{
				segments[segment].pos = smoothedGoalPos;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMeshAtlased(segments.Length, pointyTip: false, customColor: true);
			int num = 0;
			for (int i = 0; i < bumps.Length; i++)
			{
				sLeaser.sprites[firstSprite + 1 + i] = new FSprite("Circle20", quadType: false);
				sLeaser.sprites[firstSprite + 1 + i].scale = Mathf.Lerp(2f, 6f, bumps[i].size) / 10f;
				if (bumps[i].eyeSize > 0f)
				{
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num] = new FSprite("Circle20", quadType: false);
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num].scale = Mathf.Lerp(2f, 6f, bumps[i].size) * bumps[i].eyeSize / 10f;
					num++;
				}
			}
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(segments[0].lastPos, segments[0].pos, timeStacker);
			vector += Custom.DirVec(Vector2.Lerp(segments[1].lastPos, segments[1].pos, timeStacker), vector) * 1f;
			float num = (owner.SizeClass ? 2f : 1.7f);
			for (int i = 0; i < segments.Length; i++)
			{
				Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num - camPos);
				vector = vector2;
			}
			int num2 = 0;
			for (int j = 0; j < bumps.Length; j++)
			{
				Vector2 vector4 = OnTubePos(bumps[j].pos, timeStacker);
				sLeaser.sprites[firstSprite + 1 + j].x = vector4.x - camPos.x;
				sLeaser.sprites[firstSprite + 1 + j].y = vector4.y - camPos.y;
				if (bumps[j].eyeSize > 0f)
				{
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num2].x = vector4.x - camPos.x;
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num2].y = vector4.y - camPos.y;
					num2++;
				}
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color color = palette.blackColor;
			if (owner.daddy.HDmode)
			{
				color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
			}
			for (int i = 0; i < (sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length; i++)
			{
				float floatPos = Mathf.InverseLerp(0.3f, 1f, (float)i / (float)((sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length - 1));
				(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, owner.EffectColor, OnTubeEffectColorFac(floatPos));
			}
			int num = 0;
			for (int j = 0; j < bumps.Length; j++)
			{
				sLeaser.sprites[firstSprite + 1 + j].color = Color.Lerp(color, owner.EffectColor, OnTubeEffectColorFac(bumps[j].pos.y));
				if (bumps[j].eyeSize > 0f)
				{
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num].color = (owner.colorClass ? owner.EffectColor : color);
					num++;
				}
			}
		}

		public virtual float OnTubeEffectColorFac(float floatPos)
		{
			return Mathf.Pow(floatPos, 1.5f) * 0.4f;
		}

		public Vector2 OnTubePos(Vector2 pos, float timeStacker)
		{
			Vector2 p = OneDimensionalTubePos(pos.y - 1f / (float)segments.Length, timeStacker);
			Vector2 p2 = OneDimensionalTubePos(pos.y + 1f / (float)segments.Length, timeStacker);
			return OneDimensionalTubePos(pos.y, timeStacker) + Custom.PerpendicularVector(Custom.DirVec(p, p2)) * pos.x;
		}

		public Vector2 OneDimensionalTubePos(float floatPos, float timeStacker)
		{
			int num = Custom.IntClamp(Mathf.FloorToInt(floatPos * (float)(segments.Length - 1)), 0, segments.Length - 1);
			int num2 = Custom.IntClamp(num + 1, 0, segments.Length - 1);
			float t = Mathf.InverseLerp(num, num2, floatPos * (float)(segments.Length - 1));
			return Vector2.Lerp(Vector2.Lerp(segments[num].lastPos, segments[num2].lastPos, t), Vector2.Lerp(segments[num].pos, segments[num2].pos, t), timeStacker);
		}
	}

	public class DaddyDangleTube : DaddyTubeGraphic
	{
		public class Connection
		{
			public DaddyLegGraphic daddyLeg;

			public float legPos;

			public BodyChunk chunk;

			public Vector2 Pos(float timeStacker)
			{
				if (chunk != null)
				{
					return Vector2.Lerp(chunk.lastPos, chunk.pos, timeStacker);
				}
				return daddyLeg.OneDimensionalTubePos(legPos, timeStacker);
			}
		}

		public GenericBodyPart[] bodyParts;

		public Connection Acon;

		public Connection Bcon;

		public DaddyDangleTube(DaddyGraphics owner, int parts, int firstSprite, Connection Acon, Connection Bcon)
			: base(owner, parts, firstSprite)
		{
			this.Acon = Acon;
			this.Bcon = Bcon;
			bodyParts = new GenericBodyPart[parts];
			for (int i = 0; i < parts; i++)
			{
				bodyParts[i] = new GenericBodyPart(owner, 3f, 0.5f, 0.99f, owner.daddy.mainBodyChunk);
			}
			sprites = 1;
			bumps = new Bump[UnityEngine.Random.Range(5, 18)];
			for (int j = 0; j < bumps.Length; j++)
			{
				bumps[j] = new Bump(new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 2f, UnityEngine.Random.value), Mathf.Pow(UnityEngine.Random.value, 1.4f) * (owner.SizeClass ? 1f : 0.8f), 0f);
				sprites++;
			}
		}

		public override void Update()
		{
			for (int i = 0; i < bodyParts.Length; i++)
			{
				bodyParts[i].vel.y -= 0.5f * owner.owner.room.gravity;
				bodyParts[i].Update();
				if (i > 1)
				{
					bodyParts[i - 2].vel += Custom.DirVec(bodyParts[i].pos, bodyParts[i - 2].pos);
					bodyParts[i].vel += Custom.DirVec(bodyParts[i - 2].pos, bodyParts[i].pos);
				}
				if (i < bodyParts.Length - 2)
				{
					bodyParts[i + 2].vel += Custom.DirVec(bodyParts[i].pos, bodyParts[i + 2].pos);
					bodyParts[i].vel += Custom.DirVec(bodyParts[i + 2].pos, bodyParts[i].pos);
				}
			}
			float num = Mathf.Max(10f, Vector2.Distance(Acon.Pos(1f), Bcon.Pos(1f)) / (float)bodyParts.Length);
			bodyParts[0].pos = Acon.Pos(1f);
			for (int j = 1; j < bodyParts.Length; j++)
			{
				Vector2 vector = Custom.DirVec(bodyParts[j].pos, bodyParts[j - 1].pos);
				float num2 = Vector2.Distance(bodyParts[j].pos, bodyParts[j - 1].pos);
				bodyParts[j].pos += vector * (num2 - num) * 0.5f;
				bodyParts[j].vel += vector * (num2 - num) * 0.5f;
				bodyParts[j - 1].pos -= vector * (num2 - num) * 0.5f;
				bodyParts[j - 1].vel -= vector * (num2 - num) * 0.5f;
			}
			int num3 = bodyParts.Length - 1;
			bodyParts[num3].pos = Bcon.Pos(1f);
			for (int num4 = num3 - 1; num4 >= 0; num4--)
			{
				Vector2 vector2 = Custom.DirVec(bodyParts[num4].pos, bodyParts[num4 + 1].pos);
				float num5 = Vector2.Distance(bodyParts[num4].pos, bodyParts[num4 + 1].pos);
				bodyParts[num4].pos += vector2 * (num5 - num) * 0.5f;
				bodyParts[num4].vel += vector2 * (num5 - num) * 0.5f;
				bodyParts[num4 + 1].pos -= vector2 * (num5 - num) * 0.5f;
				bodyParts[num4 + 1].vel -= vector2 * (num5 - num) * 0.5f;
			}
			bodyParts[0].pos = Acon.Pos(1f);
			bodyParts[num3].pos = Bcon.Pos(1f);
			for (int k = 0; k < segments.Length; k++)
			{
				segments[k].lastPos = segments[k].pos;
				segments[k].pos = bodyParts[k].pos;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color color = palette.blackColor;
			if (owner.daddy.HDmode)
			{
				color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
			}
			for (int i = 0; i < (sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length; i++)
			{
				float floatPos = Mathf.InverseLerp(0.3f, 1f, (float)i / (float)((sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length - 1));
				(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, owner.EffectColor, OnTubeEffectColorFac(floatPos));
			}
			sLeaser.sprites[firstSprite].color = color;
			for (int j = 0; j < bumps.Length; j++)
			{
				sLeaser.sprites[firstSprite + 1 + j].color = Color.Lerp(color, owner.EffectColor, OnTubeEffectColorFac(bumps[j].pos.y));
			}
		}

		public override float OnTubeEffectColorFac(float floatPos)
		{
			float a = 0f;
			float b = 0f;
			if (Acon.chunk == null)
			{
				a = Acon.daddyLeg.OnTubeEffectColorFac(Acon.legPos);
			}
			if (Bcon.chunk == null)
			{
				b = Bcon.daddyLeg.OnTubeEffectColorFac(Bcon.legPos);
			}
			return Mathf.Lerp(a, b, floatPos);
		}
	}

	public class DaddyDeadLeg : DaddyTubeGraphic
	{
		public GenericBodyPart[] bodyParts;

		private float deadness;

		private BodyChunk secondaryChunk;

		public DaddyDeadLeg(DaddyGraphics owner, int parts, int firstSprite, BodyChunk connectedChunk, BodyChunk secondaryChunk)
			: base(owner, parts + 1, firstSprite)
		{
			this.secondaryChunk = secondaryChunk;
			bodyParts = new GenericBodyPart[parts];
			for (int i = 0; i < parts; i++)
			{
				bodyParts[i] = new GenericBodyPart(owner, 3f, 0.5f, 0.99f, connectedChunk);
			}
			deadness = UnityEngine.Random.value;
			sprites = 1;
			bumps = new Bump[bodyParts.Length / 2 + UnityEngine.Random.Range(5, 8)];
			for (int j = 0; j < bumps.Length; j++)
			{
				float num = Mathf.Pow(UnityEngine.Random.value, 0.5f);
				if (j == 0)
				{
					num = 1f;
				}
				bumps[j] = new Bump(new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3f * num, Mathf.Lerp(Mathf.InverseLerp(0f, parts, parts - 20), 1f, num)), Mathf.Lerp(UnityEngine.Random.value, num, UnityEngine.Random.value) * (owner.SizeClass ? 1f : 0.8f), (UnityEngine.Random.value * (1f + deadness) < Mathf.Lerp(0f, 0.6f, num)) ? Mathf.Lerp(0.2f, 0.8f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, num))) : 0f);
				sprites++;
				if (bumps[j].eyeSize > 0f)
				{
					sprites++;
				}
			}
		}

		public override void Update()
		{
			Vector2 middleOfBody = owner.daddy.MiddleOfBody;
			for (int i = 0; i < bodyParts.Length; i++)
			{
				bodyParts[i].vel += Vector2.Lerp(Custom.DirVec(middleOfBody, Vector2.Lerp(bodyParts[i].connection.pos, secondaryChunk.pos, 0.3f)) * 3f, new Vector2(0f, -0.5f * owner.owner.room.gravity), Mathf.InverseLerp(0f, 4f, i));
				Vector2 p = middleOfBody;
				if (i == 1)
				{
					p = bodyParts[i].connection.pos;
				}
				else if (i > 1)
				{
					bodyParts[i - 2].vel += Custom.DirVec(bodyParts[i].pos, bodyParts[i - 2].pos);
					p = bodyParts[i - 2].pos;
				}
				bodyParts[i].vel += Custom.DirVec(p, bodyParts[i].pos);
				bodyParts[i].Update();
			}
			Vector2 vector = Custom.DirVec(bodyParts[0].pos, bodyParts[0].connection.pos);
			float num = Vector2.Distance(bodyParts[0].pos, bodyParts[0].connection.pos);
			bodyParts[0].pos += vector * (num - bodyParts[0].connection.rad);
			bodyParts[0].vel += vector * (num - bodyParts[0].connection.rad);
			for (int j = 1; j < bodyParts.Length; j++)
			{
				vector = Custom.DirVec(bodyParts[j].pos, bodyParts[j - 1].pos);
				num = Vector2.Distance(bodyParts[j].pos, bodyParts[j - 1].pos);
				bodyParts[j].pos += vector * (num - 10f) * 0.5f;
				bodyParts[j].vel += vector * (num - 10f) * 0.5f;
				bodyParts[j - 1].pos -= vector * (num - 10f) * 0.5f;
				bodyParts[j - 1].vel -= vector * (num - 10f) * 0.5f;
			}
			segments[0].lastPos = segments[0].pos;
			segments[0].pos = bodyParts[0].connection.pos;
			for (int k = 1; k < segments.Length; k++)
			{
				segments[k].lastPos = segments[k].pos;
				segments[k].pos = bodyParts[k - 1].pos;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color color = palette.blackColor;
			if (owner.daddy.HDmode)
			{
				color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
			}
			for (int i = 0; i < (sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length; i++)
			{
				float floatPos = Mathf.InverseLerp(0.3f, 1f, (float)i / (float)((sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length - 1));
				(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, owner.EffectColor, OnTubeEffectColorFac(floatPos));
			}
			int num = 0;
			for (int j = 0; j < bumps.Length; j++)
			{
				sLeaser.sprites[firstSprite + 1 + j].color = Color.Lerp(color, owner.EffectColor, OnTubeEffectColorFac(bumps[j].pos.y));
				if (bumps[j].eyeSize > 0f)
				{
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num].color = (owner.colorClass ? (owner.EffectColor * Mathf.Lerp(0.5f, 0.2f, deadness)) : color);
					num++;
				}
			}
		}

		public override float OnTubeEffectColorFac(float floatPos)
		{
			return base.OnTubeEffectColorFac(floatPos) * (1f - deadness);
		}
	}

	public class DaddyLegGraphic : DaddyTubeGraphic
	{
		public int tentacleIndex;

		public DaddyTentacle tentacle => owner.daddy.tentacles[tentacleIndex];

		public DaddyLegGraphic(DaddyGraphics owner, int tentacleIndex, int firstSprite)
			: base(owner, (int)(owner.daddy.tentacles[tentacleIndex].idealLength / 10f), firstSprite)
		{
			this.tentacleIndex = tentacleIndex;
			sprites = 1;
			int num = (int)(owner.daddy.tentacles[tentacleIndex].idealLength / 10f);
			bumps = new Bump[num / 2 + UnityEngine.Random.Range(5, 8)];
			for (int i = 0; i < bumps.Length; i++)
			{
				float num2 = Mathf.Pow(UnityEngine.Random.value, 0.3f);
				if (i == 0)
				{
					num2 = 1f;
				}
				bumps[i] = new Bump(new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3f * num2, Mathf.Lerp(Mathf.InverseLerp(0f, num, num - 20), 1f, num2)), Mathf.Lerp(UnityEngine.Random.value, num2, UnityEngine.Random.value) * (owner.SizeClass ? 1f : 0.8f), (UnityEngine.Random.value < Mathf.Lerp(0f, 0.6f, num2)) ? Mathf.Lerp(0.2f, 0.8f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, num2))) : 0f);
				sprites++;
				if (bumps[i].eyeSize > 0f)
				{
					sprites++;
				}
			}
		}

		public override void Update()
		{
			int listCount = 0;
			AddToPositionsList(listCount++, tentacle.FloatBase);
			for (int i = 0; i < tentacle.tChunks.Length; i++)
			{
				for (int j = 1; j < tentacle.tChunks[i].rope.TotalPositions; j++)
				{
					AddToPositionsList(listCount++, tentacle.tChunks[i].rope.GetPosition(j) + Custom.RNV() * Mathf.InverseLerp(4f, 14f, tentacle.stun) * 4f * UnityEngine.Random.value);
				}
			}
			AlignAndConnect(listCount);
		}

		public override void ConnectPhase(float totalRopeLength)
		{
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class HunterDummy
	{
		public DaddyGraphics owner;

		public int startSprite;

		public int numberOfSprites;

		public TailSegment[] tail;

		public GenericBodyPart head;

		private GenericBodyPart legs;

		public BodyPart[] bodyParts;

		public Vector2[,] drawPositions;

		private Vector2 legsDirection;

		public float breath;

		public float lastBreath;

		public float darkenFactor;

		public HunterDummy(DaddyGraphics dg, int startSprite)
		{
			owner = dg;
			this.startSprite = startSprite;
			numberOfSprites = 6;
			List<BodyPart> list = new List<BodyPart>();
			tail = new TailSegment[4];
			tail[0] = new TailSegment(owner, 6f, 4f, null, 0.85f, 1f, 1f, pullInPreviousPosition: true);
			tail[1] = new TailSegment(owner, 4f, 7f, tail[0], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			tail[2] = new TailSegment(owner, 2.5f, 7f, tail[1], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			tail[3] = new TailSegment(owner, 1f, 7f, tail[2], 0.85f, 1f, 0.5f, pullInPreviousPosition: true);
			for (int i = 0; i < tail.Length; i++)
			{
				list.Add(tail[i]);
			}
			head = new GenericBodyPart(owner, 4f, 0.8f, 0.99f, owner.daddy.bodyChunks[0]);
			list.Add(head);
			legs = new GenericBodyPart(owner, 1f, 0.8f, 0.99f, owner.daddy.bodyChunks[1]);
			list.Add(legs);
			legsDirection = new Vector2(0f, -1f);
			drawPositions = new Vector2[2, 2];
			for (int j = 0; j < 2; j++)
			{
				drawPositions[j, 0] = owner.daddy.bodyChunks[j].pos;
				drawPositions[j, 1] = owner.daddy.bodyChunks[j].lastPos;
			}
			bodyParts = list.ToArray();
		}

		public void Update()
		{
			lastBreath = breath;
			breath += 0.0125f;
			for (int i = 0; i < 2; i++)
			{
				drawPositions[i, 1] = drawPositions[i, 0];
			}
			drawPositions[0, 0] = owner.daddy.bodyChunks[0].pos;
			drawPositions[1, 0] = owner.daddy.bodyChunks[1].pos;
			float num = 0f;
			Vector2 vector = owner.daddy.bodyChunks[0].pos;
			Vector2 pos = owner.daddy.bodyChunks[1].pos;
			float num2 = 28f;
			tail[0].connectedPoint = drawPositions[1, 0];
			for (int j = 0; j < tail.Length; j++)
			{
				tail[j].Update();
				tail[j].vel *= Mathf.Lerp(0.75f, 0.95f, num * (1f - owner.daddy.mainBodyChunk.submersion));
				TailSegment tailSegment = tail[j];
				tailSegment.vel.y = tailSegment.vel.y - Mathf.Lerp(0.1f, 0.5f, num) * (1f - owner.daddy.mainBodyChunk.submersion) * owner.daddy.room.gravity;
				num = (num * 10f + 1f) / 11f;
				if (!Custom.DistLess(tail[j].pos, owner.daddy.bodyChunks[1].pos, 9f * (float)(j + 1)))
				{
					tail[j].pos = owner.daddy.bodyChunks[1].pos + Custom.DirVec(owner.daddy.bodyChunks[1].pos, tail[j].pos) * 9f * (j + 1);
				}
				tail[j].vel += Custom.DirVec(vector, tail[j].pos) * num2 / Vector2.Distance(vector, tail[j].pos);
				num2 *= 0.5f;
				vector = pos;
				pos = tail[j].pos;
			}
			Vector2 vector2 = Custom.DirVec(drawPositions[1, 0], drawPositions[0, 0]) * 3f;
			head.Update();
			head.ConnectToPoint(Vector2.Lerp(drawPositions[0, 0], drawPositions[1, 0], 0.2f) + vector2, 3f, push: false, 0.2f, owner.daddy.bodyChunks[0].vel, 0.7f, 0.1f);
			legs.Update();
			legs.ConnectToPoint(owner.daddy.bodyChunks[1].pos + new Vector2(legsDirection.x * 8f, -2f), 4f, push: false, 0.25f, new Vector2(owner.daddy.bodyChunks[1].vel.x, -10f), 0.5f, 0.1f);
			legsDirection += owner.daddy.bodyChunks[1].vel * 0.01f;
			legsDirection.y -= 0.05f;
			legsDirection.Normalize();
		}

		public void Reset()
		{
			for (int i = 0; i < 2; i++)
			{
				drawPositions[i, 0] = owner.daddy.bodyChunks[i].pos;
				drawPositions[i, 1] = owner.daddy.bodyChunks[i].pos;
			}
			for (int j = 0; j < tail.Length; j++)
			{
				tail[j].Reset(owner.daddy.bodyChunks[1].pos);
			}
			head.Reset(owner.daddy.bodyChunks[0].pos);
			legs.Reset(owner.daddy.bodyChunks[1].pos);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[startSprite] = new FSprite("BodyA");
			sLeaser.sprites[startSprite].anchorY = 0.7894737f;
			sLeaser.sprites[startSprite + 1] = new FSprite("HipsA");
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[13]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(1, 2, 3),
				new TriangleMesh.Triangle(4, 5, 6),
				new TriangleMesh.Triangle(5, 6, 7),
				new TriangleMesh.Triangle(8, 9, 10),
				new TriangleMesh.Triangle(9, 10, 11),
				new TriangleMesh.Triangle(12, 13, 14),
				new TriangleMesh.Triangle(2, 3, 4),
				new TriangleMesh.Triangle(3, 4, 5),
				new TriangleMesh.Triangle(6, 7, 8),
				new TriangleMesh.Triangle(7, 8, 9),
				new TriangleMesh.Triangle(10, 11, 12),
				new TriangleMesh.Triangle(11, 12, 13)
			};
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: false);
			sLeaser.sprites[startSprite + 2] = triangleMesh;
			sLeaser.sprites[startSprite + 3] = new FSprite("HeadA0");
			sLeaser.sprites[startSprite + 4] = new FSprite("LegsA0");
			sLeaser.sprites[startSprite + 4].anchorY = 0.25f;
			sLeaser.sprites[startSprite + 5] = new FSprite("FaceDead");
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastBreath, breath, timeStacker) * (float)Math.PI * 2f);
			Vector2 vector = Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker);
			Vector2 vector2 = Vector2.Lerp(drawPositions[1, 1], drawPositions[1, 0], timeStacker);
			Vector2 p = Vector2.Lerp(head.lastPos, head.pos, timeStacker);
			float num2 = Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(Custom.DirVec(vector2, vector).y));
			sLeaser.sprites[startSprite].x = vector.x - camPos.x;
			sLeaser.sprites[startSprite].y = vector.y - camPos.y + 0.5f * num * (1f - num2);
			sLeaser.sprites[startSprite].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
			sLeaser.sprites[startSprite].scaleX = 1f + Mathf.Lerp(-0.15f, 0.05f, num) * num2;
			sLeaser.sprites[startSprite + 1].x = (vector2.x * 2f + vector.x) / 3f - camPos.x;
			sLeaser.sprites[startSprite + 1].y = (vector2.y * 2f + vector.y) / 3f - camPos.y;
			sLeaser.sprites[startSprite + 1].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(tail[0].lastPos, tail[0].pos, timeStacker));
			sLeaser.sprites[startSprite + 1].scaleY = 1f;
			sLeaser.sprites[startSprite + 1].scaleX = 1f + 0.05f * num - 0.05f;
			Vector2 vector3 = (vector2 * 3f + vector) / 4f;
			float num3 = 0.8f;
			float num4 = 6f;
			for (int i = 0; i < 4; i++)
			{
				Vector2 vector4 = Vector2.Lerp(tail[i].lastPos, tail[i].pos, timeStacker);
				Vector2 normalized = (vector4 - vector3).normalized;
				Vector2 vector5 = Custom.PerpendicularVector(normalized);
				float num5 = Vector2.Distance(vector4, vector3) / 5f;
				if (i == 0)
				{
					num5 = 0f;
				}
				(sLeaser.sprites[startSprite + 2] as TriangleMesh).MoveVertice(i * 4, vector3 - vector5 * num4 * num3 + normalized * num5 - camPos);
				(sLeaser.sprites[startSprite + 2] as TriangleMesh).MoveVertice(i * 4 + 1, vector3 + vector5 * num4 * num3 + normalized * num5 - camPos);
				if (i < 3)
				{
					(sLeaser.sprites[startSprite + 2] as TriangleMesh).MoveVertice(i * 4 + 2, vector4 - vector5 * tail[i].StretchedRad * num3 - normalized * num5 - camPos);
					(sLeaser.sprites[startSprite + 2] as TriangleMesh).MoveVertice(i * 4 + 3, vector4 + vector5 * tail[i].StretchedRad * num3 - normalized * num5 - camPos);
				}
				else
				{
					(sLeaser.sprites[startSprite + 2] as TriangleMesh).MoveVertice(i * 4 + 2, vector4 - camPos);
				}
				num4 = tail[i].StretchedRad;
				vector3 = vector4;
			}
			float num6 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector, 0.5f), p);
			int num7 = Mathf.RoundToInt(Mathf.Abs(num6 / 360f * 34f));
			Vector2 zero = Vector2.zero;
			zero *= 0f;
			num7 = 0;
			sLeaser.sprites[startSprite + 5].rotation = num6;
			sLeaser.sprites[startSprite + 3].x = p.x - camPos.x;
			sLeaser.sprites[startSprite + 3].y = p.y - camPos.y;
			sLeaser.sprites[startSprite + 3].rotation = num6;
			sLeaser.sprites[startSprite + 3].scaleX = ((num6 >= 0f) ? 1f : (-1f));
			sLeaser.sprites[startSprite + 3].element = Futile.atlasManager.GetElementWithName("HeadA" + num7);
			sLeaser.sprites[startSprite + 5].x = p.x + zero.x - camPos.x;
			sLeaser.sprites[startSprite + 5].y = p.y + zero.y - 2f - camPos.y;
			Vector2 vector6 = Vector2.Lerp(legs.lastPos, legs.pos, timeStacker);
			sLeaser.sprites[startSprite + 4].x = vector6.x - camPos.x;
			sLeaser.sprites[startSprite + 4].y = vector6.y - camPos.y;
			sLeaser.sprites[startSprite + 4].rotation = Custom.AimFromOneVectorToAnother(legsDirection, new Vector2(0f, 0f));
			sLeaser.sprites[startSprite + 4].isVisible = true;
			string elementName = "LegsAAir0";
			sLeaser.sprites[startSprite + 4].element = Futile.atlasManager.GetElementWithName(elementName);
			if (darkenFactor > 0f)
			{
				for (int j = 0; j < numberOfSprites; j++)
				{
					Color color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
					sLeaser.sprites[startSprite + j].color = new Color(Mathf.Min(1f, color.r * (1f - darkenFactor) + 0.01f), Mathf.Min(1f, color.g * (1f - darkenFactor) + 0.01f), Mathf.Min(1f, color.b * (1f - darkenFactor) + 0.01f));
				}
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
			Color blackColor = palette.blackColor;
			for (int i = 0; i < numberOfSprites - 1; i++)
			{
				sLeaser.sprites[startSprite + i].color = color;
			}
			sLeaser.sprites[startSprite + 5].color = blackColor;
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Midground");
			}
			for (int i = 0; i < numberOfSprites; i++)
			{
				newContatiner.AddChild(sLeaser.sprites[startSprite + i]);
			}
		}
	}

	public DaddyLegGraphic[] legGraphics;

	public DaddyDeadLeg[] deadLegs;

	public DaddyDangleTube[] danglers;

	private float[,] chunksRotats;

	private int totalLegSprites;

	private int totalDeadLegSprites;

	private int totalDanglers;

	public Color blackColor;

	public float digesting;

	public int reactionSoundDelay;

	public int feelSomethingReactionDelay;

	public Eye[] eyes;

	public StaticSoundLoop digestLoop;

	public HunterDummy dummy;

	private int TotalSprites => totalLegSprites + totalDeadLegSprites + totalDanglers + daddy.bodyChunks.Length * (daddy.HDmode ? 4 : 3) + (daddy.HDmode ? dummy.numberOfSprites : 0);

	public DaddyLongLegs daddy => base.owner as DaddyLongLegs;

	public bool SizeClass => daddy.SizeClass;

	public bool colorClass => daddy.colorClass;

	public Color EffectColor => daddy.effectColor;

	private int DanglerSprite(int dangler)
	{
		return dangler;
	}

	private int LegSprite(int leg)
	{
		return totalDanglers + leg;
	}

	private int DeadLegSprite(int leg)
	{
		return totalDanglers + totalLegSprites + leg;
	}

	private int BodySprite(int chunk)
	{
		return totalLegSprites + totalDeadLegSprites + totalDanglers + chunk;
	}

	private int EyeSprite(int eye, int part)
	{
		return totalLegSprites + totalDeadLegSprites + totalDanglers + daddy.bodyChunks.Length + eye * (daddy.HDmode ? 3 : 2) + part;
	}

	public DaddyGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		cullRange = 1400f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(daddy.graphicsSeed);
		int num = 0;
		int num2 = 0;
		legGraphics = new DaddyLegGraphic[daddy.tentacles.Length];
		for (int i = 0; i < legGraphics.Length; i++)
		{
			legGraphics[i] = new DaddyLegGraphic(this, i, num2);
			num2 += legGraphics[i].sprites;
		}
		totalLegSprites = num2;
		int num3 = (SizeClass ? UnityEngine.Random.Range(5, 15) : UnityEngine.Random.Range(3, 8));
		List<int> list = new List<int>();
		if (daddy.HDmode)
		{
			deadLegs = new DaddyDeadLeg[0];
		}
		else
		{
			deadLegs = new DaddyDeadLeg[UnityEngine.Random.Range((!SizeClass) ? 1 : 2, UnityEngine.Random.Range((!SizeClass) ? 1 : 2, num3))];
		}
		for (int j = 0; j < deadLegs.Length; j++)
		{
			if (list.Count == 0)
			{
				for (int k = 0; k < daddy.bodyChunks.Length; k++)
				{
					list.Add(k);
				}
			}
			deadLegs[j] = new DaddyDeadLeg(this, UnityEngine.Random.Range(4, UnityEngine.Random.Range(8, SizeClass ? 17 : 8)), num2, daddy.bodyChunks[list[UnityEngine.Random.Range(0, list.Count)]], daddy.bodyChunks[UnityEngine.Random.Range(0, daddy.bodyChunks.Length)]);
			list.Remove(deadLegs[j].bodyParts[0].connection.index);
			num2 += deadLegs[j].sprites;
			num += deadLegs[j].bodyParts.Length;
		}
		totalDeadLegSprites = num2 - totalLegSprites;
		DaddyDangleTube.Connection connection = GenerateDangleCon(UnityEngine.Random.value < 0.5f);
		DaddyDangleTube.Connection connection2 = GenerateDangleCon(connection.chunk != null);
		danglers = new DaddyDangleTube[num3 - deadLegs.Length];
		bool flag = UnityEngine.Random.value < 0.5f;
		for (int l = 0; l < danglers.Length; l++)
		{
			if (UnityEngine.Random.value < 0.2f)
			{
				connection = GenerateDangleCon(UnityEngine.Random.value < 0.5f);
				connection2 = GenerateDangleCon(connection.chunk != null);
			}
			else
			{
				if (flag)
				{
					connection = GenerateDangleCon(connection2.chunk != null || UnityEngine.Random.value < 0.2f);
				}
				else
				{
					connection2 = GenerateDangleCon(connection.chunk != null || UnityEngine.Random.value < 0.2f);
				}
				flag = !flag;
			}
			if (connection.daddyLeg != null)
			{
				connection.legPos = Mathf.Lerp(0.1f, Mathf.Min(0.7f, 20f / (float)connection.daddyLeg.segments.Length), Mathf.Pow(UnityEngine.Random.value, 1.2f));
			}
			if (connection2.daddyLeg != null)
			{
				connection2.legPos = Mathf.Lerp(0.1f, Mathf.Min(0.7f, 20f / (float)connection2.daddyLeg.segments.Length), Mathf.Pow(UnityEngine.Random.value, 1.2f));
			}
			danglers[l] = new DaddyDangleTube(this, UnityEngine.Random.Range(2, SizeClass ? 25 : 12), num2, connection, connection2);
			num2 += danglers[l].sprites;
			num += danglers[l].bodyParts.Length;
		}
		totalDanglers = num2 - totalLegSprites - totalDeadLegSprites;
		chunksRotats = new float[daddy.bodyChunks.Length, 2];
		eyes = new Eye[daddy.bodyChunks.Length];
		for (int m = 0; m < daddy.bodyChunks.Length; m++)
		{
			chunksRotats[m, 0] = UnityEngine.Random.value * 360f;
			chunksRotats[m, 1] = UnityEngine.Random.value;
			eyes[m] = new Eye(this, m);
		}
		UnityEngine.Random.state = state;
		if (daddy.HDmode)
		{
			dummy = new HunterDummy(this, DummySprite());
			bodyParts = new BodyPart[num + dummy.bodyParts.Length];
		}
		else
		{
			bodyParts = new BodyPart[num];
		}
		num = 0;
		for (int n = 0; n < deadLegs.Length; n++)
		{
			for (int num4 = 0; num4 < deadLegs[n].bodyParts.Length; num4++)
			{
				bodyParts[num] = deadLegs[n].bodyParts[num4];
				num++;
			}
		}
		for (int num5 = 0; num5 < danglers.Length; num5++)
		{
			for (int num6 = 0; num6 < danglers[num5].bodyParts.Length; num6++)
			{
				bodyParts[num] = danglers[num5].bodyParts[num6];
				num++;
			}
		}
		if (daddy.HDmode)
		{
			for (int num7 = 0; num7 < dummy.bodyParts.Length; num7++)
			{
				bodyParts[num + num7] = dummy.bodyParts[num7];
			}
		}
		internalContainerObjects = new List<ObjectHeldInInternalContainer>();
		digestLoop = new StaticSoundLoop(daddy.SizeClass ? SoundID.Daddy_Digestion_LOOP : SoundID.Bro_Digestion_LOOP, base.owner.firstChunk.pos, base.owner.room, 0f, 1f);
	}

	private DaddyDangleTube.Connection GenerateDangleCon(bool otherIsBodyChunk)
	{
		if (otherIsBodyChunk)
		{
			return new DaddyDangleTube.Connection
			{
				daddyLeg = legGraphics[UnityEngine.Random.Range(0, legGraphics.Length)]
			};
		}
		return new DaddyDangleTube.Connection
		{
			chunk = daddy.bodyChunks[UnityEngine.Random.Range(0, daddy.bodyChunks.Length)]
		};
	}

	public override void Update()
	{
		base.Update();
		if (!culled)
		{
			for (int i = 0; i < legGraphics.Length; i++)
			{
				legGraphics[i].Update();
			}
			for (int j = 0; j < deadLegs.Length; j++)
			{
				deadLegs[j].Update();
			}
			for (int k = 0; k < danglers.Length; k++)
			{
				danglers[k].Update();
			}
			for (int l = 0; l < daddy.bodyChunks.Length; l++)
			{
				eyes[l].Update();
			}
		}
		if (daddy.HDmode)
		{
			dummy.Update();
		}
		digestLoop.Update();
		if (digestLoop.volume > 0f)
		{
			digestLoop.pos = daddy.MiddleOfBody;
		}
		digestLoop.volume = Mathf.Lerp(digestLoop.volume, Mathf.Pow(digesting, 0.8f), 0.2f);
		if (reactionSoundDelay > 0)
		{
			reactionSoundDelay--;
		}
		if (feelSomethingReactionDelay > 0)
		{
			feelSomethingReactionDelay--;
		}
		digesting = Mathf.Lerp(digesting, Mathf.Clamp(Mathf.Pow(daddy.MostDigestedEatObject, 0.5f), 0f, 1f), 0.1f);
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < legGraphics.Length; i++)
		{
			legGraphics[i].Reset(daddy.mainBodyChunk.pos);
		}
		for (int j = 0; j < deadLegs.Length; j++)
		{
			deadLegs[j].Reset(daddy.mainBodyChunk.pos);
		}
		for (int k = 0; k < danglers.Length; k++)
		{
			danglers[k].Reset(daddy.mainBodyChunk.pos);
		}
		if (daddy.HDmode)
		{
			dummy.Reset();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		for (int i = 0; i < daddy.bodyChunks.Length; i++)
		{
			sLeaser.sprites[BodySprite(i)] = new FSprite("Futile_White");
			sLeaser.sprites[BodySprite(i)].scale = (base.owner.bodyChunks[i].rad * 1.1f + 2f) / 8f;
			sLeaser.sprites[BodySprite(i)].shader = rCam.room.game.rainWorld.Shaders["JaggedCircle"];
			sLeaser.sprites[BodySprite(i)].alpha = 0.25f;
			sLeaser.sprites[EyeSprite(i, 0)] = MakeSlitMesh();
			sLeaser.sprites[EyeSprite(i, 1)] = MakeSlitMesh();
			if (daddy.HDmode)
			{
				sLeaser.sprites[EyeSprite(i, 2)] = new FSprite("CorruptGrad");
				sLeaser.sprites[EyeSprite(i, 2)].scale = 0.0625f * base.owner.bodyChunks[i].rad * 2f;
			}
		}
		for (int j = 0; j < daddy.tentacles.Length; j++)
		{
			legGraphics[j].InitiateSprites(sLeaser, rCam);
		}
		for (int k = 0; k < deadLegs.Length; k++)
		{
			deadLegs[k].InitiateSprites(sLeaser, rCam);
		}
		for (int l = 0; l < danglers.Length; l++)
		{
			danglers[l].InitiateSprites(sLeaser, rCam);
		}
		if (daddy.HDmode)
		{
			dummy.InitiateSprites(sLeaser, rCam);
		}
		sLeaser.containers = new FContainer[1]
		{
			new FContainer()
		};
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
		if (sLeaser.containers != null)
		{
			FContainer[] containers = sLeaser.containers;
			foreach (FContainer node in containers)
			{
				newContatiner.AddChild(node);
			}
		}
		for (int j = 0; j < sLeaser.sprites.Length; j++)
		{
			newContatiner.AddChild(sLeaser.sprites[j]);
		}
		if (daddy.HDmode)
		{
			dummy.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		Vector2 vector = Vector2.Lerp(base.owner.bodyChunks[0].lastPos, base.owner.bodyChunks[0].pos, timeStacker) * base.owner.bodyChunks[0].mass;
		for (int i = 1; i < daddy.bodyChunks.Length; i++)
		{
			vector += Vector2.Lerp(base.owner.bodyChunks[i].lastPos, base.owner.bodyChunks[i].pos, timeStacker) * base.owner.bodyChunks[i].mass;
		}
		vector /= daddy.TotalMass;
		for (int j = 0; j < daddy.bodyChunks.Length; j++)
		{
			Vector2 vector2 = Vector2.Lerp(base.owner.bodyChunks[j].lastPos, base.owner.bodyChunks[j].pos, timeStacker) + Custom.RNV() * digesting * 4f * UnityEngine.Random.value;
			if (daddy.HDmode && j < 2)
			{
				sLeaser.sprites[BodySprite(j)].isVisible = false;
			}
			sLeaser.sprites[BodySprite(j)].x = vector2.x - camPos.x;
			sLeaser.sprites[BodySprite(j)].y = vector2.y - camPos.y;
			sLeaser.sprites[BodySprite(j)].rotation = Custom.AimFromOneVectorToAnother(vector2, vector) + chunksRotats[j, 0];
			if (daddy.HDmode)
			{
				sLeaser.sprites[EyeSprite(j, 2)].color = Color.Lerp(eyes[j].renderColor, Color.black, 0.4f);
				sLeaser.sprites[EyeSprite(j, 2)].x = vector2.x - camPos.x;
				sLeaser.sprites[EyeSprite(j, 2)].y = vector2.y - camPos.y;
			}
			RenderSlits(j, vector2, vector, Custom.AimFromOneVectorToAnother(vector2, vector) + chunksRotats[j, 0], sLeaser, rCam, timeStacker, camPos);
		}
		for (int k = 0; k < legGraphics.Length; k++)
		{
			legGraphics[k].DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		for (int l = 0; l < deadLegs.Length; l++)
		{
			deadLegs[l].DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		for (int m = 0; m < danglers.Length; m++)
		{
			danglers[m].DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		if (daddy.HDmode)
		{
			dummy.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		blackColor = palette.blackColor;
		for (int i = 0; i < daddy.bodyChunks.Length; i++)
		{
			if (daddy.HDmode)
			{
				sLeaser.sprites[BodySprite(i)].color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
			}
			else
			{
				sLeaser.sprites[BodySprite(i)].color = blackColor;
			}
		}
		for (int j = 0; j < legGraphics.Length; j++)
		{
			legGraphics[j].ApplyPalette(sLeaser, rCam, palette);
		}
		for (int k = 0; k < deadLegs.Length; k++)
		{
			deadLegs[k].ApplyPalette(sLeaser, rCam, palette);
		}
		for (int l = 0; l < danglers.Length; l++)
		{
			danglers[l].ApplyPalette(sLeaser, rCam, palette);
		}
		if (daddy.HDmode)
		{
			dummy.ApplyPalette(sLeaser, rCam, palette);
		}
	}

	private TriangleMesh MakeSlitMesh()
	{
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[8];
		for (int i = 0; i < 8; i++)
		{
			array[i] = new TriangleMesh.Triangle(i, i + 1, i + 2);
		}
		return new TriangleMesh("Futile_White", array, customColor: false);
	}

	private void RenderSlits(int chunk, Vector2 pos, Vector2 middleOfBody, float rotation, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (daddy.HDmode && chunk < 2)
		{
			sLeaser.sprites[EyeSprite(chunk, 0)].isVisible = false;
			sLeaser.sprites[EyeSprite(chunk, 1)].isVisible = false;
			sLeaser.sprites[EyeSprite(chunk, 2)].isVisible = false;
			return;
		}
		float rad = daddy.bodyChunks[chunk].rad;
		float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(eyes[chunk].lastClosed, eyes[chunk].closed, timeStacker)), 0.6f);
		float num2 = (SizeClass ? 1f : 0.8f) * (1f - num);
		Vector2 b = Vector2.Lerp(eyes[chunk].lastDir, eyes[chunk].dir, timeStacker);
		float a = Mathf.Lerp(eyes[chunk].lastFocus, eyes[chunk].focus, timeStacker) * Mathf.Pow(Mathf.InverseLerp(-1f, 1f, Vector2.Dot(Custom.DirVec(middleOfBody, pos), b.normalized)), 0.7f);
		a = Mathf.Max(a, num);
		float a2 = Mathf.InverseLerp(0f, Mathf.Lerp(30f, 50f, chunksRotats[chunk, 1]), Vector2.Distance(middleOfBody, pos + Custom.DirVec(middleOfBody, pos) * rad)) * 0.9f;
		a2 = Mathf.Lerp(a2, 1f, 0.5f * a);
		Vector2 vector = Vector2.Lerp(Custom.DirVec(middleOfBody, pos) * a2, b, b.magnitude * 0.5f);
		eyes[chunk].centerRenderPos = pos + vector * rad;
		eyes[chunk].renderColor = Color.Lerp(daddy.eyeColor, new Color(1f, 1f, 1f), Mathf.Lerp(UnityEngine.Random.value * eyes[chunk].light, 1f, num));
		if (num > 0f)
		{
			eyes[chunk].renderColor = Color.Lerp(eyes[chunk].renderColor, blackColor, num);
		}
		eyes[chunk].renderColor = Color.Lerp(eyes[chunk].renderColor, Color.white, eyes[chunk].flash);
		sLeaser.sprites[EyeSprite(chunk, 0)].color = eyes[chunk].renderColor;
		sLeaser.sprites[EyeSprite(chunk, 1)].color = eyes[chunk].renderColor;
		for (int i = 0; i < 2; i++)
		{
			Vector2 vector2 = Custom.DegToVec(rotation + 90f * (float)i);
			Vector2 vector3 = Custom.PerpendicularVector(vector2);
			(sLeaser.sprites[EyeSprite(chunk, i)] as TriangleMesh).MoveVertice(0, pos + BulgeVertex(vector2 * rad * 0.9f * Mathf.Lerp(1f, 0.6f, a), vector, rad) - camPos);
			(sLeaser.sprites[EyeSprite(chunk, i)] as TriangleMesh).MoveVertice(9, pos + BulgeVertex(vector2 * (0f - rad) * 0.9f * Mathf.Lerp(1f, 0.6f, a), vector, rad) - camPos);
			for (int j = 1; j < 5; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					float num3 = rad * ((j < 3) ? 0.7f : 0.25f) * ((k == 0) ? 1f : (-1f)) * Mathf.Lerp(1f, 0.6f, a);
					int num4 = ((k == 0) ? j : (9 - j));
					float num5 = num2 * ((j < 3) ? 0.5f : 1f) * ((num4 % 2 == 0) ? 1f : (-1f)) * Mathf.Lerp(1f, 2.5f, a);
					(sLeaser.sprites[EyeSprite(chunk, i)] as TriangleMesh).MoveVertice(num4, pos + BulgeVertex(vector2 * num3 + vector3 * num5, vector, rad) - camPos);
				}
			}
		}
	}

	private Vector2 BulgeVertex(Vector2 v, Vector2 dir, float rad)
	{
		return Vector2.Lerp(v, Vector2.ClampMagnitude(v + dir * rad, rad), dir.magnitude);
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
		if (!daddy.SizeClass)
		{
			return;
		}
		Vector2 middleOfBody = daddy.MiddleOfBody;
		float num = float.MinValue;
		int num2 = -1;
		for (int i = 0; i < eyes.Length; i++)
		{
			float num3 = Vector2.Dot(Custom.DirVec(middleOfBody, daddy.bodyChunks[i].pos), Custom.DirVec(middleOfBody, noise.pos));
			if (eyes[i].soundSource != null)
			{
				num3 -= 1f;
			}
			if (eyes[i].creatureRep != null)
			{
				num3 -= 2f;
			}
			if (num3 > num)
			{
				num = num3;
				num2 = i;
			}
		}
		eyes[num2].ReactToSound(source);
		float num4 = 0.5f;
		if (daddy.AI.behavior == DaddyAI.Behavior.Hunt && daddy.AI.preyTracker.MostAttractivePrey != null)
		{
			num4 = ((daddy.AI.preyTracker.MostAttractivePrey == source.creatureRep) ? 1f : 0.2f);
		}
		else if (daddy.AI.behavior == DaddyAI.Behavior.ExamineSound && daddy.AI.noiseTracker.soundToExamine != null)
		{
			num4 = ((daddy.AI.noiseTracker.soundToExamine == source) ? 1f : 0.2f);
		}
		num4 = Mathf.Clamp01((num4 + Mathf.InverseLerp(150f, 400f, noise.strength) + num4 * noise.interesting) / 3f);
		if (source.creatureRep != null && source.creatureRep.VisualContact)
		{
			num4 *= 0.1f;
		}
		num4 = Mathf.Max(num4, Mathf.InverseLerp(1f, 4f, noise.interesting));
		if (num4 * 40f > (float)reactionSoundDelay)
		{
			float num5 = (num4 * 3f + Mathf.InverseLerp(4f, 16f, eyes[num2].chunk.rad) + Mathf.InverseLerp(1200f, 800f, Vector2.Distance(middleOfBody, noise.pos)) + UnityEngine.Random.value) / 6f;
			for (int j = 0; j < (int)Mathf.Lerp(2f, 9f, num5); j++)
			{
				daddy.room.AddObject(new DaddyBubble(eyes[num2], Custom.DirVec(eyes[num2].chunk.pos, noise.pos) * 12f / (1f + (float)j * 0.2f), 1f, UnityEngine.Random.value, 0f));
			}
			daddy.room.AddObject(new DaddyRipple(eyes[num2], noise.pos, default(Vector2), num5, daddy.eyeColor));
			base.owner.room.PlaySound(SoundID.Daddy_React_To_Noise, middleOfBody);
			reactionSoundDelay = Math.Max(reactionSoundDelay, UnityEngine.Random.Range(10, 40));
		}
	}

	public void FeelSomethingWithTentacle(Tracker.CreatureRepresentation creatureRep, Vector2 feelPos)
	{
		if (feelSomethingReactionDelay > 0)
		{
			return;
		}
		feelSomethingReactionDelay = UnityEngine.Random.Range(10, 40);
		if (!daddy.SizeClass)
		{
			base.owner.room.PlaySound(SoundID.Bro_React_To_Tentacle_Touch, daddy.MiddleOfBody);
			return;
		}
		Vector2 middleOfBody = daddy.MiddleOfBody;
		float num = float.MinValue;
		int num2 = -1;
		for (int i = 0; i < eyes.Length; i++)
		{
			float num3 = Vector2.Dot(Custom.DirVec(middleOfBody, daddy.bodyChunks[i].pos), Custom.DirVec(middleOfBody, feelPos));
			if (eyes[i].soundSource != null)
			{
				num3 -= 1f;
			}
			if (eyes[i].creatureRep != null)
			{
				num3 -= 2f;
			}
			if (num3 > num)
			{
				num = num3;
				num2 = i;
			}
		}
		eyes[num2].ReactToCreature(creatureRep);
		base.owner.room.PlaySound(SoundID.Daddy_React_To_Tentacle_Touch, middleOfBody);
	}

	private int DummySprite()
	{
		return totalLegSprites + totalDeadLegSprites + totalDanglers + daddy.bodyChunks.Length * 4;
	}
}
