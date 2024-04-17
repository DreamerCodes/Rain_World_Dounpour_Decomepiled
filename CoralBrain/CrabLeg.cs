using RWCustom;
using UnityEngine;

namespace CoralBrain;

public class CrabLeg
{
	public Vector2[,] points;

	public float conRad;

	public Color color;

	public CircuitCrab owner;

	public int index;

	public CrabLeg(CircuitCrab owner, int index, float length, Vector2 initPoint)
	{
		this.owner = owner;
		this.index = index;
		points = new Vector2[Custom.IntClamp((int)(length / 10f), 1, 100), 3];
		conRad = length / (float)points.GetLength(0);
		for (int i = 0; i < points.GetLength(0); i++)
		{
			points[i, 0] = initPoint + Custom.RNV();
			points[i, 1] = points[i, 0];
			points[i, 2] = Custom.RNV();
		}
	}

	public void Update()
	{
		for (int i = 0; i < points.GetLength(0); i++)
		{
			points[i, 1] = points[i, 0];
			points[i, 0] += points[i, 2];
			points[i, 2] *= 0.999f;
		}
		for (int j = 0; j < points.GetLength(0); j++)
		{
			if (j > 0)
			{
				Vector2 normalized = (points[j, 0] - points[j - 1, 0]).normalized;
				float num = Vector2.Distance(points[j, 0], points[j - 1, 0]);
				points[j, 0] += normalized * (conRad - num) * 0.5f;
				points[j, 2] += normalized * (conRad - num) * 0.5f;
				points[j - 1, 0] -= normalized * (conRad - num) * 0.5f;
				points[j - 1, 2] -= normalized * (conRad - num) * 0.5f;
				if (j > 1)
				{
					normalized = (points[j, 0] - points[j - 2, 0]).normalized;
					points[j, 2] += normalized * 0.2f;
					points[j - 2, 2] -= normalized * 0.2f;
				}
			}
		}
		points[0, 0] = owner.ConnectionPos(index, 1f);
		points[0, 2] *= 0f;
	}

	public void InitiateSprites(int spr, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[spr] = TriangleMesh.MakeLongMesh(points.GetLength(0), pointyTip: false, customColor: false);
		sLeaser.sprites[spr].color = color;
	}

	public void DrawSprites(int spr, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[spr].color = color;
		Vector2 vector = Vector2.Lerp(points[0, 1], points[0, 0], timeStacker);
		vector = owner.ConnectionPos(index, timeStacker);
		float num = 0.5f;
		for (int i = 0; i < points.GetLength(0); i++)
		{
			Vector2 vector2 = Vector2.Lerp(points[i, 1], points[i, 0], timeStacker);
			Vector2 normalized = (vector - vector2).normalized;
			Vector2 vector3 = Custom.PerpendicularVector(normalized);
			float num2 = Vector2.Distance(vector, vector2) / 5f;
			(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4, vector - normalized * num2 - vector3 * num - camPos);
			(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4 + 1, vector - normalized * num2 + vector3 * num - camPos);
			(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 + normalized * num2 - vector3 * num - camPos);
			(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized * num2 + vector3 * num - camPos);
			vector = vector2;
		}
	}
}
