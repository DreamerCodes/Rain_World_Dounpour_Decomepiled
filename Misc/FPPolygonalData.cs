using System;
using System.Collections.Generic;
using UnityEngine;

public class FPPolygonalData
{
	public bool hasBeenDecomposedIntoConvexPolygons;

	public bool shouldUseSmoothSphereCollisions;

	public Vector2[] sourceVertices;

	public List<Vector2[]> vertexPolygons;

	public List<int[]> trianglePolygons;

	public Mesh[] meshes;

	public FPPolygonalData(Vector2[] vertices, bool shouldDecomposeIntoConvexPolygons)
	{
		sourceVertices = vertices;
		if (shouldDecomposeIntoConvexPolygons)
		{
			hasBeenDecomposedIntoConvexPolygons = true;
			List<Vector2> list = new List<Vector2>(sourceVertices);
			list.Reverse();
			vertexPolygons = FPDecomposer.Decompose(list);
			int count = vertexPolygons.Count;
			meshes = new Mesh[count];
			trianglePolygons = new List<int[]>(count);
			for (int i = 0; i < count; i++)
			{
				Array.Reverse(vertexPolygons[i]);
				trianglePolygons.Add(null);
				meshes[i] = CreateMeshFromPolygon(i);
			}
		}
		else
		{
			hasBeenDecomposedIntoConvexPolygons = false;
			meshes = new Mesh[1];
			vertexPolygons = new List<Vector2[]>(1);
			vertexPolygons.Add(sourceVertices);
			trianglePolygons = new List<int[]>(1);
			trianglePolygons.Add(null);
			meshes[0] = CreateMeshFromPolygon(0);
		}
	}

	private Mesh CreateMeshFromPolygon(int polygonIndex)
	{
		Vector2[] array = vertexPolygons[polygonIndex];
		int[] array2 = trianglePolygons[polygonIndex];
		int num = array.Length;
		if (array2 == null)
		{
			int[] array4 = (trianglePolygons[polygonIndex] = FPUtils.Triangulate(array));
			array2 = array4;
		}
		int num2 = array2.Length;
		Mesh mesh = new Mesh();
		Vector3[] array5 = new Vector3[num * 2];
		int[] array6 = new int[num2 * 2 + num * 6];
		for (int i = 0; i < num2; i += 3)
		{
			array6[i] = array2[i];
			array6[i + 1] = array2[i + 1];
			array6[i + 2] = array2[i + 2];
			int num3 = num2 + i;
			array6[num3] = num + array2[i];
			array6[num3 + 2] = num + array2[i + 1];
			array6[num3 + 1] = num + array2[i + 2];
		}
		int num4 = num2 * 2;
		for (int j = 0; j < num; j++)
		{
			Vector2 vector = array[j];
			Vector3 vector2 = (array5[j] = new Vector3(vector.x * FPhysics.POINTS_TO_METERS, vector.y * FPhysics.POINTS_TO_METERS, 0f));
			vector2.z = 1f;
			array5[j + num] = vector2;
			int num5 = j * 6;
			array6[num4 + num5] = j;
			array6[num4 + num5 + 1] = j + num;
			array6[num4 + num5 + 2] = (j + 1) % num + num;
			array6[num4 + num5 + 3] = j;
			array6[num4 + num5 + 4] = (j + 1) % num + num;
			array6[num4 + num5 + 5] = (j + 1) % num;
		}
		mesh.vertices = array5;
		mesh.triangles = array6;
		return mesh;
	}

	public FPPolygonalData(List<Vector2[]> vertexPolygons, List<int[]> trianglePolygons)
	{
		hasBeenDecomposedIntoConvexPolygons = true;
		int count = vertexPolygons.Count;
		this.vertexPolygons = vertexPolygons;
		this.trianglePolygons = trianglePolygons;
		meshes = new Mesh[count];
		for (int i = 0; i < count; i++)
		{
			meshes[i] = CreateMeshFromPolygon(i);
		}
	}

	public FPPolygonalData(List<Vector2[]> vertexPolygons)
	{
		hasBeenDecomposedIntoConvexPolygons = true;
		int count = vertexPolygons.Count;
		this.vertexPolygons = vertexPolygons;
		trianglePolygons = new List<int[]>(count);
		meshes = new Mesh[count];
		for (int i = 0; i < count; i++)
		{
			trianglePolygons.Add(FPUtils.Triangulate(vertexPolygons[i]));
			meshes[i] = CreateMeshFromPolygon(i);
		}
	}
}
