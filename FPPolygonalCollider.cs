using UnityEngine;

public class FPPolygonalCollider : MonoBehaviour
{
	private FPPolygonalData _polygonalData;

	private MeshCollider[] _colliders;

	public FPPolygonalData polygonalData => _polygonalData;

	public MeshCollider[] colliders => _colliders;

	public void Init(FPPolygonalData polygonalData)
	{
		_polygonalData = polygonalData;
		int num = _polygonalData.meshes.Length;
		_colliders = new MeshCollider[num];
		if (num == 1)
		{
			_colliders[0] = CreatePolygonMeshCollider(base.gameObject, _polygonalData.meshes[0]);
			return;
		}
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = new GameObject("Decomposed Convex Polygon");
			gameObject.transform.parent = base.gameObject.transform;
			gameObject.transform.localPosition = Vector3.zero;
			_colliders[i] = CreatePolygonMeshCollider(gameObject, _polygonalData.meshes[i]);
		}
	}

	private MeshCollider CreatePolygonMeshCollider(GameObject polygonGameObject, Mesh mesh)
	{
		MeshCollider meshCollider = polygonGameObject.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;
		if (_polygonalData.hasBeenDecomposedIntoConvexPolygons)
		{
			meshCollider.convex = true;
		}
		else
		{
			meshCollider.convex = FPUtils.CheckIfConvex(_polygonalData.sourceVertices);
		}
		return meshCollider;
	}

	public void OnDestroy()
	{
	}
}
