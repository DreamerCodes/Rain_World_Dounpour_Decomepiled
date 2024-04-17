using UnityEngine;

public class FPNodeLink : MonoBehaviour
{
	private FNode _node;

	private bool _shouldLinkRotation;

	private bool _shouldUseLocalPosition;

	public FNode node => _node;

	public bool shouldLinkRotation
	{
		get
		{
			return _shouldLinkRotation;
		}
		set
		{
			if (_shouldLinkRotation != value)
			{
				_shouldLinkRotation = value;
				if (_shouldLinkRotation)
				{
					Update();
				}
			}
		}
	}

	public bool shouldUseLocalPosition
	{
		get
		{
			return _shouldUseLocalPosition;
		}
		set
		{
			if (_shouldUseLocalPosition != value)
			{
				_shouldUseLocalPosition = value;
				if (_shouldUseLocalPosition)
				{
					Update();
				}
			}
		}
	}

	public void Init(FNode node, bool shouldLinkRotation)
	{
		_node = node;
		_shouldLinkRotation = shouldLinkRotation;
		Update();
	}

	public void Update()
	{
		_ = _node;
		Transform transform = base.gameObject.transform;
		if (_shouldUseLocalPosition)
		{
			Vector3 localPosition = transform.localPosition;
			_node.x = localPosition.x * FPhysics.METERS_TO_POINTS;
			_node.y = localPosition.y * FPhysics.METERS_TO_POINTS;
		}
		else
		{
			Vector3 position = transform.position;
			_node.x = position.x * FPhysics.METERS_TO_POINTS;
			_node.y = position.y * FPhysics.METERS_TO_POINTS;
		}
		if (_shouldLinkRotation)
		{
			_node.rotation = 0f - base.gameObject.transform.rotation.eulerAngles.z;
		}
	}
}
