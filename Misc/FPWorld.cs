using UnityEngine;

public class FPWorld : MonoBehaviour
{
	public static FPWorld instance;

	private FPWorld()
	{
	}

	public static FPWorld Create(float metersToPointsRatio)
	{
		instance = new GameObject("FPWorld Root").AddComponent<FPWorld>();
		instance.Init(metersToPointsRatio);
		return instance;
	}

	protected void Init(float metersToPointsRatio)
	{
		FPhysics.METERS_TO_POINTS = metersToPointsRatio;
		FPhysics.POINTS_TO_METERS = 1f / metersToPointsRatio;
	}
}
