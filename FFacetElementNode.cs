public class FFacetElementNode : FFacetNode
{
	protected FAtlasElement _element;

	public FAtlasElement element
	{
		get
		{
			return _element;
		}
		set
		{
			if (_element == value)
			{
				return;
			}
			bool num = _element.atlas != value.atlas;
			_element = value;
			if (num)
			{
				_atlas = _element.atlas;
				if (_isOnStage)
				{
					_stage.HandleFacetsChanged();
				}
			}
			HandleElementChanged();
		}
	}

	protected void Init(FFacetType facetType, FAtlasElement element, int numberOfFacetsNeeded)
	{
		_element = element;
		base.Init(facetType, _element.atlas, numberOfFacetsNeeded);
		HandleElementChanged();
	}

	public void SetElementByName(string elementName)
	{
		element = Futile.atlasManager.GetElementWithName(elementName);
	}

	public virtual void HandleElementChanged()
	{
	}
}
