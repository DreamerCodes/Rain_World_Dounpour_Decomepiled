using System.Collections.Generic;

public class FFacetType
{
	public delegate FFacetRenderLayer CreateRenderLayerDelegate(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader);

	public static FFacetType defaultFacetType;

	public static FFacetType Quad;

	public static FFacetType Triangle;

	private static int _nextFacetTypeIndex = 0;

	private static List<FFacetType> _facetTypes = new List<FFacetType>();

	public int index;

	public string name;

	public int initialAmount;

	public int expansionAmount;

	public int maxEmptyAmount;

	public CreateRenderLayerDelegate createRenderLayer;

	private FFacetType(string name, int index, int initialAmount, int expansionAmount, int maxEmptyAmount, CreateRenderLayerDelegate createRenderLayer)
	{
		this.index = index;
		this.name = name;
		this.initialAmount = initialAmount;
		this.expansionAmount = expansionAmount;
		this.maxEmptyAmount = maxEmptyAmount;
		this.createRenderLayer = createRenderLayer;
	}

	public static void Init()
	{
		Quad = CreateFacetType("Quad", 10, 10, 60, CreateQuadLayer);
		Triangle = CreateFacetType("Triangle", 16, 16, 64, CreateTriLayer);
		defaultFacetType = Quad;
	}

	public static FFacetType CreateFacetType(string facetTypeShortName, int initialAmount, int expansionAmount, int maxEmptyAmount, CreateRenderLayerDelegate createRenderLayer)
	{
		for (int i = 0; i < _facetTypes.Count; i++)
		{
			if (_facetTypes[i].name == facetTypeShortName)
			{
				return _facetTypes[i];
			}
		}
		FFacetType fFacetType = new FFacetType(facetTypeShortName, _nextFacetTypeIndex++, initialAmount, expansionAmount, maxEmptyAmount, createRenderLayer);
		_facetTypes.Add(fFacetType);
		return fFacetType;
	}

	private static FFacetRenderLayer CreateQuadLayer(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader)
	{
		return new FQuadRenderLayer(stage, facetType, atlas, shader);
	}

	private static FFacetRenderLayer CreateTriLayer(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader)
	{
		return new FTriangleRenderLayer(stage, facetType, atlas, shader);
	}
}
