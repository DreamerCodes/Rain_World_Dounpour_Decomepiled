using System;

public class FTriangleRenderLayer : FFacetRenderLayer
{
	public FTriangleRenderLayer(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader)
		: base(stage, facetType, atlas, shader)
	{
	}

	protected override void FillUnusedFacetsWithZeroes()
	{
		_lowestZeroIndex = Math.Max(_nextAvailableFacetIndex, Math.Min(_maxFacetCount, _lowestZeroIndex));
		for (int i = _nextAvailableFacetIndex; i < _lowestZeroIndex; i++)
		{
			int num = i * 3;
			_vertices[num].Set(50f, 0f, 1000000f);
			_vertices[num + 1].Set(50f, 0f, 1000000f);
			_vertices[num + 2].Set(50f, 0f, 1000000f);
		}
		_lowestZeroIndex = _nextAvailableFacetIndex;
	}

	protected override void ShrinkMaxFacetLimit(int deltaDecrease)
	{
		if (deltaDecrease > 0)
		{
			_maxFacetCount = Math.Max(_facetType.initialAmount, _maxFacetCount - deltaDecrease);
			Array.Resize(ref _vertices, _maxFacetCount * 3);
			Array.Resize(ref _uvs, _maxFacetCount * 3);
			Array.Resize(ref _colors, _maxFacetCount * 3);
			Array.Resize(ref _triangles, _maxFacetCount * 3);
			_didVertCountChange = true;
			_didVertsChange = true;
			_didUVsChange = true;
			_didColorsChange = true;
			_isMeshDirty = true;
			_doesMeshNeedClear = true;
		}
	}

	protected override void ExpandMaxFacetLimit(int deltaIncrease)
	{
		if (deltaIncrease > 0)
		{
			int maxFacetCount = _maxFacetCount;
			_maxFacetCount += deltaIncrease;
			Array.Resize(ref _vertices, _maxFacetCount * 3);
			Array.Resize(ref _uvs, _maxFacetCount * 3);
			Array.Resize(ref _colors, _maxFacetCount * 3);
			Array.Resize(ref _triangles, _maxFacetCount * 3);
			for (int i = maxFacetCount; i < _maxFacetCount; i++)
			{
				int num = i * 3;
				_triangles[num] = num;
				_triangles[num + 1] = num + 1;
				_triangles[num + 2] = num + 2;
			}
			_didVertCountChange = true;
			_didVertsChange = true;
			_didUVsChange = true;
			_didColorsChange = true;
			_isMeshDirty = true;
		}
	}
}
