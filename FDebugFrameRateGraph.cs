using UnityEngine;

public class FDebugFrameRateGraph : FContainer
{
	private const int BASE_TEX_WIDTH = 128;

	private const int BASE_TEX_HEIGHT = 32;

	private const float IDEAL_FRAME_TIME = 1f / 60f;

	private const float MAX_FRAME_TIME = 0.050000004f;

	private bool _useSmoothDeltaTime;

	private int _texWidth;

	private int _texHeight;

	private int _numPixels;

	private int _targetRow;

	private int _doubleTargetRow;

	private Color[] _newFramePixels;

	private Texture2D _graphTex;

	private FSprite _graphSprite;

	private Color _blank = new Color(0f, 0f, 0f, 0.5f);

	public FDebugFrameRateGraph()
	{
		_texWidth = 128;
		_texHeight = 32;
		_numPixels = _texWidth * _texHeight;
		_newFramePixels = new Color[_texHeight];
		_graphTex = new Texture2D(_texWidth, _texHeight, TextureFormat.ARGB32, mipChain: false);
		_graphTex.filterMode = FilterMode.Point;
		Color[] array = new Color[_numPixels];
		for (int i = 0; i < _numPixels; i++)
		{
			array[i] = _blank;
		}
		_graphTex.SetPixels(array);
		_targetRow = Mathf.FloorToInt(1f / 60f / (0.050000004f / (float)_texHeight));
		_doubleTargetRow = Mathf.FloorToInt(1f / 30f / (0.050000004f / (float)_texHeight));
		Color[] array2 = new Color[_texWidth];
		Color[] array3 = new Color[_texWidth];
		Color[] array4 = new Color[_texWidth];
		for (int j = 0; j < _texWidth; j++)
		{
			array2[j] = Color.black;
			array3[j] = Color.black;
			array4[j] = Color.black;
		}
		_graphTex.SetPixels(0, _targetRow, _texWidth, 1, array2);
		_graphTex.SetPixels(0, _doubleTargetRow, _texWidth, 1, array3);
		_graphTex.SetPixels(0, _texHeight - 1, _texWidth, 1, array4);
		_graphTex.Apply();
		Futile.atlasManager.LoadAtlasFromTexture("debugFrameGraph", _graphTex, textureFromAsset: false);
		_graphSprite = new FSprite("debugFrameGraph");
		_graphSprite.SetAnchor(0f, 0f);
		_graphSprite.scale = Futile.resourceScale;
		AddChild(_graphSprite);
		ListenForUpdate(HandleUpdate);
	}

	private void HandleUpdate()
	{
		float num = Time.deltaTime;
		if (_useSmoothDeltaTime)
		{
			num = Time.smoothDeltaTime;
		}
		Color[] pixels = _graphTex.GetPixels(1, 0, _texWidth - 1, _texHeight);
		_graphTex.SetPixels(0, 0, _texWidth - 1, _texHeight, pixels);
		int num2 = Mathf.FloorToInt(num / (0.050000004f / (float)_texHeight));
		Color color = Color.red;
		if (num2 <= _targetRow)
		{
			color = Color.green;
		}
		else if (num2 <= _doubleTargetRow)
		{
			color = Color.yellow;
		}
		for (int i = 0; i <= num2 && i < _texHeight; i++)
		{
			_newFramePixels[i] = color;
		}
		for (int j = num2 + 1; j < _texHeight; j++)
		{
			_newFramePixels[j] = _blank;
		}
		_newFramePixels[_targetRow] = Color.black;
		_newFramePixels[_doubleTargetRow] = Color.black;
		_newFramePixels[_texHeight - 1] = Color.black;
		_graphTex.SetPixels(_texWidth - 1, 0, 1, _texHeight, _newFramePixels);
		_graphTex.Apply();
	}
}
