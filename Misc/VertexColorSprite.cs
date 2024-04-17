using UnityEngine;

public class VertexColorSprite : FSprite
{
	public Color[] verticeColors;

	public VertexColorSprite(string elementName)
		: base(elementName)
	{
		verticeColors = new Color[4];
	}

	public override void PopulateRenderLayer()
	{
		if (_isOnStage && _firstFacetIndex != -1)
		{
			_isMeshDirty = false;
			int num = _firstFacetIndex * 4;
			int num2 = num + 1;
			int num3 = num + 2;
			int num4 = num + 3;
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num], _localVertices[0], _meshZ);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num2], _localVertices[1], _meshZ);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num3], _localVertices[2], _meshZ);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num4], _localVertices[3], _meshZ);
			uvs[num] = _element.uvTopLeft;
			uvs[num2] = _element.uvTopRight;
			uvs[num3] = _element.uvBottomRight;
			uvs[num4] = _element.uvBottomLeft;
			colors[num] = verticeColors[0];
			colors[num2] = verticeColors[1];
			colors[num3] = verticeColors[2];
			colors[num4] = verticeColors[3];
			_renderLayer.HandleVertsChange();
		}
	}
}
