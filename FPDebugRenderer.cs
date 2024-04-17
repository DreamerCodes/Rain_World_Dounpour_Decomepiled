using System.Collections.Generic;
using UnityEngine;

public class FPDebugRenderer : MonoBehaviour
{
	public static bool IS_ENABLED = true;

	private FContainer _container;

	private FContainer _drawHolder;

	private List<FNode> _nodes = new List<FNode>(1);

	private Color _color;

	public bool shouldUpdateColliders;

	public static FPDebugRenderer Create(GameObject targetGameObject, FContainer container, uint color, bool shouldUpdateColliders)
	{
		if (!IS_ENABLED)
		{
			return null;
		}
		FPDebugRenderer fPDebugRenderer = targetGameObject.AddComponent<FPDebugRenderer>();
		fPDebugRenderer.Init(container, color, shouldUpdateColliders);
		return fPDebugRenderer;
	}

	public void Init(FContainer container, uint color, bool shouldUpdateColliders)
	{
		_container = container;
		_container.AddChild(_drawHolder = new FContainer());
		_color = RXUtils.GetColorFromHex(color);
		this.shouldUpdateColliders = shouldUpdateColliders;
		Collider[] components = base.gameObject.GetComponents<Collider>();
		int num = components.Length;
		for (int i = 0; i < num; i++)
		{
			Collider collider = components[i];
			FNode fNode = null;
			if (collider is BoxCollider)
			{
				fNode = new FSprite("Debug/Square")
				{
					color = _color
				};
			}
			else if (collider is SphereCollider)
			{
				fNode = new FSprite("Debug/Circle")
				{
					color = _color
				};
			}
			if (fNode != null)
			{
				_drawHolder.AddChild(fNode);
				_nodes.Add(fNode);
			}
		}
		FPPolygonalCollider component = base.gameObject.GetComponent<FPPolygonalCollider>();
		if (component != null)
		{
			FPDebugPolygonColliderView fPDebugPolygonColliderView = new FPDebugPolygonColliderView("Debug/Triangle", component);
			fPDebugPolygonColliderView.color = _color;
			_drawHolder.AddChild(fPDebugPolygonColliderView);
			_nodes.Add(fPDebugPolygonColliderView);
		}
		Update();
		if (!shouldUpdateColliders)
		{
			UpdateColliders();
		}
	}

	public void Update()
	{
		Transform transform = base.gameObject.transform;
		Vector3 position = transform.position;
		_drawHolder.x = position.x * FPhysics.METERS_TO_POINTS;
		_drawHolder.y = position.y * FPhysics.METERS_TO_POINTS;
		_drawHolder.rotation = 0f - transform.rotation.eulerAngles.z;
		if (shouldUpdateColliders)
		{
			UpdateColliders();
		}
	}

	public void UpdateColliders()
	{
		Collider[] components = base.gameObject.GetComponents<Collider>();
		int num = components.Length;
		for (int i = 0; i < num; i++)
		{
			Collider collider = components[i];
			if (collider is BoxCollider)
			{
				BoxCollider obj = collider as BoxCollider;
				Vector3 size = obj.size;
				Vector3 center = obj.center;
				FSprite obj2 = _nodes[i] as FSprite;
				obj2.width = size.x * FPhysics.METERS_TO_POINTS;
				obj2.height = size.y * FPhysics.METERS_TO_POINTS;
				obj2.x = center.x * FPhysics.METERS_TO_POINTS;
				obj2.y = center.y * FPhysics.METERS_TO_POINTS;
			}
			else if (collider is SphereCollider)
			{
				SphereCollider sphereCollider = collider as SphereCollider;
				Vector3 center2 = sphereCollider.center;
				FSprite obj3 = _nodes[i] as FSprite;
				float width = (obj3.height = sphereCollider.radius * 2f * FPhysics.METERS_TO_POINTS);
				obj3.width = width;
				obj3.x = center2.x * FPhysics.METERS_TO_POINTS;
				obj3.y = center2.y * FPhysics.METERS_TO_POINTS;
			}
		}
	}

	public void OnDestroy()
	{
		_drawHolder.RemoveFromContainer();
	}
}
