using RWCustom;
using UnityEngine;

public class Dangler
{
	public class DanglerSegment : BodyPart
	{
		public Dangler dangler;

		public int index;

		public new float rad;

		public float stretchedRad;

		public float conRad;

		public GraphicsModule gModule => dangler.gModule;

		public DanglerSegment(Dangler dangler, int index, float rad, float conRad)
			: base(dangler.gModule)
		{
			this.dangler = dangler;
			this.index = index;
			this.rad = rad;
			this.conRad = conRad;
		}

		public override void Update()
		{
			base.Update();
			lastPos = pos;
			pos += vel;
			if (gModule.owner.room.PointSubmerged(pos))
			{
				vel.y -= dangler.Props.waterGravity;
				vel *= dangler.Props.waterFriction;
			}
			else
			{
				vel *= dangler.Props.airFriction;
				vel.y -= dangler.Props.gravity;
			}
			Vector2 vector = dangler.ConPos(1f);
			float elasticity = dangler.Props.elasticity;
			if (index == 0)
			{
				Vector2 vector2 = Custom.DirVec(pos, vector);
				float num = Vector2.Distance(pos, vector);
				float num2 = conRad;
				pos -= vector2 * (num2 - num);
				vel -= vector2 * (num2 - num);
				stretchedRad = rad * Mathf.Clamp(Mathf.Lerp(Mathf.Pow(num2 / num, 0.5f), 1f, 0.5f), 0.2f, 1.8f);
			}
			else
			{
				Vector2 vector3 = Custom.DirVec(pos, dangler.segments[index - 1].pos);
				float num3 = Vector2.Distance(pos, dangler.segments[index - 1].pos);
				float num4 = conRad;
				float a = dangler.segments[index - 1].rad / (rad + dangler.segments[index - 1].rad);
				a = Mathf.Lerp(a, dangler.Props.weightSymmetryTendency, 0.5f);
				pos -= vector3 * (num4 - num3) * elasticity * a;
				vel -= vector3 * (num4 - num3) * elasticity * a;
				dangler.segments[index - 1].pos += vector3 * (num4 - num3) * elasticity * (1f - a);
				dangler.segments[index - 1].vel += vector3 * (num4 - num3) * elasticity * (1f - a);
				stretchedRad = rad * Mathf.Clamp(Mathf.Lerp(Mathf.Pow(num4 / num3, 0.5f), 1f, 0.5f), 0.2f, 1.8f);
			}
			if (index > 1)
			{
				for (int i = 0; i < dangler.gModule.owner.bodyChunks.Length; i++)
				{
					if (dangler.gModule.owner.bodyChunks[i].collideWithObjects)
					{
						PushFromPoint(dangler.gModule.owner.bodyChunks[i].pos, dangler.gModule.owner.bodyChunks[i].rad, 1f);
					}
				}
			}
			if (!OnOtherSideOfTerrain(vector, FromBaseRadius() * 1.2f))
			{
				PushOutOfTerrain(gModule.owner.room, vector);
			}
		}

		public override void Reset(Vector2 resetPos)
		{
			pos = resetPos;
			lastPos = resetPos;
			vel *= 0f;
		}

		public float FromBaseRadius()
		{
			if (index == 0)
			{
				return conRad;
			}
			return dangler.segments[index - 1].FromBaseRadius() + conRad;
		}
	}

	public class DanglerProps
	{
		public float gravity;

		public float airFriction;

		public float waterGravity;

		public float waterFriction;

		public float elasticity;

		public float weightSymmetryTendency;

		public DanglerProps()
		{
			gravity = 0.9f;
			airFriction = 0.98f;
			waterGravity = -0.2f;
			waterFriction = 0.8f;
			elasticity = 0.85f;
			weightSymmetryTendency = 0.7f;
		}
	}

	public GraphicsModule gModule;

	public DanglerSegment[] segments;

	public int danglerNum;

	public DanglerProps Props => (gModule as HasDanglers).Props(danglerNum);

	public Vector2 ConPos(float timeStacker)
	{
		return (gModule as HasDanglers).DanglerConnection(danglerNum, timeStacker);
	}

	public Dangler(GraphicsModule gModule, int danglerNum, int segs, float baseRad, float conRad)
	{
		this.gModule = gModule;
		this.danglerNum = danglerNum;
		segments = new DanglerSegment[segs];
		for (int i = 0; i < segs; i++)
		{
			segments[i] = new DanglerSegment(this, i, baseRad, conRad);
		}
	}

	public void Reset()
	{
		Vector2 resetPoint = ConPos(1f);
		for (int i = 0; i < segments.Length; i++)
		{
			resetPoint.y -= segments[i].conRad;
			segments[i].Reset(resetPoint);
		}
	}

	public void Update()
	{
		for (int i = 0; i < segments.Length; i++)
		{
			segments[i].Update();
		}
	}

	public void InitSprite(RoomCamera.SpriteLeaser sLeaser, int spriteIndex)
	{
		sLeaser.sprites[spriteIndex] = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: false);
	}

	public void DrawSprite(int spriteIndex, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = ConPos(timeStacker);
		float stretchedRad = segments[0].stretchedRad;
		for (int i = 0; i < segments.Length; i++)
		{
			Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
			Vector2 normalized = (vector2 - vector).normalized;
			Vector2 vector3 = Custom.PerpendicularVector(normalized);
			float num = Vector2.Distance(vector2, vector) / 5f;
			if (i == 0)
			{
				(sLeaser.sprites[spriteIndex] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * (stretchedRad + segments[i].stretchedRad) * 0.5f - camPos);
				(sLeaser.sprites[spriteIndex] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * (stretchedRad + segments[i].stretchedRad) * 0.5f - camPos);
			}
			else
			{
				(sLeaser.sprites[spriteIndex] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * (stretchedRad + segments[i].stretchedRad) * 0.5f + normalized * num - camPos);
				(sLeaser.sprites[spriteIndex] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * (stretchedRad + segments[i].stretchedRad) * 0.5f + normalized * num - camPos);
			}
			(sLeaser.sprites[spriteIndex] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * segments[i].stretchedRad - normalized * num - camPos);
			(sLeaser.sprites[spriteIndex] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * segments[i].stretchedRad - normalized * num - camPos);
			stretchedRad = segments[i].stretchedRad;
			vector = vector2;
		}
	}
}
