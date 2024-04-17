using System;

public class FQuadRenderLayer : FFacetRenderLayer
{
	public FQuadRenderLayer(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader)
		: base(stage, facetType, atlas, shader)
	{
	}

	protected override void FillUnusedFacetsWithZeroes()
	{
		_lowestZeroIndex = Math.Max(_nextAvailableFacetIndex, Math.Min(_maxFacetCount, _lowestZeroIndex));
		for (int i = _nextAvailableFacetIndex; i < _lowestZeroIndex; i++)
		{
			int num = i * 4;
			_vertices[num].Set(50f, 0f, 1000000f);
			_vertices[num + 1].Set(50f, 0f, 1000000f);
			_vertices[num + 2].Set(50f, 0f, 1000000f);
			_vertices[num + 3].Set(50f, 0f, 1000000f);
		}
		_lowestZeroIndex = _nextAvailableFacetIndex;
	}

	protected override void ShrinkMaxFacetLimit(int deltaDecrease)
	{
		if (deltaDecrease > 0)
		{
			_maxFacetCount = Math.Max(_facetType.initialAmount, _maxFacetCount - deltaDecrease);
			Array.Resize(ref _vertices, _maxFacetCount * 4);
			Array.Resize(ref _uvs, _maxFacetCount * 4);
			Array.Resize(ref _colors, _maxFacetCount * 4);
			Array.Resize(ref _triangles, _maxFacetCount * 6);
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
			Array.Resize(ref _vertices, _maxFacetCount * 4);
			Array.Resize(ref _uvs, _maxFacetCount * 4);
			Array.Resize(ref _colors, _maxFacetCount * 4);
			Array.Resize(ref _triangles, _maxFacetCount * 6);
			for (int i = maxFacetCount; i < _maxFacetCount; i++)
			{
				_triangles[i * 6] = i * 4;
				_triangles[i * 6 + 1] = i * 4 + 1;
				_triangles[i * 6 + 2] = i * 4 + 2;
				_triangles[i * 6 + 3] = i * 4;
				_triangles[i * 6 + 4] = i * 4 + 2;
				_triangles[i * 6 + 5] = i * 4 + 3;
			}
			_didVertCountChange = true;
			_didVertsChange = true;
			_didUVsChange = true;
			_didColorsChange = true;
			_isMeshDirty = true;
		}
	}
}
