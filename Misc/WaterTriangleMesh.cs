using UnityEngine;

public class WaterTriangleMesh : FSprite
{
	public TriangleMesh.Triangle[] triangles;

	public Vector2[] vertices;

	public Color[] verticeColors;

	public bool customColor;

	public WaterTriangleMesh(string imageName, TriangleMesh.Triangle[] tris, bool customColor)
	{
		this.customColor = customColor;
		triangles = tris;
		int num = 2;
		for (int i = 0; i < triangles.Length; i++)
		{
			if (triangles[i].a > num)
			{
				num = triangles[i].a;
			}
			if (triangles[i].b > num)
			{
				num = triangles[i].b;
			}
			if (triangles[i].c > num)
			{
				num = triangles[i].c;
			}
		}
		vertices = new Vector2[num + 1];
		for (int j = 0; j < num; j++)
		{
			vertices[j] = new Vector2(0f, 0f);
		}
		if (customColor)
		{
			verticeColors = new Color[num + 1];
			for (int k = 0; k < verticeColors.Length; k++)
			{
				verticeColors[k] = _alphaColor;
			}
		}
		Init(FFacetType.Triangle, Futile.atlasManager.GetElementWithName(imageName), triangles.Length);
		_isAlphaDirty = true;
		UpdateLocalVertices();
	}

	public void MoveVertice(int ind, Vector2 pos)
	{
		vertices[ind] = pos;
		Refresh();
	}

	public void Refresh()
	{
		_isMeshDirty = true;
	}

	public override void PopulateRenderLayer()
	{
		if (!_isOnStage || _firstFacetIndex == -1)
		{
			return;
		}
		_isMeshDirty = false;
		int num = _firstFacetIndex * 3;
		Vector3[] array = _renderLayer.vertices;
		Vector2[] uvs = _renderLayer.uvs;
		Vector2[] array2 = new Vector2[6];
		Color[] colors = _renderLayer.colors;
		float num2 = _element.uvTopRight.x - _element.uvTopLeft.x;
		array2[3] = new Vector2(_element.uvTopLeft.x, _element.uvTopLeft.y);
		array2[4] = new Vector2(_element.uvBottomLeft.x, _element.uvBottomLeft.y);
		array2[5] = new Vector2(_element.uvTopLeft.x + num2, _element.uvTopLeft.y);
		array2[0] = new Vector2(_element.uvBottomLeft.x, _element.uvBottomLeft.y);
		array2[1] = new Vector2(_element.uvTopLeft.x, _element.uvTopLeft.y);
		array2[2] = new Vector2(_element.uvBottomLeft.x + num2, _element.uvBottomLeft.y);
		if (customColor)
		{
			for (int i = 0; i < triangles.Length; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					colors[num + i * 3 + j] = verticeColors[triangles[i].GetAt(j)];
				}
			}
		}
		else
		{
			for (int k = 0; k < triangles.Length * 3; k++)
			{
				colors[num + k] = _alphaColor;
			}
		}
		int num3 = 0;
		for (int l = 0; l < triangles.Length; l++)
		{
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref array[num + num3], vertices[triangles[l].a], 0f);
			uvs[num + num3] = array2[l % 2 * 3];
			num3++;
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref array[num + num3], vertices[triangles[l].b], 0f);
			uvs[num + num3] = array2[1 + l % 2 * 3];
			num3++;
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref array[num + num3], vertices[triangles[l].c], 0f);
			uvs[num + num3] = array2[2 + l % 2 * 3];
			num3++;
		}
		_renderLayer.HandleVertsChange();
	}
}
