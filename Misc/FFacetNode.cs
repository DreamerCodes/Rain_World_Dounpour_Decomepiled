public class FFacetNode : FNode
{
	protected FAtlas _atlas;

	protected FShader _shader;

	protected int _firstFacetIndex = -1;

	protected int _numberOfFacetsNeeded;

	protected FFacetRenderLayer _renderLayer;

	protected FFacetType _facetType;

	private bool _hasInited;

	public virtual int firstFacetIndex => _firstFacetIndex;

	public FShader shader
	{
		get
		{
			return _shader;
		}
		set
		{
			if (_shader != value)
			{
				_shader = value;
				if (_isOnStage)
				{
					_stage.HandleFacetsChanged();
				}
			}
		}
	}

	public FFacetType facetType => _facetType;

	protected virtual void Init(FFacetType facetType, FAtlas atlas, int numberOfFacetsNeeded)
	{
		_facetType = facetType;
		_atlas = atlas;
		if (_shader == null)
		{
			_shader = FShader.defaultShader;
		}
		_numberOfFacetsNeeded = numberOfFacetsNeeded;
		_hasInited = true;
	}

	protected void UpdateFacets()
	{
		if (_hasInited)
		{
			_stage.renderer.GetFacetRenderLayer(out _renderLayer, out _firstFacetIndex, _facetType, _atlas, _shader, _numberOfFacetsNeeded);
		}
	}

	public virtual void PopulateRenderLayer()
	{
	}

	public override void HandleAddedToStage()
	{
		if (!_isOnStage)
		{
			base.HandleAddedToStage();
			_stage.HandleFacetsChanged();
		}
	}

	public override void HandleRemovedFromStage()
	{
		if (_isOnStage)
		{
			base.HandleRemovedFromStage();
			_stage.HandleFacetsChanged();
		}
	}
}
