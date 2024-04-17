using UnityEngine;

public class FGameObjectNode : FNode, FRenderableLayerInterface
{
	protected GameObject _gameObject;

	protected int _renderQueueDepth = -1;

	protected bool _shouldLinkPosition;

	protected bool _shouldLinkRotation;

	protected bool _shouldLinkScale;

	protected Vector3 _initialGameObjectScale = new Vector3(1f, 1f, 1f);

	protected float _initialGameObjectRotationZ;

	protected float _previousGameObjectRotationZ;

	public bool shouldDestroyOnRemoveFromStage = true;

	public GameObject gameObject
	{
		get
		{
			return _gameObject;
		}
		set
		{
			if (_gameObject != value)
			{
				Unsetup();
				_gameObject = value;
				Setup();
			}
		}
	}

	public int renderQueueDepth => _renderQueueDepth;

	public FGameObjectNode(GameObject gameObject, bool shouldLinkPosition, bool shouldLinkRotation, bool shouldLinkScale)
	{
		Init(gameObject, shouldLinkPosition, shouldLinkRotation, shouldLinkScale);
	}

	protected FGameObjectNode()
	{
	}

	protected void Init(GameObject gameObject, bool shouldLinkPosition, bool shouldLinkRotation, bool shouldLinkScale)
	{
		_gameObject = gameObject;
		_shouldLinkPosition = shouldLinkPosition;
		_shouldLinkRotation = shouldLinkRotation;
		_shouldLinkScale = shouldLinkScale;
		_initialGameObjectScale = gameObject.transform.localScale;
		_initialGameObjectRotationZ = gameObject.transform.rotation.eulerAngles.z;
		_previousGameObjectRotationZ = _initialGameObjectRotationZ;
		Setup();
	}

	protected void Setup()
	{
		if (_isOnStage)
		{
			_gameObject.transform.parent = Futile.instance.gameObject.transform;
			if (_gameObject.GetComponent<Renderer>() != null && _gameObject.GetComponent<Renderer>().material != null)
			{
				_gameObject.GetComponent<Renderer>().material.renderQueue = _renderQueueDepth;
			}
			_gameObject.layer = _stage.layer;
			UpdateGameObject();
		}
	}

	protected void Unsetup()
	{
		if (_gameObject != null)
		{
			_gameObject.transform.parent = null;
		}
	}

	public override void HandleAddedToStage()
	{
		if (!_isOnStage)
		{
			base.HandleAddedToStage();
			_stage.HandleFacetsChanged();
			_gameObject.transform.parent = Futile.instance.gameObject.transform;
			UpdateGameObject();
		}
	}

	public override void HandleRemovedFromStage()
	{
		if (_isOnStage)
		{
			base.HandleRemovedFromStage();
			_gameObject.transform.parent = null;
			if (shouldDestroyOnRemoveFromStage)
			{
				Object.Destroy(_gameObject);
			}
			_stage.HandleFacetsChanged();
		}
	}

	public override void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		shouldForceDirty = true;
		bool num = _isMatrixDirty;
		bool isAlphaDirty = _isAlphaDirty;
		bool flag = false;
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		if (shouldUpdateDepth)
		{
			flag = true;
			UpdateDepth();
		}
		if (num || shouldForceDirty || shouldUpdateDepth)
		{
			flag = true;
		}
		if (isAlphaDirty || shouldForceDirty)
		{
			flag = true;
		}
		if (flag)
		{
			UpdateGameObject();
		}
	}

	protected void UpdateDepth()
	{
		base.stage.renderer.AddRenderableLayer(this);
	}

	public virtual void Update(int depth)
	{
		_renderQueueDepth = Futile.baseRenderQueueDepth + depth;
		if (_gameObject.GetComponent<Renderer>() != null && _gameObject.GetComponent<Renderer>().material != null)
		{
			_gameObject.GetComponent<Renderer>().material.renderQueue = _renderQueueDepth;
		}
	}

	public void UpdateGameObject()
	{
		if (_isOnStage)
		{
			FMatrix fMatrix = screenConcatenatedMatrix;
			if (_shouldLinkPosition)
			{
				_gameObject.transform.localPosition = fMatrix.GetVector3FromLocalVector2(Vector2.zero, 0f);
			}
			if (_shouldLinkRotation)
			{
				float num = fMatrix.GetRotation();
				_gameObject.transform.Rotate(0f, 0f, _previousGameObjectRotationZ - _initialGameObjectRotationZ - num, Space.World);
				_previousGameObjectRotationZ = _initialGameObjectRotationZ + num;
			}
			if (_shouldLinkScale)
			{
				_gameObject.transform.localScale = new Vector3(_initialGameObjectScale.x * fMatrix.GetScaleX(), _initialGameObjectScale.y * fMatrix.GetScaleY(), _initialGameObjectScale.z);
			}
			_gameObject.layer = _stage.layer;
		}
	}
}
