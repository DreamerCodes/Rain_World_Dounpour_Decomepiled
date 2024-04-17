using System.Collections.Generic;
using UnityEngine;

public class FutileParams
{
	public List<FResolutionLevel> resLevels = new List<FResolutionLevel>();

	public Vector2 origin = new Vector2(0.5f, 0.5f);

	public int targetFrameRate = 60;

	public ScreenOrientation singleOrientation = ScreenOrientation.LandscapeLeft;

	public bool supportsLandscapeLeft;

	public bool supportsLandscapeRight;

	public bool supportsPortrait;

	public bool supportsPortraitUpsideDown;

	public Color backgroundColor = Color.black;

	public bool shouldLerpToNearestResolutionLevel = true;

	public FutileParams(bool supportsLandscapeLeft, bool supportsLandscapeRight, bool supportsPortrait, bool supportsPortraitUpsideDown)
	{
		this.supportsLandscapeLeft = supportsLandscapeLeft;
		this.supportsLandscapeRight = supportsLandscapeRight;
		this.supportsPortrait = supportsPortrait;
		this.supportsPortraitUpsideDown = supportsPortraitUpsideDown;
	}

	public FResolutionLevel AddResolutionLevel(float maxLength, float displayScale, float resourceScale, string resourceSuffix)
	{
		FResolutionLevel fResolutionLevel = new FResolutionLevel();
		fResolutionLevel.maxLength = maxLength;
		fResolutionLevel.displayScale = displayScale;
		fResolutionLevel.resourceScale = resourceScale;
		fResolutionLevel.resourceSuffix = resourceSuffix;
		bool flag = false;
		for (int i = 0; i < resLevels.Count; i++)
		{
			if (fResolutionLevel.maxLength < resLevels[i].maxLength)
			{
				resLevels.Insert(i, fResolutionLevel);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			resLevels.Add(fResolutionLevel);
		}
		return fResolutionLevel;
	}
}
