using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace CoralBrain;

public struct JobData
{
	[ReadOnly]
	public NativeArray<int2> naFourDirections;

	public NativeArray<float2> nativePoints;

	public NativeArray<Vector2> outPoints;
}
