using System;
using RWCustom;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CoralBrain;

[BurstCompile]
internal struct MyceliumJob01 : IJob
{
	[ReadOnly]
	public int pointslen;

	[ReadOnly]
	public float conRad;

	[ReadOnly]
	public bool moveAwayFromWalls;

	[ReadOnly]
	public int aiMapWidth;

	[ReadOnly]
	public int aiMapHeight;

	public NativeArray<float2> points;

	[ReadOnly]
	public NativeArray<int> terrainProximity;

	[ReadOnly]
	public NativeArray<int2> naFourDirections;

	[WriteOnly]
	public NativeArray<Vector2> outPoints;

	public void Execute()
	{
		int width = 3;
		ExtraExtentions.Indexer indexer = default(ExtraExtentions.Indexer);
		indexer.width = width;
		ExtraExtentions.Indexer indexer2 = indexer;
		for (int i = 0; i < pointslen; i++)
		{
			if (Hint.Likely(i > 0))
			{
				float2 f = new float2(points[indexer2.ind(i, 0)].x - points[indexer2.ind(i - 1, 0)].x, points[indexer2.ind(i, 0)].y - points[indexer2.ind(i - 1, 0)].y);
				float num = f.magnitude();
				float2 @float = f.normalized();
				float2 float2 = new float2(@float.x * (conRad - num) * 0.5f, @float.y * (conRad - num) * 0.5f);
				points[indexer2.ind(i, 0)] = new float2(points[indexer2.ind(i, 0)].x + float2.x, points[indexer2.ind(i, 0)].y + float2.y);
				points[indexer2.ind(i, 2)] = new float2(points[indexer2.ind(i, 2)].x + float2.x, points[indexer2.ind(i, 2)].y + float2.y);
				points[indexer2.ind(i - 1, 0)] = new float2(points[indexer2.ind(i - 1, 0)].x - float2.x, points[indexer2.ind(i - 1, 0)].y - float2.y);
				points[indexer2.ind(i - 1, 2)] = new float2(points[indexer2.ind(i - 1, 2)].x - float2.x, points[indexer2.ind(i - 1, 2)].y - float2.y);
				if (Hint.Likely(i > 1))
				{
					@float = new float2(points[indexer2.ind(i, 0)].x - points[indexer2.ind(i - 2, 0)].x, points[indexer2.ind(i, 0)].y - points[indexer2.ind(i - 2, 0)].y).normalized();
					points[indexer2.ind(i, 2)] = new float2(points[indexer2.ind(i, 2)].x + @float.x * 0.2f, points[indexer2.ind(i, 2)].y + @float.y * 0.2f);
					points[indexer2.ind(i - 2, 2)] = new float2(points[indexer2.ind(i - 2, 2)].x - @float.x * 0.2f, points[indexer2.ind(i - 2, 2)].y - @float.y * 0.2f);
				}
			}
			int2 @int = Room.StaticGetTilePosition(points[indexer2.ind(i, 0)]);
			int num2 = int.MaxValue;
			if (aiMapHeight != -1 && @int.x >= 0 && @int.y >= 0 && @int.x < aiMapWidth && @int.y < aiMapHeight)
			{
				num2 = terrainProximity[ExtraExtentions.ind(@int.x, @int.y, aiMapHeight)];
			}
			if (moveAwayFromWalls && num2 < 4)
			{
				float2 f2 = new float2(0f, 0f);
				for (int j = 0; j < 4; j++)
				{
					int num3 = 0;
					for (int k = 0; k < 4; k++)
					{
						int2 int2 = @int + naFourDirections[j] + naFourDirections[k];
						if (int2.x >= 0 && int2.y >= 0 && int2.x < aiMapWidth && int2.y < aiMapHeight)
						{
							num3 += terrainProximity[ExtraExtentions.ind(int2.x, int2.y, aiMapHeight)];
						}
					}
					f2 = new float2(f2.x + (float)(naFourDirections[j].x * num3), f2.y + (float)(naFourDirections[j].y * num3));
				}
				float num4 = Custom.LerpMap(num2, 0f, 3f, 2f, 0.2f);
				float2 float3 = f2.normalized();
				points[indexer2.ind(i, 2)] = new float2(points[indexer2.ind(i, 2)].x + float3.x * num4, points[indexer2.ind(i, 2)].y + float3.y * num4);
			}
			outPoints[indexer2.ind(i, 1)] = new Vector2(points[indexer2.ind(i, 1)].x, points[indexer2.ind(i, 1)].y);
			if (Hint.Likely(i > 1))
			{
				outPoints[indexer2.ind(i - 2, 0)] = new Vector2(points[indexer2.ind(i - 2, 0)].x, points[indexer2.ind(i - 2, 0)].y);
				outPoints[indexer2.ind(i - 2, 2)] = new Vector2(points[indexer2.ind(i - 2, 2)].x, points[indexer2.ind(i - 2, 2)].y);
			}
		}
		for (int l = Math.Max(0, pointslen - 3); l < pointslen; l++)
		{
			outPoints[indexer2.ind(l, 0)] = new Vector2(points[indexer2.ind(l, 0)].x, points[indexer2.ind(l, 0)].y);
			outPoints[indexer2.ind(l, 2)] = new Vector2(points[indexer2.ind(l, 2)].x, points[indexer2.ind(l, 2)].y);
		}
	}
}
