using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class CreatureSpecificAImap
{
	private static readonly AGLog<CreatureSpecificAImap> Log = new AGLog<CreatureSpecificAImap>();

	private int[,,] intGrid;

	private float[,] floatGrid;

	private IntRect coveredArea;

	public IntVector2[] accessableTiles;

	public int numberOfNodes;

	public CreatureSpecificAImap(AImap aiMap, CreatureTemplate crit)
	{
		numberOfNodes = AIdataPreprocessor.NodesRelevantToCreature(aiMap.room, crit);
		int num = aiMap.width;
		int num2 = 0;
		int num3 = 0;
		int num4 = aiMap.height;
		bool flag = false;
		for (int i = 0; i < aiMap.width; i++)
		{
			for (int j = 0; j < aiMap.height; j++)
			{
				if (aiMap.TileAccessibleToCreature(i, j, crit))
				{
					flag = true;
					if (i < num)
					{
						num = i;
					}
					if (j < num4)
					{
						num4 = j;
					}
					if (i > num3)
					{
						num3 = i;
					}
					if (j > num2)
					{
						num2 = j;
					}
				}
			}
		}
		coveredArea = new IntRect(num, num4, num3, num2);
		num3 = aiMap.width - num3 - 1;
		num2 = aiMap.height - num2 - 1;
		if (flag)
		{
			intGrid = new int[aiMap.width - num3 - num, aiMap.height - num2 - num4, numberOfNodes];
			floatGrid = new float[aiMap.width, aiMap.height];
		}
		else
		{
			intGrid = new int[1, 1, numberOfNodes];
			floatGrid = new float[1, 1];
		}
		for (int k = 0; k < aiMap.width - num3 - num; k++)
		{
			for (int l = 0; l < aiMap.height - num2 - num4; l++)
			{
				for (int m = 0; m < numberOfNodes; m++)
				{
					intGrid[k, l, m] = -1;
				}
			}
		}
		List<IntVector2> list = new List<IntVector2>();
		for (int n = 0; n < aiMap.width; n++)
		{
			for (int num5 = 0; num5 < aiMap.height; num5++)
			{
				if (aiMap.TileAccessibleToCreature(n, num5, crit))
				{
					for (int num6 = 0; num6 < numberOfNodes; num6++)
					{
						intGrid[n - coveredArea.left, num5 - coveredArea.bottom, num6] = 0;
					}
					list.Add(new IntVector2(n, num5));
				}
			}
		}
		accessableTiles = list.ToArray();
	}

	public int TriangulateDistance(IntVector2 A, IntVector2 B)
	{
		return (int)Vector2.Distance(IntVector2.ToVector2(A), IntVector2.ToVector2(B)) + 1;
	}

	public int GetDistanceToExit(int x, int y, int creatureSpecificExitIndex)
	{
		if (creatureSpecificExitIndex == -1)
		{
			return -1;
		}
		if (Custom.InsideRect(x, y, coveredArea) && creatureSpecificExitIndex < numberOfNodes)
		{
			return intGrid[x - coveredArea.left, y - coveredArea.bottom, creatureSpecificExitIndex];
		}
		return -1;
	}

	public float GetAccessibility(int x, int y)
	{
		if (x >= 0 && x < floatGrid.GetLength(0) && y >= 0 && y < floatGrid.GetLength(1))
		{
			return floatGrid[x, y];
		}
		return -1f;
	}

	public void SetDistanceToExit(int x, int y, int exitNumber, int i)
	{
		if (Custom.InsideRect(x, y, coveredArea) && intGrid[x - coveredArea.left, y - coveredArea.bottom, exitNumber] != -1 && i > -1)
		{
			intGrid[x - coveredArea.left, y - coveredArea.bottom, exitNumber] = i;
		}
	}

	public void SetAccessibility(int x, int y, float f)
	{
		if (x >= 0 && x < floatGrid.GetLength(0) && y >= 0 && y < floatGrid.GetLength(1))
		{
			floatGrid[x, y] = f;
		}
	}

	public int[] ReturnCompressedIntGrid()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < intGrid.GetLength(0); i++)
		{
			for (int j = 0; j < intGrid.GetLength(1); j++)
			{
				for (int k = 0; k < intGrid.GetLength(2); k++)
				{
					if (intGrid[i, j, k] != -1)
					{
						list.Add(intGrid[i, j, k]);
					}
				}
			}
		}
		return list.ToArray();
	}

	public float[] ReturnCompressedFloatGrid()
	{
		List<float> list = new List<float>();
		for (int i = 0; i < floatGrid.GetLength(0); i++)
		{
			for (int j = 0; j < floatGrid.GetLength(1); j++)
			{
				list.Add(floatGrid[i, j]);
			}
		}
		return list.ToArray();
	}

	public void LoadFromCompressedIntGrid(int[] intArray)
	{
		int num = 0;
		bool flag = false;
		for (int i = 0; i < intGrid.GetLength(0); i++)
		{
			if (flag)
			{
				break;
			}
			for (int j = 0; j < intGrid.GetLength(1); j++)
			{
				if (flag)
				{
					break;
				}
				for (int k = 0; k < intGrid.GetLength(2); k++)
				{
					if (flag)
					{
						break;
					}
					if (intGrid[i, j, k] != -1)
					{
						if (num >= intArray.Length)
						{
							flag = true;
							break;
						}
						intGrid[i, j, k] = intArray[num];
						num++;
					}
				}
			}
		}
		if (!flag)
		{
			return;
		}
		int num2 = 0;
		for (int l = 0; l < intGrid.GetLength(0); l++)
		{
			for (int m = 0; m < intGrid.GetLength(1); m++)
			{
				for (int n = 0; n < intGrid.GetLength(2); n++)
				{
					if (intGrid[l, m, n] != -1)
					{
						num2++;
					}
				}
			}
		}
	}

	public void LoadFromCompressedFloatGrid(float[] floatArray)
	{
		int num = 0;
		for (int i = 0; i < floatGrid.GetLength(0); i++)
		{
			for (int j = 0; j < floatGrid.GetLength(1); j++)
			{
				floatGrid[i, j] = floatArray[num];
				num++;
			}
		}
	}
}
