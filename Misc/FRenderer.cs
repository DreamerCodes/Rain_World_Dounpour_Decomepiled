using System.Collections.Generic;

public class FRenderer
{
	private List<FFacetRenderLayer> _allLayers = new List<FFacetRenderLayer>();

	private List<FFacetRenderLayer> _liveLayers = new List<FFacetRenderLayer>();

	private List<FFacetRenderLayer> _previousLiveLayers = new List<FFacetRenderLayer>();

	private List<FFacetRenderLayer> _cachedLayers = new List<FFacetRenderLayer>();

	private List<FRenderableLayerInterface> _allRenderables = new List<FRenderableLayerInterface>();

	private FFacetRenderLayer _topLayer;

	private FStage _stage;

	public FRenderer(FStage stage)
	{
		_stage = stage;
	}

	public void Clear()
	{
		int count = _allLayers.Count;
		for (int i = 0; i < count; i++)
		{
			_allLayers[i].Destroy();
		}
		_allLayers.Clear();
		_liveLayers.Clear();
		_previousLiveLayers.Clear();
		_cachedLayers.Clear();
		_allRenderables.Clear();
	}

	public void ClearLayersThatUseAtlas(FAtlas atlas)
	{
		bool flag = false;
		for (int num = _allLayers.Count - 1; num >= 0; num--)
		{
			if (_allLayers[num].atlas == atlas)
			{
				flag = true;
				FFacetRenderLayer fFacetRenderLayer = _allLayers[num];
				_liveLayers.Remove(fFacetRenderLayer);
				_previousLiveLayers.Remove(fFacetRenderLayer);
				_cachedLayers.Remove(fFacetRenderLayer);
				_allRenderables.Remove(fFacetRenderLayer);
				_allLayers.Remove(fFacetRenderLayer);
				fFacetRenderLayer.Destroy();
			}
		}
		if (flag)
		{
			_stage.HandleFacetsChanged();
		}
	}

	public void UpdateLayerTransforms()
	{
		int count = _allLayers.Count;
		for (int i = 0; i < count; i++)
		{
			_allLayers[i].UpdateTransform();
		}
	}

	public void StartRender()
	{
		List<FFacetRenderLayer> liveLayers = _liveLayers;
		_liveLayers = _previousLiveLayers;
		_previousLiveLayers = liveLayers;
		_topLayer = null;
		_allRenderables.Clear();
	}

	public void EndRender()
	{
		int count = _previousLiveLayers.Count;
		for (int i = 0; i < count; i++)
		{
			_previousLiveLayers[i].RemoveFromWorld();
			_cachedLayers.Add(_previousLiveLayers[i]);
		}
		_previousLiveLayers.Clear();
		if (_topLayer != null)
		{
			_topLayer.Close();
		}
	}

	protected FFacetRenderLayer CreateFacetRenderLayer(FFacetType facetType, int batchIndex, FAtlas atlas, FShader shader)
	{
		int count = _previousLiveLayers.Count;
		for (int i = 0; i < count; i++)
		{
			FFacetRenderLayer fFacetRenderLayer = _previousLiveLayers[i];
			if (fFacetRenderLayer.batchIndex == batchIndex)
			{
				_previousLiveLayers.RemoveAt(i);
				_liveLayers.Add(fFacetRenderLayer);
				_allRenderables.Add(fFacetRenderLayer);
				return fFacetRenderLayer;
			}
		}
		int count2 = _cachedLayers.Count;
		for (int j = 0; j < count2; j++)
		{
			FFacetRenderLayer fFacetRenderLayer2 = _cachedLayers[j];
			if (fFacetRenderLayer2.batchIndex == batchIndex)
			{
				_cachedLayers.RemoveAt(j);
				fFacetRenderLayer2.AddToWorld();
				_liveLayers.Add(fFacetRenderLayer2);
				_allRenderables.Add(fFacetRenderLayer2);
				return fFacetRenderLayer2;
			}
		}
		FFacetRenderLayer fFacetRenderLayer3 = facetType.createRenderLayer(_stage, facetType, atlas, shader);
		_liveLayers.Add(fFacetRenderLayer3);
		_allLayers.Add(fFacetRenderLayer3);
		_allRenderables.Add(fFacetRenderLayer3);
		fFacetRenderLayer3.AddToWorld();
		return fFacetRenderLayer3;
	}

	public void GetFacetRenderLayer(out FFacetRenderLayer renderLayer, out int firstFacetIndex, FFacetType facetType, FAtlas atlas, FShader shader, int numberOfFacetsNeeded)
	{
		int num = facetType.index * 10000000 + atlas.index * 10000 + shader.index;
		if (_topLayer == null)
		{
			_topLayer = CreateFacetRenderLayer(facetType, num, atlas, shader);
			_topLayer.Open();
		}
		else if (_topLayer.batchIndex != num)
		{
			_topLayer.Close();
			_topLayer = CreateFacetRenderLayer(facetType, num, atlas, shader);
			_topLayer.Open();
		}
		renderLayer = _topLayer;
		firstFacetIndex = _topLayer.GetNextFacetIndex(numberOfFacetsNeeded);
	}

	public void AddRenderableLayer(FRenderableLayerInterface renderableLayer)
	{
		_allRenderables.Add(renderableLayer);
		if (_topLayer != null)
		{
			_topLayer.Close();
			_topLayer = null;
		}
	}

	public void Update()
	{
		int count = _allRenderables.Count;
		for (int i = 0; i < count; i++)
		{
			_allRenderables[i].Update(Futile.nextRenderLayerDepth++);
		}
	}
}
