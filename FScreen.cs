using UnityEngine;

public class FScreen
{
	public delegate void ScreenOrientationChangeDelegate();

	public delegate void ScreenResizeDelegate(bool wasResizedDueToOrientationChange);

	private float _originX;

	private float _originY;

	private FutileParams _futileParams;

	public int pixelWidth { get; private set; }

	public int pixelHeight { get; private set; }

	public RenderTexture renderTexture { get; private set; }

	public int renderScale { get; private set; }

	public float originX
	{
		get
		{
			return _originX;
		}
		set
		{
			if (_originX != value)
			{
				_originX = value;
				Futile.instance.UpdateCameraPosition();
			}
		}
	}

	public float originY
	{
		get
		{
			return _originY;
		}
		set
		{
			if (_originY != value)
			{
				_originY = value;
				Futile.instance.UpdateCameraPosition();
			}
		}
	}

	public event ScreenOrientationChangeDelegate SignalOrientationChange;

	public event ScreenResizeDelegate SignalResize;

	public FScreen(FutileParams futileParams)
	{
		_futileParams = futileParams;
		Futile.displayScale = 1f;
		Futile.displayScaleInverse = 1f / Futile.displayScale;
		Futile.resourceScale = 1f;
		Futile.resourceScaleInverse = 1f / Futile.resourceScale;
		pixelWidth = (int)futileParams.resLevels[0].maxLength;
		pixelHeight = 768;
		renderScale = 1;
		UpdateScreenOffset();
		_originX = _futileParams.origin.x;
		_originY = _futileParams.origin.y;
		renderTexture = new RenderTexture(pixelWidth * renderScale, pixelHeight * renderScale, 0);
	}

	public void ReinitRenderTexture(int displayWidth)
	{
		_futileParams.resLevels[0].maxLength = displayWidth;
		pixelWidth = displayWidth;
		UpdateScreenOffset();
		renderTexture.Release();
		renderTexture.DiscardContents();
		renderTexture = new RenderTexture(pixelWidth * renderScale, pixelHeight * renderScale, 0);
		if (Display.main.systemWidth < displayWidth || Display.main.systemHeight < 768)
		{
			renderTexture.filterMode = FilterMode.Bilinear;
		}
		else
		{
			renderTexture.filterMode = FilterMode.Point;
		}
	}

	private void UpdateScreenOffset()
	{
		if (Futile.isOpenGL)
		{
			Futile.screenPixelOffset = Vector2.zero;
			Shader.SetGlobalVector(RainWorld.ShadPropScreenOffset, new Vector2(0f, 0f));
		}
		else if (renderScale == 1)
		{
			Futile.screenPixelOffset = new Vector2(0.5f * Futile.displayScaleInverse, 0.5f * Futile.displayScaleInverse);
			Shader.SetGlobalVector(RainWorld.ShadPropScreenOffset, new Vector2(0f, 0f));
		}
		else
		{
			Futile.screenPixelOffset = new Vector2(0f * Futile.displayScaleInverse, 1f * Futile.displayScaleInverse);
			Shader.SetGlobalVector(RainWorld.ShadPropScreenOffset, new Vector2(0.5f / (float)pixelWidth, 0.5f / (float)pixelHeight));
		}
	}

	public void Update()
	{
	}
}
