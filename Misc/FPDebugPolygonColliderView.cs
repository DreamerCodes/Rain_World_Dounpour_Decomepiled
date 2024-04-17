using System.Collections.Generic;
using UnityEngine;

public class FPDebugPolygonColliderView : FFacetElementNode
{
	private FPPolygonalCollider _mesh2D;

	private int _triangleCount;

	private Color _color = Futile.white;

	private Color _alphaColor = Futile.white;

	private bool _isMeshDirty;

	private bool _areLocalVerticesDirty;

	private Vector2 _uvTopLeft;

	private Vector2 _uvBottomLeft;

	private Vector2 _uvBottomRight;

	public virtual Color color
	{
		get
		{
			return _color;
		}
		set
		{
			if (_color != value)
			{
				_color = value;
				_isAlphaDirty = true;
			}
		}
	}

	public FPDebugPolygonColliderView(string elementName, FPPolygonalCollider mesh2D)
	{
		_mesh2D = mesh2D;
		List<int[]> trianglePolygons = _mesh2D.polygonalData.trianglePolygons;
		int count = trianglePolygons.Count;
		_triangleCount = 0;
		for (int i = 0; i < count; i++)
		{
			_triangleCount += trianglePolygons[i].Length / 3;
		}
		Init(FFacetType.Triangle, Futile.atlasManager.GetElementWithName(elementName), _triangleCount);
		_isAlphaDirty = true;
		UpdateLocalVertices();
	}

	public override void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool num = _isMatrixDirty;
		bool isAlphaDirty = _isAlphaDirty;
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		if (shouldUpdateDepth)
		{
			UpdateFacets();
		}
		if (num || shouldForceDirty || shouldUpdateDepth)
		{
			_isMeshDirty = true;
		}
		if (isAlphaDirty || shouldForceDirty)
		{
			_isMeshDirty = true;
			_color.ApplyMultipliedAlpha(ref _alphaColor, _concatenatedAlpha);
		}
		if (_areLocalVerticesDirty)
		{
			UpdateLocalVertices();
		}
		if (_isMeshDirty)
		{
			PopulateRenderLayer();
		}
	}

	public override void HandleElementChanged()
	{
		_areLocalVerticesDirty = true;
	}

	public virtual void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
		_uvTopLeft = _element.uvTopLeft;
		_uvBottomLeft = _element.uvBottomLeft;
		_uvBottomRight = _element.uvBottomRight;
		List<int[]> trianglePolygons = _mesh2D.polygonalData.trianglePolygons;
		int count = trianglePolygons.Count;
		_triangleCount = 0;
		for (int i = 0; i < count; i++)
		{
			_triangleCount += trianglePolygons[i].Length / 3;
		}
		if (_numberOfFacetsNeeded != _triangleCount)
		{
			_numberOfFacetsNeeded = _triangleCount;
			if (_isOnStage)
			{
				_stage.HandleFacetsChanged();
			}
		}
		_isMeshDirty = true;
	}

	public override void PopulateRenderLayer()
	{
		if (!_isOnStage || _firstFacetIndex == -1)
		{
			return;
		}
		_isMeshDirty = false;
		Vector3[] vertices = _renderLayer.vertices;
		Vector2[] uvs = _renderLayer.uvs;
		Color[] colors = _renderLayer.colors;
		List<Vector2[]> vertexPolygons = _mesh2D.polygonalData.vertexPolygons;
		List<int[]> trianglePolygons = _mesh2D.polygonalData.trianglePolygons;
		int count = trianglePolygons.Count;
		int num = _firstFacetIndex;
		for (int i = 0; i < count; i++)
		{
			Vector2[] array = vertexPolygons[i];
			int[] array2 = trianglePolygons[i];
			int num2 = array2.Length / 3;
			Color color = RXColor.ColorFromHSL(0.8f + RXRandom.Float(i) * 0.3f, 1f, 0.5f);
			for (int j = 0; j < num2; j++)
			{
				int num3 = num * 3;
				int num4 = num3 + 1;
				int num5 = num3 + 2;
				int num6 = j * 3;
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num3], array[array2[num6]], 0f);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num4], array[array2[num6 + 1]], 0f);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num5], array[array2[num6 + 2]], 0f);
				uvs[num3] = _uvBottomLeft;
				uvs[num4] = _uvTopLeft;
				uvs[num5] = _uvBottomRight;
				colors[num3] = color;
				colors[num4] = color;
				colors[num5] = color;
				num++;
			}
		}
		_renderLayer.HandleVertsChange();
	}
}
