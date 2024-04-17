using RWCustom;
using UnityEngine;

namespace Smoke;

public abstract class MeshSmoke : SmokeSystem
{
	public abstract class SmokeSegment : SmokeSystemParticle
	{
		protected Vector2 lingerPosVel;

		protected Vector2 prevLingerPos;

		protected Vector2 lastPrevLingerPos;

		protected Vector2 prevLingerPosVel;

		protected float prevLingerRad;

		protected Color lingerColor;

		public virtual float ConDist(float timeStacker)
		{
			return 1f;
		}

		public virtual void WindAndDrag(Room r, ref Vector2 v, Vector2 p)
		{
		}

		public virtual float MyRad(float timeStacker)
		{
			return 1f;
		}

		public virtual float MyOpactiy(float timeStacker)
		{
			return 1f;
		}

		public virtual Color MyColor(float timeStacker)
		{
			return Color.white;
		}

		public SmokeSegment()
		{
		}

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime)
		{
			base.Reset(newOwner, pos, vel, lifeTime);
			lingerPosVel = vel;
			prevLingerPosVel = vel;
			prevLingerRad = 0f;
			prevLingerPos = pos + Custom.RNV();
			lastPrevLingerPos = prevLingerPos;
		}

		public override void Update(bool eu)
		{
			lastPrevLingerPos = prevLingerPos;
			base.Update(eu);
			if (resting)
			{
				return;
			}
			WindAndDrag(room, ref vel, pos);
			if (room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
			{
				IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
				FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
				pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
				if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
				{
					vel.x = Mathf.Abs(vel.x) * 0.4f;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
				{
					vel.x = (0f - Mathf.Abs(vel.x)) * 0.4f;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
				{
					vel.y = Mathf.Abs(vel.y) * 0.4f;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
				{
					vel.y = (0f - Mathf.Abs(vel.y)) * 0.4f;
				}
				life -= 0.02f;
			}
			if (base.nextParticle != null)
			{
				if ((owner as MeshSmoke).PushApartSegments > 0)
				{
					SmokeSystemParticle smokeSystemParticle = base.nextParticle;
					for (int i = 0; i < (owner as MeshSmoke).PushApartSegments; i++)
					{
						if (Custom.DistLess(pos, smokeSystemParticle.pos, ConDist(1f)))
						{
							Vector2 vector = Custom.DirVec(pos, smokeSystemParticle.pos) * (Vector2.Distance(pos, smokeSystemParticle.pos) - ConDist(1f));
							vector *= 0.5f / (1f + (float)i);
							vel += vector;
							pos += vector;
							smokeSystemParticle.vel -= vector;
							smokeSystemParticle.pos -= vector;
						}
						smokeSystemParticle = smokeSystemParticle.nextParticle;
						if (smokeSystemParticle == null)
						{
							break;
						}
					}
				}
				lingerPosVel = base.nextParticle.vel;
			}
			else
			{
				lingerPos += lingerPosVel;
				if (room.PointSubmerged(lingerPos))
				{
					lingerPos.y = room.FloatWaterLevel(lingerPos.x);
				}
				WindAndDrag(room, ref lingerPosVel, lingerPos);
			}
			if (base.prevParticle != null)
			{
				prevLingerPosVel = base.prevParticle.vel;
				return;
			}
			prevLingerPos += prevLingerPosVel;
			if (room.PointSubmerged(prevLingerPos))
			{
				prevLingerPos.y = room.FloatWaterLevel(prevLingerPos.x);
			}
			WindAndDrag(room, ref prevLingerPosVel, prevLingerPos);
		}

		public Vector2 NextPos(float timeStacker)
		{
			if (base.nextParticle != null)
			{
				lastLingerPos = base.nextParticle.lastPos;
				lingerPos = base.nextParticle.pos;
			}
			return Vector2.Lerp(lastLingerPos, lingerPos, timeStacker);
		}

		public Vector2 PrevPos(float timeStacker)
		{
			if (base.prevParticle != null)
			{
				lastPrevLingerPos = base.prevParticle.lastPos;
				prevLingerPos = base.prevParticle.pos;
			}
			return Vector2.Lerp(lastPrevLingerPos, prevLingerPos, timeStacker);
		}

		public float PrevRad(float timeStacker)
		{
			if (base.prevParticle != null)
			{
				prevLingerRad = (base.prevParticle as SmokeSegment).MyRad(timeStacker);
			}
			return prevLingerRad;
		}

		public float NextOpacity(float timeStacker)
		{
			if (resting)
			{
				return 0f;
			}
			if (base.nextParticle != null)
			{
				return (base.nextParticle as SmokeSegment).MyOpactiy(timeStacker);
			}
			return 0f;
		}

		public Color OtherColor(float timeStacker)
		{
			if (base.nextParticle != null)
			{
				lingerColor = (base.nextParticle as SmokeSegment).MyColor(timeStacker);
			}
			return lingerColor;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[4]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(1, 2, 3),
				new TriangleMesh.Triangle(2, 3, 4),
				new TriangleMesh.Triangle(3, 4, 5)
			};
			sLeaser.sprites[0] = new TriangleMesh("Futile_White", tris, customColor: true);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[0] = new Vector2(0f, 0f);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[1] = new Vector2(1f, 0f);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[2] = new Vector2(0f, 0.5f);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[3] = new Vector2(1f, 0.5f);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[4] = new Vector2(0f, 1f);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[5] = new Vector2(1f, 1f);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (!resting)
			{
				Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
				Vector2 vector2 = NextPos(timeStacker);
				Vector2 vector3 = PrevPos(timeStacker);
				float num = MyRad(timeStacker);
				float num2 = PrevRad(timeStacker);
				Vector2 vector4 = Custom.PerpendicularVector(vector, vector3);
				Vector2 vector5 = Custom.PerpendicularVector(vector2, vector);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, Vector2.Lerp(vector, vector3, 0.2f) - vector4 * num2 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, Vector2.Lerp(vector, vector3, 0.2f) + vector4 * num2 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, Vector2.Lerp(vector, vector2, 0.2f) - vector5 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(3, Vector2.Lerp(vector, vector2, 0.2f) + vector5 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(4, Vector2.Lerp(vector2, vector, 0.2f) - vector5 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(5, Vector2.Lerp(vector2, vector, 0.2f) + vector5 * num - camPos);
				float a = 0f;
				if (base.prevParticle != null)
				{
					a = (base.prevParticle as SmokeSegment).MyOpactiy(timeStacker);
				}
				float num3 = MyOpactiy(timeStacker);
				Color color = Custom.RGB2RGBA(MyColor(timeStacker), Mathf.Min(a, num3));
				Color color2 = Custom.RGB2RGBA(MyColor(timeStacker), num3);
				Color color3 = Custom.RGB2RGBA(OtherColor(timeStacker), Mathf.Min(num3, NextOpacity(timeStacker)));
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[0] = color;
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[1] = color;
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[2] = color2;
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[3] = color2;
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[4] = color3;
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[5] = color3;
				if (this is HyrbidSmokeSegment)
				{
					(this as HyrbidSmokeSegment).HybridDraw(sLeaser, rCam, timeStacker, camPos, Vector2.Lerp(vector, vector3, 0.2f), Vector2.Lerp(vector, vector2, 0.2f), color, color2, num2, (num2 + num) * 0.5f);
				}
			}
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
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

	public abstract class HyrbidSmokeSegment : SmokeSegment
	{
		public HyrbidSmokeSegment()
		{
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[3];
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[4]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(1, 2, 3),
				new TriangleMesh.Triangle(2, 3, 4),
				new TriangleMesh.Triangle(3, 4, 5)
			};
			sLeaser.sprites[0] = new TriangleMesh("Futile_White", tris, customColor: true);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[0] = new Vector2(0f, 0f);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[1] = new Vector2(1f, 0f);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[2] = new Vector2(0f, 0.5f);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[3] = new Vector2(1f, 0.5f);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[4] = new Vector2(0f, 1f);
			(sLeaser.sprites[0] as TriangleMesh).UVvertices[5] = new Vector2(1f, 1f);
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].color = Color.green;
			sLeaser.sprites[2] = new FSprite("Futile_White");
			sLeaser.sprites[2].color = Color.green;
			AddToContainer(sLeaser, rCam, null);
		}

		public virtual void HybridDraw(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 Apos, Vector2 Bpos, Color Acol, Color Bcol, float Arad, float Brad)
		{
			sLeaser.sprites[1].x = Apos.x - camPos.x;
			sLeaser.sprites[1].y = Apos.y - camPos.y;
			sLeaser.sprites[2].x = Bpos.x - camPos.x;
			sLeaser.sprites[2].y = Bpos.y - camPos.y;
		}
	}

	public virtual float ParticleLifeTime => 1f;

	public virtual int PushApartSegments => 5;

	public MeshSmoke(SmokeType smokeType, Room room, int connectParticlesTime, float minParticleDistance)
		: base(smokeType, room, connectParticlesTime, minParticleDistance)
	{
	}
}
