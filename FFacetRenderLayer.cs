using System;
using UnityEngine;
using UnityEngine.Rendering;

public class FFacetRenderLayer : FRenderableLayerInterface
{
	public int batchIndex;

	protected FStage _stage;

	protected FFacetType _facetType;

	protected FAtlas _atlas;

	protected FShader _shader;

	protected GameObject _gameObject;

	protected Transform _transform;

	protected Material _material;

	protected MeshFilter _meshFilter;

	protected MeshRenderer _meshRenderer;

	protected Mesh _mesh;

	protected Vector3[] _vertices = new Vector3[0];

	protected int[] _triangles = new int[0];

	protected Vector2[] _uvs = new Vector2[0];

	protected Color[] _colors = new Color[0];

	protected bool _isMeshDirty;

	protected bool _didVertsChange;

	protected bool _didUVsChange;

	protected bool _didColorsChange;

	protected bool _didVertCountChange;

	protected bool _doesMeshNeedClear;

	protected int _expansionAmount;

	protected int _maxEmptyFacets;

	protected int _maxFacetCount;

	protected int _depth = -1;

	protected int _nextAvailableFacetIndex;

	protected int _lowestZeroIndex;

	public int expansionAmount
	{
		get
		{
			return _expansionAmount;
		}
		set
		{
			_expansionAmount = value;
		}
	}

	public Vector3[] vertices => _vertices;

	public Vector2[] uvs => _uvs;

	public Color[] colors => _colors;

	public FAtlas atlas => _atlas;

	public FFacetRenderLayer(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader)
	{
		_stage = stage;
		_facetType = facetType;
		_atlas = atlas;
		_shader = shader;
		_expansionAmount = _facetType.expansionAmount;
		_maxEmptyFacets = _facetType.maxEmptyAmount;
		batchIndex = _facetType.index * 10000000 + atlas.index * 10000 + shader.index;
		_gameObject = new GameObject("FRenderLayer (" + _stage.name + ") (" + _facetType.name + ")");
		_transform = _gameObject.transform;
		_transform.parent = Futile.instance.gameObject.transform;
		_meshFilter = _gameObject.AddComponent<MeshFilter>();
		_meshRenderer = _gameObject.AddComponent<MeshRenderer>();
		_meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		_meshRenderer.receiveShadows = false;
		_mesh = _meshFilter.mesh;
		_material = new Material(_shader.shader);
		_material.mainTexture = _atlas.texture;
		_meshRenderer.GetComponent<Renderer>().sharedMaterial = _material;
		_gameObject.SetActive(value: false);
		_mesh.MarkDynamic();
		ExpandMaxFacetLimit(_facetType.initialAmount);
		UpdateTransform();
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(_gameObject);
		UnityEngine.Object.Destroy(_mesh);
		UnityEngine.Object.Destroy(_material);
	}

	public void UpdateTransform()
	{
		_transform.position = _stage.transform.position;
		_transform.rotation = _stage.transform.rotation;
		_transform.localScale = _stage.transform.localScale;
		_gameObject.layer = _stage.layer;
	}

	public void AddToWorld()
	{
		_gameObject.SetActive(value: true);
	}

	public void RemoveFromWorld()
	{
		_gameObject.SetActive(value: false);
	}

	public void Open()
	{
		_nextAvailableFacetIndex = 0;
	}

	public int GetNextFacetIndex(int numberOfFacetsNeeded)
	{
		int nextAvailableFacetIndex = _nextAvailableFacetIndex;
		_nextAvailableFacetIndex += numberOfFacetsNeeded;
		if (_nextAvailableFacetIndex - 1 >= _maxFacetCount)
		{
			int val = _nextAvailableFacetIndex - _maxFacetCount + 1;
			ExpandMaxFacetLimit(Math.Max(val, _expansionAmount));
		}
		return nextAvailableFacetIndex;
	}

	public void Close()
	{
		if (_nextAvailableFacetIndex < _maxFacetCount - _maxEmptyFacets)
		{
			ShrinkMaxFacetLimit(Math.Max(0, _maxFacetCount - _nextAvailableFacetIndex - _expansionAmount));
		}
		FillUnusedFacetsWithZeroes();
	}

	protected virtual void FillUnusedFacetsWithZeroes()
	{
		throw new NotImplementedException("Override me!");
	}

	protected virtual void ShrinkMaxFacetLimit(int deltaDecrease)
	{
		throw new NotImplementedException("Override me!");
	}

	protected virtual void ExpandMaxFacetLimit(int deltaIncrease)
	{
		throw new NotImplementedException("Override me!");
	}

	public void Update(int depth)
	{
		if (_depth != depth)
		{
			_depth = depth;
			_material.renderQueue = Futile.baseRenderQueueDepth + _depth;
		}
		if (_isMeshDirty)
		{
			UpdateMeshProperties();
		}
	}

	protected void UpdateMeshProperties()
	{
		_isMeshDirty = false;
		if (_didVertCountChange)
		{
			_didVertCountChange = false;
			_didColorsChange = false;
			_didVertsChange = false;
			_didUVsChange = false;
			if (_doesMeshNeedClear)
			{
				_mesh.Clear();
			}
			_mesh.vertices = _vertices;
			_mesh.triangles = _triangles;
			_mesh.uv = _uvs;
			_mesh.bounds = new Bounds(Vector3.zero, new Vector3(1E+10f, 1E+10f, 1E+10f));
			_mesh.colors = _colors;
		}
		else
		{
			if (_didVertsChange)
			{
				_didVertsChange = false;
				_mesh.vertices = _vertices;
			}
			if (_didColorsChange)
			{
				_didColorsChange = false;
				_mesh.colors = _colors;
			}
			if (_didUVsChange)
			{
				_didUVsChange = false;
				_mesh.uv = _uvs;
			}
		}
	}

	public void HandleVertsChange()
	{
		_didVertsChange = true;
		_didUVsChange = true;
		_didColorsChange = true;
		_isMeshDirty = true;
	}
}
